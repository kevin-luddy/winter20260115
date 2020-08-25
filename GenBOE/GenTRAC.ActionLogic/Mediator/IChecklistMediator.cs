// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using IES.Common;

    /// <summary>
    /// Checklist mediator interface
    /// </summary>
    public interface IChecklistMediator
    {
        /// <summary>
        /// Save checklist proposal data
        /// </summary>
        /// <param name="inProposalChecklist">proposal checklist dto</param>
        /// <returns>proposal checklist id</returns>
        int? SaveChecklistProposal(GenTRAC.DataBridge.DTO.ProposalChecklistDto inProposalChecklist);

        /// <summary>
        /// Unlock checklist for the selected user(s)
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="updateDate">Proposal update date</param>
        /// <param name="unlockOption">Indicates pricer, peer, or both users</param>
        /// <returns>Id of the saved Proposal</returns>
        int? UnlockChecklist(int proposalId, DateTime updateDate, UnlockChecklistOption unlockOption);
    }
}
