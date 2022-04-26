using EncodeDecode;
using MVCApp.CommonFunction;
using MVCApp.Models;
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
    public class FamilySerialController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";

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

        public PartialViewResult BindStage(string Plant, string Family)
        {
            if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
            {
                ViewBag.Stage = new SelectList(fun.Fill_All_Stage_ByPlantAndFamily(Plant, Family), "Value", "Text");
            }
            return PartialView();
        }

        public PartialViewResult Grid(ItemModel data)
        {
           
            if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
            {

                ViewBag.DataSource = fun.GridFamilySerial(data);
            }

            return PartialView();
        }

        public JsonResult Save(ItemModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidateNullEmpty("FamilySerial", "Save",data);
                
                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                //////////////////////////////////////////////////////////////////
                //////////
                string strtSerial = data.Start_Serial.Trim();
                char[] arrayStartSerial = fun.characterArray(strtSerial);
                string endSerial = data.End_Serial.Trim();
                char[] arrayEndSerial = fun.characterArray(endSerial);
                Int64 j, k,l;
                string stageValue = data.Stage.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (arrayStartSerial.Length != arrayEndSerial.Length)
                {
                    msg = "Length must be Same in Start and End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
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

                ///////////////////////////////////////////////////////////////////////////
                if (!string.IsNullOrEmpty(data.Current_Serial))
                {
                    string curentSerial = data.Current_Serial.Trim();
                    char[] arrayCurrentSerial = fun.characterArray(curentSerial);
                    

                    if (arrayEndSerial.Length != arrayCurrentSerial.Length)
                    {
                        msg = "Current Serial Length must be equal with Start & End ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (!Int64.TryParse(curentSerial, out l))
                    {

                        msg = "Current Serial is not a valid number ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (!(l > j))
                    {
                        msg = "Current Serial must be Greater than Start Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (!(l < k))
                    {
                        msg = "Current Serial must be Less than End Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    string Tractor_Month_Code = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + DateTime.Now.Date.ToString("MMM-yyyy").ToUpper() + "' and type='DOMESTIC'");
                    string Current_Serial_number = data.Prefix.Trim().ToUpper() + Tractor_Month_Code + data.Current_Serial.Trim();
                    query = "select count(*) from XXES_PRINT_SERIALS where offline_keycode='" + subs[1].Trim() + "' and srno='" + Current_Serial_number.Trim() + "'";
                    if (fun.CheckExits(query))
                    {
                        
                        msg = "Current Serial already used. Please enter valid no. ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    string keycode = subs[1].Trim();// pbf.get_Col_Value("select offline_keycode from XXES_STAGE_MASTER where stage_ID='" + stageid.Trim() + "'");
                    if (!string.IsNullOrEmpty(keycode))
                    {
                        query = "select count(*) from XXES_DELETED_SERIAL where offline_keycode = '" + keycode.Trim() + "' and serial_number = '" + Current_Serial_number.Trim() + "'";
                        if (fun.CheckExits(query))
                        {
                            msg = "Current Serial already used in Deleted Table. Please enter valid no. ..";
                            mstType = "alert-danger";
                            status = "error";
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }

                }

             ////////////////////////////////////////////////////////////////////////////////////   

                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM xxes_family_serial WHERE PLANT_CODE = '" + data.Plant.ToUpper().Trim() + "' AND FAMILY_CODE = '" + data.Family.ToUpper().Trim() + "' AND STAGE_ID = '" + subs[1].ToUpper().Trim() + "'")) > 0)
                {
                    msg = Validation.str10;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    
                    if (fun.InsertFamilySerial(data))
                    {
                        msg = Validation.str9;
                        mstType = Validation.str;
                        status = Validation.stus;
                    }
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

        public JsonResult Update(ItemModel data)
        {
            string msg = string.Empty,mstType = string.Empty, status = string.Empty;
            try
            {
                var tuple = Validation.ValidateNullEmpty("FamilySerial", "Update", data);

                if (tuple.Item4 == false)
                {
                    var resul = new { Msg = tuple.Item1, ID = tuple.Item2, validation = tuple.Item3 };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


                string strtSerial = data.Start_Serial.Trim();
                char[] arrayStartSerial = fun.characterArray(strtSerial);
                string endSerial = data.End_Serial.Trim();
                char[] arrayEndSerial = fun.characterArray(endSerial);
                Int64 j, k,l;
                string stageValue = data.Stage.Trim();
                char[] separators = new char[] { '#' };
                string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (arrayStartSerial.Length != arrayEndSerial.Length)
                {
                    msg = "Length must be Same in Start and End Serial ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
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

                ///////////////////////////////////////////////////////////////////////////
                if (!string.IsNullOrEmpty(data.Current_Serial))
                {
                    string curentSerial = data.Current_Serial.Trim();
                    char[] arrayCurrentSerial = fun.characterArray(curentSerial);


                    if (arrayEndSerial.Length != arrayCurrentSerial.Length)
                    {
                        msg = "Current Serial Length must be equal with Start & End ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (!Int64.TryParse(curentSerial, out l))
                    {

                        msg = "Current Serial is not a valid number ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    //if (!((j > l) || (k > l)))
                    //{
                    //    msg = "Current Serial must between Start & End Serial ..";
                    //    mstType = "alert-danger";
                    //    status = "error";
                    //    var resul = new { Msg = msg, ID = mstType, validation = status };
                    //    return Json(resul, JsonRequestBehavior.AllowGet);
                    //}
                    if(!(l>j))
                    {
                        msg = "Current Serial must be Greater than Start Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if(!(l<k))
                    {
                        msg = "Current Serial must be Less than End Serial ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    string Tractor_Month_Code = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + DateTime.Now.Date.ToString("MMM-yyyy").ToUpper() + "' and type='DOMESTIC'");
                    string Current_Serial_number = data.Prefix.Trim().ToUpper() + Tractor_Month_Code + data.Current_Serial.Trim();
                    query = "select count(*) from XXES_PRINT_SERIALS where offline_keycode='" + subs[1].Trim() + "' and srno='" + Current_Serial_number.Trim() + "'";
                    if (fun.CheckExits(query))
                    {

                        msg = "Current Serial already used. Please enter valid no. ..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    string keycode = subs[1].Trim();// pbf.get_Col_Value("select offline_keycode from XXES_STAGE_MASTER where stage_ID='" + stageid.Trim() + "'");
                    if (!string.IsNullOrEmpty(keycode))
                    {
                        query = "select count(*) from XXES_DELETED_SERIAL where offline_keycode = '" + keycode.Trim() + "' and serial_number = '" + Current_Serial_number.Trim() + "'";
                        if (fun.CheckExits(query))
                        {
                            msg = "Current Serial already used in Deleted Table. Please enter valid no. ..";
                            mstType = "alert-danger";
                            status = "error";
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }

                }

                //////////////////////////////////////////////////////////////////////////////////// 

                if (fun.UpdateFamilySerial(data))
                {
                    msg = Validation.str11;
                    mstType = Validation.str;
                    status = Validation.stus;
                }
            }
            catch(Exception ex) 
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

        public JsonResult Delete(ItemModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                
                if (fun.DeleteFamilySerial(data))
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