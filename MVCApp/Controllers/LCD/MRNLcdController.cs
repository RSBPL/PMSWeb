using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;

namespace MVCApp.Controllers.LCD
{
    public class MRNLcdController : Controller
    {
        // GET: MRNLcd
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

        public ActionResult Index()
        {
            //ViewBag.PlantCode = id;
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
        public JsonResult getPlanVertical()
        {
            var result = new GridData();
            var gridleft = new List<gridtabl>();
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string DisplayMethod = "";
                

                DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
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
                    

                    result.lblDate = "DATE: " + Plandate.ToString("dd MMM yyyy");
                    result.lblShift = "SHIFT: " + Shiftcode;
                    result.lblProductionUnit = "SHIFT PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    //+ "SHIFT: " + Shiftcode + "         " + " PRODUCTION PLAN: " + (expqty.Trim() == "" ? "0" : expqty.Trim());
                    query = "";

                    DataTable dt = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        //query = string.Format(@"SELECT M.* FROM
                        //                (SELECT I.PLANT_CODE, I.VEHICLE_NO,
                        //                I.VENDOR_NAME,
                        //                COUNT(*) TOTAL,
                        //                'DONE' STATUS 
                        //                FROM ITEM_RECEIPT_DETIALS I 
                        //                WHERE TRUNC(I.PRINTED_ON) = '{0}'
                        //                AND I.TIMEIN IS NULL 
                        //                AND I.VEHICLE_NO IS NOT NULL 
                        //                AND I.VENDOR_NAME IS NOT NULL
                        //                AND I.PLANT_CODE IS NOT NULL
                        //                GROUP BY I.VEHICLE_NO, 
                        //                I.VENDOR_NAME, I.PLANT_CODE)m
                        //                WHERE M.PLANT_CODE = 'T04' OR M.PLANT_CODE = 'T05'",
                        //                Plandate.ToString("dd-MMM-yyyy"));
                        query = string.Format(@"SELECT SUBSTR(I.VEHICLE_NO,1,11) VEHICLE_NO,
                                        I.VENDOR_NAME,
                                        COUNT(*) TOTAL,
                                        'DONE' STATUS 
                                        FROM ITEM_RECEIPT_DETIALS I 
                                        WHERE TRUNC(I.PRINTED_ON) = '{0}'
                                        AND I.TIMEIN IS NULL 
                                        AND I.VEHICLE_NO IS NOT NULL 
                                        AND I.VENDOR_NAME IS NOT NULL
                                        AND I.PLANT_CODE IS NOT NULL
                                        AND UPPER(I.VEHICLE_NO) NOT IN('B','BILL')
                                        AND I.PLANT_CODE IN('T04','T05')
                                        GROUP BY I.VEHICLE_NO, 
                                        I.VENDOR_NAME
                                        ORDER BY VENDOR_NAME",
                                        Plandate.ToString("dd-MMM-yyyy"));



                        dt = fun.returnDataTable(query);

                    }
                    else
                    {

                        try
                        {
                            using (OracleCommand sc = new OracleCommand("USP_GETLIVEDATA", fun.Connection()))
                            {
                                fun.ConOpen();
                                sc.CommandType = CommandType.StoredProcedure;
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "MSS";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
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
                                //fun.ConClose();
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
                        //return ;

                    }
                    try
                    {
                        if (dt.Rows.Count > 0)
                        {
                            int srno = 1;
                            foreach (DataRow dr in dt.Rows)
                            {
                                var gridleftdata = new gridtabl();
                                gridleftdata.SRNO = Convert.ToString(srno);
                                gridleftdata.VEHICLENUMBER = dr["VEHICLE_NO"].ToString();
                                gridleftdata.VENDOR_NAME = dr["VENDOR_NAME"].ToString();
                                gridleftdata.TOTAL = dr["TOTAL"].ToString();
                                gridleftdata.STATUS = dr["STATUS"].ToString();

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
        public class gridtabl
        {
            public string SRNO { get; set; }
            public string VEHICLENUMBER { get; set; }
            public string VENDOR_NAME { get; set; }
            public string TOTAL { get; set; }
            public string STATUS { get; set; }


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