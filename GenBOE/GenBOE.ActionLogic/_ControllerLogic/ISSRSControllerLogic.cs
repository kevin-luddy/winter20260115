// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.SSRS;
    using GenBOE.Objects;
    using IES.Common;

    public interface ISSRSControllerLogic
    {
        /// <summary>
        /// Get the ModelViews for a Cost Analysis report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="fullWS">The FullWorkspace object.</param>
        /// <param name="reportType">The report type.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<CostAnalysisReportRMSModelView> GetCostAnalysisReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS, SSRSReportType reportType);

        /// <summary>
        /// Gets the offloaded BOEs from a FullWorkspace, ordering them by the standard Project Map sort.
        /// </summary>
        /// <param name="fullWS">The FullWorkspace.</param>
        /// <returns>The offloaded BOEs, ordered by standard Project Map sort.</returns>
        ICollection<FullBoe> GetOffloadBOEsFromFullWorkspace(FullWorkspace fullWS);

        /// <summary>
        /// Gets the ModelViews for a Staffing Curves report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="workspace">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<StaffingCurvesReportModelView> GetStaffingCurvesReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace);

        /// <summary>
        /// Gets model views for both the offload cost by year and offload cost summary reports
        /// </summary>
        /// <param name="offloadedBoes">Collection of BOEs including offloaded BOEs</param>
        /// <param name="fullWS">Full workspace</param>
        /// <returns>Collection of objects used to populate data in offload cost summary and by year reports</returns>
        ICollection<OffloadCostByYearReportRMSModelView> GetOffloadCostByYearReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS);

        /// <summary>
        /// Gets the summary report RMS.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="ws">The workspace.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>A list of model views for the summary report.</returns>
        ICollection<SummaryReportRMSModelView> GetSummaryReportRMSModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace ws, SSRSReportType reportType);

        /// <summary>
        /// Gets model views for the offload cost summary report
        /// </summary>
        /// <param name="offloadCostByYearReportRMSModelViews">model views broken out by year</param>
        /// <returns>consolidated model views</returns>
        ICollection<OffloadCostByYearReportRMSModelView> ConsolidateOffloadCostByYear(ICollection<OffloadCostByYearReportRMSModelView> offloadCostByYearReportRMSModelViews);

        /// <summary>
        /// Gets the Boe Summary report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<BOESummaryReportModelView> GetBOESummaryReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace);

        /// <summary>
        /// Gets the ModelViews for Pre Vs Post Offload Totals (Diagnostics Report)
        /// </summary>
        /// <param name="workspace">Workspace for the report</param>
        /// <returns>he ModelViews for the report</returns>
        ICollection<PreVsPostOffloadTotalsModelView> GetPreVsPostOffloadTotalsModelViews(FullWorkspace workspace);

        /// <summary>
        /// Get the Model Views for the RAM (Responsibility Assignment Matrix)
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="workspace">Workspace for the report</param>
        /// <returns>The ModelViews for the report</returns>
        ICollection<RAMReportModelView> GetRamReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace);

        /// <summary>
        /// Gets the Engineering and Non-Engineering Project WBS Cost Summary By CLIN report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="fullWS">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<ProjectWbsCostSummaryByClinModelView> GetProjectCLINCostSummaryModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS);

        /// <summary>
        /// Gets the Cost By Pricing Code/Resource report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="fullWS">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<CostByPricingCodeReportModelView> GetCostByPriceCodeResource(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS);

        /// <summary>
        /// Gets the Cost By Category/Pricing Code Report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="ws">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<CostByPricingCodeReportModelView> GetCostByCategoryPricingCodeReportModelView(ICollection<FullBoe> offloadedBoes, FullWorkspace ws);

        /// <summary>
        /// Adds elements to the collection with a zero value for each month that does not currently exist to ensure that the report
        /// has columns for every month, even if none of the resources have a value for that given month.
        /// </summary>
        /// <param name="rpsReport">The existing collection of report ModelViews to which the new elements will be added.</param>
        void RPSReportCreateMissingMonths(ICollection<RPSReportModelView> rpsReport);

        /// <summary>
        /// Get the ModelViews for the RPS report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="fullWS">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<RPSReportModelView> GetRPSReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS);

        /// <summary>
        /// Builds model views for PRP report
        /// </summary>
        /// <param name="boes">All BOEs including offloaded</param>
        /// <param name="fullWS">Full Workspace</param>
        /// <returns></returns>
        ICollection<PRPModelView> GetPRPModelViews(ICollection<FullBoe> boes, FullWorkspace fullWS);

        /// <summary>
        /// Get the ModelViews for the Workbench Offload report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="workspace">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        ICollection<WorkbenchOffloadModelView> GetWorkbenchOffloadModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace);

        /// <summary>
        /// Generates the report with a nonce string that relates to that report..
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>The nonce string related to the generated report.</returns>
        string GenerateReportNonce(FullWorkspace workspace, Reports reportType);

        /// <summary>
        /// Bulk downloads reports.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="downloadReports">The download reports.</param>
        /// <param name="zipFilePath">The zip file path to save into.</param>
        /// <returns>The location of the bulk file to send back.</returns>
        string BulkDownloadReport(FullWorkspace workspace, ICollection<DownloadReportModelView> downloadReports, string zipFilePath);
    }
}
