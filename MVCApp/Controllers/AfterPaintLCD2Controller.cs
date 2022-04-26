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
using Oracle.Web;
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
        

        public ActionResult Index()
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
            Session["AfterScreenUnit"] = Url.RequestContext.RouteData.Values["id"];
            //ViewBag.CheckURL = "Y"; 


            return View();
        }
        public PartialViewResult Grid()
        {
            string plant = "", family = "";
            List<APGRID2> GridList = new List<APGRID2>();
            MsgReturn mr = new MsgReturn();
            try
            {
                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = "";

                //if (string.IsNullOrEmpty(Convert.ToString(Session["AfterScreenUnit"])))
                //{
                //   return PartialView(GridList);
                //}

                plant = Convert.ToString(Session["AfterScreenUnit"]);
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

                    DateTime Date = fun.GetServerDateTime();

                    string Str = Convert.ToString(Date);
                    char[] Spearator = { ' ' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);

                    ViewBag.Time = "TIME: " + StrDate[1];

                    query = @"select ITEM_CODE,SUBSTR(ITEM_DESCRIPTION,0,30) as ITEM_DESCRIPTION,SHORT_CODE,FROM_ID,TO_ID ,abs((from_id-to_id))+1 QTY , ";
                    query += " (select (RTRIM (XMLAGG (XMLELEMENT (e, ltrim(rtrim(SUBSTR(a.HOOK_NO,3,2))) || ',') order by SRLNO).EXTRACT ('//text()'),',')) as HOOK from ";
                    query += "  ( ";
                    query += "  select row_number() over (order by c.entry_date) as SRLNO, ";
                    query += "  FCODE_ID,c.ITEM_CODE,c.HOOK_NO  from XXES_CONTROLLERS_DATA c,XXES_ITEM_MASTER m ";
                    query += "  where c.stage='BP' and c.ITEM_CODE=m.ITEM_CODE and c.FCODE_ID<>'0' and c.REMARKS1 is null and c.plant_code=m.plant_code and c.family_code=m.family_code and m.plant_code='" + plant + "' and m.family_code='" + family + "' ";
                    query += "  and c.JOBID in (select jobid from XXES_JOB_STATUS j where j.FINAL_LABEL_DATE is null and j.fcode_id=c.FCODE_ID and j.plant_code=c.plant_code and j.family_code=c.family_code  ) ";
                    query += "  ORDER by c.entry_date ";
                    query += "  )a ";
                    query += "  where TO_NUMBER(SRLNO,'999')>=TO_NUMBER(FROM_ID,'999') and TO_NUMBER(SRLNO,'999')<=TO_NUMBER(to_id,'999') ";
                    query += "  ) as HOOK, BATTERY,HEAD_LAMP,STEERING_WHEEL,REAR_HOOD_WIRING_HARNESS,FRONTTYRE,REARTYRE,SEAT";
                    query += "  from ";
                    query += "  ( ";
                    query += "  select min(SRLNO) from_id, max(SRLNO) to_id, ITEM_CODE,ITEM_DESCRIPTION,short_code,STEERING_WHEEL,REAR_HOOD_WIRING_HARNESS,FRONTTYRE,REARTYRE,BATTERY,HEAD_LAMP,SEAT from ";
                    query += "  ( ";
                    query += "  select SRLNO, SRLNO - row_number() over (partition by ITEM_CODE order by SRLNO) grp,FCODE_ID,ITEM_CODE,ITEM_DESCRIPTION,short_code,STEERING_WHEEL,REAR_HOOD_WIRING_HARNESS,FRONTTYRE,REARTYRE,HOOK_NO,BATTERY,HEAD_LAMP,SEAT from ";
                    query += "  (";
                    query += "  select row_number() over (order by c.entry_date) as SRLNO, ";
                    query += "  FCODE_ID,m.ITEM_CODE,ITEM_DESCRIPTION,m.short_code,STEERING_WHEEL,REAR_HOOD_WIRING_HARNESS,FRONTTYRE,REARTYRE,c.HOOK_NO,m.BATTERY,m.HEAD_LAMP, m.SEAT from XXES_CONTROLLERS_DATA c,XXES_ITEM_MASTER m ";
                    query += "  where c.stage='BP' and c.ITEM_CODE=m.ITEM_CODE and c.FCODE_ID<>'0' and c.REMARKS1 is null and c.plant_code=m.plant_code and c.family_code=m.family_code and c.plant_code='" + plant + "' and c.family_code='" + family + "'";
                    query += "  and c.JOBID in (select jobid from XXES_JOB_STATUS j where j.FINAL_LABEL_DATE is null and j.fcode_id=c.FCODE_ID and j.plant_code=c.plant_code and j.family_code=c.family_code  )  ";
                    query += "  ORDER by c.entry_date ";
                    query += "  ) ";
                    query += "  order by srlno ";
                    query += "  ) ";
                    query += "  group by ITEM_CODE,ITEM_DESCRIPTION,short_code, STEERING_WHEEL,REAR_HOOD_WIRING_HARNESS,FRONTTYRE,REARTYRE,BATTERY,HEAD_LAMP,SEAT, grp order by from_id ";
                    query += "  ) order by from_id";
                    try
                    {
                        //using (dt = returnDataTableUsingCommand("usp_getLiveData", "PROC"))
                        using (dt = fun.returnDataTable(query))
                        {
                            if (dt.Rows.Count > 0)
                            {
                                //query = @"Select 0 FCODE_ID,'NPT' ITEM_CODE,'NPT' ITEM_DESCRIPTION,'NPT' short_code,
                                //        (SELECT count(*)  FROM xxes_controllers_data where stage='BP' and FCODE_ID='0' and flag is null) QTY
                                //        ,(SELECT RTRIM (XMLAGG (XMLELEMENT (e, SUBSTR(HOOK_NO,3,2) || ',') order by HOOK_NO).EXTRACT ('//text()'),',') hook_no  FROM xxes_controllers_data where stage='BP' and FCODE_ID= '0' and flag is null) HOOK
                                //        ,'' MAKE,'' RADIATOR
                                //        from xxes_controllers_data where fcode_id='0' and flag is null 
                                //        group by FCODE_ID order by FCODE_ID";
                                //OracleDataAdapter a = new OracleDataAdapter(query, fun.ConString());
                                //a.Fill(dt);
                                if (Convert.ToString(ConfigurationSettings.AppSettings["AP_HOOK_SORT"]) == "Y")
                                    dt.DefaultView.Sort = "HOOK";

                                int COUNT = 0;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    COUNT++;
                                    APGRID2 GR = new APGRID2
                                    {
                                        SRNO = COUNT,
                                        ITEM_CODE = dr["ITEM_CODE"].ToString(),
                                        ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                                        SHORT_CODE = dr["SHORT_CODE"].ToString(),
                                        FROM_ID = dr["FROM_ID"].ToString(),
                                        TO_ID = dr["TO_ID"].ToString(),
                                        QTY = dr["QTY"].ToString(),
                                        HOOK = dr["HOOK"].ToString(),
                                        BATTERY = dr["BATTERY"].ToString(),
                                        HEAD_LAMP = dr["HEAD_LAMP"].ToString(),
                                        STEERING_WHEEL = dr["STEERING_WHEEL"].ToString(),
                                        REAR_HOOD_WIRING_HARNESS = dr["REAR_HOOD_WIRING_HARNESS"].ToString(),
                                        SEAT = dr["SEAT"].ToString()
                                    };
                                    GridList.Add(GR);
                                }                                              
                            }
                            //ViewBag.DataSource = GridList;

                            string fromtime = ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss");
                            string totime = shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss");

                            query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='AP' AND HOOK_NO <> 9999 AND PLANT_CODE = 'T04' AND ENTRY_DATE BETWEEN to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), fromtime, totime);
                            expqty = fun.get_Col_Value(query);
                            ViewBag.lblHookDown = "HOOKDOWN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                            ViewBag.lblDayTotal = GetDayTotalHookedDown(plant.Trim());


                            //expqty = fun.get_Col_Value("select count(*) from XXES_CONTROLLERS_DATA where stage='AP' and to_char(ENTRY_DATE,'dd-Mon-yyyy')=TO_CHAR(SYSDATE, 'dd-Mon-yyyy')");
                            //ViewBag.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                        }
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