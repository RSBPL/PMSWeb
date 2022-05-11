using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.Linq;
using MVCApp.Models;

namespace MVCApp.Controllers.LCD
{
    public class DailyPartScanningScreenController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;

        Function fun = new Function();
        string query = string.Empty;

        // GET: DailyPartScanningScreen
        public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
            ViewBag.DefaultProdDate = DateTime.Now;
            return View();
        }
        public string get_Col_Value(string command)
        {
            OracleConnection ConOrcl = new OracleConnection(Function.orCnstr);
            try
            {
                string returnValue = "";
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = ConOrcl;
                cmd.CommandText = command;
                cmd.CommandType = CommandType.Text;
                returnValue = Convert.ToString(cmd.ExecuteScalar());
                return returnValue;
            }
            catch { throw; return ""; }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }


        [HttpGet]
        public JsonResult getPlanVertical(string PLANTCODE,string SelectedDate)
        {
            var result = new GridData();
            var gridleft = new List<gridtabl>();

            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                DateTime dateTime = Convert.ToDateTime(SelectedDate);
                string plant = "", family = "", DisplayMethod = "";
                plant = PLANTCODE;
                if (plant == "4")
                {
                    plant = "T04";
                    family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);

                }
                if (PLANTCODE == "5")
                {
                    plant = "T05";
                    family = Convert.ToString(ConfigurationManager.AppSettings["PT_FAMILY"]);
                }

                string Shiftcode = "", NightExists = "",  data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); DateTime plusday = new DateTime(); string expqty = "";
                DateTime ShiftStart, shiftEnd;    
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    result.lblError = "";
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    NightExists = data.Split('#')[1].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    ShiftStart = Convert.ToDateTime(data.Split('#')[3].Trim());
                    shiftEnd = Convert.ToDateTime(data.Split('#')[4].Trim());
                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                    {
                        Plandate = fun.ServerDate.Date.AddDays(-1);
                    } 
                    else
                    {
                        //Plandate = fun.ServerDate.Date;
                        Plandate = dateTime.Date;
                    } 
                    

                    //plusday = fun.ServerDate.Date.AddDays(1);
                    plusday = dateTime.Date.AddDays(1);

                    string Todays = plusday.ToString("dd MMM yyyy") + " " + "01:00:00";
                    string Fromdays = Plandate.ToString("dd MMM yyyy") + " " + "08:00:00";


                    result.lblDate = "DATE: " + fun.ServerDate.Date.ToString("dd MMM yyyy");
                    query = "";

                    DataTable QueryData = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query = string.Format(@"SELECT B.*,
                                           ROUND((TOTAL / TOTAL_PROD) * 100, 2) EFFICIENCY
                                      FROM (SELECT A.*,
                                                   (SELECT COUNT(*)
                                                       FROM XXES_JOB_STATUS J
                                                       WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                         AND PLANT_CODE = '{0}') TOTAL_PROD

                                          FROM (SELECT 'BACKEND' HEAD,
                                                       COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                                AND PLANT_CODE = '{0}'
                                                AND J.BACKEND_SRLNO IS NOT NULL
                                              UNION

                                            SELECT 'TRANMISSION' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                                AND PLANT_CODE = '{0}'
                                                AND J.TRANSMISSION_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'REARAXEL' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.REARAXEL_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'ENGINE' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.ENGINE_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'HYDRAULIC' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.HYDRALUIC_SRLNO IS NOT NULL
                                              UNION
                                           -- SELECT 'ENGINE FIP' HEAD,
                                            --       COUNT(*) TOTAL
                                             -- FROM XXES_JOB_STATUS J
                                            --  WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                             -- AND PLANT_CODE = '{0}'
                                             --   AND J.FIPSRNO IS NOT NULL
                                             -- UNION
                                            SELECT 'STARTER MOTOR' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                             WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.STARTER_MOTOR_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'ALTERNATOR' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.ALTERNATOR_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'RADIATOR' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.RADIATOR_SRLNO IS NOT NULL
                                              UNION
                                            SELECT 'FRONT TYRE RIGHT' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.FRONTTYRE_SRLNO1 IS NOT NULL
                                              UNION
                                            SELECT 'FRONT TYRE LEFT' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.FRONTTYRE_SRLNO2 IS NOT NULL
                                              UNION
                                            SELECT 'REAR TYRE RIGHT' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.REARTYRE_SRLNO1 IS NOT NULL
                                              UNION
                                            SELECT 'REAR TYRE LEFT' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.REARTYRE_SRLNO2 IS NOT NULL
                                              UNION
                                            SELECT 'CARE BUTTON' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.SIM_SERIAL_NO IS NOT NULL
                                              UNION
                                            SELECT 'BATTERY' HEAD,
                                                   COUNT(*) TOTAL
                                              FROM XXES_JOB_STATUS J
                                              WHERE TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                                              AND PLANT_CODE = '{0}'
                                                AND J.BATTERY_SRLNO IS NOT NULL) A
                                          GROUP BY A.HEAD,
                                                   A.TOTAL
                                          ORDER BY CASE head WHEN 'TRANMISSION' THEN 1 WHEN 'REARAXEL' THEN 2 
                                        WHEN 'BACKEND' THEN 3 WHEN 'ENGINE' THEN 4 WHEN 'ENGINE FIP' THEN 5 
                                        WHEN 'HYDRAULIC' THEN 6 WHEN 'ALTERNATOR' THEN 7 WHEN 'STARTER MOTOR' THEN 8 
                                        WHEN 'RADIATOR' THEN 9 WHEN 'REAR TYRE LEFT' THEN 10 WHEN 'REAR TYRE RIGHT' THEN 11 
                                        WHEN 'FRONT TYRE LEFT' THEN 12 WHEN 'FRONT TYRE RIGHT' THEN 13 
                                        WHEN 'CARE BUTTON' THEN 14 WHEN 'BATTERY' THEN 15 END) B",
                            plant.Trim(), Plandate.ToString("dd-MMM-yyyy"));
                        //query = string.Format(@"SELECT B.*,
                        //                   ROUND((TOTAL / TOTAL_PROD) * 100, 2) EFFICIENCY
                        //              FROM (SELECT A.*,
                        //                           (SELECT COUNT(*)
                        //                               FROM XXES_JOB_STATUS J
                        //                               WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                        //                                 AND PLANT_CODE = '{0}') TOTAL_PROD

                        //                  FROM (SELECT 'BACKEND' HEAD,
                        //                               COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN TO_DATE('{2}', 'DD-MON-YYYY HH24:MI:SS')
                        //                        AND TO_DATE('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                        AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                        AND PLANT_CODE = '{0}'
                        //                        AND J.BACKEND_SRLNO IS NOT NULL
                        //                      UNION

                        //                    SELECT 'TRANMISSION' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN TO_DATE('{2}', 'DD-MON-YYYY HH24:MI:SS')
                        //                        AND TO_DATE('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                        AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                        AND PLANT_CODE = '{0}'
                        //                        AND J.TRANSMISSION_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'REARAXEL' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.REARAXEL_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'ENGINE' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.ENGINE_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'HYDRAULIC' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.HYDRALUIC_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'ENGINE FIP' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.FIPSRNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'STARTER MOTOR' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                     WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.STARTER_MOTOR_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'ALTERNATOR' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.ENTRYDATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.ENTRYDATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.ALTERNATOR_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'RADIATOR' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.RADIATOR_SRLNO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'FRONT TYRE RIGHT' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.FRONTTYRE_SRLNO1 IS NOT NULL
                        //                      UNION
                        //                    SELECT 'FRONT TYRE LEFT' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.FRONTTYRE_SRLNO2 IS NOT NULL
                        //                      UNION
                        //                    SELECT 'REAR TYRE RIGHT' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.REARTYRE_SRLNO1 IS NOT NULL
                        //                      UNION
                        //                    SELECT 'REAR TYRE LEFT' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.REARTYRE_SRLNO2 IS NOT NULL
                        //                      UNION
                        //                    SELECT 'CARE BUTTON' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.SIM_SERIAL_NO IS NOT NULL
                        //                      UNION
                        //                    SELECT 'BATTERY' HEAD,
                        //                           COUNT(*) TOTAL
                        //                      FROM XXES_JOB_STATUS J
                        //                      WHERE J.FINAL_LABEL_DATE BETWEEN to_date('{2}', 'DD-MON-YYYY HH24:MI:SS' )
                        //                      AND to_date('{3}', 'DD-MON-YYYY HH24:MI:SS')
                        //                      AND TRUNC(J.FINAL_LABEL_DATE) = '{1}'
                        //                      AND PLANT_CODE = '{0}'
                        //                        AND J.BATTERY_SRLNO IS NOT NULL) A
                        //                  GROUP BY A.HEAD,
                        //                           A.TOTAL
                        //                  ORDER BY CASE head WHEN 'TRANMISSION' THEN 1 WHEN 'REARAXEL' THEN 2 
                        //                WHEN 'BACKEND' THEN 3 WHEN 'ENGINE' THEN 4 WHEN 'ENGINE FIP' THEN 5 
                        //                WHEN 'HYDRAULIC' THEN 6 WHEN 'ALTERNATOR' THEN 7 WHEN 'STARTER MOTOR' THEN 8 
                        //                WHEN 'RADIATOR' THEN 9 WHEN 'REAR TYRE LEFT' THEN 10 WHEN 'REAR TYRE RIGHT' THEN 11 
                        //                WHEN 'FRONT TYRE LEFT' THEN 12 WHEN 'FRONT TYRE RIGHT' THEN 13 
                        //                WHEN 'CARE BUTTON' THEN 14 WHEN 'BATTERY' THEN 15 END) B",
                        //    plant.Trim(), Plandate.ToString("dd-MMM-yyyy"), Fromdays, Todays);



                        QueryData = fun.returnDataTable(query);

                        if (QueryData.Rows.Count < 1)
                        {
                            result.lblError = "NO RECORD FOUND FOUND FOR CURRENT DATE";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {

                        try
                        {
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                                fun.ConOpen();
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DPSS";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                OracleDataAdapter dr = new OracleDataAdapter(sc);
                                dr.Fill(dt);
                                fun.ConClose();
                            }
                        }
                        catch (Exception ex)
                        {
                            fun.LogWrite(ex);
                            throw;
                        }
                        finally
                        {
                            fun.ConClose();
                            fun.Connection().Dispose();
                        }
                        //return ;

                    }
                    try
                    {
                        DataTable NEWRECORD = new DataTable();
                        NEWRECORD.Columns.Add("HEAD");
                        NEWRECORD.Columns.Add("TOTAL");
                        NEWRECORD.Columns.Add("TOTAL_PROD");
                        NEWRECORD.Columns.Add("EFFICIENCY");
                        if (QueryData.Rows.Count < 0)
                        {
                            result.lblError = "";
                            result.lblErrorVisible = true;

                        }
                        if (QueryData.Rows.Count > 0)
                        {

                            for (int i = 0; i < QueryData.Rows.Count; i++)
                            {
                                if (plant == "T05")
                                {
                                    string hednme = QueryData.Rows[i]["HEAD"].ToString();
                                    if (QueryData.Rows[i]["HEAD"].ToString() != "TRANMISSION" && QueryData.Rows[i]["HEAD"].ToString() != "REARAXEL")
                                    {
                                        DataRow dataRow = NEWRECORD.NewRow();
                                        dataRow["HEAD"] = QueryData.Rows[i]["HEAD"].ToString();
                                        dataRow["TOTAL"] = QueryData.Rows[i]["TOTAL"].ToString();
                                        dataRow["TOTAL_PROD"] = QueryData.Rows[i]["TOTAL_PROD"].ToString();
                                        dataRow["EFFICIENCY"] = QueryData.Rows[i]["EFFICIENCY"].ToString();
                                        NEWRECORD.Rows.Add(dataRow);
                                    }
                                }
                                else if (plant == "T04")
                                {
                                    if (QueryData.Rows[i]["HEAD"].ToString() != "BACKEND")
                                    {
                                        DataRow dataRow = NEWRECORD.NewRow();
                                        dataRow["HEAD"] = QueryData.Rows[i]["HEAD"].ToString();
                                        dataRow["TOTAL"] = QueryData.Rows[i]["TOTAL"].ToString();
                                        dataRow["TOTAL_PROD"] = QueryData.Rows[i]["TOTAL_PROD"].ToString();
                                        dataRow["EFFICIENCY"] = QueryData.Rows[i]["EFFICIENCY"].ToString();
                                        NEWRECORD.Rows.Add(dataRow);
                                    }
                                }
                            }

                            int srno = 1;
                            foreach (DataRow dr in NEWRECORD.Rows)
                            {

                                var gridleftdata = new gridtabl();
                                gridleftdata.SRNO = Convert.ToString(srno);
                                gridleftdata.HEAD = dr["HEAD"].ToString();
                                gridleftdata.TOTAL_PROD = dr["TOTAL_PROD"].ToString();
                                gridleftdata.Quantity = dr["TOTAL"].ToString();
                                gridleftdata.EFFICIENCY = Convert.ToDouble(dr["EFFICIENCY"]);

                                gridleft.Add(gridleftdata);
                                srno++;
                            }
                        }
                        else
                        {
                            result.lblBKTotDayVisible = false;
                            //gridVertical.DataSource = null;
                            //gridVertical.DataBind();
                            result.lblPendingVisible = false;
                            result.lblBKVisible = false;
                            result.lblPlanQtyVisible = false;
                        }
                        result.lblErrorVisible = false;
                    }
                    catch (Exception ex)
                    {
                        result.lblError = ex.Message.ToString();
                        result.lblErrorVisible = true;

                    }
                    finally { }
                    getMsg();
                }
            }
            catch (Exception ex)
            {
                //result.Visible = true;

                //result.lblError = ex.Message.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            finally { }
            result.lblTime = "TIME: " + fun.ServerDate.ToString("HH:mm:ss");
            result.gridleft = gridleft;
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult DisplayChart(string PLANTCODE, string SelectedDate)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                List<gridtabl> chartModels = ChartDataMaster(PLANTCODE, SelectedDate);
                if(chartModels.Count==0)
                {
                    fun.WriteLog("LCD CHART DATA NOT DOUND");
                }
                JsonResult result = Json(chartModels, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
                TempData["msgType"] = "alert-danger";
                return new JsonResult() { Data = 12, MaxJsonLength = Int32.MaxValue };
            }
        }
        public DateTime ServerDate;
        public List<gridtabl> ChartDataMaster(string PLANTCODE, string SelectedDate)
        {
            string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime();DateTime plusday = new DateTime();
            DateTime dateTime = Convert.ToDateTime(SelectedDate);
            DateTime ShiftStart, shiftEnd;
            List<gridtabl> CM = new List<gridtabl>();
            string DisplayMethod = string.Empty;
            DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
            string plant = "";
            plant = PLANTCODE;
            if (plant == "4")
            {
                plant = "T04";

            }
            if (PLANTCODE == "5")
            {
                plant = "T05";
            }
            data = fun.getshift();
            if (!string.IsNullOrEmpty(data))
            {
                Shiftcode = data.Split('#')[0].Trim().ToUpper();
                NightExists = data.Split('#')[1].Trim().ToUpper();
                isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                ShiftStart = Convert.ToDateTime(data.Split('#')[3].Trim());
                shiftEnd = Convert.ToDateTime(data.Split('#')[4].Trim());

                if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                    Plandate = fun.GetServerDateTime().Date.AddDays(-1);
                else
                    //Plandate = fun.GetServerDateTime();
                    Plandate = dateTime.Date;
                string Fromdays = plusday.ToString("dd MMM yyyy") + " " + "01:00:00";
                string Todays = Plandate.ToString("dd MMM yyyy") + " " + "08:00:00";

                DataTable DATACHART = new DataTable();
                DATACHART.Columns.Add("HEAD");
                DATACHART.Columns.Add("TOTAL");
                DATACHART.Columns.Add("TOTAL_PROD");
                DATACHART.Columns.Add("EFFICIENCY");

                try
                {
                    DataTable QueryData1 = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query = string.Format(@"SELECT B.*,
                                                   ROUND((TOTAL / TOTAL_PROD) * 100, 2) EFFICIENCY
                                              FROM (SELECT A.*,
                                                           (SELECT COUNT(*)
                                                               FROM XXES_JOB_STATUS J
                                                               WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                                 AND PLANT_CODE = '{0}') TOTAL_PROD
                                                  FROM (SELECT 'BACKEND' HEAD,
                                                               COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.BACKEND_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'TRANMISSION' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.TRANSMISSION_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'REARAXEL' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.REARAXEL_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'ENGINE' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.ENGINE_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'HYDRAULIC' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.HYDRALUIC_SRLNO IS NOT NULL
                                                      UNION
                                                  --  SELECT 'ENGINE FIP' HEAD,
                                                     --      COUNT(*) TOTAL
                                                     -- FROM XXES_JOB_STATUS J
                                                    --  WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                      --  AND PLANT_CODE = '{0}'
                                                      --  AND J.FIPSRNO IS NOT NULL
                                                   --   UNION
                                                    SELECT 'STARTER MOTOR' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.STARTER_MOTOR_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'ALTERNATOR' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.ALTERNATOR_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'RADIATOR' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.RADIATOR_SRLNO IS NOT NULL
                                                      UNION
                                                    SELECT 'FRONT TYRE RIGHT' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.FRONTTYRE_SRLNO1 IS NOT NULL
                                                      UNION
                                                    SELECT 'FRONT TYRE LEFT' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.FRONTTYRE_SRLNO2 IS NOT NULL
                                                      UNION
                                                    SELECT 'REAR TYRE RIGHT' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.REARTYRE_SRLNO1 IS NOT NULL
                                                      UNION
                                                    SELECT 'REAR TYRE LEFT' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.REARTYRE_SRLNO2 IS NOT NULL
                                                      UNION
                                                    SELECT 'CARE BUTTON' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.SIM_SERIAL_NO IS NOT NULL
                                                      UNION
                                                    SELECT 'BATTERY' HEAD,
                                                           COUNT(*) TOTAL
                                                      FROM XXES_JOB_STATUS J
                                                      WHERE TRUNC(FINAL_LABEL_DATE) = '{1}'
                                                        AND PLANT_CODE = '{0}'
                                                        AND J.BATTERY_SRLNO IS NOT NULL 
                                                        )A
                                                        GROUP BY A.HEAD,A.TOTAL
                                                       ORDER BY CASE head 
                                                       WHEN 'TRANMISSION' THEN 1
                                                       WHEN 'REARAXEL' THEN 2
                                                       WHEN 'BACKEND' THEN 3
                                                       WHEN 'ENGINE' THEN 4
                                                       WHEN 'ENGINE FIP' THEN 5 
                                                       WHEN 'HYDRAULIC' THEN 6
                                                       WHEN 'ALTERNATOR' THEN 7
                                                       WHEN 'STARTER MOTOR' THEN 8
                                                       WHEN 'RADIATOR' THEN 9
                                                       WHEN 'REAR TYRE LEFT' THEN 10
                                                       WHEN 'REAR TYRE RIGHT' THEN 11
                                                       WHEN 'FRONT TYRE LEFT' THEN 12
                                                       WHEN 'FRONT TYRE RIGHT' THEN 13
                                                       WHEN 'CARE BUTTON' THEN 14
                                                       WHEN 'BATTERY' THEN 15      
                                                       END) 
                                                        B", plant.Trim(), Plandate.ToString("dd-MMM-yyyy"));


                        QueryData1 = fun.returnDataTable(query);
                    }
                    else
                    {
                        using (OracleCommand oc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                        {
                            fun.ConOpen();
                            oc.CommandType = CommandType.StoredProcedure;
                            oc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DPSS";
                            oc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                            oc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);

                            OracleDataAdapter oda = new OracleDataAdapter(oc);
                            DataTable dt = new DataTable();
                            oda.Fill(dt);
                        }
                    }
                    if (QueryData1.Rows.Count > 0)
                    {

                        for (int i = 0; i < QueryData1.Rows.Count; i++)
                        {
                            if (plant == "T05")
                            {
                                string hednme = QueryData1.Rows[i]["HEAD"].ToString();
                                if (QueryData1.Rows[i]["HEAD"].ToString() != "TRANMISSION" && QueryData1.Rows[i]["HEAD"].ToString() != "REARAXEL")
                                {
                                    DataRow dataRow = DATACHART.NewRow();
                                    dataRow["HEAD"] = QueryData1.Rows[i]["HEAD"].ToString();
                                    dataRow["TOTAL"] = QueryData1.Rows[i]["TOTAL"].ToString();
                                    dataRow["TOTAL_PROD"] = QueryData1.Rows[i]["TOTAL_PROD"].ToString();
                                    dataRow["EFFICIENCY"] = QueryData1.Rows[i]["EFFICIENCY"].ToString();
                                    DATACHART.Rows.Add(dataRow);
                                }
                            }
                            else if (plant == "T04")
                            {
                                if (QueryData1.Rows[i]["HEAD"].ToString() != "BACKEND")
                                {
                                    DataRow dataRow = DATACHART.NewRow();
                                    dataRow["HEAD"] = QueryData1.Rows[i]["HEAD"].ToString();
                                    dataRow["TOTAL"] = QueryData1.Rows[i]["TOTAL"].ToString();
                                    dataRow["TOTAL_PROD"] = QueryData1.Rows[i]["TOTAL_PROD"].ToString();
                                    dataRow["EFFICIENCY"] = QueryData1.Rows[i]["EFFICIENCY"].ToString();
                                    DATACHART.Rows.Add(dataRow);
                                }
                            }
                        }
                        foreach (DataRow dr in DATACHART.AsEnumerable())
                        {
                            CM.Add(new gridtabl
                            {
                                HEAD = dr["HEAD"].ToString(),
                                TOTAL_PROD = dr["TOTAL_PROD"].ToString(),
                                Quantity = dr["TOTAL"].ToString(),
                                EFFICIENCY = Convert.ToDouble(dr["EFFICIENCY"])
                            });
                        }
                    }
                    else
                    {

                    }
                    return CM;
                }
                catch (Exception ex)
                {

                    fun.LogWrite(ex);
                }
                finally
                {

                    fun.ConClose();
                    fun.Connection().Dispose();
                }
            }
            

            return CM;

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

        private List<string> getLastshifts(string Shiftcode, DateTime curShiftstart, DateTime curShiftend)
        {
            var retList = new List<string>();
            try
            {
                if (Shiftcode.Trim().ToUpper() == "C")
                {
                    retList.Add("A");
                    retList.Add("B");
                }
                else if (Shiftcode.Trim().ToUpper() == "B")
                {
                    retList.Add("A");
                }
                return retList;
            }
            catch
            {
                return retList;
            }
        }

        private void getMsg()
        {
            var result = new GridData();
            try
            {
                query = "select PARAMVALUE from XXES_SFT_SETTINGS where PARAMETERINFO='LCD_MSG' and STATUS='Y'";
                string msg = get_Col_Value(query);
                if (!string.IsNullOrEmpty(msg))
                {
                    result.relblMsg = msg;
                    result.lblMsgVisible = true;
                }
                else
                {
                    //result.lblMsg = "WELCOME";
                    //Random rnd = new Random();
                    //result.lblMsg.ForeColor = Color.FromArgb((rnd.Next(0, 255)), (rnd.Next(0, 255)), (rnd.Next(0, 255)));
                    //result.lblMsgVisible = true;
                }
            }
            catch { throw; }
            finally { }
        }


        protected void Timer1_Tick(object sender, EventArgs e)
        {
            var result = new GridData();
            try
            {

                fun.ServerDate = fun.GetServerDateTime();
                //getPlanVertical();
                result.lblTime = "Time: " + fun.ServerDate.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                //result.lblError.Visible = true;
                result.lblError = ex.Message.ToString();
                Timer1_Tick(new object(), new EventArgs());
            }
            finally { }
        }

        public JsonResult DataExport(string PLANTCODE, string SelectedDate)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string UserIpAdd = string.Empty; string errorNo = string.Empty;
            DataTable dt = new DataTable();
            
            try
            {
                if (string.IsNullOrEmpty(PLANTCODE))
                {
                    msg = Validation.str30;
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
                else if (string.IsNullOrEmpty(SelectedDate))
                {
                    msg = "Date is required..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                string plant = "", family = "", DisplayMethod = "";
                plant = PLANTCODE;
                if (plant == "4")
                {
                    plant = "T04";
                    family = Convert.ToString(ConfigurationManager.AppSettings["FT_FAMILY"]);

                }
                if (PLANTCODE == "5")
                {
                    plant = "T05";
                    family = Convert.ToString(ConfigurationManager.AppSettings["PT_FAMILY"]);
                }

                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                string orgid = fun.getOrgId(Convert.ToString(plant).Trim().ToUpper(), Convert.ToString(family).Trim().ToUpper());

                using (OracleCommand oc = new OracleCommand("USP_SHIFTREPORTS", fun.Connection()))
                {
                    fun.ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                    oc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                    oc.Parameters.Add("PORGID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = orgid;
                    oc.Parameters.Add("pFROMDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = SelectedDate;
                    oc.Parameters.Add("pTODATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = SelectedDate;
                    oc.Parameters.Add("pASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("pBSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("pCSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("PISLESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("PFILTERBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "FINAL_LABEL_DATE";
                    oc.Parameters.Add("pREPORTTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "TOT";
                    oc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    dt = new DataTable();
                    oda.Fill(dt);
                }
                if (dt.Rows.Count > 0)
                {
                    dt.Namespace = "Production";
                    dt.TableName = "Production";
                    string filename = "Production" + DateTime.Now.ToString("ddMMyyyy");
                    DataColumnCollection columns = dt.Columns;
                    //data.ImportExcel = filename;
                    if (columns.Contains("PLAN_ID"))
                        dt.Columns.Remove("PLAN_ID");
                    if (columns.Contains("OIL QTY"))
                        dt.Columns.Remove("OIL QTY");
                    if (columns.Contains("BUCKLE UP DATE"))
                        dt.Columns.Remove("BUCKLE UP DATE");
                    if (columns.Contains("SKID"))
                        dt.Columns.Remove("SKID");
                    if (columns.Contains("RTRIM1"))
                        dt.Columns.Remove("RTRIM1");
                    if (columns.Contains("RTRIM2"))
                        dt.Columns.Remove("RTRIM2");
                    if (columns.Contains("RTTYRE1"))
                        dt.Columns.Remove("RTTYRE1");
                    if (columns.Contains("RTTYRE2"))
                        dt.Columns.Remove("RTTYRE2");
                    if (columns.Contains("FTRIM1"))
                        dt.Columns.Remove("FTRIM1");
                    if (columns.Contains("FTRIM2"))
                        dt.Columns.Remove("FTRIM2");
                    if (columns.Contains("FTTYRE1"))
                        dt.Columns.Remove("FTTYRE1");
                    if (columns.Contains("FTTYRE2"))
                        dt.Columns.Remove("FTTYRE2");
                    if (columns.Contains("HYD_PUMP_SRLNO"))
                        dt.Columns.Remove("HYD_PUMP_SRLNO");
                    if (columns.Contains("STEERING_MOTOR_SRLNO"))
                        dt.Columns.Remove("STEERING_MOTOR_SRLNO");
                    if (columns.Contains("STEERING_ASSEMBLY_SRLNO"))
                        dt.Columns.Remove("STEERING_ASSEMBLY_SRLNO");
                    if (columns.Contains("STERING_CYLINDER_SRLNO"))
                        dt.Columns.Remove("STERING_CYLINDER_SRLNO");
                    if (columns.Contains("CLUSSTER_SRLNO"))
                        dt.Columns.Remove("CLUSSTER_SRLNO");
                    if (columns.Contains("ROPS_SRNO"))
                        dt.Columns.Remove("ROPS_SRNO");

                    if (columns.Contains("CAREBUTTONOIL"))
                    {
                        dt.Columns.Remove("CAREBUTTONOIL");
                    }

                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add(dt);

                    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                    ws.Range("A1:X1").Style.Font.Bold = true;
                    //ws.Range("C1:D1").Style.Font.Bold = true;
                    ws.Columns().AdjustToContents();

                    string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }

                    wb.SaveAs(FilePath);
                    msg = "File Exported Successfully ...";
                    mstType = Validation.str;
                    //excelName = data.ImportExcel;
                    var resul = new { Msg = msg, ID = mstType, ExcelName = filename };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "No Record Found..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //if (dt.Rows.Count > 0)
                //{
                //    dt.Namespace = "Production";
                //    dt.TableName = "Production";
                //    string filename = "Production" + DateTime.Now.ToString("ddMMyyyy");

                //    //data.ImportExcel = filename;
                //    dt.Columns.Remove("PLAN_ID");
                //    dt.Columns.Remove("OIL QTY");
                //    dt.Columns.Remove("BUCKLE UP DATE");
                //    dt.Columns.Remove("SKID");
                //    dt.Columns.Remove("RTRIM1");
                //    dt.Columns.Remove("RTRIM2");
                //    dt.Columns.Remove("RTTYRE1");
                //    dt.Columns.Remove("RTTYRE2");
                //    dt.Columns.Remove("FTRIM1");
                //    dt.Columns.Remove("FTRIM2");
                //    dt.Columns.Remove("FTTYRE1");
                //    dt.Columns.Remove("FTTYRE2");
                //    dt.Columns.Remove("HYD_PUMP_SRLNO");
                //    dt.Columns.Remove("STEERING_MOTOR_SRLNO");
                //    dt.Columns.Remove("STEERING_ASSEMBLY_SRLNO");
                //    dt.Columns.Remove("STERING_CYLINDER_SRLNO");
                //    dt.Columns.Remove("CLUSSTER_SRLNO");
                //    dt.Columns.Remove("ROPS_SRNO");
                //    dt.Columns.Remove("CAREBUTTONOIL");
                //    var wb = new XLWorkbook();
                //    var ws = wb.Worksheets.Add(dt);

                //    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                //    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                //    ws.Range("A1:X1").Style.Font.Bold = true;
                //    //ws.Range("C1:D1").Style.Font.Bold = true;
                //    ws.Columns().AdjustToContents();

                //    string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                //    if (System.IO.File.Exists(FilePath))
                //    {
                //        System.IO.File.Delete(FilePath);
                //    }

                //    wb.SaveAs(FilePath);
                //    msg = "File Exported Successfully ...";
                //    mstType = Validation.str;
                //    //excelName = data.ImportExcel;
                //    var resul = new { Msg = msg, ID = mstType, ExcelName = filename };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    msg = "No Record Found..!!!";
                //    mstType = "alert-danger";
                //    errorNo = "1";
                //    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}


            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = "alert-danger";
                errorNo = "1";
                var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally
            {

                fun.ConClose();
                fun.Connection().Dispose();
            }
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        public class gridtabl
        {
            public string SRNO { get; set; }
            public string HEAD { get; set; }
            public string TOTAL_PROD { get; set; }
            public string Quantity { get; set; }
            public double EFFICIENCY { get; set; }

        }
        
        public class ChartModel
        {
            public string HEAD { get; set; }
            public string TOTAL_PROD { get; set; }
            public string Quantity { get; set; }
            public string EFFICIENCY { get; set; }
        }

        public class GridData
        {

            public string lblError { get; set; }
            public string lblDcode { get; set; }
            public string relblMsg { get; set; }
            public string lblMsg { get; set; }
            public bool lblBKTotDayVisible { get; set; }
            public bool lblPendingVisible { get; set; }
            public bool tdMsgVisible { get; set; }
            public bool lblBKVisible { get; set; }
            public bool lblPlanQtyVisible { get; set; }
            public bool lblErrorVisible { get; set; }
            public bool lblMsgVisible { get; set; }
            public string lblShift { get; set; }
            public string lblProductionUnit { get; set; }
            public string lblBK { get; set; }
            public string lblPending { get; set; }
            public string lblBKTotDay { get; set; }
            public string lblTime { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }
            public bool tdError { get; set; }
            public string lblErrordb { get; set; }
            public string ImageUrl { get; set; }
            public string BackColor { get; set; }
            public bool ImageStatus { get; set; }
            public virtual ICollection<gridtabl> gridleft { get; set; }

        }
    }
}