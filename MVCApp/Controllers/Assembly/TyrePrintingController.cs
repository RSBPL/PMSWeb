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
    public class TyrePrintingController : Controller
    {
        // GET: TyrePrinting
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "";
        List<LabelPrinting> labelPrinting = new List<LabelPrinting>(); int qty;
        string tyreinfo = string.Empty; Assemblyfunctions assemblyfunctions = null;
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
        public JsonResult BindItemCode(string Plant, string Family)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select ITEM_CODE  || '(' || SUBSTR(ITEM_DESCRIPTION,0,30)  || ')' as DESCRIPTION , ITEM_CODE 
                        from XXES_ITEM_MASTER where  plant_code='{0}' and family_code='{1}' order by Item_code", Plant, Family);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["DESCRIPTION"]),
                                Value = Convert.ToString(dr["ITEM_CODE"])
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult BindTyre()
        {
            List<DDLTextValue> _Tyre = new List<DDLTextValue>();
            try
            {
                DataTable dt = new DataTable();
                query = string.Format(@"select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Tyre.Add(new DDLTextValue
                        {
                            Text =Convert.ToString(dr["Name"]),
                            Value =Convert.ToString(dr["Name"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_Tyre, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetTyreDeCode(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string LHFrontTyre = string.Empty, RHFrontTyre = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.ItemCode))
                {
                    msg = "Please Select Tractor..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT FRONTTYRE || '#' || SUBSTR(FRONTTYRE_DESCRIPTION,0,50) || '#' || RH_FRONTTYRE || '#' || SUBSTR(RH_FRONTTYRE_DESC,0,50)  
                        AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.ItemCode.Trim(), data.Plant.Trim(), data.Family.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    LHFrontTyre = line.Split('#')[0].Trim().ToUpper();
                    data.FTLH = line.Split('#')[1].Trim().ToUpper() + "(" + LHFrontTyre + ")";
                    RHFrontTyre = line.Split('#')[2].Trim().ToUpper();
                    data.FTRH = line.Split('#')[3].Trim().ToUpper() + "(" + RHFrontTyre + ")";
                    tyreinfo = line;
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
        public JsonResult Print(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string stage = "FT";
            string stageid = string.Empty; string tyreinfo1 = string.Empty; string Current_Serial_number = string.Empty; string srnoTobeUpdate = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(data.ItemCode))
                {
                    msg = "Please Select Tractor Fcode..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if(string.IsNullOrEmpty(data.Quantity))
                {
                    msg = "Please Enter Quantity..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT FRONTTYRE || '#' || SUBSTR(FRONTTYRE_DESCRIPTION,0,50) || '#' || RH_FRONTTYRE || '#' || SUBSTR(RH_FRONTTYRE_DESC,0,50)  
                        AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.ItemCode.Trim(), data.Plant.Trim(), data.Family.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    string LHFrontTyre = line.Split('#')[0].Trim().ToUpper();
                    data.FTLH = line.Split('#')[1].Trim().ToUpper() + "(" + LHFrontTyre + ")";
                    string RHFrontTyre = line.Split('#')[2].Trim().ToUpper();
                    data.FTRH = line.Split('#')[3].Trim().ToUpper() + "(" + RHFrontTyre + ")";
                    tyreinfo1 = line;
                }

                query = string.Format(@"SELECT STAGE_ID FROM XXES_STAGE_MASTER WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND OFFLINE_KEYCODE='{2}'
                        ", data.Plant.Trim(), data.Family.Trim(), stage);
                stageid = fun.get_Col_Value(query);
                qty = Convert.ToInt32(data.Quantity);
                for(int i = 1; i <= qty; i++)
                {
                    if(data.chkFTRH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo1.Split('#')[2].Trim().ToUpper(),
                            description = tyreinfo1.Split('#')[3].Trim().ToUpper(),
                            Plant = data.Plant.Trim().ToUpper(),
                            Family = data.Family.Trim().ToUpper()
                        });
                    }
                    if (data.chkFTLH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo1.Split('#')[0].Trim().ToUpper(),
                            description = tyreinfo1.Split('#')[1].Trim().ToUpper(),
                            Plant = data.Plant.Trim().ToUpper(),
                            Family = data.Family.Trim().ToUpper()
                            
                        });
                    }
                }
                foreach(LabelPrinting label in labelPrinting)
                {
                    
                    Current_Serial_number = fun.getSeries(data.Plant.Trim(), data.Family.Trim(), stage);
                    srnoTobeUpdate = Current_Serial_number.Trim().Split('#')[1].Trim();
                    Current_Serial_number = Current_Serial_number.Replace("#", "").Trim();
                    //PrintSticker(data, 1); 
                    PrintSticker(label.dcode,label.description,Current_Serial_number, stage, data);
                    if(Current_Serial_number.Trim() != "")
                    {
                        query = string.Format(@"update XXES_FAMILY_SERIAL set Current_Serial_number='{0}',LAST_PRINTED_LABEL_DATE_TI = SYSDATE where  plant_code='{1}'
                                and family_code='{2}' and offline_keycode='{3}'", srnoTobeUpdate.Trim(), data.Plant.Trim(), data.Family.Trim(), stage);
                        if(fun.EXEC_QUERY(query))
                        {
                            label.srlno = Current_Serial_number;
                        }
                    }
                   
                }
                if(labelPrinting.Count > 0)
                {
                    foreach(LabelPrinting label in labelPrinting)
                    {
                        Insert_Into_Print_Serials(label.srlno, label.dcode,label.description, stageid, stage, data);
                    }
                    msg = "Barcode Printed Sucessfully !!";
                    mstType = "alert-success";
                    status = Validation.str2;
                }
                else
                {
                    msg = "Error found while printing barcode";
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
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        public bool PrintSticker(string dcode, string fcode_desc, string Current_Serial_number, string stage , TyrePrinting data)
        {
            bool status = false;
            string TyreType = string.Empty, Filename = string.Empty, itemname1 = string.Empty, itemname2 = string.Empty;
            //string Current_Serial_number = string.Empty , fcode_desc = string.Empty;
            string tyre = string.Empty;
            PrintAssemblyBarcodes af = new PrintAssemblyBarcodes();
            try
            {
                Filename = "RT.txt";
                query = af.ReadFile(Filename); string cmd1 = "", cmd2 = "";
                if (!string.IsNullOrEmpty(query))
                {
                    if (assemblyfunctions == null)
                        assemblyfunctions = new Assemblyfunctions();
                    assemblyfunctions.getName(fcode_desc, ref itemname1, ref itemname2);
                    query = query.Replace("SERIES_NO", Current_Serial_number);
                    query = query.Replace("ITEM_NAME1", itemname1);
                    query = query.Replace("ITEM_NAME2", itemname2);
                    query = query.Replace("DCODE_VAL", dcode);
                    if(stage == "FT")
                    {
                        query = query.Replace("MAKE_VAL", data.MakeTyre);
                    }                    
                    else if(stage == "RT")
                    {
                        query = query.Replace("MAKE_VAL", data.RMakeTyre);
                    }
                    cmd1 = query;
                    if (PrintTyreLable(cmd1, stage, data))
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
        public bool PrintTyreLable(string cmd1,string stage , TyrePrinting data)
        {
            System.Net.Sockets.TcpClient tc;
            try
            {
                if(stage == "FT")
                {
                    query = string.Format(@"select ipaddr || '#' || ipport from xxes_stage_master where 
                        offline_keycode='{2}' and plant_code='{0}' and family_code='{1}'", data.Plant.Trim(), data.Family.Trim(),stage);
                }
                else if(stage == "RT")
                {
                    query = string.Format(@"select ipaddr || '#' || ipport from xxes_stage_master where 
                        offline_keycode='{2}' and plant_code='{0}' and family_code='{1}'", data.RPlant.Trim(), data.RFamily.Trim(),stage);
                }              
                string line = fun.get_Col_Value(query);
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

        private bool Insert_Into_Print_Serials(string Current_Serial_number, string Dcode, string description, string stageid ,string stage, TyrePrinting data)
        {
            bool status = false;
            try
            {
                if(stage == "FT")
                {
                    query = string.Format(@"select count(*) from XXES_PRINT_SERIALS where PLANT_CODE='{0}' and  FAMILY_CODE='{1}' and STAGE_ID='{2}' and DCODE='{3}' and SRNO='{4}'",
                        data.Plant.Trim(), data.Family.Trim(), stageid, Dcode.Trim(), Current_Serial_number);
                    if (fun.CheckExits(query))
                    {
                        throw new Exception("Serial No already exist..");
                    }
                    else
                    {
                        query = string.Format(@"INSERT INTO XXES_PRINT_SERIALS(PLANT_CODE,FAMILY_CODE,STAGE_ID,DCODE,SRNO,PRINTDATE,OFFLINE_KEYCODE,TYRE_DCODE,RIM_DCODE,MISC1,FCODE,DESCRIPTION)
                            VALUES('{0}','{1}','{2}','{3}','{4}',SYSDATE,'{5}','','','{6}','{7}','{8}')", data.Plant.Trim(), data.Family.Trim(), stageid, Dcode.Trim(),
                                Current_Serial_number, stage, data.MakeTyre.Trim(), data.ItemCode.Trim(), description);
                    }
                }
                else if(stage == "RT")
                {
                    query = string.Format(@"select count(*) from XXES_PRINT_SERIALS where PLANT_CODE='{0}' and  FAMILY_CODE='{1}' and STAGE_ID='{2}' and DCODE='{3}' and SRNO='{4}'",
                        data.RPlant.Trim(), data.RFamily.Trim(), stageid, Dcode.Trim(), Current_Serial_number);
                    if (fun.CheckExits(query))
                    {
                        throw new Exception("Serial No already exist..");
                    }
                    else
                    {
                        query = string.Format(@"INSERT INTO XXES_PRINT_SERIALS(PLANT_CODE,FAMILY_CODE,STAGE_ID,DCODE,SRNO,PRINTDATE,OFFLINE_KEYCODE,TYRE_DCODE,RIM_DCODE,MISC1,FCODE,DESCRIPTION)
                            VALUES('{0}','{1}','{2}','{3}','{4}',SYSDATE,'{5}','','','{6}','{7}')", data.RPlant.Trim(), data.RFamily.Trim(), stageid, Dcode.Trim(),
                                Current_Serial_number, stage, data.RMakeTyre.Trim(), data.RItemCode.Trim(), description);
                    }
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


        /*************************************Rear Tyre********************************************/

        [HttpGet]
        public JsonResult BindRPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindRFamily(string RPlant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(RPlant))
            {
                result = fun.Fill_All_Family(RPlant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindRItemCode(string RPlant, string RFamily)
        {
            List<DDLTextValue> _Item = new List<DDLTextValue>();

            try
            {
                if (!string.IsNullOrEmpty(RPlant) && !string.IsNullOrEmpty(RFamily))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select ITEM_CODE  || '(' || SUBSTR(ITEM_DESCRIPTION,0,30)  || ')' as DESCRIPTION , ITEM_CODE 
                        from XXES_ITEM_MASTER where  plant_code='{0}' and family_code='{1}' order by Item_code", RPlant, RFamily);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _Item.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["DESCRIPTION"]),
                                Value = Convert.ToString(dr["ITEM_CODE"])
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(_Item, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult BindRTyre()
        {
            List<DDLTextValue> _Tyre = new List<DDLTextValue>();
            try
            {

                DataTable dt = new DataTable();
                query = string.Format(@"select PARAMETERINFO as Name from XXES_SFT_SETTINGS where PARAMVALUE='TYRE_MAN_NAME' order by PARAMETERINFO");
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        _Tyre.Add(new DDLTextValue
                        {
                            Text = Convert.ToString(dr["Name"]),
                            Value = Convert.ToString(dr["Name"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_Tyre, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRTyreDeCode(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string LHRearTyre = string.Empty, RHRearTyre = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.RItemCode))
                {
                    msg = "Please Select Tractor..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT REARTYRE || '#' || SUBSTR(REARTYRE_DESCRIPTION,0,50) || '#' || RH_REARTYRE || '#' || SUBSTR(RH_REARTYRE_DESC,0,50)  
                             AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.RItemCode.Trim(), data.RPlant.Trim(), data.RFamily.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    LHRearTyre = line.Split('#')[0].Trim().ToUpper();
                    data.RTLH = line.Split('#')[1].Trim().ToUpper() + "(" + LHRearTyre + ")";
                    RHRearTyre = line.Split('#')[2].Trim().ToUpper();
                    data.RTRH = line.Split('#')[3].Trim().ToUpper() + "(" + RHRearTyre + ")";
                    tyreinfo = line;
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
        public JsonResult RPrint(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string stage = "RT";
            string stageid = string.Empty; string tyreinfo1 = string.Empty; string Current_Serial_number = string.Empty; string srnoTobeUpdate = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.RItemCode))
                {
                    msg = "Please Select Tractor Fcode..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.RQuantity))
                {
                    msg = "Please Enter Quantity..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT REARTYRE || '#' || SUBSTR(REARTYRE_DESCRIPTION,0,50) || '#' || RH_REARTYRE || '#' || SUBSTR(RH_REARTYRE_DESC,0,50)  
                             AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.RItemCode.Trim(), data.RPlant.Trim(), data.RFamily.Trim());
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    string LHRearTyre = line.Split('#')[0].Trim().ToUpper();
                    data.RTLH = line.Split('#')[1].Trim().ToUpper() + "(" + LHRearTyre + ")";
                    string RHRearTyre = line.Split('#')[2].Trim().ToUpper();
                    data.RTRH = line.Split('#')[3].Trim().ToUpper() + "(" + RHRearTyre + ")";
                    tyreinfo1 = line;
                }

                query = string.Format(@"SELECT STAGE_ID FROM XXES_STAGE_MASTER WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND OFFLINE_KEYCODE='{2}'
                        ", data.RPlant.Trim(), data.RFamily.Trim(),stage);
                stageid = fun.get_Col_Value(query);
                qty = Convert.ToInt32(data.RQuantity);
                for (int i = 1; i <= qty; i++)
                {
                    if (data.chkRTRH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo1.Split('#')[2].Trim().ToUpper(),
                            description = tyreinfo1.Split('#')[3].Trim().ToUpper(),
                            Plant = data.RPlant.Trim().ToUpper(),
                            Family = data.RFamily.Trim().ToUpper()
                        });
                    }
                    if (data.chkRTLH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo1.Split('#')[0].Trim().ToUpper(),
                            description = tyreinfo1.Split('#')[1].Trim().ToUpper(),
                            Plant = data.RPlant.Trim().ToUpper(),
                            Family = data.RFamily.Trim().ToUpper()

                        });
                    }
                }
                foreach (LabelPrinting label in labelPrinting)
                {
                    
                    Current_Serial_number = fun.getSeries(data.RPlant.Trim(), data.RFamily.Trim(), stage);
                    srnoTobeUpdate = Current_Serial_number.Trim().Split('#')[1].Trim();
                    Current_Serial_number = Current_Serial_number.Replace("#", "").Trim();
                    //PrintSticker(data, 1); 
                    PrintSticker(label.dcode, label.description, Current_Serial_number, stage, data);
                    if (Current_Serial_number.Trim() != "")
                    {
                        query = string.Format(@"update XXES_FAMILY_SERIAL set Current_Serial_number='{0}',LAST_PRINTED_LABEL_DATE_TI = SYSDATE where  plant_code='{1}'
                                and family_code='{2}' and offline_keycode='{3}'", srnoTobeUpdate.Trim(), data.RPlant.Trim(), data.RFamily.Trim(),stage);
                        if (fun.EXEC_QUERY(query))
                        {
                            label.srlno = Current_Serial_number;
                        }
                    }

                }
                if (labelPrinting.Count > 0)
                {
                    foreach (LabelPrinting label in labelPrinting)
                    {
                        Insert_Into_Print_Serials(label.srlno, label.dcode,label.description, stageid, stage, data);
                    }
                    msg = "Barcode Printed Sucessfully !!";
                    mstType = "alert-success";
                    status = Validation.str2;
                }
                else
                {
                    msg = "Error found while printing barcode";
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
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        /************************************RePrint**************************************/

        [HttpPost]
        public JsonResult BindSrNo(string Plant, string Family, string Date)
        {
            List<DDLTextValue> _SrNo = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(Plant) && !string.IsNullOrEmpty(Family) && !string.IsNullOrEmpty(Date))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select SRNO from XXES_PRINT_SERIALS WHERE plant_code='{0}' and family_code='{1}' and PRINTDATE>='{2}'
                            and OFFLINE_KEYCODE='FT' order by SRNO", Plant, Family, Date);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _SrNo.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["SRNO"]),
                                Value = Convert.ToString(dr["SRNO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_SrNo, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PasswordPopup(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {

                query = string.Format(@"select COUNT(*) from xxes_stage_master where  PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ad_password='{2}' AND OFFLINE_KEYCODE='FT'", 
                        data.Plant.Trim().ToUpper(), data.Family.Trim().ToUpper() , data.Password.Trim());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
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
        [HttpPost]
        public JsonResult Reprint(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string serialno = string.Empty, dcode = string.Empty, desc = string.Empty, make = string.Empty;
            string stage = "FT"; 
            try
            {
                if (string.IsNullOrEmpty(data.Date))
                {
                    msg = "Please Select Date..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.srlno))
                {
                    msg = "Please Select Serial no..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT SRNO ||'#'|| DCODE ||'#'|| DESCRIPTION ||'#'|| MISC1 FROM XXES_PRINT_SERIALS  
                        WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND SRNO='{2}'", data.Plant, data.Family, data.srlno);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    serialno = line.Split('#')[0].Trim().ToUpper();
                    dcode = line.Split('#')[1].Trim().ToUpper();
                    desc = line.Split('#')[2].Trim().ToUpper();
                    data.MakeTyre = line.Split('#')[3].Trim().ToUpper();
                }
                qty = 1;
                for (int i = 1; i <= qty; i++)
                {
                    PrintSticker(dcode, desc, serialno, stage, data);
                   
                }
                msg = "Barcode Printed Sucessfully !!";
                mstType = "alert-success";
                status = Validation.str2;

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
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RPasswordPopup(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            try
            {
           
                    query = string.Format(@"select COUNT(*) from xxes_stage_master where  PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND ad_password='{2}' AND OFFLINE_KEYCODE='RT'",
                           data.RPlant.Trim().ToUpper(), data.RFamily.Trim().ToUpper(), data.RPassword.Trim());
                if (fun.CheckExits(query))
                {
                    msg = "Valid Password";
                    mstType = Validation.str1;
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

        [HttpPost]
        public JsonResult RearReprint(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string serialno = string.Empty, dcode = string.Empty, desc = string.Empty, make = string.Empty;
            string stage = "RT";
            try
            {
                if (string.IsNullOrEmpty(data.RDate))
                {
                    msg = "Please Select Date..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.Rsrlno))
                {
                    msg = "Please Select Serial no..";
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                query = string.Format(@"SELECT SRNO ||'#'|| DCODE ||'#'|| DESCRIPTION ||'#'|| MISC1 FROM XXES_PRINT_SERIALS  
                        WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND SRNO='{2}'", data.RPlant, data.RFamily, data.Rsrlno);
                string line = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(line))
                {
                    serialno = line.Split('#')[0].Trim().ToUpper();
                    dcode = line.Split('#')[1].Trim().ToUpper();
                    desc = line.Split('#')[2].Trim().ToUpper();
                    data.RMakeTyre = line.Split('#')[3].Trim().ToUpper();
                }
                qty = 1;
                for (int i = 1; i <= qty; i++)
                {
                    PrintSticker(dcode, desc, serialno, stage, data);

                }
                msg = "Barcode Printed Sucessfully !!";
                mstType = "alert-success";
                status = Validation.str2;

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
            return Json(myResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BindRSrNo(string RPlant, string RFamily, string RDate)
        {
            List<DDLTextValue> _SrNo = new List<DDLTextValue>();
            try
            {
                if (!string.IsNullOrEmpty(RPlant) && !string.IsNullOrEmpty(RFamily) && !string.IsNullOrEmpty(RDate))
                {
                    DataTable dt = new DataTable();
                    query = string.Format(@"select SRNO from XXES_PRINT_SERIALS WHERE plant_code='{0}' and family_code='{1}' and PRINTDATE>='{2}'
                            and OFFLINE_KEYCODE='RT' order by SRNO", RPlant, RFamily, RDate);
                    dt = fun.returnDataTable(query);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.AsEnumerable())
                        {
                            _SrNo.Add(new DDLTextValue
                            {
                                Text = Convert.ToString(dr["SRNO"]),
                                Value = Convert.ToString(dr["SRNO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            return Json(_SrNo, JsonRequestBehavior.AllowGet);
        }
    }
}