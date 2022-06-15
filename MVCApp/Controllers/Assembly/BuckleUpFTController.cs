using MVCApp.CommonFunction;
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
    public class BuckleUpFTController : Controller
    {
        // GET: BuckleUpFT

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
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

            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select ITEM_CODE  || '(' || SUBSTR(ITEM_DESCRIPTION,0,30)  || ')' as DESCRIPTION , ITEM_CODE 
                        from XXES_ITEM_MASTER where  plant_code='{0}' and family_code='{1}' order by Item_code", Plant, Family);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["DESCRIPTION"]),
                                Value = Convert.ToString(dr["ITEM_CODE"])
                            });
                        }
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