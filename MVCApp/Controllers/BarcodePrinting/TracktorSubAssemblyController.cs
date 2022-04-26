using MVCApp.Common;
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
    public class TracktorSubAssemblyController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        funTracktorsubAssembly TSAFun = new funTracktorsubAssembly();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "", LoginStageCode = "", Login_User="";
        // GET: FamilyMaster

        [HttpGet]
        public ActionResult Index()
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

        [HttpPost]
        public JsonResult Grid(SubAssemblyData obj)
        {

            int recordsTotal = 0;
            obj.PSearch = Request.Form.GetValues("search[value]").FirstOrDefault();
            List<SubAssemblyData> objData = TSAFun.GridTracktorSubAssemblyData(obj);
            if (objData.Count > 0)
            {
                recordsTotal = objData[0].TOTALCOUNT;
            }

            //return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = objData }, JsonRequestBehavior.AllowGet);
            var jsonResult = Json(new { aaData = objData }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
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
        public JsonResult Bindshift()
        {
            return Json(fun.FillShift(), JsonRequestBehavior.AllowGet);
        }

    }
}