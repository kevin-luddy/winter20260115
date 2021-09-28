// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// prposal mediator class
    /// </summary>
    public class ProposalMediator : IProposalMediator
    {
        /// <summary>
        /// proposal Loader
        /// </summary>
        private IProposalLoader proposalLoader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inProposalLoader">proposal loader</param>
        public ProposalMediator(
            IProposalLoader inProposalLoader)
        {
            this.proposalLoader = inProposalLoader;
        }

        /// <summary>
        /// Saves a Proposal DTO
        /// </summary>
        /// <param name="inProposal">proposal Dto </param>
        /// <returns>Saved capture dto</returns>
        public int? SaveProposal(ProposalDto inProposal)
        {
            // Pre save business logic
            if (inProposal == null)
            {
                throw new ArgumentNullException(nameof(inProposal));
            }

            // Save
            int? toReturn = this.proposalLoader.Save(inProposal);
            
            // Return
            return toReturn;
        }

        /// <summary>
        /// save the proposal status
        /// </summary>
        /// <param name="inProposalId">proposal id</param>
        /// <param name="inUpdateDate">update date</param>
        /// <param name="inProposalState">proposal state</param>
        /// <returns>proposal id</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:ElementDocumentationMustNotBeCopiedAndPasted", Justification = "Need to have the proposal status more than once on this call")]
        public int? SaveProposalStatus(int inProposalId, DateTime inUpdateDate, ProposalStatus inProposalState)
        {
            int? toReturn = this.proposalLoader.UpdateProposalStatus(inProposalId, inUpdateDate, inProposalState);
            return toReturn;
        }
    }
}
