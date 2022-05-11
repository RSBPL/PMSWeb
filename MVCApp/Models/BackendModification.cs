using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class BackendModification
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Backend { get; set; }
        public string JobId { get; set; }
        public string RearAxleSrno { get; set; }
        public string BackendSrno { get; set; }
        public string TransmissionSrno { get; set; }
        public string HydraulicSrno { get; set; }
        public string runningSrlno { get; set; }
        public string backend_desc { get; set; }
        public string Transmission { get; set; }
        public string RearAxle { get; set; }
        public string Hydraulic { get; set; }
        public string FCODE_ID { get; set; }
        public string IPADDR { get; set; }
        public string IPPORT { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        /*-----------------------------------Reprint---------------------------------------------*/

        public string RPlant { get; set; }
        public string RFamily { get; set; }
        public string RBackendSrno { get; set; }
        public string RRearAxleSrno { get; set; }
        public string RTransmissionSrno { get; set; }
        public string RHydraulicSrno { get; set; }

        /*-----------------------------------PartModification---------------------------------------------*/

        public string MPlant { get; set; }
        public string MFamily { get; set; }
        public string MBackendSrno { get; set; }
        public string MRearAxleSrno { get; set; }
        public string MTransmissionSrno { get; set; }
        public string MHydraulicSrno { get; set; }


    }
}