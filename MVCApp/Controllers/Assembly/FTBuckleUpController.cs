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
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class FTBuckleUpController : Controller
    {
        // GET: BuckleUpFT

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
        public JsonResult BindJob(string Plant, string Family,string autoid)
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
                    string line = tractor.GetTransmissionRearAxel(cOMMONDATA);
                    if (!string.IsNullOrEmpty(line))
                    {
                        TransDcode = line.Split('#')[0].Trim() + " (" + line.Split('#')[4].Trim() + ")";
                        AxleDcode = line.Split('#')[1].Trim() + " (" + line.Split('#')[3].Trim() + ")";
                        TRACTOR_DESC = line.Split('#')[2].Trim();
                        isBackEndRequire = (line.Split('#')[5].Trim() == "Y" ? true : false);
                        Not_Require_seq = line.Split('#')[6].Trim();
                        if (isBackEndRequire)
                        {
                            Result = Convert.ToString(isBackEndRequire);
                            if(Result == "True")
                            {
                                Result = "Y";
                            }
                            return Json(Result, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Result = Convert.ToString(isBackEndRequire);
                            {
                                if (Result == "False")
                                {
                                    
                                    Result = "N#"  + TransDcode + "#" + AxleDcode;

                                }
                            }
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
        public JsonResult Print(BuckleUPFT data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string isBackEndRequire = string.Empty;
            try
            {
                query = string.Format(@"SELECT ITEM_CODE ||'#'|| ITEM_DESC  FROM XXES_DAILY_PLAN_TRAN WHERE AUTOID='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'"
                            , data.ItemCode.Trim().ToUpper(), data.Plant, data.Family);
                string line = fun.get_Col_Value(query);
                query = string.Format(@"SELECT REQUIRE_BACKEND FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' and PLANT_CODE='{1}' and 
                     FAMILY_CODE='{2}'", line.Split('#')[0].Trim().ToUpper(), data.Plant, data.Family);
                isBackEndRequire = fun.get_Col_Value(query);
                if(!data.isbypass == true)
                {
                    if (isBackEndRequire == "N")
                    {
                        if (string.IsNullOrEmpty(data.TransmissionSrno))
                        {
                            msg = "Please Enter Transmission Srno..";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(data.RearAxleSrno))
                        {
                            msg = "Please Enter RearAxle Srno..";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(data.BackendSrno))
                        {
                            msg = "Please Enter Backend Srno..";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                

                TractorController tractor = new TractorController();
                FTBuckleup fTBuckleup = new FTBuckleup();
                fTBuckleup.PLANT = data.Plant.Trim().ToUpper();
                fTBuckleup.FAMILY = data.Family.Trim().ToUpper();
                fTBuckleup.ITEMCODE = line.Split('#')[0].Trim().ToUpper();
                fTBuckleup.TRACTOR_DESC = line.Split('#')[1].Trim().ToUpper();
                fTBuckleup.JOB = data.JobId.Trim().ToUpper();
                fTBuckleup.FCODEID = data.ItemCode;
                fTBuckleup.TRANSMISSIONSRLNO = data.TransmissionSrno;
                fTBuckleup.REARAXELSRLNO = data.RearAxleSrno;
                fTBuckleup.BackendSrlno = data.BackendSrno;
                fTBuckleup.CREATEDBY = Convert.ToString(Session["Login_User"]);
                fTBuckleup.STAGE = "BK";
                fTBuckleup.SYSUSER = fTBuckleup.CREATEDBY;
                fTBuckleup.SYSTEMNAME = fun.GetUserIP();
                if (data.isbypass == true)
                    fTBuckleup.BYPASS = "Y";
                else
                    fTBuckleup.BYPASS = "";
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'",fTBuckleup.PLANT,fTBuckleup.FAMILY, fTBuckleup.STAGE);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count>0)
                {
                    fTBuckleup.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    fTBuckleup.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    fTBuckleup.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    fTBuckleup.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    fTBuckleup.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + fTBuckleup.PLANT + "'");
                }
                string response  = tractor.FarmTractorBuckleUp(fTBuckleup);
                if(!string.IsNullOrEmpty(response))
                {
                    if(response.StartsWith("OK"))
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
        public JsonResult PasswordPopup(BuckleUPFT data)
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
        public JsonResult Reprint(BuckleUPFT data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(data.Job))
                {
                    msg = "Please Enter Job..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                TractorController tractor = new TractorController();
                FTBuckleup fTBuckleup = new FTBuckleup();
                fTBuckleup.PLANT = data.Plant.Trim().ToUpper();
                fTBuckleup.FAMILY = data.Family.Trim().ToUpper();
                fTBuckleup.JOB = data.Job.Trim().ToUpper();
                fTBuckleup.CREATEDBY = Convert.ToString(Session["Login_User"]);
                fTBuckleup.STAGE = "BK";
                fTBuckleup.SYSUSER = fTBuckleup.CREATEDBY;
                fTBuckleup.SYSTEMNAME = fun.GetUserIP();
                query = string.Format(@"select * from xxes_stage_master where offline_keycode='{2}' and plant_code='{0}'
                and family_code='{1}'", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.STAGE);
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    fTBuckleup.PrintMMYYFormat = Convert.ToString(dt.Rows[0]["ONLINE_SCREEN"]);
                    fTBuckleup.IsPrintLabel = Convert.ToString(dt.Rows[0]["PRINT_LABEL"]);
                    fTBuckleup.IPADDR = Convert.ToString(dt.Rows[0]["ipaddr"]);
                    fTBuckleup.IPPORT = Convert.ToString(dt.Rows[0]["IPPORT"]);
                    fTBuckleup.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + fTBuckleup.PLANT + "'");
                }
                string response = tractor.ReprintTractorLabelFT(fTBuckleup);
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