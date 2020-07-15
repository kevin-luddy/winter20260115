// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web
{
    using System;
    using System.Web.Optimization;
    using IES.Common;

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

            var bundle = new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/modernizr-{version}.js");

            bundle.Orderer = new AsIsBundleOrderer();
            bundles.Add(bundle);

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/angular-ui-tree.css",
                "~/Content/jquery-ui.css",
                "~/Content/angular-material.css",
                "~/Content/ui-grid.css",
                "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/Scripts/angular-ui/sortable.js",
                "~/Scripts/rdm.js",
                "~/app/RDMapp.js",
                "~/app/Admin/*.js",
                "~/app/Components/*.js",
                "~/app/FileAttachment/*.js",
                "~/app/Home/*.js",
                "~/app/Menus/*.js",
                "~/app/PPRD/*.js",
                "~/app/Rate/*.js",
                "~/app/Reports/*.js",
                "~/app/Services/*.js",
                "~/app/Version/*.js",
                "~/Scripts/ui-grid.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularapp").Include(
                     "~/Scripts/angular.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                     "~/Scripts/angular-animate.min.js",
                     "~/Scripts/angular-aria.min.js",
                     "~/Scripts/angular-material.min.js",
                     "~/Scripts/angular-ui-tree.min.js",
                     "~/Scripts/angular-sanitize.min.js",
                     "~/Scripts/csv.js"));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                     "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new StyleBundle("~/Content/datepicker").Include(
               "~/Content/bootstrap-datepicker.min.css"));

            bundles.Add(new ScriptBundle("~/Scripts/tinymce/tinymceBundle").Include(
                     "~/Scripts/tinymce/tinymce.min.js",
                     "~/Scripts/tinymce/ui-tinymce.min.js"));
        }
    }
}
