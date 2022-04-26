using ClosedXML.Excel;
using EncodeDecode;
using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers
{
    [Authorize]
    public class EngineAssemblyController : Controller
    {
        string FIP_ITEM_CODE = string.Empty;
        bool isInjectorRequire = false;
        string ItemCode, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode;
        
        public static string Plant, family, Engine_ItemCode, Engine_SrNo, FuellInjection_Itemcode, FuellInjection_SrNo, CylinderHead_Itemcode, Cylinderhead_SrNo, CylinderBlock_ItemCode, CylinderBlock_SrNo,
        Crankshaft_ItemCode, Cranckshaft_SrNo, Camshaft_Itemcode, Camshaft_SrNo, ConnectionRod_ItemCode, ConnectingRod1_SrNo, ConnectingRod2_SrNo, ConnectingRod3_SrNo, ConnectingRod4_SrNo,
        Injector1_SrNo, Injector2_SrNo, Injector3_SrNo, Injector4_SrNo, query, item;
        
        public static string FUEL_INJECTION_BARCODEDATA, CYLINDER_HEAD_BARCODEDATA, CYLINDER_BLOCK_BARCODEDATA, CRANK_SHAFT_BARCODEDATA, CAM_SHAFT_BARCODEDATA,
            CONNECTINGROD1_BARCODEDATA, CONNECTINGROD2_BARCODEDATA, CONNECTINGROD3_BARCODEDATA, CONNECTINGROD4_BARCODEDATA, INJECTOR1_BARCODRDATA, INJECTOR2_BARCODRDATA, INJECTOR3_BARCODRDATA, INJECTOR4_BARCODRDATA;
        
        Function fun = new Function();
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Account", "Login");
            }
            else
            {
                ViewBag.FromDate = "01-Jan-2016";
                ViewBag.DefaultDate = Convert.ToDateTime(Session["ServerDate"]);
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
                result = fun.Fill_Engine_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult searchEngine(EngineAssembly data)
        {
            string query = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.EngineSrNo))
                {
                    msg = Validation.str30;
                    mstType = Validation.str1;
                    status = Validation.str2;
                    var err = new { Msg = msg, ID = mstType, validation = status };
                    return Json(err, JsonRequestBehavior.AllowGet);
                    
                }
               
                DataTable dt = new DataTable();
                query = string.Format(@"Select ES.ENGINE_SRNO || '(' || EM.ITEM_CODE ||  ')' || ' # ' || EM.ITEM_DESC  as DESCRIPTION,ES.ENGINE_SRNO as ITEM_CODE
                from XXES_ENGINE_MASTER EM Inner join XXES_Engine_status ES on ES.ITEM_CODE=EM.ITEM_CODE 
                where ES.Plant_code='{0}' and ES.Family_code='{1}' AND ES.ENGINE_SRNO LIKE '{2}%' order by ES.ENGINE_SRNO", data.Plant.Trim(), data.Family.Trim(),data.EngineSrNo.Trim().ToUpper());
                
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["DESCRIPTION"].ToString(),
                            Value = dr["ITEM_CODE"].ToString(),
                        }); 
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SearchEngineItemCode(EngineAssembly data)
        {
            
            string Engine_ItemCode = fun.get_Col_Value("select item_code from xxes_engine_status where engine_srno='" + data.EngineSrNo.Trim() + "' and plant_code='" + data.Plant.Trim() + "' and family_code='" + data.Family.Trim() + "'");
            return Json(Engine_ItemCode, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetInjectorPiston(string ItemCode)
        {
            List<EngineAssembly> Item = new List<EngineAssembly>();
            DataTable dt = new DataTable();
            try
            {
                string query = "SELECT NO_OF_PISTONS,INJECTOR FROM XXES_ENGINE_MASTER WHERE ITEM_CODE='" + ItemCode.Trim() + "'";
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {

                        Item.Add(new EngineAssembly
                        {
                            noPistons = dr["NO_OF_PISTONS"].ToString(),
                            injector = dr["INJECTOR"].ToString(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }


            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetBarcode(EngineAssembly data)
        {
            string query = string.Empty;
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;
            List<EngineAssembly> Item = new List<EngineAssembly>();
            try
            {
                //if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family) || string.IsNullOrEmpty(data.EngineSrNo))
                //{
                //    msg = Validation.str30;
                //    mstType = Validation.str1;
                //    status = Validation.str2;
                //    var err = new { Msg = msg, ID = mstType, validation = status };
                //    return Json(err, JsonRequestBehavior.AllowGet);

                //}
                DataTable dt = new DataTable();
                query = "Select EBD.remarks1,EBD.barcode_data,ES.INJECTOR1,ES.INJECTOR2,ES.INJECTOR3,ES.INJECTOR4,EBD.ITEM_CODE,EBD.MARK_MONTH,EBD.MARK_DATE,EBD.MARK_YEAR,EBD.MARK_TIMIE,EBD.VENDOR_CODE,EBD.SRNO,EBD.HEAT_CODE from XXES_engine_barcode_data EBD Inner join XXES_Engine_status ES on ES.ENGINE_SRNO = EBD.ENGINE_SRLNO WHERE ES.ENGINE_SRNO = '" + data.EngineSrNo.Trim() + "' and es.plant_code = '" + data.Plant.Trim() + "' and es.family_code = '" + data.Family.Trim() + "'";
                
                //query = string.Format(@"Select em.INJECTOR,em.REQUIRE_CAM_SHAFT,em.REQ_FUEL_INJECTION_PUMP,em.REQUIRE_CONNECTING_ROD,em.REQUIRE_CRANK_SHAFT,em.REQUIRE_CYLINDER_HEAD,em.REQUIRE_CYLINDER_BLOCK,ebd.ITEM_CODE,ebd.remarks1,em.injector,em.no_of_pistons,EBD.barcode_data,
                //ES.INJECTOR1,ES.INJECTOR2,ES.INJECTOR3,ES.INJECTOR4 
                //from XXES_engine_barcode_data EBD Inner join XXES_Engine_status ES on ES.ENGINE_SRNO=EBD.ENGINE_SRLNO
                //INNER JOIN XXES_ENGINE_MASTER EM on ES.ITEM_CODE=EM.ITEM_CODE
                //WHERE ES.ENGINE_SRNO = '{0}' and es.plant_code='{1}' and es.family_code='{2}'", data.EngineSrNo.Trim().ToUpper(), data.Plant.Trim(), data.Family.Trim());

                dt = fun.returnDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new EngineAssembly
                        {
                            remarks1 = dr["remarks1"].ToString(),
                            barcode = dr["barcode_data"].ToString(),
                            INJ1 = dr["INJECTOR1"].ToString(),
                            INJ2 = dr["INJECTOR2"].ToString(),
                            INJ3 = dr["INJECTOR3"].ToString(),
                            INJ4 = dr["INJECTOR4"].ToString(),
                        });
                        if (Convert.ToString(dr["remarks1"]) == "FuelInjection")
                        {
                            FUEL_INJECTION_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "CylinderHead")
                        {
                            CYLINDER_HEAD_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "CylinderBlock")
                        {
                            CYLINDER_BLOCK_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "Crankshaft")
                        {
                            CRANK_SHAFT_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "Camshaft")
                        {
                            CAM_SHAFT_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "ConnectingRod1")
                        {
                            CONNECTINGROD1_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "ConnectingRod2")
                        {
                            CONNECTINGROD2_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "ConnectingRod3")
                        {
                            CONNECTINGROD3_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        if (Convert.ToString(dr["remarks1"]) == "ConnectingRod4")
                        {
                            CONNECTINGROD4_BARCODEDATA = Convert.ToString(dr["barcode_data"]);
                        }
                        INJECTOR1_BARCODRDATA = Convert.ToString(dr["INJECTOR1"]);

                        INJECTOR2_BARCODRDATA = Convert.ToString(dr["INJECTOR2"]);

                        INJECTOR3_BARCODRDATA = Convert.ToString(dr["INJECTOR3"]);

                        INJECTOR4_BARCODRDATA = Convert.ToString(dr["INJECTOR4"]);
                        
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetBoxes(string ItemCode)
        {
            List<EngineAssembly> Item = new List<EngineAssembly>();
            DataTable dt = new DataTable();
            try
            {
                string query = "SELECT * FROM XXES_ENGINE_MASTER WHERE ITEM_CODE='" + ItemCode.Trim() + "'";
                dt = fun.returnDataTable(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {

                        Item.Add(new EngineAssembly
                        {
                            noPistons = dr["NO_OF_PISTONS"].ToString(),
                            RqrCyBK = dr["REQUIRE_CYLINDER_BLOCK"].ToString(),
                            RqrCyHd = dr["REQUIRE_CYLINDER_HEAD"].ToString(),
                            RqrCrnkSft = dr["REQUIRE_CRANK_SHAFT"].ToString(),
                            RqrConRod = dr["REQUIRE_CONNECTING_ROD"].ToString(),
                            RqrFip = dr["REQ_FUEL_INJECTION_PUMP"].ToString(),
                            RqrCamSft = dr["REQUIRE_CAM_SHAFT"].ToString(),
                            injector = dr["INJECTOR"].ToString(),

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;
            }


            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveAndUpdate(EngineAssembly data)
        {
            List<EngineAssembly> Item = new List<EngineAssembly>();
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty;string errorNo = string.Empty;
            try
            {
                bool check = false;
                // validate Engine
                dynamic output = ValidateEngine(data);
                 
                if(output.Data.Result == false)
                {
                    return output;
                }

                /////////////////////// save update Engine///////////////////////////


                Plant = data.Plant.Trim().ToUpper();
                family = data.Family.Trim().ToUpper();
                Engine_ItemCode = fun.get_Col_Value("select item_code from print_serial_number where serial_number='" + data.EngineSrNo.Trim().ToUpper() + "'");
                Engine_SrNo = data.EngineSrNo.Trim().ToUpper();
                
                if (!string.IsNullOrEmpty(data.FIP) && isInjectorRequire == true)
                {
                    FuellInjection_Itemcode = FIP_ITEM_CODE.Trim().ToUpper();
                    FuellInjection_SrNo = Convert.ToString(data.FIP.Substring(4, 10).Trim().ToUpper());
                    
                }
                else
                {
                    FuellInjection_Itemcode = FIP_ITEM_CODE.Trim().ToUpper();
                    FuellInjection_SrNo = Convert.ToString(data.FIP.Substring(10).Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.cylinderHead))
                {
                    CylinderHead_Itemcode = Convert.ToString(data.cylinderHead.Split('_')[0].Trim().ToUpper());
                    Cylinderhead_SrNo = Convert.ToString(data.cylinderHead.Split('_')[6].Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.cylinderBlock))
                {
                    CylinderBlock_ItemCode = Convert.ToString(data.cylinderBlock.Split('_')[0].Trim().ToUpper());
                    CylinderBlock_SrNo = Convert.ToString(data.cylinderBlock.Split('_')[6].Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.crankShaft))
                {
                    Crankshaft_ItemCode = Convert.ToString(data.crankShaft.Split('_')[0].Trim().ToUpper());
                    Cranckshaft_SrNo = Convert.ToString(data.crankShaft.Split('_')[6].Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.camShaft))
                {
                    Camshaft_Itemcode = Convert.ToString(data.camShaft.Split('_')[0].Trim().ToUpper());
                    Camshaft_SrNo = Convert.ToString(data.camShaft.Split('_')[6].Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.CR1))
                {
                    ConnectionRod_ItemCode = Convert.ToString(data.CR1.Split('_')[0].Trim().ToUpper());
                    ConnectingRod1_SrNo = Convert.ToString(data.CR1.Split('_')[6].Trim().ToUpper());
                }
                if (!string.IsNullOrEmpty(data.CR2))
                    ConnectingRod2_SrNo = Convert.ToString(data.CR2.Split('_')[6].Trim().ToUpper());
                if (!string.IsNullOrEmpty(data.CR3))
                    ConnectingRod3_SrNo = Convert.ToString(data.CR3.Split('_')[6].Trim().ToUpper());
                if (!string.IsNullOrEmpty(data.CR4))
                    ConnectingRod4_SrNo = Convert.ToString(data.CR4.Split('_')[6].Trim().ToUpper());
                Injector1_SrNo = string.IsNullOrEmpty(data.INJ1) == true ? "" : Convert.ToString(data.INJ1).Trim().ToUpper();
                Injector2_SrNo = string.IsNullOrEmpty(data.INJ2) == true ? "" : Convert.ToString(data.INJ2).Trim().ToUpper();
                Injector3_SrNo = string.IsNullOrEmpty(data.INJ3) == true ? "" : Convert.ToString(data.INJ3).Trim().ToUpper();
                Injector4_SrNo = string.IsNullOrEmpty(data.INJ4) == true ? "" : Convert.ToString(data.INJ4).Trim().ToUpper();

                #region Update Engine Status Record
                if (fun.CheckExits("Select count(*) from XXES_ENGINE_STATUS where ENGINE_SRNO='" + Convert.ToString(data.EngineSrNo).Trim().ToUpper() + "'"))
                {
                    check = fun.InsertEngineStatus(Plant, family, Engine_ItemCode, Engine_SrNo, FuellInjection_Itemcode, FuellInjection_SrNo, CylinderHead_Itemcode, Cylinderhead_SrNo,
                    CylinderBlock_ItemCode, CylinderBlock_SrNo, Crankshaft_ItemCode, Cranckshaft_SrNo, Camshaft_Itemcode, Camshaft_SrNo, ConnectionRod_ItemCode, ConnectingRod1_SrNo, ConnectingRod2_SrNo,
                    ConnectingRod3_SrNo, ConnectingRod4_SrNo, Injector1_SrNo, Injector2_SrNo, Injector3_SrNo, Injector4_SrNo, 1);
                    Insert_Audit_BarcodeData(Plant, family,data);
                    if (check == true)
                    {
                        InsertBarcodeData(Plant, family,data);
                        //PrintEngine(Engine_ItemCode, Engine_SrNo, FuellInjection_SrNo, Injector1_SrNo, Injector2_SrNo, Injector3_SrNo, Injector4_SrNo, Plant, family);
                        //BtnExportQR_Click(sender, e);
                        msg = "Engine Update successfully !!!";
                        mstType = Validation.str;
                        errorNo = "-99";
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo};
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }

                }
                #endregion
                #region Insert Engine Status Record
                else
                {

                    bool Check = false;
                    Check = fun.InsertEngineStatus(Plant, family, Engine_ItemCode, Engine_SrNo, FuellInjection_Itemcode, FuellInjection_SrNo, CylinderHead_Itemcode, Cylinderhead_SrNo,
                    CylinderBlock_ItemCode, CylinderBlock_SrNo, Crankshaft_ItemCode, Cranckshaft_SrNo, Camshaft_Itemcode, Camshaft_SrNo, ConnectionRod_ItemCode, ConnectingRod1_SrNo, ConnectingRod2_SrNo,
                    ConnectingRod3_SrNo, ConnectingRod4_SrNo, Injector1_SrNo, Injector2_SrNo, Injector3_SrNo, Injector4_SrNo, 0);
                    if (Check == true)
                    {
                        InsertBarcodeData(Plant, family,data);
                        //PrintEngine(Engine_ItemCode, Engine_SrNo, FuellInjection_SrNo, Injector1_SrNo, Injector2_SrNo, Injector3_SrNo, Injector4_SrNo, Plant, family);
                        msg = "Engine Insert Successfully !!!";
                        mstType = Validation.str;
                        errorNo = "-99";
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.ToString();
                mstType = Validation.str1;
                errorNo = "0";
                var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(err, JsonRequestBehavior.AllowGet);
                
            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        private JsonResult ValidateEngine(EngineAssembly data)
        {
            List<EngineAssembly> Item = new List<EngineAssembly>();
            DataTable dt = new DataTable();
            string msg = string.Empty; string mstType = string.Empty; string status = string.Empty; string errorNo = string.Empty;
            bool result = true;
            try
            {
                // validate Engine
                if (string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str4;
                    mstType = Validation.str1;
                    errorNo = "1";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(data.EngineSrNo))
                {
                    msg = "Select engine serial no to continue. !!!";
                    mstType = Validation.str1;
                    errorNo = "1";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                string item = fun.get_Col_Value("select item_code from print_serial_number where serial_number='" + data.EngineSrNo.Trim().ToUpper() + "'");
                if (string.IsNullOrEmpty(item))
                {
                    msg = "Engine not found. !!!";
                    mstType = Validation.str1;
                    errorNo = "2";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                Engine_SrNo = Convert.ToString(data.EngineSrNo).Trim().ToUpper();

                string CYLINDER_BLOCK, CYLINDER_HEAD, CONNECTING_ROD, CRANK_SHAFT, CAM_SHAFT, FUEL_INJECTION_PUMP;
                CYLINDER_BLOCK = CYLINDER_HEAD = CONNECTING_ROD = CRANK_SHAFT = CAM_SHAFT = FUEL_INJECTION_PUMP = string.Empty;

                bool isREQUIRE_CYLINDER_BLOCK = false, isREQUIRE_CYLINDER_HEAD = false, isREQUIRE_CONNECTING_ROD = false, isREQUIRE_CRANK_SHAFT = false, isREQ_FUEL_INJECTION_PUMP = false, isREQUIRE_CAM_SHAFT = false, Check = false;
                int pistons = 0;
                dt = fun.returnDataTable("select * from XXES_ENGINE_MASTER where item_code='" + item.Trim() + "'");
                if (dt.Rows.Count > 0)
                {
                    isREQUIRE_CYLINDER_BLOCK = (Convert.ToString(dt.Rows[0]["REQUIRE_CYLINDER_BLOCK"]) == "Y" ? true : false);
                    isREQUIRE_CYLINDER_HEAD = (Convert.ToString(dt.Rows[0]["REQUIRE_CYLINDER_HEAD"]) == "Y" ? true : false);
                    isREQUIRE_CRANK_SHAFT = (Convert.ToString(dt.Rows[0]["REQUIRE_CRANK_SHAFT"]) == "Y" ? true : false);
                    isREQUIRE_CONNECTING_ROD = (Convert.ToString(dt.Rows[0]["REQUIRE_CONNECTING_ROD"]) == "Y" ? true : false);
                    isREQ_FUEL_INJECTION_PUMP = (Convert.ToString(dt.Rows[0]["REQ_FUEL_INJECTION_PUMP"]) == "Y" ? true : false);
                    isREQUIRE_CAM_SHAFT = (Convert.ToString(dt.Rows[0]["REQUIRE_CAM_SHAFT"]) == "Y" ? true : false);
                    isInjectorRequire = (Convert.ToString(dt.Rows[0]["INJECTOR"]) == "Y" ? true : false);
                    CYLINDER_BLOCK = Convert.ToString(dt.Rows[0]["CYLINDER_BLOCK"]).Trim().ToUpper();
                    CYLINDER_HEAD = Convert.ToString(dt.Rows[0]["CYLINDER_HEAD"]).Trim().ToUpper();
                    CONNECTING_ROD = Convert.ToString(dt.Rows[0]["CONNECTING_ROD"]).Trim().ToUpper();
                    CRANK_SHAFT = Convert.ToString(dt.Rows[0]["CRANK_SHAFT"]).Trim().ToUpper();
                    CAM_SHAFT = Convert.ToString(dt.Rows[0]["CAM_SHAFT"]).Trim().ToUpper();
                    FUEL_INJECTION_PUMP = Convert.ToString(dt.Rows[0]["FUEL_INJECTION_PUMP"]).Trim().ToUpper();
                    pistons = Convert.ToInt32(dt.Rows[0]["NO_OF_PISTONS"]);
                }
                if (isInjectorRequire == true)
                {
                    FIP_ITEM_CODE = fun.get_Col_Value("Select ITEM_CODE from XXES_FIPMODEL_CODE where MODEL_CODE_NO='" + data.FIP.Trim().Substring(0, 4).Trim() + "'");
                }
                else
                {
                    FIP_ITEM_CODE = fun.get_Col_Value("Select ITEM_CODE from XXES_FIPMODEL_CODE where MODEL_CODE_NO='" + data.FIP.Trim().Substring(0, 10).Trim() + "'");
                }

                if (isInjectorRequire == true)
                {
                    if (!string.IsNullOrEmpty(data.FIP) && isREQ_FUEL_INJECTION_PUMP && !fun.CheckExits("select count(*) from XXES_FIPMODEL_CODE where MODEL_CODE_NO='" + data.FIP.Trim().Substring(0, 4).Trim() + "'"))
                    {
                        msg = "FIP Modal Code Does not exists !!!";
                        mstType = Validation.str1;
                        errorNo = "3";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(data.FIP) && isREQ_FUEL_INJECTION_PUMP && !fun.CheckExits("select count(*) from XXES_FIPMODEL_CODE where MODEL_CODE_NO='" + data.FIP.Trim().Substring(0, 10).Trim() + "'"))
                    {
                        msg = "FIP Modal Code Does not exists !!!";
                        mstType = Validation.str1;
                        errorNo = "3";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }

                if (string.IsNullOrEmpty(data.FIP) && isREQ_FUEL_INJECTION_PUMP)
                {
                    msg = "Please Enter Fuel Injection Pump !!!";
                    mstType = Validation.str1;
                    errorNo = "4";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }

                else if (string.IsNullOrEmpty(data.cylinderHead) && isREQUIRE_CYLINDER_HEAD)
                {
                    msg = "Please Enter Cylinder Head !!!";
                    mstType = Validation.str1;
                    errorNo = "5";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                else if (!string.IsNullOrEmpty(data.cylinderHead) && isREQUIRE_CYLINDER_HEAD && (Check == ValidateMaskValues(data.cylinderHead) ? true : false))
                {
                    msg = "Please Enter Valid Cylinder Head Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "6";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(data.cylinderBlock) && isREQUIRE_CYLINDER_BLOCK)
                {
                    msg = "Please Enter Cylinder Block !!!";
                    mstType = Validation.str1;
                    errorNo = "7";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.cylinderBlock) && isREQUIRE_CYLINDER_BLOCK && (Check == ValidateMaskValues(data.cylinderBlock) ? true : false))
                {
                    msg = "Please Enter Valid Cylinder Block Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "8";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(data.crankShaft) && isREQUIRE_CRANK_SHAFT)
                {
                    msg = "Please Enter Crank Shaft !!!";
                    mstType = Validation.str1;
                    errorNo = "9";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.crankShaft) && isREQUIRE_CRANK_SHAFT && (Check == ValidateMaskValues(data.crankShaft) ? true : false))
                {
                    msg = "Please Enter Valid Crank Shaft Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "10";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(data.camShaft) && isREQUIRE_CAM_SHAFT)
                {
                    msg = "Please Enter Cam Shaft. !!!";
                    mstType = Validation.str1;
                    errorNo = "11";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.camShaft) && isREQUIRE_CAM_SHAFT && (Check == ValidateMaskValues(data.camShaft) ? true : false))
                {
                    msg = "Please Enter Valid Cam Shaft. Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "12";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if ((string.IsNullOrEmpty(data.CR1)) && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Please Enter Connecting Road 1 !!!";
                    mstType = Validation.str1;
                    errorNo = "13";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR1) && isREQUIRE_CONNECTING_ROD && (Check == ValidateMaskValues(data.CR1) ? true : false) && pistons <= 4)
                {
                    msg = "Please Enter Valid Connecting Road 1 Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "14";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if ((string.IsNullOrEmpty(data.CR2)) && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Please Enter Connecting Road 2 !!!";
                    mstType = Validation.str1;
                    errorNo = "15";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                else if (!string.IsNullOrEmpty(data.CR2) && isREQUIRE_CONNECTING_ROD && (Check == ValidateMaskValues(data.CR2) ? true : false) && pistons <= 4)
                {
                    msg = "Please Enter Valid Connecting Road 2 Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "16";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(data.CR3) && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Please Enter Connecting Road 3 !!!";
                    mstType = Validation.str1;
                    errorNo = "17";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR3) && isREQUIRE_CONNECTING_ROD && (Check == ValidateMaskValues(data.CR3) ? true : false) && pistons <= 4)
                {
                    msg = "Please Enter Valid Connecting Road 3 Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "18";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if ((string.IsNullOrEmpty(data.CR4)) && isREQUIRE_CONNECTING_ROD && pistons == 4)
                {
                    msg = "Please Enter Connecting Road 4 !!!";
                    mstType = Validation.str1;
                    errorNo = "19";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR4) && isREQUIRE_CONNECTING_ROD && (Check == ValidateMaskValues(data.CR4) ? true : false) && pistons == 4)
                {
                    msg = "Please Enter Valid Connecting Road 4 Sr.No !!!";
                    mstType = Validation.str1;
                    errorNo = "20";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (data.pnlInjector == true && isInjectorRequire)
                {
                    if (string.IsNullOrEmpty(data.INJ1))
                    {
                        msg = "Please Enter Injector 1 Serial No !!!";
                        mstType = Validation.str1;
                        errorNo = "21";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (fun.CheckExits("select count(*) from XXES_Engine_status  WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND INJECTOR1='" + data.INJ1.Trim().ToUpper() + "'") && isInjectorRequire && !data.INJ1.Equals("._33333_"))
                    {
                        msg = "Injector 1 Serial No Already Used. !!!";
                        mstType = Validation.str1;
                        errorNo = "22";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (string.IsNullOrEmpty(data.INJ2))
                    {
                        msg = "Please Enter Injector 2 Serial No!!!";
                        mstType = Validation.str1;
                        errorNo = "23";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (fun.CheckExits("select count(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND INJECTOR2='" + data.INJ2.Trim().ToUpper() + "'") && isInjectorRequire && !data.INJ2.Equals("._33333_"))
                    {
                        msg = "Injector 2 Serial No Already Used. !!!";
                        mstType = Validation.str1;
                        errorNo = "24";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (string.IsNullOrEmpty(data.INJ3))
                    {
                        msg = "Please Enter Injector 3 Serial No !!!";
                        mstType = Validation.str1;
                        errorNo = "25";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (fun.CheckExits("select count(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND INJECTOR3='" + data.INJ3.Trim().ToUpper() + "'") && isInjectorRequire && !data.INJ3.Equals("._33333_"))
                    {
                        msg = "Injector 3 Serial No Already Used. !!!";
                        mstType = Validation.str1;
                        errorNo = "26";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (string.IsNullOrEmpty(data.INJ4))
                    {
                        msg = "Please Enter Injector 4 Serial No. !!!";
                        mstType = Validation.str1;
                        errorNo = "27";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                    else if (fun.CheckExits("select count(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND INJECTOR4='" + data.INJ4.Trim().ToUpper() + "'") && isInjectorRequire && !data.INJ4.Equals("._33333_"))
                    {
                        msg = "Injector 4 Serial No Already Used. !!!";
                        mstType = Validation.str1;
                        errorNo = "28";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (FUEL_INJECTION_PUMP != FIP_ITEM_CODE && isREQ_FUEL_INJECTION_PUMP)
                {
                    msg = "Invalid Fuel Injection Pump ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "29";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.cylinderBlock) && CYLINDER_BLOCK != data.cylinderBlock.Split('_')[0].Trim().ToUpper() && isREQUIRE_CYLINDER_BLOCK)
                {
                    msg = "Invalid Cylinder Block ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "30";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.cylinderHead) && CYLINDER_HEAD != data.cylinderHead.Split('_')[0].Trim().ToUpper() && isREQUIRE_CYLINDER_HEAD)
                {
                    msg = "Invalid Cylinder HEAD ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "31";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.crankShaft) && CRANK_SHAFT != data.crankShaft.Split('_')[0].Trim().ToUpper() && isREQUIRE_CRANK_SHAFT)
                {
                    msg = "Invalid Crank Shaft ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "32";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.camShaft) && CAM_SHAFT != data.camShaft.Split('_')[0].Trim().ToUpper() && isREQUIRE_CAM_SHAFT)
                {
                    msg = "Invalid Cam Shaft ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "33";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR1) && CONNECTING_ROD != data.CR1.Split('_')[0].Trim().ToUpper() && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Invalid Connecting Rod 1 ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "34";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);

                }
                else if (!string.IsNullOrEmpty(data.CR2) && CONNECTING_ROD != data.CR2.Split('_')[0].Trim().ToUpper() && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Invalid Connecting Rod 2 ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "35";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR3) && CONNECTING_ROD != data.CR3.Split('_')[0].Trim().ToUpper() && isREQUIRE_CONNECTING_ROD && pistons <= 4)
                {
                    msg = "Invalid Connecting Rod 3 ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "36";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                else if (!string.IsNullOrEmpty(data.CR4) && CONNECTING_ROD != data.CR4.Split('_')[0].Trim().ToUpper() && isREQUIRE_CONNECTING_ROD && pistons == 4)
                {
                    msg = "Invalid Connecting Rod 4 ItemCode !!!";
                    mstType = Validation.str1;
                    errorNo = "37";
                    result = false;
                    var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data.FIP) && SplitFIPBarcode(Convert.ToString(data.FIP).Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "'  AND FUEL_INJECTION_PUMP_SRNO='" + SrNo + "'"))
                    {
                        msg = "Fuel Injection Pump SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "38";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.cylinderHead) && Split2DBarcode(data.cylinderHead.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CYLINDER_HEAD_SRNO='" + SrNo + "'"))
                    {
                        msg = "Cylinder head SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "39";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.cylinderBlock) && Split2DBarcode(data.cylinderBlock.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CYLINDER_BLOCK_SRNO='" + SrNo + "'"))
                    {
                        msg = "Cylinder Block SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "40";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.crankShaft) && Split2DBarcode(data.crankShaft.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CRANKSHAFT_SRNO='" + SrNo + "'"))
                    {
                        msg = "Crank Shaft SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "41";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.camShaft) && Split2DBarcode(data.camShaft.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CAMSHAFT_SRNO='" + SrNo + "'"))
                    {
                        msg = "Cam Shaft SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "42";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.CR1) && Split2DBarcode(data.CR1.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND  CONNECTING_ROD_SRNO1='" + SrNo + "'"))
                    {
                        msg = "Connecting Road 1 SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "43";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.CR2) && Split2DBarcode(data.CR2.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "'  AND CONNECTING_ROD_SRNO2='" + SrNo + "'"))
                    {
                        msg = "Connecting Road 2 SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "44";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.CR3) && Split2DBarcode(data.CR3.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CONNECTING_ROD_SRNO3='" + SrNo + "'"))
                    {
                        msg = "Connecting Road 3 SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "45";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if (!string.IsNullOrEmpty(data.CR4) && Split2DBarcode(data.CR4.Trim()))
                {
                    if (fun.CheckExits("SELECT COUNT(*) from XXES_Engine_status WHERE ENGINE_SRNO<>'" + Engine_SrNo + "' AND CONNECTING_ROD_SRNO4='" + SrNo + "'"))
                    {
                        msg = "Connecting Road 4 SrNo Already Used !!!";
                        mstType = Validation.str1;
                        errorNo = "46";
                        result = false;
                        var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                        return Json(err, JsonRequestBehavior.AllowGet);
                    }
                }
                if(result)
                {
                    //msg = "Connecting Road 4 SrNo Already Used !!!";
                    // = Validation.str1;
                    //errorNo = "46";
                    //result = false;
                    var err = new { Result = result };
                    return Json(err, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.ToString();
                mstType = Validation.str1;
                errorNo = "0";
                result = false;
                var err = new { Msg = msg, ID = mstType, ErrorNo = errorNo, Result = result };
                return Json(err, JsonRequestBehavior.AllowGet);

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }
        private bool ValidateMaskValues(string data)
        {
            bool flag = true;
            try
            {

                if (string.IsNullOrEmpty(data.Split('_')[0]) && data.Split('_')[0].Length != 9)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[1]) || data.Split('_')[1].Length != 1)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[2]) || data.Split('_')[2].Length != 1)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[3]) && data.Split('_')[3].Length != 2)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[4]) && data.Split('_')[4].Length != 1)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[5]) && data.Split('_')[5].Length != 5)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[6]) && data.Split('_')[6].Length != 4)
                    flag = false;
                else if (string.IsNullOrEmpty(data.Split('_')[7]) && data.Split('_')[7].Length != 4)
                    flag = false;
                return flag;
            }
            catch(Exception ex) {
                fun.LogWrite(ex);
                throw;
            }
            finally { }
        }
        private bool SplitFIPBarcode(string barcode)
        {
            try
            {
                if (!string.IsNullOrEmpty(barcode))
                {
                    if (isInjectorRequire == true)
                    {
                        ItemCode = FIP_ITEM_CODE.Trim();
                        SrNo = barcode.Substring(4, 10).Trim();
                        return true;
                    }
                    else
                    {
                        ItemCode = FIP_ITEM_CODE.Trim();
                        SrNo = barcode.Substring(10).Trim();
                        return true;
                    }
                }
                else return false;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return false;
            }
        }
        private bool Split2DBarcode(string barcode)
        {
            try
            {
                if (!string.IsNullOrEmpty(barcode))
                {
                    string[] barcodeData = barcode.Split('_');
                    ItemCode = barcodeData[0].Trim();
                    MarkMonth = barcodeData[1].Trim();
                    MarkDate = barcodeData[2].Trim();
                    MarkYear = barcodeData[3].Trim();
                    MarkTime = barcodeData[4].Trim();
                    VendorCode = barcodeData[5].Trim();
                    SrNo = barcodeData[6].Trim();
                    HeatCode = barcodeData[7].Trim();
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                return false;
            }
        }
        private void Insert_Audit_BarcodeData(string plant, string family,EngineAssembly data)
        {
            try
            {

                if (FUEL_INJECTION_BARCODEDATA != data.FIP && (!string.IsNullOrEmpty(FUEL_INJECTION_BARCODEDATA)) && isInjectorRequire == false)
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, FIP_ITEM_CODE, FUEL_INJECTION_BARCODEDATA.Substring(10).Trim(), "FuelInjection", FUEL_INJECTION_BARCODEDATA.Trim(), "", "", "", "");
                }
                else
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, FIP_ITEM_CODE, FUEL_INJECTION_BARCODEDATA.Substring(4, 10).Trim(), "FuelInjection", FUEL_INJECTION_BARCODEDATA.Trim(), "", "", "", "");
                }
                if (CYLINDER_HEAD_BARCODEDATA != data.cylinderHead && (!string.IsNullOrEmpty(CYLINDER_HEAD_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CYLINDER_HEAD_BARCODEDATA.Split('_')[0].Trim(), CYLINDER_HEAD_BARCODEDATA.Split('_')[6].Trim(), "CylinderHead", CYLINDER_HEAD_BARCODEDATA.Trim(), "", "", "", "");
                }
                if (CYLINDER_BLOCK_BARCODEDATA != data.cylinderBlock && (!string.IsNullOrEmpty(CYLINDER_BLOCK_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CYLINDER_BLOCK_BARCODEDATA.Split('_')[0].Trim(), CYLINDER_BLOCK_BARCODEDATA.Split('_')[6].Trim(), "CylinderBlock", CYLINDER_BLOCK_BARCODEDATA, "", "", "", "");
                }
                if (CRANK_SHAFT_BARCODEDATA != data.crankShaft && (!string.IsNullOrEmpty(CRANK_SHAFT_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CRANK_SHAFT_BARCODEDATA.Split('_')[0].Trim(), CRANK_SHAFT_BARCODEDATA.Split('_')[6].Trim(), "Crankshaft", CRANK_SHAFT_BARCODEDATA, "", "", "", "");
                }
                if (CAM_SHAFT_BARCODEDATA != data.camShaft && (!string.IsNullOrEmpty(CAM_SHAFT_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CAM_SHAFT_BARCODEDATA.Split('_')[0].Trim(), CAM_SHAFT_BARCODEDATA.Split('_')[6].Trim(), "Camshaft", CAM_SHAFT_BARCODEDATA, "", "", "", "");
                }
                if (CONNECTINGROD1_BARCODEDATA != data.CR1 && (!string.IsNullOrEmpty(CONNECTINGROD1_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CONNECTINGROD1_BARCODEDATA.Split('_')[0].Trim(), CONNECTINGROD1_BARCODEDATA.Split('_')[6].Trim(), "ConnectingRod1", CONNECTINGROD1_BARCODEDATA, "", "", "", "");
                }
                if (CONNECTINGROD2_BARCODEDATA != data.CR2 && (!string.IsNullOrEmpty(CONNECTINGROD2_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CONNECTINGROD2_BARCODEDATA.Split('_')[0].Trim(), CONNECTINGROD2_BARCODEDATA.Split('_')[6].Trim(), "ConnectingRod2", CONNECTINGROD2_BARCODEDATA, "", "", "", "");
                }
                if (CONNECTINGROD3_BARCODEDATA != data.CR3 && (!string.IsNullOrEmpty(CONNECTINGROD3_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CONNECTINGROD3_BARCODEDATA.Split('_')[0].Trim(), CONNECTINGROD3_BARCODEDATA.Split('_')[6].Trim(), "ConnectingRod3", CONNECTINGROD3_BARCODEDATA, "", "", "", "");
                }
                if (CONNECTINGROD4_BARCODEDATA != data.CR4 && (!string.IsNullOrEmpty(CONNECTINGROD4_BARCODEDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, CONNECTINGROD4_BARCODEDATA.Split('_')[0].Trim(), CONNECTINGROD4_BARCODEDATA.Split('_')[6].Trim(), "ConnectingRod4", CONNECTINGROD4_BARCODEDATA, "", "", "", "");
                }
                if (INJECTOR1_BARCODRDATA != data.INJ1 && (!string.IsNullOrEmpty(INJECTOR1_BARCODRDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, "", "", "Injector1", INJECTOR1_BARCODRDATA, "", "", "", "");
                }
                if (INJECTOR2_BARCODRDATA != data.INJ2 && (!string.IsNullOrEmpty(INJECTOR2_BARCODRDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, "", "", "Injector2", INJECTOR2_BARCODRDATA, "", "", "", "");
                }
                if (INJECTOR3_BARCODRDATA != data.INJ3 && (!string.IsNullOrEmpty(INJECTOR3_BARCODRDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, "", "", "Injector3", INJECTOR3_BARCODRDATA, "", "", "", "");
                }
                if (INJECTOR4_BARCODRDATA != data.INJ4 && (!string.IsNullOrEmpty(INJECTOR4_BARCODRDATA)))
                {
                    fun.Insert_Part_Audit_Data(plant, family, Engine_ItemCode, Engine_SrNo, "", "", "Injector4", INJECTOR4_BARCODRDATA, "", "", "", "");
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                
            }
            finally { }

        }
        private void InsertBarcodeData(string Plant, string family,EngineAssembly data)
        {
            try
            {
                if (!string.IsNullOrEmpty(data.cylinderBlock) && Split2DBarcode(Convert.ToString(data.cylinderBlock).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='CylinderBlock'"))
                    {
                        string Remarks1 = "CylinderBlock";
                        string Barcodedata = Convert.ToString(data.cylinderBlock);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='CylinderBlock'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.cylinderHead) && Split2DBarcode(Convert.ToString(data.cylinderHead).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='CylinderHead'"))
                    {
                        string Remarks1 = "CylinderHead";
                        string Barcodedata = Convert.ToString(data.cylinderHead);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='CylinderHead'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.crankShaft) && Split2DBarcode(Convert.ToString(data.crankShaft).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='Crankshaft'"))
                    {
                        string Remarks1 = "Crankshaft";
                        string Barcodedata = Convert.ToString(data.crankShaft);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='Crankshaft'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.camShaft) && Split2DBarcode(Convert.ToString(data.camShaft).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='Camshaft'"))
                    {
                        string Remarks1 = "Camshaft";
                        string Barcodedata = Convert.ToString(data.camShaft);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='Camshaft'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.FIP) && SplitFIPBarcode(Convert.ToString(data.FIP).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='FuelInjection'"))
                    {
                        string Remarks1 = "FuelInjection";
                        string Barcodedata = Convert.ToString(data.FIP);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='FuelInjection'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.CR1) && Split2DBarcode(Convert.ToString(data.CR1).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='ConnectingRod1'"))
                    {
                        string Remarks1 = "ConnectingRod1";
                        string Barcodedata = Convert.ToString(data.CR1);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='ConnectingRod1'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.CR2) && Split2DBarcode(Convert.ToString(data.CR2).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='ConnectingRod2'"))
                    {
                        string Remarks1 = "ConnectingRod2";
                        string Barcodedata = Convert.ToString(data.CR2);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='ConnectingRod2'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.CR3) && Split2DBarcode(Convert.ToString(data.CR3).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='ConnectingRod3'"))
                    {
                        string Remarks1 = "ConnectingRod3";
                        string Barcodedata = Convert.ToString(data.CR3);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='ConnectingRod3'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }
                if (!string.IsNullOrEmpty(data.CR4) && Split2DBarcode(Convert.ToString(data.CR4).Trim()))
                {
                    if (!fun.CheckExits("Select Count(*) from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND SRNO='" + SrNo + "' AND REMARKS1='ConnectingRod4'"))
                    {
                        string Remarks1 = "ConnectingRod4";
                        string Barcodedata = Convert.ToString(data.CR4);
                        fun.EXEC_QUERY("Delete from XXES_ENGINE_BARCODE_DATA where ENGINE_SRLNO='" + Engine_SrNo + "' AND ITEM_CODE='" + ItemCode + "' AND REMARKS1='ConnectingRod4'");
                        fun.InsertBarcodeData(Plant, family, ItemCode, Engine_SrNo, MarkMonth, MarkDate, MarkYear, MarkTime, VendorCode, SrNo, HeatCode, Remarks1, Barcodedata);
                        Clear2DBarcode();
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
        }
        public void Clear2DBarcode()
        {
            ItemCode = string.Empty;
            MarkMonth = string.Empty;
            MarkDate = string.Empty;
            MarkYear = string.Empty;
            MarkTime = string.Empty;
            VendorCode = string.Empty;
            SrNo = string.Empty;
            HeatCode = string.Empty;
        }

        public JsonResult CheckPassword(EngineAssembly data)
        {
            string msg = string.Empty; string mstType = string.Empty; string UserIpAdd = string.Empty;
            string errorNo = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
                else if (string.IsNullOrEmpty(data.Password))
                {
                    msg = "Enter admin password to continue !!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                    
                }
                if (!fun.CheckExits("select count(*) from xxes_stage_master where ad_password='" + data.Password.Trim() + "' and offline_keycode='ST3' and family_code='" + data.Family.Trim() + "' and plant_code='" + data.Plant.Trim() + "'"))
                {
                    msg = "Invalid Stage Admin Password. !!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "";
                    mstType = Validation.str;
                    errorNo = "0";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                mstType = "alert-danger";
                errorNo = "1";
                var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ExportEngineCategory(EngineAssembly data)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string UserIpAdd = string.Empty; string errorNo = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
                else if(string.IsNullOrEmpty(data.FrmDate) || string.IsNullOrEmpty(data.ToDate))
                {
                    msg = "From & To Date is required..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                dt = BindEngineAssembly(data);
                if (dt.Rows.Count > 0)
                {
                    dt.Namespace = data.cbEngineCategory;
                    dt.TableName = data.cbEngineCategory;
                    string filename = data.cbEngineCategory + DateTime.Now.ToString("ddMMyyyy");
                    //data.ImportExcel = filename;
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add(dt);
                    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                    ws.Range("A1:X1").Style.Font.Bold = true;
                    //ws.Range("C1:D1").Style.Font.Bold = true;
                    ws.Columns().AdjustToContents();

                    string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }

                    wb.SaveAs(FilePath);
                    msg = "File Exported Successfully ...";
                    mstType = Validation.str;
                    //excelName = data.ImportExcel;
                    var resul = new { Msg = msg, ID = mstType, ExcelName = filename };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "No Record Found..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = "alert-danger";
                errorNo = "1";
                var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public DataTable BindEngineAssembly(EngineAssembly data)
        {

            if (data.cbEngineCategory == "All")
                query = "SELECT ENGINE_SRNO,ES.ITEM_CODE,ES.CYLINDER_BLOCK,CYLINDER_BLOCK_SRNO,ES.CYLINDER_HEAD,CYLINDER_HEAD_SRNO,ES.FUEL_INJECTION_PUMP,FUEL_INJECTION_PUMP_SRNO,ES.CRANKSHAFT,CRANKSHAFT_SRNO,ES.CAMSHAFT,CAMSHAFT_SRNO,ES.CONNECTING_ROD,CONNECTING_ROD_SRNO1,CONNECTING_ROD_SRNO2,CONNECTING_ROD_SRNO3,CONNECTING_ROD_SRNO4,INJECTOR1,INJECTOR2,INJECTOR3,INJECTOR4,to_char( ENTRYDATE, 'dd-Mon-yyyy' ) SCAN_DATE,to_char( ENTRYDATE, 'HH24:MI:SS' ) SCAN_TIME,ES.remarks2 MONTH_VALUE from XXES_Engine_status ES,xxes_engine_master M where ES.ITEM_CODE=M.ITEM_CODE and ES.Plant_code='" + data.Plant + "' and ES.Family_code='" + data.Family + "' and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') order by ES.ENGINE_SRNO";
            else if (data.cbEngineCategory == "CRDI")
                query = "SELECT ENGINE_SRNO,ES.ITEM_CODE,ES.CYLINDER_BLOCK,CYLINDER_BLOCK_SRNO,ES.CYLINDER_HEAD,CYLINDER_HEAD_SRNO,ES.FUEL_INJECTION_PUMP,FUEL_INJECTION_PUMP_SRNO,ES.CRANKSHAFT,CRANKSHAFT_SRNO,ES.CAMSHAFT,CAMSHAFT_SRNO,ES.CONNECTING_ROD,CONNECTING_ROD_SRNO1,CONNECTING_ROD_SRNO2,CONNECTING_ROD_SRNO3,CONNECTING_ROD_SRNO4,INJECTOR1,INJECTOR2,INJECTOR3,INJECTOR4,to_char( ENTRYDATE, 'dd-Mon-yyyy' ) SCAN_DATE,to_char( ENTRYDATE, 'HH24:MI:SS' ) SCAN_TIME ,ES.remarks2 MONTH_VALUE from XXES_Engine_status ES,xxes_engine_master M where ES.ITEM_CODE=M.ITEM_CODE and ES.Plant_code='" + data.Plant + "' and ES.Family_code='" + data.Family + "'  and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and INJECTOR='Y' order by ES.ENGINE_SRNO";
            else if (data.cbEngineCategory == "WithoutCRDI")
                query = "SELECT ENGINE_SRNO,ES.ITEM_CODE,ES.CYLINDER_BLOCK,CYLINDER_BLOCK_SRNO,ES.CYLINDER_HEAD,CYLINDER_HEAD_SRNO,ES.FUEL_INJECTION_PUMP,FUEL_INJECTION_PUMP_SRNO,ES.CRANKSHAFT,CRANKSHAFT_SRNO,ES.CAMSHAFT,CAMSHAFT_SRNO,ES.CONNECTING_ROD,CONNECTING_ROD_SRNO1,CONNECTING_ROD_SRNO2,CONNECTING_ROD_SRNO3,CONNECTING_ROD_SRNO4,INJECTOR1,INJECTOR2,INJECTOR3,INJECTOR4,to_char( ENTRYDATE, 'dd-Mon-yyyy' ) SCAN_DATE,to_char( ENTRYDATE, 'HH24:MI:SS' ) SCAN_TIME,ES.remarks2 MONTH_VALUE  from XXES_Engine_status ES ,xxes_engine_master M where ES.ITEM_CODE=M.ITEM_CODE and ES.Plant_code='" + data.Plant + "' and ES.Family_code='" + data.Family + "'  and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and NVL(INJECTOR,'N')='N' order by ES.ENGINE_SRNO";

            return fun.returnDataTable(query);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            string FilePath = Server.MapPath("~/TempExcelFile/" + file);
            return File(FilePath, "application/vnd.ms-excel", file);
        }

        public JsonResult DownloadBosh(EngineAssembly data)
        {
            string msg = string.Empty; string excelName = string.Empty; string mstType = string.Empty;
            string UserIpAdd = string.Empty; string errorNo = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(data.Plant) || string.IsNullOrEmpty(data.Family))
                {
                    msg = Validation.str30;
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);

                }
                else if (string.IsNullOrEmpty(data.FrmDate) || string.IsNullOrEmpty(data.ToDate))
                {
                    msg = "From & To Date is required..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                dt = BindBocsh(data);
                if (dt.Rows.Count > 0)
                {


                    dt.Namespace = "DATA_STORED_IN_EOL_FOR_CRDI";
                    dt.TableName = "DATA_STORED_IN_EOL_FOR_CRDI";
                    string filename = "BOSCH_ECU_FLOW" + DateTime.Now.ToString("ddMMyyyy");
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add(dt);
                    ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                    ws.Tables.FirstOrDefault().Theme = XLTableTheme.None;
                    ws.Range("A1:K1").Style.Font.Bold = true;
                    
                    ws.Columns().AdjustToContents();

                    string FilePath = Server.MapPath("~/TempExcelFile/" + filename + ".xlsx");
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }

                    wb.SaveAs(FilePath);
                    msg = "File Exported Successfully ...";
                    mstType = Validation.str;
                    //excelName = data.ImportExcel;
                    var resul = new { Msg = msg, ID = mstType, ExcelName = filename };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    msg = "No Record Found..!!!";
                    mstType = "alert-danger";
                    errorNo = "1";
                    var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = "alert-danger";
                errorNo = "1";
                var resul = new { Msg = msg, ID = mstType, ErrorNo = errorNo };
                return Json(resul, JsonRequestBehavior.AllowGet);
            }
            var result = new { Msg = msg, ID = mstType, ExcelName = excelName };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public DataTable BindBocsh(EngineAssembly data)
        {
            if (data.cbEngineCategory == "All")
                query = "select s.engine_srno as \"E code\",s.item_code as \"Dcode\",substr(remarks2,0,1) as \"Month\",substr(remarks2,3,1) as DATE1,substr(remarks2,5,2) as \"Year\",s.fuel_injection_pump_srno as \"FIP no\",s.INJECTOR1 as \"Injector 1\",s.INJECTOR2 as \"Injector 2\",s.INJECTOR3 as \"Injector 3\",s.INJECTOR4 as \"Injector 4\",m.ecu from xxes_engine_status s , xxes_engine_master m where s.item_code=m.item_code and s.plant_code='" + data.Plant + "' and s.family_code='" + data.Family + "' and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') order by s.ENGINE_SRNO";
            else if (data.cbEngineCategory == "CRDI")
                query = "select s.engine_srno as \"E code\",s.item_code as \"Dcode\",substr(remarks2,0,1) as \"Month\",substr(remarks2,3,1) as DATE1,substr(remarks2,5,2) as \"Year\",s.fuel_injection_pump_srno as \"FIP no\",s.INJECTOR1 as \"Injector 1\",s.INJECTOR2 as \"Injector 2\",s.INJECTOR3 as \"Injector 3\",s.INJECTOR4 as \"Injector 4\",m.ecu from xxes_engine_status s , xxes_engine_master m where s.item_code=m.item_code and s.plant_code='" + data.Plant + "' and s.family_code='" + data.Family + "' and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and injector='Y' order by s.ENGINE_SRNO";
            else if (data.cbEngineCategory == "WithoutCRDI")
                query = "select s.engine_srno as \"E code\",s.item_code as \"Dcode\",substr(remarks2,0,1) as \"Month\",substr(remarks2,3,1) as DATE1,substr(remarks2,5,2) as \"Year\",s.fuel_injection_pump_srno as \"FIP no\",m.ecu from xxes_engine_status s , xxes_engine_master m where s.item_code=m.item_code and s.plant_code='" + data.Plant + "' and s.family_code='" + data.Family + "' and to_char(ENTRYDATE,'dd-Mon-yyyy')>=to_date('" + Convert.ToDateTime(data.FrmDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and to_char(ENTRYDATE,'dd-Mon-yyyy')<=to_date('" + Convert.ToDateTime(data.ToDate).ToString("dd-MMM-yyyy") + "','dd-Mon-yyyy') and nvl(injector,'N')='N' order by s.ENGINE_SRNO";

            return fun.returnDataTable(query);
        }

    }
}