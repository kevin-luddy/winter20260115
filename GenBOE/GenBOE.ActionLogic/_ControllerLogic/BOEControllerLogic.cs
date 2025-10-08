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
	using System.Transactions;
	using System.Web;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.CopyBOE;
	using GenBOE.ActionLogic.IESSAPClient;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Misc;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.ActionLogic.NewValidation;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using Microsoft.Practices.ObjectBuilder2;

	public class BOEControllerLogic : IBOEControllerLogic, IDisposable
	{
        private readonly IBOESummary _BOESummary;
        private readonly IUserDTODataLoader UserLoader;
        private readonly IActiveDirectoryUtilities _ADUtils;
        private readonly IPermissionsDTODataLoader PermissionsLoader;
        private readonly IFullObjectFactory Factory;
        private readonly IBOEExporter _BOEExporter;
        private readonly IBOECustomExporter _boeCustomExporter;
        private readonly IGenBOEControllerLogic _genBOEControllerLogic;
        private readonly IBoeMediator _BoeMediator;
        private readonly IValidationHelper _validationHelper;
        private readonly IBOECommentDTODataLoader _boeCommentLoader;
        private readonly IBoeEmailer _emailer;
        private readonly IBoeTaskElementMediator _BoeTaskElementMediator;
        private readonly IWorkspaceVariableDTODataLoader _workspaceVariableLoader;
        private readonly IBOEStateMachine _boeStateMachine;
        private readonly IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation;
        private readonly IBOELaborControllerLogic BoeLaborControllerLogic;
        private readonly IValidateBOE _validateBOE;
        private readonly ISecurityInformation _SecurityInformation;
        private readonly IBOESearchDTODataLoader _boeSearchLoader;
        private readonly ISecurityAccess _SecurityAccess;
        private readonly IBoeTaskElementRecalculation _BoeTaskElementRecalculation;
        private readonly IBOEImporter _BOEImporter;
        private readonly IVariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private readonly IConflictBOE _ConflictBOE;
        private readonly INestedWBSUtilities _nestedWbsUtilities;
        private readonly IProjectMapDataLoader projectMapLoader;
        private readonly RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private readonly IRteTemplateDataLoader rteTemplateDataLoader;
        private readonly IMoqTypeDataLoader moqTypeDataLoader;
        private readonly IBoeApproverResponseDTODataLoader boeApproverResponseLoader;
        private readonly IESSAPClient iesSapClient;
        private readonly ITokenService tokenService;
        private readonly Logger logger = new Logger(typeof(BOEControllerLogic));
		private readonly BOEReportsHttpService boeReportsHttpService = new BOEReportsHttpService();
		private readonly ITravelDTODataLoader _TravelDTOLoader;
		private readonly IMaterialDTODataLoader _MaterialLoader;
		private readonly IWbsDTODataLoader wbsLoader;
		private readonly ICommonDataMapper _CommonDataMapper;

		/// <summary>
		/// Memory Cache
		/// </summary>
		private readonly MemoryCache memCache;
		private bool disposedValue;

		/// <summary>
		/// Cache key for Get All Operators
		/// </summary>
		private const string CACHE_GET_ALL_OPERATORS = "CACHE_GET_ALL_OPERATORS";

		/// <summary>
		/// Cache key for fields for company
		/// </summary>
		private const string CACHE_GET_FIELDS = "CACHE_GET_FIELDS_";

		/// <summary>
		/// Cache duration - 6 hours
		/// </summary>
		private const int CACHE_DURATION = 6 * 60 * 60;

		/// <summary>
		/// The version loader.
		/// </summary>
		private IWorkspaceVersionMetaDataDTODataLoader versionLoader { get; set; }

		#region Protected Properties and Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public BOEControllerLogic(
            IBOESummary inBOESummary,
            IUserDTODataLoader inUserLoader,
            IActiveDirectoryUtilities inActiveDirectoryUtil,
            IPermissionsDTODataLoader inPermissionsLoader,
            IFullObjectFactory inFactory,
            IBOEExporter inBOEExporter,
            IBOECustomExporter inBoeCustomExporter,
            IGenBOEControllerLogic inGenBOEControllerLogic,
            IBoeMediator inBoeMediator,
            IValidationHelper inValidationHelper,
            IBOECommentDTODataLoader inBoeCommentLoader,
            IBoeEmailer inEmailer,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
            IBOEStateMachine inBOEStateMachine,
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            IBOELaborControllerLogic inBOELaborControllerLogic,
            IValidateBOE inValidateBOE,
            ISecurityInformation inSecurityInformation,
            IBOESearchDTODataLoader inBoeSearchLoader,
            ISecurityAccess inSecurityAccess,
            IBoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IBOEImporter inBOEImporter,
            IVariableCircularReferenceChecker inVariableCircularReferenceChecker,
            IConflictBOE inConflictBOE,
            INestedWBSUtilities inNestedWBSUtilities,
            IProjectMapDataLoader projectMapLoader,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader,
            IRteTemplateDataLoader rteTemplateDataLoader,
            IMoqTypeDataLoader moqTypeDataLoader,
            IBoeApproverResponseDTODataLoader boeApproverResponseLoader,
            IESSAPClient iesSapClient,
            ITokenService tokenService,
			ICommonDataMapper inCommonDataMapper,
			IMaterialDTODataLoader inMaterialLoader,
			ITravelDTODataLoader travelLoader,
			IWorkspaceVersionMetaDataDTODataLoader versionLoader,
			IWbsDTODataLoader wbsLoader)
        {
            this._BOESummary = inBOESummary;
            this.UserLoader = inUserLoader;
            this._ADUtils = inActiveDirectoryUtil;
            this.PermissionsLoader = inPermissionsLoader;
            this.Factory = inFactory;
            this._BOEExporter = inBOEExporter;
            this._boeCustomExporter = inBoeCustomExporter;
            this._genBOEControllerLogic = inGenBOEControllerLogic;
            this._BoeMediator = inBoeMediator;
            this._validationHelper = inValidationHelper;
            this._boeCommentLoader = inBoeCommentLoader;
            this._emailer = inEmailer;
            this._BoeTaskElementMediator = inBoeTaskElementMediator;
            this._workspaceVariableLoader = inWorkspaceVariableLoader;
            this._boeStateMachine = inBOEStateMachine;
            this._variableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
            this.BoeLaborControllerLogic = inBOELaborControllerLogic;
            this._validateBOE = inValidateBOE;
            this._SecurityInformation = inSecurityInformation;
            this._boeSearchLoader = inBoeSearchLoader;
            this._SecurityAccess = inSecurityAccess;
            this._BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
            this._BOEImporter = inBOEImporter;
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
            this._ConflictBOE = inConflictBOE;
            this._nestedWbsUtilities = inNestedWBSUtilities;
            this.projectMapLoader = projectMapLoader;
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
            this.moqTypeDataLoader = moqTypeDataLoader;
            this.boeApproverResponseLoader = boeApproverResponseLoader;
			this.iesSapClient = iesSapClient;
            this.tokenService = tokenService;
			this._CommonDataMapper = inCommonDataMapper;
			this._MaterialLoader = inMaterialLoader;
			this._TravelDTOLoader = travelLoader;
			this.versionLoader = versionLoader;
			this.wbsLoader = wbsLoader;
			memCache = new MemoryCache();
		}

		#endregion

		#region Get Actions

		/// <summary>
		/// Get BOE Header Model View
		/// </summary>
		/// <param name="boe">The <see cref="BoeDTO"/> used to populate the <see cref="BOEHeaderISGSModelView"/></param>
		/// <returns>the populated <see cref="BOEHeaderISGSModelView"/></returns>
		public virtual IBOEHeaderModelView GetCreateBOEHeaderMV(BoeDTO boe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers)
        {
            return new BOEHeaderISGSModelView(boe, answers);
        }

		/// <summary>
		/// Get BOE Header Description Model View
		/// </summary>
		/// <param name="boe">BOE containing BOE Summary</param>
		/// <param name="ws">Workspace containing the BOE</param>
		/// <returns>ModelView for the description in BOE Header</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public BOEHeaderDescriptionModelView GetBOEHeaderDescriptionMv(FullBoe boe, FullWorkspace ws)
		{
			if (boe == null)
			{
				throw new ArgumentNullException(nameof(boe));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id).Where(t => t.SourceId == (int)RteTemplateSource.BoeDescription).OrderBy(r => r.SortOrder).ToList();
			BOEHeaderDescriptionModelView description = new BOEHeaderDescriptionModelView(boe, rteTemplateAnswers);
			return description;
		}

		#endregion Get Actions

		#region Display

		#region Views

		#endregion

		#region Partial Views

		#region Edit BOE

		/// <summary>
		/// Gets the model view for the BOE task element grid
		/// </summary>
		/// <param name="boe">BOE containing the task element grid</param>
		/// <param name="ws">Workspace the BOE exists in</param>
		/// <returns>model view for the BOE task element grid</returns>
		public GenericTaskElementGridModelView GetTaskGridModelView(FullBoe boe, FullWorkspace ws)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            int workspaceID = ws.Id;
            bool IsSubContractor = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspaceID)
                                    where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                    select p).Any();

            GenericTaskElementGridModelView theModelView = new GenericTaskElementGridModelView();
			theModelView.IsUCOTEnabledForWorkspace = Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.Shortname);
            theModelView.DeleteAction = WebConstants.ACTION_DELETE_TASK_ELEMENTS;
            theModelView.DisplayEvent = WebConstants.EVENT_DISPLAY_TASK_ELEMENT_DETAILS;

			// pulling from workspace instead of by BOE since we are already loading the full workspace task elements elsewhere to show the page, and for validation
			boe.SetTaskElements(ws.TaskElements);
			UCOTUtility.SetTaskElementsUCOTHours(ws, boe.TaskElements);
			IReadOnlyCollection<BoeTaskElementDTO> taskElementCollection = boe.TaskElements;

			if (IsSubContractor)
            {
                theModelView.TaskElements = new Collection<GenericTaskElementGridRow>((
                               from t in taskElementCollection
                               orderby t.TaskElementType
                               select new GenericTaskElementGridRow
                               {
                                   TaskElementDetailID = t.Id,
                                   TaskID = t.BOETaskID,
                                   Title = t.TaskTitle,
                                   StartDate = t.StartDate,
                                   EndDate = t.EndDate,
                                   TaskType = t.TaskElementType,
                                   TotalHours = t.TotalHours,
                                   UpdateDate = t.UpdateDate,
                                   TotalCost = t.TotalCost,
                                   BOETaskElementOrder = t.BOETaskElementOrder,
								   TotalUCOTHours = t.UCOTHours,
								   TotalHoursWithUCOT = t.TotalHoursWithUCOT
                               }).ToArray());
            }
            else
            {
                theModelView.TaskElements = new Collection<GenericTaskElementGridRow>((
                from t in taskElementCollection
                orderby t.TaskElementType
                select new GenericTaskElementGridRow
                {
                    TaskElementDetailID = t.Id,
                    TaskID = t.BOETaskID,
                    Title = t.TaskTitle,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    TaskType = t.TaskElementType,
                    TotalHours = t.TotalHours,
                    UpdateDate = t.UpdateDate,
                    TotalCost = t.TotalCost,
                    BOETaskElementOrder = t.BOETaskElementOrder,
					TotalUCOTHours = t.UCOTHours,
					TotalHoursWithUCOT = t.TotalHoursWithUCOT
				}).ToArray());

            }

            foreach (GenericTaskElementGridRow row in theModelView.TaskElements)
            {
                if (row.TaskElementDetailID.HasValue)
                {
                    BoeTaskElementDTO taskElement = boe.TaskElements.First(x => x.Id == row.TaskElementDetailID.Value);
                    row.ResourceDecimalPrecision = ws.DecimalPrecision;
                    if (IsSubContractor && row.TaskType == TaskElementType.Labor)
                    {
                        // TotalCost is already available (and includes discrete costs ONLY)
                    }
                    else
                    {
                        // Sum up all the Cost type resources.
                        decimal taskElementCostTotal = taskElement.taskElementLabors.Where(l => l.SpreadType == SpreadType.Cost).Sum(l => l.ValueSpread ?? 0);

                        row.TotalCost = taskElementCostTotal;
                    }                 
                }
            }

            theModelView.TaskElements = theModelView.TaskElements.OrderBy(teOrder => teOrder.BOETaskElementOrder).ToCollection();

            return theModelView;
        }

        /// <summary>
        /// Gets the model view for the Duplicate Task Elements Dialog
        /// </summary>
        /// <param name="boe">BOE containing the tasks to be duplicated</param>
        /// <param name="taskType">Type of task to be duplicated</param>
        /// <returns>Model view for the duplicate tasks dialog</returns>
        public TaskElementDuplicateFormCollection GetDuplicateTaskModelView(FullBoe boe, TaskType taskType)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            TaskElementDuplicateFormCollection toReturn = new TaskElementDuplicateFormCollection();

            toReturn.Boe = boe;
            toReturn.TaskType = taskType;
            toReturn.DuplicateTaskRequests = new Collection<TaskElementDuplicateFormModelView>();

            //Get all task elements for a specific task type
            switch (taskType)
            {
                case TaskType.Labor:
                    {
                        foreach (BoeTaskElementDTO taskElementDTO in boe.TaskElements)
                        {
                            TaskElementDuplicateFormModelView mv = new TaskElementDuplicateFormModelView
                            {
                                TaskID = taskElementDTO.Id,
                                DuplicateCount = 0,
                                BOETaskID = taskElementDTO.BOETaskID,
                                TaskTitle = taskElementDTO.TaskTitle
                            };

                            toReturn.DuplicateTaskRequests.Add(mv);
                        }
                        break;
                    }
                case TaskType.ODC:
                    {
                        foreach (OtherDirectCostDTO otherDirectCostDTO in boe.OtherDirectCosts)
                        {
                            TaskElementDuplicateFormModelView mv = new TaskElementDuplicateFormModelView
                            {
                                TaskID = otherDirectCostDTO.Id,
                                DuplicateCount = 0,
                                BOETaskID = otherDirectCostDTO.TaskID,
                                TaskTitle = otherDirectCostDTO.TaskTitle
                            };

                            toReturn.DuplicateTaskRequests.Add(mv);

                        }
                        break;
                    }
                case TaskType.Travel:
                    {
                        foreach (TravelDTO travelDTO in boe.Travels)
                        {
                            TaskElementDuplicateFormModelView mv = new TaskElementDuplicateFormModelView
                            {
                                TaskID = travelDTO.Id,
                                DuplicateCount = 0,
                                BOETaskID = travelDTO.TaskID,
                                TaskTitle = travelDTO.TaskTitle
                            };

                            toReturn.DuplicateTaskRequests.Add(mv);
                        }
                        break;
                    }

            }
            return toReturn;
        }

        /// <summary>
        /// Creates the model view for the BOE Header
        /// </summary>
        /// <param name="boe">BOE containing the header to create</param>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <returns>model view for the BOE Header</returns>
        public IBOEHeaderModelView CreateBOEHeaderMV(FullBoe boe, WorkspaceDTO ws)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            
            ICollection<RTECustomTemplateQuestionAnswerModelView> answers = this.rteTemplateDataLoader.GetByBoeId(ws.Id, boe.Id);
            // Perform Action
            IBOEHeaderModelView theModelView = this.GetCreateBOEHeaderMV(boe, answers);

            theModelView.WBS = boe.Wbs != null ? boe.Wbs.WbsString : CommonConstants.Unassigned_WBS_Display_Text;
			theModelView.State = boe.State;
            
            // get the clin
            ClinDTO clin = boe.Clin;
            theModelView.CLIN = clin != null ? clin.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;

            if (boe.StartDate.ToString("MM/yyyy") == DateTime.MinValue.ToString("MM/yyyy"))
            {
                if (clin?.StartDate != null)
                {
                    theModelView.StartDate = clin.StartDate;
                }
                else
                {
                    theModelView.StartDate = ws.ContractStartDate;
                }
            }

            if (boe.EndDate.ToString("MM/yyyy") == DateTime.MinValue.ToString("MM/yyyy"))
            {
                if (clin?.EndDate != null)
                {
                    theModelView.EndDate = clin.EndDate;
                }
                else
                {
                    theModelView.EndDate = ws.ContractEndDate;
                }
            }

            //Load the customFields
            Collection<CustomFieldSelectionModelView> selectedOptionsMV = new Collection<CustomFieldSelectionModelView>();
            ICollection<CustomFieldValueContainer> selectedOptions = boe.CustomFieldValueContainers;
            if (selectedOptions != null)
            {
                foreach (CustomFieldValueContainer xrefSelection in selectedOptions)
                {
                    selectedOptionsMV.Add(new CustomFieldSelectionModelView() { CustomFieldValueID = xrefSelection.CustomFieldValueID, SelectionID = xrefSelection.ContainerID, UpdateDate = xrefSelection.UpdateDate, OpenEndedValue = xrefSelection.OpenEndedValue, CustomFieldID = xrefSelection.CustomFieldID, IsOpenEnded = xrefSelection.IsOpenEnded });
                }

                theModelView.CustomFieldValues = selectedOptionsMV;
            }

            ICollection<int> ids = new Collection<int>();
            ids.Add(boe.Id);

            return theModelView;
        }

		/// <summary>
		/// Gets model views for custom fields in the BOE Header
		/// </summary>
		/// <param name="ws">workspace containing the BOE/Custom fields</param>
		/// <returns>model views for custom fields in the BOE Header</returns>
		public Collection<BOECustomFieldModelView> GetCustomFieldModelViews(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            Collection<BOECustomFieldModelView> customFieldModelViews = new Collection<BOECustomFieldModelView>();
            if (ws.CustomFields.Any())
            {
                IReadOnlyCollection<CustomFieldValueDTO> allCustomFieldValues = ws.CustomFieldValues;
                foreach (CustomFieldDTO customField in ws.CustomFields)
                {
                    BOECustomFieldsInUseGridModelView metadata = new BOECustomFieldsInUseGridModelView(customField);
                    Collection<CustomFieldValueDTO> options = allCustomFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection();

                    if (metadata.CustomFieldDisplayID == CustomFieldType.BoeDisplay)
                    {
                        Collection<BOECustomFieldOptionModelView> optionstoAdd = new Collection<BOECustomFieldOptionModelView>();

                        foreach (CustomFieldValueDTO option in options)
                        {
                            optionstoAdd.Add(new BOECustomFieldOptionModelView(option));
                        }

                        customFieldModelViews.Add(new BOECustomFieldModelView()
                        {
                            CustomFieldMetaData = metadata,
                            CustomFieldOptions = optionstoAdd
                        });
                    }
                }
            }
            return customFieldModelViews;
        }

        /// <summary>
        /// Gets the model view for the BOE Summary Grid
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE containing BOE Summary</param>
        /// <param name="isSubcontractorUser">Bool designating if user is a subcontractor</param>
        /// <returns>model view for the BOE Summary Grid</returns>
        public ICollection<BOESummaryGridModelView> GetBOESummaryGridModelViews(FullWorkspace ws, FullBoe boe, bool isSubcontractorUser)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            BOEExportInputs exportInputs = new BOEExportInputs(boe, ws);

            ICollection <BOESummaryGridModelView> results = this._BOESummary.GetBOESummaryGridModelViews(boe, exportInputs, isSubcontractorUser).OrderBy(x => x.Category).ToArray();

            return results;
        }

        #endregion

        #region Manage BOE

        /// <summary>
        /// Gets the model view BOE properties for the Manage BOE Grid.
        /// </summary>
        /// <param name="ws">Workspace containing the BOEs</param>
        /// <param name="boes">The Boes to get data for</param>
        public ICollection<ManageBOEModelView> GetManageBOEGridData(FullWorkspace ws, IReadOnlyCollection<FullBoe> boes)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            ICollection<ManageBOEModelView> modelViews = new List<ManageBOEModelView>();

            #region Get the data

            // Get roles for the boes
            List<int> boeIds = boes.Select(x => x.Id).ToList();

            HashSet<PermissionsDTO> rolesWithBoes = new HashSet<PermissionsDTO>(this.PermissionsLoader.GetBOEPermissions(boeIds).Where(x => x.BOEId.HasValue).ToCollection());

            // Get users broken down by roles, for the boes
            HashSet<UserDTO> allBoeUsers = new HashSet<UserDTO>(this.UserLoader.GetByIds(rolesWithBoes.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

            Dictionary<int, Collection<UserDTO>> boeAuthors = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.Author);
            Dictionary<int, Collection<UserDTO>> boeSubcontractorAuthors = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.SubcontractorAuthor);
            Dictionary<int, Collection<UserDTO>> boeApprovers = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.Approver);

            // Get potential WS users
            Collection<PermissionsDTO> wsPotentialPermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);

            Collection<UserDTO> potentialWsUsers = this.UserLoader.GetByIds(
                wsPotentialPermissions.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor || x.Role == Role.Approver).Select(x => x.ETIUserId).Distinct().ToList()
                ).ToCollection();

            // add group users..
            List<UserDTO> tempUsers = new List<UserDTO>();
            foreach (UserDTO user in potentialWsUsers)
            {
                if (user.NTID.Contains('.')) // AD group name
                {
                    ICollection<UserData> members = this._ADUtils.GetAdGroupUsers(user.DisplayName);

                    ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
                    int userId;
                    List<int> userIds = new List<int>();

                    foreach (UserData member in orderedMembers)
                    {
                        bool userExists = this.UserLoader.UserExists(member.Ntid, out userId);

                        if (userExists)
                        {
                            if (potentialWsUsers.All(x => x.UserID != userId))
                            {
                                userIds.Add(userId);
                            }
                        }
                    }

                    tempUsers.AddRange(this.UserLoader.GetByIds(userIds));
                }
            }

            tempUsers.AddRange(potentialWsUsers.ToList());
            potentialWsUsers = new Collection<UserDTO>(tempUsers);

            #endregion

            // Only get escalation rates and fees once since they exist at the Workspace level.
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);
            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(ws.Id).ToDictionary(f => f.ModeID);

            foreach (FullBoe boe in boes)
            {
                WbsDTO wbsDto = ws.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                ClinDTO clinDto = ws.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

                //loop through BOE Authors and Subcontractor Authors to build these lists
                Collection<UserDTO> authorsAsUser;
                if (!boeAuthors.TryGetValue(boe.Id, out authorsAsUser))
                {
                    authorsAsUser = new Collection<UserDTO>();
                }

                Collection<UserDTO> subcontractorAuthorsAsUser;
                if (!boeSubcontractorAuthors.TryGetValue(boe.Id, out subcontractorAuthorsAsUser))
                {
                    subcontractorAuthorsAsUser = new Collection<UserDTO>();
                }

                Collection<UserDTO> approversAsUser;
                if (!boeApprovers.TryGetValue(boe.Id, out approversAsUser))
                {
                    approversAsUser = new Collection<UserDTO>();
                }

                Collection<UserDTO> users = potentialWsUsers;

                PermissionsDTO[] authors = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.Author).Select(x => x).ToArray();
                PermissionsDTO[] subcontractorAuthors = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray();
                PermissionsDTO[] approvers = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
                List<TravelDTO> travels = ws.Travels.Where(x => x.BoeID == boe.Id).ToList();

                
                    decimal totalCostTravel = this.CalculateTotalCostTravel(travels, ws.DecimalPrecision, escalationRates, fees);

                    ManageBOEModelView manageBoe = new ManageBOEModelView(boe, wbsDto, clinDto, users, authors.ToCollection(),
                        subcontractorAuthors.ToCollection(), approvers.ToCollection(), ws.TaskElements.Where(t => t.BoeID == boe.Id).ToCollection(), totalCostTravel);

                modelViews.Add(manageBoe);
            }

            return modelViews;
        }

        /// <summary>
        /// Gets Users based on role in BOE
        /// </summary>
        /// <param name="boeRolesWithBOEID">Roles in the BOE</param>
        /// <param name="allUsers">Users with any role in the BOE</param>
        /// <param name="role">Role to get users by</param>
        /// <returns>Dictionary of uers mapped to the BOE ID</returns>
        private Dictionary<int, Collection<UserDTO>> GetBoeUsersByRole(HashSet<PermissionsDTO> boeRolesWithBOEID, HashSet<UserDTO> allUsers, Role role)
        {
            ICollection<int> userIdsForRole = boeRolesWithBOEID.Where(x => x.Role == role).Select(x => x.ETIUserId).Distinct().ToList();
            ICollection<UserDTO> usersForRole = allUsers.Where(x => userIdsForRole.Contains(x.UserID)).ToList();

            Dictionary<int, Collection<UserDTO>> usersMappedToBoeId = new Dictionary<int, Collection<UserDTO>>();

            foreach (int boeId in boeRolesWithBOEID.Select(x => x.BOEId).Distinct())
            {
                IEnumerable<int> matchingUserIds = boeRolesWithBOEID.Where(x => x.BOEId == boeId).Select(x => x.ETIUserId).Distinct();
                Collection<UserDTO> matchingUsers = usersForRole.Where(x => matchingUserIds.Contains(x.UserID)).ToCollection();

                usersMappedToBoeId.Add(boeId, matchingUsers);
            }

            return usersMappedToBoeId;
        }

        /// <summary>
        /// Updates the BOE Start/End dates with the default dates. Either sets them to Workspace
        /// PoP dates or the CLIN dates when applicable.
        /// </summary>
        /// <param name="ws">Full Workspace</param>
        /// <param name="boe">BOE to update</param>
        /// <param name="clinId">New clin of BOE</param>
        public void setDefaultBoeDates(FullWorkspace ws, FullBoe boe, int? clinId)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            boe.StartDate = ws.ContractStartDate;
            boe.EndDate = ws.ContractEndDate;

            // BOEJ-921 - Only override Start/End Dates if we have a valid CLINID.
            if (clinId != null)
            {
                ClinDTO clin = (from c in ws.Clins
                                where c.Id == clinId
                                select c).FirstOrDefault();

                if (clin != null && clin.StartDate.HasValue && clin.EndDate.HasValue)
                {
                    boe.StartDate = clin.StartDate.Value;
                    boe.EndDate = clin.EndDate.Value;
                }
            }
        }

        /// <summary>
        /// Creates the select list for authors/approvers when creating/managing a BOE
        /// </summary>
        /// <param name="inUserIds">UserIDs for the select list</param>
        /// <param name="isSubcontractors">True if the list is for subcontractors</param>
        /// <returns>select list for authors/approvers</returns>
        private Collection<SelectListItem> CreateUserSelectList(Collection<int> inUserIds, bool isSubcontractors)
        {
            if (inUserIds == null)
            {
                throw new ArgumentNullException(nameof(inUserIds));
            }

            Collection<SelectListItem> returnList = new Collection<SelectListItem>();

            ICollection<UserDTO> allUsers = this.UserLoader.GetByIds(inUserIds);

            foreach (int user in inUserIds)
            {
                UserDTO selectedUser = allUsers.First(x => x.UserID == user);
                string userDisplayName = selectedUser.DisplayName;

                if (selectedUser.NTID.Contains('.')) // AD group name
                {
                    ICollection<UserData> members = this._ADUtils.GetAdGroupUsers(userDisplayName);

                    List<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
                    foreach (UserData member in orderedMembers)
                    {
                        // Load the user's information.  If the user does not currently exist in the database, create it and use its new ID.
                        UserDTO userInfo = this.UserLoader.GetOrCreateUserByNtid(member.Ntid);
                        returnList.Add(new SelectListItem
                        {
                            Value = userInfo.UserID.ToString(),
                            Text = userInfo.DisplayName
                        });
                    }
                }
                else
                {
                    if (isSubcontractors)
                    {
                        returnList.Add(new SelectListItem
                        {
                            Text = selectedUser.DisplayName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX,
                            Value = selectedUser.UserID.ToString()
                        });
                    }
                    else
                    {
                        returnList.Add(new SelectListItem
                        {
                            Text = selectedUser.DisplayName,
                            Value = selectedUser.UserID.ToString()
                        }); 
                    }
                }

                returnList = (from a in returnList
                              group a by new { Selected = a.Selected, Value = a.Value, Text = a.Text } into g
                              select new SelectListItem
                              {
                                  Value = g.Key.Value,
                                  Text = g.Key.Text,
                                  Selected = g.Key.Selected
                              }).ToCollection();
            }

            return returnList;
        }

        #endregion

        #endregion

        #endregion

        #region AJAX Calls

        /// <summary>
        /// Exports a BOE to a pre-formatted MS Word template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <param name="ws">Workspace containing the boe</param>
        /// <param name="boeID">ID of BOE to preview</param>
        /// <param name="Response">current HTTP response</param>
        public async Task ExportBOESearchPreview(FullWorkspace ws, int boeID, HttpResponseBase Response)
        {
            // Get the BOE to export
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            if (boe != null)
            {
                // Get the Workspace that the BOE belongs to. If it's the same one as what's being passed in, then we do not need to create one..
                FullWorkspace exportWorkspace = boe.WorkspaceID == ws.Id ? ws : boe.Workspace;

                if (exportWorkspace.AllowSearch || ws.Id == exportWorkspace.Id)
                {
                    bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(boe.WorkspaceID)
                                                where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                                select p).Any();

                    // Load up the data needed
                    boe.LoadBOEsRTEData();
                    exportWorkspace.LoadTaskElementRTEData();
                    exportWorkspace.LoadTravelRTEData();
                    exportWorkspace.LoadODCsRTEData();
                    exportWorkspace.LoadMaterialsRTEData();

                    // Get RTE overrides
                    ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateOverrides = this.rteTemplateDataLoader.GetByWorkspaceId(exportWorkspace.Id, new FullBoe[] { boe });

                    BOEExportInputs inputs = new BOEExportInputs(new FullBoe[] { boe }, exportWorkspace.Boes.ToList(), exportWorkspace.TaskElements.ToList(), exportWorkspace, rteTemplateOverrides, exportWorkspace.MoqTypeSelections.ToList(), true, true);

                    // Get BOE Summary Grid data for the current BOE. Used for populating the summary grid on the template
                    List<BOESummaryGridModelView> boeSummaryGridModelViews = this._BOESummary.GetBOESummaryGridModelViews(boe, inputs, isSubcontractorUser).ToList();

					// Get template based on workspace preferences
					WorkspaceExportFormatDTO wsExportFormatDTO = ws.WorkspaceExportFormatTemplate;

                    // Get BOE Export Model View for the current BOE. Used for filling in most of the data on the template.
                    BOEExportModelView boeExportModelView;
					bool isCustomExport = wsExportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.MASTER;
					ICollection<BOEExportModelView> boeExportModelViews;

					// Setup BOE Export ModelViews
					if (isCustomExport)
					{
						//use the custom exporter for master template types
						this._boeCustomExporter.SetWorkspacePrecisionVariables(exportWorkspace);
						boeExportModelView = this._boeCustomExporter.ConvertBoeDTOsToExportMVs(new FullBoe[] { boe }, inputs).First();
						boeExportModelViews = new List<BOEExportModelView> { boeExportModelView };
					}
					else
					{
						this._BOEExporter.SetWorkspacePrecisionVariables(exportWorkspace);
						boeExportModelView = this._BOEExporter.ConvertBoeDTOsToExportMVs(inputs).First();
						boeExportModelViews = new List<BOEExportModelView> { boeExportModelView };
					}

					if (Utilities.IsReportGenerationExternal)
					{
						await this.boeReportsHttpService.ExportBOEsToWord(null, Response, isCustomExport, wsExportFormatDTO, inputs, boeExportModelViews,
							boeSummaryGridModelViews, false);
					}
					else if (isCustomExport)
					{
							
						this._boeCustomExporter.ExportBOEToWordFile(inputs, boeExportModelViews, boeSummaryGridModelViews, ws, null, Response, string.Format("genBOEExport-{0}.docx", boeID), wsExportFormatDTO);
					}
					else
					{
						this._BOEExporter.ExportBOEToWordFile(inputs, boeExportModelViews, boeSummaryGridModelViews, ws, Response, string.Format("genBOEExport-{0}.docx", boeID), wsExportFormatDTO.PhysicalFilePathCache);
					}
                }
                else
                {
                    throw new ArgumentException("The workspace that this BOE belongs to does not allow searches.");
                }
            }
            else
            {
                throw new ArgumentException("There is no BOE with the ID given.");
            }
        }

        /// <summary>
        /// Exports a ProjectMap to a pre-formatted MS Word template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <param name="ws">Workspace containing the boe</param>
        /// <param name="projectMapId">ID of projectMapId to preview</param>
        /// <param name="Response">current HTTP response</param>
        public void ExportProjectMapSearchPreview(FullWorkspace ws, int projectMapId, HttpResponseBase Response)
        {
            // Get the projectMap to export
            ProjectMapModelView projectMap = this.projectMapLoader.GetById(projectMapId);

            if (projectMap != null)
            {
                // Get the Workspace that the BOE belongs to. If it's the same one as what's being passed in, then we do not need to create one..
                FullWorkspace exportWorkspace = projectMap.WorkspaceId == ws.Id ? ws : this.Factory.CreateFullWorkspace(projectMap.WorkspaceId);

                if (exportWorkspace.AllowSearch || ws.Id == exportWorkspace.Id)
                {
                    bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(projectMap.WorkspaceId)
                                                where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                                select p).Any();

                    FullBoe boe = exportWorkspace.Boes.First(b => b.TaskElements.Any(t => t.taskElementLabors.Any(tl => tl.ProjectMapId == projectMapId)));
                    
                    // Make a clone so we do not mess with anything cached
                    boe = boe.DeepClone();
                    BoeTaskElementDTO taskElement = boe.TaskElements.First(t => t.taskElementLabors.Any(tl => tl.ProjectMapId == projectMapId));
                    ResourceTypeDto matchingResource = taskElement.taskElementLabors.First(tl => tl.ProjectMapId == projectMapId);
                    taskElement.taskElementLabors = new Collection<ResourceTypeDto> { matchingResource };
                    boe.SetTaskElements(new Collection<BoeTaskElementDTO> { taskElement });

                    // there should only be one Task Element for a tiered BOE, only show the correct taskElementLabor

                    BOEExportInputs inputs = new BOEExportInputs(new FullBoe[] { boe }, exportWorkspace.Boes.ToList(), exportWorkspace.TaskElements.ToList(), exportWorkspace);

                    // Get BOE Summary Grid data for the current BOE. Used for populating the summary grid on the template
                    ICollection<BOESummaryGridModelView> boeSummaryGridModelViews = this._BOESummary.GetBOESummaryGridModelViews(boe, inputs, isSubcontractorUser);

					// Get template based on workspace preferences
					WorkspaceExportFormatDTO wsExportFormatDTO = ws.WorkspaceExportFormatTemplate;

                    // Get BOE Export Model View for the current BOE. Used for filling in most of the data on the template.
                    BOEExportModelView boeExportModelView;

                    if (wsExportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.MASTER)
                    {
                        // use the custom exporter for master template types
                        this._boeCustomExporter.SetWorkspacePrecisionVariables(exportWorkspace);
                        boeExportModelView = this._boeCustomExporter.ConvertBoeDTOsToExportMVs(new FullBoe[] { boe }, inputs).First();
                        this._boeCustomExporter.ExportBOEToWordFile(inputs, new List<BOEExportModelView> { boeExportModelView }, boeSummaryGridModelViews, ws, null, Response, string.Format("genBOEExport-{0}.docx", projectMapId), wsExportFormatDTO);
                    }
                    else
                    {
                        this._BOEExporter.SetWorkspacePrecisionVariables(exportWorkspace);
                        boeExportModelView = this._BOEExporter.ConvertBoeDTOsToExportMVs(inputs).First();
                        this._BOEExporter.ExportBOEToWordFile(inputs, new List<BOEExportModelView> { boeExportModelView }, boeSummaryGridModelViews, ws, Response, string.Format("genBOEExport-{0}.docx", projectMapId), wsExportFormatDTO.PhysicalFilePathCache);
                    }
                }
                else
                {
                    throw new ArgumentException("The workspace that this Project Map belongs to does not allow searches.");
                }
            }
            else
            {
                throw new ArgumentException("There is no Project Map with the ID given.");
            }
        }

        /// <summary>
        /// Copies the project map.
        /// </summary>
        /// <param name="ws">Workspace to be copied to.</param>
        /// <param name="copyProjectMapId">Id of Project Map to be copied.</param>
        public void CopyProjectMap(FullProjectMapWorkspace ws, int copyProjectMapId)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (copyProjectMapId <= 0)
            {
                throw new ArgumentException("The project map Id is invalid.");
            }

            ProjectMapModelView copyModel = this.projectMapLoader.GetById(copyProjectMapId);

            if (copyModel == null)
            {
                throw new ArgumentException("The project map Id is invalid.");
            }

            ICollection<ProjectMapModelView> models = new Collection<ProjectMapModelView> { copyModel };
            copyModel.Id = -1;
            copyModel.WorkspaceId = ws.Id;
            copyModel.Updateable = UpdateType.Upsert;

            // perform validation
            ICollection<ProjectMapModelView> allModels = ws.ProjectMapData.ToList();
            allModels.Add(copyModel);

            ProjectMapValidator validator = new ProjectMapValidator(allModels.ToArray(), ws);
            validator.Validate();

            if (validator.ValidationMessages.Any(vm => !vm.TreatAsWarning))
            {
                // need to remove the warnings, and weed out duplicate messages
                ICollection<ValidationMessage> messages = new Collection<ValidationMessage>();
                foreach (ValidationMessage message in validator.ValidationMessages)
                {
                    if (!message.TreatAsWarning)
                    {
                        if (messages.All(vm => vm.ValidationIssue != message.ValidationIssue))
                        {
                            messages.Add(message);
                        }
                    }
                }

                throw new GenValidationException(messages);
            }

            this.projectMapLoader.BulkSave(models);

            // Clear the cache after the project map has been saved since it is a kill/fill
            this.Factory.ClearWorkspaceCache(ws.Shortname);
        }

        /// <summary>
        /// Saves a BOE Header edit.
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boe">BOE containing the header</param>
        /// <param name="inBOEHeader">Modelview for the BOE Header</param>
        /// <param name="inBOEHeaderDescription">Modelview for the BOE description</param>
        /// <param name="descriptionOnly">Bool denoting if only description was changed</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public virtual void SaveEditBoeHeader(FullWorkspace ws, FullBoe boe, IBOEHeaderModelView inBOEHeader, BOEHeaderDescriptionModelView inBOEHeaderDescription, bool descriptionOnly)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (inBOEHeader == null)
            {
                throw new ArgumentNullException(nameof(inBOEHeader));
            }

            if (inBOEHeaderDescription == null)
            {
                throw new ArgumentNullException(nameof(inBOEHeaderDescription));
            }

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            if (string.IsNullOrEmpty(inBOEHeaderDescription.Description))
            {
                ValidationErrors.Add(new ValidationMessage("Description", "Description is required."));
                throw new GenValidationException(ValidationErrors);
            }
            
            WorkspaceDTO workspaceDTO = ws;
            List<ValidationMessage> richTextValidationMessages = new List<ValidationMessage>();

            richTextValidationMessages.AddRange(this._genBOEControllerLogic.ScrubRichTextPropertiesForSave(inBOEHeader));
            richTextValidationMessages.AddRange(this._genBOEControllerLogic.ScrubRichTextPropertiesForSave(inBOEHeaderDescription));

            if (descriptionOnly)
            {
                // the only field we're allowed to adjust is the description
                boe.Title = inBOEHeader.Title;
                boe.Description = inBOEHeaderDescription.Description;
                boe.Updateable = UpdateType.Upsert;
                boe.DataSource = inBOEHeader.DataSource;

                BOEDateValidator validator = new BOEDateValidator(this.Factory);
                List<string> validationerrors = new List<string>();

                if (boe.Updateable != UpdateType.Deleted)
                {
                    Collection<Dictionary<string, string>> boeDictionary = new Collection<Dictionary<string, string>>();
                    boeDictionary.Add(new Dictionary<string, string>());
                    boeDictionary.First<Dictionary<string, string>>().Add("pkid", boe.Id.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("StartDate", boe.StartDate.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("EndDate", boe.EndDate.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("ClinID", boe.CLINID.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("WorkspaceID", boe.WorkspaceID.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("context", "true");
                    boeDictionary.First<Dictionary<string, string>>().Add("contextElement", "StartDate");
                    validationerrors.AddRange(validator.validation(null, boeDictionary));
                    boeDictionary.First<Dictionary<string, string>>()["contextElement"] = "EndDate";
                    validationerrors.AddRange(validator.validation(null, boeDictionary));
                }

                validationerrors.AddRange(richTextValidationMessages.Select(v => v.ValidationIssue));

                if (validationerrors.Any())
                {
                    foreach (string message in validationerrors)
                    {
                        ValidationErrors.Add(new ValidationMessage("BOEStartDate", message));
                    }
                    throw new GenValidationException(ValidationErrors);
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // do not use the MediatedSave for saving the boe header. In MediatedSave, there is unnecessary logic to go through for this
                    // type of save (like rates, recalculations) none of that can be effected and
                    this._BoeMediator.SaveEditBoeHeader(boe);
                    if (inBOEHeaderDescription.RteTemplateAnswers != null && inBOEHeaderDescription.RteTemplateAnswers.Any())
                    {
                        this.rteTemplateDataLoader.SaveAnswers(inBOEHeaderDescription.RteTemplateAnswers);
                    }

                    scope.Complete();
                }

                boe = this.Factory.CreateFullBoe(boe.Id);

            }
            else
            {
                // get boe dto
                DataRelationshipVerifier.VerifyDataRelation(boe, workspaceDTO.Id);
                boe.HistoricMetricDisclosureChecked = inBOEHeader.HistoricMetricDisclosureChecked;
                boe.Title = inBOEHeader.Title;
                boe.Description = inBOEHeaderDescription.Description;
                boe.Updateable = UpdateType.Upsert;
                boe.DataSource = inBOEHeader.DataSource;

                BOEDateValidator validator = new BOEDateValidator(this.Factory);
                List<string> validationerrors = new List<string>();

                if (boe.Updateable != UpdateType.Deleted)
                {
                    Collection<Dictionary<string, string>> boeDictionary = new Collection<Dictionary<string, string>>();
                    boeDictionary.Add(new Dictionary<string, string>());
                    boeDictionary.First<Dictionary<string, string>>().Add("pkid", boe.Id.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("StartDate", boe.StartDate.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("EndDate", boe.EndDate.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("ClinID", boe.CLINID.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("WorkspaceID", boe.WorkspaceID.ToString());
                    boeDictionary.First<Dictionary<string, string>>().Add("context", "true");
                    boeDictionary.First<Dictionary<string, string>>().Add("contextElement", "StartDate");
                    validationerrors.AddRange(validator.validation(null, boeDictionary));
                    boeDictionary.First<Dictionary<string, string>>()["contextElement"] = "EndDate";
                    validationerrors.AddRange(validator.validation(null, boeDictionary));
                }

                validationerrors.AddRange(richTextValidationMessages.Select(v => v.ValidationIssue));

                //Validate BOE Custom Fields
                this.ValidateBOEHeaderCustomFields(ws, boe, inBOEHeader);


                if (validationerrors.Any())
                {
                    foreach (string message in validationerrors)
                    {
                        ValidationErrors.Add(new ValidationMessage("BOEStartDate", message));
                    }
                    throw new GenValidationException(ValidationErrors);
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // do not use the MediatedSave for saving the boe header. In MediatedSave, there is unnecessary logic to go through for this
                    // type of save (like rates, recalculations) none of that can be effected and
                    this._BoeMediator.MediatedSaveBOEs(ws, new Collection<BoeDTO>() { boe });
                    if (inBOEHeaderDescription.RteTemplateAnswers != null && inBOEHeaderDescription.RteTemplateAnswers.Any())
                    {
                        this.rteTemplateDataLoader.SaveAnswers(inBOEHeaderDescription.RteTemplateAnswers);
                    }

                    scope.Complete();
                }

                boe = this.Factory.CreateFullBoe(boe.Id);
            }
        }

        /// <summary>
        /// Validates the BOE Level Custom Fields being saved for a BOE.
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boe">BOE containing the header</param>
        /// <param name="inBOEHeader">Modelview for the BOE Header</param>
        public virtual void ValidateBOEHeaderCustomFields(FullWorkspace ws, FullBoe boe, IBOEHeaderModelView inBOEHeader)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (inBOEHeader == null)
            {
                throw new ArgumentNullException(nameof(inBOEHeader));
            }

            Collection<CustomFieldValueContainer> selectionDTO = new Collection<CustomFieldValueContainer>();

            foreach (CustomFieldSelectionModelView selection in inBOEHeader.CustomFieldValues)
            {
                UpdateType typeOfUpdate = UpdateType.Upsert;
                int customFieldValueID;
                
                // -1 is used when a custom field value has been removed and the blank is essentially chosen
                // need to use the custom field value id so this is correctly deleted from the db
                if (selection.CustomFieldValueID == -1 && !selection.IsOpenEnded)
                {
                    typeOfUpdate = UpdateType.Deleted;
                    customFieldValueID = boe.CustomFieldValueContainers.Where(x => x.ContainerID == selection.SelectionID).Select(x => x.CustomFieldValueID).FirstOrDefault();
                }
                else
                {
                    if (selection.IsOpenEnded && string.IsNullOrEmpty(selection.OpenEndedValue))
                    {
                        typeOfUpdate = selection.SelectionID > 0 ? UpdateType.Deleted : UpdateType.None;
                    }
                    customFieldValueID = selection.CustomFieldValueID;
                }

                selectionDTO.Add(new CustomFieldValueContainer()
                {
                    ContainerID = selection.SelectionID,
                    CustomFieldID = selection.CustomFieldID,
                    CustomFieldValueID = customFieldValueID,
                    Updateable = typeOfUpdate,
                    UpdateDate = selection.UpdateDate,
                    IsOpenEnded = selection.IsOpenEnded,
                    OpenEndedValue = selection.OpenEndedValue
                });
            }

            ICollection<CustomFieldDTO> allCustomFieldsForWorkspace = new Collection<CustomFieldDTO>(ws.CustomFields.Where(x => x.CustomFieldDisplayID.Equals(CustomFieldType.BoeDisplay)).Select(x => x).ToArray());

            Collection<ValidationMessage> errors = this._validationHelper.BOEHeaderCustomFieldValidation(selectionDTO, allCustomFieldsForWorkspace);

            if (errors != null)
            {
                throw new GenValidationException(errors);
            }

            boe.CustomFieldValueContainers = selectionDTO;
        }

        /// <summary>
        /// Perform the action for submitting a BOE for review
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boeID">ID of BOE to be submitted</param>
        public void SubmitForReview(FullWorkspace ws, int boeID)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                // write a message to the log
                this._boeCommentLoader.Save(new Collection<BOECommentDTO> { 
                new BOECommentDTO { BOEComment = "Submitted for review",
                                    Id = -1,
                                    FieldID = (int)FieldType.SubmittedForReview,
                                    BoeID = boeID,
                                    Updateable = UpdateType.Upsert,                                    
                                    BOEResponseToCommentID = null,
                                    BOECommentETIUserID = ws.CurrentActiveUser.UserID
                                  }});

                scope.Complete();
            }

            // send email (after save is complete)
            this._emailer.SendBOESubmittedForReview(this.Factory.CreateFullBoe(boeID));
        }

        /// <summary>
        /// Performs action to delete all task elements for a BOE
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE Containing task elements</param>
        public void DeleteAllBOETaskElements(FullWorkspace ws, FullBoe boe)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = boe ?? throw new ArgumentNullException(nameof(boe));

            ICollection<BoeTaskElementDTO> allTEs = boe.TaskElements.ToList();

            allTEs.ForEach(taskElement => { taskElement.Updateable = UpdateType.Deleted; });

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.DeleteMoqTypesForBoe(ws, new List<int>() { boe.Id });
                this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(allTEs, ws);

                scope.Complete();
            }
        }

        /// <summary>
        /// Delete Moq Types For Boes
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boeIds">BoeIds</param>
        public void DeleteMoqTypesForBoe(FullWorkspace ws, ICollection<int> boeIds)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = boeIds ?? throw new ArgumentNullException(nameof(boeIds));

            ICollection<MoqTypeSelection> moqTypesToDelete = ws.MoqTypeSelections.Where(x => boeIds.Contains(x.BoeId)).ToList();
            moqTypesToDelete.ForEach(moqType => { moqType.Updateable = UpdateType.Deleted; });
            this.moqTypeDataLoader.Save(moqTypesToDelete);
        }

        /// <summary>
        /// Performs the action to delete the given task element
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">Boe containing task to be deleted</param>
        /// <param name="deletedTask">Task to be deleted</param>
        public void DeleteTaskElement(FullWorkspace ws, FullBoe boe, GenericTaskElementGridRow deletedTask)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = boe ?? throw new ArgumentNullException(nameof(boe));
            _ = deletedTask ?? throw new ArgumentNullException(nameof(deletedTask));

            // add deleted taskElementGrid to boeDTODeleteTaskElements
            BoeTaskElementDTO boeDTOTaskElement = (from t in boe.TaskElements
                                                   where t.Id == deletedTask.TaskElementDetailID
                                                   select t).FirstOrDefault();

            // Dictionary to keep track of workspace variable IDs that need to be updated and their old variable total
            Dictionary<int, decimal> WorkspaceVarOldValueID = new Dictionary<int, decimal>();

            if (boeDTOTaskElement != null)
            {
                DataRelationshipVerifier.VerifyDataRelation(boeDTOTaskElement, boe.Id);
                boeDTOTaskElement.UpdateDate = deletedTask.UpdateDate;
                boeDTOTaskElement.Updateable = UpdateType.Deleted;
                
                IReadOnlyCollection<int> WorkspaceVarIds = boe.WorkspaceVariablesIdsForBoe;
                ICollection<WorkspaceVariableDTO> workspaceVariablesOldValue = new Collection<WorkspaceVariableDTO>();
                if (WorkspaceVarIds.Any())
                {
                    workspaceVariablesOldValue = this._workspaceVariableLoader.GetByIds(WorkspaceVarIds);
                    foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOldValue)
                    {
                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

                        decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                        WorkspaceVarOldValueID.Add(workspaceVar.Id, oldTotalValue);
                    }
                }

                ICollection<MoqTypeSelection> moqTypesToDelete = this.moqTypeDataLoader.GetByBoeId(boe.Id).Where(x => x.TaskId == boeDTOTaskElement.Id).ToList();
                moqTypesToDelete.ForEach(moqType => { moqType.Updateable = UpdateType.Deleted; });

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this.moqTypeDataLoader.Save(moqTypesToDelete);
                    this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { boeDTOTaskElement }, ws);

                    if (WorkspaceVarOldValueID.Any())
                    {
                        // save the workspace variables
                        foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOldValue)
                        {
                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

                            workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                            workspaceVar.Updateable = UpdateType.Upsert;
                            this._workspaceVariableLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
                        }
                    }

                    this.BoeLaborControllerLogic.CalculateLinkedTaskElements(new Collection<ValidationMessage>(), new Collection<BoeTaskElementDTO>() { boeDTOTaskElement }, new Collection<BoeTaskElementDTO>(), ws);

                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// For each task element that was recalculated, if their parent boe was in approved or awaiting approval state, move it back to draft
        /// </summary>
        /// <param name="workspace">Workspace containing task elements</param>
        /// <param name="boeTaskElementsToRecalculate">Elements to recalculate</param>
        public void AdjustStateOfTaskElements(FullWorkspace workspace, Collection<BoeTaskElementDTO> boeTaskElementsToRecalculate)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<int> boesAssociatedWithTaskElements = boeTaskElementsToRecalculate.Select(x => x.BoeID).ToCollection();
            ICollection<FullBoe> boes = (from b in workspace.Boes where boesAssociatedWithTaskElements.Contains(b.Id) select b).ToCollection();

            foreach (FullBoe boe in boes)
            {
                FullBoe readjustBoe = this.Factory.CreateFullBoe(boe);
                if (readjustBoe.State.Equals(BOEState.AwaitingApproval) || readjustBoe.State.Equals(BOEState.Approved) || readjustBoe.State == BOEState.DraftLocked)
                {
                    BOEState oldBOEState = readjustBoe.State;
                    BOEState newBOEState = BOEState.Draft;

                    // Validate the Awaiting Approval or Approved to Draft state transition
                    string validationMessage;
                    if (!this._boeStateMachine.PerformStateTransitionValidation(readjustBoe, workspace, readjustBoe.State, newBOEState, out validationMessage))
                    {
                        // not valid ... communicate to user
                        throw new ValidationException(validationMessage);
                    }

                    // If the transition is valid, set the BOE to Draft and save it
                    readjustBoe.Updateable = UpdateType.Upsert;
                    readjustBoe.State = newBOEState;
                    this._BoeMediator.MediatedSave(workspace, readjustBoe);

                    // Perform common state transition actions
                    this._boeStateMachine.PerformStateTransitionAction(readjustBoe, workspace, oldBOEState, readjustBoe.State);
                }
            }

            workspace.RefreshBoes();
        }

        /// <summary>
        /// Perform the action for submitting a BOE for approval
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE to be submitted</param>
        /// <returns>Modelview for BOE Validation</returns>
        public ValidationBOEModelView SubmitForApproval(FullWorkspace ws, FullBoe boe)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            // need to save the BOE state to Awaiting Approval, keep a copy of the BOE before the save to be used below
            BOEState stateBeforeSave = boe.State;

            // check if state transition is valid before actually validating the internal BOE data
            string errorMessage;
            if (!this._boeStateMachine.PerformStateTransitionValidation(boe, ws, boe.State, BOEState.AwaitingApproval, out errorMessage))
            {
                // pull this error message from the state machine itself
                throw new ValidationException(errorMessage);
            }

            // Call to the business layer to validate the BOE
            ValidationBOEModelView validatedBOE = this._validateBOE.ValidateBOE_OnValidateBtnClick(boe,ws);

            // if there is nothing in the validation BOE form, then we can precede with setting the 
            // BOE state to Awaiting Approval
            if (validatedBOE.isValid)
            {
                boe.State = BOEState.AwaitingApproval;
                boe.Updateable = UpdateType.Upsert;
                
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this._BoeMediator.MediatedSave(ws, boe);

                    // write a message to the log
                    this._boeCommentLoader.Save(new Collection<BOECommentDTO> { 
                        new BOECommentDTO { BOEComment = "Submitted for approval",
                            Id = -1,
                            FieldID = (int)FieldType.BOEStatus,
                            BoeID = boe.Id,
                            Updateable = UpdateType.Upsert,                                    
                            BOEResponseToCommentID = null,
                            BOECommentETIUserID = ws.CurrentActiveUser.UserID
                        }});

                    // send emails to approvers that BOE is awaiting approval
                    this._boeStateMachine.PerformStateTransitionAction(this.Factory.CreateFullBoe(boe), ws, stateBeforeSave, boe.State);

                    scope.Complete();
                }
            }

            return validatedBOE;
        }

        /// <summary>
        /// Performs a search for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">The boe identifier.</param>
        /// <param name="advSearchParams">Parameters to search for</param>
        /// <returns>Modelview for search results</returns>
        public SearchResultsModelView AdvancedSearchForBOEs(FullWorkspace ws, int boeID, BOEAdvancedSearchModelView advSearchParams)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (advSearchParams == null)
            {
                throw new ArgumentNullException(nameof(advSearchParams));
            }

			// Perform Date Range validation
			DateRangeValidator validator = new DateRangeValidator();

            Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "StartDate", advSearchParams.StartDate },
                        { "EndDate", advSearchParams.EndDate }
                    }
                };

            Collection<string> validationerrors = validator.validation(null, validationData);

            if (validationerrors.Count != 0)
            {
                throw new GenValidationException(Validator.CreateValidationErrorResponse(validationerrors));
            }

            int searchResultsThreshold = ConfigurationUtilities.GetAppSetting<int>("SearchResultsThreshold", Constants.SEARCH_RESULTS_THRESHOLD_DEFAULT);
            BOESearchDTO searchDTO = new BOESearchDTO()
            {
                IsQuickSearch = false,
                ApproverUser = advSearchParams.ApproverNTID,
                AuthorUser = advSearchParams.AuthorNTID,
                BoeTitle = advSearchParams.BOETitle,
                BoeDescription = advSearchParams.BoeDescription,
                CostVolumeLeadUser = advSearchParams.CostVolumeLeadNTID,
                EndDate = !string.IsNullOrEmpty(advSearchParams.EndDate) ? (DateTime?)Convert.ToDateTime(advSearchParams.EndDate) : null,
                PerformingOrg = advSearchParams.PerformingOrg,
                RFP = advSearchParams.RFP,
                SelectedCategory = advSearchParams.SelectedCategory,
                SourcesOfData = advSearchParams.SourcesOfData,
                StartDate = !string.IsNullOrEmpty(advSearchParams.StartDate) ? (DateTime?)Convert.ToDateTime(advSearchParams.StartDate) : null,
                TaskDescription = advSearchParams.TaskDescription,
                TaskTitle = advSearchParams.TaskTitle,
                WorkspaceDescription = advSearchParams.WorkspaceDescription,
                CLINNumber = advSearchParams.CLINNumber,
                CLINTitle = advSearchParams.CLINTitle,
                WBSNumber = advSearchParams.WBSNumber,
                WBSTitle = advSearchParams.WBSTitle,
                WorkspaceName = advSearchParams.WorkspaceName,
                WorkspaceID = ws.Id,
                BOEID = boeID,
                SearchResultsThreshold = searchResultsThreshold
            };

            return this.CreateSearchResultsMV(ws, advSearchParams.IsCopyFromBoeContext, searchResultsThreshold, this._boeSearchLoader.GetAdvancedSearchResults(searchDTO));
        }

        /// <summary>
        /// Performs a search for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="advSearchParams">Parameters to search for</param>
        /// <returns>Modelview for search results</returns>
        public SearchResultsModelView AdvancedSearchForBOEs(FullWorkspace ws, BOEProjectMapAdvancedSearchModelView advSearchParams)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (advSearchParams == null)
            {
                throw new ArgumentNullException(nameof(advSearchParams));
            }

            int searchResultsThreshold = ConfigurationUtilities.GetAppSetting<int>("SearchResultsThreshold", Constants.SEARCH_RESULTS_THRESHOLD_DEFAULT);
            BOEProjectMapSearchDTO searchDTO = new BOEProjectMapSearchDTO()
            {
                ActivityId = advSearchParams.ActivityId,
                ActivityName = advSearchParams.ActivityName,
                CamName = advSearchParams.CamName,
                Category = advSearchParams.Category,
                Rationale = advSearchParams.Rationale,
                SowTitle = advSearchParams.SowTitle,
                Task = advSearchParams.Task,
                WorkspaceDescription = advSearchParams.WorkspaceDescription,
                WBSNumber = advSearchParams.WBSNumber,
                WBSTitle = advSearchParams.WBSTitle,
                WorkspaceName = advSearchParams.WorkspaceName,
                WorkspaceID = ws.Id,
                SearchResultsThreshold = searchResultsThreshold
            };

            SearchResultsModelView results = this.CreateProjectMapSearchResultsMV(ws, advSearchParams.IsCopyFromBoeContext, searchResultsThreshold, this._boeSearchLoader.GetAdvancedSearchResults(searchDTO));

            return results;
        }

        /// <summary>
        /// Validate that atleast one field is filled in.
        /// </summary>
        /// <param name="advSearchParams">Parameters for advanced search</param>
        /// <returns>if search is valid or not</returns>
        public bool AdvSearchRequiredFieldValidation(BOEAdvancedSearchModelView advSearchParams)
        {
            if (advSearchParams == null)
            {
                throw new ArgumentNullException(nameof(advSearchParams));
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WorkspaceDescription))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WorkspaceName))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.StartDate))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.EndDate))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.RFP))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.BOETitle))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.BoeDescription))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.TaskTitle))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.TaskDescription))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.SourcesOfData))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.PerformingOrg))
            {
                return true;
            }
            if (!string.IsNullOrWhiteSpace(advSearchParams.CostVolumeLeadNTID))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.AuthorNTID))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.ApproverNTID))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WBSNumber))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WBSTitle))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.CLINNumber))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.CLINTitle))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate that atleast one field is filled in.
        /// </summary>
        /// <param name="advSearchParams">Parameters for advanced search</param>
        /// <returns>if search is valid or not</returns>
        public bool AdvSearchRequiredFieldValidation(BOEProjectMapAdvancedSearchModelView advSearchParams)
        {
            if (advSearchParams == null)
            {
                throw new ArgumentNullException(nameof(advSearchParams));
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WorkspaceDescription))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WorkspaceName))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.ActivityId))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.ActivityName))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.CamName))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.Category))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.Rationale))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.SowTitle))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.Task))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WBSNumber))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(advSearchParams.WBSTitle))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Performs a quicksearch for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">The boe identifier.</param>
        /// <param name="quickSearchParams">Parameters to search for</param>
        /// <returns>Modelview of search results</returns>
        public SearchResultsModelView QuickSearchForBOEs(FullWorkspace ws, int? boeID, BOEQuickSearchModelView quickSearchParams)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (quickSearchParams == null)
            {
                throw new ArgumentNullException(nameof(quickSearchParams));
            }

            int searchResultsThreshold = ConfigurationUtilities.GetAppSetting<int>("SearchResultsThreshold", Constants.SEARCH_RESULTS_THRESHOLD_DEFAULT);

            SearchResultsModelView results;
            if (ws.IsProjectMapWorkspace)
            {
                BOEProjectMapSearchDTO searchDTO = new BOEProjectMapSearchDTO()
                {
                    QuickSearchText = quickSearchParams.QuickSearchText,
                    SelectedCategory = quickSearchParams.SelectedCategory,
                    WorkspaceID = ws.Id,
                    SearchResultsThreshold = searchResultsThreshold
                };

                results = this.CreateProjectMapSearchResultsMV(ws, quickSearchParams.IsCopyFromBoeContext, searchResultsThreshold, this._boeSearchLoader.GetQuickSearchResults(searchDTO));
            }
            else
            {
                BOESearchDTO searchDTO = new BOESearchDTO()
                {
                    IsQuickSearch = true,
                    QuickSearchText = quickSearchParams.QuickSearchText,
                    SelectedCategory = quickSearchParams.SelectedCategory,
                    WorkspaceID = ws.Id,
                    BOEID = boeID ?? -1,
                    SearchResultsThreshold = searchResultsThreshold
                };

                results = this.CreateSearchResultsMV(ws, quickSearchParams.IsCopyFromBoeContext, searchResultsThreshold, this._boeSearchLoader.GetQuickSearchResults(searchDTO));
            }

            return results;
        }

        /// <summary>
        /// Create search results model view using results from either Quick Search or Advanced Search.
        /// If the current user is Domestic, return all BOEs.
        /// If the current user is Foreign, only return BOEs within workspaces where user has access.
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="isCopyFromBoeContext"></param>
        /// <param name="searchResultsThreshold"></param>
        /// <param name="searchResults"></param>
        /// <returns>ModelView containing search results.</returns>
        private SearchResultsModelView CreateSearchResultsMV(FullWorkspace ws, bool isCopyFromBoeContext, int searchResultsThreshold, ICollection<BOESearchResultDTO> searchResults)
        {
            Collection<int> results;
            // Check for Foreign users
            if (this._SecurityInformation.IsDomesticUser(System.Threading.Thread.CurrentPrincipal))
            {
                // return all BOEIDs for Domestic users
                results = searchResults.Select(i => i.BOEID).ToCollection();
            }
            else
            {
                UserDTO currentUser = ws.CurrentActiveUser;
                IReadOnlyCollection<SecurityPermissionsResponse> usersPermissions = this.Factory.GetPermissionsForUser(currentUser.NTID);

                // check access before returning BOEIDs for Foreign users
                results = new Collection<int>((from i in searchResults
                                               where this._SecurityAccess.IsAuthorized(new SecurityPermissionsRequested
                                                {
                                                    WorkspaceId = i.WorkspaceID,
                                                    PageToCheck = SecurityPage.WorkspaceHome
                                                }, ws, usersPermissions) == SecurityAuthorization.Read
                                               select i.BOEID).ToArray());
            }

            SearchResultsModelView modelView = new SearchResultsModelView(isCopyFromBoeContext);
            modelView.PagedIndexes = results;
            modelView.CurrentPage = 1;

            for (int i = modelView.StartArrayIndex; i <= modelView.EndArrayIndex; i++)
            {
                FullBoe currentBOE = this.Factory.CreateFullBoe(results[i]);
                modelView.BOEResults.Add(this.GetBOESearchResult(currentBOE, currentBOE.Workspace));
            }
            
            // Base searchResultsThreshold check on original searchResults (not final results after pruning for foreign user access).
            if (searchResults.Count >= searchResultsThreshold)
            {
                modelView.SearchResultsMessage = string.Format("Warning: The Search Results Threshold has been reached. Only the first {0} results were returned.  Please refine your criteria and search again.", searchResultsThreshold);
            }

            return modelView;
        }

        /// <summary>
        /// Create search results model view using results from either Quick Search or Advanced Search.
        /// If the current user is Domestic, return all ProjectMaps.
        /// If the current user is Foreign, only return ProjectMaps within workspaces where user has access.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="isCopyFromBoeContext">if set to <c>true</c> [is copy from BOE context].</param>
        /// <param name="searchResultsThreshold">The search results threshold.</param>
        /// <param name="searchResults">The search results.</param>
        /// <returns>
        /// ModelView containing search results.
        /// </returns>
        private SearchResultsModelView CreateProjectMapSearchResultsMV(FullWorkspace ws, bool isCopyFromBoeContext, int searchResultsThreshold, ICollection<BOESearchResultDTO> searchResults)
        {
            Collection<int> results;
            // Check for Foreign users
            if (this._SecurityInformation.IsDomesticUser(System.Threading.Thread.CurrentPrincipal))
            {
                // return all ProjectMapIds for Domestic users
                results = searchResults.Select(i => i.ProjectMapId).ToCollection();
            }
            else
            {
                UserDTO currentUser = ws.CurrentActiveUser;
                IReadOnlyCollection<SecurityPermissionsResponse> usersPermissions = this.Factory.GetPermissionsForUser(currentUser.NTID);

                // check access before returning ProjectMapIds for Foreign users
                results = new Collection<int>((from i in searchResults
                                               where this._SecurityAccess.IsAuthorized(new SecurityPermissionsRequested
                                                {
                                                    WorkspaceId = i.WorkspaceID,
                                                    PageToCheck = SecurityPage.WorkspaceHome
                                                }, ws, usersPermissions) == SecurityAuthorization.Read
                                               select i.ProjectMapId).ToArray());
            }

            SearchResultsModelView modelView = new SearchResultsModelView(isCopyFromBoeContext);
            modelView.PagedIndexes = results;
            modelView.CurrentPage = 1;

            ICollection<int> boeIds = new List<int>();
            for (int i = modelView.StartArrayIndex; i <= modelView.EndArrayIndex; i++)
            {
                boeIds.Add(results[i]);
            }

            if (boeIds.Any())
            {
                modelView.BOEResults = this._boeSearchLoader.GetSearchResults(boeIds);
            }
            
            // Base searchResultsThreshold check on original searchResults (not final results after pruning for foreign user access).
            if (searchResults.Count >= searchResultsThreshold)
            {
                modelView.SearchResultsMessage = string.Format("Warning: The Search Results Threshold has been reached. Only the first {0} results were returned.  Please refine your criteria and search again.", searchResultsThreshold);
            }

            return modelView;
        }

        /// <summary>
        /// Gets data for search results
        /// </summary>
        /// <param name="ws">Workspace search is performed in</param>
        /// <param name="searchResults">Results of search</param>
        public void PageSearchResults(FullWorkspace ws, SearchResultsModelView searchResults)
        {
            if (searchResults == null)
            {
                throw new ArgumentNullException(nameof(searchResults));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            
            searchResults.BOEResults = new Collection<BOESearchResult>();
            ICollection<int> boeIds = new Collection<int>();
            for (int i = searchResults.StartArrayIndex; i <= searchResults.EndArrayIndex; i++)
            {
                boeIds.Add(searchResults.PagedIndexes[i]);
            }

            if (ws.IsProjectMapWorkspace)
            {
                searchResults.BOEResults = this._boeSearchLoader.GetSearchResults(boeIds);
            }
            else
            {
                ICollection<FullBoe> allBoes = this.Factory.CreateFullBoes(boeIds);

                for (int i = searchResults.StartArrayIndex; i <= searchResults.EndArrayIndex; i++)
                {
                    FullBoe currentBoe = allBoes.First(x => x.Id == searchResults.PagedIndexes[i]);
                    // Call GetBOESearchResult with the workspace of the current BOE. This must be done because
                    // Boes from the search result can come from different workspaces.
                    searchResults.BOEResults.Add(this.GetBOESearchResult(currentBoe, currentBoe.Workspace));
                }
            }
        }

        /// <summary>
        /// Performs action to export BOEs from the ManageBOE page
        /// </summary>
        /// <param name="ws">Workspace containing BOEs</param>
        /// <param name="templateFileName">file name of the template used for export</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        /// <returns>exported file name and formatted filename in an array</returns>
        public async Task<string[]> ExportManageBOE(FullWorkspace ws, string templateFileName, bool blankTemplate)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName;
			if (Utilities.IsReportGenerationExternal)
			{
				BOEExcelExportInputs exportInputs = new BOEExcelExportInputs(ws, templateFileName, blankTemplate,
					this.UserLoader, this._ADUtils, this.PermissionsLoader);
				exportedFileName = await this.boeReportsHttpService.ExportManageBoesToExcel(exportInputs);
			}
			else
			{
				exportedFileName = this._BOEExporter.ExportToExcelFile(templateFileName, ws, blankTemplate);
			}
			
			string formattedWithWorkspace = string.Format("genBOE-{0}-BOEs.xlsm", ws.WorkspaceName);

			return new string[] { exportedFileName, formattedWithWorkspace };
			
        }

        /// <summary>
        /// Performs actions to start import of BOEs on the Manage BOEs page
        /// </summary>
        /// <param name="ws">Workspace containing BOEs</param>
        /// <param name="Request">current HTTP request</param>
        /// <param name="dataToSave">(output) Data to be saved by import</param>
        /// <param name="errorsOccurred">(output) bool noting if any errors occurred</param>
        /// <param name="exception">(output) Exception if any occurred</param>
        /// <returns>Modelview of the import results</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public Collection<ImportBoeResultsModelView> ImportManageBOE(FullWorkspace ws, HttpRequestBase Request, out ICollection<ImportBoeResultsModelView> dataToSave, out bool errorsOccurred, out Exception exception)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (Request == null)
            {
                throw new ArgumentNullException(nameof(Request));
            }

            Collection<ImportBoeResultsModelView> theModelViews = new Collection<ImportBoeResultsModelView>();

            exception = null;
            errorsOccurred = false;

            try
            {
                // Perform Action
                // If a file was uploaded successfully
                if (Request.Files.Count > 0 && Request.Files[0].FileName.Length > 0)
                {
                    // Call the business layer to parse the uploaded file
                    // If the file was successfully parsed, add the results to the genBOE database
                    Collection<ImportedBoe> importResults = this._BOEImporter.ImportBoeFromExcelFile(Request.Files[0].InputStream, ws);

                    List<ImportedBoe> updatedBOEs = importResults.Where(w => w.ImportTypes.Contains(BoeImportResult.UpdateBoe)).ToList();

					VariableCircularReferenceCheckerCache cache = new VariableCircularReferenceCheckerCache();

                    foreach (ImportedBoe updatedBOE in updatedBOEs)
                    {
                        if (updatedBOE.Id > 0)
                        {
                            bool circularReferenceFound = false;
                            FullBoe boe = this.Factory.CreateFullBoe(updatedBOE);

                            if (updatedBOE.WBSID.HasValue)
                            {
                                FullWbs wbs = ws.WbsElements.First(i => i.Id == updatedBOE.WBSID.Value);

                                // Validate chosen WBS for circular references
                                if (this._VariableCircularReferenceChecker.BOEWBSMoveCreatesCircularReference(cache, boe, wbs, updatedBOEs.ToList<BoeDTO>(), ws))
                                {
                                    circularReferenceFound = true;
                                }
                            }

                            if (updatedBOE.CLINID.HasValue)
                            {
                                ClinDTO clin = ws.Clins.First(i => i.Id == updatedBOE.CLINID.Value);
                                // Validate chosen CLINs for circular references
                                if (this._VariableCircularReferenceChecker.BOECLINMoveCreatesCircularReference(ws, cache, boe, clin, updatedBOEs.ToList<BoeDTO>()))
                                {
                                    circularReferenceFound = true;
                                }
                            }

                            if (circularReferenceFound)
                            {
                                updatedBOE.ImportTypes.Remove(BoeImportResult.UpdateBoe);
                                updatedBOE.ImportTypes.Add(BoeImportResult.CircularReferences);
                            }
                        }
                    }

                    foreach (ImportedBoe importResult in importResults)
                    {
                        if (ws.WorkspaceState == WorkspaceState.Initialization)
                        {
                            if (importResult.State == BOEState.Approved || importResult.State == BOEState.AwaitingApproval)
                            {
                                importResult.ImportTypes.Remove(BoeImportResult.UpdateBoe);
                                importResult.ImportTypes.Remove(BoeImportResult.CreateBoe);
                                importResult.ImportTypes.Add(BoeImportResult.WorkspaceNotInWorkingState);
                            }
                        }

                        foreach (BoeImportResult resultType in importResult.ImportTypes)
                        {
                            theModelViews.Add(new ImportBoeResultsModelView()
                            {
                                BoeID = importResult.Id,
                                BOETitle = importResult.Title,
                                ImportType = (int)resultType,
                                WbsID = importResult.WBSID,
                                WbsString = importResult.WbsString,
                                ClinID = importResult.CLINID,
                                ClinString = importResult.ClinString,
                                AuthorIDs = importResult.AuthorIDs,
                                SubcontractorAuthorIDs = importResult.SubcontractorAuthorIDs,
                                ApproverIDs = importResult.ApproverIDs,
                                Status = (int)importResult.State,
                                BoeXrefID = importResult.WCBID,
                                isMaterial = importResult.isMaterial,
                                IsMultiClinWbs = importResult.IsMultiClinWbs
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                errorsOccurred = true;
            }

            dataToSave = (from m in theModelViews
                         where m.ImportType == (int)BoeImportResult.CreateBoe || m.ImportType == (int)BoeImportResult.UpdateBoe || m.ImportType == (int)BoeImportResult.DeleteBoe
                          select m).ToList();

            return theModelViews;
        }

        /// <summary>
        /// Determines if there are any conflicts when copying a BOE
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">ID of BOE being copied to</param>
        /// <param name="copyBOEID">ID of BOE being copied</param>
        /// <param name="taskElementsToCopy">Task elements being copied</param>
        /// <returns>Modelview of copy BOE conflicts</returns>
        public BOECopyConflictsModelView DisplayCopyBOEConflicts(FullWorkspace ws, int boeID, int copyBOEID, ICollection<int> taskElementsToCopy, ICollection<int> travelElementsToCopy)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            BOECopyConflictsModelView boeCopyConflictsModelView = new BOECopyConflictsModelView();
            boeCopyConflictsModelView.BoeId = boeID;
            boeCopyConflictsModelView.CopyBoeId = copyBOEID;
            boeCopyConflictsModelView.TaskElementsToCopy = taskElementsToCopy;
            boeCopyConflictsModelView.TravelElementsToCopy = travelElementsToCopy;

            FullBoe copyBoe = this.Factory.CreateFullBoe(copyBOEID);
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            FullWorkspace copyWorkspace = copyBoe.Workspace;

            ICollection<BoesWithConflicts> boesWithConflicts = this._ConflictBOE.CopyBOEConflicts(copyBoe, boe, taskElementsToCopy);

            if (boesWithConflicts.Any())
            {
                IReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables;

                foreach (BoesWithConflicts boesWithConflict in boesWithConflicts)
                {
                    BOECopyConflictModelView boeCopyConflict = new BOECopyConflictModelView();

                    if (boesWithConflict.taskElement != null)
                    {
                        boeCopyConflict.taskTitle = boesWithConflict.taskElement.BOETaskID + " - " + boesWithConflict.taskElement.TaskTitle;
                    }

                    TaskElementDetailModelView taskDetail = new TaskElementDetailModelView();
                    if (boesWithConflict.laborTypeConflicts.Any())
                    {
                        foreach (BoeTaskElementDTO taskElement in boesWithConflict.laborTypeConflicts)
                        {
                            taskDetail.TaskID = taskElement.BOETaskID;
                            taskDetail.Title = taskElement.TaskTitle;
                            boeCopyConflict.taskElementTypeConflict.Add(taskDetail);
                        }

                    }

                    foreach (WorkspaceVariableDTO workspaceVariableDTO in boesWithConflict.workspaceVariableConflicts)
                    {
						WorkspaceVariableModelView workspaceVariable = new WorkspaceVariableModelView(workspaceVariableDTO, this._variableSelectBOEtoSumCalculation, copyWorkspace);

                        decimal newValue = 0m;

						WorkspaceVariableDTO matchingWorkspaceVariable = (from w in workspaceVariables
                                                         where w.WorkspaceVariableName.Equals(workspaceVariable.WorkspaceVariableName, StringComparison.CurrentCultureIgnoreCase)
                                                         select w).FirstOrDefault();

                        if (matchingWorkspaceVariable != null)
                        {
                            if (matchingWorkspaceVariable.ValueType == VarValueType.SumOfBOEs)
                            {
                                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                                data.FillData(null, new List<WorkspaceVariableDTO>() { matchingWorkspaceVariable }, ws);

                                newValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(matchingWorkspaceVariable, data);
                            }
                            else
                            {
                                newValue = matchingWorkspaceVariable.WorkspaceVariableValue;
                            }
                        }

                        boeCopyConflict.workspaceVariableConflicts.Add(workspaceVariable, newValue);
                    }

                    foreach (WorkspaceVariableDTO workspaceVariableCR in boesWithConflict.workspaceVariableCircularReferences)
                    {
                        boeCopyConflict.workspaceVariableCircularReferences.Add(new WorkspaceVariableModelView(workspaceVariableCR, this._variableSelectBOEtoSumCalculation, ws));
                    }

                    foreach (OrdinaryVariableDto ordinaryVariableCR in boesWithConflict.ordinaryVariableCircularReferences)
                    {
                        boeCopyConflict.ordinaryVariableCircularReferences.Add(new BoeTaskOrdinaryVariableModelView(ordinaryVariableCR, this._variableSelectBOEtoSumCalculation, ws));
                    }

                    foreach (PerformingOrgDTO performingOrgDTO in boesWithConflict.performingOrgConflicts)
                    {
                        PerformingOrgModelView performingOrgModelView = new PerformingOrgModelView();
                        performingOrgModelView.PerformingOrgName = performingOrgDTO.PerformingOrgName;
                        performingOrgModelView.PerformingOrgDesc = performingOrgDTO.PerformingOrgDesc;
                        boeCopyConflict.performingOrgConflicts.Add(performingOrgModelView);
                    }

                    ResourceModelView resourceModelView = new ResourceModelView();
                    foreach (ResourceDTO resourceDTO in boesWithConflict.resourceConflicts)
                    {
                        resourceModelView.ResourceDesc = resourceDTO.ResourceDesc;
                        resourceModelView.ResourceName = resourceDTO.ResourceName;
                        boeCopyConflict.resourceConflicts.Add(resourceModelView);
                    }

                    if (!string.IsNullOrEmpty(boesWithConflict.taskIDConflict))
                    {
                        boeCopyConflict.taskIDConflict = boesWithConflict.taskIDConflict;
                    }
                    boeCopyConflictsModelView.BOECopyConflictModelViews.Add(boeCopyConflict);
                }
            }

            return boeCopyConflictsModelView;
        }

        #endregion

        /// <summary>
        /// Hits all the necessary functions to get all the data needed for a single BOE in the search results.
        /// </summary>
        /// <param name="boe">The Boe.</param>
        /// <param name="ws">The ws.</param>
        /// <returns>Fully populate BOESearchResult view model.</returns>
        private BOESearchResult GetBOESearchResult(FullBoe boe, FullWorkspace ws)
        {
			int currentWorkspaceID = ws.Id;
            WbsDTO wbs = boe.Wbs;
            string wbsNum = (wbs == null ? CommonConstants.Unassigned_WBS_Display_Text : wbs.WbsNumber);
            string wbsTitle = (wbs == null ? CommonConstants.Unassigned_WBS_Display_Text : wbs.WbsTitle);

            ClinDTO clin = boe.Clin;
            string clinNum = (clin == null ? CommonConstants.Unassigned_CLIN_Display_Text : clin.ClinNumber);
            string clinTitle = (clin == null ? CommonConstants.Unassigned_CLIN_Display_Text : clin.ClinTitle);
            
            //get the workspace of the boe. We need this to match the current workspace.
            FullWorkspace workspace = boe.Workspace;
            //lets check the workspace to make sure they are the same

            if (currentWorkspaceID != workspace.Id)
            {
                if (!workspace.AllowSearch || workspace.ContainsOCI)
                {
                    throw new InvalidDataRelationException(
                        "Searching for a BOE in a workspace that cannot be searched. BOE: " + boe.Id + " workspace: " + workspace.Id);
                }
            }

            // retrieve the authors and subcontractor authors assigned to the BOE
            Collection<UserDTO> authors = new Collection<UserDTO>((from p in this.PermissionsLoader.GetBOEPermissions(new List<int>() { boe.Id })
                                                                   where p.Role == Role.Author || p.Role == Role.SubcontractorAuthor
                                                                   select this.UserLoader.GetUserByID(p.ETIUserId)).Distinct().OrderBy(x => x.DisplayName).ToCollection());
            
            // Get candidate task elements to copy.
            Collection<TaskElementModelView> taskElementModelViews = this.GetCandidateTaskElementsToCopy(boe, ws);
            Collection<TravelElementModelView> travelElementModelViews = this.GetCandidateTravelElementsToCopy(boe);

            return new BOESearchResult()
            {
                WBSNumber = wbsNum,
                WBSTitle = wbsTitle,
                ClinNumber = clinNum,
                ClinTitle = clinTitle,
                WorkspaceName = workspace.WorkspaceName,
                WorkspaceShortName = workspace.Shortname,
                SubmittalDate = workspace.ProposalSubmittalDate,
                BOETitle = boe.Title,
                BOEDescription = boe.Description ?? string.Empty,
                BOEID = boe.Id,
                AuthorDisplayName = authors.Any() ? string.Join("; ", authors.Select(x => x.DisplayName)) : string.Empty,
                TaskElements = taskElementModelViews,
                TravelElements = travelElementModelViews
            };
        }

        /// <summary>
        /// Gets a list of task elements from a BOE that are candidates to copy to another BOE.
        /// </summary>
        /// <param name="boe">Source BOE.</param>
        /// <returns>Candidate task elements to copy.</returns>
        private Collection<TaskElementModelView> GetCandidateTaskElementsToCopy(FullBoe boe, FullWorkspace ws)
        {
            // Get candidate task elements to copy.
            Collection<TaskElementModelView> taskElementModelViews = new Collection<TaskElementModelView>();

            // Weed out any task elements that are N/A: i.e. Those in Material BOEs, others?
            if (!boe.isMaterial && boe.TaskElements != null && boe.TaskElements.Any())
            {
                IReadOnlyCollection<WorkspaceVariableDTO> allWorkspaceVariables = boe.WorkspaceVariables;

                foreach (BoeTaskElementDTO boeTaskElement in boe.TaskElements)
                {
                    string moqEquationResult = this._BoeTaskElementRecalculation.CalculateMOQHoursTotal(boeTaskElement, ws);

                    List<WorkspaceVariableDTO> inUseWorkspaceVariables = (from wID in boeTaskElement.WorkspaceVariableIDs
                                                   from workspaceVariable in allWorkspaceVariables
                                                   where workspaceVariable.Id == wID
                                                   select workspaceVariable).ToList();

                    boeTaskElement.MOQHoursEquation = Common.MOQ.Parser.UntagVariables(boeTaskElement.MOQHoursEquation, inUseWorkspaceVariables);

                    taskElementModelViews.Add(new TaskElementModelView(boeTaskElement, moqEquationResult));
                }
            }

            return taskElementModelViews;
        }

        /// <summary>
        /// Gets a list of travel elements from a BOE that are candidates to copy to another BOE.
        /// </summary>
        /// <param name="boe">Source BOE.</param>
        /// <returns>Candidate travel elements to copy.</returns>
        private Collection<TravelElementModelView> GetCandidateTravelElementsToCopy(FullBoe boe)
        {
            // Get candidate travel elements to copy.
            Collection<TravelElementModelView> travelElementModelViews = new Collection<TravelElementModelView>();

            // Weed out any travel elements that are N/A: i.e. Those in Material BOEs, others?
            if (!boe.isMaterial && boe.Travels != null && boe.Travels.Any())
            {
                foreach (TravelDTO travelElement in boe.Travels)
                {
                    travelElementModelViews.Add(new TravelElementModelView(travelElement));
                }
            }

            return travelElementModelViews;
        }

        /// <summary>
        /// Calculates the defaults for the Manage BOE page
        /// </summary>
        /// <param name="theModelView">modelview of the Manage BOE Grid</param>
        /// <param name="workspace">workspace containing Mamage BOE page</param>
        public void CalculateManageBOEDefaults(ManageBOEGridWidgetModelView theModelView, FullWorkspace workspace)
        {
            if (theModelView == null)
            {
                throw new ArgumentNullException(nameof(theModelView));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            // WBS Names
            Collection<WbsDTO> allWbsForWorkspace = workspace.WbsElementsNoMultiWbs.ToCollection<WbsDTO>();

            List<WbsDTO> availableWbs = allWbsForWorkspace.Where(x => x.inUse).ToList();
            this._nestedWbsUtilities.SetParentsAndChildrenInUse(allWbsForWorkspace);

            List<WbsDTO> otherAvailbleWBS = allWbsForWorkspace.Where(x => !x.inUse).ToList();

            availableWbs.AddRange(otherAvailbleWBS);

            // CLIN Names
            Collection<ManageCLINModelView> clinNames = new Collection<ManageCLINModelView>(
                (from c in workspace.ClinsNoMultiClin
                 orderby c.ClinNumber
                 select new ManageCLINModelView
                 {
                     ClinNumber = c.ClinNumber,
                     ClinTitle = c.ClinTitle,
                     ClinID = c.Id,
                     StartDate = c.StartDate.HasValue ? c.StartDate.Value.ToString("MM/yyyy") : workspace.ContractStartDate.ToString("MM/yyyy"),
                     EndDate = c.EndDate.HasValue ? c.EndDate.Value.ToString("MM/yyyy") : workspace.ContractEndDate.ToString("MM/yyyy")
                 }).ToList());

            Collection<PermissionsDTO> potentialBOEPermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);
            // Approver Names
            IEnumerable<int> approverIDs = (from p in potentialBOEPermissions
                               where p.Role == Role.Approver
                               select p.ETIUserId).Distinct();
            Collection<SelectListItem> approvers = this.CreateUserSelectList(approverIDs.ToCollection(), false);

            // Author Names
            IEnumerable<int> authorIDs = (from p in potentialBOEPermissions
                             where p.Role == Role.Author
                             select p.ETIUserId).Distinct();
            Collection<SelectListItem> authors = this.CreateUserSelectList(authorIDs.ToCollection(), false);

            // Subcontractor Author Names
            IEnumerable<int> subcontractorAuthorIDs = (from p in potentialBOEPermissions
                                          where p.Role == Role.SubcontractorAuthor
                                          select p.ETIUserId).Distinct();
            Collection<SelectListItem> subcontractorAuthors = this.CreateUserSelectList(subcontractorAuthorIDs.ToCollection(), true);

            theModelView.DefaultStartDate = workspace.ContractStartDate.ToString("MM/yyyy");
            theModelView.DefaultEndDate = workspace.ContractEndDate.ToString("MM/yyyy");

            theModelView.PotentialWbs = new Collection<SelectListItem>((from x in availableWbs
                                                                        orderby x.WbsPaddedNumber
                                                                        select new SelectListItem()
                                                                        {
                                                                            Text = x.WbsString,
                                                                            Value = x.Id.ToString()
                                                                        }).ToList());

            theModelView.PotentialWbs.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });



            //Lets get the Multi WBS for this workspace. 
            theModelView.MultiWBSId = workspace.MultiBOEWbs.Id;
            theModelView.MultiWBSName = workspace.MultiBOEWbs.WbsString;


            //Lets get the Multi Clin for this workspace. 
            theModelView.MultiClin = ( new ManageCLINModelView(workspace.MultiBOEClin, null));

            theModelView.PotentialClins = clinNames;
            theModelView.PotentialAuthors = new Collection<SelectListItem>(authors.OrderBy(x => x.Text).ToArray());
            theModelView.PotentialSubcontractorAuthors = new Collection<SelectListItem>(subcontractorAuthors.OrderBy(x => x.Text).ToArray());
            theModelView.PotentialApprovers = new Collection<SelectListItem>(approvers.OrderBy(x => x.Text).ToArray());

            theModelView.BoeResults = this.GetManageBOEGridData(workspace, workspace.Boes);
        }

		/// <summary>
		/// Saves a BOE(s) from the Manage BOE page.  There are also many side affects that occur with this save.
		/// </summary>
		/// <param name="boes">List of Boes to be saved</param>
		/// <param name="modelState">Model state</param>
		/// <param name="ws">Full Workspace</param>
		/// <returns>ManageBOEModelView</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ManageBOEModelView SaveManageBOE(Collection<ManageBOEModelView> boes, ModelStateDictionary modelState,FullWorkspace ws)
		{
			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}
			if (modelState == null)
			{
				throw new ArgumentNullException(nameof(modelState));
			}
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

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

			if (!modelState.IsValid && boes.Count(b => b.Deleted) == 0)
			{
				ValidationErrors = Utilities.CreateModelStateValidationErrorList(modelState);
			}
			else
			{
				// create a dictionary of all the BOEs so we can reference them in the loop rather than getting one at a time.
				// it's ok if these elements are modified later in the logic
				IDictionary<int, FullBoe> workspaceBoeDictionary = ws.Boes.ToDictionary(x => x.Id);

				Collection<int> allMVboeIds = (from mv in boes
											   select mv.BoeID).Distinct().ToCollection();

				// get all approver responses at one time
				ICollection<BoeApproverResponseDTO> allApproverResponses = this.boeApproverResponseLoader.GetByBoeIds(allMVboeIds);

				// check state validation before worrying about committing to the database but only if NOT a delete
				if (!requestIsADelete)
				{
					ValidateSaveManageBOE(boes, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);
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
						setDefaultBoeDates(ws, originalBoe, boeMV.ClinID);
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
			//JsonResult toReturn;

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
						RemoveMultiBOEReferenceWorkspaceVar(ws, workspaceVariablesEffectedByMulti, BoeIdsEffectedByMulti);
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
						this.boeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));
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
					DeleteMoqTypesForBoe(ws, BoesToBeDeleted.Select(x => x.Id).ToList());
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

							this.boeApproverResponseLoader.Save(new Collection<BoeApproverResponseDTO>(boeApproversToSave));

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

						AdjustStateOfTaskElements(ws, boeTaskElementsToRecalculate.ToCollection());
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

					Collection<BoeTaskElementDTO> taskEffectedByMutliBOE = new Collection<BoeTaskElementDTO>(RemoveMultiBOEReferenceTaskVar(ws, BoeIdsEffectedByMulti));
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

						this.BoeLaborControllerLogic.CalculateLinkedTaskElements(ValidationErrors, taskElements, new Collection<BoeTaskElementDTO>(), ws);
					}

					if (boeTaskElementsToRecalculate.Any())
					{
						AdjustStateOfTaskElements(ws, boeTaskElementsToRecalculate.ToCollection());
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

				if (singleEditMV != null)
				{
					// Get the boe
					int boeToGetID = boeSaveIDDict != null && boeSaveIDDict.ContainsKey(singleEditMV.BoeID) ? boeSaveIDDict[singleEditMV.BoeID] : singleEditMV.BoeID;

					FullBoe boe = this.Factory.CreateFullBoe(boeToGetID);

					// populate the MV
					singleEditMV = GetManageBOEGridData(ws, new List<FullBoe>() { boe }).First();
				}

				return singleEditMV;

				#endregion
			}
			else
			{
				throw new GenValidationException(ValidationErrors);
			}

		}

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="BOEAdvancedSearchModelView"/> to populate</param>
        public virtual void PopulateCompanySpecificProperties(BOEAdvancedSearchModelView theModel)
        {
            if (theModel != null)
            {
                theModel.LabelLeadPricer = CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC;
                theModel.ShowRFP = true;
            }
        }

        /// <summary>
        /// Gets company specific header information for the manage Boe page.
        /// </summary>
        /// <returns>Header Info for Manage Boe</returns>
        public virtual string GetCompanySpecificManageBoeHeaderInfo
        {
            get
            {
                return CommonConstants.MANAGE_BOE_HEADER_ISGS;
            }
        }

        /// <summary>
        /// Updates the OrderList for Taskelements in a BOE
        /// </summary>
        /// <param name="ws">Full workspace</param>
        /// <param name="boeObject">Full BOE</param>
        /// <param name="theModelView">Collection of TaskElementReOrderingCollection </param>
        public virtual void ReOrderTaskElementOrder(FullWorkspace ws, FullBoe boeObject, TaskElementOrderCollection theModelView)
        {
            if (boeObject == null)
            {
                throw new ArgumentNullException(nameof(boeObject));
            }

            //Update the Order of the taskelements. We want to update each one thats being shown to the user.

            boeObject.TaskElements.Select(w => { w.BOETaskElementOrder = (theModelView.BOETaskElements.First(t => t.TaskID == w.Id)).ListOrder; return w; }).ToCollection();
            //Update the updatetype for all the taskelements . the bulk save needs them all updated.
            boeObject.TaskElements.Select(x => { x.Updateable = UpdateType.Upsert; return x; }).ToCollection();
            //save
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(boeObject.TaskElements.ToList(), ws);
                scope.Complete();
            }
        }

        /// <summary>
        /// Validates the BOE from SaveManageBOE
        /// </summary>
        /// <param name="boes">Collection of BOES that are being saved</param>
        /// <param name="ws">full workspace for the boes</param>
        /// <param name="boePermissions">All the Permisssions</param>
        /// <param name="BoeStateDictionary">BOE States</param>
        /// <param name="ValidationErrors">Collection Of Error Messages to add too</param>
        /// <param name="workspaceBoeDictionary"></param>
        public void ValidateSaveManageBOE(Collection<ManageBOEModelView> boes, FullWorkspace ws, ICollection<PermissionsDTO> boePermissions, Dictionary<int, BOEState> BoeStateDictionary, Collection<ValidationMessage> ValidationErrors, IDictionary<int, FullBoe> workspaceBoeDictionary)
        {

            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (boePermissions == null)
            {
                throw new ArgumentNullException(nameof(boePermissions));
            }
            if (ValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(ValidationErrors));
            }
            if (BoeStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(BoeStateDictionary));
            }
            if (workspaceBoeDictionary == null)
            {
                throw new ArgumentNullException(nameof(workspaceBoeDictionary));
            }

            //get the multi clin and wbs, will use for validation.
            ClinDTO multiClin = ws.MultiBOEClin;
            FullWbs multiWbs = this.Factory.CreateFullWbs(ws.MultiBOEWbs);

            #region Validation Logic for ManageBOEModelView
            foreach (ManageBOEModelView boeMVStates in boes)
            {
                bool isNewBOE = boeMVStates.BoeID < 0;
                #region Gather Data
                // Gather Data
                ICollection<int> originalBOEApprovers = (from x in boePermissions
                                                         where x.BOEId == boeMVStates.BoeID && x.Role == Role.Approver
                                                         select x.ETIUserId).ToList();
                #endregion

                FullBoe originalBoe = ws.Boes.FirstOrDefault(x => x.Id == boeMVStates.BoeID);

                // Validate chosen WBS for circular references but only if the WBS has changed
                #region Validate chosen WBS for circular references
                if (boeMVStates.WbsID.HasValue && boeMVStates.WbsID.Value > 0 && !(isNewBOE) && originalBoe != null)
                {
                    WbsDTO originalWbs = ws.WbsElements.FirstOrDefault(x => x.Id == originalBoe.WBSID);

                    if (originalWbs != null && originalWbs.Id != boeMVStates.WbsID.Value)
                    {
                        // Validate WBS Unique Number and no circular references
                        Collection<Dictionary<string, object>> validationData = new Collection<Dictionary<string, object>>();
                        validationData.Add(new Dictionary<string, object>() {
                                    { "Boe", ws.Boes.FirstOrDefault(x => x.Id == boeMVStates.BoeID)  },
                                    { "Workspace", ws}
                        });

                        Collection<string> validatorResponse = ValidationFactory.Instance.getValidator(ValidationType.BOEWBSMove).validation(ws.WbsElements.FirstOrDefault(x => x.Id == boeMVStates.WbsID.Value), validationData);
                        if (validatorResponse.Any())
                        {
                            foreach (string message in validatorResponse)
                            {
                                ValidationErrors.Add(new ValidationMessage("WBS", message));
                            }
                        }
                    }
                }
                #endregion

                // Validate chosen CLIN for circular references but only if the CLIN has changed
                #region Validate chosen CLIN for circular references
                if (boeMVStates.ClinID.HasValue && boeMVStates.ClinID > 0 && !(isNewBOE) && originalBoe != null)
                {
                    // grab the original clin
                   
                    ClinDTO originalClin = ws.Clins.FirstOrDefault(x => x.Id == originalBoe.CLINID);

                    if(originalClin !=null && originalClin.Id != boeMVStates.ClinID.Value)
                    {
                        // Validate Clin Unique Number and no circular references
                        Collection<Dictionary<string, object>> validationData = new Collection<Dictionary<string, object>>();
                        validationData.Add(new Dictionary<string, object>() {
                                { "Boe", ws.Boes.FirstOrDefault(x => x.Id == boeMVStates.BoeID) },
                                { "Workspace", ws}
                        });

                        Collection<string> validatorResponse = ValidationFactory.Instance.getValidator(ValidationType.BOECLINMove).validation(this.Factory.CreateFullClin(ws.Clins.FirstOrDefault(x => x.Id == boeMVStates.ClinID.Value)), validationData);
                        if (validatorResponse.Any())
                        {
                            foreach (string message in validatorResponse)
                            {
                                ValidationErrors.Add(new ValidationMessage("CLIN", message));
                            }
                        }
                    }
                }
                #endregion

                // validate if the BOE is valid material BOE
                #region Validate Material
                if (boeMVStates.isMaterial)
                {
                    // verify that there are no authors that have the SubcontractorAuthor role (subs may not author a Material BOE)
                    if (boeMVStates.SubcontractorAuthors.Any())
                    {
                        ValidationErrors.Add(new ValidationMessage("Subcontractor users are restricted from being assigned to Material BOEs."));
                    }

                    // validate if BOE exists for the same WBS and CLIN
                    // TODO: Matt K - Add correct Validation to make sure the current BOE is not converting an existing BOE to a Material BOE with the same Clin and WBS/adding a new boe that has a clin and wbs as 
                    // an existing material BOE
                    // 1 material boe for wbs/clin pair.

                    // Material BOEs require a WBS
                    // Blank WBS has ID -1, but including null just in case
                    if(boeMVStates.WbsID == -1 || boeMVStates.WbsID == null)
                    {
                        ValidationErrors.Add(new ValidationMessage("Material BOEs require a WBS."));
                    }

                    Collection<Dictionary<string, string>> validationData2 = new Collection<Dictionary<string, string>>();
                    validationData2.Add(new Dictionary<string, string>() { { "BoeID", boeMVStates.BoeID.ToString() }, { "Material", boeMVStates.isMaterial ? "on" : "false" } });

                    Collection<string> validatorResonse2 = ValidationFactory.Instance.getValidator(ValidationType.BoeLaborCostElementExists).validation(boeMVStates.BoeID.ToString(), validationData2);
                    if (validatorResonse2.Any())
                    {
                        foreach (string message in validatorResonse2)
                        {
                            ValidationErrors.Add(new ValidationMessage("Material", message));
                        }
                    }
                }
                else
                {
                    // need to check if this BOE wasn't update to a non-material BOE if it already contains material
                    if (boeMVStates.BoeID > 0 && boeMVStates.Deleted != true)
                    {
                        Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>>();
                        validationData.Add(new Dictionary<string, string>() { { "BoeID", boeMVStates.BoeID.ToString() }, { "Material", boeMVStates.isMaterial ? "on" : "false" } });


                        Collection<string> validatorResponse = ValidationFactory.Instance.getValidator(ValidationType.BoeMaterialElementExists).validation(boeMVStates.BoeID.ToString(), validationData);
                        if (validatorResponse.Any())
                        {
                            foreach (string message in validatorResponse)
                            {
                                ValidationErrors.Add(new ValidationMessage("Material", message));
                            }
                        }
                    }
                }
                #endregion 
                // validate if the BOE is valid Multi BOE
                #region Validate Multi BOE

                ValidateMultiBOE(ValidationErrors, multiClin, multiWbs, boeMVStates);

                #endregion
                //Check to verify if authors is assigned that an approver is also assigned.
                #region Validate Authors Approvers
                ValidateAuthorApproverAssigned(ValidationErrors, workspaceBoeDictionary, boeMVStates, isNewBOE, originalBOEApprovers);
                #endregion
                Collection<PermissionsDTO> workspacePermissions = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);
                // verify that approver cannot be the same as author unless approver is an a-account
                #region validate approvers

                this.ValidateApproverIsNotAuthor(ValidationErrors, boeMVStates, workspacePermissions);
                #endregion
                // no point in checking state if it's a new BOE
                if (!(isNewBOE))
                {
                    #region
                    FullBoe originalBOE = workspaceBoeDictionary[boeMVStates.BoeID];
                    this.ValidateBOEState(ws, BoeStateDictionary, ValidationErrors, boeMVStates, originalBOEApprovers, originalBOE);
                    #endregion
                }
            }

            #endregion
        }

        /// <summary>
        /// Validates if a BOE can be a Multi BOE
        /// </summary>
        /// <param name="ValidationErrors">collection of error messages</param>
        /// <param name="multiClin">Multi BOE clin</param>
        /// <param name="multiWbs">Multi BOE Wbs</param>
        /// <param name="boeMVStates">current BOE state</param>
        private static void ValidateMultiBOE(Collection<ValidationMessage> ValidationErrors, ClinDTO multiClin, FullWbs multiWbs, ManageBOEModelView boeMVStates)
        {
            if (boeMVStates.IsMultiClinWbs)
            {
                //can not be a multiboe and a material boe
                if (boeMVStates.isMaterial)
                {
                    ValidationErrors.Add(new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_MATERIAL_BOE));
                }
                if (boeMVStates.ClinID != multiClin.Id)
                {
                    ValidationErrors.Add(new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_NEEDS_CLIN));
                }
                if (boeMVStates.WbsID != multiWbs.Id)
                {
                    ValidationErrors.Add(new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_NEEDS_WBS));
                }
            }
            else
            {

                if (boeMVStates.ClinID.HasValue && boeMVStates.ClinID == multiClin.Id)
                {
                    ValidationErrors.Add(new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_CLIN_ASSIGNED));
                }
                if (boeMVStates.WbsID.HasValue && boeMVStates.WbsID == multiWbs.Id)
                {
                    ValidationErrors.Add(new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_WBS_ASSIGNED));
                }
            }
        }

        /// <summary>
        /// Makes sure the author and approver are assigned to the boe
        /// </summary>
        /// <param name="ValidationErrors">collection of validation errors</param>
        /// <param name="workspaceBoeDictionary">a dictionary of boes</param>
        /// <param name="boeMVStates">the boe state</param>
        /// <param name="isNewBOE">new boe bool</param>
        /// <param name="originalBOEApprovers">orginal approvers. </param>
        private static void ValidateAuthorApproverAssigned(Collection<ValidationMessage> ValidationErrors, IDictionary<int, FullBoe> workspaceBoeDictionary, ManageBOEModelView boeMVStates, bool isNewBOE, ICollection<int> originalBOEApprovers)
        {
            if (boeMVStates.Authors.Any() || boeMVStates.SubcontractorAuthors.Any())
            {
                if (!(isNewBOE))
                {
                    FullBoe originalBOE = workspaceBoeDictionary[boeMVStates.BoeID];

                    // if BOE is in Approved state, the approvers on the FE are disabled so they don't get past back in the save
                    if (originalBOE.State.Equals(BOEState.Approved))
                    {
                        boeMVStates.Approvers = originalBOEApprovers.ToCollection();
                    }

                }
                if (boeMVStates.Approvers == null || boeMVStates.Approvers.Count == 0)
                {
                    ValidationErrors.Add(new ValidationMessage("Approver", "Author assigned, however no approver was assigned."));
                }
            }
            else // an author (or subcontractor author) is required if the boe has been created/updated before or if an approver exists
            {
                if (boeMVStates.Approvers.Any())
                {
                    ValidationErrors.Add(new ValidationMessage("Author", "An Author or Subcontractor Author is required."));
                }
            }
        }
        /// <summary>
        /// verify that approver cannot be the same as author unless approver is an a-account
        /// </summary>
        /// <param name="ValidationErrors">collection of messages to show user</param>
        /// <param name="boeMVStates">the boe</param>
        /// <param name="workspacePermissions">workspace permissions</param>
        private void ValidateApproverIsNotAuthor(Collection<ValidationMessage> ValidationErrors, ManageBOEModelView boeMVStates, Collection<PermissionsDTO> workspacePermissions)
        {
            // build a combined list of Author IDs and Subcontractor Author IDs
            ICollection<int> allAuthors = boeMVStates.Authors.Union<int>(boeMVStates.SubcontractorAuthors).ToList();
            bool authorCheck = false;     // flag used below to track if there was 1 occurrence of an attempt to assign and Author and an Approver
            bool approverCheck = false;   // flag used below to track if there was 1 occurrence of an assigned Approver not having the Approver role

            foreach (int approver in boeMVStates.Approvers)
            {
                UserDTO userDTO = this.UserLoader.GetUserByID(approver);
                // verify that the current approver is NOT in the combined list of Authors and Subcontractor Authors

                if (allAuthors.Contains(approver) && !authorCheck)
                {
                    authorCheck = true;
                    ValidationErrors.Add(new ValidationMessage("Author", "Approver cannot be the same person as author"));
                }

                // verify that approvers have the appropriate role - bug 8698
                IEnumerable<int> approverIDs = (from p in workspacePermissions
                                   where p.Role == Role.Approver &&
                                   p.ETIUserId == userDTO.UserID
                                   select p.ETIUserId).Distinct();

                if (!approverIDs.Any() && !approverCheck) // if this is true, then the user is not in the list of approvers
                {
                    // need check to see if the user has approver role via a group
                    List<int> approverIds = workspacePermissions.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).Distinct().ToList();
                    ICollection<UserDTO> users = this.UserLoader.GetByIds(approverIds);

                    List<UserDTO> tempUsers = new List<UserDTO>();
                    foreach (UserDTO user in users)
                    {
                        if (user.NTID.Contains('.')) // AD group name
                        {
                            ICollection<UserData> members = this._ADUtils.GetAdGroupUsers(user.DisplayName);

                            ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
                            List<int> userIds = new List<int>();

                            foreach (UserData member in orderedMembers)
                            {
                                int userId;
                                bool userExists = this.UserLoader.UserExists(member.Ntid, out userId);

                                if (userExists)
                                {
                                    userIds.Add(userId);
                                }
                            }

                            tempUsers.AddRange(this.UserLoader.GetByIds(userIds));
                        }
                    }

                    if (tempUsers.All(x => x.UserID != userDTO.UserID)) // if there isn't a match via groups -> throw an error
                    {
                        approverCheck = true;
                        ValidationErrors.Add(new ValidationMessage("Approver", "Approver must have the Approver role"));
                    }
                }
            }
        }

        /// <summary>
        /// checks the boe state
        /// </summary>
        /// <param name="ws">full workspace of a boe</param>
        /// <param name="BoeStateDictionary">BOE state dictionary </param>
        /// <param name="ValidationErrors">a collection of error messages to add to</param>
        /// <param name="boeMVStates">current boe state</param>
        /// <param name="originalBOEApprovers">the orginal approvers</param>
        /// <param name="originalBOE">if the boe exist - this is the object</param>
        private void ValidateBOEState(FullWorkspace ws, Dictionary<int, BOEState> BoeStateDictionary, Collection<ValidationMessage> ValidationErrors, ManageBOEModelView boeMVStates, ICollection<int> originalBOEApprovers, FullBoe originalBOE)
        {
            DataRelationshipVerifier.VerifyDataRelation(originalBOE, ws.Id);

            // verfiy if author & approver were assigned, BOE is not saved without an author and approver
            if (originalBOE.State != BOEState.Unassigned && originalBOE.State != BOEState.None)
            {
                if ((boeMVStates.Authors.Count == 0 && boeMVStates.SubcontractorAuthors.Count == 0) && boeMVStates.Approvers.Count == 0)
                {
                    ValidationErrors.Add(new ValidationMessage("Author", "Author and Approver cannot be removed once they are assigned to a BOE. Please select an Author and Approver(s)."));
                }
            }

            // Check if the author was changed, and if a change was allowed
            if ((!Enumerable.SequenceEqual(originalBOE.AuthorIDs, boeMVStates.Authors)) &&
                boeMVStates.State != BOEState.Draft && boeMVStates.State != BOEState.Unassigned)
            {
                ValidationErrors.Add(new ValidationMessage("Author", "Unable to reassign author since BOE is not in DRAFT or UNASSIGNED state."));
            }

            // Check if the approver was changed, and if a change was allowed
            if ((!Enumerable.SequenceEqual(originalBOEApprovers, boeMVStates.Approvers)) &&
                boeMVStates.State != BOEState.Draft && boeMVStates.State != BOEState.AwaitingApproval && boeMVStates.State != BOEState.Unassigned)
            {
                ValidationErrors.Add(new ValidationMessage("Approver", "Unable to reassign approver since BOE is not in DRAFT, AWAITING APPROVAL, or UNASSIGNED state."));
            }

            if ((!Enumerable.SequenceEqual(originalBOE.SubcontractorAuthorIDs, boeMVStates.SubcontractorAuthors)) &&
                boeMVStates.State != BOEState.Draft && boeMVStates.State != BOEState.Unassigned)
            {
                ValidationErrors.Add(new ValidationMessage("Subcontractor Author", "Unable to reassign subcontractor author since BOE is not in DRAFT or UNASSIGNED state."));
            }

            string errorMessage;
            if (!this._boeStateMachine.PerformStateTransitionValidation(originalBOE, ws, originalBOE.State, boeMVStates.State, out errorMessage))
            {
                // not valid ... communicate to user

                ValidationErrors.Add(new ValidationMessage("State", errorMessage));
            }
            else // add to boe state dictionary
            {
                BoeStateDictionary[boeMVStates.BoeID] = originalBOE.State;
            }
        }

        /// <summary>
        /// Updates the workspace variables by removing multiboe references 
        /// </summary>
        /// <param name="ws">full workspace</param>
        /// <param name="workspaceVariablesAffectedByMulti">workspace vars that use boe</param>
        /// <param name="MultiBOEIDs">MultiBOE ID's</param>
        public void RemoveMultiBOEReferenceWorkspaceVar(FullWorkspace ws, IList<WorkspaceVariableDTO> workspaceVariablesAffectedByMulti, Collection<int> MultiBOEIDs)
        {
            // Note: This method modifies Read-Only Collections, but it is always called with a non-cached version of FullWorkspace

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (workspaceVariablesAffectedByMulti == null)
            {
                throw new ArgumentNullException(nameof(workspaceVariablesAffectedByMulti));
            }
           
            ICollection<WorkspaceVariableDTO> workspaceVariables = (from v in ws.WorkspaceVariables
                                                                    from b in v.SelectedBOEsToSum
                                                                    where b.BoeID.HasValue && MultiBOEIDs.Contains(b.BoeID.Value)
                                                                    select v).ToCollection();

            Collection<WorkspaceVariableDTO> toSaveWorkspaceVar = new Collection<WorkspaceVariableDTO>();
            foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
            {
                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);
                foreach (int boeID in MultiBOEIDs)
                {
                    workspaceVar.SelectedBOEsToSum.Remove(workspaceVar.SelectedBOEsToSum.FirstOrDefault(b => b.BoeID == boeID));
                }
                workspaceVar.Updateable = UpdateType.Upsert;
                workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                workspaceVariablesAffectedByMulti.Add(workspaceVar);
                toSaveWorkspaceVar.Add(workspaceVar);
            }
            this._workspaceVariableLoader.SaveWorkspaceVariables(toSaveWorkspaceVar);

            ws.RefreshWorkspaceVariables();
        }

        /// <summary>
        /// Removes boes from a task variable that are now multiboe
        /// </summary>
        /// <param name="ws">full boe</param>
        /// <param name="multiBOEIDs">multi boe ids</param>
        /// <returns>returns a collection of modified taskelements with removed sumofboes that are multiboe</returns>
        public Collection<BoeTaskElementDTO> RemoveMultiBOEReferenceTaskVar(FullWorkspace ws, ICollection<int> multiBOEIDs)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }

            List<OrdinaryVariableDto> taskVars = new List<OrdinaryVariableDto>();
            foreach (int boeID in multiBOEIDs)
            {
                taskVars.AddRange(FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(boeID, ws));
            }

            List<BoeTaskElementDTO> taskElementsToBeUpdated = new List<BoeTaskElementDTO>();
            foreach (OrdinaryVariableDto taskVar in taskVars)
            {
                taskElementsToBeUpdated.Add(ws.TaskElements.FirstOrDefault(x => taskVar.TaskElementId == x.Id));
            }

            foreach (BoeTaskElementDTO taskElement in taskElementsToBeUpdated)
            {
                foreach (OrdinaryVariableDto taskVar in taskElement.OrdinaryVariables)
                {
                    taskVar.SelectedBOEsToSum = taskVar.SelectedBOEsToSum.Where(x => !(multiBOEIDs.Contains(x.BoeID.Value))).ToCollection();
                }
            }

            return new Collection<BoeTaskElementDTO>(taskElementsToBeUpdated);
        }

        /// <summary>
        /// Gets the Boe offload data.
        /// </summary>
        /// <param name="ws">The full workspace.</param>
        /// <param name="boeId">The boe identifier.</param>
        /// <returns>A Model View housing data for a Boe Offload</returns>
        public virtual BoeOffloadModelView RetrieveBoeOffloadData(FullWorkspace ws, int boeId)
        {
            // nothing to do here
            return null;
        }

        /// <summary>
        /// Calculates the total cost of travel for all MSTTravelTrips for the BOE.  Overridden by the BOEControllerLogicMST.cs class.
        /// </summary>
        /// <param name="travels">The travel elements.</param>
        /// <param name="decimalPrecision">The decimal precision for the workspace.</param>
        /// <param name="escalationRates">The escalation rates.</param>
        /// <param name="fees">The fees.</param>
        /// <returns>0 for SSC</returns>
        internal virtual decimal CalculateTotalCostTravel(ICollection<TravelDTO> travels, int decimalPrecision, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates, Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
        {
            return 0;  // Returns zero by default for SpaceSystems.  Method is overridden for RMS to calculate this appropriately.
        }

        /// <summary>
        /// Saves the BOE states.
        /// </summary>
        /// <param name="boeStates">The BOE states.</param>
        /// <param name="ws">The ws.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>The updated date as a long</returns>
        public long? SaveBOEStates(IDictionary<int, BOEState> boeStates, FullWorkspace ws, IList<string> errorMessages)
        {
            if (errorMessages == null)
            {
                throw new ArgumentNullException(nameof(errorMessages));
            }
            
            long? updateDateLong = null;
            if (boeStates != null)
            {
                foreach (KeyValuePair<int, BOEState> boeStateChange in boeStates)
                {
                    int boeId = boeStateChange.Key;
                    FullBoe boe = this.Factory.CreateFullBoe(boeId);
                    BOEState oldBOEState = boe.State;
                    BOEState newBOEState = boeStateChange.Value;

                    string validationMessage;
                    if (this._boeStateMachine.PerformStateTransitionValidation(boe, ws, boe.State, newBOEState, out validationMessage))
                    {
                        boe.Updateable = UpdateType.Upsert;
                        boe.State = newBOEState;

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                        {
                            this._BoeMediator.MediatedSave(ws, boe);

                            scope.Complete();
                        }

                        updateDateLong = boe.UpdateDate.Ticks;  // send the refreshed (DB) update-date to the client

                        this._boeStateMachine.PerformStateTransitionAction(boe, ws, oldBOEState, newBOEState);
                    }
                    else  // not a valid state transition
                    {
                        errorMessages.Add(validationMessage);
                    }
                }
            }

            return updateDateLong;
        }

        /// <summary>
        /// Save Bulk Boe Roles. This is no longer a kill and fill, only saving BOEs roles from a workspace where the ids match the boeRolesToSave
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boeRolesToSave">Boe Roles to Save</param>
        /// <returns>Error messages, if any</returns>
        public IList<string> SaveBoeBulkRoles(FullWorkspace ws, ICollection<ManageBOEModelView> boeRolesToSave)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = boeRolesToSave ?? throw new ArgumentNullException(nameof(boeRolesToSave));

            IList<string> errorMessages = this.ValidateBoeBulkRoles(ws, boeRolesToSave);
            if (errorMessages.Any()) { return errorMessages; }

			// Get existing data
			ICollection<int> boeIdsToSave = boeRolesToSave.Select(b => b.BoeID).ToList();
            ICollection<FullBoe> originalBoes = ws.Boes.Where(boe => boeIdsToSave.Contains(boe.Id) && (boe.State == BOEState.Draft || boe.State == BOEState.Unassigned || boe.State == BOEState.None)).ToList();
            ICollection<BoeDTO> boesToSave = originalBoes.Select(boe => boe as BoeDTO).ToList().DeepClone();
            ICollection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(originalBoes.Select(x => x.Id).ToList());
            ICollection<UserDTO> wsUsers = this.UserLoader.GetByIds(boePermissions.Select(x => x.ETIUserId).Distinct().ToList());
            ICollection<BoeApproverResponseDTO> approverResponses = this.boeApproverResponseLoader.GetByWorkspaceId(ws.Id).SelectMany(x => x.Value).ToList();

            // Process changes
            Dictionary<int, Collection<UserDTO>> authorsChangeDictionary = this.ProcessAuthorsForBulkRoleSave(boeRolesToSave, boesToSave, wsUsers);
            Dictionary<int, Collection<UserDTO>> approversChangeDictionary = this.GetApproverChangesForBulkRoleSave(boeRolesToSave, boesToSave, boePermissions, wsUsers, ws.CurrentActiveUser.UserID, approverResponses, out ICollection<BoeApproverResponseDTO> approversToSave);

            List<(int BoeId, BOEState OldState, BOEState NewState)> transitionsToPerform = this.GetBoeTransitionsForBulkRoleSave(originalBoes, boesToSave, approverResponses);

            this.DoBulkBoeRoleSave(ws, boesToSave, originalBoes, approversToSave, transitionsToPerform);

            this.SendEmailsAfterBulkRoleSave(ws, boesToSave, originalBoes, authorsChangeDictionary, approversChangeDictionary);

            return errorMessages;
        }

        /// <summary>
        /// Validate Bulk Boe Roles
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boeRolesToSave">Boe Roles to Save</param>
        /// <returns>Error messages, if any</returns>
        public IList<string> ValidateBoeBulkRoles(FullWorkspace ws, ICollection<ManageBOEModelView> boeRolesToSave)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = boeRolesToSave ?? throw new ArgumentNullException(nameof(boeRolesToSave));

            IList<string> errorMessages = new List<string>();

            if (boeRolesToSave.Any(x => !x.Deleted && !x.Approvers.Any() && (x.Authors.Any() || x.SubcontractorAuthors.Any())))
            {
                errorMessages.Add("All BOEs that have an author / subcontractor author assigned must also have an approver assigned as well.");
            }

            if (boeRolesToSave.Any(x => !x.Deleted && x.Approvers.Any() && !x.Authors.Any() && !x.SubcontractorAuthors.Any()))
            {
                errorMessages.Add("All BOEs that have an approver assigned must also have an author / subcontractor author assigned as well.");
            }

            if (boeRolesToSave.Any(x => !x.Deleted && x.State >= BOEState.Draft && (!x.Approvers.Any() || !(x.Authors.Any() || x.SubcontractorAuthors.Any()))))
            {
                errorMessages.Add("All BOEs in draft, awaiting approval, or approved, must have approver and author roles assigned.");
            }

            if (boeRolesToSave.Any(x => x.Authors.Intersect(x.Approvers).Any()))
            {
                errorMessages.Add("The same person cannot be assigned as both Author and Approver to the same BOE.");
            }

            return errorMessages;
        }

        /// <summary>
        /// Process Author changes, update BOEs.
        /// </summary>
        /// <param name="boeRolesToSave">Boe Roles to save</param>
        /// <param name="boesToSave">Boes To Save</param>
        /// <param name="wsUsers">WS Users</param>
        private Dictionary<int, Collection<UserDTO>> ProcessAuthorsForBulkRoleSave(ICollection<ManageBOEModelView> boeRolesToSave, ICollection<BoeDTO> boesToSave, ICollection<UserDTO> wsUsers)
        {
            Dictionary<int, Collection<UserDTO>> authorsChangeDictionary = new Dictionary<int, Collection<UserDTO>>();

            Collection<int> authorsChangedIds;
            ManageBOEModelView rolesBeingSavedForBoe;

            foreach (BoeDTO boe in boesToSave)
            {
                boe.AuthorIDs = boe.AuthorIDs ?? new Collection<int>();
                boe.SubcontractorAuthorIDs = boe.SubcontractorAuthorIDs ?? new Collection<int>();
                boe.Updateable = UpdateType.Upsert;

                authorsChangedIds = null;
                rolesBeingSavedForBoe = boeRolesToSave.FirstOrDefault(x => x.BoeID == boe.Id);

                if (rolesBeingSavedForBoe != null)
                {
                    authorsChangedIds = rolesBeingSavedForBoe.Authors.Where(x => !boe.AuthorIDs.Contains(x)).ToCollection();
                    authorsChangedIds.AddRange(rolesBeingSavedForBoe.SubcontractorAuthors.Where(x => !boe.SubcontractorAuthorIDs.Contains(x)).ToCollection());
                    authorsChangedIds.AddRange(boe.AuthorIDs.Where(x => !rolesBeingSavedForBoe.Authors.Contains(x)).ToCollection());
                    authorsChangedIds.AddRange(boe.SubcontractorAuthorIDs.Where(x => !rolesBeingSavedForBoe.SubcontractorAuthors.Contains(x)).ToCollection());

                    boe.AuthorIDs = rolesBeingSavedForBoe.Authors.Any() ? rolesBeingSavedForBoe.Authors : null;
                    boe.SubcontractorAuthorIDs = rolesBeingSavedForBoe.SubcontractorAuthors.Any() ? rolesBeingSavedForBoe.SubcontractorAuthors : null;
                }
                else
                {
                    authorsChangedIds = boe.AuthorIDs;
                    authorsChangedIds.AddRange(boe.SubcontractorAuthorIDs);

                    boe.AuthorIDs = null;
                    boe.SubcontractorAuthorIDs = null;
                }

                authorsChangeDictionary.Add(boe.Id, wsUsers.Where(x => authorsChangedIds.Contains(x.UserID)).ToCollection());
            }

            return authorsChangeDictionary;
        }

        /// <summary>
        /// Process Approver changes
        /// </summary>
        /// <param name="boeRolesToSave">Boe Roles to save</param>
        /// <param name="boesToSave">Boes To Save</param>
        /// <param name="wsPermissions">WS Permissions</param>
        /// <param name="wsUsers">WS Users</param>
        /// <param name="currentUserId">Current User Id</param>
        /// <param name="approverResponses">Current Approver Responses</param>
        /// <param name="approversToSave">Approvers To Save</param>
        private Dictionary<int, Collection<UserDTO>> GetApproverChangesForBulkRoleSave(ICollection<ManageBOEModelView> boeRolesToSave, ICollection<BoeDTO> boesToSave, ICollection<PermissionsDTO> wsPermissions, ICollection<UserDTO> wsUsers,
            int currentUserId, ICollection<BoeApproverResponseDTO> approverResponses, out ICollection<BoeApproverResponseDTO> approversToSave)
        {
            Dictionary<int, Collection<UserDTO>> approversChangeDictionary = new Dictionary<int, Collection<UserDTO>>();
            approversToSave = new List<BoeApproverResponseDTO>();

            // need to update approverResponses -> remove ones being deleted, and add new ones.. basically the final state is what it will look like
            // need to record the changes (add / delete) into boeApproversToSave, which is what is then going to be saved
            int i = -1;

            Collection<int> currentBoeApproverIds, approversToAdd, approversToRemove;
            ManageBOEModelView rolesBeingSavedForBoe;

            foreach (BoeDTO boe in boesToSave)
            {
                rolesBeingSavedForBoe = boeRolesToSave.FirstOrDefault(x => x.BoeID == boe.Id);
                currentBoeApproverIds = wsPermissions.Where(x => x.Role == Role.Approver && x.BOEId == boe.Id).Select(x => x.ETIUserId).ToCollection();

                if (rolesBeingSavedForBoe != null)
                {
                    approversToAdd = rolesBeingSavedForBoe.Approvers.Where(x => !currentBoeApproverIds.Contains(x)).ToCollection();
                    approversToRemove = currentBoeApproverIds.Where(x => !rolesBeingSavedForBoe.Approvers.Contains(x)).ToCollection();
                }
                else
                {
                    approversToAdd = new Collection<int>();
                    approversToRemove = currentBoeApproverIds;                    
                }

                // add new approvers into Approvers To save
                approversToSave.AddRange(approversToAdd.Select(x => new BoeApproverResponseDTO() { Id = i--, ETIUserID = x, BoeID = boe.Id, Updateable = UpdateType.Upsert, CurrentUserETIUserID = currentUserId }));

                // Mark approvers being removed as "Deleted"
                approverResponses.Where(x => x.BoeID == boe.Id && approversToRemove.Contains(x.ETIUserID)).ForEach(x => { x.Updateable = UpdateType.Deleted; x.CurrentUserETIUserID = currentUserId; });

                // record changes for emails
                approversChangeDictionary.Add(boe.Id, wsUsers.Where(x => approversToAdd.Contains(x.UserID) || approversToRemove.Contains(x.UserID)).ToCollection());
            }

            // At this point approversToSave contains only new Approvers. We need to add these into approverResponses
            approverResponses.AddRange(approversToSave);

            // Process all marked "Deleted" approvers
            approversToSave.AddRange(approverResponses.Where(x => x.Updateable == UpdateType.Deleted));
            approverResponses = approverResponses.Where(x => x.Updateable != UpdateType.Deleted).ToCollection();

            return approversChangeDictionary;
        }

        /// <summary>
        /// Gets BOE transitions when bulk saving BOE permissions
        /// </summary>
        /// <param name="originalBoes">Original BOEs</param>
        /// <param name="boesToSave">BOEs being saved</param>
        /// <param name="allApproverResponses">All Approver Responses</param>
        /// <returns>Transitions to perform</returns>
        private List<(int BoeId, BOEState OldState, BOEState NewState)> GetBoeTransitionsForBulkRoleSave(ICollection<FullBoe> originalBoes, ICollection<BoeDTO> boesToSave, ICollection<BoeApproverResponseDTO> allApproverResponses)
        {
            List<(int BoeId, BOEState OldState, BOEState NewState)> transitionsToPerform = new List<(int BoeId, BOEState OldState, BOEState NewState)>();

            foreach (BoeDTO boe in boesToSave)
            {
                BoeDTO oldBoe = originalBoes.FirstOrDefault(b => b.Id == boe.Id);

                // if all approvers have approved, change state to Approved
                if (oldBoe.State == BOEState.AwaitingApproval)
                {
                    ICollection<BoeApproverResponseDTO> responses = allApproverResponses.Where(x => x.BoeID == boe.Id).ToList();

                    if (responses.Any() && responses.All(x => x.ApproverResponse == ApproverReponseType.Approved && x.Updateable != UpdateType.Deleted))
                    {
                        boe.State = BOEState.Approved;
                        transitionsToPerform.Add((BoeId: boe.Id, OldState: oldBoe.State, NewState: boe.State));
                    }
                }

                // Unassigned state -> draft if roles have been assigned
                if (oldBoe.State == BOEState.Unassigned)
                {
                    if (boe.AuthorIDs != null && boe.AuthorIDs.Any())
                    {
                        boe.State = BOEState.Draft;
                        transitionsToPerform.Add((BoeId: boe.Id, OldState: oldBoe.State, NewState: boe.State));
                    }
                }
            }

            return transitionsToPerform;
        }

        /// <summary>
        /// Performs the BOE permission save from the BOE Permission Bulk Page
        /// </summary>
        /// <param name="ws">WS</param>
        /// <param name="boesToSave">BOEs to save</param>
        /// <param name="originalBoes">Original BOEs</param>
        /// <param name="boeApproversToSave">Boe approver roles to save</param>
        /// <param name="transitionsToPerform">Transitions to perform</param>
        private void DoBulkBoeRoleSave(FullWorkspace ws, ICollection<BoeDTO> boesToSave, ICollection<FullBoe> originalBoes, ICollection<BoeApproverResponseDTO> boeApproversToSave, List<(int BoeId, BOEState OldState, BOEState NewState)> transitionsToPerform)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                // Save approvers
                this.boeApproverResponseLoader.Save(boeApproversToSave);

                // Save authors and sub authors
                this._BoeMediator.MediatedSaveBOEs(ws, boesToSave);

                // Transition BOEs
                foreach ((int BoeId, BOEState OldState, BOEState NewState) transition in transitionsToPerform)
                {
                    this._boeStateMachine.PerformStateTransitionAction(originalBoes.FirstOrDefault(x => x.Id == transition.BoeId), ws, transition.OldState, transition.NewState);
                }

                scope.Complete();
            }

            ws.RefreshBoes(); // remove cached data
        }

        /// <summary>
        /// Sends out an email after bulk BOE permission save
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boesToSave">Boes that were saved</param>
        /// <param name="originalBoes">Original BOEs</param>
        /// <param name="authorsChangeDictionary">List of Authors with changes</param>
        /// <param name="approversChangeDictionary">List of Approvers with changes</param>
        private void SendEmailsAfterBulkRoleSave(FullWorkspace ws, ICollection<BoeDTO> boesToSave, ICollection<FullBoe> originalBoes, Dictionary<int, Collection<UserDTO>> authorsChangeDictionary, Dictionary<int, Collection<UserDTO>> approversChangeDictionary)
        {
            foreach (BoeDTO boe in boesToSave)
            {
                // If the BOE has been put in draft mode, send BOE author email opened for edit, else Send the 'Author Changed' email
                if (ws.WorkspaceState == WorkspaceState.Working && authorsChangeDictionary.ContainsKey(boe.Id))
                {
                    if (boe.State == BOEState.Draft && originalBoes.First(x => x.Id == boe.Id).State == BOEState.Unassigned)
                    {
                        this._emailer.SendBOEAuthorsEmailOpenedForEdit(this.Factory.CreateFullBoe(boe), ws);
                    }
                    else
                    {
                        this._emailer.SendBOEAuthorsChanged(authorsChangeDictionary[boe.Id], this.Factory.CreateFullBoe(boe));
                    }
                }

                // Send the 'Approvers Changed' email, if applicable
                if (boe.State == BOEState.AwaitingApproval)
                {
                    this._emailer.SendBOEApproversChanged(approversChangeDictionary[boe.Id], this.Factory.CreateFullBoe(boe));
                }
            }
        }

		/// <summary>
		/// Gets all of the possible query operators.
		/// </summary>
		/// <returns>Collection of view model operators</returns>
		public async Task<ICollection<QueryOperatorViewModel>> GetAllOperators()
		{
			ICollection<QueryOperatorViewModel> data = (ICollection<QueryOperatorViewModel>)memCache.GetData(CACHE_GET_ALL_OPERATORS);

			if (data == null)
			{
				data = await GetAllOperatorsActual();
				if (data != null && data.Count > 0)
				{
					memCache.Add(CACHE_GET_ALL_OPERATORS, data, CACHE_DURATION);
				}
			}

			return data;
		}

		/// <summary>
		/// Gets all of the possible query operators.
		/// </summary>
		/// <returns>Collection of view model operators</returns>
		private async Task<ICollection<QueryOperatorViewModel>> GetAllOperatorsActual()
		{
            if (Utilities.IsSAPEnabledForSystem)
            {
                try
                {
                    Utilities.AddAuthorizationHeader(iesSapClient.HttpClient, (await tokenService.GetToken()).AccessToken);

                    return await iesSapClient.ApiQueryFilterGetAllOperatorsAsync();
                }
                catch (Exception ex)
                {
                    // gracefully handle error
                    logger.Error(ex, "Error calling SAP API to get Operators.");
                    return new List<QueryOperatorViewModel>();
                }
            }
            else
            {
                return new List<QueryOperatorViewModel>();
            }
		}

		/// <summary>
		/// Gets all of the possible query fields.
		/// </summary>
		/// <returns>Collection of view model fields</returns>
		public async Task<ICollection<QueryFieldViewModel>> GetAllFields()
		{
			string company = SystemConfiguration.Instance().CompanyMode.ToString();
			ICollection<QueryFieldViewModel> data = (ICollection<QueryFieldViewModel>)memCache.GetData(CACHE_GET_FIELDS + company);

			if (data == null)
			{
				data = await GetAllFieldsActual(company);
				if (data != null && data.Count > 0)
				{
					memCache.Add(CACHE_GET_FIELDS + company, data, CACHE_DURATION);
				}
			}

			return data;
		}

		/// <summary>
		/// Gets all of the possible query fields.
		/// </summary>
		/// <returns>Collection of view model fields</returns>
		private async Task<ICollection<QueryFieldViewModel>> GetAllFieldsActual(string company)
		{
            if (Utilities.IsSAPEnabledForSystem)
            {
                try
                {
                    Utilities.AddAuthorizationHeader(iesSapClient.HttpClient, (await tokenService.GetToken()).AccessToken);
					
					ActionLogic.IESSAPClient.CompanyConfiguration configuration;

					if (Enum.TryParse<ActionLogic.IESSAPClient.CompanyConfiguration>(company, out configuration))
					{
						return await iesSapClient.ApiQueryFilterGetAllFieldsForCompanyCodeAsync(configuration);
					}
					else
					{
						throw new GeneralAppException("Company Code could not be converted.");
					}
                }
                catch(Exception ex)
                {
                    // gracefully handle error
                    logger.Error(ex, "Error calling SAP API to get Fields.");
                    return new List<QueryFieldViewModel>();
                }
            }
            else
            {
                return new List<QueryFieldViewModel>();
            }
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
		// ~BOEControllerLogic()
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

		internal class BOEStateTransition
		{
			public int ID { get; set; }
			public BOEState OldState { get; set; }
			public BOEState NewState { get; set; }
		}
	}
}