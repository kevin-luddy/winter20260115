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
	using IES.Standard;
	using IES.DataBridge.ModelViews;
	using IES.Models;

	/// <summary>
	/// Rate Grid Loader
	/// </summary>
	public class RateCodeYearLoader : BulkDataLoader<RateYearModelView, RateCodeYear>, IRateCodeYearLoader
    {
        #region Retrieves

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<RateYearModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Upsert(RateYearModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Delete(RateYearModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for RateCodeYear.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.IES_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertRateCodeYearviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateRateCodeYearviaTableParameter";
            // only insert & update are performed using the RateCodeYear entity.
            // deletion is performed using the RateCode entity.

            metaData.BulkDeleteStoredProcedureName = null;

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_RateCodeYear";
            metaData.StoredProcedureTableTypeParameterName = "@RateCodeYear";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "ID", "UpdateDate", "RateCodeID", "Year", "Rate"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override RateCodeYear ConvertDtoToEntity(RateYearModelView dtoToConvert)
        {
            RateCodeYear entity = new RateCodeYear();

            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            entity.ID = dtoToConvert.Id;
            entity.UpdateDate = dtoToConvert.UpdateDate;
            entity.RateCodeID = dtoToConvert.RateCodeId;
            entity.Year = dtoToConvert.Year;
            entity.Rate = dtoToConvert.Value;

            return entity;
        }
        #endregion
    }
}