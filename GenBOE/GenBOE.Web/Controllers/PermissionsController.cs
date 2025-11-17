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
	using System.IO;
	using System.Linq;
	using System.Transactions;
	using System.Web.Mvc;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.Permissions;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;

	public class PermissionsController : GenBOEController
	{
		private readonly Logger _log = new Logger(typeof(PermissionsController));
		private readonly IActiveDirectoryUtilities _ADUtils = null;
		private readonly PermissionControllerLogic _permissionControllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		public PermissionsController(ISecurityAccess inSecurityAccess,
			CommonDataMapper inCommonDataMapper,
			SiteMasterUtilities inSiteMasterUtilities,
			IPermissionsDTODataLoader inPermissionsLoader,
			IUserDTODataLoader inuserLoader,
			IActiveDirectoryUtilities inADUtils,
			SystemMetrics inSystemMetrics,
			PermissionControllerLogic inPermissionControllerLogic,
			IFullObjectFactory factory,
			IGenBOEControllerLogic inControllerLogic)
			: base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inuserLoader, inPermissionsLoader, inControllerLogic)
		{
			_ADUtils = inADUtils;
			_permissionControllerLogic = inPermissionControllerLogic;
		}

		#region Display

		[HttpGet]
		public ViewResult ManagePermissions(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_MANAGE_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.Read, ws, null);

			ViewData["CurrentUser"] = ws.CurrentActiveUser.DisplayName;
			ViewBag.IsProjectMapWs = ws.IsProjectMapWorkspace;

			// Perform Action
			ViewResult toReturn = GetMasterView(WebConstants.VIEW_MANAGE_PERMISSIONS, workspace);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_MANAGE_PERMISSIONS, sw);
			return toReturn;
		}

		/// <summary>
		/// Get Permissions Model
		/// </summary>
		/// <param name="workspace">workspace</param>
		/// <returns>model</returns>
		[HttpPost]
		public JsonResult GetManagePermissionsModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_WORKSPACE_PERMISSION_MODEL, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.Read, ws, null);

			// Perform Action
			PermissionModelView theModelView = _GetPermissionsGrid(ws);
			JsonResult toReturn = Json(theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_GET_WORKSPACE_PERMISSION_MODEL, sw);
			return toReturn;
		}

		#endregion

		#region AJAX Calls

		/// <summary>
		/// A method to save a new permissions set.
		/// </summary>
		/// <param name="workspace">workspace for permissions</param>
		/// <param name="inPermission">SavePermission MV</param>
		/// <returns>A Json value</returns>
		[HttpPost]
		public JsonResult SaveNewPermissions(string workspace, SavePermissionsModelView inPermission)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_SAVE_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (inPermission == null)
			{
				throw new ArgumentNullException(nameof(inPermission));
			}

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				SavePermissionModelView nonWebPermissionModelView = new SavePermissionModelView() { EntityIds = inPermission.EntityIds, Roles = inPermission.Roles, WorkspaceId = inPermission.WorkspaceId };
				_permissionControllerLogic.SaveNewPermissions(workspace, new SavePermissionModelView[] { nonWebPermissionModelView });
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// if we're here, everything was successful.  Return the new data.
			PermissionModelView theModelView = _GetPermissionsGrid(ws);
			JsonResult toReturn = Json(theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_SAVE_PERMISSIONS, sw);
			return toReturn;
		}

		/// <summary>
		/// Adjust permissions for the user or group.
		/// </summary>
		/// <param name="workspace">workspace for permissions</param>
		/// <param name="inRoles">the roles the user is in</param>
		/// <param name="inType">user or group</param>
		/// <param name="inEntityId">the id of the user or group</param>
		/// <returns>A Json value, if the user is actively assigned a BOE role (Approver, Author) the edit will fail</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[HttpPost]
		public JsonResult EditPermissions(string workspace, Collection<Role> inRoles, EntityType inType, int inEntityId)
		{
			//this method basically will create a USERDTO based on if the thing beign updated is a group or user.

			// Perform Action
			if (inRoles == null)
			{
				ModelState.AddModelError("Roles", "The user must have at least one role selected");
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EDIT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			if (ModelState.IsValid)
			{
				this._permissionControllerLogic.EditPermissions(ws, inRoles, inType, inEntityId);
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			PermissionModelView theModelView = _GetPermissionsGrid(ws);
			JsonResult toReturn = Json(theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EDIT_PERMISSIONS, sw);
			return toReturn;
		}

		/// <summary>
		/// Delete all user permissions for a given workspace
		/// </summary>
		///<param name="workspace">The workspace to delete permissions from.</param>
		///<param name="inUserID">The User ID to delete permissions from.</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DeleteUserPermissions(string workspace, int inUserID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DELETE_USER_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			JsonResult toReturn = Json(new { Status = false });

			/** Valid Model Check */
			if (ModelState.IsValid)
			{
				PermissionDeleteAction action = PermissionsDelete.CheckUserAssignments(ws, inUserID, this.PermissionsLoader);

				if (action >= PermissionDeleteAction.DeleteRoleRequireReassignment)
				{
					throw new GenValidationException(String.Format(
								"{0} is assigned to at least one BOE. BOE(s) must be reassigned through the <a href=\"{1}\">Manage BOEs</a> page before the roles can be removed.",
								this.UserLoader.GetUserByID(inUserID).DisplayName,
						Url.RouteUrl(WebConstants.ROUTE_WORKSPACE, new
						{
							action = WebConstants.ACTION_INDEX,
							controller = WebConstants.CONTROLLER_BOE,
							workspace = workspace
						})));
				}
				else
				{
					List<PermissionsDTO> permissionsToSave = new List<PermissionsDTO>();

					// get permissions for workspace so we can mine them later on for the specific user/role
					List<PermissionsDTO> permissionsForWS = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id).ToList();
					Collection<PermissionsDTO> workspacePermissions = this.PermissionsLoader.GetWorkspacePermissions(ws.Id);
					permissionsForWS.AddRange(workspacePermissions);

					UserDTO user = this.UserLoader.GetUserByID(inUserID);

					// get this permissions for this user/group so we can delete it with the correct datetime
					IEnumerable<PermissionsDTO> permissionsForUser = permissionsForWS
												.Where(x => x.ETIUserId == user.UserID)
												.Select(x => x);

					int workspaceAdmins = (from r in workspacePermissions
										   where r.Role == Role.WorkspaceAdmin
										   select r.ETIUserId).Count();

					// now we have permissions for this user/group so we can delete it correctly
					foreach (PermissionsDTO permissionForUser in permissionsForUser)
					{
						// adjust updateable field and then add to our save list
						permissionForUser.Updateable = UpdateType.Deleted;
						permissionsToSave.Add(permissionForUser);


						if (permissionForUser.Role == Role.WorkspaceAdmin)
						{
							if (workspaceAdmins <= 1)
							{
								throw new GenValidationException("The Workspace Administrator cannot be deleted. In order to delete the user, at least one other Workspace Administrator must exist.");
							}
						}
					}



					// perform all saves here
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
					{
						foreach (PermissionsDTO permissionToSave in permissionsToSave)
						{
							this.PermissionsLoader.SavePermission(permissionToSave);
						}
						scope.Complete();
					}

					toReturn = Json(new { Status = true });
				}
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
			}

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_DELETE_USER_PERMISSIONS, sw);
			return toReturn;
		}
		#endregion AJAX Calls

		private PermissionModelView _GetPermissionsGrid(FullWorkspace ws)
		{
			Collection<PermissionsDTO> allPerms = this.PermissionsLoader.GetPermissionsForGridData(ws.Id);

			Collection<PermissionsGridModelView> permissions = new Collection<PermissionsGridModelView>();

			// get all the distinct groups we have to organize by.  then loop over each group name
			List<int> distinctUsers = (from u in allPerms
									   where u.WorkspaceId == ws.Id
									   select u.ETIUserId).Distinct().ToList();

			ICollection<UserDTO> users = this.UserLoader.GetByIds(distinctUsers);

			//get current user info for use later
			UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
			ICollection<GroupData> currentUserGroups = this._ADUtils.GetGroupsForUser(currentUser.NTID.ToString());
			IDictionary<int, RoleModelView> roleModelViews = this._CommonDataMapper.getRolesDictionary();

			Dictionary<UserData, bool> usersBoeMembership = this._permissionControllerLogic.GetGenBOEAccess(users.Select(x => new UserData() { Ntid = x.NTID }).ToList());

			foreach (int distinctUser in distinctUsers)
			{
				// for each group grab the users for the group.  note this will include the 'null' group
				PermissionsGridModelView modelViewToAdd = new PermissionsGridModelView();

				UserDTO userDTO = users.First(x => x.UserID == distinctUser);

				// get the users for this workspace with this distinct group and for this workspace
				// (for the 'none' entry, there should only be 1 user here .. with many roles)               
				modelViewToAdd.Users = new Collection<PermissionUserModelView> {
											(from u in allPerms
											where u.WorkspaceId == ws.Id && u.ETIUserId == distinctUser
											orderby u.ETIUserId
											select new PermissionUserModelView
											{
												DisplayName = userDTO.DisplayName,
												UserID = distinctUser
											}).FirstOrDefault()};

				// The distinct roles for the workspace/user/group combo
				Collection<Role> roles = new Collection<Role>((from u in allPerms
															   where u.WorkspaceId == ws.Id && u.ETIUserId == distinctUser
															   orderby u.ETIUserId, u.Role
															   select u.Role).Distinct().ToArray());

				// convert the distinct roles into the proper modelview class
				modelViewToAdd.Roles = new Collection<PermissionRoleModelView>(
											(from r in roles
											 select new PermissionRoleModelView
											 {
												 RoleID = (int)r,
												 RoleName = roleModelViews[(int)r].RoleName
											 }).ToArray());

				//If current user is a member of the group, add them to group members for admin checking purposes
				//move these higher to not do it every loop
				if (userDTO.NTID.Contains("."))
				{
					modelViewToAdd.isGroup = true;
					foreach (GroupData group in currentUserGroups)
					{
						if (modelViewToAdd.Users[0].DisplayName == group.DisplayName)
						{
							modelViewToAdd.groupMembers.Add(currentUser.DisplayName);
						}
					}
				}

				if (modelViewToAdd.isGroup)
				{
					modelViewToAdd.genBOEAccess = "View Users";
				}
				else
				{
					modelViewToAdd.genBOEAccess = usersBoeMembership.First(x => x.Key.Ntid == userDTO.NTID).Value ? "Yes" : "No";
				}

				permissions.Add(modelViewToAdd);
			}

			IEnumerable<PermissionsGridModelView> nonGroups = from t in permissions
															  from u in t.Users
															  orderby u.DisplayName
															  select t;

			List<PermissionsGridModelView> reGrouping = new List<PermissionsGridModelView>();
			reGrouping.AddRange(nonGroups);

			PermissionModelView toReturn = new PermissionModelView();
			toReturn.Permissions = reGrouping;
			toReturn.CurrentUserId = ws.CurrentActiveUser.UserID;
			toReturn.CurrentUserDisplayName = ws.CurrentActiveUser.DisplayName;

			return toReturn;
		}

		///<summary>
		///Get members of group
		///</summary>
		///<param name="groupName">Name of the group to get users from</param>
		[HttpPost]
		public JsonResult GetGroupMembers(string groupName)
		{
			// Member list - string for user's name, bool for having genBOE access
			Dictionary<string, bool> memberList = new Dictionary<string, bool>();
			ICollection<UserData> members = _ADUtils.GetAdGroupUsers(groupName);
			ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();

			Dictionary<UserData, bool> usersBoeMembership = this._permissionControllerLogic.GetGenBOEAccess(orderedMembers);
			foreach (UserData member in orderedMembers)
			{
				memberList.Add(member.DisplayName, usersBoeMembership[member]);
			}

			return Json(memberList);
		}

		/// <summary>
		/// Export Permissions
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <returns>The permissions export</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[HttpGet]
		public ActionResult ExportPermissions(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_EXPORT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action

			string fileName = string.Format("{0}_Permissions.xlsx", ws.WorkspaceName);
			// Generate a custom ActionResult to cause a file download to the client
			FileStream fs = this._permissionControllerLogic.ExportPermissions(ws);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_EXPORT_PERMISSIONS, sw);

			return File(
				fileStream: fs,
				contentType: ExportFileDownloadBase.GetContentType(fileName),
				fileDownloadName: fileName);
		}

		/// <summary>
		/// Import Permissions from File
		/// </summary>
		/// <param name="workspace">The workspace</param>
		/// <returns>Import Permission results</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpPost]
		public JsonResult ImportPermissions(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
			Stream importFile = Request.Files[0].InputStream;
			string errorMessage;

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_IMPORT_PERMISSIONS, SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			// If a file was uploaded successfully
			if (importFile != null)
			{
				errorMessage = this._permissionControllerLogic.ImportPermissions(workspace, importFile);
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				errorMessage = "No file selected for upload";
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new GenValidationException(errorMessage);
			}

			// if we're here, everything was successful.  Return the new data.
			PermissionModelView theModelView = _GetPermissionsGrid(ws);
			JsonResult toReturn = Json(theModelView);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_IMPORT_PERMISSIONS, sw);
			return toReturn;
		}
	}
}
