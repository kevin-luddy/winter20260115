// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using GenTRAC.DataBridge.Common.Security;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// RDSB base controller
    /// </summary>
    public class RDSBController : IES.Common.IESController
    {
        #region Variables

        /// <summary>
        ///  The logger
        /// </summary>
        private readonly IES.Common.Logger log = new IES.Common.Logger(typeof(RDSBController));

        /// <summary>
        /// The controller dictionary
        /// </summary>
        private static Dictionary<string, Dictionary<string, SecurityAuthorization>> controllerDictionary;

        /// <summary>
        /// Dictionary to be used when checking controller's action permissions
        /// </summary>
        private static Dictionary<string, Dictionary<string, SecurityAuthorization>> ControllerDictionary
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
        /// The security mapper
        /// </summary>
        protected ISecurityMapper SecurityMapper { get; }

        /// <summary>
        /// The security information
        /// </summary>
        protected IES.Common.ISecurityInformation SecurityInformation { get; }

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        protected IES.Common.IActiveDirectoryUtilities AdUtils { get; set; }

        /// <summary>
        /// Who's Online Loader
        /// </summary>
        private IWhosOnlineLoader whosOnlineLoader;

        /// <summary>
        /// Gets current user information
        /// </summary>
        public IES.Common.UserData ActiveUser
        {
            get { return this.AdUtils.GetUserByQualifiedAccount(this.SecurityInformation.ActiveUserNTID, false); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// No empty constructor allowed
        /// </summary>
        private RDSBController()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityInformation">The security information.</param>
        /// <param name="securityMapper">The security mapper.</param>
        /// <param name="adUtils">Active Directory Utilities</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public RDSBController(IES.Common.ISecurityInformation securityInformation, GenTRAC.DataBridge.Common.Security.ISecurityMapper securityMapper, IES.Common.IActiveDirectoryUtilities adUtils, IWhosOnlineLoader whosOnlineLoader)
        {
            this.SecurityInformation = securityInformation;
            this.SecurityMapper = securityMapper;
            this.AdUtils = adUtils;
            this.whosOnlineLoader = whosOnlineLoader;
        }
        #endregion

        /// <summary>
        /// Initalizes an action with any permissions for actions WITHOUT a Document ID
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function to initialize</param>
        /// <param name="authorizationRequired">The required authorization</param>
        [NonAction]
        protected void InitializeAction(IES.Common.Logger logger, string functionName,
            SecurityAuthorization authorizationRequired)
        {
            this.InitializeAction(logger, functionName, authorizationRequired, null);
        }

        /// <summary>
        /// Initalizes an action with any permissions
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function to initialize</param>
        /// <param name="authorizationRequired">The required authorization</param>
        /// <param name="proposalId">the current proposalId linked to the current Document</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void InitializeAction(IES.Common.Logger logger, string functionName, 
            SecurityAuthorization authorizationRequired, int? proposalId)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            // Add current user details to Who's Online
            try
            {
                this.whosOnlineLoader.UpdateLastAccessTime(this.ActiveUser, IES.Common.ApplicationName.RDSB.GetDescription());
            }
            catch
            {
                // Prevent displaying error in case user is in the middle of a transaction with save
            }

            this.StartAction(logger, functionName);

            // Check authorization
            // We will be driving it by PTM roles. If you have one of these roles, you can view, edit and export the documents:
            //      Lead Estimator, Backup Lead Estimator, Cost Volume Lead, System Admin
            //
            //  Read only access (view the page in read only mode, generate the document)
            //      Allow anyone in the approval workflow (PTM) to view the document
            //
            //  Authorization mechanism
            //  - the home page will not be restricted, beyond that, everyone else will need to be authorized. 
            //  - authorization data in  -> proposal ID, requested page
            //  - authorization data out -> no access (throw a security exception), 
            //                              read only access(set ReadOnly flag that can be accessed on the page),
            //                              full access
            bool authorizationFound = false;
            bool readOnly = true;
            if (authorizationRequired != SecurityAuthorization.None)
            {
                if (proposalId == null)
                {
                    throw new ArgumentNullException(nameof(proposalId));  // A proposalId is required for all pages other than home page
                }
                else
                {
                    IReadOnlyCollection<SecurityPermissionsResponse> roles = this.SecurityMapper.GetRolesForLoggedInUser();
                    SecurityAuthorization authorization = SecurityAuthorization.None;

                    // check for System Admin
                    if (roles.Any(r => r.AuthorizedRole == PtmRole.Admin))
                    {
                        authorization = SecurityAuthorization.CreateReadUpdateDelete;
                    }
                    else
                    {
                        IList<SecurityPermissionsResponse> proposalRoles = roles.Where(r => r.ProposalID == proposalId).ToList();

                        // Check for Edit Roles and ReadOnly Roles
                        if (proposalRoles.Any(r => Constants.EDIT_ROLES.Contains(r.AuthorizedRole)))
                        {
                            authorization = SecurityAuthorization.CreateReadUpdateDelete;
                        }
                        else if (proposalRoles.Any(r => Constants.READ_ONLY_ROLES.Contains(r.AuthorizedRole)))
                        {
                            authorization = SecurityAuthorization.Read;
                        }
                    }

                    if (authorization >= authorizationRequired)
                    {
                        authorizationFound = true;

                        if (authorization == SecurityAuthorization.ReadUpdate || authorization == SecurityAuthorization.CreateReadUpdateDelete)
                        {
                            readOnly = false;
                        }
                    }

                    if (!authorizationFound)
                    {
                        throw new AuthorizationException(functionName + " was not authorized");
                    }
                }
            }

            this.ViewBag.ReadOnly = readOnly ? "true" : "false";
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
            if (filterContext != null)
            {
                Dictionary<string, SecurityAuthorization> actionDictionary;

                // Check to see if we have a dictionary for the controller. If we do, get it. If not, throw an exception
                if (ControllerDictionary.TryGetValue(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower(), out actionDictionary))
                {
                    string functionName = filterContext.ActionDescriptor.ActionName.ToLower();

                    SecurityAuthorization authorizationRequired;
                    if (actionDictionary.TryGetValue(functionName, out authorizationRequired))
                    {
                        int? proposalId = filterContext.ActionParameters.ContainsKey("id") ? filterContext.ActionParameters["id"] as int? : null;
                        this.InitializeAction(this.log, functionName, authorizationRequired, proposalId);
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
                this.FinalizeAction(this.log, functionName);
            }

            base.OnActionExecuted(filterContext);
        }

        #endregion

        #region Who's Online

        /// <summary>
        /// Determine if Who's Online button should show
        /// </summary>
        /// <returns>True if should show button, False if not</returns>
        [HttpPost]
        public ActionResult CanViewWhosOnline()
        {
            bool toReturn = false;

            string[] viewWhosOnlineGroups = IES.Common.ConfigurationUtilities.GetAppSetting("AuthorizeViewWhosOnlineGroups").Split(',');
            foreach (string group in viewWhosOnlineGroups)
            {
                if (this.AdUtils.IsMemberOfADGroup(this.ActiveUser.Ntid, group))
                {
                    toReturn = true;
                    break;
                }
            }

            return this.Json(toReturn);
        }

        /// <summary>
        /// Gets data for Who's Online
        /// </summary>
        /// <returns>Who's Online data</returns>
        [HttpPost]
        public ActionResult GetWhosOnline()
        {
            ICollection<WhosOnlineModelView> whosOnline = this.whosOnlineLoader.GetWhosOnlineData(IES.Common.ApplicationName.RDSB.GetDescription());
            return this.Json(whosOnline);
        }
       
        #endregion
    }
}