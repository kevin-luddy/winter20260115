// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace CopyWorkspace
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

    public class DatabaseWorkspaceCopier
    {
        private IUserDTODataLoader _UserDTODataLoader = null;
        private IWorkspaceVariableDTODataLoader _WorkspaceVariableLoader = null;
        private IPermissionsDTODataLoader _PermissionsLoader = null;
        private IResourceDTODataLoader _ResourceDTODataLoader = null;
        private ICustomFieldDTODataLoader _CustomFieldDTODataLoader = null;
        private ICustomFieldValueDTODataLoader _CustomFieldValueDTODataLoader = null;
        private IBoeTaskElementDTODataLoader _BoeTaskElementLoader = null;
        private ITravelDTODataLoader _TravelLoader = null;
        private IBoeMediator _BoeMediator = null;
        private IFullObjectFactory factory;
        private IClinDTODataLoader clinLoader;
        private IWorkspaceDTODataLoader workspaceLoader;
        private IWbsDTODataLoader wbsLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private IDictionary<int, int> mappedUserIds;
        private ICollection<UserDTO> users;

        private IBOEFormIBOEDTODataLoader iboeDataLoader;
        private IBOEFormPBOEDTODataLoader pboeDataLoader;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesLoader;

        List<BoeTaskElementDTO> copiedFromTaskElements = new List<BoeTaskElementDTO>();
        List<TravelDTO> copiedFromTravelElements = new List<TravelDTO>();

        public DatabaseWorkspaceCopier(
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
            IUserDTODataLoader userDTODataLoader)
        {
            this._UserDTODataLoader = userDTODataLoader;
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
        }

        public int MapUserId(int userId)
        {
            // this handles cases where the userId is not set (default int is 0 in .NET)
            if (userId == 0)
            {
                return 0;
            }

            int destinationUserId;
            if (!this.mappedUserIds.TryGetValue(userId, out destinationUserId))
            {
                // User does not exist in the other Database, add it
                UserDTO user = users.First(u => u.UserID == userId);
                Console.WriteLine(string.Format("User with NTId {0} does not exist in the destination Database, adding user.", user.NTID));
                UserDTO destinationUser = new UserDTO
                {
                    DisplayName = user.DisplayName,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MiddleName = user.MiddleName,
                    NTID = user.NTID,
                    PhoneNumber = user.PhoneNumber,
                    UserID = -1,
                    UpdateDate = DateTime.Now,
                    IsUsPerson = user.IsUsPerson,
                    IsSubcontractor = user.IsSubcontractor
                };

                destinationUser = this._UserDTODataLoader.SaveUser(destinationUser);
                destinationUserId = destinationUser.UserID;
                this.mappedUserIds.Add(user.UserID, destinationUserId);
            }

            return destinationUserId;
        }

        public bool CopyWorkspace(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace, Collection<FullBoe> BOEToCopy,
            bool copyPermissions, bool copyTasks, bool copyLaborSpreads, ICollection<PermissionsDTO> boePotentialPermissionsWorkspaceToCopy, IDictionary<int, ICollection<BoeApproverResponseDTO>> approvers,
            ICollection<ResourceDTO> resourcesToCopy, ICollection<CustomFieldValueDTO> customFieldValuesToCopy, ICollection<BoeTaskElementDTO> tasks, ICollection<TravelDTO> travelElements,
            ICollection<PerformingOrgDTO> performingOrgsToCopy, ICollection<BOEFormIBOEDTO> iboes, ICollection<BOEFormPBOEDTO> pboes, IDictionary<int, int> mappedUserIds, ICollection<UserDTO> users)
        {
            if (workspaceToCopy == null)
            {
                throw new ArgumentNullException(nameof(workspaceToCopy));
            }

            this.users = users;
            this.mappedUserIds = mappedUserIds;

            this.copiedFromTaskElements.AddRange(tasks);
            if (travelElements.Any() && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
            {
                this.copiedFromTravelElements.AddRange(travelElements);
            }

            bool finishedCorrectly = true;

            this.CopyWorkspaceProperties(workspaceToCopy, newWorkspace);
            this.AddAuthorPermissionForCostLead(newWorkspace);
            Console.WriteLine("Copying CLIN");
            Dictionary<int, int> ClinIDMapping = this.CopyAllCLIN(workspaceToCopy, newWorkspace);

            // Get copy of custom fields for use with CopyCustomFieldValues before any changes are made to them
            IReadOnlyCollection<CustomFieldDTO> customFieldsToCopy = workspaceToCopy.CustomFields.DeepClone();

            Console.WriteLine("Copying WBS");
            Dictionary<int, int> WBSIDMapping = this.CopyAllWBS(workspaceToCopy, newWorkspace, ClinIDMapping);
            Console.WriteLine("Copying Custom Fields");
            Dictionary<int, int> CustomFieldIDMapping = this.CopyCustomFields(workspaceToCopy, newWorkspace);
            Dictionary<int, int> CustomFieldValueIDMapping = this.CopyCustomFieldValues(CustomFieldIDMapping, customFieldsToCopy, workspaceToCopy, BOEToCopy.Select(b => b.Id).ToList(), copyTasks, copyLaborSpreads, customFieldValuesToCopy);

            Console.WriteLine("Copying BOEs");
            Dictionary<int, int> BOEIDMapping = this.CopyBOEs(newWorkspace, BOEToCopy, ClinIDMapping, WBSIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping);
            // If the user specified to copy workspace permissions, we'll do that
            if (copyPermissions)
            {
                Console.WriteLine("Copying Permissions");
                this.CopyPotentialPermissions(workspaceToCopy, newWorkspace, boePotentialPermissionsWorkspaceToCopy, approvers, BOEIDMapping);
            }

            Dictionary<int, int> VariableIDMapping = this.CopyWorkspaceVariables(workspaceToCopy, newWorkspace, BOEIDMapping, WBSIDMapping, ClinIDMapping);
            this.CopyINLForms(workspaceToCopy, newWorkspace, iboes, pboes);

            Dictionary<int, int> performingOrgMapping = new Dictionary<int, int>();
            Dictionary<int, int> resourceMapping = new Dictionary<int, int>();
            if (copyLaborSpreads)
            {
                Console.WriteLine("Copying Perf Orgs");
                performingOrgMapping = this.CopyPerformingOrganizations(newWorkspace, performingOrgsToCopy);
                Console.WriteLine("Copying Resources");
                resourceMapping = this.CopyResources(newWorkspace, resourcesToCopy);
            }

            if (copyTasks)
            {
                Console.WriteLine("Copying Tasks");
                finishedCorrectly = this.CopyTasks(BOEIDMapping, WBSIDMapping, ClinIDMapping, VariableIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping, copyLaborSpreads, resourceMapping, performingOrgMapping) && finishedCorrectly;
                Console.WriteLine("Copying Travel");
                finishedCorrectly = this.CopyTravelTasks(newWorkspace, BOEIDMapping, CustomFieldIDMapping, CustomFieldValueIDMapping, copyLaborSpreads, ClinIDMapping, WBSIDMapping, performingOrgMapping, resourceMapping) && finishedCorrectly;
            }

            return finishedCorrectly;
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
            ICollection<int> boeToCopy, bool copyTasks, bool copyLaborSpreads, ICollection<CustomFieldValueDTO> customFieldValuesToCopy)
        {
            // Create a collection to map old custom field value IDs to new copied ones
            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            // If there are custom fields to copy, let's do it
            if (customFieldIDMapping.Any())
            {
                // Get all custom field values in the source workspace.
                customFieldValuesToCopy = customFieldValuesToCopy.Where(c => customFieldIDMapping.ContainsKey(c.CustomFieldID)).ToList();
                    //this._CustomFieldValueDTODataLoader.GetCustomFieldValueDTOsByCustomFieldIds(customFieldIDMapping
                    //    .Select(i => i.Key).ToCollection<int>());

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
                        currentCustomFieldValuesToCopy = this.GetOpenEndedCustomFieldValues(workspaceToCopy, customFieldValuesToCopy,
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
        private ICollection<CustomFieldValueDTO> GetOpenEndedCustomFieldValues(FullWorkspace workspaceToCopy,
            ICollection<CustomFieldValueDTO> allCustomFieldValues, CustomFieldDTO customField,
            ICollection<FullBoe> boesToCopy, bool copyTasks, bool copyLaborSpreads)
        {
            ICollection<CustomFieldValueDTO> toReturn = new Collection<CustomFieldValueDTO>();
            ICollection<int> boeIds = boesToCopy.Select(b => b.Id).ToList();

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
                            ICollection<BoeTaskElementDTO> tasks = workspaceToCopy.TaskElements.Where(t => boeIds.Contains(t.BoeID)).ToCollection();

                            foreach (BoeTaskElementDTO task in tasks)
                            {
                                toReturn.AddRange(allCustomFieldValues.Where(x =>
                                    task.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                        .Contains(x.CustomFieldValueID)));
                            }
                            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                            {
                                ICollection<TravelDTO> travelTasks = workspaceToCopy.Travels.Where(t => boeIds.Contains(t.BoeID)).ToCollection();

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
                            ICollection<BoeTaskElementDTO> tasks = workspaceToCopy.TaskElements.Where(t => boeIds.Contains(t.BoeID)).ToCollection();
                            ICollection<ResourceTypeDto> resourceTypes =
                                tasks.SelectMany(x => x.taskElementLabors).ToCollection();

                            foreach (ResourceTypeDto resourceType in resourceTypes)
                            {
                                toReturn.AddRange(allCustomFieldValues.Where(x =>
                                    resourceType.CustomFieldValueContainers.Where(c => c.CustomFieldID == customField.Id).Select(y => y.CustomFieldValueID)
                                        .Contains(x.CustomFieldValueID)));
                            }

                            ICollection<TravelDTO> travelTasks = workspaceToCopy.Travels.Where(t => boeIds.Contains(t.BoeID)).ToCollection();
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
        private Dictionary<int, int> CopyPerformingOrganizations(WorkspaceDTO newWorkspace, ICollection<PerformingOrgDTO> performingOrgsToCopy)
        {
			// Create a collection to map old Performing Organization IDs to new copied ones
			Dictionary<int, int> toReturn = new Dictionary<int, int>();

			// Create a list of Performing Organizations to save
			Collection<PerformingOrgDTO> performingOrganizationsToSave = new Collection<PerformingOrgDTO>();

			// Get a list of all in-use Perf Org IDs
			IEnumerable<int> taskOrganizationIDs = (from t in this.copiedFromTaskElements
                                        from l in t.taskElementLabors
                                        where l.PerformingOrgID.HasValue
                                        select l.PerformingOrgID.Value).Distinct();

			IEnumerable<int> ZoneTravelPerfOrgIDs = (from t in this.copiedFromTravelElements
                                        from l in t.MSTTravelTrips
                                        select l.PerfOrgID).Distinct();

			IEnumerable<int> performingOrganizationIDs = taskOrganizationIDs.Union(ZoneTravelPerfOrgIDs);

			// Create a mapping between old Perf Org IDs and new DTOs for the copied workspace
			Dictionary<int, PerformingOrgDTO> performingOrganizationIDMapping = new Dictionary<int, PerformingOrgDTO>();
            int newItemID = -1;
            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(this.perfOrgLoader.GetByIds(performingOrganizationIDs.Distinct().ToList()));

            foreach (int performingOrganizationID in performingOrganizationIDs)
            {
				PerformingOrgDTO performingOrganizationToCopy = perfOrgsFromDb.FirstOrDefault(x => x.Id == performingOrganizationID);

                // If there is a Performing Organization to copy, let's do it
                if (performingOrganizationToCopy != null)
                {
					// Get the corresponding Performing Organization from the copied workspace
					PerformingOrgDTO newWorkspacePerformingOrganization = this.perfOrgLoader.GetByListIdAndName(newWorkspace.PerfOrgListID, performingOrganizationToCopy.PerformingOrgName);

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
        private Dictionary<int, int> CopyResources(WorkspaceDTO newWorkspace, ICollection<ResourceDTO> resourcesToCopy)
        {
			// Create a collection to map old resource IDs to new copied ones
			Dictionary<int, int> toReturn = new Dictionary<int, int>();

			// Create a list of resources to save
			Collection<ResourceDTO> resourcesToSave = new Collection<ResourceDTO>();

			IEnumerable<int> resourceIDs = resourcesToCopy.Select(r => r.Id);

			// Create a mapping between old Resource IDs and new DTOs for the copied workspace
			Dictionary<int, ResourceDTO> resourceIDMapping = new Dictionary<int, ResourceDTO>();

            int newItemID = -1;

            ICollection<ResourceDTO> resourcesForWsListId = this._ResourceDTODataLoader.GetByListId(newWorkspace.ResourceListID);
            
            foreach (int resourceID in resourceIDs)
            {
				ResourceDTO resourceToCopy = resourcesToCopy.FirstOrDefault(x => x.Id == resourceID);

                // If there are resources to copy, let's do it
                if (resourceToCopy != null)
                {
					// Get the corresponding resource from the copied workspace
					ResourceDTO newWorkspaceResource = resourcesForWsListId.FirstOrDefault(x => x.ResourceName == resourceToCopy.ResourceName);

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
			Dictionary<int, int> toReturn = new Dictionary<int, int>();
            bool bfound;

			// Get the variables to copy
			IReadOnlyCollection<WorkspaceVariableDTO> currentWorkspaceVariables = workspaceToCopy.WorkspaceVariables;
			Collection<WorkspaceVariableDTO> workspaceVariablesToCopy = new Collection<WorkspaceVariableDTO>();

            if (currentWorkspaceVariables.Any())
            {
                int newItemID = -1;
                int oldItemID;

                // Iterate through each existing variables and set values to create a copy of it in the new workspace
                foreach (WorkspaceVariableDTO workspaceVariable in currentWorkspaceVariables)
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

                    oldItemID = workspaceVariable.Id;
                    newVariable.Id = newItemID--;
                    bfound = false;
                    // Remap Sum of BOEs IDs from old to new
                    if (workspaceVariable.ValueType == VarValueType.SumOfBOEs)
                    {
                        foreach (SelectBOEsToSum sumOfBOEs in workspaceVariable.SelectedBOEsToSum)
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
                        foreach (BoeTaskElementDTO taskElement in this.copiedFromTaskElements)
                        {
                            if (taskElement.WorkspaceVariableIDs.Any())
                            {
                                foreach (int variableID in taskElement.WorkspaceVariableIDs)
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

                // WorkspaceCopier change for current workspace Variables
                // Get a mapping between old and new IDs for the values
                toReturn = (from o in currentWorkspaceVariables
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
            int currentUserID = workspaceToCopy.CreatedByUserID;
            this.workspaceLoader.SaveWorkspaceSettings(currentUserID, newWorkspace);
        }

        private Dictionary<int, int> CopyAllWBS(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace, Dictionary<int, int> ClinIDMapping)
        {
			// Create a collection to map old WBS Numbers to new copied ones
			Dictionary<int, int> toReturn = new Dictionary<int, int>();

			// Get the list of all WBS from the workspace to copy
			List<WbsDTO> wbsToCopy = workspaceToCopy.WbsElements.ToList<WbsDTO>();

            // If there are WBSs, let's copy them
            if (wbsToCopy.Any())
            {
                int newItemID = -1;
                ICollection<FullWbs> newWbsElementsToSave = new Collection<FullWbs>();
                // Iterate through each existing WBS and set values to create a copy of
                // it in the new workspace
                foreach (WbsDTO wbs in wbsToCopy)
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
						Collection<int> newCLINIDs = new Collection<int>();

                        // Iterate over old CLIN IDs and add the IDs of their copy in the new workspace
                        foreach (int clin in wbs.ClinIDs)
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
			Dictionary<int, int> toReturn = new Dictionary<int, int>();

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
			PermissionsDTO authorPermission = new PermissionsDTO();
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
        private void CopyPotentialPermissions(FullWorkspace workspaceToCopy, WorkspaceDTO newWorkspace, ICollection<PermissionsDTO> boePotentialPermissionsWorkspaceToCopy, IDictionary<int, ICollection<BoeApproverResponseDTO>> approvers, Dictionary<int, int> BOEIDMapping)
        {
			// Create one list to hold the FROM permissions and one to hold the TO permissions
			// Set the FROM permissions to the workspace permissions
			IReadOnlyCollection<PermissionsDTO> workspaceToCopyPermissions = workspaceToCopy.WorkspacePermissions; // WorkspaceCopier change for Workspace permissions
			Collection<PermissionsDTO> newWorkspacePermissions = this._PermissionsLoader.GetWorkspacePermissions(newWorkspace.Id);
			Collection<PermissionsDTO> permissionsToSave = new Collection<PermissionsDTO>();

            if (workspaceToCopyPermissions.Any())
            {
                int newItemID = -1;

                // Set each existing workspace permission to reference the new workspace and reset the
                // PKID to indicate a new permission
                foreach (PermissionsDTO workspacePermission in workspaceToCopyPermissions)
                {
                    // map the User ID
                    workspacePermission.ETIUserId = this.MapUserId(workspacePermission.ETIUserId);

                    bool permissionsExist = (from p in newWorkspacePermissions
                                                where p.ETIUserId == workspacePermission.ETIUserId &&
                                                p.Role == workspacePermission.Role
                                                select p).Count() > 0;

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
            newWorkspacePermissions = this._PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(newWorkspace.Id);

            // Set each existing BOE potential permission to reference the new workspace and reset the
            // PKID to indicate a new permission
            if (boePotentialPermissionsWorkspaceToCopy.Any())
            {
                int newItemID = -1;

                foreach (PermissionsDTO workspacePermission in boePotentialPermissionsWorkspaceToCopy)
                {
                    // map the User ID
                    workspacePermission.ETIUserId = this.MapUserId(workspacePermission.ETIUserId);

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

            if (approvers.Any())
            {
                int newItemID = -1;
                int userId = this._UserDTODataLoader.GetUserForActiveUser().UserID;
                List<BoeApproverResponseDTO> responsesToSave = new List<BoeApproverResponseDTO>();

                foreach (KeyValuePair<int, ICollection<BoeApproverResponseDTO>> kvp in approvers)
                {
                    if (BOEIDMapping.ContainsKey(kvp.Key))
                    {
                        int newBoeId = BOEIDMapping[kvp.Key];
                        foreach (BoeApproverResponseDTO response in kvp.Value)
                        {
                            response.BoeID = newBoeId;
                            response.Id = newItemID--;
                            responsesToSave.Add(response);
                            response.Updateable = UpdateType.Upsert;
                            response.CurrentUserETIUserID = userId;
                        }
                    }
                }

                new BoeApproverResponseDTODataLoader().Save(responsesToSave);
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
        private void CopyINLForms(FullWorkspace workspaceToCopy, FullWorkspace newWorkspace, ICollection<BOEFormIBOEDTO> iboes, ICollection<BOEFormPBOEDTO> pboes)
        {
            int newItemID = -1;

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
        private Dictionary<int, int> CopyBOEs(WorkspaceDTO newWorkspace, Collection<FullBoe> BOEToCopy,
            Dictionary<int, int> ClinIDMapping, Dictionary<int, int> WBSIDMapping, Dictionary<int, int> customFieldIDMapping, Dictionary<int, int> customFieldValueIDMapping)
        {
			Dictionary<int, int> toReturn = new Dictionary<int, int>();

            if (BOEToCopy.Any())
            {
                ICollection<BoeDTO> boesToSave = new Collection<BoeDTO>();
                Dictionary<int, int> BoeMap = new Dictionary<int, int>();
                int newItemID = -1;

                foreach (FullBoe boeToCopy in BOEToCopy)
                {
                    FullBoe boe = this.factory.CreateFullBoe(boeToCopy);
                    
                    boe.CopySourceBoeId = boeToCopy.Id;

                    boe.Id = newItemID--;
                    boe.WCBID = newItemID--;
                    boe.Updateable = UpdateType.Upsert;
                    boe.AuthorIDs = boeToCopy.AuthorIDs;
                    boe.SubcontractorAuthorIDs = boeToCopy.SubcontractorAuthorIDs;
                    boe.State = boeToCopy.State; // WorkspaceCopier changed from Unassigned
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
                    BoeMap.Add(boeToCopy.Id, boe.Id);
                }

                // Save the copied BOEs
                FullWorkspace fullWorkspace = this.factory.CreateFullWorkspace(newWorkspace);
                IDictionary<int, int> boeIDDictionary = this._BoeMediator.MediatedSaveBOEs(fullWorkspace, boesToSave);

                //clean up before using boeIDMapping
                int counter = 0;

                foreach (int boeid in BOEToCopy.Select(b => b.Id))
                {
                    // get the dictionary's key value given the counter index so we can pull from boeIDDictionary correctly
                    int key = boeIDDictionary.Keys.ElementAt(counter);
                    toReturn.Add(boeid, boeIDDictionary[key]);
                    counter++;
                }

                // Set all BOEs to not Upsert again. Allows future saves of BOE sub-objects.
                foreach (BoeDTO boe in boesToSave)
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
            int idVal = -1;

            foreach (BoeTaskElementDTO taskElement in this.copiedFromTaskElements)
            {
                BoeTaskElementDTO newTaskElement = new BoeTaskElementDTO();

                int newBoeID = boeIDMapping[taskElement.BoeID];

                taskElement.Id = newItemID--;
                taskElement.BoeID = newBoeID;
                taskElement.Updateable = UpdateType.Upsert;

                foreach (OrdinaryVariableDto ordinaryVariable in taskElement.OrdinaryVariables)
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
                        foreach (SelectBOEsToSum sumOfBOEs in ordinaryVariable.SelectedBOEsToSum)
                        {
                            idVal = -1;
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

                    foreach (int variableID in taskElement.WorkspaceVariableIDs)
                    {
                        newVariableIDs.Add(variableIDMapping[variableID]);
                        taskElement.MOQHoursEquation = taskElement.MOQHoursEquation.Replace(
                                                    String.Format(
                                                        GenBOE.ActionLogic.Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
                                                        variableID),
                                                    String.Format(
                                                        GenBOE.ActionLogic.Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
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
                    foreach (ResourceTypeDto laborType in taskElement.taskElementLabors)
                    {

                        laborType.Id = newItemID--;
                        laborType.Updateable = UpdateType.Upsert;
                        laborType.BoeID = newBoeID;

                        // if the task does not have a performing org or a resource, we will still copy the WS, but we will want to tell the user to double check the new WS..
                        // because the data will be incomplete/incorrect
                        if (laborType.ResourceID.HasValue) {
                            if (Resources.ContainsKey(laborType.ResourceID.Value))
                            {
                                laborType.ResourceID = Resources[laborType.ResourceID.Value];
                            }
                            else
                            {
                                finishedCorrectly = false;
                                Console.WriteLine("Could not find Resource with ID " + laborType.ResourceID.Value + " for boeId " + newBoeID.ToString() + " labor type id" + laborType.Id);
                            }
                        }
                        else
                        {
                            finishedCorrectly = false;
                        }

                        if (laborType.PerformingOrgID.HasValue)
                        {
                            if (perfOrgs.ContainsKey(laborType.PerformingOrgID.Value))
                            {
                                laborType.PerformingOrgID = perfOrgs[laborType.PerformingOrgID.Value];
                            }
                            else
                            {
                                finishedCorrectly = false;
                                Console.WriteLine("Could not find Perf Org with ID " + laborType.PerformingOrgID.Value + " for boeId " + newBoeID.ToString() + " labor type id" + laborType.Id);
                            }
                        }
                        else
                        {
                            finishedCorrectly = false;
                        }

                        if (laborType.CustomFieldValueContainers.Any())
                        {
                            foreach (CustomFieldValueContainer laborTypeCustomFieldXRefToCopy in laborType.CustomFieldValueContainers)
                            {
                                laborTypeCustomFieldXRefToCopy.Id = newItemID--;
                                laborTypeCustomFieldXRefToCopy.ContainerID = laborTypeCustomFieldXRefToCopy.Id;
                                laborTypeCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                                laborTypeCustomFieldXRefToCopy.CustomFieldValueID = customFieldValueIDMapping[laborTypeCustomFieldXRefToCopy.CustomFieldValueID];
                                laborTypeCustomFieldXRefToCopy.CustomFieldID = customFieldIDMapping[laborTypeCustomFieldXRefToCopy.CustomFieldID];
                            }
                        }
                        if (laborType.CLINID.HasValue)
                        {
                            laborType.CLINID = clinIDMapping[laborType.CLINID.Value];
                        }
                        if (laborType.WBSID.HasValue)
                        {
                            laborType.WBSID = wbsIDMapping[laborType.WBSID.Value];
                        }
                        // copy spreads
                        foreach (ResourceSpreadDto laborSpread in laborType.LaborSpreads)
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

                foreach (TravelDTO travelElement in this.copiedFromTravelElements)
                {
                    int newBoeID = boeIDMapping[travelElement.BoeID];

                    travelElement.Id = newItemID--;
                    travelElement.BoeID = newBoeID;
                    travelElement.Updateable = UpdateType.Upsert;

                    // if there are custom cross refs, let's copy them
                    if (travelElement.CustomFieldValueContainers.Any())
                    {
                        foreach (CustomFieldValueContainer travelElementCustomFieldXRefToCopy in travelElement.CustomFieldValueContainers)
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
