using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class MRNDisplayModel
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}