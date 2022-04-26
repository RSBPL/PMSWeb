using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class Store
    {
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string Remarks { get; set; }
        public string AutoId { get; set; }
        public string LocationName { get; set; }
        public string Remarks1 { get; set; }
        public string PlantStore { get; set; }
        public string FamilyStore { get; set; }
        public string PlantSuper { get; set; }
        public string FamilySuper { get; set; }

        public string ZONE { get; set; }
        public string SuperMarket { get; set; }
        public string FromRange { get; set; }
        public string ToRange { get; set; }
        public string QTY { get; set; }
        public string Search { get; set; }
        public string CAPACITY { get; set; }
        

    }
    public class LayoutSuperMktSubList
    {
        public List<Store> SuperMktLayoutMode { get; set; }
        public string Search { get; set; }
    }
}