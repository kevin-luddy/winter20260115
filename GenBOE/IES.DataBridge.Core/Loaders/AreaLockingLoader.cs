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
	using System.Configuration;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using Microsoft.Extensions.Configuration;

	/// <summary>
	/// Loader for Area Locking
	/// </summary>
	public class AreaLockingLoader : IAreaLockingLoader
    {
		/// <summary>
		/// Configuration for appsettings.json.
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// Cache Object
		/// </summary>
		private readonly ICacheService cache;

        /// <summary>
        /// key for the cache
        /// </summary>
        private readonly string cacheKey = "AreaLock_";

        /// <summary>
        /// An object for locking items while modifying cache
        /// </summary>
        private static readonly object LockObject = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache">Cache</param>
        public AreaLockingLoader(ICacheService cacheservice, IConfiguration configuration)
        {
            this.cache = cacheservice;
			this.configuration = configuration;
        }

        /// <summary>
        /// Adds lock for area to cache - can add new lock or extend existing
        /// </summary>
        /// <param name="area">area to lock</param>
        /// <param name="data">Area Lock data</param>
        public void LockArea(LockArea area, AreaLockData data)
        {
            lock (LockObject)
            {
                // Remove any existing lock from cache
                this.UnlockArea(area);

                string key = this.cacheKey + area;

				int secondsToLock = Convert.ToInt32(this.configuration["EditLockTimeoutExpirationMinutes"]) * 60;

				this.cache.AddAbsolute(key, data, secondsToLock);
            }
        }

        /// <summary>
        /// Removes the lock for the area
        /// </summary>
        /// <param name="area">area with lock</param>
        public void UnlockArea(LockArea area)
        {
            lock (LockObject)
            {
                string key = this.cacheKey + area;

                this.cache.Remove(key);
            }
        }

        /// <summary>
        /// Gets the area lock if it exists
        /// </summary>
        /// <param name="area">area to get lock for</param>
        /// <returns>lock data if it exists, null if not</returns>
        public AreaLockData GetAreaLock(LockArea area)
        {
            object fromCache;
            lock (LockObject)
            {
                string key = this.cacheKey + area;

                fromCache = this.cache.GetData(key);
            }

            return fromCache as AreaLockData;
        }

        /// <summary>
        /// Gets all active locks not owned by the active user
        /// </summary>
        /// <param name="activeUser">User data for the Active User</param>
        /// <returns>Any locks that are active but not owned by the active user</returns>
        public ICollection<AreaLockData> GetActiveLocksByOtherUsers(UserData activeUser)
        {
            if (activeUser == null)
            {
                throw new ArgumentNullException(nameof(activeUser));
            }

            ICollection<AreaLockData> toReturn = new Collection<AreaLockData>();

            lock (LockObject)
            {
                foreach (LockArea area in Enum.GetValues(typeof(LockArea)))
                {
                    if (area != IES.Common.Core.Enums.LockArea.None)
                    {
                        AreaLockData areaLock = this.GetAreaLock(area);

                        if (areaLock != null && (areaLock.LockedBy.Ntid != activeUser.Ntid))
                        {
                            toReturn.Add(areaLock);
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}
