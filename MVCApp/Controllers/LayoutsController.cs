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
    public class LayoutsController : Controller
    {
        // GET: BulkLayout

        OracleCommand cmd;
        // OracleDataAdapter DA;
        OracleDataReader dr;
        OracleDataAdapter da;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Account", "Login");
            }
            else
            {
                ViewBag.Operation = fun.BindOperation();
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
        public PartialViewResult ViewLayout(BulkLayout layout)
        {
            string _layoutType = string.Empty;
            if (layout.OPERATION == "VL")
            {
                BULKSTORAGE(layout);
                _layoutType = "BulkStorageLayout";
                //return PartialView("BulkStorageLayout");
            }
            else if (layout.OPERATION == "SM")
            {
                SMSTORAGE(layout);
                _layoutType = "SuperMktLayout";
                //return PartialView("SuperMktLayout");
            }
            return PartialView(_layoutType);

        }

        [NonAction]

        public void BULKSTORAGE(BulkLayout layout)
        {
            try
            {
                List<LayoutModeSubList> UL = new List<LayoutModeSubList>();
                //if (layout.Search == null)
                //{
                //    query = @"SELECT  M.TEMP_LOC,M.ITEM_CODE,M.LOCATION_CODE,M.LOCATION_CODE, nvl((SELECT SUM(S.QUANTITY) FROM XXES_BULKSTORAGEITEMS s WHERE 
                //          s.PLANT_CODE=M.PLANT_CODE AND s.FAMILY_CODE=M.FAMILY_CODE AND s.LOCATION_CODE=M.LOCATION_CODE) ,0) Qty           
                //         FROM XXES_BULK_STORAGE m WHERE m.PLANT_CODE= '" + layout.Plant.ToUpper().Trim() + "' " +
                //              "AND m.FAMILY_CODE= '" + layout.Family.ToUpper().Trim() + "' AND m. temp_loc IS NOT NULL order by m.LOCATION_CODE";
                //}
                if (!string.IsNullOrEmpty(layout.Search))
                {
                    layout.Search = layout.Search.ToUpper();
                }
                query = string.Format(@"SELECT  M.TEMP_LOC,M.ITEM_CODE,M.LOCATION_CODE,M.CAPACITY,nvl((SELECT SUM(S.QUANTITY) FROM xxes_bulkstock s 
                        WHERE s.PLANT_CODE=M.PLANT_CODE AND s.FAMILY_CODE=M.FAMILY_CODE AND s.LOCATION_CODE=M.LOCATION_CODE) ,0) Qty           
                        FROM XXES_BULK_STORAGE m WHERE m.PLANT_CODE= '{0}' AND m.FAMILY_CODE= '{1}' AND ('%{2}%' IS NULL OR M.ITEM_CODE like '%{2}%' OR M.LOCATION_CODE LIKE '%{2}%')  
                        AND m. temp_loc IS NOT NULL order by m.LOCATION_CODE", layout.Plant.ToUpper(), layout.Family.ToUpper(), layout.Search);

                da = new OracleDataAdapter(query, fun.Connection());
                DataTable dt = new DataTable();
                da.Fill(dt);

                //da = new OracleDataAdapter("SELECT * FROM XXES_BULK_STORAGE WHERE PLANT_CODE= '" + layout.Plant.ToUpper().Trim() + "' AND FAMILY_CODE= '" + layout.Family.ToUpper().Trim() + "'", fun.Connection());
                //DataTable CellDT = new DataTable();
                //da.Fill(CellDT);

                if (dt.Rows.Count > 0)
                {

                    List<BulkLayout> IL = new List<BulkLayout>();
                    //var DBS = new List<dropdwonForBulkStorage>();
                    foreach (DataRow dr in dt.Rows)
                    {

                        IL.Add(new BulkLayout
                        {
                            Plant = layout.Plant,
                            Family = layout.Family,
                            Location = Convert.ToString(dr["LOCATION_CODE"]),
                            QTY = Convert.ToString(dr["QTY"]),
                            TEMP_LOC = Convert.ToString(dr["TEMP_LOC"]),
                            Capacity = Convert.ToString(dr["CAPACITY"]),
                            //DDLLayoutValue = "VL",
                            dllLocation = Convert.ToString(dr["LOCATION_CODE"]),

                        });
                        //var dbs = new dropdwonForBulkStorage();
                        //dbs.dllItemCode = Convert.ToString(dr["ITEM_CODE"]);
                        //dbs.dllLocation = Convert.ToString(dr["LOCATION_CODE"]);
                        //DBS.Add(dbs);

                    }
                    UL.Add(new LayoutModeSubList { BulkLayoutMode = IL });
                    ViewBag.Layout = UL;

                    //List<SelectListItem> ObjList = new List<SelectListItem>();
                    //foreach (var item in DBS)
                    //{
                    //    ObjList.Add(new SelectListItem { Text = item.dllLocation.ToString(), Value = item.dllLocation.ToString() });
                    // }
                    //List<SelectListItem> ObjListItem = new List<SelectListItem>();
                    //foreach (var item in DBS)
                    //{

                    //    ObjListItem.Add(new SelectListItem { Text = item.dllItemCode.ToString(), Value = item.dllItemCode.ToString() });
                    //}
                    //ViewBag.dllLocation = ObjList;
                    //ViewBag.dllitemcode = ObjListItem;
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
        }
        [NonAction]
        public void SMSTORAGE(BulkLayout layout)
        {
            try
            {
                List<LayoutSuperMktSubList> UL = new List<LayoutSuperMktSubList>();
                //query = String.Format(@"SELECT L.LOCATION_NAME,
                //         NVL((SELECT SUM(I.QUANTITY)
                //             FROM XXES_SUMMKTSTOCK I
                //             WHERE I.PLANT_CODE = L.PLANT_CODE
                //               AND I.FAMILY_CODE = L.FAMILY_CODE
                //               AND I.LOCATION_CODE = L.LOCATION_NAME), 0) QTY,
                //               K.CAPACITY

                //          FROM XXES_SUPERMKT_LOCATIONS L JOIN XXES_KANBAN_MASTER K
                //          ON L.PLANT_CODE = K.PLANT_CODE
                //          AND L.FAMILY_CODE = K.FAMILY_CODE
                //          AND L.LOCATION_NAME = K.SUMKTLOC
                //          WHERE L.PLANT_CODE = '{0}'
                //            AND L.FAMILY_CODE = '{1}'
                //            AND ('%{2}%' IS NULL OR L.LOCATION_NAME LIKE '%{2}%')
                //          ORDER BY L.LOCATION_NAME", layout.Plant.ToUpper(), layout.Family.ToUpper(), layout.Search);

                query = String.Format(@"SELECT LOCATION_NAME,
                                 NVL((SELECT SUM(I.QUANTITY)
                             FROM XXES_SUMMKTSTOCK I
                             WHERE I.PLANT_CODE = L.PLANT_CODE
                               AND I.FAMILY_CODE = L.FAMILY_CODE
                               AND I.LOCATION_CODE = L.LOCATION_NAME), 0) QTY,
                           NVL(CAPACITY,0) CAPACITY
                          FROM XXES_SUPERMKT_LOCATIONS L  WHERE PLANT_CODE='{0}' AND FAMILY_CODE = '{1}'
                            AND ('%{2}%' IS NULL OR L.LOCATION_NAME LIKE '%{2}%') ORDER BY LOCATION_NAME", 
                            layout.Plant.ToUpper(), layout.Family.ToUpper(), layout.Search);

                //query = string.Format(@"SELECT L.LOCATION_NAME,nvl((SELECT SUM(I.QUANTITY) FROM XXES_SUMKTSTORAGEITEMS I WHERE 
                //              I.PLANT_CODE=L.PLANT_CODE AND I.FAMILY_CODE=L.FAMILY_CODE AND I.SUMKTLOC=L.LOCATION_NAME) ,0) Qty
                //              FROM  XXES_SUPERMKT_LOCATIONS L 
                //              WHERE L.PLANT_CODE= '{0}' AND L.FAMILY_CODE= '{1}' AND ('%{2}%' IS NULL OR L.LOCATION_NAME LIKE '%{2}%') order by L.LOCATION_NAME", layout.Plant.ToUpper(), layout.Family.ToUpper(), layout.Search);
                da = new OracleDataAdapter(query, fun.Connection());
                DataTable dt = new DataTable();
                da.Fill(dt);

                //da = new OracleDataAdapter("SELECT * FROM XXES_BULK_STORAGE WHERE PLANT_CODE= '" + layout.Plant.ToUpper().Trim() + "' AND FAMILY_CODE= '" + layout.Family.ToUpper().Trim() + "'", fun.Connection());
                //DataTable CellDT = new DataTable();
                //da.Fill(CellDT);

                if (dt.Rows.Count > 0)
                {

                    List<Store> IL = new List<Store>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        IL.Add(new Store
                        {
                            PlantSuper = layout.Plant,
                            FamilySuper = layout.Family,
                            LocationName = Convert.ToString(dr["LOCATION_NAME"]),
                            CAPACITY = Convert.ToString(dr["CAPACITY"]),
                            QTY = Convert.ToString(dr["QTY"]),
                            //DDLLayoutValue = "SM",
                        });
                    }
                    UL.Add(new LayoutSuperMktSubList { SuperMktLayoutMode = IL });

                    ViewBag.Layout = UL;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
        }
        public PartialViewResult CellDetail(string cell)
        {
            try
            {
                DataTable dt = new DataTable();

                if (!string.IsNullOrEmpty(cell))
                {
                    string PID = string.Empty, FID = string.Empty, LID = string.Empty, LO = string.Empty;

                    String str = cell;
                    String[] spearator = { "/" };
                    String[] strlist = str.Split(spearator, StringSplitOptions.RemoveEmptyEntries);

                    PID = strlist[0];
                    FID = strlist[1];
                    LID = strlist[2];
                    LO = strlist[4];
                    if (LO == "VL")
                    {
                        string checkAlias = "SELECT * FROM XXES_BULK_STORAGE WHERE PLANT_CODE = '" + PID + "' AND  FAMILY_CODE = '" + FID + "' AND  LOCATION_CODE = '" + LID + "' ";
                        da = new OracleDataAdapter(checkAlias, fun.Connection());
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)

                        {
                            string Query = @"SELECT xb.ITEMCODE,xrm.ITEM_DESCRIPTION DESCRIPTION, SUM(xb.QUANTITY) QUANTITY FROM xxes_bulkstock xb, XXES_RAWMATERIAL_MASTER xrm 
                                WHERE XB.PLANT_CODE=xrm.PLANT_CODE AND xb.FAMILY_CODE=xrm.FAMILY_CODE AND xb.ITEMCODE=xrm.ITEM_CODE and xb.LOCATION_CODE='" + LID + "' AND xb.QUANTITY>0 AND XB.PLANT_CODE='" + PID + "' AND XB.FAMILY_CODE='" + FID + "' GROUP BY XB.ITEMCODE ,xrm.ITEM_DESCRIPTION ";
                            da = new OracleDataAdapter(Query, fun.Connection());
                            da.Fill(dt);

                        }
                    }
                    else if (LO == "SM")
                    {
                        string checkAlias = "SELECT * FROM XXES_SUPERMKT_LOCATIONS WHERE PLANT_CODE = '" + PID + "' AND  FAMILY_CODE = '" + FID + "' AND  LOCATION_NAME = '" + LID + "' ";
                        da = new OracleDataAdapter(checkAlias, fun.Connection());
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)

                        {
                            string Query = @"SELECT xb.ITEMCODE,xrm.ITEM_DESCRIPTION DESCRIPTION, SUM(xb.QUANTITY) QUANTITY FROM XXES_SUMMKTSTOCK xb, XXES_RAWMATERIAL_MASTER xrm 
                                WHERE XB.PLANT_CODE=xrm.PLANT_CODE AND xb.FAMILY_CODE=xrm.FAMILY_CODE AND xb.ITEMCODE=xrm.ITEM_CODE and xb.LOCATION_CODE='" + LID + "' AND xb.QUANTITY>0 AND XB.PLANT_CODE='" + PID + "' AND XB.FAMILY_CODE='" + FID + "' GROUP BY XB.ITEMCODE ,xrm.ITEM_DESCRIPTION ";
                            da = new OracleDataAdapter(Query, fun.Connection());
                            da.Fill(dt);

                        }
                    }



                }
                return PartialView(dt);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
        }
        public class dropdwonForBulkStorage
        {
            public string dllLocation { get; set; }
            public string dllItemCode { get; set; }
        }
    }
}