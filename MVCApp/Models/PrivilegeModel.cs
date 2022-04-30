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
    }
}