using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

using System.Data;
using Newtonsoft.Json;

namespace MVCApp.Controllers
{
    [Authorize]
    public class HolidayMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "";
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.DefaultDate = DateTime.Now;
                return View();
            }
        }

        public JsonResult Save(HolidaysModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant_Code))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Holi_Date))
                {
                    msg = "Please Choose Date ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Description))
                {
                    msg = "Description should not be empty ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                query = "INSERT INTO XXES_HOLIDAYS (PLANT_CODE, HOLI_DATE, DESCRIPTION) VALUES('"+ data.Plant_Code +"','" + data.Holi_Date +"','" + data.Description +"')";

                if (fun.EXEC_QUERY(query))
                {
                    msg = "Saved successfully...";
                    mstType = "alert-success";
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

        public JsonResult Delete(HolidaysModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                query = "DELETE FROM XXES_HOLIDAYS WHERE AUTOID = '" + data.AutoId + "'";

                if (fun.EXEC_QUERY(query))
                {
                    msg = "Deleted successfully...";
                    mstType = "alert-danger";
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
        
        public PartialViewResult Grid(HolidaysModel data) 
        {
            query = @"SELECT * FROM XXES_HOLIDAYS WHERE PLANT_CODE = '" + data.Plant_Code +"'";
            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }
    }
}