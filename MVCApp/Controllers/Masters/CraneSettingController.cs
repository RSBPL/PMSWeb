using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class CraneSettingController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: CraneSetting
        public ActionResult Index()
        {
            if(string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var now = DateTimeOffset.Now;
                var result = Enumerable.Range(0, 6).Select(i => now.AddMonths(i).ToString("MMM/yyyy"));
                ViewBag.Months = new SelectList(result);
                var result1 = Enumerable.Range(0, 6).Select(i => now.AddYears(i).ToString("yyyy"));
                ViewBag.Years = new SelectList(result1);
                return View();
            }
            
        }

        public PartialViewResult BindPlant()
        {
            ViewBag.Plant = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult Grid(string Type, string Plant)
        {
            if(!string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Plant))
            {
                ViewBag.DataSource = fun.GridCraneSetting(Type, Plant);
            }
            return PartialView();
        }

        public JsonResult Save(CraneSetting crane)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(crane.Plant))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(crane.Month))
                {
                    msg = "Please Select Month";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(crane.Type))
                {
                    msg = "Please Select Type";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
              
                if (string.IsNullOrEmpty(crane.Code))
                {
                    msg = "Please Enter Code";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUFFIX_CODE WHERE MY_CODE = '" + crane.Code.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Code already exist..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fun.InsertCraneSetting(crane))
                    {
                        msg = "Data Insert Successfully...";
                        mstType = "alert-success";
                        status = "success";

                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(CraneSetting crane)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteCraneSetting(crane))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //***************************Crane Setting Year**************************//

        public PartialViewResult BindPlantYear()
        {
            ViewBag.PlantYear = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult GridYear(string PlantYear)
        {
            if(!string.IsNullOrEmpty(PlantYear))
            {
                ViewBag.DataSourceYear = fun.GridCraneSettingYear(PlantYear);
            }
            return PartialView();
        }

        public JsonResult SaveYear(CraneSetting crane)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(crane.PlantYear))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(crane.MonthYear))
                {
                    msg = "Please Select Month";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(crane.CodeYear))
                {
                    msg = "Please Enter Code";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUFFIX_CODE WHERE MY_CODE = '" + crane.CodeYear.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Code already exist..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fun.InsertCraneSettingYear(crane))
                    {
                        msg = "Data Insert Successfully...";
                        mstType = "alert-success";
                        status = "success";

                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteYear(CraneSetting crane)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteCraneSettingYear(crane))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}