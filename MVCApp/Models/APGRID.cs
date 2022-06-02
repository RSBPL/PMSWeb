using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class APGRID
    {
        public string SRNO { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string SHORT_CODE { get; set; }
        public string FROM_ID { get; set; }
        public string TO_ID { get; set; }
        public string QTY { get; set; }
        public string HOOK { get; set; }
        public string FRONT_SUPPORT { get; set; }
        public string CENTER_AXEL { get; set; }
        public string SLIDER { get; set; }
        public string STERING_CYLINDER { get; set; }
        public string STEERING_COLUMN { get; set; }
        public string STEERING_BASE { get; set; }
        public string FENDER { get; set; }
        public string FENDER_RAILING { get; set; }
        public string RADIATOR { get; set; }
        public string FRONTTYRE { get; set; }
        public string REARTYRE { get; set; }
        public string Time { get; set; }
        public string Shift { get; set; }
        public string lblDate { get; set; }
        public string lblHookDown { get; set; }
        public string lblDayTotal { get; set; }






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



        public virtual ICollection<APGRID> APgriddata { get; set; }
    }
}