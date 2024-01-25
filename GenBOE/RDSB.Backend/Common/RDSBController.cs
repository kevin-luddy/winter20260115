// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenTRAC.DataBridge.Core.Common.Security;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.Security;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Controllers;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// RDSB base controller
	/// </summary>
	public abstract class RDSBController : IES.Common.Core.IESController, IActionFilter
	{
        #region Variables

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
        /// Active Directory Utilities
        /// </summary>
        protected IActiveDirectoryService AdUtils { get; set; }

        /// <summary>
        /// Who's Online Loader
        /// </summary>
        private readonly IWhosOnlineLoader whosOnlineLoader;

		/// <summary>
		/// HttpContext accessor
		/// </summary>
		private readonly IHttpContextAccessor contextAccessor;

		/// <summary>
		/// Gets current user information
		/// </summary>
		private UserData ActiveUser
        {
            get { return this.AdUtils.GetUserByQualifiedAccount(this.securityInformation.ActiveUserNTID, false); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityInformation">The security information.</param>
        /// <param name="securityMapper">The security mapper.</param>
        /// <param name="adUtils">Active Directory Utilities</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public RDSBController(ISecurityInformation securityInformation, 
			ISecurityMapper securityMapper, 
			IActiveDirectoryService adUtils, IWhosOnlineLoader whosOnlineLoader,
			ILogger logger, IHttpContextAccessor contextAccessor) : base(logger, securityInformation)
        {
			this.SecurityMapper = securityMapper;
            this.AdUtils = adUtils;
            this.whosOnlineLoader = whosOnlineLoader;
			this.contextAccessor = contextAccessor;

		}
        #endregion

        /// <summary>
        /// Initalizes an action with any permissions for actions WITHOUT a Document ID
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="functionName">The function to initialize</param>
        /// <param name="authorizationRequired">The required authorization</param>
        [NonAction]
        protected void InitializeAction(ILogger logger, string functionName,
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
		[NonAction]
		protected void InitializeAction(ILogger logger, string functionName, 
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
                this.whosOnlineLoader.UpdateLastAccessTime(this.ActiveUser, ApplicationName.RDSB.GetDescription());
            }
            catch
            {
                // Prevent displaying error in case user is in the middle of a transaction with save
            }

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
                        if (proposalRoles.Any(r => CommonConstants.EDIT_ROLES.Contains(r.AuthorizedRole)))
                        {
                            authorization = SecurityAuthorization.CreateReadUpdateDelete;
                        }
                        else if (proposalRoles.Any(r => CommonConstants.READ_ONLY_ROLES.Contains(r.AuthorizedRole)))
                        {
                            authorization = SecurityAuthorization.Read;
                        }
                    }

					if (authorization >= authorizationRequired)
					{
						authorizationFound = true;
					}
					else
					{
                        throw new AuthorizationException(functionName + " was not authorized");
                    }
                }
            }
        }

        #region Events

        /// <summary>
        /// Override of the default OnActionExecuting, to allow us to do security verification
        /// </summary>
        /// <param name="filterContext">Context</param>
        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
			IdentitySwap.IdentitySwappingForTesting.SwapIdentity("IsIdentitySwappingAllowed", "ActiveDirectoryPath",
				"ADGroupsAllowedToSwapIdentity", context.HttpContext);

			if (context != null)
            {
				ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
				// Check to see if we have a dictionary for the controller. If we do, get it. If not, throw an exception
				if (ControllerDictionary.TryGetValue(descriptor.ControllerName.ToLower(), out Dictionary<string, SecurityAuthorization> actionDictionary))
                {
					string functionName = descriptor?.ActionName.ToLower();

					if (actionDictionary.TryGetValue(functionName, out SecurityAuthorization authorizationRequired))
					{
						int? proposalId = context.ActionArguments.ContainsKey("id") ? context.ActionArguments["id"] as int? : null;
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

            base.OnActionExecuting(context);
        }

		#endregion

		#region Who's Online

		/// <summary>
		/// Determine if Who's Online button should show
		/// </summary>
		/// <returns>True if should show button, False if not</returns>
		[HttpPost("[action]")]
		public bool CanViewWhosOnline()
        {
            bool toReturn = false;

            string[] viewWhosOnlineGroups = ConfigurationUtilities.GetAppSetting("AuthorizeViewWhosOnlineGroups").Split(',');
            foreach (string group in viewWhosOnlineGroups)
            {
                if (this.AdUtils.IsMemberOfADGroup(this.ActiveUser.Ntid, group))
                {
                    toReturn = true;
                    break;
                }
            }

            return toReturn;
        }

		/// <summary>
		/// Gets data for Who's Online
		/// </summary>
		/// <returns>Who's Online data</returns>
		[HttpPost("[action]")]
		public ICollection<WhosOnlineModelView> GetWhosOnline()
        {
            ICollection<WhosOnlineModelView> whosOnline = this.whosOnlineLoader.GetWhosOnlineData(ApplicationName.RDSB.GetDescription());
            return whosOnline;
        }
       
        #endregion
    }
}