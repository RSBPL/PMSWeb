using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class Message
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string MsgType { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string MessageBox { get; set; }
        public string AutoId { get; set; }

    }
}