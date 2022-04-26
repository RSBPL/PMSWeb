using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class UserMaster
    {
      
        public string FAMILY_CODE { get; set; }
        public string Level_Name { get; set; }
        public string PUNAME { get; set; }
        public string USRNAME { get; set; }
        public string PSWORD { get; set; }
        public string U_CODE { get; set; }
        public string L_CODE { get; set; }
        public string STAGEID { get; set; }
        public bool ISACTIVE { get; set; }
        public string GATE_USER { get; set; }
        
        public bool STOREBYPASS { get; set; }
        public bool PDIUSER { get; set; }

        //for checkboxs
        public bool ShowSave { get; set; }
        public bool ShowDelete { get; set; }
        public bool ShowPrint { get; set; }
        public bool MannualMapping { get; set; }
        public bool AllowROPS { get; set; }
        public bool SaveButton { get; set; }
        public bool MRNSAVE { get; set; }





    }
}