using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class JobFinalController : Controller
    {
        Function fun = new Function();
        JobFinalFunction jff = new JobFinalFunction();
        string query = string.Empty;
        [HttpGet]
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

        [HttpGet]
        public JsonResult GetBattery()
        {
            return Json(jff.Battery_Name(), JsonRequestBehavior.AllowGet);
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
        public JsonResult BindJobFields(TRACTOR data)
        {
            string query = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {

                if (string.IsNullOrEmpty(data.PLANT_CODE) || string.IsNullOrEmpty(data.FAMILY_CODE) || string.IsNullOrEmpty(data.JOBID_TRACTORSRNO))
                {
                    msg = "Plant,Family,Job or Tractor Sr No is required";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }

                DataTable dt = new DataTable();
                query = string.Format(@"select FCODE_srlno || ' # ' || m.ITEM_CODE || '(' || SUBSTR(m.ITEM_DESCRIPTION,0,30) || ')' || ' JOB: ' || Jobid  as JOB_DESCRIPTION, 
                Jobid as JOB,FCODE_srlno as SRLNO from xxes_job_status  s , xxes_item_master m where s.item_code=m.item_code and 
                m.PLANT_CODE='{0}' and m.family_code='{1}' AND (s.JOBID LIKE '{2}%' OR s.FCODE_SRLNO LIKE '{2}%' )
                order by FCODE_srlno", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.JOBID_TRACTORSRNO.Trim().ToUpper());

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["JOB_DESCRIPTION"].ToString(),
                            Value = dr["JOB"].ToString(),
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
        public JsonResult GetJobData(TRACTOR data)
        {
            string query = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<TRACTOR> Item = new List<TRACTOR>();
            try
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE) || string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                if (string.IsNullOrEmpty(data.JOBID))
                {
                    msg = "JOB NOT FOUND !!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                DataTable dt = new DataTable();
                query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND JOBID = '{2}'",
                                        data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.JOBID.Trim().ToUpper());
                if (!fun.CheckExits(query))
                {
                    msg = "INVALID JOB ID !!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                query = string.Format(@"SELECT * FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND JOBID = '{2}'",
                                        data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.JOBID.Trim().ToUpper());

                dt = fun.returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new TRACTOR
                        {
                            TRANSMISSION_SRLNO = Convert.ToString(dr["TRANSMISSION_SRLNO"]),
                            REARAXEL_SRLNO = Convert.ToString(dr["REARAXEL_SRLNO"]),
                            FIPSRNO = Convert.ToString(dr["FIPSRNO"]),
                            HOOKUP_NO = "",
                            HYDRALUIC_SRLNO = Convert.ToString(dr["HYDRALUIC_SRLNO"]),
                            ENGINE_SRLNO = Convert.ToString(dr["ENGINE_SRLNO"]),
                            BACKEND_SRLNO = Convert.ToString(dr["BACKEND_SRLNO"]),
                            REARTYRE_SRLNO1 = Convert.ToString(dr["REARTYRE_SRLNO1"]),
                            RTRIM1 = Convert.ToString(dr["RTRIM1"]),
                            RTTYRE1 = Convert.ToString(dr["RTTYRE1"]),

                            REARTYRE_SRLNO2 = Convert.ToString(dr["REARTYRE_SRLNO2"]),
                            RTRIM2 = Convert.ToString(dr["RTRIM2"]),
                            RTTYRE2 = Convert.ToString(dr["RTTYRE2"]),


                            FRONTTYRE_SRLNO1 = Convert.ToString(dr["FRONTTYRE_SRLNO1"]),
                            FTRIM1 = Convert.ToString(dr["FTRIM1"]),
                            FTTYRE1 = Convert.ToString(dr["FTTYRE1"]),

                            FRONTTYRE_SRLNO2 = Convert.ToString(dr["FRONTTYRE_SRLNO2"]),
                            FTRIM2 = Convert.ToString(dr["FTRIM2"]),
                            FTTYRE2 = Convert.ToString(dr["FTTYRE2"]),

                            BATTERY_SRLNO = Convert.ToString(dr["BATTERY_SRLNO"]),
                            BATTERY_MAKE = Convert.ToString(dr["BATTERY_MAKE"]),

                            HYD_PUMP_SRLNO = Convert.ToString(dr["HYD_PUMP_SRLNO"]),
                            RADIATOR_SRLNO = Convert.ToString(dr["RADIATOR_SRLNO"]),
                            STERING_CYLINDER_SRLNO = Convert.ToString(dr["STERING_CYLINDER_SRLNO"]),
                            STEERING_MOTOR_SRLNO = Convert.ToString(dr["STEERING_MOTOR_SRLNO"]),

                            STEERING_ASSEMBLY_SRLNO = Convert.ToString(dr["STEERING_ASSEMBLY_SRLNO"]),
                            ALTERNATOR_SRLNO = Convert.ToString(dr["ALTERNATOR_SRLNO"]),

                            CLUSSTER_SRLNO = Convert.ToString(dr["CLUSSTER_SRLNO"]),
                            STARTER_MOTOR_SRLNO = Convert.ToString(dr["STARTER_MOTOR_SRLNO"]),
                            QUANTITY = "1",

                            SIM_SERIAL_NO = Convert.ToString(dr["SIM_SERIAL_NO"]),
                            IMEI_NO = Convert.ToString(dr["IMEI_NO"]),
                            MOBILE = Convert.ToString(dr["MOBILE"]),


                            ROPS_SRNO = Convert.ToString(dr["ROPS_SRNO"]),
                            OIL = Convert.ToString(dr["OIL"]),

                        });

                    }
                    msg = "OK !!";
                    mstType = Validation.str;
                    status = Validation.stus;
                    var err = new { Items = Item, Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    msg = "NO RECORD FOUND !!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var err = new { Msg = msg, ID = mstType, validation = status };
                return Json(err, JsonRequestBehavior.AllowGet);
            }
            //var error = new { Msg = msg, ID = mstType, validation = status };
            //return Json(error, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateJob(TRACTOR data)
        {

            DataTable Jobdt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string errorNo = string.Empty;

            bool isTransRequire, isRearAxelRequire, isBackendRequire, isEngineRequire, isSrNoRequire, isHydraulicRequire, isRearTyreRequire, isFrontTyreRequire, isBatRequire, isPumpRequire, isNewROPSSrNo = false,
                    isStMotorRequire, isStAssemblyRequire, isStCylinderRequre, isRaditorRequire, isClusterRequire, isAlternatorRequire, isStartMotorRequire, isROPSRequire; string ROPS_SRNO = string.Empty, ROPS_DCODE = string.Empty;
            string Fcode = "", DCODE_TRANS = "", DCODE_AXEL = "", DCODE_ENGINE = "", DCODE_HYDRUALIC = "", DCODE_RT = "", DCODE_FT = "", DCODE_BATTERY = "", RT_MAKE = "", FT_MAKE = "", Data = "", NewTransmission = "", NewTransJob = "", NewAxel = "", NewAxelJob = "", NewEngine = "", NewEngineJob = "", Fcode_id = "";
            string NewBackend = "", NewBackendJob = "", oldFcodeid = "", oldFcode = "";
            string newTractorSrno = "", SrnotoUpdate = "", stageid = ""; string ActualBackend = "", ActualEngine = "", seljobsdropdown = string.Empty;
            string FCODE_SRLNO = "";
            try
            {
                //bool BlankCheck = blankCheck();

                query = string.Format(@"select ITEM_CODE, FCODE_SRLNO, FCODE_ID, ROPS_SRNO from XXES_JOB_STATUS
                    where JOBID='{0}' and plant_code='{1}' and family_code='{2}'", data.JOBID.Trim().ToUpper(),
                    data.PLANT_CODE.Trim().ToUpper(), data.FAMILY_CODE.Trim().ToUpper());

                Jobdt = fun.returnDataTable(query);
                if (Jobdt.Rows.Count > 0)
                {
                    foreach (DataRow item in Jobdt.Rows)
                    {

                        Fcode = Convert.ToString(item["ITEM_CODE"]) == null ? "" : Convert.ToString(item["ITEM_CODE"]).Trim();
                        Fcode_id = Convert.ToString(item["FCODE_ID"]) == null ? "" : Convert.ToString(item["FCODE_ID"]).Trim();
                        ROPS_SRNO = Convert.ToString(item["ROPS_SRNO"]) == null ? "" : Convert.ToString(item["ROPS_SRNO"]).Trim();
                        FCODE_SRLNO = Convert.ToString(item["FCODE_SRLNO"]) == null ? "" : Convert.ToString(item["FCODE_SRLNO"]).Trim();
                    }

                    query = string.Format(@"select * from XXES_ITEM_MASTER where ITEM_CODE='{0}' and plant_code='{1}' and 
                                family_code='{2}'", Fcode, data.PLANT_CODE.Trim().ToUpper(), data.FAMILY_CODE.Trim().ToUpper());


                    DataTable dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        isTransRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y" ? true : false); isRearAxelRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y" ? true : false);
                        isEngineRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y" ? true : false); isSrNoRequire = (Convert.ToString(dt.Rows[0]["GEN_SRNO"]) == "Y" ? true : false);
                        isHydraulicRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y" ? true : false); isRearTyreRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y" ? true : false);
                        isFrontTyreRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y" ? true : false); isBatRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_BATTERY"]) == "Y" ? true : false);
                        isPumpRequire = (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y" ? true : false); isStMotorRequire = (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y" ? true : false);
                        isStAssemblyRequire = (Convert.ToString(dt.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y" ? true : false); isStCylinderRequre = (Convert.ToString(dt.Rows[0]["REQ_STERING_CYLINDER"]) == "Y" ? true : false);
                        isRaditorRequire = (Convert.ToString(dt.Rows[0]["REQ_RADIATOR"]) == "Y" ? true : false); isClusterRequire = (Convert.ToString(dt.Rows[0]["REQ_CLUSSTER"]) == "Y" ? true : false);
                        isAlternatorRequire = (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y" ? true : false); isStartMotorRequire = (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y" ? true : false);
                        isBackendRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y" ? true : false);
                        isROPSRequire = (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y" ? true : false);
                        ROPS_DCODE = Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"]).Trim();

                        #region BLANKCHECK
                        if (string.IsNullOrEmpty(data.TRANSMISSION_SRLNO) && isTransRequire)
                        {
                            msg = "Transmission SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "1";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.REARAXEL_SRLNO) && isRearAxelRequire)
                        {
                            msg = "RearAxel SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "2";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.BACKEND_SRLNO) && isBackendRequire) //&& Convert.ToString(cmbPlant.SelectedValue).Trim() == "T05"
                        {
                            msg = "Backend SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "3";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.ENGINE_SRLNO) && isEngineRequire)
                        {
                            msg = "Engine SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "4";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.HYDRALUIC_SRLNO) && isHydraulicRequire)
                        {
                            msg = "Hydraulic SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "5";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.REARTYRE_SRLNO1) && isRearTyreRequire)
                        {
                            msg = "Rear Tyre Assembly SrlNo1 should not be empty";
                            mstType = Validation.str1;
                            errorNo = "6";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.REARTYRE_SRLNO2) && isRearTyreRequire)
                        {
                            msg = "Rear Tyre Assembly SrlNo2 should not be empty";
                            mstType = Validation.str1;
                            errorNo = "7";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }

                        else if (string.IsNullOrEmpty(data.FRONTTYRE_SRLNO1) && isFrontTyreRequire)
                        {
                            msg = "Front Tyre Assembly SrlNo1 should not be empty";
                            mstType = Validation.str1;
                            errorNo = "8";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);

                        }
                        else if (string.IsNullOrEmpty(data.FRONTTYRE_SRLNO2) && isFrontTyreRequire)
                        {
                            msg = "Front Tyre Assembly SrlNo2 should not be empty";
                            mstType = Validation.str1;
                            errorNo = "9";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }

                        else if (string.IsNullOrEmpty(data.BATTERY_SRLNO) && isBatRequire)
                        {
                            msg = "Battery SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "10";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (isBatRequire && string.IsNullOrEmpty(data.BATTERY_MAKE))
                        {
                            msg = "Select Make of battery";
                            mstType = Validation.str1;
                            errorNo = "11";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.HYD_PUMP_SRLNO) && isPumpRequire)
                        {
                            msg = "Hydraulic Pump SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "12";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.STEERING_MOTOR_SRLNO) && isStMotorRequire)
                        {
                            msg = "Steering Motor SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "13";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.STEERING_ASSEMBLY_SRLNO) && isStAssemblyRequire)
                        {
                            msg = "Steering Assembly SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "14";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.STERING_CYLINDER_SRLNO) && isStCylinderRequre)
                        {
                            msg = "Power Steering Cylinder SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "15";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.RADIATOR_SRLNO) && isRaditorRequire)
                        {
                            msg = "Radiator Assembly SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "16";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.CLUSSTER_SRLNO) && isClusterRequire)
                        {
                            msg = "Cluster Assembly SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "17";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.ALTERNATOR_SRLNO) && isAlternatorRequire)
                        {
                            msg = "Alternator SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "18";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(data.STARTER_MOTOR_SRLNO) && isStartMotorRequire)
                        {
                            msg = "Starter motor SrlNo should not be empty";
                            mstType = Validation.str1;
                            errorNo = "19";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        else if (string.IsNullOrEmpty(ROPS_DCODE) && isROPSRequire)
                        {
                            msg = "ROPS DCODE NOT FOUND IN TRACTOR MAPPING";
                            mstType = Validation.str1;
                            errorNo = "20";
                            var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                            return Json(err, JsonRequestBehavior.AllowGet);
                        }
                        #endregion BLANKCHECK

                        
                        if (data.PLANT_CODE.Trim() == "T04")
                        {
                            #region CHECK TRANSMISSION AND REARAXEL
                            var buckleup = jff.CheckBuckleUp(data, Fcode.ToUpper().Trim(), isRearAxelRequire, isTransRequire);

                            if (buckleup.Item1 == false)
                            {
                                mstType = Validation.str1;
                                errorNo = buckleup.Item2;
                                var err = new { Msg = buckleup.Item3, ID = mstType, ErrorNo = errorNo };
                                return Json(err, JsonRequestBehavior.AllowGet);
                            }
                            else if (buckleup.Item1 == true)
                            {
                                DCODE_TRANS = buckleup.Item4;
                                DCODE_AXEL = buckleup.Item5;
                            }
                            #endregion
                            
                            #region CHECK HYDRAULIC
                            var hydraulic = jff.ChecKHydraulic(data, Fcode, isHydraulicRequire);

                            if (hydraulic.Item1 == false)
                            {
                                mstType = Validation.str1;
                                errorNo = hydraulic.Item2;
                                var err = new { Msg = hydraulic.Item3, ID = mstType, ErrorNo = errorNo };
                                return Json(err, JsonRequestBehavior.AllowGet);
                            }
                            else if (hydraulic.Item1 == true)
                            {
                                DCODE_HYDRUALIC = hydraulic.Item4;
                            }
                            #endregion
                        }


                        if (isROPSRequire == true)
                        {
                            ROPS_SRNO = data.ROPS_SRNO;
                            if (string.IsNullOrEmpty(ROPS_SRNO))
                            {
                                if (fun.CheckExits("select count(*) from  xxes_torque_master where item_dcode='" + ROPS_DCODE.Trim() + "' and srno_req=1 and plant_code='" + data.PLANT_CODE.Trim() + "' and family_code='" + data.FAMILY_CODE.Trim() + "'"))
                                {
                                    if(string.IsNullOrEmpty(data.RopsUserConfirmation))
                                    {
                                        errorNo = "-100";
                                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                                        return Json(err, JsonRequestBehavior.AllowGet);
                                    }
                                    else if(data.RopsUserConfirmation == "YES")
                                    {
                                        jff.GetROPSSrno(data.PLANT_CODE,data.FAMILY_CODE,Fcode, ROPS_DCODE, out ROPS_SRNO);
                                        if (!string.IsNullOrEmpty(ROPS_SRNO))
                                        {
                                            //txtJRops.Text = ROPS_SRNO;
                                            data.ROPS_SRNO = ROPS_SRNO; 
                                            isNewROPSSrNo = true;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                query = @"select count(*) from  xxes_torque_master where item_dcode='" + ROPS_DCODE.Trim() + "' and srno_req=1 and plant_code='" + data.PLANT_CODE.Trim() + "' and family_code='" + data.FAMILY_CODE.Trim() + "' and nvl(current_serialno,0)<'" + ROPS_SRNO + "'";
                                if (fun.CheckExits(query))
                                {
                                    msg = "ROPS Serial no not valid. It should be in range of start and end serial number";
                                    mstType = Validation.str1;
                                    errorNo = "20";
                                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                                    return Json(err, JsonRequestBehavior.AllowGet);
                                    
                                }
                                isNewROPSSrNo = false;
                            }
                            
                            
                             query = "select count(*) from XXES_JOB_STATUS s,xxes_item_master m where s.item_code=m.item_code and ROPS_ITEM_CODE='" + ROPS_DCODE + "' and  ROPS_SRNO='" + ROPS_SRNO + "' and jobid<>'" + data.JOBID.Trim() + "' and s.plant_code='" + data.PLANT_CODE.Trim() + "' and s.family_code='" + data.FAMILY_CODE.Trim() + "'";
                            if (fun.CheckExits(query))
                            {
                                msg = "ROPS Srlno used in some other job";
                                mstType = Validation.str1;
                                errorNo = "20";
                                var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                                return Json(err, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //if (!string.IsNullOrEmpty(data.ENGINE_SRLNO) && data.PLANT_CODE.Trim() == "T04")
                        //{
                        //    //var tuple = jff.CheckEngine(data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.ENGINE_SRLNO.Trim(), data.JOBID, Fcode.ToUpper().Trim(), isEngineRequire, isSrNoRequire, FCODE_SRLNO.Trim(), Fcode_id);
                        //    //if (tuple.Item1 == false)
                        //    //{
                        //    //    mstType = Validation.str1;
                        //    //    errorNo = tuple.Item2;
                        //    //    var err = new { Msg = tuple.Item3, ID = mstType, ErrorNo = errorNo };
                        //    //    return Json(err, JsonRequestBehavior.AllowGet);
                        //    //}
                        //    //else if (tuple.Item1 == true)
                        //    //{
                        //    //    //DCODE_TRANS = tuple.Item4;
                        //    //    //DCODE_AXEL = tuple.Item5;
                        //    //}

                        //    //if (!CheckEngine(txtJEngine.Text.Trim(), JOB, Fcode, isEngineRequire, isSrNoRequire, Data.Split('#')[1].Trim(), Fcode_id, out newTractorSrno, out SrnotoUpdate, out stageid, out DCODE_ENGINE, out NewEngine, out NewEngineJob))
                        //    //    return false;
                        //    //if (!string.IsNullOrEmpty(newTractorSrno) && !chkReplace.Checked)
                        //    //{
                        //    //    if (newTractorSrno.Trim() != "" && newTractorSrno.Trim().ToUpper() != Data.Split('#')[1].Trim().ToUpper() && isSrNoRequire == true)
                        //    //    {
                        //    //        pbf.UpdateTractor(newTractorSrno.Trim(), SrnotoUpdate, JOB.Trim(), Fcode, txtJAxel.Text.Trim(), txtJTrans.Text.Trim(), txtJEngine.Text.Trim().ToUpper(), DCODE_ENGINE, false, "", Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper(), isROPSRequire, ROPS_SRNO, ROPS_DCODE);
                        //    //    }
                        //    //}
                        //}
                        //else if (!string.IsNullOrEmpty(txtJEngine.Text.Trim().ToUpper()) && !string.IsNullOrEmpty(txtJBackend.Text.Trim().ToUpper()) && Convert.ToString(cmbPlant.SelectedValue).Trim() == "T05")
                        //{
                        //    if (!CheckBackendEngine(Fcode, JOB, txtJBackend.Text.Trim(), txtJEngine.Text, isBackendRequire, isEngineRequire, isSrNoRequire, Data.Split('#')[1].Trim(), Fcode_id, out newTractorSrno, out SrnotoUpdate, out ActualBackend, out ActualEngine, out NewBackend, out NewBackendJob, out NewEngine, out NewEngineJob))
                        //        return false;
                        //    if (!string.IsNullOrEmpty(newTractorSrno) && !chkReplace.Checked)
                        //    {
                        //        if (newTractorSrno.Trim() != "" && newTractorSrno.Trim().ToUpper() != Data.Split('#')[1].Trim().ToUpper() && isSrNoRequire == true && !string.IsNullOrEmpty(SrnotoUpdate) && !optPlanned.Checked)
                        //        {
                        //            pbf.UpdateTractorPT(newTractorSrno.Trim(), SrnotoUpdate, JOB, Fcode, txtJBackend.Text.Trim(), ActualBackend, txtJEngine.Text.Trim(), ActualEngine, false, "", "", Fcode_id, Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper(), false, isROPSRequire, ROPS_DCODE, ROPS_SRNO);
                        //        }
                        //    }
                        //}

                        if (!string.IsNullOrEmpty(data.REARTYRE_SRLNO1) && !string.IsNullOrEmpty(data.REARTYRE_SRLNO2))
                        {
                            #region CHECK REAR TYRE
                            var reartyre = jff.CheckTyres_COM(isRearTyreRequire,data.REARTYRE_SRLNO1.Trim().ToUpper(), data.REARTYRE_SRLNO2.Trim().ToUpper(),data.JOBID, Fcode, "RT");

                            if (reartyre.Item1 == false)
                            {
                                mstType = Validation.str1;
                                errorNo = reartyre.Item2;
                                var err = new { Msg = reartyre.Item3, ID = mstType, ErrorNo = errorNo };
                                return Json(err, JsonRequestBehavior.AllowGet);
                            }
                            else if (reartyre.Item1 == true)
                            {
                                DCODE_RT = reartyre.Item4;
                                RT_MAKE = reartyre.Item5;
                            }
                            #endregion
                            
                        }
                    }
                }
                else
                {
                    msg = "INVALID JOB !!";
                    mstType = Validation.str1;
                    errorNo = "0";
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }



            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.ToString();
                mstType = Validation.str1;
                errorNo = "0";
                var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(err, JsonRequestBehavior.AllowGet);

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private bool blankCheck()
        {
            bool isBlankcheckRequire = false;
            try
            {
                if (Convert.ToString(Session["Login_User"]).Trim().ToUpper() != "GU")
                {
                    query = string.Format(@"select count(*)  from XXES_SFT_SETTINGS where PARAMVALUE='{0}' and PARAMETERINFO='SHOW_SAVE_CHECK_EMPTY' 
                                        and Status='Y'", Convert.ToString(Session["Login_User"]).Trim().ToUpper());
                    if (fun.CheckExits(query))
                        isBlankcheckRequire = true;
                    else
                        isBlankcheckRequire = false;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return isBlankcheckRequire;
        }
    }
}