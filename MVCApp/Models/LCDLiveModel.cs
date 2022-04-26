using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class LCDLiveModel
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Date { get; set; }        
        public string Shift { get; set; }              
    }
}