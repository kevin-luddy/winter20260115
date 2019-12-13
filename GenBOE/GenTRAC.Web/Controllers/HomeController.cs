// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.Home;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : GenTRACController
    {
        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private HomeControllerLogic homeLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSiteMasterUtilities">Site Master Utilities</param>
        /// <param name="inGenTRACControllerLogic">Controller Logic</param>
        /// <param name="inHomeLogic">Home Controller Logic</param>
        /// <param name="inSecurityInformation">security information about user and their context</param>
        public HomeController(
            SiteMasterUtilities inSiteMasterUtilities,
            GenTRACControllerLogic inGenTRACControllerLogic,
            HomeControllerLogic inHomeLogic,
            ISecurityInformation inSecurityInformation)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.homeLogic = inHomeLogic;
        }

        /// <summary>
        /// GenTRAC Home
        /// </summary>
        /// <returns>App home page</returns>
        public ActionResult DisplayGenTRACHome()
        {
            return this.View(WebConstants.View.HOME_GENTRAC_HOME);
        }

        /// <summary>
        /// Home proposal grid
        /// </summary>
        /// <returns>home proposal</returns>
        public PartialViewResult DisplayHomeProposal()
        {
            HomeProposalModelView model = this.homeLogic.GetRolesForNewProposal();
            return this.PartialView(WebConstants.View.HOME_PROPOSAL, model);
        }

        /// <summary>
        /// Home proposal grid
        /// </summary>
        /// <param name="sortField">sort field</param>
        /// <param name="order">sort order</param>
        /// <param name="filtersModelView">Filters model view</param>
        /// <param name="searchText">search text</param>
        /// <returns>home proposal grid</returns>
        public PartialViewResult DisplayHomeProposalGrid(string sortField, System.Data.SqlClient.SortOrder? order, HomeProposalFiltersModelView filtersModelView, string searchText)
        {
            // first, make sure there are no filter validation errors (like with dates) 
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            ProposalFiltersCookie cookie = null;
            if (filtersModelView != null)
            {
                this.SaveProposalFiltersCookie(filtersModelView);
                cookie = new ProposalFiltersCookie()
                {
                    FilterOption = filtersModelView.FilterOption,
                    ViewerFilterOption = filtersModelView.ViewerFilterOption,
                    FilterStartDate = filtersModelView.FilterStartDate,
                    FilterEndDate = filtersModelView.FilterEndDate,
                    ProposalClassFilterOption = filtersModelView.ProposalClassFilterOption
                };
            }
            else
            {
                cookie = this.GetProposalFiltersCookie();
            }

            HomeProposalModelView model = this.homeLogic.GetDataForHomeProposal(sortField, order, cookie, searchText);
            return this.PartialView(WebConstants.View.HOME_PROPOSAL_GRID, model);
        }

        /// <summary>
        /// Home proposal filters
        /// </summary>
        /// <returns>View for proposal filters</returns>
        public PartialViewResult DisplayHomeProposalFilters()
        {
            HomeProposalFiltersModelView model = this.homeLogic.GetDataForProposalFilters();

            ProposalFiltersCookie cookie = this.GetProposalFiltersCookie();
            model.FilterOption = cookie.FilterOption;
            model.ViewerFilterOption = cookie.ViewerFilterOption;
            model.FilterStartDate = cookie.FilterStartDate;
            model.FilterEndDate = cookie.FilterEndDate;

            return this.PartialView(WebConstants.View.HOME_PROPOSAL_FILTERS, model);
        }
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

            return this.PartialView(WebConstants.View.ACTIVE_DIRECTORY_SEARCH, theModelView);
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
                    isGroup = match.IsGroup
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

        /// <summary>
        /// This method will check the DB
        /// </summary>
        /// <returns>junk for now</returns>
        public int CheckForEmail()
        {
            //// This method will poll the DB for emails that need to be sent.
            int i = 0;

            return i;
        }

        /// <summary>
        /// Get proposal filters cookie
        /// </summary>
        /// <returns>Proposal filters cookie object</returns>
        private ProposalFiltersCookie GetProposalFiltersCookie()
        {
            ProposalFiltersCookie proposalFilters = new ProposalFiltersCookie();
            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.ProposalFiltersCookieConstants.COOKIE_KEY];
            if (cookie != null)
            {
                proposalFilters.FilterOption = (ProposalFilterOption)Enum.Parse(typeof(ProposalFilterOption), cookie.Values[WebConstants.ProposalFiltersCookieConstants.OPTION]);
                proposalFilters.ViewerFilterOption = cookie.Values[WebConstants.ProposalFiltersCookieConstants.VIEWER_OPTION] == null ? ViewerProposalFilterOption.ShowOnlyMyProposals : (ViewerProposalFilterOption)Enum.Parse(typeof(ViewerProposalFilterOption), cookie.Values[WebConstants.ProposalFiltersCookieConstants.VIEWER_OPTION]);
                proposalFilters.FilterStartDate = cookie.Values[WebConstants.ProposalFiltersCookieConstants.START_DATE];
                proposalFilters.FilterEndDate = cookie.Values[WebConstants.ProposalFiltersCookieConstants.END_DATE];
            }

            return proposalFilters;
        }

        /// <summary>
        /// Save the proposal filters cookie
        /// </summary>
        /// <param name="filtersModelView">Filters model view</param>
        private void SaveProposalFiltersCookie(HomeProposalFiltersModelView filtersModelView)
        {
            // save cookie
            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[WebConstants.ProposalFiltersCookieConstants.COOKIE_KEY] ??
                new HttpCookie(WebConstants.ProposalFiltersCookieConstants.COOKIE_KEY);
            cookie.Values[WebConstants.ProposalFiltersCookieConstants.OPTION] = filtersModelView.FilterOption.ToString();
            cookie.Values[WebConstants.ProposalFiltersCookieConstants.VIEWER_OPTION] = filtersModelView.ViewerFilterOption.ToString();
            cookie.Values[WebConstants.ProposalFiltersCookieConstants.START_DATE] = filtersModelView.FilterStartDate;
            cookie.Values[WebConstants.ProposalFiltersCookieConstants.END_DATE] = filtersModelView.FilterEndDate;
            cookie.Expires = DateTime.Now.AddDays(180);
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Go to the proposal identified by the given tracking number.  If proposal does not exist a "not found" error is provided.
        /// If the user does not have permissions to this proposal a "security" error is provided.
        /// </summary>
        /// <param name="inProposalTrackingNumber">Tracking number to navigate to</param>
        /// <returns>Json result with status of request</returns>
        public JsonResult GoToProposal(string inProposalTrackingNumber)
        {
            if (string.IsNullOrWhiteSpace(inProposalTrackingNumber))
            {
                // do nothing
                return this.Json(new { Result = string.Empty });
            }

            int proposalId = this.homeLogic.GetProposalIdByTrackingNumber(inProposalTrackingNumber);
            if (proposalId < 0)
            {
                // proposal does not exist
                return this.Json(new { Result = "not_found" });
            }

            SecurityAuthorizationAndRole authorization = this.homeLogic.CheckPermissions(PtmSecurityPage.Proposal, proposalId);
            if (authorization.Authorization == SecurityAuthorization.None)
            {
                return this.Json(new { Result = "access_denied" });
            }

            return this.Json(new { Result = proposalId.ToString() });
        }

        /// <summary>
        /// View the Export Proposals report
        /// </summary>
        /// <param name="reportParameters">Export Proposal filter and search values</param>
        /// <returns>Json result with status of request</returns>
        public JsonResult ViewExportProposalsReport(ExportProposalReportModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            Uri reportUri = this.homeLogic.PopulateExportSSRSParameters(reportParameters);
            return this.Json(new { Status = true, Url = reportUri });
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        /// <returns>Success or Failure message</returns>
        public ActionResult ClearCache()
        {
            this.homeLogic.ClearCache();

            return this.Content("Success");
        }

        /// <summary>
        /// Display current user's approvals
        /// </summary>
        /// <returns>Displays all of the current user's approvals</returns>
        public ViewResult DisplayMyApprovals()
        {
            ICollection<RequiredApprovalsModelView> model = this.homeLogic.GetMyApprovals();

            return this.View(WebConstants.View.GET_MY_APPROVALS, model);
        }
    }
}
