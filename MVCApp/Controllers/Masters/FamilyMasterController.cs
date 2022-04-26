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
    public class FamilyMasterController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        // GET: FamilyMaster
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

        public PartialViewResult Grid()
        {

             ViewBag.DataSource = fun.GridFamilyData();

            return PartialView();
        }

        public JsonResult Save(PlantAndFamily data)
        {
            
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                var tuple = Validation.ValidateFamilyMaster("FamilyMaster", "Save", data);

                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                
                string noofstages = data.NoOfStages.Trim();
              
                Int64 j;
                if(!Int64.TryParse(noofstages ,out j))
                {
                    msg = "No. of stages should be numeric only ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string orgId = data.ORGId.Trim();

                Int64 k;
                if (!Int64.TryParse(orgId, out k))
                {
                    msg = "Organisation Id should be numeric only ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_FAMILY_MASTER WHERE FAMILY_CODE = '" + data.FamilyCode.ToUpper().Trim()+"'"))>0)
                {
                    msg = "Family Code already exist..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                   
                    if (fun.InsertFamily(data))
                    {
                        msg = "Saved successfully...";
                        mstType = "alert-success";
                        status = "success";
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

        public JsonResult Update(PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                var tuple = Validation.ValidateFamilyMaster("FamilyMaster", "Update", data);

                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                
                string noofstages = data.NoOfStages.Trim();

                Int64 j;
                if (!Int64.TryParse(noofstages, out j))
                {
                    msg = "No. of stages should be numeric only .. ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string orgId = data.ORGId.Trim();
                Int64 k;
                if (!Int64.TryParse(orgId, out k))
                {
                    msg = "Organisation Id should be numeric only ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (fun.UpdateFamily(data))
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

        public JsonResult Delete(PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (fun.DeleteFamilyMaster(data))
                {
                    msg = Validation.str23;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }

            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


    }
}