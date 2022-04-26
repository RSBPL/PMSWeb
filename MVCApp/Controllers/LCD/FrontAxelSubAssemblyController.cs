using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;


namespace MVCApp.Controllers
{
    public class FrontAxelSubAssemblyController : Controller
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
            string ashift = string.Empty, bshift = string.Empty, cshift = string.Empty;

            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string plant = "", family = "", DisplayMethod = "";

                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
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

                    ShiftTlt = get_Col_Value(query).Trim();
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
                                query += string.Format(@"select '' as SRNO,Item_code || '(' ||  '{0}' || ')'  as F_CODE,(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{1}' and family_code = '{2}') as SHORTCODE,

                                        (Select FRONTAXEL from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{1}' and family_code = '{2}' ) as FRONTAXEL,
                                    
                                        (Select FRONT_SUPPORT from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{1}' and family_code = '{2}' ) as FRONT_SUPPORT,
                                    
                                        (Select CENTER_AXEL from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{1}' AND family_code = '{2}' ) as CENTER_AXEL,
                                    
                                        (SELECT SLIDER FROM   xxes_item_master WHERE  item_code = p.item_code AND    plant_code = '{1}' AND    family_code = '{2}' ) AS SLIDER,
            
                                        (SELECT SLIDER_RH FROM   xxes_item_master WHERE  item_code = p.item_code AND    plant_code = '{1}' AND    family_code = '{2}' ) AS SLIDER_RH,
                                    
                                        (Select STEERING_COLUMN from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{1}' and family_code = '{2}' ) as STEERING_COLUMN,
                                    
                                        m.PLAN_ID,p.AUTOID, SUBSTR(p.ITEM_DESC, 0, 30) as DESCRIPTION,m.shiftcode as SHIFT, qty PLAN,
                                    
                                        (select count(*) from XXES_JOB_STATUS  where ITEM_CODE = p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID = p.AUTOID and plant_code = '{1}' and family_code = '{2}' ) as DONE,
                                    
                                        p.qty - (select count(*) from XXES_JOB_STATUS  where ITEM_CODE = p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID = p.AUTOID and plant_code = '{1}' and family_code = '{2}'  ) as PENDING
                                    
                                        from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m where p.PLAN_ID = m.PLAN_ID and m.plant_code = '{1}' and m.family_code = '{2}'
                                    
                                        and to_char(PLAN_DATE,'dd-Mon-yyyy')= '{3}' and m.SHIFTCODE = '{0}'   GROUP BY P.AUTOID, m.PLAN_ID,p.AUTOID, Item_code,p.ITEM_DESC,m.shiftcode,P.QTY


                                        having P.QTY > (select count(*) from xxes_job_status where fcode_id = p.autoid and fcode_srlno is not null)  Union ", shift.Trim().ToUpper(), plant.Trim().ToUpper(), family.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"));

                            }

                                 query += string.Format(@"select '' as SRNO,Item_code AS F_CODE,(Select SHORT_CODE from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{0}' and family_code = '{1}') as SHORTCODE,
                                    
                                        (Select FRONTAXEL from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{0}' and family_code = '{1}' ) as FRONTAXEL,
                                    
                                        (Select FRONT_SUPPORT from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{0}' and family_code = '{1}' ) as FRONT_SUPPORT,
                                    
                                        (Select CENTER_AXEL from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{0}' AND family_code = '{1}' ) as CENTER_AXEL,
                                    
                                        (SELECT SLIDER FROM   xxes_item_master WHERE  item_code = p.item_code AND    plant_code = '{0}' AND    family_code = '{1}' ) AS SLIDER,
            
                                        (SELECT SLIDER_RH FROM   xxes_item_master WHERE  item_code = p.item_code AND    plant_code = '{0}' AND    family_code = '{1}' ) AS SLIDER_RH,
                                    
                                        (Select STEERING_COLUMN from XXES_ITEM_MASTER where ITEM_CODE = p.ITEM_CODE and plant_code = '{0}' and family_code = '{1}' ) as STEERING_COLUMN,
                                    
                                        m.PLAN_ID,p.AUTOID, SUBSTR(p.ITEM_DESC, 0, 30) as DESCRIPTION,m.shiftcode as SHIFT, qty PLAN,
                                    
                                        (select count(*) from XXES_JOB_STATUS  where ITEM_CODE = p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID = p.AUTOID and plant_code = '{0}' and family_code = '{1}' ) as DONE,
                                    
                                        p.qty - (select count(*) from XXES_JOB_STATUS  where ITEM_CODE = p.ITEM_CODE and XXES_JOB_STATUS.FCODE_ID = p.AUTOID and plant_code = '{0}' and family_code = '{1}'  ) as PENDING
                                    
                                        from XXES_DAILY_PLAN_TRAN p , XXES_DAILY_PLAN_MASTER m where p.PLAN_ID = m.PLAN_ID and m.plant_code = '{0}' and m.family_code = '{1}'
                                    
                                        and to_char(PLAN_DATE,'dd-Mon-yyyy')= '{2}' and m.SHIFTCODE = 'A' GROUP BY P.AUTOID, m.PLAN_ID,p.AUTOID, Item_code,p.ITEM_DESC,m.shiftcode,P.QTY
                                   
                                        having P.QTY > (select count(*) from xxes_job_status where fcode_id = p.autoid and fcode_srlno is not null) order by AUTOID", plant.Trim().ToUpper(), family.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"));

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
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "FASA";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                                sc.Parameters.Add("PPLAN_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Plandate.ToString("dd-MMM-yyyy");
                                sc.Parameters.Add("ASHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = ashift;
                                sc.Parameters.Add("BSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = bshift;
                                sc.Parameters.Add("CSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = cshift;
                                sc.Parameters.Add("PPLANTID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PSTARTSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PENDSFHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                                sc.Parameters.Add("PCURSHIFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
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

                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var pTtablsdata = new BuckleUpPTtabl();
                                pTtablsdata.SRNO = Convert.ToString(srno);
                                pTtablsdata.F_CODE = dr["F_CODE"].ToString();
                                pTtablsdata.SHORTCODE = dr["SHORTCODE"].ToString();
                                //pTtablsdata.FRONTAXEL = dr["FRONTAXEL"].ToString();
                                pTtablsdata.FRONT_SUPPORT = dr["FRONT_SUPPORT"].ToString();
                                pTtablsdata.CENTER_AXEL = dr["CENTER_AXEL"].ToString();
                                pTtablsdata.SLIDER = dr["SLIDER"].ToString();
                                pTtablsdata.SLIDER_RH = dr["SLIDER_RH"].ToString();
                                pTtablsdata.STEERING_COLUMN = dr["STEERING_COLUMN"].ToString();
                                pTtablsdata.PLAN = dr["PLAN"].ToString();
                                pTtablsdata.DONE = dr["DONE"].ToString();
                                pTtablsdata.PENDING = dr["PENDING"].ToString();

                                pTtabls.Add(pTtablsdata);
                                srno++;
                            }
                            


                            result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            result.lblBK = "DONE: " + Convert.ToString(dt.Compute("Sum(DONE)", ""));
                            result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            expqty = get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
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
            result.lblTime = "Time: " + fun.ServerDate.ToString("HH:mm:ss");
            result.PTtabls = pTtabls;
            //result.PtTableRight = PtTableRight;
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
            public string F_CODE { get; set; }
            public string FRONTAXEL { get; set; }
            public string FRONT_SUPPORT { get; set; }
            public string CENTER_AXEL { get; set; }
            public string SLIDER { get; set; }
            public string SLIDER_RH { get; set; }
            public string STEERING_COLUMN { get; set; }
            public string DONE { get; set; }
            public string SHORTCODE { get; set; }
            public string PLAN { get; set; }
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
            public string ShiftTlt { get; set; }
            public string lblPlanQty { get; set; }

            //FRONTAXELSUBASSEMBLY
            public string F_CODE { get; set; }
            public string FRONTAXEL { get; set; }
            public string FRONT_SUPPORT { get; set; }
            public string CENTER_AXEL { get; set; }
            public string SPINDLE { get; set; }
            public string STEERING_CYLINDER { get; set; }
            public string STEERING_COLUMN { get; set; }
            public string STEERING_BASE { get; set; }
            public virtual ICollection<BuckleUpPTtabl> PTtabls { get; set; }
            //public virtual ICollection<BuckleUpPtTableRight> PtTableRight { get; set; }

        }
        
     
    }
    
}