using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCApp.Models;
using System.Web.Mvc;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using MVCApp.CommonFunction;

namespace MVCApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter da;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        public ActionResult Index()
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ViewBag.value = DateTime.Now;
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw;
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
            List<DDLTextValue> Result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                Result = fun.Fill_All_Family(Plant);
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
      
        [HttpPost]
        public JsonResult DisplayChart(DashboardModel model)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {

                List<ChartModel> DATA = new List<ChartModel>();
                if (string.IsNullOrEmpty(model.Plant) || string.IsNullOrEmpty(model.Family) || string.IsNullOrEmpty(model.FromDate) || string.IsNullOrEmpty(model.ToDate))
                {
                    msg = Validation.str29;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                List<ChartModel> chartModels = fun.DashBoardMaster(model);
                JsonResult result = Json(chartModels, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {              
                TempData["msg"] = ex.Message;
                TempData["msgType"] = "alert-danger";
                return new JsonResult() { Data = 12, MaxJsonLength = Int32.MaxValue };
            }
        }

        [HttpPost]
        public JsonResult GetAvilableStock(string Plant, string Family)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {

                List<ChartModel> DATA = new List<ChartModel>();
                if (string.IsNullOrEmpty(Plant) || string.IsNullOrEmpty(Family))
                {
                    msg = Validation.str29;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                List<ChartModel> chartModels = fun.Tractor_Available_Stock(Plant, Family);
                JsonResult result = Json(chartModels, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                TempData["msgType"] = "alert-danger";
                return new JsonResult() { Data = 12, MaxJsonLength = Int32.MaxValue };
            }
        }
    }
}