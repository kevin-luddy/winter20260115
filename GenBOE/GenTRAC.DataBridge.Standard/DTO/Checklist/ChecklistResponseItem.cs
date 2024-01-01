// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Standard;

    /// <summary>
    /// Checklist Response Item
    /// </summary>
    [Serializable]
    public class ChecklistResponseItem
    {
        /// <summary>
        /// Proposal Id
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Checklist type: ProposalPricingReview, ProposalAdequacyReview
        /// </summary>
        public ChecklistType ChecklistType { get; set; }

        /// <summary>
        /// Checklist content Id
        /// </summary>
        public int ChecklistContentId { get; set; }

        /// <summary>
        /// Response Id: Yes, No, NA, NotSet
        /// </summary>
        public ChecklistResponseOption Response { get; set; }

        /// <summary>
        /// Response Type: Pricer, Peer
        /// </summary>
        public ChecklistResponseType ResponseType { get; set; }

        /// <summary>
        /// Pricer Page Number for each PAR checklist row, can be digits or numbers (30 character max limit enforced in model)
        /// </summary>
        public string PricerPageNumber { get; set; }

        /// <summary>
        /// Row comment for each PAR checklist row (300 characters max limit enforced in model)
        /// </summary>
        public string RowComment { get; set; }

        /// <summary>
        /// Optional Canned Reponse (Id)
        /// </summary>
        public int? CannedResponseId { get; set; }

        /// <summary>
        /// Selected canned response text (optional)
        /// </summary>
        public string CannedResponseText { get; set; }
    }
}
