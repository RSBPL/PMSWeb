using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

using System.Data;
using Newtonsoft.Json;
using System.Configuration;

namespace MVCApp.Controllers
{

    public class BeforePaintLCDController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

         
        public ActionResult Index()
        {
            //if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            //{
            //    Session["BeforeScreenUnit"] = Url.RequestContext.RouteData.Values["id"];
            //    ViewBag.CheckURL = "Y";
            //}    
            //else
            //    ViewBag.CheckURL = "N";

           
                Session["BeforeScreenUnit"] = Url.RequestContext.RouteData.Values["id"];
                //ViewBag.CheckURL = "Y";

                return View();
        }

        public PartialViewResult Grid()
        {
            string plant = "", family = "", DisplayMethod = ""; 
            List<BEFOREPAINTLCDGRID> GridList = new List<BEFOREPAINTLCDGRID>();
            MsgReturn mr = new MsgReturn();
            try
            {
                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = "";

                //if (string.IsNullOrEmpty(Convert.ToString(Session["BeforeScreenUnit"])))
                //{
                //    return PartialView(GridList);
                //}
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);

                plant = Convert.ToString(Session["BeforeScreenUnit"]);
                if (plant == "4")
                {
                    plant = "T04";
                    family = "TRACTOR FTD";
                }
                else if (plant == "5")
                {
                    plant = "T05";
                    family = "TRACTOR FT";
                }               

                mr = CheckInfo(plant, family);

                DateTime ShiftStart, shiftEnd;
                data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    NightExists = data.Split('#')[1].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    ShiftStart = Convert.ToDateTime(data.Split('#')[3].Trim());
                    shiftEnd = Convert.ToDateTime(data.Split('#')[4].Trim());

                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                        Plandate = fun.GetServerDateTime().Date.AddDays(-1);
                    else
                        Plandate = fun.GetServerDateTime().Date;

                    ViewBag.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    ViewBag.Shift = "SHIFT: " + Shiftcode;
                    //string planid = fun.GetplanId(plant, family, Plandate, Shiftcode);
                    //if(string.IsNullOrEmpty(planid))
                    //{
                    //    //return v;
                    //}
                    DateTime Date = fun.GetServerDateTime();

                    string Str = Convert.ToString(Date);
                    char[] Spearator = { ' ' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);

                    ViewBag.Time = "TIME: " + StrDate[1];

                    DataTable dt = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query = String.Format(@"SELECT ROW_NUMBER() OVER(ORDER BY PLAN_ID) AS SRLNO, PLAN_ID, AUTOID, FCODE, SUBSTR(DESCRIPTION, 0, 30) DESCRIPTION, SHORTCODE, ENGINE, LIFT, SPOOL_VALUE, TANDEM_PUMP, STARTER_MOTOR, ALTERNATOR, BRK_PAD, FUEL_TANK, CLUTCH_PEDAL, PLANNED, ACTUAL, PENDING, NVL((TRUNC(SYSDATE) - TRUNC(BUCKLEUP_DATE)), 0) AGEING_DAYS FROM (SELECT M.PLAN_ID, P.AUTOID, P.ITEM_CODE AS FCODE, SUBSTR(P.ITEM_DESC, 0, 30) AS DESCRIPTION, ENGINE, HYDRAULIC AS LIFT, SPOOL_VALUE, TANDEM_PUMP, STARTER_MOTOR, ALTERNATOR, BRK_PAD, FUEL_TANK, CLUTCH_PEDAL, (SELECT SHORT_CODE FROM XXES_ITEM_MASTER WHERE ITEM_CODE = P.ITEM_CODE AND PLANT_CODE = M.PLANT_CODE AND FAMILY_CODE = M.FAMILY_CODE) AS SHORTCODE, M.PLAN_DATE, (CASE WHEN TRUNC(M.plan_date) < TRUNC(SYSDATE) THEN 0 ELSE qty END) AS PLANNED, (CASE WHEN TRUNC(M.plan_date) < TRUNC(SYSDATE) THEN (SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE XXES_JOB_STATUS.FCODE_ID = P.AUTOID AND XXES_JOB_STATUS.JOBID NOT IN (SELECT JOBID FROM XXES_CONTROLLERS_DATA WHERE STAGE = 'BP' AND FCODE_ID = P.AUTOID)) ELSE P.qty - (SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE XXES_CONTROLLERS_DATA.JOBID IN (SELECT JOBID FROM XXES_DAILY_PLAN_JOB WHERE XXES_DAILY_PLAN_JOB.FCODE_AUTOID = P.AUTOID) AND XXES_CONTROLLERS_DATA.STAGE = 'BP' AND XXES_CONTROLLERS_DATA.FCODE_ID = P.AUTOID) END) AS PENDING, (CASE WHEN TRUNC(M.plan_date) < TRUNC(SYSDATE) THEN (SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE XXES_CONTROLLERS_DATA.STAGE = 'BP' AND XXES_CONTROLLERS_DATA.FCODE_ID = P.AUTOID AND TRUNC(ENTRY_DATE) = TRUNC(SYSDATE) AND XXES_CONTROLLERS_DATA.JOBID IN (SELECT JOBID FROM XXES_DAILY_PLAN_JOB WHERE XXES_DAILY_PLAN_JOB.FCODE_AUTOID = P.AUTOID)) ELSE (SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE XXES_CONTROLLERS_DATA.STAGE = 'BP' AND XXES_CONTROLLERS_DATA.FCODE_ID = P.AUTOID AND XXES_CONTROLLERS_DATA.JOBID IN (SELECT JOBID FROM XXES_DAILY_PLAN_JOB WHERE XXES_DAILY_PLAN_JOB.FCODE_AUTOID = P.AUTOID)) END) AS ACTUAL, (SELECT DISTINCT ENTRYDATE FROM XXES_JOB_STATUS WHERE XXES_JOB_STATUS.ITEM_CODE = ITEM_CODE AND XXES_JOB_STATUS.FCODE_ID = AUTOID AND ROWNUM = 1) AS BUCKLEUP_DATE FROM XXES_DAILY_PLAN_MASTER M, XXES_DAILY_PLAN_TRAN P, XXES_ITEM_MASTER I WHERE P.ITEM_CODE = I.ITEM_CODE AND M.PLAN_ID = P.PLAN_ID AND M.PLANT_CODE = P.PLANT_CODE AND M.FAMILY_CODE = P.FAMILY_CODE AND M.PLANT_CODE = I.PLANT_CODE AND M.FAMILY_CODE = P.FAMILY_CODE AND M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND TRUNC(M.plan_date) <= '{2}' ORDER BY AUTOID) WHERE pending > '0' ORDER BY AGEING_DAYS DESC", plant.Trim(), family.Trim(), Plandate.ToString("dd-MMM-yyyy"));
                        dt = fun.returnDataTable(query);
                    }
                    else 
                    {
                        bool result = false;

                        try
                        {
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                                fun.ConOpen();
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BP";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Plandate.ToString("dd-MMM-yyyy");
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
                                fun.ConClose();
                                result = true;
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
                        //using (dt = returnDataTableUsingCommand("usp_getLiveData", "PROC"))
                       
                            if (dt.Rows.Count > 0)
                            {
                                DataRow[] matches = dt.Select("PENDING='0'");
                                foreach (DataRow row in matches)
                                    dt.Rows.Remove(row);
                                int COUNT = 0;
                                bool Resetflag = false;
                                foreach (DataRow dr in dt.Rows)
                                {
                                   COUNT++;
                                    if (Convert.ToInt32(dr["AGEING_DAYS"]) == 0 && !Resetflag)
                                    {
                                        COUNT = 1;
                                        Resetflag = true;
                                    };
                                    BEFOREPAINTLCDGRID GR = new BEFOREPAINTLCDGRID
                                    {
                                        SRNO = COUNT.ToString(),
                                        FCODE = dr["FCODE"].ToString(),
                                        //DESCRIPTION = dr["DESCRIPTION"].ToString(),
                                        SHORTCODE = dr["SHORTCODE"].ToString(),
                                        //ENGINE = dr["ENGINE"].ToString(),
                                        //LIFT = dr["LIFT"].ToString(),
                                        //FRONT_AXLE = dr["FRONT_AXLE"].ToString(),
                                        BRAKE_PEDAL = dr["BRK_PAD"].ToString(),
                                        FUEL_TANK = dr["FUEL_TANK"].ToString(),
                                        CLUTCH_PEDAL = dr["CLUTCH_PEDAL"].ToString(),
                                        SPOOL_VALUE = dr["SPOOL_VALUE"].ToString(),
                                        TANDEM_PUMP = dr["TANDEM_PUMP"].ToString(),
                                        STARTER_MOTOR = dr["STARTER_MOTOR"].ToString(),
                                        ALTERNATOR = dr["ALTERNATOR"].ToString(),
                                        //FRONT_SUPPORT = dr["FRONT_SUPPORT"].ToString(),
                                        //STEERING_COLUMN = dr["STEERING_COLUMN"].ToString(),
                                        PLANNED = dr["PLANNED"].ToString(),
                                        ACTUAL = dr["ACTUAL"].ToString(),
                                        PENDING = dr["PENDING"].ToString(),
                                        AGEING_DAYS = Convert.ToInt32(dr["AGEING_DAYS"].ToString()),

                                    };
                                    GridList.Add(GR);
                                    
                                }                                
                            }
                            //ViewBag.DataSource = GridList;

                            ViewBag.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));

