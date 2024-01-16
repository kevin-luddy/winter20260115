// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common.Core;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for Offline Application Loader
    /// </summary>
    public interface IOfflineApplicationLoader : IDataLoader<OfflineApplicationModelView>
    {
        /// <summary>
        /// Get all Offline Application data
        /// </summary>
        /// <returns>all Offline Application data</returns>
        ICollection<OfflineApplicationModelView> GetAll();

        /// <summary>
        /// Get data for specific application
        /// </summary>
        /// <param name="application">Application to get data for</param>
        /// <returns>Data for the specified application</returns>
        OfflineApplicationModelView GetByApplication(string application);

        /// <summary>
        /// Update Offline Application status
        /// </summary>
        /// <param name="dtoToUpdate">dto to update</param>
        void Update(OfflineApplicationModelView dtoToUpdate);
    }
}
