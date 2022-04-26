using EncodeDecode;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;


namespace MVCApp.Controllers
{
    [Authorize]
    public class ItemMasterController : Controller
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

        public PartialViewResult Grid(ItemModel data)
        {
            if (!string.IsNullOrEmpty(data.Family))
            {
                query = @"SELECT PLANT_CODE, FAMILY_CODE, ITEM_CODE, ITEM_DESCRIPTION, ENGINE, ENGINE_DESCRIPTION,
                              BACKEND, BACKEND_DESCRIPTION, TRANSMISSION, TRANSMISSION_DESCRIPTION, REARAXEL, REARAXEL_DESCRIPTION, ORG_ID  
                              FROM item_master where PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
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
                ViewBag.Family = new SelectList(fun.Fill_All_Family_Instead_Tractor(Plant), "Value", "Text");
            }
            return PartialView();
        }

        public JsonResult BindItemCode(ItemModel obj) 
        {
            string query = string.Empty;
            string orgid = fun.getOrgId(Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
            string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
            if (Convert.ToString(obj.Family).ToUpper().Contains("ENGINE"))
            {
                query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1";
            }
            else if (Convert.ToString(obj.Family).ToUpper().Contains("REAR AXEL"))
            {
                query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'AXLE ASSEMBLY%' order by segment1";
            }
            else if (Convert.ToString(obj.Family).ToUpper().Contains("TRANSMISSION"))
            {
                query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1";
            }
            else if (Convert.ToString(obj.Family).ToUpper().Contains("HYDRAULIC"))
            {
                query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'HYDRAULIC LIFT ASSEMBLY%' order by segment1";
            }
            else if (Convert.ToString(obj.Family).ToUpper().Contains("BACK END"))
            {
                query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and (description like 'BACKEND%' or description like 'SKID%') order by segment1";
            }

            //DataTable dt = fun.returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') order by segment1");
           
            List<DDLTextValue> _Item = new List<DDLTextValue>();
            List<DDLTextValue> _Desc = new List<DDLTextValue>();
            DataTable dt = fun.returnDataTable(query);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.AsEnumerable())
                {
                    _Item.Add(new DDLTextValue
                    {                        
                        Value = dr["ITEM_CODE"].ToString(),
                        Text = dr["ITEM_CODE"].ToString()
                        //Text = dr["DESCRIPTION"].ToString()
                    });                    
                }
            }
            var myResult = new
            {
                Item = _Item               
            };
            JsonResult result = Json(myResult, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;            
        }

        public JsonResult BindItemDesc(ItemModel obj)
        {
            string query = string.Empty;
            string desc = string.Empty;
            string orgid = fun.getOrgId(Convert.ToString(obj.Plant).Trim().ToUpper(), Convert.ToString(obj.Family).Trim().ToUpper());
            string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
            string icode = Convert.ToString(obj.ItemCode);
            if (!string.IsNullOrEmpty(icode))
            {
                query = "select distinct description from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and segment1 = '" + icode.Trim() + "'";
                desc = fun.get_Col_Value(query);
            }

            JsonResult result = Json(desc, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }

        public JsonResult Save(ItemModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Family))
                {
                    msg = "Please Choose Date ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.ItemCode))
                {
                    msg = "Item code should not be empty ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Item_Description))
                {
                    msg = "Description should not be empty ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM item_master WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND ITEM_CODE = '" + data.ItemCode.Trim() + "'")) > 0)
                {
                    msg = "Item already exists ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //query = "INSERT INTO item_master (PLANT_CODE, FAMILY_CODE, ITEM_CODE, ITEM_DESCRIPTION, ORG_ID) VALUES('" + data.Plant + "','" + data.Family + "','" + data.ItemCode.Trim() + "','" + data.Item_Description.Trim() + "','" + orgid + "')";
                    query = String.Format(@"INSERT INTO item_master (PLANT_CODE, FAMILY_CODE, ITEM_CODE, 
                            ITEM_DESCRIPTION, ORG_ID) VALUES('{0}','{1}','{2}','{3}','{4}')",data.Plant,data.Family,data.ItemCode.Trim(),data.Item_Description.Trim(),orgid);
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Saved successfully...";
                        mstType = "alert-success";
                    }
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

        public JsonResult Delete(ItemModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;           
            try
            {
                query = "DELETE FROM item_master WHERE PLANT_CODE ='" + data.Plant + "' AND FAMILY_CODE ='" + data.Family + "' AND ITEM_CODE ='" + data.ItemCode + "'";

                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }               
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}