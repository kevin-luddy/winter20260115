// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.Web.SessionState;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.ModelView;
	using GenBOE.Models;

    [IES.Common.Exceptions.HandleError]
    [SessionState(SessionStateBehavior.Disabled)]
    public class GenBOEController : Controller
    {
        #region Variables

        private const string STOPWATCH = "Stopwatch";

        private Logger _log = new Logger(typeof(GenBOEController));
        private static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> _controllerDictionary;

        protected ISecurityAccess _SecurityAccess { get; set; }
        protected ICommonDataMapper _CommonDataMapper { get; set; }
        protected SiteMasterUtilities _SiteMasterUtilities { get; set; }
        protected SystemMetrics _SystemMetrics { get; set; }
        protected IFullObjectFactory Factory { get; set; }
        protected IUserDTODataLoader UserLoader { get; set; }
        protected IPermissionsDTODataLoader PermissionsLoader { get; set; }
        private IGenBOEControllerLogic _ControllerLogic = null;

        /// <summary>
        /// Dictionary to be used when checking controller's action permissions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        private static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> ControllerDictionary
        {
            get
            {
                if (_controllerDictionary == null || !_controllerDictionary.Any())
                {
                    _controllerDictionary = AuthorizationDictionarySetup.GenerateDictionary();
                }
                return _controllerDictionary;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// No empty constructor allowed
        /// </summary>
        private GenBOEController() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Access to the security APIs</param>
        public GenBOEController(ISecurityAccess inSecurityAccess,
            ICommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader PermissionsLoader,
            IGenBOEControllerLogic inControllerLogic)
        {
            this._SecurityAccess = inSecurityAccess;
            this._CommonDataMapper = inCommonDataMapper;
            this._SiteMasterUtilities = inSiteMasterUtilities;
            this._SystemMetrics = inSystemMetrics;
            this.Factory = factory;
            this.UserLoader = userLoader;
            this.PermissionsLoader = PermissionsLoader;
            this._ControllerLogic = inControllerLogic;
        }

        #endregion

        /// <summary>
        /// Returns a view with the proposal name and workspace status
        /// </summary>
        /// <param name="inView">The view to return</param>
        /// <returns></returns>
        public virtual ViewResult GetMasterView(string inView, string workspace)
        {
            GenBOEMasterModelView model = null;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {

                FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

                model = new GenBOEMasterModelView();
                InitializeMasterViewModel(model, ws);
            }

            return View(inView, model);
        }

        /// <summary>
        /// Initializes the master view model.
        /// </summary>
        /// <param name="model">The model to initialize.</param>
        /// <param name="ws">The workspace to use for initialization.</param>
        protected virtual void InitializeMasterViewModel(GenBOEMasterModelView model, FullWorkspace ws)
        {
            if (ReferenceEquals(model, null))
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (ReferenceEquals(ws, null))
            {
                throw new ArgumentNullException(nameof(ws));
			}

			if (ws.Id == 0)
            {
                // if the workspace doesn't exist .. for example if we are running a system job
                model.ProposalName = "No Workspace";

                model.WorkspaceState = _CommonDataMapper.getWorkspaceStateName(WorkspaceState.None);

                model.HeaderFooter = "Lockheed Martin Proprietary Information";

				model.ContainsOCI = false;
			}
            else
            {
                model.ProposalName = ws.WorkspaceName;

                model.WorkspaceState = _CommonDataMapper.getWorkspaceStateName(ws.WorkspaceState);

                model.HeaderFooter = ws.ContainsOCI ?
                    "Organizational Conflict of Interest - Lockheed Martin Proprietary Information" :
                    "Lockheed Martin Proprietary Information";

				model.ContainsOCI = ws.ContainsOCI;
			}
        }

        /// <summary>
        /// Returns a view with the master page menu
        /// </summary>
        /// <param name="inView">The view to return</param>
        /// <returns></returns>
        public virtual ViewResult DisplaySiteMasterMenu(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			bool hideINLMenuItem = ws.CreationDate > Utilities.ShowINLCutoffDate;

			// Action Initialize
			Stopwatch sw = InitializeAction(_log, "DisplayMasterMenu", SecurityPage.Home, SecurityAuthorization.Read, ws, null);

			Collection<GenBOEMasterMenuItemModelView> theModelViews = new Collection<GenBOEMasterMenuItemModelView>();

            // Iterate over the static collection of Menu Items defined in GenBOEMasterMenuItemModelView
            ICollection<GenBOEMasterMenuItemModelView> MenuItems = GenBOEMasterMenuItemModelView.BuildSiteMasterMenuItems(ws);

			foreach (GenBOEMasterMenuItemModelView menuItem in MenuItems)
            {
                // For menu items with no sub items, check access
                if (!menuItem.subMenuItems.Any())
                {
                    bool menuItemAuthorization = false;

                    // For Home and GenerationHome we don't need to run the checks
                    if (menuItem.securityPage == SecurityPage.Home || menuItem.securityPage == SecurityPage.GenerationHome)
                    {
                        menuItemAuthorization = true;
                    }
                    else
                    {
                        menuItemAuthorization = CheckPermissions(menuItem.securityPage, ws, null) != SecurityAuthorization.None;
                    }

                    // If the user has access to this Security Page (which has a given server action), add the menu item to the Model Views
                    // OR if the user has access to a URL link, add the menu item
                    if (menuItemAuthorization && (menuItem.actionName.Length > 0 || menuItem.linkUrl != null))
                    {
                        theModelViews.Add(menuItem);
                    }
                }
                // If the menu items has sub items, we'll check access on each
                else
                {
					// Create a new model View for the inactive top-level menu item
					GenBOEMasterMenuItemModelView inactiveMenuItem = new GenBOEMasterMenuItemModelView
                    {
                        linkText = menuItem.linkText,
                        menuLocation = menuItem.menuLocation
                    };

					if (menuItem.linkText == "Workspace Administration" && hideINLMenuItem)
					{
						menuItem.subMenuItems = FilterOutINLForms(menuItem.subMenuItems);
					}

					// Iterate the sub items
					foreach (GenBOEMasterMenuItemModelView subMenuItem in menuItem.subMenuItems)
                    {
                        bool submenuItemAuthorization = CheckPermissions(subMenuItem.securityPage, ws, null) != SecurityAuthorization.None;

                        // If the user has access to this Security Page (which has a given server action), add the sub menu item to the
                        // inactive top-level menu item Model View
                        // OR if the user has access to a URL link, add the menu item
                        if (submenuItemAuthorization && (subMenuItem.actionName.Length > 0 || subMenuItem.linkUrl != null))
                        {
                            inactiveMenuItem.subMenuItems.Add(subMenuItem);
                        }
                    }

                    // If the inactive top-level menu item has sub items that the user has access to,
                    // we'll add the top-level menu item to the Model Views
                    if (inactiveMenuItem.subMenuItems.Any())
                    {
                        theModelViews.Add(inactiveMenuItem);
                    }
                }
            }

            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            this.DecideIfNonProjectMapLinksShouldBeVisible(ws);

            this.DecideIfZoneTravelUpdateDataLinkShouldBeVisible(ws, currentUser);

            this.DecideIfOffloadUpdateDataLinkShouldBeVisible(ws, currentUser);

			this.DecideIfUCOTFactorDataLinkShouldBeVisible(ws, currentUser);

			#region Decide if Manage INL Forms should be visible

			bool displayINLForms = false;

            if (SiteMasterUtilities.IsBOEFormVisible)
            {
                bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                            where p.Role == Role.SubcontractorAuthor && p.ETIUserId == currentUser.UserID
                                            select p).Any();
                if (!isSubcontractorUser)
                {
                    displayINLForms = true;
                }
            }

            ViewData["DisplayINLForms"] = displayINLForms;
            ViewData["EnableSAP"] = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);

			#endregion

			ViewResult toReturn = View(WebConstants.VIEW_SITE_MASTER_MENU, theModelViews);

            // Action Finalize
            FinalizeAction(_log, "DisplayMasterMenu", sw);

            return toReturn;
        }

        /// <summary>
        /// Decides if non-ProjectMap links should be visible.
        /// </summary>
        /// <param name="ws">The Workspace.</param>
        public void DecideIfNonProjectMapLinksShouldBeVisible(WorkspaceDTO ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ViewData["DisplayProjectMapOnly"] = ws.IsProjectMapWorkspace;

            ViewData["ReadOnlyMode"] = false;
			if (SiteMasterUtilities.IsReadOnly())
			{
				if (CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.CreateReadUpdateDelete)
				{
                    ViewData["ReadOnlyMode"] = true;
				}
			}
		}

        /// <summary>
        /// Decides whether the Zone Travel Update Link should be displayed
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="currentUser">Current User</param>
        private void DecideIfZoneTravelUpdateDataLinkShouldBeVisible(FullWorkspace ws, UserDTO currentUser)
        {
            bool displayUpdateRates = false;

            // is the data out-of-date
            bool isDataOutOfDate = false;

            isDataOutOfDate = !ws.IsProjectMapWorkspace && _ControllerLogic.AreZoneTravelRatesOutOfDate(ws.Id);

            if (isDataOutOfDate)
            {
                // only show the link if the user is a WS admin
                displayUpdateRates = PermissionsLoader.GetWorkspacePermissions(ws.Id).Any(x => x.Role == Role.WorkspaceAdmin && x.ETIUserId == currentUser.UserID);
            }

            ViewBag.DisplayZoneTravelUpdateRates = displayUpdateRates;
        }

        /// <summary>
        /// Decides whether the Offload Update Link should be displayed
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="currentUser">Current User</param>
        private void DecideIfOffloadUpdateDataLinkShouldBeVisible(FullWorkspace ws, UserDTO currentUser)
        {
            bool displayUpdateRates = false;

            // is the data out-of-date
            bool isDataOutOfDate = false;

            isDataOutOfDate = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload && this._ControllerLogic.AreOffloadRatesOutOfDate(ws.Id);

            if (isDataOutOfDate)
            {
                // only show the link if the user is a WS admin
                displayUpdateRates = this.PermissionsLoader.GetWorkspacePermissions(ws.Id).Any(x => x.Role == Role.WorkspaceAdmin && x.ETIUserId == currentUser.UserID);
            }

            this.ViewBag.DisplayOffloadUpdateRates = displayUpdateRates;
        }

		/// <summary>
		/// Decides whether the Offload Update Link should be displayed
		/// </summary>
		/// <param name="ws">Workspace</param>
		/// <param name="currentUser">Current User</param>
		private void DecideIfUCOTFactorDataLinkShouldBeVisible(FullWorkspace ws, UserDTO currentUser)
		{
			bool displayUpdateRates = false;

			// is the data out-of-date
			bool isDataOutOfDate = false;

			isDataOutOfDate = Utilities.ShowUCOTForWorkspace(ws.CreationDate, ws.TrackingNumber) && this._ControllerLogic.IsUCOTFactorOutOfDate(ws);

			if (isDataOutOfDate)
			{
				// only show the link if the user is a WS admin
				displayUpdateRates = this.PermissionsLoader.GetWorkspacePermissions(ws.Id).Any(x => x.Role == Role.WorkspaceAdmin && x.ETIUserId == currentUser.UserID);
			}

			this.ViewBag.DisplayUpdateUCOTFactor = displayUpdateRates;
		}

		/// <summary>
		/// Filters out the Manage INL Forms menu item if after the cutoff date
		/// </summary>
		/// <param name="currentMenuItems">The collection of MenuItems</param>
		/// <returns>Collection of MenuItems</returns>
		private Collection<GenBOEMasterMenuItemModelView> FilterOutINLForms(Collection<GenBOEMasterMenuItemModelView> currentMenuItems)
		{
			return currentMenuItems.Where(x => x.securityPage != SecurityPage.ManageBOEForms).ToCollection();
		}

		/// <summary>
		/// Display the Error page for general exceptions
		/// </summary>
		/// <returns>Error Page</returns>
		public ActionResult Error()
        {
            return this.View(WebConstants.VIEW_ERROR);
        }

        public ActionResult InvalidRequest()
        {
            return View(WebConstants.VIEW_INVALID_PARAMETERS, new GenBOEMasterModelView());
        }

        /// <summary>
        /// Display the Security Error page for security exceptions
        /// </summary>
        /// <returns>Security Error Page</returns>
        public ActionResult SecurityError()
        {
            return View(WebConstants.VIEW_SECURITY_ERROR, new GenBOEMasterModelView());
        }

        /// <summary>
        /// Check permissions and return the authorization of the user
        /// </summary>
        /// <param name="inPage">The page to check</param>
        /// <param name="inWorkspace">The workspace id (optional, null if not required)</param>
        /// <param name="inBOEId">The BOE ID (optional, null if not required)</param>
        /// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed</returns>
        protected SecurityAuthorization CheckPermissions(SecurityPage inPage, WorkspaceDTO workspace, int? inBOEId)
        {
            Dictionary<SecurityPage, SecurityAuthorization> securityPermission = CheckPermissions(new Collection<SecurityPage>() { inPage }, workspace, inBOEId);

            // NOTE: If you want to turn security "off" uncomment the next line.  everyone accessing the system will get CRUD access...
            // authorizationForUser = SecurityAuthorization.CreateReadUpdateDelete;

            return securityPermission.Values.First();
        }

        /// <summary>
        /// Check permissions and return the authorization of the user
        /// </summary>
        /// <param name="inPage">The page to check</param>
        /// <param name="inWorkspace">The workspace id (optional, null if not required)</param>
        /// <param name="inBOEId">The BOE ID (optional, null if not required)</param>
        /// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed</returns>
        protected Dictionary<SecurityPage, SecurityAuthorization> CheckPermissions(Collection<SecurityPage> inPages, WorkspaceDTO workspace, int? inBOEId)
        {
            if (inPages == null)
            {
                throw new ArgumentNullException(nameof(inPages));
            }
            if (inBOEId.HasValue)
            {
                if (workspace == null) { throw new ArgumentNullException(nameof(workspace), "If BOEId is specified, workspace must be specified as well"); }
                if (!this.Factory.BoeLoader.DoesWorkspaceContainBoe(workspace.Id, inBOEId.Value))
                { throw new InvalidDataRelationException("The requested BOE: " + inBOEId.Value + " does not belong to the current workspace: " + workspace.Id + "."); }
            }

            Dictionary<SecurityPage, SecurityAuthorization> securityDictionary = new Dictionary<SecurityPage, SecurityAuthorization>();
            int? wsId = workspace == null ? null : (int?)workspace.Id;

            UserDTO user = this.UserLoader.GetUserForActiveUser();
            string overrideNonUsString = ConfigurationUtilities.GetAppSetting("OverrideSubNonUs");
            bool overrideNonUs = string.IsNullOrEmpty(overrideNonUsString) ? false : overrideNonUsString.ToLower() == "true";
            bool? isUsPerson = overrideNonUs ? true : user.IsUsPerson;

            if (isUsPerson == null)
            {
                throw new ValidationException("IsUsPerson cannot be null");
            }

            IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = this.Factory.GetPermissionsForUser(user.NTID);

            foreach (SecurityPage page in inPages)
            {
                SecurityAuthorization authorizationForUser;

                if ((bool)isUsPerson)
                {
                    authorizationForUser = _SecurityAccess.IsAuthorized(
                        new SecurityPermissionsRequested { PageToCheck = page, WorkspaceId = wsId, BOEId = inBOEId }, workspace, rolesForUser);
                }
                else
                {
                    throw new UnauthorizedAccessException("Access is denied for non-US users.");
                }

                securityDictionary.Add(page, authorizationForUser);
            }

            return securityDictionary;
        }

        /// <summary>
        /// Get the read only attribute for the given workspace
        /// </summary>
        /// <param name="workspace">The workspace ID</param>
        /// <returns>False if the workspace is in the 'Working' state, true otherwise</returns>
        protected String GetReadOnlyAttribute(SecurityAuthorization securityAuthorization)
        {
            // Default is read-only
            String toReturn = "true";

            // Return true if the Security Authorization is Read or None
            toReturn = (securityAuthorization == SecurityAuthorization.Read || securityAuthorization == SecurityAuthorization.None).ToString().ToLower();

            return toReturn;
        }

        protected Stopwatch InitializeActionWithAnyPermission(Logger logger, string functionName, Collection<SecurityPage> pages, SecurityAuthorization authorizationRequired,
            WorkspaceDTO ws, int? boeID)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (pages == null)
            {
                throw new ArgumentNullException(nameof(pages));
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool authorizationFound = false;
            bool readOnly = true;

            foreach (SecurityPage page in pages)
            {
                SecurityAuthorization authorization = CheckPermissions(page, ws, boeID);

                if (authorization >= authorizationRequired)
                {
                    authorizationFound = true;

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

            if (ws != null && ws.Shortname != null)
            {
                _SiteMasterUtilities.SetWorkspaceInCurrentRequestCache(ws.Shortname);
                ViewData["WorkspaceState"] = ws.WorkspaceState;
                ViewData["ContainsOCI"] = ws.ContainsOCI.ToString().ToLower();
            }

			ViewData["ReadOnlyMode"] = false;
			if (SiteMasterUtilities.IsReadOnly())
			{
				if (CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.CreateReadUpdateDelete)
				{
					ViewData["ReadOnlyMode"] = true;
				}
			}

			ViewData["READONLY"] = readOnly ? "true" : "false";

            return sw;
        }

        /// <summary>
        /// Initializes a controller action.
        /// </summary>
        /// <param name="logger">The logger for the controller calling the action</param>
        /// <param name="functionName">The name of the function being initialized</param>
        /// <param name="page">The security page being initialized</param>
        /// <param name="authorizationRequired">The minimum required to perform the action</param>
        /// <param name="workspace">The workspace shortname</param>
        /// <param name="boeID">The current BOE ID if one exists</param>
        /// <returns>A stopwatch to track the action start</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We do not want to cause issues if the boe metrics user update fails.")]
        protected Stopwatch InitializeAction(Logger logger, string functionName, SecurityPage page,
                                             SecurityAuthorization authorizationRequired, WorkspaceDTO workspace, int? boeID)
        {
            UserData currentUser = null;

            // in case the user is in the middle of running a transaction w/ a save, this could fail and cause an error to be displayed to the user. This will prevent that.
            // this is also not being done in a transaction, to avoid possible issues w/ AD calls taking too long and locking things out.
            try {
                currentUser = this._SystemMetrics.AddCurrentUserDetails();
            } catch { }

            if (workspace != null)
            {
                ViewData["DateOnWhichTheWorkspaceWillBeDeleted"] = workspace.DateDeleted.HasValue ? (DateTime?)workspace.DateDeleted.Value.AddDays(60) : null;
                ViewData["TimeWhenWorkspaceWasLocked"] = workspace.DateRecalculationStarted;
            }

			ViewData["CurrentUserName"] = currentUser != null ? currentUser.DisplayName : string.Empty;

            bool isAdmin = false;
            bool isSystemAdmin = false;
            if(currentUser != null)
            {
                IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(currentUser.Ntid);
                isAdmin = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin || (workspace != null && x.AuthorizedRole == Role.WorkspaceAdmin && x.WorkspaceId == workspace.Id));
                isSystemAdmin = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin);
            }
            ViewData["IsAdmin"] = isAdmin;
            ViewData["IsSystemAdmin"] = isSystemAdmin;

            return InitializeActionWithAnyPermission(logger, functionName, new Collection<SecurityPage>() { page }, authorizationRequired, workspace, boeID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="functionName"></param>
        /// <param name="sw"></param>
        protected void FinalizeAction(Logger logger, string functionName, Stopwatch sw)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (sw != null)
            {
                sw.Stop();
                logger.Performance("ACTION - " + functionName, sw.ElapsedMilliseconds);
            }
            else
            {
                logger.Performance(string.Format("Finished " + functionName + ": " + "This action was not timed."), 0);
            }
        }

        /// <summary>
        /// Generates a formatted response from the upload action.
        /// </summary>
        /// <param name="validationErrors">Validation collection</param>
        /// <returns>Content result</returns>
        protected ContentResult GenerateUploadResponse(Collection<ValidationMessage> validationErrors)
        {
            StringBuilder sb = new StringBuilder();

            if (validationErrors != null && validationErrors.Any())
            {
                foreach (ValidationMessage message in validationErrors)
                {
                    sb.Append(message.ValidationIssue + "</br>");
                }
            }

            return this.GenerateUploadResponse(false, sb.ToString());
        }

        /// <summary>
        /// Generates a formatted response from the Upload action.
        /// </summary>
        /// <param name="status">Whether the upload succeeded or not</param>
        /// <param name="message">The error or success message to pass back to the View</param>
        /// <param name="args">a list </param>
        /// <returns></returns>
        protected ContentResult GenerateUploadResponse(bool status, string response, params string[] messageFormatReplacements)
        {
            return GenerateUploadResponse(status, null, response, messageFormatReplacements);
        }

        /// <summary>
        /// Generates a formatted response from the Upload action. This response is specialized to set
        /// the document.domain on the response iframe for completing AJAX file uploads.
        /// </summary>
        /// <param name="status">Whether the upload succeeded or not</param>
        /// <param name="message">The error or success message to pass back to the View</param>
        /// <param name="args">a list </param>
        /// <returns></returns>
        protected ContentResult GenerateUploadResponse(bool status, object data, string response, params string[] messageFormatReplacements)
        {
            string uploadResponse = "<div id=\"UploadResponse\">";

            // Format the message with string replacements
            string formattedMessage = String.Format(response, messageFormatReplacements);

            // Serialize the JSON response object to pass back to the View. Then prepend the script
            // block and return a ContentResponse
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return this.Content(uploadResponse + serializer.Serialize(new { Status = status, Data = data, Message = formattedMessage }) + "</div>");
        }

        /// <summary>
        /// Generates a formatted response from the Upload action.
        /// </summary>
        /// <param name="status">Whether the upload succeeded or not</param>
        /// <param name="message">The error or success message to pass back to the View</param>
        /// <param name="args">a list </param>
        /// <returns></returns>
        protected JsonResult GenerateJsonUploadResponse(bool status, string response, params string[] messageFormatReplacements)
        {
            return GenerateJsonUploadResponse(status, null, response, messageFormatReplacements);
        }

        /// <summary>
        /// Generates a formatted response from the Upload action.
        /// </summary>
        /// <param name="status">Whether the upload succeeded or not</param>
        /// <param name="message">The error or success message to pass back to the View</param>
        /// <param name="args">a list </param>
        /// <returns></returns>
        protected JsonResult GenerateJsonUploadResponse(bool status, object data, string response, params string[] messageFormatReplacements)
        {
            // Format the message with string replacements
            string formattedMessage = String.Format(response, messageFormatReplacements);

            return this.Json(new { Status = status, Data = data, Message = formattedMessage });
        }

        /// <summary>
        /// Generates a formatted response from the Upload action. This response is specialized to set
        /// the document.domain on the response iframe for completing AJAX file uploads.
        /// </summary>
        /// <param name="status">Whether the upload succeeded or not</param>
        /// <param name="errorMessage">The error message to pass back to the View.</param>
        /// <param name="warningMessage">The warning message to pass back to the View.</param>
        /// <returns></returns>
        protected ContentResult GenerateUploadWithWarningResponse(bool status, string errorMessage, string warningMessage)
        {
            string uploadResponse = "<div id=\"UploadResponse\">";

            // Serialize the JSON response object to pass back to the View. Then prepend the script
            // block and return a ContentResponse
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return this.Content(uploadResponse + serializer.Serialize(new { Status = status, Message = errorMessage, WarningMessage = warningMessage }) + "</div>");
        }

        protected string ConvertToOptionList(IDictionary<string, string> values, bool flipKeys)
        {
            return ConvertToOptionList(values, flipKeys, false);
        }

        protected string ConvertToOptionListWithText(IDictionary<string, string> values, bool flipKeys)
        {
            return ConvertToOptionList(values, flipKeys, true);
        }

        private string ConvertToOptionList(IDictionary<string, string> values, bool flipKeys, bool useText)
        {
            StringBuilder optionsBuilder = new StringBuilder();
            optionsBuilder.Append("<option value=\"\"></option>");

            foreach (KeyValuePair<string, string> value in values)
            {
                optionsBuilder.Append("<option value=\"");
                optionsBuilder.Append(Server.HtmlEncode(flipKeys ? value.Value : value.Key));
                if (useText)
                {
                    optionsBuilder.Append("\" text=\"");
                    optionsBuilder.Append(Server.HtmlEncode(flipKeys ? value.Key : value.Value));
                }
                optionsBuilder.Append("\">");
                optionsBuilder.Append(Server.HtmlEncode(flipKeys ? value.Key : value.Value));
                optionsBuilder.Append("</option>");
            }

            return optionsBuilder.ToString();
        }

        protected string ConvertToOptionList(ICollection<SelectListItem> options)
        {
            StringBuilder optionsBuilder = new StringBuilder();

            if (options != null)
            {
                foreach (SelectListItem option in options)
                {
                    optionsBuilder.Append("<option value=\"");
                    optionsBuilder.Append(Server.HtmlEncode(option.Value));
                    optionsBuilder.Append("\"");
                    if (option.Selected)
                    {
                        optionsBuilder.Append(" selected=\"selected\"");
                    }

                    optionsBuilder.Append(">");
                    optionsBuilder.Append(Server.HtmlEncode(option.Text));
                    optionsBuilder.Append("</option>");
                }
            }

            return optionsBuilder.ToString();
        }

        #region Events

        /// <summary>
        /// Override of the default OnActionExecuting, to allow us to do security verification
        /// </summary>
        /// <param name="filterContext">Context</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IdentitySwap.IdentitySwappingForTesting.SwapIdentity("IsIdentitySwappingAllowed", "ActiveDirectoryPath", "ADGroupsAllowedToSwapIdentity");
            if (!HttpContext.Items.Contains(DbQueryConstants.MAX_QUERIES))
            {
                this.HttpContext.Items[DbQueryConstants.MAX_QUERIES] = ConfigurationUtilities.GetAppSetting<int>(DbQueryConstants.MAX_QUERIES, -1);
            }

            if (filterContext != null)
            {
                this._log.Performance(string.Format("Action Starting. Controller: {0}, Action: {1}, Workspace: {2}, BoeId: {3}, Ntid: {4}",
                            filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                            filterContext.ActionDescriptor.ActionName,
                            filterContext.ActionParameters.ContainsKey("workspace") ? filterContext.ActionParameters["workspace"] as string : "N/A",
                            filterContext.ActionParameters.ContainsKey("boeID") ? ((filterContext.ActionParameters["boeID"] as int?) ?? -99).ToString() : "N/A",
                            System.Threading.Thread.CurrentPrincipal.Identity.Name
                        ), 0);

                ConfigurationUtilities.SetSiteUrlPrefix(filterContext.HttpContext, _log);

                if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.IsEquivalentTo("Home") || filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.IsEquivalentTo("Excel"))
                {
                    Dictionary<string, SecurityPageAndAuthorization> ActionDictionary;

                    // Check to see if we have a dictionary for the controller. If we do, get it. If not, throw an exception
                    if (ControllerDictionary.TryGetValue(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, out ActionDictionary))
                    {
                        string functionName = filterContext.ActionDescriptor.ActionName.ToLower();

                        SecurityPageAndAuthorization securityAndAuthorization;
                        if (ActionDictionary.TryGetValue(functionName, out securityAndAuthorization))
                        {
                            SecurityPage page = securityAndAuthorization.Page;
                            SecurityAuthorization authorization = securityAndAuthorization.Authorization;

                            string workspace = filterContext.ActionParameters.ContainsKey("workspace") ? filterContext.ActionParameters["workspace"] as string : null;
                            int? boeID = filterContext.ActionParameters.ContainsKey("boeID") ? filterContext.ActionParameters["boeID"] as int? : null;

                            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

                            Stopwatch actionStopwatch = InitializeAction(_log, "Event - " + functionName, page, authorization, ws, boeID);

                            HttpContext.Items[STOPWATCH] = actionStopwatch;
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
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Overrides OnActionExecuted to allow us to finalize the action and log the load times.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext != null)
            {
                this._log.Performance(string.Format("Action Finished. Controller: {0}, Action: {1}, Ntid: {2}",
                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                        filterContext.ActionDescriptor.ActionName,
                        System.Threading.Thread.CurrentPrincipal.Identity.Name
                    ), 0);

                string functionName = filterContext.ActionDescriptor.ActionName;

                Stopwatch actionStopwatch = HttpContext.Items[STOPWATCH] as Stopwatch;

                if (actionStopwatch != null)
                {
                    FinalizeAction(_log, "Event - " + functionName, actionStopwatch);
                    HttpContext.Items.Remove(STOPWATCH);
                }
                else
                {
                    FinalizeAction(_log, "Event - " + functionName, null);
                }
            }

            base.OnActionExecuted(filterContext);
        }

        #endregion

        /// <summary>
        /// This method can be used to stream a text file to the browser that contains a series of error messages separated
        /// by newlines.  This is a quick and easy way to alert the user that there was a problem (e.g. an exception thrown)
        /// during file download processing.
        /// </summary>
        /// <param name="errorMessages">List of error messages</param>
        /// <returns>Text file</returns>
        protected ActionResult CreateTextFileWithErrorMessage(params string[] errorMessages)
        {
            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();

            Response.ContentType = "text/plain";
            Response.ContentEncoding = System.Text.Encoding.ASCII;
            Response.AppendHeader("Content-Disposition", string.Format("attachment;filename={0}", "error.txt"));

            byte[] newline = System.Text.Encoding.ASCII.GetBytes("\r\n");

            if (errorMessages != null)
            {
                for (int i = 0; i < errorMessages.Length; i++)
                {
                    byte[] errorContent = System.Text.Encoding.ASCII.GetBytes(errorMessages[i]);
                    Response.OutputStream.Write(errorContent, 0, errorContent.Length);
                    Response.OutputStream.Write(newline, 0, newline.Length);
                }
            }

            Response.OutputStream.Flush();

            return new EmptyResult();
        }

        /// <summary>
        /// This method can be used to stream a text file to the browser that contains a series of error messages separated
        /// by newlines.  This is a quick and easy way to alert the user that there was a problem (e.g. an exception thrown)
        /// during file download processing.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>Text file</returns>
        protected ActionResult CreateTextFileWithErrorMessage(Exception ex)
        {
            if (ex == null)
            {
                return new EmptyResult();
            }
            else if (ConfigurationUtilities.GetAppSetting<bool>("LocalDebug"))
            {
                return this.CreateTextFileWithErrorMessage(ex.ToDisplayString());
            }
            else
            {
				string supportLink = Utilities.ServiceCentralLink();

				return this.CreateTextFileWithErrorMessage(string.Format("An error has occurred.  This might be the result of invalid data.  Try running the 'Validate All BOEs' report, and correct any errors it may find.  If the data is valid, and the error persists, please create a ticket with Helpdesk at {0}.", supportLink));
            }
        }

        /// <summary>
        /// Convert HTML to text.
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns>Text</returns>
        public JsonResult ConvertHtmlToText(string html, bool? returnConvertedText)
        {
            string htmlDecoded = HttpUtility.UrlDecode(html);
            string text = IES.Common.GenBOEUtilities.ConvertHtmlToText(htmlDecoded);

            return Json(new
            {
                Text = returnConvertedText.HasValue && !returnConvertedText.Value ? string.Empty : text,  // return converted text by default (unless explicitly disabled)
                Length = text.Length,
                WhiteSpace = string.IsNullOrWhiteSpace(text)
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// For any view model properties marked as rich-text, remove styling and/or markup that is incompatible with saving.
        /// </summary>
        /// <param name="model">View model to be saved</param>
        /// <returns>List of validation messages, if any</returns>
        /// <seealso cref="IES.Common.RichTextAttribute"/>
        protected ICollection<ValidationMessage> ScrubViewModelRichTextForSave(object model)
        {
            return this._ControllerLogic.ScrubRichTextPropertiesForSave(model);
        }

		/// <summary>
		/// Validates the RTE Answers
		/// </summary>
		/// <param name="answers">The answers to validate.</param>
		/// <param name="sources">The sources for RTE Templates.</param>
		/// <param name="rteSizeLimit">The RTE Size limit for the workspace if overridden.</param>
		/// <returns>Validation warnings.</returns>
		protected ICollection<ValidationMessage> ValidateRteAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<RteCustomTemplateSourceModelView> sources, int? rteSizeLimit)
        {
            return this._ControllerLogic.ValidateRteAnswers(answers, sources, rteSizeLimit);
        }

        /// <summary>
        /// Assemble a JSON response containing a list of validation errors
        /// </summary>
        /// <param name="validationErrors">Errors</param>
        /// <returns>JSON</returns>
        protected JsonResult CreateJsonValidationErrors(Collection<ValidationMessage> validationErrors)
        {
            return Json(new { Status = false, Results = new { ValidationMessages = validationErrors } });
        }

        /// <summary>
        /// Scrub rich-text markup to remove unwanted items and convert image tag src-attribute route values to base-64.
        /// </summary>
        /// <param name="html">Rich-text markup</param>
        /// <returns>Scrubbed rich-text</returns>
        public ContentResult PreProcessRichTextPaste(string html)
        {
            string htmlDecoded = HttpUtility.UrlDecode(html);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            string scrubbedHtml = this._ControllerLogic.ScrubRichTextForPaste(htmlDecoded, validationMessages);

            return new ContentResult
            {
                Content = scrubbedHtml,
                ContentType = "text/html",
                ContentEncoding = System.Text.Encoding.UTF8
            };
        }

        public ContentResult GetBase64ImageString()
        {
            string base64ImageString = string.Empty;

            if (Request.Files.Count > 0)
            {
                base64ImageString = GenImageUtilities.ConvertImageToBase64SrcString(Request.Files[0]);
            }

            return new ContentResult
            {
                Content = base64ImageString,
                ContentType = "text/plain",
                ContentEncoding = System.Text.Encoding.UTF8
            };
        }

        /// <summary>
        /// Confirms a message by a User
        /// </summary>
        /// <param name="messageId">The message Id to confirm</param>
        public void ConfirmMessage(ConfirmationMessage message)
        {
            if (message == ConfirmationMessage.NOT_APPLICABLE)
            {
                _log.Error("Unknown Confirmation Message");
            }
            else
            {
                UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

				// save to Database
				this.UserLoader.SaveMessageConfirmation(currentUser.UserID, message);
			}
        }

        /// <summary>
        /// Returns a JSON object for the data
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <param name="contentType">The content type to return.</param>
        /// <param name="contentEncoding">The encoding to utilize.</param>
        /// <param name="behavior">The request behavior.</param>
        /// <returns></returns>
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}
