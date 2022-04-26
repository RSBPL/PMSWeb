using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class DashboardModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }        

    }

    public class ChartModel
    {
        public string Item { set; get; }
        public string ItemDesc { set; get; }
        public int Quantity { set; get; }
    }
}