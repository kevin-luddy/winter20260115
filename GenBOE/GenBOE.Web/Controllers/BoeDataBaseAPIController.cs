// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http;
	using IES.Common;
	using IES.Common.Exceptions;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using System.Diagnostics;

	/// <summary>
	/// Base BOE Api data controller
	/// </summary>
	public abstract class BoeDataBaseAPIController : ApiController
	{
		/// <summary>
		/// Security Access
		/// </summary>
		protected ISecurityAccess SecurityAccess { get; set; }

		/// <summary>
		/// Full object factory
		/// </summary>
		protected IFullObjectFactory Factory { get; set; }

		/// <summary>
		/// User loader
		/// </summary>
		protected IUserDTODataLoader UserLoader { get; set; }

		/// <summary>
		/// Permission loader
		/// </summary>
		protected IPermissionsDTODataLoader PermissionsLoader { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		protected BoeDataBaseAPIController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
		{
			this.Factory = factory;
			this.UserLoader = userLoader;
			this.PermissionsLoader = permissionsLoader;
			this.SecurityAccess = securityAccess;
		}

		/// <summary>
		/// Checks if user has permission to Security Page inside a Workspace.
		/// First, checks whether workspace has OCI.  If true, then checks user's permission to workspace; otherwise, returns true.
		/// </summary>
		/// <param name="page">The security page to check for access</param>
		/// <param name="workspace">The workspace to check for access</param>
		/// <returns>True if user has permission to Security Page inside a Workspace.</returns>
		protected bool HasOciPermission(SecurityPage page, WorkspaceDTO workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			bool permissionOk = true;
			if (workspace.ContainsOCI)
			{
				SecurityAuthorization permission = this.CheckPermission(page, workspace);

				if (permission < SecurityAuthorization.Read)
				{
					permissionOk = false;
				}
			}

			return permissionOk;
		}

		/// <summary>
		/// Checks permission for page (Some logic pulled from GenBOEController.cs)
		/// </summary>
		/// <param name="page">Security Page</param>
		/// <param name="workspace">Workspace</param>
		/// <param name="inBOEId">BOE ID</param>
		/// <returns>Security Authorization</returns>
		protected SecurityAuthorization CheckPermission(SecurityPage page, WorkspaceDTO workspace, int? inBOEId = null)
		{
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

			SecurityAuthorization authorizationForUser;

			if ((bool)isUsPerson)
			{
				authorizationForUser = SecurityAccess.IsAuthorized(
					new SecurityPermissionsRequested { PageToCheck = page, WorkspaceId = wsId }, workspace, rolesForUser);
			}
			else
			{
				throw new UnauthorizedAccessException("Access is denied for non-US users.");
			}

			securityDictionary.Add(page, authorizationForUser);

			return securityDictionary.Values.First();
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
       // [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We do not want to cause issues if the boe metrics user update fails.")]
        protected Stopwatch InitializeAction(Logger logger, string functionName, SecurityPage page, SecurityAuthorization authorizationRequired, ICollection<WorkspaceDTO>  workspaces, int? boeID)
        {
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			Stopwatch sw = new Stopwatch();
			sw.Start();

			if (workspaces == null)
			{
				throw new ArgumentNullException(nameof(workspaces));
			}

			foreach (WorkspaceDTO ws in workspaces)
			{
				bool authorizationFound = false;

				SecurityAuthorization authorization = this.CheckPermission(page, ws, boeID);

				if (authorization >= authorizationRequired)
				{
					authorizationFound = true;
				}

				if (!authorizationFound)
				{
					throw new AuthorizationException(functionName + " was not authorized");
				}
			}

			return sw;
        }

        /// <summary>
        /// Finalizes a controller action
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
    }
}