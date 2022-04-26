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

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class CraneMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: CraneMaster
        public ActionResult Index()
        {
            if(string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return View();
            }
           
        }
        public PartialViewResult Grid(CraneMapping data)
        {
            if (string.IsNullOrEmpty(data.PlantCode) || string.IsNullOrEmpty(data.FamilyCode))
            {
                return PartialView();
            }

            string query = string.Format(@"SELECT  PLANT_CODE,FAMILY_CODE,ITEM_CODE,DESCRIPTION,MODEL,MODEL_TYPE,SHORTNAME,ENGINE_DCODE,ENGINE_DESC,PREFIX1,PREFIX2,SUFFIX1,SUFFIX2,REMARKS1,
                        CREATEDBY, to_char(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') as CREATEDDATE,UPDATEDBY,
                        to_char(UPDATEDDATE, 'DD-MM-YYYY HH24:MI:SS') as UPDATEDDATE FROM XXES_CRANE_MASTER WHERE PLANT_CODE = '" + data.PlantCode.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.FamilyCode.ToUpper().Trim() + "' ORDER BY CREATEDDATE DESC ");
            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }
        [HttpGet]
        public JsonResult BindPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindFamily(string PlantCode)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(PlantCode))
            {
                result = fun.Fill_All_Family(PlantCode);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindEngine(string PlantCode, string FamilyCode)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(PlantCode) && !string.IsNullOrEmpty(FamilyCode))
            {
                result = fun.Fill_Engine_Name(PlantCode, FamilyCode);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindCrane(CraneMapping data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
             {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PlantCode) || string.IsNullOrEmpty(data.FamilyCode))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PlantCode).Trim().ToUpper(), Convert.ToString(data.FamilyCode).Trim().ToUpper());

                DataTable dt = new DataTable();

                // query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' order by segment1", data.ItemCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
                query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 2) in ('CF') order by segment1");
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
  
        public JsonResult Save(CraneMapping data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PlantCode) || string.IsNullOrEmpty(data.FamilyCode) || string.IsNullOrEmpty(data.ItemCode) || string.IsNullOrEmpty(data.ModelType) || string.IsNullOrEmpty(data.EngineDcode))
                {
                    msg = Validation.str25;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ItemCode.Split(spearator, StringSplitOptions.None);
                data.ItemCode = SplitItemCode[0];
                data.DesCription = SplitItemCode[1];
               
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_CRANE_MASTER WHERE PLANT_CODE = '" + data.PlantCode.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.FamilyCode.ToUpper().Trim() + "' AND ITEM_CODE = '" + data.ItemCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Item code already exist with same plant and family";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                    
                if (fun.InsertCraneMaster(data))
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

        public JsonResult Update(CraneMapping data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PlantCode) || string.IsNullOrEmpty(data.FamilyCode) || string.IsNullOrEmpty(data.ItemCode) || string.IsNullOrEmpty(data.ModelType) || string.IsNullOrEmpty(data.EngineDcode))
                {
                    msg = Validation.str25;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ItemCode.Split(spearator, StringSplitOptions.None);
                if (SplitItemCode.Length > 1)
                {
                    data.ItemCode = SplitItemCode[0];
                    data.DesCription = SplitItemCode[1];
                }
                else 
                { 
                    data.ItemCode = SplitItemCode[0]; 
                }

                if (fun.UpdateCraneMaster(data))
                {

                    msg = Validation.str11;
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
        public JsonResult Delete(CraneMapping data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PlantCode) && string.IsNullOrEmpty(data.FamilyCode) && string.IsNullOrEmpty(data.ItemCode))
                {
                    msg = Validation.str2;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                if (fun.DeleteCraneMaster(data))
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