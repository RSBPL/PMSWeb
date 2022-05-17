using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Assembly
{
    [Authorize]
    public class BackendPartModificationController : Controller
    {
        // GET: BackendPartModification
        OracleCommand cmd;
        OracleDataAdapter DA; 
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
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

        [HttpPost]
        public JsonResult BindBackend(BackendModification backend)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                string Shift = ""; DateTime Plandate = new DateTime();
                string Shiftcode = "", isDayNeedToLess = ""; 
                string  Backend = string.Empty, autoid = string.Empty;
                string data = fun.getshift();
                if (!string.IsNullOrEmpty(data))
                {
                    Shiftcode = data.Split('#')[0].Trim().ToUpper();
                    isDayNeedToLess = data.Split('#')[2].Trim().ToUpper();
                    if (Shiftcode.Trim().ToUpper() == "C" || isDayNeedToLess == "1")
                        Plandate = fun.GetServerDateTime().Date.AddDays(-1);
                    else
                        Plandate = fun.GetServerDateTime().Date;

                }
                DataTable dt = new DataTable();
                query = string.Format(@"select distinct  xt.item_code || ' # ' || xt.item_desc  as text,xt.item_code, xm.plan_id,xt.autoid,
                       xt.item_code || '#' || xt.autoid as itemcode from xxes_daily_plan_master xm,xxes_daily_plan_tran xt where 
                       xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}'
                       and xt.qty > (select count(*) from xxes_backend_status where fcode_id=xt.AUTOID) order by xm.PLAN_ID,xt.AUTOID",
                       backend.Plant.Trim().ToUpper(), backend.Family.Trim().ToUpper(), Plandate.ToString("dd-MMM-yyyy"));
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["ITEMCODE"]),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindJob(string dcode, string FCODE_ID)
        {
            string backendplant = string.Empty, backendfamily = string.Empty, orgid = string.Empty, Backend = string.Empty, autoid = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<DDLTextValue> result = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(dcode))
                {
                    Backend = Convert.ToString(dcode.Split('#')[0]);
                    autoid = Convert.ToString(dcode.Split('#')[1]);
                }
                query = string.Format(@"SELECT M.PLANT_CODE || '#' || M.FAMILY_CODE FROM XXES_ITEM_MASTER  m JOIN XXES_DAILY_PLAN_TRAN t
                ON m.ITEM_CODE = t.FCODE 
                WHERE T.FCODE IS NOT NULL AND T.AUTOID='{0}'", autoid);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    backendplant = line.Split('#')[0].Trim().ToUpper();
                    backendfamily = line.Split('#')[1].Trim().ToUpper();
                }
                if (string.IsNullOrEmpty(backendplant) || string.IsNullOrEmpty(backendfamily))
                {
                    msg = "Plant/Family not found";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (backendplant == "T04")
                {
                    orgid = "149";
                    backendfamily = "BACK END FTD";
                }
                else
                {
                    orgid = "150";
                    backendfamily = "BACK END TD";
                }
                DataTable dt = new DataTable();
                query = string.Format(@"SELECT WIP_ENTITY_NAME TEXT,WIP_ENTITY_NAME || '#' || START_QUANTITY || '#' || NVL(CC,0) AS JOBID FROM 
                (SELECT DISTINCT A.WIP_ENTITY_NAME,START_QUANTITY,SEGMENT1 FROM RELESEDJOBORDER A WHERE  A.SEGMENT1 ='{0}' and A.ORGANIZATION_ID='{1}'  ) 
                X LEFT JOIN (SELECT JOBID ,COUNT(SRNO) AS CC  FROM xxes_print_serials WHERE  DCODE ='{0}' GROUP BY JOBID) Y ON  X.WIP_ENTITY_NAME = Y.JOBID WHERE 
                X.START_QUANTITY != NVL( Y.CC,0)  and SEGMENT1 ='{0}' ORDER BY WIP_ENTITY_NAME", Backend, orgid);
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    foreach(DataRow dr in dt.AsEnumerable())
                    {
                        result.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["TEXT"]),
                            Value = Convert.ToString(dr["JOBID"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public string GetDecode()
        {
            string Backend = string.Empty, autoid = string.Empty;
            string result = string.Empty;
            try
            {
                BindJob(Backend, autoid);
                query = string.Format(@"select t.item_code || '#' || t.item_desc || '#' ||
                        (select nvl(rearaxel,'') || '`' || Nvl(transmission,'') || '`' || nvl(hydraulic,'')
                        from xxes_backend_master b where b.backend=t.item_code and b.plant_code=t.plant_code)
                        from  xxes_daily_plan_tran t where t.autoid='{0}'", autoid);
                autoid = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(autoid) && autoid.Contains("`"))
                {
                    string item = autoid.Split('#')[0].Trim();
                    string realaxel = autoid.Split('#')[2].Split('`')[0].Trim();
                    string trans = autoid.Split('#')[2].Split('`')[1].Trim();
                    string hyd = autoid.Split('#')[2].Split('`')[2].Trim();
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return result;
        }
        [HttpPost]
        public JsonResult Save(BackendModification data)
        {
            bool printStatus = false; string response = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string RearAxle = string.Empty, Transmission = string.Empty, ActualTrans = string.Empty, ActualAxle = string.Empty,
            ActualHydrualic = string.Empty, Backenddesc = string.Empty, runningSrlno = string.Empty, BackendSrlno = string.Empty; 
            try
            
            
            {
                if(string.IsNullOrEmpty(data.RearAxleSrno))
                {
                    msg = "Please Enter RearAxle Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.TransmissionSrno))
                {
                    msg = "Please Enter Transmission Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (!CheckQuantity(data)) { }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  REARAXEL_SRLNO='{0}'",data.RearAxleSrno.Trim().ToUpper());
                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "RearAxle Serial No. Aleardy Exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  TRANSMISSION_SRLNO='{0}'", data.TransmissionSrno.Trim().ToUpper());
                if(Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "Transmission Serial No. Aleardy Exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                query = string.Format(@"select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='{0}'", data.RearAxleSrno.Trim().ToUpper());
                RearAxle = fun.get_Col_Value(query);
                if(string.IsNullOrEmpty(RearAxle))
                {
                    msg = "REARAXEL NOT FOUND";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='{0}'", data.TransmissionSrno.Trim().ToUpper());
                Transmission = fun.get_Col_Value(query);
                if(string.IsNullOrEmpty(Transmission))
                {
                    msg = "TRANSMISSION NOT FOUND";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT TRANSMISSION || '#' || REARAXEL || '#' ||HYDRAULIC|| '#' || BACKEND_DESC from XXES_BACKEND_MASTER where trim(BACKEND)='{0}' 
                        and PLANT_CODE='{1}' and FAMILY_CODE='{2}'", data.Backend.Split('#')[0].Trim(), data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    ActualTrans = line.Split('#')[0].Trim().ToUpper();
                    ActualAxle = line.Split('#')[1].Trim().ToUpper();
                    ActualHydrualic = line.Split('#')[2].Trim().ToUpper();
                    Backenddesc = line.Split('#')[3].Trim().ToUpper();

                    if (string.IsNullOrEmpty(ActualAxle))
                    {
                        msg = "REARAXLE ITEMCODE NOT FOUND IN MES";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else if (RearAxle.Trim().ToUpper() != ActualAxle.Trim().ToUpper())
                    {
                        msg = "REARAXLE MISMATCH ACTUAL REARAXLE : " + ActualAxle;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(ActualTrans))
                    {
                        msg = "TRANSMISSION ITEMCODE NOT FOUND IN MES";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else if (Transmission.Trim().ToUpper() != ActualTrans.Trim().ToUpper())
                    {
                        msg = "TRANSMISSION MISMATCH ACTUAL TRANSMISSION : " + ActualTrans;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    msg = "DCODE NOT FOUND IN PLANT : " + data.Backend;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                getSeries(data.Plant.Trim(), data.Family.Trim(), "BAB", out runningSrlno, out BackendSrlno);
                if(string.IsNullOrEmpty(BackendSrlno))
                {
                    msg = "KINDLY CHECK FAMILY SERIES . UNABLE TO GENERATE BACKEND SERIAL NO";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
                data.runningSrlno = runningSrlno;
                data.BackendSrno = BackendSrlno;
                data.backend_desc = Backenddesc;
                data.Transmission = ActualTrans;
                data.RearAxle = ActualAxle;
                data.Hydraulic = ActualHydrualic;      
                if (UpdateBackend(data))
                {
                    data.Backend = data.Backend.Split('#')[0].Trim();
                    data.JobId = data.JobId.Split('#')[0].Trim();
                    if (PrintBackendFT(data,1))
                    {
                        msg = "Backend Srno generated and printed successfully !!";
                        mstType = "alert-success";
                        status = Validation.str2;
                    }
                    else
                    {
                        msg = "Print File Not Found";
                        mstType = "alert-danger";
                        status = Validation.str2;
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var myResult = new
            {
                Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            //var result = new { Msg = msg, ID = mstType, validation = status };
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public void getSeries(string unit, string family, string stage, out string runningSrlno, out string BackendSrlno)
        
        
        {
            int Start_serial_number, Current_Serial_number, End_serial_Number;
            runningSrlno = "";
            BackendSrlno = "";
            DataTable dt = new DataTable();
            try
            {
                string Prefix = "", toReturn = ""; Current_Serial_number = Start_serial_number = End_serial_Number = 0;
                query = string.Format(@"select Start_serial_number, Current_Serial_number,End_serial_Number,Barcode_prefix from XXES_FAMILY_SERIAL 
                        where plant_code='{0}' and family_code='{1}' and offline_keycode='{2}'", unit.Trim().ToUpper(), family.Trim().ToUpper(), stage.Trim());
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    Prefix = Convert.ToString(dt.Rows[0]["Barcode_prefix"]).Trim();
                    if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() == "")
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim()) + 1;
                    if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() != "")
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim()) + 1;
                    if(Current_Serial_number > Convert.ToInt32(Convert.ToString(dt.Rows[0]["End_serial_Number"]).Trim()))
                    {
                        Current_Serial_number = -99; //series full
                        Prefix = "";
                        throw new Exception("BACKEND SERIAL NOS SERIES REACHED ITS MAXIMUM LEVEL FOR PLANT " + unit + " FAMILY " + family);
                    }
                    toReturn = Convert.ToString(Current_Serial_number);
                    while(toReturn.Trim().Length < Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Length)
                    {
                        toReturn = "0" + toReturn;
                    }
                    runningSrlno = toReturn;
                    BackendSrlno = Prefix.Trim() + toReturn.Trim();
                    if(string.IsNullOrEmpty(runningSrlno))
                    {
                        throw new Exception("UNABLE TO GET BACKEND RUNNING NO. CHECK FAMILY SERIAL");
                    }
                    if(string.IsNullOrEmpty(BackendSrlno))
                    {
                        throw new Exception("UNABLE TO GENERATE BACKEND SERIAL NO. CHECK FAMILY SERIAL");
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
        }

        public bool UpdateBackend(BackendModification data)
        {
            string stage = "BAB";
            bool result = false; string Backend = string.Empty, FCODE_ID = string.Empty, JobId = string.Empty;
            string orgid = fun.getOrgId(data.Plant, data.Family);
            if (!string.IsNullOrEmpty(data.runningSrlno))
            {
                Backend = data.Backend.Split('#')[0].Trim();
                FCODE_ID = data.Backend.Split('#')[1].Trim();
                JobId = data.JobId.Split('#')[0].Trim();
            }

            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using(OracleConnection connection = new OracleConnection(connectionString))
            {
                if(connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();
                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    
                    if (!string.IsNullOrEmpty(data.runningSrlno))
                    {
                       
                        query = string.Format(@"update XXES_FAMILY_SERIAL set Current_Serial_number='{0}',LAST_PRINTED_LABEL_DATE_TI=SYSDATE WHERE
                            plant_code='{1}' and family_code='{2}' and offline_keycode='{3}'", data.runningSrlno.Trim(), data.Plant.Trim(), data.Family.Trim(), stage);
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                        //Insert into integration table
                        #region Integration
                        string IsExists = fun.get_Col_Value(@"select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='PRINT_SERIAL_NUMBER' and STATUS='Y'");
                        if (!string.IsNullOrEmpty(IsExists) && Convert.ToInt16(IsExists) > 0 && !string.IsNullOrEmpty(data.runningSrlno))
                        {

                            query = string.Format(@"select count(*) from PRINT_SERIAL_NUMBER where Plant_CODE='{0}' and  
                            SERIAL_NUMBER='{1}' and ORGANIZATION_ID='150'", data.Plant.Trim(), data.runningSrlno.Trim());
                            if (!fun.CheckExits(query))
                            {
                                query = string.Format(@"insert into PRINT_SERIAL_NUMBER(Plant_CODE,ITEM_CODE,SERIAL_NUMBER,ORGANIZATION_ID,
                            CREATION_DATE,BIG_LABEL_PRINTED,JOB_ID) values('{0}','{1}','{2}','{3}',SYSDATE,-1,'{4}')", data.Plant.Trim().ToUpper(),
                                Backend.Trim().ToUpper(), data.BackendSrno.Trim().ToUpper(), orgid, JobId.Trim());
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                        }
                        #endregion Integration
                        query = string.Format(@"select count(*) from XXES_PRINT_SERIALS where PLANT_CODE='{0}' and  FAMILY_CODE='{1}' and offline_keycode='{2}'
                             and DCODE='{3}' and SRNO='{4}'", data.Plant.Trim(), data.Family.Trim(), stage, Backend.Trim(), data.BackendSrno.Trim());
                        if (!fun.CheckExits(query) && !string.IsNullOrEmpty(data.BackendSrno.Trim()))
                        {
                            query = string.Format(@"insert into XXES_PRINT_SERIALS(PLANT_CODE,FAMILY_CODE,STAGE_ID,DCODE,SRNO,PRINTDATE,OFFLINE_KEYCODE,TYRE_DCODE,RIM_DCODE,MISC1,FCODE,JOBID)
                                values('{0}','{1}','{2}','{3}','{4}', SYSDATE,'{5}','','','','{6}','{7}')", data.Plant.Trim(), data.Family.Trim(), stage, Backend.Trim(),
                                    data.BackendSrno.Trim(), stage, Backend.Trim(), JobId);
                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                        query = string.Format(@"Insert into XXES_SCAN_TIME(PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,STAGE,SCAN_DATE,SCANNED_BY) 
                           values('{0}','{1}','{2}','{3}','{4}',SYSDATE,'{5}')", data.Plant.Trim(), data.Family.Trim(), Backend.Trim(),
                               JobId.Trim(), stage, Convert.ToString(Session["Login_User"]).Trim());
                        command.CommandText = query;
                        command.ExecuteNonQuery();

                        query = string.Format(@"insert into XXES_BACKEND_STATUS(PLANT_CODE,FAMILY_CODE,BACKEND,BACKEND_DESC,TRANSMISSION,TRANSMISSION_SRLNO,REARAXEL, REARAXEL_SRLNO, HYDRAULIC,
                            HYDRAULIC_SRLNO,BACKEND_SRLNO,CREATEDBY,FCODE_ID,CREATEDDATE,JOBID) values('{0}','{1}','{2}','{3}' ,'{4}' ,'{5}' ,'{6}','{7}','{8}','{9}','{10}','{11}','{12}',sysdate,'{13}')",
                                data.Plant.Trim(), data.Family.Trim(), Backend.Trim(), data.backend_desc, data.Transmission, data.TransmissionSrno, data.RearAxle, data.RearAxleSrno, data.Hydraulic,
                                data.HydraulicSrno, data.BackendSrno, Convert.ToString(Session["Login_User"]).Trim(), FCODE_ID, JobId);
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        query = string.Format(@"update XXES_BACKEND_STATUS set TRANSMISSION_SRLNO='{0}' ,REARAXEL_SRLNO='{1}', HYDRAULIC_SRLNO='{2}' where  BACKEND_SRLNO='{3}'"
                               , data.TransmissionSrno, data.RearAxleSrno, data.HydraulicSrno, data.BackendSrno);
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    fun.LogWrite(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
            
            return result;
        }
        public bool CheckQuantity(BackendModification data)
        {
            
            bool result = false;
            try
            {
                string Backend = Convert.ToString(data.Backend.Split('#')[0].Trim());
                string FCODE_ID = Convert.ToString(data.Backend.Split('#')[1].Trim());
                query = string.Format(@"select PLAN_ID from XXES_DAILY_PLAN_TRAN where AUTOID ='{0}'", FCODE_ID);
                string plan_id = fun.get_Col_Value(query);
                if(string.IsNullOrEmpty(plan_id))
                {
                    throw new Exception("Plan Not Found");
                }
                query = string.Format(@"select nvl(sum(qty),0) from xxes_daily_plan_tran  where item_code='{0}' and family_code='{1}' and plant_code='{2}' and plan_id='{3}'",
                         Backend.Trim(), data.Family.Trim(), data.Plant.Trim(), plan_id);
                int PlannedQty = Convert.ToInt32(fun.get_Col_Value(query));
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where BACKEND = '{0}' and FCODE_ID in (select autoid from XXES_DAILY_PLAN_TRAN where PLAN_ID = '{1}'
                             and item_code = '{2}')", Backend.Trim(), plan_id, Backend.Trim());
                int Buckleup = Convert.ToInt32(fun.get_Col_Value(query));
                if (Buckleup > PlannedQty)
                {
                    throw new Exception("Qty Exceeds : " + data.Backend + "\n Planned : " + PlannedQty.ToString());
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return result;
        }

        public bool PrintBackendFT(BackendModification data, int copies)
        {
            bool status = false;
            PrintAssemblyBarcodes af = new PrintAssemblyBarcodes();
            try
            {

                string query = af.ReadFile("BackendFT.prn"), cmd1 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    query = query.Replace("JOBID", data.JobId);
                    query = query.Replace("BSRLNO", data.BackendSrno);
                    query = query.Replace("BACKEND", data.Backend);
                    if (!string.IsNullOrEmpty(data.backend_desc))
                    {
                        if (data.backend_desc.Length > 20)
                        {
                            query = query.Replace("ITEM_NAME1", data.backend_desc.Substring(0, 19));
                            query = query.Replace("ITEM_NAME2", data.backend_desc.Substring(20));
                        }
                        else
                        {
                            query = query.Replace("ITEM_NAME1", data.backend_desc);
                            query = query.Replace("ITEM_NAME2", string.Empty);
                        }
                    }
                    else
                    {
                        query = query.Replace("ITEM_NAME1", data.backend_desc);
                        query = query.Replace("ITEM_NAME2", data.backend_desc);
                    }
                    cmd1 = query;
                    if (PrintBackendLable(cmd1, data))
                        status = true;
                    else
                        status = false;
                }
                else
                {
                    throw new Exception("Print File Not Found");
                }
                return status;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }

        public bool PrintBackendLable(string cmd1, BackendModification data)
        {
            System.Net.Sockets.TcpClient tc;
            try
            {
                query = string.Format(@"select ipaddr || '#' || ipport from xxes_stage_master where 
                        offline_keycode='BAB' and plant_code='{0}' and family_code='{1}'", data.Plant.Trim(),data.Family.Trim());
                string line =  fun.get_Col_Value(query);
                data.IPADDR = line.Split('#')[0].Trim();
                data.IPPORT = line.Split('#')[1].Trim();
                if (string.IsNullOrEmpty(Convert.ToString(data.IPPORT)))
                {
                    throw new Exception("PRINTER PORT NOT FOUND");
                }
                System.Net.Sockets.NetworkStream myStream;
                tc = new System.Net.Sockets.TcpClient();
                tc.Connect(data.IPADDR, Convert.ToInt32(data.IPPORT)); // IP address of the printer
                // convert the string command to bytes
                myStream = tc.GetStream();
                if (myStream.CanWrite)
                {
                    Byte[] myFP = System.Text.Encoding.ASCII.GetBytes(cmd1.Trim());
                    myStream.Write(myFP, 0, myFP.Length);
                    myStream.Flush();
                }
                tc.Close();
                return true;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                if (ex.Message.ToUpper().Contains(("A connection attempt failed").ToUpper()))
                {
                    return false;
                    //throw new Exception("PRINTER NOT CONNECTED " + bACKEND.IPADDR);
                }
                else
                    throw;
            }
            finally { }
        }


        /******************************************RePrint*************************************************/

        [HttpGet]
        public JsonResult BindReprintPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindReprinFamily(string RPlant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(RPlant))
            {
                result = fun.Fill_All_Family(RPlant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetSerialNo(BackendModification data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(data.RBackendSrno))
                {
                    msg = "Please Enter Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT COUNT(*) FROM XXES_BACKEND_STATUS xbs WHERE xbs.PLANT_CODE='{0}' AND xbs.FAMILY_CODE='{1}' 
                       AND xbs.BACKEND_SRLNO='{2}'", data.RPlant.Trim().ToUpper(), data.RFamily.Trim().ToUpper(), data.RBackendSrno.Trim().ToUpper());
                dt = fun.returnDataTable(query);
                if(dt.Rows.Count == 0)
                {
                    msg = "Invalid Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select m.TRANSMISSION || '#' || m.REARAXEL || '#' || m.BACKEND_DESC || '#' || s.BACKEND_SRLNO || '#' || m.BACKEND
                        || '#' || TRANSMISSION_SRLNO || '#'  || REARAXEL_SRLNO || '#' || HYDRAULIC_SRLNO || '#' ||
                        (select fcode from xxes_daily_plan_tran where autoid=s.fcode_id) || '#' || m.HYDRAULIC 
                        from XXES_BACKEND_MASTER m ,XXES_BACKEND_STATUS s where m.backend=s.backend and m.plant_code=s.plant_code
                        and m.family_code=s.family_code  and  trim(BACKEND_SRLNO)='{0}'", data.RBackendSrno.Trim());
                string line = fun.get_Col_Value(query);
                if(!string.IsNullOrEmpty(line))
                {
                    string trans = line.Split('#')[0].Trim().ToUpper();
                    string Rear = line.Split('#')[1].Trim().ToUpper();
                    string BackDesc = line.Split('#')[2].Trim().ToUpper();
                    data.backend_desc = BackDesc;
                    data.RBackendSrno = line.Split('#')[3].Trim().ToUpper();
                    data.BackendSrno = data.RBackendSrno;
                    string BackD = line.Split('#')[4].Trim().ToUpper();
                    data.Backend = BackD;
                    data.RTransmissionSrno = line.Split('#')[5].Trim().ToUpper();
                    data.RRearAxleSrno = line.Split('#')[6].Trim().ToUpper();
                    data.RHydraulicSrno = line.Split('#')[7].Trim().ToUpper();
                    string hydra = line.Split('#')[8].Trim().ToUpper();
                    data.Plant = data.RPlant;
                    data.Family = data.RFamily;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var myResult = new
            {
                Result = data,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Reprint(BackendModification data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.RBackendSrno))
                {
                    msg = "Please Enter Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT JOBID FROM XXES_BACKEND_STATUS  WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' 
                        AND BACKEND_SRLNO='{2}'", data.RPlant.Trim(), data.RFamily.Trim(), data.RBackendSrno.Trim());
                data.JobId = fun.get_Col_Value(query);
                //GetSerialNo(data);
                if (PrintBackendFT(data, 1))
                {
                    msg = "Backend Reprinted !!";
                    mstType = "alert-success";
                    status = Validation.str2;
                }
                else
                {
                    msg = "Matched but not Reprinted successfully";
                    mstType = "alert-danger";
                    status = Validation.str2;
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var myResult = new
            {
                Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            //var result = new { Msg = msg, ID = mstType, validation = status };
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        /******************************************PartModify*************************************************/

        [HttpGet]
        public JsonResult BindModifyPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindModifyFamily(string MPlant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(MPlant))
            {
                result = fun.Fill_All_Family(MPlant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetModifySerialNo(BackendModification data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.MBackendSrno))
                {
                    msg = "Please Enter Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT COUNT(*) FROM XXES_BACKEND_STATUS xbs WHERE xbs.PLANT_CODE='{0}' AND xbs.FAMILY_CODE='{1}' 
                       AND xbs.BACKEND_SRLNO='{2}'", data.MPlant.Trim().ToUpper(), data.MFamily.Trim().ToUpper(), data.MBackendSrno.Trim().ToUpper());
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count == 0)
                {
                    msg = "Invalid Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select m.TRANSMISSION || '#' || m.REARAXEL || '#' || m.BACKEND_DESC || '#' || s.BACKEND_SRLNO || '#' || m.BACKEND
                        || '#' || TRANSMISSION_SRLNO || '#'  || REARAXEL_SRLNO || '#' || HYDRAULIC_SRLNO || '#' ||
                        (select fcode from xxes_daily_plan_tran where autoid=s.fcode_id) || '#' || m.HYDRAULIC 
                        from XXES_BACKEND_MASTER m ,XXES_BACKEND_STATUS s where m.backend=s.backend and m.plant_code=s.plant_code
                        and m.family_code=s.family_code  and  trim(BACKEND_SRLNO)='{0}'", data.MBackendSrno.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    string trans = line.Split('#')[0].Trim().ToUpper();
                    string Rear = line.Split('#')[1].Trim().ToUpper();
                    string BackDesc = line.Split('#')[2].Trim().ToUpper();
                    data.backend_desc = BackDesc;
                    data.MBackendSrno = line.Split('#')[3].Trim().ToUpper();
                    data.BackendSrno = data.MBackendSrno;
                    string BackD = line.Split('#')[4].Trim().ToUpper();
                    data.Backend = BackD;
                    data.MTransmissionSrno = line.Split('#')[5].Trim().ToUpper();
                    data.MRearAxleSrno = line.Split('#')[6].Trim().ToUpper();
                    data.MHydraulicSrno = line.Split('#')[7].Trim().ToUpper();
                    string hydra = line.Split('#')[8].Trim().ToUpper();
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            var myResult = new
            {
                Result = data,
                Msg = msg
            };
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveModify(BackendModification data)
        {
            bool printStatus = false; string response = string.Empty , backendplant = string.Empty , backendfamily = string.Empty, Backend = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty , orgid = string.Empty;
            string RearAxle = string.Empty, Transmission = string.Empty, Hydraulic = string.Empty, ActualTrans = string.Empty, ActualAxle = string.Empty,
            ActualHydrualic = string.Empty, Backenddesc = string.Empty, runningSrlno = string.Empty, BackendSrlno = string.Empty , jobid = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.MBackendSrno))
                {
                    msg = "Please Enter Backend Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.MRearAxleSrno))
                {
                    msg = "Please Enter RearAxle Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.MTransmissionSrno))
                {
                    msg = "Please Enter Transmission Serial No.";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                //if (string.IsNullOrEmpty(data.MHydraulicSrno))
                //{
                //    msg = "Please Enter Hydraulic Serial No.";
                //    mstType = Validation.str1;
                //    status = Validation.str2;
                //    var resul = new { Msg = msg, ID = mstType, validation = status };
                //    return Json(resul, JsonRequestBehavior.AllowGet);
                //}
                query = string.Format(@"SELECT PLANT_CODE || '#' || FAMILY_CODE || '#' || BACKEND FROM
                XXES_BACKEND_STATUS WHERE BACKEND_SRLNO='{0}'", data.MBackendSrno.Trim());
                string line = fun.get_Col_Value(query);
                if (line.Contains('#'))
                {
                    backendplant = line.Split('#')[0].Trim().ToUpper();
                    backendfamily = line.Split('#')[1].Trim().ToUpper();
                    Backend = line.Split('#')[2].Trim().ToUpper();
                }
                if (backendplant == "T04")
                {
                    orgid = "149";
                    backendfamily = "BACK END FTD";
                }
                else
                {
                    orgid = "150";
                    backendfamily = "BACK END TD";
                }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  REARAXEL_SRLNO='{0}' and BACKEND_SRLNO<>'{1}'", data.MRearAxleSrno,data.MBackendSrno);
                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "RearAxle Serial No. Aleardy Exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  TRANSMISSION_SRLNO='{0}'  and BACKEND_SRLNO<>'{1}'", data.MTransmissionSrno,data.MBackendSrno);
                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "Transmission Serial No. Aleardy Exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  HYDRAULIC_SRLNO='{0}'  and BACKEND_SRLNO<>'{1}'", data.MHydraulicSrno, data.MBackendSrno);
                if (Convert.ToInt32(fun.get_Col_Value(query)) > 0)
                {
                    msg = "Hydraulic Serial No. Aleardy Exist..!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='{0}'", data.MRearAxleSrno.Trim().ToUpper());
                RearAxle = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(RearAxle))
                {
                    msg = "REARAXEL NOT FOUND";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='{0}'", data.MTransmissionSrno.Trim().ToUpper());
                Transmission = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(Transmission))
                {
                    msg = "TRANSMISSION NOT FOUND";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if(!string.IsNullOrEmpty(data.MHydraulicSrno))
                {
                    query = string.Format(@"select DCODE from XXES_PRINT_SERIALS where SRNO='{0}'", data.MHydraulicSrno.Trim().ToUpper());
                    Hydraulic = fun.get_Col_Value(query);
                    if(string.IsNullOrEmpty(Hydraulic))
                    {
                        msg = "HYDRAULIC NOT FOUND";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var err = new { Msg = msg, ID = mstType, validation = status };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                query = string.Format(@"select m.TRANSMISSION || '#' || m.REARAXEL || '#' | |m.HYDRAULIC || '#' || m.BACKEND_DESC || '#' ||
                        S.JOBID from XXES_BACKEND_MASTER m , XXES_BACKEND_STATUS s  WHERE trim(m.BACKEND) = '{0}' and m.PLANT_CODE = '{1}' 
                        and m.family_code = '{2}' AND S.BACKEND_SRLNO = '{3}'",Backend, backendplant.Trim().ToUpper(),
                        backendfamily.Trim().ToUpper(),data.MBackendSrno.Trim());
                string line1 = fun.get_Col_Value(query);
                if(!string.IsNullOrEmpty(line1))
                {
                    ActualTrans = line1.Split('#')[0].Trim().ToUpper();
                    ActualAxle = line1.Split('#')[1].Trim().ToUpper();
                    ActualHydrualic = line1.Split('#')[2].Trim().ToUpper();
                    Backenddesc = line1.Split('#')[3].Trim().ToUpper();
                    jobid = line1.Split('#')[4].Trim().ToUpper();

                    if (string.IsNullOrEmpty(ActualAxle))
                    {
                        msg = "REARAXLE ITEMCODE NOT FOUND IN MES";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else if (RearAxle.Trim().ToUpper() != ActualAxle.Trim().ToUpper())
                    {
                        msg = "REARAXLE MISMATCH ACTUAL REARAXLE : " + ActualAxle;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(ActualTrans))
                    {
                        msg = "TRANSMISSION ITEMCODE NOT FOUND IN MES";
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    else if (Transmission.Trim().ToUpper() != ActualTrans.Trim().ToUpper())
                    {
                        msg = "TRANSMISSION MISMATCH ACTUAL TRANSMISSION : " + ActualTrans;
                        mstType = Validation.str1;
                        status = Validation.str2;
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                    if(!string.IsNullOrEmpty(Hydraulic))
                    {
                        if (Hydraulic.Trim().ToUpper() != ActualHydrualic.Trim().ToUpper())
                        {
                            msg = "HYDRAULIC MISMATCH ACTUAL HYDRAULIC : " + ActualHydrualic;
                            mstType = Validation.str1;
                            status = Validation.str2;
                            var resul = new { Msg = msg, ID = mstType, validation = status };
                            return Json(resul, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    msg = "DCODE NOT FOUND IN PLANT : " + Backend;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                data.Plant = backendplant;
                data.Family = backendfamily;
                data.runningSrlno = runningSrlno;               
                data.backend_desc = Backenddesc;
                data.Transmission = ActualTrans;
                data.RearAxle = ActualAxle;
                data.BackendSrno = data.MBackendSrno;
                data.RearAxleSrno = data.MRearAxleSrno;
                data.TransmissionSrno = data.MTransmissionSrno;
                data.HydraulicSrno = data.MHydraulicSrno;
                data.Backend = Backend;
                data.JobId = jobid;
                if (UpdateBackend(data))
                {
                    if (PrintBackendFT(data, 1))
                    {
                        msg = "Updated and printed successfully !!";
                        mstType = "alert-success";
                        status = Validation.str2;
                    }
                    else
                    {
                        msg = "Updated but not printed successfully";
                        mstType = "alert-danger";
                        status = Validation.str2;
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = Validation.str1;
                status = Validation.str2;
            }
            var myResult = new
            {
                Result = data,
                Msg = msg,
                ID = mstType,
                validation = status
            };
            //var result = new { Msg = msg, ID = mstType, validation = status };
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopup(BackendModification data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_STAGE_MASTER xsm WHERE xsm.PLANT_CODE='{0}' AND xsm.FAMILY_CODE='{1}' AND xsm.OFFLINE_KEYCODE='BAB' 
                        AND xsm.AD_PASSWORD='{2}'", data.MPlant.Trim().ToUpper(),data.MFamily.Trim().ToUpper(),data.Password.Trim());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = "alert-success";
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "Invalid Password..!!";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var reult = new { Msg = msg, ID = mstType, validation = status };
                    return Json(reult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

    }
}