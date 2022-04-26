using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class RoleModel
    {
        public string AutoId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Role { get; set; }

        //public string Remark { get; set; }
        //public Nullable<bool> IsActive { get; set; }
        //public string CreatedBy { get; set; }
        //public Nullable<System.DateTime> CreatedDate { get; set; }
        //public string UpdatedBy { get; set; }
        //public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}