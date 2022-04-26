using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Newtonsoft.Json;


namespace MVCApp.Controllers.MASTERS
{
    [Authorize]
    public class PlantMasterController : Controller
    {
        //OracleCommand cmd;
        //OracleDataAdapter DA;
        //OracleDataReader dr;
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
                return View();
            }
        }


        public PartialViewResult Grid()
        {            
           ViewBag.DataSource = fun.GridPlantMaster();
           return PartialView();
        }

        public JsonResult Save(PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidatePlantMaster("PlantMaster", "Save", data);

                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

              
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_UNIT_MASTER WHERE U_Code = '" + data.PlantCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str14;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //data.Session = Convert.ToString(Session["Login_User"]);
                    if (fun.InsertPlantMaster(data))
                    {
                        msg = Validation.str9;
                        mstType = Validation.str;
                        status = Validation.stus;
                    }
                }


            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidatePlantMaster("PlantMaster", "Update", data);

                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.UpdatePlantMaster(data))
                {
                    msg = Validation.str11;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);

            }

            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public JsonResult Delete(PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (fun.DeletePlantMaster(data))
                {
                    msg = Validation.str23;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }

            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        
    }
}