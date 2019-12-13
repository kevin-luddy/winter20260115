// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System.Collections.Generic;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public interface IVariableCircularReferenceChecker
    {
        /// <summary>
        /// Takes a source BOEID and a BOEID of a BOE that will be connected by a Summed Variable reference
        /// and recursively determines whether or not adding a connection to that BOE will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingBOE">The BOEID to check connection to</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        bool BOECreatesCircularReference(VariableCircularReferenceCheckerCache cache, int originalBOEID, FullBoe checkingBOE, FullWorkspace workspace);

        /// <summary>
        /// Takes a source BOEID and a collection of Ordinary Variables that could be added to that source BOE
        /// and determines whether or not adding a connection to those variables will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingOrdinaryVariables">The ordinary variables to check</param>
        /// <returns>A collection of all of the specified ordinary variable that cause circular references</returns>
        ICollection<OrdinaryVariableDto> OrdinaryVariablesCreateCircularReference(VariableCircularReferenceCheckerCache cache, int originalBOEID, ICollection<OrdinaryVariableDto> checkingOrdinaryVariables, FullWorkspace workspace);

        /// <summary>
        /// Takes a source BOEID and a Workspace Variable DTO that could be added to that source BOE
        /// and determines whether or not adding a connection to that variable will cause a circular reference.
        /// </summary>
        /// <param name="cache">Explicit cache object to use</param>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingWorkspaceVariable">The workspace variable to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        bool WorkspaceVariableCreatesCircularReference(VariableCircularReferenceCheckerCache cache, int originalBOEID, WorkspaceVariableDTO checkingWorkspaceVariable, FullWorkspace workspace);

        /// <summary>
        /// Takes a source BOEID and a collection of of Worksapce Variables that could be added to that source BOE
        /// and determines whether or not adding a connection to those variables will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingWorkspaceVariables">The Workspace Variables to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>A collection of all of the specified workspace variables that cause circular references</returns>
        ICollection<WorkspaceVariableDTO> WorkspaceVariablesCreateCircularReference(VariableCircularReferenceCheckerCache cache, int originalBOEID, ICollection<WorkspaceVariableDTO> checkingWorkspaceVariables, FullWorkspace workspace);

        /// <summary>
        /// Checks to see whether renumbering a WBS will cause a circular reference.
        /// </summary>
        /// <param name="cache">The explicit cache object to use</param>
        /// <param name="wbsID">The WBS that will be renumbered</param>
        /// <param name="newWBSNumber">The new number for the WBS</param>
        /// <returns>True if the renumber will create a circular reference, false if not</returns>
        bool WBSRenumberCreatesCircularReference(VariableCircularReferenceCheckerCache cache, FullWbs wbs, string newWBSNumber, ICollection<WbsDTO> inMemoryWBS, FullWorkspace ws);

        /// <summary>
        /// Gets a workspace variable and returns a list of BOEs in the same workspace that will be valid
        /// selections to sum under that variable.
        /// </summary>
        /// <param name="checkingWorkspaceVariable">The workspace variable to check</param>
        /// <returns>Collection of valid BOE IDs for the variable</returns>
        ICollection<int> FindValidBOEsForWorkspaceVariable(VariableCircularReferenceCheckerCache cache, WorkspaceVariableDTO checkingWorkspaceVariable, FullWorkspace workspace);

        /// <summary>
        /// For a not in use workspace variable or a new workspace variables, returns a list of BOEs in the same workspace that will be valid selection
        /// to sum under that variable
        /// </summary>
        /// <param name="inWorkspaceID">workspace ID</param>
        /// <returns>valid BOE IDS</returns>
        ICollection<int> FindValidBOEsForNotInUseWorkspaceVariable(FullWorkspace ws);

        ICollection<WorkspaceVariableDTO> ValidateWorkspaceVariableSave(VariableCircularReferenceCheckerCache cache, ICollection<WorkspaceVariableDTO> checkingWorkspaceVariables, FullWorkspace workspace);

        /// <summary>
        /// Gets all valid WBS selections for the given BOE, removing those that would cause a circular reference.
        /// </summary>
        /// <param name="cache">Cache object</param>
        /// <param name="boe">The BOE to check</param>
        /// <param name="wbs">WBS</param>
        /// <param name="inMemoryBOE">boe to check against</param>
        /// <returns>All valid WBS IDs</returns>
        bool BOEWBSMoveCreatesCircularReference(VariableCircularReferenceCheckerCache cache, FullBoe boe, FullWbs wbs, ICollection<BoeDTO> inMemoryBOE, FullWorkspace ws);

        /// <summary>
        /// Gets all valid CLIN selections for the given BOE, removing those that would cause a circular reference.
        /// </summary>
        /// <param name="cache">Cache object</param>
        /// <param name="inBOEID">The BOE to check</param>
        /// <returns>All valid CLIN IDs</returns>
        bool BOECLINMoveCreatesCircularReference(FullWorkspace workspace, VariableCircularReferenceCheckerCache cache, FullBoe Boe, ClinDTO Clin, ICollection<BoeDTO> inMemoryBOE);

        /// <summary>
        /// Take a BOE ID and return all other BOEs that are referenced by Task Elements under the BOE. Drills
        /// down from BOE into task elements, into summed variables, into explicitly referenced BOE IDs and
        /// implicitly referenced BOEs through WBS/CLIN IDs.
        /// </summary>
        /// <param name="boeID">The BOE to search for referenced BOEs</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>A distinct set of BOE IDs that are referenced by this BOE through summed variables</returns>
        ICollection<int> GetBOEIDsReferencedByBOEID(FullBoe boe, ICollection<WorkspaceVariableDTO> inMemoryWorkspaceVariables, ICollection<WbsDTO> inMemoryWBS, ICollection<BoeDTO> inMemoryBOE, FullWorkspace workspace);
    }
}
