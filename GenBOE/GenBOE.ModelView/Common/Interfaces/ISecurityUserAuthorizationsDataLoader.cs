// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common.Interfaces
{
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.Common;

    /// <summary>
    /// DataLoader responsible for querying database to determine what roles the
    /// user has setup for given workspaces and boes.
    /// </summary>
    public interface ISecurityUserAuthorizationsDataLoader
    {
        /// <summary>
        /// Get the collection of user permissions for the specified user.
        /// </summary>
        /// <param name="inUserNTID">The NTID of the user whose permissions we will load</param>
        /// <returns>Collection of permissions for each user permission in the database</returns>
        Collection<SecurityPermissionsResponse> GetPermissionsForUser(string inUserNTID);
    }
}
