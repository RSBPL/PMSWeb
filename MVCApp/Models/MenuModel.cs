using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCApp.Models  
{
    public class MenuModel
    {
        public int AutoId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MenuCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MenuName { get; set; }       
        public string ControllerName { get; set; }       
        public string ActionName { get; set; }
        public string Rout_Id { get; set; }
        public string Icon { get; set; }
        public string Sequence { get; set; }
    }

    public class SideMenuModel
    {
        public string MCode { get; set; }
        public string Mname { get; set; }
        public string Mcontroller { get; set; }
        public string Maction { get; set; }
        public string Icon { get; set; }
        public string Rout { get; set; }
    }

    public class SideMenuListModel
    {
        public List<SideMenuModel> Menu { get; set; }
        public List<SideMenuModel> SubMenu { get; set; }
        public List<SideMenuModel> SubSubMenu { get; set; }
    }
}