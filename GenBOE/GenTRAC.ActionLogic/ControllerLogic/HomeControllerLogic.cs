// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.CacheWarming;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Home;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using IES.Common;

    /// <summary>
    /// The business logic for the Home Controller
    /// </summary>
    public class HomeControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// The number of seconds to store in cache
        /// </summary>
        private int permissionsSecondsToCache = 60;

        /// <summary>
        /// The permission lock
        /// </summary>
        private object permissionLock = new object();

        /// <summary>
        /// The security loader
        /// </summary>
        private GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader securityLoader;

        /// <summary>
        /// The BOE security access.
        /// </summary>
        private GenBOE.DataBridge.Common.Interfaces.ISecurityAccess boeSecurityAccess;

        /// <summary>
        /// Cache key for User's permissions.
        /// </summary>
        private string cacheKeyUsersPermissions = "UsersPermissions_";

        /// <summary>
        /// The workspace dto data loader.
        /// </summary>
        private GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader workspaceDTODataLoader;
        
        /// <summary>
        /// The AD utils
        /// </summary>
        private IES.Common.IActiveDirectoryUtilities activeDirectoryUtils = null;

        /// <summary>
        /// Org structure mapper for Program Areas and Lines of Business
        /// </summary>
        private IOrgStructureDataMapper orgStructureMapper = null;

        /// <summary>
        /// Cache
        /// </summary>
        private IES.Common.ICache cache = null;

        /// <summary>
        /// Cache Warmer
        /// </summary>
        private ICacheWarmer cacheWarmer = null;
        
        /// <summary>
        /// The name of the proposal grid containing the proposal search box - needed for validation
        /// </summary>
        public const string PROPOSAL_GRID_FORM = "proposalGridForm";

        /// <summary>
        /// The name of the proposal filters secion - needed for validation
        /// </summary>
        public const string PROPOSAL_FILTERS_FORM = "proposalFiltersForm";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="inActiveDirectoryUtils">AD Utils</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="inOrgStructureMapper">Org Structure mapper</param>
        /// <param name="inCache">cache</param>
        /// <param name="inCacheWarmer">cache warmer</param>
        /// <param name="approvalsLoader">approvals loader</param>
        /// <param name="proposalChecklistLoader">proposal checklist loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inProposalMediator">Proposal Mediator</param>
        /// <param name="workspaceDTODataLoader">The Workspace DTO Data Loader</param>
        /// <param name="securityLoader">The BOE Security Loader</param>
        /// <param name="boeSecurityAccess">The BOE Security Access</param>
        public HomeControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IUserMapper inUserMapper,
            IES.Common.IActiveDirectoryUtilities inActiveDirectoryUtils,
            IFullObjectFactory objectFactory,
            IOrgStructureDataMapper inOrgStructureMapper,
            IES.Common.ICache inCache,
            ICacheWarmer inCacheWarmer,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator inChecklistMediator,
            IProposalMediator inProposalMediator,
            GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader workspaceDTODataLoader,
            GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader securityLoader,
            GenBOE.DataBridge.Common.Interfaces.ISecurityAccess boeSecurityAccess)
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
        {
            this.activeDirectoryUtils = inActiveDirectoryUtils;
            this.orgStructureMapper = inOrgStructureMapper;
            this.cache = inCache;
            this.cacheWarmer = inCacheWarmer;
            this.workspaceDTODataLoader = workspaceDTODataLoader;
            this.securityLoader = securityLoader;
            this.boeSecurityAccess = boeSecurityAccess;
        }

        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results view</returns>
        public ICollection<IES.Common.UserData> SearchUsers(string userSearchString, IES.Common.ActiveDirectorySearchBy searchBy, IES.Common.ActiveDirectoryMatchType matchBy)
        {
            return this.activeDirectoryUtils.SearchUsers(userSearchString, searchBy, matchBy);
        }

        /// <summary>
        /// Rule to show/hide new proposal button on My Proposals page
        /// </summary>
        /// <returns>Flag to show/hide new proposal button flag</returns>        
        public HomeProposalModelView GetRolesForNewProposal()
        {
            HomeProposalModelView result = new HomeProposalModelView();

            // check if the current user has the proper system roles to create a new proposal
            result.AllowNewProposal = this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);

            if (!result.AllowNewProposal)
            {
                result.AllowNewProposal = this.SecurityAccess.CurrentUserHasRole(PtmRole.SystemPricer, null);
            }

            return result;
        }

        /// <summary>
        /// Get data for home proposal grid
        /// </summary>
        /// <param name="sortField">field to sort</param>
        /// <param name="order"> field order></param>
        /// <param name="filtersCookie">Proposal filters cookie</param>
        /// <param name="searchText">search text</param>
        /// <returns>home proposal grid</returns>
        public HomeProposalModelView GetDataForHomeProposal(string sortField, System.Data.SqlClient.SortOrder? order, ProposalFiltersCookie filtersCookie, string searchText)
        {
            if (filtersCookie == null)
            {
                throw new ArgumentNullException(nameof(filtersCookie));
            }

            HomeProposalModelView result = new HomeProposalModelView();

            // get the current user
            UserDTO currentUser = this.UserMapper.GetActiveUser();
            bool isSystemAdmin = this.CheckPermissions(PtmSecurityPage.Admin, null).Role == PtmRole.Admin;

            // Get the dates from the filter if they exist.
            DateTime? filterStartDate = null;
            DateTime? filterEndDate = null;
            if (!string.IsNullOrEmpty(filtersCookie.FilterStartDate))
            {
                filterStartDate = filtersCookie.FilterStartDate.ToDateTime("MM/dd/yyyy");
                filterEndDate = filtersCookie.FilterEndDate.ToDateTime("MM/dd/yyyy");
            }

            // Convert the Proposal Status filter option to the correct ProposalStatus type.
            ICollection<int> statuses = new List<int>();
            if (filtersCookie.FilterOption == ProposalFilterOption.InProgress)
            {
                statuses.Add((int)ProposalStatus.InProgress);
                statuses.Add((int)ProposalStatus.Submitted);
            }
            else if (filtersCookie.FilterOption == ProposalFilterOption.Completed)
            {
                statuses.Add((int)ProposalStatus.Completed);
            }
            else if (filtersCookie.FilterOption == ProposalFilterOption.NoBid)
            {
                statuses.Add((int)ProposalStatus.NoBid);
            }

            // Get the Proposal Class filter option as appropriate.  If less than zero, use null (which returns all Proposal Classes).
            int? proposalClassFilterID = null;
            if (filtersCookie.ProposalClassFilterOption > 0)
            {
                proposalClassFilterID = (int)filtersCookie.ProposalClassFilterOption;
            }

            // if user wants to showProposalsForMyOrganization, then generate a list of their AD groups (as XML) to pass to proposal search.
            bool showProposalsForMyOrganization = filtersCookie.ViewerFilterOption == ViewerProposalFilterOption.ShowProposalsForMyOrganization;
            string userAndGroupIdsAsXml = showProposalsForMyOrganization ? this.activeDirectoryUtils.GetUserAndGroupIdsAsXml(currentUser.Ntid, this.activeDirectoryUtils.GetGroupsForUser(currentUser.Ntid)) : string.Empty;

            ICollection<HomeProposalViewDto> allProposalData;
            if (statuses.Any())
            {
                List<HomeProposalViewDto> allProposalDataList = new List<HomeProposalViewDto>();
                foreach (int status in statuses)
                {
                    ICollection<HomeProposalViewDto> data = this.ProposalLoader.GetProposalsByUser(status, filterStartDate, filterEndDate, searchText, currentUser.Ntid, showProposalsForMyOrganization, userAndGroupIdsAsXml, proposalClassFilterID);
                    if (data != null && data.Any())
                    {
                        allProposalDataList.AddRange(data);
                    }
                }

                allProposalData = allProposalDataList;
            }
            else
            {
                allProposalData = this.ProposalLoader.GetProposalsByUser(null, filterStartDate, filterEndDate, searchText, currentUser.Ntid, showProposalsForMyOrganization, userAndGroupIdsAsXml, proposalClassFilterID);
            }
            
            #region Setup Row Data

            foreach (HomeProposalViewDto proposal in allProposalData)
            {
                #region Setup a new MV Row

                result.DataRows.Add(new HomeProposalGridModelView()
                {
                    ProposalId = proposal.ProposalId,
                    ProposalTitle = proposal.ProposalTitle,
                    TrackingNumber = proposal.TrackingNumber,
                    ForecastedTrackingNumber = proposal.ForecastedTrackingNumber,
                    EstValue = proposal.EstValue,
                    CaptureManagerDisplayName = proposal.CaptureManagerDisplayName,
                    PricerDisplayName = proposal.PricerDisplayName,
                    PeerReviewerDisplayName = proposal.PeerReviewerDisplayName,
                    CostVolumeLeadDisplayName = proposal.CostVolumeLeadDisplayName,
                    ProposalDateAssigned = proposal.ProposalDateAssigned,
                    ProposalDueDate = proposal.ProposalDueDate,
                    ProposalCompletedDate = proposal.ProposalSubmittalDate,
                    ChecklistCompleteDate = proposal.ChecklistCompleteDate,
                    Status = proposal.Status,
                    ProgramArea = proposal.ProgramArea,
                    Customer = proposal.Customer,
                    IsCommercialCustomer = proposal.IsCommercialCustomer,
                    HasLinkedDocument = proposal.HasLinkedDocument,
                    IsForecastProposal = proposal.IsForecastProposal,
                    HasWriteAccessToLinkedDocument = isSystemAdmin || proposal.HasWriteAccessToLinkedDocument,
                    IsDeleteAllowed = (isSystemAdmin || this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposal.ProposalId)) && !proposal.HasLinkedDocument
                });

                #endregion
            }

            #endregion

            if (string.IsNullOrEmpty(sortField))
            {
                result.SortField = HomeProposalModelView.DEFAULT_SORT;
                result.Order = System.Data.SqlClient.SortOrder.Ascending;
            }
            else
            {
                result.SortField = sortField;
                result.Order = order ?? System.Data.SqlClient.SortOrder.Ascending;
            }

            result.Sort();

            // get dynamic help text for ProgramAreas
            result.ProgramAreaHelpText = this.orgStructureMapper.GetProgramAreaDynamicHelpText();

            // pull information from BOE
            ICollection<GenBOE.Dtos.WorkspaceDTO> workspaces = this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo();
            foreach (HomeProposalGridModelView row in result.DataRows)
            {
                if (row.TrackingNumber != null)
                {
                    row.Workspaces = workspaces.Where(n => n.TrackingNumber == row.TrackingNumber).Select(t => t.Shortname).OrderBy(s => s).ToList();
                    if (row.Workspaces.Any())
                    {
                        row.IsDeleteAllowed = false;
                    }
                }
            }

            // Figure out if user has access to show the Create WS link
            string activeUserNtid = this.UserMapper.GetActiveUser().Ntid;
            IReadOnlyCollection<GenBOE.DataBridge.Common.SecurityPermissionsResponse> rolesForUser = this.GetBOEPermissionsForUser(activeUserNtid);

            IES.Common.SecurityAuthorization authorizationForUser = this.boeSecurityAccess.IsAuthorized(
                    new GenBOE.DataBridge.Common.SecurityPermissionsRequested { PageToCheck = IES.Common.SecurityPage.CreateWorkspacePermissions }, null, rolesForUser);
            
            result.CanCreateBOEWorkspace = authorizationForUser == IES.Common.SecurityAuthorization.CreateReadUpdateDelete;

            return result;
        }

        /// <summary>
        /// Gets permissions for the specified user
        /// </summary>
        /// <param name="ntId">The nt identifier.</param>
        /// <returns>Permissions for the user</returns>
        private IReadOnlyCollection<GenBOE.DataBridge.Common.SecurityPermissionsResponse> GetBOEPermissionsForUser(string ntId)
        {
            Collection<GenBOE.DataBridge.Common.SecurityPermissionsResponse> data = null;
            string key = this.cacheKeyUsersPermissions + ntId;

            object fromCache = this.cache.GetData(key);

            if (fromCache == null)
            {
                lock (this.permissionLock)
                {
                    fromCache = this.cache.GetData(key);

                    if (fromCache == null)
                    {
                        fromCache = this.securityLoader.GetPermissionsForUser(ntId);

                        if (fromCache != null)
                        {
                            this.cache.Add(key, fromCache, this.permissionsSecondsToCache);
                        }
                    }
                }
            }

            data = fromCache as Collection<GenBOE.DataBridge.Common.SecurityPermissionsResponse>;

            return data.ToList().AsReadOnly();
        }

        /// <summary>
        /// Get data for proposal filters
        /// </summary>
        /// <returns>Home proposal filters model view</returns>
        public HomeProposalFiltersModelView GetDataForProposalFilters()
        {
            HomeProposalFiltersModelView result = new HomeProposalFiltersModelView();

            // populate filter options
            result.FilterOptions = new Collection<SelectListItem>();
            foreach (ProposalFilterOption option in Enum.GetValues(typeof(ProposalFilterOption)))
            {
                result.FilterOptions.Add(new SelectListItem { Text = option.GetDescription(), Value = option.ToString() });
            }

            // Populate Proposal Class filter options.
            result.ProposalClassFilterOptions = new Collection<SelectListItem>();
            foreach (ProposalClassFilterOption option in Enum.GetValues(typeof(ProposalClassFilterOption)))
            {
                result.ProposalClassFilterOptions.Add(new SelectListItem { Text = option.GetDescription(), Value = option.ToString() });
            }

            // populate viewer filter options if the user has viewer role or is in a group that has viewer role
            bool isViewer = this.SecurityAccess.CurrentUserHasRole(PtmRole.Viewer, null);

            result.IsViewer = isViewer.ToString().ToLower();
            result.ViewerFilterOptions = new Collection<SelectListItem>();
            if (isViewer)
            {
                foreach (ViewerProposalFilterOption option in Enum.GetValues(typeof(ViewerProposalFilterOption)))
                {
                    result.ViewerFilterOptions.Add(new SelectListItem { Text = option.GetDescription(), Value = option.ToString() });
                }
            }

            return result;
        }

        /// <summary>
        /// populate the SSRS report for the genTrac home page
        /// </summary>
        /// <param name="reportParameters">json stringified export parameters</param>
        /// <returns>Uri parameters to open the SSRS report in</returns>
        public Uri PopulateExportSSRSParameters(ExportProposalReportModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);

            // get proposal status, send null if All is selected
            if (!(reportParameters.FilterOption == ProposalFilterOption.All.ToString()))
            {
                ProposalFilterOption option = (ProposalFilterOption)Enum.Parse(typeof(ProposalFilterOption), reportParameters.FilterOption);
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS_ID, (int)option + 1));
            }

            if (!string.IsNullOrEmpty(reportParameters.FilterStartDate) && reportParameters.FilterStartDate != Constants.Report.PROPOSAL_DATE_FORMAT)
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.FILTER_START_DATE, reportParameters.FilterStartDate));
            }

            if (!string.IsNullOrEmpty(reportParameters.FilterStartDate) && reportParameters.FilterEndDate != Constants.Report.PROPOSAL_DATE_FORMAT)
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.FILTER_END_DATE, reportParameters.FilterEndDate));
            }

            if (!(reportParameters.FilterProposalClass == ProposalClassFilterOption.All.ToString()))
            {
                ProposalClassFilterOption option = (ProposalClassFilterOption)Enum.Parse(typeof(ProposalClassFilterOption), reportParameters.FilterProposalClass);
                sb.Append(string.Format("&{0}={1}", Constants.Report.FILTER_PROPOSAL_CLASS, (int)option));
            }

            if (!string.IsNullOrEmpty(reportParameters.SearchText))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.SEARCH_TEXT, reportParameters.SearchText));
            }

            UserDTO user = this.UserMapper.GetActiveUser();
            sb.Append(string.Format("&{0}={1}", Constants.Report.NTID, user.Ntid));
            
            Uri toReturn = new Uri(string.Format("{0}/{1}/{2}{3}", WebConfigurationManager.AppSettings["ReportServerLocation"], WebConfigurationManager.AppSettings["ReportServerFolderName"], "Proposal Dashboard Report", sb));
            return toReturn;
        }

        /// <summary>
        /// Get Proposal ID by Tracking Number
        /// </summary>
        /// <param name="inTrackingNumber">Tracking Number</param>
        /// <returns>Proposal Id</returns>
        public int GetProposalIdByTrackingNumber(string inTrackingNumber)
        {
            return this.ProposalLoader.GetIdByTrackingNumber(inTrackingNumber);
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void ClearCache()
        {
            this.cache.ClearCache();
            this.cacheWarmer.DoWarmCache();
        }

        /// <summary>
        /// Gets my approvals
        /// </summary>
        /// <returns>My Approval Data</returns>
        public ICollection<RequiredApprovalsModelView> GetMyApprovals()
        {
            List<RequiredApprovalsModelView> allApprovals = this.ApprovalsLoader.GetApprovalsForUser(this.UserMapper.GetActiveUser().Id)
                .Select(x => new RequiredApprovalsModelView(x)) // convert from DTO to an MV
                .OrderBy(x => x.ApprovedDate.HasValue ? 1 : 0) // put the pending approvals on top
                    .ThenBy(x => x.TrackingNumber) // now sort by tracking number
                .ToList();

            List<RequiredApprovalsModelView> approvalsToDisplay = new List<RequiredApprovalsModelView>();

            foreach (RequiredApprovalsModelView approval in allApprovals)
            {
                ProposalDto proposal = this.GetByProposalId(approval.ProposalId);

                switch(approval.UserRole)
                {
                    case PtmRole.Pricer:
                        approvalsToDisplay.Add(approval);
                        break;
                    case PtmRole.CoverSheetApprover:
                    case PtmRole.PricingVerification:
                    case PtmRole.PeerReviewer:
                        if(proposal.LeadEstimatorSignedDate != null)
                        {
                            approvalsToDisplay.Add(approval);
                        }

                        break;
                    case PtmRole.LOBEstLead:
                        bool hasIndependentReviewer = this.ObjectFactory.CreateFullProposal(proposal).Permissions.Any(p => p.Role == PtmRole.PeerReviewer);
                        bool coverSheetNeeded = !proposal.IsCCPDRequired.HasValue || proposal.IsCCPDRequired.Value == true;
                        if (proposal.LeadEstimatorSignedDate != null && (!coverSheetNeeded || proposal.CoverSheetApproverSignedDate != null) && proposal.PricingVerifierSignedDate != null && 
                            ((!hasIndependentReviewer && coverSheetNeeded) || proposal.IndependentReviewerSignedDate != null))
                        {
                            approvalsToDisplay.Add(approval);
                        }

                        break;
                    default:
                        break;
                }
            }

            return approvalsToDisplay;
        }
    }
}