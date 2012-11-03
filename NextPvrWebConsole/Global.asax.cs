using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Net.Http.Formatting;

namespace NextPvrWebConsole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));

            System.Timers.Timer t = new System.Timers.Timer(10 * 1000);
            t.AutoReset = true;
            t.Elapsed += delegate { Hubs.NextPvrEventHub.Clients_ShowErrorMessage("Testing: " + DateTime.Now.ToLongTimeString()); };
            t.Start();

            Workers.DeviceWatcher watcherDevice = new Workers.DeviceWatcher();
            watcherDevice.Start();
            watcherDevice.TunerStatusUpdated += delegate(Workers.DeviceUpdateEvent[] Events)
            {
                Hubs.NextPvrEventHub.Clients_DeviceStatusUpdated(Events);
            };
        }
    }
}