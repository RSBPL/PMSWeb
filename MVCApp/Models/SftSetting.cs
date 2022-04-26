using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class SftSetting
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ParamValue { get; set; }
        public string ParamInfo { get; set; }
        public string SuccessIntvl { get; set; }
        public string ErrorIntvl { get; set; }
        public string QtyVeriLbl { get; set; }
        public bool A4Sheet { get; set; }
        public bool Barcode { get; set; }
        public bool Quality { get; set; }
        public string PrintingCategory { get; set; }
        public string QCFromDays { get; set; }
        public string PrintVerification { get; set; }
    }
    
}