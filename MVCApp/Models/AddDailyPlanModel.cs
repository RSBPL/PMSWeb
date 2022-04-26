using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class AddDailyPlanModel
    {
        public string Date { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Shift { get; set; }
        public int TotalQty { get; set; }
        public string Plant { get; set; }
        public string ImportExcel { get; set; }
        public string Family { get; set; }
        public string ItemCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public int Qty { get; set; }
        public int Seq { get; set; }
        public string Tyres { get; set; }
        public string ModelType { get; set; }
        public string SeqForPerticularNo { get; set; }

        public string BackendItemFcodes { get; set; }

        public int AutoId { get; set; }
        public int PlanId { get; set; }
        public int TargetSeq { get; set; }
        public bool IsOverride { get; set; }
        //public int TargetAutoId { get; set; } 
        public string Item_desc { get; set; }
    }
}