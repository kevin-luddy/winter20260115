// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Who's Online Loader
    /// </summary>
    public interface IWhosOnlineLoader
    {
        /// <summary>
        /// Updates the database with the latest access time of the current user.
        /// </summary>
        /// <param name="userData">User Data</param>
        /// <param name="applicationName">Name of the application the user is accessing</param>
        void UpdateLastAccessTime(UserData userData, string applicationName);
        
        /// <summary>
        /// Gets Who's Online Data
        /// </summary>
        /// <param name="applicationName">Name of the application to get Who's Online for</param>
        /// <returns>Who's Online Data</returns>
        ICollection<WhosOnlineModelView> GetWhosOnlineData(string applicationName);
    }
}
