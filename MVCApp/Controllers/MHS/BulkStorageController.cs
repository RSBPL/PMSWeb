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

namespace MVCApp.Controllers.Mrn
{
    [Authorize]
    public class BulkStorageController : Controller
    {
        // GET: BulkStorage

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
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult BindItemCode(BulkStorage data)
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

                query = string.Format(@"SELECT ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION, ITEM_CODE 
                                        FROM XXES_RAWMATERIAL_MASTER WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND
                                    (ITEM_CODE LIKE '%{2}%' OR ITEM_DESCRIPTION LIKE '%{2}%')", data.Plant.Trim(),
                                  data.Family.Trim(), data.Item.Trim().ToUpper());

                //query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%' )order by segment1", data.Item.Trim().ToUpper(), data.Item.Trim().ToUpper());
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


     

        public PartialViewResult Grid(BulkStorage data)
        {
            if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
            {
                return PartialView();
            }

            string query = string.Format(@"select AUTOID , PLANT_CODE, FAMILY_CODE, LOCATION_CODE, ITEM_CODE,SAFTY_STOCK_QUANTITY,MAX_INVENTORY,NO_OF_LOC_ALLOCATED,PACKAGING_TYPE,VERTICAL_STACKING_LEVEL,BULK_STORAGE_SNP,USAGE_PER_TRACTOR,REVISION,
                         (select SUBSTR(M.ITEM_DESCRIPTION,1,50) AS ITEM_DESCRIPTION from XXES_RAWMATERIAL_MASTER m where m.item_code=s.item_code and m.plant_code=s.plant_code and m.family_code=s.family_code AND ROWNUM <= 1) as ITEM_DESCRIPTION, CAPACITY, 
                        TEMP_LOC, UNPACKED,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE, UPDATED_BY, 
                       TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE FROM XXES_Bulk_Storage s WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' ORDER BY CREATED_DATE DESC ");

            string gridType = "Grid";
            DataTable dt = fun.returnDataTable(query);
            if(dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string plant = row["PLANT_CODE"].ToString();
                string family = row["FAMILY_CODE"].ToString();
                if(plant.Contains("T02") && family.Contains("TRACTOR EKI"))
                {
                    gridType = "EKIgrid";
                }
            }
            ViewBag.DataSource = dt;
            return PartialView(gridType);
        }

        public PartialViewResult EKIgrid(BulkStorage data)
        {
            if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
            {
                return PartialView();
            }

            string query = string.Format(@"select AUTOID , PLANT_CODE, FAMILY_CODE, LOCATION_CODE, ITEM_CODE,SAFTY_STOCK_QUANTITY,MAX_INVENTORY,NO_OF_LOC_ALLOCATED,PACKAGING_TYPE,VERTICAL_STACKING_LEVEL,BULK_STORAGE_SNP,USAGE_PER_TRACTOR,REVISION,
                         (select SUBSTR(M.ITEM_DESCRIPTION,1,50) AS ITEM_DESCRIPTION from XXES_RAWMATERIAL_MASTER m where m.item_code=s.item_code and m.plant_code=s.plant_code and m.family_code=s.family_code AND ROWNUM <= 1) as ITEM_DESCRIPTION, CAPACITY,                        
                        TEMP_LOC,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE, UPDATED_BY, 
                       TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE FROM XXES_Bulk_Storage s WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' ORDER BY CREATED_DATE DESC ");
            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }

        public string replaceApostophi(string chkstr)
        {
            return chkstr.Replace("'", "''");
        }
        
        [HttpPost]
        
        public JsonResult Save(BulkStorage data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (data.TempLoc == false)
                {
                    if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.Location)
                       || string.IsNullOrEmpty(data.Capacity) || string.IsNullOrEmpty(data.SftStkQuantity))
                    {
                        msg = Validation.str26;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family)
                        || string.IsNullOrEmpty(data.Location))
                    {
                        msg = Validation.str40;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                
               
                string query = string.Empty;
                if (!string.IsNullOrEmpty(data.Item))
                {
                    char[] spearator = { '#' };
                    String[] SplitItemCode = data.Item.Split(spearator, StringSplitOptions.None);
                    data.Item = SplitItemCode[0].ToUpper().Trim();
                    data.Description = replaceApostophi(SplitItemCode[1]);
                }
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_Bulk_Storage WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND LOCATION_CODE = '" + data.Location.ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str28;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(data.TempLoc == false)
                {
                    if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_Bulk_Storage WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND ITEM_CODE = '" + data.Item.ToUpper().Trim() + "'")) > 0)
                    {
                        msg = Validation.str39;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.SftStkQuantity))
                {
                   
                    if (Convert.ToInt32(data.SftStkQuantity.Trim().ToUpper()) <= 0)
                    {
                        msg = Validation.str37;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.SftStkQuantity) && !string.IsNullOrEmpty(data.Capacity))
                {
                    if (Convert.ToInt32(data.SftStkQuantity.Trim().ToUpper()) > Convert.ToInt32(data.Capacity.Trim().ToUpper()))
                    {
                        msg = Validation.str36;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.MaxInventory))
                {
                    Double j;
                    if (!Double.TryParse(data.MaxInventory.Trim(), out j))
                    {
                        msg = "Maximum Inventory should be number only..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                        
                    }
                    if (j <= 0)
                    {
                        msg = "Maximum Inventory Should be Greater Than 0";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                if (!string.IsNullOrEmpty(data.PackingType))
                {
                    data.PackingType = data.PackingType.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.VerticalStkLevel))
                {
                    data.VerticalStkLevel = data.VerticalStkLevel.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.BulkStoreSNP))
                {
                    data.BulkStoreSNP = data.BulkStoreSNP.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.UsagePerTractor))
                {
                    data.UsagePerTractor = data.UsagePerTractor.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.Revision))
                {
                    data.Revision = data.Revision.ToUpper().Trim();
                }
              
