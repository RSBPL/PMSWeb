using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class HookUpAndDown
    {
        public String PLANTCODE { get; set; }
        public String FAMILYCODE { get; set; }
        public String STAGE { get; set; }
        public String ToDate { get; set; }
        public String FromDate { get; set; }
        public String HOOK { get; set; }
        public String HOOK_NO { get; set; }
        public String ITEM_CODE { get; set; }
        public String DESCRIPTION { get; set; }
        public String ENTRYDATE { get; set; }
        public String JOBID { get; set; }
        public String AGEING_DAYS { get; set; }
        public int draw { get; set; }
        public string start { get; set; }
        public string STARTROWINDEX { get; set; }
        public string MAXROWS { get; set; }
        public string P_Search { get; set; }
        public int TOTALCOUNT { get; set; }
        public int length { get; set; }


    }
}