// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using IES.ActionLogic.ControllerLogic;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using RDM.Web.Common;

    /// <summary>
    /// Controller for the Versions.
    /// </summary>
    public class VersionController : RDMController
    {
        /// <summary>
        /// Version Controller Logic
        /// </summary>
        private readonly IVersionControllerLogic controllerLogic;

        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private readonly IHomeControllerLogic homeControllerLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerLogic">Version Controller Logic</param>
        /// <param name="homeControllerLogic">Home Controller Logic</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public VersionController(IVersionControllerLogic controllerLogic, IHomeControllerLogic homeControllerLogic, 
            IWhosOnlineLoader whosOnlineLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
            this.homeControllerLogic = homeControllerLogic;
        }

        /// <summary>
        /// Index for Versions.
        /// </summary>
        /// <param name="id">A possible id to a revision, if passed in.</param>
        /// <returns>Index page for Versions.</returns>
        public ActionResult Index(int? id)
        {
            int revisionId = id ?? -1;

            VersionComparisonModelView modelView = this.controllerLogic.GetVersionDifferences(revisionId, -1, this.Logic.IsRDMAdminUser, this.Logic.ActiveUser);
            return this.View(modelView);
        }

        /// <summary>
        /// Get the differences for the Version Comparison grid when a new Version is selected
        /// </summary>
        /// <param name="id">Version Selected</param>
        /// <param name="secondId">Second selected revision ID, or -1 to return previous revision</param>
        /// <returns>Updated differences based on the selected version</returns>
        [HttpPost]
        public ActionResult GetVersionDifferences(int id, int secondId)
        {
            return this.Json(this.controllerLogic.GetVersionDifferences(id, secondId, this.Logic.IsRDMAdminUser, this.Logic.ActiveUser));
        }

        /// <summary>
        /// Validates the rates for the selected revision to see if any rates would have all 0 values for the displayed years prior to publishing.
        /// </summary>
        /// <param name="id">The revision to validate.</param>
        /// <returns>JsonResult containing whether the rates were valid.</returns>
        [HttpPost]
        public ActionResult ValidateRates(int? id)
        {
            ICollection<string> invalidRates = this.controllerLogic.GetInvalidRates(id);

            return this.Json(new { Valid = !invalidRates.Any(), InvalidRates = invalidRates });
        }

        /// <summary>
        /// Publish the current WIP revision.
        /// </summary>
        /// <param name="revision">The revision of the WIP to publish.</param>
        /// <param name="history">The updated history for publishing.</param>
        /// <param name="releaseNotes">The release notes for publishing.</param>
        /// <returns>JsonResult containing the Id of the new WIP revision.</returns>
        public ActionResult Publish(string revision, string history, string releaseNotes)
        {
            int? newId = null;
            bool status;
            string message;
            ICollection<AreaLockData> activeLocks;

            // lock the revision (all areas)
            this.homeControllerLogic.LockAllAreas(out status, out message);

            try
            {
                if (status)
                {
                    this.Log.Debug($"Revision locked successfully by {this.Logic.ActiveUser.DisplayName}.");
                    RevisionModelView wipRevision = this.Logic.WipRevision;

                    if (wipRevision.Revision != revision)
                    {
                        message = $"Publish failed - Revision {revision} is no longer the WIP.  Please refresh the page.";
                    }
                    else
                    {
                        wipRevision.History = history;
                        wipRevision.ReleaseNotes = releaseNotes;

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                        {
                            newId = this.Logic.RevisionMediator.Publish(wipRevision, this.Logic.ActiveUser.DisplayName);
                            scope.Complete();
                        }

                        RevisionModelView publishedRevision = this.Logic.RevisionMediator.GetById(wipRevision.Id);
                        this.controllerLogic.SendPublishEmail(publishedRevision, this.Logic.ActiveUser);

                        message = "Publish operation was successful.";
                    }
                }
                else
                {
                    message = $"Publish failed - {message}";
                    this.Log.Debug(message);
                }
            }
            catch (GeneralAppException ex)
            {
                message = "Publish failed due to a system exception.";
                this.Log.Error(ex);
            }
            finally
            {
                activeLocks = this.homeControllerLogic.UnlockAllAreas();
            }

            return this.Json(new { Status = status, Id = newId, ActiveLocks = activeLocks, Message = message });
        }

        /// <summary>
        /// Rollback the current WIP revision.
        /// </summary>
        /// <param name="revision">The revision of the WIP to publish.</param>
        /// <returns>JsonResult containing the Id of the new WIP revision.</returns>
        public ActionResult Rollback(string revision)
        {
            int? id = null;
            bool status;
            string message;
            ICollection<AreaLockData> activeLocks;

            // lock the revision (all areas)
            this.homeControllerLogic.LockAllAreas(out status, out message);
            try
            {
                if (status)
                {
                    this.Log.Debug($"Revision locked successfully by {this.Logic.ActiveUser.DisplayName}.");

                    RevisionModelView wipRevision = this.Logic.WipRevision;

                    if (wipRevision.Revision != revision)
                    {
                        message = $"Rollback failed - Revision {revision} is no longer the WIP.  Please refresh the page.";
                    }
                    else
                    {
                        RevisionModelView lastPublishedRevision = this.Logic.LastPublishedRevision;
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                        {
                            id = this.Logic.RevisionMediator.Rollback(lastPublishedRevision, wipRevision);
                            scope.Complete();
                        }

                        message = "Rollback operation was successful.";
                    }
                }
                else
                {
                    message = $"Rollback failed - {message}";
                    this.Log.Debug(message);
                }
            }
            catch (GeneralAppException ex)
            {
                message = "Rollback failed due to a system exception.";
                this.Log.Error(ex);
            }
            finally
            {
                activeLocks = this.homeControllerLogic.UnlockAllAreas();
            }

            return this.Json(new { Status = status, Id = id, ActiveLocks = activeLocks, Message = message });
        }
    }
}