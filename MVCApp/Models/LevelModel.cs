using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models 
{
    public class LevelModel
    {
        public string MenuId { get; set; }
        public string MenuName { get; set; }    
        public string MenuController { get; set; }
        public string MenuAction { get; set; }
        public bool IsChecked { get; set; }
    }    

    public class LevelMstList
    {
        public List<LevelModel> FirstLevel { get; set; }
        public List<LevelModel> SecondLevel { get; set; }
        public List<LevelModel> ThirdLevel { get; set; }
    }
}