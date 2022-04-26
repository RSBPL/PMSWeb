using ClosedXML.Excel;
using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class ImportExportController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: ImportExport
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
        
        public JsonResult BindTractor(string plnt, string fam)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(plnt) && !string.IsNullOrEmpty(fam))
            {
                result = Tractor(plnt, fam);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BINDJOB(string plnt,string tractor, string fam)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(plnt) && !string.IsNullOrEmpty(fam) && !string.IsNullOrEmpty(tractor) )
            {
                result = JOB(plnt, tractor, fam);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        public List<DDLTextValue> Tractor(string plnt, string fam)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> tractor = new List<DDLTextValue>();
            try
            {
                query = string.Format(@"Select plant_code,family_code, fcode_srlno || '(' || item_code || ')' as text,
                                     fcode_srlno VALUE from eki_xxes_job_status  where fcode_srlno is not null
                                    and plant_code='{0}'  AND family_code = '{1}' and fcode_srlno not in 
                                    (SELECT fcode_srlno FROM xxes_job_status where fcode_srlno is not null)", plnt, fam);

                //and fcode_srlno NOT IN(select fcode_srlno from xxes_job_status where fcode_srlno is NOT null

                TmpDs = fun.returnDataTable(query);

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        tractor.Add(new DDLTextValue
                        {
                            Text = dr["text"].ToString(),
                            Value = dr["text"].ToString(),
                        });
                    }
                }
                return tractor;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return tractor;
            }
            finally
            {
                fun.ConClose();
            }
        }
        
        public List<DDLTextValue> JOB(string plnt, string tractor, string fam)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> JOB = new List<DDLTextValue>();
            string plant = string.Empty;            
            string orgid = fun.getOrgId(Convert.ToString(plnt.ToUpper().Trim()), Convert.ToString(fam.ToUpper().Trim()));            
            try
            {
                string itemDcode = string.Empty;
                itemDcode = tractor.Split('(', ')')[1].Trim().ToUpper();

                query = string.Format(@"SELECT WIP_ENTITY_NAME FROM RELESEDJOBORDER WHERE SEGMENT1='{1}'
                        AND ORGANIZATION_ID='{0}' AND WIP_ENTITY_NAME NOT IN (SELECT jobid FROM XXES_JOB_STATUS 
                        where jobid is not null ) and WIP_ENTITY_NAME is not null", orgid, itemDcode);

                //and fcode_srlno NOT IN(select fcode_srlno from xxes_job_status where fcode_srlno is NOT null

                TmpDs = fun.returnDataTable(query);

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        JOB.Add(new DDLTextValue
                        {
                            Text = dr["WIP_ENTITY_NAME"].ToString(),
                            Value = dr["WIP_ENTITY_NAME"].ToString(),
                        });
                    }
                }
                return JOB;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return JOB;
            }
            finally
            {
                fun.ConClose();
            }
        }

        public JsonResult SaveTable(JOBSTATUS data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty, Orgid = string.Empty, Jobid = string.Empty,
                finallabeldate = string.Empty;
            Assemblyfunctions af = new Assemblyfunctions();
            try
            {
                string itemDcode = string.Empty;
                itemDcode = data.TRACTOR.Split('(', ')')[1].Trim().ToUpper();
                data.ITEM_CODE = itemDcode;
                string TSN = data.TRACTOR.Split('(', ')')[0].Trim().ToUpper();
                if (string.IsNullOrEmpty(data.T2_PLANT_CODE))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.T2_FAMILY_CODE))
                {
                    msg = "Please Select Family";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.TRACTOR))
                {
                    msg = "Please Select Tractor code";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.JOB))
                {
                    msg = "Please Select JOB";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                Orgid = fun.getOrgId(data.T2_PLANT_CODE, data.T2_FAMILY_CODE);
                if (string.IsNullOrEmpty(Orgid))
                {
                    msg = "OrgId not found for selected plant and family";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                TSN = TSN.Trim().ToUpper();
                query = string.Format(@"SELECT PLANT_CODE, FAMILY_CODE, ITEM_CODE,ITEM_DESCRIPTION,JOBID,TRANSMISSION,TRANSMISSION_DESCRIPTION,TRANSMISSION_SRLNO,REARAXEL,
                        REARAXEL_DESCRIPTION, REARAXEL_SRLNO, ENGINE, ENGINE_DESCRIPTION, ENGINE_SRLNO,FCODE_SRLNO, HYDRALUIC,HYDRALUIC_DESCRIPTION,
                        HYDRALUIC_SRLNO,REARTYRE,REARTYRE_DESCRIPTION,REARTYRE_SRLNO1,REARTYRE_SRLNO2,REARTYRE_MAKE,FRONTTYRE,
                        FRONTTYRE_DESCRIPTION, FRONTTYRE_SRLNO1, FRONTTYRE_SRLNO2, FRONTTYRE_MAKE, BATTERY, BATTERY_DESCRIPTION,
                        BATTERY_SRLNO, BATTERY_MAKE,FCODE_ID,TO_CHAR(ENTRYDATE,'DD-Mon-YYYY') ENTRYDATE, REMARKS1, REMARKS2, HYD_PUMP, HYD_PUMP_SRLNO, STEERING_MOTOR, STEERING_MOTOR_SRLNO,
                        STEERING_ASSEMBLY,STEERING_ASSEMBLY_SRLNO, STERING_CYLINDER, STERING_CYLINDER_SRLNO, RADIATOR,RADIATOR_SRLNO, CLUSSTER, CLUSSTER_SRLNO,ALTERNATOR,
                        ALTERNATOR_SRLNO, STARTER_MOTOR, STARTER_MOTOR_SRLNO,RTRIM1,RTTYRE1, RTRIM2, RTTYRE2, FTRIM1,FTTYRE1,FTRIM2,  FTTYRE2,
                        to_char (to_date(FINAL_LABEL_DATE,'YYYYMMDD'), 'DD-Mon-YY') FINAL_LABEL_DATE, BACKEND, BACKEND_SRLNO,SIM_SERIAL_NO,  IMEI_NO, MOBILE,
                        ROPS_SRNO, PDIOKDATE, PDIDONEBY,OIL, SWAPCAREBTN, FIPSRNO, REMARKS, CAREBUTTONOIL,TRANSFER_TO_EL, CREATION_DATE,
                        CREATED_BY, LAST_UPDATED_BY, LAST_UPDATE_LOGIN, LAST_UPDATE_DATE, PROCESS_FLAG, ERROR_MESSAGES,LABELPRINTED
                        FROM EKI_XXES_JOB_STATUS where FCODE_SRLNO='{0}' and plant_code='{1}'", TSN, data.T2_PLANT_CODE);
                //              query = string.Format(@"select * from EKI_XXES_JOB_STATUS where FCODE_SRLNO='{0}' and plant_code='{1}'", TSN,data.T2_PLANT_CODE);
                DataTable dataTable = fun.returnDataTable(query);

                if (dataTable.Rows.Count > 0)
                {
                    data.FCODE_SRLNO = TSN;
                    DataRow dataRow = dataTable.Rows[0];
                    if (af.InsertTractorfromExcel(dataRow, data.JOB, data))
                    {
                        msg = "Saved Successfully Into Table...";
                        mstType = "alert-success";
                        status = "success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        msg = "JOB ALREADY EXIST IN TABLE..!";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    msg = "SELECTED TRACTOR NOT FOUND...!";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
        }


        [HttpPost]
        public ActionResult ExportToExcel(string PLANT_CODE, string FAMILY_CODE)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string errorNo = string.Empty;
            string filename = "JobStatus";
            string ImportExcel = filename;
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("JobStatus");

            ws.Cell("A1").Value = "PLANT_CODE"; ws.Cell("B1").Value = "FAMILY_CODE"; ws.Cell("C1").Value = "ITEM_CODE"; ws.Cell("D1").Value = "ITEM_DESCRIPTION";
            ws.Cell("E1").Value = "JOBID"; ws.Cell("F1").Value = "TRANSMISSION"; ws.Cell("G1").Value = "TRANSMISSION_DESCRIPTION"; ws.Cell("H1").Value = "TRANSMISSION_SRLNO";
            ws.Cell("I1").Value = "REARAXEL"; ws.Cell("J1").Value = "REARAXEL_DESCRIPTION"; ws.Cell("K1").Value = "REARAXEL_SRLNO"; ws.Cell("L1").Value = "ENGINE";
            ws.Cell("M1").Value = "ENGINE_DESCRIPTION"; ws.Cell("N1").Value = "ENGINE_SRLNO"; ws.Cell("O1").Value = "FCODE_SRLNO"; ws.Cell("P1").Value = "HYDRALUIC";
            ws.Cell("Q1").Value = "HYDRALUIC_DESCRIPTION"; ws.Cell("R1").Value = "HYDRALUIC_SRLNO"; ws.Cell("S1").Value = "REARTYRE"; ws.Cell("T1").Value = "REARTYRE_DESCRIPTION";
            ws.Cell("U1").Value = "REARTYRE_SRLNO1"; ws.Cell("V1").Value = "REARTYRE_SRLNO2"; ws.Cell("W1").Value = "REARTYRE_MAKE"; ws.Cell("X1").Value = "FRONTTYRE";
            ws.Cell("Y1").Value = "FRONTTYRE_DESCRIPTION"; ws.Cell("Z1").Value = "FRONTTYRE_SRLNO1"; ws.Cell("AA1").Value = "FRONTTYRE_SRLNO2"; ws.Cell("AB1").Value = "FRONTTYRE_MAKE";
            ws.Cell("AC1").Value = "BATTERY"; ws.Cell("AD1").Value = "BATTERY_DESCRIPTION"; ws.Cell("AE1").Value = "BATTERY_SRLNO"; ws.Cell("AF1").Value = "BATTERY_MAKE";
            ws.Cell("AG1").Value = "FCODE_ID"; ws.Cell("AH1").Value = "REMARKS1"; ws.Cell("AI1").Value = "REMARKS2";
            ws.Cell("AJ1").Value = "HYD_PUMP"; ws.Cell("AK1").Value = "HYD_PUMP_SRLNO"; ws.Cell("AL1").Value = "STEERING_MOTOR"; ws.Cell("AM1").Value = "STEERING_MOTOR_SRLNO";
            ws.Cell("AN1").Value = "STEERING_ASSEMBLY"; ws.Cell("AO1").Value = "STEERING_ASSEMBLY_SRLNO"; ws.Cell("AP1").Value = "STERING_CYLINDER"; ws.Cell("AQ1").Value = "STERING_CYLINDER_SRLNO";
            ws.Cell("AR1").Value = "RADIATOR"; ws.Cell("AS1").Value = "RADIATOR_SRLNO"; ws.Cell("AT1").Value = "CLUSSTER"; ws.Cell("AU1").Value = "CLUSSTER_SRLNO";
            ws.Cell("AV1").Value = "ALTERNATOR"; ws.Cell("AW1").Value = "ALTERNATOR_SRLNO"; ws.Cell("AX1").Value = "STARTER_MOTOR"; ws.Cell("AY1").Value = "STARTER_MOTOR_SRLNO";
            ws.Cell("AZ1").Value = "FTTYRE2"; ws.Cell("BA1").Value = "RTRIM1"; ws.Cell("BB1").Value = "RTTYRE1"; ws.Cell("BC1").Value = "RTRIM2";
            ws.Cell("BD1").Value = "RTTYRE2"; ws.Cell("BE1").Value = "FTRIM1"; ws.Cell("BF1").Value = "FTTYRE1"; ws.Cell("BG1").Value = "FTRIM2";
            ws.Cell("BH1").Value = "FINAL_LABEL_DATE"; ws.Cell("BI1").Value = "BACKEND"; ws.Cell("BJ1").Value = "BACKEND_SRLNO"; ws.Cell("BK1").Value = "SIM_SERIAL_NO";
            ws.Cell("BL1").Value = "IMEI_NO"; ws.Cell("BM1").Value = "MOBILE"; ws.Cell("BN1").Value = "ROPS_SRNO"; ws.Cell("BO1").Value = "PDIOKDATE"; ws.Cell("BP1").Value = "PDIDONEBY";
            ws.Cell("BQ1").Value = "OIL"; ws.Cell("BR1").Value = "SWAPCAREBTN"; ws.Cell("BS1").Value = "FIPSRNO"; ws.Cell("BT1").Value = "REMARKS";
            ws.Cell("BU1").Value = "CAREBUTTONOIL"; ws.Cell("BV1").Value = "LABELPRINTED";

            ws.Range("A1:BV1").Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }
            wb.SaveAs(FilePath);
            msg = "Format downloaded ...";
            mstType = "alert-info";
            excelName = ImportExcel;
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(JOBSTATUS data)
        {
            if (!string.IsNullOrEmpty(data.TRACTOR))
            {
                ViewBag.DataSource = Gridtable(data);
            }
            return PartialView();
        }
        
        public DataTable Gridtable(JOBSTATUS data)
        {
            string TSN = data.TRACTOR.Split('(', ')')[0].Trim().ToUpper();
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"SELECT  ITEM_CODE,
                             ITEM_DESCRIPTION,
                             ENGINE,
                            ENGINE_SRLNO,
                             REARAXEL,
                             REARAXEL_SRLNO,
                             TRANSMISSION,
                             TRANSMISSION_SRLNO,
                             HYDRALUIC,
                             HYDRALUIC_SRLNO,
                             BATTERY,
                             BATTERY_SRLNO,
                             SIM_SERIAL_NO
                              FROM BARCODE.EKI_XXES_JOB_STATUS where FCODE_SRLNO='{0}'", TSN.Trim().ToUpper());

                dt = fun.returnDataTable(query);

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally
            {

            }
            return dt;

        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        [HttpPost]
        public ActionResult ImportExcelJobStatus(JOBSTATUS data, HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty; string JobId = string.Empty; string status = string.Empty;
            string lastJobId = "", errormsg = string.Empty; string excelName = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = "Please Enter Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = "Please Enter Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (inputFile == null || inputFile.ContentLength == 0)
                {
                    msg = "Please Choose Excel Sheet ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (inputFile.FileName.EndsWith(".xlx") || inputFile.FileName.EndsWith(".xlsx"))
                {
                    string path = Server.MapPath("~/TempExcelFile/" + inputFile.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    inputFile.SaveAs(path);
                    string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties='Excel 12.0 Xml;HDR=YES;'", path);
                    OleDbConnection connection = new OleDbConnection();
                    connection.ConnectionString = constr;
                    OleDbCommand command = new OleDbCommand("SELECT * FROM [" + "JobStatus" + "$]", connection);
                    OleDbDataAdapter da = new OleDbDataAdapter(command);
                    dt = new DataTable();
                    DataColumn column = new DataColumn("Autoid");
                    column.DataType = System.Type.GetType("System.Int32");
                    column.AutoIncrement = true;
                    column.AutoIncrementSeed = 1;
                    column.AutoIncrementStep = 1;
                    dt.Columns.Add(column);
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        msg = "NO ROWS FOUND IN EXCEL FILE !!";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    int count = 0;
                    int rowupdate = 0;
                    string cellvalue = string.Empty, colname = string.Empty,
                        errormessage = string.Empty, rowid = string.Empty, Orgid = string.Empty, Jobid = string.Empty;
                    bool error = false;
                    if (dt.Rows.Count > 0)
                    {
                        Orgid = fun.getOrgId(data.PLANT_CODE, data.FAMILY_CODE);
                        if (string.IsNullOrEmpty(Orgid))
                        {
                            msg = "OrgId not found for selected plant and family";
                            mstType = "alert-danger";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                        Assemblyfunctions af = new Assemblyfunctions();
                        DataTable dtStatus = new DataTable();
                        dtStatus = dt.Clone();
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            error = false;
                            Jobid = errormessage = string.Empty;

                            data.PLANT_CODE = Convert.ToString(dataRow["PLANT_CODE"]);
                            data.ITEM_CODE = Convert.ToString(dataRow["ITEM_CODE"]);
                            //data.FAMILY_CODE = Convert.ToString(dataRow["FAMILY_CODE"]);
                            //data.FCODE_SRLNO = Convert.ToString(dataRow["FCODE_SRLNO"]);
                            data.FAMILY_CODE = dataRow["FAMILY_CODE"].ToString();
                            data.FCODE_SRLNO = dataRow["FCODE_SRLNO"].ToString();
                            rowid = Convert.ToString(dataRow["Autoid"]);

                            if (string.IsNullOrEmpty(data.FCODE_SRLNO)) //tractor srlno shud not be blank
                            {
                                error = true;
                                errormessage = errormessage + " TRACTOR SRLNO NOT FOUND";
                                dtStatus.ImportRow(dataRow);
                                IEnumerable<DataRow> rows = dt.Rows.Cast<DataRow>().Where(r => r["Autoid"].ToString() == rowid);
                                rows.ToList().ForEach(r => r.SetField("Remarks", errormessage));
                                continue;
                            }
                            else if (string.IsNullOrEmpty(data.ITEM_CODE)) //tractor srlno should not be blank
                            {
                                error = true;
                                errormessage = errormessage + " FCODE NOT FOUND";
                                dtStatus.ImportRow(dataRow);
                                IEnumerable<DataRow> rows = dtStatus.Rows.Cast<DataRow>().Where(r => r["Autoid"].ToString() == rowid);
                                rows.ToList().ForEach(r => r.SetField("Remarks", errormessage));
                                continue;
                            }
                            foreach (DataColumn dataColumn in dt.Columns)
                            {
                                cellvalue = Convert.ToString(dataRow[dataColumn]);
                                colname = dataColumn.ColumnName;
                                if (!string.IsNullOrEmpty(cellvalue))
                                {
                                    if (colname.Contains("SRLNO") || colname.Contains("SRNO") ||
                                        colname.Contains("JOB"))
                                    {
                                        if (af.DuplicateCheck(cellvalue, colname))
                                        {
                                            errormessage = errormessage + "DUPLICATE SRLNO FOUND FOR " + colname;
                                            dtStatus.ImportRow(dataRow);
                                            error = true;
                                            IEnumerable<DataRow> rows = dtStatus.Rows.Cast<DataRow>().Where(r => r["Autoid"].ToString() == rowid);
                                            rows.ToList().ForEach(r => r.SetField("Remarks", errormessage));
                                            break;
                                        }

                                    }
                                }
                            }
                            if (!error) //one row complte here
                            {
                                //Insert in table
                                Jobid = af.GetfreeJobFromOracle(data.ITEM_CODE, Orgid);
                                if (string.IsNullOrEmpty(Jobid)) //tractor srlno shud not be blank
                                {
                                    error = true;
                                    errormessage = errormessage + " RELEASED JOBID NOT FOUND";
                                    dtStatus.ImportRow(dataRow);
                                    IEnumerable<DataRow> rows = dt.Rows.Cast<DataRow>().Where(r => r["Autoid"].ToString() == rowid);
                                    rows.ToList().ForEach(r => r.SetField("Remarks", errormessage));
                                    continue;
                                }
                                if (af.InsertTractorfromExcel(dataRow, Jobid, data))
                                    rowupdate++;
                            }
                            //if (count > 0)
                            //{
                            //    msg = "Total " + Convert.ToString(count) + " Items Saved Successfully. ";
                            //    mstType = "alert-success";
                            //    var resul = new { Msg = msg, ID = mstType };

                            //    return Json(resul, JsonRequestBehavior.AllowGet);
                            //}
                        }
                   
                        if (dtStatus.Rows.Count > 0)
                        {
                            dtStatus.TableName = "Err_JobStatus";
                            string filename = "Err_JobStatus";
                            data.ImportExcel = filename;
                            var wb = new XLWorkbook();
                            var ws = wb.Worksheets.Add(dtStatus);
                            ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                            ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                            ws.Range("A1:BV1").Style.Font.Bold = true;
                            ws.Columns().AdjustToContents();
                            string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                            if (System.IO.File.Exists(FilePath))
                            {
                                System.IO.File.Delete(FilePath);
                            }
                            wb.SaveAs(FilePath);
                            msg = "Error File Exported Successfully ...";
                            mstType = Validation.str;
                            excelName = data.ImportExcel;
                            var resul = new { Msg = msg, ID = mstType, ExcelName = excelName };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                            
                            //ViewBag.DataSource = dtStatus;
                            //return PartialView("DuplicateErrorGrid");
                            // return PartialView("~/Views/ImportExport/DuplicateErrorGrid.cshtml");
                            //show these error rows
                        }

                    }

                    //if (count > 0)
                    //{
                    //    msg = "Total " + Convert.ToString(count) + " Items Saved Successfully. ";
                    //    mstType = "alert-success";
                    //    var resul = new { Msg = msg, ID = mstType, result = dt };

                    //    return Json(resul, JsonRequestBehavior.AllowGet);
                    //}


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
            //mstType = "alert-success";
            //var excelName = data.ImportExcel; 
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ValidateFileExtention(HttpPostedFileBase inputFile)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                if (inputFile == null)
                {
                    msg = "File Must be Excel file ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
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

        public PartialViewResult DuplicateErrorGrid(string PLANT_CODE,string FAMILY_CODE)
        {
            //query = string.Format(@"Select PLANT_CODE,FAMILY_CODE ,ITEM_CODE,FCODE_SRLNO,ENGINE_SRLNO,REARAXEL_SRLNO,
            //       TRANSMISSION_SRLNO,BACKEND_SRLNO,BATTERY_SRLNO,FTTYRE1,FTTYRE2,RTTYRE1,RTTYRE2 from XXES_JOB_STATUS where  PLANT_CODE ='{0}' and FAMILY_CODE = '{1}'", PLANT_CODE.ToUpper().Trim(), FAMILY_CODE.ToUpper().Trim());

            return PartialView();
        }
    }
}