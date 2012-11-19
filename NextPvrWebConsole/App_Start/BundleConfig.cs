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
                        "~/Scripts/core/jquery-ui-{version}.js",
                        "~/Scripts/core/jquery.unobtrusive-ajax",
                        "~/Scripts/core/jquery.validate*",
                        "~/Scripts/core/jquery.unobtrusive*",
                        "~/Scripts/core/jquery.validate.unobtrusive",
                        "~/Scripts/core/jquery.i18n.js",
                        "~/Scripts/translator.js",
                        "~/Scripts/core/jquery.signalR-{version}.js",
                        "~/Scripts/core/linq.js",
                        "~/Scripts/core/jquery.linq.js*",
                        "~/Scripts/core/jquery.dateFormat-1.0.js",
                        "~/Scripts/core/knockout-{version}.js",
                        "~/Scripts/core/knockout-sortable.js",
                        "~/Scripts/core/jquery.easing.1.3.js",
                        "~/Scripts/core/jquery.metadata.js",
                        "~/Scripts/core/jquery.ibutton.js",
                        "~/Scripts/core/jquery.timespinner.js",
                        "~/Scripts/addons/toastr.js",
                        "~/Scripts/vtabs.js",
                        "~/Scripts/functions.js",
                        "~/Scripts/apihelper.js",
                        "~/Scripts/global.js"));

            bundles.Add(new ScriptBundle("~/bundles/loginjs").Include(
                        "~/Scripts/core/jquery-{version}.js",
                        "~/Scripts/core/jquery-ui-{version}.js",
                        "~/Scripts/core/jquery.unobtrusive-ajax",
                        "~/Scripts/core/jquery.validate*",
                        "~/Scripts/core/jquery.unobtrusive*",
                        "~/Scripts/core/jquery.validate.unobtrusive",
                        "~/Scripts/core/jquery.i18n.js",
                        "~/Scripts/core/jquery.ibutton.js",
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

            bundles.Add(new ScriptBundle("~/bundles/api.js").IncludeDirectory("~/Scripts/api-wrappers", "*.js"));

            bundles.Add(new ScriptBundle("~/bundles/charting").IncludeDirectory("~/Scripts/charting", "*.js"));

            var cssBundle = new Bundle("~/Content/css").Include(
                                                        "~/Content/site.less", 
                                                        "~/Content/custom.css",
                                                        "~/Content/buttons.css",
                                                        "~/Content/addons/toastr.css",
                                                        "~/Content/jquery.ibutton.css");
            cssBundle.Transforms.Add(new LessTransform());
            cssBundle.Transforms.Add(new CssMinify());
            bundles.Add(cssBundle);

            bundles.Add(new StyleBundle("~/Content/themes/bootstrap/css").Include("~/Content/themes/bootstrap/theme.less"));

            bundles.Add(new StyleBundle("~/Content/jquery.mobile/css").Include(
                        "~/Content/jquery.mobile-{version}.css",
                        "~/Content/jquery.mobile-structure-{version}.css",
                        "~/Content/jquery.mobile-theme-{version}.css"));

            PageBundle(bundles, "Dashboard");
            PageBundle(bundles, "Guide");
            PageBundle(bundles, "Recordings");
            PageBundle(bundles, "UserSettings");
            PageBundle(bundles, "Configuration");
            PageBundle(bundles, "Account");
            PageBundle(bundles, "Setup");
            PageBundle(bundles, "System");

            // this allows bundling when in debug mode
            //BundleTable.EnableOptimizations = true;   
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