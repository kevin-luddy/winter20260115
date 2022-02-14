// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Bulk data loader
    /// </summary>
    /// <typeparam name="TDtoType">Dto Data Type</typeparam>
    /// <typeparam name="TEntityType">Entity Data Type</typeparam>
    public abstract class BulkDataLoader<TDtoType, TEntityType> : DataLoader<TDtoType>, IBulkDataLoader<TDtoType>
        where TDtoType : IUpdateableDTO
    {
        /// <summary>
        /// Provides the metadata to support bulk save processing.
        /// </summary>
        /// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public abstract BulkSaveMetaData CreateBulkSaveMetaData();

        /// <summary>
        /// Converts the dto into it's associated TEntityType
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>TEntityType</returns>
        protected abstract TEntityType ConvertDtoToEntity(TDtoType dtoToConvert);

        /// <summary>
        /// Performs a bulk insert, delete, and/or update of the dtos in the collection.
        /// 
        /// Preconditions:
        /// - The child loader must have implemented the following methods:
        ///      - CreateBulkSaveMetaData TEntityType: provides the data necessary for the common BulkSave to perform bulk deletes, inserts, and/or updates.
        ///      - ConvertDtoToEntity: supports the creation of entities to populate the DataTable needed by the bulk stored procedure.
        /// - The TDtoType of the loader must implement the following methods:
        ///      - PropagateNewParentIdToChildDTOs: a dto may have one or more collections of 'child' dtos that have a property identifying it. This method allows the
        ///        common BulkInsert to update those child DTOs with the dtos new id so they are ready for BulkInsert themselves.
        /// 
        /// Postconditions:
        /// - Inserted dto: 
        ///      - UpdateableDto.Updateable is reset to None 
        ///      - UpdateableDto.Id is updated with the new id value
        ///      - the UpdateableDto.UpdateDate has the new update date
        ///      - The new UpdateableDto.Id is propagated to each dto in each child collection owned by this dto
        /// - Updated dto: 
        ///      - the UpdateableDto.Updateable is reset to None
        ///      - the UpdateableDto.UpdateDate has the new update date
        /// - Deleted dto: 
        ///      - the deleted dto is not changed - 
        ///      ********
        ///      WARNING: callers are responsible for removing deleted dtos from their collection after successful completion of this method if they wish to
        ///      ********  continue processing with that same list.
        /// 
        /// </summary>
        /// <param name="dtosToSave">the set of dtos to bulk save. These include all deletable, insertable, and updateable dtos.</param>
        /// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
        public virtual IDictionary<int, int> BulkSave(ICollection<TDtoType> dtosToSave)
        {
            if (dtosToSave == null)
            {
                throw new ArgumentNullException(nameof(dtosToSave));
            }

            this.ValidateTransactionScope();

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                // Don't bother if there is nothing to process
                if (dtosToSave.Any(d => d.Updateable != UpdateType.None))
                {
                    Dictionary<int, int> insertResults = null;
                    Dictionary<int, int> updateResults = null;

                    // Get the loader's bulk save meta data
                    BulkSaveMetaData metadata = this.CreateBulkSaveMetaData();

                    bool loaderSupportsBulkUpdate = !string.IsNullOrEmpty(metadata.BulkUpdateStoredProcedureName);
                    bool loaderSupportsBulkDelete = !string.IsNullOrEmpty(metadata.BulkDeleteStoredProcedureName);
                    bool loaderSupportsBulkInsert = !string.IsNullOrEmpty(metadata.BulkInsertStoredProcedureName);

                    // separate dtosToSave into delete, insert, update lists.
                    ICollection<TDtoType> deleteCollection = dtosToSave.Where(d => d.Updateable == UpdateType.Deleted).ToCollection();
                    ICollection<TDtoType> insertCollection = dtosToSave.Where(d => d.Updateable == UpdateType.Upsert && d.Id < 0).ToCollection();
                    ICollection<TDtoType> updateCollection = dtosToSave.Where(d => d.Updateable == UpdateType.Upsert && d.Id > 0).ToCollection();

                    if (dtosToSave.Any(d => d.Updateable == UpdateType.Upsert && d.Id == 0))
                    {
                        // this is an invalid condition
                        throw new ArgumentException("One or more upsertable dtos has an invalid id of 0.", nameof(dtosToSave));
                    }

                    // Most loaders support insert, delete and update but some only support insert and delete and rarer still some only support insert.
                    // Those that only support insert do so because the delete is handled when the owning dto is deleted.
                    // So if the data has insertable, deleteable or updateable dtos then the meta data better say the loader supports it.
                    // If not, we throw an exception cause there is a coding error.
                    if (!loaderSupportsBulkUpdate && updateCollection != null && updateCollection.Any())
                    {
                        throw new ArgumentException("Bulk Update is not supported for DTO Type of " + typeof(TDtoType).Name, nameof(dtosToSave));
                    }

                    if (!loaderSupportsBulkDelete && deleteCollection != null && deleteCollection.Any())
                    {
                        throw new ArgumentException("Bulk Delete is not supported for DTO Type of " + typeof(TDtoType).Name, nameof(dtosToSave));
                    }

                    if (!loaderSupportsBulkInsert && insertCollection != null && insertCollection.Any())
                    {
                        throw new ArgumentException("Bulk Insert is not supported for DTO Type of " + typeof(TDtoType).Name, nameof(dtosToSave));
                    }

                    if (updateCollection != null && updateCollection.Any() && loaderSupportsBulkUpdate)
                    {
                        updateResults = this.BulkUpdate(updateCollection, metadata);
                    }

                    // callers are responsible for removing deleted dtos from the input collection
                    if (deleteCollection != null && deleteCollection.Any() && loaderSupportsBulkDelete)
                    {
                        this.BulkDelete(deleteCollection, metadata);
                    }

                    if (insertCollection != null && insertCollection.Any() && loaderSupportsBulkInsert)
                    {
                        insertResults = this.BulkInsert(insertCollection, metadata);
                    }

                    // now combine insert and update results - note that delete never returns any results that we care about
                    if (updateResults != null && updateResults.Any())
                    {
                        toReturn = updateResults;
                    }

                    // there should never be any duplicate keys since the key is the primary key of the table
                    if (insertResults != null && insertResults.Any())
                    {
                        toReturn = toReturn.Concat(insertResults).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Performs a bulk insert on the collection.
        /// </summary>
        /// <param name="dtosToInsert">dtos to insert</param>
        /// <param name="metadata">Bulk save metadata.</param>
        /// <returns>dictionary where key = old id and value = new id</returns>
        protected virtual Dictionary<int, int> BulkInsert(ICollection<TDtoType> dtosToInsert, BulkSaveMetaData metadata)
        {
            if (dtosToInsert == null)
            {
                throw new ArgumentNullException(nameof(dtosToInsert), "The input argument for 'BulkInsert' cannot be null.");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Log.Debug("Bulk Insert starting.");

            ConcurrentDictionary<int, int> toReturn = new ConcurrentDictionary<int, int>();

            if (dtosToInsert.Any())
            {
                // We'll hold onto the original negative ids and return them in the dictionary
                Collection<int> origIds = new Collection<int>();
                int j = -1;
                foreach (TDtoType dto in dtosToInsert)
                {
                    origIds.Add(dto.Id);
                    dto.Id = j--;
                }

                this.Log.Debug("Starting entity conversion for BulkInsert.");
                ICollection<TEntityType> entitiesToInsert = dtosToInsert.Select(d => this.ConvertDtoToEntity(d)).ToCollection();

                this.Log.Debug("Starting conversion from entities to DataTable for BulkInsert.");
                DataTable dataTable = StoredProcedureHelper.ToDataTable<TEntityType>(entitiesToInsert, metadata.EntityPropertiesToMapToDataTable);

                using (DbContext objectContext = new DbContext(metadata.DbContextName))
                {
                    this.Log.Debug("Executing the Bulk Insert");
                    ICollection<KeyValuePair<int, DateTime?>> insertResult = null;

                    try
                    {
                        this.Log.Debug("Start Insert: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                        // Execute the bulk insert stored procedure and await result
                        insertResult = StoredProcedureHelper.ExecuteTableValueProcedure(
                            objectContext,
                            dataTable,
                            metadata.BulkInsertStoredProcedureName,
                            metadata.StoredProcedureTableTypeParameterName,
                            metadata.DBTableTypeName,
                            metadata.BulkInsertStoredProcedureReturnsUpdateDate);

                        this.Log.Debug("End Insert: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        this.Log.Error(ex, $"Executing {metadata.BulkInsertStoredProcedureName} failed for {metadata.DBTableTypeName}");
                        throw;
                    }

                    this.Log.Debug($"Updating {dtosToInsert.Count} child DTOs for new Ids.");

                    // iterate thru dtos and update with the new Id and new 'UpdateDate' 
                    // also update the task element id in each 'child'
                    Parallel.For(0, dtosToInsert.Count, i =>
                    {
                        try
                        {
                            IUpdateableDTO dto = dtosToInsert.ElementAt(i);
                            int originalId = origIds.ElementAt(i);
                            KeyValuePair<int, DateTime?> kvp = insertResult.ElementAt(i);

                            // now update the child DTOs in each collection to use the new parent Dto id
                            dto.Update(kvp.Key, kvp.Value);

                            toReturn.TryAdd(originalId, kvp.Key);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Update failed for item {i} with message {ex.Message}, originalId {origIds.ElementAt(i)}, key {dtosToInsert.ElementAt(i)?.Id}.");
                            throw;
                        }
                    });

                    this.Log.Debug("Child DTO updates complete.");
                }
            }

            Log.Debug("Bulk Insert complete.");

            return toReturn.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Performs a bulk update on the collection.
        /// </summary>
        /// <param name="dtosToUpdate">dtos to update</param>
        /// <param name="metadata">Bulk save metadata.</param>
        /// <returns>Dictionary where key = old id and value = new id</returns>
        protected virtual Dictionary<int, int> BulkUpdate(ICollection<TDtoType> dtosToUpdate, BulkSaveMetaData metadata)
        {
            if (dtosToUpdate == null)
            {
                throw new ArgumentNullException(nameof(dtosToUpdate), "The argument for 'BulkUpdate' cannot be null.");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Log.Debug("Executing Bulk Update.");

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            if (dtosToUpdate.Any())
            {
                // Get the loader bulk save meta data
                ICollection<TEntityType> entitiesToUpdate = dtosToUpdate.Select(d => this.ConvertDtoToEntity(d)).ToCollection();
                DataTable dataTable = StoredProcedureHelper.ToDataTable<TEntityType>(entitiesToUpdate, metadata.EntityPropertiesToMapToDataTable);

                using (DbContext objectContext = new DbContext(metadata.DbContextName))
                {
                    ICollection<KeyValuePair<int, DateTime?>> updateResult = StoredProcedureHelper.ExecuteTableValueProcedure(
                        objectContext,
                        dataTable,
                        metadata.BulkUpdateStoredProcedureName,
                        metadata.StoredProcedureTableTypeParameterName,
                        metadata.DBTableTypeName,
                        metadata.BulkUpdateStoredProcedureReturnsUpdateDate);

                    // iterate thru dtos and update 'UpdateDate' with new value
                    for (int i = 0; i < dtosToUpdate.Count; i++)
                    {
                        IUpdateableDTO dto = dtosToUpdate.ElementAt(i);

                        if (updateResult.Any())
                        {
                            KeyValuePair<int, DateTime?> kvp = updateResult.ElementAt(i);
                            if (dto.Id == kvp.Key)
                            {
                                // most if not all bulk update stored procedures will return an update date.
                                // But there are some that don't so we have to support that.
                                if (metadata.BulkUpdateStoredProcedureReturnsUpdateDate)
                                {
                                    dto.UpdateDate = kvp.Value.Value;
                                }

                                // Now update the child DTOs in each collection to use the parent Dto id. This is required
                                // because some of the child DTOs may be new and we need to maintain referential integrity.
                                dto.Update(kvp.Key, kvp.Value);

                                // Reset updateable to none now that the dto has been successfully updated
                                dto.Updateable = UpdateType.None;

                                toReturn.Add(dto.Id, kvp.Key);
                            }
                        }
                    }
                }
            }

            Log.Debug("Bulk Update Complete.");

            return toReturn;
        }

        /// <summary>
        /// Performs a bulk delete on the collection. All dtos marked as 'Deleted' remain marked as deleted. It is the responsibility of the caller
        /// to remove the deleted items from the dtosToDelete collection after successful completion of this method.
        /// </summary>
        /// <param name="dtosToDelete">Dtos to delete</param>
        /// <param name="metadata">Bulk save metadata.</param>
        protected virtual void BulkDelete(ICollection<TDtoType> dtosToDelete, BulkSaveMetaData metadata)
        {
            if (dtosToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtosToDelete), "The argument to 'BulkDelete' cannot be null.");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (dtosToDelete.Any())
            {
                ICollection<TEntityType> entitiesToDelete = dtosToDelete.Select(d => this.ConvertDtoToEntity(d)).ToCollection();

                DataTable dataTable = StoredProcedureHelper.ToDataTable<TEntityType>(entitiesToDelete, metadata.EntityPropertiesToMapToDataTable);

                using (DbContext objectContext = new DbContext(metadata.DbContextName))
                {
                    StoredProcedureHelper.ExecuteTableValueProcedure(
                        objectContext,
                        dataTable,
                        metadata.BulkDeleteStoredProcedureName,
                        metadata.StoredProcedureTableTypeParameterName,
                        metadata.DBTableTypeName,
                        false); // NOTE: bulk delete never returns any data thus no update date
                }
            }
        }
    }
}