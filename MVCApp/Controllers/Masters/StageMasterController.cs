using MVCApp.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCApp.Models;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class StageMasterController : Controller
    {
        Function fun = new Function();
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

        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_All_Family(Plant), "Value", "Text");
            }
            return PartialView();
        }

        public PartialViewResult BindStageId(string Family)
        {
            if (!string.IsNullOrEmpty(Family))
            {
                ViewBag.StageId = new SelectList(fun.FillStageIdByFamily(Family), "Value", "Text");
            }
            return PartialView();
        }

        public PartialViewResult BindStage(string Family)
        {
            if (!string.IsNullOrEmpty(Family))
            {
                ViewBag.Stage = new SelectList(fun.FillStageByFamily(Family), "Value", "Text");
            }
            return PartialView();
        }
      

        public JsonResult GetCompletedStage(string plant, string family)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(plant) && !string.IsNullOrEmpty(family))
                {
                  string lblmessage =   fun.getCompletedStages(plant, family);
                    if(!string.IsNullOrEmpty(lblmessage))
                    {
                        msg = lblmessage;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }

            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
            
           
        }

        public JsonResult Save(StageMsterModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidateStageMaster("StageMaster", "Save", data);
                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_STAGE_MASTER where plant_code='" + data.PLANT_CODE.Trim().ToUpper() + "' and family_code='" + data.FAMILY_CODE.Trim().ToUpper() + "' and (OFFLINE_KEYCODE='" + data.OFFLINEITEMS.Trim().ToUpper() + "' OR STAGE_ID='" + data.STAGE_ID.Trim() +"')")) > 0)
                {
                    msg = Validation.str17;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }

                if (!string.IsNullOrEmpty(data.IPADDR))
                {
                    if (Convert.ToInt32(fun.get_Col_Value(@"select count(*) from XXES_STAGE_MASTER where plant_code='" + data.PLANT_CODE.Trim().ToUpper() + "' and family_code='" + data.FAMILY_CODE.Trim().ToUpper() + "' AND OFFLINE_KEYCODE = '" + data.OFFLINEITEMS.Trim().ToUpper() + "' and ipaddr='" + data.IPADDR.Trim() + "'")) > 0)
                    {
                        msg = Validation.str18;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);

                    }
                }

                
                if (fun.InsertStageMaster(data))
                {
                    msg = Validation.str9;
                    mstType = Validation.str;
                    status = Validation.stus;
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
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

        public PartialViewResult Grid(StageMsterModel data)
        {
            if (!string.IsNullOrEmpty(data.PLANT_CODE) && !string.IsNullOrEmpty(data.FAMILY_CODE))
            {

                ViewBag.DataSource = fun.GridStageMaster(data);
            }

            return PartialView();
        }

        public JsonResult Update(StageMsterModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidateStageMaster("StageMaster", "Update", data);
                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                
                if (!string.IsNullOrEmpty(data.IPADDR))
                {
                    if (Convert.ToInt32(fun.get_Col_Value(@"select count(*) from XXES_STAGE_MASTER where plant_code='" + data.PLANT_CODE.Trim().ToUpper() + "' and family_code='" + data.FAMILY_CODE.Trim().ToUpper() + "' AND OFFLINE_KEYCODE <> '" + data.OFFLINEITEMS.Trim().ToUpper() + "' and ipaddr = '" + data.IPADDR.Trim() + "'")) > 0)
                    {
                        msg = Validation.str18;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);

                    }
                }


                if (fun.UpdateStageMaster(data))
                {
                    msg = Validation.str11;
                    mstType = Validation.str;
                    status = Validation.stus;
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
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


        public JsonResult Delete(StageMsterModel data)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (fun.DeleteStageMaster(data))
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