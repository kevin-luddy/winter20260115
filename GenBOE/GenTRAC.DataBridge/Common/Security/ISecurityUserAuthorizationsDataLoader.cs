// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Security data loader interface
    /// </summary>
    public interface ISecurityUserAuthorizationsDataLoader
    {
        /// <summary>
        /// Get the collection of user permissions for the currently logged in user
        /// </summary>
        /// <param name="inUserDTO">The user we're trying to load</param>
        /// <returns>Collection of permissions for each user permission in the database</returns>
        IReadOnlyCollection<SecurityPermissionsResponse> GetPermissionsForUser(UserDTO inUserDTO);
    }
}
