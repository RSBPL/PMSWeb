using MVCApp.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace MVCApp.Models
{
    public class TractorMster
    {
        public string ORG_ID { get; set; }
        public string BK_BOLT_VALUE { get; set; }
        //description
        public string ItemCode_Desc { get; set; }
        public string Engine_Desc { get; set; }
        public string Backend_Desc { get; set; }
        public string Transmission_Desc { get; set; }
        public string RearAxel_Desc { get; set; }
        public string Hydraulic_Desc { get; set; }
        public string FrontTyre_Desc { get; set; }
        public string RearTyre_Desc { get; set; }
        public string Battery_Desc { get; set; }
        public string HydraulicPump_Desc { get; set; }
        public string SteeringMotor_Desc { get; set; }
        public string SteeringAssembly_Desc { get; set; }
        public string SteeringCylender_Desc { get; set; }
        public string RadiatorAssembly_Desc { get; set; }
        public string ClusterAssembly_Desc { get; set; }
        public string Alternator_Desc { get; set; }
        public string StartorMotor_Desc { get; set; }
        public string Rops_Desc { get; set; }
        public string BrakePedal_Desc { get; set; }
        public string ClutchPedal_Desc { get; set; }
        public string SpoolValue_Desc { get; set; }
        public string TandemPump_Desc { get; set; }
        public string Fender_Desc { get; set; }
        public string FenderRailing_Desc { get; set; }
        public string HeadLamp_Desc { get; set; }
        public string SteeringWheel_Desc { get; set; }
        public string RearHoolWiringHarness_Desc { get; set; }
        public string Seat_Desc { get; set; }
        








        //Tab1
        public string gleSearch { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; } 
        public string Transmission { get; set; }        
        public string Engine { get; set; }            
        public string RearAxel { get; set; }       
        public string Hydraulic { get; set; }
        public string ShortDesc { get; set; }
        public string RearTyre { get; set; }       
        public string FrontTyre { get; set; }
        public string RHRearTyre { get; set; }
        public string RHFrontTyre { get; set; }
        public string RHRearTyre_Desc { get; set; }
        public string RHFrontTyre_Desc { get; set; }
        public string Battery { get; set; }        
        public string Backend { get; set; }        
        public string HydraulicPump { get; set; }       
        public string SteeringMotor { get; set; }       
        public string SteeringAssembly { get; set; }        
        public string SteeringCylinder { get; set; }        
        public string RadiatorAssembly { get; set; }        
        public string ClusterAssembly { get; set; }        
        public string Alternator { get; set; }        
        public string StartorMotor { get; set; }  
        public string Prefix1 { get; set; }
        public string Prefix2 { get; set; }
        public string Prefix3 { get; set; }
        public string Prefix4 { get; set; }
        public string Remarks { get; set; }        
        public string DefaultAtPlanning { get; set; }
        public string DomesticExport { get; set; }
        
        public string Rops { get; set; }

        //=============================NEW COLUMN IS ADDED=============

        public string BRAKE_PEDAL { get; set; }       
        public string CLUTCH_PEDAL { get; set; }        
        public string SPOOL_VALUE { get; set; }      
        public string TANDEM_PUMP { get; set; }      
        public string FENDER { get; set; }    
        public string FENDER_RAILING { get; set; }       
        public string HEAD_LAMP { get; set; }       
        public string STEERING_WHEEL { get; set; }       
        public string REAR_HOOD_WIRING_HARNESS { get; set; }        
        public string SEAT { get; set; }


        //Tab1 Checkboxes
        public bool TransmissionChk { get; set; } = false;
        public bool EngineChk { get; set; } = false;
        public bool RearAxelChk { get; set; } = false;
        public bool HydraulicChk { get; set; } = false;
        public bool RearTyreChk { get; set; } = false;
        public bool FrontTyreChk { get; set; } = false;
        public bool BatteryChk { get; set; } = false;
        public bool BackendChk { get; set; } = false;
        public bool HydraulicPumpChk { get; set; } = false;
        public bool SteeringMotorChk { get; set; } = false;
        public bool SteeringAssemblyChk { get; set; } = false;
        public bool SteeringCylinderChk { get; set; } = false;
        public bool RadiatorAssemblyChk { get; set; } = false;
        public bool ClusterAssemblyChk { get; set; } = false;
        public bool AlternatorChk { get; set; } = false;
        public bool StartorMotorChk { get; set; } = false;
        public bool RopsChk { get; set; } = false;
        public bool EnableCarButtonChk { get; set; } = false;
        public bool GenerateSerialNoChk { get; set; } = false;
        public bool Seq_Not_RequireChk { get; set; } = false;
        public bool ElectricMotorChk { get; set; } = false;
        public bool RHRearTyreChk { get; set; } = false;
        public bool RHFrontTyreChk { get; set; } = false;


        //Tab2
        public string NoOfBoltsFrontAxel { get; set; }
        public string NoOfBoltsHydraulic { get; set; }
        public string NoOfBoltsFrontTYre { get; set; }
        public string NoOfBoltsRearTYre { get; set; }
        public string NoOfBoltsTRANSAXELToruqe1 { get; set; }
        public string NoOfBoltsEnToruqe1 { get; set; }
        public string NoOfBoltsEnToruqe2 { get; set; }
        public string NoOfBoltsEnToruqe3 { get; set; }
        public string NoOfBoltsTRANSAXELToruqe2 { get; set; }

        //TAB3
        public string T3_Plant { get; set; }
        public string T3_Family { get; set; }
        public string T3_ItemCode { get; set; }
        public string T3_ItemCode_Value { get; set; }
        public bool T3_chkNotGenrateSrno { get; set; } = false;

        //TAB4

        public string T4_Plant { get; set; }
        public string T4_Family { get; set; }
        public string T4_ItemCode { get; set; }
        public string T4_ItemCode_Desc { get; set; }
        public string FrontSupport { get; set; }
        public string FrontSupport_Desc { get; set; }
        public string CenterAxel { get; set; }
        public string CenterAxel_Desc { get; set; }
        public string Slider { get; set; }
        public string Slider_Desc { get; set; }
        public string SteeringColumn { get; set; }
        public string SteeringColumn_Desc { get; set; }
        public string SteeringBase { get; set; }
        public string SteeringBase_Desc { get; set; }
        public string Lowerlink { get; set; }
        public string Lowerlink_Desc { get; set; }
        public string RBFrame { get; set; }
        public string RBFrame_Desc { get; set; }
        public string FuelTank { get; set; }
        public string FuelTank_Desc { get; set; }
        public string Cylinder { get; set; }
        public string Cylinder_Desc { get; set; }
        public string FenderRH { get; set; }
        public string FenderRH_Desc { get; set; }
        public string FenderLH { get; set; }
        public string FenderLH_Desc { get; set; }
        public string FenderHarnessRH { get; set; }
        public string FenderHarnessRH_Desc { get; set; }
        public string FenderHarnessLH { get; set; }
        public string FenderHarnessLH_Desc { get; set; }
        public string FenderLamp4Types { get; set; }
        public string FenderLamp4Types_Desc { get; set; }
        public string RBHarnessLH { get; set; }
        public string RBHarnessLH_Desc { get; set; }
        public string FrontRim { get; set; }
        public string FrontRim_Desc { get; set; }
        public string RearRim { get; set; }
        public string RearRim_Desc { get; set; }
        public string TyreMake { get; set; }
        public string TyreMake_Desc { get; set; }
        public string RearHood { get; set; }
        public string RearHood_Desc { get; set; }
        public string ClusterMeter { get; set; }
        public string ClusterMeter_Desc { get; set; }
        public string IPHarness { get; set; }
        public string IPHarness_Desc { get; set; }
        public string RadiatorShell { get; set; }
        public string RadiatorShell_Desc { get; set; }
        public string AirCleaner { get; set; }
        public string AirCleaner_Desc { get; set; }
        public string HeadLampLH { get; set; }
        public string HeadLampLH_Desc { get; set; }
        public string HeadLampRH { get; set; }
        public string HeadLampRH_Desc { get; set; }
        public string FrontGrill { get; set; }
        public string FrontGrill_Desc { get; set; }
        public string MainHarnessBonnet { get; set; }
        public string MainHarnessBonnet_Desc { get; set; }
        public string Spindle { get; set; }
        public string Spindle_Desc { get; set; }
        //public string Motor { get; set; }
        //public string Motor_Desc { get; set; }
        public string cbTractorMaster { get; set; }
        public string ImportExcel { get; set; }
        public string gleSearchS { get; set; }

        //---------------------------------------Tab4 Checkbox-------------------------------------------------

        public bool RearRimChk { get; set; } = false;
        public bool FrontRimChk { get; set; } = false;

        //-------------------------------Add New Models Start--------------------------------------------
        public string Slider_RH { get; set; }
        public string Slider_RH_Desc { get; set; }
        public string BRK_PAD { get; set; }
        public string BRK_PAD_DESC { get; set; }
        public string FRB_RH { get; set; }
        public string FRB_RH_DESC { get; set; }
        public string FRB_LH { get; set; }
        public string FRB_LH_DESC { get; set; }
        public string FR_AS_RB { get; set; }
        public string FR_AS_RB_DESC { get; set; }

        //-------------------------------Pasword--------------------------------------------//

        public string Password { get; set; }
        public string PasswordTab2 { get; set; }

        //---------------------------------------Tab4 New Field-------------------------------------------------
        public bool ECUChk { get; set; } = false;
        public string ECU { get; set; }
        public string ECU_DESC { get; set; }

    }
    public class TractorMsterOld
    {
        public string ORG_ID { get; set; }
        public string BK_BOLT_VALUE { get; set; }
        //description
        public string ItemCode_Desc { get; set; }
        public string Engine_Desc { get; set; }
        public string Backend_Desc { get; set; }
        public string Transmission_Desc { get; set; }
        public string RearAxel_Desc { get; set; }
        public string Hydraulic_Desc { get; set; }
        public string FrontTyre_Desc { get; set; }
        public string RearTyre_Desc { get; set; }
        public string Battery_Desc { get; set; }
        public string HydraulicPump_Desc { get; set; }
        public string SteeringMotor_Desc { get; set; }
        public string SteeringAssembly_Desc { get; set; }
        public string SteeringCylender_Desc { get; set; }
        public string RadiatorAssembly_Desc { get; set; }
        public string ClusterAssembly_Desc { get; set; }
        public string Alternator_Desc { get; set; }
        public string StartorMotor_Desc { get; set; }
        public string Rops_Desc { get; set; }
        public string BrakePedal_Desc { get; set; }
        public string ClutchPedal_Desc { get; set; }
        public string SpoolValue_Desc { get; set; }
        public string TandemPump_Desc { get; set; }
        public string Fender_Desc { get; set; }
        public string FenderRailing_Desc { get; set; }
        public string HeadLamp_Desc { get; set; }
        public string SteeringWheel_Desc { get; set; }
        public string RearHoolWiringHarness_Desc { get; set; }
        public string Seat_Desc { get; set; }









        //Tab1
        public string gleSearch { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; }
        public string Transmission { get; set; }
        public string Engine { get; set; }
        public string RearAxel { get; set; }
        public string Hydraulic { get; set; }
        public string ShortDesc { get; set; }
        public string RearTyre { get; set; }
        public string FrontTyre { get; set; }
        public string RHRearTyre { get; set; }
        public string RHFrontTyre { get; set; }
        public string RHRearTyre_Desc { get; set; }
        public string RHFrontTyre_Desc { get; set; }
        public string Battery { get; set; }
        public string Backend { get; set; }
        public string HydraulicPump { get; set; }
        public string SteeringMotor { get; set; }
        public string SteeringAssembly { get; set; }
        public string SteeringCylinder { get; set; }
        public string RadiatorAssembly { get; set; }
        public string ClusterAssembly { get; set; }
        public string Alternator { get; set; }
        public string StartorMotor { get; set; }
        public string Prefix1 { get; set; }
        public string Prefix2 { get; set; }
        public string Prefix3 { get; set; }
        public string Prefix4 { get; set; }
        public string Remarks { get; set; }
        public string DefaultAtPlanning { get; set; }
        public string DomesticExport { get; set; }

        public string Rops { get; set; }

        //=============================NEW COLUMN IS ADDED=============

        public string BRAKE_PEDAL { get; set; }
        public string CLUTCH_PEDAL { get; set; }
        public string SPOOL_VALUE { get; set; }
        public string TANDEM_PUMP { get; set; }
        public string FENDER { get; set; }
        public string FENDER_RAILING { get; set; }
        public string HEAD_LAMP { get; set; }
        public string STEERING_WHEEL { get; set; }
        public string REAR_HOOD_WIRING_HARNESS { get; set; }
        public string SEAT { get; set; }


        //Tab1 Checkboxes
        public bool TransmissionChk { get; set; } = false;
        public bool EngineChk { get; set; } = false;
        public bool RearAxelChk { get; set; } = false;
        public bool HydraulicChk { get; set; } = false;
        public bool RearTyreChk { get; set; } = false;
        public bool FrontTyreChk { get; set; } = false;
        public bool BatteryChk { get; set; } = false;
        public bool BackendChk { get; set; } = false;
        public bool HydraulicPumpChk { get; set; } = false;
        public bool SteeringMotorChk { get; set; } = false;
        public bool SteeringAssemblyChk { get; set; } = false;
        public bool SteeringCylinderChk { get; set; } = false;
        public bool RadiatorAssemblyChk { get; set; } = false;
        public bool ClusterAssemblyChk { get; set; } = false;
        public bool AlternatorChk { get; set; } = false;
        public bool StartorMotorChk { get; set; } = false;
        public bool RopsChk { get; set; } = false;
        public bool EnableCarButtonChk { get; set; } = false;
        public bool GenerateSerialNoChk { get; set; } = false;
        public bool Seq_Not_RequireChk { get; set; } = false;
        public bool ElectricMotorChk { get; set; } = false;
        public bool RHRearTyreChk { get; set; } = false;
        public bool RHFrontTyreChk { get; set; } = false;


        //Tab2
        public string NoOfBoltsFrontAxel { get; set; }
        public string NoOfBoltsHydraulic { get; set; }
        public string NoOfBoltsFrontTYre { get; set; }
        public string NoOfBoltsRearTYre { get; set; }
        public string NoOfBoltsTRANSAXELToruqe1 { get; set; }
        public string NoOfBoltsEnToruqe1 { get; set; }
        public string NoOfBoltsEnToruqe2 { get; set; }
        public string NoOfBoltsEnToruqe3 { get; set; }
        public string NoOfBoltsTRANSAXELToruqe2 { get; set; }

        //TAB3
        public string T3_Plant { get; set; }
        public string T3_Family { get; set; }
        public string T3_ItemCode { get; set; }
        public string T3_ItemCode_Value { get; set; }
        public bool T3_chkNotGenrateSrno { get; set; } = false;

        //TAB4

        public string T4_Plant { get; set; }
        public string T4_Family { get; set; }
        public string T4_ItemCode { get; set; }
        public string T4_ItemCode_Desc { get; set; }
        public string FrontSupport { get; set; }
        public string FrontSupport_Desc { get; set; }
        public string CenterAxel { get; set; }
        public string CenterAxel_Desc { get; set; }
        public string Slider { get; set; }
        public string Slider_Desc { get; set; }
        public string SteeringColumn { get; set; }
        public string SteeringColumn_Desc { get; set; }
        public string SteeringBase { get; set; }
        public string SteeringBase_Desc { get; set; }
        public string Lowerlink { get; set; }
        public string Lowerlink_Desc { get; set; }
        public string RBFrame { get; set; }
        public string RBFrame_Desc { get; set; }
        public string FuelTank { get; set; }
        public string FuelTank_Desc { get; set; }
        public string Cylinder { get; set; }
        public string Cylinder_Desc { get; set; }
        public string FenderRH { get; set; }
        public string FenderRH_Desc { get; set; }
        public string FenderLH { get; set; }
        public string FenderLH_Desc { get; set; }
        public string FenderHarnessRH { get; set; }
        public string FenderHarnessRH_Desc { get; set; }
        public string FenderHarnessLH { get; set; }
        public string FenderHarnessLH_Desc { get; set; }
        public string FenderLamp4Types { get; set; }
        public string FenderLamp4Types_Desc { get; set; }
        public string RBHarnessLH { get; set; }
        public string RBHarnessLH_Desc { get; set; }
        public string FrontRim { get; set; }
        public string FrontRim_Desc { get; set; }
        public string RearRim { get; set; }
        public string RearRim_Desc { get; set; }
        public string TyreMake { get; set; }
        public string TyreMake_Desc { get; set; }
        public string RearHood { get; set; }
        public string RearHood_Desc { get; set; }
        public string ClusterMeter { get; set; }
        public string ClusterMeter_Desc { get; set; }
        public string IPHarness { get; set; }
        public string IPHarness_Desc { get; set; }
        public string RadiatorShell { get; set; }
        public string RadiatorShell_Desc { get; set; }
        public string AirCleaner { get; set; }
        public string AirCleaner_Desc { get; set; }
        public string HeadLampLH { get; set; }
        public string HeadLampLH_Desc { get; set; }
        public string HeadLampRH { get; set; }
        public string HeadLampRH_Desc { get; set; }
        public string FrontGrill { get; set; }
        public string FrontGrill_Desc { get; set; }
        public string MainHarnessBonnet { get; set; }
        public string MainHarnessBonnet_Desc { get; set; }
        public string Spindle { get; set; }
        public string Spindle_Desc { get; set; }
        //public string Motor { get; set; }
        //public string Motor_Desc { get; set; }
        public string cbTractorMaster { get; set; }
        public string ImportExcel { get; set; }
        public string gleSearchS { get; set; }

        //---------------------------------------Tab4 Checkbox-------------------------------------------------

        public bool RearRimChk { get; set; } = false;
        public bool FrontRimChk { get; set; } = false;

        //-------------------------------Add New Models Start--------------------------------------------
        public string Slider_RH { get; set; }
        public string Slider_RH_Desc { get; set; }
        public string BRK_PAD { get; set; }
        public string BRK_PAD_DESC { get; set; }
        public string FRB_RH { get; set; }
        public string FRB_RH_DESC { get; set; }
        public string FRB_LH { get; set; }
        public string FRB_LH_DESC { get; set; }
        public string FR_AS_RB { get; set; }
        public string FR_AS_RB_DESC { get; set; }

        //-------------------------------Pasword--------------------------------------------//

        public string Password { get; set; }
        public string PasswordTab2 { get; set; }


    }

    public class TractorFun
    {
       
    }
}