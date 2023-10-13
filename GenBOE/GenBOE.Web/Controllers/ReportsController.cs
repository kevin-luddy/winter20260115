// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.VisualBasic.Logging;

    public class ReportsController : GenBOEController
    {
        private Logger log = new Logger(typeof(ReportsController));

        private WorkspaceActivityReport _WorkspaceActivityReport;
        private BOEActivityReport _BOEActivityReport;
        private IProPricerDTODataLoader proPricerLoader;
        private ICustomFieldValueDTODataLoader customFieldValueDTODataLoader;
        private ProPricerExporter proPricerExporter;
        private WorkspaceExporter workspaceExporter;
        private ITravelUnitCostExporter travelUnitCostExporter;
        private IBOEStatusReport _BOEStatusReport;
        private ITravelExtendedCostExporter travelExtendedCostExporter;
        private IWorkspaceDTODataLoader workspaceLoader;
        private IValidateBOE validateBOE;
        private IBOEFormControllerLogic boeFormControllerLogic;
        private ISSRSControllerLogic ssrsControllerLogic;
        private IUserDTODataLoader userLoader;
        private ContractTypeLoader contractTypeLoader;

        /// <summary>
        /// Business logic for the reports controller
        /// </summary>
        private IReportsControllerLogic reportsControllerLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportsController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            WorkspaceActivityReport inWorkspaceActivityReport,
            IProPricerDTODataLoader inProPricerLoader,
            ICustomFieldValueDTODataLoader inCustomFieldValueDTODataLoader,
            ProPricerExporter inProPricerExporter,
            BOEActivityReport inBOEActivityReport,
            WorkspaceExporter inWorkspaceExporter,
            ITravelUnitCostExporter inTravelUnitCostExporter,
            IBOEStatusReport inBOEStatusReport,
            ITravelExtendedCostExporter inTravelExtendedCostExporter,
            SystemMetrics inSystemMetrics,
            IValidateBOE inValidateBOE,
            IUserDTODataLoader inUserDTODataLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            IReportsControllerLogic inReportsControllerLogic,
            IFullObjectFactory factory,
            IWorkspaceDTODataLoader workspaceLoader,
            IGenBOEControllerLogic inControllerLogic,
            IBOEFormControllerLogic inBOEFormControllerLogic,
            ISSRSControllerLogic ssrsControllerLogic,
            IUserDTODataLoader userLoader,
            ContractTypeLoader contractTypeLoader)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, inPermissionsLoader, inControllerLogic)
        {
            this._WorkspaceActivityReport = inWorkspaceActivityReport;
            this.proPricerLoader = inProPricerLoader;
            this.customFieldValueDTODataLoader = inCustomFieldValueDTODataLoader;
            this.proPricerExporter = inProPricerExporter;
            this._BOEActivityReport = inBOEActivityReport;
            this.workspaceExporter = inWorkspaceExporter;
            this.travelUnitCostExporter = inTravelUnitCostExporter;
            this._BOEStatusReport = inBOEStatusReport;
            this.travelExtendedCostExporter = inTravelExtendedCostExporter;
            this.validateBOE = inValidateBOE;
            this.reportsControllerLogic = inReportsControllerLogic;
            this.workspaceLoader = workspaceLoader;
            this.boeFormControllerLogic = inBOEFormControllerLogic;
            this.ssrsControllerLogic = ssrsControllerLogic;
            this.userLoader = userLoader;
            this.contractTypeLoader = contractTypeLoader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "Report Index", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            this.DecideIfNonProjectMapLinksShouldBeVisible(ws);

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);

            // Finalize Action
            FinalizeAction(log, "Report Index", sw);
            return toReturn;
        }

        /// <summary>
        /// Return the main ProPricer page which will render the ProPricer control
        /// </summary>
        /// <param name="workspace">The Workspace</param>
        /// <returns>ProPricer Index view</returns>
        [ProPricerExportAccess]
        public ViewResult ProPricerIndex(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "ProPricerIndex", SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = GetMasterView(WebConstants.VIEW_EXPORT_TO_PROPRICER_INDEX, workspace);

            // Finalize Action
            FinalizeAction(log, "ProPricerIndex", sw);
            return toReturn;
        }

        #region Display

        /// <summary>
        /// Display the Exports View
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ActionResult DisplayExports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayExports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewData["IsUsingSummarizeByCustomFieldTemplate"] = false;
            ViewData["SummarizeByCustomFieldOptions"] = null;
            
            bool hasTravel = false;

            if (ws.Travels.Any())
            {
                hasTravel = true;
            }

            // Perform Action
            // Get all available reports
            Collection<ReportDTO> reportsAvailable = _CommonDataMapper.getReports();
            Collection<ExportsModelView> theModelViews = new Collection<ExportsModelView>();
            bool isSubcontractorUser = IsSubcontractorUser(ws);
            // Filter exports
            if (reportsAvailable != null)
            {
                foreach (ReportDTO report in reportsAvailable)
                {
                    if ((isSubcontractorUser && report.ReportID == (int)Reports.INLFormsExport) || 
                        (!SiteMasterUtilities.IsBOEFormVisible && report.ReportID == (int)Reports.INLFormsExport)) {
                        continue;
                    }

                    if (ws.IsProjectMapWorkspace && report.ReportID != (int)Reports.AllBOEs && report.ReportID != (int)Reports.StandardReports && report.ReportID != (int)Reports.WorkbenchOffload)
                    {
                        // In a Project Map Workspace, only All BOEs, Standard Reports, and Workbench Offload are shown
                        // If this is a Project Map WS, but the report is not All BOEs or Standard Reports or Workbench Offload,
                        // skip the rest of the logic so the report is not shown
                        continue;
                    }

                    if (SystemConfiguration.Instance().CompanyMode != IES.Common.CompanyConfiguration.MST && (report.ReportID == (int)Reports.StandardReports || report.ReportID == (int)Reports.WorkbenchOffload))
                    {
                        // Standard Reports and Workbench are only shown for RMS Workspaces
                        // If the report is one of these but this is not a RMS WS,
                        // skip the rest of the logic so the report is not shown
                        continue;
                    }

                    if (report.ReportType == ReportType.Export)
                    {
                        ExportsModelView theModelView = new ExportsModelView(report);

                        if (theModelView.ReportID == (int)Reports.WorkbenchOffload)
                        {
                            // we need to get the ssrs report uri as well for the front-end
                            theModelView = new SSRSReportsModelView(report, this.reportsControllerLogic.GetReportUrl(Reports.WorkbenchOffload));
                        }

                        if (theModelView.ReportID == (int)Reports.AllBOEs)
                        {
                            WorkspaceExportFormatDTO exportFormat = ws.WorkspaceExportFormats.FirstOrDefault(x => x.Id == ws.TemplateID);
                            theModelView.Description = String.Format(theModelView.Description, exportFormat.ExportFormatName);

                            // Do we need to support the special Labor Hours Summary by Custom Field template?
                            if (this.reportsControllerLogic.IsUsingSummarizeByCustomFieldTemplate(exportFormat.ExportFormatName))
                            {
                                this.ViewData["IsUsingSummarizeByCustomFieldTemplate"] = true;
                                this.ViewData["SummarizeByCustomFieldOptions"] = this.reportsControllerLogic.SummarizeByCustomFieldOptions(ws.CustomFields);
                            }
                        }

                        if (theModelView.ReportID == (int)Reports.AllBOEsSegmented)
                        {
                            WorkspaceExportFormatDTO exportFormat = ws.WorkspaceExportFormats.FirstOrDefault(x => x.Id == ws.TemplateID);
                            theModelView.Description = String.Format(theModelView.Description, exportFormat.ExportFormatName);

                            // Do we need to support the special Labor Hours Summary by Custom Field template?
                            if (this.reportsControllerLogic.IsUsingSummarizeByCustomFieldTemplate(exportFormat.ExportFormatName))
                            {
                                this.ViewData["IsUsingSummarizeByCustomFieldTemplate"] = true;
                                this.ViewData["SummarizeByCustomFieldOptions"] = this.reportsControllerLogic.SummarizeByCustomFieldOptions(ws.CustomFields);
                            }
                        }

                        if (!((report.ReportID == (int)Reports.TravelExtendedCost || report.ReportID == (int)Reports.TravelUnitCost) && !hasTravel))
                        {
                            theModelViews.Add(theModelView);
                        }
                    }
                }
            }

            ViewData["SupportCustomExport"] = this.reportsControllerLogic.SupportCustomExport;
            ViewBag.IsProjectMapWs = ws.IsProjectMapWorkspace;
            
            ICollection<string> outOfSyncMessages = this.reportsControllerLogic.GetPtmDataOutOfSyncMessages(ws);
            this.ViewBag.IsPtmDataOutOfSync = outOfSyncMessages.Any();
            this.ViewData["OutOfSyncMessages"] = outOfSyncMessages;

            ICollection<PermissionsDTO> adminDtos = this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
                .Where(x => x.Role == Role.WorkspaceAdmin).ToCollection();
            ICollection<UserDTO> users = this.userLoader.GetByIds(adminDtos.Select(a => a.ETIUserId).ToList());
            this.ViewData["WorkspaceAdmins"] = users.Select(u => u.DisplayName).ToCollection();

            // find the position of "allBOEs"
            int? pos = theModelViews.Select((report, index) => new { report, index }).FirstOrDefault(x => x.report.ReportID == (int)Reports.AllBOEs)?.index;
            // insert Chunked Report after it if it exists
            if (pos != null && theModelViews.FirstOrDefault(x => x.ReportID == (int)Reports.AllBOEsSegmented) != null)
            {
                int newPosition = (int)pos + 1;
                theModelViews.Insert(newPosition, theModelViews.First(x => x.ReportID == (int)Reports.AllBOEsSegmented));
                int? oldPos = theModelViews.Select((report, index) => new { report, index }).LastOrDefault(x => x.report.ReportID == (int)Reports.AllBOEsSegmented)?.index;
                theModelViews.RemoveAt(oldPos.Value);
            }

            ViewResult toReturn = View(WebConstants.VIEW_EXPORTS, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayExports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the General Reports View
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ActionResult DisplayGeneralReports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayGeneralReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            // Perform Action
            // Get all available reports
            Collection<ReportDTO> reportsAvailable = _CommonDataMapper.getReports();
            Collection<ExportsModelView> theModelViews = new Collection<ExportsModelView>();

            if (reportsAvailable != null)
            {
                // get the reports we are interested in and order the way we want
                ReportDTO boeStatusReport = reportsAvailable.Where(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BOEStatus).Single();
                theModelViews.Add(new ExportsModelView(boeStatusReport));

                ReportDTO boeActivityReport = reportsAvailable.Where(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BOEActivity).Single();
                theModelViews.Add(new ExportsModelView(boeActivityReport));

                ReportDTO workspaceActivityReport = reportsAvailable.Where(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.WorkspaceActivity).Single();
                theModelViews.Add(new ExportsModelView(workspaceActivityReport));

                ReportDTO discrepancyReport = reportsAvailable.Where(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BoeDiscrepancy).Single();
                theModelViews.Add(new ExportsModelView(discrepancyReport));

                ReportDTO validateAllReport = reportsAvailable.Where(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.ValidateAllBOE).Single();
                theModelViews.Add(new ExportsModelView(validateAllReport));

                if (FullObjectHelper.ShowEquivalentPersonsOption && ws.IsUsingEquivalentPerson)
                {
                    foreach (ExportsModelView model in theModelViews)
                    {
                        model.Description = model.Description.Replace("Hours", "EPs").Replace("hour", "EP");
                    }
                }
            }
            ViewResult toReturn = View(WebConstants.VIEW_GENERAL_REPORTS, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayGeneralReports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Summary SSRS Reports for RMS Project Map Workspaces
        /// </summary>
        /// <param name="workspace">workspace shortname</param>
        /// <returns>action result with modelviews</returns>
        public ActionResult DisplaySummaryReports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplaySummaryReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = null;

            // Only get reports for RMS
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // Perform Action
                // Get all available reports
                ICollection<SSRSReportsModelView> theModelViews = this.reportsControllerLogic.GetSummaryReportsModelViews();

                toReturn = this.View(WebConstants.VIEW_SUMMARY_REPORTS, theModelViews);
            }

            // Finalize Action
            this.FinalizeAction(this.log, "DisplaySummaryReports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Customer SSRS Reports for RMS Project Map Workspaces
        /// </summary>
        /// <param name="workspace">workspace shortname</param>
        /// <returns>action result with modelviews</returns>
        public ActionResult DisplayCustomerReports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplayCustomerReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = null;

            // Only get reports for RMS
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // Perform Action
                ICollection<SSRSReportsModelView> theModelViews = this.reportsControllerLogic.GetCustomerReportsModelViews();

                // Non-Project Maps will not include the BOE Summary Report
                if(!ws.IsProjectMapWorkspace)
                {
                    theModelViews = theModelViews.Where(x => x.ReportID != (int)Reports.BOESummaryReport).ToList();
                }

                toReturn = this.View(WebConstants.VIEW_CUSTOMER_REPORTS, theModelViews);
            }

            // Finalize Action
            this.FinalizeAction(this.log, "DisplayCustomerReports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Finance SSRS Reports for RMS Project Map Workspaces
        /// </summary>
        /// <param name="workspace">workspace shortname</param>
        /// <returns>action result with modelviews</returns>
        public ActionResult DisplayFinanceReports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplayFinanceReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = null;

            // Only get reports for RMS
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // Perform Action
                ICollection<SSRSReportsModelView> theModelViews = this.reportsControllerLogic.GetFinanceReportsModelViews();
                
                toReturn = this.View(WebConstants.VIEW_FINANCE_REPORTS, theModelViews);
            }

            // Finalize Action
            this.FinalizeAction(this.log, "DisplayFinanceReports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Additional SSRS Reports for RMS Project Map Workspaces
        /// </summary>
        /// <param name="workspace">workspace shortname</param>
        /// <returns>action result with modelviews</returns>
        public ActionResult DisplayAdditionalReports(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplayAdditionalReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = null;

            // Only get reports for RMS
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // Perform Action
                ICollection<SSRSReportsModelView> theModelViews = this.reportsControllerLogic.GetAdditionalReportsModelViews();

                toReturn = this.View(WebConstants.VIEW_ADDITIONAL_REPORTS, theModelViews);
            }

            // Finalize Action
            this.FinalizeAction(this.log, "DisplayAdditionalReports", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the Workspace Activity Report
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ActionResult DisplayWorkspaceActivityReport(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayWorkspaceActivityReport", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            // Call the BL to generate the report
            WorkspaceActivityReportModelView theModelViews =
                _WorkspaceActivityReport.GenerateReport(ws);

            ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_ACTIVITY_REPORT, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayWorkspaceActivityReport", sw);
            return toReturn;
        }

        /// <summary>
        /// Display the BOE Activity Report
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ActionResult DisplayBOEActivityReport(string workspace, string sortField, string sortDirection)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayBOEActivityReport", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            // Call the BL to generate the report
            BOEActivityReportModelView theModelView = _BOEActivityReport.GenerateReport(ws);

            // sort by WBS # first priority, then CLIN # within that
            this.SortBOEActivityReport(theModelView, sortField, sortDirection);

            ViewResult toReturn = View(WebConstants.VIEW_BOE_ACTIVITY_REPORT, theModelView);

            // Finalize Action
            FinalizeAction(log, "DisplayBOEActivityReport", sw);
            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void SortBOEActivityReport(BOEActivityReportModelView theModelView, string sortField, string sortDirection)
        {
            ViewData["SortField"] = sortField;
            ViewData["SortDirection"] = sortDirection;

            const string SORT_WBS_NUM = "wbsnum";
            const string SORT_WBS_TITLE = "wbstitle";
            const string SORT_BOE_TITLE = "boetitle";
            const string SORT_CLIN_NUM = "clinnum";
            const string SORT_CLIN_TITLE = "clintitle";
            const string SORT_AUTHOR = "author";
            const string SORT_STATUS = "status";
            const string SORT_DAYS_UNASSIGNED = "days_unassigned";
            const string SORT_DAYS_DRAFT = "days_draft";
            const string SORT_DAYS_WAITING = "days_waiting";
            const string SORT_DAYS_CREATE_APPROVE = "days_create_approve";
            const string SORT_DAYS_DRAFT_APPROVE = "days_draft_approve";
            const string SORT_TIMES_DRAFT = "times_draft";
            const string SORT_TIMES_WAITING = "times_waiting";
            const string SORT_TIMES_APPROVED = "times_approved";
            const string SORT_TIMES_AUTHOR_REASSIGNED = "times_author_reassigned";

            const string DIRECTION_ASCENDING = "Asc";
            const string DIRECTION_DESCENDING = "Desc";

            // must have a valid sort direction (blank is valid - counts as ascending
            if (!
                (string.IsNullOrEmpty(sortField) ||
                sortDirection == DIRECTION_ASCENDING ||
                sortDirection == DIRECTION_DESCENDING))
            {
                throw new ValidationException("Invalid sort direction, unable to sort BOE Activity Report.");
            }

            if (string.IsNullOrEmpty(sortField) || sortField == SORT_WBS_NUM)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.WBSNumPadded descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_WBS_TITLE)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.WBSTitle, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.WBSTitle descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_BOE_TITLE)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.BOETitle, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.BOETitle descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_CLIN_NUM)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.CLINNum, x.WBSNumPadded
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.CLINNum descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_CLIN_TITLE)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.CLINTitle, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.CLINTitle descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_AUTHOR)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.Authors, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.Authors descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_STATUS)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.Status, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.Status descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_DAYS_UNASSIGNED)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInUnassigned, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInUnassigned descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_DAYS_DRAFT)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInDraft, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInDraft descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_DAYS_WAITING)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInAwaitingApproval, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysInAwaitingApproval descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_DAYS_CREATE_APPROVE)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysFromCreatedToApproved, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysFromCreatedToApproved descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_DAYS_DRAFT_APPROVE)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysFromDraftToApproved, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.DaysFromDraftToApproved descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_TIMES_DRAFT)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInDraft, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInDraft descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_TIMES_WAITING)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInAwaitingApproval, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInAwaitingApproval descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_TIMES_APPROVED)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInApproved, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesInApproved descending
	                                  select x).ToList());
                }
            }
            else if (sortField == SORT_TIMES_AUTHOR_REASSIGNED)
            {
                if (string.IsNullOrEmpty(sortDirection) || sortDirection == DIRECTION_ASCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesAuthorReassigned, x.WBSNumPadded, x.CLINNum
	                                  select x).ToList());
                }
                else if (sortDirection == DIRECTION_DESCENDING)
                {
                    theModelView.Rows = new Collection<BOEActivityReportRow>((from x in theModelView.Rows
	                                  orderby x.NumTimesAuthorReassigned descending
	                                  select x).ToList());
                }
            }
            else
            {
                throw new ValidationException("Invalid sort field, unable to sort BOE Activity Report.");
            }
        }

        /// <summary>
        /// Display the BOE Status Report
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ActionResult DisplayBOEStatusReport(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayBOEStatusReport", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewData["AllWBS"] = ws.WbsElements;
            ViewData["AllCLIN"] = ws.Clins;
            ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

            // Call the BL to generate the status report
            // All BOEs for the workspace as a default
            List<FullBoe> boes = ws.Boes.ToList();
            List<BoeTaskElementDTO> tasks = ws.TaskElements.ToList();

            bool isOffloading = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload;
            if (isOffloading)
            {
                OffloadLaborRates offloader = new OffloadLaborRates();
                List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
                OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), ws);

                boes = results.Boes.ToList();
                tasks = boes.SelectMany(b => b.TaskElements).ToList();
            }

            BOEExportInputs exportInputs = new BOEExportInputs(boes, boes, tasks, ws);
            Collection<BOEStatusReportModelView> theModelViews = _BOEStatusReport.GenerateBOEStatusReport(exportInputs);

            ViewResult toReturn = View(WebConstants.VIEW_BOE_STATUS_REPORT, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayBOEStatusReport", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the Boe Discrepancy Report
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <returns>A Page displaying the boe discrepancy report</returns>
        public ActionResult DisplayBoeDiscrepancyReport(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayBoeDiscrepancyReport", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewData.Remove("ExportBoeDiscrepancyReport");

            ICollection<BoeDiscrepancyReportModelView> theModelViews = this.reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, true);
            ViewResult toReturn = View(WebConstants.VIEW_BOE_DISCREPANCY_REPORT, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayBoeDiscrepancyReport", sw);
            return toReturn;
        }

        /// <summary>
        /// Displays the Validation of All BOE
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <returns>A Page displaying the validation for all boe's</returns>
        public ActionResult DisplayValidateAllBOE(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayValidateAllBOE", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);


            ValidationAllBOEModelView theModelViews = this.validateBOE.ValidateAllBOEs(ws);
            ViewResult toReturn = View(WebConstants.VIEW_VALIDATE_ALL_BOE_REPORT, theModelViews);

            // Finalize Action
            FinalizeAction(log, "DisplayValidateAllBOE", sw);
            return toReturn;
        }


        /// <summary>
        /// Display the main ProPricer control which will render the grid and contains
        /// controls used for adding and editing formats
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        [ProPricerExportAccess]
        public ViewResult DisplayProPricer(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplayProPricer", SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

	        this.ViewData["EnableSendDirectly"] = !ws.IsProjectMapWorkspace && ConfigurationUtilities.GetAppSetting<bool>("EnableSendToProPricerDirectly", false);

            // Get Select list for Tasks
            ICollection<SelectListItem> taskSelectList_Unselected = new Collection<SelectListItem>();
            ICollection<SelectListItem> taskSelectList_Selected = new Collection<SelectListItem>();
            IDictionary<int, EnumTypeModelView> allProPricerFieldNames = this._CommonDataMapper.GetProPricerFieldsDictionary(ws.IsProjectMapWorkspace);
            List<int> fieldIDs = allProPricerFieldNames.Select(x => x.Value.EnumTypeID).ToList();
            foreach (ProPricerField_Task task in Enum.GetValues(typeof(ProPricerField_Task)))
            {
                if (task != ProPricerField_Task.BLANK && fieldIDs.Contains((int)task))
                {
                    taskSelectList_Unselected.Add(new SelectListItem { Text = allProPricerFieldNames[(int)task].EnumTypeName, Value = ((int)task).ToString() });
                }
            }

            // Get Select list for Resources
            ICollection<SelectListItem> resourceSelectList_Unselected = new Collection<SelectListItem>();
            ICollection<SelectListItem> resourceSelectList_Selected = new Collection<SelectListItem>();
            IDictionary<int, EnumTypeModelView> allProPricerFields = this._CommonDataMapper.GetProPricerFieldsDictionary(ws.IsProjectMapWorkspace);

            foreach (ProPricerField_Resources resource in Enum.GetValues(typeof(ProPricerField_Resources)))
            {
                if (resource != ProPricerField_Resources.BLANK && fieldIDs.Contains((int)resource))
                {
                    resourceSelectList_Unselected.Add(new SelectListItem { Text = allProPricerFields[(int)resource].EnumTypeName, Value = ((int)resource).ToString() });
                }
            }

            // regardless of what level the custom field is defined at, it should be shown in both the task and resource list options
            foreach (CustomFieldDTO customField in ws.CustomFields)
            {
                taskSelectList_Unselected.Add(new SelectListItem { Text = customField.CustomFieldName + " Description", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customField.Id, ProPricerCustomFieldSelection.CustomFieldDescription) });
                if (!customField.IsOpenEnded)
                {
                    taskSelectList_Unselected.Add(new SelectListItem { Text = customField.CustomFieldName + " ID", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customField.Id, ProPricerCustomFieldSelection.CustomFieldID) });
                }
                resourceSelectList_Unselected.Add(new SelectListItem { Text = customField.CustomFieldName + " Description", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customField.Id, ProPricerCustomFieldSelection.CustomFieldDescription) });
                if (!customField.IsOpenEnded)
                {
                    resourceSelectList_Unselected.Add(new SelectListItem { Text = customField.CustomFieldName + " ID", Value = ExportToProPricerModelView.GetFormattedCustomFieldID(customField.Id, ProPricerCustomFieldSelection.CustomFieldID) });
                }
            }

            // Pass all select list items to the View
	        this.ViewData["SelectList_Task_Unselected"] = taskSelectList_Unselected;
	        this.ViewData["SelectList_Task_Selected"] = taskSelectList_Selected;

	        this.ViewData["SelectList_Resources_Unselected"] = resourceSelectList_Unselected;
	        this.ViewData["SelectList_Resources_Selected"] = resourceSelectList_Selected;

            if (ws.IsProjectMapWorkspace)
            {
	            this.ViewData["DisplayTravelLockedMessage"] = false;
	            this.ViewData["DisplayTravelWarningMessage"] = false;
            }
            else if (ws.WorkspaceState == WorkspaceState.Locked || ws.WorkspaceState == WorkspaceState.Complete || ws.WorkspaceState == WorkspaceState.Closed)
            {
                IReadOnlyCollection<WorkspaceHistoryDTO> historyRecords = ws.WorkspaceHistory;
                DateTime lockedSince = historyRecords.Where(x => x.NewValue == WorkspaceState.Locked || x.NewValue == WorkspaceState.Complete || x.NewValue == WorkspaceState.Closed).Select(x => x.Date).Max();

	            this.ViewData["DisplayTravelLockedMessage"] = true;
	            this.ViewData["DisplayTravelWarningMessage"] = false;
	            this.ViewData["TravelMessage"] = @"Workspace's travel rates and hour and cost estimates are locked as of " + lockedSince.ToString("M/d/yyyy h:m tt") + " Eastern Time.";
            }
            else if (ws.WorkspaceState == WorkspaceState.Initialization || ws.WorkspaceState == WorkspaceState.Working)
            {
	            this.ViewData["DisplayTravelLockedMessage"] = false;
	            this.ViewData["DisplayTravelWarningMessage"] = true;
	            this.ViewData["TravelMessage"] = @"Workspace's travel rates and hour and cost estimates are not locked.  It is recommended that the Workspace is locked prior to exporting to ProPricer to prevent BOE estimates from changing during pricing.";
            }
            else
            {
	            this.ViewData["DisplayTravelLockedMessage"] = false;
            }

            // Perform Action
            ViewResult toReturn = this.View(WebConstants.VIEW_EXPORT_TO_PROPRICER);

            // Finalize Action
	        this.FinalizeAction(this.log, "DisplayProPricer", sw);
            return toReturn;
        }

        /// <summary>
        /// Updates the pro pricer last instance for a workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="selectedInstance">The selected instance.</param>
        /// <returns>The JsonResult</returns>
        public ActionResult UpdateProPricerLastInstance(string workspace, int? selectedInstance)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_INSTANCE, SecurityPage.ExportToProPricer, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            ws.LastProPricerInstance = selectedInstance;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.workspaceLoader.SaveIdentificationAndExportFormat(ws.CurrentActiveUser.UserID, ws);
                scope.Complete();
            }

            // Clear the cache after since LastUpdateTime was changed
            this.Factory.ClearWorkspaceCache(ws.Shortname);

            // Finalize Action
            this.FinalizeAction(this.log, WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_INSTANCE, sw);

            return Json(new { Status = true });
        }

        /// <summary>
        /// Updates the pro pricer last proposal for a workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <returns>The JsonResult</returns>
        public ActionResult UpdateProPricerLastProposal(string workspace, string proposalId)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_PROPOSAL, SecurityPage.ExportToProPricer, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            ws.LastProPricerProposal = proposalId;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.workspaceLoader.SaveIdentificationAndExportFormat(ws.CurrentActiveUser.UserID, ws);
                scope.Complete();
            }

            // Clear the cache after since LastUpdateTime was changed
            this.Factory.ClearWorkspaceCache(ws.Shortname);

            // Finalize Action
            this.FinalizeAction(this.log, WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_PROPOSAL, sw);

            return Json(new { Status = true });
        }

        /// <summary>
        /// Returns the ProPricer preview data.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="formatId">The format identifier.</param>
        /// <returns>ProPricer preview data.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Inline use, expecting for garbage collection to take care of things.")]
        public ActionResult ProPricerPreview(string workspace, int formatId, ProPricerScope scope)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, WebConstants.ACTION_WS_PROPRICER_PREVIEW, SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

            // Get the format
            ProPricerDTO format = proPricerLoader.GetById(formatId, scope);
            ProPricerPreview preview = new Dtos.ProPricerPreview();

            if (format != null)
            {
                // Check that format is part of this workspace
                if (format.Scope != ProPricerScope.System && format.WorkspaceID != ws.Id)
                {
                    throw new GenValidationException("The format selected for export is not part of the current workspace.");
                }

                if (format.Scope == ProPricerScope.System)
                {
                    // If the template is scoped at the system level, run it thru the model view. The reason for this is; if 
                    // there are custom fields in the system level template, their Ids are incorrect for the current workspace.
                    // Running thru the model view will re-map the custom field Ids to those of the current workspace.
                    ExportToProPricerModelView modelView = new ExportToProPricerModelView(format, this.Factory, ws.CustomFields);
                    format = modelView.GetAssociatedDTO();
                }

                IDictionary<int, EnumTypeModelView> allProPricerFieldNames = this._CommonDataMapper.GetProPricerFieldsDictionary(ws.IsProjectMapWorkspace);
                List<int> fieldIDs = allProPricerFieldNames.Select(x => x.Value.EnumTypeID).ToList();

                // add blanks for row number
                preview.TaskHeaders.Add("genBOE Header *");
                preview.TaskHeaders.Add(string.Empty);
                preview.ResourceHeaders.Add("genBOE Header *");
                preview.ResourceHeaders.Add(string.Empty);

                foreach (ProPricerTasks task in format.ProPricerTasks.OrderBy(t => t.ListOrder))
                {
                    if (task.CustomFieldID.HasValue)
                    {
                        preview.TaskHeaders.Add(GetCustomFieldHeader(ws, task.CustomFieldID.Value, task.Selection));
                    }
                    else
                    {
                        if (task.Task == ProPricerField_Task.BLANK)
                        {
                            preview.TaskHeaders.Add("Blank");
                        }
                        else if (fieldIDs.Contains((int)task.Task))
                        {
                            preview.TaskHeaders.Add(allProPricerFieldNames[(int)task.Task].EnumTypeName);
                        }
                    }
                }

                foreach (ProPricerResources resource in format.ProPricerResources.OrderBy(r => r.ListOrder))
                {
                    if (resource.CustomFieldID.HasValue)
                    {
                        preview.ResourceHeaders.Add(GetCustomFieldHeader(ws, resource.CustomFieldID.Value, resource.Selection));
                    }
                    else
                    {
                        if (resource.Resource == ProPricerField_Resources.BLANK)
                        {
                            preview.ResourceHeaders.Add("Blank");
                        }
                        else if (fieldIDs.Contains((int)resource.Resource))
                        {
                            preview.ResourceHeaders.Add(allProPricerFieldNames[(int)resource.Resource].EnumTypeName);
                        }
                    }
                }

                PpDataReadyForExport ppDataToExport = proPricerExporter.ExportProPricer(format, ws);
                string[] tasks = ppDataToExport.TaskData.Take(10).ToArray();
                string[] resources = ppDataToExport.ResourceData.Take(10).ToArray();
                TextFieldParser parser = null;

                // Add row numbers, remove quotes, and remove the extra ',' at the end
                for (int i = 0; i < tasks.Length; i++)
                {
                    string task = ',' + (i + 1).ToString() + "," + tasks[i];
                    parser = new TextFieldParser(new StringReader(task)) { HasFieldsEnclosedInQuotes = true, Delimiters = new[] { "," } };
                    string[] taskRows = parser.ReadFields();

                    for (int j = 0; j < taskRows.Length; j++)
                    {
                        string taskRow = taskRows[j];
                        if (taskRow.StartsWith("\""))
                        {
                            taskRows[j] = taskRow.Substring(1, taskRow.Length - 2);
                        }
                    }
                    List<string> rows = taskRows.ToList();
                    rows.RemoveAt(rows.Count - 1);
                    preview.TaskRows.Add(rows);
                }

                // Add row numbers remove quotes, and remove the extra ',' at the end
                for (int i = 0; i < resources.Length; i++)
                {
                    string resource = ',' + (i+ 1).ToString() + "," + resources[i];
                    parser = new TextFieldParser(new StringReader(resource)) { HasFieldsEnclosedInQuotes = true, Delimiters = new[] { "," } };
                    string[] resourceRows = parser.ReadFields();

                    for (int j = 0; j < resourceRows.Length; j++)
                    {
                        string resourceRow = resourceRows[j];
                        if (resourceRow.StartsWith("\""))
                        {
                            resourceRows[j] = resourceRow.Substring(1, resourceRow.Length - 2);
                        }
                    }
                    List<string> rows = resourceRows.ToList();
                    rows.RemoveAt(rows.Count - 1);
                    preview.ResourceRows.Add(rows);
                }
            }
            else
            {
                throw new GenValidationException("Unknown Propricer Format");
            }

            // Finalize Action
            this.FinalizeAction(this.log, WebConstants.ACTION_WS_PROPRICER_PREVIEW, sw);
            return this.Json(preview);
        }

        /// <summary>
        /// Returns the ProPricer preview data.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="formatId">The format identifier.</param>
        /// <returns>ProPricer preview data.</returns>
        public ActionResult ProPricerExport(string workspace, int formatId, ProPricerScope scope)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, WebConstants.ACTION_WS_PROPRICER_EXPORT, SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

            // Get the format
            ProPricerDTO format = proPricerLoader.GetById(formatId, scope);
            string taskFile;
            string resourceFile;

            if (format != null)
            {
                // Check that format is part of this workspace
                if (format.Scope != ProPricerScope.System && format.WorkspaceID != ws.Id)
                {
                    throw new GenValidationException("The format selected for export is not part of the current workspace.");
                }

                if (format.Scope == ProPricerScope.System)
                {
                    // If the template is scoped at the system level, run it thru the model view. The reason for this is; if 
                    // there are custom fields in the system level template, their Ids are incorrect for the current workspace.
                    // Running thru the model view will re-map the custom field Ids to those of the current workspace.
                    ExportToProPricerModelView modelView = new ExportToProPricerModelView(format, this.Factory, ws.CustomFields);
                    format = modelView.GetAssociatedDTO();
                }
                
                PpDataReadyForExport ppDataToExport = proPricerExporter.ExportProPricer(format, ws);

                // remove the extra , when creating the data output string
                taskFile = proPricerExporter.CreateDataOutputString(ppDataToExport.TaskData);
                resourceFile = proPricerExporter.CreateDataOutputString(ppDataToExport.ResourceData);
            }
            else
            {
                throw new GenValidationException("Unknown Propricer Format");
            }

            // Finalize Action
            this.FinalizeAction(this.log, WebConstants.ACTION_WS_PROPRICER_EXPORT, sw);
            return this.Json(new {tasks = taskFile, resources = resourceFile });
        }

        /// <summary>
        /// Gets the custom field header depending on if it is ID or Description.
        /// </summary>
        /// <param name="workspace">The workspace to pull the custom field from.</param>
        /// <param name="customFieldId">The custom field id.</param>
        /// <param name="selection">The ID or Description for the Custom Field.</param>
        /// <returns></returns>
        private string GetCustomFieldHeader(FullWorkspace workspace, int customFieldId, ProPricerCustomFieldSelection selection)
        {
            string header = string.Empty;
            CustomFieldDTO customField = workspace.CustomFields.FirstOrDefault(c => c.Id == customFieldId);
            if (customField != null)
            {
                if (selection == ProPricerCustomFieldSelection.CustomFieldID)
                {
                    header = customField.CustomFieldName + " ID";
                }
                else
                {
                    // custom field description
                    header = customField.CustomFieldName + " Description";
                }
            }

            return header;
        }

        /// <summary>
        /// Display the ProPricer Grid
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>the propricer grid view</returns>
        [ProPricerExportAccess]
        public ViewResult DisplayProPricerGrid(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "DisplayProPricerGrid", SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

	        this.ViewData["EnableSendDirectly"] = !ws.IsProjectMapWorkspace && ConfigurationUtilities.GetAppSetting<bool>("EnableSendToProPricerDirectly", false);

            SendToProPricerModelView sendToProPricerModelView = new SendToProPricerModelView();
            sendToProPricerModelView.SelectedInstance = ws.LastProPricerInstance;
            sendToProPricerModelView.SelectedProposalId = ws.LastProPricerProposal ?? string.Empty;

            this.ViewData["SendToProPricerData"] = sendToProPricerModelView;

            //Filter out ProPricer formats that use ProjectMap specific fields if workspace is not a ProjectMap type workspace
            ICollection<ProPricerDTO> formatDtos = (ws.IsProjectMapWorkspace ? ws.ProPricerExports.Where(p => p.IsProjectMapTemplate) : ws.ProPricerExports.Where(p => !p.IsProjectMapTemplate)).ToList();
            IReadOnlyCollection<CustomFieldDTO> availableCustomFields = ws.CustomFields;

            List<ExportToProPricerModelView> theModelViews = formatDtos.Select(f => new ExportToProPricerModelView(f, this.Factory, availableCustomFields)).ToList();

            theModelViews.Sort();

            ViewResult toReturn = this.View(WebConstants.VIEW_EXPORT_TO_PROPRICER_GRID, theModelViews);

            // Finalize Action
	        this.FinalizeAction(this.log, "DisplayProPricerGrid", sw);
            return toReturn;
        }

        /// <summary>
        /// Return dialog for selection of criteria for the BOE custom export
        /// </summary>
        /// <param name="workspace">Workspace name</param>
        /// <param name="selections">User's filter selections</param>
        /// <returns>Dialog view</returns>
        public PartialViewResult DisplayBoeCustomReportSelector(string workspace, BoeCustomReportSelections selections)
        {
            if (selections == null)
            {
                throw new ArgumentNullException(nameof(selections));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            BoeCustomReportSortBy selectedSortBy = selections.BoeSortBy.GetEnumeratedValue<BoeCustomReportSortBy>(BoeCustomReportSortBy.SelectSortByCriteria);
            BoeCustomReportSortBy secondarySelectedSortBy = selections.BoeSecondarySortBy.GetEnumeratedValue<BoeCustomReportSortBy>(BoeCustomReportSortBy.SelectSortByCriteria);

            // generate the complete set of all BOEs for this workspace
            ICollection<BoeCustomReportBoeData> boeData = this.GetBoeDataForWorkspace(ws, selectedSortBy, secondarySelectedSortBy);

            CustomReportSelectorModelView viewModelCustomReport = new CustomReportSelectorModelView(workspace, boeData, selectedSortBy, secondarySelectedSortBy, selections, ws.UsingTemplateBOE);
            ViewData["ContainsOCI"] = ws.ContainsOCI.ToString().ToLower();

            return this.PartialView(WebConstants.VIEW_BOE_CUSTOM_REPORT_SELECTOR, viewModelCustomReport);
        }

        /// <summary>
        /// Returns grid to display all INL Form Export options
        /// </summary>
        /// <param name="workspace">Workspace name</param>
        /// <returns>ViewResult</returns>
        public ViewResult DisplayInlFormExportGrid(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DisplayInlFormExportGrid", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            foreach (ValidationMessage v in reportsControllerLogic.DisplayInlFormExportGrid_Validate(ws))
            {
                ModelState.AddModelError(v.FieldName, v.ValidationIssue);
            }
            ICollection<BOEFormModelView> forms = this.boeFormControllerLogic.GetSummaryForms(ws);

            ViewResult toReturn = View(WebConstants.VIEW_INL_FORM_EXPORT_SELECTOR_GRID, forms);

            // Action Finalize
            FinalizeAction(log, "DisplayInlFormExportGrid", sw);

            return toReturn;
        }

        /// <summary>
        /// Generates the report and returns a nonce.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>A Json result containing the Nonce generated, the Nonce will be used to retrieve the XML from the Database.</returns>
        public JsonResult GenerateReportNonce(string workspace, Reports reportType)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(this.log, "GenerateReportNonce", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            string nonce = this.ssrsControllerLogic.GenerateReportNonce(ws, reportType);

            // Finalize Action
            this.FinalizeAction(this.log, "GenerateReportNonce", sw);

            return Json(new { Nonce = nonce });
        }

        /// <summary>
        /// Generate BOE custom export
        /// </summary>
        /// <param name="workspace">Workspace name</param>
        /// <param name="BoesSelected">Selected list of BOEs</param>
        /// <param name="ComponentsSelected">Selected report components</param>
        /// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template; null otherwise.</param>
        /// <returns>Report</returns>
        public ActionResult ExportCustomReport(string workspace, ICollection<int> BoesSelected, ICollection<BoeCustomReportComponent> ComponentsSelected, string summarizeByCustomField)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            return this.ExportAllBOEsReport(ws, summarizeByCustomField, BoesSelected, ComponentsSelected, custom: true);
        }

        private ICollection<BoeCustomReportBoeData> GetBoeDataForWorkspace(FullWorkspace workspaceObject, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy)
        {
            // Thread-safe container for the Boe Data of the current Workspace
            ConcurrentBag<BoeCustomReportBoeData> allData = new ConcurrentBag<BoeCustomReportBoeData>();


            System.Threading.Tasks.Parallel.ForEach(workspaceObject.Boes, boe =>
            {
                // get data for all sortable fields

                string wbsDisplayValue = string.Empty;
                string wbsNumber = string.Empty;
                string wbsTitle = string.Empty;
                string clinDisplayValue = string.Empty;
                string clinNumber = string.Empty;
                string clinTitle = string.Empty;
                string boeTitle = string.Empty;

                int? wbsID = null;
                int? clinID = null;

                IList<BoeCustomReportAuthorData> boeAuthors = new List<BoeCustomReportAuthorData>();
                IList<BoeCustomReportCustomFieldData> boeCustomFields = new List<BoeCustomReportCustomFieldData>();

                // always
                if (boe.Wbs != null)
                {
                    WbsDTO wbs = boe.Wbs;
                    if (wbs != null)
                    {
                        wbsID = wbs.Id;
                        wbsNumber = wbs.WbsNumber ?? string.Empty;
                        wbsTitle = wbs.WbsTitle ?? string.Empty;
                        wbsDisplayValue = string.Format("{0} {1}", wbsNumber, wbsTitle);
                    }
                }

                // always
                if (boe.Clin != null)
                {
                    ClinDTO clin = boe.Clin;
                    if (clin != null)
                    {
                        clinID = clin.Id;
                        clinNumber = clin.ClinNumber ?? string.Empty;
                        clinTitle = clin.ClinTitle ?? string.Empty;
                        clinDisplayValue = string.Format("{0} {1}", clinNumber, clinTitle);
                    }
                }

                // always
                boeTitle = boe.Title ?? string.Empty;

                // sort by Author ONLY, using both employee authors and subcontractor authors
                if ((sortBy == BoeCustomReportSortBy.Author || secondarySortBy == BoeCustomReportSortBy.Author) && (boe.AuthorIDs.Any() || boe.SubcontractorAuthorIDs.Any()))
                {
                    ICollection<int> allAuthors = boe.AuthorIDs.Union(boe.SubcontractorAuthorIDs).ToList();

                    foreach (int authorID in allAuthors)
                    {
                        UserDTO author = this.UserLoader.GetUserByID(authorID);

                        if (author != null)
                        {
                            string authorFirstName = author.FirstName ?? string.Empty;
                            string authorMiddleName = author.MiddleName ?? string.Empty;
                            string authorLastName = author.LastName ?? string.Empty;

                            // Check if the user is a subcontractor
                            bool isSubcontractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspaceObject.Id)
	        where p.Role == Role.SubcontractorAuthor && p.ETIUserId == author.UserID
	        select p).Any();

                            boeAuthors.Add(new BoeCustomReportAuthorData
                            {
                                AuthorUserID = author.UserID,
                                AuthorFirstName = authorFirstName,
                                AuthorMiddleName = authorMiddleName,
                                AuthorLastName = authorLastName,
                                AuthorDisplayValue = author.DisplayName,
                                IsSubcontractor = isSubcontractor
                            });
                        }
                    }
                }

                // sort by Custom Fields ONLY
                if (sortBy == BoeCustomReportSortBy.BOECustomField || secondarySortBy == BoeCustomReportSortBy.BOECustomField)
                {
                    ICollection<CustomFieldValueDTO> customFields = this.customFieldValueDTODataLoader.GetByIds(boe.CustomFieldValueContainers.Select(i => i.CustomFieldValueID).ToCollection<int>());
                    foreach (CustomFieldValueDTO customField in customFields)
                    {
                        boeCustomFields.Add(new BoeCustomReportCustomFieldData
                        {
                            CustomFieldValueID = customField.CustomFieldValueID,
                            CustomFieldValueName = customField.CustomFieldValueName ?? string.Empty,
                            CustomFieldValueDescription = customField.CustomFieldValueDescription ?? string.Empty
                        });
                    }
                }

                BoeCustomReportBoeData data = new BoeCustomReportBoeData
                {
                    BOEID = boe.Id,
                    BoeTitle = boeTitle,
                    WBSID = wbsID,
                    WbsDisplayValue = wbsDisplayValue,
                    WbsNumber = wbsNumber,
                    WbsTitle = wbsTitle,
                    CLINID = clinID,
                    ClinDisplayValue = clinDisplayValue,
                    ClinNumber = clinNumber,
                    ClinTitle = clinTitle,
                    Authors = boeAuthors,
                    CustomFields = boeCustomFields
                };

                allData.Add(data);
            });

            return allData.ToList();
        }

        #endregion Display

        #region AJAX calls

        /// <summary>
        /// Reusable logic for exporting the "All BOEs" report
        /// </summary>
        /// <param name="workspace">The current workspace name</param>
        /// <param name="workspaceID">The corresponding workspace ID</param>
        /// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
        /// <param name="selectedBOEs">List of BOEs to be included in the report; if null, then include ALL</param>
        /// <param name="selectedComponents">List of resources to be included in the report; if null, then include ALL</param>
        /// <param name="segmented">Whether the output should be broken into segments and zipped</param>
        /// <returns>Contents of the ALL BOEs report</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The UI might hang indefinitely, never returning control to the user, unless all exceptions are handled.")]
        private ActionResult ExportAllBOEsReport(FullWorkspace workspace, string summarizeByCustomField, ICollection<int> selectedBOEs, ICollection<BoeCustomReportComponent> selectedComponents, bool custom, bool segmented = false)
        {
            ActionResult result = new EmptyResult();

            try
            {
                bool isCustomExport;
                WorkspaceExportFormatDTO wsExportFormatDTO;
                BOEExportInputs exportInputs;
                ICollection<BOEExportModelView> boeExportModelViews;
                List<BOESummaryGridModelView> boeSummaryGridModelViews;

                this.reportsControllerLogic.PrepareAllBOEsReport(workspace, IsSubcontractorUser(workspace), summarizeByCustomField, selectedBOEs, ViewData, out isCustomExport, 
                    out wsExportFormatDTO, out exportInputs, out boeExportModelViews, out boeSummaryGridModelViews, custom);
                this.reportsControllerLogic.ExportAllBOEsReport(workspace, selectedComponents, Response, isCustomExport, wsExportFormatDTO, exportInputs, 
                    boeExportModelViews, boeSummaryGridModelViews, segmented);
            }
            catch (GenValidationException ex)
            {
                result = this.CreateTextFileWithErrorMessage(ex.Message);
            }
            catch (Exception e)
            {
                log.Error(e);

                result = this.CreateTextFileWithErrorMessage(e);
            }

            return result;
        }

        /// <summary>
        /// Determines whether current user has a subcontractor role in the current workspace
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>True if user has a subcontractor role in the current workspace</returns>
        private bool IsSubcontractorUser(FullWorkspace workspace)
        {
            Collection<PermissionsDTO> potentialPermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);
            UserDTO currentUser = workspace.CurrentActiveUser;
            bool isSubcontractorUser = potentialPermissions.Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == currentUser.UserID);
            return isSubcontractorUser;
        }

        /// <summary>
        /// Exports a report
        /// </summary>
        /// <param name="workspace">The current workspace name</param>
        /// <param name="reportID">The report ID</param>
        /// <param name="summarizeByCustomField">Name of custom field to group by when running All BOEs report with special format template.</param>
        /// <returns>Report</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "reportModelView")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The UI might hang indefinitely, never returning control to the user, unless all exceptions are handled.")]
        public ActionResult Export(string workspace, int reportID, string summarizeByCustomField)
        {
            ActionResult toReturn = null;
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
                
            try
            {
                
                // Initialize Action
                Stopwatch sw = this.InitializeAction(this.log, "Export", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);
                this.log.Performance("Exporting - ReportsController - Begin", 0);

                if (reportID == (int)Reports.BOEStatus ||
                    reportID == (int)Reports.BOEStatusByBOE ||
                    reportID == (int)Reports.BOEStatusByWBS ||
                    reportID == (int)Reports.BOEStatusByCLIN)
                {
                    // Call the BL to generate the status report
                    BOEExportInputs exportInputs = this.reportsControllerLogic.GetExportInputsForStatusAndWbsReports(ws);
                    Collection<BOEStatusReportModelView> theModelViews = this._BOEStatusReport.GenerateBOEStatusReport(exportInputs);

                    if (theModelViews.Count > 0)
                    {
                        string templateFileName;

                        switch (reportID)
                        {
                            case (int)Reports.BOEStatusByWBS:
                                // Get BOE Status Report template file name
                                templateFileName = Server.MapPath("~/Templates/Export/BOEStatusByWBS.xlsx");
                                break;

                            case (int)Reports.BOEStatusByCLIN:
                                // Get BOE Status Report template file name
                                templateFileName = Server.MapPath("~/Templates/Export/BOEStatusByCLIN.xlsx");
                                break;

                            default:
                                // Get BOE Status Report template file name
                                templateFileName = Server.MapPath("~/Templates/Export/BOEStatusByBOE.xlsx");
                                break;
                        }

                        // Generate an export file from the data
                        string exportedFileName = this._BOEStatusReport.SendBOEStatusReportToFile(templateFileName, theModelViews, reportID, exportInputs);

                        // Pass the file to the user
                        string fileName = string.Format("BOEStatusExport_{0}.xlsx", ws.WorkspaceName);
                        // Generate a custom ActionResult to cause a file download to the client
                        FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                        toReturn = File(
                            fileStream: fs,
                            contentType: ExportFileDownloadBase.GetContentType(fileName),
                            fileDownloadName: fileName);
                    }
                }
                else if (reportID == (int)Reports.BOEActivity)
                {
                    // Call the BL to generate the status report
                    BOEActivityReportModelView theModelView = this._BOEActivityReport.GenerateReport(ws);

                    // Get BOE Status Report template file name
                    string templateFileName = Server.MapPath("~/Templates/Export/BOEActivityReport.xlsx");

                    // Generate an export file from the data
                    string exportedFileName = this._BOEActivityReport.SendBOEActivityReportToFile(templateFileName, theModelView);

                    // Pass the file to the user
                    string fileName = string.Format("BOEActivityReport_{0}.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
                //add 
                //
                else if (reportID == (int)Reports.AllBOEs || reportID == (int)Reports.StandardReports)
                {
                    toReturn = this.ExportAllBOEsReport(ws, summarizeByCustomField, null, null, false);
                }
                else if (reportID == (int)Reports.AllBOEsSegmented)
                {
                    toReturn = this.ExportAllBOEsReport(ws, summarizeByCustomField, null, null, false, true);
                }
                else if (reportID == (int)Reports.WorkspaceData)
                {
					bool isOffloading = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload;
                    List<FullBoe> boes;
                    List<BoeTaskElementDTO> tasks;
					// Get RTE overrides
					ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateOverrides = ws.TemplateQuestionsAndAnswers.ToList();

                    ws.LoadODCsRTEData();
					ws.LoadMaterialsRTEData();

					if (!isOffloading)
                    {
                        // offloading will create a copy of workspace so no need to retrieve data twice
                        ws.LoadBoesAndTaskElementsRTEData();
                        ws.LoadTravelRTEData();
                        
                        // All BOEs for the workspace as a default
                        boes = ws.Boes.ToList();
                        tasks = ws.TaskElements.OrderBy(t => t.BOETaskElementOrder).ToList();
                    }
                    else
                    {
						boes = ws.Boes.ToList();
						OffloadLaborRates offloader = new OffloadLaborRates();
                        OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes, ws);

                        boes = results.Boes.ToList();
                        tasks = boes.SelectMany(b => b.TaskElements).OrderBy(t => t.BOETaskElementOrder).ToList();
                        rteTemplateOverrides = boes.SelectMany(x => x.TemplateQuestionsAndAnswers).ToList();
                    }

                    BOEExportInputs exportInputs = new BOEExportInputs(boes, ws.Boes.ToList(), tasks, ws, rteTemplateOverrides, ws.MoqTypeSelections.ToList());
                    // Need picklist values for contract type for Workspace Identification sheet
                    exportInputs.ContractTypes = this.contractTypeLoader.GetPickListValues();

                    MetricNameTaskElementMappingDTO metricTaskElementMappings = this.reportsControllerLogic.GetMetricNameTaskElementMappingDTO(ws);
                    ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
                    string exportedFileName = this.workspaceExporter.ExportToExcelFile(Server.MapPath(workspaceExporter.WORKSPACE_DATA_EXCEL_MAP_PATH), exportInputs, metricTaskElementMappings, contractTypes);

                    // Generate a custom ActionResult to cause a file download to the client
                    string fileName = string.Format("{0}_WorkspaceData.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
                else if (reportID == (int)Reports.TravelUnitCost)
                {
                    string fileName = (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) ? "~/Templates/Export/TravelUnitCostRMS.xlsx" : "~/Templates/Export/TravelUnitCost.xlsx";
                    string exportedFileName = this.travelUnitCostExporter.ExportToExcelFile(Server.MapPath(fileName), ws);

                    // Generate a custom ActionResult to cause a file download to the client
                    fileName = string.Format("{0}_TravelUnitCost.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
                else if (reportID == (int)Reports.TravelExtendedCost)
                {
                    string fileName = (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) ? "~/Templates/Export/TravelExtendedCostRMS.xlsx" : "~/Templates/Export/TravelExtendedCost.xlsx";
                    string exportedFileName = this.travelExtendedCostExporter.ExportToExcelFile(Server.MapPath(fileName), ws);

                    // Generate a custom ActionResult to cause a file download to the client
                    fileName = string.Format("{0}_TravelExtendedCost.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
                else if (reportID == (int)Reports.WbsBoeReport)
                {
                    BOEExportInputs exportInputs = this.reportsControllerLogic.GetExportInputsForStatusAndWbsReports(ws);
                    ICollection<BoeWbsReportModelView> reportModelView = this.reportsControllerLogic.GenerateWbsBoeReport(exportInputs);

                    string exportedFileName = this.reportsControllerLogic.ExportWbsBoeReport(ws, Server.MapPath("~/Templates/Export/WbsBoeReport.xlsx"), reportModelView, exportInputs);

                    string fileName = string.Format("{0}_WbsSummaryReport.xlsx", ws.WorkspaceName);
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
                else
                {
                    toReturn = new EmptyResult();
                }

                // Finalize Action
                this.FinalizeAction(this.log, "Exporting - ReportsController - End", sw);
            }
            catch (GenValidationException ex)
            {
                toReturn = this.CreateTextFileWithErrorMessage(ex.Message);
            }
            catch (Exception e)  // The UI might hang indefinitely, never returning control to the user, unless all exceptions are handled.
            {
                this.log.Error(e);
                if (ws.IsProjectMapWorkspace)
                {
					string supportLink = Utilities.ServiceCentralLink();

					toReturn = this.CreateTextFileWithErrorMessage(string.Format("An error has occurred. This might be the result of invalid data such as missing Offload Rates. If the data is valid, and the error persists, please contact the GenBOE Helpdesk at {0}.", supportLink));
                }
                else
                {
                    toReturn = this.CreateTextFileWithErrorMessage(e);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a ProPricer Export Format
        /// </summary>
        /// <param name="inModelView">The export format to save/delete</param>
        /// <param name="workspace">The workspace</param>
        /// <returns>json result</returns>
        [ProPricerExportAccess]
        public JsonResult SaveProPricerExportFormat(ExportToProPricerModelView inModelView, string workspace)
        {
            if (inModelView == null) { throw new ArgumentNullException(nameof(inModelView)); }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            ProPricerDTO original = ws.ProPricerExports.FirstOrDefault(p => p.ExportID == inModelView.ID);
            bool isOriginalSystemScope = original != null ? original.Scope == ProPricerScope.System : false;
            // Initialize Action
            Stopwatch sw = InitializeAction(log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT, SecurityPage.ExportToProPricer, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            JsonResult toReturn = Json(new { Status = true });

            // throw validation error if this is for a system scope
            if (inModelView.Scope == ProPricerScope.System || isOriginalSystemScope)
            {
                throw new GenValidationException("System ProPricer Exports cannot be deleted or updated. Please save as a Workspace ProPricer Export.");
            }

            ProPricerDTO dto = inModelView.GetAssociatedDTO();
            dto.WorkspaceID = ws.Id;
            
            // If this is not a deletion, validate the modelView
            if (!inModelView.Deleted)
            {
                if (ModelState.IsValid)
                {
                    // Validate Format
                    ExportToProPricerFormatValidator validator = new ExportToProPricerFormatValidator(this.Factory);
                    Collection<string> validationerrors = validator.validation(dto, (Collection<Dictionary<string, string>>)null);

                    if (validationerrors.Count != 0)
                    {
                        throw new GenValidationException(ExportToProPricerFormatValidator.CreateValidationErrorResponse(validationerrors));
                    }
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                proPricerLoader.SaveProPricerExport(dto);
                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(log, WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT, sw);
            return toReturn;
        }

        /// <summary>
        /// Deletes a collection of ProPricer Export Formats.
        /// </summary>
        /// <param name="inFormatsToDelete">The collection of export formats to delete</param>
        /// <returns>True</returns>
        [ProPricerExportAccess]
        public JsonResult DeleteProPricerExportFormats(ICollection<ExportToProPricerModelView> inFormatsToDelete, string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "DeleteProPricerExportFormats", SecurityPage.ExportToProPricer, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
            
            if (inFormatsToDelete == null)
            {
                throw new ArgumentNullException(nameof(inFormatsToDelete));
            }

            if (inFormatsToDelete.Count() > 0)
            {
                foreach (ExportToProPricerModelView format in inFormatsToDelete)
                {
                    // Save each deleted format
                    if (format.Deleted)
                    {
                        SaveProPricerExportFormat(format, workspace);
                    }
                }
            }

            // Finalize Action
            FinalizeAction(log, "DeleteProPricerExportFormats", sw);
            return Json(true);
        }

        /// <summary>
        /// Export a ProPricer Report
        /// </summary>
        /// <param name="inFormatID">The ID of the format to Export</param>
        /// <param name="workspace">The workspace</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [ProPricerExportAccess]
        public ActionResult ExportProPricerExportFormat(string workspace, int reportID, ProPricerScope scope)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "ExportProPricerExportFormat", SecurityPage.ExportToProPricer, SecurityAuthorization.Read, ws, null);

            ActionResult toReturn = null;

            try
            {
                if (reportID > 0)
                {
                    string lockKey = string.Intern("Export_ProPricer_" + workspace);
                    lock(lockKey)
                    {
                        // Get the workspace again in case the user clicked twice
                        ws = this.Factory.CreateFullWorkspace(workspace);

                        // once the export is complete, increment the total of exports for this workspace for reporting tracking purposes
                        ws.NumberOfTimesExportedToProPricer = ++ws.NumberOfTimesExportedToProPricer;

                        // Get the user who is saving the BOE(s)
                        int currentUserID = ws.CurrentActiveUser.UserID;

                        using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            this.workspaceLoader.SaveWorkspaceSettings(currentUserID, ws);
                            transactionScope.Complete();
                        }

                        // Clear the cache after since LastUpdateTime was changed
                        this.Factory.ClearWorkspaceCache(ws.Shortname);

                        // Get the format
                        ProPricerDTO format = proPricerLoader.GetById(reportID, scope);

                        if (format != null)
                        {
                            // Check that format is part of this workspace
                            if (format.Scope != ProPricerScope.System && format.WorkspaceID != ws.Id)
                            {
                                throw new GenValidationException("The format selected for export is not part of the current workspace.");
                            }

                            if (format.Scope == ProPricerScope.System)
                            {
                                // If the template is scoped at the system level, run it thru the model view. The reason for this is; if 
                                // there are custom fields in the system level template, their Ids are incorrect for the current workspace.
                                // Running thru the model view will re-map the custom field Ids to those of the current workspace.
                                ExportToProPricerModelView modelView = new ExportToProPricerModelView(format, this.Factory, ws.CustomFields);
                                format = modelView.GetAssociatedDTO();
                            }

                            // Export the format to file
                            string filePath = proPricerExporter.ExportReport(format, Server.MapPath("~/Templates/Export"), ws);

                            if (filePath.Length > 0)
                            {
                                // Return the file to the user
                                string fileName = "ProPricerExport_" + ws.WorkspaceName + ".zip";
                                // Generate a custom ActionResult to cause a file download to the client
                                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                                toReturn = File(
                                    fileStream: fs,
                                    contentType: ExportFileDownloadBase.GetContentType(fileName),
                                    fileDownloadName: fileName);
                            }
                        }
                    }
                }
            }
            catch (GenValidationException ex)
            {
                // clear the workspace in case the user clicked export twice (so subsequent retrievals of workspace gets latest update time from DB)
                this.Factory.ClearWorkspaceCache(workspace);
                toReturn = this.CreateTextFileWithErrorMessage(ex.Message);
            }
            catch (Exception e)
            {
                // clear the workspace in case the user clicked export twice (so subsequent retrievals of workspace gets latest update time from DB)
                this.Factory.ClearWorkspaceCache(workspace);
                log.Error(e);

                toReturn = this.CreateTextFileWithErrorMessage(e);
            }

            // Finalize Action
            FinalizeAction(log, "ExportProPricerExportFormat", sw);
            return toReturn;
        }
        
        /// <summary>
        /// Sends the view HTML as an Excel document
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <returns>Excel document for export</returns>
        public ActionResult ExportBoeDiscrepancyReport(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "ExportBoeDiscrepancyReport", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ViewData["ExportBoeDiscrepancyReport"] = true;
            Response.AddHeader("Content-Type", "application/vnd.ms-excel");
            Response.AppendHeader("Content-Disposition", "attachment;filename=BoeDiscrepancyReport-" + ws.WorkspaceName + ".xls");

            ICollection<BoeDiscrepancyReportModelView> theModelViews = this.reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, false);
            ViewResult toReturn = View(WebConstants.VIEW_BOE_DISCREPANCY_REPORT, theModelViews);

            // Finalize Action
            FinalizeAction(log, "ExportBoeDiscrepancyReport", sw);
            return toReturn;
        }

        /// <summary>
        /// Bulk downloads the reports.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="downloadReports">The download reports.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">downloadReports</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ActionResult BulkDownloadReports(string workspace, ICollection<DownloadReportModelView> downloadReports)
        {
            if (downloadReports == null || downloadReports.None())
            {
                throw new ArgumentNullException(nameof(downloadReports));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(log, "BulkDownloadReports", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

            ActionResult toReturn = null;

            try
            {
                // Export the format to file
                string filePath = this.ssrsControllerLogic.BulkDownloadReport(ws, downloadReports, Server.MapPath("~/Templates/Export"));

                if (filePath.Length > 0)
                {
                    // Return the file to the user
                    string fileName = "BulkReportExport_" + ws.WorkspaceName + ".zip";
                    // Generate a custom ActionResult to cause a file download to the client
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                    toReturn = File(
                        fileStream: fs,
                        contentType: ExportFileDownloadBase.GetContentType(fileName),
                        fileDownloadName: fileName);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }

            // Finalize Action
            FinalizeAction(log, "BulkDownloadReports", sw);
            return toReturn;

        }

        /// <summary>
        /// Gets the resulting validation data for the BOE Discrepancy Report as a JSON object.
        /// </summary>
        /// <param name="workspace">The workspace short name.</param>
        /// <returns>JSON object for the validation data.</returns>
        public JsonResult ValidateBoesForDiscrepancies(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            ICollection<BoeDiscrepancyReportModelView> theModelViews = new List<BoeDiscrepancyReportModelView>();

            // for project maps we do not populate the MOQ equation, which would cause the BOE Discrepancy report to generate errors; so we are going to skip it
            if (!ws.IsProjectMapWorkspace)
            {
                // Initialize Action
                Stopwatch sw = this.InitializeAction(this.log, "ValidateBoesForDiscrepancies", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

                theModelViews = this.reportsControllerLogic.GenerateDataForBoeDiscrepancyReport(ws, false);

                // Finalize Action
                this.FinalizeAction(this.log, "ValidateBoesForDiscrepancies", sw);
            }

            return this.Json(theModelViews);
        }

        /// <summary>
        /// Copy a Workspace level Propricer Format into System level
        /// </summary>
        /// <param name="workspace">The workspace short name.</param>
        /// <param name="reportId">The id of the report to copy.</param>
        /// <returns>JSON object for the validation data.</returns>
        public JsonResult CopyFormatToSystemLevel(string workspace, int reportId)
        {
            if (reportId < 1)
            {
                throw new GenValidationException("The report Id is required to make a copy.");
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = this.InitializeAction(log, WebConstants.ACTION_COPY_FORMAT_TO_SYSTEM_LEVEL, SecurityPage.Reports, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            ICollection<ProPricerDTO> formatDtos = (ws.IsProjectMapWorkspace ? ws.ProPricerExports.Where(p => p.IsProjectMapTemplate) : ws.ProPricerExports.Where(p => !p.IsProjectMapTemplate)).ToList();
            IReadOnlyCollection<CustomFieldDTO> availableCustomFields = ws.CustomFields;

            ProPricerDTO proPricerFormat = formatDtos.FirstOrDefault(f => f.ExportID == reportId && f.Scope == ProPricerScope.Workspace);
            if (proPricerFormat == null)
            {
                throw new GenValidationException("Report to copy not found in this workspace.");
            }
            else
            {
                proPricerFormat.Id = -1;
                proPricerFormat.ExportID = -1;
                proPricerFormat.Scope = ProPricerScope.System;
                proPricerFormat.Updateable = UpdateType.Upsert;

                foreach (ProPricerTasks task in proPricerFormat.ProPricerTasks)
                {
                    if (task.CustomFieldID != null)
                    {
                        // find the custom field
                        CustomFieldDTO customField = availableCustomFields.First(c => c.Id == task.CustomFieldID);
                        task.CustomFieldName = customField.CustomFieldName;
                        task.CustomFieldID = null;
                    }
                }

                foreach (ProPricerResources resource in proPricerFormat.ProPricerResources)
                {
                    if (resource.CustomFieldID != null)
                    {
                        // find the custom field
                        CustomFieldDTO customField = availableCustomFields.First(c => c.Id == resource.CustomFieldID);
                        resource.CustomFieldName = customField.CustomFieldName;
                        resource.CustomFieldID = null;
                    }
                }

                // Validate Format
                SystemExportToProPricerFormatValidator validator = new SystemExportToProPricerFormatValidator(this.proPricerLoader);
                Collection<string> validationerrors = validator.validation(proPricerFormat, (Collection<Dictionary<string, string>>)null);

                if (validationerrors.Count != 0)
                {
                    throw new GenValidationException(SystemExportToProPricerFormatValidator.CreateValidationErrorResponse(validationerrors));
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this.proPricerLoader.SaveSystemProPricerExport(proPricerFormat);
                    scope.Complete();
                }
            }

            // Finalize Action
            this.FinalizeAction(log, "BulkDownloadReports", sw);
            return this.Json(new { Status = true });
        }

        #endregion
    }
}
