using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class SettingModel
    {
        [Required(ErrorMessage = "Required")]
        public string FixedPlanDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string RollaingPlanDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string HydraulicDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string EngineDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ReareAxleDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string TransmissionDays { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Engine_ShiftA { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Engine_ShiftB { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Engine_ShiftC { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Transmission_ShiftA { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Transmission_ShiftB { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Transmission_ShiftC { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ReareAxle_ShiftA { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ReareAxle_ShiftB { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ReareAxle_ShiftC { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Hydraulic_ShiftA { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Hydraulic_ShiftB { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Hydraulic_ShiftC { get; set; }

    }

    public class Planning_Days_Model
    {
        public string Plant_PD { get; set; }
        public string Family_PD { get; set; }
        public string Description_PD { get; set; }
        public string ParamInfo_PD { get; set; }
        public string ParamValue_PD { get; set; }
        public string Status_PD { get; set; }        
    }

    public class Production_Capacity_Model
    {
        public string Plant_PC { get; set; }
        public string Family_PC { get; set; }
        public string Description_PC { get; set; }
        public string Shift_PC { get; set; }
        public string ParamInfo_PC { get; set; }
        public string ParamValue_PC { get; set; }
        public string Status_PC  { get; set; }
    }
    public class Weekly_Off_Model 
    {
        public string Plant_WO { get; set; }
        public string Family_WO { get; set; }
        public string Description_WO { get; set; }
        public string ParamInfo_WO { get; set; }
        public string ParamValue_WO { get; set; }
        public string Status_WO { get; set; }
    }
}