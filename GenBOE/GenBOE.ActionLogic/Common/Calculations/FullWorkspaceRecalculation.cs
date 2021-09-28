// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;

    /// <summary>
    /// Used to recalculate the entire workspace
    /// 
    /// Class Usage -> Need to run methods in the following order:
    ///     1. (NO DB data modification) RecalculateLaborInAWorkspaceWithoutSaving - this will put all the new data (that needs to be saved) into the 3 Hashsets
    ///     2. (NO DB data modification) ValidateStateTransitionForBoesEffectedByRecalculation - validate that the Boes can be transitioned correctly
    ///     3. (DB data modified, should be done in a transaction) SaveDataEffectedByRecalculation - saves all the data from recalculation
    ///     4. (NO DB data modification) PerformStateTransitionActionsForBoesEffectedByRecalculation - sends out emails and performs any other cleanup due to the state transition changes
    /// 
    /// </summary>
    public class FullWorkspaceRecalculation : IFullWorkspaceRecalculation
    {
        #region Properties & Ctor

        /// <summary>
        /// Used for error messages communicated back to the user
        /// </summary>
        private static string ERROR_MESSAGE_TEXT = "During recalculation, a validation exception was encountered: ";

        /// <summary>
        /// Boe State Machine
        /// </summary>
        private IBOEStateMachine BoeStateMachine { get; set; }

        /// <summary>
        /// Boe Mediator
        /// </summary>
        private IBoeMediator BoeMediator { get; set; }

        /// <summary>
        /// Boe Task Element Mediator
        /// </summary>
        private IBoeTaskElementMediator BoeTaskElementMediator { get; set; }

        /// <summary>
        /// Variable Select BOE to Sum Calculation
        /// </summary>
        private IVariableSelectBOEtoSumCalculation VariableSelectBOEtoSumCalculation { get; set; }

        /// <summary>
        /// Workspace Variable Loader
        /// </summary>
        private IWorkspaceVariableDTODataLoader WorkspaceVarLoader { get; set; }

        /// <summary>
        /// Boe Task Element Recalculation
        /// </summary>
        private IBoeTaskElementRecalculation TaskElementRecalculation { get; set; }

        /// <summary>
        /// Public Ctor
        /// </summary>
        public FullWorkspaceRecalculation(IWorkspaceVariableDTODataLoader wsVariableLoader, IBoeTaskElementRecalculation taskElementRecalculation, IBOEStateMachine boeStateMachine,
            IBoeMediator boeMediator, IBoeTaskElementMediator taskElementMediator, IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation)
        {
            this.WorkspaceVarLoader = wsVariableLoader;
            this.TaskElementRecalculation = taskElementRecalculation;
            this.BoeStateMachine = boeStateMachine;
            this.BoeMediator = boeMediator;
            this.BoeTaskElementMediator = taskElementMediator;
            this.VariableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
        }

        #endregion

        #region WS Recalculation, boe state validation and transition & saving of all data

        /// <summary>
        /// Will recalculate task element resources in a WS that have a rate type of cost. 
        /// </summary>
        /// <param name="ws">Workspace to recalculate cost resources.</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later.</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        /// <param name="costPrecision">The current cost decimal precision.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void RecalculateCostInAWorkspaceWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<FullBoe> boesToTransition, int costPrecision)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (tasksToSave == null)
            {
                tasksToSave = new HashSet<BoeTaskElementDTO>();
            }
            if (boesToTransition == null)
            {
                boesToTransition = new HashSet<FullBoe>();
            }

            ICollection<BoeTaskElementDTO> taskElementsWithCostSpreads = ws.TaskElements.Where(x => x.taskElementLabors.Any(l => l.SpreadType == SpreadType.Cost)).ToCollection<BoeTaskElementDTO>();
            foreach(BoeTaskElementDTO taskElement in taskElementsWithCostSpreads)
            {
                FullBoe boeToSave = ws.Boes.First(i => i.Id == taskElement.BoeID);
                // Go thru each of the cost resources in the task element and readjust the spreads to the new workspace cost precision.
                foreach (ResourceTypeDto resource in taskElement.taskElementLabors.Where(z => z.SpreadType == SpreadType.Cost).ToCollection())
                {
                    if (resource.SpreadCurveID == SpreadCurves.DiscreteCost)
                    {
                        // If the old precision is 2 and was changed to 0.
                        if (costPrecision == 0)
                        {
                            // Only update discrete spreads if the precision is changing from 2 to 0. 0 to 2 does not require spreads to be updated.
                            foreach (ResourceSpreadDto spread in resource.LaborSpreads)
                            {
                                // Round each discrete spread to the new precision. Note this will likely change the eventual total cost.
                                decimal newSpreadValue = Utilities.AdjustPrecision(spread.LaborSpreadValue, costPrecision);

                                spread.LaborSpreadValue = newSpreadValue;
                                spread.Updateable = UpdateType.Upsert;
                            }
                            // Sum up the newly adjusted spreads and update the total cost for the resource.
                            resource.ValueSpread = resource.LaborSpreads.Sum(s => s.LaborSpreadValue);
                        }
                        resource.Updateable = UpdateType.Upsert;
                    }
                    else
                    {
                        // Re-spread the costs using the new precision.
                        this.GetNewLaborSpreadsOnCurve(resource, costPrecision);
                        resource.Updateable = UpdateType.Upsert;
                    }
                }

                if (!tasksToSave.Contains(taskElement))
                {
                    taskElement.Updateable = UpdateType.Upsert;
                    tasksToSave.Add(taskElement);
                }

                // determine if boe state needs to change. if a variable causes the task element to be recalculated and the boe is in awaiting approval/approved, it needs
                // to go back to draft
                if (boeToSave.State == BOEState.AwaitingApproval || boeToSave.State == BOEState.Approved || boeToSave.State == BOEState.DraftLocked)
                {
                    boesToTransition.RemoveWhere(x => x.Id == boeToSave.Id);
                    boesToTransition.Add(boeToSave);
                }
            }
        }

        /// <summary>
        /// Will recalculate everything in a WS that has to do w/ the labor.. Variables (Workspace and Task/Ordinary), labor... all the way down to spreads
        /// </summary>
        /// <param name="ws">Workspace to recalculate</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later</param>
        /// <param name="workspaceVariablesToSave">WS Variables that will be updated with data to be saved later</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        public void RecalculateLaborInAWorkspaceWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition)
        {
            try
            {
                if (ws == null)
                {
                    throw new ArgumentNullException(nameof(ws));
                }

                // Move task elements that use Ordinary Variables to the top of the list; this helps ensure that ordinary variables get recalculated before they are used
                List<BoeTaskElementDTO> taskElements = ws.TaskElements.ToList();

                this.RecalculateLaborInTaskElementsWithoutSaving(ws, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, taskElements);
            }
            catch (GenValidationException ex)
            {
                throw new GenValidationException(ERROR_MESSAGE_TEXT + ex.Message);
            }
            catch (ValidationException ex)
            {
                throw new GenValidationException(ERROR_MESSAGE_TEXT + ex.Message);
            }
        }

        /// <summary>
        /// Will recalculate the list of task elements and any other impacted data - task elements as well as variables (Workspace and Task/Ordinary)
        /// </summary>
        /// <param name="ws">Workspace to recalculate</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later</param>
        /// <param name="workspaceVariablesToSave">WS Variables that will be updated with data to be saved later</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        public void RecalculateLaborInTaskElementsWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave,
            ref HashSet<FullBoe> boesToTransition, ICollection<BoeTaskElementDTO> taskElements)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (tasksToSave == null)
            {
                tasksToSave = new HashSet<BoeTaskElementDTO>();
            }
            if (workspaceVariablesToSave == null)
            {
                workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
            }
            if (boesToTransition == null)
            {
                boesToTransition = new HashSet<FullBoe>();
            }

            // Perform recalculation
            this.RecalculateTaskElements(taskElements.ToCollection(), ws, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition);

            #region Manual fix for Resource Types with discrete labor curves

            // Need to adjust all spreads from resource types with discrete spread curves.. in case they need to be rounded off due to precision change
            foreach (ResourceSpreadDto spread in tasksToSave
                .SelectMany(x => x.taskElementLabors).Where(z => z.SpreadCurveID == SpreadCurves.DiscreteHours)
                .SelectMany(x => x.LaborSpreads).Where(z => z.Updateable != UpdateType.Deleted).ToCollection())
            {
                decimal newValue = Utilities.AdjustPrecision(spread.LaborSpreadValue, ws.DecimalPrecision);

                spread.LaborSpreadValue = newValue;
                spread.Updateable = UpdateType.Upsert;
            }

            // now need to adjust all of the resource type values for the ones with discrete spread curves, so that way they are added up right
            foreach (ResourceTypeDto resourceType in tasksToSave
                .SelectMany(x => x.taskElementLabors).Where(z => z.SpreadCurveID == SpreadCurves.DiscreteHours).ToCollection())
            {
                decimal newValue = resourceType.LaborSpreads.Sum(x => x.LaborSpreadValue);

                BoeTaskElementDTO taskElement = tasksToSave.First(x => x.taskElementLabors.Any(z => z.Id == resourceType.Id));

                decimal moqTotal = this.CalculateMoqTotal(ws, taskElement);
                resourceType.ValueSpread = newValue;
                resourceType.PercentSpread = 0m;
                resourceType.Updateable = UpdateType.Upsert;

                if (moqTotal != 0)
                {
                    decimal percentSpread = Utilities.AdjustPrecision(newValue / moqTotal * 100m, 3); // percent precision is always 3
                    resourceType.PercentSpread = percentSpread;
                }
            }

            #endregion
        }

        #region Calculation Helpers

        /// <summary>
        /// Calculate any linked task elements that need to be updated based on the workspace variable
        /// </summary>
        /// <param name="inTaskElementsToSave">Task elements to save</param>
        /// <param name="inWorkspace">Workspace</param>
        private void RecalculateTaskElements(Collection<BoeTaskElementDTO> inTaskElementsToSave, FullWorkspace inWorkspace, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition)
        {
            Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave = new Collection<BoeTaskElementDTO>(); // keeps track of Task Elements that were recalculated during one of the passes

            // Setup hashsets that are queried a lot, to cut down on time in the recursive calls
            HashSet<FullBoe> wsBoes = new HashSet<FullBoe>(inWorkspace.Boes);
            HashSet<WorkspaceVariableDTO> wsVarsHash = new HashSet<WorkspaceVariableDTO>(inWorkspace.WorkspaceVariables);

            foreach (BoeTaskElementDTO taskElement in inTaskElementsToSave)
            {
                // need to save this task element
                tasksToSave.RemoveWhere(x => x.Id == taskElement.Id);
                tasksToSave.Add(taskElement);

                Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
                Collection<WorkspaceVariableDTO> workspaceVariables = new Collection<WorkspaceVariableDTO>();

                // check if labor recalculation is needed given a TASK variable. if it is, it will be saved
                Collection<BoeTaskElementDTO> temp = new Collection<BoeTaskElementDTO>() { this.TaskElementRecalculation.RecalculateLaborWithTaskElement(taskElement, VariableType.Task, inWorkspace, boeTaskElements) };
                this.RecalculateBasedOnSumToBOEVariable(inWorkspace, inOtherBoeTaskElementToSave, temp, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, boeTaskElements, workspaceVariables);

                // check if labor recalculation is needed given a WORKSPACE variable. if it is, it will be saved
                temp = new Collection<BoeTaskElementDTO>() { this.TaskElementRecalculation.RecalculateLaborWithTaskElement(taskElement, VariableType.Workspace, inWorkspace, boeTaskElements) };
                this.RecalculateBasedOnSumToBOEVariable(inWorkspace, inOtherBoeTaskElementToSave, temp, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, boeTaskElements, workspaceVariables);
            }
        }

        /// <summary>
        /// check if labor recalculation is needed for a boe to sum workspace/task variable based off of the boe
        /// </summary>
        /// <param name="workspace">Full workspace</param>
        /// <param name="inOtherBoeTaskElementToSave">Other elements that are going to be saved; this is used to make sure we don't recalculate the same item more than once</param>
        /// <param name="inNextSetToCheck">The next set of elements to check</param>
        /// <param name="tasksToSave">Tasks that will be saved in the end</param>
        /// <param name="workspaceVariablesToSave">WS Variables that will be saved in the end</param>
        /// <param name="boesToTransition">Boes that will be transitioned and saved in the end</param>
        /// <param name="wsVarsHash">Workspace Variables that belong to the WS</param>
        /// <param name="wsBoes">Boes that belong to the WS</param>
        /// <param name="incomingBoeTaskElements">Any incoming BOE Task Elements</param>
        /// <param name="incomingWorkspaceVariables">Any incoming WS variables</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
        public void RecalculateBasedOnSumToBOEVariable(FullWorkspace workspace, Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, Collection<BoeTaskElementDTO> inNextSetToCheck,
            ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition, 
            HashSet<WorkspaceVariableDTO> wsVarsHash, HashSet<FullBoe> wsBoes,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (inOtherBoeTaskElementToSave == null)
            {
                throw new ArgumentNullException(nameof(inOtherBoeTaskElementToSave));
            }
            if (inNextSetToCheck == null)
            {
                throw new ArgumentNullException(nameof(inNextSetToCheck));
            }
            if (wsBoes == null)
            {
                throw new ArgumentNullException(nameof(wsBoes));
            }

            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }
            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }
            if (tasksToSave == null)
            {
                tasksToSave = new HashSet<BoeTaskElementDTO>();
            }
            if (workspaceVariablesToSave == null)
            {
                workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
            }
            if (boesToTransition == null)
            {
                boesToTransition = new HashSet<FullBoe>();
            }
            if (wsVarsHash == null)
            {
                wsVarsHash = new HashSet<WorkspaceVariableDTO>();
            }

            foreach (BoeTaskElementDTO task in inNextSetToCheck)
            {
                bool containsTaskAlready = inOtherBoeTaskElementToSave.Any(x => x.Id == task.Id);

                if (!containsTaskAlready)
                {
                    // get boe to determine if state needs to change. if a variable causes the task element to be recalculated and the boe is in awaiting approval/approved, it needs
                    // to go back to draft
                    FullBoe boeToSave = wsBoes.First(i => i.Id == task.BoeID);

                    inOtherBoeTaskElementToSave.Add(task);

                    if (boeToSave.State == BOEState.AwaitingApproval || boeToSave.State == BOEState.Approved || boeToSave.State == BOEState.DraftLocked)
                    {
                        boesToTransition.RemoveWhere(x => x.Id == boeToSave.Id);
                        boesToTransition.Add(boeToSave);
                    }

                    // Save the updated BOE Task Element
                    tasksToSave.RemoveWhere(x => x.Id == task.Id);
                    tasksToSave.Add(task);

                    // Need to makes sure any workspace variables that are based on this BOE are updated as well
                    ICollection<WorkspaceVariableDTO> workspaceVariables = wsVarsHash.Where(i => boeToSave.WorkspaceVariablesIdsForBoe.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();

                    if (workspaceVariables.Any())
                    {
                        foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
                        {
                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, workspace);

                            workspaceVar.WorkspaceVariableValue = this.VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                            workspaceVar.Updateable = UpdateType.Upsert;

                            workspaceVariablesToSave.RemoveWhere(x => x.Id == workspaceVar.Id);
                            workspaceVariablesToSave.Add(workspaceVar);
                        }
                    }

                    // recursive call to make sure the last updated task element doesn't effect another task element
                    this.RecalculateBasedOnSumToBOEVariable(workspace, inOtherBoeTaskElementToSave,
                        this.TaskElementRecalculation.RecalculateLaborWithBoe(boeToSave, VariableType.Task, workspace, incomingBoeTaskElements, incomingWorkspaceVariables),
                        ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, incomingBoeTaskElements, incomingWorkspaceVariables);
                    this.RecalculateBasedOnSumToBOEVariable(workspace, inOtherBoeTaskElementToSave,
                        this.TaskElementRecalculation.RecalculateLaborWithBoe(boeToSave, VariableType.Workspace, workspace, incomingBoeTaskElements, incomingWorkspaceVariables),
                        ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, incomingBoeTaskElements, incomingWorkspaceVariables);
                }
            }
        }

        /// <summary>
        /// Recomputes the labor spreads for the given resource and precision. Updates the resource with new spreads and the old spreads are set for
        /// deletion on save.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="precision">Precision to use on spreads.</param>
        private void GetNewLaborSpreadsOnCurve(ResourceTypeDto resource, int precision)
        {
            // Costs are spread on a curve. Re-spread over the curve using the new cost precision.
            decimal newTotal = Utilities.AdjustPrecision(resource.ValueSpread.HasValue ? resource.ValueSpread.Value : 0, precision);

            LaborSpreadRequest laborSpreadRequest = new LaborSpreadRequest();
            // get labor spread request
            laborSpreadRequest.CurveID = resource.SpreadCurveID;
            laborSpreadRequest.StartDate = (DateTime)resource.StartDate;
            laborSpreadRequest.EndDate = (DateTime)resource.EndDate;
            laborSpreadRequest.HourSpread = newTotal;

            // Recalculate Labor Spread
            resource.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, precision);
            resource.ValueSpread = newTotal;
            resource.LaborSpreads.ToList().ForEach(s => s.Updateable = UpdateType.Upsert);
        }

        #endregion

        /// <summary>
        /// Validate Boe's State Transition for Boes that will be effected by recalculation
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boesToValidate">Boes to validate</param>
        /// <param name="originalBoes">Original Boes</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Boe")]
        public void ValidateStateTransitionForBoesEffectedByRecalculation(FullWorkspace ws, HashSet<FullBoe> boesToValidate, HashSet<BoeDTO> originalBoes)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (boesToValidate == null)
            {
                throw new ArgumentNullException(nameof(boesToValidate));
            }
            if (originalBoes == null)
            {
                throw new ArgumentNullException(nameof(originalBoes));
            }

            if (boesToValidate.Any())
            {
                // Validate individual Boes
                // If valid, change them to draft and upsert
                foreach (FullBoe boeToSave in boesToValidate)
                {
                    // set the WS for the boe, to prevent the loads down the line (in the emailer for example)
                    boeToSave.Workspace = ws;

                    BOEState oldBOEState = originalBoes.First(x => x.Id == boeToSave.Id).State;
                    BOEState newBOEState = BOEState.Draft;

                    // Validate the Awaiting Approval or Approved to Draft state transition
                    string validationMessage = string.Empty;

                    if (!this.BoeStateMachine.PerformStateTransitionValidation(boeToSave, ws, oldBOEState, newBOEState, out validationMessage))
                    {
                        // not valid ... communicate to user
                        throw new GenValidationException("Boe Id: " + boeToSave.Id.ToString() + ":" + validationMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Performs state transition actions (emails and such) for all Boes that were saved w/ a new state
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boesToTransition">Boes to transition</param>
        /// <param name="originalBoes">Original Boes</param>
        public void PerformStateTransitionActionsForBoesEffectedByRecalculation(FullWorkspace ws, HashSet<FullBoe> boesToTransition, HashSet<BoeDTO> originalBoes)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (boesToTransition == null)
            {
                throw new ArgumentNullException(nameof(boesToTransition));
            }
            if (originalBoes == null)
            {
                throw new ArgumentNullException(nameof(originalBoes));
            }

            // Perform transition of state actions (emails, and so on)
            foreach (FullBoe boeToSave in boesToTransition)
            {
                BOEState oldBOEState = originalBoes.First(x => x.Id == boeToSave.Id).State;

                // Perform common state transition actions
                this.BoeStateMachine.PerformStateTransitionAction(boeToSave, ws, oldBOEState, boeToSave.State);
            }
        }

        /// <summary>
        /// Saves the data that was recalculated
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="tasksToSave">Task Elements to Save</param>
        /// <param name="workspaceVariablesToSave">WS Vars to Save</param>
        /// <param name="boesToSave">Boes To Save</param>
        public void SaveDataEffectedByRecalculation(FullWorkspace ws, HashSet<BoeTaskElementDTO> tasksToSave, HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, HashSet<FullBoe> boesToSave)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (tasksToSave == null)
            {
                throw new ArgumentNullException(nameof(tasksToSave));
            }
            if (workspaceVariablesToSave == null)
            {
                throw new ArgumentNullException(nameof(workspaceVariablesToSave));
            }
            if (boesToSave == null)
            {
                throw new ArgumentNullException(nameof(boesToSave));
            }

            // Save WS Variables
            if (workspaceVariablesToSave.Any())
            {
                this.WorkspaceVarLoader.SaveWorkspaceVariables(workspaceVariablesToSave.ToCollection());
            }

            // BULK Save task elements ..
            if (tasksToSave.Any())
            {
                this.BoeTaskElementMediator.MediatedBulkSaveTaskElements(tasksToSave, ws);
            }

            // Adjust each Boe's state to draft and it's Updateable to Upsert
            if (boesToSave.Any())
            {
                BOEState newBOEState = BOEState.Draft;

                foreach (FullBoe boeToSave in boesToSave)
                {
                    boeToSave.Updateable = UpdateType.Upsert;
                    boeToSave.State = newBOEState;
                }

                // "Bulk" Save the BOEs
                this.BoeMediator.MediatedSaveBOEs(ws, boesToSave.ToCollection<BoeDTO>());
            }
        }

        #endregion

        #region Validation of Labor Hours

        /// <summary>
        /// Gets all task elements that contain task elements (FOR THE WORKSPACE) and their delta is not 0; 
        /// This could be caused by precision change, date spread or other spread recalculating functions
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of task elements where delta is not 0</returns>
        public Collection<BoeTaskElementDTO> GetTaskElementsWithNonZeroDeltaLabor(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            Collection<BoeTaskElementDTO> problematicTaskElements = this.GetAllTaskElementsWithNonZeroDeltaLabor(ws.TaskElements, ws);

            return problematicTaskElements;
        }

        /// <summary>
        /// Gets all task elements that contain task elements (FOR THE BOES) and their delta is not 0; 
        /// This could be caused by precision change, date spread or other spread recalculating functions
        /// </summary>
        /// <param name="boes">Boes to check</param>
        /// <param name="ws">Full workspace (used for parsing)</param>
        /// <returns>A collection of task elements where delta is not 0</returns>
        public Collection<BoeTaskElementDTO> GetTaskElementsWithNonZeroDeltaLabor(ICollection<FullBoe> boes, FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            IReadOnlyCollection<BoeTaskElementDTO> taskElements = boes.SelectMany(x => x.TaskElements).ToList().AsReadOnly();
            Collection<BoeTaskElementDTO> problematicTaskElements = this.GetAllTaskElementsWithNonZeroDeltaLabor(taskElements, ws);

            return problematicTaskElements;
        }

        /// <summary>
        /// Checks the Task Elements to find any that have task elements and their delta is not 0
        /// </summary>
        /// <param name="taskElementsToCheck">Task Elements to check</param>
        /// <param name="ws">Workspace used for parsing</param>
        /// <returns>Task Elements that are problematic</returns>
        private Collection<BoeTaskElementDTO> GetAllTaskElementsWithNonZeroDeltaLabor(IReadOnlyCollection<BoeTaskElementDTO> taskElementsToCheck, FullWorkspace ws)
        {
            try
            {
                Collection<BoeTaskElementDTO> result = new Collection<BoeTaskElementDTO>();

                foreach (BoeTaskElementDTO taskElement in taskElementsToCheck)
                {
                    Collection<ResourceTypeDto> taskElementLaborsForTaskElement = taskElement.taskElementLabors.Where(x => x.ValueSpread.HasValue && x.SpreadType == SpreadType.Hours && x.Updateable != UpdateType.Deleted).ToCollection();

                    decimal laborTypeTotal = taskElementLaborsForTaskElement.Sum(x => x.ValueSpread.Value);
                    decimal spreadTotal = taskElement.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours && x.Updateable != UpdateType.Deleted).SelectMany(x => x.LaborSpreads).Sum(x => x.LaborSpreadValue);

                    decimal moqTotal = this.CalculateMoqTotal(ws, taskElement);

                    // Checks MOQ vs total of types vs total of spreads for the whole task
                    if (spreadTotal != moqTotal || moqTotal != laborTypeTotal)
                    {
                        result.Add(taskElement);
                    }
                    else
                    {
                        // checks each task vs its spreads
                        foreach (ResourceTypeDto type in taskElementLaborsForTaskElement)
                        {
                            decimal hoursForType = type.ValueSpread.Value;
                            decimal hourSpreadTotalForType = type.LaborSpreads.Sum(x => x.LaborSpreadValue);

                            if (hoursForType != hourSpreadTotalForType)
                            {
                                result.Add(taskElement);
                                break;
                            }
                        }
                    }
                }

                return result;
            }
            catch
            {
                throw new GenValidationException("<b>Your last action was successful.</b><br />The application encountered an error while attempting to confirm that the recalculation was done correctly. Please check your tasks to verify that the LABOR HOURS data is correct.");
            }
        }

        /// <summary>
        /// Calculates MOQ total for the Task Element
        /// </summary>
        /// <param name="ws">Workspace to which the task belongs</param>
        /// <param name="taskElement">Task element whose Moq Equation we want to calculate</param>
        /// <returns>Decimal value of the equation</returns>
        private decimal CalculateMoqTotal(FullWorkspace ws, BoeTaskElementDTO taskElement)
        {
            decimal moqTotal = 0;

            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(taskElement.OrdinaryVariables, ws.WorkspaceVariables.ToList(), ws);
            string moqTotalString = MOQ.Parser.Calculate(taskElement.MOQHoursEquation, taskElement.OrdinaryVariables, ws.WorkspaceVariables.ToList(), this.VariableSelectBOEtoSumCalculation, data, ws);

            decimal tempMoqResult;
            if (decimal.TryParse(moqTotalString, out tempMoqResult))
            {
                moqTotal = Utilities.AdjustPrecision(tempMoqResult, ws.DecimalPrecision);
            }

            return moqTotal;
        }

        #endregion

        #region Validation of Cost

        /// <summary>
        /// Gets all task elements (for the workspace) where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of task elements where cost is incorrect</returns>
        public Collection<BoeTaskElementDTO> GetTaskElementsWithInconsistentCosts(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            Collection<BoeTaskElementDTO> problematicTaskElements = this.GetAllTaskElementsWithInconsistentCosts(ws.TaskElements);

            return problematicTaskElements;
        }

        /// <summary>
        /// Checks the Task Elements to find any that have task elements and their cost spreads and cost types do not match.
        /// </summary>
        /// <param name="taskElementsToCheck">Task Elements to check.</param>
        /// <returns>A collection of task elements where cost is incorrect.</returns>
        private Collection<BoeTaskElementDTO> GetAllTaskElementsWithInconsistentCosts(IReadOnlyCollection<BoeTaskElementDTO> taskElementsToCheck)
        {
            try
            {
                Collection<BoeTaskElementDTO> result = new Collection<BoeTaskElementDTO>();

                foreach (BoeTaskElementDTO taskElement in taskElementsToCheck)
                {
                    // checks each type vs its spreads
                    foreach (ResourceTypeDto type in taskElement.taskElementLabors.Where(x => x.ValueSpread.HasValue && x.SpreadType == SpreadType.Cost && x.Updateable != UpdateType.Deleted))
                    {
                        decimal costTotal = type.ValueSpread.Value;
                        decimal costSpreadTotal = type.LaborSpreads.Sum(x => x.LaborSpreadValue);

                        if (costTotal != costSpreadTotal)
                        {
                            result.Add(taskElement);
                            break;
                        }
                    }
                }

                return result;
            }
            catch
            {
                throw new GenValidationException("<b>Your last action was successful.</b><br />The application encountered an error while attempting to confirm that the recalculation was done correctly. Please check your tasks to verify that the COST data is correct.");
            }
        }

        #endregion

        #region Validation of ODC

        /// <summary>
        /// Gets all ODC elements where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of ODC elements where data doesn't match</returns>
        public Collection<OtherDirectCostDTO> GetElementsWithInconsistentODCs(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            Collection<OtherDirectCostDTO> problematicODCElements = this.GetAllElementsWithInconsistentODCs(ws.Odcs);

            return problematicODCElements;
        }

        /// <summary>
        /// Gets all ODC elements where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="boes">Boes to check</param>
        /// <returns>A collection of ODC elements where data doesn't match</returns>
        public Collection<OtherDirectCostDTO> GetElementsWithInconsistentODCs(ICollection<FullBoe> boes)
        {
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            IReadOnlyCollection<OtherDirectCostDTO> odcElements = boes.SelectMany(x => x.OtherDirectCosts).ToList().AsReadOnly();

            Collection<OtherDirectCostDTO> problematicODCElements = this.GetAllElementsWithInconsistentODCs(odcElements);

            return problematicODCElements;
        }

        /// <summary>
        /// Gets all ODC elements where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="elementsToCheck">ODC Elements to check</param>
        /// <returns>ODC Elements that are problematic</returns>
        private Collection<OtherDirectCostDTO> GetAllElementsWithInconsistentODCs(IReadOnlyCollection<OtherDirectCostDTO> elementsToCheck)
        {
            try
            {
                Collection<OtherDirectCostDTO> result = new Collection<OtherDirectCostDTO>();

                foreach (OtherDirectCostDTO odcElement in elementsToCheck)
                {
                    // checks each type vs its spreads
                    foreach (OtherDirectCostType type in odcElement.ODCTypes.Where(x => x.Cost.HasValue && x.Updateable != UpdateType.Deleted))
                    {
                        decimal typeTotal = type.Cost.Value;
                        decimal spreadTotal = type.ODCSpreads.Where(x => x.CostSpreadValue.HasValue).Sum(x => x.CostSpreadValue.Value / (decimal)100);

                        if (spreadTotal != typeTotal)
                        {
                            result.Add(odcElement);
                            break;
                        }
                    }
                }

                return result;
            }
            catch
            {
                throw new GenValidationException("<b>Your last action was successful.</b><br />The application encountered an error while attempting to confirm that the recalculation was done correctly. Please check your tasks to verify that the COST data is correct.");
            }
        }

        #endregion

        /// <summary>
        /// Throws an exception if the WS contains any task elements that have non-zero delta
        /// </summary>
        /// <param name="ws">WS to check</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Clin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "nbspnbspnbspnbspnbspTask"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Wbs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Boe")]
        public void ThrowValidationExceptionIfWsContainsTasksWithNonZeroDelta(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            string message = this.GenerateMsgIfWsContainsTaskElementsWithNonZeroDeltaLabor(ws);

            if(message.Length > 0)
            {
                throw new GenValidationException(message);
            }
        }

        /// <summary>
        /// Generates an error message that can then be displayed to the user, if the WS contains any task elements that have non-zero delta
        /// </summary>
        /// <param name="ws">Full WS To Check</param>
        /// <returns>Empty string or an error message</returns>
        public string GenerateMsgIfWsContainsTaskElementsWithNonZeroDeltaLabor(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            HashSet<BoeTaskElementDTO> taskElements = new HashSet<BoeTaskElementDTO>(this.GetTaskElementsWithNonZeroDeltaLabor(ws));

            StringBuilder message = new StringBuilder();

            if (taskElements.Any())
            {
                // preload data
                HashSet<FullBoe> boes = new HashSet<FullBoe>(ws.Boes);
                ws.WbsElements.Any();
                ws.Clins.Any();

                Collection<int> boeIds = taskElements.Select(x => x.BoeID).Distinct().ToCollection();
                string hoursLabel = FullObjectHelper.HoursLabel(ws);
                message.Append("<b>Your last action was successful.</b><br />" +
                    "Resource " + hoursLabel + " have been automatically adjusted. This process caused some tasks to contain a delta between spread values and the MOQ equation.  " +
                    "Tasks containing discrete spreads may have been adjusted and their values rounded.  " +
                    "<b>The following BOEs and their associated Tasks contain delta " + hoursLabel + " and will need to be manually updated.</b><br />");

                object LOCK = new object();

                Parallel.ForEach(boeIds, new ParallelOptions { MaxDegreeOfParallelism = 5 }, boeId =>
                {
                    FullBoe boe = boes.First(x => x.Id == boeId);
                    Collection<BoeTaskElementDTO> affectedTaskElementsForTheBoe = taskElements.Where(x => x.BoeID == boeId).ToCollection();

                    string wbsText = boe.Wbs != null ? boe.Wbs.WbsString : CommonConstants.Unassigned_WBS_Display_Text;
                   

                    string clinText = boe.Clin != null ? boe.Clin.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;

                    // Generate text for this particular Boe
                    StringBuilder tempText = new StringBuilder("<br /><b>BOE: </b>");
                    tempText.Append(boe.Title);
                    tempText.Append("&nbsp;");
                    tempText.Append(wbsText);
                    tempText.Append("&nbsp;");
                    tempText.Append(clinText);
                    tempText.Append("<br />");
                    foreach (BoeTaskElementDTO taskElement in affectedTaskElementsForTheBoe)
                    {
                        tempText.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Task: </b>");
                        tempText.Append(taskElement.BOETaskID);
                        tempText.Append("&nbsp;");
                        tempText.Append(taskElement.TaskTitle);
                        tempText.Append("<br />");
                    }

                    lock (LOCK) // add this BOE's text to the full one
                    {
                        message.Append(tempText);
                    }
                });
            }

            return message.ToString();
        }
    }
}