using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    public class BackendMappingController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        DataSet ds;
        Function fun = new Function();
        string query = ""; string ORGID = "";
        
        // GET: BackendMapping
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.FAMILY_CODE = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindBackend(BackendModel BM)
        {
            if (!string.IsNullOrEmpty(BM.PLANT_CODE) && !string.IsNullOrEmpty(BM.FAMILY_CODE))
            {
                ViewBag.BACKEND = new SelectList(fun.Fill_BACKEND(BM), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindHydraulic(BackendModel BM)
        {
            if (!string.IsNullOrEmpty(BM.PLANT_CODE) && !string.IsNullOrEmpty(BM.FAMILY_CODE))
            {
                ViewBag.HYDRAULIC = new SelectList(fun.Fill_HYDRAULIC(BM), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindTransmission(BackendModel BM) 
        {
            if (!string.IsNullOrEmpty(BM.PLANT_CODE) && !string.IsNullOrEmpty(BM.FAMILY_CODE))
            {
                ViewBag.TRANSMISSION = new SelectList(fun.Fill_Transmission(BM), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindRearAxel(BackendModel BM)
        {
            if(!string.IsNullOrEmpty(BM.PLANT_CODE) && !string.IsNullOrEmpty(BM.FAMILY_CODE))
            {
                ViewBag.REARAXEL = new SelectList(fun.Fill_RearAxel(BM), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult Grid(BackendModel data)
        {
            if (!string.IsNullOrEmpty(data.PLANT_CODE) && !string.IsNullOrEmpty(data.FAMILY_CODE))
            {
                ViewBag.DataSource = fun.GridBackendMaster(data);
            }

            return PartialView();
        }
        public JsonResult SaveBackendMaster(BackendModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = "Please Select Plant";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = "Please Select Family";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.BACKEND))
                {
                    msg = "Please Select Engine";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.HYDRAULIC))
                {
                    msg = "Please Select Cylinder Block";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.TRANSMISSION))
                {
                    msg = "Please Select Cylinder Head";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.REARAXEL))
                {
                    msg = "Please Select Connecting Rod";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string stageValue = data.BACKEND_DESC.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_BACKEND_MASTER WHERE PLANT_CODE = '" + data.PLANT_CODE.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.FAMILY_CODE.ToUpper().Trim() + "' AND BACKEND = '" + subs[0].ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    //status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (fun.InsertBackendMaster(data))
                {
                    msg = "Saved successfully...";
                    mstType = "alert-success";
                }


            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);




        }
        public JsonResult Update(BackendModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = "Please Fill Plant Code ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = "Please Fill Family Code ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
             
                if (string.IsNullOrEmpty(data.BACKEND))
                {
                    msg = "Please Fill Engine ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.HYDRAULIC))
                {
                    msg = "Please Fill Fuel Pump ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.TRANSMISSION))
                {
                    msg = "Please Fill Cynlinder Block ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.REARAXEL))
                {
                    msg = "Please Fill Cynlinder Head ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
              
                if (fun.UpdateBackendMaster(data))
                {
                    msg = "Update successfully...";
                    mstType = "alert-success";
                    status = "success";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);

            }

            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult Delete(BackendModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                query = "DELETE FROM XXES_BACKEND_MASTER WHERE AUTOID = '" + data.AUTOID + "'";

                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record Deleted successfully...";
                    mstType = "alert-danger";
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

            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}