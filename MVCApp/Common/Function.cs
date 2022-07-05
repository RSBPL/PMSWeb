using System;
using EncodeDecode;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCApp.Models;
using System.Web.Mvc;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using MVCApp.CommonFunction;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Net.Mail;

namespace MVCApp.CommonFunction
{
    static class stringExtention
    {
        
        public static void SetPropertiesToDefaultValues<T>(this T obj)
        {
            var props = obj.GetType().GetProperties();
            Type propType;
            foreach (var prop in props)
            {
                propType = prop.GetType();
                if (prop.PropertyType.Name == "String" && prop.GetValue(obj) == null)
                    prop.SetValue(obj, string.Empty);
                //else if (propType.IsValueType)
                //    prop.SetValue(obj, Activator.CreateInstance(propType));
                //else
                //    prop.SetValue(obj, null);
            }
        }
    }
    public class Function
    {
        public static OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["CON"].ConnectionString);
        public static string orConnstring = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
        OracleCommand cmd;
        OracleDataAdapter da;
        BaseEncDec bed = new BaseEncDec();
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public OracleConnection Connection()
        {

            //if (con == null)
            //con = new OracleConnection(ConfigurationManager.ConnectionStrings["CON"].ConnectionString);
            return con;
        }

        public Function()
        {
            con = new OracleConnection(orConnstring);
        }

        public string ConString()
        {
            return con.ToString();
        }

        public void ConOpen()
        {
            if (con.State == ConnectionState.Closed)
            { con.Open(); }
        }

