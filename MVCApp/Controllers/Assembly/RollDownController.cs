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
using MVCApp.Controllers.DCU;

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

        [HttpGet]
        public JsonResult BatteryName()
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                result = assemblyfunctions.Battery_Name();

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindIPAddress(string STAGECode)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(STAGECode))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(STAGECode))
                {
                    result = fun.IpAdress(STAGECode);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

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
                    if (data.JOBID.Contains("F1") || data.JOBID.Contains("F2"))
                    {
                        query = string.Format(@"SELECT DATA.DESCRIPTION , DATA.JOBID FROM (select FCODE_SRLNO || ' # '|| ITEM_CODE || '(' || substr(ITEM_DESCRIPTION,1,25) || ')' || ' # ' || ' JOB :' || ' # ' || 
                             JOBID DESCRIPTION,JOBID,ROW_NUMBER() OVER (ORDER BY JOBID) as ROWNMBER   FROM XXES_JOB_STATUS" +
                     " where JOBID LIKE '%{0}%'  Or ITEM_CODE LIKE '%{0}%' AND PLANT_CODE='{1}' And FAMILY_CODE='{2}' order by JOBID) Data WHERE DATA.ROWNMBER < 50", data.JOBID.Trim().ToUpper(), data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper());
                        //   query = string.Format(@"select FCODE_SRLNO || ' # '|| ITEM_CODE || '(' || substr(ITEM_DESCRIPTION,1,25) || ')' || ' # ' || ' JOB :' || ' # ' || 
                        //        JOBID DESCRIPTION,JOBID  FROM XXES_JOB_STATUS" +
                        //" where JOBID LIKE '%{0}%'  Or ITEM_CODE LIKE '%{0}%' AND PLANT_CODE='{1}' And FAMILY_CODE='{2}' order by JOBID", data.JOBID.Trim().ToUpper(), data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper());
                        //   //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());

                    }
                    else
                    {
                        query = string.Format(@"select FCODE_SRLNO || ' # '|| ITEM_CODE || '(' || substr(ITEM_DESCRIPTION,1,25) || ')' || ' # ' || ' JOB :' || ' # ' || 
                             JOBID DESCRIPTION,JOBID  FROM XXES_JOB_STATUS" +
                            " where JOBID LIKE '%{0}%'  Or FCODE_SRLNO LIKE '%{0}%' Or ITEM_CODE LIKE '%{0}%' AND PLANT_CODE='{1}' And FAMILY_CODE='{2}' order by JOBID", data.JOBID.Trim().ToUpper(), data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper());
                        //    query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                        //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '{0}%' OR DESCRIPTION LIKE '{1}%') order by segment1", data.SubAssembly1.Trim().ToUpper(), data.SubAssembly1.Trim().ToUpper());
                    }
                }

                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable().Take(20))
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
        public JsonResult FillReplaceJob(RollDown data)
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
                query = string.Format(@"select T.ITEM_CODE || '(' ||  SUBSTR(T.ITEM_DESC,0,30) || ')' || ' JOB: ' || J.Jobid  as DESCRIPTION,
                                       JOBID as JOBID from XXES_DAILY_PLAN_JOB J , XXES_DAILY_PLAN_TRAN T
                                        where  J.plant_code='" + Convert.ToString(data.PLANTCODE).Trim().ToUpper() + "' and J.family_code='" + Convert.ToString(data.FAMILYCODE).Trim().ToUpper() + "' AND J.PLANT_CODE=T.PLANT_CODE AND J.FAMILY_CODE=T.FAMILY_CODE and J.FCODE_AUTOID=T.AUTOID and t.plan_id in (select plan_id from xxes_daily_plan_master where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + data.Date + "' and plant_code='" + Convert.ToString(data.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(data.FAMILYCODE).Trim().ToUpper() + "') and j.jobid not in (select jobid from xxes_job_status) order by J.JOBID");
         
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable().Take(20))
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
                query = "select * from XXES_JOB_STATUS where JOBID='" + obj.JOBID.Trim() + "' or  FCODE_SRLNO='" + obj.TractorSrlno.Trim() + "' or  ITEM_CODE='" + obj.JOBID.Trim() + "' and  plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
                dt = new DataTable();
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    tm = null;
                    Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                    tm = assemblyfunctions.GetTractorDetails(obj.JOBID.Trim(), obj.PLANTCODE, obj.FAMILYCODE,
                               "JOBID");
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
                bool bypass = down.isBypass;
                RollDown rolldown = new RollDown();
                if (down.JOBID != null || down.PLANTCODE != null || down.FAMILYCODE != null)
                {
                    Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                    rolldown = assemblyfunctions.GetTractorDetails(down.JOBID.Trim(), down.PLANTCODE, down.FAMILYCODE,
                       "JOBID");
                    if (rolldown.JOBID == null)
                    {
                        msg = "JOBID Not Found";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "Please Select JOBID , Plant AND FamilyCode";
                    myResult = new
                    {
                        Result = result,
                        Msg = msg
                    };
                    return Json(myResult, JsonRequestBehavior.AllowGet);
                }
                rolldown.STAGE_Code = down.STAGE_Code;
                rolldown.Quantity = down.Quantity;
                rolldown.IPAddress = down.IPAddress;
                if (rolldown.Battery_srlno == null)
                {
                    rolldown.Battery_srlno = down.Battery_srlno;
                }
                if (rolldown.RearTyre1_srlno1 == null)
                {
                    rolldown.RearTyre1_srlno1 = down.RearTyre1_srlno1;
                }
                if (rolldown.RearTyre2_srlno2 == null)
                {
                    rolldown.RearTyre2_srlno2 = down.RearTyre2_srlno2;
                }
                if (rolldown.FrontTyre1_srlno1 == null)
                {
                    rolldown.FrontTyre1_srlno1 = down.FrontTyre1_srlno1;
                }
                if (rolldown.FrontTyre2_srlno2 == null)
                {
                    rolldown.FrontTyre2_srlno2 = down.FrontTyre2_srlno2;
                }
                if (string.IsNullOrEmpty(down.reartyremake))
                {
                    down.reartyremake = rolldown.reartyremake;
                }
                if (string.IsNullOrEmpty(down.fronttyremake))
                {
                    down.fronttyremake = rolldown.fronttyremake;
                }

                if (string.IsNullOrEmpty(rolldown.TractorType))
                {
                    //MessageBox.Show("Invalid Tractor Type. i.e EXPORT or DOMESTIC"); 
                    msg = "Invalid Tractor Type. i.e EXPORT or DOMESTIC";
                    myResult = new
                    {
                        Result = result,
                        Msg = msg
                    };
                    return Json(myResult, JsonRequestBehavior.AllowGet);
                }
                string mode = "NETWORK", ip = string.Empty; int port = 0;
                if (down.isBypass == false) { 
                if (down.STAGE_Code == "COM")
                {
                    if (string.IsNullOrEmpty(rolldown.Battery_srlno))
                    {
                        msg = "Battery Should Not Empty";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(rolldown.RearTyre1_srlno1))
                    {
                        msg = "rear Tyre1 Should Not Empty";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(rolldown.RearTyre2_srlno2))
                    {
                        msg = "rear Tyre2 Should Not Empty";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(rolldown.FrontTyre1_srlno1))
                    {
                        msg = " Front Tyre1 Should Not Empty";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(rolldown.FrontTyre2_srlno2))
                    {
                        msg = " Front Tyre2 Should Not Empty";
                        myResult = new
                        {
                            Result = result,
                            Msg = msg
                        };
                        return Json(myResult, JsonRequestBehavior.AllowGet);
                    }
                }
                }
                if (rolldown.Quantity == null)
                {
                    rolldown.Quantity = "1";
                }
                //MessageBox.Show("PDI OK sticker printed successfully !! ");

                string status = ValidateJobforEmpty(rolldown, down, "Print");
                if (status == "OK")
                {
                    msg = af.PrintAssemblyStagesSticker(rolldown, down, Convert.ToInt32(rolldown.Quantity));
                }
                else
                {
                    msg = status;
                }
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
        public JsonResult BypassPasswordPopup(RollDown data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER xss WHERE  XSS.PLANT_CODE='{0}'
                       AND XSS.FAMILY_CODE='{1}' AND OFFLINE_KEYCODE='ROLLDOWN_BYPASS' And AD_PASSWORD='{2}'",
                       data.PLANTCODE.Trim().ToUpper(), data.FAMILYCODE.Trim().ToUpper(), data.BypassPassword.Trim());
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
        public JsonResult UpdateConform(RollDown obj)
        {
            RollDown tractormaster = new RollDown();
            Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
            string msg = string.Empty;
            string sa = obj.Srno;
            if (obj.isSrNoRequire && string.IsNullOrEmpty(obj.TractorSrlno))
            {
                TractorController tractorController = new TractorController();
                COMMONDATA cOMMONDATA = new COMMONDATA();
                cOMMONDATA.PLANT = obj.PLANTCODE;
                cOMMONDATA.FAMILY = obj.FAMILYCODE;
                cOMMONDATA.REMARKS = "EN";
                obj.TractorSrlno = tractorController.getNextTractorNo(cOMMONDATA);
            }
            tractormaster = assemblyfunctions.GetTractorDetails(obj.JOBID.Trim(), obj.PLANTCODE, obj.FAMILYCODE,
                     "JOBID");
            if (obj.isReplaceJob == false)
            {
                if (Convert.ToString(obj.PLANTCODE).Trim().ToUpper() == "T04")
                {
                    query = string.Format(@"update xxes_job_status SET ENGINE='{0}',ENGINE_SRLNO ='{1}',TRANSMISSION='{2}',TRANSMISSION_SRLNO='{3}'
                            ,REARAXEL='{4}',REARAXEL_SRLNO='{5}',
                            HYDRALUIC_SRLNO='{6}',HYDRALUIC='{7}',RTTYRE1='{8}',REARTYRE_SRLNO1='{9}',RTTYRE2='{10}',REARTYRE_SRLNO2='{11}'
                            ,FTTYRE1='{12}',FTTYRE2='{13}',FRONTTYRE_SRLNO1='{14}',FRONTTYRE_SRLNO2='{15}'
                            ,REARRIM_SRLNO1='{16}',REARRIM_SRLNO2='{17}'
                            ,FRONTRIM_SRLNO1='{18}',FRONTRIM_SRLNO2='{19}'
                            ,BATTERY_SRLNO='{20}',BATTERY='{21}'
                            ,RADIATOR_SRLNO='{22}',RADIATOR='{23}',STEERING_MOTOR='{24}',STEERING_MOTOR_SRLNO='{25}'
                            ,STEERING_ASSEMBLY_SRLNO='{26}',STEERING_ASSEMBLY='{27}'
                            ,ALTERNATOR_SRLNO='{28}',ALTERNATOR='{29}',CLUSSTER_SRLNO='{30}',CLUSSTER='{31}'
                            ,STARTER_MOTOR_SRLNO='{32}',STARTER_MOTOR='{33}',ROPS_SRNO='{34}',SIM_SERIAL_NO='{35}',MOBILE='{36}',IMEI_NO='{37}',BATTERy_MAKE='{38}'
                            Where JOBID='{39}'",
                         obj.Engine, obj.Engine_srlno, obj.Transmission, obj.Transmission_srlno, obj.RearAxel, obj.RearAxel_srlno
                         , obj.Hydraulic_srlno, obj.Hydraulic, obj.RearTyre1_dcode, obj.RearTyre1_srlno1, obj.RearTyre2_dcode, obj.RearTyre2_srlno2
                         , obj.FrontTyre1_Dcode, obj.FrontTyre2_Dcode, obj.FrontTyre1_srlno1, obj.FrontTyre2_srlno2
                         , obj.RearRIM1, obj.RearRIM2, obj.FrontRIM1, obj.FrontRIM2, obj.Battery_srlno, obj.Battery
                         , obj.Radiator_srlno, obj.Radiator, obj.SteeringMotor, obj.SteeringMotor_srlno
                         , obj.SteeringAssem_srlno, obj.SteeringAssem, obj.Alternator_srlno, obj.Alternator
                         , obj.Cluster_srlno, obj.Cluster, obj.Motor_srlno, obj.Motor, obj.ROPSrno,obj.Srno, obj.MOBILE, obj.IMEI, obj.BatMake, obj.JOBID.Trim());
                    string check = query;
                    fun.EXEC_QUERY(query);
                    if (tractormaster.Engine_srlno != obj.Engine_srlno && !string.IsNullOrEmpty(obj.Engine_srlno))
                    {
                        query = @"Update SUB_ASSEMBLY_SERIAL_NUMBER set sub_assembly_serial_number='" + obj.Engine_srlno + "' where SERIAL_NUMBER='" + obj.TractorSrlno + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE FTD'";
                        fun.EXEC_QUERY(query);
                        fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, obj.Engine, obj.Engine_srlno, "Engine", (string.IsNullOrEmpty(obj.Engine_srlno) ? "NEW ENTERED" : ""), obj.JOBID, "", "", "");

                    }
                    if (tractormaster.Transmission_srlno != obj.Transmission_srlno && !string.IsNullOrEmpty(obj.Transmission_srlno))
                    {
                        query = @"Update SUB_ASSEMBLY_SERIAL_NUMBER set sub_assembly_serial_number='" + obj.Transmission_srlno + "' where SERIAL_NUMBER='" + obj.TractorSrlno + "' and SUB_ASSEMBLY_FAMILY_CODE='TRANSMISSION FTD'";
                        fun.EXEC_QUERY(query);
                        fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, obj.Transmission
                            , obj.Transmission_srlno, "Transmission",
                     (string.IsNullOrEmpty(obj.Transmission_srlno) ? "NEW ENTERED" : ""), obj.JOBID, "", "", "");
                    }
                    if (tractormaster.RearAxel_srlno != obj.RearAxel_srlno && !string.IsNullOrEmpty(obj.RearAxel_srlno))
                    {
                        query = @"Update SUB_ASSEMBLY_SERIAL_NUMBER set sub_assembly_serial_number='" + obj.RearAxel_srlno + "' where SERIAL_NUMBER='" + obj.TractorSrlno + "' and SUB_ASSEMBLY_FAMILY_CODE='REAR AXEL FTD'";
                        fun.EXEC_QUERY(query);
                        fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, obj.RearAxel, obj.RearAxel_srlno, "RearAxel", (string.IsNullOrEmpty(obj.RearAxel_srlno) ? "NEW ENTERED" : ""), obj.JOBID, "", "", "");

                    }
                }
                else if (Convert.ToString(obj.PLANTCODE).Trim().ToUpper() == "T05")
                {
                    query = string.Format(@"update xxes_job_status SET ENGINE='{0}',ENGINE_SRLNO ='{1}',TRANSMISSION='{2}',TRANSMISSION_SRLNO='{3}'
                            ,BACKEND='{4}',BACKEND_SRLNO='{5}',
                            HYDRALUIC_SRLNO='{6}',HYDRALUIC='{7}',RTTYRE1='{8}',REARTYRE_SRLNO1='{9}',RTTYRE2='{10}',REARTYRE_SRLNO2='{11}'
                            ,FTTYRE1='{12}',FTTYRE2='{13}',FRONTTYRE_SRLNO1='{14}',FRONTTYRE_SRLNO2='{15}'
                            ,REARRIM_SRLNO1='{16}',REARRIM_SRLNO2='{17}'
                            ,FRONTRIM_SRLNO1='{18}',FRONTRIM_SRLNO2='{19}'
                            ,BATTERY_SRLNO='{20}',BATTERY='{21}'
                            ,RADIATOR_SRLNO='{22}',RADIATOR='{23}',STEERING_MOTOR='{24}',STEERING_MOTOR_SRLNO='{25}'
                            ,STEERING_ASSEMBLY_SRLNO='{26}',STEERING_ASSEMBLY='{27}'
                            ,ALTERNATOR_SRLNO='{28}',ALTERNATOR='{29}',CLUSSTER_SRLNO='{30}',CLUSSTER='{31}'
                            ,STARTER_MOTOR_SRLNO='{32}',STARTER_MOTOR='{33}',ROPS_SRNO='{34}',SIM_SERIAL_NO='{35}',MOBILE='{36}',IMEI_NO='{37}',BATTERY_MAKE='{37}'
                            Where JOBID='{38}'",
                               obj.Engine, obj.Engine_srlno, obj.Transmission, obj.Transmission_srlno, obj.Backend, obj.Backend_srlno
                         , obj.Hydraulic_srlno, obj.Hydraulic, obj.RearSrnn1, obj.RearTyre1_srlno1, obj.RearSrnn2, obj.RearTyre2_srlno2
                         , obj.FrontSrnn1, obj.FrontSrnn2, obj.FrontTyre1_srlno1, obj.FrontTyre2_srlno2
                         , obj.RearRIM1, obj.RearRIM2, obj.FrontRIM1, obj.FrontRIM2, obj.Battery_srlno, obj.Battery
                         , obj.Radiator_srlno, obj.Radiator, obj.SteeringMotor, obj.SteeringMotor_srlno
                         , obj.SteeringAssem_srlno, obj.SteeringAssem, obj.Alternator_srlno, obj.Alternator
                         , obj.Cluster_srlno, obj.Cluster, obj.Motor_srlno, obj.Motor, obj.ROPSrno, obj.Srno, obj.MOBILE, obj.IMEI, obj.BatMake, obj.JOBID);
                    fun.EXEC_QUERY(query);
                    if (tractormaster.Engine_srlno != obj.Engine_srlno && !string.IsNullOrEmpty(obj.Engine_srlno))
                    {
                        query = @"Update SUB_ASSEMBLY_SERIAL_NUMBER set sub_assembly_serial_number='" + obj.Engine_srlno + "' where SERIAL_NUMBER='" + obj.TractorSrlno + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE TD'";
                        fun.EXEC_QUERY(query);
                    }
                    if (tractormaster.Backend_srlno != obj.Backend_srlno && !string.IsNullOrEmpty(obj.Backend_srlno))
                    {
                        query = @"Update SUB_ASSEMBLY_SERIAL_NUMBER set sub_assembly_serial_number='" + obj.Backend_srlno + "' where SERIAL_NUMBER='" + obj.TractorSrlno + "' and SUB_ASSEMBLY_FAMILY_CODE='BACK END TD'";
                        fun.EXEC_QUERY(query);
                    }

                }
                string LoginOrgId = fun.getOrgId(obj.PLANTCODE, obj.FAMILYCODE);
                if (!fun.CheckExits("select count(*) from PRINT_SERIAL_NUMBER where Plant_CODE='" + obj.PLANTCODE + "' and  SERIAL_NUMBER='" + obj.TractorSrlno.Trim().ToUpper() + "' and ORGANIZATION_ID='" + LoginOrgId + "'"))
                {

                    query = string.Format("insert into PRINT_SERIAL_NUMBER(Plant_CODE,ITEM_CODE,JOB_ID,SERIAL_NUMBER,ORGANIZATION_ID,CREATION_DATE,BIG_LABEL_PRINTED) values('{0}','{1}','{2}','{3}','{4}',SYSDATE,-1)", obj.PLANTCODE.Trim().ToUpper(), obj.TractorCode.Trim().ToUpper(), obj.JOBID.Trim().ToUpper(), obj.TractorSrlno.Trim().ToUpper(), LoginOrgId);
                    fun.EXEC_QUERY(query);

                }
                query = "";
                query = "select count(*) from JOB_SERIAL_MOVEMENT where PLANT_CODE='" + obj.PLANTCODE.Trim() + "' and  FAMILY_CODE='" + obj.FAMILYCODE + "' and SERIAL_NO='" + obj.TractorSrlno.Trim() + "'";
                if (!fun.CheckExits(query))
                {
                    query = "INSERT INTO JOB_SERIAL_MOVEMENT (PLANT_CODE,ITEM_CODE,JOB_ID,SERIAL_NO,FAMILY_CODE,CURRENT_STAGE_ID,TRANSACTION_COMPLETE,START_STAGE_USER,STAGE_10_TIME) VALUES ('"
                       + obj.PLANTCODE.Trim() + "','" + obj.TractorCode + "' , '" + obj.JOBID + "' , '" + obj.TractorSrlno.Trim() + "' , '" + obj.FAMILYCODE + "' , 0,0,'" + Session["Login_User"] + "',sysdate)";
                    fun.EXEC_QUERY(query);
                }
                if (obj.PLANTCODE == "T04")
                    fun.HookUpDown(obj.JOBID, obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, "9999", obj.TractorAutoid, true, true, "");

                bool datainsert = Insert_Audi(tractormaster, obj);
                msg = "Update successfully";
            }
            else
            {
                string oldFcodeid = fun.getFcodeId(Convert.ToString(obj.JOBID).Trim(), Convert.ToString(obj.PLANTCODE), Convert.ToString(obj.FAMILYCODE)).Trim().ToUpper();

                query = "select ITEM_CODE || '#' || ''  || '#' || FCODE_AUTOID || '#' || ''|| '#' || ITEM_DESC  from XXES_DAILY_PLAN_JOB J, XXES_DAILY_PLAN_TRAN T where J.FCODE_AUTOID=T.AUTOID and J.PLANT_CODE=T.PLANT_CODE and J.FAMILY_CODE=T.FAMILY_CODE and JOBID='" + obj.RepJOBID.ToString().Trim() + "' and  J.plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "' and J.family_code='" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "'";
                string Data = fun.get_Col_Value(query);
                string Fcode = Data.Split('#')[0].Trim();
                string newTractorSrno = Data.Split('#')[1].Trim();
                string Fcode_id = Data.Split('#')[2].Trim();
                string ROPS_SRNO = Data.Split('#')[3].Trim();
                string FCODE_DESC = Data.Split('#')[4].Trim();
                query = "select count(*) from XXES_JOB_STATUS where JOBID='" + obj.RepJOBID.ToString().Trim() + "' and ITEM_CODE='" + Fcode.Trim().ToUpper() + "' and PLANT_CODE='" + Convert.ToString(obj.PLANTCODE) + "' and family_code='" + Convert.ToString(obj.FAMILYCODE) + "'";
                if (!fun.CheckExits(query))
                {
                    if (obj.isReplaceJob==true && string.IsNullOrEmpty(newTractorSrno))
                        newTractorSrno = fun.getTractorcode(obj.JOBID.Trim(), Convert.ToString(obj.PLANTCODE).Trim(), Convert.ToString(obj.FAMILYCODE).Trim());
                    if (Convert.ToString(obj.PLANTCODE).Trim().ToUpper() == "T04")
                    {
                        string Itemdesc = fun.replaceApostophi(FCODE_DESC.Replace("'", "").Replace(",", "").Trim().ToUpper());
                        query = @"insert into XXES_JOB_STATUS(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,JOBID,TRANSMISSION,TRANSMISSION_SRLNO,REARAXEL,REARAXEL_SRLNO,ENGINE,ENGINE_SRLNO,FCODE_SRLNO,HYDRALUIC,HYDRALUIC_SRLNO,
                                        REARTYRE,REARTYRE_SRLNO1,REARTYRE_SRLNO2,REARTYRE_MAKE,FRONTTYRE,FRONTTYRE_SRLNO1,FRONTTYRE_SRLNO2,FRONTTYRE_MAKE,BATTERY,BATTERY_SRLNO,BATTERY_MAKE,FCODE_ID,ENTRYDATE,HYD_PUMP_SRLNO,STEERING_MOTOR_SRLNO,
                                        STEERING_ASSEMBLY_SRLNO,STERING_CYLINDER_SRLNO,RADIATOR_SRLNO,CLUSSTER_SRLNO,ALTERNATOR_SRLNO,STARTER_MOTOR_SRLNO,ROPS_SRNO,SIM_SERIAL_NO,IMEI_NO,MOBILE,BACKEND_SRLNO,OIL,FIPSRNO) values('" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "','" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "','" + Fcode.Trim().ToUpper() + "','" + Itemdesc.Trim().ToUpper() + "'"
                                + ",'" + obj.RepJOBID.Trim().ToUpper().ToString() + "','" + obj.Transmission + "','" + obj.Transmission_srlno.Trim() + "','" + obj.RearAxel + "','" + obj.RearAxel_srlno.Trim() + "','" + obj.Engine + "','" + obj.Engine_srlno.Trim() + "','" + newTractorSrno + "','" + obj.Hydraulic + "','" + obj.Hydraulic_srlno.Trim() + "'"
                                + ",'" + obj.RearTyre1_dcode + "','" + obj.RearTyre1_srlno1.Trim() + "','" + obj.RearTyre2_srlno2.Trim() + "','" + obj.reartyremake.Trim() + "','" + obj.FrontTyre1_Dcode + "','" + obj.FrontTyre1_srlno1.Trim() + "','" + obj.FrontTyre2_srlno2.Trim() + "','" + obj.fronttyremake.Trim() + "','" + obj.Battery + "','" + obj.Battery_srlno.Trim() + "','" + obj.BatMake + "','" + Fcode_id + "',SYSDATE,'" + obj.HydrualicPump_srlno.Trim() + "','" + obj.SteeringMotor_srlno.Trim() + "'"
                                + ",'" + obj.SteeringAssem_srlno.Trim() + "','" + obj.SteeringCylinder_srlno.Trim() + "','" + obj.Radiator_srlno.Trim() + "','" + obj.Cluster_srlno.Trim() + "','" + obj.Alternator_srlno.Trim() + "','" + obj.Motor_srlno.Trim() + "','" + ROPS_SRNO.Trim() + "','" + obj.Srno.Trim().ToUpper() + "','" + obj.IMEI.ToUpper() + "','" + obj.MOBILE.Trim().ToUpper() + "','" + obj.Backend_srlno.Trim() + "','" + obj.OilQty.Trim().ToUpper() + "','" + obj.FIP.Trim() + "')";
                    }
                    else if (Convert.ToString(obj.PLANTCODE).Trim().ToUpper() == "T05")
                    {
                        string Itemdesc = fun.replaceApostophi(FCODE_DESC.Replace("'","").Replace(",","").Trim().ToUpper());
                        query = @"insert into XXES_JOB_STATUS(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,JOBID,BACKEND,BACKEND_SRLNO,ENGINE,ENGINE_SRLNO,FCODE_SRLNO,HYDRALUIC,HYDRALUIC_SRLNO,
                                                                        REARTYRE,REARTYRE_SRLNO1,REARTYRE_SRLNO2,REARTYRE_MAKE,FRONTTYRE,FRONTTYRE_SRLNO1,FRONTTYRE_SRLNO2,FRONTTYRE_MAKE,BATTERY,BATTERY_SRLNO,BATTERY_MAKE,FCODE_ID,ENTRYDATE,HYD_PUMP_SRLNO,STEERING_MOTOR_SRLNO,
                                                                        STEERING_ASSEMBLY_SRLNO,STERING_CYLINDER_SRLNO,RADIATOR_SRLNO,CLUSSTER_SRLNO,ALTERNATOR_SRLNO,STARTER_MOTOR_SRLNO,ROPS_SRNO,SIM_SERIAL_NO,IMEI_NO,MOBILE,OIL,FIPSRNO) values('" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "','" + Convert.ToString(obj.FAMILYCODE).Trim().ToUpper() + "','" + Fcode.Trim().ToUpper() + "','" + fun.replaceApostophi(Itemdesc.Trim().ToUpper()) + "'"
                                + ",'" + obj.RepJOBID.Trim().ToUpper().ToString() + "','" + obj.Backend + "','" + obj.Backend_srlno.Trim() + "','" + obj.Engine + "','" + obj.Engine_srlno.Trim() + "','" + newTractorSrno + "','" + obj.Hydraulic.ToString() + "','" + obj.Hydraulic_srlno.Trim() + "'"
                                + ",'" + obj.RearTyre1_dcode + "','" + obj.RearTyre1_srlno1.Trim() + "','" + obj.RearTyre2_srlno2.Trim() + "','" + obj.reartyremake.Trim() + "','" + obj.FrontTyre1_Dcode + "','" + obj.FrontTyre1_srlno1.Trim() + "','" + obj.FrontTyre2_srlno2.Trim() + "','" + obj.fronttyremake.Trim() + "','" + obj.Battery + "','" + obj.Battery_srlno.Trim() + "','" + obj.BatMake + "','" + Fcode_id + "',SYSDATE,'" + obj.HydrualicPump_srlno.Trim() + "','" + obj.SteeringMotor_srlno.Trim() + "'"
                                + ",'" + obj.SteeringAssem_srlno.Trim() + "','" + obj.SteeringCylinder_srlno.Trim() + "','" + obj.Radiator_srlno.Trim() + "','" + obj.Cluster_srlno.Trim() + "','" + obj.Alternator_srlno.Trim() + "','" + obj.Motor_srlno.Trim() + "','" + ROPS_SRNO.Trim() + "','" + obj.Srno.Trim().ToUpper() + "','" + obj.IMEI.ToUpper() + "','" + obj.MOBILE.Trim().ToUpper() + "','" + obj.OilQty.Trim().ToUpper() + "','" + obj.FIP.Trim() + "')";
                    }
                    UpdateJobsWhileReplace(oldFcodeid.Trim().ToUpper(), Fcode_id.Trim().ToUpper(), obj.JOBID.Trim().ToUpper(), Convert.ToString(obj.RepJOBID).Trim().ToUpper(), obj.TractorCode.Trim().ToUpper(), Fcode.Trim().ToUpper(), newTractorSrno.Trim().ToUpper(), Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper(), query.Trim().ToUpper(),obj);

                    //if (optPlanned.Checked && !chkReplace.Checked)
                    //{
                    //    if (pbf.EXEC_QUERY(query))
                    //    {
                    //        pbf.UpdateTractorPT(newTractorSrno.Trim(), SrnotoUpdate, JOB, Fcode, txtJBackend.Text.Trim(), ActualBackend, txtJEngine.Text.Trim(), ActualEngine, false, "", "", Fcode_id, Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper(), false, isROPSRequire, ROPS_DCODE, ROPS_SRNO);
                    //    }
                    //}
                    //else if (optPlanned.Checked && chkReplace.Checked)
                    //{
                    //        chkReplace.Checked = false;
                    //}
                    //if (optPlanned.Checked == true)
                    //{
                    //    ClearJobBoxes();
                    //    optPlanned_CheckedChanged(new object(), new EventArgs());

                    //}
                }
            }
            //fun.Insert_Part_Audit_Data
            return Json(msg, JsonRequestBehavior.AllowGet);

        }
        private bool UpdateJobsWhileReplace(string oldfcodeid, string newfcodeid, string oldJob,
         string newJob, string oldFcode, string newFcode, string TractorSrno, string plant,
         string family, string tractorquery,RollDown roll)
        {
            string query = "", Suffix = string.Empty, Prefix = string.Empty, plantcode = string.Empty, runningSrlno = string.Empty,
                OldPrefix = string.Empty, TO_BE_DELETE_TRACTOR_NO = string.Empty;
            using (OracleConnection connection = new OracleConnection(ConfigurationManager.ConnectionStrings["CON"].ConnectionString))
            {
                TO_BE_DELETE_TRACTOR_NO = TractorSrno;
                connection.Open();
                string StageCode = "EN";
                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;
                string SDS = fun.GetServerDateTime().Date.ToString("MMM-yyyy").ToUpper();
                // Start a local transaction
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                // Assign transaction object for a pending local transaction
                command.Transaction = transaction;
                string LoginOrgId = fun.getOrgId(plant, family);
                try
                {

                    string OldTractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code='" + oldFcode + "' and autoid='" + oldfcodeid + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                    string NewTractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code='" + newFcode + "' and autoid='" + newfcodeid + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                    if (OldTractorType == "DOMESTIC" && NewTractorType == "EXPORT")
                    {
                        if (TractorSrno.Trim().Length != 12)
                        {
                            throw new Exception("SERIAL NO LENGTH OF DOMESTIC TRACTOR SHOULD BE 12 DIGIT " + oldFcode);
                        }
                        runningSrlno = TractorSrno.Substring(4, 6).Trim();
                        Suffix = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + fun.GetServerDateTime().Date.ToString("MMM-yyyy").ToUpper() + "' and TYPE='EXPORT' and plant='" + plant + "'");
                        Prefix = fun.get_Col_Value("select PREFIX_2 from XXES_ITEM_MASTER where ITEM_CODE='" + newFcode.Trim() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                        if (string.IsNullOrEmpty(Suffix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR SUFFIX NOT DEFINED FOR EXPORT TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR SUFFIX NOT DEFINED FOR EXPORT TRACTOR");
                        }
                        if (string.IsNullOrEmpty(Prefix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR PREFIX NOT DEFINED FOR EXPORT TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR PREFIX NOT DEFINED FOR EXPORT TRACTOR");
                        }
                        if (plant == "T04")
                            plantcode = "F";
                        else if (plant == "T05")
                            plantcode = "P";
                        TractorSrno = Prefix.Trim().ToUpper() + Suffix.Trim().ToUpper() + plantcode + runningSrlno;
                    }
                    else if (OldTractorType == "EXPORT" && NewTractorType == "DOMESTIC")
                    {
                        //if (TractorSrno.Trim().Length != 17)
                        //{
                        //    throw new Exception("SERIAL NO LENGTH OF EXPORT TRACTOR SHOULD BE 17 DIGIT " + oldFcode);
                        //}
                        Suffix = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + fun.GetServerDateTime().Date.ToString("MMM-yyyy").ToUpper() + "' and TYPE='DOMESTIC' and plant='" + plant.Trim().ToUpper() + "'");
                        Prefix = fun.get_Col_Value("select PREFIX_1 from XXES_ITEM_MASTER where ITEM_CODE='" + newFcode.Trim() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                        OldPrefix = fun.get_Col_Value("select PREFIX_1 from XXES_ITEM_MASTER where ITEM_CODE='" + oldFcode.Trim() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                        if (string.IsNullOrEmpty(Suffix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR SUFFIX NOT DEFINED FOR DOMESTIC TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR SUFFIX NOT DEFINED FOR DOMESTIC TRACTOR");
                        }
                        if (string.IsNullOrEmpty(Prefix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR PREFIX NOT DEFINED FOR DOMESTIC TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR PREFIX NOT DEFINED FOR DOMESTIC TRACTOR");
                        }
                        if (string.IsNullOrEmpty(OldPrefix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR OLD PREFIX NOT DEFINED FOR EXPORT TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR PREFIX NOT DEFINED FOR DOMESTIC TRACTOR");
                        }
                        if (plant == "T04")
                            runningSrlno = "2" + TractorSrno.Substring(TractorSrno.Length - 6);
                        else
                            runningSrlno = "3" + TractorSrno.Substring(TractorSrno.Length - 6);
                        TractorSrno = Prefix.Trim() + runningSrlno.Trim() + Suffix.Trim();
                    }
                    else if (OldTractorType == "EXPORT" && NewTractorType == "EXPORT")
                    {
                        runningSrlno = TractorSrno.Substring(TractorSrno.Length - 6).Trim();
                        Suffix = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + fun.GetServerDateTime().Date.ToString("MMM-yyyy").ToUpper() + "' and TYPE='EXPORT' and plant='" + plant + "'");
                        Prefix = fun.get_Col_Value("select PREFIX_2 from XXES_ITEM_MASTER where ITEM_CODE='" + newFcode.Trim() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'");
                        if (string.IsNullOrEmpty(Suffix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR SUFFIX NOT DEFINED FOR EXPORT TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR SUFFIX NOT DEFINED FOR EXPORT TRACTOR");
                        }
                        if (string.IsNullOrEmpty(Prefix))
                        {
                            fun.Insert_Into_ActivityLog("MODAL_CHANGE", "EN", newJob.Trim(), "TRACTOR PREFIX NOT DEFINED FOR EXPORT TRACTOR", plant.Trim().ToUpper(), family.Trim().ToUpper());
                            throw new Exception("TRACTOR PREFIX NOT DEFINED FOR EXPORT TRACTOR");
                        }
                        if (plant == "T04")
                            plantcode = "F";
                        else if (plant == "T05")
                            plantcode = "P";
                        TractorSrno = Prefix.Trim().ToUpper() + Suffix.Trim().ToUpper() + plantcode + runningSrlno;
                    }
                    DeleteJobBeforeReplace(oldfcodeid, newfcodeid, oldJob, newJob, oldFcode, newFcode, TO_BE_DELETE_TRACTOR_NO, plant, family);
                    //pbf.InsertIntoScannedStages(newFcode, newJob, "EN", PubFun.Login_User);
                    query = @"Update xxes_scan_time set jobid='" + newJob + "', item_code='" + newFcode + "' where jobid='" + oldJob + "' and item_code='" + oldFcode + "' and  PLANT_CODE='" + plant.Trim() + "' and  FAMILY_CODE='" + family.Trim() + "'";
                    fun.EXEC_QUERY(query);
                    command.CommandText = tractorquery;
                    command.ExecuteNonQuery();

                    query = "update xxes_job_status set fcode_srlno='" + TractorSrno.Trim() + "' where jobid='" + newJob.Trim() + "' and  PLANT_CODE='" + plant.Trim() + "' and  FAMILY_CODE='" + family.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                    #region Integration
                    query = "";
                    string IsExists = fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='PRINT_SERIAL_NUMBER' and STATUS='Y'");
                    if (!string.IsNullOrEmpty(IsExists) && Convert.ToInt16(IsExists) > 0 && !string.IsNullOrEmpty(TractorSrno.Trim()))
                    {
                        if (!fun.CheckExits("select count(*) from PRINT_SERIAL_NUMBER where Plant_CODE='" + plant.Trim().ToUpper() + "' and  SERIAL_NUMBER='" + TractorSrno.Trim().ToUpper() + "' and ORGANIZATION_ID='" + LoginOrgId + "'"))
                        {
                            query = "";
                            query = String.Format("insert into PRINT_SERIAL_NUMBER(Plant_CODE,ITEM_CODE,JOB_ID,SERIAL_NUMBER,ORGANIZATION_ID,CREATION_DATE,BIG_LABEL_PRINTED) values('{0}','{1}','{2}','{3}','{4}',SYSDATE,-1)", plant.Trim().ToUpper(), newFcode.Trim().ToUpper(), newJob.Trim().ToUpper(), TractorSrno.Trim().ToUpper(), LoginOrgId);
                            command.CommandText = query;
                            command.ExecuteNonQuery();

                        }
                        query = "";
                        query = "select count(*) from JOB_SERIAL_MOVEMENT where PLANT_CODE='" + plant.Trim() + "' and  FAMILY_CODE='" + family + "' and SERIAL_NO='" + TractorSrno.Trim() + "'";
                        if (!fun.CheckExits(query))
                        {
                            query = "";
                            query = "INSERT INTO JOB_SERIAL_MOVEMENT (PLANT_CODE,ITEM_CODE,JOB_ID,SERIAL_NO,FAMILY_CODE,CURRENT_STAGE_ID,TRANSACTION_COMPLETE) VALUES ('"
                               + plant.Trim() + "','" + newFcode.Trim().ToUpper() + "' , '" + newJob + "' , '" + TractorSrno + "' , '" + family + "' , 0,0)";
                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                    }
                    IsExists = fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='SUB_ASSEMBLY_SERIAL_NUMBER' and STATUS='Y'");
                    if (!string.IsNullOrEmpty(IsExists) && Convert.ToInt16(IsExists) > 0)
                    {
                        if (plant == "T04")
                        {
                            if (!string.IsNullOrEmpty(roll.RearAxel_srlno.Trim()) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                            {
                                query = "";
                                query = "select count(*) from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='REAR AXEL FTD'";
                                if (fun.CheckExits(query))
                                {
                                    query = "";
                                    query = "delete from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='REAR AXEL FTD'";
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                }
                                query = "";
                                query = string.Format("INSERT INTO SUB_ASSEMBLY_SERIAL_NUMBER(SERIAL_NUMBER ,SUB_ASSEMBLY_SERIAL_NUMBER,SUB_ASSEMBLY_FAMILY_CODE) values('{0}','{1}','{2}')", TractorSrno.Trim().ToUpper(), roll.RearAxel_srlno.Trim().ToUpper(), "REAR AXEL FTD");
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                            if (!string.IsNullOrEmpty(roll.Transmission_srlno.Trim()) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                            {
                                query = "";
                                query = "select count(*) from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='TRANSMISSION FTD'";
                                if (fun.CheckExits(query))
                                {
                                    query = "";
                                    query = "delete from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='TRANSMISSION FTD'";
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                }
                                query = string.Format("INSERT INTO SUB_ASSEMBLY_SERIAL_NUMBER(SERIAL_NUMBER ,SUB_ASSEMBLY_SERIAL_NUMBER,SUB_ASSEMBLY_FAMILY_CODE) values('{0}','{1}','{2}')", TractorSrno.Trim().ToUpper(), roll.Transmission_srlno.Trim(), "TRANSMISSION FTD");
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                            if (!string.IsNullOrEmpty(roll.Engine_srlno.Trim()) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                            {

                                query = "";
                                query = "select count(*) from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE FTD'";
                                if (fun.CheckExits(query))
                                {
                                    query = "";
                                    query = "delete from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE FTD'";
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                }
                                query = "";
                                query = string.Format("INSERT INTO SUB_ASSEMBLY_SERIAL_NUMBER(SERIAL_NUMBER ,SUB_ASSEMBLY_SERIAL_NUMBER,SUB_ASSEMBLY_FAMILY_CODE) values('{0}','{1}','{2}')", TractorSrno.Trim().ToUpper(), roll.Engine_srlno.Trim(), "ENGINE FTD");
                                command.CommandText = query;
                                command.ExecuteNonQuery();

                            }
                        }
                        else if (plant == "T05")
                        {
                            if (!string.IsNullOrEmpty(roll.Engine_srlno.Trim()) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                            {

                                query = "";

                                query = "select count(*) from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE TD'";
                                if (fun.CheckExits(query))
                                {
                                    query = "";
                                    query = "delete from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='ENGINE TD'";
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                }
                                query = "";
                                query = string.Format("INSERT INTO SUB_ASSEMBLY_SERIAL_NUMBER(SERIAL_NUMBER ,SUB_ASSEMBLY_SERIAL_NUMBER,SUB_ASSEMBLY_FAMILY_CODE) values('{0}','{1}','{2}')", TractorSrno.Trim().ToUpper(), roll.Engine_srlno.Trim(), "ENGINE TD");
                                command.CommandText = query;
                                command.ExecuteNonQuery();

                            }
                            if (!string.IsNullOrEmpty(roll.Backend_srlno.Trim()) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                            {

                                query = "";
                                query = "select count(*) from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='BACK END TD'";
                                if (fun.CheckExits(query))
                                {
                                    query = "";
                                    query = "delete from SUB_ASSEMBLY_SERIAL_NUMBER where SERIAL_NUMBER='" + TractorSrno.Trim() + "' and SUB_ASSEMBLY_FAMILY_CODE='BACK END TD'";
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                }
                                query = "";
                                query = string.Format("INSERT INTO SUB_ASSEMBLY_SERIAL_NUMBER(SERIAL_NUMBER ,SUB_ASSEMBLY_SERIAL_NUMBER,SUB_ASSEMBLY_FAMILY_CODE) values('{0}','{1}','{2}')", TractorSrno.Trim().ToUpper(), roll.Backend_srlno.Trim(), "BACK END TD");
                                command.CommandText = query;
                                command.ExecuteNonQuery();

                            }
                        }
                    }
                    string stage_id = fun.get_Col_Value("select stage_id from XXES_STAGE_MASTER where plant_code='" + plant + "' and family_code='" + family + "' and offline_keycode='EN'");
                    query = @"select count(*) from XXES_PRINT_SERIALS where PLANT_CODE='" + plant.Trim() + "' and  FAMILY_CODE='" + family + "' and DCODE='" + newFcode + "' and SRNO='" + TractorSrno + "'";
                    if (!fun.CheckExits(query) && !string.IsNullOrEmpty(TractorSrno.Trim()))
                    {
                        query = "";
                        query = @"insert into XXES_PRINT_SERIALS(PLANT_CODE,FAMILY_CODE,STAGE_ID,DCODE,SRNO,PRINTDATE,OFFLINE_KEYCODE,TYRE_DCODE,RIM_DCODE,MISC1,FCODE)
                            values('" + plant.Trim() + "','" + family + "','" + stage_id + "','" + newFcode.Trim() + "','" + TractorSrno + "',SYSDATE,'EN','','','','" + newJob.Trim() + "')";
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                    fun.Insert_Part_Audit_Data(plant.Trim().ToUpper(), family.Trim().ToUpper(), oldFcode.Trim().ToUpper(), TO_BE_DELETE_TRACTOR_NO, newFcode, TractorSrno, "CHANGE_JOB_MODEL", "", oldJob.Trim().ToUpper(), newJob.Trim().ToUpper(), OldTractorType, NewTractorType);


                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    //MessageBox.Show(ex.Message.ToString(), "PMS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    fun.LogWrite(ex);
                    throw;
                }
                finally { }
            }
        }

        private bool DeleteJobBeforeReplace(string oldfcodeid, string newfcodeid, string oldJob, string newJob, string oldFcode, string newFcode, string TractorSrno, string plant, string family)
        {

            string query = "";
            using (OracleConnection connection = new OracleConnection(ConfigurationManager.ConnectionStrings["CON"].ConnectionString))
            {
                connection.Open();
                string StageCode = "EN";
                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                // Start a local transaction
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                // Assign transaction object for a pending local transaction
                command.Transaction = transaction;
                string LoginOrgId = fun.getOrgId(plant, family);
                try
                {
                    //query = "update xxes_daily_plan_tran set qty=qty-1 where autoid='" + oldfcodeid + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    //command.CommandText = query;
                    //command.ExecuteNonQuery();

                    //query = "delete from xxes_daily_plan_job where jobid='" + oldJob.Trim() + "' and fcode_autoid='" + oldfcodeid + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    //command.CommandText = query;
                    //command.ExecuteNonQuery();

                    query = "delete from xxes_job_status where jobid='" + oldJob.Trim() + "' and fcode_id='" + oldfcodeid + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "delete from xxes_controllers_data where jobid='" + oldJob.Trim() + "' and fcode_id='" + oldfcodeid + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "delete from XXES_PRINT_SERIALS where srno='" + TractorSrno.Trim().ToUpper() + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "delete from print_serial_number where serial_number='" + TractorSrno.Trim().ToUpper() + "' and job_id='" + oldJob.Trim() + "' and plant_code='" + plant.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "delete from JOB_SERIAL_MOVEMENT where serial_no='" + TractorSrno.Trim().ToUpper() + "' and job_id='" + oldJob.Trim() + "' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    //query = "delete from xxes_scan_time where jobid='" + oldJob.ToUpper() + "' and stage='EN' and plant_code='" + plant.Trim() + "' and family_code='" + family.Trim() + "'";
                    //command.CommandText = query;
                    //command.ExecuteNonQuery();


                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    fun.WriteLog("Module:DeleteJobBeforeReplace=>" + ex.Message.ToString());
                    throw;
                }
                finally { }

            }

        }
        public bool Insert_Audi(RollDown tractormaster, RollDown obj)
        {
            var result = false;
            if (tractormaster.Hydraulic_srlno != obj.Hydraulic_srlno && (!string.IsNullOrEmpty(obj.Hydraulic_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, obj.Hydraulic, obj.Hydraulic_srlno, "Hydraulic", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.Backend_srlno != obj.Backend_srlno && (!string.IsNullOrEmpty(obj.Backend_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.Backend_srlno, "Backend", (string.IsNullOrEmpty(obj.Backend_srlno) ? "NEW ENTERED" : ""), obj.JOBID, "", "", "");
            }
            if (tractormaster.Battery_srlno != obj.Battery_srlno && (!string.IsNullOrEmpty(obj.Battery_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, obj.Battery, obj.Battery_srlno, "Battery", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.HydrualicPump_srlno != obj.HydrualicPump_srlno && (!string.IsNullOrEmpty(obj.HydrualicPump_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.HydrualicPump_srlno, "HydrulicPump", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.SteeringMotor_srlno != obj.SteeringMotor_srlno && (!string.IsNullOrEmpty(obj.SteeringMotor_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.SteeringMotor_srlno, "SteeringMotor", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.SteeringAssem_srlno != obj.SteeringAssem_srlno && (!string.IsNullOrEmpty(obj.SteeringAssem_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.SteeringAssem_srlno, "SteeringAssembly", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.SteeringCylinder_srlno != obj.SteeringCylinder_srlno && (!string.IsNullOrEmpty(obj.SteeringCylinder_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.SteeringCylinder_srlno, "SteeringCylinder", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.Radiator_srlno != obj.Radiator_srlno && (!string.IsNullOrEmpty(obj.Radiator_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.Radiator_srlno, "RaditorAssembly", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.Cluster_srlno != obj.Cluster_srlno && (!string.IsNullOrEmpty(obj.Cluster_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.Cluster_srlno, "clusterAssembly", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.Alternator_srlno != obj.Alternator_srlno && (!string.IsNullOrEmpty(obj.Alternator_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.Alternator_srlno, "Alternator", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.Motor_srlno != obj.Motor_srlno && (!string.IsNullOrEmpty(obj.Motor_srlno)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.Motor_srlno, "StarterMotor", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.RearTyre1_srlno1 != obj.RearTyre1_srlno1 && (!string.IsNullOrEmpty(obj.RearTyre1_srlno1)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.RearTyre1_srlno1, "Reartyre_SrNo1", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.RearRIM1 != obj.RearRIM1 && (!string.IsNullOrEmpty(obj.RearRIM1)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.RearRIM1, "Reartyre_Rim1", "", obj.RearRIM1, "", "", "");
            }
            if (tractormaster.RearRIM1 != obj.RearRIM1 && (!string.IsNullOrEmpty(obj.RearRIM1)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.RearRIM1, "Reartyre_Rim1", "", obj.RearRIM1, "", "", "");
            }
            if (tractormaster.RearTyre2_srlno2 != obj.RearTyre2_srlno2 && (!string.IsNullOrEmpty(obj.RearTyre2_srlno2)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.RearTyre2_srlno2, "Reartyre_SrNo2", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.RearRIM2 != obj.RearRIM2 && (!string.IsNullOrEmpty(obj.RearRIM2)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.RearRIM2, "Reartyre_Rim2", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.FrontTyre1_srlno1 != obj.FrontTyre1_srlno1 && (!string.IsNullOrEmpty(obj.FrontTyre1_srlno1)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.FrontTyre1_srlno1, "FrontTyre_SrNo1", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.FrontRIM1 != obj.FrontRIM1 && (!string.IsNullOrEmpty(obj.FrontRIM1)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorCode, "", obj.FrontRIM1, "FrontTyre_Rim1", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.FrontTyre2_srlno2 != obj.FrontTyre2_srlno2 && (!string.IsNullOrEmpty(obj.FrontTyre2_srlno2)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.FrontTyre2_srlno2, "FrontTyre_SrNo2", "", obj.JOBID, "", "", "");
            }
            if (tractormaster.FrontRIM2 != obj.FrontRIM2 && (!string.IsNullOrEmpty(obj.FrontRIM2)))
            {
                fun.Insert_Part_Audit_Data(obj.PLANTCODE, obj.FAMILYCODE, obj.TractorCode, obj.TractorSrlno, "", obj.FrontRIM2, "FrontTyre_Rim2", "", obj.JOBID, "", "", "");
            }
            return result;
        }
        public JsonResult Update(RollDown obj)
        {
            RollDown rollDown = new RollDown();
            string msg = string.Empty;
            try
            {
                string check = obj.FrontTyre1_srlno1;
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
                string status = "";
                if (string.IsNullOrEmpty(obj.CHANGEJOB_NO))
                {
                     status = ValidateJobforEmpty(rollDown, obj, "update");
                }
                else
                {
                     status = Channge_Job(rollDown, obj);
                }
                if (status != "OK")
                {
                    return Json(status, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Update Successfully";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                msg = "Error " + ex.Message;
            }
            finally { }
            return Json(msg, JsonRequestBehavior.AllowGet);

        }
        public string Channge_Job(RollDown tractormaster, RollDown obj)
        {
            try
            {
                string msg = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                query = "select count(*) from RELESEDJOBORDER where WIP_ENTITY_NAME='" + obj.CHANGEJOB_NO.Trim() + "' and segment1='" + tractormaster.TractorCode.Trim() + "' and ORGANIZATION_ID='" + orgid.Trim() + "'";
                if (!fun.CheckExits(query))
                {
                    msg  = "New Job :" + obj.CHANGEJOB_NO.Trim() + " does not belong to tractor " + tractormaster.TractorSrlno.Trim();
                    return msg;
                }
                query = "select count(*) from xxes_job_status where jobid='" + obj.CHANGEJOB_NO.Trim() + "'";
                if (fun.CheckExits(query))
                {
                    msg = "New Job :" + obj.CHANGEJOB_NO.Trim() + " already buckleup.";
                    return msg;
                }
                query = "select count(*) from print_serial_number where job_id='" + obj.CHANGEJOB_NO.Trim() + "'";
                if (fun.CheckExits(query))
                {
                    msg = "New Job :" + obj.CHANGEJOB_NO + " already used in oracle (print_serial_number).";
                    return msg;
                }
                query = "select count(*) from job_serial_movement where job_id='" + obj.CHANGEJOB_NO.Trim() + "'";
                if (fun.CheckExits(query))
                {
                    msg = "New Job :" + obj.CHANGEJOB_NO.Trim() + " already used in oracle (job_serial_movement).";
                    return msg;
                }
                query = "select count(*) from xxes_daily_plan_job where jobid='" + obj.CHANGEJOB_NO.Trim() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(obj.PLANTCODE.Trim()) + "'";
                if (fun.CheckExits(query))
                {
                    msg = "New Job :" + obj.CHANGEJOB_NO.Trim() + " already found in PMS";
                    return msg;
                }
                query = "select count(*) from xxes_scan_time where jobid='" + obj.CHANGEJOB_NO.Trim() + "'";
                if (fun.CheckExits(query))
                {
                    msg = "New Job :" + obj.CHANGEJOB_NO.Trim() + " already scanned";
                    return msg;
                }
                query = "update xxes_job_status set jobid='" + obj.CHANGEJOB_NO.Trim() + "' where  jobid='" + Convert.ToString(obj.JOBID).Trim() + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("XXES_JOB_STATUS", "UPDATE NEW JOB " + obj.CHANGEJOB_NO.Trim() + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO.Trim(), query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //fun.LogWrite(query.ToString());
                }
                query = "update xxes_scan_time set jobid='" + obj.CHANGEJOB_NO.Trim() + "' where  jobid='" + Convert.ToString(obj.JOBID).Trim() + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("XXES_SCAN_TIME", "UPDATE NEW JOB " + obj.CHANGEJOB_NO.Trim() + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO.Trim(), query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //fun.LogWrite("-- " + fun.ServerDate + "--------" + fun.Login_User);
                    //fun.LogWrite(query);
                }
                query = "update xxes_daily_plan_job set jobid='" + obj.CHANGEJOB_NO.Trim() + "' where  jobid='" + Convert.ToString(obj.JOBID).Trim() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(obj.PLANTCODE) + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_JOB", "UPDATE NEW JOB " + obj.CHANGEJOB_NO.Trim()  + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO.Trim(), query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //pbf.LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                    //fun.LogWrite(query);
                }
                query = "update print_serial_number set job_id='" + obj.CHANGEJOB_NO.Trim() + "' where  job_id='" + Convert.ToString(obj.JOBID).Trim() + "' and plant_code='" + Convert.ToString(obj.PLANTCODE).Trim().ToUpper() + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("PRINT_SERIAL_NUMBER", "UPDATE NEW JOB " + obj.CHANGEJOB_NO.Trim() + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO.Trim(), query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //pbf.LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                    //fun.LogWrite(query);
                }
                query = "update job_serial_movement set job_id='" + obj.CHANGEJOB_NO.Trim() + "' where  job_id='" + Convert.ToString(obj.JOBID).Trim() + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("JOB_SERIAL_MOVEMENT", "UPDATE NEW JOB " + obj.CHANGEJOB_NO.Trim() + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO.Trim(), query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //pbf.LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                    //fun.LogWrite(query);
                }
                string fcodeid = fun.get_Col_Value("select fcode_id from xxes_job_status where  jobid='" + obj.JOBID.Trim() + "'");
                query = "update xxes_controllers_data set jobid='" + obj.CHANGEJOB_NO.Trim() + "',fcode_id='" + fcodeid + "' where  jobid='" + Convert.ToString(obj.JOBID).Trim() + "' and family_code='" + Convert.ToString(obj.FAMILYCODE).Trim() + "' and plant_code='" + Convert.ToString(obj.PLANTCODE) + "'";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("xxes_controllers_data", "UPDATE NEW JOB " + obj.CHANGEJOB_NO + " ON OLD JOB " + Convert.ToString(obj.JOBID).Trim(), obj.CHANGEJOB_NO, query, Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper());
                    //pbf.LogWrite("-- " + PubFun.ServerDate + "--------" + PubFun.Login_User);
                    //fun.LogWrite(query);
                }
                if (!string.IsNullOrEmpty(obj.CHANGEJOB_NO.Trim()))
                {
                    if (obj.JOBID != obj.CHANGEJOB_NO.Trim())
                    {
                        fun.Insert_Part_Audit_Data(Convert.ToString(obj.PLANTCODE).Trim().ToUpper(), Convert.ToString(obj.FAMILYCODE).Trim().ToUpper(), tractormaster.TractorSrlno, tractormaster.TractorSrlno.ToUpper().Trim(), "", "", "Change_In_Job", "", Convert.ToString(obj.JOBID).ToUpper().Trim(), Convert.ToString(obj.CHANGEJOB_NO).ToUpper().Trim(), "", "");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return "OK";
        }
        public string ValidateJobforEmpty(RollDown tractormaster, RollDown updateTractor, string CHktype)
        {
            string foundjob = string.Empty, founddcode = string.Empty;
            try
            {
                bool check = false;
                updateTractor.TractorSrlno = tractormaster.TractorSrlno;
                updateTractor.TractorAutoid = tractormaster.TractorAutoid;
                updateTractor.TractorCode = tractormaster.TractorCode;
                Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
                if (tractormaster.isEngineRequire && string.IsNullOrEmpty(updateTractor.Engine_srlno))
                {
                    return "Engine srlno should not be blank ";
                }
                if (CHktype == "Print")
                {
                    if (updateTractor.isBypass == true)
                    {
                        if (string.IsNullOrEmpty(updateTractor.Engine_srlno))
                        {
                            return "Engine srlno Required";
                        }
                        if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                        {
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                            if (!string.IsNullOrEmpty(foundjob))
                            {
                                return "Engine srlno already found on job " + foundjob;
                            }
                            founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                            if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                            {
                                return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                            }
                            updateTractor.Engine = founddcode;
                        }
                        if (string.IsNullOrEmpty(tractormaster.TractorSrlno))
                        {
                            return "Tractor Srlno should Not Found ";
                        }
                        if (tractormaster.TractorSrlno.Trim().Length == 12) //domestic tractor
                        {
                            if ((string.IsNullOrEmpty(updateTractor.IMEI) || string.IsNullOrEmpty(updateTractor.MOBILE) ||
                                string.IsNullOrEmpty(updateTractor.IMEI)) && tractormaster.reqcarebtn.Trim() == "Y")
                            {
                                return  "For Domestic Tractors, Care button scanning is mandatory";
                            }
                        }
                    }
                    else
                    {
                        if (updateTractor.STAGE_Code == "ENG")
                        {
                            if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Engine srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                                if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Engine = founddcode;
                            }

                            if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                return "Transmission srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Transmission srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Transmission_srlno, "TRB");
                                if (tractormaster.Transmission.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Transmission Dcode mismatch. Master Dcode " + tractormaster.Transmission + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Transmission = founddcode;
                            }
                            if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                return "RearAxel srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "RearAxel srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearAxel_srlno, "HYD");
                                if (tractormaster.RearAxel.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "RearAxel Dcode mismatch. Master Dcode " + tractormaster.RearAxel + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearAxel = founddcode;
                            }
                        }
                        if (updateTractor.STAGE_Code == "BK")
                        {
                            if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                return "Transmission srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Transmission srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Transmission_srlno, "TRB");
                                if (tractormaster.Transmission.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Transmission Dcode mismatch. Master Dcode " + tractormaster.Transmission + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Transmission = founddcode;
                            }
                            if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                return "RearAxel srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "RearAxel srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearAxel_srlno, "HYD");
                                if (tractormaster.RearAxel.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "RearAxel Dcode mismatch. Master Dcode " + tractormaster.RearAxel + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearAxel = founddcode;
                            }
                        }
                        if (updateTractor.STAGE_Code == "COM")
                        {
                            if (string.IsNullOrEmpty(updateTractor.Battery_srlno))
                            {
                                return "Battery Not Scan ";
                            }
                            if (tractormaster.isREQUIRE_REARTYRE && string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                            {
                                return "Left sidde Rear Srnn 1 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre1_srlno1, "REARTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Left sidde Rear Srnn1 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre1_srlno1, "RT");
                                if (tractormaster.RearTyre1_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Left sidde Rear Srnn1 Dcode mismatch. Master Dcode " + tractormaster.RearTyre1_srlno1 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearTyre1_dcode = founddcode;
                            }

                            if (tractormaster.isREQ_RHRT && string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                            {
                                return "right side Rear Srnn2 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre2_srlno2, "REARTYRE_SRLNO2", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "right side Rear Srnn2 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre2_srlno2, "RT");
                                if (tractormaster.RearTyre2_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "right side Rear Srnn2 Dcode mismatch. Master Dcode " + tractormaster.RearSrnn2 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearTyre2_dcode = founddcode;
                            }

                            if (tractormaster.isREQUIRE_FRONTTYRE && string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                            {
                                //return "Left Side Front Srnn1 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre1_srlno1, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Left Side Front Srnn1 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre1_srlno1, "FT");
                                if (tractormaster.FrontTyre1_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Left Side Front Srnn1 Dcode mismatch. Master Dcode " + tractormaster.FrontSrnn1 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.FrontTyre1_Dcode = founddcode;
                            }

                            if (tractormaster.isREQ_RHFT && string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                            {
                                return "right Side Front Srnn2 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre2_srlno2, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "right Side Front Srnn2 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre2_srlno2, "FT");
                                if (tractormaster.FrontTyre2_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "right Side Front Srnn2 Dcode mismatch. Master Dcode " + tractormaster.FrontTyre2_srlno2 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.FrontTyre2_Dcode = founddcode;
                            }
                            if (tractormaster.isHydrualicRequire && string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                            {
                                return "Hydraulic srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Hydraulic_srlno, "HYDRALUIC_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Hydraulic srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Hydraulic_srlno, "HYD");
                                if (tractormaster.Hydraulic.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Hydraulic Dcode mismatch. Master Dcode " + tractormaster.Hydraulic + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Hydraulic = founddcode;
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Engine srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                                if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Engine = founddcode;
                            }

                            if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                return "Transmission srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Transmission srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Transmission_srlno, "TRB");
                                if (tractormaster.Transmission.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Transmission Dcode mismatch. Master Dcode " + tractormaster.Transmission + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Transmission = founddcode;
                            }
                            if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                return "RearAxel srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "RearAxel srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearAxel_srlno, "HYD");
                                if (tractormaster.RearAxel.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "RearAxel Dcode mismatch. Master Dcode " + tractormaster.RearAxel + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearAxel = founddcode;
                            }
                        }
                        if (updateTractor.STAGE_Code == "PDIOK")
                        {
                            if (string.IsNullOrEmpty(updateTractor.Battery_srlno))
                            {
                                return "Battery Not Scan ";
                            }
                            if (tractormaster.isREQUIRE_REARTYRE && string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                            {
                                return "Left sidde Rear Srnn 1 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre1_srlno1, "REARTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Left sidde Rear Srnn1 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre1_srlno1, "RT");
                                if (tractormaster.RearTyre1_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Left sidde Rear Srnn1 Dcode mismatch. Master Dcode " + tractormaster.RearTyre1_srlno1 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearTyre1_dcode = founddcode;
                            }

                            if (tractormaster.isREQ_RHRT && string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                            {
                                return "right side Rear Srnn2 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre2_srlno2, "REARTYRE_SRLNO2", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "right side Rear Srnn2 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre2_srlno2, "RT");
                                if (tractormaster.RearTyre2_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "right side Rear Srnn2 Dcode mismatch. Master Dcode " + tractormaster.RearSrnn2 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearTyre2_dcode = founddcode;
                            }

                            if (tractormaster.isREQUIRE_FRONTTYRE && string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                            {
                                //return "Left Side Front Srnn1 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre1_srlno1, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Left Side Front Srnn1 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre1_srlno1, "FT");
                                if (tractormaster.FrontTyre1_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Left Side Front Srnn1 Dcode mismatch. Master Dcode " + tractormaster.FrontSrnn1 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.FrontTyre1_Dcode = founddcode;
                            }

                            if (tractormaster.isREQ_RHFT && string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                            {
                                return "right Side Front Srnn2 srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre2_srlno2, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "right Side Front Srnn2 srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre2_srlno2, "FT");
                                if (tractormaster.FrontTyre2_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "right Side Front Srnn2 Dcode mismatch. Master Dcode " + tractormaster.FrontTyre2_srlno2 + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.FrontTyre2_Dcode = founddcode;
                            }
                            if (tractormaster.isHydrualicRequire && string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                            {
                                return "Hydraulic srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Hydraulic_srlno, "HYDRALUIC_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Hydraulic srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Hydraulic_srlno, "HYD");
                                if (tractormaster.Hydraulic.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Hydraulic Dcode mismatch. Master Dcode " + tractormaster.Hydraulic + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Hydraulic = founddcode;
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Engine srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                                if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Engine = founddcode;
                            }

                            if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                return "Transmission srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "Transmission srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.Transmission_srlno, "TRB");
                                if (tractormaster.Transmission.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "Transmission Dcode mismatch. Master Dcode " + tractormaster.Transmission + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.Transmission = founddcode;
                            }
                            if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                return "RearAxel srlno should not be blank ";
                            }
                            if (!string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                            {
                                foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID);
                                if (!string.IsNullOrEmpty(foundjob))
                                {
                                    return "RearAxel srlno already found on job " + foundjob;
                                }
                                founddcode = assemblyfunctions.getPartDcode(updateTractor.RearAxel_srlno, "HYD");
                                if (tractormaster.RearAxel.Trim().ToUpper() != founddcode.Trim().ToUpper())
                                {
                                    return "RearAxel Dcode mismatch. Master Dcode " + tractormaster.RearAxel + " and Serial No Dcode " + founddcode;
                                }
                                updateTractor.RearAxel = founddcode;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Engine srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                        if (tractormaster.Engine == null)
                        {
                            tractormaster.Engine = "";
                        }
                        if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.Engine = founddcode;
                    }

                    if (tractormaster.isTransRequire && string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                    {
                        return "Transmission srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.Transmission_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Transmission_srlno, "TRANSMISSION_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Transmission srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.Transmission_srlno, "TRB");
                        if (tractormaster.Transmission.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Transmission Dcode mismatch. Master Dcode " + tractormaster.Transmission + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.Transmission = founddcode;
                    }
                    if (tractormaster.isRearAxelRequire && string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                    {
                        return "RearAxel srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.RearAxel_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearAxel_srlno, "REARAXEL_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "RearAxel srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.RearAxel_srlno, "HYD");
                        if (tractormaster.RearAxel.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "RearAxel Dcode mismatch. Master Dcode " + tractormaster.RearAxel + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.RearAxel = founddcode;
                    }


                    if (tractormaster.isHydrualicRequire && string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                    {
                        return "Hydraulic srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Hydraulic_srlno, "HYDRALUIC_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Hydraulic_srlno, "HYDRALUIC_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Hydraulic srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.Hydraulic_srlno, "HYD");
                        if (tractormaster.Hydraulic.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Hydraulic Dcode mismatch. Master Dcode " + tractormaster.Hydraulic + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.Hydraulic = founddcode;
                    }
                    if (!string.IsNullOrEmpty(updateTractor.Engine_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Engine, "ENGINE_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Engine srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.Engine_srlno, (updateTractor.PLANTCODE == "T04" ? "ENF" : "ENP"));
                        if (tractormaster.Engine.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Engine Dcode mismatch. Master Dcode " + tractormaster.Engine + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.Engine = founddcode;
                    }

                    if (tractormaster.isREQ_HYD_PUMP && string.IsNullOrEmpty(updateTractor.HydrualicPump_srlno))
                    {
                        return "Hydraulic pump srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.HydrualicPump_srlno))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.HydrualicPump_srlno, "HYD_PUMP_SRLNO", updateTractor.JOBID,updateTractor.RepJOBID);
                         else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.HydrualicPump_srlno, "HYD_PUMP_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Hydraulic pump srlno already found on job " + foundjob;
                        }
                        updateTractor.HydrualicPump = founddcode;

                    }

                    if (tractormaster.isREQUIRE_REARTYRE && string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                    {
                        return "Left sidde Rear Srnn 1 srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.RearTyre1_srlno1, "REARTYRE_SRLNO1", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre1_srlno1, "REARTYRE_SRLNO1", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Left sidde Rear Srnn1 srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre1_srlno1, "RT");
                        if (tractormaster.RearTyre1_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Left sidde Rear Srnn1 Dcode mismatch. Master Dcode " + tractormaster.RearTyre1_srlno1 + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.RearTyre1_dcode = founddcode;
                    }

                    if (tractormaster.isREQ_RHRT && string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                    {
                        return "right side Rear Srnn2 srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.RearTyre2_srlno2, "REARTYRE_SRLNO2", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearTyre2_srlno2, "REARTYRE_SRLNO2", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "right side Rear Srnn2 srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.RearTyre2_srlno2, "RT");
                        if (tractormaster.RearTyre2_dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "right side Rear Srnn2 Dcode mismatch. Master Dcode " + tractormaster.RearSrnn2 + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.RearTyre2_dcode = founddcode;
                    }

                    if (tractormaster.isREQUIRE_FRONTTYRE && string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                    {
                        //return "Left Side Front Srnn1 srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.FrontTyre1_srlno1, "FRONTTYRE_SRLNO1", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre1_srlno1, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Left Side Front Srnn1 srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre1_srlno1, "FT");
                        if (tractormaster.FrontTyre1_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Left Side Front Srnn1 Dcode mismatch. Master Dcode " + tractormaster.FrontSrnn1 + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.FrontTyre1_Dcode = founddcode;
                    }

                    if (tractormaster.isREQ_RHFT && string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                    {
                        return "right Side Front Srnn2 srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.FrontTyre2_srlno2, "FRONTTYRE_SRLNO1", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontTyre2_srlno2, "FRONTTYRE_SRLNO1", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "right Side Front Srnn2 srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.FrontTyre2_srlno2, "FT");
                        if (tractormaster.FrontTyre2_Dcode.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "right Side Front Srnn2 Dcode mismatch. Master Dcode " + tractormaster.FrontTyre2_srlno2 + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.FrontTyre2_Dcode = founddcode;
                    }
                    if (tractormaster.isBackendRequire && string.IsNullOrEmpty(updateTractor.Backend_srlno) && updateTractor.PLANTCODE == "T05")
                    {
                        return "Backend srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.Backend_srlno) && updateTractor.PLANTCODE == "T05")
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Backend_srlno, "BACKEND_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Backend_srlno, "BACKEND_SRLNO", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Backend srlno already found on job " + foundjob;
                        }
                        founddcode = assemblyfunctions.getPartDcode(updateTractor.Backend_srlno, "BAB");
                        if (tractormaster.Backend.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        {
                            return "Backend Dcode mismatch. Master Dcode " + tractormaster.Backend_srlno + " and Serial No Dcode " + founddcode;
                        }
                        updateTractor.Backend = founddcode;
                    }

                    if (tractormaster.isREQ_REARRIM && string.IsNullOrEmpty(updateTractor.RearRIM1) && string.IsNullOrEmpty(updateTractor.RearRIM2))
                    {
                        return "Rear RIM1 And RIM2 srlno should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.RearRIM1) && !string.IsNullOrEmpty(updateTractor.RearRIM2))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.RearRIM1, "REARRIM_SRLNO1", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearRIM1, "REARRIM_SRLNO1", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Rear RIM1 srlno already found on job " + foundjob;
                        }
                        updateTractor.RearRIM1 = founddcode;
                        foundjob = assemblyfunctions.DuplicateCheck(updateTractor.RearRIM2, "REARRIM_SRLNO2", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Rear RIM2 srlno already found on job " + foundjob;
                        }
                        updateTractor.RearRIM2 = founddcode;
                        //founddcode = assemblyfunctions.getPartDcode(updateTractor.Backend, "BAB");
                        //if (tractormaster.Backend.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        //{
                        //    return "Backend Dcode mismatch. Master Dcode " + tractormaster.HydrualicPump + " and Serial No Dcode " + founddcode;
                        //}
                    }


                    if (tractormaster.isREQ_FRONTRIM && string.IsNullOrEmpty(updateTractor.FrontRIM1) && string.IsNullOrEmpty(updateTractor.FrontRIM2))
                    {
                        return "Front RIM1 And RIM2 should not be blank ";
                    }
                    if (!string.IsNullOrEmpty(updateTractor.FrontRIM1) && !string.IsNullOrEmpty(updateTractor.FrontRIM2))
                    {
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.FrontRIM1, "FRONTRIM_SRLNO1", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontRIM1, "FRONTRIM_SRLNO1", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Front RIM1 srlno already found on job " + foundjob;
                        }
                        if (updateTractor.isReplaceJob == true)
                            foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.FrontRIM2, "FRONTRIM_SRLNO2", updateTractor.JOBID, updateTractor.RepJOBID);
                        else
                            foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontRIM2, "FRONTRIM_SRLNO2", updateTractor.JOBID);
                        if (!string.IsNullOrEmpty(foundjob))
                        {
                            return "Front RIM2 srlno already found on job " + foundjob;
                        }
                        //founddcode = assemblyfunctions.getPartDcode(updateTractor.Backend, "BAB");
                        //if (tractormaster.Backend.Trim().ToUpper() != founddcode.Trim().ToUpper())
                        //{
                        //    return "Backend Dcode mismatch. Master Dcode " + tractormaster.HydrualicPump + " and Serial No Dcode " + founddcode;
                        //}
                    }
                    //if (tractormaster.isREQUIRE_BATTERY && string.IsNullOrEmpty(updateTractor.Battery_srlno))
                    //{
                    //    return "Battery Not Scan ";
                    //}
                    if (tractormaster.isREQUIRE_BATTERY && string.IsNullOrEmpty(updateTractor.Battery_srlno))
                    {
                        return "BATTERY srlno should not be blank ";
                    }
                    //if (!string.IsNullOrEmpty(updateTractor.Battery_srlno))
                    //{

                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Battery_srlno, "BATTERY_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Battery_srlno, "BATTERY_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Battery srlno already found on job " + foundjob;
                    //    }
                    //    if (!string.IsNullOrEmpty(tractormaster.Battery))
                    //    {
                    //        string dummyDcode = fun.get_Col_Value(
                    //            string.Format(@"select distinct parameterinfo from xxes_sft_settings where status='BATDUMMY' 
                    //            and plant_code='{0}'", tractormaster.PLANTCODE));
                    //        if (!string.IsNullOrEmpty(dummyDcode))
                    //        {

                    //            if (dummyDcode.Trim().ToUpper() == tractormaster.Battery.Trim().ToUpper())
                    //            {
                    //                query = string.Format(@"select count(*) from xxes_sft_settings where paramvalue='{0}' 
                    //                and status='BATDUMMYNO' and parameterinfo='{1}'", updateTractor.Battery_srlno, dummyDcode.Trim());
                    //                if (!fun.CheckExits(query))
                    //                {
                    //                    return "Invalid Dummy Serial No";
                    //                }
                    //            }
                    //        }
                    //    }

                    //}

                    //if (tractormaster.isREQ_RADIATOR && string.IsNullOrEmpty(updateTractor.Radiator_srlno))
                    //{
                    //    return "Radiator srlno should not be blank ";
                    //}

                    //if (!string.IsNullOrEmpty(updateTractor.Radiator_srlno))
                    //{     
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Radiator_srlno, "RADIATOR_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Radiator_srlno, "RADIATOR_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Radiator srlno already found on job " + foundjob;
                    //    }
                    //    founddcode = assemblyfunctions.SplitDcode(updateTractor.Radiator_srlno.Trim().ToUpper(), "RADIATOR").Trim().ToUpper();
                    //    if (founddcode != tractormaster.Radiator.ToUpper().Trim().ToUpper())
                    //    {
                    //        return "Radiator Dcode mismatch. Master Dcode " + tractormaster.Radiator + " and Serial No Dcode " + founddcode;
                    //    }
                    //    updateTractor.Radiator = founddcode;
                    //}

                    //if (tractormaster.isREQ_STEERING_MOTOR && string.IsNullOrEmpty(updateTractor.SteeringMotor_srlno))
                    //{
                    //    return "Steering Motor srlno should not be blank ";
                    //}

                    //if (!string.IsNullOrEmpty(updateTractor.SteeringMotor_srlno))
                    //{
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.SteeringMotor_srlno, "STEERING_MOTOR_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.SteeringMotor_srlno, "STEERING_MOTOR_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "SteeringMotor srlno already found on job " + foundjob;
                    //    }
                    //    founddcode = assemblyfunctions.SplitDcode(updateTractor.SteeringMotor_srlno.Trim().ToUpper(), "POWER_STMOTOR").Trim().ToUpper();
                    //    if (founddcode != tractormaster.SteeringMotor.ToUpper().Trim().ToUpper())
                    //    {
                    //        return "Radiator Dcode mismatch. Master Dcode " + tractormaster.SteeringMotor + " and Serial No Dcode " + founddcode;
                    //    }
                    //    updateTractor.SteeringMotor = founddcode;
                    //}
                    //if (tractormaster.isREQ_STEERING_ASSEMBLY && string.IsNullOrEmpty(updateTractor.SteeringAssem_srlno))
                    //{
                    //    return "STEERING ASSEMBLY srlno should not be blank ";
                    //}

                    //if (!string.IsNullOrEmpty(updateTractor.SteeringAssem_srlno))
                    //{
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.SteeringAssem_srlno, "STEERING_ASSEMBLY_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.SteeringAssem_srlno, "STEERING_ASSEMBLY_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Steering Assembly srlno already found on job " + foundjob;
                    //    }
                    //}

                    //if (tractormaster.isREQ_ALTERNATOR && string.IsNullOrEmpty(updateTractor.Alternator_srlno))
                    //{
                    //    return "Alternator srlno should not be blank ";
                    //}
                    //if (!string.IsNullOrEmpty(updateTractor.Alternator_srlno))
                    //{
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Alternator_srlno, "ALTERNATOR_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Alternator_srlno, "ALTERNATOR_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Alternator srlno already found on job " + foundjob;
                    //    }
                    //    founddcode = assemblyfunctions.SplitDcode(updateTractor.Alternator_srlno.Trim().ToUpper(), "ALT").Trim().ToUpper();
                    //    if (founddcode != tractormaster.Alternator.ToUpper().Trim().ToUpper())
                    //    {
                    //        return "Alternator Dcode mismatch. Master Dcode " + tractormaster.Alternator + " and Serial No Dcode " + founddcode;
                    //    }
                    //    updateTractor.Alternator = founddcode;
                    //}

                    //if (tractormaster.isREQ_CLUSSTER && string.IsNullOrEmpty(updateTractor.Cluster_srlno))
                    //{
                    //    return "Cluster srlno should not be blank ";
                    //}
                    //if (!string.IsNullOrEmpty(updateTractor.Cluster))
                    //{
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Cluster_srlno, "CLUSSTER_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Cluster_srlno, "CLUSSTER_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Cluster srlno already found on job " + foundjob;
                    //    }
                    //if (string.IsNullOrEmpty(tractormaster.Srno)) { 
                    //}
                    //}
                    //if (tractormaster.isREQ_STARTER_MOTOR && string.IsNullOrEmpty(updateTractor.Motor_srlno))
                    //{
                    //    return "Motor srlno should not be blank ";
                    //}
                    //if (!string.IsNullOrEmpty(updateTractor.Motor))
                    //{
                    //if (updateTractor.isReplaceJob == true)
                    //    foundjob = assemblyfunctions.DuplicateJobCheck(updateTractor.Motor_srlno, "STARTER_MOTOR_SRLNO", updateTractor.JOBID, updateTractor.RepJOBID);
                    //else
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Motor_srlno, "STARTER_MOTOR_SRLNO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "Motor srlno already found on job " + foundjob;
                    //    }
                    //    founddcode = assemblyfunctions.SplitDcode(updateTractor.Motor_srlno.Trim().ToUpper(), "START_MOTOR").Trim().ToUpper();
                    //    if (founddcode != tractormaster.Motor.ToUpper().Trim().ToUpper())
                    //    {
                    //        return "Starter motor Dcode mismatch. Master Dcode " + tractormaster.Motor + " and Serial No Dcode " + founddcode;
                    //    }
                    //    updateTractor.Motor = founddcode;
                    //}
                    //bool isNewROPSSrNo = false;
                    //if (!string.IsNullOrEmpty(updateTractor.ROPSrno))
                    //{
                    //    query = string.Format(@"select count(*) from  xxes_torque_master where item_dcode='{0}' and srno_req=1 
                    //    and plant_code='{1}' and family_code='{2}' and (to_number(START_SERIALNO)<{3}
                    //    OR TO_NUMBER(END_SERIALNO)>{3})", tractormaster.ROPS, tractormaster.PLANTCODE, tractormaster.FAMILYCODE, updateTractor.ROPS);
                    //    if (fun.CheckExits(query))
                    //    {
                    //        return "ROPS Serial no not valid. It should be in range of start and end serial number";

                    //    }
                    //    isNewROPSSrNo = false;
                    //}
                    //if (tractormaster.isREQ_ROPS && string.IsNullOrEmpty(updateTractor.ROPSrno))
                    //{
                    //    string ROPS_SRNO = string.Empty;
                    //    //generate new rops serial no and assign to textboxstring 
                    //    assemblyfunctions.GetROPSSrno(updateTractor.PLANTCODE, updateTractor.FAMILYCODE, tractormaster.ROPS, out ROPS_SRNO);
                    //    updateTractor.ROPSrno = ROPS_SRNO;
                    //    if (string.IsNullOrEmpty(updateTractor.ROPSrno))
                    //        return "Unable to generate ROPS Serial no";
                    //    else
                    //        isNewROPSSrNo = true;
                    //}
                    // make drop down. get tyre make for all tyre serial no and make should be same. set make to selected value if any for selected job
                    if (!string.IsNullOrEmpty(updateTractor.RearTyre1_srlno1))
                    {
                        updateTractor.reartyreleftsidemake1 = assemblyfunctions.makeTyre("RT", updateTractor.RearTyre1_srlno1);
                    }
                    if (!string.IsNullOrEmpty(updateTractor.RearTyre2_srlno2))
                    {
                        updateTractor.reartyrerightsidemake2 = assemblyfunctions.makeTyre("RT", updateTractor.RearTyre2_srlno2);
                    }
                    if (!string.IsNullOrEmpty(updateTractor.FrontTyre1_srlno1))
                    {
                        updateTractor.fronttyreleftsidemake1 = assemblyfunctions.makeTyre("FT", updateTractor.FrontTyre1_srlno1);
                    }
                    if (!string.IsNullOrEmpty(updateTractor.FrontTyre2_srlno2))
                    {
                        updateTractor.fronttyrerightsidemake2 = assemblyfunctions.makeTyre("FT", updateTractor.FrontTyre2_srlno2);
                    }
                    if (updateTractor.reartyreleftsidemake1 != updateTractor.fronttyreleftsidemake1 || updateTractor.reartyrerightsidemake2 != updateTractor.fronttyrerightsidemake2)
                    {
                        return "Check both tyre assemblies serial nos. They should belong to Same making company.i.e.";
                    }
                    //if (!string.IsNullOrEmpty(updateTractor.MOBILE))
                    //{
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.MOBILE, "MOBILE", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "MOBILE already found on job " + foundjob;
                    //    }
                    //}
                    //if (!string.IsNullOrEmpty(updateTractor.IMEI))
                    //{
                    //    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.IMEI, "IMEI_NO", updateTractor.JOBID);
                    //    if (!string.IsNullOrEmpty(foundjob))
                    //    {
                    //        return "IMEI already found on job " + foundjob;
                    //    }
                    //}
                    if (tractormaster.reartyremake != null)
                    {
                        updateTractor.reartyremake = tractormaster.reartyremake;
                    }
                    if (tractormaster.fronttyremake != null)
                    {
                        updateTractor.fronttyremake = tractormaster.fronttyremake;
                    }
                    if (string.IsNullOrEmpty(updateTractor.Hydraulic))
                    {
                        updateTractor.Hydraulic = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Hydraulic_srlno))
                    {
                        updateTractor.Hydraulic_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Battery))
                    {
                        updateTractor.Battery = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Battery_srlno))
                    {
                        updateTractor.Battery_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.batterymake))
                    {
                        updateTractor.batterymake = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.HydrualicPump_srlno))
                    {
                        updateTractor.HydrualicPump_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.SteeringMotor_srlno))
                    {
                        updateTractor.SteeringMotor_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.SteeringAssem_srlno))
                    {
                        updateTractor.SteeringAssem_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.SteeringCylinder_srlno))
                    {
                        updateTractor.SteeringCylinder_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Radiator_srlno))
                    {
                        updateTractor.Radiator_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Cluster_srlno))
                    {
                        updateTractor.Cluster_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Alternator_srlno))
                    {
                        updateTractor.Alternator_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Motor_srlno))
                    {
                        updateTractor.Motor_srlno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Srno))
                    {
                        updateTractor.Srno = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.IMEI))
                    {
                        updateTractor.IMEI = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.MOBILE))
                    {
                        updateTractor.MOBILE = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.OilQty))
                    {
                        updateTractor.OilQty = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.FIP))
                    {
                        updateTractor.FIP = "";
                    }
                    if (string.IsNullOrEmpty(updateTractor.Backend_srlno))
                    {
                        updateTractor.Backend_srlno = "";
                    }
                    if (tractormaster.isSrNoRequire && string.IsNullOrEmpty(updateTractor.TractorSrlno))
                    {
                        TractorController tractorController = new TractorController();
                        COMMONDATA cOMMONDATA = new COMMONDATA();
                        cOMMONDATA.PLANT = updateTractor.PLANTCODE;
                        cOMMONDATA.FAMILY = updateTractor.FAMILYCODE;
                        cOMMONDATA.REMARKS = "EN";
                        updateTractor.TractorSrlno = tractorController.getNextTractorNo(cOMMONDATA);
                        if (tractormaster.TractorSrlno != updateTractor.TractorSrlno)
                        {
                            return "Conform Message Create Tractor Srlno";
                        }
                    }
                    else
                    {
                        UpdateConform(updateTractor);
                    }
                }
            }
            catch (Exception ex)
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
    #endregion
}