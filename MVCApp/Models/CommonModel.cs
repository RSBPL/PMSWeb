using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class CommonModel
    {

    }

    public class DDLTextValue
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class EditSQN
    {
        public int AutoId { get; set; }
        public int Qty { get; set; }
        public int Sqn { get; set; }
    }

    
}