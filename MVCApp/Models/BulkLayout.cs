using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BulkLayout
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Item { get; set; }
        public string Location { get; set; }
        public string TempLoc { get; set; }
        public string Capacity { get; set; }
        public string QTY { get; set; }

        public string ALIASNAME { get; set; }
        public string OPERATION { get; set; }
        public string TEMP_LOC { get; set; }
        public string dllItemcode { get; set; }
        public string dllLocation { get; set; }
        public string Search { get; set; }

    }
    public class RowColumn
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class LayoutModeSubList
    {
        public List<BulkLayout> BulkLayoutMode { get; set; }
        public string dllLocation { get; set; }
        public string dllItemcode { get; set; }
        public string Search { get; set; }

    }

}