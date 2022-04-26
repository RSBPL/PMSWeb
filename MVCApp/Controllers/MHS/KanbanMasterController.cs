using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class KanbanMasterController : Controller
    {
        // GET: KanbanMaster
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
        public JsonResult BindItem(KanbanMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                DataTable dt = new DataTable();
                

                query = string.Format(@"SELECT ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION, ITEM_CODE 
                                        FROM XXES_RAWMATERIAL_MASTER WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND
                                    (ITEM_CODE LIKE '%{2}%' OR ITEM_DESCRIPTION LIKE '%{3}%')",data.Plant.Trim(),
                                    data.Family.Trim(),data.Item.Trim().ToUpper(),data.Item.Trim().ToUpper());
                
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
        public JsonResult BindSuperLocation(KanbanMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                DataTable dt = new DataTable();


                //query = string.Format(@"SELECT DISTINCT LOCATION_NAME   
                //                        FROM XXES_SUPERMKT_LOCATIONS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND
                //                        (LOCATION_NAME LIKE '%{2}%')
                //                        ", data.Plant.Trim(), data.Family.Trim(), data.SuperMarketLoc.Trim().ToUpper());

                query = string.Format(@"SELECT DISTINCT LOCATION_NAME   
                                        FROM XXES_SUPERMKT_LOCATIONS sml WHERE location_name NOT IN 
                                        (SELECT km.SUMKTLOC FROM XXES_KANBAN_MASTER km 
                                        WHERE km.plant_code= sml.plant_code and km.family_code = sml.family_code)
                                        and   sml.PLANT_CODE = '{0}' AND sml.FAMILY_CODE = '{1}' AND
                                       sml.LOCATION_NAME LIKE '%{2}%'", data.Plant.Trim(), data.Family.Trim(), 
                                       data.SuperMarketLoc.Trim().ToUpper());

                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["LOCATION_NAME"].ToString(),
                            Value = dr["LOCATION_NAME"].ToString(),
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
        public JsonResult BindKanbanNo(KanbanMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            
            try
            {
                //if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                //{
                //    return Json(null, JsonRequestBehavior.AllowGet);
                //}
                DataTable dt = new DataTable();
                query = string.Format(@"SELECT KANBAN_NO
                                      FROM XXES_KANBAN_MASTER
                                      WHERE (KANBAN_NO LIKE '%{0}%')
                                      ORDER BY AUTOID ASC", data.KanbanNumber.Trim().ToUpper());
                dt = fun.returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["KANBAN_NO"].ToString(),
                            Value = dr["KANBAN_NO"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            //var result = new { kanNo = Item, kandata = kanban };
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindDataforKanbanNo(KanbanMaster data) 
        {
            KanbanMaster kanban = new KanbanMaster();
            DataTable dt = new DataTable();
            try
            {
                //query = string.Format(@"SELECT KANBAN_NO,
                //                        PLANT_CODE,
                //                        FAMILY_CODE,
                //                        ITEM_CODE,
                //                        ITEM_DESCRIPTION,
                //                        SUMKTLOC,
                //                        CAPACITY,
                //                        SAFTY_STOCK_QUANTITY,
                //                        NO_OF_BINS,
                //                        USAGE_PER_TRACTOR
                //                      FROM XXES_KANBAN_MASTER
                //                      WHERE (KANBAN_NO LIKE '%{0}%')
                //                        and ROWNUM <= 1
                //                      ORDER BY AUTOID ASC", data.KanbanNumber.Trim().ToUpper());

                query = string.Format(@"SELECT AUTOID, PLANT_CODE, FAMILY_CODE, KANBAN_NO, ITEM_CODE, ITEM_DESCRIPTION, SUMKTLOC, CAPACITY, SAFTY_STOCK_QUANTITY, NO_OF_BINS, USAGE_PER_TRACTOR,
                    CREATED_BY, to_char(CREATED_DATE, 'DD-MM-YYYY HH24:MI:SS') as CREATED_DATE,UPDATED_BY,
                    to_char(UPDATED_DATE, 'DD-MM-YYYY HH24:MI:SS') as UPDATED_DATE FROM XXES_KANBAN_MASTER WHERE(KANBAN_NO LIKE '%{0}%')and ROWNUM <= 1 ORDER BY AUTOID ASC", data.KanbanNumber.Trim().ToUpper());

                dt = fun.returnDataTable(query);
                {
                    data.Plant = dt.Rows[0]["PLANT_CODE"].ToString();
                    data.Family = dt.Rows[0]["FAMILY_CODE"].ToString();
                    data.Item = dt.Rows[0]["ITEM_CODE"].ToString();
                    data.Description = dt.Rows[0]["ITEM_DESCRIPTION"].ToString();
                    data.SuperMarketLoc = dt.Rows[0]["SUMKTLOC"].ToString();
                    data.Capacity = dt.Rows[0]["CAPACITY"].ToString();
                    data.SftStkQuantity = dt.Rows[0]["SAFTY_STOCK_QUANTITY"].ToString();
                    data.NoOfBins = dt.Rows[0]["NO_OF_BINS"].ToString();
                    data.UsagePerTractor = dt.Rows[0]["USAGE_PER_TRACTOR"].ToString();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        

        [HttpPost]
        public JsonResult Save(KanbanMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;

            try
            {
            
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.Item) 
                    || string.IsNullOrEmpty(data.SuperMarketLoc) || string.IsNullOrEmpty(data.Capacity) || string.IsNullOrEmpty(data.SftStkQuantity) || 
                    string.IsNullOrEmpty(data.NoOfBins))
                {
                    msg = Validation.str26;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                //if (!string.IsNullOrEmpty(data.SftStkQuantity))
                //{
                //    data.SftStkQuantity = data.SftStkQuantity.ToUpper().Trim();
                //}
                if (Convert.ToInt32(data.Capacity.Trim()) <= 0)
                {
                    msg = "Capacity Should be Greater Than 0";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(data.SftStkQuantity.Trim()) <= 0)
                {
                    msg = Validation.str37;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                    if (Convert.ToInt32(data.NoOfBins.Trim()) <= 0)
                    {
                        msg = Validation.str38;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                if(!string.IsNullOrEmpty(data.UsagePerTractor))
                {
                    if (Convert.ToInt32(data.UsagePerTractor.Trim()) <= 0)
                    {
                        msg = "Usage Per Tractor Should be Greater Than 0"; ;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        data.UsagePerTractor = data.UsagePerTractor.Trim();
                    }
                }

                if (Convert.ToInt32(data.SftStkQuantity.Trim()) > Convert.ToInt32(data.Capacity.Trim()))
                {
                    msg = Validation.str36;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(data.Capacity.Trim()) % Convert.ToInt32(data.NoOfBins.Trim()) != 0)
                {
                    msg = "Invalid Capacity or No of bins...!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.InsertKanbanMaster(data))
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

        public JsonResult Update(KanbanMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.Item) || string.IsNullOrEmpty(data.SuperMarketLoc) 
                     || string.IsNullOrEmpty(data.Capacity) || string.IsNullOrEmpty(data.SftStkQuantity) ||
                    string.IsNullOrEmpty(data.NoOfBins))
                {
                    msg = Validation.str26;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.Item.Split(spearator, StringSplitOptions.None);
                data.Item = SplitItemCode[0];
                data.Description = SplitItemCode[1];

                //if (!string.IsNullOrEmpty(data.NoOfBins))
                //{
                //    if (Convert.ToInt32(data.NoOfBins.Trim().ToUpper()) <= 0)
                //    {
                //        msg = Validation.str38;
                //        mstType = Validation.str1;
                //        status = Validation.str2;
                //        var resul = new { Msg = msg, ID = mstType, validation = status };
                //        return Json(resul, JsonRequestBehavior.AllowGet);
                //    }
                //}
                if (Convert.ToInt32(data.Capacity.Trim()) <= 0)
                {
                    msg = "Capacity Should be Greater Than 0";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(data.SftStkQuantity.Trim()) <= 0)
                {
                    msg = Validation.str37;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(data.SftStkQuantity.Trim()) > Convert.ToInt32(data.Capacity.Trim()))
                {
                    msg = Validation.str36;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(data.Capacity.Trim()) % Convert.ToInt32(data.NoOfBins.Trim()) != 0)
                {
                    msg = "Invalid Capacity or No of bins...!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.UpdatekanbanMaster(data))
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

        public PartialViewResult Grid(KanbanMaster data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return PartialView("Grid");
                }

                string query = string.Format(@"SELECT AUTOID,PLANT_CODE,FAMILY_CODE,KANBAN_NO,ITEM_CODE,ITEM_DESCRIPTION,SUMKTLOC,CAPACITY,SAFTY_STOCK_QUANTITY,NO_OF_BINS,USAGE_PER_TRACTOR,
                        CREATED_BY, to_char(CREATED_DATE, 'DD-MM-YYYY HH24:MI:SS') as CREATED_DATE,UPDATED_BY,
                        to_char(UPDATED_DATE, 'DD-MM-YYYY HH24:MI:SS') as UPDATED_DATE FROM XXES_KANBAN_MASTER WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' ORDER BY AUTOID DESC ");
                ViewBag.DataSource = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }

            
            return PartialView("Grid");
        }

        public JsonResult Delete(KanbanMaster data)
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

                if (fun.DeletekanbanMaster(data))
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