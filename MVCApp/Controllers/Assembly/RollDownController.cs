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
           isEnableCarebutton = string.Empty;string TractorCode = "";
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
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindStages(string plant,string family)
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
                if (!string.IsNullOrEmpty(plant)&& !string.IsNullOrEmpty(family))
                {
                    query = string.Format(@"SELECT stage_description description,offline_keycode code FROM xxes_stage_master
                        WHERE plant_code='{0}' AND family_code='{1}'
                        AND IPADDR IS NOT NULL and offline_keycode not in ('QUALITY')", plant.Trim().ToUpper(), family.Trim().ToUpper());
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

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PLANTCODE) || string.IsNullOrEmpty(data.FAMILYCODE))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PLANTCODE).Trim().ToUpper(), Convert.ToString(data.FAMILYCODE).Trim().ToUpper());

                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.ITEM_CODE))
                {
                    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        "where organization_id in (" + orgid + ") AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%') order by segment1", data.ITEM_CODE.Trim().ToUpper(), data.ITEM_CODE.Trim().ToUpper());
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
                if (string.IsNullOrEmpty(Convert.ToString(obj.ITEM_CODE.Trim())))
                {
                    msg = "Scan job to continue.";
                    var myResult1 = new
                    {
                        Result = tm,
                        Msg = msg
                    };
                    return Json(myResult1, JsonRequestBehavior.AllowGet);
                }
                query = @"select count(*) from XXES_JOB_STATUS where JOBID='" + obj.ITEM_CODE.Trim() + "' " +
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
                scanjob = obj.ITEM_CODE.Trim().ToUpper();
                //displayJobDetails(obj.ITEM_CODE.Trim().ToUpper());
                query = "select * from XXES_JOB_STATUS where JOBID='" + obj.ITEM_CODE.Trim() + "' and  plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    scanjob = obj.ITEM_CODE;
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
                    tm.Hydraulic= Convert.ToString(dt.Rows[0]["HYDRALUIC_SRLNO"]);
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
                    tm.Radiator  = Convert.ToString(dt.Rows[0]["RADIATOR_SRLNO"]);
                    //RaditorAssembly = txtJRadiator.Text;
                    tm.Cluster = Convert.ToString(dt.Rows[0]["CLUSSTER_SRLNO"]);
                    //clusterAssembly = txtJCLAssem.Text;
                    tm.Alternator = Convert.ToString(dt.Rows[0]["ALTERNATOR_SRLNO"]);
                    //Alternator = txtJAlternator.Text;
                    tm.Motor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR_SRLNO"]);
                    //StarterMotor = txtJSTRTMOTOR.Text;
                    tm.RearRIM1 = Convert.ToString(dt.Rows[0]["RTRIM1"]);
                    //Reartyre_Rim1 = txtRTRIM1.Text;
                    tm.RearRIM2  = Convert.ToString(dt.Rows[0]["RTRIM2"]);
                    //Reartyre_Rim2 = txtRTRIM2.Text;
                    tm.FrontRIM1  = Convert.ToString(dt.Rows[0]["FTRIM1"]);
                    //FrontTyre_Rim1 = txtFTRIM1.Text;
                    tm.FrontRIM2 = Convert.ToString(dt.Rows[0]["FTRIM2"]);
                    //FrontTyre_Rim2 = txtFTRIM2.Text;
                    tm.RearTyre1= Convert.ToString(dt.Rows[0]["RTTYRE1"]);
                    //Reartyre1 = txtRTTYRE1.Text;
                    tm.RearTyre2 = Convert.ToString(dt.Rows[0]["RTTYRE2"]);
                    //Reartyre2 = txtRTTYRE2.Text;
                    tm.FrontTyre1 = Convert.ToString(dt.Rows[0]["FTTYRE1"]);
                    //FrontTyre1 = txtFTTYRE1.Text;
                    tm.FrontTyre2 = Convert.ToString(dt.Rows[0]["FTTYRE2"]);
                    tm.Srno = Convert.ToString(dt.Rows[0]["SIM_SERIAL_NO"]);
                    tm.IMEI = Convert.ToString(dt.Rows[0]["IMEI_NO"]);
                    tm.MOBILE = Convert.ToString(dt.Rows[0]["MOBILE"]);
                    if (!string.IsNullOrEmpty(tm.Srno) || !string.IsNullOrEmpty(tm.IMEI)
                        || !string.IsNullOrEmpty(tm.MOBILE))
                        tm.isMobiledetails = true;

                    tm.ExistingSimSerialNo = tm.Srno.Trim().ToUpper();
                    tm.ExistingSimImei = tm.IMEI.Trim().ToUpper();
                    tm.ExistingSimMobileNo = tm.MOBILE.Trim().ToUpper();


                    string FrontTyre2 = tm.FrontTyre2;
                    tm.gbRT = "Rear Tyre " + Convert.ToString(dt.Rows[0]["REARTYRE_MAKE"]);
                    tm.gbFT = "Front Tyre " + Convert.ToString(dt.Rows[0]["FRONTTYRE_MAKE"]);
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["FCODE_SRLNO"])))
                    {
                        tm.lblTractorSrlno = "Tractor Srlno: " + Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]);
                        tm.lblTractorSrlnoisvisible = true;
                    }
                    else
                        tm.lblTractorSrlnoisvisible = false;
                    if (fun.CheckExits("select count(*) from print_serial_number where serial_number='" + Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]) + "' and ORACLE='P'"))
                    {
                        tm.label9= "Caution -This serial no is already Transfered in Oracle.\nIf modify, Ensure to paste the new sticker on Tractor Route card";
                        tm.label9isvisible = true;
                    }
                    else
                        tm.label9isvisible = false;
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
                    query = "select ITEM_CODE from XXES_DAILY_PLAN_JOB J, XXES_DAILY_PLAN_TRAN T where J.FCODE_AUTOID=T.AUTOID and J.PLANT_CODE=T.PLANT_CODE and J.FAMILY_CODE=T.FAMILY_CODE and JOBID='" + obj.ITEM_CODE + "' and  J.plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and J.family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
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

        public string PrintStageSticker(RollDown down)
        {
            try
            {
                PrinterSettings settings = new PrinterSettings();
                bool isTransRequire, isRearAxelRequire, isEngineRequire, isSrNoRequire, isHydraulicRequire, isRearTyreRequire, isFrontTyreRequire, isBatRequire, isPumpRequire,
                isStMotorRequire, isStAssemblyRequire, isStCylinderRequre, isRaditorRequire, isClusterRequire, isAlternatorRequire, isStartMotorRequire, isBackendRequire;
                isTransRequire = isRearAxelRequire = isEngineRequire = isSrNoRequire = isHydraulicRequire = isRearTyreRequire = isFrontTyreRequire = isBatRequire = isPumpRequire =
                isStMotorRequire = isStAssemblyRequire = isStCylinderRequre = isRaditorRequire = isClusterRequire = isAlternatorRequire = isBackendRequire = isStartMotorRequire = false;
                string TractorDesc = string.Empty, SelectedJob = string.Empty, TractorType = string.Empty, Filename = string.Empty, TractorSrno = string.Empty, HydrualicDcode = string.Empty, Suffix = string.Empty, itemname1 = string.Empty, itemname2 = string.Empty, HydraulicDesc = string.Empty;
                query = "select * from XXES_ITEM_MASTER where ITEM_CODE='" + down.TractorStagePrint.Trim().ToUpper() + "' and plant_code='" + Convert.ToString(down.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(down.FAMILYCODE).Trim().ToUpper() + "'";
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    TractorDesc = Convert.ToString(dt.Rows[0]["ITEM_DESCRIPTION"]); isTransRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y" ? true : false);
                    isRearAxelRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y" ? true : false); isEngineRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y" ? true : false);
                    isSrNoRequire = (Convert.ToString(dt.Rows[0]["GEN_SRNO"]) == "Y" ? true : false); isHydraulicRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y" ? true : false);
                    isRearTyreRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y" ? true : false); isFrontTyreRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y" ? true : false);
                    isBatRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_BATTERY"]) == "Y" ? true : false); isPumpRequire = (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y" ? true : false);
                    isStMotorRequire = (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y" ? true : false); isStAssemblyRequire = (Convert.ToString(dt.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y" ? true : false);
                    isStCylinderRequre = (Convert.ToString(dt.Rows[0]["REQ_STERING_CYLINDER"]) == "Y" ? true : false); isRaditorRequire = (Convert.ToString(dt.Rows[0]["REQ_RADIATOR"]) == "Y" ? true : false);
                    isClusterRequire = (Convert.ToString(dt.Rows[0]["REQ_CLUSSTER"]) == "Y" ? true : false); isAlternatorRequire = (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y" ? true : false);
                    isStartMotorRequire = (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y" ? true : false);
                    isBackendRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y" ? true : false);

                    HydrualicDcode = Convert.ToString(dt.Rows[0]["HYDRAULIC"]);
                    HydraulicDesc = Convert.ToString(dt.Rows[0]["HYDRAULIC_DESCRIPTION"]);
                }
                dt.Dispose();
                if (down.lblTractorSrlnoisvisible==true)
                    TractorSrno = ((down.lblTractorSrlno.IndexOf(':') > 0) ? down.lblTractorSrlno.Split(':')[1].Trim() : "");
                //PrintDialog pd = new PrintDialog();
                SelectedJob = Convert.ToString(down.ITEM_CODE).ToUpper();
                itemname1 = itemname2 = "";
                if (!string.IsNullOrEmpty(TractorDesc))
                    af.getNameSubAssembly(TractorDesc.Trim().ToUpper(), ref itemname1, ref itemname2);
                TractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code ='" + down.TractorStagePrint.Trim() + "' and autoid='" + down.TractorAutoid + "' and plant_code='" + Convert.ToString(down.PLANTCODE).Trim() + "' and family_code='" + Convert.ToString(down.FAMILYCODE).Trim() + "'");
                if (string.IsNullOrEmpty(TractorType))
                {
                    //MessageBox.Show("Invalid Tractor Type. i.e EXPORT or DOMESTIC"); 
                    return "Invalid Tractor Type. i.e EXPORT or DOMESTIC";
                }
                 if (down.STAGE_Code == "PDIOK")
                {

                    string mode = "LOCAL", ip = string.Empty; int port = 0;
                    if (down.chkPrint==true)
                    {
                        string line = Convert.ToString(fun.get_Col_Value(@"select IPADDR || '#' || IPPORT  as IP 
                        from xxes_stage_master where FAMILY_CODE='" + Convert.ToString(down.FAMILYCODE).Trim() + "' and " +
                         "plant_code='" + Convert.ToString(down.PLANTCODE).Trim() + "' and offline_keycode='" + Convert.ToString(down.STAGE_Code) + "'"));
                        ip = line.Split('#')[0].Trim();
                        port = Convert.ToInt32(line.Split('#')[1].Trim());
                        mode = "NETWORK";
                    }
                    PrintLabel printLabel = new PrintLabel();
                    Tractor tractor = new Tractor();
                    tractor.PLANT = Convert.ToString(down.PLANTCODE);
                    tractor.FAMILY = Convert.ToString(down.FAMILYCODE);
                    tractor.JOB = Convert.ToString(down.ITEM_CODE);
                    tractor.ITEMCODE = down.TractorStagePrint;
                    tractor.TSN = down.ITEM_CODE.Trim().Split('#')[0].Trim();
                    af.RecordPDIOK(tractor)
;

                    isEnableCarebutton = string.Empty;
                    query = string.Format("select req_CAREBTN from xxes_item_master where item_code='{0}'", down.TractorStagePrint.Trim());
                    isEnableCarebutton = fun.get_Col_Value(query);

                    string pdiline = string.Empty, avgHours = string.Empty;
                    query = string.Format(@"select to_char( FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
                    '#' || to_char( PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
                    '#' || to_char( CAREBUTTONOIL, 'dd-Mon-yyyy HH24:MI:SS' )  from XXES_JOB_STATUS where 
                        JOBID='{0}' and  plant_code='{1}' and family_code='{2}'",
                        Convert.ToString(down.ITEM_CODE), Convert.ToString(down.PLANTCODE).Trim().ToUpper(),
                     Convert.ToString(down.FAMILYCODE).Trim().ToUpper());
                    pdiline = fun.get_Col_Value(query);
                    string finaldate = string.Empty, pdidate = string.Empty, CAREBUTTONOIL = string.Empty;
                    if (!string.IsNullOrEmpty(pdiline))
                    {
                        finaldate = pdiline.Split('#')[0].Trim();
                        pdidate = pdiline.Split('#')[1].Trim();
                        CAREBUTTONOIL = pdiline.Split('#')[2].Trim();
                        if (isEnableCarebutton == "Y")
                        {
                            if (string.IsNullOrEmpty(CAREBUTTONOIL))
                            {
                                //MessageBox.Show("CARE BUTTON NOT SCANNED AT PDI STAGE", PubFun.AppName);
                                return "CARE BUTTON NOT SCANNED AT PDI STAGE";
                            }
                            if (!string.IsNullOrEmpty(finaldate) && !string.IsNullOrEmpty(CAREBUTTONOIL))
                            {
                                TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(CAREBUTTONOIL);
                                span = new TimeSpan(Math.Abs(span.Ticks));
                                avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
                            }
                            tractor.avgHours = avgHours;
                            tractor.pdidate = CAREBUTTONOIL;
                        }
                        else
                        {
                            TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(pdidate);
                            span = new TimeSpan(Math.Abs(span.Ticks));
                            avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
                            tractor.avgHours = avgHours;
                            tractor.pdidate = pdidate;
                        }

                    }
                    //tractor.avgHours = avgHours;
                    //tractor.pdidate = CAREBUTTONOIL;
                    //{
                    if (printLabel.PrintPDIOK(tractor, 1, mode, ip, port,down.STAGE_Code))
                    {
                        //MessageBox.Show("PDI OK sticker printed successfully !! ");
                        return "PDI OK sticker printed successfully !! ";
                    }
                    //}

                }
                if (down.STAGE_Code == "BK") //FT buckleup
                {
                    query = af.ReadFile("BK.txt");
                    if (!string.IsNullOrEmpty(query))
                    {
                        query = query.Replace("JOB_VAL", Convert.ToString(down.ITEM_CODE).ToUpper());
                        query = query.Replace("ITEM_NAME1", itemname1.Trim()); query = query.Replace("ITEM_NAME2", itemname2.Trim());
                        query = query.Replace("FCODE_VAL", down.TractorStagePrint);
                        if (isTransRequire == false) query = query.Replace("TRANS_VAL", "NA");
                        else query = query.Replace("TRANS_VAL", down.Transmission.Trim().ToUpper());
                        if (isRearAxelRequire == false) query = query.Replace("REAR_VAL", "NA");
                        else query = query.Replace("REAR_VAL", down.RearAxel.Trim().ToUpper());
                    }
                }
                else if (down.STAGE_Code == "EN" || down.STAGE_Code == "BA") //FT egnine or backend
                {
                    string PrintMMYYFormat = fun.get_Col_Value("select ONLINE_SCREEN  from " +
                        " XXES_Stage_Master where plant_code='" + Convert.ToString(down.PLANTCODE).Trim() + "' and family_code='" + Convert.ToString(down.FAMILYCODE).Trim() + "' and OFFLINE_KEYCODE='" + down.STAGE_Code + "'");

                    if (PrintMMYYFormat.Trim() == "1")
                    {
                        string EnMisc = fun.get_Col_Value("select to_char(scan_date,'MON-YYYY') scan_date" +
                            " from XXES_SCAN_TIME where jobid='" + SelectedJob.Trim() + "' and ITEM_CODE='" + down.TractorStagePrint.Trim().ToUpper() + "' and stage='EN' and PLANT_CODE='" + Convert.ToString(down.PLANTCODE).ToUpper() + "' and FAMILY_CODE='" + Convert.ToString(down.FAMILYCODE).ToUpper() + "' and rownum=1");
                        if (string.IsNullOrEmpty(EnMisc))
                        {
                            //MessageBox.Show("Record not found when the engine assembled on this job : " + SelectedJob);
                            return "Record not found when the engine assembled on this job : " + SelectedJob;
                        }
                        string querynew = string.Format(@"select prefix_4 from xxes_item_master where item_code='{0}' and plant_code='{1}'",
                                        down.TractorStagePrint.Trim().ToUpper(), Convert.ToString(down.PLANTCODE));
                        Suffix = fun.get_Col_Value(querynew);
                        if (string.IsNullOrEmpty(Suffix))
                            Suffix = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                            MON_YYYY='" + EnMisc.ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + Convert.ToString(down.PLANTCODE) + "'");
                    }

                    if (TractorType.ToUpper().Trim() == "EXPORT")
                    {
                        if (down.STAGE_Code == "BA")
                            Filename = "BD17.txt";
                        else
                            Filename = "EN17.txt";
                    }
                    else
                    {
                        if (down.STAGE_Code == "EN")
                            Filename = "EN.txt";
                        else
                            Filename = "BD.txt";
                    }
                    query = af.ReadFile(Filename);
                    if (!string.IsNullOrEmpty(query))
                    {
                        query = query.Replace("JOB_VAL", SelectedJob);
                        query = query.Replace("ITEM_NAME1", itemname1.Trim());
                        query = query.Replace("ITEM_NAME2", itemname2.Trim());
                        query = query.Replace("FCODE_VAL", down.TractorStagePrint.Trim().ToUpper());
                        query = query.Replace("SERIES_NO", TractorSrno.Trim().ToUpper());
                        if (isEngineRequire == false)
                            query = query.Replace("ENGG_VAL", "NA");
                        else
                            //down.Engine = "E2498927";
                            query = query.Replace("ENGG_VAL", down.Engine.Trim().ToUpper());
                        if (down.STAGE_Code == "EN")
                        {
                            if (!isBackendRequire)
                            {
                                query = query.Replace("TRANS_VAL", down.Transmission.Trim().ToUpper());
                                query = query.Replace("REAR_VAL", down.RearAxel.Trim().ToUpper());
                            }
                            else
                            {
                                query = query.Replace("TRANS_VAL", down.Backend.Trim().ToUpper());
                                query = query.Replace("REAR_VAL", "");
                            }
                        }
                        else
                        {
                            query = query.Replace("BACKEND_VAL", down.Backend.Trim().ToUpper());
                        }
                        //query = query.Replace("ROPS_SRNO", down.ROPSrno.Trim());
                        if (!string.IsNullOrEmpty(TractorSrno))
                        {
                            if (TractorSrno.Trim().Length == 17)
                                query = query.Replace("MONTH", TractorSrno.Trim().ToUpper().Substring(8, 2));
                            else
                                if (TractorType.ToUpper().Trim() == "DOMESTIC" && PrintMMYYFormat == "1")
                            {
                                query = query.Replace("MONTH", Suffix.Trim());
                            }
                            else
                                query = query.Replace("MONTH", TractorSrno.Trim().ToUpper().Substring(TractorSrno.Trim().Length - 2, 2));
                        }
                        else
                            query = query.Replace("MONTH", "");
                    }
                }
                else if (down.STAGE_Code == "HYD") //Hydrualic
                {
                    itemname1 = itemname2 = string.Empty;
                    query = af.ReadFile("BK.txt");
                    if (!string.IsNullOrEmpty(query))
                    {
                        itemname1 = HydraulicDesc;
                        itemname2 = "";
                        if (itemname1.Length > 50)
                        {
                            itemname1 = itemname1.Trim().Substring(0, 25);
                            itemname2 = HydraulicDesc.Split('#')[1].Trim().Substring(25, HydraulicDesc.Trim().Length - 25);
                            if (itemname2.Trim().Length > 25)
                                itemname2 = itemname2.Substring(0, 24);
                        }
                        else if (itemname1.Length > 25)
                        {
                            itemname1 = itemname1.Trim().Substring(0, 25);
                            itemname2 = HydraulicDesc.Split('#')[1].Trim().Trim().Substring(25, HydraulicDesc.Trim().Length - 25);
                        }
                        query = query.Replace("SERIES_NO", down.Hydraulic.Trim().ToUpper());
                        query = query.Replace("ITEM_NAME1", itemname1.Trim());
                        query = query.Replace("ITEM_NAME2", itemname2.Trim());
                        query = query.Replace("DCODE_VAL", Convert.ToString(HydrualicDcode));
                    }
                }
                else if (down.STAGE_Code == "RT") //Rear Tyre
                {

                }
                else if (down.STAGE_Code == "FT") //Front Tyre
                {

                }
                if (down.chkPrint == true)
                {
                    string ip = string.Empty, port = string.Empty, line = string.Empty;
                    if (down.STAGE_Code == "BA")
                        line = Convert.ToString(fun.get_Col_Value("select IPADDR || '#' || IPPORT  as IP from xxes_stage_master where FAMILY_CODE='" + Convert.ToString(down.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(down.PLANTCODE).Trim() + "' and offline_keycode='EN'"));
                    else
                        line = Convert.ToString(fun.get_Col_Value("select IPADDR || '#' || IPPORT  as IP from xxes_stage_master where FAMILY_CODE='" + Convert.ToString(down.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(down.PLANTCODE).Trim() + "' and offline_keycode='" + down.STAGE_Code + "'"));
                    if (!string.IsNullOrEmpty(line))
                    {
                        ip = line.Split('#')[0].Trim();
                        port = line.Split('#')[1].Trim();
                        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
                        {
                            //MessageBox.Show("Invalid Ip address or port");
                            return "Invalid Ip address or port";
                        }
                        if (down.STAGE_Code == "BA" || down.STAGE_Code == "EN")
                            af.PrintLabelViaNetwork(query, query, ip, Convert.ToInt16(port));
                        else
                            af.PrintLabelViaNetwork(query, "", ip, Convert.ToInt16(port));
                    }

                }
                else
                {
                    af.WriteDataToLabelFile(query);

                    if (!string.IsNullOrEmpty(query))
                    {
                        bool status = af.PrintStandardLabel(query, down.STAGE_Code, down.PLANTCODE.Trim(), down.FAMILYCODE.Trim());
                        if (status == true)
                        {
                            return "Barcode Printed Sucessfully !!";
                        }
                        else
                        {
                            return "Error found while printing barcode";
                        }
                    }
                   
                    //if (SendtoPrinter.SendFileToPrinter(settings.PrinterName, System.IO.Directory.GetCurrentDirectory().ToString() + "\\Label"))
                    //{
                    //    af.Record_Reprint(down.TractorStagePrint, "", SelectedJob,down.PLANTCODE,down.FAMILYCODE);
                    //    if (down.STAGE_Code == "EN" || down.STAGE_Code == "BA")
                    //        SendtoPrinter.SendFileToPrinter(settings.PrinterName, System.IO.Directory.GetCurrentDirectory().ToString() + "\\Label");
                    //    //lblMsgPrint.Visible = true;
                    //    //lblMsgPrint.Text = "Barcode Printed Sucessfully !!";
                        
                    //}
                    else
                    {
                        //lblMsgPrint.Visible = true;
                        //lblMsgPrint.Text = "Error found while printing barcode";
                        return "Error found while printing barcode";
                    }
                }
                return "";
            }
            catch
            {
                throw;
            }
            finally
            {

            }

        }
      
        public JsonResult PRINTDATA(RollDown obj)
        {
            string msg = string.Empty; string TractorCode = string.Empty; string Fcode = string.Empty;string result=string.Empty;
            string swapbtn = string.Empty, CAREBUTTONOIL = string.Empty, TRACTORTYPE = string.Empty, ROLLOUTSTICKER = string.Empty,
            isEnableCarebutton = string.Empty;
            string TractorHook = "", FCODEIDHOOK = string.Empty, EngineDcode = string.Empty, Backend = string.Empty, Engine = string.Empty,
            FINALDATEHOOK, fipsrno = string.Empty, scanjob = string.Empty;
            try 
            {
                msg = PrintStageSticker(obj);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }
            var myResult = new
            {
                Result = result,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        private void PrintSticker(RollDown option)
        {
            try
            {
                if (string.IsNullOrEmpty(option.TractorStagePrint))
                {
                    //MessageBox.Show("Selected job yet to be buckleup");
                    return;
                }
                else if (string.IsNullOrEmpty(Convert.ToString(option.ITEM_CODE)))
                {
                    //MessageBox.Show("select valid job");
                    return;
                }
                else if (string.IsNullOrEmpty(Convert.ToString(option.PLANTCODE)))
                {
                    //MessageBox.Show("select valid plant");
                    return;
                }
                else if (string.IsNullOrEmpty(Convert.ToString(option.FAMILYCODE)))
                {
                    //MessageBox.Show("select valid family");
                    return;
                }
                if (option.STAGE_Code == "ROlldown")
                    PrintStageSticker(option);
                else if (option.STAGE_Code == "PDI")
                {

                    string mode = "LOCAL", ip = string.Empty; int port = 0;
                    if (option.chkPrint==true)
                    {
                        string line = Convert.ToString(fun.get_Col_Value(@"select IPADDR || '#' || IPPORT  as IP 
                        from xxes_stage_master where FAMILY_CODE='" + Convert.ToString(option.FAMILYCODE).Trim() + "' and " +
                         "plant_code='" + Convert.ToString(option.PLANTCODE).Trim() + "' and offline_keycode='" + Convert.ToString(option.STAGE_Code) + "'"));
                        ip = line.Split('#')[0].Trim();
                        port = Convert.ToInt32(line.Split('#')[1].Trim());
                        mode = "NETWORK";
                    }
                    PrintLabel printLabel = new PrintLabel();
                    Tractor tractor = new Tractor();
                    tractor.PLANT = Convert.ToString(option.PLANTCODE);
                    tractor.FAMILY = Convert.ToString(option.FAMILYCODE);
                    tractor.JOB = Convert.ToString(option.ITEM_CODE);
                    tractor.ITEMCODE = option.TractorStagePrint;
                    tractor.TSN = option.ITEM_CODE.Trim().Split('#')[0].Trim();
                    jobFinalFunction.RecordPDIOK(tractor);

                    isEnableCarebutton = string.Empty;
                    query = string.Format("select req_CAREBTN from xxes_item_master where item_code='{0}'", TractorCode.Trim());
                    isEnableCarebutton = fun.get_Col_Value(query);

                    string pdiline = string.Empty, avgHours = string.Empty;
                    query = string.Format(@"select to_char( FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
                    '#' || to_char( PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
                    '#' || to_char( CAREBUTTONOIL, 'dd-Mon-yyyy HH24:MI:SS' )  from XXES_JOB_STATUS where 
                        JOBID='{0}' and  plant_code='{1}' and family_code='{2}'",
                        Convert.ToString(option.ITEM_CODE), Convert.ToString(option.PLANTCODE).Trim().ToUpper(),
                     Convert.ToString(option.FAMILYCODE).Trim().ToUpper());
                    pdiline = fun.get_Col_Value(query);
                    string finaldate = string.Empty, pdidate = string.Empty, CAREBUTTONOIL = string.Empty;
                    if (!string.IsNullOrEmpty(pdiline))
                    {
                        finaldate = pdiline.Split('#')[0].Trim();
                        pdidate = pdiline.Split('#')[1].Trim();
                        CAREBUTTONOIL = pdiline.Split('#')[2].Trim();
                        if (isEnableCarebutton == "Y")
                        {
                            if (string.IsNullOrEmpty(CAREBUTTONOIL))
                            {
                                //MessageBox.Show("CARE BUTTON NOT SCANNED AT PDI STAGE", PubFun.AppName);
                                return;
                            }
                            if (!string.IsNullOrEmpty(finaldate) && !string.IsNullOrEmpty(CAREBUTTONOIL))
                            {
                                TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(CAREBUTTONOIL);
                                span = new TimeSpan(Math.Abs(span.Ticks));
                                avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
                            }
                            tractor.avgHours = avgHours;
                            tractor.pdidate = CAREBUTTONOIL;
                        }
                        else
                        {
                            TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(pdidate);
                            span = new TimeSpan(Math.Abs(span.Ticks));
                            avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
                            tractor.avgHours = avgHours;
                            tractor.pdidate = pdidate;
                        }

                    }
                    //tractor.avgHours = avgHours;
                    //tractor.pdidate = CAREBUTTONOIL;
                    //{
                    if (printLabel.PrintPDIOK(tractor, 1, mode, ip, port,option.STAGE_Code))
                    {
                        //MessageBox.Show("PDI OK sticker printed successfully !! ");
                    }
                    //}

                }
            }
            catch
            {
                throw;
            }
            finally { }
        }

    }
}