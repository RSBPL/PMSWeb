using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class EKIDashbordModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string TODAYS_MRN { get; set; }
        public string QC_REJECTION { get; set; }
        public string SHORT_MRN { get; set; }
        public string REJECTION_LINE { get; set; }
        public string ITEM_EXCEED_MAXINVENTORY { get; set; }
        public string PACKETS_OTHER_SNP { get; set; }
        public string ITEM_TEMP_LOCATION { get; set; }
        public string SHORT_BULK { get; set; }
        public string SHORT_SUPRMKT { get; set; }
        public string VENDOR_SHORT { get; set; }
        

    }
}