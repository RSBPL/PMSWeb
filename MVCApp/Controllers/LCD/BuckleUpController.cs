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
    public class BuckleUpController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        // GET: BuckleUp


        public ActionResult Index()
        {
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

        [HttpPost]
        public JsonResult getPlanVertical()
        {
            var result = new BuckleUp();
            var gridleft = new List<BuckleUptabl>();
            var gridright = new List<BuckleUpTableRight>();
            string ashift = string.Empty, bshift = string.Empty, cshift = string.Empty;
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", AutoRefresh = "", DisplayMethod = "";
                plant = Convert.ToString(ConfigurationManager.AppSettings["FT_PLANT"]);
                family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);
                AutoRefresh = Convert.ToString(ConfigurationManager.AppSettings["pagerefresh"]);
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);


                string Shiftcode = "", daytot = "",NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = ""; string ShiftTlt = "";
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
                    
                    var retList = new List<string>();
                    retList = getLastshifts(Shiftcode, ShiftStart, shiftEnd);
                    //query = "select to_char(EXPECTED_qty) from XXES_DAILY_PLAN_MASTER  where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and XXES_DAILY_PLAN_MASTER.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family + "'";
                    //expqty = get_Col_Value(query).Trim();

                    //query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS s WHERE
                    //             S.ENTRYDATE>=to_date('{0}','dd-Mon-yyyy hh24:mi:ss')
                    //            AND S.ENTRYDATE<=to_date('{1}','dd-Mon-yyyy hh24:mi:ss')
                    //             and plant_code='{2}' and family_code='{3}'", ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"), plant, family);

                    //ShiftTlt = get_Col_Value(query).Trim();
                    result.lblDate = Plandate.ToString("dd MMM yyyy");
                    result.lblTime = fun.ServerDate.ToString("HH:mm:ss");
                    //lblTime.Text = "Time: " + pbf.ServerDate.ToString("HH:mm:ss");
                    result.lblShift = "SHIFT: " + Shiftcode;

                    DateTime plusday = fun.ServerDate.Date.AddDays(1);
                    string Fromdays = Plandate.ToString("dd MMM yyyy") + " " + "08:00:00";
                    string Todays = plusday.ToString("dd MMM yyyy") + " " + "01:00:00";

                    query = string.Format(@"select count(*) from xxes_job_status where plant_code = '{0}'  AND family_code = '{1}' and 
                     entrydate BETWEEN TO_DATE('{2}','DD-MON-YYYY HH24:MI:SS') and TO_DATE('{3}','DD-MON-YYYY HH24:MI:SS')",
                      plant.Trim().ToUpper(),family.Trim().ToUpper(),Fromdays,Todays);

                    daytot = get_Col_Value(query).Trim();
                    result.lblBKTotDay = "DAY TOTAL: " + (daytot.Trim() == "" ? "0" : daytot.Trim());
                    //result.lblProductionUnit = "PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());

                    //query = string.Format(@"SELECT CASE WHEN SUM(T.qty) IS NULL THEN 0 ELSE SUM(T.qty) END PLAN FROM XXES_DAILY_PLAN_TRAN T WHERE plant_code = '{0}' and family_code = '{1}' 
                    //        and PLAN_ID = (select M.plan_id from XXES_DAILY_PLAN_MASTER M  where to_char(M.PLAN_DATE,'dd-Mon-yyyy')='{2}' 
                    //        and M.SHIFTCODE='{3}' and M.plant_code= T.plant_code and M.family_code=T.family_code)", plant.Trim().ToUpper(), 
                    //        family.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"), Shiftcode.Trim().ToUpper());
                    //string planned = fun.get_Col_Value(query);
                    //result.lblPlanQty = "PLANNED: " + planned;

                    //query = string.Format(@"SELECT COUNT(*)BUCKLEUP FROM XXES_JOB_STATUS j inner join XXES_DAILY_PLAN_TRAN P on 
                    //        j.ITEM_CODE = P.ITEM_CODE and j.FCODE_ID = P.AUTOID where j.FAMILY_CODE = '{0}' AND j.PLANT_CODE = '{1}' and 
                    //        P.PLAN_ID = (select M.plan_id from XXES_DAILY_PLAN_MASTER M  where to_char(M.PLAN_DATE,'dd-Mon-yyyy')='{2}' 
                    //        and M.SHIFTCODE='{3}' and M.plant_code= P.plant_code and M.family_code=P.family_code)", family.Trim().ToUpper(),
                    //        plant.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"), Shiftcode.Trim().ToUpper());
                    //string bklup = fun.get_Col_Value(query);
                    //result.lblBK = "BUCKLE-UP: " + bklup;
                    //result.lblPending = "PENDING: " + (Convert.ToDouble(planned) - Convert.ToDouble(bklup));
                    DataTable dt = new DataTable();

                    try
                    {
                        query = "";
                        if (DisplayMethod.ToUpper() == "INLINE")
                        {
                            foreach (string shift in retList)
                            {
                                query = "select '' as SRNO, m.PLAN_ID,p.AUTOID,Item_code || '(' ||  '" + shift + "' || ')'  as MODEL,SUBSTR(p.ITEM_DESC,0,30) as DESCRIPTION,m.shiftcode as SHIFT,  " +
                                "(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as SHORTCODE  ," +
                                "(Select (case when REQUIRE_TRANS='N' then 'NA' ELSE TRANSMISSION  END) as TRANSMISSION from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as TRANS  ," +
                                "(Select (case when REQUIRE_REARAXEL='N' then 'NA' ELSE REARAXEL END) as REARAXEL from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as AXEL  ," +
                                "(Select (case when REQUIRE_HYD='N' then 'NA' ELSE HYDRAULIC END) as HYDRAULIC from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as HYD  ," +
                                "(Select (case when REQUIRE_BACKEND='N' then 'NA' ELSE BACKEND END) as BACKEND from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as SKID  ," +
                                "qty PLAN,(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as BUCKLEUP," +
                                "p.qty-(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as PENDING " +
                                "from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m  " +
                                "where p.PLAN_ID=m.PLAN_ID and m.plant_code='" + plant.Trim().ToUpper() + "' and m.family_code='" + family.Trim().ToUpper() + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and m.SHIFTCODE='" + shift.Trim().ToUpper() + "'" +
                                "GROUP BY P.AUTOID, m.PLAN_ID,p.AUTOID, Item_code,p.ITEM_DESC,m.shiftcode,P.QTY having P.QTY > (select count(*) from xxes_job_status where fcode_id=p.autoid and fcode_srlno is not null) union ";
                            }

                            query += "select '' as SRNO, m.PLAN_ID,p.AUTOID, Item_code  as MODEL,SUBSTR(p.ITEM_DESC,0,30) as DESCRIPTION,m.shiftcode as SHIFT," +
                            "(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "' ) as SHORTCODE ,  " +
                            "(Select (case when REQUIRE_TRANS='N' then 'NA' ELSE TRANSMISSION  END) as TRANSMISSION from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as TRANS  ," +
                            "(Select " +
                            "(case when REQUIRE_REARAXEL='N' then 'NA' ELSE REARAXEL END) as REARAXEL from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as AXEL  ," +
                            "(Select (case when REQUIRE_HYD='N' then 'NA' ELSE HYDRAULIC END) as HYDRAULIC from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as HYD  ," +
                            "(Select (case when REQUIRE_BACKEND='N' then 'NA' ELSE BACKEND END) as BACKEND from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as SKID  ," +
                            "qty PLAN,(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "' ) as BUCKLEUP, " +
                            "p.qty-(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as PENDING  " +
                            "from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m " +
                            "where p.PLAN_ID=m.PLAN_ID and m.plant_code='" + plant.Trim().ToUpper() + "' and m.family_code='" + family.Trim().ToUpper() + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and m.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' " +
                            " GROUP BY P.AUTOID, m.PLAN_ID,p.AUTOID, Item_code,p.ITEM_DESC,m.shiftcode,P.QTY having P.QTY>" +
                            " (select count(*) from xxes_job_status where fcode_id=p.autoid and fcode_srlno is not null) " +
                            "order by AUTOID";

                            dt = returnDataTable(query);
                        }
                        else
                        {
                            if (Shiftcode == "A")
                                ashift = Shiftcode;
                            foreach (string shift in retList)
                            {
                                if (shift == "A")
                                    ashift = shift;
                                else if (shift == "B")
                                    bshift = shift;
                                else if (shift == "C")
                                    cshift = shift;
                            }
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                               
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BKFT";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Plandate.ToString("dd-MMM-yyyy");
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = ashift;
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = bshift;
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = cshift;
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Shiftcode.Trim().ToUpper();
                                sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                OracleDataAdapter dr = new OracleDataAdapter(sc);
                                dr.Fill(dt);
                                
                            }
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
                  

                  

                    try
                    {
                        
                        List<BuckleUp> BuckleUps = new List<BuckleUp>();
                        if (dt.Rows.Count > 0)
                        {
                            if (Shiftcode == "B")
                            {
                                DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                                foreach (DataRow row in matches)
                                    dt.Rows.Remove(row);
                            }
                            else if (Shiftcode == "C")
                            {
                                DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                                foreach (DataRow row in matches)
                                    dt.Rows.Remove(row);
                                DataRow[] matches1 = dt.Select("PENDING='0' and SHIFT='B'");
                                foreach (DataRow row1 in matches1)
                                    dt.Rows.Remove(row1);
                            }

                            query = "select * from (select FCODE_ID from XXES_JOB_STATUS where FCODE_ID is not null and plant_code='" + plant + "' and family_code='" + family + "' order by ENTRYDATE DESC) where rownum=1";


                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var gridleftdata = new BuckleUptabl();
                                gridleftdata.SRNO = Convert.ToString(srno);
                                gridleftdata.MODEL = dr["MODEL"].ToString();
                                gridleftdata.DESCRIPTION = dr["DESCRIPTION"].ToString();
                                gridleftdata.SHORTCODE = dr["SHORTCODE"].ToString();
                                gridleftdata.TRANS = dr["TRANS"].ToString();
                                gridleftdata.AXEL = dr["AXEL"].ToString();
                                gridleftdata.HYD = dr["HYD"].ToString();
                                gridleftdata.SKID = dr["SKID"].ToString();
                                gridleftdata.PLAN = dr["PLAN"].ToString();
                                gridleftdata.BUCKLEUP = dr["BUCKLEUP"].ToString();
                                gridleftdata.PENDING = dr["PENDING"].ToString();

                                if (get_Col_Value(query).Trim() == dr["AUTOID"].ToString().Trim())
                                {
                                    gridleftdata.BackColor = "Yellow";
                                }

                                gridleft.Add(gridleftdata);
                                srno++;
                            }

                            result.lblSftotDay = "TOTAL PLAN: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            result.lblBK = "TOTAL BUCKLE-UP: " + Convert.ToString(dt.Compute("Sum(BUCKLEUP)", "")); 
                            
                            result.lblPending = "TOTAL PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));


                        }
                        else
                        {
                            result.lblBKTotDayVisible = false;
                            BuckleUps = null;
                            result.lblPendingVisible = false;
                            result.lblBKVisible = false;
                            result.lblPlanQtyVisible = false;
                        }
                        result.lblErrorVisible = false;
                        //expqty = get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                        //result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        //result.lblSftotDay = "SHIFT TOTAL: " + (ShiftTlt.Trim() == "" ? "0" : ShiftTlt.Trim());


                    }
                    catch (Exception ex)
                    {

                        result.lblError = ex.Message.ToString();
                        result.lblErrorVisible = true;

                    }
                    finally { }
                    //try
                    //{
                    //    query = "";

                    //    query += "SELECT * FROM (Select XT.ITEM_CODE as MODEL,XS.JOBID JOB," +
                    //    "(Select (case when REQUIRE_TRANS='N' then 'NA' ELSE XS.TRANSMISSION_SRLNO  END) as TRANSMISSION from XXES_ITEM_MASTER where ITEM_CODE=XS.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as TRANS  ," +
                    //    "(Select (case when REQUIRE_REARAXEL='N' then 'NA' ELSE XS.REARAXEL_SRLNO END) as REARAXEL from XXES_ITEM_MASTER where ITEM_CODE=XS.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as AXEL  " +
                    //    "from XXES_JOB_STATUS XS, XXES_DAILY_PLAN_TRAN XT  where XS.ITEM_CODE=XT.ITEM_CODE and XT.autoid=XS.FCODE_id and XS.plant_code='" + plant.Trim().ToUpper() + "' and XS.family_code='" + family.Trim().ToUpper() + "' and PLAN_ID IN (select PLAN_ID from XXES_DAILY_PLAN_MASTER where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "')  order by entrydate desc ) WHERE ROWNUM <= 20";
                    //    dt = new DataTable();
                    //    dt = returnDataTable(query);
                    //    if (dt.Rows.Count > 0)
                    //    {
                    //        foreach (DataRow dr in dt.Rows)
                    //        {
                    //            var gridrightdata = new BuckleUpTableRight();
                    //            gridrightdata.JOB = dr["JOB"].ToString();
                    //            gridrightdata.MODEL = dr["MODEL"].ToString();
                    //            gridrightdata.TRANS = dr["TRANS"].ToString();
                    //            gridrightdata.AXEL = dr["AXEL"].ToString();
                    //            gridright.Add(gridrightdata);
                    //        }

                    //    }
                    //    else
                    //    {

                    //    }

                    //}
                    //catch (Exception ex)
                    //{
                    //    //lblError.Visible = true;
                    //    //lblError.InnerHtml = ex.Message.ToString();
                    //}
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
            result.gridleft = gridleft;
            result.gridright = gridright;
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
            var result = new BuckleUp();
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
        public JsonResult PendingTaskComplete(BuckleUp data)
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
                if (get_Col_Value(query).Trim() == value.Trim())
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


        public class BuckleUptabl
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
            public string TRANS { get; set; }
            public string SKID { get; set; }
            public string HYD { get; set; }
            public string AXEL { get; set; }
            public string BackColor { get; set; }

        }

        public class BuckleUpTableRight
        {
            public string MODEL { get; set; }
            public string JOB { get; set; }
            public string TRANS { get; set; }
            public string AXEL { get; set; }
        }
        public class BuckleUp
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
            public string lblSftotDay { get; set; }
            public string lblTime { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }
            public bool tdError { get; set; }
            public string BackColor { get; set; }
            public string lblErrordb { get; set; }
            public string ImageUrl { get; set; }
            public bool ImageStatus { get; set; }
            public virtual ICollection<BuckleUptabl> gridleft { get; set; }
            public virtual ICollection<BuckleUpTableRight> gridright { get; set; }



        }

    }
}