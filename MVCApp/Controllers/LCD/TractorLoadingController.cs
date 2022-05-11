using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MVCApp.Controllers
{
    public class TractorLoadingController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        // GET: Tractor
        public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
            return View();
        }



        [HttpGet]
        public JsonResult getPlanVertical(string PLANTCODE)
        {
            var result = new Tractor();
            var SentData = new List<Tractortable>();
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", DisplayMethod = "";

                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                plant = PLANTCODE;
                if (plant == "4")
                {
                    plant = "T04";
                    family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);

                }
                if (PLANTCODE == "5")
                {
                    plant = "T05";
                    family = Convert.ToString(ConfigurationManager.AppSettings["PT_FAMILY"]);
                }

                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = ""; string ShiftTlt = "";
                DateTime ShiftStart, shiftEnd; string Enginefamily = "ENGINE FTD";
                data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    result.lblError = "";
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    NightExists = data.Split('#')[1].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    ShiftStart = Convert.ToDateTime(data.Split('#')[3].Trim());
                    shiftEnd = Convert.ToDateTime(data.Split('#')[4].Trim());
                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                        Plandate = fun.ServerDate.Date.AddDays(-1);
                    else
                        Plandate = fun.ServerDate.Date;
                    var retList = new List<string>();
                    retList = getLastshifts(Shiftcode, ShiftStart, shiftEnd);
                    query = "select to_char(EXPECTED_qty) from XXES_DAILY_PLAN_MASTER  where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and XXES_DAILY_PLAN_MASTER.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family + "'";
                    expqty = fun.get_Col_Value(query).Trim();

                    query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS s WHERE
                                 S.ENTRYDATE>=to_date('{0}','dd-Mon-yyyy hh24:mi:ss')
                                AND S.ENTRYDATE<=to_date('{1}','dd-Mon-yyyy hh24:mi:ss')
                                 and plant_code='{2}' and family_code='{3}'", ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"), plant, family);

                    ShiftTlt = fun.get_Col_Value(query).Trim();
                    result.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    result.lblTime = "TIME:" + fun.ServerDate.ToString("HH:mm:ss");
                    //lblTime.Text = "Time: " + pbf.ServerDate.ToString("HH:mm:ss");
                    result.lblShift = "SHIFT: " + Shiftcode;
                    result.lblProductionUnit = "PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    query = "";

                    DataTable dt = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query += String.Format(@"SELECT ROWNUM SERIAL_NO,
                                        J.JOBID,
                                        M.ITEM_CODE FCODE,
                                        M.SHORT_CODE,
                                        M.ENGINE,
                                        (SELECT SHORT_CODE FROM   XXES_SUB_ASSEMBLY_MASTER t WHERE  t.FAMILY_CODE= '{2}'  AND t.PLANT_CODE = M.PLANT_CODE AND t.main_sub_assembly=m.engine) AS ENGINE_DESC
                                        FROM   xxes_item_master m JOIN XXES_JOB_STATUS j 
                                        ON M.ITEM_CODE=j.ITEM_CODE 
                                        AND m.PLANT_CODE='{0}' 
                                        AND m.FAMILY_CODE='{1}'
                                        JOIN XXES_SCANED_JOBS s ON j.JOBID=s.JOBID
                                        ORDER BY s.AUTOID", plant.Trim().ToUpper(), family.Trim().ToUpper(), Enginefamily);
                        dt = returnDataTable(query);

                    }
                    else
                    {
                        try
                        {
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                                fun.ConOpen();
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "TL";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.Trim().ToUpper();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                OracleDataAdapter dr = new OracleDataAdapter(sc);
                                dr.Fill(dt);
                                //fun.ConClose();
                            }
                        }
                        catch (Exception ex)
                        {
                            fun.LogWrite(ex);
                            throw;
                        }
                        finally
                        {
                            fun.ConClose();
                        }
                        //return ;

                    }
                    try
                    {                        
                        List<Tractor> TractorD = new List<Tractor>();
                        if (dt.Rows.Count > 0)
                        {
                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var TractorData = new Tractortable();
                                TractorData.SRNO = Convert.ToString(srno);
                                TractorData.JOBID = dr["JOBID"].ToString();
                                TractorData.FCODE = dr["FCODE"].ToString();
                                TractorData.SHORT_CODE = dr["SHORT_CODE"].ToString();
                                TractorData.ENGINE = dr["ENGINE"].ToString();
                                TractorData.ENGINE_DESCRIPTION = dr["ENGINE_DESC"].ToString();


                                SentData.Add(TractorData);
                                srno++;
                            }

                            //result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            //result.lblBK = "BUCKLE-UP: " + Convert.ToString(dt.Compute("Sum(BUCKLEUP)", ""));
                            //result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            //expqty = get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            //result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                            result.lblSftotDay = "SHIFT TOTAL: " + (ShiftTlt.Trim() == "" ? "0" : ShiftTlt.Trim());


                        }
                        else
                        {
                            result.lblBKTotDayVisible = false;
                            TractorD = null;
                            result.lblPendingVisible = false;
                            result.lblBKVisible = false;
                            result.lblPlanQtyVisible = false;
                        }
                        result.lblErrorVisible = false;
                    }
                    catch (Exception ex)
                    {

                        result.lblError = ex.Message.ToString();
                        result.lblErrorVisible = true;

                    }
                    finally { }

                }
                getMsg();
            }
            catch (Exception ex)
            {
                //result.Visible = true;

                result.lblError = ex.Message.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            finally { }
            result.SentData = SentData;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public DataTable returnDataTable(string SqlQuery)
        {
            OracleConnection ConOrcl = new OracleConnection(Function.orCnstr);
            DataTable dt = new DataTable();
            try
            {
                OracleDataAdapter Oda = new OracleDataAdapter(SqlQuery, ConOrcl);
                Oda.Fill(dt);
                return dt;
            }
            catch
            {
                throw;
            }
            finally { ConOrcl.Dispose(); }
        }

        private List<string> getLastshifts(string Shiftcode, DateTime curShiftstart, DateTime curShiftend)
        {
            var retList = new List<string>();
            try
            {
                if (Shiftcode.Trim().ToUpper() == "C")
                {
                    retList.Add("A");
                    retList.Add("B");
                }
                else if (Shiftcode.Trim().ToUpper() == "B")
                {
                    retList.Add("A");
                }
                return retList;
            }
            catch
            {
                return retList;
            }
        }

        private void getMsg()
        {
            var result = new Tractor();
            try
            {
                query = "select PARAMVALUE from XXES_SFT_SETTINGS where PARAMETERINFO='LCD_MSG' and STATUS='Y'";
                string msg = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(msg))
                {
                    result.relblMsg = msg;
                    result.lblMsgVisible = true;
                }
                else
                {
                    //result.lblMsg = "WELCOME";
                    //Random rnd = new Random();
                    //result.lblMsg.ForeColor = Color.FromArgb((rnd.Next(0, 255)), (rnd.Next(0, 255)), (rnd.Next(0, 255)));
                    //result.lblMsgVisible = true;
                }
            }
            catch { throw; }
            finally { }
        }


        public JsonResult PendingTaskComplete(Tractor data)
        {
            var result = "";

            try
            {
                string value = Convert.ToString("AUTOID");
                string plan = Convert.ToString("PLAN");
                string buckleup = Convert.ToString("BUCKLEUP");
                string pending = Convert.ToString("PENDING");
                string plant = "", family = "";
                plant = Convert.ToString(ConfigurationManager.AppSettings["FT_PLANT"]);
                family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);
                if (string.IsNullOrEmpty(pending)) pending = "0";
                query = "select * from (select FCODE_ID from XXES_JOB_STATUS where FCODE_ID is not null and plant_code='" + plant + "' and family_code='" + family + "' order by ENTRYDATE DESC) where rownum=1";
                if (fun.get_Col_Value(query).Trim() == value.Trim())
                {
                    string BackColor = "Yellow";
                }
                else if (buckleup.Trim() == plan.Trim() && pending == "0")
                {
                    string BackColor = "LightGreen";
                }

            }
            catch { throw; }
            finally { }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public class Tractortable
        {
            public string SRNO { get; set; }
            public string JOBID { get; set; }
            public string FCODE { get; set; }
            public string ENGINE { get; set; }
            public string ENGINE_DESCRIPTION { get; set; }
            public string PLAN_ID { get; set; }
            public string AUTOID { get; set; }
            public string MODEL { get; set; }
            public string SHORT_CODE { get; set; }
            public string SHIFT { get; set; }
            public string SHORTCODE { get; set; }
            public string BACKEND { get; set; }

            public string PLAN { get; set; }
            public string BUCKLEUP { get; set; }
            public string PENDING { get; set; }
            public string TRANS { get; set; }
            public string SKID { get; set; }
            public string HYD { get; set; }
            public string AXEL { get; set; }
            public string BackColor { get; set; }

        }

        public class Tractor
        {
            public string SRNO { get; set; }
            public string PLAN_ID { get; set; }
            public string AUTOID { get; set; }
            public string MODEL { get; set; }
            public string DESCRIPTION { get; set; }
            public string SHIFT { get; set; }
            public string SHORTCODE { get; set; }
            public string BACKEND { get; set; }
            public string ENGINE { get; set; }
            public string PLAN { get; set; }
            public string BUCKLEUP { get; set; }
            public string PENDING { get; set; }
            public string lblError { get; set; }
            public string lblDcode { get; set; }
            public string lblInj1Srlno { get; set; }
            public string relblMsg { get; set; }
            public string lblMsg { get; set; }
            public bool lblBKTotDayVisible { get; set; }
            public bool lblPendingVisible { get; set; }
            public bool tdMsgVisible { get; set; }
            public bool lblBKVisible { get; set; }
            public bool lblPlanQtyVisible { get; set; }
            public bool lblErrorVisible { get; set; }
            public bool lblMsgVisible { get; set; }
            public string lblShift { get; set; }
            public string lblProductionUnit { get; set; }
            public string lblBK { get; set; }
            public string lblPending { get; set; }
            public string lblBKTotDay { get; set; }
            public string lblTime { get; set; }
            public string lblSftotDay { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }
            public bool tdError { get; set; }
            public string BackColor { get; set; }
            public string lblErrordb { get; set; }
            public string ImageUrl { get; set; }
            public bool ImageStatus { get; set; }
            public virtual ICollection<Tractortable> SentData { get; set; }



        }

    }
}