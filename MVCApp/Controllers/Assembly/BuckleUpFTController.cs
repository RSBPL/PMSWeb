using MVCApp.Common;
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
            DataTable dt = new DataTable();
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                ShiftDetail shiftDetail = af.getshift();
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family) && !string.IsNullOrEmpty(shiftDetail.Shiftcode) && shiftDetail.Plandate != null)
                {
                    query = string.Format(@"select distinct  xt.ITEM_CODE || ' # ' || substr( REPLACE( REPLACE(xt.ITEM_DESC , 'TRACTOR FARMTRAC', 'FT'),'TRACTOR POWERTRAC','PT'),1,25) || '#' || xt.AUTOID as TEXT,xt.ITEM_CODE,
                        xm.PLAN_ID,xt.AUTOID,xt.ITEM_CODE || '#' || xt.AUTOID as ITEMCODE,xt.seq_no from XXES_DAILY_PLAN_MASTER xm,XXES_DAILY_PLAN_TRAN xt,XXES_DAILY_PLAN_JOB jt
                        where xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' 
                        and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}' and xt.AUTOID=jt.FCODE_AUTOID and jt.JOBID not in (select S.JOBID from XXES_JOB_STATUS S)", Plant, Family, shiftDetail.Plandate.ToString("dd-MMM-yyyy"));
                    //DataTable dt = new DataTable();
                    //query = string.Format(@"select ITEM_CODE  || '(' || SUBSTR(ITEM_DESCRIPTION,0,30)  || ')' as DESCRIPTION , ITEM_CODE 
                    //    from XXES_ITEM_MASTER where  plant_code='{0}' and family_code='{1}' order by Item_code", Plant, Family);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["TEXT"]),
                                Value = Convert.ToString(dr["ITEM_CODE"])
                            });
                        }
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
        public JsonResult BindJob(string Plant,string Family)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            DataTable dt = new DataTable();
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                ShiftDetail shiftDetail = af.getshift();
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family) && !string.IsNullOrEmpty(shiftDetail.Shiftcode) && shiftDetail.Plandate != null)
                {
                    query = string.Format(@"select distinct  xt.ITEM_CODE || ' # ' || substr( REPLACE( REPLACE(xt.ITEM_DESC , 'TRACTOR FARMTRAC', 'FT'),'TRACTOR POWERTRAC','PT'),1,25) || '#' || xt.AUTOID as TEXT,xt.ITEM_CODE,
                        xm.PLAN_ID,xt.AUTOID,xt.ITEM_CODE || '#' || xt.AUTOID as ITEMCODE,xt.seq_no from XXES_DAILY_PLAN_MASTER xm,XXES_DAILY_PLAN_TRAN xt,XXES_DAILY_PLAN_JOB jt
                        where xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' 
                        and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}' and xt.AUTOID=jt.FCODE_AUTOID and jt.JOBID not in (select S.JOBID from XXES_JOB_STATUS S)", Plant, Family, shiftDetail.Plandate.ToString("dd-MMM-yyyy"));
                    string data = fun.get_Col_Value(query);
                    if(!string.IsNullOrEmpty(data))
                    {
                        planid = data.Split('#')[2].Trim().ToUpper();
                    }
                    query = string.Format(@"select JOBID as TEXT,JOBID as CODE  from XXES_DAILY_PLAN_JOB where FCODE_AUTOID = '{0}' 
                            and JOBID not in (select JOBID from XXES_JOB_STATUS) and plant_code ='{1}' and family_code ='{2}' order by JOBID", planid, Plant, Family);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            result.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["TEXT"]),
                                Value = Convert.ToString(dr["CODE"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Print(FTBuckleup fTBuckleup)
        //{
        //    try
        //    {
                
        //        tractor.FarmTractorBuckleUp();
        //    }
        //    catch (Exception ex)
        //    {
        //        fun.LogWrite(ex);
        //    }
        //}
        
    }
}