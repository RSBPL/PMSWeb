using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class RollDown
    {
        public String PLANTCODE { get; set; }
        public String FAMILYCODE { get; set; }
        public String STAGE { get; set; }
        public String ITEM_CODE { get; set; }
        public string Transmission { get; set; }
        public string RearAxel { get; set; }
        public string FIP { get; set; }
        public string HookupNo { get; set; }
        public string Hydraulic { get; set; }
        public string Engine { get; set; }
        public string Skid { get; set; }
        public string RearSrnn1 { get; set; }
        public string RearRIM1 { get; set; }
        public string RearTyre1 { get; set; }
        public string RearSrnn2 { get; set; }
        public string RearRIM2 { get; set; }
        public string RearTyre2 { get; set; }
        public string FrontSrnn1 { get; set; }
        public string FrontRIM1 { get; set; }
        public string FrontTyre1 { get; set; }
        public string FrontSrnn2 { get; set; }
        public string FrontRIM2 { get; set; }
        public string FrontTyre2 { get; set; }


        public string Battery { get; set; }
        public string HydrualicPump { get; set; }
        public string Radiator { get; set; }
        public string SteeringCylinder { get; set; }
        public string SteeringMotor { get; set; }
        public string SteeringAssem { get; set; }
        public string Alternator { get; set; }
        public string Cluster { get; set; }
        public string Motor { get; set; }
        public string Quantity { get; set; }
        public string Srno { get; set; }
        public string IMEI { get; set; }
        public string MOBILE { get; set; }
        public string ROPSrno { get; set; }
        public string OilQty { get; set; }

        public int TOTALCOUNT { get; set; }


    }
}