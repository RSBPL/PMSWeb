using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Web;
using System.Text;

namespace MVCApp.Controllers
{
    [Authorize]
    public class TractorMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        TractorMsterOld tmold = new TractorMsterOld();

        Function fun = new Function();

        string query = ""; string ORGID = "";
        int TrnNo = 0;
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
        public PartialViewResult BindSearchItem(TractorMster obj)
        {
            ViewBag.gleSearch = new SelectList(Fill_Search_Item(obj), "Value", "Text");
            return PartialView();
        }
        public List<DDLTextValue> Fill_Search_Item(TractorMster obj)
        {
            DataTable dt = null;
            List<DDLTextValue> SearchItem = new List<DDLTextValue>();
            try
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    //query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER order by ITEM_CODE";
                    query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER where PLANT_CODE='" + Convert.ToString(obj.Plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(obj.Family) + "' order by ITEM_CODE";
                }
                else
                    query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER where Plant_code='" + Convert.ToString(obj.Plant).Trim() + "' and family_code='" + Convert.ToString(obj.Family) + "' order by ITEM_CODE";
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        SearchItem.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return SearchItem;
            }
            catch { }
            finally { }
            return SearchItem;
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
                ViewBag.Family = new SelectList(fun.Fill_FamilyOnlyTractor(Plant), "Value", "Text");
            }
            return PartialView();
        }

        //public PartialViewResult BindT3_Plant()
        //{
        //    ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
        //    return PartialView();
        //}
        public PartialViewResult BindT3_Family(string Plant)
        {
            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }
        public JsonResult Bind_T3_Item(TractorMster obj)
        {
            string text = string.Empty, value = string.Empty, family = string.Empty;
            List<DDLTextValue> Items = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(obj.T3_Plant) && !string.IsNullOrEmpty(obj.T3_Family))
                {
                    if (Convert.ToString(obj.T3_Family).ToUpper().Contains("ENGINE"))
                    {
                        text = "ENGINE_DESCRIPTION || ' (' || ENGINE || ')'";
                        value = "ENGINE";
                    }
                    else if (Convert.ToString(obj.T3_Family).ToUpper().Contains("TRANSMISSION"))
                    {
                        text = "TRANSMISSION_DESCRIPTION || ' (' || TRANSMISSION || ')'";
                        value = "TRANSMISSION";
                    }
                    else if (Convert.ToString(obj.T3_Family).ToUpper().Contains("REAR AXLE"))
                    {
                        text = "REARAXLE_DESCRIPTION || ' (' || REARAXLE || ')'";
                        value = "REARAXLE";
                    }
                    else if (Convert.ToString(obj.T3_Family).ToUpper().Contains("HYDRAULIC"))
                    {
                        text = "HYDRAULIC_DESCRIPTION || ' (' || HYDRAULIC || ')'";
                        value = "HYDRAULIC";
                    }
                    else if (Convert.ToString(obj.T3_Family).ToUpper().Contains("BACK END"))
                    {
                        text = "BACKEND_DESCRIPTION || ' (' || BACKEND || ')'";
                        value = "BACKEND";
                    }
                    if (Convert.ToString(obj.T3_Plant) == "T04")
                        family = "TRACTOR FTD";
                    else
                        family = "TRACTOR TD";
                    //if (string.IsNullOrEmpty(text))
                    //{
                    //    this.Cursor = Cursors.Default;
                    //    MessageBox.Show("Invalid family", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

                    //query = string.Format(@"select DISTINCT {0} DESCRIPTION, {1} ITEMCODE FROM xxes_item_master WHERE PLANT_CODE ='{2}' AND FAMILY_CODE='{3}' and {1} is not null", text, value, Convert.ToString(obj.T3_Plant), family);
                    query = string.Format(@"select DISTINCT {0} ITEM_DESCRIPTION, {1} ITEM_CODE FROM xxes_item_master WHERE PLANT_CODE ='{2}' AND FAMILY_CODE='{3}' and {1} is not null", text, value, Convert.ToString(obj.T3_Plant), family);
                    DataTable dt = new DataTable();
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            Items.Add(new DDLTextValue
                            {
                                Text = dr["ITEM_DESCRIPTION"].ToString(),
                                Value = dr["ITEM_CODE"].ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var myResult = new
            {
                res = Items
            };
            JsonResult result = Json(myResult, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }
        public PartialViewResult T3_Grid(TractorMster obj)
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select parameterinfo ITEMCODE,DESCRIPTION from xxes_sft_settings where plant_code='{0}' and family_code='{1}' and status='SRNO'", Convert.ToString(obj.T3_Plant), Convert.ToString(obj.T3_Family));

                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                //hrow;
            }
            ViewBag.DataSource = dt;
            return PartialView();
        }
        public JsonResult Save_T3_SubAssembly_Click(TractorMster obj)
        {
            string msg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(obj.T3_Plant))
                {
                    msg = "Invalid Plant";
                }
                else if (string.IsNullOrEmpty(obj.T3_Family))
                {
                    msg = "Invalid Family";
                }
                else if (string.IsNullOrEmpty(obj.T3_ItemCode_Value))
                {
                    msg = "Invalid Item Code";
                }
                else
                {
                    query = string.Format(@"delete from xxes_sft_settings where plant_code='{0}' and family_code='{1}'and parameterinfo='{2}' and status='SRNO'", Convert.ToString(obj.T3_Plant),
                    Convert.ToString(obj.T3_Family), Convert.ToString(obj.T3_ItemCode_Value));
                    fun.EXEC_QUERY(query);
                    if (obj.T3_chkNotGenrateSrno == true)
                    {
                        string description = obj.T3_ItemCode.Split('(')[0].Trim();
                        query = string.Format(@"insert into xxes_sft_settings (plant_code,family_code,parameterinfo,DESCRIPTION,STATUS)values('{0}','{1}','{2}','{3}','SRNO')", Convert.ToString(obj.T3_Plant), Convert.ToString(obj.T3_Family), Convert.ToString(obj.T3_ItemCode_Value), description);
                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Saved Successfully..";
                        }
                        else
                        {
                            msg = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "Error! " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillItemCode(TractorMster data)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.ItemCode))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('F','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.ItemCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
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
        public JsonResult FillTransMission(TractorMster data)
        {
            List<DDLTextValue> _TransMission = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Transmission))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Transmission.Trim().ToUpper(), data.Transmission.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _TransMission.Add(new DDLTextValue
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
            return Json(_TransMission, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillEngine(TractorMster data)
        {
            List<DDLTextValue> _Engine = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Engine))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Engine.Trim().ToUpper(), data.Engine.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Engine.Add(new DDLTextValue
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
            return Json(_Engine, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRearTyre(TractorMster data)
        {
            List<DDLTextValue> _RearTyre = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RearTyre))
                {

                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RearTyre.Trim().ToUpper(), data.RearTyre.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RearTyre.Add(new DDLTextValue
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
            return Json(_RearTyre, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRHRearTyre(TractorMster data)
        {
            List<DDLTextValue> _RHRearTyre = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RHRearTyre))
                {

                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RHRearTyre.Trim().ToUpper(), data.RHRearTyre.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RHRearTyre.Add(new DDLTextValue
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
            return Json(_RHRearTyre, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillBattery(TractorMster data)
        {
            List<DDLTextValue> _Battery = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Battery))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S','X') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Battery.Trim().ToUpper(), data.Battery.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Battery.Add(new DDLTextValue
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
            return Json(_Battery, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillHydraulicPump(TractorMster data)
        {
            List<DDLTextValue> _HydraulicPump = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.HydraulicPump))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.HydraulicPump.Trim().ToUpper(), data.HydraulicPump.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _HydraulicPump.Add(new DDLTextValue
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
            return Json(_HydraulicPump, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSteeringAssembly(TractorMster data)
        {
            List<DDLTextValue> _SteeringAssembly = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SteeringAssembly))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SteeringAssembly.Trim().ToUpper(), data.SteeringAssembly.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SteeringAssembly.Add(new DDLTextValue
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
            return Json(_SteeringAssembly, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRadiatorAssembly(TractorMster data)
        {
            List<DDLTextValue> _RadiatorAssembly = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RadiatorAssembly))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RadiatorAssembly.Trim().ToUpper(), data.RadiatorAssembly.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RadiatorAssembly.Add(new DDLTextValue
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
            return Json(_RadiatorAssembly, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillAlternator(TractorMster data)
        {
            List<DDLTextValue> _Alternator = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Alternator))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Alternator.Trim().ToUpper(), data.Alternator.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Alternator.Add(new DDLTextValue
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
            return Json(_Alternator, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillBRAKE_PEDAL(TractorMster data)
        {
            List<DDLTextValue> _BRAKE_PEDAL = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.BRAKE_PEDAL))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.BRAKE_PEDAL.Trim().ToUpper(), data.BRAKE_PEDAL.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _BRAKE_PEDAL.Add(new DDLTextValue
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
            return Json(_BRAKE_PEDAL, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillCLUTCH_PEDAL(TractorMster data)
        {
            List<DDLTextValue> _CLUTCH_PEDAL = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.CLUTCH_PEDAL))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.CLUTCH_PEDAL.Trim().ToUpper(), data.CLUTCH_PEDAL.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _CLUTCH_PEDAL.Add(new DDLTextValue
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
            return Json(_CLUTCH_PEDAL, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSPOOL_VALUE(TractorMster data)
        {
            List<DDLTextValue> _SPOOL_VALUE = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SPOOL_VALUE))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SPOOL_VALUE.Trim().ToUpper(), data.SPOOL_VALUE.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SPOOL_VALUE.Add(new DDLTextValue
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
            return Json(_SPOOL_VALUE, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillTANDEM_PUMP(TractorMster data)
        {
            List<DDLTextValue> _TANDEM_PUMP = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.TANDEM_PUMP))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.TANDEM_PUMP.Trim().ToUpper(), data.TANDEM_PUMP.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _TANDEM_PUMP.Add(new DDLTextValue
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
            return Json(_TANDEM_PUMP, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFENDER(TractorMster data)
        {
            List<DDLTextValue> _FENDER = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FENDER))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FENDER.Trim().ToUpper(), data.FENDER.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FENDER.Add(new DDLTextValue
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
            return Json(_FENDER, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRearAxel(TractorMster data)
        {
            List<DDLTextValue> _RearAxel = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RearAxel))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RearAxel.Trim().ToUpper(), data.RearAxel.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RearAxel.Add(new DDLTextValue
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
            return Json(_RearAxel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillHydraulic(TractorMster data)
        {
            List<DDLTextValue> _Hydraulic = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Hydraulic))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Hydraulic.Trim().ToUpper(), data.Hydraulic.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Hydraulic.Add(new DDLTextValue
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
            return Json(_Hydraulic, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFrontTyre(TractorMster data)
        {
            List<DDLTextValue> _FrontTyre = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FrontTyre))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FrontTyre.Trim().ToUpper(), data.FrontTyre.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FrontTyre.Add(new DDLTextValue
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
            return Json(_FrontTyre, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRHFrontTyre(TractorMster data)
        {
            List<DDLTextValue> _RHFrontTyre = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RHFrontTyre))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RHFrontTyre.Trim().ToUpper(), data.RHFrontTyre.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RHFrontTyre.Add(new DDLTextValue
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
            return Json(_RHFrontTyre, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillBackend(TractorMster data)
        {
            List<DDLTextValue> _Backend = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Backend))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Backend.Trim().ToUpper(), data.Backend.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Backend.Add(new DDLTextValue
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
            return Json(_Backend, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSteeringMotor(TractorMster data)
        {
            List<DDLTextValue> _SteeringMotor = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SteeringMotor))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SteeringMotor.Trim().ToUpper(), data.SteeringMotor.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SteeringMotor.Add(new DDLTextValue
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
            return Json(_SteeringMotor, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSteeringCylinder(TractorMster data)
        {
            List<DDLTextValue> _SteeringCylinder = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SteeringCylinder))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SteeringCylinder.Trim().ToUpper(), data.SteeringCylinder.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SteeringCylinder.Add(new DDLTextValue
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
            return Json(_SteeringCylinder, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillClusterAssembly(TractorMster data)
        {
            List<DDLTextValue> _ClusterAssembly = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.ClusterAssembly))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.ClusterAssembly.Trim().ToUpper(), data.ClusterAssembly.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _ClusterAssembly.Add(new DDLTextValue
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
            return Json(_ClusterAssembly, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillStartorMotor(TractorMster data)
        {
            List<DDLTextValue> _StartorMotor = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.StartorMotor))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.StartorMotor.Trim().ToUpper(), data.StartorMotor.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _StartorMotor.Add(new DDLTextValue
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
            return Json(_StartorMotor, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRops(TractorMster data)
        {
            List<DDLTextValue> _Rops = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Rops))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Rops.Trim().ToUpper(), data.Rops.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Rops.Add(new DDLTextValue
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
            return Json(_Rops, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFENDER_RAILING(TractorMster data)
        {
            List<DDLTextValue> _FENDER_RAILING = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FENDER_RAILING))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FENDER_RAILING.Trim().ToUpper(), data.FENDER_RAILING.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FENDER_RAILING.Add(new DDLTextValue
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
            return Json(_FENDER_RAILING, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillHEAD_LAMP(TractorMster data)
        {
            List<DDLTextValue> _HEAD_LAMP = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.HEAD_LAMP))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.HEAD_LAMP.Trim().ToUpper(), data.HEAD_LAMP.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _HEAD_LAMP.Add(new DDLTextValue
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
            return Json(_HEAD_LAMP, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSTEERING_WHEEL(TractorMster data)
        {
            List<DDLTextValue> _STEERING_WHEEL = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.STEERING_WHEEL))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.STEERING_WHEEL.Trim().ToUpper(), data.STEERING_WHEEL.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _STEERING_WHEEL.Add(new DDLTextValue
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
            return Json(_STEERING_WHEEL, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillREAR_HOOD_WIRING_HARNESS(TractorMster data)
        {
            List<DDLTextValue> _REAR_HOOD_WIRING_HARNESS = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.REAR_HOOD_WIRING_HARNESS))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.REAR_HOOD_WIRING_HARNESS.Trim().ToUpper(), data.REAR_HOOD_WIRING_HARNESS.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _REAR_HOOD_WIRING_HARNESS.Add(new DDLTextValue
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
            return Json(_REAR_HOOD_WIRING_HARNESS, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSEAT(TractorMster data)
        {
            List<DDLTextValue> _SEAT = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SEAT))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SEAT.Trim().ToUpper(), data.SEAT.Trim().ToUpper());


                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SEAT.Add(new DDLTextValue
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
            return Json(_SEAT, JsonRequestBehavior.AllowGet);
        }
        /////////////////////////////////////////////////////////////

        public JsonResult gleSearch_EditValueChanged(TractorMster obj)
        {
            string msg = string.Empty;
            TractorMster tm = new TractorMster();
            try
            {

                if (!string.IsNullOrEmpty(obj.gleSearch) && !string.IsNullOrEmpty(obj.Plant) && !string.IsNullOrEmpty(obj.Family))
                {
                    query = @"select * from XXES_ITEM_MASTER where ITEM_CODE='" + obj.gleSearch.ToString().Trim() + "' and Plant_code='" + Convert.ToString(obj.Plant).Trim() + "'  and Family_code='" + Convert.ToString(obj.Family).Trim() + "'  order by FAMILY_CODE";
                    DataTable dt = new DataTable();
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        tm.Plant = Convert.ToString(dt.Rows[0]["PLANT_CODE"]);
                        tm.Family = Convert.ToString(dt.Rows[0]["FAMILY_CODE"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        {
                            tm.ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]) + " # " + Convert.ToString(dt.Rows[0]["item_description"]);
                        }
                        //if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        //{
                        //    tm.T4_ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]) + " # " + Convert.ToString(dt.Rows[0]["item_description"]);
                        //}
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ENGINE"])))
                        {
                            tm.Engine = Convert.ToString(dt.Rows[0]["ENGINE"]) + " # " + Convert.ToString(dt.Rows[0]["engine_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BACKEND"])))
                        {
                            tm.Backend = Convert.ToString(dt.Rows[0]["BACKEND"]) + " # " + Convert.ToString(dt.Rows[0]["backend_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TRANSMISSION"])))
                        {
                            tm.Transmission = Convert.ToString(dt.Rows[0]["TRANSMISSION"]) + " # " + Convert.ToString(dt.Rows[0]["transmission_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REARAXEL"])))
                        {
                            tm.RearAxel = Convert.ToString(dt.Rows[0]["REARAXEL"]) + " # " + Convert.ToString(dt.Rows[0]["rearaxel_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Hydraulic"])))
                        {
                            tm.Hydraulic = Convert.ToString(dt.Rows[0]["Hydraulic"]) + " # " + Convert.ToString(dt.Rows[0]["hydraulic_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FrontTyre"])))
                        {
                            tm.FrontTyre = Convert.ToString(dt.Rows[0]["FrontTyre"]) + " # " + Convert.ToString(dt.Rows[0]["fronttyre_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"])))
                        {
                            tm.RHFrontTyre = Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"]) + " # " + Convert.ToString(dt.Rows[0]["RH_FRONTTYRE_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RearTyre"])))
                        {
                            tm.RearTyre = Convert.ToString(dt.Rows[0]["RearTyre"]) + " # " + Convert.ToString(dt.Rows[0]["reartyre_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RH_REARTYRE"])))
                        {
                            tm.RHRearTyre = Convert.ToString(dt.Rows[0]["RH_REARTYRE"]) + " # " + Convert.ToString(dt.Rows[0]["RH_REARTYRE_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BATTERY"])))
                        {
                            tm.Battery = Convert.ToString(dt.Rows[0]["BATTERY"]) + " # " + Convert.ToString(dt.Rows[0]["battery_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"])))
                        {
                            tm.Rops = Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"]);
                        }
                        tm.Prefix1 = Convert.ToString(dt.Rows[0]["Prefix_1"]);
                        tm.Prefix2 = Convert.ToString(dt.Rows[0]["Prefix_2"]);
                        tm.Prefix3 = Convert.ToString(dt.Rows[0]["Prefix_3"]);
                        tm.Prefix4 = Convert.ToString(dt.Rows[0]["Prefix_4"]);
                        tm.Remarks = Convert.ToString(dt.Rows[0]["remarks"]);
                        tm.ShortDesc = Convert.ToString(dt.Rows[0]["SHORT_CODE"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HYD_PUMP"])))
                        {
                            tm.HydraulicPump = Convert.ToString(dt.Rows[0]["HYD_PUMP"]) + " # " + Convert.ToString(dt.Rows[0]["hyd_pump_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Steering_Assembly"])))
                        {
                            tm.SteeringAssembly = Convert.ToString(dt.Rows[0]["Steering_Assembly"]) + " # " + Convert.ToString(dt.Rows[0]["steering_assembly_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Radiator"])))
                        {
                            tm.RadiatorAssembly = Convert.ToString(dt.Rows[0]["Radiator"]) + " # " + Convert.ToString(dt.Rows[0]["radiator_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ALTERNATOR"])))
                        {
                            tm.Alternator = Convert.ToString(dt.Rows[0]["ALTERNATOR"]) + " # " + Convert.ToString(dt.Rows[0]["alternator_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BRAKE_PEDAL"])))
                        {
                            tm.BRAKE_PEDAL = Convert.ToString(dt.Rows[0]["BRAKE_PEDAL"]) + " # " + Convert.ToString(dt.Rows[0]["brake_pedal_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUTCH_PEDAL"])))
                        {
                            tm.CLUTCH_PEDAL = Convert.ToString(dt.Rows[0]["CLUTCH_PEDAL"]) + " # " + Convert.ToString(dt.Rows[0]["clutch_pedal_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SPOOL_VALUE"])))
                        {
                            tm.SPOOL_VALUE = Convert.ToString(dt.Rows[0]["SPOOL_VALUE"]) + " # " + Convert.ToString(dt.Rows[0]["spool_value_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TANDEM_PUMP"])))
                        {
                            tm.TANDEM_PUMP = Convert.ToString(dt.Rows[0]["TANDEM_PUMP"]) + " # " + Convert.ToString(dt.Rows[0]["tandem_pump_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER"])))
                        {
                            tm.FENDER = Convert.ToString(dt.Rows[0]["FENDER"]) + " # " + Convert.ToString(dt.Rows[0]["fender_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_MOTOR"])))
                        {
                            tm.SteeringMotor = Convert.ToString(dt.Rows[0]["STEERING_MOTOR"]) + " # " + Convert.ToString(dt.Rows[0]["steering_motor_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STERING_CYLINDER"])))
                        {
                            tm.SteeringCylinder = Convert.ToString(dt.Rows[0]["STERING_CYLINDER"]) + " # " + Convert.ToString(dt.Rows[0]["stering_cylinder_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUSSTER"])))
                        {
                            tm.ClusterAssembly = Convert.ToString(dt.Rows[0]["CLUSSTER"]) + " # " + Convert.ToString(dt.Rows[0]["clusster_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STARTER_MOTOR"])))
                        {
                            tm.StartorMotor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR"]) + " # " + Convert.ToString(dt.Rows[0]["starter_motor_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_RAILING"])))
                        {
                            tm.FENDER_RAILING = Convert.ToString(dt.Rows[0]["FENDER_RAILING"]) + " # " + Convert.ToString(dt.Rows[0]["fender_railing_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP"])))
                        {
                            tm.HEAD_LAMP = Convert.ToString(dt.Rows[0]["HEAD_LAMP"]) + " # " + Convert.ToString(dt.Rows[0]["head_lamp_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_WHEEL"])))
                        {
                            tm.STEERING_WHEEL = Convert.ToString(dt.Rows[0]["STEERING_WHEEL"]) + " # " + Convert.ToString(dt.Rows[0]["steering_wheel_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_HOOD_WIRING_HARNESS"])))
                        {
                            tm.REAR_HOOD_WIRING_HARNESS = Convert.ToString(dt.Rows[0]["REAR_HOOD_WIRING_HARNESS"]) + " # " + Convert.ToString(dt.Rows[0]["rear_hood_wiring_harnesse_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SEAT"])))
                        {
                            tm.SEAT = Convert.ToString(dt.Rows[0]["SEAT"]) + " # " + Convert.ToString(dt.Rows[0]["seat_description"]);
                        }

                        if (Convert.ToString(dt.Rows[0]["REQ_CAREBTN"]) == "Y")
                            tm.EnableCarButtonChk = true;
                        else
                            tm.EnableCarButtonChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y")
                            tm.EngineChk = true;
                        else
                            tm.EngineChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y")
                            tm.TransmissionChk = true;
                        else
                            tm.TransmissionChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y")
                            tm.RearAxelChk = true;
                        else
                            tm.RearAxelChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y")
                            tm.BackendChk = true;
                        else
                            tm.BackendChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y")
                            tm.HydraulicChk = true;
                        else
                            tm.HydraulicChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y")
                            tm.RearTyreChk = true;
                        else
                            tm.RearTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y")
                            tm.FrontTyreChk = true;
                        else
                            tm.FrontTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_BATTERY"]) == "Y")
                            tm.BatteryChk = true;
                        else
                            tm.BatteryChk = false;
                        if (Convert.ToString(dt.Rows[0]["GEN_SRNO"]) == "Y")
                            tm.GenerateSerialNoChk = true;
                        else
                            tm.GenerateSerialNoChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y")
                            tm.RopsChk = true;
                        else
                            tm.RopsChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y")
                            tm.HydraulicPumpChk = true;
                        else
                            tm.HydraulicPumpChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y")
                            tm.SteeringAssemblyChk = true;
                        else
                            tm.SteeringAssemblyChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_RADIATOR"]) == "Y")
                            tm.RadiatorAssemblyChk = true;
                        else
                            tm.RadiatorAssemblyChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y")
                            tm.AlternatorChk = true;
                        else
                            tm.AlternatorChk = false;

                        if (Convert.ToString(dt.Rows[0]["SEQ_NOT_REQUIRE"]) == "Y")
                            tm.Seq_Not_RequireChk = true;
                        else
                            tm.Seq_Not_RequireChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y")
                            tm.SteeringMotorChk = true;
                        else
                            tm.SteeringMotorChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STERING_CYLINDER"]) == "Y")
                            tm.SteeringCylinderChk = true;
                        else
                            tm.SteeringCylinderChk = false;



                        if (Convert.ToString(dt.Rows[0]["REQ_CLUSSTER"]) == "Y")
                            tm.ClusterAssemblyChk = true;
                        else
                            tm.ClusterAssemblyChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y")
                            tm.StartorMotorChk = true;
                        else
                            tm.StartorMotorChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y")
                            tm.RopsChk = true;
                        else
                            tm.RopsChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_RHFT"]) == "Y")
                            tm.RHFrontTyreChk = true;
                        else
                            tm.RHFrontTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_RHRT"]) == "Y")
                            tm.RHRearTyreChk = true;
                        else
                            tm.RHRearTyreChk = false;



                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["MODEL_TYPE"])))
                        {
                            if (Convert.ToString(dt.Rows[0]["MODEL_TYPE"]).ToUpper() == "DOMESTIC")
                                tm.DomesticExport = "Domestic";
                            else if (Convert.ToString(dt.Rows[0]["MODEL_TYPE"]).ToUpper() == "EXPORT")
                                tm.DomesticExport = "Export";
                        }
                        else
                        {
                            tm.DomesticExport = "";
                        }
                        tm.NoOfBoltsFrontAxel = Convert.ToString(dt.Rows[0]["FRONTAXEL_BOLTVALUE"]);
                        tm.NoOfBoltsHydraulic = Convert.ToString(dt.Rows[0]["HYDRAULIC_BOLTVALUE"]);
                        tm.NoOfBoltsFrontTYre = Convert.ToString(dt.Rows[0]["FRONTTYRE_BOLTVALUE"]);
                        tm.NoOfBoltsRearTYre = Convert.ToString(dt.Rows[0]["REARTYRE_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe1 = Convert.ToString(dt.Rows[0]["EN_TORQUE1_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe2 = Convert.ToString(dt.Rows[0]["EN_TORQUE2_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe3 = Convert.ToString(dt.Rows[0]["EN_TORQUE3_BOLTVALUE"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"])) && Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).IndexOf("#") > 0)
                        {
                            tm.NoOfBoltsTRANSAXELToruqe1 = Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).Split('#')[0].Trim();
                            tm.NoOfBoltsTRANSAXELToruqe2 = Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).Split('#')[1].Trim();
                        }
                        tmold = JsonConvert.DeserializeObject<TractorMsterOld>(JsonConvert.SerializeObject(tm));
                    }


                    //msg = "";
                }
                else
                {
                    //msg = "Item Not Found";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }
            var myResult = new
            {
                Result = tm,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }
        public string[] StringSpliter(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string Strr = str;
                string[] StrrArr = Strr.Split('#');
                return StrrArr;
            }
            else
            {
                string[] StrrArr = { "", "" };
                return StrrArr;
            }
        }
        public JsonResult Save(TractorMster obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;

            try
            {
                #region Validation
                if (string.IsNullOrEmpty(obj.Plant))
                {
                    msg = "Select Plant to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(obj.Family))
                {
                    msg = "Select family to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(obj.ItemCode))
                {
                    msg = "Select item to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string[] item = StringSpliter(obj.ItemCode);
                    obj.ItemCode = item[0].Trim();
                    obj.ItemCode_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Engine))
                {
                    string[] item = StringSpliter(obj.Engine);
                    obj.Engine = item[0].Trim();
                    obj.Engine_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Backend))
                {
                    string[] item = StringSpliter(obj.Backend);
                    obj.Backend = item[0].Trim();
                    obj.Backend_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Transmission))
                {
                    string[] item = StringSpliter(obj.Transmission);
                    obj.Transmission = item[0].Trim();
                    obj.Transmission_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RearAxel))
                {
                    string[] item = StringSpliter(obj.RearAxel);
                    obj.RearAxel = item[0].Trim();
                    obj.RearAxel_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Hydraulic))
                {
                    string[] item = StringSpliter(obj.Hydraulic);
                    obj.Hydraulic = item[0].Trim();
                    obj.Hydraulic_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FrontTyre))
                {
                    string[] item = StringSpliter(obj.FrontTyre);
                    obj.FrontTyre = item[0].Trim();
                    obj.FrontTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RHFrontTyre))
                {
                    string[] item = StringSpliter(obj.RHFrontTyre);
                    obj.RHFrontTyre = item[0].Trim();
                    obj.RHFrontTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RearTyre))
                {
                    string[] item = StringSpliter(obj.RearTyre);
                    obj.RearTyre = item[0].Trim();
                    obj.RearTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RHRearTyre))
                {
                    string[] item = StringSpliter(obj.RHRearTyre);
                    obj.RHRearTyre = item[0].Trim();
                    obj.RHRearTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Battery))
                {
                    string[] item = StringSpliter(obj.Battery);
                    obj.Battery = item[0].Trim();
                    obj.Battery_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix1))
                {
                    obj.Prefix1 = replaceApostophi(obj.Prefix1.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix2))
                {
                    obj.Prefix2 = replaceApostophi(obj.Prefix2.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix3))
                {
                    obj.Prefix3 = replaceApostophi(obj.Prefix3.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix4))
                {
                    obj.Prefix4 = replaceApostophi(obj.Prefix4.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Remarks))
                {
                    obj.Remarks = replaceApostophi(obj.Remarks.Trim());
                }
                if (!string.IsNullOrEmpty(obj.ShortDesc))
                {
                    obj.ShortDesc = replaceApostophi(obj.ShortDesc.Trim());
                }
                if (!string.IsNullOrEmpty(obj.HydraulicPump))
                {
                    string[] item = StringSpliter(obj.HydraulicPump);
                    obj.HydraulicPump = item[0].Trim();
                    obj.HydraulicPump_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringMotor))
                {
                    string[] item = StringSpliter(obj.SteeringMotor);
                    obj.SteeringMotor = item[0].Trim();
                    obj.SteeringMotor_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringAssembly))
                {
                    string[] item = StringSpliter(obj.SteeringAssembly);
                    obj.SteeringAssembly = item[0].Trim();
                    obj.SteeringAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringCylinder))
                {
                    string[] item = StringSpliter(obj.SteeringCylinder);
                    obj.SteeringCylinder = item[0].Trim();
                    obj.SteeringCylender_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RadiatorAssembly))
                {
                    string[] item = StringSpliter(obj.RadiatorAssembly);
                    obj.RadiatorAssembly = item[0].Trim();
                    obj.RadiatorAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.ClusterAssembly))
                {
                    string[] item = StringSpliter(obj.ClusterAssembly);
                    obj.ClusterAssembly = item[0].Trim();
                    obj.ClusterAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Alternator))
                {
                    string[] item = StringSpliter(obj.Alternator);
                    obj.Alternator = item[0].Trim();
                    obj.Alternator_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.StartorMotor))
                {
                    string[] item = StringSpliter(obj.StartorMotor);
                    obj.StartorMotor = item[0].Trim();
                    obj.StartorMotor_Desc = replaceApostophi(item[1].Trim());
                }

                //DefaultAtPlanning = obj.DefaultAtPlanning;

                //obj.DomesticExport = obj.DomesticExport.Trim();

                if (string.IsNullOrEmpty(obj.NoOfBoltsTRANSAXELToruqe1))
                    obj.NoOfBoltsFrontAxel = "0";
                else
                    obj.BK_BOLT_VALUE = obj.NoOfBoltsTRANSAXELToruqe1.Trim();

                if (string.IsNullOrEmpty(obj.NoOfBoltsTRANSAXELToruqe2))
                    obj.BK_BOLT_VALUE += "#0";
                else
                    obj.BK_BOLT_VALUE += "#" + obj.NoOfBoltsTRANSAXELToruqe1.Trim();


                if (!string.IsNullOrEmpty(obj.Rops))
                {
                    string[] item = StringSpliter(obj.Rops);
                    obj.Rops = item[0].Trim();
                    if (item.Length > 1)
                    {
                        obj.Rops_Desc = replaceApostophi(item[1].Trim());
                    }

                }

                if (!string.IsNullOrEmpty(obj.BRAKE_PEDAL))
                {
                    string[] item = StringSpliter(obj.BRAKE_PEDAL);
                    obj.BRAKE_PEDAL = item[0].Trim();
                    obj.BrakePedal_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.CLUTCH_PEDAL))
                {
                    string[] item = StringSpliter(obj.CLUTCH_PEDAL);
                    obj.CLUTCH_PEDAL = item[0].Trim();
                    obj.ClutchPedal_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SPOOL_VALUE))
                {
                    string[] item = StringSpliter(obj.SPOOL_VALUE);
                    obj.SPOOL_VALUE = item[0].Trim();
                    obj.SpoolValue_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.TANDEM_PUMP))
                {
                    string[] item = StringSpliter(obj.TANDEM_PUMP);
                    obj.TANDEM_PUMP = item[0].Trim();
                    obj.TandemPump_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FENDER))
                {
                    string[] item = StringSpliter(obj.FENDER);
                    obj.FENDER = item[0].Trim();
                    obj.Fender_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FENDER_RAILING))
                {
                    string[] item = StringSpliter(obj.FENDER_RAILING);
                    obj.FENDER_RAILING = item[0].Trim();
                    obj.FenderRailing_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.HEAD_LAMP))
                {
                    string[] item = StringSpliter(obj.HEAD_LAMP);
                    obj.HEAD_LAMP = item[0].Trim();
                    obj.HeadLamp_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.STEERING_WHEEL))
                {
                    string[] item = StringSpliter(obj.STEERING_WHEEL);
                    obj.STEERING_WHEEL = item[0].Trim();
                    obj.SteeringWheel_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.REAR_HOOD_WIRING_HARNESS))
                {
                    string[] item = StringSpliter(obj.REAR_HOOD_WIRING_HARNESS);
                    obj.REAR_HOOD_WIRING_HARNESS = item[0].Trim();
                    obj.RearHoolWiringHarness_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SEAT))
                {
                    string[] item = StringSpliter(obj.SEAT);
                    obj.SEAT = item[0].Trim();
                    obj.Seat_Desc = replaceApostophi(item[1].Trim());
                }



                #endregion
                //ChangeUpdate(tmold, obj);

                obj.ORG_ID = fun.getOrgId(Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_ITEM_MASTER WHERE PLANT_CODE = '" + obj.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + obj.Family.ToUpper().Trim() + "' AND ITEM_CODE = '" + obj.ItemCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str31;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fun.InsertTractorMaster(obj))
                    {
                        int transactionNo = InsertSave(obj, "TRACTOR_TAB_1");
                        string head = "New Tractor added in MES with following details :- Tractor Code : " + obj.gleSearch + "<br>" + obj.ItemCode_Desc + "";
                        string mailbody = MailBODY(transactionNo, obj.gleSearch, "TRACTOR_TAB_1", head);
                        string mailsend = sendMail("Insert_Tractor", mailbody);
                        query = "select count(*) from ITEM_MASTER where trim(PLANT_CODE)='" + Convert.ToString(obj.Plant) + "' and trim(FAMILY_CODE)='" + Convert.ToString(obj.Family) + "' and trim(ITEM_CODE)='" + Convert.ToString(obj.ItemCode).Trim() + "'";

                        //fun.Insert_Into_ActivityLog("TRACTOR MASTER", "Insert_Update", Convert.ToString(obj.Plant) + " # " + Convert.ToString(obj.Family) + " # " + Convert.ToString(obj.ItemCode), query, Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                        if (!fun.CheckExits(query))
                        {
                            query = @"insert into ITEM_MASTER(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,ENGINE,ENGINE_DESCRIPTION,BACKEND,BACKEND_DESCRIPTION,
                                    TRANSMISSION,TRANSMISSION_DESCRIPTION,REARAXEL,REARAXEL_DESCRIPTION,ORG_ID) 
                                values ('" + Convert.ToString(obj.Plant).Trim() + "','" + Convert.ToString(obj.Family).Trim() + "','" + obj.ItemCode + "','" + obj.ItemCode_Desc + "','" + obj.Engine + "','" + obj.Engine_Desc + "','" + obj.Backend + "','" + obj.Backend_Desc + "','" + obj.Transmission + "','" + obj.Transmission_Desc + "','" + obj.RearAxel + "','" + obj.RearAxel_Desc + "','" + obj.ORG_ID + "')";
                            if (fun.EXEC_QUERY(query))
                            {
                                fun.Insert_Into_ActivityLog("TRACTOR MASTER BI", "Insert_Update", Convert.ToString(obj.Plant) + " # " + Convert.ToString(obj.Family) + " # " + Convert.ToString(obj.ItemCode), query, Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                            }
                            msg = "Saved successfully...";
                        }
                        msg = "Saved successfully...";

                    }
                    else
                    {
                        msg = "Error found while mapping of Item !!";
                    }

                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        public TractorMsterOld EXISTDATA(TractorMster obj)
        {
            if (obj.ItemCode == null)
            {
                obj.ItemCode = obj.T4_ItemCode;
            }
            if (obj.Plant == null)
            {
                obj.Plant = obj.T4_Plant;
            }
            if (obj.Family == null)
            {
                obj.Family = obj.T4_Family;
            }
            string msg = string.Empty;
            TractorMster tm = new TractorMster();
            try
            {
                if (!string.IsNullOrEmpty(obj.ItemCode) && !string.IsNullOrEmpty(obj.Plant) && !string.IsNullOrEmpty(obj.Family))
                {
                    query = @"select * from XXES_ITEM_MASTER where ITEM_CODE='" + obj.ItemCode.ToString().Trim() + "' and Plant_code='" + Convert.ToString(obj.Plant).Trim() + "'  and Family_code='" + Convert.ToString(obj.Family).Trim() + "'  order by FAMILY_CODE";
                    DataTable dt = new DataTable();
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        tm.Plant = Convert.ToString(dt.Rows[0]["PLANT_CODE"]);
                        tm.Family = Convert.ToString(dt.Rows[0]["FAMILY_CODE"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        {
                            tm.ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                            tm.ItemCode_Desc = Convert.ToString(dt.Rows[0]["item_description"]);
                        }
                        //if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        //{
                        //    tm.T4_ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]) + " # " + Convert.ToString(dt.Rows[0]["item_description"]);
                        //}
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        {
                            tm.T4_ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                            tm.T4_ItemCode_Desc = Convert.ToString(dt.Rows[0]["item_description"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_SUPPORT"])))
                        {
                            tm.FrontSupport = Convert.ToString(dt.Rows[0]["FRONT_SUPPORT"]);
                            tm.FrontSupport_Desc = Convert.ToString(dt.Rows[0]["FRONT_SUPPORT_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CENTER_AXEL"])))
                        {
                            tm.CenterAxel = Convert.ToString(dt.Rows[0]["CENTER_AXEL"]);
                            tm.CenterAxel_Desc = Convert.ToString(dt.Rows[0]["CENTER_AXEL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SLIDER"])))
                        {
                            tm.Slider = Convert.ToString(dt.Rows[0]["SLIDER"]);
                            tm.Slider_Desc = Convert.ToString(dt.Rows[0]["SLIDER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_COLUMN"])))
                        {
                            tm.SteeringColumn = Convert.ToString(dt.Rows[0]["STEERING_COLUMN"]);
                            tm.SteeringColumn_Desc = Convert.ToString(dt.Rows[0]["STEERING_COLUMN_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_BASE"])))
                        {
                            tm.SteeringBase = Convert.ToString(dt.Rows[0]["STEERING_BASE"]);
                            tm.SteeringBase_Desc = Convert.ToString(dt.Rows[0]["STEERING_BASE_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["LOWER_LINK"])))
                        {
                            tm.Lowerlink = Convert.ToString(dt.Rows[0]["LOWER_LINK"]);
                            tm.Lowerlink_Desc = Convert.ToString(dt.Rows[0]["LOWER_LINK_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RB_FRAME"])))
                        {
                            tm.RBFrame = Convert.ToString(dt.Rows[0]["RB_FRAME"]);
                            tm.RBFrame_Desc = Convert.ToString(dt.Rows[0]["RB_FRAME_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FUEL_TANK"])))
                        {
                            tm.FuelTank = Convert.ToString(dt.Rows[0]["FUEL_TANK"]);
                            tm.FuelTank_Desc = Convert.ToString(dt.Rows[0]["FUEL_TANK_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CYLINDER"])))
                        {
                            tm.Cylinder = Convert.ToString(dt.Rows[0]["CYLINDER"]);
                            tm.Cylinder_Desc = Convert.ToString(dt.Rows[0]["CYLINDER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_RH"])))
                        {
                            tm.FenderRH = Convert.ToString(dt.Rows[0]["FENDER_RH"]);
                            tm.FenderRH_Desc = Convert.ToString(dt.Rows[0]["FENDER_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_LH"])))
                        {
                            tm.FenderLH = Convert.ToString(dt.Rows[0]["FENDER_LH"]);
                            tm.FenderLH_Desc = Convert.ToString(dt.Rows[0]["FENDER_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH"])))
                        {
                            tm.FenderHarnessRH = Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH"]);
                            tm.FenderHarnessRH_Desc = Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_LAMP"])))
                        {
                            tm.FenderLamp4Types = Convert.ToString(dt.Rows[0]["FENDER_LAMP"]);
                            tm.FenderLamp4Types_Desc = Convert.ToString(dt.Rows[0]["FENDER_LAMP_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RB_HARNESS_LH"])))
                        {
                            tm.RBHarnessLH = Convert.ToString(dt.Rows[0]["RB_HARNESS_LH"]);
                            tm.RBHarnessLH_Desc = Convert.ToString(dt.Rows[0]["RB_HARNESS_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_RIM"])))
                        {
                            tm.FrontRim = Convert.ToString(dt.Rows[0]["FRONT_RIM"]);
                            tm.FrontRim_Desc = Convert.ToString(dt.Rows[0]["FRONT_RIM_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_RIM"])))
                        {
                            tm.RearRim = Convert.ToString(dt.Rows[0]["REAR_RIM"]);
                            tm.RearRim_Desc = Convert.ToString(dt.Rows[0]["REAR_RIM_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TYRE_MAKE"])))
                        {
                            tm.TyreMake = Convert.ToString(dt.Rows[0]["TYRE_MAKE"]);
                            tm.TyreMake_Desc = Convert.ToString(dt.Rows[0]["TYRE_MAKE_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_HOOD"])))
                        {
                            tm.RearHood = Convert.ToString(dt.Rows[0]["REAR_HOOD"]);
                            tm.RearHood_Desc = Convert.ToString(dt.Rows[0]["REAR_HOOD_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUSTER_METER"])))
                        {
                            tm.ClusterMeter = Convert.ToString(dt.Rows[0]["CLUSTER_METER"]);
                            tm.ClusterMeter_Desc = Convert.ToString(dt.Rows[0]["CLUSTER_METER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["IP_HARNESS"])))
                        {
                            tm.IPHarness = Convert.ToString(dt.Rows[0]["IP_HARNESS"]);
                            tm.IPHarness_Desc = Convert.ToString(dt.Rows[0]["IP_HARNESS_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RADIATOR_SHELL"])))
                        {
                            tm.RadiatorShell = Convert.ToString(dt.Rows[0]["RADIATOR_SHELL"]);
                            tm.RadiatorShell_Desc = Convert.ToString(dt.Rows[0]["RADIATOR_SHELL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["AIR_CLEANER"])))
                        {
                            tm.AirCleaner = Convert.ToString(dt.Rows[0]["AIR_CLEANER"]);
                            tm.AirCleaner_Desc = Convert.ToString(dt.Rows[0]["AIR_CLEANER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH"])))
                        {
                            tm.HeadLampLH = Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH"]);
                            tm.HeadLampLH_Desc = Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH"])))
                        {
                            tm.HeadLampRH = Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH"]);
                            tm.HeadLampRH_Desc = Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_GRILL"])))
                        {
                            tm.FrontGrill = Convert.ToString(dt.Rows[0]["FRONT_GRILL"]);
                            tm.FrontGrill_Desc = Convert.ToString(dt.Rows[0]["FRONT_GRILL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["MAIN_HARNESS_BONNET"])))
                        {
                            tm.MainHarnessBonnet = Convert.ToString(dt.Rows[0]["MAIN_HARNESS_BONNET"]);
                            tm.MainHarnessBonnet_Desc = Convert.ToString(dt.Rows[0]["MAIN_HARNESSBONNET_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SPINDLE"])))
                        {
                            tm.Spindle = Convert.ToString(dt.Rows[0]["SPINDLE"]);
                            tm.Spindle_Desc = Convert.ToString(dt.Rows[0]["SPINDLE_DESCRIPTION"]);
                        }
                        //----------------------------------Add New Start-----------------------------------------
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SLIDER_RH"])))
                        {
                            tm.Slider_RH = Convert.ToString(dt.Rows[0]["SLIDER_RH"]);
                            tm.Slider_RH_Desc = Convert.ToString(dt.Rows[0]["SLIDER_RH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BRK_PAD"])))
                        {
                            tm.BRK_PAD = Convert.ToString(dt.Rows[0]["BRK_PAD"]);
                            tm.BRK_PAD_DESC = Convert.ToString(dt.Rows[0]["BRK_PAD_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRB_RH"])))
                        {
                            tm.FRB_RH = Convert.ToString(dt.Rows[0]["FRB_RH"]);
                            tm.FRB_RH_DESC = Convert.ToString(dt.Rows[0]["FRB_RH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRB_LH"])))
                        {
                            tm.FRB_LH = Convert.ToString(dt.Rows[0]["FRB_LH"]);
                            tm.FRB_LH_DESC = Convert.ToString(dt.Rows[0]["FRB_LH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FR_AS_RB"])))
                        {
                            tm.FR_AS_RB = Convert.ToString(dt.Rows[0]["FR_AS_RB"]);
                            tm.FR_AS_RB_DESC = Convert.ToString(dt.Rows[0]["FR_AS_RB_DESC"]);
                        }

                        if (Convert.ToString(dt.Rows[0]["REQ_FRONTRIM"]) == "Y")
                            tm.FrontRimChk = true;
                        else
                            tm.FrontRimChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_REARRIM"]) == "Y")
                            tm.RearRimChk = true;
                        else
                            tm.RearRimChk = false;
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ENGINE"])))
                        {
                            tm.Engine = Convert.ToString(dt.Rows[0]["ENGINE"]);
                            tm.Engine_Desc = Convert.ToString(dt.Rows[0]["engine_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BACKEND"])))
                        {
                            tm.Backend = Convert.ToString(dt.Rows[0]["BACKEND"]);
                            tm.Backend_Desc = Convert.ToString(dt.Rows[0]["backend_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TRANSMISSION"])))
                        {
                            tm.Transmission = Convert.ToString(dt.Rows[0]["TRANSMISSION"]);
                            tm.Transmission_Desc = Convert.ToString(dt.Rows[0]["transmission_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REARAXEL"])))
                        {
                            tm.RearAxel = Convert.ToString(dt.Rows[0]["REARAXEL"]);
                            tm.RearAxel_Desc = Convert.ToString(dt.Rows[0]["rearaxel_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Hydraulic"])))
                        {
                            tm.Hydraulic = Convert.ToString(dt.Rows[0]["Hydraulic"]);
                            tm.Hydraulic_Desc = Convert.ToString(dt.Rows[0]["hydraulic_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FrontTyre"])))
                        {
                            tm.FrontTyre = Convert.ToString(dt.Rows[0]["FrontTyre"]);
                            tm.FrontTyre_Desc = Convert.ToString(dt.Rows[0]["fronttyre_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"])))
                        {
                            tm.RHFrontTyre = Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"]);
                            tm.RHFrontTyre_Desc = Convert.ToString(dt.Rows[0]["RH_FRONTTYRE_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RearTyre"])))
                        {
                            tm.RearTyre = Convert.ToString(dt.Rows[0]["RearTyre"]);
                            tm.RearTyre_Desc = Convert.ToString(dt.Rows[0]["reartyre_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RH_REARTYRE"])))
                        {
                            tm.RHRearTyre = Convert.ToString(dt.Rows[0]["RH_REARTYRE"]);
                            tm.RHRearTyre_Desc = Convert.ToString(dt.Rows[0]["RH_REARTYRE_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BATTERY"])))
                        {
                            tm.Battery = Convert.ToString(dt.Rows[0]["BATTERY"]);
                            tm.Battery_Desc = Convert.ToString(dt.Rows[0]["battery_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"])))
                        {
                            tm.Rops = Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"]);
                        }
                        tm.Prefix1 = Convert.ToString(dt.Rows[0]["Prefix_1"]);
                        tm.Prefix2 = Convert.ToString(dt.Rows[0]["Prefix_2"]);
                        tm.Prefix3 = Convert.ToString(dt.Rows[0]["Prefix_3"]);
                        tm.Prefix4 = Convert.ToString(dt.Rows[0]["Prefix_4"]);
                        tm.Remarks = Convert.ToString(dt.Rows[0]["remarks"]);
                        tm.ShortDesc = Convert.ToString(dt.Rows[0]["SHORT_CODE"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HYD_PUMP"])))
                        {
                            tm.HydraulicPump = Convert.ToString(dt.Rows[0]["HYD_PUMP"]);
                            tm.HydraulicPump_Desc = Convert.ToString(dt.Rows[0]["hyd_pump_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Steering_Assembly"])))
                        {
                            tm.SteeringAssembly = Convert.ToString(dt.Rows[0]["Steering_Assembly"]);
                            tm.SteeringAssembly_Desc = Convert.ToString(dt.Rows[0]["steering_assembly_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Radiator"])))
                        {
                            tm.RadiatorAssembly = Convert.ToString(dt.Rows[0]["Radiator"]);
                            tm.RadiatorAssembly_Desc = Convert.ToString(dt.Rows[0]["radiator_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ALTERNATOR"])))
                        {
                            tm.Alternator = Convert.ToString(dt.Rows[0]["ALTERNATOR"]);
                            tm.Alternator_Desc = Convert.ToString(dt.Rows[0]["alternator_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BRAKE_PEDAL"])))
                        {
                            tm.BRAKE_PEDAL = Convert.ToString(dt.Rows[0]["BRAKE_PEDAL"]);
                            tm.BrakePedal_Desc = Convert.ToString(dt.Rows[0]["brake_pedal_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUTCH_PEDAL"])))
                        {
                            tm.CLUTCH_PEDAL = Convert.ToString(dt.Rows[0]["CLUTCH_PEDAL"]);
                            tm.ClutchPedal_Desc = Convert.ToString(dt.Rows[0]["clutch_pedal_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SPOOL_VALUE"])))
                        {
                            tm.SPOOL_VALUE = Convert.ToString(dt.Rows[0]["SPOOL_VALUE"]);
                            tm.SpoolValue_Desc = Convert.ToString(dt.Rows[0]["spool_value_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TANDEM_PUMP"])))
                        {
                            tm.TANDEM_PUMP = Convert.ToString(dt.Rows[0]["TANDEM_PUMP"]);
                            tm.TandemPump_Desc = Convert.ToString(dt.Rows[0]["tandem_pump_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER"])))
                        {
                            tm.FENDER = Convert.ToString(dt.Rows[0]["FENDER"]);
                            tm.Fender_Desc = Convert.ToString(dt.Rows[0]["fender_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_MOTOR"])))
                        {
                            tm.SteeringMotor = Convert.ToString(dt.Rows[0]["STEERING_MOTOR"]);
                            tm.SteeringMotor_Desc = Convert.ToString(dt.Rows[0]["steering_motor_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STERING_CYLINDER"])))
                        {
                            tm.SteeringCylinder = Convert.ToString(dt.Rows[0]["STERING_CYLINDER"]);
                            tm.SteeringCylender_Desc = Convert.ToString(dt.Rows[0]["stering_cylinder_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUSSTER"])))
                        {
                            tm.ClusterAssembly = Convert.ToString(dt.Rows[0]["CLUSSTER"]);
                            tm.ClusterAssembly_Desc = Convert.ToString(dt.Rows[0]["clusster_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STARTER_MOTOR"])))
                        {
                            tm.StartorMotor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR"]);
                            tm.StartorMotor_Desc = Convert.ToString(dt.Rows[0]["starter_motor_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_RAILING"])))
                        {
                            tm.FENDER_RAILING = Convert.ToString(dt.Rows[0]["FENDER_RAILING"]);
                            tm.FenderRailing_Desc = Convert.ToString(dt.Rows[0]["fender_railing_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP"])))
                        {
                            tm.HEAD_LAMP = Convert.ToString(dt.Rows[0]["HEAD_LAMP"]);
                            tm.HeadLamp_Desc = Convert.ToString(dt.Rows[0]["head_lamp_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_WHEEL"])))
                        {
                            tm.STEERING_WHEEL = Convert.ToString(dt.Rows[0]["STEERING_WHEEL"]);
                            tm.SteeringWheel_Desc = Convert.ToString(dt.Rows[0]["steering_wheel_description"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_HOOD_WIRING_HARNESS"])))
                        {
                            tm.REAR_HOOD_WIRING_HARNESS = Convert.ToString(dt.Rows[0]["REAR_HOOD_WIRING_HARNESS"]);
                            tm.RearHoolWiringHarness_Desc = Convert.ToString(dt.Rows[0]["rear_hood_wiring_harnesse_desc"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SEAT"])))
                        {
                            tm.SEAT = Convert.ToString(dt.Rows[0]["SEAT"]);
                            tm.Seat_Desc = Convert.ToString(dt.Rows[0]["seat_description"]);
                        }

                        if (Convert.ToString(dt.Rows[0]["REQ_CAREBTN"]) == "Y")
                            tm.EnableCarButtonChk = true;
                        else
                            tm.EnableCarButtonChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y")
                            tm.EngineChk = true;
                        else
                            tm.EngineChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y")
                            tm.TransmissionChk = true;
                        else
                            tm.TransmissionChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y")
                            tm.RearAxelChk = true;
                        else
                            tm.RearAxelChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y")
                            tm.BackendChk = true;
                        else
                            tm.BackendChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y")
                            tm.HydraulicChk = true;
                        else
                            tm.HydraulicChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y")
                            tm.RearTyreChk = true;
                        else
                            tm.RearTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y")
                            tm.FrontTyreChk = true;
                        else
                            tm.FrontTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQUIRE_BATTERY"]) == "Y")
                            tm.BatteryChk = true;
                        else
                            tm.BatteryChk = false;
                        if (Convert.ToString(dt.Rows[0]["GEN_SRNO"]) == "Y")
                            tm.GenerateSerialNoChk = true;
                        else
                            tm.GenerateSerialNoChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y")
                            tm.RopsChk = true;
                        else
                            tm.RopsChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y")
                            tm.HydraulicPumpChk = true;
                        else
                            tm.HydraulicPumpChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y")
                            tm.SteeringAssemblyChk = true;
                        else
                            tm.SteeringAssemblyChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_RADIATOR"]) == "Y")
                            tm.RadiatorAssemblyChk = true;
                        else
                            tm.RadiatorAssemblyChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y")
                            tm.AlternatorChk = true;
                        else
                            tm.AlternatorChk = false;

                        if (Convert.ToString(dt.Rows[0]["SEQ_NOT_REQUIRE"]) == "Y")
                            tm.Seq_Not_RequireChk = true;
                        else
                            tm.Seq_Not_RequireChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y")
                            tm.SteeringMotorChk = true;
                        else
                            tm.SteeringMotorChk = false;

                        if (Convert.ToString(dt.Rows[0]["REQ_STERING_CYLINDER"]) == "Y")
                            tm.SteeringCylinderChk = true;
                        else
                            tm.SteeringCylinderChk = false;



                        if (Convert.ToString(dt.Rows[0]["REQ_CLUSSTER"]) == "Y")
                            tm.ClusterAssemblyChk = true;
                        else
                            tm.ClusterAssemblyChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y")
                            tm.StartorMotorChk = true;
                        else
                            tm.StartorMotorChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y")
                            tm.RopsChk = true;
                        else
                            tm.RopsChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_RHFT"]) == "Y")
                            tm.RHFrontTyreChk = true;
                        else
                            tm.RHFrontTyreChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_RHRT"]) == "Y")
                            tm.RHRearTyreChk = true;
                        else
                            tm.RHRearTyreChk = false;



                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["MODEL_TYPE"])))
                        {
                            if (Convert.ToString(dt.Rows[0]["MODEL_TYPE"]).ToUpper() == "DOMESTIC")
                                tm.DomesticExport = "Domestic";
                            else if (Convert.ToString(dt.Rows[0]["MODEL_TYPE"]).ToUpper() == "EXPORT")
                                tm.DomesticExport = "Export";
                        }
                        else
                        {
                            tm.DomesticExport = "";
                        }
                        tm.NoOfBoltsFrontAxel = Convert.ToString(dt.Rows[0]["FRONTAXEL_BOLTVALUE"]);
                        tm.NoOfBoltsHydraulic = Convert.ToString(dt.Rows[0]["HYDRAULIC_BOLTVALUE"]);
                        tm.NoOfBoltsFrontTYre = Convert.ToString(dt.Rows[0]["FRONTTYRE_BOLTVALUE"]);
                        tm.NoOfBoltsRearTYre = Convert.ToString(dt.Rows[0]["REARTYRE_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe1 = Convert.ToString(dt.Rows[0]["EN_TORQUE1_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe2 = Convert.ToString(dt.Rows[0]["EN_TORQUE2_BOLTVALUE"]);
                        tm.NoOfBoltsEnToruqe3 = Convert.ToString(dt.Rows[0]["EN_TORQUE3_BOLTVALUE"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"])) && Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).IndexOf("#") > 0)
                        {
                            tm.NoOfBoltsTRANSAXELToruqe1 = Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).Split('#')[0].Trim();
                            tm.NoOfBoltsTRANSAXELToruqe2 = Convert.ToString(dt.Rows[0]["BK_TORQUE_BOLTVALUE"]).Split('#')[1].Trim();
                        }
                        tmold = JsonConvert.DeserializeObject<TractorMsterOld>(JsonConvert.SerializeObject(tm));
                    }


                    //msg = "";
                }
                else
                {
                    //msg = "Item Not Found";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }
            var myResult = new
            {
                Result = tm,
                Msg = msg
            };
            return tmold;
        }
        public int ChangeUpdate(TractorMsterOld Tmold, TractorMster TM, string Tab)
        {

            if (TM.Plant == null)
            {
                TM.Plant = TM.T4_Plant;
            }
            if (TM.Family == null)
            {
                TM.Family = TM.T4_Family;
            }
            string transactionNo = fun.get_Col_Value("SELECT NVL( MAX(TRANSACTION_NUMBER),0)+1 AS TRANSACTION_NUMBER FROM XXES_PARTS_AUDIT_DATA WHERE Remarks1 = 'TRACTOR_TAB_1' AND Remarks2 = 'F00000000'");
            TrnNo = Convert.ToInt32(transactionNo);
            if (Tab == "TRACTOR_TAB_1")
            {
                if (Tmold.ItemCode != TM.ItemCode)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.ItemCode, TM.ItemCode, tmold.ItemCode, tmold.ItemCode_Desc, Tab, TM.gleSearch, TM.ItemCode, TM.ItemCode_Desc, "Change_ITEM", TrnNo);
                }
                if (Tmold.Engine != TM.Engine)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Engine, TM.Engine, tmold.Engine, tmold.Engine_Desc, Tab, TM.gleSearch, TM.Engine, TM.Engine_Desc, "Change_Engine", TrnNo);
                }
                if (Tmold.Backend != TM.Backend)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Backend, TM.Backend, tmold.Backend, tmold.Backend_Desc, Tab, TM.gleSearch, TM.Backend, TM.Backend_Desc, "Change_Backend", TrnNo);
                }
                if (Tmold.Transmission != TM.Transmission)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Transmission, TM.Transmission, tmold.Transmission, tmold.Transmission_Desc, Tab, TM.gleSearch, TM.Transmission, TM.Transmission_Desc, "Change_Transmission", TrnNo);
                }
                if (Tmold.RearAxel != TM.RearAxel)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearAxel, TM.RearAxel, tmold.RearAxel, tmold.RearAxel_Desc, Tab, TM.gleSearch, TM.RearAxel, TM.RearAxel_Desc, "Change_RearAxel", TrnNo);
                }
                if (Tmold.Hydraulic != TM.Hydraulic)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Hydraulic, TM.Hydraulic, tmold.Hydraulic, tmold.Hydraulic_Desc, Tab, TM.gleSearch, TM.Hydraulic, TM.Hydraulic_Desc, "Change_Hydraulic", TrnNo);
                }
                if (Tmold.FrontTyre != TM.FrontTyre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FrontTyre, TM.FrontTyre, tmold.FrontTyre, tmold.FrontTyre_Desc, Tab, TM.gleSearch, TM.FrontTyre, TM.FrontTyre_Desc, "Change_FrontTyre", TrnNo);
                }
                if (Tmold.RHFrontTyre != TM.RHFrontTyre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RHFrontTyre, TM.RHFrontTyre, tmold.RHFrontTyre, tmold.RHFrontTyre_Desc, Tab, TM.gleSearch, TM.RHFrontTyre, TM.RHFrontTyre_Desc, "Change_RHFrontTyre", TrnNo);
                }
                if (Tmold.RearTyre != TM.RearTyre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearTyre, TM.RearTyre, tmold.RearTyre, tmold.RearTyre_Desc, Tab, TM.gleSearch, TM.RearTyre, TM.RearTyre_Desc, "Change_RearTyre", TrnNo);
                }
                if (Tmold.RHRearTyre != TM.RHRearTyre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RHRearTyre, TM.RHRearTyre, tmold.RHRearTyre, tmold.RHRearTyre_Desc, Tab, TM.gleSearch, TM.RHRearTyre, TM.RHRearTyre_Desc, "Change_RHRearTyre", TrnNo);
                }
                if (Tmold.Battery != TM.Battery)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Battery, TM.Battery, tmold.Battery, tmold.Battery_Desc, Tab, TM.gleSearch, TM.Battery, TM.Battery_Desc, "Change_Battery", TrnNo);
                }
                if (Tmold.Prefix1 != TM.Prefix1)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix1, TM.Prefix1, tmold.Prefix1, "", Tab, TM.gleSearch, TM.Prefix1, "", "Change_Prefix1", TrnNo);
                }
                if (Tmold.Prefix2 != TM.Prefix2)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix2, TM.Prefix2, tmold.Prefix2, "", Tab, TM.gleSearch, TM.Prefix2, "", "Change_Prefix2", TrnNo);
                }
                if (TM.Prefix3 == null)
                {
                    TM.Prefix3 = "";
                }
                if (Tmold.Prefix3 != TM.Prefix3)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix3, TM.Prefix3, tmold.Prefix3, "", Tab, TM.gleSearch, TM.Prefix3, "", "Change_Prefix2", TrnNo);
                }
                if (TM.Prefix4==null) {
                    TM.Prefix4 = "";
                }
                if (Tmold.Prefix4 != TM.Prefix4)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix4, TM.Prefix4, tmold.Prefix4, "", Tab, TM.gleSearch, TM.Prefix4, "", "Change_Prefix4", TrnNo);
                }
                if (TM.Remarks == null)
                    TM.Remarks = "";
                if (Tmold.Remarks != TM.Remarks)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Remarks, TM.Remarks, tmold.Remarks, "", Tab, TM.gleSearch, TM.Remarks, "", "Change_Remarks", TrnNo);
                }
                if (Tmold.ShortDesc != TM.ShortDesc && TM.ShortDesc != null)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.ShortDesc, TM.ShortDesc, tmold.ShortDesc, "", Tab, TM.gleSearch, TM.ShortDesc, "", "Change_ShortDesc", TrnNo);
                }
                if (Tmold.HydraulicPump != TM.HydraulicPump)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HydraulicPump, TM.HydraulicPump, tmold.HydraulicPump, tmold.HydraulicPump_Desc, Tab, TM.gleSearch, TM.HydraulicPump, TM.HydraulicPump_Desc, "Change_HydraulicPump", TrnNo);
                }
                if (Tmold.SteeringAssembly != TM.SteeringAssembly)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringAssembly, TM.SteeringAssembly, tmold.SteeringAssembly, tmold.SteeringAssembly_Desc, Tab, TM.gleSearch, TM.SteeringAssembly, TM.SteeringAssembly_Desc, "Change_SteeringAssembly", TrnNo);
                }
                if (Tmold.RadiatorAssembly != TM.RadiatorAssembly)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RadiatorAssembly, TM.RadiatorAssembly, tmold.RadiatorAssembly, tmold.RadiatorAssembly_Desc, Tab, TM.gleSearch, TM.RadiatorAssembly, TM.RadiatorAssembly_Desc, "Change_RadiatorAssembly", TrnNo);
                }
                if (Tmold.Alternator != TM.Alternator)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Alternator, TM.Alternator, tmold.Alternator, tmold.Alternator_Desc, Tab, TM.gleSearch, TM.Alternator, TM.Alternator_Desc, "Change_Alternator", TrnNo);
                }
                if (Tmold.BRAKE_PEDAL != TM.BRAKE_PEDAL)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.BRAKE_PEDAL, TM.BRAKE_PEDAL, tmold.BRAKE_PEDAL, tmold.BrakePedal_Desc, Tab, TM.gleSearch, TM.BRAKE_PEDAL, TM.BrakePedal_Desc, "Change_BRAKE_PEDAL", TrnNo);
                }
                if (Tmold.BRAKE_PEDAL != TM.BRAKE_PEDAL)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.CLUTCH_PEDAL, TM.CLUTCH_PEDAL, tmold.CLUTCH_PEDAL, tmold.ClutchPedal_Desc, Tab, TM.gleSearch, TM.CLUTCH_PEDAL, TM.ClutchPedal_Desc, "Change_CLUTCH_PEDAL", TrnNo);
                }
                if (Tmold.SPOOL_VALUE != TM.SPOOL_VALUE)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SPOOL_VALUE, TM.SPOOL_VALUE, tmold.SPOOL_VALUE, tmold.SpoolValue_Desc, Tab, TM.gleSearch, TM.SPOOL_VALUE, TM.SpoolValue_Desc, "Change_SPOOL_VALUE", TrnNo);
                }
                if (Tmold.TANDEM_PUMP != TM.TANDEM_PUMP)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.TANDEM_PUMP, TM.SPOOL_VALUE, tmold.SPOOL_VALUE, tmold.TandemPump_Desc, Tab, TM.gleSearch, TM.TANDEM_PUMP, TM.TandemPump_Desc, "Change_TANDEM_PUMP", TrnNo);
                }
                if (Tmold.FENDER != TM.FENDER)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FENDER, TM.FENDER, tmold.FENDER, tmold.Fender_Desc, Tab, TM.gleSearch, TM.FENDER, TM.Fender_Desc, "Change_FENDER", TrnNo);
                }
                if (Tmold.SteeringMotor != TM.SteeringMotor)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringMotor, TM.SteeringMotor, tmold.SteeringMotor, tmold.SteeringMotor_Desc, Tab, TM.gleSearch, TM.SteeringMotor, TM.SteeringMotor_Desc, "Change_SteeringMotor", TrnNo);
                }
                if (Tmold.FENDER_RAILING != TM.FENDER_RAILING)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FENDER_RAILING, TM.FENDER_RAILING, tmold.FENDER_RAILING, tmold.FenderRailing_Desc, Tab, TM.gleSearch, TM.FENDER_RAILING, TM.FenderRailing_Desc, "Change_FENDER_RAILING", TrnNo);
                }
                if (Tmold.HEAD_LAMP != TM.HEAD_LAMP)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HEAD_LAMP, TM.HEAD_LAMP, tmold.HEAD_LAMP, tmold.HeadLamp_Desc, Tab, TM.gleSearch, TM.HEAD_LAMP, TM.HeadLamp_Desc, "Change_HEAD_LAMP", TrnNo);
                }
                if (Tmold.STEERING_WHEEL != TM.STEERING_WHEEL)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.STEERING_WHEEL, TM.STEERING_WHEEL, tmold.STEERING_WHEEL, tmold.SteeringWheel_Desc, Tab, TM.gleSearch, TM.STEERING_WHEEL, TM.SteeringWheel_Desc, "Change_STEERING_WHEEL", TrnNo);
                }
                if (Tmold.REAR_HOOD_WIRING_HARNESS != TM.REAR_HOOD_WIRING_HARNESS)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.REAR_HOOD_WIRING_HARNESS, TM.REAR_HOOD_WIRING_HARNESS, tmold.REAR_HOOD_WIRING_HARNESS, tmold.RearHoolWiringHarness_Desc, Tab, TM.gleSearch, TM.REAR_HOOD_WIRING_HARNESS, TM.RearHoolWiringHarness_Desc, "Change_STEERING_WHEEL", TrnNo);
                }
                if (Tmold.SEAT != TM.SEAT)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SEAT, TM.SEAT, tmold.SEAT, tmold.Seat_Desc, Tab, TM.gleSearch, TM.SEAT, TM.Seat_Desc, "Change_SEAT", TrnNo);
                }
                if (Tmold.EnableCarButtonChk != TM.EnableCarButtonChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.EnableCarButtonChk), Convert.ToString(TM.EnableCarButtonChk), Convert.ToString(tmold.EnableCarButtonChk), Convert.ToString(tmold.EnableCarButtonChk), Tab, TM.gleSearch, Convert.ToString(TM.EnableCarButtonChk), Convert.ToString(TM.EnableCarButtonChk), "Change_CarButton", TrnNo);
                }
                if (Tmold.EngineChk != TM.EngineChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.EngineChk), Convert.ToString(TM.EngineChk), Convert.ToString(tmold.EngineChk), Convert.ToString(tmold.EngineChk), Tab, TM.gleSearch, Convert.ToString(TM.EngineChk), Convert.ToString(TM.EngineChk), "Change_EngineChk", TrnNo);
                }
                if (Tmold.TransmissionChk != TM.TransmissionChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.TransmissionChk), Convert.ToString(TM.TransmissionChk), Convert.ToString(tmold.TransmissionChk), Convert.ToString(tmold.TransmissionChk), Tab, TM.gleSearch, Convert.ToString(TM.TransmissionChk), Convert.ToString(TM.TransmissionChk), "Change_TransmissionChk", TrnNo);
                }
                if (Tmold.RearAxelChk != TM.RearAxelChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RearAxelChk), Convert.ToString(TM.RearAxelChk), Convert.ToString(tmold.RearAxelChk), Convert.ToString(tmold.RearAxelChk), Tab, TM.gleSearch, Convert.ToString(TM.RearAxelChk), Convert.ToString(TM.RearAxelChk), "Change_RearAxelChk", TrnNo);
                }
                if (Tmold.FrontTyreChk != TM.FrontTyreChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.FrontTyreChk), Convert.ToString(TM.FrontTyreChk), Convert.ToString(tmold.FrontTyreChk), Convert.ToString(tmold.FrontTyreChk), Tab, TM.gleSearch, Convert.ToString(TM.FrontTyreChk), Convert.ToString(TM.FrontTyreChk), "Change_FrontTyreChk", TrnNo);
                }
                if (Tmold.BatteryChk != TM.BatteryChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.BatteryChk), Convert.ToString(TM.BatteryChk), Convert.ToString(tmold.BatteryChk), Convert.ToString(tmold.BatteryChk), Tab, TM.gleSearch, Convert.ToString(TM.BatteryChk), Convert.ToString(TM.BatteryChk), "Change_BatteryChk", TrnNo);
                }
                if (Tmold.GenerateSerialNoChk != TM.GenerateSerialNoChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.GenerateSerialNoChk), Convert.ToString(TM.GenerateSerialNoChk), Convert.ToString(tmold.GenerateSerialNoChk), Convert.ToString(tmold.GenerateSerialNoChk), Tab, TM.gleSearch, Convert.ToString(TM.GenerateSerialNoChk), Convert.ToString(TM.GenerateSerialNoChk), "Change_GenerateSerialNoChk", TrnNo);
                }
                if (Tmold.RopsChk != TM.RopsChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RopsChk), Convert.ToString(TM.RopsChk), Convert.ToString(tmold.RopsChk), Convert.ToString(tmold.RopsChk), Tab, TM.gleSearch, Convert.ToString(TM.RopsChk), Convert.ToString(TM.RopsChk), "Change_RopsChk", TrnNo);
                }
                if (Tmold.HydraulicPumpChk != TM.HydraulicPumpChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.HydraulicPumpChk), Convert.ToString(TM.HydraulicPumpChk), Convert.ToString(tmold.HydraulicPumpChk), Convert.ToString(tmold.HydraulicPumpChk), Tab, TM.gleSearch, Convert.ToString(TM.HydraulicPumpChk), Convert.ToString(TM.HydraulicPumpChk), "Change_HydraulicPumpChk", TrnNo);
                }
                if (Tmold.SteeringAssemblyChk != TM.SteeringAssemblyChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.SteeringAssemblyChk), Convert.ToString(TM.SteeringAssemblyChk), Convert.ToString(tmold.SteeringAssemblyChk), Convert.ToString(tmold.SteeringAssemblyChk), Tab, TM.gleSearch, Convert.ToString(TM.SteeringAssemblyChk), Convert.ToString(TM.SteeringAssemblyChk), "Change_SteeringAssemblyChk", TrnNo);
                }
                if (Tmold.RadiatorAssemblyChk != TM.RadiatorAssemblyChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RadiatorAssemblyChk), Convert.ToString(TM.RadiatorAssemblyChk), Convert.ToString(tmold.RadiatorAssemblyChk), Convert.ToString(tmold.RadiatorAssemblyChk), Tab, TM.gleSearch, Convert.ToString(TM.RadiatorAssemblyChk), Convert.ToString(TM.RadiatorAssemblyChk), "Change_RadiatorAssemblyChk", TrnNo);
                }
                if (Tmold.AlternatorChk != TM.AlternatorChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.AlternatorChk), Convert.ToString(TM.AlternatorChk), Convert.ToString(tmold.AlternatorChk), Convert.ToString(tmold.AlternatorChk), Tab, TM.gleSearch, Convert.ToString(TM.AlternatorChk), Convert.ToString(TM.AlternatorChk), "Change_AlternatorChk", TrnNo);
                }
                if (Tmold.Seq_Not_RequireChk != TM.Seq_Not_RequireChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.Seq_Not_RequireChk), Convert.ToString(TM.Seq_Not_RequireChk), Convert.ToString(tmold.Seq_Not_RequireChk), Convert.ToString(tmold.Seq_Not_RequireChk), Tab, TM.gleSearch, Convert.ToString(TM.Seq_Not_RequireChk), Convert.ToString(TM.Seq_Not_RequireChk), "Change_Seq_Not_RequireChk", TrnNo);
                }
                if (Tmold.SteeringMotorChk != TM.SteeringMotorChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.SteeringMotorChk), Convert.ToString(TM.SteeringMotorChk), Convert.ToString(tmold.SteeringMotorChk), Convert.ToString(tmold.SteeringMotorChk), Tab, TM.gleSearch, Convert.ToString(TM.SteeringMotorChk), Convert.ToString(TM.SteeringMotorChk), "Change_SteeringMotorChk", TrnNo);
                }
                if (Tmold.ClusterAssemblyChk != TM.ClusterAssemblyChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.ClusterAssemblyChk), Convert.ToString(TM.ClusterAssemblyChk), Convert.ToString(tmold.ClusterAssemblyChk), Convert.ToString(tmold.ClusterAssemblyChk), Tab, TM.gleSearch, Convert.ToString(TM.ClusterAssemblyChk), Convert.ToString(TM.ClusterAssemblyChk), "Change_ClusterAssemblyChk", TrnNo);
                }
                if (Tmold.StartorMotorChk != TM.StartorMotorChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.StartorMotorChk), Convert.ToString(TM.StartorMotorChk), Convert.ToString(tmold.StartorMotorChk), Convert.ToString(tmold.StartorMotorChk), Tab, TM.gleSearch, Convert.ToString(TM.StartorMotorChk), Convert.ToString(TM.StartorMotorChk), "Change_StartorMotorChk", TrnNo);
                }
                if (Tmold.RHFrontTyreChk != TM.RHFrontTyreChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RHFrontTyreChk), Convert.ToString(TM.RHFrontTyreChk), Convert.ToString(tmold.RHFrontTyreChk), Convert.ToString(tmold.RHFrontTyreChk), Tab, TM.gleSearch, Convert.ToString(TM.RHFrontTyreChk), Convert.ToString(TM.RHFrontTyreChk), "Change_RHFrontTyreChk", TrnNo);
                }
                if (Tmold.RHRearTyreChk != TM.RHRearTyreChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RHRearTyreChk), Convert.ToString(TM.RHRearTyreChk), Convert.ToString(tmold.RHRearTyreChk), Convert.ToString(tmold.RHRearTyreChk), Tab, TM.gleSearch, Convert.ToString(TM.RHRearTyreChk), Convert.ToString(TM.RHRearTyreChk), "Change_RHRearTyreChk", TrnNo);
                }
                if (Tmold.DomesticExport != TM.DomesticExport)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.DomesticExport), Convert.ToString(TM.DomesticExport), Convert.ToString(tmold.DomesticExport), Convert.ToString(tmold.DomesticExport), Tab, TM.gleSearch, Convert.ToString(TM.DomesticExport), Convert.ToString(TM.DomesticExport), "Change_DomesticExport", TrnNo);
                }
                if (Tmold.NoOfBoltsFrontAxel != TM.NoOfBoltsFrontAxel)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsFrontAxel), Convert.ToString(TM.NoOfBoltsFrontAxel), Convert.ToString(tmold.NoOfBoltsFrontAxel), Convert.ToString(tmold.NoOfBoltsFrontAxel), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsFrontAxel), Convert.ToString(TM.NoOfBoltsFrontAxel), "Change_NoOfBoltsFrontAxel", TrnNo);
                }
                if (Tmold.NoOfBoltsHydraulic != TM.NoOfBoltsHydraulic)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsHydraulic), Convert.ToString(TM.NoOfBoltsHydraulic), Convert.ToString(tmold.NoOfBoltsHydraulic), Convert.ToString(tmold.NoOfBoltsHydraulic), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsHydraulic), Convert.ToString(TM.NoOfBoltsHydraulic), "Change_NoOfBoltsHydraulic", TrnNo);
                }
                if (Tmold.NoOfBoltsFrontTYre != TM.NoOfBoltsFrontTYre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsFrontTYre), Convert.ToString(TM.NoOfBoltsFrontTYre), Convert.ToString(tmold.NoOfBoltsFrontTYre), Convert.ToString(tmold.NoOfBoltsFrontTYre), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsFrontTYre), Convert.ToString(TM.NoOfBoltsFrontTYre), "Change_NoOfBoltsFrontTYre", TrnNo);
                }
                if (Tmold.NoOfBoltsRearTYre != TM.NoOfBoltsRearTYre)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsRearTYre), Convert.ToString(TM.NoOfBoltsRearTYre), Convert.ToString(tmold.NoOfBoltsRearTYre), Convert.ToString(tmold.NoOfBoltsRearTYre), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsRearTYre), Convert.ToString(TM.NoOfBoltsRearTYre), "Change_NoOfBoltsRearTYre", TrnNo);
                }
                if (Tmold.NoOfBoltsEnToruqe1 != TM.NoOfBoltsEnToruqe1)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe1), Convert.ToString(TM.NoOfBoltsEnToruqe1), Convert.ToString(tmold.NoOfBoltsEnToruqe1), Convert.ToString(tmold.NoOfBoltsEnToruqe1), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe1), Convert.ToString(TM.NoOfBoltsEnToruqe1), "Change_NoOfBoltsEnToruqe1", TrnNo);
                }
                if (Tmold.NoOfBoltsEnToruqe2 != TM.NoOfBoltsEnToruqe2)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe2), Convert.ToString(TM.NoOfBoltsEnToruqe2), Convert.ToString(tmold.NoOfBoltsEnToruqe2), Convert.ToString(tmold.NoOfBoltsEnToruqe2), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe2), Convert.ToString(TM.NoOfBoltsEnToruqe2), "Change_NoOfBoltsEnToruqe2", TrnNo);
                }
                if (Tmold.NoOfBoltsEnToruqe3 != TM.NoOfBoltsEnToruqe3)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe3), Convert.ToString(TM.NoOfBoltsEnToruqe3), Convert.ToString(tmold.NoOfBoltsEnToruqe3), Convert.ToString(tmold.NoOfBoltsEnToruqe3), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe3), Convert.ToString(TM.NoOfBoltsEnToruqe3), "Change_NoOfBoltsEnToruqe3", TrnNo);
                }
                if (Tmold.NoOfBoltsTRANSAXELToruqe1 != TM.NoOfBoltsTRANSAXELToruqe1)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(tmold.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(tmold.NoOfBoltsTRANSAXELToruqe1), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), "Change_NoOfBoltsTRANSAXELToruqe1", TrnNo);
                }
                if (Tmold.NoOfBoltsTRANSAXELToruqe2 != TM.NoOfBoltsTRANSAXELToruqe2)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(tmold.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(tmold.NoOfBoltsTRANSAXELToruqe2), Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), "Change_NoOfBoltsTRANSAXELToruqe2", TrnNo);
                }
                if (Tmold.Rops == null)
                {
                    Tmold.Rops = "";
                }
                if (TM.Rops == null)
                {
                    TM.Rops = "";
                }
                if (Tmold.Rops.Trim() != TM.Rops.Trim())
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Rops, TM.Rops, tmold.Rops, "", Tab, TM.gleSearch, TM.Rops, "", "Change_Rops", TrnNo);
                }
            }
            else
            {
                if (Tmold.T4_ItemCode != TM.T4_ItemCode)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.T4_ItemCode, TM.T4_ItemCode, tmold.T4_ItemCode, tmold.T4_ItemCode_Desc, Tab, TM.gleSearch, TM.T4_ItemCode, TM.T4_ItemCode_Desc, "Change_T4_ItemCode", TrnNo);
                }
                if (Tmold.FrontSupport != TM.FrontSupport)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FrontSupport, TM.FrontSupport, tmold.FrontSupport, tmold.FrontSupport_Desc, Tab, TM.gleSearch, TM.FrontSupport, TM.FrontSupport_Desc, "Change_FrontSupport", TrnNo);
                }
                if (Tmold.CenterAxel != TM.CenterAxel)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.CenterAxel, TM.CenterAxel, tmold.CenterAxel, tmold.CenterAxel_Desc, Tab, TM.gleSearch, TM.CenterAxel, TM.CenterAxel_Desc, "Change_CenterAxel", TrnNo);
                }
                if (Tmold.Slider != TM.Slider)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Slider, TM.Slider, tmold.Slider, tmold.Slider_Desc, Tab, TM.gleSearch, TM.Slider, TM.Slider_Desc, "Change_Slider", TrnNo);
                }
                if (Tmold.SteeringColumn != TM.SteeringColumn)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringColumn, TM.SteeringColumn, tmold.SteeringColumn, tmold.SteeringColumn_Desc, Tab, TM.gleSearch, TM.SteeringColumn, TM.SteeringColumn_Desc, "Change_SteeringColumn", TrnNo);
                }
                if (Tmold.SteeringBase != TM.SteeringBase)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringBase, TM.SteeringBase, tmold.SteeringBase, tmold.SteeringBase_Desc, Tab, TM.gleSearch, TM.SteeringBase, TM.SteeringBase_Desc, "Change_SteeringBase", TrnNo);
                }
                if (Tmold.Lowerlink != TM.Lowerlink)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Lowerlink, TM.Lowerlink, tmold.Lowerlink, tmold.Lowerlink_Desc, Tab, TM.gleSearch, TM.Lowerlink, TM.Lowerlink_Desc, "Change_Lowerlink", TrnNo);
                }
                if (Tmold.RBFrame != TM.RBFrame)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RBFrame, TM.RBFrame, tmold.RBFrame, tmold.RBFrame_Desc, Tab, TM.gleSearch, TM.RBFrame, TM.RBFrame_Desc, "Change_RBFrame", TrnNo);
                }
                if (Tmold.FuelTank != TM.FuelTank)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FuelTank, TM.FuelTank, tmold.FuelTank, tmold.FuelTank_Desc, Tab, TM.gleSearch, TM.FuelTank, TM.FuelTank_Desc, "Change_FuelTank", TrnNo);
                }
                if (Tmold.Cylinder != TM.Cylinder)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Cylinder, TM.Cylinder, tmold.Cylinder, tmold.Cylinder_Desc, Tab, TM.gleSearch, TM.Cylinder, TM.Cylinder_Desc, "Change_Cylinder", TrnNo);
                }
                if (Tmold.FenderRH != TM.FenderRH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FenderRH, TM.FenderRH, tmold.FenderRH, tmold.FenderRH_Desc, Tab, TM.gleSearch, TM.FenderRH, TM.FenderRH_Desc, "Change_FenderRH", TrnNo);
                }
                if (Tmold.FenderLH != TM.FenderLH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FenderLH, TM.FenderLH, tmold.FenderLH, tmold.FenderLH_Desc, Tab, TM.gleSearch, TM.FenderLH, TM.FenderLH_Desc, "Change_FenderLH", TrnNo);
                }
                if (Tmold.FenderHarnessRH != TM.FenderHarnessRH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FenderHarnessRH, TM.FenderHarnessRH, tmold.FenderHarnessRH, tmold.FenderHarnessRH_Desc, Tab, TM.gleSearch, TM.FenderHarnessRH, TM.FenderHarnessRH_Desc, "Change_FenderHarnessRH", TrnNo);
                }
                if (Tmold.FenderLamp4Types != TM.FenderLamp4Types)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FenderLamp4Types, TM.FenderLamp4Types, tmold.FenderLamp4Types, tmold.FenderLamp4Types_Desc, Tab, TM.gleSearch, TM.FenderLamp4Types, TM.FenderLamp4Types_Desc, "Change_FenderLamp4Types", TrnNo);
                }
                if (Tmold.RBHarnessLH != TM.RBHarnessLH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RBHarnessLH, TM.RBHarnessLH, tmold.RBHarnessLH, tmold.RBHarnessLH_Desc, Tab, TM.gleSearch, TM.RBHarnessLH, TM.RBHarnessLH_Desc, "Change_RBHarnessLH", TrnNo);
                }
                if (Tmold.FrontRim != TM.FrontRim)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FrontRim, TM.FrontRim, tmold.FrontRim, tmold.FrontRim_Desc, Tab, TM.gleSearch, TM.FrontRim, TM.FrontRim_Desc, "Change_FrontRim", TrnNo);
                }
                if (Tmold.RearRim != TM.RearRim)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearRim, TM.RearRim, tmold.RearRim, tmold.RearRim_Desc, Tab, TM.gleSearch, TM.RearRim, TM.RearRim_Desc, "Change_RearRim", TrnNo);
                }
                if (Tmold.TyreMake != TM.TyreMake)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.TyreMake, TM.TyreMake, tmold.TyreMake, tmold.TyreMake_Desc, Tab, TM.gleSearch, TM.TyreMake, TM.TyreMake_Desc, "Change_TyreMake", TrnNo);
                }
                if (Tmold.RearHood != TM.RearHood)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearHood, TM.RearHood, tmold.RearHood, tmold.RearHood_Desc, Tab, TM.gleSearch, TM.RearHood, TM.RearHood_Desc, "Change_RearHood", TrnNo);
                }
                if (Tmold.ClusterMeter != TM.ClusterMeter)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.ClusterMeter, TM.ClusterMeter, tmold.ClusterMeter, tmold.ClusterMeter_Desc, Tab, TM.gleSearch, TM.ClusterMeter, TM.ClusterMeter_Desc, "Change_ClusterMeter", TrnNo);
                }
                if (Tmold.IPHarness != TM.IPHarness)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.IPHarness, TM.IPHarness, tmold.IPHarness, tmold.IPHarness_Desc, Tab, TM.gleSearch, TM.IPHarness, TM.IPHarness_Desc, "Change_IPHarness", TrnNo);
                }
                if (Tmold.RadiatorShell != TM.RadiatorShell)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RadiatorShell, TM.RadiatorShell, tmold.RadiatorShell, tmold.RadiatorShell_Desc, Tab, TM.gleSearch, TM.RadiatorShell, TM.RadiatorShell_Desc, "Change_RadiatorShell", TrnNo);
                }
                if (Tmold.AirCleaner != TM.AirCleaner)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.AirCleaner, TM.AirCleaner, tmold.AirCleaner, tmold.AirCleaner_Desc, Tab, TM.gleSearch, TM.AirCleaner, TM.AirCleaner_Desc, "Change_AirCleaner", TrnNo);
                }
                if (Tmold.HeadLampLH != TM.HeadLampLH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HeadLampLH, TM.HeadLampLH, tmold.HeadLampLH, tmold.HeadLampLH_Desc, Tab, TM.gleSearch, TM.HeadLampLH, TM.HeadLampLH_Desc, "Change_HeadLampLH", TrnNo);
                }
                if (Tmold.HeadLampRH != TM.HeadLampRH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HeadLampRH, TM.HeadLampRH, tmold.HeadLampRH, tmold.HeadLampRH_Desc, Tab, TM.gleSearch, TM.HeadLampRH, TM.HeadLampRH_Desc, "Change_HeadLampRH", TrnNo);
                }
                if (Tmold.FrontGrill != TM.FrontGrill)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FrontGrill, TM.FrontGrill, tmold.FrontGrill, tmold.FrontGrill_Desc, Tab, TM.gleSearch, TM.FrontGrill, TM.FrontGrill_Desc, "Change_FrontGrill", TrnNo);
                }
                if (Tmold.MainHarnessBonnet != TM.MainHarnessBonnet)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.MainHarnessBonnet, TM.MainHarnessBonnet, tmold.MainHarnessBonnet, tmold.MainHarnessBonnet_Desc, Tab, TM.gleSearch, TM.MainHarnessBonnet, TM.MainHarnessBonnet_Desc, "Change_MainHarnessBonnet", TrnNo);
                }
                if (Tmold.Spindle != TM.Spindle)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Spindle, TM.Spindle, tmold.Spindle, tmold.Spindle_Desc, Tab, TM.gleSearch, TM.Spindle, TM.Spindle_Desc, "Change_Spindle", TrnNo);
                }
                if (Tmold.Slider_RH != TM.Slider_RH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Slider_RH, TM.Slider_RH, tmold.Slider_RH, tmold.Slider_RH_Desc, Tab, TM.gleSearch, TM.Slider_RH, TM.Slider_RH_Desc, "Change_Slider_RH", TrnNo);
                }
                if (Tmold.BRK_PAD != TM.BRK_PAD)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.BRK_PAD, TM.BRK_PAD, tmold.BRK_PAD, tmold.BRK_PAD_DESC, Tab, TM.gleSearch, TM.BRK_PAD, TM.BRK_PAD_DESC, "Change_BRK_PAD", TrnNo);
                }
                if (Tmold.FRB_RH != TM.FRB_RH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FRB_RH, TM.FRB_RH, tmold.FRB_RH, tmold.FRB_RH_DESC, Tab, TM.gleSearch, TM.FRB_RH, TM.FRB_RH_DESC, "Change_BRK_PAD", TrnNo);
                }
                if (Tmold.FRB_LH != TM.FRB_LH)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FRB_LH, TM.FRB_LH, tmold.FRB_LH, tmold.FRB_LH_DESC, Tab, TM.gleSearch, TM.FRB_LH, TM.FRB_LH_DESC, "Change_FRB_LH", TrnNo);
                }
                if (Tmold.FR_AS_RB != TM.FR_AS_RB)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FR_AS_RB, TM.FR_AS_RB, tmold.FR_AS_RB, tmold.FR_AS_RB_DESC, Tab, TM.gleSearch, TM.FR_AS_RB, TM.FR_AS_RB_DESC, "Change_FR_AS_RB", TrnNo);
                }
                if (Tmold.FR_AS_RB != TM.FR_AS_RB)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FR_AS_RB, TM.FR_AS_RB, tmold.FR_AS_RB, tmold.FR_AS_RB_DESC, Tab, TM.gleSearch, TM.FR_AS_RB, TM.FR_AS_RB_DESC, "Change_FR_AS_RB", TrnNo);
                }
                if (Tmold.FR_AS_RB != TM.FR_AS_RB)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.FR_AS_RB), Convert.ToString(TM.FR_AS_RB), Convert.ToString(tmold.FR_AS_RB), Convert.ToString(tmold.FR_AS_RB), Tab, TM.gleSearch, Convert.ToString(TM.FR_AS_RB), Convert.ToString(TM.FR_AS_RB), "Change_FR_AS_RB", TrnNo);
                }
                if (Tmold.FrontRimChk != TM.FrontRimChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.FR_AS_RB), Convert.ToString(TM.FR_AS_RB), Convert.ToString(tmold.FR_AS_RB), Convert.ToString(tmold.FR_AS_RB), Tab, TM.gleSearch, Convert.ToString(TM.FR_AS_RB), Convert.ToString(TM.FR_AS_RB), "Change_FR_AS_RB", TrnNo);
                }
                if (Tmold.RearRimChk != TM.RearRimChk)
                {
                    fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RearRimChk), Convert.ToString(TM.RearRimChk), Convert.ToString(tmold.RearRimChk), Convert.ToString(tmold.RearRimChk), Tab, TM.gleSearch, Convert.ToString(TM.RearRimChk), Convert.ToString(TM.RearRimChk), "Change_RearRimChk", TrnNo);
                }
            }
            return TrnNo;
        }
        public int InsertSave(TractorMster TM, string Tab)
        {
            string transactionNo = fun.get_Col_Value("SELECT NVL( MAX(TRANSACTION_NUMBER),0)+1 AS TRANSACTION_NUMBER FROM XXES_PARTS_AUDIT_DATA WHERE Remarks1 = 'TRACTOR_TAB_1' AND Remarks2 = 'F00000000'");
            TrnNo = Convert.ToInt32(transactionNo);
            if (TM.ItemCode != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.ItemCode, TM.ItemCode, "", "", "TRACTOR_TAB_1", TM.gleSearch, TM.ItemCode, TM.ItemCode_Desc, "Insert_ITEM", TrnNo);
            }
            if (TM.Engine != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Engine, TM.Engine, "", "", Tab, TM.gleSearch, TM.Engine, TM.Engine_Desc, "Insert_Engine", TrnNo);
            }
            if (TM.Backend != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Backend, TM.Backend, "", "", Tab, TM.gleSearch, TM.Backend, TM.Backend_Desc, "Insert_Backend", TrnNo);
            }
            if (TM.Transmission != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Transmission, TM.Transmission, "", "", Tab, TM.gleSearch, TM.Transmission, TM.Transmission_Desc, "Insert_Transmission", TrnNo);
            }
            if (TM.RearAxel != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearAxel, TM.RearAxel, "", "", Tab, TM.gleSearch, TM.RearAxel, TM.RearAxel_Desc, "Insert_RearAxel", TrnNo);
            }
            if (TM.Hydraulic != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Hydraulic, TM.Hydraulic, "", "", Tab, TM.gleSearch, TM.Hydraulic, TM.Hydraulic_Desc, "Insert_Hydraulic", TrnNo);
            }
            if (TM.FrontTyre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FrontTyre, TM.FrontTyre, "", "", Tab, TM.gleSearch, TM.FrontTyre, TM.FrontTyre_Desc, "Insert_FrontTyre", TrnNo);
            }
            if (TM.RHFrontTyre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RHFrontTyre, TM.RHFrontTyre, "", "", Tab, TM.gleSearch, TM.RHFrontTyre, TM.RHFrontTyre_Desc, "Insert_RHFrontTyre", TrnNo);
            }
            if (TM.RearTyre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RearTyre, TM.RearTyre, "", "", Tab, TM.gleSearch, TM.RearTyre, TM.RearTyre_Desc, "Insert_RearTyre", TrnNo);
            }
            if (TM.RHRearTyre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RHRearTyre, TM.RHRearTyre, "", "", Tab, TM.gleSearch, TM.RHRearTyre, TM.RHRearTyre_Desc, "Insert_RHRearTyre", TrnNo);
            }
            if (TM.Battery != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Battery, TM.Battery, "", "", Tab, TM.gleSearch, TM.Battery, TM.Battery_Desc, "Insert_Battery", TrnNo);
            }
            if (TM.Rops != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Rops, TM.Rops, "", "", Tab, TM.gleSearch, TM.Rops, "", "Insert_Rops", TrnNo);
            }
            if (TM.Prefix1 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix1, TM.Prefix1, "", "", Tab, TM.gleSearch, TM.Prefix1, "", "Insert_Prefix1", TrnNo);
            }
            if (TM.Prefix2 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix2, TM.Prefix2, "", "", Tab, TM.gleSearch, TM.Prefix2, "", "Insert_Prefix2", TrnNo);
            }
            if (TM.Prefix3 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix3, TM.Prefix3, "", "", Tab, TM.gleSearch, TM.Prefix3, "", "Insert_Prefix2", TrnNo);
            }
            if (TM.Prefix4 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Prefix4, TM.Prefix4, "", "", Tab, TM.gleSearch, TM.Prefix4, "", "Insert_Prefix4", TrnNo);
            }
            if (TM.Remarks != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Remarks, TM.Remarks, "", "", Tab, TM.gleSearch, TM.Remarks, "", "Insert_Remarks", TrnNo);
            }
            if (TM.ShortDesc != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.ShortDesc, TM.ShortDesc, "", "", Tab, TM.gleSearch, TM.ShortDesc, "", "Insert_ShortDesc", TrnNo);
            }
            if (TM.HydraulicPump != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HydraulicPump, TM.HydraulicPump, "", "", Tab, TM.gleSearch, TM.HydraulicPump, TM.HydraulicPump_Desc, "Insert_HydraulicPump", TrnNo);
            }
            if (TM.SteeringAssembly != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringAssembly, TM.SteeringAssembly, "", "", Tab, TM.gleSearch, TM.SteeringAssembly, TM.SteeringAssembly_Desc, "Insert_SteeringAssembly", TrnNo);
            }
            if (TM.RadiatorAssembly != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.RadiatorAssembly, TM.RadiatorAssembly, "", "", Tab, TM.gleSearch, TM.RadiatorAssembly, TM.RadiatorAssembly_Desc, "Insert_RadiatorAssembly", TrnNo);
            }
            if (TM.Alternator != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.Alternator, TM.Alternator, "", "", Tab, TM.gleSearch, TM.Alternator, TM.Alternator_Desc, "Insert_Alternator", TrnNo);
            }
            if (TM.BRAKE_PEDAL != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.BRAKE_PEDAL, TM.BRAKE_PEDAL, "", "", Tab, TM.gleSearch, TM.BRAKE_PEDAL, TM.BrakePedal_Desc, "Insert_BRAKE_PEDAL", TrnNo);
            }
            if (TM.BRAKE_PEDAL != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.CLUTCH_PEDAL, TM.CLUTCH_PEDAL, "", "", Tab, TM.gleSearch, TM.CLUTCH_PEDAL, TM.ClutchPedal_Desc, "Insert_CLUTCH_PEDAL", TrnNo);
            }
            if (TM.SPOOL_VALUE != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SPOOL_VALUE, TM.SPOOL_VALUE, "", "", Tab, TM.gleSearch, TM.SPOOL_VALUE, TM.SpoolValue_Desc, "Insert_SPOOL_VALUE", TrnNo);
            }
            if (TM.TANDEM_PUMP != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.TANDEM_PUMP, TM.SPOOL_VALUE, "", "", Tab, TM.gleSearch, TM.TANDEM_PUMP, TM.TandemPump_Desc, "Insert_TANDEM_PUMP", TrnNo);
            }
            if (TM.FENDER != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FENDER, TM.FENDER, "", "", Tab, TM.gleSearch, TM.FENDER, TM.Fender_Desc, "Insert_FENDER", TrnNo);
            }
            if (TM.SteeringMotor != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SteeringMotor, TM.SteeringMotor, "", "", Tab, TM.gleSearch, TM.SteeringMotor, TM.SteeringMotor_Desc, "Insert_SteeringMotor", TrnNo);
            }
            if (TM.FENDER_RAILING != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.FENDER_RAILING, TM.FENDER_RAILING, "", "", Tab, TM.gleSearch, TM.FENDER_RAILING, TM.FenderRailing_Desc, "Insert_FENDER_RAILING", TrnNo);
            }
            if (TM.HEAD_LAMP != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.HEAD_LAMP, TM.HEAD_LAMP, "", "", Tab, TM.gleSearch, TM.HEAD_LAMP, TM.HeadLamp_Desc, "Insert_HEAD_LAMP", TrnNo);
            }
            if (TM.STEERING_WHEEL != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.STEERING_WHEEL, TM.STEERING_WHEEL, "", "", Tab, TM.gleSearch, TM.STEERING_WHEEL, TM.SteeringWheel_Desc, "Insert_STEERING_WHEEL", TrnNo);
            }
            if (TM.REAR_HOOD_WIRING_HARNESS != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.REAR_HOOD_WIRING_HARNESS, TM.REAR_HOOD_WIRING_HARNESS, "", "", Tab, TM.gleSearch, TM.REAR_HOOD_WIRING_HARNESS, TM.RearHoolWiringHarness_Desc, "Insert_STEERING_WHEEL", TrnNo);
            }
            if (TM.SEAT != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, TM.SEAT, TM.SEAT, "", "", Tab, TM.gleSearch, TM.SEAT, TM.Seat_Desc, "Insert_SEAT", TrnNo);
            }
            if (TM.EnableCarButtonChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.EnableCarButtonChk), Convert.ToString(TM.EnableCarButtonChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.EnableCarButtonChk), Convert.ToString(TM.EnableCarButtonChk), "Insert_CarButton", TrnNo);
            }
            if (TM.EngineChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.EngineChk), Convert.ToString(TM.EngineChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.EngineChk), Convert.ToString(TM.EngineChk), "Insert_EngineChk", TrnNo);
            }
            if (TM.TransmissionChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.TransmissionChk), Convert.ToString(TM.TransmissionChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.TransmissionChk), Convert.ToString(TM.TransmissionChk), "Insert_TransmissionChk", TrnNo);
            }
            if (TM.RearAxelChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RearAxelChk), Convert.ToString(TM.RearAxelChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.RearAxelChk), Convert.ToString(TM.RearAxelChk), "Insert_RearAxelChk", TrnNo);
            }
            if (TM.FrontTyreChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.FrontTyreChk), Convert.ToString(TM.FrontTyreChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.FrontTyreChk), Convert.ToString(TM.FrontTyreChk), "Insert_FrontTyreChk", TrnNo);
            }
            if (TM.BatteryChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.BatteryChk), Convert.ToString(TM.BatteryChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.BatteryChk), Convert.ToString(TM.BatteryChk), "Insert_BatteryChk", TrnNo);
            }
            if (TM.GenerateSerialNoChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.GenerateSerialNoChk), Convert.ToString(TM.GenerateSerialNoChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.GenerateSerialNoChk), Convert.ToString(TM.GenerateSerialNoChk), "Insert_GenerateSerialNoChk", TrnNo);
            }
            if (TM.RopsChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RopsChk), Convert.ToString(TM.RopsChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.RopsChk), Convert.ToString(TM.RopsChk), "Insert_RopsChk", TrnNo);
            }
            if (TM.HydraulicPumpChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.HydraulicPumpChk), Convert.ToString(TM.HydraulicPumpChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.HydraulicPumpChk), Convert.ToString(TM.HydraulicPumpChk), "Insert_HydraulicPumpChk", TrnNo);
            }
            if (TM.SteeringAssemblyChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.SteeringAssemblyChk), Convert.ToString(TM.SteeringAssemblyChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.SteeringAssemblyChk), Convert.ToString(TM.SteeringAssemblyChk), "Insert_SteeringAssemblyChk", TrnNo);
            }
            if (TM.RadiatorAssemblyChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RadiatorAssemblyChk), Convert.ToString(TM.RadiatorAssemblyChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.RadiatorAssemblyChk), Convert.ToString(TM.RadiatorAssemblyChk), "Insert_RadiatorAssemblyChk", TrnNo);
            }
            if (TM.AlternatorChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.AlternatorChk), Convert.ToString(TM.AlternatorChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.AlternatorChk), Convert.ToString(TM.AlternatorChk), "Insert_AlternatorChk", TrnNo);
            }
            if (TM.Seq_Not_RequireChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.Seq_Not_RequireChk), Convert.ToString(TM.Seq_Not_RequireChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.Seq_Not_RequireChk), Convert.ToString(TM.Seq_Not_RequireChk), "Insert_Seq_Not_RequireChk", TrnNo);
            }
            if (TM.SteeringMotorChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.SteeringMotorChk), Convert.ToString(TM.SteeringMotorChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.SteeringMotorChk), Convert.ToString(TM.SteeringMotorChk), "Insert_SteeringMotorChk", TrnNo);
            }
            if (TM.ClusterAssemblyChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.ClusterAssemblyChk), Convert.ToString(TM.ClusterAssemblyChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.ClusterAssemblyChk), Convert.ToString(TM.ClusterAssemblyChk), "Insert_ClusterAssemblyChk", TrnNo);
            }
            if (TM.StartorMotorChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.StartorMotorChk), Convert.ToString(TM.StartorMotorChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.StartorMotorChk), Convert.ToString(TM.StartorMotorChk), "Insert_StartorMotorChk", TrnNo);
            }
            if (TM.RHFrontTyreChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RHFrontTyreChk), Convert.ToString(TM.RHFrontTyreChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.RHFrontTyreChk), Convert.ToString(TM.RHFrontTyreChk), "Insert_RHFrontTyreChk", TrnNo);
            }
            if (TM.RHRearTyreChk != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.RHRearTyreChk), Convert.ToString(TM.RHRearTyreChk), "", "", Tab, TM.gleSearch, Convert.ToString(TM.RHRearTyreChk), Convert.ToString(TM.RHRearTyreChk), "Insert_RHRearTyreChk", TrnNo);
            }
            if (TM.DomesticExport != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.DomesticExport), Convert.ToString(TM.DomesticExport), "", "", Tab, TM.gleSearch, Convert.ToString(TM.DomesticExport), Convert.ToString(TM.DomesticExport), "Insert_DomesticExport", TrnNo);
            }
            if (TM.NoOfBoltsFrontAxel != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsFrontAxel), Convert.ToString(TM.NoOfBoltsFrontAxel), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsFrontAxel), Convert.ToString(TM.NoOfBoltsFrontAxel), "Insert_NoOfBoltsFrontAxel", TrnNo);
            }
            if (TM.NoOfBoltsHydraulic != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsHydraulic), Convert.ToString(TM.NoOfBoltsHydraulic), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsHydraulic), Convert.ToString(TM.NoOfBoltsHydraulic), "Insert_NoOfBoltsHydraulic", TrnNo);
            }
            if (TM.NoOfBoltsFrontTYre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsFrontTYre), Convert.ToString(TM.NoOfBoltsFrontTYre), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsFrontTYre), Convert.ToString(TM.NoOfBoltsFrontTYre), "Insert_NoOfBoltsFrontTYre", TrnNo);
            }
            if (TM.NoOfBoltsRearTYre != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsRearTYre), Convert.ToString(TM.NoOfBoltsRearTYre), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsRearTYre), Convert.ToString(TM.NoOfBoltsRearTYre), "Insert_NoOfBoltsRearTYre", TrnNo);
            }
            if (TM.NoOfBoltsEnToruqe1 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe1), Convert.ToString(TM.NoOfBoltsEnToruqe1), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe1), Convert.ToString(TM.NoOfBoltsEnToruqe1), "Insert_NoOfBoltsEnToruqe1", TrnNo);
            }
            if (TM.NoOfBoltsEnToruqe2 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe2), Convert.ToString(TM.NoOfBoltsEnToruqe2), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe2), Convert.ToString(TM.NoOfBoltsEnToruqe2), "Insert_NoOfBoltsEnToruqe2", TrnNo);
            }
            if (TM.NoOfBoltsEnToruqe3 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsEnToruqe3), Convert.ToString(TM.NoOfBoltsEnToruqe3), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsEnToruqe3), Convert.ToString(TM.NoOfBoltsEnToruqe3), "Insert_NoOfBoltsEnToruqe3", TrnNo);
            }
            if (TM.NoOfBoltsTRANSAXELToruqe1 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe1), "Insert_NoOfBoltsTRANSAXELToruqe1", TrnNo);
            }
            if (TM.NoOfBoltsTRANSAXELToruqe2 != null)
            {
                fun.Insert_Part_Audit_DataNEW(TM.Plant, TM.Family, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), "", "", Tab, TM.gleSearch, Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), Convert.ToString(TM.NoOfBoltsTRANSAXELToruqe2), "Insert_NoOfBoltsTRANSAXELToruqe2", TrnNo);
            }

            return TrnNo;
        }
        public JsonResult Update(TractorMster obj)
        {
            string msg = string.Empty;
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(obj.Plant))
                {
                    msg = "Select Plant to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(obj.Family))
                {
                    msg = "Select family to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(obj.ItemCode))
                {
                    msg = "Select item to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string[] item = StringSpliter(obj.ItemCode);
                    obj.ItemCode = item[0].Trim();
                    obj.ItemCode_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Engine))
                {
                    string[] item = StringSpliter(obj.Engine);
                    obj.Engine = item[0].Trim();
                    obj.Engine_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Backend))
                {
                    string[] item = StringSpliter(obj.Backend);
                    obj.Backend = item[0].Trim();
                    obj.Backend_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Transmission))
                {
                    string[] item = StringSpliter(obj.Transmission);
                    obj.Transmission = item[0].Trim();
                    obj.Transmission_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RearAxel))
                {
                    string[] item = StringSpliter(obj.RearAxel);
                    obj.RearAxel = item[0].Trim();
                    obj.RearAxel_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Hydraulic))
                {
                    string[] item = StringSpliter(obj.Hydraulic);
                    obj.Hydraulic = item[0].Trim();
                    obj.Hydraulic_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FrontTyre))
                {
                    string[] item = StringSpliter(obj.FrontTyre);
                    obj.FrontTyre = item[0].Trim();
                    obj.FrontTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RHFrontTyre))
                {
                    string[] item = StringSpliter(obj.RHFrontTyre);
                    obj.RHFrontTyre = item[0].Trim();
                    obj.RHFrontTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RearTyre))
                {
                    string[] item = StringSpliter(obj.RearTyre);
                    obj.RearTyre = item[0].Trim();
                    obj.RearTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RHRearTyre))
                {
                    string[] item = StringSpliter(obj.RHRearTyre);
                    obj.RHRearTyre = item[0].Trim();
                    obj.RHRearTyre_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Battery))
                {
                    string[] item = StringSpliter(obj.Battery);
                    obj.Battery = item[0].Trim();
                    obj.Battery_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix1))
                {
                    obj.Prefix1 = replaceApostophi(obj.Prefix1.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix2))
                {
                    obj.Prefix2 = replaceApostophi(obj.Prefix2.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix3))
                {
                    obj.Prefix3 = replaceApostophi(obj.Prefix3.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Prefix4))
                {
                    obj.Prefix4 = replaceApostophi(obj.Prefix4.Trim());
                }
                if (!string.IsNullOrEmpty(obj.Remarks))
                {
                    obj.Remarks = replaceApostophi(obj.Remarks.Trim());
                }
                if (!string.IsNullOrEmpty(obj.ShortDesc))
                {
                    obj.ShortDesc = replaceApostophi(obj.ShortDesc.Trim());
                }
                if (!string.IsNullOrEmpty(obj.HydraulicPump))
                {
                    string[] item = StringSpliter(obj.HydraulicPump);
                    obj.HydraulicPump = item[0].Trim();
                    obj.HydraulicPump_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringMotor))
                {
                    string[] item = StringSpliter(obj.SteeringMotor);
                    obj.SteeringMotor = item[0].Trim();
                    obj.SteeringMotor_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringAssembly))
                {
                    string[] item = StringSpliter(obj.SteeringAssembly);
                    obj.SteeringAssembly = item[0].Trim();
                    obj.SteeringAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.SteeringCylinder))
                {
                    string[] item = StringSpliter(obj.SteeringCylinder);
                    obj.SteeringCylinder = item[0].Trim();
                    obj.SteeringCylender_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.RadiatorAssembly))
                {
                    string[] item = StringSpliter(obj.RadiatorAssembly);
                    obj.RadiatorAssembly = item[0].Trim();
                    obj.RadiatorAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.ClusterAssembly))
                {
                    string[] item = StringSpliter(obj.ClusterAssembly);
                    obj.ClusterAssembly = item[0].Trim();
                    obj.ClusterAssembly_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Alternator))
                {
                    string[] item = StringSpliter(obj.Alternator);
                    obj.Alternator = item[0].Trim();
                    obj.Alternator_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.StartorMotor))
                {
                    string[] item = StringSpliter(obj.StartorMotor);
                    obj.StartorMotor = item[0].Trim();
                    obj.StartorMotor_Desc = replaceApostophi(item[1].Trim());
                }

                //DefaultAtPlanning = obj.DefaultAtPlanning;

                //obj.DomesticExport = obj.DomesticExport.Trim();

                if (string.IsNullOrEmpty(obj.NoOfBoltsTRANSAXELToruqe1))
                    obj.NoOfBoltsFrontAxel = "0";
                else
                    obj.BK_BOLT_VALUE = obj.NoOfBoltsTRANSAXELToruqe1.Trim();

                if (string.IsNullOrEmpty(obj.NoOfBoltsTRANSAXELToruqe2))
                    obj.BK_BOLT_VALUE += "#0";
                else
                    obj.BK_BOLT_VALUE += "#" + obj.NoOfBoltsTRANSAXELToruqe1.Trim();


                if (!string.IsNullOrEmpty(obj.Rops))
                {
                    string[] item = StringSpliter(obj.Rops);
                    obj.Rops = item[0].Trim();
                    if (item.Length > 1)
                    {
                        obj.Rops_Desc = replaceApostophi(item[1].Trim());
                    }

                }

                if (!string.IsNullOrEmpty(obj.BRAKE_PEDAL))
                {
                    string[] item = StringSpliter(obj.BRAKE_PEDAL);
                    obj.BRAKE_PEDAL = item[0].Trim();
                    obj.BrakePedal_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.CLUTCH_PEDAL))
                {
                    string[] item = StringSpliter(obj.CLUTCH_PEDAL);
                    obj.CLUTCH_PEDAL = item[0].Trim();
                    obj.ClutchPedal_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SPOOL_VALUE))
                {
                    string[] item = StringSpliter(obj.SPOOL_VALUE);
                    obj.SPOOL_VALUE = item[0].Trim();
                    obj.SpoolValue_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.TANDEM_PUMP))
                {
                    string[] item = StringSpliter(obj.TANDEM_PUMP);
                    obj.TANDEM_PUMP = item[0].Trim();
                    obj.TandemPump_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FENDER))
                {
                    string[] item = StringSpliter(obj.FENDER);
                    obj.FENDER = item[0].Trim();
                    obj.Fender_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FENDER_RAILING))
                {
                    string[] item = StringSpliter(obj.FENDER_RAILING);
                    obj.FENDER_RAILING = item[0].Trim();
                    obj.FenderRailing_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.HEAD_LAMP))
                {
                    string[] item = StringSpliter(obj.HEAD_LAMP);
                    obj.HEAD_LAMP = item[0].Trim();
                    obj.HeadLamp_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.STEERING_WHEEL))
                {
                    string[] item = StringSpliter(obj.STEERING_WHEEL);
                    obj.STEERING_WHEEL = item[0].Trim();
                    obj.SteeringWheel_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.REAR_HOOD_WIRING_HARNESS))
                {
                    string[] item = StringSpliter(obj.REAR_HOOD_WIRING_HARNESS);
                    obj.REAR_HOOD_WIRING_HARNESS = item[0].Trim();
                    obj.RearHoolWiringHarness_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SEAT))
                {
                    string[] item = StringSpliter(obj.SEAT);
                    obj.SEAT = item[0].Trim();
                    obj.Seat_Desc = replaceApostophi(item[1].Trim());
                }



                #endregion
                tmold = EXISTDATA(obj);
                int transaction = ChangeUpdate(tmold, obj, "TRACTOR_TAB_1");
                obj.ORG_ID = fun.getOrgId(Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (fun.UpdateTractorMaster(obj))
                {

                    string head = "Update Tractor in MES with following details :- Tractor Code : " + obj.gleSearch + "<br>" + obj.ItemCode_Desc + "";
                    string mailbody = MailBODY(transaction, obj.gleSearch, "TRACTOR_TAB_1", head);
                    string mailsend = sendMail("Update_Change", mailbody);
                    query = "select count(*) from ITEM_MASTER where trim(PLANT_CODE)='" + Convert.ToString(obj.Plant) + "' and trim(FAMILY_CODE)='" + Convert.ToString(obj.Family) + "' and trim(ITEM_CODE)='" + Convert.ToString(obj.ItemCode).Trim() + "'";
                    if (!fun.CheckExits(query))
                    {
                        query = @"insert into ITEM_MASTER(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,ENGINE,ENGINE_DESCRIPTION,BACKEND,BACKEND_DESCRIPTION,
                                    TRANSMISSION,TRANSMISSION_DESCRIPTION,REARAXEL,REARAXEL_DESCRIPTION,ORG_ID) 
                                values ('" + Convert.ToString(obj.Plant).Trim() + "','" + Convert.ToString(obj.Family).Trim() + "','" + obj.ItemCode + "','" + obj.ItemCode_Desc + "','" + obj.Engine + "','" + obj.Engine_Desc + "','" + obj.Backend + "','" + obj.Backend_Desc + "','" + obj.Transmission + "','" + obj.Transmission_Desc + "','" + obj.RearAxel + "','" + obj.RearAxel_Desc + "','" + obj.ORG_ID + "')";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("TRACTOR MASTER BI", "Insert_Update", Convert.ToString(obj.Plant) + " # " + Convert.ToString(obj.Family) + " # " + Convert.ToString(obj.ItemCode), query, Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                        }
                        msg = " Saved successfully...";
                    }
                    msg = " Update successfully...";
                }
                else
                {
                    msg = "Error found while mapping of Item !!";
                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);

        }
        public string sendMail(string Type, string mailbody)
        {
            string Email_To = string.Empty; string Email_CC = string.Empty; string Username = string.Empty;
            Email_To = fun.get_Col_Value("select EMAIL as EMAIL  from XXES_STAGE_EMAILS WHERE STAGE='TRACTOR_BOM'");
            string sendMail = fun.SendMails(Type, mailbody, "Tractor Changes", Email_To, Email_CC, Username);
            return sendMail;
        }
        public string MailBODY(int Transaction, string FCode, string TransctionType, string Heading)
        {
            DataTable data_table = new DataTable();
            string query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,ITEM_CODE,ENTRYDATE,PART_ITEM_CODE,OLD_PART_DESC,REMARKS1,REMARKS2,PART_NEW_ITEMCODE,NEW_PART_DESC,LOGIN_USER,SYSTEM,PART_DESC,TRANSACTION_NUMBER
                                            FROM XXES_PARTS_AUDIT_DATA
                                            WHERE Remarks1='{0}' AND Remarks2='{1}' AND TRANSACTION_NUMBER={2}", TransctionType.Trim(), FCode.Trim(), Transaction);
            data_table = fun.returnDataTable(query);

            string textBody = " <table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 600 + ">";
            textBody += " <tr><th colspan='7'>" + Heading + " </th></tr><tr><th>CHanges Flied Name</th><th>OldCode</th><th>Old Name</th><th>New Code</th><th>New Name</th><th>ENTRY Date</th><th>ENTRY BY</th><th>SYSTEM ID</th></tr>";
            for (int loopCount = 0; loopCount < data_table.Rows.Count; loopCount++)
            {
                textBody += "<tr><td>" + data_table.Rows[loopCount]["PART_DESC"] + "</td><td> " + data_table.Rows[loopCount]["PART_ITEM_CODE"] + "</td><td> " + data_table.Rows[loopCount]["OLD_PART_DESC"] + "</td><td> " + data_table.Rows[loopCount]["PART_NEW_ITEMCODE"] + "</td><td> " + data_table.Rows[loopCount]["NEW_PART_DESC"] + "</td><td> " + data_table.Rows[loopCount]["ENTRYDATE"] + "</td><td> " + data_table.Rows[loopCount]["LOGIN_USER"] + "</td> <td> " + data_table.Rows[loopCount]["SYSTEM"] + "</td> </tr>";
            }
            textBody += "</table>";
            return textBody;
        }
        public string replaceApostophi(string chkstr)
        {
            chkstr = chkstr.Replace("\"", string.Empty).Trim(); //"
            return chkstr.Replace("'", "''");  //'
        }

        ///////////////////////////////////////Tractor Sub Assembly Items////////////////////////////////////////////////


        public PartialViewResult BindT4_Plant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult BindT4_Family(string T4_Plant)
        {
            if (!string.IsNullOrEmpty(T4_Plant))
            {
                ViewBag.T4Family = new SelectList(fun.Fill_FamilyOnlyTractor(T4_Plant), "Value", "Text");
            }
            return PartialView();
        }

        public JsonResult FillItemCodes(TractorMster data)
        {
            List<DDLTextValue> _Itemcode = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.T4_ItemCode))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('F','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.T4_ItemCode.Trim().ToUpper(), data.T4_ItemCode.Trim().ToUpper());
                    //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                    //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Itemcode.Add(new DDLTextValue
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
            return Json(_Itemcode, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFrontSupport(TractorMster data)
        {
            List<DDLTextValue> _FrontSupport = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FrontSupport))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FrontSupport.Trim().ToUpper(), data.FrontSupport.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FrontSupport.Add(new DDLTextValue
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
            }
            return Json(_FrontSupport, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillCenterAxel(TractorMster data)
        {
            List<DDLTextValue> _CenterAxel = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.CenterAxel))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.CenterAxel.Trim().ToUpper(), data.CenterAxel.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _CenterAxel.Add(new DDLTextValue
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
            }
            return Json(_CenterAxel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSlider(TractorMster data)
        {
            List<DDLTextValue> _Slider = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Slider))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Slider.Trim().ToUpper(), data.Slider.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Slider.Add(new DDLTextValue
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
            }
            return Json(_Slider, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSteeringColumn(TractorMster data)
        {
            List<DDLTextValue> _SteeringColumn = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SteeringColumn))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SteeringColumn.Trim().ToUpper(), data.SteeringColumn.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SteeringColumn.Add(new DDLTextValue
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
            }
            return Json(_SteeringColumn, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSteeringBase(TractorMster data)
        {
            List<DDLTextValue> _SteeringBase = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SteeringBase))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SteeringBase.Trim().ToUpper(), data.SteeringBase.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SteeringBase.Add(new DDLTextValue
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
            }
            return Json(_SteeringBase, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillLowerlink(TractorMster data)
        {
            List<DDLTextValue> _Lowerlink = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Lowerlink))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Lowerlink.Trim().ToUpper(), data.Lowerlink.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Lowerlink.Add(new DDLTextValue
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
            }
            return Json(_Lowerlink, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRBFrame(TractorMster data)
        {
            List<DDLTextValue> _RBFrame = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RBFrame))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RBFrame.Trim().ToUpper(), data.RBFrame.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RBFrame.Add(new DDLTextValue
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
            }
            return Json(_RBFrame, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFuelTank(TractorMster data)
        {
            List<DDLTextValue> _FuelTank = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FuelTank))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FuelTank.Trim().ToUpper(), data.FuelTank.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FuelTank.Add(new DDLTextValue
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
            }
            return Json(_FuelTank, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillCylinder(TractorMster data)
        {
            List<DDLTextValue> _Cylinder = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Cylinder))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Cylinder.Trim().ToUpper(), data.Cylinder.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Cylinder.Add(new DDLTextValue
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
            }
            return Json(_Cylinder, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFenderRH(TractorMster data)
        {
            List<DDLTextValue> _FenderRH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FenderRH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FenderRH.Trim().ToUpper(), data.FenderRH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FenderRH.Add(new DDLTextValue
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
            }
            return Json(_FenderRH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFenderLH(TractorMster data)
        {
            List<DDLTextValue> _FenderLH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FenderLH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FenderLH.Trim().ToUpper(), data.FenderLH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FenderLH.Add(new DDLTextValue
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
            }
            return Json(_FenderLH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFenderHarnessRH(TractorMster data)
        {
            List<DDLTextValue> _FenderHarnessRH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FenderHarnessRH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FenderHarnessRH.Trim().ToUpper(), data.FenderHarnessRH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FenderHarnessRH.Add(new DDLTextValue
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
            }
            return Json(_FenderHarnessRH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFenderHarnessLH(TractorMster data)
        {
            List<DDLTextValue> _FenderHarnessLH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FenderHarnessLH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FenderHarnessLH.Trim().ToUpper(), data.FenderHarnessLH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FenderHarnessLH.Add(new DDLTextValue
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
            }
            return Json(_FenderHarnessLH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFenderLamp4Types(TractorMster data)
        {
            List<DDLTextValue> _FenderLamp4Types = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FenderLamp4Types))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FenderLamp4Types.Trim().ToUpper(), data.FenderLamp4Types.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FenderLamp4Types.Add(new DDLTextValue
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
            }
            return Json(_FenderLamp4Types, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRBHarnessLH(TractorMster data)
        {
            List<DDLTextValue> _RBHarnessLH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RBHarnessLH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RBHarnessLH.Trim().ToUpper(), data.RBHarnessLH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RBHarnessLH.Add(new DDLTextValue
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
            }
            return Json(_RBHarnessLH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFrontRim(TractorMster data)
        {
            List<DDLTextValue> _FrontRim = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FrontRim))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FrontRim.Trim().ToUpper(), data.FrontRim.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FrontRim.Add(new DDLTextValue
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
            }
            return Json(_FrontRim, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRearRim(TractorMster data)
        {
            List<DDLTextValue> _RearRim = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RearRim))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RearRim.Trim().ToUpper(), data.RearRim.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RearRim.Add(new DDLTextValue
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
            }
            return Json(_RearRim, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillTyreMake(TractorMster data)
        {
            List<DDLTextValue> _TyreMake = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.TyreMake))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.TyreMake.Trim().ToUpper(), data.TyreMake.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _TyreMake.Add(new DDLTextValue
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
            }
            return Json(_TyreMake, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRearHood(TractorMster data)
        {
            List<DDLTextValue> _RearHood = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RearHood))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RearHood.Trim().ToUpper(), data.RearHood.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RearHood.Add(new DDLTextValue
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
            }
            return Json(_RearHood, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillClusterMeter(TractorMster data)
        {
            List<DDLTextValue> _ClusterMeter = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.ClusterMeter))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.ClusterMeter.Trim().ToUpper(), data.ClusterMeter.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _ClusterMeter.Add(new DDLTextValue
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
            }
            return Json(_ClusterMeter, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillIPHarness(TractorMster data)
        {
            List<DDLTextValue> _IPHarness = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.IPHarness))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.IPHarness.Trim().ToUpper(), data.IPHarness.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _IPHarness.Add(new DDLTextValue
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
            }
            return Json(_IPHarness, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillRadiatorShell(TractorMster data)
        {
            List<DDLTextValue> _RadiatorShell = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.RadiatorShell))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.RadiatorShell.Trim().ToUpper(), data.RadiatorShell.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _RadiatorShell.Add(new DDLTextValue
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
            }
            return Json(_RadiatorShell, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillAirCleaner(TractorMster data)
        {
            List<DDLTextValue> _AirCleaner = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.AirCleaner))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.AirCleaner.Trim().ToUpper(), data.AirCleaner.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _AirCleaner.Add(new DDLTextValue
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
            }
            return Json(_AirCleaner, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillHeadLampLH(TractorMster data)
        {
            List<DDLTextValue> _HeadLampLH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.HeadLampLH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.HeadLampLH.Trim().ToUpper(), data.HeadLampLH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _HeadLampLH.Add(new DDLTextValue
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
            }
            return Json(_HeadLampLH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillHeadLampRH(TractorMster data)
        {
            List<DDLTextValue> _HeadLampRH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.HeadLampRH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.HeadLampRH.Trim().ToUpper(), data.HeadLampRH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _HeadLampRH.Add(new DDLTextValue
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
            }
            return Json(_HeadLampRH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFrontGrill(TractorMster data)
        {
            List<DDLTextValue> _FrontGrill = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FrontGrill))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FrontGrill.Trim().ToUpper(), data.FrontGrill.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FrontGrill.Add(new DDLTextValue
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
            }
            return Json(_FrontGrill, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillMainHarnessBonnet(TractorMster data)
        {
            List<DDLTextValue> _MainHarnessBonnet = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.MainHarnessBonnet))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.MainHarnessBonnet.Trim().ToUpper(), data.MainHarnessBonnet.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _MainHarnessBonnet.Add(new DDLTextValue
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
            }
            return Json(_MainHarnessBonnet, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSpindle(TractorMster data)
        {
            List<DDLTextValue> _Spindle = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Spindle))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Spindle.Trim().ToUpper(), data.Spindle.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Spindle.Add(new DDLTextValue
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
            }
            return Json(_Spindle, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult FillMotor(TractorMster data)
        //{
        //    List<DDLTextValue> _Motor = new List<DDLTextValue>();
        //    try
        //    {
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
        //        if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
        //        {
        //            return Json(null, JsonRequestBehavior.AllowGet);
        //        }
        //        string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
        //        DataTable dt = new DataTable();
        //        if (!string.IsNullOrEmpty(data.Motor))
        //        {
        //            query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
        //                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Motor.Trim().ToUpper(), data.Motor.Trim().ToUpper());
        //        }

        //        dt = fun.returnDataTable(query);
        //        if (dt.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in dt.AsEnumerable())
        //            {
        //                _Motor.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        fun.LogWrite(ex);
        //    }
        //    return Json(_Motor, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult SaveS(TractorMster obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(obj.T4_Plant))
                {
                    msg = "Select Plant to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(obj.T4_Family))
                {
                    msg = "Select family to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(obj.T4_ItemCode))
                {
                    msg = "Select item to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string[] item = StringSpliter(obj.T4_ItemCode);
                    obj.T4_ItemCode = item[0].Trim();
                    obj.T4_ItemCode_Desc = replaceApostophi(item[1].Trim());
                }

                //////////////////////TAB4////////////////////////////////////////

                if (!string.IsNullOrEmpty(obj.FrontSupport))
                {
                    string[] item = StringSpliter(obj.FrontSupport);
                    obj.FrontSupport = item[0].Trim();
                    obj.FrontSupport_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.CenterAxel))
                {
                    string[] item = StringSpliter(obj.CenterAxel);
                    obj.CenterAxel = item[0].Trim();
                    obj.CenterAxel_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Slider))
                {
                    string[] item = StringSpliter(obj.Slider);
                    obj.Slider = item[0].Trim();
                    obj.Slider_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SteeringColumn))
                {
                    string[] item = StringSpliter(obj.SteeringColumn);
                    obj.SteeringColumn = item[0].Trim();
                    obj.SteeringColumn_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SteeringBase))
                {
                    string[] item = StringSpliter(obj.SteeringBase);
                    obj.SteeringBase = item[0].Trim();
                    obj.SteeringBase_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Lowerlink))
                {
                    string[] item = StringSpliter(obj.Lowerlink);
                    obj.Lowerlink = item[0].Trim();
                    obj.Lowerlink_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RBFrame))
                {
                    string[] item = StringSpliter(obj.RBFrame);
                    obj.RBFrame = item[0].Trim();
                    obj.RBFrame_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FuelTank))
                {
                    string[] item = StringSpliter(obj.FuelTank);
                    obj.FuelTank = item[0].Trim();
                    obj.FuelTank_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Cylinder))
                {
                    string[] item = StringSpliter(obj.Cylinder);
                    obj.Cylinder = item[0].Trim();
                    obj.Cylinder_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderRH))
                {
                    string[] item = StringSpliter(obj.FenderRH);
                    obj.FenderRH = item[0].Trim();
                    obj.FenderRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderLH))
                {
                    string[] item = StringSpliter(obj.FenderLH);
                    obj.FenderLH = item[0].Trim();
                    obj.FenderLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderHarnessRH))
                {
                    string[] item = StringSpliter(obj.FenderHarnessRH);
                    obj.FenderHarnessRH = item[0].Trim();
                    obj.FenderHarnessRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderHarnessLH))
                {
                    string[] item = StringSpliter(obj.FenderHarnessLH);
                    obj.FenderHarnessLH = item[0].Trim();
                    obj.FenderHarnessLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderLamp4Types))
                {
                    string[] item = StringSpliter(obj.FenderLamp4Types);
                    obj.FenderLamp4Types = item[0].Trim();
                    obj.FenderLamp4Types_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RBHarnessLH))
                {
                    string[] item = StringSpliter(obj.RBHarnessLH);
                    obj.RBHarnessLH = item[0].Trim();
                    obj.RBHarnessLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FrontRim))
                {
                    string[] item = StringSpliter(obj.FrontRim);
                    obj.FrontRim = item[0].Trim();
                    obj.FrontRim_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RearRim))
                {
                    string[] item = StringSpliter(obj.RearRim);
                    obj.RearRim = item[0].Trim();
                    obj.RearRim_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.TyreMake))
                {
                    string[] item = StringSpliter(obj.TyreMake);
                    obj.TyreMake = item[0].Trim();
                    obj.TyreMake_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RearHood))
                {
                    string[] item = StringSpliter(obj.RearHood);
                    obj.RearHood = item[0].Trim();
                    obj.RearHood_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.ClusterMeter))
                {
                    string[] item = StringSpliter(obj.ClusterMeter);
                    obj.ClusterMeter = item[0].Trim();
                    obj.ClusterMeter_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.IPHarness))
                {
                    string[] item = StringSpliter(obj.IPHarness);
                    obj.IPHarness = item[0].Trim();
                    obj.IPHarness_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RadiatorShell))
                {
                    string[] item = StringSpliter(obj.RadiatorShell);
                    obj.RadiatorShell = item[0].Trim();
                    obj.RadiatorShell_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.AirCleaner))
                {
                    string[] item = StringSpliter(obj.AirCleaner);
                    obj.AirCleaner = item[0].Trim();
                    obj.AirCleaner_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.HeadLampLH))
                {
                    string[] item = StringSpliter(obj.HeadLampLH);
                    obj.HeadLampLH = item[0].Trim();
                    obj.HeadLampLH_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.HeadLampRH))
                {
                    string[] item = StringSpliter(obj.HeadLampRH);
                    obj.HeadLampRH = item[0].Trim();
                    obj.HeadLampRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FrontGrill))
                {
                    string[] item = StringSpliter(obj.FrontGrill);
                    obj.FrontGrill = item[0].Trim();
                    obj.FrontGrill_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.MainHarnessBonnet))
                {
                    string[] item = StringSpliter(obj.MainHarnessBonnet);
                    obj.MainHarnessBonnet = item[0].Trim();
                    obj.MainHarnessBonnet_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Spindle))
                {
                    string[] item = StringSpliter(obj.Spindle);
                    obj.Spindle = item[0].Trim();
                    obj.Spindle_Desc = replaceApostophi(item[1].Trim());
                }
                //----------------------------Add New Start------------------------------

                if (!string.IsNullOrEmpty(obj.Slider_RH))
                {
                    string[] item = StringSpliter(obj.Slider_RH);
                    obj.Slider_RH = item[0].Trim();
                    obj.Slider_RH_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.BRK_PAD))
                {
                    string[] item = StringSpliter(obj.BRK_PAD);
                    obj.BRK_PAD = item[0].Trim();
                    obj.BRK_PAD_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FRB_RH))
                {
                    string[] item = StringSpliter(obj.FRB_RH);
                    obj.FRB_RH = item[0].Trim();
                    obj.FRB_RH_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FRB_LH))
                {
                    string[] item = StringSpliter(obj.FRB_LH);
                    obj.FRB_LH = item[0].Trim();
                    obj.FRB_LH_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FR_AS_RB))
                {
                    string[] item = StringSpliter(obj.FR_AS_RB);
                    obj.FR_AS_RB = item[0].Trim();
                    obj.FR_AS_RB_DESC = replaceApostophi(item[1].Trim());
                }

                //----------------------------Add New end------------------------------


                #endregion

                obj.ORG_ID = fun.getOrgId(Convert.ToString(obj.T4_Plant).Trim().ToUpper(), Convert.ToString(obj.T4_Family).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_ITEM_MASTER WHERE PLANT_CODE = '" + obj.T4_Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + obj.T4_Family.ToUpper().Trim() + "' AND ITEM_CODE = '" + obj.T4_ItemCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str31;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertTractorMasterS(obj))
                {
                    int transactionNo = InsertSave(obj, "TRACTOR_TAB_2");
                    string head = "New Tractor added in MES with following details :- Tractor Code : " + obj.gleSearch + "<br>" + obj.ItemCode_Desc + "";
                    string mailbody = MailBODY(transactionNo, obj.gleSearch, "TRACTOR_TAB_1", head);
                    string mailsend = sendMail("Insert_Tractor", mailbody);
                    query = "select count(*) from ITEM_MASTER where trim(PLANT_CODE)='" + Convert.ToString(obj.T4_Plant) + "' and trim(FAMILY_CODE)='" + Convert.ToString(obj.T4_Family) + "'and trim(ITEM_CODE)='" + Convert.ToString(obj.T4_ItemCode).Trim() + "' ";

                    //fun.Insert_Into_ActivityLog("TRACTOR MASTER", "Insert_Update", Convert.ToString(obj.Plant) + " # " + Convert.ToString(obj.Family) + " # " + Convert.ToString(obj.ItemCode), query, Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                    if (!fun.CheckExits(query))
                    {

                        query = @"insert into ITEM_MASTER(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,ENGINE,ENGINE_DESCRIPTION,BACKEND,BACKEND_DESCRIPTION,
                                    TRANSMISSION,TRANSMISSION_DESCRIPTION,REARAXEL,REARAXEL_DESCRIPTION,ORG_ID) 
                                values ('" + Convert.ToString(obj.T4_Plant).Trim() + "','" + Convert.ToString(obj.T4_Family).Trim() + "','" + obj.T4_ItemCode + "','" + obj.T4_ItemCode_Desc + "','" + obj.Engine + "','" + obj.Engine_Desc + "','" + obj.Backend + "','" + obj.Backend_Desc + "','" + obj.Transmission + "','" + obj.Transmission_Desc + "','" + obj.RearAxel + "','" + obj.RearAxel_Desc + "','" + obj.ORG_ID + "')";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("TRACTOR MASTER BI", "Insert_Update", Convert.ToString(obj.T4_Plant) + " # " + Convert.ToString(obj.T4_Family) + " # " + Convert.ToString(obj.T4_ItemCode), query, Convert.ToString(obj.T4_Plant).Trim().ToUpper(), Convert.ToString(obj.T4_Family).Trim().ToUpper());
                        }
                        msg = "Saved successfully...";
                    }
                    msg = "Saved successfully...";
                }
                else
                {
                    msg = "Error found while mapping of Item !!";
                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateS(TractorMster obj)
        {
            string msg = string.Empty;
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(obj.T4_Plant))
                {
                    msg = "Select Plant to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(obj.T4_Family))
                {
                    msg = "Select family to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(obj.T4_ItemCode))
                {
                    msg = "Select item to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string[] item = StringSpliter(obj.T4_ItemCode);
                    obj.T4_ItemCode = item[0].Trim();
                    obj.T4_ItemCode_Desc = replaceApostophi(item[1].Trim());
                }

                //////////////////////TAB4////////////////////////////////////////

                if (!string.IsNullOrEmpty(obj.FrontSupport))
                {
                    string[] item = StringSpliter(obj.FrontSupport);
                    obj.FrontSupport = item[0].Trim();
                    obj.FrontSupport_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.CenterAxel))
                {
                    string[] item = StringSpliter(obj.CenterAxel);
                    obj.CenterAxel = item[0].Trim();
                    obj.CenterAxel_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Slider))
                {
                    string[] item = StringSpliter(obj.Slider);
                    obj.Slider = item[0].Trim();
                    obj.Slider_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SteeringColumn))
                {
                    string[] item = StringSpliter(obj.SteeringColumn);
                    obj.SteeringColumn = item[0].Trim();
                    obj.SteeringColumn_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.SteeringBase))
                {
                    string[] item = StringSpliter(obj.SteeringBase);
                    obj.SteeringBase = item[0].Trim();
                    obj.SteeringBase_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Lowerlink))
                {
                    string[] item = StringSpliter(obj.Lowerlink);
                    obj.Lowerlink = item[0].Trim();
                    obj.Lowerlink_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RBFrame))
                {
                    string[] item = StringSpliter(obj.RBFrame);
                    obj.RBFrame = item[0].Trim();
                    obj.RBFrame_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FuelTank))
                {
                    string[] item = StringSpliter(obj.FuelTank);
                    obj.FuelTank = item[0].Trim();
                    obj.FuelTank_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.Cylinder))
                {
                    string[] item = StringSpliter(obj.Cylinder);
                    obj.Cylinder = item[0].Trim();
                    obj.Cylinder_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderRH))
                {
                    string[] item = StringSpliter(obj.FenderRH);
                    obj.FenderRH = item[0].Trim();
                    obj.FenderRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderLH))
                {
                    string[] item = StringSpliter(obj.FenderLH);
                    obj.FenderLH = item[0].Trim();
                    obj.FenderLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderHarnessRH))
                {
                    string[] item = StringSpliter(obj.FenderHarnessRH);
                    obj.FenderHarnessRH = item[0].Trim();
                    obj.FenderHarnessRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderHarnessLH))
                {
                    string[] item = StringSpliter(obj.FenderHarnessLH);
                    obj.FenderHarnessLH = item[0].Trim();
                    obj.FenderHarnessLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FenderLamp4Types))
                {
                    string[] item = StringSpliter(obj.FenderLamp4Types);
                    obj.FenderLamp4Types = item[0].Trim();
                    obj.FenderLamp4Types_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RBHarnessLH))
                {
                    string[] item = StringSpliter(obj.RBHarnessLH);
                    obj.RBHarnessLH = item[0].Trim();
                    obj.RBHarnessLH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FrontRim))
                {
                    string[] item = StringSpliter(obj.FrontRim);
                    obj.FrontRim = item[0].Trim();
                    obj.FrontRim_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RearRim))
                {
                    string[] item = StringSpliter(obj.RearRim);
                    obj.RearRim = item[0].Trim();
                    obj.RearRim_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.TyreMake))
                {
                    string[] item = StringSpliter(obj.TyreMake);
                    obj.TyreMake = item[0].Trim();
                    obj.TyreMake_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RearHood))
                {
                    string[] item = StringSpliter(obj.RearHood);
                    obj.RearHood = item[0].Trim();
                    obj.RearHood_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.ClusterMeter))
                {
                    string[] item = StringSpliter(obj.ClusterMeter);
                    obj.ClusterMeter = item[0].Trim();
                    obj.ClusterMeter_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.IPHarness))
                {
                    string[] item = StringSpliter(obj.IPHarness);
                    obj.IPHarness = item[0].Trim();
                    obj.IPHarness_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.RadiatorShell))
                {
                    string[] item = StringSpliter(obj.RadiatorShell);
                    obj.RadiatorShell = item[0].Trim();
                    obj.RadiatorShell_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.AirCleaner))
                {
                    string[] item = StringSpliter(obj.AirCleaner);
                    obj.AirCleaner = item[0].Trim();
                    obj.AirCleaner_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.HeadLampLH))
                {
                    string[] item = StringSpliter(obj.HeadLampLH);
                    obj.HeadLampLH = item[0].Trim();
                    obj.HeadLampLH_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.HeadLampRH))
                {
                    string[] item = StringSpliter(obj.HeadLampRH);
                    obj.HeadLampRH = item[0].Trim();
                    obj.HeadLampRH_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.FrontGrill))
                {
                    string[] item = StringSpliter(obj.FrontGrill);
                    obj.FrontGrill = item[0].Trim();
                    obj.FrontGrill_Desc = replaceApostophi(item[1].Trim());
                }

                if (!string.IsNullOrEmpty(obj.MainHarnessBonnet))
                {
                    string[] item = StringSpliter(obj.MainHarnessBonnet);
                    obj.MainHarnessBonnet = item[0].Trim();
                    obj.MainHarnessBonnet_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.Spindle))
                {
                    string[] item = StringSpliter(obj.Spindle);
                    obj.Spindle = item[0].Trim();
                    obj.Spindle_Desc = replaceApostophi(item[1].Trim());
                }

                //----------------------------Add New Start------------------------------

                if (!string.IsNullOrEmpty(obj.Slider_RH))
                {
                    string[] item = StringSpliter(obj.Slider_RH);
                    obj.Slider_RH = item[0].Trim();
                    obj.Slider_RH_Desc = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.BRK_PAD))
                {
                    string[] item = StringSpliter(obj.BRK_PAD);
                    obj.BRK_PAD = item[0].Trim();
                    obj.BRK_PAD_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FRB_RH))
                {
                    string[] item = StringSpliter(obj.FRB_RH);
                    obj.FRB_RH = item[0].Trim();
                    obj.FRB_RH_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FRB_LH))
                {
                    string[] item = StringSpliter(obj.FRB_LH);
                    obj.FRB_LH = item[0].Trim();
                    obj.FRB_LH_DESC = replaceApostophi(item[1].Trim());
                }
                if (!string.IsNullOrEmpty(obj.FR_AS_RB))
                {
                    string[] item = StringSpliter(obj.FR_AS_RB);
                    obj.FR_AS_RB = item[0].Trim();
                    obj.FR_AS_RB_DESC = replaceApostophi(item[1].Trim());
                }

                //----------------------------Add New end------------------------------

                #endregion
                tmold = EXISTDATA(obj);
                int transaction = ChangeUpdate(tmold, obj, "TRACTOR_TAB_2");
                obj.ORG_ID = fun.getOrgId(Convert.ToString(obj.T4_Plant).Trim().ToUpper(), Convert.ToString(obj.T4_Family).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (fun.UpdateTractorMasterS(obj))
                {

                    string head = "Update TAb 2 Tractor in MES with following details :- Tractor Code : " + obj.gleSearch + "<br>" + obj.ItemCode_Desc + "";
                    string mailbody = MailBODY(transaction, obj.gleSearch, "TRACTOR_TAB_2", head);
                    string mailsend = sendMail("Update_Change_TAb2", mailbody);
                    query = "select count(*) from ITEM_MASTER where trim(PLANT_CODE)='" + Convert.ToString(obj.T4_Plant) + "' and trim(FAMILY_CODE)='" + Convert.ToString(obj.T4_Family) + "'and trim(ITEM_CODE)='" + Convert.ToString(obj.T4_ItemCode).Trim() + "' ";

                    //fun.Insert_Into_ActivityLog("TRACTOR MASTER", "Insert_Update", Convert.ToString(obj.Plant) + " # " + Convert.ToString(obj.Family) + " # " + Convert.ToString(obj.ItemCode), query, Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
                    //query = "select count(*) from ITEM_MASTER where trim(PLANT_CODE)='" + Convert.ToString(obj.T4_Plant) + "' and trim(FAMILY_CODE)='" + Convert.ToString(obj.T4_Family) + "'and trim(ITEM_CODE)='" + Convert.ToString(obj.T4_ItemCode).Trim() + "' ";
                    if (!fun.CheckExits(query))
                    {
                        query = @"insert into ITEM_MASTER(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,ENGINE,ENGINE_DESCRIPTION,BACKEND,BACKEND_DESCRIPTION,
                                    TRANSMISSION,TRANSMISSION_DESCRIPTION,REARAXEL,REARAXEL_DESCRIPTION,ORG_ID) 
                                values ('" + Convert.ToString(obj.T4_Plant).Trim() + "','" + Convert.ToString(obj.T4_Family).Trim() + "','" + obj.T4_ItemCode + "','" + obj.T4_ItemCode_Desc + "','" + obj.Engine + "','" + obj.Engine_Desc + "','" + obj.Backend + "','" + obj.Backend_Desc + "','" + obj.Transmission + "','" + obj.Transmission_Desc + "','" + obj.RearAxel + "','" + obj.RearAxel_Desc + "','" + obj.ORG_ID + "')";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("TRACTOR MASTER BI", "Insert_Update", Convert.ToString(obj.T4_Plant) + " # " + Convert.ToString(obj.T4_Family) + " # " + Convert.ToString(obj.T4_ItemCode), query, Convert.ToString(obj.T4_Plant).Trim().ToUpper(), Convert.ToString(obj.T4_Family).Trim().ToUpper());
                        }
                        msg = "Saved successfully...";
                    }
                    msg = "Update successfully...";
                }
                else
                {
                    msg = "Error found while mapping of Item !!";
                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult gleSearch_EditValueChangedTab2(TractorMster obj)
        {
            string msg = string.Empty;
            TractorMster tm = new TractorMster();
            try
            {
                if (!string.IsNullOrEmpty(obj.gleSearch) && !string.IsNullOrEmpty(obj.Plant) && !string.IsNullOrEmpty(obj.Family))
                {
                    query = @"select * from XXES_ITEM_MASTER where ITEM_CODE='" + obj.gleSearch.ToString().Trim() + "' and Plant_code='" + Convert.ToString(obj.Plant).Trim() + "'  and Family_code='" + Convert.ToString(obj.Family).Trim() + "'  order by FAMILY_CODE";
                    DataTable dt = new DataTable();
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        tm.Plant = Convert.ToString(dt.Rows[0]["PLANT_CODE"]);
                        tm.Family = Convert.ToString(dt.Rows[0]["FAMILY_CODE"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ITEM_CODE"])))
                        {
                            tm.T4_ItemCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]) + " # " + Convert.ToString(dt.Rows[0]["item_description"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_SUPPORT"])))
                        {
                            tm.FrontSupport = Convert.ToString(dt.Rows[0]["FRONT_SUPPORT"]) + " # " + Convert.ToString(dt.Rows[0]["FRONT_SUPPORT_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CENTER_AXEL"])))
                        {
                            tm.CenterAxel = Convert.ToString(dt.Rows[0]["CENTER_AXEL"]) + " # " + Convert.ToString(dt.Rows[0]["CENTER_AXEL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SLIDER"])))
                        {
                            tm.Slider = Convert.ToString(dt.Rows[0]["SLIDER"]) + " # " + Convert.ToString(dt.Rows[0]["SLIDER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_COLUMN"])))
                        {
                            tm.SteeringColumn = Convert.ToString(dt.Rows[0]["STEERING_COLUMN"]) + " # " + Convert.ToString(dt.Rows[0]["STEERING_COLUMN_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STEERING_BASE"])))
                        {
                            tm.SteeringBase = Convert.ToString(dt.Rows[0]["STEERING_BASE"]) + " # " + Convert.ToString(dt.Rows[0]["STEERING_BASE_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["LOWER_LINK"])))
                        {
                            tm.Lowerlink = Convert.ToString(dt.Rows[0]["LOWER_LINK"]) + " # " + Convert.ToString(dt.Rows[0]["LOWER_LINK_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RB_FRAME"])))
                        {
                            tm.RBFrame = Convert.ToString(dt.Rows[0]["RB_FRAME"]) + " # " + Convert.ToString(dt.Rows[0]["RB_FRAME_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FUEL_TANK"])))
                        {
                            tm.FuelTank = Convert.ToString(dt.Rows[0]["FUEL_TANK"]) + " # " + Convert.ToString(dt.Rows[0]["FUEL_TANK_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CYLINDER"])))
                        {
                            tm.Cylinder = Convert.ToString(dt.Rows[0]["CYLINDER"]) + " # " + Convert.ToString(dt.Rows[0]["CYLINDER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_RH"])))
                        {
                            tm.FenderRH = Convert.ToString(dt.Rows[0]["FENDER_RH"]) + " # " + Convert.ToString(dt.Rows[0]["FENDER_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_LH"])))
                        {
                            tm.FenderLH = Convert.ToString(dt.Rows[0]["FENDER_LH"]) + " # " + Convert.ToString(dt.Rows[0]["FENDER_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH"])))
                        {
                            tm.FenderHarnessRH = Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH"]) + " # " + Convert.ToString(dt.Rows[0]["FENDER_HARNESS_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FENDER_LAMP"])))
                        {
                            tm.FenderLamp4Types = Convert.ToString(dt.Rows[0]["FENDER_LAMP"]) + " # " + Convert.ToString(dt.Rows[0]["FENDER_LAMP_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RB_HARNESS_LH"])))
                        {
                            tm.RBHarnessLH = Convert.ToString(dt.Rows[0]["RB_HARNESS_LH"]) + " # " + Convert.ToString(dt.Rows[0]["RB_HARNESS_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_RIM"])))
                        {
                            tm.FrontRim = Convert.ToString(dt.Rows[0]["FRONT_RIM"]) + " # " + Convert.ToString(dt.Rows[0]["FRONT_RIM_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_RIM"])))
                        {
                            tm.RearRim = Convert.ToString(dt.Rows[0]["REAR_RIM"]) + " # " + Convert.ToString(dt.Rows[0]["REAR_RIM_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["TYRE_MAKE"])))
                        {
                            tm.TyreMake = Convert.ToString(dt.Rows[0]["TYRE_MAKE"]) + " # " + Convert.ToString(dt.Rows[0]["TYRE_MAKE_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["REAR_HOOD"])))
                        {
                            tm.RearHood = Convert.ToString(dt.Rows[0]["REAR_HOOD"]) + " # " + Convert.ToString(dt.Rows[0]["REAR_HOOD_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CLUSTER_METER"])))
                        {
                            tm.ClusterMeter = Convert.ToString(dt.Rows[0]["CLUSTER_METER"]) + " # " + Convert.ToString(dt.Rows[0]["CLUSTER_METER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["IP_HARNESS"])))
                        {
                            tm.IPHarness = Convert.ToString(dt.Rows[0]["IP_HARNESS"]) + " # " + Convert.ToString(dt.Rows[0]["IP_HARNESS_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["RADIATOR_SHELL"])))
                        {
                            tm.RadiatorShell = Convert.ToString(dt.Rows[0]["RADIATOR_SHELL"]) + " # " + Convert.ToString(dt.Rows[0]["RADIATOR_SHELL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["AIR_CLEANER"])))
                        {
                            tm.AirCleaner = Convert.ToString(dt.Rows[0]["AIR_CLEANER"]) + " # " + Convert.ToString(dt.Rows[0]["AIR_CLEANER_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH"])))
                        {
                            tm.HeadLampLH = Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH"]) + " # " + Convert.ToString(dt.Rows[0]["HEAD_LAMP_LH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH"])))
                        {
                            tm.HeadLampRH = Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH"]) + " # " + Convert.ToString(dt.Rows[0]["HEAD_LAMP_RH_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRONT_GRILL"])))
                        {
                            tm.FrontGrill = Convert.ToString(dt.Rows[0]["FRONT_GRILL"]) + " # " + Convert.ToString(dt.Rows[0]["FRONT_GRILL_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["MAIN_HARNESS_BONNET"])))
                        {
                            tm.MainHarnessBonnet = Convert.ToString(dt.Rows[0]["MAIN_HARNESS_BONNET"]) + " # " + Convert.ToString(dt.Rows[0]["MAIN_HARNESSBONNET_DESCRIPTION"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SPINDLE"])))
                        {
                            tm.Spindle = Convert.ToString(dt.Rows[0]["SPINDLE"]) + " # " + Convert.ToString(dt.Rows[0]["SPINDLE_DESCRIPTION"]);
                        }
                        //----------------------------------Add New Start-----------------------------------------
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["SLIDER_RH"])))
                        {
                            tm.Slider_RH = Convert.ToString(dt.Rows[0]["SLIDER_RH"]) + " # " + Convert.ToString(dt.Rows[0]["SLIDER_RH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BRK_PAD"])))
                        {
                            tm.BRK_PAD = Convert.ToString(dt.Rows[0]["BRK_PAD"]) + " # " + Convert.ToString(dt.Rows[0]["BRK_PAD_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRB_RH"])))
                        {
                            tm.FRB_RH = Convert.ToString(dt.Rows[0]["FRB_RH"]) + " # " + Convert.ToString(dt.Rows[0]["FRB_RH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FRB_LH"])))
                        {
                            tm.FRB_LH = Convert.ToString(dt.Rows[0]["FRB_LH"]) + " # " + Convert.ToString(dt.Rows[0]["FRB_LH_DESC"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FR_AS_RB"])))
                        {
                            tm.FR_AS_RB = Convert.ToString(dt.Rows[0]["FR_AS_RB"]) + " # " + Convert.ToString(dt.Rows[0]["FR_AS_RB_DESC"]);
                        }

                        if (Convert.ToString(dt.Rows[0]["REQ_FRONTRIM"]) == "Y")
                            tm.FrontRimChk = true;
                        else
                            tm.FrontRimChk = false;
                        if (Convert.ToString(dt.Rows[0]["REQ_REARRIM"]) == "Y")
                            tm.RearRimChk = true;
                        else
                            tm.RearRimChk = false;
                        //---------------------------------Add New End-------------------------------------------

                        //if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["MOTOR"])))
                        //{
                        //    tm.Motor = Convert.ToString(dt.Rows[0]["MOTOR"]) + " # " + Convert.ToString(dt.Rows[0]["MOTOR_DESC"]);
                        //}
                        tmold = JsonConvert.DeserializeObject<TractorMsterOld>(JsonConvert.SerializeObject(tm));
                    }
                    //msg = "";
                }
                //else
                //{
                //    msg = "Item Not Found";
                //}
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }
            var myResult = new
            {
                Result = tm,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public List<DDLTextValue> Fill_Search_ItemS(TractorMster obj)
        {
            DataTable dt = null;
            List<DDLTextValue> Search = new List<DDLTextValue>();
            try
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    //query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER order by ITEM_CODE";
                    query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER where PLANT_CODE='" + Convert.ToString(obj.Plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(obj.Family) + "' order by ITEM_CODE";
                }
                else
                    query = "select ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_ITEM_MASTER where Plant_code='" + Convert.ToString(obj.Plant).Trim() + "' and family_code='" + Convert.ToString(obj.Family) + "' order by ITEM_CODE";
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Search.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return Search;
            }
            catch { }
            finally { }
            return Search;
        }

        public PartialViewResult BindSearchItemS(TractorMster obj)
        {
            ViewBag.gleSearchS = new SelectList(Fill_Search_ItemS(obj), "Value", "Text");
            return PartialView();
        }

        //////////////////////////////Export Excel//////////////////////////////

        public JsonResult ExportTractormaster(TractorMster data)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string UserIpAdd = string.Empty; string errorNo = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                dt = BindTractormaster(data);
                if (dt.Rows.Count > 0)
                {
                    dt.Namespace = "Tractor";
                    dt.TableName = "Tractor";
                    string filename = "Tractor" + DateTime.Now.ToString("ddMMyyyy");
                    data.ImportExcel = filename;
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add(dt);
                    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                    ws.Range("A1:EX1").Style.Font.Bold = true;
                    ws.Columns().AdjustToContents();

                    string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }
                    wb.SaveAs(FilePath);
                    msg = "File Exported Successfully ...";
                    mstType = Validation.str;
                    excelName = data.ImportExcel;
                    var resul = new { Msg = msg, ID = mstType, ExcelName = filename };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "No Record Found..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

            }

            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = "alert-danger";
                errorNo = "1";
                var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public DataTable BindTractormaster(TractorMster data)
        {
            query = string.Format("select * from XXES_ITEM_MASTER where PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' and FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "'");

            return fun.returnDataTable(query);
        }
        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        public JsonResult FillSliderRH(TractorMster data)
        {
            List<DDLTextValue> _SliderRH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Slider_RH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.Slider_RH.Trim().ToUpper(), data.Slider_RH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _SliderRH.Add(new DDLTextValue
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
            }
            return Json(_SliderRH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillBRK_PAD(TractorMster data)
        {
            List<DDLTextValue> _BRKPAD = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.BRK_PAD))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.BRK_PAD.Trim().ToUpper(), data.BRK_PAD.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _BRKPAD.Add(new DDLTextValue
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
            }
            return Json(_BRKPAD, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFRB_RH(TractorMster data)
        {
            List<DDLTextValue> _FRBRH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FRB_RH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FRB_RH.Trim().ToUpper(), data.FRB_RH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FRBRH.Add(new DDLTextValue
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
            }
            return Json(_FRBRH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFRB_LH(TractorMster data)
        {
            List<DDLTextValue> _FRBLH = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FRB_LH))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FRB_LH.Trim().ToUpper(), data.FRB_LH.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FRBLH.Add(new DDLTextValue
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
            }
            return Json(_FRBLH, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillFR_AS_RB(TractorMster data)
        {
            List<DDLTextValue> _FRASRB = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.FR_AS_RB))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.FR_AS_RB.Trim().ToUpper(), data.FR_AS_RB.Trim().ToUpper());
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _FRASRB.Add(new DDLTextValue
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
            }
            return Json(_FRASRB, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopup(TractorMster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_SFT_SETTINGS xss WHERE XSS.PARAMVALUE='{0}' AND XSS.PLANT_CODE='{1}'
                       AND XSS.FAMILY_CODE='{2}' AND XSS.PARAMETERINFO='TRACTOR_MASTER_PASSWORD'", data.Password.Trim(),
                       data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Invalid Password..!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopupTab2(TractorMster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_SFT_SETTINGS xss WHERE XSS.PARAMVALUE='{0}' AND XSS.PLANT_CODE='{1}'
                       AND XSS.FAMILY_CODE='{2}' AND XSS.PARAMETERINFO='TRACTOR_MASTER_PASSWORD'", data.PasswordTab2.Trim(),
                       data.T4_Plant.Trim().ToUpper(), data.T4_Family.Trim().ToUpper());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Invalid Password..!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult NewTractorCode(TractorMster data)
        {

            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                string[] item = StringSpliter(data.ItemCode);
                data.ItemCode = item[0].Trim();
                data.ItemCode_Desc = replaceApostophi(item[1].Trim());
                query = string.Format(@"SELECT COUNT(*) FROM XXES_ITEM_MASTER xim WHERE xim.PLANT_CODE='{0}' AND xim.FAMILY_CODE='{1}' 
                        AND xim.ITEM_CODE='{2}'", data.Plant, data.Family, data.ItemCode);
                if (!fun.CheckExits(query))
                {
                    data.Prefix1 = "T05";
                    string Prefix1 = data.Prefix1;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = Prefix1, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }



    }
}