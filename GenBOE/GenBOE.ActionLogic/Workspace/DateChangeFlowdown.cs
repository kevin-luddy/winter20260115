// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2016 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Business.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Business.BLL;
    using GenBOE.Business.Common.Email;
    using GenBOE.Business.WBS.BOE.TaskElement;
    using GenBOE.Common;
    using GenBOE.Common.classes;
    using GenBOE.Common.Exceptions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Class used to change dates from any level in the workspace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class DateChangeFlowdown
    {
        private IBoeMediator _BoeMediator = null;
        private IBoeTaskElementMediator _BoeTaskElementMediator = null;
        private ITravelDTODataLoader _TravelLoader = null;
        private IEmailer _Emailer = null;
        private IUserDTODataLoader userDTOLoader = null;
        private IBOEStateMachine _BOEStateMachine = null;
        private IPermissionsDTODataLoader _PermissionLoader { get; set; }
        private IFullObjectFactory factory;
        private IClinDTODataLoader clinLoader;
        private IWorkspaceDTODataLoader workspaceLoader;
        private IBoeDTODataLoader boeLoader;

        /// <summary>
        /// Flowdown update selection
        /// </summary>
        public FlowdownUpdateType FlowdownUpdateSelection { get; set; }

        /// <summary>
        /// How to adjust discreet values
        /// </summary>
        public HandleDiscrete DiscreteResourceSelection { get; set; }

        /// <summary>
        /// Does discrete resources exist for the workspace.
        /// </summary>
        public bool DiscreteResourcesExist { get; set; }

        /// <summary>
        /// Should we send out an email
        /// </summary>
        public bool SendEmailForBoeUpdates { get; set; }

        /// <summary>
        /// The Collection of the Clins that are to be shown or saved
        /// </summary>
        public ICollection<ClinDateCollection> ClinCollection { get; set; }

        /// <summary>
        /// The Clin object to be used when a specific Clin is passed in.
        /// </summary>
        public objCLINDateChange ClinDateChange { get; set; }

        /// <summary>
        /// workspace id
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Cline id
        /// </summary>
        public int ClinId { get; set; }

        /// <summary>
        /// boe id
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Task element id
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// level to change
        /// </summary>
        public Level changeLevel { get; set; }

        /// <summary>
        /// New start date
        /// </summary>
        public DateTime? NewStartDate { get; set; }

        /// <summary>
        /// New end date
        /// </summary>
        public DateTime? NewEndDate { get; set; }

        /// <summary>
        /// Old start date
        /// </summary>
        public DateTime? OldStartDate { get; set; }

        /// <summary>
        /// Old end date
        /// </summary>
        public DateTime? OldEndDate { get; set; }

        public DateAdjustment dateAdjustment { get; set; }

        /// <summary>
        /// Start date difference
        /// </summary>
        public int? startDateDiff { get; set; }

        /// <summary>
        /// End date difference
        /// </summary>
        public int? endDateDiff { get; set; }

        /// <summary>
        /// Old date difference
        /// </summary>
        public int? oldDateDiff { get; set; }

        /// <summary>
        /// New date difference
        /// </summary>
        public int? newDateDiff { get; set; }

        private HashSet<UserDateChangeInfo> userDateChangeInfo = new HashSet<UserDateChangeInfo>();

        /// <summary>
        /// workspace
        /// </summary>
        public FullWorkspace wsObject { get; set; }

        /// <summary>
        /// Clins
        /// </summary>
        public Collection<ClinDTO> clinDTOCollection { get; set; }

        private ICollection<FullBoe> _boeCollection = null;

        /// <summary>
        /// Boes
        /// </summary>
        public ICollection<FullBoe> boeCollection 
        {
            get
            {
                if (this._boeCollection == null)
                {
                    // return the boes from the wsObject if it exists
                    if (this.wsObject != null)
                    {
                        this._boeCollection = wsObject.Boes;
                    }
                }

                return this._boeCollection;
            }
            set
            {
                this._boeCollection = value;
            }
        }

        /// <summary>
        /// DTS regions
        /// </summary>
        public ICollection<WorkspaceDTSRegionDTO> workspaceDTSRegionCollection { get; set; }

        /// <summary>
        /// Set of task elements to be modified
        /// </summary>
        public ICollection<BoeTaskElementDTO> taskDTOCollection { get; set; }

        /// <summary>
        /// Original Unmodified task elements
        /// </summary>
        public ICollection<BoeTaskElementDTO> originalUnmodifiedTaskDTOCollection { get; set; }

        /// <summary>
        /// Original unmodified boes
        /// </summary>
        public ICollection<BoeDTO> originalUnmodifiedBoes { get; set; }

        /// <summary>
        /// travel dtos
        /// </summary>
        public ICollection<TravelDTO> travelDTOCollection { get; set; }

        /// <summary>
        /// WBSes
        /// </summary>
        public ICollection<WbsDTO> wbsDTOCollection { get; set; }

        private WorkspaceDTO workspaceDtoToSave;
        private Collection<BoeDTO> dtsBoesToSave;
        private Collection<ClinDTO> clinDtosToSave;
        private Collection<BoeDTO> boesToSave;
        private Collection<BoeTaskElementDTO> tasksToSave;
        private Collection<TravelDTO> travelsToSave;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DateChangeFlowdown()
        {
            this._BoeMediator = GenBOEUnityContainer.Container.Resolve(typeof(IBoeMediator)) as IBoeMediator;
            this._BoeTaskElementMediator = GenBOEUnityContainer.Container.Resolve(typeof(IBoeTaskElementMediator)) as IBoeTaskElementMediator;
            this._TravelLoader = GenBOEUnityContainer.Container.Resolve(typeof(ITravelDTODataLoader)) as ITravelDTODataLoader;
            this._Emailer = GenBOEUnityContainer.Container.Resolve(typeof(IEmailer)) as IEmailer;
            this.userDTOLoader = GenBOEUnityContainer.Container.Resolve(typeof(IUserDTODataLoader)) as IUserDTODataLoader;
            this._PermissionLoader = GenBOEUnityContainer.Container.Resolve(typeof(IPermissionsDTODataLoader)) as IPermissionsDTODataLoader;
            this._BOEStateMachine = GenBOEUnityContainer.Container.Resolve(typeof(IBOEStateMachine)) as IBOEStateMachine;
            this.factory = GenBOEUnityContainer.Container.Resolve(typeof(IFullObjectFactory)) as IFullObjectFactory;
            this.clinLoader = GenBOEUnityContainer.Container.Resolve(typeof(IClinDTODataLoader)) as IClinDTODataLoader;
            this.workspaceLoader = GenBOEUnityContainer.Container.Resolve(typeof(IWorkspaceDTODataLoader)) as IWorkspaceDTODataLoader;
            this.boeLoader = GenBOEUnityContainer.Container.Resolve(typeof(IBoeDTODataLoader)) as IBoeDTODataLoader;

            wsObject = this.factory.CreateFullWorkspace(new WorkspaceDTO());
            clinDTOCollection = new Collection<ClinDTO>();
            taskDTOCollection = new Collection<BoeTaskElementDTO>();
            travelDTOCollection = new Collection<TravelDTO>();
            ClinCollection = new Collection<ClinDateCollection>();
            wbsDTOCollection = new Collection<WbsDTO>();

            this.dtsBoesToSave = new Collection<BoeDTO>();
            this.clinDtosToSave = new Collection<ClinDTO>();
            this.boesToSave = new Collection<BoeDTO>();
            this.tasksToSave = new Collection<BoeTaskElementDTO>();
            this.travelsToSave = new Collection<TravelDTO>();
        }

        /// <summary>
        /// Flows the date change to all elements in the workspace
        /// </summary>
        /// <returns></returns>
        public bool ChangedDates()
        {
            bool toReturn = false;
            int outID = 0;

            switch (changeLevel)
            {
                case Level.Workspace:
                    outID = WorkspaceId;
                    break;
                case Level.CLIN:
                    outID = ClinId;
                    break;
                case Level.BOE:
                    outID = BoeId;
                    break;
                case Level.Task:
                case Level.Travel:
                    outID = TaskId;
                    break;
            }

            if (dateAdjustment == DateAdjustment.None)
            {
                dateAdjustment = DetermineDateAdjustment();
            }

            if (dateAdjustment == DateAdjustment.Shift && FlowdownUpdateSelection == FlowdownUpdateType.NotSet)
            {
                FlowdownUpdateSelection = FlowdownUpdateType.Automatic;
            }

            toReturn = ShiftDates(outID, changeLevel, startDateDiff, endDateDiff);

            SaveAllData();

            if (toReturn && SendEmailForBoeUpdates)
            {
                _Emailer.SendBOEAuthorsApproversDatesUpdated(userDateChangeInfo);
            }

            return toReturn;
        }

        /// <summary>
        /// Saves all of the data, at the end of the date shift; this was done for performance reasons
        /// </summary>]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void SaveAllData()
        {
            // Save changes from ModifyWorkspaceDate
            if (this.workspaceDtoToSave != null)
            {
                // Get the user who is saving the BOE(s)
                int currentUserID = this.userDTOLoader.GetUserForActiveUser().UserID;
                this.workspaceLoader.SaveWorkspaceSettings(currentUserID, workspaceDtoToSave);
            }

            // Save changes from ModifyClinDate
            if (this.clinDtosToSave.Any())
            {
                clinLoader.Save(clinDtosToSave);
            }

            // Check to see if any Summary BOEs will be impacted by the BOEs being changed.
            {
                List<BoeDTO> boesImpactedByDateShift = boesToSave.ToList();

                if (this.tasksToSave.Any())
                {
                    boesImpactedByDateShift.AddRange(wsObject.Boes.Where(x => tasksToSave.Select(z => z.BoeID).Contains(x.Id)).ToList());
                }

                List<FullBoe> summaryBoes = this.wsObject.Boes.Where(x => x.IsSummaryBoe).ToList();
                foreach (FullBoe summaryBoe in summaryBoes)
                {
                    foreach (BoeDTO boeToCheck in boesImpactedByDateShift)
                    {
                        if (FullObjectHelper.DoChangesToBoeImpactSummaryBoe(wsObject, summaryBoe, factory.CreateFullBoe(boeToCheck)))
                        {
                            if (!boesToSave.Select(x => x.Id).Contains(summaryBoe.Id))
                            {
                                summaryBoe.Updateable = UpdateType.Upsert;
                                boesToSave.Add(summaryBoe);
                            }

                            break;
                        }
                    }
                }
            }

            // Save changes from ModifyBoeDate
            if (this.boesToSave.Any())
            {
                List<BoeDtoForStateTransition> boesForTransition = new List<BoeDtoForStateTransition>();

                foreach (BoeDTO boeDto in this.boesToSave)
                {
                    bool stateChange = false;
                    BOEState oldBOEState = boeDto.State;

                    // only need to transition BOEs that are in awaiting approval or approved state
                    if (boeDto.State == BOEState.AwaitingApproval || boeDto.State == BOEState.Approved || boeDto.State == BOEState.DraftLocked)
                    {
                        BOEState newBOEState = BOEState.Draft;

                        // Validate the Awaiting Approval or Approved to Draft state transition
                        string validationMessage = string.Empty;

                        if (!_BOEStateMachine.PerformStateTransitionValidation(this.factory.CreateFullBoe(boeDto), this.wsObject, oldBOEState, newBOEState, out validationMessage))
                        {
                            // TODO? - If we throw, then what happens to Saves that were already applied? 31680

                            // not valid ... communicate to user
                            throw new ValidationException(validationMessage);
                        }

                        // If the transition is valid, set the BOE to Draft
                        boeDto.State = newBOEState;
                        stateChange = true;
                    }

                    boesForTransition.Add(new BoeDtoForStateTransition() { BoeDto = boeDto, OldBoeState = oldBOEState, StateChangeHappening = stateChange });

                    // avoid double-save of DTS BOE
                    if (dtsBoesToSave.Any(b => b.Id == boeDto.Id))
                    {
                        this.boesToSave.Remove(boeDto);
                    }
                }

                int currentUserID = this.userDTOLoader.GetUserForActiveUser().UserID;
                foreach (var boe in boesToSave)
                {
                    boe.UpdatedByUserId = currentUserID;
                }

                // Make sure that RTE data is loaded, that way we do not wipe it out..
                this.boeLoader.LoadRTEFields(boesToSave);

                this.boeLoader.Save(boesToSave);

                foreach (BoeDtoForStateTransition boe in boesForTransition)
                {
                    // Perform common state transition actions
                    if (boe.StateChangeHappening)
                    {
                        BOEState tempState = boe.BoeDto.State;

                        if (tempState == BOEState.Draft || tempState == BOEState.DraftLocked)
                        {
                            tempState = BOEState.DateShiftDraft;
                        }

                        _BOEStateMachine.PerformStateTransitionAction(this.factory.CreateFullBoe(boe.BoeDto), this.wsObject, boe.OldBoeState, tempState);
                    }
                }
            }  // end if boesToSave

            // Save changes from ModifyTaskDate
            if (this.tasksToSave.Any())
            {
                _BoeTaskElementMediator.MediatedBulkSaveTaskElements(
                    this.tasksToSave,
                    this.wsObject);
            }

            // Save changes from ModifyTravelDate
            if (this.travelsToSave.Any())
            {
                _TravelLoader.SaveTravels(this.travelsToSave);
            }

            // Save DTS Boe changes, if needed
            if (this.workspaceDtoToSave != null && this.workspaceDtoToSave.DTSSetting == DTSAutoCalculateSetting.InSummaryBoes
                && this.dtsBoesToSave != null && this.dtsBoesToSave.Any(d => d.Updateable != UpdateType.None))
            {
                foreach (BoeDTO dtsBOE in this.dtsBoesToSave)
                {
                    dtsBOE.Updateable = UpdateType.Upsert;
                }
                this._BoeMediator.MediatedSaveBOEs(this.wsObject, this.dtsBoesToSave, originalUnmodifiedBoes);
            }
        }

        /// <summary>
        /// Sets up date info for the flow down.
        /// </summary>
        /// <returns>always returns false</returns>
        private bool setupDateInfo()
        {
            DateTime oldSD;
            DateTime oldED;
            DateTime newSD;
            DateTime newED;

            if (OldStartDate.HasValue && OldEndDate.HasValue && NewStartDate.HasValue && NewEndDate.HasValue)
            {
                oldSD = OldStartDate.Value;
                oldED = OldEndDate.Value;
                newSD = NewStartDate.Value;
                newED = NewEndDate.Value;

                startDateDiff = oldSD.MonthDifference(newSD);
                endDateDiff = oldED.MonthDifference(newED);
                oldDateDiff = oldSD.MonthDifference(oldED);
                newDateDiff = newSD.MonthDifference(newED);

                return true;
            }
            else
            {
                startDateDiff = null;
                endDateDiff = null;
                oldDateDiff = null;
                newDateDiff = null;

                return false;
            }
        }

        /// <summary>
        /// Determins the date adjustment
        /// </summary>
        /// <returns>The Date Adjustment</returns>
        private DateAdjustment DetermineDateAdjustment()
        {
            DateAdjustment toReturn = DateAdjustment.None;

            if (setupDateInfo())
            {
                if (startDateDiff == endDateDiff && startDateDiff != 0 && endDateDiff != 0)
                {
                    toReturn = DateAdjustment.Shift;
                }
                else if (oldDateDiff < newDateDiff)
                {
                    toReturn = DateAdjustment.Expand;
                }
                else if (newDateDiff < oldDateDiff)
                {
                    toReturn = DateAdjustment.Compress;
                }
            }

            return toReturn;
        }

        //// TODO - remove the boolean return for all methods that always return a true. Use WI 29246
        /// <summary>
        /// Modifies the workspace date
        /// </summary>
        /// <param name="inID">Id of the workspace</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <returns>always returns true</returns>
        private bool ModifyWorkspaceDate(int inID, int? inStartDateDiff, int? inEndDateDiff)
        {
            if (changeLevel == Level.Workspace || dateAdjustment == DateAdjustment.Shift ||
                FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual)
            {
                DateTime workspaceRequestedStartDate = wsObject.ContractStartDate.AddMonths(inStartDateDiff.Value);
                if (workspaceRequestedStartDate != null)
                    workspaceRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(workspaceRequestedStartDate, DateTimePrecision.Month);

                DateTime workspaceRequestedEndDate = wsObject.ContractEndDate.AddMonths(inEndDateDiff.Value);
                if (workspaceRequestedEndDate != null)
                    workspaceRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(workspaceRequestedEndDate, DateTimePrecision.Month);

                if (workspaceRequestedStartDate.CompareTo(NewStartDate) < 0 || workspaceRequestedStartDate.CompareTo(NewEndDate) > 0 ||
                    workspaceRequestedEndDate.CompareTo(NewStartDate) < 0 || workspaceRequestedEndDate.CompareTo(NewEndDate) > 0 ||
                    workspaceRequestedStartDate.CompareTo(workspaceRequestedEndDate) > 0)
                {
                    wsObject.ContractStartDate = NewStartDate.Value;
                    wsObject.ContractEndDate = NewEndDate.Value;
                }
                else
                {
                    wsObject.ContractStartDate = workspaceRequestedStartDate;
                    wsObject.ContractEndDate = workspaceRequestedEndDate;
                }

                workspaceDtoToSave = wsObject;

                Dictionary<int, int> ClinIDByBoeID = new Dictionary<int, int>();

                //  Determine if we need to adjust CLIN dates.
                //  Add BOE IDs to a collection so that they will be excluded when
                //  we update the BOEs since the BOE is adjusted here.
                if (ClinCollection != null)
                {
                    foreach (ClinDateCollection tempclin in ClinCollection)
                    {
                        Collection<int> tempboeids = ((from x in this.boeCollection
                                                       where x.CLINID == tempclin.ClinId
                                                       && !x.IsSummaryBoe
                                                       select x.Id).ToList()).ToCollection();

                        if (tempboeids != null)
                        {
                            foreach (int tempId in tempboeids)
                            {
                                // Each BOE maps to a single CLIN
                                if (!ClinIDByBoeID.ContainsKey(tempId))
                                    ClinIDByBoeID.Add(tempId, tempclin.ClinId);
                            }
                        }

                        if (!ShiftDates(tempclin.ClinId, Level.CLIN, inStartDateDiff, inEndDateDiff))
                            throw new GeneralAppException("Unable to Shift CLIN with ID of " + tempclin.ClinId);
                    }
                }

                // Change BOE dates for Summary BOEs and BOEs that do not have CLINS
                Collection<int> boes = ((from x in this.boeCollection
                                         where x.WorkspaceID == inID
                                         select x.Id).ToList()).ToCollection();

                if (ClinIDByBoeID != null && boes != null)
                {
                    foreach (int tempBoeId in boes)
                    {
                        if (!ClinIDByBoeID.ContainsKey(tempBoeId))
                        {
                            if (!ShiftDates(tempBoeId, Level.BOE, inStartDateDiff, inEndDateDiff))
                                throw new GeneralAppException("Unable to Shift BOE with ID of " + tempBoeId);
                        }
                    }
                }

                // the DTS BOE associated with the CLIN needs to match the CLIN dates.
                if (wsObject.DTSSetting == DTSAutoCalculateSetting.InSummaryBoes)
                {
                    // InSummaryBoes means there is one and only one workspace level DTS boe for DTS which is indicated by CLINID == null
                    BoeDTO dtsBoe = this.boeCollection.Where(x => x.CLINID == null && x.isDTS).FirstOrDefault();
                    if (dtsBoe != null)
                    {
                        dtsBoe.StartDate = wsObject.ContractStartDate;
                        dtsBoe.EndDate = wsObject.ContractEndDate;
                        dtsBoe.Updateable = UpdateType.Upsert;

                        if (!dtsBoesToSave.Any(b => b.Id == dtsBoe.Id))
                        {
                            dtsBoesToSave.Add(dtsBoe);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Modifies the CLIN dates
        /// </summary>
        /// <returns>always returns true</returns>
        private bool ModifyClinCollectionDates()
        {
            bool FlowdownUpdateSelection_NotSet = false;

            if (ClinCollection != null)
            {
                if (FlowdownUpdateSelection == FlowdownUpdateType.NotSet)
                    FlowdownUpdateSelection_NotSet = true;

                foreach (ClinDateCollection tempclin in ClinCollection)
                {
                    ClinDTO clinDTO = (from x in this.clinDTOCollection
                                       where x.Id == tempclin.ClinId
                                       select x).FirstOrDefault();

                    NewStartDate = tempclin.ClinStartDate;
                    NewEndDate = tempclin.ClinEndDate;
                    OldStartDate = clinDTO.StartDate;
                    OldEndDate = clinDTO.EndDate;

                    dateAdjustment = DetermineDateAdjustment();

                    if (FlowdownUpdateSelection_NotSet && dateAdjustment == DateAdjustment.Shift)
                    {
                        FlowdownUpdateSelection = FlowdownUpdateType.Automatic;
                    }
                    else if (FlowdownUpdateSelection_NotSet)
                    {
                        FlowdownUpdateSelection = FlowdownUpdateType.NotSet;
                    }

                    if (!ShiftDates(tempclin.ClinId, Level.CLIN, startDateDiff, endDateDiff))
                    {
                        throw new GeneralAppException("Unable to Shift CLIN with ID of " + tempclin.ClinNumber + tempclin.ClinTitle);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Modifieds the date of a single clin
        /// </summary>
        /// <param name="inID">id of the clin to modify</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date differenct</param>
        /// <returns>always returns true</returns>
        private bool ModifyClinDate(int inID, int? inStartDateDiff, int? inEndDateDiff)
        {
            if (changeLevel == Level.Workspace || changeLevel == Level.CLINCollection || dateAdjustment == DateAdjustment.Shift ||
                FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual)
            {
                DateTime wsStartDate = new DateTime();
                DateTime wsEndDate = new DateTime();
                DateTime? clinNewStartDate = null; ;
                DateTime? clinNewEndDate = null;
                bool clearOutDates = false;

                if (wsObject != null)
                {
                    if (wsObject.ContractStartDate != null)
                        wsStartDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractStartDate, DateTimePrecision.Month);

                    if (wsObject.ContractEndDate != null)
                        wsEndDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractEndDate, DateTimePrecision.Month);
                }
                else
                {
                    return false;
                }

                int? clinStartDateDiff;
                int? clinEndDateDiff;

                // Saves shifted dates for CLINs
                ClinDTO clinDTO = (from x in clinDTOCollection
                                   where x.Id == inID
                                   select x).FirstOrDefault();

                if (clinDTO != null)
                {
                    ClinDateCollection clinDate = (from c in ClinCollection
                                                   where c.ClinId == inID
                                                   select c).FirstOrDefault();

                    if (clinDate == null)
                        return false;

                    clinNewStartDate = clinDate.ClinStartDate;
                    clinNewEndDate = clinDate.ClinEndDate;

                    if (((clinDTO.StartDate.ToString()).IsValidDate() && string.IsNullOrEmpty(clinDate.ClinStartDate.ToString())) && ((clinDTO.EndDate.ToString()).IsValidDate() && string.IsNullOrEmpty(clinDate.ClinEndDate.ToString())))
                    {
                        clearOutDates = true;

                        if (wsObject.ContractStartDate != null)
                            clinNewStartDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractStartDate, DateTimePrecision.Month);

                        if (wsObject.ContractEndDate != null)
                            clinNewEndDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractEndDate, DateTimePrecision.Month);
                    }

                    if (clinNewStartDate.HasValue && clinNewEndDate.HasValue)
                    {
                        DateTime? clinOldStartDate;
                        DateTime? clinOldEndDate;
                        DateTime clinRequestedStartDate;
                        DateTime clinRequestedEndDate;

                        if (clinDTO.StartDate.HasValue && clinDTO.EndDate.HasValue)
                        {
                            // CLIN DTO has a Start/End Date
                            clinOldStartDate = GenBOEUtilities.AdjustDateTimePrecision(clinDTO.StartDate.Value, DateTimePrecision.Month);
                            clinOldEndDate = GenBOEUtilities.AdjustDateTimePrecision(clinDTO.EndDate.Value, DateTimePrecision.Month);
                        }
                        else
                        {
                            // CLIN DTO does not have a Start/End Date
                            clinOldStartDate = null;
                            clinOldEndDate = null;
                        }

                        if (dateAdjustment == DateAdjustment.Shift && changeLevel != Level.CLINCollection)
                        {
                            // Only way this will be a shift is if Workspace was a shift.  If a CLIN grid was displayed, 
                            // this code will never be reached.
                            clinRequestedStartDate = clinNewStartDate.Value.AddMonths(inStartDateDiff.Value);
                            clinRequestedEndDate = clinNewEndDate.Value.AddMonths(inEndDateDiff.Value);
                        }
                        else
                        {
                            clinRequestedStartDate = clinNewStartDate.Value;
                            clinRequestedEndDate = clinNewEndDate.Value;
                        }

                        clinRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(clinRequestedStartDate, DateTimePrecision.Month);
                        clinRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(clinRequestedEndDate, DateTimePrecision.Month);

                        if (clinRequestedStartDate.CompareTo(wsStartDate) < 0 || clinRequestedStartDate.CompareTo(wsEndDate) > 0 ||
                            clinRequestedEndDate.CompareTo(wsStartDate) < 0 || clinRequestedEndDate.CompareTo(wsEndDate) > 0 ||
                            clinRequestedStartDate.CompareTo(clinRequestedEndDate) > 0)
                        {
                            clinDTO.StartDate = wsStartDate;
                            clinDTO.EndDate = wsEndDate;
                        }
                        else
                        {
                            clinDTO.StartDate = clinRequestedStartDate;
                            clinDTO.EndDate = clinRequestedEndDate;
                        }

                        if (clinOldStartDate.HasValue && clinOldEndDate.HasValue)
                        {
                            DateTime tempSD, tempED;

                            tempSD = clinOldStartDate.Value;
                            tempED = clinOldEndDate.Value;

                            clinStartDateDiff = tempSD.MonthDifference(clinDTO.StartDate.Value);
                            clinEndDateDiff = tempED.MonthDifference(clinDTO.EndDate.Value);
                        }
                        else
                        {
                            clinStartDateDiff = null;
                            clinEndDateDiff = null;
                        }

                        clinDTO.Updateable = UpdateType.Upsert;

                        if (clearOutDates)
                        {
                            clinDTO.StartDate = null;
                            clinDTO.EndDate = null;
                        }

                        this.clinDtosToSave.Add(clinDTO);
                    }
                    else
                    {
                        clinStartDateDiff = inStartDateDiff;
                        clinEndDateDiff = inEndDateDiff;
                    }

                    if (FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual)
                    {   
                        //Do not include Summary BOEs since their dates are not affected by their CLIN dates
                        Collection<int> colBoeIds = ((from x in this.boeCollection
                                                      where x.CLINID == clinDTO.Id
                                                      && !x.IsSummaryBoe
                                                      select x.Id).ToList()).ToCollection();

                        if (colBoeIds != null)
                        {
                            foreach (int tempBoeId in colBoeIds)
                            {
                                if(!ShiftDates(tempBoeId, Level.BOE, clinStartDateDiff, clinEndDateDiff, clinDTO))
                                    throw new GeneralAppException("Unable to Shift BOE with ID of " + tempBoeId);
                            }
                        }
                    }

                    // the DTS BOE associated with the CLIN needs to match the CLIN dates.
                    if (wsObject.DTSSetting == DTSAutoCalculateSetting.InSummaryBoes)
                    {
                        BoeDTO dtsBoe = this.boeCollection.Where(x => x.CLINID.HasValue && x.CLINID.Value == clinDTO.Id && x.isDTS).FirstOrDefault();
                        if (dtsBoe != null)
                        {
                            dtsBoe.StartDate = clinDTO.StartDate.HasValue ? clinDTO.StartDate.Value : wsObject.ContractStartDate;
                            dtsBoe.EndDate = clinDTO.EndDate.HasValue ? clinDTO.EndDate.Value : wsObject.ContractEndDate;
                            dtsBoe.Updateable = UpdateType.Upsert;

                            if (!dtsBoesToSave.Any(b => b.Id == dtsBoe.Id))
                            {
                                dtsBoesToSave.Add(dtsBoe);
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Modifies the date of a single BOE
        /// </summary>
        /// <param name="inID">id of the boe to modify</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="clinDTO">clin associated with boe</param>
        /// <returns>always returns true</returns>
        private bool ModifyBoeDate(int inID, int? inStartDateDiff, int? inEndDateDiff, ClinDTO clinDTO)
        {
            // Uses mediated save to save shifted date in BOE, then calls shiftdates for task level entries
            BoeDTO boeDTO = (from x in this.boeCollection
                             where x.Id == inID
                             select x).FirstOrDefault();

            if (boeDTO == null)
            {
                return false;
            }

            if (changeLevel == Level.BOE || dateAdjustment == DateAdjustment.Shift ||
                FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual
                || boeDTO.IsSummaryBoe)
            {                
                // DTS Summary and material BOEs don't get updated. (DTS Summary gets updated automatically in the mediators, material is excluded as per the wireframe)
                if (wsObject.DTSSetting == DTSAutoCalculateSetting.InSummaryBoes && boeDTO.isDTS || boeDTO.isMaterial)
                    return true;

                DateTime originalStartDate = boeDTO.StartDate;
                DateTime originalEndDate = boeDTO.EndDate;

                if (clinDTO == null && boeDTO.CLINID != null)
                {
                    clinDTO = (from x in clinDTOCollection
                               where x.Id == boeDTO.CLINID.Value
                               select x).FirstOrDefault();
                }

                // if inStartDateDiff and inEndDateDiff are null then this method was called from CLIN method
                // where the original CLIN date was null and a new start date was entered.  The code now
                // needs to determine what the difference is using the boe start/end date as the old date
                // and do a diff on them.
                if (!inStartDateDiff.HasValue || !inEndDateDiff.HasValue)
                {
                    OldStartDate = boeDTO.StartDate;
                    OldEndDate = boeDTO.EndDate;

                    NewStartDate = clinDTO.StartDate;
                    NewEndDate = clinDTO.EndDate;

                    setupDateInfo();

                    inStartDateDiff = startDateDiff.Value;
                    inEndDateDiff = endDateDiff.Value;
                }

                DateTime compareStartDate;
                DateTime compareEndDate;

                // if BOE is mapped to a CLIN and the BOE is not a Summary BOE, then use the CLIN dates ...
                if (boeDTO.CLINID != null && clinDTO != null && clinDTO.StartDate.HasValue && clinDTO.EndDate.HasValue && !boeDTO.IsSummaryBoe)
                {
                    compareStartDate = GenBOEUtilities.AdjustDateTimePrecision(clinDTO.StartDate.Value, DateTimePrecision.Month);
                    compareEndDate = GenBOEUtilities.AdjustDateTimePrecision(clinDTO.EndDate.Value, DateTimePrecision.Month);
                }
                else  // ... otherwise use the workspace dates
                {
                    compareStartDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractStartDate, DateTimePrecision.Month);
                    compareEndDate = GenBOEUtilities.AdjustDateTimePrecision(wsObject.ContractEndDate, DateTimePrecision.Month);
                }

                DateTime boeRequestedStartDate;
                DateTime boeRequestedEndDate;

                // Again, if it is a Summary BOE,  start/end dates need to be the same as the Workspace Start/End dates.
                if (boeDTO.IsSummaryBoe)
                {
                    boeRequestedStartDate = wsObject.ContractStartDate;
                    boeRequestedEndDate = wsObject.ContractEndDate;
                }
                else
                {
                    boeRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.StartDate.AddMonths(inStartDateDiff.Value), DateTimePrecision.Month);
                    boeRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.EndDate.AddMonths(inEndDateDiff.Value), DateTimePrecision.Month);
                }

                if (boeRequestedStartDate.CompareTo(compareStartDate) < 0 || boeRequestedStartDate.CompareTo(compareEndDate) > 0 ||
                    boeRequestedEndDate.CompareTo(compareStartDate) < 0 || boeRequestedEndDate.CompareTo(compareEndDate) > 0 ||
                    boeRequestedStartDate.CompareTo(boeRequestedEndDate) > 0)
                {
                    boeDTO.StartDate = compareStartDate;
                    boeDTO.EndDate = compareEndDate;
                }
                else
                {
                    boeDTO.StartDate = boeRequestedStartDate;
                    boeDTO.EndDate = boeRequestedEndDate;
                }

                // Gather Email information
                if (SendEmailForBoeUpdates && (changeLevel == Level.Workspace || changeLevel == Level.CLINCollection) &&
                    (boeDTO.State == BOEState.Approved || boeDTO.State == BOEState.AwaitingApproval || boeDTO.State == BOEState.Draft ||  boeDTO.State == BOEState.DraftLocked))
                {
                    GenerateEmail(boeDTO, originalStartDate, originalEndDate);
                }

                boeDTO.Updateable = UpdateType.Upsert;

                this.boesToSave.Add(boeDTO);

                Collection<int> taskids = ((from x in taskDTOCollection
                                            where x.BoeID == boeDTO.Id
                                            select x.Id).ToList()).ToCollection();

                if (taskids != null)
                {
                    foreach (int taskid in taskids)
                    {
                        if(!ShiftDates(taskid, Level.Task, inStartDateDiff, inEndDateDiff, boeDTO))
                            throw new GeneralAppException("Unable to Shift Task with ID of " + taskid);
                    }
                }

                taskids = ((from x in travelDTOCollection
                            where x.BoeID == boeDTO.Id
                            select x.Id).ToList()).ToCollection();

                if (taskids != null)
                {
                    foreach (int taskid in taskids)
                    {
                        if(!ShiftDates(taskid, Level.Travel, inStartDateDiff, inEndDateDiff, boeDTO))
                            throw new GeneralAppException("Unable to Shift Task with ID of " + taskid);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Generates and sends an email to notify the boe authors of a date change
        /// </summary>
        /// <param name="boeDTO">boe that changed</param>
        /// <param name="originalStartDate">original start date</param>
        /// <param name="originalEndDate">original end date</param>
        private void GenerateEmail(BoeDTO boeDTO, DateTime originalStartDate,DateTime originalEndDate)
        {
            bool userIsAuthor = false;
            WbsDTO wbs;
            ClinDTO clin;
            string wbsTitle = string.Empty;
            string wbsDisplayID = string.Empty;
            string clinTitle = string.Empty;
            string clinDisplayID = string.Empty;

            UserDTO workspaceAdmin = userDTOLoader.GetUserForActiveUser();

            // retrieve the list of authors on the BOE
            var authorsList = from x in _PermissionLoader.GetBOEPermissions(new List<int>() { boeDTO.Id} )
                              where x.Role == Role.Author || x.Role == Role.SubcontractorAuthor
                              select x.ETIUserId;

            userIsAuthor = authorsList.Contains(workspaceAdmin.UserID);   // check if the current user is assigned as an author

            if (boeDTO.WBSID.HasValue)
            {
                wbs = (from w in wbsDTOCollection
                       where w.Id == boeDTO.WBSID.Value
                       select w).FirstOrDefault();

                if (wbs != null)
                {
                    wbsTitle = wbs.WbsTitle;
                    wbsDisplayID = wbs.WbsNumber;
                }
            }

            if (boeDTO.CLINID.HasValue)
            {
                clin = (from c in clinDTOCollection
                        where c.Id == boeDTO.CLINID.Value
                        select c).FirstOrDefault();

                if (clin != null)
                {
                    clinTitle = clin.ClinTitle;
                    clinDisplayID = clin.ClinNumber;
                }
            }
            // for each Author
            foreach (int authorID in authorsList)
            {
                AddEmailInfo(authorID, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, originalStartDate, originalEndDate, workspaceAdmin.DisplayName, userIsAuthor, false);
            }
        }

        /// <summary>
        /// Adds email infor
        /// </summary>
        /// <param name="userID">user id</param>
        /// <param name="boeDTO">boe</param>
        /// <param name="wbsDisplayID">wbs display id</param>
        /// <param name="wbsTitle">wbs title</param>
        /// <param name="clinDisplayID">clin display id</param>
        /// <param name="clinTitle">clin title</param>
        /// <param name="orgSD">org</param>
        /// <param name="orgED">org</param>
        /// <param name="workspaceAdmin">workspace admin</param>
        /// <param name="userIsAuthor">indicates if user is the author</param>
        /// <param name="bApprover">approver</param>
        private void AddEmailInfo(int userID, BoeDTO boeDTO, string wbsDisplayID, string wbsTitle, string clinDisplayID, string clinTitle, DateTime orgSD, DateTime orgED, string workspaceAdmin, bool userIsAuthor, bool bApprover)
        {
            DateChangeInfo currentInfo = new DateChangeInfo();

            currentInfo.userID = userID;
            currentInfo.originalStartDate = orgSD;
            currentInfo.originalEndDate = orgED;
            currentInfo.boeID = boeDTO.Id;
            currentInfo.wbsDisplayID = wbsDisplayID;
            currentInfo.wbsTitle = wbsTitle;
            currentInfo.clinDisplayID = clinDisplayID;
            currentInfo.clinTitle = clinTitle;
            currentInfo.newStartDate = boeDTO.StartDate;
            currentInfo.newEndDate = boeDTO.EndDate;
            currentInfo.orginalState = boeDTO.State;

            var temp = (from x in userDateChangeInfo
                        where x.userID == userID
                        select x);

            if (temp.Any())
            {
                UserDateChangeInfo tempAuthorDateInfo = temp.First();

                tempAuthorDateInfo.AddBoeInfo(currentInfo);
            }
            else
            {
                UserDateChangeInfo tempUserDateInfo = new UserDateChangeInfo();

                tempUserDateInfo.userID = userID;
                tempUserDateInfo.workspaceName = wsObject.Shortname;
                tempUserDateInfo.changedBy = workspaceAdmin;

                UserDTO author = userDTOLoader.GetUserByID(userID);
                tempUserDateInfo.email = author.EmailAddress;
                
                tempUserDateInfo.AddBoeInfo(currentInfo);

                userDateChangeInfo.Add(tempUserDateInfo);
            }

            // Gather the approvers
            if (!userIsAuthor && !bApprover && (currentInfo.orginalState != BOEState.Draft && currentInfo.orginalState != BOEState.DraftLocked && currentInfo.orginalState != BOEState.DateShiftDraft))
            {
                var BoeApprovers = _PermissionLoader.GetBOEPermissions(new List<int>() { boeDTO.Id }).Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
                var approvers2 = from a in BoeApprovers
                                 select userDTOLoader.GetUserByID(a.ETIUserId);
                Collection<UserDTO> approvers = new Collection<UserDTO>(approvers2.ToArray());

                // build info for each approver
                foreach (UserDTO approver in approvers)
                {
                    AddEmailInfo(approver.UserID, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, orgSD, orgED, workspaceAdmin, userIsAuthor, true);
                }
            }
        }

        /// <summary>
        /// Modifies the task date
        /// </summary>
        /// <param name="inID">id of the task to be modified</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="boeDTO">boe associated with the task</param>
        /// <returns>always returns true</returns>
        private bool ModifyTaskDate(int inID, int? inStartDateDiff, int? inEndDateDiff, BoeDTO boeDTO)
        {
            if (changeLevel == Level.Task || dateAdjustment == DateAdjustment.Shift ||
                FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual
                || boeDTO.IsSummaryBoe)
            {
                // Uses mediated save to save shifted date in Task and LaborTypes
                BoeTaskElementDTO taskDTO = (from x in taskDTOCollection
                                             where x.Id == inID
                                             select x).FirstOrDefault();

                if (taskDTO != null && taskDTO.TaskElementType != TaskElementType.DTS &&
                        taskDTO.StartDate.HasValue && taskDTO.EndDate.HasValue)
                {
                    if (boeDTO == null)
                    {
                        boeDTO = (from x in this.boeCollection
                                  where x.Id == taskDTO.BoeID
                                  select x).FirstOrDefault();
                    }

                    DateTime boeStartDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.StartDate, DateTimePrecision.Month);
                    DateTime boeEndDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.EndDate, DateTimePrecision.Month);

                    DateTime taskRequestedStartDate = taskDTO.StartDate.Value.AddMonths(inStartDateDiff.Value);
                    if (taskRequestedStartDate != null)
                        taskRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(taskRequestedStartDate, DateTimePrecision.Month);

                    DateTime taskRequestedEndDate = taskDTO.EndDate.Value.AddMonths(inEndDateDiff.Value);
                    if (taskRequestedEndDate != null)
                        taskRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(taskRequestedEndDate, DateTimePrecision.Month);

                    if (taskRequestedStartDate.CompareTo(boeStartDate) < 0 || taskRequestedStartDate.CompareTo(boeEndDate) > 0 ||
                        taskRequestedEndDate.CompareTo(boeStartDate) < 0 || taskRequestedEndDate.CompareTo(boeEndDate) > 0 ||
                        taskRequestedStartDate.CompareTo(taskRequestedEndDate) > 0 || taskDTO.IsSummaryTaskElement)
                    {
                        taskDTO.StartDate = boeStartDate;
                        taskDTO.EndDate = boeEndDate;
                    }
                    else
                    {
                        taskDTO.StartDate = taskRequestedStartDate;
                        taskDTO.EndDate = taskRequestedEndDate;
                    }

                    taskDTO.Updateable = UpdateType.Upsert;

                    if (FlowdownUpdateSelection == FlowdownUpdateType.Automatic)
                    {
                        // Adjust Task Element Labors
                        ModifyTaskElementLabors(inStartDateDiff, inEndDateDiff, taskDTO);
                    }

                    this.tasksToSave.Add(taskDTO);
                }
            }

            return true;
        }

        /// <summary>
        /// Modifies the date on a travel dto
        /// </summary>
        /// <param name="inID">id of the travel dto to be modified</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="boeDTO">boe associated with the travel</param>
        /// <returns>always returns true</returns>
        private bool ModifyTravelDate(int inID, int? inStartDateDiff, int? inEndDateDiff, BoeDTO boeDTO)
        {
            if (changeLevel == Level.Travel || dateAdjustment == DateAdjustment.Shift ||
                FlowdownUpdateSelection == FlowdownUpdateType.Automatic || FlowdownUpdateSelection == FlowdownUpdateType.Manual)
            {
                TravelDTO travelDTO = (from x in travelDTOCollection
                                       where x.Id == inID
                                       select x).FirstOrDefault();

                if (boeDTO == null)
                {
                    boeDTO = (from x in this.boeCollection
                              where x.Id == inID
                              select x).FirstOrDefault();
                }

                DateTime boeStartDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.StartDate, DateTimePrecision.Month);
                DateTime boeEndDate = GenBOEUtilities.AdjustDateTimePrecision(boeDTO.EndDate, DateTimePrecision.Month);

                if (travelDTO.StartDate.HasValue && travelDTO.EndDate.HasValue)
                {
                    DateTime TravelRequestedStartDate = travelDTO.StartDate.Value.AddMonths(inStartDateDiff.Value);
                    if (TravelRequestedStartDate != null)
                        TravelRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(TravelRequestedStartDate, DateTimePrecision.Month);

                    DateTime TravelRequestedEndDate = travelDTO.EndDate.Value.AddMonths(inEndDateDiff.Value);
                    if (TravelRequestedEndDate != null)
                        TravelRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(TravelRequestedEndDate, DateTimePrecision.Month);

                    if (TravelRequestedStartDate.CompareTo(boeStartDate) < 0 || TravelRequestedStartDate.CompareTo(boeEndDate) > 0 ||
                        TravelRequestedEndDate.CompareTo(boeStartDate) < 0 || TravelRequestedEndDate.CompareTo(boeEndDate) > 0 ||
                        TravelRequestedStartDate.CompareTo(TravelRequestedEndDate) > 0)
                    {
                        travelDTO.StartDate = boeStartDate;
                        travelDTO.EndDate = boeEndDate;
                    }
                    else
                    {
                        travelDTO.StartDate = TravelRequestedStartDate;
                        travelDTO.EndDate = TravelRequestedEndDate;
                    }

                    travelDTO.Updateable = UpdateType.Upsert;

                    this.travelsToSave.Add(travelDTO);
                }
            }

            return true;
        }

        /// <summary>
        /// Modifies the task elements labors.
        /// </summary>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="taskDTO">task whose task labors need to be modified</param>
        private void ModifyTaskElementLabors(int? inStartDateDiff, int? inEndDateDiff, BoeTaskElementDTO taskDTO)
        {
            Collection<ResourceTypeDto> inTaskElementLabors = taskDTO.taskElementLabors;

            if (inTaskElementLabors != null)
            {
                decimal tempTotal = 0;

                foreach (ResourceTypeDto laborType in inTaskElementLabors)
                {
                    if (laborType.StartDate.HasValue && laborType.EndDate.HasValue)
                    {
                        DateTime laborTypeRequestedStartDate = laborType.StartDate.Value.AddMonths(inStartDateDiff.Value);
                        if (laborTypeRequestedStartDate != null)
                            laborTypeRequestedStartDate = GenBOEUtilities.AdjustDateTimePrecision(laborTypeRequestedStartDate, DateTimePrecision.Month);

                        DateTime laborTypeRequestedEndDate = laborType.EndDate.Value.AddMonths(inEndDateDiff.Value);
                        if (laborTypeRequestedEndDate != null)
                            laborTypeRequestedEndDate = GenBOEUtilities.AdjustDateTimePrecision(laborTypeRequestedEndDate, DateTimePrecision.Month);

                        if (laborTypeRequestedStartDate.CompareTo(taskDTO.StartDate) < 0 || laborTypeRequestedStartDate.CompareTo(taskDTO.EndDate) > 0 ||
                            laborTypeRequestedEndDate.CompareTo(taskDTO.StartDate) < 0 || laborTypeRequestedEndDate.CompareTo(taskDTO.EndDate) > 0 ||
                            laborTypeRequestedStartDate.CompareTo(laborTypeRequestedEndDate) > 0)
                        {
                            laborType.StartDateValue = taskDTO.StartDate.Value;
                            laborType.EndDateValue = taskDTO.EndDate.Value;
                        }
                        else
                        {
                            laborType.StartDateValue = laborTypeRequestedStartDate;
                            laborType.EndDateValue = laborTypeRequestedEndDate;
                        }

                        laborType.Updateable = UpdateType.Upsert;
                        if (laborType.TaskElementId == 0)
                        {
                            // make sure the dto points to the parent
                            laborType.TaskElementId = taskDTO.Id;
                        }

                        if (laborType.SpreadCurveID != SpreadCurves.DiscreteHours && laborType.SpreadCurveID != SpreadCurves.DiscreteCost)
                        {
                            LaborSpreadRequest tempLSR = new LaborSpreadRequest
                            {
                                CurveID = laborType.SpreadCurveID,
                                StartDate = laborType.StartDateValue,
                                EndDate = laborType.EndDateValue,
                                HourSpread = (laborType.ValueSpread.HasValue ? laborType.ValueSpread.Value : 0)
                            };
                            int decimalPrecision = laborType.SpreadType == SpreadType.Cost ? wsObject.CostDecimalPrecision : wsObject.DecimalPrecision;
                            laborType.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(tempLSR, decimalPrecision);
                            
                            foreach (ResourceSpreadDto spread in laborType.LaborSpreads)
                            {
                                spread.BoeID = laborType.BoeID;
                                spread.Updateable = UpdateType.Upsert;
                                spread.LaborTypeId = laborType.Id;
                            }
                        }
                        else
                        {
                            if (DiscreteResourceSelection == HandleDiscrete.Curve)
                            {
                                LaborSpreadRequest tempLSR = new LaborSpreadRequest 
                                { 
                                    CurveID = SpreadCurves.SpreadCurve3, 
                                    StartDate = laborType.StartDateValue, 
                                    EndDate = laborType.EndDateValue, 
                                    HourSpread = (laborType.ValueSpread.HasValue ? laborType.ValueSpread.Value : 0) 
                                };

                                int decimalPrecision = laborType.SpreadType == SpreadType.Cost ? wsObject.CostDecimalPrecision : wsObject.DecimalPrecision;
                                laborType.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(tempLSR, decimalPrecision);

                                foreach (ResourceSpreadDto spread in laborType.LaborSpreads)
                                {
                                    spread.BoeID = laborType.BoeID;
                                    spread.Updateable = UpdateType.Upsert;
                                    spread.LaborTypeId = laborType.Id;
                                }
                            }
                            else
                            {
                                tempTotal = 0;

                                DateTime laborSpreadRequestedDate;

                                ICollection<ResourceSpreadDto> currentSpread = laborType.LaborSpreads;

                                // The hour is saved as AM but the CaclulateLaborSpreadsBasedOnCurve returns the hour as PM.
                                // Need to convert to PM to perform compare.
                                foreach (ResourceSpreadDto spread in currentSpread)
                                {
                                    laborSpreadRequestedDate = spread.LaborSpreadDate.AddMonths(inStartDateDiff.Value);
                                    if (laborSpreadRequestedDate != null)
                                        spread.LaborSpreadDate = GenBOEUtilities.AdjustDateTimePrecision(laborSpreadRequestedDate, DateTimePrecision.Month);

                                    if (spread.LaborSpreadDate.CompareTo(laborType.EndDateValue) > 0 && (DiscreteResourceSelection == HandleDiscrete.First || DiscreteResourceSelection == HandleDiscrete.Last))
                                    {
                                        tempTotal += spread.LaborSpreadValue;
                                    }

                                    spread.Updateable = UpdateType.Deleted;
                                }

                                LaborSpreadRequest spreadRequest = new LaborSpreadRequest()
                                {
                                    CurveID = laborType.SpreadCurveID,
                                    HourSpread = 0,
                                    StartDate = laborType.StartDateValue,
                                    EndDate = laborType.EndDateValue
                                };
                                int decimalPrecision = laborType.SpreadType == SpreadType.Cost ? wsObject.CostDecimalPrecision : wsObject.DecimalPrecision;
                                laborType.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(spreadRequest, decimalPrecision);

                                foreach (ResourceSpreadDto spread in laborType.LaborSpreads)
                                {
                                    ResourceSpreadDto matchingSpread = ((from x in currentSpread
                                                                         where x.LaborSpreadDate == spread.LaborSpreadDate
                                                                         select x)).FirstOrDefault();

                                    if (matchingSpread != null)
                                    {
                                        spread.LaborSpreadValue = matchingSpread.LaborSpreadValue;
                                    }

                                    if (DiscreteResourceSelection == HandleDiscrete.First)
                                    {
                                        DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

                                        if (spreadDate.CompareTo(laborType.StartDateValue) == 0)
                                        {
                                            spread.LaborSpreadValue += tempTotal;
                                        }
                                    }
                                    else if (DiscreteResourceSelection == HandleDiscrete.Last)
                                    {
                                        DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

                                        if (spreadDate.CompareTo(laborType.EndDateValue) == 0)
                                        {
                                            spread.LaborSpreadValue += tempTotal;
                                        }
                                    }

                                    spread.Updateable = UpdateType.Upsert;
                                    spread.BoeID = laborType.BoeID;
                                    spread.LaborTypeId = laborType.Id;
                                }

                                laborType.ValueSpread = Utilities.AdjustPrecision(laborType.LaborSpreads.Sum(x => x.LaborSpreadValue), wsObject.DecimalPrecision);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shift dates
        /// </summary>
        /// <param name="inID">id of the object to be modified</param>
        /// <param name="currentLevel">level at which to change dates</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <returns>always returns true</returns>
        private bool ShiftDates(int inID, Level currentLevel, int? inStartDateDiff, int? inEndDateDiff)
        {
            return (ShiftDates(inID, currentLevel, inStartDateDiff, inEndDateDiff, null, null));
        }

        /// <summary>
        /// Shifts the dates of a clin
        /// </summary>
        /// <param name="inID">id of the object to be modified</param>
        /// <param name="currentLevel">level at which the adjust dates was requested</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="inClin">clin</param>
        /// <returns>always returns true</returns>
        private bool ShiftDates(int inID, Level currentLevel, int? inStartDateDiff, int? inEndDateDiff, ClinDTO inClin)
        {
            return (ShiftDates(inID, currentLevel, inStartDateDiff, inEndDateDiff, inClin, null));
        }

        /// <summary>
        /// Shifts the dates of a boe
        /// </summary>
        /// <param name="inID">id of the object to be modified</param>
        /// <param name="currentLevel">level at which the adjust dates was requested</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="inBoe">boe</param>
        /// <returns>always returns true</returns>
        private bool ShiftDates(int inID, Level currentLevel, int? inStartDateDiff, int? inEndDateDiff, BoeDTO inBoe)
        {
            return (ShiftDates(inID, currentLevel, inStartDateDiff, inEndDateDiff, null, inBoe));
        }

        /// <summary>
        /// Shifts dates
        /// </summary>
        /// <param name="inID">id of the object which is the target of the shift date request</param>
        /// <param name="currentLevel">level at which the request was made</param>
        /// <param name="inStartDateDiff">start date difference</param>
        /// <param name="inEndDateDiff">end date difference</param>
        /// <param name="inClin">clin</param>
        /// <param name="inBoe">boe</param>
        /// <returns>always returns true because all the methods it calls always returns true</returns>
        private bool ShiftDates(int inID, Level currentLevel, int? inStartDateDiff, int? inEndDateDiff, ClinDTO inClin, BoeDTO inBoe)
        {
            bool toReturn = false;

            switch (currentLevel)
            {
                case Level.Workspace:
                    // will recursively call the 'ShiftDates' methods to cascade the date change throughout the workspace
                    toReturn = ModifyWorkspaceDate(inID, inStartDateDiff, inEndDateDiff);
                    break;

                case Level.CLINCollection:
                    toReturn = ModifyClinCollectionDates();
                    break;

                case Level.CLIN:
                    toReturn = ModifyClinDate(inID, inStartDateDiff, inEndDateDiff);
                    break;

                case Level.BOE:
                    toReturn = ModifyBoeDate(inID, inStartDateDiff, inEndDateDiff, inClin);
                    break;

                case Level.Task:
                    toReturn = ModifyTaskDate(inID, inStartDateDiff, inEndDateDiff, inBoe);
                    break;

                case Level.Travel:
                    toReturn = ModifyTravelDate(inID, inStartDateDiff, inEndDateDiff, inBoe);
                    break;
            }

            return toReturn;
        }
    }

    /// <summary>
    /// clin date change
    /// </summary>
    public class objCLINDateChange
    {
        public int ClinID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// clin title date change
    /// </summary>
    public class objCLINTitleDateChange
    {
        public int ClinID { get; set; }
        public string ClinTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Clin date collection
    /// </summary>
    public class ClinDateCollection
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClinDateCollection()
        {

        }

        /// <summary>
        /// Clin Id
        /// </summary>
        public int ClinId { get; set; }

        /// <summary>
        /// Clin Title
        /// </summary>
        public string ClinTitle { get; set; }

        /// <summary>
        /// Clin Start Date
        /// </summary>
        public DateTime? ClinStartDate { get; set; }

        /// <summary>
        /// Clin End Date
        /// </summary>
        public DateTime? ClinEndDate { get; set; }

        /// <summary>
        /// Clin Number
        /// </summary>
        public string ClinNumber { get; set; }

    }

    /// <summary>
    /// Used to hold onto data for BOE state transition processing, when saving the data
    /// </summary>
    public class BoeDtoForStateTransition
    {
        public BOEState OldBoeState { get; set; }

        public BoeDTO BoeDto { get; set; }

        public bool StateChangeHappening { get; set; }
    }
}
