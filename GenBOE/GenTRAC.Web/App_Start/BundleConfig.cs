// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC
{
    using System;
    using System.Web.Optimization;

    /// <summary>
    /// The configuration for Bundles
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

            bundles.Add(new StyleBundle("~/Content/homeCss").Include(
                "~/Content/generation.css",
                "~/Content/jquery-ui.css",
                "~/Content/font-awesome.css",
                "~/Content/genTRAC.css"));

            bundles.Add(new ScriptBundle("~/bundles/home").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery-migrate-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/moment-timezone-with-data-2012-2022.js",
                "~/Scripts/json2.js",
                "~/Scripts/Generation.js",
                "~/Scripts/GenerationInit.js",
                "~/Scripts/GenTRAC.js",
                "~/Scripts/ActiveDirectorySearch.js",
                "~/Scripts/UserLookupWidget.js",
                "~/Scripts/ITCOConfirmationDialog.js",
                "~/Scripts/ChecklistTypeChangeConfirmationDialog.js"));

            // Uncomment this for debugging bundles to ensure bundles work
            // BundleTable.EnableOptimizations = true;
        }
    }
}
