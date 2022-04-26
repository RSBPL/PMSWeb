using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCApp.Models
{
    public class OtherSuffix
    {
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "HH:mm ")]
        [DataType(DataType.Time)]
        public string StartTime { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "HH:mm ")]
        [DataType(DataType.Time)]
        public string EndTime { get; set; }
        public string Plant { get; set; }
        public string PlantExport { get; set; }
       
        public string MonthYear { get; set; }
        
        public string MonthYearExport { get; set; }
        public string MyCode { get; set; }

        public string MyCodeExport { get; set; }
        public string Type { get; set; }
        public string TypeExport { get; set; }
        public string AutoId { get; set; }

        public string TyreName { get; set; }

        public string PARAMVALUE { get; set; }

        public string BatteryName { get; set; }

        public string ShiftCode { get; set; }
            
        public bool NightExist { get; set; }
        public string PlantBattery { get; set; }

        public string BatteryDecode { get; set; }

        public string DammySrNo { get; set; }

        public string BatterySrl { get; set; }
        public string PlantSrl { get; set; }

        public string PlantFcode { get; set; }
        public string FamilyFcode { get; set; }
        public string Fcode { get; set; }
        public string Tyre { get; set; }



    }
}