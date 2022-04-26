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
using Oracle.Web;
using System.Data;
using Newtonsoft.Json;
using System.Configuration;

namespace MVCApp.Controllers
{
    [Authorize]
    public class LCDLiveController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

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

        public PartialViewResult Grid(LCDLiveModel lcd)
        {
            try
            {
                
                string Plant = "", Family = "", Shift = "", Str = ""; DateTime Date = new DateTime();
                string Shiftcode = "", isDayNeedToLess = ""; //DateTime dtDeleteJobs = new DateTime();
                string data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    if (string.IsNullOrEmpty(lcd.Plant) || string.IsNullOrEmpty(lcd.Family))
                    {
                        ViewBag.Error = "Both Plant and Family is required.";
                        return PartialView();
                    }
                    Plant = lcd.Plant;
                    Family = lcd.Family;

                    Shiftcode = data.Split('#')[0].Trim().ToUpper();

                    Shift = data.Split('#')[0].Trim().ToUpper();

                    ViewBag.Shift = data.Split('#')[0].Trim().ToUpper();

                    Date = fun.GetServerDateTime();
                    ViewBag.Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                    Str = Convert.ToString(Date);
                    char[] Spearator = { ' ' };
                    String[] StrDate = Str.Split(Spearator, StringSplitOptions.None);

                    ViewBag.Time = StrDate[0] + " " + StrDate[1];
                }
                if (string.IsNullOrEmpty(Plant) || string.IsNullOrEmpty(Family))
                {
                    ViewBag.Error = "Both Plant and Family is required.";
                    return PartialView();
                }

                query = string.Format(@"Select TRAN_ID from XXES_PRINT_SERIALS where rownum=1 and FAMILY_CODe='" + Family + "' and  plant_code='" + Plant + "' and tran_id is not null order by PrintDate");

                string TranID = fun.get_Col_Value(query);

                int ProvideDate = 0;
                //string Loginfamily = Convert.ToString(Session["LoginFamily"]);


                query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                  Convert.ToString(Plant).Trim(),
                  Convert.ToString(Family).Trim());

                string result = Convert.ToString(fun.get_Col_Value(query));

                if (!string.IsNullOrEmpty(result))
                {
                    ProvideDate = Convert.ToInt32(fun.get_Col_Value(query));
                }

                DateTime date = Date;

                String APlanExpectedDate = date.AddDays(ProvideDate).ToString("dd-MMM-yyyy");


                query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE to_char(PLAN_DATE,'dd-Mon-yyyy')='" + APlanExpectedDate + "' AND PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND SHIFTCODE = '" + Shift + "'");
                string PlanId = Convert.ToString(fun.get_Col_Value(query));
                //ViewBag.error = PlanId;


                query = "select a.AUTOID,a.TRAN_ID, a.SEQ_NO, a.PLAN_ID, a.ITEMCODE,SUBSTR( a.DESCRIPTION, 1, 50 ) AS DESCRIPTION, " +

                        "(SELECT DESCRIPTION1 ||'('|| SUBASSEMBLY1 ||')' FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SUBASSEMBLY1, " +
                        "(SELECT DESCRIPTION2 ||'('|| SUBASSEMBLY2 ||')' FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SUBASSEMBLY2, " +
                        "(SELECT SHORT_CODE FROM XXES_SUB_ASSEMBLY_MASTER WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND MAIN_SUB_ASSEMBLY = a.ITEMCODE) AS SHORTCODE, ";

                if (Convert.ToString(Family).ToUpper().Contains("ENGINE"))
                {
                    query += "(select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'ENF' AND PLANT_CODE = '" + Plant + "' AND DCODE = a.ITEMCODE " +
                                "AND SRNO NOT IN(SELECT ENGINE_SRLNO " +
                                "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND ENGINE_SRLNO IS NOT NULL) )  AS AVL ";
                }
                else if (Convert.ToString(Family).ToUpper().Contains("REAR AXEL"))
                {
                    query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'RAB' AND PLANT_CODE = '" + Plant + "' AND DCODE = a.ITEMCODE " +
                                "AND SRNO NOT IN(SELECT REARAXEL_SRLNO " +
                                "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND REARAXEL_SRLNO IS NOT NULL ) ) AS AVL ";
                }
                else if (Convert.ToString(Family).ToUpper().Contains("TRANSMISSION"))
                {
                    query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '" + Plant + "' AND DCODE = a.ITEMCODE " +
                                "AND SRNO NOT IN(SELECT TRANSMISSION_SRLNO " +
                                "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND TRANSMISSION_SRLNO IS NOT NULL) ) AS AVL ";
                }
                else if (Convert.ToString(Family).ToUpper().Contains("HYDRAULIC"))
                {
                    query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '" + Plant + "' AND DCODE = a.ITEMCODE " +
                                "AND SRNO NOT IN(SELECT HYDRALUIC_SRLNO " +
                                "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND HYDRALUIC_SRLNO IS NOT NULL) ) AS AVL ";
                }
                else if (Convert.ToString(Family).ToUpper().Contains("BACK END"))
                {
                    query += " (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = 'BAB' AND PLANT_CODE = '" + Plant + "' AND DCODE = a.ITEMCODE " +
                                "AND SRNO NOT IN(SELECT BACKEND_SRLNO " +
                                "FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Plant + "' AND FAMILY_CODE = '" + Family + "' AND BACKEND_SRLNO IS NOT NULL) ) AS AVL ";
                }

                query += @" , a.QTY, (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code
                            and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) COMPLETED,
                            a.QTY - (select COUNT(*) from xxes_print_serials where plant_code = a.plant_code
                            and family_code = a.family_code and SUBASSEMBLY_ID = a.AUTOID) PENDING
                            FROM xxes_daily_plan_assembly A 
                            WHERE a.PLANT_CODE = '" + Convert.ToString(Plant) + "' AND a.FAMILY_CODE = '" + Convert.ToString(Family) + "' AND plan_id = '" + PlanId + "' order by a.SEQ_NO ";

            }
            catch (Exception ex)
            {
                ViewBag.error = ex;
            }
            dt = fun.returnDataTable(query);
            ViewBag.DataSource = dt;

            if (dt.Rows.Count > 0)
            {
                ViewBag.Planned = Convert.ToInt32(dt.Compute("SUM(QTY)", string.Empty));
                ViewBag.Completed = Convert.ToInt32(dt.Compute("SUM(COMPLETED)", string.Empty));
                ViewBag.Pending = Convert.ToInt32(dt.Compute("SUM(PENDING)", string.Empty));

            }
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
                List<DDLTextValue> dDLTextValues = fun.Fill_FamilyForSubAssembly(Plant);
                if(dDLTextValues.Count > 0)
                {
                    ViewBag.Family = new SelectList(dDLTextValues, "Value", "Text");
                    ViewBag.Error = "";
                }
                else
                {
                    ViewBag.Family = new SelectList(dDLTextValues, "Value", "Text");
                    ViewBag.Error = "Both Plant and Family is required.";
                }
                
                
            }
            return PartialView();
        }
    }
}