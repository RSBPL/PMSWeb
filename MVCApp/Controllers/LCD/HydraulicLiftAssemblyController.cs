using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;

namespace MVCApp.Controllers.LCD
{
    public class HydraulicLiftAssemblyController : Controller
    {
        // GET: HydraulicLiftAssembly
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;
        string PlanIdQuery = string.Empty;
        public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
            return View();
        }
        

        [HttpGet]
        public JsonResult getPlanVertical(string PLANTCODE)
        {
            var result = new BuckleUpPT();
            var HydMdlData = new List<Hydrualicmodel>();
            string ashift = string.Empty, bshift = string.Empty, cshift = string.Empty;
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", PlanId = "", DisplayMethod = "";
                plant = PLANTCODE;
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                if (plant == "4")
                {
                    plant = "T04";
                    family = "HYDRAULIC FTD";

                }
                if (PLANTCODE == "5")
                {
                    plant = "T05";
                    family = "HYDRAULIC TD";
                }
                string Shiftcode = "", NightExists = "", data = "", isDayNeedToLess = "0"; DateTime Plandate = new DateTime(); string expqty = "";
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
                    query = "select to_char(EXPECTED_qty) from XXES_DAILY_PLAN_MASTER  where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and XXES_DAILY_PLAN_MASTER.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family + "'";
                    expqty = fun.get_Col_Value(query).Trim();
                    result.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    result.lblShift = "SHIFT: " + Shiftcode;
                    result.lblProductionUnit = "PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());

                    query = "";
                    DataTable dt = new DataTable();
                    try
                    {
                        if (DisplayMethod.ToUpper() == "INLINE")
                        {
                            foreach (string shift in retList)
                            {
                                query = string.Format(@"SELECT rownum SERIAL,A.*, ( A.PLAN - A.done ) PENDING FROM   (SELECT h.item_code, h.short_code,
                                    h.spool_value, h.cynlinder, h.part1, h.part2, A.qty  PLAN, 
                                    (SELECT Count(*) FROM   xxes_print_serials
                                    WHERE  plant_code = a.plant_code AND family_code = a.family_code AND subassembly_id = a.autoid) DONE
                                    FROM   xxes_daily_plan_assembly a JOIN xxes_hydrualic_master h ON A.plant_code = h.plant_code
                                    join xxes_daily_plan_master m on m.plan_id=a.plan_id
                                    AND A.family_code = h.family_code AND A.itemcode = h.item_code AND A.PLANT_CODE='{0}' 
                                    AND A.FAMILY_CODE='{1}' AND 
                                    to_char(PLAN_DATE,'dd-Mon-yyyy')='{2}' and m.SHIFTCODE='{3}')A union ", plant.Trim().ToUpper(), family.Trim().ToUpper(),
                                        Plandate.ToString("dd-MMM-yyyy"), shift);
                            }

                            query += string.Format(@"SELECT rownum SERIAL,A.*, ( A.PLAN - A.done ) PENDING FROM   (SELECT h.item_code, h.short_code,
                                    h.spool_value, h.cynlinder, h.part1, h.part2, A.qty  PLAN, 
                                    (SELECT Count(*) FROM   xxes_print_serials
                                    WHERE  plant_code = a.plant_code AND family_code = a.family_code AND subassembly_id = a.autoid) DONE
                                    FROM   xxes_daily_plan_assembly a JOIN xxes_hydrualic_master h ON A.plant_code = h.plant_code
                                    join xxes_daily_plan_master m on m.plan_id=a.plan_id
                                    AND A.family_code = h.family_code AND A.itemcode = h.item_code AND A.PLANT_CODE='{0}' 
                                    AND A.FAMILY_CODE='{1}' AND 
                                    to_char(PLAN_DATE,'dd-Mon-yyyy')='{2}' and m.SHIFTCODE='{3}')A", plant.Trim().ToUpper(), family.Trim().ToUpper(),
                                        Plandate.ToString("dd-MMM-yyyy"), Shiftcode);

                            dt = fun.returnDataTable(query);
                        }
                        else
                        {
                            if (Shiftcode == "A")
                                ashift = Shiftcode;
                            foreach (string shift in retList)
                            {
                                if (shift == "A")
                                    ashift = shift;
                                else if (shift == "B")
                                    bshift = shift;
                                else if (shift == "C")
                                    cshift = shift;
                            }
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {

                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "HLA";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Plandate.ToString("dd-MMM-yyyy");
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = ashift;
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = bshift;
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = cshift;
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Shiftcode.Trim().ToUpper();
                                sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                OracleDataAdapter dr = new OracleDataAdapter(sc);
                                dr.Fill(dt);
                                fun.ConClose();
                            }
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

                    try
                    {
                        List<BuckleUpPT> buckleUpPTs = new List<BuckleUpPT>();
                        if (dt.Rows.Count > 0)
                        {
                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var HydrualicTbl = new Hydrualicmodel();
                                HydrualicTbl.SRNO = Convert.ToString(srno);
                                HydrualicTbl.ITEM_CODE = dr["ITEM_CODE"].ToString();
                                HydrualicTbl.SHORT_CODE = dr["SHORT_CODE"].ToString();
                                HydrualicTbl.SPOOL_VALUE = dr["SPOOL_VALUE"].ToString();
                                HydrualicTbl.CYNLINDER = dr["CYNLINDER"].ToString();
                                HydrualicTbl.PART1 = dr["PART1"].ToString();
                                HydrualicTbl.PART2 = dr["PART2"].ToString();
                                HydrualicTbl.PLAN = dr["PLAN"].ToString();
                                HydrualicTbl.DONE = dr["DONE"].ToString();
                                HydrualicTbl.PENDING = dr["PENDING"].ToString();

                                HydMdlData.Add(HydrualicTbl);
                                srno++;
                            }



                            result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            result.lblBK = "DONE: " + Convert.ToString(dt.Compute("Sum(DONE)", ""));
                            result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            expqty = fun.get_Col_Value("select count(*) from xxes_print_serials where subassembly_id in (select autoid from xxes_daily_plan_assembly where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());



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
            result.lblTime = "Time: " + fun.ServerDate.ToString("HH:mm:ss");
            result.HydMdlDataSent = HydMdlData;
            //result.PtTableRight = PtTableRight;
            return Json(result, JsonRequestBehavior.AllowGet);
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
                string msg = fun.get_Col_Value(query);
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

        public class Hydrualicmodel
        {
            public string SRNO { get; set; }
            public string ITEM_CODE { get; set; }
            public string SHORT_CODE { get; set; }
            public string SPOOL_VALUE { get; set; }
            public string CYNLINDER { get; set; }
            public string PART1 { get; set; }
            public string PART2 { get; set; }

            public string DONE { get; set; }
            public string PLAN { get; set; }
            public string PENDING { get; set; }
            public string JOB { get; set; }
        }

        public class BuckleUpPT
        {
            public string SRNO { get; set; }
            public string ITEM_CODE { get; set; }
            public string SHORT_CODE { get; set; }
            public string SPOOL_VALUE { get; set; }
            public string CYNLINDER { get; set; }
            public string PART1 { get; set; }
            public string PART2 { get; set; }

            public string DONE { get; set; }
            public string PLAN { get; set; }
            public string PENDING { get; set; }
            public string JOB { get; set; }
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

            public virtual ICollection<Hydrualicmodel> HydMdlDataSent { get; set; }

        }
    }
}