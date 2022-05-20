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
        public String STAGE_Code { get; set; }
        public String ITEM_CODE { get; set; }
        public String BatMake { get; set; }
        public string Transmission { get; set; }
        public string RearAxel { get; set; }
        public string FIP { get; set; }
        public string HookupNo { get; set; }
        public string Hydraulic { get; set; }
        public string Engine { get; set; }
        public string Backend { get; set; }
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
        public string ExistingSimSerialNo { get; set; }
        public string ExistingSimImei { get; set; }
        public string ExistingSimMobileNo { get; set; }
        public string gbRT { get; set; }
        public string gbFT { get; set; }
        public string lblTractorSrlno { get; set; }
        public bool isMobiledetails { get; set; }
        public bool lblTractorSrlnoisvisible { get; set; }
        public string label9 { get; set; }
        public string TractorStagePrint { get; set; }
        public string TractorAutoid { get; set; }
        public string lblROPS { get; set; }
        public bool lblROPSisvisible { get; set; }
        public bool label9isvisible { get; set; }
        public int TOTALCOUNT { get; set; }
        public bool chkPrint { get; set; }


    }
    public class StageCode
    {
        public string Value { get; set; }

        public string Text { get; set; }
    }
    public class Tractor
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string TSN { get; set; }
        public string ITEMCODE { get; set; }
        public string DESC { get; set; }
        public string JOB { get; set; }
        public string avgHours { get; set; }
        public string pdidate { get; set; }

    }


}