                query = string.Format(@"INSERT INTO XXES_Bulk_Storage(PLANT_CODE, FAMILY_CODE, LOCATION_CODE,ITEM_CODE,
                         CAPACITY, SAFTY_STOCK_QUANTITY ,NO_OF_LOC_ALLOCATED,PACKAGING_TYPE,VERTICAL_STACKING_LEVEL,BULK_STORAGE_SNP,USAGE_PER_TRACTOR,REVISION,
                         TEMP_LOC,UNPACKED, CREATED_BY, CREATED_DATE,MAX_INVENTORY ) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}', '{14}',SYSDATE,'{15}')"
                         , data.Plant.ToUpper().Trim(), data.Family.ToUpper().Trim(), data.Location.ToUpper().Trim(), data.Item,
                         Convert.ToString(data.Capacity), Convert.ToString(data.SftStkQuantity), Convert.ToString(data.NoOfLocAllocated), 
                         data.PackingType,data.VerticalStkLevel, data.BulkStoreSNP, data.UsagePerTractor, data.Revision, (data.TempLoc ? "Y" : "N"), (data.chkUnpck ? "Y" : "N"), HttpContext.Session["Login_User"].ToString(),data.MaxInventory.Trim());
                
                if (fun.EXEC_QUERY(query))
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

        [HttpPost]
        public JsonResult Update(BulkStorage data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (data.TempLoc == false)
                {
                    if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.Location) 
                        || string.IsNullOrEmpty(data.Capacity) || string.IsNullOrEmpty(data.SftStkQuantity))
                    {
                        msg = Validation.str26;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family)
                       || string.IsNullOrEmpty(data.Location))
                    {
                        msg = Validation.str40;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                 
                string query = string.Empty;
                if (!string.IsNullOrEmpty(data.Item))
                {
                    char[] spearator = { '#' };
                    String[] SplitItemCode = data.Item.Split(spearator, StringSplitOptions.None);
                    data.Item = SplitItemCode[0].ToUpper().Trim();
                    data.Description = replaceApostophi(SplitItemCode[1]);
                }
                if(data.TempLoc == false)
                {
                    query = string.Format(@"Select count(*) from XXES_Bulk_Storage where 
                        LOCATION_CODE = '" + data.Location.Trim() + "' and PLANT_CODE = '" + data.Plant.Trim() + "' AND " +
                      "FAMILY_CODE = '" + data.Family.Trim() + "' AND AUTOID <> '" + data.AutoId + "'");

                    if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                    {
                        msg = "Location already exists..!";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    query = string.Format(@"Select count(*) from XXES_Bulk_Storage where 
                        LOCATION_CODE = '" + data.Location.Trim() + "' and PLANT_CODE = '" + data.Plant.Trim() + "' AND " +
                       "FAMILY_CODE = '" + data.Family.Trim() + "' AND AUTOID <> '" + data.AutoId + "'");

                    if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                    {
                        msg = "Location already exists..!";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                
                query = string.Format(@"Select count(*) from XXES_Bulk_Storage where 
                        ITEM_CODE = '" + data.Item + "' and PLANT_CODE = '" + data.Plant.Trim() + "' AND " +
                      "FAMILY_CODE = '" + data.Family.Trim() + "' AND AUTOID <> '" + data.AutoId + "'");

                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "ItemCode already exists..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.SftStkQuantity))
                {
                    if (Convert.ToInt32(data.SftStkQuantity.Trim().ToUpper()) <= 0)
                    {
                        msg = Validation.str37;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.SftStkQuantity) && !string.IsNullOrEmpty(data.Capacity))
                {
                    if (Convert.ToInt32(data.SftStkQuantity.Trim().ToUpper()) > Convert.ToInt32(data.Capacity.Trim().ToUpper()))
                    {
                        msg = Validation.str36;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.MaxInventory))
                {
                    Double j;
                    if (!Double.TryParse(data.MaxInventory.Trim(), out j))
                    {
                        msg = "Maximum Inventory should be number only..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);

                    }
                    if (j <= 0)
                    {
                        msg = "Maximum Inventory Should be Greater Than 0";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.PackingType))
                {
                    data.PackingType = data.PackingType.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.VerticalStkLevel))
                {
                    data.VerticalStkLevel = data.VerticalStkLevel.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.BulkStoreSNP))
                {
                    data.BulkStoreSNP = data.BulkStoreSNP.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.UsagePerTractor))
                {
                    data.UsagePerTractor = data.UsagePerTractor.ToUpper().Trim();
                }
                if (!string.IsNullOrEmpty(data.Revision))
                {
                    data.Revision = data.Revision.ToUpper().Trim();
                }
                
                query = string.Format(@"
                            UPDATE XXES_BULK_STORAGE 
                            SET 
                            PLANT_CODE='{0}',
                            FAMILY_CODE='{1}',
                            ITEM_CODE='{2}', 
                            LOCATION_CODE='{3}',
                            CAPACITY='{4}',
                            SAFTY_STOCK_QUANTITY='{5}',
                            NO_OF_LOC_ALLOCATED='{6}',
                            PACKAGING_TYPE='{7}',
                            VERTICAL_STACKING_LEVEL='{8}',
                            BULK_STORAGE_SNP='{9}',
                            USAGE_PER_TRACTOR='{10}', 
                            REVISION='{11}',
                            TEMP_LOC='{12}',
                            UPDATED_BY='{13}',
                            UPDATED_DATE=SYSDATE, 
                            UNPACKED='{15}',
                            MAX_INVENTORY= '{16}'
                            WHERE AUTOID='{14}'", 
                        
                            data.Plant.ToUpper().Trim(),
                            data.Family.ToUpper().Trim(), 
                            data.Item,
                            data.Location.ToUpper().Trim(), 
                            data.Capacity,
                            data.SftStkQuantity, 
                            data.NoOfLocAllocated, 
                            data.PackingType, 
                            data.VerticalStkLevel, 
                            data.BulkStoreSNP, 
                            data.UsagePerTractor,
                            data.Revision,
                            (data.TempLoc ? "Y" : "N"), 
                            HttpContext.Session["Login_User"].ToString().ToUpper().Trim(),
                            data.AutoId,
                            (data.chkUnpck ? "Y" : "N"),
                            data.MaxInventory.Trim());

                if (fun.EXEC_QUERY(query))
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
        public JsonResult Delete(BulkStorage data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_BULKSTORAGEITEMS WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND LOCATION_CODE = '" + data.Location.ToUpper().Trim() + "'")) > 0)
                {
                    msg = data.Location + " Location can not delete..";
                    mstType = Validation.str;
                    status = Validation.stus;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = "delete from XXES_Bulk_Storage where AUTOID ='" + data.AutoId + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = Validation.str23;
                        mstType = Validation.str;
                        status = Validation.stus;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        msg = Validation.str2;
                        mstType = Validation.str1;
                        status = Validation.str2;

                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

       
    }
}