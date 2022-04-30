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
using System.Configuration;

namespace MVCApp.Controllers
{
    [Authorize]
    public class SubAssemblyMappingMasterController : Controller
    {
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

        public JsonResult Add(SubAssemblyMapping data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty; string status = string.Empty;
            try
            {
                string Shiftcode = "", isDayNeedToLess = ""; DateTime ServerDate = new DateTime(); int SettingRollingDays = 0; int Setting_FixPlanDays = 0; string SR = string.Empty;
                DateTime date;
                string query = string.Empty;

                char[] spearator = { '#' };
                if(string.IsNullOrEmpty(data.MainSubAssembly))
                {
                    msg = "Main Sub-Assembly is required..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string[] MainSubAssembly = data.MainSubAssembly.Split(spearator, StringSplitOptions.None);
                data.MainSubAssembly = MainSubAssembly[0];
                data.MainDescription = fun.replaceApostophi(MainSubAssembly[1]);

                if (!string.IsNullOrEmpty(data.SubAssembly1))
                {
                    string[] Description1 = data.SubAssembly1.Split(spearator, StringSplitOptions.None);
                    data.SubAssembly1 = Description1[0].Trim();
                    data.Description1 = fun.replaceApostophi(Description1[1]).Trim();
                }

                if (!string.IsNullOrEmpty(data.SubAssembly2))
                {
                    string[] Description2 = data.SubAssembly2.Split(spearator, StringSplitOptions.None);
                    data.SubAssembly2 = Description2[0].Trim();
                    data.Description2 = fun.replaceApostophi(Description2[1]).Trim();
                }

                query = string.Format(@"select  count(*) from XXES_SUB_ASSEMBLY_MASTER where plant_code = '{0}' and family_code = '{1}' 
                                        and main_sub_assembly = '{2}'", data.Plant.Trim(),data.Family.Trim(),data.MainSubAssembly.Trim());
                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "Main Sub-Assembly is already exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                    query = "INSERT INTO XXES_SUB_ASSEMBLY_MASTER(PLANT_CODE, FAMILY_CODE, MAIN_SUB_ASSEMBLY, DESCRIPTION, SUBASSEMBLY1, DESCRIPTION1, SUBASSEMBLY2, DESCRIPTION2, CREATEDBY, CREATEDDATE, SHORT_CODE) " +
                    "VALUES('" + data.Plant + "','" + data.Family + "','" + data.MainSubAssembly.Trim() + "','" + data.MainDescription.Trim() + "','" + data.SubAssembly1 + "','" + data.Description1 + "','" + data.SubAssembly2 + "','" + data.Description2 + "','" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "',SYSDATE, '" + data.ShortCode + "')";
                if (fun.EXEC_QUERY(query))
                {
                    //fun.Insert_Into_ActivityLog("XXES_SUB_ASSEMBLY_MASTER", "INSERT", data.MainSubAssembly, query, data.Plant, data.Family);
                    msg = Validation.str9;
                    mstType = Validation.str;
                    status = Validation.stus;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(SubAssemblyMapping data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                //Delete Sub assembly item from XXES_DAILY_PLAN_ASSEMBLY
                query = "DELETE FROM XXES_SUB_ASSEMBLY_MASTER WHERE AUTOID = '" + data.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Data Deleted Successfully...";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
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
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(SubAssemblyMapping data)
        {

            query = "SELECT * FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + data.Plant + "' ";

            if (!string.IsNullOrEmpty(data.Family))
            {
                query += "AND FAMILY_CODE = '" + data.Family + "' ";
            }
            if (!string.IsNullOrEmpty(data.MainSubAssembly))
            {
                char[] spearator = { '#' };

                String[] MainSubAssembly = data.MainSubAssembly.Split(spearator, StringSplitOptions.None);
                data.MainSubAssembly = MainSubAssembly[0];
                data.MainDescription = MainSubAssembly[1];
                query += "AND MAIN_SUB_ASSEMBLY = '" + data.MainSubAssembly.Trim() + "'";
            }

            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
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

        public PartialViewResult BindMainSubAssembly(SubAssemblyMapping data)
        {
            ViewBag.SubAssembly2 = new SelectList(FillItemMaster(data), "Value", "Text");
            return PartialView();
        }

        private List<DDLTextValue> FillItemMaster(SubAssemblyMapping data)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string query = string.Empty;
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Item;
                }

                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (Convert.ToString(data.Family).ToUpper().Contains("ENGINE"))
                {
                    query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1";
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("REAR AXEL"))
                {
                    query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'AXLE ASSEMBLY%' order by segment1";
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("TRANSMISSION"))
                {
                    query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1";
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("HYDRAULIC"))
                {
                    query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'HYDRAULIC LIFT ASSEMBLY%' order by segment1";
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                {
                    query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and (description like 'BACKEND%' or description like 'SKID%') order by segment1";
                }
                if (!string.IsNullOrEmpty(query))
                {
                    dt = fun.returnDataTable(query);
                }
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["DESCRIPTION"].ToString(),
                        });
                    }
                }
                return Item;
            }
            catch (Exception ex)
            {
                //throw;
                return Item;
            }
            finally { }
        }

       public JsonResult FillSubAssembly1(SubAssemblyMapping data)
        {
            List<SubAssemblyMappingDropdown> Item = new List<SubAssemblyMappingDropdown>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SubAssembly1))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                }
                

                dt = fun.returnDataTable(query);

                //if (!string.IsNullOrEmpty(query))
                //{
                //    dt = fun.returnDataTable(query);
                //}
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new SubAssemblyMappingDropdown
                        {
                            DESCRIPTION = dr["DESCRIPTION"].ToString(),
                            ITEM_CODE = dr["ITEM_CODE"].ToString(),
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

       public JsonResult FillSubAssembly2(SubAssemblyMapping data)
        {
            List<SubAssemblyMappingDropdown> Item = new List<SubAssemblyMappingDropdown>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.SubAssembly2))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.SubAssembly2.Trim().ToUpper(), data.SubAssembly2.Trim().ToUpper());

                }
                

                dt = fun.returnDataTable(query);

                //if (!string.IsNullOrEmpty(query))
                //{
                //    dt = fun.returnDataTable(query);
                //}
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new SubAssemblyMappingDropdown
                        {
                            DESCRIPTION = dr["DESCRIPTION"].ToString(),
                            ITEM_CODE = dr["ITEM_CODE"].ToString(),
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

        
    }
}