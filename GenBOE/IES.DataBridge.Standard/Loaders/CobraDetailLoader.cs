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
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using IES.Standard;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Cobra Grid Loader
	/// </summary>
	public class CobraDetailLoader : BulkDataLoader<CobraDetailModelView, RateCode>, ICobraDetailLoader
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CobraDetailLoader"/> class.
        /// </summary>
        public CobraDetailLoader(ILogger<CobraDetailLoader> logger) : base(logger)
		{
		}

        #endregion

        #region Retrieves

        /// <summary>
        /// Get COBRA Details by Revision.
        /// </summary>
        /// <param name="revision">Revision to retrieve.</param>
        /// <returns>All COBRA Mapping Details for the given revision.</returns>
        public ICollection<CobraDetailModelView> GetCobraDetailsByRevision(RevisionModelView revision)
        {
            ICollection<CobraDetailModelView> cobraDetails;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    // Grab all the Cobra Mapping Details for this revision.
                    cobraDetails = context.RateCodes.Where(x => x.RevisionID == revision.Id)
                    .OrderBy(y => y.CategoryID)
                    .Select(r =>
                    new CobraDetailModelView()
                    {
                        Description = r.Description,
                        Id = r.ID,
                        RateCategoryDescription = r.CategoryLU.Description,
                        RateCode = r.RateCode1,                     // Entity Framework adds 1 to avoid name collision.
                        RevisionId = r.RevisionID,
                        RateSet = r.CobraRateSet,
                        Code1 = r.CobraCode1ID == null ? Code1.NA : (Code1)r.CobraCode1ID,
                        UpdateDate = r.UpdateDate
                    }).ToList();
                }
            }

            return cobraDetails;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<CobraDetailModelView> GetByIds(ICollection<int> ids)
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
        protected override int? Upsert(CobraDetailModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        protected override int? Delete(CobraDetailModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for Cobra Mapping Details
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        public override BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.IES_DB_CONTEXT_NAME);

            metaData.BulkDeleteStoredProcedureName = string.Empty;  // Not implemented
            metaData.BulkInsertStoredProcedureName = string.Empty;  // Not implemented
            metaData.BulkUpdateStoredProcedureName = "updateCobraDetailviaTableParameter";

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = false;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

            metaData.DBTableTypeName = "TT_CobraDetail";
            metaData.StoredProcedureTableTypeParameterName = "@CobraDetailParam";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "ID", "UpdateDate", "CobraRateSet", "CobraCode1ID"
            };

            return metaData;
        }

        /// <summary>
        /// Bulk save Cobra Mapping Details.  (Only supports Upsert operations.)
        /// </summary>
        /// <param name="dtosToSave">dtos to save</param>
        /// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
        public override IDictionary<int, int> BulkSave(ICollection<CobraDetailModelView> dtosToSave)
        {
            IDictionary<int, int> toReturn = new Dictionary<int, int>();

            if (dtosToSave == null)
            {
                throw new ArgumentNullException(nameof(dtosToSave));
            }

            if (dtosToSave.Any(d => d.Updateable == UpdateType.None || d.Updateable == UpdateType.Deleted))
            {
                throw new ArgumentException("One or more COBRA Details has UpdateType of None or Deleted.", nameof(dtosToSave));
            }

            if (dtosToSave.Any())
            {
                this.Log.LogDebug(string.Format("CobraDetailLoader.BulkSave => Ntid: {2}, RateCodeId: {1}, Count: {0}",
                dtosToSave.Count, dtosToSave.First().Id, System.Threading.Thread.CurrentPrincipal.Identity.Name));

                foreach (CobraDetailModelView dto in dtosToSave)
                {
                    this.Log.LogDebug(string.Format("CobraDetailLoader.BulkSave => Item Id: {0}, Value: {1}", dto.Id, dto.RateCode));
                }

                using (StopwatchTimer sw = new StopwatchTimer(this.Log))
                {
                    // Set the Code1 values to null if the selected value is empty (not set)
                    foreach (CobraDetailModelView cobraDetailModelView in dtosToSave)
                    {
                        cobraDetailModelView.Code1 = cobraDetailModelView.Code1.HasValue && cobraDetailModelView.Code1.Value == Code1.NA ? null : cobraDetailModelView.Code1;
                    }

                    // Bulk save Cobra Details
                    toReturn = base.BulkSave(dtosToSave);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Converts the DTO into an entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override RateCode ConvertDtoToEntity(CobraDetailModelView dtoToConvert)
        {
            RateCode entity = new RateCode();

            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            entity.ID = dtoToConvert.Id;
            entity.UpdateDate = dtoToConvert.UpdateDate;
            entity.RevisionID = dtoToConvert.RevisionId;
            entity.CobraRateSet = dtoToConvert.RateSet;
            entity.CobraCode1ID = (int?)dtoToConvert.Code1;
            entity.RateCode1 = dtoToConvert.RateCode;

            return entity;
        }

        /// <summary>
        /// Save CobraDetailModelView to database and reset Dirty flags.
        /// </summary>
        /// <param name="dirtyCobraDetails">The collection of CobraDetailModelView with changes.</param>
        public void SaveDetails(ICollection<CobraDetailModelView> dirtyCobraDetails)
        {
            if (dirtyCobraDetails == null)
            {
                throw new ArgumentNullException(nameof(dirtyCobraDetails));
            }

            // Save all the Dirty CobraDetails to the database.
            foreach (CobraDetailModelView cdmv in dirtyCobraDetails)
            {
                if (cdmv.Dirty)
                {
                    // Only supporting updates at this time
                    cdmv.Updateable = UpdateType.Upsert;
                }
            }

            // Using Bulk Save
            this.BulkSave(dirtyCobraDetails);
        }

        #endregion
    }
}