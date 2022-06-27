using EncodeDecode;
using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MVCApp.Controllers
{
    public class MAHController : ApiController
    {
        public static string orConnstring = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
        string query = string.Empty;
        BaseEncDec bed = new BaseEncDec();
        Assemblyfunctions af = null;
        PrintAssemblyBarcodes printAssemblyBarcodes = null;
        Function fun = new Function();
        [HttpGet]

        public string GetMrnInfo(string Mrn)
        {
            try
            {

                query = string.Format(@"select count(*) from item_receipt_detials where MRN_NO='{0}' and timein is null",
                    Mrn.Trim());
                if (fun.CheckExits(query))
                {
                    return "ERROR: MRN NOT SCANNED AT GATE";
                }
                query = string.Format(@"SELECT NVL(SUM(xm.QUANTITY),0) QUANTITY 
                  FROM XXES_MRNINFO xm
                  inner join item_receipt_detials ir
                  on xm.mrn_no = ir.mrn_no AND ir.TIMEIN IS NOT NULL AND ir.IN_BY IS NOT NULL and ir.plant_code = xm.plant_code
                  and ir.family_code = xm.family_code
                  WHERE XM.MRN_NO = '{0}'", Mrn);
                string mrnqty = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(mrnqty))
                    mrnqty = "0";
                query = string.Format(@"SELECT nvl(SUM(quantity),0) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}' GROUP BY MRN", Mrn);
                string recqty = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(recqty))
                    recqty = "0";
                if (mrnqty == recqty)
                {
                    return "REFRESH # COMPLETE QUANTITY RECVD " + mrnqty;
                }

                string constant = "\"";
                query = string.Format(@"
                SELECT xm.ITEMCODE, (CASE WHEN LENGTH(XM.ITEM_DESCRIPTION) >16 
                THEN  REPLACE(SUBSTR(xm.ITEM_DESCRIPTION,1,15),'{1}','')
                ELSE
              REPLACE(xm.ITEM_DESCRIPTION, '{1}', '')
                end
                )
                  ITEM_DESCRIPTION ,NVL(SUM(xm.QUANTITY),0) QUANTITY ,
                  NVL(SUM(xm.REC_QTY),0) RCVD_QTY,xm.STATUS
                  FROM XXES_MRNINFO xm 
                  inner join item_receipt_detials ir
                  on xm.mrn_no = ir.mrn_no AND ir.TIMEIN IS NOT NULL AND ir.IN_BY IS NOT NULL and ir.plant_code = xm.plant_code 
                  and ir.family_code = xm.family_code
                  WHERE XM.MRN_NO='{0}'
                  AND nvl(QUANTITY,0)>
                nvl((SELECT SUM(quantity) FROM XXES_VERIFYSTOREMRN WHERE MRN = xm.mrn_no AND ITEMCODE = xm.itemcode),0)
                group BY xm.mrn_no, xm.ITEMCODE,xm.ITEM_DESCRIPTION,XM.STATUS", Mrn, constant);
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }

        public string GetsupermarketLocInfo(string Location)
        {
            try
            {

                //query = string.Format(@"SELECT COUNT(*) FROM XXES_Users_Master WHERE LOCATION_CODE = {0}",
                //    Location.Trim());
                //if (fun.CheckExits(query))
                //{
                //    return "ERROR: LOCATION NOT FOUNT SUPER MARCKET";
                //}
                query = string.Format(@"SELECT distinct sms.*,ird.ITEM_DESCRIPTION FROM XXES_SUMMKTSTOCK sms 
INNER JOIN ITEM_RECEIPT_DETIALS ird ON sms.ITEMCODE= ird.ITEM_CODE
WHERE SMS.LOCATION_CODE='{0}'", Location);

                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }
        public string GetBarcodeInfo(string Mrn)
        {
            string result = "";
            try
            {

                //query = string.Format(@"select count(*) from XXES_RECEIPTBARCODES where MRN_NO='{0}'",
                //    Mrn.Trim());
                //if (fun.CheckExits(query))
                //{
                //    return "ERROR: MRN NOT Found";
                //}
                string constant = "\"";
                query = string.Format(@"SELECT (SELECT COUNT(*) FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}') AS TotalBarcodes,(SELECT COUNT(*) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}') AS PrintBarcode,((SELECT COUNT(*) FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}') - (SELECT COUNT(*) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}')) AS PendingBarcode FROM DUAL", Mrn);
                DataTable dt = returnDataTable(query);
                result += "Total : " + dt.Rows[0]["TotalBarcodes"].ToString() + ",";
                result += "Printed : " + dt.Rows[0]["PrintBarcode"].ToString() + ",";
                result += "Pending : " + dt.Rows[0]["PendingBarcode"].ToString();
                return result;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }
        public string GetRECEIPTBARCODESInfo(string Mrn)
        {
            try
            {

                //query = string.Format(@"select count(*) from XXES_RECEIPTBARCODES where MRN_NO='{0}'",
                //    Mrn.Trim());
                //if (fun.CheckExits(query))
                //{
                //    return "ERROR: MRN NOT Found";
                //}
                string constant = "\"";
                query = string.Format(@"SELECT ITEMCODE,QTY,BOX_NO, NVL(QTY_RECEIVED, 0) AS QTY_RECEIVED  FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}' AND QR_CODE NOT IN (SELECT BARCODE FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}') ORDER BY AUTOID", Mrn);
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }
        //public string GetBarcodeInfo(string Mrn)
        //{
        //    string result = "";
        //    try
        //    {

        //        //query = string.Format(@"select count(*) from XXES_RECEIPTBARCODES where MRN_NO='{0}'",
        //        //    Mrn.Trim());
        //        //if (fun.CheckExits(query))
        //        //{
        //        //    return "ERROR: MRN NOT Found";
        //        //}
        //        string constant = "\"";
        //        query = string.Format(@"SELECT (SELECT COUNT(*) FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}') AS TotalBarcodes,(SELECT COUNT(*) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}') AS PrintBarcode,((SELECT COUNT(*) FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}') - (SELECT COUNT(*) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}')) AS PendingBarcode FROM DUAL", Mrn);
        //        return JsonConvert.SerializeObject(returnDataTable(query));
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        return JsonConvert.SerializeObject("ERROR:" + ex.Message);
        //    }
        //}
        //public string GetRECEIPTBARCODESInfo(string Mrn)
        //{
        //    try
        //    {

        //        //query = string.Format(@"select count(*) from XXES_RECEIPTBARCODES where MRN_NO='{0}'",
        //        //    Mrn.Trim());
        //        //if (fun.CheckExits(query))
        //        //{
        //        //    return "ERROR: MRN NOT Found";
        //        //}
        //        string constant = "\"";
        //        query = string.Format(@"SELECT ITEMCODE,QTY,BOX_NO FROM XXES_RECEIPTBARCODES WHERE MRN_NO='{0}' AND QR_CODE NOT IN (SELECT BARCODE FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}') ORDER BY AUTOID", Mrn);
        //        return JsonConvert.SerializeObject(returnDataTable(query));
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        return JsonConvert.SerializeObject("ERROR:" + ex.Message);
        //    }
        //}
        public string GetQrCodesInfo(string Mrn)
        {
            try
            {


                query = string.Format(@"SELECT NVL(SUM(xm.QUANTITY),0) QUANTITY 
                  FROM XXES_MRNINFO xm
                  inner join item_receipt_detials ir
                  on xm.mrn_no = ir.mrn_no AND ir.TIMEIN IS NOT NULL AND ir.IN_BY IS NOT NULL and ir.plant_code = xm.plant_code
                  and ir.family_code = xm.family_code
                  WHERE XM.MRN_NO = '{0}'", Mrn);
                string mrnqty = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(mrnqty))
                    mrnqty = "0";
                query = string.Format(@"SELECT nvl(SUM(quantity),0) FROM XXES_VERIFYSTOREMRN WHERE MRN='{0}' GROUP BY MRN", Mrn);
                string recqty = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(recqty))
                    recqty = "0";
                if (mrnqty == recqty)
                {
                    return "REFRESH # COMPLETE QUANTITY RECVD " + mrnqty;
                }

                string constant = "\"";
                query = string.Format(@"
                SELECT xm.ITEMCODE, (CASE WHEN LENGTH(XM.ITEM_DESCRIPTION) >16 
                THEN  REPLACE(SUBSTR(xm.ITEM_DESCRIPTION,1,15),'{1}','')
                ELSE
              REPLACE(xm.ITEM_DESCRIPTION, '{1}', '')
                end
                )
                  ITEM_DESCRIPTION ,NVL(SUM(xm.QUANTITY),0) QUANTITY ,
                  NVL(SUM(xm.REC_QTY),0) RCVD_QTY,xm.STATUS
                  FROM XXES_MRNINFO xm 
                  inner join item_receipt_detials ir
                  on xm.mrn_no = ir.mrn_no AND ir.TIMEIN IS NOT NULL AND ir.IN_BY IS NOT NULL and ir.plant_code = xm.plant_code 
                  and ir.family_code = xm.family_code
                  WHERE XM.MRN_NO='{0}'
                  AND nvl(QUANTITY,0)>
                nvl((SELECT SUM(quantity) FROM XXES_VERIFYSTOREMRN WHERE MRN = xm.mrn_no AND ITEMCODE = xm.itemcode),0)
                group BY xm.mrn_no, xm.ITEMCODE,xm.ITEM_DESCRIPTION,XM.STATUS", Mrn, constant);
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }
        public DataTable GetITEMDETAILS(string MRN, string ITEMCODE, string PLANT, string FAMILY)
        {
            DataTable DT_ALLMRN = new DataTable();
            try
            {
                query = string.Format(@"SELECT M.PLANT_CODE,M.FAMILY_CODE, M.MRN_NO, M.ITEMCODE ITEM_CODE, M.ITEM_DESCRIPTION ITEM_DESC, I.VENDOR_CODE, 
                                CASE WHEN I.VENDOR_NAME IS NULL THEN 'ESCORTS LTD' ELSE  SUBSTR(I.VENDOR_NAME,0,'50') END AS SUPP_NAME, M.QUANTITY QTY_ORD, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE,
                                R.PACKING_STANDARD,to_char(SYSDATE, 'dd-Mon-yyyy' ) CURRENT_DATE,U.U_NAME,M.PUNAME, to_char( I.TRANSACTION_DATE, 'dd-Mon-yyyy') as  TRANSACTION_DATE,
                                M.REC_QTY AS QTY_DLV,B.LOCATION_CODE BULK_LOC,b.packaging_type BPACKAGING,b.bulk_storage_snp BULK_SNP,
                                CASE WHEN (b.unpacked IS NULL OR b.unpacked = 'N') THEN 'N' ELSE 'Y' END UNPACKED,STORE_VERIFIED,M.STATUS
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
                                MRN.Trim(), ITEMCODE.Trim(), PLANT.Trim(), FAMILY.Trim());
                return fun.returnDataTable(query);


            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public void printMrn(COMMONDATA cOMMONDATA)
        {
            if (Convert.ToString(ConfigurationManager.AppSettings["PRINT_MRN_BULKITEM"]) != "Y")
                return;
            string line = fun.getPrinterIp("QUALITY", cOMMONDATA.PLANT.ToUpper().Trim(),
                cOMMONDATA.FAMILY.ToUpper().Trim());
            if (string.IsNullOrEmpty(line))
            {
                fun.LogWrite(new Exception("QUALITY IP ADDRESS NOT FOUND"));
                return;

            }
            string IPADDR = line.Split('#')[0].Trim();
            string IPPORT = line.Split('#')[1].Trim();

            string MRN_NO = cOMMONDATA.DATA.Split(',')[0].Trim();
            DataTable dt = new DataTable();
            List<BARCODEPRINT> mainbarcodeList = new List<BARCODEPRINT>();
            List<BOXBARCODE> bOXBARCODEs = new List<BOXBARCODE>();
            int NoBox = 0;
            try
            {
                //221001013048
                query = string.Format(@"select distinct M.ITEMCODE from XXES_MRNINFO M where m.mrn_no='{0}' 
                and m.plant_code='{1}' and m.family_code='{2}'", MRN_NO, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                using (DataTable dataTable = fun.returnDataTable(query))
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {

                            dt = GetITEMDETAILS(MRN_NO, Convert.ToString(dataRow["ITEMCODE"]).Trim(), cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                            if (dt.Rows.Count > 0)
                            {

                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["PACKING_STANDARD"])) ||
                                    Convert.ToString(dt.Rows[0]["PACKING_STANDARD"]) == "0")
                                    return;


                                string plant = dt.Rows[0]["PLANT_CODE"].ToString();
                                string PO = "PO";
                                string ItemCode = dt.Rows[0]["ITEM_CODE"].ToString();

                                string BULKSTORAGE = dt.Rows[0]["BULK_LOC"].ToString();
                                string LINE = "LINE";
                                string supplier = dt.Rows[0]["SUPP_NAME"].ToString();
                                string IF = "IF";
                                string transactionDate = Convert.ToDateTime(dt.Rows[0]["TRANSACTION_DATE"]).ToString("dd-MM-yyyy").Replace("-", "");

                                string qrBeforeQty_recAndBox = plant + "$" + PO + "$" + ItemCode + "$";
                                string qrAfterQty_rec = "$" + BULKSTORAGE + "$" + LINE + "$" + supplier + "$" + IF + "$" + transactionDate + "$";


                                dt.Columns.Add("BOX_NO", typeof(string));
                                dt.Columns.Add("QR_CODE", typeof(string));
                                DataRow dr = null;
                                int PACKING_STD = Convert.ToInt32(dt.Rows[0]["PACKING_STANDARD"]);
                                int QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]);
                                int bal_qty = 0;
                                float a = 0;
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STORE_VERIFIED"])))
                                {
                                    QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]);
                                    NoBox = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]) / PACKING_STD;
                                }
                                else
                                {
                                    QTY_RECD = Convert.ToInt32(dt.Rows[0]["QTY_DLV"]);
                                    NoBox = Convert.ToInt32(dt.Rows[0]["QTY_DLV"]) / PACKING_STD;
                                }
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["STORE_VERIFIED"])))
                                    a = Convert.ToInt32(dt.Rows[0]["QTY_ORD"]) % PACKING_STD;
                                else
                                    a = Convert.ToInt32(dt.Rows[0]["QTY_DLV"]) % PACKING_STD;
                                if (Convert.ToString(dt.Rows[0]["STATUS"]) == "QA")
                                {
                                    bOXBARCODEs.Add(new BOXBARCODE()
                                    {
                                        MRN = MRN_NO.Trim(),
                                        ITEMCODE = Convert.ToString(dataRow["ITEMCODE"]),
                                        RECQTY = Convert.ToString(QTY_RECD),
                                        PRINTERIP = IPADDR,
                                        PRINTERPORT = IPPORT
                                    });
                                }
                                if (a > 0)
                                    NoBox = NoBox + 1;
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
                                        string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + MRN_NO;
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
                                        string BarcodeMsg = qrBeforeQty_recAndBox + QtyReceived + qrAfterQty_rec + BOX + "$" + MRN_NO;
                                        dr["QR_CODE"] = BarcodeMsg;
                                        dt.Rows.Add(dr);
                                    }

                                }
                            }


                            List<BARCODEPRINT> barcodeList = (from DataRow dr in dt.Rows
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
                            mainbarcodeList.AddRange(barcodeList);
                        }
                    }
                }
                PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                if (InsertIntoRECEIPTBARCODES(mainbarcodeList) == true)
                {
                    if (barcodes.PrintBoxs(mainbarcodeList))
                    {
                        //msg = "Barcode Printing Successfully";
                        //mstType = Validation.str;
                        //status = Validation.stus;
                    }
                    else
                    {
                        //msg = "Error in printing";
                        //mstType = Validation.str1;
                        //status = Validation.str2;
                    }
                }
                if (fun.PrintingEnable("QUALITY",
                     cOMMONDATA.PLANT.ToUpper().Trim(),
                     cOMMONDATA.FAMILY.ToUpper().Trim()
                     ))
                {
                    barcodes.PrintMRNQualityBarcodes(bOXBARCODEs);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool InsertIntoRECEIPTBARCODES(List<BARCODEPRINT> bOXBARCODEs)
        {
            bool status = false;
            string errModule = "";
            try
            {
                foreach (var item in bOXBARCODEs)
                {
                    string Checkexits = string.Format(@"SELECT count(*) FROM XXES_RECEIPTBARCODES WHERE QR_CODE='{0}'",
                   item.QR_CODE);
                    if (!CheckExitsOra(Checkexits))
                    {
                        string query = string.Format(@"Insert into XXES_RECEIPTBARCODES(PLANT_CODE,FAMILY_CODE,MRN_NO,ITEMCODE,QR_CODE,CREATEDBY,CREATEDDATE,Qty,BOX_NO)
                                                    values('{0}','{1}','{2}','{3}','{4}','',sysdate,'{5}','{6}')", item.PLANT.Trim().ToUpper(), item.FAMILY.Trim().ToUpper(), item.MRN_NO.Trim().ToUpper(), fun.replaceApostophi(item.ITEMCODE.Trim().ToUpper()), item.QR_CODE, item.QTY_ORD.Trim(), item.BOX_NO.Trim());
                        fun.EXEC_QUERY(query);
                        errModule = "";
                    }

                }

            }
            catch
            {
                return false;
            }
            finally { }
            return true;
        }


        [HttpPost]

        public HttpResponseMessage UpdateSuperMktQtyLocation(SUPERMKTQTYUPDATE sUPERMKTQTYUPDATE)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {

                string ItemlocCheckexits = string.Format(@"SELECT count(*) FROM XXES_SUMMKTSTOCK WHERE ITEMCODE='{0}' And LOCATION_CODE='{1}'",
               sUPERMKTQTYUPDATE.ITEMCODE, sUPERMKTQTYUPDATE.SECONDLOCATION);
                if (!CheckExitsOra(ItemlocCheckexits))
                {
                    response = "Location Or Item Not Found In Super Market";
                }
                else
                {
                    string upateqtylocby = string.Format(@"UPDATE XXES_SUMMKTSTOCK SET QUANTITY = (QUANTITY + "+ sUPERMKTQTYUPDATE .UPDATEQUANTITY + ") WHERE LOCATION_CODE = '{0}' And ITEMCODE='{1}'",
                        sUPERMKTQTYUPDATE.SECONDLOCATION, sUPERMKTQTYUPDATE.ITEMCODE);
                    fun.EXEC_QUERY(upateqtylocby);
                    string minqtyloc = string.Format(@"UPDATE XXES_SUMMKTSTOCK SET QUANTITY = (QUANTITY - " + sUPERMKTQTYUPDATE.UPDATEQUANTITY + ") WHERE LOCATION_CODE = '{0}' And ITEMCODE='{1}'",
                        sUPERMKTQTYUPDATE.FIRSTLOCATION, sUPERMKTQTYUPDATE.ITEMCODE);
                    fun.EXEC_QUERY(minqtyloc);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                };

            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }
        [HttpPost]

        public HttpResponseMessage UpdateSuperMktLocationNew(SUPERMKTSTORGAE sUPERMKTSTORGAE)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                if (sUPERMKTSTORGAE.LOCATION.Contains("$"))
                    sUPERMKTSTORGAE.LOCATION = sUPERMKTSTORGAE.LOCATION.Split('$')[1].Trim();

                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_SUPERMARKETITEMS", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("S_PLANT_CODE", sUPERMKTSTORGAE.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("S_FAMILY_CODE", sUPERMKTSTORGAE.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("S_SUMKTLOC", sUPERMKTSTORGAE.LOCATION.Trim().ToUpper());
                    //comm.Parameters.Add("S_ITEMCODE", sUPERMKTSTORGAE.ITEMCODE.Trim().ToUpper());
                    //comm.Parameters.Add("S_QUANTITY", sUPERMKTSTORGAE.QUANTITY.Trim().ToUpper());
                    comm.Parameters.Add("S_KANBAN_NO", sUPERMKTSTORGAE.KANBAN.Trim().ToUpper());
                    comm.Parameters.Add("S_CREATEDBY", sUPERMKTSTORGAE.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK";
                        }
                        else
                        {
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                };

            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }
        [HttpPost]

        public HttpResponseMessage UpdateSuperMktLocation(SUPERMKTSTORGAE sUPERMKTSTORGAE)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                if (sUPERMKTSTORGAE.LOCATION.Contains("$"))
                    sUPERMKTSTORGAE.LOCATION = sUPERMKTSTORGAE.LOCATION.Split('$')[1].Trim();

                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_SUPERMARKETITEMS", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("S_PLANT_CODE", sUPERMKTSTORGAE.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("S_FAMILY_CODE", sUPERMKTSTORGAE.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("S_SUMKTLOC", sUPERMKTSTORGAE.LOCATION.Trim().ToUpper());
                    //comm.Parameters.Add("S_ITEMCODE", sUPERMKTSTORGAE.ITEMCODE.Trim().ToUpper());
                    //comm.Parameters.Add("S_QUANTITY", sUPERMKTSTORGAE.QUANTITY.Trim().ToUpper());
                    comm.Parameters.Add("S_KANBAN_NO", sUPERMKTSTORGAE.KANBAN.Trim().ToUpper());
                    comm.Parameters.Add("S_CREATEDBY", sUPERMKTSTORGAE.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK";
                        }
                        else
                        {
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                };

            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public string UpdateBulkLocation(BULKSTORGAE bULKSTORGAE)
        {
            try
            {
                string capacity = string.Empty, existingqty = string.Empty, Code = string.Empty, LocCode = string.Empty;
                double comingqty = 0;
                if (!string.IsNullOrEmpty(bULKSTORGAE.AVAIL_LOC) && bULKSTORGAE.AVAIL_LOC != "TEMPORARY")
                {
                    if (bULKSTORGAE.LOCATION.Trim().ToUpper() != bULKSTORGAE.AVAIL_LOC.Trim().ToUpper())
                    {
                        return "ERROR : ITEM MUST BE PLACE ON AVAILABLE LOCATION";
                    }
                }
                SplitItemBarcode splitItem = SplitItemQrcode(bULKSTORGAE.QRCODE);
                if (string.IsNullOrEmpty(splitItem.PKGQTY) || splitItem.PKGQTY == "0")
                    return "ERROR: INVALID QUANTITY";
                float f;
                if (!float.TryParse(splitItem.PKGQTY, out f))
                {
                    return "ERROR: INVALID QUANTITY";
                }
                if (bULKSTORGAE.LOCATION.Contains("$"))
                    bULKSTORGAE.LOCATION = bULKSTORGAE.LOCATION.Split('$')[1].Trim();
                if (bULKSTORGAE.AVAIL_LOC.Trim().ToUpper() == "TEMPORARY")
                {
                    query = string.Format(@"SELECT count(*) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}' and NVL(TEMP_LOC,'')='Y'  ",
                    bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, bULKSTORGAE.LOCATION);
                    if (!CheckExitsOra(query))
                    {
                        return "ERROR: LOCATION FOR ITEMCODE " + splitItem.ITEMCODE + " SHOULD BE TEMPORARY";
                    }
                    query = string.Format(@"SELECT nvl(capacity,0) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}' ",
               bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, bULKSTORGAE.LOCATION);
                    capacity = GetColValueOra(query);
                    if (string.IsNullOrEmpty(capacity))
                        capacity = "0";

                    //query = string.Format(@"SELECT ITEMCODE || '#' || LOCATION_CODE  FROM XXES_BULKSTORAGEITEMS S WHERE 
                    //         S.PLANT_CODE='{0}' AND S.FAMILY_CODE='{1}' AND S.LOCATION_CODE='{2}'", bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, bULKSTORGAE.LOCATION);
                    //string line = string.Empty;
                    //line = GetColValueOra(query);
                    //Code = line.Split('#')[0].Trim().ToUpper();
                    //LocCode = line.Split('#')[1].Trim().ToUpper();
                    //if (Code != splitItem.ITEMCODE.Trim().ToUpper())
                    //{
                    //    return "ERROR : ITEMCODE MISMATCH FOR TEMP LOCATION" + Code;
                    //}

                    query = string.Format(@"SELECT nvl(QUANTITY,0) FROM XXES_BULKSTORAGEITEMS s WHERE s.plant_code='{0}' AND s.family_code='{1}' and itemcode='{2}' and location_code='{3}' ",
                     bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                    existingqty = GetColValueOra(query);
                    if (string.IsNullOrEmpty(existingqty))
                        existingqty = "0";

                    comingqty = Convert.ToDouble(splitItem.PKGQTY) + Convert.ToDouble(existingqty);
                    if (comingqty > Convert.ToDouble(capacity))
                    {
                        //return "ERROR : EXISTING QTY FOUND " + existingqty + ", BARCODE QTY " + splitItem.PKGQTY + ", TOTAL = " + comingqty + " WHICH IS EXCEEDING CAPACITY " + capacity;
                        return "ERROR : CAPACITY FINISHED " + bULKSTORGAE.LOCATION;
                    }

                }

                else
                {
                    if (bULKSTORGAE.LOCATION.Trim().ToUpper() != bULKSTORGAE.AVAIL_LOC.Trim().ToUpper())
                    {
                        return "ERROR : LOCATION MISMATCH";
                    }
                    query = string.Format(@"SELECT count(*) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and item_code='{2}' and location_code='{3}' ",
                    bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                    if (!CheckExitsOra(query))
                    {
                        return "ERROR: INVALID LOCATION";// + splitItem.ITEMCODE;
                    }
                    query = string.Format(@"SELECT nvl(capacity,0) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and item_code='{2}' and location_code='{3}' ",
                 bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                    capacity = GetColValueOra(query);
                    if (string.IsNullOrEmpty(capacity))
                        capacity = "0";

                    query = string.Format(@"SELECT nvl(QUANTITY,0) FROM XXES_BULKSTORAGEITEMS s WHERE s.plant_code='{0}' AND s.family_code='{1}' and itemcode='{2}' and location_code='{3}' ",
                     bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                    existingqty = GetColValueOra(query);
                    if (string.IsNullOrEmpty(existingqty))
                        existingqty = "0";

                    comingqty = Convert.ToDouble(splitItem.PKGQTY) + Convert.ToDouble(existingqty);
                    if (comingqty > Convert.ToDouble(capacity))
                    {
                        //return "ERROR : EXISTING QTY FOUND " + existingqty + ", BARCODE QTY " + splitItem.PKGQTY + ", TOTAL = " + comingqty + " WHICH IS EXCEEDING CAPACITY " + capacity;
                        return "ERROR : CAPACITY FINISHED " + bULKSTORGAE.LOCATION;
                    }

                }
                #region qc check first
                query = string.Format(@"SELECT MRN FROM  XXES_VERIFYSTOREMRN WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' 
                        AND ITEMCODE = '{2}' AND BARCODE = '{3}'", bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.QRCODE);
                string MRN = GetColValueOra(query);
                if (string.IsNullOrEmpty(MRN))
                {
                    return "ERROR : MRN NOT FOUND IN QR CODE";
                }
                else
                {
                    query = string.Format(@"SELECT STATUS, QUALITY_OK FROM XXES_MRNINFO WHERE PLANT_CODE = '{0}' AND 
                                            FAMILY_CODE = '{1}' AND ITEMCODE = '{2}' AND MRN_NO = '{3}'",
                            bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, MRN.Trim());
                    DataTable dt = returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        string STATUS = Convert.ToString(dt.Rows[0]["STATUS"]);
                        if (!string.IsNullOrEmpty(STATUS) && STATUS == "QA")
                        {
                            string QUALITY_OK = Convert.ToString(dt.Rows[0]["QUALITY_OK"]);
                            if (string.IsNullOrEmpty(QUALITY_OK))
                            {
                                return "ERROR : QC NOT DONE IN QR CODE";
                            }
                        }
                    }

                }

                #endregion qc check first
                //query = string.Format(@"delete from XXES_BULKSTORAGEITEMS where nvl(quantity,0)=0");
                //ExecQueryOra(query);

                query = string.Format(@"INSERT INTO XXES_BULKSTORAGEITEMS(PLANT_CODE,FAMILY_CODE,LOCATION_CODE,ITEMCODE,QUANTITY,BARCODE,CREATEDBY,CREATEDDATE)
                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',sysdate)", bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, bULKSTORGAE.LOCATION, splitItem.ITEMCODE,
                splitItem.PKGQTY, bULKSTORGAE.QRCODE, bULKSTORGAE.CREATEDBY);
                if (ExecQueryOra(query))
                {
                    return "OK";
                }
                return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }

        [HttpPost]
        public string UpdateBulkLocationNew(BULKSTORGAE bULKSTORGAE)
        {
            string response = string.Empty;
            try
            {
                SplitItemBarcode splitItem = SplitItemQrcode(bULKSTORGAE.QRCODE);
                if (string.IsNullOrEmpty(splitItem.PKGQTY) || splitItem.PKGQTY == "0")
                    return "ERROR: INVALID QUANTITY";
                float f;
                if (!float.TryParse(splitItem.PKGQTY, out f))
                {
                    return "ERROR: INVALID QUANTITY";
                }
                if (bULKSTORGAE.LOCATION.Contains("$"))
                    bULKSTORGAE.LOCATION = bULKSTORGAE.LOCATION.Split('$')[1].Trim();
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_UPDATEBULKLOCATION", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("B_PLANT_CODE", bULKSTORGAE.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("B_FAMILY_CODE", bULKSTORGAE.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("B_LOCATION_CODE", bULKSTORGAE.LOCATION.Trim().ToUpper());
                    comm.Parameters.Add("B_AVAIL_LOC", bULKSTORGAE.AVAIL_LOC.Trim().ToUpper());
                    comm.Parameters.Add("B_ITEMCODE", splitItem.ITEMCODE.Trim().ToUpper());
                    comm.Parameters.Add("B_PKGQTY", splitItem.PKGQTY);
                    comm.Parameters.Add("B_BARCODE", bULKSTORGAE.QRCODE);
                    comm.Parameters.Add("B_CREATEDBY", bULKSTORGAE.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MASSEGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MASSEGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MASSEGE"].Value);
                    oracleConnection.Close();
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return response;
        }

        [HttpPost]
        public string GetBulkStorageItemsForKAnban(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (cOMMONDATA.DATA.Contains("$"))
                    cOMMONDATA.DATA = cOMMONDATA.DATA.Split('$')[1].Trim();

                query = string.Format(@"select count(*) from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and LOCATION_CODE='{2}' and createdby='{3}'",
                           cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.DATA, cOMMONDATA.CREATEDBY);
                if (!CheckExitsOra(query))
                {
                    return "INVALID PICKLIST LOCATION FOR USER " + cOMMONDATA.CREATEDBY;
                }

                query = string.Format(@"select nvl(QUANTITY,0) from XXES_BULKSTOCK where plant_code='{0}' and family_code='{1}'
                            and LOCATION_CODE='{2}'",
                            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.DATA);

                //query = string.Format(@"select nvl(sum(QUANTITY),0) from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                //            and status='PENDING' and LOCATION_CODE='{2}' and createdby='{3}'",
                //            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.DATA, cOMMONDATA.CREATEDBY);

                response = GetColValueOra(query);
                if (!string.IsNullOrEmpty(response))
                {
                    response = "OK#" + response;
                }
                else
                {
                    response = "OK#0";
                }
            }
            catch (Exception ex)
            {
                response = "ERROR:" + ex.Message;
            }
            return response;
        }
        [HttpPost]
        public string GetPendingKanban(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            string response = string.Empty;
            try
            {
                query = string.Format(@"select KANBANNO KANBAN,LOCATION_CODE,ITEMCODE,QUANTITY from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and createdby='{2}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.CREATEDBY
                    );
                dataTable = returnDataTable(query);
                response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {

                dataTable = errorTable(ex.Message);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return response;
        }
        //[HttpPost]
        //public string GetBulkStorageLocation(COMMONDATA cOMMONDATA)
        //{
        //    DataTable dataTable = new DataTable();
        //    bool FLAG = false;
        //    string response = string.Empty, existingqty = "0";
        //    try
        //    {
        //        SplitItemBarcode splitItemBarcode = SplitItemQrcode(cOMMONDATA.DATA);
        //        if (string.IsNullOrEmpty(splitItemBarcode.BOX) || (!splitItemBarcode.BOX.Contains("/")))
        //        {
        //            return "ERROR: BOX NO NOT FOUND SO RE-PRINT PACKING SLIP";
        //        }
        //        query = string.Format(@"select count(*) from XXES_BULKSTORAGEITEMS where barcode='{0}' and plant_code='{1}' and family_code='{2}'",
        //           cOMMONDATA.DATA, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
        //        if (CheckExitsOra(query))
        //        {
        //            return "ERROR: BARCODE ALREADY SCANNED";
        //        }
        //        #region CHECK IS QR CODE IS MATCHED?

        //        string BOXNO = splitItemBarcode.BOX.Split('/')[0].Trim() ;
        //        query = string.Format(@"select COUNT(*) from xxes_verifystoremrn where plant_code = '{0}' and family_code = '{1}' 
        //                and boxno = '{2}' AND itemcode = '{3}' AND barcode = '{4}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, BOXNO,
        //                splitItemBarcode.ITEMCODE, cOMMONDATA.DATA);
        //        if (!CheckExitsOra(query))
        //        {
        //            //return "ERROR: QUANTITY MISMATCH. REPRINT IS REQUIRED";
        //            return "ERROR: BOX NOT RECEIVED OR REPRINT REQUIRED";
        //        }
        //        #endregion

        //        #region qc check first
        //        query = string.Format(@"SELECT MRN FROM  XXES_VERIFYSTOREMRN WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' 
        //                AND ITEMCODE = '{2}' AND BARCODE = '{3}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE, cOMMONDATA.DATA);
        //        string MRN = GetColValueOra(query);
        //        if (string.IsNullOrEmpty(MRN))
        //        {
        //            return "ERROR : MRN NOT FOUND IN QR CODE";
        //        }
        //        else
        //        {
        //            query = string.Format(@"SELECT STATUS,QUALITY_OK FROM XXES_MRNINFO WHERE PLANT_CODE = '{0}' AND 
        //                                    FAMILY_CODE = '{1}' AND ITEMCODE = '{2}' AND MRN_NO = '{3}'",
        //                    cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE, MRN.Trim());
        //            DataTable dt = returnDataTable(query);
        //            if (dt.Rows.Count > 0)
        //            {
        //                string STATUS = Convert.ToString(dt.Rows[0]["STATUS"]);
        //                if (!string.IsNullOrEmpty(STATUS) && STATUS == "QA")
        //                {
        //                    string QUALITY_OK = Convert.ToString(dt.Rows[0]["QUALITY_OK"]);
        //                    if (string.IsNullOrEmpty(QUALITY_OK))
        //                    {
        //                        return "ERROR : QC NOT DONE IN QR CODE";
        //                    }
        //                }
        //            }

        //        }

        //        #endregion qc check first


        //        query = string.Format(@"SELECT LOCATION_CODE,nvl(CAPACITY,0) CAPACITY  FROM XXES_BULK_STORAGE WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ITEM_CODE='{2}'
        //        and NVL(TEMP_LOC,'')<>'Y'
        //        ORDER BY LOCATION_CODE", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE
        //            );
        //        dataTable = returnDataTable(query);
        //        if (dataTable.Rows.Count == 0)
        //        {
        //            response = "ERROR: BULK STORAGE NOT DEFINED FOR ITEM " + splitItemBarcode.ITEMCODE;
        //        }
        //        else
        //        {
        //            foreach (DataRow dataRow in dataTable.Rows)
        //            {
        //                query = string.Format(@"SELECT nvl(SUM(to_number(quantity)),0) FROM XXES_BULKSTOCK WHERE PLANT_CODE='{0}' 
        //                AND FAMILY_CODE='{1}' AND ITEMCODE='{2}' AND LOCATION_CODE='{3}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE,
        //                Convert.ToString(dataRow["LOCATION_CODE"]));
        //                existingqty = GetColValueOra(query);
        //                if (string.IsNullOrEmpty(existingqty))
        //                {
        //                    response = "OK #" + Convert.ToString(dataRow["LOCATION_CODE"]);
        //                    FLAG = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow["CAPACITY"])))
        //                    {
        //                        int rem = Convert.ToInt32(dataRow["CAPACITY"]) - Convert.ToInt32(existingqty);
        //                        if (Convert.ToInt32(splitItemBarcode.PKGQTY) <= rem)
        //                        {
        //                            response = "OK #" + Convert.ToString(dataRow["LOCATION_CODE"]);
        //                            FLAG = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (!FLAG)
        //        {

        //            response = "TEMP : BULK STORAGE NOT AVAILABLE FOR ITEM " + splitItemBarcode.ITEMCODE + ". YOU CAN PLACE ITEM AT TEMPORARY LOCATION";
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        response = "ERROR : " + ex.Message;
        //    }
        //    return response;
        //}
        [HttpPost]
        public HttpResponseMessage GetBulkStorageLocation(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            bool FLAG = false;
            string response = string.Empty, existingqty = "0";
            RETURNTABLEMSG MSGTABLE = new RETURNTABLEMSG();
            try
            {
                SplitItemBarcode splitItemBarcode = SplitItemQrcode(cOMMONDATA.DATA);
                if (string.IsNullOrEmpty(splitItemBarcode.BOX) || (!splitItemBarcode.BOX.Contains("/")))
                {
                    MSGTABLE.MSG = "ERROR: BOX NO NOT FOUND SO RE-PRINT PACKING SLIP";
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
                query = string.Format(@"select count(*) from XXES_BULKSTORAGEITEMS where barcode='{0}' and plant_code='{1}' and family_code='{2}'",
                   cOMMONDATA.DATA, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                if (CheckExitsOra(query))
                {
                    MSGTABLE.MSG = "ERROR: BARCODE ALREADY SCANNED";
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
                #region CHECK IS QR CODE IS MATCHED?

                string BOXNO = splitItemBarcode.BOX.Split('/')[0].Trim();
                query = string.Format(@"select COUNT(*) from xxes_verifystoremrn where plant_code = '{0}' and family_code = '{1}' 
                        and boxno = '{2}' AND itemcode = '{3}' AND barcode = '{4}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, BOXNO,
                        splitItemBarcode.ITEMCODE, cOMMONDATA.DATA);
                if (!CheckExitsOra(query))
                {
                    MSGTABLE.MSG = "ERROR: BOX NOT RECEIVED OR REPRINT REQUIRED";
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
                #endregion

                #region qc check first
                query = string.Format(@"SELECT MRN FROM  XXES_VERIFYSTOREMRN WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' 
                        AND ITEMCODE = '{2}' AND BARCODE = '{3}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE, cOMMONDATA.DATA);
                string MRN = GetColValueOra(query);
                if (string.IsNullOrEmpty(MRN))
                {
                    MSGTABLE.MSG = "ERROR : MRN NOT FOUND IN QR CODE";
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
                else
                {
                    query = string.Format(@"SELECT STATUS,QUALITY_OK FROM XXES_MRNINFO WHERE PLANT_CODE = '{0}' AND 
                                            FAMILY_CODE = '{1}' AND ITEMCODE = '{2}' AND MRN_NO = '{3}'",
                            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE, MRN.Trim());
                    DataTable dt = returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        string STATUS = Convert.ToString(dt.Rows[0]["STATUS"]);
                        if (!string.IsNullOrEmpty(STATUS) && STATUS == "QA")
                        {
                            string QUALITY_OK = Convert.ToString(dt.Rows[0]["QUALITY_OK"]);
                            if (string.IsNullOrEmpty(QUALITY_OK))
                            {
                                MSGTABLE.MSG = "ERROR : QC NOT DONE IN QR CODE";
                                return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                            }
                        }
                    }

                }

                #endregion qc check first


                query = string.Format(@"SELECT LOCATION_CODE,nvl(CAPACITY,0) CAPACITY  FROM XXES_BULK_STORAGE WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ITEM_CODE='{2}'
                and NVL(TEMP_LOC,'')<>'Y'
                ORDER BY LOCATION_CODE", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE
                    );
                dataTable = returnDataTable(query);
                if (dataTable.Rows.Count == 0)
                {
                    MSGTABLE.MSG = "ERROR: BULK STORAGE NOT DEFINED FOR ITEM " + splitItemBarcode.ITEMCODE;
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);

                }
                else
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        query = string.Format(@"SELECT nvl(SUM(to_number(quantity)),0) FROM XXES_BULKSTOCK WHERE PLANT_CODE='{0}' 
                        AND FAMILY_CODE='{1}' AND ITEMCODE='{2}' AND LOCATION_CODE='{3}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, splitItemBarcode.ITEMCODE,
                        Convert.ToString(dataRow["LOCATION_CODE"]));
                        existingqty = GetColValueOra(query);
                        if (string.IsNullOrEmpty(existingqty))
                        {
                            MSGTABLE.MSG = "OK #" + Convert.ToString(dataRow["LOCATION_CODE"]);
                            FLAG = true;
                            break;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow["CAPACITY"])))
                            {
                                int rem = Convert.ToInt32(dataRow["CAPACITY"]) - Convert.ToInt32(existingqty);
                                if (Convert.ToInt32(splitItemBarcode.PKGQTY) <= rem)
                                {
                                    MSGTABLE.MSG = "OK #" + Convert.ToString(dataRow["LOCATION_CODE"]);
                                    FLAG = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!FLAG)
                {
                    query = string.Format(@"SELECT LOCATION_CODE LOCATION FROM XXES_BULK_STORAGE WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' 
                                            AND TEMP_LOC = 'Y' ORDER BY TEMP_LOC", cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                    DataTable dataTable1 = returnDataTable(query);
                    MSGTABLE.MSG = "TEMP : BULK STORAGE NOT AVAILABLE FOR ITEM " + splitItemBarcode.ITEMCODE + ". YOU CAN PLACE ITEM AT TEMPORARY LOCATION";
                    MSGTABLE.LOCATION = dataTable1;
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
                }
            }
            catch (Exception ex)
            {
                MSGTABLE.MSG = "ERROR : " + ex.Message;
                return Request.CreateResponse(HttpStatusCode.OK, MSGTABLE);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]

        public HttpResponseMessage GetKanBanPKLST(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_KANBANPKLIST", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("pPLANT", cOMMONDATA.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("pFAMILY", cOMMONDATA.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("pKANBANNO", cOMMONDATA.DATA.Trim().ToUpper());
                    comm.Parameters.Add("pCREATEDBY", cOMMONDATA.CREATEDBY);
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = GetPendingKanban(cOMMONDATA);
                        }
                        else
                        {
                            dataTable = errorTable(response);
                            response = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        dataTable = errorTable(response);
                        response = JsonConvert.SerializeObject(dataTable);
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                dataTable = errorTable(response);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }
        [HttpPost]

        public HttpResponseMessage GetKanBanPKLST_NEW(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_KANBANPKLIST", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("pPLANT", cOMMONDATA.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("pFAMILY", cOMMONDATA.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("pKANBANNO", cOMMONDATA.DATA.Trim().ToUpper());
                    comm.Parameters.Add("pCREATEDBY", cOMMONDATA.CREATEDBY);
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = GetPendingKanban(cOMMONDATA);
                        }
                        else
                        {
                            dataTable = errorTable(response);
                            response = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        dataTable = errorTable(response);
                        response = JsonConvert.SerializeObject(dataTable);
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                dataTable = errorTable(response);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateKanbanPickupItemsInBulkStorage(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                if (cOMMONDATA.LOCATION.Contains("$"))
                    cOMMONDATA.LOCATION = cOMMONDATA.LOCATION.Split('$')[1].Trim();

                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_KANBANPKITEMS", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("pPLANT", cOMMONDATA.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("pFAMILY", cOMMONDATA.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("pLOC", cOMMONDATA.LOCATION.Trim().ToUpper());
                    comm.Parameters.Add("pQTY", cOMMONDATA.REMARKS.Trim().ToUpper());
                    comm.Parameters.Add("pCREATEDBY", cOMMONDATA.CREATEDBY);
                    comm.Parameters.Add("pKANBANNO", cOMMONDATA.DATA);
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = GetPendingKanban(cOMMONDATA);
                        }
                        else
                        {
                            dataTable = errorTable(response);
                            response = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        dataTable = errorTable(response);
                        response = JsonConvert.SerializeObject(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                dataTable = errorTable(response);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateKanbanPickupItemsInBulkStorageNEW(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                if (cOMMONDATA.LOCATION.Contains("$"))
                    cOMMONDATA.LOCATION = cOMMONDATA.LOCATION.Split('$')[1].Trim();
                if (string.IsNullOrEmpty(cOMMONDATA.DATA))
                {
                    response = "ERROR: SELECT VALID KANBAN";
                    dataTable = errorTable(response);
                    response = JsonConvert.SerializeObject(dataTable);
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                string kanban = string.Empty, autoid = string.Empty;
                kanban = cOMMONDATA.DATA.Split('(')[0].Trim();
                autoid = cOMMONDATA.DATA.Split('=')[1].Split(')')[0].Trim();
                if (string.IsNullOrEmpty(kanban))
                {
                    response = "ERROR: SELECT VALID KANBAN";
                    dataTable = errorTable(response);
                    response = JsonConvert.SerializeObject(dataTable);
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (string.IsNullOrEmpty(autoid))
                {
                    response = "ERROR: ID NOT FOUND";
                    dataTable = errorTable(response);
                    response = JsonConvert.SerializeObject(dataTable);
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_KANBANPKITEMS", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("pPLANT", cOMMONDATA.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("pFAMILY", cOMMONDATA.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("pLOC", cOMMONDATA.LOCATION.Trim().ToUpper());
                    comm.Parameters.Add("pQTY", cOMMONDATA.REMARKS.Trim().ToUpper());
                    comm.Parameters.Add("pCREATEDBY", cOMMONDATA.CREATEDBY);
                    comm.Parameters.Add("pKANBANNO", kanban.Trim().ToUpper());
                    comm.Parameters.Add("pAUTOID", autoid.Trim().ToUpper());
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = GetPendingKanban(cOMMONDATA);
                        }
                        else
                        {
                            dataTable = errorTable(response);
                            response = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        dataTable = errorTable(response);
                        response = JsonConvert.SerializeObject(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                dataTable = errorTable(response);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }


        private DataTable errorTable(string response)
        {
            DataTable dataTable = new DataTable();
            try
            {

                dataTable.Clear();
                dataTable.Columns.Add("KANBAN");
                dataTable.Columns.Add("LOCATION_CODE");
                dataTable.Columns.Add("ITEMCODE");
                dataTable.Columns.Add("QUANTITY");
                DataRow dataRow = dataTable.NewRow();
                dataRow["KANBAN"] = "ERROR: " + response;
                dataRow["LOCATION_CODE"] = "";
                dataRow["ITEMCODE"] = "";
                dataRow["ITEMCODE"] = "";
                dataTable.Rows.Add(dataRow);
            }
            catch (Exception)
            {

                throw;
            }
            return dataTable;
        }
        [HttpPost]
        public string GetFcodesToMake(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.GetFcodes(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: PLAN NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);

            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string UpdateSuperMarketStock(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty, super = string.Empty, zone = string.Empty, job = string.Empty, fcode = string.Empty,
                component = string.Empty, fcodeid = string.Empty; double qty = 0;
            try
            {
                super = cOMMONDATA.SUPERMARKET.Split('(')[1].Trim().Split(')')[0].Trim();
                zone = cOMMONDATA.ZONE.Split('(')[1].Trim().Split(')')[0].Trim();
                fcode = cOMMONDATA.DATA.Split('#')[0].Trim();
                fcodeid = cOMMONDATA.DATA.Split('#')[2].Trim();
                job = cOMMONDATA.REMARKS.Trim();
                component = cOMMONDATA.DATA;
                query = string.Format(@"SELECT COUNT(*) INTO TOTITEM FROM  xxes.XXES_BOM_EXPLOSION_V WHERE
                    model ='{0}' AND ORGANIZATION_CODE='{1}'", fcode, cOMMONDATA.PLANT);
                string tot = GetColValueOra(query);
                qty = (string.IsNullOrEmpty(cOMMONDATA.REMARKS) ? 0 : Convert.ToDouble(cOMMONDATA.REMARKS));
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_UPDATESUPERMKT_STOCK", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("pPLANT", cOMMONDATA.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("pFAMILY", cOMMONDATA.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("pSUPERMKT", super.Trim().ToUpper());
                    comm.Parameters.Add("pZONE", zone.Trim().ToUpper());
                    comm.Parameters.Add("pFCODE", fcode.Trim().ToUpper());
                    comm.Parameters.Add("pJOB", job.Trim().ToUpper());
                    comm.Parameters.Add("pLOC", cOMMONDATA.LOCATION.Trim().ToUpper());
                    comm.Parameters.Add("pCOMPONENT", component.Trim().ToUpper());
                    comm.Parameters.Add("pQTY", qty);
                    comm.Parameters.Add("pCREATEDBY", cOMMONDATA.CREATEDBY);
                    comm.Parameters.Add("pFCODEID", fcodeid);
                    comm.Parameters.Add("pTOTITEM", tot);
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                }

            }
            catch (Exception ex)
            {
                response = "ERROR : " + ex.Message;
            }
            return response;
        }
        [HttpPost]
        public string PickSuperMktItems(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty, super = string.Empty, zone = string.Empty;
            try
            {
                super = cOMMONDATA.SUPERMARKET.Split('(')[1].Trim().Split(')')[0].Trim();
                zone = cOMMONDATA.ZONE.Split('(')[1].Trim().Split(')')[0].Trim();
                cOMMONDATA.ITEMCODE = cOMMONDATA.ITEMCODE.Split('#')[0].Trim();
                //                query = string.Format(@"SELECT COMPONENT ITEM,COMP_DESC ,COMPONENT_QUANTITY QUANTITY,
                //               (SELECT S.SUMKTLOC FROM XXES_SUMKTSTORAGEITEMS S, XXES_SUPERMKT_LOCATIONS l
                //                WHERE S.ITEMCODE=V.COMPONENT AND s.PLANT_CODE=l.PLANT_CODE AND s.FAMILY_CODE=l.FAMILY_CODE
                //                AND s.SUMKTLOC=l.LOCATION_NAME AND l.SUPERMARKET='{2}' AND l.ZONE='{3}'
                //                AND S.QUANTITY>=V.COMPONENT_QUANTITY
                //                AND S.PLANT_CODE=V.ORGANIZATION_CODE ) LOCATION
                //                  FROM xxes.XXES_BOM_EXPLOSION_V V WHERE ORGANIZATION_CODE='{0}' AND PARENT='{1}' and 
                //COMPONENT not in (SELECT COMPONENT FROM XXES_PICKED_SUPERMKT_ITEMS WHERE JOBID='{4}' AND ITEMCODE='{1}' AND PLANT_CODE='{0}'
                //AND FAMILY_CODE='{5}')
                //",
                //                cOMMONDATA.PLANT, cOMMONDATA.ITEMCODE, super, zone,cOMMONDATA.JOB,cOMMONDATA.FAMILY);
                query = string.Format(@"SELECT COMPONENT ITEM,COMP_DESC ,COMPONENT_QUANTITY QUANTITY,
               S.SUMKTLOC LOCATION
                  FROM xxes.XXES_BOM_EXPLOSION_V V,XXES_SUMKTSTORAGEITEMS S, XXES_SUPERMKT_LOCATIONS l
                  WHERE ORGANIZATION_CODE='{0}' AND PARENT='{1}' and 
                  S.ITEMCODE=V.COMPONENT AND s.PLANT_CODE=l.PLANT_CODE AND s.FAMILY_CODE=l.FAMILY_CODE
                AND l.SUPERMARKET='{2}' AND l.ZONE='{3}'
                AND S.SUMKTLOC=L.LOCATION_NAME
                AND S.QUANTITY>=V.COMPONENT_QUANTITY
                AND S.PLANT_CODE=V.ORGANIZATION_CODE AND V.ORGANIZATION_CODE=L.PLANT_CODE
                AND
COMPONENT not in (SELECT COMPONENT FROM XXES_PICKED_SUPERMKT_ITEMS WHERE JOBID='{4}' AND ITEMCODE='{1}' AND PLANT_CODE='{0}'
AND FAMILY_CODE='{5}')
",
                               cOMMONDATA.PLANT, cOMMONDATA.ITEMCODE, super, zone, cOMMONDATA.JOB, cOMMONDATA.FAMILY);
                DataTable dataTable = returnDataTable(query);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: SUPERMARKETS NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string BindSuperMkt(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                query = string.Format(@"SELECT PARAMETERINFO + ' (' + PARAMVALUE + ')' TEXT, PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE DESCRIPTION='SUPER_MARKETS'",
                cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                DataTable dataTable = returnDataTable(query);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: SUPERMARKETS NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string BindZones(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                query = string.Format(@"SELECT PARAMETERINFO + ' (' + PARAMVALUE + ')' TEXT, PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE DESCRIPTION='SUPER_ZONES'",
                cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                DataTable dataTable = returnDataTable(query);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: ZONES NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string GetJobs(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.BindJobs(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: JOBS NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);

            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string UpdateItemBarcode(BOXBARCODE bOXBARCODE) // Store Qty Verification
        {
            string response = string.Empty;
            try
            {
                string totqty = string.Empty, boxcount = string.Empty, boxno = string.Empty, newQR = string.Empty, oldBarcode = string.Empty;
                if (fun == null)
                    fun = new Function();
                if (!bOXBARCODE.BOX.Contains('/'))
                {
                    return "ERROR: BOX NO NOT FOUND . E.g. 1/5 ";
                }
                else
                {
                    boxno = bOXBARCODE.BOX.Split('/')[0].Trim();
                    boxcount = bOXBARCODE.BOX.Split('/')[1].Trim();
                }
                if (!string.IsNullOrEmpty(bOXBARCODE.QRCODE)) // && !string.IsNullOrEmpty(bOXBARCODE.BOX) //means mannual entered the qty
                {
                    //query = string.Format(@"select count(*) from XXES_VERIFYSTOREMRN where plant_code='{0}' and family_code='{1}' and mrn='{2}' and  barcode='{3}' ",
                    //    bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.QRCODE);
                    //query = string.Format(@"select count(*) from XXES_VERIFYSTOREMRN where plant_code='{0}' and family_code='{1}' and mrn='{2}' and  boxno='{3}'  and itemcode = '{4}'",
                    //    bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, boxno.Trim(), bOXBARCODE.ITEMCODE);
                    query = string.Format(@"select count(*) from XXES_VERIFYSTOREMRN where plant_code='{0}' and family_code='{1}'  and  trim(UPPER(barcode))='{2}' ",
                        bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.QRCODE.Trim().ToUpper());
                    if (CheckExitsOra(query))
                    {
                        if (fun == null)
                            fun = new Function();
                        //if (fun.PrintingEnable("STORE_DCU", bOXBARCODE.PLANT, bOXBARCODE.FAMILY))
                        //    printQAbarcode(bOXBARCODE);
                        //return "ERROR: BARCODE ALREADY SCANNED IN MRN " + bOXBARCODE.MRN;
                        return "ERROR: BARCODE ALREADY SCANNED";
                    }

                }
                query = string.Format(@"select nvl(sum(quantity),0) from XXES_VERIFYSTOREMRN where plant_code='{0}' and family_code='{1}' and mrn='{2}' and  itemcode='{3}' ",
                    bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.ITEMCODE);
                string exqty = GetColValueOra(query);
                if (string.IsNullOrEmpty(exqty))
                    exqty = "0";
                double comingqty = 0;
                if (bOXBARCODE.REMARKS == "CELL_EDIT")
                {
                    if (Convert.ToDouble(bOXBARCODE.RECQTY) < Convert.ToDouble(exqty))
                    {
                        return "ERROR: QUANTITY SHOULD BE GREATER THAN RECEVIED QTY  " + exqty;
                    }
                }
                if (Convert.ToDouble(bOXBARCODE.RECQTY) > Convert.ToDouble(bOXBARCODE.QTY))
                {
                    return "ERROR: RECEVIED QTY SHOULD BE LESS THAN OR EQUAL TO PACKING QTY " + bOXBARCODE.RECQTY;
                }

                comingqty = Convert.ToDouble(bOXBARCODE.RECQTY) + Convert.ToDouble(exqty);
                query = string.Format(@"select nvl(sum(quantity),0) from XXES_MRNINFO where plant_code='{0}' and family_code='{1}' and mrn_no='{2}' and  itemcode='{3}'   ",
                    bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.ITEMCODE);
                string poqty = GetColValueOra(query);
                if (comingqty > Convert.ToDouble(poqty))
                {
                    return "ERROR: QUANTITY EXCEEDS " + poqty;
                }


                if (Convert.ToDouble(bOXBARCODE.RECQTY) < Convert.ToDouble(bOXBARCODE.QTY))
                {
                    //CHANGE BARCODE
                    SplitItemBarcode splitItemBarcode = SplitItemQrcode(bOXBARCODE.QRCODE.Trim().ToUpper());
                    splitItemBarcode.PKGQTY = bOXBARCODE.RECQTY;
                    newQR = Convert.ToString(splitItemBarcode.PLANT + "$" + splitItemBarcode.PO + "$" + splitItemBarcode.ITEMCODE + "$" + splitItemBarcode.PKGQTY + "$" + splitItemBarcode.BULKLOC + "$" + splitItemBarcode.POLINE + "$" + splitItemBarcode.SUPPLIER + "$" + splitItemBarcode.IF + "$" + splitItemBarcode.DATE + "$" + splitItemBarcode.BOX + "$" + bOXBARCODE.MRN);
                    oldBarcode = bOXBARCODE.QRCODE + "$" + bOXBARCODE.MRN;
                    bOXBARCODE.QRCODE = newQR;
                }
                else
                {
                    SplitItemBarcode splitItemBarcode = SplitItemQrcode(bOXBARCODE.QRCODE.Trim().ToUpper());

                    string AddMrnWithQR = Convert.ToString(splitItemBarcode.PLANT + "$" + splitItemBarcode.PO + "$" + splitItemBarcode.ITEMCODE + "$" + splitItemBarcode.PKGQTY + "$" + splitItemBarcode.BULKLOC + "$" + splitItemBarcode.POLINE + "$" + splitItemBarcode.SUPPLIER + "$" + splitItemBarcode.IF + "$" + splitItemBarcode.DATE + "$" + splitItemBarcode.BOX + "$" + bOXBARCODE.MRN);

                    bOXBARCODE.QRCODE = AddMrnWithQR;
                }


                //check item is already scanned from web(partial update boxes)
                query = string.Format(@"select REC_MODE from XXES_MRNINFO where plant_code='{0}' and family_code='{1}' and mrn_no='{2}' and  itemcode='{3}'",
                    bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.ITEMCODE);
                string REC_MODE = GetColValueOra(query);
                if (!string.IsNullOrEmpty(REC_MODE))
                {
                    return "ERROR: ITEM CODE IS ALREADY RECEIVED FROM WEB";
                }




                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand oracleCommand;
                    oracleCommand = new OracleCommand("UDSP_UPDATEITEMBARCODE", oracleConnection);
                    oracleConnection.Open();
                    oracleCommand.CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add("U_PLANT", bOXBARCODE.PLANT.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_FAMILY", bOXBARCODE.FAMILY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_ITEMCODE", bOXBARCODE.ITEMCODE.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_MRN", bOXBARCODE.MRN.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_QRCODE", bOXBARCODE.QRCODE.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_RECQTY", bOXBARCODE.RECQTY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_QTY", bOXBARCODE.QTY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("U_BOXCOUNT", boxcount);
                    oracleCommand.Parameters.Add("U_BOXNO", boxno);
                    oracleCommand.Parameters.Add("U_NEWQR", newQR);
                    oracleCommand.Parameters.Add("U_OLDBARCODE", oldBarcode);
                    oracleCommand.Parameters.Add("U_CREATEDBY", bOXBARCODE.CREATEDBY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    oracleCommand.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    oracleCommand.ExecuteNonQuery();
                    response = Convert.ToString(oracleCommand.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                }
                //query = string.Format(@"INSERT INTO XXES_VERIFYSTOREMRN(PLANT_CODE,FAMILY_CODE,MRN,ITEMCODE,QUANTITY,BOXCOUNT,BOXNO,BARCODE,CREATEDBY,CREATEDDATE)
                //    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',sysdate)", bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.ITEMCODE,
                //bOXBARCODE.RECQTY, boxcount, boxno, bOXBARCODE.QRCODE, bOXBARCODE.CREATEDBY);
                //if (ExecQueryOra(query))
                //{
                //    query = string.Format(@"UPDATE XXES_MRNINFO SET REC_QTY=to_number(nvl(REC_QTY,0)) + {0},STORE_VERIFIED='VERIFIED',
                //    STORE_VERIFIEDBY='{5}',STORE_VERIFIEDDATE=SYSDATE WHERE MRN_NO='{1}' AND ITEMCODE='{2}' AND PLANT_CODE='{3}'
                //    AND FAMILY_CODE='{4}'", Convert.ToInt32(bOXBARCODE.RECQTY), bOXBARCODE.MRN, bOXBARCODE.ITEMCODE, bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.CREATEDBY);
                //    if (ExecQueryOra(query))
                //    {
                //        if (!string.IsNullOrEmpty(newQR))
                //        {
                //            query = string.Format(@"INSERT INTO xxes_qc_log(PLANT_CODE,FAMILY_CODE,MRN,ITEMCODE,LAST_QTY,REJ_QTY,BOXNO,
                //                            REMARKS, BAROCDE, CREATEDBY, CREATEDDATE, TYPE) 
                //           VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', 'SHORT RECEIVED', '{7}', '{8}', sysdate, 'STORE_DCU')
                //        ", bOXBARCODE.PLANT, bOXBARCODE.FAMILY, bOXBARCODE.MRN, bOXBARCODE.ITEMCODE, bOXBARCODE.QTY, bOXBARCODE.RECQTY, boxno, oldBarcode, bOXBARCODE.CREATEDBY);
                //        }


                //        //if (fun == null)
                //        //    fun = new Function();
                //        //if (fun.PrintingEnable("STORE_DCU", bOXBARCODE.PLANT, bOXBARCODE.FAMILY))
                //        //    printQAbarcode(bOXBARCODE);
                //        //print box barcode if item is of QA. Two sticker print here
                //        return "OK";

                //    }
                //}
                //return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return response;
        }
        private bool printQAbarcode(BOXBARCODE bOXBARCODE)
        {
            try
            {
                List<BOXBARCODE> bOXBARCODEs = new List<BOXBARCODE>();
                if (fun == null)
                    fun = new Function();

                query = string.Format(@"select count(*)  from xxes_mrninfo where mrn='{0}' and itemcode='{1}' and status='QA'",
                            bOXBARCODE.MRN.Trim(), bOXBARCODE.ITEMCODE.Trim());
                if (fun.CheckExits(query))
                {
                    bOXBARCODEs.Add(bOXBARCODE);
                    printAssemblyBarcodes.PrintMRNQualityBarcodes(bOXBARCODEs);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }
        [HttpGet]
        public string Login(string username, string password)
        {
            AppLogin appLogin = new AppLogin();
            try
            {

                query = string.Format(@"Select * from XXES_Users_Master where upper(UsrName)='{0}' and isactive=1", username.Trim().ToUpper());
                DataTable Ds = returnDataTable(query);
                if (Ds.Rows.Count > 0)
                {
                    string test = bed.base64Decode(Ds.Rows[0]["PsWord"].ToString().Trim());
                    if (bed.base64Decode(Ds.Rows[0]["PsWord"].ToString().Trim()) == password.Trim())
                    {

                        appLogin.Login_Unit = Ds.Rows[0]["U_Code"].ToString().Trim();

                        appLogin.Login_Level = Ds.Rows[0]["L_Code"].ToString().Trim();

                        appLogin.Login_User = username.Trim().ToUpper();
                        appLogin.LoginStage = Ds.Rows[0]["StageId"].ToString().Trim();
                        appLogin.LoginFamily = Ds.Rows[0]["FamilyCode"].ToString().Trim();
                        appLogin.PUNAME = Ds.Rows[0]["PUNAME"].ToString().Trim();
                        appLogin.LoginOrgId = GetColValueOra(@"select ORG_ID from XXES_FAMILY_MASTER where 
                                family_code in (select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where 
                            plant_code='" + appLogin.Login_Unit.Trim().ToUpper() + "') and " +
                        "FAMILY_CODE='" + appLogin.LoginFamily.Trim().ToUpper() + "'");
                        appLogin.STOREBYPASS = Convert.ToString(Ds.Rows[0]["STOREBYPASS"]).Trim();
                        DataTable dtStage = new DataTable();
                        if (appLogin.LoginStage.Trim().ToUpper() == "99")
                        {

                            dtStage = returnDataTable(@"select *  from XXES_Stage_Master
                            where plant_code='" + appLogin.Login_Unit.Trim().ToUpper() + "' and family_code='" + appLogin.LoginFamily.Trim().ToUpper() + "' " +
                                " and offline_keycode='EN'");
                        }
                        else
                        {
                            dtStage = returnDataTable(@"select *  from  XXES_Stage_Master
                            where plant_code='" + appLogin.Login_Unit.Trim().ToUpper() + "' and family_code='" + appLogin.LoginFamily.Trim().ToUpper() + "' " +
                                " and stage_id='" + appLogin.LoginStage.Trim().ToUpper() + "'");

                        }
                        if (dtStage.Rows.Count > 0)
                        {
                            if (appLogin.LoginStage.Trim().ToUpper() == "99")
                                appLogin.LoginStageCode = "EN";
                            else
                                appLogin.LoginStageCode = Convert.ToString(dtStage.Rows[0]["offline_keycode"]);
                            appLogin.IPADDR = Convert.ToString(dtStage.Rows[0]["ipaddr"]);
                            appLogin.IPPORT = Convert.ToString(dtStage.Rows[0]["IPPORT"]);
                            appLogin.IsPrintLabel = Convert.ToString(dtStage.Rows[0]["PRINT_LABEL"]);
                            appLogin.PrintMMYYFormat = Convert.ToString(dtStage.Rows[0]["ONLINE_SCREEN"]);
                        }
                        appLogin.Controller = GetLoginControllerForApp(appLogin.LoginStageCode);
                        if (fun == null)
                            fun = new Function();
                        appLogin.SUFFIX = GetColValueOra(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + appLogin.Login_Unit.Trim().ToUpper() + "'");
                        appLogin.Message = "OK";
                        appLogin.ERROR_INTERVAL = Convert.ToString(fun.get_Col_Value
                            (
                            string.Format(@"select paramvalue from xxes_sft_settings where parameterinfo='ERROR_INTERVAL' and plant_code='{0}'
                                and family_code='{1}'", appLogin.Login_Unit, appLogin.LoginFamily)
                            ));
                        appLogin.SUCCESS_INTERVAL = Convert.ToString(fun.get_Col_Value
                            (
                            string.Format(@"select paramvalue from xxes_sft_settings where parameterinfo='SUCCESS_INTERVAL' and plant_code='{0}'
                                and family_code='{1}'", appLogin.Login_Unit, appLogin.LoginFamily)
                            ));
                        appLogin.OffTyreMakeCheck = fun.CheckExits(@"select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='OFF_TYRE_MAKE_CHECK' 
                                                    and STATUS='Y'");
                        appLogin.SCANNER = Convert.ToString(fun.get_Col_Value(

                                 string.Format(@"select paramvalue from XXES_SFT_SETTINGS where PARAMETERINFO='SCANNER_ENABLED' 
                                                    and plant_code='{0}'  and family_code='{1}'", appLogin.Login_Unit, appLogin.LoginFamily)
                                 ));
                    }
                    else
                    {
                        appLogin.Message = "ERROR: INVALID USERNAME OR PASSWORD";
                    }
                }
                else
                {
                    appLogin.Message = "ERROR: INVALID USERNAME OR PASSWORD";
                }
                return JsonConvert.SerializeObject(appLogin);
            }
            catch (Exception ex)
            {
                appLogin.Message = "ERROR: " + ex.Message.Trim();
                LogWrite(ex);
                return JsonConvert.SerializeObject(appLogin);
            }
            //return View();          
        }

        public string GetLoginControllerForApp(string stage)
        {
            string controller = string.Empty;
            try
            {
                switch (stage)
                {
                    case "STORE_DCU":
                    case "BLK_STORE":
                    case "KNPICKLIST":
                    case "SUMKT_STORE":
                    case "KITTING":
                    case "TMP_BLK":
                    case "FAULTY_SUMKT":
                    case "FAULTY_BLK":
                    case "STORE":
                        controller = "MAH";
                        break;
                    case "EN":
                    case "BK":
                    case "BAT":
                    case "RDTR":
                    case "STALT":
                    case "QR":
                    case "PDIOIL":
                    case "IN":
                    case "OUT":
                    case "TRAC_LOAD":
                    case "FT":
                    case "RT":
                    case "BAB":
                    case "CLS":
                    case "PSTR":
                    case "STR":
                    case "HYD":
                    case "FTRT":
                    case "HOOK_UP":
                    case "FIPNO":
                    //case "INJ_SCAN":
                        controller = "Tractor";
                        break;
                    case "INJ_SCAN":
                        controller = "EngineAssembly";
                        break;
                    default:
                        controller = "INVALID APP CONTROLLER";
                        break;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return controller;
        }
        public SplitItemBarcode SplitItemQrcode(string barcode)
        {
            SplitItemBarcode splitItemBarcode = new SplitItemBarcode();
            try
            {
                string[] arr = barcode.Split('$');


                splitItemBarcode.PLANT = arr[0].Trim().ToUpper();
                splitItemBarcode.PO = arr[1].Trim();
                splitItemBarcode.ITEMCODE = arr[2].Trim();
                splitItemBarcode.PKGQTY = arr[3].Trim();
                splitItemBarcode.BULKLOC = arr[4].Trim();
                splitItemBarcode.POLINE = arr[5].Trim();
                splitItemBarcode.SUPPLIER = arr[6].Trim();
                splitItemBarcode.IF = arr[7].Trim();
                splitItemBarcode.DATE = arr[8].Trim();
                splitItemBarcode.BOX = arr[9].Trim();

                if (arr.Length == 11)
                    splitItemBarcode.MRN = arr[10].Trim();
                else
                    splitItemBarcode.MRN = string.Empty;
            }
            catch (Exception)
            {

                throw;
            }
            return splitItemBarcode;
        }
        public string GetColValueOra(string command)
        {
            OracleConnection ConOrcl = new OracleConnection(orConnstring);
            try
            {
                string returnValue = "";
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = ConOrcl;
                cmd.CommandText = command;
                cmd.CommandType = CommandType.Text;
                returnValue = Convert.ToString(cmd.ExecuteScalar());
                return returnValue;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }
        public void LogWrite(Exception ex)
        {
            try
            {
                if (ex.Message != "Thread was being aborted." || ex.Message != "The ConnectionString property has not been initialized.")
                {
                    string DirectoryPath = HttpContext.Current.Server.MapPath("~/ErrorLog/");

                    if (!Directory.Exists(DirectoryPath))
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }

                    string FileName = DateTime.Now.ToString("MM-dd-yyyy") + ".txt";
                    string FilePath = DirectoryPath + FileName;
                    using (StreamWriter streamWriter = new StreamWriter(FilePath, true))
                    {
                        streamWriter.WriteLine(DateTime.Now);
                        streamWriter.WriteLine(ex.Message);
                        streamWriter.WriteLine(ex.StackTrace);
                        streamWriter.WriteLine("____________________________________________________________________________________________");
                        streamWriter.WriteLine();
                    }
                }
            }
            catch
            {
            }
        }
        public Boolean CheckExitsOra(string SqlQuery)
        {
            Boolean returnValue = false;
            OracleConnection ConOrcl = new OracleConnection(orConnstring);
            try
            {
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand sc = new OracleCommand(SqlQuery, ConOrcl);
                sc.CommandType = CommandType.Text;
                returnValue = Convert.ToBoolean(sc.ExecuteScalar());
                return returnValue;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }
        public DataTable returnDataTable(string SqlQuery)
        {

            OracleConnection ConOrcl = new OracleConnection(orConnstring);
            DataTable dt = new DataTable();
            try
            {
                OracleDataAdapter Oda = new OracleDataAdapter(SqlQuery, ConOrcl);
                Oda.Fill(dt);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { ConOrcl.Dispose(); }
            return dt;
        }

        public bool ExecQueryOra(string command)
        {
            bool returnValue = false;
            OracleConnection ConOrcl = new OracleConnection(orConnstring);
            try
            {
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand sc = new OracleCommand(command, ConOrcl);
                sc.CommandType = CommandType.Text;
                returnValue = Convert.ToBoolean(sc.ExecuteNonQuery());
                return returnValue;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }

        [HttpPost]
        public string KettingProcess(Kitting KT)
        {
            string query = string.Empty;
            try
            {
                query = ValidateKittingItems(KT);
                if (query != "OK")
                {
                    return query;
                }
                query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,KITNO,ITEMCODE,QUANTITY,CREATEDBY FROM XXES_KITMASTER WHERE PLANT_CODE='{0}'
                AND FAMILY_CODE='{1}' AND KITNO='{2}'", KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim(), KT.KITNO.ToUpper().Trim());

                DataTable dt = new DataTable();
                dt = returnDataTable(query);
                double pickedqty = 0, remainqty = 0;
                foreach (DataRow dataRow in dt.Rows)
                {
                    remainqty = pickedqty = 0;
                    string TotalQuantity = string.Empty;
                    query = string.Format(@"SELECT AUTOID,quantity FROM XXES_SUMKTSTORAGEITEMS WHERE ITEMCODE='{0}' AND PLANT_CODE='{1}'
                    AND FAMILY_CODE='{2}' and nvl(quantity,0)>0", Convert.ToString(dataRow["ITEMCODE"]), KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim());
                    DataTable dtInside = returnDataTable(query);
                    if (dtInside.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtInside.Rows)
                        {
                            if (Convert.ToDouble(row["QUANTITY"]) < Convert.ToDouble(dataRow["QUANTITY"]))
                            {
                                query = string.Format(@"update XXES_SUMKTSTORAGEITEMS set QUANTITY='0' where AUTOID='{0}'",
                                    row["autoid"]);
                                if (ExecQueryOra(query))
                                {
                                    pickedqty += Convert.ToDouble(row["QUANTITY"]);
                                }

                            }
                            else
                            {
                                remainqty = Convert.ToDouble(dataRow["QUANTITY"]) - pickedqty;
                                query = string.Format(@"update XXES_SUMKTSTORAGEITEMS set QUANTITY=QUANTITY-'{1}' where autoid='{0}'",
                                    row["autoid"], remainqty);
                                ExecQueryOra(query);
                            }
                            if (pickedqty == Convert.ToDouble(dataRow["QUANTITY"]))
                            {
                                break;
                            }
                        }
                    }
                }

                query = String.Format(@"INSERT INTO XXES_SCANKITTING(PLANT_CODE,FAMILY_CODE,KITNO,CREATEDBY) VALUES('{0}','{1}','{2}','{3}')",
                    KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim(), KT.KITNO.ToUpper().Trim(), KT.CREATEDBY.ToUpper().Trim());
                ExecQueryOra(query);

                // insertr into kitting scan table
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return "OK";
        }


        [HttpPost]
        public string KettingProcessNew(Kitting KT)
        {
            string query = string.Empty, response = string.Empty;
            try
            {
                query = ValidateKittingItemsNew(KT);
                if (query != "OK")
                {
                    return query;
                }
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_KITTING", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("S_PLANT_CODE", KT.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("S_FAMILY_CODE", KT.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("S_KITNO", KT.KITNO.Trim().ToUpper());
                    comm.Parameters.Add("S_CREATEDBY", KT.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (response.StartsWith("OK"))
                    {
                        try
                        {
                            var year = ""; var currentyear = DateTime.Now.ToString("yy");
                            DateTime currentDateTime = DateTime.Now.Date;
                            string currentmonth = DateTime.Now.ToString("04-April");
                            year = DateTime.Now.ToString("yy");
                            if (DateTime.Now.Month <= 3)
                                year = DateTime.Now.ToString("yy");
                            else
                                year = DateTime.Now.AddYears(1).ToString("yy");

                            DataTable dt = new DataTable(); string OldDate = string.Empty; string series = string.Empty;
                            string chalanno = string.Empty; string Autoid = string.Empty; string createdate = string.Empty;
                            query = string.Format(@"SELECT AutoID, SERIES_NO , CHALLAN_NO , CHALLAN_DATE, CREATEDDATE FROM XXES_SCANKITTING_ITEMS  WHERE AutoID=
                                (SELECT MAX(AutoID) FROM XXES_SCANKITTING_ITEMS  where  PLANT_CODE='{0}'
                                AND FAMILY_CODE='{1}' and trunc(createddate)=trunc(sysdate-1)) and   PLANT_CODE='{0}'
                                AND FAMILY_CODE='{1}'",
                                    KT.PLANT.Trim().ToUpper(), KT.FAMILY.Trim().ToUpper());
                            dt = fun.returnDataTable(query);
                            if (dt.Rows.Count > 0)
                            {
                                OldDate = Convert.ToString(dt.Rows[0]["CHALLAN_DATE"]);
                                series = Convert.ToString(dt.Rows[0]["SERIES_NO"]);
                                Autoid = Convert.ToString(dt.Rows[0]["AutoID"]);
                                createdate = Convert.ToString(dt.Rows[0]["CREATEDDATE"]);
                            }
                            createdate = DateTime.Now.ToString("dd-MMMM");
                            if ((createdate == currentmonth) || string.IsNullOrEmpty(OldDate) && string.IsNullOrEmpty(series))
                            {
                                series = "35001";
                                chalanno = KT.PLANT + "/" + year + "/" + series;
                            }
                            else if (DateTime.Parse(OldDate) < currentDateTime)
                            {
                                int count;
                                count = Convert.ToInt32(series) + 1;
                                series = Convert.ToString(count);
                                chalanno = KT.PLANT + "/" + year + "/" + series;
                            }

                            query = "";

                            if (!string.IsNullOrEmpty(chalanno))
                            {
                                query = string.Format(@"UPDATE XXES_SCANKITTING_ITEMS SET SERIES_NO='{0}',CHALLAN_NO='{1}',
                                        CHALLAN_DATE = SYSDATE WHERE TRUNC(CREATEDDATE)=TRUNC(SYSDATE) AND CHALLAN_NO IS NULL", series, chalanno);
                                if (fun.EXEC_QUERY(query))
                                {
                                    // return response;
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            fun.LogWrite(ex);
                            //throw;
                        }

                    }
                }



                // insertr into kitting scan table
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return response;
        }

        public string ValidateKittingItems(Kitting KT)
        {

            try
            {
                string query = string.Empty;

                // check last scanned kittno no get the time difference in minue if less than 5 then error show already scanned

                query = String.Format(@"SELECT TO_CHAR(CREATEDDATE,'dd-Mon-yyyy HH24:MI:SS') || '#' ||
                TO_CHAR(SYSDATE,'dd-Mon-yyyy HH24:MI:SS')  FROM XXES_SCANKITTING WHERE AUTOID=
                (SELECT MAX(AUTOID) FROM XXES_SCANKITTING WHERE KITNO='{0}')", KT.KITNO);
                string lastScanned = GetColValueOra(query);
                if (!string.IsNullOrEmpty(lastScanned))
                {
                    DateTime lastscannedDate = Convert.ToDateTime(lastScanned.Split('#')[0].Trim());
                    DateTime Currentdatetime = Convert.ToDateTime(lastScanned.Split('#')[1].Trim());
                    TimeSpan ts = Currentdatetime - lastscannedDate;
                    if (ts.Minutes < 5)
                    {
                        return "ERROR : KITNO " + KT.KITNO + " ALREADY SCANNED ";
                    }
                }


                query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,KITNO,ITEMCODE,QUANTITY,CREATEDBY FROM XXES_KITMASTER WHERE PLANT_CODE='{0}'
                AND FAMILY_CODE='{1}' AND KITNO='{2}'", KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim(), KT.KITNO.ToUpper().Trim());

                DataTable dt = new DataTable();
                dt = returnDataTable(query);

                foreach (DataRow dataRow in dt.Rows)
                {
                    string TotalQuantity = string.Empty;
                    query = string.Format(@"SELECT SUM(QUANTITY) FROM XXES_SUMKTSTORAGEITEMS WHERE ITEMCODE='{0}' AND PLANT_CODE='{1}'
                    AND FAMILY_CODE='{2}'", Convert.ToString(dataRow["ITEMCODE"]), KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim());
                    TotalQuantity = GetColValueOra(query);
                    if (string.IsNullOrEmpty(TotalQuantity) || TotalQuantity == "0")
                    {
                        return "ERROR : QTY IS NOT AVAILABLE FOR ITEM " + Convert.ToString(dataRow["ITEMCODE"]);

                    }
                    if (Convert.ToDouble(TotalQuantity) < Convert.ToInt32(dataRow["QUANTITY"]))
                    {
                        return "ERROR : QTY IS NOT AVAILABLE FOR ITEM " + Convert.ToString(dataRow["ITEMCODE"]);
                    }

                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR: " + ex.Message;
            }
            return "OK";
        }
        public string ValidateKittingItemsNew(Kitting KT)
        {

            try
            {
                string query = string.Empty;

                // check last scanned kittno no get the time difference in minue if less than 5 then error show already scanned

                query = String.Format(@"SELECT TO_CHAR(CREATEDDATE,'dd-Mon-yyyy HH24:MI:SS') || '#' ||
                TO_CHAR(SYSDATE,'dd-Mon-yyyy HH24:MI:SS')  FROM XXES_SCANKITTING WHERE AUTOID=
                (SELECT MAX(AUTOID) FROM XXES_SCANKITTING WHERE KITNO='{0}')", KT.KITNO);
                string lastScanned = GetColValueOra(query);
                if (!string.IsNullOrEmpty(lastScanned))
                {
                    DateTime lastscannedDate = Convert.ToDateTime(lastScanned.Split('#')[0].Trim());
                    DateTime Currentdatetime = Convert.ToDateTime(lastScanned.Split('#')[1].Trim());
                    TimeSpan ts = Currentdatetime - lastscannedDate;
                    if (ts.Minutes < 1)
                    {
                        return "ERROR : KITNO " + KT.KITNO + " ALREADY SCANNED ";
                    }
                }


                query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,KITNO,ITEMCODE,QUANTITY,CREATEDBY FROM XXES_KITMASTER WHERE PLANT_CODE='{0}'
                AND FAMILY_CODE='{1}' AND KITNO='{2}'", KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim(), KT.KITNO.ToUpper().Trim());

                DataTable dt = new DataTable();
                dt = returnDataTable(query);
                if (dt.Rows.Count == 0)
                {
                    return "ERROR : INVALID KIT NO";
                }
                foreach (DataRow dataRow in dt.Rows)
                {
                    string TotalQuantity = string.Empty;
                    query = string.Format(@"SELECT SUM(QUANTITY) FROM XXES_SUMMKTSTOCK WHERE ITEMCODE='{0}' AND PLANT_CODE='{1}'
                    AND FAMILY_CODE='{2}'", Convert.ToString(dataRow["ITEMCODE"]), KT.PLANT.ToUpper().Trim(), KT.FAMILY.ToUpper().Trim());
                    TotalQuantity = GetColValueOra(query);
                    if (string.IsNullOrEmpty(TotalQuantity) || TotalQuantity == "0")
                    {
                        return "ERROR : QTY IS NOT AVAILABLE FOR ITEM " + Convert.ToString(dataRow["ITEMCODE"]);

                    }
                    if (Convert.ToDouble(TotalQuantity) < Convert.ToInt32(dataRow["QUANTITY"]))
                    {
                        return "ERROR : QTY IS NOT AVAILABLE FOR ITEM " + Convert.ToString(dataRow["ITEMCODE"]);
                    }

                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR: " + ex.Message;
            }
            return "OK";
        }

        [HttpPost]
        public string GetTempInfo(COMMONDATA cOMMONDATA) // COMMONDATA data
        {
            string response = string.Empty;
            try
            {

                query = string.Format(@"select count(*) from XXES_BULK_STORAGE s where s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR : INVALID LOCATION";
                }
                query = string.Format(@"SELECT count(*) FROM XXES_BULKSTORAGEITEMS s WHERE s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR: NO ITEM FOUND AT SCANNED TEMP LOCATION";// + splitItem.ITEMCODE;

                }
                query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,A.LOCATION_CODE,A.ITEM_CODE,A.ITEM_DESCRIPTION,A.CAPACITY,A.EXISTING_QTY
                        ,A.CAPACITY-A.EXISTING_QTY AVAILABLE_SPACE,'' TEMP_QTY
                         FROM (SELECT S.PLANT_CODE,S.FAMILY_CODE, LOCATION_CODE,S.ITEM_CODE,R.ITEM_DESCRIPTION, CAPACITY,
                        (NVL((SELECT SUM(xb.QUANTITY) FROM XXES_BULKSTORAGEITEMS xb WHERE XB.PLANT_CODE=S.PLANT_CODE
                        AND XB.FAMILY_CODE=S.FAMILY_CODE AND XB.LOCATION_CODE=S.LOCATION_CODE AND XB.ITEMCODE=S.ITEM_CODE
                        ),0)) EXISTING_QTY
                         FROM XXES_BULK_STORAGE S JOIN XXES_RAWMATERIAL_MASTER R
                         ON S.PLANT_CODE=R.PLANT_CODE AND S.FAMILY_CODE=R.FAMILY_CODE
                         AND S.ITEM_CODE=R.ITEM_CODE AND NVL(S.TEMP_LOC,'')='N'
                         AND S.ITEM_CODE
                        IN (SELECT DISTINCT itemcode FROM XXES_BULKSTORAGEITEMS i JOIN XXES_BULK_STORAGE a ON i.PLANT_CODE=a.PLANT_CODE
                        AND i.FAMILY_CODE=a.FAMILY_CODE AND
                        i.PLANT_CODE='{0}' AND i.FAMILY_CODE='{1}'
                        AND A.TEMP_LOC='Y' AND A.LOCATION_CODE='{2}')
                        AND s.PLANT_CODE='{0}' AND s.FAMILY_CODE='{1}'
                        AND NVL(S.CAPACITY,0)>
                        NVL((SELECT SUM(xb.QUANTITY) FROM XXES_BULKSTORAGEITEMS xb WHERE XB.PLANT_CODE=S.PLANT_CODE
                        AND XB.FAMILY_CODE=S.FAMILY_CODE AND XB.LOCATION_CODE=S.LOCATION_CODE AND XB.ITEMCODE=S.ITEM_CODE
                        AND xb.PLANT_CODE='{0}' AND xb.FAMILY_CODE='{1}'),0))A", cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper());
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }

        }

        [HttpPost]
        public string GetTempInfoNew(COMMONDATA cOMMONDATA) // COMMONDATA data
        {
            string response = string.Empty;
            try
            {
                if (cOMMONDATA.LOCATION.Contains("$"))
                {
                    cOMMONDATA.LOCATION = cOMMONDATA.LOCATION.Split('$')[1].Trim();
                }
                query = string.Format(@"select count(*) from XXES_BULK_STORAGE s where s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR : INVALID LOCATION";
                }
                query = string.Format(@"SELECT count(*) FROM XXES_BULKSTOCK s WHERE s.plant_code='{0}' AND s.family_code='{1}' and location_code='{2}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR: NO ITEM FOUND AT SCANNED TEMP LOCATION";// + splitItem.ITEMCODE;
                }
                query = string.Format(@"SELECT S.ITEMCODE,R.ITEM_DESCRIPTION,SUM(S.QUANTITY) TEMP_QTY,
                    (
                          SELECT  FN_GETSTORGE_LOCATION('BULK_DCU',S.PLANT_CODE,S.FAMILY_CODE,S.ITEMCODE) FROM DUAL
                    ) BLK_AVAIL
                     FROM XXES_BULKSTOCK S JOIN XXES_BULK_STORAGE M ON S.PLANT_CODE=M.PLANT_CODE AND S.FAMILY_CODE=M.FAMILY_CODE
                    AND S.LOCATION_CODE=M.LOCATION_CODE  JOIN XXES_RAWMATERIAL_MASTER R ON S.PLANT_CODE=R.PLANT_CODE AND S.FAMILY_CODE=R.FAMILY_CODE
                    AND S.ITEMCODE=R.ITEM_CODE
                    AND M.TEMP_LOC='Y' WHERE M.LOCATION_CODE='{2}' AND M.PLANT_CODE='{0}' AND M.FAMILY_CODE='{1}'
                    GROUP BY S.ITEMCODE,R.ITEM_DESCRIPTION,S.PLANT_CODE,S.FAMILY_CODE,S.ITEMCODE",
                    cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(),
                    cOMMONDATA.LOCATION.Trim().ToUpper());
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }

        }

        [HttpPost]
        public string GetBarcodeInfo(COMMONDATA cOMMONDATA) // COMMONDATA data
        {
            string response = string.Empty;
            try
            {
                query = string.Format(@"SELECT count(*) FROM XXES_BULKSTORAGEITEMS s WHERE s.plant_code='{0}' AND s.family_code='{1}' and LOCATION_CODE='{2}' and BARCODE='{3}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper(), cOMMONDATA.DATA.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR: BARCODE SHOULD BE FROM TEMP LOCATION";// + splitItem.ITEMCODE;
                }
                query = string.Format(@"SELECT DISTINCT ITEMCODE,QUANTITY,BARCODE FROM XXES_BULKSTORAGEITEMS WHERE
                        PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ITEMCODE='{2}' AND LOCATION_CODE='{3}' AND BARCODE='{4}'",
                        cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(),
                        cOMMONDATA.ITEMCODE.Trim().ToUpper(), cOMMONDATA.LOCATION.Trim().ToUpper(), cOMMONDATA.DATA.Trim().ToUpper());
                return JsonConvert.SerializeObject(returnDataTable(query));
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }

        }
        [HttpPost]
        public string GetBarcodeInfoNew(COMMONDATA cOMMONDATA) // COMMONDATA data
        {
            string response = string.Empty;
            try
            {
                query = string.Format(@"SELECT count(*) FROM XXES_BULKSTORAGEITEMS s join XXES_BULK_STORAGE m on S.PLANT_CODE=M.PLANT_CODE AND S.FAMILY_CODE=M.FAMILY_CODE
                    AND S.LOCATION_CODE=M.LOCATION_CODE  WHERE s.plant_code='{0}' AND s.family_code='{1}' and m.TEMP_LOC='Y' and BARCODE='{2}'",
                cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(), cOMMONDATA.DATA.Trim().ToUpper());
                if (!CheckExitsOra(query))
                {
                    return "ERROR: BARCODE SHOULD BE FROM TEMP LOCATION";// + splitItem.ITEMCODE;
                }
                query = string.Format(@"SELECT  FN_GETSTORGE_LOCATION('BULK_DCU','{0}','{1}','{2}') FROM DUAL",
                        cOMMONDATA.PLANT.Trim().ToUpper(), cOMMONDATA.FAMILY.Trim().ToUpper(),
                        cOMMONDATA.ITEMCODE.Trim().ToUpper());
                string loc = Convert.ToString(GetColValueOra(query));
                if (string.IsNullOrEmpty(loc))
                    return "MAIN LOCATION : 0";
                else
                    return "MAIN LOCATION : " + loc.Trim();
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }

        }


        [HttpPost]
        public string MoveTempToMainLocation(BULKSTORGAE bULKSTORGAE)
        {
            try
            {
                string capacity = string.Empty, existingqty = string.Empty;
                double comingqty = 0;
                if (!string.IsNullOrEmpty(bULKSTORGAE.LOCATION))
                {
                    if (bULKSTORGAE.LOCATION.Contains("$"))
                    {
                        bULKSTORGAE.LOCATION = bULKSTORGAE.LOCATION.Split('$')[1].Trim();
                    }
                }
                if (!string.IsNullOrEmpty(bULKSTORGAE.TEMPLOCATION))
                {
                    if (bULKSTORGAE.TEMPLOCATION.Contains("$"))
                    {
                        bULKSTORGAE.TEMPLOCATION = bULKSTORGAE.TEMPLOCATION.Split('$')[1].Trim();
                    }
                }
                if (!string.IsNullOrEmpty(bULKSTORGAE.AVAIL_LOC))
                {
                    if (bULKSTORGAE.AVAIL_LOC.Contains("$"))
                    {
                        bULKSTORGAE.AVAIL_LOC = bULKSTORGAE.AVAIL_LOC.Split('$')[1].Trim();
                    }
                }
                if (!string.IsNullOrEmpty(bULKSTORGAE.LOCATION))
                {

                    if (bULKSTORGAE.LOCATION.Trim().ToUpper() != bULKSTORGAE.AVAIL_LOC.Trim().ToUpper())
                    {
                        return "ERROR : ITEM MUST BE PLACE ON AVAILABLE LOCATION";
                    }
                }
                SplitItemBarcode splitItem = SplitItemQrcode(bULKSTORGAE.QRCODE);
                if (string.IsNullOrEmpty(splitItem.PKGQTY) || splitItem.PKGQTY == "0")
                    return "ERROR: INVALID QUANTITY";
                float f;
                if (!float.TryParse(splitItem.PKGQTY, out f))
                {
                    return "ERROR: INVALID QUANTITY";
                }

                if (bULKSTORGAE.LOCATION.Trim().ToUpper() != bULKSTORGAE.AVAIL_LOC.Trim().ToUpper())
                {
                    return "ERROR : LOCATION MISMATCH";
                }
                query = string.Format(@"SELECT count(*) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and item_code='{2}' and location_code='{3}' ",
                bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                if (!CheckExitsOra(query))
                {
                    return "ERROR: INVALID MAIN LOCATION";// + splitItem.ITEMCODE;
                }
                query = string.Format(@"SELECT nvl(capacity,0) FROM XXES_BULK_STORAGE s WHERE s.plant_code='{0}' AND s.family_code='{1}' and item_code='{2}' and location_code='{3}' ",
                bULKSTORGAE.PLANT.Trim().ToUpper(), bULKSTORGAE.FAMILY.Trim().ToUpper(), splitItem.ITEMCODE.Trim().ToUpper(), bULKSTORGAE.LOCATION.Trim().ToUpper());
                capacity = GetColValueOra(query);
                if (string.IsNullOrEmpty(capacity))
                    capacity = "0";

                query = string.Format(@"SELECT nvl(sum(QUANTITY),0) FROM xxes_bulkstock s WHERE s.plant_code='{0}' AND s.family_code='{1}' and itemcode='{2}' and location_code='{3}' ",
                    bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, splitItem.ITEMCODE, bULKSTORGAE.LOCATION);
                existingqty = GetColValueOra(query);
                if (string.IsNullOrEmpty(existingqty))
                    existingqty = "0";

                comingqty = Convert.ToDouble(splitItem.PKGQTY) + Convert.ToDouble(existingqty);
                if (comingqty > Convert.ToDouble(capacity))
                {
                    //return "ERROR : EXISTING QTY FOUND " + existingqty + ", BARCODE QTY " + splitItem.PKGQTY + ", TOTAL = " + comingqty + " WHICH IS EXCEEDING CAPACITY " + capacity;
                    return "ERROR : SPACE NOT AVAILABLE " + bULKSTORGAE.LOCATION;
                }
                //query = string.Format(@"delete from XXES_BULKSTORAGEITEMS where nvl(quantity,0)=0");
                //ExecQueryOra(query);
                bool result = false;
                string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    { connection.Open(); }

                    OracleCommand command = connection.CreateCommand();
                    OracleTransaction transaction;

                    transaction = connection.BeginTransaction();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    try
                    {
                        command.CommandText = string.Format(@"DELETE FROM XXES_BULKSTORAGEITEMS s WHERE s.PLANT_CODE='{0}' AND s.FAMILY_CODE='{1}' 
                          and LOCATION_CODE='{2}' AND S.BARCODE='{3}'", bULKSTORGAE.PLANT.Trim().ToUpper(), bULKSTORGAE.FAMILY.Trim().ToUpper(), bULKSTORGAE.TEMPLOCATION.Trim().ToUpper(), bULKSTORGAE.QRCODE.Trim().ToUpper());
                        ExecQueryOra(query);
                        command.ExecuteNonQuery();

                        command.CommandText = string.Format(@"INSERT INTO XXES_BULKSTORAGEITEMS(PLANT_CODE,FAMILY_CODE,LOCATION_CODE,ITEMCODE,QUANTITY,BARCODE,CREATEDBY,CREATEDDATE)
                        values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',sysdate)", bULKSTORGAE.PLANT, bULKSTORGAE.FAMILY, bULKSTORGAE.LOCATION, splitItem.ITEMCODE,
                           splitItem.PKGQTY, bULKSTORGAE.QRCODE, bULKSTORGAE.CREATEDBY);
                        command.ExecuteNonQuery();

                        command.CommandText = string.Format(@"UPDATE XXES_BULKSTOCK SET QUANTITY=NVL(QUANTITY,0)-{0},UPDATEBY='{5}',UPDATEDDATE=sysdate WHERE PLANT_CODE='{1}'
                            AND FAMILY_CODE='{2}' AND ITEMCODE='{3}' AND LOCATION_CODE='{4}' ",
                               Convert.ToDouble(splitItem.PKGQTY), bULKSTORGAE.PLANT.Trim().ToUpper(),
                               bULKSTORGAE.FAMILY.Trim().ToUpper(), splitItem.ITEMCODE, bULKSTORGAE.TEMPLOCATION.Trim().ToUpper(),
                               bULKSTORGAE.CREATEDBY);
                        command.ExecuteNonQuery();


                        query = string.Format(@"select count(*) from XXES_BULKSTOCK where plant_code='{0}' 
                            and family_code='{1}' and itemcode='{2}' and location_code='{3}'",
                            bULKSTORGAE.PLANT.Trim().ToUpper(), bULKSTORGAE.FAMILY.Trim().ToUpper(), splitItem.ITEMCODE, bULKSTORGAE.LOCATION.Trim().ToUpper());
                        if (fun.CheckExits(query))
                        {
                            command.CommandText = string.Format(@"UPDATE XXES_BULKSTOCK SET QUANTITY=NVL(QUANTITY,0)+{0},UPDATEBY='{5}',UPDATEDDATE=sysdate WHERE PLANT_CODE='{1}'
                            AND FAMILY_CODE='{2}' AND ITEMCODE='{3}' AND LOCATION_CODE='{4}' ",
                                  Convert.ToDouble(splitItem.PKGQTY), bULKSTORGAE.PLANT.Trim().ToUpper(),
                                  bULKSTORGAE.FAMILY.Trim().ToUpper(), splitItem.ITEMCODE, bULKSTORGAE.LOCATION.Trim().ToUpper(), bULKSTORGAE.CREATEDBY);
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            command.CommandText = string.Format(@"INSERT INTO XXES_BULKSTOCK (PLANT_CODE, FAMILY_CODE, LOCATION_CODE, ITEMCODE, QUANTITY, CREATEDBY, CREATEDDATE)
                                VALUES ('{0}','{1}','{2}', '{3}',{4},'{5}', SYSDATE)", bULKSTORGAE.PLANT.Trim().ToUpper(),
                              bULKSTORGAE.FAMILY.Trim().ToUpper(), bULKSTORGAE.LOCATION.Trim().ToUpper(),
                              splitItem.ITEMCODE, Convert.ToDouble(splitItem.PKGQTY), bULKSTORGAE.CREATEDBY);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        LogWrite(ex);
                        result = false;
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2)
                        {

                        }
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        { connection.Close(); }
                        connection.Dispose();
                    }
                }
                if (result)
                    return "OK";
                else
                    return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }

        [HttpPost]
        public HttpResponseMessage MoveTempToMainLocationNew(BULKSTORGAE bULKSTORGAE)
        {
            string response = string.Empty;
            //DataTable dt = new DataTable();
            try
            {
                SplitItemBarcode splitItem = SplitItemQrcode(bULKSTORGAE.QRCODE);
                if (string.IsNullOrEmpty(splitItem.PKGQTY) || splitItem.PKGQTY == "0")
                {
                    response = "ERROR: INVALID QUANTITY";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                float f;
                if (!float.TryParse(splitItem.PKGQTY, out f))
                {
                    response = "ERROR: INVALID QUANTITY";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }

                if (bULKSTORGAE.LOCATION.Contains("$"))
                    bULKSTORGAE.LOCATION = bULKSTORGAE.LOCATION.Split('$')[1].Trim();

                if (bULKSTORGAE.TEMPLOCATION.Contains("$"))
                    bULKSTORGAE.TEMPLOCATION = bULKSTORGAE.TEMPLOCATION.Split('$')[1].Trim();

                if (bULKSTORGAE.AVAIL_LOC.Contains("$"))
                    bULKSTORGAE.AVAIL_LOC = bULKSTORGAE.AVAIL_LOC.Split('$')[1].Trim();
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_MOVETEMPTOMAINLOCATION", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("M_PLANT_CODE", bULKSTORGAE.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("M_FAMILY_CODE", bULKSTORGAE.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("M_LOCATION_CODE", bULKSTORGAE.LOCATION.Trim().ToUpper());
                    comm.Parameters.Add("M_AVAIL_LOC", bULKSTORGAE.AVAIL_LOC.Trim().ToUpper());
                    comm.Parameters.Add("M_TEMP_LOC", bULKSTORGAE.TEMPLOCATION.Trim().ToUpper());
                    comm.Parameters.Add("M_PKGQTY", splitItem.PKGQTY);
                    comm.Parameters.Add("M_ITEMCODE", splitItem.ITEMCODE.Trim().ToUpper());
                    comm.Parameters.Add("M_BARCODE", bULKSTORGAE.QRCODE.Trim().ToUpper());
                    comm.Parameters.Add("M_CREATEDBY", bULKSTORGAE.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MASSEGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MASSEGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MASSEGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK";
                        }
                        else
                        {
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                };

            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        public string GetLocationItems(string Location, string Plant, string Family, string Stage)
        {
            try
            {
                // SM = D10398450$ES03323$B02-114
                //BK = D10664270/A$B21-306-311$ES08-016
                //string[] splitloc = Location.Trim().Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                int count = Location.Split('$').Length - 1;
                if (count < 2)
                {
                    return JsonConvert.SerializeObject("ERROR: INVALID LOCATION");
                }

                Location = Location.Split('$')[1].Trim();

                DataTable dt = new DataTable();
                if (Stage == "FAULTY_SUMKT")
                {

                    //    query = string.Format(@"
                    //SELECT COUNT(*) FROM xxes_summktstock SMI
                    //INNER JOIN XXES_SUPERMKT_LOCATIONS SML
                    //ON SMI.PLANT_CODE = SML.PLANT_CODE AND SMI.FAMILY_CODE = SML.FAMILY_CODE AND SMI.LOCATION_CODE = SML.LOCATION_NAME
                    //WHERE SMI.PLANT_CODE = '{0}' AND SMI.FAMILY_CODE = '{1}' AND SMI.LOCATION_CODE = '{2}'", Plant, Family, Location);

                    //    if (!CheckExitsOra(query))
                    //    {
                    //        return JsonConvert.SerializeObject("ERROR: INVALID LOCATION");
                    //    }
                    query = string.Format(@"
                SELECT SMI.LOCATION_CODE LOCATION, SMI.ITEMCODE, SMI.QUANTITY FROM xxes_summktstock SMI
                INNER JOIN XXES_SUPERMKT_LOCATIONS SML
                ON SMI.PLANT_CODE = SML.PLANT_CODE AND SMI.FAMILY_CODE = SML.FAMILY_CODE AND SMI.LOCATION_CODE = SML.LOCATION_NAME
                WHERE SMI.PLANT_CODE = '{0}' AND SMI.FAMILY_CODE = '{1}' AND SMI.LOCATION_CODE = '{2}'", Plant, Family, Location);

                }
                else if (Stage == "FAULTY_BLK")
                {
                    //    query = string.Format(@"
                    //SELECT BLI.LOCATION_CODE LOCATION,BLI.ITEMCODE,BLI.QUANTITY FROM XXES_BULKSTOCK BLI
                    //INNER JOIN XXES_BULK_STORAGE BS
                    //ON BLI.PLANT_CODE = BS.PLANT_CODE AND BLI.FAMILY_CODE = BS.FAMILY_CODE AND BLI.LOCATION_CODE = BS.LOCATION_CODE
                    // WHERE BLI.PLANT_CODE = '{0}' AND BLI.FAMILY_CODE = '{1}' AND BLI.LOCATION_CODE = '{2}' ", Plant, Family, Location);

                    //    if (!CheckExitsOra(query))
                    //    {
                    //        return JsonConvert.SerializeObject("ERROR: INVALID LOCATION");
                    //    }
                    query = string.Format(@"
                SELECT BLI.LOCATION_CODE LOCATION,BLI.ITEMCODE,BLI.QUANTITY FROM XXES_BULKSTOCK BLI
                INNER JOIN XXES_BULK_STORAGE BS
                ON BLI.PLANT_CODE = BS.PLANT_CODE AND BLI.FAMILY_CODE = BS.FAMILY_CODE AND BLI.LOCATION_CODE = BS.LOCATION_CODE
                 WHERE BLI.PLANT_CODE = '{0}' AND BLI.FAMILY_CODE = '{1}' AND BLI.LOCATION_CODE = '{2}' ", Plant, Family, Location);

                }

                dt = returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                else
                {
                    return JsonConvert.SerializeObject("ERROR: EITHER INVALID LOCATION OR RECORD NOT FOUND");
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }

        [HttpPost]
        public string UpdateFaultyItem(FAULTYITEMS fAULITEMS)
        {
            bool result = false;
            try
            {
                if (fAULITEMS.LOCATION.Contains("$"))
                    fAULITEMS.LOCATION = fAULITEMS.LOCATION.Split('$')[1].Trim();

                if (fAULITEMS.STAGE == "FAULTY_SUMKT")
                {
                    result = ExecuteSMFaultyItems(fAULITEMS);
                }
                else if (fAULITEMS.STAGE == "FAULTY_BLK")
                {
                    result = ExecuteBSFaultyItems(fAULITEMS);
                }
                if (result)
                {
                    return "OK";
                }
                else
                {
                    return "SOMETHING WENT WRONG !!";
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }

        public bool ExecuteSMFaultyItems(FAULTYITEMS fAULITEMS)
        {
            bool result = false;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"INSERT INTO XXES_QC_LOG (PLANT_CODE,FAMILY_CODE,ITEMCODE,LAST_QTY,REJ_QTY,
                                          REMARKS,CREATEDBY,CREATEDDATE,TYPE,STAGE,LOCATION) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',
                                          SYSDATE,'{7}','{8}','{9}')", fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.ITEMCODE, fAULITEMS.LASTQTY,
                                       fAULITEMS.RECQTY, fAULITEMS.REASON, fAULITEMS.CREATEDBY, fAULITEMS.TRANSACTIONTYPE, fAULITEMS.STAGE, fAULITEMS.LOCATION);
                    command.ExecuteNonQuery();

                    if (fAULITEMS.TRANSACTIONTYPE == "ISSUE")
                    {
                        command.CommandText = string.Format(@"UPDATE XXES_SUMMKTSTOCK SET QUANTITY = to_number(nvl(QUANTITY,0)) - {0} 
                                            WHERE PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND LOCATION_CODE = '{3}' AND ITEMCODE = '{4}'",
                                            Convert.ToInt32(fAULITEMS.RECQTY), fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.LOCATION, fAULITEMS.ITEMCODE);
                        command.ExecuteNonQuery();
                    }
                    if (fAULITEMS.TRANSACTIONTYPE == "RETURN")
                    {
                        command.CommandText = string.Format(@"UPDATE XXES_SUMMKTSTOCK SET QUANTITY = to_number(nvl(QUANTITY,0)) + {0} 
                                            WHERE PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND LOCATION_CODE = '{3}' AND ITEMCODE = '{4}'",
                                            Convert.ToInt32(fAULITEMS.RECQTY), fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.LOCATION, fAULITEMS.ITEMCODE);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    LogWrite(ex);
                    result = false;
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }
            }


            return result;
        }
        public bool ExecuteBSFaultyItems(FAULTYITEMS fAULITEMS)
        {
            bool result = false;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"INSERT INTO XXES_QC_LOG (PLANT_CODE,FAMILY_CODE,ITEMCODE,LAST_QTY,REJ_QTY,
                                          REMARKS,CREATEDBY,CREATEDDATE,TYPE,STAGE,LOCATION) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',
                                          SYSDATE,'{7}','{8}','{9}')", fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.ITEMCODE, fAULITEMS.LASTQTY,
                                       fAULITEMS.RECQTY, fAULITEMS.REASON, fAULITEMS.CREATEDBY, fAULITEMS.TRANSACTIONTYPE, fAULITEMS.STAGE, fAULITEMS.LOCATION);
                    command.ExecuteNonQuery();

                    if (fAULITEMS.TRANSACTIONTYPE == "ISSUE")
                    {
                        command.CommandText = string.Format(@"UPDATE XXES_BULKSTOCK SET QUANTITY = to_number(nvl(QUANTITY,0)) - {0} 
                                           WHERE PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND LOCATION_CODE = '{3}' AND ITEMCODE = '{4}'",
                                            Convert.ToInt32(fAULITEMS.RECQTY), fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.LOCATION, fAULITEMS.ITEMCODE);
                        command.ExecuteNonQuery();
                    }
                    if (fAULITEMS.TRANSACTIONTYPE == "RETURN")
                    {
                        command.CommandText = string.Format(@"UPDATE XXES_BULKSTOCK SET QUANTITY = to_number(nvl(QUANTITY,0)) + {0} 
                                           WHERE PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}' AND LOCATION_CODE = '{3}' AND ITEMCODE = '{4}'",
                                            Convert.ToInt32(fAULITEMS.RECQTY), fAULITEMS.PLANT, fAULITEMS.FAMILY, fAULITEMS.LOCATION, fAULITEMS.ITEMCODE);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    LogWrite(ex);
                    result = false;
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {

                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }
            }


            return result;
        }

        [HttpPost]
        public string GetPendingKanbanFromLocation(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            string response = string.Empty;
            if (cOMMONDATA.LOCATION.Contains("$"))
                cOMMONDATA.LOCATION = cOMMONDATA.LOCATION.Split('$')[1].Trim();
            try
            {
                query = string.Format(@"select KANBANNO Code,KANBANNO Text from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and location_code='{2}' and CREATEDBY='{3}'",
                            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.LOCATION, cOMMONDATA.CREATEDBY
                    );
                dataTable = returnDataTable(query);
                response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {

                dataTable = errorTable(ex.Message);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return response;
        }
        [HttpPost]
        public string GetPendingKanbanFromLocationNew(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            string response = string.Empty;
            if (cOMMONDATA.LOCATION.Contains("$"))
                cOMMONDATA.LOCATION = cOMMONDATA.LOCATION.Split('$')[1].Trim();
            try
            {
                query = string.Format(@"select KANBANNO Code,KANBANNO || '(QTY: ' || K.QUANTITY || ' ID=' || K.BLK_AUTOID || ')' Text from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and location_code='{2}' and createdby='{3}'",
                            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.LOCATION, cOMMONDATA.CREATEDBY.Trim().ToUpper()
                    );
                dataTable = returnDataTable(query);
                response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {

                dataTable = errorTable(ex.Message);
                response = JsonConvert.SerializeObject(dataTable);
            }
            return response;
        }
        [HttpPost]
        public string GetQtyForKanban(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                query = string.Format(@"select nvl(sum(QUANTITY),0) from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and KANBANNO='{2}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, cOMMONDATA.DATA);
                response = GetColValueOra(query);
                if (!string.IsNullOrEmpty(response))
                {
                    response = "OK#" + response;
                }
                else
                {
                    response = "OK#0";
                }
            }
            catch (Exception ex)
            {
                response = "ERROR:" + ex.Message;
            }
            return response;
        }

        [HttpPost]
        public string GetQtyForKanbanNew(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                string kanban = string.Empty, autoid = string.Empty;
                kanban = cOMMONDATA.DATA.Split('(')[0].Trim();
                autoid = cOMMONDATA.DATA.Split('=')[1].Split(')')[0].Trim();
                if (string.IsNullOrEmpty(kanban))
                {
                    return "ERROR: SELECT VALID KANBAN";

                }
                if (string.IsNullOrEmpty(autoid))
                {
                    return "ERROR: ID NOT FOUND";
                }
                query = string.Format(@"select nvl(sum(QUANTITY),0) from XXES_KANBANPKLIST k where k.plant_code='{0}' and k.family_code='{1}'
                            and status='PENDING' and KANBANNO='{2}'", cOMMONDATA.PLANT, cOMMONDATA.FAMILY, kanban.Trim().ToUpper());
                response = GetColValueOra(query);
                if (!string.IsNullOrEmpty(response))
                {
                    response = "OK#" + response;
                }
                else
                {
                    response = "OK#0";
                }
            }
            catch (Exception ex)
            {
                response = "ERROR:" + ex.Message;
            }
            return response;
        }




        //[HttpPost]
        //public string MRNSNoInsert(COMMONDATA CD)
        //{
        //    string query = string.Empty, response = string.Empty, Message = string.Empty;
        //    try
        //    {
        //        query = string.Format(@"SELECT COUNT(*) FROM XXES_MRNSRNO WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND SRNO='{2}'",
        //            CD.PLANT, CD.FAMILY, CD.DATA);

        //        if (CheckExitsOra(query))
        //        {
        //            query = string.Format(@"UPDATE XXES_MRNSRNO SET SCANDATE=sysdate , SCANBY='{0}' WHERE PLANT_CODE='{1}' AND FAMILY_CODE='{2}'
        //            AND SRNO='{3}'", CD.CREATEDBY, CD.PLANT, CD.FAMILY, CD.DATA);
        //            ExecQueryOra(query);

        //            query = string.Format(@"SELECT ITEMCODE || '#' || DESCRIPTION   FROM XXES_MRNSRNO WHERE SRNO='{0}'", CD.DATA);

        //            Message = GetColValueOra(query);

        //            return "OK ." + Message;
        //        }
        //        else
        //        {
        //            return "ERROR : SRNo Not Found";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        return "ERROR:" + ex.Message;
        //    }

        //}
        [HttpPost]
        public string MRNSNoInsert(COMMONDATA CD)
        {
            string query = string.Empty, response = string.Empty, Message = string.Empty, Count = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_MRNSRNO WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND SRNO='{2}'",
                    CD.PLANT, CD.FAMILY, CD.DATA);

                if (CheckExitsOra(query))
                {
                    query = string.Format(@"SELECT SCANBY FROM XXES_MRNSRNO WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND SRNO='{2}'",
                        CD.PLANT, CD.FAMILY, CD.DATA);

                    Count = Convert.ToString(GetColValueOra(query));
                    if (string.IsNullOrEmpty(Count))
                    {
                        query = string.Format(@"UPDATE XXES_MRNSRNO SET SCANDATE=sysdate , SCANBY='{0}' WHERE PLANT_CODE='{1}' AND FAMILY_CODE='{2}'
                        AND SRNO='{3}'", CD.CREATEDBY, CD.PLANT, CD.FAMILY, CD.DATA);
                        ExecQueryOra(query);
                        query = string.Format(@"SELECT ITEMCODE || '#' || DESCRIPTION   FROM XXES_MRNSRNO WHERE SRNO='{0}'", CD.DATA);
                        Message = GetColValueOra(query);
                        return "OK ." + Message;
                    }
                    else
                    {
                        return "Already Scaned..";
                    }
                }
                else
                {
                    return "ERROR : SRNo Not Found";
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "ERROR:" + ex.Message;
            }

        }

    }

}
