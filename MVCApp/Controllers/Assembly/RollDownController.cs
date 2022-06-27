using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using MVCApp.Common;
using System.Drawing.Printing;
using System.IO;
using System.Threading;

namespace MVCApp.Controllers
{
    public class RollDownController : Controller
    {
        // GET: HookUpAndDown
        string msg = string.Empty; string mstType = string.Empty; string swapbtn = string.Empty, CAREBUTTONOIL = string.Empty, TRACTORTYPE = string.Empty, ROLLOUTSTICKER = string.Empty,
           isEnableCarebutton = string.Empty; string TractorCode = "";
        Function fun = new Function();
        string query = string.Empty;
        DataTable dt = new DataTable();
        PrintAssemblyBarcodes af = new PrintAssemblyBarcodes();
        JobFinalFunction jobFinalFunction = new JobFinalFunction();
        [HttpGet]
        public ActionResult Index()
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
                result = fun.Fill_FamilyMRNVerfication(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindStages(string plant, string family)
        {
            List<StageCode> result = new List<StageCode>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(plant) || string.IsNullOrEmpty(family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(family))
                {
                    if (fun.getUSERNAME() == "RS")
                    {
                        query = string.Format(@"SELECT SM.stage_description description,SM.offline_keycode code FROM xxes_stage_master SM 
                                    WHERE SM.PLANT_CODE='{0}' AND SM.family_code='{1}' AND IPADDR IS NOT NULL and offline_keycode not in ('QUALITY')", plant.Trim().ToUpper(), family.Trim().ToUpper(), fun.getUSERNAME());

                    }
                    else
                    {
                        query = string.Format(@"SELECT SM.stage_description description,SM.offline_keycode code FROM xxes_stage_master SM 
                                    inner join xxes_users_master UM ON SM.STAGE_ID=UM.STAGEID
                                    WHERE SM.PLANT_CODE='{0}' AND SM.family_code='{1}' AND UM.USRNAME='{2}' AND IPADDR IS NOT NULL and offline_keycode not in ('QUALITY')", plant.Trim().ToUpper(), family.Trim().ToUpper(), fun.getUSERNAME());
                    }
                    //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                    //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        result.Add(new StageCode
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["CODE"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillItemCode(RollDown data)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.JOBID))
                {
                    query = string.Format(@"select FCODE_SRLNO || ' # '|| ITEM_CODE || '(' || substr(ITEM_DESCRIPTION,1,25) || ')' || ' # ' || ' JOB :' || ' # ' || 
                             JOBID DESCRIPTION,JOBID  FROM XXES_JOB_STATUS" +
                        " where JOBID LIKE '%{0}%'  Or FCODE_SRLNO LIKE '%{0}%' Or ITEM_CODE LIKE '%{0}%' AND PLANT_CODE='{1}' And FAMILY_CODE='{2}' order by JOBID", data.JOBID.Trim().ToUpper(), data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper());
                    //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                    //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                }


                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["JOBID"].ToString(),
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

        public JsonResult BindData(RollDown obj)
        {
            string msg = string.Empty; string TractorCode = string.Empty; string Fcode = string.Empty;
            string swapbtn = string.Empty, CAREBUTTONOIL = string.Empty, TRACTORTYPE = string.Empty, ROLLOUTSTICKER = string.Empty,
            isEnableCarebutton = string.Empty;
            string TractorHook = "", FCODEIDHOOK = string.Empty, EngineDcode = string.Empty, Backend = string.Empty, Engine = string.Empty,
                FINALDATEHOOK, fipsrno = string.Empty, scanjob = string.Empty;
            RollDown tm = new RollDown();
            JobFinalFunction jobFinal = new JobFinalFunction();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(obj.JOBID.Trim())) || obj.JOBID == null)
                {
                    msg = "Scan job to continue.";
                    var myResult1 = new
                    {
                        Result = tm,
                        Msg = msg
                    };
                    return Json(myResult1, JsonRequestBehavior.AllowGet);
                }
                if (obj.PLANTCODE == null || obj.FAMILYCODE == null)
                {
                    msg = "Please Select Plant And Family";
                    var myResult1 = new
                    {
                        Result = tm,
                        Msg = msg
                    };
                    return Json(myResult1, JsonRequestBehavior.AllowGet);
                }
                query = @"select count(*) from XXES_JOB_STATUS where JOBID='" + obj.JOBID.Trim() + "' or  FCODE_SRLNO='" + obj.TractorSrlno.Trim() + "' or  ITEM_CODE='" + obj.JOBID.Trim() + "' " +
                    " and  plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' " +
                    " and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";

                if (!fun.CheckExits(query))
                {
                    msg = "Invalid Job";
                    var myResult1 = new
                    {
                        Result = tm,
                        Msg = msg
                    };
                    return Json(myResult1, JsonRequestBehavior.AllowGet);
                    //return;
                }
                scanjob = obj.JOBID.Trim().ToUpper();
                //displayJobDetails(obj.ITEM_CODE.Trim().ToUpper());
                query = "select * from XXES_JOB_STATUS where JOBID='" + obj.JOBID.Trim() + "' or  FCODE_SRLNO='" + obj.TractorSrlno.Trim() + "' or  ITEM_CODE='" + obj.JOBID.Trim() + "' and  plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    scanjob = obj.JOBID;
                    FCODEIDHOOK = Convert.ToString(dt.Rows[0]["FCODE_ID"]);
                    FINALDATEHOOK = Convert.ToString(dt.Rows[0]["FINAL_LABEL_DATE"]);

                    TractorHook = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                    swapbtn = Convert.ToString(dt.Rows[0]["SWAPCAREBTN"]);
                    CAREBUTTONOIL = Convert.ToString(dt.Rows[0]["CAREBUTTONOIL"]);
                    TRACTORTYPE = Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]);
                    ROLLOUTSTICKER = FINALDATEHOOK;
                    tm.Quantity = Convert.ToString(dt.Rows[0]["OIL"]);
                    tm.Transmission = Convert.ToString(dt.Rows[0]["TRANSMISSION_SRLNO"]);
                    //tm.Transmission = txtJTrans.Text;
                    tm.RearAxel = Convert.ToString(dt.Rows[0]["REARAXEL_SRLNO"]);
                    //RearAxel = txtJAxel.Text;
                    tm.Engine = Convert.ToString(dt.Rows[0]["ENGINE_SRLNO"]);
                    Engine = tm.Engine;
                    EngineDcode = Convert.ToString(dt.Rows[0]["ENGINE"]);
                    fipsrno = Convert.ToString(dt.Rows[0]["FIPSRNO"]);
                    tm.Backend = Convert.ToString(dt.Rows[0]["BACKEND_SRLNO"]);
                    Backend = tm.Backend;
                    tm.Hydraulic = Convert.ToString(dt.Rows[0]["HYDRALUIC_SRLNO"]);
                    // Hydraulic = txtJHydraulic.Text;
                    tm.RearSrnn1 = Convert.ToString(dt.Rows[0]["REARTYRE_SRLNO1"]);
                    //Reartyre_SrNo1 = txtJRTSR1.Text;
                    tm.RearSrnn2 = Convert.ToString(dt.Rows[0]["REARTYRE_SRLNO2"]);
                    //Reartyre_SrNo2 = txtJRTSR2.Text;
                    tm.FrontSrnn1 = Convert.ToString(dt.Rows[0]["FRONTTYRE_SRLNO1"]);
                    //FrontTyre_SrNo1 = txtJFTSR1.Text;
                    tm.FrontSrnn2 = Convert.ToString(dt.Rows[0]["FRONTTYRE_SRLNO2"]);
                    //FrontTyre_SrNo2 = txtJFTSR2.Text;
                    tm.Battery = Convert.ToString(dt.Rows[0]["BATTERY_SRLNO"]);
                    //Battery = txtJBattery.Text;
                    tm.HydrualicPump = Convert.ToString(dt.Rows[0]["HYD_PUMP_SRLNO"]);
                    //HydrulicPump = txtJPump.Text;
                    tm.SteeringMotor = Convert.ToString(dt.Rows[0]["STEERING_MOTOR_SRLNO"]);
                    //SteeringMotor = txtJStMotor.Text;
                    tm.SteeringAssem = Convert.ToString(dt.Rows[0]["STEERING_ASSEMBLY_SRLNO"]);
                    //SteeringAssembly = txtJSTAssem.Text;
                    tm.SteeringCylinder = Convert.ToString(dt.Rows[0]["STERING_CYLINDER_SRLNO"]);
                    //SteeringCylinder = txtJSTCylinder.Text;
                    tm.Radiator = Convert.ToString(dt.Rows[0]["RADIATOR_SRLNO"]);
                    //RaditorAssembly = txtJRadiator.Text;
                    tm.Cluster = Convert.ToString(dt.Rows[0]["CLUSSTER_SRLNO"]);
                    //clusterAssembly = txtJCLAssem.Text;
                    tm.Alternator = Convert.ToString(dt.Rows[0]["ALTERNATOR_SRLNO"]);
                    //Alternator = txtJAlternator.Text;
                    tm.Motor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR_SRLNO"]);
                    //StarterMotor = txtJSTRTMOTOR.Text;
                    tm.RearRIM1 = Convert.ToString(dt.Rows[0]["RTRIM1"]);
                    //Reartyre_Rim1 = txtRTRIM1.Text;
                    tm.RearRIM2 = Convert.ToString(dt.Rows[0]["RTRIM2"]);
                    //Reartyre_Rim2 = txtRTRIM2.Text;
                    tm.FrontRIM1 = Convert.ToString(dt.Rows[0]["FTRIM1"]);
                    //FrontTyre_Rim1 = txtFTRIM1.Text;
                    tm.FrontRIM2 = Convert.ToString(dt.Rows[0]["FTRIM2"]);
                    //FrontTyre_Rim2 = txtFTRIM2.Text;
                    tm.RearTyre1 = Convert.ToString(dt.Rows[0]["RTTYRE1"]);
                    //Reartyre1 = txtRTTYRE1.Text;
                    tm.RearTyre2_srlno2 = Convert.ToString(dt.Rows[0]["RTTYRE2"]);
                    //Reartyre2 = txtRTTYRE2.Text;
                    tm.FrontTyre1_srlno1 = Convert.ToString(dt.Rows[0]["FTTYRE1"]);
                    //FrontTyre1 = txtFTTYRE1.Text;
                    tm.FrontTyre2_srlno2 = Convert.ToString(dt.Rows[0]["FTTYRE2"]);
                    tm.Srno = Convert.ToString(dt.Rows[0]["SIM_SERIAL_NO"]);
                    tm.IMEI = Convert.ToString(dt.Rows[0]["IMEI_NO"]);
                    tm.MOBILE = Convert.ToString(dt.Rows[0]["MOBILE"]);
                    if (!string.IsNullOrEmpty(tm.Srno) || !string.IsNullOrEmpty(tm.IMEI)
                        || !string.IsNullOrEmpty(tm.MOBILE))
                        tm.isMobiledetails = true;

                    tm.ExistingSimSerialNo = tm.Srno.Trim().ToUpper();
                    tm.ExistingSimImei = tm.IMEI.Trim().ToUpper();
                    tm.ExistingSimMobileNo = tm.MOBILE.Trim().ToUpper();


                    string FrontTyre2 = tm.FrontTyre2_srlno2;
                    tm.gbRT = "Rear Tyre " + Convert.ToString(dt.Rows[0]["REARTYRE_MAKE"]);
                    tm.gbFT = "Front Tyre " + Convert.ToString(dt.Rows[0]["FRONTTYRE_MAKE"]);
                    //if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FCODE_SRLNO"])))
                    //{
                    //    tm.lblTractorSrlno = "Tractor Srlno: " + Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]);
                    //    tm.lblTractorSrlnoisvisible = true;
                    //}
                    //else
                    //    tm.lblTractorSrlnoisvisible = false;
                    //if (fun.CheckExits("select count(*) from print_serial_number where serial_number='" + Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]) + "' and ORACLE='P'"))
                    //{
                    //    tm.label9 = "Caution -This serial no is already Transfered in Oracle.\nIf modify, Ensure to paste the new sticker on Tractor Route card";
                    //    tm.label9isvisible = true;
                    //}
                    //else
                    //    tm.label9isvisible = false;
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["BATTERY_MAKE"])))
                        tm.BatMake = Convert.ToString(dt.Rows[0]["BATTERY_MAKE"]);
                    else
                        tm.BatMake = "";
                    Fcode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);

                    TractorCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                    tm.TractorStagePrint = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                    tm.TractorAutoid = Convert.ToString(dt.Rows[0]["FCODE_ID"]);
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ROPS_SRNO"])))
                    {
                        tm.lblROPS = "ROPS SRNO: " + Convert.ToString(dt.Rows[0]["ROPS_SRNO"]);
                        tm.lblROPSisvisible = true;
                        tm.lblROPS = Convert.ToString(dt.Rows[0]["ROPS_SRNO"]);
                    }
                    else
                    {
                        tm.lblROPS = string.Empty;
                        tm.lblROPSisvisible = false;
                        tm.lblROPS = string.Empty;
                    }
                }
                else
                {
                    EngineDcode = Engine = string.Empty;
                    ROLLOUTSTICKER = FINALDATEHOOK = TractorHook = FCODEIDHOOK = string.Empty;
                    TRACTORTYPE = CAREBUTTONOIL = swapbtn = "";
                    query = "select ITEM_CODE from XXES_DAILY_PLAN_JOB J, XXES_DAILY_PLAN_TRAN T where J.FCODE_AUTOID=T.AUTOID and J.PLANT_CODE=T.PLANT_CODE and J.FAMILY_CODE=T.FAMILY_CODE and JOBID='" + obj.JOBID + "' and  J.plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and J.family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
                    Fcode = fun.get_Col_Value(query).Trim().ToUpper();
                    TractorCode = Fcode.Trim().ToUpper();
                    tm.TractorAutoid = tm.TractorStagePrint = string.Empty;
                    tm.lblROPSisvisible = false;
                }
                //txtScanJob.Enabled = false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }
            var myResult = new
            {
                Result = tm,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }



        public JsonResult PRINTDATA(RollDown down)
        {
            string msg = string.Empty; string TractorCode = string.Empty; string Fcode = string.Empty; string result = string.Empty;
            string swapbtn = string.Empty, CAREBUTTONOIL = string.Empty, TRACTORTYPE = string.Empty, ROLLOUTSTICKER = string.Empty,
            isEnableCarebutton = string.Empty;
            string TractorHook = "", FCODEIDHOOK = string.Empty, EngineDcode = string.Empty, Backend = string.Empty, Engine = string.Empty,
            FINALDATEHOOK, fipsrno = string.Empty, scanjob = string.Empty;
            var myResult = new
            {
                Result = result,
                Msg = msg
            };
            try
            {

                Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                RollDown rolldown = new RollDown();
                if (down.JOBID != null || down.PLANTCODE != null || down.FAMILYCODE != null)
                {
                    rolldown = assemblyfunctions.GetTractorDetails(down.JOBID.Trim(), down.PLANTCODE, down.FAMILYCODE,
                       "JOBID");
                    if (rolldown.JOBID == null)
                    {
                        msg = "JOBID Not Found";

                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "Please Select JOBID , Plant AND FamilyCode";

                    return Json(myResult, JsonRequestBehavior.AllowGet);
                }
                rolldown.STAGE_Code = down.STAGE_Code;
                rolldown.Quantity = down.Quantity;

                if (string.IsNullOrEmpty(rolldown.TractorType))
                {
                    //MessageBox.Show("Invalid Tractor Type. i.e EXPORT or DOMESTIC"); 
                    msg = "Invalid Tractor Type. i.e EXPORT or DOMESTIC";

                    return Json(myResult, JsonRequestBehavior.AllowGet);
                }
                string mode = "NETWORK", ip = string.Empty; int port = 0;

                if (rolldown.Quantity == null)
                {
                    rolldown.Quantity = "1";
                }
                //MessageBox.Show("PDI OK sticker printed successfully !! ");
                msg = af.PrintAssemblyStagesSticker(rolldown, Convert.ToInt32(rolldown.Quantity));
                myResult = new
                {
                    Result = result,
                    Msg = msg
                };
                return Json(myResult, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                myResult = new
                {
                    Result = result,
                    Msg = ex.Message
                };
            }
            finally { }

            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopup(RollDown data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER xss WHERE  XSS.PLANT_CODE='{0}'
                       AND XSS.FAMILY_CODE='{1}' AND OFFLINE_KEYCODE='COM' And AD_PASSWORD='{2}'",
                       data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper(), data.Password.Trim());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Invalid Password..!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(RollDown obj)
        {
            RollDown rollDown = new RollDown();
            string msg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(obj.PLANTCODE))
                {
                    msg = "Select Plant to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(obj.FAMILYCODE))
                {
                    msg = "Select family to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(obj.JOBID))
                {
                    msg = "Select item to continue.";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                rollDown = assemblyfunctions.GetTractorDetails(obj.JOBID.Trim(), obj.PLANTCODE, obj.FAMILYCODE,
                       "JOBID");
                string status = ValidateJobforEmpty(rollDown, obj);
                if (status != "OK")
                {
                    return Json(status, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);

        }
        public string ValidateJobforEmpty(RollDown tractormaster, RollDown updateTractor)
        {
            string foundjob = string.Empty,founddcode=string.Empty;
            try
            {
                bool check = false;
                Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                if (tractormaster.isEngineRequire && string.IsNullOrEmpty(updateTractor.Engine))
                {
                    return "Engine srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Engine))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                    if(!string.IsNullOrEmpty(foundjob))
                    {
                        return "Engine srlno already found on job " + foundjob;
                    }
                    founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine, "EN");
                    if(tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                    {
                        return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                    }
                }



                if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission))
                    return "Transmission srlno should not be blank ";
                if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel))
                    return "RearAxel srlno should not be blank ";
                if (tractormaster.isREQ_HYD_PUMP && string.IsNullOrEmpty(updateTractor.Hydraulic))
                    return "Hydraulic srlno should not be blank ";
                if (tractormaster.isREQUIRE_REARTYRE && string.IsNullOrEmpty(updateTractor.RearSrnn1))
                    return "RearSrnn1 srlno should not be blank ";
                if (tractormaster.isREQ_RHRT && string.IsNullOrEmpty(updateTractor.RearSrnn2))
                    return "RearSrnn2 srlno should not be blank ";
                if (tractormaster.isREQUIRE_FRONTTYRE && string.IsNullOrEmpty(updateTractor.FrontSrnn1))
                    return "FrontSrnn1 srlno should not be blank ";
                if (tractormaster.isREQ_RHFT && string.IsNullOrEmpty(updateTractor.FrontSrnn2))
                    return "FrontSrnn2 srlno should not be blank ";
                if (tractormaster.isREQ_REARRIM && string.IsNullOrEmpty(updateTractor.RearRIM1) && string.IsNullOrEmpty(updateTractor.RearRIM2))
                    return "Rear RIM1 And RIM2 srlno should not be blank ";
                if (tractormaster.isREQ_FRONTRIM && string.IsNullOrEmpty(updateTractor.FrontRIM1) && string.IsNullOrEmpty(updateTractor.FrontRIM2))
                    return "Front RIM1 And RIM2 should not be blank ";
                if (tractormaster.isREQUIRE_BATTERY && string.IsNullOrEmpty(updateTractor.Battery))
                    return "BATTERY srlno should not be blank ";
                if (tractormaster.isREQ_HYD_PUMP && string.IsNullOrEmpty(updateTractor.HydrualicPump))
                    return "Hydrualic Pump srlno should not be blank ";
                if (tractormaster.isREQ_RADIATOR && string.IsNullOrEmpty(updateTractor.Radiator))
                    return "Radiator srlno should not be blank ";
                if (tractormaster.isREQ_STEERING_MOTOR && string.IsNullOrEmpty(updateTractor.SteeringCylinder))
                    return "Steering Cylinder srlno should not be blank ";
                if (tractormaster.isREQ_STARTER_MOTOR && string.IsNullOrEmpty(updateTractor.SteeringMotor))
                    return "Steering Motor srlno should not be blank ";
                if (tractormaster.isREQ_STEERING_ASSEMBLY && string.IsNullOrEmpty(updateTractor.SteeringAssem))
                    return "STEERING ASSEMBLY srlno should not be blank ";
                if (tractormaster.isREQ_ALTERNATOR && string.IsNullOrEmpty(updateTractor.Alternator))
                    return "Alternator srlno should not be blank ";
                if (tractormaster.isREQ_CLUSSTER && string.IsNullOrEmpty(updateTractor.Cluster))
                    return "Cluster srlno should not be blank ";
                if (tractormaster.isREQ_STARTER_MOTOR && string.IsNullOrEmpty(updateTractor.Motor))
                    return "Motor srlno should not be blank ";
                if (tractormaster.isREQ_ROPS && string.IsNullOrEmpty(updateTractor.ROPSrno))
                    return "ROP srlno should not be blank ";
            }
            catch (Exception)
            {

                throw;
            }
            return "OK";
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