using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Common
{
    public class JobFinalFunction
    {
        string query = string.Empty;
        Function fun = new Function();
        public List<DDLTextValue> Battery_Name()
        {
            DataTable dt = null;
            try
            {
                List<DDLTextValue> Unit = new List<DDLTextValue>();
                query = string.Format(@"select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' 
                                        order by PARAMETERINFO");
                dt = fun.returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Unit.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["Name"].ToString(),
                        });
                    }
                }
                return Unit;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                fun.ConClose();
            }
        }
        public string getItemCode(string srlno)
        {
            try
            {
                return fun.get_Col_Value("select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='" + srlno.Trim().ToUpper() + "'");
            }
            catch { throw; }
            finally { }
        }
        public Tuple<bool, string, string, string, string> CheckBuckleUp(TRACTOR data, string fcode, bool isRearAxelRequire, bool isTransRequire)
        {
            
            bool result = true; string msg = string.Empty, errorNo = string.Empty;
            string TransDcode = "", AxelDCODE = "", FoundJob = "";
           string DCODE_TRANS="", DCODE_AXEL="";
            try
            {
                if (isTransRequire && string.IsNullOrEmpty(data.TRANSMISSION_SRLNO))
                {
                    result = false;
                    errorNo = "1";
                    msg = "Transmission Serial No Is Required !!";
                    fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "TRANSMISSION SERIAL NO IS REQUIRED", data.PLANT_CODE, data.FAMILY_CODE);
                    return new Tuple<bool, string, string, string,string>(result, errorNo, msg, "","");
                }
                if (isRearAxelRequire && string.IsNullOrEmpty(data.REARAXEL_SRLNO))
                {
                    result = false;
                    errorNo = "2";
                    msg = "RearAxel Serial No Is Required !!";
                    fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "REARAXEL SERIAL NO IS REQUIRED", data.PLANT_CODE, data.FAMILY_CODE);
                    return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                }
                if (!string.IsNullOrEmpty(data.TRANSMISSION_SRLNO))
                {
                    TransDcode = getItemCode(data.TRANSMISSION_SRLNO.Trim());
                    if (string.IsNullOrEmpty(TransDcode.Trim()))
                    {
                        result = false;
                        errorNo = "1";
                        msg = "Transmission Item Code Not Found";
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "TRANSMISSION ITEMCODE NOT FOUND. SCANNED SERIAL NO ARE " + data.TRANSMISSION_SRLNO + "", data.PLANT_CODE, data.FAMILY_CODE);
                        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                    }

                    query = string.Format(@"select JOBID from XXES_JOB_STATUS where TRANSMISSION_SRLNO='{0}' and 
                            JOBID<>'{1}'", data.TRANSMISSION_SRLNO.Trim().ToUpper(), data.JOBID.Trim().ToUpper());
                    FoundJob = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(FoundJob))
                    {
                        result = false;
                        errorNo = "1";
                        msg = "Transmission SrlNo Already Scanned On Job : " + FoundJob.Trim();
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "TRANSMISSION SRLNO ALREADY SCANNED ON JOB : " + FoundJob.Trim() + " SCANNED SERIAL NO ARE " + data.TRANSMISSION_SRLNO + "", data.PLANT_CODE, data.FAMILY_CODE);
                        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                    }
                }

                
                if (!string.IsNullOrEmpty(data.REARAXEL_SRLNO))
                {
                    AxelDCODE = getItemCode(data.REARAXEL_SRLNO.Trim());
                    if (string.IsNullOrEmpty(AxelDCODE))
                    {
                        result = false;
                        errorNo = "2";
                        msg = "RearAxle Item Code Not Found";
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "REARAXLE ITEMCODE NOT FOUND. SCANNED SERIAL NO ARE " + data.REARAXEL_SRLNO + "", data.PLANT_CODE, data.FAMILY_CODE);
                        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                    }

                    query = string.Format(@"select JOBID from XXES_JOB_STATUS 
                                        where REARAXEL_SRLNO='{0}' and JOBID<>'{1}'", data.REARAXEL_SRLNO.Trim().ToUpper(), data.JOBID.Trim().ToUpper());
                    FoundJob = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(FoundJob))
                    {
                        result = false;
                        errorNo = "2";
                        msg = "RearAxel SrlNo Already Scanned On Job" + FoundJob.Trim().ToUpper();
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "REARAXEL SRLNO ALREADY SCANNED ON JOB : " + FoundJob.Trim() + " SCANNED SERIAL NO ARE " + data.REARAXEL_SRLNO + "", data.PLANT_CODE, data.FAMILY_CODE);
                        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                    }

                }

                  query = string.Format(@"select TRANSMISSION, REARAXEL,TRANSMISSION_DESCRIPTION,REARAXEL_DESCRIPTION  
                                    from XXES_ITEM_MASTER where trim(ITEM_CODE)='{0}' and PLANT_CODE='{1}' and 
                                    family_code='{2}'", fcode.ToUpper().Trim(), data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                    DataTable Data = fun.returnDataTable(query);
                    if (Data.Rows.Count > 0)
                    {
                        DCODE_TRANS = Convert.ToString(Data.Rows[0]["TRANSMISSION"]) == null ? "" : Convert.ToString(Data.Rows[0]["TRANSMISSION"]).Trim().ToUpper();
                        DCODE_AXEL = Convert.ToString(Data.Rows[0]["REARAXEL"]) == null ? "" : Convert.ToString(Data.Rows[0]["REARAXEL"]).Trim().ToUpper();
                        if (!string.IsNullOrEmpty(data.TRANSMISSION_SRLNO) && TransDcode.Trim().ToUpper() != DCODE_TRANS.Trim().ToUpper())
                        {
                            result = false;
                            errorNo = "1";
                            msg = "MisMatch !! in Actual Transmission: " + DCODE_TRANS + " And Found Transmission: " + TransDcode;
                            fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "MISMATCH ACTUAL TRANSMISSION " + DCODE_TRANS + " AND FOUND TRANSMISSION " + TransDcode + ". SCANNED SERIAL NO ARE " + data.TRANSMISSION_SRLNO, data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                            return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                        }
                        else if (!string.IsNullOrEmpty(data.REARAXEL_SRLNO) && AxelDCODE.Trim().ToUpper() != DCODE_AXEL.Trim().ToUpper())
                        {
                            result = false;
                            errorNo = "2";
                            msg = "MisMatch !! in Actual Rear Axel: " + DCODE_AXEL + " And Rear Axel: " + AxelDCODE;
                            fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", data.JOBID.Trim(), "MISMATCH ACTUAL AXEL " + DCODE_AXEL + " AND FOUND AXEL " + AxelDCODE + ". SCANNED SERIAL NO ARE " + data.REARAXEL_SRLNO, data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                            return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
                        }
                        else
                        {
                            result = true;
                            errorNo = "-99";
                            msg = "OK";
                            return new Tuple<bool, string, string, string, string>(result, errorNo, msg, DCODE_TRANS, DCODE_AXEL);
                        }

                    }
               
            }
            catch (Exception ex)
            {

                result = false;
                errorNo = "0";
                msg = ex.Message;
                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
            }
            
            return new Tuple<bool, string, string, string, string>(result, errorNo, msg, DCODE_TRANS, DCODE_AXEL);
        }

        public void GetROPSSrno(string plant, string family, string fcode, string ROPS_DCODE, out string ROPS_SRNO)
        {
            try
            {
                long Current_Serial_number = 0;
                string toReturn = string.Empty;
                ROPS_SRNO = "";
               
                    try
                    {
                        
                        string Query = string.Format(@"select start_serialno,end_serialno,current_serialno from xxes_torque_master 
                                        where item_dcode ='{0}' and srno_req=1 and plant_code='{1}' and family_code='{2}'"
                                        ,ROPS_DCODE.Trim(),plant.Trim(),family.Trim());

                    DataTable dt = fun.returnDataTable(Query);    
                            
                            if (dt.Rows.Count > 0)
                            {
                                foreach(DataRow dr in dt.Rows)
                                {
                                    if (Convert.ToString(dr["current_serialno"]).Trim() == "" || Convert.ToString(dr["current_serialno"]).Trim() == "0")
                                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dr["start_serialno"]).Trim())) + 1;
                                    else if (Convert.ToString(dr["current_serialno"]).Trim() != "")
                                    {
                                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dr["current_serialno"]).Trim())) + 1;
                                    }
                                    if (Current_Serial_number > Convert.ToInt32(Convert.ToString(Convert.ToString(dr["end_serialno"]).Trim())))
                                    {
                                        Current_Serial_number = -99; //series full
                                        throw new Exception("ROPS SERIES FOR FCODE " + fcode + " REACHED ITS MAXIMUM LEVEL FOR PLANT " + plant + " FAMILY " + family);
                                    }
                                    toReturn = Convert.ToString(Current_Serial_number);
                                    while (toReturn.Trim().Length < Convert.ToString(dr["start_serialno"]).Trim().Length)
                                    {
                                        toReturn = "0" + toReturn;
                                    }

                                    if (string.IsNullOrEmpty(toReturn))
                                    {
                                        throw new Exception("UNABLE TO GET RUNNING NO. FOR ROPS DCODE " + ROPS_DCODE + ".CHECK ROPS MASTER");
                                    }

                                }
                            }
                        

                    }
                    catch (Exception ex)
                    {
                        fun.LogWrite(ex);
                        throw;
                    }
                    finally { }
               
                ROPS_SRNO = toReturn;
            }
            catch (Exception ex)
            {
                ROPS_SRNO = "";
                fun.LogWrite(ex);
                throw;
            }
        }

        //public Tuple<bool, string, string, string, string> CheckEngine(string plant, string family, string EnggSrlno, string job, string fcode, bool isEngineRequire, bool isSrNoRequire, string TractorSrno, string Fcode_id)
        //{
        //    bool result = true; string msg = string.Empty, errorNo = string.Empty;
        //    string FoundenggDcode = "",fcode_id = "";
        //    try
        //    {
        //        if (isEngineRequire == true)
        //        {
        //            query = string.Format(@"select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='{0}' and 
        //                        plant_code='{1}'",EnggSrlno.Trim().ToUpper(), plant.Trim().ToUpper());
        //            FoundenggDcode = fun.get_Col_Value(query);
        //            if (string.IsNullOrEmpty(FoundenggDcode))
        //            {
        //                result = false;
        //                errorNo = "4";
        //                msg = "Engine Item Code Not Found";
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR", "EN", job.Trim(), "ENGINE ITEM CODE NOT FOUND IN PRINT SERIAL NUMBER TABLE. SCANNED SERIAL NO ARE " + EnggSrlno.Trim().ToUpper() + "", plant.Trim().ToUpper(), family.Trim().ToUpper());
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //            }

        //            query = "select FCODE_SRLNO || '#' || ENGINE || '#' ||ENGINE_SRLNO || '#' || JOBID || '#' || FCODE_ID from XXES_JOB_STATUS where ENGINE_SRLNO='" + EnggSrlno.Trim().ToUpper() + "' and jobid<>'" + job.Trim() + "'";
        //           string Misc = fun.get_Col_Value(query);
        //            if (!string.IsNullOrEmpty(Misc.Trim()) && Misc.Trim().Contains('#'))
        //            {
        //                fcode_id = Misc.Split('#')[4].Trim();
        //                if ((!string.IsNullOrEmpty(Misc.Split('#')[0].Trim())) || optPlanned.Checked == true)
        //                {
        //                    MessageBox.Show("Engine Srlno. Already Scanned\nOn Tractor:" + Misc.Split('#')[0].Trim(), PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                    pbf.Insert_Into_ActivityLog("SCAN_ERROR", "EN", job.Trim(), "ENGINE SRLNO ALREADY SCANNED ON TRACTOR " + Misc.Split('#')[0].Trim() + ". SCANNED SERIAL NO ARE " + EnggSrlno, Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper());
        //                    newTractorSrno = ""; SrnotoUpdate = ""; stageid = ""; DCODE_ENGINE = "";
        //                    txtJEngine.Enabled = true;
        //                    txtJEngine.Focus();
        //                    return false;
        //                }
        //                if (!chkReplace.Checked)
        //                {
        //                    if (MessageBox.Show("Engine SrlNo Already Scanned On Tractor: " + Misc.Split('#')[0].Trim() + "\nDo you want to save entered engine for Current Job: " + job.Trim() + "\nFound On Job: " + Misc.Split('#')[3].Trim(), "Invalid", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.No && !string.IsNullOrEmpty(Misc.Split('#')[0].Trim()))
        //                    {
        //                        newTractorSrno = ""; SrnotoUpdate = ""; stageid = ""; DCODE_ENGINE = "";
        //                        txtJEngine.Enabled = true;
        //                        txtJEngine.Focus();
        //                        return false;
        //                    }
        //                    else
        //                    {

        //                        NewEngine = Prompt.ShowDialog("Enter new engine serial number for job " + Misc.Split('#')[3].Trim() + "", "INPUT SERIAL NUMBER", false);
        //                        if (string.IsNullOrEmpty(NewEngine))
        //                        {
        //                            MessageBox.Show("Invalid New engine Number for job " + Misc.Split('#')[3].Trim(), "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            pbf.Insert_Into_ActivityLog("SCAN_ERROR", "EN", job.Trim(), "INVALID NEW ENGINE NUMBER FOR JOB " + Misc.Split('#')[3].Trim() + ". SCANNED SERIAL NO ARE " + EnggSrlno, Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper());
        //                            return false;
        //                        }
        //                        if (Misc.Split('#')[1].Trim() != pbf.getItemCode(NewEngine))
        //                        {
        //                            MessageBox.Show("Invalid New engine Number for job " + Misc.Split('#')[3].Trim(), "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            pbf.Insert_Into_ActivityLog("SCAN_ERROR", "EN", job.Trim(), "INVALID NEW ENGINE NUMBER FOR JOB " + Misc.Split('#')[3].Trim() + ". SCANNED SERIAL NO ARE " + EnggSrlno, Convert.ToString(cmbPlant.SelectedValue).Trim().ToUpper(), Convert.ToString(cmbFamily.SelectedValue).Trim().ToUpper());
        //                            return false;
        //                        }
        //                        NewEngineJob = Misc.Split('#')[3].Trim();

        //                    }
        //                }
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(AxelSrlno))
        //        {
        //            AxelDCODE = getItemCode(AxelSrlno);
        //            if (string.IsNullOrEmpty(AxelDCODE))
        //            {
        //                result = false;
        //                errorNo = "2";
        //                msg = "RearAxle Item Code Not Found";
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", job.Trim(), "REARAXLE ITEMCODE NOT FOUND. SCANNED SERIAL NO ARE " + AxelSrlno + "", plant, family);
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //            }

        //            query = string.Format(@"select JOBID from XXES_JOB_STATUS 
        //                                where REARAXEL_SRLNO='{0}' and JOBID<>'{1}'", AxelSrlno.Trim().ToUpper(), job.Trim().ToUpper());
        //            FoundJob = fun.get_Col_Value(query);
        //            if (!string.IsNullOrEmpty(FoundJob))
        //            {
        //                result = false;
        //                errorNo = "2";
        //                msg = "RearAxel SrlNo Already Scanned On Job" + FoundJob.Trim().ToUpper();
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", job.Trim(), "REARAXEL SRLNO ALREADY SCANNED ON JOB : " + FoundJob.Trim() + " SCANNED SERIAL NO ARE " + AxelSrlno + "", plant, family);
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //            }

        //        }

        //        query = string.Format(@"select TRANSMISSION, REARAXEL,TRANSMISSION_DESCRIPTION,REARAXEL_DESCRIPTION  
        //                            from XXES_ITEM_MASTER where trim(ITEM_CODE)='{0}' and PLANT_CODE='{1}' and 
        //                            family_code='{2}'", fcode.ToUpper().Trim(), plant.Trim(), family.Trim());
        //        DataTable Data = fun.returnDataTable(query);
        //        if (Data.Rows.Count > 0)
        //        {
        //            DCODE_TRANS = Convert.ToString(Data.Rows[0]["TRANSMISSION"]) == null ? "" : Convert.ToString(Data.Rows[0]["TRANSMISSION"]).Trim().ToUpper();
        //            DCODE_AXEL = Convert.ToString(Data.Rows[0]["REARAXEL"]) == null ? "" : Convert.ToString(Data.Rows[0]["REARAXEL"]).Trim().ToUpper();
        //            if (!string.IsNullOrEmpty(transSrlno) && TransDcode.Trim().ToUpper() != DCODE_TRANS.Trim().ToUpper())
        //            {
        //                result = false;
        //                errorNo = "1";
        //                msg = "MisMatch !! in Actual Transmission: " + DCODE_TRANS + " And Found Transmission: " + TransDcode;
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", job.Trim(), "MISMATCH ACTUAL TRANSMISSION " + DCODE_TRANS + " AND FOUND TRANSMISSION " + TransDcode + ". SCANNED SERIAL NO ARE " + transSrlno, plant.Trim().ToUpper(), family.Trim().ToUpper());
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //            }
        //            else if (!string.IsNullOrEmpty(AxelSrlno) && AxelDCODE.Trim().ToUpper() != DCODE_AXEL.Trim().ToUpper())
        //            {
        //                result = false;
        //                errorNo = "2";
        //                msg = "MisMatch !! in Actual Rear Axel: " + DCODE_AXEL + " And Rear Axel: " + AxelDCODE;
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR", "BK", job.Trim(), "MISMATCH ACTUAL AXEL " + DCODE_AXEL + " AND FOUND AXEL " + AxelDCODE + ". SCANNED SERIAL NO ARE " + AxelSrlno, plant.Trim().ToUpper(), family.Trim().ToUpper());
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //            }
        //            else
        //            {
        //                result = true;
        //                errorNo = "-99";
        //                msg = "OK";
        //                return new Tuple<bool, string, string, string, string>(result, errorNo, msg, DCODE_TRANS, DCODE_AXEL);
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        result = false;
        //        errorNo = "0";
        //        msg = ex.Message;
        //        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");
        //    }

        //    return new Tuple<bool, string, string, string, string>(result, errorNo, msg, DCODE_TRANS, DCODE_AXEL);
        //}
       
        public Tuple<bool, string, string, string> ChecKHydraulic(TRACTOR data, string FCODE, bool isRequireHyrdraulic)
        {
            bool result = true; string msg = string.Empty, errorNo = string.Empty;
            string DCODE_HYDRUALIC = "", FoundJob ="";
            try
            {
                if(isRequireHyrdraulic && string.IsNullOrEmpty(data.HYDRALUIC_SRLNO))
                {
                    result = false;
                    errorNo = "5";
                    msg = "Hydraulic Serial No Is Required !!";
                    fun.Insert_Into_ActivityLog("SCAN_ERROR", "HYDRALUIC", data.JOBID.Trim(), "HYDRALUIC SRLNO IS REQUIRED", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                    return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
                }
                if (!string.IsNullOrEmpty(data.HYDRALUIC_SRLNO))
                {
                    query = string.Format(@"select JOBID from XXES_JOB_STATUS where HYDRALUIC_SRLNO='{0}' 
                                and jobid<>'{1}'", data.HYDRALUIC_SRLNO.Trim().ToUpper(), data.JOBID.Trim());
                    FoundJob = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(FoundJob))
                    {
                        result = false;
                        errorNo = "5";
                        msg = "Hydraulic Serial No Already Scanned On Job : " + FoundJob.Trim();
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "HYDRALUIC", data.JOBID.Trim(), "HYDRALUIC SRLNO ALREADY SCANNED ON JOB : " + FoundJob.Trim() + " SCANNED SERIAL NO ARE " + data.HYDRALUIC_SRLNO + "", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                        return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
                    }
                    query = string.Format(@"select XS.DCODE from XXES_ITEM_MASTER XM,XXES_PRINT_SERIALS XS 
                            where trim(XM.ITEM_CODE)='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}' and 
                            XM.HYDRAULIC=XS.DCODE and trim(XS.SRNO)='{3}' and XM.PLANT_CODE=XS.PLANT_CODE and 
                            XM.FAMILY_CODE=XS.FAMILY_CODE", FCODE.Trim(), data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.HYDRALUIC_SRLNO.Trim().ToUpper());

                    string Misc = fun.get_Col_Value(query);

                    if (string.IsNullOrEmpty(Misc))
                    {
                        result = false;
                        errorNo = "5";
                        msg = "MisMatch Hydraulic Srlno";
                        fun.Insert_Into_ActivityLog("SCAN_ERROR", "HYDRAULIC", data.JOBID, "MISMATCH HYDRAULIC.SCANNED SERIAL NO ARE " + data.HYDRALUIC_SRLNO.Trim() + "", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                        return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
                    }
                    else
                    {
                        DCODE_HYDRUALIC = Misc.Trim();
                        result = true;
                        errorNo = "-99";
                        msg = "OK";
                        return new Tuple<bool, string, string, string>(result, errorNo, msg, DCODE_HYDRUALIC);
                        
                    }

                }
               
            }
            catch (Exception ex)
            {

                result = false;
                errorNo = "0";
                msg = ex.Message;
                return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
            }

            return new Tuple<bool, string, string, string>(result, errorNo, msg, DCODE_HYDRUALIC);
        }
        public Tuple<bool, string, string, string,string> CheckTyres_COM(bool isRequire, string TyreSrlno1, string TyreSrlno2, string JOB, string FCODE, string Mode )
        {
            bool result = true; string msg = string.Empty, errorNo = string.Empty;
           string DCODE = "", MAKE = "";
            try
            {
                if (TyreSrlno1.Trim().ToUpper() == TyreSrlno2.Trim().ToUpper() && isRequire)
                {
                    if (Mode == "RT")
                    {
                        result = false;
                        errorNo = "21";
                        msg = "Both Rear Tyre Assembly serialnos should not be the same !!";
                        return new Tuple<bool, string, string, string,string>(result, errorNo, msg, "","");
                    }
                    else
                    {
                        result = false;
                        errorNo = "22";
                        msg = "Both Front Tyre Assembly serialnos should not be the same !!";
                        return new Tuple<bool, string, string, string, string>(result, errorNo, msg, "", "");

                    }

                }
                //if (!string.IsNullOrEmpty(data.HYDRALUIC_SRLNO))
                //{
                //    query = string.Format(@"select JOBID from XXES_JOB_STATUS where HYDRALUIC_SRLNO='{0}' 
                //                and jobid<>'{1}'", data.HYDRALUIC_SRLNO.Trim().ToUpper(), data.JOBID.Trim());
                //    FoundJob = fun.get_Col_Value(query);
                //    if (!string.IsNullOrEmpty(FoundJob))
                //    {
                //        result = false;
                //        errorNo = "5";
                //        msg = "Hydraulic Serial No Already Scanned On Job : " + FoundJob.Trim();
                //        fun.Insert_Into_ActivityLog("SCAN_ERROR", "HYDRALUIC", data.JOBID.Trim(), "HYDRALUIC SRLNO ALREADY SCANNED ON JOB : " + FoundJob.Trim() + " SCANNED SERIAL NO ARE " + data.HYDRALUIC_SRLNO + "", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                //        return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
                //    }
                //    query = string.Format(@"select XS.DCODE from XXES_ITEM_MASTER XM,XXES_PRINT_SERIALS XS 
                //            where trim(XM.ITEM_CODE)='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}' and 
                //            XM.HYDRAULIC=XS.DCODE and trim(XS.SRNO)='{3}' and XM.PLANT_CODE=XS.PLANT_CODE and 
                //            XM.FAMILY_CODE=XS.FAMILY_CODE", FCODE.Trim(), data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim(), data.HYDRALUIC_SRLNO.Trim().ToUpper());

                //    string Misc = fun.get_Col_Value(query);

                //    if (string.IsNullOrEmpty(Misc))
                //    {
                //        result = false;
                //        errorNo = "5";
                //        msg = "MisMatch Hydraulic Srlno";
                //        fun.Insert_Into_ActivityLog("SCAN_ERROR", "HYDRAULIC", data.JOBID, "MISMATCH HYDRAULIC.SCANNED SERIAL NO ARE " + data.HYDRALUIC_SRLNO.Trim() + "", data.PLANT_CODE.Trim(), data.FAMILY_CODE.Trim());
                //        return new Tuple<bool, string, string, string>(result, errorNo, msg, "");
                //    }
                //    else
                //    {
                //        DCODE_HYDRUALIC = Misc.Trim();
                //        result = true;
                //        errorNo = "-99";
                //        msg = "OK";
                //        return new Tuple<bool, string, string, string>(result, errorNo, msg, DCODE_HYDRUALIC);

                //    }

                //}

            }
            catch (Exception ex)
            {

                result = false;
                errorNo = "0";
                msg = ex.Message;
                return new Tuple<bool, string, string, string,string>(result, errorNo, msg, "","");
            }

            return new Tuple<bool, string, string, string,string>(result, errorNo, msg, DCODE,MAKE);
        }

        public bool RecordPDIOK(Tractor tractor)
        {
            try
            {
                //query = string.Format(@"update xxes_job_status set PDIOKDATE=SYSDATE+  (1/1440*12),
                //PDIDONEBY='{0}' where fcode_srlno='{1}' and PDIOKDATE is null", PubFun.Login_User, tractor.TSN);
                //EXEC_QUERY(query);
                query = string.Format(@"update xxes_job_status set PDIOKDATE=SYSDATE,
                PDIDONEBY='{0}' where fcode_srlno='{1}' and PDIOKDATE is null", Convert.ToString(HttpContext.Current.Session["Login_User"]), tractor.TSN);
                return fun.EXEC_QUERY(query);
                //query = string.Format(@"select count(*) from XXES_PDIOK Where TSN='{0}'",
                //    tractor.TSN);
                //if (!CheckExits(query))
                //{
                //    query = string.Format(@"insert into XXES_PDIOK(plant_code,family_code,TSN,CREATEDBY) 
                //values('{0}','{1}','{2}','{3}')", tractor.PLANT, tractor.FAMILY, tractor.TSN, PubFun.Login_User);
                //    EXEC_QUERY(query);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}