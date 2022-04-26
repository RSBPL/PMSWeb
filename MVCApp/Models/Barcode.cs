using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class Barcode
    {

        public string Plant { get; set; }
        public string Family { get; set; }
        public string BarcodeType { get; set; }
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string sSearch { get; set; }
        public string P_Search { get; set; }
       
        public int AUTOID { get; set; }
        public int TOTALCOUNT { get; set; }
        public string MAX_INVENTORY { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string LOCATION { get; set; }

        public string SFTSTKQUANTITY { get; set; }
        public string NOOFLOCALLOCATED { get; set; }
        public string PACKINGTYPE { get; set; }
        public string VERTICALSTKLEVEL { get; set; }
        public string BULKSTORESNP { get; set; }
        public string USAGEPERTRACTOR { get; set; }
        public string REVISION { get; set; }
        public string SUPERMKT_LOC { get; set; }
        public string CAPACITY { get; set; }
        public string BLK_LOC { get; set; }
        public string BIN_NO { get; set; }
        public string LOCTYPE { get; set; }
        public string KANBAN { get; set; }
    }
    public class KanbanPrinting
    {

        public string ITEM_CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string KANBAN_NO { get; set; }
        public string SM_ROWNO { get; set; }
        public string SM_SHELFNO { get; set; }
        public string BIN_NO { get; set; }
        public string BULK_ROWNO { get; set; }
        public string BULK_SHELFNO { get; set; }
        public int PKG { get; set; }
        public int SNP { get; set; }
        public byte[] QRCode { get; set; }
        

    }
}