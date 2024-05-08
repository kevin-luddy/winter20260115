// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public interface IReportsControllerLogic
    {
        /// <summary>
        /// Reusable logic for preparing the "All BOEs" report
        /// </summary>
        /// <param name="workspace">The current workspace</param>
        /// <param name="isSubcontractorUser">Whether the current user is a subcontractor</param>
        /// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
        /// <param name="selectedBOEs">List of BOEs to be included in the report; if null, then include ALL</param>
        /// <param name="viewDataDictionary">View data</param>
        /// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
        /// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
        /// <param name="exportInputs">the export inputs</param>
        /// <param name="boeExportModelViews">the boe export model views</param>
        /// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
        /// <param name="custom">Flag indicating whether the template file is based on the custom export template</param>
        void PrepareAllBOEsReport(FullWorkspace workspace, bool isSubcontractorUser, string summarizeByCustomField, ICollection<int> selectedBOEs,
            ViewDataDictionary viewDataDictionary, out bool isCustomExport, out WorkspaceExportFormatDTO wsExportFormatDTO, out BOEExportInputs exportInputs,
            out ICollection<BOEExportModelView> boeExportModelViews, out List<BOESummaryGridModelView> boeSummaryGridModelViews, bool custom = false);

        /// <summary>
        /// Reusable logic for exporting the "All BOEs" report
        /// </summary>
        /// <param name="workspace">The current workspace</param>
        /// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
        /// <param name="httpResponse">HTTP response object</param>
        /// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
        /// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
        /// <param name="exportInputs">the export inputs</param>
        /// <param name="boeExportModelViews">the boe export model views</param>
        /// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
        /// <param name="segmentedOutput">Should the output be broken into segments and zipped</param>
        void ExportAllBOEsReport(FullWorkspace workspace, ICollection<BoeCustomReportComponent> selectedComponents, HttpResponseBase httpResponse, bool isCustomExport,
            WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, List<BOESummaryGridModelView> boeSummaryGridModelViews,
            bool segmentedOutput = false);

        /// <summary>
        /// Gets a boolen indicating if custom export is support per company configuration
        /// </summary>
        bool SupportCustomExport { get; }

        /// <summary>
        /// Gets a mapping dto of task element to metric names for export.
        /// </summary>
        /// <param name="workspace">Full workspace</param>
        /// <returns>Metric name to task element mappings.</returns>
        MetricNameTaskElementMappingDTO GetMetricNameTaskElementMappingDTO(FullWorkspace workspace);

		/// <summary>
		/// Generates Data for the Boe Discrepancy Report. Used by both the page and also Excel export..
		/// 
		/// This is cached for 5 minutes in case the user wants to export the data, so that way we don't have to keep rerunning all the data..
		/// </summary>
		/// <param name="ws">workspace</param>
		/// <param name="clearCache">Should be cleared when the web report is being displayed, to get a clean view; for the export we should be using cache</param>
		/// <param name="processResources">Should Resources be processed for missing Resources/BRCs? 
		///		This should typically only be true when running export to ProPricer or All BOEs Report as additional validation</param>
		/// <returns>Data for the Boe Discrepancy Report</returns>
		ICollection<BoeDiscrepancyReportModelView> GenerateDataForBoeDiscrepancyReport(FullWorkspace ws, bool clearCache, bool processResources = false);
        
        /// <summary>
        /// Performs Validation for DisplayInlFormExportGrid
        /// </summary>
        /// <param name="ws">FullWorkspace</param>
        /// <returns>ICollection of ValidationMessage.</returns>
        ICollection<ValidationMessage> DisplayInlFormExportGrid_Validate(FullWorkspace ws);

        /// <summary>
        /// Get the Summary Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Summary Reports Model Views</returns>
        ICollection<SSRSReportsModelView> GetSummaryReportsModelViews();

        /// <summary>
        /// Get the Customer Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Customer Reports Model Views</returns>
        ICollection<SSRSReportsModelView> GetCustomerReportsModelViews();

        /// <summary>
        /// Get the Finance Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Finance Reports Model Views</returns>
        ICollection<SSRSReportsModelView> GetFinanceReportsModelViews();

        /// <summary>
        /// Get the Additional Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Additional Reports Model Views</returns>
        ICollection<SSRSReportsModelView> GetAdditionalReportsModelViews();

        /// <summary>
        /// Gets the ssrs report URL.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>The Uri for the SSRS report.</returns>
        Uri GetReportUrl(Reports reportType);

        /// <summary>
        /// Determine if the AllBOEs export is using the template that allows summarization by custom fields.
        /// </summary>
        /// <param name="templateName">Name of Output Template being used by the AllBOEs export.</param>
        /// <returns>true, if the AllBOEs export is using the summary by custom fields template; false otherwise.</returns>
        bool IsUsingSummarizeByCustomFieldTemplate(string templateName);

        /// <summary>
        /// Get the set of "Summarize By" options when using the template that allows summarization by custom fields.
        /// </summary>
        /// <param name="customFields"></param>
        /// <returns>A list of options including "None" plus all resource level custom field names for the workspace.</returns>
        ICollection<SelectListItem> SummarizeByCustomFieldOptions(IReadOnlyCollection<CustomFieldDTO> customFields);

        /// <summary>
        /// Check if the PTM data is out of sync
        /// Data is out of sync if
        /// -Integration with PTM is turned off
        /// -Workspace is not linked to a PTM record
        /// -Any PTM data is out of date
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        ICollection<string> GetPtmDataOutOfSyncMessages(FullWorkspace ws);

        /// <summary>
        /// Get the BOE Export Inputs used for the BOE Status and BOE/WBS Reports
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <returns>BOE Export Inputs</returns>
        BOEExportInputs GetExportInputsForStatusAndWbsReports(FullWorkspace ws);
        
        /// <summary>
        /// Generates a ModelView for the WBS/BOE Report
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// WBS/BOE Report Modelview
        /// </returns>
        ICollection<BoeWbsReportModelView> GenerateWbsBoeReport(BOEExportInputs exportInputs);
        
        /// <summary>
        /// Export the WBS/BOE Report for the given workspace
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="fileLocation">Location of the template file</param>
        /// <param name="reportModelView">Report model view</param>
        /// <param name="exportInputs">export inputs</param>
        /// <returns>Report filename</returns>
        string ExportWbsBoeReport(FullWorkspace ws, string fileLocation, ICollection<BoeWbsReportModelView> reportModelView, BOEExportInputs exportInputs);
    }
}