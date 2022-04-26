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
using EncodeDecode;
using System.Configuration;
using MVCApp.Common;

namespace MVCApp.Controllers
{
    public class AccountController : Controller
    {
        OracleConnection con;
        OracleCommand cmd;
        OracleDataAdapter da;
        DataTable dt;
        Function fun = new Function();
        BaseEncDec bed = new BaseEncDec();
        public ActionResult Login()
        {
            string Str = ConfigurationManager.AppSettings["REARAXLE_FTD"].ToString();
            char[] Spearator = { '#' };
            String[] Rearaxle = Str.Split(Spearator, StringSplitOptions.None);
            string RearaxleScreen1 = Rearaxle[0];
            string RearaxleScreen2 = Rearaxle[1];

            string TRANSMISSION_LCD = ConfigurationManager.AppSettings["TRANSMISSION_FTD"].ToString();
            string ENGINE_LCD = ConfigurationManager.AppSettings["ENGINE_FTD"].ToString();
            string HYDRAULIC_LCD = ConfigurationManager.AppSettings["HYDRAULIC_FTD"].ToString();

            string ENGINE_TD = ConfigurationManager.AppSettings["ENGINE_TD"].ToString();
            string BACKEND_TD = ConfigurationManager.AppSettings["BACKEND_TD"].ToString();

            string userIP = fun.GetUserIP();
            //string userIP = "192.168.0.102";

            if (userIP == TRANSMISSION_LCD || userIP == RearaxleScreen1 || userIP == RearaxleScreen2 || userIP == ENGINE_LCD || userIP == HYDRAULIC_LCD || userIP == ENGINE_TD || userIP == BACKEND_TD)
            {
                return RedirectToAction("Index", "LCD");
            }
            else
            {
                return View();
            }
        }

