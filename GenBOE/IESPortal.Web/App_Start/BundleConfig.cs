namespace IESPortal.Web
{
    using System;
    using System.Web.Optimization;
    using IES.Common;

    public static class BundleConfig
    {

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification ="Visual Studio required this even though the argument is validated.")]
        public static void RegisterBundles(BundleCollection bundles)
        {
            if (bundles == null)
            {
                throw new ArgumentNullException(nameof(bundles));
            }

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css",
                "~/Content/banner.css"));

            bundles.Add(new StyleBundle("~/Content/bannerCss").Include(
                "~/Content/banner.css"));

            bundles.Add(new ScriptBundle("~/bundles/banner").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/maintenanceBanner.js"));

            bundles.Add(new StyleBundle("~/Content/appOfflineCss").Include(
                "~/Content/app-offline.css"));

            // bootstrap always has to be in front of jquery ui for dialog css to work correctly!!
            var bundle = new ScriptBundle("~/bundles/adminJquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/respond.js",
                "~/Scripts/iesportal.js");

            bundle.Orderer = new AsIsBundleOrderer();
            bundles.Add(bundle);

            bundles.Add(new StyleBundle("~/Content/adminCss").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css",
                "~/Content/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/app/Portalapp.js",
                "~/app/Components/directives.js",
                "~/app/Controllers/*Controller.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularapp").Include(
                     "~/Scripts/angular.min.js",
                     "~/Scripts/angular-animate.min.js",
                     "~/Scripts/angular-sanitize.min.js"
                     ));
        }
    }
}
