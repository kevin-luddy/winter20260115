// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class BoeTaskElementImportMerge
    {
        private IFullObjectFactory _factory;
        private ICustomFieldValueDTODataLoader _customFieldValueLoader;

        public BoeTaskElementImportMerge(IFullObjectFactory factory, ICustomFieldValueDTODataLoader customFieldValueLoader)
        {
            this._factory = factory;
            this._customFieldValueLoader = customFieldValueLoader;
        }

        public BoeTaskElementDTO ExtractValidTaskElementsFromOfflineImport(WorkofflineImportedTaskElement importedBoe)
        {
            if (importedBoe != null)
            {
                importedBoe.CloneImportedCustomFieldsToContainer();
            }
            return importedBoe as BoeTaskElementDTO;
        }

        public BoeTaskElementDTO MergeTaskWithData(BoeTaskElementDTO importedData, bool updateAllTaskElementData, WorkspaceDTO workspace)
        {
            if (importedData == null)
            {
                return null;
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            BoeTaskElementDTO originalTask = null;

            if (importedData.Id > 0)
            {
                originalTask = this._factory.CreateTaskElement(importedData.Id, workspace.DecimalPrecision, workspace.CostDecimalPrecision);
            }

            if (originalTask == null)
            {
                originalTask = new BoeTaskElementDTO();
                originalTask.Id = importedData.Id;
            }
            originalTask.Updateable = UpdateType.Upsert;

            if (updateAllTaskElementData)
            {
                originalTask.TaskElementType = importedData.TaskElementType;
                originalTask.MOQType = importedData.MOQType;

                if (importedData.Description != null)
                {
                    originalTask.Description = importedData.Description;
                }
                if (importedData.MOQText != null)
                {
                    originalTask.MOQText = importedData.MOQText;
                }
                if (importedData.TaskTitle != null)
                {
                    originalTask.TaskTitle = importedData.TaskTitle;
                }
                if (importedData.BOETaskID != null)
                {
                    originalTask.BOETaskID = importedData.BOETaskID;
                }
                if (importedData.EndDate.HasValue)
                {
                    originalTask.EndDate = importedData.EndDate;
                }
                if (importedData.StartDate.HasValue)
                {
                    originalTask.StartDate = importedData.StartDate;
                }
                if (importedData.MOQHoursEquation != null)
                {
                    originalTask.MOQHoursEquation = importedData.MOQHoursEquation;
                }

                originalTask.WorkspaceVariableIDs = importedData.WorkspaceVariableIDs ?? new Collection<int>();

                IList<CustomFieldContainerFieldValueMapping> taskElementCustomFields = this._customFieldValueLoader.GetTaskElementCustomFieldContainerFieldValueMappings(importedData.Id);
                originalTask.CustomFieldValueContainers.Merge(importedData.CustomFieldValueContainers, taskElementCustomFields);
            }

            return originalTask;
        }
    }
}
