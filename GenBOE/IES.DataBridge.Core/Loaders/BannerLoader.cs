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
	/// Banner Loader
	/// </summary>
    public class BannerLoader : DataLoader<BannerModelView>, IBannerLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public BannerLoader(ILogger<BannerLoader> logger) : base(logger)
		{ }

		#region Retrieves

		/// <summary>
		/// Get All Banners.
		/// </summary>
		/// <returns>All Banners.</returns>
		public ICollection<BannerModelView> GetAll()
        {
            ICollection<BannerModelView> result;

            using (StopwatchTimer sw = new(this.Log))
            {
                using (IESEntities context = new())
                {
                    result = context.Banners.Select(e =>
                        new BannerModelView()
                        {
                            Id = e.ID,
                            UpdateDate = e.UpdateDate,
                            BannerText = e.BannerText,
                            HoursToShow = e.HoursToShow,
                            SelectedAppsAsString = e.SelectedApps,
                            StartDate = e.StartDate,
                            TurnOffTicker = e.TurnOffTicker ?? false
                        }).OrderByDescending(x => x.StartDate).ToList();
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
        public override ICollection<BannerModelView> GetByIds(ICollection<int> ids)
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
        protected override int? Upsert(BannerModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;
            
            using (IESEntities iesEntities = new())
            {
                result = iesEntities.upsertBanner(dtoToUpsert.Id, dtoToUpsert.UpdateDate, string.Join(",", dtoToUpsert.SelectedApps), dtoToUpsert.StartDate, dtoToUpsert.HoursToShow, dtoToUpsert.BannerText, dtoToUpsert.TurnOffTicker).First();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(BannerModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn;

            using (IESEntities iesEntities = new())
            {
                toReturn = iesEntities.deleteBanner(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        #endregion
    }
} 