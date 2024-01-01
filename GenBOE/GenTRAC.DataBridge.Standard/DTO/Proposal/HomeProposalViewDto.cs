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
    /// Dto used to hold data that will be displayed on Home Proposal View.
    /// </summary>
    [Serializable]
    public class HomeProposalViewDto
    {
        /// <summary>
        /// Proposal ID
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Tracking Number
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Forecasted Tracking Number
        /// </summary>
        public string ForecastedTrackingNumber { get; set; }

        /// <summary>
        /// Proposal title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Program Area
        /// </summary>
        public string ProgramArea { get; set; }

        /// <summary>
        /// Customer
        /// </summary>
        public string Customer { get; set; }
        
        /// <summary>
        /// If Customer Type is Commercial or International Commercial
        /// </summary>
        public bool IsCommercialCustomer { get; set; }

        /// <summary>
        /// estimate value
        /// </summary>
        public long? EstValue { get; set; }

        /// <summary>
        /// submitted value
        /// </summary>
        public long? SubmittedValue { get; set; }

        /// <summary>
        /// Proposal date assigned
        /// </summary>
        public DateTime? ProposalDateAssigned { get; set; }

        /// <summary>
        /// proposal due date
        /// </summary>
        public DateTime ProposalDueDate { get; set; }

        /// <summary>
        /// proposal submittal date
        /// </summary>
        public DateTime? ProposalSubmittalDate { get; set; }

        /// <summary>
        /// checklist complete date
        /// </summary>
        public DateTime? ChecklistCompleteDate { get; set; }

        /// <summary>
        /// proposal status 
        /// </summary>
        public ProposalStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Pricers's account display name
        /// </summary>
        public string PricerDisplayName { get; set; }

        /// <summary>
        /// Peer Reviewer's Display Name
        /// </summary>
        public string PeerReviewerDisplayName { get; set; }

        /// <summary>
        /// Cost Volume Lead's Display Name.
        /// </summary>
        public string CostVolumeLeadDisplayName { get; set; }

        /// <summary>
        /// Capture Manager's Display Name
        /// </summary>
        public string CaptureManagerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has linked document.
        /// </summary>
        public bool HasLinkedDocument { get; set; }

        /// <summary>
        /// Indicates whether the user has write access to the linked document
        /// </summary>
        public bool HasWriteAccessToLinkedDocument { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is forecast proposal.
        /// </summary>
        public bool IsForecastProposal { get; set; }

        /// <summary>
        /// Proposal either has, or is, a revision
        /// </summary>
        public bool HasOrIsRevision { get; set; }
    }
}
