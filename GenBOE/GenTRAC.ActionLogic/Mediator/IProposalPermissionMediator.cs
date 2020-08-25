// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Proposal Permission Mediator
    /// </summary>
    public interface IProposalPermissionMediator
    {
        /// <summary>
        /// Delete permission IDs by proposal ID
        /// </summary>
        /// <param name="inProposalID">Proposal ID</param>
        void DeleteIdsByProposalId(int inProposalID);

        /// <summary>
        /// Delete permission DTO objects from database
        /// </summary>
        /// <param name="inProposalPermissions">Proposal Permission DTOs to delete</param>
        void Delete(ICollection<ProposalPermissionDto> inProposalPermissions);

        /// <summary>
        /// Saves a collection of permissions
        /// </summary>
        /// <param name="inProposalPermissionDto">Permissions to save</param>
        /// <returns>The saved permissions</returns>
        GenTRAC.DataBridge.DTO.ProposalPermissionDto SaveProposalPermissionDto(GenTRAC.DataBridge.DTO.ProposalPermissionDto inProposalPermissionDto);

        /// <summary>
        /// Save permissions
        /// </summary>
        /// <param name="inProposalPermissionDtos">permission to save</param>
        /// <returns>saved permission</returns>
        System.Collections.Generic.ICollection<GenTRAC.DataBridge.DTO.ProposalPermissionDto> SaveProposalPermissionDtos(System.Collections.Generic.ICollection<GenTRAC.DataBridge.DTO.ProposalPermissionDto> inProposalPermissionDtos);
    }
}
