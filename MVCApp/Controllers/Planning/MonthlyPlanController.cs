using ClosedXML.Excel;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers
{
    [Authorize]
    public class MonthlyPlanController : Controller
    {
        OleDbConnection Econ;
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();

        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";

        public ActionResult Index()
        {
            return View();
        }      
       
        public JsonResult AddMonthlyPlan(MonthlyPlanModel MP)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(MP.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(MP.Family))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(MP.Date))
                {
                    msg = "Please Select Month & Year ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //if (string.IsNullOrEmpty(MP.Year))
                //{
                //    msg = "Please Select Year ..";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                //if (string.IsNullOrEmpty(MP.Month))
                //{
                //    msg = "Please Select Month ..";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}

                string Month = string.Empty;
                string Year = string.Empty;

                char[] spearator = { ' ' };
                String[] strDate = MP.Date.Split(spearator, StringSplitOptions.None);
                Month = strDate[0].ToUpper();
                Year = strDate[1].ToUpper();


                query = "INSERT INTO XXES_MONTHLYPLANNING(PLANT_CODE, FAMILY_CODE, ITEM_CODE, QTY, MONTH, YEAR, CREATEDBY, CREATEDDATE) " +
                    "VALUES('" + MP.Plant + "', '" + MP.Family + "', '" + MP.ItemCode + "' , '" + MP.Qty + "','" + Month + "', '" + Year + "','" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "', SYSDATE)";
                if (fun.EXEC_QUERY(query))
                {
                    //fun.Insert_Into_ActivityLog("MONTHLY_PLAN", "INSERT", Convert.ToString(data.AutoId), query, data.Plant, data.Family)
                    msg = "Data Saved Successfully...";
                    mstType = "alert-success";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult Grid(MonthlyPlanModel data)
        {
            string Month = string.Empty;
            string Year = string.Empty;

            char[] spearator = { ' ' };
            String[] strDate = data.Date.Split(spearator, StringSplitOptions.None);
            Month = strDate[0].ToUpper();
            Year = strDate[1].ToUpper();

            //query = @"SELECT B.AUTOID, B.PLANT_CODE, B.FAMILY_CODE, B.ITEM_CODE, SUBSTR( M.ITEM_DESCRIPTION, 1, 50 ) AS DESCRIPTION, M.SHORT_CODE AS SHORT_DESC,  
            //            B.MONTH || ' ' || B.YEAR  AS MONTHYEAR, B.QTY AS QUANTITY FROM XXES_MONTHLYPLANNING B JOIN XXES_ITEM_MASTER M
            //            ON B.ITEM_CODE = M.ITEM_CODE AND B.PLANT_CODE = M.PLANT_CODE AND B.FAMILY_CODE = M.FAMILY_CODE
            //             WHERE
            //            B.PLANT_CODE = '" + data.Plant + "' AND B.FAMILY_CODE = '" + data.Family + "' AND B.YEAR = '" + Year + "' AND B.MONTH = '" + Month + "' ORDER BY B.AUTOID";

            query = @"select AUTOID, PLANT_CODE, ITEM_CODE, DESCRIPTION, SHORT_DESC,MONTHYEAR, QUANTITY, ACTUAL, (QUANTITY - nvl(ACTUAL,0)) AS PENDING  from 
                        (SELECT B.AUTOID, B.PLANT_CODE, B.FAMILY_CODE, B.ITEM_CODE, SUBSTR( M.ITEM_DESCRIPTION, 1, 50 ) AS DESCRIPTION, M.SHORT_CODE AS SHORT_DESC,  
                        B.MONTH || ' ' || B.YEAR  AS MONTHYEAR, B.QTY AS QUANTITY,
                        (select count(*) from xxes_job_status j where j.item_code=b.item_code and j.plant_code=b.plant_code and j.family_code=b.family_code 
                        and  upper(ltrim(TO_CHAR(j.entrydate,'Mon'),'0') )=upper(b.month) and upper(ltrim(TO_CHAR(j.entrydate,'yyyy'),'0'))=b.year) ACTUAL 
                        
                        FROM XXES_MONTHLYPLANNING B JOIN XXES_ITEM_MASTER M 
                        ON B.ITEM_CODE = M.ITEM_CODE AND B.PLANT_CODE = M.PLANT_CODE AND B.FAMILY_CODE = M.FAMILY_CODE 
                         WHERE 
                        B.PLANT_CODE = '" + data.Plant + "' AND B.FAMILY_CODE = '" + data.Family + "' AND B.YEAR = '" + Year + "' AND B.MONTH = '" + Month + "' ORDER BY B.AUTOID)a";

            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }
        public JsonResult DeleteMonthlyPlanItem(MonthlyPlanModel MP)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                query = "DELETE FROM XXES_MONTHLYPLANNING WHERE AUTOID = '" + MP.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    //fun.Insert_Into_ActivityLog("MONTHLY_PLAN", "INSERT", Convert.ToString(data.AutoId), query, data.Plant, data.Family)
                    msg = "Item Deleted Successfully...";
                    mstType = "alert-success";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditMonthlyPlan(MonthlyPlanModel MP)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                query = "UPDATE XXES_MONTHLYPLANNING SET QTY = '" + MP.Qty + "' WHERE AUTOID = '" + MP.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    //fun.Insert_Into_ActivityLog("DAILY_PLAN_TRAN", "Update_Sqn_While_Update", Convert.ToString(data.AutoId), query, data.Plant, data.Family);     
                    msg = "Data Updated Successfully...";
                    mstType = "alert-success";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportToExcel(MonthlyPlanModel MP)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(MP.DateExcel);

            ws.Cell("A1").Value = "Item";
            ws.Cell("B1").Value = "Qty";
            ws.Range("A1:B1").Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            string FilePath = Server.MapPath("~/TempExcelFile/" + MP.DateExcel + ".xlsx");
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }
                 
            wb.SaveAs(FilePath);

            msg = "Format downloaded ...";
            mstType = "alert-info";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        [HttpPost]
        public ActionResult ImportExcelMonthlyPlan(MonthlyPlanModel MP, HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(MP.FamilyExcel))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(MP.FamilyExcel))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(MP.DateExcel))
                {
                    msg = "Please Select Month & Year ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //if (string.IsNullOrEmpty(MP.YearExcel))
                //{
                //    msg = "Please Select Year ..";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                //if (string.IsNullOrEmpty(MP.MonthExcel))
                //{
                //    msg = "Please Select Month ..";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                if (inputFile == null || inputFile.ContentLength == 0)
                {
                    msg = "Please Choose Excel Sheet ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string Month = string.Empty;
                string Year = string.Empty;

                char[] spearator = { ' ' };
                String[] strDate = MP.DateExcel.Split(spearator, StringSplitOptions.None);
                Month = strDate[0].ToUpper();
                Year = strDate[1].ToUpper();

                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    string path = Server.MapPath("~/TempExcelFile/" + inputFile.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    inputFile.SaveAs(path);
                    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
                    OleDbConnection connection = new OleDbConnection();
                    connection.ConnectionString = constr;
                    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + MP.DateExcel +"$]", connection);

                    OleDbDataAdapter da = new OleDbDataAdapter(command);
                    //DataSet ds = new DataSet();
                    //da.Fill(ds);
                    dt = new DataTable();
                    da.Fill(dt);

                    int count = 0;

                    if (MP.IsOverride == true)
                    {
                        query = "DELETE FROM XXES_MONTHLYPLANNING WHERE PLANT_CODE = '" + MP.PlantExcel + "' AND FAMILY_CODE = '" + MP.FamilyExcel + "' AND YEAR = '" + Year + "' AND MONTH = '" + Month + "'";
                        fun.EXEC_QUERY(query);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        query = "INSERT INTO XXES_MONTHLYPLANNING(PLANT_CODE, FAMILY_CODE, ITEM_CODE, QTY, MONTH, YEAR, CREATEDBY, CREATEDDATE) " +
                                "VALUES('" + MP.PlantExcel + "', '" + MP.FamilyExcel + "', '" + row["Item"].ToString() + "' , '" + row["Qty"].ToString() + "','" + Month + "', '" + Year + "','" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "', SYSDATE)";
                        if (fun.EXEC_QUERY(query))
                        {
                            //fun.Insert_Into_ActivityLog("MONTHLY_PLAN", "INSERT", Convert.ToString(data.AutoId), query, data.Plant, data.Family)
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        msg = "Total " + Convert.ToString(count) + " Items Saved Successfully...";
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ValidateFileExtention(HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    mstType = "alert-success";
                }
                else
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public PartialViewResult CheckItemCodeExistAndGridDisplayMonthlyPlan(MonthlyPlanModel MP, HttpPostedFileBase inputFile) 
        {
            DataTable DTable = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;

            try
            {
                if (inputFile.FileName.EndsWith("xlx") || inputFile.FileName.EndsWith("xlsx"))
                {
                    string path = Server.MapPath("~/TempExcelFile/" + inputFile.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    inputFile.SaveAs(path);
                    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
                    OleDbConnection connection = new OleDbConnection();
                    connection.ConnectionString = constr;
                    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + MP.DateExcel + "$]", connection);

                    OleDbDataAdapter da = new OleDbDataAdapter(command);
                    da.Fill(DTable);

                    int count = 0;
                    List<ExcelGridList> EGL = new List<ExcelGridList>();
                    foreach (DataRow row in DTable.Rows)
                    {
                        query = "Select Count(*) from XXES_ITEM_MASTER where FAMILY_CODE = '" + MP.FamilyExcel + "' and ITEM_CODE = '" + row["Item"].ToString() + "'";
                        if (fun.CheckExits(query))
                        {
                            EGL.Add(new ExcelGridList { Plant = MP.PlantExcel, Family = MP.FamilyExcel, ItemCode = row["Item"].ToString(), Qty = Convert.ToInt32(row["Qty"]), IsCorrect = 1 });
                        }
                        else
                        {
                            EGL.Add(new ExcelGridList { Plant = MP.PlantExcel, Family = MP.FamilyExcel, ItemCode = row["Item"].ToString(), Qty = Convert.ToInt32(row["Qty"]), IsCorrect = 0 });
                        }
                    }
                    var CountUnmachedItem = EGL.Where(x => x.IsCorrect == 0).Select(x => x.IsCorrect).Count();
                    if (CountUnmachedItem > 0)
                    {
                        ViewBag.ExportButton = "Hide";
                    }
                    else
                    {
                        ViewBag.ExportButton = "Show";
                    }
                    ViewBag.DataSource = EGL;
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return PartialView("RecordNotFoundGrid");
            }
           
            return PartialView();
        }

        public PartialViewResult BindItems(AddDailyPlanModel data)
        {
            ViewBag.Items = new SelectList(FillItemMaster(data), "Value", "Text");
            return PartialView();
        }

        private List<DDLTextValue> FillItemMaster(AddDailyPlanModel data)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string Year = fun.GetServerDateTime().Year.ToString();
                string Month = fun.GetServerDateTime().ToString("dd-MMM-yyyy");

                char[] spearator = { '-' };

                String[] strMonth = Month.Split(spearator, StringSplitOptions.None);


                string query = string.Empty;

                //DataTable dt = pbf.returnDataSetERP("select segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from mtl_system_items where organization_id in (149,150) and substr(segment1, 1, 1) in ('D', 'F', 'S')");
                //DataTable dt = pbf.returnDataTable("select segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + PubFun.ITEMS_USER + ".mtl_system_items where organization_id in (149,150) and substr(segment1, 1, 1) in ('F') order by segment1");
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Family))
                {
                    if (Convert.ToString(data.Family).ToUpper().Contains("TRACTOR"))
                    {
                        if (Convert.ToString(data.Plant).Trim().ToUpper() == "T05")
                            query = @"select SHORT_CODE,ITEM_CODE as FCODE ,ITEM_CODE || ' # ' || SUBSTR(REPLACE(ITEM_DESCRIPTION,'POWER FARMTRAC','PT'),0,150) as DESCRIPTION from XXES_ITEM_MASTER where ITEM_CODE is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";
                        else if (Convert.ToString(data.Plant).Trim().ToUpper() == "T04")
                            query = "select SHORT_CODE,ITEM_CODE as FCODE ,ITEM_CODE || ' # ' || SUBSTR(REPLACE(ITEM_DESCRIPTION,'TRACTOR FARMTRAC','FT'),0,150) as DESCRIPTION from XXES_ITEM_MASTER where ITEM_CODE is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";

                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                    {
                        query = "select BACKEND as FCODE ,BACKEND || ' # ' || nvl(backend_desc,'') as DESCRIPTION from xxes_backend_master where BACKEND is not null and plant_code='" + Convert.ToString(data.Plant).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    }
                    if (!string.IsNullOrEmpty(query))
                    {
                        dt = fun.returnDataTable(query);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            Item.Add(new DDLTextValue
                            {
                                Text = dr["DESCRIPTION"].ToString(),
                                Value = dr["FCODE"].ToString(),
                            });
                        }
                    }                    
                }
                return Item;
            }
            catch (Exception ex)
            {
                //throw;
                return Item;
            }
            finally { }
        }

        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_FamilyOnlyTractor(Plant), "Value", "Text");
            }
            return PartialView();
        }
    }

    public class ExcelGridList
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; }
        public int Qty { get; set; }
        public int IsCorrect { get; set; }
    }
}