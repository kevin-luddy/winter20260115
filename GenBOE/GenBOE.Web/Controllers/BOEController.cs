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
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.DTO.SkillMix;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;
	using Microsoft.VisualBasic.Logging;

	public class BOEController : GenBOEController
	{
		private Logger _log = new Logger(typeof(BOEController));
		private BoeEmailer _emailer = null;
		private BOEStateMachine _boeStateMachine = null;
		private WorkspaceVariableDTODataLoader _workspaceVariableLoader = null;
		private IValidateBOE _validateBOE = null;
		private IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation = null;
		private BoeTaskElementRecalculation _BoeTaskElementRecalculation = null;
		private VariableCircularReferenceChecker _VariableCircularReferenceChecker = null;
		private ActionLogic.CopyBOE.BOECopier _BOECopier = null;
		private BoeApproverResponseDTODataLoader _BoeApproverResponseLoader = null;
		private IMaterialDTODataLoader _MaterialLoader = null;
		private BoeTaskElementMediator _BoeTaskElementMediator = null;
		private BoeMediator _BoeMediator = null;
		private IBOEControllerLogic _ControllerLogic = null;
		private IBOELaborControllerLogic _BoeLaborControllerLogic = null;
		private IWbsDTODataLoader wbsLoader;
		private ITravelControllerLogic _TravelControllerLogic = null;
		private ITravelDTODataLoader _TravelDTOLoader = null;
		private readonly IRteTemplateDataLoader rteTemplateDataLoader;
		/// <summary>
		/// Task Element Validation Class
		/// </summary>
		private TaskElementValidation taskElementValidation { get; set; }

		/// <summary>
		/// The version loader.
		/// </summary>
		private IWorkspaceVersionMetaDataDTODataLoader versionLoader { get; set; }

		/// <summary>
		/// Business logic for the reports controller
		/// </summary>
		private IReportsControllerLogic reportsControllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		public BOEController(ISecurityAccess inSecurityAccess,
			CommonDataMapper inCommonDataMapper,
			SiteMasterUtilities inSiteMasterUtilities,
			UserDTODataLoader inUserDataLoader,
			IPermissionsDTODataLoader inPermissionsLoader,
			BOEStateMachine inBoeStateMachine,
			BoeEmailer inEmailer,
			WorkspaceVariableDTODataLoader inWorkspaceVariabledDTODataLoader,
			IValidateBOE inValidateBOE,
			IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
			BoeTaskElementRecalculation inBoeTaskElementRecalculation,
			VariableCircularReferenceChecker inVariableCircularReferenceChecker,
			ActionLogic.CopyBOE.BOECopier inBOECopier,
			BoeApproverResponseDTODataLoader inBoeApproverLoader,
			IMaterialDTODataLoader inMaterialLoader,
			BoeTaskElementMediator inBoeTaskElementMediator,
			BoeMediator inBoeMediator,
			SystemMetrics inSystemMetrics,
			IBOEControllerLogic inBOEControllerLogic,
			IBOELaborControllerLogic inBoeLaborControllerLogic,
			IFullObjectFactory factory,
			IReportsControllerLogic inReportsControllerLogic,
			IWbsDTODataLoader wbsLoader,
			IGenBOEControllerLogic inControllerLogic,
			ITravelControllerLogic inTravelControllerLogic,
			TaskElementValidation taskElementValidation,
			ITravelDTODataLoader travelLoader,
			IWorkspaceVersionMetaDataDTODataLoader versionLoader,
			IRteTemplateDataLoader rteTemplateDataLoader)
		  : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDataLoader, inPermissionsLoader, inControllerLogic)
		{
			_emailer = inEmailer;
			_boeStateMachine = inBoeStateMachine;
			_workspaceVariableLoader = inWorkspaceVariabledDTODataLoader;
			_validateBOE = inValidateBOE;
			_variableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
			_BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
			_VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
			_BOECopier = inBOECopier;
			_BoeApproverResponseLoader = inBoeApproverLoader;
			_MaterialLoader = inMaterialLoader;
			_BoeTaskElementMediator = inBoeTaskElementMediator;
			_BoeMediator = inBoeMediator;
			_ControllerLogic = inBOEControllerLogic;
			_BoeLaborControllerLogic = inBoeLaborControllerLogic;
			this.reportsControllerLogic = inReportsControllerLogic;
			this.wbsLoader = wbsLoader;
			this._TravelControllerLogic = inTravelControllerLogic;
			this.taskElementValidation = taskElementValidation;
			this._TravelDTOLoader = travelLoader;
			this.versionLoader = versionLoader;
			this.rteTemplateDataLoader = rteTemplateDataLoader;
		}

		#region Display

		#region Views

		/// <summary>
		/// Returns the Edit BOE Index View
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <returns></returns>
		public async Task<ViewResult> EditBOEIndex(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "EditBOEIndex", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			// Perform Action
			this.ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;
			this.ViewData["BOEID"] = boeID;
			ViewData["UcotFactor"] = ws.UCOTFactor / 100.0m;
			ViewData["SkillMixTableHelpUrls"] = new SkillMixTableHelpUrls();
			this.ViewData["ElementsOfCost"] = this._CommonDataMapper.GetElementOfCostTypes().Where(
				x => x.ElementOfCostId == (int)ElementOfCostType.LMLabor ||
					x.ElementOfCostId == (int)ElementOfCostType.IWTA ||
					x.ElementOfCostId == (int)ElementOfCostType.Materials ||
					x.ElementOfCostId == (int)ElementOfCostType.ODC ||
					x.ElementOfCostId == (int)ElementOfCostType.Sub ||
					x.ElementOfCostId == (int)ElementOfCostType.Travel).ToList();
			ViewData["EnableSAP"] = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			this.ViewData["IsSkillMixEnabled"] = Utilities.ShowSkillMixForWorkspace(ws.CreationDate);


			Collection<SelectListItem> WBSElements = new Collection<SelectListItem>((from x in ws.WbsElementsNoMultiWbs
											 orderby x.WbsPaddedNumber
											 select new SelectListItem()
											 {
		 Text = x.WbsString,
		 Value = x.Id.ToString()
											 }).ToList());
			WBSElements.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });

			Collection<SelectListItem> ClinElements = new Collection<SelectListItem>((from x in ws.ClinsNoMultiClin
											  orderby x.ClinPaddedNumber
											  select new SelectListItem()
											  {
		  Text = x.ClinString,
		  Value = x.Id.ToString()
											  }).ToList());
			ClinElements.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });

			string hoursLabel = FullObjectHelper.HoursLabel(ws);
			Collection<SelectListItem> costCurves = new Collection<SelectListItem>((from x in this._BoeLaborControllerLogic.GetSpreadCurves(RateType.Cost)
											select new SelectListItem()
											{
		Text = x.GetDescription().Replace("Hours", hoursLabel),
		Value = ((int)x).ToString()
											}).ToList());
			costCurves.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });


			Collection<SelectListItem> hourCurves = new Collection<SelectListItem>((from x in this._BoeLaborControllerLogic.GetSpreadCurves(RateType.Hours)
											select new SelectListItem()
											{
		Text = x.GetDescription().Replace("Hours", hoursLabel),
		Value = ((int)x).ToString()
											}).ToList());
			hourCurves.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });

			this.ViewData["WBSElements"] = WBSElements;
			this.ViewData["CLINElements"] = ClinElements;
			this.ViewData["SpreadCurvesCost"] = costCurves;
			this.ViewData["SpreadCurvesHours"] = hourCurves;
			this.ViewData["CostDecimalPrecision"] = ws.CostDecimalPrecision;
			this.ViewData["DecimalPrecision"] = ws.DecimalPrecision;
			this.ViewData["SapFields"] = await _ControllerLogic.GetAllFields();
			this.ViewData["SapOperators"] = await _ControllerLogic.GetAllOperators();
			this.ViewData["EnableSAPConnection"] = ws.EnableSAPConnection;

			ViewResult toReturn = this.GetMasterView(WebConstants.VIEW_EDIT_BOE_INDEX, workspace);

			// Finalize Action
			this.FinalizeAction(this._log, "EditBOEIndex", sw);
			return toReturn;

		}

		/// <summary>
		/// The initial view for Managing BOE Calls DisplayManageBOEGrid
		/// </summary>
		/// <param name="workspace">Workspace short name</param>
		/// <returns>a view</returns>
		public virtual ActionResult Index(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "Index", SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);

			ViewResult toReturn = null;

			ViewData["WorkspaceID"] = ws.Id;

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				// Perform Action
				toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "Index", sw);
			return toReturn;
		}

		#endregion Views

		#region Partial Views

		/// <summary>
		/// Populates the Duplicate Task Element dialog with task data for a BOE
		/// </summary>
		/// <param name="workspace">Current Workspace</param>
		/// <param name="boeID">Current BOE</param>
		/// <param name="taskType">Type of tasks to display</param>
		/// <returns>Populated Duplicate Task Element View</returns>
		public ViewResult LoadDuplicateTaskDialog(string workspace, int boeID, TaskType taskType)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boeObject = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "LoadDuplicateTaskDialog", SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			// Perform Action
			ViewData["BOEID"] = boeID;
			TaskElementDuplicateFormCollection theModelView = _ControllerLogic.GetDuplicateTaskModelView(boeObject, taskType);

			theModelView.ContainsOCI = ws.ContainsOCI;

			// Perform Action
			ViewResult toReturn = View(WebConstants.VIEW_DUPLICATE_TASK_DIALOG, theModelView);

			// Finalize Action
			FinalizeAction(_log, "LoadDuplicateTaskDialog", sw);

			return toReturn;
		}

		#region Edit BOE

		/// <summary>
		/// Displays the Task Element Grid Partial View
		/// </summary>
		/// <param name="workspace">Workspace Shortname</param>
		/// <param name="boeID">BOE ID</param>
		/// <returns>DisplayTaskElementGrid ActionResult</returns>
		public virtual ViewResult DisplayTaskElementGrid(string workspace, int boeID)
		{
			FullWorkspace workspaceObject = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = workspaceObject.Boes.First(x => x.Id == boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayTaskElementGrid", SecurityPage.BOELaborGrid, SecurityAuthorization.Read, workspaceObject, boeID);

			// Perform Action
			ViewData["BOEID"] = boeID;
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(workspaceObject);

			GenericTaskElementGridModelView theModelView = _ControllerLogic.GetTaskGridModelView(boe, workspaceObject);

			theModelView.TaskElements = theModelView.TaskElements.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.TaskElementDetailID).ToCollection();
			theModelView.ContainsOCI = workspaceObject.ContainsOCI;

			ViewData["DISABLE_ALLOCATED_LABOR"] = false;

			bool disableAddButtons = CheckPermissions(SecurityPage.BOELaborGrid, workspaceObject, boeID) != SecurityAuthorization.CreateReadUpdateDelete;

			if (disableAddButtons)
			{
				ViewData["DISABLE_ALLOCATED_LABOR"] = true;
			}

			//create a var for list items
			Collection<SelectListItem> orderOfTaskElements = new Collection<SelectListItem>();
			//get a list of each task element.
			foreach (GenericTaskElementGridRow row in theModelView.TaskElements)
			{
				orderOfTaskElements.Add(new SelectListItem { Text = row.TaskID + " " + row.Title, Value = row.TaskElementDetailID.ToString() });
			}

			ViewData["Order_Of_TaskElements"] = orderOfTaskElements;

			//if start date before and end date after, must have resource and brc, if both after, must have brc
			//set view data for task grid to disable "add task element" and "duplicate task"
			if (Utilities.IsBRCEnabledForWorkspace(workspace) && boe.EndDate >= Utilities.OneLmxStartDate)
			{
				ICollection<ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(workspaceObject.ResourcesForWsResourceListId.ToList(), true, workspace);
				//if no brcs in ws, display warning
				if (resources.Count == 0)
				{
					ViewData["MissingBrcCodes"] = true;
				}
			}

			sw.Stop();
			_log.Performance("Finished BOEController.DisplayTaskElementGrid.", sw.ElapsedMilliseconds);

			#region Validate Task Elements

			ICollection<int> invalidTaskElementIds = this.taskElementValidation.GetInvalidTaskElementIds(workspaceObject, boe.TaskElements);
			// Update the failed task elements w/ the info about it..
			theModelView.TaskElements.AsParallel()
				.Where(x => x.TaskElementDetailID.HasValue && invalidTaskElementIds.Contains(x.TaskElementDetailID.Value))
				.ForAll(z => z.FailedValidation = true);

			#endregion

			ViewResult toReturn = View(WebConstants.VIEW_TASK_ELEMENT_GRID, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayTaskElementGrid", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the Validation Results view
		/// </summary>
		/// <param name="workspace">Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>DisplayBOEValidateResults ActionResult</returns>
		public virtual ViewResult DisplayBOEValidateResults(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOEValidateResults", SecurityPage.ValidateBOE, SecurityAuthorization.Read, ws, boeID);

			ViewData["BOEID"] = boeID;

			ViewResult toReturn = View(WebConstants.VIEW_BOE_VALIDATION_RESULTS);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEValidateResults", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the boe offload results.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeId">The boe identifier.</param>
		/// <returns>DisplayBOEOffloadResults View.</returns>
		public virtual ViewResult DisplayBOEOffloadResults(string workspace, int boeId)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOEOffloadResults", SecurityPage.BOELaborGrid, SecurityAuthorization.Read, ws, boeId);

			BoeOffloadModelView model = this._ControllerLogic.RetrieveBoeOffloadData(ws, boeId);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_OFFLOAD_RESULTS, model);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEOffloadResults", sw);
			return toReturn;
		}

		/// <summary>
		/// Returns The BOE Header (upper portion)
		/// </summary>
		/// <param name="workspace">THe Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>The view for the BOE Header</returns>
		public virtual ViewResult DisplayBOEHeaderDescription(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOEHeaderDescription", SecurityPage.EditBOEHeaderDescription,
				SecurityAuthorization.Read, ws, boeID);

			ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boeID).Where(t => t.SourceId == (int)RteTemplateSource.BoeDescription).OrderBy(r => r.SortOrder).ToList();
			ViewResult toReturn = View(WebConstants.VIEW_BOE_HEADER_DESCRIPTION, _createBOEHeaderMVDescription(boe, rteTemplateAnswers));

			ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEHeaderDescription", sw);
			return toReturn;
		}

		/// <summary>
		/// Returns The BOE Header (lower portion)
		/// </summary>
		/// <param name="workspace">THe Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>The view for the BOE Header</returns>
		public virtual ViewResult DisplayBOEHeader(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = ws.Boes.First(x => x.Id == boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOEHeader", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["BOEID"] = boeID;
			ViewData["CustomFields"] = _ControllerLogic.GetCustomFieldModelViews(ws);
			ViewData["BOEState"] = (int)boe.State;
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);
			ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;

			SecurityAuthorization boeDateShiftAuthorization = CheckPermissions(SecurityPage.BoeTaskDates, ws, boeID);
			ViewData["ALLOW_DATE_SHIFT"] = (boeDateShiftAuthorization == SecurityAuthorization.CreateReadUpdateDelete);

			//if start date before and end date after, must have resource and brc, if both after, must have brc
			//set view data for boe header to display read only warning
			if (Utilities.IsBRCEnabledForWorkspace(workspace) && boe.EndDate >= Utilities.OneLmxStartDate)
			{
				ICollection<ResourceDTO> resources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(ws.ResourcesForWsResourceListId.ToList(), true, workspace);
				//if no brcs in ws, display warning
				if (resources.Count == 0)
				{
					ViewData["MissingBrcCodes"] = true;
				}
			}

			ViewResult toReturn = View(WebConstants.VIEW_BOE_HEADER, _ControllerLogic.CreateBOEHeaderMV(boe, ws));

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEHeader", sw);
			return toReturn;
		}

		private BOEHeaderDescriptionModelView _createBOEHeaderMVDescription(FullBoe boe, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers)
		{
			// Perform Action
			BOEHeaderDescriptionModelView theModelView = new BOEHeaderDescriptionModelView(boe, rteTemplateAnswers);
			ViewData["BOEID"] = boe.Id;

			return theModelView;
		}

		/// <summary>
		/// Displays the boe search.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeID">The boe identifier.</param>
		/// <returns>View containing BOE Search or nothing if no access.</returns>
		public virtual ViewResult DisplayBOESearch(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
									where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
									select p).Any();
			if (IsSubContractor)
			{
				return null;
			}

			Stopwatch sw = InitializeAction(_log, "DisplayBOESearch", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["BOEID"] = boeID;
			ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

			// Only an Author in WS=Working and BOE=Draft should be able to Search to copy a BOE. 
			ViewData["BOESearch_ReadOnly"] = (Boolean.Parse(GetReadOnlyAttribute(CheckPermissions(SecurityPage.BoeSearch, ws, boeID)))).ToString().ToLower();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_SEARCH);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOESearch", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the project map boe search button.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns>View containing BOE Search button or nothing if no access.</returns>
		public virtual ViewResult DisplayProjectMapBOESearch(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
									where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
									select p).Any();
			if (IsSubContractor)
			{
				return null;
			}

			Stopwatch sw = InitializeAction(_log, "DisplayProjectMapBOESearch", SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			// Only an Author/Admin in WS=Working should be able to Search to copy a BOE.
			ViewData["BOESearch_ReadOnly"] = (Boolean.Parse(GetReadOnlyAttribute(CheckPermissions(SecurityPage.ProjectMapBoeSearch, ws, null)))).ToString().ToLower();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_PROJECTMAP_SEARCH);

			// Finalize Action
			FinalizeAction(_log, "DisplayProjectMapBOESearch", sw);
			return toReturn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <returns></returns>
		public virtual ViewResult DisplayBOEAdvancedSearch(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(_log, "DisplayBOEAdvancedSearch", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			ViewData["WSPERFORGS"] = _BoeLaborControllerLogic.GetPerformingOrgs(ws);
			BOEAdvancedSearchModelView theModelView = new BOEAdvancedSearchModelView();
			_ControllerLogic.PopulateCompanySpecificProperties(theModelView);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_ADVANCED_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEAdvancedSearch", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the boe project map advanced search.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns></returns>
		public virtual ViewResult DisplayBOEProjectMapAdvancedSearch(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(_log, "DisplayBOEProjectMapAdvancedSearch", SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.Read, ws, null);
			ViewData["WSPERFORGS"] = _BoeLaborControllerLogic.GetPerformingOrgs(ws);
			BOEAdvancedSearchModelView theModelView = new BOEAdvancedSearchModelView();
			_ControllerLogic.PopulateCompanySpecificProperties(theModelView);
			theModelView.IsProjectMapDiscrete = ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap;
			ViewResult toReturn = View(WebConstants.VIEW_BOE_PROJECTMAP_ADVANCED_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEProjectMapAdvancedSearch", sw);
			return toReturn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <returns></returns>
		public virtual ViewResult DisplayBOEQuickSearch(string workspace, int? boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			SecurityPage securityPage = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, "DisplayBOEQuickSearch", securityPage, SecurityAuthorization.Read, ws, boeID);

			BOEQuickSearchModelView theModelView = new BOEQuickSearchModelView();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_QUICK_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEQuickSearch", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the BOESummary Partial View
		/// </summary>
		/// <param name="workspace">Workspace short name</param>
		/// <param name="id">BOE ID</param>
		/// <returns>BOE Summary view Action Result</returns>
		public virtual ViewResult DisplayBOESummary(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOESummary", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
										where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
										select p).Any();

			ViewData["BOEID"] = boeID;
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

			ICollection<BOESummaryGridModelView> results = _ControllerLogic.GetBOESummaryGridModelViews(ws, boe, isSubcontractorUser);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_SUMMARY, results);

			// Checks Web.Config Read Only Mode
			this.ViewData["IsReadOnlyMode"] = false;
			if (SiteMasterUtilities.IsReadOnly())
			{
				if (CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.CreateReadUpdateDelete)
				{
					this.ViewData["IsReadOnlyMode"] = true;
				}
			}

			// Finalize Action
			FinalizeAction(_log, "DisplayBOESummary", sw);
			return toReturn;
		}

		/// <summary>
		/// Displays containing Page for BOE Details including tabs
		/// </summary>
		/// <param name="workspace">Workspace short name</param>
		/// <param name="id">BOE ID</param>
		/// <returns></returns>
		public virtual ViewResult DisplayBOEDetails(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayBOEDetails", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
									where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
									select p).Any();

			// Initiate a NON Readonly list of Resources
			ICollection<ResourceDTO> originalResourceList = ws.ResourcesForWsResourceListId.ToList();
			
			// Perform Action
			ViewData["BOEID"] = boeID;
			ViewData["isMaterial"] = boe.isMaterial;
			ICollection<ResourceDTO> wsResources = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, false, workspace); ;
			ViewData["WSRESOURCES"] = wsResources;
			ViewData["WSRESOURCESTM"] = wsResources.Where(r => r.SegRegion == WebConstants.SPACE_LEGACY_TM).Select(a => a.Id).ToArray();
			ViewData["WSBUSINESSRESOURCECODES"] = BRCValidationUtility.GetResourcesBasedOnCompanyMode(originalResourceList, true, workspace);
			ViewData["WSPERFORGS"] = _BoeLaborControllerLogic.GetPerformingOrgs(ws);
			ViewBag.WsClins = ws.Clins.Where(x => !x.ClinNumber.Equals("MULTI")).Select(x => new { ClinId = x.Id, ClinName = x.ClinString }).ToList();
			ViewBag.WsWbss = ws.WbsElements.Where(x => !x.WbsNumber.Equals("MULTI")).Select(x => new { WbsId = x.Id, WbsName = x.WbsString }).ToList();

			// display when you are not a sub, the boe is not summary or multi & ODC items exist
			ViewData["displayODCTab"] = !(IsSubContractor || boe.IsMultiClinWbs) && boe.OtherDirectCosts.Any();

			// display travel only when you are not a sub AND you have Travel data already
			ViewData["displayTravelTab"] = !IsSubContractor && boe.Travels.Any();

			ViewResult toReturn = GetMasterView(WebConstants.VIEW_BOE_DETAILS, workspace);

			// Finalize Action
			FinalizeAction(_log, "DisplayBOEDetails", sw);
			return toReturn;
		}

		/// <summary>
		/// Display the submit for review button
		/// </summary>
		/// <param name="workspace">the workspace name</param>
		/// <param name="boeID">the boeid</param>
		/// <returns>the button/script</returns>
		public ViewResult DisplaySubmitForReviewBOEButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplaySubmitForReviewBOEButton", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["SubmitForReview_ReadOnly"] = GetReadOnlyAttribute(CheckPermissions(SecurityPage.SubmitForReview, ws, boeID));

			if (SiteMasterUtilities.IsReadOnly())
			{
				if (CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.CreateReadUpdateDelete)
				{
					ViewData["SubmitForReview_ReadOnly"] = "true";
				}
			}

			// Pass the BOE ID to the Validate BOE partial
			ViewData["BOEID"] = boeID;

			// Return the Validate BOE partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_BOE_SUBMIT_FOR_REVIEW);

			// Finalize Action
			FinalizeAction(_log, "DisplaySubmitForReviewBOEButton", sw);
			return toReturn;
		}

		/// <summary>
		/// Display the Validate BOE partial view
		/// </summary>
		/// <param name="workspace">The current workspace ID</param>
		/// <param name="id">The current BOE ID</param>
		/// <returns>ActionResult to display the Validate BOE partial</returns>
		public ViewResult DisplayValidateBOEButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayValidateBOEButton", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["ValidateBOE_ReadOnly"] = GetReadOnlyAttribute(CheckPermissions(SecurityPage.ValidateBOE, ws, boeID));

			// Pass the BOE ID to the Validate BOE partial
			ViewData["BOEID"] = boeID;

			// Return the Validate BOE partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_BOE_VALIDATE);

			// Finalize Action
			FinalizeAction(_log, "DisplayValidateBOEButton", sw);
			return toReturn;
		}

		/// <summary>
		/// Display the Offload BOE partial view
		/// </summary>
		/// <param name="workspace">The current workspace ID</param>
		/// <param name="id">The current BOE ID</param>
		/// <returns>ActionResult to display the Offload BOE partial</returns>
		public ViewResult DisplayOffloadBOEButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayOffloadBOEButton", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["HideOffloadBOE"] = (ws.ProjectMapType == ProjectMapType.StandardWithoutOffload).ToString().ToLower();
			ViewData["BOEID"] = boeID;
			// Return the Offload BOE partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_BOE_OFFLOAD);

			// Finalize Action
			FinalizeAction(_log, "DisplayOffloadBOEButton", sw);
			return toReturn;
		}

		/// <summary>
		/// Display the submit for approval button
		/// </summary>
		/// <param name="workspace">the workspace name</param>
		/// <param name="boeID">the boeid</param>
		/// <returns>the button/script</returns>
		public ViewResult DisplaySubmitForApproval(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplaySubmitForApproval", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			ViewData["SubmitForApproval_ReadOnly"] = GetReadOnlyAttribute(CheckPermissions(SecurityPage.SubmitForApproval, ws, boeID));

			// Pass the BOE ID to the Validate BOE partial
			ViewData["BOEID"] = boeID;

			// Return the Validate BOE partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_BOE_SUBMIT_FOR_APPROVAL);

			// Finalize Action
			FinalizeAction(_log, "DisplaySubmitForApproval", sw);
			return toReturn;
		}

		/// <summary>
		/// Display the Confidence Report Button in a BOE
		/// </summary>
		/// <param name="workspace">Workspace Shortname</param>
		/// <param name="boeID">BOE ID</param>
		/// <returns>ViewResult for Confidence Report Button</returns>
		public ViewResult DisplayConfidenceReportButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_BUTTON, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			
			ViewData["BOEID"] = boeID;
			ViewData["HideConfidenceReport"] = !Utilities.IsConfidenceReportEnabled;
			ViewResult toReturn = View(WebConstants.VIEW_BOE_CONFIDENCE_REPORT_BUTTON);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_BUTTON, sw);
			return toReturn;
		}

		#endregion Edit BOE

		#region Manage BOE

		/// <summary>
		/// Manage BOE Model
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public virtual JsonResult GetManageBOEGridModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_MANAGE_BOE_MODEL, SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);

			ManageBOEGridWidgetModelView theModelView = new ManageBOEGridWidgetModelView();
			theModelView.ContainsOCI = ws.ContainsOCI;

			_ControllerLogic.CalculateManageBOEDefaults(theModelView, ws);

			theModelView.ManageBoeHeaderInfo = _ControllerLogic.GetCompanySpecificManageBoeHeaderInfo;
			theModelView.WorkspaceState = ws.WorkspaceState;
			theModelView.AllowBOEStateChanges = (CheckPermissions(SecurityPage.EditBoeLockedState, ws, null) == SecurityAuthorization.CreateReadUpdateDelete);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_GET_MANAGE_BOE_MODEL, sw);
			return this.Json(theModelView);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JsonResult SaveBOEStates(string workspace, IDictionary<int, BOEState> boeStates)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "SaveBOEStates", SecurityPage.EditBoeLockedState, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
			IList<string> errorMessages = new List<string>();

			long? updateDateLong = this._ControllerLogic.SaveBOEStates(boeStates, ws, errorMessages);

			JsonResult response = Json(new { Status = true, UpdateDateLong = updateDateLong.ToString(), ErrorMessages = errorMessages });

			FinalizeAction(_log, "SaveBOEStates", sw);

			return response;
		}

		/// <summary>
		/// Display the Export BOE partial view
		/// </summary>
		/// <param name="workspace">The current workspace ID</param>
		/// <param name="boeID">The current BOE ID</param>
		/// <returns>ActionResult to display the Export BOE partial</returns>
		public ViewResult DisplayExportBOEButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DisplayExportBOEButton", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			// Perform Action
			// Pass the BOE ID and Title to the Export BOE partial
			ViewData["BOEID"] = boeID;

			FullBoe boe = ws.Boes.FirstOrDefault(b => b.Id == boeID);
			ViewData["BOETitle"] = boe != null ? boe.Title : string.Empty;

			// Do we need to support the special Labor Hours Summary by Custom Field template?
			ViewData["IsUsingSummarizeByCustomFieldTemplate"] = false;
			ViewData["SummarizeByCustomFieldOptions"] = null;

			string exportFormatName = ws.SelectedWorkspaceExportFormatName;
			if (this.reportsControllerLogic.IsUsingSummarizeByCustomFieldTemplate(exportFormatName))
			{
				this.ViewData["IsUsingSummarizeByCustomFieldTemplate"] = true;
				this.ViewData["SummarizeByCustomFieldOptions"] = this.reportsControllerLogic.SummarizeByCustomFieldOptions(ws.CustomFields);
			}

			// Return the Export BOE partial view
			ViewResult toReturn = View(WebConstants.VIEW_BOE_BOE_EXPORT);

			// Finalize Action
			FinalizeAction(_log, "DisplayExportBOEButton", sw);
			return toReturn;
		}

		#endregion

		#endregion Partial Views

		#endregion Display

		#region AJAX Calls

		/// <summary>
		/// Exports a BOE to a pre-formatted MS Word template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <param name="workspace">WS</param>
		/// <param name="boeId">Boe Id</param>
		/// <param name="summarizeByCustomField">Summarize by Custom Field</param>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Word template.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public async Task<ActionResult> ExportBOEToWordFile(string workspace, int boeId, string summarizeByCustomField)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
										where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
										select p).Any();

			List<int> ids = new List<int>();
			ids.Add(boeId);

			ActionResult result = new EmptyResult();

			try
			{
				bool isCustomExport;
				WorkspaceExportFormatDTO wsExportFormatDTO;
				BOEExportInputs exportInputs;
				ICollection<BOEExportModelView> boeExportModelViews;
				List<BOESummaryGridModelView> boeSummaryGridModelViews;

				this.reportsControllerLogic.PrepareAllBOEsReport(ws, isSubcontractorUser, summarizeByCustomField, ids, ViewData, out isCustomExport, out wsExportFormatDTO,
					out exportInputs, out boeExportModelViews, out boeSummaryGridModelViews, false);
				await this.reportsControllerLogic.ExportAllBOEsReport(ws, null, Response, isCustomExport, wsExportFormatDTO, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
			}
			catch (Exception ex)
			{
				if (ConfigurationUtilities.GetAppSetting<bool>("LocalDebug"))
				{
					Exception x = ex;
					while (x.InnerException != null)
					{
						x = x.InnerException;
					}

					string details =
						"Message : " + (x.Message == null ? "No Message" : x.Message) + "\n" +
						"Stack : " + (x.StackTrace == null ? "No StackTrace" : x.StackTrace.ToString());

					result = this.CreateTextFileWithErrorMessage(details);
				}
				else
				{
					_log.Error(ex);

					result = this.CreateTextFileWithErrorMessage(ex);
				}
			}

			return result;
		}

		/// <summary>
		/// Exports a BOE to a pre-formatted MS Word template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeID">BOE</param>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Word template.</returns>
		public async Task<ActionResult> BOESearchPreview(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			//The BOE id sent in is not the BOE the user has permissions too its the BOE they are previewing which they are allowed to view if it is searchable.
			Stopwatch sw = InitializeAction(_log, "BOESearchPreview", SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);

			await _ControllerLogic.ExportBOESearchPreview(ws, boeID, Response);

			// Finalize Action
			FinalizeAction(_log, "BOESearchPreview", sw);
			return new EmptyResult();
		}

		/// <summary>
		/// Exports a ProjectMap BOE to a pre-formatted MS Word template and sends the file as a download
		/// to the user.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="projectMapId">The project map identifier.</param>
		/// <returns>A special ActionResult that generates a file download for the user to download the
		/// populated Word template.</returns>
		public ActionResult ProjectMapSearchPreview(string workspace, int projectMapId)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			//The ProjectMap id sent in is not the ProjectMap the user has permissions too its the ProjectMap they are previewing which they are allowed to view if it is searchable.
			Stopwatch sw = InitializeAction(_log, "ProjectMapSearchPreview", SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);

			_ControllerLogic.ExportProjectMapSearchPreview(ws, projectMapId, Response);

			// Finalize Action
			FinalizeAction(_log, "BOESearchPreview", sw);
			return new EmptyResult();
		}

		/// <summary>
		/// Calls to the Business layer to Validate the BOE. Then passes results
		/// of the validation to the UI for rendering.
		/// </summary>
		/// <param name="workspace">The workspace ID</param>
		/// <param name="id">The ID of the BOE to validate</param>
		/// <returns>The results of the validate function</returns>
		public JsonResult ValidateBOE(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ValidateBOE", SecurityPage.ValidateBOE, SecurityAuthorization.Read, ws, boeID);

			// Perofrm Action
			// Get the BOE's entire set of data in a DTO object

			JsonResult toReturn = null;

			// Call to the business layer to validate the BOE
			toReturn = Json(_validateBOE.ValidateBOE_OnValidateBtnClick(boe, ws));

			// Finalize Action
			FinalizeAction(_log, "ValidateBOE", sw);
			return toReturn;
		}

		/// <summary>
		/// Saves a BOE Header edit.
		/// </summary>
		/// <param name="inBOEHeader">The modified header</param>
		/// <returns></returns>
		[HttpPost]
		public virtual ActionResult SaveEditBOEHeader(string workspace, int boeID, [TitleBinder] IBOEHeaderModelView inBOEHeader, BOEHeaderDescriptionModelView inBOEHeaderDescription)
		{
			_ = inBOEHeader ?? throw new ArgumentNullException(nameof(inBOEHeader));
			_ = inBOEHeaderDescription ?? throw new ArgumentNullException(nameof(inBOEHeaderDescription));

			FullBoe boe = this.Factory.CreateFullBoe(boeID);
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw;

			// Initialize Action
			bool descriptionOnly = false;
			try
			{
				sw = InitializeAction(_log, "SaveEditBOEHeader", SecurityPage.EditBOEHeader, SecurityAuthorization.ReadUpdate, ws, boe.Id);
			}
			catch (AuthorizationException)
			{
				// we failed saving at the 'edit boe header' level .. let's see if the EditBOEHeaderDescription is allowed
				sw = InitializeAction(_log, "SaveEditBOEHeader", SecurityPage.EditBOEHeaderDescription, SecurityAuthorization.ReadUpdate, ws, boe.Id);

				descriptionOnly = true;
			}


			// validate RTE field length
			if (ws.RteSizeLimit.HasValue)
			{
				if (!string.IsNullOrEmpty(inBOEHeaderDescription.Description) && ws.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(inBOEHeaderDescription.Description).Length)
				{
					ModelState.AddModelError("Description", string.Format("The maximum length of BOE Description is {0} characters.", ws.RteSizeLimit.Value));
				}

				if (!descriptionOnly && !string.IsNullOrEmpty(inBOEHeader.DataSource) && ws.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(inBOEHeader.DataSource).Length)
				{
					ModelState.AddModelError("DataSource", string.Format("The maximum length of BOE Source of Data is {0} characters.", ws.RteSizeLimit.Value));
				}
			}

			if (inBOEHeaderDescription.RteTemplateAnswers != null && inBOEHeaderDescription.RteTemplateAnswers.Any())
			{
				ICollection<RteCustomTemplateSourceModelView> sources = this.rteTemplateDataLoader.GetSources(ws.UsingTemplateBOE);
				ICollection<ValidationMessage> rteValidationErrors = this.ValidateRteAnswers(inBOEHeaderDescription.RteTemplateAnswers, sources, ws.RteSizeLimit);

				// convert from validationmessage to modelerror
				if (rteValidationErrors.Any())
				{
					foreach (ValidationMessage message in rteValidationErrors)
					{
						ModelState.AddModelError(message.FieldName, message.ValidationIssue);
					}
				}
			}

			JsonResult toReturn;
			if (ModelState.IsValid)
			{
				_ControllerLogic.SaveEditBoeHeader(ws, boe, inBOEHeader, inBOEHeaderDescription, descriptionOnly);
				boe = this.Factory.CreateFullBoe(boeID);
				toReturn = Json(_ControllerLogic.CreateBOEHeaderMV(boe, ws));
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "SaveEditBOEHeader", sw);

			toReturn.MaxJsonLength = int.MaxValue;
			return toReturn;
		}

		/// <summary>
		/// Perform the action for submitting a BOE for review
		/// </summary>
		/// <param name="workspace">the workspace name</param>
		/// <param name="boeID">the boeid</param>
		[HttpPost]
		public virtual void SubmitForReview(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "SubmitForReview", SecurityPage.SubmitForReview, SecurityAuthorization.ReadUpdate, ws, boeID);
			// when we submit for review we want to send an email to reviewers
			// and also write a message to the log

			_ControllerLogic.SubmitForReview(ws, boeID);

			// Finalize Action
			FinalizeAction(_log, "SubmitForReview", sw);
		}

		/// <summary>
		/// Deletes all task elements for a BOE
		/// </summary>
		/// <param name="workspace">Workspace Name</param>
		/// <param name="boeID">BOE ID</param>
		/// <returns></returns>
		public JsonResult DeleteAllBOETaskElements(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DeleteAllBOETaskElements", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			// Perform Action
			JsonResult toReturn = Json(new { Status = true });

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				_ControllerLogic.DeleteAllBOETaskElements(ws, boe);
				scope.Complete();
			}

			// Process task variable and workspace variable dependencies
			this._BoeLaborControllerLogic.ProcessAllVariableDependencies(boeID, ws);

			// Finalize Action
			FinalizeAction(_log, "DeleteAllBOETaskElements", sw);

			return toReturn;
		}

		/// <summary>
		/// Deletes the given task elements
		/// </summary>
		/// <param name="deletedTasks">A collection of task elements</param>
		[HttpPost]
		public virtual void DeleteTaskElements(string workspace, int boeID,
			GenericTaskElementGridRow deletedTask)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "DeleteTaskElements", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			// Perform Action
			if (deletedTask == null)
			{
				throw new ArgumentNullException(nameof(deletedTask));
			}

			if (ModelState.IsValid)
			{
				_ControllerLogic.DeleteTaskElement(ws, boe, deletedTask);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, "DeleteTaskElements", sw);
		}

		/// <summary>
		/// Resets boes with the ids below to draft
		/// </summary>
		/// <param name="workspace">Workspace name.</param>
		/// <param name="boes">List of BOEs to be saved.</param>
		/// <returns>JsonResult of True or GenValidationException</returns>
		public ActionResult ResetDraftBOE(string workspace, Collection<ManageBOEModelView> boes)
		{
			// reuse SaveManageBOE to do state transitions
			// BOEs that should be reset to draft have their IsDeleted set to true

			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}
			
			boes = boes.Where(b => b.State != BOEState.Draft).ToCollection();

			if (boes.Count == 0)
			{
				throw new GenValidationException("There must be at least one BOE that is not already set to Draft");
			}

			foreach (ManageBOEModelView boe in boes)
			{
				// reset to false and set state to draft
				boe.Deleted = false;
				boe.State = BOEState.Draft;
			}

			return this.SaveManageBOE(workspace, boes);
		}

		/// <summary>
		/// Saves a BOE(s) from the Manage BOE page.  There are also many side affects that occur with this save.
		/// </summary>
		/// <param name="workspace">Workspace name.</param>
		/// <param name="boes">List of BOEs to be saved.</param>
		/// <returns>
		/// (json): Either "true" or a single modelView with one BOE.  When true, a reload of the boe grid is required, when a single
		/// model view is returned, that one particular row is updated in the UI.
		/// </returns>

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public ActionResult SaveManageBOE(string workspace, Collection<ManageBOEModelView> boes)
		{
			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// we're gonna need the original unmodified boes and clins for later processing so grab them now
			// we need to make sure to NOT modify the elements in these collections
			ICollection<FullBoe> originalUnmodifiedBOEs = ws.Boes.ToList().DeepClone();
			ICollection<ClinDTO> originalUnmodifiedCLINs = ws.Clins.ToList<ClinDTO>().DeepClone();
			ICollection<WbsDTO> originalUnmodifiedWbses = (from w in ws.WbsElements select new WbsDTO(w)).ToCollection().DeepClone();

			// grab the ids of all original boes that have been modified in some way
			ICollection<int> originalModifiedBoeIds = (from b in boes
			   where b.BoeID > 0 && !b.Deleted
			   select b.BoeID).ToCollection();

			// grab all the original boe permissions for each original boe that has been modified
			ICollection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(originalModifiedBoeIds);
			Collection<int> BoeIdsEffectedByMulti = new Collection<int>();
			#region memberVariables

			// Create an authorID and approver collection that will keep track of previous author
			// and removed approvers, in case these were changed on the BOE
			Dictionary<int, Collection<UserDTO>> AuthorsChangeDictionary = new Dictionary<int, Collection<UserDTO>>();
			Dictionary<int, Collection<UserDTO>> ApproversChangeDictionary = new Dictionary<int, Collection<UserDTO>>();

			// create a dictionary that will keep track of the BOE ID and its original state before a save
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();

			// BOE's Labor Spread values that need to be recalculated because of any changes made during this save
			List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

			//keep track of BOE IDs that have had its WBS or CLIN changed
			Dictionary<int, Collection<FieldChanged>> BoeFieldsChanged = new Dictionary<int, Collection<FieldChanged>>();

			Collection<int> ClinIDsToRecalculateLaborSpread = new Collection<int>();
			ICollection<int> WbsIDsToRecalculateLaborSpread = new Collection<int>();
			List<BoeApproverResponseDTO> boeApproverResponsesToPotentiallySave = new List<BoeApproverResponseDTO>();
			Collection<BOEStateTransition> transitionsToPerform = new Collection<BOEStateTransition>();

			Collection<BoeDTO> boesToSave = new Collection<BoeDTO>();
			ICollection<FullBoe> BoesToBeDeleted = new Collection<FullBoe>();
			Collection<BoeDTO> NewMaterialBoes = new Collection<BoeDTO>();

			Collection<BoeTaskElementDTO> laborElementsUpdated = new Collection<BoeTaskElementDTO>();
			Collection<TravelDTO> travelElementsUpdated = new Collection<TravelDTO>();

			Dictionary<BoeDTO, Collection<int>> boeInformationCollection = new Dictionary<BoeDTO, Collection<int>>();
			// This holds the current MV if we're just editing a single row.  This way we can return the changes to the UI.
			ManageBOEModelView singleEditMV = null;

			#endregion

			// get the user
			UserDTO activeUser = ws.CurrentActiveUser;

			#region Validation
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			// can't mix together boe edits and boe deletes cause the ManageBOEs page won't allow it. They are on separate pages
			// so we can leverage that information to skip processing for single edits that don't apply to a boe delete
			//----------------------------------------------------------------------------------------------
			bool requestIsADelete = boes.Any(b => b.Deleted);

			if (!this.ModelState.IsValid && boes.Count(b => b.Deleted) == 0)
			{
				ValidationErrors = Utilities.CreateModelStateValidationErrorList(this.ModelState);
			}
			else
			{
				// create a dictionary of all the BOEs so we can reference them in the loop rather than getting one at a time.
				// it's ok if these elements are modified later in the logic
				IDictionary<int, FullBoe> workspaceBoeDictionary = ws.Boes.ToDictionary(x => x.Id);

				Collection<int> allMVboeIds = (from mv in boes
	   select mv.BoeID).Distinct().ToCollection();

				// get all approver responses at one time
				ICollection<BoeApproverResponseDTO> allApproverResponses = this._BoeApproverResponseLoader.GetByBoeIds(allMVboeIds);

				// check state validation before worrying about committing to the database but only if NOT a delete
				if (!requestIsADelete)
				{
					this._ControllerLogic.ValidateSaveManageBOE(boes, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);
				}

				#region MV to DTO conversion

				// create a dictionary so we can lookup the permissions easily
				ILookup<int, PermissionsDTO> boePermissionsLookup = boePermissions.ToLookup(x => x.BOEId.Value);

				//Convert to use BOE DTOs
				foreach (ManageBOEModelView boeMV in boes)
				{
					FullBoe originalBoe;
					Collection<FieldChanged> changed = new Collection<FieldChanged>();

					// If the BOE is is not new, grab it from the DB or create an empty BOE to hold the new data from the user.
					#region
					if (boeMV.BoeID > 0)
					{
						originalBoe = workspaceBoeDictionary[boeMV.BoeID];
						DataRelationshipVerifier.VerifyDataRelation(originalBoe, ws.Id);

						if (boes.Count == 1 && boeMV.Deleted == false)
						{
							singleEditMV = boeMV;
						}
					}
					else
					{
						originalBoe = this.Factory.CreateFullBoe();
						originalBoe.Id = boeMV.BoeID;
					}
					#endregion

					// if DELETE or the BOE's CLIN was changed, need to recalculate labor spread for any other BOE that had either the old CLIN or the new CLIN referenced
					#region
					if (requestIsADelete || boeMV.ClinID != originalBoe.CLINID)
					{
						if (boeMV.ClinID.HasValue)
						{
							ClinIDsToRecalculateLaborSpread.Add(boeMV.ClinID.Value);
						}

						// if this is a new CLIN association, no need to recalculate
						// only if the values are different..
						if (originalBoe.CLINID.HasValue && boeMV.ClinID != originalBoe.CLINID)
						{
							ClinIDsToRecalculateLaborSpread.Add(originalBoe.CLINID.Value);
						}

						changed.Add(new FieldChanged
						{
							Field = "CLIN",
							OldValue = originalBoe.CLINID.HasValue ? (from c in originalUnmodifiedCLINs where c.Id == originalBoe.CLINID select c.ClinString).First() : CommonConstants.Unassigned_CLIN_Display_Text,
							NewValue = boeMV.ClinID.HasValue ? ws.Clins.First(c => c.Id == boeMV.ClinID.Value).ClinString : CommonConstants.Unassigned_CLIN_Display_Text
						});
					}
					#endregion

					// if the boe already existed, and its going from a non multi boe to a multi boe we'll need to update the resources.
					if (boeMV.BoeID > 0 && boeMV.IsMultiClinWbs && !originalBoe.IsMultiClinWbs)
					{
						originalBoe.TaskElements.SelectMany(t => t.taskElementLabors.Select(l => { l.Updateable = UpdateType.Upsert; l.WBSID = originalBoe.WBSID; l.CLINID = originalBoe.CLINID; return l; })).ToCollection();
						laborElementsUpdated = originalBoe.TaskElements.ToCollection();
						//delete the travel and odc elements
						travelElementsUpdated = originalBoe.Travels.Select(t => { t.Updateable = UpdateType.Deleted; return t; }).ToCollection();
						BoeIdsEffectedByMulti.Add(boeMV.BoeID);

					}
					else if (boeMV.BoeID > 0 && !boeMV.IsMultiClinWbs && originalBoe.IsMultiClinWbs)
					{
						// if the boe already existed, and its going from a multi boe to a non multi boe we'll need to update the resources.
						originalBoe.TaskElements.SelectMany(t => t.taskElementLabors.Select(l => { l.Updateable = UpdateType.Upsert; l.WBSID = null; l.CLINID = null; return l; })).ToCollection();
						laborElementsUpdated = originalBoe.TaskElements.ToCollection();
					}
					originalBoe.IsMultiClinWbs = boeMV.IsMultiClinWbs;
					originalBoe.CLINID = boeMV.ClinID;

					if (boeMV.WbsID.HasValue && boeMV.WbsID < 0)
					{
						boeMV.WbsID = null;
					}

					// if the BOE's WBS was changed, need to recalculate labor spread for any other BOE that had either the old WBS or the new WBS referenced
					#region
					if (requestIsADelete || boeMV.WbsID != originalBoe.WBSID)
					{
						if (boeMV.WbsID.HasValue)
						{
							WbsIDsToRecalculateLaborSpread.Add(boeMV.WbsID.Value);
						}

						// if this is a new WBS association, no need to recalculate
						// only if the values are different..
						if (originalBoe.WBSID.HasValue && boeMV.WbsID != originalBoe.WBSID)
						{
							WbsIDsToRecalculateLaborSpread.Add(originalBoe.WBSID.Value);
						}

						changed.Add(new FieldChanged
						{
							Field = "WBS",
							OldValue = (originalBoe.WBSID.HasValue && originalBoe.WBSID > 0) ? (from w in originalUnmodifiedWbses where w.Id == originalBoe.WBSID.Value select w.WbsString).First() : CommonConstants.Unassigned_WBS_Display_Text,
							NewValue = (boeMV.WbsID.HasValue && boeMV.WbsID > 0) ? (from w in ws.WbsElements where w.Id == boeMV.WbsID.Value select w.WbsString).First() : CommonConstants.Unassigned_WBS_Display_Text
						});
					}
					#endregion

					originalBoe.WBSID = boeMV.WbsID;
					originalBoe.WCBID = boeMV.BoeXrefID;



					if (!(boeMV.State == BOEState.Unassigned || boeMV.State == BOEState.None))
					{
						if (changed.Count() != 0)
						{
							BoeFieldsChanged[originalBoe.Id] = changed;
						}
					}

					// If NOT a DELETE and the BOE State is unassigned but authors and/or subcontractor authors were added and non-approver fields were changed, 
					// automatically change state to draft.  Otherwise, the state is selected by the dropdown from the model view.
					#region
					if (!requestIsADelete
						&& (boeMV.BoeID > 0 && (boeMV.Authors.Any() || boeMV.SubcontractorAuthors.Any()) && (
						boeMV.WbsID != originalBoe.WBSID ||
						boeMV.ClinID != originalBoe.CLINID ||
						boeMV.isMaterial != originalBoe.isMaterial ||
						boeMV.IsMultiClinWbs != originalBoe.IsMultiClinWbs ||
						(!Enumerable.SequenceEqual(boeMV.Authors, originalBoe.AuthorIDs)) ||
						(!Enumerable.SequenceEqual(boeMV.SubcontractorAuthors, originalBoe.SubcontractorAuthorIDs)))))
					{
						originalBoe.State = BOEState.Draft;
					}
					else if (boeMV.BoeID < 0 && (boeMV.Authors.Any() || boeMV.SubcontractorAuthors.Any()))
					{
						originalBoe.State = BOEState.Draft;
					}
					else
					{
						originalBoe.State = boeMV.State;
					}
					#endregion

					// Set the start and end dates for new BOEs
					#region
					if (boeMV.BoeID < 0)
					{
						this._ControllerLogic.setDefaultBoeDates(ws, originalBoe, boeMV.ClinID);
					}
					#endregion

					// Get Employee Authors
					originalBoe.AuthorIDs = boeMV.Authors.Any() ? boeMV.Authors : null;

					// Get the Subcontractor Authors
					originalBoe.SubcontractorAuthorIDs = boeMV.SubcontractorAuthors.Any() ? boeMV.SubcontractorAuthors : null;

					// If NOT a DELETE request && the authors were changed, we need to save the old list of authors to pass to our email function
					#region
					if (!requestIsADelete && originalBoe.Id > 0)
					{
						ICollection<PermissionsDTO> permissionsForThisBoe = boePermissionsLookup[boeMV.BoeID].ToList();

						// retrieve the Authors on the BOE before any reassignments were performed
						ICollection<PermissionsDTO> authors = permissionsForThisBoe.Where(x => x.Role == Role.Author).ToList();

						// set a count of any Authors that were reassigned (removed)
						int authorsReassigned = authors.Count(a => !boeMV.Authors.Contains(a.ETIUserId));

						// set a count of any Authors that were added
						int authorsAdded = boeMV.Authors.Count(id => !authors.Select(a => a.ETIUserId).Contains(id));

						// set a count of any Authors that were reassigned, to include both removed and added
						authorsReassigned += authorsAdded;

						// retrieve the Subcontractor Authors on the BOE before any reassignments were performed
						ICollection<PermissionsDTO> subcontractorAuthors = permissionsForThisBoe.Where(x => x.Role == Role.SubcontractorAuthor).ToList();

						// set a count of any Subcontractor Authors that were reassigned (removed)
						int subcontractorAuthorsReassigned = subcontractorAuthors.Count(a => !boeMV.SubcontractorAuthors.Contains(a.ETIUserId));

						// set a count of any Subcontractor Authors that were reassigned, to include both removed and added
						int subcontractorAuthorsAdded = boeMV.SubcontractorAuthors.Count(id => !subcontractorAuthors.Select(a => a.ETIUserId).Contains(id));

						// set a count of any Subcontractor Authors that were reassigned, to include both removed and added
						subcontractorAuthorsReassigned += subcontractorAuthorsAdded;

						// total up the additions and removals for both Authors and Subcontractor Authors
						int totalAuthorChanges = authorsReassigned + subcontractorAuthorsReassigned;

						// if there have been any removals or additions of either Subcontractor Authors or Authors, update the counts and send emails
						if (totalAuthorChanges > 0)
						{
							// collect the old authors and combine them into 1 user collection
							ICollection<UserDTO> oldAuthors = authors.Select(a => this.UserLoader.GetUserByID(a.ETIUserId)).ToList();
							ICollection<UserDTO> oldSubcontractorAuthors = subcontractorAuthors.Select(a => this.UserLoader.GetUserByID(a.ETIUserId)).ToList();
							oldAuthors = oldAuthors.Union(oldSubcontractorAuthors).ToList();

							AuthorsChangeDictionary[boeMV.BoeID] = new Collection<UserDTO>(oldAuthors.ToArray());

							// increment the reassigned number in the BOE
							originalBoe.NumAuthorReassigned += authorsReassigned;
							originalBoe.NumAuthorReassigned += subcontractorAuthorsReassigned;
						}
					}

					PermissionsDTO[] Approvers = boePermissionsLookup[originalBoe.Id].Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
					bool approversRemoved = (from removedApprover in Approvers
	where !boeMV.Approvers.Contains(removedApprover.ETIUserId)
	select removedApprover).Any();

					bool approversAdded = (from addedApprover in boeMV.Approvers
										  where !(Approvers.Select(a => a.ETIUserId).Contains(addedApprover))
										  select addedApprover).Any();

					if (approversRemoved || approversAdded)
					{
						IEnumerable<UserDTO> oldApprovers = from oldApprover in Approvers
										   select this.UserLoader.GetUserByID(oldApprover.ETIUserId);

						ApproversChangeDictionary[originalBoe.Id] = new Collection<UserDTO>(oldApprovers.ToArray());
					}

					// Get Approvers
					// grab all approver ids for the boe MVs and get them all at once
					int insertNewApproverIndex = -1;
					foreach (int BoeApproverID in boeMV.Approvers)
					{

						BoeApproverResponseDTO boeApprover = allApproverResponses.FirstOrDefault(a => a.BoeID == boeMV.BoeID && a.ETIUserID == BoeApproverID);
						
						if (boeApprover == null)
						{
							// create a new one
							boeApprover = new BoeApproverResponseDTO();
							boeApprover.Id = insertNewApproverIndex;
							boeApprover.ETIUserID = BoeApproverID;
							boeApprover.BoeID = boeMV.BoeID;
							boeApprover.Updateable = UpdateType.Upsert;
							boeApprover.CurrentUserETIUserID = activeUser.UserID;
							insertNewApproverIndex--;
						}
						boeApproverResponsesToPotentiallySave.Add(boeApprover);
					}

					// if NOT a DELETE request && any approvers were deleted, mark them as such
					if (!requestIsADelete && originalBoe.Id > 0)
					{
						ICollection<BoeApproverResponseDTO> approversThatWereRemoved = allApproverResponses.Where(approvers => approvers.BoeID == boeMV.BoeID && !boeMV.Approvers.Contains(approvers.ETIUserID)).ToCollection<BoeApproverResponseDTO>();

						if (approversThatWereRemoved.Any())
						{
							foreach (BoeApproverResponseDTO boeApprover in approversThatWereRemoved)
							{
								boeApprover.Updateable = UpdateType.Deleted;
								boeApprover.CurrentUserETIUserID = activeUser.UserID;
								boeApproverResponsesToPotentiallySave.Add(boeApprover);
							}
						}
					}
					#endregion

					originalBoe.WorkspaceID = ws.Id;
					originalBoe.UpdateDate = boeMV.UpdateDate;

					if (requestIsADelete)
					{
						originalBoe.Updateable = UpdateType.Deleted;
						BoesToBeDeleted.Add(originalBoe);
						boeInformationCollection[originalBoe] = boeMV.Approvers;
					}
					else
					{
						originalBoe.Updateable = UpdateType.Upsert;
					}

					// if NOT a DELETE request && user is changing boe to a material boe, keep track of it
					if (!requestIsADelete && originalBoe.isMaterial == false && boeMV.isMaterial)
					{
						NewMaterialBoes.Add(originalBoe);
					}
					originalBoe.isMaterial = boeMV.isMaterial;

					boesToSave.Add(originalBoe);
				}

				#endregion

				// if NOT a DELETE then transition any states
				#region
				if (!requestIsADelete)
				{
					foreach (BoeDTO boe in boesToSave)
					{
						BoeDTO oldBoe = null;
						if (boe.Id > 0)
						{
							oldBoe = originalUnmodifiedBOEs.FirstOrDefault(b => b.Id == boe.Id);
						}

						if (oldBoe != null)
						{
							// if all approvers have approved, change state to Approved
							if (oldBoe.State == boe.State && boe.State == BOEState.AwaitingApproval)
							{
								IEnumerable<BoeApproverResponseDTO> responses = boeApproverResponsesToPotentiallySave.Where(x => x.BoeID == boe.Id && x.Updateable != UpdateType.Deleted);

								if (responses.Any() && responses.Count(x => x.ApproverResponse == ApproverReponseType.Approved) ==
									responses.Count())
								{
									boe.State = BOEState.Approved;
								}
							}

							// if the CLIN association changed, put the BOE back to draft

							if (boe.CLINID != oldBoe.CLINID)
							{
								if (boe.State == BOEState.AwaitingApproval || boe.State == BOEState.Approved || boe.State == BOEState.DraftLocked)
								{
									boe.State = BOEState.Draft;
								}
							}

							if (boe.IsMultiClinWbs != oldBoe.IsMultiClinWbs)
							{
								if (boe.State == BOEState.AwaitingApproval || boe.State == BOEState.Approved || boe.State == BOEState.DraftLocked)
								{
									boe.State = BOEState.Draft;
								}
							}

							if (oldBoe.State != boe.State)
							{
								// Validate the Awaiting Approval or Approved to Draft state transition
								string validationMessage;
								if (!this._boeStateMachine.PerformStateTransitionValidation(this.Factory.CreateFullBoe(boe), ws, oldBoe.State, boe.State, out validationMessage))
								{
									// not valid ... communicate to user
									ValidationErrors.Add(new ValidationMessage("Something", validationMessage));
								}

								// Perform common state transition actions
								transitionsToPerform.Add(new BOEStateTransition() { ID = boe.Id, OldState = oldBoe.State, NewState = boe.State });
							}
						}
					}
				}
				#endregion

			}

			#endregion

			// Perform Action
			JsonResult toReturn;

			if (!ValidationErrors.Any())
			{
				// BOEJ-4000: If too many are being deleted, do a backup first
				if (BoesToBeDeleted.Count >= CommonConstants.AUTO_SYSTEM_BACKUP_DELETION_BOES_THRESHOLD)
				{
					WorkspaceVersionMetaDataDTO backup = new WorkspaceVersionMetaDataDTO()
					{
						VersionID = -1,
						CreatedByID = ws.CurrentActiveUser.UserID,
						Updateable = UpdateType.Upsert,
						VersionName = $"{CommonConstants.AUTO_SYSTEM_BACKUP_DELETION_BOES} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}",
						VersionState = ws.WorkspaceState,
						WorkspaceID = ws.Id
					};

					// backup
					versionLoader.Upsert(backup, backup.WorkspaceID);
				}

				#region moreDataManipulationAndThenTheSave

				List<WorkspaceVariableDTO> workspaceVariablesEffectedByDelete = new List<WorkspaceVariableDTO>();

				// Dictionary to keep track of workspace variable IDs that need to be updated and their old variable total
				Dictionary<int, decimal> WorkspaceVarOldValueID = new Dictionary<int, decimal>();

				// grab the clins that need to be recalculated
				ICollection<ClinDTO> clinsToRecalculate = (from c in ws.Clins
				   where ClinIDsToRecalculateLaborSpread.Contains(c.Id)
				   select c).ToCollection<ClinDTO>();

				// update BOE sums based on the workspace variables associated with each clin to recalculate
				#region
				foreach (ClinDTO clinObject in clinsToRecalculate)
				{
					ICollection<WorkspaceVariableDTO> workspaceVariablesForThisClin = FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithClin(clinObject.Id, ws);

					if (workspaceVariablesForThisClin.Any())
					{
						foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesForThisClin)
						{
							DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
							data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

							decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
							WorkspaceVarOldValueID[workspaceVar.Id] = oldTotalValue;

							workspaceVariablesEffectedByDelete.Add(workspaceVar);
						}
					}
				}
				#endregion


				// for BOEs to be deleted, grab the workspace variables they're using and update the BOE sums for the remaining BOEs
				#region

				foreach (FullBoe boeDeleted in BoesToBeDeleted)
				{
					ICollection<WorkspaceVariableDTO> workspaceVariablesForThisBoe = FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithBoe(boeDeleted.Id, ws);

					foreach (WorkspaceVariableDTO workspaceVariable in workspaceVariablesForThisBoe)
					{
						DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
						data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVariable }, ws);

						decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVariable, data);
						WorkspaceVarOldValueID[workspaceVariable.Id] = oldTotalValue;
					}
				}

				#endregion

				// variables that were effected by a BOE delete
				List<OrdinaryVariableDto> taskVariablesEffectedByDelete = new List<OrdinaryVariableDto>();
				List<int> WorkspaceVarIdsEffectedByDelete = new List<int>();

				List<int> BoeIdsEffectedByDelete = new List<int>();
				// variables that were effected by a BOE to Multi
				List<OrdinaryVariableDto> taskVariablesEffectedByMulti = new List<OrdinaryVariableDto>();
				List<WorkspaceVariableDTO> workspaceVariablesEffectedByMulti = new List<WorkspaceVariableDTO>();

				IDictionary<int, int> boeSaveIDDict;
				_ = ws.MoqTypeSelections; // preload the data prior to transaction

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					// save all workspace variables
					if (BoeIdsEffectedByMulti.Any())
					{
						this._ControllerLogic.RemoveMultiBOEReferenceWorkspaceVar(ws, workspaceVariablesEffectedByMulti, BoeIdsEffectedByMulti);
						foreach (int boeID in BoeIdsEffectedByMulti)
						{
							taskVariablesEffectedByMulti.AddRange(FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(boeID, ws));
						}
					}

					// if BOEs were marked to be deleted, get all the task elements that would be effected by this delete before it's actually deleted
					#region
					ICollection<int> boeIdsToBeDeleted = (from b in BoesToBeDeleted select b.Id).ToCollection();
					foreach (FullBoe boe in BoesToBeDeleted)
					{
						taskVariablesEffectedByDelete.AddRange(FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(boe.Id, ws));
						workspaceVariablesEffectedByDelete.AddRange(FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithBoe(boe.Id, ws));
						WorkspaceVarIdsEffectedByDelete.AddRange(workspaceVariablesEffectedByDelete.Select(i => i.Id));
						BoeIdsEffectedByDelete.AddRange(FullWorkspaceHelper.GetBoeIdsImpactedByDeletedBoe(boe.Id, ws, boeIdsToBeDeleted));


						if (boe.WBSID.HasValue)
						{
							ICollection<WbsDTO> parentWbs = FullWorkspaceHelper.GetAllParentWBS(ws.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID), ws);

							if (parentWbs.Any())
							{
								foreach (int wbsId in parentWbs.Select(a => a.Id))
								{
									WbsIDsToRecalculateLaborSpread.Add(wbsId);
								}
							}
						}

						// if the approver response is mapped to a deleted BOE, then we shouldn't bother with saving them
						boeApproverResponsesToPotentiallySave.RemoveAll(x => x.BoeID == boe.Id);
					}
					#endregion

					// We don't need to recompute any boes that are going to be deleted
					BoeIdsEffectedByDelete.RemoveAll(x => boeIdsToBeDeleted.Contains(x));

					// Save approvers for existing BOEs so they can be copied correctly in the mediator.
					BoeApproverResponseDTO[] boeApproversToSave = boeApproverResponsesToPotentiallySave.Where(x => x.BoeID > 0 && x.Updateable != UpdateType.None).ToArray();
					if (boeApproversToSave.Any())
					{
						this._BoeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));
					}

					foreach (BOEStateTransition transition in transitionsToPerform)
					{
						this._boeStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(originalUnmodifiedBOEs.FirstOrDefault(x => x.Id == transition.ID)), ws, transition.OldState, transition.NewState);
					}

					// This method can save boes and task elements. If they are saved, the ws is updated so that any subsequent
					// calls to the TaskElements and/or BOEs will result in a re-read from the db. 
					//need to update travel and odc elements if we are creating a multi boe from a non multi boe
					//need to update the labor resources 
					this._TravelDTOLoader.SaveTravels(travelElementsUpdated);
					this._BoeTaskElementMediator.MediatedSaveTaskElements(laborElementsUpdated, ws);
					this._ControllerLogic.DeleteMoqTypesForBoe(ws, BoesToBeDeleted.Select(x => x.Id).ToList());
					boeSaveIDDict = this._BoeMediator.MediatedSaveBOEs(ws, boesToSave);

					if (!requestIsADelete)
					{
						int newMaterialID = -1;
						foreach (BoeDTO boe in NewMaterialBoes)
						{
							int boeId = boeSaveIDDict.ContainsKey(boe.Id) ? boeSaveIDDict[boe.Id] : -1;
							// add a Material Task Element.
							MaterialDTO MaterialDTOtoSave = new MaterialDTO();
							MaterialDTOtoSave.BoeID = boeId;
							MaterialDTOtoSave.Id = newMaterialID;
							MaterialDTOtoSave.TaskTitle = "Material";
							MaterialDTOtoSave.Updateable = UpdateType.Upsert;

							newMaterialID--;

							this._MaterialLoader.SaveMaterials(new Collection<MaterialDTO> { MaterialDTOtoSave });
						}

						// Save Boe Approvers for new BOEs
						boeApproversToSave = boeApproverResponsesToPotentiallySave.Where(x => x.BoeID < 0).ToArray();
						if (boeApproversToSave.Any())
						{
							foreach (BoeApproverResponseDTO boeapprover in boeApproversToSave)
							{
								boeapprover.BoeID = boeSaveIDDict[boeapprover.BoeID];
							}

							this._BoeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));

							// Need to see if we need to create a WS level role for the user (if the permissions are being granted via a group)
							Collection<PermissionsDTO> wsPermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);

							foreach (BoeApproverResponseDTO boeapprover in boeApproversToSave)
							{
								Collection<PermissionsDTO> wsApproverPermissionsForUser = wsPermissions.Where(x => x.ETIUserId == boeapprover.ETIUserID && x.Role == Role.Approver).ToCollection();

								if (!wsApproverPermissionsForUser.Any())
								{
									PermissionsDTO permission = new PermissionsDTO();

									// need to insert a potential WS permission based on the permission dto
									permission.Id = -1;
									permission.Role = Role.Approver;
									permission.Updateable = UpdateType.Upsert;
									permission.BOEId = null;
									permission.WorkspaceId = ws.Id;
									permission.PermissionId = -1;
									permission.ETIUserId = boeapprover.ETIUserID;

									this.PermissionsLoader.SavePermission(permission);
								}
							}
						}
					}

					// need to get the BOE task elements that were effected by the above BOE save
					ICollection<ClinDTO> clinObjectsToRecalulateLaborSpread = (from c in originalUnmodifiedCLINs where ClinIDsToRecalculateLaborSpread.Contains(c.Id) select c).ToCollection();
					foreach (ClinDTO clinObject in clinObjectsToRecalulateLaborSpread)
					{
						FullClin fullClin = this.Factory.CreateFullClin(clinObject);
						boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithClin(fullClin, VariableType.Task, ws));
						boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithClin(fullClin, VariableType.Workspace, ws));
					}

					if (boeTaskElementsToRecalculate.Any())
					{
						boeTaskElementsToRecalculate = boeTaskElementsToRecalculate.Distinct().ToList();
						this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);

						this._ControllerLogic.AdjustStateOfTaskElements(ws, boeTaskElementsToRecalculate.ToCollection());
					}

					boeTaskElementsToRecalculate.Clear();

					ICollection<FullWbs> WbsObjectsToRecalculateSpread = (from w in ws.WbsElements
								  where WbsIDsToRecalculateLaborSpread.Contains(w.Id)
								  select w).ToCollection();
					foreach (FullWbs wbsObject in WbsObjectsToRecalculateSpread)
					{
						boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Task, ws));
						boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Workspace, ws));
					}

					// If the BOE that contains the variable has been removed, we don't need to recalculate it.. So only recalculate task elements that have active BOEs.
					ws.RefreshBoes();

					taskVariablesEffectedByDelete = taskVariablesEffectedByDelete.Where(x => ws.Boes.Select(z => z.Id).Contains(x.BoeID)).ToList();
					if (taskVariablesEffectedByDelete.Any())
					{
						// A Boe was deleted that was part of a sum of boe task variable. Refresh the task elements so when
						// they are re-retrieved, they will contain updated ordinary variables.
						ws.RefreshTaskElements();
					}

					// check if any task elements need to be recalculated that were effected by a delete boe.
					// check task variables and workspace variables separately
					foreach (OrdinaryVariableDto taskVar in taskVariablesEffectedByDelete)
					{
						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(taskVar.Id, VariableType.Task, ws)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}

					Collection<BoeTaskElementDTO> taskEffectedByMutliBOE = new Collection<BoeTaskElementDTO>(this._ControllerLogic.RemoveMultiBOEReferenceTaskVar(ws, BoeIdsEffectedByMulti));
					// check if any task elements need to be recalculated that were effected by a multi boe being changed
					// check task variables and workspace variables separately
					foreach (OrdinaryVariableDto taskVar in taskVariablesEffectedByMulti)
					{

						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(taskVar.Id, VariableType.Task, ws, taskEffectedByMutliBOE)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}
					// Task elements and workspace variables may have been updated as part of a side affect of saving Boes.
					ws.RefreshTaskElements();
					ws.RefreshWorkspaceVariables();

					// save all workspace variables
					Collection<int> WSIds = new Collection<int>(WorkspaceVarIdsEffectedByDelete.Union(WorkspaceVarOldValueID.Keys).ToArray());
					if (WSIds.Any())
					{
						ICollection<WorkspaceVariableDTO> workspaceVariables = (from v in ws.WorkspaceVariables
										where WSIds.Contains(v.Id)
										select v).ToCollection();

						Collection<WorkspaceVariableDTO> workspaceVarToSave = new Collection<WorkspaceVariableDTO>();
						foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
						{
							DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
							data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

							workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
							workspaceVar.Updateable = UpdateType.Upsert;
							workspaceVarToSave.Add(workspaceVar);
						}
						this._workspaceVariableLoader.SaveWorkspaceVariables(workspaceVarToSave);

						ws.RefreshWorkspaceVariables();
					}

					ws.RefreshBoes();
					ws.RefreshTaskElements();

					workspaceVariablesEffectedByDelete = workspaceVariablesEffectedByDelete.Where(x => x.ValueType == VarValueType.SumOfBOEs && ws.TaskElements.SelectMany(z => z.WorkspaceVariableIDs).Contains(x.Id)).Distinct().ToList();

					foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesEffectedByDelete)
					{
						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(workspaceVar.Id, VariableType.Workspace, ws)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}

					workspaceVariablesEffectedByMulti = workspaceVariablesEffectedByMulti.Where(x => ws.TaskElements.SelectMany(z => z.WorkspaceVariableIDs).Contains(x.Id)).ToList();

					foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesEffectedByMulti)
					{
						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(workspaceVar.Id, VariableType.Workspace, ws)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}

					// save all the task elements that were effected by a BOE deletion or a CLIN/WBS remapping
					if (boeTaskElementsToRecalculate.Any())
					{
						boeTaskElementsToRecalculate = boeTaskElementsToRecalculate.Distinct().ToList();
						this._BoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);

						// refresh the task elements
						ws.RefreshTaskElements();
					}

					// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
					// Now that the first layer has been recalculated, cause a recalculation of everything else.
					ICollection<BoeDTO> boesEffectedByDelete = (from b in ws.Boes
						where BoeIdsEffectedByDelete.Contains(b.Id)
						select b as BoeDTO).ToCollection();

					foreach (BoeDTO boe in boesEffectedByDelete)
					{
						ICollection<BoeTaskElementDTO> taskElements = (from te in ws.TaskElements
							   where te.BoeID == boe.Id
							   select te).ToCollection();

						this._BoeLaborControllerLogic.CalculateLinkedTaskElements(ValidationErrors, taskElements, new Collection<BoeTaskElementDTO>(), ws);
					}

					if (boeTaskElementsToRecalculate.Any())
					{
						this._ControllerLogic.AdjustStateOfTaskElements(ws, boeTaskElementsToRecalculate.ToCollection());
					}

					// Now we need to see if the Wbs had a variable mapped to it. if it did, remap to BOEs created for it
					// only do this if there are workspace variables
					if (ws.WorkspaceVariables.Any())
					{
						foreach (BoeDTO boe in boesToSave)
						{
							if (boe.WBSID.HasValue)
							{
								this.wbsLoader.RemapTaskAndWorkspaceVariablesFromWbsToBoe(boe.WBSID.Value, boe.Id);
							}
						}
					}

					scope.Complete();
				}

				#region emailing

				IDictionary<int, BOEStateModelView> boeStateNameDictionary = this._CommonDataMapper.getBOEStatesDictionary();

				// emails need to be sent after the save
				foreach (BoeDTO boe in boesToSave)
				{
					// if the boe has just been deleted, need to send BOE Deleted email to
					// approvers, author, and workspace admins
					if (boe.Updateable == UpdateType.Deleted)
					{
						foreach (KeyValuePair<BoeDTO, Collection<int>> keyValuePair in boeInformationCollection)
						{
							BoeDTO boeMarkedForDelete = keyValuePair.Key;
							Collection<int> approverIds = keyValuePair.Value;
							WbsDTO wbsAssociatedWithBoe = ws.WbsElements.FirstOrDefault(w => w.Id == boeMarkedForDelete.WBSID);
							ClinDTO clinAssociatedWithBoe = ws.Clins.FirstOrDefault(c => c.Id == boeMarkedForDelete.CLINID);

							if (boeMarkedForDelete.Id == boe.Id)
							{
								this._emailer.SendBOEDeleted(boeMarkedForDelete, activeUser, approverIds, wbsAssociatedWithBoe, clinAssociatedWithBoe, ws, boeStateNameDictionary);
							}

						}
					}
					else
					{
						if (BoeStateDictionary.ContainsKey(boe.Id))
						{
							using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
							{
								this._boeStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(boe), ws, BoeStateDictionary[boe.Id], boe.State);
								scope.Complete();
							}
						}

						// If the BOE has been put in draft mode, send BOE author email opened for edit.
						// else Send the 'Author Changed' email
						if (ws.WorkspaceState == WorkspaceState.Working)
						{
							if (AuthorsChangeDictionary.ContainsKey(boe.Id))
							{
								if (boe.State == BOEState.Draft && BoeStateDictionary[boe.Id] == BOEState.Unassigned)
								{
									this._emailer.SendBOEAuthorsEmailOpenedForEdit(this.Factory.CreateFullBoe(boe), ws);
								}
								else
								{
									this._emailer.SendBOEAuthorsChanged(AuthorsChangeDictionary[boe.Id], this.Factory.CreateFullBoe(boe));
								}

							}

							// Send BOE Author email when Author and Approver are selected during BOE Creation
							// BOE is assigned Draft State and skips Unassigned State.
							if (boe.State == BOEState.Draft && !(BoeStateDictionary.ContainsKey(boe.Id)))
							{
								this._emailer.SendBOEAuthorsEmailOpenedForEdit(this.Factory.CreateFullBoe(boe), ws);
							}
						}

						// Send the 'Approvers Changed' email, if applicable
						if (ApproversChangeDictionary.ContainsKey(boe.Id) && boe.State == BOEState.AwaitingApproval)
						{
							this._emailer.SendBOEApproversChanged(ApproversChangeDictionary[boe.Id], this.Factory.CreateFullBoe(boe));
						}

						// Send CLIN/WBS updated email, if applicable (not sending for new MultiCLIN or for updating MultiCLIN boe to MultiCLIN boe)
						BoeDTO originalBOE = originalUnmodifiedBOEs.FirstOrDefault(b => b.Id == boe.Id);
						if (originalBOE != null)
						{
							bool clinChanged = originalBOE.CLINID != boe.CLINID;
							bool wbsChanged = originalBOE.WBSID != boe.WBSID;
							bool notMultiOrChanged = !boe.IsMultiClinWbs || !originalBOE.IsMultiClinWbs;
							if (notMultiOrChanged && (clinChanged || wbsChanged))
							{
								FullBoe fullBoe = this.Factory.CreateFullBoe(boe);

								// It is an updated BOE, and either CLIN or WBS was changed
								this._emailer.SendBOECLINWBSChanged(fullBoe, clinChanged, wbsChanged, false);
							}
						}
						else if (!boe.IsMultiClinWbs && boe.CLINID.HasValue || boe.WBSID.HasValue)
						{
							int boeToGetID = boeSaveIDDict[boe.Id];
							FullBoe fullBoe = this.Factory.CreateFullBoe(boeToGetID);

							// It is a new BOE, and either CLIN or WBS was set 
							this._emailer.SendBOECLINWBSChanged(fullBoe, fullBoe.CLINID.HasValue, fullBoe.WBSID.HasValue, false);
						}
					}
				}

				#endregion

				if (singleEditMV == null)
				{
					// Returning a status of true forces the entire BOE grid to be reloaded.
					toReturn = this.Json(new { Status = true });
				}
				else
				{
					// Returning a single model view allows the ui to just update that one BOE entry on the grid rather than forcing a 
					// reload of the entire BOE grid.
					// Get the boe
					int boeToGetID = boeSaveIDDict != null && boeSaveIDDict.ContainsKey(singleEditMV.BoeID) ? boeSaveIDDict[singleEditMV.BoeID] : singleEditMV.BoeID;

					FullBoe boe = this.Factory.CreateFullBoe(boeToGetID);

					// populate the MV
					singleEditMV = this._ControllerLogic.GetManageBOEGridData(ws, new List<FullBoe>() { boe }).First();

					toReturn = this.Json(singleEditMV);
				}

				#endregion
			}
			else
			{
				throw new GenValidationException(ValidationErrors);
			}

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_MANAGE_BOE, sw);
			return toReturn;
		}


		/// <summary>
		/// Determines whether or not the specified BOE contains references to other BOEs through Sum of BOEs
		/// Task or Workspace variables.
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <param name="boeID">The BOE</param>
		/// <returns>True if task elements under the BOE have references to Sum of BOEs variables, false otherwise.</returns>
		public JsonResult BOEContainsSumOfBOEs(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			Stopwatch sw = this.InitializeAction(this._log, "BOEContainsSumOfBOEs", SecurityPage.SubmitForApproval, SecurityAuthorization.Read, ws, boeID);

			// Get a value to indicate whether or not this BOE contains task elements that reference
			// other BOEs through Sum of BOEs task or workspace variables
			JsonResult result = this.Json(new
			{
				Result = this._VariableCircularReferenceChecker.GetBOEIDsReferencedByBOEID(boe, null, null, null, ws).Any(),
				Type = "Variable"
			});

			this.FinalizeAction(this._log, "BOEContainsSumOfBOEs", sw);
			return result;
		}

		/// <summary>
		/// Perform the action for submitting a BOE for approval
		/// </summary>
		/// <param name="workspace">the workspace name</param>
		/// <param name="boeID">the boeid</param>
		public virtual JsonResult SubmitForApproval(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			Stopwatch sw = InitializeAction(_log, "SubmitForApproval", SecurityPage.SubmitForApproval, SecurityAuthorization.ReadUpdate, ws, boeID);

			JsonResult toReturn = Json(new { Status = true });
			// when we submit for approval we want to send an email to approvers
			// and also write a message to the log

			ValidationBOEModelView validatedBOE = _ControllerLogic.SubmitForApproval(ws, boe);

			// if validation fails, then BOEs in Locked-Draft should be moved to (unlocked) Draft
			if (!validatedBOE.isValid)
			{
				if (boe.State == BOEState.DraftLocked)
				{
					string validationMessage = string.Empty;
					if (_boeStateMachine.PerformStateTransitionValidation(boe, ws, BOEState.DraftLocked, BOEState.Draft, out validationMessage))
					{
						boe.Updateable = UpdateType.Upsert;
						boe.State = BOEState.Draft;

						using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
						{
							_BoeMediator.MediatedSave(ws, boe);
							scope.Complete();
						}

						_boeStateMachine.PerformStateTransitionAction(boe, ws, BOEState.DraftLocked, BOEState.Draft);

						validatedBOE.NewState = BOEState.Draft;
					}
				}
			}

			toReturn = Json(validatedBOE);

			FinalizeAction(_log, "SubmitForApproval", sw);
			return toReturn;


		}

		/// <summary>
		/// Performs a search for BOEs based on the params passed in
		/// </summary>
		/// <param name="workspace">Name of Workspace containing BOE</param>
		/// <param name="boeID">ID of BOE search is being performed from</param>
		/// <param name="advSearchParams">parameters to search for</param>
		/// <returns></returns>
		public ViewResult AdvancedSearchForBOEs(string workspace, int boeID, BOEAdvancedSearchModelView advSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "SearchForBOEs", SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			ViewResult toReturn = null;

			if (advSearchParams == null)
			{
				throw new ArgumentNullException(nameof(advSearchParams));
			}

			if (!_ControllerLogic.AdvSearchRequiredFieldValidation(advSearchParams))
			{
				throw new GenValidationException("At least one search term besides category is required.");
			}
			if ((advSearchParams.StartDate != null && advSearchParams.EndDate == null) ||
				(advSearchParams.EndDate != null && advSearchParams.StartDate == null))
			{
				throw new GenValidationException("You must supply both dates in the range.");
			}

			ViewData["BOEID"] = boeID;

			if (ModelState.IsValid)
			{
				SearchResultsModelView modelView = _ControllerLogic.AdvancedSearchForBOEs(ws, boeID, advSearchParams);

				// Return the partial view
				toReturn = View(WebConstants.VIEW_BOE_SEARCH_RESULTS, modelView);

				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}
			ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

			return toReturn;
		}

		/// <summary>
		/// Performs a search for BOEs based on the params passed in
		/// </summary>
		/// <param name="workspace">Name of Workspace containing BOE</param>
		/// <param name="advSearchParams">parameters to search for</param>
		/// <returns></returns>
		public ViewResult ProjectMapAdvancedSearchForBOEs(string workspace, BOEProjectMapAdvancedSearchModelView advSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "SearchForBOEs", SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.Read, ws, null);
			ViewResult toReturn = null;

			if (advSearchParams == null)
			{
				throw new ArgumentNullException(nameof(advSearchParams));
			}

			if (!_ControllerLogic.AdvSearchRequiredFieldValidation(advSearchParams))
			{
				throw new GenValidationException("At least one search term besides category is required.");
			}

			ViewData["BOEID"] = null;

			if (ModelState.IsValid)
			{
				SearchResultsModelView modelView = _ControllerLogic.AdvancedSearchForBOEs(ws, advSearchParams);

				// Return the partial view
				toReturn = View(WebConstants.VIEW_BOE_PROJECTMAP_SEARCH_RESULTS, modelView);

				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			return toReturn;
		}

		/// <summary>
		/// Performs a search for BOEs based on the params passed in
		/// </summary>
		/// <param name="workspace">Name of Workspace containing BOE</param>
		/// <param name="boeID">ID of BOE search is being performed from</param>
		/// <param name="quickSearchParams">Parmeters of the search</param>
		/// <returns></returns>
		public ViewResult QuickSearchForBOEs(string workspace, int? boeID, BOEQuickSearchModelView quickSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			SecurityPage security = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, "SearchForBOEs", security, SecurityAuthorization.Read, ws, boeID);

			if (quickSearchParams == null)
			{
				throw new ArgumentNullException(nameof(quickSearchParams));
			}

			ViewData["BOEID"] = boeID;

			ViewResult toReturn = null;

			if (ModelState.IsValid)
			{
				SearchResultsModelView modelView = _ControllerLogic.QuickSearchForBOEs(ws, boeID, quickSearchParams);

				// Return the partial view
				string view = ws.IsProjectMapWorkspace ? WebConstants.VIEW_BOE_PROJECTMAP_SEARCH_RESULTS : WebConstants.VIEW_BOE_SEARCH_RESULTS;
				toReturn = View(view, modelView);

				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, "SearchForBOEs", sw);
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

			return toReturn;
		}

		/// <summary>
		/// Returns a set of data for the given search results.
		/// </summary>
		/// <param name="workspace">Name of workspace containing the BOE</param>
		/// <param name="boeID">ID of BOE search is being performed in</param>
		/// <param name="searchResults">Modelview of search resultsd</param>
		/// <returns></returns>
		public ViewResult PageSearchResults(string workspace, int? boeID, SearchResultsModelView searchResults)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			SecurityPage security = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, "PageSearchResults", security, SecurityAuthorization.Read, ws, boeID);

			if (searchResults == null)
			{
				throw new ArgumentNullException(nameof(searchResults));
			}

			ViewData["BOEID"] = boeID;
			_ControllerLogic.PageSearchResults(ws, searchResults);

			// Return the partial view
			string view = ws.IsProjectMapWorkspace ? WebConstants.VIEW_BOE_PROJECTMAP_SEARCH_RESULTS : WebConstants.VIEW_BOE_SEARCH_RESULTS;
			ViewResult toReturn = View(view, searchResults);

			ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

			// Finalize Action
			FinalizeAction(_log, "PageSearchResults", sw);
			return toReturn;
		}

		/// <summary>
		/// Perform actions to export BOEs from Manage BOEs page
		/// </summary>
		/// <param name="workspace">Workspace containing BOEs</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public async Task<ActionResult> ExportManageBOE(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			ActionResult result;
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportManageBOE", SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);

			try
			{
				// Get BOEs template file name
				string templateFileName = Server.MapPath("~/Templates/Export/BOEs.xlsm");

				string[] fileNames = await _ControllerLogic.ExportManageBOE(ws, templateFileName, false);

				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				// Finalize Action
				FinalizeAction(_log, "ExportManageBOE", sw);

				result = File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileNames[1]),
					fileDownloadName: fileNames[1]);
			}
			catch (GenValidationException ex)
			{
				result = this.CreateTextFileWithErrorMessage(ex.Message);
			}
			catch (Exception e)
			{
				this._log.Error(e);

				result = this.CreateTextFileWithErrorMessage(e);
			}

			return result;
		}

		/// <summary>
		/// Exports a template of BOEs to be used for imports
		/// </summary>
		/// <param name="workspace">Workspace containing BOEs</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public async Task<ActionResult> ExportManageBOETemplate(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "ExportManageBOETemplate", SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);
			ActionResult result;

			try
			{
				// Get Performing Orgs template file name
				string templateFileName = Server.MapPath("~/Templates/Export/BOEs.xlsm");

				//code used was the same as ExportManageBOE, so can use the same method
				string[] fileNames = await _ControllerLogic.ExportManageBOE(ws, templateFileName, true);

				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				// Finalize Action
				FinalizeAction(_log, "ExportManageBOETemplate", sw);

				result = File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileNames[1]),
					fileDownloadName: fileNames[1]);
			}
			catch (GenValidationException ex)
			{
				result = this.CreateTextFileWithErrorMessage(ex.Message);
			}
			catch (Exception e)
			{
				this._log.Error(e);

				result = this.CreateTextFileWithErrorMessage(e);
			}

			return result;
		}

		/// <summary>
		/// Begins an import of BOEs from an excel file template
		/// </summary>
		/// <param name="workspace">Workspace to import to</param>
		/// <returns></returns>
		public ViewResult ImportManageBOE(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_IMPORT_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
			ICollection<ImportBoeResultsModelView> dataToSave;
			bool errorsOccurred;
			Exception exception;

			Collection<ImportBoeResultsModelView> theModelViews = this._ControllerLogic.ImportManageBOE(ws, this.Request, out dataToSave, out errorsOccurred, out exception);

			if (errorsOccurred)
			{
				this._log.Error(exception);
				this.ViewData["ERRORS_OCCURRED"] = true;
			}

			this.ViewData["SERIALIZED_DATA"] = serializer.Serialize(dataToSave);
			this.ViewData["DOCUMENT_DOMAIN"] = this.Request["documentDomain"];

			ViewResult toReturn = this.View(WebConstants.VIEW_MANAGE_BOE_IMPORT_VERIFICATION, theModelViews);

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_IMPORT_MANAGE_BOE, sw);
			return toReturn;
		}

		/// <summary>
		/// Completes the import of BOEs from an excel file template
		/// </summary>
		/// <param name="workspace">Workspace to import to</param>
		/// <param name="importResults">Modelviews with the results of the import</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public JsonResult CompleteImportManageBOE(string workspace, Collection<ImportBoeResultsModelView> importResults)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_COMPLETE_IMPORT_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn;
			Collection<BoeDTO> NewMaterialBoes = new Collection<BoeDTO>();

			if (importResults != null)
			{
				Collection<ImportBoeResultsModelView> updatedBOEs = new Collection<ImportBoeResultsModelView>(importResults.Where(x => x.ImportType != (int)BoeImportResult.DeleteBoe).ToArray());

				// create a dictionary that will keep track of the BOE ID and its original state before a save
				Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();

				// Create an authorID and approver collection that will keep track of previous author
				// and removed approvers, in case these were changed on the BOE
				Dictionary<int, Collection<UserDTO>> AuthorsChangeDictionary = new Dictionary<int, Collection<UserDTO>>();
				Dictionary<int, Collection<UserDTO>> ApproversChangeDictionary = new Dictionary<int, Collection<UserDTO>>();

				// BOE's Labor Spread values that need to be recalculated because of any changes made during this save
				List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

				// check state validation before worrying about committing to the database 

				ICollection<FullBoe> importedBoes = this.Factory.CreateFullBoes(updatedBOEs.Select(x => x.BoeID).ToList());
				ICollection<BoeDTO> originalUnmodifiedBoes = new Collection<BoeDTO>();
				UserDTO activeUser = ws.CurrentActiveUser;
				List<FullBoe> BoeIdsEffectedByMulti = new List<FullBoe>();
				Collection<int> multiBOEIDsWorkspaceVar = new Collection<int>();

				WbsDTO MultiWbs = ws.MultiBOEWbs;
				ClinDTO MultiClin = ws.MultiBOEClin;

				List<OrdinaryVariableDto> taskVariablesEffectedByMulti = new List<OrdinaryVariableDto>();
				List<WorkspaceVariableDTO> workspaceVariablesEffectedByMulti = new List<WorkspaceVariableDTO>();



				foreach (ImportBoeResultsModelView boeImportResult in updatedBOEs)
				{
					//Check to verify if an author is assigned that an approver is also assigned.
					if (boeImportResult.AuthorIDs.Any() || boeImportResult.SubcontractorAuthorIDs.Any())
					{
						if (boeImportResult.ApproverIDs == null || boeImportResult.ApproverIDs.Count == 0)
						{
							throw new ValidationException("Author assigned, however no approver was assigned.");
						}
					}
					else // an author is required if the boe has been created before or if an approver exists
					{
						if (boeImportResult.BoeID != -1 && boeImportResult.ApproverIDs.Count > 0)
						{
							throw new ValidationException("An author is required.");
						}
					}

					// no point in checking state if it's a new BOE
					if (boeImportResult.BoeID > 0)
					{
						FullBoe originalBOE = importedBoes.First(x => x.Id == boeImportResult.BoeID);
						originalUnmodifiedBoes.Add(originalBOE.DeepClone());
						DataRelationshipVerifier.VerifyDataRelation(originalBOE, ws.Id);

						// move state back to draft
						if (boeImportResult.AuthorIDs.Any() || boeImportResult.SubcontractorAuthorIDs.Any())
						{
							boeImportResult.Status = (int)BOEState.Draft;
						}

						string errorMessage;
						if (!this._boeStateMachine.PerformStateTransitionValidation(originalBOE, ws, originalBOE.State, (BOEState)boeImportResult.Status, out errorMessage))
						{
							// not valid ... communicate to user
							throw new ValidationException(errorMessage);
						}
						else // add to boe state dictionary
						{
							BoeStateDictionary[boeImportResult.BoeID] = originalBOE.State;
						}

					}
				}

				Collection<BoeDTO> boesToSave = new Collection<BoeDTO>();
				Collection<int> ClinIDsToRecalculateLaborSpread = new Collection<int>();
				Collection<int> WbsIDsToRecalculateLaborSpread = new Collection<int>();
				Collection<BoeDTO> BoesToBeDeleted = new Collection<BoeDTO>();
				Dictionary<BoeDTO, Collection<int>> boeInformationCollection = new Dictionary<BoeDTO, Collection<int>>();
				List<BoeApproverResponseDTO> boeApproverResponses = new List<BoeApproverResponseDTO>();
				Collection<BoeTaskElementDTO> laborElementsUpdated = new Collection<BoeTaskElementDTO>();
				Collection<TravelDTO> travelElementsUpdated = new Collection<TravelDTO>();

				//Convert to use BOE DTOs

				int insertApproverId = -1;
				foreach (ImportBoeResultsModelView boeImportResult in importResults)
				{
					FullBoe boe;
					if (boeImportResult.BoeID >= 0 && boeImportResult.ImportType == (int)BoeImportResult.UpdateBoe)
					{
						boe = importedBoes.First(x => x.Id == boeImportResult.BoeID);
					}
					else
					{
						boe = this.Factory.CreateFullBoe();
						boe.Id = boeImportResult.BoeID;
						boe.Title = boeImportResult.BOETitle;
						this._ControllerLogic.setDefaultBoeDates(ws, boe, boeImportResult.ClinID);
					}

					//if the BOE's CLIN was changed, need to recalculate labor spread for any other BOE that had either the old CLIN or the new CLIN referenced
					if (boeImportResult.ClinID != boe.CLINID)
					{
						if (boeImportResult.ClinID.HasValue)
						{
							ClinIDsToRecalculateLaborSpread.Add(boeImportResult.ClinID.Value);
						}

						// if this is a new CLIN association, no need to recalculate
						if (boe.CLINID.HasValue)
						{
							ClinIDsToRecalculateLaborSpread.Add(boe.CLINID.Value);
						}
					}
					boe.CLINID = boeImportResult.ClinID;
					// if the BOE's WBS was changed, need to recalculate labor spread for any other BOE that had either the old WBS or the new WBS referenced
					if (boeImportResult.WbsID != boe.WBSID && boeImportResult.WbsID.HasValue)
					{
						WbsIDsToRecalculateLaborSpread.Add(boeImportResult.WbsID.Value);

						// if this is a new WBS association, no need to recalculate
						if (boe.WBSID.HasValue)
						{
							WbsIDsToRecalculateLaborSpread.Add(boe.WBSID.Value);
						}
					}
					boe.WBSID = boeImportResult.WbsID;
					boe.WCBID = boeImportResult.BoeXrefID;


					if (boeImportResult.IsMultiClinWbs)
					{
						//if this is a boe and wasnt a multi we'll default the resources to the old values
						if (boe.Id > 0 && !boe.IsMultiClinWbs)
						{
							boe.TaskElements.SelectMany(t => t.taskElementLabors.Select(l => { l.Updateable = UpdateType.Upsert; l.WBSID = boe.WBSID; l.CLINID = boe.CLINID; return l; })).ToCollection();
							laborElementsUpdated = laborElementsUpdated.Concat(boe.TaskElements).ToCollection();
							//delete the travel and odc elements
							travelElementsUpdated = travelElementsUpdated.Concat(boe.Travels.Select(t => { t.Updateable = UpdateType.Deleted; return t; })).ToCollection();

							multiBOEIDsWorkspaceVar.Add(boe.Id);
							BoeIdsEffectedByMulti.Add(boe);
						}

						boe.WBSID = ws.MultiBOEWbs.Id;
						boe.CLINID = ws.MultiBOEClin.Id;
						boe.IsMultiClinWbs = boeImportResult.IsMultiClinWbs;


					}
					//the boe is not a multi
					else
					{
						//this is not a multi boe, but a user may have selected either a multi wbs/clin or both.. this is extra validation.
						if (boeImportResult.WbsID == MultiWbs.Id && boeImportResult.ClinID == MultiClin.Id)
						{
							if (boe.Id > 0)
							{
								boe.TaskElements.SelectMany(t => t.taskElementLabors.Select(l => { l.Updateable = UpdateType.Upsert; l.WBSID = boe.WBSID; l.CLINID = boe.CLINID; return l; })).ToCollection();
								laborElementsUpdated = laborElementsUpdated.Concat(boe.TaskElements).ToCollection();
								//delete the travel and odc elements
								travelElementsUpdated = travelElementsUpdated.Concat(boe.Travels.Select(t => { t.Updateable = UpdateType.Deleted; return t; })).ToCollection();

								multiBOEIDsWorkspaceVar.Add(boe.Id);
								BoeIdsEffectedByMulti.Add(boe);
							}
							//user set both clin and wbs to multi. this is a multi boe
							boeImportResult.IsMultiClinWbs = true;
							boe.WBSID = ws.MultiBOEWbs.Id;
							boe.CLINID = ws.MultiBOEClin.Id;
						}
						else if (boeImportResult.ClinID == MultiClin.Id)
						{//if import is trying to get a multi clin in, null it out.
							boe.CLINID = null;
						}
						else if (boeImportResult.WbsID == MultiWbs.Id)
						{
							//if its not a multi boe and the clin is not a multi clin then null out the wbs.
							boe.WBSID = null;
						}

						//if the boe is not a multi and an existing boe we need to see if the old boe was a multi boe
						if (boe.Id > 0 && boe.IsMultiClinWbs)
						{
							//lets set all the task elements in the boe with null wbs and clins on the resource level.
							boe.TaskElements.SelectMany(t => t.taskElementLabors.Select(l => { l.Updateable = UpdateType.Upsert; l.WBSID = null; l.CLINID = null; return l; })).ToCollection();
							laborElementsUpdated = laborElementsUpdated.Concat(boe.TaskElements).ToCollection();

						}
					}

					boe.IsMultiClinWbs = boeImportResult.IsMultiClinWbs;

					// If the BOE State is unassigned but an author was added, automatically change state to draft.
					// Otherwise, the state is selected by the dropdown from the model view.
					if (((BOEState)boeImportResult.Status == BOEState.Unassigned || boeImportResult.BoeID < 0) && (boeImportResult.AuthorIDs.Any() || boeImportResult.SubcontractorAuthorIDs.Any()))
					{
						boe.State = BOEState.Draft;
					}
					else
					{
						boe.State = (BOEState)boeImportResult.Status;
					}

					boe.AuthorIDs = boeImportResult.AuthorIDs.Any() ? boeImportResult.AuthorIDs : null;
					boe.SubcontractorAuthorIDs = boeImportResult.SubcontractorAuthorIDs.Any() ? boeImportResult.SubcontractorAuthorIDs : null;


					// If the authors were changed, we need to save the old list of authors to pass to our email function
					if (boe.Id >= 0)
					{

						ICollection<PermissionsDTO> permissionData = this.PermissionsLoader.GetBOEPermissions(new List<int>() { boe.Id });
						ICollection<PermissionsDTO> Authors = permissionData.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).ToArray();
						ICollection<PermissionsDTO> BoeApprovers = permissionData.Where(x => x.Role == Role.Approver).ToArray();

						Collection<int> userIds = Authors.Select(x => x.ETIUserId).Union(BoeApprovers.Select(x => x.ETIUserId)).Distinct().ToCollection();
						ICollection<UserDTO> userData = this.UserLoader.GetByIds(userIds);

						bool authorsRemoved = false;
						bool authorsAdded = false;

						if (boe.AuthorIDs != null)
						{
							authorsRemoved = (from removedAuthor in Authors
	  where !boe.AuthorIDs.Contains(removedAuthor.ETIUserId)
	  select removedAuthor).Any();

							authorsAdded = (from addedAuthor in boe.AuthorIDs
	where !(Authors.Select(a => a.ETIUserId).Contains(addedAuthor))
	select addedAuthor).Any();
						}

						if (authorsRemoved || authorsAdded)
						{
							IEnumerable<UserDTO> oldAuthors = from oldAuthor in Authors
	 select userData.First(x => x.UserID == oldAuthor.ETIUserId);

							AuthorsChangeDictionary[boe.Id] = new Collection<UserDTO>(oldAuthors.ToArray());
						}

						bool approversRemoved = (from removedApprover in BoeApprovers
		where !boeImportResult.ApproverIDs.Contains(removedApprover.ETIUserId)
		select removedApprover).Any();

						bool approversAdded = (from addedApprover in boeImportResult.ApproverIDs
	  where !(BoeApprovers.Select(a => a.ETIUserId).Contains(addedApprover))
	  select addedApprover).Any();

						if (approversRemoved || approversAdded)
						{
							IEnumerable<UserDTO> oldApprovers = from oldApprover in BoeApprovers
	   select userData.First(x => x.UserID == oldApprover.ETIUserId);

							ApproversChangeDictionary[boe.Id] = new Collection<UserDTO>(oldApprovers.ToArray());
						}
					}

					// Get Approvers
					foreach (int BoeApproverID in boeImportResult.ApproverIDs)
					{
						BoeApproverResponseDTO boeApprover = new BoeApproverResponseDTO();
						BoeApproverResponseDTO originalBoeApprover = boe.ApproverResponses.FirstOrDefault(a => a.ETIUserID == BoeApproverID);
						int approvalID = originalBoeApprover == null ? insertApproverId : originalBoeApprover.Id;
						boeApprover.Id = approvalID;
						boeApprover.ETIUserID = BoeApproverID;
						boeApprover.BoeID = boeImportResult.BoeID;
						boeApprover.UpdateDate = originalBoeApprover != null ? originalBoeApprover.UpdateDate : new DateTime();
						boeApprover.Updateable = UpdateType.Upsert;
						boeApprover.CurrentUserETIUserID = activeUser.UserID;
						boeApproverResponses.Add(boeApprover);
						insertApproverId--;
					}

					// if any approvers were deleted, mark them as such
					if (boe.Id > 0)
					{
						ICollection<BoeApproverResponseDTO> approversRemoved = boe.ApproverResponses.Where(approvers => !boeImportResult.ApproverIDs.Contains(approvers.ETIUserID)).ToCollection();

						if (approversRemoved.Any())
						{
							foreach (BoeApproverResponseDTO approver in approversRemoved)
							{

								approver.Updateable = UpdateType.Deleted;
								approver.CurrentUserETIUserID = activeUser.UserID;
								boeApproverResponses.Add(approver);
							}
						}
					}

					boe.WorkspaceID = ws.Id;

					if (boeImportResult.ImportType == (int)BoeImportResult.DeleteBoe)
					{
						boe.Updateable = UpdateType.Deleted;
						boe.UpdateDate = ws.Boes.First(i => i.Id == boe.Id).UpdateDate;
						BoesToBeDeleted.Add(boe);
						boeInformationCollection[boe] = boeImportResult.ApproverIDs;
					}
					else
					{
						boe.Updateable = UpdateType.Upsert;
					}
					boe.isMaterial = boeImportResult.isMaterial;

					// only add new material boes to list
					if (boeImportResult.isMaterial && boe.Id < 0)
					{
						NewMaterialBoes.Add(boe);
					}
					boesToSave.Add(boe);
				}

				// Check for circular references before saving
				VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();

				Collection<FullClin> clinsForBoesToSave = ws.Clins.Where(z => boesToSave.Where(x => x.CLINID.HasValue).Select(x => x.CLINID.Value).Contains(z.Id)).ToCollection();
				Collection<FullWbs> wbsElementsForBoesToSave = ws.WbsElements.Where(z => boesToSave.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).Contains(z.Id)).ToCollection();
				ICollection<FullBoe> fullBoesToSave = new List<FullBoe>();

				foreach (BoeDTO boe in boesToSave)
				{
					fullBoesToSave.Add(this.Factory.CreateFullBoe(boe));
				}

				foreach (FullBoe updatedBOE in fullBoesToSave)
				{
					if (updatedBOE.Id > 0)
					{
						bool circularReferenceFound = false;

						if (updatedBOE.WBSID.HasValue)
						{
							FullWbs wbs = wbsElementsForBoesToSave.First(x => x.Id == updatedBOE.WBSID.Value);

							// Validate chosen WBS for circular references
							if (this._VariableCircularReferenceChecker.BOEWBSMoveCreatesCircularReference(cache, updatedBOE, wbs, boesToSave, ws))
							{
								circularReferenceFound = true;
							}
						}

						if (updatedBOE.CLINID.HasValue)
						{
							ClinDTO clin = clinsForBoesToSave.First(x => x.Id == updatedBOE.CLINID.Value);

							// Validate chosen CLINs for circular references
							if (this._VariableCircularReferenceChecker.BOECLINMoveCreatesCircularReference(ws, cache, updatedBOE, clin, boesToSave))
							{
								circularReferenceFound = true;
							}
						}

						// If a circular reference is found, don't save the offending BOE
						if (circularReferenceFound)
						{
							updatedBOE.Updateable = UpdateType.None;
						}
					}
				}

				Collection<int> BoeIDs = new Collection<int>();
				IDictionary<int, int> boeSaveIDDict;
				_ = ws.MoqTypeSelections; // preload the data prior to transaction

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					if (travelElementsUpdated.Any())
					{
						//updating travels that need to be deleted and odc.
						this._TravelDTOLoader.SaveTravels(travelElementsUpdated);
					}
					if (laborElementsUpdated.Any())
					{
						//updating resource types that need to change. 
						this._BoeTaskElementMediator.MediatedSaveTaskElements(laborElementsUpdated, ws);

						ws.RefreshTaskElements();
					}
					// if BOEs were marked to be deleted, get all the task elements that would be effected by this delete before it's actually deleted
					foreach (BoeDTO boe in BoesToBeDeleted)
					{
						FullBoe boeObject = this.Factory.CreateFullBoe(boe);

						boeTaskElementsToRecalculate.AddRange(
							this._BoeTaskElementRecalculation.RecalculateLaborWithBoe(boeObject, VariableType.Task, ws)
							.Where(recalculatedTask => boeTaskElementsToRecalculate.Count(task => task.Id == recalculatedTask.Id) == 0));

						// if the approver response is mapped to a deleted BOE, then we shouldn't bother with saving them
						boeApproverResponses.RemoveAll(x => x.BoeID == boe.Id);
					}

					foreach (FullBoe boe in BoeIdsEffectedByMulti)
					{
						this._ControllerLogic.RemoveMultiBOEReferenceWorkspaceVar(ws, workspaceVariablesEffectedByMulti, BoeIdsEffectedByMulti.Select(b => b.Id).ToCollection());
						taskVariablesEffectedByMulti.AddRange(FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(boe.Id, ws));
					}
					foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesEffectedByMulti)
					{
						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(workspaceVar.Id, VariableType.Workspace, ws)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}
					Collection<BoeTaskElementDTO> taskAffectedByMutliBOE = new Collection<BoeTaskElementDTO>(this._ControllerLogic.RemoveMultiBOEReferenceTaskVar(ws, BoeIdsEffectedByMulti.Select(b => b.Id).ToList()));
					// check if any task elements need to be recalculated that were effected by a multi boe being changed
					// check task variables and workspace variables separately
					foreach (OrdinaryVariableDto taskVar in taskVariablesEffectedByMulti)
					{

						boeTaskElementsToRecalculate.AddRange(from t in this._BoeTaskElementRecalculation.RecalculateLaborWithVariable(taskVar.Id, VariableType.Task, ws, taskAffectedByMutliBOE)
					  where !(from o in boeTaskElementsToRecalculate
							  select o.Id).Contains(t.Id)
					  select t);
					}

					// Save approvers for existing BOEs so they can be copied correctly in the mediator.
					BoeApproverResponseDTO[] boeApproversToSave = boeApproverResponses.Where(x => x.BoeID > 0).ToArray();
					this._BoeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));

					// Need to see if we need to create a WS level role for the user (if the permissions are being granted via a group)
					Collection<PermissionsDTO> wsPermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);

					foreach (BoeApproverResponseDTO boeapprover in boeApproverResponses)
					{
						Collection<PermissionsDTO> wsApproverPermissionsForUser = wsPermissions.Where(x => x.ETIUserId == boeapprover.ETIUserID && x.Role == Role.Approver).ToCollection();

						if (!wsApproverPermissionsForUser.Any())
						{
							PermissionsDTO permission = new PermissionsDTO();

							// need to insert a potential WS permission based on the permission dto
							permission.Id = -1;
							permission.Role = Role.Approver;
							permission.Updateable = UpdateType.Upsert;
							permission.BOEId = null;
							permission.WorkspaceId = ws.Id;
							permission.PermissionId = -1;
							permission.ETIUserId = boeapprover.ETIUserID;

							this.PermissionsLoader.SavePermission(permission);
						}
					}

					this._ControllerLogic.DeleteMoqTypesForBoe(ws, BoesToBeDeleted.Select(x => x.Id).ToList());

					boeSaveIDDict = this._BoeMediator.MediatedSaveBOEs(ws, boesToSave);

					// Create material task elements for new material boes
					int newMaterialID = -1;
					foreach (BoeDTO boe in NewMaterialBoes)
					{
						// add a Material Task Element.
						MaterialDTO MaterialDTOtoSave = new MaterialDTO();
						MaterialDTOtoSave.BoeID = boeSaveIDDict[boe.Id];
						MaterialDTOtoSave.Id = newMaterialID;
						MaterialDTOtoSave.TaskTitle = "Material";
						MaterialDTOtoSave.Updateable = UpdateType.Upsert;

						newMaterialID--;

						this._MaterialLoader.SaveMaterials(new Collection<MaterialDTO> { MaterialDTOtoSave });
					}

					//Save Boe Approvers for new BOEs
					boeApproversToSave = boeApproverResponses.Where(x => x.BoeID < 0).ToArray();
					int count = 0;
					foreach (BoeApproverResponseDTO boeapprover in boeApproversToSave)
					{
						// need to make sure the negative ids are unique
						boeapprover.Id = boeapprover.Id - count;
						count++;
						boeapprover.BoeID = boeSaveIDDict[boeapprover.BoeID];
					}

					this._BoeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));

					// need to get the BOE task elements that were effected by the above BOE save

					ICollection<FullClin> clinsEffected = this.Factory.CreateFullClins(ClinIDsToRecalculateLaborSpread.Distinct().ToList());
					foreach (FullClin clin in clinsEffected)
					{
						boeTaskElementsToRecalculate.AddRange(
							this._BoeTaskElementRecalculation.RecalculateLaborWithClin(clin, VariableType.Task, ws)
							.Where(recalculatedTask => boeTaskElementsToRecalculate.Count(task => task.Id == recalculatedTask.Id) == 0));
					}

					ICollection<FullWbs> wbsEffected = this.Factory.CreateFullWbses(WbsIDsToRecalculateLaborSpread.Distinct().ToList());
					foreach (FullWbs wbsObject in wbsEffected)
					{
						boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Task, ws)
							.Where(recalculatedTask => boeTaskElementsToRecalculate.Count(task => task.Id == recalculatedTask.Id) == 0));
					}

					// get the unique BOE IDs from boeTaskElementsToRecalculate so we can set their state back to Draft
					Collection<int> BoeIDsToCheck = new Collection<int>(boeTaskElementsToRecalculate.Where(x => x.BoeID > 0).Select(x => x.BoeID).ToList());
					ICollection<FullBoe> fullBoes = this.Factory.CreateFullBoes(BoeIDsToCheck);

					foreach (FullBoe boe in fullBoes)
					{
						BOEState oldBOEState = boe.State;
						if (boe.State == BOEState.Approved || boe.State == BOEState.AwaitingApproval || boe.State == BOEState.DraftLocked)
						{
							BoeIDs.Add(boe.Id);

							BOEState newBOEState = BOEState.Draft;

							// Validate the Awaiting Approval or Approved to Draft state transition
							string validationMessage;
							if (!this._boeStateMachine.PerformStateTransitionValidation(boe, ws, boe.State, newBOEState, out validationMessage))
							{
								// not valid ... communicate to user
								throw new ValidationException(validationMessage);
							}

							// If the transition is valid, set the BOE to Draft and save it
							boe.Updateable = UpdateType.Upsert;
							boe.State = newBOEState;
							this._BoeMediator.MediatedSave(ws, boe);

							// Perform common state transition actions
							this._boeStateMachine.PerformStateTransitionAction(boe, ws, oldBOEState, boe.State);
						}
					}

					// save all the task elements that were effected by a BOE deletion or a CLIN/WBS remapping
					boeTaskElementsToRecalculate = boeTaskElementsToRecalculate.Distinct().ToList();
					this._BoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);
					scope.Complete();
				}

				IDictionary<int, BOEStateModelView> boeStateDictionary = this._CommonDataMapper.getBOEStatesDictionary();

				// emails need to be sent after the save
				foreach (FullBoe fullBoe in fullBoesToSave)
				{
					// if the boe has just been deleted, need to send BOE Deleted email to
					// approvers, author, and workspace admins
					if (fullBoe.Updateable == UpdateType.Deleted)
					{
						foreach (KeyValuePair<BoeDTO, Collection<int>> keyValuePair in boeInformationCollection)
						{
							BoeDTO boeMarkedForDelete = keyValuePair.Key;
							Collection<int> approverIds = keyValuePair.Value;
							WbsDTO wbsAssociatedWithBoe = ws.WbsElements.FirstOrDefault(w => w.Id == boeMarkedForDelete.WBSID);
							ClinDTO clinAssociatedwithBoe = ws.Clins.FirstOrDefault(c => c.Id == boeMarkedForDelete.CLINID);

							if (boeMarkedForDelete.Id == fullBoe.Id)
							{
								this._emailer.SendBOEDeleted(boeMarkedForDelete, activeUser, approverIds, wbsAssociatedWithBoe, clinAssociatedwithBoe, ws, boeStateDictionary);
							}
						}
					}
					else
					{

						if (BoeStateDictionary.ContainsKey(fullBoe.Id))
						{
							this._boeStateMachine.PerformStateTransitionAction(fullBoe, ws, BoeStateDictionary[fullBoe.Id], fullBoe.State);
						}

						// If the BOE has been put in draft mode, send BOE author email opened for edit.
						// else Send the 'Author Changed' email
						// only send these emails if the workspace is in working
						if (ws.WorkspaceState == WorkspaceState.Working)
						{
							if (AuthorsChangeDictionary.ContainsKey(fullBoe.Id))
							{
								if (fullBoe.State == BOEState.Draft && BoeStateDictionary[fullBoe.Id] == BOEState.Unassigned)
								{
									this._emailer.SendBOEAuthorsEmailOpenedForEdit(fullBoe, ws);
								}
								else
								{
									this._emailer.SendBOEAuthorsChanged(AuthorsChangeDictionary[fullBoe.Id], fullBoe);
								}
							}
						}

						// Send the 'Approvers Changed' email, if applicable
						if (ApproversChangeDictionary.ContainsKey(fullBoe.Id) && fullBoe.State == BOEState.AwaitingApproval && ws.WorkspaceState == WorkspaceState.Working)
						{
							this._emailer.SendBOEApproversChanged(ApproversChangeDictionary[fullBoe.Id], fullBoe);
						}

						// Send CLIN/WBS updated email, if applicable
						BoeDTO originalBOE = originalUnmodifiedBoes.FirstOrDefault(b => b.Id == fullBoe.Id);
						if (originalBOE != null)
						{
							bool clinChanged = originalBOE.CLINID != fullBoe.CLINID;
							bool wbsChanged = originalBOE.WBSID != fullBoe.WBSID;
							bool notMultiOrChanged = !fullBoe.IsMultiClinWbs || !originalBOE.IsMultiClinWbs;
							if (notMultiOrChanged && (clinChanged || wbsChanged))
							{
								// It is an updated BOE, and either CLIN or WBS was changed
								this._emailer.SendBOECLINWBSChanged(fullBoe, clinChanged, wbsChanged, false);
							}
						}
						else if (fullBoe.CLINID.HasValue || fullBoe.WBSID.HasValue)
						{
							int realBoeId = boeSaveIDDict[fullBoe.Id];
							FullBoe boe = this.Factory.CreateFullBoe(realBoeId);
							// It is a new BOE, and either CLIN or WBS was set 
							this._emailer.SendBOECLINWBSChanged(boe, boe.CLINID.HasValue, boe.WBSID.HasValue, false);
						}
					}
				}
			}

			toReturn = this.Json(new { Status = true });

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_COMPLETE_IMPORT_MANAGE_BOE, sw);
			return toReturn;
		}

		/// <summary>
		/// Determines any conflicts when copying a BOE
		/// </summary>
		/// <param name="workspace">Workspace containing BOE to be copied to</param>
		/// <param name="boeID">ID of BOE to be copied to</param>
		/// <param name="copyBOEID">ID of BOE to be copied</param>
		/// <param name="taskElementsToCopy">Task elements of BOE to be copied</param>
		/// <param name="travelElementsToCopy">Travel elements of BOE to be copied</param>
		/// <returns></returns>
		public ViewResult DisplayCopyBOEConflicts(string workspace, int boeID, int copyBOEID, ICollection<int> taskElementsToCopy, ICollection<int> travelElementsToCopy)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			if (taskElementsToCopy == null)
			{
				taskElementsToCopy = new Collection<int>();
			}
			Stopwatch sw = InitializeAction(_log, "DisplayCopyBOEConflicts", SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);

			ViewResult toReturn = null;

			BOECopyConflictsModelView boeCopyConflictsModelView = _ControllerLogic.DisplayCopyBOEConflicts(ws, boeID, copyBOEID, taskElementsToCopy, travelElementsToCopy);
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

			toReturn = View(WebConstants.VIEW_BOE_COPY_CONFLICTS, boeCopyConflictsModelView);

			// Finalize Action
			FinalizeAction(_log, "DisplayCopyBOEConflicts", sw);
			return toReturn;
		}

		/// <summary>
		/// Performs actions to copy BOE
		/// </summary>
		/// <param name="workspace">Workspace containing BOE to be copied to</param>
		/// <param name="boeID">ID of BOE to be copied to</param>
		/// <param name="copyBOEID">ID of BOE to be copied</param>
		/// <param name="taskElementsToCopy">Task elements of BOE to be copied</param>
		/// <param name="travelElementsToCopy">Travel elements of BOE to be copied</param>
		/// <returns></returns>
		public ActionResult SaveCopyOfBOE(string workspace, int? boeID, int copyBOEID, Collection<int> taskElementsToCopy, Collection<int> travelElementsToCopy)
		{
			// Perform Action
			JsonResult toReturn = Json(new { Status = true });
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			if (taskElementsToCopy == null)
			{
				taskElementsToCopy = new Collection<int>();
			}

			if (travelElementsToCopy == null)
			{
				travelElementsToCopy = new Collection<int>();
			}

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveCopyOfBOE", SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					_BOECopier.CopyBOE(ws, copyBOEID, boeID, taskElementsToCopy);

					scope.Complete();
				}

				// Process task variable dependencies
				this._BoeLaborControllerLogic.ProcessAllVariableDependencies(boeID.Value, ws);
			}

			// Finalize Action
			FinalizeAction(_log, "SaveCopyOfBOE", sw);
			return toReturn;
		}

		/// <summary>
		/// Performs actions to copy Project Map
		/// </summary>
		/// <param name="workspace">Workspace to be copied to</param>
		/// <param name="copyProjectMapId">Id of Project Map to be copied</param>
		/// <returns>Json status for saving a copy of a Project Map.</returns>
		public ActionResult SaveCopyOfProjectMap(string workspace, int copyProjectMapId)
		{
			// Perform Action
			JsonResult toReturn = Json(new { Status = true });
			FullProjectMapWorkspace ws = this.Factory.CreateFullProjectMapWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveCopyOfProjectMap", SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this._ControllerLogic.CopyProjectMap(ws, copyProjectMapId);

					scope.Complete();
				}
			}

			// Finalize Action
			FinalizeAction(_log, "SaveCopyOfProjectMap", sw);
			return toReturn;
		}

		#endregion AJAX Calls

		/// <summary>
		/// Reorders the taskelement for labor task
		/// </summary>
		/// <param name="theModelView">user order</param>
		/// <param name="workspace">current workspace</param>
		/// <param name="boeID">current boe</param>
		/// <returns></returns>
		public JsonResult SaveReorderLaborTaskElements(TaskElementOrderCollection theModelView, string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boeObject = this.Factory.CreateFullBoe(boeID);

			if (theModelView == null)
			{
				throw new ArgumentNullException(nameof(theModelView));
			}

			if (boeObject.TaskElements.Count != theModelView.BOETaskElements.Count)
			{
				throw new ValidationException("Number of Task Elements in save does not match Number of Task Elements in Database.");
			}

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveReorderLaborTaskElements", SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			_ControllerLogic.ReOrderTaskElementOrder(ws, boeObject, theModelView);

			JsonResult toReturn = Json(new { Status = true });


			// Finalize Action
			FinalizeAction(_log, "SaveReorderLaborTaskElements", sw);
			return toReturn;
		}

		/// <summary>
		/// Saves duplicates of task elements.
		/// </summary>
		/// <param name="theModelView">User's request of task ids and number of copies for each task</param>
		/// <param name="workspace">Current workspace</param>
		/// <param name="boeID">Current BOE Id</param>
		/// <param name="taskType">Task Element type</param>
		/// <returns>Returns a status of true if successful; otherwise validation errors are returned</returns>
		public JsonResult SaveDuplicateTaskElements(TaskElementDuplicateFormCollection theModelView, string workspace, int boeID, TaskType taskType)
		{
			if (theModelView == null)
			{
				throw new ArgumentNullException(nameof(theModelView));
			}

			if (!ModelState.IsValid)
			{
				Collection<ValidationMessage> errors = Utilities.CreateModelStateValidationErrorList(ModelState);
				return Json(new { Status = false, Results = new { ValidationMessages = errors } });
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boeObject = this.Factory.CreateFullBoe(boeID);

			theModelView.TaskType = taskType;

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "SaveDuplicateTaskElements", SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			try
			{
				Dictionary<int, int> duplicateRequest = theModelView.DuplicateTaskRequests.ToDictionary(x => x.TaskID, y => y.DuplicateCount);

				switch (theModelView.TaskType)
				{
					case (TaskType.Labor):
						{
							boeObject.LoadTaskElementRTEData();

							using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
							{
								_BOECopier.DuplicateTasksInABoe(duplicateRequest, boeObject, ws);
								scope.Complete();
							}

							break;
						}
					case (TaskType.Travel):
						{
							boeObject.LoadTravelRTEData();

							using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
							{
								_TravelControllerLogic.DuplicateTravelTaskElements(duplicateRequest, boeObject);
								scope.Complete();
							}
							break;
						}
					default:
						{
							throw new GenValidationException("An invalid Task Type was used.");
						}
				}
			}
			catch (SystemException ex)
			{   // Catch any timeout exceptions and request user to make fewer duplicates
				// There are several exceptions that could be thrown, but all seem to have 1 of 2 base exceptions:
				if (ex.GetBaseException() is System.InvalidOperationException ||
					ex.GetBaseException() is System.TimeoutException)
				{
					return Json(new { Status = false, Results = new { ValidationMessages = new List<ValidationMessage> { new ValidationMessage("", "Duplicate Task request could not be completed. Please decrease the number of tasks being duplicated, or the number of copies per task and try again.", null) } } });
				}
				else
				{
					throw;
				}
			}

			// Process task variable dependencies if a labor task was duplicated
			if (theModelView.TaskType == TaskType.Labor)
			{
				this._BoeLaborControllerLogic.ProcessAllVariableDependencies(boeID, ws);
			}

			JsonResult toReturn = Json(new { Status = true });

			// Finalize Action
			FinalizeAction(_log, "SaveDuplicateTaskElements", sw);
			return toReturn;
		}

		/// <summary>
		/// Save Bulk Boe Roles
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeRolesToSave">Boe Roles to Save</param>
		public JsonResult SaveBoeBulkRoles(string workspace, ICollection<ManageBOEModelView> boeRolesToSave)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, "SaveBoeBulkRoles", SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			IList<string> errorMessages = this._ControllerLogic.SaveBoeBulkRoles(ws, boeRolesToSave);

			JsonResult response = Json(new { Status = true, ErrorMessages = errorMessages });

			FinalizeAction(_log, "SaveBoeBulkRoles", sw);

			return response;
		}
	}

	internal class BOEStateTransition
	{
		public int ID { get; set; }
		public BOEState OldState { get; set; }
		public BOEState NewState { get; set; }
	}
}
