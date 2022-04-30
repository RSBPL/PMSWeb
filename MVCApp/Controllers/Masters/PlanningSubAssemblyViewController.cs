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
using Newtonsoft.Json;
using EncodeDecode;
using System.Globalization;
using System.Configuration;

namespace MVCApp.Controllers
{
    [Authorize]
    public class PlanningSubAssemblyViewController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        string query1 = string.Empty;

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                //int ProvideDate;
                //string Loginfamily = Convert.ToString(Session["LoginFamily"]);

                //if (!string.IsNullOrEmpty(Loginfamily))
                //{
                //    if (Loginfamily == "ENGINE FTD")
                //    {
                //        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'EngineDays'";
                //    }
                //    if (Loginfamily == "HYDRAULIC FTD")
                //    {
                //        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'HydraulicDays'";
                //    }
                //    if (Loginfamily == "REAR AXLE FTD")
                //    {
                //        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'ReareAxleDays'";
                //    }
                //    if (Loginfamily == "TRANSMISSION FTD")
                //    {
                //        query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MVC' AND PARAMETERINFO = 'TransmissionDays'";
                //    }
                //    ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                //    ViewBag.TodayDate = DateTime.Now.AddDays(-ProvideDate);
                //    ViewBag.maxDate = DateTime.Now.AddDays(-ProvideDate);
                //    ViewBag.minDate = DateTime.Now.AddDays(-ProvideDate);
                //}
                ViewBag.TodayDate = Convert.ToDateTime(Session["ServerDate"]).AddDays(0);
                ViewBag.maxDate = null;
                ViewBag.minDate = null;

