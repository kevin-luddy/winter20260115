// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Home Model View
    /// </summary>
    public class WhosOnlineModelView
    {
        /// <summary>
        /// Public Constructor
        /// </summary>
        public WhosOnlineModelView()
        {
            this.NumberOfUsers = 0;
        }

        /// <summary>
        /// Number of registered users
        /// </summary>
        public int NumberOfUsers { get; set; }

        /// <summary>
        /// Dictionary that keeps track of users logged into system.  Keyed by ntid, has stats
        /// on the user in the system.
        /// </summary>
        public ICollection<UserDTO> UserDetails { get; set; }

        /// <summary>
        /// The number of minutes of a 'window' to create for logged in users
        /// </summary>
        public int MinutesToExpireUsers { get; set; }

        /// <summary>
        /// Last time the Cache was Refreshed
        /// </summary>
        public string LastCacheRefresh { get; set; }
    }
}