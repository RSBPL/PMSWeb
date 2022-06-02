using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class SftSetting
    {
        public string AutoId { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ParamValue { get; set; }
        public string ParamInfo { get; set; }
        public string SuccessIntvl { get; set; }
        public string ErrorIntvl { get; set; }
        public string QtyVeriLbl { get; set; }
        public bool A4Sheet { get; set; }
        public bool Barcode { get; set; }
        public bool Quality { get; set; }
        public string PrintingCategory { get; set; }
        public string QCFromDays { get; set; }
        public string PrintVerification { get; set; }

        public string SMTPServer { get; set; }
        public bool ChkSSL { get; set; }
        public string SMTPEMAILID { get; set; }
        public string EmailTo { get; set; }
        public string SMTPPSWORD { get; set; }
        public string PRIORITY { get; set; }
        public string SMTPPORT { get; set; }
        public bool ChkPRINT_SERIAL_NUMBER { get; set; }
        public bool ChkSUB_ASSEMBLY_SERIAL_NUMBER { get; set; }
        public bool ChkFAMILY_SERIAL { get; set; }
        public bool ChkSwitch_Of_Tyre_Make { get; set; }
        public string STAGE { get; set; }
        public string ITEM { get; set; }
        public string NOTIFYEMAILID { get; set; }
        public string NOTIFYMOBILE { get; set; }
        public string NOTIFYUSERNAME { get; set; }





    }

}