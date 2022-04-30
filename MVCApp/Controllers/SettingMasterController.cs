using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

using System.Data;
using Newtonsoft.Json;


namespace MVCApp.Controllers
{
    [Authorize]
    public class SettingMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
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
                return View();
            }
        }

        public JsonResult SaveSetting(SettingModel setting)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    cmd = new OracleCommand("UDSP_PLANNING_SETTING", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("RollaingPlanDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.RollaingPlanDays;
                    cmd.Parameters.Add("FixedPlanDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.FixedPlanDays;

                    cmd.Parameters.Add("HydroDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.HydraulicDays;
                    cmd.Parameters.Add("EngDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.EngineDays;
                    cmd.Parameters.Add("RExlDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.ReareAxleDays;
                    cmd.Parameters.Add("TransDays", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.TransmissionDays;

                    cmd.Parameters.Add("Engine_ShiftA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Engine_ShiftA;
                    cmd.Parameters.Add("Engine_ShiftB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Engine_ShiftB;
                    cmd.Parameters.Add("Engine_ShiftC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Engine_ShiftC;

                    cmd.Parameters.Add("Transmission_ShiftA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Transmission_ShiftA;
                    cmd.Parameters.Add("Transmission_ShiftB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Transmission_ShiftB;
                    cmd.Parameters.Add("Transmission_ShiftC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Transmission_ShiftC;

                    cmd.Parameters.Add("ReareAxle_ShiftA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.ReareAxle_ShiftA;
                    cmd.Parameters.Add("ReareAxle_ShiftB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.ReareAxle_ShiftB;
                    cmd.Parameters.Add("ReareAxle_ShiftC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.ReareAxle_ShiftC;

                    cmd.Parameters.Add("Hydraulic_ShiftA", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Hydraulic_ShiftA;
                    cmd.Parameters.Add("Hydraulic_ShiftB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Hydraulic_ShiftB;
                    cmd.Parameters.Add("Hydraulic_ShiftC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = setting.Hydraulic_ShiftC;

                    cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                    fun.ConOpen();
                    cmd.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                    {
                        if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                        {
                            msg = "All setting saved successfully...";
                            mstType = "alert-success";
                        }
                        var resulT = new { Msg = msg, ID = mstType };
                        return Json(resulT, JsonRequestBehavior.AllowGet);
                    }
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

        public JsonResult GetSetting()
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                List<ListSetting> Tyres = new List<ListSetting>();
                query = "SELECT * FROM xxes_sft_settings where status = 'MVC'";
                DataTable dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Tyres.Add(new ListSetting
                        {
                            Description = Convert.ToString(dr["PARAMETERINFO"]),
                            Value = Convert.ToInt32(dr["PARAMVALUE"]),
                        });
                    }
                    var resul = Tyres;
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
            
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }

    public class ListSetting
    {
        public string Description { get; set; }
        public int Value { get; set; }
    }
}