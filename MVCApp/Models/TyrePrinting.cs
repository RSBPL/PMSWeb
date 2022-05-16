using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class TyrePrinting
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; }
        public string MakeTyre { get; set; }
        public string Quantity { get; set; }
        public string FTRH { get; set; }
        public string FTLH { get; set; }
        public string RTRH { get; set; }
        public string RTLH { get; set; }
        public bool chkFTRH { get; set; } 
        public bool chkFTLH { get; set; } 
        public bool chkRTRH { get; set; }
        public bool chkRTLH { get; set; }


    }
}