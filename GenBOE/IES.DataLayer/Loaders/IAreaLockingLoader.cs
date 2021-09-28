// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// Interface for the Area Locking Loader
    /// </summary>
    public interface IAreaLockingLoader
    {
        /// <summary>
        /// Adds lock for area to cache - can add new lock or extend existing
        /// </summary>
        /// <param name="area">area to lock</param>
        /// <param name="data">Area Lock data</param>
        void LockArea(LockArea area, AreaLockData data);

        /// <summary>
        /// Removes the lock for the area
        /// </summary>
        /// <param name="area">area with lock</param>
        void UnlockArea(LockArea area);

        /// <summary>
        /// Gets the area lock if it exists
        /// </summary>
        /// <param name="area">area to get lock for</param>
        /// <returns>lock data if it exists, null if not</returns>
        AreaLockData GetAreaLock(LockArea area);

        /// <summary>
        /// Gets all active locks not owned by the active user
        /// </summary>
        /// <param name="activeUser">User data for the Active User</param>
        /// <returns>Any locks that are active but not owned by the active user</returns>
        ICollection<AreaLockData> GetActiveLocksByOtherUsers(UserData activeUser);
    }
}
