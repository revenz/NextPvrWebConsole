using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Models.DbHelper.Test();

            NUtility.Logger.SetLogFileName(@"logs\reven.log");

            AreaRegistration.RegisterAllAreas();

            // Register CustomRegularExpressionValidator
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.DirectoryAttribute), typeof(Validators.DirectoryValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.EmailAttribute), typeof(Validators.EmailValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.UsernameAttribute), typeof(Validators.UsernameValidator));


            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));

            Workers.DeviceWatcher watcherDevice = new Workers.DeviceWatcher();
            //watcherDevice.Start(); // getting silly sqlite lock issues with this... i hate sqlite....
            watcherDevice.TunerStatusUpdated += delegate(Workers.DeviceUpdateEvent[] Events)
            {
                Hubs.NextPvrEventHub.Clients_DeviceStatusUpdated(Events);
            };

            Models.Configuration config = new Models.Configuration();
        }

        public void Application_BeginRequest()
        {
            string url = Request.Url.ToString().ToLower();
            if (!Regex.IsMatch(url, "/(setup|bundle|scripts|content|languages)"))
            {
                Models.Configuration config = new Models.Configuration();
                if (config.FirstRun)
                    HttpContext.Current.Response.Redirect("~/Setup");
            }
        }
    }
}