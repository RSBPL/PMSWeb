using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class StageMsterModel
    {
        public string PLANT_CODE { get; set; }
        public string FAMILY_CODE { get; set; }
        public string STAGE_ID { get; set; }
        public string STAGE_DESCRIPTION { get; set; }
        public string OFFLINEITEMS { get; set; }
        public string PRODUCT_TYPE { get; set; }
        public bool START_STAGE { get; set; }
        public bool END_STAGE { get; set; }
        public bool PRINT_LABEL { get; set; }
        public string PRINT_LABEL_QUANTITY { get; set; }
        public bool SHOW_STAGE_FORM { get; set; }
        public string STAGE_1_NAME { get; set; }
        public string STAGE_2_NAME { get; set; }
        public string STAGE_3_NAME { get; set; }
        public bool SCAN_JOB { get; set; }
        public string SCAN_JOB_KEY_STOKES { get; set; }
        public bool SCAN_SERIAL { get; set; }
        public string SCAN_SERIAL_KEY_STOKES { get; set; }
        public bool SCAN_SUB_ASSEMBLY_1 { get; set; }
        public string SCAN_SUB_ASSEMBLY_1_KEY_STOKES { get; set; }
        public bool SCAN_SUB_ASSEMBLY_2 { get; set; }
        public string SCAN_SUB_ASSEMBLY_2_KEY_STOKES { get; set; }
        public bool SCAN_SUB_ASSEMBLY_3 { get; set; }
        public string SCAN_SUB_ASSEMBLY_3_KEY_STOKES { get; set; }
        public bool ONLINE_SCREEN { get; set; }
        public bool START_FIFO { get; set; }
        public bool END_FIFO { get; set; }
        public bool MOVE_COMPLETE { get; set; }
        public bool DELAY_TIME { get; set; }
        public bool ISBIG { get; set; }
        public string OFFLINE_KEYCODE { get; set; }
        public string IPADDR { get; set; }
        public string IPPORT { get; set; }
        public string AD_USER { get; set; }
        public string AD_PASSWORD { get; set; }
        public string MEDIA { get; set; }
        public string AUTOID { get; set; }

      

    }
}