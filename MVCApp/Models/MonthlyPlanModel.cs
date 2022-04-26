using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace MVCApp.Models
{
    public class MonthlyPlanModel
    {
        public string Date { get; set; }
        public string DateExcel { get; set; }
        public string Year { get; set; }
        public string YearExcel { get; set; }
        public string Month { get; set; } 
        public string MonthExcel { get; set; }
       
        public string Plant { get; set; }
        public string PlantExcel { get; set; }
        public string Family { get; set; }
        public string FamilyExcel { get; set; }
        public string ItemCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public int Qty { get; set; }
        
        public int AutoId { get; set; }
      
        public bool IsOverride { get; set; } 
       
    }
}