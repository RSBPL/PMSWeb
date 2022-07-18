using System;
using System.Data;
using System.Linq;
using System.Web;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Oracle.ManagedDataAccess.Client;
using MVCApp.CommonFunction;

namespace MVCApp.Models
{
    public class PrivilegeModel
    {
        [Required(ErrorMessage = "Required")]
        public string RoleId { get; set; } 
        public string EngCode { get; set; } 
        public string EngText { get; set; } 
        public string Type { get; set; } 
        public string ENGINE { get; set; } 
        public string TRACTOR { get; set; } 
        public string CRANE { get; set; } 
        public string EKI { get; set; } 
        public string BACKEND { get; set; } 
        public string HYDRAULIC { get; set; } 
        public string CheckboxCheck { get; set; } 
       
    }
}