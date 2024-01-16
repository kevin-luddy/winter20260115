// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common.Core;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.ModelViews;
    using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Rate Code Replication Loader
	/// </summary>
    public class RateCodeReplicationLoader : DataLoader<RateCodeModelView>, IRateCodeReplicationLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public RateCodeReplicationLoader(ILogger<RateCodeReplicationLoader> logger) : base(logger)
		{ }

		#region Retrieves

		/// <summary>
		/// Gets all the Rate Code Replications.
		/// </summary>
		/// <returns>All of the Rate Code Replications from the Database.</returns>
		public ICollection<RateCodeModelView> GetAll()
        {
            ICollection<RateCodeModelView> result;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    result = context.RateCodeReplications.Select(e =>
                        new RateCodeModelView()
                        {
                            Id = e.ID,
                            UpdateDate = e.UpdateDate,
                            From = e.From,
                            To = e.To
                        }).OrderBy(x => x.From).ThenBy(x => x.To).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<RateCodeModelView> GetByIds(ICollection<int> ids)
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
        protected override int? Upsert(RateCodeModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;
            
            using (IESEntities iesEntities = new IESEntities())
            {
                result = iesEntities.upsertReplication(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.From, dtoToUpsert.To).First();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(RateCodeModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            using (IESEntities iesEntities = new IESEntities())
            {
                toReturn = iesEntities.deleteReplication(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        #endregion
    }
} 