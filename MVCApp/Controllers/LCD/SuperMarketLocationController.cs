using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;

namespace MVCApp.Controllers.LCD
{
    public class SuperMarketLocationController : Controller
    {
        // GET: SuperMarketLocation
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = string.Empty;

        public ActionResult Index(string id)
        {
            ViewBag.PlantCode = id;
            return View();
        }
        [HttpGet]
        public JsonResult getPlanVertical(string PLANTCODE)
        {
            var result = new TblColomn();
            var Grid = new List<TblColomnData>();
            string plant = "", family = "", DisplayMethod = "";
            DisplayMethod = Convert.ToString(ConfigurationManager.AppSettings["DisplayMethod"]);
            plant = PLANTCODE;
            if (plant == "4")
            {
                plant = "T04";
                family = "TRACTOR FTD";

            }
            if (PLANTCODE == "5")
            {
                plant = "T05";
                family = "TRACTOR TD";
            }
            if (PLANTCODE == "2")
            {
                plant = "T02";
                family = "TRACTOR EKI";
            }
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
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
                    query = "";

                    DataTable dt = new DataTable();
                    if (DisplayMethod.ToUpper() == "INLINE")
                    {
                        query = String.Format(@"SELECT P.*,( SELECT SUM(B.QUANTITY) FROM XXES_BULKSTOCK b WHERE B.LOCATION_CODE=P.LOCATION AND b.PLANT_CODE=P.PLANT_CODE AND b.FAMILY_CODE=P.FAMILY_CODE) BULK_QTY FROM ( SELECT S.PLANT_CODE, S.FAMILY_CODE, S.SUMKTLOC, S.ITEM_CODE, R.ITEM_DESCRIPTION, S.SAFTY_STOCK_QUANTITY, (NVL((SELECT SUM(XB.QUANTITY) FROM XXES_SUMMKTSTOCK XB WHERE XB.PLANT_CODE = S.PLANT_CODE AND XB.FAMILY_CODE = S.FAMILY_CODE AND XB.LOCATION_CODE = S.SUMKTLOC AND XB.PLANT_CODE = '{0}' AND XB.FAMILY_CODE = '{1}' AND XB.ITEMCODE = S.ITEM_CODE), 0)) EXISTING_QTY, (FN_GETSTORGE_LOCATION('BULK', S.PLANT_CODE, S.FAMILY_CODE, S.ITEM_CODE)) LOCATION FROM XXES_KANBAN_MASTER S JOIN XXES_RAWMATERIAL_MASTER R ON S.PLANT_CODE = R.PLANT_CODE AND S.FAMILY_CODE = R.FAMILY_CODE AND S.ITEM_CODE = R.ITEM_CODE GROUP BY s.PLANT_CODE,s.FAMILY_CODE,s.SUMKTLOC,S.ITEM_CODE,R.ITEM_DESCRIPTION,S.SAFTY_STOCK_QUANTITY )P WHERE P.SAFTY_STOCK_QUANTITY>P.EXISTING_QTY AND P.LOCATION IS NOT NULL", plant.Trim().ToUpper(), family.Trim().ToUpper());
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
                                sc.Parameters.Add("PROCESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "SML";
                                sc.Parameters.Add("PPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plant.ToUpper().Trim();
                                sc.Parameters.Add("PFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
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
                                var TblMdl = new TblColomnData();
                                TblMdl.SRNO = Convert.ToString(srno);
                                TblMdl.ITEMCODE = dr["ITEM_CODE"].ToString();
                                TblMdl.ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString();
                                TblMdl.SUMKTLOC = dr["SUMKTLOC"].ToString();
                                TblMdl.SAFTY_STOCK = dr["SAFTY_STOCK_QUANTITY"].ToString();
                                TblMdl.EXISTING_QTY = dr["EXISTING_QTY"].ToString();
                                TblMdl.LOCATION = dr["LOCATION"].ToString();
                                TblMdl.BULK_QTY = dr["BULK_QTY"].ToString();

                                Grid.Add(TblMdl);
                                srno++;
                            }



                            //result.lblPlanQty = "PLANNED: " + Convert.ToString(dt.Compute("Sum(PLAN)", ""));
                            //result.lblBK = "BUCKLE-UP: " + Convert.ToString(dt.Compute("Sum(BUCKLEUP)", ""));
                            //result.lblPending = "PENDING: " + Convert.ToString(dt.Compute("Sum(PENDING)", ""));
                            //expqty = fun.get_Col_Value("select count(*) from XXES_JOB_STATUS where FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID in (select plan_id from XXES_DAILY_PLAN_MASTER where plant_code='" + plant.Trim() + "' and family_code='" + family + "' and to_char(PLAN_DATE,'dd-Mon-yyyy')='" + Plandate.ToString("dd-MMM-yyyy") + "'))");
                            //result.lblBKTotDay = "DAY TOTAL: " + (expqty.Trim() == "" ? "0" : expqty.Trim());


                            //result.lblBKTotDay =  true;
                            //result.lblPending.Visible = true;
                            //result.lblBK.Visible = true;
                            //result.lblPlanQty.Visible = true;
                        }
                        else
                        {
                            result.lblBKTotDayVisible = false;
                            //gridVertical.DataSource = null;
                            //buckleUpPTs = null;
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
                //getMsg();
            }
            catch (Exception ex)
            {
                //result.Visible = true;

                result.lblError = ex.Message.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            finally { }
            result.lblTime = "TIME: " + fun.ServerDate.ToString("HH:mm:ss");
            result.DataGrid = Grid;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class TblColomnData
        {
            public string SRNO { get; set; }
            public string SUMKTLOC { get; set; }
            public string ITEMCODE { get; set; }
            public string PLAN_ID { get; set; }
            public string LOCATION { get; set; }
            public string BULK_QTY { get; set; }
            public string ITEM_CODE { get; set; }
            public string ITEM_DESCRIPTION { get; set; }
            public string SAFTY_STOCK { get; set; }
            public string EXISTING_QTY { get; set; }
            public string AVAILABLE_SPACE { get; set; }

        }


        public class TblColomn
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
            public virtual ICollection<TblColomnData> DataGrid { get; set; }

        }
    }
}