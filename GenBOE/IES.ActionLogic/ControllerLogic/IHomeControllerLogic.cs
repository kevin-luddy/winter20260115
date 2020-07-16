// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Home Controller Logic
    /// </summary>
    public interface IHomeControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Attempts to lock area for edit
        /// </summary>
        /// <param name="area">area to lock</param>
        /// <param name="isLockingAllAreas">true if user is locking all areas (i.e. for publish or rollback)</param>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        LockModelView LockArea(LockArea area, bool isLockingAllAreas, out bool status, out string message);

        /// <summary>
        /// Attempts to lock all areas, i.e. when performing a publish or rollback operation.
        /// </summary>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        LockModelView LockAllAreas(out bool status, out string message);

        /// <summary>
        /// Attempts to unlock area
        /// </summary>
        /// <param name="area">area to unlock</param>
        /// <param name="isUnlockingAllAreas">true if user is unlocking all areas (i.e. for publish or rollback)</param>
        /// <returns>Lock Model View</returns>
        LockModelView UnlockArea(LockArea area, bool isUnlockingAllAreas);

        /// <summary>
        /// Attempts to unlock all areas, i.e. after performing a publish or rollback operation.
        /// </summary>
        /// <returns>Any locks that are active but not owned by the active user</returns>
        ICollection<AreaLockData> UnlockAllAreas();

        /// <summary>
        /// Attempts to refresh the lock on area for edit
        /// </summary>
        /// <param name="area">area to refresh lock</param>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        LockModelView RefreshLockOnArea(LockArea area, out bool status, out string message);
    }
}
