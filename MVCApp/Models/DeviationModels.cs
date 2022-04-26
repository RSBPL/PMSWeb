using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class DeviationModels
    {
        public string AUTOID { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string DeviationType { get; set; }
        public string DeviationQty { get; set; }
        public string EndDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string AprovedBy { get; set; }
        public string Remarks { get; set; }
    }
}