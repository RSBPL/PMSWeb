using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BeforePaintAssemblyModel
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string FCode { get; set; }
        public string TRANSMISSION { get; set; }
        public string REARAXEL { get; set; }
        public string ScanJOB { get; set; }
        public string ENGINE { get; set; }
        public string HYDRAULIC { get; set; }
        public string SKID { get; set; }
        public string STERRINGMOTOR { get; set; }
        public string STARTERMOTOR { get; set; }
        public string ALTERNATOR { get; set; }
        public string HYDRAULIC_PUMP { get; set; }
        public string REMARKS { get; set; }
        public string FIP { get; set; }
        public string FCId { get; set; }
        public string HOOKNO { get; set; }
        public string HType { get; set; }
        public string HKNo { get; set; }
        public string Password { get; set; }
        public string ErrorMsg { get; set; }
        public bool HookChk { get; set; }
        public string ENGINE_DCODE { get; set; }
        public string FIP_DCODE { get; set; }

    }
}