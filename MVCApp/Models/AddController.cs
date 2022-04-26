using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class AddController
    {
        public string Plant { get; set; }
        public string Family { get; set; }
        public string ID { get; set; }
        public bool IsActive { get; set; }
        public string Stage { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public string Mode { get; set; }
        public string ReadingChannel { get; set; }
        public string Remarks { get; set; }
        public string Description { get; set; }
        public string AutoId { get; set; }

    }
}