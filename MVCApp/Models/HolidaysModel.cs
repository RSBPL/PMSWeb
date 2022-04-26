using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class HolidaysModel
    {
        public int AutoId { get; set; }
        public string Plant_Code { get; set; }
        public string Holi_Date { get; set; }
        public string Description { get; set; }
    }
}