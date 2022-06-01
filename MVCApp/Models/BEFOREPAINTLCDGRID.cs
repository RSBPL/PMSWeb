using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BEFOREPAINTLCDGRID
    {
        public string SRNO { get; set; }
        public string FCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string SHORTCODE { get; set; }
        public string ENGINE { get; set; }
        public string LIFT { get; set; }
        public string FRONT_AXLE { get; set; }
        public string BRAKE_PEDAL { get; set; }
        public string FUEL_TANK { get; set; }
        public string CLUTCH_PEDAL { get; set; }
        public string SPOOL_VALUE { get; set; }
        public string TANDEM_PUMP { get; set; }
        public string STARTER_MOTOR { get; set; }
        public string ALTERNATOR { get; set; }
        public string FRONT_SUPPORT { get; set; }
        public string STEERING_COLUMN { get; set; }
        public string PLANNED { get; set; }
        public string ACTUAL { get; set; }
        public string PENDING { get; set; }
        public int AGEING_DAYS { get; set; }
        public string BackColor { get; set; }
        public string lblPending { get; set; }
        public string lblHookUp { get; set; }
        public string lblDayTotal { get; set; }
        public string lblInfo { get; set; }
        public string lblDate { get; set; }
        public string Shift { get; set; }
        public string lblTime { get; set; }
        public bool lblInfoTF { get; set; }

        public string lblInfodb { get; set; }
        public bool lblInfodbTF { get; set; }

        public string lblError { get; set; }
        public bool lblErrorTF { get; set; }

        public string lblErrordb { get; set; }
        public bool lblErrordbTF { get; set; }

        public string imgstatus { get; set; }
        public string imgstatusHS { get; set; }

        public virtual ICollection<BEFOREPAINTLCDGRID> BPgriddata { get; set; }
    }
}