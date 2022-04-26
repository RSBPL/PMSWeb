using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Mrn
{
    [Authorize]
    public class FuelInjectionMappingController : Controller
    {
        string msg = string.Empty; string mstType = string.Empty;
        Function fun = new Function();

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

        [HttpPost]
        public JsonResult BindItemCode(FIPModel data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PLANT_CODE) || string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PLANT_CODE).Trim().ToUpper(), Convert.ToString(data.FAMILY_CODE).Trim().ToUpper());

                DataTable dt = new DataTable();

                query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.ITEM_CODE.Trim().ToUpper(), data.ITEM_CODE.Trim().ToUpper());
                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
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
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Add(FIPModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE) || string.IsNullOrEmpty(data.FAMILY_CODE) || string.IsNullOrEmpty(data.ITEM_CODE))
                {
                    msg = "Plant,Family & Item Code are required...";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                //string query = string.Empty;
                //char[] spearator = { '#' };
                //String[] SplitItemCode = data.ITEM_CODE.Split(spearator, StringSplitOptions.None);
                //data.ITEM_CODE = SplitItemCode[0];
                //data.DESCRIPTION = SplitItemCode[1];

                if (fun.InsertFIPMaster(data))
                {

                    msg = Validation.str9;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
                else
                {

                    msg = Validation.str2;
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
                //var resul = new { Msg = msg, ID = mstType, validation = status };
                //return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public PartialViewResult Grid(FIPModel data)
        {
            if (string.IsNullOrEmpty(data.PLANT_CODE) || string.IsNullOrEmpty(data.FAMILY_CODE))
            {
                return PartialView();
            }

            string query = string.Format(@"SELECT AUTOID, PLANT_CODE, FAMILY_CODE, ITEM_CODE, DESCRIPTION, MODEL_CODE_NO, 
                                        TO_CHAR(ENTRYDATE, 'DD-MM-YYYY HH24:MI:SS') AS ENTRYDATE, CREATEDBY, TO_CHAR(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS CREATEDDATE, UPDATEDBY, 
                                        TO_CHAR(UPDATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS UPDATEDDATE  FROM XXES_FIPMODEL_CODE 
                                        where PLANT_CODE= '" + data.PLANT_CODE + "' AND FAMILY_CODE= '" + data.FAMILY_CODE + "' ORDER BY AUTOID DESC");
            
            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }

        public JsonResult Delete(FIPModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.AUTOID))
                {
                    msg = Validation.str2;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                if (fun.DeleteFIP(data))
                {

                    msg = Validation.str23;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
                else
                {
                    msg = Validation.str2;
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
                //var resul = new { Msg = msg, ID = mstType, validation = status };
                //return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}