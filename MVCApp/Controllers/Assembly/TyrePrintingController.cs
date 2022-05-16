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
    public class TyrePrintingController : Controller
    {
        // GET: TyrePrinting
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
        public JsonResult BindItemCode(string Plant, string Family)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select ITEM_CODE  || '(' || SUBSTR(ITEM_DESCRIPTION,0,30)  || ')' as DESCRIPTION , ITEM_CODE 
                        from XXES_ITEM_MASTER where  plant_code='{0}' and family_code='{1}' order by Item_code", Plant, Family);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["DESCRIPTION"]),
                                Value = Convert.ToString(dr["ITEM_CODE"])
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult BindTyre()
        {
            List<DDLTextValue> _Tyre = new List<DDLTextValue>();
            try
            {
                DataTable dt = new DataTable();
                query = string.Format(@"select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Tyre.Add(new DDLTextValue
                        {
                            Text =Convert.ToString(dr["Name"]),
                            Value =Convert.ToString(dr["Name"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_Tyre, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetTyreDeCode(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string LHFrontTyre = string.Empty , RHFrontTyre = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.ItemCode))
                {
                    msg = "Please Select Tractor..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT FRONTTYRE || ',' || SUBSTR(FRONTTYRE_DESCRIPTION,0,50) || ',' || RH_FRONTTYRE || ',' || SUBSTR(RH_FRONTTYRE_DESC,0,50)  
                        AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.ItemCode.Trim(), data.Plant.Trim(), data.Family.Trim());
                string line = fun.get_Col_Value(query);
                if(!string.IsNullOrEmpty(line))
                {
                    LHFrontTyre = line.Split(',')[0].Trim().ToUpper();
                    data.FTLH = line.Split(',')[1].Trim().ToUpper() + "(" + LHFrontTyre + ")";
                    RHFrontTyre = line.Split(',')[2].Trim().ToUpper();
                    data.FTRH = line.Split(',')[3].Trim().ToUpper() + "(" + RHFrontTyre + ")";
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var myResult = new
            {
                Result = data,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }
    }
}