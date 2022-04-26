using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class GoodsRecivingatStoreModel
    {
        public int AUTOID { get; set; }
        public string MRN_QR { get; set; }
        public string MRN_NO { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string VENDOR_CODE { get; set; }
        public string VENDOR_NAME { get; set; }
        public string QUANTITY { get; set; }
        public string CREATEDDATE { get; set; }
        public string CREATEDTIME { get; set; }
        public string STATUS { get; set; }
        public string VERIFICATION{ get; set; }
        public string STORE_VERIFIED { get; set; } 
        public string PUNAME { get; set; }
        public string INSPECTION_DONE { get; set; }
        public string QUALITY_OK { get; set; }
        public string ITEM_REVISION { get; set; }
        public string BOM_REVISION { get; set; } 
        public string COUNTING { get; set; }
        public string MT_TEST { get; set; }
        public string CNT { get; set; }
        public string DAYS_DIFFERENCE { get; set; }
        public string FROMDATE { get; set; }
        public string TODATE { get; set; }
        public string PLANT { get; set; }
        public string ReprintSrno { get; set; }
        public string PrntRprnt { get; set; }


        public string P_Search { get; set; }
        
        public string STARTROWINDEX { get; set; }
        public string MAXROWS { get; set; }
        public int draw { get; set; }
        public string start { get; set; }
        public string length { get; set; }
        public string customStiker { get; set; }
        public string PACKING_STANDARD { get; set; }
        public string DELIVERY_DATE { get; set; }
        public string U_NAME { get; set; }
        
        public string TRANSACTION_DATE { get; set; }
        public string QTY_RECEIVED { get; set; }
        public string COUNT { get; set; }
        public byte[] QRCode { get; set; }

        public string FAMILY { get; set; }
        public int TOTALCOUNT { get; set; }
        public string BULKSTORAGE { get; set; }
        public string SUPERMARKET { get; set; }
        public string PARAMETERINFO { get; set; }
        public string PARAMVALUE { get; set; }
        public string ORDERBY { get; set; }

        public string boxNo { get; set; }
        public string Media_Type { get; set; }
        public string Remarks { get; set; }
        public string PreBarcode { get; set; }
        public string UpdatedBarcode { get; set; }
        public string REJECT_QTY { get; set; }
        public string FROMDATE_INSP { get; set; }
        public string TODATE_INSP { get; set; }
        public string TYPE { get; set; }
        public bool MRNPrint { get; set; }

        public string LOCATION { get; set; }
        public string INVOICE_DATE { get; set; }
        public string INVOICE_NUMBER { get; set; }
        public string IMPLEMENT { get; set; }
        public string PRINTED_ON { get; set; }

        //FOR GETTING PRINTER IP AND PORT
        public string MRNSRNO_PRINTER_IP { get; set; }
        public string MRNSRNO_PRINTER_PORT { get; set; }
        public string PSWORD { get; set; }
        public bool PrintSerialNo { get; set; }
        public string SerialNo { get; set; }

        public string BUYER_NAME { get; set; }

       



    }   

    public class BARCODEPRINT
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string MRN_NO { get; set; }
        public string ITEMCODE { get; set; }
        public string QUANTITY { get; set; }
        public string QTY_RECEIVED { get; set; }
        
        public string TOTALBOX { get; set; }
        public string STIKER { get; set; }
        public string MODE { get; set; }
        public string ITEM_DESC { get; set; }
        public string SUPP_NAME { get; set; }

        public string QTY_ORD { get; set; }
        public string PKG_STD { get; set; }
        public string CURRENT_DATE { get; set; }
        public string PUNAME { get; set; }
        public string TRANSACTION_DATE { get; set; }
        public string QTY_DLV { get; set; }
        public string BULK_LOC { get; set; }
        public string BPACKAGING { get; set; }
        public string BULK_SNP { get; set; }
        public string BOX_NO { get; set; }
        public string QR_CODE { get; set; }

        public string UNPACKED { get; set; }
        public string QLTY { get; set; }
        public string BUYER_NAME { get; set; }
        public string PO_LINE { get; set; }
        

    }

    public class VERIFICATIONCRYSTALREPORT
    {
        public string TRANSACTION_DATE { get; set; }
        public string PLANT { get; set; }
        public string U_NAME { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEM_DESC { get; set; }
        public string SUPP_NAME { get; set; }
        public string QTY_ORD { get; set; }
        public string PKG_STD { get; set; }
        public string CURRENT_DATE { get; set; }
        public string PUNAME { get; set; }
        public string QTY_DLV { get; set; }
        public string BULK_LOC { get; set; }
        public string BPACKAGING { get; set; }
        public string BULK_SNP { get; set; }
        public string BOX_NO { get; set; }
        public byte[] QR_CODE { get; set; }
        public string UNPACKED { get; set; }

    }
}