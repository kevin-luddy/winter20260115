// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The descriptor for the online users.
    /// </summary>
    [Serializable]
    public class UsersOnlineDetailDescriptor
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public UsersOnlineDetailDescriptor()
        {
            this.LastCacheRefresh = DateTime.Now;
        }

        /// <summary>
        /// The number of minutes of a 'window' to create for logged in users
        /// </summary>
        public int MinutesToExpireUsers { get; set; }

        /// <summary>
        /// The peak number of users online at one time since a cache clear
        /// </summary>
        public int PeakUsersSinceCacheClear { get; set; }

        /// <summary>
        /// Collection of user information for user that have been online in the last X minutes
        /// </summary>
        public ICollection<UserDTO> UserOnlineDetailsCollection { get; set; }

        /// <summary>
        /// The last time cache was refreshed
        /// </summary>
        public DateTime LastCacheRefresh { get; protected set; }

        /// <summary>
        /// The last cache refresh display.
        /// </summary>
        public string LastCacheRefreshDisplay
        {
            get
            {
                // see http://msdn.microsoft.com/en-us/library/az4se3k1.aspx for format strings for date
                // "g" is 6/15/2009 1:45:30 PM -> 6/15/2009 1:45 PM (en-US)
                return this.LastCacheRefresh.ToString("g");
            }
        }
    }
}