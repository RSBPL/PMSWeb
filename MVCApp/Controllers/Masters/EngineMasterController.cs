using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web;

namespace MVCApp.Controllers.Masters
{
    public class EngineMasterController : Controller
    {
        string msg = string.Empty; string mstType = string.Empty;
        Function fun = new Function();

        // GET: EngineMaster
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
        public JsonResult BindEngine(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.Engine.Trim().ToUpper(), data.Engine.Trim().ToUpper());


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
        public JsonResult BindFualPump(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.FUEL_INJECTION_PUMP.Trim().ToUpper(), data.FUEL_INJECTION_PUMP.Trim().ToUpper());


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
        public JsonResult BindCylinderBlock(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CYLINDER_BLOCK.Trim().ToUpper(), data.CYLINDER_BLOCK.Trim().ToUpper());


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
        public JsonResult BindCylinderHead(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CYLINDER_HEAD.Trim().ToUpper(), data.CYLINDER_HEAD.Trim().ToUpper());


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
        public JsonResult Bind_Connecting_Rod(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CONNECTING_ROD.Trim().ToUpper(), data.CONNECTING_ROD.Trim().ToUpper());


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
        public JsonResult BindCrackShaft(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CRANK_SHAFT.Trim().ToUpper(), data.CRANK_SHAFT.Trim().ToUpper());


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
        public JsonResult BindCamShaft(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.CAM_SHAFT.Trim().ToUpper(), data.CAM_SHAFT.Trim().ToUpper());


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
        public JsonResult BindECU(EngineModel data)
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
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' )order by segment1", data.ECU.Trim().ToUpper(), data.ECU.Trim().ToUpper());


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
        
        public JsonResult SaveEngineMaster(EngineModel data) 
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
                if (string.IsNullOrEmpty(data.Engine))
                {
                    msg = "Please Select Engine";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string stageValue = data.Engine.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM  XXES_ENGINE_MASTER WHERE PLANT_CODE = '" + data.PLANT_CODE.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.FAMILY_CODE.ToUpper().Trim() + "' AND ITEM_CODE = '" + subs[0].ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    //status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Engine))
                {
                    msg = "Please Select Engine";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertEngineMaster(data))
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
        
        public PartialViewResult Grid(EngineModel data)
        {
            if (!string.IsNullOrEmpty(data.PLANT_CODE) && !string.IsNullOrEmpty(data.FAMILY_CODE))
            {
                ViewBag.DataSource = fun.GridEngineMaster(data);
            }
            return PartialView();
        }
        
        public JsonResult Update(EngineModel data)
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
                
                if (fun.UpdateEngineMaster(data))
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
       
        public JsonResult Delete(EngineModel data)
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

                if (fun.DeleteEngineMaster(data))
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
