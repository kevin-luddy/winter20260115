// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    /// <summary>
    /// Loader for the Custom Field values associated with a BOE Task Element
    /// </summary>
    public class BoeTaskElementCustomFieldValueXREFLoader : BulkDataLoader<CustomFieldValueContainer, BOETaskElementCustomFieldValueXREF>, IBoeTaskElementCustomFieldValueXREFLoader
    {
        private Logger log = new Logger(typeof(BoeTaskElementCustomFieldValueXREFLoader));

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoeTaskElementCustomFieldValueXREFLoader()
        {
        }

        /// <summary>
        /// Get CustomfieldVAlueContainers by their ids.
        /// </summary>
        /// <param name="ids">id of the BOE Task Element Custom Field Value ID to retrieve</param>
        /// <returns></returns>
        override public ICollection<CustomFieldValueContainer> GetByIds(ICollection<int> ids)
        {
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                containers = (from data in gbe.BOETaskElementCustomFieldValueXREFs
                              where ids.Contains(data.BTECFVID)
                              select new CustomFieldValueContainer
                              {
                                  OwnerID = data.BOETaskElementID,
                                  ContainerID = data.BTECFVID,
                                  CustomFieldValueID = data.CustomFieldValueID,
                                  UpdateDate = data.UpdateDT
                              }).ToCollection();
            }

            return containers;
        }

        /// <summary>
        /// This funciton will handle an individual BOE Task Element Custom Field Value delete
        /// </summary>
        /// <param name="dtoToDelete">the selected Task Element to delete</param>
        protected override int? Delete(CustomFieldValueContainer dtoToDelete)
        {
            int? toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                if (dtoToDelete == null)
                {
                    throw new ArgumentNullException(nameof(dtoToDelete));
                }

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // OwnerID is a generic property that holds the id of the owner of this custom field value and in this case it's the BOETAskElementID
                    toReturn = gbe.deleteBOETaskElementCustomFieldValue(
                            dtoToDelete.ContainerID,
                            dtoToDelete.OwnerID,
                            dtoToDelete.CustomFieldValueID,
                            dtoToDelete.UpdateDate,
                            dtoToDelete.IsOpenEnded);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save/Create the BOE Task Element Custom Field Value
        /// </summary>
        /// <param name="dtoToUpsert">the element to save</param>
        /// <returns>the task element details</returns>
        override protected int? Upsert(CustomFieldValueContainer dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                if (dtoToUpsert == null)
                {
                    throw new ArgumentNullException(nameof(dtoToUpsert));
                }

                int containerID = 0;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    int? sprocResults =
                    gbe.upsertBOETaskElementCustomFieldValue(
                        dtoToUpsert.ContainerID,
                        dtoToUpsert.OwnerID,
                        dtoToUpsert.CustomFieldID,
                        dtoToUpsert.CustomFieldValueID,
                        dtoToUpsert.OpenEndedValue,
                        dtoToUpsert.UpdateDate,
                        dtoToUpsert.IsOpenEnded).FirstOrDefault();

                    containerID = sprocResults.HasValue ? sprocResults.Value : 0;

                    // Set the new ID on the DTO for later use, if necessary
                    if (dtoToUpsert.Id < 0)
                    {
                        dtoToUpsert.Id = containerID;
                    }
                    // Need to save any workspace variables that are used by this task element
                    // It may be possible that the workspaceVariables count is 0, but we still need
                    // to go through with the save because a workspace variable association may have
                    // been deleted
                } // end using gbe

                return containerID;
            }
        }

        /// <summary>
        /// Save BOE Task Element Custom Field Value Containers
        /// </summary>
        /// <param name="inBOETaskElementCustomFieldValueContainers"></param>
        virtual public void SaveBOETaskElementCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOETaskElementCustomFieldValueContainers, int inBoeTaskElementID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                if (inBOETaskElementCustomFieldValueContainers == null)
                {
                    throw new ArgumentNullException(nameof(inBOETaskElementCustomFieldValueContainers));
                }

                foreach (CustomFieldValueContainer BOETaskElementCustomFieldValueXref in inBOETaskElementCustomFieldValueContainers)
                {
                    this.SaveBOETaskElementCustomFieldValueContainer(BOETaskElementCustomFieldValueXref, inBoeTaskElementID);
                }
            }
        }

        /// <summary>
        /// Save a single BOE Task Element Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer"></param>
        virtual public void SaveBOETaskElementCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBoeTaskElementID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                if (inCustomFieldValueContainer == null)
                {
                    throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
                }

                inCustomFieldValueContainer.OwnerID = inBoeTaskElementID;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    if (inCustomFieldValueContainer.Updateable == UpdateType.Deleted)
                    {
                        this.Delete(inCustomFieldValueContainer);
                    }
                    else if (inCustomFieldValueContainer.Updateable == UpdateType.Upsert)
                    {
                        this.Upsert(inCustomFieldValueContainer);
                    }

                }
            }
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for BoeTaskElementDTOs
        /// </summary>
        /// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertBOETaskElementCustomFieldValueviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateBOETaskElementCustomFieldValueviaTableParameter";
            metaData.BulkDeleteStoredProcedureName = "deleteBOETaskElementCustomFieldValueviaTableParameter";

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

            metaData.DBTableTypeName = "TT_BOETaskElementCustomFieldValueXREF";
            metaData.StoredProcedureTableTypeParameterName = "@BOETaskElementCustomFieldValueXREF";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "BTECFVID", "UpdateDT", "BOETaskElementID", "CustomFieldID", "CustomFieldValueID", "CustomFieldValueDescription", "IsOpenEnded"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the Task Element DTO into a BOETaskElementCustomFieldValueXREF entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override BOETaskElementCustomFieldValueXREF ConvertDtoToEntity(CustomFieldValueContainer dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            BOETaskElementCustomFieldValueXREF entity = null;

            entity = new BOETaskElementCustomFieldValueXREF()
            {
                BTECFVID = dtoToConvert.ContainerID,
                BOETaskElementID = dtoToConvert.OwnerID,
                CustomFieldValueID = dtoToConvert.CustomFieldValueID,
                UpdateDT = dtoToConvert.UpdateDate,
                CustomFieldValue = new CustomFieldValue()
                {
                    CustomFieldID = dtoToConvert.CustomFieldID,
                    CustomFieldValueDescription = dtoToConvert.OpenEndedValue,
                    CustomField = new CustomField() { IsOpenEnded = dtoToConvert.IsOpenEnded }
                }
            };

            return entity;
        }
    }
}
