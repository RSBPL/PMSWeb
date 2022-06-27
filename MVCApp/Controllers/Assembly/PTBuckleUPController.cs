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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class PTBuckleUPController : Controller
    {
        // GET: PTBuckleUP
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
        public JsonResult BindItemCode(string Plant, string Family)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();
            //DataTable dt = new DataTable();
            try
            {
                COMMONDATA cOMMONDATA = new COMMONDATA();
                cOMMONDATA.PLANT = Plant;
                cOMMONDATA.FAMILY = Family;
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.GetFcodes(cOMMONDATA);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.AsEnumerable())
                    {
                        _Item.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["AUTOID"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindJob(string Plant, string Family, string autoid)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {

                COMMONDATA cOMMONDATA = new COMMONDATA();
                cOMMONDATA.PLANT = Plant;
                cOMMONDATA.FAMILY = Family;
                cOMMONDATA.REMARKS = autoid;
                cOMMONDATA.LOCATION = "BUCKLEUP";
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.BindJobs(cOMMONDATA);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["CODE"])
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetDcode(string Plant, string Family, string item)
        {
            string AxleDcode, TransDcode, TRACTOR_DESC, Not_Require_seq; string Result = string.Empty;
            bool isBackEndRequire = false; string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            TractorController tractor = new TractorController();
            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
                {
                    query = string.Format(@"SELECT ITEM_CODE FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'"
                             , item, Plant, Family);
                    string itemcode = fun.get_Col_Value(query);
                    COMMONDATA cOMMONDATA = new COMMONDATA();
                    cOMMONDATA.PLANT = Plant;
                    cOMMONDATA.FAMILY = Family;
                    cOMMONDATA.ITEMCODE = itemcode;
                    string line = tractor.GetBackendEgnine(cOMMONDATA);
                    if (!string.IsNullOrEmpty(line))
                    {
                        AxleDcode = line.Split('#')[0].Trim() + " (" + line.Split('#')[4].Trim() + ")";
                        TransDcode = line.Split('#')[1].Trim() + " (" + line.Split('#')[3].Trim() + ")";
                        TRACTOR_DESC = line.Split('#')[2].Trim();
                        Not_Require_seq = line.Split('#')[5].Trim();
                        if (!string.IsNullOrEmpty(AxleDcode) && !string.IsNullOrEmpty(TransDcode))
                        {
                            Result = AxleDcode + "#" + TransDcode;
                           
                            return Json(Result, JsonRequestBehavior.AllowGet);
                        }
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
        public JsonResult GetNextTractorNo(string Plant ,string Family)
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
                    if(!string.IsNullOrEmpty(line))
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
        public JsonResult Print(PTBUCKlUP data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string isBackEndRequire = string.Empty;
            try
            {
                query = string.Format(@"SELECT ITEM_CODE ||'#'|| ITEM_DESC  FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'"
                            , data.ItemCode.Trim().ToUpper(), data.Plant, data.Family);
                string line = fun.get_Col_Value(query);
                if (!data.isbypass == true)
                {                    
                    if (string.IsNullOrEmpty(data.BackendSrno))
                    {
                        msg = "Please Enter Backend Srno..";
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
                PTBuckleup pTBuckleup = new PTBuckleup();
                pTBuckleup.PLANT = data.Plant.Trim().ToUpper();
                pTBuckleup.FAMILY = data.Family.Trim().ToUpper();
                pTBuckleup.ITEMCODE = line.Split('#')[0].Trim().ToUpper();
                pTBuckleup.TRACTOR_DESC = line.Split('#')[1].Trim().ToUpper();
                pTBuckleup.JOB = data.JobId.Trim().ToUpper();
                pTBuckleup.FCODEID = data.ItemCode;
                pTBuckleup.BACKENDSRLNO = data.BackendSrno;
                pTBuckleup.ENGINESRLNO = data.EngineSrno;
                pTBuckleup.CREATEDBY = Convert.ToString(Session["Login_User"]);
                pTBuckleup.STAGE = "EN";
                pTBuckleup.SYSUSER = pTBuckleup.CREATEDBY;
                pTBuckleup.SYSTEMNAME = fun.GetUserIP();
                if (data.isbypass == true)
                    pTBuckleup.BYPASS = "Y";
                else
                    pTBuckleup.BYPASS = "";
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'", pTBuckleup.PLANT, pTBuckleup.FAMILY, pTBuckleup.STAGE);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    pTBuckleup.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    pTBuckleup.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    pTBuckleup.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    pTBuckleup.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    pTBuckleup.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + pTBuckleup.PLANT + "'");
                }
                HttpResponseMessage  httpresponse = tractor.PowerTractorBuckleUp(pTBuckleup);
               string response = httpresponse.Content.ReadAsStringAsync().Result;

                //string response = Convert.ToString(res);
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
        public JsonResult Reprint(PTBUCKlUP data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.TractorSrno))
                {
                    msg = "Please Enter Job..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                TractorController tractor = new TractorController();
                PTBuckleup pTBuckleup = new PTBuckleup();
                pTBuckleup.PLANT = data.Plant.Trim().ToUpper();
                pTBuckleup.FAMILY = data.Family.Trim().ToUpper();
                pTBuckleup.TSN = data.TractorSrno.Trim().ToUpper();
                pTBuckleup.CREATEDBY = Convert.ToString(Session["Login_User"]);
                pTBuckleup.STAGE = "EN";
                pTBuckleup.SYSUSER = pTBuckleup.CREATEDBY;
                pTBuckleup.SYSTEMNAME = fun.GetUserIP();
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'", pTBuckleup.PLANT, pTBuckleup.FAMILY, pTBuckleup.STAGE);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    pTBuckleup.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    pTBuckleup.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    pTBuckleup.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    pTBuckleup.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    pTBuckleup.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + pTBuckleup.PLANT + "'");
                }
                string response = tractor.ReprintTractorLabel(pTBuckleup);
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