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
using System.Reflection;

namespace NextPvrWebConsole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        System.Timers.Timer timer;

        protected void Application_Start()
        {
            Globals.WebConsolePhysicalPath = Server.MapPath("~/");
            Globals.WebConsoleLoggingDirectory = Server.MapPath("~/Logging");
            Globals.NextPvrWebConsoleVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Globals.NextPvrVersion = new Version(NUtility.SettingsHelper.GetInstance().GetSetting("/Settings/Version/CurrentVersion", "0.0.0") + ".0");

            Models.DbHelper.Test();

            //NUtility.Logger.SetLogFileName(@"logs\reven.log");

            AreaRegistration.RegisterAllAreas();

            // Register CustomRegularExpressionValidator
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.DirectoryAttribute), typeof(Validators.DirectoryValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.EmailAttribute), typeof(Validators.EmailValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.UsernameAttribute), typeof(Validators.UsernameValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(Validators.WebsiteAddressAttribute), typeof(Validators.WebsiteAddressValidator));
            
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));

            Workers.DeviceWatcher watcherDevice = new Workers.DeviceWatcher();
            watcherDevice.Start();
            watcherDevice.TunerStatusUpdated += delegate(Workers.DeviceUpdateEvent[] Events)
            {
                Hubs.NextPvrEventHub.Clients_DeviceStatusUpdated(Events);
            };

            Models.Configuration config = new Models.Configuration();
            Logger.Log("Application started.");

            timer_Elapsed(null, null); // it on startup
            timer = new System.Timers.Timer(60 * 60 * 1000);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

        }

        protected void Application_End()
        {
            Logger.Log("Application stopped.");
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Logger.DeleteOldLogFiles();
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