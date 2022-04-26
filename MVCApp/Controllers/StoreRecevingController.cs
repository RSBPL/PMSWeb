using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using Syncfusion.EJ2.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace MVCApp.Controllers
{
    [Authorize]
    public class StoreRecevingController : Controller
    {
        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt = new DataTable();
        Function fun = new Function();

        string query = ""; string ORGID = "";

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

        public ActionResult Grid(GoodsRecivingatStoreModel obj)
        {
            //123456789,200675,V1001

            string MRN = string.Empty, Invoice = string.Empty, VehicleNo = string.Empty, Msg = string.Empty;
            string unit = "T04";
            try
            {
                if (string.IsNullOrEmpty(obj.MRN_QR))
                {
                    Msg = "Error ! Invalid Scan..";
                }
                else
                {
                    int count = obj.MRN_QR.Count(f => f == ',');
                    if (count != 2)
                    {
                        Msg = "Error ! Invalid QR Code..";
                    }
                    else
                    {
                        MRN = obj.MRN_QR.Split(',')[0].Trim();
                        Invoice = obj.MRN_QR.Split(',')[1].Trim();
                        VehicleNo = obj.MRN_QR.Split(',')[2].Trim();
                        //query = string.Format(@"SELECT M.MRN_NO, M.ITEMCODE, M.ITEM_DESCRIPTION, I.VENDOR_CODE, I.VENDOR_NAME, M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE ,
                        //                        to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS
                        //                        FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                        //                        ON M.MRN_NO = I.MRN_NO WHERE M.MRN_NO = '"+ MRN +"' AND M.PLANT_CODE = '"+ Convert.ToString(Session["Login_Unit"]) + "'");
                        query = string.Format(@"SELECT M.MRN_NO, M.ITEMCODE, M.ITEM_DESCRIPTION, I.VENDOR_CODE, I.VENDOR_NAME, M.QUANTITY, to_char( M.CREATEDDATE, 'dd-Mon-yyyy' ) as CREATEDDATE ,
                                            to_char( M.CREATEDDATE, 'HH24:MI:SS' ) as  CREATEDTIME, M.STATUS
                                            FROM XXES_MRNINFO M JOIN ITEM_RECEIPT_DETIALS I
                                            ON M.MRN_NO = I.MRN_NO WHERE M.MRN_NO = {0} AND M.PLANT_CODE = '{1}'", MRN, unit);
                        dt = fun.returnDataTable(query);
                        ViewBag.DataSource = dt;
                        ViewBag.DropDownData = FillVerification();
                    }
                }                            
            }
            catch (Exception ex)
            {
                Msg = "Error ! " + ex.Message;
            }
            if (string.IsNullOrEmpty(Msg))
            {
                return PartialView();
            }
            else
            {
                var result = Msg;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UrlDatasource(DataManagerRequest dm)
        {
            IEnumerable DataSource = (IEnumerable)dt;
            DataOperations operation = new DataOperations();
            int count = dt.Rows.Count;
            if (dm.Skip != 0)
            {
                DataSource = operation.PerformSkip(DataSource, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                DataSource = operation.PerformTake(DataSource, dm.Take);
            }
            return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(DataSource);
        }

        private List<DDLTextValue> FillVerification() 
        {
            try
            { 
                List<DDLTextValue> Status = new List<DDLTextValue>();
                Status.Add(new DDLTextValue
                {
                    Text = "-- Select --",
                    Value = ""
                });
                Status.Add(new DDLTextValue
                {
                    Text = "Verified",
                    Value = "Verified"
                });

                return Status;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private DataTable bindMrn()
        {
            DataTable dt = new DataTable();
            try
            {
                //query = string.Format(@"update ITEM_RECEIPT_DETIALS set STORESCANBY='{0}' , STORESCANDATE=sysdate, STOREIPADDR='{3}' where
                //mrn_no='{1}' and plant_code='{2}'", Convert.ToString(Session["Login_User"]), MRN, Convert.ToString(Session["Login_Unit"]), fun.GetUserIP());
                //if (fun.EXEC_QUERY(query))
                //{
                //    Msg = "MRN No: " + MRN + " sucessfully recorded at store";
                //    dt = bindMrn();
                //}
                query = string.Format(@"SELECT MRN_NO,VENDOR_CODE,VENDOR_NAME,INVOICE_NO,VEHICLE_NO,ITEM_CODE,ITEM_DESCRIPTION,TOTAL_ITEM,STORESCANBY CREATEDBY , 
                TO_CHAR(STORESCANDATE,'dd-Mon-yyyy HH24:MI:SS') as CREATEDDATE FROM ITEM_RECEIPT_DETIALS
                where to_char(STORESCANDATE,'dd-Mon-yyyy')=to_char(sysdate,'dd-Mon-yyyy') and plant_code='{0}' order by MRN_NO", Convert.ToString(Session["Login_Unit"]));
                dt = fun.returnDataTable(query);
                //if (dt.Rows.Count > 0)
                //{
                //    return
                //    //gdvUsers.DataSource = dataTable;
                //    //gridView1.ViewCaption = "TOTAL MRN : " + dataTable.Rows.Count.ToString();
                //    //gridView1.OptionsView.ShowViewCaption = true;
                //    //Font dFont = gridView1.Appearance.ViewCaption.Font;
                //    //gridView1.Appearance.ViewCaption.Font = new Font(dFont.FontFamily, 15);
                //    //gridView1.BestFitColumns();
                //}
            }
            catch (Exception ex)
            {
                //pbf.DisplayMsg(pnlMsg, lblMsg, ex.Message, "E");
            }
            return dt;
        }
    }
}