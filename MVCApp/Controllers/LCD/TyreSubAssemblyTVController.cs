using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;


namespace MVCApp.Controllers.LCD
{
    public class TyreSubAssemblyTVController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

        // GET: Injectors
        public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
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
        public JsonResult getPlanVertical(string PLANTCODE)
        {
            var result = new BuckleUpPT();
            var pTtabls = new List<BuckleUpPTtabl>();
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "";
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
                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = ""; string ShiftTlt = "";
                DateTime ShiftStart, shiftEnd;
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
                        Plandate = fun.ServerDate.Date.AddDays(-1);
                    else
                        Plandate = fun.ServerDate.Date;
                    var retList = new List<string>();
                    retList = getLastshifts(Shiftcode, ShiftStart, shiftEnd);

                    query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS s WHERE
                                 S.ENTRYDATE>=to_date('{0}','dd-Mon-yyyy hh24:mi:ss')
                                AND S.ENTRYDATE<=to_date('{1}','dd-Mon-yyyy hh24:mi:ss')
                                 and plant_code='{2}' and family_code='{3}'", ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"), plant, family);

                    //query = string.Format(@"SELECT COUNT(*) FROM XXES_JOB_STATUS s WHERE to_char(S.ENTRYDATE,'dd-Mon-yyyy hh24:mi:ss')>='{0}'
                    //AND to_char(S.ENTRYDATE,'dd-Mon-yyyy hh24:mi:ss')<='{1}' and plant_code='{2}' and family_code='{3}'",
                    //ShiftStart.ToString("dd-MMM-yyyy HH:mm:ss"), shiftEnd.ToString("dd-MMM-yyyy HH:mm:ss"), plant, family);

                    ShiftTlt = get_Col_Value(query).Trim();

                    query = "select to_char(EXPECTED_qty) from XXES_DAILY_PLAN_MASTER  where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and XXES_DAILY_PLAN_MASTER.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family + "'";
                    expqty = get_Col_Value(query).Trim();
                    result.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    result.lblShift = "SHIFT: " + Shiftcode;
                    result.lblProductionUnit = "PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    query = "";
                    foreach (string shift in retList)
                    {
                        query = String.Format(@"SELECT FCODE || '(' || '{0}' || ')' AS FCODE, SUBSTR(ITEM_DESCRIPTION, 0, 30) AS ITEM_DESCRIPTION, SHORT_CODE, from_id, to_id, ABS((from_id - to_id)) + 1 QTY,(SELECT (RTRIM(XMLAGG (XMLELEMENT(E, LTRIM(RTRIM(SUBSTR(A.HOOK_NO, 3, 2))) || ',') ORDER BY srlno).EXTRACT('//text()'), ',')) AS HOOK FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, C.FCODE_ID, C.ITEM_CODE, C.HOOK_NO FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND M.PLANT_CODE = '{1}' AND M.FAMILY_CODE = '{2}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.FINAL_LABEL_DATE IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) A WHERE TO_NUMBER(srlno, '999') >= TO_NUMBER(from_id, '999') AND TO_NUMBER(srlno, '999') <= TO_NUMBER(to_id, '999')) AS HOOK, FENDER, FENDER_RAILING, RADIATOR, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE FROM (SELECT MIN(srlno) FROM_ID, MAX(srlno) TO_ID, FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, FENDER, FENDER_RAILING FROM (SELECT srlno, srlno - ROW_NUMBER() OVER (PARTITION BY ITEM_CODE ORDER BY srlno) GRP, FCODE_ID, ITEM_CODE AS FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, REARTYRE, FRONT_RIM, REAR_RIM, TYRE_MAKE, RADIATOR, HOOK_NO, FENDER, FENDER_RAILING FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, C.FCODE_ID, M.ITEM_CODE, ITEM_DESCRIPTION, M.SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, C.HOOK_NO, M.FENDER, M.FENDER_RAILING FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND C.PLANT_CODE = '{1}' AND C.FAMILY_CODE = '{2}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.FINAL_LABEL_DATE IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) ORDER BY srlno) GROUP BY FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, FENDER, FENDER_RAILING, grp ORDER BY from_id) UNION ALL ", shift.Trim().ToUpper(), plant.Trim().ToUpper(), family.Trim().ToUpper());
                    }
                    query += String.Format(@"SELECT FCODE, SUBSTR(ITEM_DESCRIPTION, 0, 30) AS ITEM_DESCRIPTION, SHORT_CODE, from_id, to_id, ABS((from_id - to_id)) + 1 QTY,(SELECT (RTRIM(XMLAGG (XMLELEMENT(E, LTRIM(RTRIM(SUBSTR(A.HOOK_NO, 3, 2))) || ',') ORDER BY srlno).EXTRACT('//text()'), ',')) AS HOOK FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, C.FCODE_ID, C.ITEM_CODE, C.HOOK_NO FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND M.PLANT_CODE = '{0}' AND M.FAMILY_CODE = '{1}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.FINAL_LABEL_DATE IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) A WHERE TO_NUMBER(srlno, '999') >= TO_NUMBER(from_id, '999') AND TO_NUMBER(srlno, '999') <= TO_NUMBER(to_id, '999')) AS HOOK, FENDER, FENDER_RAILING, RADIATOR, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE FROM (SELECT MIN(srlno) FROM_ID, MAX(srlno) TO_ID, FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, FENDER, FENDER_RAILING FROM (SELECT srlno, srlno - ROW_NUMBER() OVER (PARTITION BY ITEM_CODE ORDER BY srlno) GRP, FCODE_ID, ITEM_CODE AS FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, REARTYRE, FRONT_RIM, REAR_RIM, TYRE_MAKE, RADIATOR, HOOK_NO, FENDER, FENDER_RAILING FROM (SELECT ROW_NUMBER() OVER (ORDER BY C.ENTRY_DATE) AS SRLNO, C.FCODE_ID, M.ITEM_CODE, ITEM_DESCRIPTION, M.SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, C.HOOK_NO, M.FENDER, M.FENDER_RAILING FROM XXES_CONTROLLERS_DATA C, XXES_ITEM_MASTER M WHERE C.STAGE = 'BP' AND C.ITEM_CODE = M.ITEM_CODE AND C.FCODE_ID <> '0' AND C.REMARKS1 IS NULL AND C.PLANT_CODE = M.PLANT_CODE AND C.FAMILY_CODE = M.FAMILY_CODE AND C.PLANT_CODE = '{0}' AND C.FAMILY_CODE = '{1}' AND C.JOBID IN (SELECT JOBID FROM XXES_JOB_STATUS J WHERE J.FINAL_LABEL_DATE IS NULL AND J.FCODE_ID = C.FCODE_ID AND J.PLANT_CODE = C.PLANT_CODE AND J.FAMILY_CODE = C.FAMILY_CODE) ORDER BY C.ENTRY_DATE) ORDER BY srlno) GROUP BY FCODE, ITEM_DESCRIPTION, SHORT_CODE, FRONTTYRE, REAR_HOOD, CLUSTER_METER, IP_HARNESS, RADIATOR_SHELL, AIR_CLEANER, HEAD_LAMP_LH, HEAD_LAMP_RH, FRONT_RIM, REAR_RIM, TYRE_MAKE, REARTYRE, RADIATOR, FENDER, FENDER_RAILING, grp ORDER BY from_id)", plant.Trim().ToUpper(), family.Trim().ToUpper());


                    try
                    {
                        dt = returnDataTable(query);
                        List<BuckleUpPT> buckleUpPTs = new List<BuckleUpPT>();
                        if (dt.Rows.Count > 0)
                        {
                            //if (Shiftcode == "B")
                            //{
                            //    DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                            //    foreach (DataRow row in matches)
                            //        dt.Rows.Remove(row);
                            //}
                            //else if (Shiftcode == "C")
                            //{
                            //    DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                            //    foreach (DataRow row in matches)
                            //        dt.Rows.Remove(row);
                            //    DataRow[] matches1 = dt.Select("PENDING='0' and SHIFT='B'");
                            //    foreach (DataRow row1 in matches1)
                            //        dt.Rows.Remove(row1);
                            //}

                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {

                                var pTtablsdata = new BuckleUpPTtabl();
                                pTtablsdata.SRNO = Convert.ToString(srno);
                                pTtablsdata.FCODE = dr["FCODE"].ToString();
                                pTtablsdata.SHORTNAME = dr["SHORT_CODE"].ToString();
                                pTtablsdata.QTY = dr["QTY"].ToString();
                                pTtablsdata.HOOK = dr["HOOK"].ToString();
                                pTtablsdata.FRONTTYRE = dr["FRONTTYRE"].ToString();
                                pTtablsdata.FRONT_RIM = dr["FRONT_RIM"].ToString();
                                pTtablsdata.REARTYRE = dr["REARTYRE"].ToString();
                                pTtablsdata.REAR_RIM = dr["REAR_RIM"].ToString();
                                pTtablsdata.TYRE_MAKE = dr["TYRE_MAKE"].ToString();
                                //pTtablsdata.PLANNED = dr["PLANNED"].ToString();
                                //pTtablsdata.DONE = dr["DONE"].ToString();
                                //pTtablsdata.PENDING = dr["PENDING"].ToString();

                                pTtabls.Add(pTtablsdata);
                                srno++;
                            }



                            //result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLANNED)", ""));
                            //result.lblBK = "DONE: " + Convert.ToString(dt.Compute("Sum(DONE)", ""));
                            //result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            expqty = get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            result.lblBKTotDay = GetDayTotalHookedDown(plant.Trim());
                            result.lblSftotDay = "SHIFT TOTAL: " + (ShiftTlt.Trim() == "" ? "0" : ShiftTlt.Trim());



                        }
                        else
                        {
                            result.lblBKTotDayVisible = false;
                            //gridVertical.DataSource = null;
                            buckleUpPTs = null;
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

                }
                getMsg();
            }
            catch (Exception ex)
            {
                //result.Visible = true;

                result.lblError = ex.Message.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            finally { }
            result.lblTime = "TIME: " + fun.ServerDate.ToString("HH:mm:ss");
            result.PTtabls = pTtabls;
            //result.PtTableRight = PtTableRight;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public string GetDayTotalHookedDown(string plant)
        {
            DataTable dtshift = new DataTable();
            string ShiftStart = string.Empty;
            string ShiftEnd = string.Empty;
            dtshift = fun.returnDataTable("select * from XXES_SHIFT_MASTER order by SHIFTCODE");
            if (dtshift.Rows.Count > 0)
            {
                foreach (DataRow dr in dtshift.Rows)
                {
                    DateTime abc = fun.GetServerDateTime();

                    if (Convert.ToString(dr["SHIFTCODE"]) == "A")
                    {
                        ShiftStart = Convert.ToDateTime(dr["START_TIME"]).ToString("dd-MMM-yyyy HH:mm:ss");
                    }
                    else if (Convert.ToString(dr["SHIFTCODE"]) == "C")
                    {
                        ShiftEnd = Convert.ToDateTime(dr["END_TIME"]).AddDays(1).ToString("dd-MMM-yyyy HH:mm:ss");
                    }
                }
            }
            query = string.Format(@"select count(*) from XXES_CONTROLLERS_DATA where stage='AP' AND HOOK_NO <> 9999 AND PLANT_CODE = '{0}' AND ENTRY_DATE BETWEEN 
                                          to_date('{1}', 'DD-MON-YYYY HH24:MI:SS') AND to_date('{2}', 'DD-MON-YYYY HH24:MI:SS')", plant.Trim(), ShiftStart, ShiftEnd);
            string expqty = fun.get_Col_Value(query);
            return "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
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
            var result = new BuckleUpPT();
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
            var result = new BuckleUpPT();
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

        public class BuckleUpPTtabl
        {
            public string SRNO { get; set; }
            public string FCODE { get; set; }
            public string FRONTTYRE { get; set; }
            public string FRONT_RIM { get; set; }
            public string REARTYRE { get; set; }
            public string REAR_RIM { get; set; }
            public string TYRE_MAKE { get; set; }
            public string STEERING_CYLINDER { get; set; }
            public string STEERING_COLUMN { get; set; }
            public string STEERING_BASE { get; set; }
            public string DONE { get; set; }
            public string SHORTNAME { get; set; }
            public string QTY { get; set; }
            public string HOOK { get; set; }
            public string PLANNED { get; set; }
            public string PENDING { get; set; }
            public string JOB { get; set; }
        }


        public class BuckleUpPT
        {
            public string SRNO { get; set; }
            public string PLAN_ID { get; set; }
            public string AUTOID { get; set; }
            public string MODEL { get; set; }
            public string DESCRIPTION { get; set; }
            public string SHIFT { get; set; }
            public string SHORTCODE { get; set; }
            public string BACKEND { get; set; }
            public string ENGINE { get; set; }
            public string PLAN { get; set; }
            public string BUCKLEUP { get; set; }
            public string PENDING { get; set; }
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
            public string lblSftotDay { get; set; }
            public string lblTime { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }

            //TYRESUBASSEMBLYTV
            public string FCODE { get; set; }
            public string SHORTNAME { get; set; }
            public string FRONTTYRE { get; set; }
            public string FRONT_RIM { get; set; }
            public string REARTYRE { get; set; }
            public string REAR_RIM { get; set; }
            public string PLANNED { get; set; }
            public virtual ICollection<BuckleUpPTtabl> PTtabls { get; set; }
            //public virtual ICollection<BuckleUpPtTableRight> PtTableRight { get; set; }

        }


    }

}