using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

using System.Data;
using Newtonsoft.Json;

namespace MVCApp.Controllers 
{
    [Authorize]
    public class ROLEMASTERController : Controller 
    {
        OracleConnection con;
        OracleCommand cmd; 
        OracleDataAdapter da;
        DataTable dt;
        Function fun = new Function();

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_LEVEL_MASTER WHERE ROLL_FOR = 'WEB'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    ViewBag.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message.ToString();
                TempData["msgType"] = "alert-danger";
            }
            //return PartialView("Grid");
            return View("Index");
        }
         
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(RoleModel roleModel) 
        { 
            if (ModelState.IsValid)
            {
                try
                {
                    cmd = new OracleCommand("USP_ROLE_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "INSERT";
                    cmd.Parameters.Add("ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
                    cmd.Parameters.Add("ROLE_N", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Role; 
                    //cmd.Parameters.Add("REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Remark;
                    //cmd.Parameters.Add("IS_ACTIVE", OracleDbType.Char, ParameterDirection.Input).Value = 'Y';
                    //cmd.Parameters.Add("CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = System.Web.HttpContext.Current.User.Identity.Name.ToString();                   
                    cmd.Parameters.Add("RETURN_MSG", OracleDbType.NVarchar2, 500);
                    cmd.Parameters["RETURN_MSG"].Direction = ParameterDirection.Output;
                    fun.ConOpen();
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["RETURN_MSG"].Value)))
                    {
                        TempData["msg"] = Convert.ToString(cmd.Parameters["RETURN_MSG"].Value);
                        TempData["msgType"] = "alert-success";
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["msg"] = ex.Message.ToString();
                    TempData["msgType"] = "alert-danger";
                    return View();
                }
                finally
                {
                    fun.ConClose();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Edit(string Code) 
        {
            try
            {
                if (!string.IsNullOrEmpty(Code))
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_LEVEL_MASTER WHERE L_CODE = '" + Code + "' AND ROLL_FOR = 'WEB'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    RoleModel rm = new RoleModel();
                    rm.AutoId = dt.Rows[0]["L_NAME"].ToString();
                    rm.Role = dt.Rows[0]["L_CODE"].ToString();
                    //rm.Remark = dt.Rows[0]["REMARK"].ToString();
                    return View(rm);
                }
                //if (id != null)
                //{
                //    da = new OracleDataAdapter("SELECT * FROM TBL_ROLEMASTER WHERE AUTOID = '" + id + "'", fun.Connection());
                //    DataTable dt = new DataTable();
                //    da.Fill(dt);
                //    RoleModel rm = new RoleModel();
                //    rm.AutoId = Convert.ToInt32(id);
                //    rm.Role = dt.Rows[0]["ROLE_NAME"].ToString();                   
                //    //rm.Remark = dt.Rows[0]["REMARK"].ToString();
                //    return View(rm);
                //}
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message.ToString();
                TempData["msgType"] = "alert-danger";
            }
            return View();
        }

        [HttpPost]
        public ActionResult Edit(RoleModel roleModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    cmd = new OracleCommand("USP_ROLE_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UPDATE";
                    cmd.Parameters.Add("ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.AutoId;
                    cmd.Parameters.Add("ROLE_N", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Role;
                    //cmd.Parameters.Add("REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Remark;
                    //cmd.Parameters.Add("IS_ACTIVE", OracleDbType.Char, ParameterDirection.Input).Value = 'Y';
                    //cmd.Parameters.Add("CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                    cmd.Parameters.Add("RETURN_MSG", OracleDbType.NVarchar2, 500);
                    cmd.Parameters["RETURN_MSG"].Direction = ParameterDirection.Output;
                    fun.ConOpen();
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["RETURN_MSG"].Value)))
                    {
                        TempData["msg"] = Convert.ToString(cmd.Parameters["RETURN_MSG"].Value);
                        TempData["msgType"] = "alert-success";
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["msg"] = ex.Message.ToString();
                    TempData["msgType"] = "alert-danger";
                    return View();
                }
                finally
                {
                    fun.ConClose();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(string Code)
        {
            try
            {
                if (!string.IsNullOrEmpty(Code))
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_LEVEL_MASTER WHERE L_CODE = '" + Code + "' AND ROLL_FOR = 'WEB'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    RoleModel rm = new RoleModel();
                    rm.AutoId = dt.Rows[0]["L_NAME"].ToString();
                    rm.Role = dt.Rows[0]["L_CODE"].ToString();                    
                    //rm.Remark = dt.Rows[0]["REMARK"].ToString();
                    return View(rm);
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message.ToString();
                TempData["msgType"] = "alert-danger";
            }
            return View();
        } 

        [HttpPost]
        public ActionResult Delete(RoleModel roleModel)  
        {
            if (!string.IsNullOrEmpty(roleModel.AutoId))
            {
                try
                {
                    cmd = new OracleCommand("USP_ROLE_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DELETE";                    
                    cmd.Parameters.Add("ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.AutoId;
                    cmd.Parameters.Add("ROLE_N", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Role;
                    //cmd.Parameters.Add("REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = roleModel.Remark;
                    //cmd.Parameters.Add("IS_ACTIVE", OracleDbType.Char, ParameterDirection.Input).Value = 'Y';
                    //cmd.Parameters.Add("CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                    cmd.Parameters.Add("RETURN_MSG", OracleDbType.NVarchar2, 500);
                    cmd.Parameters["RETURN_MSG"].Direction = ParameterDirection.Output;
                    fun.ConOpen();
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(Convert.ToString(cmd.Parameters["RETURN_MSG"].Value)))
                    {
                        TempData["msg"] = Convert.ToString(cmd.Parameters["RETURN_MSG"].Value);
                        TempData["msgType"] = "alert-danger";
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["msg"] = ex.Message.ToString();
                    TempData["msgType"] = "alert-danger";
                }
                finally
                {
                    fun.ConClose();
                }
            }
            return View();
        }
    } 
}