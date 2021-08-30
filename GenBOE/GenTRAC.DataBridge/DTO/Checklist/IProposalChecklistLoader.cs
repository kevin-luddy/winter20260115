// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// Interface for Checklist Loader
    /// </summary>
    public interface IProposalChecklistLoader : IDataLoader<ProposalChecklistDto>
    {
        /// <summary>
        /// Returns the Checklist DTOs for a given Proposal IDs.
        /// </summary>
        /// <param name="inProposalIds">Collection of Proposal IDs</param>
        /// <returns>Collection of checklist DTOs</returns>
        ICollection<ProposalChecklistDto> GetByProposalIds(ICollection<int> inProposalIds);

        /// <summary>
        /// Get PPR checklist responses for given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Collection of PPR checklist response items</returns>
        ICollection<ChecklistResponseItem> GetPPRResponses(int proposalId);

        /// <summary>
        /// Get PAR checklist responses for given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Collection of PAR checklist response items</returns>
        ICollection<ChecklistResponseItem> GetPARResponses(int proposalId);

        /// <summary>
        /// Get all checklist save info for the given proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Collection of Save Info objects</returns>
        ICollection<ProposalChecklistSaveInfo> GetAllChecklistSaveInfo(int proposalId);

        /// <summary>
        /// Unlock checklist for the selected user(s)
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="updateDate">Proposal update date</param>
        /// <param name="unlockOption">Indicates pricer, peer, or both users</param>
        /// <returns>Id of the saved Proposal</returns>
        int? UnlockChecklist(int proposalId, DateTime updateDate, UnlockChecklistOption unlockOption);

        /// <summary>
        /// Get the proposal submit date for given proposal ids.
        /// </summary>
        /// <param name="proposalIds">Collection of proposal ids</param>
        /// <returns>Dictionary of proposal ids mapped to submit date</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<int, DateTime?> GetProposalSubmittalDate(ICollection<int> proposalIds);
    }
}
