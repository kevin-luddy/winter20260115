// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    /// Controller for Managing File Attachments
    /// </summary>
    /// <seealso cref="RDMController" />
    public class FileAttachmentController : RDMController
    {
        /// <summary>
        /// The controller logic.
        /// </summary>
        private readonly IFileAttachmentControllerLogic controllerLogic;

        /// <summary>
        /// The section loader.
        /// </summary>
        private readonly ISectionLoader sectionLoader;

        /// <summary>
        /// The file attachment loader
        /// </summary>
        private readonly IFileAttachmentLoader fileAttachmentLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        /// <param name="controllerLogic">File Attachment Controller Logic</param>
        /// <param name="sectionLoader">The section loader.</param>
        /// <param name="fileAttachmentLoader">The file attachment loader.</param>
        public FileAttachmentController(IWhosOnlineLoader whosOnlineLoader, IFileAttachmentControllerLogic controllerLogic, 
            ISectionLoader sectionLoader, IFileAttachmentLoader fileAttachmentLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
            this.sectionLoader = sectionLoader;
            this.fileAttachmentLoader = fileAttachmentLoader;
        }

        /// <summary>
        /// Index for Managing File Attachments.
        /// </summary>
        /// <param name="id">Optional Revision Id</param>
        /// <returns>Index page for Managing File Attachments.</returns>
        public ActionResult Index(int? id)
        {
            FileAttachmentGridModelView model = new FileAttachmentGridModelView
            {
                SelectedRevisionId = id,
                LockInfo = this.controllerLogic.GetCurrentLockInfo(LockArea.FileAttachments)
            };

            return this.View(model);
        }

        /// <summary>
        /// Get the File Attachments information for the specified versionId (if provided).
        /// </summary>
        /// <param name="versionId">Optional revisionId.</param>
        /// <returns>File Attachments Grid data for specified revisionId.  If revisionId is null, return data for WIP version.</returns>
        public ActionResult GetFileAttachments(int? versionId)
        {
            ICollection<RevisionModelView> revisions = this.Logic.Revisions;

            RevisionModelView revision = versionId.HasValue ? revisions.FirstOrDefault(r => r.Id == versionId.Value) : revisions.LastOrDefault();
            if (revision == null)
            {
                throw new GenValidationException("Revision not found.");
            }

            FileAttachmentGridModelView model = new FileAttachmentGridModelView
            {
                SelectedRevisionId = revision.Id,
                Sections = this.sectionLoader.RetrieveSectionsAsOptions(revision),
                Versions = this.Logic.RevisionMediator.GetRevisionOptions(revisions),
                FileAttachments = this.fileAttachmentLoader.GetByRevision(revision.Id),
                LockInfo = this.controllerLogic.GetCurrentLockInfo(LockArea.FileAttachments)
            };

            return this.Json(model);
        }

        /// <summary>
        /// Saves the specified collection.
        /// POST: File Attachments/Save
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>Json Result of the save.</returns>
        [HttpPost]
        public ActionResult Save([ModelBinder(typeof(JsonNetModelBinder))] Collection<FileAttachmentRowModelView> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Client can add new rows, then delete them all - catch it here.
            Collection<FileAttachmentRowModelView> filteredCollection = new Collection<FileAttachmentRowModelView>();
            foreach (FileAttachmentRowModelView mv in collection)
            {
                if (mv.Id > 0 || !mv.IsDeleted)
                {
                    filteredCollection.Add(mv);
                }
            }

            if (filteredCollection.Any())
            {
                // If we don't have the revisionId in our filteredCollection, get the WIP from DB.
                int revisionId = 0;
                FileAttachmentRowModelView revisionFA = filteredCollection.FirstOrDefault(x => x.RevisionID > 0);

                if (revisionFA != null)
                {
                    revisionId = revisionFA.RevisionID;
                }

                // If we have no revisionIds - get the WIP revision.
                if (revisionId == 0)
                {
                    revisionId = this.Logic.WipRevision.Id;
                }

                // Confirm that either user owns lock or area is unlocked
                this.Logic.VerifyLockForSaving(LockArea.FileAttachments, revisionId);

                // Get the revision's Sections
                ICollection<OptionModelView> sections = this.sectionLoader.RetrieveSectionsAsOptions(new RevisionModelView { Id = revisionId });
                ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateFileAttachments(filteredCollection, sections);
                if (validationErrors.Any())
                {
                    throw new GenValidationException(validationErrors);
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    foreach (FileAttachmentRowModelView filtered in filteredCollection)
                    {
                        filtered.RevisionID = revisionId;
                        this.fileAttachmentLoader.Save(filtered);
                    }

                    scope.Complete();
                }
            }

            return new JsonResult();
        }
    }
}
