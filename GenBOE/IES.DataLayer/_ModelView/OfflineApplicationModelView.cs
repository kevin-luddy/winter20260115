// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;

    /// <summary>
    /// ModelView for Manage Offline Applications
    /// </summary>
    public class OfflineApplicationModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OfflineApplicationModelView()
        {
        }

        /// <summary>
        /// Get/Set the application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Get/Set whether application is offline
        /// </summary>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Get/Set the update date
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}