using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace MVCApp.Controllers
{
    public class RollDownController : Controller
    {
        // GET: HookUpAndDown
        string msg = string.Empty; string mstType = string.Empty;
        Function fun = new Function();
        string query = string.Empty;
        DataTable dt = new DataTable();

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
        public JsonResult FillItemCode(RollDown data)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PLANTCODE).Trim().ToUpper(), Convert.ToString(data.FAMILYCODE).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.ITEM_CODE))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.ITEM_CODE.Trim().ToUpper(), data.ITEM_CODE.Trim().ToUpper());
                    //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                    //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }
    }
}