// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.Models;
    using IES.Common;

    /// <summary>
    /// DataLoader responsible for querying database to determine what roles the
    /// user has setup for given workspaces and boes.
    /// </summary>
    public class SecurityUserAuthorizationsDataLoader : ISecurityUserAuthorizationsDataLoader
    {
        private Logger _log = new Logger(typeof(SecurityUserAuthorizationsDataLoader));

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private IActiveDirectoryUtilities activeDirectoryUtilities = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SecurityUserAuthorizationsDataLoader(IActiveDirectoryUtilities inActiveDirectoryUtilities)
        {
            this.activeDirectoryUtilities = inActiveDirectoryUtilities;
        }

        /// <summary>
        /// Get the collection of user permissions for the currently logged in user
        /// </summary>
        /// <param name="inUserNTID">The NTID of the user whose permissions we will load</param>
        /// <returns>Collection of permissions for each user permission in the database</returns>
        [DbQuery(2)]
        public Collection<SecurityPermissionsResponse> GetPermissionsForUser(string inUserNTID)
        {
            // Initialize result list with the "none" role, every user gets this.
            List<SecurityPermissionsResponse> permissionsToReturn = new List<SecurityPermissionsResponse> { new SecurityPermissionsResponse(Role.None, null, null) };

            var groups = this.activeDirectoryUtilities.GetGroupsForUser(inUserNTID);
            using (StopwatchTimer sw = new StopwatchTimer("GetPermissionsForUser for user " + inUserNTID, this._log))
            {
                var groupNames = groups != null ? groups.Select(x => x.Ntid) : new List<string>();

                using (StopwatchTimer sw2 = new StopwatchTimer("DB call in GetPermissionsForUser for user " + inUserNTID, this._log))
                {
                    ICollection<SecurityPermissionsResponseMutable> resultsLinq = new Collection<SecurityPermissionsResponseMutable>();
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        // use a List here instead of IQueryable so that the Ids are passed into the LINQ Unions below instead of adding the same LINQ SQL statement over and over
                        List<int> users = (from x in gbe.ETIusers
                                           where (x.NTID == inUserNTID ||
                                               groupNames.Contains(x.NTID))
                                           select x.ETIUserID).ToList();

                        //get workspace permissions
                        // Note - Only parameterless constructors and initializers are supported in LINQ to Entities.
                        // Therefore, we gather permissions in two steps as follows:
                        // Step 1 - Select permission data into a temporary collection of mutable objects (with a parameterless constructor).
                        resultsLinq = (from x in gbe.WorkspaceUserRoles
                                           from y in users
                                           where x.ETIUserID == y
                                           select new SecurityPermissionsResponseMutable()
                                           {
                                               AuthorizedRole = (Role)x.RoleID,
                                               WorkspaceId = x.WorkspaceID,
                                               BOEId = null
                                           })
                                        .Union(
                                            from x in gbe.SystemUserRoles
                                            from y in users
                                            where x.ETIUserID == y
                                            select new SecurityPermissionsResponseMutable()
                                            {
                                                AuthorizedRole = (Role)x.RoleID,
                                                WorkspaceId = null,
                                                BOEId = null
                                            }
                                        ).Union(
                                            from br in gbe.BOEUserRoles
                                            from y in users
                                            where br.ETIUserID == y
                                            select new SecurityPermissionsResponseMutable()
                                            {
                                                AuthorizedRole = (Role)br.RoleID,
                                                WorkspaceId = br.BOE.WorkspaceID,
                                                BOEId = br.BOEID
                                            }

                                        ).ToCollection();
                    }

                    // Step 2 - Convert LINQ results into immutable objects for return.
                    permissionsToReturn.AddRange(resultsLinq.Select(x => new SecurityPermissionsResponse(x.AuthorizedRole, x.WorkspaceId, x.BOEId)).ToArray());
                }
            }

            return permissionsToReturn.ToCollection();
        }

        /// <summary>
        /// Private mutable class used internally to create immutable SecurityPermissionResponse objects.
        /// </summary>
        private class SecurityPermissionsResponseMutable
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public SecurityPermissionsResponseMutable()
            {
                AuthorizedRole = Role.None;
            }

            /// <summary>
            /// The roles the user is authorized for
            /// </summary>
            public Role AuthorizedRole { get; set; }

            /// <summary>
            /// Optional workspaceId the user is authorized for
            /// </summary>
            public int? WorkspaceId { get; set; }

            /// <summary>
            /// Optional BOEId the user is authorized for
            /// </summary>
            public int? BOEId { get; set; }
        }
    }
}
