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
    //[Authorize]
    public class MenuMasterController : Controller
    {
        OracleConnection con;
        OracleCommand cmd;
        OracleDataAdapter da;
        DataTable dt;
        Function fun = new Function();

        [HttpGet]
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                da = new OracleDataAdapter("SELECT * FROM XXES_MMENU WHERE MTYPE = 'WEB' ORDER BY MENUCODE,SEQUENCE", fun.Connection());
                DataTable dt = new DataTable();
                da.Fill(dt);
                ViewBag.DataSource = dt;
                return View("Index");
            }
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
        public ActionResult Create(MenuModel menu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    cmd = new OracleCommand("USP_MENU_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "INSERT";
                    cmd.Parameters.Add("ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
                    cmd.Parameters.Add("MENU_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuCode;
                    cmd.Parameters.Add("MENU_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuName;
                    cmd.Parameters.Add("CONTROLLER_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ControllerName;
                    cmd.Parameters.Add("ACTION_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ActionName;
                    cmd.Parameters.Add("ROUT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Rout_Id;
                    cmd.Parameters.Add("ICON", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Icon;
                    cmd.Parameters.Add("SEQUNCE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Sequence;
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
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id != null)
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_MMENU WHERE MTYPE = 'WEB' AND AUTOID = '" + id + "'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    MenuModel mm = new MenuModel();
                    mm.AutoId = Convert.ToInt32(id);
                    mm.MenuCode = dt.Rows[0]["MENUCODE"].ToString();
                    mm.MenuName = dt.Rows[0]["MNAME"].ToString();
                    mm.ControllerName = dt.Rows[0]["CONTROLLER"].ToString();
                    mm.ActionName = dt.Rows[0]["ACTION"].ToString();
                    mm.Rout_Id = dt.Rows[0]["ROUT_ID"].ToString();
                    mm.Icon = dt.Rows[0]["URL"].ToString();
                    mm.Sequence = dt.Rows[0]["SEQUENCE"].ToString();
                    return View(mm);
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
        public ActionResult Edit(MenuModel menu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    cmd = new OracleCommand("USP_MENU_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UPDATE";
                    cmd.Parameters.Add("ID", OracleDbType.Int32, ParameterDirection.Input).Value = menu.AutoId;
                    cmd.Parameters.Add("MENU_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuCode;
                    cmd.Parameters.Add("MENU_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuName;
                    cmd.Parameters.Add("CONTROLLER_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ControllerName;
                    cmd.Parameters.Add("ACTION_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ActionName;
                    cmd.Parameters.Add("ROUT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Rout_Id;
                    cmd.Parameters.Add("ICON", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Icon;
                    cmd.Parameters.Add("SEQUNCE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Sequence;
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
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id != null)
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_MMENU WHERE MTYPE = 'WEB' AND AUTOID = '" + id + "'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    MenuModel mm = new MenuModel();
                    mm.AutoId = Convert.ToInt32(dt.Rows[0]["AUTOID"]);
                    return View(mm);
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
        public ActionResult Delete(MenuModel menu)
        {
            if (menu.AutoId > 0)
            {
                try
                {
                    cmd = new OracleCommand("USP_MENU_OPERATION", fun.Connection());
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("OPERATION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DELETE";
                    cmd.Parameters.Add("ID", OracleDbType.Int32, ParameterDirection.Input).Value = menu.AutoId;
                    cmd.Parameters.Add("MENU_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuCode;
                    cmd.Parameters.Add("MENU_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MenuName;
                    cmd.Parameters.Add("CONTROLLER_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ControllerName;
                    cmd.Parameters.Add("ACTION_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.ActionName;
                    cmd.Parameters.Add("ROUT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Rout_Id;
                    cmd.Parameters.Add("ICON", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Icon;
                    cmd.Parameters.Add("SEQUNCE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.Sequence;
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
                    return RedirectToAction("Index");
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
