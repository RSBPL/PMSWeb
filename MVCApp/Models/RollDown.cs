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
        public String IPAddress { get; set; }
        public String JOBID { get; set; }
        public String RepJOBID { get; set; }
        public String TYPE { get; set; }
        public String ROPS { get; set; }
        
        public string Date { get; set; }
        public string BatMake { get; set; }
        public string Transmission { get; set; }
        public string Transmission_srlno { get; set; }
        public string RearAxel { get; set; }
        public string RearAxel_srlno { get; set; }
        public string FIP { get; set; }
        public string HookupNo { get; set; }
        public string Hydraulic { get; set; }
        public string Hydraulic_srlno { get; set; }
        public string Engine { get; set; }
        public string Engine_srlno { get; set; }
        public string Backend { get; set; }
        public string Backend_srlno { get; set; }
        public string Skid { get; set; }
        public string RearSrnn1 { get; set; }
        public string RearRIM1 { get; set; }
        public string RearTyre1 { get; set; }
        public string RearTyre1_srlno1 { get; set; }
        public string RearTyre1_dcode { get; set; }
        public string RearTyre2_dcode { get; set; }
        public string RearSrnn2 { get; set; }
        public string reartyreleftsidemake1 { get; set; }
        public string reartyrerightsidemake2 { get; set; }
        public string fronttyreleftsidemake1 { get; set; }
        public string fronttyrerightsidemake2 { get; set; }
        public string reartyremake { get; set; }
        public string fronttyremake { get; set; }
        public string RearRIM2 { get; set; }
        public string RearTyre2_srlno2 { get; set; }
        public string FrontSrnn1 { get; set; }
        public string FrontRIM1 { get; set; }
        public string FrontTyre1_srlno1 { get; set; }
        public string FrontTyre1_Dcode { get; set; }
        public string FrontSrnn2 { get; set; }
        public string FrontRIM2 { get; set; }
        


        public string Battery { get; set; }
        public string Battery_srlno { get; set; }
        public string HydrualicPump { get; set; }
        public string HydrualicPump_srlno { get; set; }
        public string Radiator_srlno { get; set; }
        public string Radiator { get; set; }
        public string SteeringCylinder { get; set; }
        public string SteeringCylinder_srlno { get; set; }
        public string SteeringMotor { get; set; }
        public string SteeringMotor_srlno { get; set; }
        public string SteeringAssem { get; set; }
        public string SteeringAssem_srlno { get; set; }
        public string Alternator { get; set; }
        public string Alternator_srlno { get; set; }
        public string Cluster { get; set; }
        public string Cluster_srlno { get; set; }
        public string Motor { get; set; }
        public string Motor_srlno { get; set; }
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
        
        public bool isMobiledetails { get; set; }
      
        public string lblROPS { get; set; }
        public bool lblROPSisvisible { get; set; }
        public bool label9isvisible { get; set; }
        public int TOTALCOUNT { get; set; }
        
        public string FrontTyre2_srlno2 { get; set; }
        public string FrontTyre2_Dcode { get; set; }
        public string TractorCode  { get; set; }
        public string TractorDesc { get; set; }
        public string TractorStagePrint { get; set; }
        public string TractorAutoid { get; set; }
        public string TractorSrlno { get; set; }
        public string Pdidate { get; set; }
        public string Rolloutdate { get; set; }
        public string Carebuttonoildate { get; set; }
        public string batterymake { get; set; }
        public string simserialno { get; set; }
        public string reqcarebtn { get; set; }
        public string swapbtn { get; set; }
        public string shortcode { get; set; }
        public string remarks { get; set; }

        public string isEnableCarebutton { get; set; }
        public bool isTransRequire { get; set; }
        public bool isRearAxelRequire { get; set; }
        public bool isBackendRequire { get; set; }
        public bool isEngineRequire { get; set; }
        public bool isSrNoRequire { get; set; }
        public string Suffix { get; set; }       
        public string TractorType { get; set; }
        public string hydrualic_desc { get; set; }
        public string REARTYRE_DESCRIPTION { get; set; }
        public string FRONTTYRE_DESCRIPTION { get; set; }

        public string PrintMMYYFormat { get; set; }
        public string Prefix_4 { get; set; }
        public string avgHours { get; set; }
        public string Password { get; set; }
        public string BypassPassword { get; set; }
        public string ORG_ID { get; set; }
        public bool isREQUIRE_REARTYRE { get; set; }
        public bool isBypass { get; set; }
        public bool isHydrualicRequire { get; set; }
        public bool isREQ_RHRT { get; set; }
        public bool isREQUIRE_FRONTTYRE { get; set; }
        public bool isREQUIRE_BATTERY { get; set; }
        public bool isREQ_HYD_PUMP { get; set; }
        public bool isREQ_RADIATOR { get; set; }
        public bool isREQ_RHFT { get; set; }
        public bool isREQ_CLUSSTER { get; set; }
        public bool isREQ_ALTERNATOR { get; set; }
        public bool isREQ_STEERING_ASSEMBLY { get; set; }
        public bool isREQ_STERING_CYLINDER { get; set; }
        public bool isREQ_ROPS { get; set; }
        public bool isREQ_STARTER_MOTOR { get; set; }
        public bool isREQ_STEERING_MOTOR { get; set; }
        public bool isREQ_FRONTRIM { get; set; }
        public bool isREQ_REARRIM { get; set; }
        public bool isReplaceJob { get; set; }





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