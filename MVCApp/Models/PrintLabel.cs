using MVCApp.Common;
using MVCApp.CommonFunction;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace MVCApp.Models
{
    public class PrintLabel
    {
        Function fun = new Function();
        PrintAssemblyBarcodes af = new PrintAssemblyBarcodes();
        public bool PrintPDIOK(Tractor tractor, int copies, string mode,string ip,int port,string stage)
        {
            string fileData = string.Empty;bool Status;
            string prnfilename = string.Empty, barcode = string.Empty, AppPath = string.Empty;
            try
            {
                PrinterSettings pd = new PrinterSettings();
                if (!fun.CheckMyPrinter(pd.PrinterName) && mode.Equals("LOCAL"))
                {
                    //MessageBox.Show("Printer must be online or default printer should be barcode printer to print the barcode", "Barcoding System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                AppPath = System.IO.Directory.GetCurrentDirectory();
                prnfilename = "PDI.txt";
                if (!File.Exists(AppPath + "\\" + prnfilename))
                {
                    //MessageBox.Show(prnfilename + " file doesn't exists");
                    return false;
                }
                StreamReader sread = null;
                sread = File.OpenText(AppPath + "\\" + prnfilename);
                while (sread.EndOfStream == false)
                {
                    fileData = sread.ReadToEnd();
                }
                barcode = fileData;

                for (int i = 0; i < copies; i++)
                {
                    fileData = barcode;
                    fileData = fileData.Replace("TSN", tractor.TSN);
                    fileData = fileData.Replace("JOB_ID", tractor.JOB);
                    fileData = fileData.Replace("ITEM_CODE", tractor.ITEMCODE);
                    fileData = fileData.Replace("ITEM_DESC", tractor.DESC);
                    fileData = fileData.Replace("AVG_HOURS", tractor.avgHours);
                    fileData = fileData.Replace("PDI_DATE", tractor.pdidate);
                    af.WriteDataToLabelFile(fileData);
                    if (mode.Equals("LOCAL"))
                    {
                        Status = af.PrintStandardLabel(fileData, stage, tractor.PLANT, tractor.FAMILY);
                       
                    }
                    else
                    {
                        if (af.PrintLabelViaNetwork(fileData, "",ip, port))
                        {
                            Status = true;
                        }
                    }
                }
                fun.Insert_Into_ActivityLog("PDI_STICKER", "PDI_STICKER", tractor.TSN, Convert.ToString(HttpContext.Current.Session["IPADDR"]),
                  Convert.ToString(HttpContext.Current.Session["Login_Unit"]), Convert.ToString(HttpContext.Current.Session["LoginFamily"]) );
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }
        //public bool PrintCraneEngine(CraneBarcode craneBarcode, int copies, string mode)
        //{
        //    string fileData = string.Empty; string prnfilename = string.Empty, barcode = string.Empty, AppPath = string.Empty;
        //    try
        //    {
        //        PrintDialog pd = new PrintDialog();
        //        if (!pf.CheckMyPrinter(pd.PrinterSettings.PrinterName) && mode.Equals("LOCAL"))
        //        {
        //            MessageBox.Show("Printer must be online or default printer should be barcode printer to print the barcode", "Barcoding System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return false;
        //        }
        //        AppPath = Application.StartupPath;
        //        prnfilename = "CraneEngine.txt";
        //        if (!File.Exists(AppPath + "\\" + prnfilename))
        //        {
        //            MessageBox.Show(prnfilename + " file doesn't exists");
        //            return false;
        //        }
        //        StreamReader sread = null;
        //        sread = File.OpenText(AppPath + "\\" + prnfilename);
        //        while (sread.EndOfStream == false)
        //        {
        //            fileData = sread.ReadToEnd();
        //        }
        //        barcode = fileData;

        //        for (int i = 0; i < copies; i++)
        //        {
        //            fileData = barcode;
        //            fileData = fileData.Replace("MODEL_NO", craneBarcode.SHORTNAME);
        //            fileData = fileData.Replace("CHASIS_NO", craneBarcode.CHASISNO);
        //            fileData = fileData.Replace("ENGINE_NO", craneBarcode.ENGINESRNO);
        //            fileData = fileData.Replace("CF_CODE", craneBarcode.CFCode);
        //            //fileData = fileData.Replace("MONTH_CODE", pf.GetServerDateTime().ToString("MM/yyyy"));
        //            fileData = fileData.Replace("MONTH_CODE", craneBarcode.QRMONTHCODE);
        //            pf.WriteDataToLabelFile(fileData);
        //            if (mode.Equals("LOCAL"))
        //            {
        //                if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
        //                {
        //                    //  Status = true;
        //                }
        //            }
        //        }
        //        //else
        //        //{
        //        //    if (PrintLabelViaNetwork(fileData, PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
        //        //    {
        //        //        Status = true;
        //        //    }
        //        //}
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return true;
        //}
        //public bool GenerateChasisJobBarcode(CraneBarcode craneBarcode, int copies, string mode)
        //{
        //    string fileData = string.Empty; string prnfilename = string.Empty, barcode = string.Empty, AppPath = string.Empty;
        //    try
        //    {
        //        PrintDialog pd = new PrintDialog();
        //        if (!pf.CheckMyPrinter(pd.PrinterSettings.PrinterName) && mode.Equals("LOCAL"))
        //        {
        //            MessageBox.Show("Printer must be online or default printer should be barcode printer to print the barcode", "Barcoding System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return false;
        //        }
        //        AppPath = Application.StartupPath;
        //        prnfilename = "CRANEJOB.txt";
        //        if (!File.Exists(AppPath + "\\" + prnfilename))
        //        {
        //            MessageBox.Show(prnfilename + " file doesn't exists");
        //            return false;
        //        }
        //        StreamReader sread = null;
        //        sread = File.OpenText(AppPath + "\\" + prnfilename);
        //        while (sread.EndOfStream == false)
        //        {
        //            fileData = sread.ReadToEnd();
        //        }
        //        barcode = fileData;

        //        for (int i = 0; i < copies; i++)
        //        {
        //            fileData = barcode;
        //            fileData = fileData.Replace("CHASIS_NO", craneBarcode.CHASISNO);
        //            fileData = fileData.Replace("JOBID", craneBarcode.JOBID);
        //            fileData = fileData.Replace("ENGINE_NO", craneBarcode.ENGINESRNO);
        //            fileData = fileData.Replace("CF_CODE", craneBarcode.CFCode);
        //            pf.WriteDataToLabelFile(fileData);
        //            if (mode.Equals("LOCAL"))
        //            {
        //                if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
        //                {
        //                    //  Status = true;
        //                }
        //            }
        //        }
        //        //else
        //        //{
        //        //    if (PrintLabelViaNetwork(fileData, PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
        //        //    {
        //        //        Status = true;
        //        //    }
        //        //}
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return true;
        //}
        //public bool GenerateChasisBarcode(CraneBarcode craneBarcode, int copies, string mode)
        //{
        //    string fileData = string.Empty; string prnfilename = string.Empty, barcode = string.Empty, AppPath = string.Empty;
        //    try
        //    {
        //        PrintDialog pd = new PrintDialog();
        //        if (!pf.CheckMyPrinter(pd.PrinterSettings.PrinterName) && mode.Equals("LOCAL"))
        //        {
        //            MessageBox.Show("Printer must be online or default printer should be barcode printer to print the barcode", "Barcoding System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return false;
        //        }
        //        AppPath = Application.StartupPath;
        //        prnfilename = "Chasis.txt";
        //        if (!File.Exists(AppPath + "\\" + prnfilename))
        //        {
        //            MessageBox.Show(prnfilename + " file doesn't exists");
        //            return false;
        //        }
        //        StreamReader sread = null;
        //        sread = File.OpenText(AppPath + "\\" + prnfilename);
        //        while (sread.EndOfStream == false)
        //        {
        //            fileData = sread.ReadToEnd();
        //        }
        //        barcode = fileData;

        //        for (int i = 0; i < copies; i++)
        //        {
        //            fileData = barcode;
        //            fileData = fileData.Replace("CHASIS_NO", craneBarcode.CHASISNO);
        //            //if (!string.IsNullOrEmpty(craneBarcode.DESCRIPTION))
        //            //{
        //            //    if (craneBarcode.DESCRIPTION.Trim().Length > 40)
        //            //        fileData = fileData.Replace("MODEL_NO", craneBarcode.DESCRIPTION.Substring(0, 40));
        //            //    else
        //            //        fileData = fileData.Replace("MODEL_NO", craneBarcode.DESCRIPTION.Trim());
        //            //}
        //            fileData = fileData.Replace("MODEL_NO", craneBarcode.SHORTNAME.Trim());
        //            fileData = fileData.Replace("CF_CODE", craneBarcode.CFCode);
        //            pf.WriteDataToLabelFile(fileData);
        //            if (mode.Equals("LOCAL"))
        //            {
        //                if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
        //                {
        //                    //  Status = true;
        //                }
        //            }
        //        }
        //        //else
        //        //{
        //        //    if (PrintLabelViaNetwork(fileData, PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
        //        //    {
        //        //        Status = true;
        //        //    }
        //        //}
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return true;
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
        //        pf.WriteDataToLabelFile(fileData);
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
