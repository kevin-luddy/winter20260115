// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Security.Principal;

    /// <summary>
    /// The security authorizations a user can be
    /// allowed for a given page, Role
    /// combination
    /// </summary>
    public enum SecurityAuthorization
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        None = 0,

        /// <summary>
        /// Read access
        /// </summary>
        Read = 1,

        /// <summary>
        /// Read and update access
        /// </summary>
        ReadUpdate = 2,

        /// <summary>
        /// Create, read, update and delete access
        /// </summary>
        CreateReadUpdateDelete = 3,

        /// <summary>
        /// Create, read, update and delete access for Forecast proposals
        /// </summary>
        CreateReadUpdateDeleteForecast = 4
    }

    /// <summary>
    /// The pages security is configured to secure
    /// </summary>
    /// <remarks>
    /// This enumeration is no longer limited to pages ONLY.  It actually includes/represents any and all
    /// elements of the user interface (e.g. fields, sections OR entire pages) that can have their own distinct
    /// accessibility.  A name-change might be useful to clarify this.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum SecurityPage
    {
        None = 0,
        Home = 1,
        ManageCLINs = 2,
        ManageWBS = 3,
        ManageBOEs = 4,
        WorkspaceAdminPermissions = 5,
        EditBOEHeader = 6,
        TaskElements = 7,
        MetricsAdmin = 8,
        GlobalConfig = 9,
        WorkspaceSettings = 10,
        // NOTE: WorkspaceSettingStatus is necessary because there is a case where a user needs Update permissions on just the 
        // workspace status section of workspace settings, while they see the rest of the page as read only.
        WorkspaceSettingsStatus = 11,
        Reports = 12,
        //This is used for all pages common between Metrics Admin and System Admin.
        Admin = 13,
        SelectWorkspace = 14, // NOTE: this is used for filtering the dropdown to choose a workspace.
        BOEApproval = 15,
        BOEComment = 16,
        BOECommentResponse = 17,
        WorkspaceHome = 18,
        SubmitForReview = 19,
        SubmitForApproval = 20,
        ValidateBOE = 21,
        FindReplace = 22,
        ExportToProPricer = 23,
        ExportToProPricerSystemAdmin = 24,
        SystemAdmin = 25,
        Help = 26,
        BoeLaborTypes = 28,
        HistoricalMetricSearch = 29,
        BOEMaterialsTypes = 30,
        // ImportMaterials = 31, OBE
        EditBOEHeaderDescription = 32,
        MOQEquationField = 33,
        BoeSearch = 34,
        BoeCopyConflicts = 35,
        ImportMaterialsAsBoeAuthor = 36,
        BoeMaterialsGrid = 37,
        BoeODCGrid = 38,
        BOETravelGrid = 39,
        UpdateLockedResourceRates = 41,
        UpdateLockedResourceRatesMenuOption = 42,
        WorkspaceLaborCost = 43,
        BoeCustomFieldResource = 44,
        TaskElementsMissionTask = 45,
        CreateWorkspacePermissions = 46,
        BoeCustomFields = 47, //represents generic boe custom fields not resources/perf orgs
        BoeTaskDates = 48, // used for the Adjust Dates fields on the BOE Header and the Task Element Details pages (permissions do not follow the parent page)
        SaveVersion = 49,
        AboutToolsMenu = 50,  // menu selections that have associated URLs
        ContactMenu = 51,
        HelpMenu = 52,
        GenerationHome = 53,
        GenBOEHelp = 54,
        // Mobile = 56, // OBE
        WorkspaceResource = 57,
        WorkspacePerfOrg = 58,
        EditBoeLockedState = 59,  // change BOE states between Draft and Locked-Draft for a Locked workspace
        BOELaborGrid = 60,
        // ManageCustomForm = 61, OBE
        BOEZoneTravelGrid = 62,
        ManageBOEForms = 63,
        UpdateZoneTravelRatesMenuOption = 64,
        UpdateOffloadRatesMenuOption = 65,
        SSRSReports = 66,
        ProjectMapBoeSearch = 67, // Allowing BOE Search on Project Map Workspace Home
        // NOTE: WorkspaceSettingsOutputTemplateFormat is necessary because there is a case where a user needs Update permissions on just the 
        // Output Format Template section of workspace settings, while they see the rest of the page as read only.
        WorkspaceSettingsOutputFormatTemplate = 68,
        BulkSubmit = 69,
        WorkspaceDelete = 70,
        WorkspaceRestore = 71,
        GettingStartedMenuOption = 72,
        WorkspaceSettingsShareAndAllowSearch = 73,
        RTETemplates = 74,
		WorkspaceRecalculateActuals = 75
    }

    /// <summary>
    /// This class encapsulates high level security information about the 
    /// active user and the resource account the ASP.NET process is running within.
    /// 
    /// It also contains some high level helper functions to assist in converting
    /// some security enums to strings.
    /// </summary>
    public class SecurityInformation : ISecurityInformation
    {
        private IActiveDirectoryUtilities _ADUtils = null;

        /// <summary>
        /// Constructor to use for dependency injection
        /// </summary>
        public SecurityInformation(IActiveDirectoryUtilities inADUtils, ICache memCache) 
        {
            _ADUtils = inADUtils;
            this.cache = memCache;
        }

        /// <summary>
        /// Returns .Net System.Security.WindowsIdentity object for the resource account running the application
        /// (ex. acct04\genboed) - Always returns resource account name
        /// </summary>
        virtual public string ResourceAccount
        {
            get
            {
                return WindowsIdentity.GetCurrent().Name.ToLower();
            }
        }

        /// <summary>
        /// Query the AD for more detailed information.  This method is slower and less performant than just 
        /// retrieving the username via the ActiveUserNTID method.
        /// </summary>
        virtual public UserData ActiveUserData
        {
            get
            {
                return _ADUtils.GetUserByQualifiedAccount(this.ActiveUserNTID, false);
            }
        }

        /// <summary>
        /// Returns the name of the currently logged on user (ex. jsmith)
        /// </summary>
        virtual public string ActiveUserNTID
        {
            get
            {
                // split the ntid and just pass in the ntid
                if (string.IsNullOrEmpty(ActiveUserNTIDWithDomain))
                {
                    return "";
                }

                string[] splitDomainAndNTID = ActiveUserNTIDWithDomain.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string ntIDOnly = splitDomainAndNTID.Count() == 2 ? splitDomainAndNTID[1] : splitDomainAndNTID[0];

                return ntIDOnly.ToLower();
            }
        }

        /// <summary>
        /// Returns the name of the currently logged on user (ex. acct04\jsmith)
        /// </summary>
        protected virtual string ActiveUserNTIDWithDomain
        {
            get
            {
                return System.Threading.Thread.CurrentPrincipal.Identity.Name.ToLower();
            }
        }

        /// <summary>
        /// Helper function to return roles as string
        /// </summary>
        /// <param name="inRoles">roles to emit</param>
        /// <returns>comma separated flat string</returns>
        virtual public string GetRoleAsString(Collection<Role> inRoles)
        {
            if (inRoles == null)
            {
                return "<none>";
            }
			System.Collections.Generic.IEnumerable<string> toReturn = from role in inRoles select role.ToString();

            return String.Join(",", toReturn.ToArray());
        }

        /// <summary>
        /// Determine if the logged on user is a memeber of the us domestic user group
        /// </summary>
        /// <param name="inPrincipal">User ID</param>
        /// <returns></returns>
        virtual public Boolean IsDomesticUser(IPrincipal inPrincipal)
        {
            if (inPrincipal == null)
            {
                throw new ArgumentNullException(nameof(inPrincipal));
            }

            String DomesticUsersGroup = ConfigurationUtilities.GetAppSetting("DomesticUsersGroup");

            bool approved = inPrincipal.IsInRole(DomesticUsersGroup);

            // otherwise, if we are on dev ... allow a-* accounts through since they'll show up as foreign users
            // since they aren't in the US group.
            if (!approved && ConfigurationUtilities.GetAppSetting("ServerURL").Contains("dev"))
            {
                if (inPrincipal.IsInRole(@"US\EBS.EstimationInitiative.DevTeam"))
                {
                    approved = true;
                }
            }

            return approved;
        }
        
        /// <summary>
        /// Determines if user is a subcontractor (and not overridden)
        /// </summary>
        /// <param name="ntid">NTID</param>
        /// <param name="isSubcontractor">bool from db if user is subcontractor</param>
        /// <returns>true if user is a subcontractor and not overridden</returns>
        virtual public Boolean IsSubcontractorUser(string ntid, bool? isSubcontractor)
        {
            if (string.IsNullOrEmpty(ntid))
            {
                throw new ArgumentNullException(nameof(ntid));
            }

            string overrideSubString = ConfigurationUtilities.GetAppSetting("OverrideSubNonUs");
            bool overrideSub = string.IsNullOrEmpty(overrideSubString) ? false : overrideSubString.ToLower() == "true";
            if (overrideSub)
            {
                return false;
            }

            if (isSubcontractor == null)
            {
                throw new ArgumentNullException(nameof(isSubcontractor));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(ntid, this.cacheKeyIsSubcontractorByUserName);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isTreatedAsLmEmployee = false;

            if ((bool)isSubcontractor)
            {
                try
                {
                    isTreatedAsLmEmployee = this.IsMemberOfADGroupInAppSettingsList(ntid, "SubsTreatedAsLmEmployees");
                }
                catch (ArgumentException)
                {
                    // Do nothing - this exception is thrown if group is blank, which is fine. bool is left false
                }
            }

            bool isSub = (bool)isSubcontractor && !isTreatedAsLmEmployee;

            this.AddResultToCache(ntid, this.cacheKeyIsSubcontractorByUserName, this.secondsToCacheIsSubcontractor, isSub);

            return isSub;
        }

        /// <summary>
        /// Determine if the specified user is a member of the RDM Admin group(s)
        /// </summary>
        /// <param name="inUserName">The NTID of the user trying to gain access</param>
        /// <returns>true if user is a member of one of the RDM Admin groups; false otherwise</returns>
        virtual public Boolean IsRdmAdminUser(string inUserName)
        {
            if (inUserName == null)
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(inUserName, this.cacheKeyIsRdmAdminByUserName);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isRDMAdminUser = this.IsMemberOfADGroupInAppSettingsList(inUserName, "RDMAdminGroups");
            this.AddResultToCache(inUserName, this.cacheKeyIsRdmAdminByUserName, this.secondsToCacheIsRdmAdmin, isRDMAdminUser);
            return isRDMAdminUser;
        }

        /// <summary>
        /// Determine if the specified user is a member of the RDM COBRA Admin group(s)
        /// </summary>
        /// <param name="inUserName">The NTID of the user trying to gain access</param>
        /// <returns>true if user is a member of one of the RDM COBRA Admin groups; false otherwise</returns>
        virtual public Boolean IsRdmCobraAdminUser(string inUserName)
        {
            if (inUserName == null)
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(inUserName, this.cacheKeyIsRdmCobraAdminByUserName);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isRDMCobraAdminUser = this.IsMemberOfADGroupInAppSettingsList(inUserName, "RDMCobraAdminGroups");
            this.AddResultToCache(inUserName, this.cacheKeyIsRdmCobraAdminByUserName, this.secondsToCacheIsRdmCobraAdmin, isRDMCobraAdminUser);
            return isRDMCobraAdminUser;
        }
        
        /// <summary>
         /// Determine if the specified user is a member of the IES Portal Admin group(s)
         /// </summary>
         /// <param name="inUserName">The NTID of the user trying to gain access</param>
         /// <returns>true if user is a member of one of the IES Portal Admin groups; false otherwise</returns>
        virtual public Boolean IsIESPortalAdminUser(string inUserName)
        {
            if (inUserName == null)
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(inUserName, this.cacheKeyIsIESPortalAdminByUserName);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isIESPortalAdminUser = this.IsMemberOfADGroupInAppSettingsList(inUserName, "IESPortalAdminGroups");
            this.AddResultToCache(inUserName, this.cacheKeyIsIESPortalAdminByUserName, this.secondsToCacheIsIESPortalAdmin, isIESPortalAdminUser);
            return isIESPortalAdminUser;
        }

        /// <summary>
        /// Determine if the specified user is a member of the RDM Viewer group(s)
        /// </summary>
        /// <param name="inUserName">The NTID of the user trying to gain access</param>
        /// <returns>true if user is a member of one of the RDM Viewer groups; false otherwise</returns>
        virtual public Boolean IsRdmViewerUser(string inUserName)
        {
            if (inUserName == null)
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(inUserName, this.cacheKeyIsRdmViewerByUserName);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isRDMViewerUser = this.IsMemberOfADGroupInAppSettingsList(inUserName, "RDMViewerGroups");
            this.AddResultToCache(inUserName, this.cacheKeyIsRdmViewerByUserName, this.secondsToCacheIsRdmViewer, isRDMViewerUser);
            return isRDMViewerUser;
        }

        /// <summary>
        /// Determine if the specified user is allowed to access the ProPricer Export
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        virtual public Boolean IsAllowedProPricerAccess(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(userName, this.cacheKeyIsAllowedProPricerAccess);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isAllowedProPricerAccess =
                this.IsMemberOfADGroupInAppSettingsList(userName, "UsersAllowedProPricerAccess");
            this.AddResultToCache(userName, this.cacheKeyIsAllowedProPricerAccess, this.secondsToCacheIsAllowedProPricerAccess, isAllowedProPricerAccess);
            return isAllowedProPricerAccess;
        }

        /// <summary>
        /// Determine if the specified user is a system or subcontract admin based on the AD groups
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        virtual public Boolean IsSystemOrSubcontractAdmin(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            bool? resultFromCache = this.IsUserInCacheCheck(userName, this.cacheKeyIsSystemOrSubcontractAdmin);
            if (resultFromCache.HasValue)
            {
                return resultFromCache.Value;
            }

            bool isSystemOrSubcontractAdmin =
                this.IsMemberOfADGroupInAppSettingsList(userName, "SystemAdminADGroup") || this.IsMemberOfADGroupInAppSettingsList(userName, "SubcontractAdminADGroup");
            this.AddResultToCache(userName, this.cacheKeyIsSystemOrSubcontractAdmin, this.secondsToCacheIsSystemOrSubcontractAdmin, isSystemOrSubcontractAdmin);
            return isSystemOrSubcontractAdmin;
        }

        /// <summary>
        /// Returns true if a user is a member of a group (from a list of AD groups stored in the Application Settings).
        /// </summary>
        /// <param name="inUserName">The NTID of the user trying to gain access</param>
        /// <param name="inADGroupListAppSettingsKey">The application configuration key holding the list of AD group names.</param>
        /// <returns>true if user is a member of one of the AD groups; false otherwise</returns>
        public bool IsMemberOfADGroupInAppSettingsList(string inUserName, string inADGroupListAppSettingsKey)
        {
            if (string.IsNullOrEmpty(inUserName))
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            if (string.IsNullOrEmpty(inADGroupListAppSettingsKey))
            {
                throw new ArgumentNullException(nameof(inADGroupListAppSettingsKey));
            }

            string groupNames = ConfigurationUtilities.GetAppSetting(inADGroupListAppSettingsKey);
            if (string.IsNullOrEmpty(groupNames))
            {
                throw new ArgumentException("System configuration AppSettings {0} entry is empty or missing.", inADGroupListAppSettingsKey);
            }

            bool isUserInGroups = false;

            // Get a list of user group(s).
            string[] groupNamesArray = null;
            if (groupNames.Contains(","))
            {
                groupNamesArray = groupNames.Split(',');
            }
            else
            {
                groupNamesArray = new string[] { groupNames };
            }

            // Loop through each group, and see if the user is a member of the group.
            foreach (string item in groupNamesArray)
            {
                string[] splitDomainAndGroup = item.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitDomainAndGroup.Count() == 2)
                {
                    string groupName = splitDomainAndGroup[1];

                    isUserInGroups = _ADUtils.IsMemberOfADGroup(inUserName.ToLower(), groupName.ToLower());
                }
                else if (splitDomainAndGroup.Count() == 1)
                {   // Group name only (no domain specified)
                    // lookup the group domain
                    string groupName = splitDomainAndGroup[0];

                    isUserInGroups = _ADUtils.IsMemberOfADGroup(inUserName.ToLower(), groupName.ToLower());
                }
                else
                {
                    throw new ArgumentException("System configuration AppSettings {0} entry is invalid.", inADGroupListAppSettingsKey);
                }

                if (isUserInGroups)
                {
                    break;
                }
            }
            return isUserInGroups;
        }

        #region Cache Setup

        /// <summary>
        /// Cache Object
        /// </summary>
        private ICache cache;

        /// <summary>
        /// We are caching IsSubcontractor for 30 seconds.
        /// </summary>
        private int secondsToCacheIsSubcontractor = 30;

        /// <summary>
        /// We are caching IsRdmAdminUser for 30 seconds.
        /// </summary>
        private int secondsToCacheIsRdmAdmin = 30;

        /// <summary>
        /// We are caching IsRdmCobraAdminUser for 30 seconds.
        /// </summary>
        private int secondsToCacheIsRdmCobraAdmin = 30;

        /// <summary>
        /// We are caching IsRdmViewerUser for 30 seconds.
        /// </summary>
        private int secondsToCacheIsRdmViewer = 30;

        /// <summary>
        /// We are caching IsIESPortalAdminUser for 5 minutes.
        /// </summary>
        private int secondsToCacheIsIESPortalAdmin = 300;

        /// <summary>
        /// We are caching IsAllowedProPricerAccess for 30 seconds
        /// </summary>
        private int secondsToCacheIsAllowedProPricerAccess = 30;

		/// <summary>
        /// We are caching IsSystemOrSubcontractAdmin for 5 minutes.
        /// </summary>
        private int secondsToCacheIsSystemOrSubcontractAdmin = 300;

        /// <summary>
        /// Key for the cache
        /// </summary>
        private string cacheKeyIsSubcontractorByUserName = "IsSubcontractorByUserName_";

        /// <summary>
        /// Key for the IsRdmAdmin cache
        /// </summary>
        private string cacheKeyIsRdmAdminByUserName = "IsRdmAdminByUserName_";

        /// <summary>
        /// Key for the IsRdmCobraAdmin cache
        /// </summary>
        private string cacheKeyIsRdmCobraAdminByUserName = "IsRdmCobraAdminByUserName_";

        /// <summary>
        /// Key for the IsRdmViewer cache
        /// </summary>
        private string cacheKeyIsRdmViewerByUserName = "IsRdmViewerByUserName_";

        /// <summary>
        /// Key for the IsIESPortalAdmin cache
        /// </summary>
        private string cacheKeyIsIESPortalAdminByUserName = "IsIESPortalAdminByUserName_";

        /// <summary>
        /// Key for the IsAllowedProPricerAccess cache
        /// </summary>
        private string cacheKeyIsAllowedProPricerAccess = "IsAllowedProPricerAccess_";

		/// <summary>
        /// Key for the IsSystemOrSubcontractAdmin cache
        /// </summary>
        private string cacheKeyIsSystemOrSubcontractAdmin = "IsSystemOrSubcontractAdmin_";

        #endregion

        #region Cache Helper Methods

        /// <summary>
        /// Is user in cache check - returns null if not in cache, returns bool if stored in cache
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="cacheKey">cache key</param>
        /// <returns>NULL or cached result</returns>
        private bool? IsUserInCacheCheck(string userName, string cacheKey)
        {
            bool? result = null;

            if (this.cache.Contains(cacheKey + userName))
            {
                result = (bool)this.cache.GetData(cacheKey + userName);
            }

            return result;
        }

        /// <summary>
        /// Adds the result (i.e. Is user a member of a specific AD group) into the specified memory cache
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="cacheKey">cache key</param>
        /// <param name="secondsToCache">seconds to keep in cache</param>
        /// <param name="result">result to cache</param>
        private void AddResultToCache(string userName, string cacheKey, int secondsToCache, bool result)
        {
            this.cache.Add(cacheKey + userName, result, secondsToCache);
        }

        #endregion
    }
}
