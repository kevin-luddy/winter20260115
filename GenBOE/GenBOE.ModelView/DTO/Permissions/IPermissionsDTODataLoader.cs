// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IPermissionsDTODataLoader
    {
        /// <summary>
        /// Get the admin permissions (MetricsAdmin, SystemAdmin) for users across workspaces and BOEs
        /// </summary>
        /// <returns>Admin permissions for system, only the admin subset of Role</returns>
        Collection<PermissionsDTO> GetAdminPermissions();

        /// <summary>
        /// Get the create workspace permissions
        /// </summary>
        /// <returns>Create Workspace permissions for system, only the Create Workspace subset of Role</returns>
        Collection<PermissionsDTO> GetCreateWorkspacePermissions();

        /// <summary>
        /// Get the BOE permissions (Author, Reviewer, Approver) for users in the workspace and on the BOE of interest
        /// NOTE: This is the active users assigned to the role, not the 'selection list' of potential users.
        /// </summary>
        /// <param name="inBOEId">The boe ID to get permissions for</param>
        /// <returns>BOE permissions for the boe of interest, only the BOE subset of SecurityRole</returns>
        Collection<PermissionsDTO> GetBOEPermissions(ICollection<int> inBOEIds);
        
        /// <summary>
        /// Get the users permissions
        /// </summary>
        /// <param name="User">User's DTO record</param>
        /// <returns>Get all of a user's system permissions</returns>
        Collection<PermissionsDTO> GetUserPermissions(UserDTO User);

        /// <summary>
        /// Return the workspace security permissions (WorkspaceAdmin, WorkspaceUser) for a given workspace
        /// </summary>
        /// <param name="inWorkspaceShortName">the workspace id</param>
        /// <returns>a collection of permissions DTOs for the given workspace, only the workspace subset of Role</returns>
        Collection<PermissionsDTO> GetWorkspacePermissions(int inWorkspaceId);

        /// <summary>
        /// Return the workspace security permissions (WorkspaceAdmin, WorkspaceUser) for a given workspace
        /// </summary>
        /// <param name="inBoeId">the boe id</param>
        /// <returns>a collection of permissions DTOs for the given workspace, only the workspace subset of Role</returns>
        Collection<PermissionsDTO> GetWorkspacePermissionsByBoeID(int inBoeId);

        ///// <summary>
        ///// Saves a single potential permission
        ///// </summary>
        ///// <param name="inPermission">The permission DTO to save</param>
        void SavePermission(PermissionsDTO dtoToSave);

        ///// <summary>
        ///// Saves a Collection of potential permissions
        ///// </summary>
        ///// <param name="inPermission">The permission DTO to save</param>
        void SavePermissions(Collection<PermissionsDTO> dtosToSave);

        /// <summary>
        /// Get the BOE permissions (Author, Reviewer, Approver) for users in the workspace
        /// NOTE: This is the POTENTIAL users assigned to the role (i.e. the 'pick list' of potential users)
        /// </summary>
        /// <param name="inWorkspaceId">The workspace ID to get permissions for</param>
        /// <returns>BOE permissions for the workspace of interest, only the BOE subset of SecurityRole</returns>
        Collection<PermissionsDTO> GetBOEPotentialPermissionsForWorkspace(int inWorkspaceId);
        
        Collection<PermissionsDTO> GetAllBOEPermissions();
        Collection<PermissionsDTO> GetAllBOEPotentialPermissions();

        void SaveWorkspaceHideHelp(PermissionsDTO inPermission);

        /// <summary>
        /// Returns Workspace Ids for the Users
        /// </summary>
        /// <param name="userIds">User Ids</param>
        /// <returns>All WS Permissions for that user</returns>
        Collection<int> GetWorkspaceIdsThatUsersHaveAccessTo(ICollection<int> userIds);

        /// <summary>
        /// Returns Workspace Ids for the Users where the Users are workspace admins
        /// </summary>
        /// <param name="userIds">User Ids</param>
        /// <returns>Workspace Ids</returns>
        Collection<int> GetWorkspaceIdsWhereUserIsWorkspaceAdmin(ICollection<int> userIds);

        /// <summary>
        /// Get Workspace Creator Roles for PTM
        /// System Admins and users with Create Workspace permissions
        /// </summary>
        /// <param name="selectedNtid">NTID of the selected user</param>
        /// <param name="selectedUser">Display Name of the selected user</param>
        /// <returns>Key Value Pairs of NTID and Display Name of users with Workspace Creator Roles, distinct and alphabetized</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<KeyValuePair<string, string>> GetCreateWorkspaceRolesForPtm(string selectedNtid, string selectedUser);

        /// <summary>
        /// Retrieves permissions for the Permissions Grid
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>WS Permissions</returns>
        Collection<PermissionsDTO> GetPermissionsForGridData(int wsId);

        /// <summary>
        /// Retrieves all of the NTIds that have that Workspace Role in the database.
        /// </summary>
        /// <param name="role">The role to filter on.</param>
        /// <returns>Distinct list of ntids.</returns>
        ICollection<string> GetAllNtIdsForWorkspacePermission(Role role);
    }
}
