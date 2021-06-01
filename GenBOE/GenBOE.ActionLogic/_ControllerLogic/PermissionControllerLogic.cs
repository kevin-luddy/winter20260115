// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public class PermissionControllerLogic
    {
        private Logger _log = new Logger(typeof(PermissionControllerLogic));

        protected IFullObjectFactory Factory { get; set; }
        private IPermissionsDTODataLoader permissionLoader;
        private IActiveDirectoryUtilities ADUtils;
        private ISecurityInformation _SecurityInformation;
        private IUserDTODataLoader _UserDTODataLoader;

        public PermissionControllerLogic(IPermissionsDTODataLoader inPermissionsLoader,
            IUserDTODataLoader inUserLoader,
            IActiveDirectoryUtilities inADUtils,
            ISecurityInformation inISecurityInformation,
            IFullObjectFactory factory)
        {
            this.Factory = factory;
            this.permissionLoader = inPermissionsLoader;
            this.ADUtils = inADUtils;
            this._SecurityInformation = inISecurityInformation;
            this._UserDTODataLoader = inUserLoader;
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
        /// <param name="inPermission">Permission to save</param>
        public void SaveNewPermission(string workspace, SavePermissionModelView inPermission)
        {
            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }
            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            inPermission.WorkspaceId = ws.Id;

            if (!inPermission.Roles.Any())
            {
                ValidationErrors.Add(new ValidationMessage("AtLeastOneRole", "At Least one level of access is required."));
            }

            if (!inPermission.EntityIds.Any() ||
                string.IsNullOrEmpty(inPermission.EntityIds.FirstOrDefault())) // the model returns an empty string 
            {
                ValidationErrors.Add(new ValidationMessage("AtLeastOneUser", "At Least one user is required."));
            }

            if (ValidationErrors.Any())
            {
                throw new GenValidationException(ValidationErrors);
            }

            ICollection<PermissionsDTO> currentWorkspacePermissions = this.permissionLoader.GetWorkspacePermissions(ws.Id);
            ICollection<PermissionsDTO> currentBoePermissions = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                // Seperate nt ids
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

                scope.Complete();
            } // end transaction scope
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
            if(Roles.Any(x => x == Role.WorkspaceAdmin))
            {
                // need to figure out if the ntid belongs to a group.. if yes, then break it up into users and run it through.
                // else, just check the user

                if(this.ADUtils.IsGroup(ntid))
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
    }
}
