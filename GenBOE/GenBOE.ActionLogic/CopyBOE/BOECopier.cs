// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;

    public class BOECopier
    {
        private IBoeDTODataLoader boeLoader;
        private IClinDTODataLoader clinLoader;
        private IWbsDTODataLoader wbsLoader;
        private ICustomFieldValueDTODataLoader _ICustomFieldValueDTODataLoader;
        private IResourceDTODataLoader _IResourceDTODataLoader;
        private ITravelDTODataLoader _ITravelDTODataLoader;
        private IBoeTaskElementMediator _IBoeTaskElementMediator;
        private IBoeMediator _IBoeMediator;
        private IFullObjectFactory factory;
        private IBOECopierCompany _boeCopierCompany;
        private VariableCircularReferenceChecker _VariableCircularReferenceChecker;
        private IVariableSelectBOEtoSumCalculation _VariableSelectBOEtoSumCalculation;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private IWorkspaceVariableDTODataLoader _workspaceVariableLoader;
        private IBoeTaskElementRecalculation _boeTaskElementRecalculation;

        /// <summary>
        /// RTE Template Data Loader
        /// </summary>
        private IRteTemplateDataLoader rteTemplateDataLoader;

        public BOECopier(
            IBoeDTODataLoader boeLoader,
            ICustomFieldValueDTODataLoader inICustomFieldValueDTODataLoader,
            IResourceDTODataLoader inIResourceDTODataLoader,
            ITravelDTODataLoader inITravelDTODataLoader,
            IBoeTaskElementMediator inIBoeTaskElementMediator,
            IBoeMediator inIBoeMediator,
            VariableCircularReferenceChecker inVariableCircularReferenceChecker,
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            IFullObjectFactory factory,
            IBOECopierCompany boeCopierCompany,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IWorkspaceVariableDTODataLoader workspaceVariableLoader,
            IBoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IClinDTODataLoader clinLoader,
            IWbsDTODataLoader wbsLoader,
            IRteTemplateDataLoader rteTemplateDataLoader)
        {
            this.boeLoader = boeLoader;
            this.clinLoader = clinLoader;
            this.wbsLoader = wbsLoader;
            this._IResourceDTODataLoader = inIResourceDTODataLoader;
            this._ICustomFieldValueDTODataLoader = inICustomFieldValueDTODataLoader;
            this._ITravelDTODataLoader = inITravelDTODataLoader;
            this._IBoeTaskElementMediator = inIBoeTaskElementMediator;
            this._IBoeMediator = inIBoeMediator;

            this._VariableCircularReferenceChecker = inVariableCircularReferenceChecker;
            this._VariableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
            this.factory = factory;
            this._boeCopierCompany = boeCopierCompany;
            this.perfOrgLoader = perfOrgLoader;
            this._workspaceVariableLoader = workspaceVariableLoader;
            this._boeTaskElementRecalculation = inBoeTaskElementRecalculation;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
        }

        /// <summary>
        /// Copies a given BOE to the given workspace
        /// </summary>
        /// <param name="inSourceBOEID">The ID of the BOE to copy</param>
        /// <param name="destinationBOEID">The ID of the BOE to copy the BOE to</param>
        /// <param name="taskElementsToCopy">List of task element Ids to copy from the source boe.</param>
        /// <param name="travelElementsToCopy">List of travel element Ids to copy from the source boe.</param>
        public void CopyBOE(
            FullWorkspace workspace,
            int inSourceBOEID,
            int? destinationBOEID,
            ICollection<int> taskElementsToCopy,
            ICollection<int> travelElementsToCopy)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            FullBoe sourceBoe = this.factory.CreateFullBoe(inSourceBOEID);
            FullBoe destinationBoe;

            if (!workspace.IsProjectMapWorkspace)
            {
                if (!destinationBOEID.HasValue)
                {
                    throw new ArgumentNullException(nameof(destinationBOEID));
                }

                destinationBoe = this.factory.CreateFullBoe(destinationBOEID.Value);
            }
            else
            {
                // check to see if copying into same workspace, if so then source BOE is destination BOE
                if (workspace.Id == sourceBoe.WorkspaceID)
                {
                    destinationBoe = sourceBoe;
                }
                else
                {
                    // We are creating a brand new BOE to copy into
                    destinationBoe = this.factory.CreateFullBoe();
                    destinationBoe.WorkspaceID = workspace.Id;
                    destinationBoe.State = BOEState.Draft;
                    destinationBoe.Workspace = workspace;
                }
            }

            // might as well preload the data..
            sourceBoe.LoadTravelRTEData();
            sourceBoe.LoadBOEsRTEData();
            sourceBoe.LoadTaskElementRTEData();

            this.CopyBOE(sourceBoe, destinationBoe, taskElementsToCopy, travelElementsToCopy);
        }

        /// <summary>
        /// Copies a given BOE to the given workspace
        /// </summary>
        /// <param name="inSourceBOE">The BOE to copy</param>
        /// <param name="inDestinationBOE">The BOE to copy the BOE to</param>
        /// <param name="taskElementsToCopy">Task element ids to copy from the source BOE.</param>
        private void CopyBOE(
            FullBoe inSourceBOE,
            FullBoe inDestinationBOE,
            ICollection<int> taskElementsToCopy,
            ICollection<int> travelElementsToCopy)
        {
            if (inSourceBOE == null)
            {
                throw new ArgumentNullException(nameof(inSourceBOE));
            }
            if (inDestinationBOE == null)
            {
                throw new ArgumentNullException(nameof(inDestinationBOE));
            }

            FullWorkspace SourceWorkspace = inSourceBOE.Workspace;
            FullWorkspace DestinationWorkspace = inDestinationBOE.Workspace;

            //Only copy custom fields when copying within the same workspace
            bool copyCustomFields = SourceWorkspace.Id == DestinationWorkspace.Id;

            this.CopyBOEHeader(
                inSourceBOE,
                inDestinationBOE);

            if (copyCustomFields)
            { 
                this.CopyBOECustomFieldCrossRefs(
                    inSourceBOE,
                    inDestinationBOE);
            }
            Dictionary<int, Tuple<int, decimal?>> VariableIDMapping = this.MapWorkspaceVariables(
                inSourceBOE,
                inDestinationBOE);

            var ResourceIDMapping = this.MapResources(
                inSourceBOE,
                DestinationWorkspace.ResourceListID);

            var PerformingOrgIDMapping = this.MapPerformingOrganizations(
                inSourceBOE,
                DestinationWorkspace);

            this.CopyTasks(
                inSourceBOE,
                inDestinationBOE,
                DestinationWorkspace,
                SourceWorkspace,
                VariableIDMapping,
                ResourceIDMapping,
                PerformingOrgIDMapping,
                taskElementsToCopy,
                copyCustomFields);

            // Travel is shown for MultiCLIN in RMS
            if (!inDestinationBOE.IsMultiClinWbs || SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
            {
                this.CopyTravelTasks(
                    inSourceBOE,
                    inDestinationBOE,
                    PerformingOrgIDMapping,
                    travelElementsToCopy,
                    copyCustomFields);
            }
        }



        /// <summary>
        /// Duplicates labor tasks within a BOE.
        /// </summary>
        /// <param name="duplicateRequest">Dictionary of labor task IDs and the number of duplicates requested for the task.</param>
        /// <param name="inSourceBOE">BOE that contains the task to be duplicated</param>
        /// <param name="ws">Workspace of the BOE</param>
        public void DuplicateLaborTaskElements(Dictionary<int, int> duplicateRequest, FullBoe inSourceBOE, FullWorkspace ws)
        {
            if (inSourceBOE == null) { throw new ArgumentNullException(nameof(inSourceBOE)); }
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }
            if (duplicateRequest == null) { throw new ArgumentNullException(nameof(duplicateRequest)); }

            foreach (KeyValuePair<int,int> task in duplicateRequest)
            {
                ICollection<int> inUseMetricIDs = this._boeCopierCompany.GetMetricsUsedByTaskElement(task.Key); //dictionary key is the task id

                ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(inSourceBOE.WorkspaceID, inSourceBOE.Id, task.Key);

                for (int i = 1; i <= task.Value; i++)//dictionary value is number of times to duplicate task
                {
                    // Create a new duplicate for each iteration so things like Open Ended Custom Field Values are unique
                    BoeTaskElementDTO taskDuplicate = this.GetDuplicateTask(inSourceBOE, task.Key);
                    int taskDuplicateId = -1;

                    //Save the specified number of duplicates for the task
                    if (taskDuplicate != null)
                    {
                        taskDuplicateId = this.SaveDuplicateTask(ws, taskDuplicate, inUseMetricIDs, i);
                    }

                    if (rteTemplateAnswers.Any() && taskDuplicateId > 0)
                    {
                        ICollection<RTECustomTemplateQuestionAnswerModelView> answerDuplicates = this.GetDuplicateRteTemplateAnswers(rteTemplateAnswers, taskDuplicateId); ;

                        if (answerDuplicates.Any())
                        {
                            this.rteTemplateDataLoader.SaveAnswers(answerDuplicates);
                        }
                    }
                }   
            }
        }
       

        /// <summary>
        /// Copies performing organizations from one workspace to another
        /// </summary>
        private Dictionary<int, int> MapPerformingOrganizations(
            FullBoe inSourceBOE,
            FullWorkspace inDestinationWorkspace)
        {
            // Create a collection to map old perf org IDs to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Get a list of all in-use Performing Ord IDs
            var perfOrgIDsUsedByLabor = from t in inSourceBOE.TaskElements
                                        from l in t.taskElementLabors
                                        where l.PerformingOrgID.HasValue
                                        select l.PerformingOrgID.Value;

            var perfOrgIDsUsedByTravel = from o in inSourceBOE.Travels
                                         from t in o.TravelTrips
                                         select t.PerfOrgID;
            // need org ids for mst travel added to perfOrgIDs for MST Travel
            var perfOrgIDsUsedByMSTTravel = from o in inSourceBOE.Travels
                                         from t in o.MSTTravelTrips
                                         select t.PerfOrgID;



            List<int> perfOrgIDs = perfOrgIDsUsedByLabor.Union(perfOrgIDsUsedByTravel).Union(perfOrgIDsUsedByMSTTravel).ToList();

            var perfOrgsToCopy = this.perfOrgLoader.GetByIds(perfOrgIDs);

            if (perfOrgsToCopy.Any())
            {
                // Get all performing orgs in the workspace
                var allCurrentPerformingOrganizations = inDestinationWorkspace.PerformingOrgsForWsList;

                foreach (var performingOrganization in perfOrgsToCopy)
                {
                    // Get the perf org to copy
                    var matchingPerformingOrg = (from r in allCurrentPerformingOrganizations
                                                 where r.PerformingOrgName.Equals(performingOrganization.PerformingOrgName, StringComparison.CurrentCultureIgnoreCase)
                                                 select r).FirstOrDefault();

                    // If a perf org exists with the same name, add the ID mapping to return
                    if (matchingPerformingOrg != null)
                    {
                        toReturn.Add(performingOrganization.Id, matchingPerformingOrg.Id);
                    }
                    else // Map perf orgs that don't exist in the new workspace to -1
                    {
                        toReturn.Add(performingOrganization.Id, -1);
                    }
                }
            }

            // Return the mapping of original perf org IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copies resources from one workspace to another
        /// </summary>
        /// <param name="workspaceToCopy"></param>
        /// <param name="inDestinationWorkspaceResourceListID"> destination workspace's resource list ID</param>
        private Dictionary<int, int> MapResources(
            FullBoe inSourceBOE,
            int inDestinationWorkspaceResourceListID)
        {
            // Create a collection to map old resource IDs to new copied ones
            var toReturn = new Dictionary<int, int>();

            // Get a list of all in-use Resource IDs
            var resourceIDs = from t in inSourceBOE.TaskElements
                                         from l in t.taskElementLabors
                                         where l.ResourceID.HasValue
                                         select l.ResourceID.Value;

            var resourcesToMap = this._IResourceDTODataLoader.GetByIds(resourceIDs.ToList());

            if (resourcesToMap.Any())
            {
                // Get all resources in the workspace
                var allCurrentResources = this._IResourceDTODataLoader.GetByListId(inDestinationWorkspaceResourceListID);

                foreach (var resource in resourcesToMap)
                {
                    // Get the resource to copy
                    var matchingResource = (from r in allCurrentResources
                                            where r.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase)
                                            select r).FirstOrDefault();

                    // If a resource exists with the same name, add the ID mapping to return
                    if (matchingResource != null)
                    {
                        toReturn.Add(resource.Id, matchingResource.Id);
                    }
                    // Map resources that don't exist in the new workspace to -1
                    else
                    {
                        toReturn.Add(resource.Id, -1);
                    }
                }
            }

            // Return the mapping of original resource IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Copy workspace variables from one workspace to another
        /// </summary>
        private Dictionary<int, Tuple<int, decimal?>> MapWorkspaceVariables(
            FullBoe inSourceBOE,
            FullBoe inDestinationBOE)
        {
            // Create a collection to map old variable IDs to new copied ones
            var toReturn = new Dictionary<int, Tuple<int, decimal?>>();

            // Get the variables to copy
            ICollection<WorkspaceVariableDTO> workspaceVariablesToCopy = this._workspaceVariableLoader.GetByIds(inSourceBOE.TaskElements.SelectMany(v => v.WorkspaceVariableIDs).ToCollection<int>());

            if (workspaceVariablesToCopy.Any())
            {
                // Get all variables in the current workspace
                toReturn = this.GetWorkspaceVariableIDMapping(workspaceVariablesToCopy, inDestinationBOE.WorkspaceVariables);
            }

            // Return the mapping of original variable IDs to their copies
            return toReturn;
        }

        /// <summary>
        /// Creates mapping of workspace variable Ids that need to be copied to the destination workspace.  If the variable
        /// does not exist in the destination workspace, sets up the mapping so the variable can be created.
        /// </summary>
        /// <param name="workspaceVariablesToCopy">Workspace variables from the source workspace.</param>
        /// <param name="currentWorkspaceVariables">Workspace variables in the destination workspace.</param>
        /// <returns>Workspace Id mappings.</returns>
        private Dictionary<int, Tuple<int, decimal?>> GetWorkspaceVariableIDMapping(ICollection<WorkspaceVariableDTO> workspaceVariablesToCopy, IReadOnlyCollection<WorkspaceVariableDTO> currentWorkspaceVariables)
        {
            Dictionary<int, Tuple<int, decimal?>> toReturn = new Dictionary<int, Tuple<int, decimal?>>();

            if (workspaceVariablesToCopy.Any())
            {
                // Iterate through each existing variables and set values to create a copy of it in the new workspace
                foreach (var workspaceVariable in workspaceVariablesToCopy)
                {
                    var matchingWorkspaceVariable = (from w in currentWorkspaceVariables
                                                     where w.WorkspaceVariableName.Equals(workspaceVariable.WorkspaceVariableName, StringComparison.CurrentCultureIgnoreCase)
                                                     select w).FirstOrDefault();

                    if (matchingWorkspaceVariable == null)
                    {
                        toReturn.Add(workspaceVariable.Id, new Tuple<int, decimal?>(-1, null));
                    }
                    else  // matching workspace variable (name) already exists
                    {
                        // only add the workspace variable key if it hasn't been added before
                        if (!toReturn.ContainsKey(workspaceVariable.Id))
                        {
                            // compare the before/after workspace variable values - if they are different, then include the new value in the results
                            decimal? updatedValue = (workspaceVariable.WorkspaceVariableValue == matchingWorkspaceVariable.WorkspaceVariableValue) ? (decimal?)null : matchingWorkspaceVariable.WorkspaceVariableValue;

                            toReturn.Add(workspaceVariable.Id, new Tuple<int, decimal?>(matchingWorkspaceVariable.Id, updatedValue));
                        }
                    }
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Copies BOE custom field selections from one BOE to another
        /// </summary>
        private void CopyBOECustomFieldCrossRefs(
            BoeDTO inSourceBOE,
            FullBoe inDestinationBOE)
        {
            if (inSourceBOE == null)
            {
                throw new ArgumentNullException(nameof(inSourceBOE));
            }
            if (inDestinationBOE == null)
            {
                throw new ArgumentNullException(nameof(inDestinationBOE));
            }

            if (inSourceBOE.Id == inDestinationBOE.Id)
            {
                return;  // bug #32242 - Do not copy custom field mappings for BOEs that are being copied to themselves
            }
            else if (inSourceBOE.CustomFieldValueContainers.Any())  // if there are custom cross refs, let's copy them
            {
                // Get a fresh copy of the BOE, since we'll be saving it and need the UpdateDate to be fresh
                inDestinationBOE = this.factory.CreateFullBoe(this.boeLoader.GetById(inDestinationBOE.Id));
                
                // set the new copy to be updateable
                inDestinationBOE.Updateable = UpdateType.Upsert;

                List<CustomFieldValueContainer> destinationBOECustomFieldContainers = new List<CustomFieldValueContainer>();

                destinationBOECustomFieldContainers.AddRange(inDestinationBOE.CustomFieldValueContainers);
                ICollection<CustomFieldValueDTO> sourceCustomFieldValueDtos = this._ICustomFieldValueDTODataLoader.GetByIds(inSourceBOE.CustomFieldValueContainers.Select(i => i.CustomFieldValueID).ToCollection<int>());

                ICollection<CustomFieldValueDTO> allExistingValuesForCustomFields = this._ICustomFieldValueDTODataLoader.GetByIds(destinationBOECustomFieldContainers.Select(i => i.CustomFieldValueID).ToCollection<int>());
                int NewCustomFieldContainerID = -1;
                foreach (var boeCustomFieldXRefToCopy in inSourceBOE.CustomFieldValueContainers)
                {
                    int fieldID = sourceCustomFieldValueDtos.First(i => i.CustomFieldValueID == boeCustomFieldXRefToCopy.CustomFieldValueID).CustomFieldID;

                    // If there is not already a value selected for this custom field on this BOE, we'll add this one
                    if (!allExistingValuesForCustomFields.Any(i => i.CustomFieldID == fieldID))
                    {
                        boeCustomFieldXRefToCopy.ContainerID = NewCustomFieldContainerID--;
                        boeCustomFieldXRefToCopy.Updateable = UpdateType.Upsert;
                        if (boeCustomFieldXRefToCopy.IsOpenEnded)
                        {
                            boeCustomFieldXRefToCopy.CustomFieldValueID = this.CreateDuplicateOpenEndedCustomField(boeCustomFieldXRefToCopy.CustomFieldValueID);
                        }
                        else
                        {
                            boeCustomFieldXRefToCopy.CustomFieldValueID = boeCustomFieldXRefToCopy.CustomFieldValueID;
                        }

                        destinationBOECustomFieldContainers.Add(boeCustomFieldXRefToCopy);
                    }
                }

                inDestinationBOE.CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(destinationBOECustomFieldContainers.ToArray());

                this._IBoeMediator.MediatedSave(this.factory.CreateFullWorkspace(inDestinationBOE.WorkspaceID), inDestinationBOE);
            }
        }

        /// <summary>
        /// Copies BOE Title, Description and Sources of Data if and only if all three fields are empty in the 
        /// destination BOE.
        /// </summary>
        /// <param name="inSourceBOE">Source copy BOE.</param>
        /// <param name="inDestinationBOE">Destination copy BOE.</param>
        private void CopyBOEHeader(
            FullBoe inSourceBOE,
            FullBoe inDestinationBOE)
        {
            if (inSourceBOE == null)
            {
                throw new ArgumentNullException(nameof(inSourceBOE));
            }
            if (inDestinationBOE == null)
            {
                throw new ArgumentNullException(nameof(inDestinationBOE));
            }

            // Get a fresh copy of the BOE since we'll be saving it and need the UpdateDate to be fresh
            // unless this is project map in which case this BOE is brand new (not saved yet) if it's copying from a different workspace
            if (!inDestinationBOE.Workspace.IsProjectMapWorkspace || inSourceBOE.WorkspaceID == inDestinationBOE.WorkspaceID)
            {
                inDestinationBOE = this.factory.CreateFullBoe(this.boeLoader.GetById(inDestinationBOE.Id));
            }
            else
            {
                inDestinationBOE.CamName = inSourceBOE.CamName;
                inDestinationBOE.Category = inSourceBOE.Category;
                inDestinationBOE.ClassOfCost = inSourceBOE.ClassOfCost;
                inDestinationBOE.SOW = inSourceBOE.SOW;
                inDestinationBOE.SOWTitle = inSourceBOE.SOWTitle;
                inDestinationBOE.StartDate = inSourceBOE.StartDate;
                inDestinationBOE.EndDate = inSourceBOE.EndDate;

                // Need to copy over the CLIN and WBS (if applicable) for the new BOE
                inDestinationBOE.CLINID = this.CopyCLIN(inSourceBOE, inDestinationBOE.Workspace);
                inDestinationBOE.WBSID = this.CopyWBS(inSourceBOE, inDestinationBOE.Workspace);
            }


            inDestinationBOE.CopySourceBoeId = inSourceBOE.Id;
            inDestinationBOE.Updateable = UpdateType.Upsert;

            // Only update if all 3 fields in the destination BOE are empty.
            if (string.IsNullOrEmpty(inDestinationBOE.Title) && string.IsNullOrEmpty(inDestinationBOE.Description) && string.IsNullOrEmpty(inDestinationBOE.DataSource))
            {
                inDestinationBOE.Title = inSourceBOE.Title;
                inDestinationBOE.Description = inSourceBOE.Description;
                inDestinationBOE.DataSource = inSourceBOE.DataSource;
            }

            // Save the updated BOE
            IDictionary<int, int> newIds = this._IBoeMediator.MediatedSave(this.factory.CreateFullWorkspace(inDestinationBOE.WorkspaceID), inDestinationBOE);
            inDestinationBOE.Id = newIds[inDestinationBOE.Id];
        }

        /// <summary>
        /// Copies the WBS.
        /// </summary>
        /// <param name="inSourceBOE">The in source boe.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>Id of a new WBS or a matching WBS.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private int? CopyWBS(FullBoe inSourceBOE, FullWorkspace workspace)
        {
            int? wbsId = null;

            if (inSourceBOE.Wbs != null)
            {
                FullWbs wbs = workspace.WbsElements.FirstOrDefault(c => c.WbsString == inSourceBOE.Wbs.WbsString);
                if (wbs == null)
                {
                    // need to create it
                    WbsDTO newCLIN = new WbsDTO
                    {
                        WbsNumber = inSourceBOE.Wbs.WbsNumber,
                        WbsPaddedNumber = inSourceBOE.Wbs.WbsPaddedNumber,
                        WbsTitle = inSourceBOE.Wbs.WbsTitle,
                        Updateable = UpdateType.Upsert,
                        WorkspaceID = workspace.Id
                    };

                    wbsId = this.wbsLoader.Save(newCLIN);
                }
                else
                {
                    // found a matching wbs
                    wbsId = wbs.Id;
                }
            }

            return wbsId;
        }

        /// <summary>
        /// Copies the clin.
        /// </summary>
        /// <param name="inSourceBOE">The in source boe.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>Id of a new CLIN or a matching CLIN.</returns>
        private int? CopyCLIN(FullBoe inSourceBOE, FullWorkspace workspace)
        {
            int? clinId = null;

            if (inSourceBOE.Clin != null)
            {
                FullClin clin = workspace.Clins.FirstOrDefault(c => c.ClinString == inSourceBOE.Clin.ClinString);
                if (clin == null)
                {
                    // need to create it
                    ClinDTO newCLIN = new ClinDTO
                    {
                        ClinNumber = inSourceBOE.Clin.ClinNumber,
                        ClinPaddedNumber = inSourceBOE.Clin.ClinPaddedNumber,
                        ClinTitle = inSourceBOE.Clin.ClinTitle,
                        Updateable = UpdateType.Upsert,
                        WorkspaceID = workspace.Id
                    };

                    clinId = this.clinLoader.Save(newCLIN);
                }
                else
                {
                    // found a matching clin
                    clinId = clin.Id;
                }
            }

            return clinId;
        }



        /// <summary>
        /// Copies task elements from one BOE to another.
        /// </summary>
        /// <param name="inSourceBOE">Source BOE.</param>
        /// <param name="inDestinationBOE">Destination BOE.</param>
        /// <param name="inDestinationWorkspace">Destination workspace DTO.</param>
        /// <param name="inVariableIDMapping">Mapping of workspace variable Ids.</param>
        /// <param name="inResourceIDMapping">Mapping of resource Ids.</param>
        /// <param name="inPerformingOrgIDMapping">Mapping of performing Org Ids.</param>
        /// <param name="taskElementsToCopy">Task element Ids to be copied.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void CopyTasks(
            FullBoe inSourceBOE,
            FullBoe inDestinationBOE,
            FullWorkspace inDestinationWorkspace,
            FullWorkspace inSourceWorkspace,
            Dictionary<int, Tuple<int, decimal?>> inVariableIDMapping,
            Dictionary<int, int> inResourceIDMapping,
            Dictionary<int, int> inPerformingOrgIDMapping,
            ICollection<int> taskElementsToCopy,
            bool copyCustomFields)
        {
            // Only consider individual task elements to be copied.
            Collection<BoeTaskElementDTO> tasksToCopy = inSourceBOE.TaskElements.Where(id => taskElementsToCopy.Contains(id.Id)).ToCollection<BoeTaskElementDTO>();

            if (tasksToCopy.Any())
            {
                #region Determine if a recalculation is needed due to workspace variable value changes (between the source and destination BOEs)

                bool workspaceVariableValuesChanged = inVariableIDMapping.Where(v => v.Value.Item2.HasValue).Select(v => v.Value.Item1).Any();

                #endregion
                bool costPrecisionChanged = (inDestinationBOE.WorkspaceID != inSourceBOE.WorkspaceID) && (inDestinationWorkspace.CostDecimalPrecision != inSourceBOE.Workspace.CostDecimalPrecision);
                bool hoursPrecisionChanged = (inDestinationBOE.WorkspaceID != inSourceBOE.WorkspaceID) && (inDestinationWorkspace.ResourceDecimalPrecision != inSourceBOE.Workspace.ResourceDecimalPrecision);
                int NewTaskElementID = -1;
                foreach (BoeTaskElementDTO taskElementOrig in tasksToCopy)
                {
                    BoeTaskElementDTO taskElementCopy = taskElementOrig.DeepClone();
                    ICollection<int> inUseMetricIDs = this._boeCopierCompany.GetMetricsUsedByTaskElement(taskElementCopy.Id);

                    // Create new Task Element for Project Map if copying from another workspace
                    if (!inDestinationWorkspace.IsProjectMapWorkspace || inDestinationWorkspace.Id != inSourceWorkspace.Id)
                    {
                        taskElementCopy.Id = NewTaskElementID;
                    }

                    taskElementCopy.BoeID = inDestinationBOE.Id;
                    taskElementCopy.Updateable = UpdateType.Upsert;

                    //Do not copy the task ID if the ID is already used by any task in the BOE
                    if (taskElementCopy.BOETaskID != null && this.isTaskIdUsedInBoe(inDestinationBOE, taskElementCopy.BOETaskID))
                    {
                        taskElementCopy.BOETaskID = String.Empty;
                    }

                    if (!inDestinationWorkspace.IsProjectMapWorkspace)
                    {
                        if (!this.CopyTaskElementMoqEquation(taskElementCopy, inSourceBOE, inDestinationBOE, inVariableIDMapping))
                        {
                            // The MOQ equation could not be copied, because a circular reference was detected attempting to resolve equation variable.
                            // In this case, skip copying the taskelement altogether.
                            continue;
                        }

                        //recalculate labor when coming from a different ws that may have a different precision
                        if (inDestinationBOE.WorkspaceID != inSourceBOE.WorkspaceID)
                        {
                            this._boeTaskElementRecalculation.RecalculateLaborWithTaskElement(taskElementCopy, VariableType.Task, inDestinationWorkspace, null, costPrecisionChanged, hoursPrecisionChanged);
                        }

                        // if there are custom cross refs, let's copy them
                        if (copyCustomFields && taskElementCopy.CustomFieldValueContainers.Any())
                        {
                            taskElementCopy.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(taskElementCopy.CustomFieldValueContainers);
                        }
                    }

                    int NewLaborTypeID = -1;
                    foreach (var laborType in taskElementCopy.taskElementLabors)
                    {
                        laborType.Id = NewLaborTypeID;
                        laborType.Updateable = UpdateType.Upsert;
                        laborType.BoeID = inDestinationBOE.Id;

                        if (inResourceIDMapping.ContainsKey(laborType.ResourceID.Value))
                        {
                            laborType.ResourceID = (laborType.ResourceID.HasValue && inResourceIDMapping[laborType.ResourceID.Value] != -1) ? inResourceIDMapping[laborType.ResourceID.Value] : (int?)null;
                        }

                        if (inPerformingOrgIDMapping.ContainsKey(laborType.PerformingOrgID.Value))
                        {
                            laborType.PerformingOrgID = (laborType.PerformingOrgID.HasValue && inPerformingOrgIDMapping[laborType.PerformingOrgID.Value] != -1) ? inPerformingOrgIDMapping[laborType.PerformingOrgID.Value] : (int?)null;
                        }

                        if ((inSourceWorkspace.Id != inDestinationWorkspace.Id) || (inSourceBOE.IsMultiClinWbs != inDestinationBOE.IsMultiClinWbs))
                        {
                            laborType.WBSID = null;
                            laborType.CLINID = null;
                        }

                        // PercentSpreadLocked and HourSpreadLocked remain as-is

                        if (!inDestinationWorkspace.IsProjectMapWorkspace)
                        {
                            if (copyCustomFields && laborType.CustomFieldValueContainers.Any())
                            {
                                laborType.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(laborType.CustomFieldValueContainers);
                            }

                            if (workspaceVariableValuesChanged)
                            {
                                // recalculate spreads
                                this._boeTaskElementRecalculation.RecalculateLaborWithTaskElement(
                                    taskElementCopy,
                                    VariableType.Workspace,
                                    inDestinationWorkspace,
                                    tasksToCopy);
                            }
                        }

                        // copy spreads
                        int NewLaborSpreadID = -1;
                        foreach (var laborSpread in laborType.LaborSpreads)
                        {
                            laborSpread.BoeID = inDestinationBOE.Id;
                            laborSpread.Id = NewLaborSpreadID--;
                            laborSpread.Updateable = UpdateType.Upsert;
                        }

                        NewLaborTypeID--;

                    }  // end foreach laborType

                    this._IBoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO> { taskElementCopy }, inDestinationWorkspace);

                    if (taskElementCopy.Id > 0 && inUseMetricIDs.Any())
                    {
                        this._boeCopierCompany.SaveMetricsToTaskElement(taskElementCopy.Id, inUseMetricIDs);
                    }

                    NewTaskElementID--;

                }  // end foreach taskElement
            }
        }

        /// <summary>
        /// Returns a duplicate of a labor task that can be saved.
        /// </summary>
        /// <param name="inSourceBOE">Full BOE</param>
        /// <param name="taskElementToCopy">ID of task to be copied</param>
        /// <returns>Copy of labor task DTO that can be saved</returns>
        private BoeTaskElementDTO GetDuplicateTask(
            FullBoe inSourceBOE,
            int taskElementToCopy)
        {
            BoeTaskElementDTO taskToCopy = inSourceBOE.TaskElements.FirstOrDefault(x => x.Id == taskElementToCopy);
            BoeTaskElementDTO duplicateTaskElement = null;
            if (taskToCopy != null)
            {
                duplicateTaskElement = taskToCopy.DeepClone();

                int resCfId = -1;

                duplicateTaskElement.Id = -1;
                duplicateTaskElement.Updateable = UpdateType.Upsert;
                duplicateTaskElement.BOETaskID = string.Empty;

                duplicateTaskElement.BOETaskElementOrder = 2000; //New tasks should be put at bottom of order

                //Create new copies of the Task (ordinary) Variables. WS Variables do not need special handling
                if (taskToCopy.OrdinaryVariables.Any())
                {
                    List<OrdinaryVariableDto> taskOrdinaryVariables = new List<OrdinaryVariableDto>();
                    int NewTaskVariableID = -1;
                    foreach (OrdinaryVariableDto ordinaryVariable in taskToCopy.OrdinaryVariables)
                    {
                        ordinaryVariable.Id = NewTaskVariableID;
                        ordinaryVariable.Updateable = UpdateType.Upsert;

                        taskOrdinaryVariables.Add(ordinaryVariable);
                        NewTaskVariableID--;
                    }
                    duplicateTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto>(taskOrdinaryVariables.ToArray());
                }

                // Copy Task Level custom cross refs
                if (duplicateTaskElement.CustomFieldValueContainers.Any())
                {
                    duplicateTaskElement.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(duplicateTaskElement.CustomFieldValueContainers);
                }

                //Create new copies of Resource Types
                int NewLaborTypeID = -1;
                foreach (ResourceTypeDto laborType in duplicateTaskElement.taskElementLabors)
                {
                    laborType.Id = NewLaborTypeID;
                    laborType.Updateable = UpdateType.Upsert;


                    // Copy Resource Level custom cross refs
                    if (laborType.CustomFieldValueContainers.Any())
                    {
                        laborType.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(laborType.CustomFieldValueContainers, resCfId);
                        resCfId -= laborType.CustomFieldValueContainers.Count;
                    }

                    //Create new copies of the labor spreads
                    int NewLaborSpreadID = -1;
                    foreach (ResourceSpreadDto laborSpread in laborType.LaborSpreads)
                    {
                        laborSpread.Id = NewLaborSpreadID--;
                        laborSpread.Updateable = UpdateType.Upsert;
                    }

                    NewLaborTypeID--;

                }  // end foreach laborType       
            }
            return duplicateTaskElement;
        }

        /// <summary>
        /// Saves a copy of a task. Copy first makes a Deep Clone of the task to be copied, so the original task can be
        /// saved multiple times (for duplicate task feature). The copied task will include the duplicate
        /// number in its new task title.
        /// </summary>
        /// <param name="inDestinationWorkspace">Current workspace</param>
        /// <param name="taskElement">Task element to save. Task will be cloned, so it can be saved multiple times.</param>
        /// <param name="inUseMetricIDs">Metric IDs used by task element being saved.</param>
        /// <param name="duplicateNumber">Current duplicate number of the task being saved. This will be included in the new task title.</param>
        /// <returns>ID of duplicate task</returns>
        private int SaveDuplicateTask(FullWorkspace inDestinationWorkspace, BoeTaskElementDTO taskElement, ICollection<int> inUseMetricIDs, int duplicateNumber)
        {
            Collection<BoeTaskElementDTO> tasksToSave = new Collection<BoeTaskElementDTO>();

            BoeTaskElementDTO taskToSave = taskElement.DeepClone();

            //Update the task title with the duplicate number
            taskToSave.TaskTitle = Utilities.appendCopyPrefixToTitle(taskElement.TaskTitle, duplicateNumber);
            tasksToSave.Add(taskToSave);

            //Save Tasks
            IDictionary<int, int> savedTasks = this._IBoeTaskElementMediator.MediatedBulkSaveTaskElements(tasksToSave, inDestinationWorkspace);

            //Save Metrics if they are used by task
            if (inUseMetricIDs.Any() && savedTasks.Any())
            {
                foreach (int taskId in savedTasks.Values)
                {
                    this._boeCopierCompany.SaveMetricsToTaskElement(taskId, inUseMetricIDs);
                }
            }

            return savedTasks.First().Value;
        }

        /// <summary>
        /// Creates a copy of a new custom field container
        /// </summary>
        /// <param name="CustomFieldValueContainers">Custom field container to be copied</param>
        /// <param name="newId">Starting id to use for new custom field containers. Defaults to -1, but may need to be specified when using bulk save</param>
        /// <returns>Custom field container</returns>
        private Collection<CustomFieldValueContainer> GetCustomFieldValueContainersCopy(Collection<CustomFieldValueContainer> CustomFieldValueContainers, int newId = -1)
        {
            Collection<CustomFieldValueContainer> destinationCustomFieldValueContainers = new Collection<CustomFieldValueContainer>();

            foreach (CustomFieldValueContainer customFieldXRefToCopy in CustomFieldValueContainers)
            {
                customFieldXRefToCopy.Id = newId;
                customFieldXRefToCopy.ContainerID = newId;
                customFieldXRefToCopy.Updateable = UpdateType.Upsert;

                if (customFieldXRefToCopy.IsOpenEnded)
                {
                    // need to create the duplicate open-ended custom field
                    customFieldXRefToCopy.CustomFieldValueID = this.CreateDuplicateOpenEndedCustomField(customFieldXRefToCopy.CustomFieldValueID);
                }
                else
                {
                    customFieldXRefToCopy.CustomFieldValueID = customFieldXRefToCopy.CustomFieldValueID;
                }

                destinationCustomFieldValueContainers.Add(customFieldXRefToCopy);
                newId--;
            }
            return destinationCustomFieldValueContainers;
        }

        /// <summary>
        /// Creates the duplicate open ended custom field using the id passed in.
        /// </summary>
        /// <param name="customFieldValueID">The open ended custom field value identifier.</param>
        /// <returns>The new id for the duplicate open ended custom field</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private int CreateDuplicateOpenEndedCustomField(int customFieldValueID)
        {
            CustomFieldValueDTO openEnded = this._ICustomFieldValueDTODataLoader.GetById(customFieldValueID);
            openEnded.Id = -1;
            openEnded.CustomFieldValueID = -1;
            openEnded.Updateable = UpdateType.Upsert;

            int? result = this._ICustomFieldValueDTODataLoader.Save(openEnded);

            return result.Value;
        }

        /// <summary>
        /// Checks to see if a task ID is already used by an existing labor/travel task in a BOE.
        /// </summary>
        /// <param name="inDestinationBOE">BOE to search for Task ID in</param>
        /// <param name="taskId">Task ID</param>
        /// <returns>True if the task ID is already used by another task in the BOE.</returns>
        private bool isTaskIdUsedInBoe(FullBoe inDestinationBOE, String taskId)
        {
            bool taskIdExists = false;

            var matchingTaskID = (from t in inDestinationBOE.TaskElements
                                  where t.BOETaskID != null &&
                                      t.BOETaskID.Equals(taskId, StringComparison.CurrentCulture)
                                  select t).Any();

            var matchingTaskIDForTravel = (from t in inDestinationBOE.Travels
                                           where t.TaskID != null &&
                                                   t.TaskID.Equals(taskId, StringComparison.CurrentCulture)
                                           select t).Any();

            if (matchingTaskID || matchingTaskIDForTravel)
            {
                taskIdExists = true;
            }
            return taskIdExists;
        }

        /// <summary>
        /// Updates a MOQ equation in a task element.  This includes copying and resolving conflicts of any necessary
        /// variables from the source equation. 
        /// </summary>
        /// <param name="taskElement">The source task element to copy the equation from.  A side effect of this method is that this task
        /// element will contain the new MOQ equation and necessary variables.</param>
        /// <param name="destinationBOEId">Destination BOE id</param>
        /// <returns></returns>
        public bool CopyTaskElementMoqEquation(BoeTaskElementDTO taskElement, int destinationBOEId)
        {
            if (taskElement == null) { throw new ArgumentNullException(nameof(taskElement)); }

            FullBoe sourceBoe = this.factory.CreateFullBoe(taskElement.BoeID);
            FullBoe destinationBoe = this.factory.CreateFullBoe(destinationBOEId);

            // Get all workspace variables that are used in the task element to be copied from.
            ICollection<WorkspaceVariableDTO> workspaceVariablesToCopy = sourceBoe.WorkspaceVariables.Where(i => taskElement.WorkspaceVariableIDs.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();

            Dictionary<int, Tuple<int, decimal?>> workspaceVariableIDMapping = this.GetWorkspaceVariableIDMapping(workspaceVariablesToCopy, destinationBoe.WorkspaceVariables);
            return this.CopyTaskElementMoqEquation(taskElement, sourceBoe, destinationBoe, workspaceVariableIDMapping);
        }

        /// <summary>
        /// Updates a MOQ equation in a task element.  This includes copying and resolving conflicts of any necessary
        /// variables from the source equation to be compatible with the destination BOE.
        /// </summary>
        /// <param name="inSourceTaskElement">The source task element to copy the equation from.  A side effect of this method is that this task
        /// element will contain the new MOQ equation and necessary variables.</param>
        /// <param name="inSourceBOE">Source BOE.</param>
        /// <param name="inDestinationBOE">Destination BOE.</param>
        /// <param name="inVariableIDMapping">Dictionary that maps destination variables to source variables.</param>
        /// <returns>True if the Moq Equation can be copied. False when the result of the copy causes a circular reference.</returns>
        private bool CopyTaskElementMoqEquation(BoeTaskElementDTO inSourceTaskElement, FullBoe inSourceBOE, FullBoe inDestinationBOE, Dictionary<int, Tuple<int, decimal?>> inVariableIDMapping)
        {
            bool moqEquationCanBeCopied = true;

            // Cache for circular references
            var cache = new VariableCircularReferenceCheckerCache();

            // Get all workspace variables that are used in this task, but don't have a matching
            // WS variable in this new workspace. We'll need to turn these into task variables.
            var workspaceVariablesToConvertToOrdinary = (from w in inSourceBOE.WorkspaceVariables.Where(i => inSourceTaskElement.WorkspaceVariableIDs.Contains(i.Id))
                                                        from m in inVariableIDMapping
                                                        where w.Id == m.Key &&
                                                              m.Value.Item1 == -1
                                                        select w).ToList();
            List<int> oldWorkspaceVariableIds = workspaceVariablesToConvertToOrdinary.Select(i => i.Id).ToList<int>();

            // Turn unmatched workspace variables into task variables
            if (workspaceVariablesToConvertToOrdinary.Any())
            {
                var workspaceVariableIDs = new List<int>();
                var ordinaryVariables = new List<OrdinaryVariableDto>();

                workspaceVariableIDs.AddRange(inSourceTaskElement.WorkspaceVariableIDs);
                ordinaryVariables.AddRange(inSourceTaskElement.OrdinaryVariables);

                int newOrdinaryVariableID = -1;
                foreach (var workspaceVariable in workspaceVariablesToConvertToOrdinary)
                {
                    var newOrdinaryVariable = new OrdinaryVariableDto();

                    newOrdinaryVariable.OrdinaryVariableName = workspaceVariable.WorkspaceVariableName;
                    newOrdinaryVariable.Id = newOrdinaryVariableID;
                    newOrdinaryVariable.Updateable = UpdateType.Upsert;

                    if (workspaceVariable.ValueType == VarValueType.SumOfBOEs)
                    {
                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVariable }, inSourceBOE.Workspace);

                        newOrdinaryVariable.OrdinaryVariableValue = this._VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVariable, data);
                    }
                    else
                    {
                        newOrdinaryVariable.OrdinaryVariableValue = workspaceVariable.WorkspaceVariableValue;
                    }

                    newOrdinaryVariable.IsPercentage = workspaceVariable.IsPercentage;

                    ordinaryVariables.Add(newOrdinaryVariable);
                    workspaceVariableIDs.Remove(workspaceVariable.Id);
                    newOrdinaryVariableID--;
                }

                inSourceTaskElement.WorkspaceVariableIDs = new Collection<int>(workspaceVariableIDs.ToArray());
                inSourceTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto>(ordinaryVariables.ToArray());
            }

            if (inSourceTaskElement.WorkspaceVariableIDs.Any())
            {
                Collection<int> newVariableIDs = new Collection<int>();

                foreach (var variableID in inSourceTaskElement.WorkspaceVariableIDs)
                {
                    if (inVariableIDMapping.ContainsKey(variableID))
                    {
                        newVariableIDs.Add(inVariableIDMapping[variableID].Item1);

                        inSourceTaskElement.MOQHoursEquation = inSourceTaskElement.MOQHoursEquation.Replace(
                                                    String.Format(
                                                        Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
                                                        variableID),
                                                    String.Format(
                                                        Common.MOQ.Parser.REGEX_WorkspaceVariableTagReplacement,
                                                        inVariableIDMapping[variableID].Item1));
                    }
                }

                inSourceTaskElement.WorkspaceVariableIDs = newVariableIDs;
            }

            // Untag and retag the workspace variables in the equation to tag any ordinary variables that were replaced
            // by exising workspace variables by the same name
            ICollection<WorkspaceVariableDTO> oldWorkspaceVariables = inSourceBOE.WorkspaceVariables.Where(i => oldWorkspaceVariableIds.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();

            var untaggedHoursEquation = Common.MOQ.Parser.UntagVariables(inSourceTaskElement.MOQHoursEquation, inDestinationBOE.WorkspaceVariables.Union(oldWorkspaceVariables).ToList());
            inSourceTaskElement.MOQHoursEquation = Common.MOQ.Parser.TagVariables(untaggedHoursEquation, inDestinationBOE.WorkspaceVariables.ToList());

            if (inSourceTaskElement.OrdinaryVariables.Any())
            {
                var taskOrdinaryVariables = new List<OrdinaryVariableDto>();

                var taskWorkspaceVariableIDs = new List<int>();
                taskWorkspaceVariableIDs.AddRange(inSourceTaskElement.WorkspaceVariableIDs);

                int NewTaskVariableID = -1;
                foreach (var ordinaryVariable in inSourceTaskElement.OrdinaryVariables)
                {
                    var matchingWorkspaceVariable = (from w in inDestinationBOE.WorkspaceVariables
                                                     where w.WorkspaceVariableName.Equals(ordinaryVariable.OrdinaryVariableName, StringComparison.CurrentCultureIgnoreCase)
                                                     select w).FirstOrDefault();

                    // Only add ordinary variables if there are no workspace variables using the same name
                    if (matchingWorkspaceVariable == null)
                    {
                        // Remap Sum of BOEs variables to just their value if the BOEs are in different workspaces
                        if (ordinaryVariable.ValueType == VarValueType.SumOfBOEs &&
                                (inSourceBOE.WorkspaceID != inDestinationBOE.WorkspaceID ||
                                    this._VariableCircularReferenceChecker.OrdinaryVariablesCreateCircularReference(cache, inDestinationBOE.Id, new Collection<OrdinaryVariableDto> { ordinaryVariable }, inDestinationBOE.Workspace).Any()))
                        {
                            ordinaryVariable.ValueType = VarValueType.Discrete;
                            ordinaryVariable.SelectedBOEsToSum = new Collection<SelectBOEsToSum>();

                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(new List<OrdinaryVariableDto>() { ordinaryVariable }, null, inSourceBOE.Workspace);

                            ordinaryVariable.OrdinaryVariableValue = this._VariableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(ordinaryVariable, data);
                        }

                        ordinaryVariable.Id = NewTaskVariableID;
                        ordinaryVariable.BoeID = inDestinationBOE.Id;

                        ordinaryVariable.Updateable = UpdateType.Upsert;

                        taskOrdinaryVariables.Add(ordinaryVariable);
                    }
                    // If there is an existing workspace variable using the same name as this ordinary variable, then
                    // we'll remove it from the task element and add the workspace variable ID to the collection of
                    // IDs being used by the task element
                    else
                    {
                        if (!taskWorkspaceVariableIDs.Contains(matchingWorkspaceVariable.Id))
                        {
                            taskWorkspaceVariableIDs.Add(matchingWorkspaceVariable.Id);
                        }
                    }

                    NewTaskVariableID--;
                }

                inSourceTaskElement.OrdinaryVariables = new Collection<OrdinaryVariableDto>(taskOrdinaryVariables.ToArray());
                inSourceTaskElement.WorkspaceVariableIDs = new Collection<int>(taskWorkspaceVariableIDs.ToArray());
            }

            // If any workspace variables create circular references we need to skip the copying
            // of this task all together
            ICollection<WorkspaceVariableDTO> sourceTaskElementWorkspaceVariables = oldWorkspaceVariables.Where(i => inSourceTaskElement.WorkspaceVariableIDs.Contains(i.Id)).ToCollection();
            if (this._VariableCircularReferenceChecker.WorkspaceVariablesCreateCircularReference(cache, inDestinationBOE.Id, sourceTaskElementWorkspaceVariables, inDestinationBOE.Workspace).Any())
            {
                moqEquationCanBeCopied = false;
            }
            return moqEquationCanBeCopied;
        }

        /// <summary>
        /// Copy all Travel Tasks, with trips, from one BOE to another
        /// </summary>
        /// <param name="inSourceBOE"></param>
        /// <param name="inDestinationBOE"></param>
        /// <param name="inPerformingOrgIDMapping"></param>
        /// <param name="travelElementsToCopy">Travel element Ids to be copied</param>
        private void CopyTravelTasks(FullBoe inSourceBOE, FullBoe inDestinationBOE, Dictionary<int, int> inPerformingOrgIDMapping, ICollection<int> travelElementsToCopy, bool copyCustomFields)
        {
            // Get all travel tasks in the BOE
            inSourceBOE.LoadTravelRTEData();
            ICollection<TravelDTO> travelTasksToCopy = inSourceBOE.Travels.Where(t => travelElementsToCopy.Contains(t.Id)).ToList();

            if (travelTasksToCopy.Any())
            {
                int NewTravelID = -1;
                foreach (TravelDTO travelTask in travelTasksToCopy)
                {
                    // Copy to this BOE
                    travelTask.BoeID = inDestinationBOE.Id;
                    travelTask.Id = NewTravelID;
                    travelTask.Updateable = UpdateType.Upsert;

                    //Do not copy the task ID if the ID is already used by any task in the BOE
                    if (travelTask.TaskID != null && this.isTaskIdUsedInBoe(inDestinationBOE, travelTask.TaskID))
                    {
                        travelTask.TaskID = string.Empty;
                    }

                    // if there are task level custom fields, copy them
                    if (copyCustomFields && travelTask.CustomFieldValueContainers.Any())
                    {
                        travelTask.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(travelTask.CustomFieldValueContainers);
                    }

                    // copy mst travel trips within the travel task (won't be any for non-mst)
                    if (travelTask.MSTTravelTrips.Any())
                    {
                        var mstTravelTripsToSave = new Collection<MSTTravelTripType>();
                        int NewTravelTripID = -1;
                        foreach (var mstTravelTrip in travelTask.MSTTravelTrips)
                        {
                            // If the workspace doesn't contain this trip's perf org, we won't
                            // copy this trip. A mapping of -1 means the performing org does not exist in the destination workspace.
                            if (inPerformingOrgIDMapping.ContainsKey(mstTravelTrip.PerfOrgID) && inPerformingOrgIDMapping[mstTravelTrip.PerfOrgID] != -1)
                            {
                                mstTravelTrip.PerfOrgID = inPerformingOrgIDMapping[mstTravelTrip.PerfOrgID];
                            }
                            else
                            {
                                // Skip trip
                                continue;
                            }
                            mstTravelTrip.Id = NewTravelTripID;
                            //Copy trip level custom fields if they exist
                            if (copyCustomFields && mstTravelTrip.CustomFieldValueContainers.Any())
                            {
                                mstTravelTrip.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(mstTravelTrip.CustomFieldValueContainers);
                            }
                            mstTravelTrip.Updateable = UpdateType.Upsert;
                            mstTravelTrip.BoeID = inDestinationBOE.Id;
                            mstTravelTripsToSave.Add(mstTravelTrip);
                            NewTravelTripID--;
                        }
                        travelTask.MSTTravelTrips = mstTravelTripsToSave;
                    }

                    // Copy all trips within the task (non-mst only, will be none for mst)
                    if (travelTask.TravelTrips.Any())
                    {
                        var travelTripsToSave = new Collection<TravelTripType>();

                        int NewTravelTripID = -1;
                        foreach (var travelTrip in travelTask.TravelTrips)
                        {
                            // If the workspace doesn't contain this trip's perf org, we won't
                            // copy this trip. A mapping of -1 means the performing org does not exist in the destination workspace.
                            if (inPerformingOrgIDMapping.ContainsKey(travelTrip.PerfOrgID) && inPerformingOrgIDMapping[travelTrip.PerfOrgID] != -1)
                            {
                                travelTrip.PerfOrgID = inPerformingOrgIDMapping[travelTrip.PerfOrgID];
                            }
                            else
                            {
                                // Skip trip
                                continue;
                            }

                            travelTrip.TravelTripID = NewTravelTripID;

                            // If the travel trip is locked, reset to the original Trip ID
                            if (travelTrip.LockedDate != null && travelTrip.OriginatingTripID != null)
                            {
                                travelTrip.SystemTripID = travelTrip.OriginatingTripID.Value;
                            }

                            //Copy trip level custom fields if they exist
                            if (copyCustomFields && travelTrip.CustomFieldValueContainers.Any())
                            {
                                travelTrip.CustomFieldValueContainers = this.GetCustomFieldValueContainersCopy(travelTrip.CustomFieldValueContainers);
                            }

                            // Reset locked fields
                            travelTrip.LockedDate = null;
                            travelTrip.OriginatingTripID = null;

                            travelTrip.Updateable = UpdateType.Upsert;
                            travelTrip.BoeID = inDestinationBOE.Id;

                            travelTripsToSave.Add(travelTrip);
                            NewTravelTripID--;
                        }

                        travelTask.TravelTrips = travelTripsToSave;
                    }
                    NewTravelID--;
                }

                this._ITravelDTODataLoader.SaveTravels(travelTasksToCopy);
            }
        }

        /// <summary>
        /// Get duplicates of the RTE Template Answers for the given task
        /// </summary>
        /// <param name="templateAnswers">Answers to duplicate</param>
        /// <param name="duplicateTaskId">ID of the duplicate task</param>
        /// <returns>Duplicate RTE Answers</returns>
        private ICollection<RTECustomTemplateQuestionAnswerModelView> GetDuplicateRteTemplateAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> templateAnswers, int duplicateTaskId)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> toReturn = new Collection<RTECustomTemplateQuestionAnswerModelView>();

            foreach(RTECustomTemplateQuestionAnswerModelView answer in templateAnswers)
            {
                RTECustomTemplateQuestionAnswerModelView duplicateAnswer = answer.DeepClone();
                duplicateAnswer.Id = -1;
                duplicateAnswer.TaskId = duplicateTaskId;
                duplicateAnswer.Updateable = UpdateType.Upsert;

                toReturn.Add(duplicateAnswer);
            }

            return toReturn;
        }
    }
}
