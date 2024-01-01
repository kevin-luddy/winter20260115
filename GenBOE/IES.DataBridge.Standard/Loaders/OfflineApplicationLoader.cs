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
    using IES.Standard;
    using IES.DataBridge.ModelViews;
    using IES.Models;

    /// <summary>
    /// Offline Application Loader
    /// </summary>
    public class OfflineApplicationLoader : DataLoader<OfflineApplicationModelView>, IOfflineApplicationLoader
    {
        #region Retrieves

        /// <summary>
        /// Get all Offline Application data
        /// </summary>
        /// <returns>all Offline Application data</returns>
        public ICollection<OfflineApplicationModelView> GetAll()
        {
            ICollection<OfflineApplicationModelView> result;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    result = iesEntities.OfflineApplications.Select(o =>
                        new OfflineApplicationModelView()
                        {
                            ApplicationName = o.ApplicationName,
                            IsOffline = o.IsOffline,
                            UpdateDate = o.UpdateDate
                        }).ToCollection();
                }
            }

            return result;
        }
        
        /// <summary>
        /// Get data for specific application
        /// </summary>
        /// <param name="application">Application to get data for</param>
        /// <returns>Data for the specified application</returns>
        public OfflineApplicationModelView GetByApplication(string application)
        {
            OfflineApplicationModelView result;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    result = iesEntities.OfflineApplications
                        .Where(a => a.ApplicationName == application)
                        .Select(o =>
                        new OfflineApplicationModelView()
                        {
                            ApplicationName = o.ApplicationName,
                            IsOffline = o.IsOffline,
                            UpdateDate = o.UpdateDate
                        }).FirstOrDefault();
                }
            }

            return result;
        }

        /// <summary>
        /// unused
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<OfflineApplicationModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Commits

        /// <summary>
        /// Update Offline Application status
        /// </summary>
        /// <param name="dtoToUpdate">dto to update</param>
        public void Update (OfflineApplicationModelView dtoToUpdate)
        {
            if (dtoToUpdate == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpdate));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    iesEntities.updateOfflineApplication(dtoToUpdate.ApplicationName, dtoToUpdate.IsOffline, dtoToUpdate.UpdateDate);
                }
            }
        }

        /// <summary>
        /// unused
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>id of upserted dto</returns>
        protected override int? Upsert(OfflineApplicationModelView dtoToUpsert)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// unused
        /// </summary>
        /// <param name="dtoToDelete">dto to delete</param>
        /// <returns>id of deleted dto</returns>
        protected override int? Delete(OfflineApplicationModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
