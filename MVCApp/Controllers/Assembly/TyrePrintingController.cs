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
        List<LabelPrinting> labelPrinting = null; int qty;
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
            string LHFrontTyre = string.Empty , RHFrontTyre = string.Empty;
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
                query = string.Format(@"SELECT FRONTTYRE || ',' || SUBSTR(FRONTTYRE_DESCRIPTION,0,50) || ',' || RH_FRONTTYRE || ',' || SUBSTR(RH_FRONTTYRE_DESC,0,50)  
                        AS Text FROM XXES_ITEM_MASTER WHERE ITEM_CODE='{0}' AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", data.ItemCode.Trim(), data.Plant.Trim(), data.Family.Trim());
                string line = fun.get_Col_Value(query);
                if(!string.IsNullOrEmpty(line))
                {
                    LHFrontTyre = line.Split(',')[0].Trim().ToUpper();
                    data.FTLH = line.Split(',')[1].Trim().ToUpper() + "(" + LHFrontTyre + ")";
                    RHFrontTyre = line.Split(',')[2].Trim().ToUpper();
                    data.FTRH = line.Split(',')[3].Trim().ToUpper() + "(" + RHFrontTyre + ")";
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

        [HttpGet]
        public JsonResult Print(TyrePrinting data)
        {
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            string stageid = string.Empty;
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
                query = string.Format(@"SELECT STAGE_ID FROM XXES_STAGE_MASTER WHERE PLANT_CODE='{0}' AND FAMILY_CODE='{1}' AND OFFLINE_KEYCODE='FT'
                        ", data.Plant.Trim(), data.Family.Trim());
                stageid = fun.get_Col_Value(query);
                qty = Convert.ToInt32(data.Quantity);
                for(int i = 1; i <= qty; i++)
                {
                    if(data.chkFTRH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo.Split(',')[0].Trim().ToUpper(),
                            description = tyreinfo.Split(',')[1].Trim().ToUpper(),
                            plantcode = Convert.ToString(data.Plant).Trim().ToUpper(),
                            familycode = Convert.ToString(data.Family).Trim().ToUpper()
                        });
                    }
                    if (data.chkFTLH == true)
                    {
                        labelPrinting.Add(new LabelPrinting
                        {
                            dcode = tyreinfo.Split(',')[0].Trim().ToUpper(),
                            description = tyreinfo.Split(',')[1].Trim().ToUpper(),
                            plantcode = Convert.ToString(data.Plant).Trim().ToUpper(),
                            familycode = Convert.ToString(data.Family).Trim().ToUpper()
                        });
                    }
                }
                foreach(LabelPrinting label in labelPrinting)
                {
                    fun.getSeries(data.Plant.Trim(), data.Family.Trim(), stageid);
                    
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

        //public bool PrintSticker(string line, TyrePrinting data)
        //{
        //    bool status = false;
        //    string TyreType = string.Empty, Filename = string.Empty, itemname1 = string.Empty, itemname2 = string.Empty;
        //    string cmd1 = "", cmd2 = "";
        //    PrintAssemblyBarcodes af = new PrintAssemblyBarcodes();
        //    try
        //    {
        //        Filename = "RT.txt";
        //        query = af.ReadFile(Filename);
        //        if (!string.IsNullOrEmpty(query))
        //        {
        //            if (assemblyfunctions == null)
        //                assemblyfunctions = new Assemblyfunctions();
        //            assemblyfunctions.getName(data.description.Trim().ToUpper(), ref itemname1, ref itemname2);
        //            query = query.Replace("SERIES_NO", "");
        //            query = query.Replace("ITEM_NAME1", itemname1);
        //            query = query.Replace("ITEM_NAME2", itemname2);
        //            query = query.Replace("DCODE_VAL", data.ItemCode);
        //            query = query.Replace("MAKE_VAL", data.MakeTyre);
        //            cmd1 = query;
        //            if (PrintTyreLable(cmd1, data))
        //                status = true;
        //            else
        //                status = false;
        //        }
        //        else
        //        {
        //            throw new Exception("Print File Not Found");
        //        }
        //        return status;
        //    }

        //    catch (Exception ex)
        //    {
        //        fun.LogWrite(ex);
        //    }
        //    finally { }
        //}
        public bool PrintTyreLable(string cmd1, TyrePrinting data)
        {
            System.Net.Sockets.TcpClient tc;
            try
            {
                query = string.Format(@"select ipaddr || '#' || ipport from xxes_stage_master where 
                        offline_keycode='FT' and plant_code='{0}' and family_code='{1}'", data.Plant.Trim(), data.Family.Trim());
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
    }
}