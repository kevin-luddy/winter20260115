// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// DataLoader responsible for querying database to determine what roles the
    /// user has setup for given workspaces and boes.
    /// </summary>
    public class SecurityUserAuthorizationsDataLoader : ISecurityUserAuthorizationsDataLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        private IES.Common.Logger log = new IES.Common.Logger(typeof(SecurityUserAuthorizationsDataLoader));

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private IES.Common.IActiveDirectoryUtilities activeDirectoryUtilities = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="inActiveDirectoryUtilities">Active Directory Utilities</param>
        public SecurityUserAuthorizationsDataLoader(IES.Common.IActiveDirectoryUtilities inActiveDirectoryUtilities)
        {
            this.activeDirectoryUtilities = inActiveDirectoryUtilities;
        }

        /// <summary>
        /// Get the collection of user permissions for the currently logged in user
        /// </summary>
        /// <param name="inUserDTO">The user we're trying to load</param>
        /// <returns>Collection of permissions for each user permission in the database</returns>
        public IReadOnlyCollection<SecurityPermissionsResponse> GetPermissionsForUser(UserDTO inUserDTO)
        {
            // Initialize result list with the "NotSet" role, every user gets this.
            List<SecurityPermissionsResponse> permissionsToReturn = new List<SecurityPermissionsResponse> { new SecurityPermissionsResponse(PtmRole.NotSet, null) };
            if (inUserDTO != null)
            {
                this.log.Info("BEGIN GetPermissionsForUser for user " + inUserDTO.Ntid);
                var groups = this.activeDirectoryUtilities.GetGroupsForUser(inUserDTO.Ntid);
                List<string> groupNames = groups != null ? groups.Select(x => x.Ntid).ToList() : new List<string>();
                groupNames.Add(inUserDTO.Ntid);

                ICollection<SecurityPermissionsResponseMutable> resultsLinq = new Collection<SecurityPermissionsResponseMutable>();
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // use a List here instead of IQueryable so that the Ids are passed into the LINQ Unions below instead of adding the same LINQ SQL statement over and over
                    List<int> users = (from x in dbModel.genTRACUsers
                                            where (x.UserID == inUserDTO.Id ||
                                                groupNames.Contains(x.NTID))
                                            select x.UserID).ToList();

                    IQueryable<SystemUserRole> systemUserRoles = from x in dbModel.SystemUserRoles
                                                                 from y in users
                                                                 where x.UserID == y
                                                                 select x;

                    int proposalSetupAdminId = (int)PtmRole.ProposalSetupAdmin;
                    int viewerId = (int)PtmRole.Viewer;

                    // add Permissions for LOB, Proposal Users, System Users
                    // Note - Only parameterless constructors and initializers are supported in LINQ to Entities.
                    // Therefore, we gather permissions in two steps as follows:
                    // Step 1 - Select permission data into a temporary collection of mutable objects (with a parameterless constructor).
                    resultsLinq = (from x in dbModel.ProposalUserRoles
                                   from y in users
                                   where x.UserID == y
                                   select new SecurityPermissionsResponseMutable()
                                   {
                                       AuthorizedRole = (PtmRole)x.RoleID,
                                       ProposalID = x.ProposalID
                                   }).Union(
                                  (from x in systemUserRoles
                                   select new SecurityPermissionsResponseMutable()
                                   {
                                       AuthorizedRole = (PtmRole)x.RoleID,
                                       ProposalID = null
                                   })
                                  .Union(
                                 from x in systemUserRoles
                                 from y in dbModel.Proposals
                                 where
                               (x.RoleID == viewerId || x.RoleID == proposalSetupAdminId) &&
                               x.LineOfBusinessLUs.Contains(y.LineOfBusinessLU)
                                 select new SecurityPermissionsResponseMutable()
                                 {
                                     AuthorizedRole = (PtmRole)x.RoleID,
                                     ProposalID = y.ProposalID
                                 })).ToCollection();
                }

                // Step 2 - Convert LINQ results into immutable objects for return.
                permissionsToReturn.AddRange(resultsLinq.Select(x => new SecurityPermissionsResponse(x.AuthorizedRole, x.ProposalID)).ToArray());

                this.log.Info("END GetPermissionsForUser for user " + inUserDTO.Ntid);
            }
            
            return permissionsToReturn;
        }

        /// <summary>
        /// Private mutable class used internally to create immutable SecurityPermissionResponse objects.
        /// </summary>
        private class SecurityPermissionsResponseMutable
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public SecurityPermissionsResponseMutable()
            {
                this.AuthorizedRole = PtmRole.NotSet;
            }

            /// <summary>
            /// The roles the user is authorized for
            /// </summary>
            public PtmRole AuthorizedRole { get; set; }

            /// <summary>
            /// Optional proposalID the user is authorized for
            /// </summary>
            public int? ProposalID { get; set; }
        }
    }
}
