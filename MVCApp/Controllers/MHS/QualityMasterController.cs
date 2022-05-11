using ClosedXML.Excel;
using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using Syncfusion.EJ2.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;



namespace MVCApp.Controllers
{
    [Authorize]
    public class QualityMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt = new DataTable();
        Function fun = new Function();

        string query = ""; string ORGID = "";
        string Msg = string.Empty;
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.DefaultDate = DateTime.Now;
                ViewBag.DefaultDateQuality = DateTime.Now;
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
        public ActionResult Grid(GoodsRecivingatStoreModel obj)
        {
            GoodsRecivingatStoreModel GR = new GoodsRecivingatStoreModel();
            obj.draw = Convert.ToInt32(Request.Form.GetValues("draw").FirstOrDefault());

            obj.STARTROWINDEX = Request.Form.GetValues("start").FirstOrDefault();
            obj.MAXROWS = Request.Form.GetValues("length").FirstOrDefault();


            int recordsTotal = 0;
            //obj.PLANT = Convert.ToString(Session["Login_Unit"]);

            obj.PUNAME = Convert.ToString(Session["PUname"]);
            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            
            if (!string.IsNullOrEmpty(obj.P_Search))
            {
                obj.P_Search = Convert.ToString(obj.P_Search).ToUpper().Trim();
            }
            List<GoodsRecivingatStoreModel> empDetails = fun.GridInspectionPaging(obj);

            if (string.IsNullOrEmpty(obj.PUNAME))
            {
                query = string.Format(@"SELECT count(*) FROM XXES_MRNINFO M INNER JOIN ITEM_RECEIPT_DETIALS I
                                    ON M.MRN_NO = I.MRN_NO WHERE M.STORE_VERIFIED = 'VERIFIED' AND M.STATUS = 'QA' AND M.QUALITY_OK IS NULL  
                                AND M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')>= to_date('{2}','dd-Mon-yyyy') AND 
                        TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')<=to_date('{3}','dd-Mon-yyyy')
                        AND ('{4}' IS NULL OR M.ITEMCODE like '%'||'{4}'||'%' OR M.MRN_NO LIKE '%'||'{4}'||'%' OR I.VENDOR_NAME LIKE '%'||'{4}'||'%' OR I.VENDOR_CODE LIKE '%'||'{4}'||'%')", 
                        obj.PLANT.ToUpper().Trim(), obj.FAMILY.ToUpper().Trim(), obj.FROMDATE_INSP.Trim(), obj.TODATE_INSP.Trim(), obj.P_Search);
            }
            else
            {
                query = string.Format(@"SELECT count(*) FROM XXES_MRNINFO M INNER JOIN ITEM_RECEIPT_DETIALS I
                                    ON M.MRN_NO = I.MRN_NO WHERE M.STORE_VERIFIED = 'VERIFIED' AND M.STATUS = 'QA' AND M.QUALITY_OK IS NULL  AND M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND M.PUNAME = '{2}' AND TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')>= to_date('{3}','dd-Mon-yyyy') AND 
                        TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')<=to_date('{4}','dd-Mon-yyyy')
                        AND ('{4}' IS NULL OR M.ITEMCODE like '%'||'{5}'||'%' OR M.MRN_NO LIKE '%'||'{5}'||'%' OR I.VENDOR_NAME LIKE '%'||'{5}'||'%' OR I.VENDOR_CODE LIKE '%'||'{5}'||'%')", 
                        obj.PLANT.ToUpper().Trim(), obj.FAMILY.ToUpper().Trim(), obj.PUNAME.ToUpper().Trim(), obj.FROMDATE_INSP.Trim(), obj.TODATE_INSP.Trim(), obj.P_Search);
            }

            string Total = fun.get_Col_Value(query);
            if (!string.IsNullOrEmpty(Total))
            {
                recordsTotal = Convert.ToInt32(Total);
            }


            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = empDetails }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Edit(List<GoodsRecivingatStoreModel> obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty, IPADDR = string.Empty,
              IPPORT = string.Empty;
            List<BOXBARCODE> lstbarcodes = new List<BOXBARCODE>();
            BOXBARCODE bOXBARCODE = null;
           string QCPrint =  Convert.ToString(ConfigurationManager.AppSettings["QC_PRINT"]);
            try
            {
                if (obj != null)
                {
                    if (obj.Count > 0 && QCPrint.ToUpper() == "Y")
                    {
                        string line = fun.getPrinterIp("QUALITY", Convert.ToString(Session["Login_Unit"]).ToUpper().Trim(), Convert.ToString(Session["LoginFamily"]).ToUpper().Trim());
                        if (string.IsNullOrEmpty(line))
                        {
                            msg = "STAGE PRINTER IP ADDRESS AND PORT NOT FOUND";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);

                        }
                        IPADDR = line.Split('#')[0].Trim();
                        IPPORT = line.Split('#')[1].Trim();
                    }
                }
                if (obj != null)
                {
                    foreach (var item in obj)
                    {
                        bOXBARCODE = new BOXBARCODE();

                        if (!string.IsNullOrEmpty(item.QUALITY_OK) && item.QUALITY_OK != "0")
                        {
                            if ("ACCEPTED" == item.QUALITY_OK)
                            {
                                bOXBARCODE.MRN = item.MRN_NO;
                                bOXBARCODE.ITEMCODE = item.ITEMCODE;
                                bOXBARCODE.PLANT = item.PLANT;
                                bOXBARCODE.FAMILY = item.FAMILY;
                                bOXBARCODE.RECQTY = item.QTY_RECEIVED;
                                bOXBARCODE.PRINTERIP = IPADDR;
                                bOXBARCODE.PRINTERPORT = IPPORT;
                                item.QUALITY_OK = "OK";
                                query = string.Format(@"UPDATE XXES_MRNINFO SET QUALITY_OK = '{0}', QUALITY_OK_BY = '{1}', QUALITY_OK_DATE = SYSDATE WHERE AUTOID = '{2}'", item.QUALITY_OK, Convert.ToString(System.Web.HttpContext.Current.User.Identity.Name).ToUpper(), item.AUTOID);
                                bool RESULT = fun.EXEC_QUERY(query);
                                if (RESULT)
                                {
                                    msg = "Record Saved Successfully";
                                    mstType = Validation.str;
                                    status = Validation.stus;
                                }

                                lstbarcodes.Add(bOXBARCODE);
                            }

                        }
                    }
                    if (QCPrint.ToUpper() == "Y" &&  fun.PrintingEnable("QUALITY", Convert.ToString(Session["Login_Unit"]).ToUpper().Trim(), Convert.ToString(Session["LoginFamily"]).ToUpper().Trim()))
                    {
                        PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                        if (barcodes.PrintMRNQualityBarcodes(lstbarcodes))
                        {
                            msg = "Record Saved & Printing Successfully";
                            mstType = Validation.str;
                            status = Validation.stus;
                        }
                        else
                        {
                            msg = "Error in printing";
                            mstType = Validation.str1;
                            status = Validation.str2;
                        }
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
        public ActionResult Grid_Report(GoodsRecivingatStoreModel OBJ)
        {
            try
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    query = @"SELECT M.AUTOID, I.VENDOR_CODE,SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char( M.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as STORE_VERIFIEDDATE, M.QUALITY_OK, M.QUALITY_OK_BY, to_char( M.QUALITY_OK_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) as QUALITY_OK_DATE 
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND to_char(M.QUALITY_OK_DATE,'dd-Mon-yyyy') >=to_date('" + Convert.ToDateTime(OBJ.FROMDATE).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(M.QUALITY_OK_DATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(OBJ.TODATE).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy')";

                }
                else
                {
                    query = @"SELECT M.AUTOID, I.VENDOR_CODE,SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char( M.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as STORE_VERIFIEDDATE, M.QUALITY_OK, M.QUALITY_OK_BY, to_char( M.QUALITY_OK_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) as QUALITY_OK_DATE
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND M.PLANT_CODE = '" + Convert.ToString(Session["Login_Unit"]) + "' AND M.PUNAME = '" + Convert.ToString(Session["PUname"]) + "' AND to_char(M.QUALITY_OK_DATE,'dd-Mon-yyyy') >=to_date('" + Convert.ToDateTime(OBJ.FROMDATE).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(M.QUALITY_OK_DATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(OBJ.TODATE).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy')";

                }

                dt = fun.returnDataTable(query);
                ViewBag.DataSourceH = dt;
            }
            catch (Exception ex)
            {
                //Msg = "Error ! " + ex.Message;
            }
            return PartialView();
        }

        public ActionResult Grid_NewMRN()
        {
            try
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND M.STORE_VERIFIED IS NULL AND M.QUALITY_OK_BY IS NULL order BY m.createddate desc");

                }
                else
                {
                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND M.STORE_VERIFIED IS NULL AND M.QUALITY_OK_BY IS NULL AND M.PLANT_CODE = '{0}' AND M.PUNAME = '{1}' order BY m.createddate desc", Convert.ToString(Session["Login_Unit"]), Convert.ToString(Session["PUname"]));
                }

                dt = fun.returnDataTable(query);
                ViewBag.DataSourceH = dt;
            }
            catch (Exception ex)
            {
                //Msg = "Error ! " + ex.Message;
            }
            return PartialView();
        }

        [HttpPost]
        public ActionResult CheckBoxes(List<GoodsRecivingatStoreModel> obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;

            List<GoodsRecivingatStoreModel> listUpdateQty = new List<GoodsRecivingatStoreModel>();

            try
            {
                if (obj.Count > 0)
                {
                    foreach (var item in obj)
                    {
                        query = string.Format(@"SELECT AUTOID, MRN, ITEMCODE, QUANTITY, BOXNO 
                                        FROM XXES_VERIFYSTOREMRN WHERE MRN= '{0}' AND ITEMCODE = '{1}' ORDER BY BOXNO", item.MRN_NO, item.ITEMCODE);


                        dt = fun.returnDataTable(query);
                        #region get boxes from XXES_VERIFYSTOREMRN table
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                GoodsRecivingatStoreModel grs = new GoodsRecivingatStoreModel();
                                {
                                    grs.QUANTITY = dt.Rows[i]["QUANTITY"].ToString();
                                    grs.boxNo = dt.Rows[i]["BOXNO"].ToString();
                                    grs.ITEMCODE = dt.Rows[i]["ITEMCODE"].ToString();
                                    grs.MRN_NO = dt.Rows[i]["MRN"].ToString();
                                };

                                listUpdateQty.Add(grs);
                            }
                            msg = "DCU";
                            mstType = Validation.str;
                            status = Validation.stus;
                        }
                        #endregion

                        #region get data from XXES_MRNINFO table
                        else
                        {
                            query = string.Format(@"SELECT MRN_NO, ITEMCODE, REC_QTY FROM XXES_MRNINFO WHERE MRN_NO= '{0}' AND ITEMCODE = '{1}'", item.MRN_NO, item.ITEMCODE);


                            dt = fun.returnDataTable(query);
                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    GoodsRecivingatStoreModel grs = new GoodsRecivingatStoreModel();
                                    {
                                        grs.QTY_RECEIVED = dt.Rows[i]["REC_QTY"].ToString();
                                        grs.ITEMCODE = dt.Rows[i]["ITEMCODE"].ToString();
                                        grs.MRN_NO = dt.Rows[i]["MRN_NO"].ToString();
                                    };

                                    listUpdateQty.Add(grs);
                                }
                                msg = "WEB";
                                mstType = Validation.str;
                                status = Validation.stus;
                            }
                        }
                        #endregion

                    }
                }

                var reslt = new { data = listUpdateQty, Msg = msg, ID = mstType, validation = status };
                return Json(reslt, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var reslt = new { Msg = msg, ID = mstType, validation = status };
                return Json(reslt, JsonRequestBehavior.AllowGet);
            }

            var result = new { data = listUpdateQty, Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateRejectQty(List<GoodsRecivingatStoreModel> obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;

            int j;
            string MODE = string.Empty; string MRN = string.Empty; string ITEMCODE = string.Empty; int p = 0;
            string PLANT = string.Empty; string FAMILY = string.Empty; string REMARKS = string.Empty; int q = 0;
            GoodsRecivingatStoreModel gr = new GoodsRecivingatStoreModel();


            List<string> lstBOXNO = new List<string>();
            List<string> lstLASTQTY = new List<string>();
            List<string> lstREJQTY = new List<string>();
            List<string> lstPREBARCODE = new List<string>();
            List<string> lstPOSTBARCODE = new List<string>();

            string LoginUser = Convert.ToString(System.Web.HttpContext.Current.User.Identity.Name).ToUpper();

            try
            {

                if (obj.Count > 0)
                {
                    foreach (var item in obj)
                    {

                        if (string.IsNullOrEmpty(item.Remarks))
                        {
                            msg = "Remarks is Required..";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);

                        }
                        if (string.IsNullOrEmpty(item.REJECT_QTY))
                        {
                            msg = "Enter Zero or Rejected Qty ....";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (!int.TryParse(item.REJECT_QTY, out j))
                            {

                                msg = "Rejected Qty is not a valid number ..";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var resul = new { Msg = msg, ID = mstType, validation = status };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                            if (j == 0)
                            {
                                continue;
                            }
                            else if (j > Convert.ToInt32(item.QUANTITY) || j < 0)
                            {
                                msg = "Rejected Qty should between Zero and Last Qty ..";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var resul = new { Msg = msg, ID = mstType, validation = status };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }
                        if (item.Media_Type == "DCU")
                        {
                            while (p < 1)
                            {
                                MODE = item.Media_Type;
                                REMARKS = item.Remarks;
                                MRN = item.MRN_NO;
                                ITEMCODE = item.ITEMCODE;
                                p++;
                            }

                            item.boxNo = item.boxNo.Substring(3);
                            query = string.Format(@"SELECT PLANT_CODE || '#' || FAMILY_CODE || '#' || BARCODE BARCODE FROM XXES_VERIFYSTOREMRN WHERE MRN= '{0}' AND ITEMCODE = '{1}' 
                                                    AND BOXNO = '{2}'", MRN, ITEMCODE, item.boxNo);
                            string BARCODE = fun.get_Col_Value(query);
                            //char[] ch = { '#' };
                            string[] ARR = BARCODE.Split('#');
                            if (ARR[0] == "" || ARR[1] == "" || ARR[2] == "")
                            {
                                msg = "Either Plant or Family or Barcode is not found ..";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var resul = new { Msg = msg, ID = mstType, validation = status };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                            while (q < 1)
                            {
                                PLANT = ARR[0]; FAMILY = ARR[1];
                                q++;
                            }

                            SplitItemBarcode splitItem = new SplitItemBarcode();
                            if (!string.IsNullOrEmpty(ARR[2]))
                            {
                                MAHController mAHController = new MAHController();
                                splitItem = mAHController.SplitItemQrcode(ARR[2]);
                            }


                            //to create list of string

                            lstBOXNO.Add(item.boxNo);
                            lstLASTQTY.Add(item.QUANTITY);
                            lstREJQTY.Add(item.REJECT_QTY);
                            lstPREBARCODE.Add(ARR[2]);


                            if (item.REJECT_QTY != "0")
                            {
                                int UPDATEDQTY = Convert.ToInt32(splitItem.PKGQTY) - Convert.ToInt32(item.REJECT_QTY);
                                gr.UpdatedBarcode = splitItem.PLANT + "$" + splitItem.PO + "$" + splitItem.ITEMCODE + "$" + UPDATEDQTY + "$" + splitItem.BULKLOC + "$" +
                                splitItem.POLINE + "$" + splitItem.SUPPLIER + "$" + splitItem.IF + "$" + splitItem.DATE + "$" + splitItem.BOX + "$" + item.MRN_NO.Trim();
                                lstPOSTBARCODE.Add(gr.UpdatedBarcode);
                            }
                            else
                            {
                                lstPOSTBARCODE.Add(ARR[2]);

                            }



                        }
                        else if (item.Media_Type == "WEB")
                        {
                            query = string.Format(@"SELECT PLANT_CODE|| '#' ||FAMILY_CODE PLANTFAMILY FROM XXES_MRNINFO WHERE MRN_NO= '{0}' AND ITEMCODE = '{1}'", item.MRN_NO, item.ITEMCODE);
                            string PLANTFAMILY = fun.get_Col_Value(query);
                            char[] ch = { '#' };
                            string[] ARR = PLANTFAMILY.Split(ch, StringSplitOptions.RemoveEmptyEntries);
                            if (ARR[0] == "" || ARR[1] == "")
                            {
                                msg = "Either Plant or Family is not found ..";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var resul = new { Msg = msg, ID = mstType, validation = status };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                            PLANT = ARR[0];
                            FAMILY = ARR[1];
                            MODE = item.Media_Type;
                            gr.ITEMCODE = item.ITEMCODE;
                            gr.MRN_NO = item.MRN_NO;
                            gr.Media_Type = item.Media_Type;
                            gr.QUANTITY = item.QUANTITY;
                            gr.REJECT_QTY = item.REJECT_QTY;
                            gr.Remarks = item.Remarks;
                        }

                    }

                    if (MODE == "DCU")
                    {
                        string Result = UpdatePartialQty(PLANT, FAMILY, LoginUser, MODE, REMARKS, MRN, ITEMCODE, lstBOXNO, lstLASTQTY, lstREJQTY, lstPREBARCODE, lstPOSTBARCODE);
                        if (Result.Contains("OK"))
                        {
                            msg = "Quantity updated successfully !";
                            mstType = Validation.str;
                            status = Validation.stus;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Exception e = new Exception(Result);
                            fun.LogWrite(e);
                            msg = "Error in updating quantity !";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);

                        }
                    }

                    else if (MODE == "WEB")
                    {
                        string OUTPUT = UPDATEMRNQTY(PLANT, FAMILY, LoginUser, MODE, gr.Remarks, gr.MRN_NO, gr.ITEMCODE,
                                              gr.QUANTITY, gr.REJECT_QTY);

                        if (OUTPUT.Contains("OK"))
                        {
                            msg = "Quantity updated successfully !";
                            mstType = Validation.str;
                            status = Validation.stus;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Exception e = new Exception(OUTPUT);
                            fun.LogWrite(e);
                            msg = "Error in updating quantity !";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        msg = "Zero Qty cannot be updated !";
                        mstType = Validation.str;
                        status = Validation.stus;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
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

        public string UpdatePartialQty(string PLANT, string FAMILY, string CREATED_BY, string CALLTYPE, string REMARKS, string MRN, string ITEMCODE,
                                            List<string> lstBOXNO, List<string> lstLASTQTY, List<string> lstREJQTY, List<string> lstPREBARCODE, List<string> lstPOSTBARCODE)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty, icon = string.Empty;
            string response = string.Empty;
            try
            {
                using (OracleCommand command = new OracleCommand("QC_QTYVERIFY.USP_QC_VERIFICATION", fun.Connection()))
                {
                    fun.ConOpen();
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("PLANT", PLANT.Trim());
                    command.Parameters.Add("FAMILY", FAMILY.Trim());
                    command.Parameters.Add("CREATED_BY", CREATED_BY.Trim());
                    command.Parameters.Add("REMARKS", REMARKS.Trim());
                    command.Parameters.Add("MRN", MRN.Trim());
                    command.Parameters.Add("ITEMCODE", ITEMCODE.Trim());


                    OracleParameter ARRBOXNO = new OracleParameter();
                    ARRBOXNO.OracleDbType = OracleDbType.Varchar2;
                    ARRBOXNO.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                    ARRBOXNO.Value = lstBOXNO.ToArray();
                    ARRBOXNO.Size = lstBOXNO.Count;
                    command.Parameters.Add(ARRBOXNO);

                    OracleParameter ARRLASTQTY = new OracleParameter();
                    ARRLASTQTY.OracleDbType = OracleDbType.Varchar2;
                    ARRLASTQTY.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                    ARRLASTQTY.Value = lstLASTQTY.ToArray();
                    ARRLASTQTY.Size = lstLASTQTY.Count;
                    command.Parameters.Add(ARRLASTQTY);

                    OracleParameter ARRREJQTY = new OracleParameter();
                    ARRREJQTY.OracleDbType = OracleDbType.Varchar2;
                    ARRREJQTY.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                    ARRREJQTY.Value = lstREJQTY.ToArray();
                    ARRREJQTY.Size = lstREJQTY.Count;
                    command.Parameters.Add(ARRREJQTY);

                    OracleParameter ARRPREBARCODE = new OracleParameter();
                    ARRPREBARCODE.OracleDbType = OracleDbType.Varchar2;
                    ARRPREBARCODE.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                    ARRPREBARCODE.Value = lstPREBARCODE.ToArray();
                    ARRPREBARCODE.Size = lstPREBARCODE.Count;
                    command.Parameters.Add(ARRPREBARCODE);

                    OracleParameter ARRPOSTBARCODE = new OracleParameter();
                    ARRPOSTBARCODE.OracleDbType = OracleDbType.Varchar2;
                    ARRPOSTBARCODE.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                    ARRPOSTBARCODE.Value = lstPOSTBARCODE.ToArray();
                    ARRPOSTBARCODE.Size = lstPOSTBARCODE.Count;
                    command.Parameters.Add(ARRPOSTBARCODE);


                    command.Parameters.Add("return_message", OracleDbType.NVarchar2, 8000);
                    command.Parameters["return_message"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    response = Convert.ToString(command.Parameters["return_message"].Value);
                    //fun.ConClose();

                }


            }
            catch (Exception ex)
            {
                response = ex.Message;
                fun.LogWrite(ex);
            }
            finally
            {

                fun.ConClose();
            }
            return response;
        }

        public string UPDATEMRNQTY(string PLANT, string FAMILY, string CREATED_BY, string CALLTYPE, string REMARKS, string MRN, string ITEMCODE,
                                             string LASTQTY, string REJQTY)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty, icon = string.Empty;
            string response = string.Empty;
            try
            {
                using (OracleCommand command = new OracleCommand("QC_QTYVERIFY.USP_UPDATEMRNQTY", fun.Connection()))
                {
                    fun.ConOpen();
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("PLANT", PLANT.Trim());
                    command.Parameters.Add("FAMILY", FAMILY.Trim());
                    command.Parameters.Add("CREATED_BY", CREATED_BY.Trim());
                    //command.Parameters.Add("CALLTYPE", CALLTYPE.Trim());
                    command.Parameters.Add("REMARKS", REMARKS.Trim());
                    command.Parameters.Add("MRN", MRN.Trim());
                    command.Parameters.Add("ITEMCODE", ITEMCODE.Trim());
                    command.Parameters.Add("LASTQTY", LASTQTY.Trim());
                    command.Parameters.Add("REJQTY", REJQTY.Trim());


                    command.Parameters.Add("return_message", OracleDbType.NVarchar2, 8000);
                    command.Parameters["return_message"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    response = Convert.ToString(command.Parameters["return_message"].Value);
                    //fun.ConClose();

                }


            }
            catch (Exception ex)
            {
                response = ex.Message;
                fun.LogWrite(ex);
            }
            finally
            {

                fun.ConClose();
            }
            return response;
        }

        public ActionResult ExportQuality(GoodsRecivingatStoreModel OBJ)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string errorNo = string.Empty;
            DataTable dt = new DataTable();
            //GoodsRecivingatStoreModel obj = new GoodsRecivingatStoreModel();
            try
            {

                OBJ.PUNAME = Convert.ToString(Session["PUname"]);


                dt = fun.GetQualityEXCELData(OBJ);
                if (dt.Rows.Count > 0)
                {
                    dt.Namespace = "QCExport";
                    dt.TableName = "QCExport";
                    string filename = "QCExport" + DateTime.Now.ToString("ddMMyyyy");
                    //data.ImportExcel = filename;
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add(dt);
                    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                    ws.Range("A1:S1").Style.Font.Bold = true;
                    //ws.Range("C1:D1").Style.Font.Bold = true;
                    ws.Columns().AdjustToContents();

                    //string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                    //if (System.IO.File.Exists(FilePath))
                    //{
                    //    System.IO.File.Delete(FilePath);
                    //}

                    string DirectoryPath = Server.MapPath("~/TempExcelFile/");

                    if (!Directory.Exists(DirectoryPath))
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }

                    string FileName = filename + ".xlsx";
                    string FilePath = DirectoryPath + FileName;

                    wb.SaveAs(FilePath);
                    msg = "File Exported Successfully ...";
                    mstType = Validation.str;
                    //excelName = data.ImportExcel;
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

        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        [HttpPost]
        public ActionResult UpdateHoldQty(List<GoodsRecivingatStoreModel> obj)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;

            try
            {

                if (obj.Count > 0)
                {
                    foreach (var item in obj)
                    {
                        if (string.IsNullOrEmpty(item.Remarks))
                        {
                            msg = "Remarks is Required..";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }

                        string OUTPUT = UPDATEHOLDITEM(Convert.ToString(item.AUTOID), item.Remarks);

                        if (OUTPUT.Contains("OK"))
                        {
                            msg = "Hold Item updated successfully !";
                            mstType = Validation.str;
                            status = Validation.stus;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Exception e = new Exception(OUTPUT);
                            fun.LogWrite(e);
                            msg = "Error in updating Hold Item !";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }

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
        public string UPDATEHOLDITEM(string AUTOID, string Remarks)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty, icon = string.Empty;
            string response = string.Empty;
            try
            {
                using (OracleCommand command = new OracleCommand("USP_QUALITYHOLD", fun.Connection()))
                {
                    fun.ConOpen();
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("P_AUTOID", AUTOID.Trim());
                    command.Parameters.Add("P_CREATED_BY", Convert.ToString(Session["Login_User"]).Trim().ToUpper());
                    command.Parameters.Add("P_REMARKS", Remarks.Trim().ToUpper());
                    command.Parameters.Add("return_message", OracleDbType.Varchar2, 8000);
                    command.Parameters["return_message"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    response = Convert.ToString(command.Parameters["return_message"].Value);
                    //fun.ConClose();
                }


            }
            catch (Exception ex)
            {
                response = ex.Message;
                fun.LogWrite(ex);
            }
            finally
            {

                fun.ConClose();
            }
            return response;
        }

        public ActionResult CheckQCFromDays(GoodsRecivingatStoreModel Data)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string errorNo = string.Empty;
              
            try
            {
                query = string.Format(@"select paramvalue from xxes_sft_settings where plant_code = '{0}' and family_code = '{1}' 
                                    and parameterinfo = 'QC_FROMDAYS'", Data.PLANT.Trim(), Data.FAMILY.Trim());
                string days = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(days))
                {
                    DateTime currentdate = fun.GetServerDateTime();
                    currentdate = currentdate.AddDays(-Convert.ToInt32(days));
                    DateTime frmdate = Convert.ToDateTime(Data.FROMDATE_INSP);
                    if (frmdate.Date < currentdate.Date)
                    {
                        msg = "From date cannot be earlier than " + currentdate.ToString("dd-MM-yyyy");
                        mstType = "alert-danger";
                        errorNo = "1";
                        var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                         
                    }

                }

                else
                {
                    msg = "OK";
                    mstType = "alert-success";
                    errorNo = "0";
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
    }
}