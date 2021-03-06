using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Common
{
    public class Assemblyfunctions
    {
        Function funtion = new Function();
        public static string Connstring = ConfigurationManager.ConnectionStrings["CON"].ConnectionString;
        string query = string.Empty;
        public static void setdate()
        {
            try
            {
                //PubFun.SystemDateFormat = Convert.ToString(Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\International", "sShortDate", ""));
                Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\International", "sShortDate", "dd-MMM-yyyy");
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
        }
        public DataTable BindJobs(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (!string.IsNullOrEmpty(cOMMONDATA.REMARKS))
                {
                    if (cOMMONDATA.REMARKS.Contains("#"))
                        cOMMONDATA.REMARKS = cOMMONDATA.REMARKS.Split('#')[2].Trim();
                }
                
                if (cOMMONDATA.LOCATION == "BUCKLEUP")
                {
                    query = string.Format(@"select JOBID as TEXT,JOBID as CODE  from XXES_DAILY_PLAN_JOB where FCODE_AUTOID = '{0}' 
                and JOBID not in (select JOBID from XXES_JOB_STATUS) and plant_code = '{1}' and family_code = '{2}' order by JOBID",
                    cOMMONDATA.REMARKS, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                }
                else
                {
                    query = string.Format(@"select JOBID as TEXT,JOBID as CODE  from XXES_DAILY_PLAN_JOB where FCODE_AUTOID = '{0}' 
                and JOBID not in (select JOBID from XXES_KITTING_JOBS) and plant_code = '{1}' and family_code = '{2}' order by JOBID",
                 cOMMONDATA.REMARKS, cOMMONDATA.PLANT, cOMMONDATA.FAMILY);
                }
                dataTable = funtion.returnDataTable(query);
            }
            catch (Exception)
            {

                throw;
            }
            return dataTable;
        }
        public ShiftDetail getshift()
        {
            ShiftDetail shiftDetail = null;
            try
            {

                DateTime ServerDate = funtion.GetServerDateTime();
                using (OracleConnection cn = new OracleConnection(Connstring))
                {
                    string returnData = "", isDayNeedToLess = "";
                    DataTable dtshift = new DataTable();
                    DateTime ShiftStart = new DateTime(), shiftEnd = new DateTime();
                    cn.Open();
                    OracleCommand cmd = new OracleCommand("select * from XXES_SHIFT_MASTER order by SHIFTCODE", cn);
                    cmd.CommandType = CommandType.Text;
                    OracleDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            if (Convert.ToString(dr["SHIFTCODE"]) == "A")
                            {
                                ShiftStart = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                                shiftEnd = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            }
                            else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && ServerDate >= Convert.ToDateTime(DateTime.Parse("00:01")) && ServerDate <= Convert.ToDateTime(DateTime.Parse("01:00")))
                            {
                                ShiftStart = Convert.ToDateTime(ServerDate.Date.AddDays(-1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                                shiftEnd = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                                isDayNeedToLess = "1";
                            }
                            else if (Convert.ToString(dr["SHIFTCODE"]) == "B" && ServerDate >= Convert.ToDateTime(DateTime.Parse(Convert.ToString(dr["START_TIME"]))) && ServerDate <= Convert.ToDateTime(DateTime.Parse("23:59")))
                            {
                                ShiftStart = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                                shiftEnd = Convert.ToDateTime(ServerDate.Date.AddDays(1).ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            }
                            else if (Convert.ToString(dr["SHIFTCODE"]) == "C")
                            {
                                ShiftStart = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["START_TIME"]));
                                shiftEnd = Convert.ToDateTime(ServerDate.Date.ToString("dd/MMM/yyy") + " " + Convert.ToString(dr["END_TIME"]));
                            }
                            if (ServerDate >= ShiftStart && ServerDate <= shiftEnd)
                            {
                                if (shiftDetail == null)
                                    shiftDetail = new ShiftDetail();
                                shiftDetail.Shiftcode = Convert.ToString(dr["SHIFTCODE"]);
                                shiftDetail.NightExists = Convert.ToString(dr["NIGHT_EXISTS"]);
                                shiftDetail.isDayNeedToLess = isDayNeedToLess;
                                shiftDetail.ShiftStart = ShiftStart.ToString();
                                shiftDetail.shiftEnd = shiftEnd.ToString();
                                if (isDayNeedToLess == "1" || shiftDetail.Shiftcode == "C")
                                    shiftDetail.Plandate = ServerDate.Date.AddDays(-1);
                                else
                                    shiftDetail.Plandate = ServerDate;
                                break;
                            }

                        }
                    }
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            return shiftDetail;
        }
        public void getName(string fcode_desc, ref string itemname1, ref string itemname2)
        {
            try
            {
                itemname2 = "";
                itemname1 = fcode_desc;
                if (itemname1.Length > 50)
                {
                    itemname1 = itemname1.Trim().Substring(0, 27);
                    itemname2 = fcode_desc.Trim().Substring(27, fcode_desc.Trim().ToUpper().Length - 27);
                    if (itemname2.Trim().Length > 25)
                    {
                        itemname2 = itemname2.Substring(0, 24);
                    }
                }
                else if (itemname1.Length > 25)
                {
                    itemname1 = itemname1.Trim().Substring(0, 27);
                    itemname2 = fcode_desc.Trim().ToUpper().Substring(27, fcode_desc.Trim().Length - 27);
                }

            }
            catch { throw; }
            finally { }
        }
        public void getNameLongDesc(string fcode_desc, out string[] itemname)
        {
            string line = string.Empty;
            try
            {
                int start = 0, end = 25, size = 0;
                size = fcode_desc.Length / 25;
                if (size > 8)
                    size = 8;
                else
                    size = size + 1;
                itemname = new string[size];

                for (int i = 0; i < size; i++)
                {
                    if (fcode_desc.Length >= 25)
                    {
                        line = fcode_desc.Trim().ToUpper().Substring(start, end);
                        fcode_desc = fcode_desc.Trim().ToUpper().Substring(end);
                    }
                    else
                    {
                        if (fcode_desc.Length > 0)
                            line = fcode_desc.Trim().ToUpper().Substring(start, fcode_desc.Length - 1);
                        else
                            line = string.Empty;
                    }
                    itemname[i] = line;
                    //start = start + end;
                }


            }
            catch
            {
                throw;
            }
            finally { }
        }
        public string SplitDcode(string barcode, string type)
        {
            string dcode = string.Empty;
            try
            {
                if (barcode.Contains("$"))
                {
                    if (type.Equals("RADIATOR") || type.Equals("ALT") || type.Equals("START_MOTOR") || type.Equals("POWER_STMOTOR"))
                        dcode = barcode.Split('$')[1].Trim();
                    else if (type.Equals("ALT") || type.Equals("FTRT") || type.Equals("FT")
                         || type.Equals("RT") || type.Equals("STEERING") || type.Equals("HYDRAULIC"))
                        dcode = barcode.Split('$')[0].Trim();
                }
                else
                    dcode = string.Empty;
            }
            catch (Exception ex)
            {


            }
            return dcode;
        }
        public void InsertIntoScannedStages(string PLANT, string FAMILY, string FCODE, string JOB, string StageId, string SCANNED_BY)
        {
            string errModule = "";
            try
            {

                string query = string.Format(@"Insert into XXES_SCAN_TIME(PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,STAGE,SCAN_DATE,SCANNED_BY)
                values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", PLANT.Trim().ToUpper(), FAMILY.Trim().ToUpper(), FCODE.Trim().ToUpper(), JOB.Trim(),
                StageId.ToUpper().Trim(), "SYSDATE", SCANNED_BY.Trim().ToUpper());
                funtion.EXEC_QUERY(query);
                errModule = "";
            }
            catch
            {
                errModule = "InsertIntoScannedStages";
            }
            finally { }
        }
        public string getTractordescription(string plant, string family, string itemcode)
        {
            try
            {
                query = string.Format(@"select REPLACE( REPLACE(ITEM_DESCRIPTION , 'TRACTOR FARMTRAC', 'FT'),'TRACTOR POWERTRAC','PT') from XXES_ITEM_MASTER where ITEM_CODE='{0}' and plant_code='{1}' 
                                and family_code='{2}'", itemcode.Trim(), plant.Trim(), family.Trim());
                return funtion.get_Col_Value(query);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public DataTable GetFcodes(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            try
            {
                ShiftDetail shiftDetail = getshift();
                if (shiftDetail != null)
                {
                    if (!string.IsNullOrEmpty(shiftDetail.Shiftcode) && shiftDetail.Plandate != null)
                    {
                        if (cOMMONDATA.REMARKS == "KITTING")
                        {
                            query = string.Format(@"select distinct  xt.ITEM_CODE || ' # ' || substr( REPLACE( REPLACE(xt.ITEM_DESC , 'TRACTOR FARMTRAC', 'FT'),'TRACTOR POWERTRAC','PT'),1,25) || '#' || xt.AUTOID as TEXT,
                        xt.AUTOID as CODE from XXES_DAILY_PLAN_MASTER xm,XXES_DAILY_PLAN_TRAN xt,XXES_DAILY_PLAN_JOB jt
                        where xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' 
                        and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}' and xt.AUTOID=jt.FCODE_AUTOID and jt.JOBID not in (select S.JOBID from XXES_KITTING_JOBS S) ",
                            cOMMONDATA.PLANT, cOMMONDATA.FAMILY, shiftDetail.Plandate.ToString("dd-MMM-yyyy"));
                            //and jt.JOBID not in (select S.JOBID from XXES_JOB_STATUS S)
                        }
                        else
                        {
                            query = string.Format(@"select distinct  xt.ITEM_CODE || ' # ' || substr( REPLACE( REPLACE(xt.ITEM_DESC , 'TRACTOR FARMTRAC', 'FT'),'TRACTOR POWERTRAC','PT'),1,25) || '#' || xt.AUTOID as TEXT,xt.ITEM_CODE,
                        xm.PLAN_ID,xt.AUTOID,xt.ITEM_CODE || '#' || xt.AUTOID as ITEMCODE,xt.seq_no from XXES_DAILY_PLAN_MASTER xm,XXES_DAILY_PLAN_TRAN xt,XXES_DAILY_PLAN_JOB jt
                        where xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' 
                        and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}' and xt.AUTOID=jt.FCODE_AUTOID and jt.JOBID not in (select S.JOBID from XXES_JOB_STATUS S)",
                           cOMMONDATA.PLANT, cOMMONDATA.FAMILY, shiftDetail.Plandate.ToString("dd-MMM-yyyy"));
                        }
                        dataTable = funtion.returnDataTable(query);
                    }
                }
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            return dataTable;
        }

        public DataTable GetBackends(COMMONDATA cOMMONDATA)
        {
            DataTable dataTable = new DataTable();
            try
            {
                ShiftDetail shiftDetail = getshift();
                if (shiftDetail != null)
                {
                    if (!string.IsNullOrEmpty(shiftDetail.Shiftcode) && shiftDetail.Plandate != null)
                    {
                        query = string.Format(@"select distinct  xt.ITEM_CODE || ' # ' || xt.ITEM_DESC  as TEXT,xt.ITEM_CODE, xm.PLAN_ID,xt.AUTOID,
                                xt.ITEM_CODE || '#' || xt.AUTOID as ITEMCODE from XXES_DAILY_PLAN_MASTER xm,XXES_DAILY_PLAN_TRAN xt where 
                                xm.Plan_id=xt.Plan_id and xm.plant_code='{0}' and xm.family_code='{1}' and to_char(xm.PLAN_DATE,'dd-Mon-yyyy')='{2}'
                                and xt.qty > (select count(*) from xxes_backend_status where fcode_id=xt.AUTOID) order by xm.PLAN_ID,xt.AUTOID",
                                cOMMONDATA.PLANT, cOMMONDATA.FAMILY, shiftDetail.Plandate.ToString("dd-MMM-yyyy"));
                    }
                    dataTable = funtion.returnDataTable(query);
                }
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
            }
            return dataTable;
        }
        public DataTable BindBackendJob(COMMONDATA cOMMONDATA)
        {
            string response = string.Empty;
            DataTable dataTable = new DataTable();
            try
            {
                if (cOMMONDATA.ITEMCODE.Contains("#"))
                    cOMMONDATA.ITEMCODE = cOMMONDATA.ITEMCODE.Split('#')[0].Trim();
                string backendplant = string.Empty, backendfamily = string.Empty, orgid = string.Empty;
                query = string.Format(@"SELECT M.PLANT_CODE || '#' || M.FAMILY_CODE FROM XXES_ITEM_MASTER  m JOIN XXES_DAILY_PLAN_TRAN t
                ON m.ITEM_CODE = t.FCODE 
                WHERE T.FCODE IS NOT NULL AND T.AUTOID='{0}'", cOMMONDATA.JOB);
                string line = funtion.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    backendplant = line.Split('#')[0].Trim().ToUpper();
                    backendfamily = line.Split('#')[1].Trim().ToUpper();
                }
                if (string.IsNullOrEmpty(backendplant) || string.IsNullOrEmpty(backendfamily))
                {
                    response = "ERROR : PLANT/FAMILY NOT FOUND";
                }
                if (backendplant == "T04")
                {
                    orgid = "149";
                    backendfamily = "BACKEND FTD";
                }
                else if (backendplant == "T05")
                {
                    orgid = "150";
                    backendfamily = "BACKEND TD";
                }

                query = string.Format(@"SELECT WIP_ENTITY_NAME TEXT,WIP_ENTITY_NAME || '#' || START_QUANTITY || '#' || NVL(CC,0) AS JOBID FROM 
                (SELECT DISTINCT A.WIP_ENTITY_NAME,START_QUANTITY,SEGMENT1 FROM RELESEDJOBORDER A WHERE  A.SEGMENT1 ='{0}' and A.ORGANIZATION_ID='{1}'  ) 
                X LEFT JOIN (SELECT JOBID ,COUNT(SRNO) AS CC  FROM xxes_print_serials WHERE  DCODE ='{0}' GROUP BY JOBID) Y ON  X.WIP_ENTITY_NAME = Y.JOBID WHERE 
                X.START_QUANTITY != NVL( Y.CC,0)  and SEGMENT1 ='{0}' ORDER BY WIP_ENTITY_NAME", cOMMONDATA.ITEMCODE, orgid);
                dataTable = funtion.returnDataTable(query);
            }
            catch (Exception ex)
            {

                throw;
            }
            return dataTable;
        }
        public string getSeries(string unit, string family, string stage)
        {
            int Start_serial_number, Current_Serial_number, End_serial_Number;
            try
            {
                string prefix = ""; Current_Serial_number = Start_serial_number = End_serial_Number = 0;
                DataTable dt = funtion.returnDataTable(@"select Start_serial_number,
                Current_Serial_number,End_serial_Number,Barcode_prefix from XXES_FAMILY_SERIAL
                where plant_code='" + unit.Trim() + "' and family_code='" + family.Trim() + "'" +
                " and offline_keycode='" + stage.Trim() + "'");
                if (dt.Rows.Count > 0)
                {
                    prefix = Convert.ToString(dt.Rows[0]["Barcode_prefix"]).Trim();
                    if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() == "")
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["Start_serial_number"]).Trim())) + 1;
                    else if (Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim() != "")
                        Current_Serial_number = Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["Current_Serial_number"]).Trim())) + 1;
                    if (Current_Serial_number > Convert.ToInt32(Convert.ToString(Convert.ToString(dt.Rows[0]["End_serial_Number"]).Trim())))
                    {
                        Current_Serial_number = -99; //series full
                        prefix = "";
                    }
                }
                return prefix.Trim() + "#" + Convert.ToString(Current_Serial_number);
            }
            catch { return ""; }
            finally { }
        }
        public void ResetSerialNos(string plant, string family, string offlinekeycode, string PartConstant)
        {
            try
            {
                string MonthName = funtion.GetServerDateTime().ToString("MMM-yyyy").ToUpper();
                query = string.Format(@"select count(*) from XXES_SFT_SETTINGS where parameterinfo='{0}' and paramvalue='{1}'
                and plant_code='{2}' and family_code='{3}'", PartConstant, MonthName, plant, family);

                if (!funtion.CheckExits(query))
                {
                    query = string.Format(@"update XXES_FAMILY_SERIAL set CURRENT_SERIAL_NUMBER='' where OFFLINE_KEYCODE='{0}' 
                    and family_code='{1}' and plant_code='{2}'", offlinekeycode, family, plant);

                    if (funtion.EXEC_QUERY(query))
                    {
                        query = string.Format(@"Insert into XXES_SFT_SETTINGS(parameterinfo,paramvalue,plant_code,family_code) 
                        values('{0}','{1}','{2}','{3}')", PartConstant, MonthName, plant, family);
                        funtion.EXEC_QUERY(query);
                    }
                }

            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            finally { }
        }

       
        public string GetfreeJobFromOracle(string itemcode, string Orgid)
        {
            try
            {
                query = string.Format(@"SELECT WIP_ENTITY_NAME FROM RELESEDJOBORDER WHERE SEGMENT1='{0}' AND ORGANIZATION_ID='{1}' AND WIP_ENTITY_NAME NOT IN
               (SELECT jobid FROM XXES_JOB_STATUS where jobid is not null ) and WIP_ENTITY_NAME is not null", itemcode.Trim(), Orgid);
                return funtion.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }

        }

        public bool InsertTractorfromExcel(DataRow dataRow, string Jobid, JOBSTATUS data)
        {
            bool result = false;

            try
            {
                if (funtion == null)
                    funtion = new Function();
                if (!DuplicateCheck(Convert.ToString(dataRow["FCODE_SRLNO"]), "FCODE_SRLNO"))
                {
                    using (OracleCommand sc = new OracleCommand("UDSP_JOB_STATUS", funtion.Connection()))
                    {
                        funtion.ConOpen();
                        sc.CommandType = CommandType.StoredProcedure;
                        sc.Parameters.Add("p_STEERING_MOTOR_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STEERING_MOTOR_SRLNO"]) == null ? null : Convert.ToString(dataRow["STEERING_MOTOR_SRLNO"]);
                        sc.Parameters.Add("p_REARTYRE_SRLNO1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARTYRE_SRLNO1"]) == null ? null : Convert.ToString(dataRow["REARTYRE_SRLNO1"]);
                        sc.Parameters.Add("p_REARTYRE_SRLNO2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARTYRE_SRLNO2"]) == null ? null : Convert.ToString(dataRow["REARTYRE_SRLNO2"]);
                        sc.Parameters.Add("p_BACKEND_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BACKEND_SRLNO"]) == null ? null : Convert.ToString(dataRow["BACKEND_SRLNO"]);
                        sc.Parameters.Add("p_RADIATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RADIATOR"]) == null ? null : Convert.ToString(dataRow["RADIATOR"]);
                        sc.Parameters.Add("p_TRANSMISSION_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["TRANSMISSION_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["TRANSMISSION_DESCRIPTION"]);
                        sc.Parameters.Add("p_LABELPRINTED", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["LABELPRINTED"]) == null ? null : Convert.ToString(dataRow["LABELPRINTED"]);
                        sc.Parameters.Add("p_HYD_PUMP_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["HYD_PUMP_SRLNO"]) == null ? null : Convert.ToString(dataRow["HYD_PUMP_SRLNO"]);
                        sc.Parameters.Add("p_REMARKS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REMARKS"]) == null ? null : Convert.ToString(dataRow["REMARKS"]);
                        sc.Parameters.Add("p_BATTERY_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BATTERY_SRLNO"]) == null ? null : Convert.ToString(dataRow["BATTERY_SRLNO"]);
                        sc.Parameters.Add("p_ITEM_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ITEM_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["ITEM_DESCRIPTION"]);
                        sc.Parameters.Add("p_STARTER_MOTOR_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STARTER_MOTOR_SRLNO"]) == null ? null : Convert.ToString(dataRow["STARTER_MOTOR_SRLNO"]);
                        sc.Parameters.Add("p_RTRIM2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RTRIM2"]) == null ? null : Convert.ToString(dataRow["RTRIM2"]);
                        sc.Parameters.Add("p_RTRIM1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RTRIM1"]) == null ? null : Convert.ToString(dataRow["RTRIM1"]);
                        sc.Parameters.Add("p_PDIDONEBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["PDIDONEBY"]) == null ? null : Convert.ToString(dataRow["PDIDONEBY"]);
                        sc.Parameters.Add("p_REARAXEL_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARAXEL_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["REARAXEL_DESCRIPTION"]);
                        sc.Parameters.Add("p_REARTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARTYRE_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["REARTYRE_DESCRIPTION"]);
                        sc.Parameters.Add("p_CLUSSTER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["CLUSSTER"]) == null ? null : Convert.ToString(dataRow["CLUSSTER"]);
                        sc.Parameters.Add("p_FRONTTYRE_SRLNO2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FRONTTYRE_SRLNO2"]) == null ? null : Convert.ToString(dataRow["FRONTTYRE_SRLNO2"]);
                        sc.Parameters.Add("p_BATTERY_MAKE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BATTERY_MAKE"]) == null ? null : Convert.ToString(dataRow["BATTERY_MAKE"]);
                        sc.Parameters.Add("p_FRONTTYRE_SRLNO1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FRONTTYRE_SRLNO1"]) == null ? null : Convert.ToString(dataRow["FRONTTYRE_SRLNO1"]);
                        sc.Parameters.Add("p_HYD_PUMP", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["HYD_PUMP"]) == null ? null : Convert.ToString(dataRow["HYD_PUMP"]);
                        sc.Parameters.Add("p_FRONTTYRE_MAKE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FRONTTYRE_MAKE"]) == null ? null : Convert.ToString(dataRow["FRONTTYRE_MAKE"]);
                        sc.Parameters.Add("p_STEERING_ASSEMBLY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STEERING_ASSEMBLY"]) == null ? null : Convert.ToString(dataRow["STEERING_ASSEMBLY"]);
                        sc.Parameters.Add("p_TRANSMISSION_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["TRANSMISSION_SRLNO"]) == null ? null : Convert.ToString(dataRow["TRANSMISSION_SRLNO"]);
                        sc.Parameters.Add("p_FCODE_ID", OracleDbType.Int32, ParameterDirection.Input).Value = dataRow["FCODE_ID"] == System.DBNull.Value ? 0 : Convert.ToInt32(dataRow["FCODE_ID"]);
                        sc.Parameters.Add("p_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.T2_PLANT_CODE.ToUpper().Trim();
                        sc.Parameters.Add("p_ITEM_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.ITEM_CODE;
                        sc.Parameters.Add("p_ENGINE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ENGINE_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["ENGINE_DESCRIPTION"]);
                        sc.Parameters.Add("p_SWAPCAREBTN", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["SWAPCAREBTN"]) == null ? null : Convert.ToString(dataRow["SWAPCAREBTN"]);
                        sc.Parameters.Add("p_OIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["OIL"]) == null ? null : Convert.ToString(dataRow["OIL"]);
                        sc.Parameters.Add("p_CLUSSTER_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["CLUSSTER_SRLNO"]) == null ? null : Convert.ToString(dataRow["CLUSSTER_SRLNO"]);
                        sc.Parameters.Add("p_FTTYRE2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FTTYRE2"]) == null ? null : Convert.ToString(dataRow["FTTYRE2"]);
                        sc.Parameters.Add("p_REARTYRE_MAKE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARTYRE_MAKE"]) == null ? null : Convert.ToString(dataRow["REARTYRE_MAKE"]);
                        sc.Parameters.Add("p_ALTERNATOR_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ALTERNATOR_SRLNO"]) == null ? null : Convert.ToString(dataRow["ALTERNATOR_SRLNO"]);
                        sc.Parameters.Add("p_FTTYRE1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FTTYRE1"]) == null ? null : Convert.ToString(dataRow["FTTYRE1"]);
                        sc.Parameters.Add("p_STARTER_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STARTER_MOTOR"]) == null ? null : Convert.ToString(dataRow["STARTER_MOTOR"]);
                        sc.Parameters.Add("p_FRONTTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FRONTTYRE"]) == null ? null : Convert.ToString(dataRow["FRONTTYRE"]);
                        sc.Parameters.Add("p_BATTERY_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BATTERY_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["BATTERY_DESCRIPTION"]);
                        sc.Parameters.Add("p_STERING_CYLINDER_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STERING_CYLINDER_SRLNO"]) == null ? null : Convert.ToString(dataRow["STERING_CYLINDER_SRLNO"]);
                        sc.Parameters.Add("p_JOBID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Jobid;
                        sc.Parameters.Add("p_HYDRALUIC_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["HYDRALUIC_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["HYDRALUIC_DESCRIPTION"]);
                        sc.Parameters.Add("p_FCODE_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.FCODE_SRLNO.ToUpper().Trim();
                        sc.Parameters.Add("p_ALTERNATOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ALTERNATOR"]) == null ? null : Convert.ToString(dataRow["ALTERNATOR"]);
                        sc.Parameters.Add("p_REARAXEL_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARAXEL_SRLNO"]) == null ? null : Convert.ToString(dataRow["REARAXEL_SRLNO"]);
                        sc.Parameters.Add("p_REARTYRE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARTYRE"]) == null ? null : Convert.ToString(dataRow["REARTYRE"]);
                        sc.Parameters.Add("p_STEERING_ASSEMBLY_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STEERING_ASSEMBLY_SRLNO"]) == null ? null : Convert.ToString(dataRow["STEERING_ASSEMBLY_SRLNO"]);
                        sc.Parameters.Add("p_FIPSRNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FIPSRNO"]) == null ? null : Convert.ToString(dataRow["FIPSRNO"]);
                        sc.Parameters.Add("p_HYDRALUIC_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["HYDRALUIC_SRLNO"]) == null ? null : Convert.ToString(dataRow["HYDRALUIC_SRLNO"]);
                        sc.Parameters.Add("p_BATTERY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BATTERY"]) == null ? null : Convert.ToString(dataRow["BATTERY"]);
                        sc.Parameters.Add("p_ENGINE_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ENGINE_SRLNO"]) == null ? null : Convert.ToString(dataRow["ENGINE_SRLNO"]);
                        sc.Parameters.Add("p_ROPS_SRNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ROPS_SRNO"]) == null ? null : Convert.ToString(dataRow["ROPS_SRNO"]);
                        sc.Parameters.Add("p_IMEI_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["IMEI_NO"]) == null ? null : Convert.ToString(dataRow["IMEI_NO"]);
                        sc.Parameters.Add("p_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = data.T2_FAMILY_CODE == null ? null : data.T2_FAMILY_CODE.ToUpper().Trim();
                        sc.Parameters.Add("p_RTTYRE2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RTTYRE2"]) == null ? null : Convert.ToString(dataRow["RTTYRE2"]);
                        sc.Parameters.Add("p_ENGINE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ENGINE"]) == null ? null : Convert.ToString(dataRow["ENGINE"]);
                        sc.Parameters.Add("p_RTTYRE1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RTTYRE1"]) == null ? null : Convert.ToString(dataRow["RTTYRE1"]);
                        sc.Parameters.Add("p_STEERING_MOTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STEERING_MOTOR"]) == null ? null : Convert.ToString(dataRow["STEERING_MOTOR"]);
                        sc.Parameters.Add("p_BACKEND", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["BACKEND"]) == null ? null : Convert.ToString(dataRow["BACKEND"]);
                        sc.Parameters.Add("p_FTRIM1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FTRIM1"]) == null ? null : Convert.ToString(dataRow["FTRIM1"]);
                        sc.Parameters.Add("p_HYDRALUIC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["HYDRALUIC"]) == null ? null : Convert.ToString(dataRow["HYDRALUIC"]);
                        sc.Parameters.Add("p_TRANSMISSION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["TRANSMISSION"]) == null ? null : Convert.ToString(dataRow["TRANSMISSION"]);
                        sc.Parameters.Add("p_REMARKS2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REMARKS2"]) == null ? null : Convert.ToString(dataRow["REMARKS2"]);
                        sc.Parameters.Add("p_SIM_SERIAL_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["SIM_SERIAL_NO"]) == null ? null : Convert.ToString(dataRow["SIM_SERIAL_NO"]);
                        sc.Parameters.Add("p_FTRIM2", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FTRIM2"]) == null ? null : Convert.ToString(dataRow["FTRIM2"]);
                        sc.Parameters.Add("p_REMARKS1", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REMARKS1"]) == null ? null : Convert.ToString(dataRow["REMARKS1"]);
                        sc.Parameters.Add("p_CAREBUTTONOIL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["CAREBUTTONOIL"]) == null ? null : Convert.ToString(dataRow["CAREBUTTONOIL"]);
                        sc.Parameters.Add("p_PDIOKDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["PDIOKDATE"]) == null ? null : Convert.ToString(dataRow["PDIOKDATE"]);
                        sc.Parameters.Add("p_STERING_CYLINDER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["STERING_CYLINDER"]) == null ? null : Convert.ToString(dataRow["STERING_CYLINDER"]);
                        sc.Parameters.Add("p_RADIATOR_SRLNO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["RADIATOR_SRLNO"]) == null ? null : Convert.ToString(dataRow["RADIATOR_SRLNO"]);
                        sc.Parameters.Add("p_FINAL_LABEL_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FINAL_LABEL_DATE"]) == null ? null : Convert.ToString(dataRow["FINAL_LABEL_DATE"]);
                        sc.Parameters.Add("p_REARAXEL", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["REARAXEL"]) == null ? null : Convert.ToString(dataRow["REARAXEL"]);
                        sc.Parameters.Add("p_FRONTTYRE_DESCRIPTION", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["FRONTTYRE_DESCRIPTION"]) == null ? null : Convert.ToString(dataRow["FRONTTYRE_DESCRIPTION"]);
                        sc.Parameters.Add("p_MOBILE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["MOBILE"]) == null ? null : Convert.ToString(dataRow["MOBILE"]);
                        sc.Parameters.Add("p_ENTRYDATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = Convert.ToString(dataRow["ENTRYDATE"]) == null ? null : Convert.ToString(dataRow["ENTRYDATE"]);

                        sc.Parameters.Add("return_message", OracleDbType.NVarchar2, 500);
                        sc.Parameters["return_message"].Direction = ParameterDirection.Output;
                        sc.ExecuteNonQuery();
                        string response = Convert.ToString(sc.Parameters["return_message"].Value);

                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            finally
            {
                funtion.ConClose();
            }
            return result;
        }
        public void HookdownAccordingtoCurrentHook(BeforePaintAssemblyModel beforePaintAssemblyModel, bool isShuffleHook)
        {
            try
            {
                int hookno = Convert.ToInt32(beforePaintAssemblyModel.HOOKNO);
                int previousHook = 0; string JOBID = string.Empty;
                if (!isShuffleHook)
                {
                    previousHook = hookno;
                }
                else
                {
                    previousHook = Convert.ToInt32(hookno) - 22;
                    if (Convert.ToInt32(previousHook) < 1001)
                    {
                        int rem = Convert.ToInt32(hookno) - 1001;
                        int ac = 22 - rem;
                        previousHook = 1093 - ac;
                    }
                }
                if (previousHook != 0)
                {
                    query = string.Format(@"SELECT * FROM (SELECT JOBID FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP' AND HOOK_NO<>'9999' 
                                    and HOOK_NO='{0}'
                                    AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC)a WHERE  ROWNUM=1", previousHook.ToString());
                    JOBID = funtion.get_Col_Value(query);
                }
                if (!string.IsNullOrEmpty(JOBID))
                {
                    if (ValidJobForHookDown(JOBID))
                    {
                        HookUpDown(JOBID, beforePaintAssemblyModel.Plant, beforePaintAssemblyModel.Family, "",
                            "", "", false, true, "", "");
                    }
                }
            }
            catch (Exception ex)
            {

                funtion.WriteLog(ex.Message);
            }
        }
        public bool ValidJobForHookDown(string job)
        {
            bool result = true;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='BP'", job);

                int BPCount = Convert.ToInt32(funtion.CheckExits(query));

                if (BPCount > 0)
                {
                    query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", job);

                    int count = Convert.ToInt32(funtion.CheckExits(query));
                    if (count > 0)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        public bool HookUpDown(string job, string plant, string family, string Fcode, string hook, string fcode_id,
           bool isHookUp, bool isHookDown, string hookupdate, string HType)
        {
            try
            {
                DateTime hookdate = new DateTime();
                string existingHook = string.Empty;
                if (string.IsNullOrEmpty(hookupdate))
                {
                    hookdate = funtion.GetServerDateTime();
                }
                else if (!DateTime.TryParse(hookupdate, out hookdate))
                {

                }
                string query = string.Empty;
                if (string.IsNullOrEmpty(Fcode))
                {
                    query = string.Format(@"select item_code || '#' || fcode_id from xxes_job_status where plant_code='{0}'
                    and family_code='{1}' and jobid='{2}'", plant, family, job);
                    string line = funtion.get_Col_Value(query);
                    if (!string.IsNullOrEmpty(line))
                    {
                        Fcode = line.Split('#')[0].Trim();
                        fcode_id = line.Split('#')[1].Trim();
                    }
                }
                if (isHookUp)
                {
                    if (!funtion.CheckExits("select count(*) from xxes_controllers_data where jobid = '" + job + "' and stage = 'BP' and HOOK_NO = '" + hook + "' and plant_code = '" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE = '" + Convert.ToString(family).Trim().ToUpper() + "'"))
                    {
                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID,HOOKUP_DUMMY_ITEM) values(TO_DATE('" + hookdate.ToString("yyyy/MM/dd HH:mm:ss") + "','yyyy/mm/dd HH24:MI:SS'),'" + plant.Trim().ToUpper() + "','" + family.Trim().ToUpper() + "','" + Fcode.Trim().ToUpper() + "','" + job.Trim() + "','" + hook + "','BP','" + fcode_id.Trim() + "','" + HType + "')";
                        if (funtion.EXEC_QUERY(query))
                        {
                        }
                    }
                }
                existingHook = funtion.get_Col_Value("select hook_no from xxes_controllers_data where jobid='" + job + "' and stage='BP' and plant_code='" + Convert.ToString(plant).Trim() + "' and FAMILY_CODE='" + Convert.ToString(family).Trim().ToUpper() + "'");
                if (!string.IsNullOrEmpty(existingHook))
                {
                    hook = existingHook;
                }

                if (isHookDown)
                {
                    if (!funtion.CheckExits("select count(*) from xxes_controllers_data where jobid = '" + job.Trim() + "' and stage = 'AP' and HOOK_NO = '" + hook + "' and plant_code = '" + plant.Trim() + "' and FAMILY_CODE = '" + family.Trim().ToUpper() + "'"))
                    {
                        query = @"insert into XXES_CONTROLLERS_DATA(ENTRY_DATE,PLANT_CODE,FAMILY_CODE,ITEM_CODE,JOBID,HOOK_NO,STAGE,FCODE_ID,FLAG)
                            values(sysdate,'" + plant + "','" + family + "','" + Fcode + "','" + job + "','" + hook + "','AP','" + fcode_id + "','Y')";
                        if (funtion.EXEC_QUERY(query))
                        {
                            // MessageBox.Show("Hooked UP sucessfully !!", PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    query = "update xxes_controllers_data set REMARKS1='MANNUAL',FLAG='Y' where jobid='" + job + "' and hook_no='" + hook.Trim() + "' and stage='BP'  and plant_code='" + plant.Trim() + "' and FAMILY_CODE='" + family.Trim().ToUpper() + "'";
                    if (funtion.EXEC_QUERY(query))
                    {

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                funtion.WriteLog(ex.Message);
                throw;
            }
            finally
            { }

        }
        public bool CheckIsHook_UP_DOWN(string job)
        {
            bool result = false;
            try
            {
                query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='BP'", job.Trim());

                int BPCount = Convert.ToInt32(funtion.CheckExits(query));

                if (BPCount > 0)
                {

                    query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE jobid='{0}' AND STAGE='AP'", job.Trim());

                    int count = Convert.ToInt32(funtion.CheckExits(query));
                    if (count > 0)
                    {
                        result = false;

                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public string NextHookNo()
        {
            string LastHookNo = "";
            string query = string.Empty;
            string Data = string.Empty;
            string LastDetail = string.Empty;
            try
            {
                query = string.Format(@"SELECT * FROM (SELECT HOOK_NO || '#' || JOBID FROM XXES_CONTROLLERS_DATA WHERE STAGE='BP'
                         AND HOOK_NO<>'9999' AND TRUNC(ENTRY_DATE)>TRUNC(SYSDATE-15) ORDER BY ENTRY_DATE DESC) a WHERE  ROWNUM=1");
                LastDetail = Convert.ToString(funtion.get_Col_Value(query));
                if (!string.IsNullOrEmpty(LastDetail))
                {
                    string hook = LastDetail.Split('#')[0];
                    string job = LastDetail.Split('#')[1];
                    if (hook != "9999")
                    {
                        query = string.Format(@"SELECT COUNT(*) FROM XXES_CONTROLLERS_DATA WHERE STAGE='AP' AND JOBID = '{0}'", job.Trim());
                        int count = Convert.ToInt32(funtion.get_Col_Value(query));
                        if (count == 0)
                        {
                            if (!string.IsNullOrEmpty(hook))
                            {
                                if (hook == "1093")
                                {
                                    LastHookNo = "1001";
                                }
                                else
                                {
                                    LastHookNo = (Convert.ToInt32(hook) + 1).ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
            }
            return LastHookNo;
        }
        public bool DuplicateCheck(string data, string field)
        {
            try
            {
                    query = string.Format("select count(*) from xxes_job_status where {0}='{1}'", field, data);
                    return funtion.CheckExits(query);
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
        }
        public string DuplicateCheck(string data, string field,string jobid)
        {
            string foundjobid = string.Empty;
            try
            {
                query = string.Format("select jobid from xxes_job_status where {0}='{1}' and jobid<>'{2}'", field, data, jobid.Trim());
                foundjobid = funtion.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            return foundjobid;
        }
        public string DuplicateJobCheck(string data, string field,string jobid,string replacejob)
        {
            string foundjobid = string.Empty;
            try
            {
                query = string.Format("select jobid from xxes_job_status where {0}='{1}' and jobid<>'{2}' and jobid<>'{3}'", field, data, jobid.Trim(), replacejob.Trim());
                foundjobid = funtion.get_Col_Value(query);
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
                throw;
            }
            return foundjobid;
        }
        public string getPartDcode(string srno, string stagecode)
        {
            string dcode = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(stagecode))
                    query = string.Format(@"select dcode from XXES_PRINT_SERIALS where srno='{0}'",
                    srno);
                else
                    query = string.Format(@"select dcode from XXES_PRINT_SERIALS where srno='{0}' and offline_keycode='{1}'",
                        srno, stagecode);
                dcode = funtion.get_Col_Value(query);
                if(string.IsNullOrEmpty(dcode))
                {
                    query = string.Format(@"SELECT S.ITEM_CODE FROM PRINT_SERIAL_NUMBER s WHERE S.SERIAL_NUMBER='{0}'",srno.Trim());
                    dcode = funtion.get_Col_Value(query);
                }
            }
            catch (Exception)
            {

                throw;
            }

            return dcode;
        }

        public RollDown GetTractorDetails(string data, string plant, string family, string field)
        {

            RollDown down = new RollDown();
            try
            {

                query = string.Format(@"select j.JOBID,j.ITEM_CODE FCODE ,j.SIM_SERIAL_NO,
                j.ENGINE_SRLNO,j.FCODE_SRLNO ,j.CLUSSTER_SRLNO,j.ALTERNATOR_SRLNO,j.RTRIM1,j.FTRIM1,j.FTRIM2,j.STEERING_MOTOR_SRLNO
                ,j.STARTER_MOTOR_SRLNO,j.HYD_PUMP_SRLNO,j.STERING_CYLINDER_SRLNO,
                j.RADIATOR_SRLNO ,j.STEERING_ASSEMBLY_SRLNO ,j.RTRIM2,REARTYRE_SRLNO1,REARTYRE_SRLNO2,FRONTTYRE_SRLNO1,FRONTTYRE_SRLNO2,
                SIM_SERIAL_NO,IMEI_NO,MOBILE,

                  m.RH_REARTYRE,m.REARTYRE, M.FRONTTYRE,M.RH_FRONTTYRE,M.TRANSMISSION,M.HYDRAULIC,M.REARAXEL REAR_AXLE,M.BATTERY,M.RADIATOR,M.HYD_PUMP,M.STEERING_MOTOR,M.STEERING_ASSEMBLY
                ,M.STERING_CYLINDER,M.ALTERNATOR,M.CLUSSTER,M.STARTER_MOTOR,M.REQ_ROPS,
                 M.REQUIRE_ENGINE,M.REQUIRE_TRANS,M.REQUIRE_REARAXEL,M.REQUIRE_BACKEND,M.REQUIRE_HYD,M.REQUIRE_REARTYRE,M.REQ_RHRT
                ,M.REQUIRE_FRONTTYRE,M.REQUIRE_BATTERY,M.REQ_HYD_PUMP,M.REQ_RADIATOR,M.REQ_RHFT
                ,M.REQ_CLUSSTER,M.REQ_ALTERNATOR,M.REQ_STEERING_ASSEMBLY,M.REQ_STERING_CYLINDER,
                M.REQ_ROPS,M.GEN_SRNO,M.ROPS_ITEM_CODE,M.REQ_STARTER_MOTOR,M.STARTER_MOTOR,M.REQ_STEERING_MOTOR,M.REQ_FRONTRIM,M.REQ_REARRIM                 
                ,m.ITEM_DESCRIPTION DESCRIPTION,HYDRALUIC_SRLNO HYDRAULIC_LIFT,REARTYRE_MAKE TYRE_MAKE,FRONTTYRE_MAKE,  
                j.REARAXEL REAR_AXLE,REARAXEL_SRLNO,j.BACKEND,j.BACKEND_SRLNO,TRANSMISSION_SRLNO,m.ENGINE,BATTERY_MAKE,BATTERY_SRLNO,
                fcode_id,
                REQ_CAREBTN CAREBUTTONREQ,to_char( FINAL_LABEL_DATE, 'dd-Mon-yyyy HH24:MI:SS' )  FINAL_LABEL_DATE,
                to_char( PDIOKDATE, 'dd-Mon-yyyy HH24:MI:SS' ) PDIOKDATE,   
                M.remarks,M.short_code ,to_char( CAREBUTTONOIL, 'dd-Mon-yyyy HH24:MI:SS' )  CAREBUTTONOIL,
                 SWAPCAREBTN,j.HYDRALUIC_DESCRIPTION,j.REARTYRE_DESCRIPTION,j.FRONTTYRE_DESCRIPTION,Prefix_4
                ,j.HYDRALUIC_SRLNO,j.ROPS_SRNO,j.OIL
                from XXES_ITEM_MASTER m join xxes_job_status j on m.plant_code=j.plant_code
                and m.family_code=j.family_code and m.item_code=j.item_code where 
                 j.plant_code='{0}' and j.family_code='{1}' and j.{3}='{2}'",
                   plant.Trim().ToUpper(),
                  family.Trim().ToUpper(), data.Trim(), field.Trim());
                DataTable dt = new DataTable();
                dt = funtion.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    down.PLANTCODE = plant;
                    down.FAMILYCODE = family;
                    down.ROPSrno = Convert.ToString(dt.Rows[0]["ROPS_SRNO"]);
                    down.OilQty = Convert.ToString(dt.Rows[0]["OIL"]);
                    down.Srno = Convert.ToString(dt.Rows[0]["SIM_SERIAL_NO"]);
                    down.ROPS = Convert.ToString(dt.Rows[0]["ROPS_ITEM_CODE"]);
                    down.Carebuttonoildate = Convert.ToString(dt.Rows[0]["CAREBUTTONOIL"]);
                    down.Hydraulic = Convert.ToString(dt.Rows[0]["HYDRAULIC"]);
                    down.hydrualic_desc = Convert.ToString(dt.Rows[0]["HYDRALUIC_DESCRIPTION"]);
                    down.REARTYRE_DESCRIPTION = Convert.ToString(dt.Rows[0]["REARTYRE_DESCRIPTION"]);
                    down.FRONTTYRE_DESCRIPTION = Convert.ToString(dt.Rows[0]["FRONTTYRE_DESCRIPTION"]);
                    down.TractorCode = Convert.ToString(dt.Rows[0]["FCODE"]);
                    down.TractorSrlno = Convert.ToString(dt.Rows[0]["FCODE_SRLNO"]);
                    down.TractorDesc = Convert.ToString(dt.Rows[0]["DESCRIPTION"]);
                    down.Hydraulic_srlno = Convert.ToString(dt.Rows[0]["HYDRALUIC_SRLNO"]);
                    
                    down.RearRIM1= Convert.ToString(dt.Rows[0]["RTRIM1"]);
                    down.RearRIM2= Convert.ToString(dt.Rows[0]["RTRIM2"]);
                    down.FrontRIM1 = Convert.ToString(dt.Rows[0]["FTRIM1"]);
                    down.FrontRIM2 = Convert.ToString(dt.Rows[0]["FTRIM2"]);
                    down.HydrualicPump = Convert.ToString(dt.Rows[0]["HYD_PUMP"]);
                    down.HydrualicPump_srlno = Convert.ToString(dt.Rows[0]["HYD_PUMP_SRLNO"]);
                    down.Radiator = Convert.ToString(dt.Rows[0]["RADIATOR"]);
                    down.Radiator_srlno = Convert.ToString(dt.Rows[0]["RADIATOR_SRLNO"]);
                    down.SteeringCylinder_srlno = Convert.ToString(dt.Rows[0]["STERING_CYLINDER_SRLNO"]);
                    down.SteeringCylinder = Convert.ToString(dt.Rows[0]["STERING_CYLINDER"]);
                    down.SteeringMotor_srlno = Convert.ToString(dt.Rows[0]["STEERING_MOTOR_SRLNO"]);
                    down.SteeringMotor = Convert.ToString(dt.Rows[0]["STEERING_MOTOR"]);
                    down.SteeringAssem_srlno = Convert.ToString(dt.Rows[0]["STEERING_ASSEMBLY_SRLNO"]);
                    down.SteeringAssem = Convert.ToString(dt.Rows[0]["STEERING_ASSEMBLY"]);
                    down.Alternator = Convert.ToString(dt.Rows[0]["ALTERNATOR"]);
                    down.Alternator_srlno = Convert.ToString(dt.Rows[0]["ALTERNATOR_SRLNO"]);
                    down.Cluster = Convert.ToString(dt.Rows[0]["CLUSSTER"]);
                    down.Cluster_srlno = Convert.ToString(dt.Rows[0]["CLUSSTER_SRLNO"]);
                    down.Motor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR"]);
                    down.Motor_srlno = Convert.ToString(dt.Rows[0]["STARTER_MOTOR_SRLNO"]);

                    down.reartyremake = Convert.ToString(dt.Rows[0]["TYRE_MAKE"]);
                    down.fronttyremake = Convert.ToString(dt.Rows[0]["FRONTTYRE_MAKE"]);
                    down.RearAxel = Convert.ToString(dt.Rows[0]["REAR_AXLE"]);
                    down.RearAxel_srlno = Convert.ToString(dt.Rows[0]["REARAXEL_SRLNO"]);
                    down.Transmission = Convert.ToString(dt.Rows[0]["TRANSMISSION"]);
                    down.Transmission_srlno = Convert.ToString(dt.Rows[0]["TRANSMISSION_SRLNO"]);
                    down.Engine = Convert.ToString(dt.Rows[0]["ENGINE"]);
                    down.Engine_srlno = Convert.ToString(dt.Rows[0]["ENGINE_SRLNO"]);
                    down.Backend = Convert.ToString(dt.Rows[0]["backend"]);
                    down.Backend_srlno = Convert.ToString(dt.Rows[0]["BACKEND_SRLNO"]);
                    down.batterymake = Convert.ToString(dt.Rows[0]["BATTERY_MAKE"]);
                    down.Battery_srlno = Convert.ToString(dt.Rows[0]["BATTERY_SRLNO"]);
                    down.Battery = Convert.ToString(dt.Rows[0]["BATTERY"]);
                    down.TractorAutoid = Convert.ToString(dt.Rows[0]["FCODE_ID"]);
                    down.JOBID = Convert.ToString(dt.Rows[0]["JOBID"]);
                    down.simserialno = Convert.ToString(dt.Rows[0]["SIM_SERIAL_NO"]);
                    down.IMEI = Convert.ToString(dt.Rows[0]["IMEI_NO"]);
                    down.MOBILE = Convert.ToString(dt.Rows[0]["MOBILE"]);
                    down.RearTyre1_srlno1 = Convert.ToString(dt.Rows[0]["REARTYRE_SRLNO1"]);
                    down.RearTyre1_dcode = Convert.ToString(dt.Rows[0]["REARTYRE"]);
                    down.RearTyre2_dcode = Convert.ToString(dt.Rows[0]["RH_REARTYRE"]);
                    down.RearTyre2_srlno2 = Convert.ToString(dt.Rows[0]["REARTYRE_SRLNO2"]);
                    down.FrontTyre1_srlno1 = Convert.ToString(dt.Rows[0]["FRONTTYRE_SRLNO1"]);
                    down.FrontTyre2_srlno2 = Convert.ToString(dt.Rows[0]["FRONTTYRE_SRLNO2"]);
                    down.FrontTyre1_Dcode = Convert.ToString(dt.Rows[0]["FRONTTYRE"]);
                    down.FrontTyre2_Dcode = Convert.ToString(dt.Rows[0]["RH_FRONTTYRE"]);
                    down.reqcarebtn = Convert.ToString(dt.Rows[0]["CAREBUTTONREQ"]);
                    down.swapbtn = Convert.ToString(dt.Rows[0]["SWAPCAREBTN"]);
                    down.Rolloutdate = Convert.ToString(dt.Rows[0]["FINAL_LABEL_DATE"]);
                    down.Pdidate = Convert.ToString(dt.Rows[0]["PDIOKDATE"]);
                    down.remarks = Convert.ToString(dt.Rows[0]["remarks"]);
                    down.shortcode = Convert.ToString(dt.Rows[0]["short_code"]);
                    down.isEnableCarebutton = Convert.ToString(dt.Rows[0]["CAREBUTTONREQ"]);
                    down.Carebuttonoildate = Convert.ToString(dt.Rows[0]["CAREBUTTONOIL"]);
                    down.isHydrualicRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_HYD"]) == "Y" ? true : false); 
                    down.isRearAxelRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_REARAXEL"]) == "Y" ? true : false);
                    down.isEngineRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_ENGINE"]) == "Y" ? true : false);
                    down.isTransRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_TRANS"]) == "Y" ? true : false);
                    down.isBackendRequire = (Convert.ToString(dt.Rows[0]["REQUIRE_BACKEND"]) == "Y" ? true : false);

                    down.isREQUIRE_REARTYRE = (Convert.ToString(dt.Rows[0]["REQUIRE_REARTYRE"]) == "Y" ? true : false);
                    down.isREQ_RHRT = (Convert.ToString(dt.Rows[0]["REQ_RHRT"]) == "Y" ? true : false);
                    down.isREQUIRE_FRONTTYRE = (Convert.ToString(dt.Rows[0]["REQUIRE_FRONTTYRE"]) == "Y" ? true : false);
                    down.isREQUIRE_BATTERY = (Convert.ToString(dt.Rows[0]["REQUIRE_BATTERY"]) == "Y" ? true : false);
                    down.isREQ_HYD_PUMP = (Convert.ToString(dt.Rows[0]["REQ_HYD_PUMP"]) == "Y" ? true : false);
                    down.isREQ_RADIATOR = (Convert.ToString(dt.Rows[0]["REQ_RADIATOR"]) == "Y" ? true : false);
                    down.isREQ_RHFT = (Convert.ToString(dt.Rows[0]["REQ_RHFT"]) == "Y" ? true : false);
                    down.isREQ_CLUSSTER = (Convert.ToString(dt.Rows[0]["REQ_CLUSSTER"]) == "Y" ? true : false);
                    down.isREQ_ALTERNATOR = (Convert.ToString(dt.Rows[0]["REQ_ALTERNATOR"]) == "Y" ? true : false);
                    down.isREQ_STEERING_ASSEMBLY = (Convert.ToString(dt.Rows[0]["REQ_STEERING_ASSEMBLY"]) == "Y" ? true : false);
                    down.isREQ_STERING_CYLINDER = (Convert.ToString(dt.Rows[0]["REQ_STERING_CYLINDER"]) == "Y" ? true : false);
                    down.isREQ_ROPS = (Convert.ToString(dt.Rows[0]["REQ_ROPS"]) == "Y" ? true : false);
                    down.isREQ_STARTER_MOTOR = (Convert.ToString(dt.Rows[0]["REQ_STARTER_MOTOR"]) == "Y" ? true : false);
                    down.Motor = Convert.ToString(dt.Rows[0]["STARTER_MOTOR"]);
                    down.isREQ_STEERING_MOTOR = (Convert.ToString(dt.Rows[0]["REQ_STEERING_MOTOR"]) == "Y" ? true : false);
                    down.isREQ_FRONTRIM = (Convert.ToString(dt.Rows[0]["REQ_FRONTRIM"]) == "Y" ? true : false);
                    down.isREQ_REARRIM = (Convert.ToString(dt.Rows[0]["REQ_REARRIM"]) == "Y" ? true : false);
                    down.isSrNoRequire = (Convert.ToString(dt.Rows[0]["GEN_SRNO"]) == "Y" ? true : false);
                    down.TractorType = funtion.get_Col_Value(String.Format(@"select TYPE from xxes_daily_plan_TRAN 
                    where autoid='{0}' and plant_code='{1}' and family_code='{2}'",
                    down.TractorAutoid, down.PLANTCODE, down.FAMILYCODE
                    ));
                    down.PrintMMYYFormat = funtion.get_Col_Value("select ONLINE_SCREEN  from " +
                    "XXES_Stage_Master where plant_code='" + down.PLANTCODE + "' and family_code='" + down.FAMILYCODE + "' and OFFLINE_KEYCODE='" + down.STAGE_Code + "'");
                    down.Prefix_4 = Convert.ToString(dt.Rows[0]["Prefix_4"]);

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return down;
        }
        public string SplitEngineDcode(string barcode, string type)
        {
            FIP data = new FIP();
            string dcode = string.Empty;
            try
            {
                if (type == "Y")
                {
                    query = string.Format(@"SELECT ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO = '{0}'", barcode.Trim().Substring(0, 4).Trim());
                    data.Fipdcode = funtion.get_Col_Value(query);
                    data.SplitSerialno = barcode.Substring(4, 10);
                }
                else
                {
                    query = string.Format(@"SELECT ITEM_CODE FROM XXES_FIPMODEL_CODE WHERE MODEL_CODE_NO = '{0}'", barcode.Trim().Substring(0, 10).Trim());
                    data.Fipdcode = funtion.get_Col_Value(query);
                    data.SplitSerialno = barcode.Substring(10);
                }
                dcode = string.Empty;
            }
            catch (Exception ex)
            {
                funtion.LogWrite(ex);
            }
            dcode = data.Fipdcode + "#" + data.SplitSerialno;
            return dcode;
        }


        public void GetROPSSrno(string plant, string family,  string ROPS_DCODE, out string ROPS_SRNO)
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
                                    , ROPS_DCODE.Trim(), plant.Trim(), family.Trim());

                    DataTable dt = funtion.returnDataTable(Query);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
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
                                throw new Exception("ROPS SERIES FOR FCODE " + ROPS_DCODE + " REACHED ITS MAXIMUM LEVEL FOR PLANT " + plant + " FAMILY " + family);
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
                    funtion.LogWrite(ex);
                    throw;
                }
                finally { }

                ROPS_SRNO = toReturn;
            }
            catch (Exception ex)
            {
                ROPS_SRNO = "";
                funtion.LogWrite(ex);
                throw;
            }
            
        }


        public string makeTyre(string Stage, string Srno)
        {
            string result = string.Empty;
            query = string.Format(@"Select Misc1 From xxes_print_serials Where SRNO='{0}' And offline_keycode='{1}'", Stage.Trim(), Srno.Trim());
            result = funtion.get_Col_Value(query);
            return result;
        }
        public List<DDLTextValue> Battery_Name()
        {
            DataTable dt = null;
            try
            {
                List<DDLTextValue> Unit = new List<DDLTextValue>();
                query = string.Format(@"select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='BATT_MAN_NAME' 
                                        order by PARAMETERINFO");
                dt = funtion.returnDataTable(query);

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
                funtion.LogWrite(ex);
                throw;
                //MessageBox.Show("Module Fill_Unit_Name: " + ex.Message, PubFun.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error); return TmpDs; 
            }
            finally
            {
                funtion.ConClose();
            }
        }
    }
}