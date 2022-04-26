using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class KanbanMaster
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string KanbanNumber { get; set; }
        public string Item { get; set; }
        public string SuperMarketLoc { get; set; }
        public string Capacity { get; set; }
        public string Description { get; set; }
        public string SftStkQuantity { get; set; }
        public string NoOfBins { get; set; }
        public string UsagePerTractor { get; set; }
        public string Modes { get; set; }
        

    }
}