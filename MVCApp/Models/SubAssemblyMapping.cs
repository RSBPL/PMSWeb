using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class SubAssemblyMapping
    {
        public int AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string MainSubAssembly { get; set; }
        public string MainDescription { get; set; }
        public string ShortCode { get; set; }
        public string SubAssembly1 { get; set; }
        public string Description1 { get; set; }
        public string SubAssembly2 { get; set; }
        public string Description2 { get; set; }       
    }

    public class SubAssemblyMappingDropdown
    {
        public string DESCRIPTION { get; set; }
        public string ITEM_CODE { get; set; }
    }
}