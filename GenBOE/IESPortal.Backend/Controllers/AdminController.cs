// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Backend.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using IES.ActionLogic.Common;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.ModelView;
    using IES.Core;
    using IES.Core.Exceptions;
    using IES.Core.PickList;
    using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// The Admin Controller for Admin functions
	/// </summary>
	public class AdminController : IESController
    {

        /// <summary>
        /// The admin controller logic
        /// </summary>
        private IESPortalAdminControllerLogic adminControllerLogic;

        /// <summary>
        /// The banner mediator.
        /// </summary>
        private BannerMediator bannerMediator;

        /// <summary>
        /// The security information.
        /// </summary>
        private ISecurityInformation securityInformation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="securityInformation">The security information.</param>
        /// <param name="bannerMediator">The banner mediator.</param>
        public AdminController(ISecurityInformation securityInformation, 
            BannerMediator bannerMediator, 
            IESPortalAdminControllerLogic adminControllerLogic,
            ILogger<AdminController> logger) : base(logger)
        {
            this.securityInformation = securityInformation;
            this.bannerMediator = bannerMediator;
            this.adminControllerLogic = adminControllerLogic;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <returns>Returns the Index page.</returns>
        public IActionResult ClearCache()
        {
            this.InitializeAction("ClearCache");
            this.bannerMediator.ClearCache();
            return this.View("Index");
        }

        /// <summary>
        /// Returns the Admin index view.
        /// </summary>
        /// <returns>Returns the Admin index view.</returns>
        public ActionResult Index()
        {
            this.InitializeAction("Index");
            return this.View();
        }

        /// <summary>
        /// Returns the Banner Grid View.
        /// </summary>
        /// <returns>The Banner Grid View.</returns>
        public ActionResult Banners()
        {
            this.InitializeAction("Banners");

            // retrieve all Banners
            IReadOnlyCollection<BannerModelView> banners = this.bannerMediator.GetAll();
            return this.View(banners);
        }

        /// <summary>
        /// Returns the Edit View for a banner
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Edit view for a banner</returns>
        public ActionResult EditBanner(int id)
        {
            this.InitializeAction("EditBanner");
            // Defaults for New
            DateTime tomorrow = DateTime.Now.AddDays(1);
            
            // Create a default banner if this is new
            BannerModelView banner = new BannerModelView()
            {
                StartDate = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 5, 0, 0),
                HoursToShow = 17,
                BannerText = string.Empty,
                TurnOffTicker = false,
                SelectedApps = new string[] { "BOESSC", "BOERMS" }
            };
            
            if (id > 0)
            {
                banner = this.bannerMediator.GetById(id);

                if (banner == null)
                {
                    throw new GenValidationException("The Banner Id passed in was not found.");
                }
            }

            return View(banner);
        }

        /// <summary>
        /// Deletes the specified banner.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json result of the deletion.</returns>
        [HttpPost]
        public ActionResult DeleteBanner(int id)
        {
            this.InitializeAction("DeleteBanner");
            BannerModelView banner = this.bannerMediator.GetById(id);

            if (banner == null)
            {
                throw new GenValidationException("The Banner Id passed in was not found.");
            }

            banner.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Snapshot,
                    Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
                }))
            {
                this.bannerMediator.Save(banner);

                scope.Complete();
            }

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Saves the specified banner.
        /// </summary>
        /// <param name="banner">The banner.</param>
        /// <returns>Json result of the save.</returns>
        /// <exception cref="GenValidationException">Banner cannot be null.</exception>
        [HttpPost]
        public ActionResult SaveBanner([ModelBinder(typeof(JsonNetModelBinder))] BannerModelView banner)
        {
            this.InitializeAction("SaveBanner");
            if (banner == null)
            {
                throw new GenValidationException("Banner cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            if (banner.SelectedApps == null || banner.SelectedApps.None())
            {
                throw new GenValidationException("At least one Application must be selected.");
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Snapshot,
                    Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
                }))
            {
                banner.Updateable = UpdateType.Upsert;
                this.bannerMediator.Save(banner);

                scope.Complete();
            }

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Initalizes an action with any permissions
        /// </summary>
        /// <param name="functionName">The function to initialize</param>
        [NonAction]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void InitializeAction(string functionName)
        {
            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            this.StartAction(this.logger, functionName);

            // Check authorization
            if (!this.securityInformation.IsIESPortalAdminUser(this.securityInformation.ActiveUserNTID))
            {
                throw new AuthorizationException(functionName + " was not authorized");
            }
        }

        /// <summary>
        /// Overrides OnActionExecuted to allow us to finalize the action and log the load times.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        [NonAction]
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext != null)
            {
                string functionName = filterContext.ActionDescriptor.ActionName;
                this.FinalizeAction(this.logger, functionName);
            }

            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Manage Pick List page
        /// </summary>
        /// <returns>Partial view w/ the page</returns>
        public PartialViewResult DisplaySelectPickListToManage()
        {
            this.InitializeAction(IESWebConstants.DISPLAY_SELECT_PICK_LIST_TO_MANAGE);
            ICollection<SelectListItem> data = EnumUtilities.GetListItemsForEnum(typeof(PickListEnum));

            return this.PartialView(data);
        }

        /// <summary>
        /// Load the Manage Offline Applications View
        /// </summary>
        /// <returns>Manage Offline Applications View</returns>
        public ActionResult ManageOfflineApplications()
        {
            this.InitializeAction(IESWebConstants.MANAGE_OFFLINE_APPLICATIONS);

            ICollection<OfflineApplicationModelView> applications = this.adminControllerLogic.GetOfflineApplicationData();

            return this.View(applications);
        }

        /// <summary>
        /// Save updates to application offline status
        /// </summary>
        /// <param name="applications">ModelViews of the applications and statuses</param>
        public void SaveManageOfflineApplications(ICollection<OfflineApplicationModelView> applications)
        {
            this.adminControllerLogic.SaveOfflineApplicationData(applications);
        }

        /// <summary>
        /// Manage Pick List page
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <returns>Partial view w/ the page</returns>
        public PartialViewResult DisplayManagePickLists(int pickListType)
        {
            this.InitializeAction(IESWebConstants.DISPLAY_MANAGE_PICK_LISTS);
            PickListGridMV data = this.adminControllerLogic.GetPickListItems((PickListEnum)pickListType);

            return this.PartialView(data);
        }

        /// <summary>
        /// Fixes the pick list errors.
        /// </summary>
        /// <param name="pickListType">Type of the pick list.</param>
        /// <returns>success/failure</returns>
        public JsonResult FixPickListErrors(PickListEnum pickListType)
        {
            this.InitializeAction(IESWebConstants.ACTION_SAVE_MANAGE_PICK_LISTS);

            this.adminControllerLogic.FixPickListErrors(pickListType);

            return this.Json(true);
        }

        /// <summary>
        /// Save method for Pick List changes
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <param name="dataToSave">Pick List items to save</param>
        /// <returns>success/failure</returns>
        public JsonResult SaveManagePickLists(PickListEnum pickListType, ICollection<PickListModelView> dataToSave)
        {
            this.InitializeAction(IESWebConstants.ACTION_SAVE_MANAGE_PICK_LISTS);
            if (dataToSave == null || !dataToSave.Any())
            {
                throw new ArgumentNullException(nameof(dataToSave));
            }

            // Trim off any whitespace prior to validation and save
            foreach (PickListModelView item in dataToSave)
            {
                if (item.Text != null)
                {
                    item.Text = item.Text.Trim();
                }
            }

            ICollection<ValidationMessage> validationErrors = this.adminControllerLogic.ValidatePickListItems(pickListType, dataToSave);
            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            this.adminControllerLogic.SavePickListItems(pickListType, dataToSave);

            return this.Json(true);
        }
    }
}