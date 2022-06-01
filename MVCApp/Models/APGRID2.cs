using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class APGRID2
    {
        public int SRNO { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string SHORT_CODE { get; set; }
        public string FROM_ID { get; set; }
        public string TO_ID { get; set; }
        public string QTY { get; set; }
        public string HOOK { get; set; }
        public string BATTERY { get; set; }
        public string HEAD_LAMP { get; set; }
        public string STEERING_WHEEL { get; set; }
        public string REAR_HOOD_WIRING_HARNESS { get; set; }
        public string SEAT { get; set; }

        //NEW COLUNMS

        public string F_CODE { get; set; }
        public string FENDER_RH { get; set; }
        public string FENDER_LH { get; set; }
        public string FENDER_HARNESS_LH { get; set; }
        public string FENDER_HARNESS_RH { get; set; }
        public string RADIATOR { get; set; }
        public string FENDER_LAMP { get; set; }
        public string PLAN { get; set; }
        public string DONE { get; set; }
        public string PENDING { get; set; }
        public string lblHookDown { get; set; }
        public string lblDayTotal { get; set; }
        public string lblDate { get; set; }
        public string Shift { get; set; }
        public string Time { get; set; }



        public string lblInfo { get; set; }
        public bool lblInfoTF { get; set; }

        public string lblInfodb { get; set; }
        public bool lblInfodbTF { get; set; }

        public string lblError { get; set; }
        public bool lblErrorTF { get; set; }

        public string lblErrordb { get; set; }
        public bool lblErrordbTF { get; set; }

        public string imgstatus { get; set; }
        public string imgstatusHS { get; set; }
        public virtual ICollection<APGRID2> APgriddata { get; set; }
    }
}