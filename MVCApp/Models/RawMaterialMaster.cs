using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class RawMaterialMaster
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string PackingStandard { get; set; }
        public string ItemDescription { get; set; }
        public string ItemCode { get; set; }
        
    }
}