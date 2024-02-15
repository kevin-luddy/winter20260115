// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Loaders
{
	using System.Collections.Generic;
	using IES.Common.Core.Interfaces;

	/// <summary>
	/// Interface for the base class DataLoader
	/// </summary>
	public interface IBulkDataLoader<TDtoType> : IDataLoader<TDtoType>
		where TDtoType : IUpdateableDTO
	{
		/// <summary>        
		/// Performs a bulk insert, delete, and/or update of the dtos in the collection based on the UpdateableDto.Updateable and UpdateableDto.Id values.
		/// 
		/// Preconditions:
		/// - The child loader must have implemented the following methods:
		///      - CreateBulkSaveMetaData TEntityType: provides the data necessary for the common BulkSave to perform bulk delets, inserts, and/or updates.
		///      - ConvertDtoToEntity: supports the creation of entities to populate the DataTable needed by the bulk stored procedure.
		/// - The TDtoType of the loader must implement the following methods:
		///      - PropagateNewParentIdToChildDTOs: a dto may have one or more collections of 'child' dtos that have a property identifying it. This method allows the
		///        common BulkInsert to update those child DTOs with the dtos new id so they are ready for BulkInsert themselves. If there is nothing to update, create
		///        the method as a noop
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
		/// 
		/// </summary>
		/// <param name="dtosToSave">the set of dtos to bulk save. These include all deletable, insertable, and updateable dtos.</param>
		/// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
		IDictionary<int, int> BulkSave(ICollection<TDtoType> dtosToSave);

		/// <summary>
		/// Provides the metadata to support bulk save processing.
		/// </summary>
		/// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		/// <returns>Meta data required for bulk save processing</returns>
		BulkSaveMetaData CreateBulkSaveMetaData();
	}
}
