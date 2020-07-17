// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Full boe data importer
    /// </summary>
    public class FullBoeDataImporter
    {
        private IBoeMediator _boeMediator;
        private IBoeTaskElementMediator _boeTaskElementMediator;
        private BOEImportMerge _boeMerge;
        BoeTaskElementImportMerge _taskMerger;
        ResourceTypeImportMerge _resourceMerger;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="boeMediator">boe mediator</param>
        /// <param name="boeTaskElementMediator">task element mediator</param>
        /// <param name="boeMerge">boe merge</param>
        /// <param name="taskMerger">task merge</param>
        /// <param name="resourceMerger">resource merge</param>
        public FullBoeDataImporter(
            IBoeMediator boeMediator, 
            IBoeTaskElementMediator boeTaskElementMediator, 
            BOEImportMerge boeMerge,
            BoeTaskElementImportMerge taskMerger, 
            ResourceTypeImportMerge resourceMerger)
        {
            this._boeMediator = boeMediator;
            this._boeTaskElementMediator = boeTaskElementMediator;
            this._boeMerge = boeMerge;
            this._taskMerger = taskMerger;
            this._resourceMerger = resourceMerger;
        }

        public IList<int> ImportAndMergeOfflineImportData(WorkofflineImport allImportData, FullWorkspace workspace)
        {
            //import data or workspace cannot be null
            if (allImportData == null || workspace == null)
            {
                return null;
            }

            IList<int> affectedBoeIds = new List<int>();

            //BOE's and BoeTaskElements get saved separately

            //perform BOE extraction
            Collection<BoeDTO> importedBoeData = this._boeMerge.ExtractValidBoesFromOfflineImport(allImportData);

            //for each record, merge with existing data and perform a
            //  mediated save.

            foreach (BoeDTO boe in importedBoeData)
            {
                //check if an update should be performed
                var originalImportedBoe = allImportData.ImportedBoes.FirstOrDefault(b => b.Id == boe.Id);
                if (originalImportedBoe != null && originalImportedBoe.ImportTypes.Contains(BoeImportResult.UpdateBoe))
                {
                    BoeDTO modifiedBoe = this._boeMerge.MergeBoeWithData(boe);
                    this._boeMediator.MediatedSave(workspace, modifiedBoe);

                    if (!affectedBoeIds.Contains(modifiedBoe.Id))
                    {
                        affectedBoeIds.Add(modifiedBoe.Id);
                    }
                }
            }

            //Collection used to store finalized merged BoeTaskElements for saving
            Collection<BoeTaskElementDTO> finalTaskElementCollection =
                new Collection<BoeTaskElementDTO>();

            //iterate over imported BOE's to get each 
            //  raw, imported BoeTaskElements to import/merge/save BoeTaskElements
            //NOTE: Since BOE's cannot be added through this interface, there is 
            //  no risk of missing the association between the task elements in this 
            //  loop with a BOE that was created in the previous BOE save loop.
            foreach (WorkofflineImportedBoe rawBoe in allImportData.ImportedBoes)
            {
                //iterate over the raw BoeTaskElements 
                foreach (WorkofflineImportedTaskElement rawTaskElement in rawBoe.ImportedTaskElements)
                {
                    bool isTaskElementValid =
                        rawTaskElement.ImportTypes.Contains(TaskElementImportResult.CreateTaskElement) || rawTaskElement.ImportTypes.Contains(TaskElementImportResult.UpdateTaskElement);

                    if (isTaskElementValid)
                    {
                        //Extract and merge imported data with existing data
                        BoeTaskElementDTO mergedBoeTask = this._taskMerger.MergeTaskWithData(this._taskMerger.ExtractValidTaskElementsFromOfflineImport(rawTaskElement), isTaskElementValid, workspace);

                        //ensure the BOEID is correct, this value is used in member collections
                        if (mergedBoeTask.BoeID <= 0)
                        {
                            mergedBoeTask.BoeID = rawBoe.Id;
                        }

                        if (!affectedBoeIds.Contains(mergedBoeTask.BoeID))
                        {
                            affectedBoeIds.Add(mergedBoeTask.BoeID);
                        }

                        decimal moqTotalHours = this._resourceMerger.GetMOQTotalHours(mergedBoeTask, workspace);

                        //because LaborTypeDTO's are saved with BoeTaskElements, make sure the 
                        //  imported LaborTypes(and spreads) are associated with the correct TaskElement.
                        //NOTE: this must all be done in the same loop in case the TaskElement associated
                        //  with the LaborType is new.

                        Collection<ResourceTypeDto> resources = this._resourceMerger.ExtractLaborTypesFromOfflineImport(rawTaskElement);
                        foreach (ResourceTypeDto importedLaborType in resources)
                        {
                            int importedLaborTypeId = importedLaborType.Id;

                            //determine if the Labor Type should ingest all updates or only child records
                            WorkofflineImportedResourceType originalImportedLaborType = rawTaskElement.ImportedResourceTypes.FirstOrDefault(l => l.Id == importedLaborTypeId);
                            bool updateAllLaborTypeData = (originalImportedLaborType.ImportTypes.Contains(LaborTypeImportResult.AddLaborType) || originalImportedLaborType.ImportTypes.Contains(LaborTypeImportResult.UpdateLaborType));

                            //incorporate the imported data
                            ResourceTypeDto originalLaborType = this._resourceMerger.MergeLaborTypeWithData(importedLaborType, mergedBoeTask, updateAllLaborTypeData, moqTotalHours, workspace);

                            //if it doesn't already exist on the TaskElement, then add it
                            if (!mergedBoeTask.taskElementLabors.Any(l => l.Id == importedLaborTypeId))
                            {
                                mergedBoeTask.taskElementLabors.Add(originalLaborType);
                            }

                            if (updateAllLaborTypeData)
                            {
                                if (!affectedBoeIds.Contains(mergedBoeTask.BoeID))
                                {
                                    affectedBoeIds.Add(mergedBoeTask.BoeID);
                                }
                            }
                        }

                        #region Apply spread adjustments if needed (100% allocation case)

                        this._resourceMerger.AdjustDeltaHours(mergedBoeTask.taskElementLabors, moqTotalHours, workspace);

                        #endregion

                        //BoeTaskElements and Resource/Labor types are saved together
                        finalTaskElementCollection.Add(mergedBoeTask);
                    }
                }
            }
            // Processing the task elements puts them in a shuffled order different from where entered in the spreadsheet. 
            // Order prior to save so new task elements display order matches the spreadsheet.
            finalTaskElementCollection = finalTaskElementCollection.OrderByDescending(i => i.Id).ToCollection<BoeTaskElementDTO>();
            this._boeTaskElementMediator.MediatedSaveTaskElements(finalTaskElementCollection, workspace);

            workspace.RefreshBoes();

            return affectedBoeIds;
        }
    }
}
