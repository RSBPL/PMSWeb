using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class FIPModel
    {
        public string PLANT_CODE { get; set; }
        public string FAMILY_CODE { get; set; }
        public string ITEM_CODE { get; set; }
        public string DESCRIPTION { get; set; }

        public string MODEL_CODE_NO { get; set; }
        public string ENTRYDATE { get; set; }
        public string REMARKS1 { get; set; }
        public string REMARKS2 { get; set; }
        public string CREATEDBY { get; set; }
        public string CREATEDDATE { get; set; }
        public string UPDATEDBY { get; set; }
        public string UPDATEDDATE { get; set; }
        public string AUTOID { get; set; }
    }
}