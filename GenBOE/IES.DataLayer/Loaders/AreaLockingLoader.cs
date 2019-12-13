// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Configuration;
    using IES.Common;

    /// <summary>
    /// Loader for Area Locking
    /// </summary>
    public class AreaLockingLoader : IAreaLockingLoader
    {
        /// <summary>
        /// Cache Object
        /// </summary>
        private MemoryCache cache;

        /// <summary>
        /// Number of seconds to store lock in cache
        /// </summary>
        private int secondsToCache = Convert.ToInt32(WebConfigurationManager.AppSettings["EditLockTimeoutExpirationMinutes"]) * 60;

        /// <summary>
        /// key for the cache
        /// </summary>
        private string cacheKey = "AreaLock_";

        /// <summary>
        /// An object for locking items while modifying cache
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public AreaLockingLoader()
        {
            this.cache = new MemoryCache();
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

                this.cache.AddAbsolute(key, data, this.secondsToCache);
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
                    if (area != IES.Common.LockArea.None)
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
