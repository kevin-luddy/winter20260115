// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Interface for a base RDM Controller Logic class.
    /// </summary>
    public interface IRdmControllerLogic
    {
        #region Loaders

        /// <summary>
        /// Area Locking Loader
        /// </summary>
        IAreaLockingLoader AreaLockingLoader { get; }

        /// <summary>
        /// Revision Loader
        /// </summary>
        IRevisionMediator RevisionMediator { get; }

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        IActiveDirectoryService AdUtils { get; }

        /// <summary>
        /// Security Information
        /// </summary>
        ISecurityInformation SecurityInformation { get; }

        #endregion

        /// <summary>
        /// Gets the current lock information without trying to lock.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <returns>Lock Model View</returns>
        LockModelView GetCurrentLockInfo(LockArea area);

        /// <summary>
        /// Checks if the active user owns the lock
        /// </summary>
        /// <param name="areaLock">lock data containing lock owner data to compare to active user</param>
        /// <returns>True if active user owns lock, false if not</returns>
        bool ActiveUserOwnsLock(AreaLockData areaLock);

        /// <summary>
        /// Gets NT ID for current user
        /// </summary>
        string ActiveUserNTID { get; }

        /// <summary>
        /// Gets Current user information
        /// </summary>
        UserData ActiveUser { get; }

        /// <summary>
        /// Helper method to determine if the current user is a member of the RDMAdmin user group.
        /// </summary>
        bool IsRDMAdminUser { get; }

        /// <summary>
        /// Helper method to determine if the current user is a member of the RDMCobraAdmin user group.
        /// </summary>
        bool IsRDMCobraAdminUser { get; }

        /// <summary>
        /// IsUserAuthorized
        /// returns true if user is authorized access to controller action as follows:
        /// 1) User is RDM Admin or RDM COBRA Admin - allow access to common Admin actions (i.e. Lock/Unlock/RefreshLock)
        /// 2) User is RDM Admin - allow access to all actions (except COBRA Admin actions)
        /// 3) User is RDM COBRA Admin - allow access to COBRA Admin actions
        /// 4) User is Viewer - allow access to Viewer actions
        /// </summary>
        /// <param name="controllerName">Controller</param>
        /// <param name="functionName">Action</param>
        /// <returns>bool</returns>
        bool IsUserAuthorized(string controllerName, string functionName);

        /// <summary>
        /// Gets all Revisions, removing the WIP revision for non-admins
        /// </summary>
        ICollection<RevisionModelView> Revisions { get; }

        /// <summary>
        /// WIP Revision
        /// </summary>
        RevisionModelView WipRevision { get; }

        /// <summary>
        /// Last Published Revision
        /// </summary>
        RevisionModelView LastPublishedRevision { get; }

        /// <summary>
        /// Verify whether the user owns the area lock for an area they are trying to save
        /// </summary>
        /// <param name="areaToCheck">Are which you are checking</param>
        /// <param name="revisionId">Revision Id</param>
        void VerifyLockForSaving(LockArea areaToCheck, int revisionId);
    }
}
