// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Used to carry data into the parser and Sum Of Boes calculation
    /// </summary>
    public class DataClassForSumOfBOEsCalculation
    {
        #region Dictionaries

        /// <summary>
        /// Wbs Id to BOE Ids mapping
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, Collection<int>> WbsIdToBoeIds { get; set; }

        /// <summary>
        /// Clin Id to BOE Ids mapping
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, List<int>> ClinIdToBoeIds { get; set; }

        /// <summary>
        /// Boe Id to Task Elements Mapping
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, List<BoeTaskElementDTO>> BoeIdToTaskElements { get; set; }

        /// <summary>
        /// Boe Id to Resources Mapping
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, ICollection<ResourceDTO>> BoeIdToResources { get; set; }

        #endregion
        
        #region Constructors

        /// <summary>
        /// Public C-tor to initialize the dictionaries
        /// </summary>
        public DataClassForSumOfBOEsCalculation()
        {
            this.WbsIdToBoeIds = new Dictionary<int, Collection<int>>();
            this.ClinIdToBoeIds = new Dictionary<int, List<int>>();
            this.BoeIdToTaskElements = new Dictionary<int, List<BoeTaskElementDTO>>();
            this.BoeIdToResources = new Dictionary<int, ICollection<ResourceDTO>>();
        }

        #endregion

        /// <summary>
        /// Fills the data based on the BOE Task Element
        /// </summary>
        /// <param name="ordinaryVariables">fill data for these ordinary variables if provided</param>
        /// <param name="workspaceVariables">fill data for these workspace variables if provided</param>
        /// <param name="wbsElements">The WBS elements.</param>
        /// <param name="boes">The boes.</param>
        /// <param name="taskElements">The task elements.</param>
        /// <param name="resourcesForWsResourceListId">The resources for ws resource list identifier.</param>
        /// <param name="clins">The clins.</param>
        /// <exception cref="System.ArgumentNullException">wbsElements or boes or taskElements</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void FillData(ICollection<OrdinaryVariableDto> ordinaryVariables, ICollection<WorkspaceVariableDTO> workspaceVariables, IReadOnlyCollection<WbsDTO> wbsElements, IReadOnlyCollection<BoeDTO> boes, IReadOnlyCollection<BoeTaskElementDTO> taskElements, IReadOnlyCollection<ResourceDTO> resourcesForWsResourceListId, IReadOnlyCollection<ClinDTO> clins)
        {
            if (ReferenceEquals(wbsElements, null))
            {
                throw new ArgumentNullException(nameof(wbsElements));
            }

            if (ReferenceEquals(boes, null))
            {
                throw new ArgumentNullException(nameof(boes));
            }

            if (ReferenceEquals(taskElements, null))
            {
                throw new ArgumentNullException(nameof(taskElements));
            }
            if (ordinaryVariables == null)
            {
                ordinaryVariables = new List<OrdinaryVariableDto>();
            }
            if (workspaceVariables == null)
            {
                workspaceVariables = new List<WorkspaceVariableDTO>();
            }

            ordinaryVariables = ordinaryVariables.Where(x => x != null).ToList();
            workspaceVariables = workspaceVariables.Where(x => x != null).ToList();

            // Get IDs that we'll be working with, if it's ordinary variables
            List<int> boeIds = ordinaryVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.BoeID.HasValue).Select(y => y.BoeID.Value)).ToList();
            boeIds.AddRange(workspaceVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.BoeID.HasValue).Select(y => y.BoeID.Value)).ToList());
            boeIds = boeIds.Distinct().ToList();

            List<int> wbsIds = ordinaryVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.WBSID.HasValue).Select(y => y.WBSID.Value)).ToList();
            wbsIds.AddRange(workspaceVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.WBSID.HasValue).Select(y => y.WBSID.Value)).ToList());
            wbsIds = wbsIds.Distinct().ToList();

            List<int> clinIds = ordinaryVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.CLINID.HasValue).Select(y => y.CLINID.Value)).ToList();
            clinIds.AddRange(workspaceVariables.SelectMany(x => x.SelectedBOEsToSum.Where(z => z.CLINID.HasValue).Select(y => y.CLINID.Value)).ToList());
            clinIds = clinIds.Distinct().ToList();

            #region Preload from Db, so that way we can run in parallel

            // Preload from Db, so that way we can run in parallel

            HashSet<WbsDTO> allWbsLoaded = wbsIds.Any() ? new HashSet<WbsDTO>((from w in wbsElements where new HashSet<int>(wbsIds).Contains(w.Id) select w).ToArray()) : new HashSet<WbsDTO>();
            Collection<BoeDTO> allBoesLoaded = boeIds.Any() ? boes.ToCollection() : new Collection<BoeDTO>();
            HashSet<ClinDTO> allClinsLoaded = clinIds.Any() ? new HashSet<ClinDTO>((from c in clins where new HashSet<int>(clinIds).Contains(c.Id) select c).ToArray()) : new HashSet<ClinDTO>();

            IDictionary<WbsDTO, Collection<BoeDTO>> allWbsBoesWithNestingLoaded = new Dictionary<WbsDTO, Collection<BoeDTO>>();
            foreach (FullWbs wbs in allWbsLoaded)
            {
                allWbsBoesWithNestingLoaded[wbs] = this.FindNestedBoes(wbs, boes, wbsElements).ToCollection();

                // make sure the WBS's child BOEs are included in "all BOEs"
                foreach (FullBoe wbsBoe in wbs.BoesWithNesting)
                {
                    if (!allBoesLoaded.Any(b => b.Id == wbsBoe.Id))
                    {
                        allBoesLoaded.Add(wbsBoe);
                    }
                }
            }

            IDictionary<int, Collection<BoeDTO>> allClinBoesLoaded = new Dictionary<int, Collection<BoeDTO>>();
            foreach (ClinDTO clin in allClinsLoaded)
            {
                if (!allClinBoesLoaded.ContainsKey(clin.Id))  // avoid duplication of CLINs
                {
                    Collection<BoeDTO> clinBoes = new Collection<BoeDTO>();

                    foreach (BoeDTO boe in boes.Where(b => b.CLINID == clin.Id))  // for each BOE belonging to this CLIN
                    {
                        clinBoes.Add(boe);  // add it to the CLIN's FULL BOE list

                        // make sure the CLIN's child BOEs are included in "all BOEs"
                        if (!allBoesLoaded.Any(b => b.Id == boe.Id))
                        {
                            allBoesLoaded.Add(boe);
                        }
                    }

                    allClinBoesLoaded[clin.Id] = clinBoes;
                }
            }

            #endregion

            // object to lock on when adjusting the 4 dictionaries
            object LOCK = new object();

            // Process all Wbs
            Parallel.ForEach(allWbsLoaded, new ParallelOptions { MaxDegreeOfParallelism = 5 }, wbs =>
            {
                Collection<int> boeIdsInWbs = new Collection<int>(allWbsBoesWithNestingLoaded[wbs].Select(x => x.Id).ToList());

                lock (LOCK)
                {
                    boeIds.AddRange(boeIdsInWbs);
                    if (!this.WbsIdToBoeIds.ContainsKey(wbs.Id))
                    {
                        this.WbsIdToBoeIds.Add(wbs.Id, boeIdsInWbs);
                    }
                }
            });

            // Process all Clins
            Parallel.ForEach(clinIds, new ParallelOptions { MaxDegreeOfParallelism = 5 }, clinId =>
            {
                ClinDTO clinObject = allClinsLoaded.FirstOrDefault(c => c.Id == clinId);
                if (clinObject != null)
                {
                    List<int> boeIdsInClin = allBoesLoaded.Where(x => x.CLINID == clinObject.Id).Select(x => x.Id).ToList();

                    lock (LOCK)
                    {
                        boeIds.AddRange(boeIdsInClin);
                        if (!this.ClinIdToBoeIds.ContainsKey(clinId))
                        {
                            this.ClinIdToBoeIds.Add(clinId, boeIdsInClin);
                        }
                    }
                }
            });

            // Clear out duplicates
            boeIds = boeIds.Distinct().ToList();

            // Now that we have a list of all BOEs needed, process each of them..
            if (boeIds.Any())
            {
                // Get data that will remain the same for all BOEs
                ICollection<BoeDTO> foundBoes = allBoesLoaded.Where(x => boeIds.Contains(x.Id)).ToCollection();
                HashSet<ResourceDTO> wsResources = new HashSet<ResourceDTO>(resourcesForWsResourceListId.ToCollection());
                HashSet<BoeTaskElementDTO> wsTaskElements = new HashSet<BoeTaskElementDTO>(taskElements.ToCollection());

                Parallel.ForEach(foundBoes, new ParallelOptions { MaxDegreeOfParallelism = 5 }, fullBoe =>
                {
                    List<BoeTaskElementDTO> boeElements = wsTaskElements.Where(t => t.BoeID == fullBoe.Id).ToList();
                    HashSet<int> resourceIds = new HashSet<int>(boeElements.SelectMany(a => a.taskElementLabors.Where(x => x.ResourceID.HasValue).Select(b => b.ResourceID.Value)).Distinct().ToCollection());
                    // add BRC resources to list - but don't duplicate tasks if it has both resource and brc
                    resourceIds.AddRange(new HashSet<int>(boeElements.SelectMany(a => a.taskElementLabors.Where(x => x.BusinessResourceCodeID.HasValue).Select(b => b.BusinessResourceCodeID.Value)).Distinct().ToCollection()));
                    ICollection<ResourceDTO> resources = wsResources.Where(r => resourceIds.Contains(r.Id)).ToCollection();

                    lock (LOCK)
                    {
                        this.BoeIdToTaskElements.Add(fullBoe.Id, boeElements);
                        this.BoeIdToResources.Add(fullBoe.Id, resources);
                    }
                });
            }
        }

        /// <summary>
        /// Fills the data.
        /// </summary>
        /// <param name="ordinaryVariables">The ordinary variables.</param>
        /// <param name="workspaceVariables">The workspace variables.</param>
        /// <param name="fullWorkspace">The full workspace.</param>
        public void FillData(ICollection<OrdinaryVariableDto> ordinaryVariables, ICollection<WorkspaceVariableDTO> workspaceVariables, FullWorkspace fullWorkspace)
        {
            if (ReferenceEquals(fullWorkspace, null))
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            this.FillData(ordinaryVariables, workspaceVariables, fullWorkspace.WbsElements, fullWorkspace.Boes, fullWorkspace.TaskElements, fullWorkspace.ResourcesForWsResourceListId, fullWorkspace.Clins);
        }

        public void UpdateBOETasks(int boeID, ICollection<BoeTaskElementDTO> inTaskElements, FullWorkspace workspace)
        {
            if (inTaskElements == null)
            {
                return;
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            List<BoeTaskElementDTO> newTaskElementList = new List<BoeTaskElementDTO>();
            ICollection<int> resourceIds = new Collection<int>();

            foreach (BoeTaskElementDTO orgTask in this.BoeIdToTaskElements[boeID])
            {
                bool bFound = false;
                Collection<int> processedLaborIds = new Collection<int>();
                Collection<ResourceTypeDto> colLT = new Collection<ResourceTypeDto>();
                BoeTaskElementDTO newTask = new BoeTaskElementDTO();

                foreach (BoeTaskElementDTO currentTask in inTaskElements)
                {
                    if (orgTask.Id == currentTask.Id)
                    {
                        bFound = true;

                        foreach (ResourceTypeDto lt in orgTask.taskElementLabors)
                        {
                            ResourceTypeDto foundLT = (from x in currentTask.taskElementLabors
                                                       where x.Id == lt.Id
                                                       select x).FirstOrDefault();

                            if (foundLT != null)
                            {
                                processedLaborIds.Add(lt.Id);

                                if (foundLT.Updateable != UpdateType.Deleted)
                                {
                                    colLT.Add(foundLT);
                                    if (foundLT.ResourceID.HasValue && !resourceIds.Contains(foundLT.ResourceID.Value))
                                    {
                                        resourceIds.Add(foundLT.ResourceID.Value);
                                    }
                                    if (foundLT.BusinessResourceCodeID.HasValue && !resourceIds.Contains(foundLT.BusinessResourceCodeID.Value))
                                    {
                                        resourceIds.Add(foundLT.BusinessResourceCodeID.Value);
                                    }
                                }
                            }
                            else
                            {
                                processedLaborIds.Add(lt.Id);

                                colLT.Add(lt);
                                if (lt.ResourceID.HasValue && !resourceIds.Contains(lt.ResourceID.Value))
                                {
                                    resourceIds.Add(lt.ResourceID.Value);
                                }
                                if (lt.BusinessResourceCodeID.HasValue && !resourceIds.Contains(lt.BusinessResourceCodeID.Value))
                                {
                                    resourceIds.Add(lt.BusinessResourceCodeID.Value);
                                }
                            }
                        }

                        newTask = this.CopyCurrentTask(currentTask, colLT);
                        
                        break;
                    }
                }

                if (bFound)
                {
                    //Add new labor types to the task.
                    foreach (BoeTaskElementDTO tempTask in inTaskElements)
                    {
                        if (orgTask.Id == tempTask.Id)
                        { 
                            foreach (ResourceTypeDto tempLT in tempTask.taskElementLabors)
                            {
                                ICollection<int> foundId = (from x in processedLaborIds
                                                            where x == tempLT.Id
                                                            select x).ToList();

                                if (!foundId.Any())
                                {
                                    if (tempLT.Updateable != UpdateType.Deleted)
                                    {
                                        colLT.Add(tempLT);
                                    }
                                }
                            }
                            
                            newTaskElementList.Add(this.CopyCurrentTask(newTask, colLT));
                        }
                    }
                }
                else
                {
                    newTaskElementList.Add(orgTask);

                    foreach (ResourceTypeDto lt in orgTask.taskElementLabors)
                    {
                        if (lt.ResourceID.HasValue && !resourceIds.Contains(lt.ResourceID.Value))
                        {
                            resourceIds.Add(lt.ResourceID.Value);
                        }
                        if (lt.BusinessResourceCodeID.HasValue && !resourceIds.Contains(lt.BusinessResourceCodeID.Value))
                        {
                            resourceIds.Add(lt.BusinessResourceCodeID.Value);
                        }
                    }
                }
            }

            this.BoeIdToTaskElements.Remove(boeID);
            this.BoeIdToTaskElements.Add(boeID,newTaskElementList);

            //Add any resources that were added by the user to the resource list.
            foreach (BoeTaskElementDTO currentTask in inTaskElements)
            {
                // make sure that each of the (passed-in) task elements is on the "list" (so new tasks elements will be factored-in to the BOE sum)
                if (!newTaskElementList.Exists(t => t.Id == currentTask.Id))
                {
                    newTaskElementList.Add(currentTask);
                }

                foreach (ResourceTypeDto lt in currentTask.taskElementLabors)
                {
                    if (lt.Updateable != UpdateType.Deleted)
                    {
                        if (lt.ResourceID.HasValue && !resourceIds.Contains(lt.ResourceID.Value))
                        {
                            resourceIds.Add(lt.ResourceID.Value);
                        }
                        if (lt.BusinessResourceCodeID.HasValue && !resourceIds.Contains(lt.BusinessResourceCodeID.Value))
                        {
                            resourceIds.Add(lt.BusinessResourceCodeID.Value);
                        }
                    }
                }
            }

            //Update the resources to match new task elements
            ICollection<ResourceDTO> resources = workspace.ResourcesForWsResourceListId.Where(i => resourceIds.Contains(i.Id)).ToCollection<ResourceDTO>();

            this.BoeIdToResources.Remove(boeID);
            this.BoeIdToResources.Add(boeID, resources);
        }

        private BoeTaskElementDTO CopyCurrentTask(BoeTaskElementDTO task, Collection<ResourceTypeDto> colLT)
        {
            BoeTaskElementDTO newTask = new BoeTaskElementDTO();

            newTask.Id = task.Id;
            newTask.BOETaskID = task.BOETaskID;
            newTask.TaskTitle = task.TaskTitle;
            newTask.MOQType = task.MOQType;
            newTask.MOQTypeName = task.MOQTypeName;
            newTask.MOQHoursEquation = task.MOQHoursEquation;
            newTask.MOQText = task.MOQText;
            newTask.IMS_ID = task.IMS_ID;
            newTask.Description = task.Description;
            newTask.StartDate = task.StartDate;
            newTask.EndDate = task.EndDate;
            newTask.OrdinaryVariables = task.OrdinaryVariables;
            newTask.WorkspaceVariableIDs = task.WorkspaceVariableIDs;
            newTask.taskElementLabors = colLT;
            newTask.LaborTypeWarningFlag = task.LaborTypeWarningFlag;
            newTask.CustomFieldValueContainers = task.CustomFieldValueContainers;
            newTask.TaskElementType = task.TaskElementType;

            return newTask;
        }

        /// <summary>
        /// Finds the nested boes.
        /// </summary>
        /// <param name="parentWbs">The parent WBS.</param>
        /// <param name="boes">The boes.</param>
        /// <param name="wbsElements">The WBS elements.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ICollection<BoeDTO> FindNestedBoes(WbsDTO parentWbs, IReadOnlyCollection<BoeDTO> boes, IReadOnlyCollection<WbsDTO> wbsElements)
        {
            List<BoeDTO> nestedBoes = new List<BoeDTO>();

            // A nested wbs is defined as a wbs nested inside an originalWbs where wbs.WBSNumber.StartsWith(originalWbs.WBSNumber + ".")))
            // A nested boe is a boe that is assigned to a nested wbs
            foreach (WbsDTO wbs in wbsElements)
            {
                if (wbs.Id == parentWbs.Id || wbs.WbsNumber.StartsWith(parentWbs.WbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                {
                    List<BoeDTO> matchingBoes = boes.Where(b => b.WBSID == wbs.Id).ToList();

                    if (matchingBoes.Any())
                    {
                        nestedBoes.AddRange(matchingBoes);
                    }
                }
            }

            return nestedBoes;
        }
    }
}