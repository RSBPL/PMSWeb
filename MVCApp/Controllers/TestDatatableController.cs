using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MVCApp.Controllers
{

    public class TestData
    {
        public string jsondata { get; set; }
        public string columns { get; set; }
        public string data { get; set; }
    }
    public class columnsinfo
    {
        public string title { get; set; }
        public string data { get; set; }
    }
    public class TestDatatableController : Controller
    {
        Function fun = new Function();
        public ActionResult Index()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            TestData t = new TestData();
            List<columnsinfo> _col = new List<columnsinfo>();

            DataTable dt = new DataTable();

           string query = string.Format(@"SELECT * from xxes_supermkt_locations where PLANT_CODE = 'T02' AND FAMILY_CODE = 'TRACTOR EKI'");
            dt = fun.returnDataTable(query);

            

            for (int i = 0; i <= dt.Columns.Count - 1; i++)
            {
                _col.Add(new columnsinfo { title = dt.Columns[i].ColumnName, data = dt.Columns[i].ColumnName });
            }

            string col = (string)serializer.Serialize(_col);
            t.columns = col;


            var lst = dt.AsEnumerable()
            .Select(r => r.Table.Columns.Cast<DataColumn>()
                    .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                   ).ToDictionary(z => z.Key, z => z.Value)
            ).ToList();

            string data = serializer.Serialize(lst);
            t.data = data;

            return View(t);
        }

       
    }
}