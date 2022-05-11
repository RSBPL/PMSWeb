using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;

namespace MVCApp.Controllers.LCD
{
    public class FTTractorAssemblyController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        // GET: FTTractorAssembly
         public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
            return View();
        }

        public string get_Col_Value(string command)
        {
            OracleConnection ConOrcl = new OracleConnection(Function.orCnstr);
            try
            {
                string returnValue = "";
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = ConOrcl;
                cmd.CommandText = command;
                cmd.CommandType = CommandType.Text;
                returnValue = Convert.ToString(cmd.ExecuteScalar());
                return returnValue;
            }
            catch { throw; return ""; }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }


        [HttpGet]
        public JsonResult getPlanVertical(string PLANTCODE)
        {
            var result = new GridData();
            var BuckleUp = new List<BuckleUp>();
            var Hookup = new List<Hookup>();
            var RollDown = new List<RollDown>();
            var PdiOk = new List<PdiOk>();
            
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", DisplayMethod = "";
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
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                plant = Convert.ToString(ConfigurationManager.AppSettings["FT_PLANT"]);
                family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);
                string ShiftTlt = "", Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = ""; 
                DateTime ShiftStart, shiftEnd;
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
                    string planid = fun.GetplanId(plant, family, Plandate, Shiftcode);
                    
                    if (!string.IsNullOrEmpty(planid))
                    {
                        query = "select to_char(EXPECTED_qty) from XXES_DAILY_PLAN_MASTER  where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and XXES_DAILY_PLAN_MASTER.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family + "'";
                        expqty = get_Col_Value(query).Trim();
                        result.lblDate = Plandate.ToString("dd MMM yyyy");
                        result.lblShift = "SHIFT: " + Shiftcode;
                        query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS s WHERE
                                 S.ENTRYDATE>=to_date('{0}','dd-Mon-yyyy hh24:mi:ss')
                                AND S.ENTRYDATE<=to_date('{1}','dd-Mon-yyyy hh24:mi:ss')
                                 and plant_code='{2}' and family_code='{3}'", ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"), plant, family);

                        ShiftTlt = get_Col_Value(query).Trim();
                        result.lblSftotDay = "SHIFT TOTAL: " + (ShiftTlt.Trim() == "" ? "0" : ShiftTlt.Trim());
                        result.lblProductionUnit = "PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        query = "";

                        DataTable buckleupdt = new DataTable();
                        DataTable Hookupdt = new DataTable();
                        DataTable RollDowndt = new DataTable();
                        DataTable PdiOkdt = new DataTable();

                        if (DisplayMethod.ToUpper() == "INLINE")
                        {
                            string buckleupQuery = string.Format(@"select STAGE,PLANNED,ACTUAL,
                                        (CASE WHEN PLANNED<>0 THEN
                                        ROUND((ACTUAL/PLANNED)*100 ,1)
                                        ELSE
                                        0 END) Efficiency
                                        from (
                                        select 'BUCKLE-UP' Stage, sum(qty) Planned,
                                        (select count(*) from xxes_job_status where fcode_id in (select autoid from xxes_daily_plan_tran where plan_id='{0}')
                                        ) Actual
                                        from xxes_daily_plan_tran t where plan_id='{0}' group by t.plan_id)a", planid.Trim());
                            buckleupdt = fun.returnDataTable(buckleupQuery);

                            string HookupQuery = string.Format(@"SELECT 
                                            PLANNED,
                                            ACTUAL,
                                            (CASE WHEN PLANNED <> 0 THEN ROUND((ACTUAL / PLANNED) * 100, 1) ELSE 0 END) EFFICIENCY
                                            FROM (SELECT 
                                            (SELECT SUM(QTY) FROM XXES_DAILY_PLAN_TRAN
                                            WHERE PLAN_ID = '{0}') PLANNED ,
                                            (SELECT COUNT(*)
                                            FROM XXES_CONTROLLERS_DATA D
                                            WHERE stage='BP' AND HOOK_NO <> 9999 AND PLANT_CODE = '{1}' AND ENTRY_DATE BETWEEN 
                                            to_date('{2}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')) ACTUAL
                                            FROM XXES_JOB_STATUS S
                                            WHERE FCODE_ID IN (SELECT AUTOID
                                            FROM XXES_DAILY_PLAN_TRAN
                                            WHERE PLAN_ID = '{0}')
                                            GROUP BY S.PLANT_CODE
                                            ) A", planid.Trim(), plant.Trim(), ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"));
                            Hookupdt = fun.returnDataTable(HookupQuery);

                            string RollDownQuery = string.Format(@"SELECT 
                                        PLANNED,
                                        ACTUAL,
                                        (CASE WHEN PLANNED <> 0 THEN ROUND((ACTUAL / PLANNED) * 100, 1) ELSE 0 END) EFFICIENCY
                                    FROM (
                                    SELECT
                                            (SELECT SUM(QTY)
                                                FROM XXES_DAILY_PLAN_TRAN D
                                                        WHERE PLAN_ID = '{0}') PLANNED,
                                            COUNT(*) ACTUAL
                                        FROM XXES_JOB_STATUS
                                        WHERE TO_CHAR(FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS') >= '{2}'
                                        AND TO_CHAR(FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS') <= '{3}'
                                        AND PLANT_CODE = '{1}'
                                        GROUP BY PLANT_CODE) A", planid.Trim(), plant.Trim(), ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"));
                            RollDowndt = fun.returnDataTable(RollDownQuery);

                            string PdiOdQuery = string.Format(@"SELECT PLANNED,
                                    ACTUAL,
                                    (CASE WHEN PLANNED <> 0 THEN ROUND((ACTUAL / PLANNED) * 100, 1) ELSE 0 END) EFFICIENCY
                                FROM (SELECT 
                                        (SELECT SUM(QTY) FROM XXES_DAILY_PLAN_TRAN
                                            WHERE PLAN_ID = '{1}') PLANNED,
                                            (SELECT COUNT(*)
                                                FROM XXES_JOB_STATUS
                                                WHERE TO_CHAR(PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS') >= '{2}'
                                                    AND TO_CHAR(PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS') <= '{3}'
                                                    AND PLANT_CODE = 'T04') ACTUAL

                                    FROM XXES_JOB_STATUS
                                    WHERE TO_CHAR(FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS') >= '{2}'
                                    AND TO_CHAR(FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS') <= '{3}'
                                    AND PLANT_CODE = '{0}'
                                    GROUP BY PLANT_CODE) A ", plant.Trim(), planid.Trim(), ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"));
                            PdiOkdt = fun.returnDataTable(PdiOdQuery);                            

                        }
                        else {

                            try
                            {
                                using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                                {
                                    fun.ConOpen();
                                    sc.CommandType = CommandType.StoredProcedure;
                                    sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "FTTA";
                                    sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                    sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = planid.Trim();
                                    sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss");
                                    sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss");
                                    sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                    sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                    OracleDataAdapter dr = new OracleDataAdapter(sc);
                                    dr.Fill(dt);
                                    fun.ConClose();
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
                                fun.Connection().Dispose();
                            }
                            //return ;

                        }
                        try
                        {
                            if (buckleupdt.Rows.Count > 0)
                            {
                                int srno = 1;
                                foreach (DataRow dr in buckleupdt.Rows)
                                {
                                    var BuckleUpdata = new BuckleUp();

                                    BuckleUpdata.SRNO = Convert.ToString(srno);
                                    BuckleUpdata.PLANNED = dr["PLANNED"].ToString();
                                    BuckleUpdata.ACTUAL = dr["ACTUAL"].ToString();
                                    BuckleUpdata.EFFICIENCY = dr["EFFICIENCY"].ToString();

                                    BuckleUp.Add(BuckleUpdata);
                                    srno++;
                                }
                            }
                            if (Hookupdt.Rows.Count > 0) 
                            {
                                int srno = 1;
                                foreach (DataRow dr in Hookupdt.Rows)
                                {
                                    var Hookupdata = new Hookup();

                                    Hookupdata.SRNO = Convert.ToString(srno);
                                    Hookupdata.PLANNED = dr["PLANNED"].ToString();
                                    Hookupdata.ACTUAL = dr["ACTUAL"].ToString();
                                    Hookupdata.EFFICIENCY = dr["EFFICIENCY"].ToString();

                                    Hookup.Add(Hookupdata);
                                    srno++;
                                }
                            }
                            if (RollDowndt.Rows.Count > 0)
                            {
                                int srno = 1;
                                foreach (DataRow dr in RollDowndt.Rows)
                                {
                                    var RollDowndata = new RollDown();

                                    RollDowndata.SRNO = Convert.ToString(srno);
                                    RollDowndata.PLANNED = dr["PLANNED"].ToString();
                                    RollDowndata.ACTUAL = dr["ACTUAL"].ToString();
                                    RollDowndata.EFFICIENCY = dr["EFFICIENCY"].ToString();

                                    RollDown.Add(RollDowndata);
                                    srno++;
                                }
                            }
                            if (PdiOkdt.Rows.Count > 0)
                            {
                                int srno = 1;
                                foreach (DataRow dr in PdiOkdt.Rows)
                                {
                                    var PdiOkdata = new PdiOk();

                                    PdiOkdata.SRNO = Convert.ToString(srno);
                                    PdiOkdata.PLANNED = dr["PLANNED"].ToString();
                                    PdiOkdata.ACTUAL = dr["ACTUAL"].ToString();
                                    PdiOkdata.EFFICIENCY = dr["EFFICIENCY"].ToString();

                                    PdiOk.Add(PdiOkdata);
                                    srno++;
                                }
                            }
                            else
                            {
                                result.lblBKTotDayVisible = false;
                                //gridVertical.DataSource = null;
                                //gridVertical.DataBind();
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
                    else
                    {
                        result.lblError = "PLAN NOT FOUND FOR CURRENT DATE";
                    }
                    getMsg();
                }
            }
            catch (Exception ex)
            {
                //result.Visible = true;

                result.lblError = ex.Message.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            finally { }
            result.lblTime = fun.ServerDate.ToString("HH:mm:ss");
            
            result.BuckleUp = BuckleUp;
            result.Hookup = Hookup;
            result.RollDown = RollDown;
            result.PdiOk = PdiOk;
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
            var result = new GridData();
            try
            {
                query = "select PARAMVALUE from XXES_SFT_SETTINGS where PARAMETERINFO='LCD_MSG' and STATUS='Y'";
                string msg = get_Col_Value(query);
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

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            var result = new GridData();
            try
            {

                fun.ServerDate = fun.GetServerDateTime();
                //getPlanVertical();
                result.lblTime = "Time: " + fun.ServerDate.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                //result.lblError.Visible = true;
                result.lblError = ex.Message.ToString();
                Timer1_Tick(new object(), new EventArgs());
            }
            finally { }
        }
        
        public class BuckleUp
        {
            public string SRNO { get; set; }
            public string PLANNED { get; set; }
            public string ACTUAL { get; set; }
            public string EFFICIENCY { get; set; }
            
        }
        
        public class Hookup
        {
            public string SRNO { get; set; }
            public string PLANNED { get; set; }
            public string ACTUAL { get; set; }
            public string EFFICIENCY { get; set; }
            
        }
        
        public class RollDown
        {
            public string SRNO { get; set; }
            public string PLANNED { get; set; }
            public string ACTUAL { get; set; }
            public string EFFICIENCY { get; set; }
            
        }
        
        public class PdiOk
        {
            public string SRNO { get; set; }
            public string PLANNED { get; set; }
            public string ACTUAL { get; set; }
            public string EFFICIENCY { get; set; }
            
        }

        public class GridData
        {
            public string SRNO { get; set; }
            public string PLANID { get; set; }
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
            public string lblSftotDay { get; set; }
            public string lblProductionUnit { get; set; }
            public string lblBK { get; set; }
            public string lblPending { get; set; }
            public string lblBKTotDay { get; set; }
            public string lblTime { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }
            public bool tdError { get; set; }
            public string lblErrordb { get; set; }
            public string ImageUrl { get; set; }
            public string BackColor { get; set; }
            public bool ImageStatus { get; set; }
            public virtual ICollection<BuckleUp> BuckleUp { get; set; }
            public virtual ICollection<Hookup> Hookup { get; set; }
            public virtual ICollection<RollDown> RollDown { get; set; }
            public virtual ICollection<PdiOk> PdiOk { get; set; }

        }


    }

}