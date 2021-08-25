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

    public class CustomFieldValueDTODataLoader : BulkDataLoader<CustomFieldValueDTO, CustomFieldValue>, ICustomFieldValueDTODataLoader
    {
        public CustomFieldValueDTODataLoader()
        {
            this.Log = new Logger(typeof(CustomFieldValueDTODataLoader));
        }

        #region Retrieve

        /// <summary>
        /// Get custom field value dtos by custom field id.
        /// </summary>
        /// <param name="inCustomFieldID">Custom field id.</param>
        /// <returns>Collection of custom field value ids</returns>
        [DbQuery]
        virtual public ICollection<CustomFieldValueDTO> GetCustomFieldValueDTOsByCustomFieldID(int inCustomFieldID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<CustomFieldValueDTO> toReturn = new Collection<CustomFieldValueDTO>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.CustomFieldValues
                                where c.CustomFieldID == inCustomFieldID
                                select new CustomFieldValueDTO
                                {
                                    Id = c.CustomFieldValueID,
                                    CustomFieldValueID = c.CustomFieldValueID,
                                    CustomFieldValueName = c.CustomFieldValueName,
                                    CustomFieldValueDescription = c.CustomFieldValueDescription,
                                    CustomFieldID = c.CustomFieldID,
                                    CustomFieldValueInUseFlag = c.CustomFieldValueInUseFlag,
                                    UpdateDate = c.UpdateDT
                                }).OrderBy(o => o.CustomFieldValueDescription).ToCollection<CustomFieldValueDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get custom field value dtos by custom field ids.
        /// </summary>
        /// <param name="inCustomFieldID">Custom field ids.</param>
        /// <returns>Collection of custom field value ids</returns>
        [DbQuery]
        virtual public ICollection<CustomFieldValueDTO> GetCustomFieldValueDTOsByCustomFieldIds(ICollection<int> inCustomFieldIds)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<CustomFieldValueDTO> toReturn = new Collection<CustomFieldValueDTO>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.CustomFieldValues
                                where inCustomFieldIds.Contains(c.CustomFieldID)
                                select new CustomFieldValueDTO
                                {
                                    Id = c.CustomFieldValueID,
                                    CustomFieldValueID = c.CustomFieldValueID,
                                    CustomFieldValueName = c.CustomFieldValueName,
                                    CustomFieldValueDescription = c.CustomFieldValueDescription,
                                    CustomFieldID = c.CustomFieldID,
                                    CustomFieldValueInUseFlag = c.CustomFieldValueInUseFlag,
                                    UpdateDate = c.UpdateDT
                                }).OrderBy(o => o.CustomFieldValueDescription).ToCollection<CustomFieldValueDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Gets a collection of custom field values by Ids.
        /// </summary>
        /// <param name="ids">Custom field value Ids.</param>
        /// <returns>Collection of custom field values.</returns>
        [DbQuery]
        public override ICollection<CustomFieldValueDTO> GetByIds(ICollection<int> ids)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<CustomFieldValueDTO> toReturn = new List<CustomFieldValueDTO>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.CustomFieldValues.Where(c => ids.Contains(c.CustomFieldValueID))
                                select new CustomFieldValueDTO
                                {
                                    Id = c.CustomFieldValueID,
                                    CustomFieldValueID = c.CustomFieldValueID,
                                    CustomFieldValueName = c.CustomFieldValueName,
                                    CustomFieldValueDescription = c.CustomFieldValueDescription,
                                    CustomFieldID = c.CustomFieldID,
                                    CustomFieldValueInUseFlag = c.CustomFieldValueInUseFlag,
                                    UpdateDate = c.UpdateDT
                                }).ToCollection<CustomFieldValueDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of task element to custom field value Id.
        /// </summary>
        /// <param name="inTaskElementIDs">Task element Ids.</param>
        /// <returns>Mapping of task element to custom field value Id.</returns>
        [DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTaskElementIDs(Collection<int> inTaskElementIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.BOETaskElementCustomFieldValueXREFs
                                    where inTaskElementIDs.Contains(c.BOETaskElementID)
                                    select new
                                    {
                                        TaskElementID = c.BOETaskElementID,
                                        ContainerID = c.BTECFVID,
                                        CustomFieldValueID = (int)c.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.TaskElementID))
                        {
                            // No need to check WBSID.HasValue because nulls are excluded in the query
                            toReturn[item.TaskElementID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.TaskElementID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of MOQ Type Table to Custom Field Value ID
        /// </summary>
        /// <param name="moqTypeTableIds">MOQ Type Table IDs</param>
        /// <returns>Mapping of MOQ Type Table to Custom Field Value ID</returns>
        [DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByMoqTypeTableIds(ICollection<int> moqTypeTableIds)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.MoqTypeTableCustomFieldValueXREFs
                                    where moqTypeTableIds.Contains(c.MoqTypeTableDataId)
                                    select new
                                    {
                                        MoqTypeTableId = c.MoqTypeTableDataId,
                                        ContainerId = c.Id,
                                        CustomFieldValueId = c.CustomFieldValueId
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.MoqTypeTableId))
                        {
                            toReturn[item.MoqTypeTableId].Add(new KeyValuePair<int, int>(item.ContainerId, item.CustomFieldValueId));
                        }
                        else
                        {
                            toReturn[item.MoqTypeTableId] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerId, item.CustomFieldValueId) };
                        }
                    }
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of labor type to custom field value Id.
        /// </summary>
        /// <param name="inLaborTypeIDs">Labor Type Ids.</param>
        /// <returns>Mapping of task element to custom field value Id.</returns>
        [DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByLaborTypeIDs(Collection<int> inLaborTypeIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.BOELaborTypeCustomFieldValueXREFs
                                    where inLaborTypeIDs.Contains(c.BOELaborTypeID)
                                    select new
                                    {
                                        BOELaborTypeID = c.BOELaborTypeID,
                                        ContainerID = c.BLTCFVID,
                                        CustomFieldValueID = (int)c.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.BOELaborTypeID))
                        {
                            // No need to check WBSID.HasValue because nulls are excluded in the query
                            toReturn[item.BOELaborTypeID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.BOELaborTypeID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of travel element to custom field value Id.
        /// </summary>
        /// <param name="inTravelElementIDs">travel element Ids.</param>
        /// <returns>Mapping of travel element to custom field value Id.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTravelElementIDs(Collection<int> inTravelElementIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.TravelTripTaskElementCustomFieldValueXREFs
                                    where inTravelElementIDs.Contains(c.TravelTripTaskElementID)
                                    select new
                                    {
                                        TaskElementID = c.TravelTripTaskElementID,
                                        ContainerID = c.TTECFVID,
                                        CustomFieldValueID = (int)c.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.TaskElementID))
                        {
                            toReturn[item.TaskElementID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.TaskElementID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of travel trip to custom field value Id.
        /// </summary>
        /// <param name="inTravelTripIDs">travel trip Ids.</param>
        /// <returns>Mapping of travel trip to custom field value Id.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByTravelTripIDs(Collection<int> inTravelTripIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.TravelTripCustomFieldValueXREFs
                                    where inTravelTripIDs.Contains(c.TravelTripID)
                                    select new
                                    {
                                        TaskElementID = c.TravelTripID,
                                        ContainerID = c.TCFVID,
                                        CustomFieldValueID = (int)c.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.TaskElementID))
                        {
                            toReturn[item.TaskElementID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.TaskElementID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Gets a mapping of resource to custom field value Id.
        /// </summary>
        /// <param name="inResourceIDs">Resource Ids</param>
        /// <returns>Mapping of resource to custom field value Id</returns>
        [DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIDsByResourceIDs(Collection<int> inResourceIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var response = (from c in gbe.BOELaborTypeCustomFieldValueXREFs
                                    where inResourceIDs.Contains(c.BOELaborTypeID)
                                    select new
                                    {
                                        ResourceID = c.BOELaborTypeID,
                                        ContainerID = c.BLTCFVID,
                                        CustomFieldValueID = (int)c.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.ResourceID))
                        {
                            // No need to check WBSID.HasValue because nulls are excluded in the query
                            toReturn[item.ResourceID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.ResourceID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }
                return toReturn;
            }
        }

        [DbQuery]
        virtual public Dictionary<int, ICollection<KeyValuePair<int, int>>> GetLaborTypeCustomFieldValueIDsContainerIDsByTaskElementIDs(Collection<int> inTaskElementIDs)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();

                using (GenBoeEntities db = new GenBoeEntities())
                {
                    var response = (from b in db.BOEs
                                    join t in db.BOETaskElements on b.BOEID equals t.BOEID
                                    join lt in db.BOELaborTypes on t.BOETaskElementID equals lt.BOETaskElementID
                                    join r in db.Resources on lt.ResourceID equals r.ResourceID
                                    join po in db.PerformingOrganizations on lt.PerformingOrganizationID equals po.PerformingOrganizationID
                                    join x in db.BOELaborTypeCustomFieldValueXREFs on lt.BOELaborTypeID equals x.BOELaborTypeID
                                    join cfv in db.CustomFieldValues on x.CustomFieldValueID equals cfv.CustomFieldValueID
                                    join cf in db.CustomFields on cfv.CustomFieldID equals cf.CustomFieldID
                                    where inTaskElementIDs.Contains(t.BOETaskElementID)
                                    select new
                                    {
                                        LaborTypeID = x.BOELaborTypeID,
                                        ContainerID = x.BLTCFVID,
                                        CustomFieldValueID = x.CustomFieldValueID
                                    }).ToList();

                    foreach (var item in response)
                    {
                        if (toReturn.ContainsKey(item.LaborTypeID))
                        {
                            // No need to check WBSID.HasValue because nulls are excluded in the query
                            toReturn[item.LaborTypeID].Add(new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID));
                        }
                        else
                        {
                            toReturn[item.LaborTypeID] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(item.ContainerID, item.CustomFieldValueID) };
                        }
                    }
                }

                return toReturn;
            }
        }

        virtual public IList<CustomFieldContainerFieldValueMapping> GetBOECustomFieldContainerFieldValueMappings(int boeID)
        {
            IList<CustomFieldContainerFieldValueMapping> results;

            using (GenBoeEntities db = new GenBoeEntities())
            {
                results =
                    (from x in db.BOECustomFieldValueXREFs
                     join cfv in db.CustomFieldValues on x.CustomFieldValueID equals cfv.CustomFieldValueID
                     where x.BOEID == boeID
                     select new CustomFieldContainerFieldValueMapping
                     {
                         ContainerID = x.BCFVID,
                         CustomFieldID = cfv.CustomFieldID,
                         CustomFieldValueID = cfv.CustomFieldValueID,
                         CustomFieldValueDescription = cfv.CustomFieldValueDescription
                     }).ToList();
            }

            return results;
        }

        virtual public IList<CustomFieldContainerFieldValueMapping> GetTaskElementCustomFieldContainerFieldValueMappings(int taskElementID)
        {
            IList<CustomFieldContainerFieldValueMapping> results;

            using (GenBoeEntities db = new GenBoeEntities())
            {
                results =
                    (from x in db.BOETaskElementCustomFieldValueXREFs
                     join cfv in db.CustomFieldValues on x.CustomFieldValueID equals cfv.CustomFieldValueID
                     where x.BOETaskElementID == taskElementID
                     select new CustomFieldContainerFieldValueMapping
                     {
                         ContainerID = x.BTECFVID,
                         CustomFieldID = cfv.CustomFieldID,
                         CustomFieldValueID = cfv.CustomFieldValueID,
                         CustomFieldValueDescription = cfv.CustomFieldValueDescription
                     }).ToList();
            }

            return results;
        }

        virtual public IList<CustomFieldContainerFieldValueMapping> GetResourceCustomFieldContainerFieldValueMappings(int laborTypeID)
        {
            IList<CustomFieldContainerFieldValueMapping> results;

            using (GenBoeEntities db = new GenBoeEntities())
            {
                results =
                    (from x in db.BOELaborTypeCustomFieldValueXREFs
                     join cfv in db.CustomFieldValues on x.CustomFieldValueID equals cfv.CustomFieldValueID
                     where x.BOELaborTypeID == laborTypeID
                     select new CustomFieldContainerFieldValueMapping
                     {
                         ContainerID = x.BLTCFVID,
                         CustomFieldID = cfv.CustomFieldID,
                         CustomFieldValueID = cfv.CustomFieldValueID,
                         CustomFieldValueDescription = cfv.CustomFieldValueDescription
                     }).ToList();
            }

            return results;
        }

        #endregion Retrieve

        #region Commit

        public override Dictionary<int, int> Save(ICollection<CustomFieldValueDTO> dtosToSave)
        {
            // Do not attempt to save dtos that have an UpdateType of none.
            return base.Save(dtosToSave.Where(d => d.Updateable != UpdateType.None).ToCollection<CustomFieldValueDTO>());
        }

        protected override int? Upsert(CustomFieldValueDTO dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? result = null;

                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        result = gbe.upsertCustomFieldValue(
                            dtoToUpsert.CustomFieldValueID,
                            dtoToUpsert.CustomFieldValueName,
                            dtoToUpsert.CustomFieldValueDescription,
                            dtoToUpsert.CustomFieldID,
                            dtoToUpsert.UpdateDate).First();
                    }
                    dtoToUpsert.CustomFieldValueID = result != null ? (int)result : dtoToUpsert.CustomFieldValueID;
                }
                return result;
            }
        }

        protected override int? Delete(CustomFieldValueDTO dtoToDelete)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;

                if (dtoToDelete != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteCustomFieldValueByCustomFieldValueID(dtoToDelete.CustomFieldValueID, dtoToDelete.UpdateDate);
                    }

                    toReturn = dtoToDelete.Id;
                }
                return toReturn;
            }
        }

        #endregion Commit

        /// <summary>
        /// Refresh the In Use Flags for a workspace
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to refresh.</param>
        public void RefreshCustomFieldInUseByWorkspaceID(int workspaceId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.refreshCustomFieldInUseFlagByWorkspaceID(workspaceId);
                }
            }
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for Custom Field Value DTOs
        /// </summary>
        /// <returns>Meta data required for bulk save processing</returns>
        override public BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkDeleteStoredProcedureName = "deleteBOELaborTypeCustomFieldValueviaTableParameter";
            metaData.BulkInsertStoredProcedureName = "insertBOELaborTypeCustomFieldValueviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateBOELaborTypeCustomFieldValueviaTableParameter";

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
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override CustomFieldValue ConvertDtoToEntity(CustomFieldValueDTO dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            CustomFieldValue entity = null;

            entity = new CustomFieldValue()
            {
                CustomFieldValueID = dtoToConvert.CustomFieldValueID,
                CustomFieldValueName = dtoToConvert.CustomFieldValueName,
                CustomFieldValueDescription = dtoToConvert.CustomFieldValueDescription,
                CustomFieldID = dtoToConvert.CustomFieldValueID,
                CustomFieldValueInUseFlag = dtoToConvert.CustomFieldValueInUseFlag,
                UpdateDT = dtoToConvert.UpdateDate
            };

            return entity;
        }
    }
}
