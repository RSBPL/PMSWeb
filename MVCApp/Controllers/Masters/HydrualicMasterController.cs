using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class HydrualicMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();

        string query = ""; string ORGID = "";

        // GET: HydrualicMaster
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
        public JsonResult BindItemCode(HydrualicMaster data)
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
        public JsonResult BindShortCode(HydrualicMaster data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.SHORT_CODE.Trim().ToUpper(), data.SHORT_CODE.Trim().ToUpper());


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
        public JsonResult BindSpoolValue(HydrualicMaster data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.SPOOL_VALUE.Trim().ToUpper(), data.SPOOL_VALUE.Trim().ToUpper());


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
        public JsonResult BindCylinder(HydrualicMaster data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CYLINDER.Trim().ToUpper(), data.CYLINDER.Trim().ToUpper());


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
        public JsonResult BindPart1(HydrualicMaster data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.PART1.Trim().ToUpper(), data.PART1.Trim().ToUpper());


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
        public JsonResult BindPart2(HydrualicMaster data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.PART2.Trim().ToUpper(), data.PART2.Trim().ToUpper());


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


        public JsonResult SaveHydrualicMaster(HydrualicMaster data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = "Please Select Family";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.ITEM_CODE))
                {
                    msg = "Please Select Item Code";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.SHORT_CODE))
                {
                    msg = "Please Select Short Code";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.SPOOL_VALUE))
                {
                    msg = "Please Select Spool Value";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.CYLINDER))
                {
                    msg = "Please Select Cylinder";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.PART1))
                {
                    msg = "Please Select Part1";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.PART2))
                {
                    msg = "Please Select Part2";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


                string stageValue = data.ITEM_CODE.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM  XXES_HYDRUALIC_MASTER WHERE PLANT_CODE = '" + data.PLANT_CODE.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.FAMILY_CODE.ToUpper().Trim() + "' AND ITEM_CODE = '" + subs[0].ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    //status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.InsertHydrualicMaster(data))
                {
                    msg = "Saved successfully...";
                    mstType = "alert-success";
                    status = "success";

                }


            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateHydrualicMaster(HydrualicMaster data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = "Please Fill Plant Code ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = "Please Fill Family Code ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.UpdateHydrualicMaster(data))
                {
                    msg = "Update successfully...";
                    mstType = "alert-success";
                    status = "success";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);

            }

            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public PartialViewResult HydrualicMasterGrid(HydrualicMaster data)
        {
            if (!string.IsNullOrEmpty(data.PLANT_CODE) && !string.IsNullOrEmpty(data.FAMILY_CODE))
            {
                ViewBag.DataSource = fun.HydrualicMasterGrid(data);
            }
            return PartialView();
        }

        public JsonResult DeleteHydrualicMaster(HydrualicMaster data)
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

                if (fun.DeleteHydrualicMaster(data))
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