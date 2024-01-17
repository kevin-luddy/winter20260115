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
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using IES.Common.Core;
	using Microsoft.Extensions.Logging;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Constants;

	/// <summary>
	/// RDSB Rate Code Xref Loader
	/// </summary>
	public class RdsbRateCodeXrefLoader : BulkDataLoader<RdsbRateCodeXrefModelView, RDSBRateCodeXref>, IRdsbRateCodeXrefLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public RdsbRateCodeXrefLoader(ILogger<RdsbRateCodeXrefLoader> logger) : base(logger)
		{ }

		/// <summary>
		/// Gets all objects based on the IDs that were passed in
		/// </summary>
		/// <param name="ids">IDs</param>
		/// <returns>
		/// Corresponding Data
		/// </returns>
		public override ICollection<RdsbRateCodeXrefModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">DTO to upsert</param>
        /// <returns>Id of the DTO after the modification</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Upsert(RdsbRateCodeXrefModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">DTO to delete</param>
        /// <returns>Id of the deleted DTO</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Delete(RdsbRateCodeXrefModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for RDSBRateCodeXref
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new(CommonConstants.IES_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertRDSBRateCodeXrefviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = string.Empty; // not implemented
            metaData.BulkDeleteStoredProcedureName = string.Empty; // not implemented

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_RDSBRateCodeXref";
            metaData.StoredProcedureTableTypeParameterName = "@RDSBRateCodeXref";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "ID", "UpdateDate", "RDSBDocumentInformationID", "RateCodeID"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override RDSBRateCodeXref ConvertDtoToEntity(RdsbRateCodeXrefModelView dtoToConvert)
        {
            RDSBRateCodeXref entity = new();

            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            entity.ID = dtoToConvert.Id;
            entity.UpdateDate = dtoToConvert.UpdateDate;
            entity.RateCodeID = dtoToConvert.RateCodeId;
            entity.RDSBDocumentInformationID = dtoToConvert.RdsbDocumentInformationId;

            return entity;
        }
    }
}
