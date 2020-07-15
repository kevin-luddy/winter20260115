// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Checklist Mediator
    /// </summary>
    public class ChecklistMediator : IChecklistMediator
    {
        /// <summary>
        /// proposal checklist loader
        /// </summary>
        private IProposalChecklistLoader checklistLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inChecklistLoader">proposal checklist loader</param>
        public ChecklistMediator(IProposalChecklistLoader inChecklistLoader)
        {
            this.checklistLoader = inChecklistLoader;
        }

        /// <summary>
        /// Saves a proposal checklist DTO
        /// </summary>
        /// <param name="inProposalChecklist">proposal checklist Dto </param>
        /// <returns>Saved proposal checklist dto</returns>
        public int? SaveChecklistProposal(ProposalChecklistDto inProposalChecklist)
        {
            // Pre save business logic
            if (inProposalChecklist == null)
            {
                throw new ArgumentNullException(nameof(inProposalChecklist));
            }

            // Save
            int? toReturn = this.checklistLoader.Save(inProposalChecklist);

            // Return
            return toReturn;
        }

        /// <summary>
        /// Unlock checklist for the selected user(s)
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="updateDate">Proposal update date</param>
        /// <param name="unlockOption">Indicates pricer, peer, or both users</param>
        /// <returns>Id of the saved Proposal</returns>
        public int? UnlockChecklist(int proposalId, DateTime updateDate, UnlockChecklistOption unlockOption)
        {
            int? toReturn = this.checklistLoader.UnlockChecklist(proposalId, updateDate, unlockOption);
            return toReturn;
        }
    }
}
