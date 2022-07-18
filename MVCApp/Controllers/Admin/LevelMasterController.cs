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
        string query = string.Empty;

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

        public JsonResult Create(string Role, IEnumerable<string> MenuCode , List<PrivilegeModel> eng, List<PrivilegeModel> trac, List<PrivilegeModel> crn, List<PrivilegeModel> eki,
            List<PrivilegeModel> bac, List<PrivilegeModel> hyd)
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
                    SaveReports(Role, eng, trac, crn, eki, bac, hyd);
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



        //--------------------------------------------NEW WORK ----------------------------------------------------


        //[HttpPost]
        //public JsonResult ENGINEREPORTS()
        //{

        //    //int recordsTotal = 0;
        //    //obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
        //    //List<PrivilegeModel> HookDetails = GetENGINEREPORTS();
        //    //if (HookDetails.Count > 0)
        //    //{
        //    //    recordsTotal = HookDetails[0].TOTALCOUNT;
        //    //}

        //   return Json(new { draw = "" ,recordsFiltered = "", recordsTotal = "", data = "" }, JsonRequestBehavior.AllowGet);
          
        //}



        [HttpPost]
        public JsonResult ENGINEREPORTS(PrivilegeModel obj)
        {
            return Json(new { data = GetENGINEREPORTS(obj.Type) }, JsonRequestBehavior.AllowGet);
        }

        public List<PrivilegeModel> GetENGINEREPORTS(string Type)
        {
            List<PrivilegeModel> lstLevels = new List<PrivilegeModel>();
            try
            {
                if (Type == "ENGINE_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='ENGINE_REPORTS' AND STATUS='Y'");
                }
                else if (Type == "TRACTOR_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='TRACTOR_REPORTS' AND STATUS='Y'");
                }
                else if (Type == "CRANE_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='CRANE_REPORTS' AND STATUS='Y'");
                }
                else if (Type == "EKI_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='EKI_REPORTS' AND STATUS='Y'");
                }
                else if (Type == "BACKEND_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='BACKEND_REPORTS' AND STATUS='Y'");
                }
                else if (Type == "HYDRAULIC_REPORTS")
                {
                    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='HYDRAULIC_REPORTS' AND STATUS='Y'");
                }


                using (DataTable dt = fun.returnDataTable(query))
                {
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            lstLevels.Add(
                                new PrivilegeModel
                                {
                                    RoleId = "",
                                    EngCode = Convert.ToString(dataRow["PARAMVALUE"]),
                                    EngText = Convert.ToString(dataRow["DESCRIPTION"]),
                                }
                                );
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return lstLevels;
        }

        public bool SaveReports(string Role, List<PrivilegeModel> eng, List<PrivilegeModel> trac, List<PrivilegeModel> crn, List<PrivilegeModel> eki,List<PrivilegeModel> bac, 
            List<PrivilegeModel> hyd)
        {
            string msg = "";
            try
            {
                if(!string.IsNullOrEmpty(Role))
                {
                    if (eng != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "ENGINE_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in eng)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                            VALUES ( '{0}'  ,'{1}'  ,'ENGINE_REPORTS'  ,'{2}'  ,'{3}' )", Role, Convert.ToString(item.ENGINE), null, Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }
                        }
                    }
                    if (trac != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "TRACTOR_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in trac)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                            VALUES ( '{0}'  ,'{1}'  ,'TRACTOR_REPORTS'  ,SYSDATE ,'{2}' )", Role, Convert.ToString(item.TRACTOR), Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }
                        }
                    }
                    if (crn != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "CRANE_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in crn)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                            VALUES ( '{0}'  ,'{1}'  ,'CRANE_REPORTS'  ,SYSDATE  ,'{2}' )", Role, Convert.ToString(item.CRANE), Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }
                        }
                    }
                    if (eki != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "EKI_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in eki)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                            VALUES ( '{0}'  ,'{1}'  ,'EKI_REPORTS'  ,SYSDATE  ,'{2}' )", Role, Convert.ToString(item.EKI), Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }

                        }
                    }
                    if (bac != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "BACKEND_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in bac)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                                VALUES ( '{0}'  ,'{1}'  ,'BACKEND_REPORTS'  ,SYSDATE  ,'{2}' )", Role, Convert.ToString(item.BACKEND), Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }
                        }
                    }
                    if (hyd != null)
                    {
                        query = string.Format(@"DELETE FROM XXES_ROLE_REPORT WHERE CATEGORY='{0}'", "HYDRAULIC_REPORTS");
                        fun.EXEC_QUERY(query);
                        foreach (var item in hyd)
                        {
                            query = string.Format(@"INSERT INTO BARCODE.XXES_ROLE_REPORT (ROLEID ,REPORTCODE ,CATEGORY ,CREATEDDATE ,CREATEDBY ) 
                             VALUES ( '{0}'  ,'{1}'  ,'HYDRAULIC_REPORTS'  ,SYSDATE  ,'{2}' )", Role, Convert.ToString(item.HYDRAULIC), Convert.ToString(Session["Login_User"]));
                            if (fun.EXEC_QUERY(query))
                            {

                            }

                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            { }

        }

        [HttpPost]
        public JsonResult SelectedSection(string RoleId, string Type)
        {
            return Json(FillSectionSelection(RoleId, Type), JsonRequestBehavior.AllowGet);
        }

        public List<PrivilegeModel> FillSectionSelection(string RoleId, string Type)
        {
            string TmpDs = string.Empty;
            DataTable dataTable = null;
            List<PrivilegeModel> Menu = new List<PrivilegeModel>();
            string query = string.Empty;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EngCode", typeof(string));
                dt.Columns.Add("EngText", typeof(string));
                dt.Columns.Add("CheckboxCheck", typeof(string));

               // if (Type == "ENGINE_REPORTS")
                //{
                   query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                //else if (Type == "TRACTOR_REPORTS")
                //{
                //    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                //else if (Type == "CRANE_REPORTS")
                //{
                //    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                //else if (Type == "EKI_REPORTS")
                //{
                //    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                //else if (Type == "BACKEND_REPORTS")
                //{
                //    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                //else if (Type == "HYDRAULIC_REPORTS")
                //{
                //    query = string.Format(@"SELECT PARAMVALUE ,DESCRIPTION FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='{0}' AND STATUS='Y'", Type);
                //}
                dataTable = fun.returnDataTable(query);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.AsEnumerable())
                    {
                        string value = Convert.ToString(dr["PARAMVALUE"]);
                        query = string.Format(@"SELECT REPORTCODE FROM XXES_ROLE_REPORT WHERE ROLEID='{0}' AND REPORTCODE='{1}'", RoleId, Convert.ToString(dr["PARAMVALUE"]));

                        cmd = new OracleCommand(query, fun.Connection());
                        cmd.CommandType = CommandType.Text;
                        fun.ConOpen();
                        TmpDs = Convert.ToString(cmd.ExecuteScalar());

                       // TmpDs = Convert.ToInt32(fun.EXEC_QUERY(query));
                        if (!string.IsNullOrEmpty(TmpDs))
                        {
                            dt.Rows.Add(Convert.ToString(dr["PARAMVALUE"]), Convert.ToString(dr["DESCRIPTION"]), "Check");
                        }
                        else
                        {
                            dt.Rows.Add(Convert.ToString(dr["PARAMVALUE"]), Convert.ToString(dr["DESCRIPTION"]), "UnChecked");
                        }
                    }

                    foreach (DataRow drr in dt.AsEnumerable())
                    {
                        Menu.Add(new PrivilegeModel
                        {
                            EngCode = Convert.ToString(drr["EngCode"]),
                            EngText = Convert.ToString(drr["EngText"]),
                            CheckboxCheck = Convert.ToString(drr["CheckboxCheck"]),
                        });
                    }
                }

                return Menu;
            }
            catch (Exception ex)
            {
                //LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }

        }
    }
}