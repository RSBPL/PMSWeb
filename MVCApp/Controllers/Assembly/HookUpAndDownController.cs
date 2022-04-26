using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace MVCApp.Controllers
{
    public class HookUpAndDownController : Controller
    {
        // GET: HookUpAndDown
        string msg = string.Empty; string mstType = string.Empty;
        Function fun = new Function();
        string query = string.Empty;
        DataTable dt = new DataTable();

        [HttpGet]
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.value = DateTime.Now;
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
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Grid(HookUpAndDown obj)
        {
            
            int recordsTotal = 0;
            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            List<HookUpAndDown> HookDetails = fun.GridHookData(obj);
            if(HookDetails.Count > 0)
            {
                recordsTotal = HookDetails[0].TOTALCOUNT;
            } 

            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = HookDetails }, JsonRequestBehavior.AllowGet);

        }
        public DataTable returnDataTable(string SqlQuery)
        {
            OracleConnection ConOrcl = new OracleConnection(Function.orCnstr);
            DataTable dt = new DataTable();
            try
            {
                OracleDataAdapter Oda = new OracleDataAdapter(SqlQuery, ConOrcl);
                Oda.Fill(dt);
                return dt;
            }
            catch
            {
                throw;
            }
            finally { ConOrcl.Dispose(); }
        }
        [HttpPost]
        public ActionResult Save(List<HookUpAndDown> data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                foreach (var item in data)
                {
                    query = @"SELECT m.plant_code, m.family_code, m.item_code, m.item_description DESCRIPTION, To_char(entrydate, 
                            'dd-Mon-yyyy HH24:MI:SS') ENTRYDATE, c.jobid, c.fcode_id FROM xxes_job_status c, xxes_item_master m WHERE c.item_code = m.item_code 
                            AND m.plant_code = '" + item.PLANTCODE + "' AND c.JOBID = '" + item.JOBID.Trim() + "' AND To_char(entrydate, 'dd-Mon-yyyy') >= To_date" +
                            "('" + item.ToDate + "', 'dd-Mon-yyyy') AND To_char(entrydate, 'dd-Mon-yyyy') <= To_date('" + item.FromDate + "', 'dd-Mon-yyyy') AND " +
                            "jobid NOT IN(SELECT jobid FROM xxes_controllers_data WHERE stage = 'BP' AND fcode_id = c.fcode_id) ORDER BY fcode_id ";
                    dt = returnDataTable(query);
                    foreach (DataRow dr in dt.Rows)
                    {
                        fun.HookUpDown(Convert.ToString(dr["jobid"]), Convert.ToString(dr["plant_code"]), Convert.ToString(dr["family_code"]), Convert.ToString(dr["item_code"]), "9999", Convert.ToString(dr["fcode_id"]), true, true, item.ToDate.Trim());
                    }
                }
                msg = "TRACTOR HOOKUP AND DOWN SUCCESSFULLY...";
                mstType = "alert-success";
                status = "success";

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}