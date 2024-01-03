// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using IES.Core;

    /// <summary>
    /// system permission loader interface
    /// </summary>
    public interface ISystemPermissionLoader : IDataLoader<SystemPermissionDto>
    {
        /// <summary>
        /// Gets all system permission IDs
        /// </summary>
        /// <returns>Collection of IDs</returns>
        System.Collections.Generic.ICollection<int> GetAllIds();

        /// <summary>
        /// Get all permissions
        /// </summary>
        /// <returns>All Permissions</returns>
        System.Collections.Generic.ICollection<SystemPermissionDto> GetAllSystemPermissions();

        /// <summary>
        /// Returns all system permission ids for the given user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>list of all system permission ids for that user</returns>
        ICollection<int> GetIdsByUserId(int userId);
    }
}
