// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class ConflictBOE : IConflictBOE
    {
        IResourceDTODataLoader _IResourceDTODataLoader;
        VariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private IPerformingOrgDTODataLoader perfOrgLoader;

        public ConflictBOE(
            IResourceDTODataLoader inIResourceDTODataLoader,
            VariableCircularReferenceChecker inVariableCircularReferenceChecker,
            IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this._IResourceDTODataLoader = inIResourceDTODataLoader;
            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;

            this.perfOrgLoader = perfOrgLoader;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ICollection<BoesWithConflicts> CopyBOEConflicts(FullBoe sourceBOE, FullBoe destinationBOE, ICollection<int> selectedTaskElementsToCopy)
        {
            var toReturn = new Collection<BoesWithConflicts>();

            if (sourceBOE == null)
            {
                throw new ArgumentNullException(nameof(sourceBOE));
            }

            if (destinationBOE == null)
            {
                throw new ArgumentNullException(nameof(destinationBOE));
            }

            // BOE Copy
            var destinationWorkspace = destinationBOE.Workspace;            
            
            // Get source task elements
            var taskElementsToCopy = sourceBOE.TaskElements;
            // Only consider individual task elements to be copied.
            taskElementsToCopy = taskElementsToCopy.Where(id => selectedTaskElementsToCopy.Contains(id.Id)).ToCollection();
            // Get collection of WS Vars in the destination workspace
            var destinationWorkspaceVariables = destinationBOE.WorkspaceVariables;  

            foreach (var taskElementToCopy in taskElementsToCopy)
            {
                // Collection to track conflict results
                var taskConflicts = new Collection<CopyBOEConflictResults>();
                // Get any workspace variables that are associated with the taskElementToCopy.
                ICollection<WorkspaceVariableDTO> sourceTaskElementWorkspaceVariables = sourceBOE.WorkspaceVariables.Where(i => taskElementToCopy.WorkspaceVariableIDs.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();                
                var conflictedWorkspaceVariables = this.GetWorkspaceVariableConflicts(sourceTaskElementWorkspaceVariables, destinationWorkspaceVariables);
                var circularReferencedWorkspaceVariables = this.GetWorkspaceVariableCircularReferences(destinationBOE.Id, taskElementToCopy, sourceTaskElementWorkspaceVariables, destinationWorkspaceVariables, destinationWorkspace);
                var circularReferencedOrdinaryVariables = this.GetOrdinaryVariableCircularReferences(destinationBOE.Id, taskElementToCopy, destinationWorkspaceVariables, destinationWorkspace);
                var conflictedResources = this.GetResourceConflicts(taskElementToCopy, destinationWorkspace.ResourcesForWsResourceListId);
                var conflictedPerformingOrgs = this.GetPerformingOrgConflicts(taskElementToCopy, destinationWorkspace.PerformingOrgsForWsList);
                var uniqueTaskID = this.GetTaskIDUniqueness(taskElementToCopy, destinationBOE);

                // Denote Workspace variable conflicts if there are any
                if (conflictedWorkspaceVariables.Any() && !taskConflicts.Contains(CopyBOEConflictResults.WorkSpaceVariables))
                {
                    taskConflicts.Add(CopyBOEConflictResults.WorkSpaceVariables);
                }

                // Denote Workspace variable conflicts if there are any
                if (circularReferencedWorkspaceVariables.Any() && !taskConflicts.Contains(CopyBOEConflictResults.WorkspaceVariableCircularReferences))
                {
                    taskConflicts.Add(CopyBOEConflictResults.WorkspaceVariableCircularReferences);
                }

                // Denote Workspace variable conflicts if there are any
                if (circularReferencedOrdinaryVariables.Any() && !taskConflicts.Contains(CopyBOEConflictResults.OrdinaryVariableCircularReferences))
                {
                    taskConflicts.Add(CopyBOEConflictResults.OrdinaryVariableCircularReferences);
                }                

                // Denote Resource conflicts if there are any
                if (conflictedResources.Any() && !taskConflicts.Contains(CopyBOEConflictResults.Resources))
                {
                    taskConflicts.Add(CopyBOEConflictResults.Resources);
                }

                // Denote Performing Org conflicts if there are any
                if (conflictedPerformingOrgs.Any() && !taskConflicts.Contains(CopyBOEConflictResults.PerformingOrgs))
                {
                    taskConflicts.Add(CopyBOEConflictResults.PerformingOrgs);
                }

                // denote task id conflicts if there are any
                if (!string.IsNullOrEmpty(uniqueTaskID))
                {
                    taskConflicts.Add(CopyBOEConflictResults.TaskIDUniqueness);
                }

                if (taskConflicts.Any())
                {
                    toReturn.Add(
                        new BoesWithConflicts(
                            taskElementToCopy,
                            conflictedWorkspaceVariables,
                            circularReferencedWorkspaceVariables,
                            circularReferencedOrdinaryVariables,
                            conflictedPerformingOrgs,
                            conflictedResources,
                            taskConflicts,
                            null,
                            uniqueTaskID)
                            );
                }
            }

            // Display the next Tasks and Conflicts
            return toReturn;
        }        

        private ICollection<WorkspaceVariableDTO> GetWorkspaceVariableConflicts(
            ICollection<WorkspaceVariableDTO> inTaskElementWorkspaceVariables,
            IReadOnlyCollection<WorkspaceVariableDTO> inDestinationWorkspaceVariables)
        {
            // Get workspace variables used by the tasks that have matching workspace variables in the new workspace
            return (from sourceVariable in inTaskElementWorkspaceVariables.Distinct()
                   from destinationVariable in inDestinationWorkspaceVariables
                   where sourceVariable.WorkspaceVariableName.Equals(destinationVariable.WorkspaceVariableName, StringComparison.CurrentCultureIgnoreCase)
                   select sourceVariable).ToList();
        }

        private ICollection<ResourceDTO> GetResourceConflicts(BoeTaskElementDTO inTaskElement, IReadOnlyCollection<ResourceDTO> inDestinationResources)
        {
            // Get resources used by the tasks that have matching resources in the new workspace
            var distinctResourceIDs = (from r in inTaskElement.taskElementLabors
                                       where r.ResourceID.HasValue
                                       select r.ResourceID.Value).Distinct().ToList();

            ICollection<ResourceDTO> resourcesForIds = this._IResourceDTODataLoader.GetByIds(distinctResourceIDs);

            return (from sourceResource in distinctResourceIDs.Select(r => resourcesForIds.First(x => x.Id == r))
                   where !inDestinationResources.Select(c => c.ResourceName.ToLower()).Contains(sourceResource.ResourceName.ToLower())
                   select sourceResource).ToList();
        }

        private ICollection<PerformingOrgDTO> GetPerformingOrgConflicts(
            BoeTaskElementDTO inTaskElement,
            IReadOnlyCollection<PerformingOrgDTO> inDestinationPerformingOrgs)
        {
            // Get perf orgs used by the tasks that have matching perf orgs in the new workspace
            List<int> distinctPerformingOrgIDs = (from r in inTaskElement.taskElementLabors
                                                  where r.PerformingOrgID.HasValue
                                                  select r.PerformingOrgID.Value).Distinct().ToList();

            Collection<PerformingOrgDTO> performingOrgs = this.perfOrgLoader.GetByIds(distinctPerformingOrgIDs);

            return (from sourcePerformingOrg in performingOrgs
                   where !inDestinationPerformingOrgs.Select(c => c.PerformingOrgName.ToLower()).Contains(sourcePerformingOrg.PerformingOrgName.ToLower())
                    select sourcePerformingOrg).ToList();
        }

        /// <summary>
        /// Gets any circular references that may occur due to a task element copy. 
        /// </summary>
        /// <param name="inDestinationBOEID">Destination copy Boe.</param>
        /// <param name="inTaskElement">Source Task Element to copy.</param>
        /// <param name="inTaskElementWorkspaceVariables">Workspace variables associated to the source Task Element.</param>
        /// <param name="inDestinationWorkspaceVariables">Workspace variables currently part of the destination copy workspace.</param>
        /// <returns></returns>
        private ICollection<WorkspaceVariableDTO> GetWorkspaceVariableCircularReferences(
            int inDestinationBOEID,
            BoeTaskElementDTO inTaskElement,
            ICollection<WorkspaceVariableDTO> inTaskElementWorkspaceVariables,
            IReadOnlyCollection<WorkspaceVariableDTO> inDestinationWorkspaceVariables,
            FullWorkspace workspace)
        {
            var workspaceVariableNames = inTaskElementWorkspaceVariables.Select(i => i.WorkspaceVariableName.ToLower());

            var ordinaryVariableNames = inTaskElement.OrdinaryVariables.Select(o => o.OrdinaryVariableName.ToLower());

            var allVariableNames = workspaceVariableNames.Union(ordinaryVariableNames);
            
            var cache = new VariableCircularReferenceCheckerCache();

            return (from w in inDestinationWorkspaceVariables
                   where w.ValueType == VarValueType.SumOfBOEs &&
                         allVariableNames.Contains(w.WorkspaceVariableName.ToLower()) &&
                         this._VariableCircularReferenceChecker.WorkspaceVariableCreatesCircularReference(cache, inDestinationBOEID, w, workspace)
                   select w).ToList();
        }

        private ICollection<OrdinaryVariableDto> GetOrdinaryVariableCircularReferences(
            int inDestinationBOEID,
            BoeTaskElementDTO inTaskElement,
            IReadOnlyCollection<WorkspaceVariableDTO> inDestinationWorkspaceVariables,
            FullWorkspace destinationWorkspace)
        {
            var workspaceVariableNames = inDestinationWorkspaceVariables.Select(w => w.WorkspaceVariableName.ToLower());

            var ordinaryVariables = from o in inTaskElement.OrdinaryVariables
                                    where !workspaceVariableNames.Contains(o.OrdinaryVariableName.ToLower())
                                    select o;

            var cache = new VariableCircularReferenceCheckerCache();

            return (from o in ordinaryVariables
                   where o.ValueType == VarValueType.SumOfBOEs && this._VariableCircularReferenceChecker.OrdinaryVariablesCreateCircularReference(cache, inDestinationBOEID, new Collection<OrdinaryVariableDto> { o }, destinationWorkspace).Any()
                    select o).ToList();
        }

        private string GetTaskIDUniqueness(BoeTaskElementDTO inTaskElement, FullBoe inDestinationBOE)
        {
            string taskID = string.Empty;
            if (inTaskElement.BOETaskID != null)
            {
                var matchingTaskID = (from t in inDestinationBOE.TaskElements
                                      where t.BOETaskID != null &&
                                            t.BOETaskID.Equals(inTaskElement.BOETaskID, StringComparison.CurrentCulture)
                                      select t).Any();

                var matchingTaskIDForODC = (from t in inDestinationBOE.OtherDirectCosts
                                            where t.TaskID != null &&
                                                  t.TaskID.Equals(inTaskElement.BOETaskID, StringComparison.CurrentCulture)
                                            select t).Any();

                var matchingTaskIDForTravel = (from t in inDestinationBOE.Travels
                                               where t.TaskID != null &&
                                                     t.TaskID.Equals(inTaskElement.BOETaskID, StringComparison.CurrentCulture)
                                               select t).Any();

                var matchingTaskIDForMaterial = (from t in inDestinationBOE.Materials
                                                 where t.TaskID != null &&
                                                       t.TaskID.Equals(inTaskElement.BOETaskID, StringComparison.CurrentCulture)
                                                 select t).Any();

                // If there is already a matching task ID in the destination BOE, return the taskID so we can display it on the conflict dialog
                if (matchingTaskID || matchingTaskIDForODC || matchingTaskIDForTravel || matchingTaskIDForMaterial)
                {
                    taskID = inTaskElement.BOETaskID;
                }
            }

            return taskID;
        }

    }

    public enum CopyBOEConflictResults
    {
        None = 1,
        BoeCustomFields = 2,
        WorkSpaceVariables = 3,
        TaskElementCustomFields = 4,
        Resources = 5,
        PerformingOrgs = 6,
        LaborTypeCustomFields = 7,
        WorkspaceVariableCircularReferences = 8,
        OrdinaryVariableCircularReferences = 9,
        TaskElementType = 10,
        TaskIDUniqueness = 11
    }

    [ExcludeFromCodeCoverage]
    public class BoesWithConflicts
    {
        public BoesWithConflicts()
        {
            this.taskElement = null;
            this.workspaceVariableConflicts = new Collection<WorkspaceVariableDTO>();
            this.workspaceVariableCircularReferences = new Collection<WorkspaceVariableDTO>();
            this.ordinaryVariableCircularReferences = new Collection<OrdinaryVariableDto>();
            this.performingOrgConflicts = new Collection<PerformingOrgDTO>();
            this.resourceConflicts = new Collection<ResourceDTO>();
            this.copyBOEConflictResults = new Collection<CopyBOEConflictResults>();
            this.laborTypeConflicts = new Collection<BoeTaskElementDTO>();
        }

        public BoesWithConflicts(
            BoeTaskElementDTO inTaskElement,
            ICollection<WorkspaceVariableDTO> inWorkspaceVariableConflicts,
            ICollection<WorkspaceVariableDTO> inWorkspaceVariableCircularReferences,
            ICollection<OrdinaryVariableDto> inOrdinaryVariableCircularReferences,
            ICollection<PerformingOrgDTO> inPerformingOrgConflicts,
            ICollection<ResourceDTO> inResourceConflicts,
            ICollection<CopyBOEConflictResults> inCopyBOEConflictResults,
            ICollection<BoeTaskElementDTO> inLaborTypeConflicts,
            string inTaskIDConflict)
            : this()
        {
            this.taskElement = inTaskElement;

            if (inWorkspaceVariableConflicts != null) {
                this.workspaceVariableConflicts = inWorkspaceVariableConflicts; }

            if (inWorkspaceVariableCircularReferences != null) {
                this.workspaceVariableCircularReferences = inWorkspaceVariableCircularReferences; }

            if (inOrdinaryVariableCircularReferences != null) {
                this.ordinaryVariableCircularReferences = inOrdinaryVariableCircularReferences; }

            if (inPerformingOrgConflicts != null) {
                this.performingOrgConflicts = inPerformingOrgConflicts; }

            if (inResourceConflicts != null) {
                this.resourceConflicts = inResourceConflicts; }

            if (inCopyBOEConflictResults != null) {
                this.copyBOEConflictResults = inCopyBOEConflictResults; }

            if (inLaborTypeConflicts != null) {
                this.laborTypeConflicts = inLaborTypeConflicts; }

            this.taskIDConflict = inTaskIDConflict;
        }

        public BoeTaskElementDTO taskElement { get; set; }
        public ICollection<WorkspaceVariableDTO> workspaceVariableConflicts { get; set; }
        public ICollection<WorkspaceVariableDTO> workspaceVariableCircularReferences { get; set; }
        public ICollection<OrdinaryVariableDto> ordinaryVariableCircularReferences { get; set; }
        public ICollection<PerformingOrgDTO> performingOrgConflicts { get; set; }
        public ICollection<ResourceDTO> resourceConflicts { get; set; }
        public ICollection<CopyBOEConflictResults> copyBOEConflictResults { get; set; }
        public ICollection<BoeTaskElementDTO> laborTypeConflicts { get; set; }
        public string taskIDConflict { get; set; }

    }
}
