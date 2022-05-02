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
using EncodeDecode;
using System.Globalization;


namespace MVCApp.Controllers
{
    [Authorize]
    public class ApprovalMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        string query1 = string.Empty;

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.TodayDate = Convert.ToDateTime(Session["ServerDate"]).AddDays(0);               
                return View();
            }
        }
        public PartialViewResult Grid(AddDailyPlanModel data)
        {            
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    DateTime ServerDate = Convert.ToDateTime(Session["ServerDate"]).Date;

                    string Date = ServerDate.ToString("dd-MMM-yyyy");

                    query = @"select a.AUTOID, a.PLAN_ID, a.ITEMCODE,SUBSTR( a.DESCRIPTION, 1, 50 ) AS DESCRIPTION, a.STATUS, 
                            (select to_char(PLAN_DATE,'dd-Mon-yyyy') from xxes_daily_plan_master where plan_id = a.plan_id AND trunc(PLAN_DATE) >= '" + Date +"') as PLANDATE, " +
                            "(select shiftcode from xxes_daily_plan_master where plan_id = a.plan_id) as SHIFT" +
                            " FROM xxes_daily_plan_assembly A" +
                            " WHERE a.PLANT_CODE = '" + Convert.ToString(data.Plant) + "' AND a.FAMILY_CODE = '" + Convert.ToString(data.Family) + "' AND A.STATUS like 'PENDING%' ";
                }
            }
            catch (Exception ex)
            {

                //throw;
            }
            DataTable dt = new DataTable();
            dt = fun.returnDataTable(query);

            ViewBag.DataSource = dt.AsEnumerable().ToList();
            return PartialView();
        }
        public JsonResult Approval(Approv[] data)
        {
            string msg, mstType;
            int count = 0;
            try
            {
                if (data != null)
                {                    
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.Status))
                        {
                            query = "UPDATE XXES_DAILY_PLAN_ASSEMBLY SET STATUS = '"+ item.Status.ToUpper() +"', APPROVEDBY = '"+ System.Web.HttpContext.Current.User.Identity.Name.ToString() + "', APPROVEDDATE = SYSDATE WHERE AUTOID = '"+ item.AutoId +"'";
                            if (fun.EXEC_QUERY(query))
                            {
                                count++;
                            }
                        }
                    }
                }
                msg = Convert.ToString(count) + " Items Approved...";
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {                
                msg = ex.Message;
                mstType = "alert-success";
                var result = new { Msg = msg, ID = mstType };
                return Json(result, JsonRequestBehavior.AllowGet);
            }  
        }
        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_FamilyForSubAssembly(Plant), "Value", "Text");
            }
            return PartialView();
        }       
    }
    public class Approv 
    {
        public int AutoId { get; set; }
        public string Status { get; set; }
    } 
}