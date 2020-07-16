// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Abstract base class for RDM Controller Logic classes.
    /// </summary>
    /// <seealso cref="IES.ActionLogic.ControllerLogic.IRdmControllerLogic" />
    public abstract class RdmControllerLogic : IRdmControllerLogic
    {
        #region Loaders

        /// <summary>
        /// Area Locking Loader
        /// </summary>
        public IAreaLockingLoader AreaLockingLoader { get; }

        /// <summary>
        /// Revision Loader
        /// </summary>
        public IRevisionMediator RevisionMediator { get; }

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        public IActiveDirectoryUtilities AdUtils { get; }

        /// <summary>
        /// Security Information
        /// </summary>
        public ISecurityInformation SecurityInformation { get; }

        #endregion

        #region Controller Action Dictionaries

        /// <summary>
        /// The viewer controller action dictionary
        /// </summary>
        private static Dictionary<string, List<string>> viewerControllerActionDictionary;

        /// <summary>
        /// The COBRA Administrator controller action dictionary
        /// </summary>
        private static Dictionary<string, List<string>> cobraAdminControllerActionDictionary;

        /// <summary>
        /// The controller action dictionary for operations that may be performed by a regular or Cobra Admin (i.e. Lock/Unlock/RefreshLock)
        /// </summary>
        private static Dictionary<string, List<string>> commonAdminControllerActionDictionary;

        /// <summary>
        /// Dictionary to be used when checking a Viewer's controller action permissions
        /// </summary>
        private static Dictionary<string, List<string>> ViewerControllerActionDictionary
        {
            get
            {
                if (viewerControllerActionDictionary == null || !viewerControllerActionDictionary.Any())
                {
                    viewerControllerActionDictionary = AuthorizationDictionarySetup.GenerateViewerDictionary();
                }

                return viewerControllerActionDictionary;
            }
        }

        /// <summary>
        /// Dictionary to be used when checking a COBRA Administrator's controller action permissions
        /// </summary>
        private static Dictionary<string, List<string>> CobraAdminControllerActionDictionary
        {
            get
            {
                if (cobraAdminControllerActionDictionary == null || !cobraAdminControllerActionDictionary.Any())
                {
                    cobraAdminControllerActionDictionary = AuthorizationDictionarySetup.GenerateCobraAdminDictionary();
                }

                return cobraAdminControllerActionDictionary;
            }
        }

        /// <summary>
        /// Dictionary to be used when checking operations that may be performed by a regular or Cobra Admin (i.e. Lock/Unlock/RefreshLock)
        /// </summary>
        private static Dictionary<string, List<string>> CommonAdminControllerActionDictionary
        {
            get
            {
                if (commonAdminControllerActionDictionary == null || !commonAdminControllerActionDictionary.Any())
                {
                    commonAdminControllerActionDictionary = AuthorizationDictionarySetup.GenerateCommonAdminDictionary();
                }

                return commonAdminControllerActionDictionary;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RdmControllerLogic"/> class.
        /// </summary>
        /// <param name="areaLockingLoader">The area locking loader.</param>
        /// <param name="revisionMediator">The revision Mediator.</param>
        /// <param name="adUtils">Active Directory Utilities</param>
        /// <param name="securityInfo">Security Info</param>
        protected RdmControllerLogic(IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo)
        {
            this.AreaLockingLoader = areaLockingLoader;
            this.RevisionMediator = revisionMediator;
            this.AdUtils = adUtils;
            this.SecurityInformation = securityInfo;
        }

        /// <summary>
        /// Gets the current lock information without trying to lock.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <returns>Lock Model View</returns>
        public LockModelView GetCurrentLockInfo(LockArea area)
        {
            LockModelView lockInfo;

            AreaLockData areaLock = this.AreaLockingLoader.GetAreaLock(area);

            if (areaLock == null)
            {
                lockInfo = new LockModelView(true, this.IsLockAllowed(area, false), null, string.Empty);
            }
            else
            {
                // already locked
                bool status = this.ActiveUserOwnsLock(areaLock);
                lockInfo = new LockModelView(!status, this.IsLockAllowed(area, false), areaLock.TimeOfLock, areaLock.LockedBy.DisplayName);
            }

            return lockInfo;
        }

        /// <summary>
        /// Checks if the active user owns the lock
        /// </summary>
        /// <param name="areaLock">lock data containing lock owner data to compare to active user</param>
        /// <returns>True if active user owns lock, false if not</returns>
        public bool ActiveUserOwnsLock(AreaLockData areaLock)
        {
            if (areaLock == null)
            {
                throw new ArgumentNullException(nameof(areaLock));
            }

            return this.ActiveUser.Ntid == areaLock.LockedBy.Ntid;
        }

        /// <summary>
        /// Gets NT ID for current user
        /// </summary>
        public string ActiveUserNTID
        {
            get { return this.SecurityInformation.ActiveUserNTID; }
        }

        /// <summary>
        /// Gets Current user information
        /// </summary>
        public UserData ActiveUser
        {
            get
            {
                return this.AdUtils.GetUserByQualifiedAccount(this.ActiveUserNTID, false);
            }
        }

        /// <summary>
        /// Helper method to determine if the current user is a member of the RDMAdmin user group.
        /// </summary>
        public bool IsRDMAdminUser
        {
            get
            {
                // return false; // For testing only - Uncomment this line and comment out next line to set current user to non-admin read only
                return this.SecurityInformation.IsRdmAdminUser(this.ActiveUserNTID);
            }
        }

        /// <summary>
        /// Helper method to determine if the current user is a member of the RDMCobraAdmin user group.
        /// </summary>
        public bool IsRDMCobraAdminUser
        {
            get
            {
                // return false; // For testing only - Uncomment this line and comment out next line to set current user to non-cobra-admin access
                return this.SecurityInformation.IsRdmCobraAdminUser(this.ActiveUserNTID);
            }
        }

        /// <summary>
        /// Helper method to determine if the current user is a member of the RDMViewer user group.
        /// </summary>
        public bool IsRDMViewerUser
        {
            get
            {
                return this.SecurityInformation.IsRdmViewerUser(this.ActiveUserNTID);
            }
        }

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
        public bool IsUserAuthorized(string controllerName, string functionName)
        {
            // First, check if this is a common admin operation (i.e. Lock/Unlock/RefreshLock)
            if (this.IsCommonAdminAction(controllerName, functionName) && (this.IsRDMAdminUser || this.IsRDMCobraAdminUser))
            {
                return true;
            }

            // Next, check RDM Admin or RDM COBRA Admin specific operations
            bool isCobraAdminAction = this.IsCobraAdminAction(controllerName, functionName);
            if ((this.IsRDMAdminUser && !isCobraAdminAction) || (this.IsRDMCobraAdminUser && isCobraAdminAction))
            {
                return true;
            }

            // Otherwise, check to see if this is a Viewer action. 
            return (this.IsRDMViewerUser || this.IsRDMAdminUser || this.IsRDMCobraAdminUser) && this.IsViewerAction(controllerName, functionName);
        }

        /// <summary>
        /// Checks if the specified controller/function is allowed for a Viewer user.
        /// </summary>
        /// <param name="controllerName">Controller Name</param>
        /// <param name="functionName">Function Name</param>
        /// <returns>true if Viewer action; false otherwise.</returns>
        private bool IsViewerAction(string controllerName, string functionName)
        {
            List<string> viewerActions;
            return ViewerControllerActionDictionary.TryGetValue(controllerName.ToLower(), out viewerActions) && viewerActions.Contains(functionName.ToLower());
        }

        /// <summary>
        /// Checks if the specified controller/function is allowed for a COBRA Admin user.
        /// </summary>
        /// <param name="controllerName">Controller Name</param>
        /// <param name="functionName">Function Name</param>
        /// <returns>true if COBRA Admin action; false otherwise.</returns>
        private bool IsCobraAdminAction(string controllerName, string functionName)
        {
            List<string> cobraAdminActions;
            return CobraAdminControllerActionDictionary.TryGetValue(controllerName.ToLower(), out cobraAdminActions) && cobraAdminActions.Contains(functionName.ToLower());
        }

        /// <summary>
        /// Checks if the specified controller/function is allowed for a regular or COBRA admin user (i.e. Lock, Unlock, or RefreshLock).
        /// </summary>
        /// <param name="controllerName">Controller Name</param>
        /// <param name="functionName">Function Name</param>
        /// <returns>true if this action is allowed for a regular or COBRA admin; false otherwise.</returns>
        private bool IsCommonAdminAction(string controllerName, string functionName)
        {
            List<string> commonAdminActions;
            return CommonAdminControllerActionDictionary.TryGetValue(controllerName.ToLower(), out commonAdminActions) && commonAdminActions.Contains(functionName.ToLower());
        }

        /// <summary>
        /// Gets all Revisions, removing the WIP revision for non-admins
        /// </summary>
        public ICollection<RevisionModelView> Revisions
        {
            get
            {
                ICollection<RevisionModelView> data = this.RevisionMediator.GetAll();

                if (!this.IsRDMAdminUser && !this.IsRDMCobraAdminUser)
                {
                    // remove WIP if you are not an admin
                    RevisionModelView wipRevision = data.FirstOrDefault(x => x.Id == this.WipRevision.Id);
                    if (wipRevision != null)
                    {
                        data.Remove(wipRevision);
                    }
                }

                return data;
            }
        }

        /// <summary>
        /// WIP Revision
        /// </summary>
        public RevisionModelView WipRevision
        {
            get { return this.RevisionMediator.GetWipRevision(); }
        }

        /// <summary>
        /// Last Published Revision
        /// </summary>
        public RevisionModelView LastPublishedRevision
        {
            get
            {
                return this.Revisions.Where(r => r.DatePublished != null).OrderByDescending(r => r.DatePublished).FirstOrDefault();
            }
        }

        /// <summary>
        /// Verify whether the user owns the area lock for an area they are trying to save
        /// </summary>
        /// <param name="areaToCheck">Are which you are checking</param>
        /// <param name="revisionId">Revision Id</param>
        public void VerifyLockForSaving(LockArea areaToCheck, int revisionId)
        {
            // Confirm that either user owns lock or area is unlocked
            AreaLockData areaLock = this.AreaLockingLoader.GetAreaLock(areaToCheck);

            if (areaLock != null && (areaLock.LockedBy.Ntid != this.ActiveUser.Ntid))
            {
                string validationString = $"Revision {revisionId} is locked by {areaLock.LockedBy.DisplayName}.  Your editing session may have timed out.";
                throw new GenValidationException(validationString);
            }
        }

        /// <summary>
        /// Helper method to determine if editing is allowed for the specified area.
        /// </summary>
        /// <param name="area">lock area to check</param>
        /// <param name="isLockingAllAreas">bool noting if user is locking all areas, i.e. for publish or rollback.</param>
        /// <returns>true if user is allowed to lock the specified area; false otherwise.</returns>
        protected bool IsLockAllowed(LockArea area, bool isLockingAllAreas)
        {
            // if locking all areas, allow lock for RDM Admins (normally, RDM Admins aren't allowed to lock Cobra Data).
            if (isLockingAllAreas && this.IsRDMAdminUser)
            {
                return true;
            }

            return area == LockArea.CobraData ? this.IsRDMCobraAdminUser : this.IsRDMAdminUser;
        }
    }
}
