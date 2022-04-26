using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
	public class JOBSTATUS 
	{
		public string PLANT_CODE { get; set; }
		public string T2_PLANT_CODE { get; set; }
		public string FAMILY_CODE { get; set; }
		public string T2_FAMILY_CODE { get; set; }
		public string ITEM_CODE { get; set; }
		public string TRACTOR { get; set; }
		public string ITEM_DESCRIPTION { get; set; }
		public string JOBID { get; set; }
		public string JOB { get; set; }
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
		public string PDIDONEBY { get; set; }
		public string OIL { get; set; }
		public string SWAPCAREBTN { get; set; }
		public string FIPSRNO { get; set; }
		public string REMARKS { get; set; }
		public string CAREBUTTONOIL { get; set; }
		public string LABELPRINTED { get; set; }
		public string ImportExcel { get; set; }
		public bool IsOverride { get; set; }

		//public JOBSTATUS(string PLANT_CODE_, string FAMILY_CODE_, string ITEM_CODE_, 
		//	string ITEM_DESCRIPTION_, string JOBID_, string TRANSMISSION_, string TRANSMISSION_DESCRIPTION_,
		//	string TRANSMISSION_SRLNO_, string REARAXEL_, string REARAXEL_DESCRIPTION_, string REARAXEL_SRLNO_, 
		//	string ENGINE_, string ENGINE_DESCRIPTION_, string ENGINE_SRLNO_, string FCODE_SRLNO_, 
		//	string HYDRALUIC_, string HYDRALUIC_DESCRIPTION_, string HYDRALUIC_SRLNO_, string REARTYRE_,
		//	string REARTYRE_DESCRIPTION_, string REARTYRE_SRLNO1_, string REARTYRE_SRLNO2_, 
		//	string REARTYRE_MAKE_, string FRONTTYRE_, string FRONTTYRE_DESCRIPTION_, string FRONTTYRE_SRLNO1_,
		//	string FRONTTYRE_SRLNO2_, string FRONTTYRE_MAKE_, string BATTERY_, string BATTERY_DESCRIPTION_,
		//	string BATTERY_SRLNO_, string BATTERY_MAKE_, string FCODE_ID_, string ENTRYDATE_, string REMARKS1_, 
		//	string REMARKS2_, string HYD_PUMP_, string HYD_PUMP_SRLNO_, string STEERING_MOTOR_, 
		//	string STEERING_MOTOR_SRLNO_, string STEERING_ASSEMBLY_, string STEERING_ASSEMBLY_SRLNO_,
		//	string STERING_CYLINDER_, string STERING_CYLINDER_SRLNO_, string RADIATOR_, string RADIATOR_SRLNO_, 
		//	string CLUSSTER_, string CLUSSTER_SRLNO_, string ALTERNATOR_, string ALTERNATOR_SRLNO_, 
		//	string STARTER_MOTOR_, string STARTER_MOTOR_SRLNO_, string FTTYRE2_, string RTRIM1_, string RTTYRE1_, 
		//	string RTRIM2_, string RTTYRE2_, string FTRIM1_, string FTTYRE1_, string FTRIM2_, string FINAL_LABEL_DATE_,
		//	string BACKEND_, string BACKEND_SRLNO_, string SIM_SERIAL_NO_, string IMEI_NO_, string MOBILE_, 
		//	string ROPS_SRNO_, string PDIOKDATE_, string PDIDONEBY_, string OIL_, string SWAPCAREBTN_, string FIPSRNO_, string REMARKS_,
		//	string CAREBUTTONOIL_, string LABELPRINTED_, string ImportExcel_, bool IsOverride_)
		//{
		//	this.PLANT_CODE = PLANT_CODE_;
		//	this.FAMILY_CODE = FAMILY_CODE_;
		//	this.ITEM_CODE = ITEM_CODE_;
		//	this.ITEM_DESCRIPTION = ITEM_DESCRIPTION_;
		//	this.JOBID = JOBID_;
		//	this.TRANSMISSION = TRANSMISSION_;
		//	this.TRANSMISSION_DESCRIPTION = TRANSMISSION_DESCRIPTION_;
		//	this.TRANSMISSION_SRLNO = TRANSMISSION_SRLNO_;
		//	this.REARAXEL = REARAXEL_;
		//	this.REARAXEL_DESCRIPTION = REARAXEL_DESCRIPTION_;
		//	this.REARAXEL_SRLNO = REARAXEL_SRLNO_;
		//	this.ENGINE = ENGINE_;
		//	this.ENGINE_DESCRIPTION = ENGINE_DESCRIPTION_;
		//	this.ENGINE_SRLNO = ENGINE_SRLNO_;
		//	this.FCODE_SRLNO = FCODE_SRLNO_;
		//	this.HYDRALUIC = HYDRALUIC_;
		//	this.HYDRALUIC_DESCRIPTION = HYDRALUIC_DESCRIPTION_;
		//	this.HYDRALUIC_SRLNO = HYDRALUIC_SRLNO_;
		//	this.REARTYRE = REARTYRE_;
		//	this.REARTYRE_DESCRIPTION = REARTYRE_DESCRIPTION_;
		//	this.REARTYRE_SRLNO1 = REARTYRE_SRLNO1_;
		//	this.REARTYRE_SRLNO2 = REARTYRE_SRLNO2_;
		//	this.REARTYRE_MAKE = REARTYRE_MAKE_;
		//	this.FRONTTYRE = FRONTTYRE_;
		//	this.FRONTTYRE_DESCRIPTION = FRONTTYRE_DESCRIPTION_;
		//	this.FRONTTYRE_SRLNO1 = FRONTTYRE_SRLNO1_;
		//	this.FRONTTYRE_SRLNO2 = FRONTTYRE_SRLNO2_;
		//	this.FRONTTYRE_MAKE = FRONTTYRE_MAKE_;
		//	this.BATTERY = BATTERY_;
		//	this.BATTERY_DESCRIPTION = BATTERY_DESCRIPTION_;
		//	this.BATTERY_SRLNO = BATTERY_SRLNO_;
		//	this.BATTERY_MAKE = BATTERY_MAKE_;
		//	this.FCODE_ID = FCODE_ID_;
		//	this.ENTRYDATE = ENTRYDATE_;
		//	this.REMARKS1 = REMARKS1_;
		//	this.REMARKS2 = REMARKS2_;
		//	this.HYD_PUMP = HYD_PUMP_;
		//	this.HYD_PUMP_SRLNO = HYD_PUMP_SRLNO_;
		//	this.STEERING_MOTOR = STEERING_MOTOR_;
		//	this.STEERING_MOTOR_SRLNO = STEERING_MOTOR_SRLNO_;
		//	this.STEERING_ASSEMBLY = STEERING_ASSEMBLY_;
		//	this.STEERING_ASSEMBLY_SRLNO = STEERING_ASSEMBLY_SRLNO_;
		//	this.STERING_CYLINDER = STERING_CYLINDER_;
		//	this.STERING_CYLINDER_SRLNO = STERING_CYLINDER_SRLNO_;
		//	this.RADIATOR = RADIATOR_;
		//	this.RADIATOR_SRLNO = RADIATOR_SRLNO_;
		//	this.CLUSSTER = CLUSSTER_;
		//	this.CLUSSTER_SRLNO = CLUSSTER_SRLNO_;
		//	this.ALTERNATOR = ALTERNATOR_;
		//	this.ALTERNATOR_SRLNO = ALTERNATOR_SRLNO_;
		//	this.STARTER_MOTOR = STARTER_MOTOR_;
		//	this.STARTER_MOTOR_SRLNO = STARTER_MOTOR_SRLNO_;
		//	this.FTTYRE2 = FTTYRE2_;
		//	this.RTRIM1 = RTRIM1_;
		//	this.RTTYRE1 = RTTYRE1_;
		//	this.RTRIM2 = RTRIM2_;
		//	this.RTTYRE2 = RTTYRE2_;
		//	this.FTRIM1 = FTRIM1_;
		//	this.FTTYRE1 = FTTYRE1_;
		//	this.FTRIM2 = FTRIM2_;
		//	this.FINAL_LABEL_DATE = FINAL_LABEL_DATE_;
		//	this.BACKEND = BACKEND_;
		//	this.BACKEND_SRLNO = BACKEND_SRLNO_;
		//	this.SIM_SERIAL_NO = SIM_SERIAL_NO_;
		//	this.IMEI_NO = IMEI_NO_;
		//	this.MOBILE = MOBILE_;
		//	this.ROPS_SRNO = ROPS_SRNO_;
		//	this.PDIOKDATE = PDIOKDATE_;
		//	this.PDIDONEBY = PDIDONEBY_;
		//	this.OIL = OIL_;
		//	this.SWAPCAREBTN = SWAPCAREBTN_;
		//	this.FIPSRNO = FIPSRNO_;
		//	this.REMARKS = REMARKS_;
		//	this.CAREBUTTONOIL = CAREBUTTONOIL_;
		//	this.LABELPRINTED = LABELPRINTED_;
		//	this.ImportExcel = ImportExcel_;
		//	this.IsOverride = IsOverride_;
		//}
	}
}