using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Controllers.DCU;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class FTEngineController : Controller
    {
        // GET: FTEngine
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        Assemblyfunctions af = new Assemblyfunctions();
        Tractor tractor = new Tractor();
        string planid = string.Empty;
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public JsonResult BindPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindFamily(string Plant)
        {

            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                result = fun.Fill_FamilyOnlyTractor(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetNextTractorNo(string Plant, string Family)
        {
            string Result = string.Empty;
            TractorController tractor = new TractorController();
            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
                {
                    COMMONDATA cOMMONDATA = new COMMONDATA();
                    cOMMONDATA.PLANT = Plant;
                    cOMMONDATA.FAMILY = Family;
                    cOMMONDATA.REMARKS = "EN";
                    string line = tractor.getNextTractorNo(cOMMONDATA);
                    if (!string.IsNullOrEmpty(line))
                    {
                        Result = line;
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Print(FTENGINE data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string isBackEndRequire = string.Empty;
            try
            {
                if (!data.isbypass == true)
                {
                    if (string.IsNullOrEmpty(data.JobId))
                    {
                        msg = "Please Enter Job..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(data.EngineSrno))
                    {
                        msg = "Please Enter Engine Srno..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                TractorController tractor = new TractorController();
                ENGINE engine = new ENGINE();
                engine.Plant = data.Plant.Trim().ToUpper();
                engine.Family = data.Family.Trim().ToUpper();
                engine.Fcode = data.EngineSrno;
                engine.JobId = data.JobId;
                engine.LoginUser = Convert.ToString(Session["Login_User"]);
                engine.StageCode = "EN";
                engine.LoginStage = "99";
                engine.SYSUSER = engine.LoginUser;
                engine.SYSTEMNAME = fun.GetUserIP();
                if (data.isbypass == true)
                    engine.IsBy_Pass = "Y";
                else
                    engine.IsBy_Pass = "";
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'", engine.Plant, engine.Family, engine.StageCode);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    engine.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    engine.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    engine.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    engine.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    engine.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + engine.Plant + "'");
                }
                HttpResponseMessage httpresponse = tractor.UpdateEngine(engine);
                string response = httpresponse.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(response))
                {
                    if (response.StartsWith("OK"))
                    {
                        msg = response.Split('#')[1].Trim().ToUpper();
                        mstType = "alert-success";
                        status = Validation.str2;
                    }
                    else
                    {
                        msg = response.Trim().ToUpper();
                        mstType = "alert-danger";
                        status = Validation.str2;
                    }
                }
                else
                {
                    msg = "SOMETHING WENT WRONG !! ";
                    mstType = "alert-danger";
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var myResult = new
            {
                Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopup(PTBUCKlUP data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {

                query = string.Format(@"select COUNT(*) from xxes_stage_master where  PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ad_password='{2}' AND OFFLINE_KEYCODE='BK'",
                        data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(), data.Password.Trim());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Invalid Password..!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Reprint(FTENGINE data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Job))
                {
                    msg = "Please Enter Job..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                TractorController tractor = new TractorController();
                ENGINE engine = new ENGINE();
                engine.Plant = data.Plant.Trim().ToUpper();
                engine.Family = data.Family.Trim().ToUpper();
                engine.JobId = data.Job.Trim().ToUpper();
                engine.LoginUser = Convert.ToString(Session["Login_User"]);
                engine.StageCode = "EN";
                engine.LoginStage = "99";
                engine.SYSUSER = engine.LoginUser;
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'", engine.Plant, engine.Family, engine.StageCode);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    engine.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    engine.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    engine.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    engine.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    engine.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + engine.Plant + "'");
                }
                string response = tractor.ReprintEngineLabelFT(engine);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response.StartsWith("OK"))
                    {
                        msg = response.Split('#')[1].Trim().ToUpper();
                        mstType = "alert-success";
                        status = Validation.str2;
                    }
                    else
                    {
                        msg = response.Trim().ToUpper();
                        mstType = "alert-danger";
                        status = Validation.str2;
                    }
                }
                else
                {
                    msg = "SOMETHING WENT WRONG !! ";
                    mstType = "alert-danger";
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var myResult = new
            {
                Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }
    }
}