
using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.MHS
{
    [Authorize]
    public class DashbordController : Controller
    {
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
        public JsonResult BindDashBord(EKIDashbordModel data)
        {
            return Json(fun.BindEKIDashbord(data), JsonRequestBehavior.AllowGet);
        }

    }
}