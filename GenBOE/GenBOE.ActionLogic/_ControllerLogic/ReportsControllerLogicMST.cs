// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Web.Configuration;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Handles MST specific reports controller logic.
    /// </summary>
    public class ReportsControllerLogicMST : ReportsControllerLogic
    {
        /// <summary>
        /// Metric loader for MST.
        /// </summary>
        private IMSTMetricLoader _metricLoader;

        /// <summary>
        /// Common Data Mapper
        /// </summary>
        private ICommonDataMapper _CommonDataMapper { get; set; }

        /// <summary>
        /// Injection constructor.
        /// </summary>
        /// <param name="boeExporter"></param>
        /// <param name="boeSummary"></param>
        /// <param name="boeCustomExporter"></param>
        /// <param name="workspaceExportFormatDTOLoader"></param>
        /// <param name="metricLoader"></param>
        /// <param name="boeDiscrepancyReport"></param>
        /// <param name="commonDataMapper"></param>
        /// <param name="proposalLoader">Proposal Loader</param>
        /// <param name="workspaceControllerLogic">Workspace Controller logic</param>
        /// <param name="rteTemplateDataLoader">RTE Template data loader</param>
        /// <param name="travelTripCostCalculator">Travel Trip Cost Calculator</param>
        public ReportsControllerLogicMST(
            IBOEExporter boeExporter,
            BOESummary boeSummary,
            IBOECustomExporter boeCustomExporter,
            IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader,
            IMSTMetricLoader metricLoader,
            BOEDiscrepancyReport boeDiscrepancyReport,
            ICommonDataMapper commonDataMapper,
            IProposalLoader proposalLoader,
            IWorkspaceControllerLogic workspaceControllerLogic,
            TravelTripCostCalculation travelTripCostCalculator)
            : base(boeExporter, boeSummary, boeCustomExporter, workspaceExportFormatDTOLoader, boeDiscrepancyReport, proposalLoader, workspaceControllerLogic, travelTripCostCalculator)
        {
            this._metricLoader = metricLoader;
            this._CommonDataMapper = commonDataMapper;
        }

        /// <summary>
        /// Gets a boolen indicating if custom export is support per MST company configuration
        /// </summary>
        public override bool SupportCustomExport { get { return true; } }

        /// <summary>
        /// Gets a mapping task element to metric names for export.
        /// </summary>
        /// <param name="workspace">Full workspace</param>
        /// <returns>Metric name to task element mappings.</returns>
        public override MetricNameTaskElementMappingDTO GetMetricNameTaskElementMappingDTO(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            ICollection<MetricIdTaskElementIdXrefDTO> xrefs = this._metricLoader.GetTaskElementIdMeticIdMappings(workspace.TaskElements.Select(i => i.Id).ToCollection<int>());
            ICollection<MSTMetricDetailsDTO> metrics = this._metricLoader.GetByTaskElementIds(workspace.TaskElements.Select(i => i.Id).ToCollection<int>());
            Dictionary<int, string> metricTitles = new Dictionary<int, string>();
            foreach (MSTMetricDetailsDTO metric in metrics)
            {
                metricTitles.Add(metric.Id, metric.MeasureName);
            }
            return new MetricNameTaskElementMappingDTO(xrefs, metricTitles);
        }

        /// <summary>
        /// Get the Summary Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Summary Reports Model Views</returns>
        public override ICollection<SSRSReportsModelView> GetSummaryReportsModelViews()
        {
            Collection<SSRSReportsModelView> theModelViews = new Collection<SSRSReportsModelView>();

            // Get all available reports
            Collection<ReportDTO> reportsAvailable = this._CommonDataMapper.getReports();

            if (reportsAvailable != null)
            {
                // get the reports we are interested in and order the way we want
                ReportDTO projectCategoryCLIN = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.CategoryClinSummary);
                if (projectCategoryCLIN != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(projectCategoryCLIN, this.GetReportUrl(Reports.CategoryClinSummary)));
                }

                ReportDTO projectCLINCategory = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.ClinCategorySummary);
                if (projectCLINCategory != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(projectCLINCategory, this.GetReportUrl(Reports.ClinCategorySummary))); 
                }

                ReportDTO projectCLINCostSummary = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.ProjectClinCostSummary);
                if (projectCLINCostSummary != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(projectCLINCostSummary, this.GetReportUrl(Reports.ProjectClinCostSummary)));
                }
            }

            return theModelViews;
        }

        /// <summary>
        /// Get the Customer Reports Model Views for Project Map Workspaces
        /// </summary> 
        /// <returns>Customer Reports Model Views</returns>
        public override ICollection<SSRSReportsModelView> GetCustomerReportsModelViews()
        {
            Collection<SSRSReportsModelView> theModelViews = new Collection<SSRSReportsModelView>();

            // Get all available reports
            Collection<ReportDTO> reportsAvailable = this._CommonDataMapper.getReports();

            if (reportsAvailable != null)
            {
                // get the reports we are interested in and order the way we want
                ReportDTO flatCostAnalysisByCLINResActCY = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int)Reports.CostByClinResActYrFlat);
                if (flatCostAnalysisByCLINResActCY != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(flatCostAnalysisByCLINResActCY, this.GetReportUrl(Reports.CostByClinResActYrFlat)));
                }

                ReportDTO costAnalysisByCLINResActCY = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.CostByClinResActYr);
                if (costAnalysisByCLINResActCY != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(costAnalysisByCLINResActCY, this.GetReportUrl(Reports.CostByClinResActYr))); 
                }

                ReportDTO costAnalysisByCLINActCY = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.CostByClinActYr);
                if (costAnalysisByCLINActCY != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(costAnalysisByCLINActCY, this.GetReportUrl(Reports.CostByClinActYr))); 
                }

                ReportDTO boeSummaryReport = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.BOESummaryReport);
                if (boeSummaryReport != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(boeSummaryReport, this.GetReportUrl(Reports.BOESummaryReport)));
                }
            }

            return theModelViews;
        }

        /// <summary>
        /// Get the Finance Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Finance Reports Model Views</returns>
        public override ICollection<SSRSReportsModelView> GetFinanceReportsModelViews()
        {
            Collection<SSRSReportsModelView> theModelViews = new Collection<SSRSReportsModelView>();

            // Get all available reports
            Collection<ReportDTO> reportsAvailable = this._CommonDataMapper.getReports();

            if (reportsAvailable != null)
            {
                // get the reports we are interested in and order the way we want
                ////ReportDTO costByPricing = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.ByPricingCode);
                ////if (costByPricing != null)
                ////{
                ////    theModelViews.Add(new SSRSReportsModelView(costByPricing, this.GetReportUrl(Reports.ByPricingCode))); 
                ////}

                ////ReportDTO costByCategoryPricingCode = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.ByCatPricingCode);
                ////if (costByCategoryPricingCode != null)
                ////{
                ////    theModelViews.Add(new SSRSReportsModelView(costByCategoryPricingCode, this.GetReportUrl(Reports.ByCatPricingCode))); 
                ////}

                ////ReportDTO offloadCostByYear = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.OffloadCostByYear);
                ////if (offloadCostByYear != null)
                ////{
                ////    theModelViews.Add(new SSRSReportsModelView(offloadCostByYear, this.GetReportUrl(Reports.OffloadCostByYear))); 
                ////}

                ////ReportDTO offloadCostSummary = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.OffloadCostSummary);
                ////if (offloadCostSummary != null)
                ////{
                ////    theModelViews.Add(new SSRSReportsModelView(offloadCostSummary, this.GetReportUrl(Reports.OffloadCostSummary))); 
                ////}

                ReportDTO offloadDetailedReport = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int)Reports.OffloadDetailedReport);
                if (offloadDetailedReport != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(offloadDetailedReport, this.GetReportUrl(Reports.OffloadDetailedReport)));
                }
                

                ReportDTO staffingCurves = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.StaffingCurves);
                if (staffingCurves != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(staffingCurves, this.GetReportUrl(Reports.StaffingCurves))); 
                }
            }

            return theModelViews;
        }

        /// <summary>
        /// Get the Additional Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Additional Reports Model Views</returns>
        public override ICollection<SSRSReportsModelView> GetAdditionalReportsModelViews()
        {
            Collection<SSRSReportsModelView> theModelViews = new Collection<SSRSReportsModelView>();

            // Get all available reports
            Collection<ReportDTO> reportsAvailable = this._CommonDataMapper.getReports();

            if (reportsAvailable != null)
            {
                // get the reports we are interested in and order the way we want
                ReportDTO rps = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.Rps);
                if (rps != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(rps, this.GetReportUrl(Reports.Rps))); 
                }

                ReportDTO prp = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.Prp);
                if (prp != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(prp, this.GetReportUrl(Reports.Prp))); 
                }

                ReportDTO ram = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.Ram);
                if (ram != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(ram, this.GetReportUrl(Reports.Ram))); 
                }

                ReportDTO preVsPostOffloadTotals = reportsAvailable.SingleOrDefault(x => x.ReportType == ReportType.SSRS && x.ReportID == (int) Reports.PreVsPostOffloadTotals);
                if (preVsPostOffloadTotals != null)
                {
                    theModelViews.Add(new SSRSReportsModelView(preVsPostOffloadTotals, this.GetReportUrl(Reports.PreVsPostOffloadTotals))); 
                }
            }

            return theModelViews;
        }

        /// <summary>
        /// Gets the report URL.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>The SSRS report url with Nonce.</returns>
        public override Uri GetReportUrl(Reports reportType)
        {
            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.NO_REPORT_PARAMETERS);
            
            string reportName = string.Empty;
            switch (reportType)
            {
                case Reports.CategoryClinSummary:
                    reportName = Constants.SSRSReportName.SUMMARY_REPORT_RMS;
                    sb.Append($"&{Constants.REPORT_TYPE}={(int) SSRSReportType.ProjectCategoryCLINCostSummary}");
                    break;
                case Reports.ClinCategorySummary:
                    reportName = Constants.SSRSReportName.SUMMARY_REPORT_RMS;
                    sb.Append($"&{Constants.REPORT_TYPE}={(int) SSRSReportType.ProjectCLINCategoryCostSummary}");
                    break;
                case Reports.ProjectClinCostSummary:
                    reportName = Constants.SSRSReportName.PROJECT_CLIN_COST_SUMMARY;
                    break;
                case Reports.CostByClinResActYrFlat:
                    reportName = Constants.SSRSReportName.COST_ANALYSIS_REPORT_RMS_FLAT;
                    break;
                case Reports.CostByClinResActYr:
                    sb.Append($"&{Constants.REPORT_TYPE}={(int) SSRSReportType.CostAnalysis8Years}");
                    reportName = Constants.SSRSReportName.COST_ANALYSIS_REPORT_RMS;
                    break;
                case Reports.CostByClinActYr:
                    sb.Append($"&{Constants.REPORT_TYPE}={(int) SSRSReportType.CostAnalysis17Years}");
                    reportName = Constants.SSRSReportName.COST_ANALYSIS_REPORT_RMS;
                    break;
                case Reports.BOESummaryReport:
                    reportName = Constants.SSRSReportName.BOE_SUMMARY_REPORT;
                    break;
                case Reports.ByPricingCode:
                    reportName = Constants.SSRSReportName.BY_PRICING_CODE;
                    break;
                case Reports.ByCatPricingCode:
                    reportName = Constants.SSRSReportName.BY_CAT_PRICING_CODE;
                    break;
                case Reports.OffloadCostByYear:
                    reportName = Constants.SSRSReportName.OFFLOAD_COST_BY_YEAR;
                    break;
                case Reports.OffloadDetailedReport:
                    reportName = Constants.SSRSReportName.OFFLOAD_DETAILED_REPORT;
                    break;
                case Reports.OffloadCostSummary:
                    reportName = Constants.SSRSReportName.OFFLOAD_COST_SUMMARY;
                    break;
                case Reports.StaffingCurves:
                    reportName = Constants.SSRSReportName.STAFFING_CURVES;
                    break;
                case Reports.Rps:
                    reportName = Constants.SSRSReportName.RPS;
                    break;
                case Reports.Prp:
                    reportName = Constants.SSRSReportName.PRP;
                    break;
                case Reports.Ram:
                    reportName = Constants.SSRSReportName.RAM;
                    break;
                case Reports.PreVsPostOffloadTotals:
                    reportName = Constants.SSRSReportName.PRE_VS_POST_OFFLOAD_TOTALS;
                    break;
                case Reports.WorkbenchOffload:
                    sb.Append("&rs:format=word");
                    reportName = Constants.SSRSReportName.WORKBENCH_OFFLOAD;
                    break;
            }
            
            sb.Append("&nonce=");

            Uri toReturn = new Uri($"{WebConfigurationManager.AppSettings["ReportServerLocation"]}/{WebConfigurationManager.AppSettings["ReportServerFolderName"]}/{reportName}{sb}");

            return toReturn;
        }

        /// <summary>
        /// Check if the PTM data is out of sync
        /// RMS does not use PTM integration, so always return an empty collection
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <returns>Empty collection of strings for RMS</returns>
        public override ICollection<string> GetPtmDataOutOfSyncMessages(FullWorkspace ws)
        {
            return new Collection<string>();
        }
    }
}