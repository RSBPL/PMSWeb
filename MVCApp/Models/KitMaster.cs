using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class KitMaster
    {
        public string AUTOID { get; set; }
        public string PLANTCODE { get; set; }
        public string FAMILYCODE { get; set; }
        public string KITNO { get; set; }
        public string ITEMCODE { get; set; }
        public string QUANTITY { get; set; }
        public string ITEMDescription { get; set; }
        public byte[] QRCode { get; set; }
        public string SMLocation { get; set; }

    }
}