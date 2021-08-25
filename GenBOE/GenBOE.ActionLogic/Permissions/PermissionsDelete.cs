// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Permissions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public static class PermissionsDelete
    {
        /// <summary>
        /// Matrix of workspace state/boe state to action combinations.
        /// Commented combinations should never occur in genBOE
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private static int[,] ActionMatrix = {
            {(int)WorkspaceState.Initialization,    (int)BOEState.Unassigned,       (int)PermissionDeleteAction.DeleteRoleRequireReassignment},
            {(int)WorkspaceState.Initialization,    (int)BOEState.Draft,            (int)PermissionDeleteAction.DeleteRoleRequireReassignment},
            {(int)WorkspaceState.Working,           (int)BOEState.Unassigned,       (int)PermissionDeleteAction.DeleteRoleRequireReassignment},
            {(int)WorkspaceState.Working,           (int)BOEState.Draft,            (int)PermissionDeleteAction.RequireReassignment},
            {(int)WorkspaceState.Working,           (int)BOEState.AwaitingApproval, (int)PermissionDeleteAction.RequireReassignment},
            {(int)WorkspaceState.Working,           (int)BOEState.Approved,         (int)PermissionDeleteAction.DeleteRole},
            {(int)WorkspaceState.Locked,            (int)BOEState.Unassigned,       (int)PermissionDeleteAction.DeleteRoleRequireReassignment},
            {(int)WorkspaceState.Locked,            (int)BOEState.Draft,            (int)PermissionDeleteAction.RequireReassignment},
            {(int)WorkspaceState.Locked,            (int)BOEState.AwaitingApproval, (int)PermissionDeleteAction.RequireReassignment},
            {(int)WorkspaceState.Locked,            (int)BOEState.Approved,         (int)PermissionDeleteAction.DeleteRole},
            {(int)WorkspaceState.Complete,          (int)BOEState.Approved,         (int)PermissionDeleteAction.DeleteRole},
            {(int)WorkspaceState.Closed,            (int)BOEState.AwaitingApproval, (int)PermissionDeleteAction.DeleteRole},
            {(int)WorkspaceState.Closed,            (int)BOEState.Approved,         (int)PermissionDeleteAction.DeleteRole}
        };

        /// <summary>
        /// Takes a workspace state and boe state and returns the associated delete action
        /// </summary>
        /// <param name="securityWorkspaceState">The workspace state</param>
        /// <param name="securityBOEState">The BOE state</param>
        /// <returns>A PermissionDeleteAction enum value specifying the action to take</returns>
        private static PermissionDeleteAction GetDeleteAction(WorkspaceState securityWorkspaceState, BOEState securityBOEState)
        {
            // Search each row in the Action Matrix for the given Workspace and BOE state
            for (int ndx = 0; ndx < ActionMatrix.GetUpperBound(0); ndx++)
            {
                // When found, return this row's action value
                if (ActionMatrix[ndx, 0] == (int)securityWorkspaceState && ActionMatrix[ndx, 1] == (int)securityBOEState)
                {
                    return (PermissionDeleteAction)ActionMatrix[ndx, 2];
                }
            }

            // For invalid state combinations, return and Invalid identifier
            return PermissionDeleteAction.Invalid;
        }

        /// <summary>
        /// Check to see whether a given user is assigned any role on a BOE
        /// within the given workspace. If so, pass back the necessary action for the
        /// given workspace state and the state of any assigned BOEs.
        /// </summary>
        /// <returns></returns>
        public static PermissionDeleteAction CheckUserAssignments(FullWorkspace ws, int userID, IPermissionsDTODataLoader permissionsLoader)
        {
            return _CheckUserAssignments(ws, userID, null, permissionsLoader);
        }

        /// <summary>
        /// Check to see whether a given user is assigned a given role on a BOE
        /// within the given workspace. If so, pass back the necessary action for the
        /// given workspace state and the state of any assigned BOEs.
        /// </summary>
        /// <returns></returns>
        public static PermissionDeleteAction CheckUserAssignments(FullWorkspace ws, int userID, ICollection<Role> checkRoles, IPermissionsDTODataLoader permissionsLoader)
        {
            return _CheckUserAssignments(ws, userID, checkRoles, permissionsLoader);
        }

        /// <summary>
        /// Check to see whether a given user is assigned a given role on a BOE
        /// within the given workspace. If so, pass back the necessary action for the
        /// given workspace state and the state of any assigned BOEs.
        /// </summary>
        private static PermissionDeleteAction _CheckUserAssignments(FullWorkspace ws, int userID, ICollection<Role> checkRoles, IPermissionsDTODataLoader permissionsLoader)
        {
            // Default to invalid role
            PermissionDeleteAction toReturn = PermissionDeleteAction.Invalid;

            // Check loader for validity
            if (permissionsLoader != null)
            {
                // Set delete role once we know the loader are valid
                toReturn = PermissionDeleteAction.DeleteRole;

                ICollection<BoeDTO> boeDTOs = ws.Boes.ToList<BoeDTO>();

                // For each BOE ID given, get the set of permissions on that BOE.
                // If the permissions on any BOE contain the given User ID and Role, add that
                // BOE ID a set.

                ICollection<BoeDTO> assignedBOEs = new Collection<BoeDTO>();

                // Find all BOEs that the user is assigned to in the given role. Store this off
                // temporarily. We'll return it if certain criteria are met.
                var temp_assignedBOEs = (from boeDTO in boeDTOs
                                        from permissions in permissionsLoader.GetBOEPermissions(new List<int>(){boeDTO.Id})
                                        where permissions.ETIUserId == userID &&
                                        (checkRoles == null || checkRoles.Contains(permissions.Role))
                                        select boeDTO).ToList();

                // If the user is assigned to BOEs in the given role
                if (temp_assignedBOEs.Any())
                {
                    assignedBOEs = temp_assignedBOEs;
                }

                // If the set is not null and contains items, let's continue.
                if (assignedBOEs.Any())
                {
                    // Get the state of the given workspace
                    WorkspaceState workspaceState = ws.WorkspaceState;

                    // For each assigned BOE get the mapped delete action based on the workspace state
                    // and the state of the BOE
                    var actions = (from boeDTO in assignedBOEs
                                   select GetDeleteAction(workspaceState, boeDTO.State)).ToList();

                    // If the set is not null and contains items, let's continue.
                    if (actions.Any())
                    {
                        // Filter out duplicate actions, and return the maximum enum value
                        // This will be the action that takes precedence over any others
                        toReturn = actions.Distinct().Max();
                    }
                }
            }

            return toReturn;
        }
    }

    /// <summary>
    /// Represents an action to complete on user permission deletion
    /// </summary>
    public enum PermissionDeleteAction
    {
        DeleteRole,
        DeleteRoleRequireReassignment,
        RequireReassignment,
        Invalid
    }
}
