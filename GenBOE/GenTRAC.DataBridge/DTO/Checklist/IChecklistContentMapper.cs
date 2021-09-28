// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    /// <summary>
    /// Interface for Checklist Content Mapper
    /// </summary>
    public interface IChecklistContentMapper
    {
        /// <summary>
        /// Get Checklist Content DTO by Proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Checklist Content DTO</returns>
        ChecklistContentDto GetChecklistByProposalId(int proposalId);
    }
}
