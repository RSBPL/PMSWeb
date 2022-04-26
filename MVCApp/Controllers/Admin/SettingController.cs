using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Admin
{
    [Authorize]
    public class SettingController : Controller
    {
        Function fun = new Function();
        string query = string.Empty;
        [HttpGet]
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
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BindTimeInterval()
        {
            List<DDLTextValue> Time = new List<DDLTextValue>();
            for (int i = 1; i <= 60; i++)
            {
                Time.Add(new DDLTextValue
                {
                    Text = Convert.ToString(i),
                    Value = Convert.ToString(i),
                });
            }
            Time.Insert(0, new DDLTextValue { Text = "SELECT", Value = "SELECT" });
            return Json(Time, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ExistingRecord(SftSetting data)
        {
            string msg = string.Empty;
            SftSetting sft = new SftSetting();
            DataTable dt = new DataTable();
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE FROM XXES_SFT_SETTINGS WHERE PLANT_CODE = '{0}' AND 
                                            FAMILY_CODE = '{1}'", data.Plant.Trim(), data.Family.Trim());
                    dt = fun.returnDataTable(query);
                    if(dt.Rows.Count > 0)
                    {
                        for(int i = 0; i<dt.Rows.Count; i++)
                        {
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "SUCCESS_INTERVAL")
                                sft.SuccessIntvl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            else if(Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "ERROR_INTERVAL")
                                sft.ErrorIntvl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            //if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "PRINTQTY_LABEL")
                            //    sft.QtyVeriLbl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "A4")
                                sft.A4Sheet =  true;                          
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "BARCODE")
                                sft.Barcode =  true;                        
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "QUALITY")
                                sft.Quality =  true;
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "KANBAN_PRINT")
                                sft.PrintingCategory = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "QC_FROMDAYS")
                                sft.QCFromDays = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "VERIFICATION_PRINT")
                                sft.PrintVerification = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);

                        }

                    }
                }
                else
                {
                    msg = "Record Not Found";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            var myResult = new
            {
                Result = sft,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(SftSetting data)
        {
            if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
            {
                return PartialView();
            }
            query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,AUTOID,
                                    CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MON_YYYY HH24:MI:SS') CREATED_DATE
                                    FROM XXES_SFT_SETTINGS where PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}'", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper());
            DataTable dt = fun.returnDataTable(query);
           
            ViewBag.DataSource = dt;
            return PartialView();
        }

        [HttpPost]
        public JsonResult Add(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            int j;
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.QCFromDays))
                {
                    if (!Int32.TryParse(data.QCFromDays, out j))
                    {
                        msg = "QC From Days is not a valid number ..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    if( j < 1)
                    {
                        msg = "Minimum no. of QC days should be greater than 0 ..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    
                }


                data.Status = "Y";
                data.Description = "SCREEN_SETTING";
                var tuple = fun.MobileMsgAlert(data, "ERROR_INTERVAL", "SUCCESS_INTERVAL");
                bool output = fun.QtyVerificationLabel(data, "PRINTQTY_LABEL", "QTY_LABEL_SETTING");
                bool barcode = fun.BarcodePrintingLabel(data, "KANBAN_PRINT", "KANBAN_SETTING");
                bool QcFrmDays = fun.QCFromDays(data, "QC_FROMDAYS", "QC From Date Days");
                bool VeficationPrinting = fun.VerificationPrinting(data, "VERIFICATION_PRINT", "VERIFICATION_SETTING");
                if (tuple.Item2 && output && barcode && QcFrmDays && VeficationPrinting)
                {
                    msg = tuple.Item1;
                    mstType = Validation.str;
                    status = Validation.stus;
                }

                else
                {
                    msg = tuple.Item1;
                    mstType = Validation.str1;
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
    }
}