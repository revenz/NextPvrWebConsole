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
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.signalR-{version}.js",
                        "~/Scripts/linq.js",
                        "~/Scripts/jquery.linq.js*",
                        "~/Scripts/jquery.dateFormat-1.0.js",
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/addons/toastr.js",
                        "~/Scripts/vtabs.js",
                        "~/Scripts/functions.js",
                        "~/Scripts/apihelper.js"));

            bundles.Add(new ScriptBundle("~/bundles/api.js").IncludeDirectory("~/Scripts/api-wrappers", "*.js"));

            var cssBundle = new Bundle("~/Content/css").Include(
                                                        "~/Content/site.less", 
                                                        "~/Content/custom.css",
                                                        "~/Content/buttons.css",
                                                        "~/Content/addons/toastr.css");
            cssBundle.Transforms.Add(new LessTransform());
            cssBundle.Transforms.Add(new CssMinify());
            bundles.Add(cssBundle);

            //bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            //            "~/Content/themes/base/jquery.ui.core.css",
            //            "~/Content/themes/base/jquery.ui.resizable.css",
            //            "~/Content/themes/base/jquery.ui.selectable.css",
            //            "~/Content/themes/base/jquery.ui.accordion.css",
            //            "~/Content/themes/base/jquery.ui.autocomplete.css",
            //            "~/Content/themes/base/jquery.ui.button.css",
            //            "~/Content/themes/base/jquery.ui.dialog.css",
            //            "~/Content/themes/base/jquery.ui.slider.css",
            //            "~/Content/themes/base/jquery.ui.tabs.css",
            //            "~/Content/themes/base/jquery.ui.datepicker.css",
            //            "~/Content/themes/base/jquery.ui.progressbar.css",
            //            "~/Content/themes/base/jquery.ui.theme.css"));
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