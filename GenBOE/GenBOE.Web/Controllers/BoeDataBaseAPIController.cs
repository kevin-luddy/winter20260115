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
		/// <returns>Security Authorization</returns>
		protected SecurityAuthorization CheckPermission(SecurityPage page, WorkspaceDTO workspace)
		{
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

	}
}