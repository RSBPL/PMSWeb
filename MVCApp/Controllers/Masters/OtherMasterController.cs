using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class OtherMasterController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: OtherMaster
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var now = DateTimeOffset.Now;
                var RESULT = Enumerable.Range(0, 6).Select(i => now.AddMonths(i).ToString("MMM-yyyy"));
                ViewBag.Months = new SelectList(RESULT);
                return View();
            }
        }

        public PartialViewResult Grid(string Type, string Plant)
        {
            if(!string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Plant))
            {
                ViewBag.DataSource = fun.GridOtherMaster(Type, Plant);
                
            }

            return PartialView();
        }     
        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public JsonResult Save(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty; 
            try
            {
               
                if (string.IsNullOrEmpty(suffix.Plant))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
              
                if (string.IsNullOrEmpty(suffix.Type))
                {
                    msg = "Please Select Type";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(string.IsNullOrEmpty(suffix.MonthYear))
                {
                    msg = "Please Select Month";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(suffix.MyCode))
                {
                    msg = "Please Enter Suffix";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUFFIX_CODE WHERE MY_CODE = '" + suffix.MyCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Suffix already exist..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(fun.InsertOtherSuffix(suffix))
                    {
                        msg = "Insert Successfully...";
                        mstType = "alert-success";
                        status = "success";

                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteOtherSuffix(suffix))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //*******************************************Export*****************************************//
        public PartialViewResult BindPlantExport()
        {
            ViewBag.PlantExport = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult GridExport(string PlantExport)
        {
            if (!string.IsNullOrEmpty(PlantExport))
            {
                ViewBag.DataSourceE = fun.GridOtherMasterExport(PlantExport);

            }

            return PartialView();
        }
        public JsonResult SaveExport(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(suffix.PlantExport))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(suffix.MonthYearExport))
                {
                    msg = "Please Select Month";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(suffix.MyCodeExport))
                {
                    msg = "Please Enter Suffix";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUFFIX_CODE WHERE MY_CODE = '" + suffix.MyCodeExport.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Suffix already exist..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    if (fun.InsertOtherSuffixExport(suffix))
                    {
                        msg = "Insert Successfully...";
                        mstType = "alert-success";
                        status = "success";

                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteExport(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteOtherSuffixExport(suffix))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //******************************************Tyre Manufacture********************************//

        public JsonResult SaveTyre(OtherSuffix suffix)
        {
            DateTime time = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;

            try
            {
                if(string.IsNullOrEmpty(suffix.TyreName))
                {
                    msg = "Please enter Tyre Manufacturer name..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName.ToUpper().Trim() + "' and PARAMVALUE='TYRE_MAN_NAME'")) > 0) 

                {
                    msg = "Tyre Manufacturer name already exists";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,CREATED_BY,CREATED_DATE) values('" + suffix.TyreName.ToUpper().Trim() + "','TYRE_MAN_NAME' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)"; 
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Data Saved successfully...";
                        mstType = "alert-success";
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result , JsonRequestBehavior.AllowGet);
            
        }
        public PartialViewResult GridTyre()
        {
            DataTable dt = new DataTable();
            
            try
            {
                query = string.Format(@"select PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                //query = "select * from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO";
                //dt = fun.returnDataTable(query);
            }

            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSourceT = dt;
            return PartialView();
        }
        public JsonResult DeleteTyre(OtherSuffix suffix)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

               // query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName + "' and AUTOID='" + suffix.AutoId + "'and PARAMVALUE='TYRE_MAN_NAME'";
                query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + suffix.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //******************************************Battery Manufacture********************************//

        public JsonResult SaveBattery(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(suffix.BatteryName))
                {
                    msg = "Please Enter Battery Manufacture Name..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO ='" + suffix.BatteryName.ToUpper().Trim() + "' and PARAMVALUE='BATT_MAN_NAME'")) > 0)
                {
                    msg = "Battery Manufacturer name already exists";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,CREATED_BY,CREATED_DATE) values('" + suffix.BatteryName.ToUpper().Trim() + "' ,'BATT_MAN_NAME','" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)";
                    if(fun.EXEC_QUERY(query))
                    {
                        msg = "Data Save Successfully..";
                        mstType = "alert-success";
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult GridBattery()
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                //query = "select * from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' order by PARAMETERINFO";
                //dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {

                fun.LogWrite(ex);
            }
            ViewBag.DataSourceB = dt;
            return PartialView();
        }
        public JsonResult DeleteBattery(OtherSuffix suffix)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                // query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName + "' and AUTOID='" + suffix.AutoId + "'and PARAMVALUE='TYRE_MAN_NAME'";
                query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + suffix.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //******************************************Shift Details********************************//

        public PartialViewResult GridShift()
        {
            ViewBag.DataSourceS = fun.GridShiftDetail();
            return PartialView();
        }
        public JsonResult SaveShift(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(suffix.ShiftCode))
                {
                    msg = "Please Enter Shift Code..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(Convert.ToString(suffix.StartTime)))
                {
                    msg = "Please Slect Start Time..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(Convert.ToString(suffix.EndTime)))
                {
                    msg = "Please Slect End Time..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SHIFT_MASTER where SHIFTCODE='" + suffix.ShiftCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Shift Code Already Exist..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(fun.InsertShiftDetail(suffix))
                    {
                        msg = "Data Insert Successfully";
                        mstType = "alert-success";
                        status = "success";
                    }
                }
               
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteShift(OtherSuffix suffix)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteShiftDetail(suffix))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //******************************************Dummy Battery DeCode********************************//

        public PartialViewResult BindPlantBattery()
        {
            ViewBag.PlantBattery = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindBatteryDecode(string PlantBattery )
        {
            if(!string.IsNullOrEmpty(PlantBattery))
            {
                ViewBag.BatteryDecode = new SelectList(fun.Fill_BatteryDeCode_Name(PlantBattery), "Value", "Text");
            }

            return PartialView();
        }
        public PartialViewResult GridBatteryDeCode()
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SFT_SETTINGS where status='BATDUMMY' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                //query = "select * from XXES_SFT_SETTINGS where status='BATDUMMY' order by PARAMETERINFO";               
                //dt = fun.returnDataTable(query);

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSourceDecode = dt;
            return PartialView();
        }
        public JsonResult SaveBatteryDecode(OtherSuffix suffix)
        {
           
            
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(suffix.PlantBattery))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(suffix.BatteryDecode))
                {
                    msg = "Please Select Item Code..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                {
                    query = "insert into XXES_SFT_SETTINGS(plant_code,parameterinfo,status,CREATED_BY,CREATED_DATE) values('" + suffix.PlantBattery.ToUpper().Trim() + "','" + suffix.BatteryDecode.ToUpper().Trim() + "','BATDUMMY' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)";

                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Data Saved successfully...";
                        mstType = "alert-success";
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteBatteryDeCode(OtherSuffix suffix)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                // query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName + "' and AUTOID='" + suffix.AutoId + "'and PARAMVALUE='TYRE_MAN_NAME'";
                query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + suffix.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //******************************************Dummy Battery SerialNo********************************//

        public PartialViewResult BindPlantSrl()
        {
            ViewBag.PlantSrl = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindBatterySrl(string PlantSrl)
        {
            if (!string.IsNullOrEmpty(PlantSrl))
            {
                ViewBag.BatterySrl = new SelectList(fun.Fill_BatteryDeCode_Name(PlantSrl), "Value", "Text");
            }

            return PartialView();
        }
        public PartialViewResult GridBatterySrl()
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SFT_SETTINGS where status='BATDUMMYNO' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                //query = "select * from XXES_SFT_SETTINGS where status='BATDUMMYNO' order by PARAMETERINFO";
                //dt = fun.returnDataTable(query);

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSourceSrl = dt;
            return PartialView();
        }
        public JsonResult SaveBatterySrl(OtherSuffix suffix)
        {         
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(suffix.PlantSrl))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(suffix.BatterySrl))
                {
                    msg = "Please Select Item Code..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(suffix.DammySrNo))
                {
                    msg = "Please Enter Serial No..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where paramvalue='" + suffix.DammySrNo.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Serial No Already Exist..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    {
                        query = "insert into XXES_SFT_SETTINGS(plant_code,parameterinfo,paramvalue,status,CREATED_BY ,CREATED_DATE) values('" + suffix.PlantSrl.ToUpper().Trim() + "','" + suffix.BatterySrl.ToUpper().Trim() + "','" + suffix.DammySrNo.ToUpper().Trim() + "','BATDUMMYNO' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)";                        

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved successfully...";
                            mstType = "alert-success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteBatterySrl(OtherSuffix suffix)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                // query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName + "' and AUTOID='" + suffix.AutoId + "'and PARAMVALUE='TYRE_MAN_NAME'";
                query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + suffix.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //******************************************FCode Tyres********************************//
        public PartialViewResult BindPlantFCode()
        {
            ViewBag.PlantFcode = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindFamilyFCode(string Plant)
        {
            if(!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }
       
        [HttpPost]
        public JsonResult BindFCode(OtherSuffix data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PlantFcode) || string.IsNullOrEmpty(data.FamilyFcode))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PlantFcode).Trim().ToUpper(), Convert.ToString(data.FamilyFcode).Trim().ToUpper());

                DataTable dt = new DataTable();

                query = string.Format(@"SELECT M.ITEM_CODE || '#' || M.ITEM_DESCRIPTION  TEXT, M.ITEM_CODE  VALUE FROM XXES_ITEM_MASTER m WHERE M.PLANT_CODE='" + data.PlantFcode.Trim().ToUpper() + "' AND M.FAMILY_CODE= '"+ data.FamilyFcode.Trim().ToUpper() + "'");
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["TEXT"].ToString(),
                            Value = dr["VALUE"].ToString(),
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

        public JsonResult BindTyre(OtherSuffix data)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(data.PlantFcode) || string.IsNullOrEmpty(data.FamilyFcode))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(data.PlantFcode).Trim().ToUpper(), Convert.ToString(data.FamilyFcode).Trim().ToUpper());

                DataTable dt = new DataTable();

                query = string.Format(@"SELECT DISTINCT PARAMETERINFO FROM XXES_SFT_SETTINGS WHERE PARAMVALUE='TYRE_MAN_NAME'");

                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["PARAMETERINFO"].ToString(),
                            Value = dr["PARAMETERINFO"].ToString(),
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

        public JsonResult SaveFcodetyre(OtherSuffix data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PlantFcode))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FamilyFcode))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Fcode))
                {
                    msg = "Please Select FCode..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Tyre))
                {
                    msg = "Please Select Tyre..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where TYRE ='" + data.Tyre.ToUpper().Trim() + "' and ITEMCODE  ='" + data.Fcode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "The selected FCode is already exists for same Tyre ..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    {
                        //query = "insert into XXES_SFT_SETTINGS(plant_code,parameterinfo,paramvalue,status,CREATED_BY ,CREATED_DATE) values('" + suffix.PlantSrl.ToUpper().Trim() + "','" + suffix.BatterySrl.ToUpper().Trim() + "','" + suffix.DammySrNo.ToUpper().Trim() + "','BATDUMMYNO' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)";
                        query = "insert into XXES_SFT_SETTINGS(plant_code,family_code,ITEMCODE ,TYRE ,parameterinfo,CREATED_BY ,CREATED_DATE) values('" + data.PlantFcode.ToUpper().Trim() + "','" + data.FamilyFcode.ToUpper().Trim() + "','" + data.Fcode.ToUpper().Trim() + "','" + data.Tyre.ToUpper().Trim() + "','FCODE_TYRES' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)";

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved successfully...";
                            mstType = "alert-success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateFcodetyre(OtherSuffix data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PlantFcode))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FamilyFcode))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Fcode))
                {
                    msg = "Please Select FCode..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Tyre))
                {
                    msg = "Please Select Tyre..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }                                         
                
                {
                        //query = "update from XXES_SFT_SETTINGS SET PLANT_CODE = '" + data.PlantFcode.ToUpper().Trim() + "', FAMILY_CODE = '" + data.FamilyFcode.ToUpper().Trim() + "'  where AUTOID ='" + data.AutoId + "'";
                        query = string.Format(@"update XXES_SFT_SETTINGS SET PLANT_CODE = '" + data.PlantFcode.ToUpper().Trim() + "', FAMILY_CODE = '" + data.FamilyFcode.ToUpper().Trim() + "', ITEMCODE = '" + data.Fcode.ToUpper().Trim() + "', TYRE = '" + data.Tyre.ToUpper().Trim() + "', UPDATED_BY = '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "', UPDATED_DATE = SYSDATE  where autoid = '" + data.AutoId + "'");

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Updated successfully...";
                            mstType = "alert-success";
                        }
                    
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GridFCodeTyre(OtherSuffix otherSuffix)
        {
            DataTable dt = new DataTable();

            try
            {
                query = string.Format(@"select ITEMCODE,m.ITEM_DESCRIPTION,  TYRE,s.PLANT_CODE,s.FAMILY_CODE,s.CREATED_BY,
                TO_CHAR(s.CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,s.UPDATED_BY, TO_CHAR(s.UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS UPDATED_DATE,s.AUTOID 
                from XXES_SFT_SETTINGS s JOIN XXES_ITEM_MASTER m ON s.itemcode=m.item_code 
                AND s.plant_code=m.plant_code AND s.family_code=m.family_code
                 where PARAMETERINFO='FCODE_TYRES' AND s.plant_code='{0}' AND s.family_code='{1}'
                 order BY  S.ITEMCODE", otherSuffix.PlantFcode,otherSuffix.FamilyFcode);
                dt = fun.returnDataTable(query);
               
            }

            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSourceFCodeTyre = dt;
            return PartialView();
        }
        public JsonResult DeleteFCode(OtherSuffix suffix)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_SFT_SETTINGS where TYRE ='" + suffix.Tyre.ToUpper().Trim() + "' and PARAMETERINFO='FCODE_TYRES'")) > 0)
                {
                    msg = "Already mapped ,Can't Deleted ..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='" + suffix.TyreName + "' and AUTOID='" + suffix.AutoId + "'and PARAMVALUE='TYRE_MAN_NAME'";
                    query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + suffix.AutoId + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Record deleted successfully ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
