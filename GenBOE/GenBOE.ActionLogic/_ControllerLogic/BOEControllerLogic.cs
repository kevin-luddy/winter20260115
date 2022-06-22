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

    public class BOEControllerLogic : IBOEControllerLogic
    {
        private IBOESummary _BOESummary;
        private IUserDTODataLoader UserLoader;
        private IActiveDirectoryUtilities _ADUtils;
        private IPermissionsDTODataLoader PermissionsLoader;
        private IFullObjectFactory Factory;
        private IBOEExporter _BOEExporter;
        private IBOECustomExporter _boeCustomExporter;
        private IGenBOEControllerLogic _genBOEControllerLogic;
        private IBoeMediator _BoeMediator;
        private IValidationHelper _validationHelper;
        private IBOECommentDTODataLoader _boeCommentLoader;
        private IBoeEmailer _emailer;
        private IBoeTaskElementMediator _BoeTaskElementMediator;
        private IWorkspaceVariableDTODataLoader _workspaceVariableLoader;
        private IBOEStateMachine _boeStateMachine;
        private IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation;
        private IBOELaborControllerLogic BoeLaborControllerLogic;
        private IValidateBOE _validateBOE;
        private ISecurityInformation _SecurityInformation;
        private IBOESearchDTODataLoader _boeSearchLoader;
        private ISecurityAccess _SecurityAccess;
        private IBoeTaskElementRecalculation _BoeTaskElementRecalculation;
        private IBOEImporter _BOEImporter;
        private IVariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private IConflictBOE _ConflictBOE;
        private INestedWBSUtilities _nestedWbsUtilities;
        private IProjectMapDataLoader projectMapLoader;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private IRteTemplateDataLoader rteTemplateDataLoader;
        private readonly IMoqTypeDataLoader moqTypeDataLoader;
        private IBoeApproverResponseDTODataLoader boeApproverResponseLoader;
        private IESSAPClient iesSapClient;

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
            IESSAPClient iesSapClient)
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

            theModelView.DeleteAction = WebConstants.ACTION_DELETE_TASK_ELEMENTS;
            theModelView.DisplayEvent = WebConstants.EVENT_DISPLAY_TASK_ELEMENT_DETAILS;

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
                                   BOETaskElementOrder = t.BOETaskElementOrder
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
                    BOETaskElementOrder = t.BOETaskElementOrder
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

            theModelView.ShowHistoricMetricCheck = false;

            ICollection<int> ids = new Collection<int>();
            ids.Add(boe.Id);
            theModelView.ShowHistoricMetricCheck = this.ShowHistoricMetricCheck(ids);

            return theModelView;
        }

        /// <summary>
        /// Gets  model views for custom fields in the BOE Header
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
                    BOECustomFieldsGridModelView metadata = new BOECustomFieldsGridModelView(customField);
                    Collection<CustomFieldValueDTO> options = allCustomFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection();

                    metadata.inUse = options.Any(x => x.CustomFieldValueInUseFlag);

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
                        subcontractorAuthors.ToCollection(), approvers.ToCollection(), boe.TaskElements.ToCollection(), totalCostTravel);

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
        public void ExportBOESearchPreview(FullWorkspace ws, int boeID, HttpResponseBase Response)
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

                    BOEExportInputs inputs = new BOEExportInputs(new FullBoe[] { boe }, exportWorkspace.Boes.ToList(), exportWorkspace.TaskElements.ToList(), exportWorkspace, rteTemplateOverrides, exportWorkspace.MoqTypeSelections.ToList());

                    // Get BOE Summary Grid data for the current BOE. Used for populating the summary grid on the template
                    ICollection<BOESummaryGridModelView> boeSummaryGridModelViews = this._BOESummary.GetBOESummaryGridModelViews(boe, inputs, isSubcontractorUser);

                    // Get template based on workspace preferences
                    WorkspaceExportFormatDTO wsExportFormatDTO = ws.WorkspaceExportFormats.First(x => x.Id == ws.TemplateID);

                    // Get BOE Export Model View for the current BOE. Used for filling in most of the data on the template.
                    BOEExportModelView boeExportModelView;
                                        
                    if (wsExportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.MASTER)
                    {
                        //use the custom exporter for master template types
                        this._boeCustomExporter.SetWorkspacePrecisionVariables(exportWorkspace);
                        boeExportModelView = this._boeCustomExporter.ConvertBoeDTOsToExportMVs(new FullBoe[] { boe }, inputs).First();
                        this._boeCustomExporter.ExportBOEToWordFile(inputs, new List<BOEExportModelView> { boeExportModelView }, boeSummaryGridModelViews, ws, null, Response, string.Format("genBOEExport-{0}.docx", boeID), wsExportFormatDTO);
                    }
                    else
                    {
                        this._BOEExporter.SetWorkspacePrecisionVariables(exportWorkspace);
                        boeExportModelView = this._BOEExporter.ConvertBoeDTOsToExportMVs(inputs).First();
                        this._BOEExporter.ExportBOEToWordFile(inputs, new List<BOEExportModelView> { boeExportModelView }, boeSummaryGridModelViews, ws, Response, string.Format("genBOEExport-{0}.docx", boeID), wsExportFormatDTO.PhysicalFilePathCache);
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
                    WorkspaceExportFormatDTO wsExportFormatDTO = ws.WorkspaceExportFormats.First(x => x.Id == ws.TemplateID);

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
        /// <param name="descriptionOnly">Bool denoting if only descrpition was changed</param>
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
            var validator = new DateRangeValidator();

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
        /// <returns>exprted file name and formatted filename in an array</returns>
        public string[] ExportManageBOE(FullWorkspace ws, string templateFileName, bool blankTemplate)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            // Call the export function in the business layer and get back the file name of the populated template.
            string exportedFileName = this._BOEExporter.ExportToExcelFile(templateFileName, ws, blankTemplate);

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

                    var cache = new VariableCircularReferenceCheckerCache();

                    foreach (var updatedBOE in updatedBOEs)
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
                        var workspaceVariable = new WorkspaceVariableModelView(workspaceVariableDTO, this._variableSelectBOEtoSumCalculation, copyWorkspace);

                        decimal newValue = 0m;

                        var matchingWorkspaceVariable = (from w in workspaceVariables
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

                    foreach (var workspaceVariableCR in boesWithConflict.workspaceVariableCircularReferences)
                    {
                        boeCopyConflict.workspaceVariableCircularReferences.Add(new WorkspaceVariableModelView(workspaceVariableCR, this._variableSelectBOEtoSumCalculation, ws));
                    }

                    foreach (var ordinaryVariableCR in boesWithConflict.ordinaryVariableCircularReferences)
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
        /// Returns a bool indicating if historic metrics should be shown
        /// </summary>
        /// <param name="ids">The id's of the boe's to check</param>
        /// <returns>true if the metrics should be shown, false otherwise</returns>
        public virtual bool ShowHistoricMetricCheck(ICollection<int> ids)
        {
            return false;
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
        /// Save Bulk Boe Roles. This is a kill and fill, so the assumption is that all BOEs from a workspace with roles will be include in Boe Roles To Save
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
            ICollection<FullBoe> originalBoes = ws.Boes.Where(boe => boe.State == BOEState.Draft || boe.State == BOEState.Unassigned || boe.State == BOEState.None).ToList();
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
            return await iesSapClient.ApiQueryFilterGetAllOperatorsAsync();
		}

        /// <summary>
        /// Gets all of the possible query fields.
        /// </summary>
        /// <returns>Collection of view model fields</returns>
        public async Task<ICollection<QueryFieldViewModel>> GetAllFields()
		{
            return await iesSapClient.ApiQueryFilterGetAllFieldsAsync();
		}
    }
}