using Microsoft.Ajax.Utilities;
using MVCApp.Common;
using MVCApp.CommonFunction;
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
    public class BeforePaintAssemblyController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        Assemblyfunctions assemblyfunctions = new Assemblyfunctions();
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
        public string ValidateFIP(BeforePaintAssemblyModel fipdata)
        {
            string family = string.Empty;
            string IsInjector = string.Empty, MappedFIP = string.Empty, line = string.Empty;
            try
            {
                if (fipdata.Plant == "T04")
                {
                    family = "ENGINE FTD";
                }
                else if (fipdata.Plant == "T05")
                {
                    family = "ENGINE TD";
                }
                query = string.Format(@"select ITEM_CODE from XXES_ENGINE_STATUS where FUEL_INJECTION_PUMP_SRNO='{0}'
                and ENGINE_SRNO<>'{1}'", fipdata.FIP, fipdata.ENGINE);
                line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    return "FIP srno already used on Engine :" + line;

                }
                string enginedcode = fun.get_Col_Value(string.Format(@"select engine from xxes_job_status where jobid='{0}' and plant_code='{1}'
                and family_code='{2}'", fipdata.ScanJOB, fipdata.Plant, fipdata.Family));
                if (!string.IsNullOrEmpty(enginedcode))
                {

                    line = fun.get_Col_Value(string.Format(@"SELECT injector || '#' || fuel_injection_pump FROM xxes_engine_master where ITEM_CODE =  '{0}' 
                        AND plant_code = '{1}' AND FAMILY_CODE = '{2}'", enginedcode, fipdata.Plant, family));
                    if (!string.IsNullOrEmpty(line))
                    {
                        IsInjector = line.Split('#')[0].Trim();
                        MappedFIP = line.Split('#')[1].Trim();
                        char[] strLength = fipdata.FIP.Trim().ToCharArray();

                        if (IsInjector.Trim() == "Y")
                        {
                            if (strLength.Length < 4)
                            {
                                return "FIP SHOULD BE MINIMUM 4 CHARACTERS LONG";
                            }
                            query = "Select ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO='" + fipdata.FIP.Trim().Substring(0, 10).Trim() + "'";
                            fipdata.FIP_DCODE = fun.get_Col_Value(query);

                        }
                        else
                        {
                            if (strLength.Length < 10)
                            {
                                return "FIP SHOULD BE MINIMUM 10 CHARACTERS LONG";
                            }
                            query = "Select ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO='" + fipdata.FIP.Trim().Substring(0, 10).Trim() + "'";
                            fipdata.FIP_DCODE = fun.get_Col_Value(query);

                        }
                        if (string.IsNullOrEmpty(fipdata.FIP_DCODE))
                        {
                            return "FIP MODAL NOT FOUND FROM BARCODE ";
                        }
                    }
                    else
                    {
                        return "FIP DCODE NOT FOUND IN MASTER";
                    }
                }
                else
                {
                    return "ENGINE DCODE NOT FOUND FOR FIP";
                }
                if (fipdata.FIP_DCODE != MappedFIP)
                {
                    return "FIP MISMATCH IN SELECTED JOB,ENGINE";
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return "";
        }

        
public JsonResult UpdateRecord(BeforePaintAssemblyModel data)
        {
            string HookMsg = string.Empty; string HookMsgType = string.Empty;
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty;
            try
            {
                if (Convert.ToInt32(data.HOOKNO) >= 1001 && Convert.ToInt32(data.HOOKNO) <= 1093)
                {
                    string resVali = CheckValidation(data);
                    string fipValidate = string.Empty;
                    if (!string.IsNullOrEmpty(data.FIP))
                    {
                        fipValidate = ValidateFIP(data);
                    }
                    if (string.IsNullOrEmpty(resVali) && string.IsNullOrEmpty(fipValidate))
                    {
                        query = "UPDATE xxes_job_status SET TRANSMISSION_SRLNO ='" + data.TRANSMISSION + "',REARAXEL_SRLNO='" + data.REARAXEL + "',ENGINE_SRLNO='" + data.ENGINE + "'," +
                            "HYDRALUIC_SRLNO='" + data.HYDRAULIC + "',BACKEND_SRLNO='" + data.SKID + "',STEERING_MOTOR_SRLNO='" + data.STERRINGMOTOR + "',STARTER_MOTOR_SRLNO='" + data.STARTERMOTOR + "',ALTERNATOR_SRLNO='" + data.ALTERNATOR + "'," +
                            "FIPSRNO='" + data.FIP + "',HYD_PUMP_SRLNO='" + data.HYDRAULIC_PUMP + "',REMARKS='" + data.REMARKS + "' WHERE jobid='" + data.ScanJOB + "'";

                        if (fun.EXEC_QUERY(query))
                        {


                            UpdateBackwardEngineSrno(data);

                            string hookno = data.HOOKNO;
                            if (!string.IsNullOrEmpty(hookno))
                            {
                                if (data.HookChk == true)
                                {
                                    //Put the code to check is job already hooked up and down
                                  assemblyfunctions.HookdownAccordingtoCurrentHook(data, false);
                                    if (assemblyfunctions.CheckIsHook_UP_DOWN(data.ScanJOB)) // here we are checking is this job is already hookup or not. if not then proceed
                                    {
                                        assemblyfunctions.HookdownAccordingtoCurrentHook(data,true);
                                        //here lets hookdown the job -22 and below in-between hooksnos down logic not require
                                        //string res =   DoHookDown(hookno);
                                        //if (res == "Saved")
                                        //{
                                        bool isHookedUp = HookUpDown(data.ScanJOB, data.Plant, data.Family, data.FCode, data.HOOKNO, data.FCId, true, false, "", "");
                                        if (isHookedUp)
                                        {
                                            msg = "Update successfully...";
                                            HookMsg = "Job No. " + data.ScanJOB + " is Hookedup on hook no. " + data.HOOKNO;
                                            fun.UpdateLcdDisplay(data.Plant, data.Family, HookMsg, data.ScanJOB, "BP", "MSG");
                                        }
                                        else
                                        {

                                        }
                                        //}
                                        //else
                                        //{
                                        //    msg = "Detail updated successfully and " + res;
                                        //}
                                    }
                                    else
                                    {
                                        msg = "Job no. " + data.ScanJOB + " is already hook down..";
                                    }
                                }
                                else
                                {
                                    msg = "Update successfully...";
                                }
                            }
                            else
                            {
                                msg = "HOOK NO is missing...";
                            }
                        }
                        else
                        {
                            msg = "Some thing went wrong ...";
                        }
                    }
                    else
                    {
                        msg = resVali;
                        msg += "  " + fipValidate;
                    }
                }
                else
                {
                    msg = "Invalid hook no.";
                }
            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
                msg = ex.Message;
            }

            var result = new { Msg = msg, MsgHook = HookMsg };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult UpdateRecord(BeforePaintAssemblyModel data)
        //{
        //    string HookMsg = string.Empty; string HookMsgType = string.Empty;
        //    DataTable dt = new DataTable();
        //    string msg = string.Empty; string mstType = string.Empty;
        //    try
        //    {
        //        if (Convert.ToInt32(data.HOOKNO) >= 1001 && Convert.ToInt32(data.HOOKNO) <= 1093)
        //        {
        //            string resVali = CheckValidation(data);
        //            string fipValidate = string.Empty;
        //            if (!string.IsNullOrEmpty(data.FIP))
        //            {
        //                fipValidate = ValidateFIP(data);
        //            }
        //            if (string.IsNullOrEmpty(resVali) && string.IsNullOrEmpty(fipValidate))
        //            {
        //                query = "UPDATE xxes_job_status SET TRANSMISSION_SRLNO ='" + data.TRANSMISSION + "',REARAXEL_SRLNO='" + data.REARAXEL + "',ENGINE_SRLNO='" + data.ENGINE + "'," +
        //                    "HYDRALUIC_SRLNO='" + data.HYDRAULIC + "',BACKEND_SRLNO='" + data.SKID + "',STEERING_MOTOR_SRLNO='" + data.STERRINGMOTOR + "',STARTER_MOTOR_SRLNO='" + data.STARTERMOTOR + "',ALTERNATOR_SRLNO='" + data.ALTERNATOR + "'," +
        //                    "FIPSRNO='" + data.FIP + "',HYD_PUMP_SRLNO='" + data.HYDRAULIC_PUMP + "',REMARKS='" + data.REMARKS + "' WHERE jobid='" + data.ScanJOB + "'";

        //                if (fun.EXEC_QUERY(query))
        //                {


        //                    UpdateBackwardEngineSrno(data);

        //                    string hookno = data.HOOKNO;
        //                    if (!string.IsNullOrEmpty(hookno))
        //                    {
        //                        if (data.HookChk == true)
        //                        {
        //                            //Put the code to check is job already hooked up and down
        //                            HookdownAccordingtoCurrentHook(data, false);
        //                            if (CheckIsHook_UP_DOWN(data.ScanJOB)) // here we are checking is this job is already hookup or not. if not then proceed
        //                            {
        //                                HookdownAccordingtoCurrentHook(data,true);
        //                                //here lets hookdown the job -22 and below in-between hooksnos down logic not require
        //                                //string res =   DoHookDown(hookno);
        //                                //if (res == "Saved")
        //                                //{
        //                                bool isHookedUp = HookUpDown(data.ScanJOB, data.Plant, data.Family, data.FCode, data.HOOKNO, data.FCId, true, false, "", "");
        //                                if (isHookedUp)
        //                                {
        //                                    msg = "Update successfully...";
        //                                    HookMsg = "Job No. " + data.ScanJOB + " is Hookedup on hook no. " + data.HOOKNO;
        //                                    fun.UpdateLcdDisplay(data.Plant, data.Family, HookMsg, data.ScanJOB, "BP", "MSG");
        //                                }
        //                                else
        //                                {

        //                                }
        //                                //}
        //                                //else
        //                                //{
        //                                //    msg = "Detail updated successfully and " + res;
        //                                //}
        //                            }
        //                            else
        //                            {
        //                                msg = "Job no. " + data.ScanJOB + " is already hook down..";
        //                            }
        //                        }
        //                        else
        //                        {
        //                            msg = "Update successfully...";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        msg = "HOOK NO is missing...";
        //                    }
        //                }
        //                else
        //                {
        //                    msg = "Some thing went wrong ...";
        //                }
        //            }
        //            else
        //            {
        //                msg = resVali;
        //                msg += "  " + fipValidate;
        //            }
        //        }
        //        else
        //        {
        //            msg = "Invalid hook no.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        fun.WriteLog(ex.Message);
        //        msg = ex.Message;
        //    }

        //    var result = new { Msg = msg, MsgHook = HookMsg };
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        private void HookdownAccordingtoCurrentHook(BeforePaintAssemblyModel beforePaintAssemblyModel,bool isShuffleHook)
        {
            try
            {
                int hookno = Convert.ToInt32(beforePaintAssemblyModel.HOOKNO);
                int previousHook = 0; string JOBID = string.Empty;
                if(!isShuffleHook)
                {
                    previousHook = hookno;
                }
                else
                {
                    previousHook = Convert.ToInt32(hookno) - 22;
                    if (Convert.ToInt32(previousHook) < 1001)
                    {
                        int rem = Convert.ToInt32(hookno) - 1001;
                        int ac = 22 - rem;
                        previousHook = 1093 - ac;
                    }
                }
                if (previousHook != 0)
                {
                    query = string.Format(@"SELECT * FROM (SELECT JOBID FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP' AND HOOK_NO<>'9999' 
                                    and HOOK_NO='{0}'
                                    AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC)a WHERE  ROWNUM=1", previousHook.ToString());
                    JOBID = fun.get_Col_Value(query);
                }
                if (!string.IsNullOrEmpty(JOBID))
                {
                    if (ValidJobForHookDown(JOBID))
                    {
                        HookUpDown(JOBID, beforePaintAssemblyModel.Plant, beforePaintAssemblyModel.Family, "",
                            "", "", false, true, "", "");
                    }
                }
            }
            catch (Exception ex)
            {

                fun.WriteLog(ex.Message);
            }
        }

        private void UpdateBackwardEngineSrno(BeforePaintAssemblyModel fipdata)
        {
            try
            {
                query = string.Format(@"select count(*) FROM XXES_ENGINE_STATUS where engine_srno='{0}'", fipdata.ENGINE);
                string family = string.Empty;
                if (!fun.CheckExits(query))
                {
                    if (fipdata.Plant == "T04")
                    {
                        family = "ENGINE FTD";
                    }
                    else if (fipdata.Plant == "T05")
                    {
                        family = "ENGINE TD";
                    }
                    //string findPlantFamily = string.Format(@"select plant_code,family_code,engine_srlno from xxes_job_status where jobid='{0}'", fipdata.ScanJOB.Trim());
                    query = string.Format(@"insert into XXES_ENGINE_STATUS(plant_code,family_code,item_code,engine_srno,
                    FUEL_INJECTION_PUMP_SRNO,FUEL_INJECTION_PUMP) values('{0}','{1}',
                    '{2}','{3}','{4}','{5}') ", fipdata.Plant, family, fipdata.ENGINE_DCODE, fipdata.ENGINE, fipdata.FIP,
                    fipdata.FIP_DCODE);
                    if (fun.EXEC_QUERY(query))
                    {

                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(fipdata.FIP))
                    {
                        query = string.Format(@"update XXES_ENGINE_STATUS set FUEL_INJECTION_PUMP_SRNO='{0}',
                        FUEL_INJECTION_PUMP='{2}' where engine_srno='{1}'", fipdata.FIP, fipdata.ENGINE, fipdata.FIP_DCODE);
                        if (fun.EXEC_QUERY(query))
                        {

                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool ValidJobForHookDown(string job)
        {
            bool result = true;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='BP'", job);

                int BPCount = Convert.ToInt32(fun.CheckExits(query));

                if (BPCount > 0)
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", job);

                    int count = Convert.ToInt32(fun.CheckExits(query));
                    if (count > 0)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        public bool CheckIsHook_UP_DOWN(string job)
        {
            bool result = false;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='BP'", job);

                int BPCount = Convert.ToInt32(fun.CheckExits(query));

                if (BPCount > 0)
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", job);

                    int count = Convert.ToInt32(fun.CheckExits(query));
                    if (count > 0)
                    {
                        result = false;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public JsonResult UpdateRecordDummy(BeforePaintAssemblyModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty;
            try
            {
                string hookno = data.HKNo;
                if (!string.IsNullOrEmpty(hookno))
                {
                    string res = DoHookDown(hookno);
                    if (res == "Saved")
                    {
                        query = string.Format(@"SELECT 'Q' || TO_CHAR(SYSDATE, 'DDMMYYHH24MISS') FROM DUAL");
                        string DummyJob = fun.get_Col_Value(query);
                        bool isHookedUp = HookUpDown(DummyJob, "T04", "TRACTOR FTD", "DUMMY", data.HKNo, "0", true, false, "", data.HType);
                        if (isHookedUp)
                        {
                            msg = "Update Successfully...";
                        }
                    }
                    else
                    {
                        msg = res;
                    }

                    //string IsOcupi = isValidHook(hookno);
                    //if (!string.IsNullOrEmpty(IsOcupi))
                    //{
                    //    if (IsOcupi == "Free")
                    //    {
                    //        query = string.Format(@"SELECT 'Q' || TO_CHAR(SYSDATE, 'DDMMYYHH24MISS') FROM DUAL");
                    //        string DummyJob = fun.get_Col_Value(query);
                    //        bool isHookedUp = HookUpDown(DummyJob, "T04", "TRACTOR FTD", "DUMMY", data.HKNo, "0", true, false, "", data.HType);
                    //        if (isHookedUp)
                    //        {
                    //            msg = "Update Successfully...";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        msg = IsOcupi;
                    //    }
                    //}
                }
                else
                {
                    msg = "HOOK NO is missing...";
                }
            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
                msg = ex.Message;
            }

            var result = new { Msg = msg };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDetailbyId(BeforePaintAssemblyModel data)
        {
            dt = new DataTable();
            string query = string.Empty;
            string Data = string.Empty;
            // string ScanJob = Convert.ToString(sj);
            BeforePaintAssemblyModel BPA = new BeforePaintAssemblyModel();
            if (!string.IsNullOrEmpty(data.ScanJOB))
            {
                try
                {
                    query = string.Format(@"SELECT PLANT_CODE,FAMILY_CODE,ITEM_CODE,TRANSMISSION_SRLNO,REARAXEL_SRLNO,ENGINE, 
                            ENGINE_SRLNO,HYDRALUIC_SRLNO,BACKEND_SRLNO,STEERING_MOTOR_SRLNO,STARTER_MOTOR_SRLNO,
                            ALTERNATOR_SRLNO,FIPSRNO,FCODE_ID,HYD_PUMP_SRLNO,REMARKS,final_label_date from xxes_job_status
                            where jobid = '" + data.ScanJOB + "'");

                    dt = fun.returnDataTable(query);

                    if (dt.Rows.Count > 0)
                    {
                        BPA.Plant = Convert.ToString(dt.Rows[0]["PLANT_CODE"]);
                        BPA.Family = Convert.ToString(dt.Rows[0]["FAMILY_CODE"]);
                        BPA.FCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                        BPA.TRANSMISSION = Convert.ToString(dt.Rows[0]["TRANSMISSION_SRLNO"]);
                        BPA.REARAXEL = Convert.ToString(dt.Rows[0]["REARAXEL_SRLNO"]);
                        BPA.ENGINE = Convert.ToString(dt.Rows[0]["ENGINE_SRLNO"]);
                        BPA.ENGINE_DCODE = Convert.ToString(dt.Rows[0]["ENGINE"]);
                        BPA.HYDRAULIC = Convert.ToString(dt.Rows[0]["HYDRALUIC_SRLNO"]);
                        BPA.SKID = Convert.ToString(dt.Rows[0]["BACKEND_SRLNO"]);
                        BPA.STERRINGMOTOR = Convert.ToString(dt.Rows[0]["STEERING_MOTOR_SRLNO"]);
                        BPA.STARTERMOTOR = Convert.ToString(dt.Rows[0]["STARTER_MOTOR_SRLNO"]);
                        BPA.ALTERNATOR = Convert.ToString(dt.Rows[0]["ALTERNATOR_SRLNO"]);
                        BPA.FIP = Convert.ToString(dt.Rows[0]["FIPSRNO"]);

                        BPA.FCId = Convert.ToString(dt.Rows[0]["FCODE_ID"]);
                        BPA.HYDRAULIC_PUMP = Convert.ToString(dt.Rows[0]["HYD_PUMP_SRLNO"]);
                        BPA.REMARKS = Convert.ToString(dt.Rows[0]["REMARKS"]);

                        if (string.IsNullOrEmpty(BPA.FIP))
                        {
                            BPA.FIP = getFip(BPA.ENGINE);
                        }





                        if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["final_label_date"])))
                        {
                            BPA.ErrorMsg = "";
                            // if exsiting hook no not come and job is fresh then sequence
                            string res = getHookno(data.ScanJOB);
                            if (!string.IsNullOrEmpty(res))
                            {
                                BPA.HOOKNO = res;
                            }
                            else
                            {
                                BPA.HOOKNO = NextHookNo();
                            }
                            //call function to getexisting hook for this job;
                        }
                        else
                        {
                            BPA.ErrorMsg = "This job no. rollout done..";
                        }
                    }
                }
                catch (Exception ex)
                {
                    fun.WriteLog(ex.Message);
                    BPA.ErrorMsg = "Error ! " + ex.Message;
                }
            }
            JsonResult result = Json(BPA, JsonRequestBehavior.AllowGet);
            return result;
        }

        private string getFip(string enginesrno)
        {
            try
            {
                query = string.Format(@"SELECT FUEL_INJECTION_PUMP_SRNO FROM XXES_ENGINE_STATUS e WHERE E.ENGINE_SRNO='{0}'", enginesrno);
                return Convert.ToString(fun.get_Col_Value(query));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string getHookno(string job)
        {
            string r = string.Empty;
            try
            {
                query = string.Format(@"SELECT hook_no FROM XXES_CONTROLLERS_DATA  WHERE JOBID='{0}' AND STAGE='BP' AND JOBID NOT IN 
                (SELECT jobid FROM  XXES_CONTROLLERS_DATA  WHERE JOBID='{0}' AND STAGE='AP')", job);
                r = fun.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
                //throw;
            }
            return r;
        }

        public JsonResult CheckPassword(BeforePaintAssemblyModel data)
        {
            string query = string.Empty;
            string msg = string.Empty;
            if (!string.IsNullOrEmpty(data.Password))
            {
                if (Convert.ToString(Session["Login_Unit"]) == "GU")
                {
                    query = String.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER WHERE OFFLINE_KEYCODE = '{0}' AND AD_PASSWORD = '{1}'", "BPE", data.Password);
                }
                else
                {
                    query = String.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND OFFLINE_KEYCODE = '{2}' AND AD_PASSWORD = '{3}'", data.Plant, data.Family, "BPE", data.Password);
                }

                int count = Convert.ToInt32(fun.get_Col_Value(query));
                if (count > 0)
                {
                    msg = "OK";
                }
                else
                {
                    msg = "Invalid password..";
                }
            }
            else
            {
                msg = "Please enter password..";
            }

            JsonResult result = Json(msg, JsonRequestBehavior.AllowGet);
            return result;
        }

        private string NextHookNo()
        {
            string LastHookNo = "";
            string query = string.Empty;
            string Data = string.Empty;
            string LastDetail = string.Empty;
            try
            {
                query = string.Format(@"SELECT * FROM (SELECT HOOK_NO || '#' || JOBID FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP' AND HOOK_NO<>'9999' 
                                    AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC)a WHERE  ROWNUM=1");


                LastDetail = Convert.ToString(fun.get_Col_Value(query));
                if (!string.IsNullOrEmpty(LastDetail))
                {
                    string hook = LastDetail.Split('#')[0];
                    string job = LastDetail.Split('#')[1];
                    if (hook != "9999")
                    {
                        query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE STAGE='AP' AND JOBID = '{0}'", job.Trim());
                        int count = Convert.ToInt32(fun.get_Col_Value(query));

                        if (count == 0)
                        {
                            if (!string.IsNullOrEmpty(hook))
                            {
                                if (hook == "1093")
                                {
                                    LastHookNo = "1001";
                                }
                                else
                                {
                                    LastHookNo = (Convert.ToInt32(hook) + 1).ToString();
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
            }

            return LastHookNo;
        }

        public JsonResult GetNextHookNo()
        {

            string LastHookNo = NextHookNo();
            JsonResult result = Json(LastHookNo, JsonRequestBehavior.AllowGet);
            return result;
        }

        public string DoHookDown(string hook)
        {
            String status = String.Empty;
            DataTable dt = new DataTable();

            int CurrentHookNo = Convert.ToInt32(hook);
            string lhook = NextHookNo();
            int LastHookNo = 0;
            if (!string.IsNullOrEmpty(lhook))
                LastHookNo = Convert.ToInt32(lhook) - 1;
            int NextHook = LastHookNo + 1;

            if (Convert.ToInt32(hook) >= 1001 && Convert.ToInt32(hook) <= 1093)
            {
                if (CurrentHookNo > LastHookNo)
                {
                    int Difference = CurrentHookNo - LastHookNo;
                    if (Difference != 0)
                    {
                        for (int i = NextHook; i <= CurrentHookNo; i++)
                        {
                            query = string.Format(@"SELECT * FROM (SELECT * FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP' AND HOOK_NO<>'9999' AND HOOK_NO = '{0}'
                                      AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC)a WHERE  ROWNUM=1", Convert.ToString(i));

                            dt = fun.returnDataTable(query);

                            if (dt.Rows.Count > 0)
                            {
                                query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", Convert.ToString(dt.Rows[0]["JOBID"]));

                                int count = Convert.ToInt32(fun.CheckExits(query));
                                if (count == 0)
                                {
                                    bool isHookedUp = HookUpDown(
                                    Convert.ToString(dt.Rows[0]["JOBID"]),
                                    Convert.ToString(dt.Rows[0]["PLANT_CODE"]),
                                    Convert.ToString(dt.Rows[0]["FAMILY_CODE"]),
                                    Convert.ToString(dt.Rows[0]["ITEM_CODE"]),
                                    Convert.ToString(dt.Rows[0]["HOOK_NO"]),
                                    Convert.ToString(dt.Rows[0]["FCODE_ID"]),
                                    false, true, "", ""
                                    );
                                }
                            }
                        }
                        status = "Saved";
                    }
                }
                else
                {
                    int DiffToLast = 1093 - LastHookNo;
                    int DiffToFirst = CurrentHookNo - 1001;
                    int TotalDiff = DiffToLast + DiffToFirst;

                    int LoopEnd = 1093;
                    for (int i = NextHook; i <= LoopEnd; i++)
                    {
                        query = string.Format(@"SELECT * FROM (SELECT * FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP' AND HOOK_NO<>'9999' AND HOOK_NO = '{0}'
                                      AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC)a WHERE  ROWNUM=1", Convert.ToString(i));

                        dt = fun.returnDataTable(query);

                        if (dt.Rows.Count > 0)
                        {
                            query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", Convert.ToString(dt.Rows[0]["JOBID"]));

                            int count = Convert.ToInt32(fun.CheckExits(query));
                            if (count == 0)
                            {
                                bool isHookedUp = HookUpDown(
                                Convert.ToString(dt.Rows[0]["JOBID"]),
                                Convert.ToString(dt.Rows[0]["PLANT_CODE"]),
                                Convert.ToString(dt.Rows[0]["FAMILY_CODE"]),
                                Convert.ToString(dt.Rows[0]["ITEM_CODE"]),
                                Convert.ToString(dt.Rows[0]["HOOK_NO"]),
                                Convert.ToString(dt.Rows[0]["FCODE_ID"]),
                                false, true, "", ""
                                );
                            }
                        }
                        if (i == 1093)
                        {
                            i = 1000;
                            LoopEnd = CurrentHookNo;
                        }
                    }
                    status = "Saved";
                }
            }
            else
            {
                status = "Invalid hook no.";
            }
            return status;
        }

        public bool HookUpDown(string job, string plant, string family, string Fcode, string hook, string fcode_id,
            bool isHookUp, bool isHookDown, string hookupdate, string HType)
        {
            try
            {
                DateTime hookdate = new DateTime();
                string existingHook = string.Empty;
                if (string.IsNullOrEmpty(hookupdate))
                {
                    hookdate = fun.GetServerDateTime();
                }
                else if (!DateTime.TryParse(hookupdate, out hookdate))
                {

                }
                string query = string.Empty;
                if (string.IsNullOrEmpty(Fcode))
                {
                    query = string.Format(@"select item_code || '#' || fcode_id from xxes_job_status where plant_code='{0}'
                    and family_code='{1}' and jobid='{2}'", plant, family, job);
                    string line = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        Fcode = line.Split('#')[0].Trim();
                        fcode_id = line.Split('#')[1].Trim();
                    }
                }
                if (isHookUp)
                {
                    if (!fun.CheckExits("select count(*) from xxes_controllers_data where jobid = '" + job + "' and stage = 'BP' and HOOK_NO = '" + hook + "' and plant_code = '" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE = '" + Convert.ToString(family).Trim().ToUpper() + "'"))
                    {
                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID,HOOKUP_DUMMY_ITEM) values(TO_DATE('" + hookdate.ToString("yyyy/MM/dd HH:mm:ss") + "','yyyy/mm/dd HH24:MI:SS'),'" + plant.Trim().ToUpper() + "','" + family.Trim().ToUpper() + "','" + Fcode.Trim().ToUpper() + "','" + job.Trim() + "','" + hook + "','BP','" + fcode_id.Trim() + "','" + HType.Trim() + "')";
                        if (fun.EXEC_QUERY(query))
                        {
                        }
                    }
                }
                existingHook = fun.get_Col_Value("select hook_no from xxes_controllers_data where jobid='" + job + "' and stage='BP' and plant_code='" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(family).Trim().ToUpper() + "'");
                if (!string.IsNullOrEmpty(existingHook))
                {
                    hook = existingHook;
                }

                if (isHookDown)
                {
                    if (!fun.CheckExits("select count(*) from xxes_controllers_data where jobid = '" + job.Trim() + "' and stage = 'AP' and HOOK_NO = '" + hook + "' and plant_code = '" + plant.Trim() + "' and FAMILY_CODE = '" + family.Trim().ToUpper() + "'"))
                    {
                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID,FLAG)
                            values(sysdate,'" + plant + "','" + family + "','" + Fcode + "','" + job + "','" + hook.Trim() + "','AP','" + fcode_id.Trim() + "','Y')";
                        if (fun.EXEC_QUERY(query))
                        {
                            // MessageBox.Show("Hooked UP sucessfully !!", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    query = "update xxes_controllers_data set REMARKS1='MANNUAL',FLAG='Y' where jobid='" + job + "' and hook_no='" + hook.Trim() + "' and stage='BP'  and plant_code='" + plant.Trim() + "' and FAMILY_CODE='" + family.Trim().ToUpper() + "'";
                    if (fun.EXEC_QUERY(query))
                    {

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
                throw;
            }
            finally
            { }

        }

        public string CheckValidation(BeforePaintAssemblyModel data)
        {
            try
            {
                DataTable dt = new DataTable();
                string msg = string.Empty; string mstType = string.Empty;
                string query = "";
                int count = 0;

                query = string.Format(@"SELECT I.ENGINE, S.ENGINE_SRLNO, I.REQUIRE_ENGINE, 
                                        I.REARAXEL, I.REQUIRE_REARAXEL, S.REARAXEL_SRLNO,
                                        I.TRANSMISSION, I.REQUIRE_TRANS,S.TRANSMISSION_SRLNO,                                        
                                        I.BACKEND, I.REQUIRE_BACKEND, S.BACKEND_SRLNO, 
                                        I.HYDRAULIC, I.REQUIRE_HYD, S.HYDRALUIC_SRLNO,
                                        I.STEERING_MOTOR, I.REQ_STEERING_MOTOR, S.STEERING_MOTOR_SRLNO,
                                        I.STARTER_MOTOR, I.REQ_STARTER_MOTOR, S.STARTER_MOTOR_SRLNO,
                                        I.ALTERNATOR, I.REQ_ALTERNATOR, S.ALTERNATOR_SRLNO, 
                                        S.FIPSRNO, S.ITEM_CODE, S.FCODE_ID,
                                        I.HYD_PUMP, I.REQ_HYD_PUMP, S.HYD_PUMP_SRLNO
                                        FROM XXES_JOB_STATUS S JOIN XXES_ITEM_MASTER I ON S.ITEM_CODE = I.ITEM_CODE AND S.PLANT_CODE = I.PLANT_CODE AND
                                        S.JOBID='{0}' and s.plant_code='{1}' and s.family_code='{2}'", data.ScanJOB, data.Plant, data.Family);

                dt = fun.returnDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return "Data not found..";
                }
                string enggfamily = string.Empty;
                if (data.Plant.Trim() == "T04")
                    enggfamily = "ENGINE FTD";
                else if (data.Plant.Trim() == "T05")
                    enggfamily = "ENGINE TD";
                bool MannualSrnohascome = false;
                string engine_Dcode = string.Empty;
            
                //VALIDATION ENGINE
                if (!string.IsNullOrEmpty(data.ENGINE))
                {

                    
                    if (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y")
                    {
                        if(string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["ENGINE"])))
                        {
                            return "Engine dcode not found in master";
                        }
                        query = string.Format(@"select count(*) from xxes_sft_settings where plant_code='{0}' and 
                    family_code='{1}' and parameterinfo='{2}' and status='SRNO'",
                    data.Plant, enggfamily.Trim(), Convert.ToString(dt.Rows[0]["ENGINE"]).Trim());
                        if (fun.CheckExits(query))
                        {
                            MannualSrnohascome = true;
                            engine_Dcode = fun.get_Col_Value("select dcode from xxes_print_serials where srno='" + data.ENGINE.Trim().ToUpper() + "' and dcode='" + Convert.ToString(dt.Rows[0]["ENGINE"]).Trim().ToUpper() + "'");
                        }
                        else
                        {
                            MannualSrnohascome = false;
                            engine_Dcode = fun.get_Col_Value("select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='" + data.ENGINE.Trim().ToUpper() + "' and plant_code='" + data.Plant.Trim().ToUpper() + "'");
                        }
                        if(string.IsNullOrEmpty(engine_Dcode))
                        {
                            return "Engine dcode not found for engine serial no..";
                        }
                        if (engine_Dcode != Convert.ToString(dt.Rows[0]["ENGINE"]))
                        {
                            return "Engine dcode mismatch..";
                        }
                        if (MannualSrnohascome)
                            query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE ENGINE_SRLNO = '{0}' AND JOBID <> '{1}' and  ENGINE='{2}'", data.ENGINE, data.ScanJOB, engine_Dcode.Trim().ToUpper());
                        else
                            query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE ENGINE_SRLNO = '{0}' AND JOBID <> '{1}'", data.ENGINE, data.ScanJOB);
                        string engCount = Convert.ToString(fun.get_Col_Value(query));
                        if (!string.IsNullOrEmpty(engCount))
                        {
                            return "Engine serial no already found on jobid " + engCount;
                        }
                    }
                }

                //VALIDATION REAR AXEL
                if (!string.IsNullOrEmpty(data.REARAXEL))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE REARAXEL_SRLNO = '{0}' AND JOBID <> '{1}'", data.REARAXEL, data.ScanJOB);
                    string rearAxelCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(rearAxelCount))
                    {
                        return "Rear Axel serial no already found on jobid " + rearAxelCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y")
                    {
                        query = string.Format(@"SELECT ITEM_CODE FROM PRINT_SERIAL_NUMBER WHERE SERIAL_NUMBER = '{0}'", Convert.ToString(dt.Rows[0]["REARAXEL_SRLNO"]));
                        string rearAxel_Dcode = fun.get_Col_Value(query);
                        if (rearAxel_Dcode != Convert.ToString(dt.Rows[0]["REARAXEL"]))
                        {
                            return "Rear Axel dcode mismatch..";
                        }
                    }
                }

                //VALIDATION TRANSMISSION
                if (!string.IsNullOrEmpty(data.TRANSMISSION))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE TRANSMISSION_SRLNO = '{0}' AND JOBID <> '{1}'", data.TRANSMISSION, data.ScanJOB);
                    string transmissionCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(transmissionCount))
                    {
                        return "Transmission serial no already found on jobid " + transmissionCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y")
                    {
                        query = string.Format(@"SELECT ITEM_CODE FROM PRINT_SERIAL_NUMBER WHERE SERIAL_NUMBER = '{0}'", Convert.ToString(dt.Rows[0]["TRANSMISSION_SRLNO"]));
                        string transmission_Dcode = fun.get_Col_Value(query);
                        if (transmission_Dcode != Convert.ToString(dt.Rows[0]["TRANSMISSION"]))
                        {
                            return "Transmission dcode mismatch..";
                        }
                    }
                }

                //VALIDATION BACKEND
                if (!string.IsNullOrEmpty(data.FCId))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE BACKEND_SRLNO = '{0}' AND JOBID <> '{1}'", data.FCId, data.ScanJOB);
                    string backendCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(backendCount))
                    {
                        return "Backend serial  serial no already found on jobid " + backendCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y")
                    {
                        if (data.Plant == "T05")
                        {
                            query = string.Format(@"SELECT ITEM_CODE FROM PRINT_SERIAL_NUMBER WHERE SERIAL_NUMBER = '{0}'", Convert.ToString(dt.Rows[0]["BACKEND_SRLNO"]));
                            string backend_Dcode = fun.get_Col_Value(query);
                            if (backend_Dcode != Convert.ToString(dt.Rows[0]["BACKEND"]))
                            {
                                return "Backend dcode mismatch..";
                            }
                        }
                    }
                }

                //VALIDATION HYDRAULIC
                if (!string.IsNullOrEmpty(data.HYDRAULIC))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE HYDRALUIC_SRLNO = '{0}' AND JOBID <> '{1}'", data.HYDRAULIC, data.ScanJOB);
                    string hydraulicCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(hydraulicCount))
                    {
                        return "Hydraulic serial no already found on jobid " + hydraulicCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y")
                    {
                        query = string.Format(@"SELECT DCODE FROM XXES_PRINT_SERIALS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND OFFLINE_KEYCODE = '{2}' AND SRNO = '{3}'", data.Plant, data.Family, "HYD", Convert.ToString(dt.Rows[0]["HYDRALUIC_SRLNO"]));
                        string hydraulicDcode = fun.get_Col_Value(query);
                        if (hydraulicDcode != Convert.ToString(dt.Rows[0]["HYDRAULIC"]))
                        {
                            return "Hydraulic dcode mismatch..";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data.HYDRAULIC))
                        {
                            if (data.HYDRAULIC.Contains('$'))
                            {
                                string hydraulicDcode = data.HYDRAULIC.Trim().ToUpper().Substring(9, data.HYDRAULIC.Trim().Length - 9).Trim();
                                if (hydraulicDcode != Convert.ToString(dt.Rows[0]["HYDRAULIC"]))
                                {
                                    return "Hydraulic dcode mismatch..";
                                }
                            }
                            else
                            {
                                return "Invalid hydraulic..";
                            }
                        }
                    }
                }

                //VALIDATION STEERING MOTOR
                if (!string.IsNullOrEmpty(data.STERRINGMOTOR))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE STEERING_MOTOR_SRLNO = '{0}' AND JOBID <> '{1}'", data.STERRINGMOTOR, data.ScanJOB);
                    string steeringMotorCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(steeringMotorCount))
                    {
                        return "Steering Motor serial no already found on jobid " + steeringMotorCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y")
                    {
                        string steeringMotorDcode = SplitDcode(data.STERRINGMOTOR, "POWER_STMOTOR");
                        if (steeringMotorDcode != Convert.ToString(dt.Rows[0]["STEERING_MOTOR"]))
                        {
                            return "Steering Motor dcode mismatch..";
                        }
                    }
                }

                //VALIDATION HYDRAULIC PUMP
                if (!string.IsNullOrEmpty(data.HYDRAULIC_PUMP))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE HYD_PUMP_SRLNO = '{0}' AND JOBID <> '{1}'", data.HYDRAULIC_PUMP, data.ScanJOB);
                    string hydraulicPumpCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(hydraulicPumpCount))
                    {
                        return "Hydraulic Pump serial no already found on jobid " + hydraulicPumpCount;
                        //if (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y")
                        //{
                        //    string hydraulicPumpDcode = SplitDcode(data.STERRINGMOTOR, "POWER_STMOTOR");
                        //    if (hydraulicPumpDcode != Convert.ToString(dt.Rows[0]["STEERING_MOTOR"]))
                        //    {
                        //        return "Steering Motor dcode mismatch..";
                        //    }
                        //}
                    }
                }

                //VALIDATION STARTER MOTOR
                if (!string.IsNullOrEmpty(data.STARTERMOTOR))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE STARTER_MOTOR_SRLNO = '{0}' AND JOBID <> '{1}'", data.STARTERMOTOR, data.ScanJOB);
                    string starterMotorCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(starterMotorCount))
                    {
                        return "Steering Motor serial no already found on jobid " + starterMotorCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y")
                    {
                        string steeringMotorDcode = SplitDcode(data.STARTERMOTOR, "START_MOTOR");
                        if (steeringMotorDcode != Convert.ToString(dt.Rows[0]["STARTER_MOTOR"]))
                        {
                            return "Starter Motor dcode mismatch..";
                        }
                    }
                }

                //VALIDATION ALTERNATOR
                if (!string.IsNullOrEmpty(data.ALTERNATOR))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE ALTERNATOR_SRLNO = '{0}' AND JOBID <> '{1}'", data.ALTERNATOR, data.ScanJOB);
                    string alternatorCount = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(alternatorCount))
                    {
                        return "Alternator serial no already found on jobid " + alternatorCount;
                    }
                    if (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y")
                    {
                        string steeringMotorDcode = SplitDcode(data.ALTERNATOR, "ALT");
                        if (steeringMotorDcode != Convert.ToString(dt.Rows[0]["ALTERNATOR"]))
                        {
                            return "Alternator dcode mismatch..";
                        }
                    }
                }

                //VALIDATION STARTER MOTOR
                if (!string.IsNullOrEmpty(data.FIP))
                {
                    query = string.Format(@"SELECT JOBID FROM XXES_JOB_STATUS WHERE FIPSRNO = '{0}' AND JOBID <> '{1}'", data.FIP, data.ScanJOB);
                    string FIPSRNO_Count = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(FIPSRNO_Count))
                    {
                        return "FIPSR already found on jobid " + FIPSRNO_Count;
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                fun.WriteLog(ex.Message);
                return ex.Message;
            }
            finally { }
        }

        public string SplitDcode(string barcode, string type)
        {
            string dcode = string.Empty;
            try
            {
                if (barcode.Contains("$"))
                {
                    if (type.Equals("RADIATOR") || type.Equals("ALT") || type.Equals("START_MOTOR") || type.Equals("POWER_STMOTOR"))
                        dcode = barcode.Split('$')[1].Trim();
                    else if (type.Equals("ALT"))
                        dcode = barcode.Split('$')[0].Trim();
                }
                else
                    dcode = string.Empty;
            }
            catch (Exception ex)
            {

            }
            return dcode;
        }

        public string getItemCode(string srlno)
        {
            try
            {
                return fun.get_Col_Value("select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='" + srlno.Trim().ToUpper() + "'");
            }
            catch { throw; }
            finally { }
        }
    }
}
