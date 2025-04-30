// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// Manage Permissions Controller for getting workspace home data.
	/// </summary>
	public class ManagePermissionsController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Service for PermissionsController
		/// </summary>
		private PermissionControllerLogic PermissionControllerLogic { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("ManagePermissionsController");
		private IActiveDirectoryUtilities _ADUtils = null;

		/// <summary>
		/// ctor
		/// </summary>
		public ManagePermissionsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, PermissionControllerLogic PermissionControllerLogic
			,IActiveDirectoryUtilities inADUtils)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.PermissionControllerLogic = PermissionControllerLogic;
			_ADUtils = inADUtils;
		}

		/// <summary>
		/// Gets Permissions Model
		/// </summary>
		/// <returns>PermissionViewModel that includes PermissionsGridViewModel</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<PermissionViewModel> GetManagePermissionModel(string workspace)
		{
			IESSingleResponse<PermissionViewModel> result = new IESSingleResponse<PermissionViewModel>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
				result.Data = PermissionControllerLogic._GetPermissionsGrid(ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Delete workspaces
		/// </summary>
		///<param name="workspace">The workspace to delete permissions from.</param>
		///<param name="inUserID">The User ID to delete permissions from.</param>
		/// <returns>Object to indicate if operation is successfull</returns>
		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> DeleteUserPermissions(string workspace, string inUserID)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (inUserID == null || !inUserID.Any())
			{
				throw new ArgumentNullException(nameof(inUserID));
			}

			try
			{
				result.Data = PermissionControllerLogic.DeleteUserPermissions(workspace, Int32.Parse(inUserID));
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}

		/// <summary>
		/// Gets Members of Group
		/// </summary>
		/// <returns>Group Members</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IESSingleResponse<ICollection<UserData>> GetGroupMembers(string groupName)
		{
			IESSingleResponse<ICollection<UserData>> result = new IESSingleResponse<ICollection<UserData>>();

			try
			{
				ICollection<UserData> members = _ADUtils.GetAdGroupUsers(groupName);
				ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();

				result.Data = orderedMembers;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured: {ex.Message}");
			}

			return result;
		}
	}
}