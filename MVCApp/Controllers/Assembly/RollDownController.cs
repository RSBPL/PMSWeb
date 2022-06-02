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
                result = fun.Fill_All_Family(Plant);
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
                    query = string.Format(@"SELECT SM.stage_description description,SM.offline_keycode code FROM xxes_stage_master SM 
                                    inner join xxes_users_master UM ON SM.STAGE_ID=UM.STAGEID
                                    WHERE SM.PLANT_CODE='{0}' AND SM.family_code='{1}' AND UM.USRNAME='{2}' AND IPADDR IS NOT NULL and offline_keycode not in ('QUALITY')", plant.Trim().ToUpper(), family.Trim().ToUpper(), fun.getUSERNAME());
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
                    query = string.Format(@"select ITEM_DESCRIPTION || ' # ' || '(' || ITEM_CODE || ')' || ' # ' || ' TSN : '|| ' # ' ||  FCODE_SRLNO || ' # ' || ' JOB :' || ' # ' ||  JOBID DESCRIPTION,JOBID  FROM XXES_JOB_STATUS" +
                        " where JOBID LIKE '%{0}%'  Or FCODE_SRLNO LIKE '%{0}%' AND PLANT_CODE='{1}' And FAMILY_CODE='{2}' order by JOBID", data.JOBID.Trim().ToUpper(), data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper());
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
                query = @"select count(*) from XXES_JOB_STATUS where JOBID='" + obj.JOBID.Trim() + "' " +
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
                query = "select * from XXES_JOB_STATUS where JOBID='" + obj.JOBID.Trim() + "' and  plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
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
                    rolldown = assemblyfunctions.GetTractorDetails(down.JOBID, down.PLANTCODE, down.FAMILYCODE,
                        "JOBID");
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
                if (af.PrintAssemblyStagesSticker(rolldown, Convert.ToInt32(rolldown.Quantity)))
                {
                    //MessageBox.Show("PDI OK sticker printed successfully !! ");
                    msg = "BuckleUp sticker printed successfully !! ";

                    return Json(myResult, JsonRequestBehavior.AllowGet);
                }
                msg = "";

                return Json(myResult, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { }

            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        //private void PrintSticker(RollDown option)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(option.TractorStagePrint))
        //        {
        //            //MessageBox.Show("Selected job yet to be buckleup");
        //            return;
        //        }
        //        else if (string.IsNullOrEmpty(Convert.ToString(option.JOBID)))
        //        {
        //            //MessageBox.Show("select valid job");
        //            return;
        //        }
        //        else if (string.IsNullOrEmpty(Convert.ToString(option.PLANTCODE)))
        //        {
        //            //MessageBox.Show("select valid plant");
        //            return;
        //        }
        //        else if (string.IsNullOrEmpty(Convert.ToString(option.FAMILYCODE)))
        //        {
        //            //MessageBox.Show("select valid family");
        //            return;
        //        }
        //        if (option.STAGE_Code == "ROlldown")
        //            PrintStageSticker(option);
        //        else if (option.STAGE_Code == "PDI")
        //        {

        //            string mode = "LOCAL", ip = string.Empty; int port = 0;
        //            if (option.chkPrint == true)
        //            {
        //                string line = Convert.ToString(fun.get_Col_Value(@"select IPADDR || '#' || IPPORT  as IP 
        //                from xxes_stage_master where FAMILY_CODE='" + Convert.ToString(option.FAMILYCODE).Trim() + "' and " +
        //                 "plant_code='" + Convert.ToString(option.PLANTCODE).Trim() + "' and offline_keycode='" + Convert.ToString(option.STAGE_Code) + "'"));
        //                ip = line.Split('#')[0].Trim();
        //                port = Convert.ToInt32(line.Split('#')[1].Trim());
        //                mode = "NETWORK";
        //            }
        //            PrintLabel printLabel = new PrintLabel();
        //            Tractor tractor = new Tractor();
        //            tractor.PLANT = Convert.ToString(option.PLANTCODE);
        //            tractor.FAMILY = Convert.ToString(option.FAMILYCODE);
        //            tractor.JOB = Convert.ToString(option.JOBID);
        //            tractor.ITEMCODE = option.TractorStagePrint;
        //            tractor.TSN = option.JOBID.Trim().Split('#')[4].Trim();
        //            jobFinalFunction.RecordPDIOK(tractor);

        //            isEnableCarebutton = string.Empty;
        //            query = string.Format("select req_CAREBTN from xxes_item_master where item_code='{0}'", TractorCode.Trim());
        //            isEnableCarebutton = fun.get_Col_Value(query);

        //            string pdiline = string.Empty, avgHours = string.Empty;
        //            query = string.Format(@"select to_char( FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
        //            '#' || to_char( PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS' ) ||
        //            '#' || to_char( CAREBUTTONOIL, 'dd-Mon-yyyy HH24:MI:SS' )  from XXES_JOB_STATUS where 
        //                JOBID='{0}' and  plant_code='{1}' and family_code='{2}'",
        //                Convert.ToString(option.JOBID), Convert.ToString(option.PLANTCODE).Trim().ToUpper(),
        //             Convert.ToString(option.FAMILYCODE).Trim().ToUpper());
        //            pdiline = fun.get_Col_Value(query);
        //            string finaldate = string.Empty, pdidate = string.Empty, CAREBUTTONOIL = string.Empty;
        //            if (!string.IsNullOrEmpty(pdiline))
        //            {
        //                finaldate = pdiline.Split('#')[0].Trim();
        //                pdidate = pdiline.Split('#')[1].Trim();
        //                CAREBUTTONOIL = pdiline.Split('#')[2].Trim();
        //                if (isEnableCarebutton == "Y")
        //                {
        //                    if (string.IsNullOrEmpty(CAREBUTTONOIL))
        //                    {
        //                        //MessageBox.Show("CARE BUTTON NOT SCANNED AT PDI STAGE", PubFun.AppName);
        //                        return;
        //                    }
        //                    if (!string.IsNullOrEmpty(finaldate) && !string.IsNullOrEmpty(CAREBUTTONOIL))
        //                    {
        //                        TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(CAREBUTTONOIL);
        //                        span = new TimeSpan(Math.Abs(span.Ticks));
        //                        avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
        //                    }
        //                    tractor.avgHours = avgHours;
        //                    tractor.pdidate = CAREBUTTONOIL;
        //                }
        //                else
        //                {
        //                    TimeSpan span = Convert.ToDateTime(finaldate) - Convert.ToDateTime(pdidate);
        //                    span = new TimeSpan(Math.Abs(span.Ticks));
        //                    avgHours = (int)span.TotalHours + span.ToString(@"\:mm\:ss");
        //                    tractor.avgHours = avgHours;
        //                    tractor.pdidate = pdidate;
        //                }

        //            }
        //            //tractor.avgHours = avgHours;
        //            //tractor.pdidate = CAREBUTTONOIL;
        //            //{
        //            if (printLabel.PrintPDIOK(tractor, 1, mode, ip, port, option.STAGE_Code))
        //            {
        //                //MessageBox.Show("PDI OK sticker printed successfully !! ");
        //            }
        //            //}

        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally { }
        //}

    }
}