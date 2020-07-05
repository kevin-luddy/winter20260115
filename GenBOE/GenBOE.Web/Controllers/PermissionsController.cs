// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Permissions;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;

    public class PermissionsController : GenBOEController
    {
        private Logger _log = new Logger(typeof(PermissionsController));
        private IActiveDirectoryUtilities _ADUtils = null;
        private ISecurityInformation _SecurityInformation;
        private PermissionControllerLogic _permissionControllerLogic;

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
            ISecurityInformation inISecurityInformation,
            PermissionControllerLogic inPermissionControllerLogic,
            IFullObjectFactory factory,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inuserLoader, inPermissionsLoader, inControllerLogic)
        {
            _ADUtils = inADUtils;
            _SecurityInformation = inISecurityInformation;
            _permissionControllerLogic = inPermissionControllerLogic;
        }

        #region Display

        public ViewResult ManagePermissions(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "ManagePermissions", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.Read, ws, null);

            ViewData["CurrentUser"] = ws.CurrentActiveUser.DisplayName;
            ViewBag.IsProjectMapWs = ws.IsProjectMapWorkspace;

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_MANAGE_PERMISSIONS, workspace);

            // Finalize Action
            FinalizeAction(_log, "ManagePermissions", sw);
            return toReturn;
        }

        /// <summary>
        /// Get Permissions Model
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <returns>model</returns>
        public JsonResult GetManagePermissionsModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "GetManagePermissionsModel", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.Read, ws, null);

            // Perform Action
            PermissionModelView theModelView = _GetPermissionsGrid(ws);
            JsonResult toReturn = Json(theModelView);

            // Finalize Action
            FinalizeAction(_log, "GetManagePermissionsModel", sw);
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
        public JsonResult SaveNewPermissions(string workspace, SavePermissionsModelView inPermission)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "SaveNewPermissions", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }

            /** Valid Model Check */
            if (ModelState.IsValid)
            {
                SavePermissionModelView nonWebPermissionModelView = new SavePermissionModelView() { EntityIds = inPermission.EntityIds, Roles = inPermission.Roles, WorkspaceId = inPermission.WorkspaceId };
                _permissionControllerLogic.SaveNewPermission(workspace, nonWebPermissionModelView);
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            // if we're here, everything was successful.  Return the new data.
            PermissionModelView theModelView = _GetPermissionsGrid(ws);
            JsonResult toReturn = Json(theModelView);

            // Finalize Action
            FinalizeAction(_log, "SaveNewPermissions", sw);
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
            Stopwatch sw = InitializeAction(_log, "EditPermissions", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            if (ModelState.IsValid)
            {
                inRoles = new Collection<Role>(inRoles.Distinct().ToList());

                PermissionDeleteAction action = PermissionDeleteAction.Invalid;
                Collection<UserDTO> usersToReassign = new Collection<UserDTO>();

                // if this is a group, get the users in the group
                Collection<UserDTO> usersToAdjust = new Collection<UserDTO>();

                if (inType == EntityType.User)
                {
                    // Get the roles that this user had previously which are being removed by this edit
                    ICollection<Role> removedRoles = (from permission in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id)
                                                      where permission.ETIUserId == inEntityId &&
                                                      (inRoles == null || !inRoles.Contains(permission.Role))
                                                      select permission.Role).Distinct().ToList();

                    // Get the users current roles
                    ICollection<Role> currentRoles = (from permission in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
                                                      where permission.ETIUserId == inEntityId
                                                      select permission.Role).Distinct().ToList();

                    int workspaceAdmins = (from r in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
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
                    UserDTO currentUser = this.UserLoader.GetUserByID(inEntityId);
                    this.Factory.ClearPermissionsCache(currentUser.NTID);
                    bool isSubcontractor = _SecurityInformation.IsSubcontractorUser(currentUser.NTID, currentUser.IsSubcontractor ?? false);

                    if (isSubcontractor && (!inRoles.Contains(Role.SubcontractorAuthor) || inRoles.Count() > 1))
                    {
                        ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "Subcontractor users are only permitted Subcontractor Author permissions"));
                    }
                    if (!isSubcontractor && inRoles.Contains(Role.SubcontractorAuthor))
                    {
                        ValidationErrors.Add(new ValidationMessage("SubcontractorAuthor", "LM Users are not permitted Subcontractor Author permissions"));
                    }

                    // perform checks for genBOE access
                    if (currentUser.NTID.Contains('.') && this._ADUtils.IsGroup(currentUser.NTID))
                    {
                        ICollection<UserData> groupMembers = this._ADUtils.GetAdGroupUsers(currentUser.NTID);
                        Dictionary<UserData, bool> groupMemberAccess = this._permissionControllerLogic.GetGenBOEAccess(groupMembers);

                        if (groupMemberAccess.Any(x => x.Value == false))
                        {
                            ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", string.Format("The following members of group {0} do not have access to genBOE and therefore the group's permissions cannot be changed: <ul><li>{1}</li></ul>Please have the user request access.",
                                currentUser.NTID, string.Join("</li><li>", groupMemberAccess.Where(x => x.Value == false).Select(x => x.Key).Select(x => x.DisplayName)))));
                            throw new GenValidationException(ValidationErrors);
                        }
                    }
                    else
                    {
                        Dictionary<UserData, bool> genBoeAccess = this._permissionControllerLogic.GetGenBOEAccess(new Collection<UserData>() { new UserData() { Ntid = currentUser.NTID } });
                        if (genBoeAccess.Any(x => x.Value == false))
                        {
                            ValidationErrors.Add(new ValidationMessage("NoGenBoeAccess", "The user does not have access to genBOE and their permissions cannot be changed. Please have the user request access."));
                        }
                    }

                    // Check the Workspace's BOEs for assignments using one of the roles being removed
                    action = PermissionsDelete.CheckUserAssignments(
                        ws,
                        inEntityId,
                        removedRoles,
                        this.PermissionsLoader);

                    // If the action requires reassignment, then the user is the sole user to reassign
                    if (action >= PermissionDeleteAction.DeleteRoleRequireReassignment)
                    {
                        usersToReassign = new Collection<UserDTO> { this.UserLoader.GetUserByID(inEntityId) };
                    }

                    usersToAdjust.Add(this.UserLoader.GetUserByID(inEntityId));
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
                        Url.RouteUrl(WebConstants.ROUTE_WORKSPACE, new
                        {
                            action = WebConstants.ACTION_INDEX,
                            controller = WebConstants.CONTROLLER_BOE,
                            workspace = workspace
                        }),
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
                    this._EditPermissions(inRoles, ws, usersToAdjust);
                }

                // if I'm trying to delete a WS permission that has BOE level permissions, prevent that..



                if (ValidationErrors.Any())
                {
                    throw new GenValidationException(ValidationErrors);
                }
            }
            else
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            PermissionModelView theModelView = _GetPermissionsGrid(ws);
            JsonResult toReturn = Json(theModelView);

            // Finalize Action
            FinalizeAction(_log, "EditPermissions", sw);
            return toReturn;
        }

        /// <summary>
        /// Checks to see if the action will remove the persons admin access for that workspace.
        /// </summary>
        /// <param name="workspace">The workspace name</param>
        /// <param name="inType">the type of entity the action is beign performed on.</param>
        /// <param name="inEntityId">The entity ID</param>
        /// <param name="inRoles">The list of roles(if this is null a delete is being performed)</param>
        /// <returns></returns>
        public JsonResult CheckIfUserWillLoseTheirAdminAccess(String workspace, int inEntityId, Collection<Role> inRoles)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            Stopwatch sw = InitializeAction(_log, "CheckIfUserWillLoseTheirAdminAccess", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            int currentUserID = ws.CurrentActiveUser.UserID;


            if (inRoles != null)
            {//means we are editing a person or group
                if (inRoles.Contains(Role.WorkspaceAdmin))
                {
                    //if they contain WS admin then just return false since they will not loose it.
                    return Json(new { Status = false });
                }
            }

            // Get the users current roles
            ICollection<PermissionsDTO> allCurrentUsersAdminRoleForThisWorkspace = (from permission in this.PermissionsLoader.GetWorkspacePermissions(ws.Id)
                                                                                    where permission.ETIUserId == currentUserID && permission.WorkspaceId == ws.Id && permission.Role == Role.WorkspaceAdmin
                                                                                    select new PermissionsDTO
                                                                                    {
                                                                                        ETIUserId = permission.ETIUserId,
                                                                                        Role = (Role)permission.Role,
                                                                                        WorkspaceId = ws.Id,
                                                                                        UpdateDate = permission.UpdateDate
                                                                                    }).ToList();

            if (inEntityId != currentUserID)
            {
                return Json(new { Status = false });
            }

            if (allCurrentUsersAdminRoleForThisWorkspace.Count() > 1)
            {
                FinalizeAction(_log, "CheckIfUserWillLoseTheirAdminAccess", sw);
                return Json(new { Status = false });
            }
            else
            {
                FinalizeAction(_log, "CheckIfUserWillLoseTheirAdminAccess", sw);
                return Json(new { Status = true });
            }
        }

        /// <summary>
        /// Delete all user permissions for a given workspace
        /// </summary>
        ///<param name="workspace">The workspace to delete permissions from.</param>
        ///<param name="inUserID">The User ID to delete permissions from.</param>
        /// <returns></returns>
        public JsonResult DeleteUserPermissions(string workspace, int inUserID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DeleteUserPermissions", SecurityPage.WorkspaceAdminPermissions, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

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
            FinalizeAction(_log, "DeleteUserPermissions", sw);
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

        /// <summary>
        /// Edit permissions
        /// </summary>
        /// <param name="inRoles">The roles selected from the UI</param>
        /// <param name="inEntityId">the entity id (groupId or userId)</param>
        /// <param name="workspaceID">workspace id</param>
        /// <param name="usersToAdjust">the users to check permissions for and adjust</param>
        /// <returns>string.empty if no errors, otherwise errors returned in the string</returns>
        private void _EditPermissions(Collection<Role> inRoles, FullWorkspace ws, Collection<UserDTO> usersToAdjust)
        {
            // by now we've found all roles are ok, none are in use that are attempting to edit.  
            Collection<PermissionsDTO> toSave = new Collection<PermissionsDTO>();

            foreach (UserDTO user in usersToAdjust)
            {
                Collection<Role> newRoles = new Collection<Role>(inRoles.Distinct().ToList());
                List<PermissionsDTO> permissionsForUser = this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id).ToList();
                permissionsForUser.AddRange(this.PermissionsLoader.GetWorkspacePermissions(ws.Id));

                // get the workspace level permissions this user has assigned right now (in the database)
                permissionsForUser =
                            permissionsForUser
                                    .Where(x => x.ETIUserId == user.UserID &&
                                                x.BOEId == null &&
                                                ((x.Role >= Role.Author && x.Role <= Role.WorkspaceAdmin) || x.Role == Role.SubcontractorAuthor || x.Role == Role.SubcontractAdmin) &&
                                                x.WorkspaceId == ws.Id).Select(x => x).ToList<PermissionsDTO>();

                foreach (PermissionsDTO currentRoll in permissionsForUser)
                {
                    Boolean delete = true;
                    foreach (Role uiPickedRole in newRoles)
                    {
                        if (currentRoll.Role == uiPickedRole)
                        {
                            delete = false;
                            break;
                        }
                    }

                    if (delete == true)
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
                        this.PermissionsLoader.SavePermission(save);
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

        ///<summary>
        ///Get members of group
        ///</summary>
        ///<param name="groupName">Name of the group to get users from</param>
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
    }
}
