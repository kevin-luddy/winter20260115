// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.Entity.Core;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Transactions;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.CustomFields;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.ActionLogic.NewValidation;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.Workspace;
	using GenBOE.ActionLogic.Workspace.Creation;
	using GenBOE.ActionLogic.WorkspaceTransitions;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using GenTRAC.DataBridge.DTO;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Compression;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;
	using IES.Common.PickList;
	using UserDTO = Dtos.UserDTO;

	public class WorkspaceController : GenBOEController
	{
		private Logger _log = new Logger(typeof(WorkspaceController));
		private WorkspaceStateMachine _WorkspaceStateMachine = null;
		private ActiveDirectoryUtilities _ADUtils = null;
		private ResourceDTODataLoader _ResourceLoader = null;
		private IResourceListDTODataLoader _ResourceListLoader = null;
		private PerformingOrgListDTODataLoader _PerformingOrgListLoader = null;
		private RestoreDefaultOptions _RestoreDefaultOptions = null;
		private CustomFieldDTODataLoader _CustomFieldLoader = null;
		private CustomFieldValueDTODataLoader _CustomFieldValueLoader = null;
		private IWorkspaceVariableDTODataLoader _WorkspaceVariableLoader = null;
		private BoeEmailer _emailer = null;
		private WorkspaceVariableUniqueNameValidator _WSVarUniqueValidator = null;
		private IVariableSelectBOEtoSumCalculation _VariableSelectBOEtoSumCalculation = null;
		private BOEStateMachine _BOEStateMachine = null;
		private WorkspaceExportFormatDTODataLoader _WorkspaceExportFormatDTOLoader = null;
		private WorkspaceVersionMetaDataDTODataLoader _WorkspaceVersionMetaDataDTODataLoader = null;
		private VariableCircularReferenceChecker _VariableCircularReferenceChecker = null;
		private BoeTaskElementRecalculation _BoeTaskElementRecalculation = null;
		private ArtemisImporter _ArtemisImporter = null;
		private ProjectImporter _ProjectImporter = null;
		private CustomFieldImporter _CustomFieldImporter = null;
		private ResourcesImporter _ResourcesImporter = null;
		private PerformingOrgImporter _PerformingOrgImporter = null;
		private BoeTaskElementMediator _BoeTaskElementMediator = null;
		private BoeMediator _BoeMediator = null;
		private ISecurityInformation _securityInformation = null;
		private WorkspaceSearchDTODataLoader _WorkspaceSearchDTOLoader = null;
		private WorkspaceCopier _WorkspaceCopier = null;
		private TMResourceRateDTODataLoader tmResourceRateLoader = null;
		private IWorkspaceControllerLogic _ControllerLogic = null;
		private TMResourceRateImporter tmResourceRateImporter = null;
		private InUseDataLoader _InUseDataLoader = null;
		private IClinDTODataLoader clinLoader;
		private IWorkspaceDTODataLoader workspaceLoader;
		private IWbsDTODataLoader wbsLoader;
		private IPerformingOrgDTODataLoader perfOrgLoader;
		private PackageUtilities _PackageUtilities;
		private IBoeApproverResponseDTODataLoader _IBoeApproverResponseDTODataLoader;
		private RMSZoneTravelRatesFeesDataLoader travelRatesFeesDataLoader;
		private IOffloadRatesDTOLoader offloadRatesDTOLoader;
		private IRetriever retriever;
		private GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader;
		private GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper;
		private BoePickListMapper boePickListMapper;
		private CommentsAndResponsesExporter _commentsAndResponsesExporter = null;
		private BOECommentsControllerLogic _boeCommentsControllerLogic = null;
		private readonly GenBOE.DataBridge.DTO.IPldDTODataLoader _pldDTODataLoader;
		private readonly DataBridge.DTO.LineOfBusinessDataLoader _lineOfBusinessDataLoader;


		/// <summary>
		/// Workspace Exporter
		/// </summary>
		private WorkspaceExporter workspaceExporter;

		/// <summary>
		/// Reports Controller Logic
		/// </summary>
		private IReportsControllerLogic reportsControllerLogic;

		/// <summary>
		/// BOE Exporter
		/// </summary>
		private IBOEExporter boeExporter;

		/// <summary>
		/// BOE Custom Exporter
		/// </summary>
		private IBOECustomExporter boeCustomExporter;

		/// <summary>
		/// Full WS Recalculation
		/// </summary>
		private IFullWorkspaceRecalculation FullWsRecalc { get; set; }

		/// <summary>
		/// Angular rewrite Workspace settings controller Logic
		/// </summary>
		private WorkspaceSettingsControllerLogic _workspaceSettingsControllerLogic { get; set; }

		private const int SYSTEM_PERF_ORG_LIST_ID = 1;

		private const string WORKSPACE_COPY_ERROR = "The workspace has been successfully copied, but may contain errors due to missing data in the original workspace.  Please run the \"Validate All BOEs\" report to find any errors.";

		private const int WORKSPACE_NAME_TRUNCATE_INDEX = 87;
		private const int WORKSPACE_SHORT_NAME_TRUNCATE_INDEX = 14;

		private const string PROJECT_MAP_EXPORT_TEMPLATE = "~/Templates/Export/ProjectMap.xlsx";

		/// <summary>
		/// Constructor
		/// </summary>
		public WorkspaceController(ISecurityAccess inSecurityAccess,
			ICommonDataMapper inCommonDataMapper,
			SiteMasterUtilities inSiteMasterUtilities,
			UserDTODataLoader inUserDTODataLoader,
			WorkspaceStateMachine inWorkspaceStateMachine,
			IPermissionsDTODataLoader inPermissionsDTOLoader,
			ActiveDirectoryUtilities inADUtils,
			ResourceDTODataLoader inResourceDataLoader,
			IResourceListDTODataLoader inResourceListDataLoader,
			PerformingOrgListDTODataLoader inPerformingOrgListDataLoader,
			RestoreDefaultOptions inRestoreDefaultOptions,
			CustomFieldDTODataLoader inCustomFieldLoader,
			CustomFieldValueDTODataLoader inCustomFieldValueLoader,
			IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
			BoeEmailer inEmailer,
			WorkspaceVariableUniqueNameValidator inWSVarUniqueValidator,
			IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
			BOEStateMachine inBOEStateMachine,
			WorkspaceExportFormatDTODataLoader inWorkspaceExportFormatDTOLoader,
			VariableCircularReferenceChecker inVariableCircularReferenceChecker,
			BoeTaskElementRecalculation inBoeTaskElementRecalculation,
			WorkspaceVersionMetaDataDTODataLoader inWorkspaceVersionMetaDataDTODataLoader,
			ArtemisImporter inArtemisImporter,
			ProjectImporter inProjectImporter,
			CustomFieldImporter inCustomFieldImporter,
			ResourcesImporter inResourcesImporter,
			PerformingOrgImporter inPerformingOrgImporter,
			BoeTaskElementMediator inBoeTaskElementMediator,
			BoeMediator inBoeMediator,
			ISecurityInformation inSecurityInformation,
			WorkspaceSearchDTODataLoader inWorkspaceSearchLoader,
			WorkspaceCopier inWorkspaceCopier,
			TMResourceRateDTODataLoader tmResourceRateLoader,
			IWorkspaceControllerLogic inWorkspaceControllerLogic,
			TMResourceRateImporter tmResourceRateImporter,
			SystemMetrics inSystemMetrics,
			InUseDataLoader InUseDataLoader,
			IFullObjectFactory factory,
			IClinDTODataLoader clinLoader,
			IWorkspaceDTODataLoader workspaceLoader,
			IWbsDTODataLoader wbsLoader,
			IPerformingOrgDTODataLoader perfOrgLoader,
			IGenBOEControllerLogic inControllerLogic,
			IFullWorkspaceRecalculation fullWsRecalc,
			PackageUtilities inPackageUtilities,
			IBoeApproverResponseDTODataLoader inBoeApproverResponseDTODataLoader,
			RMSZoneTravelRatesFeesDataLoader travelRatesFeesDataLoader,
			IOffloadRatesDTOLoader offloadRatesDTOLoader,
			IRetriever retriever,
			GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader,
			GenBOE.DataBridge.DTO.IPldDTODataLoader pldDTODataLoader,
			DataBridge.DTO.LineOfBusinessDataLoader lineOfBusinessDataLoader,
			GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper,
			BoePickListMapper boePickListMapper,
			WorkspaceExporter workspaceExporter,
			IReportsControllerLogic reportsControllerLogic,
			IBOEExporter boeExporter,
			IBOECustomExporter boeCustomExporter,
			CommentsAndResponsesExporter commentsAndResponsesExporter,
			BOECommentsControllerLogic boeCommentsControllerLogic,
			WorkspaceSettingsControllerLogic workspaceSettingsControllerLogic)
			: base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, inPermissionsDTOLoader, inControllerLogic)
		{
			_WorkspaceStateMachine = inWorkspaceStateMachine;
			_ADUtils = inADUtils;
			_ResourceLoader = inResourceDataLoader;
			_ResourceListLoader = inResourceListDataLoader;
			_PerformingOrgListLoader = inPerformingOrgListDataLoader;
			_RestoreDefaultOptions = inRestoreDefaultOptions;
			_CustomFieldLoader = inCustomFieldLoader;
			_CustomFieldValueLoader = inCustomFieldValueLoader;
			_WorkspaceVariableLoader = inWorkspaceVariableLoader;
			_emailer = inEmailer;
			_WSVarUniqueValidator = inWSVarUniqueValidator;
			_VariableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
			_BOEStateMachine = inBOEStateMachine;
			_WorkspaceExportFormatDTOLoader = inWorkspaceExportFormatDTOLoader;
			_VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
			_BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
			_WorkspaceVersionMetaDataDTODataLoader = inWorkspaceVersionMetaDataDTODataLoader;
			_ArtemisImporter = inArtemisImporter;
			_ProjectImporter = inProjectImporter;
			_CustomFieldImporter = inCustomFieldImporter;
			_ResourcesImporter = inResourcesImporter;
			_PerformingOrgImporter = inPerformingOrgImporter;
			_BoeTaskElementMediator = inBoeTaskElementMediator;
			_BoeMediator = inBoeMediator;
			_securityInformation = inSecurityInformation;
			_WorkspaceCopier = inWorkspaceCopier;
			_WorkspaceSearchDTOLoader = inWorkspaceSearchLoader;
			this.tmResourceRateLoader = tmResourceRateLoader;
			_ControllerLogic = inWorkspaceControllerLogic;
			this.tmResourceRateImporter = tmResourceRateImporter;
			_InUseDataLoader = InUseDataLoader;
			this.clinLoader = clinLoader;
			this.workspaceLoader = workspaceLoader;
			this.wbsLoader = wbsLoader;
			this.perfOrgLoader = perfOrgLoader;
			this.FullWsRecalc = fullWsRecalc;
			_PackageUtilities = inPackageUtilities;
			_IBoeApproverResponseDTODataLoader = inBoeApproverResponseDTODataLoader;
			this.travelRatesFeesDataLoader = travelRatesFeesDataLoader;
			this.offloadRatesDTOLoader = offloadRatesDTOLoader;
			this.retriever = retriever;
			this.proposalLoader = proposalLoader;
			_pldDTODataLoader = pldDTODataLoader;
			this.ptmSecurityMapper = ptmSecurityMapper;
			this.boePickListMapper = boePickListMapper;
			this.workspaceExporter = workspaceExporter;
			this.reportsControllerLogic = reportsControllerLogic;
			this.boeExporter = boeExporter;
			this.boeCustomExporter = boeCustomExporter;
			_commentsAndResponsesExporter = commentsAndResponsesExporter;
			_boeCommentsControllerLogic = boeCommentsControllerLogic;
			_workspaceSettingsControllerLogic = workspaceSettingsControllerLogic;
			_lineOfBusinessDataLoader = lineOfBusinessDataLoader;
		}

		#region Public Methods

		#region Display

		#region Views

		/// <summary>
		/// Creates a model view for use in WorkspaceResourceRateTMModelView
		/// </summary>
		/// <returns>WorkspaceResourceRateTMModelView</returns>
		protected virtual WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView()
		{
			return _ControllerLogic.CreateWorkspaceResourceRateTMModelView();
		}

		/// <summary>
		/// Displays workspace Settings View.
		/// </summary>
		/// <param name="workspace">workspace short name.</param>
		/// <returns>View.</returns>
		public ViewResult WorkspaceSettings(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Init
			Stopwatch sw = InitializeAction(_log, "WorkspaceSettings", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Perform Action
			ViewResult toReturn = GetMasterView(WebConstants.VIEW_WORKSPACE_SETTINGS, workspace);

			// Action Finalize
			FinalizeAction(_log, "WorkspaceSettings", sw);

			return toReturn;
		}

		/// <summary>
		/// This function will make the call to display Workspace Home.
		/// </summary>
		/// <param name="workspace">workspace short name.</param>
		/// <returns>basic View.</returns>
		public ViewResult Index(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "Index", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			if (ws.IsProjectMapWorkspace)
			{
				ViewBag.IsBucketized = ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap;
				ViewBag.Resources = ws.ResourcesForWsResourceListId.Where(rl => rl.ElementOfCost == ElementOfCostType.LMLabor || rl.ElementOfCost == ElementOfCostType.IWTA || rl.ElementOfCost == ElementOfCostType.Travel || rl.ElementOfCost == ElementOfCostType.ODC).OrderBy(x => x.ResourceName).Select(r => new OptionModelView()
				{
					Id = r.Id,
					Label = r.ResourceName
				});
				ViewBag.PerformingOrgs = ws.PerformingOrgsForWsList.OrderBy(po => po.PerformingOrgName).Select(p => new OptionModelView()
				{
					Id = p.Id,
					Label = p.PerformingOrgName
				});
				ViewBag.LegacyResources = _CommonDataMapper.GetSikorskyLegacyResources().Select(r => new OptionModelView()
				{
					Id = r.LegacyID,
					Label = r.LegacyResourceID
				});
				ViewBag.AllowGridEdit = ws.AllowGridEdit;
			}
			// Perform Action
			ViewResult toReturn;
			if (ws.IsProjectMapWorkspace && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
			{
				toReturn = GetMasterView(WebConstants.VIEW_PROJ_MAP_WORKSPACE_HOME, workspace);
			}
			else
			{
				toReturn = GetMasterView(WebConstants.VIEW_WORKSPACE_HOME, workspace);
			}

			// Set decimal precision for summary values.
			ViewBag.HoursPrecision = ws.DecimalPrecision;
			ViewBag.DollarsPrecision = ws.CostDecimalPrecision;

			// Action Finalize
			FinalizeAction(_log, "Index", sw);

			return toReturn;
		}

		/// <summary>
		/// RESTful endpoint to retrieve project map grid data for workspace.
		/// </summary>
		/// <param name="workspace">The workspace short name.</param>
		/// <param name="page">The page number of data to return.  If not set, then return all data</param>
		/// <returns>project map data in json format.</returns>
		public ActionResult GetProjectMapData(string workspace, int? page)
		{
			FullProjectMapWorkspace ws = this.Factory.CreateFullProjectMapWorkspace(workspace);
			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "GetProjectMapData", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);
			JsonResult jsonResult = null;

			if (page.HasValue)
			{
				ProjectMapPageModelView projectMapPagedData = this.retriever.GetProjectMapPagedData(ws.Id, page.Value);
				ProjectMapOffloadConverter.AddOffloadWarnings(ws, projectMapPagedData.Data);
				jsonResult = Json(projectMapPagedData, JsonRequestBehavior.AllowGet);
			}
			else
			{
				IReadOnlyCollection<ProjectMapModelView> projectMapData = ws.ProjectMapData;
				ProjectMapOffloadConverter.AddOffloadWarnings(ws, projectMapData);
				jsonResult = Json(projectMapData, JsonRequestBehavior.AllowGet);
			}

			// Action Finalize
			FinalizeAction(_log, "GetProjectMapData", sw);
			jsonResult.MaxJsonLength = int.MaxValue;
			return jsonResult;
		}

		/// <summary>
		/// Exports the Project Map Data.
		/// Note:  This is RMS-specific, will need to be reworked to support Space.
		/// </summary>
		/// <param name="workspace">Workspace Short Name</param>
		/// <param name="offload">An indication whether we should try to offload first</param>
		/// <returns>Download Result for the Project Map Data.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The UI might hang indefinitely, never returning control to the user, unless all exceptions are handled.")]
		public ActionResult ExportProjectMapData(string workspace, bool offload)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "ExportProjectMapData", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			ActionResult result = new EmptyResult();
			try
			{
				ws.LoadBoesAndTaskElementsRTEData();
				// Get the modelviews
				IReadOnlyCollection<ProjectMapModelView> modelViews;
				if (offload && (ws.IsProjectMapWorkspace || ws.ProjectMapType == ProjectMapType.StandardWithOffload))
				{
					// run offload first
					modelViews = ProjectMapOffloadConverter.ConvertToProjectMap(ws, offload, false);
				}
				else if (ws.IsProjectMapWorkspace)
				{
					FullProjectMapWorkspace mapWS = ws as FullProjectMapWorkspace;
					modelViews = mapWS.ProjectMapData;
				}
				else
				{
					modelViews = ProjectMapOffloadConverter.ConvertToProjectMap(ws, false, false);
				}

				string templateName = Server.MapPath(PROJECT_MAP_EXPORT_TEMPLATE);
				// Call the export function in the business layer and get back the file name of the populated template.
				string exportedFileName = ProjectMapExporter.ExportToExcelFile(templateName, modelViews, ws, offload);

				// Generate a custom ActionResult to cause a file download to the client
				string fileName = string.Format("ProjectMap_{0}.xlsx", ws.WorkspaceName);
				FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				result = File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileName),
					fileDownloadName: fileName);
			}
			catch (GenValidationException ex)
			{
				result = this.CreateTextFileWithErrorMessage(ex.Message);
			}
			catch (Exception e)
			{
				_log.Error(e);
				string supportLink = Utilities.ServiceCentralLink();

				result = this.CreateTextFileWithErrorMessage(string.Format("An error has occurred. This might be the result of invalid data such as missing Offload Rates. If the data is valid, and the error persists, please contact the GenBOE Helpdesk at {0}.", supportLink));
			}

			// Finalize Action
			FinalizeAction(_log, "ExportProjectMapData", sw);
			return result;
		}

		/// <summary>
		/// Exports all workspace author, reviewer and approver comments and responses into an excel download
		/// </summary>
		/// <param name="workspace">Workspace Short Name</param>
		/// <returns>Download Result for the excel sheet.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The UI might hang indefinitely, never returning control to the user, unless all exceptions are handled.")]
		public ActionResult ExportWorkspaceCommentsAndResponses(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "ExportWorkspaceCommentsAndResponses", SecurityPage.Reports, SecurityAuthorization.Read, ws, null);

			ActionResult result = new EmptyResult();
			try
			{
				IDictionary<int, ICollection<BOEComment>> commentDTOs = _boeCommentsControllerLogic.GetAllCommentsInWorkspace(ws.Boes);

				string templateName = Server.MapPath("~/Templates/Export/CommentsAndResponses.xlsx");
				// Call the export function in the business layer and get back the file name of the populated template.
				string exportedFileName = _commentsAndResponsesExporter.ExportToExcelFile(templateName, commentDTOs);

				// Generate an custom ActionResult to cause a file download to the client
				result = new ExportFileDownloadResult(exportedFileName, string.Format("WorkspaceCommentsAndResponses_{0}.xlsx", ws.WorkspaceName));
			}
			catch (Exception e)
			{
				_log.Error(e);
				string supportLink = Utilities.ServiceCentralLink();

				result = this.CreateTextFileWithErrorMessage(string.Format("An error has occurred. If the data is valid, and the error persists, please contact the GenBOE Helpdesk at {0}.", supportLink));
			}

			// Finalize Action
			FinalizeAction(_log, "ExportWorkspaceCommentsAndResponses", sw);
			return result;
		}


		/// <summary>
		/// Imports the project map data.
		/// Note:  This is RMS-specific, will need to be reworked to support Space.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ContentResult ImportProjectMapData(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportProjectMapData", SecurityPage.WorkspaceHome, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				try
				{
					Collection<ValidationMessage> warnings = ProjectMapImporter.ValidateLegacyResources(Request.Files[0].InputStream, ws.ProjectMapType);

					// Call the business layer to parse the uploaded file
					// If the file was successfully parsed, add the results to the genBOE database
					ICollection<ProjectMapModelView> projectMapData = ProjectMapImporter.ImportFromExcelFile(Request.Files[0].InputStream, ws.ProjectMapType);

					_log.Debug("Saving Import of Project Map data");

					// Save the new list, extra validation is done inside the Save
					warnings = this.SaveProjectMapData(projectMapData, ws, warnings);

					string returnString = ConvertValidationsToString(warnings, ProjectMapValidationOffset.ExcelImport);

					// Return a success message
					toReturn = GenerateUploadResponse(true, returnString);

				}
				// Catch custom exceptions from ExcelImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
				}
				catch (ColumnMissingException ex2)
				{
					toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. The following columns are missing: {0}.", ex2.Message);
				}
				catch (CellValueMissingException ex3)
				{
					toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for the required columns. Each row must have every column filled out. Check the following column: {0}.", ex3.Message);
				}
				catch (DuplicateValuesException ex4)
				{
					toReturn = GenerateUploadResponse(false, "Ids must be unique. The following Ids are not unique: {0}", ex4.Message);
				}
				catch (EntityCommandExecutionException ex)
				{
					_log.Error(ex);
					_log.Error(ex.InnerException);
					toReturn = GenerateUploadResponse(false, "The Project Map data was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
				}
				catch (ValidationException ex5)
				{
					toReturn = GenerateUploadResponse(false, ex5.Message);
				}
				catch (GenValidationException ex6)
				{
					string errorString = ex6.Message;
					string warningString = string.Empty;

					if (ex6.ValidationList.Any())
					{
						warningString = ConvertValidationsToString(ex6.ValidationList.Where(v => v.TreatAsWarning).ToList(), ProjectMapValidationOffset.ExcelImport);
						errorString = ConvertValidationsToString(ex6.ValidationList.Where(v => !v.TreatAsWarning).ToList(), ProjectMapValidationOffset.ExcelImport);
					}
					toReturn = GenerateUploadWithWarningResponse(false, errorString, warningString);
				}
				catch (DateImportException ex7)
				{
					toReturn = GenerateUploadResponse(false, ex7.Message);
					throw;
				}
				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Project Map Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = GenerateUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			FinalizeAction(_log, "ImportProjectMapData", sw);
			return toReturn;
		}

		/// <summary>
		/// Converts the message validations to a string.
		/// </summary>
		/// <param name="validationList">The validation list.</param>
		/// <param name="rowOffset">For Excel add 2 (header row counts) for the grid add 1 (header row does not count)</param>
		/// <returns>A joined string of all validation messages.</returns>
		private static string ConvertValidationsToString(IList<ValidationMessage> validationList, ProjectMapValidationOffset rowOffset)
		{
			string message = string.Empty;
			if (validationList != null && validationList.Any())
			{
				Dictionary<string, List<int>> validationsByRows = new Dictionary<string, List<int>>();
				foreach (ValidationMessage vm in validationList)
				{
					string vmMessage = vm.ValidationIssue;
					if (!string.IsNullOrEmpty(vm.FieldName))
					{
						vmMessage += ": " + vm.FieldName;
					}

					if (!validationsByRows.ContainsKey(vmMessage))
					{
						validationsByRows.Add(vmMessage, new List<int>());
					}

					if (vm.RowIndex.HasValue)
					{
						// rowOffset should be +2 for Excel import because the data starts in row 2, which translates to index 0
						// and +1 for the grid because data starts in row 1
						validationsByRows[vmMessage].Add(vm.RowIndex.Value + (int)rowOffset);
					}
				}

				StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, List<int>> kvp in validationsByRows)
				{
					sb.Append(kvp.Key);

					if (kvp.Value.Any())
					{
						if (rowOffset == ProjectMapValidationOffset.GridSave)
						{
							sb.Append(". Grid Row(s): ");
						}
						else if (rowOffset == ProjectMapValidationOffset.ExcelImport)
						{
							sb.Append(". Excel Row(s): ");
						}
						sb.Append(string.Join(", ", kvp.Value.Select(r => r.ToString())));
					}
					sb.AppendLine();
				}
				message = sb.ToString();
			}
			return message;
		}

		/// <summary>
		/// RESTful endpoint to save project map grid data for workspace.
		/// </summary>
		/// <param name="projectMapData"></param>
		/// <param name="ws"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult SaveProjectMapData(string workspace, ICollection<ProjectMapModelView> projectMapData)
		{
			string status;
			string message = string.Empty;
			try
			{
				if (projectMapData == null)
				{
					projectMapData = new List<ProjectMapModelView>();
				}

				Collection<ValidationMessage> warnings = this.SaveProjectMapData(projectMapData, this.Factory.CreateFullWorkspace(workspace));

				if (warnings.Any())
				{
					status = "warning";
					message = ConvertValidationsToString(warnings, ProjectMapValidationOffset.GridSave);
				}
				else
				{
					status = "success";
				}
			}
			catch (GenValidationException ex)
			{
				status = "error";
				message = ex.Message;
				if (ex.ValidationList.Any())
				{
					message = ConvertValidationsToString(ex.ValidationList, ProjectMapValidationOffset.GridSave);
				}
			}
			return Json(new { status = status, message = message });
		}

		/// <summary>
		/// Saves the project map data.
		/// </summary>
		/// <param name="projectMapData">The project map data.</param>
		/// <returns>Collection of validation warnings.</returns>
		private Collection<ValidationMessage> SaveProjectMapData(ICollection<ProjectMapModelView> projectMapData, FullWorkspace ws, Collection<ValidationMessage> additionalErrors = null)
		{
			ProjectMapValidator validator = new ProjectMapValidator(projectMapData.ToArray(), ws);
			validator.Validate();

			Collection<ValidationMessage> result = additionalErrors ?? new Collection<ValidationMessage>();
			result.AddRange(validator.ValidationMessages);

			if (result.Any(vm => !vm.TreatAsWarning))
			{
				throw new GenValidationException(result);
			}

			this._ControllerLogic.SaveProjectMapData(projectMapData, ws);

			// Clear the cache after the project map has been saved since it is a kill/fill
			this.Factory.ClearWorkspaceCache(ws.Shortname);

			return result;
		}

		/// <summary>
		/// Displays Import IMS page
		/// </summary>
		/// <returns></returns>
		public ViewResult DisplayIMSImportPage(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "DisplayIMSImportPage", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ICollection<int> IDs = ws.WbsElements.Select(x => x.Id).ToList();
			ICollection<int> boeIDs = ws.Boes.Select(x => x.Id).ToList();

			bool DoesWorkspaceHaveAnyAssets = (IDs.Count() + boeIDs.Count() != 0);

			ViewData["WBSAsset_Check"] = DoesWorkspaceHaveAnyAssets;

			ViewResult toReturn = GetMasterView(WebConstants.VIEW_FOR_IMS_IMPORT_PAGE, workspace);

			// Action Finalize
			FinalizeAction(_log, "DisplayIMSImportPage", sw);

			return toReturn;
		}

		/// <summary>
		/// Returns the CreateWorkspace View in the Home Folder
		/// </summary>
		/// <returns></returns>
		public ViewResult CreateWorkspace()
		{
			Stopwatch sw = InitializeAction(_log, "CreateWorkspace", SecurityPage.CreateWorkspacePermissions, SecurityAuthorization.CreateReadUpdateDelete, null, null);

			CreateWorkspacePageModelView model = new CreateWorkspacePageModelView();

			Array projectMapTypeValues = Enum.GetValues(typeof(ProjectMapType));
			Collection<SelectListItem> projectMapTypes = new Collection<SelectListItem>();
			foreach (ProjectMapType projectMapTypeValue in projectMapTypeValues)
			{
				projectMapTypes.Add(new SelectListItem
				{
					Text = projectMapTypeValue.ToDescription(),
					Value = ((int)projectMapTypeValue).ToString()
				});
			}
			model.ProjectMapTypes = projectMapTypes.Where(x => !x.Text.Equals(Constants.PROJECT_TYPE_MAP_STANDARD_WITH_OFFLOAD)).ToList();
			model.LineOfBusinessTypes = this.boePickListMapper.GetPickListValues(PickListEnum.LineOfBusiness).PickLists;
			model.ProposalClassTypes = this.boePickListMapper.GetPickListValues(PickListEnum.ProposalClass).PickLists;
			model.ContractTypes = this.boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists;

			model.ApplicationUrl = new System.Uri(ConfigurationUtilities.GetAppSetting("ServerURL", Request.Url.Host) + "/");

			model.CostVolumeLeadPricerNTID = this._securityInformation.ActiveUserNTID;
			model.CostVolumeLeadPricerDisplayName = this._securityInformation.ActiveUserData.DisplayName;

			Collection<SelectListItem> trackingNumbers = new Collection<SelectListItem>();
			if (Utilities.IsPTMIntegrated)
			{
				// retrieve valid tracking numbers for the current user
				IReadOnlyCollection<GenTRAC.DataBridge.Common.Security.SecurityPermissionsResponse> roles = this.ptmSecurityMapper.GetRolesForLoggedInUser();
				bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);
				
				ICollection<ProposalDto> proposals = (isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(this._securityInformation.ActiveUserNTID, true))
																	.Where(p => !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.NoBid && p.ProposalStatus != ProposalStatus.Revised).ToList();

				foreach (ProposalDto proposal in proposals)
				{
					trackingNumbers.Add(new SelectListItem
					{
						Text = proposal.TrackingNumber + " - " + proposal.ProposalTitle,
						Value = proposal.TrackingNumber
					});
				}
			}

			model.TrackingNumbers = trackingNumbers;
			
			IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(this._securityInformation.ActiveUserNTID);
			model.IsAdmin = permissions.Any(p => p.AuthorizedRole == Role.SystemAdmin);
			
			model.PtmTrackingNumberNotRequired = string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("CanCreateWorkspaceWithoutPtmTrackingNumber")) ?
				false :
				_securityInformation.IsMemberOfADGroupInAppSettingsList(this._securityInformation.ActiveUserNTID, "CanCreateWorkspaceWithoutPtmTrackingNumber");

			model.IsSAPConnectionEnabled = Utilities.IsSAPEnabledForSystem;
			model.IsAssignTaskAuthorEnabled = Utilities.IsAssignTaskAuthorEnabledForSystem;

			ViewResult toReturn = View(WebConstants.VIEW_HOME_CREATE_WORKSPACE, model);

			FinalizeAction(_log, "CreateWorkspace", sw);
			return toReturn;
		}

		/// <summary>
		///  Returns Top 50 PLD proposals for the RMS search for pa numbers and pa titles
		/// </summary>
		/// <param name="term"></param>
		/// <returns></returns>
		public JsonResult SearchPLDProposals(string term)
		{
			Stopwatch sw = InitializeAction(_log, "GetProposalDetails", SecurityPage.CreateWorkspacePermissions, SecurityAuthorization.Read, null, null);

			ICollection<ProposalDTO> matches = _pldDTODataLoader.GetTopProposals(term);

			var results = matches.Select(p => new
			{
				Text = p.PA_Number + " - " + p.PA_Title,
				Value = p.PA_Number
			});

			FinalizeAction(_log, "GetProposalDetails", sw);

			return Json(results, JsonRequestBehavior.AllowGet);
				
		}


		/// <summary>
		///  Get Proposal Detail by paNumber 
		/// </summary>
		/// <param name="paNumber"></param>
		/// <returns></returns>
		public JsonResult GetProposalDetails(string paNumber)
		{
						
			Stopwatch sw = InitializeAction(_log, "GetProposalDetails", SecurityPage.CreateWorkspacePermissions, SecurityAuthorization.Read, null, null);

			ProposalDTO result = _pldDTODataLoader.GetProposalDetails(paNumber);

			if (result != null)
			{
				ICollection<PickListDto> pickList = _lineOfBusinessDataLoader.GetPickListValues();

				string resolvedLob = LineOfBusinessHelper.Resolve(result.Line_of_Business, pickList);
				result.Line_of_Business = resolvedLob;

				PickListDto match = pickList.FirstOrDefault(p => string.Equals(p.Text.Trim(), resolvedLob.Trim(), StringComparison.OrdinalIgnoreCase));

				if (match != null)
				{
					result.Line_of_Business_ID = match.Id;
				}


			}

			FinalizeAction(_log, "GetProposalDetails", sw);

			return Json(result, JsonRequestBehavior.AllowGet);
		}
			

		/// <summary>
		/// Get Short Name Workspace from Tracking Number  PLD 
		/// </summary>
		/// <param name="paNumber"></param>
		/// <returns></returns>
		public JsonResult GetNextWorkspaceShortNameFromTrackingNumber(string paNumber)
		{
			Stopwatch sw = InitializeAction(_log, "GetProposalDetails", SecurityPage.CreateWorkspacePermissions, SecurityAuthorization.Read, null, null);
			
			ICollection<WorkspaceDTO> workspaces  = this.workspaceLoader.GetAllWsNamesForTrackingNumber(paNumber)
				.ToList() ?? new List<WorkspaceDTO>();

			Dictionary<string, object> payload = _ControllerLogic.NextTrackingNumber(workspaces, paNumber);

			FinalizeAction(_log, "GetProposalDetails", sw);

			return Json(payload, JsonRequestBehavior.AllowGet);
			
		}




		#endregion Views

		#region Partial Views
		/// <summary>
		/// Displays the email preferences.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns>The view for the email preferences</returns>
		public virtual ActionResult DisplayEmailPreferences(string workspace)
		{
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_EMAIL_PREFERENCES, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, Factory.CreateFullWorkspace(workspace), null);

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_EMAIL_PREFERENCES);
			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_EMAIL_PREFERENCES, sw);

			return toReturn;
		}

		/// <summary>
		/// Determine if any of the update dialogs should be displayed and display them
		/// Dialogs include Zone Travel Rates, Offload Rates, and UCOT Factor
		/// </summary>
		/// <param name="useCookie">If a cookie should be used</param>
		/// <param name="workspace">Workspace to display dialogs for</param>
		/// <returns>View Result</returns>
		public ViewResult DisplayUpdateWorkspaceRatesDialog(bool useCookie, string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG, SecurityPage.UpdateLockedResourceRates, SecurityAuthorization.Read, ws, null);

			int currentUserID = ws.CurrentActiveUser.UserID;

			Collection<PermissionsDTO> userPermissions = (from p in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
														  where p.Role == Role.WorkspaceAdmin && p.ETIUserId == currentUserID
														  select p).ToCollection();
			UpdateWorkspaceResourceRateModelView theModelView = new UpdateWorkspaceResourceRateModelView();
			theModelView.ShowZoneTravelRatesDialog = false;

			#region Decide if Zone Travel Rates Dialog should be displayed
			{
				if (!ws.IsProjectMapWorkspace)
				{
					bool userHasCookie = false;
					HttpCookie cookie = null;

					if (useCookie)
					{
						// check users cookie to see if we should even bother checking for rates update dialog
						cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_TRAVEL_ZONE_RATE] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_TRAVEL_ZONE_RATE);

						if (!string.IsNullOrEmpty(cookie.Values[workspace]))
						{
							userHasCookie = true;
						}
					}

					// if the user has the cookie it means they said 'no' that they didn't want to apply updates
					if (userHasCookie == false && userPermissions.Count > 0)
					{
						//Get the actual date!!! => if it's null, it means it's fine. If it's not null, it means that there are newer dates available
						theModelView.LastUpdatedTimeZoneTravel = this._ControllerLogic.GetLastupdatedTimeZoneTravel(ws.Id);

						if (theModelView.LastUpdatedTimeZoneTravel != null)
						{
							// we will show the dialog to the user asking them if they want to update rates
							theModelView.ShowZoneTravelRatesDialog = true;

							if (useCookie)
							{
								// add the cookie to the users browser that will keep them from being prompted over and over again if they choose 'no' to the prompt to update rates
								cookie.Values[workspace] = WebConstants.UPDATE_WORKSPACE_TRAVEL_ZONE_RATE;
								cookie.Expires = DateTime.Now.AddMinutes(5);
								System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
							}
						}
						else
						{
							theModelView.ShowZoneTravelRatesDialog = false;
						}
					}
				}
			}

			#endregion

			#region Decide if Offload Rates Dialog should be displayed

			if (ws.ProjectMapType != ProjectMapType.StandardWithoutOffload)
			{
				bool userHasCookie = false;
				HttpCookie cookie = null;

				if (useCookie)
				{
					// check users cookie to see if we should even bother checking for rates update dialog
					cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_OFFLOAD_RATES] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_OFFLOAD_RATES);

					if (!string.IsNullOrEmpty(cookie.Values[workspace]))
					{
						userHasCookie = true;
					}
				}

				// if the user has the cookie it means they said 'no' that they didn't want to apply updates
				if (!userHasCookie && userPermissions.Any())
				{
					//Get the actual date!!! => if it's null, it means it's fine. If it's not null, it means that there are newer dates available
					theModelView.LastUpdatedTimeOffloadRates = this._ControllerLogic.GetLastupdatedTimeOffload(ws.Id);

					if (theModelView.LastUpdatedTimeOffloadRates != null)
					{
						// we will show the dialog to the user asking them if they want to update rates
						theModelView.ShowOffloadRatesDialog = true;

						if (useCookie)
						{
							// add the cookie to the users browser that will keep them from being prompted over and over again if they choose 'no' to the prompt to update rates
							cookie.Values[workspace] = WebConstants.UPDATE_WORKSPACE_OFFLOAD_RATES;
							cookie.Expires = DateTime.Now.AddMinutes(5);
							System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
						}
					}
					else
					{
						theModelView.ShowOffloadRatesDialog = false;
					}
				}
			}

			#endregion

			#region Decide if UCOT Factor Dialog should be displayed

			// Space ONLY
			if (Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.Shortname) && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				bool userHasCookie = false;
				HttpCookie cookie = null;

				if (useCookie)
				{
					// check users cookie to see if we should even bother checking for rates update dialog
					cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_UCOT_FACTOR] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_UCOT_FACTOR);

					if (!string.IsNullOrEmpty(cookie.Values[workspace]))
					{
						userHasCookie = true;
					}
				}

				// if the user has the cookie it means they said 'no' that they didn't want to apply updates
				if (!userHasCookie && userPermissions.Any())
				{
					// Get the System level value
					decimal ucot = this._ControllerLogic.GetUcotSystemSettingsValue();

					if (ucot != ws.UCOTFactor)
					{
						// we will show the dialog to the user asking them if they want to update rates
						theModelView.ShowUCOTFactorDialog = true;

						if (useCookie)
						{
							// add the cookie to the users browser that will keep them from being prompted over and over again if they choose 'no' to the prompt to update UCOT Factor
							cookie.Values[workspace] = WebConstants.UPDATE_WORKSPACE_UCOT_FACTOR;
							cookie.Expires = DateTime.Now.AddMinutes(5);
							System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
						}
					}
					else
					{
						theModelView.ShowUCOTFactorDialog = false;
					}
				}
			}

			#endregion

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_UPDATE_RESOURCE_RATES, theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG, sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the Workspace Settings Jump Page
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceSettingsJump(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_SETTINGS_JUMP, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ViewBag.IsProjectMapWs = ws.IsProjectMapWorkspace;

			ViewData["DisplayLaborCostTM"] = ws.IsUsingTM;
			this.DecideIfNonProjectMapLinksShouldBeVisible(ws);
			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_SETTINGS_JUMP, this._ControllerLogic.CreateWorkspaceResourceRateTMModelView());

			// Action Finalize
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_SETTINGS_JUMP, sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the Workspace Identification Partial View
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <returns>partial view</returns>
		public ViewResult DisplayWorkspaceIdentification(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceIdentification", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Perform Action
			IWorkspaceIdentificationModelView workspaceModelView = this._ControllerLogic.GetWorkspaceIdentificationModelView(ws);

			// We load the server url here so that we have access to the Request object
			workspaceModelView.ApplicationURL = new System.Uri(ConfigurationUtilities.GetAppSetting("ServerURL", Request.Url.Host) + "/" + workspaceModelView.ShortName);

			ViewData["LineOfBusinessTypes"] = this.boePickListMapper.GetPickListValues(PickListEnum.LineOfBusiness).PickLists;
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);
			ViewData["EnableSAP"] = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			ViewData["ShowSAP"] = Utilities.ShowSAPForWorkspace(ws.CreationDate);

			ICollection<WorkspaceDTO> workspaceChecks = this.workspaceLoader.GetAllWsNamesForTrackingNumber(ws.TrackingNumber)
				.Where(x => x.CurrentPTMWorkspace).ToList();

			ViewData["DoesPTMMultipleWorkspaces"] = "false";
			if (workspaceChecks.Count() == 1 && workspaceChecks.Any(x => x.Id != ws.Id))
			{
				ViewData["DoesPTMMultipleWorkspaces"] = "true";
			}
			else if (workspaceChecks.Count() > 1)
			{
				ViewData["DoesPTMMultipleWorkspaces"] = "true";
			}

			// gather up proposal class types
			this.GetProposalClassOptionList();

			// gather up contract types
			ViewData["ContractTypes"] = this.boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists.Where(p => p.IsActive || ws.SelectedContractTypes.Contains(p.Id)).ToList();

			// gather up tracking numbers
			this.GetTrackingNumberOptionList(workspaceModelView);

			// gather up contract types
			ViewData["SelectedContractTypes"] = string.Join(",", ws.SelectedContractTypes.Select(i => i));

			ViewResult toReturn = this.View(_ControllerLogic.WorkspaceIdentificationViewName, workspaceModelView);

			// Action Finalize
			FinalizeAction(_log, "DisplayWorkspaceIdentification", sw);

			return toReturn;
		}

		/// <summary>
		/// Gets the tracking number option list.
		/// </summary>
		/// <param name="workspaceModelView">The workspace model view.</param>
		private void GetTrackingNumberOptionList(IWorkspaceIdentificationModelView workspaceModelView)
		{
			Collection<SelectListItem> trackingNumbers = new Collection<SelectListItem>();
			if (Utilities.IsPTMIntegrated)
			{
				WorkspaceIdentificationSpaceModelView spaceModel = workspaceModelView as WorkspaceIdentificationSpaceModelView;

				if (spaceModel != null)
				{
					// retrieve valid tracking numbers for the current user
					IReadOnlyCollection<GenTRAC.DataBridge.Common.Security.SecurityPermissionsResponse> roles = this.ptmSecurityMapper.GetRolesForLoggedInUser();
					bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

					ICollection<ProposalDto> proposals = (isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(this._securityInformation.ActiveUserNTID, true))
																					.Where(p => !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.NoBid && p.ProposalStatus != ProposalStatus.Revised).ToList();

					if (!string.IsNullOrWhiteSpace(spaceModel.TrackingNumber) && !proposals.Any(p => p.TrackingNumber == spaceModel.TrackingNumber))
					{
						// Currently has a bad Tracking Number saved in DB, but we will let that slide
						trackingNumbers.Add(new SelectListItem
						{
							Text = spaceModel.TrackingNumber,
							Value = spaceModel.TrackingNumber
						});
					}

					foreach (ProposalDto proposal in proposals)
					{
						trackingNumbers.Add(new SelectListItem
						{
							Text = proposal.TrackingNumber + " - " + proposal.ProposalTitle,
							Value = proposal.TrackingNumber
						});
					}
				}
			}

			ViewData["TrackingNumbers"] = trackingNumbers;
		}

		private void GetProposalClassOptionList()
		{
			ICollection<PickListDto> proposalClassTypes = this.boePickListMapper.GetPickListValues(PickListEnum.ProposalClass).PickLists;
			proposalClassTypes = proposalClassTypes.Where(p => p.Id != Constants.PROPOSAL_CLASS_TYPE_NOT_SET).ToList();

			ViewData["ProposalClassTypes"] = proposalClassTypes;
		}

		private int[] GetSelectedContractTypeOptionList(FullWorkspace ws)
		{
			ICollection<PickListDto> dtos = this.boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists;
			int[] result = dtos.Where(d => ws.SelectedContractTypes.Contains(d.Id)).Select(s => s.Id).ToArray();

			return result;
		}

		/// <summary>
		/// Displays the Sum of BOE workspace variables partial view
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <returns>partial view</returns>
		public ViewResult DisplayWorkspaceSumofBOEVariables(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceSumofBOEVariables", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ViewData["WorkspaceID"] = ws.Id;

			// Perform Action
			Collection<WorkspaceVariableModelView> theModelViews = new Collection<WorkspaceVariableModelView>();

			Collection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(x => x.ValueType == VarValueType.SumOfBOEs).ToCollection<WorkspaceVariableDTO>();

			foreach (WorkspaceVariableDTO variable in workspaceVariables)
			{
				theModelViews.Add(new WorkspaceVariableModelView(variable, _VariableSelectBOEtoSumCalculation, ws));
			}

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_SUM_OF_BOE_VARIABLES, theModelViews);

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceSumofBOEVariables", sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the discrete workspace variables partial view
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <returns>partial view</returns>
		public ViewResult DisplayWorkspaceDiscreteVariables(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceDiscreteVariables", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ViewData["WorkspaceID"] = ws.Id;

			// Perform Action
			Collection<WorkspaceVariableModelView> theModelViews = new Collection<WorkspaceVariableModelView>();

			Collection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(x => x.ValueType == VarValueType.Discrete).ToCollection<WorkspaceVariableDTO>();

			foreach (WorkspaceVariableDTO variable in workspaceVariables)
			{
				theModelViews.Add(new WorkspaceVariableModelView(variable, _VariableSelectBOEtoSumCalculation, ws));
			}

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_DISCRETE_VARIABLES, theModelViews);

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceDiscreteVariables", sw);

			return toReturn;
		}

		/// <summary>
		/// Get the valid Sum Of BOEs selections for a given Workspace Variable
		/// </summary>
		/// <param name="workspace">The workspace containing the variable</param>
		/// <param name="workspaceVariableID">The variable to check</param>
		/// <returns></returns>
		public JsonResult FindValidBOEsForWorkspaceVariable(string workspace, int workspaceVariableID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			WorkspaceVariableDTO workspaceVariable = ws.WorkspaceVariables.FirstOrDefault(i => i.Id == workspaceVariableID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "FindValidBOEsForWorkspaceVariable", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			DataRelationshipVerifier.VerifyDataRelation(workspaceVariable, ws.Id);

			JsonResult toReturn = Json(_VariableCircularReferenceChecker.FindValidBOEsForWorkspaceVariable(new VariableCircularReferenceCheckerCache(), workspaceVariable, ws));

			// Finalize Action
			FinalizeAction(_log, "FindValidBOEsForWorkspaceVariable", sw);

			return toReturn;
		}

		/// <summary>
		/// Get the valid Sum of BOEs selection for new or not in use Workspace Variables
		/// </summary>
		/// <param name="workspace">the workspace containing the variable</param>
		/// <returns>valid BOE IDs</returns>
		public JsonResult FindValidBOEsForNotInUseWorkspaceVariable(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "FindValidBOEsForNotInUseWorkspaceVariable", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			JsonResult toReturn = Json(_VariableCircularReferenceChecker.FindValidBOEsForNotInUseWorkspaceVariable(ws));

			// Finalize Action
			FinalizeAction(_log, "FindValidBOEsForNotInUseWorkspaceVariable", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the workspace output format template
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <returns>partial view</returns>
		public ViewResult DisplayWorkspaceOutputFormat(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceOutputFormat", SecurityPage.WorkspaceSettingsOutputFormatTemplate, SecurityAuthorization.Read, ws, null);

			// Perform Action
			// gather up output format types
			List<SelectListItemWithTitle> outputFormatTypes = new List<SelectListItemWithTitle>();

			Collection<WorkspaceExportFormatDTO> exportFormats = ws.WorkspaceExportFormats.ToCollection();

			foreach (WorkspaceExportFormatDTO exportFormat in exportFormats.OrderBy(x => x.ExportFormat.TemplateId))
			{
				if (exportFormat.IsActive || ws.TemplateID == exportFormat.ExportFormat.TemplateId)
				{
					outputFormatTypes.Add(new SelectListItemWithTitle
					{
						Title = exportFormat.ExportFormatDescription,
						Text = exportFormat.ExportFormatName,
						Value = exportFormat.ExportFormat.TemplateId.ToString(),
						Selected = (ws.TemplateID == exportFormat.ExportFormat.TemplateId) ? true : false
					});
				}
			}

			ViewData["ExportFormatTypes"] = outputFormatTypes.OrderBy(x => x.Text).ToList();


			// gather up output sort order types
			Collection<SelectListItemWithTitle> SortOrderTypes = new Collection<SelectListItemWithTitle>();
			Collection<SortByModelView> exportSortOrder = _CommonDataMapper.getSortBy();
			foreach (SortByModelView sortOrder in exportSortOrder)
			{
				SortOrderTypes.Add(new SelectListItemWithTitle
				{
					Title = sortOrder.SortBy,
					Text = sortOrder.SortBy,
					Value = sortOrder.SortByID.ToString(),
					Selected = (ws.BOEExportSortByID == sortOrder.SortByID) ? true : false
				});
			}

			ViewData["SortOrderTypes"] = SortOrderTypes;

			WorkspaceOutputFormatModelView mv = new WorkspaceOutputFormatModelView(ws);
			UserDTO currentUser = ws.CurrentActiveUser;
			mv.ActiveUserDisplayName = currentUser.DisplayName;
			mv.ActiveUserPhoneNumber = currentUser.PhoneNumber;
			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_OUTPUT_FORMAT, mv);

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceOutputFormat", sw);

			return toReturn;
		}

		/// <summary>
		/// Get the output format template as a word document content type
		/// </summary>
		/// <param name="workspace">the name of the workspace we are in</param>
		/// <param name="id">the template id from the database</param>
		/// <returns>The word file, complete with correct content type</returns>
		public FileContentResult GetOutputFormatTemplate(string workspace, int? id)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "GetOutputFormatTemplate", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			FileContentResult toReturn = null;

			WorkspaceExportFormatDTO template = ws.WorkspaceExportFormats.FirstOrDefault(x => x.Id == id.Value);

			// Return the template as a download for the user
			using (MemoryStream mem = _PackageUtilities.UpdateDocumentVersion(template.FileData, template.PhysicalFilePathCache, template.ExportFormat))
			{
				toReturn = new FileContentResult(mem.ToArray(), ExportFileDownloadBase.ContentType_DOCX);
				toReturn.FileDownloadName = template.ExportFormatName + ".docx";
			}

			// Finalize Action
			FinalizeAction(_log, "GetOutputFormatTemplate", sw);

			return toReturn;
		}

		/// <summary>
		/// This function calls the Workspace status partial view with the workspace ID
		/// </summary>
		///<param name="workspace">the workspace to use in the system</param>
		/// <returns>partial view</returns>
		public ViewResult DisplayWorkspaceStatus(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceStatus", SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.Read, ws, null);

			// Perform Action
			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_STATUS, new WorkspaceStatusModelView(ws));

			ViewBag.IsProjectMapWs = ws.IsProjectMapWorkspace;

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceStatus", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the Workspace Status History
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ActionResult DisplayWorkspaceStatusHistoryGrid(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			//// Call the BL to generate the status report
			ICollection<WorkspaceStatusHistoryModelView> theModelViews = this._workspaceSettingsControllerLogic.GetWorkspaceStatusHistory(ws);

			ViewResult toReturn;

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				toReturn = View(WebConstants.VIEW_WORKSPACE_STATUS_HISTORY_GRID, theModelViews);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID, sw);
			return toReturn;
		}

		/// <summary>
		/// Display the BOE Custom Fields Grid page
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldsGrid(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldsGrid", SecurityPage.BoeCustomFields, SecurityAuthorization.Read, ws, null);

			ICollection<CustomFieldDTO> customFields = ws.CustomFields.ToCollection();

			ViewResult toReturn = null;
			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				ICollection<BOECustomFieldsInUseGridModelView> theModelViews = _ControllerLogic.GetCustomFieldsGridModelViews(customFields, ws.Id);

				ViewData["IsProjectMapWorkspace"] = ws.IsProjectMapWorkspace;
				ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

				toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELDS_GRID, theModelViews);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}


			// Finalize Action
			FinalizeAction(_log, "DisplayBOECustomFieldsGrid", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the BOE Custom Field page
		/// </summary>
		/// <param name="workspace">The Workspace</param>
		/// <param name="boeCustomFieldID">The Custom Field ID</param>
		/// <returns>View Result for the Custom Field page</returns>
		public ViewResult DisplayBOECustomField(string workspace, int? boeCustomFieldID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomField", SecurityPage.BoeCustomFields, SecurityAuthorization.Read, ws, null);

			BOECustomFieldsInUseGridModelView metaData;
			Collection<BOECustomFieldOptionModelView> options = new Collection<BOECustomFieldOptionModelView>();

			if (boeCustomFieldID != null)
			{
				// refresh in use flag
				_CustomFieldValueLoader.RefreshCustomFieldInUseByWorkspaceID(ws.Id);

				CustomFieldDTO metaFromDB = this.Factory.CreateCustomField(boeCustomFieldID.Value);
				metaData = new BOECustomFieldsInUseGridModelView(metaFromDB);
				ICollection<CustomFieldValueDTO> customFieldValues = _CustomFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldID(boeCustomFieldID.Value);
				metaData.inUse = customFieldValues.Any(x => x.CustomFieldValueInUseFlag);

				options = new Collection<BOECustomFieldOptionModelView>((from v in customFieldValues
																		 select new BOECustomFieldOptionModelView(v)).ToArray());

				if (ws.CustomFieldSorting == CustomFieldSorting.ID)
				{
					options = options.OrderBy(x => x.ID).ToCollection();
				}
				else
				{
					options = options.OrderBy(x => x.Description).ToCollection();
				}
			}
			else
			{
				metaData = new BOECustomFieldsInUseGridModelView();
			}

			//BOECustomField
			BOECustomFieldModelView theModelView = new BOECustomFieldModelView() { CustomFieldMetaData = metaData, CustomFieldOptions = options, UsingTemplateBoe = ws.UsingTemplateBOE };

			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOECustomField", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the BOE Custom Field Resource page
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldResource(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldResource", SecurityPage.BoeCustomFieldResource, SecurityAuthorization.Read, ws, null);

			ResourceListDTO systemList = _ResourceListLoader.GetResourceList(_ResourceLoader.GlobalListID);
			ViewData["SYSTEM_LIST_NAME"] = systemList.ResourceListName;
			ViewData["SYSTEM_LIST_ID"] = ws.ResourceListID;
			ViewData["RESOURCES_CHANGED"] = _RestoreDefaultOptions.ResourcesHaveChanged(ws);

			// gather up element of cost types
			ICollection<SelectListItem> elementOfCostTypes = _CommonDataMapper.GetElementOfCostTypes().Select(x => new SelectListItem
			{
				Text = x.ElementOfCostName,
				Value = x.ElementOfCostId.ToString()
			}).ToList();

			elementOfCostTypes = new Collection<SelectListItem>() { new SelectListItem { Text = "Select Element of Cost", Value = "0" } }.Union(elementOfCostTypes).ToList();
			ViewData["ElementOfCostTypes"] = elementOfCostTypes;

			// gather up spread/rate types
			ICollection<SelectListItem> rateTypes = _CommonDataMapper.GetRateTypes().Select(x => new SelectListItem
			{
				Text = x.RateTypeName,
				Value = x.RateTypeID.ToString()
			}).ToList();
			ViewData["RateTypes"] = new Collection<SelectListItem>() { new SelectListItem { Text = "Select Rate Type", Value = "0" } }.Union(rateTypes);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOECustomFieldResource", sw);

			return toReturn;
		}

		public ViewResult DisplayBOECustomFieldResourceGrid(string workspace, string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldResourceGrid", SecurityPage.BoeCustomFieldResource, SecurityAuthorization.Read, ws, null);

			BOECustomFieldResourceGridModelView theModelView = _GetAndFilterCustomFieldResources(workspace, searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE_GRID, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOECustomFieldResourceGrid", sw);

			return toReturn;
		}

		/// <summary>
		/// Returns a grid of all the default resources.
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldResourceViewDefault(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldResourceViewDefault", SecurityPage.BoeCustomFieldResource, SecurityAuthorization.Read, ws, null);

			Collection<BOECustomFieldResourceModelView> theModelViews = new Collection<BOECustomFieldResourceModelView>();
			IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = this._CommonDataMapper.GetElementOfCostTypesDictionary();
			ICollection<ResourceDTO> resources = _ResourceLoader.GetGlobalResources();

			foreach (ResourceDTO resource in resources)
			{
				string ElementOfCostDisplay = allElementOfCostTypes[(int)resource.ElementOfCost].ElementOfCostName;
				string rateTypeDisplay = resource.RateType.GetDescription();

				//note: in use is not shown on this popup so no extra calls are needed to get it
				theModelViews.Add(new BOECustomFieldResourceModelView(resource, rateTypeDisplay, ElementOfCostDisplay));
			}

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT, theModelViews);

			FinalizeAction(_log, "DisplayBOECustomFieldResourceViewDefault", sw);

			return toReturn;
		}

		public ViewResult DisplayBackupVersions(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBackupVersions", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BACKUP_VERSIONS);

			FinalizeAction(_log, "DisplayBackupVersions", sw);

			return toReturn;
		}

		/// <summary>
		/// Renders the grid
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBackupVersionsGrid(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBackupVersionsGrid", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);
			Collection<WorkspaceVersionModelView> model = new Collection<WorkspaceVersionModelView>();

			Collection<WorkspaceVersionMetaDataDTO> backupVersionDTOs = _WorkspaceVersionMetaDataDTODataLoader.GetByWorkspaceID(ws.Id);

			IDictionary<int, ICollection<BoeVersionDTO>> backupBoes = new Dictionary<int, ICollection<BoeVersionDTO>>();

			foreach (WorkspaceVersionMetaDataDTO bk in backupVersionDTOs)
			{
				ICollection<BoeVersionDTO> boes = _WorkspaceVersionMetaDataDTODataLoader.GetBoesByVersionID(bk.VersionID, bk.WorkspaceID);
				backupBoes.Add(bk.Id, boes);
				model.Add(new WorkspaceVersionModelView(bk, (UserDTODataLoader)this.UserLoader, boes));
			}

			ViewBag.IsSystemAdmin = CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.None;

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BACKUP_VERSIONS_GRID, model);

			FinalizeAction(_log, "DisplayBackupVersionsGrid", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the BOE Custom Field Performing Org page
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldPerfOrg(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldPerfOrg", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ViewData["PERFORMING_ORGS_CHANGED"] = ws.PerfOrgsChanged;
			PerformingOrgListDTO systemList = _PerformingOrgListLoader.GetPerfOrgList(CommonConstants.GLOBAL_PERFORMING_ORG_LIST_ID);
			ViewData["SYSTEM_LIST_NAME"] = systemList.PerformingOrgListName;
			ViewData["SYSTEM_LIST_ID"] = ws.PerfOrgListID;
			if (ws.WorkspaceState == WorkspaceState.Closed)
			{
				ViewData["READONLY"] = "true";
			}
			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG);

			FinalizeAction(_log, "DisplayBOECustomFieldPerfOrg", sw);

			return toReturn;
		}

		/// <summary>
		/// Renders the grid
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldPerfOrgGrid(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldPerfOrgGrid", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);
			if (ws.WorkspaceState == WorkspaceState.Closed)
			{
				ViewData["READONLY"] = "true";
			}

			BOECustomFieldPerformingOrgGridModelView theModelView = new BOECustomFieldPerformingOrgGridModelView();
			theModelView.PerformingOrgResults = new Collection<BOECustomFieldOptionModelView>();

			HashSet<int> perfOrgIdsInUse = _InUseDataLoader.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(ws.PerfOrgListID);
			foreach (PerformingOrgDTO performingOrg in ws.PerformingOrgsForWsList)
			{
				theModelView.PagedIndexes.Add(performingOrg.Id);
				if (theModelView.PerformingOrgResults.Count < theModelView.ResultsPerPage)
				{
					BOECustomFieldOptionModelView mv = new BOECustomFieldOptionModelView(performingOrg);
					if (perfOrgIdsInUse.Contains(performingOrg.Id))
					{
						mv.InUse = true;
					}
					else
					{
						mv.InUse = false;
					}

					theModelView.PerformingOrgResults.Add(mv);
				}
			}

			if (ws.PerfOrgSorting == CustomFieldSorting.ID)
			{
				theModelView.PerformingOrgResults = theModelView.PerformingOrgResults.OrderBy(x => x.ID).ToCollection();
			}
			else
			{
				theModelView.PerformingOrgResults = theModelView.PerformingOrgResults.OrderBy(x => x.Description).ToCollection();
			}

			theModelView.TotalResults = theModelView.PagedIndexes.Count;

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID, theModelView);

			FinalizeAction(_log, "DisplayBOECustomFieldPerfOrgGrid", sw);

			return toReturn;
		}

		/// <summary>
		/// Returns a grid of all the default performing orgs.
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayBOECustomFieldPerfOrgViewDefault(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOECustomFieldPerfOrgViewDefault", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);
			if (ws.WorkspaceState == WorkspaceState.Closed)
			{
				ViewData["READONLY"] = "true";
			}

			Collection<BOECustomFieldOptionModelView> theModelViews = new Collection<BOECustomFieldOptionModelView>();

			foreach (PerformingOrgDTO performingOrg in this.perfOrgLoader.GetGlobalPerformingOrgs())
			{
				theModelViews.Add(new BOECustomFieldOptionModelView(performingOrg));
			}

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT, theModelViews);

			FinalizeAction(_log, "DisplayBOECustomFieldPerfOrgViewDefault", sw);

			return toReturn;
		}

		public ViewResult DisplayWorkspaceAllowSearch(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceAllowSearch", SecurityPage.WorkspaceSettingsShareAndAllowSearch, SecurityAuthorization.Read, ws, null);

			// Perform Action
			if (ws.ContainsOCI == true)
			{
				ViewData["READONLY"] = "true";
			}

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_ALLOW_SEARCH_FORMAT, new WorkspaceAllowSearchModelView(ws));

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceAllowSearch", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the Getting Started with Workspace Help if the user is a workspace admin and hasn't chosen to hide it
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceHomeHelp(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceHomeHelp", SecurityPage.Help, SecurityAuthorization.Read, ws, null);

			// Check to see if WS admin
			UserDTO user = ws.CurrentActiveUser;
			Collection<PermissionsDTO> wsAdminPermissions = (from p in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
															 where (p.Role == Role.WorkspaceAdmin) && p.ETIUserId == user.UserID
															 select p).ToCollection();

			// Check to see if System Admin
			bool isSystemAdmin = CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.None;

			// convert to ModelView
			GettingStartedHelpModelView theModelView = new GettingStartedHelpModelView();
			this.DecideIfNonProjectMapLinksShouldBeVisible(ws);

			// Use show/hide from the db if user is ws admin
			if (wsAdminPermissions.Any())
			{
				theModelView.HideGettingStartedHelp = wsAdminPermissions[0].HideWorkspaceHelp;
				theModelView.UpdateDate = wsAdminPermissions[0].UpdateDate;
			}
			// Always show for system admins that are not the ws admin
			else if (isSystemAdmin)
			{
				theModelView.HideGettingStartedHelp = false;
				theModelView.UpdateDate = DateTime.MinValue;
			}
			// Hide for non admins 
			else
			{
				theModelView.HideGettingStartedHelp = true;
				theModelView.UpdateDate = DateTime.MinValue;
			}

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_HOME_HELP, theModelView);

			// Action Finalize
			FinalizeAction(_log, "DisplayWorkspaceHomeHelp", sw);

			return toReturn;
		}

		/// <summary>
		/// Display the Show Getting Started sub help menu to a workspace admin only;
		/// if the workspace admin clicks this menu, the "Getting Started with the Workspace" dialog will be displayed
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceShowGettingStartedHelp(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceShowGettingStartedHelp", SecurityPage.Help, SecurityAuthorization.Read, ws, null);

			// Perform Action
			UserDTO user = ws.CurrentActiveUser;
			Collection<PermissionsDTO> permissions = (from p in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
													  where p.Role == Role.WorkspaceAdmin && p.ETIUserId == user.UserID
													  select p).ToCollection();

			// convert to ModelView
			GettingStartedHelpModelView theModelView = new GettingStartedHelpModelView();

			// if the user has clicked this submenu, they want to reshow the Getting Started with the Workspace menu section so re-save their choice;
			// if they click this submenu, but have never hid the show help section, don't bother resaving
			if (permissions.Count() > 0)
			{
				if (permissions[0].HideWorkspaceHelp == true)
				{
					theModelView.HideGettingStartedHelp = false;
					theModelView.UpdateDate = permissions[0].UpdateDate;
					SaveHideGettingStartedHelpMenu(workspace, theModelView);
				}

			}

			// Action Finalize
			FinalizeAction(_log, "DisplayWorkspaceShowGettingStartedHelp", sw);

			// redirect the workspace admin to the home page
			return Index(workspace);
		}

		public ActionResult UpdateZoneTravelRates(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "UpdateZoneTravelRates", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			this._ControllerLogic.CopySystemZoneTravelRates(ws.Id);

			// user synced their rates, so we can remove the cookie now for this workspace
			HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_TRAVEL_ZONE_RATE] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_TRAVEL_ZONE_RATE);
			cookie.Expires = DateTime.Now.AddDays(-1D);
			System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

			// Finalize Action
			FinalizeAction(_log, "UpdateZoneTravelRates", sw);

			return Json(new { Status = true });
		}

		/// <summary>
		/// Update the offload rates for the workspace
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>action result</returns>
		public ActionResult UpdateOffloadRates(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "UpdateOffloadRates", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			this.offloadRatesDTOLoader.CopySystemDefaultOffloadRates(ws.Id);

			// user synced their rates, so we can remove the cookie now for this workspace
			HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_OFFLOAD_RATES] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_OFFLOAD_RATES);
			cookie.Expires = DateTime.Now.AddDays(-1D);
			System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

			// Finalize Action
			FinalizeAction(_log, "UpdateOffloadRates", sw);

			return Json(new { Status = true });
		}

		/// <summary>
		/// Update the UCOT Factor for the workspace
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>action result</returns>
		public ActionResult UpdateUCOTFactor(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_UPDATE_UCOT_FACTOR, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			this._ControllerLogic.CopySystemUCOTFactor(ws);

			this.Factory.ClearWorkspaceCache(workspace);

			// user synced their rates, so we can remove the cookie now for this workspace
			HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.UPDATE_WORKSPACE_UCOT_FACTOR] ?? new HttpCookie(WebConstants.UPDATE_WORKSPACE_UCOT_FACTOR);
			cookie.Expires = DateTime.Now.AddDays(-1D);
			System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_UPDATE_UCOT_FACTOR, sw);

			return Json(new { Status = true });
		}

		virtual public ContentResult ImportWorkspaceResourceRatesTM(ICollection<ImportedTMResourceRate> imported, string importTypeString, string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Perform Action
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportWorkspaceResourceRatesTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (imported != null)
			{
				TMResourceRatesImportType importType = importTypeString.GetEnumeratedValue<TMResourceRatesImportType>(TMResourceRatesImportType.importNew);

				ICollection<ImportedTMResourceRate> importUpdates = imported.Where(i => i.Updateable != UpdateType.None).Select(c => { c.WorkspaceID = ws.Id; return c; }).ToList();

				List<TMResourceRateDTO> toSave = new List<TMResourceRateDTO>(importUpdates);

				if (importType == TMResourceRatesImportType.replaceAll)
				{
					// delete any existing (unallocated) DB entries that are not included in the import file
					List<TMResourceRateDTO> toBeDeleted =
					(from X in this.tmResourceRateLoader.GetByWorkspaceId(ws.Id)  // existing DB entries
					 join I in imported on X.ResourceRateID equals I.ResourceRateID into G
					 from D in G.DefaultIfEmpty()
					 where D == null &&  // not in the import file
						   !_InUseDataLoader.GetInUse(InUseDataType.WorkspaceResources, X.ResourceID, ws.ResourceListID)  // unallocated
					 select X).Select(c => { c.Updateable = UpdateType.Deleted; c.WorkspaceID = ws.Id; return c; }).ToList();

					toSave.AddRange(toBeDeleted);
				}

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this.tmResourceRateLoader.SaveTMResourceRates(toSave);
					scope.Complete();
				}
			}

			// Perform Action
			ContentResult toReturn = null;
			FinalizeAction(_log, "ImportWorkspaceResourceRatesTM", sw);
			return toReturn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ActionResult ImportPreviewResultsForWorkspaceResourceRatesTM(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Perform Action
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportPreviewResultsForWorkspaceResourceRatesTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			List<String> errors = new List<string>();
			ICollection<ImportedTMResourceRate> results = new List<ImportedTMResourceRate>();

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				String importTypeString = Request.Form.GetValues(0)[0];
				TMResourceRatesImportType importType;

				if (Enum.IsDefined(typeof(TMResourceRatesImportType), importTypeString))
				{
					importType = (TMResourceRatesImportType)Enum.Parse(typeof(TMResourceRatesImportType), importTypeString, true);
				}
				else
				{
					throw new GeneralAppException("Import Type is not valid");
				}

				try
				{
					results = this.tmResourceRateImporter.ImportTMResourceRateFromExcelFile(Request.Files[0].InputStream, ws, importType, _InUseDataLoader);

					var importResults =
						(from x in results
						 where x.ImportTypes.Count == 1 &&
							(x.ImportTypes.FirstOrDefault() == ResourceRatesImportResult.Added ||
							 x.ImportTypes.FirstOrDefault() == ResourceRatesImportResult.Updated)
						 select new
						 {
							 EndDateLong = x.EndDateLong,
							 StartDateLong = x.StartDateLong,
							 ResourceRateID = x.ResourceRateID,
							 ResourceRate = x.ResourceRate,
							 ResourceID = x.ResourceID,
							 UpdateDateLong = x.UpdateDateLong,
							 Updateable = x.Updateable
						 }).ToList();

					var remainingImportResults =
						(from x in results
						 join I in importResults on x.ResourceRateID equals I.ResourceRateID into G
						 from R in G.DefaultIfEmpty()
						 where R == null  // remaining entries
						 select new
						 {
							 EndDateLong = x.EndDateLong,
							 StartDateLong = x.StartDateLong,
							 ResourceRateID = x.ResourceRateID,
							 ResourceRate = x.ResourceRate,
							 ResourceID = x.ResourceID,
							 UpdateDateLong = x.UpdateDateLong,
							 Updateable = UpdateType.None
						 });

					importResults.AddRange(remainingImportResults);

					JavaScriptSerializer jsS = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
					ViewData["SERIALIZED_DATA"] = jsS.Serialize(importResults);

					ViewData["IMPORT_TYPE"] = importTypeString;
				}
				// Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					errors.Add("File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
				}
				catch (ColumnMissingException ex2)
				{
					errors.Add("File does not contain all of the required columns. File must contain 'ID' and 'Description' columns. The following columns are missing: " + ex2.Message + ".");
				}
				catch (CellValueMissingException ex3)
				{
					errors.Add("A row in the file does not contain a value for ID, Description or both. Each row must have an ID and Description. Check the following column: " + ex3.Message + ".");
				}
				catch (Exception ex)
				{
					errors.Add("A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
					_log.Error(ex, "Unknown Import System Resource Rates Error.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				errors.Add("No file selected for upload");
			}

			ViewData["ERRORS"] = errors;

			if (errors.Any() && !results.Any())
			{
				ViewData["IMPORT_TYPE"] = string.Empty;

				List<ImportedTMResourceRate> dummyResults = new List<ImportedTMResourceRate>();

				JavaScriptSerializer jsS = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
				ViewData["SERIALIZED_DATA"] = jsS.Serialize(from x in dummyResults
															select new
															{
																EndDateLong = x.EndDateLong,
																StartDateLong = x.StartDateLong,
																ResourceRateID = x.ResourceRateID,
																ResourceRate = x.ResourceRate,
																ResourceID = x.ResourceID,
																UpdateDateLong = x.UpdateDateLong,
																Updateable = x.Updateable.ToString()
															});
			}

			// Finalize Action
			FinalizeAction(_log, "ImportPreviewResultsForWorkspaceResourceRatesTM", sw);
			return View(WebConstants.ACTION_DISPLAY_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES_TM, results);
		}

		/// <summary>
		/// Render the Workspace selection dropdown box and button
		/// </summary>
		/// <returns>The Choose Workspace Partial View</returns>
		public ViewResult DisplayChooseWorkspace(int? workspaceStatus)
		{
			Stopwatch sw = InitializeAction(_log, "DisplayChooseWorkspace", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			// Figure out if the current user is System Admin
			bool isSystemAdmin = CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.None;

			Collection<WorkspaceDTO> workspaceDtos = GetWorkspaceNameInfoToWhichTheUserHasAccessTo(isSystemAdmin, workspaceStatus);

			// create the model view
			List<ChooseWorkspaceModelView> wsModelViews = (from w in workspaceDtos
														   orderby w.WorkspaceName
														   select new ChooseWorkspaceModelView
														   {
															   WorkspaceShortname = w.Shortname,
															   WorkspaceName = w.WorkspaceName,
															   WorkspaceId = w.Id
														   }).ToList();


			// Render the workspace dropdown menu with the filtered workspace list
			ViewResult toReturn = View(WebConstants.VIEW_HOME_CHOOSE_WORKSPACE, wsModelViews);

			FinalizeAction(_log, "DisplayChooseWorkspace", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the div tag if the user is not a foreign user
		/// </summary>
		/// <returns></returns>
		public ViewResult DisplayCreateWorkspaceDiv()
		{
			Stopwatch sw = InitializeAction(_log, "DisplayCreateWorkspaceDiv", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			CreateWorkspaceModelDivView theModelView = new CreateWorkspaceModelDivView();

			ViewResult toReturn = View(WebConstants.VIEW_HOME_CREATE_WORKSPACE_DIV, theModelView);

			FinalizeAction(_log, "DisplayCreateWorkspaceDiv", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the Search for Workspace to Copy dialog
		/// </summary>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceSearch()
		{
			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceSearch", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			Collection<SelectListItem> projectMapTypes = new Collection<SelectListItem>();

			projectMapTypes.Add(new SelectListItem
			{
				Text = "All",
				Value = string.Empty // Empty/Null represents all workspace project map types
			});

			foreach (ProjectMapType projectMap in Enum.GetValues(typeof(ProjectMapType)))
			{
				projectMapTypes.Add(new SelectListItem
				{
					Text = projectMap.GetDescription(),
					Value = ((int)projectMap).ToString()
				});
			}

			ViewData["ProjectMapTypes"] = projectMapTypes;

			WorkspaceSearchModelView theModelView = new WorkspaceSearchModelView();

			ViewResult toReturn = View(WebConstants.VIEW_HOME_WORKSPACE_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayWorkspaceSearch", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the resource rates main page
		/// </summary>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceResourceRatesTM(string workspace)
		{
			return CreateWorkspaceResourceRatesTM(workspace, CreateWorkspaceResourceRateTMModelView());
		}

		protected ViewResult CreateWorkspaceResourceRatesTM(string workspace, WorkspaceResourceRateTMModelView inModel)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceResourceRatesTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_RESOURCE_RATES_TM, inModel);

			FinalizeAction(_log, "DisplayWorkspaceResourceRatesTM", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays all of the resource rates in a grid
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult DisplayWorkspaceResourceRatesGridTM(string workspace, WorkspaceResourceRateGridTMModelView modelView)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DisplayWorkspaceResourceRatesGridTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			WorkspaceResourceRateGridTMModelView theModelView = _ControllerLogic.GetWorkspaceResourceRateGridTMModelView(ws, modelView);

			ViewResult toReturn = View(WebConstants.VIEW_WORKSPACE_RESOURCE_RATES_GRID_TM, theModelView);

			FinalizeAction(_log, "DisplayWorkspaceResourceRatesGridTM", sw);
			return toReturn;
		}

		/// <summary>
		/// Allows the user to Add or Edit a workspace resource rate.
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="workspaceResourceRateID"></param>
		/// <returns></returns>
		public ViewResult DisplayAddWorkspaceResourceRateTM(string workspace, int? workspaceResourceRateID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DisplayAddWorkspaceResourceRateTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			ViewData["WORKSPACE_RATES"] = ConvertToWorkspaceResourceRateOptionList(_ControllerLogic.GetWorkspaceResourcesForWorkspaceResourceRateTM(ws));

			WorkspaceResourceRateTMModelView modelView = CreateWorkspaceResourceRateTMModelView();

			if (workspaceResourceRateID.HasValue)
			{
				modelView = _ControllerLogic.GetWorkspaceResourceRateTMModelView(ws, workspaceResourceRateID.Value);
			}

			ViewResult toReturn = View(WebConstants.VIEW_ADD_WORKSPACE_RESOURCE_RATE_TM, modelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayAddWorkspaceResourceRateTM", sw);
			return toReturn;
		}

		#endregion Partial Views

		#endregion Display

		#region AJAX Calls

		/// <summary>
		/// Finds the adjacent BOEs for the passed in BOE.
		/// </summary>
		/// <param name="workspace">The workspace name.</param>
		/// <param name="boeId">The boe id.</param>
		/// <param name="predicate">The sort predicate.</param>
		/// <param name="sortOrder">The sort order.</param>
		/// <returns></returns>
		public JsonResult FindAdjacentBoes(string workspace, int boeId, string[] predicate, string sortOrder)
		{
			if (string.IsNullOrEmpty(sortOrder))
			{
				throw new ArgumentNullException(nameof(sortOrder));
			}

			if (predicate == null || predicate.Length == 0)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_FIND_ADJACENT_BOES, SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			// get all data
			HomeWorkspaceGridModelView theModelView = _GetHomeWorkspaceGridData(ws);
			SortOrder order = SortOrder.Ascending;
			if (sortOrder.ToLower() == "desc")
			{
				order = SortOrder.Descending;
			}

			string sortColumn = predicate[0];
			if (predicate.Length == 2 && predicate[0] == "UpdateDate" && predicate[1] == "WBSText")
			{
				sortColumn = "Recent";
			}

			AdjacentItems adjacentItems = this._ControllerLogic.FindAdjacentBoes(theModelView, boeId, sortColumn, order);

			// Action Finalize
			FinalizeAction(_log, WebConstants.ACTION_FIND_ADJACENT_BOES, sw);

			return Json(adjacentItems);
		}

		/// <summary>
		/// Gets the Workspace email preferences.
		/// </summary>
		/// <returns>Json result of the Workspace email preferences.</returns>
		public JsonResult GetWorkspaceEmailPreferences(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_GET_WORKSPACE_EMAIL_PREFERENCES, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Get the current preferences
			ICollection<WorkspaceEmailOverrideModelView> emails = this._ControllerLogic.GetWorkspaceEmails(ws.Id).OrderBy(e => e.Category).ThenBy(e => e.Recipient).ToList();

			this.FinalizeAction(this._log, WebConstants.ACTION_GET_WORKSPACE_EMAIL_PREFERENCES, sw);

			return this.Json(new { emails = emails, isReadOnly = false });
		}

		/// <summary>
		/// Saves the workspace email preferences.
		/// </summary>
		/// <param name="emails">The emails.</param>
		/// <returns>A Json value</returns>
		public JsonResult SaveWorkspaceEmailPreferences(string workspace, ICollection<WorkspaceEmailOverrideModelView> emails)
		{
			if (emails == null)
			{
				throw new ArgumentNullException(nameof(emails));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_WORKSPACE_EMAIL_PREFERENCES, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			if (ModelState.IsValid)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this._ControllerLogic.SaveWorkspaceEmails(emails, ws.Id);
					scope.Complete();
				}
			}
			else
			{
				throw new ValidationException(SiteMasterUtilities.CreateValidationErrorResponse(ModelState));
			}

			this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_WORKSPACE_EMAIL_PREFERENCES, sw);

			return this.Json(new { Status = true });
		}

		/// <summary>
		/// This function calls the Workspace Home partial view with the workspace ID
		/// </summary>
		///<param name="workspace">the workspace to use in the system</param>
		/// <returns>model data returned as json</returns>
		public JsonResult GetWorkspaceHomeModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "GetWorkspaceHomeModel", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			// get all data
			HomeWorkspaceGridModelView theModelView = _GetHomeWorkspaceGridData(ws);

			theModelView.isReadOnly = SiteMasterUtilities.IsReadOnly();

			if (CheckPermissions(SecurityPage.SystemAdmin, null, null) == SecurityAuthorization.CreateReadUpdateDelete)
			{
				theModelView.isReadOnly = false;
			};

			// Action Finalize
			FinalizeAction(_log, "GetWorkspaceHomeModel", sw);

			return Json(theModelView);
		}

		/// <summary>
		/// Validates changes to workspace status
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="to">state the workspace wants to change to</param>
		/// <param name="from">state the workspace is currently in</param>
		/// <returns>Passed/Fail JSON</returns>
		public JsonResult WorkspaceStatusChangeValidation(string workspace, WorkspaceState to, WorkspaceState from)
		{
			JsonResult toReturn = Json(new { Status = true });

			if (ModelState.IsValid)
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

				try
				{
					if (!ws.IsProjectMapWorkspace)
					{
						string validationMessage = string.Empty;

						if (!_WorkspaceStateMachine.PerformStateTransitionValidation(ws, from, to, out validationMessage))
						{
							toReturn = Json(new { Status = false, Message = validationMessage });
						}
					}
				}
				catch (InvalidOperationException)
				{
					toReturn = Json(new { Status = false, Message = "Invalid State transition." });
				}
			}
			else
			{
				toReturn = Json(new { Status = false, Message = SiteMasterUtilities.CreateValidationErrorResponse(ModelState) });
			}

			return toReturn;
		}

		/// <summary>
		/// Save Custom Fields.
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="to">state the workspace wants to change to</param>
		/// <param name="from">state the workspace is currently in</param>
		/// <returns>Passed/Fail JSON</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public JsonResult SaveCustomFields(string workspace, BOECustomFieldModelView customFieldsMV)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveCustomFields", SecurityPage.BoeCustomFields, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (customFieldsMV == null)
			{
				throw new ArgumentNullException(nameof(customFieldsMV));
			}

			JsonResult result = Json(new { Status = true });

			if (ModelState.IsValid)
			{
				ICollection<CustomFieldValueDTO> customfieldValuestoSave = new Collection<CustomFieldValueDTO>();
				ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

				bool isNewCustomField;

				if (customFieldsMV.CustomFieldMetaData.CustomFieldID > 0)  // if editing an existing custom field
				{
					isNewCustomField = false;

					//need to do this because what you get back in readonly.
					customfieldValuestoSave = customfieldValuestoSave.Concat(_CustomFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldID(customFieldsMV.CustomFieldMetaData.CustomFieldID)).ToCollection<CustomFieldValueDTO>();
				}
				else
				{
					isNewCustomField = true;
				}

				bool optionsEdited = false;

				if (customFieldsMV.CustomFieldMetaData.isOpenEnded)
				{
					// clear out any values if the field used to not be open ended
					foreach (CustomFieldValueDTO currentValue in customfieldValuestoSave)
					{
						currentValue.Updateable = UpdateType.Deleted;
					}
				}
				else
				{
					//get updated latest options in a DTO we need this to validate the options.
					foreach (BOECustomFieldOptionModelView options in customFieldsMV.CustomFieldOptions)
					{
						if (options.CustomFieldOptionID < 0) // if adding a new option
						{
							if (options.ID.Contains("-"))
							{
								validationMessages.Add(new ValidationMessage(
												"ID cannot contain a dash."));
							}

							customfieldValuestoSave.Add(new CustomFieldValueDTO()
							{
								Id = options.CustomFieldOptionID,
								CustomFieldID = customFieldsMV.CustomFieldMetaData.CustomFieldID,
								CustomFieldValueName = options.ID,
								CustomFieldValueDescription = options.Description,
								CustomFieldValueID = options.CustomFieldOptionID,
								Updateable = UpdateType.Upsert
							});
						}
						else // if editing an existing option
						{
							foreach (CustomFieldValueDTO currentValue in customfieldValuestoSave)
							{
								if (currentValue.CustomFieldValueID == options.CustomFieldOptionID)
								{
									optionsEdited = true;

									if (currentValue.CustomFieldValueName != options.ID)
									{
										if (currentValue.CustomFieldValueInUseFlag == true)
										{
											validationMessages.Add(new ValidationMessage(
												"ID cannot be edited for an in-use Custom Field Value."));
										}

										if (options.ID.Contains("-"))
										{
											validationMessages.Add(new ValidationMessage(
															"ID cannot contain a dash."));
										}
									}

									currentValue.CustomFieldValueName = options.ID;
									currentValue.CustomFieldValueDescription = options.Description;
									if (options.Deleted)
									{
										currentValue.Updateable = UpdateType.Deleted;
									}
									else
									{
										currentValue.Updateable = UpdateType.Upsert;
									}

									break;
								}
							}
						}
					}
				}

				//Extra validation
				if (!CustomFieldLevelSelected(customFieldsMV.CustomFieldMetaData))
				{
					validationMessages.Add(new ValidationMessage("At Least one level is required."));
				}

				ICollection<String> toValidate = (from cfvs in customfieldValuestoSave where cfvs.Updateable != UpdateType.Deleted select cfvs.CustomFieldValueName.Trim()).ToList();
				if (!toValidate.Any() && !customFieldsMV.CustomFieldMetaData.isOpenEnded)
				{
					validationMessages.Add(new ValidationMessage("At least one option Id is required."));
				}
				UniqueStringValidatorHelper UniqueIDvalidator = new UniqueStringValidatorHelper();
				if (!UniqueIDvalidator.Validator(toValidate))
				{
					validationMessages.Add(new ValidationMessage("Option Ids must be unique."));
				}

				if (ws.CustomFields.Any(x => x.CustomFieldName.Trim().ToLower() == customFieldsMV.CustomFieldMetaData.FieldName.Trim().ToLower() &&
					x.Id != customFieldsMV.CustomFieldMetaData.CustomFieldID))
				{
					validationMessages.Add(new ValidationMessage("Field name must be unique."));
				}
				if (validationMessages.Any())
				{
					throw new GenValidationException(validationMessages);
				}
				// end validation


				CustomFieldDTO customfieldtoSave;
				CustomFieldDTO originalCustomFieldToDelete = null;

				if (customFieldsMV.CustomFieldMetaData.CustomFieldID >= 0)
				{
					customfieldtoSave = this.Factory.CreateCustomField(customFieldsMV.CustomFieldMetaData.CustomFieldID);
				}
				else
				{
					customfieldtoSave = new CustomFieldDTO() { Id = customFieldsMV.CustomFieldMetaData.CustomFieldID, WorkspaceID = ws.Id };
				}

				// determine original vs. edited values

				CustomFieldType customFieldDisplayIdOriginal = customfieldtoSave.CustomFieldDisplayID;
				CustomFieldType customFieldDisplayId = customFieldsMV.CustomFieldMetaData.CustomFieldDisplayID;
				bool customFieldTypeChanged = !isNewCustomField && (customFieldDisplayIdOriginal != customFieldDisplayId);

				bool isRequiredOriginal = customfieldtoSave.CustomFieldRequired;
				bool isRequired = customFieldsMV.CustomFieldMetaData.isRequired;
				bool requiredChanged = !isNewCustomField && (isRequiredOriginal != isRequired);

				bool isOpenEndedOriginal = customfieldtoSave.IsOpenEnded;
				bool isOpenEnded = customFieldsMV.CustomFieldMetaData.isOpenEnded;
				bool openEndedChanged = !isNewCustomField && (isOpenEndedOriginal != isOpenEnded);
				bool customFieldValueInUse = false;

				string customFieldNameOriginal = customfieldtoSave.CustomFieldName;
				string customFieldName = customFieldsMV.CustomFieldMetaData.FieldName;
				bool customFieldNameChanged = !isNewCustomField && (customFieldNameOriginal != customFieldName);

				//if the display field is changed (ex: task -> resource), the old values should be removed or they will still show up in some areas
				//the simplest way to do this is to delete the custom field and add it as new
				if (customFieldTypeChanged)
				{
					// "mark" the deletion here, but defer execution of the actual delete until we can confirm OK to proceed with committing changes
					originalCustomFieldToDelete = customfieldtoSave;
					originalCustomFieldToDelete.Updateable = UpdateType.Deleted;

					customfieldtoSave = new CustomFieldDTO() { Id = -1, WorkspaceID = ws.Id, CustomFieldRequired = isRequired, IsOpenEnded = isOpenEnded };
					int valueID = -1;
					foreach (CustomFieldValueDTO value in customfieldValuestoSave)
					{
						value.Id = valueID;
						value.CustomFieldID = -1;
						value.CustomFieldValueID = valueID;
						value.Updateable = UpdateType.Upsert;
						customFieldsMV.CustomFieldMetaData.CustomFieldID = -1;
						valueID--;
						customFieldValueInUse = customFieldValueInUse || value.CustomFieldValueInUseFlag;
					}
				}

				customfieldtoSave.CustomFieldName = customFieldsMV.CustomFieldMetaData.FieldName;
				customfieldtoSave.CustomFieldRequired = isRequired;
				customfieldtoSave.IsOpenEnded = isOpenEnded;
				customfieldtoSave.CustomFieldDisplayID = customFieldDisplayId;
				customfieldtoSave.Updateable = UpdateType.Upsert;

				ICollection<FullBoe> boesBackToDraft = new Collection<FullBoe>();

				/*
				 * The following edit scenarios might require a transition of the BOE states:
				 * 
				 *    1. Adding a new custom field that is required
				 *    2. Editing an existing non-required CF to make it required (the reverse is OK)
				 *    3. Editing an existing required field's name
				 *    4. Adding/editing an existing required fields options/values
				 *    5. Changing the Type of a custom field.
				 * 
				 */
				if (customFieldTypeChanged || (isRequired && (isNewCustomField || requiredChanged || openEndedChanged || customFieldNameChanged || optionsEdited)))
				{
					// Update all BOEs that are in awaiting approval, approved or locked-draft back to draft.
					boesBackToDraft = ws.Boes.Where(x => x.State == BOEState.Approved || x.State == BOEState.AwaitingApproval || x.State == BOEState.DraftLocked).ToCollection<FullBoe>();
				}

				bool okToProceed;

				if (boesBackToDraft.Any())
				{
					/*
					 * If these edits will cause any of the workspace BOEs to have their states changed back to Draft,
					 * then make sure the user is aware and agrees with this before continuing.
					 * 
					 */
					if ((okToProceed = customFieldsMV.UserHasConfirmed) == false)
					{
						// send response back to UI indicating that user confirmation is needed
						result = Json(new
						{
							Status = false,
							NeedUserConfirmation = true,
							UserConfirmationMessage = "Adding or editing a required Custom Field, or changing a custom field level, will set all BOEs in this workspace back to Draft.  Do you wish to continue?"
						});
					}
				}
				else if (customFieldTypeChanged && customFieldValueInUse)
				{
					/*
					 * If the custom field level changed and the field is in use, 
					 * then make sure the user is aware the usages will be cleared and agrees with this before continuing.
					 */
					if ((okToProceed = customFieldsMV.UserHasConfirmed) == false)
					{
						// send response back to UI indicating that user confirmation is needed
						result = Json(new
						{
							Status = false,
							NeedUserConfirmation = true,
							UserConfirmationMessage = "This custom field is currently in use.  Changing the level from " + customFieldDisplayIdOriginal.ToDescription() + " to " + customFieldDisplayId.ToDescription() + " will clear all existing usages.  Do you wish to continue?"
						});
					}
				}
				else
				{
					okToProceed = true;
				}

				if (okToProceed)
				{
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						if (originalCustomFieldToDelete != null)
						{
							_CustomFieldLoader.Save(originalCustomFieldToDelete);  // execute deletion that was previously detected (above)
						}

						int? customfieldID = _CustomFieldLoader.Save(customfieldtoSave);

						//change the custom field id if these are new 
						if (customFieldsMV.CustomFieldMetaData.CustomFieldID < 1)
						{
							foreach (CustomFieldValueDTO options in customfieldValuestoSave)
							{
								options.CustomFieldID = customfieldID.Value;
							}
						}

						_CustomFieldValueLoader.Save(customfieldValuestoSave.ToCollection<CustomFieldValueDTO>());

						// BOEJ-1014 - Let's take a look at the fields that are being deleted or updated during this save.
						BOEState newBOEState = BOEState.Draft;

						foreach (FullBoe boe in boesBackToDraft)
						{
							string errorMessage = string.Empty;

							if (_BOEStateMachine.PerformStateTransitionValidation(boe, ws, boe.State, newBOEState, out errorMessage))
							{
								BOEState oldState = boe.State;
								boe.State = newBOEState;
								boe.Updateable = UpdateType.Upsert;

								_BoeMediator.MediatedSave(ws, boe);

								// save of BOE worked .. perform transition steps and send email
								_BOEStateMachine.PerformStateTransitionAction(boe, ws, oldState, boe.State);
							}
							else
							{
								// we can't move the BOE back to DRAFT for some reason ... abort
								// pull this error message from the state machine itself
								throw new GeneralAppException(errorMessage);
							}
						}

						scope.Complete();
					}

					foreach (FullBoe boe in boesBackToDraft)
					{
						_emailer.SendBOEAuthorsEmailOpenedForEdit(boe, ws);
					}
				}  // end if okToProceed
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveCustomFields", sw);

			return result;
		}

		public JsonResult DeleteCustomField(string workspace, BOECustomFieldsGridModelView customFieldsMV)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DeleteCustomField", SecurityPage.BoeCustomFields, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (customFieldsMV == null)
			{
				throw new ArgumentNullException(nameof(customFieldsMV));
			}

			CustomFieldDTO customField = this.Factory.CreateCustomField(customFieldsMV.CustomFieldID);
			DataRelationshipVerifier.VerifyDataRelation(customField, ws.Id);

			customField.UpdateDate = new DateTime(customFieldsMV.UpdateDate.Ticks);
			customField.Updateable = UpdateType.Deleted;

			ICollection<FullBoe> boesBackToDraft = new Collection<FullBoe>();

			if (customField.CustomFieldRequired)
			{
				// Update all BOEs that are in awaiting approval or approved back to draft.
				boesBackToDraft = ws.Boes.Where(x => x.State == BOEState.Approved || x.State == BOEState.AwaitingApproval).ToCollection<FullBoe>();
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				_CustomFieldLoader.Save(customField);

				BOEState newBOEState = BOEState.Draft;

				foreach (FullBoe boe in boesBackToDraft)
				{
					string errorMessage = string.Empty;

					if (_BOEStateMachine.PerformStateTransitionValidation(boe, ws, boe.State, newBOEState, out errorMessage))
					{
						BOEState oldState = boe.State;
						boe.State = newBOEState;
						boe.Updateable = UpdateType.Upsert;

						_BoeMediator.MediatedSave(ws, boe);

						// save of BOE worked .. perform transition steps and send email
						_BOEStateMachine.PerformStateTransitionAction(boe, ws, oldState, boe.State);
					}
					else
					{
						// we can't move the BOE back to DRAFT for some reason ... abort
						// pull this error message from the state machine itself
						throw new GeneralAppException(errorMessage);
					}
				}

				scope.Complete();
			}

			foreach (FullBoe boe in boesBackToDraft)
			{
				_emailer.SendBOEAuthorsEmailOpenedForEdit(boe, ws);
			}

			// Finalize Action
			FinalizeAction(_log, "DeleteCustomField", sw);

			return Json(new { Status = true });
		}

		/// <summary>
		/// Saves workspace settings
		/// 
		/// This method has been changed to minimize the amount of work that is done inside of a transaction
		///     If the decimal precision was changed, a long recalculation will run before the data is saved in the transaction
		///     While this is happening, a different user could change underlying data that would make the recalculation out of date
		///     This case will be caught during the transaction, which will throw an exception, and so no harm will be done
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="workspaceDetails">Workspace Identification Model View</param>
		/// <returns>JSON true/false</returns>
		public ActionResult SaveWorkspaceIdentification(string workspace, [WorkspaceIdentificationBinder] IWorkspaceIdentificationModelView workspaceDetails)
		{
			_ = workspaceDetails ?? throw new ArgumentNullException(nameof(workspaceDetails));

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceIdentification", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			string returnMessage = string.Empty;

			if (ModelState.IsValid)
			{
				returnMessage = _workspaceSettingsControllerLogic.SaveWorkspaceIdentification(Factory, ws, workspaceDetails);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceIdentification", sw);

			return Json(new { Status = true, Message = returnMessage });
		}

		/// <summary>
		/// Update just the Current PTM Workspace during the Copy Workspace workflow
		/// Current copy workspace workflow will update current workspace to false
		/// </summary>
		/// <param name="workspace">workspace short name</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public ActionResult UpdateCurrentWorkspaceIdentification(string workspace)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);
			Stopwatch sw = InitializeAction(_log, "UpdateCurrentWorkspaceIdentification", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Get the user who is saving the BOE(s)
			int currentUserID = ws.CurrentActiveUser.UserID;

			ws.CurrentPTMWorkspace = false;

			TimeSpan timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT));

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = timeout }))
			{
				this.workspaceLoader.SaveWorkspaceSettings(currentUserID, ws);

				scope.Complete();
			}

			this.Factory.ClearWorkspaceCache(workspace);

			FinalizeAction(_log, "UpdateCurrentWorkspaceIdentification", sw);

			return Json(new { Status = true });
		}

		/// <summary>
		/// Saves workspace output format template settings
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="inOutputFormatId">The id of the template selected</param>
		/// <returns>JSON true/false</returns>
		public ActionResult SaveWorkspaceOutputFormat(string workspace, int inOutputFormatId, int inSortById)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceOutputFormat", SecurityPage.WorkspaceSettingsOutputFormatTemplate, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = Json(new { Status = true });
			if (ModelState.IsValid)
			{
				// Export Format
				ws.BOEExportSortByID = inSortById;

				ws.TemplateID = inOutputFormatId;
				// Get the user who is saving the BOE(s)
				int currentUserID = ws.CurrentActiveUser.UserID;

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this.workspaceLoader.SaveWorkspaceSettings(currentUserID, ws);
					this.Factory.ClearWorkspaceCache(ws.Shortname);
					scope.Complete();
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceOutputFormat", sw);

			return toReturn;
		}

		/// <summary>
		/// Saves workspace settings
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="workspaceState">The selected workspace status</param>
		/// <returns>JSON true/false</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ActionResult SaveWorkspaceStatus(string workspace, WorkspaceStatusModelView workspaceStatusMV)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceStatus", SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (workspaceStatusMV == null)
			{
				throw new ArgumentNullException(nameof(workspaceStatusMV));
			}

			JsonResult toReturn = Json(new { Status = false });
			if (ModelState.IsValid)
			{
				if (this._ControllerLogic.SaveWorkspaceStatus(workspaceStatusMV, ref ws, ref _log))
				{
					toReturn = Json(new { Status = true });
				}

				//WorkspaceState originalState = ws.WorkspaceState;

				//if (!ws.IsProjectMapWorkspace)
				//{
				//	string validationMessage = string.Empty;

				//	// check state validation before worrying about commiting to the database
				//	if (!_WorkspaceStateMachine.PerformStateTransitionValidation(ws, originalState, workspaceStatusMV.WorkspaceStatus, out validationMessage))
				//	{
				//		// not valid ... communicate to user
				//		throw new GenValidationException("Error: Unable to change state: " + validationMessage);
				//	}
				//}

				//// Workspace State
				//ws.WorkspaceState = workspaceStatusMV.WorkspaceStatus;
				//ws.UpdateDate = workspaceStatusMV.UpdateDate;

				//try
				//{
				//	// Get the user who is saving the BOE(s)
				//	int currentUserID = ws.CurrentActiveUser.UserID;

				//	// Save the workspace
				//	using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				//	{
				//		this.workspaceLoader.SaveWorkspaceSettings(currentUserID, ws);

				//		// transition after the save is successful
				//		ws = this.Factory.CreateFullWorkspace(ws.Shortname, true);

				//		if (!ws.IsProjectMapWorkspace)
				//		{
				//			this.TransitionBOEStates(ws, originalState, ws.WorkspaceState);
				//			_WorkspaceStateMachine.PerformStateTransitionAction(ws, originalState, ws.WorkspaceState);
				//		}
				//		this.Factory.ClearWorkspaceCache(ws.Shortname);
				//		scope.Complete();
				//	}

				//	toReturn = Json(new { Status = true });
				//}
				//catch (Exception ex)
				//{
				//	_log.Error(ex);
				//	if (ex.InnerException != null)
				//	{
				//		_log.Error(ex.InnerException);
				//	}
				//}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceStatus", sw);

			return toReturn;
		}

		/// <summary>
		/// After a workspace state has been changed, process all of its BOEs to see if they also require state changes.
		/// </summary>
		/// <param name="workspace">The workspace we are working on</param>
		/// <param name="workspaceFromState">Old state of the workspace</param>
		/// <param name="workspaceToState">New state for the workspace</param>
		private void TransitionBOEStates(FullWorkspace workspace, WorkspaceState workspaceFromState, WorkspaceState workspaceToState)
		{
			foreach (FullBoe boe in workspace.Boes)
			{
				BOEState? newBOEState = this.GetBOETransitionState(boe.State, workspaceFromState, workspaceToState);
				if (newBOEState.HasValue)
				{
					string errorMessage = string.Empty;

					if (_BOEStateMachine.PerformStateTransitionValidation(boe, workspace, boe.State, newBOEState.Value, out errorMessage))
					{
						// apply the actual state-value update
						boe.State = newBOEState.Value;
						boe.Updateable = UpdateType.Upsert;

						_BoeMediator.MediatedSave(workspace, boe);

						_BOEStateMachine.PerformStateTransitionAction(boe, workspace, boe.State, newBOEState.Value);
					}
				}
			}
		}

		/// <summary>
		/// Determine the BOE state (if any) that all workspace BOEs will need to transition to if the workspace state is transitioned.
		/// </summary>
		/// <param name="current">Current state of the BOE</param>
		/// <param name="workspaceFromState">Old state of the workspace</param>
		/// <param name="workspaceToState">New state for the workspace</param>
		/// <returns>Corresponding BOE state, or null if no BOE state change is required</returns>
		private BOEState? GetBOETransitionState(BOEState currentBOEState, WorkspaceState workspaceFromState, WorkspaceState workspaceToState)
		{
			BOEState? newBOEState = null;

			if (workspaceToState == WorkspaceState.Locked ||
				workspaceToState == WorkspaceState.Complete ||
				workspaceToState == WorkspaceState.Closed)
			{
				if (currentBOEState == BOEState.Draft)
				{
					newBOEState = BOEState.DraftLocked;
				}
			}
			else if (workspaceFromState == WorkspaceState.Locked ||
				workspaceFromState == WorkspaceState.Complete ||
				workspaceFromState == WorkspaceState.Closed)
			{
				if (currentBOEState == BOEState.DraftLocked)
				{
					newBOEState = BOEState.Draft;
				}
			}

			return newBOEState;
		}

		/// <summary>
		/// Deletes all Sum of BOE Variables for the given workspace
		/// </summary>
		/// <param name="workspace">Workspace containing Sum of BOE Variables to be deleted</param>
		/// <param name="valueType">Value type of variables to be deleted - Sum of BOEs or Discrete</param>
		/// <returns>Json result</returns>
		public JsonResult DeleteAllWorkspaceVariables(string workspace, VarValueType valueType)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DeleteAllWorkspaceVariables", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			Collection<WorkspaceVariableDTO> allVars = ws.WorkspaceVariables.Where(x => x.ValueType == valueType).ToCollection<WorkspaceVariableDTO>();

			_ControllerLogic.DeleteWorkspaceVariables(allVars);

			JsonResult toReturn = Json(new { Status = true });

			// Finalize Action
			FinalizeAction(_log, "DeleteAllWorkspaceVariables", sw);

			return toReturn;
		}

		/// <summary>
		/// Saves workspace settings
		/// </summary>
		/// <param name="workspace">workspace shortname</param>
		/// <param name="workspaceVariables">The modified workspace variable model views</param>
		/// <returns>JSON true/false</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ActionResult SaveWorkspaceVariables(string workspace, Collection<WorkspaceVariableModelView> workspaceVariables)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);
			HashSet<BoeDTO> originalWsBoes = new HashSet<BoeDTO>(ws.Boes.ToList<BoeDTO>().DeepClone());
			HashSet<BoeTaskElementDTO> tasksToSave = new HashSet<BoeTaskElementDTO>();
			HashSet<WorkspaceVariableDTO> workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
			HashSet<FullBoe> boesToTransition = new HashSet<FullBoe>();
			WorkspaceState originalWsState = ws.WorkspaceState;
			int currentUserID = ws.CurrentActiveUser.UserID;
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceVariables", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = Json(new { Status = false });

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				// Workspace Variables
				Collection<WorkspaceVariableDTO> modifiedWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
				if (workspaceVariables != null)
				{

					foreach (WorkspaceVariableModelView variableMV in workspaceVariables)
					{
						// trim any whitespace in variable name 
						variableMV.WorkspaceVariableName = variableMV.WorkspaceVariableName.Trim();

						WorkspaceVariableDTO variable = null;

						if (variableMV.WorkspaceVariableID < 0)
						{
							variable = variableMV.GetAssociatedDTO();
						}
						else
						{
							// Need to get the original copy from the DB.
							variable = _WorkspaceVariableLoader.GetByIds(new Collection<int> { variableMV.WorkspaceVariableID }).First();
							DataRelationshipVerifier.VerifyDataRelation(variable, ws.Id);
							variable = variableMV.GetAssociatedDTO(variable);
						}

						variable.WorkspaceID = ws.Id;

						modifiedWorkspaceVariables.Add(variable);
					}
				}

				VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();
				List<WorkspaceVariableDTO> validatedWorkspaceVariables = _VariableCircularReferenceChecker.ValidateWorkspaceVariableSave(cache, modifiedWorkspaceVariables, ws).ToList();
				Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
				// if any returned true there was an error
				if (validatedWorkspaceVariables.Any())
				{
					ValidationErrors.Add(new ValidationMessage("The following Workspace Variables contain circular references. To remove, edit its summed BOEs."));

					foreach (WorkspaceVariableDTO wvar in validatedWorkspaceVariables)
					{
						ValidationErrors.Add(new ValidationMessage("- " + wvar.WorkspaceVariableName));
					}
				}

				if (ValidationErrors.Any())
				{
					throw new GenValidationException(ValidationErrors);
				}


				//NOTE: This logic will need to change when Workspace Var Sum of BOEs come into play
				// since we'll have to calculate the OldValue on the fly to compare it with the newValue

				// get the original workspace variable list so we can determine if values changed
				// if they did, we need to send an email to the BOE Author
				IReadOnlyCollection<WorkspaceVariableDTO> originalWorkspaceVars = ws.WorkspaceVariables;
				IEnumerable<WorkspaceVariableDTO> changedWorkspaceVars = from o in originalWorkspaceVars
																		 from m in modifiedWorkspaceVariables
																		 where o.Id == m.Id &&
																			   (o.WorkspaceVariableValue != m.WorkspaceVariableValue ||
																				o.ValueType != m.ValueType ||
																				o.SelectedBOEsToSum.Where(b => b.BoeID.HasValue).Select(b => b.BoeID).Except(m.SelectedBOEsToSum.Where(b => b.BoeID.HasValue).Select(b => b.BoeID)).Count() > 0 ||
																				o.SelectedBOEsToSum.Where(b => b.WBSID.HasValue).Select(b => b.WBSID).Except(m.SelectedBOEsToSum.Where(b => b.WBSID.HasValue).Select(b => b.WBSID)).Count() > 0 ||
																				o.SelectedBOEsToSum.Where(b => b.CLINID.HasValue).Select(b => b.CLINID).Except(m.SelectedBOEsToSum.Where(b => b.CLINID.HasValue).Select(b => b.CLINID)).Count() > 0 ||
																				m.SelectedBOEsToSum.Where(b => b.BoeID.HasValue).Select(b => b.BoeID).Except(o.SelectedBOEsToSum.Where(b => b.BoeID.HasValue).Select(b => b.BoeID)).Count() > 0 ||
																				m.SelectedBOEsToSum.Where(b => b.WBSID.HasValue).Select(b => b.WBSID).Except(o.SelectedBOEsToSum.Where(b => b.WBSID.HasValue).Select(b => b.WBSID)).Count() > 0 ||
																				m.SelectedBOEsToSum.Where(b => b.CLINID.HasValue).Select(b => b.CLINID).Except(o.SelectedBOEsToSum.Where(b => b.CLINID.HasValue).Select(b => b.CLINID)).Count() > 0)
																		 select o;




				List<ValidationMessage> validationErrors = new List<ValidationMessage>();
				if (modifiedWorkspaceVariables.Any(x => x.Updateable != UpdateType.Deleted))
				{
					ICollection<WorkspaceVariableDTO> allNonModifiedVariables = ws.WorkspaceVariables.Where(i => !modifiedWorkspaceVariables.Select(x => x.Id).Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
					List<WorkspaceVariableDTO> allVariables = new List<WorkspaceVariableDTO>(allNonModifiedVariables);
					allVariables.AddRange(modifiedWorkspaceVariables.Where(x => x.Updateable != UpdateType.Deleted)); // from UI
																													  // determine if new/modified variables are duplicates within our WS vars and across Task Vars
					foreach (WorkspaceVariableDTO variable in modifiedWorkspaceVariables)
					{
						Collection<ValidationMessage> validationMessages = _WSVarUniqueValidator.Validate(variable.WorkspaceVariableName, allVariables, ws);
						validationErrors.AddRange(validationMessages);
					}
				}

				// Get all deleted variables that are currently in use. These must not be deleted.
				IEnumerable<ValidationMessage> invalidDeletions = from m in modifiedWorkspaceVariables
																  from o in originalWorkspaceVars
																  where m.Id == o.Id &&
																		m.Updateable == UpdateType.Deleted &&
																		o.InUse
																  select new ValidationMessage(String.Format("'{0}' is used in at least one BOE and cannot be deleted.", m.WorkspaceVariableName));

				validationErrors.AddRange(invalidDeletions);

				if (validationErrors.Any())
				{
					throw new GenValidationException(_WSVarUniqueValidator.CreateValidationErrorResponse(validationErrors));
				}

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					_WorkspaceVariableLoader.SaveWorkspaceVariables(modifiedWorkspaceVariables);

					// get any BOE Task Elements that may be have be effected by this workspace variable change
					if (changedWorkspaceVars.Any())
					{
						// Refresh workspace with newly updated workspace variables
						ws.RefreshWorkspaceVariables();

						// get the task elements that are directly effected by this workspace variable update
						foreach (WorkspaceVariableDTO workspaceVariable in modifiedWorkspaceVariables)
						{
							Collection<BoeTaskElementDTO> tasks = _BoeTaskElementRecalculation.RecalculateLaborWithVariable(workspaceVariable.Id, VariableType.Workspace, ws);

							bool boesWereUpdated = false;
							foreach (BoeTaskElementDTO task in tasks)
							{
								// get boe to determine if state needs to change. if a variable causes the task element to be recalculated and the boe is in awaiting approval/approved, it needs
								// to go back to draft
								FullBoe boeToSave = FullWorkspaceHelper.GetBoeById(ws, task.BoeID);

								BOEState oldBOEState = boeToSave.State;
								if (boeToSave.State == BOEState.AwaitingApproval || boeToSave.State == BOEState.Approved || boeToSave.State == BOEState.DraftLocked)
								{
									BOEState newBOEState = BOEState.Draft;

									// Validate the Awaiting Approval or Approved to Draft state transition
									string validationMessage = string.Empty;
									if (!_BOEStateMachine.PerformStateTransitionValidation(boeToSave, ws, oldBOEState, newBOEState, out validationMessage))
									{
										// not valid ... communicate to user
										throw new ValidationException(validationMessage);
									}
									// If the transition is valid, set the BOE to Draft and save it
									boeToSave.Updateable = UpdateType.Upsert;
									boeToSave.State = newBOEState;
									_BoeMediator.MediatedSave(ws, boeToSave);

									// Perform common state transition actions
									_BOEStateMachine.PerformStateTransitionAction(boeToSave, ws, oldBOEState, boeToSave.State);
									boesWereUpdated = true;
								}
							}
							if (boesWereUpdated)
							{
								// Refresh workspace with updated BOEs.
								ws.RefreshBoes();
							}

							// tasks are the task elements that are directly effected by the workspace variable save, but now we need to find task elements that are indirectly effected
							_ControllerLogic.CalculateLinkedTaskElements(tasks, ws);
							//make sure we have the latest information or task elements and workspace variables after the initial save.
							ws.RefreshTaskElements();
							ws.RefreshWorkspaceVariables();
							//we only want to recalculate the whole workspace if we have a workspace var thats sum of boe and in use or a task var thats a sum of boe.
							if (ws.WorkspaceVariables.Any(v => v.ValueType == VarValueType.SumOfBOEs && v.InUse) || ws.TaskElements.Any(t => t.OrdinaryVariables.Any(o => o.ValueType == VarValueType.SumOfBOEs)))
							{
								this._ControllerLogic.ChangeTheWorkspaceStateDuringRecalculation(ws, currentUserID, WorkspaceState.Initialization, DateTime.Now);
								this.FullWsRecalc.RecalculateLaborInAWorkspaceWithoutSaving(ws, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition);
								this.FullWsRecalc.SaveDataEffectedByRecalculation(ws, tasksToSave, workspaceVariablesToSave, boesToTransition);
								this.FullWsRecalc.ValidateStateTransitionForBoesEffectedByRecalculation(ws, boesToTransition, originalWsBoes);
								this.FullWsRecalc.PerformStateTransitionActionsForBoesEffectedByRecalculation(ws, boesToTransition, originalWsBoes);
								this._ControllerLogic.ChangeTheWorkspaceStateDuringRecalculation(ws, currentUserID, originalWsState, null);
							}

						}
					}

					scope.Complete();
				}

				toReturn = Json(new { Status = true });
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceVariables", sw);

			return toReturn;
		}

		public ActionResult DeleteWorkspaceVersions(string workspace, Collection<WorkspaceVersionModelView> inVersions)
		{
			if (inVersions == null)
			{
				throw new ArgumentNullException(nameof(inVersions));
			}
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DeleteWorkspaceVersions", SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = Json(new { Status = false });
			if (ModelState.IsValid)
			{
				Collection<WorkspaceVersionMetaDataDTO> toSave = new Collection<WorkspaceVersionMetaDataDTO>();
				Collection<int> inVersionIds = (from v in inVersions
												select v.VersionID).ToCollection();

				toSave = _WorkspaceVersionMetaDataDTODataLoader.GetByIds(inVersionIds).ToCollection();

				foreach (WorkspaceVersionMetaDataDTO version in toSave)
				{
					version.Updateable = UpdateType.Deleted;
				}

				// Save the workspace
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					_WorkspaceVersionMetaDataDTODataLoader.Save(toSave, ws.Id);
					scope.Complete();
				}

				toReturn = Json(new { Status = true });
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "DeleteWorkspaceVersions", sw);
			return toReturn;
		}

		public ActionResult SaveWorkspaceVersion(string workspace, String versionName)
		{
			if (versionName == null)
			{
				throw new ArgumentNullException(nameof(versionName));
			}
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceVersion", SecurityPage.SaveVersion, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = Json(new { Status = false });

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				Collection<WorkspaceVersionMetaDataDTO> toSave = new Collection<WorkspaceVersionMetaDataDTO>();

				//validate version name
				string trimmedVersionName = versionName.Trim();
				if (trimmedVersionName.StartsWith(CommonConstants.AUTO_SYSTEM_BACKUP_DAILY))
				{
					throw new GenValidationException($"Version name cannot start with \"{CommonConstants.AUTO_SYSTEM_BACKUP_DAILY}\"");
				}

				IReadOnlyCollection<WorkspaceVersionMetaDataDTO> workspaceVersions = ws.WorkspaceVersionMetaData;

				foreach (WorkspaceVersionMetaDataDTO versionToCheck in workspaceVersions)
				{
					if (trimmedVersionName == versionToCheck.VersionName)
					{
						throw new GenValidationException("Version name must be unique.");
					}
				}

				toSave.Add(new WorkspaceVersionMetaDataDTO()
				{
					VersionName = trimmedVersionName,
					Updateable = UpdateType.Upsert,
					CreatedByID = ws.CurrentActiveUser.UserID,
					DateCreated = new DateTime(),
					VersionState = ws.WorkspaceState
				});

				// Save the workspace
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					_WorkspaceVersionMetaDataDTODataLoader.Save(toSave, ws.Id);
					scope.Complete();
				}

				toReturn = Json(new { Status = true });
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceVersion", sw);

			return toReturn;
		}

		/// <summary>
		/// Saves the Allow Search bit on the workspace
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="workspaceAllowSearchMV"></param>
		/// <returns></returns>
		public ActionResult SaveWorkspaceAllowSearch(string workspace, WorkspaceAllowSearchModelView workspaceAllowSearchMV)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceAllowSearch", SecurityPage.WorkspaceSettingsShareAndAllowSearch, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (workspaceAllowSearchMV == null)
			{
				throw new ArgumentNullException(nameof(workspaceAllowSearchMV));
			}

			JsonResult toReturn = Json(new { Status = true });

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				// Export Format
				ws.AllowSearch = workspaceAllowSearchMV.AllowSearch;
				ws.ContainsTemplate = workspaceAllowSearchMV.ContainsTemplate;
				ws.UpdateDate = workspaceAllowSearchMV.UpdateDate;

				// Get the user who is saving the BOE(s)
				int currentUserID = ws.CurrentActiveUser.UserID;

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this.workspaceLoader.SaveWorkspaceSettings(currentUserID, ws);
					scope.Complete();
				}

				// Clear the cache after since LastUpdateTime was changed
				this.Factory.ClearWorkspaceCache(ws.Shortname);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveWorkspaceAllowSearch", sw);

			return toReturn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public JsonResult SaveCustomFieldResources(string workspace, Collection<BOECustomFieldResourceModelView> resources)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveCustomFieldResources", SecurityPage.BoeCustomFieldResource, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (resources == null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			JsonResult toReturn = Json(new { Status = true });
			UserDTO WorkspaceAdmin = ws.CurrentActiveUser;
			Collection<InUseResourceChangedValueContainer> inUseResourceChangedContainer = new Collection<InUseResourceChangedValueContainer>();

			BOECustomFieldResourceModelView singleEditMV = null;
			IDictionary<int, int> resourceSaveResultsDictionary = null;

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				// validate performing org name
				foreach (BOECustomFieldResourceModelView resource in resources)
				{
					// only check new items.
					if (resource.Deleted == false)
					{
						Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
						validationData.Add(new Dictionary<string, string>() {
							{"ResourceListID", ws.ResourceListID.ToString() },
							{"ResourceID", resource.CustomFieldOptionID.ToString() }
						});
						Validator validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueID);
						if (validator.validation(resource.ID, validationData).Count > 0)
						{
							ValidationErrors.Add(new ValidationMessage("resourceID", "The Resource ID must be unique"));
						}

						validator = ValidationFactory.Instance.getValidator(ValidationType.ResourceUniqueDesc);
						if (validator.validation(resource.Description, validationData).Count > 0)
						{
							ValidationErrors.Add(new ValidationMessage("resourceDesc", "The Resource Description must be unique"));
						}
					}
				}

				// convert to DTO
				Collection<ResourceDTO> resourceDTOs = new Collection<ResourceDTO>();
				Collection<FieldChanged> fieldsChanged = new Collection<FieldChanged>();

				ICollection<ResourceDTO> resourcesFromDb = this._ResourceLoader.GetByIds(resources.Select(x => x.CustomFieldOptionID).Distinct().ToList());

				foreach (BOECustomFieldResourceModelView resource in resources)
				{
					if (resource.Deleted == false)
					{
						ResourceDTO resourceDTO = null;
						// new resource
						if (resource.CustomFieldOptionID < 0)
						{
							resourceDTO = new ResourceDTO();
						}
						else
						{
							if (resource.CustomFieldOptionID > 0 && resources.Count == 1)
							{
								singleEditMV = resource;
							}

							resourceDTO = resourcesFromDb.First(x => x.Id == resource.CustomFieldOptionID);

							if (resourceDTO == null)
							{
								throw new InvalidDataRelationException("The resource was not found in the resource list for this workspace");
							}

							if (resourceDTO.ResourceName != resource.ID)
							{
								throw new GenValidationException("The resource ID cannot be modified.");
							}
						}

						Boolean inUse = _InUseDataLoader.GetInUse(InUseDataType.WorkspaceResources, resourceDTO.Id, ws.ResourceListID);

						// get field changes for a resource in use
						if (inUse == true)
						{
							if (resourceDTO.ResourceDesc != resource.Description)
							{
								fieldsChanged.Add(new FieldChanged
								{
									Field = "Description",
									OldValue = resourceDTO.ResourceDesc,
									NewValue = resource.Description
								});
							}

							if (resourceDTO.SegRegion != resource.SegmentRegion)
							{
								fieldsChanged.Add(new FieldChanged
								{
									Field = "Segment Region",
									OldValue = resourceDTO.SegRegion,
									NewValue = resource.SegmentRegion
								});
							}

							if (resourceDTO.LaborType != resource.LaborType)
							{
								fieldsChanged.Add(new FieldChanged
								{
									Field = "Labor Type",
									OldValue = resourceDTO.LaborType,
									NewValue = resource.LaborType
								});
							}
						}

						resourceDTO.LaborType = resource.LaborType;
						resourceDTO.ResourceDesc = resource.Description;
						resourceDTO.ResourceName = resource.ID;
						resourceDTO.SegRegion = resource.SegmentRegion;

						resourceDTO.RateType = resource.RateTypeID; // added to get resource save to work
						resourceDTO.Segment = SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST ? SegmentType.RMS : SegmentType.SSC;
						resourceDTO.ElementOfCost = resource.ElementOfCostId;

						resourceDTO.UpdateDate = resource.UpdateDate;
						resourceDTO.Updateable = UpdateType.Upsert;

						resourceDTOs.Add(resourceDTO);

						// add to the container if the resource is in use since the container is only used for sending in use emails
						if (inUse == true)
						{
							inUseResourceChangedContainer.Add(new InUseResourceChangedValueContainer { Resource = resourceDTO, Changes = fieldsChanged });
						}

					}
					else
					{
						ResourceDTO resourceDTO = resourcesFromDb.First(x => x.Id == resource.CustomFieldOptionID);

						if (resourceDTO == null)
						{
							throw new InvalidDataRelationException("The resource was not found in the resource list for this workspace");
						}

						resourceDTO.UpdateDate = resource.UpdateDate;
						resourceDTO.Updateable = UpdateType.Deleted;

						resourceDTOs.Add(resourceDTO);
					}
				}

				if (ValidationErrors.Count > 0)
				{
					throw new GenValidationException(ValidationErrors);
				}

				// perform save
				if (resourceDTOs.Count > 0)
				{
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						resourceSaveResultsDictionary = _ResourceLoader.SaveWorkspaceResources(ws, resourceDTOs);

						scope.Complete();
					}

					foreach (InUseResourceChangedValueContainer resourceChanged in inUseResourceChangedContainer)
					{
						// if an edit is made to a resource that was previously linked to a system resource, a new resource is created so make sure to get the new
						// resource data

						ResourceDTO newResource = this._ResourceLoader.GetByDescriptionAndListId(resourceChanged.Resource.ResourceName, ws.ResourceListID);

						if (newResource != null)
						{
							_emailer.SendBOEAuthorsApproversInUseResourceUpdated(newResource, resourceChanged.Changes, WorkspaceAdmin, ws);
						}
					}

					ProcessResourceIdUpdates(ws, resourceSaveResultsDictionary, resourceDTOs.Where(r => r.Updateable == UpdateType.Deleted).Select(rd => rd.Id).ToList());
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// if an edit, return the new data
			if (singleEditMV != null)
			{
				// Re-get the dto from the DB so we have the latest update date.
				ResourceDTO resource = this._ResourceLoader.GetById(resourceSaveResultsDictionary[singleEditMV.CustomFieldOptionID]);

				string elementOfCost = _CommonDataMapper.GetElementOfCostTypesDictionary()[(int)resource.ElementOfCost].ElementOfCostName;
				string rateTypeName = resource.RateType.GetDescription();

				singleEditMV = new BOECustomFieldResourceModelView(resource, rateTypeName, elementOfCost);

				toReturn = Json(singleEditMV);
			}

			// Finalize Action
			FinalizeAction(_log, "SaveCustomFieldResources", sw);

			return toReturn;
		}

		/// <summary>
		/// Saves modifications to performing orgs to the database.
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="performingOrgs"></param>
		/// <returns></returns>
		public JsonResult SaveCustomFieldPerformingOrgs(string workspace, Collection<BOECustomFieldOptionModelView> performingOrgs)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveCustomFieldPerformingOrgs", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (performingOrgs == null)
			{
				throw new ArgumentNullException(nameof(performingOrgs));
			}

			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			JsonResult toReturn = Json(new { Status = true });

			BOECustomFieldOptionModelView singleEditMV = null;


			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				// validate performing org name
				foreach (BOECustomFieldOptionModelView performingOrg in performingOrgs)
				{
					// only check new items.
					if (performingOrg.Deleted == false)
					{
						Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
						validationData.Add(new Dictionary<string, string>() {
							{"PerformingOrgListID", ws.PerfOrgListID.ToString() },
							{"PerformingOrgID", performingOrg.CustomFieldOptionID.ToString() }
						});
						Validator validator = ValidationFactory.Instance.getValidator(ValidationType.PerformingOrgUniqueID);
						if (validator.validation(performingOrg.ID, validationData).Count > 0)
						{
							//throw new GenValidationException("The performing organization ID must be unique");
							ValidationErrors.Add(new ValidationMessage("PerformingOrgID", "The performing organization ID must be unique"));
						}
					}
				}

				// convert to DTO
				Collection<PerformingOrgDTO> performingOrgDTOs = new Collection<PerformingOrgDTO>();
				HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(performingOrgs.Select(x => x.CustomFieldOptionID).Distinct().ToList()));

				foreach (BOECustomFieldOptionModelView performingOrg in performingOrgs)
				{
					if (performingOrg.Deleted == false)
					{
						PerformingOrgDTO performingOrgDTO = null;
						// new resource
						if (performingOrg.CustomFieldOptionID < 0)
						{
							performingOrgDTO = new PerformingOrgDTO();
						}
						else
						{
							if (performingOrgs.Count == 1)
							{
								singleEditMV = performingOrg;
							}

							performingOrgDTO = performingOrgsFromDb.FirstOrDefault(x => x.Id == performingOrg.CustomFieldOptionID);

							if (performingOrgDTO == null)
							{
								throw new InvalidDataRelationException("The resource was not found in the resource list for this workspace");
							}
						}

						performingOrgDTO.PerformingOrgName = performingOrg.ID;
						performingOrgDTO.PerformingOrgDesc = performingOrg.Description;
						performingOrgDTO.UpdateDate = performingOrg.UpdateDate;
						performingOrgDTO.Updateable = UpdateType.Upsert;

						performingOrgDTOs.Add(performingOrgDTO);
					}
					else
					{
						PerformingOrgDTO performingOrgDTO = performingOrgsFromDb.FirstOrDefault(x => x.Id == performingOrg.CustomFieldOptionID);

						if (performingOrgDTO == null)
						{
							throw new InvalidDataRelationException("The resource was not found in the resource list for this workspace");
						}

						performingOrgDTO.UpdateDate = performingOrg.UpdateDate;
						performingOrgDTO.Updateable = UpdateType.Deleted;

						performingOrgDTOs.Add(performingOrgDTO);
					}
				}

				if (ValidationErrors.Any())
				{
					throw new GenValidationException(ValidationErrors);
				}

				// perform save
				if (performingOrgDTOs.Any())
				{
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						this.perfOrgLoader.SaveWorkspacePerformingOrgs(performingOrgDTOs, ws.PerfOrgListID);

						// update workspace as well since we updated the orgs
						this.workspaceLoader.UpdatePerfOrgChangeFlag(ws, true);

						scope.Complete();
					}
				}

				if (singleEditMV != null)
				{
					// this could have changed from a system perf org to a workspace perf org so always make sure to get its new data
					PerformingOrgDTO performingOrg = this.perfOrgLoader.GetByListIdAndName(ws.PerfOrgListID, singleEditMV.ID);
					singleEditMV = new BOECustomFieldOptionModelView(performingOrg);

					toReturn = Json(singleEditMV);
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveCustomFieldPerformingOrgs", sw);

			return toReturn;
		}

		/// <summary>
		/// Restores the list of performing orgs to the global list.
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>Perf Org restore view result</returns>
		public ViewResult RestoreCustomFieldPerformingOrganizations(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "RestoreCustomFieldPerformingOrganizations", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			RestoreOptionData restoreResults = null;

			ViewResult toReturn = null;
			/** Valid Model Check */
			if (ModelState.IsValid)
			{

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					restoreResults = _RestoreDefaultOptions.RestorePerfOrg(ws);
					scope.Complete();
				}
				ViewData["SYSTEM_LIST_NAME"] = ws.PerformingOrgList.PerformingOrgListName;
				toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_RESTORE, restoreResults);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}
			// Finalize Action
			FinalizeAction(_log, "RestoreCustomFieldPerformingOrganizations", sw);

			return toReturn;
		}

		/// <summary>
		/// Restores the list of resources to the global list.
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>Resource Retore view result</returns>
		public ViewResult RestoreCustomFieldResources(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "RestoreCustomFieldResources", SecurityPage.BoeCustomFieldResource, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			RestoreOptionData restoreResults = null;
			ViewResult toReturn = null;

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					restoreResults = _RestoreDefaultOptions.RestoreSystemResources(ws);
					scope.Complete();
				}

				toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE_RESTORE, restoreResults);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "RestoreCustomFieldResources", sw);

			return toReturn;
		}

		/// <summary>
		/// Restores the workspace with a previous version
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public JsonResult RestoreWorkspaceVersion(string workspace, int VersionID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			string initialTrackingNumber = ws.TrackingNumber;
			string initialShortName = ws.Shortname;

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "RestoreWorkspaceVersion", SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			bool RestoreSuccess = false;

			JsonResult toReturn;
			string versionErrors = null;
			string shortName = null;

			if (ws.WorkspaceVersionMetaData.Where(x => x.Id == VersionID).Any())
			{
				WorkspaceVersionMetaDataDTO workspaceVersion = ws.WorkspaceVersionMetaData.FirstOrDefault(x => x.Id == VersionID);

				// Get the user who is restoring the workspace version
				int currentUserID = ws.CurrentActiveUser.UserID;

				try
				{
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						versionErrors = _WorkspaceVersionMetaDataDTODataLoader.Restore(workspaceVersion, currentUserID);

						// if the returned string is empty, the restore was a success, else not 
						// a restore still can occur if a warning occurs for performing org, resources, or travel trips
						RestoreSuccess = true;

						// now have to check if the url is ok
						if (Utilities.IsPTMIntegrated)
						{
							this.Factory.ClearWorkspaceCache(workspace);
							WorkspaceDTO wsDto = this.Factory.CreateFullWorkspace(ws.Id);

							if ((wsDto.TrackingNumber != initialTrackingNumber || wsDto.Shortname != initialShortName) && !string.IsNullOrWhiteSpace(wsDto.TrackingNumber))
							{
								string nextRevision = this.GetNextTrackingNumber(wsDto.TrackingNumber);
								wsDto.Shortname = nextRevision;

								if (wsDto.WorkspaceName.StartsWith(wsDto.TrackingNumber))
								{
									wsDto.WorkspaceName = wsDto.WorkspaceName.Substring(wsDto.TrackingNumber.Length);
								}

								wsDto.WorkspaceName = nextRevision + " " + wsDto.WorkspaceName;
								wsDto.Updateable = UpdateType.Upsert;

								this.workspaceLoader.SaveIdentificationAndExportFormat(ws.CurrentActiveUser.UserID, wsDto);

								shortName = wsDto.Shortname;
							}
						}

						scope.Complete();
					}
				}
				catch (GeneralAppException ex)
				{
					// Error during the restore
					toReturn = Json(new { Errors = ex.Message });
				}
			}

			// only return the warning msg 
			if (!string.IsNullOrEmpty(versionErrors))
			{
				toReturn = Json(new { Errors = versionErrors, Shortname = shortName });
			}
			else
			{
				toReturn = Json(new { Status = true, Shortname = shortName });
			}

			// send email if the restore was successful
			if (RestoreSuccess)
			{
				this.Factory.ClearWorkspaceCache(workspace);
				_emailer.SendWorkspaceRestored(this.Factory.CreateFullWorkspace(ws.Id), ws.WorkspaceVersionMetaData.Where(x => x.Id == VersionID).FirstOrDefault().DateCreated);
			}

			// Finalize Action
			FinalizeAction(_log, "RestoreWorkspaceVersion", sw);

			return toReturn;
		}

		/// <summary>
		/// Performs an export of a previous version of a Workspace
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <param name="versionId">ID of the version to export</param>
		/// <param name="exportAllBoes">Bool noting if all BOEs to be exported or just selected ones</param>
		/// <param name="boesToExport">BOEs to be exported if not exporting all</param>
		/// <returns>Report</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ActionResult ExportWorkspaceVersion(string workspace, int versionId, bool exportAllBoes, ICollection<int> boesToExport)
		{
			ActionResult toReturn = new EmptyResult();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			string versionName = this._WorkspaceVersionMetaDataDTODataLoader.GetByIds(new Collection<int>() { versionId }).FirstOrDefault()?.VersionName;

			int tempWsId = this._ControllerLogic.CopyWorkspaceVersion(ws, versionId, exportAllBoes, boesToExport);

			FullWorkspace tempWs = this.Factory.CreateFullWorkspace(tempWsId);
			tempWs.LoadBoesAndTaskElementsRTEData();
			tempWs.LoadTravelRTEData();
			tempWs.LoadODCsRTEData();
			tempWs.LoadMaterialsRTEData();

			string excelTemplateLocaiton = Server.MapPath(workspaceExporter.WORKSPACE_DATA_EXCEL_MAP_PATH);

			Dictionary<string, Stream> zipContents = new Dictionary<string, Stream>();

			string workspaceDataReportLocation = this._ControllerLogic.CreateWorkspaceDataReportForVersion(tempWs, excelTemplateLocaiton, ws.WorkspaceName, versionId);

			try
			{
				using (FileStream workspaceDataStream = new FileStream(workspaceDataReportLocation, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
				{
					workspaceDataStream.Position = 0;
					zipContents.Add(Utilities.CleanFileName(string.Format("WorkspaceData-{0}-{1}.xlsx", ws.Shortname, versionName)), workspaceDataStream);

					using (Stream allBoesStream = new MemoryStream())
					{
						bool isCustomExport;
						WorkspaceExportFormatDTO wsExportFormatDTO;
						BOEExportInputs exportInputs;
						ICollection<BOEExportModelView> boeExportModelViews;
						List<BOESummaryGridModelView> boeSummaryGridModelViews;

						// Set Version Template ID to the current Template ID so the user can select the template
						tempWs.TemplateID = ws.TemplateID;

						this.reportsControllerLogic.PrepareAllBOEsReport(tempWs, ws.CurrentActiveUser.IsSubcontractor ?? false, null, null, ViewData, out isCustomExport,
							out wsExportFormatDTO, out exportInputs, out boeExportModelViews, out boeSummaryGridModelViews, false);

						if (isCustomExport)
						{
							this.boeCustomExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, null, allBoesStream, wsExportFormatDTO);
						}
						else
						{
							this.boeExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, wsExportFormatDTO.PhysicalFilePathCache,
								allBoesStream, wsExportFormatDTO.ExportFormat.TemplateType);
						}

						allBoesStream.Position = 0;
						zipContents.Add(Utilities.CleanFileName(string.Format("AllBOEs-{0}-{1}.docx", ws.Shortname, versionName)), allBoesStream);

						string zipFileName = Zip.ZipFiles(zipContents, Server.MapPath("~/Templates/Export"));

						string fileName = Utilities.CleanFileName(string.Format("BackupExport_{0}_{1}.zip", ws.Shortname, versionName));
						// Generate a custom ActionResult to cause a file download to the client
						FileStream fs = new FileStream(zipFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

						toReturn = File(
							fileStream: fs,
							contentType: ExportFileDownloadBase.GetContentType(fileName),
							fileDownloadName: fileName);
					}
				}
			}
			catch (Exception e)
			{
				_log.Error(e);
			}

			return toReturn;
		}

		/// <summary>
		/// Searches the custom field performing orgs by text
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		public ViewResult SearchCustomFieldPerformingOrganizations(string workspace, string searchText)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SearchCustomFieldPerformingOrganizations", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			if (searchText == null)
			{
				throw new ArgumentNullException(nameof(searchText));
			}

			BOECustomFieldPerformingOrgGridModelView theModelView = new BOECustomFieldPerformingOrgGridModelView();
			theModelView.PerformingOrgResults = new Collection<BOECustomFieldOptionModelView>();

			string trimmedSearchText = searchText.ToLower().Trim();
			foreach (PerformingOrgDTO performingOrg in ws.PerformingOrgsForWsList)
			{
				if (performingOrg.PerformingOrgName.ToLower().Contains(trimmedSearchText) ||
					performingOrg.PerformingOrgDesc.ToLower().Contains(trimmedSearchText))
				{
					theModelView.PagedIndexes.Add(performingOrg.Id);
					if (theModelView.PerformingOrgResults.Count < theModelView.ResultsPerPage)
					{
						theModelView.PerformingOrgResults.Add(new BOECustomFieldOptionModelView(performingOrg));
					}
				}
			}

			theModelView.TotalResults = ws.PerformingOrgsForWsList.Count;

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID, theModelView);

			// Finalize Action
			FinalizeAction(_log, "SearchCustomFieldPerformingOrganizations", sw);

			return toReturn;
		}

		/// <summary>
		/// Searches the custom field resource by text
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		public ViewResult SearchCustomFieldResources(string workspace, string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SearchCustomFieldResources", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			BOECustomFieldResourceGridModelView theModelView = _GetAndFilterCustomFieldResources(workspace, searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);

			// Finalize Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE_GRID, theModelView);

			// Finalize Action
			FinalizeAction(_log, "SearchCustomFieldResources", sw);

			return toReturn;
		}

		/// <summary>
		/// Go to a new page on the resources grid
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="resources"></param>
		/// <returns></returns>
		public ViewResult PageCustomFieldResources(string workspace, BOECustomFieldResourceGridModelView resources)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "PageCustomFieldResources", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			if (resources == null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			resources.ResourceResults = new Collection<BOECustomFieldResourceModelView>();
			int resourceListID = ws.ResourceListID;

			HashSet<int> resourceIDsInUse = _InUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);

			ICollection<ResourceDTO> resourcesFromDb = this._ResourceLoader.GetByIds(resources.PagedIndexes.Distinct().ToList());
			IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = this._CommonDataMapper.GetElementOfCostTypesDictionary();

			for (int i = resources.StartArrayIndex; i <= resources.EndArrayIndex; i++)
			{
				ResourceDTO resourceDTO = resourcesFromDb.First(x => x.Id == resources.PagedIndexes[i]);

				if (resourceDTO == null)
				{
					throw new InvalidDataRelationException("One of the resources was not found.  List: " + resourceListID +
						" ID: " + resources.PagedIndexes[i]);
				}

				string ElementOfCostDisplay = allElementOfCostTypes[(int)resourceDTO.ElementOfCost].ElementOfCostName;
				string rateTypeDisplay = resourceDTO.RateType.GetDescription();

				BOECustomFieldResourceModelView mv = new BOECustomFieldResourceModelView(resourceDTO, rateTypeDisplay, ElementOfCostDisplay);
				if (resourceIDsInUse.Contains(mv.CustomFieldOptionID))
				{
					mv.InUse = true;
				}
				else
				{
					mv.InUse = false;
				}

				resources.ResourceResults.Add(mv);
			}

			resources.TotalResults = ws.PerformingOrgsForWsList.Count;

			// Return the partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_RESOURCE_GRID, resources);

			// Finalize Action
			FinalizeAction(_log, "PageCustomFieldResources", sw);
			return toReturn;
		}

		/// <summary>
		/// Go to a new page on the performing orgs grid
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="performingOrgs"></param>
		/// <returns></returns>
		public ViewResult PageCustomFieldPerformingOrgs(string workspace, BOECustomFieldPerformingOrgGridModelView performingOrgs)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "PageCustomFieldPerformingOrgs", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			if (performingOrgs == null)
			{
				throw new ArgumentNullException(nameof(performingOrgs));
			}

			performingOrgs.PerformingOrgResults = new Collection<BOECustomFieldOptionModelView>();
			int performingOrgListID = ws.PerfOrgListID;

			HashSet<int> perfOrgIDsInUse = _InUseDataLoader.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(performingOrgListID);

			List<int> perfOrgIdsToRetrieve = new List<int>();
			for (int i = performingOrgs.StartArrayIndex; i <= performingOrgs.EndArrayIndex; i++)
			{
				perfOrgIdsToRetrieve.Add(performingOrgs.PagedIndexes[i]);
			}

			HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(perfOrgIdsToRetrieve));

			for (int i = performingOrgs.StartArrayIndex; i <= performingOrgs.EndArrayIndex; i++)
			{
				PerformingOrgDTO performingOrgDTO = performingOrgsFromDb.FirstOrDefault(x => x.Id == performingOrgs.PagedIndexes[i]);

				if (performingOrgDTO == null)
				{
					throw new InvalidDataRelationException("One of the performing orgs was not found.  List: " + performingOrgListID + " ID: " + performingOrgs.PagedIndexes[i]);
				}

				BOECustomFieldOptionModelView mv = new BOECustomFieldOptionModelView(performingOrgDTO);
				if (perfOrgIDsInUse.Contains(performingOrgDTO.Id))
				{
					mv.InUse = true;
				}
				else
				{
					mv.InUse = false;
				}
				performingOrgs.PerformingOrgResults.Add(mv);
			}

			performingOrgs.TotalResults = this.perfOrgLoader.GetByListId(ws.PerfOrgListID).Count;

			// Return the partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID, performingOrgs);

			// Finalize Action
			FinalizeAction(_log, "PageCustomFieldPerformingOrgs", sw);
			return toReturn;
		}

		/// <summary>
		/// Save the workspace admin's preference if they want to see the Getting Start With Workspace Help section for this workspace
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ActionResult SaveHideGettingStartedHelpMenu(string workspace, GettingStartedHelpModelView helpModel)
		{
			if (helpModel == null)
			{
				throw new ArgumentNullException(nameof(helpModel));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveHideGettingStartedHelpMenu", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = Json(new { Status = true });
			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				UserDTO user = ws.CurrentActiveUser;
				Collection<PermissionsDTO> permissions = (from p in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
														  where p.Role == Role.WorkspaceAdmin && p.ETIUserId == user.UserID
														  select p).ToCollection();

				// don't bother saving if no permissions were returned
				// in theory, you would never see the workspace help if you didn't have permissions so you couldn't save anyway
				if (permissions.Count() > 0)
				{
					// set up permisson to save with preference of showing workspace home help
					PermissionsDTO WAPerrmison = new PermissionsDTO();
					WAPerrmison.HideWorkspaceHelp = helpModel.HideGettingStartedHelp;
					WAPerrmison.UpdateDate = permissions[0].UpdateDate;
					WAPerrmison.WorkspaceId = ws.Id;
					WAPerrmison.PermissionId = permissions[0].PermissionId;
					WAPerrmison.Role = Role.WorkspaceAdmin;
					WAPerrmison.ETIUserId = user.UserID;

					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						this.PermissionsLoader.SaveWorkspaceHideHelp(WAPerrmison);
						scope.Complete();
					}
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveHideGettingStartedHelpMenu", sw);

			return toReturn;
		}

		public JsonResult ValidateCreateWorkspaceStepOne([CreateWorkspaceStepOneBinder] ICreateWorkspaceStepOneModelView data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			List<ValidationMessage> errors = new List<ValidationMessage>();

			// do basic validation check
			if (!ModelState.IsValid && (data.WSExactCopy == null || data.WSExactCopy == false))
			{
				errors.AddRange(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Bug 6899
			// If we are doing an import, all we need to check is to make sure that the workspace name exists (that means that something is being copied)
			// If we are NOT doing an import, we do the original validation
			if (data.IsAttemptingToImport)
			{
				if (String.IsNullOrEmpty(data.WorkspaceName))
				{
					errors.Add(new ValidationMessage("WorkspaceName", "Name of Workspace To Copy is required."));
				}
			}
			else
			{
				// Perform Cost Volume Lead/Pricer validation - Must be an individual (not a group), and not a subcontractor.
				if (!string.IsNullOrEmpty(data.CostVolumeLeadPricerNTID))
				{
					errors.AddRange(_ControllerLogic.CostVolumeLeadPricerValidation(data.CostVolumeLeadPricerNTID));
				}

				// Perform Date Range validation
				DateRangeValidator validator = new DateRangeValidator();

				Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>()
				{
					new Dictionary<string, string>()
					{
						{ "StartDate", data.ContractStartDate },
						{ "EndDate", data.ContractEndDate }
					}
				};

				Collection<string> validationErrors = validator.validation(null, validationData);

				foreach (String ve in validationErrors)
				{
					errors.Add(new ValidationMessage("someDate", ve));
				}
			}

			if (errors.Any())
			{
				throw new GenValidationException(errors);
			}

			return Json(new { Status = true });
		}

		/// <summary>
		/// Gets the next tracking number revision sequence.
		/// </summary>
		/// <param name="trackingNumber">The tracking number.</param>
		/// <returns>The next tracking number revision sequence.</returns>
		public JsonResult GetNextTrackingNumberRevision(string trackingNumber)
		{
			string nextRevision = this.GetNextTrackingNumber(trackingNumber);

			// other results to return
			int lobId = -1;
			string rfpNumber = string.Empty;
			int[] selectedContractTypes = new int[0];
			string title = string.Empty;
			int proposalClassId = -1;
			string anticipatedDeliveryDate = string.Empty;
			string revisedSubmittalDate = string.Empty;
			bool usingTemplateBoe = false;
			bool isSAPEnabledConfig = false;

			// see if this is a valid PTM Tracking Number
			int proposalId = this.proposalLoader.GetIdByTrackingNumber(trackingNumber);
			if (proposalId > 0)
			{
				// found the proposal
				ProposalDto proposal = this.proposalLoader.GetById(proposalId);
				rfpNumber = proposal.RFPNumber;
				title = proposal.ProposalTitle;
				if (proposal.ContractTypeIds.Any())
				{
					List<int> contractTypeIds = new List<int>();
					foreach (int contractTypeId in proposal.ContractTypeIds)
					{
						int convertedContractTypeId = this._ControllerLogic.ConvertPTMContractTypeId(contractTypeId);
						if (convertedContractTypeId > 0)
						{
							contractTypeIds.Add(convertedContractTypeId);
						}
					}

					selectedContractTypes = contractTypeIds.ToArray();
				}

				proposalClassId = this._ControllerLogic.ConvertPTMProposalClassId(proposal.ProposalClass);

				lobId = this._ControllerLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID);

				anticipatedDeliveryDate = proposal.DeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR);

				revisedSubmittalDate = proposal.RevisedSubmittalDate.HasValue
					? proposal.RevisedSubmittalDate.Value.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)
					: string.Empty;

				// Default Template BOE switch to Yes if CCoPD is set to true
				usingTemplateBoe = proposal.IsCCPDRequired.HasValue ? proposal.IsCCPDRequired.Value : false;

				isSAPEnabledConfig = Utilities.IsSAPEnabledForSystem;
			}
			else
			{
				throw new GenValidationException("PTM Tracking Number is invalid. Please delete the current Tracking Number and choose another from the dropdown list.");
			}

			return Json(new
			{
				TrackingNumberRevision = nextRevision,
				LOBId = lobId,
				RFPNumber = rfpNumber,
				ContractTypes = selectedContractTypes,
				Title = title,
				ProposalClassId = proposalClassId,
				AnticipatedDeliveryDate = anticipatedDeliveryDate,
				RevisedSubmittalDate = revisedSubmittalDate,
				UsingTemplateBoe = usingTemplateBoe,
				IsSAPEnabledConfig = isSAPEnabledConfig
			});
		}

		/// <summary>
		/// Gets the next tracking number.
		/// </summary>
		/// <param name="trackingNumber">The tracking number.</param>
		/// <returns>Next tracking number</returns>
		private string GetNextTrackingNumber(string trackingNumber)
		{
			if (string.IsNullOrEmpty(trackingNumber))
			{
				throw new GenValidationException("PTM Tracking Number cannot be null.");
			}

			// Sanitize tracking number
			string workspaceNameText = trackingNumber.ToLower();
			char[] workspaceNameCharArray = workspaceNameText.ToCharArray();
			string whiteList = "abcdefghijklmnopqrstuvwxyz0123456789-";
			for (int index = 0; index < workspaceNameCharArray.Length; index++)
			{
				if (whiteList.IndexOf(workspaceNameCharArray[index]) < 0)
				{
					workspaceNameCharArray[index] = '_';
				}
			}

			string nextRevision = new string(workspaceNameCharArray);
			if (nextRevision.Length > 15)
			{
				nextRevision = nextRevision.Substring(0, 15);
			}

			ICollection<WorkspaceDTO> trackingNameData = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo().Where(w => w.TrackingNumber == trackingNumber || w.Shortname.StartsWith(trackingNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
			if (trackingNameData.Any())
			{
				// extract revision numbers - short names should be of the format [TrackingNumber] or [TrackingNumber]_XX, where XX is the revision number
				IList<string> revisionStrings = trackingNameData.Where(x => x.Shortname.StartsWith(nextRevision + "_")).Select(x => x.Shortname.Substring(x.Shortname.IndexOf("_") + 1, 2)).ToList();
				IList<int> revisions = new List<int>();

				// confirm the extracted values are numbers and convert them to ints
				foreach (string revision in revisionStrings)
				{
					int revisionNumber;
					if (int.TryParse(revision, out revisionNumber))
					{
						revisions.Add(revisionNumber);
					}
				}

				// Get highest number or 0 if there are none
				int highestRevision = revisions.Any() ? revisions.OrderByDescending(x => x).First() : 0;

				nextRevision = nextRevision + "_" + (highestRevision + 1).ToString("00");
			}

			return nextRevision;
		}


		public JsonResult GetExactCopyData(int WorkspaceToCopyID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(WorkspaceToCopyID);

			String newWsName = null;
			String newWsShortName = null;

			Collection<WorkspaceDTO> allWsNameData = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo();

			// check to see if 'Duplicate' was already added by finding 'Duplicate' at the end of the Workspace Name
			int dupNameIndex = ws.WorkspaceName.LastIndexOf("Duplicate");
			dupNameIndex = dupNameIndex != -1 ? dupNameIndex : WORKSPACE_NAME_TRUNCATE_INDEX;

			//Find next available suffix for the workspace name
			newWsName = ws.WorkspaceName.Truncate(dupNameIndex) + "Duplicate";
			int i = 0;
			do
			{
				i++;
			}
			while (allWsNameData.Where(x => x.WorkspaceName == (newWsName + (i).ToString("00"))).Any());
			newWsName = newWsName + i.ToString("00");

			// check to see if 'dup' was already added by finding 'dup' at the end of the Workspace Short Name
			int dupShortNameIndex = ws.Shortname.LastIndexOf("dup");
			dupShortNameIndex = dupShortNameIndex != -1 ? dupShortNameIndex : WORKSPACE_SHORT_NAME_TRUNCATE_INDEX;

			//Find next available suffix for the workspace short name
			newWsShortName = ws.Shortname.Truncate(dupShortNameIndex) + "dup";
			i = 0;
			do
			{
				i++;
			}
			while (allWsNameData.Any(x => x.Shortname == (newWsShortName + (i).ToString("0000"))));
			newWsShortName = newWsShortName + i.ToString("0000");

			UserDTO LeadPricer = this.UserLoader.GetUserByID(ws.CostVolumeLeadPricerUserID);

			int[] selectedContractTypes = this.GetSelectedContractTypeOptionList(ws);
			return Json(new { Name = newWsName, ShortName = newWsShortName, DisplayName = LeadPricer.DisplayName, CostVolumeLeadPricerNTID = LeadPricer.NTID, LOBId = ws.LineOfBusiness.Id, ProposalClass = (ws.ProposalClass.Id).ToString(), ContractTypes = selectedContractTypes });
		}

		public JsonResult IsWorkspaceNameAvailable(String value)
		{
			bool result = false;

			FullWorkspace ws = this.Factory.CreateFullWorkspace(value);

			if (ws == null || ws.Id == -1)
			{
				result = true;
			}

			return Json(new { Status = result });
		}

		public JsonResult IsWorkspaceShortNameAvailable(String value)
		{
			return Json(new { Status = IsWorkspaceShortNameAvailableBool(value) });
		}

		private bool IsWorkspaceShortNameAvailableBool(String value)
		{
			Collection<WorkspaceDTO> workspaces = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo();
			return !(from workspace in workspaces where workspace.Shortname == value select workspace).Any();
		}

		/// <summary>
		/// Saves a new workspace.
		/// </summary>
		/// <param name="newWorkspace"></param>
		/// <returns></returns>
		[MaxDbQuery(-1)]
		public JsonResult SaveNewWorkspace([CreateWorkspaceBinder] ICreateWorkspaceModelView newWorkspace)
		{
			_ = newWorkspace ?? throw new ArgumentNullException(nameof(newWorkspace));

			bool finishedWithoutErrors = true;

			HttpContext.Items["IgnoreWorkspaceFullObjectCache"] = true;
			Stopwatch sw = InitializeAction(_log, "SaveNewWorkspace", SecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, null, null);
			List<ValidationMessage> errors = new List<ValidationMessage>();
			bool decimalPrecisionChanged = false;
			bool costDecimalPrecisionChanged = false;

			JsonResult toReturn = Json(new { Status = false });
			FullWorkspace newWs = null;

			#region Validation

			if (!ModelState.IsValid)
			{
				errors.AddRange(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Perform Cost Volume Lead/Pricer validation - Must be an individual (not a group), and not a subcontractor.
			errors.AddRange(_ControllerLogic.CostVolumeLeadPricerValidation(newWorkspace.CostVolumeLeadPricerNTID));

			// Perform Date Range validation
			DateRangeValidator validator = new DateRangeValidator();

			Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>()
			{
				new Dictionary<string, string>()
				{
					{ "StartDate", newWorkspace.ContractStartDate },
					{ "EndDate", newWorkspace.ContractEndDate }
				}
			};

			Collection<string> validationErrors = validator.validation(null, validationData);

			foreach (String ve in validationErrors)
			{
				errors.Add(new ValidationMessage("someDate", ve));
			}

			if (errors.Any() && (newWorkspace.WSExactCopy == null || newWorkspace.WSExactCopy == false))
			{
				throw new GenValidationException(errors);
			}

			#endregion

			#region Do the initial save

			// Objects we could be saving
			int newWorkspaceID;
			UserDTO createdByUserDTO = this.UserLoader.GetUserForActiveUser();
			WorkspaceDTO newWorkspaceDTO;

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				if (newWorkspace.WSExactCopy.HasValue && newWorkspace.WSExactCopy.Value)
				{
					if (newWorkspace.WorkspaceToCopyID > 0)
					{
						UserDTO newCostVolumeLeadPricerDTO = this.UserLoader.GetOrCreateUserByNtid(newWorkspace.CostVolumeLeadPricerNTID);
						newWorkspaceID = _WorkspaceCopier.CopyWorkspaceExactly(newWorkspace.WorkspaceToCopyID, newWorkspace.WorkspaceName, newWorkspace.Shortname, newCostVolumeLeadPricerDTO.UserID);
						newWorkspaceDTO = this.Factory.CreateFullWorkspace(newWorkspaceID);

						// Set Project Map Workspaces to Working state - they can't be set to Initialization
						if (newWorkspaceDTO.IsProjectMapWorkspace)
						{
							newWorkspaceDTO.WorkspaceState = WorkspaceState.Working;
							this.workspaceLoader.SaveWorkspaceSettings(createdByUserDTO.UserID, newWorkspaceDTO);
						}

						if (Utilities.IsPTMIntegrated)
						{
							CreateWorkspaceSpaceModelView spaceWorkspace = newWorkspace as CreateWorkspaceSpaceModelView;
							if (spaceWorkspace != null)
							{
								// need to copy over the possibly overridden PTM values as well
								newWorkspaceDTO.TrackingNumber = spaceWorkspace.TrackingNumber;
								// Only need to update the ID for LOB
								newWorkspaceDTO.LineOfBusiness.Id = spaceWorkspace.LineOfBusinessID;
								newWorkspaceDTO.RFPNumber = spaceWorkspace.RFPNumber;
								newWorkspaceDTO.SelectedContractTypes = spaceWorkspace.SelectedContractTypes;
								newWorkspaceDTO.ProposalClass = new PickListDto { Id = spaceWorkspace.ProposalClass };
								newWorkspaceDTO.ProposalTitle = spaceWorkspace.ProposalTitle;
								newWorkspaceDTO.RevisedSubmittalDate = !string.IsNullOrEmpty(spaceWorkspace.RevisedSubmittalDate)
									? (DateTime?)Convert.ToDateTime(spaceWorkspace.RevisedSubmittalDate) : null;
								newWorkspaceDTO.CurrentPTMWorkspace = spaceWorkspace.CurrentPTMWorkspace;

								this.workspaceLoader.SaveWorkspaceSettings(createdByUserDTO.UserID, newWorkspaceDTO);
							}
							else
							{
								this._log.Error("PTM Integrated for Space set to true, but the modelView was not CreateWorkspaceSpaceModelView.");
								throw new GenValidationException("The type of the model is not set to CreateWorkspaceSpaceModelView when PTM Integration was turned on.");
							}
						}
					}
				}
				else
				{
					if (createdByUserDTO == null)
					{
						UserData createdByUserData = _securityInformation.ActiveUserData;
						createdByUserDTO = new UserDTO();
						createdByUserDTO.FirstName = createdByUserData.FirstName;
						createdByUserDTO.LastName = createdByUserData.LastName;
						createdByUserDTO.DisplayName = createdByUserData.DisplayName;
						createdByUserDTO.NTID = createdByUserData.Ntid;
						createdByUserDTO.EmailAddress = createdByUserData.Email;
						createdByUserDTO.PhoneNumber = createdByUserData.Phone;
						createdByUserDTO.IsUsPerson = createdByUserData.IsUsPerson;
						createdByUserDTO.IsSubcontractor = createdByUserData.IsSubcontractor;
						createdByUserDTO = this.UserLoader.SaveUser(createdByUserDTO);
					}

					// Get/Save Cost Volume Lead
					UserDTO costVolumeLeadPricerDTO = this.UserLoader.GetOrCreateUserByNtid(newWorkspace.CostVolumeLeadPricerNTID);

					// Create the Workspace
					newWorkspaceDTO = new WorkspaceDTO();
					newWorkspaceDTO.ContainsOCI = newWorkspace.ContainsOCI;
					newWorkspaceDTO.ContractEndDate = Convert.ToDateTime(newWorkspace.ContractEndDate);
					newWorkspaceDTO.ContractStartDate = Convert.ToDateTime(newWorkspace.ContractStartDate);
					newWorkspaceDTO.Description = newWorkspace.Description;
					newWorkspaceDTO.ProposalSubmittalDate = newWorkspace.ProposalSubmittalDate != null ? (DateTime?)Convert.ToDateTime(newWorkspace.ProposalSubmittalDate) : null;
					newWorkspaceDTO.LineOfBusiness = new PickListDto() { Id = newWorkspace.LineOfBusinessID };
					newWorkspaceDTO.Segment = newWorkspace.Segment;
					newWorkspaceDTO.RFPNumber = newWorkspace.RFPNumber;
					newWorkspaceDTO.TrackingNumber = newWorkspace.TrackingNumber;
					newWorkspaceDTO.Shortname = newWorkspace.Shortname;
					newWorkspaceDTO.WorkspaceName = newWorkspace.WorkspaceName;
					newWorkspaceDTO.CreatedByUserID = createdByUserDTO.UserID;
					newWorkspaceDTO.CostVolumeLeadPricerUserID = costVolumeLeadPricerDTO.UserID;
					newWorkspaceDTO.ContainsTemplate = newWorkspace.ContainsContentTemplates;
					newWorkspaceDTO.AllowSearch = newWorkspace.ShareAndAllowSearch;
					newWorkspaceDTO.IsUsingEquivalentPerson = newWorkspace.IsUsingEquivalentPerson;
					newWorkspaceDTO.IsUsingTM = newWorkspace.IsUsingTM;
					newWorkspaceDTO.RteSizeLimit = newWorkspace.RteSizeLimit;
					newWorkspaceDTO.UsingTemplateBOE = newWorkspace.UsingTemplateBoe;
					newWorkspaceDTO.EnableSAPConnection = newWorkspace.EnableSAPConnection;
					newWorkspaceDTO.CurrentPTMWorkspace = newWorkspace.CurrentPTMWorkspace;
					newWorkspaceDTO.EnableAssignTaskAuthor = newWorkspace.EnableAssignTaskAuthor;

					if (Utilities.ShowPLDIsIntegrated)
					{
						newWorkspaceDTO.ProposalTitle = newWorkspace.ProposalTitle;
					}

					if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
					{
						newWorkspaceDTO.ProposalTitle = newWorkspace.ProposalTitle;
						newWorkspaceDTO.ProposalClass = new PickListDto { Id = newWorkspace.ProposalClass };
					}
					else
					{
						// RMS
						newWorkspaceDTO.ProjectMapType = newWorkspace.ProjectMapType;
						newWorkspaceDTO.AllowGridEdit = newWorkspace.AllowGridEdit;
					}

					// supply an initial output template id (from the default set)
					newWorkspaceDTO.TemplateID = (int)(newWorkspaceDTO.IsProjectMapWorkspace ?
						ExcelReportTemplateType.RMS_SIKORSKY_PROJECT_MAP : _ControllerLogic.GetDefaultReportTemplateType());

					newWorkspaceDTO.ResourceDecimalPrecision = newWorkspace.ResourceDecimalPrecision;
					newWorkspaceDTO.CostDecimalPrecision = newWorkspace.CostDecimalPrecision;

					_ControllerLogic.PopulateCompanySpecificWorkspaceProperties(newWorkspace, newWorkspaceDTO);
					if (newWorkspaceDTO.BOEExportSortByID == 0)
					{
						newWorkspaceDTO.BOEExportSortByID = (int)ExportSortBOEBy.WBS;
					}

					// if this workspace is being created by a copy, need to supply that workspace's perf org and resource list id so the entire list is copied
					// Copy the BOE Export Sort Order.
					FullWorkspace copiedFromWs = null;

					if (newWorkspace.WorkspaceToCopyID > 0)
					{
						copiedFromWs = this.Factory.CreateFullWorkspace(newWorkspace.WorkspaceToCopyID);

						newWorkspaceDTO.PerfOrgListID = copiedFromWs.PerfOrgListID;
						newWorkspaceDTO.ResourceListID = copiedFromWs.ResourceListID;
						newWorkspaceDTO.CustomFieldSorting = copiedFromWs.CustomFieldSorting;
						newWorkspaceDTO.ResourceSorting = copiedFromWs.ResourceSorting;
						newWorkspaceDTO.PerfOrgSorting = copiedFromWs.PerfOrgSorting;

						if (newWorkspace.WSExactCopy.HasValue && newWorkspace.WSExactCopy.Value)
						{
							newWorkspaceDTO.WorkspaceState = copiedFromWs.WorkspaceState;
						}

						newWorkspaceDTO.BOEExportSortByID = copiedFromWs.BOEExportSortByID;
					}

					// Set Project Map Workspaces to Working state - they can't be set to Initialization
					if (newWorkspaceDTO.IsProjectMapWorkspace)
					{
						newWorkspaceDTO.WorkspaceState = WorkspaceState.Working;
					}

					newWorkspaceID = this.workspaceLoader.SaveWorkspaceSettings(createdByUserDTO.UserID, newWorkspaceDTO);
					newWs = this.Factory.CreateFullWorkspace(newWorkspaceID);

					// Set the default rates/fees
					travelRatesFeesDataLoader.CopySystemDefaultFees(newWorkspaceID);
					travelRatesFeesDataLoader.CopySystemDefaultRates(newWorkspaceID);

					// Set the default Offload Rates
					if (newWorkspaceDTO.ProjectMapType != ProjectMapType.StandardWithoutOffload)
					{
						this.offloadRatesDTOLoader.CopySystemDefaultOffloadRates(newWorkspaceID);
					}

					// if non exact copy and the decimal precision has changed -> we need to recalculate
					if ((newWorkspace.WSExactCopy.HasValue && !newWorkspace.WSExactCopy.Value)
						&& ((newWs.DecimalPrecision != copiedFromWs.DecimalPrecision) || (newWs.CostDecimalPrecision != copiedFromWs.CostDecimalPrecision)))
					{
						decimalPrecisionChanged = newWs.ResourceDecimalPrecision != copiedFromWs.ResourceDecimalPrecision;
						costDecimalPrecisionChanged = newWs.CostDecimalPrecision != copiedFromWs.CostDecimalPrecision;
					}

					// save workspace admin for cost volume lead
					PermissionsDTO workspaceAdmin = new PermissionsDTO();
					workspaceAdmin.WorkspaceId = newWorkspaceID;
					workspaceAdmin.Role = Role.WorkspaceAdmin;
					workspaceAdmin.ETIUserId = costVolumeLeadPricerDTO.UserID;
					workspaceAdmin.Updateable = UpdateType.Upsert;
					this.PermissionsLoader.SavePermission(workspaceAdmin);

					// Save workspace admin for current user
					if (costVolumeLeadPricerDTO.UserID != createdByUserDTO.UserID)
					{
						PermissionsDTO userWorkspaceAdmin = new PermissionsDTO();
						userWorkspaceAdmin.WorkspaceId = newWorkspaceID;
						userWorkspaceAdmin.Role = Role.WorkspaceAdmin;
						userWorkspaceAdmin.ETIUserId = createdByUserDTO.UserID;
						userWorkspaceAdmin.Updateable = UpdateType.Upsert;
						this.PermissionsLoader.SavePermission(userWorkspaceAdmin);
					}

					// If a workspace was selected to copy, we'll process the activities necessary to do so
					if (newWorkspace.WorkspaceToCopyID > 0)
					{
						// Call the BL to copy the workspace
						finishedWithoutErrors = _WorkspaceCopier.CopyWorkspace(copiedFromWs, newWs, newWorkspace.BOEsToCopy, newWorkspace.CopyPermissions,
						newWorkspace.CopyTasks, newWorkspace.CopyLaborSpreads);
						Collection<WorkspaceExportFormatDTO> copiedTemplateTypes = _WorkspaceExportFormatDTOLoader.GetWorkspaceExportFormatsForWorkspace(newWorkspace.WorkspaceToCopyID);
						_WorkspaceExportFormatDTOLoader.InsertWorkspaceExportFormatsPickList(copiedTemplateTypes, newWorkspaceID);
					}
					else
					{
						// associate default output formats (landscape and portrait) to new workspace
						ICollection<ExcelReportTemplateType> picklistTemplateTypes = _ControllerLogic.GetPicklistReportTemplateTypes(newWs);
						foreach (ExcelReportTemplateType templateType in picklistTemplateTypes)
						{
							_WorkspaceExportFormatDTOLoader.InsertWorkspaceExportFormatPicklist(new Collection<int> { newWorkspaceID }, (int)templateType);
						}
						this.workspaceLoader.InsertDefaultMutliValues(newWorkspaceID);
					}

					// invoke the state machine to perform actions for this create                    
					_WorkspaceStateMachine.PerformStateTransitionAction(newWs, WorkspaceState.None, WorkspaceState.Initialization);
				}

				scope.Complete();

				toReturn = Json(new { Status = true });
			}

			#endregion

			if (decimalPrecisionChanged || costDecimalPrecisionChanged)
			{
				toReturn = this.HandleRecalculationAfterNonExactCopyChangedDecimalPrecision(newWorkspace, newWs, createdByUserDTO.UserID, finishedWithoutErrors, decimalPrecisionChanged, costDecimalPrecisionChanged, newWs.CostDecimalPrecision);
			}
			else if (!finishedWithoutErrors)
			{
				toReturn = Json(new { Error = WORKSPACE_COPY_ERROR });
			}

			// Clear permissions cache so the user's new permissions to the new workspace show up
			this.Factory.ClearPermissionsCache(this._securityInformation.ActiveUserNTID);

			FinalizeAction(_log, "SaveNewWorkspace", sw);
			return toReturn;
		}

		/// <summary>
		/// Creates the Sikorsky Custom Fields
		/// </summary>
		/// <param name="workspace">A workspace to create the fields inside.</param>
		/// <returns>Success or failure.</returns>
		public JsonResult CreateSikorskyCustomFields(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CREATE_SIKORSKY_CUSTOM_FIELDS, SecurityPage.BoeCustomFields, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this._ControllerLogic.CreateSikorskyCustomFields(ws.Id);

					scope.Complete();
				}
			}

			FinalizeAction(_log, WebConstants.ACTION_CREATE_SIKORSKY_CUSTOM_FIELDS, sw);
			return Json(new { Status = true });
		}

		/// <summary>
		/// Creates the Propricer Custom Fields
		/// </summary>
		/// <param name="workspace">A workspace to create the fields inside.</param>
		/// <returns>Success or failure.</returns>
		public JsonResult CreatePropricerCustomFields(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CREATE_PROPRICER_CUSTOM_FIELDS, SecurityPage.BoeCustomFields, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this._ControllerLogic.CreateProPricerCustomFields(ws.Id);

					scope.Complete();
				}
			}

			FinalizeAction(_log, WebConstants.ACTION_CREATE_PROPRICER_CUSTOM_FIELDS, sw);
			return Json(new { Status = true });
		}

		/// <summary>
		/// Does all of the recalculation if the non exact copy changed decimal precision between the original and the new precision
		/// </summary>
		/// <param name="workspaceCopyDetails">information about workspace copy</param>
		/// <param name="newWs">Newly Created Workspace</param>
		/// <param name="currentUserId">Current User Id</param>
		/// <param name="decimalPrecisionChanged">When true, resource hours decimal precision has changed.</param>
		/// <param name="costDecimalPrecisionChanged">When true, resource cost decimal precision has changed.</param>
		/// <param name="currentCostPrecision">Cost precision for the new workspace.</param>
		/// <returns>Json result -> true if success or error message if an issue happened</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private JsonResult HandleRecalculationAfterNonExactCopyChangedDecimalPrecision(ICreateWorkspaceModelView workspaceCopyDetails, FullWorkspace newWs, int currentUserId, bool finishedWithoutErrors, bool decimalPrecisionChanged, bool costDecimalPrecisionChanged, int currentCostPrecision)
		{
			JsonResult result = Json(new { Status = false });

			try
			{
				#region Setup things needed for recalculation and execute it; Do not save any data though, that will be done in the transaction

				// The timeout for the transaction needs to be longer, since the recalculation will save a lot more data..
				TimeSpan timeout = new TimeSpan(0, 10, 0);

				// Setup hashsets to keep track of data from recalculation; this will need to be saved in the transaction
				HashSet<BoeTaskElementDTO> tasksToSave = new HashSet<BoeTaskElementDTO>();
				HashSet<WorkspaceVariableDTO> workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
				HashSet<FullBoe> boesToTransition = new HashSet<FullBoe>();
				HashSet<BoeDTO> originalWsBoes = new HashSet<BoeDTO>(newWs.Boes.ToList<BoeDTO>().DeepClone());

				this._ControllerLogic.WsRecalculationStep1(newWs, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, originalWsBoes, decimalPrecisionChanged, costDecimalPrecisionChanged, currentCostPrecision);

				#endregion

				#region Save recalculated data

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = timeout }))
				{
					this.FullWsRecalc.SaveDataEffectedByRecalculation(newWs, tasksToSave, workspaceVariablesToSave, boesToTransition);

					#region If needed, send out emails, and do other cleanup, for Boes changed by recalculation

					this.FullWsRecalc.PerformStateTransitionActionsForBoesEffectedByRecalculation(newWs, boesToTransition, originalWsBoes);

					#endregion

					scope.Complete();
				}

				#endregion

				// this checks to see if there are any discrete spreads that the user needs to deal with manually
				string errorMessage = (finishedWithoutErrors ? string.Empty : errorMessage = WORKSPACE_COPY_ERROR + "<BR />")
						+ this.FullWsRecalc.GenerateMsgIfWsContainsTaskElementsWithNonZeroDeltaLabor(newWs);

				if (errorMessage.Length > 0)
				{
					result = Json(new { Error = errorMessage });
				}
				else
				{
					result = Json(new { Status = true });
				}
			}
			catch
			{
				// need to send a message about failure
				string errorMessage = "Decimal precision update has failed.  Workspace creation successfully completed.  Please adjust the decimal precision via workspace settings.";

				// Recalculation failed -> need to revert to the original precision
				ICollection<WorkspaceDTO> wsDataFromDb = this.workspaceLoader.GetByIds(new List<int>() { newWs.Id, workspaceCopyDetails.WorkspaceToCopyID });
				WorkspaceDTO wsToUpdate = wsDataFromDb.First(x => x.Id == newWs.Id);
				WorkspaceDTO originalWs = wsDataFromDb.First(x => x.Id == workspaceCopyDetails.WorkspaceToCopyID);

				wsToUpdate.ResourceDecimalPrecision = originalWs.ResourceDecimalPrecision;
				wsToUpdate.Updateable = UpdateType.Upsert;

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this.workspaceLoader.SaveWorkspaceSettings(currentUserId, wsToUpdate);
					scope.Complete();
				}

				// Clear the cache after since LastUpdateTime was changed
				this.Factory.ClearWorkspaceCache(wsToUpdate.Shortname);

				result = Json(new { Error = errorMessage });
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public JsonResult ValidateWorkspaceSearch(WorkspaceSearchModelView workspaceSearch)
		{

			Stopwatch sw = InitializeAction(_log, "ValidateWorkspaceSearch", SecurityPage.Home, SecurityAuthorization.Read, null, null);
			if (workspaceSearch == null)
			{
				throw new ArgumentNullException(nameof(workspaceSearch), "workspaceAllowSearchMV is null");
			}

			JsonResult toReturn = Json(new { Status = true });

			List<ValidationMessage> ValidationErrors = new List<ValidationMessage>();

			if (!ModelState.IsValid)
			{
				ValidationErrors.AddRange(Utilities.CreateModelStateValidationErrorList(ModelState));
			}
			else //Model ensures correct date format
			{
				//When data is entered into one of these fields a value is required in the other
				if ((workspaceSearch.ProposalStartDate != null && workspaceSearch.ProposalEndDate == null) ||
					  (workspaceSearch.ProposalStartDate == null && workspaceSearch.ProposalEndDate != null))
				{
					ValidationErrors.Add(new ValidationMessage("BothRequired",
						"When entering Proposal Submittal Date, both fields are required"));
				}

				//The second date must be greater than or equal to the first date.
				if (workspaceSearch.ProposalStartDate != null &&
					  workspaceSearch.ProposalEndDate != null &&
					  Convert.ToDateTime(workspaceSearch.ProposalStartDate) > Convert.ToDateTime(workspaceSearch.ProposalEndDate))
				{
					ValidationErrors.Add(new ValidationMessage("CorrectRange",
						"The second Proposal Submittal Date must be greater than or equal to the first date"));
				}
			}

			if (ValidationErrors.Count != 0)
			{
				toReturn = Json(new { Status = false });
				throw new GenValidationException(ValidationErrors);
			}
			FinalizeAction(_log, "ValidateWorkspaceSearch", sw);
			return toReturn;
		}

		public ViewResult PageWorkspaceSearchResults(WorkspaceSearchResultModelView workspaceSearchResults)
		{
			Stopwatch sw = InitializeAction(_log, "PageWorkspaceSearchResults", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			if (workspaceSearchResults == null)
			{
				throw new ArgumentNullException(nameof(workspaceSearchResults));
			}

			workspaceSearchResults.WorkspaceResults = new Collection<WorkspaceSearchResult>();
			ICollection<int> workspacesToGet = new Collection<int>();

			for (int i = workspaceSearchResults.StartArrayIndex; i <= workspaceSearchResults.EndArrayIndex; i++)
			{
				workspacesToGet.Add(workspaceSearchResults.PagedIndexes[i]);
			}

			List<WorkspaceDTO> theWorkspaces = this.workspaceLoader.GetByIds(workspacesToGet).ToList();

			for (int i = workspaceSearchResults.StartArrayIndex; i <= workspaceSearchResults.EndArrayIndex; i++)
			{
				WorkspaceDTO ws = (from x in theWorkspaces
								   where x.Id == workspaceSearchResults.PagedIndexes[i]
								   select x).FirstOrDefault();
				workspaceSearchResults.WorkspaceResults.Add(GetWorkspaceSearchResult(ws));
			}


			_ControllerLogic.PopulateCompanySpecificProperties(workspaceSearchResults);
			ViewResult toReturn = View(WebConstants.VIEW_HOME_WORKSPACE_SEARCH_RESULTS, workspaceSearchResults);

			FinalizeAction(_log, "PageWorkspaceSearchResults", sw);
			return toReturn;
		}

		public ViewResult PerformWorkspaceSearch(WorkspaceSearchModelView workspaceSearch)
		{
			Stopwatch sw = InitializeAction(_log, "PerformWorkspaceSearch", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			if (workspaceSearch == null)
			{
				throw new ArgumentNullException(nameof(workspaceSearch));
			}

			WorkspaceSearchResultModelView theModelView = null;
			if (ModelState.IsValid)
			{
				WorkspaceSearchDTO workspaceSearchDTO = new WorkspaceSearchDTO
				{
					WorkspaceName = workspaceSearch.WorkspaceProposalName,
					WorkspaceDesc = workspaceSearch.WorkspaceDescription,
					ProposalSubmitStartDate = !string.IsNullOrEmpty(workspaceSearch.ProposalStartDate) ? (DateTime?)Convert.ToDateTime(workspaceSearch.ProposalStartDate) : null,
					ProposalSubmitEndDate = !string.IsNullOrEmpty(workspaceSearch.ProposalEndDate) ? (DateTime?)Convert.ToDateTime(workspaceSearch.ProposalEndDate) : null,
					RFPNumber = workspaceSearch.RFPNumber,
					CostVolumeLead = workspaceSearch.CostLead,
					ProjectMapType = workspaceSearch.ProjectMapType
				};

				List<int> tempResults = _WorkspaceSearchDTOLoader.GetWorkspaceSearchResults(workspaceSearchDTO).ToList();

				theModelView = new WorkspaceSearchResultModelView
				{
					CurrentPage = 1
				};

				ICollection<WorkspaceDTO> theWorkspaces = this.workspaceLoader.GetByIds(tempResults);

				if (tempResults.Any())
				{
					foreach (WorkspaceDTO ws in theWorkspaces)
					{
						SecurityAuthorization authorization = CheckPermissions(SecurityPage.WorkspaceAdminPermissions, ws, null);
						if (authorization == SecurityAuthorization.None)
						{
							if (ws.AllowSearch && !ws.ContainsOCI && (ws.WorkspaceState == WorkspaceState.Complete))
							{
								theModelView.PagedIndexes.Add(ws.Id);
							}
						}
						else
						{
							if (ws.AllowSearch && !ws.ContainsOCI)
							{
								theModelView.PagedIndexes.Add(ws.Id);
							}
						}
					}

					if (theModelView.StartArrayIndex >= 0)
					{
						for (int i = theModelView.StartArrayIndex; i <= theModelView.EndArrayIndex; i++)
						{
							WorkspaceDTO ws = (from x in theWorkspaces
											   where x.Id == theModelView.PagedIndexes[i]
											   select x).FirstOrDefault();
							theModelView.WorkspaceResults.Add(GetWorkspaceSearchResult(ws));
						}
					}
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			_ControllerLogic.PopulateCompanySpecificProperties(theModelView);
			ViewResult toReturn = View(WebConstants.VIEW_HOME_WORKSPACE_SEARCH_RESULTS, theModelView);

			FinalizeAction(_log, "PerformWorkspaceSearch", sw);
			return toReturn;
		}

		public ActionResult GetDetailsForWorkspaceToCopy(int workspaceID)
		{
			Stopwatch sw = InitializeAction(_log, "GetDetailsForWorkspaceToCopy", SecurityPage.Home, SecurityAuthorization.Read, null, null);

			FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceID);

			JsonResult toReturn = Json(new { Status = false });

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				WorkspaceToCopyModelView modelView = new WorkspaceToCopyModelView
				{
					WorkspaceID = workspaceID,
					WorkspaceName = workspace.WorkspaceName,
					Description = workspace.Description,
					ShortName = workspace.Shortname,
					ContractStartDate = String.Format("{0:MM/yyyy}", workspace.ContractStartDate),
					ContractEndDate = String.Format("{0:MM/yyyy}", workspace.ContractEndDate),
					ProposalSubmittalDate = String.Format("{0:MM/dd/yyyy}", workspace.ProposalSubmittalDate),
					TrackingNumber = workspace.TrackingNumber,
					RFPNumber = workspace.RFPNumber,
					ContainsOCI = workspace.ContainsOCI,
					ContainsContentTemplates = workspace.ContainsTemplate,
					ShareAndAllowSearch = workspace.AllowSearch,
					ResourceDecimalPrecision = workspace.ResourceDecimalPrecision,
					CostDecimalPrecision = workspace.CostDecimalPrecision,
					IsUsingEquivalentPerson = workspace.IsUsingEquivalentPerson,
					IsUsingTM = workspace.IsUsingTM,
					ProjectMapType = (int)workspace.ProjectMapType,
					AllowGridEdit = workspace.AllowGridEdit,
					LineOfBusinessID = workspace.LineOfBusiness.Id,
					RteSizeLimit = workspace.RteSizeLimit,
					UsingTemplateBoe = workspace.UsingTemplateBOE,
					EnableSAPConnection = workspace.EnableSAPConnection,
					CurrentPTMWorkspace = workspace.CurrentPTMWorkspace,
					EnableAssignTaskAuthor = workspace.EnableAssignTaskAuthor,
				};

				toReturn = Json(modelView);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			FinalizeAction(_log, "GetDetailsForWorkspaceToCopy", sw);
			return toReturn;
		}

		public ViewResult GetBOEsForWorkspaceToCopy(int workspaceID)
		{
			Stopwatch sw = InitializeAction(_log, "GetBOEsForWorkspaceToCopy", SecurityPage.Home, SecurityAuthorization.Read, null, null);
			FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceID);

			Collection<WorkspaceToCopyBOEListModelView> modelViews = new Collection<WorkspaceToCopyBOEListModelView>();
			foreach (FullBoe boe in workspace.Boes)
			{
				modelViews.Add(new WorkspaceToCopyBOEListModelView
				{
					BOEID = boe.Id,
					BOETitle = boe.Title,
					WBS_NUM = boe.Wbs != null ? boe.Wbs.WbsNumber.ToString() : string.Empty,
					WBS = boe.Wbs != null ? boe.Wbs.WbsTitle : string.Empty,
					CLIN_NUM = boe.Clin != null ? boe.Clin.ClinNumber.ToString() : string.Empty,
					CLIN = boe.Clin != null ? boe.Clin.ClinTitle : string.Empty,
					Description = boe.Description
				});
			}

			ViewResult toReturn = View(WebConstants.VIEW_HOME_WORKSPACE_SEARCH_BOE_LIST, modelViews);

			FinalizeAction(_log, "GetBOEsForWorkspaceToCopy", sw);
			return toReturn;
		}

		/// <summary>
		/// SaveWorkspaceResourceRateTM
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="tmWorkspaceResourceRates"></param>
		/// <returns>JsonResult</returns>
		public JsonResult SaveWorkspaceResourceRateTM(string workspace, Collection<WorkspaceResourceRateTMModelView> tmWorkspaceResourceRates)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize
			Stopwatch sw = InitializeAction(_log, "SaveWorkspaceResourceRateTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (tmWorkspaceResourceRates == null)
			{
				throw new ArgumentNullException(nameof(tmWorkspaceResourceRates));
			}

			// Validate
			List<ValidationMessage> validationErrors = new List<ValidationMessage>();
			validationErrors.AddRange(Utilities.CreateModelStateValidationErrorList(ModelState));

			#region extravalidation

			foreach (WorkspaceResourceRateTMModelView tmResourceRate in tmWorkspaceResourceRates)
			{
				if (tmResourceRate.ToDelete == false)
				{
					// check for existing rate.
					if (!_ControllerLogic.IsWorkspaceResourceRateDateValidWithoutOverlapTM(ws, tmResourceRate.ResourceRateID, tmResourceRate.ResourceID, tmResourceRate.StartDate, tmResourceRate.EndDate))
					{
						validationErrors.Add(new ValidationMessage() { FieldName = "StartDate", ValidationIssue = "The proposed rate schedule period overlaps an existing rate schedule entry." });
						validationErrors.Add(new ValidationMessage() { FieldName = "EndDate", ValidationIssue = string.Empty });
					}
				}
			}

			#endregion extravalidation

			if (validationErrors.Any())
			{
				throw new GenValidationException(validationErrors);
			}

			// Perform Action
			JsonResult toReturn = Json(new { Status = false });
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				if (_ControllerLogic.SaveWorkspaceResourceRatesTM(ws, tmWorkspaceResourceRates))
				{
					toReturn = Json(new { Status = true });
					scope.Complete();
				}
				else
				{
					throw new GeneralAppException("An error occurred while saving the workspace resource rates.");
				}
			}

			// Finalize
			FinalizeAction(_log, "SaveWorkspaceResourceRateTM", sw);
			return toReturn;
		}

		/// <summary>
		/// Workspace Resource changed value container. This is to keep track of changes made to In Use Resources
		/// </summary>
		private struct InUseResourceChangedValueContainer
		{
			public ResourceDTO Resource { get; set; }
			public Collection<FieldChanged> Changes { get; set; }
		}

		#endregion AJAX Calls

		#region Imports/Exports


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WBSs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TaskElements"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BOEs"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		virtual public ContentResult ImportIMSArtemis(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportIMSArtemis", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			ICollection<int> IDs = ws.WbsElements.Select(x => x.Id).ToList();

			// If the files where uploaded successfully
			if (Request.Files.Count == 2 && (Request.Files[0].FileName.Length > 0 && Request.Files[1].FileName.Length > 0) && IDs.Count() == 0)
			{
				try
				{
					ArtemisImport newAtremisImporting = _ArtemisImporter.ImportFromCSVFiles(Request.Files[1].InputStream, Request.Files[0].InputStream, ws);

					if (newAtremisImporting.WBSs.Any() && newAtremisImporting.Messages.Count == 0)
					{
						// preload Boes, so we don't need to do this inside of a transaction
						ws.LoadBoes();

						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
						{
							IDictionary<int, int> SavedWBSIDs = this.wbsLoader.Save(newAtremisImporting.WBSs);

							foreach (BoeDTO BOE in newAtremisImporting.BOEs)
							{
								BOE.WBSID = SavedWBSIDs[BOE.WBSID.Value];
							}

							IDictionary<int, int> SavedBOEIDs = _BoeMediator.MediatedSaveBOEs(ws, newAtremisImporting.BOEs);

							foreach (BoeTaskElementDTO TaskElement in newAtremisImporting.BoeTaskElements)
							{
								TaskElement.BoeID = SavedBOEIDs[TaskElement.BoeID];
							}

							_BoeTaskElementMediator.MediatedSaveTaskElements(newAtremisImporting.BoeTaskElements, ws);

							scope.Complete();
						}

						// Return a success message
						toReturn = GenerateUploadResponse(true, "OK", "Import Complete");

					}
					else if (newAtremisImporting.WBSs.Count <= 0)
					{
						toReturn = GenerateUploadResponse(false, "No entries found, this does not appear to be a valid Artemis export file");
					}
					else if (newAtremisImporting.Messages.Count > 0)
					{
						toReturn = GenerateUploadResponse(false, newAtremisImporting.Messages, "Errors");
					}
				}
				// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
				// exception messages to display for the user
				catch (IncorrectColumnCountException)
				{
					toReturn = GenerateUploadResponse(false, "File does not contain the correct amount of data columns.");
				}
				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Artemis Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				if (IDs.Count() != 0)
				{
					toReturn = GenerateUploadResponse(false, "A Workspace must not have any WBSs/BOEs/Task Elements to be able to perform an IMS import.");
				}
				else
				{
					toReturn = GenerateUploadResponse(false, "Both files are required for import.");
				}
			}

			// Finalize Action
			FinalizeAction(_log, "ImportIMSArtemis", sw);
			return toReturn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ContentResult ImportIMSProject(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportIMSProject", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			// If the files where uploaded successfully
			if (Request.Files.Count == 1 && (Request.Files[0].FileName.Length > 0) && !ws.WbsElements.Any())
			{
				try
				{
					ProjectImport newProjectImporting = _ProjectImporter.ImportFromExcelFiles(Request.Files[0].InputStream, ws);

					if (newProjectImporting.WBSs.Any() && newProjectImporting.Messages.Count == 0)
					{

						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
						{
							//SaveClins getting back record of new clin ID mappings
							Dictionary<int, int> SavedClinsIDs = this.clinLoader.Save(newProjectImporting.CLINs);

							foreach (WbsDTO WBS in newProjectImporting.WBSs)
							{
								//because CLins can be existing we need to keep existing Clins in the list and update new ones with their new clin IDs from the mapping
								Collection<int> newClinIDs = new Collection<int>();
								foreach (int newClinID in WBS.ClinIDs)
								{
									if (newClinID > 0)
									{
										newClinIDs.Add(newClinID);
									}
									else
									{
										newClinIDs.Add(SavedClinsIDs[newClinID]);
									}
								}
								WBS.ClinIDs = newClinIDs;
							}

							//Save WBS get back a record of ID mappings to apply to the BOEs
							IDictionary<int, int> SavedWBSIDs = this.wbsLoader.Save(newProjectImporting.WBSs);

							Collection<BoeDTO> BOEDTOsToSave = new Collection<BoeDTO>((from ABOEDTO in newProjectImporting.BOEs select ABOEDTO.thisBOE).ToArray());
							foreach (BoeDTO BOE in BOEDTOsToSave)
							{
								//Apply new saved IDs to BOEs if less than 0
								BOE.WBSID = SavedWBSIDs[BOE.WBSID.Value];
								if (BOE.CLINID.HasValue && BOE.CLINID < 0)
								{
									BOE.CLINID = SavedClinsIDs[BOE.CLINID.Value];
								}
							}

							//Repeat same logic as before BOE ID mapping to Task
							IDictionary<int, int> SavedBOEIDs = _BoeMediator.MediatedSaveBOEs(ws, BOEDTOsToSave);

							foreach (BoeTaskElementDTO TaskElement in newProjectImporting.BoeTaskElements)
							{
								TaskElement.BoeID = SavedBOEIDs[TaskElement.BoeID];
							}

							_BoeTaskElementMediator.MediatedSaveTaskElements(newProjectImporting.BoeTaskElements, ws);

							scope.Complete();
						}

						// Return a success message
						toReturn = GenerateUploadResponse(true, "OK", "Import Complete");

					}
					else if (newProjectImporting.WBSs.Count <= 0)
					{
						toReturn = GenerateUploadResponse(false, "No entries found, this does not appear to be a valid Project export file");
					}
					else if (newProjectImporting.Messages.Count > 0)
					{
						toReturn = GenerateUploadResponse(false, newProjectImporting.Messages, "Errors");
					}
				}

				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Project Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				if (ws.WbsElements.Any())
				{
					toReturn = GenerateUploadResponse(false, "A Workspace must not have any WBSs/BOEs/Task Elements to be able to perform an IMS import.");
				}
				else
				{
					toReturn = GenerateUploadResponse(false, "Please specify a file for import.");
				}
			}

			// Finalize Action
			FinalizeAction(_log, "ImportIMSProject", sw);
			return toReturn;
		}

		/// <summary>
		/// Allows uploads of Excel files for importing new Resource elements
		/// </summary>
		/// <returns>A string indicating the result of the import operation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public ContentResult ImportBOECustomFieldResource(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportBOECustomFieldResource", SecurityPage.WorkspaceResource, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				try
				{
					int resourceListID = ws.ResourceListID;

					bool importLabor = Request.Form["ImportISGSLabor"] != null;
					bool importIWTA = Request.Form["ImportIWTA"] != null;
					bool importSub = Request.Form["ImportSub"] != null;
					bool importODC = Request.Form["ImportODC"] != null;
					bool importTravel = Request.Form["ImportTravel"] != null;
					bool importMaterials = Request.Form["ImportMaterials"] != null;

					// Call the business layer to parse the uploaded file
					// If the file was successfully parsed, add the results to the genBOE database
					Collection<ResourceDTO> resourcesFromImportFile = _ResourcesImporter.ImportFromExcelFile(Request.Files[0].InputStream, resourceListID);

					ICollection<ResourceDTO> newResources = new Collection<ResourceDTO>();

					if (importLabor)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor)).ToList();
					}
					if (importIWTA)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.IWTA)).ToList();
					}
					if (importSub)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Sub)).ToList();
					}
					if (importODC)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.ODC)).ToList();
					}
					if (importTravel)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Travel)).ToList();
					}
					if (importMaterials)
					{
						newResources = newResources.Concat(resourcesFromImportFile.Where(x => x.ElementOfCost == ElementOfCostType.Materials)).ToList();
					}

					if (resourcesFromImportFile.Any())
					{
						// Get the current resources for this workspace
						ICollection<ResourceDTO> allDatabaseResources = _ResourceLoader.GetByListId(resourceListID);

						List<ResourceDTO> inUseResouresCannotBeEditedOrDeleted = new List<ResourceDTO>();

						// Process Adds. Adds are defined as new resources with a negative resource ID
						List<ResourceDTO> addedResources = newResources.Where(x => x.Id < 0).ToList();

						// Set UpdateDate and Updateable on each added item
						foreach (ResourceDTO addedResource in addedResources)
						{
							addedResource.UpdateDate = DateTime.Now;
							addedResource.Updateable = UpdateType.Upsert;
						}

						// Process Deletes
						ICollection<ResourceDTO> availableToDelete = new Collection<ResourceDTO>();
						if (importLabor)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor)).ToList();
						}
						if (importIWTA)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA)).ToList();
						}
						if (importSub)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.Sub)).ToList();
						}
						if (importODC)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.ODC)).ToList();
						}
						if (importTravel)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.Travel)).ToList();
						}
						if (importMaterials)
						{
							availableToDelete = availableToDelete.Concat(allDatabaseResources.Where(x => x.ElementOfCost == ElementOfCostType.Materials)).ToList();
						}

						// get list of deleted resources. This query only pulls back rows that are in the database(availableToDelete) but not in the excel file(newResources)
						availableToDelete = (from a in availableToDelete
											 where !(from n in newResources
													 select n.Id).Contains(a.Id)
											 select a).ToList();

						// in use resources cannot be deleted so mark them as such and then remove them from the final availableToDelete list
						HashSet<int> resourceIDsInUse = _InUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);
						inUseResouresCannotBeEditedOrDeleted = availableToDelete.Where(x => resourceIDsInUse.Contains(x.Id) == true).ToList();
						availableToDelete = availableToDelete.Where(x => resourceIDsInUse.Contains(x.Id) == false).ToList();

						List<ResourceDTO> deletedResources = availableToDelete.ToList();

						// Set Updateable on each deleted item
						foreach (ResourceDTO deletedResource in deletedResources)
						{
							deletedResource.Updateable = UpdateType.Deleted;
						}


						// Process Edits

						Collection<ResourceDTO> editedResources = new Collection<ResourceDTO>();
						Collection<ResourceDTO> InUseEditedResources = new Collection<ResourceDTO>();
						Collection<ResourceDTO> InUseEditedResourcesButNotAllowedToEdit = new Collection<ResourceDTO>();

						// get edited resources that aren't currently in use. any resource field can be updated
						editedResources = new Collection<ResourceDTO>((
							from current in allDatabaseResources
							from changed in resourcesFromImportFile
							where current.Id == changed.Id && resourceIDsInUse.Contains(current.Id) == false &&
							(current.ResourceName != changed.ResourceName ||
								current.ResourceDesc != changed.ResourceDesc ||
								current.SegRegion != changed.SegRegion ||
								current.LaborType != changed.LaborType ||
								current.Segment != changed.Segment ||
								current.RateType != changed.RateType ||
								current.ElementOfCost != changed.ElementOfCost)
							select changed).ToArray());

						// get in used edited resources. the only valid fields that can change are desc, segmentregion, and labor type
						InUseEditedResources = new Collection<ResourceDTO>((
							from current in allDatabaseResources
							from changed in resourcesFromImportFile
							where current.Id == changed.Id && resourceIDsInUse.Contains(current.Id) == true &&
							(current.ResourceDesc != changed.ResourceDesc ||
								current.SegRegion != changed.SegRegion ||
								current.LaborType != changed.LaborType)
							select changed).ToArray());

						// get in used edited resources that should not allowed to be changed. fields that cannot change are resource Name, Segment, and ElementOfCost
						InUseEditedResourcesButNotAllowedToEdit = new Collection<ResourceDTO>((
						   from current in allDatabaseResources
						   from changed in resourcesFromImportFile
						   where current.Id == changed.Id && resourceIDsInUse.Contains(current.Id) == true &&
						   (current.ResourceName != changed.ResourceName ||
							   current.Segment != changed.Segment ||
							current.ElementOfCost != changed.ElementOfCost ||
							current.RateType != changed.RateType)
						   select changed).ToArray());


						// update in Use options with the resource that editing was attempted but will be denied
						inUseResouresCannotBeEditedOrDeleted.AddRange(InUseEditedResourcesButNotAllowedToEdit);

						List<ResourceDTO> ChangedResources = editedResources.Concat(InUseEditedResources).ToList();

						foreach (ResourceDTO resource in ChangedResources)
						{
							resource.Updateable = UpdateType.Upsert;
						}

						// Get original copies of the resources being changed to display before and after values on the UI
						var changedResourcesWithOriginals = from changedResource in ChangedResources
															from aResource in allDatabaseResources
															where changedResource.Id == aResource.Id
															select new
															{
																Old = aResource,
																New = changedResource
															};
						Collection<ResourceDTO> toSave = new Collection<ResourceDTO>(ChangedResources.Concat(deletedResources).Concat(addedResources).ToArray());

						if (toSave.Any())
						{
							IDictionary<int, int> resourceSaveResultsDictionary = null;
							using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
							{
								// Save all three lists
								resourceSaveResultsDictionary = _ResourceLoader.SaveWorkspaceResources(ws, toSave);
								scope.Complete();
							}

							ProcessResourceIdUpdates(ws, resourceSaveResultsDictionary, deletedResources.Select(r => r.Id).ToList());
						}


						// Create the results data to pass back to the UI
						var importResultsData = new
						{
							AddedOptions = addedResources.Select(x => x.Id),
							ChangedOptions = changedResourcesWithOriginals,
							DeletedOptions = deletedResources,
							InUseOptions = inUseResouresCannotBeEditedOrDeleted
						};

						// Return a success message
						toReturn = GenerateUploadResponse(true, importResultsData, "Import Complete");

					}
					else
					{
						// Create the results data to pass back to the UI
						var importResultsData = new
						{
							AddedOptions = new Collection<ResourceDTO>(),
							ChangedOptions = new Collection<ResourceDTO>(),
							DeletedOptions = new Collection<ResourceDTO>(),
							InUseOptions = new Collection<ResourceDTO>()
						};

						// Return a success message
						toReturn = GenerateUploadResponse(true, importResultsData, "No Resources Were Changed");

					}
				}
				// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
				}
				catch (ColumnMissingException ex2)
				{
					toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'ID', 'Description', 'Segment/Region', 'Labor Type', 'Rate Type' and 'Element of Cost' columns. The following columns are missing: {0}.", ex2.Message);
				}
				catch (CellValueMissingException ex3)
				{
					toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for ID, Description, Segment/Region, Labor Type, Rate Type or Element of Cost. Every filled row must have a value for each. Check the following column: {0}.", ex3.Message);
				}
				catch (DuplicateValuesException ex4)
				{
					toReturn = GenerateUploadResponse(false, "Values must be unique. The following are not unique: {0}", ex4.Message);
				}
				catch (EntityCommandExecutionException)
				{
					toReturn = GenerateUploadResponse(false, "The Default Resources List was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
				}
				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Resources Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = GenerateUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			FinalizeAction(_log, "ImportBOECustomFieldResource", sw);
			return toReturn;
		}

		/// <summary>
		/// Exports Resources to a pre-formatted MS Excel template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Excel template.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportBOECustomFieldResource(string workspace, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials, string searchText)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportBOECustomFieldResource", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Perform Action
			// if the search text came in as null, then no string was inputted so set to empty string not null. this saves the url from being /""
			if (searchText == null)
			{
				searchText = string.Empty;
			}
			// Get Resources for this Workspace
			BOECustomFieldResourceGridModelView filteredResourcesMV = this._GetAndFilterCustomFieldResources(workspace, searchText, showLabor, showIWTA, showSub, showODC, showTravel, showMaterials);
			ICollection<ResourceDTO> resources = _ResourceLoader.GetByIds(filteredResourcesMV.PagedIndexes);

			// Get Resources template file name
			string templateFileName = Server.MapPath("~/Templates/Export/Resources.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = ResourcesExporter.ExportToExcelFile(templateFileName, resources, _CommonDataMapper);

			string fileName = string.Format("{0}_Resources.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportBOECustomFieldResource", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Export BOE Custom Field Resource Template
		/// </summary>
		/// <param name="showLabor"></param>
		/// <param name="showIWTA"></param>
		/// <param name="showSub"></param>
		/// <param name="showODC"></param>
		/// <param name="showTravel"></param>
		/// <param name="showMaterials"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportBOECustomFieldResourceTemplate(string workspace, string searchText)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportBOECustomFieldResourceTemplate", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Perform Action

			// if the search text came in as null, then no string was inputted so set to empty string not null. this saves the url from being /""
			if (searchText == null)
			{
				searchText = string.Empty;
			}


			// Get Resources template file name
			string templateFileName = Server.MapPath("~/Templates/Export/Resources.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = ResourcesExporter.ExportTemplate(templateFileName, _CommonDataMapper);

			string fileName = string.Format("{0}_Resources.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportBOECustomFieldResourceTemplate", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public ActionResult ExportWorkspaceResourceRatesTM(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportWorkspaceResourceRatesTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Get T&M Resource Rates template file name
			string templateFileName = Server.MapPath("~/Templates/Export/TMResourceRates.xlsx");

			ICollection<TMResourceRateDTO> Rates = this.tmResourceRateLoader.GetByWorkspaceId(ws.Id);

			ICollection<ResourceDTO> workspaceResources = _ControllerLogic.GetFilteredWorkspaceResourcesTM(ws.ResourcesForWsResourceListId);

			ICollection<ResourceDTO> otherWorkspaceResources = new Collection<ResourceDTO>();

			if (Rates.Count() > 0)
			{
				otherWorkspaceResources = _ControllerLogic.GetFilteredWorkspaceResourcesTM(ws.ResourcesForWsResourceListId);
			}

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = TMResourceRatesExporter.ExportToExcelFile(templateFileName, Rates.ToList<ResourceRateDTO>(), workspaceResources, otherWorkspaceResources);

			string fileName = string.Format("TM_{0}_ResourceRates.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportWorkspaceResourceRatesTM", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public ActionResult ExportBlankWorkspaceResourceRatesTM(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportBlankWorkspaceResourceRatesTM", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Get T&M Resource Rates template file name
			string templateFileName = Server.MapPath("~/Templates/Export/TMResourceRates.xlsx");

			ICollection<ResourceDTO> workspaceResources = (from x in ws.ResourcesForWsResourceListId
														   where
															 (x.ElementOfCost == ElementOfCostType.Sub ||
															  x.ElementOfCost == ElementOfCostType.IWTA) &&
															 (x.RateType == RateType.Hours)
														   select x).ToList();

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = TMResourceRatesExporter.ExportToExcelFile(templateFileName, new List<ResourceRateDTO>(), workspaceResources, new List<ResourceDTO>());

			string fileName = string.Format("TM_{0}_ResourceRates.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportBlankWorkspaceResourceRatesTM", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Allows uploads of Excel files for importing new Resource elements
		/// </summary>
		/// <returns>A string indicating the result of the import operation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ContentResult ImportBOECustomFieldPerfOrg(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportBOECustomFieldPerfOrg", SecurityPage.WorkspacePerfOrg, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				try
				{
					int performingOrgListID = ws.PerfOrgListID;

					// Call the business layer to parse the uploaded file
					// If the file was successfully parsed, add the results to the genBOE database
					Collection<PerformingOrgDTO> newPerformingOrgs =
						_PerformingOrgImporter.ImportFromExcelFile(Request.Files[0].InputStream);

					if (newPerformingOrgs.Count > 0)
					{
						// Get the current perf orgs for this workspace
						IReadOnlyCollection<PerformingOrgDTO> currentPerformingOrgs = ws.PerformingOrgsForWsList;

						HashSet<int> perfOrgIdsInUse =
							_InUseDataLoader.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(performingOrgListID);

						// Get workspace perf org that aren't being used 
						ICollection<PerformingOrgDTO> unusedPerformingOrgs = (from c in currentPerformingOrgs
																			  where !perfOrgIdsInUse.Contains(c.Id)
																			  select c).ToCollection();

						// Process Adds
						ICollection<PerformingOrgDTO> addedPerformingOrgs =
							newPerformingOrgs.Except(currentPerformingOrgs,
									new KeyEqualityComparer<PerformingOrgDTO>(x => x.PerformingOrgName.Trim()
										.ToLower()))
								.ToCollection();

						// Validate Adds
						ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();
						foreach (PerformingOrgDTO perfOrg in addedPerformingOrgs)
						{
							if (perfOrg.PerformingOrgName.Length > 20)
							{
								validationMessages.Add(new ValidationMessage(nameof(perfOrg.PerformingOrgName), perfOrg.PerformingOrgName));
							}

							if (perfOrg.PerformingOrgDesc.Length > Constants.PERF_ORG_DESC_MAX_LENGTH)
							{
								validationMessages.Add(new ValidationMessage(nameof(perfOrg.PerformingOrgDesc), perfOrg.PerformingOrgDesc));
							}
						}

						// Throw any validation messages
						if (validationMessages.Any())
						{
							throw new GenValidationException(validationMessages);
						}

						// Set UpdateDate and Updateable on each added item
						foreach (PerformingOrgDTO addedPerformingOrg in addedPerformingOrgs)
						{
							addedPerformingOrg.UpdateDate = DateTime.Now;
							addedPerformingOrg.Updateable = UpdateType.Upsert;
						}


						// Process Changes
						ICollection<PerformingOrgDTO> changedPerformingOrgs =
						(from newPerformingOrg in newPerformingOrgs
						 from unusedPerformingOrg in unusedPerformingOrgs
						 where newPerformingOrg.PerformingOrgName.Equals(unusedPerformingOrg.PerformingOrgName,
								   StringComparison.CurrentCultureIgnoreCase) &&
							   newPerformingOrg.PerformingOrgDesc != unusedPerformingOrg.PerformingOrgDesc
						 select new PerformingOrgDTO
						 {

							 Id = unusedPerformingOrg.Id,
							 PerformingOrgName = unusedPerformingOrg.PerformingOrgName,
							 PerformingOrgDesc = newPerformingOrg.PerformingOrgDesc,
							 UpdateDate = unusedPerformingOrg.UpdateDate,
							 Updateable = UpdateType.Upsert
						 }).ToCollection();

						// Get original copies of the performing orgs being changed to display before and after values on the UI
						var changedPerformingOrgsWithOriginals = from changedPerformingOrg in changedPerformingOrgs
																 from unusedPerformingOrg in unusedPerformingOrgs
																 where changedPerformingOrg.PerformingOrgName.Equals(unusedPerformingOrg.PerformingOrgName,
																	 StringComparison.CurrentCultureIgnoreCase)
																 select new
																 {
																	 Old = unusedPerformingOrg,
																	 New = changedPerformingOrg
																 };

						// Process Deletes
						ICollection<PerformingOrgDTO> deletedPerformingOrgs =
							unusedPerformingOrgs.Except(newPerformingOrgs,
									new KeyEqualityComparer<PerformingOrgDTO>(x => x.PerformingOrgName.Trim()
										.ToLower()))
								.ToCollection();


						// Set Updateable on each deleted item
						foreach (PerformingOrgDTO deletedPerformingOrg in deletedPerformingOrgs)
						{
							deletedPerformingOrg.Updateable = UpdateType.Deleted;
						}

						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
							new TransactionOptions
							{
								IsolationLevel = IsolationLevel.Snapshot,
								Timeout = new TimeSpan(0, 0,
									ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout",
										Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
							}))
						{
							// Save all three lists
							this.perfOrgLoader.SaveWorkspacePerformingOrgs(
								new Collection<PerformingOrgDTO>(addedPerformingOrgs.Concat(changedPerformingOrgs)
									.Concat(deletedPerformingOrgs).ToArray()), ws.PerfOrgListID);
							scope.Complete();
						}

						// Create the results data to pass back to the UI
						var importResultsData = new
						{
							AddedOptions = addedPerformingOrgs,
							ChangedOptions = changedPerformingOrgsWithOriginals,
							DeletedOptions = deletedPerformingOrgs,
							InUseOptions = currentPerformingOrgs.Except(unusedPerformingOrgs,
								new KeyEqualityComparer<PerformingOrgDTO>(x => x.PerformingOrgName.Trim().ToLower()))
						};

						// Return a success message
						toReturn = GenerateUploadResponse(true, importResultsData, "Import Complete");
					}
				}
				// Catch custom exceptions from ExcelImporter and PerformingOrgImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = GenerateUploadResponse(false,
						"File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
				}
				catch (ColumnMissingException ex2)
				{
					toReturn = GenerateUploadResponse(false,
						"File does not contain all of the required columns. File must contain 'ID' and 'Description' columns. The following columns are missing: {0}.",
						ex2.Message);
				}
				catch (CellValueMissingException ex3)
				{
					toReturn = GenerateUploadResponse(false,
						"A row in the file does not contain a value for ID, Description or both. Each row must have an ID and Description. Check the following column: {0}.",
						ex3.Message);
				}
				catch (DuplicateValuesException ex4)
				{
					toReturn = GenerateUploadResponse(false,
						"IDs must be unique. The following IDs are not unique: {0}", ex4.Message);
				}
				catch (EntityCommandExecutionException)
				{
					toReturn = GenerateUploadResponse(false,
						"The Performing Organizations List was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
				}
				catch (GenValidationException ex5)
				{
					toReturn = GenerateUploadResponse(false, this.generateValidationString(ex5.ValidationList));
				}
				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Performing Orgs Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = GenerateUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			FinalizeAction(_log, "ImportBOECustomFieldPerfOrg", sw);
			return toReturn;
		}

		/// <summary>
		/// Exports Resources to a pre-formatted MS Excel template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Excel template.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportBOECustomFieldPerfOrg(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportBOECustomFieldPerfOrg", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// Perform Action

			// Get data to export for this Workspace
			Collection<PerformingOrgDTO> performingOrgs = ws.PerformingOrgsForWsList.ToCollection();

			// Get export template file name
			string templateFileName = Server.MapPath("~/Templates/Export/PerformingOrgs.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = PerformingOrgsExporter.ExportToExcelFile(templateFileName, performingOrgs);

			string fileName = string.Format("{0}_PerformingOrgs.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportBOECustomFieldPerfOrg", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Allows uploads of Excel files for importing new Resource elements
		/// </summary>
		/// <returns>A string indicating the result of the import operation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ContentResult ImportBOECustomField(string workspace, int customFieldID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportBOECustomField", SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			ContentResult toReturn = null;

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				try
				{
					// Call the business layer to parse the uploaded file
					// If the file was successfully parsed, add the results to the genBOE database
					Collection<CustomFieldValueDTO> newCustomFieldValues = _CustomFieldImporter.ImportFromExcelFile(Request.Files[0].InputStream, customFieldID);

					if (newCustomFieldValues.Any())
					{
						// refresh the inuse field first
						_CustomFieldValueLoader.RefreshCustomFieldInUseByWorkspaceID(ws.Id);

						// Get the custom field values for this custom field.
						ICollection<CustomFieldValueDTO> currentCustomFieldValues = _CustomFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldID(customFieldID);

						IEnumerable<CustomFieldValueDTO> unusedCustomFieldValues = currentCustomFieldValues.Where(customFieldValue => !customFieldValue.CustomFieldValueInUseFlag);

						// Process Adds
						IEnumerable<CustomFieldValueDTO> addedCustomFieldValues = newCustomFieldValues.Except(currentCustomFieldValues, new KeyEqualityComparer<CustomFieldValueDTO>(x => x.CustomFieldValueName.Trim().ToLower()));

						// Set UpdateDate and Updateable on each added item
						foreach (CustomFieldValueDTO addedPerformingOrg in addedCustomFieldValues)
						{
							addedPerformingOrg.UpdateDate = DateTime.Now;
							addedPerformingOrg.Updateable = UpdateType.Upsert;
						}

						// Process Changes
						IEnumerable<CustomFieldValueDTO> changedCustomFieldValues = from newCustomFieldValue in newCustomFieldValues
																					from unusedCustomFieldValue in unusedCustomFieldValues
																					where newCustomFieldValue.CustomFieldValueName.Equals(unusedCustomFieldValue.CustomFieldValueName, StringComparison.CurrentCultureIgnoreCase) &&
																						  newCustomFieldValue.CustomFieldValueName != unusedCustomFieldValue.CustomFieldValueDescription
																					select new CustomFieldValueDTO
																					{
																						Id = unusedCustomFieldValue.CustomFieldValueID,
																						CustomFieldID = unusedCustomFieldValue.CustomFieldID,
																						CustomFieldValueID = unusedCustomFieldValue.CustomFieldValueID,
																						CustomFieldValueName = unusedCustomFieldValue.CustomFieldValueName,
																						CustomFieldValueDescription = newCustomFieldValue.CustomFieldValueDescription,
																						CustomFieldValueInUseFlag = unusedCustomFieldValue.CustomFieldValueInUseFlag,
																						UpdateDate = unusedCustomFieldValue.UpdateDate,
																						Updateable = UpdateType.Upsert
																					};

						// Get original copies of the performing orgs being changed to display before and after values on the UI
						var changedCustomFieldValuesWithOriginals = from changedCustomFieldValue in changedCustomFieldValues
																	from unusedCustomFieldValue in unusedCustomFieldValues
																	where changedCustomFieldValue.CustomFieldValueName.Equals(unusedCustomFieldValue.CustomFieldValueName, StringComparison.CurrentCultureIgnoreCase)
																	select new
																	{
																		Old = unusedCustomFieldValue,
																		New = changedCustomFieldValue
																	};

						// Process Deletes
						IEnumerable<CustomFieldValueDTO> deletedCustomFieldValues = unusedCustomFieldValues.Except(newCustomFieldValues, new KeyEqualityComparer<CustomFieldValueDTO>(x => x.CustomFieldValueName.Trim().ToLower()));

						// Set Updateable on each deleted item
						foreach (CustomFieldValueDTO deletedCustomFieldValue in deletedCustomFieldValues)
						{
							deletedCustomFieldValue.Updateable = UpdateType.Deleted;
						}

						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
						{
							// Save all three lists
							_CustomFieldValueLoader.Save(new Collection<CustomFieldValueDTO>(addedCustomFieldValues.Concat(changedCustomFieldValues).Concat(deletedCustomFieldValues).ToArray()));
							scope.Complete();
						}

						// Create the results data to pass back to the UI
						var importResultsData = new
						{
							AddedOptions = addedCustomFieldValues,
							ChangedOptions = changedCustomFieldValuesWithOriginals,
							DeletedOptions = deletedCustomFieldValues,
							InUseOptions = currentCustomFieldValues.Except(unusedCustomFieldValues, new KeyEqualityComparer<CustomFieldValueDTO>(x => x.CustomFieldValueName.Trim().ToLower()))
						};

						// Return a success message
						toReturn = GenerateUploadResponse(true, importResultsData, "Import Complete");
					}
				}
				// Catch custom exceptions from ExcelImporter and CustomFieldImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).");
				}
				catch (ColumnMissingException ex2)
				{
					toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. File must contain 'ID' and 'Description' columns. The following columns are missing: {0}.", ex2.Message);
				}
				catch (CellValueMissingException ex3)
				{
					toReturn = GenerateUploadResponse(false, "A row in the file does not contain a value for ID, Description or both. Each row must have an ID and Description. Check the following column: {0}.", ex3.Message);
				}
				catch (DuplicateValuesException ex4)
				{
					toReturn = GenerateUploadResponse(false, "IDs must be unique. The following IDs are not unique: {0}", ex4.Message);
				}
				catch (EntityCommandExecutionException)
				{
					toReturn = GenerateUploadResponse(false, "The Custom Field List was recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.");
				}
				catch (Exception ex)
				{
					_log.Error(ex, "Unknown Import Custom Field Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = GenerateUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			FinalizeAction(_log, "ImportBOECustomField", sw);
			return toReturn;
		}

		/// <summary>
		/// Exports Custom Field values to a pre-formatted MS Excel template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Excel template.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportBOECustomField(string workspace, int customFieldID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportBOECustomField", SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, ws, null);

			// refresh in use flag
			_CustomFieldValueLoader.RefreshCustomFieldInUseByWorkspaceID(ws.Id);

			string customFieldName = "BOE_Custom_Field";
			ICollection<CustomFieldValueDTO> customFieldValues = new Collection<CustomFieldValueDTO>();

			if (customFieldID > 0)
			{
				// Get the custom field name
				customFieldName = this.Factory.CreateCustomField(customFieldID).CustomFieldName;

				// Get the custom field values for this custom field.
				customFieldValues = _CustomFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldID(customFieldID);
			}

			// Get export template file name
			string templateFileName = Server.MapPath("~/Templates/Export/PerformingOrgs.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = CustomFieldExporter.ExportToExcelFile(templateFileName, customFieldValues);

			string fileName = string.Format("{0}_{1}.xlsx", ws.WorkspaceName, customFieldName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportBOECustomField", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		#endregion Imports/Exports

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Verify custom field level has been selected
		/// </summary>
		/// <param name="meta">Custom Fields Grid MV</param>
		/// <returns>True if level has been selected, otherwise false</returns>
		private bool CustomFieldLevelSelected(BOECustomFieldsGridModelView meta)
		{
			bool toReturn = false;

			if (meta.CustomFieldDisplayID == CustomFieldType.BoeDisplay ||
				meta.CustomFieldDisplayID == CustomFieldType.TaskDisplay ||
				meta.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay ||
				meta.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay)
			{
				toReturn = true;
			}

			return toReturn;
		}

		private BOECustomFieldResourceGridModelView _GetAndFilterCustomFieldResources(string workspace, string searchText, bool showLabor, bool showIWTA, bool showSub, bool showODC, bool showTravel, bool showMaterials)
		{
			if (searchText == null)
			{
				throw new ArgumentNullException(nameof(searchText));
			}

			BOECustomFieldResourceGridModelView theModelView = new BOECustomFieldResourceGridModelView();
			theModelView.ResourceResults = new Collection<BOECustomFieldResourceModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			List<ResourceDTO> filteredResources = new List<ResourceDTO>();
			if (showLabor)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor));
			}
			if (showIWTA)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.IWTA));
			}
			if (showSub)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Sub));
			}
			if (showODC)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.ODC));
			}
			if (showTravel)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel));
			}
			if (showMaterials)
			{
				filteredResources.AddRange(ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Materials));
			}

			IDictionary<int, ElementOfCostTypeModelView> elementOfCostValues = _CommonDataMapper.GetElementOfCostTypesDictionary();
			string searchTextLower = searchText.ToLower().Trim();
			HashSet<int> resourceIDsInUse = _InUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);
			filteredResources = filteredResources.OrderBy(x => x.ResourceDesc).ToList();
			foreach (ResourceDTO resource in filteredResources)
			{
				string rateTypeName = resource.RateType.GetDescription();
				string elementOfCostName = elementOfCostValues[(int)resource.ElementOfCost].ElementOfCostName;

				if (string.IsNullOrEmpty(elementOfCostName))
				{
					elementOfCostName = string.Empty;
				}

				if (resource.ResourceName.ToLower().Contains(searchTextLower) ||
					resource.ResourceDesc.ToLower().Contains(searchTextLower) ||
					resource.SegRegion.ToLower().Contains(searchTextLower) ||
					resource.LaborType.ToLower().Contains(searchTextLower) ||
					rateTypeName.ToLower().Contains(searchTextLower) ||
					elementOfCostName.ToLower().Contains(searchTextLower))
				{
					theModelView.PagedIndexes.Add(resource.Id);
					if (theModelView.ResourceResults.Count < theModelView.ResultsPerPage)
					{

						BOECustomFieldResourceModelView mv = new BOECustomFieldResourceModelView(resource, rateTypeName, elementOfCostName);
						mv.InUse = resourceIDsInUse.Contains(mv.CustomFieldOptionID);

						theModelView.ResourceResults.Add(mv);
					}
				}
			}

			if (ws.ResourceSorting == CustomFieldSorting.ID)
			{
				theModelView.ResourceResults = theModelView.ResourceResults.OrderBy(x => x.ID).ToCollection();
			}
			else
			{
				theModelView.ResourceResults = theModelView.ResourceResults.OrderBy(x => x.Description).ToCollection();
			}

			theModelView.TotalResults = filteredResources.Count;
			return theModelView;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private HomeWorkspaceGridModelView _GetHomeWorkspaceGridData(FullWorkspace workspace)
		{
			HomeWorkspaceGridModelView theModelView = new HomeWorkspaceGridModelView() { };

			#region Load Data from the DB

			UserDTO currentUser = workspace.CurrentActiveUser;
			theModelView.CurrentUserDisplayName = currentUser.DisplayName;
			HashSet<PermissionsDTO> workspacePermissions = new HashSet<PermissionsDTO>(this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id));

			bool isUserPotentialBOESubcontractorAuthor = (from p in workspacePermissions
														  where p.ETIUserId == currentUser.UserID && p.Role == Role.SubcontractorAuthor
														  select p).Any();

			List<int> boeIds = workspace.Boes.Select(x => x.Id).ToList();
			HashSet<PermissionsDTO> rolesForBoes = new HashSet<PermissionsDTO>(this.PermissionsLoader.GetBOEPermissions(boeIds));
			HashSet<BoeApproverResponseDTO> allBoeApprovals = new HashSet<BoeApproverResponseDTO>(this._IBoeApproverResponseDTODataLoader.GetByBoeIds(boeIds));

			HashSet<UserDTO> allUsersForBoes = new HashSet<UserDTO>(this.UserLoader.GetByIds(rolesForBoes.Select(x => x.ETIUserId).Distinct().ToList()));

			#endregion

			foreach (BoeDTO data in workspace.Boes)
			{
				HashSet<PermissionsDTO> boeRoles = new HashSet<PermissionsDTO>(rolesForBoes.Where(x => x.BOEId == data.Id).Where(x => x.BOEId.HasValue).ToCollection());
				HashSet<BoeApproverResponseDTO> boeApprovals = new HashSet<BoeApproverResponseDTO>(allBoeApprovals.Where(x => x.BoeID == data.Id).ToCollection());

				// Subcontractors view of a Workspace Home is filtered to show only those BOEs assigned to the subcontractor
				// Notes:
				//  Subcontractors will be limited to the "Subcontractor Author" permission.
				//  When you go to the Manage BOEs page you can add/update BOEs.  Their status can be unassigned (no author or approver), Draft, Awaiting Approval or Approved.
				//  Subcontractors should only see those BOEs (on the workspace homepage) they are assigned to and they should only ever be assigned the "Subcontractor Author" role.
				if (isUserPotentialBOESubcontractorAuthor &&
					!(from b in boeRoles
					  where b.ETIUserId == currentUser.UserID && b.Role == Role.SubcontractorAuthor
					  select b).Any())
				{
					continue;   // Skip this BOE -- Subcontractor is not assigned the "Subcontractor Author" role
				}

				WbsDTO wbsDTO = workspace.WbsElements.FirstOrDefault(x => x.Id == data.WBSID);
				ClinDTO clinDTO = workspace.Clins.FirstOrDefault(x => x.Id == data.CLINID);

				// get the list of Authors
				HashSet<int> authorIds = new HashSet<int>(boeRoles.Where(x => x.Role == Role.Author).Select(x => x.ETIUserId).Distinct().ToList());
				ICollection<UserDTO> distinctAuthors = allUsersForBoes.Where(x => authorIds.Contains(x.UserID)).ToList();
				List<string> authorNames = distinctAuthors.Select(x => x.DisplayName).ToList();

				// get the list of Subcontractors and append the string (Sub) to the end
				HashSet<int> subcontractorAuthorIds = new HashSet<int>(boeRoles.Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x.ETIUserId).Distinct().ToList());
				ICollection<UserDTO> distinctSubcontractorAuthors = allUsersForBoes.Where(x => subcontractorAuthorIds.Contains(x.UserID)).ToList();
				List<string> subcontractorNames = distinctSubcontractorAuthors.Select(x => x.DisplayName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX).ToList();

				// get the list of approvers
				HashSet<int> approverIds = new HashSet<int>(boeRoles.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).Distinct().ToList());
				ICollection<UserDTO> distinctApprovers = allUsersForBoes.Where(x => approverIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).ToList();

				// combine the Authors and Subcontractor Authors lists and order it
				ICollection<string> allAuthorNames = new Collection<string>((subcontractorNames.Union(authorNames).OrderBy(x => x).Distinct().ToList()));
				ICollection<HomeWorkspaceGridApproverModelView> allApprovers = new Collection<HomeWorkspaceGridApproverModelView>();

				foreach (UserDTO approver in distinctApprovers)
				{
					BoeApproverResponseDTO response = boeApprovals.FirstOrDefault(x => x.ETIUserID == approver.UserID);
					if (response != null)
					{
						allApprovers.Add(new HomeWorkspaceGridApproverModelView(response.ApproverResponse, response.ApproverResponded, approver.DisplayName));
					}
				}

				// add the data to our model
				theModelView.items.Add(new HomeWorkspaceModelView(data, wbsDTO, clinDTO, allAuthorNames, allApprovers));
			}

			return theModelView;
		}

		/// <summary>
		/// Returns a <see cref="WorkspaceSearchResult"/> for the passed in workspaceID or null if the criteria is not met.
		/// Criteria: user has admin and WS is in any state OR user doesn't have admin and WS is in complete state
		/// </summary>
		/// <param name="workspaceID">The <see cref="WorkspaceDTO"/> ID</param>
		/// <returns>a <see cref="WorkspaceSearchResult"/></returns>
		private WorkspaceSearchResult GetWorkspaceSearchResult(WorkspaceDTO workspace)
		{
			if (!workspace.AllowSearch || workspace.ContainsOCI)
			{
				throw new InvalidDataRelationException(
					"Searching for a workspace that cannot be searched. workspace: " + workspace.Id);
			}

			UserDTO costLead = this.UserLoader.GetUserByID(workspace.CostVolumeLeadPricerUserID);

			return new WorkspaceSearchResult()
			{
				WorkspaceID = workspace.Id,
				WorkspaceName = workspace.WorkspaceName,
				SubmittalDate = workspace.ProposalSubmittalDate,
				ContractStartDate = workspace.ContractStartDate,
				ContractEndDate = workspace.ContractEndDate,
				CostVolumeLeadPricer = costLead.DisplayName,
				Description = workspace.Description,
				ProjectMapType = workspace.ProjectMapType,
				IsProjectMapWorkspace = workspace.IsProjectMapWorkspace
			};
		}

		private string ConvertToWorkspaceResourceRateOptionList(IDictionary<string, Tuple<bool, int>> values)
		{
			StringBuilder optionsBuilder = new StringBuilder();
			optionsBuilder.Append("<option value=\"\"></option>");

			foreach (KeyValuePair<string, Tuple<bool, int>> value in values)
			{
				optionsBuilder.Append("<option value=\"");
				optionsBuilder.Append(Server.HtmlEncode(value.Value.Item2.ToString()));
				optionsBuilder.Append("\" mappable=\"");
				optionsBuilder.Append(Server.HtmlEncode(value.Value.Item1.ToString()));
				optionsBuilder.Append("\">");
				optionsBuilder.Append(Server.HtmlEncode(value.Key));
				optionsBuilder.Append("</option>");
			}

			return optionsBuilder.ToString();
		}

		/// <summary>
		/// Processes T&amp;M Rate updates when Resources are updated.
		/// </summary>
		/// <param name="ws">The workspace to use when updating T&amp;M Rates.</param>
		/// <param name="resourceSaveResultsDictionary">A dictionary keyed by old Resource Id to the new Resource Id after Resources are saved.</param>
		/// <param name="removedIds">A list of the Resource Ids that were deleted.</param>
		private void ProcessResourceIdUpdates(FullWorkspace ws, IDictionary<int, int> resourceSaveResultsDictionary, ICollection<int> deletedIds)
		{
			// Update T&M Rates based on changes 
			if (ws.IsUsingTM)
			{
				// Get list of T&M resources for this workspace 
				ICollection<TMResourceRateDTO> workspaceTMResourceRates = this.tmResourceRateLoader.GetByWorkspaceId(ws.Id);
				if (workspaceTMResourceRates.Any())
				{
					Collection<TMResourceRateDTO> tmResourceRatesUpdated = new Collection<TMResourceRateDTO>();
					foreach (TMResourceRateDTO workspaceTMResourceRate in workspaceTMResourceRates)
					{
						// delete any T&M Resource Rates that are associated with deleted Resources
						if (deletedIds.Contains(workspaceTMResourceRate.ResourceID))
						{
							workspaceTMResourceRate.Updateable = UpdateType.Deleted;
							tmResourceRatesUpdated.Add(workspaceTMResourceRate);
						}
						else if (resourceSaveResultsDictionary.ContainsKey(workspaceTMResourceRate.ResourceID))
						{
							// update any T&M Resource Rates that are associated with updated Resources
							workspaceTMResourceRate.ResourceID =
								resourceSaveResultsDictionary[workspaceTMResourceRate.ResourceID];
							workspaceTMResourceRate.Updateable = UpdateType.Upsert;
							tmResourceRatesUpdated.Add(workspaceTMResourceRate);
						}
					}

					if (tmResourceRatesUpdated.Any())
					{
						// Save the updates
						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
						{
							this.tmResourceRateLoader.SaveTMResourceRates(tmResourceRatesUpdated);
							scope.Complete();
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of Workspace Dtos that the user has access to
		/// </summary>
		/// <param name="isSystemAdmin">Is User a System Admin</param>
		/// <returns>A collection of Workspace Dtos that the user has access to</returns>
		private Collection<WorkspaceDTO> GetWorkspaceNameInfoToWhichTheUserHasAccessTo(bool isSystemAdmin, int? workspaceStatus)
		{
			Collection<WorkspaceDTO> workspaceDtos = null;

			if (isSystemAdmin)
			{
				// system admins can see all workspaces..
				workspaceDtos = this.workspaceLoader.GetAllWsNamesAndTrackingNumberInfo();
			}
			else
			{
				// get current user
				UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

				// get AD groups that the user belongs to
				ICollection<GroupData> usersGroups = _ADUtils.GetGroupsForUser(currentUser.NTID);

				// figure out which of those groups exist in the DB -> get their ETI User Ids
				List<string> userData = new List<string>();
				foreach (GroupData aGroup in usersGroups)
				{
					if (!userData.Contains(aGroup.Ntid))
					{
						userData.Add(aGroup.Ntid);
					}
				}

				ICollection<int> userIds = this.UserLoader.GetIdsByNtid(userData);
				userIds.Add(currentUser.UserID);

				// Based on the ETI User Ids (the current users & any relevant AD groups), get a list of WS IDs
				Collection<int> wsIdsThatUserHasAccessTo = this.PermissionsLoader.GetWorkspaceIdsThatUsersHaveAccessTo(userIds);

				// Get corresponding, NOT deleted, workspaces
				workspaceDtos = this.workspaceLoader.GetByIds(wsIdsThatUserHasAccessTo).Where(x => !x.HasBeenDeleted).ToCollection();
			}

			if (workspaceStatus.HasValue)
			{
				Collection<WorkspaceState> allowedWorkspaceStates = new Collection<WorkspaceState>();

				if (workspaceStatus == -1) // all active
				{
					allowedWorkspaceStates.Add(WorkspaceState.Initialization);
					allowedWorkspaceStates.Add(WorkspaceState.Working);
					allowedWorkspaceStates.Add(WorkspaceState.Locked);
				}
				else
				{
					allowedWorkspaceStates.Add((WorkspaceState)workspaceStatus.Value);
				}

				workspaceDtos = workspaceDtos.Where(x => allowedWorkspaceStates.Contains(x.WorkspaceState)).ToCollection();
			}

			return workspaceDtos;
		}

		/// <summary>
		/// Generates the error message for validation errors for IDs and Descriptions being too long
		/// </summary>
		/// <param name="ValidationList">List of validation errors</param>
		/// <returns>Formatted string for validation errors</returns>
		private string generateValidationString(List<ValidationMessage> ValidationList)
		{
			string idMessage = "IDs must be 20 characters or less. The following IDs are too long: ";
			string descriptionMessage = $"Descriptions must be {Constants.PERF_ORG_DESC_MAX_LENGTH} characters or less. The following descriptions are too long: ";

			bool idErrors = false;
			bool descErrors = false;

			foreach (ValidationMessage validationMessage in ValidationList)
			{
				if (validationMessage.FieldName == nameof(PerformingOrgDTO.PerformingOrgName))
				{
					if (idErrors)
					{
						// append comma if second or later ID
						idMessage += ", ";
					}

					idErrors = true;
					idMessage += validationMessage.ValidationIssue;
				}
				else if (validationMessage.FieldName == nameof(PerformingOrgDTO.PerformingOrgDesc))
				{
					if (descErrors)
					{
						// append comma if second or later description
						descriptionMessage += ", ";
					}

					descErrors = true;
					descriptionMessage += validationMessage.ValidationIssue;
				}
			}

			return (idErrors ? idMessage : string.Empty) + // Only show ID message if there were any IDs that were too long
				   (idErrors && descErrors ? "<br />" : string.Empty) + // Only add line break if there are both ID and Description errors
				   (descErrors ? descriptionMessage : string.Empty); // Only show Description message if there were any IDs that were too long
		}

		#endregion Private Methods
	}

	internal class KeyEqualityComparer<T> : IEqualityComparer<T>
	{
		private readonly Func<T, object> keyExtractor;

		public KeyEqualityComparer(Func<T, object> keyExtractor)
		{
			this.keyExtractor = keyExtractor;
		}

		public bool Equals(T x, T y)
		{
			return this.keyExtractor(x).Equals(this.keyExtractor(y));
		}

		public int GetHashCode(T obj)
		{
			return this.keyExtractor(obj).GetHashCode();
		}
	}

	public class SelectListItemWithTitle : SelectListItem
	{
		public string Title { get; set; }
	}


	public static class LineOfBusinessHelper
	{
		private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "sac", "Sikorsky" },
				{ "iwss", "Integrated Warfare Systems and Sensors" },
				{ "tls", "Training and Logistics Solutions" },
				{ "new ventures", "new ventures"},
				{ "c6isr", "c6isr" },
				{ "cyber ships and advanced technologies", "Cyber, Ships & Advanced Technologies" },
				{ "cyber, ships & advanced technologies","cyber, ships & advanced technologies" }
			};

		public static string Resolve(string raw, IEnumerable<PickListDto> pickList)
		{
			if (string.IsNullOrWhiteSpace(raw))
			{
				return raw;
			}
						
			if (_aliases.TryGetValue(raw.Trim(), out string mapped))
			{
				raw = mapped;
			}
						
			PickListDto match = pickList.FirstOrDefault(p =>
				string.Equals(p.Text, raw, StringComparison.OrdinalIgnoreCase));

			return match?.Text ?? raw;
		}
	}
}
