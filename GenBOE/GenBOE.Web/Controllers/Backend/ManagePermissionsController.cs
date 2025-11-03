// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web;
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;

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
		private readonly Logger logger = new Logger("ManagePermissionsController");

		/// <summary>
		/// The ad utilities class
		/// </summary>
		private readonly IActiveDirectoryUtilities ADUtils = null;

		/// <summary>
		/// ctor
		/// </summary>
		public ManagePermissionsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, PermissionControllerLogic PermissionControllerLogic
			, IActiveDirectoryUtilities inADUtils)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.PermissionControllerLogic = PermissionControllerLogic;
			ADUtils = inADUtils;
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

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_MANAGE_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				result.Data = PermissionControllerLogic._GetPermissionsGrid(ws);
				result.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_MANAGE_PERMISSIONS, sw);
			return result;
		}

		/// <summary>
		/// Delete workspaces
		/// </summary>
		///<param name="workspace">The workspace to delete permissions from.</param>
		///<param name="inUserID">The User ID to delete permissions from.</param>
		/// <returns>Object to indicate if operation is successful</returns>
		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> DeleteUserPermissions(string workspace, string inUserID)
		{
			if (inUserID == null || !inUserID.Any())
			{
				throw new ArgumentNullException(nameof(inUserID));
			}

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DELETE_USER_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				result.Data = PermissionControllerLogic.DeleteUserPermissions(workspace, Int32.Parse(inUserID));
				result.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_DELETE_USER_PERMISSIONS, sw);
			return result;
		}

		/// <summary>
		/// Gets Members of Group
		/// </summary>
		///<param name="groupName">The AD group name.</param>
		/// <returns>Group Members</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IESSingleResponse<ICollection<UserData>> GetGroupMembers(string groupName)
		{
			IESSingleResponse<ICollection<UserData>> result = new IESSingleResponse<ICollection<UserData>>();

			try
			{
				ICollection<UserData> members = ADUtils.GetAdGroupUsers(groupName);
				ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();

				result.Data = orderedMembers;
				result.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return result;
		}

		/// <summary>
		/// Save New Permissions
		/// </summary>
		/// <param name="saveNewPermissionsModelView"></param>
		/// <returns>True/False if everything runs</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> SaveNewPermissions([FromBody] SaveNewPermissionsModelView saveNewPermissionsModelView)
		{
			_ = saveNewPermissionsModelView ?? throw new ArgumentNullException(nameof(saveNewPermissionsModelView));

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(saveNewPermissionsModelView.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				if (saveNewPermissionsModelView != null)
				{
					SavePermissionModelView webPermissionsModelView = new SavePermissionModelView()
					{
						EntityIds = { saveNewPermissionsModelView.entityIds },
						Roles = new System.Collections.ObjectModel.Collection<IES.Common.Role>(saveNewPermissionsModelView.roles.ToList()),
					};

					PermissionControllerLogic.SaveNewPermissions(saveNewPermissionsModelView.workspaceShortName, new SavePermissionModelView[] { webPermissionsModelView });
					result.Data = true;
					result.IsSuccessful = true;
				}
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_SAVE_PERMISSIONS, sw);
			return result;
		}

		/// <summary>
		/// Export Permissions
		/// </summary>
		/// <param name="exportPermissionModelView">ExportFileModelView</param>
		/// <returns>filestream</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportPermissions([FromBody] ExportFileModelView exportPermissionModelView)
		{
			_ = exportPermissionModelView ?? throw new ArgumentNullException(nameof(exportPermissionModelView));

			FullWorkspace ws = this.Factory.CreateFullWorkspace(exportPermissionModelView.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_EXPORT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

			try
			{
				if (exportPermissionModelView != null)
				{
					FileStream fs = this.PermissionControllerLogic.ExportPermissions(ws);
					response.Content = new StreamContent(fs);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = "Permissions.xlsx";
				}
				else
				{
					response = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent("Invalid request body")
					};
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("Unknown error exporting Permissions")
				};
			}

			FinalizeAction(logger, WebConstants.ACTION_EXPORT_PERMISSIONS, sw);
			return response;
		}

		/// <summary>
		/// Import Permissions from File
		/// </summary>
		/// <param name="workspace">The workspace name</param>
		/// <returns>boolean</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), SuppressMessage("Microsoft.Design", "CA1011: Consider passing base types as parameters")]
		public IESSingleResponse<bool> ImportPermissions(string workspace)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_IMPORT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				Stream importFile = HttpContext.Current.Request.Files[0].InputStream;

				// If a file was uploaded successfully
				if (importFile != null)
				{
					string errorMessage = PermissionControllerLogic.ImportPermissions(workspace, importFile);

					if (!string.IsNullOrEmpty(errorMessage))
					{
						throw new GenValidationException(errorMessage);
					}
					else
					{
						result.Data = true;
						result.IsSuccessful = true;
					}
				}
				// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
				else
				{
					result.Messages.Add("No file selected for upload");
				}

			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_IMPORT_PERMISSIONS, sw);
			return result;
		}

		/// <summary>
		/// POST method to edit permissions
		/// </summary>
		/// <param name="saveNewPermissionsModelView"></param>
		/// <returns></returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> EditPermissions([FromBody] EditPermissionsModelView editPermissionsModelView)
		{
			_ = editPermissionsModelView ?? throw new ArgumentNullException(nameof(editPermissionsModelView));

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(editPermissionsModelView.workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_EDIT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				if (editPermissionsModelView != null)
				{
					PermissionControllerLogic.EditPermissions(ws, new System.Collections.ObjectModel.Collection<IES.Common.Role>(editPermissionsModelView.roles.ToList()), EntityType.User, editPermissionsModelView.entityId);
					result.Data = true;
					result.IsSuccessful = true;
				}
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_EDIT_PERMISSIONS, sw);
			return result;
		}

		/// <summary>
		/// Gets Users for Bulk Assign Page "Users" Dropdown List
		/// </summary>
		///<param name="workspaceShortName">The Workspace to get users for</param>
		/// <returns>List of Users</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IESResponse<Tuple<Role, Collection<System.Web.Mvc.SelectListItem>>> GetUsersForDropdown(string workspaceShortName)
		{
			// Adding the full reference for "SelectListItem" because there will end up being ambiguity between .Mvc and .Http for the "HttpGet" "HttpPost" and etc attributes
			IESResponse<Tuple<Role, Collection<System.Web.Mvc.SelectListItem>>> result = new IESResponse<Tuple<Role, Collection<System.Web.Mvc.SelectListItem>>>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_GET_DROPDOWN_USERS_BULK_ASSIGN, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				ICollection<Tuple<Role, Collection<System.Web.Mvc.SelectListItem>>> roles  = this.PermissionControllerLogic.GetDropdownUsers(ws.Id);

				if (roles.Count > 0)
				{
					result.Data = roles;
					result.IsSuccessful = true;
				}
				else
				{
					result.Messages.Add("Failed to get Users for Dropdown List");
					result.IsSuccessful = false;
				}
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			FinalizeAction(logger, WebConstants.ACTION_GET_DROPDOWN_USERS_BULK_ASSIGN, sw);
			return result;
		}
	}
}