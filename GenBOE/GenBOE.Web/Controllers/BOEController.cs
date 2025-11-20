// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
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

	public class BOEController : GenBOEController
	{
		private Logger _log = new Logger(typeof(BOEController));
		private BoeEmailer _emailer = null;
		private BOEStateMachine _boeStateMachine = null;
		private IValidateBOE _validateBOE = null;
		private BoeTaskElementRecalculation _BoeTaskElementRecalculation = null;
		private VariableCircularReferenceChecker _VariableCircularReferenceChecker = null;
		private ActionLogic.CopyBOE.BOECopier _BOECopier = null;
		private BoeApproverResponseDTODataLoader _BoeApproverResponseLoader = null;
		private IMaterialDTODataLoader _MaterialLoader = null;
		private BoeTaskElementMediator _BoeTaskElementMediator = null;
		private BoeMediator _BoeMediator = null;
		private IBOEControllerLogic _ControllerLogic = null;
		private IBOELaborControllerLogic _BoeLaborControllerLogic = null;
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
			IValidateBOE inValidateBOE,
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
			_validateBOE = inValidateBOE;
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
			this.ViewData["IsSkillMixFeatureEnabled"] = Utilities.IsSkillMixEnabledForSystem;
			this.ViewData["IsSkillMixEnabled"] = Utilities.ShowSkillMixForWorkspace(ws.CreationDate, ws.Shortname);
			this.ViewData["IsUCOTEnabled"] = Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.Shortname);

			// Additional Controls for SkillMixHelperText
			ViewData["UsingTemplateBOE"] = ws.UsingTemplateBOE;

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

			bool enableTaskAuthor = Utilities.IsAssignTaskAuthorEnabledForSystem && ws.EnableAssignTaskAuthor;
			ICollection<UserDTO> availableTaskAuthors = enableTaskAuthor ? UserLoader.GetBoeAuthors(boeID) : null;

			this.ViewData["WBSElements"] = WBSElements;
			this.ViewData["CLINElements"] = ClinElements;
			this.ViewData["SpreadCurvesCost"] = costCurves;
			this.ViewData["SpreadCurvesHours"] = hourCurves;
			this.ViewData["CostDecimalPrecision"] = ws.CostDecimalPrecision;
			this.ViewData["DecimalPrecision"] = ws.DecimalPrecision;
			this.ViewData["SapFields"] = await _ControllerLogic.GetAllFields();
			this.ViewData["SapOperators"] = await _ControllerLogic.GetAllOperators();
			this.ViewData["EnableSAPConnection"] = ws.EnableSAPConnection;
			this.ViewData["EnableTaskAuthor"] = enableTaskAuthor;
			this.ViewData["BoeAuthors"] = availableTaskAuthors;

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
		[HttpGet]
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
		[HttpGet]
		public ViewResult LoadDuplicateTaskDialog(string workspace, int boeID, TaskType taskType)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boeObject = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_LOAD_DUPLICATE_TASK_DIALOG, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			// Perform Action
			ViewData["BOEID"] = boeID;
			TaskElementDuplicateFormCollection theModelView = _ControllerLogic.GetDuplicateTaskModelView(boeObject, taskType);

			theModelView.ContainsOCI = ws.ContainsOCI;

			// Perform Action
			ViewResult toReturn = View(WebConstants.VIEW_DUPLICATE_TASK_DIALOG, theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_LOAD_DUPLICATE_TASK_DIALOG, sw);

			return toReturn;
		}

		#region Edit BOE

		/// <summary>
		/// Displays the Task Element Grid Partial View
		/// </summary>
		/// <param name="workspace">Workspace Shortname</param>
		/// <param name="boeID">BOE ID</param>
		/// <returns>DisplayTaskElementGrid ActionResult</returns>
		[HttpPost]
		public virtual ViewResult DisplayTaskElementGrid(string workspace, int boeID)
		{
			FullWorkspace workspaceObject = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = workspaceObject.Boes.First(x => x.Id == boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_TASK_ELEMENT_GRID, SecurityPage.BOELaborGrid, SecurityAuthorization.Read, workspaceObject, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_TASK_ELEMENT_GRID, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the Validation Results view
		/// </summary>
		/// <param name="workspace">Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>DisplayBOEValidateResults ActionResult</returns>
		[HttpGet]
		public virtual ViewResult DisplayBOEValidateResults(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_VALIDATE_RESULTS, SecurityPage.ValidateBOE, SecurityAuthorization.Read, ws, boeID);

			ViewData["BOEID"] = boeID;

			ViewResult toReturn = View(WebConstants.VIEW_BOE_VALIDATION_RESULTS);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_VALIDATE_RESULTS, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the boe offload results.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="boeId">The boe identifier.</param>
		/// <returns>DisplayBOEOffloadResults View.</returns>
		[HttpGet]
		public virtual ViewResult DisplayBOEOffloadResults(string workspace, int boeId)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_OFFLOAD_RESULTS, SecurityPage.BOELaborGrid, SecurityAuthorization.Read, ws, boeId);

			BoeOffloadModelView model = this._ControllerLogic.RetrieveBoeOffloadData(ws, boeId);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_OFFLOAD_RESULTS, model);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_OFFLOAD_RESULTS, sw);
			return toReturn;
		}

		/// <summary>
		/// Returns The BOE Header (upper portion)
		/// </summary>
		/// <param name="workspace">THe Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>The view for the BOE Header</returns>
		[HttpGet, ChildActionOnly]
		public virtual ViewResult DisplayBOEHeaderDescription(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HEADER_DESCRIPTION, SecurityPage.EditBOEHeaderDescription,
				SecurityAuthorization.Read, ws, boeID);

			ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boeID).Where(t => t.SourceId == (int)RteTemplateSource.BoeDescription).OrderBy(r => r.SortOrder).ToList();
			ViewResult toReturn = View(WebConstants.VIEW_BOE_HEADER_DESCRIPTION, _createBOEHeaderMVDescription(boe, rteTemplateAnswers));

			ViewBag.RteFieldSize = ws.RteSizeLimit ?? Constants.MAX_RTE_LENGTH;

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HEADER_DESCRIPTION, sw);
			return toReturn;
		}

		/// <summary>
		/// Returns The BOE Header (lower portion)
		/// </summary>
		/// <param name="workspace">THe Workspace Shortname</param>
		/// <param name="id">BOE ID</param>
		/// <returns>The view for the BOE Header</returns>
		[ChildActionOnly, HttpGet]
		public virtual ViewResult DisplayBOEHeader(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = ws.Boes.First(x => x.Id == boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HEADER, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HEADER, sw);
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
		[HttpGet, ChildActionOnly]
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

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_SEARCH, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

			ViewData["BOEID"] = boeID;
			ViewData["UsingTemplateBoe"] = ws.UsingTemplateBOE;

			// Only an Author in WS=Working and BOE=Draft should be able to Search to copy a BOE. 
			ViewData["BOESearch_ReadOnly"] = (Boolean.Parse(GetReadOnlyAttribute(CheckPermissions(SecurityPage.BoeSearch, ws, boeID)))).ToString().ToLower();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_SEARCH);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_SEARCH, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the project map boe search button.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns>View containing BOE Search button or nothing if no access.</returns>
		[HttpGet, ChildActionOnly]
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

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_PROJECTMAP_BOE_SEARCH, SecurityPage.WorkspaceHome, SecurityAuthorization.Read, ws, null);

			// Only an Author/Admin in WS=Working should be able to Search to copy a BOE.
			ViewData["BOESearch_ReadOnly"] = (Boolean.Parse(GetReadOnlyAttribute(CheckPermissions(SecurityPage.ProjectMapBoeSearch, ws, null)))).ToString().ToLower();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_PROJECTMAP_SEARCH);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_PROJECTMAP_BOE_SEARCH, sw);
			return toReturn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <returns></returns>
		[HttpGet, ChildActionOnly]
		public virtual ViewResult DisplayBOEAdvancedSearch(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_ADVANCED_SEARCH, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			ViewData["WSPERFORGS"] = _BoeLaborControllerLogic.GetPerformingOrgs(ws);
			BOEAdvancedSearchModelView theModelView = new BOEAdvancedSearchModelView();
			_ControllerLogic.PopulateCompanySpecificProperties(theModelView);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_ADVANCED_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_ADVANCED_SEARCH, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays the boe project map advanced search.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns></returns>
		[HttpGet, ChildActionOnly]
		public virtual ViewResult DisplayBOEProjectMapAdvancedSearch(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_PROJECTMAP_ADVANCED_SEARCH, SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.Read, ws, null);
			ViewData["WSPERFORGS"] = _BoeLaborControllerLogic.GetPerformingOrgs(ws);
			BOEAdvancedSearchModelView theModelView = new BOEAdvancedSearchModelView();
			_ControllerLogic.PopulateCompanySpecificProperties(theModelView);
			theModelView.IsProjectMapDiscrete = ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap;
			ViewResult toReturn = View(WebConstants.VIEW_BOE_PROJECTMAP_ADVANCED_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_PROJECTMAP_ADVANCED_SEARCH, sw);
			return toReturn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <returns></returns>
		[HttpGet, ChildActionOnly]
		public virtual ViewResult DisplayBOEQuickSearch(string workspace, int? boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			SecurityPage securityPage = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_QUICK_SEARCH, securityPage, SecurityAuthorization.Read, ws, boeID);

			BOEQuickSearchModelView theModelView = new BOEQuickSearchModelView();

			ViewResult toReturn = View(WebConstants.VIEW_BOE_QUICK_SEARCH, theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_QUICK_SEARCH, sw);
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
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_SUMMARY, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_SUMMARY, sw);
			return toReturn;
		}

		/// <summary>
		/// Displays containing Page for BOE Details including tabs
		/// </summary>
		/// <param name="workspace">Workspace short name</param>
		/// <param name="id">BOE ID</param>
		/// <returns></returns>
		[ChildActionOnly, HttpGet]
		public virtual ViewResult DisplayBOEDetails(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_DETAILS, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

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
			ViewData["EnableLmNavigator"] = Utilities.IsLmNavigatorRteLinkEnabledForSystem && ws.EnableLmNavigator;

			// display when you are not a sub, the boe is not summary or multi & ODC items exist
			ViewData["displayODCTab"] = !(IsSubContractor || boe.IsMultiClinWbs) && boe.OtherDirectCosts.Any();

			// display travel only when you are not a sub AND you have Travel data already
			ViewData["displayTravelTab"] = !IsSubContractor && boe.Travels.Any();

			ViewResult toReturn = GetMasterView(WebConstants.VIEW_BOE_DETAILS, workspace);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_DETAILS, sw);
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
		[ChildActionOnly, HttpGet]
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
		[HttpPost]
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
			theModelView.EnableTaskAuthor = Utilities.IsAssignTaskAuthorEnabledForSystem && ws.EnableAssignTaskAuthor;

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_GET_MANAGE_BOE_MODEL, sw);
			return this.Json(theModelView);
		}

		/// <summary>
		/// Display the Export BOE partial view
		/// </summary>
		/// <param name="workspace">The current workspace ID</param>
		/// <param name="boeID">The current BOE ID</param>
		/// <returns>ActionResult to display the Export BOE partial</returns>
		[ChildActionOnly, HttpGet]
		public ViewResult DisplayExportBOEButton(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_EXPORT, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_EXPORT, sw);
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
		[HttpGet]
		public async Task<ActionResult> ExportBOEToWordFile(string workspace, int boeId, string summarizeByCustomField)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_BOE, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeId);

			bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
										where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
										select p).Any();

			List<int> ids = new List<int>();
			ids.Add(boeId);

			ActionResult result = new EmptyResult();

			try
			{
				// UCOT validation (Space only) - if there are multiple MOQ types assigned to a task and one of those MOQ types falls under a specified type, throw an exception
				if (Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.TrackingNumber) && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
				{
					FullBoe fullBoe = this.Factory.CreateFullBoe(boeId);
					MultiMOQTypeResult multiMoqResult = MultiMOQTypeUtility.DoTasksHaveMultipleMOQTypes(fullBoe, ws.CreationDate, ws.Shortname);

					if (multiMoqResult.DoMultiMOQTypesExist)
					{
						string commaSeparatedTasks = string.Join(", ", multiMoqResult.Tasks);
						string ucotExceptionString = string.Format(ValidationConstants.MULTI_TASK_WITH_MULTI_MOQ_TYPES_UCOT, commaSeparatedTasks);
						throw new GenValidationException(ucotExceptionString);
					}
				}

				bool isCustomExport;
				WorkspaceExportFormatDTO wsExportFormatDTO;
				BOEExportInputs exportInputs;
				ICollection<BOEExportModelView> boeExportModelViews;
				List<BOESummaryGridModelView> boeSummaryGridModelViews;

				this.reportsControllerLogic.PrepareAllBOEsReport(ws, isSubcontractorUser, summarizeByCustomField, ids, ViewData, out isCustomExport, out wsExportFormatDTO,
					out exportInputs, out boeExportModelViews, out boeSummaryGridModelViews, false);
				await this.reportsControllerLogic.ExportAllBOEsReport(ws, null, Response, isCustomExport, wsExportFormatDTO, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
			}
			catch (GenValidationException ex)
			{
				_log.Warn(ex);

				result = this.CreateTextFileWithErrorMessage(ex.Message);
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

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EXPORT_BOE, sw);
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
		[HttpGet]
		public async Task<ActionResult> BOESearchPreview(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			//The BOE id sent in is not the BOE the user has permissions too its the BOE they are previewing which they are allowed to view if it is searchable.
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_BOE_SEARCH_PREVIEW, SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);

			await _ControllerLogic.ExportBOESearchPreview(ws, boeID, Response);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_BOE_SEARCH_PREVIEW, sw);
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
		[HttpGet]
		public ActionResult ProjectMapSearchPreview(string workspace, int projectMapId)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			//The ProjectMap id sent in is not the ProjectMap the user has permissions too its the ProjectMap they are previewing which they are allowed to view if it is searchable.
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PROJECTMAP_SEARCH_PREVIEW, SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);

			_ControllerLogic.ExportProjectMapSearchPreview(ws, projectMapId, Response);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_PROJECTMAP_SEARCH_PREVIEW, sw);
			return new EmptyResult();
		}

		/// <summary>
		/// Calls to the Business layer to Validate the BOE. Then passes results
		/// of the validation to the UI for rendering.
		/// </summary>
		/// <param name="workspace">The workspace ID</param>
		/// <param name="id">The ID of the BOE to validate</param>
		/// <returns>The results of the validate function</returns>
		[HttpPost]
		public JsonResult ValidateBOE(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_VALIDATE_BOE, SecurityPage.ValidateBOE, SecurityAuthorization.Read, ws, boeID);

			// Perform Action
			// Get the BOE's entire set of data in a DTO object

			JsonResult toReturn = null;

			// Call to the business layer to validate the BOE
			toReturn = Json(_validateBOE.ValidateBOE_OnValidateBtnClick(boe, ws));

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_VALIDATE_BOE, sw);
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
				sw = InitializeAction(_log, WebConstants.ACTION_SAVE_EDIT_BOE_HEADER, SecurityPage.EditBOEHeader, SecurityAuthorization.ReadUpdate, ws, boe.Id);
			}
			catch (AuthorizationException)
			{
				// we failed saving at the 'edit boe header' level .. let's see if the EditBOEHeaderDescription is allowed
				sw = InitializeAction(_log, WebConstants.ACTION_SAVE_EDIT_BOE_HEADER, SecurityPage.EditBOEHeaderDescription, SecurityAuthorization.ReadUpdate, ws, boe.Id);

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
			FinalizeAction(_log, WebConstants.ACTION_SAVE_EDIT_BOE_HEADER, sw);

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

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_BOE_SUBMIT_FOR_REVIEW, SecurityPage.SubmitForReview, SecurityAuthorization.ReadUpdate, ws, boeID);
			// when we submit for review we want to send an email to reviewers
			// and also write a message to the log

			_ControllerLogic.SubmitForReview(ws, boeID);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_BOE_SUBMIT_FOR_REVIEW, sw);
		}

		/// <summary>
		/// Deletes all task elements for a BOE
		/// </summary>
		/// <param name="workspace">Workspace Name</param>
		/// <param name="boeID">BOE ID</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DeleteAllBOETaskElements(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_ALL_BOE_TASK_ELEMENTS, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DELETE_ALL_BOE_TASK_ELEMENTS, sw);

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
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_TASK_ELEMENTS, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_DELETE_TASK_ELEMENTS, sw);
		}

		/// <summary>
		/// Resets boes with the ids below to draft
		/// </summary>
		/// <param name="workspace">Workspace name.</param>
		/// <param name="boes">List of BOEs to be saved.</param>
		/// <returns>JsonResult of True or GenValidationException</returns>
		[HttpPost]
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
		[HttpPost]
		public ActionResult SaveManageBOE(string workspace, Collection<ManageBOEModelView> boes)
		{
			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_SAVE_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			ManageBOEModelView singleEditMV = _ControllerLogic.SaveManageBOE(boes, this.ModelState, ws);

			// Perform Action
			JsonResult toReturn;

			try
			{

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
					toReturn = this.Json(singleEditMV);
				}

				// Finalize Action
				this.FinalizeAction(this._log, WebConstants.ACTION_SAVE_MANAGE_BOE, sw);


			} catch (GenValidationException ex)
			{
				_log.Error(ex);
				toReturn = Json(new { status = false, message = ex.Message });
			}

			return toReturn;

		}


		/// <summary>
		/// Determines whether or not the specified BOE contains references to other BOEs through Sum of BOEs
		/// Task or Workspace variables.
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <param name="boeID">The BOE</param>
		/// <returns>True if task elements under the BOE have references to Sum of BOEs variables, false otherwise.</returns>
		[HttpPost]
		public JsonResult BOEContainsSumOfBOEs(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_BOE_CONTAINS_SUM_OF_BOES, SecurityPage.SubmitForApproval, SecurityAuthorization.Read, ws, boeID);

			// Get a value to indicate whether or not this BOE contains task elements that reference
			// other BOEs through Sum of BOEs task or workspace variables
			JsonResult result = this.Json(new
			{
				Result = this._VariableCircularReferenceChecker.GetBOEIDsReferencedByBOEID(boe, null, null, null, ws).Any(),
				Type = "Variable"
			});

			this.FinalizeAction(this._log, WebConstants.ACTION_BOE_CONTAINS_SUM_OF_BOES, sw);
			return result;
		}

		/// <summary>
		/// Perform the action for submitting a BOE for approval
		/// </summary>
		/// <param name="workspace">the workspace name</param>
		/// <param name="boeID">the boeid</param>
		[HttpPost]
		public virtual JsonResult SubmitForApproval(string workspace, int boeID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			FullBoe boe = this.Factory.CreateFullBoe(boeID);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SUBMIT_FOR_APPROVAL, SecurityPage.SubmitForApproval, SecurityAuthorization.ReadUpdate, ws, boeID);

			JsonResult toReturn;
			// when we submit for approval we want to send an email to approvers
			// and also write a message to the log

			ValidationBOEModelView validatedBOE = _ControllerLogic.SubmitForApproval(ws, boe);

			// if validation fails, then BOEs in Locked-Draft should be moved to (unlocked) Draft
			if (!validatedBOE.isValid)
			{
				if (boe.State == BOEState.DraftLocked)
				{
					if (_boeStateMachine.PerformStateTransitionValidation(boe, ws, BOEState.DraftLocked, BOEState.Draft, out _))
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

			FinalizeAction(_log, WebConstants.ACTION_SUBMIT_FOR_APPROVAL, sw);
			return toReturn;
		}

		/// <summary>
		/// Performs a search for BOEs based on the params passed in
		/// </summary>
		/// <param name="workspace">Name of Workspace containing BOE</param>
		/// <param name="boeID">ID of BOE search is being performed from</param>
		/// <param name="advSearchParams">parameters to search for</param>
		/// <returns></returns>
		[HttpPost]
		public ViewResult AdvancedSearchForBOEs(string workspace, int boeID, BOEAdvancedSearchModelView advSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_ADVANCED_SEARCH_FOR_BOES, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, ws, boeID);
			ViewResult toReturn;

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
				FinalizeAction(_log, WebConstants.ACTION_ADVANCED_SEARCH_FOR_BOES, sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, WebConstants.ACTION_ADVANCED_SEARCH_FOR_BOES, sw);
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
		[HttpPost]
		public ViewResult ProjectMapAdvancedSearchForBOEs(string workspace, BOEProjectMapAdvancedSearchModelView advSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PROJECTMAP_ADVANCED_SEARCH_FOR_BOES, SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.Read, ws, null);
			ViewResult toReturn;

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
				FinalizeAction(_log, WebConstants.ACTION_PROJECTMAP_ADVANCED_SEARCH_FOR_BOES, sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, WebConstants.ACTION_PROJECTMAP_ADVANCED_SEARCH_FOR_BOES, sw);
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
		[HttpPost]
		public ViewResult QuickSearchForBOEs(string workspace, int? boeID, BOEQuickSearchModelView quickSearchParams)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			SecurityPage security = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_QUICK_SEARCH_FOR_BOES, security, SecurityAuthorization.Read, ws, boeID);

			if (quickSearchParams == null)
			{
				throw new ArgumentNullException(nameof(quickSearchParams));
			}

			ViewData["BOEID"] = boeID;

			ViewResult toReturn;

			if (ModelState.IsValid)
			{
				SearchResultsModelView modelView = _ControllerLogic.QuickSearchForBOEs(ws, boeID, quickSearchParams);

				// Return the partial view
				string view = ws.IsProjectMapWorkspace ? WebConstants.VIEW_BOE_PROJECTMAP_SEARCH_RESULTS : WebConstants.VIEW_BOE_SEARCH_RESULTS;
				toReturn = View(view, modelView);

				// Finalize Action
				FinalizeAction(_log, WebConstants.ACTION_QUICK_SEARCH_FOR_BOES, sw);
			}
			else
			{
				// Finalize Action
				FinalizeAction(_log, WebConstants.ACTION_QUICK_SEARCH_FOR_BOES, sw);
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
		[HttpPost]
		public ViewResult PageSearchResults(string workspace, int? boeID, SearchResultsModelView searchResults)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			SecurityPage security = ws.IsProjectMapWorkspace ? SecurityPage.ProjectMapBoeSearch : SecurityPage.EditBOEHeader;
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_PAGE_SEARCH_RESULTS, security, SecurityAuthorization.Read, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_PAGE_SEARCH_RESULTS, sw);
			return toReturn;
		}

		/// <summary>
		/// Perform actions to export BOEs from Manage BOEs page
		/// </summary>
		/// <param name="workspace">Workspace containing BOEs</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
		public async Task<ActionResult> ExportManageBOE(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			ActionResult result;
			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);

			try
			{
				// Get BOEs template file name
				string templateFileName = Server.MapPath("~/Templates/Export/BOEs.xlsm");

				string[] fileNames = await _ControllerLogic.ExportManageBOE(ws, templateFileName, false);

				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				// Finalize Action
				FinalizeAction(_log, WebConstants.ACTION_EXPORT_MANAGE_BOE, sw);

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
		[HttpGet]
		public async Task<ActionResult> ExportManageBOETemplate(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_MANAGE_BOE_TEMPLATE, SecurityPage.ManageBOEs, SecurityAuthorization.Read, ws, null);
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
				FinalizeAction(_log, WebConstants.ACTION_EXPORT_MANAGE_BOE_TEMPLATE, sw);

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
		[HttpPost]
		public ViewResult ImportManageBOE(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_IMPORT_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

			Collection<ImportBoeResultsModelView> theModelViews = this._ControllerLogic.ImportManageBOE(ws, this.Request, out ICollection<ImportBoeResultsModelView> dataToSave, out bool errorsOccurred, out Exception exception);

			if (errorsOccurred)
			{
				this._log.Error(exception);
				this.ViewData["ERRORS_OCCURRED"] = true;
			}

			this.ViewData["SERIALIZED_DATA"] = serializer.Serialize(dataToSave);
			this.ViewData["DOCUMENT_DOMAIN"] = this.Request.Form["documentDomain"];

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
		[HttpPost]
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
		/// <returns></returns>
		[HttpPost]
		public ViewResult DisplayCopyBOEConflicts(string workspace, int boeID, int copyBOEID, ICollection<int> taskElementsToCopy)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			if (taskElementsToCopy == null)
			{
				taskElementsToCopy = new Collection<int>();
			}

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_BOE_COPY_CONFLICTS, SecurityPage.BoeCopyConflicts, SecurityAuthorization.Read, ws, null);
			BOECopyConflictsModelView boeCopyConflictsModelView = _ControllerLogic.DisplayCopyBOEConflicts(ws, boeID, copyBOEID, taskElementsToCopy);
			ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

			ViewResult toReturn = View(WebConstants.VIEW_BOE_COPY_CONFLICTS, boeCopyConflictsModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_BOE_COPY_CONFLICTS, sw);
			return toReturn;
		}

		/// <summary>
		/// Performs actions to copy BOE
		/// </summary>
		/// <param name="workspace">Workspace containing BOE to be copied to</param>
		/// <param name="boeID">ID of BOE to be copied to</param>
		/// <param name="copyBOEID">ID of BOE to be copied</param>
		/// <param name="taskElementsToCopy">Task elements of BOE to be copied</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult SaveCopyOfBOE(string workspace, int? boeID, int copyBOEID, Collection<int> taskElementsToCopy)
		{
			// Perform Action
			JsonResult toReturn = Json(new { Status = true });
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			if (taskElementsToCopy == null)
			{
				taskElementsToCopy = new Collection<int>();
			}

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_COPY_OF_BOE, SecurityPage.TaskElements, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

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
			FinalizeAction(_log, WebConstants.ACTION_SAVE_COPY_OF_BOE, sw);
			return toReturn;
		}

		/// <summary>
		/// Performs actions to copy Project Map
		/// </summary>
		/// <param name="workspace">Workspace to be copied to</param>
		/// <param name="copyProjectMapId">Id of Project Map to be copied</param>
		/// <returns>Json status for saving a copy of a Project Map.</returns>
		[HttpPost]
		public ActionResult SaveCopyOfProjectMap(string workspace, int copyProjectMapId)
		{
			// Perform Action
			JsonResult toReturn = Json(new { Status = true });
			FullProjectMapWorkspace ws = this.Factory.CreateFullProjectMapWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_COPY_OF_PROJECTMAP, SecurityPage.ProjectMapBoeSearch, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

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
			FinalizeAction(_log, WebConstants.ACTION_SAVE_COPY_OF_PROJECTMAP, sw);
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
		[HttpPost]
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
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_REORDER_LABOR_TASK_ELEMENTS, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			_ControllerLogic.ReOrderTaskElementOrder(ws, boeObject, theModelView);

			JsonResult toReturn = Json(new { Status = true });


			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_SAVE_REORDER_LABOR_TASK_ELEMENTS, sw);
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
		[HttpPost]
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
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_DUPLICATE_TASK_ELEMENTS, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

			try
			{
				Dictionary<int, int> duplicateRequest = theModelView.DuplicateTaskRequests.ToDictionary(x => x.TaskID, y => y.DuplicateCount);

				switch (theModelView.TaskType)
				{
					case TaskType.Labor:
						{
							boeObject.LoadTaskElementRTEData();

							using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
							{
								_BOECopier.DuplicateTasksInABoe(duplicateRequest, boeObject, ws);
								scope.Complete();
							}

							break;
						}
					case TaskType.Travel:
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
			FinalizeAction(_log, WebConstants.ACTION_SAVE_DUPLICATE_TASK_ELEMENTS, sw);
			return toReturn;
		}

		/// <summary>
		/// Save Bulk Boe Roles
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeRolesToSave">Boe Roles to Save</param>
		[HttpPost]
		public JsonResult SaveBoeBulkRoles(string workspace, ICollection<ManageBOEModelView> boeRolesToSave)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_BULK_ROLE_ASSIGN, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			IList<string> errorMessages = this._ControllerLogic.SaveBoeBulkRoles(ws, boeRolesToSave);

			JsonResult response = Json(new { Status = true, ErrorMessages = errorMessages });

			FinalizeAction(_log, WebConstants.ACTION_SAVE_BULK_ROLE_ASSIGN, sw);

			return response;
		}

	}
}
