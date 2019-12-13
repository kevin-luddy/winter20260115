// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Resource Spread Loader
    /// </summary>
    public class ResourceSpreadLoader : BulkDataLoader<ResourceSpreadDto, BOELaborSpread>, IResourceSpreadLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceSpreadLoader()
        {
            this.Log = new Logger(typeof(ResourceSpreadLoader));
        }

        /// <summary>
        /// Get By Ids
        /// </summary>
        /// <param name="ids">Collection of Ids</param>
        /// <returns>All Spreads with that Id</returns>
        [DbQuery]
        public override ICollection<ResourceSpreadDto> GetByIds(ICollection<int> ids)
        {
            ICollection<ResourceSpreadDto> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.BOELaborSpreads.Where(x => ids.Contains(x.BOELaborSpreadID))
                                    .Select(x => new ResourceSpreadDto
                                    {
                                        BoeID = x.BOELaborType.BOETaskElement.BOEID,
                                        Id = x.BOELaborSpreadID,
                                        LaborSpreadDate = x.LaborSpreadDate,
                                        LaborSpreadValue = x.LaborSpreadValue ?? 0,
                                        LaborTypeId = x.BOELaborType.BOELaborTypeID
                                    }).ToList();
                }
            }
            
            return toReturn;
        }

        /// <summary>
        /// Deletes a group of spreads by resource type id
        /// </summary>
        /// <param name="resourceTypeId">resource type id</param>
        /// <returns>type id</returns>
        public int DeleteByResourceTypeId(int resourceTypeId)
        {
            this.Log.Debug(string.Format("ResourceSpreadLoader.DeleteByResourceTypeId => Ntid: {0}, Item Id: {1}",
                 System.Threading.Thread.CurrentPrincipal.Identity.Name, resourceTypeId));

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                int toReturn;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.deleteBOELaborSpreadByBOELaborTypeID(resourceTypeId);

                } // end gbe

                return toReturn;
            }
        }

        /// <summary>
        /// Performs a bulk delete on the collection. All dtos marked as 'Deleted' remain marked as deleted. It is the responsibility of the caller
        /// to remove the deleted items from the dtosToDelete collection after successful completion of this method.
        /// </summary>
        /// <param name="dtosToDelete"></param>
        /// <param name="metadata">Bulk save metadata.</param>
        /// <exception cref="System.ArgumentNullException">
        /// dtosToDelete
        /// or
        /// metadata
        /// </exception>
        protected override void BulkDelete(ICollection<ResourceSpreadDto> dtosToDelete, BulkSaveMetaData metadata)
        {
            if (dtosToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtosToDelete));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (dtosToDelete.Any())
            {
                // Delete the existing spreads based on laborTypeId (parent's id).
                #region
                // extract a set of unique labor type ids from the dtosToDelete. 
                Collection<int> uniqueLaborTypeIDs = (from resourceSpread in dtosToDelete
                                                      select resourceSpread.LaborTypeId).Distinct().ToCollection();

                // create the dummy set of BOELaborType entities to hold these ids - BOELaborType
                Collection<BOELaborType> entitiesToDeleteByLaborTypeID = new Collection<BOELaborType>();
                DateTime dummyDate = new DateTime();

                foreach (int uniqueLaborTypeID in uniqueLaborTypeIDs)
                {
                    entitiesToDeleteByLaborTypeID.Add(new BOELaborType()
                    {
                        BOELaborTypeID = uniqueLaborTypeID,
                        BOELaborTypeStartDate = dummyDate,
                        BOELaborTypeEndDate = dummyDate,
                        UpdateDT = dummyDate,
                        SpreadCurveID = 0,
                        BOETaskElementID = 0,
                        PercentSpreadLocked = false,
                        HourSpreadLocked = false
                    });
                }

                // since we're deleting all the existing spreads associated with a set of parents (ResourceTypeDto.Id), we need to grab the metadata of
                // the ResourceTypeDtoDataLoader in order to get the correct stored procedure table type parameter name
                IResourceTypeLoader resourceTypeLoader = GenBOEUnityContainer.Container.Resolve(typeof(IResourceTypeLoader), null) as IResourceTypeLoader;
                BulkSaveMetaData resourceTypeMetaData = resourceTypeLoader.CreateBulkSaveMetaData();

                DataTable dataTable = StoredProcedureHelper.ToDataTable<BOELaborType>(entitiesToDeleteByLaborTypeID, resourceTypeMetaData.EntityPropertiesToMapToDataTable);

                using (GenBoeEntities objectContext = new GenBoeEntities())
                {
                    StoredProcedureHelper.ExecuteTableValueProcedure(
                        objectContext,
                        dataTable,
                        metadata.BulkDeleteStoredProcedureName, // hardcoded here because it's a special case and only occurs here
                        "@BOELaborType",
                        resourceTypeMetaData.DBTableTypeName, // table name from the parent
                        false);
                }
                #endregion 
            }
        }

        /// <summary>
        /// Inserts the new dtos after deleting all existing labor spreads. The common BulkInsert is overridden because
        /// the spreads are deleted using the parent's id (ResourceTypeDto) rather than using the ids of the spreads themselves. 
        /// That's just the way the table valued delete stored procedure is written.
        /// </summary>
        /// <param name="dtosToInsert">dtos to insert</param>
        /// <param name="metadata">Bulk save metadata.</param>
        /// <returns>Dictionary where key = old id and value = new id</returns>
        protected override Dictionary<int, int> BulkInsert(ICollection<ResourceSpreadDto> dtosToInsert, BulkSaveMetaData metadata)
        {
            if (dtosToInsert == null)
            {
                throw new ArgumentNullException(nameof(dtosToInsert), "The argument to 'BulkInsert' cannot be null.");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Dictionary<int, int> toReturn = new Dictionary<int,int>();

            if (dtosToInsert.Any())
            {
                // Deleted existing spreads based on laborTypeId (parent's id). Should be able to get this from the dtosToInsert
                this.BulkDelete(dtosToInsert, metadata);

                // now insert the new labor spreads
                toReturn = base.BulkInsert(dtosToInsert, metadata);
            }

            return toReturn;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Spread to Delete</param>
        /// <returns>deleted value</returns>
        protected override int? Delete(ResourceSpreadDto dtoToDelete)
        {
           throw (new NotImplementedException("Cannot delete Resource Spread by Id"));
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Spread to Upsert</param>
        /// <returns>Id of spread</returns>
        protected override int? Upsert(ResourceSpreadDto dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                int? result = null;

                if (dtoToUpsert != null)
                {
                    this.Log.Debug(string.Format("ResourceSpreadLoader.Upsert => Ntid: {1}, BoeId: {0}, Item Id: {2}, Value: {3}",
                         dtoToUpsert.BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name, dtoToUpsert.Id, dtoToUpsert.LaborSpreadValue));

                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        result = gbe.upsertBOELaborSpread(
                            dtoToUpsert.Id,
                            dtoToUpsert.LaborTypeId,
                            dtoToUpsert.LaborSpreadDate.Normalize(),
                            dtoToUpsert.LaborSpreadValue).FirstOrDefault();
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Override to allow us to log.. feel free to remove this
        /// </summary>
        public override IDictionary<int, int> BulkSave(ICollection<ResourceSpreadDto> dtosToSave)
        {
            if (dtosToSave == null) { throw new ArgumentNullException(nameof(dtosToSave)); }

            this.Log.Debug(string.Format("ResourceSpreadLoader.BulkSave => Ntid: {2}, BoeId: {1}, Count: {0}",
                dtosToSave.Count, dtosToSave.First().BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name));

            foreach (ResourceSpreadDto aDto in dtosToSave)
            {
                aDto.LaborSpreadDate = aDto.LaborSpreadDate.Normalize();

                this.Log.Debug(string.Format("ResourceSpreadLoader.BulkSave => Item Id: {0}, Value: {1}", aDto.Id, aDto.LaborSpreadValue)); 
            }

            return base.BulkSave(dtosToSave);
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for BoeTaskElementDTOs
        /// </summary>
        /// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertBOELaborSpreadviaTableParameter";

            // only insert is performed using the labor spread entity.
            // deletion is performed using the laborTypeID which unfortunately is using a diff entity so that has
            // to be done manually within the overridden BulkDelete
            metaData.BulkUpdateStoredProcedureName = null;
            metaData.BulkDeleteStoredProcedureName = "deleteBOELaborSpreadByBOELaborTypeIDviaTableParameter"; 

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_BOELaborSpread";
            metaData.StoredProcedureTableTypeParameterName = "@BOELaborSpread";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "BOELaborSpreadID", "BOELaborTypeID", "LaborSpreadDate", "LaborSpreadValue"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the Task Element DTO into a BOETaskElement entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override BOELaborSpread ConvertDtoToEntity(ResourceSpreadDto dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            BOELaborSpread entity = null;

            entity = new BOELaborSpread()
            {
                BOELaborSpreadID = dtoToConvert.Id,
                LaborSpreadDate = dtoToConvert.LaborSpreadDate,
                LaborSpreadValue = dtoToConvert.LaborSpreadValue,
                BOELaborTypeID = dtoToConvert.LaborTypeId
            };

            return entity;
        }
    }
}
