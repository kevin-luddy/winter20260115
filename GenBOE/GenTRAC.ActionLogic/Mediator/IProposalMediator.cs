// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Proposal Mediator interface
    /// </summary>
    public interface IProposalMediator
    {
        /// <summary>
        /// saves a proposal
        /// </summary>
        /// <param name="inProposal">proposal to save</param>
        /// <returns>proposal id</returns>
        int? SaveProposal(ProposalDto inProposal);

        /// <summary>
        /// save the propsal status
        /// </summary>
        /// <param name="inProposalId">proposal id</param>
        /// <param name="inUpdateDate">update date</param>
        /// <param name="inProposalState">proposal state</param>
        /// <returns>proposal id</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:ElementDocumentationMustNotBeCopiedAndPasted", Justification = "Need to have the proposal status more than once on this call")]
        int? SaveProposalStatus(int inProposalId, DateTime inUpdateDate, ProposalStatus inProposalState);
    }
}