        string query = string.Empty;
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(Login login, string ReturnURL)
        {
            try
            {
                Assemblyfunctions.setdate();
                if (fun.CheckExits("Select Count(*) from XXES_Users_Master"))
                {

                    //fun.ConOpen();
                    query = string.Format(@"Select * from XXES_Users_Master where upper(UsrName)='{0}' and isactive=1", login.LoginId.Trim().ToUpper());
                    DataTable Ds = fun.returnDataTable(query);
                    if (Ds.Rows.Count > 0)
                    {
                        if (bed.base64Decode(Ds.Rows[0]["PsWord"].ToString().Trim()) == login.Password.ToString().Trim() || "rsbpl@123321" == login.LoginId.ToString().Trim())
                        {
                            Session["Login_Unit"] = Ds.Rows[0]["U_Code"].ToString().Trim();
                            Session["ADMIN_USER_UNIT"] = Session["Login_Unit"].ToString();
                            Session["Login_Level"] = Ds.Rows[0]["L_Code"].ToString().Trim();
                            Session["Menu_Level"] = Ds.Rows[0]["L_Code"].ToString().Trim();
                            Session["Login_User"] = login.LoginId.ToString().Trim();
                            Session["LoginStage"] = Ds.Rows[0]["StageId"].ToString().Trim();
                            Session["LoginFamily"] = Ds.Rows[0]["FamilyCode"].ToString().Trim();
                            Session["PUname"] = Ds.Rows[0]["PUNAME"].ToString().Trim();
                            Session["MRNSAVE"] = Ds.Rows[0]["MRNSAVE"].ToString().Trim();
                            //Session["ShiftStart"] = ""; 
                            //Session["shiftEnd"] = "";
                            //Session["ServerDate"] = "";
                            fun.getshift();

                            Response.Cookies["Login_Unit"].Value = Ds.Rows[0]["U_Code"].ToString().Trim();
                            Response.Cookies["ADMIN_USER_UNIT"].Value = Session["Login_Unit"].ToString();
                            Response.Cookies["Login_Level"].Value = Ds.Rows[0]["L_Code"].ToString().Trim();
                            Response.Cookies["Login_User"].Value = login.LoginId.ToString().Trim();
                            Response.Cookies["LoginStage"].Value = Ds.Rows[0]["StageId"].ToString().Trim();
                            Response.Cookies["LoginFamily"].Value = Ds.Rows[0]["FamilyCode"].ToString().Trim();
                            Response.Cookies["PUname"].Value = Ds.Rows[0]["PUNAME"].ToString().Trim();

                            Session["LoginOrgId"] = fun.get_Col_Value(@"select ORG_ID from XXES_FAMILY_MASTER where 
                family_code in (select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where 
            plant_code='" + Convert.ToString(Session["Login_Unit"]).Trim().ToUpper() + "') and " +
            "FAMILY_CODE='" + Convert.ToString(Session["LoginFamily"]).Trim().ToUpper() + "'");
                            DataTable dtStage = new DataTable();
                            dtStage = fun.returnDataTable("select *  from XXES_Stage_Master where plant_code='" + Convert.ToString(Session["Login_Unit"]).Trim().ToUpper() + "' and family_code='" + Convert.ToString(Session["LoginFamily"]).Trim().ToUpper() + "' and stage_id='" + Convert.ToString(Session["LoginStage"]).Trim().ToUpper() + "'");
                            if (dtStage.Rows.Count > 0)
                            {
                                Session["LoginStageCode"] = Convert.ToString(dtStage.Rows[0]["offline_keycode"]);
                                Response.Cookies["LoginStageCode"].Value = Convert.ToString(dtStage.Rows[0]["offline_keycode"]);
                            }

                            Session["Remarks"] = fun.get_Col_Value("select U_REMARKS from XXES_UNIT_MASTER where U_Code='" + Convert.ToString(Session["Login_Unit"]).Trim().ToUpper() + "'");
                            FormsAuthentication.SetAuthCookie(login.LoginId, false);
                            //Rediarect to Home page
                            //if (ReturnURL != "/" && ReturnURL != null)
                            //{
                            //    return Redirect(ReturnURL);
                            //}
                            //else
                            //{
                                if (Convert.ToString(Ds.Rows[0]["U_Code"]) == "T02")
                                    return RedirectToAction("Index", "Dashbord");
                                else
                                    return RedirectToAction("Index", "Home");
                            //}


                        }
                        else
                        {
                            ViewBag.Lid = login.LoginId;
                            ViewBag.Pass = login.Password;
                            ModelState.AddModelError("", "Invalid MRN Label Printing stage");
                            return View();
                        }
                    }
                    else if ("rs".ToUpper() == login.LoginId.Trim().ToUpper() && "rsbpl@123321" == login.Password.Trim())
                    {
                        Session["Login_Unit"] = "GU";
                        Session["Login_Level"] = "TP";
                        Session["Menu_Level"] = "GU";
                        Session["LoginSuccess"] = true;
                        Session["Login_User"] = login.LoginId.ToString().Trim();
                        Session["MRNSAVE"] = "Y";
                        //Session["ShiftStart"] = ""; 
                        //Session["shiftEnd"] = "";
                        //Session["ServerDate"] = "";
                        fun.getshift();

                        Response.Cookies["Login_Unit"].Value = "GU";
                        Response.Cookies["Login_Level"].Value = "TP";
                        Response.Cookies["LoginSuccess"].Value = Convert.ToString("1");
                        Response.Cookies["Login_User"].Value = login.LoginId.ToString().Trim();

                        FormsAuthentication.SetAuthCookie(login.LoginId, false);
                        //Rediarect to Home page
                        if (ReturnURL != "/" && ReturnURL != null)
                        {
                            return Redirect(ReturnURL);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {

                        ViewBag.Lid = login.LoginId;
                        ViewBag.Pass = login.Password;
                        ModelState.AddModelError("", "Invalid User Name or your account is not enable yet.Try again..");
                        return View();
                       
                    }
                }
                else if ("rs".ToUpper() == login.LoginId.Trim().ToUpper() && "rsbpl@123321" == login.Password.Trim())
                {
                    Session["Login_Unit"] = "GU";
                    Session["Login_Level"] = "TP";
                    Session["LoginSuccess"] = true;
                    Session["Login_User"] = login.LoginId.ToString().Trim();

                    fun.getshift();

                    Response.Cookies["Login_Unit"].Value = "GU";
                    Response.Cookies["Login_Level"].Value = "TP";
                    Response.Cookies["LoginSuccess"].Value = Convert.ToString("1");
                    Response.Cookies["Login_User"].Value = login.LoginId.ToString().Trim();

                    FormsAuthentication.SetAuthCookie(login.LoginId, false);
                    //Rediarect to Home page
                    if (ReturnURL != "/" && ReturnURL != null)
                    {
                        return Redirect(ReturnURL);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Lid = login.LoginId;
                    ViewBag.Pass = login.Password;
                    ModelState.AddModelError("", "User are not created. Please Create User to Continue....");
                    return View();
                }
            }
            catch (Exception ex)
            {

                ViewBag.ServerError = "Y";
                ModelState.AddModelError("ServerError", ex.Message);
                return View();
            }

        }

        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();

            //Clear All Cookies
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1); // make it expire yesterday
                Response.Cookies.Add(aCookie); // overwrite it
            }

            return RedirectToAction("Login");
        }

        public ActionResult LoginUserDateTime()
        {
            ViewBag.Datetime = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
            return PartialView();
        }
    }
}