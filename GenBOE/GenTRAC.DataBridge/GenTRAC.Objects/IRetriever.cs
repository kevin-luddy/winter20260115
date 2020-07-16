// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Objects
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Retrieves child full objects / dtos for the Full Objects.
    /// </summary>
    public interface IRetriever
    {
        /// <summary>
        /// Returns the permissions for the proposal
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>permissions</returns>
        ICollection<ProposalPermissionDto> GetProposalPermissions(int proposalId);

        /// <summary>
        /// Returns the current user
        /// </summary>
        /// <returns>Current User</returns>
        UserDTO GetCurrentUser();

        /// <summary>
        /// Returns the proposal checklists for the proposal
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>proposal checklists</returns>
        ICollection<ProposalChecklistDto> GetProposalChecklists(int proposalId);

        /// <summary>
        /// get par checklist content
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <returns>PAR checklist content data</returns>
        ChecklistContentDto GetPARChecklistContent(int proposalId);

        /// <summary>
        /// get ppr checklist content
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <returns>PPR checklist content data</returns>
        ChecklistContentDto GetPPRChecklistContent(int proposalId);

        /// <summary>
        /// Get all checklist save info for the given proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Collection of Save Info objects</returns>
        ICollection<ProposalChecklistSaveInfo> GetAllChecklistSaveInfo(int proposalId);
    }
}
