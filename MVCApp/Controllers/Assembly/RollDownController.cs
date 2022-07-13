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
        public JsonResult UpdateConform(RollDown obj)
        {
            RollDown tractormaster = new RollDown();
            Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
            string msg = string.Empty;
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
                            ,STARTER_MOTOR_SRLNO='{32}',STARTER_MOTOR='{33}',ROPS_SRNO='{34}',MOBILE='{35}',IMEI_NO='{36}'
                            Where JOBID='{37}'",
                     obj.Engine, obj.Engine_srlno, obj.Transmission, obj.Transmission_srlno, obj.RearAxel, obj.RearAxel_srlno
                     , obj.Hydraulic_srlno, obj.Hydraulic, obj.RearTyre1_dcode, obj.RearTyre1_srlno1, obj.RearTyre2_dcode, obj.RearTyre2_srlno2
                     , obj.FrontTyre1_Dcode, obj.FrontTyre2_Dcode, obj.FrontTyre1_srlno1, obj.FrontTyre2_srlno2
                     , obj.RearRIM1, obj.RearRIM2, obj.FrontRIM1, obj.FrontRIM2, obj.Battery_srlno, obj.Battery
                     , obj.Radiator_srlno, obj.Radiator, obj.SteeringMotor, obj.SteeringMotor_srlno
                     , obj.SteeringAssem_srlno, obj.SteeringAssem, obj.Alternator_srlno, obj.Alternator
                     , obj.Cluster_srlno, obj.Cluster, obj.Motor_srlno, obj.Motor, obj.ROPSrno, obj.MOBILE, obj.IMEI, obj.JOBID.Trim());
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
                            ,REARRIM_SRLNO1='{18}',REARRIM_SRLNO2='{19}'
                            ,FRONTRIM_SRLNO1='{22}',FRONTRIM_SRLNO2='{23}'
                            ,BATTERY_SRLNO='{24}',BATTERY='{25}'
                            ,RADIATOR_SRLNO='{26}',RADIATOR='{27}',STEERING_MOTOR='{28}',STEERING_MOTOR_SRLNO='{29}'
                            ,STEERING_ASSEMBLY_SRLNO='{30}',STEERING_ASSEMBLY='{31}'
                            ,ALTERNATOR_SRLNO='{32}',ALTERNATOR='{33}',CLUSSTER_SRLNO='{34}',CLUSSTER='{35}'
                            ,STARTER_MOTOR_SRLNO='{36}',STARTER_MOTOR='{37}',ROPS_SRNO='{38}',MOBILE='{39}',IMEI_NO='{40}',=''
                            Where JOBID='{41}'",
                           obj.Engine, obj.Engine_srlno, obj.Transmission, obj.Transmission_srlno, obj.Backend, obj.Backend_srlno
                     , obj.Hydraulic_srlno, obj.Hydraulic, obj.RearSrnn1, obj.RearTyre1_srlno1, obj.RearSrnn2, obj.RearTyre2_srlno2
                     , obj.FrontSrnn1, obj.FrontSrnn2, obj.FrontTyre1_srlno1, obj.FrontTyre2_srlno2
                     , obj.RearRIM1, obj.RearRIM2, obj.FrontRIM1, obj.FrontRIM2, obj.Battery_srlno, obj.Battery
                     , obj.Radiator_srlno, obj.Radiator, obj.SteeringMotor, obj.SteeringMotor_srlno
                     , obj.SteeringAssem_srlno, obj.SteeringAssem, obj.Alternator_srlno, obj.Alternator
                     , obj.Cluster_srlno, obj.Cluster, obj.Motor_srlno, obj.Motor, obj.ROPSrno, obj.MOBILE, obj.IMEI, obj.JOBID);
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
            //fun.Insert_Part_Audit_Data
            return Json(msg, JsonRequestBehavior.AllowGet);

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
                if (tractormaster.isREQ_HYD_PUMP && string.IsNullOrEmpty(updateTractor.HydrualicPump_srlno))
                {
                    return "Hydraulic pump srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.HydrualicPump_srlno))
                {
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
                if (tractormaster.isBackendRequire && string.IsNullOrEmpty(updateTractor.Backend_srlno) && updateTractor.PLANTCODE == "T05")
                {
                    return "Backend srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Backend_srlno) && updateTractor.PLANTCODE == "T05")
                {
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
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.FrontRIM1, "FRONTRIM_SRLNO1", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Front RIM1 srlno already found on job " + foundjob;
                    }
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

                if (tractormaster.isREQUIRE_BATTERY && string.IsNullOrEmpty(updateTractor.Battery_srlno))
                {
                    return "BATTERY srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Battery_srlno))
                {

                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Battery_srlno, "BATTERY_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Battery srlno already found on job " + foundjob;
                    }
                    if (!string.IsNullOrEmpty(tractormaster.Battery))
                    {
                        string dummyDcode = fun.get_Col_Value(
                            string.Format(@"select distinct parameterinfo from xxes_sft_settings where status='BATDUMMY' 
                            and plant_code='{0}'", tractormaster.PLANTCODE));
                        if (!string.IsNullOrEmpty(dummyDcode))
                        {

                            if (dummyDcode.Trim().ToUpper() == tractormaster.Battery.Trim().ToUpper())
                            {
                                query = string.Format(@"select count(*) from xxes_sft_settings where paramvalue='{0}' 
                                and status='BATDUMMYNO' and parameterinfo='{1}'", updateTractor.Battery_srlno, dummyDcode.Trim());
                                if (!fun.CheckExits(query))
                                {
                                    return "Invalid Dummy Serial No";
                                }
                            }
                        }
                    }

                }

                if (tractormaster.isREQ_RADIATOR && string.IsNullOrEmpty(updateTractor.Radiator_srlno))
                {
                    return "Radiator srlno should not be blank ";
                }

                if (!string.IsNullOrEmpty(updateTractor.Radiator_srlno))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Radiator_srlno, "RADIATOR_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Radiator srlno already found on job " + foundjob;
                    }
                    founddcode = assemblyfunctions.SplitDcode(updateTractor.Radiator_srlno.Trim().ToUpper(), "RADIATOR").Trim().ToUpper();
                    if (founddcode != tractormaster.Radiator.ToUpper().Trim().ToUpper())
                    {
                        return "Radiator Dcode mismatch. Master Dcode " + tractormaster.Radiator + " and Serial No Dcode " + founddcode;
                    }
                    updateTractor.Radiator = founddcode;
                }

                if (tractormaster.isREQ_STEERING_MOTOR && string.IsNullOrEmpty(updateTractor.SteeringMotor_srlno))
                {
                    return "Steering Motor srlno should not be blank ";
                }

                if (!string.IsNullOrEmpty(updateTractor.SteeringMotor_srlno))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.SteeringMotor_srlno, "STEERING_MOTOR_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "SteeringMotor srlno already found on job " + foundjob;
                    }
                    founddcode = assemblyfunctions.SplitDcode(updateTractor.SteeringMotor_srlno.Trim().ToUpper(), "POWER_STMOTOR").Trim().ToUpper();
                    if (founddcode != tractormaster.SteeringMotor.ToUpper().Trim().ToUpper())
                    {
                        return "Radiator Dcode mismatch. Master Dcode " + tractormaster.SteeringMotor + " and Serial No Dcode " + founddcode;
                    }
                    updateTractor.SteeringMotor = founddcode;
                }
                if (tractormaster.isREQ_STEERING_ASSEMBLY && string.IsNullOrEmpty(updateTractor.SteeringAssem_srlno))
                {
                    return "STEERING ASSEMBLY srlno should not be blank ";
                }

                if (!string.IsNullOrEmpty(updateTractor.SteeringAssem_srlno))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.SteeringAssem_srlno, "STEERING_ASSEMBLY_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Steering Assembly srlno already found on job " + foundjob;
                    }
                }

                if (tractormaster.isREQ_ALTERNATOR && string.IsNullOrEmpty(updateTractor.Alternator_srlno))
                {
                    return "Alternator srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Alternator_srlno))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Alternator_srlno, "ALTERNATOR_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Alternator srlno already found on job " + foundjob;
                    }
                    founddcode = assemblyfunctions.SplitDcode(updateTractor.Alternator_srlno.Trim().ToUpper(), "ALT").Trim().ToUpper();
                    if (founddcode != tractormaster.Alternator.ToUpper().Trim().ToUpper())
                    {
                        return "Alternator Dcode mismatch. Master Dcode " + tractormaster.Alternator + " and Serial No Dcode " + founddcode;
                    }
                    updateTractor.Alternator = founddcode;
                }

                if (tractormaster.isREQ_CLUSSTER && string.IsNullOrEmpty(updateTractor.Cluster_srlno))
                {
                    return "Cluster srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Cluster))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Cluster_srlno, "CLUSSTER_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Cluster srlno already found on job " + foundjob;
                    }

                }
                if (tractormaster.isREQ_STARTER_MOTOR && string.IsNullOrEmpty(updateTractor.Motor_srlno))
                {
                    return "Motor srlno should not be blank ";
                }
                if (!string.IsNullOrEmpty(updateTractor.Motor))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.Motor_srlno, "STARTER_MOTOR_SRLNO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "Motor srlno already found on job " + foundjob;
                    }
                    founddcode = assemblyfunctions.SplitDcode(updateTractor.Motor_srlno.Trim().ToUpper(), "START_MOTOR").Trim().ToUpper();
                    if (founddcode != tractormaster.Motor.ToUpper().Trim().ToUpper())
                    {
                        return "Starter motor Dcode mismatch. Master Dcode " + tractormaster.Motor + " and Serial No Dcode " + founddcode;
                    }
                    updateTractor.Motor = founddcode;
                }
                bool isNewROPSSrNo = false;
                if (!string.IsNullOrEmpty(updateTractor.ROPSrno))
                {
                    query = string.Format(@"select count(*) from  xxes_torque_master where item_dcode='{0}' and srno_req=1 
                    and plant_code='{1}' and family_code='{2}' and (to_number(START_SERIALNO)<{3}
                    OR TO_NUMBER(END_SERIALNO)>{3})", tractormaster.ROPS, tractormaster.PLANTCODE, tractormaster.FAMILYCODE, updateTractor.ROPS);
                    if (fun.CheckExits(query))
                    {
                        return "ROPS Serial no not valid. It should be in range of start and end serial number";

                    }
                    isNewROPSSrNo = false;
                }
                if (tractormaster.isREQ_ROPS && string.IsNullOrEmpty(updateTractor.ROPSrno))
                {
                    string ROPS_SRNO = string.Empty;
                    //generate new rops serial no and assign to textboxstring 
                    assemblyfunctions.GetROPSSrno(updateTractor.PLANTCODE, updateTractor.FAMILYCODE, tractormaster.ROPS, out ROPS_SRNO);
                    updateTractor.ROPSrno = ROPS_SRNO;
                    if (string.IsNullOrEmpty(updateTractor.ROPSrno))
                        return "Unable to generate ROPS Serial no";
                    else
                        isNewROPSSrNo = true;
                }
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
                if (!string.IsNullOrEmpty(updateTractor.MOBILE))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.MOBILE, "MOBILE", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "MOBILE already found on job " + foundjob;
                    }
                }
                if (!string.IsNullOrEmpty(updateTractor.IMEI))
                {
                    foundjob = assemblyfunctions.DuplicateCheck(updateTractor.IMEI, "IMEI_NO", updateTractor.JOBID);
                    if (!string.IsNullOrEmpty(foundjob))
                    {
                        return "IMEI already found on job " + foundjob;
                    }
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

}