        public void ConClose()
        {
            if (con.State == ConnectionState.Open)
            { con.Close(); }
        }
        public DataSet returnDataSet(string SqlQuery)
        {
            DataSet ds = new DataSet();
            try
            {
                if (!string.IsNullOrEmpty(SqlQuery))
                {
                    using (OracleDataAdapter Oda = new OracleDataAdapter(SqlQuery, Connection()))
                    {
                        Oda.Fill(ds);
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
        }
        public DataTable returnDataTable(string SqlQuery)
        {
            OracleConnection ConOrcl = new OracleConnection(orConnstring);
            //OracleConfiguration.TraceFileLocation = @"D:\traces";
            //OracleConfiguration.TraceLevel = 7;
            DataTable dt = new DataTable();
            try
            {
                if (!string.IsNullOrEmpty(SqlQuery))
                {
                    OracleDataAdapter Oda = new OracleDataAdapter(SqlQuery, ConOrcl);
                    Oda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { ConOrcl.Dispose(); }
            return dt;
        }

        public Boolean CheckExits(string SqlQuery)
        {
            WriteLog(SqlQuery);

            string ORACLE_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            Boolean returnValue = false;
            OracleConnection ConOrcl = new OracleConnection(ORACLE_CONNECTION_STRING);
            try
            {
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand sc = new OracleCommand(SqlQuery, ConOrcl);
                sc.CommandType = CommandType.Text;
                returnValue = Convert.ToBoolean(sc.ExecuteScalar());
                return returnValue;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }
        public string getUSERNAME()
        {
            string UsrName = string.Empty;
             UsrName = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
            return UsrName;
        }
        public string getSeries(string unit, string family, string stage, string category = null)
        {
            int Start_serial_number, Current_Serial_number, End_serial_Number;
            try
            {
                string prefix = "", toReturn = ""; Current_Serial_number = Start_serial_number = End_serial_Number = 0;

                if (string.IsNullOrEmpty(category))
                    query = "select Start_serial_number, Current_Serial_number,End_serial_Number,Barcode_prefix from XXES_FAMILY_SERIAL where plant_code='" + unit.Trim() + "' and family_code='" + family.Trim() + "' and offline_keycode='" + stage.Trim() + "'";
                else
                    query = @"select Start_serial_number, Current_Serial_number,End_serial_Number,Barcode_prefix from XXES_FAMILY_SERIAL f,XXES_FAMILY_SERIAL_CATEGORY c
                    where f.plant_code='" + unit.Trim() + "' and f.family_code='" + family.Trim() + "' and offline_keycode='" + stage.Trim() + "' and f.autoid=c.FAMILY_SERIAL_ID and CATEGORY='" + category + "' and f.plant_code=c.plant_code and f.family_code=c.family_code ";

                DataTable dt = returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    prefix = Convert.ToString(dt.Rows[0]["Barcode_prefix"]).Trim();
                    if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() == "")
                    {
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["Start_serial_number"]).Trim())) + 1;
                    }
                    else if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() != "")
                    {
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim())) + 1;
                    }
                    if (Current_Serial_number > Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["End_serial_Number"]).Trim())))
                    {
                        Current_Serial_number = -99; //series full
                        prefix = "";
                    }
                    toReturn = Convert.ToString(Current_Serial_number);
                    while (toReturn.Trim().Length < Convert.ToString(dt.Rows[0]["Start_serial_number"]).Trim().Length)
                    {
                        toReturn = "0" + toReturn;
                    }
                }
                return prefix.Trim() + "#" + Convert.ToString(toReturn);
            }
            catch { return ""; }
            finally { }
        }

        public string get_Col_Value(string command)
        {
            //WriteLog(command);
            string returnValue = "";
            string ORACLE_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            OracleConnection ConOrcl = new OracleConnection(ORACLE_CONNECTION_STRING);
            try
            {
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = ConOrcl;
                cmd.CommandText = command;
                cmd.CommandType = CommandType.Text;
                returnValue = Convert.ToString(cmd.ExecuteScalar());
                return returnValue;
               
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }
        public bool EXEC_QUERY(string command)
        {
            string ORACLE_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            //WriteLog(command);
            bool returnValue = false;
            OracleConnection ConOrcl = new OracleConnection(ORACLE_CONNECTION_STRING);
            try
            {
                if (ConOrcl.State == ConnectionState.Closed)
                { ConOrcl.Open(); }
                OracleCommand sc = new OracleCommand(command, ConOrcl);
                sc.CommandType = CommandType.Text;
                returnValue = Convert.ToBoolean(sc.ExecuteNonQuery());
                return returnValue;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                if (ConOrcl.State == ConnectionState.Open)
                { ConOrcl.Close(); }
                ConOrcl.Dispose();
            }
        }
        public DataTable GetTyreMakeList(string fcode)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (!string.IsNullOrEmpty(fcode))
                {
                    dataTable = returnDataTable
                        (
                           string.Format(@"SELECT distinct TYRE Name FROM XXES_SFT_SETTINGS WHERE itemcode='{0}' and PARAMETERINFO='FCODE_TYRES'",
                           fcode)
                        );
                }
                if(string.IsNullOrEmpty(fcode))
                    query = "select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO";
                else if (dataTable.Rows.Count == 0)
                {
                    query = "select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO";
                }
                dataTable = returnDataTable(query);
            }
            catch (Exception)
            {

                throw;
            }
            return dataTable;

        }
        public DataTable GetUserUnit()
        {
            try
            {
                String userRole = HttpContext.Current.Session["userPermission"].ToString();
                DataTable dt = new DataTable();
                if (!String.IsNullOrEmpty(userRole))
                {
                    da = new OracleDataAdapter("USP_GETUSERUNIT", Connection());
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.Add("UROLE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = userRole;
                    da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                    da.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
        }
        public bool UpdatePrintSerialNo(SubAssembly subAssembly)
        {

            try
            {
                string query = @"update xxes_print_serials set QCOK='Y',QCDATE=sysdate where dcode='" + subAssembly.Itemcode + "' and plant_code='" + subAssembly.Plant + "' and family_code='" + subAssembly.Family + "' and srno='" + subAssembly.SerialNumber + "' and JOBID='" + subAssembly.Job + "'";
                return EXEC_QUERY(query);
            }
            catch (Exception)
            {

                throw;
            }
        }
     
        public bool UpdateFamilySerials(SubAssembly subAssembly)
        {
            string query = "";
            using (OracleConnection connection = new OracleConnection(orCnstr))
            {
                connection.Open();
                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;
                // Start a local transaction
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                // Assign transaction object for a pending local transaction
                command.Transaction = transaction;
                try
                {
                    if (subAssembly.Series == "FREE_SRNO")
                    {
                        query = String.Format(@"delete from PRINT_SERIAL_NUMBER where Plant_CODE='{0}' and SERIAL_NUMBER='{1}'", subAssembly.Plant.Trim().ToUpper(),
                            subAssembly.SerialNumber.Trim().ToUpper());
                        EXEC_QUERY(query);
                        //command.CommandText = query;
                        //command.ExecuteNonQuery();

                        query = "delete from JOB_SERIAL_MOVEMENT where PLANT_CODE='" + subAssembly.Plant.Trim() + "' and  FAMILY_CODE='" + subAssembly.Family + "' and SERIAL_NO='" + subAssembly.SerialNumber.Trim() + "'";
                        EXEC_QUERY(query);
                        //command.CommandText = query;
                        //command.ExecuteNonQuery();

                        query = @"delete from XXES_PRINT_SERIALS where PLANT_CODE='" + subAssembly.Plant.Trim() + "' and " +
                            " FAMILY_CODE='" + subAssembly.Family + "' and SRNO='" + subAssembly.SerialNumber + "'";
                        //command.CommandText = query;
                        //command.ExecuteNonQuery();
                        EXEC_QUERY(query);
                    }
                    if (!string.IsNullOrEmpty(subAssembly.Series) && subAssembly.Series != "NA"
                        && subAssembly.Series != "FREE_SRNO")
                    {
                        query = "";
                        query = String.Format("update XXES_FAMILY_SERIAL set Current_Serial_number='{0}',LAST_PRINTED_LABEL_DATE_TI=SYSDATE where  plant_code='{1}' and family_code='{2}' and offline_keycode='{3}'", subAssembly.Series.Trim(), subAssembly.Plant,
                            subAssembly.Family.Trim(), subAssembly.Stage);
                        command.CommandText = query;
                        if (Convert.ToBoolean(command.ExecuteNonQuery()))
                        {
                            query = "";
                            query = get_Col_Value("select count(*) from XXES_SFT_SETTINGS where PARAMETERINFO='FAMILY_SERIAL' and STATUS='Y'");
                            if (!string.IsNullOrEmpty(query) && Convert.ToInt16(query) > 0)
                            {
                                query = "";
                                query = String.Format("update FAMILY_SERIAL set Current_Serial_number='{0}',LAST_PRINTED_LABEL_DATE_TI=SYSDATE where  plant_code='{1}' and family_code='{2}'", subAssembly.Series.Trim(), subAssembly.Plant,
                                subAssembly.Family.Trim());
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    query = "";
                    if (!string.IsNullOrEmpty(subAssembly.Job))
                    {
                        if (!CheckExits("select count(*) from PRINT_SERIAL_NUMBER where Plant_CODE='" + subAssembly.Plant.Trim().ToUpper() + "' and  SERIAL_NUMBER='" + subAssembly.SerialNumber.Trim().ToUpper() + "' and ORGANIZATION_ID='" + subAssembly.Orgid + "' and ITEM_CODE='" + subAssembly.Itemcode.Trim() + "'"))
                        {
                            query = "";
                            query = String.Format("insert into PRINT_SERIAL_NUMBER(Plant_CODE,ITEM_CODE,JOB_ID,SERIAL_NUMBER,ORGANIZATION_ID,CREATION_DATE,BIG_LABEL_PRINTED) values('{0}','{1}','{2}','{3}','{4}',SYSDATE,-1)", subAssembly.Plant.Trim().ToUpper(),
                                subAssembly.Itemcode.Trim().ToUpper(), subAssembly.Job.Trim().ToUpper(), subAssembly.SerialNumber.Trim().ToUpper(), subAssembly.Orgid);
                            command.CommandText = query;
                            command.ExecuteNonQuery();

                        }
                    }
                    if (!string.IsNullOrEmpty(subAssembly.Job))
                    {
                        query = "select count(*) from JOB_SERIAL_MOVEMENT where PLANT_CODE='" + subAssembly.Plant.Trim() + "' and  FAMILY_CODE='" + subAssembly.Family + "' and SERIAL_NO='" + subAssembly.SerialNumber.Trim() + "' and ITEM_CODE='" + subAssembly.Itemcode.Trim() + "'";
                        if (!CheckExits(query))
                        {
                            query = "";
                            query = "INSERT INTO JOB_SERIAL_MOVEMENT (PLANT_CODE,ITEM_CODE,JOB_ID,SERIAL_NO,FAMILY_CODE,CURRENT_STAGE_ID,TRANSACTION_COMPLETE) VALUES ('"
                               + subAssembly.Plant.Trim() + "','" + subAssembly.Itemcode + "' , '" + subAssembly.Job + "' , '" + subAssembly.SerialNumber.Trim().ToUpper() + "' , '" + subAssembly.Family + "' , 0,0)";
                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                    }
                    query = @"select count(*) from XXES_PRINT_SERIALS where PLANT_CODE='" + subAssembly.Plant.Trim() + "' and  FAMILY_CODE='" + subAssembly.Family + "' and offline_keycode='" + subAssembly.Stage + "' and DCODE='" + subAssembly.Itemcode + "' and SRNO='" + subAssembly.SerialNumber + "'";
                    if (!CheckExits(query) && !string.IsNullOrEmpty(subAssembly.SerialNumber.Trim()))
                    {
                        query = "";
                        query = @"insert into XXES_PRINT_SERIALS(PLANT_CODE,FAMILY_CODE,STAGE_ID,DCODE,
                        SRNO,PRINTDATE,OFFLINE_KEYCODE,TYRE_DCODE,RIM_DCODE,MISC1,TRAN_ID,SUBASSEMBLY_ID,JOBID,DESCRIPTION,DUPLICATE)
                            values('" + subAssembly.Plant.Trim() + "','" + subAssembly.Family + "','" + HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim() + "','" + subAssembly.Itemcode.Trim() + "','" + subAssembly.SerialNumber + "',SYSDATE,'" + subAssembly.Stage + "','','','','" + subAssembly.TranId + "','" + subAssembly.SubAssembly_Id + "','" + subAssembly.Job.Trim() + "','" + subAssembly.Description + "','" + subAssembly.DuplicateFlag + "')";
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;

                }
                finally
                {
                    connection.Close();
                }
            }
        }
        //TO GET UNITS
        public List<DDLTextValue> BindUnit()
        {
            try
            {
                DataTable dt = GetUserUnit();
                List<DDLTextValue> unit = new List<DDLTextValue>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        unit.Add(new DDLTextValue
                        {
                            Text = dr["UNITNAME"].ToString(),
                            Value = dr["UNITCODE"].ToString(),
                        });
                    }
                }
                return unit;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
        }

        public List<DDLTextValue> BindFamily(string Id)
        {
            try
            {
                Connection();
                List<DDLTextValue> unit = new List<DDLTextValue>();
                string query = "SELECT PF.FAMILY_CODE AS FAMILYCODE, FM.DESCRIPTION AS FAMILYNAME FROM TBL_PLANT_FAMILY_MAPPINGMASTER PF JOIN TBL_FAMILYMASTER FM ON PF.FAMILY_CODE = FM.FAMILY_CODE WHERE PF.PLANT_CODE = '" + Id + "'";
                OracleCommand cmd = new OracleCommand(query, Connection());
                OracleDataReader sdr;
                ConOpen();
                sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        unit.Add(new DDLTextValue
                        {
                            Text = sdr["FAMILYNAME"].ToString(),
                            Value = sdr["FAMILYCODE"].ToString()
                        });
                    }
                }
                return unit;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> BindStage(string U, string F)
        {
            try
            {
                Connection();
                List<DDLTextValue> unit = new List<DDLTextValue>();
                string query = "SELECT STAGECODE, DESCRIPTION FROM TBL_STAGEMASTER SM WHERE UNITCODE = '" + U + "' AND FAMILYCODE = '" + F + "'";
                OracleCommand cmd = new OracleCommand(query, Connection());
                OracleDataReader sdr;
                ConOpen();
                sdr = cmd.ExecuteReader();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        unit.Add(new DDLTextValue
                        {
                            Text = sdr["DESCRIPTION"].ToString(),
                            Value = sdr["STAGECODE"].ToString()
                        });
                    }
                }
                return unit;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> BindRole()
        {
            try
            {
                List<DDLTextValue> Role = new List<DDLTextValue>();
                string query = "select L_CODE, L_NAME from XXES_LEVEL_MASTER WHERE ROLL_FOR = 'WEB' ";
                OracleCommand cmd = new OracleCommand(query, Connection());
                OracleDataReader sdr;
                ConOpen();
                sdr = cmd.ExecuteReader();

                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Role.Add(new DDLTextValue
                        {
                            Text = sdr["L_NAME"].ToString() + " # " + sdr["L_CODE"].ToString(),
                            Value = sdr["L_CODE"].ToString()
                        });
                    }
                }
                return Role;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> BindWarehouse(string U, string F)
        {
            try
            {
                List<DDLTextValue> Role = new List<DDLTextValue>();
                string query = "SELECT UWM.WHID, WM.DESCRIPTION, WM.SHORTNAME FROM TBL_UNIT_WAREHOUSE_MAPPING UWM JOIN TBL_WAREHOUSEMASTER WM ON UWM.WHID = WM.WHID WHERE UWM.UNITCODE = '" + U + "' AND UWM.FAMILYCODE = '" + F + "'";
                OracleCommand cmd = new OracleCommand(query, Connection());
                OracleDataReader sdr;
                ConOpen();
                sdr = cmd.ExecuteReader();

                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        Role.Add(new DDLTextValue
                        {
                            Text = sdr["SHORTNAME"].ToString(),
                            Value = sdr["WHID"].ToString()
                        });
                    }
                }
                ConClose();
                return Role;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetUserIP()
        {
            //string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (string.IsNullOrEmpty(ip))
            //{
            //    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            //}
            return HttpContext.Current.Request.UserHostAddress;

            //string ipList = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (ipList != null)
            //{
            //    if (!string.IsNullOrEmpty(ipList))
            //    {
            //        return ipList.Split(',')[0];
            //    }
            //}
            //else
            //    ipList = Convert.ToString(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]).Trim();
            //return ipList;
        }
        


        public List<DDLTextValue> Fill_Unit_Name()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> Unit = new List<DDLTextValue>();
                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    TmpDs = returnDataTable("Select U_Code || ' # ' || U_Name as  Unit_Name,U_Code from XXES_Unit_Master where U_Code<>'GU' order by U_Name");
                }
                else
                {
                    TmpDs = returnDataTable("Select U_Code || ' # ' || U_Name as   Unit_Name,U_Code from XXES_Unit_Master where U_Code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by U_Name");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Unit.Add(new DDLTextValue
                        {
                            Text = dr["Unit_Name"].ToString(),
                            Value = dr["U_Code"].ToString(),
                        });
                    }
                }
                return Unit;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }



        public List<DDLTextValue> Fill_Family(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                TmpDs = returnDataTable(@"Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER 
                    where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        if (ucode == "T04")
                        {
                            if (Convert.ToString(dr["Name"]).StartsWith("TRACTOR") || Convert.ToString(dr["Name"]).StartsWith("BACK END"))
                            {
                                Family.Add(new DDLTextValue
                                {
                                    Text = dr["Name"].ToString(),
                                    Value = dr["FAMILY_CODE"].ToString(),
                                });
                            }
                        }
                        if (ucode == "T05")
                        {
                            if (Convert.ToString(dr["Name"]).StartsWith("TRACTOR"))
                            {
                                Family.Add(new DDLTextValue
                                {
                                    Text = dr["Name"].ToString(),
                                    Value = dr["FAMILY_CODE"].ToString(),
                                });
                            }
                        }
                        if (ucode == "T02")
                        {
                            if (Convert.ToString(dr["Name"]).StartsWith("TRACTOR"))
                            {
                                Family.Add(new DDLTextValue
                                {
                                    Text = dr["Name"].ToString(),
                                    Value = dr["FAMILY_CODE"].ToString(),
                                });
                            }
                        }
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public List<DDLTextValue> Fill_All_Family(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                TmpDs = returnDataTable(@"Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER 
                    where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }
        public List<DDLTextValue> Fill_All_Family_Instead_Tractor(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                }
                else
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "') and family_code='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        if (!Convert.ToString(dr["Name"]).StartsWith("TRACTOR"))
                        {
                            Family.Add(new DDLTextValue
                            {
                                Text = dr["Name"].ToString(),
                                Value = dr["FAMILY_CODE"].ToString(),
                            });
                        }

                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }
        public List<DDLTextValue> Fill_FamilyOnlyTractor(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                }
                else
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "') and family_code='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        if (Convert.ToString(dr["Name"]).StartsWith("TRACTOR"))
                        {
                            Family.Add(new DDLTextValue
                            {
                                Text = dr["Name"].ToString(),
                                Value = dr["FAMILY_CODE"].ToString(),
                            });
                        }
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public List<DDLTextValue> Fill_FamilyForSubAssembly(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
                {
                    if (ucode == "T04")
                    {
                        TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and FAMILY_CODE not like 'TRACTOR%' AND FAMILY_CODE not like 'BACK END%')");
                    }
                    else if (ucode == "T05")
                    {
                        TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and FAMILY_CODE not like 'TRACTOR%')");
                    }

                }
                else
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "'  and FAMILY_CODE not like 'TRACTOR%' AND FAMILY_CODE not like 'BACK END%') and family_code='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public DataTable Fill_Family(string ucode, string type)
        {
            DataTable TmpDs = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(type))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as   Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                }
                else if (type.Equals("EXCLUDE"))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as   Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and family_code not like '%TRACTOR%')");
                }
                else if (type.Equals("EXCLUDE_ENGINE"))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as   Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and family_code not like '%ENGINE%')");
                }
                else
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as   Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and family_code like '" + type + "%')");
                }

                return TmpDs;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                //MessageBox.Show("Module Fill_Family : " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return TmpDs;
            }
            finally
            {
                //ConClose();
            }
        }

        public List<DDLTextValue> FillShift()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = returnDataTable("select Shiftcode || ' (' || Start_Time || '-' || End_Time || ')' as Shfit,ShiftCODE from XXES_SHIFT_MASTER order by shiftcode");
                List<DDLTextValue> Shift = new List<DDLTextValue>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Shift.Add(new DDLTextValue
                        {
                            Text = dr["Shfit"].ToString(),
                            Value = dr["ShiftCODE"].ToString(),
                        });
                    }
                }
                return Shift;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
        }

        public string Find_Unit_Name(string CheckUnit)
        {
            try
            {
                return get_Col_Value("Select U_Name from XXES_Unit_Master where U_Code='" + CheckUnit.Trim() + "'").ToString();
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "Module Find_Unit_Name: " + ex.Message.ToString();
                //throw;
                //return "";
            }
        }



        public DateTime GetServerDateTime()
        {
            try
            {
                string query = "SELECT TO_CHAR(SYSDATE, 'DD-MM-YYYY HH24:MI:SS') FROM DUAL";
                return Convert.ToDateTime(get_Col_Value(query));                 
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return DateTime.Now;
            }
            finally { ConClose(); }
        }

        public string getshift()
        {
            try
            {
                string returnData = "", isDayNeedToLess = "";
                DataTable dtshift = new DataTable();

                dtshift = returnDataTable("select * from XXES_SHIFT_MASTER order by SHIFTCODE");
                if (dtshift.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtshift.Rows)
                    {
                        DateTime abc = GetServerDateTime();

                        if (Convert.ToString(dr["SHIFTCODE"]) == "A")
                        {
                            HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                            HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(0);
                        }
                        else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && Convert.ToDateTime(abc) >= Convert.ToDateTime(DateTime.Parse("00:01")) && Convert.ToDateTime(abc) <= Convert.ToDateTime(DateTime.Parse("01:00")))
                        {
                            HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.AddDays(-1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                            HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            isDayNeedToLess = "1";
                            HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(-1);
                        }
                        else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && Convert.ToDateTime(abc) >= Convert.ToDateTime(DateTime.Parse(Convert.ToString(dr["START_TIME"]))) && Convert.ToDateTime(abc) <= Convert.ToDateTime(DateTime.Parse("23:59")))
                        {
                            HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                            HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.AddDays(1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(0);
                        }
                        else if (Convert.ToString(dr["SHIFTCODE"]) == "C")
                        {
                            HttpContext.Current.Session["ShiftStart"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                            HttpContext.Current.Session["shiftEnd"] = Convert.ToDateTime(Convert.ToDateTime(abc).Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            HttpContext.Current.Session["ServerDate"] = GetServerDateTime().AddDays(-1);
                        }
                        if (Convert.ToDateTime(abc) >= Convert.ToDateTime(HttpContext.Current.Session["ShiftStart"]) && Convert.ToDateTime(abc) <= Convert.ToDateTime(HttpContext.Current.Session["shiftEnd"]))
                        {
                            returnData = Convert.ToString(dr["SHIFTCODE"]) + '#' + Convert.ToString(dr["NIGHT_EXISTS"]) + '#' + isDayNeedToLess + '#' + Convert.ToDateTime(HttpContext.Current.Session["ShiftStart"]).ToString() + '#' + Convert.ToDateTime(HttpContext.Current.Session["shiftEnd"]).ToString();
                            break;
                        }
                    }
                }
                return returnData;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return "";
                throw;
            }
        }


        public string replaceApostophi(string chkstr)
        {
            return chkstr.Replace("'", "''");
        }

        public void Insert_Into_ActivityLog(string actMode, string actWork, string actPryField,
            string query, string plant_code, string family, string AppLoginUser = null)
        {
            try
            {
                string actUsrName = string.Empty;
                if (string.IsNullOrEmpty(AppLoginUser))
                    actUsrName = Convert.ToString(HttpContext.Current.Session["Login_Unit"]);
                else
                    actUsrName = AppLoginUser;
                string actSysName = GetUserIP();

                using (OracleCommand sc = new OracleCommand("udsp_Insert_Act_Log", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("p_Act_Mod", OracleDbType.NVarchar2).Value = actMode.Trim();
                    sc.Parameters.Add("p_Act_Work", OracleDbType.NVarchar2).Value = actWork.Trim();
                    sc.Parameters.Add("p_U_Name", OracleDbType.NVarchar2).Value = actUsrName.Trim();
                    sc.Parameters.Add("p_System_Name", OracleDbType.NVarchar2).Value = actSysName.Trim();
                    sc.Parameters.Add("p_Primary_Field", OracleDbType.NVarchar2).Value = actPryField.Trim();
                    sc.Parameters.Add("p_Sql_Qry", OracleDbType.NVarchar2).Value = query.Trim();
                    sc.Parameters.Add("p_PLANT_CODE", OracleDbType.NVarchar2).Value = plant_code.Trim();
                    sc.Parameters.Add("p_FAMILY_CODE", OracleDbType.NVarchar2).Value = family.Trim();
                    sc.ExecuteNonQuery();
                    //ConClose();
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { ConClose(); }
        }

        //To return family only for tractor (TRACTOR FTD / TRACTOR TD) as per unit (T04 or T05)
        public string FamilyReturnByUnit(string Unit)
        {
            if (Unit == "T04")
            {
                return "TRACTOR FTD";
            }
            if (Unit == "T05")
            {
                return "TRACTOR TD";
            }
            else
            {
                return "";
            }
        }

        public string getOrgId(string plant, string family)
        {
            try
            {
                string query = "select f.ORG_ID from XXES_PLANT_FAMILY_MAP m , XXES_FAMILY_MASTER f where m.FAMILY_CODE=f.FAMILY_CODE and m.PLANT_CODE='" + plant.Trim().ToUpper() + "' and m.FAMILY_CODE='" + family.Trim().ToUpper() + "'";
                return get_Col_Value(query);
            }
            catch { return ""; }
            finally
            { }
        }

        public void WriteLog(string Message, string directoryname = null)
        {
            _readWriteLock.EnterWriteLock();
            StreamWriter sw = null;

            try
            {
                string sPathName = string.Empty;
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                if (string.IsNullOrEmpty(directoryname))
                    sPathName = HttpContext.Current.Server.MapPath("~/APP_FOLDERS/Log");
                else
                    sPathName = HttpContext.Current.Server.MapPath("~/APP_FOLDERS/" + directoryname);

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sTime = sDay + "-" + sMonth + "-" + sYear;
                if (!Directory.Exists(sPathName))
                {
                    Directory.CreateDirectory(sPathName);
                }
                sw = new StreamWriter(sPathName + "\\Log_" + sTime + ".txt", true);

                sw.WriteLine(sLogFormat + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
                _readWriteLock.ExitWriteLock();
            }

        }
        public void PrinterLog(string Message, string stage)
        {
            StreamWriter sw = null;

            try
            {
                string sPathName = string.Empty;
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                //if (string.IsNullOrEmpty(stage))
                    sPathName = HttpContext.Current.Server.MapPath("~/APP_FOLDERS/Log");
                //else
                //    sPathName = HttpContext.Current.Server.MapPath("~/APP_FOLDERS/" + directoryname);

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sTime = sDay + "-" + sMonth + "-" + sYear;
                if (!Directory.Exists(sPathName))
                {
                    Directory.CreateDirectory(sPathName);
                }
                sw = new StreamWriter(sPathName + "\\" + stage + "_" + sTime + ".txt", true);

                sw.WriteLine(sLogFormat + " ==> " + stage + " ==> " + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }

        }
        public bool UpdateLcdDisplay(string plant, string family, string msg, string key, string stage, string datatype)
        {
            string query = string.Empty;
            try
            {
                query = string.Format(@"delete from XXES_LIVE_DATA where stage='{0}' and data_type='{1}' and
                            plant_code='{2}' and family_code='{3}'", stage, datatype, plant, family);
                EXEC_QUERY(query);
                query = string.Format(@"insert into XXES_LIVE_DATA(PLANT_CODE,FAMILY_CODE,STAGE,SCAN_DATE,DATA_TYPE,REMARKS1,srlno) 
                            values('{0}','{1}','{2}',SYSDATE,'{3}','{4}', '{5}')"
                       , plant, family, stage, datatype, msg, key
                         );
                EXEC_QUERY(query);
                return true;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
        }

        public List<DDLTextValue> Fill_All_Stage()
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                string query = "SELECT  stage_description, OFFLINE_KEYCODE || '#' || STAGE_ID stage_id FROM xxes_stage_master ";
                TmpDs = returnDataTable(query);


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {

                        Family.Add(new DDLTextValue
                        {
                            Text = dr["stage_description"].ToString(),
                            Value = dr["stage_id"].ToString(),
                        });

                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }
        public List<DDLTextValue> Fill_All_Stage_ByPlantAndFamily(string plant, string family)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                string query = "SELECT  stage_description, OFFLINE_KEYCODE || '#' || STAGE_ID stage_id FROM xxes_stage_master WHERE plant_code = '" + plant.Trim() + "' AND family_code = '" + family.Trim() + "'";
                TmpDs = returnDataTable(query);


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {

                        Family.Add(new DDLTextValue
                        {
                            Text = dr["stage_description"].ToString(),
                            Value = dr["stage_id"].ToString(),
                        });

                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }
        public bool UpdateFamilySerial(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_FamilySerial", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("Pplant_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pfamily_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;


                    sc.Parameters.Add("Pstage_id", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Poffline_keycode", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pstart_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Start_Serial == null ? null : itemModel.Start_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Pend_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.End_Serial == null ? null : itemModel.End_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Pnumber_sub_assemblies", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.No_SubAssemblies == null ? null : itemModel.No_SubAssemblies.ToUpper().Trim();
                    sc.Parameters.Add("Pbarcode_prefix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Prefix == null ? null : itemModel.Prefix.ToUpper().Trim();
                    sc.Parameters.Add("Psuffix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Suffix == null ? null : itemModel.Suffix.ToUpper().Trim();
                    sc.Parameters.Add("Pcurrent_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Current_Serial == null ? null : itemModel.Current_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Plast_printed_label_date_ti", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.LastPrintedDate == null ? null : itemModel.LastPrintedDate.ToUpper().Trim();
                    sc.Parameters.Add("Pautoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("PcallType", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateFamilySerial";
                    sc.Parameters.Add("PcreatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("PupdatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {

                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteFamilySerial(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_FamilySerial", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("Pplant_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pfamily_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;


                    sc.Parameters.Add("Pstage_id", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Poffline_keycode", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pstart_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pend_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pnumber_sub_assemblies", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pbarcode_prefix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Psuffix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pcurrent_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Plast_printed_label_date_ti", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("Pautoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("PcallType", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteFamilySerial";
                    sc.Parameters.Add("PcreatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("PupdatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool InsertFamilySerial(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_FamilySerial", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("Pplant_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Plant.ToUpper().Trim();
                    sc.Parameters.Add("Pfamily_code", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Family.ToUpper().Trim();
                    string stageValue = itemModel.Stage.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("Pstage_id", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();
                    sc.Parameters.Add("Poffline_keycode", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("Pstart_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Start_Serial == null ? null : itemModel.Start_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Pend_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.End_Serial == null ? null : itemModel.End_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Pnumber_sub_assemblies", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.No_SubAssemblies == null ? null : itemModel.No_SubAssemblies.ToUpper().Trim();
                    sc.Parameters.Add("Pbarcode_prefix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Prefix == null ? null : itemModel.Prefix.ToUpper().Trim();
                    sc.Parameters.Add("Psuffix", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Suffix == null ? null : itemModel.Suffix.ToUpper().Trim();
                    sc.Parameters.Add("Pcurrent_serial_number", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Current_Serial == null ? null : itemModel.Current_Serial.ToUpper().Trim();
                    sc.Parameters.Add("Plast_printed_label_date_ti", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.LastPrintedDate == null ? null : itemModel.LastPrintedDate.ToUpper().Trim();
                    sc.Parameters.Add("Pautoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("PcallType", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertFamilySerial";
                    sc.Parameters.Add("PcreatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("PupdatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable GridFamilySerial(ItemModel itemModel)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {

                //query = string.Format(@"SELECT plant_code,family_code,OFFLINE_KEYCODE || '#' || STAGE_ID AS STAGE_DESCRIPTION,start_serial_number,
                //                end_serial_number,number_sub_assemblies,barcode_prefix,suffix,current_serial_number,
                //                TO_CHAR(last_printed_label_date_ti,'DD-MM-YYYY') AS last_printed_label_date_ti,autoid,
                //                CREATEDBY, TO_CHAR(CREATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATEDDATE,UPDATEDBY, TO_CHAR(UPDATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATEDDATE
                //                FROM xxes_family_serial  
                //                WHERE plant_code = '{0}' AND family_code = '{1}'", itemModel.Plant.ToUpper().Trim(), itemModel.Family.ToUpper().Trim());

                query = string.Format(@"SELECT xfs.plant_code,xfs.family_code,xfs.OFFLINE_KEYCODE || '#' || xfs.STAGE_ID AS STAGE_DESCRIPTION,xsm.stage_description DESCRIPTION,xfs.start_serial_number,
                               xfs. end_serial_number,xfs.number_sub_assemblies,xfs.barcode_prefix,xfs.suffix,xfs.current_serial_number,
                                TO_CHAR(xfs.last_printed_label_date_ti,'DD-MM-YYYY') AS last_printed_label_date_ti,xfs.autoid,
                                xfs.CREATEDBY, TO_CHAR(xfs.CREATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATEDDATE,xfs.UPDATEDBY, TO_CHAR(xfs.UPDATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATEDDATE
                                FROM xxes_family_serial xfs
                                inner join xxes_stage_master xsm
                                on xfs.plant_code = xsm.plant_code and xfs.family_code = xsm.family_code and xfs.offline_keycode = xsm.offline_keycode
                                WHERE xfs.plant_code = '{0}' AND xfs.family_code = '{1}'", itemModel.Plant.ToUpper().Trim(), itemModel.Family.ToUpper().Trim());
                dt = returnDataTable(query);
            }

            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return dt;
        }


        public char[] characterArray(string str)
        {
            char[] arr = str.ToCharArray();
            return arr;
        }

        public List<DDLTextValue> Fill_Dcode_ByPlantAndFamily(string plant, string family)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                string query = string.Format(@"SELECT ITEM_CODE || ' # ' || item_description ITEM_CODE FROM xxes_item_master 
                WHERE plant_code='{0}' and family_code='{1}'", plant.Trim(), family.Trim());
                TmpDs = returnDataTable(query);


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {

                        Family.Add(new DDLTextValue
                        {
                            Text = dr["ITEM_CODE"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });

                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public DataTable GridRopesMaster(ItemModel itemModel)
        {

            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {

                query = string.Format(@"SELECT
                           plant_code,
                           family_code,
                           item_dcode,
                           item_desc,
                           start_serialno,
                           end_serialno,
                           current_serialno,
                           TO_CHAR(last_print_date, 'DD-MM-YYYY') AS last_print_date,
                           srno_req,
                           autoid,
                           CREATEDBY, TO_CHAR(CREATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATEDDATE,UPDATEDBY, TO_CHAR(UPDATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATEDDATE
                       FROM
                           xxes_torque_master
                       WHERE
                           plant_code = '{0}'
                           AND family_code = '{1}'", itemModel.Plant.ToUpper().Trim(), itemModel.Family.ToUpper().Trim());

                dt = returnDataTable(query);



            }

            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return dt;

        }

        public bool InsertRopesMaster(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_RopesMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("p_plant_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Plant.ToUpper().Trim();
                    sc.Parameters.Add("p_family_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Family.ToUpper().Trim();
                    string dcodeValue = itemModel.Dcode.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = dcodeValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("p_item_dcode", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("p_item_desc", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

                    sc.Parameters.Add("p_start_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Start_Serial == null ? null : itemModel.Start_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_end_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.End_Serial == null ? null : itemModel.End_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_current_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Current_Serial == null ? null : itemModel.Current_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_last_print_date", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.LastPrintedDate == null ? null : itemModel.LastPrintedDate.ToUpper().Trim();
                    sc.Parameters.Add("p_srno_req", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IsSerialNoRequired == true ? "1" : "0";
                    sc.Parameters.Add("p_autoid", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_calltype", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertRopesMaster";
                    sc.Parameters.Add("p_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("p_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdateRopesMaster(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_RopesMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("p_plant_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_family_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    //string dcodeValue = itemModel.Dcode.Trim();
                    //char[] separators = new char[] { '#' };
                    //string[] subs = dcodeValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("p_item_dcode", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_item_desc", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    sc.Parameters.Add("p_start_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Start_Serial == null ? null : itemModel.Start_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_end_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.End_Serial == null ? null : itemModel.End_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_current_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.Current_Serial == null ? null : itemModel.Current_Serial.ToUpper().Trim();
                    sc.Parameters.Add("p_last_print_date", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.LastPrintedDate == null;
                    sc.Parameters.Add("p_srno_req", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IsSerialNoRequired == true ? "1" : "0";
                    sc.Parameters.Add("p_autoid", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("p_calltype", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateRopesMaster";
                    sc.Parameters.Add("p_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("p_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteRopesMaster(ItemModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_RopesMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("p_plant_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_family_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    sc.Parameters.Add("p_item_dcode", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_item_desc", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    sc.Parameters.Add("p_start_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_end_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_current_serial", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_last_print_date", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_srno_req", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_autoid", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("p_calltype", OracleDbType.Varchar2, ParameterDirection.Input).Value = "DeleteRopesMaster";
                    sc.Parameters.Add("p_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("p_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable GridPlantMaster()
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {

                    query = string.Format(@"Select U_CODE,U_NAME,U_ADD,U_PHONE,U_EMAIL,U_FAX,U_CREATEDBY,TO_CHAR(U_CREATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  U_CREATEDDATE,U_UPDATEDBY,TO_CHAR(U_UPDATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  U_UPDATEDDATE from XXES_Unit_Master where U_Code<>'GU' order by U_Code");
                    dt = returnDataTable(query);
                }

                else
                {

                    query = string.Format(@"Select U_CODE,U_NAME,U_ADD,U_PHONE,U_EMAIL,U_FAX,U_CREATEDBY,TO_CHAR(U_CREATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  U_CREATEDDATE,U_UPDATEDBY,TO_CHAR(U_UPDATEDDATE,'DD-MM-YYYY HH24:MI:SS') AS  U_UPDATEDDATE from XXES_Unit_Master where U_Code='{0}' order by U_Code", Convert.ToString(HttpContext.Current.Session["Login_Unit"]));

                    dt = returnDataTable(query);
                }


            }

            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return dt;

            //DataTable dt = new DataTable();
            //try
            //{
            //    using (OracleCommand sc = new OracleCommand("usp_Crud_PlantMaster", Connection()))
            //    {
            //        ConOpen();
            //        sc.CommandType = CommandType.StoredProcedure;

            //        sc.Parameters.Add("P_U_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_U_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_U_ADD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_U_PHONE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_U_EMAIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_U_FAX", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BindGridPlantMaster";
            //        sc.Parameters.Add("P_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
            //        sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
            //        OracleDataAdapter Oda = new OracleDataAdapter(sc);
            //        Oda.Fill(dt);
            //        //return dt;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //throw; 
            //}
            //finally { ConClose(); }
            //return dt;
        }

        public bool InsertPlantMaster(PlantAndFamily itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_PlantMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_U_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantCode.ToUpper().Trim();
                    sc.Parameters.Add("P_U_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantName.ToUpper().Trim();
                    sc.Parameters.Add("P_U_ADD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantAddress == null ? null : itemModel.PlantAddress.ToUpper().Trim();
                    sc.Parameters.Add("P_U_PHONE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantPhone == null ? null : itemModel.PlantPhone.ToUpper().Trim();
                    sc.Parameters.Add("P_U_EMAIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantEmail == null ? null : itemModel.PlantEmail.ToUpper().Trim();
                    sc.Parameters.Add("P_U_FAX", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantFax == null ? null : itemModel.PlantFax.ToUpper().Trim();
                    sc.Parameters.Add("P_U_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Remarks == null ? null : itemModel.Remarks.ToUpper().Trim();
                    sc.Parameters.Add("P_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertPlantMaster";
                    sc.Parameters.Add("P_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdatePlantMaster(PlantAndFamily itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_PlantMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_U_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantCode.ToUpper().Trim();
                    sc.Parameters.Add("P_U_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantName.ToUpper().Trim();
                    sc.Parameters.Add("P_U_ADD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantAddress == null ? null : itemModel.PlantAddress.ToUpper().Trim();
                    sc.Parameters.Add("P_U_PHONE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantPhone == null ? null : itemModel.PlantPhone.ToUpper().Trim();
                    sc.Parameters.Add("P_U_EMAIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantEmail == null ? null : itemModel.PlantEmail.ToUpper().Trim();
                    sc.Parameters.Add("P_U_FAX", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantFax == null ? null : itemModel.Remarks.ToUpper().Trim();
                    sc.Parameters.Add("P_U_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Remarks == null ? null : itemModel.Remarks.ToUpper().Trim();
                    sc.Parameters.Add("P_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdatePlantMaster";
                    sc.Parameters.Add("P_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool DeletePlantMaster(PlantAndFamily itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("usp_Crud_PlantMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_U_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PlantCode.ToUpper().Trim();
                    sc.Parameters.Add("P_U_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_U_ADD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_U_PHONE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_U_EMAIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_U_FAX", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_U_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeletePlantMaster";
                    sc.Parameters.Add("P_createdBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_updatedBy", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);

            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable GridFamilyData()
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleCommand oc = new OracleCommand("usp_Crud_FamilyMaster", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("f_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_FAMILY_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_NO_OF_STAGES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_NOT_VALIDATE_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BindGridFamilyData";
                    oc.Parameters.Add("f_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                    ConClose();
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return dt;
        }
        public bool InsertFamily(PlantAndFamily plantAndFamily)
        {

            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("usp_Crud_FamilyMaster", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("f_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("f_FAMILY_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.FamilyName.ToUpper().Trim();
                    oc.Parameters.Add("f_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.Description == null ? null : plantAndFamily.Description.ToUpper().Trim();
                    oc.Parameters.Add("f_NO_OF_STAGES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.NoOfStages.ToUpper().Trim();
                    oc.Parameters.Add("f_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.ORGId.ToUpper().Trim();
                    oc.Parameters.Add("f_NOT_VALIDATE_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.NotValidateJob == true ? "Y" : "N";
                    oc.Parameters.Add("f_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertfamilyData";
                    oc.Parameters.Add("f_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("f_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdateFamily(PlantAndFamily plantAndFamily)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("usp_Crud_FamilyMaster", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("f_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("f_FAMILY_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.FamilyName.ToUpper().Trim();
                    oc.Parameters.Add("f_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.Description == null ? null : plantAndFamily.Description.ToUpper().Trim();
                    oc.Parameters.Add("f_NO_OF_STAGES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.NoOfStages.ToUpper().Trim();
                    oc.Parameters.Add("f_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.ORGId.ToUpper().Trim();
                    oc.Parameters.Add("f_NOT_VALIDATE_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.NotValidateJob == true ? "Y" : "N";
                    oc.Parameters.Add("f_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateFamilyData";
                    oc.Parameters.Add("f_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();

                    oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteFamilyMaster(PlantAndFamily plantAndFamily)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("usp_Crud_FamilyMaster", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("f_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = plantAndFamily.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("f_FAMILY_NAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_NO_OF_STAGES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_NOT_VALIDATE_JOB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteFamilyData";
                    oc.Parameters.Add("f_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("f_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        //public List<MappingPlantFamily> Fill_Family_CheckBox()
        //{
        //    DataTable TmpDs = new DataTable();
        //    List<MappingPlantFamily> Family = new List<MappingPlantFamily>();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
        //        {
        //            TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as FAMILY_Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE <>'GU' order by FAMILY_Name");
        //        }
        //        else
        //        {
        //            TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as FAMILY_Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE ='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'order by FAMILY_Name");
        //        }
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                Family.Add(new MappingPlantFamily
        //                {

        //                    FamilyCode = dr["FAMILY_CODE"].ToString(),
        //                    FamilyName = dr["FAMILY_Name"].ToString(),
        //                });
        //            }
        //        }
        //        return Family;
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw;
        //        return Family;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}

        //public bool InsertMappingFamilyPlant(MappingPlantFamily mapping)
        //{
        //    bool result = false;
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(mapping.FamilyCode))
        //        {
        //            string[] familycodes = mapping.FamilyCode.Split(',');
        //            foreach (string family in familycodes)
        //            {
        //                using (OracleCommand oc = new OracleCommand("USP_CRUD_MAPPINGFAMILYTOPLANT", Connection()))
        //                {
        //                    ConOpen();
        //                    oc.CommandType = CommandType.StoredProcedure;
        //                    oc.Parameters.Add("m_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = mapping.PlantCode.ToUpper().Trim();
        //                    oc.Parameters.Add("m_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
        //                    oc.Parameters.Add("m_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
        //                    oc.Parameters.Add("m_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
        //                    oc.Parameters.Add("m_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertMappingFamilyData";
        //                    oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);

        //                    oc.ExecuteNonQuery();
        //                    ConClose();
        //                    result = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //    return result;
        //}

        //public List<DDLTextValue> FillStageIdByFamily(string family)
        //{
        //    DataTable Dt = new DataTable();
        //    List<DDLTextValue> StageId = new List<DDLTextValue>();
        //    try
        //    {
        //        string query = "select No_of_Stages from xxes_family_master where family_code='" + family.Trim() + "'";
        //      Dt = returnDataTable(query);


        //        if (Dt.Rows.Count > 0)
        //        {
        //            var numberStageId = Dt.Rows.Count;
        //            for (int i = 0; i < numberStageId; i++)
        //            {

        //                int cellValue = Convert.ToInt32(Dt.Rows[i]["No_of_Stages"]);


        //            }

        //            foreach (DataRow dr in Dt.AsEnumerable())
        //            {

        //                StageId.Add(new DDLTextValue
        //                {
        //                    Text = dr["stage_description"].ToString(),
        //                    Value = dr["stage_id"].ToString(),
        //                });

        //            }
        //        }
        //        return StageId;
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw;
        //        return StageId;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}

        public List<DDLTextValue> FillStageIdByFamily(string family)
        {
            List<DDLTextValue> StageId = new List<DDLTextValue>();
            int returnValue;
            string command = "select No_of_Stages from xxes_family_master where family_code='" + family.Trim() + "'";
            try
            {

                using (OracleCommand sc = new OracleCommand(command, Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.Text;
                    returnValue = Convert.ToInt32(sc.ExecuteScalar());
                    ConClose();

                    for (int i = 1; i <= returnValue; i++)
                    {
                        StageId.Add(new DDLTextValue
                        {
                            Text = i.ToString(),
                            Value = i.ToString(),
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { ConClose(); }
            return StageId;
        }

        public List<DDLTextValue> FillStageByFamily(string family)
        {
            DataTable dt = new DataTable();
            List<DDLTextValue> Stage = new List<DDLTextValue>();

            try
            {
                if (family.ToUpper().Contains("ENGINE"))
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='EN_STAGES' order by DESCRIPTION");
                }
                else if (family.ToUpper().Equals("TRANSMISSION FTD"))
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='TRSTAGES' order by DESCRIPTION");
                }
                else if (family.ToUpper().Equals("REAR AXEL FTD") || family.ToUpper().Equals("REARAXLE FTD"))
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='RSTAGES' order by DESCRIPTION");
                }
                else if (family.ToUpper().StartsWith("BACKEND") || family.ToUpper().StartsWith("BACK END"))
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='BSTAGES' order by DESCRIPTION");
                }
                else if (family.ToUpper().Contains("CRANE"))
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='CR_STAGES' order by DESCRIPTION");
                }
                else
                {
                    dt = returnDataTable("select PARAMETERINFO Code,PARAMVALUE Text from XXES_SFT_SETTINGS where STATUS='STAGES' order by DESCRIPTION");
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Stage.Add(new DDLTextValue
                        {

                            Value = dr["Code"].ToString(),
                            Text = dr["Text"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return Stage;
        }

        public string getCompletedStages(string plant, string family)
        {
            string stageid = "", lblCompStages = "";
            try
            {
                DataTable dt = returnDataTable("select stage_id from XXES_STAGE_MASTER where PLANT_CODE='" + plant.Trim() + "' and Family_code='" + family.Trim() + "' order by stage_id");
                foreach (DataRow dr in dt.Rows)
                {
                    stageid += Convert.ToString(dr["stage_id"]) + ",";
                }
                if (stageid.EndsWith(","))
                {
                    stageid = stageid.Remove(stageid.Length - 1);
                    lblCompStages = "Existing Stages: " + stageid.Trim();

                }

            }
            catch (Exception ex)
            {

                LogWrite(ex);
            }
            finally { }

            return lblCompStages;
        }


        public void LogWrite(Exception ex)
        {
            try
            {
                //_readWriteLock.EnterWriteLock();
                if (ex.Message != "Thread was being aborted." || ex.Message != "The ConnectionString property has not been initialized.")
                {
                    string DirectoryPath = HttpContext.Current.Server.MapPath("~/LogFiles/");

                    if (!Directory.Exists(DirectoryPath))
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }

                    string FileName = DateTime.Now.ToString("MM-dd-yyyy") + ".txt";
                    string FilePath = DirectoryPath + FileName;
                    using (StreamWriter streamWriter = new StreamWriter(FilePath, true))
                    {
                        streamWriter.WriteLine(DateTime.Now);
                        streamWriter.WriteLine(ex.Message);
                        streamWriter.WriteLine(ex.StackTrace);
                        streamWriter.WriteLine("____________________________________________________________________________________________");
                        streamWriter.WriteLine();
                    }
                }
            }
            catch
            {
            }
            finally
            {
                //_readWriteLock.EnterWriteLock();
            }
        }

        public bool InsertStageMaster(StageMsterModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_STAGEMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PLANT_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.FAMILY_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_ID", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_ID.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_DESCRIPTION.ToUpper().Trim();
                    sc.Parameters.Add("P_PRODUCT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRODUCT_TYPE == null ? null : itemModel.PRODUCT_TYPE.ToUpper().Trim();
                    sc.Parameters.Add("P_START_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.START_STAGE == true ? "1" : "0";
                    sc.Parameters.Add("P_END_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.END_STAGE == true ? "1" : "0";
                    sc.Parameters.Add("P_PRINT_LABEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRINT_LABEL == true ? "1" : "0";
                    sc.Parameters.Add("P_PRINT_LABEL_QUANTITY", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRINT_LABEL_QUANTITY == null ? null : itemModel.PRINT_LABEL_QUANTITY.ToUpper().Trim();
                    sc.Parameters.Add("P_SHOW_STAGE_FORM", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SHOW_STAGE_FORM == true ? "1" : "0";
                    sc.Parameters.Add("P_STAGE_1_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_1_NAME == null ? null : itemModel.STAGE_1_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_2_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_2_NAME == null ? null : itemModel.STAGE_2_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_3_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_3_NAME == null ? null : itemModel.STAGE_3_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_JOB", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_JOB == true ? "1" : "0";
                    sc.Parameters.Add("P_SCAN_JOB_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_JOB_KEY_STOKES == null ? null : itemModel.SCAN_JOB_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SERIAL", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SERIAL == true ? "1" : "0";
                    sc.Parameters.Add("P_SCAN_SERIAL_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SERIAL_KEY_STOKES == null ? null : itemModel.SCAN_SERIAL_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_1 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_1_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_1_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_1_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_2 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_2_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_2_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_2_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_3", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_3 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_3_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_3_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_3_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_ONLINE_SCREEN", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.ONLINE_SCREEN == true ? "1" : "0";
                    sc.Parameters.Add("P_START_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.START_FIFO == true ? "1" : "0";
                    sc.Parameters.Add("P_END_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.END_FIFO == true ? "1" : "0";
                    sc.Parameters.Add("P_MOVE_COMPLETE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.MOVE_COMPLETE == true ? "1" : "0";
                    sc.Parameters.Add("P_DELAY_TIME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.DELAY_TIME == true ? "1" : "0";
                    sc.Parameters.Add("P_ISBIG", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.ISBIG == true ? "1" : "0";
                    sc.Parameters.Add("P_OFFLINE_KEYCODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.OFFLINEITEMS.ToUpper().Trim();
                    sc.Parameters.Add("P_IPADDR", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IPADDR == null ? null : itemModel.IPADDR.ToUpper().Trim();
                    sc.Parameters.Add("P_IPPORT", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IPPORT == null ? null : itemModel.IPPORT.ToUpper().Trim();
                    sc.Parameters.Add("P_AD_USER", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AD_USER == null ? null : itemModel.AD_USER.ToUpper().Trim();
                    sc.Parameters.Add("P_AD_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AD_PASSWORD == null ? null : itemModel.AD_PASSWORD.ToUpper().Trim();
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertStageMaster";
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        string query = string.Empty;
        public string getPrinterIp(string stage, string plant, string family)
        {
            query = string.Format(@"select ipaddr || '#' || ipport from xxes_stage_master where 
                        offline_keycode='{0}' and plant_code='{1}' and family_code='{2}'", stage, plant, family);
            return get_Col_Value(query);
        }
        public string getUserPrinterIp(string user, string plant, string family)
        {
            query = string.Format(@"SELECT SM.REMARKS FROM XXES_STORE_MASTER SM INNER JOIN XXES_USERS_MASTER UM
                                    ON SM.PLANT_CODE = UM.U_CODE AND SM.FAMILY_CODE = UM.FAMILYCODE AND SM.STORENAME = UM.PUNAME
                                    WHERE UM.ISACTIVE = 1 AND UPPER(UM.USRNAME) = '{0}' AND UM.U_CODE = '{1}' AND 
                                    UM.FAMILYCODE = '{2}'", user, plant, family);
            return get_Col_Value(query);
        }
        public bool PrintingEnable(string stage, string plant, string family)
        {
            query = string.Format(@"select count(*)  from xxes_stage_master where 
            print_label=1 and offline_keycode='{0}' and plant_code='{1}' and family_code='{2}'", stage, plant, family);
            return CheckExits(query);
        }
        public bool UpdateStageMaster(StageMsterModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_STAGEMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_ID", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_ID.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_DESCRIPTION.ToUpper().Trim();
                    sc.Parameters.Add("P_PRODUCT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRODUCT_TYPE == null ? null : itemModel.PRODUCT_TYPE.ToUpper().Trim();
                    sc.Parameters.Add("P_START_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.START_STAGE == true ? "1" : "0";
                    sc.Parameters.Add("P_END_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.END_STAGE == true ? "1" : "0";
                    sc.Parameters.Add("P_PRINT_LABEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRINT_LABEL == true ? "1" : "0";
                    sc.Parameters.Add("P_PRINT_LABEL_QUANTITY", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRINT_LABEL_QUANTITY == null ? null : itemModel.PRINT_LABEL_QUANTITY.ToUpper().Trim();
                    sc.Parameters.Add("P_SHOW_STAGE_FORM", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SHOW_STAGE_FORM == true ? "1" : "0";
                    sc.Parameters.Add("P_STAGE_1_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_1_NAME == null ? null : itemModel.STAGE_1_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_2_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_2_NAME == null ? null : itemModel.STAGE_2_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGE_3_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.STAGE_3_NAME == null ? null : itemModel.STAGE_3_NAME.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_JOB", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_JOB == true ? "1" : "0";
                    sc.Parameters.Add("P_SCAN_JOB_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_JOB_KEY_STOKES == null ? null : itemModel.SCAN_JOB_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SERIAL", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SERIAL == true ? "1" : "0";
                    sc.Parameters.Add("P_SCAN_SERIAL_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SERIAL_KEY_STOKES == null ? null : itemModel.SCAN_SERIAL_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_1 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_1_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_1_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_1_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_2 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_2_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_2_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_2_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_3", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_3 == true ? "1" : "0";
                    sc.Parameters.Add("P_SCANSUB_ASSEM_3_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.SCAN_SUB_ASSEMBLY_3_KEY_STOKES == null ? null : itemModel.SCAN_SUB_ASSEMBLY_3_KEY_STOKES.ToUpper().Trim();
                    sc.Parameters.Add("P_ONLINE_SCREEN", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.ONLINE_SCREEN == true ? "1" : "0";
                    sc.Parameters.Add("P_START_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.START_FIFO == true ? "1" : "0";
                    sc.Parameters.Add("P_END_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.END_FIFO == true ? "1" : "0";
                    sc.Parameters.Add("P_MOVE_COMPLETE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.MOVE_COMPLETE == true ? "1" : "0";
                    sc.Parameters.Add("P_DELAY_TIME", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.DELAY_TIME == true ? "1" : "0";
                    sc.Parameters.Add("P_ISBIG", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.ISBIG == true ? "1" : "0";
                    sc.Parameters.Add("P_OFFLINE_KEYCODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_IPADDR", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IPADDR == null ? null : itemModel.IPADDR.ToUpper().Trim();
                    sc.Parameters.Add("P_IPPORT", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.IPPORT == null ? null : itemModel.IPPORT.ToUpper().Trim();
                    sc.Parameters.Add("P_AD_USER", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AD_USER == null ? null : itemModel.AD_USER.ToUpper().Trim();
                    sc.Parameters.Add("P_AD_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AD_PASSWORD == null ? null : itemModel.AD_PASSWORD.ToUpper().Trim();
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateStageMaster";
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim();
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteStageMaster(StageMsterModel itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_STAGEMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_ID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PRODUCT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRODUCT_TYPE == null;
                    sc.Parameters.Add("P_START_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.START_STAGE == null;
                    sc.Parameters.Add("P_END_STAGE", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.END_STAGE == null;
                    sc.Parameters.Add("P_PRINT_LABEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.PRINT_LABEL == null;
                    sc.Parameters.Add("P_PRINT_LABEL_QUANTITY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SHOW_STAGE_FORM", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_1_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_2_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_STAGE_3_NAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_JOB", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_JOB_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_SERIAL", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_SERIAL_KEY_STOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCANSUB_ASSEM_1_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCANSUB_ASSEM_2_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCAN_SUB_ASSEMBLY_3", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SCANSUB_ASSEM_3_KEYSTOKES", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ONLINE_SCREEN", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_START_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_END_FIFO", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_MOVE_COMPLETE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_DELAY_TIME", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ISBIG", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_OFFLINE_KEYCODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_IPADDR", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_IPPORT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AD_USER", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AD_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = itemModel.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "DeleteStageMaster";
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable GridStageMaster(StageMsterModel itemModel)
        {
            DataTable dt = new DataTable();

            try
            {

                dt = returnDataTable(@"Select PLANT_CODE,FAMILY_CODE,STAGE_ID,STAGE_DESCRIPTION,PRODUCT_TYPE,START_STAGE,END_STAGE,
                    PRINT_LABEL,PRINT_LABEL_QUANTITY,SHOW_STAGE_FORM,STAGE_1_NAME,STAGE_2_NAME,STAGE_3_NAME,SCAN_JOB,SCAN_JOB_KEY_STOKES,
                    SCAN_SERIAL,SCAN_SERIAL_KEY_STOKES,SCAN_SUB_ASSEMBLY_1,SCAN_SUB_ASSEMBLY_1_KEY_STOKES,SCAN_SUB_ASSEMBLY_2,
                    SCAN_SUB_ASSEMBLY_2_KEY_STOKES,SCAN_SUB_ASSEMBLY_3,SCAN_SUB_ASSEMBLY_3_KEY_STOKES,ONLINE_SCREEN,START_FIFO,END_FIFO,
                    MOVE_COMPLETE,DELAY_TIME,ISBIG,OFFLINE_KEYCODE,IPADDR,IPPORT,AD_USER,AD_PASSWORD,CREATED_BY,TO_CHAR(CREATED_DATE,
                    'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE,
                    AUTOID from XXES_STAGE_MASTER where Plant_Code='" + itemModel.PLANT_CODE.ToUpper().Trim() + "' and Family_Code = '" + itemModel.FAMILY_CODE.ToUpper().Trim() + "' order by Plant_Code,Family_Code,Stage_ID");
                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //    dt = returnDataTable("Select PLANT_CODE,FAMILY_CODE,STAGE_ID,STAGE_DESCRIPTION,PRODUCT_TYPE,START_STAGE,END_STAGE,PRINT_LABEL,PRINT_LABEL_QUANTITY,SHOW_STAGE_FORM,STAGE_1_NAME,STAGE_2_NAME,STAGE_3_NAME,SCAN_JOB,SCAN_JOB_KEY_STOKES,SCAN_SERIAL,SCAN_SERIAL_KEY_STOKES,SCAN_SUB_ASSEMBLY_1,SCAN_SUB_ASSEMBLY_1_KEY_STOKES,SCAN_SUB_ASSEMBLY_2,SCAN_SUB_ASSEMBLY_2_KEY_STOKES,SCAN_SUB_ASSEMBLY_3,SCAN_SUB_ASSEMBLY_3_KEY_STOKES,ONLINE_SCREEN,START_FIFO,END_FIFO,MOVE_COMPLETE,DELAY_TIME,ISBIG,OFFLINE_KEYCODE,IPADDR,IPPORT,AD_USER,AD_PASSWORD,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE,AUTOID from XXES_STAGE_MASTER order by Plant_Code,Family_Code,Stage_ID");

                //else
                //    dt = returnDataTable("Select PLANT_CODE,FAMILY_CODE,STAGE_ID,STAGE_DESCRIPTION,PRODUCT_TYPE,START_STAGE,END_STAGE,PRINT_LABEL,PRINT_LABEL_QUANTITY,SHOW_STAGE_FORM,STAGE_1_NAME,STAGE_2_NAME,STAGE_3_NAME,SCAN_JOB,SCAN_JOB_KEY_STOKES,SCAN_SERIAL,SCAN_SERIAL_KEY_STOKES,SCAN_SUB_ASSEMBLY_1,SCAN_SUB_ASSEMBLY_1_KEY_STOKES,SCAN_SUB_ASSEMBLY_2,SCAN_SUB_ASSEMBLY_2_KEY_STOKES,SCAN_SUB_ASSEMBLY_3,SCAN_SUB_ASSEMBLY_3_KEY_STOKES,ONLINE_SCREEN,START_FIFO,END_FIFO,MOVE_COMPLETE,DELAY_TIME,ISBIG,OFFLINE_KEYCODE,IPADDR,IPPORT,AD_USER,AD_PASSWORD,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE,AUTOID from XXES_STAGE_MASTER where Plant_Code='" + itemModel.PLANT_CODE.ToUpper().Trim() + "' and Family_Code = '" + itemModel.FAMILY_CODE.ToUpper().Trim() + "' order by Plant_Code,Family_Code,Stage_ID");

            }

            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return dt;
        }

        //*************************CraneSetting Function*****************************//

        public DataTable GridCraneSetting(string Type, string Plant)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {

                if (Type == "CRMONTH" || Type == "CRMONTHQR")
                {
                    query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SUFFIX_CODE where type='" + Type.ToUpper().Trim() + "' and Plant= '" + Plant.ToUpper().Trim() + "'");
                    dt = returnDataTable(query);
                    //string query = "select * from XXES_SUFFIX_CODE where type='" + Type.Trim() + "' and Plant= '" + Plant.Trim() + "'";
                    //dt = returnDataTable(query);
                }
                else if (Type == "CRYEAR")
                {
                    query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SUFFIX_CODE where type='CRYEAR' and plant='' order by TO_DATE('01' || '-' || MON_YYYY,'DD-MM-YYYY') desc");
                    dt = returnDataTable(query);
                    //string query = "select * from XXES_SUFFIX_CODE where type='CRYEAR' and plant='' order by TO_DATE('01' || '-' || MON_YYYY,'DD-MM-YYYY') desc";
                    //dt = returnDataTable(query);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }

            return dt;
        }

        public bool InsertCraneSetting(CraneSetting crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Month.ToUpper().Trim();
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Code.ToUpper().Trim();
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Type.ToUpper().Trim();
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Plant.ToUpper().Trim();
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }

        public bool DeleteCraneSetting(CraneSetting crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.AutoId;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }

        public DataTable GridCraneSettingYear(string PlantYear)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SUFFIX_CODE where type='CRYEAR' and Plant= '" + PlantYear.ToUpper().Trim() + "' ");
                dt = returnDataTable(query);
                //string query = "select * from XXES_SUFFIX_CODE where type='CRYEAR' and Plant= '" + PlantYear.Trim() + "' ";
                //dt = returnDataTable(query);
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }

            return dt;
        }


        public bool InsertCraneSettingYear(CraneSetting crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.MonthYear.ToUpper().Trim();
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.CodeYear.ToUpper().Trim();
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "CRYEAR";
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.PlantYear.ToUpper().Trim();
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }

        public bool DeleteCraneSettingYear(CraneSetting crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.AutoId;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }

        //*************************CraneMaster Function*****************************//

        public List<DDLTextValue> Fill_Crane_Name(string PlantCode, string FamilyCode)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(PlantCode.ToUpper().Trim()), Convert.ToString(FamilyCode.ToUpper().Trim()));
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> Crane = new List<DDLTextValue>();

                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 2) in ('CF') order by segment1");
                //TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%BACKEND%' order by segment1");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Crane.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return Crane;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        // created by rajesh for merging on 31-Mar-2021
        public List<DDLTextValue> Fill_Engine_Name(string PlantCode, string FamilyCode)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(PlantCode.ToUpper().Trim()), Convert.ToString(FamilyCode.ToUpper().Trim()));
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> Engine = new List<DDLTextValue>();

                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('C','S') and segment1 not like 'CF%' order by segment1");
                //TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%BACKEND%' order by segment1");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Engine.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return Engine;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public DataTable GridCraneMaster(CraneMapping crane)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_CRANEMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("c_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.PlantCode.ToUpper().Trim();
                    oc.Parameters.Add("c_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("c_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_MODEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SHORTNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_ENGINE_DCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_ENGINE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_PREFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_PREFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SUFFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SUFFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BindGridCraneMaster";
                    oc.Parameters.Add("c_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    ConClose();
                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return dt;
        }

        public bool InsertCraneMaster(CraneMapping crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_CRANEMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    //oc.Parameters.Add("c_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.PlantCode.ToUpper().Trim();
                    oc.Parameters.Add("c_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("c_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ItemCode == null ? null : crane.ItemCode.ToUpper().Trim();
                    oc.Parameters.Add("c_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.DesCription == null ? null : crane.DesCription.ToUpper().Trim();

                    oc.Parameters.Add("c_MODEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Model == null ? null : crane.Model.ToUpper().Trim();
                    oc.Parameters.Add("c_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ModelType.ToUpper().Trim();
                    oc.Parameters.Add("c_SHORTNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ShortName == null ? null : crane.ShortName.ToUpper().Trim();
                    oc.Parameters.Add("c_ENGINE_DCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.EngineDcode.ToUpper().Trim();
                    oc.Parameters.Add("c_ENGINE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.EngineDesc == null ? null : crane.EngineDesc.ToUpper().Trim();
                    oc.Parameters.Add("c_PREFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Prefix1 == null ? null : crane.Prefix1.ToUpper().Trim();
                    oc.Parameters.Add("c_PREFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Prefix2 == null ? null : crane.Prefix2.ToUpper().Trim();
                    oc.Parameters.Add("c_SUFFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Suffix1 == null ? null : crane.Suffix1.ToUpper().Trim();
                    oc.Parameters.Add("c_SUFFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Suffix2 == null ? null : crane.Suffix2.ToUpper().Trim();
                    oc.Parameters.Add("c_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Remarks1 == null ? null : crane.Remarks1.ToUpper().Trim();
                    oc.Parameters.Add("c_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("c_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertCraneMaster";
                    oc.Parameters.Add("c_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdateCraneMaster(CraneMapping crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_CRANEMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("c_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.PlantCode == null ? null : crane.PlantCode.ToUpper().Trim();
                    oc.Parameters.Add("c_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.FamilyCode == null ? null : crane.FamilyCode.ToUpper().Trim();
                    oc.Parameters.Add("c_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ItemCode == null ? null : crane.ItemCode.ToUpper().Trim();
                    oc.Parameters.Add("c_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.DesCription == null ? null : crane.DesCription.ToUpper().Trim();

                    oc.Parameters.Add("c_MODEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Model == null ? null : crane.Model.ToUpper().Trim();
                    oc.Parameters.Add("c_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ModelType == null ? null : crane.ModelType.ToUpper().Trim();
                    oc.Parameters.Add("c_SHORTNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ShortName == null ? null : crane.ShortName.ToUpper().Trim();
                    oc.Parameters.Add("c_ENGINE_DCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.EngineDcode == null ? null : crane.EngineDcode.ToUpper().Trim();
                    oc.Parameters.Add("c_ENGINE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.EngineDesc == null ? null : crane.EngineDesc.ToUpper().Trim();
                    oc.Parameters.Add("c_PREFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Prefix1 == null ? null : crane.Prefix1.ToUpper().Trim();
                    oc.Parameters.Add("c_PREFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Prefix2 == null ? null : crane.Prefix2.ToUpper().Trim();
                    oc.Parameters.Add("c_SUFFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Suffix1 == null ? null : crane.Suffix1.ToUpper().Trim();
                    oc.Parameters.Add("c_SUFFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Suffix2 == null ? null : crane.Suffix2.ToUpper().Trim();
                    oc.Parameters.Add("c_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.Remarks1 == null ? null : crane.Remarks1.ToUpper().Trim();
                    oc.Parameters.Add("c_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("c_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateCraneMaster";
                    oc.Parameters.Add("c_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }

        public bool DeleteCraneMaster(CraneMapping crane)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_CRANEMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    //oc.Parameters.Add("c_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.AutoId;
                    oc.Parameters.Add("c_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.PlantCode;
                    oc.Parameters.Add("c_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.FamilyCode;
                    oc.Parameters.Add("c_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = crane.ItemCode;
                    oc.Parameters.Add("c_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_MODEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SHORTNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_ENGINE_DCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_ENGINE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_PREFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_PREFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SUFFIX1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_SUFFIX2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("c_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteCraneMaster";
                    oc.Parameters.Add("c_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }
            return result;
        }


        //*************************OtherMaster Function*****************************//

        public DataTable GridOtherMaster(string Type, string Plant)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {

                if (Type == "DOMESTIC" || Type == "QRDOMESTIC")
                {
                    query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SUFFIX_CODE where type='" + Type.ToUpper().Trim() + "' and Plant= '" + Plant.ToUpper().Trim() + "'");
                    dt = returnDataTable(query);


                }
                else if (Type == "EXPORT")
                {
                    query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID  from XXES_SUFFIX_CODE where type='EXPORT' and plant='' order by TO_DATE('01' || '-' || MON_YYYY,'DD-MM-YYYY') desc");
                    dt = returnDataTable(query);

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }

            return dt;
        }
        public bool InsertOtherSuffix(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.MonthYear.ToUpper().Trim();
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.MyCode.ToUpper().Trim();
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.Type.ToUpper().Trim();
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.Plant.ToUpper().Trim();
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
            return result;
        }

        public bool DeleteOtherSuffix(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.AutoId;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
            return result;
        }

        public DataTable GridOtherMasterExport(string PlantExport)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"select MON_YYYY,MY_CODE,TYPE,PLANT,CREATED_BY,TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,UPDATED_DATE,AUTOID from XXES_SUFFIX_CODE where type='EXPORT' and Plant= '" + PlantExport.ToUpper().Trim() + "' ");
                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { }

            return dt;
        }

        public bool InsertOtherSuffixExport(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.MonthYearExport.ToUpper().Trim();
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.MyCodeExport.ToUpper().Trim();
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "EXPORT";
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.PlantExport.ToUpper().Trim();
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
            return result;
        }

        public bool DeleteOtherSuffixExport(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_OTHERSUFFIXCODE", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("o_MON_YYYY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_MY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("o_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteOtherSuffixCode";
                    oc.Parameters.Add("o_Autoid", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.AutoId;

                    oc.Parameters.Add("o_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally { }
            return result;
        }

        public DataTable GridShiftDetail()
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESSHIFTMASTER", Connection()))
                {
                    con.Open();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("s_SHIFTCODE ", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_START_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_END_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_NIGHT_EXISTS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BindGridShiftMaster";
                    oc.Parameters.Add("s_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                    con.Close();
                }
            }
            catch (Exception ex)
            {

                LogWrite(ex);
            }
            finally
            {
                
            }
            return dt;
        }

        public bool InsertShiftDetail(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESSHIFTMASTER", Connection()))
                {
                    con.Open();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("s_SHIFTCODE ", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.ShiftCode.ToUpper().Trim();
                    oc.Parameters.Add("s_START_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.StartTime;
                    oc.Parameters.Add("s_END_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.EndTime;
                    oc.Parameters.Add("s_NIGHT_EXISTS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.NightExist == true ? "1" : "0";
                    oc.Parameters.Add("s_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("s_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertShiftMaster";
                    oc.Parameters.Add("s_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                   
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally { con.Close(); }
            return result;
        }

        public bool DeleteShiftDetail(OtherSuffix suffix)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESSHIFTMASTER", Connection()))
                {
                    con.Open();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("s_SHIFTCODE ", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_START_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_END_TIME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_NIGHT_EXISTS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = suffix.AutoId;
                    oc.Parameters.Add("s_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("s_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteShiftMaster";
                    oc.Parameters.Add("s_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    //con.Close();
                    result = true;

                }
            }
            catch (Exception ex)
            {

                LogWrite(ex);
            }
            finally { con.Close(); }
            return result;
        }

        public string GetOrgId(string PlantBattery)
        {
            return (PlantBattery == "T04" ? "149" : "150");
        }
        public List<DDLTextValue> Fill_BatteryDeCode_Name(string PlantBattery)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.GetOrgId(Convert.ToString(PlantBattery.ToUpper().Trim()));
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> BatteryDeCode = new List<DDLTextValue>();

                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description  as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items" + " where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','X') and DESCRIPTION " + " like 'BATTERY ASSEMBLY%' order by segment1");

                //TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 2) in ('CF') order by segment1");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        BatteryDeCode.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return BatteryDeCode;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }


        //*************************MappingFamillyToPlant Function*****************************//

        public List<DDLTextValue> Fill_Plant_Family_Name()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> PlantFamily = new List<DDLTextValue>();
                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    TmpDs = returnDataTable("Select U_Name || ' # ' || U_Code as  Unit_Name,U_Code from XXES_Unit_Master where U_Code<>'GU' order by U_Name");
                }
                else
                {
                    TmpDs = returnDataTable("Select U_Name || ' # ' || U_Name as   Unit_Name,U_Code from XXES_Unit_Master where U_Code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by U_Name");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        PlantFamily.Add(new DDLTextValue
                        {
                            Text = dr["Unit_Name"].ToString(),
                            Value = dr["U_Code"].ToString(),
                        });
                    }
                }
                return PlantFamily;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> Fill_Family_CheckBox()
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as FAMILY_CODE,FAMILY_Name from XXES_FAMILY_MASTER where FAMILY_CODE <>'GU' order by FAMILY_Name");
                }
                else
                {
                    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as FAMILY_CODE,FAMILY_Name from XXES_FAMILY_MASTER where FAMILY_CODE ='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'order by FAMILY_Name");
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["FAMILY_CODE"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                ConClose();
            }
        }

        public DataTable GridFamilyPlantData()
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_MAPPINGFAMILYTOPLANT", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("m_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("m_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("m_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("m_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("m_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "BindGridMappingFamily";
                    oc.Parameters.Add("m_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;
        }
        public bool ValidatePlantFamily(MappingFamilyToPlant mapping, out string valresult)
        {
            bool exist = false;
            valresult = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(mapping.FamilyCode))
                {
                    string[] familycodes = mapping.FamilyCode.Split(',');
                    foreach (string family in familycodes)
                    {
                        string query = string.Format(@"select plant_code from XXES_PLANT_FAMILY_MAP where family_code='{0}' and Plant_code ='{1}'", family.Trim(), mapping.PlantCode.Trim());
                        string exisintplant = get_Col_Value(query);
                        if (!string.IsNullOrEmpty(exisintplant))
                        {
                            exist = true;
                            valresult = string.Format("{0} already mapped with plant {1}", family, exisintplant);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            valresult = "Family Already Exist";
            return exist;
        }
        public bool InsertMappingFamilyPlant(MappingFamilyToPlant mapping)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(mapping.FamilyCode))
                {
                    string[] familycodes = mapping.FamilyCode.Split(',');
                    foreach (string family in familycodes)
                    {
                        using (OracleCommand oc = new OracleCommand("USP_CRUD_MAPPINGFAMILYTOPLANT", Connection()))
                        {
                            ConOpen();
                            oc.CommandType = CommandType.StoredProcedure;
                            oc.Parameters.Add("m_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = mapping.PlantCode.ToUpper().Trim();
                            oc.Parameters.Add("m_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = family.ToUpper().Trim();
                            oc.Parameters.Add("m_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                            oc.Parameters.Add("m_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                            oc.Parameters.Add("m_calltype", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertMappingFamilyData";
                            oc.Parameters.Add("f_res", OracleDbType.RefCursor, ParameterDirection.Output);

                            oc.ExecuteNonQuery();
                            ConClose();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        // created by raj for merging on 31-Mar-2021
        //public List<DDLTextValue> Fill_Engine_Name(EngineModel obj)
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        Function fun = new Function();
        //        string query = string.Empty;
        //        string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

        //        List<DDLTextValue> Engine = new List<DDLTextValue>();

        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                Engine.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return Engine;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_Fual_Pump()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> FUALPUMP = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'FUEL PUMP%'  order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                FUALPUMP.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return FUALPUMP;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_Cylinder_Block()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> CYLINDER_BLOCK = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'Cylinder Block%' order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                CYLINDER_BLOCK.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return CYLINDER_BLOCK;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}

        //public List<DDLTextValue> Fill_Cylinder_Head()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> CYLINDER_HEAD = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Cylinder Head%' order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                CYLINDER_HEAD.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return CYLINDER_HEAD;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}

        //public List<DDLTextValue> Fill_Connecting_Rod()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> CONNECTING_ROD = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Connecting Rod%'  order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                CONNECTING_ROD.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return CONNECTING_ROD;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}

        //public List<DDLTextValue> Fill_Crack_Shaft()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> CRACK_SHAFT = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Crack Shaft%' order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                CRACK_SHAFT.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return CRACK_SHAFT;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_Cam_Shaft()
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        List<DDLTextValue> CAM_SHAFT = new List<DDLTextValue>();
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
        //        //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Cam Shaft%' order by segment1");
        //        TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                CAM_SHAFT.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return CAM_SHAFT;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //        //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public bool InsertEngineMaster(EngineModel EM)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleCommand sc = new OracleCommand("USP_CRUD_EngineMaster", Connection()))
        //        {
        //            ConOpen();
        //            sc.CommandType = CommandType.StoredProcedure;
        //            sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PLANT_CODE) ? null : EM.PLANT_CODE.ToUpper().Trim();
        //            sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FAMILY_CODE) ? null : EM.FAMILY_CODE.ToUpper().Trim();


        //            sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.Engine.Trim().ToUpper();
        //            string Engine_DescValue = EM.Engine_Desc.Trim();
        //            char[] separators = new char[] { '#' };
        //            string[] subs = Engine_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //            sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

        //            if (!string.IsNullOrEmpty(EM.CYLINDER_BLOCK) && !string.IsNullOrEmpty(EM.CYLINDER_BLOCK_DESC))
        //            {
        //                sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CYLINDER_BLOCK.ToUpper().Trim();
        //                string CylinderBlock_DescValue = EM.CYLINDER_BLOCK_DESC.Trim();
        //                char[] separators1 = new char[] { '#' };
        //                string[] subs1 = CylinderBlock_DescValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
        //                sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }

        //            if (!string.IsNullOrEmpty(EM.CYLINDER_HEAD) && !string.IsNullOrEmpty(EM.CYLINDER_HEAD_DESC))
        //            {
        //                sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CYLINDER_HEAD.ToUpper().Trim();
        //                string CylinderHead_DescValue = EM.CYLINDER_HEAD_DESC.Trim();
        //                char[] separators2 = new char[] { '#' };
        //                string[] subs2 = CylinderHead_DescValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);

        //                sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }

        //            if (!string.IsNullOrEmpty(EM.CONNECTING_ROD) && !string.IsNullOrEmpty(EM.CONNECTING_ROD_DESC))
        //            {
        //                sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CONNECTING_ROD.ToUpper().Trim();
        //                string ConnectingRod_DescValue = EM.CONNECTING_ROD_DESC.Trim();
        //                char[] separators3 = new char[] { '#' };
        //                string[] subs3 = ConnectingRod_DescValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);

        //                sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }

        //            if (!string.IsNullOrEmpty(EM.CRANK_SHAFT) && !string.IsNullOrEmpty(EM.CRANK_SHAFT_DESC))
        //            {
        //                sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CRANK_SHAFT.ToUpper().Trim();
        //                string CrankShaft_DescValue = EM.CRANK_SHAFT_DESC.Trim();
        //                char[] separators4 = new char[] { '#' };
        //                string[] subs4 = CrankShaft_DescValue.Split(separators4, StringSplitOptions.RemoveEmptyEntries);

        //                sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

        //                sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }

        //            if (!string.IsNullOrEmpty(EM.CAM_SHAFT) && !string.IsNullOrEmpty(EM.CAM_SHAFT_DESC))
        //            {
        //                sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CAM_SHAFT.ToUpper().Trim();
        //                string CamShaft_DescValue = EM.CAM_SHAFT_DESC.Trim();
        //                char[] separators5 = new char[] { '#' };
        //                string[] subs5 = CamShaft_DescValue.Split(separators5, StringSplitOptions.RemoveEmptyEntries);

        //                sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }


        //            sc.Parameters.Add("P_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.INJECTOR) ? null : EM.INJECTOR.ToUpper().Trim();
        //            sc.Parameters.Add("P_REQUIRE_CYNLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_BLOCK == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CYNLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_HEAD == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CONNECTING_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CONNECTING_ROD == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CRANK_SHAFT == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
        //            sc.Parameters.Add("P_PREFIX_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_1) ? null : EM.PREFIX_1.ToUpper().Trim();
        //            sc.Parameters.Add("P_PREFIX_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_2) ? null : EM.PREFIX_2.ToUpper().Trim();
        //            sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REMARKS1) ? null : EM.REMARKS1.ToUpper().Trim();
        //            sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP) ? null : EM.FUEL_INJECTION_PUMP.ToUpper().Trim();
        //            sc.Parameters.Add("P_REQ_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REQ_FUEL_INJECTION_PUMP) ? null : EM.REQ_FUEL_INJECTION_PUMP.ToUpper().Trim();
        //            sc.Parameters.Add("P_NO_OF_PISTONS", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_PISTONS) ? null : EM.NO_OF_PISTONS.ToUpper().Trim();
        //            sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP_DESC) ? null : EM.FUEL_INJECTION_PUMP_DESC.ToUpper().Trim();
        //            sc.Parameters.Add("P_NO_OF_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_INJECTORS) ? null : EM.NO_OF_INJECTORS.ToUpper().Trim();
        //            sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();

        //            sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertEngineMaster";
        //            sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
        //            sc.ExecuteNonQuery();
        //            //ConClose();
        //            result = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //    return result;


        //}
        //public DataTable GridEngineMaster(EngineModel EM)
        //{
        //    DataTable dt = new DataTable();
        //    string query = string.Empty;
        //    try
        //    {
        //        query = "select *  from XXES_ENGINE_MASTER";
        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //   
        //        //else
        //        //    query = string.Format(@"select *  from XXES_ENGINE_MASTER where PLANT_CODE ='" +Convert.ToString(EM.PLANT_CODE).ToUpper().Trim() + "' and FAMILY_CODE = '" + Convert.ToString(EM.FAMILY_CODE).ToUpper().Trim() + "'   order by s.FAMILY_CODE");

        //        dt = returnDataTable(query);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //    }
        //    finally
        //    {

        //    }
        //    return dt;

        //}
        //public bool UpdateEngineMaster(EngineModel EM)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleCommand sc = new OracleCommand("USP_CRUD_EngineMaster", Connection()))
        //        {
        //            ConOpen();
        //            sc.CommandType = CommandType.StoredProcedure;
        //            sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

        //            sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.ITEM_CODE) ? null : EM.ITEM_CODE.ToUpper().Trim();
        //            sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.ITEM_DESC) ? null : EM.ITEM_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CYLINDER_BLOCK) ? null : EM.CYLINDER_BLOCK.ToUpper().Trim();
        //            sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CYLINDER_BLOCK_DESC) ? null : EM.CYLINDER_BLOCK_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CYLINDER_HEAD) ? null : EM.CYLINDER_HEAD.ToUpper().Trim();
        //            sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CYLINDER_HEAD_DESC) ? null : EM.CYLINDER_HEAD_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CONNECTING_ROD) ? null : EM.CONNECTING_ROD.ToUpper().Trim();
        //            sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CONNECTING_ROD_DESC) ? null : EM.CONNECTING_ROD_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CRANK_SHAFT) ? null : EM.CRANK_SHAFT.ToUpper().Trim();
        //            sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CRANK_SHAFT_DESC) ? null : EM.CRANK_SHAFT_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CAM_SHAFT) ? null : EM.CAM_SHAFT.ToUpper().Trim();
        //            sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.CAM_SHAFT_DESC) ? null : EM.CAM_SHAFT_DESC.ToUpper().Trim();

        //            sc.Parameters.Add("P_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.INJECTOR) ? null : EM.INJECTOR.ToUpper().Trim();
        //            sc.Parameters.Add("P_REQUIRE_CYNLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_BLOCK == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CYNLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_HEAD == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CONNECTING_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CONNECTING_ROD == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CRANK_SHAFT == true ? "1" : "0";
        //            sc.Parameters.Add("P_REQUIRE_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
        //            sc.Parameters.Add("P_PREFIX_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_1) ? null : EM.PREFIX_1.ToUpper().Trim();
        //            sc.Parameters.Add("P_PREFIX_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_2) ? null : EM.PREFIX_2.ToUpper().Trim();
        //            sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REMARKS1) ? null : EM.REMARKS1.ToUpper().Trim();
        //            sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP) ? null : EM.FUEL_INJECTION_PUMP.ToUpper().Trim();
        //            sc.Parameters.Add("P_REQ_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REQ_FUEL_INJECTION_PUMP) ? null : EM.REQ_FUEL_INJECTION_PUMP.ToUpper().Trim();
        //            sc.Parameters.Add("P_NO_OF_PISTONS", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_PISTONS) ? null : EM.NO_OF_PISTONS.ToUpper().Trim();
        //            sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP_DESC) ? null : EM.FUEL_INJECTION_PUMP_DESC.ToUpper().Trim();
        //            sc.Parameters.Add("P_NO_OF_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_INJECTORS) ? null : EM.NO_OF_INJECTORS.ToUpper().Trim();
        //            sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.AUTOID) ? null : EM.AUTOID.ToUpper().Trim();
        //            sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
        //            sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateEngineMaster";
        //            sc.Parameters.Add("p_res", OracleDbType.RefCursor, ParameterDirection.Output);

        //            sc.ExecuteNonQuery();
        //            ConClose();
        //            result = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //    return result;
        //}

        ///// <summary>
        ////All DDL, SAVE GRID AND UPDATE FOR BACKEND MAPPING
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>

        //public List<DDLTextValue> Fill_BACKEND(BackendModel obj)
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        Function fun = new Function();
        //        string query = string.Empty;
        //        string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
        //        List<DDLTextValue> BACKEND = new List<DDLTextValue>();
        //        TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%BACKEND%' order by segment1");


        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                BACKEND.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //            //}
        //        }
        //        return BACKEND;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_HYDRAULIC(BackendModel obj)
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        Function fun = new Function();
        //        string query = string.Empty;
        //        string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
        //        List<DDLTextValue> HYDRAULIC = new List<DDLTextValue>();
        //        TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%HYDRAULIC LIFT ASSEMBLY%' order by segment1");


        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                HYDRAULIC.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return HYDRAULIC;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_Transmission(BackendModel obj)
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        Function fun = new Function();
        //        string query = string.Empty;
        //        string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
        //        List<DDLTextValue> TRANSMISSION = new List<DDLTextValue>();
        //        TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in " + orgid + " and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149','150') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in  ('" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                TRANSMISSION.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return TRANSMISSION;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public List<DDLTextValue> Fill_RearAxel(BackendModel obj)
        //{
        //    DataTable TmpDs = null;
        //    try
        //    {
        //        Function fun = new Function();
        //        string query = string.Empty;
        //        string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
        //        string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
        //        List<DDLTextValue> REARAXEL = new List<DDLTextValue>();
        //        TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%AXLE ASSEMBLY%' order by segment1");

        //        //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
        //        //{
        //        //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149','150') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

        //        //}
        //        //else
        //        //{
        //        //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in  ('" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");
        //        //}
        //        if (TmpDs.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in TmpDs.AsEnumerable())
        //            {
        //                REARAXEL.Add(new DDLTextValue
        //                {
        //                    Text = dr["DESCRIPTION"].ToString(),
        //                    Value = dr["ITEM_CODE"].ToString(),
        //                });
        //            }
        //        }
        //        return REARAXEL;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //}
        //public bool InsertBackendMaster(BackendModel BM)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleCommand sc = new OracleCommand("USP_CRUD_BACKENDMASTER", Connection()))
        //        {
        //            ConOpen();
        //            sc.CommandType = CommandType.StoredProcedure;
        //            sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.PLANT_CODE.ToUpper().Trim();
        //            sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.FAMILY_CODE.ToUpper().Trim();

        //            if (!string.IsNullOrEmpty(BM.BACKEND) && !string.IsNullOrEmpty(BM.BACKEND_DESC))
        //            {
        //                sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.BACKEND.ToUpper().Trim();
        //                string BackendValue = BM.BACKEND_DESC.Trim();
        //                char[] separators = new char[] { '#' };
        //                string[] subs = BackendValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //                sc.Parameters.Add("P_BACKEND_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();
        //            }
        //            if (!string.IsNullOrEmpty(BM.REARAXEL) && !string.IsNullOrEmpty(BM.REARAXEL_DESC))
        //            {
        //                sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REARAXEL) ? null : BM.REARAXEL.ToUpper().Trim();
        //                string RearAxelValue = BM.REARAXEL_DESC.Trim();
        //                char[] separators1 = new char[] { '#' };
        //                string[] subs1 = RearAxelValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
        //                sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }

        //            if (!string.IsNullOrEmpty(BM.TRANSMISSION) && !string.IsNullOrEmpty(BM.TRANSMISSION_DESC))
        //            {
        //                sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.TRANSMISSION) ? null : BM.TRANSMISSION.ToUpper().Trim();
        //                string TransmissionValue = BM.TRANSMISSION_DESC.Trim();
        //                char[] separators2 = new char[] { '#' };
        //                string[] subs2 = TransmissionValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);
        //                sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }
        //            if (!string.IsNullOrEmpty(BM.HYDRAULIC) && !string.IsNullOrEmpty(BM.HYDRAULIC_DESC))
        //            {
        //                sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.HYDRAULIC) ? null : BM.HYDRAULIC.ToUpper().Trim();
        //                string HydraulicValue = BM.HYDRAULIC_DESC.Trim();
        //                char[] separators3 = new char[] { '#' };
        //                string[] subs3 = HydraulicValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);
        //                sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
        //            }
        //            else
        //            {
        //                sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //                sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            }
        //            sc.Parameters.Add("P_PREFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX1) ? null : BM.PREFIX1.ToUpper().Trim();
        //            sc.Parameters.Add("P_PREFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX2) ? null : BM.PREFIX2.ToUpper().Trim();
        //            sc.Parameters.Add("P_SUFFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX1) ? null : BM.SUFFIX1.ToUpper().Trim();
        //            sc.Parameters.Add("P_SUFFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX2) ? null : BM.SUFFIX2.ToUpper().Trim();
        //            sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REMARKS1) ? null : BM.REMARKS1.ToUpper().Trim();
        //            sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
        //            sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "INSERT";
        //            sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            //sc.Parameters.Add("P_RETURN", OracleDbType.Varchar2,2000, ParameterDirection.Output);
        //            sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
        //            int a = sc.ExecuteNonQuery();
        //            //string response = Convert.ToString(sc.Parameters["P_RETURN"].Value);

        //            //ConClose();
        //            result = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //    return result;


        //}
        //public DataTable GridBackendMaster(BackendModel BM)
        //{
        //    DataTable dt = new DataTable();
        //    string query = string.Empty;
        //    try
        //    {
        //        query = string.Format(@"select* from XXES_BACKEND_MASTER s where s.Plant_Code = '" + Convert.ToString(BM.PLANT_CODE).ToUpper().Trim() + "' AND s.FAMILY_CODE = '" + Convert.ToString(BM.FAMILY_CODE).ToUpper().Trim() + "' AND s.BACKEND = '" + Convert.ToString(BM.BACKEND).ToUpper().Trim() + "'  order by s.FAMILY_CODE");

        //        dt = returnDataTable(query);

        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //    }
        //    finally
        //    {

        //    }
        //    return dt;
        //}
        //public bool UpdateBackendMaster(BackendModel BM)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleCommand sc = new OracleCommand("USP_CRUD_BACKENDMASTER", Connection()))
        //        {
        //            ConOpen();
        //            sc.CommandType = CommandType.StoredProcedure;
        //            sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
        //            sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

        //            sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.BACKEND) ? null : BM.BACKEND.ToUpper().Trim();
        //            string BackendValue = BM.BACKEND_DESC.Trim();
        //            char[] separators = new char[] { '#' };
        //            string[] subs = BackendValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //            //sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
        //            sc.Parameters.Add("P_BACKEND_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

        //            sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REARAXEL) ? null : BM.REARAXEL.ToUpper().Trim();
        //            string RearAxelValue = BM.REARAXEL_DESC.Trim();
        //            char[] separators1 = new char[] { '#' };
        //            string[] subs1 = RearAxelValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //            sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();

        //            sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.TRANSMISSION) ? null : BM.TRANSMISSION.ToUpper().Trim();
        //            string TransmissionValue = BM.TRANSMISSION_DESC.Trim();
        //            char[] separators2 = new char[] { '#' };
        //            string[] subs2 = TransmissionValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //            sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();

        //            sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.HYDRAULIC) ? null : BM.HYDRAULIC.ToUpper().Trim();
        //            string HydraulicValue = BM.HYDRAULIC_DESC.Trim();
        //            char[] separators3 = new char[] { '#' };
        //            string[] subs3 = HydraulicValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //            sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();

        //            sc.Parameters.Add("P_PREFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX1) ? null : BM.PREFIX1.ToUpper().Trim();
        //            sc.Parameters.Add("P_PREFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX2) ? null : BM.PREFIX2.ToUpper().Trim();
        //            sc.Parameters.Add("P_SUFFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX1) ? null : BM.SUFFIX1.ToUpper().Trim();
        //            sc.Parameters.Add("P_SUFFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX2) ? null : BM.SUFFIX2.ToUpper().Trim();
        //            sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REMARKS1) ? null : BM.REMARKS1.ToUpper().Trim();
        //            sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.AUTOID) ? null : BM.AUTOID;
        //            sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
        //            sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateBackendMaster";
        //            sc.Parameters.Add("P_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
        //            sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);

        //            sc.ExecuteNonQuery();
        //            ConClose();
        //            result = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //    }
        //    finally
        //    {
        //        ConClose();
        //    }
        //    return result;
        //}

        public List<DDLTextValue> Fill_Engine_Name(EngineModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);

                List<DDLTextValue> Engine = new List<DDLTextValue>();

                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Engine.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return Engine;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Fual_Pump()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> FUALPUMP = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'FUEL PUMP%'  order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        FUALPUMP.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return FUALPUMP;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Cylinder_Block()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> CYLINDER_BLOCK = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'Cylinder Block%' order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        CYLINDER_BLOCK.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return CYLINDER_BLOCK;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Cylinder_Head()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> CYLINDER_HEAD = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Cylinder Head%' order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        CYLINDER_HEAD.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return CYLINDER_HEAD;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Connecting_Rod()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> CONNECTING_ROD = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Connecting Rod%'  order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        CONNECTING_ROD.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return CONNECTING_ROD;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Crack_Shaft()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> CRACK_SHAFT = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Crack Shaft%' order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        CRACK_SHAFT.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return CRACK_SHAFT;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Cam_Shaft()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> CAM_SHAFT = new List<DDLTextValue>();
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') order by segment1");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in (149) and substr(segment1, 1, 1) in ('D','S') AND DESCRIPTION like 'Cam Shaft%' order by segment1");
                TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'ENGINE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION ,ITEM_CODE from XXES_ENGINE_MASTER order by ITEM_CODE");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select ITEM_CODE || ' # ' || ITEM_DESC as DESCRIPTION,ITEM_CODE from XXES_ENGINE_MASTER where Plant_code='" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "' order by ITEM_CODE");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        CAM_SHAFT.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return CAM_SHAFT;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                ConClose();
            }
        }
        public bool InsertEngineMaster(EngineModel EM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_EngineMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PLANT_CODE) ? null : EM.PLANT_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.FAMILY_CODE) ? null : EM.FAMILY_CODE.ToUpper().Trim();


                    //sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.Engine.Trim().ToUpper();
                    string Engine_DescValue = EM.Engine.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = Engine_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

                    if (!string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP))
                    {
                        //sc.Parameters.Add("", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.FUEL_INJECTION_PUMP.ToUpper().Trim();
                        string CylinderBlock_DescValue = EM.FUEL_INJECTION_PUMP.Trim();
                        char[] separators6 = new char[] { '#' };
                        string[] subs6 = CylinderBlock_DescValue.Split(separators6, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs6[0].ToUpper().Trim();
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs6[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CYLINDER_BLOCK))
                    {
                        //sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CYLINDER_BLOCK.ToUpper().Trim();
                        string CylinderBlock_DescValue = EM.CYLINDER_BLOCK.Trim();
                        char[] separators1 = new char[] { '#' };
                        string[] subs1 = CylinderBlock_DescValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CYLINDER_HEAD))
                    {
                        //sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CYLINDER_HEAD.ToUpper().Trim();
                        string CylinderHead_DescValue = EM.CYLINDER_HEAD.Trim();
                        char[] separators2 = new char[] { '#' };
                        string[] subs2 = CylinderHead_DescValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CONNECTING_ROD))
                    {
                        //sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CONNECTING_ROD.ToUpper().Trim();
                        string ConnectingRod_DescValue = EM.CONNECTING_ROD.Trim();
                        char[] separators3 = new char[] { '#' };
                        string[] subs3 = ConnectingRod_DescValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CRANK_SHAFT))
                    {
                        //sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CRANK_SHAFT.ToUpper().Trim();
                        string CrankShaft_DescValue = EM.CRANK_SHAFT.Trim();
                        char[] separators4 = new char[] { '#' };
                        string[] subs4 = CrankShaft_DescValue.Split(separators4, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CAM_SHAFT))
                    {
                        //sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CAM_SHAFT.ToUpper().Trim();
                        string CamShaft_DescValue = EM.CAM_SHAFT.Trim();
                        char[] separators5 = new char[] { '#' };
                        string[] subs5 = CamShaft_DescValue.Split(separators5, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }
                    if (!string.IsNullOrEmpty(EM.ECU))
                    {
                        //sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.CAM_SHAFT.ToUpper().Trim();
                        string CamShaft_DescValue = EM.ECU.Trim();
                        char[] separators5 = new char[] { '#' };
                        string[] subs5 = CamShaft_DescValue.Split(separators5, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[0].ToUpper().Trim();
                        sc.Parameters.Add("P_ECU_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_ECU_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    sc.Parameters.Add("P_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.INJECTOR) ? null : EM.INJECTOR.ToUpper().Trim();
                    sc.Parameters.Add("P_REQ_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQ_FUEL_INJECTION_PUMP == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_BLOCK == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_HEAD == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CONNECTING_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CONNECTING_ROD == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CRANK_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_REQ_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_PREFIX_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_1) ? null : EM.PREFIX_1.ToUpper().Trim();
                    sc.Parameters.Add("P_PREFIX_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_2) ? null : EM.PREFIX_2.ToUpper().Trim();
                    sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REMARKS1) ? null : EM.REMARKS1.ToUpper().Trim();
                    sc.Parameters.Add("P_NO_OF_PISTONS", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_PISTONS) ? null : EM.NO_OF_PISTONS.ToUpper().Trim();
                    sc.Parameters.Add("P_NO_OF_INJECTORS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_INJECTORS) ? null : EM.NO_OF_INJECTORS.ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertEngineMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;

                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;


        }
        public DataTable GridEngineMaster(EngineModel EM)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"select FUEL_INJECTION_PUMP,FUEL_INJECTION_PUMP_DESC,AUTOID,PLANT_CODE, FAMILY_CODE, ITEM_CODE, ITEM_DESC, CYLINDER_BLOCK, CYLINDER_BLOCK_DESC, CYLINDER_HEAD, CYLINDER_HEAD_DESC,
                        CONNECTING_ROD, CONNECTING_ROD_DESC, CRANK_SHAFT, CRANK_SHAFT_DESC, CAM_SHAFT, CAM_SHAFT_DESC, INJECTOR,REQ_FUEL_INJECTION_PUMP, 
                        REQUIRE_CYLINDER_BLOCK, REQUIRE_CYLINDER_HEAD,REQUIRE_CONNECTING_ROD, REQUIRE_CRANK_SHAFT, REQUIRE_CAM_SHAFT, PREFIX_1,
                        PREFIX_2, REMARKS1,  NO_OF_PISTONS, CREATEDBY, TO_CHAR(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS CREATEDDATE, UPDATEDBY, UPDATEDDATE from 
                        XXES_ENGINE_MASTER where PLANT_CODE= '{0]' AND FAMILY_CODE= '{1}' ORDER BY AUTOID DESC", EM.PLANT_CODE.Trim().ToUpper(),
                        EM.FAMILY_CODE.Trim().ToUpper()) ;

                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;

        }

        public bool UpdateEngineMaster(EngineModel EM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_EngineMaster", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;


                    string Engine_DescValue = EM.Engine.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = Engine_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

                    if (!string.IsNullOrEmpty(EM.FUEL_INJECTION_PUMP))
                    {
                        string CylinderBlock_DescValue = EM.FUEL_INJECTION_PUMP.Trim();
                        char[] separators6 = new char[] { '#' };
                        string[] subs6 = CylinderBlock_DescValue.Split(separators6, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs6[0].ToUpper().Trim();
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs6[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CYLINDER_BLOCK))
                    {
                        string CylinderBlock_DescValue = EM.CYLINDER_BLOCK.Trim();
                        char[] separators1 = new char[] { '#' };
                        string[] subs1 = CylinderBlock_DescValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CYLINDER_HEAD))
                    {
                        string CylinderHead_DescValue = EM.CYLINDER_HEAD.Trim();
                        char[] separators2 = new char[] { '#' };
                        string[] subs2 = CylinderHead_DescValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CONNECTING_ROD))
                    {
                        string ConnectingRod_DescValue = EM.CONNECTING_ROD.Trim();
                        char[] separators3 = new char[] { '#' };
                        string[] subs3 = ConnectingRod_DescValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CRANK_SHAFT))
                    {
                        string CrankShaft_DescValue = EM.CRANK_SHAFT.Trim();
                        char[] separators4 = new char[] { '#' };
                        string[] subs4 = CrankShaft_DescValue.Split(separators4, StringSplitOptions.RemoveEmptyEntries);

                        sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(EM.CAM_SHAFT))
                    {
                        string CamShaft_DescValue = EM.CAM_SHAFT.Trim();
                        char[] separators5 = new char[] { '#' };
                        string[] subs5 = CamShaft_DescValue.Split(separators5, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[0].ToUpper().Trim();
                        sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }
                    if (!string.IsNullOrEmpty(EM.ECU))
                    {
                        string CamShaft_DescValue = EM.ECU.Trim();
                        char[] separators5 = new char[] { '#' };
                        string[] subs5 = CamShaft_DescValue.Split(separators5, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[0].ToUpper().Trim();
                        sc.Parameters.Add("P_ECU_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs5[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_ECU_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    sc.Parameters.Add("P_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.INJECTOR) ? null : EM.INJECTOR.ToUpper().Trim();
                    sc.Parameters.Add("P_REQ_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQ_FUEL_INJECTION_PUMP == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_BLOCK == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CYLINDER_HEAD == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CONNECTING_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CONNECTING_ROD == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CRANK_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_REQUIRE_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_REQ_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.REQUIRE_CAM_SHAFT == true ? "1" : "0";
                    sc.Parameters.Add("P_PREFIX_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_1) ? null : EM.PREFIX_1.ToUpper().Trim();
                    sc.Parameters.Add("P_PREFIX_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.PREFIX_2) ? null : EM.PREFIX_2.ToUpper().Trim();
                    sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.REMARKS1) ? null : EM.REMARKS1.ToUpper().Trim();
                    sc.Parameters.Add("P_NO_OF_PISTONS", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_PISTONS) ? null : EM.NO_OF_PISTONS.ToUpper().Trim();
                    sc.Parameters.Add("P_NO_OF_INJECTORS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(EM.NO_OF_INJECTORS) ? null : EM.NO_OF_INJECTORS.ToUpper().Trim();

                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.AUTOID.ToUpper().Trim();
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateEngineMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }


        public List<DDLTextValue> Fill_BACKEND(BackendModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> BACKEND = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%BACKEND%' order by segment1");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        BACKEND.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                    //}
                }
                return BACKEND;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_HYDRAULIC(BackendModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> HYDRAULIC = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%HYDRAULIC LIFT ASSEMBLY%' order by segment1");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        HYDRAULIC.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return HYDRAULIC;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Transmission(BackendModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> TRANSMISSION = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in " + orgid + " and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149','150') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

                //}
                //else
                //{
                //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in  ('" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        TRANSMISSION.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return TRANSMISSION;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_RearAxel(BackendModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> REARAXEL = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like '%AXLE ASSEMBLY%' order by segment1");

                //if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                //{
                //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in ('149','150') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");

                //}
                //else
                //{
                //    TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from apps.mtl_system_items where organization_id in  ('" + Convert.ToString(HttpContext.Current.Session["Login_Unit"]) + "') and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'TRANSMISSION ASSEMBLY%' order by segment1");
                //}
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        REARAXEL.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return REARAXEL;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public bool InsertBackendMaster(BackendModel BM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_BACKENDMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.PLANT_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.FAMILY_CODE.ToUpper().Trim();

                    if (!string.IsNullOrEmpty(BM.BACKEND) && !string.IsNullOrEmpty(BM.BACKEND_DESC))
                    {
                        sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = BM.BACKEND.ToUpper().Trim();
                        string BackendValue = BM.BACKEND_DESC.Trim();
                        char[] separators = new char[] { '#' };
                        string[] subs = BackendValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_BACKEND_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();
                    }
                    if (!string.IsNullOrEmpty(BM.REARAXEL) && !string.IsNullOrEmpty(BM.REARAXEL_DESC))
                    {
                        sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REARAXEL) ? null : BM.REARAXEL.ToUpper().Trim();
                        string RearAxelValue = BM.REARAXEL_DESC.Trim();
                        char[] separators1 = new char[] { '#' };
                        string[] subs1 = RearAxelValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }

                    if (!string.IsNullOrEmpty(BM.TRANSMISSION) && !string.IsNullOrEmpty(BM.TRANSMISSION_DESC))
                    {
                        sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.TRANSMISSION) ? null : BM.TRANSMISSION.ToUpper().Trim();
                        string TransmissionValue = BM.TRANSMISSION_DESC.Trim();
                        char[] separators2 = new char[] { '#' };
                        string[] subs2 = TransmissionValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }
                    if (!string.IsNullOrEmpty(BM.HYDRAULIC) && !string.IsNullOrEmpty(BM.HYDRAULIC_DESC))
                    {
                        sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.HYDRAULIC) ? null : BM.HYDRAULIC.ToUpper().Trim();
                        string HydraulicValue = BM.HYDRAULIC_DESC.Trim();
                        char[] separators3 = new char[] { '#' };
                        string[] subs3 = HydraulicValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);
                        sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
                    }
                    else
                    {
                        sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    }
                    sc.Parameters.Add("P_PREFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX1) ? null : BM.PREFIX1.ToUpper().Trim();
                    sc.Parameters.Add("P_PREFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX2) ? null : BM.PREFIX2.ToUpper().Trim();
                    sc.Parameters.Add("P_SUFFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX1) ? null : BM.SUFFIX1.ToUpper().Trim();
                    sc.Parameters.Add("P_SUFFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX2) ? null : BM.SUFFIX2.ToUpper().Trim();
                    sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REMARKS1) ? null : BM.REMARKS1.ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "INSERT";
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    //sc.Parameters.Add("P_RETURN", OracleDbType.Varchar2,2000, ParameterDirection.Output);
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    int a = sc.ExecuteNonQuery();
                    //string response = Convert.ToString(sc.Parameters["P_RETURN"].Value);

                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;


        }
        public DataTable GridBackendMaster(BackendModel BM)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"select AUTOID, PLANT_CODE, FAMILY_CODE, BACKEND, BACKEND_DESC, REARAXEL, REARAXEL_DESC, TRANSMISSION, TRANSMISSION_DESC, HYDRAULIC, 
                                                HYDRAULIC_DESC, PREFIX1, PREFIX2, SUFFIX1, SUFFIX2, REMARKS1, CREATEDBY,  TO_CHAR(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS CREATEDDATE, UPDATEDBY, TO_CHAR(UPDATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS UPDATEDDATE,
                                                UPDATEDDATE from  XXES_BACKEND_MASTER where PLANT_CODE= '" + BM.PLANT_CODE.ToUpper().Trim() + "' AND FAMILY_CODE= '" + BM.FAMILY_CODE.ToUpper().Trim() + "' ORDER BY AUTOID DESC");

                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;
        }

        public bool UpdateBackendMaster(BackendModel BM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_BACKENDMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.BACKEND) ? null : BM.BACKEND.ToUpper().Trim();
                    string BackendValue = BM.BACKEND_DESC.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = BackendValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    //sc.Parameters.Add("P_BACKEND", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("P_BACKEND_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

                    sc.Parameters.Add("P_REARAXEL", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REARAXEL) ? null : BM.REARAXEL.ToUpper().Trim();
                    string RearAxelValue = BM.REARAXEL_DESC.Trim();
                    char[] separators1 = new char[] { '#' };
                    string[] subs1 = RearAxelValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_REARAXEL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();

                    sc.Parameters.Add("P_TRANSMISSION", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.TRANSMISSION) ? null : BM.TRANSMISSION.ToUpper().Trim();
                    string TransmissionValue = BM.TRANSMISSION_DESC.Trim();
                    char[] separators2 = new char[] { '#' };
                    string[] subs2 = TransmissionValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_TRANSMISSION_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();

                    sc.Parameters.Add("P_HYDRAULIC", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.HYDRAULIC) ? null : BM.HYDRAULIC.ToUpper().Trim();
                    string HydraulicValue = BM.HYDRAULIC_DESC.Trim();
                    char[] separators3 = new char[] { '#' };
                    string[] subs3 = HydraulicValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_HYDRAULIC_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();

                    sc.Parameters.Add("P_PREFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX1) ? null : BM.PREFIX1.ToUpper().Trim();
                    sc.Parameters.Add("P_PREFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.PREFIX2) ? null : BM.PREFIX2.ToUpper().Trim();
                    sc.Parameters.Add("P_SUFFIX1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX1) ? null : BM.SUFFIX1.ToUpper().Trim();
                    sc.Parameters.Add("P_SUFFIX2", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.SUFFIX2) ? null : BM.SUFFIX2.ToUpper().Trim();
                    sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.REMARKS1) ? null : BM.REMARKS1.ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(BM.AUTOID) ? null : BM.AUTOID;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateBackendMaster";
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();
                    ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }


        public List<DDLTextValue> Fill_FIP(FIPModel obj)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> ITEM_CODE = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') and DESCRIPTION like 'FUEL INJECTION PUMP%'  order by segment1");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        ITEM_CODE.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                    //}
                }
                return ITEM_CODE;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public DataTable GridFipMaster(FIPModel FM)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"SELECT AUTOID, PLANT_CODE, FAMILY_CODE, ITEM_CODE, DESCRIPTION, MODEL_CODE_NO, 
                                        TO_CHAR(ENTRYDATE, 'DD-MM-YYYY HH24:MI:SS') AS ENTRYDATE, CREATEDBY, TO_CHAR(CREATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS CREATEDDATE, UPDATEDBY, 
                                        TO_CHAR(UPDATEDDATE, 'DD-MM-YYYY HH24:MI:SS') AS UPDATEDDATE  FROM XXES_FIPMODEL_CODE 
                                        where PLANT_CODE= '" + FM.PLANT_CODE + "' AND FAMILY_CODE= '" + FM.FAMILY_CODE + "' ORDER BY AUTOID DESC");

                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;
        }
        public bool InsertFIPMaster(FIPModel FM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_FIPMODEL_CODE", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(FM.PLANT_CODE) ? null : FM.PLANT_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(FM.FAMILY_CODE) ? null : FM.FAMILY_CODE.ToUpper().Trim();


                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = FM.ITEM_CODE.Trim().ToUpper();
                    string Engine_DescValue = FM.DESCRIPTION.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = Engine_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();

                    sc.Parameters.Add("P_MODEL_CODE_NO", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(FM.MODEL_CODE_NO) ? null : FM.MODEL_CODE_NO.ToUpper().Trim();
                    sc.Parameters.Add("P_ENTRYDATE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(FM.ENTRYDATE) ? null : FM.ENTRYDATE.ToUpper().Trim();


                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CREATEDDATE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "INSERT_FIP";

                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;


        }


        //public List<GoodsRecivingatStoreModel> GridInspectionPaging(GoodsRecivingatStoreModel obj)
        //{
        //    DataTable dt = new DataTable();
        //    List<GoodsRecivingatStoreModel> empDetails = new List<GoodsRecivingatStoreModel>();
        //    try
        //    {
        //        da = new OracleDataAdapter("USP_INSPECTIONPAGING", Connection());
        //        da.SelectCommand.CommandType = CommandType.StoredProcedure;

        //        da.SelectCommand.Parameters.Add("P_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.PLANT.ToUpper().Trim();
        //        da.SelectCommand.Parameters.Add("P_PUNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(obj.PUNAME) ? null : obj.PUNAME.ToUpper().Trim();
        //        da.SelectCommand.Parameters.Add("p_startRowIndex", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.STARTROWINDEX;
        //        da.SelectCommand.Parameters.Add("p_maximumRows", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.MAXROWS;
        //        da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
        //        da.Fill(dt);

        //        if (dt.Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                GoodsRecivingatStoreModel GR = new GoodsRecivingatStoreModel
        //                {
        //                    TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
        //                    AUTOID = Convert.ToInt32(dr["AUTOID"]),
        //                    VENDOR_CODE = dr["VENDOR_CODE"].ToString(),
        //                    MRN_NO = dr["MRN_NO"].ToString(),
        //                    CREATEDDATE = dr["CREATEDDATE"].ToString(),
        //                    CREATEDTIME = dr["CREATEDTIME"].ToString(),
        //                    ITEMCODE = dr["ITEMCODE"].ToString(),
        //                    ITEM_REVISION = dr["ITEM_REVISION"].ToString(),
        //                    BOM_REVISION = dr["BOM_REVISION"].ToString(),

        //                    ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
        //                    QUANTITY = dr["QUANTITY"].ToString(),
        //                    PUNAME = dr["PUNAME"].ToString(),
        //                    COUNTING = dr["COUNTING"].ToString(),
        //                    MT_TEST = dr["MT_TEST"].ToString(),
        //                    CNT = dr["CNT"].ToString(),
        //                    QUALITY_OK = dr["QUALITY_OK"].ToString(),
        //                    DAYS_DIFFERENCE = dr["DAYS_DIFFERENCE"].ToString(),
        //                    STATUS = dr["STATUS"].ToString()
        //                };
        //                empDetails.Add(GR);
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //        throw;
        //    }

        //    return empDetails;
        //}
        public List<GoodsRecivingatStoreModel> GridInspectionPaging(GoodsRecivingatStoreModel obj)
        {
            DataTable dt = new DataTable();
            List<GoodsRecivingatStoreModel> empDetails = new List<GoodsRecivingatStoreModel>();
            try
            {
                
                da = new OracleDataAdapter("USP_INSPECTIONPAGING", Connection());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.Add("P_PLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.PLANT.ToUpper().Trim();
                da.SelectCommand.Parameters.Add("P_FAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.FAMILY.ToUpper().Trim();
                da.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.FROMDATE_INSP.Trim();
                da.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.TODATE_INSP.Trim();
                da.SelectCommand.Parameters.Add("P_PUNAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(obj.PUNAME) ? null : obj.PUNAME.ToUpper().Trim();
                da.SelectCommand.Parameters.Add("p_startRowIndex", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.STARTROWINDEX;
                da.SelectCommand.Parameters.Add("p_maximumRows", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.MAXROWS;
                da.SelectCommand.Parameters.Add("p_maximumRows", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.P_Search;
                da.SelectCommand.Parameters.Add("prc", OracleDbType.RefCursor, ParameterDirection.Output);
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        GoodsRecivingatStoreModel GR = new GoodsRecivingatStoreModel
                        {
                            AUTOID = Convert.ToInt32(dr["AUTOID"]),
                            VENDOR_CODE = dr["VENDOR_CODE"].ToString(),
                            MRN_NO = dr["MRN_NO"].ToString(),
                            CREATEDDATE = dr["CREATEDDATE"].ToString(),
                            CREATEDTIME = dr["CREATEDTIME"].ToString(),
                            ITEMCODE = dr["ITEMCODE"].ToString(),
                            ITEM_REVISION = dr["ITEM_REVISION"].ToString(),
                            BOM_REVISION = dr["BOM_REVISION"].ToString(),
                            VENDOR_NAME = dr["VENDOR_NAME"].ToString(),
                            ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                            QUANTITY = dr["ORDERQTY"].ToString(),
                            QTY_RECEIVED = dr["ACTUALQTY"].ToString(),
                            PUNAME = dr["PUNAME"].ToString(),
                            COUNTING = dr["COUNTING"].ToString(),
                            MT_TEST = dr["MT_TEST"].ToString(),
                            CNT = dr["CNT"].ToString(),
                            QUALITY_OK = dr["QUALITY_OK"].ToString(),
                            DAYS_DIFFERENCE = dr["DAYS_DIFFERENCE"].ToString(),
                            STATUS = dr["STATUS"].ToString(),
                            TYPE = Convert.ToString(dr["TYPE"])
                        };
                        empDetails.Add(GR);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return empDetails;
        }

        public DataTable GetQualityEXCELData(GoodsRecivingatStoreModel obj)
        {
            string query = string.Empty;
            if(string.IsNullOrEmpty(obj.PUNAME))
            {
                query = string.Format(@"SELECT ROWNMBER SRNO,PLANT_CODE,MRN_NO,ITEMCODE,ITEM_DESCRIPTION,VENDOR_CODE,VENDOR_NAME,CREATEDDATE,CREATEDTIME,ITEM_REVISION,
                                        BOM_REVISION,ORDERQTY,ACTUALQTY,PUNAME,
                                             COUNTING, MT_TEST, CNT,CASE WHEN CNT <=5 THEN 'NEW' ELSE '' END AS STATUS,
                                             DAYS_DIFFERENCE
                                              
                                              FROM 

                                             (SELECT ROW_NUMBER() OVER(ORDER BY m.STORE_VERIFIEDDATE DESC) AS ROWNMBER,M.PLANT_CODE, M.AUTOID, I.VENDOR_CODE,I.VENDOR_NAME, M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE,
                                             to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as CREATEDTIME, M.ITEMCODE,
                                             M.ITEM_REVISION, M.BOM_REVISION, 
                                              M.ITEM_DESCRIPTION, M.QUANTITY AS ORDERQTY, M.REC_QTY AS ACTUALQTY,M.PUNAME,
                                             to_char( m.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) COUNTING, '' MT_TEST, (SELECT COUNT(*) FROM xxes_mrninfo i WHERE i.itemcode=M.itemcode 
                                             AND EXTRACT(YEAR FROM I.CREATEDDATE)=EXTRACT(YEAR FROM SYSDATE) AND i.MRN_NO<>M.MRN_NO ) CNT,
                                             M.QUALITY_OK,  
                                             (
                                                trunc(SYSDATE) - (SELECT trunc(A.CREATEDDATE) FROM 
                                                (SELECT i.CREATEDDATE,i.PLANT_CODE,i.ITEMCODE FROM xxes_mrninfo i   ORDER BY CREATEDDATE DESC)a
                                                WHERE ROWNUM=1 AND  a.ITEMCODE=M.ITEMCODE 
                                                AND a.PLANT_CODE=M.PLANT_CODE  )                                            
                                            ) DAYS_DIFFERENCE
                                            FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                        ON M.MRN_NO = I.MRN_NO  WHERE M.STORE_VERIFIED = 'VERIFIED' AND M.STATUS = 'QA' AND M.QUALITY_OK IS NULL AND 
                                    M.PLANT_CODE = '{0}' AND m.family_code = '{1}' AND TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')>= to_date('{2}','dd-Mon-yyyy') AND 
                        TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')<=to_date('{3}','dd-Mon-yyyy')) inspection",obj.PLANT.Trim(),obj.FAMILY.Trim(),obj.FROMDATE_INSP.Trim(),obj.TODATE_INSP.Trim());
              
            }
            else
            {
                query = string.Format(@"SELECT ROWNMBER SRNO,PLANT_CODE,MRN_NO,ITEMCODE,ITEM_DESCRIPTION,VENDOR_CODE,VENDOR_NAME,CREATEDDATE,CREATEDTIME,ITEM_REVISION,
                                            BOM_REVISION,ORDERQTY,ACTUALQTY,PUNAME,
                                             COUNTING, MT_TEST, CNT,CASE WHEN CNT <=5 THEN 'NEW' ELSE '' END AS STATUS,
                                             DAYS_DIFFERENCE FROM
                                            (SELECT ROW_NUMBER() OVER(ORDER BY m.STORE_VERIFIEDDATE DESC) AS ROWNMBER,M.PLANT_CODE, M.AUTOID, I.VENDOR_CODE, I.VENDOR_NAME,M.MRN_NO, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE,
                                             to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as CREATEDTIME, M.ITEMCODE,
                                             M.ITEM_REVISION, M.BOM_REVISION, 
                                              M.ITEM_DESCRIPTION, M.QUANTITY AS ORDERQTY, M.REC_QTY AS ACTUALQTY, M.PUNAME,
                                             to_char( m.STORE_VERIFIEDDATE, 'dd-Mon-yyyy HH24:MI:SS' ) COUNTING, '' MT_TEST, (SELECT COUNT(*) FROM xxes_mrninfo i WHERE i.itemcode=M.itemcode 
                                             AND EXTRACT(YEAR FROM I.CREATEDDATE)=EXTRACT(YEAR FROM SYSDATE) AND i.MRN_NO<>M.MRN_NO ) CNT,
                                             M.QUALITY_OK,  
                                             (
                                                trunc(SYSDATE) - (SELECT trunc(A.CREATEDDATE) FROM 
                                                (SELECT i.CREATEDDATE,i.PLANT_CODE,i.ITEMCODE FROM xxes_mrninfo i   ORDER BY CREATEDDATE DESC)a
                                                WHERE ROWNUM=1 AND  a.ITEMCODE=M.ITEMCODE 
                                                AND a.PLANT_CODE=M.PLANT_CODE  )                                            
                                            ) DAYS_DIFFERENCE
                                            FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                            ON M.MRN_NO = I.MRN_NO  WHERE M.STORE_VERIFIED = 'VERIFIED' AND M.STATUS = 'QA' AND M.QUALITY_OK IS NULL AND M.PLANT_CODE = '{0}' AND M.PUNAME = '{1}' AND m.family_code = '{2}' AND TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')>= to_date('{3}','dd-Mon-yyyy') AND 
                        TO_CHAR(m.STORE_VERIFIEDDATE,'dd-Mon-yyyy')<=to_date('{4}','dd-Mon-yyyy')) inspection", obj.PLANT.Trim(), obj.PUNAME.Trim(),obj.FAMILY.Trim(),obj.FROMDATE_INSP.Trim(),obj.TODATE_INSP.Trim());
            }
            return returnDataTable(query);
        }

        public bool InsertRawMaterialMaster(RawMaterialMaster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_RAWMATERIALMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Plant.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Family.ToUpper().Trim();
                    //string stageValue = itemModel.Stage.Trim();
                    //char[] separators = new char[] { '#' };
                    //string[] subs = stageValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode.ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemDescription.ToUpper().Trim();
                    sc.Parameters.Add("P_PACKING_STND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PackingStandard.ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Insert";
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool UpdateRawMaterialMaster(RawMaterialMaster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_RAWMATERIALMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode.ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemDescription.ToUpper().Trim();
                    sc.Parameters.Add("P_PACKING_STND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.PackingStandard.ToUpper().Trim();
                    sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AutoId;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Update";
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteRawMaterialMaster(RawMaterialMaster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_RAWMATERIALMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PACKING_STND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AutoId;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Delete";
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable GridAddController(AddController add)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESCONTROLLERS", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("C_DID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_STAGE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_IP_ADDR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_PORT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_INPUT_MODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;

                    oc.Parameters.Add("C_READING_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_ACTIVE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Plant.ToUpper().Trim();
                    oc.Parameters.Add("C_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Family.ToUpper().Trim();
                    oc.Parameters.Add("C_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "GridControllerData";
                    oc.Parameters.Add("C_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    //ConClose();
                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return dt;
        }

        public bool InsertAddController(AddController add)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESCONTROLLERS", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("C_DID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.ID == null ? null : add.ID.ToUpper().Trim();
                    oc.Parameters.Add("C_STAGE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Stage == null ? null : add.Stage.ToUpper().Trim();
                    oc.Parameters.Add("C_IP_ADDR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.IPAddress == null ? null : add.IPAddress.ToUpper().Trim();
                    oc.Parameters.Add("C_PORT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Port == null ? null : add.Port.ToUpper().Trim();
                    oc.Parameters.Add("C_INPUT_MODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Mode == null ? null : add.Mode.ToUpper().Trim();

                    oc.Parameters.Add("C_READING_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.ReadingChannel == null ? null : add.ReadingChannel.ToUpper().Trim();
                    oc.Parameters.Add("C_ACTIVE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.IsActive == true ? "Y" : "N";
                    oc.Parameters.Add("C_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Remarks == null ? null : add.Remarks.ToUpper().Trim();
                    oc.Parameters.Add("C_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Plant.ToUpper().Trim();
                    oc.Parameters.Add("C_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.Family.ToUpper().Trim();
                    oc.Parameters.Add("C_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("C_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertControllerData";
                    oc.Parameters.Add("C_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);

            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool DeleteAddController(AddController add)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESCONTROLLERS", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("C_DID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_STAGE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_IP_ADDR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_PORT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_INPUT_MODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_READING_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_ACTIVE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = add.AutoId;
                    oc.Parameters.Add("C_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("C_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteControllerData";
                    oc.Parameters.Add("C_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                   //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);

            }
            finally
            {
                ConClose();
            }
            return result;
        }


        public List<DDLTextValue> Fill_Stage_Name()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> stage = new List<DDLTextValue>();
                TmpDs = returnDataTable(@"Select PARAMVALUE || ' # ' || PARAMETERINFO as PARAMETERINFO,PARAMVALUE from XXES_SFT_SETTINGS 
                     where status='CONTRL' order by PARAMVALUE");
                //TmpDs = returnDataTable(@"select distinct segment1 || ' # ' || PARAMVALUE as PARAMVALUE, segment1 as PARAMETERINFO from XXES_SFT_SETTINGS where status='CONTRL' order by PARAMETERINFO");
                //TmpDs = returnDataTable(@"select PARAMVALUE,PARAMETERINFO from XXES_SFT_SETTINGS where status='CONTRL' order by PARAMETERINFO");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        stage.Add(new DDLTextValue
                        {
                            Text = dr["PARAMETERINFO"].ToString(),
                            Value = dr["PARAMVALUE"].ToString(),
                        });
                    }
                }
                return stage;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> Fill_Item_Bulk(string Plant, string Family)
        {
            DataTable TmpDs = null;
            try
            {
                Function fun = new Function();
                string query = string.Empty;
                string orgid = fun.getOrgId(Convert.ToString(Plant.ToUpper().Trim()), Convert.ToString(Family.ToUpper().Trim()));
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                List<DDLTextValue> Bulk = new List<DDLTextValue>();

                //TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 2) in ('CF') order by segment1");
                TmpDs = returnDataTable("select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') order by segment1");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Bulk.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        });
                    }
                }
                return Bulk;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public bool InsertKanbanMaster(KanbanMaster kanban)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESKANBANMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("K_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Plant.ToUpper().Trim();
                    oc.Parameters.Add("K_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Family.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(kanban.Modes) && !string.IsNullOrEmpty(kanban.KanbanNumber))
                    {
                        if (kanban.Modes == "EXISTING")
                        {
                            if (kanban.KanbanNumber.Contains("-"))
                                kanban.KanbanNumber = kanban.KanbanNumber.ToUpper().Trim().Split('-')[0].Trim();
                            oc.Parameters.Add("K_KANBAN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.KanbanNumber == null ? null : kanban.KanbanNumber.ToUpper().Trim();
                        }
                        else
                        {
                            oc.Parameters.Add("K_KANBAN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.KanbanNumber == null ? null : kanban.KanbanNumber.ToUpper().Trim();
                        }

                    }
                    else
                        oc.Parameters.Add("K_KANBAN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.KanbanNumber == null ? null : kanban.KanbanNumber.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(kanban.Modes) && !string.IsNullOrEmpty(kanban.KanbanNumber))
                    {
                        string DCODE = kanban.Item.Trim();
                        char[] separators = new char[] { '#' };
                        string[] subs = DCODE.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        oc.Parameters.Add("K_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();

                    }
                    else {
                        string DCODE = kanban.Item.Trim();
                        char[] separators = new char[] { '#' };
                        string[] subs1 = DCODE.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        oc.Parameters.Add("K_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs1[0].ToUpper().Trim();

                    }

                    if (!string.IsNullOrEmpty(kanban.Modes) && !string.IsNullOrEmpty(kanban.KanbanNumber))
                    {
                        string DCODE = kanban.Item.Trim();
                        char[] separators = new char[] { '#' };
                        string[] subs2 = DCODE.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        oc.Parameters.Add("K_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();


                    }
                    else
                    {
                        string DCODE = kanban.Item.Trim();
                        char[] separators = new char[] { '#' };
                        string[] subs3 = DCODE.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        oc.Parameters.Add("K_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();
                    }
                    oc.Parameters.Add("K_SUMKTLOC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.SuperMarketLoc == null ? null : kanban.SuperMarketLoc.ToUpper().Trim();
                    oc.Parameters.Add("K_CAPACITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Capacity == null ? null : kanban.Capacity.ToUpper().Trim();
                    oc.Parameters.Add("K_SAFTY_STOCK_QUANTITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.SftStkQuantity == null ? null : kanban.SftStkQuantity.ToUpper().Trim();
                    oc.Parameters.Add("K_NO_OF_BINS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.NoOfBins == null ? null : kanban.NoOfBins.ToUpper().Trim();
                    oc.Parameters.Add("K_USAGE_PER_TRACTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.UsagePerTractor == null ? null : kanban.UsagePerTractor;
                    oc.Parameters.Add("K_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("K_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_MODES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(kanban.Modes) == true ? null : kanban.Modes.Trim();
                    oc.Parameters.Add("K_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertKanbanData";
                    oc.Parameters.Add("K_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    //ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                //throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool UpdatekanbanMaster(KanbanMaster kanban)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESKANBANMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("K_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.AutoId;
                    oc.Parameters.Add("K_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Plant.ToUpper().Trim();
                    oc.Parameters.Add("K_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Family.ToUpper().Trim();
                    oc.Parameters.Add("K_KANBAN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_SUMKTLOC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.SuperMarketLoc == null ? null : kanban.SuperMarketLoc.ToUpper().Trim();
                    oc.Parameters.Add("K_CAPACITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.Capacity == null ? null : kanban.Capacity.ToUpper().Trim();
                    oc.Parameters.Add("K_SAFTY_STOCK_QUANTITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.SftStkQuantity == null ? null : kanban.SftStkQuantity.ToUpper().Trim();
                    oc.Parameters.Add("K_NO_OF_BINS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_USAGE_PER_TRACTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    oc.Parameters.Add("K_MODES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateKanbanData";
                    oc.Parameters.Add("K_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                // throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool DeletekanbanMaster(KanbanMaster kanban)
        {
            bool result = false;
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_CRUD_XXESKANBANMASTER", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("K_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = kanban.AutoId.ToUpper().Trim();
                    oc.Parameters.Add("K_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_KANBAN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_SUMKTLOC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_CAPACITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_SAFTY_STOCK_QUANTITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_NO_OF_BINS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_USAGE_PER_TRACTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_MODES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    oc.Parameters.Add("K_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DeleteKanbanData";
                    oc.Parameters.Add("K_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    oc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        //All DDL Bind anD save for User Master
        public List<DDLTextValue> FillPrivillege(UserMaster UM)
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> Previllege = new List<DDLTextValue>();
                //TmpDs = returnDataTable("Select L_Code || ' # ' || L_Name as Level_Name,L_CODE from XXES_Level_Master order by L_Name");
                TmpDs = returnDataTable("Select L_Code || ' # ' || L_Name as Level_Name,L_CODE from XXES_Level_Master WHERE roll_for = 'WEB' order by L_Name");

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Previllege.Add(new DDLTextValue
                        {
                            Text = dr["Level_Name"].ToString(),
                            Value = dr["L_Code"].ToString(),
                        });
                    }
                }
                return Previllege;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> FillPUName(UserMaster UM)
        {
            DataTable TmpDs = null;
            try
            {

                List<DDLTextValue> PUNAME = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct character6 PUNAME from apps.qa_results_v  where character6 is not null and plan_id = 224155 and character1 = '" + Convert.ToString(UM.U_CODE) + "'");

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        PUNAME.Add(new DDLTextValue
                        {
                            Text = dr["PUNAME"].ToString(),
                            Value = dr["PUNAME"].ToString(),
                        });
                    }
                }
                return PUNAME;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> FillStageID(UserMaster UM)
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> STAGEID = new List<DDLTextValue>();
                TmpDs = returnDataTable("select STAGE_ID || ' # ' || STAGE_DESCRIPTION as Name ,Stage_Id from XXES_STAGE_MASTER where family_code='" + Convert.ToString(UM.FAMILY_CODE) + "' and plant_code='" + Convert.ToString(UM.U_CODE) + "'  union select '99 # ENGINE and HYDRAULIC' as Name ,'99' as Stage_Id from dual order by Stage_Id");

                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        STAGEID.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["Stage_Id"].ToString(),
                        });
                    }
                }
                return STAGEID;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }
        public DataTable GridUserMaster(UserMaster UM)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {


                query = string.Format(@"select UsrName,PsWord,LM.L_Code as Level_Name,UM.U_Code , Unt.U_Name,
                        FAMILYCODE,(select f.family_Code || ' # ' || f.family_Name as fName from XXES_FAMILY_MASTER f where f.family_code=UM.familycode) fName,STAGEID,ISACTIVE,
                        puname,STOREBYPASS, PDIUSER,MRNSAVE from XXES_Users_Master UM,XXES_Level_Master  LM,XXES_Unit_Master  Unt where LM.L_Code=UM.L_Code and Unt.U_Code=UM.U_Code and UM.U_Code='" + UM.U_CODE + "' AND LM.roll_for = 'WEB' order by UsrName");

                dt = returnDataTable(query);
                if(dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        
                        row["PsWord"] = bed.base64Decode(row["PsWord"].ToString().Trim());
                    }
                }
                
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;

        }
        public bool InsertUserMaster(UserMaster UM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_USERMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;

                    sc.Parameters.Add("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.USRNAME) ? null : UM.USRNAME.ToUpper().Trim();
                    string encriptedPw = bed.base64Encode(UM.PSWORD.Trim());
                    sc.Parameters.Add("P_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.PSWORD) ? null : encriptedPw;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.U_CODE) ? null : UM.U_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_L_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.Level_Name) ? null : UM.Level_Name.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.FAMILY_CODE) ? null : UM.FAMILY_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGEID", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.STAGEID) ? null : UM.STAGEID.ToUpper().Trim();
                    sc.Parameters.Add("P_ISACTIVE", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.ISACTIVE == true ? "1" : "0";
                    sc.Parameters.Add("P_GATEUSER", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PUNAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.PUNAME) ? null : UM.PUNAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STOREBYPASS", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.STOREBYPASS == true ? "Y" : "N";
                    sc.Parameters.Add("P_PDIUSER", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.PDIUSER == true ? "Y" : "N";
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_MRNSAVE", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.MRNSAVE == true ? "Y" : "N";
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertUserMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    int a = sc.ExecuteNonQuery();

                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;


        }

        public bool UpdateUserMaster(UserMaster UM)
        {

            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_USERMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;

                    sc.Parameters.Add("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.USRNAME) ? null : UM.USRNAME.ToUpper().Trim();
                    string encriptedPw = bed.base64Encode(UM.PSWORD.Trim());
                    sc.Parameters.Add("P_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.PSWORD) ? null : encriptedPw;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.U_CODE) ? null : UM.U_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_L_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.Level_Name) ? null : UM.Level_Name.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.FAMILY_CODE) ? null : UM.FAMILY_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_STAGEID", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.STAGEID) ? null : UM.STAGEID.ToUpper().Trim();
                    sc.Parameters.Add("P_ISACTIVE", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.ISACTIVE == true ? "1" : "0";
                    sc.Parameters.Add("P_GATEUSER", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PUNAME", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(UM.PUNAME) ? null : UM.PUNAME.ToUpper().Trim();
                    sc.Parameters.Add("P_STOREBYPASS", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.STOREBYPASS == true ? "Y" : "N";
                    sc.Parameters.Add("P_PDIUSER", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.PDIUSER == true ? "Y" : "N";
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_MRNSAVE", OracleDbType.Varchar2, ParameterDirection.Input).Value = UM.MRNSAVE == true ? "Y" : "N";
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateUserMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    int a = sc.ExecuteNonQuery();

                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool DeleteFIP(FIPModel FM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_FIPMODEL_CODE", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_MODEL_CODE_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ENTRYDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = FM.AUTOID;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Delete";

                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool InsertTractorMaster(TractorMster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_TRACTORMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Plant.Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Family.Trim();
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode.Trim();
                    sc.Parameters.Add("P_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode_Desc.Trim();
                    sc.Parameters.Add("P_ENGINE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Engine) == true ? null : itemModel.Engine.Trim();
                    sc.Parameters.Add("P_ENGINE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Engine_Desc) == true ? null : itemModel.Engine_Desc.Trim();
                    sc.Parameters.Add("P_BACKEND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Backend) == true ? null : itemModel.Backend.Trim();
                    sc.Parameters.Add("P_BACKEND_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Backend_Desc) == true ? null : itemModel.Backend_Desc.Trim();
                    sc.Parameters.Add("P_TRANSMISSION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Transmission) == true ? null : itemModel.Transmission.Trim();
                    sc.Parameters.Add("P_TRANSMISSION_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Transmission_Desc) == true ? null : itemModel.Transmission_Desc.Trim();
                    sc.Parameters.Add("P_REARAXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearAxel) == true ? null : itemModel.RearAxel.Trim();
                    sc.Parameters.Add("P_REARAXEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearAxel_Desc) == true ? null : itemModel.RearAxel_Desc.Trim();
                    sc.Parameters.Add("P_HYDRAULIC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Hydraulic) == true ? null : itemModel.Hydraulic.Trim();
                    sc.Parameters.Add("P_HYDRAULIC_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Hydraulic_Desc) == true ? null : itemModel.Hydraulic_Desc.Trim();
                    sc.Parameters.Add("P_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontTyre) == true ? null : itemModel.FrontTyre.Trim();
                    sc.Parameters.Add("P_FRONTTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontTyre_Desc) == true ? null : itemModel.FrontTyre_Desc.Trim();
                    sc.Parameters.Add("P_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearTyre) == true ? null : itemModel.RearTyre.Trim();
                    sc.Parameters.Add("P_REARTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearTyre_Desc) == true ? null : itemModel.RearTyre_Desc.Trim();
                    sc.Parameters.Add("P_BATTERY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Battery) == true ? null : itemModel.Battery.Trim();
                    sc.Parameters.Add("P_BATTERY_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Battery_Desc) == true ? null : itemModel.Battery_Desc.Trim();
                    sc.Parameters.Add("P_PREFIX_1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix1) == true ? null : itemModel.Prefix1.Trim().ToUpper();
                    sc.Parameters.Add("P_PREFIX_2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix2) == true ? null : itemModel.Prefix2.Trim().ToUpper();
                    sc.Parameters.Add("P_PREFIX_3", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix3) == true ? null : itemModel.Prefix3.Trim().ToUpper();
                    sc.Parameters.Add("P_PREFIX_4", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix4) == true ? null : itemModel.Prefix4.Trim().ToUpper();
                    sc.Parameters.Add("P_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Remarks) == true ? null : itemModel.Remarks.Trim().ToUpper();
                    sc.Parameters.Add("P_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ORG_ID.Trim();
                    sc.Parameters.Add("P_REQUIRE_ENGINE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.EngineChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_TRANS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.TransmissionChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_REARAXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearAxelChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_BACKEND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.BackendChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_HYD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.HydraulicChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.FrontTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_BATTERY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.BatteryChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_GEN_SRNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.GenerateSerialNoChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_SHORT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ShortDesc) == true ? null : itemModel.ShortDesc.Trim();
                    sc.Parameters.Add("P_HYD_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HydraulicPump) == true ? null : itemModel.HydraulicPump.Trim();
                    sc.Parameters.Add("P_HYD_PUMP_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HydraulicPump_Desc) == true ? null : itemModel.HydraulicPump_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringMotor) == true ? null : itemModel.SteeringMotor.Trim();
                    sc.Parameters.Add("P_STEERING_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringMotor_Desc) == true ? null : itemModel.SteeringMotor_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_ASSEMBLY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringAssembly) == true ? null : itemModel.SteeringAssembly.Trim();
                    sc.Parameters.Add("P_STEERING_ASSEMBLY_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringAssembly_Desc) == true ? null : itemModel.SteeringAssembly_Desc.Trim();
                    sc.Parameters.Add("P_STERING_CYLI   DER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringCylinder) == true ? null : itemModel.SteeringCylinder.Trim();
                    sc.Parameters.Add("P_STERING_CYLINDER_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringCylender_Desc) == true ? null : itemModel.SteeringCylender_Desc.Trim();
                    sc.Parameters.Add("P_RADIATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorAssembly) == true ? null : itemModel.RadiatorAssembly.Trim();
                    sc.Parameters.Add("P_RADIATOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorAssembly_Desc) == true ? null : itemModel.RadiatorAssembly_Desc.Trim();
                    sc.Parameters.Add("P_CLUSSTER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterAssembly) == true ? null : itemModel.ClusterAssembly.Trim();
                    sc.Parameters.Add("P_CLUSSTER_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterAssembly_Desc) == true ? null : itemModel.ClusterAssembly_Desc.Trim();
                    sc.Parameters.Add("P_ALTERNATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Alternator) == true ? null : itemModel.Alternator.Trim();
                    sc.Parameters.Add("P_ALTERNATOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Alternator_Desc) == true ? null : itemModel.Alternator_Desc.Trim();
                    sc.Parameters.Add("P_STARTER_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.StartorMotor) == true ? null : itemModel.StartorMotor.Trim();
                    sc.Parameters.Add("P_STARTER_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.StartorMotor_Desc) == true ? null : itemModel.StartorMotor_Desc.Trim();
                    sc.Parameters.Add("P_REQ_HYD_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.HydraulicPumpChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STEERING_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STEERING_ASSEMBLY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STERING_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringCylinderChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_RADIATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RadiatorAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_CLUSSTER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ClusterAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_ALTERNATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AlternatorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STARTER_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.StartorMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.DomesticExport) == true ? null : itemModel.DomesticExport.Trim();
                    sc.Parameters.Add("P_FRONTAXEL_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsFrontAxel) == true ? null : itemModel.NoOfBoltsFrontAxel.Trim();
                    sc.Parameters.Add("P_HYDRAULIC_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsHydraulic) == true ? null : itemModel.NoOfBoltsHydraulic.Trim();
                    sc.Parameters.Add("P_FRONTTYRE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsFrontTYre) == true ? null : itemModel.NoOfBoltsFrontTYre.Trim();
                    sc.Parameters.Add("P_REARTYRE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsRearTYre) == true ? null : itemModel.NoOfBoltsRearTYre.Trim();
                    sc.Parameters.Add("P_EN_TORQUE1_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe1) == true ? null : itemModel.NoOfBoltsEnToruqe1.Trim();
                    sc.Parameters.Add("P_EN_TORQUE2_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe2) == true ? null : itemModel.NoOfBoltsEnToruqe2.Trim();
                    sc.Parameters.Add("P_EN_TORQUE3_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe3) == true ? null : itemModel.NoOfBoltsEnToruqe3.Trim();
                    sc.Parameters.Add("P_BK_TORQUE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BK_BOLT_VALUE) == true ? null : itemModel.BK_BOLT_VALUE.Trim();
                    sc.Parameters.Add("P_REQ_ROPS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RopsChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_ROPS_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Rops) == true ? null : itemModel.Rops.Trim();
                    sc.Parameters.Add("P_REQ_CAREBTN", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.EnableCarButtonChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_SEQ_NOT_REQUIRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Seq_Not_RequireChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_BRAKE_PEDAL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRAKE_PEDAL) == true ? null : itemModel.BRAKE_PEDAL.Trim();
                    sc.Parameters.Add("P_BRAKE_PEDAL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BrakePedal_Desc) == true ? null : itemModel.BrakePedal_Desc.Trim();
                    sc.Parameters.Add("P_CLUTCH_PEDAL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CLUTCH_PEDAL) == true ? null : itemModel.CLUTCH_PEDAL.Trim();
                    sc.Parameters.Add("P_CLUTCH_PEDAL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClutchPedal_Desc) == true ? null : itemModel.ClutchPedal_Desc.Trim();
                    sc.Parameters.Add("P_SPOOL_VALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SPOOL_VALUE) == true ? null : itemModel.SPOOL_VALUE.Trim();
                    sc.Parameters.Add("P_SPOOL_VALUE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SpoolValue_Desc) == true ? null : itemModel.SpoolValue_Desc.Trim();
                    sc.Parameters.Add("P_TANDEM_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TANDEM_PUMP) == true ? null : itemModel.TANDEM_PUMP.Trim();
                    sc.Parameters.Add("P_TANDEM_PUMP_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TandemPump_Desc) == true ? null : itemModel.TandemPump_Desc.Trim();
                    sc.Parameters.Add("P_FENDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FENDER) == true ? null : itemModel.FENDER.Trim();
                    sc.Parameters.Add("P_FENDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Fender_Desc) == true ? null : itemModel.Fender_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_RAILING", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FENDER_RAILING) == true ? null : itemModel.FENDER_RAILING.Trim();
                    sc.Parameters.Add("P_FENDER_RAILING_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRailing_Desc) == true ? null : itemModel.FenderRailing_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HEAD_LAMP) == true ? null : itemModel.HEAD_LAMP.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLamp_Desc) == true ? null : itemModel.HeadLamp_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_WHEEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.STEERING_WHEEL) == true ? null : itemModel.STEERING_WHEEL.Trim();
                    sc.Parameters.Add("P_STEERING_WHEEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringWheel_Desc) == true ? null : itemModel.SteeringWheel_Desc.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_WIRING_HARNESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.REAR_HOOD_WIRING_HARNESS) == true ? null : itemModel.REAR_HOOD_WIRING_HARNESS.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_WIRING_HRNE_DES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHoolWiringHarness_Desc) == true ? null : itemModel.RearHoolWiringHarness_Desc.Trim();
                    sc.Parameters.Add("P_SEAT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SEAT) == true ? null : itemModel.SEAT.Trim();
                    sc.Parameters.Add("P_SEAT_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Seat_Desc) == true ? null : itemModel.Seat_Desc.Trim();
                    sc.Parameters.Add("P_ELECTRIC_TRACTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ElectricMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_RH_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHFrontTyre) == true ? null : itemModel.RHFrontTyre.Trim();
                    sc.Parameters.Add("P_RH_FRONTTYRE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHFrontTyre_Desc) == true ? null : itemModel.RHFrontTyre_Desc.Trim();
                    sc.Parameters.Add("P_RH_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHRearTyre) == true ? null : itemModel.RHRearTyre.Trim();
                    sc.Parameters.Add("P_RH_REARTYRE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHRearTyre_Desc) == true ? null : itemModel.RHRearTyre_Desc.Trim();
                    sc.Parameters.Add("P_REQ_RHFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RHFrontTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_RHRT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RHRearTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertTractorMaster";
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public static string orCnstr = Convert.ToString(ConfigurationManager.ConnectionStrings["CON"]);
        public DateTime ServerDate, Login_Time;
        public List<DDLTextValue> Fill_SuperMkt()
        {
            DataTable DT = null;
            try
            {
                List<DDLTextValue> SM = new List<DDLTextValue>();

                DT = returnDataTable("SELECT PARAMETERINFO TEXT, PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE DESCRIPTION='SUPER_MARKETS'");


                if (DT.Rows.Count > 0)
                {
                    foreach (DataRow dr in DT.AsEnumerable())
                    {
                        SM.Add(new DDLTextValue
                        {
                            Text = dr["TEXT"].ToString(),
                            Value = dr["VALUE"].ToString(),
                        });
                    }
                }
                return SM;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;

            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> Fill_ZONE()
        {
            DataTable DT = null;
            try
            {
                List<DDLTextValue> ZN = new List<DDLTextValue>();

                DT = returnDataTable("SELECT PARAMETERINFO TEXT, PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE DESCRIPTION='SUPER_ZONES'");


                if (DT.Rows.Count > 0)
                {
                    foreach (DataRow dr in DT.AsEnumerable())
                    {
                        ZN.Add(new DDLTextValue
                        {
                            Text = dr["TEXT"].ToString(),
                            Value = dr["VALUE"].ToString(),
                        });
                    }
                }
                return ZN;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;

            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Location(KanbanMaster data)
        {
            DataTable DT = null;
            try
            {
                List<DDLTextValue> LN = new List<DDLTextValue>();

                DT = returnDataTable("select DISTINCT LOCATION_NAME from XXES_SUPERMKT_LOCATIONS WHERE PLANT_CODE = '" + data.Plant.Trim() + "' AND FAMILY_CODE = '" + data.Family.Trim() + "'");


                if (DT.Rows.Count > 0)
                {
                    foreach (DataRow dr in DT.AsEnumerable())
                    {
                        LN.Add(new DDLTextValue
                        {
                            Text = dr["LOCATION_NAME"].ToString(),
                            Value = dr["LOCATION_NAME"].ToString(),
                        });
                    }
                }
                return LN;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;

            }
            finally
            {
                ConClose();
            }
        }
        public List<DDLTextValue> Fill_Plant_Name()
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> plant = new List<DDLTextValue>();
                TmpDs = returnDataTable(@"Select PLANT_CODE as Plant_Name from XXES_BULKSTORAGEITEMS GROUP BY plant_code order by Plant_Name");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        plant.Add(new DDLTextValue
                        {
                            Text = dr["Plant_Name"].ToString(),
                            Value = dr["Plant_Name"].ToString(),
                        });
                    }
                }
                return plant;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }


        public List<DDLTextValue> Fill_Family_Name(string Plant)
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> family = new List<DDLTextValue>();
                TmpDs = returnDataTable(@"Select FAMILY_CODE as Family_Name from XXES_BULKSTORAGEITEMS where PLANT_CODE= '" + Plant.ToUpper().Trim() + "' GROUP BY family_code order by Family_Name");
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        family.Add(new DDLTextValue
                        {
                            Text = dr["Family_Name"].ToString(),
                            Value = dr["Family_Name"].ToString(),
                        });
                    }
                }
                return family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            //finally
            //{
            //    ConClose();
            //}
        }
        public List<DDLTextValue> BindOperation()  //For Layout Page
        {
            List<DDLTextValue> list = new List<DDLTextValue>();
            list.Add(new DDLTextValue { Value = "VL", Text = "BULK STORAGE" });
            list.Add(new DDLTextValue { Value = "SM", Text = "SUPER MARKET STORAGE" });

            return list;
        }

        public bool IsFloatOrInt(string value)
        {
            int intValue;
            float floatValue;
            return Int32.TryParse(value, out intValue) || float.TryParse(value, out floatValue);
        }

        public List<DDLTextValue> FillStageID(UserMaster UM, int? SelectOption = 0)
        {
            DataTable TmpDs = null;
            try
            {
                List<DDLTextValue> STAGEID = new List<DDLTextValue>();
                TmpDs = returnDataTable("select STAGE_ID || ' # ' || STAGE_DESCRIPTION as Name ,Stage_Id from XXES_STAGE_MASTER where family_code='" + Convert.ToString(UM.FAMILY_CODE) + "' and plant_code='" + Convert.ToString(UM.U_CODE) + "'  union select '99 # ENGINE and HYDRAULIC' as Name ,'99' as Stage_Id from dual order by Stage_Id");

                if (SelectOption == 1)
                {
                    STAGEID.Add(new DDLTextValue
                    {
                        Text = "---SELECT---",
                        Value = "0",
                    });
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        STAGEID.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["Stage_Id"].ToString(),
                        });
                    }
                }
                return STAGEID;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }


        public List<DDLTextValue> Fill_All_Family(string ucode, int? SelectOption = 0)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                TmpDs = returnDataTable(@"Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER 
                    where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                //if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["LoginFamily"])))
                //{
                //    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                //}
                //else
                //{
                //    TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "') and family_code='" + Convert.ToString(HttpContext.Current.Session["LoginFamily"]) + "'");
                //}'
                if (SelectOption == 1)
                {
                    Family.Add(new DDLTextValue
                    {
                        Text = "---SELECT---",
                        Value = "0"
                    });
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }
        public List<ChartModel> DashBoardMaster(DashboardModel model)
        {
            List<ChartModel> CM = new List<ChartModel>();
            try
            {
                using (OracleCommand oc = new OracleCommand("UDSP_DASHBOARD", Connection()))
                {
                    con.Open();
                    //oc.CommandType = CommandType.StoredProcedure;
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = model.Plant.Trim();
                    oc.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = model.Family.Trim();
                    oc.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = model.FromDate.Trim();
                    oc.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = model.ToDate.Trim();
                    oc.Parameters.Add("pDASHBOARDTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "DAY_PRODUCTION";
                    oc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    DataTable dt = new DataTable();

                    oda.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            CM.Add(new ChartModel
                            {
                                Item = dr["ITEM_CODE"].ToString(),
                                ItemDesc = dr["ITEM_DESC"].ToString(),
                                Quantity = Convert.ToInt32(dr["TOTAL"]),
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                LogWrite(ex);
            }
            finally
            {
                con.Close();
            }
            return CM;

        }
        public List<DDLTextValue> Fill_Engine_Family(string ucode)
        {

            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as   Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "' and family_code like 'ENGINE%')");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);

            }
            finally { }
            return Family;
        }

        public bool DeleteEngineMaster(EngineModel EM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_ENGINEMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FUEL_INJECTION_PUMP_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYLINDER_BLOCK_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYLINDER_HEAD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CONNECTIND_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CONNECTIND_ROD_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CRANK_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CAM_SHAFT_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_INJECTOR", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQ_FUEL_INJECTION_PUMP", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_BLOCK", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQUIRE_CYNLINDER_HEAD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQUIRE_CONNECTING_ROD", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQUIRE_CRANK_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQUIRE_CAM_SHAFT", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REQ_ECU", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PREFIX_1", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PREFIX_2", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_REMARKS1", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_NO_OF_PISTONS", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = EM.AUTOID;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "DeleteEngineMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool InsertEngineStatus(string PlantCode, string FamilyCode, string ItemCode, string EngineSrNo, string FuelInjection, string FuelInjectionSrNo, string CylinderHead, string CylinderHeadSrNo,
         string CylinderBlock, string CylinderBlockSrNo, string Crankshaft, string CrankshaftSrNo, string Camshaft, string CamshaftSrNo, string ConnectingRod, string ConnectingRodSrNo1, string ConnectingRodSrNo2,
         string ConnectingRodSrNo3, string ConnectingRodSrNo4, string Injector1, string Injector2, string Injector3, string Injector4, int status)
        {
            bool result = false;
            try
            {
                ConOpen();
                using (OracleCommand comm = new OracleCommand("UDSP_INSERTENGINESTATUS", Connection()))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("PlantCode", PlantCode);
                    comm.Parameters.Add("FamilyCode", FamilyCode);
                    comm.Parameters.Add("ItemCode", ItemCode);
                    comm.Parameters.Add("EngineSrNo", EngineSrNo);
                    comm.Parameters.Add("FuelInection", FuelInjection);
                    comm.Parameters.Add("FuelInectionSrNo", FuelInjectionSrNo);
                    comm.Parameters.Add("CylinderHead", CylinderHead);
                    comm.Parameters.Add("CylinderHeadSrNo", CylinderHeadSrNo);
                    comm.Parameters.Add("CylinderBlock", CylinderBlock);
                    comm.Parameters.Add("CylinderBlockSrNo", CylinderBlockSrNo);
                    comm.Parameters.Add("Crankshaft", Crankshaft);
                    comm.Parameters.Add("CrankshaftSrNo", CrankshaftSrNo);
                    comm.Parameters.Add("Camshaft", Camshaft);
                    comm.Parameters.Add("CamshaftSrNo", CamshaftSrNo);
                    comm.Parameters.Add("ConnectingRod", ConnectingRod);
                    comm.Parameters.Add("ConnectingRodSrNo1", ConnectingRodSrNo1);
                    comm.Parameters.Add("ConnectingRodSrNo2", ConnectingRodSrNo2);
                    comm.Parameters.Add("ConnectingRodSrNo3", ConnectingRodSrNo3);
                    comm.Parameters.Add("ConnectingRodSrNo4", ConnectingRodSrNo4);
                    comm.Parameters.Add("InjectorSr1", Injector1);
                    comm.Parameters.Add("InjectorSr2", Injector2);
                    comm.Parameters.Add("InjectorSr3", Injector3);
                    comm.Parameters.Add("InjectorSr4", Injector4);
                    comm.Parameters.Add("Status", status);
                    comm.ExecuteNonQuery();
                    //ConClose();
                    result = true;

                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public void Insert_Part_Audit_Data(string plant, string family, string Item_code, string Item_SrNo, string Part_Itemcode, string Part_SrNo, string Part_Desc, string Barcode_Data, string EXIST_JOB, string NEW_JOB, string Remarks1, string Remarks2)
        {
            try
            {
                DateTime ServerDateTime = GetServerDateTime().Date;
                string query = @"INSERT INTO XXES_PARTS_AUDIT_DATA(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_SRLNO,PART_ITEM_CODE,PART_SRLNO,PART_DESC,BARCODE_DATA,REMARKS1,REMARKS2,EXIST_JOB,NEW_JOB,LOGIN_USER,SYSTEM) 
                                values('" + plant.Trim().ToUpper() + "','" + family.Trim().ToUpper() + "','" + Item_code.Trim().ToUpper() + "','" + Item_SrNo.Trim().ToUpper() + "','"
                                          + Part_Itemcode.Trim().ToUpper() + "','" + Part_SrNo.Trim().ToUpper() + "','" + Part_Desc.Trim().ToUpper() + "','" + Barcode_Data.Trim().ToUpper() + "','" + Remarks1.Trim() + "','" + Remarks2.Trim() + "','" + EXIST_JOB.Trim().ToUpper() + "','" + NEW_JOB.Trim().ToUpper() + "','" + HttpContext.Current.Session["Login_Unit"] + "','" + GetUserIP().Trim() + "')";
                EXEC_QUERY(query);
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
        }
        public void Insert_Part_Audit_DataNEW(string plant, string family, string Item_code, string Item_SrNo, string Part_Itemcode, string Part_Desc, string Remarks1, string Remarks2, string NEW_Part,string NEW_PART_DESC,string TrancationType,int TransactionNumber)
        {
            try
            {
                DateTime ServerDateTime = GetServerDateTime().Date;
                string query = @"INSERT INTO XXES_PARTS_AUDIT_DATA(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ENTRYDATE,PART_ITEM_CODE,OLD_PART_DESC,REMARKS1,REMARKS2,PART_NEW_ITEMCODE,NEW_PART_DESC,LOGIN_USER,SYSTEM,PART_DESC,TRANSACTION_NUMBER) 
                                values('" + plant.Trim().ToUpper() + "','" + family.Trim().ToUpper() + "','" + Item_code.Trim().ToUpper() + "',SYSDATE,'"
                                          + Part_Itemcode.Trim().ToUpper() + "','" + Part_Desc.Trim().ToUpper() + "','" + Remarks1.Trim() + "','" + Remarks2.Trim() + "','" + NEW_Part.Trim().ToUpper() + "','" + NEW_PART_DESC.Trim().ToUpper() + "','" + HttpContext.Current.Session["Login_User"] + "','" + GetUserIP().Trim() + "','" + TrancationType.Trim().ToUpper() + "'," + TransactionNumber + ")";
                EXEC_QUERY(query);
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
        }

        public void InsertBarcodeData(string PlantCode, string FamilyCode, string ItemCode, string EngineSrNo, string MarkMonth, string MarkDate, string MarkYear,
       string MarkTime, string VendorCode, string SrNo, string HeatCode, string Remark1, string BarcodeData)
        {
            try
            {
                ConOpen();
                using (OracleCommand comm = new OracleCommand("UDSP_InsertBarcodeData", Connection()))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("PlantCode", PlantCode);
                    comm.Parameters.Add("FamilyCode", FamilyCode);
                    comm.Parameters.Add("EngineSrNo", EngineSrNo);
                    comm.Parameters.Add("ItemCode", ItemCode);
                    comm.Parameters.Add("MarkMonth", MarkMonth);
                    comm.Parameters.Add("MarkDate", MarkDate);
                    comm.Parameters.Add("MarkYear", MarkYear);
                    comm.Parameters.Add("MarkTime", MarkTime);
                    comm.Parameters.Add("VendorCode", VendorCode);
                    comm.Parameters.Add("SrNo", SrNo);
                    comm.Parameters.Add("HeatCode", HeatCode);
                    comm.Parameters.Add("Remarks1", Remark1);
                    comm.Parameters.Add("BarcodeData", BarcodeData);
                    comm.ExecuteNonQuery();
                    //ConClose();
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);

            }
            finally
            {
                ConClose();
            }
        }

        public bool InsertHydrualicMaster(HydrualicMaster HM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_HYDRULICMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;

                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(HM.PLANT_CODE) ? null : HM.PLANT_CODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(HM.FAMILY_CODE) ? null : HM.FAMILY_CODE.ToUpper().Trim();

                    string Item_DescValue = HM.ITEM_CODE.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = Item_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();


                    sc.Parameters.Add("P_SHORT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(HM.SHORT_CODE) ? null : HM.SHORT_CODE.ToUpper().Trim();


                    string SpoolValue_DescValue = HM.SPOOL_VALUE.Trim();
                    char[] separators1 = new char[] { '#' };
                    string[] subs1 = SpoolValue_DescValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_SPOOL_VALUE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[0].ToUpper().Trim();
                    sc.Parameters.Add("P_SPOOL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();

                    string Cylinder_DescValue = HM.CYLINDER.Trim();
                    char[] separators2 = new char[] { '#' };
                    string[] subs2 = SpoolValue_DescValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_CYNLIDER", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[0].ToUpper().Trim();
                    sc.Parameters.Add("P_CYNLINDER_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();

                    string Part1_DescValue = HM.PART1.Trim();
                    char[] separators3 = new char[] { '#' };
                    string[] subs3 = Part1_DescValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_PART1", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[0].ToUpper().Trim();
                    sc.Parameters.Add("P_PART1_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();

                    string Part2_DescValue = HM.PART2.Trim();
                    char[] separators4 = new char[] { '#' };
                    string[] subs4 = Part2_DescValue.Split(separators4, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_PART2", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[0].ToUpper().Trim();
                    sc.Parameters.Add("P_PART2_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[1].ToUpper().Trim();

                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "InsertHydrualicMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdateHydrualicMaster(HydrualicMaster HM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_HYDRULICMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;

                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = HM.AUTOID;

                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;

                    string Item_DescValue = HM.ITEM_CODE.Trim();
                    char[] separators = new char[] { '#' };
                    string[] subs = Item_DescValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[0].ToUpper().Trim();
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs[1].ToUpper().Trim();


                    sc.Parameters.Add("P_SHORT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(HM.SHORT_CODE) ? null : HM.SHORT_CODE.ToUpper().Trim();


                    string SpoolValue_DescValue = HM.SPOOL_VALUE.Trim();
                    char[] separators1 = new char[] { '#' };
                    string[] subs1 = SpoolValue_DescValue.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_SPOOL_VALUE", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[0].ToUpper().Trim();
                    sc.Parameters.Add("P_SPOOL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs1[1].ToUpper().Trim();

                    string Cylinder_DescValue = HM.CYLINDER.Trim();
                    char[] separators2 = new char[] { '#' };
                    string[] subs2 = SpoolValue_DescValue.Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_CYNLIDER", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[0].ToUpper().Trim();
                    sc.Parameters.Add("P_CYNLINDER_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs2[1].ToUpper().Trim();

                    string Part1_DescValue = HM.PART1.Trim();
                    char[] separators3 = new char[] { '#' };
                    string[] subs3 = Part1_DescValue.Split(separators3, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_PART1", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[0].ToUpper().Trim();
                    sc.Parameters.Add("P_PART1_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs3[1].ToUpper().Trim();

                    string Part2_DescValue = HM.PART2.Trim();
                    char[] separators4 = new char[] { '#' };
                    string[] subs4 = Part2_DescValue.Split(separators4, StringSplitOptions.RemoveEmptyEntries);
                    sc.Parameters.Add("P_PART2", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[0].ToUpper().Trim();
                    sc.Parameters.Add("P_PART2_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = subs4[1].ToUpper().Trim();

                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "UpdateHydrualicMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public DataTable HydrualicMasterGrid(HydrualicMaster HM)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"SELECT AUTOID,PLANT_CODE, FAMILY_CODE, ITEM_CODE, ITEM_DESC, SHORT_CODE, SPOOL_VALUE, SPOOL_DESC, CYNLINDER, CYNLINDER_DESC,
                        PART1, PART1_DESC, PART2, PART2_DESC, CREATEDBY, CREATEDDATE, UPDATEDBY, UPDATEDDATE from 
                            XXES_HYDRUALIC_MASTER where PLANT_CODE= '" + HM.PLANT_CODE + "' AND FAMILY_CODE= '" + HM.FAMILY_CODE + "' ORDER BY AUTOID DESC");

                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;

        }

        public bool DeleteHydrualicMaster(HydrualicMaster HM)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_HYDRULICMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_AUTOID", OracleDbType.Varchar2, ParameterDirection.Input).Value = HM.AUTOID;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_ITEM_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SHORT_CODE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SPOOL_VALUE", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_SPOOL_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYNLIDER", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CYNLINDER_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PART1", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PART1_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PART2", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_PART2_DESC", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATEDBY", OracleDbType.Varchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.Varchar2, ParameterDirection.Input).Value = "DeleteHydrualicMaster";
                    sc.Parameters.Add("P_res", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public bool UpdateTractorMaster(TractorMster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_TRACTORMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Plant.Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Family.Trim();
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode.Trim();
                    sc.Parameters.Add("P_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ItemCode_Desc.Trim();
                    sc.Parameters.Add("P_ENGINE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Engine) == true ? null : itemModel.Engine.Trim();
                    sc.Parameters.Add("P_ENGINE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Engine_Desc) == true ? null : itemModel.Engine_Desc.Trim();
                    sc.Parameters.Add("P_BACKEND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Backend) == true ? null : itemModel.Backend.Trim();
                    sc.Parameters.Add("P_BACKEND_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Backend_Desc) == true ? null : itemModel.Backend_Desc.Trim();
                    sc.Parameters.Add("P_TRANSMISSION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Transmission) == true ? null : itemModel.Transmission.Trim();
                    sc.Parameters.Add("P_TRANSMISSION_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Transmission_Desc) == true ? null : itemModel.Transmission_Desc.Trim();
                    sc.Parameters.Add("P_REARAXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearAxel) == true ? null : itemModel.RearAxel.Trim();
                    sc.Parameters.Add("P_REARAXEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearAxel_Desc) == true ? null : itemModel.RearAxel_Desc.Trim();
                    sc.Parameters.Add("P_HYDRAULIC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Hydraulic) == true ? null : itemModel.Hydraulic.Trim();
                    sc.Parameters.Add("P_HYDRAULIC_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Hydraulic_Desc) == true ? null : itemModel.Hydraulic_Desc.Trim();
                    sc.Parameters.Add("P_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontTyre) == true ? null : itemModel.FrontTyre.Trim();
                    sc.Parameters.Add("P_FRONTTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontTyre_Desc) == true ? null : itemModel.FrontTyre_Desc.Trim();
                    sc.Parameters.Add("P_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearTyre) == true ? null : itemModel.RearTyre.Trim();
                    sc.Parameters.Add("P_REARTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearTyre_Desc) == true ? null : itemModel.RearTyre_Desc.Trim();
                    sc.Parameters.Add("P_BATTERY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Battery) == true ? null : itemModel.Battery.Trim();
                    sc.Parameters.Add("P_BATTERY_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Battery_Desc) == true ? null : itemModel.Battery_Desc.Trim();
                    sc.Parameters.Add("P_PREFIX_1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix1) == true ? null : itemModel.Prefix1.Trim();
                    sc.Parameters.Add("P_PREFIX_2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix2) == true ? null : itemModel.Prefix2.Trim();
                    sc.Parameters.Add("P_PREFIX_3", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix3) == true ? null : itemModel.Prefix3.Trim();
                    sc.Parameters.Add("P_PREFIX_4", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Prefix4) == true ? null : itemModel.Prefix4.Trim();
                    sc.Parameters.Add("P_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Remarks) == true ? null : itemModel.Remarks.Trim();
                    sc.Parameters.Add("P_ORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ORG_ID.Trim();
                    sc.Parameters.Add("P_REQUIRE_ENGINE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.EngineChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_TRANS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.TransmissionChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_REARAXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearAxelChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_BACKEND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.BackendChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_HYD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.HydraulicChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.FrontTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQUIRE_BATTERY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.BatteryChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_GEN_SRNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.GenerateSerialNoChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_SHORT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ShortDesc) == true ? null : itemModel.ShortDesc.Trim();
                    sc.Parameters.Add("P_HYD_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HydraulicPump) == true ? null : itemModel.HydraulicPump.Trim();
                    sc.Parameters.Add("P_HYD_PUMP_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HydraulicPump_Desc) == true ? null : itemModel.HydraulicPump_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringMotor) == true ? null : itemModel.SteeringMotor.Trim();
                    sc.Parameters.Add("P_STEERING_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringMotor_Desc) == true ? null : itemModel.SteeringMotor_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_ASSEMBLY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringAssembly) == true ? null : itemModel.SteeringAssembly.Trim();
                    sc.Parameters.Add("P_STEERING_ASSEMBLY_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringAssembly_Desc) == true ? null : itemModel.SteeringAssembly_Desc.Trim();
                    sc.Parameters.Add("P_STERING_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringCylinder) == true ? null : itemModel.SteeringCylinder.Trim();
                    sc.Parameters.Add("P_STERING_CYLINDER_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringCylender_Desc) == true ? null : itemModel.SteeringCylender_Desc.Trim();
                    sc.Parameters.Add("P_RADIATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorAssembly) == true ? null : itemModel.RadiatorAssembly.Trim();
                    sc.Parameters.Add("P_RADIATOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorAssembly_Desc) == true ? null : itemModel.RadiatorAssembly_Desc.Trim();
                    sc.Parameters.Add("P_CLUSSTER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterAssembly) == true ? null : itemModel.ClusterAssembly.Trim();
                    sc.Parameters.Add("P_CLUSSTER_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterAssembly_Desc) == true ? null : itemModel.ClusterAssembly_Desc.Trim();
                    sc.Parameters.Add("P_ALTERNATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Alternator) == true ? null : itemModel.Alternator.Trim();
                    sc.Parameters.Add("P_ALTERNATOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Alternator_Desc) == true ? null : itemModel.Alternator_Desc.Trim();
                    sc.Parameters.Add("P_STARTER_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.StartorMotor) == true ? null : itemModel.StartorMotor.Trim();
                    sc.Parameters.Add("P_STARTER_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.StartorMotor_Desc) == true ? null : itemModel.StartorMotor_Desc.Trim();
                    sc.Parameters.Add("P_REQ_HYD_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.HydraulicPumpChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STEERING_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STEERING_ASSEMBLY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STERING_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.SteeringCylinderChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_RADIATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RadiatorAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_CLUSSTER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ClusterAssemblyChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_ALTERNATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.AlternatorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_STARTER_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.StartorMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_MODEL_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.DomesticExport) == true ? null : itemModel.DomesticExport.Trim().ToUpper();
                    sc.Parameters.Add("P_FRONTAXEL_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsFrontAxel) == true ? null : itemModel.NoOfBoltsFrontAxel.Trim();
                    sc.Parameters.Add("P_HYDRAULIC_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsHydraulic) == true ? null : itemModel.NoOfBoltsHydraulic.Trim();
                    sc.Parameters.Add("P_FRONTTYRE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsFrontTYre) == true ? null : itemModel.NoOfBoltsFrontTYre.Trim();
                    sc.Parameters.Add("P_REARTYRE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsRearTYre) == true ? null : itemModel.NoOfBoltsRearTYre.Trim();
                    sc.Parameters.Add("P_EN_TORQUE1_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe1) == true ? null : itemModel.NoOfBoltsEnToruqe1.Trim();
                    sc.Parameters.Add("P_EN_TORQUE2_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe2) == true ? null : itemModel.NoOfBoltsEnToruqe2.Trim();
                    sc.Parameters.Add("P_EN_TORQUE3_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.NoOfBoltsEnToruqe3) == true ? null : itemModel.NoOfBoltsEnToruqe3.Trim();
                    sc.Parameters.Add("P_BK_TORQUE_BOLTVALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BK_BOLT_VALUE) == true ? null : itemModel.BK_BOLT_VALUE.Trim();
                    sc.Parameters.Add("P_REQ_ROPS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RopsChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_ROPS_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Rops) == true ? null : itemModel.Rops.Trim();
                    sc.Parameters.Add("P_REQ_CAREBTN", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.EnableCarButtonChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_SEQ_NOT_REQUIRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.Seq_Not_RequireChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_BRAKE_PEDAL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRAKE_PEDAL) == true ? null : itemModel.BRAKE_PEDAL.Trim();
                    sc.Parameters.Add("P_BRAKE_PEDAL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BrakePedal_Desc) == true ? null : itemModel.BrakePedal_Desc.Trim();
                    sc.Parameters.Add("P_CLUTCH_PEDAL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CLUTCH_PEDAL) == true ? null : itemModel.CLUTCH_PEDAL.Trim();
                    sc.Parameters.Add("P_CLUTCH_PEDAL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClutchPedal_Desc) == true ? null : itemModel.ClutchPedal_Desc.Trim();
                    sc.Parameters.Add("P_SPOOL_VALUE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SPOOL_VALUE) == true ? null : itemModel.SPOOL_VALUE.Trim();
                    sc.Parameters.Add("P_SPOOL_VALUE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SpoolValue_Desc) == true ? null : itemModel.SpoolValue_Desc.Trim();
                    sc.Parameters.Add("P_TANDEM_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TANDEM_PUMP) == true ? null : itemModel.TANDEM_PUMP.Trim();
                    sc.Parameters.Add("P_TANDEM_PUMP_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TandemPump_Desc) == true ? null : itemModel.TandemPump_Desc.Trim();
                    sc.Parameters.Add("P_FENDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FENDER) == true ? null : itemModel.FENDER.Trim();
                    sc.Parameters.Add("P_FENDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Fender_Desc) == true ? null : itemModel.Fender_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_RAILING", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FENDER_RAILING) == true ? null : itemModel.FENDER_RAILING.Trim();
                    sc.Parameters.Add("P_FENDER_RAILING_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRailing_Desc) == true ? null : itemModel.FenderRailing_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HEAD_LAMP) == true ? null : itemModel.HEAD_LAMP.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLamp_Desc) == true ? null : itemModel.HeadLamp_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_WHEEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.STEERING_WHEEL) == true ? null : itemModel.STEERING_WHEEL.Trim();
                    sc.Parameters.Add("P_STEERING_WHEEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringWheel_Desc) == true ? null : itemModel.SteeringWheel_Desc.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_WIRING_HARNESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.REAR_HOOD_WIRING_HARNESS) == true ? null : itemModel.REAR_HOOD_WIRING_HARNESS.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_WIRING_HRNE_DES", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHoolWiringHarness_Desc) == true ? null : itemModel.RearHoolWiringHarness_Desc.Trim();
                    sc.Parameters.Add("P_SEAT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SEAT) == true ? null : itemModel.SEAT.Trim();
                    sc.Parameters.Add("P_SEAT_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Seat_Desc) == true ? null : itemModel.Seat_Desc.Trim();
                    sc.Parameters.Add("P_ELECTRIC_TRACTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.ElectricMotorChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_RH_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHFrontTyre) == true ? null : itemModel.RHFrontTyre.Trim();
                    sc.Parameters.Add("P_RH_FRONTTYRE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHFrontTyre_Desc) == true ? null : itemModel.RHFrontTyre_Desc.Trim();
                    sc.Parameters.Add("P_RH_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHRearTyre) == true ? null : itemModel.RHRearTyre.Trim();
                    sc.Parameters.Add("P_RH_REARTYRE_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RHRearTyre_Desc) == true ? null : itemModel.RHRearTyre_Desc.Trim();
                    sc.Parameters.Add("P_REQ_RHFT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RHFrontTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_RHRT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RHRearTyreChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateTractorMaster";
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                //throw;
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool InsertTractorMasterS(TractorMster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_TRACTORMASTER_TAB2", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_Plant.Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_Family.Trim();
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_ItemCode.Trim();
                    sc.Parameters.Add("P_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_ItemCode_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_SUPPORT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontSupport) == true ? null : itemModel.FrontSupport.Trim();
                    sc.Parameters.Add("P_FRONT_SUPPORT_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontSupport_Desc) == true ? null : itemModel.FrontSupport_Desc.Trim();
                    sc.Parameters.Add("P_CENTER_AXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CenterAxel) == true ? null : itemModel.CenterAxel.Trim();
                    sc.Parameters.Add("P_CENTER_AXEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CenterAxel_Desc) == true ? null : itemModel.CenterAxel_Desc.Trim();
                    sc.Parameters.Add("P_SLIDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider) == true ? null : itemModel.Slider.Trim();
                    sc.Parameters.Add("P_SLIDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_Desc) == true ? null : itemModel.Slider_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_COLUMN", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringColumn) == true ? null : itemModel.SteeringColumn.Trim();
                    sc.Parameters.Add("P_STEERING_COLUMN_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringColumn_Desc) == true ? null : itemModel.SteeringColumn_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_BASE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringBase) == true ? null : itemModel.SteeringBase.Trim();
                    sc.Parameters.Add("P_STEERING_BASE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringBase_Desc) == true ? null : itemModel.SteeringBase_Desc.Trim();
                    sc.Parameters.Add("P_LOWER_LINK", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Lowerlink) == true ? null : itemModel.Lowerlink.Trim();
                    sc.Parameters.Add("P_LOWER_LINK_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Lowerlink_Desc) == true ? null : itemModel.Lowerlink_Desc.Trim();
                    sc.Parameters.Add("P_RB_FRAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBFrame) == true ? null : itemModel.RBFrame.Trim();
                    sc.Parameters.Add("P_RB_FRAME_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBFrame_Desc) == true ? null : itemModel.RBFrame_Desc.Trim();
                    sc.Parameters.Add("P_FUEL_TANK", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FuelTank) == true ? null : itemModel.FuelTank.Trim();
                    sc.Parameters.Add("P_FUEL_TANK_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FuelTank_Desc) == true ? null : itemModel.FuelTank_Desc.Trim();
                    sc.Parameters.Add("P_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Cylinder) == true ? null : itemModel.Cylinder.Trim();
                    sc.Parameters.Add("P_CYLINDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Cylinder_Desc) == true ? null : itemModel.Cylinder_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRH) == true ? null : itemModel.FenderRH.Trim();
                    sc.Parameters.Add("P_FENDER_RH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRH_Desc) == true ? null : itemModel.FenderRH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLH) == true ? null : itemModel.FenderLH.Trim();
                    sc.Parameters.Add("P_FENDER_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLH_Desc) == true ? null : itemModel.FenderLH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_HARNESS_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessRH) == true ? null : itemModel.FenderHarnessRH.Trim();
                    sc.Parameters.Add("P_FENDERHARNESSRH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessRH_Desc) == true ? null : itemModel.FenderHarnessRH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_HARNESS_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessLH) == true ? null : itemModel.FenderHarnessLH.Trim();
                    sc.Parameters.Add("P_FENDERHARNESSLH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessLH_Desc) == true ? null : itemModel.FenderHarnessLH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_LAMP4_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLamp4Types) == true ? null : itemModel.FenderLamp4Types.Trim();
                    sc.Parameters.Add("P_FENDERLAMP4TYPE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLamp4Types_Desc) == true ? null : itemModel.FenderLamp4Types_Desc.Trim();
                    sc.Parameters.Add("P_RB_HARNESS_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBHarnessLH) == true ? null : itemModel.RBHarnessLH.Trim();
                    sc.Parameters.Add("P_RB_HARNESS_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBHarnessLH_Desc) == true ? null : itemModel.RBHarnessLH_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_RIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontRim) == true ? null : itemModel.FrontRim.Trim();
                    sc.Parameters.Add("P_FRONT_RIM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontRim_Desc) == true ? null : itemModel.FrontRim_Desc.Trim();
                    sc.Parameters.Add("P_REAR_RIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearRim) == true ? null : itemModel.RearRim.Trim();
                    sc.Parameters.Add("P_REAR_RIM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearRim_Desc) == true ? null : itemModel.RearRim_Desc.Trim();
                    sc.Parameters.Add("P_TYRE_MAKE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TyreMake) == true ? null : itemModel.TyreMake.Trim();
                    sc.Parameters.Add("P_TYRE_MAKE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TyreMake_Desc) == true ? null : itemModel.TyreMake_Desc.Trim();
                    sc.Parameters.Add("P_REAR_HOOD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHood) == true ? null : itemModel.RearHood.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHood_Desc) == true ? null : itemModel.RearHood_Desc.Trim();
                    sc.Parameters.Add("P_CLUSTER_METER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterMeter) == true ? null : itemModel.ClusterMeter.Trim();
                    sc.Parameters.Add("P_CLUSTER_METER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterMeter_Desc) == true ? null : itemModel.ClusterMeter_Desc.Trim();
                    sc.Parameters.Add("P_IP_HARNESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.IPHarness) == true ? null : itemModel.IPHarness.Trim();
                    sc.Parameters.Add("P_IP_HARNESS_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.IPHarness_Desc) == true ? null : itemModel.IPHarness_Desc.Trim();
                    sc.Parameters.Add("P_RADIATOR_SHELL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorShell) == true ? null : itemModel.RadiatorShell.Trim();
                    sc.Parameters.Add("P_RADIATOR_SHELL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorShell_Desc) == true ? null : itemModel.RadiatorShell_Desc.Trim();
                    sc.Parameters.Add("P_AIR_CLEANER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.AirCleaner) == true ? null : itemModel.AirCleaner.Trim();
                    sc.Parameters.Add("P_AIR_CLEANER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.AirCleaner_Desc) == true ? null : itemModel.AirCleaner_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampLH) == true ? null : itemModel.HeadLampLH.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampLH_Desc) == true ? null : itemModel.HeadLampLH_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampRH) == true ? null : itemModel.HeadLampRH.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_RH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampRH_Desc) == true ? null : itemModel.HeadLampRH_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_GRILL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontGrill) == true ? null : itemModel.FrontGrill.Trim();
                    sc.Parameters.Add("P_FRONT_GRILL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontGrill_Desc) == true ? null : itemModel.FrontGrill_Desc.Trim();
                    sc.Parameters.Add("P_MAIN_HARNESS_BONNET", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.MainHarnessBonnet) == true ? null : itemModel.MainHarnessBonnet.Trim();
                    sc.Parameters.Add("P_MAINHARBONNET_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.MainHarnessBonnet_Desc) == true ? null : itemModel.MainHarnessBonnet_Desc.Trim();
                    sc.Parameters.Add("P_SPINDLE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Spindle) == true ? null : itemModel.Spindle.Trim();
                    sc.Parameters.Add("P_SPINDLE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Spindle_Desc) == true ? null : itemModel.Spindle_Desc.Trim();

                    //--------------------------------------------------------------Add New Fields---------------------------------------------------------- 
                    sc.Parameters.Add("P_SLIDER_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_RH) == true ? null : itemModel.Slider_RH.Trim();
                    sc.Parameters.Add("P_SLIDER_RH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_RH_Desc) == true ? null : itemModel.Slider_RH_Desc.Trim();
                    sc.Parameters.Add("P_BRK_PAD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRK_PAD) == true ? null : itemModel.BRK_PAD.Trim();
                    sc.Parameters.Add("P_BRK_PAD_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRK_PAD_DESC) == true ? null : itemModel.BRK_PAD_DESC.Trim();
                    sc.Parameters.Add("P_FRB_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_RH) == true ? null : itemModel.FRB_RH.Trim();
                    sc.Parameters.Add("P_FRB_RH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_RH_DESC) == true ? null : itemModel.FRB_RH_DESC.Trim();
                    sc.Parameters.Add("P_FRB_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_LH) == true ? null : itemModel.FRB_LH.Trim();
                    sc.Parameters.Add("P_FRB_LH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_LH_DESC) == true ? null : itemModel.FRB_LH_DESC.Trim();
                    sc.Parameters.Add("P_FR_AS_RB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FR_AS_RB) == true ? null : itemModel.FR_AS_RB.Trim();
                    sc.Parameters.Add("P_FR_AS_RB_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FR_AS_RB_DESC) == true ? null : itemModel.FR_AS_RB_DESC.Trim();

                    sc.Parameters.Add("P_REQ_FRONTRIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.FrontRimChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_REARRIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearRimChk == true ? "Y" : "N";

                    //sc.Parameters.Add("P_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Motor) == true ? null : itemModel.Motor.Trim();
                    //sc.Parameters.Add("P_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Motor_Desc) == true ? null : itemModel.Motor_Desc.Trim();
                    sc.Parameters.Add("P_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "InsertTractorMasterTab2";
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool UpdateTractorMasterS(TractorMster itemModel)
        {
            bool result = false;
            try
            {
                using (OracleCommand sc = new OracleCommand("USP_CRUD_TRACTORMASTER_TAB2", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_Plant.Trim();
                    sc.Parameters.Add("P_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_Family.Trim();
                    sc.Parameters.Add("P_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_ItemCode.Trim();
                    sc.Parameters.Add("P_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.T4_ItemCode_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_SUPPORT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontSupport) == true ? null : itemModel.FrontSupport.Trim();
                    sc.Parameters.Add("P_FRONT_SUPPORT_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontSupport_Desc) == true ? null : itemModel.FrontSupport_Desc.Trim();
                    sc.Parameters.Add("P_CENTER_AXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CenterAxel) == true ? null : itemModel.CenterAxel.Trim();
                    sc.Parameters.Add("P_CENTER_AXEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.CenterAxel_Desc) == true ? null : itemModel.CenterAxel_Desc.Trim();
                    sc.Parameters.Add("P_SLIDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider) == true ? null : itemModel.Slider.Trim();
                    sc.Parameters.Add("P_SLIDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_Desc) == true ? null : itemModel.Slider_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_COLUMN", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringColumn) == true ? null : itemModel.SteeringColumn.Trim();
                    sc.Parameters.Add("P_STEERING_COLUMN_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringColumn_Desc) == true ? null : itemModel.SteeringColumn_Desc.Trim();
                    sc.Parameters.Add("P_STEERING_BASE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringBase) == true ? null : itemModel.SteeringBase.Trim();
                    sc.Parameters.Add("P_STEERING_BASE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.SteeringBase_Desc) == true ? null : itemModel.SteeringBase_Desc.Trim();
                    sc.Parameters.Add("P_LOWER_LINK", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Lowerlink) == true ? null : itemModel.Lowerlink.Trim();
                    sc.Parameters.Add("P_LOWER_LINK_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Lowerlink_Desc) == true ? null : itemModel.Lowerlink_Desc.Trim();
                    sc.Parameters.Add("P_RB_FRAME", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBFrame) == true ? null : itemModel.RBFrame.Trim();
                    sc.Parameters.Add("P_RB_FRAME_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBFrame_Desc) == true ? null : itemModel.RBFrame_Desc.Trim();
                    sc.Parameters.Add("P_FUEL_TANK", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FuelTank) == true ? null : itemModel.FuelTank.Trim();
                    sc.Parameters.Add("P_FUEL_TANK_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FuelTank_Desc) == true ? null : itemModel.FuelTank_Desc.Trim();
                    sc.Parameters.Add("P_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Cylinder) == true ? null : itemModel.Cylinder.Trim();
                    sc.Parameters.Add("P_CYLINDER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Cylinder_Desc) == true ? null : itemModel.Cylinder_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRH) == true ? null : itemModel.FenderRH.Trim();
                    sc.Parameters.Add("P_FENDER_RH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderRH_Desc) == true ? null : itemModel.FenderRH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLH) == true ? null : itemModel.FenderLH.Trim();
                    sc.Parameters.Add("P_FENDER_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLH_Desc) == true ? null : itemModel.FenderLH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_HARNESS_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessRH) == true ? null : itemModel.FenderHarnessRH.Trim();
                    sc.Parameters.Add("P_FENDERHARNESSRH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessRH_Desc) == true ? null : itemModel.FenderHarnessRH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_HARNESS_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessLH) == true ? null : itemModel.FenderHarnessLH.Trim();
                    sc.Parameters.Add("P_FENDERHARNESSLH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderHarnessLH_Desc) == true ? null : itemModel.FenderHarnessLH_Desc.Trim();
                    sc.Parameters.Add("P_FENDER_LAMP4_TYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLamp4Types) == true ? null : itemModel.FenderLamp4Types.Trim();
                    sc.Parameters.Add("P_FENDERLAMP4TYPE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FenderLamp4Types_Desc) == true ? null : itemModel.FenderLamp4Types_Desc.Trim();
                    sc.Parameters.Add("P_RB_HARNESS_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBHarnessLH) == true ? null : itemModel.RBHarnessLH.Trim();
                    sc.Parameters.Add("P_RB_HARNESS_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RBHarnessLH_Desc) == true ? null : itemModel.RBHarnessLH_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_RIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontRim) == true ? null : itemModel.FrontRim.Trim();
                    sc.Parameters.Add("P_FRONT_RIM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontRim_Desc) == true ? null : itemModel.FrontRim_Desc.Trim();
                    sc.Parameters.Add("P_REAR_RIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearRim) == true ? null : itemModel.RearRim.Trim();
                    sc.Parameters.Add("P_REAR_RIM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearRim_Desc) == true ? null : itemModel.RearRim_Desc.Trim();
                    sc.Parameters.Add("P_TYRE_MAKE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TyreMake) == true ? null : itemModel.TyreMake.Trim();
                    sc.Parameters.Add("P_TYRE_MAKE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.TyreMake_Desc) == true ? null : itemModel.TyreMake_Desc.Trim();
                    sc.Parameters.Add("P_REAR_HOOD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHood) == true ? null : itemModel.RearHood.Trim();
                    sc.Parameters.Add("P_REAR_HOOD_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RearHood_Desc) == true ? null : itemModel.RearHood_Desc.Trim();
                    sc.Parameters.Add("P_CLUSTER_METER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterMeter) == true ? null : itemModel.ClusterMeter.Trim();
                    sc.Parameters.Add("P_CLUSTER_METER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.ClusterMeter_Desc) == true ? null : itemModel.ClusterMeter_Desc.Trim();
                    sc.Parameters.Add("P_IP_HARNESS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.IPHarness) == true ? null : itemModel.IPHarness.Trim();
                    sc.Parameters.Add("P_IP_HARNESS_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.IPHarness_Desc) == true ? null : itemModel.IPHarness_Desc.Trim();
                    sc.Parameters.Add("P_RADIATOR_SHELL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorShell) == true ? null : itemModel.RadiatorShell.Trim();
                    sc.Parameters.Add("P_RADIATOR_SHELL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.RadiatorShell_Desc) == true ? null : itemModel.RadiatorShell_Desc.Trim();
                    sc.Parameters.Add("P_AIR_CLEANER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.AirCleaner) == true ? null : itemModel.AirCleaner.Trim();
                    sc.Parameters.Add("P_AIR_CLEANER_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.AirCleaner_Desc) == true ? null : itemModel.AirCleaner_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampLH) == true ? null : itemModel.HeadLampLH.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_LH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampLH_Desc) == true ? null : itemModel.HeadLampLH_Desc.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampRH) == true ? null : itemModel.HeadLampRH.Trim();
                    sc.Parameters.Add("P_HEAD_LAMP_RH_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.HeadLampRH_Desc) == true ? null : itemModel.HeadLampRH_Desc.Trim();
                    sc.Parameters.Add("P_FRONT_GRILL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontGrill) == true ? null : itemModel.FrontGrill.Trim();
                    sc.Parameters.Add("P_FRONT_GRILL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FrontGrill_Desc) == true ? null : itemModel.FrontGrill_Desc.Trim();
                    sc.Parameters.Add("P_MAIN_HARNESS_BONNET", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.MainHarnessBonnet) == true ? null : itemModel.MainHarnessBonnet.Trim();
                    sc.Parameters.Add("P_MAINHARBONNET_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.MainHarnessBonnet_Desc) == true ? null : itemModel.MainHarnessBonnet_Desc.Trim();
                    sc.Parameters.Add("P_SPINDLE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Spindle) == true ? null : itemModel.Spindle.Trim();
                    sc.Parameters.Add("P_SPINDLE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Spindle_Desc) == true ? null : itemModel.Spindle_Desc.Trim();

                    //-------------------------------------------Add New Fields---------------------------------------------
                    sc.Parameters.Add("P_SLIDER_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_RH) == true ? null : itemModel.Slider_RH.Trim();
                    sc.Parameters.Add("P_SLIDER_RH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Slider_RH_Desc) == true ? null : itemModel.Slider_RH_Desc.Trim();
                    sc.Parameters.Add("P_BRK_PAD", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRK_PAD) == true ? null : itemModel.BRK_PAD.Trim();
                    sc.Parameters.Add("P_BRK_PAD_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.BRK_PAD_DESC) == true ? null : itemModel.BRK_PAD_DESC.Trim();
                    sc.Parameters.Add("P_FRB_RH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_RH) == true ? null : itemModel.FRB_RH.Trim();
                    sc.Parameters.Add("P_FRB_RH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_RH_DESC) == true ? null : itemModel.FRB_RH_DESC.Trim();
                    sc.Parameters.Add("P_FRB_LH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_LH) == true ? null : itemModel.FRB_LH.Trim();
                    sc.Parameters.Add("P_FRB_LH_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FRB_LH_DESC) == true ? null : itemModel.FRB_LH_DESC.Trim();
                    sc.Parameters.Add("P_FR_AS_RB", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FR_AS_RB) == true ? null : itemModel.FR_AS_RB.Trim();
                    sc.Parameters.Add("P_FR_AS_RB_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.FR_AS_RB_DESC) == true ? null : itemModel.FR_AS_RB_DESC.Trim();

                    sc.Parameters.Add("P_REQ_FRONTRIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.FrontRimChk == true ? "Y" : "N";
                    sc.Parameters.Add("P_REQ_REARRIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = itemModel.RearRimChk == true ? "Y" : "N";



                    //sc.Parameters.Add("P_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Motor) == true ? null : itemModel.Motor.Trim();
                    //sc.Parameters.Add("P_MOTOR_DESC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(itemModel.Motor_Desc) == true ? null : itemModel.Motor_Desc.Trim();
                    sc.Parameters.Add("P_CREATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    sc.Parameters.Add("P_UPDATED_BY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "UpdateTractorMasterTab2";
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);
                    sc.ExecuteNonQuery();
                    //ConClose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }
        public List<DDLTextValue> FillPUName(UserMaster UM, int? SelectOption = 0)
        {
            DataTable TmpDs = null;
            try
            {

                List<DDLTextValue> PUNAME = new List<DDLTextValue>();
                TmpDs = returnDataTable("select distinct character6 PUNAME from apps.qa_results_v  where character6 is not null and plan_id = 224155 and character1 = '" + Convert.ToString(UM.U_CODE) + "'");

                if (SelectOption == 1)
                {
                    PUNAME.Add(new DDLTextValue
                    {
                        Text = "---SELECT---",
                        Value = "0",
                    });
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        PUNAME.Add(new DDLTextValue
                        {
                            Text = dr["PUNAME"].ToString(),
                            Value = dr["PUNAME"].ToString(),
                        });
                    }
                }
                return PUNAME;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }
            finally
            {
                ConClose();
            }
        }

        public List<DDLTextValue> Fill_FamilyMRNVerfication(string ucode)
        {
            DataTable TmpDs = new DataTable();
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {

                TmpDs = returnDataTable("Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");


                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        if (Convert.ToString(dr["FAMILY_CODE"]).StartsWith("TRACTOR"))
                        {
                            Family.Add(new DDLTextValue
                            {
                                Text = dr["Name"].ToString(),
                                Value = dr["FAMILY_CODE"].ToString(),
                            });
                        }
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public List<DDLTextValue> FillMappingFamily(string ucode)
        {
            DataTable TmpDs = new DataTable();
            string query = string.Empty;
            List<DDLTextValue> Family = new List<DDLTextValue>();
            try
            {
                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    TmpDs = returnDataTable(@"Select FAMILY_CODE || ' # ' || FAMILY_Name as Name,FAMILY_CODE from XXES_FAMILY_MASTER 
                    where FAMILY_CODE in ( select FAMILY_CODE from XXES_PLANT_FAMILY_MAP where plant_code='" + ucode.Trim() + "')");
                }
                else
                {
                    query = string.Format(@"SELECT FM.FAMILY_CODE || ' # ' || FM.FAMILY_Name as Name,FM.FAMILY_CODE 
                            FROM XXES_FAMILY_MASTER FM INNER JOIN XXES_PLANT_FAMILY_MAP PFM ON fm.family_code = pfm.family_code 
                            INNER JOIN XXES_USERS_MASTER UM ON UM.familycode = pfm.family_code AND um.u_code = pfm.plant_code 
                            WHERE pfm.plant_code = '{0}' AND um.usrname = '{1}'", ucode.ToUpper().Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).Trim().ToUpper());

                    TmpDs = returnDataTable(query);
                }
                if (TmpDs.Rows.Count > 0)
                {
                    foreach (DataRow dr in TmpDs.AsEnumerable())
                    {
                        Family.Add(new DDLTextValue
                        {
                            Text = dr["Name"].ToString(),
                            Value = dr["FAMILY_CODE"].ToString(),
                        });
                    }
                }
                return Family;
            }
            catch (Exception ex)
            {
                LogWrite(ex);
                return Family;
            }
            finally
            {
                //ConClose();
            }
        }

        public Tuple<string, bool> MobileMsgAlert(SftSetting data, string PrmInfoError, string PrmInfoSuccess)
        {
            bool result = false;
            string msg = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;

            data.ErrorIntvl = data.ErrorIntvl == "SELECT" ? null : data.ErrorIntvl.Trim().ToUpper();
            data.SuccessIntvl = data.SuccessIntvl == "SELECT" ? null : data.SuccessIntvl.Trim().ToUpper();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;
                // Start a local transaction.
                transaction = connection.BeginTransaction();

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' 
                                            AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}'", PrmInfoError, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(data.ErrorIntvl))
                    {

                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", PrmInfoError, data.ErrorIntvl, data.Status, data.Description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' AND PLANT_CODE = '{1}' 
                                            AND FAMILY_CODE = '{2}'", PrmInfoSuccess, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(data.SuccessIntvl))
                    {


                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", PrmInfoSuccess, data.SuccessIntvl, data.Status, data.Description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }



                    // Attempt to commit the transaction.
                    transaction.Commit();
                    msg = "Record inserted successfully";
                    result = true;
                }
                catch (Exception ex)
                {

                    LogWrite(ex);
                    msg = ex.Message;
                    result = false;
                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        msg = ex2.Message;
                        result = false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }

            }


            return new Tuple<string, bool>(msg, result);
        }

        public bool QtyVerificationLabel(SftSetting data, string paramInfo, string description)
        {
            bool result = false;
            string msg = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;

            //data.QtyVeriLbl = data.QtyVeriLbl == "SELECT" ? null : data.QtyVeriLbl.Trim().ToUpper();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' 
                                            AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}'", paramInfo, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();

                    if (Convert.ToBoolean(data.A4Sheet))
                    {
                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, "A4", data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }
                    if (Convert.ToBoolean(data.Barcode))
                    {
                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, "BARCODE", data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }
                    if (Convert.ToBoolean(data.Quality))
                    {
                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, "QUALITY", data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }


                    transaction.Commit();
                    msg = "Record inserted successfully";
                    result = true;
                }
                catch (Exception ex)
                {

                    LogWrite(ex);
                    msg = ex.Message;
                    result = false;
                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        msg = ex2.Message;
                        result = false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }

            }


            return result;
        }
        public List<Barcode> GridKANBAN(Barcode obj)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            List<Barcode> KanbanList = new List<Barcode>();
            try
            {
                string search = obj.P_Search;
                if (!string.IsNullOrEmpty(search))
                {
                    search = Convert.ToString(obj.P_Search).ToUpper().Trim();
                }
                obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);

                query = string.Format(@"SELECT TOTALCOUNT,AUTOID,PLANT_CODE,FAMILY_CODE,KANBAN_NO,ITEM_CODE,ITEM_DESCRIPTION,SUMKTLOC FROM (SELECT ROW_NUMBER() OVER (ORDER BY CREATED_DATE DESC) as ROWNMBER,
                                        COUNT(*) over() as TOTALCOUNT, AUTOID,PLANT_CODE,FAMILY_CODE,KANBAN_NO,ITEM_CODE,ITEM_DESCRIPTION,SUMKTLOC
                                         from XXES_KANBAN_MASTER where ( '{4}' IS NULL OR ITEM_CODE like '%{4}%' OR SUMKTLOC LIKE '%{4}%') AND 
                                        PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}') KB WHERE ROWNMBER > '{2}' and ROWNMBER <= '{3}'",
                                        obj.Plant.ToUpper().Trim(), obj.Family.ToUpper().Trim(), Convert.ToInt32(obj.start), obj.length, search);
                dt = returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Barcode BC = new Barcode
                        {
                            TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
                            AUTOID = Convert.ToInt32(dr["AUTOID"]),
                            Plant = dr["PLANT_CODE"].ToString(),
                            Family = dr["FAMILY_CODE"].ToString(),
                            KANBAN = dr["KANBAN_NO"].ToString(),
                            LOCATION = dr["SUMKTLOC"].ToString(),
                            ITEMCODE = dr["ITEM_CODE"].ToString(),
                            ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString()
                        };
                        KanbanList.Add(BC);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return KanbanList;
        }

        public List<Barcode> GridBULKSTORAGE(Barcode obj)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            List<Barcode> BulkStorage = new List<Barcode>();
            try
            {
                string search = obj.P_Search;
                if (!string.IsNullOrEmpty(search))
                {
                    search = Convert.ToString(obj.P_Search).ToUpper().Trim();
                }
                obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);

                query = string.Format(@"SELECT TOTALCOUNT,AUTOID,PLANT_CODE,FAMILY_CODE,LOCATION_CODE,ITEM_CODE,ITEM_DESCRIPTION,SAFTY_STOCK_QUANTITY,NO_OF_LOC_ALLOCATED,PACKAGING_TYPE,VERTICAL_STACKING_LEVEL
                                       ,BULK_STORAGE_SNP,USAGE_PER_TRACTOR,REVISION,MAX_INVENTORY FROM 
                                        (select ROW_NUMBER() OVER (ORDER BY CREATED_DATE DESC) as ROWNMBER,COUNT(*) over() as TOTALCOUNT,AUTOID,
                                        PLANT_CODE, FAMILY_CODE, LOCATION_CODE, ITEM_CODE,SAFTY_STOCK_QUANTITY,NO_OF_LOC_ALLOCATED,PACKAGING_TYPE,VERTICAL_STACKING_LEVEL,BULK_STORAGE_SNP,USAGE_PER_TRACTOR,REVISION,MAX_INVENTORY,
                                         (select SUBSTR(M.ITEM_DESCRIPTION,1,50) AS ITEM_DESCRIPTION from XXES_RAWMATERIAL_MASTER m 
                                         where m.item_code=s.item_code and m.plant_code=s.plant_code and m.family_code=s.family_code AND ROWNUM <= 1) as ITEM_DESCRIPTION
                                         FROM XXES_Bulk_Storage s
                                         where ('{4}' IS NULL OR ITEM_CODE like '%{4}%' OR LOCATION_CODE LIKE '%{4}%') AND 
                                        PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}') KB
                                        WHERE ROWNMBER > '{2}' and ROWNMBER <= '{3}'
                                        
                       ", obj.Plant.ToUpper().Trim(), obj.Family.ToUpper().Trim(), Convert.ToInt32(obj.start), obj.length, search);
                dt = returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Barcode BC = new Barcode
                        {
                            TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
                            AUTOID = Convert.ToInt32(dr["AUTOID"]),
                            Plant = dr["PLANT_CODE"].ToString(),
                            Family = dr["FAMILY_CODE"].ToString(),
                            LOCATION = dr["LOCATION_CODE"].ToString(),
                            ITEMCODE = dr["ITEM_CODE"].ToString(),
                            ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                            USAGEPERTRACTOR = dr["USAGE_PER_TRACTOR"].ToString(),
                            SFTSTKQUANTITY = dr["SAFTY_STOCK_QUANTITY"].ToString(),
                            NOOFLOCALLOCATED = dr["NO_OF_LOC_ALLOCATED"].ToString(),
                            PACKINGTYPE = dr["PACKAGING_TYPE"].ToString(),
                            VERTICALSTKLEVEL = dr["VERTICAL_STACKING_LEVEL"].ToString(),
                            BULKSTORESNP = dr["BULK_STORAGE_SNP"].ToString(),
                            MAX_INVENTORY = dr["MAX_INVENTORY"].ToString(),
                            REVISION = dr["REVISION"].ToString()
                        };
                        BulkStorage.Add(BC);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return BulkStorage;
        }
        public List<HookUpAndDown> GridHookData(HookUpAndDown obj)
        {
            DataTable dt = new DataTable();
            List<HookUpAndDown> HookDetails = new List<HookUpAndDown>();
            try
            {
                string search = obj.P_Search;
                if (!string.IsNullOrEmpty(search))
                {
                    search = Convert.ToString(obj.P_Search).ToUpper().Trim();
                }
                obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);

                query = string.Format(@"SELECT TOTALCOUNT,HOOK,HOOK_NO,PLANTCODE,ITEM_CODE,DESCRIPTION,
                            ENTRYDATE,JOBID, AGEING_DAYS FROM (select ROW_NUMBER() OVER (ORDER BY FCODE_ID) as ROWNMBER,
                            COUNT(*) over() as TOTALCOUNT,'NA' HOOK,'NA' HOOK_NO,m.plant_code as PLANTCODE,m.ITEM_CODE,substr(m.ITEM_DESCRIPTION,1,40) DESCRIPTION,
                            to_char( ENTRYDATE, 'dd-Mon-yyyy HH24:MI:SS' ) ENTRYDATE,c.JOBID, nvl((trunc(sysdate) - trunc(C.entrydate)),0) AGEING_DAYS   from xxes_job_status c, xxes_item_master m 
                            where c.ITEM_CODE=m.ITEM_CODE  and m.plant_code='{0}' and 
                            to_char(ENTRYDATE,'dd-Mon-yyyy') >=to_date('{1}','dd-Mon-yyyy') 
                            and  to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('{2}','dd-Mon-yyyy') 
                            and jobid not in (select jobid from xxes_controllers_data where stage='BP' and fcode_id=c.FCODE_ID) AND ('{3}' IS NULL OR m.ITEM_CODE like '%{3}%' OR c.JOBID LIKE '%{3}%')) KB
                            WHERE ROWNMBER > '{4}' and ROWNMBER <= '{5}'", obj.PLANTCODE.ToUpper().Trim(), obj.ToDate, obj.FromDate, search, Convert.ToInt32(obj.start), obj.length);

                dt = returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        HookUpAndDown HUD = new HookUpAndDown
                        {
                            TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
                            HOOK = dr["HOOK"].ToString(),
                            HOOK_NO = dr["HOOK_NO"].ToString(),
                            PLANTCODE = dr["PLANTCODE"].ToString(),
                            ITEM_CODE = dr["ITEM_CODE"].ToString(),
                            DESCRIPTION = dr["DESCRIPTION"].ToString(),
                            ENTRYDATE = dr["ENTRYDATE"].ToString(),
                            JOBID = dr["JOBID"].ToString(),
                            AGEING_DAYS = dr["AGEING_DAYS"].ToString()
                        };
                        HookDetails.Add(HUD);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return HookDetails;
        }
        public List<Barcode> GridSUPERMARKET(Barcode obj)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            List<Barcode> SuperMkt = new List<Barcode>();
            try
            {
                string search = obj.P_Search;
                if (!string.IsNullOrEmpty(search))
                {
                    search = Convert.ToString(obj.P_Search).ToUpper().Trim();
                }
                obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);

                //query = string.Format(@"SELECT TOTALCOUNT,AUTOID,PLANT_CODE,FAMILY_CODE,LOCATION_NAME,ITEM_CODE,ITEM_DESCRIPTION,USAGE_PER_TRACTOR FROM 
                //                        (select ROW_NUMBER() OVER (ORDER BY sml.CREATED_DATE DESC) as ROWNMBER,COUNT(*) over() as TOTALCOUNT,sml.AUTOID,
                //                        sml.PLANT_CODE, sml.FAMILY_CODE, sml.LOCATION_NAME,km.item_code,km.item_description,KM.USAGE_PER_TRACTOR
                //                         FROM XXES_SUPERMKT_LOCATIONS sml
                //                         INNER JOIN xxes_kanban_master km on sml.location_name = km.sumktloc
                //                        and sml.plant_code = km.plant_code and sml.family_code = km.family_code
                //                         where ('{4}' IS NULL OR sml.LOCATION_NAME like '%{4}%' OR km.ITEM_CODE LIKE '%{4}%') AND 
                //                        sml.PLANT_CODE = '{0}' AND sml.FAMILY_CODE = '{1}') KB
                //                         WHERE ROWNMBER > '{2}' and ROWNMBER <= '{3}'
                                        
                //       ", obj.Plant.ToUpper().Trim(), obj.Family.ToUpper().Trim(), Convert.ToInt32(obj.start), obj.length, search);
                
                query = string.Format(@"SELECT TOTALCOUNT,AUTOID,PLANT_CODE,FAMILY_CODE,LOCATION_NAME,ITEM_CODE,ITEM_DESCRIPTION,USAGE_PER_TRACTOR FROM 
                                        (select ROW_NUMBER() OVER (ORDER BY sml.CREATED_DATE DESC) as ROWNMBER,COUNT(*) over() as TOTALCOUNT,sml.AUTOID,
                                        sml.PLANT_CODE, sml.FAMILY_CODE, sml.LOCATION_NAME,km.item_code,km.item_description,KM.USAGE_PER_TRACTOR
                                         FROM XXES_SUPERMKT_LOCATIONS sml
                                         INNER JOIN (SELECT   sumktloc,item_code,item_description,USAGE_PER_TRACTOR,plant_code,family_code  FROM xxes_kanban_master
                                        group by sumktloc,item_code,item_description,USAGE_PER_TRACTOR,plant_code,family_code) km 
                                        on sml.location_name = km.sumktloc and sml.plant_code = km.plant_code and sml.family_code = km.family_code
                                         where ('{4}' IS NULL OR sml.LOCATION_NAME like '%{4}%' OR km.ITEM_CODE LIKE '%{4}%') AND 
                                        sml.PLANT_CODE = '{0}' AND sml.FAMILY_CODE = '{1}') KB
                                         WHERE ROWNMBER > '{2}' and ROWNMBER <= '{3}' ORDER BY KB.LOCATION_NAME
                                        
                       ", obj.Plant.ToUpper().Trim(), obj.Family.ToUpper().Trim(), Convert.ToInt32(obj.start), obj.length, search);

                dt = returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Barcode BC = new Barcode
                        {
                            TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
                            AUTOID = Convert.ToInt32(dr["AUTOID"]),
                            Plant = dr["PLANT_CODE"].ToString(),
                            Family = dr["FAMILY_CODE"].ToString(),
                            LOCATION = dr["LOCATION_NAME"].ToString(),
                            ITEMCODE = dr["ITEM_CODE"].ToString(),
                            ITEM_DESCRIPTION = dr["ITEM_DESCRIPTION"].ToString(),
                            USAGEPERTRACTOR = dr["USAGE_PER_TRACTOR"].ToString()
                        };
                        SuperMkt.Add(BC);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return SuperMkt;
        }
        public bool HookUpDown(string job, string plant, string family, string Fcode, string hook, string fcode_id, bool isHookUp, bool isHookDown, string hookupdate)
        {
            try
            {

                DateTime hookdate = new DateTime();
                string existingHook = string.Empty;
                if (string.IsNullOrEmpty(hookupdate))
                {
                    hookdate = GetServerDateTime();
                }
                else if (!DateTime.TryParse(hookupdate, out hookdate))
                {

                }
                string query = string.Empty;
                if (isHookUp)
                {

                    if (!CheckExits("select count(*) from xxes_controllers_data where jobid='" + job + "' and stage='BP' and plant_code='" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(family).Trim().ToUpper() + "'"))
                    {
                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID)
                            values(TO_DATE('" + hookdate.ToString("yyyy/MM/dd HH:mm:ss") + "','yyyy/mm/dd HH24:MI:SS'),'" + plant.Trim().ToUpper() + "','" + family.Trim().ToUpper() + "','" + Fcode.Trim().ToUpper() + "','" + job.Trim() + "','" + hook + "','BP','" + fcode_id.Trim() + "')";
                        if (EXEC_QUERY(query))
                        {
                        }
                    }
                }
                existingHook = get_Col_Value("select hook_no from xxes_controllers_data where jobid='" + job + "' and stage='BP' and plant_code='" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(family).Trim().ToUpper() + "'");
                if (!string.IsNullOrEmpty(existingHook))
                {
                    hook = existingHook;
                }

                if (isHookDown)
                {
                    if (!CheckExits("select count(*) from xxes_controllers_data where jobid='" + job.Trim() + "' and stage='AP' and plant_code='" + plant.Trim() + "' and FAMILY_CODE='" + family.Trim().ToUpper() + "'"))
                    {

                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID,FLAG)
                            values(sysdate,'" + plant + "','" + family + "','" + Fcode + "','" + job + "','" + hook.Trim() + "','AP','" + fcode_id.Trim() + "','Y')";
                        if (EXEC_QUERY(query))
                        {

                            // MessageBox.Show("Hooked UP sucessfully !!", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    query = "update xxes_controllers_data set REMARKS1='MANNUAL',FLAG='Y' where jobid='" + job + "' and hook_no='" + hook.Trim() + "' and stage='BP'  and plant_code='" + plant.Trim() + "' and FAMILY_CODE='" + family.Trim().ToUpper() + "'";
                    if (EXEC_QUERY(query))
                    {

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            { }

        }

        public bool InsertKITMaster(KitMaster KM)
        {
            bool result = false;
            try
            {

                using (OracleCommand sc = new OracleCommand("USP_CRUD_KITMASTER", Connection()))
                {
                    ConOpen();
                    sc.CommandType = CommandType.StoredProcedure;
                    sc.Parameters.Add("P_PLANTCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.PLANTCODE.ToUpper().Trim();
                    sc.Parameters.Add("P_FAMILYCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.FAMILYCODE.ToUpper().Trim();
                    sc.Parameters.Add("P_KITNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.KITNO.ToUpper().Trim();
                    sc.Parameters.Add("P_ITEMCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.ITEMCODE.ToUpper().Trim();
                    sc.Parameters.Add("P_SMLOC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.SMLocation.ToUpper().Trim();
                    sc.Parameters.Add("P_QUANTITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.QUANTITY.ToUpper().Trim();
                    if (string.IsNullOrEmpty(KM.AUTOID))
                        sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                    else
                        sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.AUTOID.Trim();

                    sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
                    if (string.IsNullOrEmpty(KM.AUTOID))
                        sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Insert";
                    else
                        sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Update";
                    sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    sc.ExecuteNonQuery();

                    result = true;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public bool DeleteKITMaster(KitMaster KM)
        {
            bool result = false;
            try
            {
                String KITNO = String.Empty;
                query = String.Format(@"SELECT KITNO FROM XXES_KITMASTER WHERE AUTOID='{0}'", KM.AUTOID);
                KITNO = get_Col_Value(query);

                query = String.Format(@"SELECT COUNT(*) FROM XXES_SCANKITTING WHERE KITNO='{0}'", Convert.ToString(KITNO));
                if (CheckExits(query))
                {
                    result = false;
                }
                else
                {
                    using (OracleCommand sc = new OracleCommand("USP_CRUD_KITMASTER", Connection()))
                    {
                        ConOpen();
                        sc.CommandType = CommandType.StoredProcedure;
                        sc.Parameters.Add("P_PLANTCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_FAMILYCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_KITNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_ITEMCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_SMLOC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_QUANTITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_AUTOID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = KM.AUTOID.Trim();
                        sc.Parameters.Add("P_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = null;
                        sc.Parameters.Add("P_CALLTYPE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = "Delete";
                        sc.Parameters.Add("P_RES", OracleDbType.RefCursor, ParameterDirection.Output);

                        sc.ExecuteNonQuery();

                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return result;
        }

        public string GetplanId(string plant, string family, DateTime plan, string shift)
        {
            query = string.Format(@"SELECT PLAN_ID FROM XXES_DAILY_PLAN_MASTER WHERE 
            to_char(PLAN_DATE,'dd-Mon-yyyy') = '{0}' AND PLANT_CODE ='{1}' 
            AND FAMILY_CODE = '{2}'  AND SHIFTCODE = '{3}'", plan.Date.ToString("dd-MMM-yyyy"), Convert.ToString(plant).Trim().ToUpper(), family, shift);
            return get_Col_Value(query);
        }

        public List<ChartModel> Tractor_Available_Stock(string Plant, string Family)
        {
            List<ChartModel> CM = new List<ChartModel>();
            try
            {
                if (Plant.Contains("T04"))
                {
                    query = string.Format(@"SELECT 'REARAXLE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'RAB' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT REARAXEL_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND REARAXEL_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'TRANSMISSION' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT TRANSMISSION_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND TRANSMISSION_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'ENGINE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'ENF' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT ENGINE_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND ENGINE_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'HYDRALUIC' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT HYDRALUIC_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND HYDRALUIC_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'BACKEND' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'BAB' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT BACKEND_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND BACKEND_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'FRONTTYRE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'FT' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN
                                (SELECT FRONTTYRE_SRLNO1 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND FRONTTYRE_SRLNO1 IS NOT NULL  
                                UNION SELECT FRONTTYRE_SRLNO2
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND FRONTTYRE_SRLNO2 IS NOT NULL )
                                UNION
                                SELECT 'REARTYRE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'RT' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT REARTYRE_SRLNO1 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND REARTYRE_SRLNO1 IS NOT NULL  
                                UNION SELECT REARTYRE_SRLNO2
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND REARTYRE_SRLNO2 IS NOT NULL)", Plant.ToUpper().Trim(), Family.ToUpper().Trim());
                }
                else if (Plant.Contains("T05"))
                {
                    query = string.Format(@"SELECT 'TRANSMISSION' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'TRB' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT TRANSMISSION_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND TRANSMISSION_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'ENGINE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'ENF' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT ENGINE_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND ENGINE_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'HYDRALUIC' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'HYD' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT HYDRALUIC_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND HYDRALUIC_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'BACKEND' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'BAB' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT BACKEND_SRLNO 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND BACKEND_SRLNO IS NOT NULL ) 
                                UNION
                                SELECT 'FRONTTYRE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'FT' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN
                                (SELECT FRONTTYRE_SRLNO1 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND FRONTTYRE_SRLNO1 IS NOT NULL  
                                UNION SELECT FRONTTYRE_SRLNO2
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND FRONTTYRE_SRLNO2 IS NOT NULL )
                                UNION
                                SELECT 'REARTYRE' Head, count(*) Total from XXES_PRINT_SERIALS WHERE  OFFLINE_KEYCODE = 'RT' AND PLANT_CODE = '{0}' AND 
                                SRNO NOT IN(SELECT REARTYRE_SRLNO1 
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND REARTYRE_SRLNO1 IS NOT NULL  
                                UNION SELECT REARTYRE_SRLNO2
                                FROM XXES_JOB_STATUS WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' AND REARTYRE_SRLNO2 IS NOT NULL)", Plant.ToUpper().Trim(), Family.ToUpper().Trim());

                }
                using (DataTable dt = returnDataTable(query))
                {
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            CM.Add(new ChartModel
                            {
                                Item = dr["HEAD"].ToString(),
                                Quantity = Convert.ToInt32(dr["TOTAL"]),
                            });
                        }
                    }
                    return CM;
                }

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            return CM;
        }

        public bool BarcodePrintingLabel(SftSetting data, string paramInfo, string description)
        {
            bool result = false;
            string msg = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' 
                                            AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}'", paramInfo, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(data.PrintingCategory))
                    {

                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, data.PrintingCategory, data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    msg = "Record inserted successfully";
                    result = true;
                }
                catch (Exception ex)
                {

                    LogWrite(ex);
                    msg = ex.Message;
                    result = false;
                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        msg = ex2.Message;
                        result = false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }

            }


            return result;
        }

        public bool VerificationPrinting(SftSetting data, string paramInfo, string description)
        {
            bool result = false;
            string msg = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' 
                                            AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}'", paramInfo, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(data.PrintVerification))
                    {

                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, data.PrintVerification, data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    msg = "Record inserted successfully";
                    result = true;
                }
                catch (Exception ex)
                {

                    LogWrite(ex);
                    msg = ex.Message;
                    result = false;
                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        msg = ex2.Message;
                        result = false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }

            }


            return result;
        }
        public bool QCFromDays(SftSetting data, string paramInfo, string description)
        {
            bool result = false;
            string msg = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                if (connection.State == ConnectionState.Closed)
                { connection.Open(); }

                OracleCommand command = connection.CreateCommand();
                OracleTransaction transaction;

                transaction = connection.BeginTransaction();


                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = string.Format(@"DELETE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO = '{0}' 
                                            AND PLANT_CODE = '{1}' AND FAMILY_CODE = '{2}'", paramInfo, data.Plant.Trim(), data.Family.Trim());
                    command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(data.QCFromDays))
                    {

                        command.CommandText = string.Format(@"INSERT INTO XXES_SFT_SETTINGS(PARAMETERINFO,PARAMVALUE,STATUS,DESCRIPTION,PLANT_CODE,FAMILY_CODE,CREATED_BY,CREATED_DATE)
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',SYSDATE)", paramInfo, data.QCFromDays, data.Status, description, data.Plant.Trim(), data.Family.Trim(), Convert.ToString(HttpContext.Current.Session["Login_User"]).ToUpper().Trim());
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    msg = "Record inserted successfully";
                    result = true;
                }
                catch (Exception ex)
                {

                    LogWrite(ex);
                    msg = ex.Message;
                    result = false;
                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        msg = ex2.Message;
                        result = false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    { connection.Close(); }
                    connection.Dispose();
                }

            }


            return result;
        }

        public DataTable GridDailyaprtScanningEfficiency(ReportModel data)
        {
            DataTable dt = new DataTable();
            string query = string.Empty;
            try
            {
                query = string.Format(@"SELECT A.Partname,
                        TOTAL_PROD,
                        Quantity,
                        ROUND((Quantity / TOTAL_PROD) * 100, 2) EFFICIENCY
                        FROM (SELECT (SELECT COUNT(*)
                        FROM XXES_JOB_STATUS J
                        WHERE  
                        to_char(FINAL_LABEL_DATE,'dd-Mon-yyyy')>= to_date('{2}','dd-Mon-yyyy')
                        and  to_char(FINAL_LABEL_DATE,'dd-Mon-yyyy')<= to_date('{3}','dd-Mon-yyyy')
                        AND PLANT_CODE = '{0}'
                        AND  FAMILY_CODE = '{1}') TOTAL_PROD,
                        Head AS Partname,
                        COUNT(*) QUANTITY

                        FROM XXES_JOB_STATUS
                        UNPIVOT (
                        (QUANTITY)  -- unpivot_clause
                        FOR HEAD --  unpivot_for_clause
                        IN ( -- unpivot_in_clause
                        TRANSMISSION_SRLNO AS 'TRANSMISSION',
                        REARAXEL_SRLNO AS 'REARAXEL',
                        ENGINE_SRLNO AS 'ENGINE',
                        HYDRALUIC_SRLNO AS 'HYDRAULIC',
                        FIPSRNO AS 'ENGINE FIP',
                        STARTER_MOTOR_SRLNO AS 'STARTER MOTOR',
                        ALTERNATOR_SRLNO AS 'ALTERNATOR',
                        RADIATOR_SRLNO AS 'RADIATOR',
                        FRONTTYRE_SRLNO1 AS 'FRONT TYRE RIGHT',
                        FRONTTYRE_SRLNO2 AS 'FRONT TYRE LEFT',
                        REARTYRE_SRLNO1 AS 'REAR TYRE RIGHT',
                        REARTYRE_SRLNO2 AS 'REAR TYRE LEFT',
                        SIM_SERIAL_NO AS 'CARE BUTTON',
                        BATTERY_SRLNO AS 'BATTERY'
                        )
                        )
                        WHERE to_char(FINAL_LABEL_DATE,'dd-Mon-yyyy')>= to_date('{2}','dd-Mon-yyyy')
                        and  to_char(FINAL_LABEL_DATE,'dd-Mon-yyyy')<= to_date('{3}','dd-Mon-yyyy')
                        AND PLANT_CODE = '{0}'
                        AND  FAMILY_CODE = '{1}'
                        GROUP BY head
                        ORDER BY CASE head WHEN 'TRANSMISSION' 
                        THEN 1 WHEN 'REARAXEL' 
                        THEN 2 WHEN 'ENGINE' 
                        THEN 3 WHEN 'HYDRAULIC' 
                        THEN 4 WHEN 'ENGINE FIP' 
                        THEN 5 WHEN 'STARTER MOTOR' 
                        THEN 6 WHEN 'ALTERNATOR' 
                        THEN 7 WHEN 'RADIATOR' 
                        THEN 8 WHEN 'FRONT TYRE RIGHT' 
                        THEN 9 WHEN 'FRONT TYRE LEFT' 
                        THEN 10 WHEN 'REAR TYRE RIGHT' 
                        THEN 11 WHEN 'REAR TYRE LEFT' 
                        THEN 12 WHEN 'CARE BUTTON' 
                        THEN 13 WHEN 'BATTERY' 
                        THEN 14 END) A", data.Plant.ToUpper(), data.Family.ToUpper(), data.FromDate, data.ToDate);

                dt = returnDataTable(query);

            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {

            }
            return dt;
        }

        public List<EKIDashbordModel> BindEKIDashbord(EKIDashbordModel data)
        {
            DataTable dt = new DataTable();
            List<EKIDashbordModel> eki = new List<EKIDashbordModel>();
            string SHORT_BULK = Convert.ToString(ConfigurationManager.AppSettings["SHORT_BULK"]);
            string SHORT_SUPERMKT = Convert.ToString(ConfigurationManager.AppSettings["SHORT_SUPERMKT"]);
            try
            {
                using (OracleCommand oc = new OracleCommand("USP_EKIDASHBORD", Connection()))
                {
                    ConOpen();
                    oc.CommandType = CommandType.StoredProcedure;
                    oc.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Plant.Trim();
                    oc.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.Family.Trim();
                    oc.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FromDate.Trim();
                    oc.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ToDate.Trim();
                    oc.Parameters.Add("pSHORT_BULK", OracleDbType.NVarchar2, ParameterDirection.Input).Value = SHORT_BULK.Trim();
                    oc.Parameters.Add("pSHORT_SUPRMKT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = SHORT_SUPERMKT.Trim();
                    oc.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);

                    OracleDataAdapter oda = new OracleDataAdapter(oc);
                    oda.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        eki.Add( new EKIDashbordModel 
                        {
                            TODAYS_MRN = Convert.ToString(dt.Rows[0]["MRN"]),
                            QC_REJECTION = Convert.ToString(dt.Rows[0]["REJ_QULTY"]),
                            SHORT_MRN = Convert.ToString(dt.Rows[0]["SHORT_MRN"]),
                            REJECTION_LINE = Convert.ToString(dt.Rows[0]["REJ_PRODUCTION"]),
                            ITEM_EXCEED_MAXINVENTORY = Convert.ToString(dt.Rows[0]["ITEM_EXCEED_MAXINVENTORY"]),
                            PACKETS_OTHER_SNP = Convert.ToString(dt.Rows[0]["PACKETS_OTHER_SNP"]),
                            ITEM_TEMP_LOCATION = Convert.ToString(dt.Rows[0]["ITEM_TEMP_LOCATION"]),
                            SHORT_BULK = Convert.ToString(dt.Rows[0]["SHORT_BULK"]),
                            SHORT_SUPRMKT = Convert.ToString(dt.Rows[0]["SHORT_SUPRMKT"]),
                            VENDOR_SHORT = Convert.ToString(dt.Rows[0]["VENDOR_SHORT"])

                        });


                    }
                    
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
            finally
            {
                ConClose();
            }
            return eki;
        }

        public List<GateEntryModel> GridGateEntryData(GateEntryModel obj)
        {
            DataTable dt = new DataTable();
            List<GateEntryModel> GateEnteryDetails = new List<GateEntryModel>();
            try
            {
                string search = obj.P_Search;
                int days = 0;
                if (!string.IsNullOrEmpty(search))
                {
                    search = Convert.ToString(obj.P_Search).ToUpper().Trim();
                }
                obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);
                if (obj.CheckboxReprint!=true)
                {
                    days = Convert.ToInt16(ConfigurationSettings.AppSettings["MRN_DAYS"]);
                    if (ConfigurationSettings.AppSettings["LOGIN_CHECK_MRN"].ToString() != "N")
                    {
                        query = @" select distinct '0' SELECT_MRN, to_char( TRANSACTION_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) TRANSACTION_DATE,MRN_NO,VEHICLE_NO, ";
                        query += " VENDOR_CODE,VENDOR_NAME,INVOICE_NO,INVOICE_DATE, ";
                        query += " (select g.ITEM_DESCRIPTION || ' [' || g.ITEM_CODE || ']' from apps.XXESGATETAGPRINT_BARCODE g where g.mrn_no=b.MRN_NO and g.Invoice_no=b.Invoice_no AND ";
                        query += " to_char(TRANSACTION_DATE,'dd-Mon-yyyy')>=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "' and UPPER(CREATED_BY)='" + HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim() + "' and rownum=1) ITEM, ";
                        query += " (select count(*) from apps.XXESGATETAGPRINT_BARCODE g where g.mrn_no=b.MRN_NO and g.Invoice_no=b.Invoice_no AND ";
                        query += " to_char(TRANSACTION_DATE,'dd-Mon-yyyy')>=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "' and UPPER(CREATED_BY)='" + HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim() + "' ) TOTAL_ITEM  ,SOURCE_DOCUMENT_CODE SOURCE_TYPE,ORGANIZATION_CODE,STATUS from apps.XXESGATETAGPRINT_BARCODE b where  ";
                        query += "  to_char(TRANSACTION_DATE,'dd-Mon-yyyy')>=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "' and UPPER(CREATED_BY)='" + HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim() + "' and MRN_NO not in (select mrn_no from ITEM_RECEIPT_DETIALS)  And MRN_No like '%" + search + "%' order by mrn_no,invoice_no";
                    }
                    else if (ConfigurationSettings.AppSettings["LOGIN_CHECK_MRN"].ToString() == "N")
                    {
                        query = @" SELECT A.TOTALCOUNT,A.SELECT_MRN,A.TRANSACTION_DATE,A.MRN_NO,A.VEHICLE_NO,A.VENDOR_CODE,A.VENDOR_NAME,A.INVOICE_NO,A.ITEM,A.TOTAL_ITEM,A.SOURCE_TYPE,A.ORGANIZATION_CODE,A.STATUS,A.INVOICE_DATE FROM(select distinct '0' SELECT_MRN,ROW_NUMBER() OVER (ORDER BY INVOICE_NO) as ROWNMBER,COUNT(*) over() as TOTALCOUNT, to_char( TRANSACTION_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) TRANSACTION_DATE,MRN_NO,VEHICLE_NO, ";
                        query += " VENDOR_CODE,VENDOR_NAME,INVOICE_NO,INVOICE_DATE, ";
                        query += " (select g.ITEM_DESCRIPTION || ' [' || g.ITEM_CODE || ']' from apps.XXESGATETAGPRINT_BARCODE g where g.mrn_no=b.MRN_NO and g.Invoice_no=b.Invoice_no AND ";
                        query += " to_char(TRANSACTION_DATE,'dd-Mon-yyyy')=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "' and rownum=1) ITEM, ";
                        query += " (select count(*) from apps.XXESGATETAGPRINT_BARCODE g where g.mrn_no=b.MRN_NO and g.Invoice_no=b.Invoice_no AND ";
                        query += " to_char(TRANSACTION_DATE,'dd-Mon-yyyy')=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "') TOTAL_ITEM  ,SOURCE_DOCUMENT_CODE SOURCE_TYPE,ORGANIZATION_CODE,STATUS from apps.XXESGATETAGPRINT_BARCODE b where  ";
                        query += "  to_char(TRANSACTION_DATE,'dd-Mon-yyyy')=to_char(sysdate-" + days + ",'dd-Mon-yyyy') and ORGANIZATION_CODE='" + obj.PLANTCODE + "' and  MRN_NO not in (select mrn_no from ITEM_RECEIPT_DETIALS) And MRN_No like '%" + search + "%' order by mrn_no,invoice_no) A  where ROWNMBER > " + Convert.ToInt32(obj.start) + " and ROWNMBER <= " + Convert.ToInt32(obj.length) + ""; 
                        }
                 
                }
                else
                    //query = @"select distinct '0' SELECT_MRN,to_char( TRANSACTION_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) TRANSACTION_DATE,MRN_NO,
                    //        VENDOR_CODE,VENDOR_NAME,INVOICE_NO,INVOICE_DATE,VEHICLE_NO,
                    //        (select g.ITEM_DESCRIPTION || ' [' || g.ITEM_CODE || ']' from apps.XXESGATETAGPRINT_BARCODE g where g.mrn_no=b.MRN_NO and g.Invoice_no=b.Invoice_no and rownum=1) ITEM,
                    //        SOURCE_DOCUMENT_CODE SOURCE_TYPE,ORGANIZATION_CODE,STATUS from apps.XXESGATETAGPRINT_BARCODE b where  
                    //         ORGANIZATION_CODE='" + PubFun.Login_Unit + "' and MRN_NO in (select mrn_no from ITEM_RECEIPT_DETIALS where to_char(TRANSACTION_DATE,'dd-Mon-yyyy')>=to_date('" + dateTimePicker1.Value.ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy'))  order by mrn_no,invoice_no";
                    query = string.Format(@"SELECT A.TOTALCOUNT,A.SELECT_MRN,A.TRANSACTION_DATE,A.MRN_NO,A.VEHICLE_NO,A.VENDOR_CODE,A.VENDOR_NAME,A.INVOICE_NO,A.ITEM,A.TOTAL_ITEM,A.SOURCE_TYPE,A.ORGANIZATION_CODE,A.STATUS,A.INVOICE_DATE FROM(SELECT distinct '0' SELECT_MRN,ROW_NUMBER() OVER (ORDER BY INVOICE_NO) as ROWNMBER,COUNT(*) over() as TOTALCOUNT,to_char( TRANSACTION_DATE, 'dd-Mon-yyyy HH24:MI:SS' ) TRANSACTION_DATE,MRN_NO,VEHICLE_NO,
                            VENDOR_CODE,VENDOR_NAME,INVOICE_NO,INVOICE_DATE,
                             b.ITEM_DESCRIPTION || ' [' || b.ITEM_CODE || ']'  ITEM,TOTAL_ITEM,
                            SOURCE_DOCUMENT_CODE SOURCE_TYPE,PLANT_CODE ORGANIZATION_CODE,STATUS from ITEM_RECEIPT_DETIALS b where  
                             to_char(TRANSACTION_DATE,'dd-Mon-yyyy')>=to_date('" + obj.FromDate.ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and PLANT_CODE='" + obj.PLANTCODE + "' And MRN_No like '%" + search + "%' order by mrn_no,invoice_no)A  where ROWNMBER > {0} and ROWNMBER <= {1} ", Convert.ToInt32(obj.start),obj.length);
             
                dt = returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        GateEntryModel gateEntry = new GateEntryModel
                        {
                            TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
                            TRANSACTION_DATE = dr["TRANSACTION_DATE"].ToString(),
                            MRN_NO = dr["MRN_NO"].ToString(),
                            VEHICLE_NO = dr["VEHICLE_NO"].ToString(),
                            VENDOR_CODE = dr["VENDOR_CODE"].ToString(),
                            VENDOR_NAME = dr["VENDOR_NAME"].ToString(),
                            ITEM = dr["ITEM"].ToString(),
                            TOTAL_ITEM = dr["TOTAL_ITEM"].ToString(),
                            ORGANIZATION_CODE = dr["ORGANIZATION_CODE"].ToString(),
                            INVOICE_NO = dr["INVOICE_NO"].ToString(),
                            INVOICE_DATE = dr["INVOICE_DATE"].ToString(),
                            SOURCE_TYPE = dr["SOURCE_TYPE"].ToString(),
                            STATUS = dr["STATUS"].ToString()
                        };
                        GateEnteryDetails.Add(gateEntry);
                    }
                }


            }
            catch (Exception ex)
            {
                LogWrite(ex);
                throw;
            }

            return GateEnteryDetails;
        }
     

        public bool isInvoiceExistsinFinancialYear(string invoice, string vendorcode)
        {
            try
            {
                int curmon = ServerDate.Month;
                string fromdate = string.Empty, todate = string.Empty;
                if (curmon > 3)
                {
                    fromdate = "01-Apr-" + (ServerDate.Year).ToString();
                    todate = "31-Mar-" + (ServerDate.Year + 1).ToString();
                }
                else
                {
                    fromdate = "01-Apr-" + (ServerDate.Year - 1).ToString();
                    todate = "31-Mar-" + ServerDate.Year.ToString();
                }

                query = string.Format(@"SELECT COUNT(*) FROM ITEM_RECEIPT_DETIALS i, XXES_MRNINFO m WHERE i.mrn_no=m.mrn_no and i.plant_code=m.plant_code 
                    and m.INVOICE_NO='{0}' AND trunc(m.CREATEDDATE)>='{1}' AND TRUNC(m.CREATEDDATE)<='{2}'
                    and i.vendor_code='{3}' ",
                    invoice, fromdate, todate, vendorcode);
                return CheckExits(query);
            }
            catch (Exception)
            {

                throw;
            }

        }
        public bool CheckMyPrinter(string printerToCheck)
        {
            DataTable dt = new DataTable();
            query = string.Format(@"SELECT * FROM   Win32_Printer");
            dt = returnDataTable(query);
            bool IsReady = false; string printerName = "";

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    printerName = row["Name"].ToString().ToLower();
                    if (printerName.ToLower().Equals(printerToCheck.Trim().ToLower()))
                    {
                        if (row["WorkOffline"].ToString().ToLower().Equals("false"))
                        {
                            IsReady = true;
                        }
                        //string state =  printer["PrinterState"].ToString();
                        //string status = printer["PrinterStatus"].ToString();
                        //string error = printer["DetectedErrorState"].ToString(); 

                    }
                }
            }
            return IsReady;
        }

        public string SendMails(string Module, string MailBody, string MailSubject, string Email_To, string Email_CC,
       string Username)
        {
           
                string MAIL_PRIORITY = string.Empty;
                string SMTP_SERVER = string.Empty;
                string LOGIN_EMAIL = string.Empty;
                string PASSWORD = string.Empty;
                string IS_SSL = string.Empty;
                string SMTP_PORT = string.Empty;
                DataTable dt = new DataTable();
                string query = string.Empty, LoggedinUser = string.Empty;
                Function pubfun = new Function();
                query = string.Format(@"SELECT *FROM XXES_SMTP_DETAILS");
                dt = pubfun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    SMTP_SERVER = dt.Rows[0]["SMTP_SERVER"].ToString();
                    LOGIN_EMAIL = dt.Rows[0]["SMTP_USER"].ToString();
                    PASSWORD = dt.Rows[0]["SMTP_PASSWORD"].ToString();
                    IS_SSL = dt.Rows[0]["SSL_ENABLE"].ToString();
                    SMTP_PORT = dt.Rows[0]["SMTP_PORT"].ToString();
                    MAIL_PRIORITY = dt.Rows[0]["SMTP_PRIORITY"].ToString();
                }
                try
                {
                    if (SMTP_SERVER == "" || LOGIN_EMAIL == "")
                    {
                        return "Kindly check the SMTP settings.";
                    }

                    MailMessage Mail = new MailMessage();
                    Mail.Subject = MailSubject;
                    Mail.To.Add(Email_To);
                    if ((Email_CC != ""))
                    {
                        Mail.CC.Add(Email_CC);
                    }

                    Mail.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
                    if (MAIL_PRIORITY.ToUpper() == "NORMAL")
                    {
                        Mail.Priority = MailPriority.Normal;
                    }
                    else if (MAIL_PRIORITY.ToUpper() == "HIGH")
                    {
                        Mail.Priority = MailPriority.High;
                    }
                    else if (MAIL_PRIORITY.ToUpper() == "LOW")
                    {
                        Mail.Priority = MailPriority.Low;
                    }

                    Mail.From = new System.Net.Mail.MailAddress(LOGIN_EMAIL);
                    Mail.IsBodyHtml = true;
                    MailBody = HttpUtility.HtmlDecode(MailBody);
                    Mail.Body = MailBody;
                    SmtpClient sc = new SmtpClient();
                    sc.Host = SMTP_SERVER.Trim();
                    //sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                    if (SMTP_PORT.Trim() != string.Empty)
                    {
                        sc.Port = Convert.ToInt16(SMTP_PORT);
                    }

                    if (IS_SSL.Trim() == "1" || IS_SSL.Trim() == "Y")
                    {
                        sc.EnableSsl = true;
                    }

                    sc.UseDefaultCredentials = false;
                    sc.Credentials = new System.Net.NetworkCredential(LOGIN_EMAIL, PASSWORD);
                    sc.Send(Mail);
                    query = string.Format(@"INSERT INTO XXES_MAILSLOG (MODULE,USERNAME,SUBJECT,EMAIL,BODY,STATUS,REMARKS)
                    VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", Module, pubfun.replaceApostophi(Username), pubfun.replaceApostophi(MailSubject), Email_To, "", "SUCCESS", "-");
                    pubfun.EXEC_QUERY(query);
                    Console.WriteLine("Application Has Mailed To " + Email_To + " at " + DateTime.Now.ToString() + ".");
                    return "Application Has Mailed To " + Email_To + " at " + DateTime.Now.ToString() + ".";
                }
                catch (Exception ex)
                {
                    pubfun.LogWrite(ex);
                    Console.WriteLine("Module SendMails : " + ex.Message.ToString());
                    query = string.Format(@"INSERT INTO XXES_MAILSLOG (MODULE,USERNAME,SUBJECT,EMAIL,BODY,STATUS,REMARKS)
                    VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", Module, pubfun.replaceApostophi(Username), pubfun.replaceApostophi(MailSubject), Email_To, "", "FAILED", pubfun.replaceApostophi(ex.Message));
                    pubfun.EXEC_QUERY(query);
                    return "Error" + ex.Message.ToString();
                }
            return "";
        }

    }


}



