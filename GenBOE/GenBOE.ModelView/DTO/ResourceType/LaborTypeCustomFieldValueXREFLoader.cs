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
    /// Loader for CustomFieldValueXREF
    /// </summary>
    public class LaborTypeCustomFieldValueXREFLoader : BulkDataLoader<CustomFieldValueContainer, BOELaborTypeCustomFieldValueXREF>, ILaborTypeCustomFieldValueXREFLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LaborTypeCustomFieldValueXREFLoader()
        {
            this.Log = new Logger(typeof(LaborTypeCustomFieldValueXREFLoader));
        }

        /// <summary>
        /// Get CustomfieldVAlueContainers by their ids.
        /// </summary>
        /// <param name="ids">ids of the Labor Type Custom Field Value to retrieve</param>
        /// <returns></returns>
        override public ICollection<CustomFieldValueContainer> GetByIds(ICollection<int> ids)
        {
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                containers = (from data in gbe.BOELaborTypeCustomFieldValueXREFs
                                                  where ids.Contains(data.BLTCFVID)
                                                  select new CustomFieldValueContainer
                                                  {
                                                      OwnerID = data.BOELaborTypeID,
                                                      ContainerID =  data.BLTCFVID,
                                                      CustomFieldValueID = data.CustomFieldValueID,
                                                      UpdateDate = data.UpdateDT,
                                                      CustomFieldID = data.CustomFieldValue.CustomFieldID,
                                                      IsOpenEnded = data.CustomFieldValue.CustomField.IsOpenEnded,
                                                      OpenEndedValue = data.CustomFieldValue.CustomFieldValueDescription
                                                  }).ToCollection();
            }

            return containers;
        }

        /// <summary>
        /// Deletes a single BOE Labor type Custom Field Value 
        /// </summary>
        /// <param name="dtoToDelete">the selected Task Element to delete</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        override protected int? Delete(CustomFieldValueContainer dtoToDelete)
        {
            int? toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                if (dtoToDelete == null)
                {
                    throw new ArgumentNullException(nameof(dtoToDelete));
                }

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // OwnerID is a generic property that holds the id of the owner of this custom field value and in this case it's the BOETAskElementID
                    toReturn = gbe.deleteBOELaborTypeCustomFieldValue(
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
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
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
                    // to go through with the save because a workspace variable assocation may have
                    // been deleted
                } // end using gbe

                return containerID;
            }
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for DTOs
        /// </summary>
        /// <returns>Meta data required for bulk save processing</returns>
        override public BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertBOELaborTypeCustomFieldValueviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateBOELaborTypeCustomFieldValueviaTableParameter";
            metaData.BulkDeleteStoredProcedureName = "deleteBOELaborTypeCustomFieldValueviaTableParameter";

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

            metaData.DBTableTypeName = "TT_BOELaborTypeCustomFieldValueXREF";
            metaData.StoredProcedureTableTypeParameterName = "@BOELaborTypeCustomFieldValueXREF";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "BLTCFVID", "UpdateDT", "BOELaborTypeID", "CustomFieldID", "CustomFieldValueID", "CustomFieldValueDescription", "IsOpenEnded"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the Task Element DTO into a BOETaskElement entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override BOELaborTypeCustomFieldValueXREF ConvertDtoToEntity(CustomFieldValueContainer dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            BOELaborTypeCustomFieldValueXREF entity = new BOELaborTypeCustomFieldValueXREF()
            {
                BLTCFVID = dtoToConvert.ContainerID,
                BOELaborTypeID = dtoToConvert.OwnerID,
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
