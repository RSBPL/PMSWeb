using MVCApp.Common;
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
    public class GateEntryController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        GateEntryFunction GEFun = new GateEntryFunction();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "", LoginStageCode = "", Login_User="";
        // GET: FamilyMaster

        [HttpGet]
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.value = DateTime.Now;
                return View();
            }
        }
        [HttpPost]
        public JsonResult Grid(GateEntryModel obj)
        {

            int recordsTotal = 0;
            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            List<GateEntryModel> gateEntryDetails = fun.GridGateEntryData(obj);
            if (gateEntryDetails.Count > 0)
            {
                recordsTotal = gateEntryDetails[0].TOTALCOUNT;
            }

            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = gateEntryDetails }, JsonRequestBehavior.AllowGet);

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
        public ActionResult PRINT(List<GateEntryModel> data)
        {

            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; int tranid = 0;
            MRNInvoice invoice = new MRNInvoice();
            string retvalue = string.Empty;
            bool rowFound = false;
            int totPrint = 0;
            string line = string.Empty;
            DateTime printtime = new DateTime();
            List<string> lstInvoice = new List<string>();
            bool copy2 = (data[0].CheckboxPrint2Label ? true : false);
            foreach (var item in data)
            {
                rowFound = true;
                if (string.IsNullOrEmpty(item.VEHICLE_NO))
                {
                    return Json("1", JsonRequestBehavior.AllowGet);
                }

                invoice.PLANT_CODE = item.ORGANIZATION_CODE;
                invoice.MRN_NO = item.MRN_NO;
                if (!string.IsNullOrEmpty(invoice.MRN_NO))
                {
                    string status1 = string.Empty;
                    status1 = GEFun.CheckduplicateInvoiceInMRN(invoice.PLANT_CODE, invoice.MRN_NO);
                    if (!string.IsNullOrEmpty(status1))
                    {
                        //string pass = Message.sa("Duplicate invoice found. Enter password to continue..\nMRN= " + invoice.MRN_NO + "\nInvoice=" + invoice.INVOICE_NO,
                        //     "Duplicate Invoice", true);
                        //if (string.IsNullOrEmpty(pass))
                        //{
                        //    //MessageBox.Show("Invalid Password");
                        //    //return;
                        //    return Json("2", JsonRequestBehavior.AllowGet);
                        //}
                        //else if (!fun.CheckExits("select count(*) from xxes_stage_master where ad_password='" + pass.Trim() + "' and offline_keycode='" + LoginStageCode.Trim() + "' and family_code='" + Convert.ToString(item.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(item.ORGANIZATION_CODE) + "'"))
                        //{
                        //    //MessageBox.Show("Invalid Password", PubFun.AppName);
                        //    //return Json();
                        //    return Json("2", JsonRequestBehavior.AllowGet);
                        //}
                        //retvalue += status;
                        //continue;
                        return Json("2", JsonRequestBehavior.AllowGet);
                    }
                }
                invoice.VENDOR_CODE = item.VENDOR_CODE;
                invoice.VENDOR_NAME = item.VENDOR_NAME;
                invoice.INVOICE_NO = item.INVOICE_NO;
                invoice.INVOICE_DATE = item.INVOICE_DATE;
                invoice.VEHICLE_NO = item.VEHICLE_NO;
                invoice.STATUS = item.STATUS;
                invoice.CITY = item.CITY;
                invoice.TOTAL_ITEM = item.TOTAL_ITEM;
                line = item.ITEM;
                if (line.Contains("["))
                {
                    invoice.ITEM_CODE = line.Split('[')[1].Split(']')[0].Trim();
                    invoice.ITEM_DESCRIPTION = line.Split('[')[0].Trim();
                    invoice.ERPCODE = invoice.ITEM_CODE;
                }
                if(invoice.ITEM_CODE.Length > 9)
                {
                    invoice.ITEM_CODE = invoice.ITEM_CODE.Substring(0, 9);
                }
                
                invoice.STORE_LOCATION = GEFun.GetStoreLoc(invoice.ITEM_CODE, invoice.PLANT_CODE);
                invoice.SAMPLE = "";// Convert.ToString(gridView1.GetRowCellValue(i, "SAMPLE"));
                invoice.SOURCE_TYPE = item.SOURCE_TYPE;
                invoice.TRANSACTION_DATE = Convert.ToDateTime(item.TRANSACTION_DATE);
                GEFun.UpdateMrnDetails(invoice.PLANT_CODE, invoice.MRN_NO,item.FAMILYCODE,invoice.ERPCODE);
                lstInvoice.Add(invoice.INVOICE_NO);
                PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                if (item.CheckboxReprint==false) //if printing
                {
                    query = "select count(*) from ITEM_RECEIPT_DETIALS where mrn_no='" + invoice.MRN_NO + "'";
                    if (!fun.CheckExits(query))
                    {
                        GEFun.UpdateRawMeterialMaster(invoice,item.FAMILYCODE);
                        if (!string.IsNullOrEmpty(invoice.VEHICLE_NO))
                        {
                            tranid = GetTransactionID(invoice);
                        }
                        query = "insert into ITEM_RECEIPT_DETIALS(PLANT_CODE,MRN_NO ,TRANSACTION_DATE ,SOURCE_DOCUMENT_CODE ,VENDOR_CODE ,VENDOR_NAME ,INVOICE_NO  ,VEHICLE_NO ,ITEM_CODE ," +
                            "ITEM_DESCRIPTION ,PRINTED_ON,createdby,status,storage,TOTAL_ITEM,family_code,tranid,CITY) " +
                            "values('" + invoice.PLANT_CODE + "','" + invoice.MRN_NO + "',TO_DATE('" + invoice.TRANSACTION_DATE.ToString("MM/dd/yyyy HH:mm:ss") + "', 'MM/DD/YYYY HH24:MI:SS'),'" + invoice.SOURCE_TYPE + "','" +
                            invoice.VENDOR_CODE + "','" + invoice.VENDOR_NAME + "','" + invoice.INVOICE_NO + "','" + invoice.VEHICLE_NO + "','" + invoice.ITEM_CODE + "','" +
                              GEFun.replaceApostophi(invoice.ITEM_DESCRIPTION) + "',sysdate,'" + Login_User + "','" + invoice.STATUS + "','" + invoice.STORE_LOCATION + "'," + invoice.TOTAL_ITEM + ",'" + Convert.ToString(item.FAMILYCODE) + "','" + tranid + "','" + invoice.CITY + "')";
                        if (fun.EXEC_QUERY(query))
                        {
                            GEFun.UpdateMrnDetails(invoice.PLANT_CODE, invoice.MRN_NO,item.FAMILYCODE, invoice.ERPCODE);
                            if (!copy2)
                            {
                                for (int ii = 0; ii < 2; ii++)
                                {
                                    if (barcodes.GenerateGateBarcode("LOCAL", "", "", invoice, printtime, copy2,item.FAMILYCODE))
                                    {
                                        totPrint++;
                                    }
                                }
                            }
                            else
                            {
                                if (barcodes.GenerateGateBarcode("LOCAL", "", "", invoice, printtime, copy2, item.FAMILYCODE))
                                {
                                    totPrint++;
                                }
                            }
                        }
                    }
                }
                else if (item.CheckboxReprint == true)
                {
                    GEFun.UpdateRawMeterialMaster(invoice, item.FAMILYCODE);
                    query = GEFun.queryget(invoice.MRN_NO);// and invoice_no='" + invoice.INVOICE_NO + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        if (barcodes.GenerateGateBarcode("LOCAL", "", "", invoice, printtime, copy2, item.FAMILYCODE))
                        {
                            totPrint++;
                        }
                    }
                }

            }
            msg = "Saved successfully...";
            mstType = "alert-success";
            status = "success";
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult OracleUpdate(List<GateEntryModel> data)
        {

            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            MRNInvoice invoice = new MRNInvoice();
            string retvalue = string.Empty;
            bool rowFound = false;
            int totPrint = 0;
            string line = string.Empty;
            DateTime printtime = new DateTime();
            List<string> lstInvoice = new List<string>();
            bool copy2 = (data[0].CheckboxPrint2Label ? true : false);
            foreach (var item in data)
            {
                invoice.MRN_NO = Convert.ToString(item.MRN_NO);
                invoice.VEHICLE_NO = Convert.ToString(item.VEHICLE_NO);
                invoice.VENDOR_CODE = Convert.ToString(item.VENDOR_CODE);
                invoice.VENDOR_NAME = Convert.ToString(item.VENDOR_NAME);
                invoice.INVOICE_NO = Convert.ToString(item.INVOICE_NO);
                invoice.INVOICE_DATE = Convert.ToString(item.INVOICE_DATE);
                invoice.STATUS = Convert.ToString(item.STATUS);
                invoice.TOTAL_ITEM = Convert.ToString(item.TOTAL_ITEM);
                line = Convert.ToString(item.ITEM);
                if (line.Contains("["))
                {
                    invoice.ITEM_CODE = line.Split('[')[1].Split(']')[0].Trim();
                    invoice.ITEM_DESCRIPTION = line.Split('[')[0].Trim();
                }
                invoice.STORE_LOCATION = GEFun.GetStoreLoc(invoice.ITEM_CODE, invoice.PLANT_CODE);
                invoice.SAMPLE = "";// Convert.ToString(gridView1.GetRowCellValue(i, "SAMPLE"));
                invoice.SOURCE_TYPE = Convert.ToString(item.SOURCE_TYPE);
                invoice.TRANSACTION_DATE = Convert.ToDateTime(item.TRANSACTION_DATE);
                query = "update ITEM_RECEIPT_DETIALS set TRANSACTION_DATE =TO_DATE('" + invoice.TRANSACTION_DATE.ToString("MM/dd/yyyy HH:mm:ss") + "', 'MM/DD/YYYY HH24:MI:SS')," +
                    " SOURCE_DOCUMENT_CODE='" + invoice.SOURCE_TYPE + "',VENDOR_CODE='" + invoice.VENDOR_CODE + "',VENDOR_NAME='" + invoice.VENDOR_NAME + "',INVOICE_NO='" + invoice.INVOICE_NO + "'," +
                    "VEHICLE_NO='" + invoice.VEHICLE_NO + "',ITEM_CODE='" + invoice.ITEM_CODE + "'," +
                    "ITEM_DESCRIPTION='" + invoice.ITEM_DESCRIPTION + "',status='" + invoice.STATUS + "',storage='" + invoice.STORE_LOCATION + "',TOTAL_ITEM='" + invoice.TOTAL_ITEM + "'  where mrn_no='" + invoice.MRN_NO + "'";// and invoice_no='" + invoice.INVOICE_NO + "'";
                if (fun.EXEC_QUERY(query))
                {
                    totPrint++;
                }

            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            if (totPrint > 0)
            {
                msg = " "+ totPrint.ToString() + "  Rows Updated Successfully !!";
                mstType = "alert-success";
                status = "success";
                result = new { Msg = msg, ID = mstType, validation = status };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                msg = " " + totPrint.ToString() + "  Row Updated !!";
                mstType = "alert-success";
                status = "success";
                result = new { Msg = msg, ID = mstType, validation = status };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            msg = " ERROR !!";
            mstType = "alert-success";
            status = "error";
            result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private int GetTransactionID(MRNInvoice mRNInvoice)
        {
            int tranid = 0;
            try
            {
                query = string.Format(@"SELECT trunc(nvl((SYSDATE-A.PRINTED_ON)*1440,0)) PRINTED_ON,TRANID  FROM 
                    (
                    SELECT I.PRINTED_ON,TRANID FROM ITEM_RECEIPT_DETIALS i WHERE I.VEHICLE_NO='{0}' AND I.TRANSACTION_DATE IS NOT NULL
                    AND TRUNC(I.PRINTED_ON)=TRUNC(SYSDATE) 
                    ORDER BY I.PRINTED_ON DESC
                    )a WHERE ROWNUM=1", mRNInvoice.VEHICLE_NO);
                DataTable dataTable = fun.returnDataTable(query);
                if (dataTable.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(dataTable.Rows[0]["PRINTED_ON"])))
                        tranid = 1;
                    else
                    {
                        if (fun.IsNumeric(Convert.ToString(dataTable.Rows[0]["PRINTED_ON"])))
                        {
                            if (Convert.ToInt32(dataTable.Rows[0]["PRINTED_ON"]) <= 60)
                            {
                                if (string.IsNullOrEmpty(Convert.ToString(dataTable.Rows[0]["TRANID"])))
                                    tranid = 1;
                                else
                                {
                                    if (fun.IsNumeric(Convert.ToString(dataTable.Rows[0]["TRANID"])))
                                        tranid = Convert.ToInt32(dataTable.Rows[0]["TRANID"]);
                                    else
                                        tranid = 1;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(Convert.ToString(dataTable.Rows[0]["TRANID"])))
                                    tranid = 1;
                                else
                                {
                                    if (fun.IsNumeric(Convert.ToString(dataTable.Rows[0]["TRANID"])))
                                        tranid = Convert.ToInt32(dataTable.Rows[0]["TRANID"]) + 1;
                                    else
                                        tranid = 1;
                                }
                            }
                        }
                        else
                            tranid = 1;
                    }
                }
                else
                {
                    tranid = 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return tranid;
        }

        //private string CheckduplicateInvoiceInMRN(string PLANT_CODE, string MRN_NO)
        //{
        //    string retvalue = string.Empty;
        //    try
        //    {

        //        query = string.Format(@"select ORGANIZATION_CODE,MRN_NO,INVOICE_NO,ITEM_CODE,ITEM_DESCRIPTION,QUANTITY,UOM,RATE,STATUS,
        //        INVOICE_DATE INVOICE_DATE,ITEM_REVISION,VENDOR_CODE from apps.XXESGATETAGPRINT_BARCODE WHERE  
        //        ORGANIZATION_CODE='{0}' AND MRN_NO='{1}' and item_code not in 
        //         (select item_code from xxes_mrninfo where plant_code='{0}' and mrn_no='{1}')", PLANT_CODE, MRN_NO);
        //        using (DataTable dt = fun.returnDataTable(query))
        //        {
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                if (fun.isInvoiceExistsinFinancialYear(Convert.ToString(dr["INVOICE_NO"]), Convert.ToString(dr["VENDOR_CODE"])))
        //                {
        //                    if (string.IsNullOrEmpty(retvalue))
        //                    {
        //                        retvalue = "DUPLICATE INVOICES FOUND IN MRN " + MRN_NO + " INV ARE : ";
        //                    }
        //                    else
        //                        retvalue += Convert.ToString(dr["INVOICE_NO"]) + ",";
        //                }

        //            }
        //        }


        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return retvalue;
        //}

        //public bool GenerateGateBarcode(string mode, string PRINTER_IP, string PRINTER_PORT, MRNInvoice invoice, DateTime printtime, bool copy2)
        //{
        //    string fileData = string.Empty; bool Status = false; string prnfilename = string.Empty, barcode = string.Empty, AppPath = string.Empty;
        //    try
        //    {
        //        PrintDialog pd = new PrintDialog();
        //        //if (!pf.CheckMyPrinter(pd.PrinterSettings.PrinterName) && mode.Equals("LOCAL"))
        //        //{
        //        //    MessageBox.Show("Printer must be online or default printer should be barcode printer to print the barcode", "Barcoding System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        //    return false;
        //        //}
        //        AppPath = Application.StartupPath;
        //        //if (copy2)
        //        prnfilename = "Gate2.prn";
        //        //else
        //        //    prnfilename = "Gate4.prn";
        //        if (!File.Exists(AppPath + "\\" + prnfilename))
        //        {
        //            MessageBox.Show("Gate.prn file doesn't exists");
        //            return false;
        //        }
        //        StreamReader sread = null;
        //        sread = File.OpenText(AppPath + "\\" + prnfilename);
        //        while (sread.EndOfStream == false)
        //        {
        //            fileData = sread.ReadToEnd();
        //        }
        //        fileData = fileData.Replace("MRN_NO", invoice.MRN_NO);
        //        fileData = fileData.Replace("IN_TIME", "  " + invoice.TRANSACTION_DATE.ToString("HH:mm"));
        //        fileData = fileData.Replace("IN_DATE", invoice.TRANSACTION_DATE.ToString("dd/MM/yy"));
        //        fileData = fileData.Replace("INVOICE_NUMBER", invoice.INVOICE_NO);
        //        fileData = fileData.Replace("TRUCK_NO", invoice.VEHICLE_NO);
        //        if (invoice.SOURCE_TYPE.Equals("REQ"))
        //            fileData = fileData.Replace("COMPANY_NAME", "ESCORTS");
        //        else
        //        {
        //            if (invoice.VENDOR_NAME.Length > 22)
        //                invoice.VENDOR_NAME = invoice.VENDOR_NAME.Substring(0, 22);
        //            fileData = fileData.Replace("COMPANY_NAME", invoice.VENDOR_NAME + "-" + invoice.VENDOR_CODE);
        //        }

        //        if (invoice.STATUS.Equals("QA"))
        //            fileData = fileData.Replace("LQ", "Q");
        //        else
        //            fileData = fileData.Replace("LQ", "");
        //        fileData = fileData.Replace("VEHICLE_NUMBER", invoice.VEHICLE_NO);
        //        fileData = fileData.Replace("PART_CODE", invoice.ITEM_CODE + " " + "+" + (Convert.ToInt16(invoice.TOTAL_ITEM) - 1).ToString());
        //        fileData = fileData.Replace("MU", invoice.STORE_LOCATION);
        //        fileData = fileData.Replace("SAMPLE", invoice.SAMPLE);
        //        fun.WriteDataToLabelFile(fileData);
        //        if (mode.Equals("LOCAL"))
        //        {
        //            if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
        //            {
        //                Status = true;
        //            }
        //        }
        //        //else
        //        //{
        //        //    if (PrintLabelViaNetwork(fileData, PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
        //        //    {
        //        //        Status = true;
        //        //    }
        //        //}
        //        return Status;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {

        //    }

        //}
    }
}