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


namespace MVCApp.Controllers 
{
    [Authorize]
    public class LeftMenuController : Controller
    {
        OracleConnection con;
        OracleCommand cmd;
        OracleDataAdapter da;
        DataTable dt;
        Function fun = new Function();

        [HandleError]
        public ActionResult SideMenu()
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])) || string.IsNullOrEmpty(Convert.ToString(Session["Menu_Level"])))
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    String role = Convert.ToString(Session["Menu_Level"]);
                    if (!string.IsNullOrEmpty(role))
                    {
                        SideMenuModel menu = null;
                        SideMenuListModel menubarList = new SideMenuListModel
                        {
                            Menu = new List<SideMenuModel>(),
                            SubMenu = new List<SideMenuModel>(),
                            SubSubMenu = new List<SideMenuModel>()
                        };

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
                                menu = new SideMenuModel
                                {
                                    MCode = PM["MCode"].ToString(),
                                    Mname = PM["Mname"].ToString(),
                                    Mcontroller = PM["Mcontroller"].ToString(),
                                    Maction = PM["Maction"].ToString(),
                                    Rout = PM["Rout"].ToString(),
                                    Icon = PM["Icon"].ToString().ToLower()
                                };
                                menubarList.Menu.Add(menu);

                                //Procedure Calling for binding [Menu]
                                //Calling Start
                                da = new OracleDataAdapter("USP_CHILDMENU", fun.Connection());
                                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                                da.SelectCommand.Parameters.Add("U_ROLL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = role;
                                da.SelectCommand.Parameters.Add("MCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MCode;
                                da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                                DataTable SMenuDT = new DataTable();
                                da.Fill(SMenuDT);
                                //var S_Menu = db.USP_CHILDMENU(role, menu.MCode).ToList();
                                //Calling End

                                if (SMenuDT.Rows.Count > 0)
                                {
                                    foreach (DataRow SM in SMenuDT.AsEnumerable())
                                    {
                                        menu = new SideMenuModel
                                        {
                                            MCode = SM["MCode"].ToString(),
                                            Mname = SM["Mname"].ToString(),
                                            Mcontroller = SM["Mcontroller"].ToString(),
                                            Maction = SM["Maction"].ToString(),
                                            Rout = SM["Rout"].ToString(),
                                            Icon = SM["Icon"].ToString().ToLower()
                                        };
                                        menubarList.SubMenu.Add(menu);

                                        //Procedure Calling for binding [Menu]
                                        //Calling Start
                                        da = new OracleDataAdapter("USP_SUBCHILEMENU", fun.Connection());
                                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                                        da.SelectCommand.Parameters.Add("U_ROLL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = role;
                                        da.SelectCommand.Parameters.Add("MCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = menu.MCode;
                                        da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                                        DataTable SSMenuDT = new DataTable();
                                        da.Fill(SSMenuDT);
                                        //var SS_Menu = db.USP_SUBCHILDMENU(role, menu.MCode).ToList();
                                        //Calling End

                                        if (SSMenuDT.Rows.Count > 0)
                                        {
                                            foreach (var SSM in SSMenuDT.AsEnumerable())
                                            {
                                                menu = new SideMenuModel
                                                {
                                                    MCode = SSM["MCode"].ToString(),
                                                    Mname = SSM["Mname"].ToString(),
                                                    Mcontroller = SSM["Mcontroller"].ToString(),
                                                    Maction = SSM["Maction"].ToString(),
                                                    Rout = SSM["Rout"].ToString(),
                                                    Icon = SSM["Icon"].ToString().ToLower()
                                                };
                                                menubarList.SubSubMenu.Add(menu);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ViewBag.Menu = menubarList;
                    }
                    return PartialView("SideMenu");
                }
            }
            catch (Exception ex) 
            {
                return PartialView("SideMenu");
            }            
        }
    }
}