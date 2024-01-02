// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using IES.Standard;
    using GenBOE.Dtos;

    //// TODO - refactor to be extensions methods on the FullWorkspace. Use WI 29248
    /// <summary>
    /// Provides static methods for searching the FullWorkspace structure
    /// </summary>
    public static class FullWorkspaceHelper
    {
        #region Get using Workspace
        /// <summary>
        /// Gets the ids of all boes that use the workspace variable.
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <param name="workspaceVariableId">workspace variable</param>
        /// <returns>ids of boes that use the workspace variable</returns>
        public static ICollection<int> GetBOEIDsUsingWorkspaceVarID(FullWorkspace workspace, int workspaceVariableId)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (workspaceVariableId <= 0)
            {
                throw new ArgumentException("Value must be greater than 0.", nameof(workspaceVariableId));
            }

            return (from b in workspace.TaskElements
                    where b.WorkspaceVariableIDs.Contains(workspaceVariableId)
                    select b.BoeID).Distinct().ToCollection();
        }
        #endregion

        #region Get using WBS

        /// <summary>
        /// Gets all the child WBSes of the parent.
        /// </summary>
        /// <param name="parentWbs">parent WBS</param>
        /// <param name="workspace">workspace of the parentWbs</param>
        /// <returns>Collection of child WBSes</returns>
        public static ICollection<FullWbs> GetAllChildWBS(FullWbs parentWbs, FullWorkspace workspace)
        {
            if (parentWbs == null)
            {
                throw new ArgumentNullException(nameof(parentWbs));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (parentWbs.WorkspaceID != workspace.Id)
            {
                throw new ArgumentException("Workspace id of the WBS does NOT match the workspace argument.", nameof(parentWbs));
            }

            ICollection<FullWbs> allChildWbs = (from w in workspace.WbsElements
                                                where w.WbsNumber.StartsWith(parentWbs.WbsNumber + '.', StringComparison.CurrentCultureIgnoreCase)
                                                select w).ToCollection();
            return allChildWbs;
        }

        /// <summary>
        /// Gets all the parent WBSEs of the childWbs
        /// </summary>
        /// <param name="childWbs">find all ancestors of this wbs</param>
        /// <param name="workspace">workspace of the childWbs</param>
        /// <returns>Collection of all the WBS ancestors of the childWbs</returns>
        public static ICollection<WbsDTO> GetAllParentWBS(FullWbs childWbs, FullWorkspace workspace)
        {
            if (childWbs == null)
            {
                throw new ArgumentNullException(nameof(childWbs));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (childWbs.WorkspaceID != workspace.Id)
            {
                throw new ArgumentException("Workspace id of the WBS does NOT match the workspace argument.", nameof(childWbs));
            }

            ICollection<WbsDTO> allParentWbses = (from w in workspace.WbsElements
                                                  where childWbs.WbsNumber.StartsWith(w.WbsNumber + '.', StringComparison.CurrentCultureIgnoreCase)
                                                  select w).ToCollection<WbsDTO>();

            return allParentWbses;
        }

        /// <summary>
        /// Finds all BOEs associated with the wbs and all its child WBSes
        /// </summary>
        /// <param name="parentWbs">find the children for this wbs if any</param>
        /// <param name="workspace">workspace of the parentWbs</param>
        /// <returns>Collection of Boes associated with the parentWbs and all its children</returns>
        public static ICollection<FullBoe> GetBoesForWbsWithNesting(FullWbs parentWbs, FullWorkspace workspace)
        {
            if (parentWbs == null)
            {
                throw new ArgumentNullException(nameof(parentWbs));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (parentWbs.WorkspaceID != workspace.Id)
            {
                throw new ArgumentException("Workspace id of the WBS does NOT match the workspace argument.", nameof(parentWbs));
            }

            List<FullBoe> boesForWbsWithNesting = new List<FullBoe>();

            ICollection<FullWbs> allChildWbses = FullWorkspaceHelper.GetAllChildWBS(parentWbs, workspace);
            allChildWbses.Add(parentWbs);

            // now find all boes that reference these wbses
            foreach (FullWbs wbs in allChildWbses)
            {
                boesForWbsWithNesting.AddRange((from b in workspace.Boes
                                         where b.WBSID == wbs.Id
                                         select b).ToList());
            }

            return boesForWbsWithNesting;
        }

        /// <summary>
        /// Finds all BOEs that are referencing the WBS through a workspace or ordinary variable
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <param name="wbs">The WBS to check</param>
        /// <returns>The IDs of all BOEs that are referencing this WBS through a summed variable</returns>
        public static ICollection<int> GetAllBOEIDsReferencingWBS(FullWorkspace workspace, FullWbs wbs)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (wbs == null)
            {
                throw new ArgumentNullException(nameof(wbs));
            }
            if (wbs.WorkspaceID != workspace.Id)
            {
                throw new ArgumentException("Workspace id of the WBS does NOT match the workspace argument.", nameof(wbs));
            }

            // Get all workspace variables that are currently summing the WBS
            var workspaceVariablesReferencingWBS = (from w in workspace.WorkspaceVariables
                                                    where w.ValueType == VarValueType.SumOfBOEs
                                                    from s in w.SelectedBOEsToSum
                                                    where s.WBSID.HasValue && s.WBSID.Value == wbs.Id
                                                    select w).Distinct();

            // Get all ordinary variables that are currently summing the WBS
            var ordinaryVariablesReferencingWBS = (from b in workspace.Boes
                                                   from t in workspace.TaskElements.Where(x => x.BoeID == b.Id)
                                                   from o in t.OrdinaryVariables
                                                   where o.ValueType == VarValueType.SumOfBOEs
                                                   from s in o.SelectedBOEsToSum
                                                   where s.WBSID.HasValue && s.WBSID.Value == wbs.Id
                                                   select o).Distinct();

            // Get all BOEs that are referencing any of the variables in the two above collections
            var originBOEsToCheck = (from w in workspaceVariablesReferencingWBS
                                     from b in FullWorkspaceHelper.GetBOEIDsUsingWorkspaceVarID(workspace, w.Id)
                                     select b).Union((from o in ordinaryVariablesReferencingWBS
                                                      select o.BoeID)).ToList();

            return originBOEsToCheck;
        }

        /// <summary>
        /// Retrieves all the task variable ids associated with the wbs
        /// </summary>
        /// <param name="wbsId">wbs id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variable ids associated with the wbs</returns>
        public static ICollection<int> GetTaskVariableIdsAssociatedWithWbs(int wbsId, FullWorkspace workspace)
        {
            if (wbsId <= 0)
            {
                throw new ArgumentException("wbsId must be greater than zero");
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<int> idsToReturn = (from t in workspace.TaskElements
                                            from o in t.OrdinaryVariables
                                            from s in o.SelectedBOEsToSum
                                            where s.WBSID == wbsId && s.OrdinaryVariableID.HasValue
                                            select s.OrdinaryVariableID.Value).ToCollection();

            return idsToReturn;
        }

        /// <summary>
        /// Retrieves all the workspace variable ids associated with the wbs
        /// </summary>
        /// <param name="wbsId">wbs id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variable ids associated with the wbs</returns>
        public static ICollection<int> GetWorkspaceVariableIdsAssociatedWithWbs(int wbsId, FullWorkspace workspace)
        {
            if (wbsId <= 0)
            {
                throw new ArgumentException("wbsId must be greater than zero");
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<int> idsToReturn = (from w in workspace.WorkspaceVariables
                                            from s in w.SelectedBOEsToSum
                                            where s.WBSID.HasValue && s.WBSID == wbsId && s.OrdinaryVariableID.HasValue
                                            select s.OrdinaryVariableID.Value).Distinct().ToCollection();

            return idsToReturn;
        }
        #endregion

        #region Get using BOE
        /// <summary>
        /// Finds a boe in the workspace with the id.
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <param name="boeId">id of boe to find</param>
        /// <returns>found boe</returns>
        public static FullBoe GetBoeById(FullWorkspace workspace, int boeId)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (boeId <= 0)
            {
                throw new ArgumentException("Id must be greater than 0.", nameof(boeId));
            }

            FullBoe foundBoe = workspace.Boes.FirstOrDefault(x => x.Id == boeId);
            if (foundBoe == null)
            {
                throw new ArgumentException("Workspace does not contain a boe with id of " + boeId, nameof(workspace));
            }

            return foundBoe;
        }

        /// <summary>
        /// Gets the ids of all boes that are impacted by the deletion of the boe.
        /// </summary>
        /// <param name="idOfBoeBeingDeleted">boe being deleted</param>
        /// <param name="workspace">workspace</param>
        /// <param name="otherBoesBeingDeleted">ids of other boes being deleted</param>
        /// <returns>Collection of boes impacted by the boe deletion</returns>
        public static ICollection<int> GetBoeIdsImpactedByDeletedBoe(
            int idOfBoeBeingDeleted,
            FullWorkspace workspace,
            ICollection<int> otherBoesBeingDeleted)
        {
            if (idOfBoeBeingDeleted <= 0)
            {
                throw new ArgumentException("boe id must be greater than 0.");
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (otherBoesBeingDeleted == null)
            {
                throw new ArgumentNullException(nameof(otherBoesBeingDeleted));
            }

            ICollection<int> boesImpactedByChange = new Collection<int>();

            List<int> currentBoeIds = (from te in workspace.TaskElements
                                       from o in te.OrdinaryVariables
                                       where o.BoeID == idOfBoeBeingDeleted
                                       select te.BoeID).Distinct().ToList();

            var workspaceVarsUsedByBOE = (from x in workspace.WorkspaceVariables
                                                from s in x.SelectedBOEsToSum
                                                where s.BoeID == idOfBoeBeingDeleted
                                                select x.Id).Distinct();

            HashSet<int> workspaceVarsUsedByBOEasHashSet = new HashSet<int>(workspaceVarsUsedByBOE);

            // now grab the boe ids of the taskelements that use those same workspace variables
            Collection<int> idsOfBoesImpactedByWorkspaceVar = (from te in workspace.TaskElements
                                                               from wvId in te.WorkspaceVariableIDs
                                                               where workspaceVarsUsedByBOEasHashSet.Contains(wvId)
                                                               select te.BoeID).Distinct().ToCollection();
            // combine the 2 lists of boe ids into one
            currentBoeIds = currentBoeIds.Union(idsOfBoesImpactedByWorkspaceVar).ToList();

            // remove all ids of boes that are being deleted so we don't waste time on them
            currentBoeIds.RemoveAll(x => otherBoesBeingDeleted.Contains(x));

            ICollection<int> boesImpacted = (from b in workspace.Boes
                                                 where currentBoeIds.Contains(b.Id)
                                                 select b.Id).ToCollection();

            // recursive for all inside boes
            foreach (int aboeId in boesImpacted)
            {
                if (aboeId != idOfBoeBeingDeleted)
                {
                    boesImpactedByChange = boesImpactedByChange.Union(GetBoeIdsImpactedByDeletedBoe(aboeId, workspace, otherBoesBeingDeleted)).Distinct().ToCollection();
                }
            }

            return boesImpactedByChange;
        }

        /// <summary>
        /// Retrieves all the TaskElement variables associated with the boe
        /// </summary>
        /// <param name="boeId">boe id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variables associated with the boe</returns>
        public static ICollection<int> GetTaskVariableIdsAssociatedWithBoe(int boeId, FullWorkspace workspace)
        {
            ICollection<int> idsToReturn = (from v in FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(boeId, workspace)
                                            select v.Id).ToCollection();

            return idsToReturn;
        }

        /// <summary>
        /// Retrieves all the TaskElement variables associated with the boe
        /// </summary>
        /// <param name="boeId">boe id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variables associated with the boe</returns>
        public static ICollection<OrdinaryVariableDto> GetTaskVariablesAssociatedWithBoe(int boeId, FullWorkspace workspace)
        {
            if (boeId <= 0)
            {
                throw new ArgumentException("boeId must be greater than zero");
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            IDictionary<int, OrdinaryVariableDto> results = new Dictionary<int, OrdinaryVariableDto>();

            // determine variables that are mapped DIRECTLY to the BOE ...
            ICollection<OrdinaryVariableDto> taskVariablesAssociatedWithBoe =
                (from t in workspace.TaskElements
                 from v in t.OrdinaryVariables
                 from sum in v.SelectedBOEsToSum
                 where sum.BoeID.HasValue && sum.BoeID.Value == boeId
                 select v).ToList();

            // ... and add them to the results
            foreach (OrdinaryVariableDto tvar in taskVariablesAssociatedWithBoe)
            {
                results[tvar.Id] = tvar;
            }

            // determine variables that are INDIRECTLY mapped to the BOE (via a parent WBS or CLIN)
            ICollection<OrdinaryVariableDto> allOrdinaryVariables = workspace.TaskElements.SelectMany(t => t.OrdinaryVariables).ToList();
            ICollection<SelectBOEsToSum> unresolvedBOEsToSum = allOrdinaryVariables.SelectMany(v => v.SelectedBOEsToSum).Where(s => !s.BoeID.HasValue).ToList();

            // if there are WBS-linked or CLIN-linked sum-of-BOE mappings ...
            if (unresolvedBOEsToSum.Any())
            {
                // ... then look for a match in their child BOEs
                foreach (SelectBOEsToSum unresolvedMapping in unresolvedBOEsToSum)
                {
                    bool isChildBOE = false;

                    if (unresolvedMapping.WBSID.HasValue)
                    {
                        int wbsID = unresolvedMapping.WBSID.Value;
                        FullWbs wbs = workspace.WbsElements.FirstOrDefault(w => w.Id == wbsID);
                        if (wbs != null)
                        {
                            // is the BOE a child of this WBS?
                            isChildBOE = wbs.BoesWithNesting.Select(b => b.Id).Contains(boeId);
                        }
                    }
                    else if (unresolvedMapping.CLINID.HasValue)
                    {
                        int clinID = unresolvedMapping.CLINID.Value;

                        // is the BOE a child of this CLIN?
                        isChildBOE = workspace.Boes.Where(b => b.CLINID.HasValue && b.CLINID.Value == clinID).Select(b => b.Id).Contains(boeId);
                    }

                    if (isChildBOE)  // if the BOE is a child ...
                    {
                        int ordinaryVariableID = unresolvedMapping.OrdinaryVariableID.HasValue ? unresolvedMapping.OrdinaryVariableID.Value : 0;

                        // ... then add the task variable
                        OrdinaryVariableDto taskVariable;
                        if ((taskVariable = allOrdinaryVariables.FirstOrDefault(v => v.Id == ordinaryVariableID)) != null)
                        {
                            if (!unresolvedMapping.ChildBoeIDs.Contains(boeId))
                            {
                                unresolvedMapping.ChildBoeIDs.Add(boeId);
                            }

                            results[ordinaryVariableID] = taskVariable;
                        }
                    }
                }  // end foreach
            }

            return results.Values;
        }
        
        /// <summary>
        /// Retrieves all the workspace variables associated with the boe
        /// </summary>
        /// <param name="boeId">boe id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variables associated with the boe</returns>
        public static ICollection<WorkspaceVariableDTO> GetWorkspaceVariablesAssociatedWithBoe(int boeId, FullWorkspace workspace)
        {
            if (boeId <= 0)
            {
                throw new ArgumentException("boeId must be greater than zero");
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            IDictionary<int, WorkspaceVariableDTO> results = new Dictionary<int, WorkspaceVariableDTO>();

            // determine variables that are mapped DIRECTLY to the BOE ...
            ICollection<WorkspaceVariableDTO> workspaceVariablesAssociatedWithBoe =
                (from v in workspace.WorkspaceVariables
                 from sum in v.SelectedBOEsToSum
                 where sum.BoeID.HasValue && sum.BoeID.Value == boeId
                 select v).ToList();

            // ... and add them to the results
            foreach (WorkspaceVariableDTO wsvar in workspaceVariablesAssociatedWithBoe)
            {
                results[wsvar.Id] = wsvar;
            }

            // determine variables that are INDIRECTLY mapped to the BOE (via a parent WBS or CLIN)
            ICollection<SelectBOEsToSum> unresolvedBOEsToSum = workspace.WorkspaceVariables.SelectMany(v => v.SelectedBOEsToSum).Where(s => !s.BoeID.HasValue).ToList();

            // if there are WBS-linked or CLIN-linked sum-of-BOE mappings ...
            if (unresolvedBOEsToSum.Any())
            {
                // ... then look for a match in their child BOEs
                foreach (SelectBOEsToSum unresolvedMapping in unresolvedBOEsToSum)
                {
                    bool isChildBOE = false;

                    if (unresolvedMapping.WBSID.HasValue)
                    {
                        int wbsID = unresolvedMapping.WBSID.Value;
                        FullWbs wbs = workspace.WbsElements.FirstOrDefault(w => w.Id == wbsID);
                        if (wbs != null)
                        {
                            // is the BOE a child of this WBS?
                            isChildBOE = wbs.BoesWithNesting.Select(b => b.Id).Contains(boeId);
                        }
                    }
                    else if (unresolvedMapping.CLINID.HasValue)
                    {
                        int clinID = unresolvedMapping.CLINID.Value;

                        // is the BOE a child of this CLIN?
                        isChildBOE = workspace.Boes.Where(b => b.CLINID.HasValue && b.CLINID.Value == clinID).Select(b => b.Id).Contains(boeId);
                    }

                    if (isChildBOE)  // if the BOE is a child ...
                    {
                        int workspaceVariableID = unresolvedMapping.OrdinaryVariableID.HasValue ? unresolvedMapping.OrdinaryVariableID.Value : 0;

                        // ... then add the task variable
                        WorkspaceVariableDTO workspaceVariable;
                        if ((workspaceVariable = workspace.WorkspaceVariables.FirstOrDefault(v => v.Id == workspaceVariableID)) != null)
                        {
                            if (!unresolvedMapping.ChildBoeIDs.Contains(boeId))
                            {
                                unresolvedMapping.ChildBoeIDs.Add(boeId);
                            }

                            results[workspaceVariableID] = workspaceVariable;
                        }
                    }
                }  // end foreach
            }

            return results.Values;
        }

        /// <summary>
        /// Get the ResourceTypeDtos associated with the boe
        /// </summary>
        /// <param name="boeId">id of the boe</param>
        /// <param name="workspace">workspace</param>
        /// <returns>Collection of ResourceTypeDtos</returns>
        public static ICollection<ResourceTypeDto> GetLaborTypesForBoeId(int boeId, FullWorkspace workspace)
        {
            if (boeId <= 0)
            {
                throw new ArgumentException("boeId must be greater than zero");
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<ResourceTypeDto> toReturn = (from te in workspace.TaskElements
                                                     from r in te.taskElementLabors
                                                     where te.BoeID == boeId && te.TaskElementType == TaskElementType.Labor
                                                     select r).ToCollection();

            return toReturn;
        }
        #endregion

        #region Get using CLIN

        /// <summary>
        /// Finds all BOEs that are referencing the Clin through a workspace or ordinary variable
        /// </summary>
        /// <param name="workspace">workspace to search</param>
        /// <param name="clin">The Clin to check</param>
        /// <returns>The IDs of all BOEs that are referencing this Clin through a summed variable</returns>
        public static ICollection<int> GetAllBOEIDsReferencingCLIN(FullWorkspace workspace, ClinDTO clin)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (clin == null)
            {
                throw new ArgumentNullException(nameof(clin));
            }
            if (clin.WorkspaceID != workspace.Id)
            {
                throw new ArgumentException("Workspace id of the CLIN does NOT match the workspace argument.", nameof(clin));
            }

            // Get all workspace variables that are currently summing the CLIN
            ICollection<int> workspaceVariablesReferencingCLIN = (from w in workspace.WorkspaceVariables
                                                                  where w.ValueType == VarValueType.SumOfBOEs
                                                                  from s in w.SelectedBOEsToSum
                                                                  where s.CLINID.HasValue && s.CLINID.Value == clin.Id
                                                                  select w.Id).Distinct().ToCollection();

            // Get all ordinary variables that are currently summing the WBS
            var ordinaryVariablesReferencingCLIN = (from b in workspace.Boes
                                                    from t in workspace.TaskElements.Where(x => x.BoeID == b.Id)
                                                    from o in t.OrdinaryVariables
                                                    where o.ValueType == VarValueType.SumOfBOEs
                                                    from s in o.SelectedBOEsToSum
                                                    where s.CLINID.HasValue && s.CLINID.Value == clin.Id
                                                    select o).Distinct();

            var originBOEsToCheck = (from w in workspaceVariablesReferencingCLIN
                                     from b in FullWorkspaceHelper.GetBOEIDsUsingWorkspaceVarID(workspace, w)
                                     select b).Union((from o in ordinaryVariablesReferencingCLIN
                                                      select o.BoeID)).ToList();

            return originBOEsToCheck;
        }

        /// <summary>
        /// Retrieves all the TaskElement variables associated with the clin
        /// </summary>
        /// <param name="clinId">clin id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variables associated with the clin</returns>
        public static ICollection<int> GetTaskVariableIdsAssociatedWithClin(int clinId, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<int> taskVariableIdsAssociatedWithClin =
                (from t in workspace.TaskElements
                 from o in t.OrdinaryVariables
                 from s in o.SelectedBOEsToSum
                 where s.CLINID == clinId && s.OrdinaryVariableID.HasValue
                 select s.OrdinaryVariableID.Value).Distinct().ToCollection();

            return taskVariableIdsAssociatedWithClin;
        }

        /// <summary>
        /// Retrieves all the Workspace variable ids associated with the clin
        /// </summary>
        /// <param name="clinId">clin id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variable ids associated with the clin</returns>
        public static ICollection<int> GetWorkspaceVariableIdsAssociatedWithClin(int clinId, FullWorkspace workspace)
        {
            ICollection<int> workspaceVariableIdsAssociatedWithClin = 
                (from w in FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithClin(clinId, workspace)
                     select w.Id).Distinct().ToCollection();

            return workspaceVariableIdsAssociatedWithClin;
        }

        /// <summary>
        /// Retrieves all the Workspace variables associated with the clin
        /// </summary>
        /// <param name="clinId">clin id</param>
        /// <param name="workspace">workspace to search</param>
        /// <returns>Collection of task variable ids associated with the clin</returns>
        public static ICollection<WorkspaceVariableDTO> GetWorkspaceVariablesAssociatedWithClin(int clinId, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<WorkspaceVariableDTO> workspaceVariablesAssociatedWithClin =
                (from w in workspace.WorkspaceVariables
                 from s in w.SelectedBOEsToSum
                 where s.CLINID.HasValue && s.CLINID == clinId && s.OrdinaryVariableID.HasValue
                 select w).ToCollection();

            return workspaceVariablesAssociatedWithClin;
        }

        #endregion



    }
}
