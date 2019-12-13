// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace RDM.Web.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Web.Mvc;
    using IES.ActionLogic.ControllerLogic;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Newtonsoft.Json;

    /// <summary>
    /// RDM base controller
    /// </summary>
    public class RDMController : IESController
    {
        #region Private Properties

        /// <summary>
        ///  The logger
        /// </summary>
        protected Logger Log { get; }

        #endregion

        #region Loaders and Such

        /// <summary>
        /// Who's Online Loader
        /// </summary>
        protected IWhosOnlineLoader WhosOnlineLoader { get; set; }

        /// <summary>
        /// Controller Logic
        /// </summary>
        protected IRdmControllerLogic Logic { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// No public empty constructor allowed
        /// </summary>
        private RDMController()
        {
            this.Log = new Logger(typeof(RDMController));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        /// <param name="logic">RDM Controller Logic</param>
        public RDMController(IWhosOnlineLoader whosOnlineLoader, IRdmControllerLogic logic)
        {
            this.WhosOnlineLoader = whosOnlineLoader;
            this.Logic = logic;
            this.Log = new Logger(typeof(RDMController));
        }

        #endregion

        #region Events

        /// <summary>
        /// Initalizes an action with any permissions
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="controllerName">The controller to initialize</param>
        /// <param name="functionName">The function to initialize</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void InitializeAction(Logger logger, string controllerName, string functionName)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            // Add current user details
            try
            {
                this.WhosOnlineLoader.UpdateLastAccessTime(this.Logic.ActiveUser, ApplicationName.RDM.GetDescription());
            }
            catch
            {
                // Prevent displaying error in case user is in the middle of a transaction with save
            }

            this.StartAction(logger, functionName);
            if (!this.Logic.IsUserAuthorized(controllerName, functionName))
            {
                throw new AuthorizationException(functionName + " was not authorized");
            }
        }
        
        /// <summary>
        /// Override of the default OnActionExecuting, to allow us to do security verification
        /// </summary>
        /// <param name="filterContext">Context</param>
        [NonAction]
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IdentitySwap.IdentitySwappingForTesting.SwapIdentity("IsIdentitySwappingAllowed", "ActiveDirectoryPath", "ADGroupsAllowedToSwapIdentity");
            if (filterContext != null)
            {
                string functionName = filterContext.ActionDescriptor.ActionName.ToLower(); // Action
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
                this.InitializeAction(this.Log, controllerName, functionName);
            }

            base.OnActionExecuting(filterContext);
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
                this.FinalizeAction(this.Log, functionName);
            }

            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Override for an errored out page. This way we are logging each exception that bubbles up to a controller
        /// 
        /// Would it make sense for us to redirect to an error page at this point?
        /// </summary>
        /// <param name="filterContext">Filter Context</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if(filterContext != null)
            { 
                this.Log.Error(filterContext.Exception, $"An exception occurred in {filterContext.RouteData.Values["controller"]}.{filterContext.RouteData.Values["action"]}().");
            }

            base.OnException(filterContext);
        }

        #endregion

        /// <summary>
        /// Gets data for Who's Online
        /// </summary>
        /// <returns>Who's Online data</returns>
        [HttpPost]
        public ActionResult GetWhosOnline()
        {
            return this.Json(this.WhosOnlineLoader.GetWhosOnlineData(ApplicationName.RDM.GetDescription()));
        }

        #region Menu

        /// <summary>
        /// Gets the Revision menu option
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetHomeMenuOption(string menuOptionControllerName)
        {
            return new MenuOptionModelView()
            {
                Label = "Home",
                Link = "/" + IESWebConstants.CONTROLLER_HOME,
                Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_HOME.ToLower(),
                UserCanAccess = true
            };
        }

        /// <summary>
        /// Gets the PPR&amp;D menu option
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetPprdMenuOption(string menuOptionControllerName)
        {
            return new MenuOptionModelView()
            {
                Label = "PPR&D",
                Link = "/" + IESWebConstants.CONTROLLER_PPRD,
                Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_PPRD.ToLower(),
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_PPRD, IESWebConstants.VIEW_PPRD)
            };
        }

        /// <summary>
        /// Gets the Rates menu option
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetRatesMenuOption(string menuOptionControllerName)
        {
            return new MenuOptionModelView()
            {
                Label = "Rates",
                Link = "#",
                Selected =
                        menuOptionControllerName == IESWebConstants.CONTROLLER_RATE.ToLower() ||
                        menuOptionControllerName == IESWebConstants.CONTROLLER_BURDEN_POOL.ToLower(),
                SubMenuOptions = new Collection<MenuOptionModelView>()
                {
                    this.GetManageRatesMenuOption(),
                    this.GetManageBurdenPoolsMenuOption(),
                    this.GetComparePublishRollbackMenuOption()
                }
            };
        }

        /// <summary>
        /// Gets the Manage Rates menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetManageRatesMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Manage Rates",
                Link = "/" + IESWebConstants.CONTROLLER_RATE,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE,
                                IESWebConstants.ACTION_VIEW_RATES)
            };
        }

        /// <summary>
        /// Gets the Manage Burden Pools menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetManageBurdenPoolsMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Manage Burden Pools",
                Link = "/" + IESWebConstants.CONTROLLER_BURDEN_POOL,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL,
                                IESWebConstants.ACTION_VIEW_BURDEN_POOLS)
            };
        }

        /// <summary>
        /// Gets the Compare / Publish / Rollback menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        private MenuOptionModelView GetComparePublishRollbackMenuOption()
        {
            return new MenuOptionModelView()
            {
                // if user is not admin, adjust menu item label to "Compare" 
                Label = this.Logic.IsRDMAdminUser ? "Compare / Publish / Rollback" : "Compare",
                Link = "/" + IESWebConstants.VIEW_VERSION,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_VERSION,
                                IESWebConstants.VIEW_VERSION_DIFF)
            };
        }

        /// <summary>
        /// Gets the Reports menu option
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetReportsMenuOption(string menuOptionControllerName)
        {
            return new MenuOptionModelView()
            {
                Label = "Reports / Exports",
                Link = "/" + IESWebConstants.CONTROLLER_REPORTS,
                Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_REPORTS.ToLower(),
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.VIEW_REPORTS)
            };
        }

        /// <summary>
        /// Gets the Admin menu option
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetAdminMenuOption(string menuOptionControllerName)
        {
            return new MenuOptionModelView()
            {
                // The UserCanAccess property is ignored on parent menu options.  It is automatically determined
                // in the Angular controller based on whether or not the user has access to any child menu options.
                Label = "Admin",
                Link = "#",
                Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_ADMIN.ToLower() || (this.Logic.IsRDMAdminUser && menuOptionControllerName == IESWebConstants.CONTROLLER_FILE_ATTACHMENTS.ToLower()),
                IsAdminMenuOption = true,
                SubMenuOptions = new Collection<MenuOptionModelView>()
                {
                    this.GetManageFileAttachmentsMenuOption(menuOptionControllerName, true),
                    this.GetRevisionConfigurationMenuOption(),
                    this.GetCobraYearConfigurationMenuOption(),
                    this.GetManageCobraMappingsMenuOption(),
                    this.GetRateCodeReplicationMenuOption()
                }
            };
        }

        /// <summary>
        /// Gets the Manage File Attachments menu option for the Admin list.
        /// </summary>
        /// <param name="menuOptionControllerName">Current controller name</param>
        /// <param name="isAdmin">True if this is for the admin menu, false if this is for the non-admin menu.</param>
        /// <returns>Menu option for Managing File Attachments</returns>
        protected MenuOptionModelView GetManageFileAttachmentsMenuOption(string menuOptionControllerName, bool isAdmin)
        {
            return new MenuOptionModelView()
            {
                Selected = !isAdmin && menuOptionControllerName == IESWebConstants.CONTROLLER_FILE_ATTACHMENTS.ToLower(),
                Label = isAdmin ? "Manage File Attachments" : "File Attachments",
                Link = "/" + IESWebConstants.CONTROLLER_FILE_ATTACHMENTS + "/" +
                                   IESWebConstants.VIEW_FILE_ATTACHMENTS,
                UserCanAccess = this.Logic.IsRDMAdminUser == isAdmin
            };
        }

        /// <summary>
        /// Gets the Revision Configuration menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        private MenuOptionModelView GetRevisionConfigurationMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Revision Configuraton",
                Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
                                   IESWebConstants.VIEW_REVISION_CONFIGURATION,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
                                IESWebConstants.VIEW_REVISION_CONFIGURATION)
            };
        }

        /// <summary>
        /// Gets the rate code replication menu option.
        /// </summary>
        /// <returns>Menu option information</returns>
        private MenuOptionModelView GetRateCodeReplicationMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Rate Code Replication",
                Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
                                   IESWebConstants.VIEW_RATE_CODE_REPLICATION,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
                                IESWebConstants.VIEW_RATE_CODE_REPLICATION)
            };
        }

        /// <summary>
        /// Gets the Cobra Year Configuration menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        private MenuOptionModelView GetCobraYearConfigurationMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Cobra Year Configuration",
                Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
                                   IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
                                IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION)
            };
        }

        /// <summary>
        /// Gets the Manage Cobra Mappings menu option
        /// </summary>
        /// <returns>Menu option information</returns>
        protected MenuOptionModelView GetManageCobraMappingsMenuOption()
        {
            return new MenuOptionModelView()
            {
                Label = "Manage Cobra Mappings",
                Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
                                   IESWebConstants.VIEW_COBRA_DETAILS,
                UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
                                IESWebConstants.VIEW_COBRA_DETAILS)
            };
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Jsons the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Returns JSON for the specified data.</returns>
        protected new ContentResult Json(object data)
        {
            JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
            return this.Content(JsonConvert.SerializeObject(data, microsoftDateFormatSettings), "application/json");
        }

        #endregion
    }
}