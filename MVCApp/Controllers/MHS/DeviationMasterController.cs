using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.MHS
{
    public class DeviationMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: DeviationMaster
        public ActionResult Index()
        {
            try
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
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
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
            List<DDLTextValue> Result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                Result = fun.Fill_All_Family(Plant);
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FillVendorName(DeviationModels data)
        {
            List<DDLTextValue> _Vendor = new List<DDLTextValue>();
            try        
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                DataTable dt = new DataTable();
                
                    query = string.Format(@"SELECT DISTINCT VENDOR_CODE || '#' || VENDOR_NAME AS VENDOR_NAME , VENDOR_CODE AS VENDOR_CODE FROM ITEM_RECEIPT_DETIALS 
                            WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND (VENDOR_CODE LIKE '%{2}%' OR VENDOR_NAME LIKE '%{3}%')  ORDER BY VENDOR_CODE", 
                            data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(),data.VendorName.Trim().ToUpper(),data.VendorName.Trim().ToUpper());
                                 
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Vendor.Add(new DDLTextValue
                        {
                            Text = dr["VENDOR_NAME"].ToString(),
                            Value = dr["VENDOR_CODE"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(_Vendor, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult FillItemCode(DeviationModels data)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                DataTable dt = new DataTable();

                query = string.Format(@"SELECT DISTINCT ITEM_CODE || '#' || ITEM_DESCRIPTION AS DESCRIPTION , ITEM_CODE AS ITEM_CODE  FROM XXES_RAWMATERIAL_MASTER 
                        WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND (ITEM_CODE LIKE '%{2}%' OR ITEM_DESCRIPTION LIKE '%{3}%') ORDER BY ITEM_CODE", 
                        data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(), data.ItemCode.Trim().ToUpper(),data.ItemCode.Trim().ToUpper());
               
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Item.Add(new DDLTextValue
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
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Save(DeviationModels data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string qty = string.Empty;
           
            try
            {
                if (string.IsNullOrEmpty(data.DeviationQty) && string.IsNullOrEmpty(data.EndDate))
                {
                    msg = "Enter Either Deviation Qty Or End Date..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if(!string.IsNullOrEmpty(data.DeviationQty))
                {
                    if (Convert.ToInt32(data.DeviationQty) <= 0)
                    {
                        msg = Validation.str44;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                } 
                if(string.IsNullOrEmpty(data.AprovedBy))
                {
                    msg = "Please Enter Aproval Name..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string[] item = StringSpliter(data.VendorName);
                data.VendorCode = item[0].Trim();
                data.VendorName = replaceApostophi(item[1].Trim());

                string[] items = StringSpliter(data.ItemCode);
                data.ItemCode = items[0].Trim();
                data.Description = replaceApostophi(items[1].Trim());
                if (!string.IsNullOrEmpty(data.DeviationQty))
                {
                    query = string.Format(@"SELECT DEVIATIONQTY FROM XXES_DEVIATION_MASTER  WHERE CREATEDDATE=(SELECT MAX(CREATEDDATE) FROM XXES_DEVIATION_MASTER 
                        WHERE PLANT='{0}' AND FAMILY='{1}' AND VENDORCODE='{2}' AND ITEMCODE='{3}')", data.Plant.Trim().ToUpper(),
                           data.Family.Trim().ToUpper(), data.VendorCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
                    string line = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        qty = line.Split()[0].Trim().ToUpper();
                        if (Convert.ToInt32(qty) > 0)
                        {
                            msg = "Deviation Already Exist..!";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var err = new { Msg = msg, ID = mstType, validation = status };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
                else if (!string.IsNullOrEmpty(data.EndDate))
                {
                    string lastdate = string.Empty;
                    query = string.Format(@"SELECT ENDDATE FROM XXES_DEVIATION_MASTER  WHERE CREATEDDATE=(SELECT MAX(CREATEDDATE) FROM XXES_DEVIATION_MASTER 
                            WHERE PLANT='{0}' AND FAMILY='{1}' AND VENDORCODE='{2}' AND ITEMCODE='{3}' AND ENDDATE IS NOT NULL)", data.Plant.Trim().ToUpper(),
                            data.Family.Trim().ToUpper(), data.VendorCode.Trim().ToUpper(), data.ItemCode.Trim().ToUpper());
                    string line = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        lastdate = line.Split()[0].Trim().ToUpper();
                        if (DateTime.Parse(lastdate) > Convert.ToDateTime(data.EndDate))
                        {
                            msg = "Deviation Already Exist..!";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var err = new { Msg = msg, ID = mstType, validation = status };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                    }
                }              
                if(!string.IsNullOrEmpty(data.DeviationQty))
                {
                    query = string.Format(@"INSERT INTO XXES_DEVIATION_MASTER(PLANT,FAMILY,VENDORCODE,VENDORNAME,ITEMCODE,DEVIATIONQTY,CREATEDBY,CREATEDDATE,DEVIATIONTYPE,APPROVEDBY,Remarks,OPENINGQTY)
                            VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}', SYSDATE,'{7}','{8}','{9}','{10}')", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(), data.VendorCode,
                            data.VendorName, data.ItemCode, data.DeviationQty, HttpContext.Session["Login_User"].ToString().ToUpper().Trim(), data.DeviationType,data.AprovedBy.Trim().ToUpper(), data.Remarks.Trim().ToUpper(), data.DeviationQty);
                }
                else if(!string.IsNullOrEmpty(data.EndDate))
                {
                    query = string.Format(@"INSERT INTO XXES_DEVIATION_MASTER(PLANT,FAMILY,VENDORCODE,VENDORNAME,ITEMCODE,CREATEDBY,CREATEDDATE, ENDDATE,DEVIATIONTYPE,APPROVEDBY,Remarks)
                            VALUES('{0}','{1}','{2}','{3}','{4}','{5}',SYSDATE, '{6}','{7}','{8}','{9}')", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(), data.VendorCode,
                            data.VendorName, data.ItemCode,  HttpContext.Session["Login_User"].ToString().ToUpper().Trim(), data.EndDate, data.DeviationType,data.AprovedBy.Trim().ToUpper(), data.Remarks.Trim().ToUpper());
                }               
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Data Saved successfully...";
                    mstType = "alert-success";
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

        public JsonResult Update(DeviationModels data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string qty = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.DeviationQty) && string.IsNullOrEmpty(data.EndDate))
                {
                    msg = "Enter Either Deviation Qty Or End Date..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.DeviationQty))
                {
                    if (Convert.ToInt32(data.DeviationQty) <= 0)
                    {
                        msg = Validation.str44;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (string.IsNullOrEmpty(data.AprovedBy))
                {
                    msg = "Please Enter Aproval Name..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                string[] item = StringSpliter(data.VendorName);
                data.VendorCode = item[0].Trim();
                data.VendorName = replaceApostophi(item[1].Trim());

                string[] items = StringSpliter(data.ItemCode);
                data.ItemCode = items[0].Trim();
                data.Description = replaceApostophi(items[1].Trim());
                if (!string.IsNullOrEmpty(data.DeviationQty))
                {
                    query = string.Format(@"SELECT DEVIATIONQTY FROM XXES_DEVIATION_MASTER WHERE AUTOID='{0}'", data.AUTOID);
                    DataTable dt = new DataTable();
                    dt = fun.returnDataTable(query);
                    qty = Convert.ToString(dt.Rows[0]["DEVIATIONQTY"]);
                    if (Convert.ToInt32(qty) == 0)
                    {
                        msg = "Deviation Can't Updated..!";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }

                }
                else if (!string.IsNullOrEmpty(data.EndDate))
                {
                    string lastdate = string.Empty; DateTime currentDateTime = DateTime.Now.Date;
                    query = string.Format(@"SELECT ENDDATE FROM XXES_DEVIATION_MASTER  WHERE AUTOID='{0}'", data.AUTOID);
                    string line = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        lastdate = line.Split()[0].Trim().ToUpper();
                        if (DateTime.Parse(lastdate) < currentDateTime)
                        {
                            msg = "Deviation Can't Updated..!";
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var err = new { Msg = msg, ID = mstType, validation = status };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(data.DeviationQty))
                {
                    query = string.Format(@"UPDATE XXES_DEVIATION_MASTER SET PLANT='{0}' , FAMILY='{1}', VENDORCODE='{2}', VENDORNAME='{3}' , ITEMCODE='{4}' , DEVIATIONQTY='{5}' ,
                            UPDATEDBY='{6}', UPDATEDDATE=SYSDATE ,DEVIATIONTYPE='{7}',APPROVEDBY='{8}',OPENINGQTY='{9}',Remarks='{10}' WHERE AUTOID ='{11}'", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(),
                            data.VendorCode.Trim(), data.VendorName.Trim(), data.ItemCode.Trim(), data.DeviationQty.Trim(),
                            HttpContext.Session["Login_User"].ToString().ToUpper().Trim(), data.DeviationType , data.AprovedBy.Trim().ToUpper(), data.DeviationQty, data.Remarks.Trim(), data.AUTOID);
                }
                else if (!string.IsNullOrEmpty(data.EndDate))
                {
                    query = string.Format(@"UPDATE XXES_DEVIATION_MASTER SET PLANT='{0}' , FAMILY='{1}', VENDORCODE='{2}', VENDORNAME='{3}' , ITEMCODE='{4}', 
                            UPDATEDBY='{5}', UPDATEDDATE=SYSDATE , ENDDATE='{6}',DEVIATIONTYPE='{7}',APPROVEDBY='{8}',Remarks='{9}' WHERE AUTOID ='{10}'", data.Plant.Trim().ToUpper(),
                            data.Family.Trim().ToUpper(), data.VendorCode.Trim(), data.VendorName.Trim(), data.ItemCode.Trim(),
                            HttpContext.Session["Login_User"].ToString().ToUpper().Trim(), data.EndDate.Trim(), data.DeviationType ,data.AprovedBy.Trim().ToUpper(), data.Remarks.Trim(), data.AUTOID);
                }
                    
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Data Updated successfully...";
                    mstType = "alert-success";
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

        public PartialViewResult Grid(DeviationModels data)
        {
            try
            {
                
                string[] item = StringSpliter(data.VendorName);
                data.VendorCode = item[0].Trim();
                data.VendorName = replaceApostophi(item[1].Trim());

                query = string.Format(@"SELECT XDM.AUTOID, XDM.PLANT,XDM.FAMILY,XDM.VENDORCODE,XDM.VENDORNAME,XDM.ITEMCODE,xrm.ITEM_DESCRIPTION, XDM.DEVIATIONQTY, XDM.DEVIATIONTYPE,TO_CHAR(XDM.ENDDATE,'DD-Mon-YYYY') ENDDATE,
                        XDM.APPROVEDBY , XDM.CREATEDBY, TO_CHAR(XDM.CREATEDDATE , 'DD-MM-YYYY HH24:MM:SS') CREATEDDATE,XDM.UPDATEDBY,TO_CHAR(XDM.UPDATEDDATE,'DD-MM-YYYY HH24:MM:SS')
                        UPDATEDDATE , XDM.OPENINGQTY, XDM.Remarks  FROM XXES_DEVIATION_MASTER xdm JOIN XXES_RAWMATERIAL_MASTER xrm ON XDM.PLANT= XRM.PLANT_CODE AND XDM.FAMILY=XRM.FAMILY_CODE AND XDM.ITEMCODE= XRM.ITEM_CODE
                        WHERE XDM.PLANT='{0}' AND XDM.FAMILY='{1}' AND XDM.VENDORNAME like '%{2}%' ORDER BY XDM.VENDORNAME DESC", data.Plant, data.Family,data.VendorName);
                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSource = dt;
            return PartialView();
        }

        public string[] StringSpliter(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string Strr = str;
                string[] StrrArr = Strr.Split('#');
                return StrrArr;
            }
            else
            {
                string[] StrrArr = { "", "" };
                return StrrArr;
            }
        }

        public string replaceApostophi(string chkstr)
        {
            chkstr = chkstr.Replace("\"", string.Empty).Trim(); //"
            return chkstr.Replace("'", "''");  //'
        }
    }
}