using System.Web.Mvc;
using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System;
using System.Collections.Generic;


namespace MVCApp.Controllers
{
    public class InjectorsController : Controller
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

        //private void getDailyPlanVertical()
        //{
        //    try
        //    {

        //        System.DateTime Plandate = new System.DateTime(); string expqty = "";
        //        CheckInfo();
        //        lblError.Text = "";

        //        try
        //        {
        //            using (dt = getLastScannedEngines())
        //            //using (dt = returnDataTable("select Item_Code,Engine_Srno,Injector1,Injector2,Injector3,Injector4 from xxes_engine_status order by Entrydate desc"))
        //            {
        //                if (dt.Rows.Count > 0)
        //                {
        //                    gridVertical.DataSource = dt;
        //                    gridVertical.DataBind();
        //                    //gridVertical.Caption = "<style>LAST ENGINES DATA</style>";
        //                }
        //                else
        //                {
        //                    //lblBKTotDay.Visible = false;
        //                    gridVertical.DataSource = null;
        //                    gridVertical.DataBind();
        //                    tdError.Visible = false;
        //                    //tdMsg.Visible = false;
        //                    //lblPending.Visible = false;
        //                    //lblBK.Visible = false;
        //                    //lblPlanQty.Visible = false;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            tdError.Visible = true;
        //            lblError.Text = ex.Message.ToString();
        //        }
        //        finally { }
        //        CheckInfo();
        //    }
        //    catch (Exception ex)
        //    {
        //        tdError.Visible = true;
        //        lblError.Text = ex.Message.ToString();
        //    }
        //    finally { }
        //}

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
        public JsonResult CheckInfos() 
        {
            try
            {
                string plant = Convert.ToString(ConfigurationManager.AppSettings["FT_ENGINE_PLANT"]);
                string family = Convert.ToString(ConfigurationManager.AppSettings["FT_ENGINE_FAMILY"]);
                query = "select remarks1 || '#' || SRLNO from XXES_LIVE_DATA where stage='INJ' and DATA_TYPE='MSG_INJ'";
                string data = get_Col_Value(query);
                var result = new injectorView();
                if (!string.IsNullOrEmpty(data))
                {
                    if (data.Contains("#"))
                    {
                        if (!string.IsNullOrEmpty(data.Split('#')[1].Trim()))
                        {
                            result.lblSrlno = "ENGINE SERIAL NO : " + data.Split('#')[1].Trim();
                            result.lblDcode = "ENGINE CODE : " + get_Col_Value("select ITEM_CODE from xxes_engine_status where engine_srno='" + data.Split('#')[1].Trim() + "'");
                        }
                        else
                        {
                            result.lblSrlno = "ENGINE SERIAL NO : "; result.lblDcode = "ENGINE CODE : ";
                            result.lblInj1Srlno = result.lblInj2Srlno = result.lblInj3Srlno = result.lblInj4Srlno = string.Empty;
                        }
                        if (data.Split('#')[0].Trim().Contains("$"))
                        {
                            string[] words = data.Split('#')[0].Trim().Split('$');
                            for (int i = 0; i < words.Length; i++)
                            {
                                if (words.Length == 4)
                                {
                                    if (!string.IsNullOrEmpty(words[0].Trim().Split('=')[1].Trim()))
                                    {
                                        result.lblInj1Srlno = words[0].Trim().Split('=')[1].Trim();
                                    }
                                    else
                                    {
                                        result.lblInj1Srlno = string.Empty;

                                    }
                                    if (!string.IsNullOrEmpty(words[1].Trim()))
                                    {
                                        result.lblInj2Srlno = words[1].Trim();

                                    }
                                    else
                                    {
                                        result.lblInj2Srlno = string.Empty;


                                    }
                                    if (!string.IsNullOrEmpty(words[2].Trim()))
                                    {
                                        result.lblInj3Srlno = words[2].Trim();

                                    }
                                    else
                                    {
                                        result.lblInj3Srlno = string.Empty;


                                    }
                                    if (!string.IsNullOrEmpty(words[3].Trim()))
                                    {
                                        result.lblInj4Srlno = words[3].Trim();

                                    }
                                    else
                                    {
                                        result.lblInj4Srlno = string.Empty;

                                    }
                                }
                            }
                        }
                        else
                        {
                            result.lblInj1Srlno = result.lblInj2Srlno = result.lblInj3Srlno = result.lblInj4Srlno = string.Empty;

                        }
                    }


                }
                else
                {
                    //tdMsg.Visible = false;
                    result.lblSrlno = "ENGINE SERIAL NO : "; result.lblDcode = "ENGINE CODE : ";
                    result.lblInj1Srlno = result.lblInj2Srlno = result.lblInj3Srlno = result.lblInj4Srlno = string.Empty;
                }
                query = "select remarks1 from XXES_LIVE_DATA where stage='INJ' and (DATA_TYPE='ERROR_INJ' or DATA_TYPE='ERROR') ";
                data = get_Col_Value(query);
                if (!string.IsNullOrEmpty(data))
                {
                    result.tdError = true;
                    result.lblErrordb = data;
                }
                else
                    result.tdError = false;

                if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='inj' and status='online' and description='" + plant.Trim().ToUpper() + "'"))
                {
                    result.ImageUrl = "~\\image\\Green.png";
                    result.ImageStatus = true;
                }
                else if (fun.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='inj' and status='offline' and description='" + plant.Trim().ToUpper() + "'"))
                {
                    result.ImageUrl = "~\\image\\Red.png";
                    result.ImageStatus = true;
                }
                else
                {
                    result.ImageStatus = false;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch { throw; }
            finally { }
        }
        public DataTable getLastScannedEngines()
        {
            DataTable dt = new DataTable();
            try
            {

                query = "select * from  (Select s.ITEM_CODE || '_' || ENGINE_SRNO || '_' || INJECTOR1|| '_' ||INJECTOR2|| '_' ||INJECTOR3|| '_' ||INJECTOR4 as LAST_ENGINES_DATA  from XXES_ENGINE_STATUS s ,XXES_ENGINE_MASTER m where  s.ITEM_CODE=m.ITEM_CODE and m.INJECTOR='Y' and m.plant_code=s.plant_code and m.item_code=s.item_code  order by entrydate desc) where rownum<=3 ";
                dt = returnDataTable(query);

            }
            catch { }
            finally { }
            return dt;
            using (OracleConnection ConOrcl = new OracleConnection(Function.orCnstr))
            {
                OracleCommand cmd = new OracleCommand("usp_getLiveData", ConOrcl);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("Process", OracleDbType.Varchar2, ParameterDirection.Input).Value = "INJ";
                cmd.Parameters.Add("p_Plant", OracleDbType.Varchar2, ParameterDirection.Input).Value = Convert.ToString(ConfigurationManager.AppSettings["PT_ENGINE_PLANT"]);
                cmd.Parameters.Add("p_Family", OracleDbType.Varchar2, ParameterDirection.Input).Value = Convert.ToString(ConfigurationManager.AppSettings["PT_ENGINE_FAMILY"]);
                cmd.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);

                try
                {
                    ConOrcl.Open();
                    cmd.ExecuteNonQuery();
                    OracleDataAdapter Oda = new OracleDataAdapter(cmd);
                    Oda.Fill(dt);
                    return dt;
                }
                catch
                {
                    throw;
                }
                finally { ConOrcl.Dispose(); }
            }
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

        protected void Timer1_Tick(object sender, EventArgs e)
        { 
            var result = new injectorView();
            try
            {

                fun.ServerDate = fun.GetServerDateTime();
                _GetDailyPlanVertical();
                Random rnd = new Random();
                //lblMsg.ForeColor = Color.FromArgb((rnd.Next(0, 255)), (rnd.Next(0, 255)), (rnd.Next(0, 255)));
            }
            catch (Exception ex)
            {
                result.tdError = true;
                result.lblError = ex.Message.ToString();
                Timer1_Tick(new object(), new EventArgs());
            }
            finally { }
        }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            var result = new injectorView();
            
            try
            {
                query = "delete from xxes_live_data where stage='INJ' and (DATA_TYPE='MSG_INJ' or data_type='ERROR_INJ')";
                fun.EXEC_QUERY(query);
            }
            catch (Exception ex)
            {
                result.tdError = true;
                result.lblError = ex.Message.ToString();
            }
            finally { }
        }
        public PartialViewResult _GetDailyPlanVertical()
        {
            var result = new injectorView();
            DataTable data = new DataTable();
            try
            {
                DateTime Plandate = new DateTime(); string expqty = "";
                CheckInfos();
                result.lblError = "";
                try
                {
                    using (dt = getLastScannedEngines())
                    //using (dt = returnDataTable("select Item_Code,Engine_Srno,Injector1,Injector2,Injector3,Injector4 from xxes_engine_status order by Entrydate desc"))
                    {
                        if (dt.Rows.Count > 0)
                        {
                            data = dt;
                            //gridVertical.Caption = "<style>LAST ENGINES DATA</style>";
                        }
                        else
                        {
                            //lblBKTotDay.Visible = false;
                            data = null;
                            result.tdError= false;
                            //tdMsg.Visible = false;
                            //lblPending.Visible = false;
                            //lblBK.Visible = false;
                            //lblPlanQty.Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.tdError= true;
                    result.lblError = ex.Message.ToString();
                }
                finally { }
                CheckInfos();
            }
            catch (Exception ex)
            {
                result.tdError = true;
                result.lblError = ex.Message.ToString();
            }
            return PartialView(data);
        }

        public class injectorView
        {
            public string lblSrlno { get; set; }
            public string lblDcode { get; set; }
            public string lblInj1Srlno { get; set; }
            public string lblInj2Srlno { get; set; }
            public string lblInj3Srlno { get; set; }
            public string lblInj4Srlno { get; set; }
            public bool tdError { get; set; }
            public string lblErrordb { get; set; }
            public string lblError { get; set; }
            public string ImageUrl { get; set; }
            public bool ImageStatus { get; set; }

        }
        //private void CheckInfo()
        //{
        //    try
        //    {
        //        string plant = Convert.ToString(ConfigurationManager.AppSettings["FT_ENGINE_PLANT"]);
        //        string family = Convert.ToString(ConfigurationManager.AppSettings["FT_ENGINE_FAMILY"]);
        //        query = "select remarks1 || '#' || SRLNO from XXES_LIVE_DATA where stage='INJ' and DATA_TYPE='MSG_INJ'";
        //        string data = get_Col_Value(query);
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            if (data.Contains("#"))
        //            {
        //                if (!string.IsNullOrEmpty(data.Split('#')[1].Trim()))
        //                {
        //                    lblSrlno = "ENGINE SERIAL NO : " + data.Split('#')[1].Trim();
        //                    lblDcode.Text = "ENGINE CODE : " + get_Col_Value("select ITEM_CODE from xxes_engine_status where engine_srno='" + data.Split('#')[1].Trim() + "'");
        //                }
        //                else
        //                {
        //                    lblSrlno.Text = "ENGINE SERIAL NO : "; lblDcode.Text = "ENGINE CODE : ";
        //                    lblInj1Srlno.Text = lblInj2Srlno.Text = lblInj3Srlno.Text = lblInj4Srlno.Text = string.Empty;
        //                }
        //                if (data.Split('#')[0].Trim().Contains("$"))
        //                {
        //                    string[] words = data.Split('#')[0].Trim().Split('$');
        //                    for (int i = 0; i < words.Length; i++)
        //                    {
        //                        if (words.Length == 4)
        //                        {
        //                            if (!string.IsNullOrEmpty(words[0].Trim().Split('=')[1].Trim()))
        //                            {
        //                                lblInj1Srlno.Text = words[0].Trim().Split('=')[1].Trim();
        //                            }
        //                            else
        //                            {
        //                                lblInj1Srlno.Text = string.Empty;

        //                            }
        //                            if (!string.IsNullOrEmpty(words[1].Trim()))
        //                            {
        //                                lblInj2Srlno.Text = words[1].Trim();

        //                            }
        //                            else
        //                            {
        //                                lblInj2Srlno.Text = string.Empty;


        //                            }
        //                            if (!string.IsNullOrEmpty(words[2].Trim()))
        //                            {
        //                                lblInj3Srlno.Text = words[2].Trim();

        //                            }
        //                            else
        //                            {
        //                                lblInj3Srlno.Text = string.Empty;


        //                            }
        //                            if (!string.IsNullOrEmpty(words[3].Trim()))
        //                            {
        //                                lblInj4Srlno.Text = words[3].Trim();

        //                            }
        //                            else
        //                            {
        //                                lblInj4Srlno.Text = string.Empty;

        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    lblInj1Srlno.Text = lblInj2Srlno.Text = lblInj3Srlno.Text = lblInj4Srlno.Text = string.Empty;

        //                }
        //            }


        //        }
        //        else
        //        {
        //            //tdMsg.Visible = false;
        //            lblSrlno.Text = "ENGINE SERIAL NO : "; lblDcode.Text = "ENGINE CODE : ";
        //            lblInj1Srlno.Text = lblInj2Srlno.Text = lblInj3Srlno.Text = lblInj4Srlno.Text = string.Empty;
        //        }
        //        query = "select remarks1 from XXES_LIVE_DATA where stage='INJ' and (DATA_TYPE='ERROR_INJ' or DATA_TYPE='ERROR') ";
        //        data = get_Col_Value(query);
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            tdError.Visible = true;
        //            lblErrordb.Text = data;
        //        }
        //        else
        //            tdError.Visible = false;

        //        if (pbf.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='INJ' and STATUS='ONLINE' and description='" + plant.Trim().ToUpper() + "'"))
        //        {
        //            imgstatus.ImageUrl = "~\\Images\\Green.png";
        //            imgstatus.Visible = true;
        //        }
        //        else if (pbf.CheckExits("select count(*) from xxes_sft_settings where parameterinfo='INJ' and STATUS='OFFLINE' and description='" + plant.Trim().ToUpper() + "'"))
        //        {
        //            imgstatus.ImageUrl = "~\\Images\\Red.png";
        //            imgstatus.Visible = true;
        //        }
        //        else
        //        {
        //            imgstatus.Visible = false;
        //        }
        //    }
        //    catch { throw; }
        //    finally { }
        //}
     
    }
    
}