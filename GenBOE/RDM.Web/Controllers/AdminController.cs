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
    /// Controller for Administration.
    /// </summary>
    public class AdminController : RDMController
    {
        /// <summary>
        /// The Admin Controller Logic.
        /// </summary>
        private readonly IAdminControllerLogic controllerLogic;

        /// <summary>
        /// The Home Controller Logic.
        /// </summary>
        private readonly IHomeControllerLogic homeControllerLogic;

        /// <summary>
        /// Cobra Mapping Detail Loader
        /// </summary>
        private readonly ICobraDetailLoader cobraDetailLoader;

        /// <summary>
        /// Cobra Years Loader
        /// </summary>
        private readonly ICobraYearsLoader cobraYearsLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerLogic">Admin Controller Logic.</param>
        /// <param name="homeControllerLogic">Home Controller Logic.</param>
        /// <param name="cobraDetailLoader">Cobra Mapping Detail Loader</param>
        /// <param name="cobraYearsLoader">Cobra Years Loader</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public AdminController(IAdminControllerLogic controllerLogic, IHomeControllerLogic homeControllerLogic, ICobraDetailLoader cobraDetailLoader, ICobraYearsLoader cobraYearsLoader, IWhosOnlineLoader whosOnlineLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
            this.homeControllerLogic = homeControllerLogic;
            this.cobraDetailLoader = cobraDetailLoader;
            this.cobraYearsLoader = cobraYearsLoader;
        }

        /// <summary>
        /// Cobra Year Configuration Page.
        /// </summary>
        /// <returns>The CobraYearConfiguration View.</returns>
        public ViewResult CobraYearConfiguration()
        {
            return this.View(IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION);
        }

        /// <summary>
        /// Cobra Mapping Details Page.
        /// </summary>
        /// <param name="id">Optional Revision Id</param>
        /// <returns>The Cobra Mapping Details View.</returns>
        public ViewResult CobraDetails(int? id)
        {
            // Only set the Revision Id. Remaining grid data will be loaded via AJAX call to GetRatesByVersion().
            return this.View(IESWebConstants.VIEW_COBRA_DETAILS, new CobraGridModelView { SelectedRevisionId = id });
        }

        /// <summary>
        /// Gets the COBRA mapping details for specified revision.
        /// </summary>
        /// <param name="id">The revision Id.</param>
        /// <returns>COBRA mapping details for the version requested</returns>
        [HttpPost]
        public ActionResult GetCobraDetailsByVersion(int? id)
        {
            ActionResult response;

            try
            {
                ICollection<RevisionModelView> revisions = this.Logic.Revisions;

                RevisionModelView revision = id.HasValue ? revisions.FirstOrDefault(r => r.Id == id.Value) : revisions.LastOrDefault();
                if (revision == null)
                {
                    throw new GenValidationException("Revision not found.");
                }

                CobraGridModelView model = new CobraGridModelView
                {
                    SelectedRevisionId = revision.Id,
                    Versions = this.Logic.RevisionMediator.GetRevisionOptions(revisions),
                    CobraDetails = this.cobraDetailLoader.GetCobraDetailsByRevision(revision),
                    CobraCodes = ExtensionMethods.GetOptions<Code1>().OrderBy(x => x.Label).ToList(),
                    LockInfo = this.homeControllerLogic.GetCurrentLockInfo(LockArea.CobraData)
                };

                response = this.Json(model);
            }
            catch (Exception ex)
            {
                throw new GenValidationException(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Saves the specified collection.
        /// POST: SaveCobraDetails
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>Json Result of the saved COBRA mapping details.</returns>
        [HttpPost]
        public ActionResult SaveCobraDetails([ModelBinder(typeof(JsonNetModelBinder))] Collection<CobraDetailModelView> collection)
        {
            if (collection == null || collection.None())
            {
                throw new GenValidationException("There are no COBRA mapping details being saved.");
            }

            // Confirm that either user owns lock or area is unlocked
            this.Logic.VerifyLockForSaving(LockArea.CobraData, collection[0].RevisionId);

            RevisionModelView revision = this.Logic.RevisionMediator.GetById(collection[0].RevisionId);
            ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateCobraDetailModelViews(collection);
            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Snapshot,
                    Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
                }))
            {
                this.cobraDetailLoader.SaveDetails(collection);
                scope.Complete();
            }

            ICollection<CobraDetailModelView> cobraDetails = this.cobraDetailLoader.GetCobraDetailsByRevision(revision);

            return this.Json(cobraDetails);
        }

        /// <summary>
        /// Rate Code Replication page.
        /// </summary>
        /// <returns>The Rate Code Replication View.</returns>
        public ViewResult RateCodeReplication()
        {
            return this.View(IESWebConstants.VIEW_RATE_CODE_REPLICATION);
        }

        /// <summary>
        /// Gets the Rate Code Replication.
        /// </summary>
        /// <returns>The Rate Code Replication</returns>
        [HttpPost]
        public ActionResult GetRateCodeReplication()
        {
            RateCodeReplicationModelView model = this.controllerLogic.GetRateCodeReplication();
            return this.Json(model);
        }

        /// <summary>
        /// Saves the Rate Code Replication.
        /// </summary>
        /// <param name="rateCodes">The Rate Codes to save.</param>
        /// <returns>The JSON result of the save.</returns>
        [HttpPost]
        public ActionResult SaveRateCodeReplication([ModelBinder(typeof(JsonNetModelBinder))] Collection<RateCodeModelView> rateCodes)
        {
            if (rateCodes == null)
            {
                throw new ArgumentNullException(nameof(rateCodes));
            }

            ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateRateCodeReplication(rateCodes);

            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            foreach (RateCodeModelView rate in rateCodes)
            {
                if (rate.IsDeleted)
                {
                    rate.Updateable = UpdateType.Deleted;
                }
                else
                {
                    rate.Updateable = UpdateType.Upsert;
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // save them
                this.controllerLogic.SaveRateCodeReplication(rateCodes);
                scope.Complete();
            }
            
            return this.Json(new { Status = true, Message = "Rate Code Replication has been saved." });
        }

        /// <summary>
        /// Revision Configuration Page.
        /// </summary>
        /// <returns>The RevisionConfiguration View.</returns>
        public ViewResult RevisionConfiguration()
        {
            return this.View(IESWebConstants.VIEW_REVISION_CONFIGURATION);
        }

        /// <summary>
        /// Gets the WIP Revision Configuration.
        /// </summary>
        /// <returns>The WIP Revision information</returns>
        [HttpPost]
        public ActionResult GetRevisionConfiguration()
        {
            return this.Json(this.Logic.WipRevision);
        }

        /// <summary>
        /// Gets the menu options.
        /// </summary>
        /// <param name="menuOptionControllerName">The name of the controller for the selected menu option.</param>
        /// <returns>The options.</returns>
        [HttpPost]
        public ActionResult GetMenuOptions(string menuOptionControllerName)
        {
            // Set to lower case to be used for comparison to determine the selected controller.
            menuOptionControllerName = menuOptionControllerName != null
                ? menuOptionControllerName.ToLower()
                : string.Empty;

            // Create the menu.
            Collection<MenuOptionModelView> menu = new Collection<MenuOptionModelView>()
            {
                this.GetHomeMenuOption(menuOptionControllerName),
                this.GetPprdMenuOption(menuOptionControllerName),
                this.GetRatesMenuOption(menuOptionControllerName),
                this.GetReportsMenuOption(menuOptionControllerName),
                this.GetManageFileAttachmentsMenuOption(menuOptionControllerName, false),
                this.GetAdminMenuOption(menuOptionControllerName)
            };

            return this.Json(new { menu });
        }

        /// <summary>
        /// Gets the Cobra Year Configuration mappings.
        /// </summary>
        /// <returns>The Year to CobraDate mappings.</returns>
        [HttpPost]
        public ActionResult GetCobraYearConfiguration()
        {
            ICollection<CobraYearGridModelView> cobraYearMappings = this.cobraYearsLoader.GetAll();

            return this.Json(new { cobraYearMappings });
        }

        /// <summary>
        /// Saves the Cobra Year Configuration mappings.
        /// </summary>
        /// <param name="dataToSave">The collection of configurations to save. We get a list of items that we want to save, 
        /// needing to remove anything that is no longer in the collection</param>
        /// <returns>The JSON result of the save.</returns>
        [HttpPost]
        public JsonResult SaveCobraYearConfiguration([ModelBinder(typeof(JsonNetModelBinder))] ICollection<CobraYearGridModelView> dataToSave)
        {
            ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateCobraYearConfigurations(dataToSave);

            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            int newId = -1;

            // retrieve all of the current mappings.. mark them all as deleted
            List<CobraYearGridModelView> currentMappings = this.cobraYearsLoader.GetAll().ToList();
            currentMappings.ForEach(x => x.Updateable = UpdateType.Deleted);

            // mark all of the data as upsert, this is what we are keeping, appropriately setting IDs for the new items
            dataToSave.ToList().ForEach(x => { x.Updateable = UpdateType.Upsert; x.Id = x.Id > 0 ? x.Id : newId--; });

            // remove from the current mappings any that we are keeping
            dataToSave.Where(itemToSave => itemToSave.Id > 0).ToList().ForEach(itemToSave => currentMappings.Remove(currentMappings.First(current => current.Id == itemToSave.Id)));

            // merge them together
            dataToSave.AddRange(currentMappings);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // save them
                this.cobraYearsLoader.Save(dataToSave);
                scope.Complete();
            }

            return new JsonResult();
        }

        /// <summary>
        /// Saves the Revision Configuration start and end years.
        /// </summary>
        /// <param name="startYear">Start year for configuration.</param>        
        /// <param name="endYear">End year for configuration.</param>
        /// <param name="releaseNotes">The release notes for this revision.</param>
        /// <param name="history">The history for this Revision.</param>
        /// <returns>The JSON result of the save.</returns>
        [HttpPost]
        public ActionResult SaveRevisionConfiguration(string startYear, string endYear, string releaseNotes, string history)
        {
            ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateRateYearConfiguration(startYear, endYear);

            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            // retrieve the current WIP revision and set the start/end year
            RevisionModelView revision = this.Logic.WipRevision;

            if (revision == null)
            {
                throw new GenValidationException("Revision Configuration could not be saved.  Unable to get current WIP revision.");
            }
            else
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    revision.StartYear = int.Parse(startYear);
                    revision.EndYear = int.Parse(endYear);
                    revision.History = history;
                    revision.ReleaseNotes = releaseNotes;
                    revision.Updateable = UpdateType.Upsert;
                    this.Logic.RevisionMediator.Upsert(revision);
                    scope.Complete();
                }
            }

            return this.Json(new { Status = true, Message = "Revision Configuration has been saved." });
        }

        /// <summary>
        /// RESTful endpoint to clear the Revision cache.
        /// </summary>
        /// <returns>JSON object with status of request.</returns>
        public ActionResult ClearRevisionCache()
        {
            this.controllerLogic.ClearRevisionCache();
            return this.Json(new { Status = true });    // success - revision cache cleared
        }
    }
}