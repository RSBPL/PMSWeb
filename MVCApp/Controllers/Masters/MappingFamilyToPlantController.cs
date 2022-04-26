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
    
    public class MappingFamilyToPlantController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: MappingFamilyToPlant
        public ActionResult Index()
        {
            if(string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else 
            {
                return View();
            }
           
        }
        public PartialViewResult Grid()
        {
           
            ViewBag.DataSource = fun.GridFamilyPlantData();
                          
            return PartialView();
        }
        public PartialViewResult BindFamilyPlant()
        {
           
             ViewBag.PlantFamily = new SelectList(fun.Fill_Plant_Family_Name(), "Value", "Text");

            return PartialView();
        }
        public PartialViewResult Checkbox()
        {

            ViewBag.dataSource = fun.Fill_Family_CheckBox();

            return PartialView();
        }

        public JsonResult Save(MappingFamilyToPlant mapping)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(mapping.PlantCode))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(mapping.FamilyCode))
                {
                    msg = "Please Select any CheckBox..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string valresult = string.Empty;
                    if (fun.ValidatePlantFamily(mapping,out valresult))
                    {
                        msg = valresult;
                        mstType = "alert-success";
                        status = "success";
                    }
                    else
                    {
                        if (fun.InsertMappingFamilyPlant(mapping))
                        {
                            msg = "Saved Successfully";
                            mstType = "alert-success";
                            status = "success";
                        }
                    }
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


        //public JsonResult Update(MappingFamilyToPlant mapping)
        //{
        //    string msg = string.Empty, mstType = string.Empty, status = string.Empty;
        //    try
        //    {
        //        if(string.IsNullOrEmpty(mapping.PlantCode))
        //        {
        //            msg = "Please Select Plant..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        if (string.IsNullOrEmpty(mapping.FamilyCode))
        //        {
        //            msg = "Please Select any CheckBox..";
        //            mstType = "alert-danger";
        //            status = "error";
        //            var resul = new { Msg = msg, ID = mstType, validation = status };
        //            return Json(resul, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            string valresult = string.Empty;
        //            if (fun.ValidatePlantFamily(mapping, out valresult))
        //            {
        //                msg = valresult;
        //                mstType = "alert-success";
        //                status = "success";
        //            }
        //            else
        //            {
        //                if (fun.UpdateMappingFamilyPlant(mapping))
        //                {
        //                    msg = "Update Successfully";
        //                    mstType = "alert-success";
        //                    status = "success";
        //                }
        //            }
                    
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        msg = ex.Message;
        //        mstType = "alert-success";
        //        status = "error";
        //        var resul = new { Msg = msg, ID = mstType, validation = status };
        //        return Json(resul, JsonRequestBehavior.AllowGet);
        //    }
        //    finally { }
        //    var result = new { Msg = msg, ID = mstType, validation = status };
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        
    }

}