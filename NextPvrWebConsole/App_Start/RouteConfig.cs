using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NextPvrWebConsole
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("DefaultWithOId", "{controller}/{oid}", new { oid = UrlParameter.Optional, action = "Index" }, new { oid = @"^\d+$" });

            routes.MapRoute("Login", "Login", new { action = "Login", controller = "Account" });
            routes.MapRoute("ResetPassword", "ResetPassword", new { action = "ResetPassword", controller = "Account" });

            routes.MapRoute("SearchRoute", "search/{searchText}", new { action = "Index", controller = "Search" });

            routes.MapRoute("StreamRecording", "stream/recording/{oid}", new { action = "recording", controller = "stream" }, new { oid = @"^\d+$" });


            routes.MapRoute("Home", "home", new { action = "Home", controller = "Dashboard" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}