// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// GenTRAC base controller
    /// </summary>
    [HandleError]
    [NoCache]
    public class GenTRACController : Controller
    {
        #region Variables

        /// <summary>
        ///  The logger
        /// </summary>
        private IES.Common.Logger log = new IES.Common.Logger(typeof(GenTRACController));

        /// <summary>
        /// The controller dictionary
        /// </summary>
        private static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> controllerDictionary;

        /// <summary>
        /// The GenTRAC Controller Logic
        /// </summary>
        protected GenTRACControllerLogic GenTRACControllerLogic { get; set; }

        /// <summary>
        /// The site utilities
        /// </summary>
        protected SiteMasterUtilities SiteMasterUtilities { get; set; }

        /// <summary>
        /// Security Information for Active User
        /// </summary>
        private IES.Common.ISecurityInformation securityInformation = null;

        /// <summary>
        /// List of actions that save user permissions
        /// </summary>
        private List<string> savePermissionActions = new List<string>()
        {
            WebConstants.Action.ADMIN_SAVE_SYSTEM_PERMISSIONS.ToLower(),
            WebConstants.Action.SAVE_PROPOSAL_INFORMATION.ToLower()
        };
        
        /// <summary>
        /// Dictionary to be used when checking controller's action permissions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        private static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> ControllerDictionary
        {
            get
            {
                if (controllerDictionary == null || !controllerDictionary.Any())
                {
                    controllerDictionary = AuthorizationDictionarySetup.GenerateDictionary();
                }

                return controllerDictionary;
            }
        }

        /// <summary>
        /// String constant for the menu item
        /// </summary>
        private const string ADD_WBS_ELEMENT = "Add WBS Element";

        /// <summary>
        /// Add Default WBS Elements menu item
        /// </summary>
        private const string ADD_DEFAULT_WBS_ELEMENTS = "Add Default WBS Elements";

        #endregion

        #region Constructors

        /// <summary>
        /// No empty constructor allowed
        /// </summary>
        private GenTRACController()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityInformation">inSecurityInformation</param>
        /// <param name="ingenTRACControllerLogic">GenTRAC Controller Logic</param>
        /// <param name="inSiteMasterUtilities">Site utilities</param>
        public GenTRACController(IES.Common.ISecurityInformation inSecurityInformation,
            GenTRACControllerLogic ingenTRACControllerLogic,
            SiteMasterUtilities inSiteMasterUtilities)
        {
            this.GenTRACControllerLogic = ingenTRACControllerLogic;
            this.SiteMasterUtilities = inSiteMasterUtilities;
            this.securityInformation = inSecurityInformation;
        }

        #endregion

        /// <summary>
        /// Returns the master view
        /// </summary>
        /// <param name="inView">The view to return</param>
        /// <returns>The master view</returns>
        public virtual ViewResult GetMasterView(string inView)
        {
            GenTRACMasterModelView model = this.GenTRACControllerLogic.GetMasterView();
            return this.View(inView, model);
        }

        /// <summary>
        /// Returns a view with the home menu
        /// </summary>
        /// <param name="context">View Context</param>
        /// <returns>The home menu</returns>
        public PartialViewResult DisplayHomeMenu(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

			ICollection<GenTRACMenuItemModelView> theModelViews = this.GetHomeMenuItems(context.RouteData.Values["controller"].ToString());

            return this.PartialView(WebConstants.View.GENTRAC_HOME_MENU, this.FilterMenuItems(theModelViews, null));
        }

        /// <summary>
        /// Display the Error page for general exceptions
        /// </summary>
        /// <returns>Error Page</returns>
        public ActionResult Error()
        {
            return this.View(WebConstants.View.ERROR);
        }

        /// <summary>
        /// Display the Security Error page for security exceptions
        /// </summary>
        /// <returns>Security Error Page</returns>
        public ActionResult SecurityError()
        {
            return this.View(WebConstants.View.SECURITY_ERROR, new GenTRACMasterModelView());
        }

        /// <summary>
        /// Display Users Online
        /// </summary>
        /// <param name="parentWidget">parentWidget</param>
        /// <param name="dialogName">dialogName</param>
        /// <returns>a view</returns>
        public ViewResult DisplayWhosOnline(string parentWidget, string dialogName)
        {
            WhosOnlineModelView model = this.GenTRACControllerLogic.GetWhosOnlineData();

            this.ViewData["parentWidget"] = parentWidget;
            this.ViewData["dialogName"] = dialogName;

            return this.View(WebConstants.View.ONLINE_USERS, model);
        }

        /// <summary>
        /// Get the read only attribute for the given proposal
        /// </summary>
        /// <param name="securityAuthorization">The Security Authorization</param>
        /// <returns>False if the proposal is in the 'Working' state, true otherwise</returns>
        [NonAction]
        protected string GetReadOnlyAttribute(SecurityAuthorization securityAuthorization)
        {
            // Default is read-only
            string toReturn = "true";

            // Return true if the Security Authorization is Read or None
            toReturn = (securityAuthorization == SecurityAuthorization.Read || securityAuthorization == SecurityAuthorization.None).ToString().ToLower();

            return toReturn;
        }

        /// <summary>
        /// Initalizes an action with any permissions for actions WITHOUT a proposal
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function to initialize</param>
        /// <param name="pages">The current page</param>
        /// <param name="authorizationRequired">The required authorization</param>
        [NonAction]
        protected void InitializeAction(IES.Common.Logger logger, string functionName, Collection<PtmSecurityPage> pages,
            SecurityAuthorization authorizationRequired)
        {
            this.InitializeAction(logger, functionName, pages, authorizationRequired, null);
        }

        /// <summary>
        /// Initalizes an action with any permissions
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function to initialize</param>
        /// <param name="pages">The current page</param>
        /// <param name="authorizationRequired">The required authorization</param>
        /// <param name="proposalId">the current proposal id</param>
        protected void InitializeAction(IES.Common.Logger logger, string functionName, Collection<PtmSecurityPage> pages,
            SecurityAuthorization authorizationRequired, int? proposalId)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (pages == null)
            {
                throw new ArgumentNullException(nameof(pages));
            }

            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            if (System.Web.HttpContext.Current != null && this.savePermissionActions.Contains(functionName.ToLower()))
            {
                System.Web.HttpContext.Current.Items.Add(IES.Common.CacheConstants.SAVE_PERMISSIONS_ACTION, true);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            logger.Debug(string.Format("Begin " + functionName));
            logger.Performance(string.Format("BEGIN ACTION -" + functionName), 0, true);

            bool authorizationFound = false;
            bool readOnly = true;
            PtmRole roleAllowingAccess = PtmRole.NotSet;  // the role associated with the granting of the authorization
            foreach (PtmSecurityPage page in pages)
            {
                SecurityAuthorizationAndRole authorizationAndRole = this.GenTRACControllerLogic.CheckPermissions(page, proposalId);

                SecurityAuthorization authorization = authorizationAndRole.Authorization;

                // allow admins to have readupdate for approvals 
                if (page == PtmSecurityPage.Approvals && authorization == SecurityAuthorization.Read && authorizationAndRole.Role == PtmRole.Admin)
                {
                    authorization = SecurityAuthorization.ReadUpdate;
                }

                // allow user to delete proposal if they are lead estimator or admin
                if (page == PtmSecurityPage.Proposal && functionName.Equals(WebConstants.Action.DELETE_PROPOSAL, StringComparison.CurrentCultureIgnoreCase) && 
                    (authorizationAndRole.Role == PtmRole.Pricer || authorizationAndRole.Role == PtmRole.Admin))
                {
                    authorization = SecurityAuthorization.CreateReadUpdateDelete;
                }

                // This is where the page-level authorization check succeeds or fails
                if (authorization >= authorizationRequired)
                {
                    authorizationFound = true;
                    roleAllowingAccess = authorizationAndRole.Role;

                    if (authorization == SecurityAuthorization.ReadUpdate || authorization == SecurityAuthorization.CreateReadUpdateDelete)
                    {
                        readOnly = false;
                    }
                }
            }

            if (!authorizationFound)
            {
                throw new AuthorizationException(functionName + " was not authorized");
            }

            if (proposalId != null)
            {
                SiteMasterUtilities.SetProposalContext(proposalId.Value);
            }

            this.ViewBag.ReadOnly = readOnly ? "true" : "false";
            this.ViewBag.RoleAllowingAccess = roleAllowingAccess;

            // Update the user status for Who's Online.
            IES.Common.UserData activeUser = this.securityInformation.ActiveUserData;
            this.ViewData["UserAccountName"] = string.Format("{0} {1} ({2})", activeUser.FirstName, activeUser.LastName, activeUser.Ntid);
			this.ViewData["CurrentNtid"] = activeUser.Ntid;

            this.GenTRACControllerLogic.UpdateUsersStatus(activeUser, proposalId);

            // validate model
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();
            validationErrors.AddRange(SiteMasterUtilities.CreateModelStateValidationErrorList(this.ModelState));
            HttpContext.Items["ValidationErrors"] = validationErrors;

            HttpContext.Items["Stopwatch"] = sw;
        }

        /// <summary>
        /// Finalizes an action logging the performance
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function name being logged</param>
        /// <param name="sw">The running stopwatch</param>
        [NonAction]
        protected void FinalizeAction(IES.Common.Logger logger, string functionName, Stopwatch sw)
        {
            if (sw == null)
            {
                throw new ArgumentNullException(nameof(sw));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            sw.Stop();
            logger.Info(string.Format("Finished " + functionName + ": " + sw.ElapsedMilliseconds + " milliseconds."));
            logger.Performance("ACTION - " + functionName, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Returns the home menu items
        /// </summary>
        /// <param name="controller">Current controller</param>
        /// <returns>Collection of menu items</returns>
        [NonAction]
        private ICollection<GenTRACMenuItemModelView> GetHomeMenuItems(string controller)
        {
            int countOfPendingProposals = this.GenTRACControllerLogic.GetPendingApprovalsCountForUser();

            List<GenTRACMenuItemModelView> toReturn = new List<GenTRACMenuItemModelView>()
            {
                new GenTRACMenuItemModelView
                {
                    LinkText = "Proposals",
                    ActionName = WebConstants.Action.HOME_DISPLAY_GENTRAC_HOME,
                    ControllerName = WebConstants.Controller.HOME,
                    RouteValues = null,
                    HtmlAttributes = null,
                    SecurityPage = PtmSecurityPage.Home,
                    Selected = controller.IsEquivalentTo(WebConstants.Controller.HOME) || controller.IsEquivalentTo(WebConstants.Controller.PROPOSAL)
                },
                new GenTRACMenuItemModelView
                {
                    LinkText = "Reports",
                    ActionName = WebConstants.Action.REPORTS_HOME,
                    ControllerName = WebConstants.Controller.REPORTS,
                    RouteValues = null,
                    HtmlAttributes = null,
                    SecurityPage = PtmSecurityPage.Reports,
                    Selected = controller.IsEquivalentTo(WebConstants.Controller.REPORTS),
                    SubMenuItems = new List<GenTRACMenuItemModelView>()
                    {
                       new GenTRACMenuItemModelView
                       {
                            LinkText = "Proposal Activity",
                            ActionName = WebConstants.Action.REPORTS_PROPOSAL_ACTIVITY,
                            ControllerName = WebConstants.Controller.REPORTS,
                            RouteName = WebConstants.Route.Name.REPORTS,
                            RouteValues = new { action = WebConstants.Action.REPORTS_PROPOSAL_ACTIVITY, controller = WebConstants.Controller.REPORTS },
                            SecurityPage = PtmSecurityPage.Reports
                        },
                        new GenTRACMenuItemModelView
                        {
                            LinkText = "Proposal Log",
                            ActionName = WebConstants.Action.REPORTS_PROPOSAL_LOG,
                            ControllerName = WebConstants.Controller.REPORTS,
                            RouteName = WebConstants.Route.Name.REPORTS,
                            RouteValues = new { action = WebConstants.Action.REPORTS_PROPOSAL_LOG, controller = WebConstants.Controller.REPORTS },
                            SecurityPage = PtmSecurityPage.Reports
                        },
                        new GenTRACMenuItemModelView
                        {
                            LinkText = "DFARS Checklist Non-Standard Responses",
                            ActionName = WebConstants.Action.REPORTS_DFARS,
                            ControllerName = WebConstants.Controller.REPORTS,
                            RouteName = WebConstants.Route.Name.REPORTS,
                            RouteValues = new { action = WebConstants.Action.REPORTS_DFARS, controller = WebConstants.Controller.REPORTS },
                            SecurityPage = PtmSecurityPage.Reports
                        }
                    }
                },
                new GenTRACMenuItemModelView
                {
                    LinkText = "Admin",
                    SubMenuItems = new List<GenTRACMenuItemModelView>()
                    {
                        new GenTRACMenuItemModelView
                        {
                            LinkText = "Bulk Archive",
                            ActionName = WebConstants.Action.DISPLAY_BULK_ARCHIVE,
                            ControllerName = WebConstants.Controller.ADMIN,
                            RouteName = WebConstants.Controller.ADMIN,
                            RouteValues = new { action = WebConstants.Action.DISPLAY_BULK_ARCHIVE, controller = WebConstants.Controller.ADMIN },
                            HtmlAttributes = null,
                            SecurityPage = PtmSecurityPage.Admin
                        },
                        new GenTRACMenuItemModelView 
                        {
                            LinkText = "Manage Proposal Information",
                            ActionName = WebConstants.Action.DISPLAY_MANAGE_PROPOSAL_INFO,
                            ControllerName = WebConstants.Controller.ADMIN,
                            RouteName = WebConstants.Route.Name.ADMIN,
                            RouteValues = new { action = WebConstants.Action.DISPLAY_MANAGE_PROPOSAL_INFO, controller = WebConstants.Controller.ADMIN },
                            HtmlAttributes = null,
                            SecurityPage = PtmSecurityPage.Admin
                        },
                        new GenTRACMenuItemModelView 
                        {
                            LinkText = "Permissions",
                            ActionName = WebConstants.Action.ADMIN_DISPLAY_SYSTEM_PERMISSIONS,
                            ControllerName = WebConstants.Controller.ADMIN,
                            RouteName = WebConstants.Route.Name.ADMIN,
                            RouteValues = new { action = WebConstants.Action.ADMIN_DISPLAY_SYSTEM_PERMISSIONS, controller = WebConstants.Controller.ADMIN },
                            HtmlAttributes = null,
                            SecurityPage = PtmSecurityPage.Admin
                        }
                    },
                    Selected = controller.IsEquivalentTo(WebConstants.Controller.ADMIN)
                },
                new GenTRACMenuItemModelView
                {
                    LinkText = "Who's Online",
                    MenuLocation = MenuLocation.Right,
                    OnClickAction = "WhosOnline(); return false;",
                    ControllerName = WebConstants.Controller.HOME,
                    RouteValues = null,
                    HtmlAttributes = null,
                    SecurityPage = PtmSecurityPage.Admin
                },
                /* new GenTRACMenuItemModelView
                {
                    LinkText = "Help",
                    MenuLocation = MenuLocation.Right,
                    SubMenuItems = new List<GenTRACMenuItemModelView>()
                    {
                        new GenTRACMenuItemModelView
                        {
                            LinkText = "About PTM",
                            LinkUrl = new Uri("http://gentracsupport.isgs.lmco.com/about/")
                        },
                        new GenTRACMenuItemModelView
                        {
                            LinkText = "PTM Help",
                            LinkUrl = new Uri("http://gentracsupport.isgs.lmco.com/help/")
                        }
                    }
                }, */
                new GenTRACMenuItemModelView
                {
                    LinkText = "Clear Cache",
                    MenuLocation = MenuLocation.Left,
                    OnClickAction = "ClearCache(); return false;",
                    ControllerName = WebConstants.Controller.HOME,
                    RouteValues = null,
                    HtmlAttributes = null,
                    SecurityPage = PtmSecurityPage.Admin
                },
                new GenTRACMenuItemModelView
                {
                    LinkText = "My Approvals" + (countOfPendingProposals > 0 ? " (" + countOfPendingProposals.ToString() + ")" : string.Empty),
                    ActionName = WebConstants.Action.DISPLAY_MY_APPROVALS,
                    ControllerName = WebConstants.Controller.HOME,
                    RouteName = WebConstants.Route.Name.HOME,
                    RouteValues = new { action = WebConstants.Action.DISPLAY_MY_APPROVALS, controller = WebConstants.Controller.HOME },
                    HtmlAttributes = null,
                    SecurityPage = PtmSecurityPage.Home
                }
            };

            return toReturn;
        }

        /// <summary>
        /// Filters the menu items based on current permissions
        /// </summary>
        /// <param name="menuItems">Unfiltered menu items</param>
        /// <param name="proposal">Current proposal</param>
        /// <returns>Filitered menu items</returns>
        [NonAction]
        private ICollection<GenTRACMenuItemModelView> FilterMenuItems(ICollection<GenTRACMenuItemModelView> menuItems, ProposalDto proposal)
        {
			List<GenTRACMenuItemModelView> toReturn = new List<GenTRACMenuItemModelView>();

            int? proposalID = proposal != null ? (int?)proposal.Id : null;

            foreach (GenTRACMenuItemModelView menuItem in menuItems)
            {
                // For menu items with no sub items, check access
                // If the menu items has sub items, we'll check access on each
                if (!menuItem.SubMenuItems.Any())
                {
                    // If the user has access to this Security Page, add the menu item to the Model Views
                    SecurityAuthorizationAndRole authorizationAndRole = this.GenTRACControllerLogic.CheckPermissions(menuItem.SecurityPage, proposalID);
                    if (menuItem.ActionName.Length == 0 || authorizationAndRole.Authorization != SecurityAuthorization.None)
                    {
                        toReturn.Add(menuItem);
                    }
                }
                else
                {
					// Create a new model View for the inactive top-level menu item
					GenTRACMenuItemModelView inactiveMenuItem = new GenTRACMenuItemModelView
                    {
                        LinkText = menuItem.LinkText,
                        MenuLocation = menuItem.MenuLocation,
                        Selected = menuItem.Selected
                    };

                    // Iterate the sub items
                    foreach (GenTRACMenuItemModelView subMenuItem in menuItem.SubMenuItems)
                    {
                        // If the user has access to this Security Page, add the sub menu item to the
                        // inactive top-level menu item Model View
                        SecurityAuthorizationAndRole authorizationAndRole = this.GenTRACControllerLogic.CheckPermissions(subMenuItem.SecurityPage, proposalID);
                        if (subMenuItem.ActionName.Length == 0 || authorizationAndRole.Authorization != SecurityAuthorization.None)
                        {
                            inactiveMenuItem.SubMenuItems.Add(subMenuItem);
                        }
                    }

                    // If the inactive top-level menu item has sub items that the user has access to,
                    // we'll add the top-level menu item to the Model Views
                    if (inactiveMenuItem.SubMenuItems.Any())
                    {
                        toReturn.Add(inactiveMenuItem);
                    }
                }
            }

            return toReturn;
        }

        #region Events

        /// <summary>
        /// Override of the default OnActionExecuting, to allow us to do security verification
        /// </summary>
        /// <param name="filterContext">Context</param>
        [NonAction]
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IdentitySwap.IdentitySwappingForTesting.SwapIdentity("IsIdentitySwappingAllowed", "ActiveDirectoryPath", "ADGroupsAllowedToSwapIdentity");
            if (!HttpContext.Items.Contains(IES.Common.DbQueryConstants.MAX_QUERIES))
            {
                HttpContext.Items[IES.Common.DbQueryConstants.MAX_QUERIES] = WebConfigurationManager.AppSettings[IES.Common.DbQueryConstants.MAX_QUERIES];
            }

            if (filterContext != null)
            {
                Dictionary<string, SecurityPageAndAuthorization> actionDictionary;

                // Check to see if we have a dictionary for the controller. If we do, get it. If not, throw an exception
                if (ControllerDictionary.TryGetValue(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower(), out actionDictionary))
                {
                    string functionName = filterContext.ActionDescriptor.ActionName.ToLower();

                    SecurityPageAndAuthorization securityAndAuthorization;
                    if (actionDictionary.TryGetValue(functionName, out securityAndAuthorization))
                    {
                        Collection<PtmSecurityPage> page = new Collection<PtmSecurityPage> { securityAndAuthorization.Page };
                        SecurityAuthorization authorization = securityAndAuthorization.Authorization;

                        int? proposalId = filterContext.ActionParameters.ContainsKey("proposalId") ? filterContext.ActionParameters["proposalId"] as int? : null;

                        this.InitializeAction(this.log, functionName, page, authorization, proposalId);
                    }
                    else
                    {
                        throw new KeyNotFoundException("Specified action has not been registered in the dictionary yet");
                    }
                }
                else
                {
                    throw new KeyNotFoundException("Specified controller has not been registered in the dictionary yet");
                }
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

                // throw exception if there are too many requests
                if (HttpContext.Items.Contains(IES.Common.DbQueryConstants.CURRENT_QUERIES) &&
                    HttpContext.Items.Contains(IES.Common.DbQueryConstants.MAX_QUERIES) && Convert.ToInt32(HttpContext.Items[IES.Common.DbQueryConstants.MAX_QUERIES]) >= 0 &&
                    (int)HttpContext.Items[IES.Common.DbQueryConstants.CURRENT_QUERIES] > Convert.ToInt32(HttpContext.Items[IES.Common.DbQueryConstants.MAX_QUERIES]))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Controller '");
                    sb.Append(filterContext.Controller.ToString().Split('.').Last());
                    sb.Append("' Action '");
                    sb.Append(functionName);
                    sb.Append("' has used ");
                    sb.Append(HttpContext.Items[IES.Common.DbQueryConstants.CURRENT_QUERIES]);
                    sb.Append(" queries.  The max number of queries allowed for this action is ");
                    sb.Append(HttpContext.Items[IES.Common.DbQueryConstants.MAX_QUERIES]);
                    sb.Append(". Please refactor this action.");
                    throw new IES.Common.DbQueryOverloadException(sb.ToString());
                } 
                
                Stopwatch sw = HttpContext.Items["Stopwatch"] as Stopwatch;

                if (sw != null)
                {
                    this.FinalizeAction(this.log, functionName, sw);
                    HttpContext.Items.Remove("Stopwatch");
                }
                else
                {
                    this.FinalizeAction(this.log, functionName, new Stopwatch());
                }
            }

            base.OnActionExecuted(filterContext);
        }

        #endregion
    }
}