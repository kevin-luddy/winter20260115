// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;

    /// <summary>
    /// Proposal Permission Mapper
    /// </summary>
    public interface IProposalPermissionMapper
    {
        /// <summary>
        /// Delete IDs by proposal id
        /// </summary>
        /// <param name="inProposalID">proposal id</param>
        void DeleteIdsByProposalId(int inProposalID);

        /// <summary>
        /// Delete permission DTO objects from database
        /// </summary>
        /// <param name="inProposalPermissions">Proposal Permission DTOs to delete</param>
        void Delete(ICollection<ProposalPermissionDto> inProposalPermissions);

        /// <summary>
        /// Get items by Proposal Id
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        /// <returns>Dto for the corresponding Id.</returns>
        GenTRAC.DataBridge.DTO.ProposalPermissionDto GetById(int itemId);

        /// <summary>
        /// Returns a list of proposal permissions for a single user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of proposal permissions</returns>
        System.Collections.Generic.ICollection<GenTRAC.DataBridge.DTO.ProposalPermissionDto> GetByUserId(int userId);

        /// <summary>
        /// Get proposal permissions by proposal id
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>propsal permissions</returns>
        System.Collections.Generic.ICollection<GenTRAC.DataBridge.DTO.ProposalPermissionDto> GetProposalPermissionsByProposalId(int proposalId);

        /// <summary>
        /// Get propsal permissions by user id and prosal id
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <param name="userId">user id</param>
        /// <returns>proposal permissions</returns>
        System.Collections.Generic.ICollection<GenTRAC.DataBridge.DTO.ProposalPermissionDto> GetProposalPermissionsByUserId(int proposalId, int userId);
    }

    /// <summary>
    /// Interface for Internal Proposal Permissions Data Mapper
    /// </summary>
    internal interface IInternalProposalPermissionMapper : IProposalPermissionMapper
    {
        /// <summary>
        /// Save Permissions
        /// </summary>
        /// <param name="inPermissionsDto">Permissions to save</param>
        /// <returns>Saved Permissions Dto</returns>
        int? Save(ProposalPermissionDto inPermissionsDto);
    }
}
