// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Core;

    /// <summary>
    /// Manage Proposal Info Dto Class
    /// </summary>
    [Serializable]
    public class ManageProposalInfoDto
    {
        /// <summary>
        /// Proposal Id
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Last Update Date
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// New Proposal Status
        /// </summary>
        public ProposalStatus NewProposalStatus { get; set; }

        /// <summary>
        /// Total Price
        /// </summary>
        public long? TotalPrice { get; set; }

        /// <summary>
        /// Checklist submitted date (Pricer)
        /// </summary>
        public DateTime? ChecklistSubmittedDatePricer { get; set; }

        /// <summary>
        /// Checklist submitted date (Peer)
        /// </summary>
        public DateTime? ChecklistSubmittedDatePeer { get; set; }

        /// <summary>
        /// Comments on the Manage Proposal Information page
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Date Estimating Submits To Contracts, used to be Proposal Submittal Date
        /// </summary>
        public DateTime? EstimatingSubmitsToContractsDate { get; set; }
    }
}
