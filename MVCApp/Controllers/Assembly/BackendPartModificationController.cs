using MVCApp.CommonFunction;
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
    public class BackendPartModificationController : Controller
    {
        // GET: BackendPartModification
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
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
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindBackend(BackendModification backend)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                string Shift = ""; DateTime Plandate = new DateTime();
                string Shiftcode = "", isDayNeedToLess = "";
                string data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                        Plandate = fun.GetServerDateTime().Date.AddDays(-1);
                    else
                        Plandate = fun.GetServerDateTime().Date.AddDays(-1);

                }
                DataTable dt = new DataTable();
                query = string.Format(@"select distinct  xt.item_code || ' # ' || xt.item_desc  as text,xt.item_code, xm.plan_id,xt.autoid,
                       xt.item_code || '#' || xt.autoid as itemcode from xxes_daily_plan_master xm,xxes_daily_plan_tran xt where 
                       xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}'
                       and xt.qty > (select count(*) from xxes_backend_status where fcode_id=xt.AUTOID) order by xm.PLAN_ID,xt.AUTOID",
                       backend.Plant.Trim().ToUpper(), backend.Family.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"));
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["ITEMCODE"]),
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
        public JsonResult BindJob(string dcode, string FCODE_ID)
        {
            string backendplant = string.Empty, backendfamily = string.Empty, orgid = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                //string Backend = Convert.ToString(dcode).Split('#')[0].Trim();
                //string autoid = Convert.ToString(dcode).Split('#')[1].Trim();
                query = string.Format(@"SELECT M.PLANT_CODE || '#' || M.FAMILY_CODE FROM XXES_ITEM_MASTER  m JOIN XXES_DAILY_PLAN_TRAN t
                ON m.ITEM_CODE = t.FCODE 
                WHERE T.FCODE IS NOT NULL AND T.AUTOID='{0}'", FCODE_ID);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    backendplant = line.Split('#')[0].Trim().ToUpper();
                    backendfamily = line.Split('#')[1].Trim().ToUpper();
                }
                if (string.IsNullOrEmpty(backendplant) || string.IsNullOrEmpty(backendfamily))
                {
                    msg = "Plant/Family not found";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (backendplant == "T04")
                {
                    orgid = "149";
                    backendfamily = "BACK END FTD";
                }
                else
                {
                    orgid = "150";
                    backendfamily = "BACK END TD";
                }
                DataTable dt = new DataTable();
                query = string.Format(@"SELECT WIP_ENTITY_NAME TEXT,WIP_ENTITY_NAME || '#' || START_QUANTITY || '#' || NVL(CC,0) AS JOBID FROM 
                (SELECT DISTINCT A.WIP_ENTITY_NAME,START_QUANTITY,SEGMENT1 FROM RELESEDJOBORDER A WHERE  A.SEGMENT1 ='{0}' and A.ORGANIZATION_ID='{1}'  ) 
                X LEFT JOIN (SELECT JOBID ,COUNT(SRNO) AS CC  FROM xxes_print_serials WHERE  DCODE ='{0}' GROUP BY JOBID) Y ON  X.WIP_ENTITY_NAME = Y.JOBID WHERE 
                X.START_QUANTITY != NVL( Y.CC,0)  and SEGMENT1 ='{0}' ORDER BY WIP_ENTITY_NAME", dcode, orgid);
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    foreach(DataRow dr in dt.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["JOBID"])
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
    }
}