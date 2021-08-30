// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;

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
    public interface IFullWorkspaceRecalculation
    {
        #region WS Recalculation, boe state validation and transition & saving of all data

        /// <summary>
        /// Will recalculate task element resources in a WS that have a rate type of cost. 
        /// </summary>
        /// <param name="ws">Workspace to recalculate cost resources.</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later.</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        /// <param name="costPrecision">The current cost decimal precision.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        void RecalculateCostInAWorkspaceWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<FullBoe> boesToTransition, int costPrecision);

        /// <summary>
        /// Will recalculate everything in a WS that has to do w/ the labor.. Variables (Workspace and Task/Ordinary), labor... all the way down to spreads
        /// </summary>
        /// <param name="ws">Workspace to recalculate</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later</param>
        /// <param name="workspaceVariablesToSave">WS Variables that will be updated with data to be saved later</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        void RecalculateLaborInAWorkspaceWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition);

        /// <summary>
        /// Will recalculate the list of task elementsin and any other impacted data - task elements as well as variables (Workspace and Task/Ordinary)
        /// </summary>
        /// <param name="ws">Workspace to recalculate</param>
        /// <param name="tasksToSave">Task Elements that will be updated with data to be saved later</param>
        /// <param name="workspaceVariablesToSave">WS Variables that will be updated with data to be saved later</param>
        /// <param name="boesToTransition">Boes that will be updated/transitioned later</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        void RecalculateLaborInTaskElementsWithoutSaving(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition, ICollection<BoeTaskElementDTO> taskElements);

        /// <summary>
        /// Saves the data that was recalculated
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="tasksToSave">Task Elements to Save</param>
        /// <param name="workspaceVariablesToSave">WS Vars to Save</param>
        /// <param name="boesToSave">Boes To Save</param>
        void SaveDataEffectedByRecalculation(FullWorkspace ws, HashSet<BoeTaskElementDTO> tasksToSave, HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, HashSet<FullBoe> boesToSave);

        /// <summary>
        /// Performs state transition actions (emails and such) for all Boes that were saved w/ a new state
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boesToTransition">Boes to transition</param>
        /// <param name="originalBoes">Original Boes</param>
        void PerformStateTransitionActionsForBoesEffectedByRecalculation(FullWorkspace ws, HashSet<FullBoe> boesToTransition, HashSet<BoeDTO> originalBoes);

        /// <summary>
        /// Validate Boe's State Transition for Boes that will be effected by recalculation
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boesToValidate">Boes to validate</param>
        /// <param name="originalBoes">Original Boes</param>
        void ValidateStateTransitionForBoesEffectedByRecalculation(FullWorkspace ws, HashSet<FullBoe> boesToValidate, HashSet<BoeDTO> originalBoes);

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#")]
        void RecalculateBasedOnSumToBOEVariable(FullWorkspace workspace, Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, Collection<BoeTaskElementDTO> inNextSetToCheck,
            ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition,
            HashSet<WorkspaceVariableDTO> wsVarsHash, HashSet<FullBoe> wsBoes,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null);

        #endregion

        #region Validation of Labor

        /// <summary>
        /// Gets all task elements that contain discrete spreads (FOR THE WORKSPACE) and their delta is not 0; 
        /// This could be caused by precision change, date spread or other spread recalculating functions
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of task elements where delta is not 0</returns>
        Collection<BoeTaskElementDTO> GetTaskElementsWithNonZeroDeltaLabor(FullWorkspace ws);
    
        /// <summary>
        /// Gets all task elements that contain discrete spreads (FOR THE BOES) and their delta is not 0; 
        /// This could be caused by precision change, date spread or other spread recalculating functions
        /// </summary>
        /// <param name="boes">Boes to check</param>
        /// <param name="ws">Full workspace (used for parsing)</param>
        /// <returns>A collection of task elements where delta is not 0</returns>
        Collection<BoeTaskElementDTO> GetTaskElementsWithNonZeroDeltaLabor(ICollection<FullBoe> boes, FullWorkspace ws);

        #endregion

        #region Validation of Cost

        /// <summary>
        /// Gets all task elements (for the WS) where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of task elements where cost is incorrect</returns>
        Collection<BoeTaskElementDTO> GetTaskElementsWithInconsistentCosts(FullWorkspace ws);
        #endregion

        #region Validation of ODC

        /// <summary>
        /// Gets all ODC elements where their costs in spreads do not equal the rolled up types
        /// </summary>
        /// <param name="ws">Workspace to check</param>
        /// <returns>A collection of ODC elements where data doesn't match</returns>
        Collection<OtherDirectCostDTO> GetElementsWithInconsistentODCs(FullWorkspace ws);

        /// <summary>
        /// Gets all ODC elements where their costs in spreads do not equal the rolled up types        /// </summary>
        /// <param name="boes">Boes to check</param>
        /// <returns>A collection of ODC elements where data doesn't match</returns>
        Collection<OtherDirectCostDTO> GetElementsWithInconsistentODCs(ICollection<FullBoe> boes);

        #endregion

        /// <summary>
        /// Throws an exception if the WS contains any task elements that contain discrete spreads and delta is not 0
        /// </summary>
        /// <param name="ws">WS to check</param>
        void ThrowValidationExceptionIfWsContainsTasksWithNonZeroDelta(FullWorkspace ws);

        /// <summary>
        /// Generates an error message that can then be displayed to the user, if the WS contains any task elements that contain discrete spreads and delta is not 0
        /// </summary>
        /// <param name="ws">Full WS To Check</param>
        /// <returns>Empty string or an error message</returns>
        string GenerateMsgIfWsContainsTaskElementsWithNonZeroDeltaLabor(FullWorkspace ws);
    }
}