                            string fromtime = ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss");
                            string totime = shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss");

                            query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='BP' AND HOOK_NO <> 9999 AND PLANT_CODE = 'T04' AND ENTRY_DATE BETWEEN to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), fromtime, totime);
                            expqty = fun.get_Col_Value(query);
                            ViewBag.lblHookUp = "HOOKUP: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                            ViewBag.lblDayTotal = GetDayTotalHookedUp(plant.Trim());
                         
                    }
                    catch (Exception ex)
                    {
                        mr.lblError = mr.lblError + "\n" + ex.Message.ToString();
                        mr.lblErrorTF = true;
                    }
                    finally { }                   
                    //CheckInfo(plant, family);
                }
            }
            catch (Exception ex)
            {
                mr.lblError = mr.lblError + "\n" + ex.Message.ToString();
                mr.lblErrorTF = true;
            }
            finally { }
            ViewBag.lblInfoTF = mr.lblInfoTF;
            ViewBag.lblInfo = mr.lblInfo;
            ViewBag.lblInfodbTF = mr.lblInfodbTF;
            ViewBag.lblInfodb = mr.lblInfodb;
            ViewBag.lblErrorTF = mr.lblErrorTF;
            ViewBag.lblError = mr.lblError;
            ViewBag.lblErrordbTF = mr.lblErrordbTF;
            ViewBag.lblErrordb = mr.lblErrordb;

            return PartialView(GridList);
        }

        //public string GetDayTotalHookUp(string plant)
        //{
        //    DataTable dtshift = new DataTable();
        //    string fromdate = string.Empty;
        //    string todate = string.Empty;

        //    dtshift = fun.returnDataTable("select * from XXES_SHIFT_MASTER order by SHIFTCODE");
        //    if (dtshift.Rows.Count > 0)
        //    {
        //        foreach (DataRow dr in dtshift.Rows)
        //        {
        //            DateTime abc = fun.GetServerDateTime();

        //            if (Convert.ToString(dr["SHIFTCODE"]) == "A")
        //            {
        //                fromdate = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"])).ToString();
                       
        //            }
        //            else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && Convert.ToDateTime(abc) >= Convert.ToDateTime(DateTime.Parse("00:01")) && Convert.ToDateTime(abc) <= Convert.ToDateTime(DateTime.Parse("01:00")))
        //            {
        //                HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.AddDays(-1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
        //                HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
        //                isDayNeedToLess = "1";
        //                HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(-1);
        //            }
        //            else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && Convert.ToDateTime(abc) >= Convert.ToDateTime(DateTime.Parse(Convert.ToString(dr["START_TIME"]))) && Convert.ToDateTime(abc) <= Convert.ToDateTime(DateTime.Parse("23:59")))
        //            {
        //                HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
        //                HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.AddDays(1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
        //                HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(0);
        //            }
        //            else if (Convert.ToString(dr["SHIFTCODE"]) == "C")
        //            {
        //                HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
        //                HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
        //                HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(-1);
        //            }
        //            if (Convert.ToDateTime(abc) >= Convert.ToDateTime(HttpContext.Current.Session["ShiftStart"]) && Convert.ToDateTime(abc) <= Convert.ToDateTime(HttpContext.Current.Session["shiftEnd"]))
        //            {
        //                returnData = Convert.ToString(dr["SHIFTCODE"]) + '#' + Convert.ToString(dr["NIGHT_EXISTS"]) + '#' + isDayNeedToLess + '#' + Convert.ToDateTime(HttpContext.Current.Session["ShiftStart"]).ToString() + '#' + Convert.ToDateTime(HttpContext.Current.Session["shiftEnd"]).ToString();
        //                break;
        //            }
        //        }
        //    }
        //}

        private MsgReturn CheckInfo(string plant, string family)
        {
            //if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            //    ViewBag.CheckURL = "Y";
            //else
            //    ViewBag.CheckURL = "N";

            MsgReturn R = new MsgReturn();
            try
            {
                bool isError = false;
                query = "select remarks1 || '#' || scan_date  from XXES_LIVE_DATA where stage='BP' and DATA_TYPE='MSG' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                string data = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(data))
                {
                    R.lblInfodbTF = true;
                    R.lblInfodb = data.Split('#')[0].Trim() + "\t\tScan Date: " + data.Split('#')[1].Trim();
                }
                query = "select remarks1 || '#' || scan_date from XXES_LIVE_DATA where stage='BP' and DATA_TYPE='ERROR'  and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                data = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(data))
                {
                    R.lblErrordbTF = true;
                    R.lblErrordb = data.Split('#')[0].Trim() + "\t\tScan Date: " + data.Split('#')[1].Trim();
                    isError = true;
                }
                try
                {
                    if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='BP' and STATUS='ONLINE'  and paramvalue='STATUS' and description='" + plant + "'"))
                    {
                        R.imgstatus = "~\\Image\\Green.png";
                        R.imgstatusHS = "visible";
                    }
                    else if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='BP' and STATUS='OFFLINE'  and paramvalue='STATUS' and description='" + plant + "'"))
                    {
                        R.imgstatus = "~\\Image\\Red.png";
                        R.imgstatusHS = "hidden";
                    }
                    else
                    {
                        R.imgstatusHS = "hidden";
                    }
                }
                catch { }
                finally { }
                try
                {
                    string mdate = fun.get_Col_Value("select PARAMVALUE from xxes_sft_settings where parameterinfo='BP' and STATUS='LAST_DATE' and description='" + plant + "'");
                    if (!string.IsNullOrEmpty(mdate))
                    {

                        TimeSpan span = fun.GetServerDateTime().Subtract(Convert.ToDateTime(mdate));
                        if (span.Minutes >= 1)
                        {
                            R.lblErrordbTF = true;
                            if (isError == true)
                                R.lblErrordb = R.lblErrordb + "\n Or Please Check Controller Service";
                            else
                                R.lblErrordb = "Please Check Controller Service";
                            R.imgstatus = "~\\Image\\Red.png";
                            R.imgstatusHS = "visible";
                        }
                        else
                        {
                            R.imgstatus = "~\\Image\\Green.png";
                            R.imgstatusHS = "visible";

                            if (isError == false)
                                R.lblErrordbTF = false;
                        }
                    }
                }
                catch { }
                finally { }
            }
            catch(Exception ex)
            {
                R.lblErrorTF = true;
                R.lblError = ex.Message.ToString();
            }
            finally { }
            return R;
        }

        public string GetDayTotalHookedUp(string plant)
        {
            DataTable dtshift = new DataTable();
            string ShiftStart = string.Empty;
            string ShiftEnd = string.Empty;
            dtshift = fun.returnDataTable("select * from XXES_SHIFT_MASTER order by SHIFTCODE");
            if (dtshift.Rows.Count > 0)
            {         
                foreach (DataRow dr in dtshift.Rows)
                {
                    DateTime abc = fun.GetServerDateTime();

                    if (Convert.ToString(dr["SHIFTCODE"]) == "A")
                    {
                        ShiftStart = Convert.ToDateTime(dr["START_TIME"]).ToString("dd-MMM-yyyy HH:mm:ss");
                    }                    
                    else if (Convert.ToString(dr["SHIFTCODE"]) == "C")
                    {
                        ShiftEnd = Convert.ToDateTime(dr["END_TIME"]).AddDays(1).ToString("dd-MMM-yyyy HH:mm:ss");
                    }
                }
            }
            query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='BP' AND HOOK_NO <> 9999 AND PLANT_CODE = '{0}' AND ENTRY_DATE BETWEEN 
                                          to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), ShiftStart, ShiftEnd);
            string expqty = fun.get_Col_Value(query);
            return "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
        }
    }

    public class MsgReturn
    {
        public string lblInfo { get; set; }
        public bool lblInfoTF { get; set; }

        public string lblInfodb { get; set; }
        public bool lblInfodbTF { get; set; }

        public string lblError { get; set; }
        public bool lblErrorTF { get; set; }

        public string lblErrordb { get; set; }
        public bool lblErrordbTF { get; set; }

        public string imgstatus { get; set; }
        public string imgstatusHS { get; set; }
    }
}