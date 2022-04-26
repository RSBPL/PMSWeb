using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BackendModel
    {
        public string PLANT_CODE { get; set; }
        public string FAMILY_CODE { get; set; }
        public string BACKEND { get; set; }
        public string BACKEND_DESC { get; set; }
        public string REARAXEL { get; set; }
        public string REARAXEL_DESC { get; set; }
        public string TRANSMISSION { get; set; }
        public string TRANSMISSION_DESC { get; set; }
        public string HYDRAULIC { get; set; }
        public string HYDRAULIC_DESC { get; set; }
        public string PREFIX1 { get; set; }
        public string PREFIX2 { get; set; }
        public string SUFFIX1 { get; set; }
        public string SUFFIX2 { get; set; }
        public string REMARKS1 { get; set; }
        public string CREATEDBY { get; set; }
        public string CREATEDDATE { get; set; }
        public string UPDATEDBY { get; set; }
        public string UPDATEDDATE { get; set; }
        public string AUTOID { get; set; }
    }
}