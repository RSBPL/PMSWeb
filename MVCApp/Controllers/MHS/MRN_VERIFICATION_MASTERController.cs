using CrystalDecisions.CrystalReports.Engine;
using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using QRCoder;
using Syncfusion.EJ2.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;



namespace MVCApp.Controllers
{
    [Authorize]
    public class MRN_VERIFICATION_MASTERController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt = new DataTable();
        Function fun = new Function();
        string ITEM_DESC = Convert.ToString(ConfigurationManager.AppSettings["ITEM_DESC"]);
        string VENDOR_DESC = Convert.ToString(ConfigurationManager.AppSettings["VENDOR_DESC"]);
        string query = ""; string ORGID = "";
        string Msg = string.Empty;
        Assemblyfunctions AsmblyFun = new Assemblyfunctions();
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.DefaultDate = DateTime.Now;
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
                result = fun.Fill_FamilyMRNVerfication(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Grid(GoodsRecivingatStoreModel obj)
        {
            //220002159792,INV00202/21-22,DL1LV7651            
            string MRN = string.Empty, Invoice = string.Empty, VehicleNo = string.Empty;

            List<GoodsRecivingatStoreModel> empDetails = new List<GoodsRecivingatStoreModel>();

            try
            {
                if (string.IsNullOrEmpty(obj.MRN_QR))
                {
                    Msg = "Error ! Invalid Scan..";
                }
                else
                {
                    if (obj.MRN_QR.Contains(","))
                    {
                        int count = obj.MRN_QR.Count(f => f == ',');
                        if (count != 2)
                        {
                            Msg = "Error ! Invalid QR Code..";
                            var result = Msg;
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        MRN = obj.MRN_QR.Split(',')[0].Trim();
                        Invoice = obj.MRN_QR.Split(',')[1].Trim();
                        VehicleNo = obj.MRN_QR.Split(',')[2].Trim();
                    }
                    else
                    {
                        MRN = obj.MRN_QR.Trim();
                    }

                    UpdateMrnDetails(obj.PLANT, MRN, obj.FAMILY);

                    //if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                    //{
                    //    query = string.Format(@"SELECT M.PLANT_CODE, M.AUTOID, M.MRN_NO, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,'{1}') AS ITEM_DESCRIPTION, I.VENDOR_CODE, SUBSTR(I.VENDOR_NAME,0,'{2}') AS VENDOR_NAME, M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE,
                    //                        to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS,  M.STORE_VERIFIED, M.REC_QTY
                    //                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                    //                        ON M.MRN_NO = I.MRN_NO WHERE M.MRN_NO = '{0}' AND M.PLANT_CODE = '{3}' AND I.TIMEIN IS NOT NULL AND I.IN_BY IS NOT NULL", MRN, ITEM_DESC, VENDOR_DESC, obj.PLANT.Trim());
                    //}
                    //else
                    //{
                    query = string.Format(@"SELECT M.PLANT_CODE, M.AUTOID, M.MRN_NO, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,'{2}') AS ITEM_DESCRIPTION, I.VENDOR_CODE,CASE WHEN I.VENDOR_NAME IS NULL THEN 'ESCORTS LTD' ELSE  SUBSTR(I.VENDOR_NAME,0,'{3}') END AS VENDOR_NAME, M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE,
                                                to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS,  M.STORE_VERIFIED,NVL(M.REC_QTY,0) REC_QTY,I.TRANSACTION_DATE
                                                FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                                ON M.MRN_NO = I.MRN_NO WHERE M.MRN_NO = '{0}' AND M.PLANT_CODE = '{1}' AND I.TIMEIN IS NOT NULL AND I.IN_BY IS NOT NULL", MRN, obj.PLANT.Trim(), ITEM_DESC, VENDOR_DESC);
                    //}


                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            GoodsRecivingatStoreModel GR = new GoodsRecivingatStoreModel
                            {

                                AUTOID = Convert.ToInt32(dr["AUTOID"]),
                                MRN_NO = dr["MRN_NO"].ToString(),
                                ITEMCODE = dr["ITEMCODE"].ToString(),
                                ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                                VENDOR_CODE = dr["VENDOR_CODE"].ToString(),
                                VENDOR_NAME = dr["VENDOR_NAME"].ToString(),
                                QUANTITY = dr["QUANTITY"].ToString(),
                                CREATEDDATE = dr["CREATEDDATE"].ToString(),
                                CREATEDTIME = dr["CREATEDTIME"].ToString(),
                                STATUS = dr["STATUS"].ToString(),
                                STORE_VERIFIED = dr["STORE_VERIFIED"].ToString(),
                                PLANT = dr["PLANT_CODE"].ToString(),
                                QTY_RECEIVED = dr["REC_QTY"].ToString(),
                                TRANSACTION_DATE = dr["TRANSACTION_DATE"].ToString()
                            };
                            empDetails.Add(GR);
                        }

                        string qtyLabel = string.Format(@"SELECT PARAMETERINFO, PARAMVALUE FROM XXES_SFT_SETTINGS 
                                                        WHERE PLANT_CODE ='{0}' AND FAMILY_CODE = '{1}' AND PARAMETERINFO = 'PRINTQTY_LABEL'", obj.PLANT.Trim(), obj.FAMILY.Trim());
                        if (dt != null)
                        {
                            dt = null;
                        }
                        dt = fun.returnDataTable(qtyLabel);
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "A4")

                                {
                                    ViewBag.A4 = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                                }
                                else if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "BARCODE")
                                {
                                    ViewBag.BARCODE = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                                }

                                else if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "QUALITY")
                                {
                                    ViewBag.QUALITY = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                                }
                            }

                        }
                        ViewBag.DataSource = empDetails;
                        if (obj.MRNPrint && obj.PLANT == "T02")
                        {
                            MAHController mAH = new MAHController();
                            COMMONDATA dta = new COMMONDATA();
                            dta.PLANT = obj.PLANT.Trim();
                            dta.FAMILY = obj.FAMILY.Trim();
                            dta.DATA = obj.MRN_QR.Trim();
                            mAH.printMrn(dta);
                        }


                    }
                    else
                    {
                        Msg = "Error MRN is not Scanned at Gate...! ";
                    }

                }
            }
            catch (Exception ex)
            {
                Msg = "Error ! " + ex.Message;
            }
            if (string.IsNullOrEmpty(Msg))
            {
                return PartialView();
            }
            else
            {
                var result = Msg;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private void UpdateMrnDetails(string PLANT_CODE, string MRN_NO, string FAMILY)
        {
            try
            {
                string puname = string.Empty, bomrevison = string.Empty, storage = string.Empty;
                query = string.Format(@"select ORGANIZATION_CODE,MRN_NO,INVOICE_NO,ITEM_CODE,ITEM_DESCRIPTION,QUANTITY,UOM,RATE,STATUS,
                INVOICE_DATE INVOICE_DATE,ITEM_REVISION from apps.XXESGATETAGPRINT_BARCODE WHERE  
                ORGANIZATION_CODE='{0}' AND MRN_NO='{1}' and item_code not in 
                 (select itemcode from xxes_mrninfo where plant_code='{0}' and mrn_no='{1}')", PLANT_CODE, MRN_NO);
                using (DataTable dt = fun.returnDataTable(query))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        bomrevison = puname = string.Empty;
                        puname = GetPuname(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        storage = GetStoreLoc(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        bomrevison = GetBomReviision(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        query = string.Format(@"select count(*) from xxes_mrninfo where plant_code='{0}' and FAMILY_CODE='{1}'
                        and MRN_NO='{2}' and ITEMCODE='{3}'", PLANT_CODE,
                        FAMILY, Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["ITEM_CODE"]));
                        if (fun.CheckExits(query))
                        {

                            query = string.Format(@"update xxes_mrninfo set QUANTITY=to_number(QUANTITY)+{4} where plant_code='{0}' and FAMILY_CODE='{1}'
                            and MRN_NO='{2}' and ITEMCODE='{3}' ",
                            PLANT_CODE,
                            FAMILY, Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["ITEM_CODE"]),
                            Convert.ToDouble(dr["QUANTITY"]));
                        }
                        else
                        {
                            query = string.Format(@"insert into xxes_mrninfo(PLANT_CODE,MRN_NO,INVOICE_NO,ITEMCODE,ITEM_DESCRIPTION,CREATEDDATE,CREATEDBY,
                        QUANTITY,UOM,RATE,INVOICE_DATE,STATUS,PUNAME,ITEM_REVISION,BOM_REVISION,FAMILY_CODE,STORAGE) 
                        values('{0}','{1}','{2}','{3}','{4}',sysdate,'{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')",
                        Convert.ToString(dr["ORGANIZATION_CODE"]), Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["INVOICE_NO"]),
                        Convert.ToString(dr["ITEM_CODE"]), fun.replaceApostophi(Convert.ToString(dr["ITEM_DESCRIPTION"])),
                        PLANT_CODE, Convert.ToString(dr["QUANTITY"]), Convert.ToString(dr["UOM"]), Convert.ToString(dr["RATE"])
                        , Convert.ToString(dr["INVOICE_DATE"]), Convert.ToString(dr["STATUS"]), puname,
                        Convert.ToString(dr["ITEM_REVISION"]), bomrevison, FAMILY, fun.replaceApostophi(storage)
                        );
                        }
                        fun.EXEC_QUERY(query);
                    }
                }


            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GetStoreLoc(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = @"select character6||'-'||character4 from apps.qa_results_v  where plan_id =224155 and Character2 ='" + itemcode + "' and character1 ='" + plant + "' and rownum=1 order by Character2 ";
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        private string GetPuname(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = @"select character6 from apps.qa_results_v  where character6 is not null and plan_id =224155 and Character2 ='" + itemcode + "' and character1 ='" + plant + "' and rownum=1 order by Character2 ";
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        private string GetBomReviision(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = string.Format(@"SELECT MAX(REV) FROM apps.XXES_BOM_REVISION_V re WHERE RE.ORGANIZATION_CODE='{0}' AND RE.SEGMENT1='{1}'",
                    plant.Trim(), itemcode.Trim());
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        public ActionResult Edit(List<GoodsRecivingatStoreModel> Data)
        {
            try
            {
                if (Data != null)
                {
                    bool status = false;
                    string mrn = string.Empty;
                    string plant = string.Empty;

                    foreach (var item in Data)
                    {
                        if (!string.IsNullOrEmpty(item.VERIFICATION))
                        {
                            Double QUANTITY = Convert.ToDouble(item.QUANTITY);
                            if (string.IsNullOrEmpty(item.QTY_RECEIVED))
                            {
                                var resul = "QTY Received should not be blank..";
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }

                            Double j;
                            if (!Double.TryParse(item.QTY_RECEIVED, out j))
                            {
                                var resul = "QTY Received should be number only..";
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                            if ((j > QUANTITY || j < 1))
                            {
                                var resul = "QTY Received should be more than zero but less than or equal to QTY Ordered..";
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    foreach (var item in Data)
                    {
                        mrn = item.MRN_NO;
                        plant = item.PLANT;
                        //if (!string.IsNullOrEmpty(item.VERIFICATION))
                        //{
                            
                            if (!string.IsNullOrEmpty(item.VERIFICATION))
                            {
                                query = string.Format(@"UPDATE XXES_MRNINFO SET REC_QTY = '{3}', STORE_VERIFIED = '{0}', STORE_VERIFIEDBY = '{1}', STORE_VERIFIEDDATE = SYSDATE, REC_MODE = 'WEB' WHERE AUTOID = '{2}'", item.VERIFICATION, Convert.ToString(System.Web.HttpContext.Current.User.Identity.Name), item.AUTOID, item.QTY_RECEIVED.Trim());
                                if (fun.EXEC_QUERY(query))

                                    status = true;
                                Msg = "Saved Successfully..";
                                if (Convert.ToDouble(item.QUANTITY) > Convert.ToDouble(item.QTY_RECEIVED))
                                {
                                    query = string.Format(@"select family_code ||'#'|| itemcode from xxes_mrninfo where autoid = '{0}'", item.AUTOID);
                                    string output = fun.get_Col_Value(query);
                                    string[] arr = output.Split('#');
                                    double rejQty = Convert.ToDouble(item.QUANTITY) - Convert.ToDouble(item.QTY_RECEIVED);
                                    query = string.Format(@"INSERT INTO xxes_qc_log(PLANT_CODE,FAMILY_CODE,MRN,ITEMCODE,LAST_QTY,REJ_QTY,
                                        REMARKS,CREATEDBY,CREATEDDATE,TYPE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','SHORT RECEIVED','{6}',SYSDATE,'STORE_WEB')",
                                        item.PLANT, arr[0], item.MRN_NO, arr[1], item.QUANTITY, rejQty, Convert.ToString(Session["Login_User"]));
                                    fun.EXEC_QUERY(query);
                                }

                            }
                        //}
                    }
                    if (status)
                    {
                        string UserIpAddress = fun.GetUserIP();
                        query = string.Format(@"UPDATE ITEM_RECEIPT_DETIALS SET STORESCANBY = '{0}', STORESCANDATE = SYSDATE, STOREIPADDR = '{3}' 
                            WHERE PLANT_CODE = '{1}' AND MRN_NO = '{2}'", Convert.ToString(Session["Login_User"]), plant.Trim().ToUpper(), 
                            mrn.Trim().ToUpper(), 
                            UserIpAddress.Trim());
                        if (fun.EXEC_QUERY(query))
                            status = true;
                        Msg = "Saved Successfully..";
                    }
                }
                else
                {
                    var resul = "Enter QR Code..";
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Msg = "Error ! " + ex.Message;
            }
            var result = Msg;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Grid_History(GoodsRecivingatStoreModel OBJ)
        {
            try
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    query = string.Format(@"SELECT M.AUTOID, M.MRN_NO, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, I.VENDOR_CODE, SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME,I.VEHICLE_NO ,M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE,
                                        to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char(M.STORE_VERIFIEDDATE, 'dd-Mon-yyyy hh24:mm:ss') as STORE_VERIFIEDDATE 
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE trunc(M.STORE_VERIFIEDDATE) BETWEEN '{0}' AND '{1}' ORDER BY m.store_verifieddate DESC", OBJ.FROMDATE, OBJ.TODATE);
                }
                else
                {
                    query = string.Format(@"SELECT M.AUTOID, M.MRN_NO, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, I.VENDOR_CODE, SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME,I.VEHICLE_NO ,M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE,
                                        to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char( M.STORE_VERIFIEDDATE,  'dd-Mon-yyyy hh24:mm:ss' ) as STORE_VERIFIEDDATE 
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.PLANT_CODE = '{0}' AND trunc(M.STORE_VERIFIEDDATE) BETWEEN '{1}' AND '{2}' ORDER BY m.store_verifieddate DESC", Convert.ToString(Session["Login_Unit"]), OBJ.FROMDATE, OBJ.TODATE);
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
                GoodsRecivingatStoreModel OBJ = new GoodsRecivingatStoreModel();
                string date = string.Empty;
                date = Convert.ToString(ConfigurationSettings.AppSettings["DateForNewItemsAtGate"]);

                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, I.VEHICLE_NO,I.STORAGE
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STORE_VERIFIED IS NULL AND M.QUALITY_OK_BY IS NULL AND M.CREATEDDATE >= '{0}' ORDER BY TO_DATE (CREATEDDATE,'dd-Mon-yyyy HH24:MI:SS') DESC", date);
                }
                else
                {


                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, I.VEHICLE_NO,I.STORAGE
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STORE_VERIFIED IS NULL AND M.QUALITY_OK_BY IS NULL AND M.PLANT_CODE = '{0}' AND M.CREATEDDATE >= '{1}' ORDER BY TO_DATE (CREATEDDATE,'dd-Mon-yyyy HH24:MI:SS') DESC", Convert.ToString(Session["Login_Unit"]), date);

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

        public ActionResult Grid_Inspected_Items()
        {
            try
            {
                GoodsRecivingatStoreModel OBJ = new GoodsRecivingatStoreModel();
                int days = 0;
                days = Convert.ToInt32(ConfigurationSettings.AppSettings["LastPreviusDaysForStoreScreen"]);

                OBJ.FROMDATE = DateTime.Now.ToString("dd-MMM-yyyy");
                OBJ.TODATE = DateTime.Now.AddDays(-days).ToString("dd-MMM-yyyy");
                if (Convert.ToString(Session["Login_Unit"]) == "GU" || Convert.ToString(Session["Login_Unit"]) == "")
                {
                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE,SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char( M.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as STORE_VERIFIEDDATE, M.QUALITY_OK, M.QUALITY_OK_BY, to_char( M.QUALITY_OK_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) as QUALITY_OK_DATE 
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND M.CREATEDDATE BETWEEN '{0}' AND '{1}'", OBJ.FROMDATE, OBJ.TODATE);

                }
                else
                {
                    query = string.Format(@"SELECT M.AUTOID, I.VENDOR_CODE,SUBSTR(I.VENDOR_NAME,0,39) AS VENDOR_NAME, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as CREATEDDATE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,39) AS ITEM_DESCRIPTION, M.QUANTITY, M.PUNAME, M.STATUS, M.STORE_VERIFIED, M.STORE_VERIFIEDBY, to_char( M.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) as STORE_VERIFIEDDATE, M.QUALITY_OK, M.QUALITY_OK_BY, to_char( M.QUALITY_OK_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) as QUALITY_OK_DATE
                                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO WHERE M.STATUS = 'QA' AND M.PLANT_CODE = '{0}' AND M.PUNAME = '{1}' AND M.CREATEDDATE BETWEEN '{2}' AND '{3}'", Convert.ToString(Session["Login_Unit"]), Convert.ToString(Session["PUname"]), OBJ.FROMDATE, OBJ.TODATE);

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

        public JsonResult OpenChoicePopUp(BARCODEPRINT OBJ)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<DDLTextValue> result = new List<DDLTextValue>();
            int StandardSize = 0; int QTY_ORD = 0; int boxes = 0;
            int QTY_REC = 0;


            string PARAMVALUE = string.Format(@"SELECT PARAMVALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = 'VERIFICATION_PRINT' 
                                                AND PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}'", OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
            string PrintOption = fun.get_Col_Value(PARAMVALUE);
            if (string.IsNullOrEmpty(PrintOption))
            {
                msg = "Printing Option is not found either PDF OR BARCODE..!";
                mstType = Validation.str1;
                status = Validation.str2;
                var reult = new { Msg = msg, ID = mstType, validation = status };
                return Json(reult, JsonRequestBehavior.AllowGet);
            }


            string resul = GetPckStd(OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), OBJ.ITEMCODE.Trim());
            if (!string.IsNullOrEmpty(resul))
            {
                StandardSize = Convert.ToInt32(resul);
            }
            else
            {
                msg = "Packing Standard Size is not available in respect to item code..!";
                mstType = Validation.str1;
                status = Validation.str2;
                var reult = new { Msg = msg, ID = mstType, validation = status };
                return Json(reult, JsonRequestBehavior.AllowGet);
            }

            #region BEFORE_REC_WEB
            if (OBJ.MODE == "BEFORE_REC_WEB")
            {
                QTY_ORD = Convert.ToInt32(OBJ.QUANTITY);

                if (StandardSize >= QTY_ORD)
                {
                    result.Add(new DDLTextValue
                    {
                        Text = "Box1",
                        Value = "Box1",
                    });
                    var reult = new { List = result, Type = "BEFORE_REC_WEB", boxcnt = result.Count, PrintType = PrintOption };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else if (StandardSize != 0)
                {
                    if ((QTY_ORD % StandardSize) == 0)
                    {
                        boxes = QTY_ORD / StandardSize;
                    }
                    else
                    {
                        boxes = (QTY_ORD / StandardSize) + 1;
                    }

                    for (int i = 1; i <= boxes; i++)
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = "Box" + i,
                            Value = "Box" + i,
                        });
                    }
                    var reult = new { List = result, Type = "BEFORE_REC_WEB", boxcnt = result.Count, PrintType = PrintOption };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            #endregion

            #region AFTER_REC_WEB
            else if (OBJ.MODE == "AFTER_REC_WEB")
            {
                int QUANTITY = Convert.ToInt32(OBJ.QUANTITY);
                
                if (string.IsNullOrEmpty(OBJ.QTY_RECEIVED))
                {
                    msg = "QTY Received should not be blank..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                int j;
                if (!int.TryParse(OBJ.QTY_RECEIVED, out j))
                {
                    msg = "QTY Received should be number only..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                if (!(j > 0 && j <= QUANTITY))
                {
                    msg = "QTY Received should be greater than 0 and less than or equal to QTY Ordered..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);

                }

                if (!string.IsNullOrEmpty(OBJ.ITEMCODE) && !string.IsNullOrEmpty(OBJ.QTY_RECEIVED))
                {
                    
                    QTY_REC = Convert.ToInt32(OBJ.QUANTITY);

                    if (StandardSize >= QTY_REC)
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = "Box1",
                            Value = "Box1",
                        });
                        var reult = new { List = result, Type = "AFTER_REC_WEB", boxcnt = result.Count, PrintType = PrintOption };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                    else if (StandardSize != 0)
                    {
                        if ((QTY_REC % StandardSize) == 0)
                        {
                            boxes = QTY_REC / StandardSize;
                        }
                        else
                        {
                            boxes = (QTY_REC / StandardSize) + 1;
                        }

                        for (int i = 1; i <= boxes; i++)
                        {
                            result.Add(new DDLTextValue
                            {
                                Text = "Box" + i,
                                Value = "Box" + i,
                            });
                        }
                        var reult = new { List = result, Type = "AFTER_REC_WEB", boxcnt = result.Count, PrintType = PrintOption };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                }
                //}

            }
            #endregion
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public string GetPckStd(string plant,string family, string itemcode)
        {
            string pckStd = string.Format(@"SELECT PACKING_STANDARD FROM XXES_RAWMATERIAL_MASTER 
                            WHERE ITEM_CODE = '{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", itemcode.ToUpper().Trim(), 
                            plant.ToUpper().Trim(), family.ToUpper().Trim());
            return fun.get_Col_Value(pckStd);
        }
        public DataTable GETBOXWISE(string mrn, string itemcode,string boxno,string totalbox,string plant, string family)
        {
            query = string.Format(@"SELECT V.PLANT_CODE AS PLANT, V.FAMILY_CODE, V.ITEMCODE,SUBSTR(M.ITEM_DESCRIPTION,0,'50') AS ITEM_DESCRIPTION,
                                CASE WHEN I.VENDOR_NAME IS NULL THEN 'ESCORTS LTD' ELSE  SUBSTR(I.VENDOR_NAME,0,'50') END AS VENDOR_NAME,M.QUANTITY,
                                to_char(SYSDATE, 'dd-Mon-yyyy' ) AS DELIVERY_DATE,to_char( I.TRANSACTION_DATE, 'dd-Mon-yyyy') as  TRANSACTION_DATE, 
                                V.QUANTITY AS QTY_RECEIVED,(V.BOXNO||'/'||V.BOXCOUNT) AS COUNT,V.BARCODE,B.LOCATION_CODE BULKSTORAGE,
                                                     b.packaging_type BPACKAGING,b.bulk_storage_snp BULK_SNP,
                                                    CASE WHEN  (b.unpacked IS NULL OR b.unpacked = 'N') THEN 'N' ELSE 'Y' END UNPACKED
                                                    FROM XXES_VERIFYSTOREMRN V
                                                    INNER JOIN XXES_MRNINFO M
                                                    ON V.MRN = M.MRN_NO AND V.ITEMCODE = M.ITEMCODE AND V.PLANT_CODE = M.PLANT_CODE AND V.FAMILY_CODE = M.FAMILY_CODE 
                                                    INNER JOIN ITEM_RECEIPT_DETIALS I
                                                    ON V.MRN = I.MRN_NO AND V.PLANT_CODE = I.PLANT_CODE AND V.FAMILY_CODE = I.FAMILY_CODE 
                                                    INNER JOIN XXES_UNIT_MASTER U
                                                    ON V.PLANT_CODE = U.U_CODE
                                                    JOIN XXES_BULK_STORAGE B            
                                                    ON M.PLANT_CODE = B.PLANT_CODE AND M.FAMILY_CODE = B.FAMILY_CODE AND M.ITEMCODE = B.ITEM_CODE

                                                     WHERE V.MRN = '{0}' AND V.ITEMCODE = '{1}' AND V.BOXNO = '{2}' AND V.BOXCOUNT = '{3}' 
                                                    AND V.PLANT_CODE = '{4}' AND V.FAMILY_CODE = '{5}'",
                                                   mrn.Trim(), itemcode.Trim(), boxno.Trim(), totalbox.Trim(), plant.Trim(), family.Trim());
            
            return fun.returnDataTable(query);
        }
        public DataTable GETPACKSTDWISE(string mrn, string itemcode, string plant, string family)
        {
            query = string.Format(@"SELECT M.PLANT_CODE AS PLANT,M.FAMILY_CODE, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,'50') AS ITEM_DESCRIPTION, 
                                                    CASE WHEN I.VENDOR_NAME IS NULL THEN 'ESCORTS LTD' ELSE  SUBSTR(I.VENDOR_NAME,0,'50') END AS VENDOR_NAME, 
                                                    M.QUANTITY,to_char(SYSDATE, 'dd-Mon-yyyy' ) DELIVERY_DATE, 
                                                    to_char( I.TRANSACTION_DATE, 'dd-Mon-yyyy') as  TRANSACTION_DATE,
                                                    R.PACKING_STANDARD AS QTY_RECEIVED,
                                                    B.LOCATION_CODE BULKSTORAGE,b.packaging_type BPACKAGING,
                                                    b.bulk_storage_snp BULK_SNP,
                                                    CASE WHEN (b.unpacked IS NULL OR b.unpacked = 'N') THEN 'N' ELSE 'Y' END UNPACKED
                                                    FROM XXES_MRNINFO M 
                                                    JOIN ITEM_RECEIPT_DETIALS I
                                                    ON M.MRN_NO = I.MRN_NO AND M.PLANT_CODE = I.PLANT_CODE AND M.FAMILY_CODE = I.FAMILY_CODE   
                                                     JOIN XXES_RAWMATERIAL_MASTER R
                                                    ON M.ITEMCODE = R.ITEM_CODE AND M.PLANT_CODE = R.PLANT_CODE AND M.FAMILY_CODE = R.FAMILY_CODE
                                                    JOIN XXES_UNIT_MASTER U
                                                    ON M.PLANT_CODE = U.U_CODE
                                                    JOIN XXES_BULK_STORAGE B
                                                    ON M.PLANT_CODE = B.PLANT_CODE AND M.FAMILY_CODE = B.FAMILY_CODE AND M.ITEMCODE = B.ITEM_CODE
                                                    WHERE M.MRN_NO = '{0}' AND M.ITEMCODE = '{1}'
                                                    and m.plant_code='{2}' and m.family_code='{3}'",
                                            mrn.Trim(), itemcode.Trim(), plant.Trim(), family.Trim());

           return fun.returnDataTable(query);
        }
        public JsonResult printBarcodes(BARCODEPRINT OBJ)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string substr = string.Empty; int NoBox = Convert.ToInt32(OBJ.TOTALBOX); int QTY_RECD = 0; int PACKING_STD = 0;
            MAHController mAHController = new MAHController();
            BARCODEPRINT obj = new BARCODEPRINT();
            List<BARCODEPRINT> barcodeList = new List<BARCODEPRINT>();
            PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
            string plant, PO, ItemCode, BULKSTORAGE, LINE, supplier, IF, transactionDate, qrBeforeQty_recAndBox, qrAfterQty_rec = string.Empty;
            try
            {

                string resul = GetPckStd(OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), OBJ.ITEMCODE.Trim());
                if (!string.IsNullOrEmpty(resul))
                {
                    PACKING_STD = Convert.ToInt32(resul);
                }
                else
                {
                    msg = "Packing Standard Size is not available in respect to item code..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                if (OBJ.MODE == "BEFORE_REC_WEB")
                {

                    dt = mAHController.GetITEMDETAILS(OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
                    if (dt.Rows.Count > 0)
                    {
                        plant = dt.Rows[0]["PLANT_CODE"].ToString();
                        PO = "PO";
                        ItemCode = dt.Rows[0]["ITEM_CODE"].ToString();

                        BULKSTORAGE = dt.Rows[0]["BULK_LOC"].ToString();
                        LINE = "LINE";
                        supplier = dt.Rows[0]["SUPP_NAME"].ToString();
                        IF = "IF";
                        transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");

                        qrBeforeQty_recAndBox = plant + "$" + PO + "$" + ItemCode + "$";
                        qrAfterQty_rec = "$" + BULKSTORAGE + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$";


                        dt.Columns.Add("BOX_NO", typeof(string));
                        dt.Columns.Add("QR_CODE", typeof(string));
                        DataRow dr = null;
                        //int PACKING_STD = Convert.ToInt32(dt.Rows[0]["PACKING_STANDARD"]);

                        if (OBJ.MODE == "BEFORE_REC_WEB")
                        {
                            QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]);
                        }
                        else
                        {
                            QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_DLV"]);
                        }


                        if (OBJ.STIKER == "All")
                        {

                            int bal_qty = 0;

                            for (int i = 0; i < NoBox; i++)
                            {

                                bal_qty = 0 + bal_qty;

                                if (i == 0)
                                {
                                    dr = dt.Rows[i];

                                    string QtyReceived = string.Empty;
                                    if (QTY_RECD > PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = PACKING_STD.ToString();
                                        QtyReceived = PACKING_STD.ToString();
                                        bal_qty = QTY_RECD - PACKING_STD;
                                    }
                                    else if (QTY_RECD == PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = PACKING_STD.ToString();
                                        QtyReceived = PACKING_STD.ToString();
                                        bal_qty = QTY_RECD - PACKING_STD;
                                    }
                                    else if (QTY_RECD < PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = QTY_RECD.ToString();
                                        QtyReceived = QTY_RECD.ToString();
                                        bal_qty = 0;
                                    }

                                    dr["BOX_NO"] = (i + 1) + "/" + NoBox;
                                    string BOX = (i + 1) + "/" + NoBox;
                                    string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + OBJ.MRN_NO.Trim();
                                    dr["QR_CODE"] = BarcodeMsg;
                                }
                                else
                                {
                                    dr = dt.NewRow();
                                    string QtyReceived = string.Empty;
                                    //add query cell value
                                    dr["PLANT_CODE"] = dt.Rows[0]["PLANT_CODE"].ToString();
                                    dr["FAMILY_CODE"] = dt.Rows[0]["FAMILY_CODE"].ToString();
                                    dr["MRN_NO"] = dt.Rows[0]["MRN_NO"].ToString();
                                    dr["TRANSACTION_DATE"] = dt.Rows[0]["TRANSACTION_DATE"].ToString();
                                    dr["U_NAME"] = dt.Rows[0]["U_NAME"].ToString();
                                    dr["SUPP_NAME"] = dt.Rows[0]["SUPP_NAME"].ToString();
                                    dr["BULK_LOC"] = dt.Rows[0]["BULK_LOC"].ToString();
                                    dr["ITEM_CODE"] = dt.Rows[0]["ITEM_CODE"].ToString();
                                    dr["CURRENT_DATE"] = dt.Rows[0]["CURRENT_DATE"].ToString();
                                    dr["QTY_ORD"] = dt.Rows[0]["QTY_ORD"].ToString();
                                    dr["ITEM_DESC"] = dt.Rows[0]["ITEM_DESC"].ToString();
                                    dr["PACKING_STANDARD"] = dt.Rows[0]["PACKING_STANDARD"].ToString();
                                    dr["VENDOR_CODE"] = dt.Rows[0]["VENDOR_CODE"].ToString();
                                    dr["CREATEDDATE"] = dt.Rows[0]["CREATEDDATE"].ToString();
                                    dr["PUNAME"] = dt.Rows[0]["PUNAME"].ToString();
                                    dr["BPACKAGING"] = dt.Rows[0]["BPACKAGING"].ToString();
                                    dr["BULK_SNP"] = dt.Rows[0]["BULK_SNP"].ToString();
                                    dr["UNPACKED"] = dt.Rows[0]["UNPACKED"].ToString();
                                    if (bal_qty > PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = PACKING_STD.ToString();
                                        QtyReceived = PACKING_STD.ToString();
                                        bal_qty = bal_qty - PACKING_STD;
                                    }
                                    else if (bal_qty == PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = PACKING_STD.ToString();
                                        QtyReceived = PACKING_STD.ToString();
                                        bal_qty = bal_qty - PACKING_STD;
                                    }
                                    else if (bal_qty < PACKING_STD)
                                    {
                                        //QTY_RECD = PACKING_STD;
                                        dr["QTY_DLV"] = bal_qty.ToString();
                                        QtyReceived = bal_qty.ToString();
                                        bal_qty = 0;
                                    }
                                    dr["BOX_NO"] = (i + 1) + "/" + NoBox;
                                    string BOX = (i + 1) + "/" + NoBox;
                                    string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + OBJ.MRN_NO.Trim();
                                    dr["QR_CODE"] = BarcodeMsg;
                                    dt.Rows.Add(dr);
                                }

                            }

                        }
                        else
                        {
                            string QtyReceived = string.Empty;
                            substr = OBJ.STIKER.Substring(3);
                            dr = dt.Rows[0];

                            int BoxQty;
                            if (Convert.ToInt32(substr) == NoBox)
                            {
                                BoxQty = QTY_RECD - ((NoBox - 1) * PACKING_STD);
                                dr["QTY_DLV"] = BoxQty.ToString();
                                QtyReceived = BoxQty.ToString();
                            }
                            else
                            {
                                BoxQty = PACKING_STD;
                                dr["QTY_DLV"] = BoxQty.ToString();
                                QtyReceived = BoxQty.ToString();
                            }
                            dr["BOX_NO"] = substr + "/" + NoBox;
                            string BOX = substr + "/" + NoBox;
                            string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + OBJ.MRN_NO.Trim();
                            dr["QR_CODE"] = BarcodeMsg;


                        }
                    }

                    
                    barcodeList = (from DataRow dr in dt.Rows
                                   select new BARCODEPRINT()
                                   {
                                       PLANT = Convert.ToString(dr["PLANT_CODE"]),
                                       FAMILY = Convert.ToString(dr["FAMILY_CODE"]),
                                       MRN_NO = Convert.ToString(dr["MRN_NO"]),
                                       ITEMCODE = Convert.ToString(dr["ITEM_CODE"]),
                                       ITEM_DESC = Convert.ToString(dr["ITEM_DESC"]),
                                       SUPP_NAME = Convert.ToString(dr["SUPP_NAME"]),
                                       QTY_ORD = Convert.ToString(dr["QTY_ORD"]),
                                       PKG_STD = Convert.ToString(dr["PACKING_STANDARD"]),
                                       CURRENT_DATE = Convert.ToString(dr["CURRENT_DATE"]),
                                       PUNAME = Convert.ToString(dr["PUNAME"]),
                                       TRANSACTION_DATE = Convert.ToString(dr["TRANSACTION_DATE"]),
                                       QTY_DLV = Convert.ToString(dr["QTY_DLV"]),
                                       BULK_LOC = Convert.ToString(dr["BULK_LOC"]),
                                       BPACKAGING = Convert.ToString(dr["BPACKAGING"]),
                                       BULK_SNP = Convert.ToString(dr["BULK_SNP"]),
                                       BOX_NO = Convert.ToString(dr["BOX_NO"]),
                                       QR_CODE = Convert.ToString(dr["QR_CODE"]),
                                       UNPACKED = Convert.ToString(dr["UNPACKED"])
                                   }).ToList();


                    
                    if (barcodes.PrintBoxs(barcodeList))
                    {
                        msg = "Barcode Printing Successfully";
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


                if (OBJ.MODE == "AFTER_REC_WEB")
                {

                    if (OBJ.STIKER == "All")
                    {
                        for(int i = 1; i<= NoBox; i++)
                        {
                            dt = GETBOXWISE(OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim(), i.ToString(), NoBox.ToString(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
                            if (dt.Rows.Count > 0)
                            {
                                //box found in verfiystore table so print as per actual quantity

                                foreach (DataRow dr in dt.Rows)
                                {
                                    barcodeList.Add(new BARCODEPRINT() {
                                    PLANT = Convert.ToString(dr["PLANT"]),
                                    FAMILY = Convert.ToString(dr["FAMILY_CODE"]),
                                    ITEMCODE = Convert.ToString(dr["ITEMCODE"]),
                                    ITEM_DESC = Convert.ToString(dr["ITEM_DESCRIPTION"]),
                                    SUPP_NAME = Convert.ToString(dr["VENDOR_NAME"]),
                                    QTY_ORD = Convert.ToString(dr["QUANTITY"]),
                                    CURRENT_DATE = Convert.ToString(dr["DELIVERY_DATE"]),
                                    TRANSACTION_DATE = Convert.ToString(dr["TRANSACTION_DATE"]),
                                    QTY_DLV = Convert.ToString(dr["QTY_RECEIVED"]),
                                    BOX_NO = Convert.ToString(dr["COUNT"]),
                                    QR_CODE = Convert.ToString(dr["BARCODE"]),
                                    BULK_LOC = Convert.ToString(dr["BULKSTORAGE"]),
                                    BPACKAGING = Convert.ToString(dr["BPACKAGING"]),
                                    BULK_SNP = Convert.ToString(dr["BULK_SNP"]),
                                    UNPACKED = Convert.ToString(dr["UNPACKED"])

                                });
                                    
                                   

                                    
                                } 
                            }
                            else
                            {
                                dt = GETPACKSTDWISE(OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());

                                plant = dt.Rows[0]["PLANT"].ToString();
                                PO = "PO";
                                ItemCode = dt.Rows[0]["ITEMCODE"].ToString();

                                BULKSTORAGE = dt.Rows[0]["BULKSTORAGE"].ToString();
                                LINE = "LINE";
                                supplier = dt.Rows[0]["VENDOR_NAME"].ToString();
                                IF = "IF";
                                transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");

                                qrBeforeQty_recAndBox = plant + "$" + PO + "$" + ItemCode + "$";
                                qrAfterQty_rec = "$" + BULKSTORAGE + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$";


                                dt.Columns.Add("COUNT", typeof(string));
                                dt.Columns.Add("BARCODE", typeof(string));
                                DataRow dr = null;


                                QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_RECEIVED"]);


                                string QtyReceived = string.Empty;
                                substr = i.ToString();
                                dr = dt.Rows[0];

                                int BoxQty;
                                if (Convert.ToInt32(substr) == NoBox)
                                {
                                    BoxQty = QTY_RECD - ((NoBox - 1) * PACKING_STD);
                                    dr["QTY_RECEIVED"] = BoxQty.ToString();
                                    QtyReceived = BoxQty.ToString();
                                }
                                else
                                {
                                    BoxQty = PACKING_STD;
                                    dr["QTY_RECEIVED"] = BoxQty.ToString();
                                    QtyReceived = BoxQty.ToString();
                                }
                                dr["COUNT"] = substr + "/" + NoBox;
                                string BOX = substr + "/" + NoBox;
                                string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + OBJ.MRN_NO.Trim();
                                dr["BARCODE"] = BarcodeMsg;

                                foreach (DataRow dr1 in dt.Rows)
                                {
                                    barcodeList.Add(new BARCODEPRINT() {
                                   PLANT = Convert.ToString(dr1["PLANT"]),
                                   FAMILY = Convert.ToString(dr1["FAMILY_CODE"]),
                                   ITEMCODE = Convert.ToString(dr1["ITEMCODE"]),
                                   ITEM_DESC = Convert.ToString(dr1["ITEM_DESCRIPTION"]),
                                   SUPP_NAME = Convert.ToString(dr1["VENDOR_NAME"]),
                                   QTY_ORD = Convert.ToString(dr1["QUANTITY"]),
                                   CURRENT_DATE = Convert.ToString(dr1["DELIVERY_DATE"]),
                                   TRANSACTION_DATE = Convert.ToString(dr1["TRANSACTION_DATE"]),
                                   QTY_DLV = Convert.ToString(dr1["QTY_RECEIVED"]),
                                   BOX_NO = Convert.ToString(dr1["COUNT"]),
                                   QR_CODE = Convert.ToString(dr1["BARCODE"]),
                                   BULK_LOC = Convert.ToString(dr1["BULKSTORAGE"]),
                                   BPACKAGING = Convert.ToString(dr1["BPACKAGING"]),
                                   BULK_SNP = Convert.ToString(dr1["BULK_SNP"]),
                                   UNPACKED = Convert.ToString(dr1["UNPACKED"])

                                });
                                    
                                    

                                    
                                }
                            }
                        }
                        if (barcodes.PrintBoxs(barcodeList))
                        {
                            msg = "Barcode Printing Successfully";
                            mstType = Validation.str;
                            status = Validation.stus;
                        }
                        else
                        {
                            msg = "Error in printing";
                            mstType = Validation.str1;
                            status = Validation.str2;
                        }
                        
                        var RE = new { Msg = msg, ID = mstType, validation = status };
                        return Json(RE, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        substr = OBJ.STIKER.Substring(3);
                        dt = GETBOXWISE(OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim(), substr.Trim(), NoBox.ToString(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim()); 
                        if (dt.Rows.Count > 0)
                        {
                            //box found in verfiystore table so print as per actual quantity
                           
                            foreach (DataRow dr in dt.Rows)
                            {
                                obj.PLANT = Convert.ToString(dr["PLANT"]);
                                obj.FAMILY = Convert.ToString(dr["FAMILY_CODE"]);
                                //MRN_NO = Convert.ToString(dr["MRN"]),
                                obj.ITEMCODE = Convert.ToString(dr["ITEMCODE"]);
                                obj.ITEM_DESC = Convert.ToString(dr["ITEM_DESCRIPTION"]);
                                obj.SUPP_NAME = Convert.ToString(dr["VENDOR_NAME"]);
                                obj.QTY_ORD = Convert.ToString(dr["QUANTITY"]);
                                obj.CURRENT_DATE = Convert.ToString(dr["DELIVERY_DATE"]);
                                obj.TRANSACTION_DATE = Convert.ToString(dr["TRANSACTION_DATE"]);
                                obj.QTY_DLV = Convert.ToString(dr["QTY_RECEIVED"]);
                                obj.BOX_NO = Convert.ToString(dr["COUNT"]);
                                obj.QR_CODE = Convert.ToString(dr["BARCODE"]);
                                obj.BULK_LOC = Convert.ToString(dr["BULKSTORAGE"]);
                                obj.BPACKAGING = Convert.ToString(dr["BPACKAGING"]);
                                obj.BULK_SNP = Convert.ToString(dr["BULK_SNP"]);
                                obj.UNPACKED = Convert.ToString(dr["UNPACKED"]);
                                //PKG_STD = Convert.ToString(dr["PACKING_STANDARD"]),
                                //PUNAME = Convert.ToString(dr["PUNAME"]) 

                                barcodeList.Add(obj);
                            }
                                                       
                            if (barcodes.PrintBoxs(barcodeList))
                            {
                                msg = "Barcode Printing Successfully";
                                mstType = Validation.str;
                                status = Validation.stus;
                            }
                            else
                            {
                                msg = "Error in printing";
                                mstType = Validation.str1;
                                status = Validation.str2;
                            }
                            var resu = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resu, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            
                            dt = GETPACKSTDWISE(OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim()); 

                            plant = dt.Rows[0]["PLANT"].ToString();
                            PO = "PO";
                            ItemCode = dt.Rows[0]["ITEMCODE"].ToString();

                            BULKSTORAGE = dt.Rows[0]["BULKSTORAGE"].ToString();
                            LINE = "LINE";
                            supplier = dt.Rows[0]["VENDOR_NAME"].ToString();
                            IF = "IF";
                            transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");

                            qrBeforeQty_recAndBox = plant + "$" + PO + "$" + ItemCode + "$";
                            qrAfterQty_rec = "$" + BULKSTORAGE + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$";


                            dt.Columns.Add("COUNT", typeof(string));
                            dt.Columns.Add("BARCODE", typeof(string));
                            DataRow dr = null;
                            
                            
                                QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_RECEIVED"]);
                            

                            string QtyReceived = string.Empty;
                            //substr = OBJ.STIKER.Substring(3);
                            dr = dt.Rows[0];

                            int BoxQty;
                            if (Convert.ToInt32(substr) == NoBox)
                            {
                                BoxQty = QTY_RECD - ((NoBox - 1) * PACKING_STD);
                                dr["QTY_RECEIVED"] = BoxQty.ToString();
                                QtyReceived = BoxQty.ToString();
                            }
                            else
                            {
                                BoxQty = PACKING_STD;
                                dr["QTY_RECEIVED"] = BoxQty.ToString();
                                QtyReceived = BoxQty.ToString();
                            }
                            dr["COUNT"] = substr + "/" + NoBox;
                            string BOX = substr + "/" + NoBox;
                            string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + OBJ.MRN_NO.Trim();
                            dr["BARCODE"] = BarcodeMsg;
                        }

                        foreach (DataRow dr in dt.Rows)
                        {
                            obj.PLANT = Convert.ToString(dr["PLANT"]);
                            obj.FAMILY = Convert.ToString(dr["FAMILY_CODE"]);
                            //MRN_NO = Convert.ToString(dr["MRN"]),
                            obj.ITEMCODE = Convert.ToString(dr["ITEMCODE"]);
                            obj.ITEM_DESC = Convert.ToString(dr["ITEM_DESCRIPTION"]);
                            obj.SUPP_NAME = Convert.ToString(dr["VENDOR_NAME"]);
                            obj.QTY_ORD = Convert.ToString(dr["QUANTITY"]);
                            obj.CURRENT_DATE = Convert.ToString(dr["DELIVERY_DATE"]);
                            obj.TRANSACTION_DATE = Convert.ToString(dr["TRANSACTION_DATE"]);
                            obj.QTY_DLV = Convert.ToString(dr["QTY_RECEIVED"]);
                            obj.BOX_NO = Convert.ToString(dr["COUNT"]);
                            obj.QR_CODE = Convert.ToString(dr["BARCODE"]);
                            obj.BULK_LOC = Convert.ToString(dr["BULKSTORAGE"]);
                            obj.BPACKAGING = Convert.ToString(dr["BPACKAGING"]);
                            obj.BULK_SNP = Convert.ToString(dr["BULK_SNP"]);
                            obj.UNPACKED = Convert.ToString(dr["UNPACKED"]);
                            //PKG_STD = Convert.ToString(dr["PACKING_STANDARD"]),
                            //PUNAME = Convert.ToString(dr["PUNAME"]) 

                            barcodeList.Add(obj);
                        }

                        
                        if (barcodes.PrintBoxs(barcodeList))
                        {
                            msg = "Barcode Printing Successfully";
                            mstType = Validation.str;
                            status = Validation.stus;
                        }
                        else
                        {
                            msg = "Error in printing";
                            mstType = Validation.str1;
                            status = Validation.str2;
                        }
                        var RE = new { Msg = msg, ID = mstType, validation = status };
                        return Json(RE, JsonRequestBehavior.AllowGet);
                    }

                }

            }
            catch (Exception ex)
            {

                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var RES = new { Msg = msg, ID = mstType, validation = status };
                return Json(RES, JsonRequestBehavior.AllowGet);

            }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CrystalReport()
        {
            string MRN = Request.QueryString["MRN"].ToString();
            string ITEMCOD = Request.QueryString["ITEMCOD"].ToString();
            string Stiker = Request.QueryString["Stiker"].ToString();
            int NoBox = Convert.ToInt16(Request.QueryString["NoBox"]);
            string Mode = Request.QueryString["Mode"].ToString();
            string substr = string.Empty; int QTY_RECD = 0;

            if (Mode == "BEFORE_REC_WEB" || Mode == "AFTER_REC_WEB")
            {


                query = string.Format(@"SELECT M.PLANT_CODE PLANT, M.ITEMCODE, SUBSTR(M.ITEM_DESCRIPTION,0,'35') ITEM_DESC,
                                SUBSTR(I.VENDOR_NAME,0,'50') AS SUPP_NAME, M.QUANTITY QTY_ORD, 
                                r.packing_standard PKG_STD,to_char(SYSDATE, 'dd-Mon-yyyy' ) CURRENT_DATE,U.U_NAME,M.PUNAME, to_char( I.TRANSACTION_DATE, 'dd-Mon-yyyy') as  TRANSACTION_DATE,
                                M.REC_QTY AS QTY_DLV,B.LOCATION_CODE BULK_LOC,b.packaging_type BPACKAGING,b.bulk_storage_snp BULK_SNP,
                                CASE WHEN (b.unpacked IS NULL OR b.unpacked = 'N') THEN 'N' ELSE 'Y' END UNPACKED
                                FROM XXES_MRNINFO M 
                                JOIN ITEM_RECEIPT_DETIALS I
                                ON M.MRN_NO = I.MRN_NO AND M.PLANT_CODE = I.PLANT_CODE AND M.FAMILY_CODE = I.FAMILY_CODE
                                JOIN XXES_RAWMATERIAL_MASTER R
                                ON M.ITEMCODE = R.ITEM_CODE AND M.PLANT_CODE = R.PLANT_CODE AND M.FAMILY_CODE = R.FAMILY_CODE  
                                JOIN XXES_UNIT_MASTER U
                                ON M.PLANT_CODE = U.U_CODE
                                JOIN XXES_BULK_STORAGE B
                                ON M.PLANT_CODE = B.PLANT_CODE AND M.FAMILY_CODE = B.FAMILY_CODE AND M.ITEMCODE = B.ITEM_CODE
                                WHERE M.MRN_NO = '{0}' AND M.ITEMCODE = '{1}'", MRN.Trim(), ITEMCOD.Trim());
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    string plant = dt.Rows[0]["PLANT"].ToString();
                    string PO = "PO";
                    string ItemCode = dt.Rows[0]["ITEMCODE"].ToString();

                    string BULKSTORAGE = dt.Rows[0]["BULK_LOC"].ToString();
                    string LINE = "LINE";
                    string supplier = dt.Rows[0]["SUPP_NAME"].ToString();
                    string IF = "IF";
                    string transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");

                    string qrBeforeQty_recAndBox = plant + "$" + PO + "$" + ItemCode + "$";
                    string qrAfterQty_rec = "$" + BULKSTORAGE + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$";


                    dt.Columns.Add("BOX_NO", typeof(string));
                    dt.Columns.Add("QR_CODE", typeof(byte[]));
                    DataRow dr = null;
                    int PACKING_STD = Convert.ToInt32(dt.Rows[0]["PKG_STD"]);

                    if (Mode == "BEFORE_REC_WEB")
                    {
                        QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]);
                    }
                    else
                    {
                        QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_DLV"]);
                    }

                    if (Stiker == "All")
                    {

                        int bal_qty = 0;
                        for (int i = 0; i < NoBox; i++)
                        {

                            bal_qty = 0 + bal_qty;

                            if (i == 0)
                            {
                                dr = dt.Rows[i];

                                string QtyReceived = string.Empty;
                                if (QTY_RECD > PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = PACKING_STD.ToString();
                                    QtyReceived = PACKING_STD.ToString();
                                    bal_qty = QTY_RECD - PACKING_STD;
                                }
                                else if (QTY_RECD == PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = PACKING_STD.ToString();
                                    QtyReceived = PACKING_STD.ToString();
                                    bal_qty = QTY_RECD - PACKING_STD;
                                }
                                else if (QTY_RECD < PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = QTY_RECD.ToString();
                                    QtyReceived = QTY_RECD.ToString();
                                    bal_qty = 0;
                                }

                                dr["BOX_NO"] = (i + 1) + "/" + NoBox;
                                string BOX = (i + 1) + "/" + NoBox;
                                string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + MRN.Trim();
                                dr["QR_CODE"] = GenerateQrCode(BarcodeMsg);
                            }
                            else
                            {
                                dr = dt.NewRow();
                                string QtyReceived = string.Empty;
                                //add query cell value
                                dr["PLANT"] = dt.Rows[0]["PLANT"].ToString();
                                dr["TRANSACTION_DATE"] = dt.Rows[0]["TRANSACTION_DATE"].ToString();
                                dr["U_NAME"] = dt.Rows[0]["U_NAME"].ToString();
                                dr["SUPP_NAME"] = dt.Rows[0]["SUPP_NAME"].ToString();
                                dr["BULK_LOC"] = dt.Rows[0]["BULK_LOC"].ToString();
                                dr["ITEMCODE"] = dt.Rows[0]["ITEMCODE"].ToString();
                                dr["CURRENT_DATE"] = dt.Rows[0]["CURRENT_DATE"].ToString();
                                dr["QTY_ORD"] = dt.Rows[0]["QTY_ORD"].ToString();
                                dr["ITEM_DESC"] = dt.Rows[0]["ITEM_DESC"].ToString();
                                dr["PKG_STD"] = dt.Rows[0]["PKG_STD"].ToString();
                                dr["PUNAME"] = dt.Rows[0]["PUNAME"].ToString();
                                dr["BPACKAGING"] = dt.Rows[0]["BPACKAGING"].ToString();
                                dr["BULK_SNP"] = dt.Rows[0]["BULK_SNP"].ToString();
                                dr["UNPACKED"] = dt.Rows[0]["UNPACKED"].ToString();
                                if (bal_qty > PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = PACKING_STD.ToString();
                                    QtyReceived = PACKING_STD.ToString();
                                    bal_qty = bal_qty - PACKING_STD;
                                }
                                else if (bal_qty == PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = PACKING_STD.ToString();
                                    QtyReceived = PACKING_STD.ToString();
                                    bal_qty = bal_qty - PACKING_STD;
                                }
                                else if (bal_qty < PACKING_STD)
                                {
                                    //QTY_RECD = PACKING_STD;
                                    dr["QTY_DLV"] = bal_qty.ToString();
                                    QtyReceived = bal_qty.ToString();
                                    bal_qty = 0;
                                }
                                dr["BOX_NO"] = (i + 1) + "/" + NoBox;
                                string BOX = (i + 1) + "/" + NoBox;
                                string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + MRN.Trim();
                                dr["QR_CODE"] = GenerateQrCode(BarcodeMsg);
                                dt.Rows.Add(dr);
                            }

                        }

                    }
                    else
                    {
                        string QtyReceived = string.Empty;
                        substr = Stiker.Substring(3);
                        dr = dt.Rows[0];

                        int BoxQty;
                        if (Convert.ToInt32(substr) == NoBox)
                        {
                            BoxQty = QTY_RECD - ((NoBox - 1) * PACKING_STD);
                            dr["QTY_DLV"] = BoxQty.ToString();
                            QtyReceived = BoxQty.ToString();
                        }
                        else
                        {
                            BoxQty = PACKING_STD;
                            dr["QTY_DLV"] = BoxQty.ToString();
                            QtyReceived = BoxQty.ToString();
                        }
                        dr["BOX_NO"] = substr + "/" + NoBox;
                        string BOX = substr + "/" + NoBox;
                        string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + MRN.Trim();
                        dr["QR_CODE"] = GenerateQrCode(BarcodeMsg);


                    }
                }
            }

            else if (Mode == "AFTER_REC_DCU")
            {
                query = string.Format(@"SELECT V.PLANT_CODE PLANT, V.ITEMCODE,SUBSTR(M.ITEM_DESCRIPTION,0,'35') AS ITEM_DESC,
                                                     SUBSTR(I.VENDOR_NAME,0,'50') AS SUPP_NAME,M.QUANTITY QTY_ORD,
                                                     R.PACKING_STANDARD PKG_STD,to_char(SYSDATE, 'dd-Mon-yyyy' ) AS CURRENT_DATE,U.U_NAME,
                                                     M.PUNAME, to_char( I.TRANSACTION_DATE, 'dd-Mon-yyyy') as  TRANSACTION_DATE,  V.QUANTITY AS QTY_DLV,
                                                     (V.BOXNO||'/'||V.BOXCOUNT) AS BOX_NO,B.LOCATION_CODE BULK_LOC,
                                                     b.packaging_type BPACKAGING,b.bulk_storage_snp BULK_SNP,
                                                    CASE WHEN  (b.unpacked IS NULL OR b.unpacked = 'N') THEN 'N' ELSE 'Y' END UNPACKED
                                                    FROM XXES_VERIFYSTOREMRN V
                                                    INNER JOIN XXES_MRNINFO M
                                                    ON V.MRN = M.MRN_NO AND V.ITEMCODE = M.ITEMCODE AND V.PLANT_CODE = M.PLANT_CODE AND V.FAMILY_CODE = M.FAMILY_CODE  
                                                    INNER JOIN ITEM_RECEIPT_DETIALS I
                                                    ON V.MRN = I.MRN_NO AND V.PLANT_CODE = I.PLANT_CODE AND V.FAMILY_CODE = I.FAMILY_CODE 
                                                    INNER JOIN XXES_RAWMATERIAL_MASTER R
                                                    ON V.ITEMCODE = R.ITEM_CODE AND V.PLANT_CODE = R.PLANT_CODE AND V.FAMILY_CODE = R.FAMILY_CODE  
                                                    INNER JOIN XXES_UNIT_MASTER U
                                                    ON V.PLANT_CODE = U.U_CODE
                                                    JOIN XXES_BULK_STORAGE B            
                                            ON M.PLANT_CODE = B.PLANT_CODE AND M.FAMILY_CODE = B.FAMILY_CODE AND M.ITEMCODE = B.ITEM_CODE

                                                     WHERE V.MRN = '{0}' AND V.ITEMCODE = '{1}' ORDER BY CAST(V.BOXNO AS NUMBER)", MRN.Trim(), ITEMCOD.Trim());
                dt = fun.returnDataTable(query);

                string plant = string.Empty; string ItemCode = string.Empty; string bulk_storage = string.Empty;
                string supplier = string.Empty; string transactionDate = string.Empty; string qty_rec = string.Empty;
                string boxno = string.Empty; string barcode = string.Empty;
                string PO = "PO"; string LINE = "LINE"; string IF = "IF";
                dt.Columns.Add("QR_CODE", typeof(byte[]));
                if (dt.Rows.Count > 0)
                {
                    if (Stiker == "All")
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            plant = dt.Rows[i]["PLANT"].ToString();
                            PO = "PO";
                            ItemCode = dt.Rows[i]["ITEMCODE"].ToString();

                            //PUNAME = dt.Rows[i]["PUNAME"].ToString();
                            bulk_storage = dt.Rows[i]["BULK_LOC"].ToString();
                            LINE = "LINE";
                            supplier = dt.Rows[i]["SUPP_NAME"].ToString();
                            IF = "IF";
                            //transactionDate = dt.Rows[i]["TRANSACTION_DATE"].ToString();
                            transactionDate = Convert.ToDateTime(dt.Rows[i]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");
                            qty_rec = dt.Rows[i]["QTY_DLV"].ToString();
                            boxno = dt.Rows[i]["BOX_NO"].ToString();

                            barcode = string.Empty;
                            barcode = plant + "$" + PO + "$" + ItemCode + "$" + qty_rec + "$" + bulk_storage + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$" + boxno + "$" + MRN.Trim();

                            dt.Rows[i]["QR_CODE"] = GenerateQrCode(barcode);
                        }
                        return GetReport(dt);
                    }

                    else
                    {

                        substr = Stiker.Substring(3);
                        string boxcount = substr + "/" + NoBox;

                        DataRow[] result = dt.Select($"BOX_NO <> '{boxcount}'");
                        foreach (DataRow row in result)
                        {
                            if (Convert.ToString(row["BOX_NO"]).Trim() != boxcount.Trim())
                                dt.Rows.Remove(row);
                        }

                        plant = dt.Rows[0]["PLANT"].ToString();
                        PO = "PO";
                        ItemCode = dt.Rows[0]["ITEMCODE"].ToString();

                        bulk_storage = dt.Rows[0]["BULK_LOC"].ToString();
                        LINE = "LINE";
                        supplier = dt.Rows[0]["SUPP_NAME"].ToString();
                        IF = "IF";
                        //transactionDate = dt.Rows[0]["TRANSACTION_DATE"].ToString().Replace('-', '/');
                        transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");
                        qty_rec = dt.Rows[0]["QTY_DLV"].ToString();
                        boxno = dt.Rows[0]["BOX_NO"].ToString();

                        barcode = string.Empty;
                        barcode = plant + "$" + PO + "$" + ItemCode + "$" + qty_rec + "$" + bulk_storage + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$" + boxno + "$" + MRN.Trim();

                        dt.Rows[0]["QR_CODE"] = GenerateQrCode(barcode);

                        return GetReport(dt);

                    }

                }


            }

            return GetReport(dt);
        }

        private FileResult GetReport(DataTable dt)
        {
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "Receipt.rpt"));
            rd.SetDataSource(dt);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf");
        }

        private byte[] GenerateQrCode(string qrmsg)
        {

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrmsg, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            //imgBarCode.Height = 150;
            //imgBarCode.Width = 150;
            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    return byteImage;
                }
            }
        }

        public JsonResult printBarcode(GoodsRecivingatStoreModel OBJ)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string Implement = string.Empty;
            List<GoodsRecivingatStoreModel> bc = new List<GoodsRecivingatStoreModel>();
            try
            {
                if (string.IsNullOrEmpty(OBJ.PLANT) || string.IsNullOrEmpty(OBJ.FAMILY))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(OBJ.PACKING_STANDARD) || string.IsNullOrEmpty(OBJ.ORDERBY))
                {
                    msg = "Packing Std. & OrderBy should not be blank..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(OBJ.STORE_VERIFIED) && string.IsNullOrEmpty(OBJ.QTY_RECEIVED))
                {
                    msg = "QTY Received should not be blank..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                int QUANTITY = Convert.ToInt32(OBJ.QUANTITY);

                int j = 0, k;
                if (!string.IsNullOrEmpty(OBJ.STORE_VERIFIED))
                {
                    if (!int.TryParse(OBJ.QTY_RECEIVED, out j))
                    {
                        msg = "QTY Received should be number only..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var reult = new { Msg = msg, ID = mstType, validation = status };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                    if (j > QUANTITY || j < 1)
                    {

                        msg = "QTY Received should be less than or equal to QTY Ordered..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var reult = new { Msg = msg, ID = mstType, validation = status };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // quantity received equals to quantity ordered
                    j = QUANTITY;
                }



                if (!int.TryParse(OBJ.PACKING_STANDARD, out k))
                {
                    msg = "Packing Std. should be number only..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                if (!(k > 0))
                {
                    msg = "Packing Std. cannot be zero..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                string location = string.Empty, invoicedate = string.Empty, invoicenum = string.Empty, printedon = string.Empty,
                    itemRevision = string.Empty, bomRevision = string.Empty;

                query = string.Format(@"SELECT STORAGE FROM XXES_MRNINFO WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND ITEMCODE = '{2}'
                           AND MRN_NO = '{3}'", OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), OBJ.ITEMCODE.Trim().ToUpper(), OBJ.MRN_NO.Trim().ToUpper());
                location = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(location))
                {
                    //ORGID = fun.getOrgId(OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
                    //update location
                    location = GetStoreLoc(OBJ.ITEMCODE.Trim().ToUpper(), OBJ.PLANT.Trim());
                    query = string.Format(@"UPDATE XXES_MRNINFO SET STORAGE = '{0}' WHERE PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND ITEMCODE = '{3}'
                           AND MRN_NO = '{4}'", location, OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), OBJ.ITEMCODE.Trim().ToUpper(), OBJ.MRN_NO.Trim().ToUpper());
                    fun.EXEC_QUERY(query);
                }


                query = string.Format(@"SELECT M.ITEM_REVISION,M.BOM_REVISION,M.STORAGE,SUBSTR(M.INVOICE_DATE,1,10) INVOICE_DATE ,I.INVOICE_NO,TO_CHAR(I.PRINTED_ON,'DD-MM-YYYY hh24:mi') PRINTED_ON FROM XXES_MRNINFO M 
                                    JOIN ITEM_RECEIPT_DETIALS I
                                    ON M.MRN_NO = I.MRN_NO AND M.PLANT_CODE = I.PLANT_CODE AND M.FAMILY_CODE = I.FAMILY_CODE
                                    WHERE M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND M.MRN_NO = '{2}' AND M.ITEMCODE = '{3}'",
                                    OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), OBJ.MRN_NO.Trim(), OBJ.ITEMCODE.Trim());
                DataTable OUTPUT = fun.returnDataTable(query);

                if (OUTPUT.Rows.Count > 0)
                {
                    location = Convert.ToString(OUTPUT.Rows[0]["STORAGE"]);
                    invoicedate = Convert.ToString(OUTPUT.Rows[0]["INVOICE_DATE"]).Trim();
                    invoicenum = Convert.ToString(OUTPUT.Rows[0]["INVOICE_NO"]);
                    printedon = Convert.ToString(OUTPUT.Rows[0]["PRINTED_ON"]);
                    itemRevision = Convert.ToString(OUTPUT.Rows[0]["ITEM_REVISION"]);
                    bomRevision = Convert.ToString(OUTPUT.Rows[0]["BOM_REVISION"]);
                }


                try
                {
                    query = string.Format("SELECT implemented FROM XXES.XXES_BOM_BARCODE_ITEMS where item_code = '{0}'", OBJ.ITEMCODE.Trim());
                    string IsImplement = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(IsImplement) && Convert.ToInt16(IsImplement) == 0)
                        Implement = "ECO";
                    else
                        Implement = "";
                }
                catch (Exception ex)
                {
                    fun.LogWrite(ex);
                    Implement = "";
                }

                ORGID = fun.getOrgId(OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
                query = string.Format(@"SELECT CASE WHEN BUYER_NAME IS NULL THEN 'NA'
                                        WHEN UPPER(BUYER_NAME) LIKE '%RAKESH CHAUHAN%' THEN 'SOURCING'
                                        WHEN UPPER(BUYER_NAME) LIKE '%PAVAN VIR SINGH%' THEN 'NPD'
                                        ELSE
                                        'SCM'
                                        END AS BUYER_NAME
                                      FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{0}' 
                                    AND ITEM_CODE = '{1}' AND ROWNUM = 1",
                                    ORGID, OBJ.ITEMCODE.Trim());

                string buyer =  fun.get_Col_Value(query);


                int NoofBarcode = 0;
                if (j == k || j < k)
                {
                    NoofBarcode = 1;
                }
                else
                {
                    NoofBarcode = (j % k == 0) ? j / k : (j / k) + 1;
                }

                int bal_qty = 0;

                for (int i = 1; i <= NoofBarcode; i++)
                {
                    bal_qty = 0 + bal_qty;
                    if (i == 1)
                    {
                        int QtyReceived = 0;
                        if (j > k)
                        {
                            QtyReceived = k;
                            bal_qty = j - k;
                        }
                        else if (j == k)
                        {
                            QtyReceived = k;
                            bal_qty = j - k;
                        }
                        else if (j < k)
                        {
                            QtyReceived = j;
                            bal_qty = 0;
                        }
                        bc.Add(new GoodsRecivingatStoreModel
                        {
                            PLANT = OBJ.PLANT,
                            FAMILY = OBJ.FAMILY,
                            MRN_NO = OBJ.MRN_NO,
                            VENDOR_NAME = OBJ.VENDOR_NAME,
                            VENDOR_CODE = OBJ.VENDOR_CODE,
                            ITEMCODE = OBJ.ITEMCODE,
                            ITEM_DESCRIPTION = OBJ.ITEM_DESCRIPTION,
                            STATUS = OBJ.STATUS,
                            QTY_RECEIVED = QtyReceived.ToString(),
                            boxNo = i.ToString() + "/" + NoofBarcode,
                            ORDERBY = OBJ.ORDERBY,
                            LOCATION = location,
                            INVOICE_DATE = invoicedate,
                            INVOICE_NUMBER = invoicenum,
                            ITEM_REVISION = itemRevision,
                            BOM_REVISION = bomRevision,
                            IMPLEMENT = Implement,
                            PRINTED_ON = printedon,
                            BUYER_NAME = buyer
                        });
                    }
                    else
                    {
                        int QtyReceived = 0;
                        if (bal_qty > k)
                        {
                            QtyReceived = k;
                            bal_qty = bal_qty - k;
                        }
                        else if (bal_qty == k)
                        {

                            QtyReceived = k;
                            bal_qty = bal_qty - k;
                        }
                        else if (bal_qty < k)
                        {

                            QtyReceived = bal_qty;
                            bal_qty = 0;
                        }
                        bc.Add(new GoodsRecivingatStoreModel
                        {
                            PLANT = OBJ.PLANT,
                            FAMILY = OBJ.FAMILY,
                            MRN_NO = OBJ.MRN_NO,
                            VENDOR_NAME = OBJ.VENDOR_NAME,
                            VENDOR_CODE = OBJ.VENDOR_CODE,
                            ITEMCODE = OBJ.ITEMCODE,
                            ITEM_DESCRIPTION = OBJ.ITEM_DESCRIPTION,
                            STATUS = OBJ.STATUS,
                            QTY_RECEIVED = QtyReceived.ToString(),
                            boxNo = i.ToString() + "/" + NoofBarcode,
                            ORDERBY = OBJ.ORDERBY,
                            LOCATION = location,
                            INVOICE_DATE = invoicedate,
                            INVOICE_NUMBER = invoicenum,
                            ITEM_REVISION = itemRevision,
                            BOM_REVISION = bomRevision,
                            IMPLEMENT = Implement,
                            PRINTED_ON = printedon,
                            BUYER_NAME = buyer

                        });

                    }

                }

                PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();

                if (barcodes.PrintBox(bc))
                {
                    string TransactionType = string.Empty;
                    string qty = string.Empty;
                    if (string.IsNullOrEmpty(OBJ.STORE_VERIFIED))
                    {
                        TransactionType = "BEFORE_REC";
                        qty = OBJ.QUANTITY.Trim();
                    }
                    else
                    {
                        TransactionType = "AFTER_REC";
                        qty = OBJ.QTY_RECEIVED.Trim();
                    }

                    query = string.Format(@"INSERT INTO XXES_REPRINT_LABEL(PLANT_CODE,FAMILY_CODE,STAGE,LOGIN_USER,PRINT_DATE,
                                        ITEM_CODE,MRN,TRANSACTION_TYPE,PACKING_STD,QUANTITY)
                                            VALUES('{0}','{1}','{2}','{3}',SYSDATE,'{4}','{5}','{6}','{7}','{8}')",
                                            OBJ.PLANT.Trim(), OBJ.FAMILY.Trim(), Convert.ToString(Session["LoginStageCode"]),
                                            Convert.ToString(Session["Login_User"]), OBJ.ITEMCODE.Trim(), OBJ.MRN_NO.Trim(),
                                            TransactionType, OBJ.PACKING_STANDARD.Trim(), qty);
                    fun.EXEC_QUERY(query);

                    msg = "Printing Successfully";
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

        public JsonResult QualitySticker(GoodsRecivingatStoreModel OBJ)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty, IPADDR = string.Empty,
              IPPORT = string.Empty;
            int QUANTITY = Convert.ToInt32(OBJ.QUANTITY);

            List<BOXBARCODE> lstbarcodes = new List<BOXBARCODE>();

            try
            {
                if (string.IsNullOrEmpty(OBJ.PLANT) || string.IsNullOrEmpty(OBJ.FAMILY))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(OBJ.QTY_RECEIVED))
                {
                    msg = "QTY Received should not be blank..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                int j;
                if (!int.TryParse(OBJ.QTY_RECEIVED, out j))
                {
                    msg = "QTY Received should be number only..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                if (!(j > 0 && j <= QUANTITY))
                {
                    msg = "QTY Received should be less than or equal to QTY Ordered..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);

                }

                string line = fun.getPrinterIp("QUALITY", OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());
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

                //query = string.Format(@"select packing_standard from xxes_rawmaterial_master where plant_code = '{0}' and 
                //        family_code ='{1}' and item_code = '{2}'", OBJ.PLANT.ToUpper().Trim(),OBJ.FAMILY.ToUpper().Trim(),OBJ.ITEMCODE.Trim().ToUpper());
                //string PCKSTD = fun.get_Col_Value(query);
                //if (string.IsNullOrEmpty(PCKSTD))
                //{
                //    msg = "PACKING STANDARD IS NOT DEFINED";
                //    mstType = Validation.str1;
                //    status = Validation.str2;
                //    var resul = new { Msg = msg, ID = mstType, validation = status };
                //    return Json(resul, JsonRequestBehavior.AllowGet);

                //}
                //int NoofBarcode = 0;
                //int k = Convert.ToInt32(PCKSTD);
                //if (j == k || j < k)
                //{
                //    NoofBarcode = 1;
                //}
                //else
                //{
                //    NoofBarcode = (j % k == 0) ? j / k : (j / k) + 1;
                //}

                //int bal_qty = 0;

                //for (int i = 1; i <= NoofBarcode; i++)
                //{
                //    bal_qty = 0 + bal_qty;
                //    if (i == 1)
                //    {
                //        int QtyReceived = 0;
                //        if (j > k)
                //        {
                //            QtyReceived = k;
                //            bal_qty = j - k;
                //        }
                //        else if (j == k)
                //        {
                //            QtyReceived = k;
                //            bal_qty = j - k;
                //        }
                //        else if (j < k)
                //        {
                //            QtyReceived = j;
                //            bal_qty = 0;
                //        }
                //        lstbarcodes.Add(new BOXBARCODE
                //        {
                //            MRN = OBJ.MRN_NO,
                //            ITEMCODE = OBJ.ITEMCODE,
                //            PLANT = OBJ.PLANT,
                //            FAMILY = OBJ.FAMILY,
                //            RECQTY = Convert.ToString(QtyReceived),
                //            PRINTERIP = IPADDR,
                //            PRINTERPORT = IPPORT

                //        });
                //    }
                //    else
                //    {
                //        int QtyReceived = 0;
                //        if (bal_qty > k)
                //        {
                //            QtyReceived = k;
                //            bal_qty = bal_qty - k;
                //        }
                //        else if (bal_qty == k)
                //        {

                //            QtyReceived = k;
                //            bal_qty = bal_qty - k;
                //        }
                //        else if (bal_qty < k)
                //        {

                //            QtyReceived = bal_qty;
                //            bal_qty = 0;
                //        }
                //        lstbarcodes.Add(new BOXBARCODE
                //        {
                //            MRN = OBJ.MRN_NO,
                //            ITEMCODE = OBJ.ITEMCODE,
                //            PLANT = OBJ.PLANT,
                //            FAMILY = OBJ.FAMILY,
                //            RECQTY = Convert.ToString(QtyReceived),
                //            PRINTERIP = IPADDR,
                //            PRINTERPORT = IPPORT

                //        });

                //    }

                //}
                lstbarcodes.Add(new BOXBARCODE
                {
                    MRN = OBJ.MRN_NO.Trim(),
                    ITEMCODE = OBJ.ITEMCODE.Trim(),
                    PLANT = OBJ.PLANT.Trim(),
                    FAMILY = OBJ.FAMILY.Trim(),
                    RECQTY = OBJ.QTY_RECEIVED.Trim(),
                    PRINTERIP = IPADDR,
                    PRINTERPORT = IPPORT

                });

                if (fun.PrintingEnable("QUALITY", OBJ.PLANT.Trim(), OBJ.FAMILY.Trim()))
                {
                    PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                    if (barcodes.PrintMRNQualityBarcodes(lstbarcodes))
                    {
                        msg = "Printing Successfully";
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


        /// <summary>
        /// /below funtion by raj
        /// </summary>
        /// <param name="OBJ"></param>
        /// <returns></returns>
        public JsonResult MRNSrlnoPrint(GoodsRecivingatStoreModel OBJ)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty, IPADDR = string.Empty, IPPORT = string.Empty;
            int QUANTITY = Convert.ToInt32(OBJ.QUANTITY);

            List<GoodsRecivingatStoreModel> Mrnsrnoprint = new List<GoodsRecivingatStoreModel>();
            //string srno = Assemblyfunctions.getSeries(OBJ.PLANT, OBJ.FAMILY, "");
            try
            {
                if (string.IsNullOrEmpty(OBJ.PLANT) || string.IsNullOrEmpty(OBJ.FAMILY))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }


                if (OBJ.PrntRprnt == "RSN" && string.IsNullOrEmpty(OBJ.ReprintSrno))
                {
                    msg = "Please Enter Serial Number To Print..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }

                if (OBJ.PrntRprnt == "Print")
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_MRNSRNO  WHERE MRNNO = '{0}' and ITEMCODE='{1}' and plant_code='{2}' 
                    and family_code='{3}'", OBJ.MRN_NO.Trim(), OBJ.ITEMCODE, OBJ.PLANT, OBJ.FAMILY);

                    int mrn = Convert.ToInt32(fun.get_Col_Value(query));
                    if (mrn > 0)
                    {
                        msg = "Mrn Already Printed ..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var reult = new { Msg = msg, ID = mstType, validation = status };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //query = string.Format(@"SELECT MAX(TO_NUMBER(SUBSTR(SRNO,3))) + 1 AS FINAL  FROM XXES_MRNSRNO WHERE PLANT_CODE= '{0}' AND FAMILY_CODE = '{1}' ORDER BY TO_NUMBER(SUBSTR(SRNO,3))", OBJ.PLANT, OBJ.FAMILY);
                        AsmblyFun.ResetSerialNos(OBJ.PLANT, OBJ.FAMILY, "STORE", "RESET_MRNSRNO");

                        query = string.Format(@"select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='{0}'
                        and TYPE='DOMESTIC' and plant='{1}'", fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper(), OBJ.PLANT);

                        string Month_Code = fun.get_Col_Value(query);
                        int QTY = Convert.ToInt32(OBJ.QUANTITY);
                        for (int i = 0; i < QTY; i++)
                        {
                            string MrnSrno = AsmblyFun.getSeries(OBJ.PLANT, OBJ.FAMILY, "STORE");
                            //double srno = Convert.ToDouble(MrnSrno) + 1;
                            string srno = MrnSrno.Split('#')[1].Trim();
                            //MrnSrno = MrnSrno.Replace("#", "");
                            MrnSrno = Month_Code + srno;
                            string genratesrno = MrnSrno;
                            if (string.IsNullOrEmpty(MrnSrno))
                            {
                                msg = "Series not define";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var reult = new { Msg = msg, ID = mstType, validation = status };
                                return Json(reult, JsonRequestBehavior.AllowGet);
                            }
                            if (string.IsNullOrEmpty(MrnSrno.Split('#')[0]) || MrnSrno.Split('#')[0] == "0")
                            {
                                msg = "Series not define";
                                mstType = Validation.str1;
                                status = Validation.str2;
                                var reult = new { Msg = msg, ID = mstType, validation = status };
                                return Json(reult, JsonRequestBehavior.AllowGet);
                            }


                            query = string.Format(@"INSERT INTO XXES_MRNSRNO(PLANT_CODE,FAMILY_CODE,MRNNO,ITEMCODE,SRNO,DESCRIPTION,PRINTBY,TRANSACTION_DATE)
                                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", OBJ.PLANT, OBJ.FAMILY, OBJ.MRN_NO, OBJ.ITEMCODE, genratesrno,
                                    OBJ.ITEM_DESCRIPTION, Convert.ToString(Session["Login_User"]), Convert.ToDateTime(OBJ.TRANSACTION_DATE).ToString("dd MMM yyyy"));

                            if (fun.EXEC_QUERY(query))
                            {
                                query = string.Format(@"UPDATE XXES_FAMILY_SERIAL SET CURRENT_SERIAL_NUMBER='{0}' WHERE PLANT_CODE='{1}' AND FAMILY_CODE='{2}'
                                AND OFFLINE_KEYCODE='STORE'", srno, OBJ.PLANT, OBJ.FAMILY);

                                if (fun.EXEC_QUERY(query))
                                {
                                    Mrnsrnoprint.Add(new GoodsRecivingatStoreModel
                                    {
                                        PLANT = OBJ.PLANT,
                                        FAMILY = OBJ.FAMILY,
                                        ITEMCODE = OBJ.ITEMCODE,
                                        ITEM_DESCRIPTION = OBJ.ITEM_DESCRIPTION,
                                        MRN_NO = OBJ.MRN_NO,
                                        boxNo = genratesrno,
                                        TRANSACTION_DATE = Convert.ToDateTime(OBJ.TRANSACTION_DATE).ToString("dd MMM yyyy")

                                        //MRNSRNO_PRINTER_IP = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP"],
                                        //MRNSRNO_PRINTER_PORT = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_PORT"]

                                    });


                                }
                            }

                        }
                    }

                }

                if (OBJ.PrntRprnt == "Reprint" && !string.IsNullOrEmpty(OBJ.PSWORD))
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER WHERE AD_PASSWORD = '{0}' AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND OFFLINE_KEYCODE = 'STORE'"
                                , OBJ.PSWORD.Trim(), OBJ.PLANT.Trim(), OBJ.FAMILY.Trim());

                    int addusr = Convert.ToInt32(fun.get_Col_Value(query));

                    if (addusr > 0)
                    {
                        query = string.Format(@"SELECT PLANT_CODE, FAMILY_CODE, ITEMCODE, DESCRIPTION, MRNNO, SRNO, TRANSACTION_DATE  FROM XXES_MRNSRNO WHERE MRNNO = '{0}' and ITEMCODE='{1}' and plant_code='{2}' 
                        and family_code='{3}'", OBJ.MRN_NO.Trim(), OBJ.ITEMCODE, OBJ.PLANT, OBJ.FAMILY);
                        dt = fun.returnDataTable(query);

                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)

                                Mrnsrnoprint.Add(new GoodsRecivingatStoreModel
                                {
                                    PLANT = dt.Rows[i]["PLANT_CODE"].ToString(),
                                    FAMILY = dt.Rows[i]["FAMILY_CODE"].ToString(),
                                    ITEMCODE = dt.Rows[i]["ITEMCODE"].ToString(),
                                    ITEM_DESCRIPTION = dt.Rows[i]["DESCRIPTION"].ToString(),
                                    MRN_NO = dt.Rows[i]["MRNNO"].ToString(),
                                    boxNo = dt.Rows[i]["SRNO"].ToString(),
                                    TRANSACTION_DATE = Convert.ToDateTime(dt.Rows[i]["TRANSACTION_DATE"]).ToString("dd MMM yyyy")

                                    //MRNSRNO_PRINTER_IP = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP"],
                                    //MRNSRNO_PRINTER_PORT = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_PORT"]

                                });
                        }

                    }
                    else
                    {
                        msg = "Entered Wrong Password or Invalid..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var reult = new { Msg = msg, ID = mstType, validation = status };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                }
                //if (OBJ.PrntRprnt == "Reprint")
                //{
                //    query = string.Format(@"SELECT PLANT_CODE, FAMILY_CODE, ITEMCODE, DESCRIPTION, MRNNO, SRNO, TRANSACTION_DATE  FROM XXES_MRNSRNO WHERE MRNNO = '{0}' and ITEMCODE='{1}' and plant_code='{2}' 
                //    and family_code='{3}'", OBJ.MRN_NO.Trim(), OBJ.ITEMCODE, OBJ.PLANT, OBJ.FAMILY);
                //    dt = fun.returnDataTable(query);

                //    if (dt.Rows.Count > 0)
                //    {
                //        for (int i = 0; i < dt.Rows.Count; i++)

                //            Mrnsrnoprint.Add(new GoodsRecivingatStoreModel
                //            {
                //                PLANT = dt.Rows[i]["PLANT_CODE"].ToString(),
                //                FAMILY = dt.Rows[i]["FAMILY_CODE"].ToString(),
                //                ITEMCODE = dt.Rows[i]["ITEMCODE"].ToString(),
                //                ITEM_DESCRIPTION = dt.Rows[i]["DESCRIPTION"].ToString(),
                //                MRN_NO = dt.Rows[i]["MRNNO"].ToString(),
                //                boxNo = dt.Rows[i]["SRNO"].ToString(),
                //                TRANSACTION_DATE = Convert.ToDateTime(dt.Rows[i]["TRANSACTION_DATE"]).ToString("dd MMM yyyy")

                //                //MRNSRNO_PRINTER_IP = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP"],
                //                //MRNSRNO_PRINTER_PORT = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_PORT"]

                //            });
                //    }
                //}





                if (OBJ.PrntRprnt == "RSN" || !string.IsNullOrEmpty(OBJ.SerialNo))
                {
                    if (OBJ.PrntRprnt == "RSN")
                    {
                        query = string.Format(@"SELECT s.PLANT_CODE,s.FAMILY_CODE, s.ITEMCODE, s.DESCRIPTION, s.MRNNO, s.SRNO, r.TRANSACTION_DATE  FROM XXES_MRNSRNO s
                                JOIN  ITEM_RECEIPT_DETIALS r ON s.MRNNO=r.MRN_NO WHERE s.SRNO = '{0}'", OBJ.ReprintSrno);
                    }
                    else if (!string.IsNullOrEmpty(OBJ.SerialNo))
                    {
                        query = string.Format(@"SELECT s.PLANT_CODE,s.FAMILY_CODE, s.ITEMCODE, s.DESCRIPTION, s.MRNNO, s.SRNO, r.TRANSACTION_DATE  FROM XXES_MRNSRNO s
                                JOIN  ITEM_RECEIPT_DETIALS r ON s.MRNNO=r.MRN_NO WHERE s.SRNO = '{0}'", OBJ.SerialNo);
                    }

                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {

                        Mrnsrnoprint.Add(new GoodsRecivingatStoreModel
                        {
                            PLANT = dt.Rows[0]["PLANT_CODE"].ToString(),
                            FAMILY = dt.Rows[0]["FAMILY_CODE"].ToString(),
                            ITEMCODE = dt.Rows[0]["ITEMCODE"].ToString(),
                            ITEM_DESCRIPTION = dt.Rows[0]["DESCRIPTION"].ToString(),
                            MRN_NO = dt.Rows[0]["MRNNO"].ToString(),
                            boxNo = dt.Rows[0]["SRNO"].ToString(),
                            TRANSACTION_DATE = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd MMM yyyy")

                            //MRNSRNO_PRINTER_IP = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP"],
                            //MRNSRNO_PRINTER_PORT = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_PORT"]

                        });
                    }
                    else
                    {
                        msg = "Invalid Serial NO Not Found..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var reult = new { Msg = msg, ID = mstType, validation = status };
                        return Json(reult, JsonRequestBehavior.AllowGet);
                    }
                }

                if (Mrnsrnoprint.Count > 0)
                {
                    if (fun.PrintingEnable("STORE", OBJ.PLANT.Trim(), OBJ.FAMILY.Trim()))
                    //if(ConfigurationManager.AppSettings["MRNSRNO"] == "Y")
                    {
                        PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                        if (barcodes.PrintMrnSrno(Mrnsrnoprint))
                        {
                            msg = "Printing Successfully";
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
                    else
                    {
                        msg = "Printing Not Enable For Selected Plant & Family..!";
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