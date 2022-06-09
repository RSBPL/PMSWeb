using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;

namespace MVCApp.Controllers.Admin
{
    [Authorize]
    public class SettingController : Controller
    {
        Function fun = new Function();
        string query = string.Empty;
        [HttpGet]
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

        [HttpGet]
        public JsonResult BindPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindFamily(string Plant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BindTimeInterval()
        {
            List<DDLTextValue> Time = new List<DDLTextValue>();
            for (int i = 1; i <= 60; i++)
            {
                Time.Add(new DDLTextValue
                {
                    Text = Convert.ToString(i),
                    Value = Convert.ToString(i),
                });
            }
            Time.Insert(0, new DDLTextValue { Text = "SELECT", Value = "SELECT" });
            return Json(Time, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ExistingRecord(SftSetting data)
        {
            string msg = string.Empty;
            SftSetting sft = new SftSetting();
            DataTable dt = new DataTable();
            try
            {
                if (!string.IsNullOrEmpty(data.Plant) && !string.IsNullOrEmpty(data.Family))
                {
                    query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE FROM XXES_SFT_SETTINGS WHERE PLANT_CODE = '{0}' AND 
                                            FAMILY_CODE = '{1}'", data.Plant.Trim(), data.Family.Trim());
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "SUCCESS_INTERVAL")
                                sft.SuccessIntvl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            else if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "ERROR_INTERVAL")
                                sft.ErrorIntvl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            //if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "PRINTQTY_LABEL")
                            //    sft.QtyVeriLbl = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "A4")
                                sft.A4Sheet = true;
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "BARCODE")
                                sft.Barcode = true;
                            if (Convert.ToString(dt.Rows[i]["PARAMVALUE"]) == "QUALITY")
                                sft.Quality = true;
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "KANBAN_PRINT")
                                sft.PrintingCategory = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "QC_FROMDAYS")
                                sft.QCFromDays = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);
                            if (Convert.ToString(dt.Rows[i]["PARAMETERINFO"]) == "VERIFICATION_PRINT")
                                sft.PrintVerification = Convert.ToString(dt.Rows[i]["PARAMVALUE"]);

                        }

                    }
                }
                else
                {
                    msg = "Record Not Found";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            var myResult = new
            {
                Result = sft,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Grid(SftSetting data)
        {
            if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
            {
                return PartialView();
            }
            query = string.Format(@"SELECT PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,AUTOID,
                                    CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MON_YYYY HH24:MI:SS') CREATED_DATE
                                    FROM XXES_SFT_SETTINGS where PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}'", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper());
            DataTable dt = fun.returnDataTable(query);

            ViewBag.DataSource = dt;
            return PartialView();
        }

        [HttpPost]
        public JsonResult Add(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            int j;
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.QCFromDays))
                {
                    if (!Int32.TryParse(data.QCFromDays, out j))
                    {
                        msg = "QC From Days is not a valid number ..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    if (j < 1)
                    {
                        msg = "Minimum no. of QC days should be greater than 0 ..";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }

                }


                data.Status = "Y";
                data.Description = "SCREEN_SETTING";
                var tuple = fun.MobileMsgAlert(data, "ERROR_INTERVAL", "SUCCESS_INTERVAL");
                bool output = fun.QtyVerificationLabel(data, "PRINTQTY_LABEL", "QTY_LABEL_SETTING");
                bool barcode = fun.BarcodePrintingLabel(data, "KANBAN_PRINT", "KANBAN_SETTING");
                bool QcFrmDays = fun.QCFromDays(data, "QC_FROMDAYS", "QC From Date Days");
                bool VeficationPrinting = fun.VerificationPrinting(data, "VERIFICATION_PRINT", "VERIFICATION_SETTING");
                if (tuple.Item2 && output && barcode && QcFrmDays && VeficationPrinting)
                {
                    msg = tuple.Item1;
                    mstType = Validation.str;
                    status = Validation.stus;
                }

                else
                {
                    msg = tuple.Item1;
                    mstType = Validation.str1;
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindStage()
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            result = fun.Fill_All_Stage();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddSMTP(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            int j;
            List<SftSetting> SMTPFinal = new List<SftSetting>();
            try
            {
                if (string.IsNullOrEmpty(data.SMTPEMAILID) || string.IsNullOrEmpty(data.SMTPServer))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.SMTPPSWORD))
                {

                    msg = "SMTP PASSWORD ENTER..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }

                query = "delete from XXES_SMTP_DETAILS";
                fun.EXEC_QUERY(query);
                fun.Insert_Into_ActivityLog("SMTP_DETAILS", "DELETE", data.SMTPServer.ToString().Trim(), query, "", "");
                //fun.LogWrite(query);

                query = "insert into XXES_SMTP_DETAILS(SMTP_SERVER,SMTP_USER,SMTP_PASSWORD,SSL_ENABLE,SMTP_PORT,SMTP_PRIORITY) values('" + data.SMTPServer.Trim() + "','" + data.SMTPEMAILID.Trim() + "','" + data.SMTPPSWORD.Trim() + "','" + (data.ChkSSL == true ? "1" : "0") + "','" + data.SMTPPORT.Trim() + "','" + data.PRIORITY.Trim() + "')";
                if (fun.EXEC_QUERY(query))
                {
                    fun.Insert_Into_ActivityLog("SMTP_DETAILS", "INSERT", data.SMTPServer.Trim(), query, "", "");
                    DataTable dataTable = new DataTable();
                    SMTPFinal = getSMTPData();

                    msg = "SMTP details saved Sucessfully !!";
                }

                else
                {
                    msg = "Some thing wrong !!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(new { Msg = msg, ID = mstType, validation = status, data = SMTPFinal }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult BindSMTPData()
        {
            List<SftSetting> result = new List<SftSetting>();
            result = getSMTPData();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public List<SftSetting> getSMTPData()
        {
            DataTable dt = new DataTable();
            List<SftSetting> sftSettings = new List<SftSetting>();
            try
            {
                dt = fun.returnDataTable("select * from XXES_SMTP_DETAILS");
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        SftSetting SMTPDATA = new SftSetting();
                        SMTPDATA.SMTPEMAILID = Convert.ToString(dr["SMTP_USER"]);
                        SMTPDATA.SMTPServer = Convert.ToString(dr["SMTP_SERVER"]);
                        SMTPDATA.SMTPPSWORD = Convert.ToString(dr["SMTP_PASSWORD"]);
                        SMTPDATA.SMTPPORT = Convert.ToString(dr["SMTP_PORT"]);
                        SMTPDATA.PRIORITY = Convert.ToString(dr["SMTP_PRIORITY"]);
                        if (Convert.ToString(dr["SSL_ENABLE"]).Trim() == "1")
                            SMTPDATA.ChkSSL = true;
                        else
                            SMTPDATA.ChkSSL = false;
                        sftSettings.Add(SMTPDATA);
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return sftSettings;
        }

        [HttpPost]
        public JsonResult TESTSMTP(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            int j;
            if (string.IsNullOrEmpty(data.EmailTo.Trim()))
            {
                msg = "Please enter Email address on which test mail need to send";
                return Json(new { Msg = msg, ID = mstType, validation = status }, JsonRequestBehavior.AllowGet);
            }
            MailMessage message = new MailMessage();
            SmtpClient client = new SmtpClient();

            MailAddress address = new MailAddress(data.SMTPEMAILID.Trim());

            // Set the sender's address
            message.From = address;

            // Allow multiple "To" addresses to be separated by a semi-colon
            if (data.SMTPEMAILID.Trim().Length > 0)
            {
                foreach (string addr in data.EmailTo.Trim().Split(';'))
                {
                    if (!string.IsNullOrEmpty(addr))
                        message.To.Add(new MailAddress(addr));
                }
            }

            // Set the subject and message body text
            message.Subject = "TEST MAIL";
            message.Body = "The is system generated test mail";
            // TODO: *** Modify for your SMTP server ***
            // Set the SMTP server to be used to send the message
            client.Host = data.SMTPServer.Trim();
            string domain = "barcode4u.com";
            if (data.SMTPPORT.Trim() != "")
                client.Port = Convert.ToInt32(data.SMTPPORT.Trim());
            if (data.PRIORITY.Trim().ToUpper() == "NORMAL")
                message.Priority = MailPriority.Normal;
            else if (data.PRIORITY.Trim().ToUpper() == "HIGH")
                message.Priority = MailPriority.High;
            else if (data.PRIORITY.Trim().ToUpper() == "LOW")
                message.Priority = MailPriority.Low;
            message.IsBodyHtml = true;
            client.Credentials = new System.Net.NetworkCredential(data.EmailTo.Trim(), data.SMTPPSWORD.Trim());
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            // Send the e-mail message 
            try
            {
                client.Send(message);
                msg = "Sent Sucessfully";
            }
            catch (Exception ex)
            {
                msg = "Error:";
            }
            finally
            {
                //btnSMTP.Enabled = true;
                //Cursor.Current = Cursors.Default;
            }
            return Json(new { Msg = msg, ID = mstType, validation = status }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult SaveEmail(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            //if (string.IsNullOrEmpty(data.NOTIFYEMAILID.Trim()))
            //{
            //     msg = "Please enter email id";
            //    return Json(new { Msg = msg, ID = mstType, validation = status }, JsonRequestBehavior.AllowGet);
            //}
            //else if (string.IsNullOrEmpty(cmbOfflineItems.Text.Trim()) || cmbOfflineItems.SelectedIndex < 0)
            //{
            //    MessageBox.Show("Please select stage");
            //    cmbOfflineItems.Focus();
            //    return;
            //}
            if (data.NOTIFYMAILTIMING == null)
            {
                data.NOTIFYMAILTIMING = "10";
            }
            if (data.NOTIFYUSERNAME == null)
            {
                data.NOTIFYUSERNAME = "";
            }
            if (data.NOTIFYEMAILID == null)
            {
                data.NOTIFYEMAILID = "";
            }
            if (data.NOTIFYMOBILE == null)
            {
                data.NOTIFYMOBILE = "";
            }
            query = string.Format(@"delete from XXES_STAGE_EMAILS where STAGE = '{0}'", Convert.ToString(data.STAGE));
            fun.EXEC_QUERY(query);
            query = "insert into XXES_STAGE_EMAILS(STAGE,ITEM ,EMAIL,STAGE_DESC,USERNAME,MOBILE,MAILTIMING,TEMPLATEID,MESSAGE) values('" + Convert.ToString(data.STAGE) + "','','" + data.NOTIFYEMAILID.Trim() + "','" + data.STAGE.Trim() + "','" + data.NOTIFYUSERNAME.Trim() + "','" + data.NOTIFYMOBILE.Trim() + "','" + data.NOTIFYMAILTIMING.Trim() + "','" + data.NOTIFYTemplateID.Trim() + "','" + data.NOTIFYMessage.Trim() + "')";
            if (fun.EXEC_QUERY(query))
            {
                fun.Insert_Into_ActivityLog("STAGE_MAIL", "INSERT", Convert.ToString(data.STAGE), query, "", "");
                GridEmails(data);
                msg = "Email Added Sucessfully !!";
            }
            return Json(new { Msg = msg, ID = mstType, validation = status }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult SaveIntegratetable(SftSetting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string query = string.Empty;
            
            if (fun.CheckExits("select count(*) from XXES_SFT_SETTINGS WHERE PARAMETERINFO='PRINT_SERIAL_NUMBER'"))
            {
                query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='PRINT_SERIAL_NUMBER'";
                fun.EXEC_QUERY(query);
            }
            if (data.ChkPRINT_SERIAL_NUMBER == true)
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('PRINT_SERIAL_NUMBER','Y')";
            else
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('PRINT_SERIAL_NUMBER','N')";
            fun.EXEC_QUERY(query);

            if (fun.CheckExits("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='SUB_ASSEMBLY_SERIAL_NUMBER'"))
            {
                query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='SUB_ASSEMBLY_SERIAL_NUMBER'";
                fun.EXEC_QUERY(query);
            }

            if (data.ChkSUB_ASSEMBLY_SERIAL_NUMBER == true)
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('SUB_ASSEMBLY_SERIAL_NUMBER','Y')";
            else
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('SUB_ASSEMBLY_SERIAL_NUMBER','N')";
            fun.EXEC_QUERY(query);
            fun.Insert_Into_ActivityLog("INTEGRATION_TABLE", "INSERT", "SUB_ASSEMBLY_SERIAL_NUMBER", query, "", "");

            if (fun.CheckExits("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='FAMILY_SERIAL'"))
            {
                query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='FAMILY_SERIAL'";
                fun.EXEC_QUERY(query);
            }

            if (data.ChkFAMILY_SERIAL == true)
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('FAMILY_SERIAL','Y')";
            else
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('FAMILY_SERIAL','N')";
            fun.EXEC_QUERY(query);
            fun.Insert_Into_ActivityLog("INTEGRATION_TABLE", "INSERT", "FAMILY_SERIAL", query, "", "");

            if (fun.CheckExits("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='OFF_TYRE_MAKE_CHECK'"))
            {
                query = "delete from XXES_SFT_SETTINGS where PARAMETERINFO='OFF_TYRE_MAKE_CHECK'";
                fun.EXEC_QUERY(query);
            }


            if (data.ChkSwitch_Of_Tyre_Make == true)
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('OFF_TYRE_MAKE_CHECK','Y')";
            else
                query = "Insert into XXES_SFT_SETTINGS(PARAMETERINFO,STATUS) values('OFF_TYRE_MAKE_CHECK','N')";
            if (fun.EXEC_QUERY(query))
            {
                fun.Insert_Into_ActivityLog("TYRE_MAKE_CHECK", "INSERT", "TYRE_MAKE", query, "", "");
                msg = "Integrate Sucessfully !!";
            }
            return Json(new { Msg = msg, ID = mstType, validation = status }, JsonRequestBehavior.AllowGet);

        }
        public PartialViewResult GridEmails(SftSetting data)
        {
            query = string.Format(@"select STAGE_DESC as STAGE, EMAIL as EMAIL, STAGE as CODE, ITEM from XXES_STAGE_EMAILS where STAGE = '{0}'", Convert.ToString(data.STAGE));
            DataTable dt = fun.returnDataTable(query);

            ViewBag.EmailDataSource = dt;
            return PartialView();
        }
        public List<SftSetting> getIntegrateData()
        {
            DataTable dt = new DataTable();
            List<SftSetting> sftSettings = new List<SftSetting>();
            try
            {
                dt = fun.returnDataTable("SELECT PARAMETERINFO,STATUS FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO in ('PRINT_SERIAL_NUMBER', 'SUB_ASSEMBLY_SERIAL_NUMBER', 'FAMILY_SERIAL', 'OFF_TYRE_MAKE_CHECK') AND STATUS='Y'");

                SftSetting SMTPDATA = new SftSetting();
                SMTPDATA.ChkPRINT_SERIAL_NUMBER = false;
                SMTPDATA.ChkSUB_ASSEMBLY_SERIAL_NUMBER = false;
                SMTPDATA.ChkFAMILY_SERIAL = false;
                SMTPDATA.ChkSwitch_Of_Tyre_Make = false;
                if (Convert.ToString(dt.Rows[0]["PARAMETERINFO"]) == "PRINT_SERIAL_NUMBER" && Convert.ToString(dt.Rows[0]["STATUS"]) == "Y")
                {
                    SMTPDATA.ChkPRINT_SERIAL_NUMBER = true;

                }
                if (Convert.ToString(dt.Rows[0]["PARAMETERINFO"]) == "SUB_ASSEMBLY_SERIAL_NUMBER" && Convert.ToString(dt.Rows[0]["STATUS"]) == "Y")
                {
                    SMTPDATA.ChkSUB_ASSEMBLY_SERIAL_NUMBER = true;
                }
                if (Convert.ToString(dt.Rows[0]["PARAMETERINFO"]) == "FAMILY_SERIAL" && Convert.ToString(dt.Rows[0]["STATUS"]) == "Y")
                {
                    SMTPDATA.ChkFAMILY_SERIAL = true;
                }
                if (Convert.ToString(dt.Rows[0]["PARAMETERINFO"]) == "OFF_TYRE_MAKE_CHECK" && Convert.ToString(dt.Rows[0]["STATUS"]) == "Y")
                {
                    SMTPDATA.ChkSwitch_Of_Tyre_Make = true;
                }
                sftSettings.Add(SMTPDATA);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return sftSettings;
        }
        [HttpGet]
        public JsonResult Integratetable()
        {
            List<SftSetting> result = new List<SftSetting>();
            result = getIntegrateData();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}