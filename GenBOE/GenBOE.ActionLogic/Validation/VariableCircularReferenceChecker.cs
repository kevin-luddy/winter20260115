// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Checks for circular references using different types of input parameters
    /// </summary>
    public class VariableCircularReferenceChecker : IVariableCircularReferenceChecker
    {
        private IWorkspaceVariableDTODataLoader _WorkspaceVariableDTODataLoader;

        public VariableCircularReferenceChecker(
            IWorkspaceVariableDTODataLoader inWorkspaceVariableDTODataLoader)
        {
            this._WorkspaceVariableDTODataLoader = inWorkspaceVariableDTODataLoader;
        }
        
        /// <summary>
        /// Takes a source BOEID and a BOEID of a BOE that will be connected by a Summed Variable reference
        /// and recursively determines whether or not adding a connection to that BOE will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingBOE">The BOEID to check connection to</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        public bool BOECreatesCircularReference(
            VariableCircularReferenceCheckerCache cache,
            int originalBOEID,
            FullBoe checkingBOE,
            FullWorkspace workspace)
        {
            return this.BOECreatesCircularReferenceRecursive(workspace, cache, originalBOEID, checkingBOE, null, null, null);
        }

        /// <summary>
        /// Takes a source BOEID and a BOEID of a BOE that will be connected by a Summed Variable reference
        /// and recursively determines whether or not adding a connection to that BOE will cause a circular reference.
        /// </summary>
        /// <param name="workspace">The full workspace.</param>
        /// <param name="cache">cache</param>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingBOE">The BOEID to check connection to</param>
        /// <param name="inMemoryWorkspaceVariables">workspace variables</param>
        /// <param name="inMemoryWBS">wbs</param>
        /// <param name="inMemoryBOE">boe</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        private bool BOECreatesCircularReferenceRecursive(
            FullWorkspace workspace,
            VariableCircularReferenceCheckerCache cache,
            int originalBOEID,
            FullBoe checkingBOE,
            ICollection<WorkspaceVariableDTO> inMemoryWorkspaceVariables,
            ICollection<WbsDTO> inMemoryWBS,
            ICollection<BoeDTO> inMemoryBOE)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // If the cache is null, initialize it
            if (cache == null)
            {
                cache = new VariableCircularReferenceCheckerCache();
            }

            // Check to see if the current check was already determined. If so, we'll return it from the cache.
            if (cache.BOEConnectionCached(originalBOEID, checkingBOE.Id))
            {
                return cache.BOECreatesCircularReference_Cached(originalBOEID, checkingBOE.Id);
            }
            //  If the current check wasn't in the cache but the current BOE being checked has the same ID as
            // the original BOE, then we've found a circular reference and we can pop back out of the
            // recursion with a true result
            else if (originalBOEID == checkingBOE.Id)
            {
                // Cache the result of the check
                cache.CacheBOEConnection(originalBOEID, checkingBOE.Id, true);

                return true;
            }

            // Get all BOEs that are referenced by variables within task elements under
            // the current BOE
            var referencedBOEIDs = this.GetBOEIDsReferencedByBOEID(checkingBOE, inMemoryWorkspaceVariables, inMemoryWBS, inMemoryBOE, workspace);

            // Recurse back into this function for each BOE referenced by the current BOE
            foreach (var referencedBOEID in referencedBOEIDs)
            {
                FullBoe refBoe = workspace.Boes.First(i => i.Id == referencedBOEID); 
                // Recurse
                if (this.BOECreatesCircularReferenceRecursive(workspace, cache, originalBOEID, refBoe, inMemoryWorkspaceVariables, inMemoryWBS, inMemoryBOE))
                {
                    // Cache the result of the check
                    cache.CacheBOEConnection(originalBOEID, checkingBOE.Id, true);

                    // If the connection was invalid we can stop the recursion and return a true result
                    return true;
                }
            }

            // Cache the result of the check
            cache.CacheBOEConnection(originalBOEID, checkingBOE.Id, false);

            // If no circular references were found, or the current BOE references no other BOEs,
            // we can pop out of this leg of the recursion with a false result.
            return false;
        }

        /// <summary>
        /// Checks for circular references when adding a new BOE reference to an in-use workspace variable.
        /// The algorithm gets each BOE that is currently using the workspace variable and determines whether
        /// adding a connection to the specified BOE ID will cause a circular reference.
        /// </summary>
        /// <param name="cache">Explicit cache collection to cache to</param>
        /// <param name="originalWorkspaceVariableID">The workspace variable to check</param>
        /// <param name="checkingBOE">The new BOE to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if the BOE can be added to the workspace variable without introducing a circular reference, false otherwise.</returns>
        private bool BOECreatesCircularReferenceForAny(
            VariableCircularReferenceCheckerCache cache,
            int originalWorkspaceVariableID,
            FullBoe checkingBOE,
            ICollection<WorkspaceVariableDTO> inMemoryWorkspaceVariables,
            FullWorkspace workspace)
        {
            // Get all BOEs that reference the workspace variable
            var originalBOEIDs = this._WorkspaceVariableDTODataLoader.GetBOEIDsUsingWorkspaceVarID(originalWorkspaceVariableID);

            // Check each BOE recursively for circular references when adding a connection TO checkingBOEID
            foreach (var originalBOEID in originalBOEIDs)
            {
                if (this.BOECreatesCircularReferenceRecursive(workspace, cache, originalBOEID, checkingBOE, inMemoryWorkspaceVariables, null, null))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Takes a source BOEID and an Ordinary Variable that could be added to that source BOE
        /// and determines whether or not adding a connection to that variable will cause a circular reference.
        /// </summary>
        /// <param name="cache">Explicit cache object to use</param>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingOrdinaryVariable">The ordinary variable to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        private bool OrdinaryVariableCreatesCircularReference(
            VariableCircularReferenceCheckerCache cache,
            int originalBOEID,
            OrdinaryVariableDto checkingOrdinaryVariable,
            FullWorkspace workspace)
        {
            // Get all BOEs that the ordinary variable references
            var referencedBOEIDs = this.GetBOEIDsReferencedByOrdinaryVariable(checkingOrdinaryVariable, workspace);

            // Check each BOE recursively for circular references
            foreach (var referencedBOEID in referencedBOEIDs)
            {
                FullBoe refBoe = workspace.Boes.FirstOrDefault(i => i.Id == referencedBOEID);
                if (refBoe != null)
                {
                    if (this.BOECreatesCircularReferenceRecursive(workspace, cache, originalBOEID, refBoe, null, null, null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Takes a source BOEID and a collection of Ordinary Variables that could be added to that source BOE
        /// and determines whether or not adding a connection to those variables will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingOrdinaryVariables">The ordinary variables to check</param>
        /// <returns>A collection of all of the specified ordinary variable that cause circular references</returns>
        public ICollection<OrdinaryVariableDto> OrdinaryVariablesCreateCircularReference(
            VariableCircularReferenceCheckerCache cache,
            int originalBOEID,
            ICollection<OrdinaryVariableDto> checkingOrdinaryVariables,
            FullWorkspace workspace)
        {
            if (checkingOrdinaryVariables == null)
            {
                throw new ArgumentNullException(nameof(checkingOrdinaryVariables));
            }

            Collection<OrdinaryVariableDto> toReturn = new Collection<OrdinaryVariableDto>();

            foreach (OrdinaryVariableDto checkingOrdinaryVariable in checkingOrdinaryVariables)
            {
                if (this.OrdinaryVariableCreatesCircularReference(cache, originalBOEID, checkingOrdinaryVariable, workspace))
                {
                    toReturn.Add(checkingOrdinaryVariable);
                }
            }
 
           return toReturn;
        }
              
        /// <summary>
        /// Takes a source BOEID and a Workspace Variable DTO that could be added to that source BOE
        /// and determines whether or not adding a connection to that variable will cause a circular reference.
        /// </summary>
        /// <param name="cache">Explicit cache object to use</param>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingWorkspaceVariable">The workspace variable to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>True if this creates a circular reference, false otherwise</returns>
        public bool WorkspaceVariableCreatesCircularReference(
            VariableCircularReferenceCheckerCache cache,
            int originalBOEID,
            WorkspaceVariableDTO checkingWorkspaceVariable,
            FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            // Get all BOEs that the workspace variable references
            var referencedBOEIDs = this.GetBOEIDsReferencedByWorkspaceVariable(checkingWorkspaceVariable, workspace);

            // Check each BOE recursively for circular references
            foreach (var referencedBOEID in referencedBOEIDs)
            {
                FullBoe refBoe = workspace.Boes.First(i => i.Id == referencedBOEID); 
                if (this.BOECreatesCircularReferenceRecursive(workspace, cache, originalBOEID, refBoe, null, null, null))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Takes a source BOEID and a collection of of Worksapce Variables that could be added to that source BOE
        /// and determines whether or not adding a connection to those variables will cause a circular reference.
        /// </summary>
        /// <param name="originalBOEID">The source BOEID that will contain a new Summed Variable</param>
        /// <param name="checkingWorkspaceVariables">The Workspace Variables to check</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>A collection of all of the specified workspace variables that cause circular references</returns>
        public ICollection<WorkspaceVariableDTO> WorkspaceVariablesCreateCircularReference(
            VariableCircularReferenceCheckerCache cache, 
            int originalBOEID,
            ICollection<WorkspaceVariableDTO> checkingWorkspaceVariables,
            FullWorkspace workspace)
        {
            if (checkingWorkspaceVariables == null)
            {
                throw new ArgumentNullException(nameof(checkingWorkspaceVariables));
            }

            Collection<WorkspaceVariableDTO> toReturn = new Collection<WorkspaceVariableDTO>();

            foreach (WorkspaceVariableDTO checkingWorkspaceVariable in checkingWorkspaceVariables)
            {
                if (this.WorkspaceVariableCreatesCircularReference(cache, originalBOEID, checkingWorkspaceVariable, workspace))
                {
                    toReturn.Add(checkingWorkspaceVariable);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Checks to see whether renumbering a WBS will cause a circular reference.
        /// </summary>
        /// <param name="cache">The explicit cache object to use</param>
        /// <param name="wbsID">The WBS that will be renumbered</param>
        /// <param name="newWBSNumber">The new number for the WBS</param>
        /// <returns>True if the renumber will create a circular reference, false if not</returns>
        public virtual bool WBSRenumberCreatesCircularReference(VariableCircularReferenceCheckerCache cache, FullWbs wbs, string newWBSNumber, ICollection<WbsDTO> inMemoryWBS, FullWorkspace ws)
        {
            if (newWBSNumber == null)
            {
                throw new ArgumentNullException(nameof(newWBSNumber));
            }

            // Only check if the saved WBS's Number is different from the one passed in
            if (wbs != null && !wbs.WbsNumber.Equals(newWBSNumber.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                // Get all BOEs directly in the WBS being renamed
                var boesInWBS = ws.Boes.Where(x => x.WBSID == wbs.Id);

                // Get the new parent WBSs, after the rename
                var newParentWBS = wbs.AllParentWbs;

                // Get all BOEs that are referencing any of the variables in the two above collections
                var originBOEIDsToCheck = (from w in newParentWBS
                                           from b in this.GetAllBOEIDsReferencingWBS(w, ws)
                                           select b).Distinct();

                // Check connections from each of the BOEs in the collection above, to each of the BOEs that are inside of the WBS being moved
                foreach (var originBOEID in originBOEIDsToCheck)
                {
                    foreach (var destBoe in boesInWBS)
                    {
                        // If any circular reference is found, return true
                        if (this.BOECreatesCircularReferenceRecursive(ws, cache, originBOEID, destBoe, null, inMemoryWBS, null))
                        {
                            return true;
                        }
                    }
                }
            }

            // Return false if no circular references were found
            return false;
        }

        /// <summary>
        /// Gets a workspace variable and returns a list of BOEs in the same workspace that will be valid
        /// selections to sum under that variable.
        /// </summary>
        /// <param name="checkingWorkspaceVariable">The workspace variable to check</param>
        /// <returns>Collection of valid BOE IDs for the variable</returns>
        public ICollection<int> FindValidBOEsForWorkspaceVariable(
            VariableCircularReferenceCheckerCache cache,
            WorkspaceVariableDTO checkingWorkspaceVariable,
            FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            var toReturn = new List<int>();
            
            if (checkingWorkspaceVariable != null)
            {
                // Get all BOEs in the workspace
                var checkingBOEs = workspace.Boes.Where(b=>!b.IsMultiClinWbs);

                // If the variable is in use, we need to check the BOEs that are referencing it against each BOE
                // in the workspace to find possible circular references.
                if (checkingWorkspaceVariable.InUse)
                {
                    // For each BOE in the workspace, check all of the BOEs that reference this variable against
                    // the BOE for circular references.
                    foreach (var checkingBOE in checkingBOEs)
                    {
                        // If there are no circular references possible between the BOEs referencing this variable
                        // and the current BOE being checked, add that BOE to the return collection as valid.
                        if (!this.BOECreatesCircularReferenceForAny(cache, checkingWorkspaceVariable.Id, checkingBOE, null, workspace))
                        {
                            toReturn.Add(checkingBOE.Id);
                        }
                    }
                }
                else
                {
                    // If the variable is not in use, all BOEs in the Workspace are valid selections
                    toReturn.AddRange(checkingBOEs.Select(x => x.Id));
                }
            
            }

            return toReturn;
        }

        /// <summary>
        /// For a not in use workspace variable or a new workspace variables, returns a list of BOEs in the same workspace that will be valid selection
        /// to sum under that variable
        /// </summary>
        /// <param name="inWorkspaceID">workspace ID</param>
        /// <returns>valid BOE IDS</returns>
        public ICollection<int> FindValidBOEsForNotInUseWorkspaceVariable(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            var toReturn = new List<int>();

            // Get all BOEs in the workspace
            var checkingBOEs = ws.Boes;

            // If the variable is not in use, all BOEs in the Workspace are valid selections 
            foreach (var boe in checkingBOEs)
            {
                toReturn.Add(boe.Id);
            }
            
            return toReturn;
        }


        public ICollection<WorkspaceVariableDTO> ValidateWorkspaceVariableSave(
            VariableCircularReferenceCheckerCache cache,
            ICollection<WorkspaceVariableDTO> checkingWorkspaceVariables,
            FullWorkspace workspace)
        {
            if (checkingWorkspaceVariables == null)
            {
                throw new ArgumentNullException(nameof(checkingWorkspaceVariables));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            List<WorkspaceVariableDTO> workspaceVariablesThatCreateCRs = new List<WorkspaceVariableDTO>();
            if (checkingWorkspaceVariables.Any())
            {
                // Iterate over each workspace variable in the workspace
                foreach (var checkingWorkspaceVariable in checkingWorkspaceVariables)
                {
                    // If the variable is in use, we need to check the BOEs that are referencing it against each BOE
                    // that it has selected to Sum.
                    if (checkingWorkspaceVariable.InUse && checkingWorkspaceVariable.Id > 0)
                    {
                        // Get all BOEs referenced by this variable in its SelectedBOEsToSum collection
                        var referencedBOEIDs = this.GetBOEIDsReferencedByWorkspaceVariable(checkingWorkspaceVariable, workspace);

                        // For each referenced BOE
                        foreach (var referencedBOEID in referencedBOEIDs)
                        {
                            FullBoe refBoe = FullWorkspaceHelper.GetBoeById(workspace, referencedBOEID); 

                            // If connecting any of the BOEs referencing this variable with any of the BOEs selected to SUM causes a
                            // circular reference, return true. Otherwise keep checking the other referenced BOEs.
                            if (this.BOECreatesCircularReferenceForAny(cache, checkingWorkspaceVariable.Id, refBoe, checkingWorkspaceVariables, workspace))
                            {
                                // don't add if this is already returned
                                if (!workspaceVariablesThatCreateCRs.Contains(checkingWorkspaceVariable))
                                {
                                    workspaceVariablesThatCreateCRs.Add(checkingWorkspaceVariable);
                                }
                            }
                           
                        }
                    }
                }
            }

            return workspaceVariablesThatCreateCRs;

        }

        /// <summary>
        /// Gets all valid WBS selections for the given BOE, removing those that would cause a circular reference.
        /// </summary>
        /// <param name="cache">Cache object</param>
        /// <param name="boe">The BOE to check</param>
        /// <param name="wbs">WBS</param>
        /// <param name="inMemoryBOE">boe to check against</param>
        /// <returns>All valid WBS IDs</returns>
        public virtual bool BOEWBSMoveCreatesCircularReference(VariableCircularReferenceCheckerCache cache, FullBoe boe, FullWbs wbs, ICollection<BoeDTO> inMemoryBOE, FullWorkspace ws)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }
            if(wbs == null)
            {
                throw new ArgumentNullException(nameof(wbs));
            }
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            // Only check if the BOE's WBSID is different from the one passed in
            if (boe.WBSID.HasValue && boe.WBSID.Value != wbs.Id)
            {
                // Get all parents of the WBS
                var parentWBSs = wbs.AllParentWbs;

                // Get all BOEs that are referencing any of this WBS's parent WBSs
                var originBOEIDsToCheck = (from w in parentWBSs
                                           from b in this.GetAllBOEIDsReferencingWBS(w, ws)
                                           select b).Distinct();

                // Get all BOEs that are referencing THIS WBS and combine with the parent WBSs BOEs
                originBOEIDsToCheck = originBOEIDsToCheck.Union(this.GetAllBOEIDsReferencingWBS(wbs, ws));

                // Check all BOEs referencing the WBS against the given BOE for validity
                foreach (var checkingBOE in originBOEIDsToCheck)
                {
                    // If any BOE creates a circular reference then this WBS is not valid for a move
                    if (this.BOECreatesCircularReferenceRecursive(boe.Workspace, cache, checkingBOE, boe, null, null, inMemoryBOE))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

  

        /// <summary>
        /// Gets all valid CLIN selections for the given BOE, removing those that would cause a circular reference.
        /// </summary>
        /// <param name="cache">Cache object</param>
        /// <param name="inBOEID">The BOE to check</param>
        /// <returns>All valid CLIN IDs</returns>
        public virtual bool BOECLINMoveCreatesCircularReference(
            FullWorkspace workspace,
            VariableCircularReferenceCheckerCache cache,
            FullBoe Boe,
            ClinDTO Clin,
            ICollection<BoeDTO> inMemoryBOE)
        {
            if (Boe == null)
            {
                throw new ArgumentNullException(nameof(Boe));
            }
            if (Clin == null)
            {
                throw new ArgumentNullException(nameof(Clin));
            }

            // Only check if the BOE's CLINID is different from the one passed in, or if it's getting assigned for the first time
            if (Boe.CLINID != Clin.Id)
            {
                // Get the CLIN

                // Get all BOEs that are referencing the current CLIN
                var allBOEsReferencingCLIN = FullWorkspaceHelper.GetAllBOEIDsReferencingCLIN(workspace, Clin);

                // Check all BOEs referencing the CLIN against the given BOE for validity
                foreach (var checkingBOE in allBOEsReferencingCLIN)
                {
                    // If any BOE creates a circular reference then this CLIN is not valid for a move
                    if (this.BOECreatesCircularReferenceRecursive(workspace, cache, checkingBOE, Boe, null, null, inMemoryBOE))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Take a BOE ID and return all other BOEs that are referenced by Task Elements under the BOE. Drills
        /// down from BOE into task elements, into summed variables, into explicitly referenced BOE IDs and
        /// implicitly referenced BOEs through WBS/CLIN IDs.
        /// </summary>
        /// <param name="boeID">The BOE to search for referenced BOEs</param>
        /// <param name="workspace">The full workspace.</param>
        /// <returns>A distinct set of BOE IDs that are referenced by this BOE through summed variables</returns>
        public ICollection<int> GetBOEIDsReferencedByBOEID(
            FullBoe boe,
            ICollection<WorkspaceVariableDTO> inMemoryWorkspaceVariables,
            ICollection<WbsDTO> inMemoryWBS,
            ICollection<BoeDTO> inMemoryBOE,
            FullWorkspace workspace)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }
            if (inMemoryWorkspaceVariables == null)
            {
                inMemoryWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }

            if (inMemoryWBS == null)
            {
                inMemoryWBS = new Collection<WbsDTO>();
            }

            if (inMemoryBOE == null)
            {
                inMemoryBOE = new Collection<BoeDTO>();
            }

            // Get the BOE
            ICollection<BoeTaskElementDTO> boeTaskElements = workspace.TaskElements.Where(i => i.BoeID == boe.Id).ToCollection<BoeTaskElementDTO>();

            // Get all Summed Task Ordinary Variables within the BOE
            var ordinaryVariableBOEsToSum = (from t in boeTaskElements
                                            from v in t.OrdinaryVariables
                                            from s in v.SelectedBOEsToSum
                                            where v.ValueType == VarValueType.SumOfBOEs
                                            select s).ToList();

            // Get all Summed Workspace Variables referenced by tasks within the BOE
            var inMemoryWorkspaceVariableIDs = inMemoryWorkspaceVariables.Select(w => w.Id).Distinct();

            var dbWorkspaceVariableBOEsToSum = from t in boeTaskElements
                                             from w in t.WorkspaceVariableIDs
                                             where !inMemoryWorkspaceVariableIDs.Contains(w)
                                             from v in workspace.WorkspaceVariables.Where(i => i.Id == w) 
                                             where v.ValueType == VarValueType.SumOfBOEs
                                             from s in v.SelectedBOEsToSum
                                             select s;

            var inMemoryWorkspaceVariableBOEsToSum = from t in boeTaskElements
                                                     from w in t.WorkspaceVariableIDs
                                                     where inMemoryWorkspaceVariableIDs.Contains(w)
                                                     from v in inMemoryWorkspaceVariables.Where(wv => wv.Id == w)
                                                     where v.ValueType == VarValueType.SumOfBOEs
                                                     from s in v.SelectedBOEsToSum
                                                     select s;

            var workspaceVariableBOEsToSum = dbWorkspaceVariableBOEsToSum.Union(inMemoryWorkspaceVariableBOEsToSum).ToList();
            
            // Get distinct BOE IDs referenced by ordinary variables
            var boesReferencedByOrinaryVariables = GetBOEIDsReferencedBySelectBOEsToSum(workspace, ordinaryVariableBOEsToSum, inMemoryWBS, inMemoryBOE);
            
            // Get distinct BOE IDs referenced by workspace variables
            var boesReferencedByWorkspaceVariables = GetBOEIDsReferencedBySelectBOEsToSum(workspace, workspaceVariableBOEsToSum, inMemoryWBS, inMemoryBOE);
            
            // Combine the collections of BOE IDs and return distinct set of BOEs that are referenced by any
            // summed variable within the specified BOE.
            return boesReferencedByOrinaryVariables.Union(boesReferencedByWorkspaceVariables).ToList();
        }

        /// <summary>
        /// Takes a WBS and returns all BOEs that are referencing that WBS through a workspace or ordinary variable
        /// </summary>
        /// <param name="inWBS">The WBS to check</param>
        /// <returns>The IDs of all BOEs that are referencing this WBS through a summed variable</returns>
        private ICollection<int> GetAllBOEIDsReferencingWBS(FullWbs inWBS, FullWorkspace workspace)
        {
            // Get all workspace variables that are currently summing the WBS
            var workspaceVariablesReferencingWBS = (from w in workspace.WorkspaceVariables
                                                    where w.ValueType == VarValueType.SumOfBOEs
                                                    from s in w.SelectedBOEsToSum
                                                    where s.WBSID.HasValue && s.WBSID.Value == inWBS.Id
                                                    select w).Distinct();

            // Get all ordinary variables that are currently summing the WBS
            var ordinaryVariablesReferencingWBS = (from b in workspace.Boes
                                                   from t in workspace.TaskElements.Where(x => x.BoeID == b.Id)
                                                   from o in t.OrdinaryVariables
                                                   where o.ValueType == VarValueType.SumOfBOEs
                                                   from s in o.SelectedBOEsToSum
                                                   where s.WBSID.HasValue && s.WBSID.Value == inWBS.Id
                                                   select o).Distinct();

            // Get all BOEs that are referencing any of the variables in the two above collections
            var originBOEsToCheck = (from w in workspaceVariablesReferencingWBS
                                     from b in this._WorkspaceVariableDTODataLoader.GetBOEIDsUsingWorkspaceVarID(w.Id)
                                     select b).Union((from o in ordinaryVariablesReferencingWBS
                                                      select o.BoeID));

            return originBOEsToCheck.ToList();
        }

        /// <summary>
        /// Takes an Ordinary Variable and returns all BOEs that the variable references
        /// </summary>
        /// <param name="ordinaryVariable">The ordinary variable to search</param>
        /// <returns>A list of distinct BOE IDs that the ordinary variable references</returns>
        private ICollection<int> GetBOEIDsReferencedByOrdinaryVariable(
            OrdinaryVariableDto ordinaryVariable,
            FullWorkspace workspace)
        {
            // If it is a summed variable, return the BOEs that are referenced by it
            if (ordinaryVariable.ValueType == VarValueType.SumOfBOEs)
            {
                return GetBOEIDsReferencedBySelectBOEsToSum(workspace, ordinaryVariable.SelectedBOEsToSum, null, null);
            }
            // If it isn't summed, return an empty collection
            else
            {
                return new Collection<int>();
            }
        }

        /// <summary>
        /// Takes a Workspace Variable and returns all BOEs that the variable references
        /// </summary>
        /// <param name="workspaceVariable">The workspace variable to search</param>
        /// <returns>A list of distinct BOE IDs that the workspace variable references</returns>
        private ICollection<int> GetBOEIDsReferencedByWorkspaceVariable(
            WorkspaceVariableDTO workspaceVariable,
            FullWorkspace workspace)
        {
            // If it is a summed variable, return the BOEs that are referenced by it
            if (workspaceVariable.ValueType == VarValueType.SumOfBOEs)
            {
                return GetBOEIDsReferencedBySelectBOEsToSum(workspace, workspaceVariable.SelectedBOEsToSum, null, null);
            }
            // If it isn't summed, return an empty collection
            else
            {
                return new Collection<int>();
            }
        }

        /// <summary>
        /// Takes a collection of SelectBOEsToSum objects and returns the distinct BOE IDs that are either
        /// explicitly referenced, or implicitly referenced through a summary WBS or CLIN.
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <param name="selectBOEsToSum">The collection to find BOEs within</param>
        /// <param name="wbsesToCheck">wbs</param>
        /// <param name="boesToCheck">boe</param>
        /// <returns>A collection of distinct referenced BOE IDs</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public static ICollection<int> GetBOEIDsReferencedBySelectBOEsToSum(
            FullWorkspace workspace,
            ICollection<SelectBOEsToSum> selectBOEsToSum,
            ICollection<WbsDTO> wbsesToCheck,
            ICollection<BoeDTO> boesToCheck)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (wbsesToCheck == null)
            {
                wbsesToCheck = new Collection<WbsDTO>();
            }

            if (boesToCheck == null)
            {
                boesToCheck = new Collection<BoeDTO>();
            }

            if (boesToCheck.Any() &&
                wbsesToCheck.Any())
            {
                throw new ArgumentException("Only one of the two in-memory collections (\"inMemoryWBS\" and \"inMemoryBOE\") should be defined");
            }
            
            // Get all explicitly summed WBS IDs on task or workspace variables
            var distinctSummedWBS = selectBOEsToSum.Where(o => o.WBSID.HasValue);

            // Get all explicitly summed CLIN IDs on task or workspace variables
            var distinctSummedCLIN = selectBOEsToSum.Where(o => o.CLINID.HasValue);

            // Get all explicitly summed BOE IDs on task or workspace variables
            var boesExplicitlyReferenced = selectBOEsToSum.Where(o => o.BoeID.HasValue).Select(v => v.BoeID.Value);

            // Get BOEs referenced implicitly through explicit CLIN references on task or workspace variables
            List<ClinDTO> clinObjects = new List<ClinDTO>();
            HashSet<int> clinIds = new HashSet<int>();
            foreach (var summedClin in distinctSummedCLIN)
            {
                ClinDTO clinToAdd = workspace.Clins.FirstOrDefault(c => c.Id == summedClin.CLINID.Value);

                if (clinToAdd != null)
                {
                    clinObjects.Add(clinToAdd);
                    clinIds.Add(clinToAdd.Id);
                }
            }

            var idsOfBoesImplicitlyReferencedByCLIN = (from b in workspace.Boes
                                                       where b.CLINID.HasValue && clinIds.Contains<int>(b.CLINID.Value) && !boesToCheck.Select(x => x.Id == b.Id).Any()
                                                       select b.Id).Distinct();
            
            // Get BOEs under the CLIN that were modified in-memory
            var inMemoryBOEsImplicitlyReferencedByCLIN = from c in distinctSummedCLIN
                                                         from b in boesToCheck
                                                         where b.CLINID.HasValue &&
                                                         b.CLINID == c.CLINID
                                                         select b.Id;
            
            var boesImplicitlyReferencedByCLIN = idsOfBoesImplicitlyReferencedByCLIN.Concat(inMemoryBOEsImplicitlyReferencedByCLIN);
            
            // Get BOEs referenced implicitly through explicit WBS references on task or workspace variables
            ICollection<int> boesImplicitlyReferencedByWBS = new Collection<int>();

            if (wbsesToCheck.Any())
            {
                Collection<int> distinceSummedWBSIds = distinctSummedWBS.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).Distinct().ToCollection();
                List<FullBoe> boes = new List<FullBoe>();
                ICollection<FullWbs> wbsObject = (from w in workspace.WbsElements
                                                  where distinceSummedWBSIds.Contains(w.Id)
                                                  select w).ToCollection();

                foreach (FullWbs wbs in wbsObject.Where(w => !wbsesToCheck.Select(x => x.Id == w.Id).Any()))
                {
                    boes.AddRange(wbs.BoesWithNesting);
                }
                
                var dbBOEsImplicitlyReferencedByWBS = boes.Where(b => !wbsesToCheck.Select(x => x.Id == b.WBSID).Any()).Select(x => x.Id);
                
                
                // For modified summed WBSs, or child WBSs who have been modified to be under
                // a summed WBS, get all direct child BOEs of the summed WBS                                      select b.Id;
                var inMemoryBOEsImplicitlyReferencedByWBS = from w in distinctSummedWBS
                                                            from s in wbsesToCheck
                                                            where w.WBSID == s.Id || (FullWorkspaceHelper.GetAllParentWBS(wbsObject.First(x => x.Id == w.WBSID), workspace)).Select(x => x.Id).Contains(w.WBSID.Value)
                                                            from b in wbsObject.First(x => x.Id == w.WBSID).Boes
                                                            select b.Id;
                
                var inMemoryNestedBOEsImplicitlyReferencedByWBS = from w in distinctSummedWBS
                                                                  from s in wbsesToCheck
                                                                  where w.WBSID == s.Id
                                                                  from b in FullWorkspaceHelper.GetBoesForWbsWithNesting(wbsObject.First(x => x.Id == w.WBSID), workspace)
                                                                  where !wbsesToCheck.Select(x => x.Id == b.WBSID).Any()
                                                                  select b.Id;
                
                boesImplicitlyReferencedByWBS = dbBOEsImplicitlyReferencedByWBS.Union(inMemoryBOEsImplicitlyReferencedByWBS).Union(inMemoryNestedBOEsImplicitlyReferencedByWBS).ToList();
            }
            else
            {
                Collection<int> idsOfDistinctSummedWBS = distinctSummedWBS.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).ToCollection();

                ICollection<FullWbs> wbsObject = (from w in workspace.WbsElements
                                                  where idsOfDistinctSummedWBS.Contains(w.Id)
                                                  select w).ToCollection();


                // Get BOEs under the WBS that were not modified in-memory
                var dbBOEsImplicitlyReferencedByWBS = from w in distinctSummedWBS
                                                      where wbsObject.Any(x => x.Id == w.WBSID)
                                                      from b in FullWorkspaceHelper.GetBoesForWbsWithNesting(wbsObject.First(x => x.Id == w.WBSID), workspace)
                                                      where !boesToCheck.Select(x => x.Id == b.Id).Any()
                                                      select b.Id;
                
                // Get BOEs under the WBS that were modified in-memory
                var inMemoryBOEsImplicitlyReferencedByWBS = from w in distinctSummedWBS
                                                            from b in boesToCheck
                                                            where b.WBSID.HasValue &&
                                                                // In-memory BOE is either directly part of a summed WBS, or one of its parent WBSs is summed
                                                            (b.WBSID == w.WBSID || FullWorkspaceHelper.GetAllParentWBS(wbsObject.First(x => x.Id == b.WBSID), workspace).Select(x => x.Id).Contains(w.WBSID.Value))
                                                            select b.Id;
                
                boesImplicitlyReferencedByWBS = dbBOEsImplicitlyReferencedByWBS.Concat(inMemoryBOEsImplicitlyReferencedByWBS).ToList();
            }

            // Combine all collections of BOE IDs and return distinct set of BOEs that are referenced by any
            // summed variable within the specified BOE.
            return boesExplicitlyReferenced.Union(boesImplicitlyReferencedByCLIN).Union(boesImplicitlyReferencedByWBS).ToList();
        }
    }

    public class VariableCircularReferenceCheckerCache
    {
        private Dictionary<int, Dictionary<int, bool>> Cache { get; set; }

        public VariableCircularReferenceCheckerCache()
        {
            this.Cache = new Dictionary<int, Dictionary<int, bool>>();
        }

        public void CacheBOEConnection(int fromBOEID, int toBOEID, bool createsCircularReference)
        {
            if (!this.BOEConnectionCached(fromBOEID, toBOEID))
            {
                if (!this.Cache.ContainsKey(fromBOEID))
                {
                    this.Cache.Add(fromBOEID, new Dictionary<int, bool>());
                }

                this.Cache[fromBOEID].Add(toBOEID, createsCircularReference);
            }
            else
            {
                throw new InvalidOperationException("Code should not attempt to cache an already cached set of IDs");
            }
        }

        public bool BOEConnectionCached(int fromBOEID, int toBOEID)
        {
            return this.Cache.ContainsKey(fromBOEID) && this.Cache[fromBOEID].ContainsKey(toBOEID);
        }

        public bool BOECreatesCircularReference_Cached(int fromBOEID, int toBOEID)
        {
            if (this.BOEConnectionCached(fromBOEID, toBOEID))
            {
                return this.Cache[fromBOEID][toBOEID];
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve cached value for an non-cached set of IDs");
            }
        }
    }
}
