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
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Transactions;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.MOQ;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IESSAPClient;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;
	using MoreLinq;

	public class BOELaborController : GenBOEController
	{
		#region Private Fields

		private readonly Logger _log = new Logger(typeof(BOELaborController));
		private readonly ResourceDTODataLoader _ResourceDTODataLoader = null;
		private readonly IVariableSelectBOEtoSumCalculation _VariableSelectBOEtoSumCalculation = null;
		private readonly VariableCircularReferenceChecker _VariableCircularReferenceChecker = null;
		private readonly BoeTaskElementRecalculation _BoeTaskElementRecalculation = null;
		private readonly LaborTypeAndSpreadImporter _LaborTypeAndSpreadImporter = null;
		private readonly IBOELaborControllerLogic _BoeLaborControllerLogic = null;
		private readonly ActionLogic.CopyBOE.BOECopier _BOECopier = null;
		private readonly IBoeTaskElementDTODataLoader taskElementDataLoader = null;
		private readonly IPerformingOrgDTODataLoader perfOrgLoader;
		private readonly IFullWorkspaceRecalculation fullWsRecalc;
		private readonly IMSTMetricLoader _MSTMetricsLoader;
		private const string TEMPLATE_FOLDER = "~/Templates/Export/";
		private const string SYSTEM_OFFLOAD_RATES_EXPORT_TEMPLATE = "OffloadRatesRMS.xlsx";
		private readonly IOffloadRatesDTOLoader offloadRatesLoader;
		private readonly IRteTemplateDataLoader rteTemplateDataLoader;
		private readonly IMoqTableExporter moqTableExporter;

		/// <summary>
		/// Starting date for MOQ Templates. WS created after this date will be using new MOQ Types.
		/// </summary>
		private readonly DateTime moqTemplateUsageStartDate = DateTime.Parse(ConfigurationUtilities.GetAppSetting("MoqTemplateStartDate"));

		#endregion Private Fields

		/// <summary>
		/// Task Element Validation Class
		/// </summary>
		private TaskElementValidation taskElementValidation { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public BOELaborController(ISecurityAccess inSecurityAccess,
			CommonDataMapper inCommonDataMapper, SiteMasterUtilities inSiteMasterUtilities, ResourceDTODataLoader inresourceDTOLoader,
			IVariableSelectBOEtoSumCalculation inVariableSelectBOEToSum,
			VariableCircularReferenceChecker inVariableCircularReferenceChecker,
			BoeTaskElementRecalculation inBoeTaskElementRecalculation,
			LaborTypeAndSpreadImporter inLaborTypeAndSpreadImporter,
			IBOELaborControllerLogic inBoeLaborControllerLogic,
			IPermissionsDTODataLoader inPermissionsLoader,
			SystemMetrics inSystemMetrics,
			IUserDTODataLoader inUserLoader,
			ActionLogic.CopyBOE.BOECopier inBOECopier,
			IFullObjectFactory factory,
			IBoeTaskElementDTODataLoader taskElementDataLoader,
			IPerformingOrgDTODataLoader perfOrgLoader,
			IGenBOEControllerLogic inControllerLogic,
			IFullWorkspaceRecalculation fullWsRecalc,
			TaskElementValidation taskElementValidation,
			IMSTMetricLoader inMSTMetricsLoader,
			IOffloadRatesDTOLoader offloadRatesDTOLoader,
			IRteTemplateDataLoader rteTemplateDataLoader,
			IMoqTableExporter moqTableExporter)
			: base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserLoader, inPermissionsLoader, inControllerLogic)
		{
			this._CommonDataMapper = inCommonDataMapper;
			this._ResourceDTODataLoader = inresourceDTOLoader;
			this._VariableSelectBOEtoSumCalculation = inVariableSelectBOEToSum;
			this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
			this._BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
			this._LaborTypeAndSpreadImporter = inLaborTypeAndSpreadImporter;
			this._BoeLaborControllerLogic = inBoeLaborControllerLogic;
			this._BOECopier = inBOECopier;
			this.taskElementDataLoader = taskElementDataLoader;
			this.perfOrgLoader = perfOrgLoader;
			this.fullWsRecalc = fullWsRecalc;
			this.taskElementValidation = taskElementValidation;
			this._MSTMetricsLoader = inMSTMetricsLoader;
			this.offloadRatesLoader = offloadRatesDTOLoader;
			this.rteTemplateDataLoader = rteTemplateDataLoader;
			this.moqTableExporter = moqTableExporter;
		}

		#region Display

		public virtual ViewResult DisplayTask(string workspace, int boeID, int? taskElementID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);
			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_DISPLAY_TASK_ELEMENT, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			// Perform Action            
			ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;
			ViewData["BOEID"] = boeID;
			bool containsDiscrete = false;
			bool isReadOnly = bool.Parse((string)this.ViewData["READONLY"]);

			//check if skill mix is enabled and workspace starts after skill mix date 
			bool enableSkillMix = false;
			if (Utilities.IsSkillMixEnabledForSystem && Utilities.ShowSkillMixForWorkspace(ws.CreationDate))
			{
				enableSkillMix = true;
			}
			ViewData["EnableSkillMix"] = enableSkillMix;
			//create a var for list items
			Collection<SelectListItem> orderOfResourceTypes = new Collection<SelectListItem>();
			string taskDescription = string.Empty;
			if (taskElementID.HasValue)
			{
				ViewData["TASKID"] = taskElementID.Value;
				// Because of current workflow, no need to convert to Summary Task Element here since the summarized data is not used
				BoeTaskElementDTO element = this.Factory.CreateTaskElement(taskElementID.Value, ws.DecimalPrecision, ws.CostDecimalPrecision);
				DataRelationshipVerifier.VerifyDataRelation(element, boeID);
				taskDescription = element.Description;
				containsDiscrete = element.taskElementLabors.Any(x => x.SpreadCurveID == SpreadCurves.DiscreteHours || x.SpreadCurveID == SpreadCurves.DiscreteCost);

				if (!isReadOnly)
				{
					HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this._ResourceDTODataLoader.GetByIds(element.taskElementLabors.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
					HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(element.taskElementLabors.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

					//get a list of each task element.
					foreach (ResourceTypeDto row in element.taskElementLabors)
					{
						ResourceDTO resource = new ResourceDTO();
						if (row.ResourceID != null)
						{
							resource = resourcesFromDb.First(x => x.Id == row.ResourceID.Value);
						}

						PerformingOrgDTO perfOrg = new PerformingOrgDTO();
						if (row.PerformingOrgID != null)
						{
							perfOrg = performingOrgsFromDb.First(x => x.Id == row.PerformingOrgID.Value);
						}

						orderOfResourceTypes.Add(new SelectListItem { Text = resource.ResourceDesc + " " + perfOrg.PerformingOrgDesc, Value = row.Id.ToString() });
					}
				}
			}

			SecurityAuthorization taskDateShiftAuthorization = this.CheckPermissions(SecurityPage.BoeTaskDates, ws, boeID);
			ICollection<RTECustomTemplateQuestionAnswerModelView> rteAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, boeID, taskElementID);

			LaborTaskModelView modelView = new LaborTaskModelView
			{
				ContainsOci = ws.ContainsOCI,
				MOQEquationLabel = this._BoeLaborControllerLogic.GetMOQEquationLabel(),
				CostDecimalPrecision = ws.CostDecimalPrecision,
				ResourceDecimalPrecision = ws.DecimalPrecision,
				IsOffloadWorkspace = ws.ProjectMapType == ProjectMapType.StandardWithOffload,
				BOEIsMulti = boe.IsMultiClinWbs,
				BOEState = (int)boe.State,
				AllowDateShift = (taskDateShiftAuthorization == SecurityAuthorization.CreateReadUpdateDelete),
				HoursLabel = FullObjectHelper.HoursLabel(ws),
				BoeId = boeID,
				LaborTypeWarning = false, // TODO
				TaskElementId = taskElementID,
				ContainsDiscrete = containsDiscrete,
				DescriptionTemplateAnswers = rteAnswers.Where(t => t.SourceId == (int)RteTemplateSource.TaskDescription).ToList(),
				TaskDescription = taskDescription,
				UsingTemplateBOE = ws.UsingTemplateBOE,
				EnableSAPConnection = ws.EnableSAPConnection
			};

			this._BoeLaborControllerLogic.GetMetricSearchDialogParameters(modelView);
			
			bool missingBRCs = false;
			if (Utilities.IsBRCEnabledForWorkspace(workspace) && boe.EndDate >= Utilities.OneLmxStartDate)
			{
				//check if ws contains BRCs, if not, mark tasks as read only
				ICollection<ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(ws.ResourcesForWsResourceListId.ToList(), true, workspace);
				//if no brcs in ws, display warning
				if (resources.Count == 0)
				{
					missingBRCs = true;
				}
			}

			ViewData["Order_Of_ResourceTypes"] = orderOfResourceTypes;
			string viewName = isReadOnly || (ws.WorkspaceState != WorkspaceState.Working && !(ws.WorkspaceState == WorkspaceState.Locked && boe.State == BOEState.Draft)) || missingBRCs
				? WebConstants.VIEW_LABOR_TASK_STATIC
				: WebConstants.VIEW_LABOR_TASK;

			ViewResult toReturn = this.View(viewName, modelView);

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_DISPLAY_TASK_ELEMENT, sw);
			return toReturn;
		}

		virtual public ActionResult ValidateResource(string workspace, string searchTerm)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			ResourceDTO resource = _ResourceDTODataLoader.GetByNameAndListId(searchTerm, ws.ResourceListID);

			return Json(resource);

		}

		virtual public ActionResult ValidateResourceByID(string workspace, int inResourceID)
		{
			ResourceDTO resource = _ResourceDTODataLoader.GetById(inResourceID);

			return Json(new { Status = resource.ResourceDesc });

		}

		virtual public ActionResult ValidatePerformingOrgs(string workspace, string searchTerm)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			PerformingOrgDTO perfOrg = this.perfOrgLoader.GetByListIdAndName(ws.PerfOrgListID, searchTerm);
			int? perfOrgID = perfOrg != null ? (int?)perfOrg.Id : null;

			return Json(new { Status = perfOrgID.Value });

		}


		virtual public ActionResult GetPerformingOrgIDByName(string workspace, int boeID, string Name)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			PerformingOrgDTO perfOrg = this.perfOrgLoader.GetByListIdAndName(ws.PerfOrgListID, Name);
			int? perfOrgId = perfOrg != null ? (int?)perfOrg.Id : null; ;

			return Json(new { Status = perfOrgId });
		}

		/// <summary>
		/// Loads the Labor Curve View
		/// </summary>
		/// <returns>Labor Curve View</returns>
		virtual public ViewResult DisplayLaborCurves()
		{
			return View();
		}

		/// <summary>
		/// Loads the Labor Resources View
		/// </summary>
		/// <returns>Labor Resources View</returns>
		public virtual ViewResult DisplayResources(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			ICollection<ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(ws.ResourcesForWsResourceListId.ToList(), false, workspace).ToList();

			ICollection<BOECustomFieldResourceModelView> theModelViews = new Collection<BOECustomFieldResourceModelView>();
			IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = this._CommonDataMapper.GetElementOfCostTypesDictionary();

			foreach (ResourceDTO resource in resources)
			{
				string ElementOfCostDisplay = allElementOfCostTypes[(int)resource.ElementOfCost].ElementOfCostName;
				string rateTypeDisplay = resource.RateType.GetDescription();

				BOECustomFieldResourceModelView mv = new BOECustomFieldResourceModelView(resource, rateTypeDisplay, ElementOfCostDisplay);

				theModelViews.Add(mv);
			}

			return View(theModelViews);
		}

		/// <summary>
		/// Loads the Labor Business Resource Codes View
		/// </summary>
		/// <returns>Labor Business Resource Codes View</returns>
		public virtual ViewResult DisplayBusinessResourceCodes(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			ICollection<ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(ws.ResourcesForWsResourceListId.ToList(), true, workspace).ToList();

			ICollection<BOECustomFieldResourceModelView> theModelViews = new Collection<BOECustomFieldResourceModelView>();
			IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = this._CommonDataMapper.GetElementOfCostTypesDictionary();

			foreach (ResourceDTO resource in resources)
			{
				string ElementOfCostDisplay = allElementOfCostTypes[(int)resource.ElementOfCost].ElementOfCostName;
				string rateTypeDisplay = resource.RateType.GetDescription();

				BOECustomFieldResourceModelView mv = new BOECustomFieldResourceModelView(resource, rateTypeDisplay, ElementOfCostDisplay);

				theModelViews.Add(mv);
			}

			return View(theModelViews);
		}

		/// <summary>
		/// Loads the Labor Perf Orgs View
		/// </summary>
		/// <returns>Labor Perf Orgs View</returns>
		public virtual ViewResult DisplayPerfOrgs(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			ICollection<BOECustomFieldOptionModelView> theModelViews = new List<BOECustomFieldOptionModelView>();
			foreach (PerformingOrgDTO performingOrg in ws.PerformingOrgsForWsList)
			{
				BOECustomFieldOptionModelView mv = new BOECustomFieldOptionModelView(performingOrg);

				theModelViews.Add(mv);
			}
			return View(theModelViews);
		}

		/// <summary>
		/// Displays the dialog that allows the user to create a variable as the sum of BOE totals,
		/// with the BOEs sorted by WBS for the user to choose from.
		/// </summary>
		/// <param name="workspace">Current workspace</param>
		/// <returns>The view</returns>
		public ViewResult DisplayVariableBOESumByWBS(string workspace, int? boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayVariableBOESumByWBS", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			ViewData["SumVariableResourceTypes"] = _CommonDataMapper.GetSumVariableResourceTypes();
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

			// Return the View using the generated ModelViews
			ViewResult toReturn = View(WebConstants.VIEW_VARIABLE_BOE_SUM_BY_WBS,
								DisplayVariableBOESumByWBSModelViews(
									workspace,
									boeID));

			// Finalize Action
			FinalizeAction(_log, "DisplayVariableBOESumByWBS", sw);

			return toReturn;
		}

		/// <summary>
		/// Gets an updated listing of WBS and BOE Sums based on a list of resource types
		/// </summary>
		/// <param name="workspace">Workspace containing the current BOE</param>
		/// <param name="boeID">Optional BOE ID</param>
		/// <param name="resourceTypes">Resource types to use for calculation</param>
		/// <returns>Updated list of sums</returns>
		public JsonResult RefreshVariableBOESumByWBS(string workspace, ICollection<SumVariableResourceType> resourceTypes)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "RefreshVariableBOESumByWBS", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			// Return the View using the generated ModelViews
			JsonResult toReturn = Json(RefreshVariableBOESumByWBSModelViews(ws, resourceTypes));

			// Finalize Action
			FinalizeAction(_log, "RefreshVariableBOESumByWBS", sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the results of the search for historical metrics
		/// </summary>
		/// <param name="workspace">Current workspace</param>
		/// <returns>The view</returns>
		public ViewResult DisplayHistoricalMetricsResults(int validatedOption, string searchTerm)
		{
			Stopwatch sw = InitializeAction(_log, "DisplayHistoricalMetricsResults", SecurityPage.HistoricalMetricSearch, SecurityAuthorization.Read, null, null);

			ViewResultData viewResultData = _BoeLaborControllerLogic.GetHistoricalMetricsResults(validatedOption, searchTerm);
			ViewResult toReturn = View(viewResultData.ViewName, viewResultData.Model);

			// Finalize Action
			FinalizeAction(_log, "DisplayHistoricalMetricsResults", sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the results of the search for historical metrics from MST PMM database.
		/// </summary>
		/// <param name="modelView">MetricsSearchDialogParametersModelView detailing search criteria.</param>
		/// <returns>The populated view result.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public ViewResult DisplayHistoricalMetricsResultsForMST(MetricsSearchDialogParametersModelView modelView)
		{
			if (modelView == null)
			{
				throw new ArgumentNullException(nameof(modelView));
			}
			Stopwatch sw = InitializeAction(_log, "DisplayHistoricalMetricsResultsForMST", SecurityPage.HistoricalMetricSearch, SecurityAuthorization.Read, null, null);

			ICollection<MSTMetricDetailsDTO> results = new Collection<MSTMetricDetailsDTO>();
			MSTMetricSearchDTO searchDto = new MSTMetricSearchDTO()
			{
				DataSourceId = modelView.SelectedDataSourceId,
				MeasureFunctionId = modelView.SelectedMeasureFunctionId,
				MeasureId = modelView.SelectedMeasureNameId,
				ProgramId = modelView.SelectedProgramId,
				SearchFor = modelView.SearchFor,
				MeasureQualifierId = modelView.SelectedMeasureQualifierId
			};
			results = _MSTMetricsLoader.SearchMSTMetrics(searchDto);
			HistoricalMetricsFromMSTModelView resultsModelView = new HistoricalMetricsFromMSTModelView(results);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_HISTORICAL_METRICS_SEARCH_RESULTS_MST, resultsModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayHistoricalMetricsResultsForMST", sw);

			return toReturn;
		}

		/// <summary>
		/// Displays the metric details of the selected metric for IS&GS
		/// </summary>
		/// <param name="workspace">Current workspace</param>
		/// <returns>The view</returns>
		public ViewResult DisplayHistoricalMetricsDetails(int metricId, bool getFromSource)
		{
			Stopwatch sw = InitializeAction(_log, "DisplayHistoricalMetricsDetails", SecurityPage.HistoricalMetricSearch, SecurityAuthorization.Read, null, null);
			ViewResultData viewResultData;
			if (getFromSource)
			{
				viewResultData = _BoeLaborControllerLogic.GetHistoricalMetricsDetailsFromSource(metricId);
			}
			else
			{
				viewResultData = _BoeLaborControllerLogic.GetHistoricalMetricsDetails(metricId);
			}
			ViewResult toReturn = View(viewResultData.ViewName, viewResultData.Model);

			// Finalize Action
			FinalizeAction(_log, "DisplayHistoricalMetricsDetails", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the dialog that allows the user to create a variable as the sum of BOE totals,
		/// with the BOEs sorted by CLIN for the user to choose from.
		/// </summary>
		/// <param name="workspace">Current workspace</param>
		/// <returns>The view</returns>
		public ViewResult DisplayVariableBOESumByCLIN(string workspace, int? boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayVariableBOESumByCLIN", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			ViewData["SumVariableResourceTypes"] = _CommonDataMapper.GetSumVariableResourceTypes();
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

			// Return the View using the generated ModelViews
			ViewResult toReturn = View(WebConstants.VIEW_VARIABLE_BOE_SUM_BY_CLIN, DisplayVariableBOESumByCLINModelViews(ws, boeID));

			// Finalize Action
			FinalizeAction(_log, "DisplayVariableBOESumByCLIN", sw);

			return toReturn;
		}

		/// <summary>
		/// Gets an updated listing of CLINs and BOE Sums based on a list of resource types
		/// </summary>
		/// <param name="workspace">Workspace containing the current BOE</param>
		/// <param name="boeID">Optional BOE ID</param>
		/// <param name="resourceTypes">Resource types to use for calculation</param>
		/// <returns>Updated list of sums</returns>
		public JsonResult RefreshVariableBOESumByCLIN(string workspace, int? boeID, ICollection<SumVariableResourceType> resourceTypes)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "RefreshVariableBOESumByCLIN", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, boeID);

			// Return the View using the generated ModelViews
			JsonResult toReturn = Json(RefreshVariableBOESumByCLINModelViews(ws, resourceTypes));

			// Finalize Action
			FinalizeAction(_log, "RefreshVariableBOESumByCLIN", sw);

			return toReturn;
		}



		/// <summary>
		/// Displays data for the boe to sum dialog
		/// </summary>
		/// <param name="workspace">Workspace containing the current BOE</param>
		/// <param name="boeID">Optional BOE ID</param>
		/// <returns>model views</returns>
		private ICollection<VariableBOESumByCLINModelView> DisplayVariableBOESumByCLINModelViews(FullWorkspace workspace, int? boeID)
		{
			Collection<VariableBOESumByCLINModelView> toReturn = new Collection<VariableBOESumByCLINModelView>();

			// Get all CLINs in the workspace
			IReadOnlyCollection<FullClin> CLINs = workspace.ClinsNoMultiClin;

			// Get all BOEs in the workspace
			ICollection<FullBoe> BOEs = workspace.Boes.Where(b => !b.IsMultiClinWbs).ToCollection();

			// Associate each BOE with its CLIN
			var boesForCLINs = (from c in CLINs
								from b in BOEs
								where b.CLINID.HasValue &&
									  b.CLINID == c.Id
								select new
								{
									BOE = b,
									CLIN = c
								}).ToList();
			IDictionary<int, BOEStateModelView> boeStates = this._CommonDataMapper.getBOEStatesDictionary();

			// Iterate over each CLIN in the workspace
			foreach (ClinDTO CLIN in CLINs)
			{
				// Create a ModelView to represent the summary CLIN row in the View's table
				VariableBOESumByCLINModelView modelView = new VariableBOESumByCLINModelView
				{
					CLINID = CLIN.Id,
					CLINTitle = CLIN.ClinString,
					WorkspaceDecimalPrecision = workspace.DecimalPrecision
				};

				// Get all BOEs for the current CLIN
				var boesForCLIN = (from b in boesForCLINs
								   where b.CLIN.Id == CLIN.Id
								   select b).ToList();

				// Iterate over each BOE for the current CLIN
				foreach (var boeForCLIN in boesForCLIN)
				{
					FullBoe boeObjectForClin = FullWorkspaceHelper.GetBoeById(workspace, boeForCLIN.BOE.Id);

					// Create a ModelView to represent the nested BOE in the View's table
					VariableBOESumBOEElement nestedModelView = new VariableBOESumBOEElement
					{
						BOEID = boeForCLIN.BOE.Id,
						BOEStatus = boeStates[(int)boeForCLIN.BOE.State].BOEState,
						WBSTitle = boeForCLIN.BOE.WBSID.HasValue ? workspace.WbsElements.First(i => i.Id == boeForCLIN.BOE.WBSID).WbsString : "None",
						Disabled = boeID.HasValue && _VariableCircularReferenceChecker.BOECreatesCircularReference(null, boeID.Value, boeObjectForClin, workspace)
					};

					// Add the nested BOE ModelView to the CLIN ModelView
					modelView.BOEs.Add(nestedModelView);
				}

				// Add the summary CLIN ModelView to the collection that will be passed to the View
				toReturn.Add(modelView);
			}

			return toReturn;
		}

		public ViewResult DisplayMOQHoursEquationField(string workspace, int boeID, int taskElementID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			bool overrideReadOnly = _BoeLaborControllerLogic.OverrideReadOnly(ws, boe);

			ViewData["ShouldMoqReadOnlyBeReversed"] = overrideReadOnly;

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayMOQEquationField", SecurityPage.MOQEquationField, SecurityAuthorization.Read, ws, boeID);

			MOQEquationModelView theModelView = CreateMOQModelView(ws, boe, taskElementID);
			ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;
			ViewData["EnableSAP"] = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			ViewData["SAPWorkspaceBeforeCutoff"] = Utilities.IsWorkspaceBeforeSAPCutoff(ws.CreationDate);
			ViewData["HistoricalReferenceExplanationIsRequired"] = Utilities.IsHistoricalReferenceExplanationRequired(ws.CreationDate);

			//check if skill mix is enabled and workspace starts after skill mix date 
			bool enableSkillMix = false;
			if (Utilities.IsSkillMixEnabledForSystem && Utilities.ShowSkillMixForWorkspace(ws.CreationDate))
			{
				enableSkillMix = true;
			}
			ViewData["EnableSkillMix"] = enableSkillMix;
			ViewData["EnableCommonDisclosure"] = false;
			if (Utilities.IsBRCEnabledForWorkspace(workspace) && boe.EndDate >= Utilities.OneLmxStartDate)
			{
				//enable the common disclosure table
				if (enableSkillMix)
				{
					ViewData["EnableCommonDisclosure"] = true;
				}
				//check if ws contains BRCs, if not, mark moq equation as read only
				ICollection <ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(ws.ResourcesForWsResourceListId.ToList(), true, workspace);
				if (resources.Count == 0)
				{
					ViewData["READONLY"] = true;
				}
			}

			ViewResult toReturn = View(WebConstants.VIEW_MOQ_EQUATION_FIELD, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayTaskElementDetails", sw);
			return toReturn;
		}

		/// <summary>
		/// Gets a partial view of a MOQ equation copied from a task element in another BOE.
		/// </summary>
		/// <param name="workspace">Current workspace.</param>
		/// <param name="boeID">(Target Boe) Current BOE Id.</param>
		/// <param name="copyBoeId">(Source Boe) BOE Id the MOQ equation is being copied from.</param>
		/// <param name="taskElementId">(Source Task) The source MOQ equation task element Id.</param>
		/// <param name="destinationTaskElementId">(Target Task) Task Element Id the MOQ equiation is being copied to.</param>
		/// <returns>Patial view containing a MOQ equation from another task element.</returns>
		public ViewResult CopyMoqEquation(string workspace, int boeID, int copyBoeId, int taskElementId, int destinationTaskElementId)
		{
			FullWorkspace fullWorkspace = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "CopyMoqEquation", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, fullWorkspace, boeID);
			FullBoe boe = this.Factory.CreateFullBoe(copyBoeId);

			BoeTaskElementDTO copyTaskElement = this.Factory.CreateTaskElement(taskElementId, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision);

			MOQEquationModelView modelView = new MOQEquationModelView();
			if (_BOECopier.CopyTaskElementMoqEquation(copyTaskElement, boeID))
			{
				modelView = GetCopyMoqEquationModelView(fullWorkspace.Id, copyBoeId, copyTaskElement, destinationTaskElementId);
			}

			modelView.UsingTemplateBOE = fullWorkspace.UsingTemplateBOE;
			modelView.MOQTypes = this._BoeLaborControllerLogic.GetMOQTypeSelectList(fullWorkspace.CreationDate >= moqTemplateUsageStartDate, null);
			if (fullWorkspace.UsingTemplateBOE)
			{
				modelView.MoqTypeTableDataLabels = this._BoeLaborControllerLogic.GetMoqTypeLabels();
				modelView.SelectedMoqTypes = boe.MoqTypeSelections.Where(x => x.TaskId == taskElementId).ToList();

				int i = 0;
				modelView.SelectedMoqTypes.ForEach(x =>
				{
					x.BoeId = boeID;
					x.TaskId = destinationTaskElementId;
					x.Id = --i;
					x.TableData.ForEach(z =>
					{
						z.Id = --i;
					});
				});
			}

			SetMOQEquationViewData(fullWorkspace, boeID, copyTaskElement.MOQType);
			ViewResult toReturn = View(WebConstants.VIEW_MOQ_EQUATION_FIELD, modelView);

			// Finalize Action
			FinalizeAction(_log, "CopyMoqEquation", sw);
			return toReturn;
		}

		#endregion Display

		#region AJAX Methods

		/// <summary>
		/// Calculates the spread.
		/// </summary>
		/// <param name="value">The spread value.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="curve">The spread curve.</param>
		/// <param name="precision">The precision.</param>
		/// <returns></returns>
		public ActionResult CalculateSpread(string workspace, RecalcSpreadModelView[] items, decimal moqTotalHours)
		{
			if (ReferenceEquals(items, null))
			{
				throw new ArgumentNullException(nameof(items));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			foreach (RecalcSpreadModelView item in items)
			{
				if (item.start == null)
				{
					throw new ArgumentException("item's start was null");
				}

				if (item.end == null)
				{
					throw new ArgumentException("Item's end was null");
				}

				if (item.curve == null)
				{
					throw new ArgumentException("Item's curve was null");
				}

				if (item.value == null)
				{
					throw new ArgumentException("Item's spread value was null");
				}

				if (item.percentLocked == null)
				{
					throw new ArgumentException("Item's percentLocked was null");
				}

				if (item.rateType == null)
				{
					throw new ArgumentException("Item's rateType was null");
				}
			}

			this._BoeLaborControllerLogic.RecalculateLaborSpreads(ws, items, moqTotalHours);

			return this.Json(items);
		}

		/// <summary>
		/// Saves the task data model.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="modelView">The model view.</param>
		/// <returns></returns>
		public ActionResult SaveTaskDataModel(string workspace, LaborTaskDataModelView modelView, bool isLocked = false)
		{
			_ = modelView ?? throw new ArgumentNullException(nameof(modelView));
			_ = modelView.TaskElementData ?? throw new ArgumentNullException("modelView", "TaskElementData is null inside modelView");
			_ = modelView.LaborTypesData ?? throw new ArgumentNullException("modelView", "LaborTypesData is null inside modelView");

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_TASK_DATA_MODEL, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, modelView.TaskElementData.BOEID);

			ICollection<ValidationMessage> validationErrors = isLocked ? new Collection<ValidationMessage>() : this._BoeLaborControllerLogic.ValidateLaborTaskDataWithDataModification(ws, modelView);

			// could not move the following logic to the BOELaborControllerLogic because ValidationFactory is static which can't be mocked, test for task id uniqueness across all task elements
			Collection<Dictionary<string, string>> boetaskDict = new Collection<Dictionary<string, string>>();
			boetaskDict.Add(new Dictionary<string, string>());
			boetaskDict.First<Dictionary<string, string>>().Add("BOEID", modelView.TaskElementData.BOEID.ToString());
			boetaskDict.First<Dictionary<string, string>>().Add("TaskID", modelView.TaskElementData.TaskID);
			boetaskDict.First<Dictionary<string, string>>().Add("TaskElementDetailID", modelView.TaskElementData.TaskElementDetailID.Value.ToString());

			Validator validator = ValidationFactory.Instance.getValidator(ValidationType.BoeTaskIDUnique);
			if (validator.validation(modelView.TaskElementData.TaskID, boetaskDict).Any())
			{
				validationErrors.Add(new ValidationMessage("TaskID", "The Task ID must be unique among Labor, Travel and ODC task elements."));
			}

			// filter out any MVC validation errors against deleted rows
			if ((!this.ModelState.IsValid && modelView != null) || validationErrors.Any())
			{
				Collection<ValidationMessage> mvcValidationErrors = Utilities.CreateModelStateValidationErrorList(this.ModelState);
				IList<ValidationMessage> mvcPreScrubValidationErrors = new List<ValidationMessage>(validationErrors.Concat(mvcValidationErrors));
				IList<ValidationMessage> mvcScrubbedValidationErrors = new List<ValidationMessage>();

				LaborTypeDataModelView[] laborTypesArray = modelView.LaborTypesData.ToArray();

				// update errors to reference the correct form
				foreach (ValidationMessage validationError in mvcPreScrubValidationErrors)
				{
					bool include = true;

					if (validationError.FieldName.ContainsEquivalent("Spreads["))
					{
						int idx = validationError.FieldName.GetIndex();
						if (laborTypesArray[idx].Deleted)
						{
							include = false;
						}
						else
						{
							validationError.FormIDToTarget = "LaborSpreadForm";
						}
					}
					else if (validationError.FieldName.ContainsEquivalent("LaborTypes[") || validationError.FieldName.ContainsEquivalent("LaborTypesData["))
					{
						int idx = validationError.FieldName.GetIndex();
						if (laborTypesArray[idx].Deleted)
						{
							include = false;
						}
						else
						{
							validationError.FormIDToTarget = "LaborTypesForm";
						}
					}

					if (include)
					{
						mvcScrubbedValidationErrors.Add(validationError);
					}
				}

				if (mvcScrubbedValidationErrors.Any())
				{
					validationErrors = mvcScrubbedValidationErrors;
				}
			}

			// Validate Rich Text
			ICollection<ValidationMessage> richTextValidationErrors = this.ScrubViewModelRichTextForSave(modelView.TaskElementData);
			if (richTextValidationErrors.Any())
			{
				validationErrors.AddRange(richTextValidationErrors);
			}

			ICollection<RteCustomTemplateSourceModelView> sources = this.rteTemplateDataLoader.GetSources(ws.UsingTemplateBOE);
			ICollection<ValidationMessage> rteValidationErrors = this.ValidateRteAnswers(modelView.TaskElementData.RteTemplateAnswers, sources, ws.RteSizeLimit);
			if (rteValidationErrors.Any())
			{
				validationErrors.AddRange(rteValidationErrors);
			}

			BoeTaskElementDTO dto = this._BoeLaborControllerLogic.ConvertModelViewToDto(modelView, ws);

			// Validate DTO 
			if (!isLocked)
			{
				validationErrors.AddRange(this._BoeLaborControllerLogic.ValidateTaskElementDto(ws, dto));
			}

			if (validationErrors.Any())
			{
				throw new GenValidationException(validationErrors);
			}

			// Save
			this._BoeLaborControllerLogic.SaveLaborTaskData(ws, dto, modelView.TaskElementData.MetricIds, modelView.TaskElementData.RteTemplateAnswers, modelView.MOQTypes);

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_TASK_DATA_MODEL, sw);

			// pass the id back to the front-end (in case this is a new task element)
			return this.Json(dto.Id);
		}

		/// <summary>
		/// Saves the task data model in a locked workspace.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="modelView">The model view.</param>
		/// <returns></returns>
		public ActionResult SaveLockedTaskDataModel(string workspace, LaborTaskDataModelView modelView)
		{
			if (modelView == null)
			{
				throw new ArgumentNullException(nameof(modelView));
			}

			if (modelView.TaskElementData == null)
			{
				throw new ArgumentNullException("modelView", "TaskElementData is null inside modelView");
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			ICollection<ValidationMessage> validationErrors = this._BoeLaborControllerLogic.ValidateLockedLaborTaskData(ws, modelView);

			if (validationErrors.Any())
			{
				throw new GenValidationException(validationErrors);
			}

			return SaveTaskDataModel(workspace, modelView, true);
		}

		/// <summary>
		/// Gets the task data model.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="taskElementId">The task element identifier.</param>
		/// <returns></returns>
		public ActionResult GetTaskDataModel(string workspace, int boeId, int taskElementId)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "GetTaskDataModel", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeId);
			FullBoe boe = this.Factory.CreateFullBoe(boeId);

			LaborTaskDataModelView modelView = this._BoeLaborControllerLogic.GetLaborTaskData(ws, boe, taskElementId);

			// Finalize Action
			this.FinalizeAction(this._log, "GetTaskDataModel", sw);

			return this.Json(modelView);
		}

		/// <summary>
		/// Page historical metrics search results for MST.
		/// </summary>
		/// <param name="HistoricalMetricsSearchResults">The Historical metrics results.</param>
		/// <returns>Paged Results</returns>
		public ViewResult PageHistoricalMetricsSearchResultsMST(HistoricalMetricsFromMSTModelView historicalMetricsSearchResults)
		{
			Stopwatch sw = InitializeAction(_log, "PageHistoricalMetricsSearchResultsMST", SecurityPage.HistoricalMetricSearch, SecurityAuthorization.Read, null, null);

			if (historicalMetricsSearchResults == null)
			{
				throw new ArgumentNullException(nameof(historicalMetricsSearchResults));
			}

			historicalMetricsSearchResults.MetricsSearchResults = new Collection<MSTMetricDetailsDTO>();

			ICollection<int> ids = new Collection<int>();

			for (int i = historicalMetricsSearchResults.StartArrayIndex; i <= historicalMetricsSearchResults.EndArrayIndex; i++)
			{
				ids.Add(historicalMetricsSearchResults.PagedIndexes[i]);
			}
			historicalMetricsSearchResults.MetricsSearchResults = _MSTMetricsLoader.GetByIds(ids).OrderBy(m => m.MeasureName).ToCollection<MSTMetricDetailsDTO>();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_HISTORICAL_METRICS_SEARCH_RESULTS_MST, historicalMetricsSearchResults);

			FinalizeAction(_log, "PageHistoricalMetricsSearchResultsMST", sw);
			return toReturn;
		}

		/// <summary>
		/// Get search results for type ahead
		/// </summary>
		/// <param name="searchTerm">search word to get type ahead</param>
		/// <returns>type ahead string</returns>
		virtual public JsonResult GetTypeAheadTerms(string searchTerm)
		{
			ICollection<String> typeAheadCollection = _BoeLaborControllerLogic.GetTypeAheadTerms(searchTerm);

			return Json(typeAheadCollection);
		}

		/// <summary>
		/// Call to the business layer to validate an MOQ equation.
		/// </summary>
		/// <param name="moqEquation">The MOQ Equation</param>
		/// <returns>A collection of variables, if the equation contained any. The collection always begins
		/// with the equation reformatted with tagged variable names.</returns>
		[HttpPost]
		virtual public JsonResult MOQValidate(string workspace, int boeID, String moqEquation)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "MOQValidate", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			// Perform Action
			ICollection<string> result;

			JsonResult toReturn = Json(new { Status = false });

			try
			{
				// Get a reformatted version of the input equation (element 0 in the return collection)
				// and a list of variables found in the equation (elements 1 -> n)
				result = ActionLogic.Common.MOQ.Parser.Validate(moqEquation);

				toReturn = Json(new { Status = true, Variables = result });

				// a user can manually type in a workspace variable name. when they do this, there is no circular reference check as there would
				// have been if they selected 'insert workspace variable'. if there are variables in this equation, they will be compared to workspace
				// variables to make sure there is no circular reference
				int count = result.Count();

				//only need to do this check if variables are part of the equation
				if (count >= 2)
				{

					// only need to worry about sum of boe workspace variables
					IEnumerable<WorkspaceVariableDTO> allWorkspaceVariables = ws.WorkspaceVariables.Where(x => x.ValueType == VarValueType.SumOfBOEs).Select(y => y);

					// if there are no workspace variables, then no circular references checks are necessary
					if (allWorkspaceVariables.Any())
					{
						foreach (string variable in result)
						{
							WorkspaceVariableDTO c = (from a in allWorkspaceVariables
													  where a.WorkspaceVariableName.ToUpper() == variable.ToUpper()
													  select a).FirstOrDefault();

							if (c != null)
							{
								VariableCircularReferenceCheckerCache circularReferenceCache = new VariableCircularReferenceCheckerCache();
								bool createsCR = _VariableCircularReferenceChecker.WorkspaceVariableCreatesCircularReference(circularReferenceCache, boeID, c, ws);
								if (createsCR)
								{
									throw new GeneralMOQParsingException(
								 string.Format(
									 "'{0}' will cause a circular reference and cannot be used.", c.WorkspaceVariableName));
								}
							}

						}
					}
				}
			}
			catch (GeneralMOQParsingException ex1)
			{
				toReturn = Json(new { Status = false, Message = ex1.Message });
			}
			catch (Exception ex2)
			{
				_log.Error(ex2, "MOQ Validation Exception");
				throw;
			}

			// Finalize Action
			FinalizeAction(_log, "MOQValidate", sw);
			return toReturn;
		}

		/// <summary>
		/// Calculates an MOQ Equation String and returns the result
		/// </summary>
		/// <param name="moqEquation">The MOQ Equation</param>
		/// <returns>A string version of the Int64 result</returns>
		[HttpPost]
		virtual public ActionResult MOQCalculate(string workspace, int boeID, String moqEquation)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "MOQCalculate", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			JsonResult toReturn = null;

			// Perform Action
			try
			{
				String result = ActionLogic.Common.MOQ.Parser.Calculate(moqEquation, ws);

				if (result.Contains("E+") || (!String.IsNullOrEmpty(result) && (decimal.Parse(result) > 9999999999 || decimal.Parse(result) < -9999999999)))
				{
					throw new GeneralMOQCalculationException("MOQ Equation must be between -9,999,999,999 and 9,999,999,999");
				}

				toReturn = Json(new { Status = true, Result = result });
			}
			catch (GeneralMOQCalculationException ex1)
			{
				_log.Warn(ex1, "MOQ Calculation Exception");
				toReturn = Json(new { Status = false, Message = ex1.Message });
			}

			// Finalize Action
			FinalizeAction(_log, "MOQCalculate", sw);
			return toReturn;
		}

		/// <summary>
		/// MarkWarningMessageAsConfirmed
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <param name="moqEquation"></param>
		/// <returns></returns>
		virtual public ActionResult MarkWarningMessageAsConfirmed(string workspace, int boeID, int TaskID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "MarkWarningMessageAsConfirmed", SecurityPage.TaskElements, SecurityAuthorization.ReadUpdate, ws, boeID);

			JsonResult toReturn = Json(new { Status = true });

			// Save the updates
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				// Save the Task Element
				this.taskElementDataLoader.SaveBOETaskElementLaborTypeWarning(TaskID, false);

				scope.Complete();
			}

			// Finalize Action
			FinalizeAction(_log, "MarkWarningMessageAsConfirmed", sw);
			return toReturn;
		}

		/// <summary>
		/// Imports the Labor Type and Labor Spread data.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeID">The boe identifier.</param>
		/// <param name="taskElementID">The task element identifier.</param>
		/// <param name="importResults">The import results.</param>
		/// <param name="importSpreadResults">The import spread results.</param>
		/// <param name="moqEquation">The moq equation.</param>
		/// <returns></returns>
		/// <exception cref="GenValidationException"></exception>
		/// <exception cref="System.ArgumentNullException">importResults</exception>
		virtual public ActionResult ImportLaborTypeAndSpread(string workspace, int boeID, int taskElementID, Collection<ImportLaborTypeModelView> importResults, string moqEquation)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportLaborTypeAndSpread", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			JsonResult toReturn = Json(new { Status = false });

			/** Valid Model Check */
			if (ModelState.IsValid && (importResults != null))
			{
				// Import the Labor Type data
				this.ImportLaborType(ws, boeID, taskElementID, importResults, moqEquation);

				toReturn = Json(new { Status = true });
			}
			else if (!ModelState.IsValid)
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}
			else
			{
				// this else case covers if both import labor type and spread results are null
				throw new ArgumentNullException(nameof(importResults));
			}

			// Finalize Action
			FinalizeAction(_log, "ImportLaborTypeAndSpread", sw);
			return toReturn;
		}

		/// <summary>
		/// Imports the preview labor type and spread.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeID">The boe identifier.</param>
		/// <param name="taskElementID">The task element identifier.</param>
		/// <param name="laborTypeImportType">Type of the labor type import: new or update</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		virtual public ActionResult ImportPreviewLaborTypeAndSpread(string workspace, int boeID, int taskElementID, string laborTypeImportType)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ImportPreviewLaborTypeAndSpread", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			ContentResult toReturn = null;

			// If a file was uploaded successfully
			if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
			{
				BoeTaskElementDTO taskElementDTO = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

				Boolean newOnly = "new".Equals(laborTypeImportType);

				try
				{
					Collection<ImportedLaborType> results = _LaborTypeAndSpreadImporter.ImportLaborTypeFromExcelFile(Request.Files[0].InputStream, taskElementDTO, ws, newOnly);

					toReturn = GenerateUploadResponse(true, results, Request.Files[0].FileName);
				}
				// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = GenerateUploadResponse(false, "File is an invalid format. File must be in a MS Excel (.xlsx) format.");
				}
				catch (ColumnMissingException ex1)
				{
					toReturn = GenerateUploadResponse(false, "File does not contain all of the required columns. The following columns are missing: {0}.", ex1.Message);
				}
				catch (DateImportException ex1)
				{
					toReturn = GenerateUploadResponse(false, "{0}", ex1.Message);
				}
				catch (DataErrorImportException ex1)
				{
					toReturn = GenerateUploadResponse(false, "{0}", ex1.Message);
				}
				catch (Exception ex0)
				{
					_log.Error(ex0, "Unknown Import Resources Error.");
					toReturn = GenerateUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = GenerateUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			FinalizeAction(_log, "ImportPreviewLaborTypeAndSpread", sw);

			return toReturn;
		}

		/// <summary>
		/// Exports the Labor Type and Labor Spread data.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeID">The boe identifier.</param>
		/// <param name="taskElementID">The task element identifier.</param>
		/// <param name="isTemplate">Whether the export is for just the template or includes the data.</param>
		/// <returns>An Excel export containing the Labor Type and Labor Spread data.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		virtual public ActionResult ExportLaborTypeAndSpread(string workspace, int boeID, int taskElementID, bool isTemplate)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportLaborTypeAndSpread", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			BoeTaskElementDTO thisTaskElement = null;

			if (taskElementID != -1)
			{
				thisTaskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);
				DataRelationshipVerifier.VerifyDataRelation(thisTaskElement, boeID);
			}

			string templateName;

			if (Utilities.IsBRCEnabledForWorkspace(workspace))
			{
				templateName = TEMPLATE_FOLDER + "LaborTypesAndSpread_BRCEnabled.xlsx";
			}
			else
			{
				templateName = TEMPLATE_FOLDER + "LaborTypesAndSpread.xlsx";
			}

			string exportedFileName = LaborTypeAndSpreadExporter.ExportToExcelFile(Server.MapPath(templateName), _ResourceDTODataLoader, _CommonDataMapper, ws, thisTaskElement, boeID, isTemplate);

			string fileName = ws.WorkspaceName + "_BOE-" + boeID + "_Task-" + taskElementID + "_ResourceTypes.xlsx";
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, "ExportLaborTypeAndSpread", sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Exports the Offload Rates.
		/// Note:  This is RMS-specific, will need to be reworked to support Space
		/// </summary>
		/// <returns>Download Result for the Offload Rates.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportOffloadRates(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_OFFLOAD_RATES, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			// Get the rates list
			ICollection<OffloadRatesDTO> ratesForMV = this.offloadRatesLoader.GetByWorkspaceId(ws.Id).OrderBy(r => r.Resource).ThenBy(r => r.PerformingOrg).ThenBy(r => r.Year).ToList();

			// Get Offload Rates template file name
			string templateFileName = Server.MapPath(TEMPLATE_FOLDER + SYSTEM_OFFLOAD_RATES_EXPORT_TEMPLATE);

			ICollection<ResourceDTO> resources = ws.ResourcesForWsResourceListId.ToList();
			ICollection<PerformingOrgDTO> performingOrgs = ws.PerformingOrgsForWsList.ToList();

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = SystemOffloadRatesExporterRMS.ExportToExcelFile(templateFileName, ratesForMV, performingOrgs, resources);

			string fileName = "Workspace_OffloadRates.xlsx";
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EXPORT_OFFLOAD_RATES, sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Validates Spread Dates
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeId">Boe Id</param>
		/// <param name="taskElementID">Task Element Id</param>
		/// <returns>Validation Messages or a status of true if all are valid</returns>
		[HttpPost]
		virtual public JsonResult ValidateSpreadDatesAndValues(string workspace, int boeId, int taskElementID)
		{
			JsonResult result = Json(new { Status = true });

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			BoeTaskElementDTO thisTaskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ValidateSpreadDates", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeId);

			ICollection<LaborValidationClass> errors = this.taskElementValidation.ValidateTaskElementsWithErrorMessages(ws, new List<BoeTaskElementDTO>() { thisTaskElement });

			if (errors.Any())
			{
				result = Json(new { Errors = errors.Select(x => x.ErrorMessage).ToList() });
			}

			// Finalize Action
			FinalizeAction(_log, "ValidateSpreadDates", sw);

			return result;
		}

		/// <summary>
		/// Performs recalculation of the underlying task element, based on the DB data. 
		/// THIS RECALCULATES THE DATA IN THE DB AND SAVES THE DATA INTO THE DB AFTERWARDS
		/// THE PURPOSE IS TO USE IT TO CLEAN UP BAD DATA
		/// THIS DOES NOT RECALCULATE THE PAGE
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeId">Boe Id</param>
		/// <param name="taskElementID">Task Element Id</param>
		/// <returns>An indication when it's finished</returns>
		[HttpPost]
		public JsonResult DoFullRecalculationWithPageRefresh(string workspace, int boeID, int? taskElementID)
		{
			if (!taskElementID.HasValue) { throw new ArgumentNullException(nameof(taskElementID)); }

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "DoFullRecalculationWithPageRefresh", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			HashSet<BoeDTO> originalWsBoes = new HashSet<BoeDTO>(ws.Boes.ToList<BoeDTO>().DeepClone());
			HashSet<BoeTaskElementDTO> tasksToSave = new HashSet<BoeTaskElementDTO>();
			HashSet<WorkspaceVariableDTO> wsVarsToSave = new HashSet<WorkspaceVariableDTO>();
			HashSet<FullBoe> boesToSave = new HashSet<FullBoe>();

			BoeTaskElementDTO thisTaskElement = this.Factory.CreateTaskElement(taskElementID.Value, ws.DecimalPrecision, ws.CostDecimalPrecision);

			// We need to clean up the dates before we call recalculate..
			// Fix start/end dates for the resource types, if necessary
			// Mark any invalid spreads for deletion
			foreach (ResourceTypeDto resourceType in thisTaskElement.taskElementLabors)
			{
				// if the dates fall outside of where they should be, fix them
				if (thisTaskElement.StartDate.HasValue && resourceType.StartDate < thisTaskElement.StartDate.Value)
				{ resourceType.StartDateValue = thisTaskElement.StartDate.Value; resourceType.Updateable = UpdateType.Upsert; }
				if (thisTaskElement.EndDate.HasValue && resourceType.EndDate > thisTaskElement.EndDate.Value)
				{ resourceType.EndDateValue = thisTaskElement.EndDate.Value; resourceType.Updateable = UpdateType.Upsert; }

				foreach (ResourceSpreadDto spread in resourceType.LaborSpreads.ToList())
				{
					if (spread.LaborSpreadDate < resourceType.StartDate || spread.LaborSpreadDate > resourceType.EndDate) { resourceType.LaborSpreads.Remove(spread); }
					else { spread.Updateable = UpdateType.Upsert; } // that way it doesn't get deleted..
				}

				// The RecalculateLaborInTaskElementsWithoutSaving method only recalculates hours. So the cost resource types need to be handled manually
				// Since cost is always discrete, we just need to update the value based on the (not deleted) spreads
				if (resourceType.SpreadType == SpreadType.Cost)
				{
					decimal newCostSpreadValue = resourceType.LaborSpreads.Where(x => x.Updateable != UpdateType.Deleted).Sum(x => x.LaborSpreadValue);

					if (newCostSpreadValue != resourceType.ValueSpread)
					{ resourceType.ValueSpread = newCostSpreadValue; resourceType.Updateable = UpdateType.Upsert; }
				}
			}

			// Run through the recalculation (of labor types only) & save process for the Boe/task element, as appropriate
			this.fullWsRecalc.RecalculateLaborInTaskElementsWithoutSaving(ws, ref tasksToSave, ref wsVarsToSave, ref boesToSave, new List<BoeTaskElementDTO>() { thisTaskElement });
			this.fullWsRecalc.ValidateStateTransitionForBoesEffectedByRecalculation(ws, boesToSave, originalWsBoes);

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				this.fullWsRecalc.SaveDataEffectedByRecalculation(ws, tasksToSave, new HashSet<WorkspaceVariableDTO>(), boesToSave);
				this.fullWsRecalc.PerformStateTransitionActionsForBoesEffectedByRecalculation(ws, boesToSave, originalWsBoes);

				scope.Complete();
			}

			FinalizeAction(_log, "DoFullRecalculationWithPageRefresh", sw);

			return Json(new { Status = true });
		}

		/// <summary>
		/// Save Reordering of Labor Types
		/// </summary>
		/// <param name="modelView"></param>
		/// <param name="workspace"></param>
		/// <param name="taskElementID"></param>
		/// <returns></returns>
		public JsonResult SaveReorderLaborTypes(LaborTypeOrderCollection modelView, string workspace, int taskElementID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

			if (modelView == null)
			{
				throw new ArgumentNullException(nameof(modelView));
			}

			if (taskElement.taskElementLabors.Count != modelView.LaborTypes.Count)
			{
				throw new ValidationException("Number of Labor Types in save does not match number of Labor Types in the Database.");
			}

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveReorderLaborTypes", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, taskElement.BoeID);

			this._BoeLaborControllerLogic.ReOrderLaborTypeOrder(ws, taskElement, modelView);

			JsonResult toReturn = Json(new { Status = true });

			// Finalize Action
			FinalizeAction(_log, "SaveReorderLaborTypes", sw);
			return toReturn;
		}

		/// <summary>
		/// Import MOQ Table data
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="taskElementID">Task ID</param>
		/// <returns>View with imported MOQ Table data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "moqTypeId")]
		public async Task<ViewResult> ImportMoqTables(string workspace, int taskElementID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_MOQ_TABLES, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, taskElement.BoeID);

			JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

			ImportMoqTableResultDataModelView importResults = await this._BoeLaborControllerLogic.ImportMoqTables(ws, Request);

			this.ViewData["ERRORS_OCCURRED"] = importResults.ErrorsOccurred;
			this.ViewData["SERIALIZED_DATA"] = serializer.Serialize(importResults.DataToSave());
			this.ViewData["DOCUMENT_DOMAIN"] = this.Request["documentDomain"];

			ViewResult toReturn = this.View(WebConstants.VIEW_MOQ_TABLE_IMPORT_VERIFICATION, importResults.Result);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_IMPORT_MOQ_TABLES, sw);
			return toReturn;
		}

		/// <summary>
		/// Complete the MOQ Table import
		/// </summary>
		/// <param name="importResults">The imoprt rsults to save</param>
		/// <param name="moqTypeId">ID of the MOQ Type</param>
		/// <param name="workspace">Workspace name</param>
		/// <param name="taskElementID">Task element ID</param>
		/// <returns>Json result</returns>
		public JsonResult CompleteImportMoqTables(ICollection<ImportMoqTableResultsModelView> importResults, int moqTypeId, string workspace, int taskElementID)
		{
			// TODO - fix dates, reload page
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_COMPLETE_IMPORT_MOQ_TABLES, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, taskElement.BoeID);

			JsonResult toReturn;

			this._BoeLaborControllerLogic.CompleteImportMoqTables(importResults, moqTypeId);

			toReturn = this.Json(new { Status = true });

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_COMPLETE_IMPORT_MOQ_TABLES, sw);
			return toReturn;
		}

		/// <summary>
		/// Export MOQ Table data
		/// </summary>
		/// <param name="moqTypeId">ID of the MOQ type</param>
		/// <param name="workspace">Workspace name</param>
		/// <param name="taskElementID">Task ID</param>
		/// <returns>Export</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportMoqTables(int moqTypeId, string workspace, int taskElementID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_MOQ_TABLES, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, taskElement.BoeID);

			string exportedFileName = this._BoeLaborControllerLogic.ExportMoqTables(moqTypeId, ws, Server.MapPath(TEMPLATE_FOLDER + this.moqTableExporter.MOQ_TABLE_EXCEL_MAP_PATH));

			string fileName = string.Format("Task-{0}_{1}_MoqTableData.xlsx", taskElement.Id, taskElement.TaskTitle);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EXPORT_MOQ_TABLES, sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Parses text into Query Filters
		/// </summary>
		/// <param name="text">The text to parse</param>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <returns></returns>
		public async Task<ActionResult> ParseSapFilter(string text, string workspace, int boeId)
		{
			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PARSE_SAP_FILTER, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			IESResponse<QueryViewModel> response = await this._BoeLaborControllerLogic.ParseSapFilter(text);
			JsonResult toReturn = this.Json(response);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_PARSE_SAP_FILTER, sw);
			return toReturn;
		}

		/// <summary>
		/// Converts Query Filters into Text
		/// </summary>
		/// <param name="filters">The query filters to convert into text</param>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <returns></returns>
		public async Task<ActionResult> ConvertSapFilter(ICollection<QueryViewModel> filters, string workspace, int boeId)
		{
			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CONVERT_SAP_FILTER, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			IESResponse<string> response = await this._BoeLaborControllerLogic.ConvertSapFilter(filters);
			JsonResult toReturn = this.Json(response);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_CONVERT_SAP_FILTER, sw);
			return toReturn;
		}

		/// <summary>
		/// Export Actuals data for SAP
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <param name="tableData">The MOQ Table Data</param>
		/// <returns>Validation Response with file as byte array</returns>

		public async Task<ActionResult> ExportActualsSap(string workspace, int boeId, MoqTableDataModelView tableData)
		{
			if (tableData == null)
			{
				throw new ArgumentNullException(nameof(tableData));
			}

			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_ACTUALS_SAP, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			IESResponse<byte> response = await this._BoeLaborControllerLogic.ExportActualsSap(tableData);

			List<ErrorModelView> errors = response.Messages?.Select(m => new ErrorModelView() { ValidationIssue = m }).ToList();
			string data = response.Data != null ? System.Convert.ToBase64String(response.Data.ToArray()) : String.Empty;
			JsonResult toReturn = this.Json(new { IsSuccessful = response.IsSuccessful, Messages = errors, Data = data });

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EXPORT_ACTUALS_SAP, sw);
			return toReturn;
		}

		/// <summary>
		/// Calculates all Actuals for MOQ Data Tables from SAP
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <param name="tableData">The MOQ Table Data</param>
		/// <returns>Validation Response</returns>
		public async Task<ActionResult> CalculateAllActualsSap(string workspace, int boeId, ICollection<MoqTableDataModelView> tableData)
		{
			if (tableData == null || !tableData.Any())
			{
				throw new ArgumentNullException(nameof(tableData));
			}

			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			ICollection<IESResponse<CalculateActualsViewModel>> response = await this._BoeLaborControllerLogic.CalculateAllActualsSap(tableData);

			JsonResult toReturn = this.Json(new { IsSuccessful = response != null, data = response });

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP, sw);
			return toReturn;
		}

		/// <summary>
		/// Calculates all Actuals for MOQ Data Tables from SAP
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <param name="tableData">The MOQ Table Data</param>
		/// <returns>Validation Response</returns>
		public async Task<ActionResult> CalculateAllActualsSapWithSkillMix(string workspace, int boeId, ICollection<MoqTableDataModelView> tableData)
		{
			if (tableData == null || !tableData.Any())
			{
				throw new ArgumentNullException(nameof(tableData));
			}

			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP_WITH_SKILL_MIX, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			ICollection<IESResponse<CalculateActualsWithSkillMixViewModel>> response = await this._BoeLaborControllerLogic.CalculateAllActualsSapWithSkillMix(tableData);

			JsonResult toReturn = this.Json(new { IsSuccessful = response != null, data = response });

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP_WITH_SKILL_MIX, sw);
			return toReturn;
		}

		/// <summary>
		/// Refreshes the Skill Mix Table with updated resource hours
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <param name="resourceHours">MOQ Table Resource Hours</param>
		/// <param name="currentSkillMixData">The current skill mix data</param>
		/// <returns></returns>
		public ActionResult RefreshSkillMixTable(string workspace, int boeId, ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours, ICollection<SkillMixModelView> currentSkillMixData)
		{
			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_REFRESH_SKILL_MIX_TABLE, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			ICollection<SkillMixModelView> response = this._BoeLaborControllerLogic.RefreshSkillMixTable(resourceHours, currentSkillMixData);

			JsonResult toReturn = this.Json(new { IsSuccessful = response != null, data = response });

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_REFRESH_SKILL_MIX_TABLE, sw);
			return toReturn;
		}

		/// <summary>
		/// Refreshes the Skill Mix Table with updated resource hours
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="boeId">BOE Id</param>
		/// <param name="currentSkillMixData">The current skill mix data</param>
		/// <param name="commonDisclosureSMData">The common disclosure skill mix data</param>
		/// <param name="resourceHours">The MOQ Table Resource Hours</param>
		/// <returns></returns>
		public ActionResult RefreshCommonDisclosureTable(string workspace, int boeId, ICollection<SkillMixModelView> currentSkillMixData, ICollection<CommonDisclosureModelView> commonDisclosureSMData,
			ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours)
		{
			// Initialize Action
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_REFRESH_COMMON_DISCLOSURE_TABLE, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeId);

			// Call to Controller Logic
			ICollection<CommonDisclosureModelView> response = this._BoeLaborControllerLogic.RefreshCommonDisclosureTable(currentSkillMixData, commonDisclosureSMData, resourceHours);

			JsonResult toReturn = this.Json(new { IsSuccessful = response != null, data = response });

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_REFRESH_COMMON_DISCLOSURE_TABLE, sw);
			return toReturn;
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the labor spreads from import spread results.
		/// </summary>
		/// <param name="laborTypeToUpdate">The labor type to update.</param>
		/// <param name="importSpreadResults">The import spread results.</param>
		/// <param name="taskElementDTO">The task element dto.</param>
		/// <param name="workspace">The workspace.</param>
		private void UpdateLaborSpreadsFromImport(ResourceTypeDto laborTypeToUpdate, Collection<ImportLaborSpreadModelView> importSpreadResults, BoeTaskElementDTO taskElementDTO, FullWorkspace workspace)
		{
			DateTime startDate = GenBOEUtilities.AdjustDateTimePrecision(laborTypeToUpdate.StartDateValue);
			DateTime endDate = GenBOEUtilities.AdjustDateTimePrecision(laborTypeToUpdate.EndDateValue);

			if (importSpreadResults != null)
			{
				// Get EXISTING UPDATED Labor Spreads from the imported data
				IEnumerable<ImportLaborSpreadModelView> updatedLaborSpreads = from x in importSpreadResults
																			  where x.ImportTypes.Contains((int)LaborSpreadImportResult.UpdateSpread)
																			  select x;

				if (importSpreadResults.Any(i => i.ImportTypes.Contains((int)LaborSpreadImportResult.UpdateSpread)))
				{
					// remove any current labor spreads (these will all be deleted during save anyway)
					ICollection<ResourceSpreadDto> originalSpreads = laborTypeToUpdate.LaborSpreads.ToList();
					laborTypeToUpdate.LaborSpreads.Clear();

					foreach (ImportLaborSpreadModelView spreadModelView in updatedLaborSpreads)
					{
						DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spreadModelView.LaborSpreadDate);
						// Only add new spreads if within the POP
						if (spreadDate >= startDate && spreadDate <= endDate)
						{
							ResourceSpreadDto spreadDto = new ResourceSpreadDto()
							{
								BoeID = laborTypeToUpdate.BoeID,
								Id = -1,
								LaborSpreadDate = spreadDate,
								LaborSpreadValue = spreadModelView.LaborSpreadValue,
								LaborTypeId = laborTypeToUpdate.Id,
								Updateable = UpdateType.Upsert
							};

							laborTypeToUpdate.LaborSpreads.Add(spreadDto);
						}
					}

					foreach (ResourceSpreadDto originalSpread in originalSpreads)
					{
						// Only add original spreads if within the POP and not overridden by new spreads
						if (originalSpread.LaborSpreadDate >= startDate && originalSpread.LaborSpreadDate <= endDate &&
							!laborTypeToUpdate.LaborSpreads.Any(s => s.LaborSpreadDate == originalSpread.LaborSpreadDate))
						{
							laborTypeToUpdate.LaborSpreads.Add(originalSpread);
						}
					}

					UpdatePercentSpread(laborTypeToUpdate, taskElementDTO, workspace);
				}
			}

			bool updatePercentSpread = false;
			// flag labor spreads for deletion that are outside POP
			foreach (ResourceSpreadDto spread in laborTypeToUpdate.LaborSpreads)
			{
				DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate);
				if (spreadDate < startDate || spreadDate > endDate)
				{
					spread.Updateable = UpdateType.Deleted;
					updatePercentSpread = true;
				}
			}

			foreach (ResourceSpreadDto spread in laborTypeToUpdate.LaborSpreads.Where(ls => ls.Updateable == UpdateType.None))
			{
				spread.Id = -1;
				spread.Updateable = UpdateType.Upsert;
			}

			if (updatePercentSpread)
			{
				UpdatePercentSpread(laborTypeToUpdate, taskElementDTO, workspace);
			}
		}

		/// <summary>
		/// Updates the percent spread for a labor resource.
		/// </summary>
		/// <param name="laborTypeToUpdate">The labor type resource to update.</param>
		/// <param name="taskElementDTO">The task element dto.</param>
		/// <param name="workspace">The workspace.</param>
		private void UpdatePercentSpread(ResourceTypeDto laborTypeToUpdate, BoeTaskElementDTO taskElementDTO, FullWorkspace workspace)
		{
			laborTypeToUpdate.Updateable = UpdateType.Upsert;
			int? decimalPrecision = laborTypeToUpdate.SpreadType == SpreadType.Hours ? workspace.ResourceDecimalPrecision : workspace.CostDecimalPrecision;
			laborTypeToUpdate.ValueSpread = Utilities.AdjustPrecision(laborTypeToUpdate.LaborSpreads.Where(ls => ls.Updateable != UpdateType.Deleted).Sum(s => s.LaborSpreadValue), decimalPrecision);
			laborTypeToUpdate.PercentSpread = 0;

			if (laborTypeToUpdate.SpreadType != SpreadType.Cost)
			{
				decimal moqHoursValue;
				if (decimal.TryParse(_BoeTaskElementRecalculation.CalculateMOQHoursTotal(taskElementDTO, workspace), out moqHoursValue))
				{
					decimal? percentSpread;
					if (moqHoursValue != 0)
					{
						percentSpread = (laborTypeToUpdate.ValueSpread / moqHoursValue) * 100;
					}
					else
					{
						//0 was decided to be the desired value.  
						percentSpread = 0;
					}
					laborTypeToUpdate.PercentSpread = percentSpread;
				}
			}
		}

		/// <summary>
		/// Imports the Labor Type Data into the database.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeID">The boe identifier.</param>
		/// <param name="taskElementID">The task element identifier.</param>
		/// <param name="importResults">The import results.</param>
		/// <param name="moqEquation">The moq equation.</param>
		private void ImportLaborType(FullWorkspace workspace, int boeID, int taskElementID, Collection<ImportLaborTypeModelView> importResults, string moqEquation)
		{
			if (importResults != null)
			{
				// Get the specified task element
				BoeTaskElementDTO taskElementDTO = this.Factory.CreateTaskElement(taskElementID, workspace.DecimalPrecision, workspace.CostDecimalPrecision);

				taskElementDTO.MOQHoursEquation = moqEquation;

				DataRelationshipVerifier.VerifyDataRelation(taskElementDTO, boeID);
				Decimal MOQtotal = Convert.ToDecimal(_BoeTaskElementRecalculation.CalculateMOQHoursTotal(taskElementDTO, workspace));

				// update all updated ones
				ICollection<ImportLaborTypeModelView> laborTypesToUpdate = (from i in importResults
																			where i.ImportTypes.Contains((int)LaborTypeImportResult.UpdateLaborType)
																			select i).ToList();
				
				ICollection<ResourceDTO> originalResourceList = workspace.ResourcesForWsResourceListId.ToList();

				foreach (ImportLaborTypeModelView laborTypeToUpdate in laborTypesToUpdate)
				{
					ResourceTypeDto thisLT = (from lt in taskElementDTO.taskElementLabors where lt.Id == laborTypeToUpdate.Id select lt).First();

					thisLT.EndDateValue = laborTypeToUpdate.EndDate;
					thisLT.StartDateValue = laborTypeToUpdate.StartDate;
					thisLT.SpreadCurveID = (SpreadCurves)laborTypeToUpdate.SpreadCurveID;
					thisLT.ResourceID = laborTypeToUpdate.ResourceID;
					thisLT.BusinessResourceCodeID = laborTypeToUpdate.BusinessResourceCodeID;

					// Determine the spread type based on the rate type of the resource.
					RateType rateType;
					ResourceDTO resourceForLabor = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, false, workspace.Shortname).FirstOrDefault(r => r.Id == laborTypeToUpdate.ResourceID);
					ResourceDTO brcForLabor = null;

					if (Utilities.IsBRCEnabledForWorkspace(workspace.Shortname))
					{
						brcForLabor = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, true, workspace.Shortname).FirstOrDefault(r => r.Id == laborTypeToUpdate.BusinessResourceCodeID);
					}

					// Rate Type Validation not needed here as the import already does it
					rateType = resourceForLabor?.RateType ?? brcForLabor.RateType;
					thisLT.SpreadType = rateType == RateType.Cost ? SpreadType.Cost : SpreadType.Hours;

					thisLT.PerformingOrgID = laborTypeToUpdate.PerformingOrgID;
					thisLT.Updateable = UpdateType.Upsert;
					thisLT.PercentSpreadLocked = laborTypeToUpdate.PercentSpreadLocked;
					thisLT.HourSpreadLocked = laborTypeToUpdate.HourSpreadLocked;
					thisLT.CLINID = laborTypeToUpdate.ClinID;
					thisLT.WBSID = laborTypeToUpdate.WbsID;
					thisLT.CanOffload = laborTypeToUpdate.CanOffload;
					thisLT.TieredPercentage = laborTypeToUpdate.TieredPercentage;
					// Discrete spreads will not require any value or percent spread changed.
					if (thisLT.SpreadCurveID != SpreadCurves.DiscreteHours && thisLT.SpreadCurveID != SpreadCurves.DiscreteCost)
					{
						if (thisLT.SpreadType == SpreadType.Cost)
						{
							if (thisLT.ValueSpread != laborTypeToUpdate.ValueSpread)
							{
								thisLT.ValueSpread = laborTypeToUpdate.ValueSpread.HasValue ? laborTypeToUpdate.ValueSpread.Value : 0;
								thisLT.PercentSpread = 0;
							}
						}
						else
						{
							// Hours spread.
							if (thisLT.ValueSpread != laborTypeToUpdate.ValueSpread)
							{
								thisLT.ValueSpread = laborTypeToUpdate.ValueSpread;
								thisLT.PercentSpread = ((laborTypeToUpdate.ValueSpread / MOQtotal) * 100);
							}
							else if (thisLT.PercentSpread != laborTypeToUpdate.PercentSpread)
							{
								thisLT.PercentSpread = laborTypeToUpdate.PercentSpread;
								thisLT.ValueSpread = Utilities.AdjustPrecision((((thisLT.PercentSpread ?? 0M) / 100) * MOQtotal), workspace.DecimalPrecision);
							}
						}
					}

					// Update the custom fields
					if (laborTypeToUpdate.CustomFieldValueContainers != null && laborTypeToUpdate.CustomFieldValueContainers.Any())
					{
						thisLT.CustomFieldValueContainers = laborTypeToUpdate.CustomFieldValueContainers.ToContainers();
					}

					// Update labor spreads from import
					UpdateLaborSpreadsFromImport(thisLT, laborTypeToUpdate.ImportedLaborSpreads, taskElementDTO, workspace);
				}

				//we need to add new ones to the list however we can't access them directly.
				Collection<ResourceTypeDto> toAppend = new Collection<ResourceTypeDto>();
				foreach (ResourceTypeDto lt in taskElementDTO.taskElementLabors)
				{
					toAppend.Add(lt);
				}

				// add all added ones
				ICollection<ImportLaborTypeModelView> laborTypesToAdd = (from i in importResults
																		 where i.ImportTypes.Contains((int)LaborTypeImportResult.AddLaborType)
																		 select i).ToList();

				foreach (ImportLaborTypeModelView laborTypeToAdd in laborTypesToAdd)
				{
					ResourceTypeDto newLT = new ResourceTypeDto();

					newLT.EndDateValue = laborTypeToAdd.EndDate;
					newLT.StartDateValue = laborTypeToAdd.StartDate;
					newLT.SpreadCurveID = (SpreadCurves)laborTypeToAdd.SpreadCurveID;
					newLT.ResourceID = laborTypeToAdd.ResourceID;
					newLT.BusinessResourceCodeID = laborTypeToAdd.BusinessResourceCodeID;
					newLT.PerformingOrgID = laborTypeToAdd.PerformingOrgID;
					newLT.Updateable = UpdateType.Upsert;
					newLT.PercentSpreadLocked = laborTypeToAdd.PercentSpreadLocked;
					newLT.HourSpreadLocked = laborTypeToAdd.HourSpreadLocked;
					newLT.CLINID = laborTypeToAdd.ClinID;
					newLT.WBSID = laborTypeToAdd.WbsID;
					newLT.CanOffload = laborTypeToAdd.CanOffload;
					newLT.TieredPercentage = laborTypeToAdd.TieredPercentage;
					newLT.CustomFieldValueContainers = laborTypeToAdd.CustomFieldValueContainers.ToContainers();

					// Determine the spread type based on the rate type of the resource.
					RateType rateType;
					ResourceDTO resourceForLabor = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, false, workspace.Shortname).FirstOrDefault(r => r.Id == laborTypeToAdd.ResourceID);
					ResourceDTO brcForLabor = null;

					if (Utilities.IsBRCEnabledForWorkspace(workspace.Shortname))
					{
						brcForLabor = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, true, workspace.Shortname).FirstOrDefault(r => r.Id == laborTypeToAdd.BusinessResourceCodeID);
					}

					// Rate Type Validation not needed here as the import already does it
					rateType = resourceForLabor?.RateType ?? brcForLabor.RateType;
					newLT.SpreadType = rateType == RateType.Cost ? SpreadType.Cost : SpreadType.Hours;

					if (newLT.SpreadCurveID != SpreadCurves.DiscreteHours && newLT.SpreadType == SpreadType.Hours)
					{
						newLT.PercentSpread = 0;
						newLT.ValueSpread = 0;

						// Hours are being spread over a curve.
						if (laborTypeToAdd.HourSpreadLocked)
						{
							// do not mess with hours since they are locked, only update percent spread

							if (laborTypeToAdd.ValueSpread.HasValue)
							{
								newLT.ValueSpread = laborTypeToAdd.ValueSpread;


								if (MOQtotal != 0)
								{
									newLT.PercentSpread = ((laborTypeToAdd.ValueSpread / MOQtotal) * 100);
								}
							}
						}
						else
						{
							// percent locked
							if (laborTypeToAdd.PercentSpread.HasValue)
							{
								newLT.PercentSpread = laborTypeToAdd.PercentSpread;
								newLT.ValueSpread = Utilities.AdjustPrecision((((newLT.PercentSpread ?? 0M) / 100) * MOQtotal), workspace.DecimalPrecision);
							}
						}
					}
					else if (newLT.SpreadCurveID != SpreadCurves.DiscreteCost && newLT.SpreadType == SpreadType.Cost)
					{
						// Costs are being spread over a curve.
						newLT.PercentSpread = 0;
						newLT.ValueSpread = laborTypeToAdd.ValueSpread;
					}
					else
					{
						// When we have a new Discrete resource type, set these values to zero regardless of the value that was imported.
						newLT.PercentSpread = 0;
						newLT.ValueSpread = 0;
					}

					// Update labor spreads from import for Discrete only, other Spread Curve types will be auto generated later
					if (newLT.SpreadCurveID == SpreadCurves.DiscreteCost || newLT.SpreadCurveID == SpreadCurves.DiscreteHours)
					{
						UpdateLaborSpreadsFromImport(newLT, laborTypeToAdd.ImportedLaborSpreads, taskElementDTO, workspace);
					}

					toAppend.Add(newLT);
				}

				//make appended List the new List
				taskElementDTO.taskElementLabors = toAppend;

				_BoeTaskElementRecalculation.AdjustLaborTotals(taskElementDTO, MOQtotal);

				//update all LSs on changed LTs
				_BoeTaskElementRecalculation.AddRecalulatedLaborSpreadsOntoLaborTypes((from changedLTs in taskElementDTO.taskElementLabors where changedLTs.Updateable == UpdateType.Upsert select changedLTs).ToList(), workspace);

				// Load the RTE data
				taskElementDTO.LoadRTEFields();
				taskElementDTO.Updateable = UpdateType.Upsert;

				Collection<BoeTaskElementDTO> toSave = new Collection<BoeTaskElementDTO>();
				toSave.Add(taskElementDTO);

				// Save the updates
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					// Save the Task Element
					this.taskElementDataLoader.BulkSave(toSave);

					scope.Complete();
				}

				// Process task variable and workspace variable dependencies
				this._BoeLaborControllerLogic.ProcessAllVariableDependencies(boeID, workspace);
			}
		}

		/// <summary>
		/// Retrieves display data for the boe sum by wbs page
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeID">Optional BOE ID</param>
		/// <returns>model view</returns>
		private ICollection<VariableBOESumByWBSModelView> DisplayVariableBOESumByWBSModelViews(string workspace, int? boeID)
		{
			Collection<VariableBOESumByWBSModelView> toReturn = new Collection<VariableBOESumByWBSModelView>();

			// Get the workspace for the given short name
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Get all WBSs for the specified workspace
			IReadOnlyCollection<FullWbs> WBSs = ws.WbsElementsNoMultiWbs;

			// Get all BOEs in the workspace
			IEnumerable<FullBoe> BOEs = ws.Boes.Where(b => !b.IsMultiClinWbs);


			// Associate each BOE with its WBS
			var boesForWBSs = (from w in WBSs
							   from b in BOEs
							   where b.WBSID.HasValue &&
									 b.WBSID == w.Id
							   select new
							   {
								   BOE = b,
								   WBS = w
							   }).ToList();

			bool clipping = false;
			string clippingWBSNumberPrefix = string.Empty;
			IDictionary<int, BOEStateModelView> boeStates = this._CommonDataMapper.getBOEStatesDictionary();

			// Iterate over each WBS in the workspace
			foreach (FullWbs WBS in WBSs)
			{
				// Get all BOEs for the current WBS
				var boesForWBS = from b in boesForWBSs
								 where b.WBS.Id == WBS.Id
								 select b;

				if (clipping && !(WBS.WbsNumber.StartsWith(clippingWBSNumberPrefix, StringComparison.CurrentCultureIgnoreCase)))
				{
					clipping = false;
					clippingWBSNumberPrefix = string.Empty;
				}

				if (!clipping)
				{
					if (!boesForWBS.Any())
					{
						// Create a ModelView to represent the summary WBS row in the View's table
						VariableBOESumByWBSModelView modelView = new VariableBOESumByWBSModelView
						{
							WBSID = WBS.Id,
							WBSLevel = WBS.Level,
							WBSNumber = WBS.WbsNumber,
							WBSName = WBS.WbsTitle,
							WorkspaceDecimalPrecision = ws.DecimalPrecision
						};

						// Add the summary WBS ModelView to the collection that will be passed to the View
						toReturn.Add(modelView);
					}
					else
					{
						// Set values to indicate that nothing directly below this WBS should be shown
						clipping = true;
						clippingWBSNumberPrefix = WBS.WbsNumber + '.';

						// Get the WBS row that these BOEs will be nested under
						VariableBOESumByWBSModelView modelViewToAppend = (from m in toReturn
																		  where m.WBSLevel < WBS.Level &&
																				WBS.WbsNumber.StartsWith(m.WBSNumber + ".", StringComparison.CurrentCultureIgnoreCase)
																		  select m).LastOrDefault();

						// Iterate over each BOE for the current WBS
						foreach (var boeForWBS in boesForWBS)
						{
							// Create a ModelView to represent the nested BOE in the View's table
							VariableBOESumBOEElement nestedModelView = new VariableBOESumBOEElement
							{
								BOEID = boeForWBS.BOE.Id,
								BOEStatus = boeStates[(int)boeForWBS.BOE.State].BOEState,
								WBSTitle = boeForWBS.WBS.WbsString,
								CLINTitle = boeForWBS.BOE.CLINID.HasValue ? ws.Clins.First(i => i.Id == boeForWBS.BOE.CLINID).ClinString : "None",
								Disabled = boeID.HasValue && _VariableCircularReferenceChecker.BOECreatesCircularReference(null, boeID.Value, boeForWBS.BOE, ws)
							};

							// If there is no previous modelView with a level lower than
							// this BOE's WBS, we'll add it to an invisible ModelView. This
							// will ensure that BOEs that are associated with a Level 1 WBS
							// get shown at the root of the View.
							if (modelViewToAppend == null)
							{
								VariableBOESumByWBSModelView invisibleModelView = new VariableBOESumByWBSModelView
								{
									WBSID = -1,
									WBSNumber = WBS.WbsNumber,
									WBSLevel = WBS.Level,
									WorkspaceDecimalPrecision = ws.DecimalPrecision
								};

								invisibleModelView.BOEs.Add(nestedModelView);
								toReturn.Add(invisibleModelView);
							}
							// Nest the BOE under the last WBS of a level lower than this BOE's WBS
							else
							{
								// Add the nested BOE ModelView to the WBS ModelView
								modelViewToAppend.BOEs.Add(nestedModelView);
							}
						}
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Retrieves total data for the boe sum by wbs page
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeID">Optional BOE ID</param>
		/// <param name="resourceTypes">resources checked</param>
		/// <returns>model view</returns>
		private ICollection<VariableBOESumByWBSModelView> RefreshVariableBOESumByWBSModelViews(FullWorkspace ws, ICollection<SumVariableResourceType> resourceTypes)
		{
			Collection<VariableBOESumByWBSModelView> toReturn = new Collection<VariableBOESumByWBSModelView>();

			if (resourceTypes == null)
			{
				resourceTypes = new List<SumVariableResourceType>();
			}

			// Get all WBSs for the specified workspace
			IReadOnlyCollection<FullWbs> WBSs = ws.WbsElementsNoMultiWbs;

			// Get all BOEs in the workspace
			IEnumerable<FullBoe> BOEs = ws.Boes.Where(b => !b.IsMultiClinWbs);

			// Associate each BOE with its WBS

			var boesForWBSs = (from w in WBSs
							   from b in BOEs
							   where b.WBSID.HasValue &&
									 b.WBSID == w.Id
							   select new
							   {
								   BOE = b,
								   WBS = w
							   }).ToList();

			bool clipping = false;
			string clippingWBSNumberPrefix = string.Empty;

			// Iterate over each WBS in the workspace

			foreach (FullWbs WBS in WBSs)
			{
				// Get all BOEs for the current WBS
				var boesForWBS = from b in boesForWBSs
								 where b.WBS.Id == WBS.Id
								 select b;

				if (clipping && !(WBS.WbsNumber.StartsWith(clippingWBSNumberPrefix, StringComparison.CurrentCultureIgnoreCase)))
				{
					clipping = false;
					clippingWBSNumberPrefix = string.Empty;
				}

				if (!clipping)
				{
					if (!boesForWBS.Any())
					{
						DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
						data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { WBSID = WBS.Id } } } }, null, ws);
						// Create a ModelView to represent the summary WBS row in the View's table
						VariableBOESumByWBSModelView modelView = new VariableBOESumByWBSModelView
						{
							WBSID = WBS.Id,

							WBSTotal = _VariableSelectBOEtoSumCalculation.GetTotalBasedOnWBSID(
	WBS.Id,
	new Collection<int>(resourceTypes
							.Select(r => (int)r)
							.ToArray()), data),
							WorkspaceDecimalPrecision = ws.DecimalPrecision
						};

						// Add the summary WBS ModelView to the collection that will be passed to the View
						toReturn.Add(modelView);
					}
					else
					{
						// Set values to indicate that nothing directly below this WBS should be shown
						clipping = true;
						clippingWBSNumberPrefix = WBS.WbsNumber + '.';

						// Get the WBS row that these BOEs will be nested under
						VariableBOESumByWBSModelView modelViewToAppend = (from m in toReturn
																		  where m.WBSLevel < WBS.Level &&
																				WBS.WbsNumber.StartsWith(m.WBSNumber + ".", StringComparison.CurrentCultureIgnoreCase)
																		  select m).LastOrDefault();

						// Iterate over each BOE for the current WBS
						foreach (var boeForWBS in boesForWBS)
						{
							DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
							data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = boeForWBS.BOE.Id } } } },
								new List<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = boeForWBS.BOE.Id } } } }, ws);

							// Create a ModelView to represent the nested BOE in the View's table
							VariableBOESumBOEElement nestedModelView = new VariableBOESumBOEElement
							{
								BOEID = boeForWBS.BOE.Id,
								BOETotal = _VariableSelectBOEtoSumCalculation.GetTotalBasedOnBoeID(boeForWBS.BOE.Id, new Collection<int>(resourceTypes.Select(r => (int)r).ToArray()), data)
							};

							// If there is no previous modelView with a level lower than
							// this BOE's WBS, we'll add it to an invisible ModelView. This
							// will ensure that BOEs that are associated with a Level 1 WBS
							// get shown at the root of the View.
							if (modelViewToAppend == null)
							{
								VariableBOESumByWBSModelView invisibleModelView = new VariableBOESumByWBSModelView
								{
									WBSID = boeForWBS.BOE.WBSID.HasValue ? boeForWBS.BOE.WBSID.Value : -1,
									WorkspaceDecimalPrecision = ws.DecimalPrecision
								};

								invisibleModelView.BOEs.Add(nestedModelView);
								toReturn.Add(invisibleModelView);
							}
							// Nest the BOE under the last WBS of a level lower than this BOE's WBS
							else
							{
								// Add the nested BOE ModelView to the WBS ModelView
								modelViewToAppend.BOEs.Add(nestedModelView);
							}
						}
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Calculates sums for CLINs and BOEs based on a given list of resource types
		/// </summary>
		/// <param name="workspace">Workspace containing the current BOE</param>
		/// <param name="resourceTypes">Resource types to use for calculation</param>
		/// <returns>model views</returns>
		private ICollection<VariableBOESumByCLINModelView> RefreshVariableBOESumByCLINModelViews(
			FullWorkspace workspace,
			ICollection<SumVariableResourceType> resourceTypes)
		{
			Collection<VariableBOESumByCLINModelView> toReturn = new Collection<VariableBOESumByCLINModelView>();

			if (resourceTypes == null)
			{
				resourceTypes = new List<SumVariableResourceType>();
			}

			// Get all CLINs in the workspace
			IReadOnlyCollection<FullClin> CLINs = workspace.ClinsNoMultiClin;

			// Get all BOEs in the workspace
			ICollection<FullBoe> BOEs = workspace.Boes.Where(b => !b.IsMultiClinWbs).ToList();

			// Associate each BOE with its CLIN
			var boesForCLINs = (from c in CLINs
								from b in BOEs
								where b.CLINID.HasValue &&
									  b.CLINID == c.Id
								select new
								{
									BOE = b,
									CLIN = c
								}).ToList();

			// Iterate over each CLIN in the workspace
			foreach (ClinDTO CLIN in CLINs)
			{
				// Create a ModelView to represent the summary CLIN row in the View's table
				VariableBOESumByCLINModelView modelView = new VariableBOESumByCLINModelView
				{
					CLINID = CLIN.Id,
					CLINTitle = CLIN.ClinString,
					WorkspaceDecimalPrecision = workspace.DecimalPrecision
				};

				// Get all BOEs for the current CLIN
				var boesForCLIN = (from b in boesForCLINs
								   where b.CLIN.Id == CLIN.Id
								   select b).ToList();

				// Iterate over each BOE for the current CLIN
				foreach (var boeForCLIN in boesForCLIN)
				{
					DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
					data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = boeForCLIN.BOE.Id } } } },
						new List<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = boeForCLIN.BOE.Id } } } }, workspace);

					// Create a ModelView to represent the nested BOE in the View's table
					VariableBOESumBOEElement nestedModelView = new VariableBOESumBOEElement
					{
						BOEID = boeForCLIN.BOE.Id,
						BOETotal = _VariableSelectBOEtoSumCalculation.GetTotalBasedOnBoeID(boeForCLIN.BOE.Id, new Collection<int>(resourceTypes.Select(r => (int)r).ToArray()), data)
					};

					// Add the nested BOE ModelView to the CLIN ModelView
					modelView.BOEs.Add(nestedModelView);
				}

				// Add the summary CLIN ModelView to the collection that will be passed to the View
				toReturn.Add(modelView);
			}

			return toReturn;
		}

		/// <summary>
		/// Creates a MOQ equation model view for a MOQ equation being copied from another BOE.
		/// </summary>
		/// <param name="workspaceId">Workspace Id.</param>
		/// <param name="boeId">BOE Id.</param>
		/// <param name="copyTaskElement">Task element containing a copy of the new MOQ equation.</param>
		/// <param name="destinationTaskElementId">Task element Id of the task element the equation is being copied to.</param>
		/// <returns>MOQEquationModelView for the copied MOQ Equation.</returns>
		private MOQEquationModelView GetCopyMoqEquationModelView(int workspaceId, int boeId, BoeTaskElementDTO copyTaskElement, int destinationTaskElementId)
		{
			FullWorkspace workspace = this.Factory.CreateFullWorkspace(workspaceId);
			if (copyTaskElement == null)
			{
				throw new ArgumentNullException(nameof(copyTaskElement));
			}

			List<WorkspaceVariableDTO> inUseWorkspaceVariables = (from wID in copyTaskElement.WorkspaceVariableIDs
																  from workspaceVariable in workspace.WorkspaceVariables
																  where workspaceVariable.Id == wID
																  select workspaceVariable).ToList();

			int originalSourceTaskElementId = copyTaskElement.Id;
			// Reset the IDs in the task element to jive with the destination task element.
			copyTaskElement.BoeID = boeId;
			copyTaskElement.Id = destinationTaskElementId;
			copyTaskElement.MOQHoursEquation = ActionLogic.Common.MOQ.Parser.UntagVariables(copyTaskElement.MOQHoursEquation, inUseWorkspaceVariables);

			if (destinationTaskElementId > 0)
			{
				// Reconcile the differences in the task variables from the old equation to the new equation.
				BoeTaskElementDTO destinationTaskElement = this.Factory.CreateTaskElement(destinationTaskElementId, workspace.DecimalPrecision, workspace.CostDecimalPrecision);
				if (destinationTaskElement.OrdinaryVariables.Any())
				{
					List<OrdinaryVariableDto> taskOrdinaryVariables = new List<OrdinaryVariableDto>(copyTaskElement.OrdinaryVariables);
					foreach (OrdinaryVariableDto ordinaryVariable in destinationTaskElement.OrdinaryVariables)
					{
						OrdinaryVariableDto match = taskOrdinaryVariables.FirstOrDefault(v => v.OrdinaryVariableName == ordinaryVariable.OrdinaryVariableName);
						if (match == null)
						{
							// This variable was in the equation prior to the copy.  We need to force a deletion of this variable.
							ordinaryVariable.Updateable = UpdateType.Deleted;
							taskOrdinaryVariables.Add(ordinaryVariable);
						}
						else
						{
							// The same variable name was found in both the source and destination.  Get the Id from the source and add to
							// the destination variable so it is updated properly on save of the task element.
							int index = taskOrdinaryVariables.IndexOf(match);
							taskOrdinaryVariables[index].Id = ordinaryVariable.Id;
							taskOrdinaryVariables[index].UpdateDate = ordinaryVariable.UpdateDate;
						}
					}
					copyTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto>(taskOrdinaryVariables.ToArray());
				}
			}

			MOQEquationModelView modelView = new MOQEquationModelView(copyTaskElement, _VariableSelectBOEtoSumCalculation, workspace);
			modelView.MOQTextLabel = _BoeLaborControllerLogic.GetMOQTextLabel();

			_BoeLaborControllerLogic.GetMetricByTaskElementIds(new Collection<int> { originalSourceTaskElementId }, modelView);

			return modelView;
		}

		private MOQEquationModelView CreateMOQModelView(FullWorkspace ws, FullBoe boe, int taskElementID)
		{
			MOQEquationModelView theModelView = new MOQEquationModelView();

			if (taskElementID > 0)
			{
				BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(taskElementID, ws.DecimalPrecision, ws.CostDecimalPrecision);
				SetMOQEquationViewData(ws, boe.Id, taskElement.MOQType);
				DataRelationshipVerifier.VerifyDataRelation(taskElement, boe.Id);
				List<WorkspaceVariableDTO> inUseWorkspaceVariables = (from wID in taskElement.WorkspaceVariableIDs
																	  from workspaceVariable in ws.WorkspaceVariables
																	  where workspaceVariable.Id == wID
																	  select workspaceVariable).ToList();

				taskElement.MOQHoursEquation = Parser.UntagVariables(taskElement.MOQHoursEquation, inUseWorkspaceVariables);

				theModelView = _BoeLaborControllerLogic.GetMOQModelView(taskElement, ws);
			}
			else
			{
				SetMOQEquationViewData(ws, boe.Id, MOQType.None);
				_BoeLaborControllerLogic.SetShowMetricLink(theModelView);
				theModelView.MoqTemplateAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(ws.Id, boe.Id, taskElementID).Where(t => t.SourceId == (int)RteTemplateSource.TaskMOQ).ToList();
			}

			theModelView.HelpText = _BoeLaborControllerLogic.GetMOQTypesHelpText();
			theModelView.MOQTextLabel = _BoeLaborControllerLogic.GetMOQTextLabel();
			theModelView.UsingTemplateBOE = ws.UsingTemplateBOE;
			theModelView.MOQTypes = this._BoeLaborControllerLogic.GetMOQTypeSelectList(ws.CreationDate >= moqTemplateUsageStartDate, null);

			if (ws.UsingTemplateBOE)
			{
				theModelView.MoqTypeTableDataLabels = this._BoeLaborControllerLogic.GetMoqTypeLabels();
				theModelView.SelectedMoqTypes = boe.MoqTypeSelections.Where(x => x.TaskId == taskElementID).ToList();
				theModelView.MoqTypeHelpUrls = this._BoeLaborControllerLogic.GetMoqTypeHelpUrls();
			}

			return theModelView;
		}

		/// <summary>
		/// Sets ViewData required for the MOQEquationField view.
		/// </summary>
		/// <param name="workspace">Full workspace object.</param>
		/// <param name="boeId">BOE Container Id.</param>
		/// <param name="moqType">moqType for the task element.</param>
		private void SetMOQEquationViewData(FullWorkspace workspace, int boeId, MOQType moqType)
		{
			bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id)
										where p.Role == Role.SubcontractorAuthor && p.ETIUserId == workspace.CurrentActiveUser.UserID
										select p).Any();

			VariableCircularReferenceCheckerCache circularReferenceCache = new VariableCircularReferenceCheckerCache();
			ICollection<WorkspaceVariableModelView> wsVariables = workspace.WorkspaceVariables.Select(x =>
				new WorkspaceVariableModelView(x, _VariableSelectBOEtoSumCalculation, workspace)
				{
					Disabled = _VariableCircularReferenceChecker.WorkspaceVariableCreatesCircularReference(circularReferenceCache, boeId, x, workspace)
				}).ToList();

			ViewBag.IsSubContractor = isSubcontractorUser;
			ViewBag.BOEID = boeId;
			ViewBag.WorkspaceVariables = wsVariables;
			ViewBag.RteFieldSize = workspace.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;

			List<SelectListItem> moqTypeSelects = this._BoeLaborControllerLogic.GetMOQTypeSelectList(workspace.CreationDate >= moqTemplateUsageStartDate, moqType).ToList();
			moqTypeSelects.Insert(0, new SelectListItem() { Value = "0", Text = string.Empty });
			ViewBag.MOQTypes = this.ConvertToOptionList(moqTypeSelects);
		}

		#endregion
	}
}

