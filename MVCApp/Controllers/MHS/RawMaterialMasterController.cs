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
    public class RawMaterialMasterController : Controller
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
        public JsonResult BindItemCode(RawMaterialMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                DataTable dt = new DataTable();
                
                // query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%' order by segment1", data.ItemCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
                query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                "where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%' )order by segment1", data.ItemCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
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
        public JsonResult Add(RawMaterialMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.ItemCode) || string.IsNullOrEmpty(data.PackingStandard))
                {
                    msg = Validation.str24; 
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                
                string[] SplitItemCode = data.ItemCode.Split(new char[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
                data.ItemCode = SplitItemCode[0].Trim();
                data.ItemDescription = fun.replaceApostophi(SplitItemCode[1]).Trim(); ;

                string pckStd = data.PackingStandard.Trim();
                

                int j;
                if (!int.TryParse(pckStd, out j))
                {
                    
                    msg = "Packing Standard is not a valid number ..";
                    mstType = Validation.str1; ;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (!(j > 0 ))
                {
                    msg = "Packing Standard should be greater than 0 ..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_RAWMATERIAL_MASTER WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND ITEM_CODE = '" + data.ItemCode.Trim() + "'")) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertRawMaterialMaster(data))
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
            catch(Exception ex)
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

        public JsonResult Update(RawMaterialMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.ItemCode) || string.IsNullOrEmpty(data.PackingStandard))
                {
                    msg = Validation.str24;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ItemCode.Split(spearator, StringSplitOptions.None);
                data.ItemCode = SplitItemCode[0];
                data.ItemDescription = fun.replaceApostophi(SplitItemCode[1]);

                string pckStd = data.PackingStandard.Trim();


                int j;
                if (!int.TryParse(pckStd, out j))
                {

                    msg = "Packing Standard is not a valid number ..";
                    mstType = Validation.str1; ;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (!(j > 0))
                {
                    msg = "Packing Standard should be greater than 0 ..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.UpdateRawMaterialMaster(data))
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

        public PartialViewResult Grid(RawMaterialMaster data)
        {
            if(string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
            {
                return PartialView();
            }
            
          string query = string.Format(@"SELECT AUTOID,PLANT_CODE,FAMILY_CODE,ITEM_CODE,SUBSTR(ITEM_DESCRIPTION,1,40) AS ITEM_DESCRIPTION,PACKING_STANDARD,
                        CREATEDBY, to_char(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') as CREATEDDATE,UPDATEDBY,
                        to_char(UPDATEDDATE, 'DD-MM-YYYY HH24:MI:SS') as UPDATEDDATE FROM XXES_RAWMATERIAL_MASTER WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' ORDER BY CREATEDDATE DESC ");

            string gridType = "Grid";
            DataTable dt = fun.returnDataTable(query);
            if(dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string plant = row["PLANT_CODE"].ToString();
                string family = row["FAMILY_CODE"].ToString();
                if (plant.Contains("T02") && family.Contains("TRACTOR EKI"))
                {
                    gridType = "EKIgrid";
                }
            }
            
            
           ViewBag.DataSource = dt;
           return PartialView(gridType);
        }

        public JsonResult Delete(RawMaterialMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.AutoId))
                {
                    msg = Validation.str2;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                if (fun.DeleteRawMaterialMaster(data))
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