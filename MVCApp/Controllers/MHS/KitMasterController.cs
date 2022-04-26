using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QRCoder;
using CrystalDecisions.CrystalReports.Engine;
using MVCApp.Common;
using Syncfusion.EJ2.Base;
using System.Collections;
using System.Configuration;
using System.Drawing;
using System.IO;




namespace MVCApp.Controllers.MHS
{
    public class KitMasterController : Controller
    {
        string msg = string.Empty; string mstType = string.Empty;
        Function fun = new Function();
        ReportDocument rd;

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
        public JsonResult BindItemCode(RawMaterialMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                DataTable dt = new DataTable();

               
                query = string.Format(@"SELECT ITEM_CODE || ' # ' || ITEM_DESCRIPTION as DESCRIPTION,ITEM_CODE from XXES_RAWMATERIAL_MASTER
                WHERE substr(ITEM_CODE, 1, 1) in ('D','S') AND (ITEM_CODE LIKE '%{0}%' OR ITEM_DESCRIPTION LIKE '%{0}%' ) 
                AND PLANT_CODE ='{1}' AND FAMILY_CODE = '{2}'
                order by ITEM_CODE", data.ItemCode.Trim().ToUpper(),data.Plant.Trim(),data.Family.Trim());
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
        public JsonResult BindLocation(KitMaster data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                DataTable dt = new DataTable();
                
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ITEMCODE.Split(spearator, StringSplitOptions.None);
                data.ITEMCODE = SplitItemCode[0];
                

                query = string.Format(@"SELECT DISTINCT(SUMKTLOC) FROM XXES_KANBAN_MASTER WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND 
                                        ITEM_CODE = '{2}' ", data.PLANTCODE.Trim(), data.FAMILYCODE.Trim(),data.ITEMCODE.Trim());
                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["SUMKTLOC"].ToString(),
                            Value = dr["SUMKTLOC"].ToString(),
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
        public JsonResult Add(KitMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE) || string.IsNullOrEmpty(data.KITNO) || string.IsNullOrEmpty(data.ITEMCODE) || string.IsNullOrEmpty(data.SMLocation) || string.IsNullOrEmpty(data.QUANTITY))
                {
                    msg = Validation.str32;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                double QTY = 0;
                if (!double.TryParse(data.QUANTITY, out QTY))
                {
                    msg = Validation.str34;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(data.QUANTITY) < 1 )
                {
                    msg = Validation.str34;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
               

                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ITEMCODE.Split(spearator, StringSplitOptions.None);
                data.ITEMCODE = SplitItemCode[0];
                data.ITEMDescription = fun.replaceApostophi(SplitItemCode[1]);

                query = string.Format(@"select count(*) from XXES_KANBAN_MASTER s 
                                        inner join XXES_RAWMATERIAL_MASTER r on s.plant_code = r.plant_code and s.family_code= r.family_code 
                                        and s.ITEM_CODE = r.item_code WHERE S.PLANT_CODE = '{0}' AND S.FAMILY_CODE = '{1}' AND S.SUMKTLOC = '{2}' AND
                                        S.ITEM_CODE = '{3}'", data.PLANTCODE.Trim(),data.FAMILYCODE.Trim(),data.SMLocation.Trim().ToUpper(),data.ITEMCODE.Trim().ToUpper());
                if (Convert.ToInt32(fun.get_Col_Value(query)) < 1)
                {
                    msg = Validation.str41;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                string isValidLocItem = string.Format(@"SELECT COUNT(*) FROM XXES_KITMASTER WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND 
                                                    ITEMCODE = '{2}' AND KITNO = '{3}'", data.PLANTCODE.ToUpper().Trim(), 
                                                    data.FAMILYCODE.ToUpper().Trim(),data.ITEMCODE.Trim(),data.KITNO.Trim().ToUpper());
                if (Convert.ToInt32(fun.get_Col_Value(isValidLocItem)) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertKITMaster(data))
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
        
        public ActionResult PrintKit()
        {
            string KITNO = Request.QueryString["KITNO"].ToString();
            string PLANT = Request.QueryString["PLANT"].ToString();
            string FAMILY = Request.QueryString["FAMILY"].ToString();
            //string SMLOC = Request.QueryString["SMLOC"].ToString();

            DataTable dt = new DataTable();
            try
            {
               string query = string.Format(@"SELECT M.SUMKTLOC SMLocation,M.KITNO,ITEMCODE,R.ITEM_DESCRIPTION ITEMDescription,M.QUANTITY FROM XXES_KITMASTER m JOIN XXES_RAWMATERIAL_MASTER r ON M.PLANT_CODE=r.PLANT_CODE
                AND M.FAMILY_CODE=r.FAMILY_CODE AND M.ITEMCODE=r.ITEM_CODE AND r.PLANT_CODE='{0}' AND M.FAMILY_CODE='{1}' and m.kitno='{2}'",
                PLANT, FAMILY,KITNO);
                dt = fun.returnDataTable(query);
                dt.Columns.Add("QRCode", typeof(byte[]));
                //dt.Columns.Add("KITNO", typeof(string));

                dt.Rows[0]["QRCode"] = GenerateQrCode(KITNO);
                //dt.Rows[0]["KITNO"] = KITNO;
                return GetReport(dt);
            }

            catch (Exception ex)
            {

                throw;
            }
            return GetReport(dt);
        }
        private FileResult GetReport(DataTable dt)
        {
            try
            {
                if (rd == null)
                {
                    rd = new ReportDocument();
                }
                rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "kitting.rpt"));
                rd.SetDataSource(dt);
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
        private byte[] GenerateQrCode(string KITNO)
        {

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(KITNO, QRCodeGenerator.ECCLevel.Q);
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
        public PartialViewResult Grid(KitMaster data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE))
                {
                    return PartialView("Grid");
                }

                string query = string.Format(@"SELECT KM.AUTOID,KM.PLANT_CODE,KM.FAMILY_CODE,KM.KITNO,(SELECT RM.ITEM_CODE || ' # ' || RM.ITEM_DESCRIPTION FROM XXES_RAWMATERIAL_MASTER RM
            WHERE RM.ITEM_CODE=KM.ITEMCODE AND RM.PLANT_CODE = KM.PLANT_CODE AND RM.FAMILY_CODE = KM.FAMILY_CODE) ITEMCODE,KM.SUMKTLOC,KM.QUANTITY,KM.CREATEDBY,to_char(KM.CREATEDDATE, 'dd-Mon-yyyy HH24:MI:SS') AS CREATEDDATE,KM.UPDATEEDBY,
            to_char(KM.UPDATEDDATE, 'dd-Mon-yyyy HH24:MI:SS') UPDATEDDATE FROM XXES_KITMASTER KM
            WHERE KM.PLANT_CODE='{0}' AND KM.FAMILY_CODE='{1}' ORDER BY KM.CREATEDDATE DESC", data.PLANTCODE.ToUpper().Trim(), data.FAMILYCODE.ToUpper().Trim());

               
                DataTable dt = fun.returnDataTable(query);

                ViewBag.DataSource = dt;
            }
            catch(Exception ex)
            {
                fun.LogWrite(ex);
            }

            
            return PartialView("Grid");
        }

        public JsonResult Update(KitMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE) || 
                    string.IsNullOrEmpty(data.KITNO) || string.IsNullOrEmpty(data.ITEMCODE) || string.IsNullOrEmpty(data.SMLocation) || string.IsNullOrEmpty(data.QUANTITY)
                    || string.IsNullOrEmpty(data.AUTOID))
                {
                    msg = Validation.str32;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                double QTY = 0;
                if (!double.TryParse(data.QUANTITY, out QTY))
                {
                    msg = Validation.str34;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(data.QUANTITY) < 1)
                {
                    msg = Validation.str34;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string query = string.Empty;
                char[] spearator = { '#' };
                String[] SplitItemCode = data.ITEMCODE.Split(spearator, StringSplitOptions.None);
                data.ITEMCODE = SplitItemCode[0];
                data.ITEMDescription = fun.replaceApostophi(SplitItemCode[1]);

                query = string.Format(@"select count(*) from XXES_KANBAN_MASTER s 
                                        inner join XXES_RAWMATERIAL_MASTER r on s.plant_code = r.plant_code and s.family_code= r.family_code 
                                        and s.ITEM_CODE = r.item_code WHERE S.PLANT_CODE = '{0}' AND S.FAMILY_CODE = '{1}' AND S.SUMKTLOC = '{2}' AND
                                        S.ITEM_CODE = '{3}'", data.PLANTCODE.Trim(), data.FAMILYCODE.Trim(), data.SMLocation.Trim().ToUpper(), data.ITEMCODE.Trim().ToUpper());
                if (Convert.ToInt32(fun.get_Col_Value(query)) < 1)
                {
                    msg = Validation.str41;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                query = string.Format(@"SELECT COUNT(*) FROM XXES_KITMASTER WHERE PLANT_CODE = '{0}'
                AND FAMILY_CODE = '{1}' AND ITEMCODE = '{2}' AND KITNO = '{3}' and autoid<>{4}",
                data.PLANTCODE.ToUpper().Trim(), data.FAMILYCODE.ToUpper().Trim(),
                data.ITEMCODE.ToUpper().Trim(), data.KITNO.ToUpper().Trim(), data.AUTOID);
                if (fun.CheckExits(query))
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertKITMaster(data))
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

        public JsonResult Delete(KitMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.AUTOID))
                {
                    msg = "Error in deleting, No auto id found";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                if (fun.DeleteKITMaster(data))
                {

                    msg = Validation.str23;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
                else
                {
                    msg = Validation.str33;
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