// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public interface IReportsControllerLogic
    {
        /// <summary>
        /// Reusable logic for exporting the "All BOEs" report
        /// </summary>
        /// <param name="workspace">The current workspace</param>
        /// <param name="isSubcontractorUser">Whether the current user is a subcontractor</param>
        /// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
        /// <param name="selectedBOEs">List of BOEs to be included in the report; if null, then include ALL</param>
        /// <param name="selectedComponents">List of resources to be included in the report; if null, then include ALL</param>
        /// <param name="viewDataDictionary">?</param>
        /// <param name="httpResponse">The <see cref="HttpResponseBase"/></param>
        /// <param name="custom">?</param>
        /// <returns>Contents of the ALL BOEs report</returns>
        void ExportAllBOEsReport(FullWorkspace workspace, bool isSubcontractorUser, string summarizeByCustomField, ICollection<int> selectedBOEs, ICollection<BoeCustomReportComponent> selectedComponents,
            ViewDataDictionary viewDataDictionary, HttpResponseBase httpResponse, bool custom = false);

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
        /// <returns>Data for the Boe Discrepancy Report</returns>
        ICollection<BoeDiscrepancyReportModelView> GenerateDataForBoeDiscrepancyReport(FullWorkspace ws, bool clearCache);
        
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
    }
}