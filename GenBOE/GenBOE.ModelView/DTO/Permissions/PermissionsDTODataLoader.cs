// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections.ObjectModel;
    using GenBOE.Models;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.Dtos;

    public class PermissionsDTODataLoader : IPermissionsDTODataLoader
    {
        private Logger _log = new Logger(typeof(PermissionsDTODataLoader));

        private IActiveDirectoryUtilities adUtils;

        public PermissionsDTODataLoader(IActiveDirectoryUtilities adUtils)
        {
            this.adUtils = adUtils;
        }

        /// <summary>
        /// Returns all Workspace Permissions for the User
        /// </summary>
        /// <param name="userIds">User Id</param>
        /// <returns>All WS Permissions for that user</returns>
        [DbQuery]
        virtual public Collection<int> GetWorkspaceIdsThatUsersHaveAccessTo(ICollection<int> userIds)
        {
            Collection<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    List<int> workspaceIds = (from boeRoles in gbe.BOEUserRoles // actual Boe level roles
                                              join boes in gbe.BOEs on boeRoles.BOEID equals boes.BOEID
                                              where userIds.Contains(boeRoles.ETIUserID)
                                              select boes.WorkspaceID
                                             ).Union(
                                              from wsRoles in gbe.WorkspaceUserRoles // Workspace level roles
                                              where userIds.Contains(wsRoles.ETIUserID)
                                              select wsRoles.WorkspaceID
                                             ).Distinct().ToList();

                    toReturn = new Collection<int>(workspaceIds);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns Workspace Ids for the Users where the Users are workspace admins
        /// </summary>
        /// <param name="userIds">User Ids</param>
        /// <returns>Workspace Ids</returns>
        [DbQuery]
        virtual public Collection<int> GetWorkspaceIdsWhereUserIsWorkspaceAdmin(ICollection<int> userIds)
        {
            Collection<int> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    List<int> workspaceIds = (from wsRoles in gbe.WorkspaceUserRoles // Workspace level roles
                                              where userIds.Contains(wsRoles.ETIUserID) && wsRoles.RoleID == (int)Role.WorkspaceAdmin
                                              select wsRoles.WorkspaceID).Distinct().ToList();

                    toReturn = new Collection<int>(workspaceIds);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all BOE Permissions in the DB
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetAllBOEPermissions()
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var boeUsers = from role in gbe.BOEUserRoles
                                   from boe in gbe.BOEs
                                   where boe.BOEID == role.BOEID
                                   select new PermissionsDTO
                                   {
                                       PermissionId = role.BOEUserRoleID,
                                       ETIUserId = role.ETIUserID,
                                       Role = (Role)role.RoleID,
                                       WorkspaceId = boe.WorkspaceID,
                                       BOEId = role.BOEID,
                                       UpdateDate = role.UpdateDT
                                   };

                    toReturn = new Collection<PermissionsDTO>(boeUsers.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all BOE Potential Permissions in the DB
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetAllBOEPotentialPermissions()
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var boeUsers = from role in gbe.BOEPotentialRoles
                                   select new PermissionsDTO
                                   {
                                       PermissionId = role.BOEPotentialRoleID,
                                       ETIUserId = role.ETIUserID,
                                       Role = (Role)role.RoleID,
                                       WorkspaceId = role.WorkspaceID,
                                       UpdateDate = role.UpdateDT
                                   };

                    toReturn = new Collection<PermissionsDTO>(boeUsers.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Return the workspace security permissions (WorkspaceAdmin, WorkspaceUser, WorkspaceReviewer, SubcontractAdministrator) for a given workspace
        /// </summary>
        /// <param name="inWorkspaceShortName">the workspace id</param>
        /// <returns>a collection of permissions DTOs for the given workspace, only the workspace subset of Role</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetWorkspacePermissions(int inWorkspaceId)
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var wsRoles = from wr in gbe.WorkspaceUserRoles
                                  where inWorkspaceId == wr.WorkspaceID
                                  select new PermissionsDTO
                                  {
                                      PermissionId = wr.WorkspaceUserRoleID,
                                      ETIUserId = wr.ETIUserID,
                                      Role = (Role)wr.RoleID,
                                      WorkspaceId = inWorkspaceId,
                                      UpdateDate = wr.UpdateDT,
                                      HideWorkspaceHelp = wr.HideHelp
                                  };

                    toReturn = new Collection<PermissionsDTO>(wsRoles.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Return the workspace security permissions (WorkspaceAdmin, WorkspaceUser, WorkspaceReviewer, SubcontractAdministrator) for a given workspace
        /// </summary>
        /// <param name="inBoeID">boe id to retrieve workspace permissions for</param>
        /// <returns>the workspace permissions</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetWorkspacePermissionsByBoeID(int inBoeId)
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var wsRoles = from wr in gbe.WorkspaceUserRoles
                                  join b in gbe.BOEs on wr.WorkspaceID equals b.WorkspaceID
                                  where b.BOEID == inBoeId
                                  select new PermissionsDTO
                                  {
                                      PermissionId = wr.WorkspaceUserRoleID,
                                      ETIUserId = wr.ETIUserID,
                                      Role = (Role)wr.RoleID,
                                      WorkspaceId = b.WorkspaceID,
                                      UpdateDate = wr.UpdateDT,
                                      HideWorkspaceHelp = wr.HideHelp
                                  };

                    toReturn = new Collection<PermissionsDTO>(wsRoles.ToArray());
                }
            }

            return toReturn;

        }

        /// <summary>
        /// Get the admin permissions (MetricsAdmin, SystemAdmin) for users across workspaces and BOEs
        /// </summary>
        /// <returns>Admin permissions for system, only the admin subset of Role</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetAdminPermissions()
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var systemRoles = from sr in gbe.SystemUserRoles
                                      select new PermissionsDTO
                                      {
                                          PermissionId = sr.SystemUserRoleID,
                                          ETIUserId = sr.ETIUserID,
                                          Role = (Role)sr.RoleID,
                                          WorkspaceId = null,
                                          UpdateDate = sr.UpdateDT
                                      };

                    toReturn = new Collection<PermissionsDTO>(systemRoles.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the create workspace permissions
        /// </summary>
        /// <returns>Create workspace permissions for system, only the create workspace subset of Role</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetCreateWorkspacePermissions()
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var systemRoles = from sr in gbe.SystemUserRoles
                                      select new PermissionsDTO
                                      {
                                          PermissionId = sr.SystemUserRoleID,
                                          ETIUserId = sr.ETIUserID,
                                          Role = (Role)sr.RoleID,
                                          WorkspaceId = null,
                                          UpdateDate = sr.UpdateDT
                                      };

                    toReturn = new Collection<PermissionsDTO>(systemRoles.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get Workspace Creator Roles for PTM
        /// System Admins and users with Create Workspace permissions
        /// </summary>
        /// <param name="selectedNtid">NTID of the selected user</param>
        /// <param name="selectedUser">Display Name of the selected user</param>
        /// <returns>Key Value Pairs of NTID and Display Name of users with Workspace Creator Roles, distinct and alphabetized</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ICollection<KeyValuePair<string, string>> GetCreateWorkspaceRolesForPtm(string selectedNtid, string selectedUser)
        {
            ICollection<KeyValuePair<string, string>> toReturn = new Collection<KeyValuePair<string, string>>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Get user data for users with System Admin or Create Workspace Permissions
                    ICollection<UserData> users = (from sur in gbe.SystemUserRoles
                                                    join eu in gbe.ETIusers on sur.ETIUserID equals eu.ETIUserID
                                                    where sur.RoleID == (int)Role.SystemAdmin
                                                        || sur.RoleID == (int)Role.CreateWorkspacePermissions
                                                    select new UserData
                                                    {
                                                        Ntid = eu.NTID,
                                                        DisplayName = eu.DisplayName
                                                    }).Distinct().ToCollection();

                    foreach(UserData user in users)
                    {
                        if(this.adUtils.IsGroup(user.Ntid))
                        {
                            // If it's a group, get the group members and add them individually if not already in the list
                            ICollection<UserData> groupMembers = this.adUtils.GetAdGroupUsers(user.DisplayName);
                            foreach(UserData groupMember in groupMembers)
                            {
                                if (!toReturn.Any(x => x.Key == groupMember.Ntid))
                                {
                                    toReturn.Add(new KeyValuePair<string, string>(groupMember.Ntid, groupMember.DisplayName));
                                }
                            }
                        }
                        else
                        {
                            // Otherwise it's a user and add them if not already added
                            if(!toReturn.Any(x => x.Key == user.Ntid))
                            {
                                toReturn.Add(new KeyValuePair<string, string>( user.Ntid, user.DisplayName ));
                            }
                        }
                    }
                }
            }

            // If the previously selected user no longer has System Admin/Create Workspace permissions
            // add them to the list because the selection is still allowed until they are unselected
            if(!string.IsNullOrEmpty(selectedNtid) && !toReturn.Any(x => x.Key == selectedNtid))
            {
                toReturn.Add(new KeyValuePair<string, string>(selectedNtid, selectedUser));
            }

            // Alphabetize the list by display name before returning
            return toReturn.OrderBy(x => x.Value).ToCollection();
        }

        /// <summary>
        /// Gets the users permissions
        /// </summary>
        /// <returns>Get all of a user's system permissions</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetUserPermissions(UserDTO User)
        {
            if (User == null)
            {
                throw new ArgumentNullException(nameof(User));
            }

            Collection<PermissionsDTO> toReturn = new Collection<PermissionsDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    Collection<int?> systemResultIds = gbe.getSystemPermissions(User.NTID).ToCollection();
                    foreach (int? systemResultId in systemResultIds)
                    {
                        toReturn.Add(new PermissionsDTO
                        {
                            ETIUserId = User.UserID,
                            Role = (Role)systemResultId.Value
                        });
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the BOE permissions (Author, Approver) for users in the workspace and on the BOE of interest
        /// NOTE: This is the active users assigned to the role, not the 'selection list' of potential users.
        /// </summary>
        /// <param name="inBOEId">The boe ID to get permissions for</param>
        /// <returns>BOE permissions for the boe of interest, only the BOE subset of SecurityRole</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetBOEPermissions(ICollection<int> inBOEIds)
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var boeUsers = from role in gbe.BOEUserRoles
                                   join boe in gbe.BOEs on role.BOEID equals boe.BOEID
                                   where inBOEIds.Contains(role.BOEID)
                                   select new PermissionsDTO
                                   {
                                       PermissionId = role.BOEUserRoleID,
                                       ETIUserId = role.ETIUserID,
                                       Role = (Role)role.RoleID,
                                       WorkspaceId = boe.WorkspaceID,
                                       BOEId = role.BOEID,
                                       UpdateDate = role.UpdateDT
                                   };

                    toReturn = new Collection<PermissionsDTO>(boeUsers.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the BOE permissions (Author, Approver) for users in the workspace
        /// NOTE: This is the POTENTIAL users assigned to the role (i.e. the 'pick list' of potential users)
        /// </summary>
        /// <param name="inWorkspaceId">The workspace ID to get permissions for</param>
        /// <returns>BOE permissions for the workspace of interest, only the BOE subset of SecurityRole</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetBOEPotentialPermissionsForWorkspace(int inWorkspaceId)
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var boeUsers = from role in gbe.BOEPotentialRoles
                                   where role.WorkspaceID == inWorkspaceId
                                   select new PermissionsDTO
                                   {
                                       PermissionId = role.BOEPotentialRoleID,
                                       ETIUserId = role.ETIUserID,
                                       Role = (Role)role.RoleID,
                                       WorkspaceId = role.WorkspaceID,
                                       UpdateDate = role.UpdateDT
                                   };

                    toReturn = new Collection<PermissionsDTO>(boeUsers.ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Workspace level roles (exclude System Roles)..
        /// </summary>
        private static List<Role> wsLevelRoles = 
            new List<Role>() { Role.Author, Role.WorkspaceReviewer, Role.Approver, Role.WorkspaceAdmin, Role.WorkspaceUser, Role.SubcontractorAuthor, Role.SubcontractAdmin };


        /// <summary>
        /// Retrieves permissions for the Permissions Grid
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>WS Permissions</returns>
        [DbQuery]
        virtual public Collection<PermissionsDTO> GetPermissionsForGridData(int wsId)
        {
            Collection<PermissionsDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.BOEPotentialRoles.Where(role => role.WorkspaceID == wsId)
                                        .Select(role => new PermissionsDTO
                                        {
                                            PermissionId = role.BOEPotentialRoleID,
                                            ETIUserId = role.ETIUserID,
                                            Role = (Role)role.RoleID,
                                            WorkspaceId = role.WorkspaceID,
                                            UpdateDate = role.UpdateDT
                                        }
                                ).Union(gbe.WorkspaceUserRoles.Where(role => role.WorkspaceID == wsId)
                                    .Select(role => new PermissionsDTO
                                    {
                                        PermissionId = role.WorkspaceUserRoleID,
                                        ETIUserId = role.ETIUserID,
                                        Role = (Role)role.RoleID,
                                        WorkspaceId = role.WorkspaceID,
                                        UpdateDate = role.UpdateDT
                                    })
                                ).Where(role => wsLevelRoles.Contains(role.Role)).ToCollection();
                }
            }

            return toReturn;
        }

        virtual public int? Delete(PermissionsDTO inPermission)
        {
            int? toReturn;
            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }

            // The save changes based on the role
            switch (inPermission.Role)
            {
                // Save to the Workspace Permissions table
                case Role.Approver:
                case Role.Author:
                case Role.SubcontractorAuthor:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOEPotentialRolebyETIUserID(
                            inPermission.ETIUserId,
                            inPermission.WorkspaceId,
                            (int)inPermission.Role,
                            inPermission.UpdateDate);
                    }
                    toReturn = inPermission.Id;
                    break;
                // Save to the Workpsace User Table
                case Role.WorkspaceReviewer:
                case Role.WorkspaceAdmin:
                case Role.WorkspaceUser:
                case Role.SubcontractAdmin:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteWorkspaceRolesByETIUserID(
                                inPermission.ETIUserId,
                                inPermission.WorkspaceId,
                                (int)inPermission.Role,
                                inPermission.UpdateDate);
                    }
                    toReturn = inPermission.Id;
                    break;
                // Save to the System User Table.
                case Role.MetricsAdmin:
                case Role.SystemAdmin:
                case Role.CreateWorkspacePermissions:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteSystemRolesByETIUserID(
                            inPermission.ETIUserId,
                            (int)inPermission.Role,
                            inPermission.UpdateDate);
                    }
                    toReturn = inPermission.Id;
                    break;
                default:
                    throw new GeneralAppException("Invalid Role cannot be Saved");
            }

            return toReturn;
        }

        virtual public int? Upsert(PermissionsDTO inPermission)
        {
            int? toReturn;
            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }

            // The save changes based on the role
            switch (inPermission.Role)
                    {
                // Save to the Workspace Permissions table
                case Role.Approver:
                case Role.Author:
                case Role.SubcontractorAuthor:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                        {
                            gbe.upsertBOEPotentialRoleByDTO(
                                inPermission.PermissionId,
                                inPermission.ETIUserId,
                                inPermission.WorkspaceId,
                                (int)inPermission.Role,
                            null,
                                inPermission.UpdateDate);
                        }


                        if (inPermission.Role != Role.SubcontractorAuthor)//Do not grant a subcontractor the WorkspaceUser role per User Story 16201
                        {
                            // Grant the user WorkspaceUser if they don't already have it
                            var currentWorkspaceUser = from p in GetWorkspacePermissions(inPermission.WorkspaceId.Value)
                                                       where p.Role == Role.WorkspaceUser &&
                                                    p.ETIUserId == inPermission.ETIUserId
                                                       select p;

                            if (currentWorkspaceUser.FirstOrDefault() == null)
                            {
                                PermissionsDTO workspaceUser = new PermissionsDTO();
                                workspaceUser.PermissionId = -1;
                                workspaceUser.WorkspaceId = inPermission.WorkspaceId;
                                workspaceUser.Role = Role.WorkspaceUser;
                                workspaceUser.ETIUserId = inPermission.ETIUserId;
                                workspaceUser.Updateable = UpdateType.Upsert;
                                workspaceUser.UpdateDate = DateTime.Now;

                                SavePermission(workspaceUser);
                            }
                    }
                    toReturn = inPermission.Id;
                    break;
                // Save to the Workpsace User Table
                case Role.WorkspaceReviewer:
                case Role.WorkspaceAdmin:
                case Role.WorkspaceUser:
                case Role.SubcontractAdmin:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                        {
                            gbe.insertWorkspaceUserRole(
                                inPermission.ETIUserId,
                                (int)inPermission.Role,
                                inPermission.WorkspaceId);
                        }
                    toReturn = inPermission.Id;
                    break;
                // Save to the System User Table.
                case Role.MetricsAdmin:
                case Role.SystemAdmin:
                case Role.CreateWorkspacePermissions:
                    using (GenBoeEntities gbe = new GenBoeEntities())
                        {
                            gbe.insertSystemUserRole(
                                inPermission.ETIUserId,
                                (int)inPermission.Role,
                                inPermission.UpdateDate);
                        }
                    toReturn = inPermission.Id;
                    break;
                default:
                    throw new GeneralAppException("Invalid Role cannot be Saved");
            }

            return toReturn;
        }

        /// <summary>
        /// Saves the specified Dto into the database, either deleting or upserting it.
        /// </summary>
        /// <param name="dtoToSave">Dto to save.</param>
        /// <returns>Wbs Id for the saved item, or null if save failed.</returns>
        virtual public void SavePermission(PermissionsDTO dtoToSave)
        {
            if (dtoToSave == null)
            {
                throw new ArgumentNullException(nameof(dtoToSave));
            }


            int? result = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (dtoToSave.Updateable == UpdateType.Deleted)
                {
                    result = this.Delete(dtoToSave);
                }
                else if (dtoToSave.Updateable == UpdateType.Upsert)
                {
                    result = this.Upsert(dtoToSave);
                }
                else
                {
                    throw new ArgumentException("The dto did not specify the Updateable type.");
                }
            }
        }

        /// <summary>
        /// Save method to process collection of dtos to save
        /// </summary>
        /// <param name="dtosToSave">collection of Dtos</param>
        /// <returns>Dictionary of saved object ids</returns>
        virtual public void SavePermissions(Collection<PermissionsDTO> dtosToSave)
        {
            if (dtosToSave == null)
            {
                throw new ArgumentNullException(nameof(dtosToSave));
            }

            foreach (PermissionsDTO dto in dtosToSave)
            {
                this.SavePermission(dto);
            }
        }

        /// <summary>
        /// Save the workspace admin's preference on hiding the workspace help
        /// </summary>
        /// <param name="inPermission"></param>
        virtual public void SaveWorkspaceHideHelp(PermissionsDTO inPermission)
        {
            if (inPermission == null)
            {
                throw new ArgumentNullException(nameof(inPermission));
            }
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
               gbe.updateWorkspaceUserRoleHideHelp(inPermission.PermissionId, inPermission.WorkspaceId, inPermission.HideWorkspaceHelp, inPermission.UpdateDate);
            }
        }

        /// <summary>
        /// Retrieves all of the NTIds that have that Workspace Role in the database.
        /// </summary>
        /// <param name="role">The role to filter on.</param>
        /// <returns>Distinct list of ntids.</returns>
        [DbQuery]
        public ICollection<string> GetAllNtIdsForWorkspacePermission(Role role)
        {
            if (role == Role.None)
            {
                throw new ArgumentException("The Role passed in must not be None");
            }

            int roleId = (int)role;
            ICollection<string> ntIds = new List<string>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                ntIds = gbe.WorkspaceUserRoles.Where(r => r.RoleID == roleId)
                                        .Select(r => r.ETIuser.NTID).Distinct().ToList();
            }

            return ntIds;
        }
    }

}
