// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.Web
{
    using System;
    using System.Web.Optimization;
    using IES.Common;

    /// <summary>
    /// Configuration for the Bundles.
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

            var bundle = new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.multiselect.js",
                        "~/Scripts/modernizr-{version}.js");

            bundle.Orderer = new AsIsBundleOrderer();
            bundles.Add(bundle);

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/angular-gantt.css",
                      "~/Content/angular-gantt-plugins.css",
                      "~/Content/angular-gantt-table-plugin.css",
                      "~/Content/angular-slider-easy.css",
                      "~/Content/jquery.multiselect.css",
                      "~/Content/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/moment.js",
                "~/Scripts/angular-cookies.js",
                "~/Scripts/angular-moment.js",
                "~/Scripts/angular-gantt.js",
                "~/Scripts/angular-gantt-plugins.js",
                "~/Scripts/angular-gantt-table-plugin.js",
                "~/Scripts/Chart.js",
                "~/Scripts/rpm.js",
                "~/Scripts/angular-chart.js",
                "~/app/Components/directives.js",
                "~/app/RPMapp.js",
                "~/app/Metrics/MetricsBarChartController.js",
                "~/app/Metrics/MetricsController.js",
                "~/app/Metrics/MetricsPieChartController.js",
                "~/app/Metrics/MetricsROSBarChartController.js",
                "~/app/OTIS/OTISController.js",
                "~/app/PTM/PTMController.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularapp").Include(
                     "~/Scripts/angular.min.js"));
        }
    }
}
