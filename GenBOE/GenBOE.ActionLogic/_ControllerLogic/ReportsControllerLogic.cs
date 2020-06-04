// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.ActionLogic.Workspace;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Action Logic for the Reports Controller
    /// </summary>
    public class ReportsControllerLogic : IReportsControllerLogic
    {
        /// <summary>
        /// The AD utils
        /// </summary>
        private readonly IBOEExporter boeExporter;
        private readonly BOESummary boeSummary;
        private readonly IBOECustomExporter boeCustomExporter;
        private readonly IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader;
        private readonly BOEDiscrepancyReport boeDiscrepancyReport;
        private readonly IProposalLoader proposalLoader;
        private readonly IWorkspaceControllerLogic workspaceControllerLogic;
        private readonly IRteTemplateDataLoader rteTemplateDataLoader;

        #region Cache Setup

        /// <summary>
        /// Cache Object
        /// </summary>
        private MemoryCache cache;

        /// <summary>
        /// The number of seconds to store in cache - 10 minutes
        /// 
        /// This can be extended to this long because it's manually cleared when you visit the report on the page; so the cached data will only be used when
        /// you are on the report page, and hit export.. It's meant to avoid recalculations for the export generation.
        /// </summary>
        private const int SECONDS_TO_CACHE_DISCREPANCY_REPORT = 600;

        /// <summary>
        /// Key for the cache
        /// </summary>
        private const string CACHE_KEY_BOE_DISCREPANCY_REPORT_BY_WORKSPACE = "boeDiscrepancyReportForWorkspace_";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsControllerLogic"/> class.
        /// </summary>
        /// <param name="boeExporter">The boe exporter.</param>
        /// <param name="boeSummary">The boe summary.</param>
        /// <param name="boeCustomExporter">The boe custom exporter.</param>
        /// <param name="workspaceExportFormatDTOLoader">The workspace export format dto loader.</param>
        /// <param name="boeDiscrepancyReport">The boe discrepancy report.</param>
        /// <param name="proposalLoader">Proposal Loader</param>
        /// <param name="workspaceControllerLogic">Workspace Controller Logic</param>
        /// <param name="rteTemplateDataLoader">The RTE Template dto loader.</param>
        public ReportsControllerLogic(
            IBOEExporter boeExporter,
            BOESummary boeSummary,
            IBOECustomExporter boeCustomExporter,
            IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader,
            BOEDiscrepancyReport boeDiscrepancyReport,
            IProposalLoader proposalLoader,
            IWorkspaceControllerLogic workspaceControllerLogic,
            IRteTemplateDataLoader rteTemplateDataLoader)
        {
            this.boeExporter = boeExporter;
            this.boeSummary = boeSummary;
            this.boeCustomExporter = boeCustomExporter;
            this.workspaceExportFormatDTOLoader = workspaceExportFormatDTOLoader;
            this.boeDiscrepancyReport = boeDiscrepancyReport;
            this.proposalLoader = proposalLoader;
            this.workspaceControllerLogic = workspaceControllerLogic;
            this.rteTemplateDataLoader = rteTemplateDataLoader;

            this.cache = new MemoryCache();
        }

        /// <summary>
        /// Gets a Boolean indicating if custom export is support per ISGS company configuration
        /// </summary>
        public virtual bool SupportCustomExport { get { return false; } }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "9")]
        public void PrepareAllBOEsReport(FullWorkspace workspace, bool isSubcontractorUser, string summarizeByCustomField, ICollection<int> selectedBOEs,
            ViewDataDictionary viewDataDictionary, out bool isCustomExport, out WorkspaceExportFormatDTO wsExportFormatDTO, out BOEExportInputs exportInputs, 
            out ICollection<BOEExportModelView> boeExportModelViews, out List<BOESummaryGridModelView> boeSummaryGridModelViews, bool custom = false)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            // Pre-load the RTE data since this is faster than loading all objects in ResourcesUsedInWsBoes property, then later adding RTE data to each object.
            workspace.LoadBoesRTEData();
            workspace.LoadTravelRTEData();
            workspace.LoadODCsRTEData();
            workspace.LoadMaterialsRTEData();
            workspace.LoadTaskElementRTEData();

            // All BOEs for the workspace as a default
            List<FullBoe> boes = workspace.Boes.ToList();
            List<BoeTaskElementDTO> tasks = workspace.TaskElements.ToList();
            boeSummaryGridModelViews = new List<BOESummaryGridModelView>();

            #region Override assigned output format template

            // Get template based on workspace preferences
            wsExportFormatDTO = workspace.WorkspaceExportFormats.First(x => x.ExportFormat.TemplateId == workspace.TemplateID);

            #endregion

            isCustomExport = custom || (wsExportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.MASTER);
            bool isOffloading = workspace.ProjectMapType != ProjectMapType.StandardWithoutOffload;
            if (selectedBOEs != null && selectedBOEs.Any())
            {
                // This seems silly, but we need to order the BOEs for the export the way they were requested..
                List<FullBoe> selectedAndOrderedBoes = new List<FullBoe>();

                foreach (int boeId in selectedBOEs)
                {
                    selectedAndOrderedBoes.Add(boes.Single(x => x.Id == boeId));
                }

                boes = selectedAndOrderedBoes;
            }

            if (isOffloading)
            {
                OffloadLaborRates offloader = new OffloadLaborRates();
                List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
                OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), workspace);

                boes = results.Boes.ToList();
                tasks = boes.SelectMany(b => b.TaskElements).ToList();
            }

            if (workspace.IsProjectMapWorkspace)
            {
                boes = ProjectMapSorter.OrderBoes(boes, workspace).ToList();
            }

            // Get RTE overrides
            ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateOverrides = this.rteTemplateDataLoader.GetByWorkspaceId(workspace.Id, boes);

            exportInputs = new BOEExportInputs(boes, workspace.Boes.ToList(), tasks, workspace, rteTemplateOverrides);
            exportInputs.SummarizeByCustomField = summarizeByCustomField;

            if (isCustomExport)
            {
                this.boeCustomExporter.SetWorkspacePrecisionVariables(workspace);
            }
            else
            {
                this.boeExporter.SetWorkspacePrecisionVariables(workspace);
            }

            foreach (FullBoe boe in boes)
            {
                // Get BOE Summary Grid data for the current BOE. Used for populating the summary grid on the template
                boeSummaryGridModelViews.AddRange(this.boeSummary.GetBOESummaryGridModelViews(boe, exportInputs, isSubcontractorUser));
            }

            // Get BOE Export Model View for the current BOE. Used for filling in most of the data on the template.
            // Note that an Export All using the Master template is treated like a Custom export
            if (isCustomExport)
            {
                boeExportModelViews = this.boeCustomExporter.ConvertBoeDTOsToExportMVs(boes, exportInputs);
            }
            else
            {
                boeExportModelViews = this.boeExporter.ConvertBoeDTOsToExportMVs(exportInputs);
            }


            // incoming boeExportModelViews are in the "Include in Report" order for custom exports - don't sort by sortField in that case
            if (!workspace.IsProjectMapWorkspace)
            {
                if (!custom || selectedBOEs == null || !selectedBOEs.Any())
                {
                    int sortField = workspace.BOEExportSortByID;

                    string sortDirection = "Asc";
                    boeExportModelViews = SortAllBOEReport(boeExportModelViews, sortField, sortDirection, viewDataDictionary);
                }
            }
        }

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
        public void ExportAllBOEsReport(FullWorkspace workspace, ICollection<BoeCustomReportComponent> selectedComponents, HttpResponseBase httpResponse, bool isCustomExport, 
            WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, List<BOESummaryGridModelView> boeSummaryGridModelViews)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (wsExportFormatDTO == null)
            {
                throw new ArgumentNullException(nameof(wsExportFormatDTO));
            }

            if (isCustomExport)
            {
                // distinguish between MASTER and legacy templates
                // if legacy, use original MASTER, otherwise use selected template
                WorkspaceExportFormatDTO exportFormat = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId >10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;

                // Call the export function in the business layer and get back the file name of the populated template.
                this.boeCustomExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, selectedComponents, httpResponse, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty), exportFormat);
            }
            else
            {
                // Call the export function in the business layer and get back the file name of the populated template.
                this.boeExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, httpResponse, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType);
            }
        }

        /// <summary>
        /// Sorts an unordered collection of BOEExportModelViews
        /// </summary>
        /// <param name="theModelView">Collection of model views</param>
        /// <param name="sortField">Numeric value of enumerated type ExportSortBOEBy</param>
        /// <param name="sortDirection">Sort Direction</param>
        /// <param name="viewDataDictionary">View Data</param>
        /// <returns>Returns a sorted collection of BOEExportModelViews</returns>
        private static ICollection<BOEExportModelView> SortAllBOEReport(ICollection<BOEExportModelView> theModelView, int sortField, string sortDirection, ViewDataDictionary viewDataDictionary)
        {
            List<BOEExportModelView> theModelViewNoMulti = new List<BOEExportModelView>();

            if (viewDataDictionary != null)
            {
                viewDataDictionary["SortField"] = sortField;
                viewDataDictionary["SortDirection"] = sortDirection;
            }

            //Sort the BOEs by WBS or CLIN, but don't include Multi BOEs. They must always print last.
            if (sortField == (int)ExportSortBOEBy.WBS)
            {
                theModelViewNoMulti = new List<BOEExportModelView>((from x in theModelView where x.IsMultiClinWbs == false orderby x.PaddedWbsName, x.PaddedClinName select x).ToList());
                
            }
            else if (sortField == (int)ExportSortBOEBy.CLIN)
            {
                theModelViewNoMulti = new List<BOEExportModelView>((from x in theModelView where x.IsMultiClinWbs == false orderby x.PaddedClinName, x.PaddedWbsName select x).ToList());
            }

            //Sort the Multi BOEs by order of creation (id)
            List<BOEExportModelView> theModelViewOnlyMulti = new List<BOEExportModelView>((from x in theModelView where x.IsMultiClinWbs orderby x.BoeID select x).ToList());

            //Join the two sorted lists
            theModelViewNoMulti.AddRange(theModelViewOnlyMulti);
            theModelView = new Collection<BOEExportModelView>(theModelViewNoMulti);

            return theModelView;
        }

        /// <summary>
        /// Gets a mapping dto of task element to metric names for export.
        /// </summary>
        /// <param name="workspace">Full workspace</param>
        /// <returns>Metric name to task element mappings.</returns>
        public virtual MetricNameTaskElementMappingDTO GetMetricNameTaskElementMappingDTO(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            return new MetricNameTaskElementMappingDTO();
        }

        /// <summary>
        /// Generates Data for the Boe Discrepancy Report. Used by both the page and also Excel export..
        /// 
        /// This is cached for 5 minutes in case the user wants to export the data, so that way we don't have to keep rerunning all the data..
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="clearCache">Should be cleared when the web report is being displayed, to get a clean view; 
        ///                             For the export we should be using cache (since you can ONLY export once the web report has been generated.</param>
        /// <returns>Data for the Boe Discrepancy Report</returns>
        public ICollection<BoeDiscrepancyReportModelView> GenerateDataForBoeDiscrepancyReport(FullWorkspace ws, bool clearCache)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            string key = CACHE_KEY_BOE_DISCREPANCY_REPORT_BY_WORKSPACE + ws.WorkspaceName;
            if (clearCache) { this.cache.Remove(key); }

            ICollection<BoeDiscrepancyReportModelView> theModelViews;
            
            if (this.cache.Contains(key))
            {
                theModelViews = (ICollection<BoeDiscrepancyReportModelView>)this.cache.GetData(key);
            }
            else
            {
                this.cache.Remove(key);
                theModelViews = this.boeDiscrepancyReport.GetReport(ws);

                if (theModelViews != null)
                {
                    this.cache.Add(key, theModelViews, SECONDS_TO_CACHE_DISCREPANCY_REPORT);
                }
            }

            return theModelViews;
        }

        /// <summary>
        /// Performs Validation for DisplayInlFormExportGrid
        /// </summary>
        /// <param name="ws">FullWorkspace</param>
        /// <returns>ICollection of ValidationMessage.</returns>
        public virtual ICollection<ValidationMessage> DisplayInlFormExportGrid_Validate(FullWorkspace ws)
        {
            // nothing to validate!
            return new Collection<ValidationMessage>();
        }

        /// <summary>
        /// Get the Summary Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Summary Reports Model Views</returns>
        public virtual ICollection<SSRSReportsModelView> GetSummaryReportsModelViews()
        {
            // do nothing for ssc/default
            return null;
        }

        /// <summary>
        /// Get the Customer Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Customer Reports Model Views</returns>
        public virtual ICollection<SSRSReportsModelView> GetCustomerReportsModelViews()
        {
            // do nothing for ssc/default
            return null;
        }

        /// <summary>
        /// Get the Finance Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Finance Reports Model Views</returns>
        public virtual ICollection<SSRSReportsModelView> GetFinanceReportsModelViews()
        {
            // do nothing for ssc/default
            return null;
        }

        /// <summary>
        /// Get the Additional Reports Model Views for Project Map Workspaces
        /// </summary>
        /// <returns>Additional Reports Model Views</returns>
        public virtual ICollection<SSRSReportsModelView> GetAdditionalReportsModelViews()
        {
            // do nothing for ssc/default
            return null;
        }

        /// <summary>
        /// Gets the ssrs report URL.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>The Uri for the SSRS report.</returns>
        public virtual Uri GetReportUrl(Reports reportType)
        {
            // do nothing for ssc/default
            return null;
        }

        /// <summary>
        /// Determine if the AllBOEs export is using the template that allows summarization by custom fields.
        /// </summary>
        /// <param name="templateName">Name of Output Template being used by the AllBOEs export.</param>
        /// <returns>true, if the AllBOEs export is using the summary by custom fields template; false otherwise.</returns>
        public bool IsUsingSummarizeByCustomFieldTemplate(string templateName)
        {
            return templateName != null && templateName.Equals(BOEExporterConstants.ALL_BOE_SUMMARY_BY_CUSTOM_FIELD_TEMPLATE_NAME);
        }

        /// <summary>
        /// Get the set of "Summarize By" options when using the template that allows summarization by custom fields.
        /// </summary>
        /// <param name="customFields"></param>
        /// <returns>A list of options including "None" plus all resource level custom field names for the workspace.
        /// Returns an empty collection if there are no customFields at the resource level.</returns>
        public ICollection<SelectListItem> SummarizeByCustomFieldOptions(IReadOnlyCollection<CustomFieldDTO> customFields)
        {
            ICollection<SelectListItem> options = new Collection<SelectListItem>();
                
            if (customFields != null && customFields.Any(x => x.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
            {
                options.Add(new SelectListItem() { Value = BOEExporterConstants.SUMMARIZE_BY_NONE, Text = BOEExporterConstants.SUMMARIZE_BY_NONE, Selected = true });
                options.AddRange(customFields
                    .Where(x => x.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CustomFieldName,
                        Text = x.CustomFieldName
                    }).ToCollection());
            }

            return options;
        }

        /// <summary>
        /// Check if the PTM data is out of sync and return collection of messages if it is
        /// Data is out of sync if
        /// -Integration with PTM is turned on
        /// -Workspace is linked to a PTM record
        /// -Any PTM data is out of date
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>Collection of any messages if PTM data is out of sync</returns>
        public virtual ICollection<string> GetPtmDataOutOfSyncMessages(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ICollection<string> toReturn = new Collection<string>();

            if (Utilities.IsPTMIntegrated)
            {
                int proposalID = string.IsNullOrEmpty(ws.TrackingNumber)? -1 : this.proposalLoader.GetIdByTrackingNumber(ws.TrackingNumber);
                if (proposalID > 0)
                {
                    ProposalDto proposal = this.proposalLoader.GetById(proposalID);
                    if (proposal != null)
                    {
                        if (this.workspaceControllerLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID) != 
                            ws.LineOfBusiness.Id)
                        {
                            toReturn.Add("Line of Business");
                        }

                        if (this.workspaceControllerLogic.ConvertPTMProposalClassId(proposal.ProposalClass) !=
                            ws.ProposalClass.Id)
                        {
                            toReturn.Add("Proposal Class");
                        }

                        foreach (int contractTypeId in proposal.ContractTypeIds)
                        {
                            if (!ws.SelectedContractTypes.Contains(
                                this.workspaceControllerLogic.ConvertPTMContractTypeId(contractTypeId)))
                            {
                                toReturn.Add("Contract Types");
                                break; // only need to find one
                            }
                        }

                        if (proposal.DeliveryDate != ws.ProposalSubmittalDate)
                        {
                            toReturn.Add("Anticipated Delivery Date");
                        }

                        if (proposal.RevisedSubmittalDate != ws.RevisedSubmittalDate)
                        {
                            toReturn.Add("Revised Submittal Date");
                        }

                        if (proposal.RFPNumber != ws.RFPNumber)
                        {
                            toReturn.Add("RFP Number");
                        }

                        if (proposal.ProposalTitle != ws.ProposalTitle)
                        {
                            toReturn.Add("PTM Proposal Title");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(ws.TrackingNumber))
                {
                    toReturn.Add("PTM Tracking #");
                }
            }

            return toReturn;
        }
    }
}