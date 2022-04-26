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
using Oracle.Web;
using System.Data;
using Newtonsoft.Json;

namespace MVCApp.Controllers
{
    [Authorize]
    public class Setting_MasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();

        string query = "";

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

        #region PLANNING DAYS
        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult Tab_PlanningDays()
        {
            ViewBag.PD_Description = new SelectList(Bind_PlanningDays_Description(), "Value", "Description");
            return PartialView();
        }
        public List<Setting> Bind_PlanningDays_Description()
        {
            List<Setting> Shift = new List<Setting>();
            try
            {
                Shift.Add(new Setting
                {
                    Description = "Rolling plan days from current date to plan date.",
                    Value = "RollaingPlanDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Fixed plan can be change before how many days of planning date.",
                    Value = "FixedPlanDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Sub-Assembly (Engine) plan can be change before how many days of planning date.",
                    Value = "EngineDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Sub-Assembly (Transmission) plan can be change before how many days of planning date.",
                    Value = "TransmissionDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Sub-Assembly (Reare Axle) plan can be change before how many days of planning date.",
                    Value = "ReareAxleDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Sub-Assembly (Hydraulic) plan can be change before how many days of planning date.",
                    Value = "HydraulicDays",
                });
                Shift.Add(new Setting
                {
                    Description = "Sub-Assembly (Backend) plan can be change before how many days of planning date.",
                    Value = "BackendDays",
                });

                return Shift;
            }
            catch (Exception ex)
            {
                return Shift;
            }
        }

