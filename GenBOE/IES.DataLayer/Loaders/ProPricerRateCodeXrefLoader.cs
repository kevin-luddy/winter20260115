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
    using System.Linq;
    using IES.Common;
    using IES.DataBridge.ModelViews;
    using IES.Models;

    /// <summary>
    /// Rate Grid Loader
    /// </summary>
    public class ProPricerRateCodeXrefLoader : BulkDataLoader<ProPricerRateCodeXrefModelView, ProPricerRateCodeXref>, IProPricerRateCodeXrefLoader
    {
        #region Retrieves

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<ProPricerRateCodeXrefModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get ProPricer Rate Code Xrefs for Rate Code ID
        /// </summary>
        /// <param name="rateCodeId">Rate Code ID</param>
        /// <returns>ProPricer Rate Code Xrefs for Rate Code ID</returns>
        public ICollection<ProPricerRateCodeXrefModelView> GetByRateCodeId(int rateCodeId)
        {
            ICollection<ProPricerRateCodeXrefModelView> toReturn = new Collection<ProPricerRateCodeXrefModelView>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    toReturn = context.ProPricerRateCodeXrefs.Where(x => x.RateCodeID == rateCodeId).OrderByDescending(x => x.UpdateDate)
                        .Select(p => new ProPricerRateCodeXrefModelView()
                    {
                        Id = p.ID,
                        RateCodeId = p.RateCodeID,
                        Description = p.Description,
                        UpdateDate = p.UpdateDate,
                        RateCodeExtensionId = p.RateCodeExtensionID,
                        ResourceClassId = p.ResourceClassID ?? 0,  // special case because of angular dropdowns such that a zero comes back instead of null
                        ResourceClass = p.ResourceClassLU == null ? string.Empty : p.ResourceClassLU.Description
                    }).ToCollection();
                }
            }

            return toReturn;
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Upsert(ProPricerRateCodeXrefModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Delete(ProPricerRateCodeXrefModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for ProPricerRateCodeXref
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.IES_DB_CONTEXT_NAME);

            metaData.BulkInsertStoredProcedureName = "insertProPricerRateCodeXrefviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateProPricerRateCodeXrefviaTableParameter";
            metaData.BulkDeleteStoredProcedureName = "deleteProPricerRateCodeXrefviaTableParameter";

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = false;

            metaData.DBTableTypeName = "TT_ProPricerRateCodeXref";
            metaData.StoredProcedureTableTypeParameterName = "@ProPricerRateCodeXref";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "ID", "UpdateDate", "RateCodeID", "Description", "RateCodeExtensionID", "ResourceClassID"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override ProPricerRateCodeXref ConvertDtoToEntity(ProPricerRateCodeXrefModelView dtoToConvert)
        {
            ProPricerRateCodeXref entity = new ProPricerRateCodeXref();

            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            entity.ID = dtoToConvert.Id;
            entity.UpdateDate = dtoToConvert.UpdateDate;
            entity.RateCodeID = dtoToConvert.RateCodeId;
            entity.Description = dtoToConvert.Description;
            entity.RateCodeExtensionID = dtoToConvert.RateCodeExtensionId;
            if (dtoToConvert.ResourceClassId == 0)
            {
                // this is a special case because of angular dropdowns such that a zero comes back instead of null
                dtoToConvert.ResourceClass = null;
            }

            entity.ResourceClassID = dtoToConvert.ResourceClassId;

            return entity;
        }
        #endregion
    }
}