// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web
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

            bundles.Add(new StyleBundle("~/Content/genCss").Include(
                "~/Resources/css/gen2.0.css"));

			bundles.Add(new StyleBundle("~/Content/laborTaskCss").Include(
				"~/Resources/css/laborTask.css"));

			bundles.Add(new StyleBundle("~/Content/homeCss").Include(
                "~/Content/bootstrap.css",
                "~/Resources/css/generation.css",
                "~/Content/jquery-ui.css"));  // jquery ui

            bundles.Add(new StyleBundle("~/Content/siteCss").Include(
                "~/Content/bootstrap.css",
                "~/Resources/css/generation.css",
                "~/Content/jquery-ui.css",  // jquery ui
                "~/Resources/css/EditBoe.css",
                "~/Resources/css/jquery.multiselect.css",
                "~/Content/ui-grid.css",
                "~/Content/font-awesome.css",
                "~/Content/angular-ui-tree.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/home").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery-migrate-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Resources/js/json2.js",
                "~/Resources/js/iBOE.js",
                "~/Resources/js/ImageToggler.js",
                "~/Resources/js/Generation.js",
                "~/Resources/js/GenerationInit.js",
                "~/Resources/js/ActiveDirectorySearch.js",
                "~/Resources/js/UserLookupWidget.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/additional-methods.js",
                "~/Scripts/jquery.validate.unobtrusive.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/genboe").Include(
                "~/Scripts/tinymce/tinymce.js",
                "~/Scripts/ui-tinymce.js",
                "~/Scripts/ui-grid.js", 
                "~/Scripts/angular-sanitize.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-ui/sortable.js",
                "~/Resources/js/angular/directives.js",
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/Resources/js/angular/MoqEquationApp.js",
                "~/Scripts/angular-cookies.js",
                "~/Resources/js/angular/genBoe.js",
                "~/Resources/js/angular/metricsController.js",
                "~/Scripts/angularjs-dropdown-multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/systemAdmin").Include(
                "~/Scripts/tinymce/tinymce.js",
                "~/Scripts/ui-tinymce.js",
                "~/Scripts/ui-grid.js",
                "~/Scripts/angular-sanitize.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-ui/sortable.js",
                "~/Resources/js/angular/directives.js",
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/Resources/js/angular/MoqEquationApp.js",
                "~/Scripts/angular-cookies.js",
                "~/Resources/js/angular/genBoe.js",
                "~/Resources/js/angular/systemEmailController.js",
                "~/Resources/js/angular/manageSystemSettingsController.js",
                "~/Resources/js/angular/manageOverdueTrainingController.js",
                "~/Scripts/angularjs-dropdown-multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery-migrate-{version}.js",
                "~/Resources/js/Scripts/jquery.multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/angular-sanitize.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/tinymce/tinymce.js",
                "~/Scripts/ui-tinymce.js",
                "~/Scripts/ui-grid.js",
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/Scripts/angular-ui/sortable.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/angular-cookies.js",
                "~/Scripts/angularjs-dropdown-multiselect.js",
                "~/Resources/js/json2.js",
                "~/Resources/js/iBOE.js",
                "~/Resources/js/ImageToggler.js",
                "~/Resources/js/Generation.js",
                "~/Resources/js/GenerationInit.js",
                "~/Resources/js/ActiveDirectorySearch.js",
                "~/Resources/js/UserLookupWidget.js",
                "~/Resources/js/Boe.js",
                "~/Resources/js/BoeTravel.js",
                "~/Resources/js/BoeZoneTravel.js",
                "~/Resources/js/BoeODC.js",
                "~/Resources/js/BoeLabor.js",
                "~/Resources/js/angular/MoqEquationApp.js",
                "~/Resources/js/angular/genBoe.js",
                "~/Resources/js/angular/directives.js",
                "~/Resources/js/angular/ProjectMap/*.js",
                "~/Resources/js/angular/workspaceEmailController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/results").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery-migrate-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Resources/js/json2.js",
                "~/Resources/js/iBOE.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/verification").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Resources/js/json2.js",
                "~/Resources/js/Generation.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/permission").Include(
                "~/Resources/js/angular/permissionController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/createWorkspace").Include(
                "~/Resources/js/angular/createWorkspaceController.js",
                "~/Resources/js/Boe.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/workspace").Include(
                "~/Resources/js/angular/workspaceHomeController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/manageCLIN").Include(
                "~/Resources/js/angular/manageCLINController.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/dateshift").Include(
                "~/Resources/js/iBOE.js", 
                "~/Resources/js/angular/dateShiftController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/manageWBS").Include(
                "~/Resources/js/angular/manageWBSController.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/calculateActuals").Include(
				"~/Resources/js/angular/workspaceCalculateActualsController.js"
				));

			bundles.Add(new ScriptBundle("~/bundles/manageBOE").Include(
                "~/Resources/js/angular/manageBOEController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/manageTask").Include(
                "~/Scripts/bignumber.js",
                "~/Resources/js/angular/UtilityService.js",
                "~/Resources/js/angular/manageTaskController.js",
                "~/Resources/js/angular/MoqEquationController.js",
                "~/Resources/js/angular/InsertWorkspaceVariableController.js"
                )); 

            bundles.Add(new ScriptBundle("~/bundles/boebulksubmit").Include(
                "~/Resources/js/angular/boeBulkSubmitController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/manageRTETemplates").Include(
                "~/Resources/js/angular/rteTemplatesController.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/discrepancy").Include(
                "~/Resources/js/DiscrepancyValidator.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/ProPricer").Include(
                "~/Resources/js/angular/UtilityService.js",
                "~/Resources/js/angular/ProPricerController.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/confidenceReport").Include(
				"~/Resources/js/angular/boeConfidenceReportController.js"
				));

            bundles.Add(new ScriptBundle("~/bundles/initialization").Include(
                "~/Resources/js/Initialization.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/identification").Include(
                "~/Resources/js/Workspace/WorkspaceIdentification.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/angularapp").Include(
                "~/Scripts/angular.min.js"
                ));

            // Uncomment this for debugging bundles to ensure bundles work
            // BundleTable.EnableOptimizations = true;
        }
    }
}