        public JsonResult Save_PD(Planning_Days_Model PD)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(PD.Plant_PD))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(PD.Family_PD))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(PD.ParamInfo_PD))
                {
                    msg = "Please Select Description ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }               
                if (string.IsNullOrEmpty(PD.ParamValue_PD))
                {
                    msg = "Days should not be empty ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                cmd = new OracleCommand("UDSP_PLANNING_SETTING", fun.Connection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("pPlant", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.Plant_PD;
                cmd.Parameters.Add("pFamily", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.Family_PD;
                cmd.Parameters.Add("pDescription", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.Description_PD;

                cmd.Parameters.Add("pParameterInfo", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.ParamInfo_PD;
                cmd.Parameters.Add("pParameterValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.ParamValue_PD;
                cmd.Parameters.Add("pStatus", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PD.Status_PD;

                cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                fun.ConOpen();
                cmd.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                {
                    if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                    {
                        msg = "Saved successfully...";
                        mstType = "alert-success";
                    }
                    var resulT = new { Msg = msg, ID = mstType };
                    return Json(resulT, JsonRequestBehavior.AllowGet);
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

        public PartialViewResult PD_Grid(Planning_Days_Model PD)
        {
            query = @"SELECT * FROM XXES_SFT_SETTINGS WHERE STATUS = '"+ PD.Status_PD + "' AND PLANT_CODE = '"+ PD.Plant_PD +"'";
            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }

        #endregion

        #region PRODUCTION CAPACITY
        public PartialViewResult Tab_ProductionCapacity()
        {
            ViewBag.Shift = new SelectList(fun.FillShift(), "Value", "Text");
            return PartialView();
        }
        public JsonResult Save_PC(Production_Capacity_Model PC)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(PC.Plant_PC))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(PC.Family_PC))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(PC.Shift_PC))
                {
                    msg = "Please Select Shift ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(PC.ParamValue_PC))
                {
                    msg = "Max Qty should not be empty ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                cmd = new OracleCommand("UDSP_PLANNING_SETTING", fun.Connection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("pPlant", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.Plant_PC;
                cmd.Parameters.Add("pFamily", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.Family_PC;
                cmd.Parameters.Add("pDescription", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.Family_PC + " Shift-" + PC.Shift_PC + " maximum production capacity of Sub-Assembly";

                cmd.Parameters.Add("pParameterInfo", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.Family_PC + "-Shift" + PC.Shift_PC;
                cmd.Parameters.Add("pParameterValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.ParamValue_PC;
                cmd.Parameters.Add("pStatus", OracleDbType.NVarchar2, ParameterDirection.Input).Value = PC.Status_PC;

                cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                fun.ConOpen();
                cmd.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                {
                    if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                    {
                        msg = "Saved successfully...";
                        mstType = "alert-success";
                    }
                    var resulT = new { Msg = msg, ID = mstType };
                    return Json(resulT, JsonRequestBehavior.AllowGet);
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

        public PartialViewResult PC_Grid()
        {
            query = @"SELECT * FROM XXES_SFT_SETTINGS WHERE STATUS = 'MAX_PRO' ORDER BY FAMILY_CODE, DESCRIPTION";

            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }

        #endregion

        #region WEEKLY OFF
        public PartialViewResult Tab_WeeklyOff()
        {
            ViewBag.WOType = new SelectList(Bind_WeeklyOff(), "Value", "Description");
            return PartialView();
        }
        public List<Setting> Bind_WeeklyOff()
        {
            List<Setting> Shift = new List<Setting>();
            try
            {
                Shift.Add(new Setting
                {
                    Description = "Sunday",
                    Value = "Sun",
                });
                Shift.Add(new Setting
                {
                    Description = "Saturday and Sunday",
                    Value = "Sat/Sun",
                });
                Shift.Add(new Setting
                {
                    Description = "Only Second Saturday and All Sunday",
                    Value = "2_Sat/Sun",
                }); Shift.Add(new Setting
                {
                    Description = "Second and Fourth Saturday and All Sunday",
                    Value = "2_4_Sat/Sun",
                });

                return Shift;
            }
            catch (Exception ex)
            {
                return Shift;
            }
        }
        public JsonResult Save_WO(Weekly_Off_Model WO)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(WO.Plant_WO))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //if (string.IsNullOrEmpty(WO.Family_WO))
                //{
                //    msg = "Please Select Family ..";
                //    mstType = "alert-danger";
                //    var resul = new { Msg = msg, ID = mstType };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                if (string.IsNullOrEmpty(WO.ParamInfo_WO))
                {
                    msg = "Please Select Weekly Off Days ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                cmd = new OracleCommand("UDSP_PLANNING_SETTING", fun.Connection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("pPlant", OracleDbType.NVarchar2, ParameterDirection.Input).Value = WO.Plant_WO;
                cmd.Parameters.Add("pFamily", OracleDbType.NVarchar2, ParameterDirection.Input).Value = WO.Family_WO;
                cmd.Parameters.Add("pDescription", OracleDbType.NVarchar2, ParameterDirection.Input).Value = WO.Description_WO + " - Weekly Off";

                cmd.Parameters.Add("pParameterInfo", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "WeeklyOff";
                cmd.Parameters.Add("pParameterValue", OracleDbType.NVarchar2, ParameterDirection.Input).Value = WO.ParamInfo_WO;
                cmd.Parameters.Add("pStatus", OracleDbType.NVarchar2, ParameterDirection.Input).Value = WO.Status_WO;

                cmd.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                cmd.Parameters["return_message"].Direction = ParameterDirection.Output;
                fun.ConOpen();
                cmd.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["return_message"].Value)))
                {
                    if (Convert.ToString(cmd.Parameters["return_message"].Value) == "OK")
                    {
                        msg = "Saved successfully...";
                        mstType = "alert-success";
                    }
                    var resulT = new { Msg = msg, ID = mstType };
                    return Json(resulT, JsonRequestBehavior.AllowGet);
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
        public PartialViewResult WO_Grid() 
        {
            query = @"SELECT * FROM XXES_SFT_SETTINGS WHERE STATUS = 'W_OFF'";

            ViewBag.DataSource = fun.returnDataTable(query);
            return PartialView();
        }
        #endregion
    }

    public class Setting
    {
        public string Description { get; set; }
        public string Value { get; set; }
    }
}