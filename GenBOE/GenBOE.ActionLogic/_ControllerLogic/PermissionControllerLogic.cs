// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Transactions;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.ActionLogic.Permissions;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;

	public class PermissionControllerLogic
	{
		private readonly Logger _log = new Logger(typeof(PermissionControllerLogic));

		protected IFullObjectFactory Factory { get; set; }
		private readonly IPermissionsDTODataLoader permissionLoader;
		private readonly IActiveDirectoryUtilities ADUtils;
		private readonly ISecurityInformation _SecurityInformation;
		private readonly IUserDTODataLoader _UserDTODataLoader;
		private readonly ICommonDataMapper _CommonDataMapper;


		public PermissionControllerLogic(IPermissionsDTODataLoader inPermissionsLoader,
			IUserDTODataLoader inUserLoader,
			IActiveDirectoryUtilities inADUtils,
			ISecurityInformation inISecurityInformation,
			IFullObjectFactory factory,
			ICommonDataMapper inCommonDataMapper)
		{
			this.Factory = factory;
			this.permissionLoader = inPermissionsLoader;
			this.ADUtils = inADUtils;
			this._SecurityInformation = inISecurityInformation;
			this._UserDTODataLoader = inUserLoader;
			this._CommonDataMapper = inCommonDataMapper;
		}

		/// <summary>
		/// Saves a single potential permission
		/// </summary>
		/// <param name="inPermission">The permission DTO to save</param>
		public void SavePotentialPermission(PermissionsDTO inPermission)
		{
			if (inPermission == null)
			{
				throw new ArgumentNullException(nameof(inPermission));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				UserDTO User = this._UserDTODataLoader.GetUserByID(inPermission.ETIUserId);

				Collection<PermissionsDTO> Permissions = this.permissionLoader.GetUserPermissions(User);

				// if user already exists throw new ArgumentException("Permissions already exist");
				if (inPermission.Updateable == UpdateType.Deleted || Permissions == null || !Permissions.Contains(inPermission))
				{
					this.permissionLoader.SavePermission(inPermission);
				}
				else
				{
					throw new ArgumentException("Permissions already exist");
				}
			}
		}

		/// <summary>
		/// Saves a new permission
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="inPermissions">Permissions to save</param>
		public void SaveNewPermissions(string workspace, ICollection<SavePermissionModelView> inPermissions)
		{
			if (inPermissions == null)
			{
				throw new ArgumentNullException(nameof(inPermissions));
			}
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			foreach (SavePermissionModelView inPermission in inPermissions)
			{
				inPermission.WorkspaceId = ws.Id;
			}

			if (inPermissions.Any(p => !p.Roles.Any()))
			{
				ValidationErrors.Add(new ValidationMessage("AtLeastOneRole", "At Least one level of access is required for every permission."));
			}

			if (inPermissions.Any(e => !e.EntityIds.Any() ||
				string.IsNullOrEmpty(e.EntityIds.FirstOrDefault()))) // the model returns an empty string 
			{
				ValidationErrors.Add(new ValidationMessage("AtLeastOneUser", "At Least one user is required for every permission."));
			}

			if (ValidationErrors.Any())
			{
				throw new GenValidationException(ValidationErrors);
			}

			ICollection<PermissionsDTO> currentWorkspacePermissions = this.permissionLoader.GetWorkspacePermissions(ws.Id);
			ICollection<PermissionsDTO> currentBoePermissions = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);
			List<string> ntIdsToClear = new List<string>();

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				foreach (SavePermissionModelView inPermission in inPermissions)
				{
					// Separate nt ids
					String[] tempids = inPermission.EntityIds[0].Split(';');
					if (tempids.Length > 1)
					{
						inPermission.EntityIds.RemoveAt(0);
						foreach (string temp in tempids)
						{
							// Filter out duplicate or blank ntids.  Ntids may be blank if the string started with a semicolon or if there were sequential semicolons.
							string tempTrimmed = temp.Trim();
							if (!inPermission.EntityIds.Contains(tempTrimmed) && !string.IsNullOrEmpty(tempTrimmed))
							{
								inPermission.EntityIds.Add(tempTrimmed);
							}
						}
					}

					// check to see if we were given a group and it exists
					foreach (string entity in inPermission.EntityIds)
					{
						UserDTO userDTO;

						if (entity.Contains('.'))
						{
							if (!this.ADUtils.IsGroup(entity.Trim()))
							{
								ValidationErrors.Add(new ValidationMessage("The group '" + entity + "' was not found"));
								throw new GenValidationException(ValidationErrors);
							}

							userDTO = this._UserDTODataLoader.GetOrCreateUserByNtid(entity);

							ICollection<UserData> groupMembers = this.ADUtils.GetAdGroupUsers(entity);
							Dictionary<UserData, bool> groupMemberAccess = this.GetGenBOEAccess(groupMembers);

							if (groupMemberAccess.Any(x => x.Value == false))
							{
								ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", string.Format("The following members of group {0} do not have access to genBOE and cannot be added to this Workspace's permissions: <ul><li>{1}</li></ul>Please have the user request access.",
									entity, string.Join("</li><li>", groupMemberAccess.Where(x => x.Value == false).Select(x => x.Key).Select(x => x.DisplayName)))));
								throw new GenValidationException(ValidationErrors);
							}
						}
						else
						{
							// details from AD
							UserData user = this.ADUtils.GetUserByQualifiedAccount(entity, false);
							if (user == null)
							{
								ValidationErrors.Add(new ValidationMessage("UserNotFound", "User not found"));
							}
							else if (this.GetGenBOEAccess(new Collection<UserData>() { user }).Any(x => !x.Value))
							{
								ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", user.DisplayName + " does not have access to genBOE and cannot be added to this Workspace's permissions. Please have the user request access."));
							}

							if (ValidationErrors.Any())
							{
								throw new GenValidationException(ValidationErrors);
							}

							// check to see if user is in the database
							userDTO = this._UserDTODataLoader.GetOrCreateUserByNtid(entity);

							if (userDTO != null)
							{
								// clear their permissions cache
								this.Factory.ClearPermissionsCache(userDTO.NTID);
								ntIdsToClear.Add(userDTO.NTID);
							}
						}

						this.ValidateWsAdminMustHaveCreateWsPermission(userDTO.NTID, inPermission.Roles);

						foreach (Role role in inPermission.Roles)
						{
							bool isSubcontractor = this._SecurityInformation.IsSubcontractorUser(userDTO.NTID, userDTO.IsSubcontractor ?? false);
							// throw exception (any pending inserts get rolled back) if attempt is made to grant any role other than Subcontractor Author to a Subcontractor
							if (isSubcontractor && role != Role.SubcontractorAuthor)
							{
								ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "Subcontractor users are only permitted Subcontractor Author permissions"));
								throw new GenValidationException(ValidationErrors);
							}

							// throw exception (any pending inserts get rolled back) if attempt is made to grant the Subcontractor Author role to an LM User
							if (!isSubcontractor && role == Role.SubcontractorAuthor)
							{
								ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "LM Users are not permitted Subcontractor Author permissions"));
								throw new GenValidationException(ValidationErrors);
							}

							// only save if permission doesn't already exist.
							if (currentWorkspacePermissions.FirstOrDefault(x => x.Role == role && x.ETIUserId == userDTO.UserID) == null &&
								currentBoePermissions.FirstOrDefault(x => x.Role == role && x.ETIUserId == userDTO.UserID) == null)
							{
								try
								{
									this.permissionLoader.SavePermission(new PermissionsDTO
									{
										BOEId = null, // no BOE at this point since we're managing workspace permissions
										ETIUserId = userDTO.UserID,
										Role = role,
										Updateable = UpdateType.Upsert,
										WorkspaceId = ws.Id
									});
								}
								catch (Exception ex)
								{
									if (ex.InnerException != null && ex.InnerException.Message == "You can not add any other roles to a user that has the Subcontractor Author role ")
									{
										throw new GenValidationException("You can not add any other roles to a user that has the Subcontractor Author role");
									}
									else
									{
										throw;
									}
								}
							}
						}

					} // end foreach entity to insert
				} // end foreach permission to insert
				scope.Complete();
			} // end transaction scope

			// Need to re-clear the permissions cache because it may have been pulled later in the transaction
			if (ntIdsToClear.Any())
			{
				ntIdsToClear = ntIdsToClear.Distinct().ToList();

				foreach (string ntid in ntIdsToClear)
				{
					this.Factory.ClearPermissionsCache(ntid);
				}
			}
		}

		/// <summary>
		/// Delete all user permissions for a given workspace
		/// </summary>
		///<param name="workspace">The workspace to delete permissions from.</param>
		///<param name="inUserID">The User ID to delete permissions from.</param>
		/// <returns></returns>
		public bool DeleteUserPermissions(string workspace, int inUserID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Perform Action
			bool toReturn = false;

			PermissionDeleteAction action = PermissionsDelete.CheckUserAssignments(ws, inUserID, this.permissionLoader);

			if (action >= PermissionDeleteAction.DeleteRoleRequireReassignment)
			{
				throw new GenValidationException(String.Format(
				"{0} is assigned to at least one BOE. BOE(s) must be reassigned through the <a href=\"{1}\">Manage BOEs</a> page before the roles can be removed.",
				this._UserDTODataLoader.GetUserByID(inUserID).DisplayName, workspace
				));
			}
			else
			{
				List<PermissionsDTO> permissionsToSave = new List<PermissionsDTO>();

				// get permissions for workspace so we can mine them later on for the specific user/role
				List<PermissionsDTO> permissionsForWS = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id).ToList();
				Collection<PermissionsDTO> workspacePermissions = this.permissionLoader.GetWorkspacePermissions(ws.Id);
				permissionsForWS.AddRange(workspacePermissions);

				UserDTO user = this._UserDTODataLoader.GetUserByID(inUserID);

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
						this.permissionLoader.SavePermission(permissionToSave);
					}
					scope.Complete();
				}

				toReturn = true;
			}

			return toReturn;
		}

		/// <summary>
		/// Edit existing Permissions for groups or single users
		/// </summary>
		/// <param name="workspace">Full Workspace</param>
		/// <param name="inRoles">Roles</param>
		/// <param name="inType">User or Group</param>
		/// <param name="inEntityId">User/Group Id</param>
		/// <exception cref="GenValidationException"></exception>
		public void EditPermissions(FullWorkspace workspace, Collection<Role> inRoles, EntityType inType, int inEntityId)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			inRoles = new Collection<Role>(inRoles.Distinct().ToList());

			PermissionDeleteAction action = PermissionDeleteAction.Invalid;
			Collection<UserDTO> usersToReassign = new Collection<UserDTO>();

			// if this is a group, get the users in the group
			Collection<UserDTO> usersToAdjust = new Collection<UserDTO>();

			if (inType == EntityType.User)
			{
				// Get the roles that this user had previously which are being removed by this edit
				ICollection<Role> removedRoles = (from permission in this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id)
												  where permission.ETIUserId == inEntityId &&
												  (inRoles == null || !inRoles.Contains(permission.Role))
												  select permission.Role).Distinct().ToList();

				ICollection<PermissionsDTO> workspacePermissions = this.permissionLoader.GetWorkspacePermissions(workspace.Id);
				// Get the users current roles
				ICollection<Role> currentRoles = (from permission in workspacePermissions
												  where permission.ETIUserId == inEntityId
												  select permission.Role).Distinct().ToList();

				int workspaceAdmins = (from r in workspacePermissions
									   where r.Role == Role.WorkspaceAdmin
									   select r.ETIUserId).Count();

				if (workspaceAdmins <= 1)
				{
					if (currentRoles.Contains(Role.WorkspaceAdmin) && !inRoles.Contains(Role.WorkspaceAdmin))
					{
						ValidationErrors.Add(new ValidationMessage("WorkspaceAdmin", "The Workspace Administrator cannot be deleted. In order to delete the user, at least one other Workspace Administrator must exist."));
					}
				}

				// perform checks for the Subcontractor Author role
				UserDTO currentUser = this._UserDTODataLoader.GetUserByID(inEntityId);
				this.Factory.ClearPermissionsCache(currentUser.NTID);
				bool isSubcontractor = _SecurityInformation.IsSubcontractorUser(currentUser.NTID, currentUser.IsSubcontractor ?? false);

				this.ValidateWsAdminMustHaveCreateWsPermission(currentUser.NTID, inRoles);

				if (isSubcontractor && (!inRoles.Contains(Role.SubcontractorAuthor) || inRoles.Count() > 1))
				{
					ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "Subcontractor users are only permitted Subcontractor Author permissions"));
				}
				if (!isSubcontractor && inRoles.Contains(Role.SubcontractorAuthor))
				{
					ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "LM Users are not permitted Subcontractor Author permissions"));
				}

				// perform checks for genBOE access
				if (currentUser.NTID.Contains('.') && this.ADUtils.IsGroup(currentUser.NTID))
				{
					ICollection<UserData> groupMembers = this.ADUtils.GetAdGroupUsers(currentUser.NTID);
					Dictionary<UserData, bool> groupMemberAccess = this.GetGenBOEAccess(groupMembers);

					if (groupMemberAccess.Any(x => !x.Value))
					{
						ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", string.Format("The following members of group {0} do not have access to genBOE and therefore the group's permissions cannot be changed: <ul><li>{1}</li></ul>Please have the user request access.",
							currentUser.NTID, string.Join("</li><li>", groupMemberAccess.Where(x => !x.Value).Select(x => x.Key).Select(x => x.DisplayName)))));
						throw new GenValidationException(ValidationErrors);
					}
				}
				else
				{
					Dictionary<UserData, bool> genBoeAccess = this.GetGenBOEAccess(new Collection<UserData>() { new UserData() { Ntid = currentUser.NTID } });
					if (genBoeAccess.Any(x => !x.Value))
					{
						ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", "The user does not have access to genBOE and their permissions cannot be changed. Please have the user request access."));
					}
				}

				// Check the Workspace's BOEs for assignments using one of the roles being removed
				action = PermissionsDelete.CheckUserAssignments(
					workspace,
					inEntityId,
					removedRoles,
					this.permissionLoader);

				// If the action requires reassignment, then the user is the sole user to reassign
				if (action >= PermissionDeleteAction.DeleteRoleRequireReassignment)
				{
					usersToReassign = new Collection<UserDTO> { this._UserDTODataLoader.GetUserByID(inEntityId) };
				}

				usersToAdjust.Add(this._UserDTODataLoader.GetUserByID(inEntityId));
			}
			else
			{
				ValidationErrors.Add(new ValidationMessage("The account could not be resolved as a user or a group."));
			}

			// If the action for the group or user requires reassignment, then we'll pass back a failed status
			// and the list of users that must be reassigned
			if (action >= PermissionDeleteAction.DeleteRoleRequireReassignment)
			{
				ValidationErrors.Add(new ValidationMessage("Delete Failed", String.Format(
					"The following users are assigned to at least one BOE. BOE(s) must be reassigned through the <a href=\"{0}\">Manage BOEs</a> page before the roles can be removed.<BR/>{1}",
					$"{WebConstants.ROUTE_WORKSPACE}/{WebConstants.ACTION_INDEX}/{WebConstants.CONTROLLER_BOE}/{workspace.Shortname}",
					String.Join("<BR/>", usersToReassign.Select(u => u.DisplayName).ToArray()))));
			}
			// Otherwise, we can go ahead and edit the roles.
			else
			{
				if (ValidationErrors.Any())
				{
					throw new GenValidationException(ValidationErrors);
				}
				// perform the actual edit ... we've made our list and checked it twice
				this._EditPermissions(inRoles, workspace, usersToAdjust);
			}

			// if I'm trying to delete a WS permission that has BOE level permissions, prevent that..
			if (ValidationErrors.Any())
			{
				throw new GenValidationException(ValidationErrors);
			}
		}

		/// <summary>
		/// Edit permissions
		/// </summary>
		/// <param name="inRoles">The roles selected from the UI</param>
		/// <param name="ws">The workspace</param>
		/// <param name="usersToAdjust">the users to check permissions for and adjust</param>
		/// <returns>string.empty if no errors, otherwise errors returned in the string</returns>
		private void _EditPermissions(Collection<Role> inRoles, FullWorkspace ws, Collection<UserDTO> usersToAdjust)
		{
			// by now we've found all roles are ok, none are in use that are attempting to edit.  
			Collection<PermissionsDTO> toSave = new Collection<PermissionsDTO>();

			foreach (UserDTO user in usersToAdjust)
			{
				Collection<Role> newRoles = new Collection<Role>(inRoles.Distinct().ToList());
				List<PermissionsDTO> permissionsForUser = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id).ToList();
				permissionsForUser.AddRange(this.permissionLoader.GetWorkspacePermissions(ws.Id));

				// get the workspace level permissions this user has assigned right now (in the database)
				permissionsForUser =
							permissionsForUser
									.Where(x => x.ETIUserId == user.UserID &&
												x.BOEId == null &&
												((x.Role >= Role.Author && x.Role <= Role.WorkspaceAdmin) || x.Role == Role.SubcontractorAuthor || x.Role == Role.SubcontractAdmin) &&
												x.WorkspaceId == ws.Id).Select(x => x).ToList<PermissionsDTO>();

				foreach (PermissionsDTO currentRoll in permissionsForUser)
				{
					bool delete = true;
					foreach (Role uiPickedRole in newRoles)
					{
						if (currentRoll.Role == uiPickedRole)
						{
							delete = false;
							break;
						}
					}

					if (delete)
					{
						// insert the new role for all users in the group.. the UI didn't previously have it assigned
						toSave.Add(new PermissionsDTO
						{
							BOEId = null, // no BOE Id b/c this is potential
							ETIUserId = user.UserID,
							Role = currentRoll.Role,
							Updateable = UpdateType.Deleted,
							UpdateDate = currentRoll.UpdateDate,
							WorkspaceId = ws.Id
						});
					}
					//removed all handled existing roles (deletes or remain the same)
					newRoles.Remove(currentRoll.Role);
				}

				//Add any new roles
				foreach (Role uiPickedRole in newRoles)
				{
					toSave.Add(new PermissionsDTO
					{
						BOEId = null, // no BOE Id b/c this is potential
						ETIUserId = user.UserID,
						Role = uiPickedRole,
						Updateable = UpdateType.Upsert,
						WorkspaceId = ws.Id
					});
				}
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				foreach (PermissionsDTO save in toSave)
				{
					try
					{
						this.permissionLoader.SavePermission(save);
					}
					catch (Exception ex)
					{
						if (ex.InnerException != null && ex.InnerException.Message == "You can not add any other roles to a user that has the Subcontractor Author role ")
						{
							throw new GenValidationException("You can not add any other roles to a user that has the Subcontractor Author role");
						}
						else
						{
							throw;
						}
					}
				}
				scope.Complete();
			}
		}

		/// <summary>
		/// Helper function to get permissions grid on GetPermissions
		/// </summary>
		/// <param name="ws">Full Workspace</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public PermissionViewModel _GetPermissionsGrid(FullWorkspace ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			Collection<PermissionsDTO> allPerms = this.permissionLoader.GetPermissionsForGridData(ws.Id);

			Collection<PermissionsGridViewModel> permissions = new Collection<PermissionsGridViewModel>();

			// get all the distinct groups we have to organize by.  then loop over each group name
			List<int> distinctUsers = (from u in allPerms
									   where u.WorkspaceId == ws.Id
									   select u.ETIUserId).Distinct().ToList();

			ICollection<UserDTO> users = this._UserDTODataLoader.GetByIds(distinctUsers);

			//get current user info for use later
			UserDTO currentUser = this._UserDTODataLoader.GetUserForActiveUser();
			ICollection<GroupData> currentUserGroups = this.ADUtils.GetGroupsForUser(currentUser.NTID.ToString());
			IDictionary<int, RoleModelView> roleModelViews = this._CommonDataMapper.getRolesDictionary();

			Dictionary<UserData, bool> usersBoeMembership = GetGenBOEAccess(users.Select(x => new UserData() { Ntid = x.NTID }).ToList());

			foreach (int distinctUser in distinctUsers)
			{
				// for each group grab the users for the group.  note this will include the 'null' group
				PermissionsGridViewModel modelViewToAdd = new PermissionsGridViewModel();

				UserDTO userDTO = users.First(x => x.UserID == distinctUser);

				// get the users for this workspace with this distinct group and for this workspace
				// (for the 'none' entry, there should only be 1 user here .. with many roles)               
				modelViewToAdd.Users = new Collection<PermissionUserViewModel> {
											(from u in allPerms
											where u.WorkspaceId == ws.Id && u.ETIUserId == distinctUser
											orderby u.ETIUserId
											select new PermissionUserViewModel
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
				modelViewToAdd.Roles = new Collection<PermissionRoleViewModel>(
											(from r in roles
											 select new PermissionRoleViewModel
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

			IEnumerable<PermissionsGridViewModel> nonGroups = from t in permissions
															  from u in t.Users
															  orderby u.DisplayName
															  select t;

			List<PermissionsGridViewModel> reGrouping = new List<PermissionsGridViewModel>();
			reGrouping.AddRange(nonGroups);

			PermissionViewModel toReturn = new PermissionViewModel();
			toReturn.Permissions = reGrouping;
			toReturn.CurrentUserId = ws.CurrentActiveUser.UserID;
			toReturn.CurrentUserDisplayName = ws.CurrentActiveUser.DisplayName;
			toReturn.IsPastCutOffDate = ws.CreationDate > Utilities.ShowINLCutoffDate;
			toReturn.WorkspaceName = ws.WorkspaceName;

			return toReturn;
		}

		/// <summary>
		/// Checks if users have access to GenBoe
		/// </summary>
		/// <param name="usersToCheck">Users to check</param>
		/// <returns>A dictionary of users and a bool indicating if they have access or not</returns>
		public Dictionary<UserData, bool> GetGenBOEAccess(ICollection<UserData> usersToCheck)
		{
			ICollection<GroupData> groups = this.ADUtils.GetAuthorizationGroupsFromWebConfig();

			Dictionary<UserData, bool> result = ADUtils.CheckUsersBoeAccess(usersToCheck, groups);

			return result;
		}

		/// <summary>
		/// If we are assigning a WS Admin role, we need to make sure that:
		///     the user has a create WS role
		///     OR the user is a system admin
		/// </summary>
		/// <param name="wsId">Workspace Id</param>
		/// <param name="ntid">User's NTID</param>
		/// <param name="Roles">Roles being assigned</param>
		/// <exception cref="GenValidationException">If invalid, the method throws a validation exception.</exception>
		public void ValidateWsAdminMustHaveCreateWsPermission(string ntid, ICollection<Role> Roles)
		{
			if (Roles.Any(x => x == Role.WorkspaceAdmin))
			{
				// need to figure out if the ntid belongs to a group.. if yes, then break it up into users and run it through.
				// else, just check the user

				if (this.ADUtils.IsGroup(ntid))
				{
					List<string> ntidsInGroup = this.ADUtils.GetAdGroupUsers(ntid).Select(x => x.Ntid).ToList();
					ntidsInGroup.ForEach(NTID => this.ValidateWsAdminMustHaveCreateWsPermission(NTID, Roles));
				}
				else
				{
					IReadOnlyCollection<SecurityPermissionsResponse> permissions = this.Factory.GetPermissionsForUser(ntid);
					bool isAllowedToCreateWs = permissions.Any(x => x.AuthorizedRole == Role.CreateWorkspacePermissions);
					bool isSystemAdmin = permissions.Any(x => x.AuthorizedRole == Role.SystemAdmin);

					if (!isAllowedToCreateWs && !isSystemAdmin)
					{
						throw new GenValidationException($"User {ntid} cannot be assigned 'Workspace Administrator' permissions, because they do not have 'Create Workspace' permissions.");
					}
				}
			}
		}

		/// <summary>
		/// Export Permissions logic
		/// </summary>
		/// <param name="ws">Full Workspace</param>
		/// <returns>Excel file as FileStream</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[SuppressMessage("Microsoft.Design", "CA1011: Consider passing base types as parameters")]
		public FileStream ExportPermissions(FullWorkspace ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			FileStream fs = null;

			// Get the Permissions template file name
			// Assume that "Templates" is a subdirectory of your application's root directory
			string templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Export");

			// Ensure the template directory exists
			if (!Directory.Exists(templateDir))
			{
				throw new InvalidOperationException($"Template directory '{templateDir}' does not exist.");
			}

			// Get data to export for this Workspace
			Collection<PermissionsDTO> allPerms = this.permissionLoader.GetPermissionsForGridData(ws.Id).Where(p => p.Role != Role.WorkspaceUser).ToCollection();

			// Get export template file name
			string templateFileName = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST
				? Path.Combine(templateDir, "PermissionsRMS.xlsx")
				: Path.Combine(templateDir, "Permissions.xlsx");

			// Check if the template file exists
			if (!File.Exists(templateFileName))
			{
				throw new FileNotFoundException($"Template file '{templateFileName}' does not exist.");
			}

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFile = PermissionsExporter.ExportToExcelFile(templateFileName, allPerms);

			if (exportedFile.Length > 0)
			{
				fs = new FileStream(exportedFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
			}

			return fs;
		}

		/// <summary>
		/// Import Permissions logic
		/// </summary>
		/// <param name="workspace">Workspace name</param>
		/// <param name="inputStream">File Stream</param>
		/// <returns>Error message</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public string ImportPermissions(string workspace, Stream inputStream)
		{
			string errorMessage = string.Empty;
			try
			{
				ICollection<SavePermissionModelView> permissionsFromImportFile = PermissionsImporter.ImportFromExcelFile(inputStream);

				if (permissionsFromImportFile.Any())
				{
					SaveNewPermissions(workspace, permissionsFromImportFile);
				}
				else
				{
					errorMessage = "No Permissions Were Imported.";
				}
			}
			// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
			// exception messages to display for the user
			catch (NotExcelFileException)
			{
				errorMessage = "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).";
			}
			catch (ColumnMissingException ex2)
			{
				errorMessage = string.Format("File does not contain all of the required columns. File must contain 'NtId', 'Role' columns. The following columns are missing: {0}.", ex2.Message);
			}
			catch (CellValueMissingException ex3)
			{
				errorMessage = string.Format("A row in the file does not contain a value for NtId and Role. Every filled row must have a value for each. Check the following column: {0}.", ex3.Message);
			}
			catch (DuplicateValuesException ex4)
			{
				errorMessage = string.Format("Values must be unique. The following are not unique: {0}", ex4.Message);
			}
			catch (EntityCommandExecutionException)
			{
				errorMessage = "The Permissions were recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.";
			}
			catch (GenValidationException ex5)
			{
				errorMessage = string.Format(ex5.ValidationList[0].ValidationIssue);
			}
			catch (Exception ex)
			{
				_log.Error(ex, "Unknown Import Permissions Error.");
				errorMessage = "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.";
			}

			return errorMessage;
		}

		/// <summary>
		/// Get Users for Dropdown list "Users" in BulkAssign BOEs page
		/// </summary>
		/// <param name="workspaceId">Workspace ID to get ALL Users for</param>
		/// <returns>Tuple of Users for the Workspace</returns>
		[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public ICollection<Tuple<Role, Collection<SelectListItem>>> GetDropdownUsers(int workspaceId)
		{
			ICollection<Tuple<Role, Collection<SelectListItem>>> dropdownUserGroups = new List<Tuple<Role, Collection<SelectListItem>>>();

			Collection<PermissionsDTO> potentialBOEPermissions = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(workspaceId);
			// Approver Names
			IEnumerable<int> approverIDs = (from p in potentialBOEPermissions
											where p.Role == Role.Approver
											select p.ETIUserId).Distinct();
			Collection<SelectListItem> approvers = ActionLogicUtility.CreateUserSelectList(approverIDs.ToCollection(), false, _UserDTODataLoader, ADUtils);

			dropdownUserGroups.Add(new Tuple<Role, Collection<SelectListItem>>(Role.Approver, approvers));

			// Author Names
			IEnumerable<int> authorIDs = (from p in potentialBOEPermissions
										  where p.Role == Role.Author
										  select p.ETIUserId).Distinct();
			Collection<SelectListItem> authors = ActionLogicUtility.CreateUserSelectList(authorIDs.ToCollection(), false, _UserDTODataLoader, ADUtils);
			dropdownUserGroups.Add(new Tuple<Role, Collection<SelectListItem>>(Role.Author, authors));


			// Subcontractor Author Names
			IEnumerable<int> subcontractorAuthorIDs = (from p in potentialBOEPermissions
													   where p.Role == Role.SubcontractorAuthor
													   select p.ETIUserId).Distinct();
			Collection<SelectListItem> subcontractorAuthors = ActionLogicUtility.CreateUserSelectList(subcontractorAuthorIDs.ToCollection(), true, _UserDTODataLoader, ADUtils);
			dropdownUserGroups.Add(new Tuple<Role, Collection<SelectListItem>>(Role.SubcontractorAuthor, subcontractorAuthors));

			return dropdownUserGroups;
		}
	}
}
