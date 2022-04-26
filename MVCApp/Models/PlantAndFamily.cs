using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class PlantAndFamily
    {
        public string PlantCode { get; set; }
        public string PlantName { get; set; }
        public string PlantAddress { get; set; }
        public string PlantPhone { get; set; }
        public string PlantEmail { get; set; }
        public string PlantFax { get; set; }
        public string Remarks { get; set; }
        public string FamilyCode { get; set; }
        public string FamilyName { get; set; }
        public string Description { get; set; }
        public string NoOfStages { get; set; }
        public string ORGId { get; set; }
        public bool NotValidateJob { get; set; }
    }
}