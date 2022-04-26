using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;


namespace MVCApp.Controllers
{
    public class BuckleUpPTController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

        // GET: Injectors
        public ActionResult Index()
        {
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


        [HttpPost]
        public JsonResult getPlanVertical()
        
        {
            var result = new BuckleUpPT();
            var gridleft = new List<BuckleUpPTtabl>();
            var gridright = new List<BuckleUpPtTableRight>();
            string ashift = string.Empty, bshift = string.Empty, cshift = string.Empty;
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", DisplayMethod = "";
                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
                plant = Convert.ToString(ConfigurationManager.AppSettings["PT_PLANT"]);
                family = Convert.ToString(ConfigurationManager.AppSettings["PT_FAMILY"]);
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
                    expqty = get_Col_Value(query).Trim();
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
                                query += "select '' as SRNO, m.PLAN_ID,p.AUTOID,Item_code || '(' ||  '" + shift + "' || ')'  as MODEL,SUBSTR(p.ITEM_DESC,0,30) as DESCRIPTION,m.shiftcode as SHIFT,  " +
                                "(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as SHORTCODE  ," +
                                "(Select (case when REQUIRE_BACKEND='N' then 'NA' ELSE BACKEND  END) as BACKEND from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as BACKEND  ," +
                                "(Select (case when REQUIRE_ENGINE='N' then 'NA' ELSE ENGINE END) as ENGINE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as ENGINE  ," +
                                " qty PLAN,(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as BUCKLEUP," +
                                " p.qty-(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as PENDING " +
                                " from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m  " +
                                " where p.PLAN_ID=m.PLAN_ID and m.plant_code='" + plant.Trim().ToUpper() + "' and m.family_code='" + family.Trim().ToUpper() + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and m.SHIFTCODE='" + shift.Trim().ToUpper() + "' Union ";

                            }

                            query += "select '' as SRNO, m.PLAN_ID,p.AUTOID, Item_code  as MODEL,SUBSTR(p.ITEM_DESC,0,30) as DESCRIPTION,m.shiftcode as SHIFT," +
                               "(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "' ) as SHORTCODE ,  " +
                             "(Select (case when REQUIRE_BACKEND='N' then 'NA' ELSE BACKEND  END) as BACKEND from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as BACKEND  ," +
                              "(Select (case when REQUIRE_ENGINE='N' then 'NA' ELSE ENGINE END) as ENGINE from XXES_ITEM_MASTER where ITEM_CODE=p.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as ENGINE  ," +
                              "qty PLAN,(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "' ) as BUCKLEUP, " +
                              "p.qty-(select count(*) from XXES_JOB_STATUS  where ITEM_CODE=p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID=p.AUTOID and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "'  ) as PENDING  " +
                              "from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m " +
                              "where p.PLAN_ID=m.PLAN_ID and m.plant_code='" + plant.Trim().ToUpper() + "' and m.family_code='" + family.Trim().ToUpper() + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and m.SHIFTCODE='" + Shiftcode.Trim().ToUpper() + "' order by AUTOID";
                            dt = returnDataTable(query);
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
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BKPT";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Plandate.ToString("dd-MMM-yyyy");
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(ashift) ? null : ashift.ToUpper().Trim();
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(bshift) ? null : bshift.ToUpper().Trim();
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(cshift) ? null : cshift.ToUpper().Trim();
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Shiftcode.Trim().ToUpper();
                                sc.Parameters.Add("PRC", OracleDbType.RefCursor, ParameterDirection.Output);
                                OracleDataAdapter dr = new OracleDataAdapter(sc);
                                dr.Fill(dt);

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
                    }
                    try
                    {
                        List<BuckleUpPT> buckleUpPTs = new List<BuckleUpPT>();
                        if (dt.Rows.Count > 0)
                        {
                            if (Shiftcode == "B")
                            {
                                DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                                foreach (DataRow row in matches)
                                    dt.Rows.Remove(row);
                            }
                            else if (Shiftcode == "C")
                            {
                                DataRow[] matches = dt.Select("PENDING='0' and SHIFT='A'");
                                foreach (DataRow row in matches)
                                    dt.Rows.Remove(row);
                                DataRow[] matches1 = dt.Select("PENDING='0' and SHIFT='B'");
                                foreach (DataRow row1 in matches1)
                                    dt.Rows.Remove(row1);
                            }

                            query = "select * from (select FCODE_ID from XXES_JOB_STATUS where FCODE_ID is not null and plant_code='" + plant + "' and family_code='" + family + "' order by ENTRYDATE DESC) where rownum=1";

                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                
                                var gridleftdata = new BuckleUpPTtabl();
                                gridleftdata.SRNO = Convert.ToString(srno);
                                gridleftdata.PLAN_ID = dr["PLAN_ID"].ToString();
                                gridleftdata.AUTOID = dr["AUTOID"].ToString();
                                gridleftdata.MODEL = dr["MODEL"].ToString();
                                gridleftdata.DESCRIPTION = dr["DESCRIPTION"].ToString();
                                gridleftdata.SHIFT = dr["SHIFT"].ToString();
                                gridleftdata.SHORTCODE = dr["SHORTCODE"].ToString();
                                gridleftdata.BACKEND = dr["BACKEND"].ToString();
                                gridleftdata.ENGINE = dr["ENGINE"].ToString();
                                gridleftdata.PLAN = dr["PLAN"].ToString();
                                gridleftdata.BUCKLEUP = dr["BUCKLEUP"].ToString();
                                gridleftdata.PENDING = dr["PENDING"].ToString();
                                if (get_Col_Value(query).Trim() == dr["AUTOID"].ToString().Trim())
                                {
                                    gridleftdata.BackColor = "Yellow";
                                }
                                gridleft.Add(gridleftdata);
                                srno++;
                            }
                            


                            result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            result.lblBK = "BUCKLE-UP: " + Convert.ToString(dt.Compute("Sum(BUCKLEUP)", ""));
                            result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            expqty = get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());


