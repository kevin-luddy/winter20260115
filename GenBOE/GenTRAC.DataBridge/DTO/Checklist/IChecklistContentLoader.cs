// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    /// <summary>
    /// Interface for Checklist Content Loader
    /// </summary>
    public interface IChecklistContentLoader
    {
        /// <summary>
        /// Get checklist content by proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Checklist Content DTO</returns>
        ChecklistContentDto GetChecklistByProposalId(int proposalId);

        /// <summary>
        /// Get Checklist ID by given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Id of checklist</returns>
        int? GetChecklistIdByProposalId(int proposalId);
    }
}
