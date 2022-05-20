using MVCApp.CommonFunction;
using MVCApp.Controllers.DCU;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;

namespace MVCApp.Common
{
    public class PrintAssemblyBarcodes
    {
        Assemblyfunctions af = null;
        Function fun = new Function();
        public static string PRINT_QA = Convert.ToString(ConfigurationManager.AppSettings["PRINT_QA"]);
        public string ReadFile(string Filename)
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~//Printer//" + Filename.Trim());
                if (!File.Exists(path))
                {
                    throw new Exception("Print file not found");
                    return "";
                }
                StreamReader sread = null; string line = "";
                sread = File.OpenText(path);
                while (sread.EndOfStream == false)
                {
                    line = sread.ReadToEnd();
                }
                if (sread != null)
                {
                    sread.Dispose();
                    sread.Close();
                }

                return line.Trim();
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }
        public bool PrintMRNQualityBarcodes(List<BOXBARCODE> bOXBARCODEs)
        {
            bool status = false; string line = string.Empty, vendorcode = string.Empty, itemdesc = string.Empty,
                  cmd1 = "", cmd2 = "", qastatus = string.Empty;
            try
            {
                foreach (BOXBARCODE bOXBARCODE in bOXBARCODEs)
                {
                    //if (PRINT_QA != "Y")
                    //    return false;
                    string query = string.Format(@"select i.vendor_name || ' # ' || m.item_description details from xxes_mrninfo m 
                                                     inner join item_receipt_detials i on m.mrn_no = i.mrn_no
                                                    where m.mrn_no = '{0}' and m.itemcode = '{1}'",
                                                        bOXBARCODE.MRN.Trim(), bOXBARCODE.ITEMCODE.Trim());

                    line = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        vendorcode = line.Split('#')[0].Trim();
                        itemdesc = line.Split('#')[1].Trim();
                        if (itemdesc.Trim().Length > 40)
                        {
                            itemdesc = itemdesc.Substring(0, 40);
                        }
                    }
                    string filename = "QA.prn";
                    //for (int i = 0; i < 2; i++)
                    //{
                    //if (i == 0)
                    //    qastatus = "PASS";
                    //else
                    //    qastatus = "FAIL";
                    query = ReadFile(filename);
                    if (!string.IsNullOrEmpty(query))
                    {
                        query = query.Replace("MRN_NO", bOXBARCODE.MRN);
                        query = query.Replace("VENDOR_CODE", vendorcode);
                        query = query.Replace("ITEM_CODE", bOXBARCODE.ITEMCODE);
                        query = query.Replace("ITEM_DESC", itemdesc);
                        query = query.Replace("PRINT_DATE", fun.GetServerDateTime().ToString("dd/MM/yyyy HH:mm:ss"));
                        query = query.Replace("QA_STATUS", qastatus);
                        query = query.Replace("BOX_QTY", bOXBARCODE.RECQTY);
                        cmd1 += "\n" + query + "\n";
                    }
                    // }

                }
                if (!string.IsNullOrEmpty(cmd1))
                {
                    PrintQcLabel(cmd1, bOXBARCODEs[0]);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return true;
        }


        public bool PrintFamilySerial(SubAssembly subAssembly, int copies, string QC = null)
        {
            bool status = false;
            try
            {
                //PrintDialog pd = new PrintDialog();
                string prnfilename = GetPrnCode(Convert.ToString(subAssembly.Family).Trim().ToUpper());
                string query = string.Empty, itemname1 = "", itemname2 = "", barcode = string.Empty;
                getNameSubAssembly(subAssembly.Description.Trim().ToUpper(), ref itemname1, ref itemname2);
                for (int i = 1; i <= copies; i++)
                {
                    //if (QC == "1")
                    //{
                    //    query = ReadFile("QC.prn");
                    //}
                    //else
                    //{
                    if (string.IsNullOrEmpty(subAssembly.Prefix1))
                        query = ReadFile(prnfilename + ".prn");
                    else
                        query = ReadFile("ECEL.prn");
                    //}
                    if (string.IsNullOrEmpty(query))
                    {
                        throw new Exception("Print File Not Found");
                    }
                    if (!string.IsNullOrEmpty(query))
                    {

                        query = query.Replace("JOBID", subAssembly.Job);
                        query = query.Replace("ITEM_CODE", subAssembly.Itemcode.Trim().ToUpper());
                        query = query.Replace("SERIAL_NO", subAssembly.Prefix1 + subAssembly.SerialNumber.Trim().ToUpper());
                        if (subAssembly.PrintDesc)
                        {
                            query = query.Replace("ITEM_NAME1", itemname1);
                            query = query.Replace("ITEM_NAME2", itemname2);
                        }
                        else
                        {
                            query = query.Replace("ITEM_NAME1", "");
                            query = query.Replace("ITEM_NAME2", "");
                        }
                        if (subAssembly.IsQuality)
                        {
                            query = query.Replace("QUALITY_STATUS", "QC");
                        }
                        else
                            query = query.Replace("QUALITY_STATUS", "");
                        barcode = barcode + query;
                    }
                }
                if (subAssembly.PrintMode.Equals("LOCAL"))
                {
                    WriteDataToLabelFile(barcode);
                    //if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
                    //{
                    //    status = true;
                    //}
                }
                else
                {
                    string result = GetPrintterIPAddress(subAssembly.Stage);
                    if (!string.IsNullOrEmpty(result))
                    {
                       string PRINTER_IP = result.Split('#')[0].Trim();
                        string PRINTER_PORT = result.Split('#')[1].Trim();
                        if (string.IsNullOrEmpty(PRINTER_IP) || string.IsNullOrEmpty(PRINTER_PORT))
                        {
                            throw new Exception("Printer Ip address not define for this stage");
                        }
                        if (PrintLabelViaNetwork(barcode, "", PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
                        {
                            status = true;
                        }
                    }
                    else
                        throw new Exception("Printer Ip address not define for this stage");
                }


                return status;
            }
            catch { throw; }
            finally { }
        }

     public bool PrintLabelViaNetwork(string cmd1, string cmd2, string ip, int port)
        {
            System.Net.Sockets.TcpClient tc;
            try
            {
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(ip, Convert.ToInt32(port)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + cmd2.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }

        public string GetPrintterIPAddress(string stage)
        {
            try
            {
                return fun.get_Col_Value("select ipaddr || '#' || ipport  from xxes_stage_master where offline_keycode='" + stage + "'");
            }
            catch (Exception)
            {

                throw;
            }
        }



        string query = string.Empty;
        public bool PrintStandardLabel(string cmd1, string stage, string plant, string family)
        {
            fun.PrinterLog(cmd1, stage);
            string IPADDR = string.Empty, IPPORT = string.Empty, line = string.Empty;
            System.Net.Sockets.TcpClient tc;
            try
            {
                line = fun.getPrinterIp(stage, plant, family);
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("STAGE PRINTER IP ADDRESS AND PORT NOT FOUND");
                }
                IPADDR = line.Split('#')[0].Trim();
                IPPORT = line.Split('#')[1].Trim();
                if (string.IsNullOrEmpty(IPADDR))
                {
                    throw new Exception("PRINTER IP NOT FOUND");
                }
                if (string.IsNullOrEmpty(IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(IPADDR, Convert.ToInt32(IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + "\n");
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }

        public bool PrintStandardLabelByUser(string cmd1, string user, string plant, string family)
        {
            string IPADDR = string.Empty, IPPORT = string.Empty, line = string.Empty;
            System.Net.Sockets.TcpClient tc;
            try
            {
                line = fun.getUserPrinterIp(user, plant, family);
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("STAGE PRINTER IP ADDRESS AND PORT NOT FOUND");
                }
                IPADDR = line.Split(':')[0].Trim();
                IPPORT = line.Split(':')[1].Trim();
                if (string.IsNullOrEmpty(IPADDR))
                {
                    throw new Exception("PRINTER IP NOT FOUND");
                }
                if (string.IsNullOrEmpty(IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(IPADDR, Convert.ToInt32(IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + "\n");
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }

        public string RemoveDoubleQuote(string element)
        {
            string result = element;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace('"', ' ').Trim();
            }
            return result;
        }
        public bool PrintLocationBarcodes(List<Barcode> obj)
        {
            bool status = false;
            try
            {
                string filename = "kanban.prn", query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty;

                if (obj != null)
                {
                    foreach (Barcode item in obj)
                    {
                        plant = item.Plant;
                        family = item.Family;

                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            query = query.Replace("KANBAN_NO", item.LOCATION);
                            query = query.Replace("SUPER_LOC", item.SUPERMKT_LOC);
                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            query = query.Replace("BULK_LOC", item.BLK_LOC);
                            query = query.Replace("BSNP", item.BULKSTORESNP);
                            query = query.Replace("NO_OF_BIN", item.BIN_NO);
                            query = query.Replace("BPACKAGING", item.PACKINGTYPE);


                            if (!string.IsNullOrEmpty(item.ITEM_DESCRIPTION) && item.ITEM_DESCRIPTION.Trim().Length > 35)
                            {
                                item.ITEM_DESCRIPTION = item.ITEM_DESCRIPTION.Substring(0, 35);
                            }
                            item.ITEM_DESCRIPTION = RemoveDoubleQuote(item.ITEM_DESCRIPTION);
                            query = query.Replace("ITEM_DESC", item.ITEM_DESCRIPTION);
                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabel(barcodedata, "LOC_BARCODE", plant, family);
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }
        public bool PrintBULKLOC_Barcodes(List<Barcode> obj)
        {
            bool status = false;
            try
            {
                string filename = string.Empty, query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty, qrcode = string.Empty;
                if (obj != null)
                {
                    foreach (Barcode item in obj)
                    {
                        plant = item.Plant;
                        family = item.Family;
                        if (item.LOCTYPE == "TEMP")
                            filename = "TEMPORARY.prn";
                        else
                            filename = "bulkstorage.prn";
                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            //query = query.Replace("QR_CODE", (item.ITEMCODE + "$" + item.LOCATION ));
                            if (string.IsNullOrEmpty(item.ITEMCODE))
                                item.ITEMCODE = "";
                            if (string.IsNullOrEmpty(item.SUPERMKT_LOC))
                                item.SUPERMKT_LOC = "";
                            qrcode = (item.ITEMCODE + "$" + item.LOCATION + "$" + item.SUPERMKT_LOC);
                            query = query.Replace("QR_CODE", qrcode);
                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            query = query.Replace("REV_NO", item.REVISION);
                            query = query.Replace("BULK_LOC", item.LOCATION);
                            query = query.Replace("NO_OF_LOC", item.NOOFLOCALLOCATED);
                            if(string.IsNullOrEmpty(item.MAX_INVENTORY))
                            {
                                item.MAX_INVENTORY = "";
                            }
                            query = query.Replace("CAPCTY", item.MAX_INVENTORY);
                            query = query.Replace("PACK_TYPE", item.PACKINGTYPE);
                            query = query.Replace("V_STACK_LEVEL", item.VERTICALSTKLEVEL);
                            query = query.Replace("BULK_SNP", item.BULKSTORESNP);
                            query = query.Replace("SUPER_LOC", item.SUPERMKT_LOC);
                            query = query.Replace("USG_TRACTOR", item.USAGEPERTRACTOR);
                            //query = query.Replace("BSAFTY", item.SFTSTKQUANTITY);


                            if (!string.IsNullOrEmpty(item.ITEM_DESCRIPTION) && item.ITEM_DESCRIPTION.Trim().Length > 26)
                            {
                                item.ITEM_DESCRIPTION = item.ITEM_DESCRIPTION.Substring(0, 26);
                            }
                            item.ITEM_DESCRIPTION = RemoveDoubleQuote(item.ITEM_DESCRIPTION);
                            query = query.Replace("ITEM_DESC", item.ITEM_DESCRIPTION);
                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabel(barcodedata, "LOC_BARCODE", plant, family);
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }

        public bool PrintSMLOCBarcodes(List<Barcode> obj)
        {
            bool status = false;
            try
            {
                string filename = string.Empty, query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty;
                if (obj != null)
                {
                    foreach (Barcode item in obj)
                    {
                        plant = item.Plant;
                        family = item.Family;
                        filename = "supermarket.prn";
                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            query = query.Replace("LOC_BARCODE", (item.ITEMCODE + "$" + item.LOCATION));
                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            query = query.Replace("BULK_LOC", item.BLK_LOC);
                            query = query.Replace("SUPER_LOC", item.LOCATION);
                            query = query.Replace("USG_TRACTOR", item.USAGEPERTRACTOR);
                            query = query.Replace("BULK_SNP", item.BULKSTORESNP);

                            if (!string.IsNullOrEmpty(item.ITEM_DESCRIPTION) && item.ITEM_DESCRIPTION.Trim().Length > 26)
                            {
                                item.ITEM_DESCRIPTION = item.ITEM_DESCRIPTION.Substring(0, 26);
                                
                            }
                            item.ITEM_DESCRIPTION = RemoveDoubleQuote(item.ITEM_DESCRIPTION);
                            query = query.Replace("ITEM_DESC", item.ITEM_DESCRIPTION);
                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabel(barcodedata, "LOC_BARCODE", plant, family);
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }
        public bool PrintBackendSticker(PTBuckleup pTbuckelup, string FileName,
           bool isEngineRequire, bool isBackEndRequire,
         string ROPS_SRNO)
        {
            bool status = false;
            try
            {
                string query = ReadFile(FileName), cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    string itemname1 = string.Empty, itemname2 = string.Empty;
                    if (af == null)
                        af = new Assemblyfunctions();
                    af.getName(pTbuckelup.TRACTOR_DESC, ref itemname1, ref itemname2);
                    string Buckleupdate = string.Format(@"select to_char( ENTRYDATE, 'dd-Mon-yyyy HH24:MI:SS' )
                    from xxes_job_status where jobid='{0}' and plant_code='{1}' and family_code='{2}'",
                    pTbuckelup.JOB.Trim().ToUpper(), pTbuckelup.PLANT.Trim().ToUpper(),
                    pTbuckelup.FAMILY.Trim().ToUpper());

                    query = query.Replace("JOB_VAL", pTbuckelup.JOB);

                    query = query.Replace("ITEM_NAME1", itemname1.Trim());
                    query = query.Replace("ITEM_NAME2", itemname2.Trim());
                    query = query.Replace("FCODE_VAL", pTbuckelup.ITEMCODE.Trim().ToUpper());
                    query = query.Replace("SERIES_NO", pTbuckelup.TSN.Trim().ToUpper());
                    query = query.Replace("ROPS_SRNO", ROPS_SRNO.Trim());
                    query = query.Replace("BK_DATETIME", Buckleupdate.Trim());
                    if (isEngineRequire == false)
                        query = query.Replace("ENGG_VAL", "NA");
                    else
                        query = query.Replace("ENGG_VAL", pTbuckelup.ENGINESRLNO.Trim());
                    query = query.Replace("BACKEND_VAL", pTbuckelup.BACKENDSRLNO.Trim().ToUpper());
                    if (!string.IsNullOrEmpty(pTbuckelup.TSN))
                    {
                        if (pTbuckelup.TSN.Trim().Length == 17)
                            query = query.Replace("MONTH", pTbuckelup.TSN.Trim().ToUpper().Substring(8, 2));
                        else
                            if (pTbuckelup.PrintMMYYFormat == "1")
                        {
                            query = query.Replace("MONTH", pTbuckelup.SUFFIX.Trim());
                        }
                        else
                            query = query.Replace("MONTH", pTbuckelup.TSN.Trim().ToUpper().Substring(pTbuckelup.TSN.Trim().Length - 2, 2));
                    }
                    else
                        query = query.Replace("MONTH", "");
                    cmd1 = query;
                    cmd2 = query;
                    if (PrintLabel(cmd1, cmd2, pTbuckelup))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }
        public bool PrintBackendStickerLongDescription(PTBuckleup pTbuckelup, string FileName,
         bool isEngineRequire, bool isBackEndRequire,
         string ROPS_SRNO)
        {
            bool status = false;
            string[] itemname = new string[8];
            try
            {
                string query = ReadFile(FileName), cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    if (af == null)
                        af = new Assemblyfunctions();
                    af.getNameLongDesc(pTbuckelup.TRACTOR_DESC, out itemname);
                    string Buckleupdate = fun.get_Col_Value(string.Format(@"select to_char( ENTRYDATE, 'dd-Mon-yyyy HH24:MI:SS' )
                    from xxes_job_status where jobid='{0}' and plant_code='{1}' and family_code='{2}'",
                    pTbuckelup.JOB.Trim().ToUpper(), pTbuckelup.PLANT.Trim().ToUpper(),
                    pTbuckelup.FAMILY.Trim().ToUpper()));
                    query = query.Replace("JOB_VAL", pTbuckelup.JOB);
                    for (int i = 0; i < itemname.Length; i++)
                    {
                        query = query.Replace("ITEM_NAME" + i, itemname[i].Trim());
                    }
                    if (itemname.Length < 8)
                    {
                        for (int i = itemname.Length; i < 8; i++)
                            query = query.Replace("ITEM_NAME" + i, string.Empty);
                    }
                    query = query.Replace("FCODE_VAL", pTbuckelup.ITEMCODE.Trim().ToUpper());
                    query = query.Replace("SERIES_NO", pTbuckelup.TSN.Trim().ToUpper());
                    query = query.Replace("ROPS_SRNO", ROPS_SRNO.Trim());
                    query = query.Replace("BK_DATETIME", Buckleupdate.Trim());
                    if (isEngineRequire == false)
                        query = query.Replace("ENGG_VAL", "NA");
                    else
                        query = query.Replace("ENGG_VAL", pTbuckelup.ENGINESRLNO.Trim());
                    query = query.Replace("BACKEND_VAL", pTbuckelup.BACKENDSRLNO.Trim().ToUpper());
                    if (!string.IsNullOrEmpty(pTbuckelup.TSN))
                    {
                        if (pTbuckelup.TSN.Trim().Length == 17)
                            query = query.Replace("MONTH", pTbuckelup.TSN.Trim().ToUpper().Substring(8, 2));
                        else if (pTbuckelup.PrintMMYYFormat == "1")
                        {
                            query = query.Replace("MONTH", pTbuckelup.SUFFIX.Trim());
                        }
                        else
                            query = query.Replace("MONTH", pTbuckelup.TSN.Trim().ToUpper().Substring(pTbuckelup.TSN.Trim().Length - 2, 2));
                    }
                    else
                        query = query.Replace("MONTH", "");
                    cmd1 = query;
                    cmd2 = query;
                    if (PrintLabel(cmd1, cmd2, pTbuckelup))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }
        public bool PrintLabel(string cmd1, string cmd2, PTBuckleup pTBuckleup)
        {

            System.Net.Sockets.TcpClient tc;
            try
            {
                //WriteFile(cmd1 + cmd2);
                if (string.IsNullOrEmpty(pTBuckleup.IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(pTBuckleup.IPADDR, Convert.ToInt32(pTBuckleup.IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + cmd2.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }
        public bool PrintQcLabel(string cmd1, BOXBARCODE bOXBARCODE)
        {

            System.Net.Sockets.TcpClient tc;
            try
            {
                //WriteFile(cmd1 + cmd2);
                if (string.IsNullOrEmpty(bOXBARCODE.PRINTERPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(bOXBARCODE.PRINTERIP, Convert.ToInt32(bOXBARCODE.PRINTERPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + "\n");
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }

        public bool PrintBackendStickerFT(FTBuckleup fTBuckleup, string FileName, string[] itemname,
          bool isRearAxelRequire, bool isTransRequire, bool isBackEndRequire)
        {
            bool status = false;
            try
            {
                string query = ReadFile(FileName), cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    string itemname1 = string.Empty, itemname2 = string.Empty; string TractorType = string.Empty;
                    if (af == null)
                        af = new Assemblyfunctions();
                    af.getName(fTBuckleup.TRACTOR_DESC, ref itemname1, ref itemname2);
                    string Buckleupdate = string.Format(@"select to_char( ENTRYDATE, 'dd-Mon-yyyy HH24:MI:SS' )
                    from xxes_job_status where jobid='{0}' and plant_code='{1}' and family_code='{2}'",
                    fTBuckleup.JOB.Trim().ToUpper(), fTBuckleup.PLANT.Trim().ToUpper(),
                    fTBuckleup.FAMILY.Trim().ToUpper());                   
                    query = query.Replace("JOB_VAL", fTBuckleup.JOB.Trim().ToUpper());
                    query = query.Replace("ITEM_NAME1", itemname1.Trim());
                    query = query.Replace("ITEM_NAME2", itemname2.Trim());
                    query = query.Replace("FCODE_VAL", fTBuckleup.ITEMCODE.Trim().ToUpper()); 
                    
                    if (isTransRequire == false && isBackEndRequire == false)
                        query = query.Replace("TRANS_VAL", "NA");
                    else
                        query = query.Replace("TRANS_VAL", fTBuckleup.TRANSMISSIONSRLNO.Trim());
                    if (isRearAxelRequire == false && isBackEndRequire == false)
                        query = query.Replace("REAR_VAL", "NA");
                    else
                        query = query.Replace("REAR_VAL", fTBuckleup.REARAXELSRLNO.Trim());
                    if (isBackEndRequire == true)
                        query = query.Replace("BACKEND_VAL", fTBuckleup.BackendSrlno.Trim());
                    else
                        query = query.Replace("BACKEND_VAL", "NA");
                    if (TractorType == "DOMESTIC")
                        query = query.Replace("TYPE_VAL", "");
                    else
                        query = query.Replace("TYPE_VAL", "E");
                    if (!string.IsNullOrEmpty(fTBuckleup.JOB))
                    {
                        if (fTBuckleup.JOB.Trim().Length == 17)
                            query = query.Replace("MONTH", fTBuckleup.JOB.Trim().ToUpper().Substring(8, 2));
                        else
                            if (fTBuckleup.PrintMMYYFormat == "1")
                        {
                            query = query.Replace("MONTH", fTBuckleup.SUFFIX.Trim());
                        }
                        else
                            query = query.Replace("MONTH", fTBuckleup.JOB.Trim().ToUpper().Substring(fTBuckleup.JOB.Trim().Length - 2, 2));
                    }
                    else
                        query = query.Replace("MONTH", "");
                    cmd1 = query;
                    cmd2 = query;
                    if (PrintLabelFT(cmd1, cmd2, fTBuckleup))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }


        public bool PrintLabelFT(string cmd1, string cmd2, FTBuckleup fTBuckleup)
        {

            System.Net.Sockets.TcpClient tc;
            try
            {
                //WriteFile(cmd1 + cmd2);
                if (string.IsNullOrEmpty(fTBuckleup.IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(fTBuckleup.IPADDR, Convert.ToInt32(fTBuckleup.IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + cmd2.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }

        public bool PrintBox(List<GoodsRecivingatStoreModel> obj)
        {
            bool status = false;
            try
            {
                string filename = string.Empty, query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty;
                if (obj != null)
                {
                    foreach (GoodsRecivingatStoreModel item in obj)
                    {
                        plant = item.PLANT;
                        family = item.FAMILY;
                        filename = "Verify_Box.prn";

                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            query = query.Replace("PLANT_CODE", item.PLANT);
                            query = query.Replace("MRN_NO", item.MRN_NO);
                            query = query.Replace("PRINT_DATE", fun.GetServerDateTime().ToString("dd/MM/yyyy HH:mm"));

                            query = query.Replace("INVOICE_NO", item.INVOICE_NUMBER);
                            query = query.Replace("INVOICE_DATE", item.INVOICE_DATE);
                            query = query.Replace("LOCATION_CODE", item.LOCATION);

                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            if (!string.IsNullOrEmpty(item.ITEM_DESCRIPTION) && item.ITEM_DESCRIPTION.Trim().Length > 26)
                            {
                                item.ITEM_DESCRIPTION = item.ITEM_DESCRIPTION.Substring(0, 26);
                            }
                            query = query.Replace("ITEM_DESC", item.ITEM_DESCRIPTION);
                            query = query.Replace("VENDOR_NAME", item.VENDOR_NAME);
                            query = query.Replace("BOX_QTY", item.QTY_RECEIVED);
                            query = query.Replace("BOX_NO", item.boxNo);
                            query = query.Replace("ORDER_BY", item.ORDERBY);
                            query = query.Replace("VENDOR_CODE", item.VENDOR_CODE);

                            char[] countDigitItemcode = fun.characterArray(item.ITEMCODE.Trim());
                            if(countDigitItemcode.Length == 9)
                            {
                                query = query.Replace("ITEM_REVISION", item.ITEM_REVISION);
                                query = query.Replace("BOM_REV", item.BOM_REVISION);

                            }
                            else if (countDigitItemcode.Length == 11)
                            {
                                query = query.Replace("ITEM_REVISION", "");
                                query = query.Replace("BOM_REV", "");
                            }

                            if (item.STATUS == "QA")
                            {
                                query = query.Replace("QA", item.STATUS);
                            }
                            else
                            {
                                query = query.Replace("QA", "");
                            }

                            query = query.Replace("IMPLEMENTED", item.IMPLEMENT);
                            query = query.Replace("PRINTED_ON", item.PRINTED_ON);
                            query = query.Replace("BUYER", item.BUYER_NAME);
                            
                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabelByUser(barcodedata, HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim(), plant.Trim(), family.Trim());
                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return status;
        }
        public bool PrintBoxs(List<BARCODEPRINT> obj)
        {
            bool status = false;
            try
            {
                string filename = string.Empty, query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty;
                if (obj != null)
                {
                    foreach (BARCODEPRINT item in obj)
                    {
                        plant = item.PLANT;
                        family = item.FAMILY;
                        filename = "BOX.prn";

                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            query = query.Replace("TRANSACTION_DATE", item.TRANSACTION_DATE);
                            query = query.Replace("PLANT_CODE", item.PLANT);
                            query = query.Replace("SUPP_NAME", item.SUPP_NAME);
                            query = query.Replace("BULK_LOC", item.BULK_LOC);
                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            query = query.Replace("CURRENT_DATE", item.CURRENT_DATE);
                            query = query.Replace("QTY_ORD", item.QTY_ORD);
                            query = query.Replace("QTY_DLV", item.QTY_DLV);
                            query = query.Replace("BOX_NO", item.BOX_NO);
                            query = query.Replace("BPACKAGING", item.BPACKAGING);
                            query = query.Replace("BULK_SNP", item.BULK_SNP);
                            query = query.Replace("QR_CODE", item.QR_CODE);

                            if (!string.IsNullOrEmpty(item.ITEM_DESC) && item.ITEM_DESC.Trim().Length > 35)
                            {
                                item.ITEM_DESC = item.ITEM_DESC.Substring(0, 35);
                            }
                            query = query.Replace("ITEM_DESC", item.ITEM_DESC);



                            query = query.Replace("UNPACKING", item.UNPACKED);
                            query = query.Replace("QLTY", item.QLTY);
                            query = query.Replace("BUYER_NAME", item.BUYER_NAME);
                            query = query.Replace("PO_LINE", item.PO_LINE);

                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabel(barcodedata, "STORE", plant, family);
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }

        public bool PrintEngineSticker(string response, ENGINE engine)
        {
            bool status = false;
            string TractorType = string.Empty, fcode_desc = string.Empty, Ejobid = string.Empty, fcode = string.Empty,
                  Current_Serial_number = string.Empty, ESrno = string.Empty, TransSrlno = string.Empty,
                  RearAxelSrlno = string.Empty, BACKEND_SRLNO = string.Empty, ROPS_SRNO = string.Empty,
                  Filename = string.Empty, Suffix = string.Empty;
            string itemname1 = string.Empty, itemname2 = string.Empty;
            bool isBackEndRequire;

            try
            {

                string[] line = response.Split('#');
                TractorType = Convert.ToString(line[1]);
                fcode_desc = Convert.ToString(line[2]);
                Ejobid = Convert.ToString(line[3]);
                fcode = Convert.ToString(line[4]);
                Current_Serial_number = Convert.ToString(line[5]);
                ESrno = Convert.ToString(line[6]);
                isBackEndRequire = (Convert.ToString(line[7]) == "Y" ? true : false);
                TransSrlno = Convert.ToString(line[8]);
                RearAxelSrlno = Convert.ToString(line[9]);
                BACKEND_SRLNO = Convert.ToString(line[10]);
                ROPS_SRNO = Convert.ToString(line[11]);

                if (TractorType == "EXPORT")
                    Filename = "EN17.txt";
                else
                    Filename = "EN.txt";

                string query = ReadFile(Filename), cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    if (af == null)
                        af = new Assemblyfunctions();
                    af.getName(fcode_desc, ref itemname1, ref itemname2);

                    query = query.Replace("JOB_VAL", Ejobid);
                    query = query.Replace("ITEM_NAME1", itemname1.Trim());
                    query = query.Replace("ITEM_NAME2", itemname2.Trim());
                    query = query.Replace("FCODE_VAL", fcode.Trim().ToUpper());
                    query = query.Replace("SERIES_NO", Current_Serial_number.Trim().ToUpper());
                    query = query.Replace("ENGG_VAL", ESrno);

                    if (!isBackEndRequire)
                    {
                        query = query.Replace("TRANS_VAL", TransSrlno.Trim().ToUpper());
                        query = query.Replace("REAR_VAL", RearAxelSrlno.Trim().ToUpper());
                    }
                    else
                    {
                        query = query.Replace("TRANS_VAL", BACKEND_SRLNO.Trim().ToUpper());
                        query = query.Replace("REAR_VAL", "");
                        query = query.Replace("TRAN", "SK");
                        query = query.Replace("REAR:", "");
                        query = query.Replace("REAR :", "");
                    }

                    query = query.Replace("ROPS_SRNO", ROPS_SRNO.Trim());

                    if (!string.IsNullOrEmpty(Current_Serial_number))
                    {
                        if (Current_Serial_number.Trim().Length == 17)
                            query = query.Replace("MONTH", Current_Serial_number.Trim().ToUpper().Substring(8, 2));
                        else
                            if (TractorType.ToUpper().Trim() == "DOMESTIC" && engine.PrintMMYYFormat == "1")
                        {
                           
                            Suffix = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + fun.ServerDate.Date.ToString("MMM-yyyy").ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + engine.Plant.Trim().ToUpper() + "'");
                            query = query.Replace("MONTH", Suffix.Trim());
                        }
                        else
                            query = query.Replace("MONTH", Current_Serial_number.Trim().ToUpper().Substring(Current_Serial_number.Trim().Length - 2, 2));
                    }
                    else
                        query = query.Replace("MONTH", "");

                    cmd1 = query;
                    cmd2 = query;
                    if (PrintEngineLabel(cmd1, cmd2, engine))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }
        public bool PrintEngineLabel(string cmd1, string cmd2, ENGINE eNGINE)
        {

            System.Net.Sockets.TcpClient tc;
            try
            {
                //WriteFile(cmd1 + cmd2);
                if (string.IsNullOrEmpty(eNGINE.IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(eNGINE.IPADDR, Convert.ToInt32(eNGINE.IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + cmd2.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }

        public bool RePrintEngineSticker(string line, ENGINE fTEngine)
        {
            bool status = false;
            //string TractorType = string.Empty, fcode_desc = string.Empty, Ejobid = string.Empty, fcode = string.Empty,
            //      Current_Serial_number = string.Empty, ESrno = string.Empty, TransSrlno = string.Empty,
            //      RearAxelSrlno = string.Empty, BACKEND_SRLNO = string.Empty, ROPS_SRNO = string.Empty,
            //      Filename = string.Empty, Suffix = string.Empty;
            //string itemname1 = string.Empty, itemname2 = string.Empty;
            //bool isBackEndRequire;
            string TractorType = string.Empty, Filename = string.Empty, itemname1 = string.Empty, itemname2 = string.Empty,
                Suffix = string.Empty, EnMisc = string.Empty;
            try
            {

                fTEngine.ITEMCODE = line.Split('#')[0].Trim();
                fTEngine.ITEMDESC = string.IsNullOrEmpty(line.Split('#')[1]) == true ? line.Split('#')[2].Trim() : line.Split('#')[1].Trim();
                fTEngine.TRANSMISSIONSRLNO = line.Split('#')[3].Trim();
                fTEngine.REARAXELSRLNO = line.Split('#')[4].Trim();
                fTEngine.ENGINESRLNO = line.Split('#')[5].Trim();
                fTEngine.FCODESRLNO = line.Split('#')[6].Trim();
                fTEngine.FCODEID = line.Split('#')[7].Trim();
                fTEngine.ROPSSRNO = line.Split('#')[8].Trim();
                fTEngine.BACKENDSRLNO = line.Split('#')[9].Trim();
                fTEngine.ENTRYDATE = line.Split('#')[10].Trim();
                fTEngine.REQUIREENGINE = line.Split('#')[11] == "Y" ? true : false;
                fTEngine.REQUIREBACKEND = line.Split('#')[12] == "Y" ? true : false;

                TractorType = fun.get_Col_Value(string.Format(@"select TYPE from xxes_daily_plan_TRAN where item_code ='{0}' and 
                                autoid='{1}' and plant_code='{2}' and family_code='{3}'",fTEngine.ITEMCODE,fTEngine.FCODEID,
                                fTEngine.Plant,fTEngine.Family));
                
                if (TractorType.ToUpper().Trim() == "EXPORT")
                    Filename = "EN17.txt";
                else
                    Filename = "EN.txt";

                string query = ReadFile(Filename), cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    if (af == null)
                        af = new Assemblyfunctions();
                    af.getName(fTEngine.ITEMDESC.Trim().ToUpper(), ref itemname1, ref itemname2);

                    query = query.Replace("JOB_VAL", fTEngine.JobId);
                    query = query.Replace("ITEM_NAME1", itemname1.Trim());
                    query = query.Replace("ITEM_NAME2", itemname2.Trim());
                    query = query.Replace("FCODE_VAL", fTEngine.ITEMCODE.Trim().ToUpper());
                    query = query.Replace("SERIES_NO", fTEngine.FCODESRLNO.Trim().ToUpper());


                    if (fTEngine.REQUIREENGINE == false)
                        query = query.Replace("ENGG_VAL", "NA");
                    else
                        query = query.Replace("ENGG_VAL", fTEngine.ENGINESRLNO);
                   

                    if (fTEngine.REQUIREBACKEND == false)
                    {
                        query = query.Replace("TRANS_VAL", fTEngine.TRANSMISSIONSRLNO.Trim().ToUpper());
                        query = query.Replace("REAR_VAL", fTEngine.REARAXELSRLNO.Trim().ToUpper());
                    }
                    else
                    {
                        query = query.Replace("TRANS_VAL", fTEngine.BACKENDSRLNO.Trim().ToUpper());
                        query = query.Replace("REAR_VAL", "");
                        query = query.Replace("TRAN", "SK");
                        query = query.Replace("REAR:", "");
                        query = query.Replace("REAR :", "");
                    }

                    query = query.Replace("ROPS_SRNO", fTEngine.ROPSSRNO.Trim());

                    if (!string.IsNullOrEmpty(fTEngine.FCODESRLNO))
                    {
                        if (fTEngine.FCODESRLNO.Trim().Length == 17)
                            query = query.Replace("MONTH", fTEngine.FCODESRLNO.Trim().ToUpper().Substring(8, 2));
                        else
                            if (TractorType.ToUpper().Trim() == "DOMESTIC" && fTEngine.PrintMMYYFormat == "1")
                        {
                            EnMisc = fun.get_Col_Value(string.Format(@"select to_char(scan_date,'MON-YYYY') scan_date from XXES_SCAN_TIME 
                                    where jobid='{0}' and ITEM_CODE='{1}' and stage='EN' and PLANT_CODE='{2}' and FAMILY_CODE='{3}' 
                                    and rownum=1", fTEngine.JobId.Trim(), fTEngine.ITEMCODE.Trim().ToUpper(), fTEngine.Plant.Trim().ToUpper(), fTEngine.Family.Trim().ToUpper()));
                            if (string.IsNullOrEmpty(EnMisc))
                            {
                               
                                throw new Exception("BUCKELUP NOT FOUND FOR JOB : " + fTEngine.JobId);
                                
                            }

                            Suffix = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + EnMisc.ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + fTEngine.Plant.Trim().ToUpper() + "'");
                            query = query.Replace("MONTH", Suffix.Trim());
                        }
                        else
                            query = query.Replace("MONTH", fTEngine.FCODESRLNO.Trim().ToUpper().Substring(fTEngine.FCODESRLNO.Trim().Length - 2, 2));
                    }
                    else
                        query = query.Replace("MONTH", "");

                    cmd1 = query;
                    cmd2 = query;
                    if (PrintEngineLabel(cmd1, cmd2, fTEngine))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }

        public bool PrintBackendFT(string response, BACKEND bACKEND)
        {
            bool status = false;

            try
            {
                string query = string.Empty, cmd1 = string.Empty, cmd2 = string.Empty, hyd = string.Empty;
                if (bACKEND.plant == "T05")
                {
                    hyd = ReadFile("HYDOK.prn"); cmd2 = "";
                    if (!string.IsNullOrEmpty(hyd))
                    {
                        hyd = hyd.Replace("BSLR_NO", bACKEND.backend_srlno);
                        hyd = hyd.Replace("HYD_VAL", bACKEND.HydraulicSrlno);
                        hyd = hyd.Replace("STATUS", "MAPPING SUCCESSFUL");
                    }
                    else
                    {
                        //hyd = hyd.Replace("BSLR_NO", bACKEND.backend_srlno);
                        //hyd = hyd.Replace("HYD_VAL", bACKEND.HydraulicSrlno);
                        //hyd = hyd.Replace("STATUS", "Match UnSuccessfully");
                    }
                    cmd2 = hyd;

                    query = ReadFile("BACKENDFT.PRN"); cmd1 = "";
                    if (!string.IsNullOrEmpty(query))
                    {
                        query = query.Replace("BSRLNO", bACKEND.backend_srlno);
                        query = query.Replace("BACKEND", bACKEND.backend);
                        query = query.Replace("JOBID", bACKEND.jobid);
                        if (!string.IsNullOrEmpty(bACKEND.backend_desc))
                        {
                            if (bACKEND.backend_desc.Length > 20)
                            {
                                query = query.Replace("ITEM_NAME1", bACKEND.backend_desc.Substring(0, 19));
                                query = query.Replace("ITEM_NAME2", bACKEND.backend_desc.Substring(20));
                            }
                            else
                            {
                                query = query.Replace("ITEM_NAME1", bACKEND.backend_desc);
                                query = query.Replace("ITEM_NAME2", string.Empty);
                            }
                        }
                        else
                        {
                            query = query.Replace("ITEM_NAME1", bACKEND.backend_desc);
                            query = query.Replace("ITEM_NAME2", bACKEND.backend_desc);
                        }
                        cmd1 = query;
                        if (PrintBackendLable(cmd1, cmd2, bACKEND))
                            status = true;
                        else
                            status = false;
                    }
                    else
                    {
                        throw new Exception("Print File Not Found");
                    }
                    return status;
                }
                else
                {
                    query = ReadFile("BACKENDFT.PRN"); cmd1 = "";
                    if (!string.IsNullOrEmpty(query))
                    {
                        query = query.Replace("BSRLNO", bACKEND.backend_srlno);
                        query = query.Replace("BACKEND", bACKEND.backend);
                        query = query.Replace("JOBID", bACKEND.jobid);
                        if (!string.IsNullOrEmpty(bACKEND.backend_desc))
                        {
                            if (bACKEND.backend_desc.Length > 20)
                            {
                                query = query.Replace("ITEM_NAME1", bACKEND.backend_desc.Substring(0, 19));
                                query = query.Replace("ITEM_NAME2", bACKEND.backend_desc.Substring(20));
                            }
                            else
                            {
                                query = query.Replace("ITEM_NAME1", bACKEND.backend_desc);
                                query = query.Replace("ITEM_NAME2", string.Empty);
                            }
                        }
                        else
                        {
                            query = query.Replace("ITEM_NAME1", bACKEND.backend_desc);
                            query = query.Replace("ITEM_NAME2", bACKEND.backend_desc);
                        }
                        cmd1 = query;
                        if (PrintBackendLable(cmd1, "", bACKEND))
                            status = true;
                        else
                            status = false;
                    }
                    else
                    {
                        throw new Exception("Print File Not Found");
                    }
                    return status;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally
            {

            }
        }

        public bool PrintBackendLable(string cmd1, string cmd2, BACKEND bACKEND)
        {
            System.Net.Sockets.TcpClient tc;
            try
            {
                //WriteFile(cmd1 + cmd2);
                if (string.IsNullOrEmpty(bACKEND.IPPORT))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(bACKEND.IPADDR, Convert.ToInt32(bACKEND.IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + cmd2.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();
                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {
                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + bACKEND.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }
        /// <summary>
        /// Below funtion by raj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool PrintMrnSrno(List<GoodsRecivingatStoreModel> obj)
        {
            DataTable dt = new DataTable();
            bool status = false;
            try
            {
                string filename = string.Empty,
                query = string.Empty,
                barcodedata = string.Empty,
                plant = string.Empty,
                family = string.Empty;


                if (obj != null)
                {
                    foreach (GoodsRecivingatStoreModel item in obj)
                    {
                        plant = item.PLANT;
                        family = item.FAMILY;

                        filename = "Mrnsrnoprint.prn";

                        query = ReadFile(filename);
                        if (!string.IsNullOrEmpty(query))
                        {
                            query = query.Replace("MRN_NO", item.MRN_NO);
                            query = query.Replace("PRINT_DATE", item.TRANSACTION_DATE);

                            query = query.Replace("ITEM_CODE", item.ITEMCODE);
                            if (!string.IsNullOrEmpty(item.ITEM_DESCRIPTION) && item.ITEM_DESCRIPTION.Trim().Length > 20)
                            {
                                item.ITEM_DESCRIPTION = item.ITEM_DESCRIPTION.Substring(0, 20);
                            }
                            query = query.Replace("ITEM_DESC", item.ITEM_DESCRIPTION);
                            query = query.Replace("MRN_SRNO", item.boxNo);


                            barcodedata += "\n" + query + "\n";
                        }


                    }
                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintMrnSrnoFile(barcodedata, "STORE", plant, family);
                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return status;
        }


        public bool PrintMrnSrnoFile(string cmd1, string stage, string plant, string family)
        {
            fun.PrinterLog(cmd1, stage);
            //string PrinterIp = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP"],
            string IPADDR = string.Empty, IPPORT = string.Empty, line = string.Empty;
            System.Net.Sockets.TcpClient tc;
            try
            {
                if (plant == "T02")
                {
                    IPADDR = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_IP_T02"];
                    IPPORT = ConfigurationManager.AppSettings["MRNSRNO_PRINTER_PORT_T02"];
                    if (string.IsNullOrEmpty(IPADDR))
                    {
                        throw new Exception("PRINTER IP NOT FOUND FOR PLANT T02");
                    }
                    if (string.IsNullOrEmpty(IPPORT))
                    {
                        throw new Exception("PRINTER PORT NOT FOUND FOR PLANT T02");
                    }
                }
                else
                {
                    line = fun.getPrinterIp(stage, plant, family);
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("PRINTER IP ADDRESS NOT FOUND");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("PRINTER PORT NOT FOUND");
                    }
                    IPADDR = line.Split('#')[0].Trim();
                    IPPORT = line.Split('#')[1].Trim();
                    if (string.IsNullOrEmpty(IPADDR))
                    {
                        throw new Exception("PRINTER IP NOT FOUND");
                    }
                    if (string.IsNullOrEmpty(IPPORT))
                    {
                        throw new Exception("PRINTER PORT NOT FOUND");
                    }
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(IPADDR, Convert.ToInt32(IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim() + "\n");
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();

                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {

                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + pTBuckleup.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }
        public bool GenerateGateBarcode(string mode, string PRINTER_IP, string PRINTER_PORT, MRNInvoice invoice, DateTime printtime, bool copy2, string Family)
        {
            PrinterSettings settings = new PrinterSettings();
            bool status = false;
            try
            {
                string filename = string.Empty, query = string.Empty, barcodedata = string.Empty,
                    plant = string.Empty, family = string.Empty;
                if (invoice != null)
                {
                    plant = invoice.PLANT_CODE;
                    family = Family;
                    filename = "Gate2.prn";

                    query = ReadFile(filename);
                    if (!string.IsNullOrEmpty(query))
                    {

                        query = query.Replace("MRN_NO", invoice.MRN_NO);
                        query = query.Replace("IN_TIME", "  " + invoice.TRANSACTION_DATE.ToString("HH:mm"));
                        query = query.Replace("IN_DATE", invoice.TRANSACTION_DATE.ToString("dd/MM/yy"));
                        query = query.Replace("INVOICE_NUMBER", invoice.INVOICE_NO);
                        query = query.Replace("TRUCK_NO", invoice.VEHICLE_NO);
                        if (invoice.SOURCE_TYPE.Equals("REQ"))
                            query = query.Replace("COMPANY_NAME", "ESCORTS");
                        else
                        {
                            if (invoice.VENDOR_NAME.Length > 22)
                                invoice.VENDOR_NAME = invoice.VENDOR_NAME.Substring(0, 22);
                            query = query.Replace("COMPANY_NAME", invoice.VENDOR_NAME + "-" + invoice.VENDOR_CODE);
                        }

                        if (invoice.STATUS.Equals("QA"))
                            query = query.Replace("LQ", "Q");
                        else
                            query = query.Replace("LQ", "");
                        query = query.Replace("VEHICLE_NUMBER", invoice.VEHICLE_NO);
                        query = query.Replace("PART_CODE", invoice.ITEM_CODE + " " + "+" + (Convert.ToInt16(invoice.TOTAL_ITEM) - 1).ToString());
                        query = query.Replace("MU", invoice.STORE_LOCATION);
                        query = query.Replace("SAMPLE", invoice.SAMPLE);
                        barcodedata += "\n" + query + "\n";
                       
                    }

                    if (!string.IsNullOrEmpty(barcodedata))
                    {
                        status = PrintStandardLabel(barcodedata, "LABEL", plant.Trim(), family.Trim());
                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return status;
        }
        public bool PrintFamilySerial(SubAssemblyModel subAssembly, int copies, string QC = null)
        {
            bool status = false;
            try
            {
                //PrintDialog pd = new PrintDialog();
                string prnfilename = GetPrnCode(Convert.ToString(subAssembly.Family).Trim().ToUpper());
                string query = string.Empty, itemname1 = "", itemname2 = "", barcode = string.Empty, PRINTER_IP = string.Empty, PRINTER_PORT = string.Empty;
                getNameSubAssembly(subAssembly.DESCRIPTION.Trim().ToUpper(), ref itemname1, ref itemname2);
                for (int i = 1; i <= copies; i++)
                {
                    //if (QC == "1")
                    //{
                    //    query = ReadFile("QC.prn");
                    //}
                    //else
                    //{
                    if (string.IsNullOrEmpty(subAssembly.Prefix1))
                        query = ReadFile(prnfilename + ".prn");
                    else
                        query = ReadFile("ECEL.prn");
                    //}
                    if (string.IsNullOrEmpty(query))
                    {
                        throw new Exception("Print File Not Found");
                    }
                    if (!string.IsNullOrEmpty(query))
                    {

                        query = query.Replace("JOBID", subAssembly.Job);
                        query = query.Replace("ITEM_CODE", subAssembly.ITEMCODE.Trim().ToUpper());
                        query = query.Replace("SERIAL_NO", subAssembly.Prefix1 + subAssembly.SerialNumber.Trim().ToUpper());
                        if (subAssembly.PrintDesc)
                        {
                            query = query.Replace("ITEM_NAME1", itemname1);
                            query = query.Replace("ITEM_NAME2", itemname2);
                        }
                        else
                        {
                            query = query.Replace("ITEM_NAME1", "");
                            query = query.Replace("ITEM_NAME2", "");
                        }
                        if (subAssembly.IsQuality)
                        {
                            query = query.Replace("QUALITY_STATUS", "QC");
                        }
                        else
                            query = query.Replace("QUALITY_STATUS", "");
                        barcode = barcode + query;
                    }
                }
                if (subAssembly.PrintMode.Equals("LOCAL"))
                {
                    WriteDataToLabelFile(barcode);
                    //if (SendtoPrinter.SendFileToPrinter(pd.PrinterSettings.PrinterName, Application.StartupPath.ToString() + "\\Label"))
                    //{
                    //    status = true;
                    //}
                }
                else
                {
                    string result = GetPrintterIPAddress(subAssembly.Stage);
                    if (!string.IsNullOrEmpty(result))
                    {
                        PRINTER_IP = result.Split('#')[0].Trim();
                        PRINTER_PORT = result.Split('#')[1].Trim();
                        if (string.IsNullOrEmpty(PRINTER_IP) || string.IsNullOrEmpty(PRINTER_PORT))
                        {
                            throw new Exception("Printer Ip address not define for this stage");
                        }
                        if (PrintLabelViaNetwork(barcode, "", PRINTER_IP, Convert.ToInt16(PRINTER_PORT)))
                        {
                            status = true;
                        }
                    }
                    else
                        throw new Exception("Printer Ip address not define for this stage");
                }

                if (!string.IsNullOrEmpty(barcode))
                {
                    status = PrintStandardLabelByUser(barcode, HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim(), subAssembly.PLANTCODE.Trim(), subAssembly.FAMILYCODE.Trim());
                }

                return status;
            }
            catch { throw; }
            finally { }
        }
        public void WriteDataToLabelFile(string data)
        {
            try
            {
                StreamWriter swrite = null;
                if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\Label"))
                {
                    File.Delete(System.IO.Directory.GetCurrentDirectory() + "\\Label");
                }

                swrite = File.AppendText(System.IO.Directory.GetCurrentDirectory() + "\\Label");
                swrite.WriteLine(data);
                swrite.Flush();
                if (swrite != null)
                {
                    swrite.Dispose();
                    swrite.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Module WriteDataToLabelFile: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            { }

        }
        public string GetPrnCode(string family)
        {
            string code = string.Empty;
            try
            {
                family = family.Trim().ToUpper();
                if (family.Equals("REARAXLE FTD") || family.Equals("REAR AXEL FTD"))
                    code = "REAR";
                else if (family.Equals("ENGINE FTD"))
                    code = "ENF";
                else if (family.Equals("ENGINE TD"))
                    code = "ENP";
                else if (family.Equals("TRANSMISSION FTD"))
                    code = "TRANS";
                else if (family.Equals("BACK END TD") || family.Equals("BACKEND TD"))
                    code = "BACK";
                else if (family.Equals("HYDRAULIC FTD"))
                    code = "HYD";

            }
            catch (Exception)
            {

                throw;
            }
            return code;
        }
        public void getNameSubAssembly(string fcode_desc, ref string itemname1, ref string itemname2)
        {
            try
            {
                itemname2 = "";
                itemname1 = fcode_desc;
                if (itemname1.Length > 50)
                {
                    itemname1 = itemname1.Trim().Substring(0, 22);
                    itemname2 = fcode_desc.Trim().Substring(22, fcode_desc.Trim().ToUpper().Length - 22);
                    if (itemname2.Trim().Length > 23)
                    {
                        itemname2 = itemname2.Substring(0, 22);
                    }
                }
                else if (itemname1.Length > 25)
                {
                    itemname1 = itemname1.Trim().Substring(0, 22).Trim();
                    itemname2 = fcode_desc.Trim().ToUpper().Substring(22, fcode_desc.Trim().Length - 22).Trim();
                }
            }

            catch (Exception ex)
            { 
            }
            finally
            { }

        }
        public void Record_Reprint(string ITEMCODE, string SRNO, string JOB, string plant,string family)
        {
            try
            {
                //    if(Convert.ToString(cmbOfflineItems.SelectedValue)=="HYD")
                query = @"insert into XXES_REPRINT_LABEL(PLANT_CODE,FAMILY_CODE,STAGE,LOGIN_USER,PRINT_DATE,ITEM_CODE,SRNO,JOB) values ('" + Convert.ToString(plant).Trim().ToUpper() + "','" + Convert.ToString(family).Trim().ToUpper() + "','" + Convert.ToString("JOB LABEL").Trim().ToUpper() + "','" + Convert.ToString(HttpContext.Current.Session["Login_User"].ToString()) + "',SYSDATE,'" + ITEMCODE.Trim() + "','" + SRNO.Trim() + "','" + JOB.Trim() + "')";
                //else
                //    query = @"insert into XXES_REPRINT_LABEL(PLANT_CODE,FAMILY_CODE,STAGE,LOGIN_USER,PRINT_DATE) values ('" + Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper() + "','" + Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper() + "','" + Convert.ToString(cmbOfflineItems.SelectedValue).Trim().ToUpper() + "','" + Convert.ToString(PubFun.Login_User).ToUpper() + "',SYSDATE)";
                fun.EXEC_QUERY(query);
            }
            catch (Exception ex) { }
            finally { }
        }
        public bool RecordPDIOK(Tractor tractor)
        {
            try
            {
                //query = string.Format(@"update xxes_job_status set PDIOKDATE=SYSDATE+  (1/1440*12),
                //PDIDONEBY='{0}' where fcode_srlno='{1}' and PDIOKDATE is null", PubFun.Login_User, tractor.TSN);
                //EXEC_QUERY(query);
                query = string.Format(@"update xxes_job_status set PDIOKDATE=SYSDATE,
                PDIDONEBY='{0}' where fcode_srlno='{1}' and PDIOKDATE is null", HttpContext.Current.Session["Login_User"].ToString(), tractor.TSN);
                return fun.EXEC_QUERY(query);
                //query = string.Format(@"select count(*) from XXES_PDIOK Where TSN='{0}'",
                //    tractor.TSN);
                //if (!CheckExits(query))
                //{
                //    query = string.Format(@"insert into XXES_PDIOK(plant_code,family_code,TSN,CREATEDBY) 
                //values('{0}','{1}','{2}','{3}')", tractor.PLANT, tractor.FAMILY, tractor.TSN, PubFun.Login_User);
                //    EXEC_QUERY(query);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}