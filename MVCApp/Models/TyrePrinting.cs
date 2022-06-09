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
        public bool chkFTRH { get; set; } 
        public bool chkFTLH { get; set; }       
        public string srlno { get; set; }
        public string description { get; set; }
        public string IPADDR { get; set; }
        public string IPPORT { get; set; }

        /**************************Rear Tyre*********************************/
        public string RPlant { get; set; }
        public string RFamily { get; set; }
        public string RItemCode { get; set; }
        public string RMakeTyre { get; set; }
        public string RQuantity { get; set; }
        public bool chkRTRH { get; set; }
        public bool chkRTLH { get; set; }
        public string RTRH { get; set; }
        public string RTLH { get; set; }
        public string Password { get; set; }
        public string RPassword { get; set; }
        public string Date { get; set; }
        public string RDate { get; set; }
        public string Rsrlno { get; set; }


    }
    public class LabelPrinting
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string dcode { get; set; }
        public string srlno { get; set; }
        public string description { get; set; }

    }
}