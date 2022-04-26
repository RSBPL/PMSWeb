using MVCApp.CommonFunction;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class GateEntryModel
    {
        public DateTime FromDate { get; set; }
        public String PLANTCODE { get; set; }
        public String FAMILYCODE { get; set; }
        public String TRANSACTION_DATE { get; set; }
        public String MRN_NO { get; set; }
        public String VEHICLE_NO { get; set; }
        public String VENDOR_CODE { get; set; }
        public String VENDOR_NAME { get; set; }
        public String ITEM { get; set; }
        public String TOTAL_ITEM { get; set; }
        public String SOURCE_TYPE { get; set; }
        public String ORGANIZATION_CODE { get; set; }
        public String INVOICE_NO { get; set; }
        public String INVOICE_DATE { get; set; }
        public String STATUS { get; set; }
        public int draw { get; set; }
        public string start { get; set; }
        public string STARTROWINDEX { get; set; }
        public string MAXROWS { get; set; }
        public string P_Search { get; set; }
        public int TOTALCOUNT { get; set; }
        public int length { get; set; }
        public bool CheckboxReprint { get; set; }
        public bool CheckboxPrint2Label { get; set; }


    }

    public class MRNInvoice
    {
        public string PLANT_CODE { get; set; }
        public string MRN_NO { get; set; }
        public string VENDOR_CODE { get; set; }
        public string VENDOR_NAME { get; set; }
        public string INVOICE_NO { get; set; }
        public string INVOICE_DATE { get; set; }
        public string VEHICLE_NO { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string STORE_LOCATION { get; set; }
        public string SAMPLE { get; set; }
        public string SOURCE_TYPE { get; set; }
        public DateTime TRANSACTION_DATE { get; set; }

        public string STATUS { get; set; }
        public string TOTAL_ITEM { get; set; }
    }

    public class GateEntryFunction
    {
        Function fun = new Function();
        string query = string.Empty;
        string Login_User = "";
        public string CheckduplicateInvoiceInMRN(string PLANT_CODE, string MRN_NO)
        {
            string retvalue = string.Empty;
            try
            {

                query = string.Format(@"select ORGANIZATION_CODE,MRN_NO,INVOICE_NO,ITEM_CODE,ITEM_DESCRIPTION,QUANTITY,UOM,RATE,STATUS,
                INVOICE_DATE INVOICE_DATE,ITEM_REVISION,VENDOR_CODE from apps.XXESGATETAGPRINT_BARCODE WHERE  
                ORGANIZATION_CODE='{0}' AND MRN_NO='{1}' and item_code not in 
                 (select item_code from xxes_mrninfo where plant_code='{0}' and mrn_no='{1}')", PLANT_CODE, MRN_NO);
                using (DataTable dt = fun.returnDataTable(query))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (fun.isInvoiceExistsinFinancialYear(Convert.ToString(dr["INVOICE_NO"]), Convert.ToString(dr["VENDOR_CODE"])))
                        {
                            if (string.IsNullOrEmpty(retvalue))
                            {
                                retvalue = "DUPLICATE INVOICES FOUND IN MRN " + MRN_NO + " INV ARE : ";
                            }
                            else
                                retvalue += Convert.ToString(dr["INVOICE_NO"]) + ",";
                        }

                    }
                }


            }
            catch (Exception)
            {

                throw;
            }
            return retvalue;
        }
        public void UpdateMrnDetails(string PLANT_CODE, string MRN_NO, string Family_Code)
        {
            try
            {
                string puname = string.Empty, bomrevison = string.Empty, storage = string.Empty;
                query = string.Format(@"select ORGANIZATION_CODE,MRN_NO,INVOICE_NO,ITEM_CODE,ITEM_DESCRIPTION,QUANTITY,UOM,RATE,STATUS,
                INVOICE_DATE INVOICE_DATE,ITEM_REVISION from apps.XXESGATETAGPRINT_BARCODE WHERE  
                ORGANIZATION_CODE='{0}' AND MRN_NO='{1}' and item_code not in 
                 (select item_code from xxes_mrninfo where plant_code='{0}' and mrn_no='{1}')", PLANT_CODE, MRN_NO);
                using (DataTable dt = fun.returnDataTable(query))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        bomrevison = puname = string.Empty;
                        puname = GetPuname(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        storage = GetStoreLoc(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        if (string.IsNullOrEmpty(puname))
                        {
                            if (Convert.ToString(PLANT_CODE) == "T02")
                            {
                                query = string.Format(@"SELECT COUNT(*) FROM XXES_RAWMATERIAL_MASTER WHERE ITEM_CODE='{0}'
                               AND PLANT_CODE='{1}' AND FAMILY_CODE='{2}'", Convert.ToString(dr["ITEM_CODE"]),
                                Convert.ToString(PLANT_CODE),
                                Convert.ToString(PLANT_CODE));
                                if (fun.CheckExits(query))
                                {
                                    //puname = Convert.ToString(ConfigurationManager.AppSettings["DEFAULT_PUNAME"]);
                                    puname = string.Empty;
                                }
                                else
                                    puname = string.Empty;
                            }
                            else
                                //puname = Convert.ToString(ConfigurationManager.AppSettings["DEFAULT_PUNAME"]);
                                puname = string.Empty;
                        }
                        bomrevison = GetBomReviision(Convert.ToString(dr["ITEM_CODE"]), Convert.ToString(dr["ORGANIZATION_CODE"]));
                        query = string.Format(@"select count(*) from xxes_mrninfo where plant_code='{0}' and FAMILY_CODE='{1}'
                        and MRN_NO='{2}' and ITEMCODE='{3}'", Convert.ToString(PLANT_CODE),
                        Convert.ToString(Family_Code), Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["ITEM_CODE"]));
                        if (fun.CheckExits(query))
                        {
                            query = string.Format(@"update xxes_mrninfo set QUANTITY=to_number(QUANTITY)+{4} 
                            where plant_code='{0}' and FAMILY_CODE='{1}'
                            and MRN_NO='{2}' and ITEMCODE='{3}' ",
                            Convert.ToString(PLANT_CODE),
                            Convert.ToString(Family_Code), Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["ITEM_CODE"]),
                            Convert.ToDouble(dr["QUANTITY"]));
                        }
                        else
                        {
                            query = string.Format(@"insert into xxes_mrninfo(PLANT_CODE,MRN_NO,INVOICE_NO,ITEMCODE,ITEM_DESCRIPTION,CREATEDDATE,CREATEDBY,
                          QUANTITY,UOM,RATE,INVOICE_DATE,STATUS,PUNAME,ITEM_REVISION,BOM_REVISION,FAMILY_CODE,STORAGE) 
                          values('{0}','{1}','{2}','{3}','{4}',sysdate,'{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')",
                            Convert.ToString(dr["ORGANIZATION_CODE"]), Convert.ToString(dr["MRN_NO"]), Convert.ToString(dr["INVOICE_NO"]),
                            Convert.ToString(dr["ITEM_CODE"]), replaceApostophi(Convert.ToString(dr["ITEM_DESCRIPTION"])),
                            Convert.ToString(HttpContext.Current.Session["Login_User"]), Convert.ToString(dr["QUANTITY"]), Convert.ToString(dr["UOM"]), Convert.ToString(dr["RATE"])
                            , Convert.ToString(dr["INVOICE_DATE"]), Convert.ToString(dr["STATUS"]), puname,
                            Convert.ToString(dr["ITEM_REVISION"]), bomrevison, Convert.ToString(Family_Code),
                            storage
                            );
                        }
                        fun.EXEC_QUERY(query);
                    }
                }


            }
            catch (Exception)
            {
                //query = string.Format(@"delete from ITEM_RECEIPT_DETIALS where mrn_no='{0}' and plant_code='{1}'", MRN_NO, PLANT_CODE);
                //pbf.EXEC_QUERY(query);
                //query = string.Format(@"delete from xxes_mrninfo where mrn_no='{0}' and plant_code='{1}'", MRN_NO, PLANT_CODE);
                //pbf.EXEC_QUERY(query);
                throw;
            }
        }
        public string GetPuname(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = @"select character6 from apps.qa_results_v  where character6 is not null and plan_id =224155 and Character2 ='" + itemcode + "' and character1 ='" + plant + "' and rownum=1 order by Character2 ";
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        public string GetStoreLoc(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = @"select character6||'-'||character4 from apps.qa_results_v  where plan_id =224155 and Character2 ='" + itemcode + "' and character1 ='" + plant + "' and rownum=1 order by Character2 ";
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        public string replaceApostophi(string chkstr)
        {
            return chkstr.Replace("'", "''");
        }
        public string GetBomReviision(string itemcode, string plant) //puname
        {
            string tobereturn = string.Empty;
            try
            {
                query = string.Format(@"SELECT MAX(REV) FROM apps.XXES_BOM_REVISION_V re WHERE RE.ORGANIZATION_CODE='{0}' AND RE.SEGMENT1='{1}'",
                    plant.Trim(), itemcode.Trim());
                tobereturn = fun.get_Col_Value(query);
            }
            catch (Exception)
            {


            }
            return tobereturn;
        }
        public void UpdateRawMeterialMaster(MRNInvoice invoice, string Family_Code)
        {
            try
            {
                if (!string.IsNullOrEmpty(invoice.ITEM_CODE) && !string.IsNullOrEmpty(invoice.PLANT_CODE))
                {
                    query = string.Format(@"select count(*) from XXES_RAWMATERIAL_MASTER where plant_code='{0}' and family_code='{1}' and item_code='{2}'",
                        invoice.PLANT_CODE, Convert.ToString(Family_Code), invoice.ITEM_CODE);
                    if (!fun.CheckExits(query))
                    {
                        query = string.Format(@"INSERT INTO XXES_RAWMATERIAL_MASTER(PLANT_CODE,FAMILY_CODE,ITEM_CODE,ITEM_DESCRIPTION,CREATEDBY,CREATEDDATE)    
                         VALUES('{0}','{1}','{2}','{3}','{4}',SYSDATE)", invoice.PLANT_CODE, Convert.ToString(Family_Code), invoice.ITEM_CODE, invoice.ITEM_DESCRIPTION,
                        Convert.ToString(HttpContext.Current.Session["Login_User"]));
                        if (fun.EXEC_QUERY(query))
                        {

                        }
                    }
                }
            }
            catch (Exception)
            {


            }
        }
    }
}