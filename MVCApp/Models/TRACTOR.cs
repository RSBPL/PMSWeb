using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class TRACTOR
    {
			public string PLANT_CODE { get; set; }
			public string FAMILY_CODE { get; set; }
			public string ITEM_CODE { get; set; }
			public string ITEM_DESCRIPTION { get; set; }
			public string JOBID { get; set; }
			public string TRANSMISSION { get; set; }
			public string TRANSMISSION_DESCRIPTION { get; set; }
			public string TRANSMISSION_SRLNO { get; set; }
			public string REARAXEL { get; set; }
			public string REARAXEL_DESCRIPTION { get; set; }
			public string REARAXEL_SRLNO { get; set; }
			public string ENGINE { get; set; }
			public string ENGINE_DESCRIPTION { get; set; }
			public string ENGINE_SRLNO { get; set; }
			public string FCODE_SRLNO { get; set; }
			public string HYDRALUIC { get; set; }
			public string HYDRALUIC_DESCRIPTION { get; set; }
			public string HYDRALUIC_SRLNO { get; set; }
			public string REARTYRE { get; set; }
			public string REARTYRE_DESCRIPTION { get; set; }
			public string REARTYRE_SRLNO1 { get; set; }
			public string REARTYRE_SRLNO2 { get; set; }
			public string REARTYRE_MAKE { get; set; }
			public string FRONTTYRE { get; set; }
			public string FRONTTYRE_DESCRIPTION { get; set; }
			public string FRONTTYRE_SRLNO1 { get; set; }
			public string FRONTTYRE_SRLNO2 { get; set; }
			public string FRONTTYRE_MAKE { get; set; }
			public string BATTERY { get; set; }
			public string BATTERY_DESCRIPTION { get; set; }
			public string BATTERY_SRLNO { get; set; }
			public string BATTERY_MAKE { get; set; }
			public string FCODE_ID { get; set; }
			public string ENTRYDATE { get; set; }
			public string REMARKS1 { get; set; }
			public string REMARKS2 { get; set; }
			public string HYD_PUMP { get; set; }
			public string HYD_PUMP_SRLNO { get; set; }
			public string STEERING_MOTOR { get; set; }
			public string STEERING_MOTOR_SRLNO { get; set; }
			public string STEERING_ASSEMBLY { get; set; }
			public string STEERING_ASSEMBLY_SRLNO { get; set; }
			public string STERING_CYLINDER { get; set; }
			public string STERING_CYLINDER_SRLNO { get; set; }
			public string RADIATOR { get; set; }
			public string RADIATOR_SRLNO { get; set; }
			public string CLUSSTER { get; set; }
			public string CLUSSTER_SRLNO { get; set; }
			public string ALTERNATOR { get; set; }
			public string ALTERNATOR_SRLNO { get; set; }
			public string STARTER_MOTOR { get; set; }
			public string STARTER_MOTOR_SRLNO { get; set; }
			public string FTTYRE2 { get; set; }
			public string RTRIM1 { get; set; }
			public string RTTYRE1 { get; set; }
			public string RTRIM2 { get; set; }
			public string RTTYRE2 { get; set; }
			public string FTRIM1 { get; set; }
			public string FTTYRE1 { get; set; }
			public string FTRIM2 { get; set; }
			public string FINAL_LABEL_DATE { get; set; }
			public string BACKEND { get; set; }
			public string BACKEND_SRLNO { get; set; }
			public string SIM_SERIAL_NO { get; set; }
			public string IMEI_NO { get; set; }
			public string MOBILE { get; set; }
			public string ROPS_SRNO { get; set; }
			public string PDIOKDATE { get; set; }
			public string OIL { get; set; }
			public string SWAPCAREBTN { get; set; }
			public string FIPSRNO { get; set; }
			public string REMARKS { get; set; }
			public string CAREBUTTONOIL { get; set; }
			public string LABELPRINTED { get; set; }
			public string PDIDONEBY { get; set; }
			public string REARRIM_SRLNO1 { get; set; }
			public string FRONTRIM_SRLNO2 { get; set; }
			public string REARRIM_SRLNO2 { get; set; }
			public string FRONTRIM_SRLNO1 { get; set; }
			public string REARRIM { get; set; }
			public string FRONTRIM { get; set; }
			
			// COMMON FIELD FOR SEARCH BOX IN VIEW
			public string JOBID_TRACTORSRNO { get; set; }
			public string HOOKUP_NO { get; set; }
			public string QUANTITY { get; set; }
			public string PRINTING_OPTION { get; set; }
			public bool chkReplace { get; set; }
			public string RopsUserConfirmation { get; set; }

	}
}