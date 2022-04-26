using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class ReportModel
    {
        OracleDataAdapter DA = null;
        Function fun = new Function();
        public string Plant { get; set; }
        public string Family { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ReportType { get; set; }
        public bool chkGateReports { get; set; }
        public bool chkShowLess { get; set; }

        public string FilterBy { get; set; }
        public bool LessFields { get; set; }

        public string gleJobs { get; set; }

        public string gleJobsDesc { get; set; }

        public string Job { get; set; }
        public string SHORT_BULK { get; set; }

        public string Shift { get; set; }
        public string ShiftText { get; set; }
        public string ShiftValue { get; set; }
        public string QCPRINT { get; set; }

        public bool ShiftA { get; set; }
        public bool ShiftB { get; set; }
        public bool ShiftC { get; set; }
        public string OrgID { get; set; }
        public string ShowLess { get; set; }
        public string STFilterBy { get; set; }


        public DataTable GenerateReports(ReportModel reportModel)
        {
            DataTable dtMain = new DataTable();
            try
            {
                if (DA == null)
                    DA = new OracleDataAdapter("USP_SHIFTREPORTS", fun.Connection());
                DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                DA.SelectCommand.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.Plant;
                DA.SelectCommand.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.Family;
                DA.SelectCommand.Parameters.Add("PORGID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.OrgID;
                DA.SelectCommand.Parameters.Add("pFROMDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.FromDate;
                DA.SelectCommand.Parameters.Add("pTODATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ToDate;
                DA.SelectCommand.Parameters.Add("pASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftA == true ? "A" : null);
                DA.SelectCommand.Parameters.Add("pBSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftB == true ? "B" : null);
                DA.SelectCommand.Parameters.Add("pCSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftC == true ? "C" : null);
                DA.SelectCommand.Parameters.Add("PISLESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ShowLess;
                DA.SelectCommand.Parameters.Add("PFILTERBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.QCPRINT;
                DA.SelectCommand.Parameters.Add("pREPORTTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ReportType;
                DA.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                DA.Fill(dtMain);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return dtMain;
        }

        public DataTable GenerateReportsTOTROLL(ReportModel reportModel)
        {
            DataTable dtMain = new DataTable();
            try
            {
                if (DA == null)
                    DA = new OracleDataAdapter("USP_SHIFTREPORTS", fun.Connection());
                DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                DA.SelectCommand.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.Plant;
                DA.SelectCommand.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.Family;
                DA.SelectCommand.Parameters.Add("PORGID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.OrgID;
                DA.SelectCommand.Parameters.Add("pFROMDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.FromDate;
                DA.SelectCommand.Parameters.Add("pTODATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ToDate;
                DA.SelectCommand.Parameters.Add("pASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftA == true ? "A" : null);
                DA.SelectCommand.Parameters.Add("pBSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftB == true ? "B" : null);
                DA.SelectCommand.Parameters.Add("pCSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = (reportModel.ShiftC == true ? "C" : null);
                DA.SelectCommand.Parameters.Add("PISLESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ShowLess;
                DA.SelectCommand.Parameters.Add("PFILTERBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.FilterBy;
                DA.SelectCommand.Parameters.Add("pREPORTTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = reportModel.ReportType;
                DA.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                DA.Fill(dtMain);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return dtMain;
        }
    }


}