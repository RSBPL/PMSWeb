using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace MVCApp.Controllers
{
    public class testController : Controller
    {
        // GET: test
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ReadFile()
        {
            bool result = false;
            StringBuilder strbuild = new StringBuilder();
            try
            {
                //var fileContents = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/file.txt"));
                using (StreamReader sr = new StreamReader(Path.Combine(Server.MapPath("~/Printer/Output.prn"))))
                        {
                            while (sr.Peek() >= 0)
                            {
                                strbuild.AppendFormat(sr.ReadLine());
                            }
                        }      
               
            }
            catch (Exception ex)
            {
                result = false;
            }

            return new JsonResult()
            {
                Data = strbuild.ToString()
            };
        }
    }
   
}