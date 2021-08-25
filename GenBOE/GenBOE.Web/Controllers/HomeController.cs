// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using UserDTO = Dtos.UserDTO;

    public class HomeController : GenBOEController
    {
        private Logger _log = new Logger(typeof(HomeController));
        private ValidationFactory _ValidationFactory = null;
        private IGenBOEMetricsDataLoader boeMetricsLoader = null;
        private ISecurityInformation _SecInfo;
        private GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper;
        private GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader;
        private IWorkspaceControllerLogic workspaceLogic;

        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private IHomeControllerLogic homeLogic = null;
        private IWorkspaceDTODataLoader _WorkspaceDTODataLoader = null;
        private ActiveDirectoryUtilities _ADUtils = null;
        private BoePickListMapper boePickListMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public HomeController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            ValidationFactory inValidationFactory,
            IGenBOEMetricsDataLoader boeMetricsLoader,
            ISecurityInformation inSecurityInformation,
            SystemMetrics inSystemMetrics,
            IHomeControllerLogic homeLogic,
            IFullObjectFactory factory,
            UserDTODataLoader inUserDTODataLoader,
            IWorkspaceDTODataLoader inWorkspaceDTODataLoader,
            IPermissionsDTODataLoader permissionsLoader,
            ActiveDirectoryUtilities adUtils,
            IGenBOEControllerLogic inControllerLogic,
            BoePickListMapper boePickListMapper,
            GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader,
            GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper,
            IWorkspaceControllerLogic workspaceLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, permissionsLoader, inControllerLogic)
        {
            _ValidationFactory = inValidationFactory;
            this.boeMetricsLoader = boeMetricsLoader;
            _SecInfo = inSecurityInformation;
            _WorkspaceDTODataLoader = inWorkspaceDTODataLoader;
            this.homeLogic = homeLogic;
            this._ADUtils = adUtils;
            this.boePickListMapper = boePickListMapper;
            this.proposalLoader = proposalLoader;
            this.ptmSecurityMapper = ptmSecurityMapper;
            this.workspaceLogic = workspaceLogic;
        }

        public JsonResult GetUserMetricsModel()
        {
            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
            bool userIsSubcontractor = _SecInfo.IsSubcontractorUser(currentUser.NTID, currentUser.IsSubcontractor);
            GenBOEHomepageModelView theModelView = new GenBOEHomepageModelView();

            if (!userIsSubcontractor)
            {
                //Initialize metrics
                theModelView.isSysAdmin = CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.None;
                theModelView.canCreateWS = CheckPermissions(SecurityPage.CreateWorkspacePermissions, null, null) == SecurityAuthorization.CreateReadUpdateDelete;
                
                // Populate Metrics Grid
                theModelView.workspaceGridRows = _GetHomepageGrid(theModelView.isSysAdmin);
            }

            JsonResult toReturn = Json(theModelView);
            toReturn.MaxJsonLength = int.MaxValue;
            return toReturn;
        }

        public JsonResult DeleteWorkspaces(GenBOEHomepageWorkspaceRowModelView[] toBeDeleted)
        {
            if (toBeDeleted == null || !toBeDeleted.Any())
            {
                throw new ArgumentNullException(nameof(toBeDeleted));
            }
            bool deleteStatus = true;
            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                foreach (GenBOEHomepageWorkspaceRowModelView wsToDelete in toBeDeleted)
                {
                    FullWorkspace ws = this.Factory.CreateFullWorkspace(wsToDelete.WorkspaceShortName);
                    Stopwatch sw = InitializeAction(_log, "DeleteWorkspaces", SecurityPage.WorkspaceDelete, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
                    _WorkspaceDTODataLoader.UpdateDeletedStatus(wsToDelete.WorkspaceId, ws.UpdateDate, true, currentUser.UserID);
                    this.Factory.ClearWorkspaceCache(ws.Shortname);
                    FinalizeAction(_log, "DeleteWorkspaces", sw);
                }
                scope.Complete();
            }

            JsonResult toReturn = Json(new { Status = deleteStatus });
            return toReturn;
        }

        /// <summary>
        /// Gets the tracking numbers allowed for an NT Id.
        /// </summary>
        /// <param name="leadEstimatorNtId">The lead estimator nt identifier.</param>
        /// <returns></returns>
        public JsonResult GetTrackingNumbers(string leadEstimatorNtId, int workspaceId)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceId);
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_HOME_GET_TRACKING_NUMBERS, SecurityPage.WorkspaceRestore, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            IReadOnlyCollection<GenTRAC.DataBridge.Common.Security.SecurityPermissionsResponse> roles = this.ptmSecurityMapper.GetRolesForUser(leadEstimatorNtId);
            bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

            ICollection<ProposalDto> proposals = (isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(leadEstimatorNtId, true))
                                                            .Where(p => !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.NoBid && p.ProposalStatus != ProposalStatus.Revised).ToList();

            Collection<SelectListItem> trackingNumbers = new Collection<SelectListItem>();
            foreach (ProposalDto proposal in proposals)
            {
                trackingNumbers.Add(new SelectListItem
                {
                    Text = proposal.TrackingNumber + " - " + proposal.ProposalTitle,
                    Value = proposal.TrackingNumber
                });
            }
            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_HOME_GET_TRACKING_NUMBERS, sw);

            JsonResult toReturn = Json(new { trackingNumbers = trackingNumbers });
            return toReturn;
        }

        public JsonResult RestorePtmWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
        {
            if (toBeRestored == null)
            {
                throw new ArgumentNullException(nameof(toBeRestored));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName, true);
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE, SecurityPage.WorkspaceRestore, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
            
            // Make sure a tracking number was selected or entered
            if (String.IsNullOrEmpty(toBeRestored.TrackingNumber))
            {
                throw new GenValidationException("A tracking number is required to restore this workspace.");
            }

            // see if this is a valid PTM Tracking Number
            int proposalId = this.proposalLoader.GetIdByTrackingNumber(toBeRestored.TrackingNumber);
            if (proposalId > 0)
            {
                // found the proposal
                GenTRAC.DataBridge.DTO.ProposalDto proposal = this.proposalLoader.GetById(proposalId);

                // Copy over the PTM Proposal values from the Tracking Number and from UI
                ws.CostVolumeLeadPricerUserID = this.UserLoader.GetIdsByNtid(new string[] { toBeRestored.CostVolumeLeadPricerNtId }).First();
                ws.TrackingNumber = toBeRestored.TrackingNumber;
                ws.RFPNumber = proposal.RFPNumber;
                ws.ProposalTitle = proposal.ProposalTitle;
                if (proposal.ContractTypeIds.Any())
                {
                    List<int> contractTypeIds = new List<int>();
                    foreach (int contractTypeId in proposal.ContractTypeIds)
                    {
                        int convertedContractTypeId = this.workspaceLogic.ConvertPTMContractTypeId(contractTypeId);
                        if (convertedContractTypeId > 0)
                        {
                            contractTypeIds.Add(convertedContractTypeId);
                        }
                    }

                    ws.SelectedContractTypes = contractTypeIds.ToArray();
                }

                ws.ProposalClass =  new PickListDto { Id = this.workspaceLogic.ConvertPTMProposalClassId(proposal.ProposalClass) };
                ws.LineOfBusiness = new PickListDto { Id = this.workspaceLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID) };
                ws.ProposalSubmittalDate = proposal.DeliveryDate;
                ws.RevisedSubmittalDate = proposal.RevisedSubmittalDate;
            }
            else
            {
                throw new GenValidationException("PTM Tracking Number is invalid. Please choose another from the dropdown list.");
            }

            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._WorkspaceDTODataLoader.SaveIdentificationAndExportFormat(currentUser.UserID, ws);
                // get the updateDT from DB
                WorkspaceDTO dto = this._WorkspaceDTODataLoader.GetById(toBeRestored.WorkspaceId);
                this._WorkspaceDTODataLoader.UpdateDeletedStatus(toBeRestored.WorkspaceId, dto.UpdateDate, false, currentUser.UserID);
                this.Factory.ClearWorkspaceCache(ws.Shortname);
                scope.Complete();
            }

            bool restoreStatus = true;


            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE, sw);

            JsonResult toReturn = Json(new { Status = restoreStatus });
            return toReturn;
        }

        public JsonResult RestoreWorkspace(GenBOEHomepageWorkspaceRowModelView toBeRestored)
        {
            if (toBeRestored == null)
            {
                throw new ArgumentNullException(nameof(toBeRestored));
            }

            bool restoreStatus = true;

            FullWorkspace ws = this.Factory.CreateFullWorkspace(toBeRestored.WorkspaceShortName);
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_HOME_RESTORE_WORKSPACE, SecurityPage.WorkspaceRestore, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._WorkspaceDTODataLoader.UpdateDeletedStatus(toBeRestored.WorkspaceId, new DateTime(toBeRestored.updateDT), false, currentUser.UserID);
                this.Factory.ClearWorkspaceCache(ws.Shortname);
                scope.Complete();
            }

            FinalizeAction(_log, WebConstants.ACTION_HOME_RESTORE_WORKSPACE, sw);
            JsonResult toReturn = Json(new { Status = restoreStatus });
            return toReturn;
        }

        /// <summary>
        /// Changes the favorite.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="isFavorite">if set to <c>true</c> [is favorite].</param>
        /// <returns>The JsonResult</returns>
        public JsonResult ChangeFavorite(int workspaceId, bool isFavorite)
        {
            try
            {
                // get current user
                UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

                this._WorkspaceDTODataLoader.UpdateFavorite(workspaceId, currentUser.UserID, isFavorite);
            }
            catch (Exception ex)
            {
                this._log.Error(ex);
                throw new GenValidationException("Error saving Favorites");
            }

            JsonResult toReturn = Json(new { Status = true });
            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private ICollection<GenBOEHomepageWorkspaceRowModelView> _GetHomepageGrid(bool isSystemAdmin)
        {
            ICollection<GenBOEHomepageWorkspaceRowModelView> toReturn = new Collection<GenBOEHomepageWorkspaceRowModelView>();

            // get current user
            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            // if you are system admin -> see all
            // else see a WS only if you are WS admin
            if (isSystemAdmin)
            {
                toReturn = this._WorkspaceDTODataLoader.GetAllWsForHomepageGrid(currentUser.UserID);
            }
            else
            {
                // get AD groups that the user belongs to
                ICollection<GroupData> usersGroups = _ADUtils.GetGroupsForUser(currentUser.NTID);

                // figure out which of those groups exist in the DB -> get their ETI User Ids
                List<string> userData = new List<string>();
                foreach (GroupData aGroup in usersGroups)
                {
                    if (!userData.Contains(aGroup.Ntid))
                    {
                        userData.Add(aGroup.Ntid);
                    }
                }

                ICollection<int> userIds = this.UserLoader.GetIdsByNtid(userData);
                userIds.Add(currentUser.UserID);

                ICollection<int> workspaceIdsToWhichUserHasAccess = this.PermissionsLoader.GetWorkspaceIdsThatUsersHaveAccessTo(userIds);
                ICollection<int> workspaceIdsWhereTheUserIsAdmin = this.PermissionsLoader.GetWorkspaceIdsWhereUserIsWorkspaceAdmin(userIds);

                toReturn = this._WorkspaceDTODataLoader.GetWsForHomepageGrid(workspaceIdsToWhichUserHasAccess, workspaceIdsWhereTheUserIsAdmin, currentUser.UserID);
            }
            return toReturn;
        }

        #region Display

        #region Views

        /// <summary>
        /// Returns the Index View in the Home Folder
        /// </summary>
        /// <returns>
        /// If the user is a Subcontractor, return a limited view with only the Getting Started and Announcements sections.
        /// Otherwise, return the Employee view including all sections.
        /// </returns>
        public ViewResult Index()
        {
            ViewData.Add("CanCreateWS", CheckPermissions(SecurityPage.CreateWorkspacePermissions, null, null) == SecurityAuthorization.CreateReadUpdateDelete);

            UserDTO currentUser = this.UserLoader.GetUserForActiveUser();

            // Check for Subcontractor users
            if (_SecInfo.IsSubcontractorUser(currentUser.NTID, currentUser.IsSubcontractor))
            {
                // return limited view for Subcontractors
                return View(WebConstants.VIEW_HOME_GENBOE_INDEX_SUBCONTRACTOR);
            }
            else
            {
                // return full view for Employees
                return View(WebConstants.VIEW_HOME_GENBOE_INDEX);
            }
        }

        /// <summary>
        /// Returns a view with the master page menu
        /// </summary>
        /// <param name="inView">The view to return</param>
        /// <returns></returns>
        public virtual ViewResult DisplayHomeMasterMenu()
        {
            var theModelViews = new Collection<GenBOEMasterMenuItemModelView>();

            Collection<GenBOEMasterMenuItemModelView> MenuItems = GenBOEMasterMenuItemModelView.BuildHomeMasterMenuItems();

            // get the Metric Admin menu so we can add company specific info to it
            var AdminMenuItem = MenuItems.Where(x => x.linkText == "Admin").Single().subMenuItems.Where(y => y.linkText == "Metrics Administration").SingleOrDefault();

            if (AdminMenuItem != null)
            {
                homeLogic.PopulateMetricsAdminCompanySpecificProperties(AdminMenuItem);
            }

            // If the external links are disabled, we need to remove these from the menu, to make the page look right
            if (SiteMasterUtilities.DisableExternalHelpLinksForClassifiedInstallations())
            {
                // ToList makes a copy of this, so we can iterate through it
                foreach (GenBOEMasterMenuItemModelView item in MenuItems.ToList())
                {
                    if (item.securityPage == SecurityPage.AboutToolsMenu
                        || item.securityPage == SecurityPage.HelpMenu
                        || item.securityPage == SecurityPage.ContactMenu)
                    {
                        MenuItems.Remove(item);
                    }
                }
            }

            // Iterate over the static collection of Menu Items defined in GenBOEMasterMenuItemModelView
            foreach (var menuItem in MenuItems)
            {
                // For menu items with no sub items, check access
                if (menuItem.subMenuItems.Count == 0)
                {
                    // If the user has access to this Security Page (which has a given server action), add the menu item to the Model Views
                    // OR if the user has access to a URL link, add the menu item
                    if ((menuItem.actionName.Length > 0 && CheckPermissions(menuItem.securityPage, null, null) != SecurityAuthorization.None) ||
                       (menuItem.linkUrl != null && CheckPermissions(menuItem.securityPage, null, null) != SecurityAuthorization.None))
                    {
                        theModelViews.Add(menuItem);
                    }
                }
                // If the menu items has sub items, we'll check access on each
                else
                {
                    // Create a new model View for the inactive top-level menu item
                    var inactiveMenuItem = new GenBOEMasterMenuItemModelView
                    {
                        linkText = menuItem.linkText
                    };

                    // Iterate the sub items
                    foreach (var subMenuItem in menuItem.subMenuItems)
                    {
                        // If the user has access to this Security Page (which has a given server action), add the sub menu item to the
                        // inactive top-level menu item Model View
                        // OR if the user has access to a URL link, add the menu item
                        if ((subMenuItem.actionName.Length > 0 && CheckPermissions(subMenuItem.securityPage, null, null) != SecurityAuthorization.None) ||
                            (subMenuItem.linkUrl != null && CheckPermissions(subMenuItem.securityPage, null, null) != SecurityAuthorization.None))
                        {
                            inactiveMenuItem.subMenuItems.Add(subMenuItem);
                        }
                    }

                    // If the inactive top-level menu item has sub items that the user has access to,
                    // we'll add the top-level menu item to the Model Views
                    if (inactiveMenuItem.subMenuItems.Count > 0)
                    {
                        theModelViews.Add(inactiveMenuItem);
                    }
                }
            }

            ViewResult toReturn = View(WebConstants.VIEW_HOME_MASTER_MENU, theModelViews);

            return toReturn;
        }

        #endregion Views

        #region Partial Views
        
        /// <summary>
        /// Set up the inital display of "Who's online?".
        /// </summary>
        /// <returns>results for display</returns>
        public ViewResult DisplayWhosOnline()
        {
            GenBOEUsersOnlineDTO systemMetricInfo = boeMetricsLoader.GetOnlineUserDetails();
            WhosOnlineGridModelView viewModel = new WhosOnlineGridModelView(systemMetricInfo);

            return View(WebConstants.VIEW_HOME_WHOS_ONLINE_INDEX, viewModel);
        }

        /// <summary>
        /// Provides paging support of the Who's online dialog.
        /// </summary>
        /// <param name="users">WhosOnlineGridModelView</param>
        /// <returns>Paged ViewResult</returns>
        public ViewResult PageGenBOEMetricsWhosOnline(WhosOnlineGridModelView users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            users.UserResults = new Collection<UserOnlineDetails>();
            users.UsersOnlineDetails = boeMetricsLoader.GetOnlineUserDetails();
            // Set up the list of users for the page clicked on by the user.
            for (int i = users.StartArrayIndex; i <= users.EndArrayIndex; i++)
            {
                // Users are paged by the users NT Id
                UserOnlineDetails user = users.UsersOnlineDetails.UserOnlineDetailsCollection.FirstOrDefault(u => u.Ntid == users.PagedIndexes[i]);
                if (user != null)
                {
                    TimeSpan timeSinceLast = DateTime.Now - user.TimeLastAccessed;
                    if (timeSinceLast.Days < 1) { user.TimeSinceLastAccess = timeSinceLast.ToString("hh\\:mm\\:ss"); }
                    else { user.TimeSinceLastAccess = "More than 24 hours"; }
                    users.UserResults.Add(user);
                }
            }

            // Return the partial view
            ViewResult toReturn = View(WebConstants.VIEW_HOME_WHOS_ONLINE, users);

            return toReturn;
        }

        /// <summary>
        /// Populates Boe Metrics numbers
        /// </summary>
        /// <param name="selectedLob">Selected Line Of Business</param>
        /// <returns>Data</returns>
        public ViewResult GenBoeMetrics()
        {
            GenBOEMetricsDTO DTOtoSend = boeMetricsLoader.GetGenBOEMetrics();
            GenBOEMetricsModelView genBOEMetricsModelView = new GenBOEMetricsModelView(DTOtoSend);

            return View(WebConstants.ACTION_HOME_DISPLAY_METRICS_DETAILS, genBOEMetricsModelView);
        }

        /// <summary>
        /// Converts the selected lo bs.
        /// </summary>
        /// <param name="selectedLobs">The selected lobs.</param>
        /// <returns></returns>
        private int[] ConvertSelectedLOBs(string[] selectedLobs)
        {
            List<int> ids = new List<int>();

            if (selectedLobs != null && selectedLobs.Any())
            {
                PickListGridMV lobMV = this.boePickListMapper.GetPickListValues(PickListEnum.LineOfBusiness);
                foreach (string lob in selectedLobs)
                {
                    if (!string.IsNullOrWhiteSpace(lob))
                    {
                        PickListDto dto = lobMV.PickLists.FirstOrDefault(p => p.Text == lob);
                        if (dto != null)
                        {
                            ids.Add(dto.Id);
                        }
                        else
                        {
                            // somehow the front-end pushed in a bad lob name
                            this._log.Error("Unknown LOB string selected in homepage filter: " + lob);
                        }
                    }
                }
            }

            return ids.ToArray();
        }

        #endregion Partial Views

        #endregion Display

        #region AJAX Calls

        /// <summary>
        /// Calls a custom server validator to validate anything on the UI using [ServerValidation] Attribute in the Model View
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public JsonResult Validate(string value, int type, Collection<Collection<String[]>> data)
        {
            if (data == null) { data = new Collection<Collection<String[]>>(); }
            Collection<Dictionary<String, String>> returnedData = new Collection<Dictionary<string, string>>();

            foreach (Collection<String[]> dicData in data)
            {
                Dictionary<String, String> aDictonary = new Dictionary<String, String>();
                foreach (String[] fields in dicData)
                {
                    aDictonary.Add(fields[0], fields[1]);

                }
                returnedData.Add(aDictonary);
            }

            Validator validator = _ValidationFactory.getValidator((ValidationType)type);
            Collection<String> messages = validator.validation(value, returnedData);

            return Json(messages);
        }

        #endregion AJAX Calls

        #region Active Directory lookup

        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results view</returns>
        public ActionResult Search(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy)
        {
            ICollection<UserData> matchingUsers = string.IsNullOrEmpty(userSearchString) ? new List<UserData>() : this.homeLogic.SearchUsers(userSearchString, searchBy, matchBy);

            List<UserData> theModelView = matchingUsers.OrderBy(ad => ad.DisplayName).ToList();

            return this.PartialView(WebConstants.VIEW_ACTIVE_DIRECTORY_SEARCH, theModelView);
        }

        /// <summary>
        /// Search Active Directory by user account name and return exact match
        /// </summary>
        /// <param name="userAccount">The user's NT account name</param>
        /// <returns>Exact match (only)</returns>
        public JsonResult SearchUserName(string userAccount)
        {
            ICollection<UserData> matchingUsers = string.IsNullOrEmpty(userAccount) ? new List<UserData>() : this.homeLogic.SearchUsers(userAccount, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);

            JsonResult result;

            if (matchingUsers.Count == 1)
            {
                UserData match = matchingUsers.First();

                result = this.Json(new
                {
                    success = true,
                    userAccount = userAccount,
                    userFullName = match.DisplayName,
                    isGroup = match.IsGroup,
                    workPhone = match.Phone
                });
            }
            else
            {
                result = this.Json(new
                {
                    success = false,
                    error = "No exact match was found"
                });
            }

            return result;
        }

        #endregion

        #region Performance Tests

        private static object FileTestLock = new object();

        public ActionResult FileTest(int? seconds, bool? locked)
        {
            string requestId = System.Guid.NewGuid().ToString();
            _log.Debug(string.Format("START FileTest [{0}]", requestId));

            #region Execute the request

            DateTime dtStart = DateTime.Now;

            bool useLocking = locked.HasValue && locked.Value;
            bool lockObtained = false;

            try
            {
                if (useLocking)
                {
                    lockObtained = System.Threading.Monitor.TryEnter(FileTestLock, 1000);
                    _log.Debug(string.Format("FileTest [{0}] - Lock {1}obtained", requestId, lockObtained ? string.Empty : "NOT "));
                }
                else
                {
                    lockObtained = false;
                }

                if (seconds.HasValue && (!useLocking || lockObtained))
                {
                    int msecs = seconds.Value * 1000;
                    _log.Debug(string.Format("FileTest [{0}] - Sleeping for {1} seconds", requestId, seconds.Value));
                    System.Threading.Thread.Sleep(msecs);
                }
            }
            finally
            {
                if (lockObtained)
                {
                    System.Threading.Monitor.Exit(FileTestLock);
                }
            }

            #endregion

            #region Result

            ActionResult result;

            if (!useLocking || lockObtained)
            {
                string elapsedTime = (DateTime.Now - dtStart).ToString();

                _log.Debug(string.Format("END FileTest [{0}], Duration = {1}", requestId, elapsedTime));

                string message = string.Format("FileTest:  Request=[{0}], Duration={1}", requestId, elapsedTime);

                result = this.CreateTextFileWithErrorMessage(message);
            }
            else
            {
                _log.Debug(string.Format("END FileTest [{0}], Another request is current in progress", requestId));

                string message = string.Format("FileTest:  Request=[{0}], Another request is current in progress.", requestId);

                result = this.CreateTextFileWithErrorMessage(message);
            }

            return result;

            #endregion
        }

        #endregion
    }
}
