// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Loaders;
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// RDSB Section Xref Loader
	/// </summary>
	public class RdsbSectionXrefLoader : BulkDataLoader<RdsbSectionXrefModelView, RDSBSectionXref>, IRdsbSectionXrefLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public RdsbSectionXrefLoader(ILogger<RdsbSectionXrefLoader> logger) : base(logger)
		{ }

		/// <summary>
		/// Gets all objects based on the IDs that were passed in
		/// </summary>
		/// <param name="ids">IDs</param>
		/// <returns>
		/// Corresponding Data
		/// </returns>
		public override ICollection<RdsbSectionXrefModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">DTO to upsert</param>
        /// <returns>Id of the DTO after the modification</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Upsert(RdsbSectionXrefModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">DTO to delete</param>
        /// <returns>Id of the deleted DTO</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Delete(RdsbSectionXrefModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for RDSBSectionXref
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(CommonConstants.IES_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertRDSBSectionXrefviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = string.Empty; // not implemented
            metaData.BulkDeleteStoredProcedureName = string.Empty; // not implemented

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_RDSBSectionXref";
            metaData.StoredProcedureTableTypeParameterName = "@RDSBSectionXref";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "ID", "UpdateDate", "RDSBDocumentInformationID", "SectionID"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override RDSBSectionXref ConvertDtoToEntity(RdsbSectionXrefModelView dtoToConvert)
        {
            RDSBSectionXref entity = new RDSBSectionXref();

            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            entity.ID = dtoToConvert.Id;
            entity.UpdateDate = dtoToConvert.UpdateDate;
            entity.SectionID = dtoToConvert.SectionId;
            entity.RDSBDocumentInformationID = dtoToConvert.RdsbDocumentInformationId;

            return entity;
        }
    }
}
