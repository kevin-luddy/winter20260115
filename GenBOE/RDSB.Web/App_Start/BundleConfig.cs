// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web
{
    using System;
    using System.Web.Optimization;
    using IES.Common;

    /// <summary>
    /// Configuration class for bundling.
    /// </summary>
    public static class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862

        /// <summary>
        /// Registers the bundles.
        /// </summary>
        /// <param name="bundles">The bundles.</param>
        /// <exception cref="System.ArgumentNullException">bundles</exception>
        public static void RegisterBundles(BundleCollection bundles)
        {
            if (bundles == null)
            {
                throw new ArgumentNullException(nameof(bundles));
            }

			Bundle bundle = new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/modernizr-{version}.js");

            bundle.Orderer = new AsIsBundleOrderer();
            bundles.Add(bundle);

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/ui-grid.css"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/rdsb.js",
                "~/app/Components/*.js",
                "~/app/RDSBapp.js",
                "~/app/Document/*.js",
                "~/app/WhosOnline/*.js",
                "~/app/Services/*.js",
                "~/Scripts/ui-grid.js",
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularapp").Include(
                    "~/Scripts/angular.min.js"));
        }
    }
}