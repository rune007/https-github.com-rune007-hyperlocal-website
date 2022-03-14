using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HLWebRole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");




            routes.MapRoute(
                "DefaultPaging", // Route name
                "{controller}/{action}/{id}/{pageNumber}" , // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, pageNumber = (int?)null }, new { pageNumber = "[0-9]+" } // Parameter defaults
            );

            //routes.MapRoute(
            //    "AzureTableKeyFields",
            //    "Message/Details/{partitionKey}/{rowKey}",
            //    new { controller = "Message", action = "Details", partitionKey = (string), rowKey = (string)null },
            //    new { partitionKey = "[a-zA-Z0-9\\-]+", rowKey = "[a-zA-Z0-9\\-]+" }
            //);

          
            routes.MapRoute(
                "ControllerDetails", // Route name
                "{controller}/{id}", // URL with parameters
                new { action = "Details", id = (int?)null }, new { id = "[0-9]+" } // Parameter defaults
            );


            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Search", action = "SearchAroundMe", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}