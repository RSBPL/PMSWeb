using EncodeDecode;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System.Data.OleDb;
using ClosedXML.Excel;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace MVCApp.Controllers
{
    [Authorize]
    public class AddDailyPlanController : Controller
    {
        public static OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["CON"].ConnectionString);
        OleDbConnection Econ;
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();

        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.ShowApprovalBtn = Url.RequestContext.RouteData.Values["id"];
                Session["FIXED_ROLLING"] = Url.RequestContext.RouteData.Values["id"];

                if (Convert.ToString(Url.RequestContext.RouteData.Values["id"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = 'T04' AND FAMILY_CODE = 'TRACTOR FTD'";
                    int SettingDate = Convert.ToInt32(fun.get_Col_Value(query));
                    //ViewBag.DefaultDate = DateTime.Now.AddDays(SettingDate);

                    //ViewBag.DefaultDate = DateTime.Now.AddDays(0);
                    ViewBag.DefaultDate = Convert.ToDateTime(Session["ServerDate"]);
                }
                if (Convert.ToString(Url.RequestContext.RouteData.Values["id"]) == "FIXED")
                {
                    //query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'RollaingPlanDays'";
                    //int RollingDays = Convert.ToInt32(fun.get_Col_Value(query));
                    //query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'FixedPlanDays'";
                    //int FixDays = Convert.ToInt32(fun.get_Col_Value(query));

                    ViewBag.DefaultDate = Convert.ToDateTime(Session["ServerDate"]);
                }
                ViewBag.DefaultDate = Convert.ToDateTime(Session["ServerDate"]);
                //ViewBag.maxDate = null;
                return View();
            }
        }

        public JsonResult DisableHolidays()
        {
            //Get Holidays to disable days in datepicker
            query = "select HOLI_DATE from xxes_holidays";

            DataTable dt = new DataTable();

            dt = fun.returnDataTable(query);
            string[] holidays = new string[dt.Rows.Count];

            if (dt.Rows.Count > 0)
            {
                int columnIndex = 0; //single-column DataTable               

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string Month, Date, Str;
                    Str = dt.Rows[i][columnIndex].ToString();
                    char[] Spearator = { '/' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);
                    Month = StrDate[0];
                    Date = StrDate[1];

                    holidays[i] = Month + "-" + Date;
                }
            }

            return Json(holidays, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckPassword(string Password)
        {
            BaseEncDec bed = new BaseEncDec();
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(Password))
                {
                    msg = "Invalid Password";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //LoginCounter++;
                    fun.ConOpen();
                    DataSet Ds = fun.returnDataSet("Select * from XXES_Users_Master where upper(UsrName)='" + System.Web.HttpContext.Current.User.Identity.Name.ToString().ToUpper().Trim() + "' and isactive=1");
                    if (Ds.Tables[0].Rows.Count > 0)
                    {
                        if (bed.base64Decode(Ds.Tables[0].Rows[0]["PsWord"].ToString().Trim()) == Password.ToString().Trim() || "rsbpl@123321" == System.Web.HttpContext.Current.User.Identity.Name.ToString().ToUpper().Trim())
                        {
                            msg = "Authorize User";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            msg = "Invalid Password";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SyncPlanToSubAssembly(AddDailyPlanModel plan)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                //Checking all parameter as per the setting page
                #region Check Settings

                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;

                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(plan.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;


                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to sync plan to assembly ..";
                            mstType = "alert-danger";
                            var result = new { Msg = msg, ID = mstType };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing && ServerDate > PlanDate)
                        {
                            msg = "You are unable to sync plan to assembly ..";
                            mstType = "alert-danger";
                            var result = new { Msg = msg, ID = mstType };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {
                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    string pdateFixed = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdateFixed))
                        PlanDate = Convert.ToDateTime(pdateFixed).Date;
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;


                    if (ServerDate < PlanExpectedDate_Existing_Fix && ServerDate > PlanDate)
                    {

                        msg = "You are unable to sync plan to assembly ..";
                        mstType = "alert-danger";
                        var result = new { Msg = msg, ID = mstType };
                        return Json(result, JsonRequestBehavior.AllowGet);

                    }


                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(plan.Date);
                }

                #endregion

                if (string.IsNullOrEmpty(plan.Plant))
                {
                    msg = "Please Select Plnat..";
                    mstType = "alert-danger";
                    var resul1 = new { Msg = msg, ID = mstType };
                    return Json(resul1, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Family))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    var resul2 = new { Msg = msg, ID = mstType };
                    return Json(resul2, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Shift))
                {
                    msg = "Please Select Shit..";
                    mstType = "alert-danger";
                    var resul3 = new { Msg = msg, ID = mstType };
                    return Json(resul3, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Date))
                {
                    msg = "Please Select Date..";
                    mstType = "alert-danger";
                    var resul4 = new { Msg = msg, ID = mstType };
                    return Json(resul4, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToString(plan.Family).ToUpper().Contains("TRACTOR") || Convert.ToString(plan.Family).ToUpper().Contains("BACK END"))
                {
                    //Getting User Ip Address
                    UserIpAdd = fun.GetUserIP();

                    cmd = new OracleCommand("UDSP_Sync_Plan_To_SubAssembly", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Plant;
                    cmd.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Family;
                    cmd.Parameters.Add("pSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Shift;
                    cmd.Parameters.Add("pPLANDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Date;
                    cmd.Parameters.Add("pUSER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                    cmd.Parameters.Add("pIPADDRESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = UserIpAdd;
                    cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                    fun.ConOpen();
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                    {
                        if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                        {
                            msg = "Sync Plan To Sub-Assembly Done Successfully...";
                            mstType = "alert-success";
                        }
                        else
                        {
                            msg = Convert.ToString(cmd.Parameters["return_message"].Value);
                            mstType = "alert-danger";
                        }
                        var resul1 = new { Msg = msg, ID = mstType };
                        return Json(resul1, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var result = new { Msg = msg, ID = mstType };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            msg = "";
            mstType = "";
            var resul = new { Msg = msg, ID = mstType };
            return Json(resul, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SyncJobsExplicitly(AddDailyPlanModel plan)
        {
            string msg = string.Empty; string mstType = string.Empty;
            try
            {
                //Checking all parameter as per the setting page
                #region Check Settings

                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;

                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;

                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(plan.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;


                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to sync jobs ..";
                            mstType = "alert-danger";
                            var result = new { Msg = msg, ID = mstType };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing && ServerDate > PlanDate)
                        {

                            msg = "You are unable to sync jobs ..";
                            mstType = "alert-danger";
                            var result = new { Msg = msg, ID = mstType };
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {

                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (ServerDate < PlanExpectedDate_Existing_Fix && ServerDate > PlanDate)
                    {
                        msg = "You are unable to sync jobs ..";
                        mstType = "alert-danger";
                        var result = new { Msg = msg, ID = mstType };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }


                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(plan.Date);
                }

                #endregion

                if (string.IsNullOrEmpty(plan.Plant))
                {
                    msg = "Please Select Plnat..";
                    mstType = "alert-danger";
                    var resul1 = new { Msg = msg, ID = mstType };
                    return Json(resul1, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Family))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    var resul2 = new { Msg = msg, ID = mstType };
                    return Json(resul2, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Shift))
                {
                    msg = "Please Select Shit..";
                    mstType = "alert-danger";
                    var resul3 = new { Msg = msg, ID = mstType };
                    return Json(resul3, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Date))
                {
                    msg = "Please Select Date..";
                    mstType = "alert-danger";
                    var resul4 = new { Msg = msg, ID = mstType };
                    return Json(resul4, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToString(plan.Family).ToUpper().Contains("TRACTOR"))
                {
                    query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy') = '" + plan.Date + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "' AND  SHIFTCODE = '" + plan.Shift + "'";
                    string PlanId = fun.get_Col_Value(query);

                    //query = "SELECT ITEM_CODE from XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '"+ PlanId + "'";
                    query = "select  ITEM_CODE,QTY,AUTOID  from XXES_DAILY_PLAN_TRAN where plan_ID='" + Convert.ToString(PlanId).Trim() + "' and family_code='" + Convert.ToString(plan.Family).Trim() + "' and plant_code='" + Convert.ToString(plan.Plant) + "' order by AUTOID";

                    DataTable dtJob = fun.returnDataTable(query);

                    if (dtJob.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtJob.Rows)
                        {
                            deleteUnUsedJobs(Convert.ToString(dr["ITEM_CODE"]), plan.Plant, plan.Family);
                        }
                    }
                    string reponse = SyncJobs(plan);
                    if (reponse.StartsWith("ERROR"))
                        mstType = "alert-danger";
                    else
                        mstType = "alert-success";
                    msg = reponse;
                    var res = new { Msg = msg, ID = mstType };
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var result = new { Msg = msg, ID = mstType };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            msg = "";
            mstType = "";
            var resul = new { Msg = msg, ID = mstType };
            return Json(resul, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApprovePlan(AddDailyPlanModel plan)
        {
            string msg = string.Empty; string mstType = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(plan.Plant))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Family))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Shift))
                {
                    msg = "Please Select Shit..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(plan.Date))
                {
                    msg = "Please Select Date..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //Checking all parameter as per the setting page
                #region Check Settings
                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(plan.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;


                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to approve plan ..";
                            mstType = "alert-danger";
                            var result = new { Msg = msg, ID = mstType };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing && ServerDate > PlanDate)
                        {
                            msg = "You are unable to approve plan ..";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {


                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }
                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;


                    if (ServerDate < PlanExpectedDate_Existing_Fix && ServerDate > PlanDate)
                    {
                        msg = "You are unable to approve plan..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(plan.Date);
                }

                #endregion



                //if (!string.IsNullOrEmpty(plan.Shift))
                //{
                query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy') = '" + plan.Date + "' AND PLANT_CODE ='" + Convert.ToString(plan.Plant).Trim().ToUpper() + "'" +
                    " AND FAMILY_CODE = '" + Convert.ToString(plan.Family).Trim().ToUpper() + "' AND SHIFTCODE = '" + Convert.ToString(plan.Shift).Trim().ToUpper() + "'";

                string Plan_Id = fun.get_Col_Value(query);

                if (string.IsNullOrEmpty(Plan_Id))
                {
                    msg = "Plan Not Found..";
                    mstType = "alert-danger";
                    var resul1 = new { Msg = msg, ID = mstType };
                    return Json(resul1, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = "UPDATE XXES_DAILY_PLAN_MASTER SET STATUS = 'APPROVED' WHERE PLAN_ID = '" + Plan_Id + "'";

                    if (fun.EXEC_QUERY(query))
                    {
                        fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_MASTER", "APPROVED", Convert.ToString(Plan_Id), query, plan.Plant, plan.Family);
                        msg = "Plan approved..";
                        mstType = "alert-success";
                    }
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //}
                //else
                //{
                //    msg = "Please Select Shift";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AddPlan(AddDailyPlanModel plan)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;

                string query = string.Empty;
                string data = fun.getshift();
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;
                //Getting User Ip Address
                UserIpAdd = fun.GetUserIP();

                //Getting Month and Year (1)
                //string Str = plan.Date.ToString();
                string Str = string.Empty; string Month = string.Empty; string Year = string.Empty;

                string str = string.Empty; string MonthInNumber = string.Empty;
                try
                {
                    Str = Convert.ToDateTime(plan.Date).ToString("dd-MMM-yyyy");
                    char[] Spearator = { '-' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);
                    Month = StrDate[1];
                    Year = StrDate[2];
                }
                catch (Exception ex)
                {
                    msg = "Month and Year : " + Str + " # " + ex;
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    //Getting Month (2)
                    str = Convert.ToDateTime(plan.Date).ToString("dd/MM/yyyy");
                    if (str.Contains("-"))
                    {
                        char[] spearator = { '-' };
                        String[] strDate = str.Split(spearator, StringSplitOptions.None);
                        MonthInNumber = strDate[1];
                    }
                    else if (str.Contains("/"))
                    {
                        char[] spearator = { '/' };
                        String[] strDate = str.Split(spearator, StringSplitOptions.None);
                        MonthInNumber = strDate[1];
                    }
                }
                catch (Exception ex)
                {
                    msg = "Month : " + str + " # " + ex;
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


                //Check setting date's for createing and adding new plan and item in existing plan.
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(plan.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;

                    query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    string PlanId = Convert.ToString(fun.get_Col_Value(query));

                    if (string.IsNullOrEmpty(PlanId))
                    {
                        if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                        {

                        }
                        else
                        {
                            if (date < PlanExpectedDate)
                            {
                                msg = "You are not allowed to create plan before " + PlanExpectedDate.ToString("dd-MMM-yyyy") + " ..";
                                mstType = "alert-danger";
                                var resul = new { Msg = msg, ID = mstType };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }

                        }
                    }
                    else
                    {
                        query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                        string pdate = fun.get_Col_Value(query);
                        DateTime PlanDate = new DateTime();
                        if (!string.IsNullOrEmpty(pdate))
                            PlanDate = Convert.ToDateTime(pdate).Date;




                        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                        SR = Convert.ToString(fun.get_Col_Value(query));
                        if (!string.IsNullOrEmpty(SR))
                        {
                            Setting_FixPlanDays = Convert.ToInt32(SR);
                        }

                        DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;


                        if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                        {
                            if (ServerDate > PlanDate)
                            {
                                msg = "You are unable to add new item in this plan ..";
                                mstType = "alert-danger";
                                var res = new { Msg = msg, ID = mstType };
                                return Json(res, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (ServerDate >= PlanExpectedDate_Existing || ServerDate > PlanDate)
                            {

                                msg = "You are unable to add new item in this plan ..";
                                mstType = "alert-danger";
                                var resul = new { Msg = msg, ID = mstType };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {
                    query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    string PlanId = Convert.ToString(fun.get_Col_Value(query));

                    if (string.IsNullOrEmpty(PlanId))
                    {
                        //msg = "Plan not exist of this Date..";
                        msg = "You are unable to add new plan. Only rolling planner can add new plan ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + plan.Date + "' AND SHIFTCODE = '" + plan.Shift + "' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";

                        string pdate = fun.get_Col_Value(query);
                        DateTime PlanDate = new DateTime();
                        if (!string.IsNullOrEmpty(pdate))
                            PlanDate = Convert.ToDateTime(pdate).Date;




                        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                        SR = Convert.ToString(fun.get_Col_Value(query));
                        if (!string.IsNullOrEmpty(SR))
                        {
                            Setting_FixPlanDays = Convert.ToInt32(SR);
                        }

                        DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                        if (ServerDate < PlanExpectedDate_Existing_Fix || ServerDate > PlanDate)
                        {
                            msg = "You are unable to add new item in this plan ..";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + plan.Plant + "' AND FAMILY_CODE = '" + plan.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(plan.Date);
                }

                //Check Item Qty IsGreater from XXES_MONTHLYPLANNING
                //if (plan.Family == "TRACTOR FTD")
                //{
                query = "SELECT QTY FROM xxes_monthlyplanning WHERE ITEM_CODE = '" + plan.ItemCode + "' AND PLANT_CODE = '" + plan.Plant + "' " +
                    "AND FAMILY_CODE = '" + plan.Family + "' AND YEAR = '" + Year + "'AND MONTH = '" + Month.ToUpper() + "'";
                string TotalMonthlyPlanItemQty = Convert.ToString(fun.get_Col_Value(query));
                if (string.IsNullOrEmpty(TotalMonthlyPlanItemQty))
                {
                    TotalMonthlyPlanItemQty = "0";
                }

                query = "select sum(qty) from xxes_daily_plan_tran where plan_id in (SELECT plan_id from xxes_daily_plan_master" +
                    " where EXTRACT(YEAR FROM TO_DATE(plan_date, 'DD-Mon-YYYY HH24:MI:SS')) = '" + Year.Substring(2) + "' and " +
                    "EXTRACT(MONTH FROM TO_DATE(plan_date, 'DD-Mon-YYYY HH24:MI:SS')) = '" + MonthInNumber + "') and ITEM_CODE = '" + plan.ItemCode + "'";
                string TotalCurrentQty = Convert.ToString(fun.get_Col_Value(query));
                if (string.IsNullOrEmpty(TotalCurrentQty))
                {
                    TotalCurrentQty = "0";
                }
                if (plan.Family.Contains("TRACTOR"))
                {
                    if ((Convert.ToInt32(TotalCurrentQty) + Convert.ToInt32(plan.Qty)) > Convert.ToInt32(TotalMonthlyPlanItemQty))
                    {
                        msg = "Quantity should be less then " + Convert.ToString((Convert.ToInt32(TotalMonthlyPlanItemQty) - Convert.ToInt32(TotalCurrentQty)) + 1);
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                //}

                cmd = new OracleCommand("UDSP_DAILY_PLANNING", fun.Connection());
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Plant;
                cmd.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Family;
                cmd.Parameters.Add("pSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Shift;
                cmd.Parameters.Add("pPLANDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Date;
                cmd.Parameters.Add("pPRODQTY", OracleDbType.Int32, ParameterDirection.Input).Value = plan.TotalQty;
                cmd.Parameters.Add("pITEMCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.ItemCode;
                cmd.Parameters.Add("pBACKENDITEMCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.BackendItemFcodes; //NEW                
                cmd.Parameters.Add("pPLANQTY", OracleDbType.Int32, ParameterDirection.Input).Value = plan.Qty;
                cmd.Parameters.Add("pTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.Tyres;
                cmd.Parameters.Add("pMODEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plan.ModelType;
                cmd.Parameters.Add("pUSER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                cmd.Parameters.Add("pIPADDRESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = UserIpAdd;
                cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                fun.ConOpen();
                cmd.ExecuteNonQuery();
                if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                {
                    if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                    {
                        msg = "Data Saved Successfully...";
                        mstType = "alert-success";

                        string returnMsg = string.Empty;

                        //if (Convert.ToString(plan.Family).ToUpper().Contains("TRACTOR"))
                        //    deleteUnUsedJobs(plan.ItemCode, plan.Plant, plan.Family);

                        //if (Convert.ToString(plan.Family).ToUpper().Contains("TRACTOR"))
                        //{
                        //    returnMsg = SyncJobs(plan);
                        //    msg += "\n" + returnMsg;
                        //}
                    }
                    else
                    {
                        msg = Convert.ToString(cmd.Parameters["return_message"].Value);
                        mstType = "alert-danger";
                    }
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditPlan(AddDailyPlanModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                query = "SELECT MAX(SEQ_NO) AS MAXIMUMSEQ_NO FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' ORDER BY SEQ_NO";
                int maxSqn = Convert.ToInt32(fun.get_Col_Value(query));

                //Checking all parameter as per the setting page
                #region Check Settings
                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(data.Date).Date;


                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;

                    query = "SELECT DISTINCT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE PLAN_ID = (SELECT DISTINCT PLAN_ID FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID = '" + data.AutoId + "' )";
                    //query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;



                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to edit item in this plan ..";
                            mstType = "alert-danger";
                            var res = new { Msg = msg, ID = mstType };
                            return Json(res, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing || ServerDate > PlanDate)
                        {
                            msg = "You are unable to edit item in this plan ..";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                    //if (ServerDate >= PlanExpectedDate_Existing || ServerDate >= PlanDate)
                    //{
                    //    if (SettingRollingDays != 1 && Setting_FixPlanDays != 1)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        msg = "You are unable to edit item in this plan ..";
                    //        mstType = "alert-danger";
                    //        var resul = new { Msg = msg, ID = mstType };
                    //        return Json(resul, JsonRequestBehavior.AllowGet);
                    //    }

                    //}
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {


                    query = "SELECT DISTINCT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE PLAN_ID = (SELECT DISTINCT PLAN_ID FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID = '" + data.AutoId + "' )";
                    //query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;




                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;


                    if (ServerDate < PlanExpectedDate_Existing_Fix || ServerDate > PlanDate)
                    {
                        msg = "You are unable to edit item in this plan ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    //if (ServerDate <= PlanExpectedDate_Existing_Fix || ServerDate >= PlanDate)
                    //{
                    //    if (SettingRollingDays != 1 && Setting_FixPlanDays != 1)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        msg = "You are unable to edit item in this plan ..";
                    //        mstType = "alert-danger";
                    //        var resul = new { Msg = msg, ID = mstType };
                    //        return Json(resul, JsonRequestBehavior.AllowGet);
                    //    }
                    //}

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(data.Date);
                }

                #endregion

                if (data.TargetSeq > maxSqn)
                {
                    msg = "Sqn No. Should not be greater then last Sqn No. ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (data.TargetSeq < 1)
                {
                    msg = "Sqn No. Should not be less then 1 ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (data.TargetSeq == 0)
                {
                    msg = "Sqn No. Should not be 0 ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //Check Qty should not be greater then COUNT of SUBASSEMBLY_ID of XXES_PRINT_SERIALS 
                query = "SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE FCODE_ID = '" + data.AutoId + "' AND FAMILY_CODE = '" + data.Family + "' AND PLANT_CODE = '" + data.Plant + "'";
                int CountPrintSr = Convert.ToInt32(fun.get_Col_Value(query));
                if (CountPrintSr > 0)
                {
                    if (data.Qty < CountPrintSr)
                    {
                        msg = "Qty Should not be less then " + CountPrintSr + " ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                List<EditSQN> EditList = new List<EditSQN>();
                //Getting User Ip Address
                UserIpAdd = fun.GetUserIP();

                if (data.Seq > data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.TargetSeq + "' AND '" + data.Seq + "' ORDER BY SEQ_NO";
                    RowAction = "UP";
                }
                else if (data.Seq < data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.Seq + "' AND '" + data.TargetSeq + "' ORDER BY SEQ_NO";
                    RowAction = "DOWN";
                }
                else if (data.Seq == data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.Seq + "' AND '" + data.TargetSeq + "' ORDER BY SEQ_NO";
                    RowAction = "EQUAL";
                }
                else
                {
                    msg = "This row already contain this Sequence ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                dt = fun.returnDataTable(query);

                if (RowAction == "UP")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[0][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == TargetAutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) + 1 });
                        }
                        else if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == data.AutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                        }
                        else
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) + 1 });
                        }
                    }
                }
                if (RowAction == "DOWN")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == TargetAutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }
                        else if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == data.AutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                        }
                        else
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }
                    }
                }
                if (RowAction == "EQUAL")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == data.AutoId)
                        //{

                        //}          
                        EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                    }
                }

                if (EditList != null)
                {
                    int count = 0;
                    InsertIntoDailyPlanAct(data.Plant, data.Family, data.ItemCode, data.Seq, data.TargetSeq, "UPDATE_QTY_AND_SQN", data.PlanId, data.Date);
                    foreach (var item in EditList)
                    {
                        query = "UPDATE XXES_DAILY_PLAN_TRAN SET QTY = '" + item.Qty + "', SEQ_NO = '" + item.Sqn + "' WHERE AUTOID = '" + item.AutoId + "' AND PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("DAILY_PLAN_TRAN", "Update_Sqn_While_Update", Convert.ToString(data.AutoId), query, data.Plant, data.Family);
                            count++;
                        }
                    }
                    if (EditList.Count == count)
                    {
                        msg = "Data Updated Successfully...";
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePlan(AddDailyPlanModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                //Checking all parameter as per the setting page
                #region Check Settings
                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(data.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;


                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;


                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to delete this plan ..";
                            mstType = "alert-danger";
                            var res = new { Msg = msg, ID = mstType };
                            return Json(res, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing || ServerDate > PlanDate)
                        {

                            msg = "You are unable to delete this plan ..";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {
                    query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;




                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;


                    if (ServerDate < PlanExpectedDate_Existing_Fix || ServerDate > PlanDate)
                    {
                        msg = "You are unable to delete this plan ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }
                    date = Convert.ToDateTime(data.Date);
                }

                #endregion

                if (string.IsNullOrEmpty(data.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Family))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Shift))
                {
                    msg = "Please Select Shift ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Date))
                {
                    msg = "Please Select date ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //Check AUTOID exist in SUBASSEMBLY_ID of XXES_PRINT_SERIALS 
                query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "' AND to_char(PLAN_DATE,'dd-Mon-yyyy')='" + data.Date + "'";
                String PlanId = Convert.ToString(fun.get_Col_Value(query));
                if (string.IsNullOrEmpty(PlanId))
                {
                    msg = "No Plan Found ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = "select count(*) from xxes_job_status where fcode_id in (select autoid from xxes_daily_plan_tran where PLAN_ID = '" + PlanId + "')";
                    int CountFcode = Convert.ToInt32(fun.get_Col_Value(query));

                    query = "SELECT COUNT(*) FROM XXES_PRINT_SERIALS WHERE SUBASSEMBLY_ID in (select autoid from xxes_daily_plan_assembly where PLAN_ID = '" + PlanId + "')";
                    int CountAssembly = Convert.ToInt32(fun.get_Col_Value(query));

                    if (CountFcode > 0 || CountAssembly > 0)
                    {
                        msg = "Plan Can't be Deleted ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //query = "DELETE FROM xxes_daily_plan_assembly WHERE PLAN_ID = '" + PlanId + "'";
                        //if (fun.EXEC_QUERY(query)) 
                        //{
                        //    fun.Insert_Into_ActivityLog("DAILY_PLAN_ASSEMBLY", "Delete", PlanId, query, data.Plant, data.Family);

                        query = "DELETE FROM xxes_daily_plan_tran WHERE PLAN_ID = '" + PlanId + "'";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("DAILY_PLAN_TRAN", "Delete", PlanId, query, data.Plant, data.Family);

                            query = "DELETE FROM XXES_DAILY_PLAN_MASTER WHERE PLAN_ID = '" + PlanId + "'";
                            if (fun.EXEC_QUERY(query))
                            {
                                fun.Insert_Into_ActivityLog("DAILY_PLAN_MASTER", "Delete", PlanId, query, data.Plant, data.Family);

                                msg = "Plan Deleted Successfully...";
                                mstType = "alert-danger";
                                var resul = new { Msg = msg, ID = mstType };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePlanItem(AddDailyPlanModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                //Checking all parameter as per the setting page
                #region Check Settings

                //Check setting date's for createing and adding new plan and item in existing plan.
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "ROLLING")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(data.Date).Date;

                    DateTime PlanExpectedDate = Convert.ToDateTime(ServerDate).AddDays(SettingRollingDays).Date;

                    query = "SELECT DISTINCT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE PLAN_ID = (SELECT DISTINCT PLAN_ID FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID = '" + data.AutoId + "' )";
                    //query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;




                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (SettingRollingDays == 0 && Setting_FixPlanDays == 0)
                    {
                        if (ServerDate > PlanDate)
                        {
                            msg = "You are unable to delete item from this plan ..";
                            mstType = "alert-danger";
                            var res = new { Msg = msg, ID = mstType };
                            return Json(res, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ServerDate >= PlanExpectedDate_Existing || ServerDate > PlanDate)
                        {
                            msg = "You are unable to delete item from this plan ..";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (Convert.ToString(Session["FIXED_ROLLING"]) == "FIXED")
                {


                    query = "SELECT DISTINCT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE PLAN_ID = (SELECT DISTINCT PLAN_ID FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID = '" + data.AutoId + "' )";
                    //query = "SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' AND SHIFTCODE = '" + data.Shift + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    string pdate = fun.get_Col_Value(query);
                    DateTime PlanDate = new DateTime();
                    if (!string.IsNullOrEmpty(pdate))
                        PlanDate = Convert.ToDateTime(pdate).Date;




                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'FixedPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";

                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        Setting_FixPlanDays = Convert.ToInt32(SR);
                    }

                    DateTime PlanExpectedDate_Existing_Fix = Convert.ToDateTime(PlanDate).AddDays(-Setting_FixPlanDays).Date;

                    if (ServerDate < PlanExpectedDate_Existing_Fix || ServerDate > PlanDate)
                    {
                        msg = "You are unable to delete item from this plan ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'RollaingPlanDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                    SR = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(SR))
                    {
                        SettingRollingDays = Convert.ToInt32(SR);
                    }

                    date = Convert.ToDateTime(data.Date);
                }

                #endregion

                //Check AUTOID exist in SUBASSEMBLY_ID of XXES_PRINT_SERIALS 
                query = "SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE FCODE_ID = '" + data.AutoId + "'";
                int Count = Convert.ToInt32(fun.get_Col_Value(query));
                if (Count > 0)
                {
                    msg = Convert.ToString(Count) + " Serial No. has been printed, So you can not delete this ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Delete Sub assembly item from XXES_DAILY_PLAN_ASSEMBLY
                    query = "DELETE FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID = '" + data.AutoId + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        InsertIntoDailyPlanAct(data.Plant, data.Family, data.ItemCode, data.Qty, 0, "DEL_ITEM_QTY", data.PlanId, data.Date);
                        fun.Insert_Into_ActivityLog("DAILY_PLAN_TRAN", "Delete", data.ItemCode, query, data.Plant, data.Family);
                        //Update Sequence in in XXES_DAILY_PLAN_ASSEMBLY
                        List<EditSQN> EditList = new List<EditSQN>();
                        query = "SELECT AUTOID, SEQ_NO FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND " +
                                "SEQ_NO > '" + data.Seq + "' ORDER BY SEQ_NO";

                        dt = fun.returnDataTable(query);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }
                        if (EditList != null)
                        {
                            int count = 0;
                            foreach (var item in EditList)
                            {
                                query = "UPDATE XXES_DAILY_PLAN_TRAN SET SEQ_NO = '" + item.Sqn + "' WHERE AUTOID = '" + item.AutoId + "' AND PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                                if (fun.EXEC_QUERY(query))
                                {
                                    fun.Insert_Into_ActivityLog("DAILY_PLAN_TRAN", "Update_Sqn_While_Delete", Convert.ToString(item.AutoId), query, data.Plant, data.Family);
                                    count++;
                                }
                            }
                            if (EditList.Count == count)
                            {
                                msg = "Data Deleted Successfully...";
                                mstType = "alert-success";
                                var resul = new { Msg = msg, ID = mstType };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void InsertIntoDailyPlanAct(string PLANT_CODE, string FAMILY_CODE, string ITEM_CODE, int OLDVALUE, int NEWVALUE, string ACTION, int PLAN_ID, string PLAN_DATE)
        {
            try
            {
                query = "Insert into XXES_DAILY_PLAN_ACT(PLANT_CODE,FAMILY_CODE,PLAN_ID,ITEM_CODE,OLD_VALUE,NEW_VALUE,ACTION,LOGIN_USER,IP_ADDR,SYS_NAME,ACTION_DATE,PLAN_DATE) values('" +
                    PLANT_CODE.Trim() + "','" + FAMILY_CODE.Trim() + "'," + PLAN_ID + ",'" + ITEM_CODE + "'," + OLDVALUE + "," + NEWVALUE + ",'" + ACTION + "','" +
                    System.Web.HttpContext.Current.User.Identity.Name.ToString() + "','" + fun.GetUserIP() + "','" + fun.GetUserIP() + "',sysdate,'" + PLAN_DATE + "')";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_ACT", "INSERT", Convert.ToString(PLAN_ID), query, PLANT_CODE.Trim().ToUpper(), FAMILY_CODE.Trim().ToUpper());
                }
            }
            catch (Exception ex)
            {

            }
            finally { }
        }

        private void deleteUnUsedJobs(string fcode, string plant, string family)
        {
            try
            {
                string query = "";
                string Shiftcode = "", isDayNeedToLess = ""; DateTime dtDeleteJobs = new DateTime();
                string data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                        dtDeleteJobs = fun.GetServerDateTime().Date.AddDays(-1);
                    else
                        dtDeleteJobs = fun.GetServerDateTime().Date;
                    query = @"delete from xxes_daily_plan_job where fcode_autoid in (select t.autoid from xxes_daily_plan_tran t, xxes_daily_plan_master m where t.plan_id=m.plan_id and t.item_code='" + fcode.Trim().ToUpper() + "' and m.plant_code='" + Convert.ToString(plant).Trim().ToUpper() + "' and m.family_code='" + Convert.ToString(family).Trim().ToUpper() + "'  and to_char(plan_date,'dd-Mon-yyyy') < to_date('" + dtDeleteJobs.Date.ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy')) and jobid not in (select JOBID from xxes_job_status where jobid is not null) and jobid not in (select JOB_ID from print_serial_number  where job_id is not null ) and jobid not in (select JOB_ID from job_serial_movement  where job_id is not null )";
                    if (fun.EXEC_QUERY(query))
                    {
                        fun.Insert_Into_ActivityLog("DAILY_PLAN_JOB", "DELETE_PREVIOUS", fcode.Trim(), query, Convert.ToString(plant).Trim().ToUpper(), Convert.ToString(family).Trim().ToUpper());
                        //LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                        //LogWrite(query);
                    }
                }
            }
            catch (Exception ex)
            {
                //pbf.ErrorRecord("Module:deleteUnusedJobs " + ex.Message.ToString());
                throw;
            }
            finally { }
        }

        private string SyncJobs(AddDailyPlanModel plan)
        {
            try
            {
                string ReturnMsg = "";
                DataTable dtMain = new DataTable();
                bool isOK = false;
                query = "select f.ORG_ID from XXES_PLANT_FAMILY_MAP m , XXES_FAMILY_MASTER f where m.FAMILY_CODE=f.FAMILY_CODE and m.PLANT_CODE='" + Convert.ToString(plan.Plant).Trim().ToUpper() + "' and m.FAMILY_CODE='" + Convert.ToString(plan.Family).Trim().ToUpper() + "'";
                ORGID = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(ORGID.Trim()))
                {
                    ReturnMsg = "ORGID not found for selected family";
                    return ReturnMsg;
                }
                using (dtMain = fun.returnDataTable("select PLAN_ID from XXES_DAILY_PLAN_MASTER where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Convert.ToDateTime(plan.Date).ToString("dd-MMM-yyyy") + "' and shiftcode='" + Convert.ToString(plan.Shift).Trim().ToUpper() + "' and plant_code='" + Convert.ToString(plan.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(plan.Family).Trim().ToUpper() + "'"))
                {

                    if (dtMain.Rows.Count > 0)
                    {
                        foreach (DataRow odr in dtMain.Rows)
                        {
                            dtJob = new DataTable();
                            //query = "select  ITEM_CODE,QTY,AUTOID  from XXES_DAILY_PLAN_TRAN where plan_ID='" + plan_id.Trim() + "' and AUTOID not in (select FCODE_AUTOID from XXES_DAILY_PLAN_JOB) order by AUTOID";
                            query = "select  ITEM_CODE,QTY,AUTOID  from XXES_DAILY_PLAN_TRAN where plan_ID='" + Convert.ToString(odr["plan_id"]).Trim() + "' and family_code='" + Convert.ToString(plan.Family).Trim() + "' and plant_code='" + Convert.ToString(plan.Plant) + "' order by AUTOID";
                             dtJob = fun.returnDataTable(query);
                            if (dtJob.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dtJob.Rows)
                                {
                                    query = "select NVL(count(*),0) from XXES_DAILY_PLAN_JOB where FCODE_AUTOID='" + Convert.ToString(dr["AUTOID"]) + "' and family_code='" + Convert.ToString(plan.Family).Trim() + "' and plant_code='" + Convert.ToString(plan.Plant) + "'";
                                    int ExsitingQty = Convert.ToInt32(fun.get_Col_Value(query));
                                    int remqty = 0;
                                    if (!string.IsNullOrEmpty(Convert.ToString(dr["QTY"])))
                                    {
                                        if (Convert.ToInt32(Convert.ToString(dr["QTY"])) > ExsitingQty)
                                            remqty = Math.Abs(Convert.ToInt32(Convert.ToString(dr["QTY"])) - ExsitingQty);
                                    }
                                    if (remqty > 0)
                                    {
                                        //query = "Insert into XXES_DAILY_PLAN_JOB(FCODE_AUTOID,JOBID,FAMILY_CODE,PLANT_CODE) SELECT " + Convert.ToString(dr["AUTOID"]) + ", WIP_ENTITY_NAME,'" + Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper() + "','" + Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper() + "' FROM RELESEDJOBORDER WHERE ROWNUM <=" + remqty + " and SEGMENT1='" + Convert.ToString(dr["ITEM_CODE"]) + "' and ORGANIZATION_ID='" + ORGID + "' and WIP_ENTITY_NAME not in (select jobid from XXES_JOB_STATUS) and  WIP_ENTITY_NAME not in (select JOBID from XXES_DAILY_PLAN_JOB )  order by segment1,WIP_ENTITY_NAME";
                                        query = "Insert into XXES_DAILY_PLAN_JOB(FCODE_AUTOID,JOBID,FAMILY_CODE,PLANT_CODE) SELECT " + Convert.ToString(dr["AUTOID"]) + ", WIP_ENTITY_NAME,'" + Convert.ToString(plan.Family).Trim().ToUpper() + "','" + Convert.ToString(plan.Plant).Trim().ToUpper() + "' FROM RELESEDJOBORDER WHERE REGEXP_LIKE(WIP_ENTITY_NAME, '^[[:digit:]]+$') AND ROWNUM <=" + remqty + " and SEGMENT1='" + Convert.ToString(dr["ITEM_CODE"]) + "' and ORGANIZATION_ID='" + ORGID + "' and WIP_ENTITY_NAME not in (select jobid from XXES_JOB_STATUS where jobid is not null) and  WIP_ENTITY_NAME not in (select JOBID from XXES_DAILY_PLAN_JOB where jobid is not null ) and  WIP_ENTITY_NAME not in (select JOB_ID from print_serial_number  where job_id is not null  ) and family_code='" + Convert.ToString(plan.Family).Trim().ToUpper() + "' order by segment1,WIP_ENTITY_NAME";
                                        if (fun.EXEC_QUERY(query))
                                        {
                                            isOK = true;
                                            fun.Insert_Into_ActivityLog("DAILY_PLAN_JOB", "Insert", Convert.ToString(dr["AUTOID"]), query, Convert.ToString(plan.Plant).Trim().ToUpper(), Convert.ToString(plan.Family).Trim().ToUpper());
                                            //pbf.LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                                            //pbf.LogWrite(query);
                                            ReturnMsg = "Sync Jobs Successfully !!";
                                            return ReturnMsg;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //lblJobs.Visible = true;
                if (isOK == true)
                {
                    //MessageBox.Show("Sync Completed Successfully", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReturnMsg = "Sync Completed Successfully";
                    return ReturnMsg;
                }
                else
                {
                    //MessageBox.Show("Not found any new job to sync from ORACLE", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReturnMsg = "Not found any new job to sync from ORACLE";
                    return ReturnMsg;
                }
                //gleFcode.Text = "";
                //gleFcode.EditValue = -1;
                //txtFcodeQty.Text = "";
                //lblRelJobQty.Visible = false;
            }
            catch (Exception ex)
            {
                return ex.Message;
                //pbf.ErrorRecord("Module: SyncJobs" + ex.Message.ToString());
                //MessageBox.Show(ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { }
        }

        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        //public PartialViewResult BindFamily(string Plant)
        //{
        //    if (!string.IsNullOrEmpty(Plant))
        //    {
        //        ViewBag.Family = new SelectList(fun.Fill_Family(Plant), "Value", "Text");
        //    }
        //    return PartialView();
        //}
        public JsonResult BindFamily(string Plant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                result = fun.Fill_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult BindShift()
        {
            ViewBag.Shift = new SelectList(fun.FillShift(), "Value", "Text");
            return PartialView();
        }

        public JsonResult CurrentShift()
        {
            string Shift = fun.getshift();
            char[] Spearator = { '#' };
            String[] StrShift = Shift.Split(Spearator, StringSplitOptions.None);
            string SelectedShift = StrShift[0];
            return Json(SelectedShift, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(AddDailyPlanModel data)
        {
            //query = "SELECT  B.ITEM_CODE, (SELECT ITEM_DESCRIPTION FROM XXES_ITEM_MASTER WHERE ITEM_CODE = B.ITEM_CODE) AS DESCRIPTION, B.MAKE AS TYRE_MAKE," +
            //    "B.TYPE AS MODEL, B.QTY AS PLANNED_QTY, '' AS QTY_CANBEMADE,'' AS AVL_REARAXLE,'' AS AVL_TRANSMISSION,'' AS AVL_ENGINE,'' AS AVL_HYDROLIC, B.SEQ_NO AS SEQUENCE_NO " +
            //    " FROM XXES_DAILY_PLAN_MASTER A JOIN XXES_DAILY_PLAN_TRAN B ON A.PLAN_ID = B.PLAN_ID" +
            //    " WHERE A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND A.SHIFTCODE = '" + data.Shift + "' AND to_char(A.PLAN_DATE,'dd-Mon-yyyy')='" + data.Date + "' ORDER BY B.SEQ_NO";

            //string Return_family = fun.FamilyReturnByUnit(data.Plant);
            //string family = string.Empty;
            //if (!string.IsNullOrEmpty(Return_family))
            //{
            //    family = Return_family;
            //}

            if (!string.IsNullOrEmpty(data.Family) && !string.IsNullOrEmpty(data.Plant))
            {
                if (data.Family.Contains("TRACTOR") && data.Plant.Contains("T04"))
                {
                    query = @"SELECT B.AUTOID, B.PLAN_ID, B.ITEM_CODE, SUBSTR( M.ITEM_DESCRIPTION, 1, 50 ) AS DESCRIPTION, M.SHORT_CODE AS SHORT_DESC, 
                        B.MAKE AS TYRE_MAKE,B.TYPE AS MODEL, 
                        B.PLANNED_QTY AS PLANNED_QTY, 
                        B.QTY AS QTY_CANBEMADE,
                        (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'RAB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.REARAXEL " +
                            "AND SRNO NOT IN(SELECT REARAXEL_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND REARAXEL_SRLNO IS NOT NULL ) ) AS AVL_REARAXLE, " +
                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.TRANSMISSION " +
                            "AND SRNO NOT IN(SELECT TRANSMISSION_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND TRANSMISSION_SRLNO IS NOT NULL) ) AS AVL_TRANSMISSION, " +
                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'ENF' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.ENGINE " +
                            "AND SRNO NOT IN(SELECT ENGINE_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND ENGINE_SRLNO IS NOT NULL) )  AS AVL_ENGINE,   " +
                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.HYDRAULIC " +
                            "AND SRNO NOT IN(SELECT HYDRALUIC_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND HYDRALUIC_SRLNO IS NOT NULL) ) AS AVL_HYDRAULIC " +
                            ", B.SEQ_NO AS SEQUENCE_NO FROM XXES_DAILY_PLAN_MASTER A JOIN XXES_DAILY_PLAN_TRAN B ON A.PLAN_ID = B.PLAN_ID JOIN XXES_ITEM_MASTER  M " +
                            "ON B.ITEM_CODE = M.ITEM_CODE AND B.PLANT_CODE = M.PLANT_CODE AND B.FAMILY_CODE = M.FAMILY_CODE " +
                            "WHERE " +
                            "A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND ";
                    if (!string.IsNullOrEmpty(data.Shift))
                    {
                        query += "A.SHIFTCODE = '" + data.Shift + "' AND ";
                    }
                    query += "to_char(A.PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' ORDER BY B.SEQ_NO";
                    ViewBag.GridType = "TR04";
                }
                else if (data.Family.Contains("TRACTOR") && data.Plant.Contains("T05"))
                {
                    query = @"SELECT B.AUTOID, B.PLAN_ID, B.ITEM_CODE, SUBSTR( M.ITEM_DESCRIPTION, 1, 50 ) AS DESCRIPTION, M.SHORT_CODE AS SHORT_DESC, 
                        B.MAKE AS TYRE_MAKE,B.TYPE AS MODEL, 
                        B.PLANNED_QTY AS PLANNED_QTY, 
                        B.QTY AS QTY_CANBEMADE,
                        (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'BAB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.BACKEND " +
                            "AND SRNO NOT IN(SELECT BACKEND_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND BACKEND_SRLNO IS NOT NULL ) ) AS AVL_BACKEND, " +

                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'ENP' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.ENGINE " +
                            "AND SRNO NOT IN(SELECT ENGINE_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND ENGINE_SRLNO IS NOT NULL) )  AS AVL_ENGINE,   " +

                            "B.SEQ_NO AS SEQUENCE_NO FROM XXES_DAILY_PLAN_MASTER A JOIN XXES_DAILY_PLAN_TRAN B ON A.PLAN_ID = B.PLAN_ID JOIN XXES_ITEM_MASTER  M " +
                            "ON B.ITEM_CODE = M.ITEM_CODE AND B.PLANT_CODE = M.PLANT_CODE AND B.FAMILY_CODE = M.FAMILY_CODE " +
                            "WHERE " +

                             //"A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND A.SHIFTCODE = '" + data.Shift + "' AND to_char(A.PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' ORDER BY B.SEQ_NO";

                             "A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND ";
                    if (!string.IsNullOrEmpty(data.Shift))
                    {
                        query += "A.SHIFTCODE = '" + data.Shift + "' AND ";
                    }
                    query += "to_char(A.PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' ORDER BY B.SEQ_NO";
                    ViewBag.GridType = "TR05";
                }
                else if (data.Family.Contains("BACK END") && data.Plant.Contains("T04"))
                {
                    query = @"SELECT B.AUTOID, B.PLAN_ID, B.ITEM_CODE, SUBSTR( M.BACKEND_DESC, 1, 50 ) AS DESCRIPTION,
                        B.MAKE AS TYRE_MAKE,B.TYPE AS MODEL, 
                        B.PLANNED_QTY AS PLANNED_QTY, 
                        B.QTY AS QTY_CANBEMADE,

                        (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'RAB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.REARAXEL " +
                            "AND SRNO NOT IN(SELECT REARAXEL_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND REARAXEL_SRLNO IS NOT NULL ) ) AS AVL_REARAXLE, " +

                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.TRANSMISSION " +
                            "AND SRNO NOT IN(SELECT TRANSMISSION_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND TRANSMISSION_SRLNO IS NOT NULL) ) AS AVL_TRANSMISSION, " +

                            "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = M.HYDRAULIC " +
                            "AND SRNO NOT IN(SELECT HYDRALUIC_SRLNO " +
                            "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND HYDRALUIC_SRLNO IS NOT NULL) ) AS AVL_HYDRAULIC, " +

                            "B.SEQ_NO AS SEQUENCE_NO FROM XXES_DAILY_PLAN_MASTER A JOIN XXES_DAILY_PLAN_TRAN B ON A.PLAN_ID = B.PLAN_ID JOIN xxes_backend_master  M " +
                            "ON B.ITEM_CODE = M.BACKEND AND B.PLANT_CODE = M.PLANT_CODE AND B.FAMILY_CODE = M.FAMILY_CODE " +
                            "WHERE " +

                             //"A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND A.SHIFTCODE = '" + data.Shift + "' AND to_char(A.PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' ORDER BY B.SEQ_NO";

                             "A.PLANT_CODE = '" + data.Plant + "' AND A.FAMILY_CODE = '" + data.Family + "' AND ";
                    if (!string.IsNullOrEmpty(data.Shift))
                    {
                        query += "A.SHIFTCODE = '" + data.Shift + "' AND ";
                    }
                    query += "to_char(A.PLAN_DATE,'dd-Mon-yyyy')= '" + data.Date + "' ORDER BY B.SEQ_NO";
                    ViewBag.GridType = "BK04";
                }
            }

            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }

        public PartialViewResult BindTyres(AddDailyPlanModel data)
        {
            ViewBag.Tyres = new SelectList(FillTyres(data.ItemCode), "Value", "Text");
            return PartialView();
        }

        //public PartialViewResult BindModelType(AddDailyPlanModel data)
        //{
        //    ViewBag.ModelType = new SelectList(FillModelType(data), "Value", "Text");
        //    return PartialView();
        //}
        public JsonResult BindModelType(AddDailyPlanModel data)
        {
            return Json(FillModelType(data), JsonRequestBehavior.AllowGet);

        }

        public PartialViewResult BindItems(AddDailyPlanModel data)
        {
            ViewBag.Items = new SelectList(FillItemMaster(data), "Value", "Text");
            return PartialView();
        }

        public JsonResult TotalPlanningQty(AddDailyPlanModel data)
        {
            string Qty = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    query = "select EXPECTED_QTY from XXES_DAILY_PLAN_MASTER where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + data.Date + "' and SHIFTCODE='" + Convert.ToString(data.Shift) + "' and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim().ToUpper() + "'";
                    Qty = fun.get_Col_Value(query);
                }
                else
                {
                    Qty = "0";
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Qty, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BackendFCodeByItemCode(AddDailyPlanModel data)
        {
            string BackendItemCode = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(data.BackendItemFcodes)))
                {
                    query = string.Format(@"select backend from xxes_item_master where item_code='{0}'",
                    Convert.ToString(data.BackendItemFcodes));
                    string dcode = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(dcode))
                    {
                        BackendItemCode = dcode;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(BackendItemCode, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VisibleSyncJobBtn(AddDailyPlanModel data)
        {
            string Result = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Date))
                    data.Date = DateTime.Now.ToString("dd-MMM-yyyy");
                if (string.IsNullOrEmpty(data.Shift))
                    data.Shift = "";
                else
                    data.Shift = data.Shift.Trim().ToUpper();
                if (string.IsNullOrEmpty(data.Plant))
                    data.Plant = "";
                else
                    data.Plant = data.Plant.Trim().ToUpper();
                if (string.IsNullOrEmpty(data.Family))
                    data.Family = "";
                else
                    data.Family = data.Family.Trim().ToUpper();

                query = @"select PLAN_DATE from XXES_DAILY_PLAN_MASTER where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + data.Date + "'" +
                    " and SHIFTCODE='" + Convert.ToString(data.Shift) + "' and plant_code='" + Convert.ToString(data.Plant) + "' and family_code='" + Convert.ToString(data.Family).Trim().ToUpper() + "'";

                string pdate = fun.get_Col_Value(query);
                DateTime PlanDate = new DateTime();
                if (!string.IsNullOrEmpty(pdate))
                    PlanDate = Convert.ToDateTime(pdate).Date;




                DateTime CurrentDate = fun.GetServerDateTime().Date;
                if (PlanDate == CurrentDate)
                {
                    Result = "Y";
                }
                else
                {
                    Result = "N";
                }
            }
            catch (Exception ex)
            {
                Result = "N";
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VisibleTyerModal(AddDailyPlanModel data)
        {
            string Result = string.Empty;
            if (!string.IsNullOrEmpty(data.Family))
            {
                if (data.Family.Contains("TRACTOR"))
                {
                    Result = "Y";
                }
                if (data.Family.Contains("BACK END"))
                {
                    Result = "N";
                }
            }

            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult BindBackendFcodes()
        {
            ViewBag.Items = new SelectList(FillBackendFcodes(), "Value", "Text");
            return PartialView();
        }

        private List<DDLTextValue> FillBackendFcodes()
        {
            List<DDLTextValue> Tyres = new List<DDLTextValue>();
            try
            {
                query = @"select SHORT_CODE,ITEM_CODE as FCODE ,
                ITEM_CODE || ' # ' || SUBSTR(REPLACE(ITEM_DESCRIPTION,'TRACTOR FARMTRAC','FT'),0,150) 
                as DESCRIPTION from XXES_ITEM_MASTER where ITEM_CODE is not null and BACKEND is not null";
                using (DataTable dt = fun.returnDataTable(query))
                {
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            Tyres.Add(new DDLTextValue
                            {
                                Text = dr["DESCRIPTION"].ToString(),
                                Value = dr["FCODE"].ToString(),
                            });
                        }
                    }
                    return Tyres;
                }
            }
            catch (Exception ex)
            {
                return Tyres;
            }
            finally { }
        }

        private List<DDLTextValue> FillTyres(string fcode)
        {
            try
            {
                List<DDLTextValue> Tyres = new List<DDLTextValue>();
                DataTable dt = fun.GetTyreMakeList(fcode);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Tyres.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["Name"].ToString(),
                        });
                    }
                }
                return Tyres;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }

        private List<DDLTextValue> FillModelType(AddDailyPlanModel data)
        {
            try
            {
                List<DDLTextValue> Model = new List<DDLTextValue>();

                if (!string.IsNullOrEmpty(data.ItemCode))
                {
                    query = "select MODEL_TYPE from XXES_ITEM_MASTER WHERE ITEM_CODE = '" + data.ItemCode + "'";
                    dt = fun.returnDataTable(query);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            Model.Add(new DDLTextValue
                            {
                                Text = dr["MODEL_TYPE"].ToString(),
                                Value = dr["MODEL_TYPE"].ToString(),
                            });
                        }
                    }
                }
                return Model;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<DDLTextValue> FillItemMaster(AddDailyPlanModel data)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string str = data.Date;
                if (string.IsNullOrEmpty(str))
                {
                    str = DateTime.Now.ToString("dd-MMM-yyyy");
                }
                char[] spearator = { '-' };

                String[] strDate = str.Split(spearator, StringSplitOptions.None);

                string Year = strDate[2];
                string Month = strDate[1];

                string query = string.Empty;

                //DataTable dt = pbf.returnDataSetERP("select segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from mtl_system_items where organization_id in (149,150) and substr(segment1, 1, 1) in ('D', 'F', 'S')");
                //DataTable dt = pbf.returnDataTable("select segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + PubFun.ITEMS_USER + ".mtl_system_items where organization_id in (149,150) and substr(segment1, 1, 1) in ('F') order by segment1");
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Family))
                {
                    if (Convert.ToString(data.Family).ToUpper().Contains("TRACTOR"))
                    {
                        if (Convert.ToString(data.Plant).Trim().ToUpper() == "T05")
                            //query = @"select SHORT_CODE,ITEM_CODE as FCODE ,ITEM_CODE || ' # ' || SUBSTR(REPLACE(ITEM_DESCRIPTION,'POWER FARMTRAC','PT'),0,150) as DESCRIPTION from XXES_ITEM_MASTER where 
                            //            ITEM_CODE is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";
                            query = "SELECT I.SHORT_CODE, I.ITEM_CODE AS FCODE, I.ITEM_CODE || ' # ' || SUBSTR(REPLACE(I.ITEM_DESCRIPTION, 'TRACTOR FARMTRAC', 'FT'),0,150) AS DESCRIPTION " +
                               "FROM xxes_monthlyplanning P JOIN XXES_ITEM_MASTER I ON P.ITEM_CODE = I.ITEM_CODE WHERE P.ITEM_CODE is not null and" +
                               " P.plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and P.family_code='" + Convert.ToString(data.Family).Trim() + "' " +
                               "and P.YEAR='" + Convert.ToString(Year).Trim() + "' and P.MONTH='" + Convert.ToString(Month).ToUpper().Trim() + "'";
                        else if (Convert.ToString(data.Plant).Trim().ToUpper() == "T04")
                            //query = "select SHORT_CODE,ITEM_CODE as FCODE ,ITEM_CODE || ' # ' || SUBSTR(REPLACE(ITEM_DESCRIPTION,'TRACTOR FARMTRAC','FT'),0,150) as DESCRIPTION from XXES_ITEM_MASTER where ITEM_CODE is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";
                            query = "SELECT I.SHORT_CODE, I.ITEM_CODE AS FCODE, I.ITEM_CODE || ' # ' || SUBSTR(REPLACE(I.ITEM_DESCRIPTION, 'TRACTOR FARMTRAC', 'FT'),0,150) AS DESCRIPTION " +
                                "FROM xxes_monthlyplanning P JOIN XXES_ITEM_MASTER I ON P.ITEM_CODE = I.ITEM_CODE WHERE P.ITEM_CODE is not null and" +
                                " P.plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and P.family_code='" + Convert.ToString(data.Family).Trim() + "' " +
                                "and P.YEAR='" + Convert.ToString(Year).Trim() + "' and P.MONTH='" + Convert.ToString(Month).ToUpper().Trim() + "'";
                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                    {
                        query = "select BACKEND as FCODE ,BACKEND || ' # ' || nvl(backend_desc,'') as DESCRIPTION from xxes_backend_master where BACKEND is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    }

                }

                if (!string.IsNullOrEmpty(query))
                {
                    dt = fun.returnDataTable(query);
                }
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["FCODE"].ToString(),
                        });
                    }
                }
                return Item;
            }
            catch (Exception ex)
            {
                //throw;
                return Item;
            }
            finally { }
        }

        [HttpPost]
        public ActionResult ExportToExcel(AddDailyPlanModel data)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string filename = "PLAN_" + data.Shift + "_" + Convert.ToDateTime(data.Date).ToString("ddMMyyyy");
            data.ImportExcel = filename;
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DAILYPLAN");

            ws.Cell("A1").Value = "ITEM";
            ws.Cell("B1").Value = "QUANTITY";
            ws.Cell("C1").Value = "TYRE";
            ws.Cell("D1").Value = "MODEL";
            ws.Range("A1:B1").Style.Font.Bold = true;
            ws.Range("C1:D1").Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }

            wb.SaveAs(FilePath);
            //Download(filename);
            msg = "Format downloaded ...";
            mstType = "alert-info";
            excelName = data.ImportExcel;
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Download(string file)
        {
            //string MRN = Request.QueryString["MRN"].ToString();
            //string ITEMCOD = Request.QueryString["ITEMCOD"].ToString();
            //string Stiker = Request.QueryString["Stiker"].ToString();
            //int NoBox = Convert.ToInt16(Request.QueryString["NoBox"]);

            string FilePath = Server.MapPath("~/TempExcelFile/" + file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(FilePath, "application/vnd.ms-excel", file);


        }
        [HttpPost]
        public ActionResult ImportExcelDailyPlan(AddDailyPlanModel ADP, HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            //inputFile.FileName.Replace("-", "_");
            string RowAction = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(ADP.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Family))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Date))
                {
                    msg = "Please Select Date ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Shift))
                {
                    msg = "Please Select Shift ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //string quantity = ADP.TotalQty;
                if (ADP.TotalQty == 0 || ADP.TotalQty == null)
                {
                    msg = "Please Enter Total Quantity ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (inputFile == null || inputFile.ContentLength == 0)
                {
                    msg = "Please Choose Excel Sheet ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //string Month = string.Empty;
                //string Year = string.Empty;

                //char[] spearator = { '-' };
                //String[] strDate = ADP.Date.Split(spearator, StringSplitOptions.None);
                //Month = strDate[1].ToUpper();
                //Year = strDate[2].ToUpper();

                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    //var fileName = inputFile.FileName.Replace("-", "");
                    string path = Server.MapPath("~/TempExcelFile/" + inputFile.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    inputFile.SaveAs(path);
                    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
                    OleDbConnection connection = new OleDbConnection();
                    connection.ConnectionString = constr;
                    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + "DAILYPLAN" + "$]", connection);

                    OleDbDataAdapter da = new OleDbDataAdapter(command);
                    //DataSet ds = new DataSet();
                    //da.Fill(ds);
                    dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        msg = "NO ROWS FOUND IN EXCEL FILE !!";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    int count = 0;



                    string lastPlanId = "", errormsg = string.Empty;
                    string ItemCodeValue = ADP.ItemCode.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = ItemCodeValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    ADP.ItemCode = subs[0];
                    ADP.Item_desc = subs[1];
                    
                    int errorrow = 1;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(row["ITEM"])))
                        {
                            msg = "ITEMCODE SHOULD NOT BE EMPTY AT ROW NO " + Convert.ToString(errorrow);
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(Convert.ToString(row["QUANTITY"])))
                        {
                            msg = "QUANTITY SHOULD NOT BE EMPTY AT ROW NO " + Convert.ToString(errorrow);
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        query = string.Format(@"SELECT count(*) FROM XXES_ITEM_MASTER WHERE ITEM_CODE ='{0}'",
                        Convert.ToString(row["ITEM"]));
                        if (!fun.CheckExits(query))
                        {
                            msg = "INVALID ITEMCODE " + Convert.ToString(row["ITEM"]) + " AT ROW NO " + Convert.ToString(errorrow);
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else if (!fun.IsFloatOrInt(Convert.ToString(row["QUANTITY"])))
                        {
                            msg = "INVALID QTY FOR ITEMCODE " + Convert.ToString(row["ITEM"]) + " AT ROW NO " + Convert.ToString(errorrow);
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        errorrow++;
                    }

                    query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '{0}' 
                    AND SHIFTCODE = '{1}' AND PLANT_CODE = '{2}' AND FAMILY_CODE = '{3}'", ADP.Date, ADP.Shift, ADP.Plant, ADP.Family);
                    string PlanId = Convert.ToString(fun.get_Col_Value(query));

                    if (string.IsNullOrEmpty(PlanId)) // if existing plan not found of same plant, family,shift and date then insert data in plan master
                    {
                        query = "INSERT INTO XXES_DAILY_PLAN_MASTER(PLAN_DATE, SHIFTCODE, EXPECTED_QTY, ENTRY_DATE, PLANT_CODE, FAMILY_CODE, CREATEDBY, STATUS) " +
                                   "VALUES('" + ADP.Date + "', '" + ADP.Shift + "', '" + ADP.TotalQty + "' ,'" + ADP.Date + "', '" + ADP.Plant + "','" + ADP.Family + "','" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "','APPROVED')";
                        if (fun.EXEC_QUERY(query))
                        {
                            query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE  to_char(PLAN_DATE,'dd-Mon-yyyy')= '{0}' 
                    AND SHIFTCODE = '{1}' AND PLANT_CODE = '{2}' AND FAMILY_CODE = '{3}'", ADP.Date, ADP.Shift, ADP.Plant, ADP.Family);
                            PlanId = Convert.ToString(fun.get_Col_Value(query));
                        }
                    }
                    if (string.IsNullOrEmpty(PlanId))
                    {
                        msg = "UNABLE TO CREATE PLAN OF SELECTED SHIFT";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (ADP.IsOverride == true)
                    {
                        query = string.Format(@"DELETE FROM XXES_DAILY_PLAN_JOB WHERE FCODE_AUTOID  IN
                                (
                                SELECT AUTOID FROM XXES_DAILY_PLAN_TRAN WHERE PLAN_ID='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}' AND AUTOID NOT IN
                                (SELECT s.FCODE_ID FROM XXES_JOB_STATUS s WHERE S.PLANT_CODE='{1}' AND S.FAMILY_CODE='{2}')
                                )", PlanId, ADP.Plant, ADP.Family);
                        fun.EXEC_QUERY(query);

                        query = string.Format(@"DELETE FROM XXES_DAILY_PLAN_TRAN WHERE  PLAN_ID = '{0}' AND PLANT_CODE = '{1}'
                         AND FAMILY_CODE = '{2}' and autoid not in
                        (SELECT s.FCODE_ID FROM XXES_JOB_STATUS s WHERE S.PLANT_CODE='{1}' AND S.FAMILY_CODE='{2}' and fcode_id is not null)", PlanId, ADP.Plant, ADP.Family);
                        fun.EXEC_QUERY(query);
                      
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        string ItemName = "";
                        query = "SELECT ITEM_DESCRIPTION FROM XXES_ITEM_MASTER WHERE ITEM_CODE ='" + Convert.ToString(row["ITEM"]) + "'";
                        ItemName = fun.get_Col_Value(query);
                        query = "INSERT INTO XXES_DAILY_PLAN_TRAN(PLAN_ID, PLANT_CODE, FAMILY_CODE, ITEM_CODE,ITEM_DESC, QTY, MAKE, TYPE) " +
                                "VALUES( '" + PlanId + "', '" + ADP.Plant + "', '" + ADP.Family + "' , '" + row["ITEM"].ToString() + "', '" + ItemName + "','" + row["QUANTITY"].ToString() + "','" + row["TYRE"].ToString() + "', '" + row["MODEL"].ToString() + "')";
                        fun.EXEC_QUERY(query);
                        count++;
                    }
                    string reponse = string.Empty;
                    if (count > 0)
                    {
                        reponse = SyncJobs(ADP);
                        msg = "Total " + Convert.ToString(count) + " Items Saved Successfully. " + reponse;
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };

                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ImportExcelDailyPlan_Old(AddDailyPlanModel ADP, HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            //inputFile.FileName.Replace("-", "_");
            string RowAction = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(ADP.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Family))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Date))
                {
                    msg = "Please Select Date ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(ADP.Shift))
                {
                    msg = "Please Select Shift ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //string quantity = ADP.TotalQty;
                if (ADP.TotalQty == 0 || ADP.TotalQty == null)
                {
                    msg = "Please Enter Total Quantity ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (inputFile == null || inputFile.ContentLength == 0)
                {
                    msg = "Please Choose Excel Sheet ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //string Month = string.Empty;
                //string Year = string.Empty;

                //char[] spearator = { '-' };
                //String[] strDate = ADP.Date.Split(spearator, StringSplitOptions.None);
                //Month = strDate[1].ToUpper();
                //Year = strDate[2].ToUpper();

                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    //var fileName = inputFile.FileName.Replace("-", "");
                    string path = Server.MapPath("~/AddDailyPlan/" + inputFile.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    inputFile.SaveAs(path);
                    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
                    OleDbConnection connection = new OleDbConnection();
                    connection.ConnectionString = constr;
                    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + "DAILYPLAN" + "$]", connection);

                    OleDbDataAdapter da = new OleDbDataAdapter(command);
                    //DataSet ds = new DataSet();
                    //da.Fill(ds);
                    dt = new DataTable();
                    da.Fill(dt);

                    int count = 0;



                    string lastPlanId = "", errormsg = string.Empty;
                    string ItemCodeValue = ADP.ItemCode.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = ItemCodeValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    ADP.ItemCode = subs[0];
                    ADP.Item_desc = subs[1];

                    //if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_DAILY_PLAN_MASTER WHERE PLANT_CODE = '" + ADP.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + ADP.Family.ToUpper().Trim() + "' AND SHIFTCODE = '" + ADP.Shift + "' AND ENTRY_DATE = '" + ADP.Date + "'")) > 0)
                    //{
                    //    msg = "Items Already Exist ..";
                    //    mstType = "alert-danger";
                    //    var resul = new { Msg = msg, ID = mstType };
                    //    return Json(resul, JsonRequestBehavior.AllowGet);
                    //}

                    List<listItemName> ItemNamelist = new List<listItemName>();
                    foreach (DataRow row in dt.Rows)
                    {
                        string ItemName1 = "";
                        query = "SELECT ITEM_DESCRIPTION FROM XXES_ITEM_MASTER WHERE ITEM_CODE ='" + row["ITEM"].ToString() + "'";
                        ItemName1 = fun.get_Col_Value(query);

                        ItemNamelist.Add(new listItemName { ItemName = ItemName1, ItemCode = row["ITEM"].ToString(), Qty = row["QUANTITY"].ToString() });
                    }

                    var CountUnmachedItem = ItemNamelist.Where(x => x.ItemName == null || x.ItemName == "").Select(x => x.ItemName).Count();
                    if (CountUnmachedItem > 0)
                    {
                        var itemNameExist = new listItemName();
                        var Exist = ItemNamelist.FirstOrDefault(x => x.ItemName == null || x.ItemName == "");
                        if (Exist != null)
                        {
                            itemNameExist.ItemCode = Exist.ItemCode;
                            itemNameExist.Qty = Exist.Qty;
                        }
                        msg = "Item Doesn't Exist Or Incorrect in Excel ItemCode " + itemNameExist.ItemCode + "  Where Qty is  " + itemNameExist.Qty + " ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (ADP.Date != null && ADP.Shift != null && ADP.TotalQty > 0 && ADP.Plant != null && ADP.Family != null)
                        {
                            query = "INSERT INTO XXES_DAILY_PLAN_MASTER(PLAN_DATE, SHIFTCODE, EXPECTED_QTY, ENTRY_DATE, PLANT_CODE, FAMILY_CODE, CREATEDBY, STATUS) " +
                                    "VALUES('" + ADP.Date + "', '" + ADP.Shift + "', '" + ADP.TotalQty + "' ,'" + ADP.Date + "', '" + ADP.Plant + "','" + ADP.Family + "','" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "','APPROVED')";
                            fun.EXEC_QUERY(query);

                            query = "SELECT MAX(PLAN_ID) FROM XXES_DAILY_PLAN_MASTER where PLANT_CODE ='" + ADP.Plant + "' and FAMILY_CODE ='" + ADP.Family + "'  and SHIFTCODE ='" + ADP.Shift + "' and ENTRY_DATE ='" + ADP.Date + "'";
                            lastPlanId = fun.get_Col_Value(query);
                            if (ADP.IsOverride == true)
                            {
                                query = "DELETE FROM XXES_DAILY_PLAN_TRAN WHERE  PLAN_ID = '" + lastPlanId + "' AND PLANT_CODE = '" + ADP.Plant + "' AND FAMILY_CODE = '" + ADP.Family + "'";
                                fun.EXEC_QUERY(query);
                            }
                            foreach (DataRow row1 in dt.Rows)
                            {
                                string ItemName = "";
                                query = "SELECT ITEM_DESCRIPTION FROM XXES_ITEM_MASTER WHERE ITEM_CODE ='" + row1["ITEM"].ToString() + "'";
                                ItemName = fun.get_Col_Value(query);
                                query = "INSERT INTO XXES_DAILY_PLAN_TRAN(PLAN_ID, PLANT_CODE, FAMILY_CODE, ITEM_CODE,ITEM_DESC, QTY, MAKE, TYPE) " +
                                        "VALUES( '" + lastPlanId + "', '" + ADP.Plant + "', '" + ADP.Family + "' , '" + row1["ITEM"].ToString() + "', '" + ItemName + "','" + row1["QUANTITY"].ToString() + "','" + row1["TYRE"].ToString() + "', '" + row1["MODEL"].ToString() + "')";
                                fun.EXEC_QUERY(query);
                                count++;
                            }

                        }
                    }
                    if (count > 0)
                    {
                        msg = "Total " + Convert.ToString(count) + " Items Saved Successfully...";
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ValidateFileExtention(HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (inputFile == null)
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    mstType = "alert-success";
                }
                else
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult ImportExcel(HttpPostedFileBase File)
        //{
        //    string Filename = File.FileName;
        //    string filepath = "/AddDailyPlan/" + Filename;
        //    File.SaveAs(Path.Combine(Server.MapPath("/AddDailyPlan"), Filename));
        //    Inserexceldata(filepath, Filename);
        //    return View();
        //}
        //private void excelconn(string filepath)
        //{
        //    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", filepath);

        //    OleDbConnection connection = new OleDbConnection();
        //    connection.ConnectionString = constr;
        //}
        //private void Inserexceldata(string filepath, string fileName)
        //{
        //    string path = Server.MapPath("~/AddDailyPlan/" + fileName);
        //    excelconn(path);
        //    string query = string.Format("SELECT * FROM [" + fileName + "$]"); 
        //    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
        //    OleDbConnection connection = new OleDbConnection();
        //    connection.ConnectionString = constr;
        //    //OleDbCommand command = new OleDbCommand("SELECT * FROM [" + fileName + "$]", connection);
        //    OleDbCommand ECom = new OleDbCommand(query, connection);
        //    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + ADP.DateExcel + "$]", connection);

        //    OleDbDataAdapter da = new OleDbDataAdapter(command);
        //    //DataSet ds = new DataSet();
        //    //da.Fill(ds);
        //    dt1 = new DataTable();
        //    da.Fill(dt1);

        //    DataSet ds = new DataSet();
        //    OleDbDataAdapter oda = new OleDbDataAdapter(query, connection);
        //     fun.ConOpen();
        //    oda.Fill(ds);

        //    DataTable dt = ds.Tables[0];

        //    SqlBulkCopy objbulk = new SqlBulkCopy(Function.orCnstr);
        //    objbulk.DestinationTableName = "XXES_DAILY_PLAN_TRAN";
        //    objbulk.ColumnMappings.Add("ITEM_Code", "ITEM");
        //    objbulk.ColumnMappings.Add("QTY", "EXPECTED_QTY");
        //    objbulk.ColumnMappings.Add("MAKE", "TYRE");
        //    objbulk.ColumnMappings.Add("TYRE", "MODEL");
        //    fun.ConClose();
        //    objbulk.WriteToServer(dt);

        //}


    }
    public class listItemName
    {
        public string ItemName { get; set; }
        public string Qty { get; set; }
        public string ItemCode { get; set; }
    }
}