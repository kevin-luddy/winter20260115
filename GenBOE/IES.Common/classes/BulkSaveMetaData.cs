// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Encapsulates the metadata required to allow the common DataLoader.BulkSave to call the 
    /// bulk insert, bulk delete and bulk update stored procedures for the TEntityType. 
    /// The metadata consists of:
    /// 
    /// - names of the bulk insert, bulk delete and bulk update stored procedures.
    /// - name of the table valued parameter (should be the same name used by each stored procedure)
    /// - name of the table type defined in the database (commonly prefixed with "TT_").
    /// - names of the entity properties to include in the DataTable being passed to the bulk stored procedure. Order matters.
    /// - 2 boolean properties to indicate whether an updateDate is returned in the bulk stored procedure result set. 
    ///    One property for the insert and update stored procedures. Delete does not return anything.
    /// 
    /// All bulk insert, delete and update stored procedures require a single DataTable argument. 
    /// The DataTable contains all the entity data that need to be processed. The DataTable structure has the same columns
    /// in the same order as the user defined table in the database. The order MUST MATCH.
    /// 
    /// The common BulkSave takes care of converting the dtos to entities and then populating the DataTable argument that is passed into the stored procedures.
    /// 
    /// Here is the common pattern for all bulk stored procedures.
    /// 
    /// - Bulk Delete stored procedure does not return any data.
    /// 
    /// - Bulk Update stored procedure always returns a result set consisting of the new id and the new updateDate for each entity that was processed 
    ///   in the same order as the data in the DataTable that was passed in.
    /// 
    /// - If BulkUpdate is defined, the Bulk Insert stored procedure will always return a result set consisting of the new id and new updateDate for each entity 
    ///   that was processed. 
    /// 
    /// The result set has a common structure: column 1 holds the elements id (new or original) and column 2 holds the new update date (if returned, otherwise null0.
    /// 
    /// A loader does not always support all 3 bulk stored procedures. Sometimes a loader may only support insert and delete. 
    /// Developers can indicate this situation by setting the BulkUpdateStoredProcedureName to string.Empty. 
    /// This means that the set of data is always replaced (existing is deleted, and new data inserted). 
    /// 
    /// In this case, an updatedate is not necessary and is not returned in the result set. The common BulkSave code needs a way to know when it is included and
    /// when it is not. This is indicated by the BulkUpdateStoredProcedureReturnsUpdateDate and BulkInsertStoredProcedureReturnsUpdateDate properties. DElete does not
    /// have one since it doesn't return a result set.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public class BulkSaveMetaData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dbContextName">Name of the database context.</param>
        public BulkSaveMetaData(string dbContextName)
        {
            this.DbContextName = dbContextName;
        }

        /// <summary>
        /// Table Type defined in the database that corresponds to the DataTable object being created in code.
        /// This name must exactly match the name of the user defined table type defined in the database.
        /// </summary>
        public string DBTableTypeName { get; set; }

        /// <summary>
        /// Argument name of the table type in the bulk save stored procedures. The argument name should be the same name
        /// in each stored procedures (insert, delete, update). This must include the '@' in the name. For an example, 
        /// look at BoeTaskElementDtoDataLoader.CreateBulkSaveMetaData.
        /// </summary>
        public string StoredProcedureTableTypeParameterName { get; set; }

        /// <summary>
        /// Gets or sets the BulkInsertStoredProcedureName. 
        /// </summary>
        public string BulkInsertStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the BulkDeleteStoredProcedureName. If the loader does not support an explicit delete of the TEntityType, set
        /// this name to string.Empty
        /// </summary>
        public string BulkDeleteStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets the BulkUpdateStoredProcedureName. If the loader does not support update for the TEntityType, set
        /// this name to string.Empty. And set the BulkInsertStoredProcedureReturnsUpdateDate to false
        /// because it means the loader can deletes all the dtos and then insert all new ones thus negating the need for an update date.
        /// </summary>
        public string BulkUpdateStoredProcedureName { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if the Bulk Insert returns an update date in the result set. 
        /// </summary>
        public bool BulkInsertStoredProcedureReturnsUpdateDate { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if the Bulk Update returns an update date in the result set.
        /// </summary>
        public bool BulkUpdateStoredProcedureReturnsUpdateDate { get; set; }

        /// <summary>
        /// Names of the entity properties to be included in the DataTable.
        /// 
        /// NOTE: The order of the property names in this collection MUST match the order of the columns in the Table Type in the database.
        /// If they are not, you will see an SQL exception of the form "Operand type clash:...." which indicates the columns are not lining up between
        /// the DataTable and the table type.
        /// </summary>
        public ICollection<string> EntityPropertiesToMapToDataTable { get; set; }

        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        public string DbContextName { get; private set; }
    }
}
