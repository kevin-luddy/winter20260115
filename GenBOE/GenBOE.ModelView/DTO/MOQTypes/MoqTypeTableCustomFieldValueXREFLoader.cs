// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    /// Loader for the Custom Field values associated with an MOQ Type Table
    /// </summary>
    public class MoqTypeTableCustomFieldValueXREFLoader : BulkDataLoader<CustomFieldValueContainer, MoqTypeTableCustomFieldValueXREF>, IMoqTypeTableCustomFieldValueXREFLoader
    {
        private Logger log = new Logger(typeof(BoeTaskElementCustomFieldValueXREFLoader));

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTypeTableCustomFieldValueXREFLoader()
        {
        }

        /// <summary>
        /// Get CustomfieldVAlueContainers by their ids.
        /// </summary>
        /// <param name="ids">id of the MOQ Type Table Custom Field Value ID to retrieve</param>
        /// <returns>Collection of Custom Field Value Containers mataching the IDs</returns>
        override public ICollection<CustomFieldValueContainer> GetByIds(ICollection<int> ids)
        {
            Collection<CustomFieldValueContainer> containers = new Collection<CustomFieldValueContainer>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                containers = (from data in gbe.MoqTypeTableCustomFieldValueXREFs
                              where ids.Contains(data.Id)
                              select new CustomFieldValueContainer
                              {
                                  OwnerID = data.MoqTypeTableDataId,
                                  ContainerID = data.Id,
                                  CustomFieldValueID = data.CustomFieldValueId,
                                  UpdateDate = data.UpdateDT
                              }).ToCollection();
            }

            return containers;
        }

        /// <summary>
        /// This funciton will handle an individual MOQ Type Table Custom Field Value delete
        /// </summary>
        /// <param name="dtoToDelete">the selected MOQ Type Table Custom Field Value to delete</param>
        protected override int? Delete(CustomFieldValueContainer dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                _ = dtoToDelete ?? throw new ArgumentNullException(nameof(dtoToDelete));

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.deleteMoqTypeTableCustomFieldValue(dtoToDelete.ContainerID, dtoToDelete.OwnerID, dtoToDelete.CustomFieldValueID, dtoToDelete.UpdateDate, dtoToDelete.IsOpenEnded);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert the MOQ Type Table Custom Field Value Container
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>Id of the upserted container</returns>
        protected override int? Upsert(CustomFieldValueContainer dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                _ = dtoToUpsert ?? throw new ArgumentNullException(nameof(dtoToUpsert));

                int containerId = 0;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    int? result = gbe.upsertMoqTypeTableCustomFieldValue(dtoToUpsert.ContainerID, dtoToUpsert.OwnerID, dtoToUpsert.CustomFieldID, dtoToUpsert.CustomFieldValueID,
                        dtoToUpsert.OpenEndedValue, dtoToUpsert.UpdateDate, dtoToUpsert.IsOpenEnded).FirstOrDefault();

                    containerId = result ?? 0;
                }

                return containerId;
            }
        }

        /// <summary>
        /// Save MOQ Type Table Custom Field Value Containers
        /// </summary>
        /// <param name="moqTypeTableCustomFieldValueContainers">MOQ Type Table Custom Field Value Containers to save</param>
        /// <param name="moqTypeTableId">MOQ Type Table ID</param>
        public ICollection<int?> SaveMoqTypeTableCustomFieldValueContainers(ICollection<CustomFieldValueContainer> moqTypeTableCustomFieldValueContainers, int moqTypeTableId)
        {
            ICollection<int?> toReturn = new Collection<int?>();

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                _ = moqTypeTableCustomFieldValueContainers ?? throw new ArgumentNullException(nameof(moqTypeTableCustomFieldValueContainers));

                foreach(CustomFieldValueContainer container in moqTypeTableCustomFieldValueContainers)
                {
                    toReturn.Add(this.SaveMoqTypeTableCustomFieldValueContainer(container, moqTypeTableId));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save a single MOQ Type Table Custom Field Value Container
        /// </summary>
        /// <param name="moqTypeTableCustomFieldValueContainer">MOQ Type Table Custom Field Value Container to save</param>
        /// <param name="moqTypeTableId">MOQ Type Table ID</param>
        public int? SaveMoqTypeTableCustomFieldValueContainer(CustomFieldValueContainer moqTypeTableCustomFieldValueContainer, int moqTypeTableId)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                _ = moqTypeTableCustomFieldValueContainer ?? throw new ArgumentNullException(nameof(moqTypeTableCustomFieldValueContainer));

                moqTypeTableCustomFieldValueContainer.OwnerID = moqTypeTableId;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (moqTypeTableCustomFieldValueContainer.Updateable == UpdateType.Deleted)
                    {
                        toReturn = this.Delete(moqTypeTableCustomFieldValueContainer);
                    }
                    else if (moqTypeTableCustomFieldValueContainer.Updateable == UpdateType.Upsert)
                    {
                        toReturn = this.Upsert(moqTypeTableCustomFieldValueContainer);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <returns>N/A</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert Custom Field Value Container to MOQ Type Table Custom Field Value Entity
        /// </summary>
        /// <param name="dtoToConvert">DTO To convert</param>
        /// <returns>MOQ Type Table Custom Field Value Entity</returns>
        protected override MoqTypeTableCustomFieldValueXREF ConvertDtoToEntity(CustomFieldValueContainer dtoToConvert)
        {
            _ = dtoToConvert ?? throw new ArgumentNullException(nameof(dtoToConvert));

            MoqTypeTableCustomFieldValueXREF entity = new MoqTypeTableCustomFieldValueXREF()
            {
                Id = dtoToConvert.ContainerID,
                MoqTypeTableDataId = dtoToConvert.OwnerID,
                CustomFieldValueId = dtoToConvert.CustomFieldValueID,
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
