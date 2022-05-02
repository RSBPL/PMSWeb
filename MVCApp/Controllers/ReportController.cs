using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace MVCApp.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        ReportType RT = new ReportType();
        Function fun = new Function();

        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.DefaultDate = DateTime.Now.AddDays(0);
                return View();
            }
        }

        public PartialViewResult Grid(ReportModel data)
        {
            try
            {
                DataTable dtMain = new DataTable();
                ReportModel reportModel = new ReportModel();
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                if (Convert.ToString(data.ReportType) == "DAILY_CRANE")
                {
                    ViewBag.heading = "TOTAL CRANE MANUFUCTURED";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdDaily_Crane");
                }
                else if (Convert.ToString(data.ReportType) == "PART_AUDIT_CRANE")
                {
                    ViewBag.heading = "AUDIT OF PARTS";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPART_AUDIT_CRANE");
                }
                else if (Convert.ToString(data.ReportType) == "PRDBACKEND")
                {
                    ViewBag.heading = "BACKEND PRODUCTION";
                    if (data.Plant == "T05")
                    {
                        if (data.QCPRINT == "PRINTEDON")
                            data.QCPRINT = "PRINTDATE";
                        else if (data.QCPRINT == "SCANDATE")
                            data.QCPRINT = "LOADER_SCAN";
                        else
                            data.QCPRINT = "QCDATE";                  

                        if (data.LessFields)
                            data.ShowLess = "1";
                        else
                            data.ShowLess = "0";
                    }
                    else
                    {
                        string filterby = string.Empty;
                        if (data.QCPRINT == "PRINTEDON")
                            data.QCPRINT = "CREATEDDATE";
                        else
                            data.QCPRINT = "CREATEDDATE";
                        if (data.LessFields)
                            data.ShowLess = "1";
                        else
                            data.ShowLess = "0";
                    }

                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReports(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    if (data.Plant == "T05")
                    {
                        return PartialView("GrdPRDBACKEND_T05");
                    }
                    else
                    {
                        return PartialView("GrdPRDBACKEND_T04");
                    }
                }
                
                else if (Convert.ToString(data.ReportType) == "STORE_REPORT")
                {

                    ViewBag.heading = "STORE REPORT";
                    if (data.STFilterBy == "PRINTDATE")
                        data.STFilterBy = "PRINTDATE";
                    else
                        data.STFilterBy = "SCANDATE";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.STFilterBy;

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GRDStoreReport");

                }
                
                else if (Convert.ToString(data.ReportType) == "PRDENGINES")
                {
                    ViewBag.heading = "ENGINE PRODUCTION";
                    string filterby = string.Empty;
                    if (data.QCPRINT == "PRINTEDON")
                        data.QCPRINT = "PRINTDATE";
                    else if (data.QCPRINT == "SCANDATE")
                        data.QCPRINT = "LOADER_SCAN";
                    else
                        data.QCPRINT = "QCDATE";

                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReports(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPRDENGINES");
                }
                
                else if (Convert.ToString(data.ReportType) == "PRDTRANSMISSION")
                {
                    ViewBag.heading = "TRANSMISSION PRODUCTION";
                    string filterby = string.Empty;
                    if (data.QCPRINT == "PRINTEDON")
                        data.QCPRINT = "PRINTDATE";
                    else if (data.QCPRINT == "SCANDATE")
                        data.QCPRINT = "LOADER_SCAN";
                    else
                        data.QCPRINT = "QCDATE";

                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReports(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPRDTRANSMISSION");
                }
                else if (Convert.ToString(data.ReportType) == "PRDHYDRAULIC")
                {
                    ViewBag.heading = "HYDRAULIC PRODUCTION";
                    string filterby = string.Empty;
                    if (data.QCPRINT == "PRINTEDON")
                        data.QCPRINT = "PRINTDATE";
                    else
                        data.QCPRINT = "QCDATE";

                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReports(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPRDHYDRAULIC");
                }
                
                else if (Convert.ToString(data.ReportType) == "PRDREARAXEL")
                {
                    ViewBag.heading = "REAR AXEL PRODUCTION";
                    if (data.QCPRINT == "PRINTEDON")
                        data.QCPRINT = "PRINTDATE";
                    else if (data.QCPRINT == "SCANDATE")
                        data.QCPRINT = "LOADER_SCAN";
                    else
                        data.QCPRINT = "QCDATE";

                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReports(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPRDREARAXEL");
                }
                

                else if (Convert.ToString(data.ReportType) == "TOT")
                {
                    ViewBag.heading = "TOTAL TRACTORS MANUFACTURED";

                    string filterby = string.Empty;
                    if (data.FilterBy == "BUCKLEUP DATE")
                        data.FilterBy = "ENTRYDATE";
                    else if (data.FilterBy == "ROLLOUT DATE")
                        data.FilterBy = "FINAL_LABEL_DATE";
                    else if (data.FilterBy == "PDI DATE")
                        data.FilterBy = "PDIOKDATE";
                    else if (data.FilterBy == "HOOKUP DATE")
                    {
                        data.FilterBy = "HOOKUP_DATE";
                    }
                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReportsTOTROLL(data);
                    if (dtMain.Rows.Count > 0)
                    {
                        string avgHours = string.Empty;
                        dtMain.Columns.Add("PDI TACK TIME", typeof(string));
                        foreach (DataRow row in dtMain.Rows)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(row["ROLL OUT DATE"])) && !string.IsNullOrEmpty(Convert.ToString(row["CAREBUTTONOIL"])))
                            {
                                TimeSpan span = Convert.ToDateTime(row["ROLL OUT DATE"]) - Convert.ToDateTime(row["CAREBUTTONOIL"]);
                                span = new TimeSpan(Math.Abs(span.Ticks));
                                avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
                                row["PDI TACK TIME"] = avgHours;
                            }
                        }
                        ViewBag.Total = dtMain.Rows.Count;
                        ViewBag.DataSource = dtMain;
                        if (data.FilterBy == "HOOKUP_DATE" && !data.chkShowLess && Convert.ToString(data.Plant) == "T04")
                            return PartialView("GrdTOTHookUpDate");
                        else if (data.FilterBy != "HOOKUP_DATE" && !data.chkShowLess && Convert.ToString(data.Plant) == "T04")
                            return PartialView("GrdTOTShowFullT04");
                        else if (data.FilterBy == "HOOKUP_DATE" && !data.chkShowLess && Convert.ToString(data.Plant) == "T05")
                            return PartialView("GrdTOTHookUpDate");
                        else if (data.FilterBy != "HOOKUP_DATE" && !data.chkShowLess && Convert.ToString(data.Plant) == "T05")
                            return PartialView("GrdTOTShowFull");
                        else if (data.FilterBy == "HOOKUP_DATE" && data.chkShowLess && Convert.ToString(data.Plant) == "T04")
                            return PartialView("GrdTOTHookUpDate");
                        else if (data.FilterBy != "HOOKUP_DATE" && data.chkShowLess && Convert.ToString(data.Plant) == "T04")
                            return PartialView("GrdTOTShowLessT04");
                        else if (data.FilterBy == "HOOKUP_DATE" && data.chkShowLess && Convert.ToString(data.Plant) == "T05")
                            return PartialView("GrdTOTHookUpDate");
                        else if (data.FilterBy != "HOOKUP_DATE" && data.chkShowLess && Convert.ToString(data.Plant) == "T05")
                            return PartialView("GrdTOTShowLess");

                    }
                    else
                    {
                        ViewBag.msg = "Record Not Found.....";
                        return PartialView("RecordNotFoundGrid");
                    }
                    return PartialView();
                }
                else if (Convert.ToString(data.ReportType) == "ERROR")
                {
                    ViewBag.heading = "ERROR LOG";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.DataSource = dtMain;
                    ViewBag.Total = dtMain.Rows.Count;
                    return PartialView("GrdERROR");
                }
                else if (Convert.ToString(data.ReportType) == "HYD")
                {
                    
                    ViewBag.heading = "DETAIL OF HYDRAULIC PRINTED";
                    query = "select DCODE, (select description  from " + schema + ".mtl_system_items where organization_id in ( " + orgid + ") and substr(segment1, 1, 1) in ('D','S') and segment1=DCODE and rownum=1 ) DESCRIPTION ,SRNO SERIAL_NUMBER, " +
                        " TO_CHAR(PRINTDATE,'dd-Mon-yyyy HH24:MI:SS') as PRINTDATE, (select fcode_srlno from xxes_job_status where hydraluic_srlno=SRNO and plant_code=s.plant_code and family_code=s.family_code) TRACTOR_SRLNO from XXES_PRINT_SERIALS s where OFFLINE_KEYCODE='HYD' and family_code='" + Convert.ToString(data.Family).Trim() + "' and plant_code='" + Convert.ToString(data.Plant) + "' and to_char(PRINTDATE,'dd-Mon-yyyy') >=to_date('" + data.FromDate + "','dd-Mon-yyyy') and  to_char(PRINTDATE,'dd-Mon-yyyy')<=to_date('" + data.ToDate + "','dd-Mon-yyyy') order by serial_number";
                    dtMain = fun.returnDataTable(query);
                    
                    ViewBag.DataSource = dtMain;
                    ViewBag.Total = dtMain.Rows.Count;
                    return PartialView("GrdHYD");
                }
                else if (Convert.ToString(data.ReportType) == "RT")
                {
                    ViewBag.heading = "DETAIL OF REAR TYRE PRINTED";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.DataSource = dtMain;
                    ViewBag.Total = dtMain.Rows.Count;
                    return PartialView("GrdRT");
                }
                else if (Convert.ToString(data.ReportType) == "FT")
                {
                    ViewBag.heading = "DETAIL OF FRONT TYRE PRINTED";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.DataSource = dtMain;
                    ViewBag.Total = dtMain.Rows.Count;
                    return PartialView("GrdFT");
                }
                else if (Convert.ToString(data.ReportType) == "JOBDATA")
                {
                    ViewBag.heading = "DETAIL OF PLAN";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdJOBDATA");
                }
                else if (Convert.ToString(data.ReportType) == "PLAN_AUDIT")
                {
                    ViewBag.heading = "AUDIT OF PLAN";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPLAN_AUDIT");
                }
                else if (Convert.ToString(data.ReportType) == "CHECK_JOB")
                {
                    ViewBag.heading = "JOB EXISTENCE";
                    if (string.IsNullOrEmpty(data.Job))
                    {
                        ViewBag.msg = "Invalid Job.....";
                        return PartialView("RecordNotFoundGrid");
                    }

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Job;
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdCHECK_JOB");
                }
                else if (Convert.ToString(data.ReportType) == "JOB")
                {
                    ViewBag.heading = "DETAIL OF JOB";
                    if (String.IsNullOrEmpty(data.gleJobs))
                    {
                        ViewBag.msg = "Please select the job to continue...";
                        return PartialView("RecordNotFoundGrid");
                    }

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    //char[] spearator = { '#' };
                    //String[] SplitgleJobs = data.gleJobs.Split(spearator, StringSplitOptions.None);
                    //data.gleJobs = SplitgleJobs[0];
                    //data.gleJobsDesc = SplitgleJobs[1];

                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(data.gleJobs.Trim());
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdJOB");
                }
                else if (Convert.ToString(data.ReportType) == "RELJOBS")
                {
                    ViewBag.heading = "STATUS OF RELEASED JOBS FROM ORACLE";
                    orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = orgid;

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdRELJOBS");
                }
                else if (Convert.ToString(data.ReportType) == "EN_AUDIT")
                {
                    ViewBag.heading = "AUDIT OF ENGINES";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdEN_AUDIT");
                }
                else if (Convert.ToString(data.ReportType) == "TOT_ENG")
                {
                    ViewBag.heading = "TOTAL ENGINES MANUFUCTURED";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdTOT_ENG");
                }
                else if (Convert.ToString(data.ReportType) == "PART_AUDIT")
                {
                    ViewBag.heading = "AUDIT OF PARTS";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPART_AUDIT");
                }
                else if (Convert.ToString(data.ReportType) == "DAILY_VEHICLE")
                {
                    ViewBag.heading = "LIST OF MRN (CHECKIN-OUT)";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdDAILY_VEHICLE");
                }
                else if (Convert.ToString(data.ReportType) == "MRNITEM")
                {
                    ViewBag.heading = "ITEM WISE MRN DETAILS";
                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdMRNITEMS");
                }
                else if (Convert.ToString(data.ReportType) == "AVG")
                {
                    ViewBag.heading = "AVERAGE TIME IN A DAY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);

                    DataTable dtAvg = new DataTable();
                    if (dtMain.Rows.Count > 0)
                    {
                        string avgHours = string.Empty;

                        dtAvg = dtMain.Clone();
                        dtAvg.Columns.Add("AVG_HOURS");
                        foreach (DataRow row in dtMain.Rows)
                        {
                            avgHours = getAvg(Convert.ToString(row["MRN_DATE"]), Convert.ToInt32(row["TOTAL_MRN"]));
                            dtAvg.Rows.Add(row["MRN_DATE"], row["TOTAL_MRN"], avgHours);
                        }
                    }
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtAvg;
                    return PartialView("GrdAVG");
                }
                else if (Convert.ToString(data.ReportType) == "AVG_TIME")
                {
                    string heading = "TOTAL HOURS VEHICLE HAS TAKEN INSIDE PREMISES WITH AVERAGE TIME";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);

                    double totHours = 0, totMinutes = 0, final = 0;
                    string hours = string.Empty;
                    try
                    {
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            //string val = Convert.ToString(gridView1.GetRowCellValue(i, "TOTAL_HOURS"));
                            string val = Convert.ToString(dtMain.Rows[i]["TOTAL_HOURS"]);
                            if (!string.IsNullOrEmpty(val))
                            {
                                if(!string.IsNullOrEmpty(val.Split(':')[0]))
                                {
                                    totHours += Convert.ToDouble(val.Split(':')[0].Trim());
                                }
                                if (!string.IsNullOrEmpty(val.Split(':')[1]))
                                {
                                    totMinutes += Convert.ToDouble(val.Split(':')[1].Trim());
                                }

                                
                            }
                        }
                        final = totHours * 60;
                        final += totMinutes;
                        final = final / dtMain.Rows.Count;
                        TimeSpan spWorkMin = TimeSpan.FromMinutes(final);
                        hours = string.Format("{0:00}:{1:00}", (int)spWorkMin.TotalHours, spWorkMin.Minutes);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.msg = ex.Message;
                        return PartialView("RecordNotFoundGrid");
                    }
                    ViewBag.heading = heading + " " + hours;
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdAVG_TIME");
                }
                else if (Convert.ToString(data.ReportType) == "MAX_MIN")
                {
                    ViewBag.heading = "MAXIMUM AND MINIMUM VEHICLE STAYS INSIDE";
                    

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdMAX_MIN");
                }
                else if (Convert.ToString(data.ReportType) == "VIP")
                {
                    ViewBag.heading = "VEHICLE INSIDE PREMISES";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdVIP");
                }
                else if (Convert.ToString(data.ReportType) == "NOM")
                {
                    ViewBag.heading = "NUMBER OF MRN OPERATOR WISE";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdNOM");
                }
                else if (Convert.ToString(data.ReportType) == "PRD")
                {
                    if (data.FromDate != data.ToDate)
                    {
                        ViewBag.msg = "Invalid Date selection ! Only one day can be selected....";
                        return PartialView("RecordNotFoundGrid");
                    }

                    ViewBag.heading = "PRODUCTION REPORT";

                    string startTime = string.Empty, endtime = string.Empty;
                    startTime = data.ShiftText.Split('-')[0].Split('(')[1].Trim();
                    endtime = data.ShiftText.Split('-')[1].Split(')')[0].Trim();
                    DateTime dtFrom = Convert.ToDateTime(data.FromDate);
                    DateTime dtTo = Convert.ToDateTime(data.ToDate);
                    DateTime dtPlan = Convert.ToDateTime(data.FromDate);
                    if (Convert.ToString(data.Shift) == "B")
                        dtTo = dtTo.Date.AddDays(1);
                    else if (Convert.ToString(data.Shift) == "C")
                    {
                        dtPlan = dtPlan.Date.AddDays(-1);
                        dtFrom = dtFrom.Date.AddDays(1);
                        dtTo = dtTo.Date.AddDays(1);
                    }

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = dtFrom.Date.ToString("dd-MMM-yyyy");
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = dtTo.Date.ToString("dd-MMM-yyyy");
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = dtPlan.Date.ToString("dd-MMM-yyyy");
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Shift;
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = startTime.Trim();
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = endtime.Trim();

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    //dtMain.Columns["SHIFT"].ColumnName = "SHIFT-" + Convert.ToString(data.Shift).Trim();
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPRD");
                }
               
                ////CREATED BY SARTHAK ON 24-MAY-2021
                else if (Convert.ToString(data.ReportType) == "BULK_STORAGE_ITEMS")
                {
                    
                    ViewBag.heading = "BULK STORAGE ITEMS";

                    
                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdBulkStorageItem");
                }
                else if (Convert.ToString(data.ReportType) == "SUPER_MARKET_ITEMS")
                {

                    ViewBag.heading = "SUPER MARKET ITEMS";


                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdSuperMktItem");
                }
                else if (Convert.ToString(data.ReportType) == "SUPERMKT_PICKED_ITEMS")
                {
                    ViewBag.heading = "SUPER MARKET PICKED ITEMS";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdSuprMktPicItems");
                }
                else if (Convert.ToString(data.ReportType) == "BULK_STORAGE_CAP")
                {
                    ViewBag.heading = "BULK STORAGE CAPACITY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdBlkStrgCap");
                }
                else if (Convert.ToString(data.ReportType) == "SUPERMKT_MKT_CAP")
                {
                    ViewBag.heading = "SUPER MARKET CAPACITY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdSupMktCap");
                }
                else if (Convert.ToString(data.ReportType) == "SHORT_RECIPT_VERIFICATION")
                {
                    ViewBag.heading = "SHORT RECIPT AT VERIFICATION";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdShtRecVrf");
                }
                else if (Convert.ToString(data.ReportType) == "SHORT_RECIPT_QUALITY")
                {
                    ViewBag.heading = "SHORT RECIPT AT QUALITY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdShtRecQc");
                }
                else if (Convert.ToString(data.ReportType) == "BULK_STORAGE_FAULTY")
                {
                    ViewBag.heading = "BULK STORAGE FAULTY ITEMS";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdBlkFaulty");
                }
                else if (Convert.ToString(data.ReportType) == "SUPER_MARKET_FAULTY")
                {
                    ViewBag.heading = "SUPER MARKET FAULTY ITEMS";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdSmFaulty");
                }
                else if (Convert.ToString(data.ReportType) == "HOLD_ITEM_QUALITY")
                {
                    ViewBag.heading = "HOLD ITEMS QUALITY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdQCHold");
                }
                else if (Convert.ToString(data.ReportType) == "REPRINT_LABEL")
                {
                    data.OrgID = orgid;
                    ViewBag.heading = "REPRINT LABEL AT STORE";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.OrgID;

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdReprintLabel");
                }
                else if (Convert.ToString(data.ReportType) == "ROLL")
                {
                    ViewBag.heading = "TOTAL TRACTORS ROLLOUT";
                    string filterby = string.Empty;
                    if (data.FilterBy == "BUCKLEUP DATE")
                        data.FilterBy = "ENTRYDATE";
                    else if (data.FilterBy == "ROLLOUT DATE")
                        data.FilterBy = "FINAL_LABEL_DATE";
                    else if (data.FilterBy == "PDI DATE")
                        data.FilterBy = "PDIOKDATE";
                    else
                        data.FilterBy = "''";

                    if (data.LessFields)
                        data.ShowLess = "1";
                    else
                        data.ShowLess = "0";
                    data.OrgID = orgid;
                    dtMain = reportModel.GenerateReportsTOTROLL(data);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    if (Convert.ToString(data.Plant) == "T02")
                        return PartialView("GrdT02RollOut");
                    else
                        return PartialView("GrdRollOut");
                }
                
                else if (Convert.ToString(data.ReportType) == "ITEM_EXCCED_MAXINVENTORY")
                {
                    ViewBag.heading = "ITEMS EXCEEDING MAX. INVENTORY";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdItemExceedInventory");
                }
                else if (Convert.ToString(data.ReportType) == "ITEMS_OTHER_SNP")
                {
                    ViewBag.heading = "ITEMS OTHER THAN SNP";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdItemOtherSNP");
                }

                else if (Convert.ToString(data.ReportType) == "MATERIAL_SHORT_BULK")
                {
                   //string SHORT_BULK = data.SHORT_BULK;
                    if(string.IsNullOrEmpty(data.SHORT_BULK))
                    {
                        ViewBag.msg = "NUMBER OF TRACTORS NOT FOUND.....";
                        return PartialView("RecordNotFoundGrid");
                    }
                    
                    ViewBag.heading = "MATERIAL SHORT  FOR 100 TRACTORS AT BULK LOCATION";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.SHORT_BULK; 

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdMaterialShortBulk");
                }
                else if (Convert.ToString(data.ReportType) == "MATERIAL_SHORT_SUPERMKT")
                {
                    //string SHORT_SUPERMKT = Convert.ToString(ConfigurationManager.AppSettings["SHORT_SUPERMKT"]);
                    if (string.IsNullOrEmpty(data.SHORT_BULK))
                    {
                        ViewBag.msg = "NUMBER OF TRACTORS NOT FOUND....."; 
                        return PartialView("RecordNotFoundGrid");
                    }
                    ViewBag.heading = "MATERIAL SHORT  FOR 20 TRACTORS AT SUPER MARKET";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.SHORT_BULK.Trim();

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdMaterialShortSuperMkt");
                }
                else if (Convert.ToString(data.ReportType) == "STOCK_STATUS")
                {
                    orgid = fun.getOrgId(Convert.ToString(data.Plant), Convert.ToString(data.Family));

                    ViewBag.heading = "STOCK STATUS REPORT";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = orgid;

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdStockStatus");
                }
                else if (Convert.ToString(data.ReportType) == "MATERIAL_BULKTEMP")
                {
                    

                    ViewBag.heading = "MATERIAL AT TEMPORARY LOCATION IN BULK STORE";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdBulkTempLoc");
                }

                else if (Convert.ToString(data.ReportType) == "ROLL_AUDIT")
                {


                    ViewBag.heading = "ROLL OUT STICKER LOG";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdROLL_AUDIT");
                }
                else if (Convert.ToString(data.ReportType) == "PDI_AUDIT")
                {


                    ViewBag.heading = "PDI STICKER LOG";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdPDI_AUDIT");
                }
                else if (Convert.ToString(data.ReportType) == "KIT_SCANNING")
                {


                    ViewBag.heading = "KIT SCANNING REPORT";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdKIT_SCANNING");
                }

                //Created BY NAUSHAD
                else if (Convert.ToString(data.ReportType) == "SHORT METERIAL DAILY")
                {
                    //string SHORT_BULK = data.SHORT_BULK;
                    if (string.IsNullOrEmpty(data.SHORT_BULK))
                    {
                        ViewBag.msg = "NUMBER OF Repeats NOT FOUND.....";
                        return PartialView("RecordNotFoundGrid");
                    }

                    ViewBag.heading = "MATERIAL SHORT  Daily";

                    DA = new OracleDataAdapter("USP_REPORTMASTER", fun.Connection());
                    DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA.SelectCommand.Parameters.Add("pREPORT_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ReportType;
                    DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant;
                    DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family;
                    DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate;
                    DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate;
                    DA.SelectCommand.Parameters.Add("pSCHEMA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pCHECK_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pGLE_JOBS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.SHORT_BULK;

                    DA.SelectCommand.Parameters.Add("pPlanDate", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pShiftValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pStartTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pEndTime", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("pChkShowLess", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";
                    DA.SelectCommand.Parameters.Add("pFilterBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "";

                    DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    DA.Fill(dtMain);
                    ViewBag.Total = dtMain.Rows.Count;
                    ViewBag.Date = "("+data.FromDate + " Between " + data.ToDate+")";
                    ViewBag.DataSource = dtMain;
                    return PartialView("GrdMaterialShortDaily");
                }

                //CREATED BY RAJ ON 08-DEC-2021
                else if (Convert.ToString(data.ReportType) == "DAILY_PART_SCANNING_EFFICIENCY")
               {
                   
                   ViewBag.heading = "DAILY PART SCANNING EFFICIENCY";
                   DataTable dt = fun.GridDailyaprtScanningEfficiency(data);
                   ViewBag.Total = dt.Rows.Count;
                   ViewBag.DataSource = dt;
                   return PartialView("PartScanningEffGrid");
               }
                else
                {
                    
                    ViewBag.msg = "Record Not Found.....";
                    return PartialView("RecordNotFoundGrid");
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                ViewBag.msg = ex.Message;
                return PartialView("RecordNotFoundGrid");
            }
            finally
            {

            }
        }


        public string getProductionReport(ReportModel data)
        {
            try
            {
                if (data.FromDate != data.ToDate)
                {
                    return "Invalid Date selection ! Only one day can be selected";
                }

                string startTime = string.Empty, endtime = string.Empty;
                startTime = data.ShiftText.Split('-')[0].Split('(')[1].Trim();
                endtime = data.ShiftText.Split('-')[1].Split(')')[0].Trim();
                DateTime dtFrom = Convert.ToDateTime(data.FromDate);
                DateTime dtTo = Convert.ToDateTime(data.ToDate);
                DateTime dtPlan = Convert.ToDateTime(data.FromDate);
                if (Convert.ToString(data.Shift) == "B")
                    dtTo = dtTo.Date.AddDays(1);
                else if (Convert.ToString(data.Shift) == "C")
                {
                    dtPlan = dtPlan.Date.AddDays(-1);
                    dtFrom = dtFrom.Date.AddDays(1);
                    dtTo = dtTo.Date.AddDays(1);
                }
                if (Convert.ToString(data.Plant) == "T04")
                {
                    query = "select 1 SERIAL_NO,'PLAN' DESCRIPTION, NVL(sum(qty),0) SHIFT  from xxes_daily_plan_tran where plan_id in (select PLAN_ID from xxes_daily_plan_master where to_char(plan_date,'dd-Mon-yyyy')=to_date('" + dtPlan.Date.ToString("dd-MMM-yyyy") + "') and shiftcode='" + Convert.ToString(data.ShiftValue) + "' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "')";
                    query += " UNION ";
                    query += " select 2 SERIAL_NO,'BUCKLEUP' DESCRIPTION, count(*) SHIFT  from XXES_job_status where entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and transmission_srlno is not null and rearaxel_srlno is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 3 SERIAL_NO,'ENGINE' DESCRIPTION, count(*) SHIFT   from XXES_job_status where   entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and engine_srlno is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 4 SERIAL_NO,'HYDRAULIC LIFT PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='HYD' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 5 SERIAL_NO,'HYDRAULIC SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and HYDRALUIC_srlno is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 6 SERIAL_NO,'BP HOOK SCANNED' DESCRIPTION, count(*) SHIFT   from xxes_controllers_data where   entry_date>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entry_date<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and stage='BP' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 7 SERIAL_NO,'AP HOOK SCANNED' DESCRIPTION, count(*) SHIFT   from xxes_controllers_data where   entry_date>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entry_date<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and stage='AP' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 8 SERIAL_NO,'REAR TYRE ASSEMBLY PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='RT' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 9 SERIAL_NO,'FRONT TYRE ASSEMBLY PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='FT' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 10 SERIAL_NO,'REAR TYRE SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and REARTYRE_srlno1 is not null and REARTYRE_srlno2 is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 11 SERIAL_NO,'FRONT TYRE SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and FRONTTYRE_srlno1 is not null and FRONTTYRE_srlno2 is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                }
                else
                {
                    query = "select 1 SERIAL_NO,'PLAN' DESCRIPTION, NVL(sum(qty),0) SHIFT  from xxes_daily_plan_tran where plan_id in (select PLAN_ID from xxes_daily_plan_master where to_char(plan_date,'dd-Mon-yyyy')=to_date('" + dtPlan.Date.ToString("dd-MMM-yyyy") + "') and shiftcode='" + Convert.ToString(data.ShiftValue) + "' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "')";
                    query += " UNION ";
                    query += " select 2 SERIAL_NO,'BUCKLEUP' DESCRIPTION, count(*) SHIFT  from XXES_job_status where entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and engine_srlno is not null  and backend_srlno is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 3 SERIAL_NO,'HYDRAULIC LIFT PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='HYD' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 4 SERIAL_NO,'HYDRAULIC SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and HYDRALUIC_srlno is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 5 SERIAL_NO,'BP HOOK SCANNED' DESCRIPTION, count(*) SHIFT   from xxes_controllers_data where   entry_date>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entry_date<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and stage='BP' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 6 SERIAL_NO,'AP HOOK SCANNED' DESCRIPTION, count(*) SHIFT   from xxes_controllers_data where   entry_date>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entry_date<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and stage='AP' and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 7 SERIAL_NO,'REAR TYRE ASSEMBLY PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='RT' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 8 SERIAL_NO,'FRONT TYRE ASSEMBLY PRINTING' DESCRIPTION,count(*) SHIFT from XXES_PRINT_SERIALS where offline_keycode='FT' and printdate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  printdate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS')  and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 9 SERIAL_NO,'REAR TYRE SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and REARTYRE_srlno1 is not null and REARTYRE_srlno2 is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                    query += " UNION ";
                    query += " select 10 SERIAL_NO,'FRONT TYRE SCANNED' DESCRIPTION, count(*) SHIFT   from XXES_job_status where  entrydate>=to_date('" + dtFrom.Date.ToString("dd-MMM-yyyy") + " " + startTime.Trim() + "','dd-Mon-yyyy HH24:MI:SS') and  entrydate<=to_date('" + dtTo.Date.ToString("dd-MMM-yyyy") + " " + endtime.Trim() + "' ,'dd-Mon-yyyy HH24:MI:SS') and FRONTTYRE_srlno1 is not null and FRONTTYRE_srlno2 is not null and Plant_code='" + Convert.ToString(data.Plant).Trim() + "' and Family_code='" + Convert.ToString(data.Family).Trim() + "'";
                }
                return query;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string getAvg(string mrndate, int count)
        {
            double totHours = 0, totMinutes = 0, final = 0;
            string hours = string.Empty;
            try
            {
                query = string.Format(@"select 
                MRN_NO, MRN_DATE, VENDOR_CODE, VENDOR_NAME, INVOICE_NO, VEHICLE_NO, PRINTED_ON, PRINTED_BY, TIMEIN, PERSON, REMARKS_IN, IN_BY,
                TIMEOUT, REMARKS_OUT, OUT_BY, (case when TOTAL_HOURS = '0000:' OR TOTAL_HOURS IS NULL then '00:00' else TOTAL_HOURS end) TOTAL_HOURS
                from
                (select MRN_NO, TRANSACTION_DATE MRN_DATE, VENDOR_CODE, VENDOR_NAME, INVOICE_NO, VEHICLE_NO, PRINTED_ON, CREATEDBY PRINTED_BY,
                to_char(TIMEIN, 'dd-Mon-yyyy HH24:MI:SS') TIMEIN, PERSON, REMARKS_IN, IN_BY, to_char(TIMEOUT, 'dd-Mon-yyyy HH24:MI:SS') TIMEOUT,
                REMARKS_OUT, OUT_BY,
                LPAD
                (substr(numtodsinterval(timeout - timein, 'day'), 2, 9) * 24 + substr(numtodsinterval(timeout - timein, 'day'), 12, 2) || ':' ||
                substr(numtodsinterval(timeout - timein, 'day'), 15, 2), 5, 0) TOTAL_HOURS
                from ITEM_RECEIPT_DETIALS where 
                to_char(TRANSACTION_DATE, 'dd-Mon-yyyy') = '{0}' order by TRANSACTION_DATE, ITEM_RECEIPT_DETIALS.VEHICLE_NO
                )b", mrndate);
                DataTable dataTable = new DataTable();
                dataTable = fun.returnDataTable(query);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        string val = Convert.ToString(dataRow["TOTAL_HOURS"]);
                        if (!string.IsNullOrEmpty(val) && val != "00:00")
                        {
                            totHours += Convert.ToDouble(val.Split(':')[0].Trim());
                            totMinutes += Convert.ToDouble(val.Split(':')[1].Trim());
                        }
                    }
                    final = totHours * 60;
                    final += totMinutes;
                    final = final / count;
                    TimeSpan spWorkMin = TimeSpan.FromMinutes(final);
                    hours = string.Format("{0:00}:{1:00}", (int)spWorkMin.TotalHours, spWorkMin.Minutes);

                }
                if (string.IsNullOrEmpty(hours))
                    hours = "00:00";
            }
            catch (Exception)
            {

                throw;
            }
            return hours;
        }

        public PartialViewResult ReportType(ReportModel data)
        {
            ViewBag.ReportType = new SelectList(FillReportType(data), "Code", "Text");
            return PartialView();
        }

        //public PartialViewResult PartScanningEfficiencyGrid(ReportModel data)
        //{
        //    if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family) && !string.IsNullOrEmpty(data.ReportType))
        //    {
        //        ViewBag.DataSource = fun.GridDailyaprtScanningEfficiency(data);
        //    }
        //    return PartialView();
        //}
        private List<ReportCode> FillReportType(ReportModel data)
        {
            List<ReportCode> Model = new List<ReportCode>();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(data.Family)))
                    return Model;
                if (!string.IsNullOrEmpty(Convert.ToString(data.Family)) && Convert.ToString(data.Family).ToUpper() == "SYSTEM.DATA.DATAROWVIEW")
                    return Model;
                if (Convert.ToString(data.Plant) == "C01" && Convert.ToString(data.Family) == "CRANE-01")
                {
                    return RT.GetCraneReportTypes;
                }
                else if (Convert.ToString(data.Plant) != "T02" && Convert.ToString(data.Family).ToUpper().Contains("TRACTOR") || Convert.ToString(data.Family).ToUpper().Contains("MRN"))
                {
                    if (data.chkGateReports == true)
                        return RT.GetGateReportTypes;
                    else
                        return RT.GetTractorReportTypes;
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("ENGINE"))
                {
                    return RT.GetEngineReportTypes;
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                {
                    return RT.GetBackendReportTypes;
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("TRANSMISSION"))
                {
                    return RT.GetTransmissionReportTypes;
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("HYDRAULIC"))
                {
                    return RT.GetHydraulicReportTypes;
                }
                else if (Convert.ToString(data.Family).ToUpper().Contains("REAR AXEL"))
                {
                    return RT.GetRearAxelReportTypes;
                }

                else if (Convert.ToString(data.Plant) == "T02" && Convert.ToString(data.Family).ToUpper().Contains("TRACTOR"))
                {
                    if (data.chkGateReports == true)
                        return RT.GetGateReportTypes;
                    else
                        return RT.GetEKIReportTypes;
                }
                else
                {
                    return Model;
                }
            }
            catch (Exception ex)
            {
                return Model;
            }
            finally { }

        }

        

        public JsonResult DDLFilJobs(ReportModel data)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                DataTable dtMain = new DataTable();

                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.gleJobs))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select Jobid || ' # ' || ITEM_CODE || '(' || ITEM_DESCRIPTION || ')' || ' SERIAL_NO:' || FCODE_srlno as JOB_DESCRIPTION,
                        Jobid as JOB,FCODE_srlno as SRLNO from xxes_job_status 
                        where  family_code='" + Convert.ToString(data.Family).Trim() + "' and " +
                        "plant_code='" + Convert.ToString(data.Plant) + "' " +
                        "and JOBID like '{0}%' order by ITEM_CODE,JOBID",data.gleJobs.Trim().ToUpper());

                    dtMain = new DataTable();
                    dtMain = fun.returnDataTable(query);
                    if (dtMain.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtMain.Rows)
                        {
                            Item.Add(new DDLTextValue
                            {
                                Text = dr["JOB_DESCRIPTION"].ToString(),
                                Value = dr["JOB"].ToString(),
                            });
                        }
                    }
                    
                //}
                
            }
            catch(Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return Json(Item, JsonRequestBehavior.AllowGet);

        }

        public PartialViewResult BindShift()
        {
            string Shift = fun.getshift();
            char[] Spearator = { '#' };
            String[] StrShift = Shift.Split(Spearator, StringSplitOptions.None);
            string SelectedShift = StrShift[0];

            ViewBag.Shift = new SelectList(fun.FillShift(), "Value", "Text", SelectedShift);
            return PartialView();
        }

        public JsonResult CurrentShift()
        {
            string Shift = fun.getshift();
            char[] Spearator = { '#' };
            String[] StrShift = Shift.Split(Spearator, StringSplitOptions.None);
            string SelectedShift = StrShift[0];
            return Json(SelectedShift, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult BindFilterBy()
        {
            ViewBag.FilterBy = new SelectList(FillFilterBy(), "Value", "Text");
            return PartialView();
        }

        private List<DDLTextValue> FillFilterBy()
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                Item.Add(new DDLTextValue
                {
                    Text = "BUCKLEUP DATE",
                    Value = "BUCKLEUP DATE",
                });
                Item.Add(new DDLTextValue
                {
                    Text = "ROLLOUT DATE",
                    Value = "ROLLOUT DATE",
                });
                Item.Add(new DDLTextValue
                {
                    Text = "PDI DATE",
                    Value = "PDI DATE",
                });
                Item.Add(new DDLTextValue
                {
                    Text = "HOOKUP DATE",
                    Value = "HOOKUP DATE",
                });
                return Item;
            }
            catch
            {
                return Item;
            }
            finally { }
        }

        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.FillMappingFamily(Plant), "Value", "Text");
            }
            return PartialView();
        }
    }
}