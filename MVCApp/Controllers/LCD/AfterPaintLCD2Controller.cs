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
    public class AfterPaintLCD2Controller : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        

        public ActionResult Index(string id)
        {
            //if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            //{
            //    Session["AfterScreenUnit"] = Url.RequestContext.RouteData.Values["id"];
            //    ViewBag.CheckURL = "Y";
            //}
            //else
            //{
            //    ViewBag.CheckURL = "N";
            //}
            ViewBag.PlantCode = id;
            //Session["AfterScreenUnit"] = Url.RequestContext.RouteData.Values["id"];
            //ViewBag.CheckURL = "Y"; 


            return View();
        }
        
        [HttpGet]
        public JsonResult Grid(string PLANTCODE)
        {
            string plant = "", family = "", DisplayMethod = "";
            List<APGRID2> GridList = new List<APGRID2>();
            var result = new APGRID2();
            MsgReturn mr = new MsgReturn();
            try
            {
                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = "";

                //if (string.IsNullOrEmpty(Convert.ToString(Session["AfterScreenUnit"])))
                //{
                //   return PartialView(GridList);
                //}
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                plant = PLANTCODE;
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

                result = CheckInfo(plant, family);

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

                    result.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    result.Shift = "SHIFT: " + Shiftcode;

                    DateTime Date = fun.GetServerDateTime();

                    string Str = Convert.ToString(Date);
                    char[] Spearator = { ' ' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);

                    result.Time = "TIME: " + StrDate[1];

                    DataTable dt = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query = String.Format(@"SELECT '' AS SRNO, ITEM_CODE AS F_CODE, SUBSTR(ITEM_DESCRIPTION, 0, 30) AS ITEM_DESCRIPTION, SHORT_CODE, FENDER_RH, FENDER_LH, FENDER_HARNESS_RH, FENDER_HARNESS_LH, FENDER_LAMP, RADIATOR, DONE, PENDING, from_id, to_id, ABS((from_id - to_id)) + 1 QTY,(SELECT (RTRIM(XMLAGG (XMLELEMENT(E, LTRIM(RTRIM(SUBSTR(A.HOOK_NO, 3, 2))) || ',') ORDER BY srlno).EXTRACT('//text()'), ',')) AS HOOK FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, FCODE_ID, C.ITEM_CODE, C.HOOK_NO FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.final_label_date IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) A WHERE TO_NUMBER(srlno, '999') >= TO_NUMBER(from_id, '999') AND TO_NUMBER(srlno, '999') <= TO_NUMBER(to_id, '999')) AS HOOK, BATTERY, HEAD_LAMP, STEERING_WHEEL, REAR_HOOD_WIRING_HARNESS, FRONTTYRE, REARTYRE, SEAT FROM (SELECT MIN(srlno) FROM_ID, MAX(srlno) TO_ID, ITEM_CODE, ITEM_DESCRIPTION, SHORT_CODE, FENDER_RH, FENDER_LH, FENDER_HARNESS_RH, FENDER_HARNESS_LH, FENDER_LAMP, RADIATOR, DONE, PENDING, STEERING_WHEEL, REAR_HOOD_WIRING_HARNESS, FRONTTYRE, REARTYRE, BATTERY, HEAD_LAMP, SEAT FROM (SELECT srlno, srlno - ROW_NUMBER() OVER (PARTITION BY ITEM_CODE ORDER BY srlno) GRP, FCODE_ID, ITEM_CODE, ITEM_DESCRIPTION, SHORT_CODE, FENDER_RH, FENDER_LH, FENDER_HARNESS_RH, FENDER_HARNESS_LH, FENDER_LAMP, RADIATOR, DONE, PENDING, STEERING_WHEEL, REAR_HOOD_WIRING_HARNESS, FRONTTYRE, REARTYRE, HOOK_NO, BATTERY, HEAD_LAMP, SEAT FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, FCODE_ID, M.ITEM_CODE, ITEM_DESCRIPTION, M.SHORT_CODE, M.FENDER_RH, M.FENDER_LH, M.FENDER_HARNESS_RH, M.FENDER_HARNESS_LH, M.FENDER_LAMP, (SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE ITEM_CODE = C.ITEM_CODE AND XXES_JOB_STATUS.FCODE_ID = C.AUTOID AND PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}') AS DONE, (SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE ITEM_CODE = C.ITEM_CODE AND XXES_JOB_STATUS.FCODE_ID = C.AUTOID AND PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}') AS PENDING, M.RADIATOR, STEERING_WHEEL, REAR_HOOD_WIRING_HARNESS, FRONTTYRE, REARTYRE, C.HOOK_NO, M.BATTERY, M.HEAD_LAMP, M.SEAT FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND C.PLANT_CODE = '{0}' AND C.FAMILY_CODE = '{1}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.final_label_date IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) ORDER BY srlno) GROUP BY ITEM_CODE, ITEM_DESCRIPTION, SHORT_CODE, FENDER_RH, FENDER_LH, FENDER_HARNESS_RH, FENDER_HARNESS_LH, FENDER_LAMP, RADIATOR, DONE, PENDING, STEERING_WHEEL, REAR_HOOD_WIRING_HARNESS, FRONTTYRE, REARTYRE, BATTERY, HEAD_LAMP, SEAT, grp ORDER BY from_id) ORDER BY from_id", plant.ToUpper().Trim(), family.ToUpper().Trim());
                        dt = fun.returnDataTable(query);
                    }
                    else
                    {
                        try
                        {
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                                fun.ConOpen();
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "AP1";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
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
                            fun.Connection().Dispose();
                        }
                        //return ;

                    }
                    
                    try
                    {
                        if (dt.Rows.Count > 0)
                        {
                            if (Convert.ToString(ConfigurationSettings.AppSettings["AP_HOOK_SORT"]) == "Y")
                                dt.DefaultView.Sort = "HOOK";
                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var APtablsdata = new APGRID2();
                                APtablsdata.SRNO = srno;
                                APtablsdata.F_CODE = dr["F_CODE"].ToString();
                                APtablsdata.SHORT_CODE = dr["SHORT_CODE"].ToString();
                                APtablsdata.FENDER_LH = dr["FENDER_LH"].ToString();
                                APtablsdata.FENDER_RH = dr["FENDER_RH"].ToString();
                                APtablsdata.FENDER_HARNESS_LH = dr["FENDER_HARNESS_LH"].ToString();
                                APtablsdata.FENDER_HARNESS_RH = dr["FENDER_HARNESS_RH"].ToString();
                                APtablsdata.RADIATOR = dr["RADIATOR"].ToString();
                                APtablsdata.FENDER_LAMP = dr["FENDER_LAMP"].ToString();
                                APtablsdata.QTY = dr["QTY"].ToString();
                                APtablsdata.DONE = dr["DONE"].ToString();
                                APtablsdata.PENDING = dr["PENDING"].ToString();

                                GridList.Add(APtablsdata);
                                srno++;
                            }
                            //int COUNT = 0;
                            //COUNT++;
                            //    APGRID2 GR = new APGRID2
                            //    {
                            //        SRNO = COUNT,
                            //        F_CODE = dr["F_CODE"].ToString(),
                            //        SHORT_CODE = dr["SHORT_CODE"].ToString(),
                            //        FENDER_LH = dr["FENDER_LH"].ToString(),
                            //        FENDER_RH = dr["FENDER_RH"].ToString(),
                            //        FENDER_HARNESS_LH = dr["FENDER_HARNESS_LH"].ToString(),
                            //        FENDER_HARNESS_RH = dr["FENDER_HARNESS_RH"].ToString(),
                            //        RADIATOR = dr["RADIATOR"].ToString(),
                            //        FENDER_LAMP = dr["FENDER_LAMP"].ToString(),
                            //        PLAN = dr["QTY"].ToString(),
                            //        DONE = dr["DONE"].ToString(),
                            //        PENDING = dr["PENDING"].ToString(),


                            //        //ITEM_CODE = dr["ITEM_CODE"].ToString(),
                            //        //ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                            //        ////SHORT_CODE = dr["SHORT_CODE"].ToString(),
                            //        //FROM_ID = dr["FROM_ID"].ToString(),
                            //        //TO_ID = dr["TO_ID"].ToString(),
                            //        //QTY = dr["QTY"].ToString(),
                            //        //HOOK = dr["HOOK"].ToString(),
                            //        //BATTERY = dr["BATTERY"].ToString(),
                            //        //HEAD_LAMP = dr["HEAD_LAMP"].ToString(),
                            //        //STEERING_WHEEL = dr["STEERING_WHEEL"].ToString(),
                            //        //REAR_HOOD_WIRING_HARNESS = dr["REAR_HOOD_WIRING_HARNESS"].ToString(),
                            //        //SEAT = dr["SEAT"].ToString()
                            //    };
                            //GridList.Add(GR);
                                                                         
                            }
                        result.APgriddata = GridList;

                        string fromtime = ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss");
                        string totime = shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss");

                        query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='AP' AND HOOK_NO <> 9999 AND PLANT_CODE = 'T04' AND ENTRY_DATE BETWEEN to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), fromtime, totime);
                        expqty = fun.get_Col_Value(query);
                        result.lblHookDown = "HOOKDOWN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        result.lblDayTotal = GetDayTotalHookedDown(plant.Trim());


                            //expqty = fun.get_Col_Value("select count(*) from XXES_CONTROLLERS_DATA where stage='AP' and to_char(ENTRY_DATE,'dd-Mon-yyyy')=TO_CHAR(SYSDATE, 'dd-Mon-yyyy')");
                            //ViewBag.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        //}
                    }
                    catch (Exception ex)
                    {
                        result.lblError = result.lblError + "\n" + ex.Message.ToString();
                        result.lblErrorTF = true;
                    }
                    finally { fun.ConClose(); }
                    //CheckInfo(plant, family);
                }
            }
            catch (Exception ex)
            {
                result.lblError = result.lblError + "\n" + ex.Message.ToString();
                result.lblErrorTF = true;
            }
            finally { fun.ConClose(); }
            //ViewBag.lblInfoTF = result.lblInfoTF;
            //ViewBag.lblInfo = result.lblInfo;
            //ViewBag.lblInfodbTF = result.lblInfodbTF;
            //ViewBag.lblInfodb = result.lblInfodb;
            //ViewBag.lblErrorTF = result.lblErrorTF;
            //ViewBag.lblError = result.lblError;
            //ViewBag.lblErrordbTF = result.lblErrordbTF;
            //ViewBag.lblErrordb = result.lblErrordb;
            result.APgriddata = GridList;
            //return PartialView(GridList);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private APGRID2 CheckInfo(string plant, string family)
        {
            //if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            //    ViewBag.CheckURL = "Y";
            //else
            //    ViewBag.CheckURL = "N";

            APGRID2 R = new APGRID2();
            try
            {
                bool isError = false;
                query = "select remarks1 || '#' || scan_date  from XXES_LIVE_DATA where stage='AP' and DATA_TYPE='MSG' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                string data = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(data))
                {
                    R.lblInfodbTF = true;
                    R.lblInfodb = data.Split('#')[0].Trim() + "\t\tScan Date: " + data.Split('#')[1].Trim();
                }
                query = "select remarks1 || '#' || scan_date from XXES_LIVE_DATA where stage='AP' and DATA_TYPE='ERROR' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                data = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(data))
                {
                    R.lblErrordbTF = true;
                    R.lblErrordb = data.Split('#')[0].Trim() + "\t\tScan Date: " + data.Split('#')[1].Trim();
                    isError = true;
                }
                try
                {
                    if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='AP' and STATUS='ONLINE' and paramvalue='STATUS'  and description='" + plant + "'"))
                    {
                        R.imgstatus = "~\\Image\\Green.png";
                        R.imgstatusHS = "visible";
                    }
                    else if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='AP' and STATUS='OFFLINE' and paramvalue='STATUS'  and description='" + plant + "'"))
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
                    string mdate = fun.get_Col_Value("select PARAMVALUE from xxes_sft_settings where parameterinfo='AP' and STATUS='LAST_DATE'  and description='" + plant + "'");
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
            catch (Exception ex)
            {
                R.lblErrorTF = true;
                R.lblError = ex.Message.ToString();
            }
            finally { }
            return R;
        }

        public string GetDayTotalHookedDown(string plant)
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
            query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='AP' AND HOOK_NO <> 9999 AND PLANT_CODE = '{0}' AND ENTRY_DATE BETWEEN 
                                          to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), ShiftStart, ShiftEnd);
            string expqty = fun.get_Col_Value(query);
            return "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
        }
    }   
}