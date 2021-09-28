// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Objects
{
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects.FullObject;

    /// <summary>
    /// Factory for creating full objects
    /// </summary>
    public interface IFullObjectFactory
    {
        /// <summary>
        /// Creates a full proposal
        /// </summary>
        /// <param name="proposal">Proposal Dto</param>
        /// <returns>Full Proposal</returns>
        FullProposal CreateFullProposal(ProposalDto proposal);
    }
}
