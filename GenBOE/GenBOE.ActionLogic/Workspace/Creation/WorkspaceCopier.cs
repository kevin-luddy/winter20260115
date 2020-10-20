// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Workspace.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.ObjectBuilder2;

    public class WorkspaceCopier
    {
        private IWorkspaceVariableDTODataLoader _WorkspaceVariableLoader;
        private IPermissionsDTODataLoader _PermissionsLoader;
        private IResourceDTODataLoader _ResourceDTODataLoader;
        private ICustomFieldDTODataLoader _CustomFieldDTODataLoader;
        private ICustomFieldValueDTODataLoader _CustomFieldValueDTODataLoader;
        private IBoeTaskElementDTODataLoader _BoeTaskElementLoader;
        private ITravelDTODataLoader _TravelLoader;
        private IBoeMediator _BoeMediator;
        private IFullObjectFactory factory;
        private IClinDTODataLoader clinLoader;
        private IWorkspaceDTODataLoader workspaceLoader;
        private IWbsDTODataLoader wbsLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private IBOEFormIBOEDTODataLoader iboeDataLoader;
        private IBOEFormPBOEDTODataLoader pboeDataLoader;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesLoader;
        private IRteTemplateDataLoader rteTemplateDataLoader;

        List<BoeTaskElementDTO> copiedFromTaskElements = new List<BoeTaskElementDTO>();
        List<TravelDTO> copiedFromTravelElements = new List<TravelDTO>();

        public WorkspaceCopier(
            IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            IResourceDTODataLoader inResourceDTODataLoader,
            ICustomFieldDTODataLoader inCustomFieldDTODataLoader,
            ICustomFieldValueDTODataLoader inCustomFieldValueDTODataLoader,
            IBoeTaskElementDTODataLoader inBoeTaskElementLoader,
            ITravelDTODataLoader inTravelLoader,
            IBoeMediator inBoeMediator,
            IFullObjectFactory factory,
            IClinDTODataLoader clinLoader,
            IWorkspaceDTODataLoader workspaceLoader,
            IWbsDTODataLoader wbsLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IBOEFormIBOEDTODataLoader iboeDataLoader,
            IBOEFormPBOEDTODataLoader pboeDataLoader,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesLoader,
            IRteTemplateDataLoader rteTemplateDataLoader)
        {
            this._WorkspaceVariableLoader = inWorkspaceVariableLoader;
            this._PermissionsLoader = inPermissionsLoader;
            this._ResourceDTODataLoader = inResourceDTODataLoader;
            this._CustomFieldDTODataLoader = inCustomFieldDTODataLoader;
            this._CustomFieldValueDTODataLoader = inCustomFieldValueDTODataLoader;
            this._BoeTaskElementLoader = inBoeTaskElementLoader;
            this._TravelLoader = inTravelLoader;
            this._BoeMediator = inBoeMediator;
            this.factory = factory;
            this.clinLoader = clinLoader;
            this.workspaceLoader = workspaceLoader;
            this.wbsLoader = wbsLoader;
            this.perfOrgLoader = perfOrgLoader;
            this.iboeDataLoader = iboeDataLoader;
            this.pboeDataLoader = pboeDataLoader;
            this.zoneTravelRatesLoader = zoneTravelRatesLoader;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
        }

        /// <summary>
        /// Performs an exact copy of a workspace
        /// </summary>
        /// <param name="workspaceIDToCopy">Workspace Id to copy</param>
        /// <param name="newWorkspaceName">New WS Name</param>
        /// <param name="newShortName">New WS Short Name</param>
        /// <param name="costVolumeLeadPricerId">Cost Volume Lead Pricer / Estimator user id</param>
        /// <returns>New Workspace Id</returns>
        public int CopyWorkspaceExactly(int workspaceIDToCopy, string newWorkspaceName, string newShortName, int costVolumeLeadPricerId)
        {
            if (workspaceIDToCopy <= 0)
            {
                throw new ArgumentNullException(nameof(workspaceIDToCopy));
            }

            return this.workspaceLoader.ExactCopyWorkspace(workspaceIDToCopy, newWorkspaceName, newShortName, costVolumeLeadPricerId);
        }

        public bool CopyWorkspace(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace, Collection<int> BOEToCopy,
            bool copyPermissions, bool copyTasks, bool copyLaborSpreads)
        {
            if (workspaceToCopy == null)
            {
                throw new ArgumentNullException(nameof(workspaceToCopy));
            }

            bool finishedCorrectly = true;

            this.CopyWorkspaceProperties(workspaceToCopy, newWorkspace);
            this.AddAuthorPermissionForCostLead(newWorkspace);
            Dictionary<int, int> ClinIDMapping = this.CopyAllCLIN(workspaceToCopy, newWorkspace);

            // Get copy of custom fields for use with CopyCustomFieldValues before any changes are made to them
            IReadOnlyCollection<CustomFieldDTO> customFieldsToCopy = workspaceToCopy.CustomFields.DeepClone();

            Dictionary<int, int> WBSIDMapping = this.CopyAllWBS(workspaceToCopy, newWorkspace, ClinIDMapping);
            Dictionary<int, int> CustomFieldIDMapping = this.CopyCustomFields(workspaceToCopy, newWorkspace);
            Dictionary<int, int> CustomFieldValueIDMapping = this.CopyCustomFieldValues(CustomFieldIDMapping, customFieldsToCopy, workspaceToCopy, BOEToCopy, copyTasks, copyLaborSpreads);

            // If the user specified to copy workspace permissions, we'll do that
            if (copyPermissions)
            {
                this.CopyPotentialPermissions(workspaceToCopy, newWorkspace);
            }

            Dictionary<int, int> BOEIDMapping = this.CopyBOEs(newWorkspace, BOEToCopy, ClinIDMapping, WBSIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping);
            Dictionary<int, int> VariableIDMapping = this.CopyWorkspaceVariables(workspaceToCopy, newWorkspace, BOEIDMapping, WBSIDMapping, ClinIDMapping);
            this.CopyINLForms(workspaceToCopy, newWorkspace);

            Dictionary<int, int> performingOrgMapping = new Dictionary<int, int>();
            Dictionary<int, int> resourceMapping = new Dictionary<int, int>();
            if (copyLaborSpreads)
            {
                performingOrgMapping = this.CopyPerformingOrganizations(newWorkspace);
                resourceMapping = this.CopyResources(newWorkspace);
            }

            if (copyTasks)
            {
                finishedCorrectly = this.CopyTasks(BOEIDMapping, WBSIDMapping, ClinIDMapping, VariableIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping, copyLaborSpreads, resourceMapping, performingOrgMapping) && finishedCorrectly;
                finishedCorrectly = this.CopyTravelTasks(newWorkspace, BOEIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping, copyLaborSpreads, ClinIDMapping, WBSIDMapping, performingOrgMapping, resourceMapping) && finishedCorrectly;
            }

            this.CopyRteCustomTemplates(workspaceToCopy, newWorkspace);

            return finishedCorrectly;
        }

        /// <summary>
        /// Copy RTE Custom Templates into new workspace
        /// </summary>
        /// <param name="workspaceToCopy">The workspace to copy.</param>
        /// <param name="newWorkspace">The workspace to copy into.</param>
        private void CopyRteCustomTemplates(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace)
        {
            ICollection<RteCustomTemplateModelView> templates = this.rteTemplateDataLoader.GetTemplates(workspaceToCopy.Id);

            int newId = -1;

            foreach (RteCustomTemplateModelView template in templates)
            {
                // Reset template
                template.Id = newId--;
                template.Updateable = UpdateType.Upsert;
                template.WorkspaceId = newWorkspace.Id;

                if (template.Questions.Any())
                {
                    template.Questions.ForEach(question =>
                    {
                        question.Id = newId--;
                        question.TemplateId = template.Id;
                        question.Updateable = UpdateType.Upsert;
                    });
                }
            }

            this.rteTemplateDataLoader.Save(templates);

            // Save Questions (Prompts)
            foreach (RteCustomTemplateModelView template in templates)
            {
                this.rteTemplateDataLoader.SaveQuestions(template.Questions, template.Id);
            }
        }

        /// <summary>
        /// Copies custom field values for each custom field from source workspace to destination workspace.
        /// </summary>
        /// <param name="customFieldIDMapping">A mapping of source custom field Ids to newly created (destination) custom field Ids.</param>
        /// <param name="customFieldsToCopy">Source custom fields</param>
        /// <param name="workspaceToCopy">Source workspace</param>
        /// <param name="boeToCopy">Collection of IDs of the BOEs to copy</param>
        /// <param name="copyTasks">Bool noting if tasks will be copied</param>
        /// <param name="copyLaborSpreads">Bool noting if resource types will be copied</param>
        /// <returns>A mapping of Original (source) Custom field value Ids to New (destination) Custom field value Ids.</returns>
        private Dictionary<int, int> CopyCustomFieldValues(Dictionary<int, int> customFieldIDMapping,
            IReadOnlyCollection<CustomFieldDTO> customFieldsToCopy, FullWorkspace workspaceToCopy,
            ICollection<int> boeToCopy, bool copyTasks, bool copyLaborSpreads)
        {
            // Create a collection to map old custom field value IDs to new copied ones
            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            // If there are custom fields to copy, let's do it
            if (customFieldIDMapping.Any())
            {
                // Get all custom field values in the source workspace.
                ICollection<CustomFieldValueDTO> customFieldValuesToCopy =
                    this._CustomFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(customFieldIDMapping
                        .Select(i => i.Key).ToCollection<int>());

                // Get full BOEs to copy - needed for open ended CF filtering
                ICollection<FullBoe> boesToCopy = new Collection<FullBoe>();

                foreach (int boeId in boeToCopy)
                {
                    FullBoe boe = workspaceToCopy.Boes.FirstOrDefault(x => x.Id == boeId);
                    if (boe != null)
                    {
                        boesToCopy.Add(boe);
                    }
                }

                // Iterate through each source custom field ID and copy its values. 
                foreach (KeyValuePair<int, int> customFieldID in customFieldIDMapping)
                {
                    // Get custom field values fro the current custom field.
                    ICollection<CustomFieldValueDTO> currentCustomFieldValuesToCopy = customFieldValuesToCopy
                        .Where(i => i.CustomFieldID == customFieldID.Key).ToCollection<CustomFieldValueDTO>();

                    CustomFieldDTO customField = customFieldsToCopy.FirstOrDefault(x => x.Id == customFieldID.Key);

                    if (customField != null && customField.IsOpenEnded)
                    {
                        currentCustomFieldValuesToCopy = this.GetOpenEndedCustomFieldValues(customFieldValuesToCopy,
                            customField, boesToCopy, copyTasks, copyLaborSpreads);
                    }

                    Dictionary<int, int> mappingsOldToNeg = new Dictionary<int, int>();

                    // If there are custom field values to copy for the current custom field, let's do it.
                    if (currentCustomFieldValuesToCopy.Any())
                    {
                        int newItemID = -1;

                        // Iterate over each value and set values to create a copy of in the new workspace.
                        foreach (CustomFieldValueDTO customFieldValue in currentCustomFieldValuesToCopy)
                        {
                            mappingsOldToNeg.Add(--newItemID, customFieldValue.Id);

                            customFieldValue.Id = newItemID;
                            customFieldValue.CustomFieldValueID = newItemID;
                            customFieldValue.CustomFieldID = customFieldID.Value;
                            customFieldValue.Updateable = UpdateType.Upsert;
                        }

                        // Save the new custom field values
                        Dictionary<int, int> mappingsNegToNew = this._CustomFieldValueDTODataLoader.Save(currentCustomFieldValuesToCopy);

                        // remap..
                        Dictionary<int, int> idMapping = new Dictionary<int, int>();

                        foreach (KeyValuePair<int, int> oldPair in mappingsOldToNeg)
                        {
                            int newKey = mappingsNegToNew[oldPair.Key];

                            idMapping.Add(oldPair.Value, newKey);
                        }

                        toReturn = toReturn.Concat(idMapping).ToDictionary(t => t.Key, t => t.Value);

                    }
                }
            }

            // Return the mapping of original custom fields IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Filters the custom field values for Open Ended Custom Fields so we don't copy unused values
        /// </summary>
        /// <param name="allCustomFieldValues">Collection of all Custom Field Values</param>
        /// <param name="customField">The custom field</param>
        /// <param name="boesToCopy">Collection of BOEs to copy</param>
        /// <param name="copyTasks">bool noting if tasks are being copied</param>
        /// <param name="copyLaborSpreads">bool noting if labors are being copied</param>
        /// <returns>Filtered collection of custom field values</returns>
        private ICollection<CustomFieldValueDTO> GetOpenEndedCustomFieldValues(
            ICollection<CustomFieldValueDTO> allCustomFieldValues, CustomFieldDTO customField,
            ICollection<FullBoe> boesToCopy, bool copyTasks, bool copyLaborSpreads)
        {
            ICollection <CustomFieldValueDTO> toReturn = new Collection<CustomFieldValueDTO>();

            switch (customField.CustomFieldDisplayID)
            {
                case CustomFieldType.BoeDisplay:
                    foreach (FullBoe boe in boesToCopy)
                    {
                        toReturn.AddRange(allCustomFieldValues.Where(x =>
                            boe.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                .Contains(x.CustomFieldValueID)));
                    }

                    break;
                case CustomFieldType.TaskDisplay:
                {
                    if (copyTasks)
                    {
                        ICollection<BoeTaskElementDTO> tasks = boesToCopy.SelectMany(x => x.TaskElements).ToCollection();

                        foreach (BoeTaskElementDTO task in tasks)
                        {
                            toReturn.AddRange(allCustomFieldValues.Where(x =>
                                task.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                    .Contains(x.CustomFieldValueID)));
                        }
                            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                            {
                                ICollection<TravelDTO> travelTasks = boesToCopy.SelectMany(x => x.Travels).ToCollection();

                                foreach (TravelDTO travelTask in travelTasks)
                                {
                                    toReturn.AddRange(allCustomFieldValues.Where(x =>
                                        travelTask.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                            .Contains(x.CustomFieldValueID)));
                                }
                            }
                    }

                    break;
                }
                case CustomFieldType.LaborTypeDisplay:
                {
                    if (copyLaborSpreads)
                    {
                        ICollection<BoeTaskElementDTO> tasks = boesToCopy.SelectMany(x => x.TaskElements).ToCollection();
                        ICollection<ResourceTypeDto> resourceTypes =
                            tasks.SelectMany(x => x.taskElementLabors).ToCollection();

                        foreach (ResourceTypeDto resourceType in resourceTypes)
                        {
                            toReturn.AddRange(allCustomFieldValues.Where(x =>
                                resourceType.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                    .Contains(x.CustomFieldValueID)));
                        }

                        ICollection<TravelDTO> travelTasks = boesToCopy.SelectMany(x => x.Travels).ToCollection();
                        ICollection<MSTTravelTripType> trips = travelTasks.SelectMany(x => x.MSTTravelTrips).ToCollection();

                        foreach (MSTTravelTripType trip in trips)
                        {
                            toReturn.AddRange(allCustomFieldValues.Where(x =>
                                trip.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                    .Contains(x.CustomFieldValueID)));
                        }
                    }

                    break;
                }
            }

            return toReturn;
        }
        
        /// <summary>
        /// Copies custom fields from one workspace to another.
        /// </summary>
        /// <param name="workspaceToCopy">Source workspace to copy.</param>
        /// <param name="newWorkspace">Destination workspace to copy.</param>
        /// <returns>A mapping of source custom field Ids to newly created (destination) custom field Ids.</returns>
        private Dictionary<int, int> CopyCustomFields(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace)
        {
            // Create a collection to map old custom field IDs to new copied ones
            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            // Get the list of custom field IDs to copy
            IReadOnlyCollection<CustomFieldDTO> customFieldsToCopy = workspaceToCopy.CustomFields;

            // If there are custom fields to copy, let's do it
            if (customFieldsToCopy.Any())
            {
                int newItemID = -1;

                // Iterate through each existing custom field ID and set values to create a copy of it in the new workspace
                foreach (CustomFieldDTO customField in customFieldsToCopy)
                {
                    int oldCustomFieldID = customField.Id;

                    customField.Id = newItemID--;
                    customField.Updateable = UpdateType.Upsert;
                    customField.WorkspaceID = newWorkspace.Id;

                    // Save the new custom field
                    int? newCustomFieldID = this._CustomFieldDTODataLoader.Save(customField);

                    // Add the old and new IDs to the mapping to return
                    toReturn.Add(oldCustomFieldID, newCustomFieldID.Value);
                }
            }

            // Return the mapping of original custom fields IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copies performing organizations from one workspace to another
        /// </summary>
        /// <param name="workspaceToCopy"></param>
        /// <param name="newWorkspace"></param>
        private Dictionary<int, int> CopyPerformingOrganizations(WorkspaceDTO newWorkspace)
        {
            // Create a collection to map old Performing Organization IDs to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Create a list of Performing Organizations to save
            var performingOrganizationsToSave = new Collection<PerformingOrgDTO>();

            // Get a list of all in-use Perf Org IDs
            var taskOrganizationIDs = (from t in this.copiedFromTaskElements
                                       from l in t.taskElementLabors
                                       where l.PerformingOrgID.HasValue
                                       select l.PerformingOrgID.Value).Distinct();

            var ZoneTravelPerfOrgIDs = (from t in this.copiedFromTravelElements
                                    from l in t.MSTTravelTrips
                                    select l.PerfOrgID).Distinct();

            var performingOrganizationIDs = taskOrganizationIDs.Union(ZoneTravelPerfOrgIDs).ToList();

            // Create a mapping between old Perf Org IDs and new DTOs for the copied workspace
            var performingOrganizationIDMapping = new Dictionary<int, PerformingOrgDTO>();
            int newItemID = -1;
            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(performingOrganizationIDs.Distinct().ToList()));

            foreach (int performingOrganizationID in performingOrganizationIDs)
            {
                var performingOrganizationToCopy = perfOrgsFromDb.FirstOrDefault(x => x.Id == performingOrganizationID);

                // If there is a Performing Organization to copy, let's do it
                if (performingOrganizationToCopy != null)
                {
                    // Get the corresponding Performing Organization from the copied workspace
                    var newWorkspacePerformingOrganization = this.perfOrgLoader.GetByListIdAndName(newWorkspace.PerfOrgListID, performingOrganizationToCopy.PerformingOrgName);

                    if (newWorkspacePerformingOrganization != null)
                    {
                        // If the matching Performing Organization does not have matching values for Description
                        // then we'll set these values to match and add the Performing Organization to the collection to be saved
                        if (!newWorkspacePerformingOrganization.PerformingOrgDesc.Equals(performingOrganizationToCopy.PerformingOrgDesc, StringComparison.CurrentCultureIgnoreCase))
                        {
                            newWorkspacePerformingOrganization.PerformingOrgDesc = performingOrganizationToCopy.PerformingOrgDesc;
                            newWorkspacePerformingOrganization.Updateable = UpdateType.Upsert;

                            performingOrganizationsToSave.Add(newWorkspacePerformingOrganization);
                        }

                        performingOrganizationIDMapping.Add(performingOrganizationID, newWorkspacePerformingOrganization);
                    }
                    // If the matching Performing Organization name is not in the new workspace, we'll add it
                    else
                    {
                        performingOrganizationToCopy.Id = newItemID--;
                        performingOrganizationToCopy.Updateable = UpdateType.Upsert;

                        performingOrganizationsToSave.Add(performingOrganizationToCopy);
                        performingOrganizationIDMapping.Add(performingOrganizationID, performingOrganizationToCopy);
                    }
                }
            }

            if (performingOrganizationsToSave.Any())
            {
                // Save all of the new Performing Organizations
                this.perfOrgLoader.SaveWorkspacePerformingOrgs(performingOrganizationsToSave, newWorkspace.PerfOrgListID);
            }

            // Get a mapping between old and new IDs for the values
            toReturn = (from p in performingOrganizationIDMapping
                        select new
                        {
                            OldID = p.Key,
                            NewID = p.Value.Id
                        }).ToDictionary(t => t.OldID, t => t.NewID);

            // Return the mapping of original Performing Organization IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copies resources from one workspace to another
        /// </summary>
        /// <param name="workspaceToCopy"></param>
        /// <param name="newWorkspace"></param>
        private Dictionary<int, int> CopyResources(WorkspaceDTO newWorkspace)
        {
            // Create a collection to map old resource IDs to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Create a list of resources to save
            var resourcesToSave = new Collection<ResourceDTO>();

            // Get a list of all in-use Resource IDs
            var LaborResourceIDs = (from r in this.copiedFromTaskElements
                                    from e in r.taskElementLabors
                                    where e.ResourceID.HasValue
                                    select e.ResourceID.Value).Distinct();

            var travelResourceIds = (from t in this.copiedFromTravelElements
                                     from tt in t.MSTTravelTrips
                                     where tt.NonZoneResourceID.HasValue
                                     select tt.NonZoneResourceID.Value).Distinct();

            var resourceIDs = LaborResourceIDs.Union(travelResourceIds);

            // Create a mapping between old Resource IDs and new DTOs for the copied workspace
            var resourceIDMapping = new Dictionary<int, ResourceDTO>();

            int newItemID = -1;

            ICollection<ResourceDTO> resourcesForWsListId = this._ResourceDTODataLoader.GetByListId(newWorkspace.ResourceListID);
            ICollection<ResourceDTO> resourcesToCopy = this._ResourceDTODataLoader.GetByIds(resourceIDs.ToList());

            foreach (int resourceID in resourceIDs)
            {
                var resourceToCopy = resourcesToCopy.FirstOrDefault(x => x.Id == resourceID);

                // If there are resources to copy, let's do it
                if (resourceToCopy != null)
                {
                    // Get the corresponding resource from the copied workspace
                    var newWorkspaceResource = resourcesForWsListId.FirstOrDefault(x => x.ResourceName == resourceToCopy.ResourceName);

                    if (newWorkspaceResource != null)
                    {
                        // If the matching resource does not have matching values for Description, Segment/Region and Labor Type
                        // then we'll set these values to match and add the resource to the collection to be saved
                        if (!(newWorkspaceResource.ResourceDesc.Equals(resourceToCopy.ResourceDesc, StringComparison.CurrentCultureIgnoreCase) &&
                            newWorkspaceResource.SegRegion.Equals(resourceToCopy.SegRegion, StringComparison.CurrentCultureIgnoreCase) &&
                            newWorkspaceResource.LaborType.Equals(resourceToCopy.LaborType, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            newWorkspaceResource.ResourceDesc = resourceToCopy.ResourceDesc;
                            newWorkspaceResource.SegRegion = resourceToCopy.SegRegion;
                            newWorkspaceResource.LaborType = resourceToCopy.LaborType;
                            newWorkspaceResource.Updateable = UpdateType.Upsert;

                            resourcesToSave.Add(newWorkspaceResource);
                        }

                        resourceIDMapping.Add(resourceID, newWorkspaceResource);
                    }
                    // If the matching resource name is not in the new workspace, we'll add it
                    else
                    {
                        resourceToCopy.Id = newItemID--;
                        resourceToCopy.Updateable = UpdateType.Upsert;

                        resourcesToSave.Add(resourceToCopy);
                        resourceIDMapping.Add(resourceID, resourceToCopy);
                    }
                }
            }

            if (resourcesToSave.Any())
            {
                // Save all of the new resources
                this._ResourceDTODataLoader.SaveWorkspaceResources(newWorkspace, resourcesToSave);
            }

            // Get a mapping between old and new IDs for the values
            toReturn = (from r in resourceIDMapping
                        select new
                        {
                            OldID = r.Key,
                            NewID = r.Value.Id
                        }).ToDictionary(t => t.OldID, t => t.NewID);

            // Return the mapping of original resource IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copy workspace variables from one workspace to another
        /// </summary>
        private Dictionary<int, int> CopyWorkspaceVariables(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace, Dictionary<int, int> boeIDMapping, Dictionary<int, int> wbsIDMapping, Dictionary<int, int> clinIDMapping)
        {
            // Create a collection to map old variable IDs to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Get the variables to copy
            var currentWorkspaceVariables = workspaceToCopy.WorkspaceVariables; 
            var workspaceVariablesToCopy = new Collection<WorkspaceVariableDTO>();

            if (currentWorkspaceVariables.Any())
            {
                int newItemID = -1;

                // Iterate through each existing variables and set values to create a copy of it in the new workspace
                foreach (var workspaceVariable in currentWorkspaceVariables)
                {
                    WorkspaceVariableDTO newVariable = new WorkspaceVariableDTO();
                    newVariable.InUse = workspaceVariable.InUse;
                    newVariable.IsPercentage = workspaceVariable.IsPercentage;
                    newVariable.SortBOEBy = workspaceVariable.SortBOEBy;
                    newVariable.SumVariableResourceTypeIDs = workspaceVariable.SumVariableResourceTypeIDs;
                    newVariable.Updateable = workspaceVariable.Updateable;
                    newVariable.UpdateDate = workspaceVariable.UpdateDate;
                    newVariable.ValueType = workspaceVariable.ValueType;
                    newVariable.WorkspaceVariableName = workspaceVariable.WorkspaceVariableName;

                    int oldItemID = workspaceVariable.Id;
                    newVariable.Id = newItemID--;
                    bool bfound = false;
                    // Remap Sum of BOEs IDs from old to new
                    if (workspaceVariable.ValueType == VarValueType.SumOfBOEs)
                    {
                        foreach (var sumOfBOEs in workspaceVariable.SelectedBOEsToSum)
                        {
                            int matchingID;
                            if (sumOfBOEs.BoeID.HasValue && boeIDMapping.TryGetValue(sumOfBOEs.BoeID.Value, out matchingID))
                            {
                                sumOfBOEs.BoeID = matchingID;
                                newVariable.SelectedBOEsToSum.Add(sumOfBOEs);
                                if (matchingID > 0)
                                {
                                    bfound = true;
                                }
                            }
                            else if (sumOfBOEs.WBSID.HasValue && wbsIDMapping.TryGetValue(sumOfBOEs.WBSID.Value, out matchingID))
                            {
                                sumOfBOEs.WBSID = matchingID;
                                newVariable.SelectedBOEsToSum.Add(sumOfBOEs);
                                if (matchingID > 0)
                                {
                                    bfound = true;
                                }
                            }
                            else if (sumOfBOEs.CLINID.HasValue && clinIDMapping.TryGetValue(sumOfBOEs.CLINID.Value, out matchingID))
                            {
                                sumOfBOEs.CLINID = matchingID;
                                newVariable.SelectedBOEsToSum.Add(sumOfBOEs);
                                if (matchingID > 0)
                                {
                                    bfound = true;
                                }
                            }
                        }
                    }
                    else if (workspaceVariable.ValueType == VarValueType.Discrete)
                    {
                        newVariable.WorkspaceVariableValue = workspaceVariable.WorkspaceVariableValue;
                    }
                    if (!bfound) //check for variables used in tasks of the selected BOEs
                    {
                        foreach (var taskElement in this.copiedFromTaskElements)
                        {
                            if (taskElement.WorkspaceVariableIDs.Any())
                            {
                                foreach (var variableID in taskElement.WorkspaceVariableIDs)
                                {
                                    if (variableID == oldItemID)
                                    {
                                        bfound = true;
                                        break;
                                    }
                                }
                                if (bfound)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (bfound)
                    {
                        newVariable.Updateable = UpdateType.Upsert;
                    }
                    else
                    {
                        newVariable.Updateable = UpdateType.Deleted;
                    }

                    newVariable.WorkspaceID = newWorkspace.Id;

                    workspaceVariablesToCopy.Add(newVariable);
                }

                Collection<WorkspaceVariableDTO> newWorkspaceVariables = workspaceVariablesToCopy.Where(x => x.Updateable == UpdateType.Upsert).Select(x => x).ToCollection();
                // Save all of the new variables
                this._WorkspaceVariableLoader.SaveWorkspaceVariables(newWorkspaceVariables);

                // Get latest copies of old and new variables to create ID mapping
                ICollection<WorkspaceVariableDTO> savedWorkspaceVariablesToCopy = this._WorkspaceVariableLoader.GetByWorkspaceID(workspaceToCopy.Id);

                // Get a mapping between old and new IDs for the values
                toReturn = (from o in savedWorkspaceVariablesToCopy
                            from n in newWorkspaceVariables
                            where n.WorkspaceVariableName == o.WorkspaceVariableName
                            select new
                            {
                                OldID = o.Id,
                                NewID = n.Id
                            }).ToDictionary(t => t.OldID, t => t.NewID);
            }

            // Return the mapping of original variable IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copy additional workspace properties from the old to the new workspace
        /// </summary>
        private void CopyWorkspaceProperties(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace)
        {
            // Set the output format of the new workspace to that of the copied workspace
            newWorkspace.TemplateID = workspaceToCopy.TemplateID;

            // Get the user who is saving the BOE(s)
            int currentUserID = workspaceToCopy.CurrentActiveUser.UserID;
            this.workspaceLoader.SaveWorkspaceSettings(currentUserID, newWorkspace);
        }

        private Dictionary<int, int> CopyAllWBS(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace, Dictionary<int, int> ClinIDMapping)
        {
            // Create a collection to map old WBS Numbers to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Get the list of all WBS from the workspace to copy
            var wbsToCopy = workspaceToCopy.WbsElements.ToList<WbsDTO>();

            // If there are WBSs, let's copy them
            if (wbsToCopy.Any())
            {
                int newItemID = -1;
                ICollection<FullWbs> newWbsElementsToSave = new Collection<FullWbs>();
                // Iterate through each existing WBS and set values to create a copy of
                // it in the new workspace
                foreach (var wbs in wbsToCopy)
                {
                    FullWbs newWbs = this.factory.CreateFullWbs(wbs);
                    newWbs.Id = newItemID--;
                    newWbs.Updateable = UpdateType.Upsert;
                    newWbs.WorkspaceID = newWorkspace.Id;

                    // If this WBS is associated to CLINs we'll re-reference the IDs with the
                    // copied CLINs in the new workspace
                    if (wbs.ClinIDs.Any())
                    {
                        // Create a new collection for the copied CLIN IDs
                        var newCLINIDs = new Collection<int>();

                        // Iterate over old CLIN IDs and add the IDs of their copy in the new workspace
                        foreach (var clin in wbs.ClinIDs)
                        {
                            newCLINIDs.Add(ClinIDMapping[clin]);
                        }

                        // Clear out the CLIN IDs and replace with the new ones
                        newWbs.ClinIDs = newCLINIDs;
                    }

                    newWbsElementsToSave.Add(newWbs);
                }

                // Save all of the new WBS
                this.wbsLoader.Save(newWbsElementsToSave.ToCollection<WbsDTO>());

                // Get a mapping between old and new IDs for the values
                toReturn = (from o in wbsToCopy
                            from n in newWbsElementsToSave
                            where n.WbsNumber == o.WbsNumber
                            select new
                            {
                                OldID = o.Id,
                                NewID = n.Id
                            }).ToDictionary(t => t.OldID, t => t.NewID);
            }

            // Return the mapping of original WBS IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copies all CLINs from one workspace to another
        /// </summary>
        /// <param name="workspaceToCopy">The workspace to copy from</param>
        /// <param name="newWorkspace">The new workspace</param>
        /// <param name="clinMapper">CLIN Mapper</param>
        private Dictionary<int, int> CopyAllCLIN(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace)
        {
            // Create a collection to map old CLIN Numbers to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Get the list of all CLINs from the workspace to copy
            IReadOnlyCollection<FullClin> clinsToCopy = workspaceToCopy.Clins;  
            ICollection<FullClin> newClinsToInsert = new Collection<FullClin>();

            // If there are CLINs, let's copy them
            if (clinsToCopy.Any())
            {
                int newItemID = -1;
                
                // Iterate through each existing CLIN and set values to create a copy of
                // it in the new workspace
                foreach (ClinDTO clin in clinsToCopy)
                {
                    FullClin newClin = this.factory.CreateFullClin(clin);
                    newClin.Id = newItemID--;
                    newClin.Updateable = UpdateType.Upsert;
                    newClin.WorkspaceID = newWorkspace.Id;
                    newClinsToInsert.Add(newClin);
                }

                // Save all of the new CLINs
                this.clinLoader.Save(newClinsToInsert.ToCollection<ClinDTO>());

                // Get a mapping between old and new IDs for the values
                toReturn = (from o in workspaceToCopy.Clins
                            from n in newClinsToInsert
                            where n.ClinNumber == o.ClinNumber
                            select new
                            {
                                OldID = o.Id,
                                NewID = n.Id
                            }).ToDictionary(t => t.OldID, t => t.NewID);
            }

            // Return the mapping of original CLIN IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Save author permissions for the Cost Volume Lead Pricer
        /// </summary>
        /// <param name="newWorkspace">The new workspace</param>
        /// <param name="permissionsMapper">The permissions mapper</param>
        private void AddAuthorPermissionForCostLead(WorkspaceDTO newWorkspace)
        {
            // Save Author Permission for Cost Volume Lead Pricer
            var authorPermission = new PermissionsDTO();
            authorPermission.WorkspaceId = newWorkspace.Id;
            authorPermission.Role = Role.Author;
            authorPermission.ETIUserId = newWorkspace.CostVolumeLeadPricerUserID;
            authorPermission.Updateable = UpdateType.Upsert;
            this._PermissionsLoader.SavePermission(authorPermission);
        }

        /// <summary>
        /// Copies workspace permissions (Workspace Admin/User/Reviewer) and BOE permissions (Author/Approver)
        /// from one workspace to another
        /// </summary>
        /// <param name="workspaceToCopy">Workspace to copy from</param>
        /// <param name="newWorkspaceID">Workspace to copy to</param>
        private void CopyPotentialPermissions(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace)
        {
            // Create one list to hold the FROM permissions and one to hold the TO permissions
            // Set the FROM permissions to the workspace permissions
            var workspaceToCopyPermissions = this._PermissionsLoader.GetWorkspacePermissions(workspaceToCopy.Id);
            var newWorkspacePermissions = this._PermissionsLoader.GetWorkspacePermissions(newWorkspace.Id);
            var permissionsToSave = new Collection<PermissionsDTO>();

            if (workspaceToCopyPermissions.Any())
            {
                int newItemID = -1;

                // Set each existing workspace permission to reference the new workspace and reset the
                // PKID to indicate a new permission
                foreach (var workspacePermission in workspaceToCopyPermissions)
                {
                    bool permissionsExist = (from p in newWorkspacePermissions
                                             where p.ETIUserId == workspacePermission.ETIUserId &&
                                             p.Role == workspacePermission.Role
                                             select p).Any();

                    if (!permissionsExist)
                    {
                        workspacePermission.PermissionId = newItemID--;
                        workspacePermission.Updateable = UpdateType.Upsert;
                        workspacePermission.WorkspaceId = newWorkspace.Id;
                        permissionsToSave.Add(workspacePermission);
                    }
                }
            }

            // Set the FROM permissions to the BOE potential permissions
            workspaceToCopyPermissions = this._PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspaceToCopy.Id);
            newWorkspacePermissions = this._PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(newWorkspace.Id);

            // Set each existing BOE potential permission to reference the new workspace and reset the
            // PKID to indicate a new permission
            if (workspaceToCopyPermissions.Any())
            {
                int newItemID = -1;
                
                foreach (var workspacePermission in workspaceToCopyPermissions)
                {
                    bool permissionsExist = ((from p in newWorkspacePermissions
                                             where p.ETIUserId == workspacePermission.ETIUserId &&
                                             p.Role == workspacePermission.Role
                                             select p).Any()
                                             || 
                                             (from s in permissionsToSave
                                             where s.ETIUserId == workspacePermission.ETIUserId &&
                                             s.Role == workspacePermission.Role
                                             select s).Any());

                    if (!permissionsExist)
                    {
                        workspacePermission.PermissionId = newItemID--;
                        workspacePermission.Updateable = UpdateType.Upsert;
                        workspacePermission.WorkspaceId = newWorkspace.Id;
                        permissionsToSave.Add(workspacePermission);
                    }
                }
            }

            if (permissionsToSave.Any())
            {
                this._PermissionsLoader.SavePermissions(permissionsToSave);
            }
        }

        /// <summary>
        /// Copies INL Forms from workspace to new workspace.
        /// </summary>
        /// <param name="workspaceToCopy">The workspace to copy from.</param>
        /// <param name="newWorkspace">The new workspace.</param>
        private void CopyINLForms(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace)
        {
            int newItemID = -1;

            ICollection<BOEFormPBOEDTO> pboes = this.pboeDataLoader.GetByWorkspaceId(workspaceToCopy.Id);
            if (pboes.Any())
            {
                foreach (BOEFormPBOEDTO pboe in pboes)
                {
                    pboe.Id = newItemID--;
                    pboe.Updateable = UpdateType.Upsert;
                    pboe.WorkspaceId = newWorkspace.Id;

                    this.pboeDataLoader.Save(pboe);
                }
            }

            ICollection<BOEFormIBOEDTO> iboes = this.iboeDataLoader.GetByWorkspaceId(workspaceToCopy.Id);
            if (iboes.Any())
            {
                foreach (BOEFormIBOEDTO iboe in iboes)
                {
                    iboe.Id = newItemID--;
                    iboe.Updateable = UpdateType.Upsert;
                    iboe.WorkspaceId = newWorkspace.Id;

                    this.iboeDataLoader.Save(iboe);
                }
            }
        }

        /// <summary>
        /// Copies BOEs from one workspace to another
        /// </summary>
        /// <param name="newWorkspace">the new workspace</param>
        /// <param name="BOEToCopy">the boe to copy</param>
        /// <param name="ClinIDMapping">CLIN ID mapping</param>
        /// <param name="WBSIDMapping">WBS ID mappings</param>
        /// <param name="customFieldIDMapping">Workspace custom field id mappings</param>
        /// <param name="customFieldValueIDMapping">Custom Field Value Id Mapping</param>
        /// <returns>BOE ID mappings</returns>
        private Dictionary<int, int> CopyBOEs(WorkspaceDTO newWorkspace, Collection<int> BOEToCopy,
            Dictionary<int, int> ClinIDMapping, Dictionary<int, int> WBSIDMapping, Dictionary<int, int> customFieldIDMapping, Dictionary<int, int> customFieldValueIDMapping)
        {
            var toReturn = new Dictionary<int, int>();

            this.copiedFromTaskElements = new List<BoeTaskElementDTO>();
            this.copiedFromTravelElements = new List<TravelDTO>();

            if (BOEToCopy.Any())
            {
                ICollection<BoeDTO> boesToSave = new Collection<BoeDTO>();
                int newItemID = -1;

                ICollection<FullBoe> fullBoeObjects = this.factory.CreateFullBoesWithRteData(BOEToCopy);

                foreach (int boeID in BOEToCopy)
                {
                    FullBoe boe = fullBoeObjects.First(x => x.Id == boeID);
                    boe.LoadTravelRTEData();
                    boe.LoadTaskElementRTEData();

                    boe.CopySourceBoeId = boeID;
                    
                    IReadOnlyCollection<BoeTaskElementDTO> taskElements = boe.TaskElements;
                    IReadOnlyCollection<TravelDTO> travelElements = boe.Travels;

                    if (taskElements.Any())
                    {
                        this.copiedFromTaskElements.AddRange(taskElements);
                    }

                    if (travelElements.Any() && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                    {
                        this.copiedFromTravelElements.AddRange(travelElements);
                    }

                    boe.Id = newItemID--;
                    boe.WCBID = newItemID--;
                    boe.Updateable = UpdateType.Upsert;
                    boe.AuthorIDs = new Collection<int>();
                    boe.SubcontractorAuthorIDs = new Collection<int>();
                    boe.State = BOEState.Unassigned;
                    boe.NumAuthorReassigned = 0;
                        
                    if (boe.CLINID.HasValue)
                    {
                        boe.CLINID = ClinIDMapping[boe.CLINID.Value];
                    }

                    if (boe.WBSID.HasValue)
                    {
                        boe.WBSID = WBSIDMapping[boe.WBSID.Value];
                    }

                    boe.WorkspaceID = newWorkspace.Id;

                    // if there are custom cross refs, let's copy them
                    if (boe.CustomFieldValueContainers.Any())
                    {
                        foreach (CustomFieldValueContainer boeCustomFieldXRefToCopy in boe.CustomFieldValueContainers)
                        {
                            boeCustomFieldXRefToCopy.ContainerID = newItemID--;
                            boeCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                            boeCustomFieldXRefToCopy.CustomFieldValueID = customFieldValueIDMapping[boeCustomFieldXRefToCopy.CustomFieldValueID];
                            boeCustomFieldXRefToCopy.CustomFieldID = customFieldIDMapping[boeCustomFieldXRefToCopy.CustomFieldID];
                        }
                    }

                    boesToSave.Add(boe);
                }

                // Save the copied BOEs
                FullWorkspace fullWorkspace = this.factory.CreateFullWorkspace(newWorkspace);
                IDictionary<int, int> boeIDDictionary = this._BoeMediator.MediatedSaveBOEs(fullWorkspace, boesToSave);

                //clean up before using boeIDMapping
                int counter = 0;

                foreach (int boeid in BOEToCopy)
                {
                    // get the dictionary's key value given the counter index so we can pull from boeIDDictionary correctly
                    int key = boeIDDictionary.Keys.ElementAt(counter);
                    toReturn.Add(boeid, boeIDDictionary[key]);
                    counter++;
                }

                // Set all BOEs to not Upsert again. Allows future saves of BOE sub-objects.
                foreach (var boe in boesToSave)
                {
                    boe.Updateable = UpdateType.None;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Copy BOE Task Elements
        /// </summary>
        /// <param name="boeIDMapping">Boe Id Mapping</param>
        /// <param name="wbsIDMapping">Wbs Id Mapping</param>
        /// <param name="clinIDMapping">Clin Id Mapping</param>
        /// <param name="variableIDMapping">Variable Id Mapping</param>
        /// <param name="customFieldIDMapping">Workspace custom field id mappings</param>
        /// <param name="customFieldValueIDMapping">Custom Field Value Id Mapping</param>
        /// <param name="copyLaborSpreads">Copy Labor Spreads?</param>
        /// <param name="Resources">Resources</param>
        /// <param name="perfOrgs">Performing Orgs</param>
        /// <returns>Boolean indicating whether there was any bad data that the user should be notified about</returns>
        private bool CopyTasks(Dictionary<int, int> boeIDMapping, Dictionary<int, int> wbsIDMapping, Dictionary<int, int> clinIDMapping,
            Dictionary<int, int> variableIDMapping, Dictionary<int, int> customFieldIDMapping, Dictionary<int, int> customFieldValueIDMapping, bool copyLaborSpreads, Dictionary<int, int> Resources, Dictionary<int, int> perfOrgs)
        {
            bool finishedCorrectly = true;

            ICollection<BoeTaskElementDTO> taskElementsToSave = new Collection<BoeTaskElementDTO>();

            int newItemID = -1;

            foreach (var taskElement in this.copiedFromTaskElements)
            {
                BoeTaskElementDTO newTaskElement = new BoeTaskElementDTO();

                int newBoeID = boeIDMapping[taskElement.BoeID];

                taskElement.Id = newItemID--;
                taskElement.BoeID = newBoeID;
                taskElement.Updateable = UpdateType.Upsert;

                foreach (var ordinaryVariable in taskElement.OrdinaryVariables)
                {
                    OrdinaryVariableDto newOrdinaryVariable = new OrdinaryVariableDto();
                    newOrdinaryVariable.Id = newItemID--;
                    newOrdinaryVariable.BoeID = newBoeID;
                    newOrdinaryVariable.IsPercentage = ordinaryVariable.IsPercentage;
                    newOrdinaryVariable.OrdinaryVariableName = ordinaryVariable.OrdinaryVariableName;
                    newOrdinaryVariable.SortBOEBy = ordinaryVariable.SortBOEBy;
                    newOrdinaryVariable.SumVariableResourceTypeIDs = ordinaryVariable.SumVariableResourceTypeIDs;
                    newOrdinaryVariable.UpdateDate = ordinaryVariable.UpdateDate;
                    newOrdinaryVariable.ValueType = ordinaryVariable.ValueType;
                    newOrdinaryVariable.OrdinaryVariableValue = ordinaryVariable.OrdinaryVariableValue;
                    newOrdinaryVariable.DefaultSize = ordinaryVariable.DefaultSize;

                    // Remap Sum of BOEs IDs from old to new
                    if (ordinaryVariable.ValueType == VarValueType.SumOfBOEs)
                    {
                        //boeSum = -1;
                        foreach (var sumOfBOEs in ordinaryVariable.SelectedBOEsToSum)
                        {
                            int idVal = -1;
                            if (sumOfBOEs.BoeID.HasValue)
                            {
                                if (boeIDMapping.TryGetValue(sumOfBOEs.BoeID.Value, out idVal))
                                {
                                    SelectBOEsToSum newBoeToSum = new SelectBOEsToSum();
                                    newBoeToSum.BoeID = idVal;
                                    newOrdinaryVariable.SelectedBOEsToSum.Add(newBoeToSum);
                                }
                            }
                            else if (sumOfBOEs.WBSID.HasValue)
                            {
                                if (wbsIDMapping.TryGetValue(sumOfBOEs.WBSID.Value, out idVal))
                                {
                                    SelectBOEsToSum newBoeToSum = new SelectBOEsToSum();
                                    newBoeToSum.WBSID = idVal;
                                    newOrdinaryVariable.SelectedBOEsToSum.Add(newBoeToSum);
                                }
                            }
                            else if (sumOfBOEs.CLINID.HasValue)
                            {
                                if (clinIDMapping.TryGetValue(sumOfBOEs.CLINID.Value, out idVal))
                                {
                                    SelectBOEsToSum newBoeToSum = new SelectBOEsToSum();
                                    newBoeToSum.CLINID = idVal;
                                    newOrdinaryVariable.SelectedBOEsToSum.Add(newBoeToSum);
                                }
                            }
                        }
                    }
                    newOrdinaryVariable.Updateable = UpdateType.Upsert;
                    newTaskElement.OrdinaryVariables.Add(newOrdinaryVariable);
                }
                taskElement.OrdinaryVariables = newTaskElement.OrdinaryVariables;

                if (taskElement.WorkspaceVariableIDs.Any())
                {
                    Collection<int> newVariableIDs = new Collection<int>();

                    foreach (var variableID in taskElement.WorkspaceVariableIDs)
                    {
                        newVariableIDs.Add(variableIDMapping[variableID]);
                        taskElement.MOQHoursEquation = taskElement.MOQHoursEquation.Replace(
                                                    String.Format(
                                                        Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
                                                        variableID),
                                                    String.Format(
                                                        Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
                                                        variableIDMapping[variableID]));
                    }

                    taskElement.WorkspaceVariableIDs = newVariableIDs;
                }

                // if there are custom cross refs, let's copy them
                if (taskElement.CustomFieldValueContainers.Any())
                {
                    foreach (CustomFieldValueContainer taskElementCustomFieldXRefToCopy in taskElement.CustomFieldValueContainers)
                    {
                        taskElementCustomFieldXRefToCopy.Id = newItemID--;
                        taskElementCustomFieldXRefToCopy.ContainerID = taskElementCustomFieldXRefToCopy.Id;
                        taskElementCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                        taskElementCustomFieldXRefToCopy.CustomFieldValueID = customFieldValueIDMapping[taskElementCustomFieldXRefToCopy.CustomFieldValueID];
                        taskElementCustomFieldXRefToCopy.CustomFieldID = customFieldIDMapping[taskElementCustomFieldXRefToCopy.CustomFieldID];
                    }
                }

                if (copyLaborSpreads)
                {
                    foreach (var laborType in taskElement.taskElementLabors)
                    {

                        laborType.Id = newItemID--;
                        laborType.Updateable = UpdateType.Upsert;
                        laborType.BoeID = newBoeID;

                        // if the task does not have a performing org or a resource, we will still copy the WS, but we will want to tell the user to double check the new WS..
                        // because the data will be incomplete/incorrect
                        if (laborType.ResourceID.HasValue) { laborType.ResourceID = Resources[laborType.ResourceID.Value]; } else { finishedCorrectly = false; }
                        if (laborType.PerformingOrgID.HasValue) { laborType.PerformingOrgID = perfOrgs[laborType.PerformingOrgID.Value]; } else { finishedCorrectly = false; }

                        if (laborType.CustomFieldValueContainers.Any())
                        {
                            foreach (var laborTypeCustomFieldXRefToCopy in laborType.CustomFieldValueContainers)
                            {
                                laborTypeCustomFieldXRefToCopy.Id = newItemID--;
                                laborTypeCustomFieldXRefToCopy.ContainerID = laborTypeCustomFieldXRefToCopy.Id;
                                laborTypeCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                                laborTypeCustomFieldXRefToCopy.CustomFieldValueID = customFieldValueIDMapping[laborTypeCustomFieldXRefToCopy.CustomFieldValueID];
                                laborTypeCustomFieldXRefToCopy.CustomFieldID = customFieldIDMapping[laborTypeCustomFieldXRefToCopy.CustomFieldID];
                            }
                        }
                        if(laborType.CLINID.HasValue)
                        {
                            laborType.CLINID = clinIDMapping[laborType.CLINID.Value];
                        }
                        if (laborType.WBSID.HasValue)
                        {
                            laborType.WBSID = wbsIDMapping[laborType.WBSID.Value];
                        }
                        // copy spreads
                        foreach (var laborSpread in laborType.LaborSpreads)
                        {
                            laborSpread.BoeID = newBoeID;
                            laborSpread.Id = newItemID--;
                            laborSpread.Updateable = UpdateType.Upsert;
                        }                            
                    }

                    // This needs to be done to preserve the original order of the labors.
                    taskElement.taskElementLabors.Reverse();
                }

                taskElementsToSave.Add(taskElement);
            }

            if (taskElementsToSave.Any())
            {
                this._BoeTaskElementLoader.BulkSave(taskElementsToSave);
            }

            return finishedCorrectly;
        }

        /// <summary>
        /// Copies the Travel task 
        /// </summary>
        /// <param name="newWorkspace">New Workspace that is being copied too</param>
        /// <param name="boeIDMapping">boe's that are being copied</param>
        /// <param name="customFieldIDMapping">Workspace custom field id mappings</param>
        /// <param name="customFieldValueIDMapping">Workspace customfield value id mappings </param>
        /// <param name="copyLaborSpreads"></param>
        /// <returns>Boolean indicating whether there was any bad data that the user should be notified about</returns>
        private bool CopyTravelTasks(WorkspaceDTO newWorkspace, Dictionary<int, int> boeIDMapping, Dictionary<int, int> customFieldIDMapping, Dictionary<int, int> customFieldValueIDMapping, bool copyLaborSpreads, 
            Dictionary<int, int> clinIdMapping, Dictionary<int, int> wbsIdMapping, Dictionary<int, int> perfOrgs, Dictionary<int, int> resources)
        {
            bool finishedCorrectly = true;

            // only copy Zone Travel for RMS
            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
            {
                // copy default fees & escalation rates into the WS (zone travel data)
                this.zoneTravelRatesLoader.CopySystemDefaultFees(newWorkspace.Id);
                this.zoneTravelRatesLoader.CopySystemDefaultRates(newWorkspace.Id);

                int newItemID = -1;
                
                foreach (var travelElement in this.copiedFromTravelElements)
                {
                    int newBoeID = boeIDMapping[travelElement.BoeID];

                    travelElement.Id = newItemID--;
                    travelElement.BoeID = newBoeID;
                    travelElement.Updateable = UpdateType.Upsert;

                    // if there are custom cross refs, let's copy them
                    if (travelElement.CustomFieldValueContainers.Any())
                    {
                        foreach (var travelElementCustomFieldXRefToCopy in travelElement.CustomFieldValueContainers)
                        {
                            travelElementCustomFieldXRefToCopy.ContainerID = newItemID--;
                            travelElementCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                            travelElementCustomFieldXRefToCopy.CustomFieldValueID = customFieldValueIDMapping[travelElementCustomFieldXRefToCopy.CustomFieldValueID];
                            travelElementCustomFieldXRefToCopy.CustomFieldID = customFieldIDMapping[travelElementCustomFieldXRefToCopy.CustomFieldID];
                        }
                    }

                    if (copyLaborSpreads)
                    {
                        #region RMS - Zone Travel Trips

                        foreach (MSTTravelTripType zoneTrip in travelElement.MSTTravelTrips)
                        {
                            foreach (CustomFieldValueContainer mstTravelTripxrefToCopy in zoneTrip.CustomFieldValueContainers)
                            {
                                mstTravelTripxrefToCopy.ContainerID = newItemID--;
                                mstTravelTripxrefToCopy.Updateable = UpdateType.Upsert;
                                mstTravelTripxrefToCopy.CustomFieldValueID = customFieldValueIDMapping[mstTravelTripxrefToCopy.CustomFieldValueID];
                                mstTravelTripxrefToCopy.CustomFieldID = customFieldIDMapping[mstTravelTripxrefToCopy.CustomFieldID];
                            }

                            zoneTrip.Id = newItemID--;
                            zoneTrip.BoeID = newBoeID;
                            zoneTrip.Updateable = UpdateType.Upsert;
                            zoneTrip.PerfOrgID = perfOrgs[zoneTrip.PerfOrgID];
                            if (zoneTrip.ClinId.HasValue) { zoneTrip.ClinId = clinIdMapping[zoneTrip.ClinId.Value]; }
                            if (zoneTrip.WbsId.HasValue) { zoneTrip.WbsId = wbsIdMapping[zoneTrip.WbsId.Value]; }
                            if (zoneTrip.NonZoneResourceID.HasValue) { zoneTrip.NonZoneResourceID = resources[zoneTrip.NonZoneResourceID.Value]; }
                        }

                        #endregion
                    }

                    this._TravelLoader.SaveTravels(new Collection<TravelDTO> { travelElement });
                }
            }

            return finishedCorrectly;
        }
    }
}