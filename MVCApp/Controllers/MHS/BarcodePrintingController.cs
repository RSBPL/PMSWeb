using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace MVCApp.Controllers.MHS
{
    [Authorize]
    public class BarcodePrintingController : Controller
    {
        Function fun = new Function();
        ReportDocument RptDoc; 
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

        public ActionResult Grid(Barcode obj)
        {
            List<Barcode> barcodeList = null;
            Barcode GR = new Barcode();
            int recordsTotal = 0;

            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            if (obj.BarcodeType == "KANBAN")
            {
                barcodeList = fun.GridKANBAN(obj);
                if (barcodeList.Count > 0)
                {
                    recordsTotal = barcodeList[0].TOTALCOUNT;
                }

            }
            else if (obj.BarcodeType == "BULKSTORAGE")
            {
                barcodeList = fun.GridBULKSTORAGE(obj);
                if (barcodeList.Count > 0)
                {
                    recordsTotal = barcodeList[0].TOTALCOUNT;
                }

            }
            else if (obj.BarcodeType == "SUPERMARKET")
            {
                barcodeList = fun.GridSUPERMARKET(obj);
                if (barcodeList.Count > 0)
                {
                    recordsTotal = barcodeList[0].TOTALCOUNT;
                }

            }

            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = barcodeList }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Print(List<Barcode> obj)
        {
            string msg = string.Empty, mstType = string.Empty, mode = string.Empty, Plant = string.Empty, Family = string.Empty,
            query = string.Empty, Type = string.Empty; bool Printing_done = false;
            try
            {
                foreach (Barcode br in obj)
                {
                    Plant = br.Plant;
                    Family = br.Family;
                    Type = br.BarcodeType;
                    
                    break;
                }

                if (Type.ToUpper() == "KANBAN")
                {
                    List<Barcode> barcodeList = new List<Barcode>();
                    //Barcode barcode = new Barcode();
                    query = string.Format(@"SELECT PARAMVALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='KANBAN_PRINT' AND PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}'", Plant.Trim(), Family.Trim());
                    string ParamValue = fun.get_Col_Value(query);
                    if (string.IsNullOrEmpty(ParamValue))
                    {
                        msg = "Printing Option is not enable..";
                        mstType = Validation.str1;
                        mode = "ERROR";
                        var resul = new { Msg = msg, Mode = mode, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else if (ParamValue.ToUpper() == "PDF")
                    {
                        
                        var KanbanList = new List<string>();
                        foreach (Barcode br in obj)
                        {

                            KanbanList.Add("'" + Convert.ToString(br.LOCATION).Trim() + "'");
                        }
                        query = string.Format(@"SELECT xkm.item_code,xkm.item_description DESCRIPTION ,xkm.kanban_no,
                    SUBSTR(xkm.sumktloc,1,4) SM_ROWNO,SUBSTR(xkm.sumktloc,5) SM_SHELFNO,
                    (SUBSTR(xkm.kanban_no, INSTR(xkm.kanban_no, '-') + 1)||'/'||
                    
                    (select COUNT(*) FROM xxes_kanban_master mm WHERE mm.KANBAN_NO LIKE SUBSTR(xkm.KANBAN_NO,1,9) || '%'  AND mm.PLANT_CODE=xkm.plant_code )
                    
                    ) BIN_NO,
               
                    SUBSTR(location_code, 1, INSTR(location_code, '-') -1 ) BULK_ROWNO , SUBSTR(bsi.location_code, INSTR(bsi.location_code, '-') + 1) BULK_SHELFNO,
                    bsi.PACKAGING_TYPE PKG,bsi.bulk_storage_snp SNP
                     FROM XXES_KANBAN_MASTER xkm
                     INNER JOIN xxes_bulk_storage bsi
                     ON xkm.plant_code = bsi.plant_code and xkm.family_code = bsi.family_code and xkm.item_code = bsi.item_code
                     INNER JOIN xxes_rawmaterial_master xrm
                     ON xkm.plant_code = xrm.plant_code and xkm.family_code = xrm.family_code and xkm.item_code = xrm.item_code
                    where xkm.plant_code = '{0}' AND xkm.family_code = '{1}' AND xkm.SUMKTLOC IN ({2})
                      ORDER BY XKM.kanban_no", Plant, Family, string.Join(",", KanbanList));



                        DataTable dt = fun.returnDataTable(query);
                        dt.Columns.Add("QRCode", typeof(byte[]));

                        DataRow dr = null;
                        if (RptDoc == null)
                        {
                            RptDoc = new ReportDocument();
                        }

                        //RptDoc.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "Kanban.rpt"));
                        RptDoc.Load(Server.MapPath(@"~/CrystalReports/kanban.rpt"));
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string Kanban = Convert.ToString(dt.Rows[i]["KANBAN_NO"]);
                                dr = dt.Rows[i];
                                dr["QRCode"] = GenerateQrCode(Kanban);
                            }
                            RptDoc.SetDataSource(dt);
                            string fname = "kanban.pdf";
                            string severFilePath = Server.MapPath("~/TempExcelFile/");
                            if (!Directory.Exists(severFilePath))
                            {
                                Directory.CreateDirectory(severFilePath);
                            }

                            string strPath = severFilePath + fname;
                            //string strPath = Server.MapPath("~/TempExcelFile/" + fname);
                            RptDoc.ExportToDisk(ExportFormatType.PortableDocFormat, strPath);
                            msg = "File Exported Successfully ...";
                            mstType = Validation.str;
                            mode = "PDF";
                            var resul = new { Msg = msg, Mode = mode, ID = mstType, PDFName = fname };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            msg = "No record found for kanban printing";
                            mstType = Validation.str1;
                            mode = "ERROR";
                            var resul = new { Msg = msg, Mode = mode, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (ParamValue.ToUpper() == "BARCODES")
                    {
                        //var KanbanBarcodeList = new List<string>();
                        foreach (Barcode br in obj)
                        {

                            //KanbanBarcodeList.Add("'" + Convert.ToString(br.LOCATION).Trim() + "'");
                            query = string.Format(@"SELECT xkm.plant_code,xkm.family_code,xkm.item_code,xkm.item_description ITEM_DESC ,
                                    xkm.kanban_no,xkm.sumktloc SUPER_LOC, (SUBSTR(xkm.kanban_no, INSTR(xkm.kanban_no, '-') + 1)||'/'||
                    (select COUNT(*) FROM xxes_kanban_master mm WHERE mm.SUMKTLOC = '{0}'  AND mm.PLANT_CODE=xkm.plant_code AND 
                     mm.FAMILY_CODE=xkm.FAMILY_CODE AND mm.SUMKTLOC=xkm.SUMKTLOC)) NO_OF_BIN,bsi.location_code BULK_LOC,
                     bsi.PACKAGING_TYPE BPACKAGING,bsi.bulk_storage_snp BSNP FROM XXES_KANBAN_MASTER xkm INNER JOIN xxes_bulk_storage bsi
                     ON xkm.plant_code = bsi.plant_code and xkm.family_code = bsi.family_code and xkm.item_code = bsi.item_code
                     INNER JOIN xxes_rawmaterial_master xrm ON xkm.plant_code = xrm.plant_code and xkm.family_code = xrm.family_code and 
                     xkm.item_code = xrm.item_code where xkm.plant_code = '{1}' AND xkm.family_code = '{2}' AND xkm.SUMKTLOC = '{0}' 
                     AND xkm.KANBAN_NO = '{3}'",br.LOCATION.Trim(),br.Plant.Trim(),br.Family.Trim(),br.KANBAN.Trim());

                            DataTable dtKanbanBC = fun.returnDataTable(query);
                            if (dtKanbanBC.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dtKanbanBC.Rows)
                                {
                                    

                                    barcodeList.Add(new Barcode() {
                                    Plant = Convert.ToString(dr["PLANT_CODE"]),
                                    Family = Convert.ToString(dr["FAMILY_CODE"]),
                                    ITEMCODE = Convert.ToString(dr["ITEM_CODE"]),
                                    ITEM_DESCRIPTION = Convert.ToString(dr["ITEM_DESC"]),
                                    LOCATION = Convert.ToString(dr["KANBAN_NO"]),
                                    SUPERMKT_LOC = Convert.ToString(dr["SUPER_LOC"]),
                                    BIN_NO = Convert.ToString(dr["NO_OF_BIN"]),
                                    BLK_LOC = Convert.ToString(dr["BULK_LOC"]),
                                    PACKINGTYPE = Convert.ToString(dr["BPACKAGING"]),
                                    BULKSTORESNP = Convert.ToString(dr["BSNP"])
                                });


                                }
                                //barcodeList.Add(barcode);
                            }

                        }
                    //    query = string.Format(@"SELECT xkm.plant_code,xkm.family_code,xkm.item_code,xkm.item_description ITEM_DESC ,xkm.kanban_no,xkm.sumktloc SUPER_LOC,
                    //(SUBSTR(xkm.kanban_no, INSTR(xkm.kanban_no, '-') + 1)||'/'||
                    //(select COUNT(*) FROM xxes_kanban_master mm WHERE mm.KANBAN_NO LIKE SUBSTR(xkm.KANBAN_NO,1,9) || '%'  AND mm.PLANT_CODE=xkm.plant_code )
                    //) NO_OF_BIN,bsi.location_code BULK_LOC,
                    // bsi.PACKAGING_TYPE BPACKAGING,bsi.bulk_storage_snp BSNP
                    // FROM XXES_KANBAN_MASTER xkm
                    // INNER JOIN xxes_bulk_storage bsi
                    // ON xkm.plant_code = bsi.plant_code and xkm.family_code = bsi.family_code and xkm.item_code = bsi.item_code
                    // INNER JOIN xxes_rawmaterial_master xrm
                    // ON xkm.plant_code = xrm.plant_code and xkm.family_code = xrm.family_code and xkm.item_code = xrm.item_code
                    //where xkm.plant_code = '{0}' AND xkm.family_code = '{1}' AND xkm.SUMKTLOC IN ({2}) AND xkm.KANBAN_NO = '{3}'
                    //  ORDER BY XKM.kanban_no", Plant, Family, string.Join(",", KanbanBarcodeList));



                        
                        
                        //if (dtKanbanBC.Rows.Count > 0)
                        //{
                        //    barcodeList = (from DataRow dr in dtKanbanBC.Rows
                        //                   select new Barcode()
                        //                   {
                        //                       Plant = Convert.ToString(dr["PLANT_CODE"]),
                        //                       Family = Convert.ToString(dr["FAMILY_CODE"]),
                        //                       ITEMCODE = Convert.ToString(dr["ITEM_CODE"]),
                        //                       ITEM_DESCRIPTION = Convert.ToString(dr["ITEM_DESC"]),
                        //                       LOCATION = Convert.ToString(dr["KANBAN_NO"]),
                        //                       SUPERMKT_LOC = Convert.ToString(dr["SUPER_LOC"]),
                        //                       BIN_NO = Convert.ToString(dr["NO_OF_BIN"]),
                        //                       BLK_LOC = Convert.ToString(dr["BULK_LOC"]),
                        //                       PACKINGTYPE = Convert.ToString(dr["BPACKAGING"]),
                        //                       BULKSTORESNP = Convert.ToString(dr["BSNP"])
                        //                   }).ToList();
                           
                        //}
                        //else
                        //{
                        //    msg = "No Record Found...!!!";
                        //    mstType = Validation.str1;
                        //    mode = "ERROR";
                        //    var resul = new { Msg = msg, Mode = mode, ID = mstType };
                        //    return Json(resul, JsonRequestBehavior.AllowGet);
                        //}
                        
                        
                        PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                        if (barcodes.PrintLocationBarcodes(barcodeList))
                        {
                            msg = "Barcode Printing Successfully";
                            mstType = Validation.str;   
                            mode = "BARCODE";
                            var resul = new { Msg = msg, Mode = mode, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            msg = "Error in Barcode Printing";
                            mstType = Validation.str1;
                            mode = "ERROR";
                            var resul = new { Msg = msg, Mode = mode, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    PrintAssemblyBarcodes barcodes = new PrintAssemblyBarcodes();
                    if (Type.ToUpper() == "BULKSTORAGE")
                    {
                        
                        foreach(Barcode br in obj)
                        {
                            query = string.Format(@"select count(*) from xxes_bulk_storage where location_code='{0}' and temp_loc='Y'
                            and plant_code='{1}' and family_code='{2}'",
                            br.LOCATION.Trim().ToUpper(),Plant.Trim(),Family.Trim());
                            if (fun.CheckExits(query))
                            {
                                br.LOCTYPE = "TEMP";
                                br.SUPERMKT_LOC = null;
                            }
                            else
                            {
                                br.LOCTYPE = "";
                                query = string.Format(@"select DISTINCT(SUMKTLOC) from xxes_kanban_master where PLANT_CODE = '{0}' AND 
                                                FAMILY_CODE = '{1}' and item_code = '{2}'", Plant.Trim(), Family.Trim(), br.ITEMCODE.Trim());

                                DataTable SM_Datatable = fun.returnDataTable(query);
                                if (SM_Datatable.Rows.Count > 0)
                                {
                                    int totalLocation = SM_Datatable.Rows.Count;
                                    if(totalLocation > 5)
                                    {
                                        string smLocationSum = string.Empty;
                                        for (int i = 0; i < SM_Datatable.Rows.Count; i++)
                                        {
                                            if(i ==5)
                                            {
                                                totalLocation = totalLocation - i;
                                                break;
                                            }
                                            smLocationSum = Convert.ToString(SM_Datatable.Rows[i]["SUMKTLOC"]) + "," + smLocationSum;
                                        }
                                        br.SUPERMKT_LOC = smLocationSum.TrimEnd(',') + " + " + totalLocation;
                                    }
                                    else
                                    {
                                            string smLocationSum = string.Empty;
                                            for (int i = 0; i < SM_Datatable.Rows.Count; i++)
                                            {
                                                
                                                smLocationSum = Convert.ToString(SM_Datatable.Rows[i]["SUMKTLOC"]) + "," + smLocationSum;
                                            }
                                            br.SUPERMKT_LOC = smLocationSum.TrimEnd(',');
                                    }
                                    
                                }
                                else
                                {
                                    br.SUPERMKT_LOC = null;
                                }
                            }

                        }
                        
                        Printing_done = barcodes.PrintBULKLOC_Barcodes(obj);
                    }
                    else if (Type.ToUpper() == "SUPERMARKET")
                    {
                        foreach (Barcode br in obj)
                        {
                            query = string.Format(@"Select DISTINCT(LOCATION_CODE),BULK_STORAGE_SNP from xxes_bulk_storage where plant_code = '{0}' and 
                                        family_code = '{1}' and item_code = '{2}'", Plant.Trim(), Family.Trim(), br.ITEMCODE.Trim());

                            DataTable BL_Datatable = fun.returnDataTable(query);
                            if (BL_Datatable.Rows.Count > 0)
                            {
                                string blLocationSum = string.Empty; string blSNP = string.Empty;
                                for (int i = 0; i < BL_Datatable.Rows.Count; i++)
                                {
                                    blLocationSum = Convert.ToString(BL_Datatable.Rows[i]["LOCATION_CODE"]) + "," + blLocationSum;
                                    blSNP = Convert.ToString(BL_Datatable.Rows[i]["BULK_STORAGE_SNP"]) + "," + blSNP;
                                }
                                br.BLK_LOC = blLocationSum.TrimEnd(',');
                                br.BULKSTORESNP = blSNP.TrimEnd(',');
                            }
                            else
                            {
                                br.BLK_LOC = null;
                                br.BULKSTORESNP = null;
                            }

                        }
                        Printing_done = barcodes.PrintSMLOCBarcodes(obj);
                    }

                    if (Printing_done)
                    {
                        msg = "Barcode Printing Successfully";
                        mstType = Validation.str;
                        mode = "BARCODE";
                        var resul = new { Msg = msg, Mode = mode, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        msg = "Error in Barcode Printing";
                        mstType = Validation.str1;
                        mode = "ERROR";
                        var resul = new { Msg = msg, Mode = mode, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                mode = "ERROR";
                var resul = new { Msg = msg, Mode = mode, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                if(RptDoc !=null)
                {
                    RptDoc.Close();
                    RptDoc.Dispose();
                }
                
            }
            var result = new { Msg = msg, Mode = mode, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);

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
        [HttpGet]
        public ActionResult Download()
        {
            string file = Request.QueryString["File"].ToString();
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/pdf");
        }
    }
}