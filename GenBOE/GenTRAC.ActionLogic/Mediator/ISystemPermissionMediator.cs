// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// The permissions mediator interface
    /// </summary>
    public interface ISystemPermissionMediator
    {
        /// <summary>
        /// Save permissions
        /// </summary>
        /// <param name="inPermissionDto">permission to save</param>
        /// <returns>saved permission</returns>
        SystemPermissionDto SavePermissionDto(SystemPermissionDto inPermissionDto);

        /// <summary>
        /// Saves a collection of permissions
        /// </summary>
        /// <param name="inPermissionDtos">Permissions to save</param>
        /// <returns>The saved permissions</returns>
        System.Collections.Generic.ICollection<SystemPermissionDto> SavePermissionDtos(System.Collections.Generic.ICollection<SystemPermissionDto> inPermissionDtos);
    }
}
