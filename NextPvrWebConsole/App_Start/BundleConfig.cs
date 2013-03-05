using System;
using System.Web;
using System.Web.Optimization;

namespace NextPvrWebConsole
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/jscore").Include(
                        "~/Scripts/core/jquery-{version}.js",
                        "~/Scripts/core/jquery.unobtrusive-ajax",
                        "~/Scripts/core/jquery.validate*",
                        "~/Scripts/core/jquery.unobtrusive*",
                        "~/Scripts/core/jquery.validate.unobtrusive",
                        "~/Scripts/core/jquery.i18n.js",
                        "~/Scripts/translator.js",
                        "~/Scripts/core/jquery.signalR-{version}.js",
                        //"~/Scripts/core/linq.js",
                        //"~/Scripts/core/jquery.linq.js*",
                        "~/Scripts/core/jquery.dateFormat-1.0.js",
                        "~/Scripts/core/angular.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/functions.js",
                        "~/Scripts/apihelper.js",
                        "~/Scripts/global.js")
                        .IncludeDirectory("~/Scripts/controllers", "*.js", true)
                        .IncludeDirectory("~/Scripts/addons", "*.js", true)
                        .Include("~/Scripts/jqueryui/core/jquery.ui.core.js")
                        .Include("~/Scripts/jqueryui/core/jquery.ui.widget.js")
                        .Include("~/Scripts/jqueryui/core/jquery.ui.position.js")
                        .Include("~/Scripts/jqueryui/core/jquery.ui.mouse.js")
                        .IncludeDirectory("~/Scripts/jqueryui/addons", "*.js", true)
                        .Include("~/Scripts/webapp.js")
                        .IncludeDirectory("~/Scripts/directives", "*.js", true)
                        );

            bundles.Add(new ScriptBundle("~/bundles/loginjs").Include(
                        "~/Scripts/core/jquery-{version}.js",
                        "~/Scripts/core/jquery-ui-{version}.js",
                        "~/Scripts/core/jquery.unobtrusive-ajax",
                        "~/Scripts/core/jquery.validate*",
                        "~/Scripts/core/jquery.unobtrusive*",
                        "~/Scripts/core/jquery.validate.unobtrusive",
                        "~/Scripts/core/jquery.i18n.js",
                        "~/Scripts/translator.js",
                        "~/Scripts/functions.js",
                        "~/Scripts/global.js"));

            bundles.Add(new ScriptBundle("~/bundles/setup").Include(
                        "~/Scripts/core/jquery-{version}.js",
                        "~/Scripts/core/jquery-ui-{version}.js",
                        "~/Scripts/core/jquery.unobtrusive-ajax",
                        "~/Scripts/core/jquery.validate*",
                        "~/Scripts/core/jquery.unobtrusive*",
                        "~/Scripts/core/jquery.validate.unobtrusive",
                        "~/Scripts/core/jquery.i18n.js",
                        "~/Scripts/translator.js",
                        "~/Scripts/setup/*.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/charting").IncludeDirectory("~/Scripts/charting", "*.js"));

            var cssBundle = new Bundle("~/Content/css").Include(
                                                        "~/Content/bootstrap/bootstrap.less",
                                                        "~/Content/site.less", 
                                                        "~/Content/custom.css",
                                                        "~/Content/buttons.css",
                                                        "~/Content/glyphicons.less"
                                                        )
                                                        .IncludeDirectory("~/Content/addons", "*.less", true)
                                                        .IncludeDirectory("~/Content/addons", "*.css", true)
                                                        .IncludeDirectory("~/Content/pages", "*.less", true)
                                                        .IncludeDirectory("~/Content/pages", "*.css", true)
                                                        //.Include("~/Content/jqueryui/theme.less")
                                                        ;
            cssBundle.Transforms.Add(new LessTransform());
            cssBundle.Transforms.Add(new CssMinify());
            bundles.Add(cssBundle);

            var cssBootstrap = new Bundle("~/Content/themes/bootstrap/css").Include("~/Content/themes/bootstrap/theme.less");
            cssBootstrap.Transforms.Add(new LessTransform());
            cssBootstrap.Transforms.Add(new CssMinify());
            bundles.Add(cssBootstrap);

            var cssMobile = new Bundle("~/Content/mobile/css").Include(
                        "~/Content/mobile/jquery.mobile-{version}.css",
                        //"~/Content/mobile/jquery.mobile-structure-{version}.css",
                        "~/Content/mobile/nextpvr.css")
                        .IncludeDirectory("~/Content/mobile/core", "*.css")
                        .IncludeDirectory("~/Content/mobile/core", "*.less");
            cssMobile.Transforms.Add(new LessTransform());
            cssMobile.Transforms.Add(new CssMinify());
            bundles.Add(cssMobile);

            bundles.Add(new ScriptBundle("~/bundles/mobile/js").Include(
                    "~/Scripts/core/jquery-{version}.js",
                    "~/Scripts/core/jquery.mobile-{version}.js",
                    "~/Scripts/core/knockout-{version}.js",
                    "~/Scripts/core/angular.js",
                    "~/Scripts/core/jquery.i18n.js",
                    "~/Scripts/core/linq.js",
                    "~/Scripts/core/jquery.linq.js*",
                    "~/Scripts/core/jquery.dateFormat-1.0.js",
                    "~/Scripts/functions.js",
                    "~/Scripts/apihelper.js")
                    .IncludeDirectory("~/Scripts/mobile/core", "*.js"));

            PageBundle(bundles, "Dashboard");
            PageBundle(bundles, "Guide");
            PageBundle(bundles, "Recordings");
            PageBundle(bundles, "UserSettings");
            PageBundle(bundles, "Configuration");
            PageBundle(bundles, "Account");
            PageBundle(bundles, "Setup");
            PageBundle(bundles, "System");
            PageBundle(bundles, "Search");

            // this allows bundling when in debug mode
#if(!DEBUG)
            BundleTable.EnableOptimizations = true;   
#endif
        }

        private static void PageBundle(BundleCollection bundles, string Name)
        {
            try
            {
                var lessBundle = new Bundle("~/Content/{0}/css".FormatStr(Name)).IncludeDirectory("~/Content/{0}".FormatStr(Name), "*.less").IncludeDirectory("~/Content/{0}".FormatStr(Name), "*.css");
                lessBundle.Transforms.Add(new LessTransform());
                lessBundle.Transforms.Add(new CssMinify());
                lessBundle.Orderer = new BundleTransformer.Core.Orderers.NullOrderer();
                bundles.Add(lessBundle);
            }
            catch (Exception) { /* throws exception if directory doesn't exist */ }

            try
            {
                var jsBundle = new ScriptBundle("~/Scripts/{0}/js".FormatStr(Name)).IncludeDirectory("~/Scripts/{0}".FormatStr(Name), "*.js");
                bundles.Add(jsBundle);
            }
            catch (Exception) { /* throws exception if directory doesn't exist */ }
        }
    }
}