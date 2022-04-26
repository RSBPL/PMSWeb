using MVCApp.CommonFunction;
using MVCApp.Models;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Admin
{
    [Authorize]
    public class MessagesController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: Messages
        public ActionResult Index()
        {
            if(string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
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

        public JsonResult Save(Message data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(data.Plant))
                {
                    msg = "Please Select Plant...";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(string.IsNullOrEmpty(data.Family))
                {
                    msg = "Please Select Family...";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.MsgType))
                {
                    msg = "Please Select MsgType...";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                if (string.IsNullOrEmpty(data.MessageBox))
                {
                    
                    msg = "Please write a somthing in Message Box...";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
               
                if (data.MsgType == "Always")
                {
                    fun.EXEC_QUERY("delete from XXES_SFT_SETTINGS where PARAMETERINFO = 'MESSAGE_ALWAYS'  and plant_code = '" + data.Plant.ToUpper().Trim() + "' and family_code = '" + data.Family.ToUpper().Trim() + "'");
                    //query = string.Format(@"insert into XXES_SFT_SETTINGS(plant_code,family_code,parameterinfo,TO_CHAR(PARAMVALUE,'DD-MM-YYYY') AS PARAMVALUE,TO_CHAR(STATUS,'DD-MM-YYYY') AS  STATUS, DESCRIPTION) values('" + data.Plant.ToUpper().Trim() + "','" + data.Family.ToUpper().Trim() + "','MESSAGE_ALWAYS' ,'" + data.FromDate + "','" + data.ToDate + "','" + data.MessageBox.ToUpper().Trim() + "')");
                    query = string.Format(@"insert into XXES_SFT_SETTINGS(plant_code,family_code,parameterinfo,PARAMVALUE ,STATUS,DESCRIPTION) values('" + data.Plant.ToUpper().Trim() + "','" + data.Family.ToUpper().Trim() + "','MESSAGE_ALWAYS' ,'" + data.FromDate + "','" + data.ToDate + "','" + data.MessageBox.ToUpper().Trim() + "')");
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Data Saved successfully...";
                        mstType = "alert-success";
                    }
                }
                else if(data.MsgType == "Period")
                {
                    fun.EXEC_QUERY("delete from XXES_SFT_SETTINGS where PARAMETERINFO = 'MESSAGE_PERIOD'  and plant_code = '" + data.Plant.ToUpper().Trim() + "' and family_code = '" + data.Family.ToUpper().Trim() + "'");
                    //query = string.Format(@"insert into XXES_SFT_SETTINGS(plant_code,family_code,parameterinfo,TO_CHAR(PARAMVALUE,'DD-MM-YYYY') AS PARAMVALUE,TO_CHAR(STATUS,'DD-MM-YYYY') AS  STATUS,DESCRIPTION) values('" + data.Plant.ToUpper().Trim() + "','" + data.Family.ToUpper().Trim() + "','MESSAGE_PERIOD' ,'" + data.FromDate + "','" + data.ToDate + "','" + data.MessageBox.ToUpper().Trim() + "')");
                    query = string.Format(@"insert into XXES_SFT_SETTINGS(plant_code,family_code,parameterinfo,PARAMVALUE ,STATUS,DESCRIPTION) values('" + data.Plant.ToUpper().Trim() + "','" + data.Family.ToUpper().Trim() + "','MESSAGE_PERIOD' ,'" + data.FromDate + "','" + data.ToDate + "','" + data.MessageBox.ToUpper().Trim() + "')");
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
        public PartialViewResult Grid()
        {
            DataTable dt = new DataTable();

            try
            {
                //query = string.Format(@"SELECT PARAMETERINFO,TO_CHAR(PARAMVALUE,'DD-MM-YYYY') AS PARAMVALUE,TO_CHAR(STATUS,'DD-MM-YYYY') AS  STATUS,  DESCRIPTION,PLANT_CODE,FAMILY_CODE,AUTOID from XXES_SFT_SETTINGS 
                //        where PARAMETERINFO = 'MESSAGE_ALWAYS' OR PARAMETERINFO= 'MESSAGE_PERIOD'");
                query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,AUTOID from XXES_SFT_SETTINGS 
                        where PARAMETERINFO = 'MESSAGE_ALWAYS' OR PARAMETERINFO= 'MESSAGE_PERIOD'");
                dt = fun.returnDataTable(query);
            }

            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSource = dt;
            return PartialView();
        }

        [HttpGet]
        public JsonResult DisplayMessage()
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,AUTOID from XXES_SFT_SETTINGS 
                        where PARAMETERINFO = 'MESSAGE_ALWAYS' OR PARAMETERINFO= 'MESSAGE_PERIOD' AND CURRENT_DATE - 1  <= STATUS");
                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(JsonConvert.SerializeObject(dt, Formatting.None), JsonRequestBehavior.AllowGet); ;
        }

        public JsonResult Delete(Message data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                query = "delete from XXES_SFT_SETTINGS where AUTOID ='" + data.AutoId + "'";
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
    }
}