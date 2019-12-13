// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the Home Controller
    /// </summary>
    public class HomeControllerLogic : RdmControllerLogic, IHomeControllerLogic
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="revisionMediator">Revision Loader</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public HomeControllerLogic(IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
        }

        /// <summary>
        /// Attempts to lock area for edit
        /// </summary>
        /// <param name="area">area to lock</param>
        /// <param name="isLockingAllAreas">true if user is locking all areas (i.e. for publish or rollback)</param>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        public LockModelView LockArea(LockArea area, bool isLockingAllAreas, out bool status, out string message)
        {
            bool isLockAllowed = this.IsLockAllowed(area, isLockingAllAreas);
            if (!isLockAllowed)
            {
                throw new AuthorizationException("You are not authorized to perform this operation.");
            }

            LockModelView lockInfo;
            AreaLockData areaLock = this.AreaLockingLoader.GetAreaLock(area);
            if (areaLock == null)
            {
                // add lock
                areaLock = new AreaLockData() { Area = area, TimeOfLock = DateTime.Now, LockedBy = this.ActiveUser.DeepClone() };
                this.AreaLockingLoader.LockArea(area, areaLock);
                lockInfo = new LockModelView(false, isLockAllowed, areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
                status = true;
                message = "Successfully locked for edit.";
            }
            else
            {
                // already locked
                status = this.ActiveUserOwnsLock(areaLock);
                lockInfo = new LockModelView(!status, isLockAllowed, areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
                message = $"The {area.GetDescription()} page is currently locked for edit by {areaLock.LockedBy.DisplayName}.";
            }

            return lockInfo;
        }

        /// <summary>
        /// Attempts to lock all areas, i.e. when performing a publish or rollback operation.
        /// </summary>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        public LockModelView LockAllAreas(out bool status, out string message)
        {
            // Only RDM Admins can lock all areas
            if (!this.IsRDMAdminUser)
            {
                throw new AuthorizationException("You are not authorized to perform this operation.");
            }

            LockModelView lockInfo = new LockModelView();
            status = false;

            foreach (LockArea area in Enum.GetValues(typeof(LockArea)))
            {
                if (area != IES.Common.LockArea.None)
                {
                    lockInfo = this.LockArea(area, true, out status, out message);
                    if (!status)
                    {
                        return lockInfo;
                    }
                }
            }

            message = $"The revision is currently locked for edit by {lockInfo.Editing}.";

            return lockInfo;
        }

        /// <summary>
        /// Attempts to unlock area
        /// </summary>
        /// <param name="area">area to unlock</param>
        /// <param name="isUnlockingAllAreas">true if user is unlocking all areas (i.e. for publish or rollback)</param>
        /// <returns>Lock Model View</returns>
        public LockModelView UnlockArea(LockArea area, bool isUnlockingAllAreas)
        {
            bool isLockAllowed = this.IsLockAllowed(area, isUnlockingAllAreas);
            if (!isLockAllowed)
            {
                throw new AuthorizationException("You are not authorized to perform this operation.");
            }

            LockModelView lockInfo = new LockModelView(true, isLockAllowed, null, string.Empty);
            AreaLockData areaLock = this.AreaLockingLoader.GetAreaLock(area);
            if (areaLock != null)
            {
                if (this.ActiveUserOwnsLock(areaLock))
                {
                    this.AreaLockingLoader.UnlockArea(area);
                }
                else
                {
                    lockInfo.Editing = areaLock.LockedBy.DisplayName;
                    lockInfo.InUse = areaLock.TimeOfLock;
                }
            }

            return lockInfo;
        }

        /// <summary>
        /// Attempts to unlock all areas, i.e. after performing a publish or rollback operation.
        /// </summary>
        /// <returns>Any locks that are active but not owned by the active user</returns>
        public ICollection<AreaLockData> UnlockAllAreas()
        {
            // Only RDM Admins can unlock all areas
            if (!this.IsRDMAdminUser)
            {
                throw new AuthorizationException("You are not authorized to perform this operation.");
            }

            foreach (LockArea area in Enum.GetValues(typeof(LockArea)))
            {
                if (area != IES.Common.LockArea.None)
                {
                    this.UnlockArea(area, true);
                }
            }

            return this.AreaLockingLoader.GetActiveLocksByOtherUsers(this.ActiveUser);
        }

        /// <summary>
        /// Attempts to refresh the lock on area for edit
        /// </summary>
        /// <param name="area">area to refresh lock</param>
        /// <param name="status">json status</param>
        /// <param name="message">json message</param>
        /// <returns>Lock Model View</returns>
        public LockModelView RefreshLockOnArea(LockArea area, out bool status, out string message)
        {
            bool isLockAllowed = this.IsLockAllowed(area, false);
            if (!isLockAllowed)
            {
                throw new AuthorizationException("You are not authorized to perform this operation.");
            }

            LockModelView lockInfo;
            AreaLockData areaLock = this.AreaLockingLoader.GetAreaLock(area);
            if (areaLock != null)
            {
                if (this.ActiveUserOwnsLock(areaLock))
                {
                    // Lock exists and user was the one to lock it
                    areaLock.TimeOfLock = DateTime.Now;
                    this.AreaLockingLoader.LockArea(area, areaLock);
                    lockInfo = new LockModelView(false, isLockAllowed, areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
                    status = true;
                    message = string.Empty;
                }
                else
                {
                    // Lock exists, but a different user locked it
                    lockInfo = new LockModelView(true, isLockAllowed, areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
                    status = false;
                    message = $"The {area.GetDescription()} page is currently locked for edit by {areaLock.LockedBy.DisplayName}. Your editing session may have timed out.";
                }
            }
            else
            {
                // Lock no longer exists, create a new lock
                areaLock = new AreaLockData() { Area = area, TimeOfLock = DateTime.Now, LockedBy = this.ActiveUser.DeepClone() };
                this.AreaLockingLoader.LockArea(area, areaLock);
                lockInfo = new LockModelView(false, isLockAllowed, areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
                status = true;
                message = string.Empty;
            }

            return lockInfo;
        }
    }
}
