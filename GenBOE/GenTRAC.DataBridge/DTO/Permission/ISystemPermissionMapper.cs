// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    /// <summary>
    /// system permissions mapper interface 
    /// </summary>
    public interface ISystemPermissionMapper
    {
        /// <summary>
        /// Get items by Proposal Id
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        /// <returns>Dto for the corresponding Id.</returns>
        SystemPermissionDto GetById(int itemId);

        /// <summary>
        /// Returns a list of permissions for a single user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of permissions</returns>
        System.Collections.Generic.ICollection<SystemPermissionDto> GetByUserId(int userId);

        /// <summary>
        /// Returns a list of permissions DTOs on the system level
        /// </summary>
        /// <returns>Permission DTO collection</returns>
        System.Collections.Generic.ICollection<SystemPermissionDto> GetSystemPermissions();
    }

    /// <summary>
    /// Interface for Internal System Permissions Data Mapper
    /// </summary>
    internal interface IInternalSystemPermissionMapper : ISystemPermissionMapper
    {
        /// <summary>
        /// Save Permissions
        /// </summary>
        /// <param name="inPermissionsDto">Permissions to save</param>
        /// <returns>Saved Permissions Dto</returns>
        int? Save(SystemPermissionDto inPermissionsDto);
    }
}
