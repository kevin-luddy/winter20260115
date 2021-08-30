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
    using GenBOE.Models;
    using IES.Common;

    public class ProjectMapSpreadLoader : BulkDataLoader<ProjectMapSpreadModelView, ProjectMapSpread>, IProjectMapSpreadLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectMapSpreadLoader()
        {
            this.Log = new Logger(typeof(ProjectMapSpreadLoader));
        }

        #region Multi DbQuery Methods

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override ICollection<ProjectMapSpreadModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Converts the dto into it's associated TEntityType.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>
        /// TEntityType
        /// </returns>
        protected override ProjectMapSpread ConvertDtoToEntity(ProjectMapSpreadModelView dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }
            
            return new ProjectMapSpread
            {
                ID = dtoToConvert.Id,
                ProjectMapId = dtoToConvert.ProjectMapId,
                SpreadDate = dtoToConvert.SpreadDate,
                SpreadValue = dtoToConvert.SpreadValue,
                WorkspaceId = dtoToConvert.WorkspaceId
            };
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for ProjectMapModelViews
        /// </summary>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertProjectMapSpreadviaTableParameter";
            
            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            // This is the table type defined in the database for the input arg to the delete, insert, and update stored procedures
            metaData.DBTableTypeName = "TT_ProjectMapSpread";

            // Name of the stored procedure argument for all 3 bulk stored procedures
            metaData.StoredProcedureTableTypeParameterName = "@ProjectMapSpread";

            // The order here matters. It must match exactly the order of the TT_ProjectMap in the database.
            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "WorkspaceId", "ProjectMapId", "SpreadDate", "SpreadValue"
            };

            return metaData;
        }

        /// <summary>
        /// Upsert method to be overridden by the derived class
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>
        /// Int representing the id of the upserted item.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override int? Upsert(ProjectMapSpreadModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete method to be overridden by the derived class.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override int? Delete(ProjectMapSpreadModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }
    }
}