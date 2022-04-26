using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class EngineModel
    {
        public string gleSearch { get; set; }
        //public string FUALPUMP_Desc { get; set; }
        //public string FUALPUMP { get; set; }

        public string Engine { get; set; }
        public string Engine_Desc { get; set; }
        public string AUTOID { get; set; }
        public string PLANT_CODE { get; set; }
        public string FAMILY_CODE { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_DESC { get; set; }
        public string CYLINDER_BLOCK { get; set; }
        public string CYLINDER_BLOCK_DESC { get; set; }
        public string CYLINDER_HEAD { get; set; }
        public string CYLINDER_HEAD_DESC { get; set; }
        public string CONNECTING_ROD { get; set; }
        public string CONNECTING_ROD_DESC { get; set; }
        public string CRANK_SHAFT { get; set; }
        public string CRANK_SHAFT_DESC { get; set; }
        public string CAM_SHAFT { get; set; }
        public string CAM_SHAFT_DESC { get; set; }
        public string ECU { get; set; }
        public string ECU_DESC { get; set; }
        public string INJECTOR { get; set; }
        public bool REQUIRE_CYLINDER_BLOCK { get; set; }
        public bool REQUIRE_CYLINDER_HEAD { get; set; }
        public bool REQUIRE_CONNECTING_ROD { get; set; }
        public bool REQUIRE_CRANK_SHAFT { get; set; }
        public bool REQUIRE_CAM_SHAFT { get; set; }
        public bool REQUIRE_ECU { get; set; }
        public string PREFIX_1 { get; set; }
        public string PREFIX_2 { get; set; }
        public string REMARKS1 { get; set; }
        public string FUEL_INJECTION_PUMP { get; set; }
        public string FUEL_INJECTION_PUMP_DESC { get; set; }
        public bool REQ_FUEL_INJECTION_PUMP { get; set; }
        public string NO_OF_PISTONS { get; set; }
        public string NO_OF_INJECTORS { get; set; }
        public string CREATEDBY { get; set; }
        public string CREATEDDATE { get; set; }
        public string UPDATEDBY { get; set; }
        public string UPDATEDDATE { get; set; }
        public string Dcode { get; set; }

        //for checkboxes

    }
}