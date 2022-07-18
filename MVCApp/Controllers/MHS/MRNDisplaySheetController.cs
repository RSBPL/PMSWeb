using CrystalDecisions.CrystalReports.Engine;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.MHS
{
    [Authorize]
    public class MRNDisplaySheetController : Controller
    {
        // GET: MRNDisplaySheet
        OracleCommand cmd;
        OracleDataAdapter da;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        AddMRN addMRN = new AddMRN();
        public ActionResult Index()
        {
            try
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
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
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
            List<DDLTextValue> Result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                Result = fun.Fill_FamilyMRNVerfication(Plant);
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Grid(MRNQCVIEWMODEL obj)
        {
            DataTable dt = new DataTable();
            List<MRNQCVIEWMODEL> MRNList = null;
            int recordsTotal = 0;
            if (Request.Form.GetValues("search[value]").FirstOrDefault() != null) { 
            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            }
            try
            {
                MRNList = addMRN.GridMRN(obj);
                if (MRNList.Count > 0)
                {
                    recordsTotal = MRNList[0].TOTALCOUNT;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = MRNList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Add(MRNQCVIEWMODEL data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            bool IsDIMEmpty = false; bool IsMTEmpty = false; double quantity;
            double DIMOKQTY0 = 0, DIMREJQTY0 = 0, DIMOKDEV0 = 0, DIMOKAFTERSEG0 = 0, DIMOKAFTEREWORK0 = 0, DIMHOLDQTY0 = 0;
            double MTOKQTY0 = 0, MTREJQTY0 = 0, MTOKDEV0 = 0, MTOKAFTERSEG0 = 0, MTOKAFTEREWORK0 = 0, MTHOLDQTY0 = 0;
            double TotalDIM, TotalMT;
            try
            {
                
                    if (addMRN.IsNumber(data.DIMOK_QTY0, out DIMOKQTY0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.DIMREJ_QTY0, out DIMREJQTY0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.DIMOK_DEV0, out DIMOKDEV0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.DIMOK_AFTERSEG0, out DIMOKAFTERSEG0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.DIMOK_AFTEREWORK0, out DIMOKAFTEREWORK0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.DIMHOLD_QTY0, out DIMHOLDQTY0))
                    {
                        IsDIMEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in Dim Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTOK_QTY0, out MTOKQTY0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTREJ_QTY0, out MTREJQTY0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTOK_DEV0, out MTOKDEV0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTOK_AFTERSEG0, out MTOKAFTERSEG0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTOK_AFTEREWORK0, out MTOKAFTEREWORK0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (addMRN.IsNumber(data.MTHOLD_QTY0, out MTHOLDQTY0))
                    {
                        IsMTEmpty = true;
                    }
                    else
                    {
                        msg = "Invalid Value in MT Fields";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                quantity = Convert.ToDouble(data.QUANTITY);
                if (IsDIMEmpty)
                {
                    TotalDIM = DIMOKQTY0 + DIMREJQTY0 + DIMOKDEV0 + DIMOKAFTERSEG0 + DIMOKAFTEREWORK0 + DIMHOLDQTY0;
                    if (TotalDIM > quantity)
                    {
                        msg = "Sum of DIM Fields cannot be more than quantity";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (quantity > TotalDIM)
                    {
                        double DimDifference = quantity - TotalDIM;
                        data.DIMHOLD_QTY0 = Convert.ToString(DIMHOLDQTY0 + DimDifference);
                    }
                }
                if (IsMTEmpty)
                {
                    TotalMT = MTOKQTY0 + MTREJQTY0 + MTOKDEV0 + MTOKAFTERSEG0 + MTOKAFTEREWORK0 + MTHOLDQTY0;
                    if (TotalMT > quantity)
                    {
                        msg = "Sum of MT Fields cannot be more than quantity";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (quantity > TotalMT)
                    {
                        double MTDifference = quantity - TotalMT;
                        data.MTHOLD_QTY0 = Convert.ToString(MTHOLDQTY0 + MTDifference);
                    }
                }

                var tuple = addMRN.InsertMRNSheet(data);

                if (tuple.Item1 == true)
                {
                    msg = tuple.Item2;
                    mstType = Validation.str;
                    status = Validation.stus;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                else
                {

                    msg = tuple.Item2;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CrystalReportForMRN(string MRN, string ItemCode, string PlantCode, string familyCode)
        {
            string query = string.Empty;
            string orgid = fun.getOrgId(Convert.ToString(PlantCode).Trim().ToUpper(), Convert.ToString(familyCode).Trim().ToUpper());

            query = string.Format(@"SELECT PLAN_ID FROM APPS.QA_PLANS WHERE NAME LIKE '%{0}%' AND ORGANIZATION_ID = '{1}'", ItemCode, orgid);
            string plant_ID = fun.get_Col_Value(query);
            if (plant_ID != "")
            {
                query = string.Format(@"SELECT '" + MRN + "' As MRNNO ,'" + ItemCode + "' AS ITEMCODE,PROMPT,CHAR_NAME AS CHARNAME,PROMPT_SEQUENCE AS PROMPTSEQUENCE,COALESCE(DATA_ENTRY_HINT,' ') AS DATAENTRYHINT FROM  apps.qa_plan_chars_v WHERE PLAN_ID = '{0}' AND ORGANIZATION_ID = '{1}' ORDER BY PROMPT_SEQUENCE", plant_ID, orgid);
                dt = fun.returnDataTable(query);
                return GetReportForMRN(dt);
            }
            return View("Empty");
        }
        private FileResult GetReportForMRN(DataTable dt)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Server.MapPath(@"~/CrystalReports/MRNSheet.rpt"));
            //rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "MRNSheet.rpt"));
            rd.SetDataSource(dt);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf");
        }
    }
}