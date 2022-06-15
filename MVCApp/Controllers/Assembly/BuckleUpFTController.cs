using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Controllers.DCU;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class BuckleUpFTController : Controller
    {
        // GET: BuckleUpFT

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        Assemblyfunctions af = new Assemblyfunctions();
        Tractor tractor = new Tractor();
        string planid = string.Empty;
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

        [HttpGet]
        public JsonResult BindPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindFamily(string Plant)
        {

            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                result = fun.Fill_FamilyOnlyTractor(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindItemCode(string Plant, string Family)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();
            //DataTable dt = new DataTable();
            try
            {
                COMMONDATA cOMMONDATA = new COMMONDATA();
                cOMMONDATA.PLANT = Plant;
                cOMMONDATA.FAMILY = Family;
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.GetFcodes(cOMMONDATA);
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dataTable.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["TEXT"]),
                                Value = Convert.ToString(dr["AUTOID"])
                            });
                        }
                    }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindJob(string Plant, string Family,string autoid)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {

                COMMONDATA cOMMONDATA = new COMMONDATA();
                cOMMONDATA.PLANT = Plant;
                cOMMONDATA.FAMILY = Family;
                cOMMONDATA.REMARKS = autoid;
                cOMMONDATA.LOCATION = "BUCKLEUP";
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.BindJobs(cOMMONDATA);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["CODE"])
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Print(BuckleUPFT data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                TractorController tractor = new TractorController();
                FTBuckleup fTBuckleup = new FTBuckleup();
                fTBuckleup.PLANT = data.Plant.Trim().ToUpper();
                fTBuckleup.FAMILY = data.Family.Trim().ToUpper();
                fTBuckleup.ITEMCODE = data.ItemCode.Split('#')[0].Trim().ToUpper();
                fTBuckleup.JOBID = data.JobId.Trim().ToUpper();
                fTBuckleup.TRANSMISSIONSRLNO = data.TransmissionSrno;
                fTBuckleup.REARAXELSRLNO = data.RearAxleSrno;
                tractor.FarmTractorBuckleUp(fTBuckleup);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var myResult = new
            {
                //Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

    }
}