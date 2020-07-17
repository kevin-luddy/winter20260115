// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Common;

    /// <summary>
    /// Approvals DTO
    /// </summary>
    [Serializable]
    public class ApprovalsDto
    {
        /// <summary>
        /// Proposal Id
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Proposal Tracking Number
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Capture Manager
        /// </summary>
        public string CaptureManager { get; set; }

        /// <summary>
        /// Proposal Status
        /// </summary>
        public ProposalStatus ProposalStatus { get; set; }

        /// <summary>
        /// User's role
        /// </summary>
        public PtmRole UserRole { get; set; }

        /// <summary>
        /// Date when it was submitted for approvals
        /// </summary>
        public DateTime SubmittedForApprovalDate { get; set; }

        /// <summary>
        /// Date when approved (if null, means it's pending)
        /// </summary>
        public DateTime? ApprovedDate { get; set; }
    }
}
