using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCApp.Models;
using System.Web.Mvc;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using MVCApp.CommonFunction;

namespace MVCApp.Controllers
{
    [Authorize]
    public class LevelMasterController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter da;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.Role = fun.BindRole();
                return View();
            }
        }

        public PartialViewResult TreeView(string Id)
        {
            try
            {
                string role = "GU";
                List<object> treedata = new List<object>();

                //Procedure Calling for binding [Menu]
                //Calling Start
                da = new OracleDataAdapter("USP_PARENTMENU", fun.Connection());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("U_ROLL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = role;
                da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                DataTable PMenuDT = new DataTable();
                da.Fill(PMenuDT);
                //Calling End

                if (PMenuDT.Rows.Count > 0)
                {
                    foreach (DataRow PM in PMenuDT.AsEnumerable())
                    {
                        string mcode = PM["MCode"].ToString();
                        treedata.Add(new
                        {
                            id = PM["MCode"].ToString(),
                            name = PM["Mname"].ToString(),
                            hasChild = true,
                            expanded = true
                        });

                        //Procedure Calling for binding [Menu]
                        //Calling Start
                        da = new OracleDataAdapter("USP_CHILDMENU", fun.Connection());
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.SelectCommand.Parameters.Add("U_ROLL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = role;
                        da.SelectCommand.Parameters.Add("MCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = mcode;
                        da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                        DataTable SMenuDT = new DataTable();
                        da.Fill(SMenuDT);
                        //var S_Menu = db.USP_CHILDMENU(role, menu.MCode).ToList();
                        //Calling End

                        if (SMenuDT.Rows.Count > 0)
                        {
                            foreach (DataRow SM in SMenuDT.AsEnumerable())
                            {
                                string smcode = SM["MCode"].ToString();
                                treedata.Add(new
                                {
                                    id = SM["MCode"].ToString(),
                                    pid = mcode,
                                    name = SM["Mname"].ToString(),
                                    hasChild = true,
                                    expanded = true
                                });

                                //Procedure Calling for binding [Menu]
                                //Calling Start
                                da = new OracleDataAdapter("USP_SUBCHILEMENU", fun.Connection());
                                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                                da.SelectCommand.Parameters.Add("U_ROLL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = role;
                                da.SelectCommand.Parameters.Add("MCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = smcode;
                                da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                                DataTable SSMenuDT = new DataTable();
                                da.Fill(SSMenuDT);
                                //Calling End

                                if (SSMenuDT.Rows.Count > 0)
                                {
                                    foreach (var SSM in SSMenuDT.AsEnumerable())
                                    {
                                        treedata.Add(new
                                        {
                                            id = SSM["MCode"].ToString(),
                                            pid = smcode,
                                            name = SSM["Mname"].ToString(),
                                            //hasChild = true,
                                            //expanded = true
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                ViewBag.dataSource = treedata;
                //Getting Role based Menus
                if (!string.IsNullOrEmpty(Id))
                {
                    DataTable dtm = new DataTable();
                    if (Id != "GU")
                    {                        
                        da = new OracleDataAdapter("select MENUCODE from XXES_PRIVILEGE_MASTER WHERE MENUTYPE = 'WEB' AND L_CODE = '" + Id.ToUpper() + "'", fun.Connection());
                        da.Fill(dtm);
                        ViewBag.checkedNodes = dtm.AsEnumerable().Select(r => r.Field<string>("MENUCODE")).ToArray();
                    }
                    else
                    {
                        da = new OracleDataAdapter("SELECT * from xxes_MMENU WHERE MTYPE = 'WEB'", fun.Connection());
                        da.Fill(dtm);
                        ViewBag.checkedNodes = dtm.AsEnumerable().Select(r => r.Field<string>("MENUCODE")).ToArray();
                    }
                }
                else
                {
                    ViewBag.checkedNodes = new string[] { };
                }

                return PartialView();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JsonResult Create(string Role, IEnumerable<string> MenuCode)
        {
            string msg = "";
            try
            {
                if (!string.IsNullOrEmpty(Role) && MenuCode != null)
                {
                    da = new OracleDataAdapter("SELECT * FROM XXES_PRIVILEGE_MASTER WHERE MENUTYPE = 'WEB' AND L_CODE = '" + Role + "'", fun.Connection());
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        cmd = new OracleCommand("DELETE FROM XXES_PRIVILEGE_MASTER WHERE MENUTYPE = 'WEB' AND L_CODE = '" + Role + "' ", fun.Connection());
                        fun.ConOpen();
                        cmd.ExecuteNonQuery();
                    }
                    int i = 0;
                    foreach (string mCode in MenuCode)
                    {
                        string Query = "INSERT INTO XXES_PRIVILEGE_MASTER(L_CODE, MENUCODE, MENUTYPE) VALUES('" + Role + "', '" + mCode + "', 'WEB')";
                        cmd = new OracleCommand(Query, fun.Connection());
                        cmd.CommandType = CommandType.Text;
                        fun.ConOpen();
                        i = cmd.ExecuteNonQuery();
                    }
                    if (i > 0)
                    {
                        msg = "RECORD SAVED SUCCESSFULLY";
                        //TempData["msg"] = Convert.ToString("RECORD SAVED SUCCESSFULLY");
                        //TempData["msgType"] = "alert-success";
                    }
                }
                else
                {
                    msg = "Role or Menus are not given...";
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                //TempData["msg"] = ex.Message.ToString();
                //TempData["msgType"] = "alert-danger";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                fun.ConClose();
            }
        }
    }
}