using System.Web;
using System.Web.Optimization;

namespace MVCApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/page/js").Include(
                "~/assets/js/jquery-2.1.4.min.js",
                "~/Scripts/ej2/ej2.min.js"
                ));

            bundles.Add(new ScriptBundle("~/assets/MyJS").Include(
                       "~/assets/MyJS/MyJS.js"
                ));

            bundles.Add(new ScriptBundle("~/assets/js").Include(
                       "~/assets/js/jquery-2.1.4.min.js",
                       "~/assets/js/bootstrap.min.js",
                       "~/assets/js/excanvas.min.js",
                       "~/assets/js/jquery-ui.custom.min.js",    
                       "~/assets/js/jquery.sparkline.index.min.js",
                       "~/assets/js/jquery.flot.min.js",
                       "~/assets/js/jquery.flot.pie.min.js",
                       "~/assets/js/jquery.flot.resize.min.js", 
                       "~/assets/js/ace-elements.min.js",
                       "~/assets/js/ace.min.js",
                       "~/assets/js/ace-extra.min.js",
                       "~/Scripts/jquery.validate.min.js",
                       "~/Scripts/jquery.validate.unobtrusive.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //ASSETS CSS
            bundles.Add(new StyleBundle("~/assets/css").Include(
                      "~/assets/css/bootstrap.min.css",
                      "~/assets/font-awesome/4.5.0/css/font-awesome.min.css",
                      "~/assets/css/ace-skins.min.css",
                      "~/assets/css/ace-rtl.min.css",
                      "~/assets/css/jquery-ui.custom.min.css",
                      "~/assets/css/CustomCss.css",                             
                      "~/assets/css/ace.min.css"
                ));

            //SYNCFUSION ESSENTIAL JS 2 STYLES
            bundles.Add(new StyleBundle("~/Content/ej2").Include(
                      "~/Content/ej2/compatibility/bootstrap4.css",
                      "~/Content/ej2/compatibility/fabric.css",
                      "~/Content/ej2/compatibility/highcontrast.css",
                      "~/Content/ej2/compatibility/material.css"
                ));

           
        }
    }
}
