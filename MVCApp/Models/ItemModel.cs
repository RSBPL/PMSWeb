using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class ItemModel
    {        
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; }
        public string Item_Description { get; set; }
        public string EngineCode { get; set; }
        public string Engine_Description { get; set; }
        public string TransmissionCode { get; set; }
        public string Transmission_Description { get; set; }
        public string RearaxelCode { get; set; }
        public string Rearaxel_Description { get; set; }
        public string BackendCode { get; set; }
        public string Backend_Description { get; set; }
        public string ORG_ID { get; set; }
        public string Stage { get; set; }
        public string Start_Serial { get; set; }
        public string End_Serial { get; set; }
        public string No_SubAssemblies { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Current_Serial { get; set; }
        public string LastPrintedDate { get; set; }
        public string AUTOID { get; set; }
        public string Dcode { get; set; }
        public bool IsSerialNoRequired { get; set; }
        //public string Session { get; set; }



    } 
}