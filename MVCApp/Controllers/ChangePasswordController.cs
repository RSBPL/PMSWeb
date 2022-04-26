using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EncodeDecode;
using MVCApp.CommonFunction;
using MVCApp.Models;

namespace MVCApp.Controllers.Admin
{
    [Authorize]
    public class ChangePasswordController : Controller
    {
        Function fun = new Function();
        BaseEncDec bed = new BaseEncDec();
        
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangePassword _changePassword)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    string query = string.Format(@"Select * from XXES_Users_Master where upper(UsrName)='{0}' and isactive=1", Convert.ToString(Session["Login_User"]).Trim().ToUpper());
                    DataTable dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        if (bed.base64Decode(dt.Rows[0]["PsWord"].ToString().Trim()) == _changePassword.Password.Trim())
                        {
                            string encriptedPW = bed.base64Encode(_changePassword.NewPassword.Trim());
                          
                            query = string.Format(@"UPDATE XXES_Users_Master SET PSWORD = '{0}', UpdatedBy = '{1}', UpdatedDate = SYSDATE WHERE upper(UsrName) = '{2}'", encriptedPW, Convert.ToString(Session["Login_User"]).Trim().ToUpper(), Convert.ToString(Session["Login_User"]).Trim().ToUpper());

                            if (fun.EXEC_QUERY(query))
                            {
                                ViewBag.msg = "Password Updated successfully...";
                            }
                            else
                            {
                                ViewBag.msg = "Something error ...";
                            }

                        }
                        else
                        {
                            ViewBag.msg = "Invalid Current Password ...";
                        }
                    }
                    else
                    {
                        ViewBag.msg = "No User exits ...";
                    }

                }


                catch (Exception ex)
                {
                    fun.LogWrite(ex);
                    ViewBag.msg = "Something error ...";
                }

                finally { }
            }
            
            return View();
        }
    }
}