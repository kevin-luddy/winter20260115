// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
    using System.Web.Mvc;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.Misc;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.ActionLogic.Reporting;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.ActionLogic.Workspace;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
	using UserDTO = Dtos.UserDTO;

	/// <summary>
	/// Action Logic for the Reports Controller
	/// </summary>
	public class ReportsControllerLogic : IReportsControllerLogic, IDisposable
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
        private readonly TravelTripCostCalculation travelTripCostCalculator;
		private readonly BOEReportsHttpService boeReportsHttpService = new BOEReportsHttpService();
		private readonly ICommonDataMapper commonDataMapper;
		private readonly IUserDTODataLoader userLoader;
		private readonly IPermissionsDTODataLoader permissionsLoader;
		#region Cache Setup

		/// <summary>
		/// Cache Object
		/// </summary>
		private MemoryCache cache;
		private bool disposedValue;

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
        /// <param name="travelTripCostCalculator">Travel Trip Cost Calculator</param>
        public ReportsControllerLogic(
            IBOEExporter boeExporter,
            BOESummary boeSummary,
            IBOECustomExporter boeCustomExporter,
            IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader,
            BOEDiscrepancyReport boeDiscrepancyReport,
            IProposalLoader proposalLoader,
            IWorkspaceControllerLogic workspaceControllerLogic,
            TravelTripCostCalculation travelTripCostCalculator,
			ICommonDataMapper inCommonDataMapper,
			IPermissionsDTODataLoader inPermissionsLoader,
			IUserDTODataLoader inUserLoader)
        {
            this.boeExporter = boeExporter;
            this.boeSummary = boeSummary;
            this.boeCustomExporter = boeCustomExporter;
            this.workspaceExportFormatDTOLoader = workspaceExportFormatDTOLoader;
            this.boeDiscrepancyReport = boeDiscrepancyReport;
            this.proposalLoader = proposalLoader;
            this.workspaceControllerLogic = workspaceControllerLogic;
            this.travelTripCostCalculator = travelTripCostCalculator;
			this.commonDataMapper = inCommonDataMapper;
			this.userLoader = inUserLoader;
			this.permissionsLoader = inPermissionsLoader;
			
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
        /// <param name="isCustomExport">Flag indicating whether the export is a custom export</param>
        /// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
        /// <param name="exportInputs">the export inputs</param>
        /// <param name="boeExportModelViews">the boe export model views</param>
        /// <param name="boeSummaryGridModelViews">the boe summary grid model views</param>
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
            workspace.LoadBoesAndTaskElementsRTEData();
            workspace.LoadTravelRTEData();
            workspace.LoadODCsRTEData();
            workspace.LoadMaterialsRTEData();

            // All BOEs for the workspace as a default
            List<FullBoe> boes = workspace.Boes.ToList();
            List<BoeTaskElementDTO> tasks = workspace.TaskElements.ToList();
            boeSummaryGridModelViews = new List<BOESummaryGridModelView>();

			// Validate for UCOT and multiple MOQ tasks - Space only
			if (Utilities.ShowUCOTForWorkspace(workspace.CreationDate, workspace.TrackingNumber))
			{
				IList<MultiMOQTypeResult> multiMoqResults = new List<MultiMOQTypeResult>();
				IList<string> allTasks = new List<string>();

				foreach (FullBoe boe in workspace.Boes)
				{
					multiMoqResults.Add(MultiMOQTypeUtility.DoTasksHaveMultipleMOQTypes(boe, workspace.CreationDate, workspace.Shortname));
				}

				if (multiMoqResults.Any(x => x.DoMultiMOQTypesExist))
				{
					foreach (MultiMOQTypeResult result in multiMoqResults)
					{
						allTasks.AddRange(result.Tasks);
					}

					string commaSeparatedTasks = string.Join(", ", allTasks);
					string ucotExceptionString = string.Format(ValidationConstants.MULTI_TASK_WITH_MULTI_MOQ_TYPES_UCOT, commaSeparatedTasks);
					throw new GenValidationException(ucotExceptionString);
				}
			}

			#region Override assigned output format template

			// Get template based on workspace preferences
			wsExportFormatDTO = workspaceExportFormatDTOLoader.GetById(workspace.TemplateID);

            #endregion

            isCustomExport = custom || (wsExportFormatDTO?.ExportFormat?.TemplateType == ExcelReportTemplateType.MASTER);
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

            // Get RTE overridescu
            ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateOverrides = workspace.TemplateQuestionsAndAnswers.ToList();

            if (isOffloading)
            {
                OffloadLaborRates offloader = new OffloadLaborRates();
                List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
                OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), workspace);

                boes = results.Boes.ToList();
                tasks = boes.SelectMany(b => b.TaskElements).ToList();
                rteTemplateOverrides = boes.SelectMany(x => x.TemplateQuestionsAndAnswers).ToList();
            }

            if (workspace.IsProjectMapWorkspace)
            {
                boes = ProjectMapSorter.OrderBoes(boes, workspace).ToList();
            }

            exportInputs = new BOEExportInputs(boes, workspace.Boes.ToList(), tasks, workspace, rteTemplateOverrides, workspace.MoqTypeSelections.ToList(), true, true);
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
		/// <param name="isCustomExport">Flag indicating whether the export is a custom export</param>
		/// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model views</param>
		/// <param name="segmentedOutput">Should the output be broken into segments and zipped</param>
		public async Task ExportAllBOEsReport(FullWorkspace workspace, ICollection<BoeCustomReportComponent> selectedComponents, HttpResponseBase httpResponse, bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, List<BOESummaryGridModelView> boeSummaryGridModelViews, bool segmentedOutput = false)
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
				wsExportFormatDTO = wsExportFormatDTO.ExportFormat.ParentTemplateId < 9001 || wsExportFormatDTO.ExportFormat.ParentTemplateId > 10000 || wsExportFormatDTO.ExportFormat.ParentTemplateId == null ? this.workspaceExportFormatDTOLoader.GetById((int)ExcelReportTemplateType.MASTER) : wsExportFormatDTO;
			}

			if (Utilities.IsReportGenerationExternal)
			{
				await this.boeReportsHttpService.ExportBOEsToWord(selectedComponents, httpResponse, isCustomExport, wsExportFormatDTO, exportInputs, boeExportModelViews,
					boeSummaryGridModelViews, segmentedOutput);
			}
			else if (isCustomExport)
			{
				if (segmentedOutput)
				{
					this.boeCustomExporter.ExportBOEsToZipFile(
						exportInputs,
						boeExportModelViews,
						boeSummaryGridModelViews,
						workspace,
						selectedComponents,
						httpResponse,
						string.Format("genBOEExport-{0}.zip", workspace.WorkspaceName).Replace(",", string.Empty),
						wsExportFormatDTO);
				}
				else
				{
					// Call the export function in the business layer and get back the file name of the populated template.
					this.boeCustomExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, selectedComponents, httpResponse, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty), wsExportFormatDTO);
				}
			}
			else
			{
				// Call the export function in the business layer and get back the file name of the populated template.
				if (segmentedOutput)
				{
					this.boeExporter.ExportBOEsToZipFile(
						exportInputs,
						boeExportModelViews,
						boeSummaryGridModelViews,
						workspace,
						httpResponse,
						string.Format("genBOEExport-{0}.zip", workspace.WorkspaceName).Replace(",", string.Empty),
						wsExportFormatDTO.PhysicalFilePathCache,
						wsExportFormatDTO.ExportFormat.TemplateType
					);
				}
				else
				{
					this.boeExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workspace, httpResponse, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName).Replace(",", string.Empty), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType);
				}
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
        /// Generates Data for the Boe Discrepancy Report. Used by both the page and also Excel export..
        /// 
        /// This is cached for 5 minutes in case the user wants to export the data, so that way we don't have to keep rerunning all the data..
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="clearCache">Should be cleared when the web report is being displayed, to get a clean view; 
        ///                             For the export we should be using cache (since you can ONLY export once the web report has been generated).</param>
		/// <param name="processResources">Should Resources be processed for missing Resources/BRCs? 
		///		This should typically only be true when running export to ProPricer or All BOEs Report as additional validation</param>
        /// <returns>Data for the Boe Discrepancy Report</returns>
        public ICollection<BoeDiscrepancyReportModelView> GenerateDataForBoeDiscrepancyReport(FullWorkspace ws, bool clearCache, bool processResources = false)
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
                theModelViews = this.boeDiscrepancyReport.GetReport(ws, processResources);

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
                            toReturn.Add("Revised Anticipated Delivery Date");
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

        /// <summary>
        /// Get the BOE Export Inputs used for the BOE Status and BOE/WBS Reports
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <returns>BOE Export Inputs</returns>
        public BOEExportInputs GetExportInputsForStatusAndWbsReports(FullWorkspace ws)
        {
            if(ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

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

            return new BOEExportInputs(boes, boes, tasks, ws);
        }

        /// <summary>
        /// Generates a ModelView for the WBS/BOE Report
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// WBS/BOE Report Modelview
        /// </returns>
        public ICollection<BoeWbsReportModelView> GenerateWbsBoeReport(BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            ICollection<BoeWbsReportModelView> toReturn = new Collection<BoeWbsReportModelView>();
			
            // Get all BOEs in the Workspace
            IReadOnlyCollection<BoeDTO> allBOEsInWorkspace = exportInputs.Boes;
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(exportInputs.TravelTrips);
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(exportInputs.PerDiemsForTravelTrips);
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(exportInputs.EscalationRates);
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(exportInputs.MiscTravelRatesForTravelTrips);

            // Create a report model view for each BOE
            foreach (BoeDTO boe in allBOEsInWorkspace)
            {
                BoeWbsReportModelView modelView = new BoeWbsReportModelView();

                // Set WBS Data
                WbsDTO wbs = exportInputs.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                modelView.WBSNumber = wbs == null ? string.Empty : wbs.WbsNumber;

                // Set BOE Data
                modelView.BOETitle = boe.Title ?? string.Empty;

                ICollection<BoeTaskElementDTO> tasks = exportInputs.TaskElements.Where(x => x.BoeID == boe.Id).ToCollection();
                ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToCollection();
                ICollection<OtherDirectCostDTO> odcs = exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToCollection();

                modelView.TotalHours = (from taskElement in tasks
                                        from laborType in taskElement.taskElementLabors
                                        where laborType.SpreadType == SpreadType.Hours
                                              && laborType.ValueSpread.HasValue
                                        select laborType.ValueSpread.Value).Sum();

				modelView.TotalUCOTHours = UCOTUtility.GetTaskElementsUCOTHours(exportInputs.FullWorkspace, (IReadOnlyCollection<BoeTaskElementDTO>)tasks);
				modelView.TotalHoursWithUCOT = modelView.TotalHours + modelView.TotalUCOTHours;


                decimal taskCost = 0;

                // calculate labor cost
                foreach (ResourceTypeDto boeResource in tasks.SelectMany(x => x.taskElementLabors))
                {
                    taskCost += boeResource.SpreadType == SpreadType.Cost ? Convert.ToDecimal(boeResource.ValueSpread) : 0m;
                }

                decimal odcCost = ((decimal)(odcs.Sum(odc => odc.ODCTypes.Sum(odcTypes => odcTypes.ODCSpreads.Sum(odcSpreads => odcSpreads.CostSpreadValue))))) / 100;
                // Sum the trip data for all travel elements.
                decimal travelCost = 0;
                foreach (TravelDTO travel in travelElements)
                {
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);
                        travelCost += this.travelTripCostCalculator.CalculateTravelCost(tripType, exportInputs.FullWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal;
                    }
                }

                modelView.TotalCost = taskCost + odcCost + travelCost;

                // Add the model view to the list to sort and return
                toReturn.Add(modelView);
            }

            // Sort the modelViews by WBS and BOE Title
            toReturn = toReturn.OrderBy(x => x.WBSNumber).ThenBy(x => x.BOETitle).ToCollection();

            // Get totals
            toReturn.Add(new BoeWbsReportModelView() {
                BOETitle = CommonConstants.SET_AS_BOLD_FOR_EXCEL + "Totals",
                TotalHours = toReturn.Sum(x => x.TotalHours),
				TotalUCOTHours = toReturn.Sum(x => x.TotalUCOTHours),
				TotalHoursWithUCOT = toReturn.Sum(x => x.TotalHoursWithUCOT),
                TotalCost = toReturn.Sum(x => x.TotalCost)
            });

            return toReturn;
        }

        /// <summary>
        /// Export the WBS/BOE Report for the given workspace
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="fileLocation">Location of the template file</param>
        /// <param name="reportModelView">Report model view</param>
        /// <param name="exportInputs">export inputs</param>
        /// <returns>Report filename</returns>
        public string ExportWbsBoeReport(FullWorkspace ws, string fileLocation, ICollection<BoeWbsReportModelView> reportModelView, BOEExportInputs exportInputs)
        {
            if(ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            string toReturn = string.Empty;

            if(reportModelView.Any())
            {
                // Create rows for the export file
                ExcelExportWorksheet worksheet = this.GetWbsBoeReportWorksheet(reportModelView, exportInputs);

                // pass to the generic excel exporter
                toReturn = ExcelExporter.ExportToExcelFile(fileLocation, true, new List<ExcelExportWorksheet> { worksheet }, new int?[] { 1 }, ws.IsUsingEquivalentPerson);
            }

            return toReturn;
        }

        /// <summary>
        /// Get the worksheet for the WBS/BOE report, populated with data from the modelview
        /// </summary>
        /// <param name="reportModelView">modelview for the report</param>
        /// <param name="exportInputs">export inputs</param>
        /// <returns>Worksheet for the report</returns>
        private ExcelExportWorksheet GetWbsBoeReportWorksheet(ICollection<BoeWbsReportModelView> reportModelView, BOEExportInputs exportInputs)
        {
            ExcelExportWorksheet toReturn = new ExcelExportWorksheet();

            string hoursFormatString = Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision);
            string hoursLabel = "Total " + FullObjectHelper.HoursLabel(exportInputs.Workspace);
			string hoursLabelUCOT = "Total UCOT " + FullObjectHelper.HoursLabel(exportInputs.Workspace);
			string grandTotalHoursLabel = "Grand Total " + FullObjectHelper.HoursLabel(exportInputs.Workspace);
			bool isUCOTEnabledForWorkspace = Utilities.ShowUCOTForWorkspace(exportInputs.FullWorkspace.CreationDate, exportInputs.FullWorkspace.Shortname);
            
			if (isUCOTEnabledForWorkspace)
			{
				// Add headers
				toReturn.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER,
					hoursLabel, hoursLabelUCOT, grandTotalHoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER);

				toReturn.AddRange(
					from mv in reportModelView
					select new Collection<string>
					{
						mv.WBSNumber,
						mv.BOETitle,
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + mv.TotalHours.ToString(hoursFormatString),
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + mv.TotalUCOTHours.ToString(hoursFormatString),
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + mv.TotalHoursWithUCOT.ToString(hoursFormatString),
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, mv.TotalCost)
					});
			}
			else
			{
				// Add headers
				toReturn.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER,
					hoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER);

				toReturn.AddRange(
					from mv in reportModelView
					select new Collection<string>
					{
						mv.WBSNumber,
						mv.BOETitle,
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + mv.TotalHours.ToString(hoursFormatString),
						CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, mv.TotalCost)
					});
			}

			return toReturn;
        }

		/// <summary>
		/// Get export data for report page
		/// </summary>
		/// <param name="ws">FullWorkspace</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public ExportReportViewModel GetDisplayExports(FullWorkspace ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			ExportReportViewModel reportView = new ExportReportViewModel();
			reportView.IsUsingSummarizeByCustomFieldTemplate = false;
			reportView.SummarizeByCustomFieldOptions = null;

			bool hasTravel = false;
			bool hideINLMenuItem = ws.CreationDate > Utilities.ShowINLCutoffDate;

			if (ws.Travels.Any())
			{
				hasTravel = true;
			}

			// Get all available reports
			Collection<ReportDTO> reportsAvailable = commonDataMapper.getReports();
			Collection<GeneralReportViewModel> theModelViews = new Collection<GeneralReportViewModel>();
			bool isSubcontractorUser = IsSubcontractorUser(ws);
			// Filter exports
			if (hideINLMenuItem)
			{
				// Hardcoded reportId 17 for PBOE/IBOE reports as that was really the only way to single it out here
				reportsAvailable = reportsAvailable.Where(x => x.ReportName != "PBOE / IBOE Forms").ToCollection();
			}
			if (reportsAvailable != null)
			{
				foreach (ReportDTO report in reportsAvailable)
				{
					if ((isSubcontractorUser && report.ReportID == (int)Reports.INLFormsExport) ||
						(!ConfigurationUtilities.GetAppSetting<bool>("IsBOEFormVisible") && report.ReportID == (int)Reports.INLFormsExport))
					{
						continue;
					}

					if (ws.IsProjectMapWorkspace && report.ReportID != (int)Reports.AllBOEs && report.ReportID != (int)Reports.WorkbenchOffload)
					{
						// In a Project Map Workspace and Workbench Offload are shown
						// If this is a Project Map WS, but the report is not All BOEs or Workbench Offload,
						// skip the rest of the logic so the report is not shown
						continue;
					}

					if (SystemConfiguration.Instance().CompanyMode != IES.Common.CompanyConfiguration.MST && report.ReportID == (int)Reports.WorkbenchOffload)
					{
						// Workbench is only shown for RMS Workspaces
						// If the report is one of these but this is not a RMS WS,
						// skip the rest of the logic so the report is not shown
						continue;
					}

					if (report.ReportType == ReportType.Export)
					{
						GeneralReportViewModel theModelView = new GeneralReportViewModel(report);

						if (theModelView.ReportID == (int)Reports.WorkbenchOffload)
						{
							// we need to get the ssrs report uri as well for the front-end
							theModelView = new SSRSReportsModelView(report, GetReportUrl(Reports.WorkbenchOffload));
						}

						if (theModelView.ReportID == (int)Reports.AllBOEs)
						{
							WorkspaceExportFormatDTO exportFormat = ws.WorkspaceExportFormats.FirstOrDefault(x => x.Id == ws.TemplateID);
							theModelView.Description = String.Format(theModelView.Description, exportFormat.ExportFormatName);

							// Do we need to support the special Labor Hours Summary by Custom Field template?
							if (IsUsingSummarizeByCustomFieldTemplate(exportFormat.ExportFormatName))
							{
								reportView.IsUsingSummarizeByCustomFieldTemplate = true;
								reportView.SummarizeByCustomFieldOptions = SummarizeByCustomFieldOptions(ws.CustomFields);
							}
						}

						if (theModelView.ReportID == (int)Reports.AllBOEsSegmented)
						{
							WorkspaceExportFormatDTO exportFormat = ws.WorkspaceExportFormats.FirstOrDefault(x => x.Id == ws.TemplateID);
							theModelView.Description = String.Format(theModelView.Description, exportFormat.ExportFormatName);

							// Do we need to support the special Labor Hours Summary by Custom Field template?
							if (IsUsingSummarizeByCustomFieldTemplate(exportFormat.ExportFormatName))
							{
								reportView.IsUsingSummarizeByCustomFieldTemplate = true;
								reportView.SummarizeByCustomFieldOptions = SummarizeByCustomFieldOptions(ws.CustomFields);
							}
						}

						if (!((report.ReportID == (int)Reports.TravelExtendedCost || report.ReportID == (int)Reports.TravelUnitCost) && !hasTravel))
						{
							theModelViews.Add(theModelView);
						}
					}
				}
			}

			reportView.SupportCustomExport = SupportCustomExport;
			reportView.IsProjectMapWs = ws.IsProjectMapWorkspace;

			ICollection<string> outOfSyncMessages = GetPtmDataOutOfSyncMessages(ws);
			reportView.IsPtmDataOutOfSync = outOfSyncMessages.Any();
			reportView.OutOfSyncMessages = outOfSyncMessages;

			ICollection<PermissionsDTO> adminDtos = this.permissionsLoader.GetWorkspacePermissions(ws.Id)
				.Where(x => x.Role == Role.WorkspaceAdmin).ToCollection();
			ICollection<UserDTO> users = this.userLoader.GetByIds(adminDtos.Select(a => a.ETIUserId).ToList());
			reportView.WorkspaceAdmins = users.Select(u => u.DisplayName).ToCollection();

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
			reportView.Reports = theModelViews;

			return reportView;

		}

		/// <summary>
		/// Get general report data for report page
		/// </summary>
		/// <param name="ws">FullWorkspace</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public Collection<GeneralReportViewModel> GetDisplayGeneralReports(FullWorkspace ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// Get all available reports
			Collection<ReportDTO> reportsAvailable = commonDataMapper.getReports();
			Collection<GeneralReportViewModel> theModelViews = new Collection<GeneralReportViewModel>();

			if (reportsAvailable != null)
			{
				// get the reports we are interested in and order the way we want
				ReportDTO boeStatusReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BOEStatus);
				theModelViews.Add(new GeneralReportViewModel(boeStatusReport));

				ReportDTO boeActivityReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BOEActivity);
				theModelViews.Add(new GeneralReportViewModel(boeActivityReport));

				ReportDTO workspaceActivityReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.WorkspaceActivity);
				theModelViews.Add(new GeneralReportViewModel(workspaceActivityReport));

				ReportDTO discrepancyReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.BoeDiscrepancy);
				theModelViews.Add(new GeneralReportViewModel(discrepancyReport));

				ReportDTO validateAllReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.ValidateAllBOE);
				theModelViews.Add(new GeneralReportViewModel(validateAllReport));

				if (Utilities.IsConfidenceReportEnabled)
				{
					ReportDTO confidenceReport = reportsAvailable.Single(x => x.ReportType == ReportType.View && x.ReportID == (int)Reports.ConfidenceReport);
					theModelViews.Add(new GeneralReportViewModel(confidenceReport));
				}

				if (FullObjectHelper.ShowEquivalentPersonsOption && ws.IsUsingEquivalentPerson)
				{
					foreach (GeneralReportViewModel model in theModelViews)
					{
						model.Description = model.Description.Replace("Hours", "EPs").Replace("hour", "EP");
					}
				}
			}
			return theModelViews;
		}

		/// <summary>
		/// Determines whether current user has a subcontractor role in the current workspace
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns>True if user has a subcontractor role in the current workspace</returns>
		private bool IsSubcontractorUser(FullWorkspace workspace)
		{
			Collection<PermissionsDTO> potentialPermissions = this.permissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);
			UserDTO currentUser = workspace.CurrentActiveUser;
			bool isSubcontractorUser = potentialPermissions.Any(p => p.Role == Role.SubcontractorAuthor && p.ETIUserId == currentUser.UserID);
			return isSubcontractorUser;
		}

		/// <summary>
		/// Dispose managed resources
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (this.boeReportsHttpService != null)
					{
						this.boeReportsHttpService.Dispose();
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~ReportsControllerLogic()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}