using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Mrn
{
    [Authorize]
    public class StoreMasterController : Controller
    {
        // GET: StoreMaster

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

        public PartialViewResult BindPlantStore()
        {
            ViewBag.PlantStore = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult BindFamilyStore(string PlantStore)
        {
            if (!string.IsNullOrEmpty(PlantStore))
            {
                ViewBag.FamilyStore = new SelectList(fun.Fill_All_Family(PlantStore), "Value", "Text");
            }

            return PartialView();
        }

        public PartialViewResult GridStore(Store store)
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select PLANT_CODE,FAMILY_CODE, STORECODE,STORENAME,REMARKS,CREATED_BY,
                                    TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,
                                    TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE ,AUTOID 
                                    from XXES_STORE_MASTER WHERE plant_code = '{0}' AND family_code = '{1}'",store.PlantStore.Trim().ToUpper(),store.FamilyStore.Trim().ToUpper());
                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSource = dt;
            return PartialView();
        }
        public JsonResult SaveStore(Store store)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(store.PlantStore))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.FamilyStore))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.StoreCode))
                {
                    msg = "Please Enter Store Code..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.StoreName))
                {
                    msg = "Please Enter Store Name..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (Convert.ToInt32(fun.get_Col_Value("select count(*) from XXES_STORE_MASTER where STORECODE='" + store.StoreCode.ToUpper().Trim() + "'")) > 0)
                {
                    msg = "Serial No Already Exist..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if(!string.IsNullOrEmpty(store.Remarks))
                    {
                        store.Remarks = store.Remarks.ToUpper().Trim();
                    }
                        query = string.Format(@"insert into XXES_STORE_MASTER(PLANT_CODE,FAMILY_CODE, STORECODE,STORENAME,REMARKS,CREATED_BY,CREATED_DATE) values('" + store.PlantStore.ToUpper().Trim() + "','" + store.FamilyStore.ToUpper().Trim() + "','" + store.StoreCode.ToUpper().Trim() + "','" + store.StoreName.ToUpper().Trim() + "' ,'" + store.Remarks + "' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE)");

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved successfully...";
                            mstType = "alert-success";
                        }
                    
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateStore(Store store)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(store.PlantStore))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.FamilyStore))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.StoreCode))
                {
                    msg = "Please Enter Store Code..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.StoreName))
                {
                    msg = "Please Enter Store Name..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                {
                    if (!string.IsNullOrEmpty(store.Remarks))
                    {
                        store.Remarks = store.Remarks.ToUpper().Trim();
                    }
                    query = string.Format(@"update XXES_STORE_MASTER SET PLANT_CODE = '" + store.PlantStore.ToUpper().Trim() + "',FAMILY_CODE = '" + store.FamilyStore.ToUpper().Trim() + "', STORECODE = '" + store.StoreCode.ToUpper().Trim() + "', STORENAME = '" + store.StoreName.ToUpper().Trim() + "', REMARKS = '" + store.Remarks + "', UPDATED_BY = '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "', UPDATED_DATE = SYSDATE  where autoid = '" + Convert.ToInt32(store.AutoId.ToUpper().Trim()) + "'");

                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Data Update successfully...";
                        mstType = "alert-success";
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteStore(Store store)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                query = "delete from XXES_STORE_MASTER where AUTOID ='" + store.AutoId + "'";
                if (fun.EXEC_QUERY(query))
                {
                    msg = "Record deleted successfully ..";
                    mstType = "alert-danger";
                    var resul = new { Msg = msg, ID = mstType };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //**********************************************Super Market Location************************************************//


        public PartialViewResult BindPlantSuper()
        {
            ViewBag.PlantSuper = new SelectList(fun.Fill_Unit_Name(), "Value", "Text");
            return PartialView();
        }

        public PartialViewResult BindFamilySuper(string PlantSuper)
        {
            if (!string.IsNullOrEmpty(PlantSuper))
            {
                ViewBag.FamilySuper = new SelectList(fun.Fill_All_Family(PlantSuper), "Value", "Text");
            }

            return PartialView();
        }

        public JsonResult BindSuperMkt()
        {
            return Json(fun.Fill_SuperMkt(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindZone()
        {
            return Json(fun.Fill_ZONE(), JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult GridSuperMarket(Store store)
        {
            DataTable dt = new DataTable();
            try
            {
                query = string.Format(@"select PLANT_CODE,FAMILY_CODE, LOCATION_NAME,REMARKS,CREATED_BY,
                                    TO_CHAR(CREATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  CREATED_DATE,UPDATED_BY,
                                    TO_CHAR(UPDATED_DATE,'DD-MM-YYYY HH24:MI:SS') AS  UPDATED_DATE ,AUTOID from XXES_SUPERMKT_LOCATIONS 
                                    WHERE PLANT_CODE = '{0}' AND FAMILY_CODE = '{1}' ORDER BY CREATED_DATE DESC, LOCATION_NAME ASC", store.PlantSuper.Trim(),store.FamilySuper.Trim());
                dt = fun.returnDataTable(query);
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            ViewBag.DataSourceS = dt;
            return PartialView();
        }

        public JsonResult SaveSuper(Store store)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(store.PlantSuper))
                {
                    msg = "Please Select Plant..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.FamilySuper))
                {
                    msg = "Please Select Family..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(store.FromRange) || string.IsNullOrEmpty(store.ToRange))
                {
                    msg = "Please Enter From & To Location Range..";
                    mstType = "alert-danger";
                    status = "Error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string FromRange = store.FromRange.Trim();
                string ToRange = store.ToRange.Trim();
                
                int j, k;
                if (!int.TryParse(FromRange, out j))
                {

                    msg = "From Range is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (!int.TryParse(ToRange, out k))
                {

                    msg = "To Range is not a valid number ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }

                if (j > k)
                {
                    msg = "From Range should be less than To Range ..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }


                if (!(j >= 1 && j <= 999999 && k >= 1 && k <= 999999))
                {
                    msg = "Valid Range should be 1 - 999999";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                string concatPlantFamily = store.SuperMarket.Trim() + store.ZONE.Trim();
                for (int i = j; i<= k; i++)
                {
                    
                    //int[] digits = i.ToString().Select(t => int.Parse(t.ToString())).ToArray();

                    string a = "";
                    //if (digits.Length == 1)
                    //{
                    //    a = "00000" + i;
                    //}
                    //else if (digits.Length == 2)
                    //{
                    //    a = "0000" + i;
                    //}
                    //else if (digits.Length == 3)
                    //{
                    //    a = "000" + i;
                    //}
                    //else if (digits.Length == 4)
                    //{
                    //    a = "00" + i;
                    //}
                    //else if (digits.Length == 5)
                    //{
                    //    a = "0" + i;
                    //}
                    //else
                    //{
                    //    a = "" + i; 
                    //}
                    a = i.ToString();
                    while(a.Length!=6)
                    {
                        a = "0" + a;
                    }
                    store.LocationName = concatPlantFamily + a.Trim();

                    if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUPERMKT_LOCATIONS WHERE PLANT_CODE = '" + store.PlantSuper.ToUpper().Trim() + "' AND FAMILY_CODE = '" + store.FamilySuper.ToUpper().Trim() + "' AND LOCATION_NAME = '" + store.LocationName.ToUpper().Trim() + "'")) > 0)
                    {
                        msg = store.LocationName + " Location already exist with super marktet and zone..";
                        mstType = "alert-danger";
                        status = "error";
                        var resul = new { Msg = msg, ID = mstType, validation = status };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
 
                }
                for (int i = j; i <= k; i++)
                {
                    //int[] digits = i.ToString().Select(t => int.Parse(t.ToString())).ToArray();

                    string a = "";
                    //if (digits.Length == 1)
                    //{
                    //    a = "00000" + i;
                    //}
                    //else if (digits.Length == 2)
                    //{
                    //    a = "0000" + i;
                    //}
                    //else if (digits.Length == 3)
                    //{
                    //    a = "000" + i;
                    //}
                    //else if (digits.Length == 4)
                    //{
                    //    a = "00" + i;
                    //}
                    //else if (digits.Length == 5)
                    //{
                    //    a = "0" + i;
                    //}
                    //else
                    //{
                    //    a = "" + i;
                    //}
                    a = i.ToString();
                    while (a.Length != 6)
                    {
                        a = "0" + a;
                    }
                    store.LocationName = concatPlantFamily + a.Trim();

                    store.Remarks1 = string.IsNullOrEmpty(store.Remarks1) ? null : store.Remarks1.ToUpper().Trim();

                        query = @"insert into XXES_SUPERMKT_LOCATIONS(PLANT_CODE,FAMILY_CODE,LOCATION_NAME,REMARKS,CREATED_BY,CREATED_DATE,SUPERMARKET,ZONE) 
                                values('" + store.PlantSuper.ToUpper().Trim() + "','" + store.FamilySuper.ToUpper().Trim() + "','" + store.LocationName.ToUpper().Trim() + "','" + store.Remarks1 + "' , '" + HttpContext.Session["Login_User"].ToString().ToUpper().Trim() + "',SYSDATE,'" + store.SuperMarket.ToUpper().Trim() + "','" + store.ZONE.ToUpper().Trim() + "')";

                        if (fun.EXEC_QUERY(query))
                        {
                            msg = "Data Saved successfully...";
                            mstType = "alert-success";
                        }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                msg = ex.Message;
                mstType = "alert-danger";
                status = "error";
                var resul = new { Msg = msg, ID = mstType, validation = status };
                return Json(resul, JsonRequestBehavior.AllowGet);

            }
            finally { }
            var result = new { Msg = msg, ID = mstType, validation = status };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteSuper(Store store)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty;
            try
            {
                if (Convert.ToInt32(fun.get_Col_Value("SELECT COUNT(*) FROM XXES_SUMKTSTORAGEITEMS WHERE PLANT_CODE = '" + store.PlantSuper.ToUpper().Trim() + "' AND FAMILY_CODE = '" + store.FamilySuper.ToUpper().Trim() + "' AND SUMKTLOC = '" + store.LocationName.ToUpper().Trim() + "'")) > 0)
                {
                    msg = store.LocationName + " Location can not delete..";
                    mstType = "alert-danger";
                    status = "error";
                    var resul = new { Msg = msg, ID = mstType, validation = status };
                    return Json(resul, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = "delete from XXES_SUPERMKT_LOCATIONS where AUTOID ='" + store.AutoId + "'";
                    if (fun.EXEC_QUERY(query))
                    {
                        msg = "Record deleted successfully ..";
                        mstType = "alert-danger";
                        var resul = new { Msg = msg, ID = mstType };
                        return Json(resul, JsonRequestBehavior.AllowGet);
                    }
                }

                
            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
            }
            finally { }
            var result = new { Msg = msg, ID = mstType };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}