using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BulkStorage
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Location { get; set; }
        public string Item { get; set; }
        public string Capacity { get; set; }
        public bool TempLoc { get; set; }
        public string Description { get; set; }
        public string SftStkQuantity { get; set; }
        public string NoOfLocAllocated { get; set; }
        public string PackingType { get; set; }
        public string VerticalStkLevel { get; set; }
        public string BulkStoreSNP { get; set; }
        public string UsagePerTractor{ get; set; }
        public string Revision { get; set; }
        public bool chkUnpck { get; set; }
        public string MaxInventory { get; set; }

    }
}