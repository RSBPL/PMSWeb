using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class AddControllerController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: AddController
        public ActionResult Index()
        {
            if(string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login","Account");
            }
            else
            {
                
                return View();
            }
           
        }

        public PartialViewResult Grid(AddController add)
        {
            if(!string.IsNullOrEmpty(add.Plant) && !string.IsNullOrEmpty(add.Family))
            {
                ViewBag.DataSource = fun.GridAddController(add);
            }
            return PartialView();
        }
        

        public PartialViewResult BindPlant()
        {
            ViewBag.Plant = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult BindFamily(string Plant)
        {
            if(!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }

        public PartialViewResult BindStage()
        {
            ViewBag.Stage = new SelectList(fun.Fill_Stage_Name(), "Value", "Text");
            return PartialView();
        }

        public JsonResult Save(AddController add)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(add.Plant))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(string.IsNullOrEmpty(add.Family))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.ID))
                {
                    msg = "Please Select Id..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.Stage))
                {
                    msg = "Please Select Stage..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.Mode))
                {
                    msg = "Please Select Mode..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.ReadingChannel))
                {
                    msg = "Please Select Reading Channel..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.IPAddress))
                {
                    msg = "Please Enter Valid  IP Address..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(add.Port))
                {
                    msg = "Please Enter Valid  Port..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_Controllers where IP_ADDR='" + add.IPAddress.ToUpper().Trim() + "' and PORT ='" + add.Port.ToUpper().Trim() + "' and DID='" + add.Port.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "IP Address with this Port already exists..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(fun.InsertAddController(add))
                    {
                        msg = "Data Saved successfully...";
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

        public JsonResult Delete(AddController add)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (fun.DeleteAddController(add))
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
        //public JsonResult Update(AddController add)
        //{
        //    string msg = string.Empty, mstType = string.Empty, status = string.Empty;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(add.Plant))
        //        {
        //            msg = "Please Select Plant..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.Family))
        //        {
        //            msg = "Please Select Family..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.ID))
        //        {
        //            msg = "Please Select Id..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.Stage))
        //        {
        //            msg = "Please Select Stage..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.Mode))
        //        {
        //            msg = "Please Select Mode..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.ReadingChannel))
        //        {
        //            msg = "Please Select Reading Channel..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.IPAddress))
        //        {
        //            msg = "Please Enter Valid  IP Address..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(add.Port))
        //        {
        //            msg = "Please Enter Valid  Port..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //       else
        //        {
        //            if (fun.UpdateAddController(add))
        //            {
        //                msg = "Updated Data successfully...";
        //                mstType = "alert-success";
        //                status = "success";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        fun.LogWrite(ex);
        //    }
        //    finally { }
        //    var result = new { Msg = msg, ID = mstType, validation = status };
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
    }
}