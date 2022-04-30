using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Common
{
    /// <summary>
    /// ///////////APP CLASSES
    /// </summary>
    public class AppLogin
    {
        public string Controller { get; set; }
        public string Message { get; set; }
        public string Login_Unit { get; set; }
        public string Login_Level { get; set; }
        public string STOREBYPASS { get; set; }
        public string Login_User { get; set; }
        public string Login_Time { get; set; }
        public string LoginStage { get; set; }
        public string LoginFamily { get; set; }
        public string LoginStageCode { get; set; }
        public string IPADDR { get; set; }
        public string IPPORT { get; set; }

        public string IsPrintLabel { get; set; }
        public string PrintMMYYFormat { get; set; }
        public string NoOfStages { get; set; }

        public string LoginOrgId { get; set; }

        public bool OffTyreMakeCheck { get; set; }
        public string PUNAME { get; set; }
        public string SUFFIX { get; set; }

        public string SUCCESS_INTERVAL { get; set; }
        public string ERROR_INTERVAL { get; set; }
        public string SCANNER { get; set; }


    }
    public class SplitItemBarcode
    {
        public string PLANT { get; set; }
        public string PO { get; set; }
        public string ITEMCODE { get; set; }
        public string PKGQTY { get; set; }
        public string BULKLOC { get; set; }
        public string POLINE { get; set; }
        public string SUPPLIER { get; set; }
        public string IF { get; set; }

        public string DATE { get; set; }
        public string BOX { get; set; }
        public string MRN { get; set; }

    }
    public class BOXBARCODE
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }


        public string MRN { get; set; }
        public string ITEMCODE { get; set; }
        public string QTY { get; set; }
        public string RECQTY { get; set; }
        public string QRCODE { get; set; }
        public string CREATEDBY { get; set; }
        public string BOX { get; set; }
        public string REMARKS { get; set; }
        public string PRINTERIP { get; set; }
        public string PRINTERPORT { get; set; }
    }

    public class SPLITBOXBARCODE
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string MRN { get; set; }
        public string QRCODE { get; set; }
        public string CREATEDBY { get; set; }

    }
    public class BULKSTORGAE
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string LOCATION { get; set; }
        public string QRCODE { get; set; }
        public string AVAIL_LOC { get; set; }
        public string REMARKS { get; set; }
        public string TEMPLOCATION { get; set; }


    }
    public class SUPERMKTSTORGAE
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string LOCATION { get; set; }
        public string KANBAN { get; set; }

    }
    public class SUPERMKTQTYUPDATE
    {
        public string ITEMCODE { get; set; }
        public string QUANTITY { get; set; }
        public string UPDATEQUANTITY { get; set; }
        public string UPDATEDBY { get; set; }
        public string FIRSTLOCATION { get; set; }
        public string SECONDLOCATION { get; set; }

    }
    public class COMMONDATA
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string DATA { get; set; }
        public string REMARKS { get; set; }
        public string SUPERMARKET { get; set; }
        public string ZONE { get; set; }
        public string ITEMCODE { get; set; }
        public string JOB { get; set; }
        public string LOCATION { get; set; }
    }
    public class RETURNTABLEMSG
    {
        public DataTable LOCATION { get; set; }
        public string MSG { get; set; }
    }
    public class ShiftDetail
    {
        public string Shiftcode { get; set; }
        public string NightExists { get; set; }
        public string isDayNeedToLess { get; set; }
        public string ShiftStart { get; set; }
        public string shiftEnd { get; set; }
        public DateTime Plandate { get; set; }

    }

    public class Kitting
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string KITNO { get; set; }
        public string CREATEDBY { get; set; }
    }
    public class FAULTYITEMS
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string RECQTY { get; set; }
        public string LOCATION { get; set; }
        public string ITEMCODE { get; set; }
        public string LASTQTY { get; set; }
        public string REASON { get; set; }
        public string CREATEDBY { get; set; }
        public string STAGE { get; set; }
        public string TRANSACTIONTYPE { get; set; }

    }
    
}