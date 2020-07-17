// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using IES.ActionLogic.ControllerLogic;
    using IES.Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using RDM.Web.Common;

    /// <summary>
    /// The Home Controller.
    /// </summary>
    public class HomeController : RDMController
    {
        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private readonly IHomeControllerLogic controllerLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerLogic">Home Controller Logic</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public HomeController(IHomeControllerLogic controllerLogic, IWhosOnlineLoader whosOnlineLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
        }

        /// <summary>
        /// Index for RDM Home page
        /// </summary>
        /// <returns>Index page for RDM.</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Gets the PPR&amp;D revisions
        /// </summary>
        /// <returns>PPR&amp;D revisions</returns>
        [HttpPost]
        public ActionResult GetRevisions()
        {
            RevisionGridModelView model = new RevisionGridModelView
            {
                Revisions = this.Logic.Revisions,
                ManagePprdMenu = this.GetPprdMenuOption(IESWebConstants.CONTROLLER_HOME),
                ManageRatesMenu = this.GetManageRatesMenuOption(),
                ManageBurdenPoolsMenu = this.GetManageBurdenPoolsMenuOption(),
                ManageCobraMappingsMenu = this.GetManageCobraMappingsMenuOption(),
                IsRdmAdminUser = this.Logic.IsRDMAdminUser,
                IsRdmCobraAdminUser = this.Logic.IsRDMCobraAdminUser
            };

            return this.Json(model);
        }

        /// <summary>
        /// Attempts to lock the PPR&amp;D revision for edit
        /// </summary>
        /// <param name="id">Area to lock</param>
        /// <returns>user's display name if revision locked; otherwise throws exceptions.</returns>
        [HttpPost]
        public ActionResult Lock(LockArea id)
        {
            this.ValidateArea(id);

            try
            {
                bool status;
                string message;

                LockModelView lockInfo = this.controllerLogic.LockArea(id, false, out status, out message);

                this.Log.Debug(
                    status
                        ? $"Lock successfully created in {id.ToDescription()} by {this.Logic.ActiveUser.DisplayName}."
                        : $"Failed to create lock in {id.ToDescription()} for {this.Logic.ActiveUser.DisplayName}. {message}");

                return this.Json(new { Status = status, LockInfo = lockInfo, Message = message });
            }
            catch (NotImplementedException ex)
            {
                return this.Json(ex);
            }
        }

        /// <summary>
        /// Attempts to unlock the PPR&amp;D revision.
        /// </summary>
        /// <param name="id">Area to lock</param>
        /// <returns>success if revision unlocked; otherwise throws exceptions.</returns>
        [HttpPost]
        public ActionResult Unlock(LockArea id)
        {
            this.ValidateArea(id);

            LockModelView lockInfo = this.controllerLogic.UnlockArea(id, false);

            this.Log.Debug(lockInfo.InUse == null
                ? $"Lock successfully removed in {id.ToDescription()}."
                : $"Failed to remove lock in {id.ToDescription()} because it was locked by {lockInfo.Editing}.");

            return this.Json(new { Status = true, LockInfo = lockInfo });
        }

        /// <summary>
        /// Attempts to refresh the lock for the PPR&amp;D revision.
        /// Called whenever the editing user performs client-side CRUD actions or requests a lock refresh.
        /// </summary>
        /// <param name="id">Area to lock</param>
        /// <returns>success if revision still locked; otherwise throws exceptions.</returns>
        [HttpPost]
        public ActionResult RefreshLock(LockArea id)
        {
            this.ValidateArea(id);

            bool status;
            string message;

            LockModelView lockInfo = this.controllerLogic.RefreshLockOnArea(id, out status, out message);

            this.Log.Debug(status
                ? $"Lock successfully refreshed in {id.ToDescription()} by {this.Logic.ActiveUser.DisplayName}."
                : $"Failed to refresh lock in {id.ToDescription()} for {this.Logic.ActiveUser.DisplayName}. {message}");

            return this.Json(new { Status = status, LockInfo = lockInfo, Message = message });
        }

        /// <summary>
        /// Validates that id for area is within the LockArea enum range
        /// </summary>
        /// <param name="areaId">area id</param>
        private void ValidateArea(LockArea areaId)
        {
            if (!Enum.IsDefined(typeof(LockArea), areaId))
            {
                throw new ArgumentException("Not a valid area to lock.");
            }
        }
    }
}