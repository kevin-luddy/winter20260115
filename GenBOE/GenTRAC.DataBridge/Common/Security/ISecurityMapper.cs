// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    /// <summary>
    /// security mapper interface
    /// </summary>
    public interface ISecurityMapper
    {
        /// <summary>
        /// Gets all roles for the logged in user
        /// </summary>
        /// <returns>Security response collection</returns>
        System.Collections.Generic.IReadOnlyCollection<SecurityPermissionsResponse> GetRolesForLoggedInUser();

        /// <summary>
        /// Gets all roles for a given user
        /// </summary>
        /// <param name="inUserNtid">The Ntid</param>
        /// <returns>Security response collection</returns>
        System.Collections.Generic.IReadOnlyCollection<SecurityPermissionsResponse> GetRolesForUser(string inUserNtid);
    }
}
