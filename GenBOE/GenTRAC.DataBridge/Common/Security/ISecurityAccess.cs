// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using IES.Common;

    /// <summary>
    /// security access interface
    /// </summary>
    public interface ISecurityAccess
    {
        /// <summary>
        /// Test whether the current user has the designated role
        /// </summary>
        /// <param name="role">The role to test</param>
        /// <param name="proposalId">Optional proposal id, if applicable for authorization</param>
        /// <returns>Yes or no</returns>
        bool CurrentUserHasRole(PtmRole role, int? proposalId);

        /// <summary>
        /// Return true/false depending on users authorizations for the roles requested
        /// </summary>
        /// <param name="inPermissions">The roles to determine and see if user has any of these roles in the system</param>
        /// <param name="highestRole">Highest role corresponding to the highest authorization level</param>
        /// <returns>The access level the user has to this page (CRUD)</returns>
        SecurityAuthorization IsAuthorized(SecurityPermissionsRequested inPermissions, out PtmRole highestRole);
    }
}
