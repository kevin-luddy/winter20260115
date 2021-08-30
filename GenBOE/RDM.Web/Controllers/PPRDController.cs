// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    /// Controller for the PPR&amp;D Document.
    /// </summary>
    public class PPRDController : RDMController
    {
        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private readonly IHomeControllerLogic homeControllerLogic;

        /// <summary>
        /// The Burden Pool Controller Logic.
        /// </summary>
        private readonly IPPRDControllerLogic controllerLogic;

        /// <summary>
        /// Section Loader
        /// </summary>
        private readonly ISectionLoader sectionLoader;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerLogic">Controller Logic</param>
        /// <param name="sectionLoader">Section Loader</param>
        /// <param name="homeControllerLogic">Home Controller Logic</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public PPRDController(IPPRDControllerLogic controllerLogic, 
            ISectionLoader sectionLoader, IHomeControllerLogic homeControllerLogic,
            IWhosOnlineLoader whosOnlineLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
            this.sectionLoader = sectionLoader;
            this.homeControllerLogic = homeControllerLogic;
        }

        /// <summary>
        /// Index for PPR&amp;D Sections.
        /// </summary>
        /// <returns>Index (read-only) page for PPR&amp;D Sections.</returns>
        public ActionResult Index()
        {
            PPRDModelView model = new PPRDModelView
            {
                Revision = this.Logic.WipRevision,
                SectionContentTypeOptions = ExtensionMethods.GetOptions<SectionContentType>().OrderBy(x => x.Label).ToList()
            };

            return this.View(model);
        }

        /// <summary>
        /// Gets the PPR&amp;D data
        /// </summary>
        /// <returns>PPR&amp;D data</returns>
        [HttpPost]
        public ActionResult GetPPRD()
        {
            RevisionModelView revision = this.Logic.WipRevision;
            PPRDModelView pprd = new PPRDModelView()
                {
                    Revision = revision,
                    ChildNodes = this.sectionLoader.RetrieveAllSections(revision),
                    LockInfo = this.homeControllerLogic.GetCurrentLockInfo(LockArea.RDMSections)
                };

            return this.Json(pprd);
        }

        /// <summary>
        /// Saves the specified PPR&amp;D document.
        /// POST: Section/Save
        /// </summary>
        /// <param name="pprd">The PPR&amp;D document to be saved.</param>
        /// <returns>Json Result of the save.</returns>
        [HttpPost]
        public ActionResult Save([ModelBinder(typeof(JsonNetModelBinder))] PPRDModelView pprd)
        {
            if (pprd == null)
            {
                throw new ArgumentNullException(nameof(pprd));
            }

            #region Validate

            Collection<ValidationMessage> errors = new Collection<ValidationMessage>();
            if (!this.ModelState.IsValid)
            {
                errors = Utilities.CreateModelStateValidationErrorList(this.ModelState);
            }

            this.controllerLogic.ValidateSections(pprd.ChildNodes, errors);
            if (errors.Any())
            {
                throw new GenValidationException(errors);
            }

            #endregion

            // Confirm that either user owns lock or area is unlocked
            this.Logic.VerifyLockForSaving(LockArea.RDMSections, pprd.Revision.Id);

            #region Save

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.sectionLoader.UpdateSectionsAndContent(pprd.Revision, pprd.ChildNodes);
                scope.Complete();
            }

            #endregion

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Validates the delete section.
        /// </summary>
        /// <param name="sectionIds">The section ids.</param>
        /// <returns>Json Result of the validation.</returns>
        [HttpPost]
        public ActionResult ValidateDeleteSection(int[] sectionIds)
        {
            Collection<ValidationMessage> errors = new Collection<ValidationMessage>();
            RevisionModelView revision = this.Logic.WipRevision;
            ICollection<OptionModelView> sections = this.sectionLoader.RetrieveSectionsAsOptions(revision);
            this.controllerLogic.ValidateDeletionSections(sectionIds, errors, sections, revision);
            if (errors.Any())
            {
                throw new GenValidationException(errors);
            }

            return this.Json(new { Status = true });
        }
    }
}