                return View();
            }
        }
        public JsonResult AddSubAssemblyPlanning(AddDailyPlanModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty; int maxSqn = 0; string PlanId = string.Empty;
            try
            {
                int ProvideDate = 0, MaxQty = 0;
                string Loginfamily = Convert.ToString(Session["LoginFamily"]);

                if (data.Family == "ENGINE FTD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'EngineDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }
                if (data.Family == "HYDRAULIC FTD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'HydraulicDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }
                if (data.Family == "REAR AXEL FTD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'ReareAxleDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }
                if (data.Family == "TRANSMISSION FTD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'TransmissionDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }

                if (data.Family == "ENGINE TD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'EngineDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }
                if (data.Family == "BACK END TD")
                {
                    query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'PL_DAYS' AND PARAMETERINFO = 'BackendDays' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "'";
                }
                string ProvideDays = Convert.ToString(fun.get_Col_Value(query));
                if (!string.IsNullOrEmpty(ProvideDays))
                {
                    ProvideDate = Convert.ToInt32(ProvideDays);
                }

                DateTime date = Convert.ToDateTime(data.Date);

                query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'";

                object ExistingPlanId = fun.get_Col_Value(query);
                if (ExistingPlanId != "")
                {
                    PlanId = Convert.ToString(ExistingPlanId);

                    //Check Max production from SFT_Setting
                    #region Check Is Total Qty of plan as per family greater then XXES_SFT_SETTING QTY

                    if (data.Family == "ENGINE FTD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE FTD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE FTD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE FTD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }
                    if (data.Family == "TRANSMISSION FTD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'TRANSMISSION FTD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'TRANSMISSION FTD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'TRANSMISSION FTD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }
                    if (data.Family == "REAR AXEL FTD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'REAR AXEL FTD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'REAR AXEL FTD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'REAR AXEL FTD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }
                    if (data.Family == "HYDRAULIC FTD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'HYDRAULIC FTD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'HYDRAULIC FTD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'HYDRAULIC FTD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }

                    if (data.Family == "ENGINE TD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE TD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE TD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'ENGINE TD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }

                    if (data.Family == "BACK END TD")
                    {
                        if (data.Shift == "A")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'BACK END TD-ShiftA' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "B")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'BACK END TD-ShiftB' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                        if (data.Shift == "C")
                        {
                            query = "SELECT PARAMVALUE FROM xxes_sft_settings WHERE STATUS = 'MAX_PRO' AND PARAMETERINFO = 'BACK END TD-ShiftC' AND PLANT_CODE = '" + data.Plant + "'";
                        }
                    }

                    string MaxCapacity = Convert.ToString(fun.get_Col_Value(query));
                    if (string.IsNullOrEmpty(MaxCapacity))
                    {
                        MaxCapacity = "0";
                    }

                    //Getting Total QTY of Plan as per Plan_Id and Family_code
                    query = "SELECT SUM(QTY) FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                    string TotalExistQty = fun.get_Col_Value(query);

                    if (string.IsNullOrEmpty(TotalExistQty))
                    {
                        TotalExistQty = "0";
                    }
                    if ((Convert.ToInt32(TotalExistQty) + Convert.ToInt32(data.Qty)) > Convert.ToInt32(MaxCapacity))
                    {
                        msg = "Quantity excceds maximum shift production Qty ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }

                    #endregion

                    query = "SELECT MAX(SEQ_NO) FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";

                    object MaxSQN = fun.get_Col_Value(query);
                    if (MaxSQN != "")
                    {
                        maxSqn = Convert.ToInt32(MaxSQN);
                    }
                    int CurrentSqnNo = maxSqn + 1;

                    char[] spearator = { '#' };

                    String[] MainSubAssembly = data.ItemCode.Split(spearator, StringSplitOptions.None);
                    data.ItemCode = MainSubAssembly[0];
                    string ItemDesc = MainSubAssembly[1];

                    if (data.SeqForPerticularNo != null)
                    {
                        List<EditSQN> EditList = new List<EditSQN>();

                        query = "SELECT AUTOID, SEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO >= '" + data.SeqForPerticularNo + "' ORDER BY SEQ_NO";
                        dt = fun.returnDataTable(query);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) + 1 });
                        }

                        foreach (var item in EditList)
                        {
                            query = "UPDATE XXES_DAILY_PLAN_ASSEMBLY SET SEQ_NO = '" + item.Sqn + "' WHERE AUTOID = '" + item.AutoId + "' AND PLAN_ID = '" + PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                            if (fun.EXEC_QUERY(query))
                            {
                                //fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_ASSEMBLY", "Update_Sqn_Qty_While_Update", Convert.ToString(data.AutoId), query, data.Plant, data.Family);                               
                            }
                        }
                        query = "INSERT INTO XXES_DAILY_PLAN_ASSEMBLY(PLANT_CODE, FAMILY_CODE, PLAN_ID, QTY, SEQ_NO, UPDATED_DATE, UPDATED_BY, ITEMCODE, DESCRIPTION, STATUS) " +
                            "VALUES('" + data.Plant + "','" + data.Family + "','" + PlanId + "','" + data.Qty + "','" + data.SeqForPerticularNo + "', SYSDATE ,'" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "','" + data.ItemCode + "','" + ItemDesc + "', 'PENDING')";

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved Successfully...";
                            mstType = "alert-success";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        query = "INSERT INTO XXES_DAILY_PLAN_ASSEMBLY(PLANT_CODE, FAMILY_CODE, PLAN_ID, QTY, SEQ_NO, UPDATED_DATE, UPDATED_BY, ITEMCODE, DESCRIPTION, STATUS) " +
                            "VALUES('" + data.Plant + "','" + data.Family + "','" + PlanId + "','" + data.Qty + "','" + CurrentSqnNo + "', SYSDATE ,'" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "','" + data.ItemCode + "','" + ItemDesc + "', 'PENDING')";

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved Successfully...";
                            mstType = "alert-success";
                            var resul = new { Msg = msg, ID = mstType };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(AddDailyPlanModel data)
        {
            try
            {
                DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    int ProvideDate = 0;
                    //string Loginfamily = Convert.ToString(Session["LoginFamily"]);

                    query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                      Convert.ToString(data.Plant).Trim(),
                      Convert.ToString(data.Family).Trim());

                    string result = Convert.ToString(fun.get_Col_Value(query));

                    if (!string.IsNullOrEmpty(result))
                    {
                        ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                    }
                    DateTime date = Convert.ToDateTime(data.Date);
                    query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'");
                    //int PlanId = Convert.ToInt32(fun.get_Col_Value(query));
                    string PlanId = Convert.ToString(fun.get_Col_Value(query));
                    if (!string.IsNullOrEmpty(PlanId))
                    {
                        query = string.Format(@"SELECT PLAN_DATE FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'");
                        DateTime PlanDate = Convert.ToDateTime(fun.get_Col_Value(query));

                        ViewBag.PlanDate = "Plan Date : " + PlanDate.ToString("dd-MMM-yyyy");

                        query = "select a.AUTOID,a.TRAN_ID, a.SEQ_NO, a.PLAN_ID, a.ITEMCODE,SUBSTR( a.DESCRIPTION, 1, 50 ) AS DESCRIPTION, " +

                          "(SELECT DESCRIPTION1 ||'('|| SUBASSEMBLY1 ||')' FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SUBASSEMBLY1, " +
                          "(SELECT DESCRIPTION2 ||'('|| SUBASSEMBLY2 ||')' FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SUBASSEMBLY2, " +
                          "(SELECT SHORT_CODE FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SHORTCODE,";

                        if (Convert.ToString(data.Family).ToUpper().Contains("ENGINE"))
                        {
                            query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'ENF' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = a.ITEMCODE " +
                                        "AND SRNO NOT IN(SELECT ENGINE_SRLNO " +
                                        "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND ENGINE_SRLNO IS NOT NULL) )  AS AVL ";                          
                        }
                        else if (Convert.ToString(data.Family).ToUpper().Contains("REAR AXEL"))
                        {
                            query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'RAB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = a.ITEMCODE " +
                                        "AND SRNO NOT IN(SELECT REARAXEL_SRLNO " +
                                        "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND REARAXEL_SRLNO IS NOT NULL ) ) AS AVL ";                           
                        }
                        else if (Convert.ToString(data.Family).ToUpper().Contains("TRANSMISSION"))
                        {
                            query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = a.ITEMCODE " +
                                        "AND SRNO NOT IN(SELECT TRANSMISSION_SRLNO " +
                                        "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND TRANSMISSION_SRLNO IS NOT NULL) ) AS AVL ";                            
                        }
                        else if (Convert.ToString(data.Family).ToUpper().Contains("HYDRAULIC"))
                        {
                            query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = a.ITEMCODE " +
                                        "AND SRNO NOT IN(SELECT HYDRALUIC_SRLNO " +
                                        "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND HYDRALUIC_SRLNO IS NOT NULL) ) AS AVL ";                            
                        }
                        else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                        {
                            query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'BAB' AND PLANT_CODE = '" + data.Plant + "' AND DCODE = a.ITEMCODE " +
                                        "AND SRNO NOT IN(SELECT BACKEND_SRLNO " +
                                        "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND BACKEND_SRLNO IS NOT NULL) ) AS AVL ";                           
                        }

                        query += @" , a.QTY, (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code
                            and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) COMPLETED,
                            a.QTY - (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code
                            and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) PENDING
                            FROM xxes_daily_plan_assembly A 
                            WHERE a.PLANT_CODE = '" + Convert.ToString(data.Plant) + "' AND a.FAMILY_CODE = '" + Convert.ToString(data.Family) + "' AND plan_id = '" + PlanId + "' order by a.SEQ_NO ";

                        dt = fun.returnDataTable(query);
                        ViewBag.DataSource = dt;
                        if (dt.Rows.Count > 0)
                        {
                            ViewBag.VisibleSQN = "SHOW";
                        }
                        else
                        {
                            ViewBag.VisibleSQN = "HIDE";
                        }
                        return PartialView();
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
            }            
           
            ViewBag.DataSource = dt;
            return PartialView();
        }
        public JsonResult EditSeq(AddDailyPlanModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                query = "SELECT MAX(SEQ_NO) AS MAXIMUMSEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' ORDER BY SEQ_NO";
                int maxSqn = Convert.ToInt32(fun.get_Col_Value(query));

                if (data.TargetSeq > maxSqn)
                {
                    msg = "Sqn No. Should not be greater then last Sqn No. ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (data.TargetSeq < 1)
                {
                    msg = "Sqn No. Should not be less then 1 ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (data.TargetSeq == 0)
                {
                    msg = "Sqn No. Should not be 0 ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                //Check Qty should not be greater then COUNT of SUBASSEMBLY_ID of XXES_PRINT_SERIALS 
                query = "SELECT COUNT(*) FROM XXES_PRINT_SERIALS WHERE SUBASSEMBLY_ID = '" + data.AutoId + "'";
                int CountPrintSr = Convert.ToInt32(fun.get_Col_Value(query));
                if (CountPrintSr > 0)
                {
                    if (data.Qty < CountPrintSr)
                    {
                        msg = "Qty Should not be less then " + CountPrintSr + " ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                List<EditSQN> EditList = new List<EditSQN>();
                //Getting User Ip Address
                UserIpAdd = fun.GetUserIP();

                if (Convert.ToInt32(data.Seq) > data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.TargetSeq + "' AND '" + data.Seq + "' ORDER BY SEQ_NO";
                    RowAction = "UP";
                }
                else if (Convert.ToInt32(data.Seq) < data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.Seq + "' AND '" + data.TargetSeq + "' ORDER BY SEQ_NO";
                    RowAction = "DOWN";
                }
                else if (Convert.ToInt32(data.Seq) == data.TargetSeq)
                {
                    query = "SELECT AUTOID, QTY, SEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND SEQ_NO BETWEEN '" + data.Seq + "' AND '" + data.TargetSeq + "' ORDER BY SEQ_NO";
                    RowAction = "EQUAL";
                }
                else
                {
                    msg = "This row already contain this Sequence ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                dt = fun.returnDataTable(query);

                if (RowAction == "UP")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[0][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == TargetAutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) + 1 });
                        }
                        else if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == data.AutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                        }
                        else
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) + 1 });
                        }
                    }
                }
                if (RowAction == "DOWN")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == TargetAutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }
                        else if (Convert.ToInt32(dt.Rows[i]["AUTOID"]) == data.AutoId)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                        }
                        else
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = Convert.ToInt32(dt.Rows[i]["QTY"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }
                    }
                }
                if (RowAction == "EQUAL")
                {
                    int TargetAutoId = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][0]);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Qty = data.Qty, Sqn = data.TargetSeq });
                    }
                }

                if (EditList != null)
                {
                    int count = 0;
                    foreach (var item in EditList)
                    {
                        query = "UPDATE XXES_DAILY_PLAN_ASSEMBLY SET QTY = '" + item.Qty + "', SEQ_NO = '" + item.Sqn + "' WHERE AUTOID = '" + item.AutoId + "' AND PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                        if (fun.EXEC_QUERY(query))
                        {
                            fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_ASSEMBLY", "Update_Sqn_Qty_While_Update", Convert.ToString(data.AutoId), query, data.Plant, data.Family);
                            count++;
                        }
                    }
                    if (EditList.Count == count)
                    {
                        msg = "Data Saved Successfully...";
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteSubAssembly(AddDailyPlanModel data)
        {
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string RowAction = string.Empty;
            try
            {
                //Check AUTOID exist in SUBASSEMBLY_ID of XXES_PRINT_SERIALS 
                query = "SELECT COUNT(*) FROM XXES_PRINT_SERIALS WHERE SUBASSEMBLY_ID = '" + data.AutoId + "'";
                int CountPrintSr = Convert.ToInt32(fun.get_Col_Value(query));
                if (CountPrintSr > 0)
                {
                    msg = "This serial no has been printed, You can not delete this ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Delete Sub assembly item from XXES_DAILY_PLAN_ASSEMBLY
                    query = "DELETE FROM XXES_DAILY_PLAN_ASSEMBLY WHERE AUTOID = '" + data.AutoId + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        //Update Sequence in in XXES_DAILY_PLAN_ASSEMBLY
                        List<EditSQN> EditList = new List<EditSQN>();
                        query = "SELECT AUTOID, SEQ_NO FROM XXES_DAILY_PLAN_ASSEMBLY WHERE PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "' AND " +
                                "SEQ_NO > '" + data.Seq + "' ORDER BY SEQ_NO";

                        dt = fun.returnDataTable(query);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            EditList.Add(new EditSQN { AutoId = Convert.ToInt32(dt.Rows[i]["AUTOID"]), Sqn = Convert.ToInt32(dt.Rows[i]["SEQ_NO"]) - 1 });
                        }

                        if (EditList != null)
                        {
                            int count = 0;
                            foreach (var item in EditList)
                            {
                                query = "UPDATE XXES_DAILY_PLAN_ASSEMBLY SET SEQ_NO = '" + item.Sqn + "' WHERE AUTOID = '" + item.AutoId + "' AND PLAN_ID = '" + data.PlanId + "' AND FAMILY_CODE = '" + data.Family + "'";
                                if (fun.EXEC_QUERY(query))
                                {
                                    fun.Insert_Into_ActivityLog("XXES_DAILY_PLAN_ASSEMBLY", "Delete", Convert.ToString(data.AutoId), query, data.Plant, data.Family);
                                    count++;
                                }
                            }
                            if (EditList.Count == count)
                            {
                                msg = "Subassembly Deleted Successfully...";
                                mstType = "alert-success";
                                var resul = new { Msg = msg, ID = mstType };
                                return Json(resul, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-success";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ApprovePlan(AddDailyPlanModel data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty; int maxSqn = 0; string PlanId = string.Empty;
            try
            {
                int ProvideDate = 0, MaxQty = 0;

                query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                  Convert.ToString(data.Plant).Trim(),
                  Convert.ToString(data.Family).Trim());
                string pid = Convert.ToString(fun.get_Col_Value(query));

                if (!string.IsNullOrEmpty(pid))
                {
                    ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                }


                DateTime date = Convert.ToDateTime(data.Date);

                query = "SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'";

                object ExistingPlanId = fun.get_Col_Value(query);

                if (ExistingPlanId != "")
                {
                    PlanId = Convert.ToString(ExistingPlanId);

                    query = "DELETE FROM XXES_SUBASEMBLY_APPROVEDPLAN WHERE PLANT_CODE = '" + data.Plant + "'AND FAMILY_CODE = '" + data.Family + "'AND PLAN_ID =   '" + PlanId + "'AND   SHIFT = '" + data.Shift + "'";

                    fun.EXEC_QUERY(query);

                    query = "INSERT INTO XXES_SUBASEMBLY_APPROVEDPLAN(PLANT_CODE, FAMILY_CODE, PLAN_ID, PLAN_DATE, SHIFT, STATUS, CREATED_DATE, CREATED_BY) " +
                        "VALUES('" + data.Plant + "','" + data.Family + "','" + PlanId + "', '" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "', '" + data.Shift + "', 'APPROVED', SYSDATE ,'" + System.Web.HttpContext.Current.User.Identity.Name.ToString() + "')";

                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Plan Approved ...";
                        mstType = "alert-success";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                var resul = new { Msg = msg, ID = mstType };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            mstType = "alert-success";
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult GetMaxSequenceOfExistingSubPlan(AddDailyPlanModel data)
        {
            //int Max = 0;
            List<DDLTextValue> Seqn = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    int ProvideDate = 0;
                    string Loginfamily = Convert.ToString(Session["LoginFamily"]);

                    query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                     Convert.ToString(data.Plant).Trim(),
                     Convert.ToString(data.Family).Trim());
                    string pid = Convert.ToString(fun.get_Col_Value(query));

                    if (!string.IsNullOrEmpty(pid))
                    {
                        ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                    }


                    DateTime date = Convert.ToDateTime(data.Date);

                    query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'");

                    int PlanId = Convert.ToInt32(fun.get_Col_Value(query));

                    //query = string.Format(@"select MAX(a.SEQ_NO) as MaxSqn
                    //FROM xxes_daily_plan_assembly A
                    //WHERE a.PLANT_CODE='{0}' AND a.FAMILY_CODE='{1}' AND plan_id='{2}' order by a.SEQ_NO ",
                    //Convert.ToString(data.Plant), Convert.ToString(data.Family), PlanId);

                    //Max = Convert.ToInt32(fun.get_Col_Value(query));

                    //Check Sequence No. where pending is 0
                    #region Check Sequence No. where pending is 0
                    query = "select a.AUTOID,a.SEQ_NO," +
                        "a.QTY, (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code " +
                        "and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) COMPLETED, " +
                        "a.QTY - (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code " +
                        "and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) PENDING " +
                        "FROM xxes_daily_plan_assembly A " +
                        "WHERE a.PLANT_CODE = '" + Convert.ToString(data.Plant) + "' AND a.FAMILY_CODE = '" + Convert.ToString(data.Family) + "' AND plan_id = '" + PlanId + "' order by a.SEQ_NO ";
                    dt = fun.returnDataTable(query);

                    #endregion

                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        if (Convert.ToInt32(dr["PENDING"]) > 0)
                        {
                            Seqn.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["SEQ_NO"]),
                                Value = Convert.ToString(dr["SEQ_NO"]),
                            });
                        }
                    }
                    if (Seqn != null)
                    {
                        string Max = Seqn.Select(x => x.Value).Max();
                        Seqn.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(Convert.ToInt32(Max) + 1),
                            Value = Convert.ToString(Convert.ToInt32(Max) + 1),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
            }

            ViewBag.DataSource = new SelectList(Seqn, "Value", "Text");
            return PartialView();
        }
        public PartialViewResult GetMaxSequenceOfExistingSubPlanForEdit(AddDailyPlanModel data)
        {
            //int Max = 0;
            List<DDLTextValue> Seqn = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    int ProvideDate = 0;
                    string Loginfamily = Convert.ToString(Session["LoginFamily"]);

                    query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                     Convert.ToString(data.Plant).Trim(),
                     Convert.ToString(data.Family).Trim());
                    string pid = Convert.ToString(fun.get_Col_Value(query));

                    if (!string.IsNullOrEmpty(pid))
                    {
                        ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                    }


                    DateTime date = Convert.ToDateTime(data.Date);

                    query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + date.AddDays(ProvideDate).Date.ToString("dd-MMM-yyyy") + "' AND PLANT_CODE = '" + data.Plant + "' AND FAMILY_CODE = '" + data.Family + "' AND SHIFTCODE = '" + data.Shift + "'");

                    int PlanId = Convert.ToInt32(fun.get_Col_Value(query));

                    //Check Sequence No. where pending is 0
                    #region Check Sequence No. where pending is 0
                    query = "select a.AUTOID,a.SEQ_NO," +
                        "a.QTY, (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code " +
                        "and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) COMPLETED, " +
                        "a.QTY - (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code " +
                        "and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) PENDING " +
                        "FROM xxes_daily_plan_assembly A " +
                        "WHERE a.PLANT_CODE = '" + Convert.ToString(data.Plant) + "' AND a.FAMILY_CODE = '" + Convert.ToString(data.Family) + "' AND plan_id = '" + PlanId + "' order by a.SEQ_NO ";
                    dt = fun.returnDataTable(query);

                    #endregion

                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        if (Convert.ToInt32(dr["PENDING"]) > 0)
                        {
                            Seqn.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["SEQ_NO"]),
                                Value = Convert.ToString(dr["SEQ_NO"]),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
            }

            ViewBag.DataSource = new SelectList(Seqn, "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindPlant()
        {
            ViewBag.Unit = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }
        public PartialViewResult BindFamily(string Plant)
        {

            if (!string.IsNullOrEmpty(Plant))
            {
                ViewBag.Family = new SelectList(fun.Fill_FamilyForSubAssembly(Plant), "Value", "Text");
            }
            return PartialView();
        }
        public PartialViewResult BindShift()
        {
            string Shift = fun.getshift();
            char[] Spearator = { '#' };
            String[] StrShift = Shift.Split(Spearator, StringSplitOptions.None);
            string SelectedShift = StrShift[0];

            ViewBag.Shift = new SelectList(fun.FillShift(), "Value", "Text", SelectedShift);
            return PartialView();
        }
        public JsonResult CurrentShift()
        {
            string Shift = fun.getshift();
            char[] Spearator = { '#' };
            String[] StrShift = Shift.Split(Spearator, StringSplitOptions.None);
            string SelectedShift = StrShift[0];
            return Json(SelectedShift, JsonRequestBehavior.AllowGet);
        }
        private List<DDLTextValue> FillItemMaster(AddDailyPlanModel data)
        {
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string query = string.Empty;
                if (!string.IsNullOrEmpty(data.Family))
                {
                    string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                    string orgid = fun.getOrgId(Convert.ToString(data.Plant).Trim().ToUpper(), Convert.ToString(data.Family).Trim().ToUpper());

                    DataTable dt = new DataTable();

                    if (Convert.ToString(data.Family).ToUpper().Contains("ENGINE"))
                    {
                        //query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1";
                        query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and (DESCRIPTION like 'ENGINE ASSEMBLY%' or DESCRIPTION like 'MOTOR ASSEMBLY%') order by segment1";
                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("REAR AXEL"))
                    {
                        query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'AXLE ASSEMBLY%' order by segment1";
                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("TRANSMISSION"))
                    {
                        query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1";
                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("HYDRAULIC"))
                    {
                        query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'HYDRAULIC LIFT ASSEMBLY%' order by segment1";
                    }
                    else if (Convert.ToString(data.Family).ToUpper().Contains("BACK END"))
                    {
                        query = "select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and (description like 'BACKEND%' or description like 'SKID%') order by segment1";
                    }
                    if (!string.IsNullOrEmpty(query))
                    {
                        dt = fun.returnDataTable(query);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            Item.Add(new DDLTextValue
                            {
                                Text = dr["DESCRIPTION"].ToString(),
                                Value = dr["DESCRIPTION"].ToString(),
                            });
                        }
                    }
                }
                return Item;
            }
            catch (Exception ex)
            {
                //throw;
                return Item;
            }
            finally { }
        }
        public PartialViewResult BindItems(AddDailyPlanModel data)
        {
            ViewBag.Items = new SelectList(FillItemMaster(data), "Value", "Text");
            return PartialView();
        }
    }
}