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
using EncodeDecode;

namespace MVCApp.Controllers
{
    public class UserMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        DataSet ds;
        Function fun = new Function();
        BaseEncDec bed = new BaseEncDec();
        string query = ""; string ORGID = "";
        // GET: UserMaster
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
                ViewBag.FAMILY_CODE = new SelectList(fun.Fill_All_Family(Plant,1), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindPrivillgeCode(UserMaster UM)
        {
            ViewBag.Previllege = new SelectList(fun.FillPrivillege(UM), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindPUName(UserMaster UM)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(UM.U_CODE)))
            {
                ViewBag.PUNAME = new SelectList(fun.FillPUName(UM, 1), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindStageId(UserMaster UM)
        {
            if (!string.IsNullOrEmpty(UM.U_CODE) && (!string.IsNullOrEmpty(UM.FAMILY_CODE)))
            {
                ViewBag.STAGEID = new SelectList(fun.FillStageID(UM,1), "Value", "Text");
            }
            return PartialView();
        }
        public JsonResult SaveUserMasterNormal(UserMaster data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.USRNAME))
                {
                    msg = "USER NAME SHOULD NOT BE LEFT BLACK...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.PSWORD))
                {
                    msg = "PASSWORD SHOULD NOT BE LEFT BLANK...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.STAGEID))
                {
                    msg = "PLEASE SELECT STAGEID TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(Convert.ToString(data.Level_Name)))
                {
                    msg = "PLEASE SELECT THE PRIVILEGE CODE TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(Convert.ToString(data.U_CODE)))
                {
                    msg = "PLEASE SELECT THE PLANT TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_Users_Master where UPPER(UsrName)='" + Convert.ToString(data.USRNAME).Trim().ToUpper() + "'")) > 0)
                {
                    msg = "User Already Exists..";
                    mstType = Validation.str1;
                    //status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                
                if (data.U_CODE != "GU # Global User" && data.U_CODE != "")
                {
                    if (Convert.ToInt32(data.STAGEID) < 0)
                    {
                        msg = "PLEASE SELECT STAGEID TO CONTINUE...";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, Validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (data.USRNAME.Trim().ToUpper().Contains("RS"))
                {
                    msg = "RS USER NAME WILL NOT BE USED TO CREATE A USER.";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string STOREBYPASS = string.Empty;
                if (!string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    if (data.FAMILY_CODE.Trim().ToUpper().Contains("TRACTOR"))
                    {
                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_PRINT' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_PRINT','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowPrint == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_SAVE' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_SAVE','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowSave == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_SAVE_CHECK_EMPTY' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_SAVE_CHECK_EMPTY','" + data.USRNAME.Trim().ToUpper() + "','" + (data.SaveButton == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='MANNUAL_USER' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('MANNUAL_USER','" + data.USRNAME.Trim().ToUpper() + "','" + (data.MannualMapping == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='ALLOW_ROPS' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('ALLOW_ROPS','" + data.USRNAME.Trim().ToUpper() + "','" + (data.AllowROPS == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_DEL' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_DEL','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowDelete == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);
                    }
                }
                string U_CODE = string.Empty, FAMILY_CODE = string.Empty, STAGEID = string.Empty, PUNAME = string.Empty;
                U_CODE = Convert.ToString(data.U_CODE);
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    FAMILY_CODE = string.Empty;
                }
                else
                {
                    FAMILY_CODE = Convert.ToString(data.FAMILY_CODE);
                    STAGEID = Convert.ToString(data.STAGEID);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(data.PUNAME)) && Convert.ToString(data.PUNAME) != "--Select--")
                {
                    PUNAME = Convert.ToString(data.PUNAME);
                }
                else
                {
                    PUNAME = string.Empty;
                    STOREBYPASS = (data.STOREBYPASS == true ? "Y" : "N");
                }

                if (fun.InsertUserMaster(data))
                {
                    msg = "Saved successfully...";
                    mstType = "alert-success";
                    status = "success";
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

        public PartialViewResult Grid(UserMaster data)
        {
            if (!string.IsNullOrEmpty(data.U_CODE) && (!string.IsNullOrEmpty(data.FAMILY_CODE)))
            {
                ViewBag.DataSource = fun.GridUserMaster(data);
            }
            return PartialView();
        }

        public JsonResult Update(UserMaster data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                
                if (string.IsNullOrEmpty(data.STAGEID))
                {
                    msg = "PLEASE SELECT STAGEID TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(Convert.ToString(data.Level_Name)))
                {
                    msg = "PLEASE SELECT THE PRIVILEGE CODE TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(Convert.ToString(data.U_CODE)))
                {
                    msg = "PLEASE SELECT THE PLANT TO CONTINUE...";
                    mstType = "alert-danger";
                    //status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_Users_Master where UsrName='" + Convert.ToString(data.USRNAME).Trim() + "'")) > 0)
                //{
                //    msg = Validation.str15;
                //    mstType = Validation.str1;
                //    //status = Validation.str2;
                //    var resul = new { Msg = msg, ID = mstType, validation = status };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}


                if (data.USRNAME != "GU # Global User" && data.U_CODE != "")
                {
                    if (Convert.ToInt32(data.STAGEID) < 0)
                    {
                        msg = "PLEASE SELECT STAGEID TO CONTINUE...";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, Validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                if (data.USRNAME.Trim().ToUpper().Contains("RS"))
                {
                    msg = "RS USER NAME WILL NOT BE USED TO CREATE A USER.";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, Validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string STOREBYPASS = string.Empty;
                if (!string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    if (data.FAMILY_CODE.Trim().ToUpper().Contains("TRACTOR"))
                    {
                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_PRINT' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_PRINT','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowPrint == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_SAVE' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_SAVE','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowSave == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_SAVE_CHECK_EMPTY' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_SAVE_CHECK_EMPTY','" + data.USRNAME.Trim().ToUpper() + "','" + (data.SaveButton == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='MANNUAL_USER' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('MANNUAL_USER','" + data.USRNAME.Trim().ToUpper() + "','" + (data.MannualMapping == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='ALLOW_ROPS' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('ALLOW_ROPS','" + data.USRNAME.Trim().ToUpper() + "','" + (data.AllowROPS == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);

                        query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SHOW_DEL' and PARAMVALUE='" + data.USRNAME.Trim().ToUpper() + "'";
                        fun.EXEC_QUERY(query);

                        query = "insert into XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS) values('SHOW_DEL','" + data.USRNAME.Trim().ToUpper() + "','" + (data.ShowDelete == true ? "Y" : "N") + "')";
                        fun.EXEC_QUERY(query);
                    }
                }
                string U_CODE = string.Empty, FAMILY_CODE = string.Empty, STAGEID = string.Empty, PUNAME = string.Empty;
                U_CODE = Convert.ToString(data.U_CODE);
                if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    FAMILY_CODE = string.Empty;
                }
                else
                {
                    FAMILY_CODE = Convert.ToString(data.FAMILY_CODE);
                    STAGEID = Convert.ToString(data.STAGEID);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(data.PUNAME)) && Convert.ToString(data.PUNAME) != "--Select--")
                {
                    PUNAME = Convert.ToString(data.PUNAME);
                }
                else
                {
                    PUNAME = string.Empty;
                    STOREBYPASS = (data.STOREBYPASS == true ? "Y" : "N");
                }
                if (fun.UpdateUserMaster(data))
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
        public JsonResult Delete(UserMaster data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            try
            {
                query = "DELETE FROM XXES_USERS_MASTER WHERE USRNAME = '" + data.USRNAME + "'";

                if (fun.EXEC_QUERY(query))
                {
                    msg = "DELETED SUCCESSFULLY...";
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