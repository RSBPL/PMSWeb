using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers
{
    [Authorize]
    public class RopsMasterController : Controller
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

       

        public JsonResult BindDcode(string Plant, string Family)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(Plant) || string.IsNullOrEmpty(Family))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                string orgid = fun.getOrgId(Convert.ToString(Plant).Trim().ToUpper(), Convert.ToString(Family).Trim().ToUpper());
                
                DataTable dt = fun.returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and (description like '%ROPS%' or description like '%DECAL%' ) order by segment1");
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
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

        public PartialViewResult Grid(ItemModel data)
        {
            if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
            {

                ViewBag.DataSource = fun.GridRopesMaster(data);
            }

            return PartialView();
        }

        public JsonResult Save(ItemModel data)
        {

            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant))
                {
                    msg = "Please Select Plant ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Family))
                {
                    msg = "Please Select Family ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Dcode))
                {
                    msg = "Please Select Dcode ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(data.Start_Serial))
                {
                    msg = "Please Fill Start Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.End_Serial))
                {
                    msg = "Please Fill End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string strtSerial = data.Start_Serial.Trim();
                char[] arrayStartSerial = fun.characterArray(strtSerial);
                string endSerial = data.End_Serial.Trim();
                char[] arrayEndSerial = fun.characterArray(endSerial);

                if (arrayStartSerial.Length != arrayEndSerial.Length)
                {
                    msg = "Number of digits must be Same in Start and End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                Int64 j, k;
                if (!Int64.TryParse(strtSerial, out j))
                {

                    msg = "Start Serial is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (!Int64.TryParse(endSerial, out k))
                {

                    msg = "End Serial is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (j >= k)
                {
                    msg = "Start Serial must be less than End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


                string DcodeValue = data.Dcode.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = DcodeValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_TORQUE_MASTER WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND ITEM_DCODE = '" + subs[0].ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Item already exists ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fun.InsertRopesMaster(data))
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

        public JsonResult Update(ItemModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(data.Start_Serial))
                {
                    msg = "Please Fill Start Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.End_Serial))
                {
                    msg = "Please Fill End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string strtSerial = data.Start_Serial.Trim();
                char[] arrayStartSerial = fun.characterArray(strtSerial);
                string endSerial = data.End_Serial.Trim();
                char[] arrayEndSerial = fun.characterArray(endSerial);
                if (arrayStartSerial.Length != arrayEndSerial.Length)
                {
                    msg = "Number of digits must be Same in Start and End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                Int64 j, k;
                if (!Int64.TryParse(strtSerial, out j))
                {

                    msg = "Start Serial is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (!Int64.TryParse(endSerial, out k))
                {

                    msg = "End Serial is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (j >= k)
                {
                    msg = "Start Serial must be less than End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.Current_Serial))
                {
                    string curentSerial = data.Current_Serial.Trim();
                    char[] arraycurentSerial = fun.characterArray(curentSerial);
                    if (arraycurentSerial.Length != arrayEndSerial.Length)
                    {
                        msg = "Number of digits must be Same in Current Serial like Start and End Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    Int64 c;
                    if (!Int64.TryParse(curentSerial, out c))
                    {

                        msg = "Current Serial is not a valid number ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (c <= j || c >=k)
                    {
                        msg = "Current Serial must between Start Serial and End Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                if (fun.UpdateRopesMaster(data))
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

        public JsonResult Delete(ItemModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {

                if (fun.DeleteRopesMaster(data))
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