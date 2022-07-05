using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Common
{
    
    public class RADIATOR
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string JOB { get; set; }
        public string RADIATORID { get; set; }
        public string LoginStageCode { get; set; }
    }
    public class PTBuckleup
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string ITEMCODE { get; set; }
        public string JOB { get; set; }
        public string BACKENDSRLNO { get; set; }
        public string ENGINESRLNO { get; set; }
        public string STAGE { get; set; }
        public string FCODEID { get; set; }
        public string SYSUSER { get; set; }
        public string SYSTEMNAME { get; set; }
        public string BYPASS { get; set; }
        public string CREATEDBY { get; set; }

        public string IsPrintLabel { get; set; }
        public string IPADDR { get; set; }
        public string PrintMMYYFormat { get; set; }
        public string SUFFIX { get; set; }
        public string TSN { get; set; }
        public string TRACTOR_DESC { get; set; }
        public string IPPORT { get; set; }

    }

    public class Tyres
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string JOB { get; set; }
        public string LHSERIALNO { get; set; }
        public string RHSERIALNO { get; set; }
        public string LOGINSTAGECODE { get; set; }
        public bool OffTyreMakeCheck { get; set; }
        public string RIMSERIALLH { get; set; }
        public string RIMSERIALRH { get; set; }
        public string LH_FRONTTYRE { get; set; }
        public string RH_FRONTTYRE { get; set; }
        public string LH_REARTYRE { get; set; }
        public string RH_REARTYRE { get; set; }
        public string FRONT_RIM { get; set; }
        public string REAR_RIM { get; set; }

    }
    public class SLTALT
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string JobID { get; set; }
        public string Alternator { get; set; }
        public string StaterMotor { get; set; }
        public string LoginStageCode { get; set; }
        public string CreatedBy { get; set; }
    }
    public class BatteryData
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string CREATEDBY { get; set; }
        public string JOB { get; set; }
        public string SERIALNO { get; set; }
        public string MAKE { get; set; }
        public string LoginStageCode { get; set; }
    }

    public class CAREBTN
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string JobID { get; set; }
        public string QRCode { get; set; }
        public string Msg { get; set; }
        public string Srno { get; set; }
        public string IMEI { get; set; }
        public string Mobile { get; set; }
        public string Modal { get; set; }

    }

    public class PDIOIL
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string TractorSrNo { get; set; }
        public string OilValue { get; set; }
        public string Msg { get; set; }
        public string CraneCode { get; set; }
        public string CraneName { get; set; }
        public string QRCode { get; set; }

        public string Srno { get; set; }
        public string IMEI { get; set; }
        public string Mobile { get; set; }
        public string Modal { get; set; }

        public string LoginStage { get; set; }
        public string Password { get; set; }
        public string CareButtonOil { get; set; }
        public string FinalLableDate { get; set; }
    }
    public class GATEIN
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string MRN { get; set; }
        public string Msg { get; set; }
        public string VehicleNo { get; set; }
        public string Invoice { get; set; }
        public string Remarks { get; set; }
        public string PersonNo { get; set; }
        public string LoginUser { get; set; }
        public string MrnTot { get; set; }
        public string Pending { get; set; }

    }

    public class GATEOUT
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string MRN { get; set; }
        public string Msg { get; set; }
        public string VehicleNo { get; set; }
        public string Invoice { get; set; }
        public string Remarks { get; set; }
        public string PersonNo { get; set; }
        public string LoginUser { get; set; }
        public string MrnTot { get; set; }
        public string Pending { get; set; }

    }

    public class ENGINE
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string JobId { get; set; }
        public string StageCode { get; set; }
        public string Msg { get; set; }
        public string Fcode { get; set; }
        public string SrlNo { get; set; }
        public string Engine { get; set; }
        public string Dcode { get; set; }

        public string SYSTEMNAME { get; set; }
        public string SYSUSER { get; set; }
        public string PrintMMYYFormat { get; set; }
        public string IsPrintLabel { get; set; }
        public string IPADDR { get; set; }
        public string SUFFIX { get; set; }
        public string IPPORT { get; set; }
        public string LoginStage { get; set; }
        public string LoginUser { get; set; }
        public string IsBy_Pass { get; set; }

        //REPRINT PROPERTIES
        public string ITEMCODE { get; set; }
        public string ITEMDESC { get; set; }
        public string TRANSMISSIONSRLNO { get; set; }
        public string REARAXELSRLNO { get; set; }
        public string ENGINESRLNO { get; set; }
        public string FCODESRLNO { get; set; }
        public string FCODEID { get; set; }
        public string ROPSSRNO { get; set; }
        public string BACKENDSRLNO { get; set; }
        public string ENTRYDATE { get; set; }
        public bool REQUIREENGINE { get; set; }
        public bool REQUIREBACKEND { get; set; }

    }

    public class FTBuckleup
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string ITEMCODE { get; set; }
        public string JOB { get; set; }
        public string TRANSMISSIONSRLNO { get; set; }
        public string REARAXELSRLNO { get; set; }
        public string STAGE { get; set; }
        public string FCODEID { get; set; }
        public string SYSUSER { get; set; }
        public string SYSTEMNAME { get; set; }
        public string BYPASS { get; set; }
        public string CREATEDBY { get; set; }
        public string IsPrintLabel { get; set; }
        public string IPADDR { get; set; }
        public string PrintMMYYFormat { get; set; }
        public string SUFFIX { get; set; }
        public string TSN { get; set; }
        public string TRACTOR_DESC { get; set; }
        public string IPPORT { get; set; }
        public string LoginStageCode { get; set; }
        public string BackendSrlno { get; set; }
        public string JOBID { get; set; }
    }

    public class EngineCare
    {
        public string AutoId { get; set; }
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOBID { get; set; }
        public string FCODEID { get; set; }
        public string FCode { get; set; }
        public string Desc { get; set; }
        public string Msg { get; set; }
        public string CREATEDBY { get; set; }
        public string Remarks { get; set; }


    }
    public class CLUSTER
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOB { get; set; }
        public string CLUSTERSRNO { get; set; }
        public string CREATEDBY { get; set; }
        public string LOGINSTAGECODE { get; set; }

    }
    public class POWERSTEERING
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOB { get; set; }
        public string POWERSTEERINGSRNO { get; set; }
        public string CREATEDBY { get; set; }
        public string LOGINSTAGECODE { get; set; }

    }
    public class STEERING
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOB { get; set; }
        public string HYDPUMPSRLNO { get; set; }
        public string STEERINGMOTORSRLNO { get; set; }
        public string STEERINGASSEMBLYSRLNO { get; set; }
        public string CREATEDBY { get; set; }
        public string LOGINSTAGECODE { get; set; }

    }
    public class HYDRAULIC
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOB { get; set; }
        public string HYDSRLNO { get; set; }
        public string CREATEDBY { get; set; }
        public string LOGINSTAGECODE { get; set; }

    }
    public class BACKEND
    {
        public string backend { get; set; }
        public string backend_desc { get; set; }
        public string backend_srlno { get; set; }
        public string runningSrlno { get; set; }
        public string transmission { get; set; }
        public string transSrlno { get; set; }
        public string Axel { get; set; }
        public string AxelSrlno { get; set; }
        public string Hydraulic { get; set; }
        public string HydraulicSrlno { get; set; }
        public string FCODE_ID { get; set; }
        public string plant { get; set; }
        public string family { get; set; }
        public string jobid { get; set; }
        public string LoginStage { get; set; }
        public string LoginUser { get; set; }
        public string StageCode { get; set; }
        public string SYSTEMNAME { get; set; }
        public string SYSUSER { get; set; }
        public string PrintMMYYFormat { get; set; }
        public string IsPrintLabel { get; set; }
        public string IPADDR { get; set; }
        public string SUFFIX { get; set; }
        public string IPPORT { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }

    }
    public class BeforePaintAssemblyModel
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string FCode { get; set; }
        public string TRANSMISSION { get; set; }
        public string REARAXEL { get; set; }
        public string ScanJOB { get; set; }
        public string ENGINE { get; set; }
        public string HYDRAULIC { get; set; }
        public string SKID { get; set; }
        public string STERRINGMOTOR { get; set; }
        public string STARTERMOTOR { get; set; }
        public string ALTERNATOR { get; set; }
        public string HYDRAULIC_PUMP { get; set; }
        public string REMARKS { get; set; }
        public string FIP { get; set; }
        public string FCId { get; set; }
        public string HOOKNO { get; set; }
        public string HType { get; set; }
        public string HKNo { get; set; }
        public string Password { get; set; }
        public string ErrorMsg { get; set; }
        public bool HookChk { get; set; }
        public string ENGINE_DCODE { get; set; }
        public string FIP_DCODE { get; set; }
        public string FINALLABELDATE { get; set; }
        public string LOGINSTAGECODE { get; set; }
        public string LOGINSTAGEID { get; set; }
        public string CREATEDBY { get; set; }

    }
    public class FIP
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string LoginStageCode { get; set; }
        public string LoginStageId { get; set; }
        public string CreatedBy { get; set; }
        public string EngineSrn { get; set; }
        public string Password { get; set; }
        public string Fipsrlno { get; set; }
        public string Fipdcode { get; set; }
        public string Masterfipdcode { get; set; }
        public string SplitSerialno { get; set; }
        public string Injector { get; set; }
        public string OrgId { get; set; }
        public string engine { get; set; }
        public string itemdesc { get; set; }
    }
    public class ENGINEINJECTORData
    {
        public string plantcode { get; set; }
        public string familycode { get; set; }
        public string engine { get; set; }
        public string engine_srlno { get; set; }
        public string fipsrlno { get; set; }
        public string fipdcode { get; set; }
        public string injector1 { get; set; }
        public string injector2 { get; set; }
        public string injector3 { get; set; }
        public string injector4 { get; set; }
        public string No_Of_Injector { get; set; }
        public string splitSerialno { get; set; }
        public string injector { get; set; }
        public string orgid { get; set; }
        public string LoginStageCode { get; set; }
    }
    public class ECUDATA
    {
        public string PLANT { get; set; }
        public string FAMILY { get; set; }
        public string JOB { get; set; }
        public string ECUSRNO { get; set; }
        public string CREATEDBY { get; set; }
        public string LOGINSTAGECODE { get; set; }
    }
}