// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using Common.Calculations;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public class WBSControllerLogic : IWBSControllerLogic
    {
        private BoeEmailer _Emailer;
        private BOEStateMachine _BOEStateMachine;
        private BoeTaskElementRecalculation _BoeTaskElementRecalculation;
        private IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation;
        private IWorkspaceVariableDTODataLoader _WorkspaceVariableLoader;
        private BoeTaskElementMediator _BoeTaskElementMediator;
        private BoeMediator _BoeMediator;
        private IBoeDTODataLoader boeLoader;
        private IWbsDTODataLoader wbsLoader;
        private IFullObjectFactory _Factory { get; set; }

        public WBSControllerLogic(
            BoeEmailer inEmailer,
            BOEStateMachine inBOEStateMachine,
            BoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IVariableSelectBOEtoSumCalculation inVarSelectBoeToSumCalculation,
            IWorkspaceVariableDTODataLoader inWorkspaceVarLoader,
            BoeTaskElementMediator inBoeTaskElementMediator,
            BoeMediator inBoeMediator,
            IFullObjectFactory factory,
            IBoeDTODataLoader boeLoader,
            IWbsDTODataLoader wbsLoader
            )
        {
            this._BOEStateMachine = inBOEStateMachine;
            this._Emailer = inEmailer;
            this._BoeTaskElementRecalculation = inBoeTaskElementRecalculation;
            this._variableSelectBOEtoSumCalculation = inVarSelectBoeToSumCalculation;
            this._WorkspaceVariableLoader = inWorkspaceVarLoader;
            this._BoeTaskElementMediator = inBoeTaskElementMediator;
            this._BoeMediator = inBoeMediator;
            this.boeLoader = boeLoader;
            this.wbsLoader = wbsLoader;
            this._Factory = factory;

        }
        /// <summary>
        /// Saves updates to the ManageWBS page, Updates, inserts or deletes. 
        /// </summary>
        /// <param name="wbsCollection">Collection of WBS's to update</param>
        /// <param name="ws">the full workspace</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void SaveManageWBS(Collection<ManageWBSModelView> wbsCollection, FullWorkspace ws)
        {
            if (wbsCollection == null)
            {
                throw new ArgumentNullException(nameof(wbsCollection));
            }
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            // if the WBS # is changed, this could effect any BOE Task Element that has a task sum of BOE variable that uses this WBS
            List<int> WbsIDsToRecalculateLaborSpread = new List<int>();
            //associated BOEs to the WBS that should be sent an email if WBS # or title has changed

            Collection<BoeDTO> newBOEs = new Collection<BoeDTO>();
            Collection<FullBoe> boesWithStateChange = new Collection<FullBoe>();
            Collection<FieldChanged> changed = new Collection<FieldChanged>();



            ICollection<FullWbs> wbsObjects = this._Factory.CreateFullWbses(wbsCollection.Select(x => x.WbsID).ToList());

            Collection<WbsDTO> wbsDTOs = this.ConvertToWBSDTO(wbsCollection, ws, WbsIDsToRecalculateLaborSpread, newBOEs, boesWithStateChange, changed, wbsObjects);

            // BOE's Labor Spread values that need to be recalculated because of any changes made during this save
            List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

            // Dictionary to keep track of task variable IDs that need to be updated and their old variable total
            Dictionary<int, decimal> WorkspaceVarOldValueD = new Dictionary<int, decimal>();

            ICollection<FullWbs> wbsObjectsForLaborSpreadRecalculation = this._Factory.CreateFullWbses(WbsIDsToRecalculateLaborSpread);


            //get the potential multiboes
            Collection<FullBoe> MultiBOEs = ws.Boes.Where(x => x.IsMultiClinWbs).ToCollection();

            //find any boes that have resources using the clin
            //for each wbs lets check to see if its being used by a multiboe
            Collection<FullBoe> boesUsingWbs = 
                            (from b in MultiBOEs
                            from l in b.TaskElements
                            from x in l.taskElementLabors
                            where x.WBSID.HasValue && wbsObjects.Any(z => z.Id == x.WBSID)
                            select b).ToCollection<FullBoe>(); 
       
            //filter out the duplicates 
            boesUsingWbs = boesUsingWbs.Distinct().ToCollection();

            foreach (FullWbs wbs in wbsObjectsForLaborSpreadRecalculation)
            {
                if (wbs.WorkspaceVariableIds.Any())
                {
                    ICollection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(i => wbs.WorkspaceVariableIds.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
                    foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
                    {
                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

                        decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                        WorkspaceVarOldValueD.Add(workspaceVar.Id, oldTotalValue);
                    }
                }
            }

            List<ChangedValueContainer> SendBOEUpdatedToAuthorsAndApproversEmails = new List<ChangedValueContainer>();

            ws.LoadBoes();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.wbsLoader.Save(wbsDTOs);

                foreach (FullWbs wbsObject in wbsObjectsForLaborSpreadRecalculation)
                {
                   
                    boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Task, ws));
                    boeTaskElementsToRecalculate.AddRange(this._BoeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Workspace, ws));
                }

                // get the unique BOE IDs from boeTaskElementsToRecalculate so we can set their state back to Draft
                var BoeIDsToCheck = new Collection<int>(boeTaskElementsToRecalculate.Where(x => x.BoeID > 0).Select(x => x.BoeID).ToList());

                ICollection<FullBoe> boes = ws.Boes.Where(x => BoeIDsToCheck.Contains(x.Id)).ToList();
                //get all the boes that need to be sent back to draft.
               boes= boes.Concat(boesUsingWbs).ToCollection();
            

                foreach (FullBoe boe in boes)
                {
                    BOEState oldBOEState = boe.State;
                    if (boe.State == BOEState.Approved || boe.State == BOEState.AwaitingApproval || boe.State == BOEState.DraftLocked)
                    {
                        BOEState newBOEState = BOEState.Draft;

                        // Validate the Awaiting Approval or Approved to Draft state transition
                        string validationMessage;

                        if (!this._BOEStateMachine.PerformStateTransitionValidation(boe, ws, oldBOEState, newBOEState, out validationMessage))
                        {
                            // not valid ... communicate to user
                            throw new GenValidationException(validationMessage);
                        }

                        // If the transition is valid, set the BOE to Draft and save it
                        boe.Updateable = UpdateType.Upsert;
                        boe.State = newBOEState;
                        this._BoeMediator.MediatedSave(ws, boe);

                        // Perform common state transition actions
                        this._BOEStateMachine.PerformStateTransitionAction(boe, ws, oldBOEState, boe.State);
                    }
                }

                foreach (FullBoe boe in boesWithStateChange)
                {
                    BOEState currentState = boe.State;
                    BOEState newBOEState = BOEState.Draft;
                    // move the BOE back to DRAFT
                    boe.State = newBOEState;

                    changed.Add(new FieldChanged
                    {
                        Field = "Status",
                        OldValue = currentState.ToString(),
                        NewValue = boe.State.ToString()
                    });

                    boe.Updateable = UpdateType.Upsert;

                    string errorMessage;
                    if (this._BOEStateMachine.PerformStateTransitionValidation(boe, ws, currentState, boe.State, out errorMessage))
                    {
                        this._BoeMediator.MediatedSave(ws, boe);

                        // save of BOE worked .. perform transition steps and send email
                        this._BOEStateMachine.PerformStateTransitionAction(boe, ws, currentState, boe.State);

                        SendBOEUpdatedToAuthorsAndApproversEmails.Add(new ChangedValueContainer
                        {
                            BOE = boe,
                            Changes = changed
                        });
                    }
                    else
                    {
                        // we can't move the BOE back to DRAFT for some reason ... abort
                        // pull this error message from the state machine itself
                        throw new ValidationException(errorMessage);
                    }
                }

                this._BoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);
                if (WorkspaceVarOldValueD.Any())
                {
                    ICollection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(i => WorkspaceVarOldValueD.Keys.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
                    foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
                    {
                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

                        workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                        workspaceVar.Updateable = UpdateType.Upsert;
                        this._WorkspaceVariableLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
                    }
                }
                scope.Complete();
            }

            // Send BOE E-mails
            foreach (ChangedValueContainer SendBOEUpdatedToAuthorsAndApproversEmail in SendBOEUpdatedToAuthorsAndApproversEmails)
            {
                this._Emailer.SendBOEUpdatedToAuthorsAndApprovers(this._Factory.CreateFullBoe(SendBOEUpdatedToAuthorsAndApproversEmail.BOE), SendBOEUpdatedToAuthorsAndApproversEmail.Changes);
            }

            if (newBOEs.Any())
            {
                this.SaveNewBOEs(ws, newBOEs);
            }
        }

        /// <summary>
        /// Saves BOEs created from the ManageWBS Page
        /// </summary>
        /// <param name="ws">current workspace</param>
        /// <param name="newBOEs">new boeDTO's </param>
        /// <returns></returns>
        private Collection<BoeDTO> SaveNewBOEs(FullWorkspace ws, Collection<BoeDTO> newBOEs)
        {
            foreach (BoeDTO newBOE in newBOEs)
            {
                int xrefID = this.boeLoader.GetWbsClinBoeXrefId(newBOE.WBSID, newBOE.CLINID, null);
                newBOE.WCBID = xrefID;
            }

            newBOEs = new Collection<BoeDTO>((from b in newBOEs
                                              where b.WCBID != 0
                                              select b).ToArray());

            if (newBOEs.Any())
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this._BoeMediator.MediatedSaveBOEs(ws, newBOEs);
                    scope.Complete();
                }
            }
            return newBOEs;
        }

        /// <summary>
        /// Converts ManageWBSModelView to WBSDTO
        /// </summary>
        /// <param name="wbsCollection">ManageWBSModelView collection</param>
        /// <param name="ws">current workspace</param>
        /// <param name="WbsIDsToRecalculateLaborSpread">WBS updated</param>
        /// <param name="newBOEs">NewBOES</param>
        /// <param name="boesWithStateChange">BOE's Chaning</param>
        /// <param name="changed">Field Change collection</param>
        /// <param name="wbsObjects">full objects</param>
        /// <returns></returns>
        private Collection<WbsDTO> ConvertToWBSDTO(Collection<ManageWBSModelView> wbsCollection, FullWorkspace ws, List<int> WbsIDsToRecalculateLaborSpread,  Collection<BoeDTO> newBOEs, Collection<FullBoe> boesWithStateChange, Collection<FieldChanged> changed, ICollection<FullWbs> wbsObjects)
        {
            int boeIDSeed = -1;
            Collection<WbsDTO> wbsDTOs = new Collection<WbsDTO>();
            foreach (ManageWBSModelView manageWbsModelView in wbsCollection)
            {
                bool wbsNumberChanged = false;
                bool wbsTitleChanged = false;

                FullWbs wbs;
                if (manageWbsModelView.WbsID < 0 && !manageWbsModelView.Deleted)
                {
                    wbs = this._Factory.CreateFullWbs();
                    wbs.Updateable = UpdateType.Upsert;
                    wbs.Id = manageWbsModelView.WbsID;
                    wbs.WorkspaceID = ws.Id;
                }
                else
                {
                    wbs = wbsObjects.First(x => x.Id == manageWbsModelView.WbsID);
                    DataRelationshipVerifier.VerifyDataRelation(wbs, ws.Id);

                    //mktodo check boes in workspace to see if they are multi and using 

                    if (manageWbsModelView.Deleted)
                    {
                        wbs.Updateable = UpdateType.Deleted;
                    }
                    else
                    {
                        wbs.Updateable = UpdateType.Upsert;
                    }

                    if (wbs.WbsNumber != manageWbsModelView.WbsNumber)
                    {
                        changed.Add(new FieldChanged
                        {
                            Field = "WBS #",
                            OldValue = wbs.WbsNumber,
                            NewValue = manageWbsModelView.WbsNumber
                        });

                        wbsNumberChanged = true;
                    }

                    if (wbs.WbsTitle != manageWbsModelView.WbsTitle)
                    {
                        changed.Add(new FieldChanged
                        {
                            Field = "WBS Title",
                            OldValue = wbs.WbsTitle,
                            NewValue = manageWbsModelView.WbsTitle
                        });

                        wbsTitleChanged = true;
                    }
                }

                // if the WBS# has changed, need to recalculate labor spread for any BOE that had either the old WBS or the new WBS referenced
                // need to find the parent and children of the old wbs and the new wbs
                if (wbsNumberChanged)
                {
                    FullWbs wbsObject = this._Factory.CreateFullWbs(wbs);

                    WbsIDsToRecalculateLaborSpread.Add(wbs.Id);
                    WbsIDsToRecalculateLaborSpread.AddRange(wbsObject.AllParentWbs.Select(a => a.Id).ToList());
                }

                wbs.WbsNumber = manageWbsModelView.WbsNumber;
                wbs.WbsTitle = manageWbsModelView.WbsTitle;
                wbs.UpdateDate = manageWbsModelView.UpdateDate;
                
                if (manageWbsModelView.ClinIDs != null && manageWbsModelView.ClinIDs.Any())
                {
                    Collection<int> wbsCLINs = new Collection<int>();
                    foreach(int clinID in manageWbsModelView.ClinIDs)  // for each NEWLY-selected CLIN
                    {
                        ClinDTO selectedClin = this._Factory.CreateFullClin(clinID);
                        DataRelationshipVerifier.VerifyDataRelation(selectedClin, ws.Id);
                        wbsCLINs.Add(clinID);

                        if (manageWbsModelView.HasBOE && !manageWbsModelView.Deleted && manageWbsModelView.WbsID >= 0 && !wbs.ClinIDs.Contains(clinID))
                        {
                            // Business rule:  Automatically create a BOE when a new CLIN is assigned to a WBS that already has associated BOEs.
                            // This seems to have existed since v1.0, circa March 2011.

                            BoeDTO newBOE = new BoeDTO();

                            newBOE.Id = boeIDSeed;
                            newBOE.CLINID = selectedClin.Id;
                            newBOE.WBSID = wbs.Id;
                            newBOE.WorkspaceID = ws.Id;
                            newBOE.StartDate = selectedClin.StartDate.HasValue ? selectedClin.StartDate.Value : ws.ContractStartDate;
                            newBOE.EndDate = selectedClin.EndDate.HasValue ? selectedClin.EndDate.Value : ws.ContractEndDate;
                            newBOE.Updateable = UpdateType.Upsert;
                            newBOE.WCBID = boeIDSeed--;

                            newBOEs.Add(newBOE);
                        }
                    }

                    wbs.ClinIDs = wbsCLINs;
                }
                else
                {
                    wbs.ClinIDs = new Collection<int>();
                }

                // if the number/title changed, need to move the associated BOEs back to draft.                  
                if (manageWbsModelView.HasBOE && (wbsNumberChanged || wbsTitleChanged))
                {
                    foreach (FullBoe boe in wbs.Boes)
                    {
                        boesWithStateChange.Add(boe);
                    }
                }

                wbsDTOs.Add(wbs);
            }
            return wbsDTOs;
        }

        /// <summary>
        /// ChangedValueContainer - pulled from the front end. 
        /// </summary>
        private struct ChangedValueContainer
        {
            public BoeDTO BOE { get; set; }
            public Collection<FieldChanged> Changes { get; set; }
        }
    }
}