                            //result.lblBKTotDay =  true;
                            //result.lblPending.Visible = true;
                            //result.lblBK.Visible = true;
                            //result.lblPlanQty.Visible = true;
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
                    try
                    {
                        query = "";

                        query = "SELECT * FROM (Select XT.ITEM_CODE as MODEL,XS.JOBID JOB," +
                            "(Select (case when REQUIRE_BACKEND='N' then 'NA' ELSE XS.BACKEND_SRLNO  END) as BACKEND from XXES_ITEM_MASTER where ITEM_CODE=XS.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as BACKEND  ," +
                            "(Select (case when REQUIRE_ENGINE='N' then 'NA' ELSE XS.ENGINE_SRLNO END) as ENGINE from XXES_ITEM_MASTER where ITEM_CODE=XS.ITEM_CODE and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "') as ENGINE  " +
                            "from XXES_JOB_STATUS XS, XXES_DAILY_PLAN_TRAN XT  where XS.ITEM_CODE=XT.ITEM_CODE and XT.autoid=XS.FCODE_id and XS.plant_code='" + plant.Trim().ToUpper() + "' and XS.family_code='" + family.Trim().ToUpper() + "' and PLAN_ID IN (select PLAN_ID from XXES_DAILY_PLAN_MASTER where to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "' and plant_code='" + plant.Trim().ToUpper() + "' and family_code='" + family.Trim().ToUpper() + "')  order by entrydate desc ) WHERE ROWNUM <= 20";

                        dt = new DataTable();
                        dt = returnDataTable(query);
                        if (dt.Rows.Count > 0)
                        {

                            foreach (DataRow dr in dt.Rows)
                            {
                                var gridrightdata = new BuckleUpPtTableRight();
                               gridrightdata.JOB = dr["JOB"].ToString();
                               gridrightdata.MODEL = dr["MODEL"].ToString();
                               gridrightdata.BACKEND = dr["BACKEND"].ToString();
                                gridrightdata.ENGINE = dr["ENGINE"].ToString();
                                gridright.Add(gridrightdata);
                            }
                            //gridVerticalRight.DataSource = dt;
                            //gridVerticalRight.DataBind();
                            //gridVerticalRight.Visible = true;

                        }
                        else
                        {
                            //gridVerticalRight.Visible = false;
                            //gridVerticalRight.DataSource = null;
                            //gridVerticalRight.DataBind();
                        }
                        //result.lblError.Visible = false;
                    }
                    catch (Exception ex)
                    {
                        result.lblErrorVisible = true;
                        result.lblError = ex.Message.ToString();
                    }
                    try
                    {

                    }
                    catch (Exception)
                    {

                        throw;
                    }
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
            result.gridleft = gridleft;
            result.gridright = gridright;
            return Json(result, JsonRequestBehavior.AllowGet);
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
                getPlanVertical();
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
        public class BuckleUpPtTableRight
        {
            public string MODEL { get; set;} 
            public string BACKEND { get; set;}
            public string ENGINE { get; set;}
            public string JOB { get; set; }
        }
            public class BuckleUpPTtabl
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
            public string BackColor { get; set; }
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
            public string lblTime { get; set; }
            public string lblDate { get; set; }
            public string lblPlanQty { get; set; }
            public bool tdError { get; set; }
            public string lblErrordb { get; set; }
            public string ImageUrl { get; set; }
            public string BackColor { get; set; }
            public bool ImageStatus { get; set; }
            public virtual ICollection<BuckleUpPTtabl> gridleft { get; set; }
            public virtual ICollection<BuckleUpPtTableRight> gridright { get; set; }

        }
        
     
    }
    
}