using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MVCApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration) 
        {
            configuration.MapHttpAttributeRoutes();

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );           
        }
    }
}
