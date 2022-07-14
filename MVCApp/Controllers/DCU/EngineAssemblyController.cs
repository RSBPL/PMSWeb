using MVCApp.Common;
using MVCApp.CommonFunction;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MVCApp.Controllers.DCU
{
    public class EngineAssemblyController : ApiController
    {

        string query = string.Empty, JSONObj = string.Empty;
        Function fun = new Function();
        public static string orConnstring = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;

        Assemblyfunctions af = new Assemblyfunctions();
        PrintAssemblyBarcodes assemblyBarcodes = null;

        [HttpPost]
        public string ValidateJobID(COMMONDATA cOMMONDATA)
        {
            string result = string.Empty;
            try
            {
                query = string.Format(@"select count(*) from XXES_JOB_STATUS where JOBID='{0}' 
                                and PLANT_CODE='{1}' and family_code='{2}'", cOMMONDATA.JOB.Trim(), cOMMONDATA.PLANT.Trim(), cOMMONDATA.FAMILY.Trim());
                if (fun.CheckExits(query))
                {
                    result = "OK # VALID JOB";
                }
                else
                {
                    result = "ERROR # INVALID JOB";
                }
            }
            catch (Exception ex)
            {

                result = "ERROR # " + ex.Message;
            }
            return result;
        }
        [HttpPost]
        public string GetFcodesToMake(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.GetFcodes(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: PLAN NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);

            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }
        [HttpPost]
        public string GetJobs(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.BindJobs(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: JOBS NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);

            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                // throw;
            }
            return response;
        }

        [HttpPost]
        public string GetBackendToMake(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.GetBackends(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: PLAN NOT FOUND ";
                else
                    response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;

            }
            return response;
        }

        [HttpPost]
        public string GetBackendJob(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            try
            {
                if (af == null)
                    af = new Assemblyfunctions();
                DataTable dataTable = af.BindBackendJob(cOMMONDATA);
                if (dataTable.Rows.Count == 0)
                    response = "ERROR: JOBS NOT FOUND";
                else
                    response = JsonConvert.SerializeObject(dataTable);
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
                //throw;
            }
            return response;

        }
        [HttpPost]
        public string GetBackendTransRearAxel(COMMONDATA cOMMONDATA)
        {

            try
            {
                query = string.Format(@"select m.item_code || '#' || m.item_description || '#' ||
                    (select nvl(rearaxel,'') || '`' || Nvl(transmission,'') || '`' || nvl(hydraulic,'') from xxes_backend_master b
                    where b.backend=t.item_code AND B.PLANT_CODE=T.PLANT_CODE AND ROWNUM <= 1)
                    from xxes_item_master m, xxes_daily_plan_tran t 
                    where m.item_code=t.fcode AND M.PLANT_CODE=T.PLANT_CODE  and t.fcode is not null and t.autoid='{0}'", cOMMONDATA.JOB);
                return fun.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR :" + ex.Message;
            }
        }
        [HttpPost]
        public HttpResponseMessage UpdateBackend(BACKEND bACKEND)
        {
            string response = string.Empty; bool printStatus = false;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand oracleCommand;
                    oracleCommand = new OracleCommand("UDSP_BACKEND", oracleConnection);
                    oracleConnection.Open();
                    oracleCommand.CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add("B_PLANT", bACKEND.plant.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_FAMILY", bACKEND.family.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_BACKEND", bACKEND.backend.Trim().ToUpper());
                    if (string.IsNullOrEmpty(bACKEND.transSrlno))
                        oracleCommand.Parameters.Add("B_TRANSMISSIONSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("B_TRANSMISSIONSRLNO", bACKEND.transSrlno.Trim().ToUpper());
                    if (string.IsNullOrEmpty(bACKEND.AxelSrlno))
                        oracleCommand.Parameters.Add("B_REARAXELSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("B_REARAXELSRLNO", bACKEND.AxelSrlno.Trim().ToUpper());
                    if (string.IsNullOrEmpty(bACKEND.Hydraulic))
                        oracleCommand.Parameters.Add("B_HYDRAULIC", "");
                    else
                        oracleCommand.Parameters.Add("B_HYDRAULIC", bACKEND.Hydraulic.Trim().ToUpper());
                    if (string.IsNullOrEmpty(bACKEND.HydraulicSrlno))
                        oracleCommand.Parameters.Add("B_HYDRAULICSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("B_HYDRAULICSRLNO", bACKEND.HydraulicSrlno.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_FCODEID", bACKEND.FCODE_ID.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_JOBID", bACKEND.jobid.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_STAGE", bACKEND.StageCode.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_STAGEID", bACKEND.LoginStage.Trim().ToUpper());
                    oracleCommand.Parameters.Add("B_CREATEDBY", bACKEND.CreatedBy.Trim().ToUpper());
                    //oracleCommand.Parameters.Add("B_CREATEDDATE", bACKEND.CreatedDate.Trim().ToUpper());
                    oracleCommand.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    oracleCommand.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    oracleCommand.ExecuteNonQuery();
                    response = Convert.ToString(oracleCommand.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (response.StartsWith("OK"))
                    {
                        if (bACKEND.IsPrintLabel == "0")
                        {
                            if (string.IsNullOrEmpty(bACKEND.IPADDR.Trim()))
                            {
                                response = "OK # Matched... but printer IP address not defined";
                            }
                            else
                            {
                                PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
                                printStatus = printAssemblyBarcodes.PrintBackendFT(response, bACKEND);
                                if (printStatus)
                                {
                                    response = "OK # Matched and printed successfully !! ";
                                }
                                else
                                {
                                    response = "OK # Matched and but not printed successfully !! ";
                                }
                            }
                        }
                        else
                        {
                            response = "OK # Matched but print not enabled";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                response = "ERROR : " + ex.Message;
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateHydraulicBackend(BACKEND bACKEND)
        {
            string response = string.Empty; bool printStatus = false;
            string backendplant = string.Empty, backendfamily = string.Empty, backendsrlno = string.Empty, backend = string.Empty, jobid = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(bACKEND.backend_srlno))
                {
                    response = "Please Scan Backend Srlno";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (string.IsNullOrEmpty(bACKEND.HydraulicSrlno))
                {
                    response = "Please Scan Hydraulic Srlno";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  HYDRAULIC_SRLNO='{0}'",
                    bACKEND.HydraulicSrlno);
                if (fun.CheckExits(query))
                {
                    response = "Hydraulic Already scanned";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (bACKEND.plant == "T04")
                {
                    query = string.Format(@"SELECT PLANT_CODE || '#' || FAMILY_CODE || '#' || BACKEND FROM XXES_BACKEND_STATUS
                        WHERE BACKEND_SRLNO='{0}'", bACKEND.backend_srlno);
                }
                else
                {
                    query = string.Format(@"SELECT count(*) FROM XXES_BACKEND_STATUS
                        WHERE BACKEND_SRLNO='{0}'", bACKEND.backend_srlno);
                    if (fun.CheckExits(query))
                    {
                        response = "Backend Srlno already scanned";
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }

                    query = string.Format(@"SELECT PLANT_CODE || '#' || FAMILY_CODE || '#' || dcode || '#' || JOBID FROM XXES_PRINT_SERIALS
                        WHERE srno='{0}' and PLANT_CODE='{1}' and FAMILY_CODE='{2}' AND OFFLINE_KEYCODE='BAB'", bACKEND.backend_srlno,
                       bACKEND.plant, bACKEND.family);
                }
                string line = fun.get_Col_Value(query);
                if (line.Contains('#'))
                {
                    backendplant = line.Split('#')[0].Trim().ToUpper();
                    backendfamily = line.Split('#')[1].Trim().ToUpper();
                    backend = line.Split('#')[2].Trim().ToUpper();
                    jobid = line.Split('#')[3].Trim().ToUpper();
                }
                if (string.IsNullOrEmpty(backendplant) || string.IsNullOrEmpty(backendfamily) || string.IsNullOrEmpty(backend))
                {
                    response = "Backend Srlno not found";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                query = string.Format(@"select DCODE from XXES_PRINT_SERIALS where SRNO='{0}' and OFFLINE_KEYCODE='HYD'", bACKEND.HydraulicSrlno);
                string HydraulicDcode = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(HydraulicDcode))
                {

                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", bACKEND.StageCode.Trim(), backend,
                        "HYDRAULIC NOT FOUND IN PRINT_SERIALS TABLE.SCANNED SERIAL NO ARE " + bACKEND.HydraulicSrlno + "", bACKEND.plant, bACKEND.family, bACKEND.CreatedBy);
                    response = "Hydraulic Not Found";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                query = string.Format(@"select TRANSMISSION || '#' || REARAXEL || '#' || BACKEND_DESC || '#' || HYDRAULIC 
                        from XXES_BACKEND_MASTER where trim(BACKEND)='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
                        backend, backendplant, backendfamily);
                string DATA = fun.get_Col_Value(query);
                string HYDRAULIC = string.Empty, TRANSMISSION = string.Empty, REARAXEL = string.Empty, BACKEND_DESC = string.Empty;
                if (DATA.Trim().IndexOf('#') != -1)
                {
                    TRANSMISSION = DATA.Split('#')[0].Trim().ToUpper();
                    REARAXEL = DATA.Split('#')[1].Trim().ToUpper();
                    BACKEND_DESC = DATA.Split('#')[2].Trim().ToUpper();
                    HYDRAULIC = DATA.Split('#')[3].Trim().ToUpper();

                    if (string.IsNullOrEmpty(HYDRAULIC))
                    {
                        response = "HYDRAULIC ItemCode Not Found in MES";
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                    if (!HYDRAULIC.ToUpper().Equals(HydraulicDcode.ToUpper()))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", bACKEND.StageCode.Trim(), backend,
                           "MISMATCH HYDRAULIC.SCANNED SERIAL NO ARE " + bACKEND.HydraulicSrlno + "", bACKEND.plant, bACKEND.family, bACKEND.CreatedBy);
                        response = "Hydraulic MisMatch !! Actual" + HYDRAULIC;
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                    if (backendplant == "T05")
                    {
                        query = string.Format(@"SELECT count(*) FROM XXES_BACKEND_STATUS xbs WHERE XBS.PLANT_CODE='{0}'
                                AND XBS.FAMILY_CODE='{1}' and XBS.BACKEND_SRLNO='{2}'", backendplant, backendfamily, bACKEND.backend_srlno);
                        if (!fun.CheckExits(query))
                        {
                            query = string.Format(@"INSERT INTO XXES_BACKEND_STATUS(PLANT_CODE,FAMILY_CODE,BACKEND,BACKEND_DESC,
                                    HYDRAULIC, HYDRAULIC_SRLNO,BACKEND_SRLNO,CREATEDBY,CREATEDDATE,JOBID) VALUES(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',sysdate,'{8}')", backendplant, backendfamily,
                                    backend, BACKEND_DESC, HYDRAULIC, bACKEND.HydraulicSrlno, bACKEND.backend_srlno, bACKEND.CreatedBy, jobid);
                        }

                    }
                    else
                    {
                        query = string.Format(@"update xxes_backend_status set hydraulic='{0}',hydraulic_srlno='{1}' where backend_srlno='{2}'
                        and plant_code='{3}' and family_code='{4}'", HydraulicDcode.ToUpper(), bACKEND.HydraulicSrlno,
                           bACKEND.backend_srlno, backendplant, backendfamily);
                    }
                    if (fun.EXEC_QUERY(query))
                    {
                        if (bACKEND.plant == "T05")
                        {
                            query = string.Format(@"select m.BACKEND_DESC || '#' || s.BACKEND_SRLNO || '#' || m.BACKEND || '#' || s.hydraulic_srlno || '#' || s.JOBID
                                from XXES_BACKEND_MASTER m ,XXES_BACKEND_STATUS s  where m.backend=s.backend and m.plant_code=s.plant_code
                                and m.family_code=s.family_code  and  trim(BACKEND_SRLNO)='{0}'", bACKEND.backend_srlno);
                            line = fun.get_Col_Value(query);
                            if (!string.IsNullOrEmpty(line))
                            {
                                //bACKEND.transmission = line.Split('#')[0].Trim().ToUpper();
                                //bACKEND.Axel = line.Split('#')[1].Trim().ToUpper();
                                bACKEND.backend_desc = line.Split('#')[0].Trim().ToUpper();
                                bACKEND.backend_srlno = line.Split('#')[1].Trim().ToUpper();
                                bACKEND.backend = line.Split('#')[2].Trim().ToUpper();
                                bACKEND.HydraulicSrlno = line.Split('#')[3].Trim().ToUpper();
                                bACKEND.jobid = line.Split('#')[4].Trim().ToUpper();

                                PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
                                printStatus = printAssemblyBarcodes.PrintBackendFT(line, bACKEND);
                                if (printStatus)
                                {
                                    response = "OK # Matched and printed successfully !! ";
                                }
                                else
                                {
                                    response = "OK # Matched and but not printed successfully !! ";
                                }
                            }
                        }
                        response = "OK # Saved Successfully !! ";
                        // print ok sticker
                        new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                    }
                }
                else
                {
                    response = "Backend not found in master";
                    new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                }

            }

            catch (Exception ex)
            {
                fun.LogWrite(ex);
                response = "ERROR :" + ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        //public HttpResponseMessage UpdateHydraulicBackend(BACKEND bACKEND)
        //{
        //    string response = string.Empty;
        //    string backendplant = string.Empty, backendfamily = string.Empty, backendsrlno = string.Empty, backend = string.Empty;
        //    try
        //    {
        //        if(string .IsNullOrEmpty(bACKEND.backend_srlno))
        //        {
        //            response = "Please Scan Backend Srlno";
        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //            };
        //        }
        //        if(string.IsNullOrEmpty(bACKEND.HydraulicSrlno))
        //        {
        //            response = "Please Scan Hydraulic Srlno";
        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //            };
        //        }
        //        query = string.Format(@"select count(*) from XXES_BACKEND_STATUS where  HYDRAULIC_SRLNO='{0}'",
        //            bACKEND.HydraulicSrlno);           
        //        if(fun.CheckExits(query))
        //        {
        //            response = "Hydraulic Already scanned";
        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //            };
        //        }
        //        query = string.Format(@"SELECT PLANT_CODE || '#' || FAMILY_CODE || '#' || BACKEND FROM XXES_BACKEND_STATUS
        //                WHERE BACKEND_SRLNO='{0}'", bACKEND.backend_srlno);
        //        string line = fun.get_Col_Value(query);
        //        if(line.Contains('#'))
        //        {
        //            backendplant = line.Split('#')[0].Trim().ToUpper();
        //            backendfamily = line.Split('#')[1].Trim().ToUpper();
        //            backend = line.Split('#')[2].Trim().ToUpper();
        //        }
        //        if(string.IsNullOrEmpty(backendplant) || string.IsNullOrEmpty(backendfamily) || string.IsNullOrEmpty(backend))
        //        {
        //            response = "Backend Srlno not found";
        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //            };
        //        }
        //        query = string.Format(@"select DCODE from XXES_PRINT_SERIALS where SRNO='{0}'", bACKEND.HydraulicSrlno);
        //        string HydraulicDcode = fun.get_Col_Value(query);
        //        if(string.IsNullOrEmpty(HydraulicDcode))
        //        {

        //           fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", bACKEND.StageCode.Trim(), backend, 
        //               "HYDRAULIC NOT FOUND IN PRINT_SERIALS TABLE.SCANNED SERIAL NO ARE " + bACKEND.HydraulicSrlno + "", bACKEND.plant, bACKEND.family, bACKEND.CreatedBy);
        //            response = "Hydraulic Not Found";
        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //            };
        //        }
        //        query = string.Format(@"select TRANSMISSION || '#' || REARAXEL || '#' || BACKEND_DESC || '#' || HYDRAULIC 
        //                from XXES_BACKEND_MASTER where trim(BACKEND)='{0}' and PLANT_CODE='{1}' and family_code='{2}'", 
        //                backend, backendplant, backendfamily);
        //        string DATA = fun.get_Col_Value(query);
        //        string HYDRAULIC = string.Empty, TRANSMISSION = string.Empty, REARAXEL = string.Empty, BACKEND_DESC = string.Empty;
        //        if (DATA.Trim().IndexOf('#') != -1)
        //        {
        //            TRANSMISSION = DATA.Split('#')[0].Trim().ToUpper();
        //            REARAXEL = DATA.Split('#')[1].Trim().ToUpper();
        //            BACKEND_DESC = DATA.Split('#')[2].Trim().ToUpper();
        //            HYDRAULIC = DATA.Split('#')[3].Trim().ToUpper();

        //            if (string.IsNullOrEmpty(HYDRAULIC))
        //            {
        //                response = "HYDRAULIC ItemCode Not Found in MES";
        //                return new HttpResponseMessage()
        //                {
        //                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //                };
        //            }
        //            if (!HYDRAULIC.ToUpper().Equals(HydraulicDcode.ToUpper()))
        //            {
        //                fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", bACKEND.StageCode.Trim(), backend,
        //                   "MISMATCH HYDRAULIC.SCANNED SERIAL NO ARE " + bACKEND.HydraulicSrlno + "", bACKEND.plant, bACKEND.family, bACKEND.CreatedBy);
        //                response = "Hydraulic MisMatch !! Actual" + HYDRAULIC;
        //                return new HttpResponseMessage()
        //                {
        //                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //                };
        //            }
        //            query = string.Format(@"update xxes_backend_status set hydraulic='{0}',hydraulic_srlno='{1}' where backend_srlno='{2}'
        //                and plant_code='{3}' and family_code='{4}'", HydraulicDcode.ToUpper(), bACKEND.HydraulicSrlno,
        //                    bACKEND.backend_srlno, backendplant, backendfamily);
        //            if (fun.EXEC_QUERY(query))
        //            {
        //                response = "OK # Saved Successfully !! ";
        //                new StringContent(response, System.Text.Encoding.UTF8, "application/json");                      
        //            }
        //        }
        //        else
        //        {
        //            response = "Backend not found in master";
        //            new StringContent(response, System.Text.Encoding.UTF8, "application/json");           
        //        }

        //    }  

        //    catch (Exception ex)
        //    {
        //        fun.LogWrite(ex);
        //        response = "ERROR :" + ex.Message;
        //        new StringContent(response, System.Text.Encoding.UTF8, "application/json");        
        //    }
        //    return new HttpResponseMessage()
        //    {
        //        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
        //    };
        //}
        [HttpPost]
        public string ReprintBackendLabel(BACKEND bACKEND)
        {
            string respone = string.Empty;

            try
            {
                stringExtention.SetPropertiesToDefaultValues(bACKEND);
                if (string.IsNullOrEmpty(bACKEND.backend_srlno))
                {
                    return "Invalid Backend Srlno";
                }
                bool printStatus = false;
                //if (bACKEND.IsPrintLabel != "1")
                //{
                //    return "PRINT OPTION NOT ENABLED !! ";
                //}
                //if (string.IsNullOrEmpty(bACKEND.IPADDR))
                //{
                //    return "IP ADDRESS NOT DEFINED";
                //}
                //if (string.IsNullOrEmpty(bACKEND.IPPORT))
                //{
                //    return "PORT NOT DEFINED";
                //}
                query = string.Format(@"select m.TRANSMISSION || '#' || m.REARAXEL || '#' || m.BACKEND_DESC || '#' || s.BACKEND_SRLNO || '#' || m.BACKEND || '#' || s.hydraulic_srlno || '#' || s.JOBID
                                from XXES_BACKEND_MASTER m ,XXES_BACKEND_STATUS s  where m.backend=s.backend and m.plant_code=s.plant_code
                       and m.family_code=s.family_code  and  trim(BACKEND_SRLNO)='{0}'", bACKEND.backend_srlno);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    bACKEND.transmission = line.Split('#')[0].Trim().ToUpper();
                    bACKEND.Axel = line.Split('#')[1].Trim().ToUpper();
                    bACKEND.backend_desc = line.Split('#')[2].Trim().ToUpper();
                    bACKEND.backend_srlno = line.Split('#')[3].Trim().ToUpper();
                    bACKEND.backend = line.Split('#')[4].Trim().ToUpper();
                    bACKEND.HydraulicSrlno = line.Split('#')[5].Trim().ToUpper();
                    bACKEND.jobid = line.Split('#')[6].Trim().ToUpper();

                    PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
                    printStatus = printAssemblyBarcodes.PrintBackendFT(line, bACKEND);

                    if (printStatus)
                    {
                        respone = "OK # REPRINTED SUCCESSFULLY !! ";
                        return respone;
                    }
                    else
                    {
                        respone = "ERROR IN PRINTING !! ";
                        return respone;
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                respone = "ERROR :" + ex.Message;
            }
            return respone;
        }

        [HttpPost]
        public HttpResponseMessage PowerTractorBuckleUp(PTBuckleup pTBuckleup)
        {
            string response = string.Empty;
            string TractorType = string.Empty, Backend_Srlno = string.Empty, Engine_Srlno = string.Empty, TSN = string.Empty,
                   Filename = string.Empty, Suffix = string.Empty;
            bool isBackEndRequire, isEngineRequire;
            DataTable dataTable = new DataTable();
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_CHECK_PARTS_FOR_PT", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("PLANT", pTBuckleup.PLANT.Trim().ToUpper());
                    comm.Parameters.Add("FAMILY", pTBuckleup.FAMILY.Trim().ToUpper());
                    comm.Parameters.Add("ITEMCODE", pTBuckleup.ITEMCODE.Trim().ToUpper());
                    comm.Parameters.Add("JOB", pTBuckleup.JOB.Trim().ToUpper());
                    if (string.IsNullOrEmpty(pTBuckleup.BACKENDSRLNO))
                        comm.Parameters.Add("BBACKEND_SRLNO", "");
                    else
                        comm.Parameters.Add("BBACKEND_SRLNO", pTBuckleup.BACKENDSRLNO.Trim().ToUpper());
                    if (string.IsNullOrEmpty(pTBuckleup.ENGINESRLNO))
                        comm.Parameters.Add("EENGINE_SRLNO", "");
                    else
                        comm.Parameters.Add("EENGINE_SRLNO", pTBuckleup.ENGINESRLNO.Trim().ToUpper());
                    if (string.IsNullOrEmpty(pTBuckleup.STAGE))
                        comm.Parameters.Add("STAGE", "");
                    else
                        comm.Parameters.Add("STAGE", pTBuckleup.STAGE.Trim().ToUpper());
                    comm.Parameters.Add("SYSUSER", pTBuckleup.SYSUSER.Trim().ToUpper());
                    comm.Parameters.Add("SYSTEMNAME", pTBuckleup.SYSTEMNAME.Trim().ToUpper());
                    comm.Parameters.Add("FFCODE_ID", pTBuckleup.FCODEID.Trim().ToUpper());
                    comm.Parameters.Add("IS_BYPASS", pTBuckleup.BYPASS);
                    if (string.IsNullOrEmpty(pTBuckleup.CREATEDBY))
                        comm.Parameters.Add("LOGINUSER", "");
                    else
                        comm.Parameters.Add("LOGINUSER", pTBuckleup.CREATEDBY.Trim().ToUpper());
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (response.StartsWith("OK"))
                    {


                        string[] line = response.Split('#');
                        TractorType = Convert.ToString(line[1]);
                        Backend_Srlno = Convert.ToString(line[2]);
                        Engine_Srlno = Convert.ToString(line[3]);
                        isBackEndRequire = (Convert.ToString(line[4]) == "Y" ? true : false);
                        isEngineRequire = (Convert.ToString(line[5]) == "Y" ? true : false);
                        pTBuckleup.TSN = Convert.ToString(line[6]).Trim();

                        if (pTBuckleup.IsPrintLabel == "1")
                        {
                            if (string.IsNullOrEmpty(pTBuckleup.IPADDR.Trim()))
                            {
                                response = "OK # Matched... but printer IP address not defined";
                            }
                            else
                            {
                                if (TractorType == "EXPORT")
                                    Filename = "BD17.txt";
                                else
                                    Filename = "BD.txt";
                                if (pTBuckleup.PrintMMYYFormat.Trim() != "1")
                                {
                                    pTBuckleup.SUFFIX = string.Empty;
                                }
                                else
                                {
                                    query = string.Format(@"select prefix_4 from xxes_item_master where item_code='{0}' and plant_code='{1}'",
                                        pTBuckleup.ITEMCODE.Trim().ToUpper(), pTBuckleup.PLANT.Trim().ToUpper());
                                    string prefix4 = fun.get_Col_Value(query);
                                    if (!string.IsNullOrEmpty(prefix4))
                                        pTBuckleup.SUFFIX = prefix4.Trim();
                                }

                                if (af == null)
                                    af = new Assemblyfunctions();
                                pTBuckleup.TRACTOR_DESC = af.getTractordescription(pTBuckleup.PLANT, pTBuckleup.FAMILY, pTBuckleup.ITEMCODE);
                                if (assemblyBarcodes == null)
                                    assemblyBarcodes = new PrintAssemblyBarcodes();
                                bool printStatus = false;
                                string LongDesc = Convert.ToString(ConfigurationManager.AppSettings["PT_LONG_DESC"]);
                                if (LongDesc == "Y")
                                    printStatus = assemblyBarcodes.PrintBackendStickerLongDescription(pTBuckleup, Filename, isEngineRequire, isBackEndRequire, "");
                                else
                                    printStatus = assemblyBarcodes.PrintBackendSticker(pTBuckleup, Filename, isEngineRequire, isBackEndRequire, "");
                                if (printStatus)
                                {
                                    response = "OK # Matched and printed successfully !! ";
                                }
                                else
                                {
                                    response = "OK # Matched and but not printed successfully !! ";
                                }
                            }
                        }
                        else
                            response = "OK # Matched but print not enabled";
                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                response = "ERROR : " + ex.Message;


            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };

        }
        [HttpPost]
        public string UpdateBatteryScanning(BatteryData batteryData)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("dd/MMM/yyyy");
                //DateTime startDate = DateTime.Parse(currentDate.ToString("dd/MM/yyyy"));
                if (string.IsNullOrEmpty(batteryData.JOB))
                {
                    return "ERROR : Please scan job";
                }
                if (string.IsNullOrEmpty(batteryData.SERIALNO))
                {
                    return "ERROR : Please scan serialno";
                }
                if (string.IsNullOrEmpty(batteryData.MAKE))
                {
                    return "ERROR : Please select make";
                }
                string[] arr = batteryData.SERIALNO.Split('$');
                string date = arr[3].Trim().ToUpper();
                date = date.Trim().ToUpper().Replace(".", "/");
                DateTime startDate = DateTime.Parse(date);
                startDate = startDate.AddDays(90);
                startDate = Convert.ToDateTime(startDate.ToString("dd/MMM/yyyy"));
                if (Convert.ToDateTime(currentDate) > startDate)
                {
                    return "ERROR : 90 Days Old battery..!!";
                }

                if (af == null)
                    af = new Assemblyfunctions();
                if (fun == null)
                    fun = new Function();
                string BATTERY_SRLNO = string.Empty;
                query = string.Format(@"select ITEM_CODE,ITEM_DESCRIPTION,BATTERY_SRLNO from XXES_JOB_STATUS 
                where JOBID='{0}' and PLANT_CODE='{1}' and family_code='{2}'", batteryData.JOB, batteryData.PLANT, batteryData.FAMILY);
                DataTable dtMain = new DataTable(); string fcode = string.Empty; string fcode_desc = string.Empty;
                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    fcode = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    if (string.IsNullOrEmpty(fcode))
                    {
                        return "Fcode not found On This Job";
                    }
                    fcode_desc = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    BATTERY_SRLNO = Convert.ToString(dtMain.Rows[0]["BATTERY_SRLNO"]);
                    if (!string.IsNullOrEmpty(BATTERY_SRLNO))
                    {

                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", batteryData.LoginStageCode, batteryData.JOB.Trim(), "BATTERY SERIAL NUMBER ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + batteryData.SERIALNO + "", batteryData.PLANT, batteryData.FAMILY, batteryData.CREATEDBY);

                        return "SerialNo Already Scanned On This Job";
                    }
                    query = string.Format(@"select battery from xxes_item_master where item_code='{0}' and plant_code='{1}'",
                        fcode, batteryData.PLANT.Trim());
                    string batDcode = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(batDcode))
                    {
                        string dummyDcode = fun.get_Col_Value(
                            string.Format(@"select parameterinfo from xxes_sft_settings where status='BATDUMMY' 
                            and plant_code='{0}'", batteryData.PLANT.Trim()));
                        if (!string.IsNullOrEmpty(dummyDcode))
                        {

                            if (dummyDcode != batDcode)
                            {
                                query = string.Format(@"select count(*) from XXES_JOB_STATUS where BATTERY_SRLNO='{0}'", batteryData.SERIALNO.ToUpper());
                                if (fun.CheckExits(query))
                                {
                                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", batteryData.LoginStageCode.Trim(), batteryData.JOB.Trim(), "BATTERY SERIAL NUMBER ALREADY SCANNED. SCANNED SERIAL NO ARE " + batteryData.SERIALNO + "", batteryData.PLANT.Trim(), batteryData.FAMILY.Trim(), batteryData.CREATEDBY);
                                    return "Serial No Already Scanned";
                                }
                            }
                            else if (dummyDcode.Trim().ToUpper() == batDcode.Trim().ToUpper())
                            {
                                query = string.Format(@"select count(*) from xxes_sft_settings where paramvalue='{0}' 
                                and status='BATDUMMYNO' and parameterinfo='{1}'", batteryData.SERIALNO.Trim(), dummyDcode.Trim());
                                //query = "select count(*) from xxes_sft_settings where BATTERY_SRLNO='" + batteryData.SERIALNO.Trim().ToUpper() + "'";
                                if (!fun.CheckExits(query))
                                {
                                    //obj.Insert_Into_ActivityLog("SCAN_ERROR_DCU", BaseClass.LoginStageCode, txtBatJob.Text.Trim(), "INVALID DUMMY SRLNO " + txtBattSrlno.Text.Trim() + "", BaseClass.Login_Unit, BaseClass.LoginFamily);                                   
                                    return "Invalid Dummy Serial No";
                                }
                            }
                        }
                    }
                }
                else
                {

                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", batteryData.LoginStageCode.Trim(), batteryData.JOB.Trim(), "BUCKLEUP NOT DONE FOR SCANNED JOB. SCANNED SERIAL NO ARE " + batteryData.SERIALNO.Trim() + "", batteryData.PLANT.Trim(), batteryData.FAMILY.Trim(), batteryData.CREATEDBY);
                    return "BUCKLEUP NOT DONE FOR SCANNED JOB";
                }
                query = string.Format(@"select count(*) from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' and PARAMETERINFO='{0}'", batteryData.MAKE.Trim().ToUpper());
                if (fun.CheckExits(query))
                {
                    query = "";
                    query = string.Format(@"update XXES_JOB_STATUS set BATTERY_MAKE='{0}',BATTERY_SRLNO='{1}' where JOBID='{2}' and PLANT_CODE='{3}' and family_code='{4}'",
                        batteryData.MAKE.Trim().ToUpper(), batteryData.SERIALNO.Trim().ToUpper(), batteryData.JOB.Trim(), batteryData.PLANT.Trim(), batteryData.FAMILY.Trim());
                    if (fun.EXEC_QUERY(query))
                    {
                        af.InsertIntoScannedStages(batteryData.PLANT.Trim(), batteryData.FAMILY.Trim(), fcode.Trim(), batteryData.JOB.Trim(), batteryData.LoginStageCode.Trim(), batteryData.CREATEDBY.Trim());
                        return "OK # Save Data Sucessfully..";
                    }

                }
                else
                {

                    return "Please select valid make";

                }
                return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }
        [HttpGet]
        public string BindMake()
        {
            try
            {
                query = string.Format(@"select PARAMETERINFO as Text from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' order by PARAMETERINFO");
                DataTable dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    JSONObj = JsonConvert.SerializeObject(dtMain);
                }
            }
            catch (Exception ex)
            {

            }
            return JSONObj;
        }
        static string NullToString(object Value)
        {

            // Value.ToString() allows for Value being DBNull, but will also convert int, double, etc.
            return Value == null ? "" : Value.ToString();

            // If this is not what you want then this form may suit you better, handles 'Null' and DBNull otherwise tries a straight cast
            // which will throw if Value isn't actually a string object.
            //return Value == null || Value == DBNull.Value ? "" : (string)Value;


        }
        [HttpPost]
        public string ReprintTractorLabel(PTBuckleup pTBuckleup)
        {
            try
            {
                stringExtention.SetPropertiesToDefaultValues(pTBuckleup);
                string Suffix = string.Empty, response = string.Empty;
                string query, TractorType, line, Filename,
                     ROPS_SRNO = string.Empty; bool isBackEndRequire = false, isEngineRequire = false;

                line = fun.get_Col_Value(String.Format(@"select ITEM_CODE || '#' || ITEM_DESCRIPTION || '#' ||BACKEND_SRLNO|| '#' ||REARAXEL_SRLNO|| '#' 
                ||ENGINE_SRLNO|| '#' ||FCODE_SRLNO || '#' || FCODE_ID || '#' || JOBID || '#' || ROPS_SRNO || '#' || TO_CHAR(entrydate, 'MM/YY')  
                from XXES_JOB_STATUS where FCODE_SRLNO='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
                pTBuckleup.TSN, pTBuckleup.PLANT, pTBuckleup.FAMILY));
                if (!string.IsNullOrEmpty(line))
                {
                    pTBuckleup.ITEMCODE = line.Split('#')[0].Trim();
                    pTBuckleup.TRACTOR_DESC = line.Split('#')[1].Trim();
                    pTBuckleup.ENGINESRLNO = line.Split('#')[4].Trim();
                    pTBuckleup.BACKENDSRLNO = line.Split('#')[2].Trim();
                    pTBuckleup.TSN = line.Split('#')[5].Trim();
                    pTBuckleup.FCODEID = line.Split('#')[6].Trim();
                    pTBuckleup.JOB = line.Split('#')[7].Trim();
                    ROPS_SRNO = line.Split('#')[8].Trim();
                    string monthcode = line.Split('#')[8].Trim();
                    query = "select count(*) from XXES_ITEM_MASTER where ITEM_CODE='" + pTBuckleup.ITEMCODE + "' and PLANT_CODE='" + pTBuckleup.PLANT.Trim().ToUpper() + "' and family_code='" + pTBuckleup.FAMILY.Trim().ToUpper() + "' and Require_Backend='Y'";
                    isBackEndRequire = fun.CheckExits(query);
                    query = "select count(*) from XXES_ITEM_MASTER where ITEM_CODE='" + pTBuckleup.ITEMCODE + "' and PLANT_CODE='" + pTBuckleup.PLANT.Trim().ToUpper() + "' and family_code='" + pTBuckleup.FAMILY.Trim().ToUpper() + "' and Require_Engine='Y'";
                    isEngineRequire = fun.CheckExits(query);
                    TractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code='" + pTBuckleup.ITEMCODE + "' and autoid='" + pTBuckleup.FCODEID + "' and plant_code='" + pTBuckleup.PLANT.Trim() + "' and family_code='" + pTBuckleup.FAMILY.Trim() + "'");

                    if (TractorType == "EXPORT")
                        Filename = "BD17.txt";
                    else
                        Filename = "BD.txt";

                    if (af == null)
                        af = new Assemblyfunctions();
                    pTBuckleup.TRACTOR_DESC = af.getTractordescription(pTBuckleup.PLANT, pTBuckleup.FAMILY,
                        pTBuckleup.ITEMCODE);

                    if (pTBuckleup.PrintMMYYFormat.Trim() == "1")
                    {
                        string EnMisc = fun.get_Col_Value(@"select to_char(scan_date,'MON-YYYY') scan_date from XXES_SCAN_TIME
                        where jobid='" + pTBuckleup.JOB.Trim() + "' and ITEM_CODE='" + pTBuckleup.ITEMCODE.Trim().ToUpper() + "' and stage='EN' and PLANT_CODE='" + pTBuckleup.PLANT.Trim().ToUpper() + "' and FAMILY_CODE='" + pTBuckleup.FAMILY.Trim().ToUpper() + "' and rownum=1");
                        query = string.Format(@"select prefix_4 from xxes_item_master where item_code='{0}' and plant_code='{1}'",
                                      pTBuckleup.ITEMCODE.Trim().ToUpper(), pTBuckleup.PLANT.Trim().ToUpper());
                        string prefix4 = fun.get_Col_Value(query);
                        if (!string.IsNullOrEmpty(prefix4))
                            pTBuckleup.SUFFIX = prefix4.Trim();
                        else
                            pTBuckleup.SUFFIX = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE where
                                            MON_YYYY='" + EnMisc.ToUpper() + "' and TYPE='QRDOMESTIC' and plant='" + pTBuckleup.PLANT.Trim().ToUpper() + "'");
                    }

                    if (assemblyBarcodes == null)
                        assemblyBarcodes = new PrintAssemblyBarcodes();
                    bool printStatus = false;
                    string LongDesc = Convert.ToString(ConfigurationManager.AppSettings["PT_LONG_DESC"]);
                    if (LongDesc == "Y")
                        printStatus = assemblyBarcodes.PrintBackendStickerLongDescription(pTBuckleup, Filename, isEngineRequire, isBackEndRequire, "");
                    else
                        printStatus = assemblyBarcodes.PrintBackendSticker(pTBuckleup, Filename, isEngineRequire, isBackEndRequire, "");
                    if (printStatus)
                    {
                        response = "OK # TSN " + pTBuckleup.TSN.Trim().ToUpper() + " REPRINTED SUCCESSFULLY !! ";
                    }
                    else
                    {
                        response = "ERROR # SOMETHING WENT WRONG !! ";
                    }


                }
                else
                {

                    return "ERROR : BUCKELUP NOT DONE";

                }


            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR : " + ex.Message;
            }
            return "OK # TSN " + pTBuckleup.TSN.Trim().ToUpper() + " REPRINTED SUCCESSFULLY !! ";
        }
        [HttpPost]
        public string UpdateStrtMotrAltntor(SLTALT _sTALT)
        {
            DataTable dtMain = new DataTable();
            string response = string.Empty;
            string ALTERNATOR_SRLNO = string.Empty, ALTDCODE = string.Empty, STARTER_MOTOR_SRLNO = string.Empty, starter_motor = string.Empty;
            string fcode = string.Empty, fcode_desc = string.Empty;
            try
            {
                query = string.Format(@"select M.ITEM_CODE,M.ITEM_DESCRIPTION,S.ALTERNATOR_SRLNO,M.REQ_ALTERNATOR,M.ALTERNATOR,
                        S.STARTER_MOTOR_SRLNO,M.REQ_STARTER_MOTOR, m.starter_motor from XXES_JOB_STATUS S, XXES_ITEM_MASTER M 
                        where S.JOBID='{0}' and M.ITEM_CODE=S.ITEM_CODE and M.PLANT_CODE=S.PLANT_CODE  and S.PLANT_CODE='{1}' and S.family_code='{2}'",
                        _sTALT.JobID.Trim(), _sTALT.Plant.Trim(), _sTALT.Family.Trim());

                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    fcode = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    fcode_desc = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    ALTDCODE = Convert.ToString(dtMain.Rows[0]["ALTERNATOR"]).ToUpper();
                    bool REQ_ALTERNATOR = false;
                    REQ_ALTERNATOR = (Convert.ToString(dtMain.Rows[0]["REQ_ALTERNATOR"]) == "Y" ? true : false);
                    ALTERNATOR_SRLNO = Convert.ToString(dtMain.Rows[0]["ALTERNATOR_SRLNO"]);
                    starter_motor = Convert.ToString(dtMain.Rows[0]["starter_motor"]).ToUpper();
                    bool REQ_STMOTOR = false;
                    REQ_STMOTOR = (Convert.ToString(dtMain.Rows[0]["REQ_STARTER_MOTOR"]) == "Y" ? true : false);
                    STARTER_MOTOR_SRLNO = Convert.ToString(dtMain.Rows[0]["STARTER_MOTOR_SRLNO"]);
                    if (!string.IsNullOrEmpty(ALTERNATOR_SRLNO))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                            "ALTERNATOR SERIAL NUMBER ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + _sTALT.Alternator.Trim() + "",
                            _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                        return "ERROR : Alternator SerialNo Already Scanned On This Job";
                    }
                    else if (!string.IsNullOrEmpty(STARTER_MOTOR_SRLNO))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                            "STARTER MOTOR SERIAL NUMBER ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + _sTALT.StaterMotor.Trim() + "", _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                        return "ERROR : Starter motor SerialNo Already Scanned On This Job";
                    }
                    if (REQ_ALTERNATOR)
                    {
                        if (string.IsNullOrEmpty(ALTDCODE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(), "ALTERNATOR DCODE NOT FOUND ",
                                _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                            return "ERROR : ALTERNATOR DCODE NOT FOUND  : " + ALTDCODE;
                        }
                        if (!_sTALT.Alternator.Trim().ToUpper().Contains("$"))
                        {
                            return "ERROR : INVALID BARCODE ($)";
                        }
                        if (af == null)
                            af = new Assemblyfunctions();
                        if (af.SplitDcode(_sTALT.Alternator.Trim().ToUpper(), "ALT").Trim().ToUpper() != ALTDCODE.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                                "ALTERNATOR DCODE MISMATCH " + _sTALT.Alternator.Trim() + " WITH DCODE : " + ALTDCODE, _sTALT.Plant, _sTALT.Family,
                                _sTALT.CreatedBy);
                            return "ERROR : MISMATCH !! MASTER DCODE : " + ALTDCODE;
                        }
                    }
                    if (REQ_STMOTOR)
                    {
                        if (string.IsNullOrEmpty(starter_motor))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(), "STARTER MOTOR DCODE NOT FOUND ",
                                _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                            return "ERROR : ALTERNATOR DCODE NOT FOUND  : " + starter_motor;
                        }
                        if (!_sTALT.StaterMotor.Trim().ToUpper().Contains("$"))
                        {
                            return "ERROR : INVALID BARCODE ($)";
                        }
                        if (af == null)
                            af = new Assemblyfunctions();
                        if (af.SplitDcode(_sTALT.StaterMotor.Trim().ToUpper(), "START_MOTOR").Trim().ToUpper() != starter_motor.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                                "STARTER MOTOR DCODE MISMATCH " + _sTALT.StaterMotor.Trim() + " WITH DCODE : " + starter_motor, _sTALT.Plant,
                                _sTALT.Family, _sTALT.CreatedBy);
                            return "ERROR : MISMATCH !! MASTER DCODE : " + starter_motor;
                        }
                    }
                    query = string.Format(@"select count(*) from XXES_JOB_STATUS where ALTERNATOR_SRLNO='{0}'", _sTALT.Alternator.Trim().ToUpper());
                    if (fun.CheckExits(query))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                            "ALTERNATOR SERIAL NUMBER ALREADY SCANNED. SCANNED SERIAL NO ARE " + _sTALT.Alternator.Trim() + "", _sTALT.Plant,
                            _sTALT.Family, _sTALT.CreatedBy);
                        return "ERROR : ALTERNATOR SERIAL NUMBER ALREADY SCANNED";
                    }
                    query = "select count(*) from XXES_JOB_STATUS where STARTER_MOTOR_SRLNO='" + _sTALT.StaterMotor.Trim().ToUpper() + "'";
                    if (fun.CheckExits(query))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                            "STARTER MOTOR SERIAL NUMBER ALREADY SCANNED. SCANNED SERIAL NO ARE " + _sTALT.StaterMotor.Trim() + "",
                            _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                        return "ERROR : STARTER MOTOR SERIAL NUMBER ALREADY SCANNED";
                    }
                    query = "";
                    query = @"update XXES_JOB_STATUS set ALTERNATOR_SRLNO='" + _sTALT.Alternator.Trim().ToUpper() + "'," +
                        "STARTER_MOTOR_SRLNO='" + _sTALT.StaterMotor.Trim().ToUpper() + "' where " +
                        "JOBID='" + _sTALT.JobID.Trim() + "' and PLANT_CODE='" + _sTALT.Plant.Trim() + "' and family_code='" + _sTALT.Family.Trim() + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        return "OK : RECORD SAVE SUCCESSFULLY ";
                    }
                }
                else
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", _sTALT.LoginStageCode, _sTALT.JobID.Trim(),
                        "BUCKLEUP NOT DONE FOR SCANNED JOB. SCANNED SERIAL NO ARE " + _sTALT.JobID.Trim() + "", _sTALT.Plant, _sTALT.Family, _sTALT.CreatedBy);
                    response = "ERROR: BUCKLEUP NOT DONE FOR SCANNED JOB";
                }
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
            }
            return response;
        }

        [HttpPost]
        public string GetStagePassword(COMMONDATA cOMMONDATA)
        {
            try
            {
                query = string.Format(@"select count(*) from xxes_stage_master where ad_password='{0}' and offline_keycode='{1}' and  plant_code='{2}' and family_code='{3}'",
                    cOMMONDATA.REMARKS, cOMMONDATA.DATA, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                if (fun.CheckExits(query))
                {
                    return "OK";
                }
                else
                    return "ERROR : INVALID PASSWORD";
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR : " + ex.Message;
            }
        }
        [HttpPost]
        public string getNextTractorNo(COMMONDATA cOMMONDATA)
        {
            string srlno = string.Empty;
            try
            {
                fun.ServerDate = fun.GetServerDateTime();
                string Current_Serial_number = "", Suffix = "";
                if (af == null)
                    af = new Assemblyfunctions();
                Current_Serial_number = af.getSeries(cOMMONDATA.PLANT,
                    cOMMONDATA.FAMILY, cOMMONDATA.REMARKS);
                if (Current_Serial_number == "#-99")
                {
                    return "ERROR: Tractor Serial No Full";
                }
                Suffix = fun.get_Col_Value(@"select MY_CODE from XXES_SUFFIX_CODE 
            where MON_YYYY='" + fun.ServerDate.Date.ToString("MMM-yyyy").ToUpper() + "' and TYPE='DOMESTIC' and plant='" + cOMMONDATA.PLANT.Trim().ToUpper() + "'");

                srlno = Current_Serial_number.Trim().Replace("#", "") + Suffix.Trim();

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR : " + ex.Message;
            }
            finally { }
            return srlno;
        }
        [HttpPost]
        public string GetBackendEgnine(COMMONDATA cOMMONDATA)
        {
            try
            {
                query = string.Format(@"select BACKEND || '#' || ENGINE || '#' || ITEM_DESCRIPTION || '#' || REQUIRE_BACKEND 
                || '#' || REQUIRE_ENGINE  ||  '#' || SEQ_NOT_REQUIRE from XXES_ITEM_MASTER 
                where ITEM_CODE='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
                cOMMONDATA.ITEMCODE, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                return fun.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }
        [HttpPost]
        public string TyreDecodedetails(Tyres tyres)
        {
            try
            {
                query = string.Format(@"SELECT M.FRONTTYRE ||'#'|| M.RH_FRONTTYRE ||'#'|| M.REARTYRE ||'#'|| M.RH_REARTYRE ||'#'|| m.FRONT_RIM ||'#'|| m.REAR_RIM FROM  XXES_JOB_STATUS xjs ,XXES_ITEM_MASTER m 
                        WHERE M.ITEM_CODE=XJS.ITEM_CODE AND M.PLANT_CODE=XJS.PLANT_CODE AND M.FAMILY_CODE=XJS.FAMILY_CODE 
                        AND XJS.PLANT_CODE='{0}' AND XJS.FAMILY_CODE='{1}' AND XJS.JOBID='{2}'", tyres.PLANT.ToUpper().Trim(), tyres.FAMILY.ToUpper().Trim(), tyres.JOB.Trim());
                return fun.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }
        [HttpPost]
        public string UpdateTyres(Tyres tyres)
        {

            string REAR_RIM, FRONT_RIM, REARRIM_SRLNO1, REARRIM_SRLNO2, FRONTRIM_SRLNO1, FRONTRIM_SRLNO2, RH_FRONTTYRE, RH_REARTYRE,
            LH_REARTYRE, LH_FRONTTYRE, DeCode1, DeCode2, DeCode3, DeCode4, itemcode = string.Empty;
            bool isREQ_RHFT = false, isREQ_RHRT = false, isLHREQUIRE_FRONTTYRE = false, isLHREQUIRE_REARTYRE, isREQ_FRONTRIM, isREQ_REARRIM = false;
            try
            {
                if (string.IsNullOrEmpty(tyres.JOB))
                {
                    return "FOCUS : Please scan job";
                }
                else if (string.IsNullOrEmpty(tyres.LHSERIALNO))
                {
                    return "FOCUS : Please scan tyre serialno1";
                }
                else if (string.IsNullOrEmpty(tyres.RHSERIALNO))
                {
                    return "FOCUS : Please scan tyre serialno2";
                }

                query = string.Format(@"select M.ITEM_CODE, M.ITEM_DESCRIPTION  , M.REQUIRE_FRONTTYRE, M.FRONTTYRE , M.REQ_RHFT, M.RH_FRONTTYRE, 
                               M.REQUIRE_REARTYRE ,M.REARTYRE  , M.REQ_RHRT, M.RH_REARTYRE ,  m.REQ_REARRIM,  m.REAR_RIM,  m.REQ_FRONTRIM, m.FRONT_RIM 
                               from XXES_JOB_STATUS s,XXES_ITEM_MASTER m  where JOBID='{0}' and M.ITEM_CODE=S.ITEM_CODE and M.PLANT_CODE=S.PLANT_CODE
                               and M.FAMILY_CODE=S.FAMILY_CODE AND S.PLANT_CODE='{1}' and S.family_code='{2}'", tyres.JOB.Trim(), tyres.PLANT.ToUpper().Trim(), tyres.FAMILY.ToUpper().Trim());
                DataTable dt = new DataTable();
                dt = fun.returnDataTable(query);

                isLHREQUIRE_FRONTTYRE = (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y" ? true : false);
                LH_FRONTTYRE = Convert.ToString(dt.Rows[0]["FRONTTYRE"]);
                isREQ_RHFT = (Convert.ToString(dt.Rows[0]["REQ_RHFT"]) == "Y" ? true : false);
                RH_FRONTTYRE = Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"]);
                isLHREQUIRE_REARTYRE = (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y" ? true : false);
                LH_REARTYRE = Convert.ToString(dt.Rows[0]["REARTYRE"]);
                isREQ_RHRT = (Convert.ToString(dt.Rows[0]["REQ_RHRT"]) == "Y" ? true : false);
                RH_REARTYRE = Convert.ToString(dt.Rows[0]["RH_REARTYRE"]);
                isREQ_REARRIM = (Convert.ToString(dt.Rows[0]["REQ_REARRIM"]) == "Y" ? true : false);
                REAR_RIM = Convert.ToString(dt.Rows[0]["REAR_RIM"]);
                isREQ_FRONTRIM = (Convert.ToString(dt.Rows[0]["REQ_FRONTRIM"]) == "Y" ? true : false);
                FRONT_RIM = Convert.ToString(dt.Rows[0]["FRONT_RIM"]);
                itemcode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);

                query = string.Format(@"select DCODE from XXES_PRINT_SERIALS XM where SRNO='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}'
                and XM.OFFLINE_KEYCODE='{3}'", tyres.LHSERIALNO.Trim().ToUpper(), tyres.PLANT.Trim().ToUpper(), tyres.FAMILY.Trim().ToUpper(),
                tyres.LOGINSTAGECODE.Trim().ToUpper());
                DeCode1 = fun.get_Col_Value(query);
                query = string.Format(@"select DCODE from XXES_PRINT_SERIALS XM where SRNO='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}'
                and XM.OFFLINE_KEYCODE='{3}'", tyres.RHSERIALNO.Trim().ToUpper(), tyres.PLANT.Trim().ToUpper(), tyres.FAMILY.Trim().ToUpper(),
                tyres.LOGINSTAGECODE.Trim().ToUpper());
                DeCode2 = fun.get_Col_Value(query);
                query = string.Format(@"SELECT FRONT_RIM ||'#'|| REAR_RIM FROM XXES_ITEM_MASTER  WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ITEM_CODE='{2}'",
                      tyres.PLANT.Trim().ToUpper(), tyres.FAMILY.Trim().ToUpper(), itemcode);
                DeCode3 = fun.get_Col_Value(query);

                if (tyres.LOGINSTAGECODE == "FT")
                {
                    if (isREQ_FRONTRIM)
                    {
                        if (string.IsNullOrEmpty(tyres.RIMSERIALLH))
                        {
                            return "FOCUS : Please scan RIM srno1";
                        }
                        else if (string.IsNullOrEmpty(tyres.RIMSERIALRH))
                        {
                            return "FOCUS : Please scan RIM srno2";
                        }
                    }
                    if (!string.IsNullOrEmpty(tyres.LHSERIALNO))
                    {
                        if (string.IsNullOrEmpty(LH_FRONTTYRE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "TYRE DCODE NOT FOUND ",
                            tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : LEFT FRONT TYRE DCODE NOT FOUND  : " + LH_FRONTTYRE;
                        }

                        if (DeCode1.ToUpper().Trim() != LH_FRONTTYRE.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "LEFT FRONT TYRE DCODE MISMATCH " + tyres.LHSERIALNO.Trim() + " WITH DCODE : " + LH_FRONTTYRE, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! LEFT FRONT TYRE DCODE : " + LH_FRONTTYRE;
                        }
                    }
                    if (!string.IsNullOrEmpty(tyres.RHSERIALNO))
                    {

                        if (string.IsNullOrEmpty(RH_FRONTTYRE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "TYRE DCODE NOT FOUND ",
                            tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : RIGHT FRONT TYRE DCODE NOT FOUND  : " + RH_FRONTTYRE;
                        }

                        if (DeCode2.Trim().ToUpper() != RH_FRONTTYRE.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIGHT FRONT TYRE DCODE MISMATCH " + tyres.RHSERIALNO.Trim() + " WITH DCODE : " + RH_FRONTTYRE, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIGHT FRONT TYRE DCODE : " + RH_FRONTTYRE;
                        }
                    }
                    if (!string.IsNullOrEmpty(tyres.RIMSERIALLH) && !string.IsNullOrEmpty(tyres.RIMSERIALRH))
                    {
                        if (string.IsNullOrEmpty(FRONT_RIM))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "RIM DCODE NOT FOUND ",
                            tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : RIM DCODE NOT FOUND  : " + FRONT_RIM;
                        }

                        if (DeCode3.Split('#')[0].Trim().ToUpper() != FRONT_RIM.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIM DCODE MISMATCH " + tyres.RIMSERIALLH.Trim() + " WITH DCODE : " + FRONT_RIM, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIM DCODE : " + FRONT_RIM;
                        }

                        if (DeCode3.Split('#')[0].Trim().ToUpper() != FRONT_RIM.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIM DCODE MISMATCH " + tyres.RIMSERIALRH.Trim() + " WITH DCODE : " + FRONT_RIM, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIM DCODE : " + FRONT_RIM;
                        }
                    }

                }
                else if (tyres.LOGINSTAGECODE == "RT")
                {
                    if (isREQ_REARRIM)
                    {
                        if (string.IsNullOrEmpty(tyres.RIMSERIALLH))
                        {
                            return "FOCUS : Please scan RIM srno1";
                        }
                        else if (string.IsNullOrEmpty(tyres.RIMSERIALRH))
                        {
                            return "FOCUS :Please scan RIM srno2";
                        }

                    }
                    if (!string.IsNullOrEmpty(tyres.LHSERIALNO))
                    {
                        if (string.IsNullOrEmpty(LH_REARTYRE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "TYRE DCODE NOT FOUND ",
                            tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : LEFT REAR TYRE DCODE NOT FOUND  : " + LH_REARTYRE;
                        }

                        if (DeCode1.Trim().ToUpper() != LH_REARTYRE.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "LEFT REAR TYRE DCODE MISMATCH " + tyres.LHSERIALNO.Trim() + " WITH DCODE : " + LH_REARTYRE, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! LEFT REAR TYRE DCODE : " + LH_REARTYRE;
                        }
                    }
                    if (!string.IsNullOrEmpty(tyres.RHSERIALNO))
                    {
                        if (string.IsNullOrEmpty(RH_REARTYRE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "TYRE DCODE NOT FOUND ",
                            tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : RIGHT REAR TYRE DCODE NOT FOUND  : " + RH_REARTYRE;
                        }

                        if (DeCode2.Trim().ToUpper() != RH_REARTYRE.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIGHT REAR TYRE DCODE MISMATCH " + tyres.RHSERIALNO.Trim() + " WITH DCODE : " + RH_REARTYRE, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIGHT REAR TYRE DCODE : " + RH_REARTYRE;
                        }
                    }
                    if (!string.IsNullOrEmpty(tyres.RIMSERIALLH) && !string.IsNullOrEmpty(tyres.RIMSERIALRH))
                    {
                        if (string.IsNullOrEmpty(REAR_RIM))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "RIM DCODE NOT FOUND ",
                           tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "ERROR : RIM DCODE NOT FOUND  : " + REAR_RIM;
                        }

                        if (DeCode3.Split('#')[1].Trim().ToUpper() != REAR_RIM.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIM DCODE MISMATCH " + tyres.RIMSERIALLH.Trim() + " WITH DCODE : " + REAR_RIM, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIM DCODE : " + REAR_RIM;
                        }

                        if (DeCode3.Split('#')[1].Trim().ToUpper() != REAR_RIM.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(),
                            "RIM DCODE MISMATCH " + tyres.RIMSERIALRH.Trim() + " WITH DCODE : " + REAR_RIM, tyres.PLANT, tyres.FAMILY,
                            tyres.CREATEDBY);
                            return "ERROR : MISMATCH !! RIM DCODE : " + REAR_RIM;
                        }
                    }

                }

                if (tyres.LHSERIALNO.Trim().ToUpper() == tyres.RHSERIALNO.Trim().ToUpper())
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "BOTH THE SERIAL NO SHOULD NOT BE THE SAME. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                    return "Both the front tyre LH and serialno should not be the same";
                }
                //if(!string.IsNullOrEmpty(tyres.RIMSERIALLH) && !string.IsNullOrEmpty(tyres.RIMSERIALRH))
                //{
                //    if(tyres.RIMSERIALLH.ToUpper().Trim() == tyres.RIMSERIALRH.ToUpper().Trim())
                //    {
                //        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "BOTH THE SERIAL NO SHOULD NOT BE THE SAME. SCANNED SERIAL NO ARE " + tyres.RIMSERIALLH.Trim().ToUpper() + " And " + tyres.RIMSERIALRH.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                //        return "Both the Rim LH and Rim RH should not be the same";
                //    }
                //}               

                if (tyres.LOGINSTAGECODE == "FT")
                    query = string.Format(@"select count(*) from XXES_JOB_STATUS where FRONTTYRE_SRLNO1='{0}' or FRONTTYRE_SRLNO2='{1}'
                                            or FRONTTYRE_SRLNO1='{1}' or FRONTTYRE_SRLNO2='{0}'", tyres.LHSERIALNO.Trim().ToUpper(), tyres.RHSERIALNO.Trim().ToUpper());
                else if (tyres.LOGINSTAGECODE == "RT")
                {
                    query = string.Format(@"select count(*) from XXES_JOB_STATUS where REARTYRE_SRLNO1='{0}' or REARTYRE_SRLNO2='{1}'
                                            or REARTYRE_SRLNO1='{1}' or REARTYRE_SRLNO2='{0}'", tyres.LHSERIALNO.Trim().ToUpper(), tyres.RHSERIALNO.Trim().ToUpper());
                }
                if (fun.CheckExits(query))
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(), tyres.JOB.Trim(), "ONE OF THE SERIAL NO ALREADY SCANNED. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                    return "Tyre Serial No Already Scanned";
                }
                if (tyres.LOGINSTAGECODE == "FT")
                {
                    query = string.Format(@"select count(*) from XXES_JOB_STATUS where FRONTRIM_SRLNO1='{0}' or FRONTRIM_SRLNO2='{1}'
                                          or FRONTRIM_SRLNO1='{1}' or FRONTRIM_SRLNO2='{0}'", tyres.RIMSERIALLH, tyres.RIMSERIALRH);
                }
                else if (tyres.LOGINSTAGECODE == "RT")
                {
                    query = string.Format(@"select count(*) from XXES_JOB_STATUS where REARRIM_SRLNO1='{0}' or REARRIM_SRLNO2='{1}'
                                          or REARRIM_SRLNO1='{1}' or REARRIM_SRLNO2='{0}'", tyres.RIMSERIALLH, tyres.RIMSERIALRH);
                }
                if (fun.CheckExits(query))
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(), tyres.JOB.Trim(), "ONE OF THE SERIAL NO ALREADY SCANNED. SCANNED SERIAL NO ARE " + tyres.RIMSERIALLH.Trim().ToUpper() + " And " + tyres.RIMSERIALRH.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                    return "RIM Serial No Already Scanned";
                }
                string JOB_REARTYRE_MAKE = string.Empty, JOB_FRONTTYRE_MAKE = string.Empty;
                //query = string.Format(@"select ITEM_CODE,ITEM_DESCRIPTION,REARTYRE_MAKE,FRONTTYRE_MAKE,REARTYRE,REARTYRE_SRLNO1,REARTYRE_SRLNO2,FRONTTYRE,FRONTTYRE_SRLNO1,FRONTTYRE_SRLNO2 from XXES_JOB_STATUS where JOBID='{0}' and PLANT_CODE='{1}' and family_code='{2}'", tyres.JOB.Trim(), tyres.PLANT.Trim(), tyres.FAMILY.Trim());
                query = string.Format(@"SELECT M.ITEM_CODE,M.ITEM_DESCRIPTION,S.REARTYRE_MAKE,S.FRONTTYRE_MAKE,S.REARTYRE,
                          S.REARTYRE_SRLNO1,S.REARTYRE_SRLNO2,S.FRONTTYRE,S.FRONTTYRE_SRLNO1,S.FRONTTYRE_SRLNO2,S.REARRIM_SRLNO1,
                          S.FRONTRIM_SRLNO2,S.REARRIM_SRLNO2,S.REARRIM,S.FRONTRIM, S.FRONTRIM_SRLNO1, S.RH_FRONTTYRE , S.RH_REARTYRE,
                          m.REAR_RIM,m.FRONT_RIM,m.REQ_FRONTRIM,m.REQ_REARRIM , M.REQ_RHFT, M.REQ_RHRT,M.REQUIRE_FRONTTYRE,
                          M.REQUIRE_REARTYRE from XXES_JOB_STATUS s,XXES_ITEM_MASTER m WHERE
                          JOBID='{0}' and M.ITEM_CODE=S.ITEM_CODE and M.PLANT_CODE=S.PLANT_CODE
                          and M.FAMILY_CODE=S.FAMILY_CODE and S.PLANT_CODE='{1}'and S.family_code='{2}'", tyres.JOB.Trim(), tyres.PLANT.Trim(), tyres.FAMILY.Trim());

                DataTable dtMain = new DataTable(); string fcode = "", fcode_desc = "", Misc1 = "", Misc = "";
                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["FRONTTYRE"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(), tyres.JOB.Trim(), "FRONT TYRE ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);

                        return "LH FrontTyre already scanned on this job";
                    }

                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["REARTYRE"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim().ToUpper(), tyres.JOB.Trim(), "REAR TYRE ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                        return "LH RearTyre already scanned on this job";
                    }
                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["FRONTRIM"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim().ToUpper(), tyres.JOB.Trim(), "FRONT RIM ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.RIMSERIALLH.Trim().ToUpper() + " And " + tyres.RIMSERIALRH.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                        return "FrontRim already scanned on this job";
                    }
                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["REARRIM"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim().ToUpper(), tyres.JOB.Trim(), "REAR RIM ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.RIMSERIALLH.Trim().ToUpper() + " And " + tyres.RIMSERIALRH.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                        return "RearRim already scanned on this job";
                    }
                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["RH_FRONTTYRE"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim().ToUpper(), tyres.JOB.Trim(), "FRONT TYRE ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                        return "RH FRONT TYRE already scanned on this job";
                    }
                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT" && !string.IsNullOrEmpty(Convert.ToString(dtMain.Rows[0]["RH_REARTYRE"])))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim().ToUpper(), tyres.JOB.Trim(), "REAR TYRE ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);
                        return "RH REAR TYRE already scanned on this job";
                    }

                    fcode = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    fcode_desc = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    JOB_REARTYRE_MAKE = Convert.ToString(dtMain.Rows[0]["REARTYRE_MAKE"]);
                    JOB_FRONTTYRE_MAKE = Convert.ToString(dtMain.Rows[0]["FRONTTYRE_MAKE"]);

                    isREQ_FRONTRIM = (Convert.ToString(dtMain.Rows[0]["REQ_FRONTRIM"]) == "Y" ? true : false);
                    isREQ_REARRIM = (Convert.ToString(dtMain.Rows[0]["REQ_REARRIM"]) == "Y" ? true : false);
                    REAR_RIM = Convert.ToString(dtMain.Rows[0]["REAR_RIM"]);  //itemmaster
                    FRONT_RIM = Convert.ToString(dtMain.Rows[0]["FRONT_RIM"]);
                    REARRIM_SRLNO1 = Convert.ToString(dtMain.Rows[0]["REARRIM_SRLNO1"]);
                    REARRIM_SRLNO2 = Convert.ToString(dtMain.Rows[0]["REARRIM_SRLNO2"]);
                    FRONTRIM_SRLNO1 = Convert.ToString(dtMain.Rows[0]["FRONTRIM_SRLNO1"]);
                    FRONTRIM_SRLNO2 = Convert.ToString(dtMain.Rows[0]["FRONTRIM_SRLNO2"]);


                }
                else
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE,
                        tyres.JOB.Trim(), "BUCKLEUP NOT DONE FOR SCANNED JOB. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT.Trim(), tyres.FAMILY.Trim(), tyres.CREATEDBY);

                    return "BuckleUp Not Done";
                }
                query = string.Format(@"select DCODE || ' # ' || misc1 from XXES_PRINT_SERIALS XM where SRNO='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}'
                and XM.OFFLINE_KEYCODE='{3}'", tyres.LHSERIALNO.Trim().ToUpper(), tyres.PLANT.Trim().ToUpper(), tyres.FAMILY.Trim().ToUpper(),
                tyres.LOGINSTAGECODE.Trim().ToUpper());
                Misc = fun.get_Col_Value(query);
                query = string.Format(@"select DCODE || ' # ' || misc1 from XXES_PRINT_SERIALS XM where SRNO='{0}' and XM.PLANT_CODE='{1}' and XM.family_code='{2}'
                and XM.OFFLINE_KEYCODE='{3}'", tyres.RHSERIALNO.Trim().ToUpper(), tyres.PLANT.Trim().ToUpper(), tyres.FAMILY.Trim().ToUpper(),
                tyres.LOGINSTAGECODE.Trim().ToUpper());
                Misc1 = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(Misc) && !string.IsNullOrEmpty(Misc1)) //dcode found on the basis of serial no now going to check item master
                {
                    //if (Misc.Trim().Split('#')[0].ToUpper().Trim() != Misc1.Trim().Split('#')[0].ToUpper().Trim())
                    //{

                    //    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "SERIAL NOS SHOULD BE OF SAME ASSEMBLY. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                    //    return "Srnos should be of same assembly";

                    //}
                    if (tyres.OffTyreMakeCheck)
                    {
                        if (Misc.Trim().Split('#')[1].ToUpper().Trim() != Misc1.Trim().Split('#')[1].ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "MAKE SHOULD BE SAME. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                            return "Make should be same";
                        }

                        string tmake = Misc.Trim().Split('#')[1].ToUpper().Trim();
                        query = string.Format("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='FCODE_TYRES' and itemcode='{0}'", fcode);
                        if (fun.CheckExits(query))
                        {
                            query = string.Format("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='FCODE_TYRES' and itemcode='{0}' and tyre='{1}' ",
                               fcode, tmake);
                            if (!fun.CheckExits(query))
                            {
                                fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "MAKE NOT ALLOWED FOR FCODE " + fcode + ". SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                                return "This tyre make not allow for Fcode " + fcode;

                            }
                        }

                        if ((tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT") && (!string.IsNullOrEmpty(JOB_REARTYRE_MAKE.Trim())))
                        {
                            if (JOB_REARTYRE_MAKE.Trim().ToUpper() != Misc.Trim().Split('#')[1].ToUpper().Trim())
                            {
                                fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(), tyres.JOB.Trim(), "FRONT AND REAR TYRE MAKE SHOULD BE SAME. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);

                                return "FRONT Make should Be Same";
                            }
                        }
                        else if ((tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT") && (!string.IsNullOrEmpty(JOB_FRONTTYRE_MAKE.Trim())))
                        {
                            if (JOB_REARTYRE_MAKE.Trim().ToUpper() != Misc.Trim().Split('#')[1].ToUpper().Trim())
                            {
                                fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(), tyres.JOB.Trim(), "FRONT AND REAR TYRE MAKE SHOULD BE SAME. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);

                                return "Rear Make should Be Same";
                            }
                        }
                    }
                    query = "";
                    if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT")
                        query = string.Format(@"select FRONTTYRE from XXES_ITEM_MASTER where item_code='{0}' and plant_code='{1}' and family_code='{2}'", fcode.Trim().ToUpper(), tyres.PLANT, tyres.FAMILY);
                    else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT")
                        query = string.Format(@"select REARTYRE from XXES_ITEM_MASTER where item_code='{0}' and plant_code='{1}' and family_code='{2}'", fcode.Trim().ToUpper(), tyres.PLANT, tyres.FAMILY);
                    if (fun.get_Col_Value(query).Trim().ToUpper().Trim() == Misc.Trim().Split('#')[0].ToUpper().Trim())
                    {
                        query = "";
                        if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "FT")
                            query = string.Format(@"update XXES_JOB_STATUS set FrontTyre='{0}',FrontTyre_Make='{1}' , FrontTyre_SRLNO1='{2}', FrontTyre_SRLNO2='{3}',
                                    FRONTRIM_SRLNO1='{4}', FRONTRIM_SRLNO2='{5}' , FRONTRIM='{6}', RH_FRONTTYRE='{7}' where JOBID='{8}' 
                                    and PLANT_CODE='{9}' and family_code='{10}'", Misc.Trim().Split('#')[0].ToUpper().Trim(), Misc.Trim().Split('#')[1],
                                    tyres.LHSERIALNO.Trim().ToUpper(), tyres.RHSERIALNO.Trim().ToUpper(), tyres.RIMSERIALLH, tyres.RIMSERIALRH,
                                    FRONT_RIM, RH_FRONTTYRE, tyres.JOB.Trim(), tyres.PLANT.Trim(), tyres.FAMILY.Trim());

                        else if (tyres.LOGINSTAGECODE.Trim().ToUpper() == "RT")
                            query = string.Format(@"update XXES_JOB_STATUS set RearTyre='{0}' ,RearTyre_Make='{1}' ,RearTyre_SRLNO1='{2}',RearTyre_SRLNO2='{3}' ,
                                    REARRIM_SRLNO1='{4}' , REARRIM_SRLNO2='{5}' , REARRIM='{6}' , RH_REARTYRE='{7}' where JOBID='{8}' 
                                    and PLANT_CODE='{9}' and family_code='{10}'", Misc.Trim().Split('#')[0].ToUpper().Trim(), Misc.Trim().Split('#')[1],
                                    tyres.LHSERIALNO.Trim().ToUpper(), tyres.RHSERIALNO.Trim().ToUpper(), tyres.RIMSERIALLH, tyres.RIMSERIALRH, REAR_RIM, RH_REARTYRE,
                                    tyres.JOB.Trim(), tyres.PLANT.Trim(), tyres.FAMILY.Trim());

                        if (fun.EXEC_QUERY(query))
                        {
                            if (af == null)
                                af = new Assemblyfunctions();
                            af.InsertIntoScannedStages(tyres.PLANT.Trim(), tyres.FAMILY.Trim(),
                                fcode.Trim(), tyres.JOB.Trim(), tyres.LOGINSTAGECODE.Trim(),
                                tyres.CREATEDBY);
                            return "OK#MATCHED SUCCESSFULLY..";
                        }
                    }
                    else
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE.Trim(),
                            tyres.JOB.Trim(), "MISMATCH TYRES. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper(), tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                        return "MisMatch";
                    }
                }
                else
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", tyres.LOGINSTAGECODE, tyres.JOB.Trim(), "INVALID SERIES NO. SCANNED SERIAL NO ARE " + tyres.LHSERIALNO.Trim().ToUpper() + " And " + tyres.RHSERIALNO.Trim().ToUpper() + "", tyres.PLANT, tyres.FAMILY, tyres.CREATEDBY);
                    return "FOCUS : Invalid SeriesNo";
                }
                return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }

        [HttpPost]
        public string UpdateRadiator(RADIATOR radiator)
        {
            try
            {
                string RADIATOR_SRLNO, RADIATOR = string.Empty;
                DataTable dtMain = new DataTable();
                if (string.IsNullOrEmpty(radiator.JOB))
                {
                    return "ERROR : Please scan job";
                }
                if (string.IsNullOrEmpty(radiator.RADIATORID))
                {
                    return "ERROR : Please scan radiator";
                }
                if (af == null)
                    af = new Assemblyfunctions();
                query = string.Format(@"select M.ITEM_CODE,M.ITEM_DESCRIPTION,S.RADIATOR_SRLNO,M.REQ_RADIATOR,M.RADIATOR from XXES_JOB_STATUS S, XXES_ITEM_MASTER M 
                where S.JOBID='" + radiator.JOB.Trim() + "' and M.ITEM_CODE=S.ITEM_CODE and M.PLANT_CODE=S.PLANT_CODE and M.FAMILY_CODE=S.FAMILY_CODE and S.PLANT_CODE = '" + radiator.PLANT.Trim() + "' and S.family_code = '" + radiator.FAMILY.Trim() + "'");
                dtMain = new DataTable(); string fcode = "", fcode_desc = "";
                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    fcode = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    fcode_desc = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    bool REQ_RADIATOR = false;
                    REQ_RADIATOR = (Convert.ToString(dtMain.Rows[0]["REQ_RADIATOR"]) == "Y" ? true : false);
                    RADIATOR_SRLNO = Convert.ToString(dtMain.Rows[0]["RADIATOR_SRLNO"]);
                    RADIATOR = Convert.ToString(dtMain.Rows[0]["RADIATOR"]);
                    if (!string.IsNullOrEmpty(RADIATOR_SRLNO))
                    {

                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", radiator.LoginStageCode, radiator.JOB, "RADIATOR SERIAL NUMBER ALREADY SCANNED ON THIS JOB. SCANNED SERIAL NO ARE " + radiator.RADIATORID + "", radiator.PLANT, radiator.FAMILY, radiator.CREATEDBY);
                        return "ERROR : Radiator SerialNo" + "\n" + "Already Scanned On This Job";


                    }

                    query = "select count(*) from XXES_JOB_STATUS where RADIATOR_SRLNO='" + radiator.RADIATORID.ToUpper() + "'";
                    if (fun.CheckExits(query))
                    {

                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", radiator.LoginStageCode, radiator.JOB, "RADIATOR SERIAL NUMBER ALREADY SCANNED. SCANNED SERIAL NO ARE " + radiator.RADIATORID + "", radiator.PLANT, radiator.FAMILY, radiator.CREATEDBY);
                        return "ERROR : Radiator Serial No" + "\n" + "Already Scanned";
                    }
                    if (REQ_RADIATOR)
                    {
                        if (string.IsNullOrEmpty(RADIATOR))
                        {

                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", radiator.LoginStageCode, radiator.JOB, "RADIATOR DCODE NOT FOUND", radiator.PLANT, radiator.FAMILY, radiator.CREATEDBY);
                            return "ERROR : RADIATOR DCODE NOT FOUND"; ;

                        }
                        if (af.SplitDcode(radiator.RADIATORID.ToUpper(), "RADIATOR").Trim().ToUpper() != RADIATOR.ToUpper().Trim().ToUpper())
                        {

                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", radiator.LoginStageCode, radiator.JOB, "RADIATOR DCODE MISMATCH " + radiator.RADIATORID + " WITH DCODE : " + RADIATOR, radiator.PLANT, radiator.FAMILY, radiator.CREATEDBY);
                            return "ERROR : MISMATCH !! MASTER DCODE : " + RADIATOR + "A";

                        }
                    }
                    query = "";

                    query = string.Format(@"update XXES_JOB_STATUS set RADIATOR='{4}' , RADIATOR_SRLNO='{0}' where JOBID='{1}' and PLANT_CODE='{2}' and family_code='{3}'",
                        radiator.RADIATORID, radiator.JOB, radiator.PLANT, radiator.FAMILY, RADIATOR);
                    if (Convert.ToBoolean(fun.EXEC_QUERY(query)))
                    {
                        string query = string.Format(@"Insert into XXES_SCAN_TIME(PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,STAGE,SCAN_DATE,SCANNED_BY) values('" +
                         radiator.PLANT.ToUpper() + "','" + radiator.FAMILY.ToUpper() + "','" + fcode.Trim().ToUpper() + "','" + radiator.JOB + "','" +
                         radiator.LoginStageCode.ToUpper().Trim() + "',SYSDATE,'" + radiator.CREATEDBY.ToUpper() + "')");
                        fun.EXEC_QUERY(query);
                        return "OK # RECORD SAVE SUCCESSFULLY";


                    }
                    //return "OK";
                }
                else
                {

                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", radiator.LoginStageCode, radiator.JOB, "BUCKLEUP NOT DONE FOR SCANNED JOB. SCANNED SERIAL NO ARE " + radiator.JOB + "", radiator.PLANT, radiator.FAMILY, radiator.CREATEDBY);
                    return "BUCKLEUP NOT DONE FOR SCANNED JOB";

                }
                return "SOMETHING WENT WRONG !!";
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }
        [HttpPost]
        public HttpResponseMessage UpdateClusterAssembly(CLUSTER cLUSTER)
        {
            string response = string.Empty;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_CLUSTERASSEMBLY", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("C_PLANT", cLUSTER.PLANT.ToUpper().Trim());
                    comm.Parameters.Add("C_FAMILY", cLUSTER.FAMILY.ToUpper().Trim());
                    comm.Parameters.Add("C_JOB", cLUSTER.JOB.Trim());
                    comm.Parameters.Add("C_STAGECODE", cLUSTER.LOGINSTAGECODE.ToUpper().Trim());
                    comm.Parameters.Add("C_CLUSTER", cLUSTER.CLUSTERSRNO.Trim().ToUpper());
                    comm.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK # RECORD SAVE SUCESSFULLY";
                        }
                        else
                        {
                            new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }
        [HttpPost]
        public HttpResponseMessage PowerSteeringCylinder(POWERSTEERING pOWERSTEERING)
        {
            string response = string.Empty;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_POWERSTEERINGCYLINDER", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("P_PLANT", pOWERSTEERING.PLANT.ToUpper().Trim());
                    comm.Parameters.Add("P_FAMILY", pOWERSTEERING.FAMILY.ToUpper().Trim());
                    comm.Parameters.Add("P_JOB", pOWERSTEERING.JOB.ToUpper().Trim());
                    comm.Parameters.Add("P_STAGECODE", pOWERSTEERING.LOGINSTAGECODE.ToUpper().Trim());
                    comm.Parameters.Add("P_STERING_SRLNO", pOWERSTEERING.POWERSTEERINGSRNO);
                    comm.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    comm.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK # RECORD SAVE SUCESSFULLY";
                        }
                        else
                        {
                            new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateSteering(STEERING sTEERING)
        {
            string response = string.Empty;
            string HYD_PUMP_SRLNO, STEERING_MOTOR_SRLNO, STEERING_ASSEMBLY_SRLNO, steering_motor = string.Empty;
            DataTable dtMain = new DataTable();
            try
            {
                query = string.Format(@"select M.ITEM_CODE,M.ITEM_DESCRIPTION,S.HYD_PUMP_SRLNO,S.STEERING_MOTOR_SRLNO,S.STEERING_ASSEMBLY_SRLNO,
                        M.REQ_HYD_PUMP,M.REQ_STEERING_MOTOR,M.REQ_STEERING_ASSEMBLY,M.steering_motor from XXES_JOB_STATUS S, XXES_ITEM_MASTER M
                        where S.JOBID='{0}' and M.ITEM_CODE=S.ITEM_CODE and M.PLANT_CODE=S.PLANT_CODE and M.FAMILY_CODE=S.FAMILY_CODE and S.PLANT_CODE = '{1}'
                        and S.family_code = '{2}'", sTEERING.JOB, sTEERING.PLANT, sTEERING.FAMILY);
                dtMain = new DataTable(); string fcode = "", fcode_desc = "";
                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    fcode = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    fcode_desc = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    steering_motor = Convert.ToString(dtMain.Rows[0]["steering_motor"]);
                    bool isHYD_PUMP_SRLNO = false, isSTEERING_MOTOR_SRLNO = false, isSTEERING_ASSEMBLY_SRLNO = false;
                    isHYD_PUMP_SRLNO = (Convert.ToString(dtMain.Rows[0]["REQ_HYD_PUMP"]) == "Y" ? true : false);
                    isSTEERING_MOTOR_SRLNO = (Convert.ToString(dtMain.Rows[0]["REQ_STEERING_MOTOR"]) == "Y" ? true : false);
                    isSTEERING_ASSEMBLY_SRLNO = (Convert.ToString(dtMain.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y" ? true : false);
                    HYD_PUMP_SRLNO = Convert.ToString(dtMain.Rows[0]["HYD_PUMP_SRLNO"]);
                    STEERING_MOTOR_SRLNO = Convert.ToString(dtMain.Rows[0]["STEERING_MOTOR_SRLNO"]);
                    STEERING_ASSEMBLY_SRLNO = Convert.ToString(dtMain.Rows[0]["STEERING_ASSEMBLY_SRLNO"]);
                    if (isSTEERING_MOTOR_SRLNO)
                    {
                        if (string.IsNullOrEmpty(steering_motor))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", sTEERING.LOGINSTAGECODE, sTEERING.JOB.Trim(), "STEERING MOTOR DCODE NOT FOUND ",
                            sTEERING.PLANT, sTEERING.FAMILY, sTEERING.CREATEDBY);
                            response = "STEERING MOTOR DCODE NOT FOUND  : " + steering_motor;
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        //if (!sTEERING.STEERINGMOTORSRLNO.Trim().ToUpper().Contains("$"))
                        //{
                        //    response = "INVALID BARCODE ($)";
                        //    return new HttpResponseMessage()
                        //    {
                        //        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        //    };
                        //}
                        if (af == null)
                            af = new Assemblyfunctions();
                        if (af.SplitDcode(sTEERING.STEERINGMOTORSRLNO.Trim().ToUpper(), "STEERING").Trim().ToUpper() != steering_motor.ToUpper().Trim())
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", sTEERING.LOGINSTAGECODE, sTEERING.JOB.Trim(),
                            "STEERING MOTOR DCODE MISMATCH " + sTEERING.STEERINGMOTORSRLNO.Trim() + " WITH DCODE : " + steering_motor, sTEERING.PLANT, sTEERING.FAMILY,
                            sTEERING.CREATEDBY);
                            response = "MISMATCH !! STEERING MOTOR DCODE : " + steering_motor;
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }

                }
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand oracleCommand;
                    oracleCommand = new OracleCommand("UDSP_STEERING", oracleConnection);
                    oracleConnection.Open();
                    oracleCommand.CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add("P_PLANT", sTEERING.PLANT.ToUpper().Trim());
                    oracleCommand.Parameters.Add("P_FAMILY", sTEERING.FAMILY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("P_JOB", sTEERING.JOB.ToUpper().Trim());
                    oracleCommand.Parameters.Add("P_STAGECODE", sTEERING.LOGINSTAGECODE.ToUpper().Trim());
                    if (string.IsNullOrEmpty(sTEERING.HYDPUMPSRLNO))
                        oracleCommand.Parameters.Add("P_HYDPUMPSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("P_HYDPUMPSRLNO", sTEERING.HYDPUMPSRLNO.ToUpper().Trim());
                    if (string.IsNullOrEmpty(sTEERING.STEERINGMOTORSRLNO))
                        oracleCommand.Parameters.Add("P_STEERINGMOTORSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("P_STEERINGMOTORSRLNO", sTEERING.STEERINGMOTORSRLNO.ToUpper().Trim());
                    if (string.IsNullOrEmpty(sTEERING.STEERINGASSEMBLYSRLNO))
                        oracleCommand.Parameters.Add("P_STEERINGASSEMBLYSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("P_STEERINGASSEMBLYSRLNO", sTEERING.STEERINGASSEMBLYSRLNO.ToUpper().Trim());
                    oracleCommand.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    oracleCommand.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    oracleCommand.ExecuteNonQuery();
                    response = Convert.ToString(oracleCommand.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK # RECORD SAVE SUCESSFULLY";
                        }
                        else
                        {
                            new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };

        }

        [HttpPost]
        public HttpResponseMessage UpdateHydraulic(HYDRAULIC hYDRAULIC)
        {
            string response = string.Empty;
            string HYDRALUIC_SRLNO, HYDRAULICDCODE, FCODE, FCODE_DESC = string.Empty;
            DataTable dtMain = new DataTable();
            try
            {
                query = string.Format(@"SELECT M.ITEM_CODE,M.ITEM_DESCRIPTION,S.HYDRALUIC_SRLNO, M.REQUIRE_HYD,M.HYDRAULIC  FROM XXES_JOB_STATUS S, XXES_ITEM_MASTER M
                WHERE S.JOBID = '{0}' AND M.ITEM_CODE = S.ITEM_CODE AND M.PLANT_CODE = S.PLANT_CODE AND M.FAMILY_CODE = S.FAMILY_CODE AND
                S.PLANT_CODE = '{1}' AND S.FAMILY_CODE = '{2}' ", hYDRAULIC.JOB, hYDRAULIC.PLANT, hYDRAULIC.FAMILY);
                dtMain = fun.returnDataTable(query);
                if (dtMain.Rows.Count > 0)
                {
                    FCODE = Convert.ToString(dtMain.Rows[0]["ITEM_CODE"]);
                    FCODE_DESC = Convert.ToString(dtMain.Rows[0]["ITEM_DESCRIPTION"]);
                    HYDRAULICDCODE = Convert.ToString(dtMain.Rows[0]["HYDRAULIC"]);
                    bool isREQUIRE_HYD = false;
                    isREQUIRE_HYD = (Convert.ToString(dtMain.Rows[0]["REQUIRE_HYD"]) == "Y" ? true : false);
                    HYDRALUIC_SRLNO = Convert.ToString(dtMain.Rows[0]["HYDRALUIC_SRLNO"]);
                    if (isREQUIRE_HYD)
                    {
                        if (string.IsNullOrEmpty(HYDRAULICDCODE))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", hYDRAULIC.LOGINSTAGECODE, hYDRAULIC.JOB.Trim(),
                                 "HYDRAULIC DCODE NOT FOUND", hYDRAULIC.PLANT, hYDRAULIC.FAMILY, hYDRAULIC.CREATEDBY);
                            response = " HYDRAULIC DCODE NOT FOUND : " + HYDRAULICDCODE;
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }
                    //if(!hYDRAULIC.HYDSRLNO.ToUpper().Trim().Contains("$"))
                    //{
                    //    response = "INVALID BARCODE ($)";
                    //    return new HttpResponseMessage()
                    //    {
                    //        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    //    };
                    //}
                    if (af == null)
                        af = new Assemblyfunctions();
                    if (af.SplitDcode(hYDRAULIC.HYDSRLNO.Trim().ToUpper(), "HYDRAULIC").Trim().ToUpper() != HYDRAULICDCODE.Trim().ToUpper())
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", hYDRAULIC.LOGINSTAGECODE, hYDRAULIC.JOB.Trim(),
                            "HYDRAULIC DCODE MISMATCH" + hYDRAULIC.HYDSRLNO.Trim().ToUpper() + " WITH DCODE :" +
                            HYDRAULICDCODE, hYDRAULIC.PLANT, hYDRAULIC.FAMILY, hYDRAULIC.CREATEDBY);
                        response = "MISMATCH !! HYDRAULIC DCODE :" + HYDRAULICDCODE;
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                }
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand oracleCommand;
                    oracleCommand = new OracleCommand("UDSP_UPDATEHYDRAULIC", oracleConnection);
                    oracleConnection.Open();
                    oracleCommand.CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add("H_PLANT", hYDRAULIC.PLANT.ToUpper().Trim());
                    oracleCommand.Parameters.Add("H_FAMILY", hYDRAULIC.FAMILY.ToUpper().Trim());
                    oracleCommand.Parameters.Add("H_JOB", hYDRAULIC.JOB.ToUpper().Trim());
                    oracleCommand.Parameters.Add("H_STAGECODE", hYDRAULIC.LOGINSTAGECODE.ToUpper().Trim());
                    if (string.IsNullOrEmpty(hYDRAULIC.HYDSRLNO))
                        oracleCommand.Parameters.Add("H_HYDRALUICSRLNO", "");
                    else
                        oracleCommand.Parameters.Add("H_HYDRALUICSRLNO", hYDRAULIC.HYDSRLNO.ToUpper().Trim());
                    oracleCommand.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
                    oracleCommand.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
                    oracleCommand.ExecuteNonQuery();
                    response = Convert.ToString(oracleCommand.Parameters["RETURN_MESSAGE"].Value);
                    oracleConnection.Close();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (response.StartsWith("OK"))
                        {
                            response = "OK # RECORD SAVE SUCCESSUFULLY";
                        }
                        else
                        {
                            new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                        }
                    }
                    else
                    {
                        response = "SOMETHING WENT WRONG FROM DATABASE";
                        new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public IHttpActionResult UpdateCareBtn(CAREBTN _careBtn)
        {
            var model = new CAREBTN();
            string foundJobno, Sno, IMI, Mob = string.Empty;
            try
            {
                query = string.Format(@"select s.ITEM_CODE || '#' || MOBILE || '#' || SIM_SERIAL_NO || '#' || IMEI_NO || '#' || JOBID 
                        from XXES_JOB_STATUS s where s.JOBID='{0}' and s.PLANT_CODE='{1}' and s.family_code='{2}'", _careBtn.JobID.Trim(), _careBtn.Plant.Trim(), _careBtn.Family.Trim());
                string line = fun.get_Col_Value(query);

                if (string.IsNullOrEmpty(line))
                {
                    model.Msg = "HIDE # BUCKLE UP NOT DONE FOR SCANNED JOB";
                    return Json(model);
                }
                model.Modal = line.Split('#')[0];
                model.Mobile = line.Split('#')[1];
                model.Srno = line.Split('#')[2];
                model.IMEI = line.Split('#')[3];
                if (!string.IsNullOrEmpty(model.Modal))
                {
                    model.Modal = line.Split('#')[0].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.Mobile))
                {
                    model.Mobile = line.Split('#')[1].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.Srno))
                {
                    model.Srno = line.Split('#')[2].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.IMEI))
                {
                    model.IMEI = line.Split('#')[3].Trim().ToUpper();
                }


                if (!string.IsNullOrEmpty(line.Split('#')[1].Trim().ToUpper()) && line.Split('#')[1].Trim().ToUpper() != "NA")
                {
                    model.Msg = "FOCUS # QR ALREADY SCANNED";
                    return Json(model);
                }

                foundJobno = line.Split('#')[4].Trim().ToUpper();

                query = string.Format(@"select count(*) from xxes_Item_master m where PLANT_CODE='{0}' 
                            and family_code='{1}' and item_code='{2}' and NVL(REQ_CAREBTN,'')='Y'",
                             _careBtn.Plant.Trim(), _careBtn.Family.Trim(), model.Modal);
                if (!fun.CheckExits(query))
                {
                    model.Msg = "FOCUS # CARE BUTTON NOT ALLOWED";
                    return Json(model);
                }

                if (_careBtn.QRCode.Trim().Split(',').Length - 1 != 2 || _careBtn.QRCode.IndexOf(',') < 0)
                {
                    model.Msg = "FOCUS # INVALID QR CODE";
                    return Json(model);
                }
                else if (_careBtn.QRCode.Trim().Split(',')[0].Trim().Length != 16)
                {
                    model.Msg = "FOCUS # INVALID SR NO IN QR CODE";
                    return Json(model);
                }
                else if (_careBtn.QRCode.Trim().Split(',')[1].Trim().Length != 15)
                {
                    model.Msg = "FOCUS # INVALID IMEI NO. IN QR CODE";
                    return Json(model);
                }
                else if (_careBtn.QRCode.Trim().Split(',')[2].Trim().Length != 13 && _careBtn.QRCode.Trim().Split(',')[2].Trim().Length != 10)
                {
                    model.Msg = "FOCUS # INVALID MOBILE NO. LENGTH SHOULD BE 13 CHARS IN QR CODE";
                    return Json(model);
                }
                Sno = _careBtn.QRCode.Trim().Split(',')[0].Trim();
                IMI = _careBtn.QRCode.Trim().Split(',')[1].Trim();
                Mob = _careBtn.QRCode.Trim().Split(',')[2].Trim();

                query = string.Format(@"select FCODE_SRLNO || '#' || JOBID from xxes_job_status 
                where SIM_SERIAL_NO='{0}' or IMEI_NO='{1}' or MOBILE='{2}'", Sno, IMI, Mob);

                string TSN = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(TSN))
                {
                    string foundtsno = TSN.Split('#')[0].Trim();
                    model.Msg = "FOCUS # IMEI/SRNO/MOBILE ALREADY USED IN QR CODE. TSN " + foundtsno;
                    return Json(model);
                }
                else
                {
                    query = string.Format(@"update xxes_job_status set SIM_SERIAL_NO='{0}',IMEI_NO='{1}',MOBILE='{2}' 
                        where jobid='{3}' and plant_code='{4}' and family_code='{5}'",
                        Sno, IMI, Mob, _careBtn.JobID.Trim(), _careBtn.Plant.Trim(), _careBtn.Family.Trim());
                    if (fun.EXEC_QUERY(query))
                    {
                        model.Msg = "OK # RECORD SAVE SUCCESSFULLY";
                        model.Srno = Sno;
                        model.IMEI = IMI;
                        model.Mobile = Mob;
                        return Json(model);
                    }
                    else
                    {
                        model.Msg = "ERROR # SOMETHING ERROR";
                        model.Srno = null;
                        model.IMEI = null;
                        model.Mobile = null;
                        model.Modal = null;
                        return Json(model);
                    }
                }
            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Srno = null;
                model.IMEI = null;
                model.Mobile = null;
                model.Modal = null;
                return Json(model);
            }
            return Json(model);
        }


        [HttpPost]
        public IHttpActionResult UpdatePDIOIL(PDIOIL _careBtn)
        {
            var model = new PDIOIL();
            try
            {
                query = string.Format(@"select item_code || '#' || item_description from xxes_job_status
                    where fcode_srlno='{0}'", _careBtn.TractorSrNo.Trim().ToUpper());
                string line = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(line))
                {
                    model.Msg = "HIDE # INVALID TRACTOR SRL NO";
                    model.CraneCode = null;
                    model.CraneName = null;
                    return Json(model);
                }

                string crnCode = line.Split('#')[0];
                string crnName = line.Split('#')[1];

                if (!string.IsNullOrEmpty(crnCode))
                {
                    model.CraneCode = crnCode.Trim();
                }
                if (!string.IsNullOrEmpty(crnName))
                {
                    model.CraneName = crnName.Trim();
                }

                double qty;
                bool isDouble = TryParseDouble(_careBtn.OilValue, out qty);
                if (!isDouble)
                {
                    model.Msg = "FOCUS # INVALID OIL VALUE";
                    return Json(model);
                }

                query = string.Format(@"update xxes_job_status set oil='{0}'
                        where fcode_srlno='{1}'", _careBtn.OilValue.Trim(), _careBtn.TractorSrNo.Trim().ToUpper());
                if (fun.EXEC_QUERY(query))
                {
                    model.Msg = "OK # OIL QUANTITY SAVED SUCCESSFULLY";
                    model.CraneCode = crnCode.Trim();
                    model.CraneName = crnName.Trim();
                    return Json(model);
                }
                else
                {
                    model.Msg = "ERROR # SOMETHING ERROR";
                    model.CraneCode = null;
                    model.CraneName = null;
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.CraneCode = null;
                model.CraneName = null;
                return Json(model);
            }
            return Json(model);
        }
        private bool TryParseDouble(string source, out double result)
        {
            result = 0;
            try
            {
                result = double.Parse(source);
            }
            catch (Exception)
            { return false; }
            return true;
        }

        [HttpPost]
        public IHttpActionResult UpdatePDIOILCareBtn(PDIOIL _pdioil)
        {
            var model = new PDIOIL();
            string foundJobno, Sno, IMI, Mob = string.Empty;
            try
            {
                query = string.Format(@"select s.ITEM_CODE || '#' || MOBILE || '#' || SIM_SERIAL_NO || '#' || IMEI_NO || '#' || JOBID || '#' || S.CAREBUTTONOIL || '#' || S.FINAL_LABEL_DATE 
                        from XXES_JOB_STATUS s where s.FCODE_SRLNO='{0}' and s.PLANT_CODE='{1}' and s.family_code='{2}'", _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());

                string line = fun.get_Col_Value(query);

                if (string.IsNullOrEmpty(line))
                {
                    model.Msg = "HIDE # BUCKLE UP NOT DONE FOR SCANNED JOB";
                    return Json(model);
                }
                model.Modal = line.Split('#')[0];
                model.Mobile = line.Split('#')[1];
                model.Srno = line.Split('#')[2];
                model.IMEI = line.Split('#')[3];
                model.CareButtonOil = line.Split('#')[5];
                model.FinalLableDate = line.Split('#')[6];
                if (!string.IsNullOrEmpty(model.Modal))
                {
                    model.Modal = line.Split('#')[0].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.Mobile))
                {
                    model.Mobile = line.Split('#')[1].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.Srno))
                {
                    model.Srno = line.Split('#')[2].Trim().ToUpper();
                }
                if (!string.IsNullOrEmpty(model.IMEI))
                {
                    model.IMEI = line.Split('#')[3].Trim().ToUpper();
                }
                if (string.IsNullOrEmpty(model.FinalLableDate))
                {
                    model.Msg = "FOCUS # ROLLOUT NOT DONE";
                    return Json(model);
                }


                foundJobno = line.Split('#')[4].Trim().ToUpper();

                query = string.Format(@"select count(*) from xxes_Item_master m where PLANT_CODE='{0}' 
                            and family_code='{1}' and item_code='{2}' and NVL(REQ_CAREBTN,'')='Y'",
                             _pdioil.Plant.Trim(), _pdioil.Family.Trim(), model.Modal);
                if (!fun.CheckExits(query))
                {
                    model.Msg = "HIDE # CARE BUTTON NOT ALLOWED";
                    return Json(model);
                }

                if (_pdioil.QRCode.Trim().Split(',').Length - 1 != 2 || _pdioil.QRCode.IndexOf(',') < 0)
                {
                    model.Msg = "FOCUS # INVALID QR CODE";
                    return Json(model);
                }
                else if (_pdioil.QRCode.Trim().Split(',')[0].Trim().Length != 16)
                {
                    model.Msg = "FOCUS # INVALID SR NO IN QR CODE";
                    return Json(model);
                }
                else if (_pdioil.QRCode.Trim().Split(',')[1].Trim().Length != 15)
                {
                    model.Msg = "FOCUS # INVALID IMEI NO. IN QR CODE";
                    return Json(model);
                }
                else if (_pdioil.QRCode.Trim().Split(',')[2].Trim().Length != 13 && _pdioil.QRCode.Trim().Split(',')[2].Trim().Length != 10)
                {
                    model.Msg = "FOCUS # INVALID MOBILE NO. LENGTH SHOULD BE 13 CHARS IN QR CODE";
                    return Json(model);
                }
                Sno = _pdioil.QRCode.Trim().Split(',')[0].Trim();
                IMI = _pdioil.QRCode.Trim().Split(',')[1].Trim();
                Mob = _pdioil.QRCode.Trim().Split(',')[2].Trim();

                query = string.Format(@"select FCODE_SRLNO || '#' || JOBID from xxes_job_status 
                where SIM_SERIAL_NO='{0}' or IMEI_NO='{1}' or MOBILE='{2}'", Sno, IMI, Mob);

                string TSN = fun.get_Col_Value(query);

                if (!string.IsNullOrEmpty(TSN))
                {
                    string foundtsno = TSN.Split('#')[0].Trim();
                    if (string.IsNullOrEmpty(foundJobno))
                    {
                        if (string.IsNullOrEmpty(model.CareButtonOil))
                        {
                            query = string.Format(@"update xxes_job_status set CAREBUTTONOIL=sysdate 
                            where FCODE_SRLNO='{0}' and plant_code='{1}' and family_code='{2}'",
                            _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                        }
                        if (fun.EXEC_QUERY(query))
                        {
                            model.Msg = "FOCUS # JOB NOT FOUND FOR TSN " + _pdioil.TractorSrNo.Trim();
                            return Json(model);
                        }

                    }
                }
                query = string.Format(@"select COUNT(*) from xxes_job_status 
                where SIM_SERIAL_NO='{0}' AND IMEI_NO='{1}' AND MOBILE='{2}'", Sno, IMI, Mob);
                string IsSIM = fun.get_Col_Value(query);

                if (Convert.ToInt32(IsSIM) > 0)
                {
                    if (string.IsNullOrEmpty(model.CareButtonOil))
                    {
                        query = string.Format(@"update xxes_job_status set CAREBUTTONOIL=sysdate 
                            where FCODE_SRLNO='{0}' and plant_code='{1}' and family_code='{2}'",
                          _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                    }
                    //query = string.Format(@"update xxes_job_status set CAREBUTTONOIL=sysdate 
                    //        where FCODE_SRLNO='{0}' and plant_code='{1}' and family_code='{2}'",
                    //      _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());

                    if (fun.EXEC_QUERY(query))
                    {

                        model.Msg = "FOCUS # SAME CARE BUTTON FOUND FOR TSN " + _pdioil.TractorSrNo.Trim();
                        return Json(model);
                    }
                }

                string IsRecordExist = string.Format(@"select s.ITEM_CODE || '#' || MOBILE || '#' || SIM_SERIAL_NO || '#' || IMEI_NO || '#' || JOBID 
                        from XXES_JOB_STATUS s where s.FCODE_SRLNO='{0}' and s.PLANT_CODE='{1}' and s.family_code='{2}'", _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());

                string IsSrExist = fun.get_Col_Value(IsRecordExist);

                string scannedMobile = IsSrExist.Split('#')[1];
                string scannedSimSerial = IsSrExist.Split('#')[2];
                string scannedImei = IsSrExist.Split('#')[3];
                if (string.IsNullOrEmpty(scannedMobile) && string.IsNullOrEmpty(scannedSimSerial) && string.IsNullOrEmpty(scannedImei))
                {
                    if (string.IsNullOrEmpty(model.CareButtonOil))
                    {
                        query = string.Format(@"UPDATE XXES_JOB_STATUS SET CAREBUTTONOIL=SYSDATE,SIM_SERIAL_NO='{0}',IMEI_NO='{1}',MOBILE='{2}' 
                            WHERE FCODE_SRLNO='{3}' AND PLANT_CODE='{4}' AND FAMILY_CODE='{5}'", Sno, IMI, Mob, _pdioil.TractorSrNo.Trim(),
                                _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                    }
                    else
                    {
                        query = string.Format(@"UPDATE XXES_JOB_STATUS SET SIM_SERIAL_NO='{0}',IMEI_NO='{1}',MOBILE='{2}' WHERE FCODE_SRLNO='{3}' 
                               AND PLANT_CODE='{4}' AND FAMILY_CODE='{5}'", Sno, IMI, Mob, _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                    }
                    if (fun.EXEC_QUERY(query))
                    {
                        query = string.Format(@"update xxes_job_status set SWAPCAREBTN='{0}' 
                        where FCODE_SRLNO='{1}' and plant_code='{2}' and family_code='{3}'", 'Y',
                        _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                        if (fun.EXEC_QUERY(query))
                        {
                            model.Msg = "OK # SAVED SUCCESSFULLY";
                            model.Srno = Sno;
                            model.IMEI = IMI;
                            model.Mobile = Mob;
                            //model.Modal = null;
                            return Json(model);
                        }
                        else
                        {
                            model.Msg = "FOCUS # SOMETHING ERROR";
                            //model.Srno = Sno;
                            //model.IMEI = IMI;
                            //model.Mobile = Mob;
                            //model.Modal = null;
                            return Json(model);
                        }
                    }
                }


                if (Sno.ToUpper() != model.Srno.ToUpper() || IMI.Trim().ToUpper() != model.IMEI.ToUpper() ||
                                    Mob.Trim().ToUpper() != model.Mobile.ToUpper())
                {
                    model.Msg = "CONFIRM # Different Care button found! Do you want to replace it";
                    return Json(model);
                }
                else
                {
                    model.Msg = "OK # SAME SRNO.FOUND ON TSN " + _pdioil.TractorSrNo;
                    return Json(model);
                }

            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Srno = null;
                model.IMEI = null;
                model.Mobile = null;
                model.Modal = null;
                return Json(model);
            }
            return Json(model);
        }


        [HttpPost]
        public IHttpActionResult VerifyPassword(PDIOIL _PdioilMode)
        {
            var model = new PDIOIL();
            try
            {
                query = string.Format(@"select count(*) from xxes_stage_master where 
                ad_password = '{0}' and offline_keycode = '{1}' and  plant_code = '{2}' and
                family_code = '{3}'", _PdioilMode.Password, _PdioilMode.LoginStage.ToUpper(), _PdioilMode.Plant, _PdioilMode.Family);

                bool IsVerify = fun.CheckExits(query);

                if (!IsVerify)
                {
                    model.Msg = "ERROR # INVALID PASSWORD";
                    return Json(model);
                }
                else
                {
                    model.Msg = "OK # VERIFYED PASSWORD";
                    return Json(model);
                }

            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                return Json(model);
            }
            return Json(model);
        }
        [HttpPost]
        public IHttpActionResult ChangeCareBtn(PDIOIL _pdioil)
        {
            var model = new PDIOIL();
            string Sno, IMI, Mob = string.Empty;
            try
            {
                Sno = _pdioil.QRCode.Trim().Split(',')[0].Trim();
                IMI = _pdioil.QRCode.Trim().Split(',')[1].Trim();
                Mob = _pdioil.QRCode.Trim().Split(',')[2].Trim();

                query = string.Format(@"update xxes_job_status set MOBILE='{0}',SIM_SERIAL_NO='{1}',IMEI_NO='{2}',CAREBUTTONOIL=sysdate 
                            where FCODE_SRLNO='{3}' and plant_code='{4}' and family_code='{5}'",
                            Mob, Sno, IMI, _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                if (fun.EXEC_QUERY(query))
                {
                    query = string.Format(@"update xxes_job_status set SWAPCAREBTN='{0}' 
                        where FCODE_SRLNO='{1}' and plant_code='{2}' and family_code='{3}'", 'Y',
                    _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());
                    if (fun.EXEC_QUERY(query))
                    {
                        model.Msg = "OK # UPDATE SUCCESSFULLY";
                        model.Srno = Sno;
                        model.IMEI = IMI;
                        model.Mobile = Mob;
                        //model.Modal = null;
                        return Json(model);
                    }
                    else
                    {
                        model.Msg = "ERROR # SOMETHING ERROR";
                        //model.Srno = Sno;
                        //model.IMEI = IMI;
                        //model.Mobile = Mob;
                        //model.Modal = null;
                        return Json(model);
                    }
                }

            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Srno = null;
                model.IMEI = null;
                model.Mobile = null;
                model.Modal = null;
                return Json(model);
            }
            return Json(model);
        }

        [HttpPost]
        public IHttpActionResult ChangeSWAPCAREBTNFlag(PDIOIL _pdioil)
        {
            var model = new PDIOIL();
            string Sno, IMI, Mob = string.Empty;
            try
            {

                query = string.Format(@"update xxes_job_status set SWAPCAREBTN='{0}' 
                        where FCODE_SRLNO='{1}' and plant_code='{2}' and family_code='{3}'", 'N',
                    _pdioil.TractorSrNo.Trim(), _pdioil.Plant.Trim(), _pdioil.Family.Trim());

                if (fun.EXEC_QUERY(query))
                {
                    model.Msg = "OK # UPDATE SUCCESSFULLY";
                    //model.Srno = Sno;
                    //model.IMEI = IMI;
                    //model.Mobile = Mob;
                    //model.Modal = null;
                    return Json(model);
                }
                else
                {
                    model.Msg = "ERROR # SOMETHING ERROR";
                    //model.Srno = Sno;
                    //model.IMEI = IMI;
                    //model.Mobile = Mob;
                    //model.Modal = null;
                    return Json(model);
                }


            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Srno = null;
                model.IMEI = null;
                model.Mobile = null;
                model.Modal = null;
                return Json(model);
            }
            return Json(model);
        }

        [HttpPost]
        public IHttpActionResult TotalMRN(GATEIN _gatein)
        {
            var model = new GATEIN();
            try
            {
                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS where trunc(timein) = trunc(sysdate)");
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    model.Msg = "OK # Total: " + line;
                    model.Plant = null;
                    model.Family = null;
                    model.MRN = null;
                    return Json(model);
                }


                else
                {
                    model.Msg = "ERROR # SOMETHING ERROR";
                    model.Plant = null;
                    model.Family = null;
                    model.MRN = null;
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Plant = null;
                model.Family = null;
                model.MRN = null;
                return Json(model);
            }
            return Json(model);
        }

        public string GetMrnInfo(string Mrn)
        {
            string mrn, invoice, vehicleNo, query = string.Empty;
            try
            {
                //if (Mrn.Split(',')[0].Trim().Length <= 0 || Mrn.Split(',')[1].Trim().Length <= 0 || Mrn.Split(',')[2].Trim().Length <= 0)
                //{
                //    return JsonConvert.SerializeObject("ERROR # INVALID MRN");
                //}

                mrn = Mrn.Split(',')[0].Trim().ToUpper();
                invoice = Mrn.Split(',')[1].Trim().ToUpper();
                vehicleNo = Mrn.Split(',')[2].Trim().ToUpper();

                if (string.IsNullOrEmpty(vehicleNo))
                {
                    return JsonConvert.SerializeObject("ERROR # BY VEHICLE OR BY HAND ?");
                }
                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEIN is not null", mrn);
                if (fun.CheckExits(query))
                {
                    return JsonConvert.SerializeObject("ERROR # ALREADY SCANNED");
                }

                DataTable dt = GetReceiptDetail(vehicleNo, mrn);
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }

        }
        [HttpGet]
        public string RebindGridAfterUpdate(string vehicleNo, string mrn)
        {
            try
            {
                DataTable dt = GetReceiptDetail(vehicleNo, mrn);
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {

                fun.LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }
        private DataTable GetReceiptDetail(string vehicleNo, string mrn)
        {
            DataTable dt = new DataTable();
            try
            {
                if (vehicleNo.Equals("BH"))
                    query = string.Format(@"select MRN_NO MRN, INVOICE_NO INVOICE,ITEM_CODE ITEM FROM ITEM_RECEIPT_DETIALS 
                                            WHERE MRN_NO='{0}' and TIMEIN is null", mrn);
                else
                    query = string.Format(@"select MRN_NO MRN, INVOICE_NO INVOICE,ITEM_CODE ITEM FROM  ITEM_RECEIPT_DETIALS 
                    WHERE VEHICLE_NO='{0}' and TIMEIN is null and  trunc(TRANSACTION_DATE)>trunc(sysdate)-3 
                    order by invoice_no", vehicleNo);

                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                throw;
            }

            return dt;
        }
        [HttpPost]
        public IHttpActionResult UpdateScanMrn(GATEIN _gateIn)
        {
            var model = new GATEIN();
            string mrn, invoice, vehicleNo, query = string.Empty;
            try
            {
                string[] splitMRN = _gateIn.MRN.Split(',');
                if (splitMRN.Length != 3)
                {
                    model.Msg = "ERROR # INVALID MRN";
                    return Json(model);
                }
                if (splitMRN[0].Trim().Length <= 0 || splitMRN[1].Trim().Length <= 0 || splitMRN[2].Trim().Length <= 0)
                {
                    model.Msg = "ERROR # INVALID MRN";
                    return Json(model);
                }

                mrn = _gateIn.MRN.Split(',')[0].Trim().ToUpper();
                invoice = _gateIn.MRN.Split(',')[1].Trim().ToUpper();
                vehicleNo = _gateIn.MRN.Split(',')[2].Trim().ToUpper();

                if (string.IsNullOrEmpty(vehicleNo))
                {
                    model.Msg = "ERROR # BY VEHICLE OR BY HAND ?";
                    return Json(model);
                }
                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}'", mrn);
                if (!fun.CheckExits(query))
                {
                    model.Msg = "ERROR # MRN " + mrn + " NOT FOUND";
                    return Json(model);

                }
                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEIN is not null", mrn);
                if (fun.CheckExits(query))
                {
                    model.Msg = "ERROR # ALREADY SCANNED";
                    return Json(model);

                }
                query = string.Format(@"update ITEM_RECEIPT_DETIALS set TIMEIN = SYSDATE,IN_BY='{0}' 
                    where MRN_NO='{1}'", _gateIn.LoginUser.Trim(), mrn);
                //query = string.Format(@"update ITEM_RECEIPT_DETIALS set TIMEIN = SYSDATE,PERSON='{0}',REMARKS_IN ='{1}',IN_BY='{2}' 
                //    where MRN_NO='{3}'", _gateIn.PersonNo, _gateIn.Remarks, _gateIn.LoginUser.Trim(), mrn);
                if (fun.EXEC_QUERY(query))
                {
                    model.Msg = "OK # UPDATED SUCCESSFULLY";
                    return Json(model);
                }


            }

            catch (Exception ex)
            {
                //model.Msg = "ERROR # " + ex.Message;
                //model.CraneCode = null;
                //model.CraneName = null;
                //return Json(model);
            }
            return Json(model);
        }

        [HttpPost]
        public IHttpActionResult TotalMRNOUT(GATEOUT _GateOut)
        {
            var model = new GATEOUT();
            try
            {
                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS where trunc(timeout) = trunc(sysdate)");
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    model.Msg = "OK # Total: " + line;
                    model.Plant = null;
                    model.Family = null;
                    model.MRN = null;
                    return Json(model);
                }


                else
                {
                    model.Msg = "ERROR # SOMETHING ERROR";
                    model.Plant = null;
                    model.Family = null;
                    model.MRN = null;
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.Plant = null;
                model.Family = null;
                model.MRN = null;
                return Json(model);
            }
            return Json(model);
        }

        [HttpGet]
        public string GetMrnInfoOut(string Mrn, string storebypass)
        {
            string mrn, invoice, vehicleNo, query = string.Empty;
            string timein = string.Empty, storescan = string.Empty, itemcode = string.Empty, timeout = string.Empty;
            try
            {

                mrn = Mrn.Split(',')[0].Trim().ToUpper();
                invoice = Mrn.Split(',')[1].Trim().ToUpper();
                vehicleNo = Mrn.Split(',')[2].Trim().ToUpper();


                //query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEIN is null", mrn);
                //if (fun.CheckExits(query))
                //{
                //    return JsonConvert.SerializeObject("ERROR # MRN NOT SCANNED FOR IN ENTRY");
                //}

                //query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEOUT is not null", mrn);
                //if (fun.CheckExits(query))
                //{
                //    return JsonConvert.SerializeObject("ERROR # ALREADY SCANNED");
                //}
                query = string.Format(@"select i.TIMEIN || '#' || i.STORESCANDATE || '#' || m.itemcode || '#' || i.TIMEOUT  FROM ITEM_RECEIPT_DETIALS i, xxes_mrninfo m
                                WHERE  trunc(PRINTED_ON)>=trunc(sysdate-3) and i.mrn_no=m.mrn_no and i.plant_code=m.plant_code  and i.MRN_NO='{0}' order by i.invoice_no ", mrn);
                string line = fun.get_Col_Value(query);
                if (string.IsNullOrEmpty(line))
                {
                    return JsonConvert.SerializeObject("ERROR # MRN NOT FOUND");
                }
                timein = line.Split('#')[0].Trim();
                storescan = line.Split('#')[1].Trim();
                itemcode = line.Split('#')[2].Trim();
                timeout = line.Split('#')[3].Trim();
                if (string.IsNullOrEmpty(timein))
                {
                    return JsonConvert.SerializeObject("ERROR # MRN NOT SCANNED FOR IN ENTRY");
                }
                else if (!string.IsNullOrEmpty(timeout))
                {
                    return JsonConvert.SerializeObject("ERROR # ALREADY SCANNED");
                }
                if (storebypass != "Y")
                {
                    if (itemcode.StartsWith("D") && string.IsNullOrEmpty(storescan))
                    {
                        return JsonConvert.SerializeObject("ERROR # STORE SCAN REQUIRE");
                    }

                }

                DataTable dt = GetReceiptDetailOut(vehicleNo, mrn, storebypass);
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }

        }
        private DataTable GetReceiptDetailOut(string vehicleNo, string mrn, string storebypass)
        {
            DataTable dt = new DataTable();
            try
            {
                if (storebypass == "Y")
                {
                    if (vehicleNo.Equals("BH"))
                        query = string.Format(@"select mrn_no mrn, invoice_no invoice,item_code item,vendor_name vendor from 
                        item_receipt_detials where mrn_no='{0}' and (timeout is null and timein is not null) and trunc(PRINTED_ON)>=trunc(sysdate-3) order by invoice_no", mrn);
                    else
                        query = string.Format(@"select mrn_no mrn, invoice_no invoice,item_code item,vendor_name vendor from 
                        item_receipt_detials where vehicle_no='{0}' and (timeout is null and timein is not null) and trunc(PRINTED_ON)>=trunc(sysdate-3) order by invoice_no", vehicleNo);
                }
                else
                {
                    if (vehicleNo.Equals("BH"))
                        query = string.Format(@"select i.mrn_no mrn, i.invoice_no invoice,i.item_code item,i.vendor_name vendor 
                                from item_receipt_detials i, xxes_mrninfo m where trunc(i.PRINTED_ON)>=trunc(sysdate-3) and i.mrn_no=m.mrn_no and i.plant_code=m.plant_code  
                                and i.mrn_no='{0}' and (i.timeout is null and i.timein is not null) and i.storescandate is not null 
                                and m.puname in (select distinct character6 puname from apps.qa_results_v  where character6 is not null 
                                and  plan_id =224155 ) and m.puname is not null  order by i.invoice_no", mrn);
                    else
                    {
                        query = string.Format(@"select i.mrn_no mrn, i.invoice_no invoice,i.item_code item,i.vendor_name vendor from 
                                item_receipt_detials i, xxes_mrninfo m where trunc(i.PRINTED_ON)>=trunc(sysdate-3) and i.mrn_no=m.mrn_no and i.plant_code=m.plant_code  and 
                                i.vehicle_no='{0}' and (i.timeout is null and i.timein is not null) and i.storescandate is not null 
                                and m.puname in (select distinct character6 puname from apps.qa_results_v  where character6 is not null 
                                and  plan_id =224155 ) and m.puname is not null  order by i.invoice_no", vehicleNo);
                    }
                }

                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                throw;
            }

            return dt;
        }

        [HttpPost]
        public IHttpActionResult UpdateScanMrnOut(GATEOUT _gateOut)
        {
            var model = new GATEOUT();
            string mrn, invoice, vehicleNo, query = string.Empty;
            try
            {
                string[] splitMRN = _gateOut.MRN.Split(',');
                if (splitMRN.Length != 3)
                {
                    model.Msg = "ERROR # INVALID MRN";
                    return Json(model);
                }
                if (splitMRN[0].Trim().Length <= 0 || splitMRN[1].Trim().Length <= 0 || splitMRN[2].Trim().Length <= 0)
                {
                    model.Msg = "ERROR # INVALID MRN";
                    return Json(model);
                }

                mrn = _gateOut.MRN.Split(',')[0].Trim().ToUpper();
                invoice = _gateOut.MRN.Split(',')[1].Trim().ToUpper();
                vehicleNo = _gateOut.MRN.Split(',')[2].Trim().ToUpper();

                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEIN is null", mrn);
                if (fun.CheckExits(query))
                {
                    model.Msg = "ERROR # MRN NOT SCANNED FOR IN ENTRY";
                    return Json(model);
                }

                query = string.Format(@"select count(*) from ITEM_RECEIPT_DETIALS WHERE MRN_NO='{0}' and TIMEOUT is not null", mrn);
                if (fun.CheckExits(query))
                {
                    model.Msg = "ERROR # ALREADY SCANNED";
                    return Json(model);
                }
                query = string.Format(@"update ITEM_RECEIPT_DETIALS set TIMEOUT = SYSDATE,OUT_BY='{0}' 
                    where MRN_NO='{1}'", _gateOut.LoginUser.Trim(), mrn);
                //query = string.Format(@"update ITEM_RECEIPT_DETIALS set TIMEOUT = SYSDATE,REMARKS_OUT ='{0}',OUT_BY='{1}' 
                //    where MRN_NO='{2}'", _gateOut.Remarks, _gateOut.LoginUser.Trim(), mrn);
                if (fun.EXEC_QUERY(query))
                {
                    model.Msg = "OK # UPDATED SUCCESSFULLY";
                    return Json(model);
                }


            }

            catch (Exception ex)
            {
                //model.Msg = "ERROR # " + ex.Message;
                //model.CraneCode = null;
                //model.CraneName = null;
                //return Json(model);
            }
            return Json(model);
        }

        [HttpGet]
        public string RebindGridAfterUpdateOut(string vehicleNo, string mrn, string storebypass)
        {
            try
            {
                DataTable dt = GetReceiptDetailOut(vehicleNo, mrn, storebypass);
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return JsonConvert.SerializeObject("ERROR:" + ex.Message);
            }
        }


        [HttpPost]
        public IHttpActionResult GetEngineDetails(ENGINE engine)
        {
            var model = new ENGINE();
            string line = string.Empty;
            try
            {
                //query = string.Format(@"select count(*) from XXES_JOB_STATUS where JOBID='{0}' 
                //                and PLANT_CODE='{1}' and family_code='{2}'", engine.JobId.Trim(), engine.Plant.Trim(), engine.Family.Trim());
                //if (fun.CheckExits(query))
                //{
                //}


                query = string.Format(@"select s.ITEM_CODE || ' # ' || s.ENGINE_SRLNO || ' # ' || s.ENGINE from XXES_JOB_STATUS s  
                        where s.JOBID='{0}' and s.PLANT_CODE='{1}' and s.family_code='{2}'", engine.JobId, engine.Plant, engine.Family);
                line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    string fcode = line.Split('#')[0].Trim().ToUpper();
                    model.Fcode = fcode;
                    if (!string.IsNullOrEmpty(line.Split('#')[1].Trim().ToUpper()))
                    {
                        model.SrlNo = line.Split('#')[1].Trim().ToUpper();
                        model.Engine = line.Split('#')[2].Trim().ToUpper();
                    }
                    else
                    {
                        query = string.Format(@"select engine from xxes_item_master where plant_code='{0}' and family_code='{1}' 
                                and item_code='{2}'", engine.Plant, engine.Family, fcode);
                        string dcode = fun.get_Col_Value(query);
                        if (!string.IsNullOrEmpty(line))
                        {
                            model.Dcode = dcode.Trim().ToUpper();
                        }
                    }
                    model.Msg = "FOCUS # ";
                }
                else
                {
                    model.Msg = "ERROR # INVALID JOB";
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                model.Msg = "ERROR # " + ex.Message;
                return Json(model);
            }

            return Json(model);
        }



        [HttpPost]
        public HttpResponseMessage UpdateEngine(ENGINE engine)
        {
            string response = string.Empty; bool printStatus = false;


            DataTable dataTable = new DataTable();
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(orConnstring))
                {
                    OracleCommand comm;
                    comm = new OracleCommand("UDSP_CHECK_PARTS_FOR_ENGINE_FT", oracleConnection);
                    oracleConnection.Open();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("PLANT", engine.Plant.Trim().ToUpper());
                    comm.Parameters.Add("FAMILY", engine.Family.Trim().ToUpper());
                    comm.Parameters.Add("ENGINESRNO", engine.Fcode);
                    comm.Parameters.Add("JOB_ID", engine.JobId.Trim().ToUpper());
                    comm.Parameters.Add("STAGE", engine.StageCode.Trim().ToUpper());
                    comm.Parameters.Add("STAGEID", engine.LoginStage.Trim().ToUpper());
                    comm.Parameters.Add("SYSUSER", engine.SYSUSER.Trim().ToUpper());
                    comm.Parameters.Add("SYSTEMNAME", engine.SYSTEMNAME.Trim().ToUpper());
                    comm.Parameters.Add("LOGINUSER", engine.LoginUser.Trim().ToUpper());
                    comm.Parameters.Add("IS_BYPASS", engine.IsBy_Pass);
                    comm.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                    comm.Parameters["return_message"].Direction = ParameterDirection.Output;
                    comm.ExecuteNonQuery();
                    response = Convert.ToString(comm.Parameters["return_message"].Value);
                    oracleConnection.Close();
                    if (response.StartsWith("OK"))
                    {


                        if (engine.IsPrintLabel == "1")
                        {
                            if (string.IsNullOrEmpty(engine.IPADDR.Trim()))
                            {
                                response = "OK # Matched... but printer IP address not defined";
                            }
                            else
                            {
                                PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
                                printStatus = printAssemblyBarcodes.PrintEngineSticker(response, engine);

                                if (printStatus)
                                {
                                    response = "OK # Matched and printed successfully !! ";
                                }
                                else
                                {
                                    response = "OK # Matched and but not printed successfully !! ";
                                }
                            }
                        }
                        else
                            response = "OK # Matched but print not enabled";
                    }


                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                response = "ERROR : " + ex.Message;


            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }


        /////////////////////created by rajesh///////////////////
        ///
        [HttpPost]
        public string FarmTractorBuckleUp(FTBuckleup fTBuckleup)
        {
            try
            {
                string Transmission_Srlno = string.Empty, RearAxel_Srlno = string.Empty;
                bool isRearAxelRequire = false;
                bool isTransRequire = false;
                bool isBackEndRequire = false;
                bool isByPass = false;
                bool isRePrint = false; string response = string.Empty; bool printStatus = false;
                query = string.Format(@"select Require_RearAxel || '#' || Require_Trans || '#' || Require_Backend 
                        from XXES_ITEM_MASTER where ITEM_CODE='{0}'and PLANT_CODE='{1}' and family_code='{2}'",
                        Convert.ToString(fTBuckleup.ITEMCODE).Split('#')[0].Trim().ToUpper(), fTBuckleup.PLANT.Trim().ToUpper()
                        , fTBuckleup.FAMILY.Trim().ToUpper());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    isRearAxelRequire = (line.Split('#')[0].Trim() == "Y" ? true : false);
                    isTransRequire = (line.Split('#')[1].Trim() == "Y" ? true : false);
                    isBackEndRequire = (line.Split('#')[2].Trim() == "Y" ? true : false);
                    if (isBackEndRequire || isByPass)
                        isRearAxelRequire = isTransRequire = false;
                    if (isTransRequire == true || isRearAxelRequire == true)
                        isBackEndRequire = false;
                }
                else
                {
                    return "Fcode not found On This Job";
                }
                if (string.IsNullOrEmpty(fTBuckleup.BYPASS) == false)
                {
                    fTBuckleup.TRANSMISSIONSRLNO = "";
                }
                if (string.IsNullOrEmpty(fTBuckleup.TRANSMISSIONSRLNO) && isTransRequire == true)
                {
                    return "ERROR : Please scan Transmission";
                }

                if (string.IsNullOrEmpty(fTBuckleup.BYPASS) == false)
                {
                    fTBuckleup.REARAXELSRLNO = "";
                }
                else if (string.IsNullOrEmpty(fTBuckleup.REARAXELSRLNO) && isRearAxelRequire == true)
                {
                    return "ERROR : Please scan RearAxle";
                }
                if (string.IsNullOrEmpty(fTBuckleup.BYPASS) == false)
                {
                    fTBuckleup.BackendSrlno = "";
                }
                else if (string.IsNullOrEmpty(fTBuckleup.BackendSrlno) && isBackEndRequire == true)
                {
                    return "ERROR : Please scan Backend";
                }


                if (af == null)
                    af = new Assemblyfunctions();
                if (fun == null)
                    fun = new Function();

                if (fTBuckleup.TRANSMISSIONSRLNO == fTBuckleup.REARAXELSRLNO
                    && isTransRequire == true && isRearAxelRequire == true)
                {
                    return "Both SrNo should not be same";
                }
                if (isBackEndRequire)
                {
                    query = string.Format(@"select transmission_srlno || '#' || rearaxel_srlno from  XXES_BACKEND_STATUS
                            where backend_srlno='{0}'", fTBuckleup.BackendSrlno);
                    line = fun.get_Col_Value(query);
                    string TransSrlno = "", AxelSrlNo = "";
                    if (!string.IsNullOrEmpty(line))
                    {
                        TransSrlno = line.Split('#')[0].Trim().ToUpper();
                        AxelSrlNo = line.Split('#')[1].Trim().ToUpper();
                    }
                }
                string TractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code ='" + fTBuckleup.ITEMCODE.Trim() + "' and autoid='" + fTBuckleup.FCODEID + "' and plant_code='" + fTBuckleup.PLANT.Trim() + "' and family_code='" + fTBuckleup.FAMILY.Trim() + "'");
                if (string.IsNullOrEmpty(TractorType))
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "TRACTOR TYPE NOT FOUND. i.e DOMESTIC OR EXPORT " + fTBuckleup.ITEMCODE + "", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                    return "Tractor type not found. i.e Domestic or Export";
                }

                string RearAxle = "", Trans = "", ActualAxle = "", ActualTrans = "", Data = "", job = "";
                if (isTransRequire == true)
                {
                    Trans = fun.get_Col_Value("select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='" + fTBuckleup.TRANSMISSIONSRLNO + "'");
                    if (string.IsNullOrEmpty(Trans))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "TRANSMISSION NOT FOUND IN PRINT_SERIAL_NUMBER TABLE.SCANNED SERIAL NO ARE " + fTBuckleup.TRANSMISSIONSRLNO + "", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return " Transmission Not Found";
                    }
                    query = "select JOBID from XXES_JOB_STATUS where TRANSMISSION_SRLNO='" + fTBuckleup.TRANSMISSIONSRLNO + "'";
                    job = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(job.Trim()))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "TRANSMISSION SERIAL NUMBER ALREADY SCANNED ON JOB " + job.Trim() + " .SCANNED SERIAL NO ARE " + fTBuckleup.TRANSMISSIONSRLNO + "", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return "Trans SrlNo Already Scanned On Job : " + job.Trim().ToUpper();

                    }
                }
                if (isRearAxelRequire == true)
                {
                    RearAxle = fun.get_Col_Value("select ITEM_CODE from PRINT_SERIAL_NUMBER where SERIAL_NUMBER='" + fTBuckleup.REARAXELSRLNO + "'");
                    if (string.IsNullOrEmpty(RearAxle))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "REARAXEL NOT FOUND IN PRINT_SERIAL_NUMBER TABLE.SCANNED SERIAL NO ARE " + fTBuckleup.REARAXELSRLNO + "", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return " RearAxle Not Found";
                    }
                    job = "";
                    query = "select JOBID from XXES_JOB_STATUS where REARAXEL_SRLNO='" + fTBuckleup.REARAXELSRLNO + "'";
                    job = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(job.Trim()))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "REARAXEL SERIAL NUMBER ALREADY SCANNED ON JOB " + job.Trim() + ".SCANNED SERIAL NO ARE " + fTBuckleup.REARAXELSRLNO + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return " RearAxel SrlNo Already Scanned On Job" + job.Trim().ToUpper();
                    }
                }
                if (isBackEndRequire == true)
                {
                    query = string.Format(@"select JOBID from XXES_JOB_STATUS where BACKEND_SRLNO='{0}'", fTBuckleup.BackendSrlno);
                    job = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(job.Trim()))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "REARAXEL SERIAL NUMBER ALREADY SCANNED ON JOB " + job.Trim() + ".SCANNED SERIAL NO ARE " + fTBuckleup.REARAXELSRLNO + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return "Backend SrlNo Already Scanned On Job" + job.Trim().ToUpper();
                    }

                }

                query = string.Format(@"select TRANSMISSION || '#' || REARAXEL || '#' || TRANSMISSION_DESCRIPTION || '#' || REARAXEL_DESCRIPTION  
                     from XXES_ITEM_MASTER where trim(ITEM_CODE)='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
                     fTBuckleup.ITEMCODE, fTBuckleup.PLANT.Trim().ToUpper(), fTBuckleup.FAMILY.Trim().ToUpper());
                Data = fun.get_Col_Value(query);
                if (Data.Trim().IndexOf('#') != -1)
                {
                    if (!isBackEndRequire)
                    {
                        ActualTrans = Data.Split('#')[0].Trim().ToUpper();
                        ActualAxle = Data.Split('#')[1].Trim().ToUpper();
                        if (isTransRequire == true && string.IsNullOrEmpty(ActualTrans))
                        {
                            return " Transmission ItemCode Not Found in MES";
                        }
                        if (isRearAxelRequire == true && string.IsNullOrEmpty(ActualAxle))
                        {
                            return " RearAxle ItemCode Not Found in MES";
                        }
                        if ((RearAxle.Trim().ToUpper() != ActualAxle.Trim().ToUpper() || Trans.Trim().ToUpper() != ActualTrans.Trim().ToUpper()) && isRearAxelRequire == true && isTransRequire == true && !string.IsNullOrEmpty(ActualTrans) && !string.IsNullOrEmpty(ActualAxle))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "MISMATCH AXEL AND TRANSMISSION.SCANNED SERIAL NO ARE " + fTBuckleup.REARAXELSRLNO + "AND " + fTBuckleup.TRANSMISSIONSRLNO + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                            return " MisMatch !! Actual Trans : " + ActualTrans + "\n" + "Actual Axle : " + ActualAxle;
                        }
                        else if (RearAxle.Trim().ToUpper() != ActualAxle.Trim().ToUpper() && isRearAxelRequire == true && isTransRequire == false && !string.IsNullOrEmpty(ActualAxle))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), " MISMATCH AXEL.SCANNED SERIAL NO ARE " + fTBuckleup.REARAXELSRLNO + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                            return " Axel MisMatch !! Actual Trans : " + ActualTrans;
                        }
                        else if (Trans.Trim().ToUpper() != ActualTrans.Trim().ToUpper() && isRearAxelRequire == false && isTransRequire == true && !string.IsNullOrEmpty(ActualTrans))
                        {
                            fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), " MISMATCH TRANSMISSION.SCANNED SERIAL NO ARE " + fTBuckleup.TRANSMISSIONSRLNO + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                            return " Trans MisMatch !! Actual Trans : " + ActualTrans;
                        }
                    }
                    query = string.Format(@"SELECT FCODE_AUTOID FROM XXES_DAILY_PLAN_JOB WHERE JOBID='{0}'", fTBuckleup.JOB);
                    if (string.IsNullOrEmpty(fTBuckleup.FCODEID))
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "INVALID JOB FOR SELECTED MODEL " + fTBuckleup.ITEMCODE.Trim() + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return "Invalid Job for selected model : " + fTBuckleup.ITEMCODE;
                    }
                    if (fun.get_Col_Value(query).Trim() != fTBuckleup.FCODEID.Trim())
                    {
                        fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.STAGE, fTBuckleup.JOB.Trim(), "INVALID JOB FOR SELECTED MODEL " + fTBuckleup.ITEMCODE.Trim() + " ", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                        return "Invalid Job for selected model : " + fTBuckleup.ITEMCODE;
                    }
                }
                query = "";
                query = string.Format(@"select count(*) from XXES_JOB_STATUS where JOBID='{0}' and PLANT_CODE='{1}' 
                        and family_code='{2}'", fTBuckleup.JOB, fTBuckleup.PLANT, fTBuckleup.FAMILY);
                if (!fun.CheckExits(query))
                {
                    if (isTransRequire == false)
                    {
                        ActualTrans = "";
                        fTBuckleup.TRANSMISSIONSRLNO = "";

                    }
                    if (isRearAxelRequire == false)
                    {
                        ActualAxle = "";
                        fTBuckleup.REARAXELSRLNO = "";
                    }
                    string remarks = "";
                    if (!isBackEndRequire)
                    {
                        if (isByPass == true)
                            remarks = "AXEL_TRANS_BYPASS";
                        else if (isRearAxelRequire == false && isTransRequire == true)
                            remarks = "AXEL_NOTENABLE_TRANS_ENABLE";
                        else if (isRearAxelRequire == true && isTransRequire == false)
                            remarks = "AXEL_ENABLE_TRANS_NOTENABLE";
                        else if (isRearAxelRequire == false && isTransRequire == false)
                            remarks = "AXEL_NOTENABLE_TRANS_NOTENABLE";
                    }
                    query = string.Format(@"insert into XXES_JOB_STATUS(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,JOBID,
                            TRANSMISSION,REARAXEL,ENTRYDATE,TRANSMISSION_SRLNO, REARAXEL_SRLNO,FCODE_ID,Remarks1,
                            BackEnd_Srlno) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',sysdate,'{7}','{8}',
                            '{9}','{10}','{11}')", fTBuckleup.PLANT.Trim().ToUpper(), fTBuckleup.FAMILY.Trim().ToUpper(),
                            fTBuckleup.ITEMCODE.Trim().ToUpper(), fTBuckleup.TRACTOR_DESC.Trim().ToUpper(), fTBuckleup.JOB,
                            ActualTrans.Trim().ToUpper(), ActualAxle.Trim().ToUpper(), fTBuckleup.TRANSMISSIONSRLNO,
                            fTBuckleup.REARAXELSRLNO, fTBuckleup.FCODEID, remarks, fTBuckleup.BackendSrlno);
                    if (!fun.EXEC_QUERY(query))
                    {
                        return "Error found while saving data for job " + fTBuckleup.JOB;
                    }
                }
                else
                {
                    fun.Insert_Into_ActivityLog("SCAN_ERROR_DCU", fTBuckleup.LoginStageCode, fTBuckleup.JOB, "JOB " + fTBuckleup.JOB + " ALREADY BUCKLED UP", fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.CREATEDBY);
                    return "Job:" + fTBuckleup.JOB + " already buckled up";
                }
                string Filename = "";
                if (fTBuckleup.IsPrintLabel == "0")
                {
                    if (string.IsNullOrEmpty(fTBuckleup.IPADDR.Trim()))
                    {
                        return "OK # Matched... but printer IP address not defined";
                    }
                    else
                    {
                        //if (TractorType == "EXPORT")
                        Filename = "BK.txt";
                        //Filename = "BD17.txt";
                        //else
                        //    Filename = "BK.txt";
                        if (fTBuckleup.PrintMMYYFormat.Trim() != "1")
                        {
                            fTBuckleup.SUFFIX = string.Empty;
                        }
                        string[] itemname = new string[8];
                        if (af == null)
                            af = new Assemblyfunctions();
                        fTBuckleup.TRACTOR_DESC = af.getTractordescription(fTBuckleup.PLANT, fTBuckleup.FAMILY, fTBuckleup.ITEMCODE);
                        //af.getNameLongDesc(pTBuckleup.TRACTOR_DESC, out itemname);


                        if (assemblyBarcodes == null)
                            assemblyBarcodes = new PrintAssemblyBarcodes();
                        if (assemblyBarcodes.PrintBackendStickerFT(fTBuckleup, Filename, itemname, isTransRequire, isRearAxelRequire, isBackEndRequire))
                        {
                            return "OK # Matched and printed successfully !! ";
                        }
                        else
                        {
                            return "OK # Matched and but not printed successfully !! ";
                        }
                    }
                }
                else
                    //    return "OK # Matched but print not enabled";

                    return "OK : RECORD SAVE SUCCESSFULLY ";

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            finally
            {

            }

        }


        [HttpPost]
        public string ReprintTractorLabelFT(FTBuckleup fTBuckleup)
        {
            try
            {
                stringExtention.SetPropertiesToDefaultValues(fTBuckleup);
                string Suffix = string.Empty, response = string.Empty;
                string query, TractorType, line, Filename, ROPS_SRNO = string.Empty; bool isTransRequire = false,
                 isRearAxelRequire = false; bool isBackEndRequire = false;

                line = fun.get_Col_Value(String.Format(@"select ITEM_CODE || '#' || ITEM_DESCRIPTION || '#' ||TRANSMISSION_SRLNO|| '#' ||REARAXEL_SRLNO||  
                 '#' ||FCODE_SRLNO || '#' || FCODE_ID || '#' || JOBID || '#' || ROPS_SRNO || '#' || TO_CHAR(entrydate, 'MM/YY')  
                from XXES_JOB_STATUS where JOBID='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
               fTBuckleup.JOB, fTBuckleup.PLANT, fTBuckleup.FAMILY));
                if (!string.IsNullOrEmpty(line))
                {
                    fTBuckleup.ITEMCODE = line.Split('#')[0].Trim();
                    fTBuckleup.TRACTOR_DESC = line.Split('#')[1].Trim();
                    fTBuckleup.TRANSMISSIONSRLNO = line.Split('#')[2].Trim();
                    fTBuckleup.REARAXELSRLNO = line.Split('#')[3].Trim();
                    fTBuckleup.FCODEID = line.Split('#')[5].Trim();
                    fTBuckleup.JOB = line.Split('#')[6].Trim();
                    ROPS_SRNO = line.Split('#')[8].Trim();
                    string monthcode = line.Split('#')[8].Trim();
                    query = "select count(*) from XXES_ITEM_MASTER where ITEM_CODE='" + fTBuckleup.ITEMCODE + "' and PLANT_CODE='" + fTBuckleup.PLANT.Trim().ToUpper() + "' and family_code='" + fTBuckleup.FAMILY.Trim().ToUpper() + "' and Require_Trans='Y'";
                    isTransRequire = fun.CheckExits(query);
                    query = "select count(*) from XXES_ITEM_MASTER where ITEM_CODE='" + fTBuckleup.ITEMCODE + "' and PLANT_CODE='" + fTBuckleup.PLANT.Trim().ToUpper() + "' and family_code='" + fTBuckleup.FAMILY.Trim().ToUpper() + "' and Require_RearAxel='Y'";
                    isRearAxelRequire = fun.CheckExits(query);
                    TractorType = fun.get_Col_Value("select TYPE from xxes_daily_plan_TRAN where item_code='" + fTBuckleup.ITEMCODE + "' and autoid='" + fTBuckleup.FCODEID + "' and plant_code='" + fTBuckleup.PLANT.Trim() + "' and family_code='" + fTBuckleup.FAMILY.Trim() + "'");
                    if (TractorType == "EXPORT")
                        Filename = "BD17.txt";
                    else
                        Filename = "BK.txt";
                    string[] itemname = new string[8];
                    if (af == null)
                        af = new Assemblyfunctions();
                    fTBuckleup.TRACTOR_DESC = af.getTractordescription(fTBuckleup.PLANT, fTBuckleup.FAMILY,
                        fTBuckleup.ITEMCODE);
                    if (fTBuckleup.PrintMMYYFormat.Trim() == "1")
                    {
                        string EnMisc = fun.get_Col_Value(@"select to_char(scan_date,'MON-YYYY') scan_date from XXES_SCAN_TIME
                        where jobid='" + fTBuckleup.JOB.Trim() + "' and ITEM_CODE='" + fTBuckleup.ITEMCODE.Trim().ToUpper() + "' and stage='BK' and PLANT_CODE='" + fTBuckleup.PLANT.Trim().ToUpper() + "' and FAMILY_CODE='" + fTBuckleup.FAMILY.Trim().ToUpper() + "' and rownum=1");
                    }
                    if (assemblyBarcodes == null)
                        assemblyBarcodes = new PrintAssemblyBarcodes();
                    if (assemblyBarcodes.PrintBackendStickerFT(fTBuckleup, Filename, itemname, isTransRequire,
                        isRearAxelRequire, isBackEndRequire))
                    {
                        response = "OK # JOB " + fTBuckleup.JOB.Trim().ToUpper() + " REPRINTED SUCCESSFULLY !! ";
                    }
                    else
                    {
                        response = "ERROR # SOMETHING WENT WRONG !! ";
                    }

                }
                else
                {

                    return "ERROR : BUCKELUP NOT DONE";

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR : " + ex.Message;
            }
            return "OK # JOB " + fTBuckleup.JOB.Trim().ToUpper() + " REPRINTED SUCCESSFULLY !! ";
        }


        [HttpPost]
        public string GetTransmissionRearAxel(COMMONDATA cOMMONDATA)
        {
            try
            {
                query = string.Format(@"select TRANSMISSION || '#' || REARAXEL || '#' || ITEM_DESCRIPTION || '#' || REQUIRE_TRANS 
                || '#' || REQUIRE_REARAXEL ||'#'|| REQUIRE_BACKEND ||  '#' || SEQ_NOT_REQUIRE from XXES_ITEM_MASTER 
                where ITEM_CODE='{0}' and PLANT_CODE='{1}' and family_code='{2}'",
                cOMMONDATA.ITEMCODE, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                return fun.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
        }

        [HttpPost]
        public IHttpActionResult EngineScanCard(EngineCare care)
        {
            var model = new EngineCare();
            string line = string.Empty;
            string foundJobno, Item, Desc = string.Empty;
            try
            {
                query = string.Format(@"select s.ITEM_CODE || ' # ' || s.ITEM_DESCRIPTION  from XXES_JOB_STATUS s  
                        where s.JOBID='{0}' and s.PLANT_CODE='{1}' and s.family_code='{2}'", care.JOBID, care.PLANT, care.FAMILY);
                line = fun.get_Col_Value(query);

                if (!string.IsNullOrEmpty(line))
                {
                    string fcode = line.Split('#')[0].Trim().ToUpper();
                    model.FCode = fcode;
                    string desc = line.Split('#')[1].Trim().ToUpper();
                    model.Desc = desc;

                    query = string.Format(@"SELECT  ENGINE_SRLNO || '#' || JOBID  FROM xxes_job_status WHERE JOBID='{0}'AND ENGINE_SRLNO IS NOT NULL", care.JOBID);
                    string TSN = fun.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(TSN))
                    {
                        string foundtsno = TSN.Split('#')[0].Trim();
                        model.Msg = "FOCUS # ENGINE SRLNO ALREADY USED IN JOBID. TSN " + foundtsno;
                        return Json(model);
                    }
                    else
                    {
                        query = "select count(*) from XXES_SCANED_JOBS where JOBID='" + care.JOBID.ToUpper().Trim() + "'";
                        if (fun.CheckExits(query))
                        {
                            model.Msg = "ERROR # ALREADY SCANNED";
                            return Json(model);
                        }
                        query = string.Format(@"Insert into XXES_SCANED_JOBS(JOBID,CREATEDBY,CREATEDDATE) values('" +
                        care.JOBID.ToUpper().Trim() + "','" + care.CREATEDBY.ToUpper().Trim() + "',SYSDATE)");

                        if (fun.EXEC_QUERY(query))
                        {
                            model.Msg = "OK # RECORD SAVE SUCCESSFULLY";
                            model.FCode = fcode;
                            model.Desc = desc;
                            return Json(model);
                        }
                        else
                        {
                            model.Msg = "ERROR # SOMETHING ERROR";
                            model.FCode = null;
                            model.Desc = null;
                            return Json(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                model.Msg = "ERROR # " + ex.Message;
                model.FCode = null;
                model.Desc = null;
                return Json(model);
            }
            return Json(model);
        }
        [HttpPost]
        public string ReprintEngineLabelFT(ENGINE fTEngine)
        {
            try
            {
                stringExtention.SetPropertiesToDefaultValues(fTEngine);
                string Suffix = string.Empty, response = string.Empty;
                string query, TractorType, line, Filename,
                     ROPS_SRNO = string.Empty; bool isTransRequire = false, isRearAxelRequire = false;
                bool printStatus = false;
                if (fTEngine.IsPrintLabel != "1")
                {
                    return "PRINT OPTION NOT ENABLED !! ";
                }
                if (string.IsNullOrEmpty(fTEngine.IPADDR))
                {
                    return "IP ADDRESS NOT DEFINED";
                }
                if (string.IsNullOrEmpty(fTEngine.IPPORT))
                {
                    return "PORT NOT DEFINED";
                }

                line = fun.get_Col_Value(String.Format(@"select JS.ITEM_CODE || '#' || IM.ITEM_DESCRIPTION || '#' || JS.ITEM_DESCRIPTION || '#' || JS.TRANSMISSION_SRLNO || '#' || JS.REARAXEL_SRLNO || '#' ||
                            JS.ENGINE_SRLNO || '#' || JS.FCODE_SRLNO || '#' || JS.FCODE_ID || '#' || JS.ROPS_SRNO || '#' || JS.BACKEND_SRLNO || '#' || 
                            TO_CHAR(JS.entrydate, 'MM/YY') || '#' || IM.REQUIRE_ENGINE || '#' || IM.REQUIRE_BACKEND
                     from XXES_JOB_STATUS JS INNER JOIN XXES_ITEM_MASTER IM ON JS.PLANT_CODE = IM.PLANT_CODE AND JS.FAMILY_CODE = IM.FAMILY_CODE
                     AND JS.ITEM_CODE = IM.ITEM_CODE where JS.JOBID='{0}' and JS.PLANT_CODE='{1}' and JS.family_code='{2}'", fTEngine.JobId.Trim(), fTEngine.Plant.Trim(), fTEngine.Family.Trim()));
                if (!string.IsNullOrEmpty(line))
                {

                    PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
                    printStatus = printAssemblyBarcodes.RePrintEngineSticker(line, fTEngine);

                    if (printStatus)
                    {
                        response = "OK # REPRINTED SUCCESSFULLY !! ";
                        return response;
                    }
                    else
                    {
                        response = "ERROR IN PRINTING !! ";
                        return response;
                    }
                }
                else
                {
                    return "BUCKELUP NOT DONE";
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR : " + ex.Message;
            }

        }
        [HttpPost]
        public HttpResponseMessage UpdateHookUp(BeforePaintAssemblyModel data)
        {
            string response = string.Empty, result = string.Empty;
            string LastDetail = "";
            DataTable dt = new DataTable();
            try
            {

                if (Convert.ToInt32(data.HOOKNO) >= 1001 && Convert.ToInt32(data.HOOKNO) <= 1093)
                {
                    string hookno = data.HOOKNO;
                    if (!string.IsNullOrEmpty(hookno))
                    {
                        af.HookdownAccordingtoCurrentHook(data, false); // Put the code to check is job already hooked up and down
                        if (Convert.ToBoolean(af.CheckIsHook_UP_DOWN(data.ScanJOB)))
                        {
                            af.HookdownAccordingtoCurrentHook(data, true);
                            bool isHookedUp = af.HookUpDown(data.ScanJOB, data.Plant, data.Family, data.FCode, data.HOOKNO, data.FCId, true, false, "", "");
                            if (isHookedUp)
                            {
                                response = "OK # UPDATE DATA SUCCESSFULLY ";
                                string HookMsg = "Job No. " + data.ScanJOB + " is Hookedup on hook no. " + data.HOOKNO;
                                fun.UpdateLcdDisplay(data.Plant, data.Family, HookMsg, data.ScanJOB, "BP", "MSG");
                            }
                            else
                            {
                                response = "SOMETHING WENT WRONG FROM DATABASE";
                                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                            }
                        }
                        else
                        {
                            response = "ERROR # ALREADY SCAN JOBID : " + data.ScanJOB;
                            new StringContent(response, System.Text.Encoding.UTF8, "application/json");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }
        [HttpPost]
        public string GetDetailbyId(BeforePaintAssemblyModel data)
        {
            string result = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(data.ScanJOB))
                {
                    data.HOOKNO = af.NextHookNo();
                    result = "OK # " + data.HOOKNO;
                }
                else if (!string.IsNullOrEmpty(data.ScanJOB))
                {
                    query = string.Format(@"SELECT XJS.ITEM_CODE,XJS.FCODE_ID,XJS.FINAL_LABEL_DATE FROM XXES_JOB_STATUS xjs
                        WHERE XJS.JOBID='{0}'", data.ScanJOB.Trim());
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        data.FCode = Convert.ToString(dt.Rows[0]["ITEM_CODE"]);
                        data.FCId = Convert.ToString(dt.Rows[0]["FCODE_ID"]);
                        data.FINALLABELDATE = Convert.ToString(dt.Rows[0]["FINAL_LABEL_DATE"]);
                    }
                    if (string.IsNullOrEmpty(data.FINALLABELDATE))
                    {
                        query = string.Format(@"SELECT XCD.HOOK_NO FROM XXES_CONTROLLERS_DATA xcd WHERE XCD.JOBID='{0}' AND XCD.STAGE='BP' AND XCD.JOBID NOT IN
                            (SELECT XCD1.JOBID FROM XXES_CONTROLLERS_DATA xcd1 WHERE XCD1.JOBID='{0}' AND XCD1.STAGE='AP')", data.ScanJOB.Trim());
                        result = fun.get_Col_Value(query);
                        if (!string.IsNullOrEmpty(result))
                        {
                            data.HOOKNO = result;
                            result = "OK # " + data.HOOKNO;
                        }
                        else
                        {
                            data.HOOKNO = af.NextHookNo();
                            result = "OK # " + data.HOOKNO;
                        }
                    }
                    else
                    {
                        result = "ERROR # THIS JOB NO. ROLLOUT DONE : " + data.ScanJOB;
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return result;
        }
        [HttpPost]
        public string GETHOOKUPDATA(COMMONDATA data)
        {
            string response = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"SELECT XCD.JOBID,XCD.ITEM_CODE,XCD.HOOK_NO,XCD.FCODE_ID FROM XXES_CONTROLLERS_DATA xcd WHERE XCD.PLANT_CODE='{0}' 
                        AND XCD.FAMILY_CODE='{1}' AND STAGE='BP' AND TO_CHAR(XCD.ENTRY_DATE,'dd-Mon-yyyy')=TO_DATE('{2}','dd-Mon-yyyy')", data.PLANT.Trim().ToUpper(),
                        data.FAMILY.Trim().ToUpper(), data.DATA);
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    //response =Convert.ToString(dt);
                    response = JsonConvert.SerializeObject(dt);
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }
            return response;
        }
        [HttpPost]
        public HttpResponseMessage UpdateFIP(FIP data)
        {
            string response = string.Empty, enginedcode = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.EngineSrn))
                {
                    response = "ERROR # PLEASE SCAN ENGINE..";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                else if (string.IsNullOrEmpty(data.Fipsrlno))
                {
                    response = "ERROR # PLEASE SCAN FIPSRLNO ";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                query = string.Format(@"select ITEM_CODE from XXES_ENGINE_STATUS where FUEL_INJECTION_PUMP_SRNO='{0}' and ENGINE_SRNO<>'{1}'"
                                       , data.Fipsrlno.Trim().ToUpper(), data.EngineSrn.Trim().ToUpper());
                enginedcode = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(enginedcode))
                {
                    response = "ERROR # FIP SRNO ALREADY USED ON ENGINE";
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                query = string.Format(@"SELECT NVL(PSN.ITEM_CODE,'') ||'#'|| NVL(XEM.ITEM_DESC,'') ||'#'|| NVL(XEM.INJECTOR,'') ||'#'|| XEM.FUEL_INJECTION_PUMP 
                        FROM PRINT_SERIAL_NUMBER psn , XXES_ENGINE_MASTER xem WHERE PSN.ITEM_CODE= XEM.ITEM_CODE AND PSN.SERIAL_NUMBER='{0}'
                        AND PSN.ORGANIZATION_ID='{1}'", data.EngineSrn.Trim(), data.OrgId);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    data.engine = line.Split('#')[0].Trim();
                    data.itemdesc = line.Split('#')[1].Trim();
                    data.Injector = line.Split('#')[2].Trim();
                    data.Masterfipdcode = line.Split('#')[3].Trim();
                }
                if (string.IsNullOrEmpty(data.Masterfipdcode))
                {
                    response = "ERROR # FIP DCODE NOT MAPPED IN ENGINE : " + data.engine;
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (!string.IsNullOrEmpty(data.EngineSrn) && !string.IsNullOrEmpty(data.Fipsrlno) && !string.IsNullOrEmpty(data.engine))
                {
                    if (af == null)
                        af = new Assemblyfunctions();
                    string value = af.SplitEngineDcode(data.Fipsrlno, data.Injector);
                    data.Fipdcode = value.Split('#')[0].ToString().Trim();
                    data.SplitSerialno = value.Split('#')[1].ToString().Trim();
                    if (string.IsNullOrEmpty(data.Fipdcode))
                    {
                        response = "ERROR # FIP DCODE NOT FOUND IN BARCODE : " + data.Fipsrlno;
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                    if (data.Fipdcode.Trim().ToUpper() != data.Masterfipdcode.Trim().ToUpper())
                    {
                        response = "ERROR # MISMATCH !! MASTER : " + data.Masterfipdcode + "BARCODE :" + data.Fipdcode;
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                        };
                    }
                    if (data.Plant == "T04")
                    {
                        data.Family = "ENGINE FTD";
                    }
                    else if (data.Plant == "T05")
                    {
                        data.Family = "ENGINE TD";
                    }
                    query = string.Format(@"select ITEM_CODE || '#' || FUEL_INJECTION_PUMP_SRNO from XXES_ENGINE_STATUS
                           where engine_srno='{0}'", data.EngineSrn);
                    string line1 = fun.get_Col_Value(query);
                    if (string.IsNullOrEmpty(line1))
                    {
                        query = string.Format(@"INSERT INTO XXES_ENGINE_STATUS(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ENGINE_SRNO, FUEL_INJECTION_PUMP_SRNO) 
                                 VALUES('{0}','{1}','{2}','{3}','{4}')", data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper(), data.engine.Trim().ToUpper(),
                                data.EngineSrn, data.Fipsrlno.Trim().ToUpper());
                        if (fun.EXEC_QUERY(query))
                        {
                            response = "OK # SAVED DATA SUCESSFULLY : " + data.EngineSrn;
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                    }
                    else
                    {
                        string foundsrlno = line1.Split('#')[1].Trim().ToUpper();
                        //if (string.IsNullOrEmpty(data.Password) && !string.IsNullOrEmpty(foundsrlno) && foundsrlno.Trim().ToUpper() != data.Fipsrlno.Trim().ToUpper())
                        if (string.IsNullOrEmpty(data.Password))
                        {
                            response = "ERROR # ENTER PASSWORD : " + data.EngineSrn;
                            return new HttpResponseMessage()
                            {
                                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            query = string.Format(@"update XXES_ENGINE_STATUS set FUEL_INJECTION_PUMP='{2}', FUEL_INJECTION_PUMP_SRNO='{0}'
                                     where engine_srno='{1}' and plant_code='{3}' and family_code='{4}'", data.Fipsrlno.Trim(),
                                     data.EngineSrn.Trim(), data.Fipdcode.Trim(), data.Plant.Trim(), data.Family.Trim());
                            if (fun.EXEC_QUERY(query))
                            {
                                response = "OK # UPDATED DATA SUCESSFULLY : " + data.EngineSrn;
                                return new HttpResponseMessage()
                                {
                                    Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                new StringContent(response, System.Text.Encoding.UTF8, "application/json");
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public string INJEngineDetails(ENGINEINJECTORData data)
        {
            string result = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ENGINE_SRNO='{2}'",
                         data.plantcode.Trim(), data.familycode.Trim(), data.engine_srlno.Trim());
                if (fun.CheckExits(query))
                {
                    result = "ERROR # ALREADY EXIST ENGINE ";
                    return result;
                }
                query = string.Format(@"SELECT nvl(p.dcode,'') || '#' || nvl(ITEM_DESC,'') || '#' ||nvl(INJECTOR,'') || '#' || m.fuel_injection_pump || '#' || m.NO_OF_INJECTORS FROM  
                        XXES_PRINT_SERIALS p,XXES_ENGINE_MASTER m   WHERE p.dcode=m.item_code and p.SRNO='{0}'", data.engine_srlno);
                return fun.get_Col_Value(query);

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return result;
        }
        [HttpPost]
        public string ValidateFIP(ENGINEINJECTORData data)
        {
            string result = string.Empty;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM xxes_engine_barcode_data WHERE ENGINE_SRLNO='{0}' AND BARCODE_DATA='{1}'",
                                     data.engine_srlno.Trim(), data.fipsrlno.Trim());
                if (fun.CheckExits(query))
                {
                    result = "ERROR # ALREADY EXIST :" + data.engine_srlno;
                    return result;
                }
                if (data.injector == "Y")
                {
                    query = string.Format(@"Select ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO='{0}'", data.fipsrlno.Substring(0, 4).Trim());
                    data.fipdcode = fun.get_Col_Value(query);
                    data.splitSerialno = data.fipsrlno.Substring(4, 10);
                }
                else
                {
                    query = string.Format(@"Select ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO='{0}'", data.fipsrlno.Substring(0, 10).Trim());
                    data.fipdcode = fun.get_Col_Value(query);
                    data.splitSerialno = data.fipsrlno.Substring(10);
                }
                if (string.IsNullOrEmpty(data.fipdcode))
                {
                    result = "ERROR # FIP DCODE NOT FOUND IN BARCODE" + data.fipsrlno;
                    return result;
                }
                query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE FUEL_INJECTION_PUMP_SRNO='{0}'",
                         data.fipsrlno.Trim());
                if (!fun.CheckExits(query))
                {
                    result = "OK # VALID FIP";

                }
                else
                {
                    result = "ERROR # ALREADY EXIST FIP : " + data.engine_srlno;
                }
            }
            catch (Exception ex)
            {

                result = "ERROR # " + ex.Message;
            }
            return result;
        }

        [HttpPost]
        public string InjectorVarify(ENGINEINJECTORData data)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(data.No_Of_Injector))
                {
                    if (Convert.ToInt32(data.No_Of_Injector) < 3)
                    {
                        if (string.IsNullOrEmpty(data.injector3))
                        {
                            data.injector3 = "._33333_";
                        }

                    }
                    if (Convert.ToInt32(data.No_Of_Injector) < 4)
                    {
                        if (string.IsNullOrEmpty(data.injector4))
                        {
                            data.injector4 = "._33333_";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(data.injector1))
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}'  AND INJECTOR1='{2}' and INJECTOR1<>'._33333_' and injector1 is not null",
                            data.plantcode, data.familycode, data.injector1);
                    if (fun.CheckExits(query))
                    {
                        result = "ERROR # INJECTOR ALREADY EXIST :" + data.injector1;
                        return result;

                    }
                }
                
                if (!string.IsNullOrEmpty(data.injector2))
                {
                    if(data.injector1 != data.injector2)
                    {
                        query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND INJECTOR2='{2}' and INJECTOR2<>'._33333_' and injector2 is not null",
                        data.plantcode, data.familycode, data.injector2);
                        if (fun.CheckExits(query))
                        {
                            result = "ERROR # INJECTOR ALREADY EXIST :" + data.injector2;
                            return result;
                        }
                    }
                    else
                    {
                        result = "ERROR # INJECTOR SHOULD BE DIFFERENT :" + data.injector2;
                        return result;
                    }
                    
                }
                if (!string.IsNullOrEmpty(data.injector3))
                {
                    if (data.injector1 != data.injector3 && data.injector2 != data.injector3)
                    {
                        query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}'  AND INJECTOR3='{2}' and INJECTOR3<>'._33333_' and injector3 is not null",
                                data.plantcode, data.familycode, data.injector3);
                        if (fun.CheckExits(query))
                        {
                            result = "ERROR # INJECTOR ALREADY EXIST :" + data.injector3;
                            return result;
                        }
                    }
                    else
                    {
                        result = "ERROR # INJECTOR SHOULD BE DIFFERENT :" + data.injector3;
                        return result;
                    }
                }
                if (!string.IsNullOrEmpty(data.injector4))
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_ENGINE_STATUS WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}'  AND INJECTOR4='{2}' and INJECTOR4<>'._33333_' and injector4 is not null",
                        data.plantcode, data.familycode, data.injector4);
                    if (fun.CheckExits(query))
                    {
                        result = "ERROR # INJECTOR ALREADY EXIST :" + data.injector4;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return result;
        }

        [HttpPost]
        public string SaveInjector(ENGINEINJECTORData data)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(data.No_Of_Injector))
                {
                    if (Convert.ToInt32(data.No_Of_Injector) < 3)
                    {
                        if (string.IsNullOrEmpty(data.injector3))
                        {
                            data.injector3 = "._33333_";
                        }

                    }
                    if (Convert.ToInt32(data.No_Of_Injector) < 4)
                    {
                        if (string.IsNullOrEmpty(data.injector4))
                        {
                            data.injector4 = "._33333_";
                        }
                    }
                }
                if (string.IsNullOrEmpty(data.engine_srlno))
                {
                    result = "ERROR # PLEASE SCAN ENGINE";
                }
                if (string.IsNullOrEmpty(data.fipsrlno))
                {
                    result = "ERROR # PLEASE SCAN FIP";
                }

                query = string.Format(@"select ITEM_CODE from XXES_ENGINE_STATUS where FUEL_INJECTION_PUMP_SRNO='{0}' and ENGINE_SRNO<>'{1}'", data.fipsrlno.Trim(), data.engine_srlno.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    result = "ERROR # FIP SRNO ALREADY USED ON ENGINE : " + line;
                }

                if (!string.IsNullOrEmpty(data.engine_srlno) && !string.IsNullOrEmpty(data.fipsrlno))
                {
                    if (data.plantcode == "T04")
                    {
                        data.familycode = "ENGINE FTD";
                    }
                    else if (data.plantcode == "T05")
                    {
                        data.familycode = "ENGINE TD";
                    }
                    query = string.Format(@"SELECT COUNT(*) FROM xxes_engine_barcode_data WHERE ENGINE_SRLNO='{0}' AND BARCODE_DATA='{1}'",
                                      data.engine_srlno.Trim(), data.fipsrlno.Trim());
                    if (!fun.CheckExits(query))
                    {
                        if (data.injector == "Y")
                        {
                            query = string.Format(@"INSERT INTO XXES_ENGINE_BARCODE_DATA ( PLANT_CODE, FAMILY_CODE, ENGINE_SRLNO, ITEM_CODE,SRNO, ENTRYDATE,  BARCODE_DATA)
                                VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', SYSDATE, '{5}')", data.plantcode.Trim().ToUpper(), data.familycode.Trim().ToUpper(), data.engine_srlno,
                                data.engine, data.fipsrlno.Trim().ToUpper().Substring(4, 10), data.fipsrlno.Trim().ToUpper());
                            if (Convert.ToBoolean(fun.EXEC_QUERY(query)))
                            {
                                query = string.Format(@"insert into XXES_ENGINE_STATUS(plant_code,family_code,item_code,engine_srno,FUEL_INJECTION_PUMP_SRNO,
                                       INJECTOR1,INJECTOR2,INJECTOR3,INJECTOR4,ENTRYDATE,fuel_injection_pump) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',SYSDATE,'{9}')", data.plantcode.Trim().ToUpper(),
                                       data.familycode.Trim().ToUpper(), data.engine, data.engine_srlno, data.fipsrlno.Trim().ToUpper().Substring(4, 10), data.injector1, data.injector2, data.injector3, data.injector4, data.fipdcode);
                                if (fun.EXEC_QUERY(query))
                                {
                                    result = "OK # SAVED SUCESSFULLY !!";
                                }
                            }
                        }
                        else
                        {
                            query = string.Format(@"INSERT INTO XXES_ENGINE_BARCODE_DATA ( PLANT_CODE, FAMILY_CODE, ENGINE_SRLNO, ITEM_CODE,SRNO, ENTRYDATE,  BARCODE_DATA)
                                VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', SYSDATE, '{5}')", data.plantcode.Trim().ToUpper(), data.familycode.Trim().ToUpper(), data.engine_srlno,
                                data.engine, data.fipsrlno.Trim().ToUpper().Substring(11), data.fipsrlno.Trim().ToUpper());
                            if (Convert.ToBoolean(fun.EXEC_QUERY(query)))
                            {
                                query = string.Format(@"insert into XXES_ENGINE_STATUS(plant_code,family_code,item_code,engine_srno,FUEL_INJECTION_PUMP_SRNO,
                                       INJECTOR1,INJECTOR2,INJECTOR3,INJECTOR4,ENTRYDATE,fuel_injection_pump) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',SYSDATE,'{9}')", data.plantcode.Trim().ToUpper(),
                                      data.familycode.Trim().ToUpper(), data.engine, data.engine_srlno, data.fipsrlno, data.injector1, data.injector2, data.injector3, data.injector4, data.fipdcode);
                                if (fun.EXEC_QUERY(query))
                                {
                                    result = "OK # SAVED SUCESSFULLY !!";
                                }
                            }
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return "ERROR:" + ex.Message;
            }
            return result;
        }
    }
}
