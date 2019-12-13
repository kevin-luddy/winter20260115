// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Manage Proposal Information Details view
    /// </summary>
    public class ManageProposalInfoDetailsView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ManageProposalInfoDetailsView()
        {
            this.ProposalStatusList = new List<SelectListItem>();
        }

        /// <summary>
        /// Proposal Id
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Tracking Number
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Old Proposal Status
        /// </summary>
        public ProposalStatus OldStatus { get; set; }

        /// <summary>
        /// New Proposal Status
        /// </summary>
        public ProposalStatus NewStatus { get; set; }

        /// <summary>
        /// List of proposal status options
        /// </summary>
        public ICollection<SelectListItem> ProposalStatusList { get; set; }

        /// <summary>
        /// Old Proposal Submittal Date
        /// </summary>
        public string OldProposalSubmittalDate { get; set; }

        /// <summary>
        /// New Proposal Submittal Date
        /// </summary>
        public string NewProposalSubmittalDate { get; set; }

        /// <summary>
        /// Old Total price
        /// </summary>
        public string OldTotalPrice { get; set; }

        /// <summary>
        /// New Total price
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ManageProposalInfoValidationConstants.TOTAL_PRICE_FORMAT)]
        public string NewTotalPrice { get; set; }

        /// <summary>
        /// Old Checklist Submitted Date (Pricer)
        /// </summary>
        public string OldChecklistSubmittedDatePricer { get; set; }

        /// <summary>
        /// New Checklist Submitted Date (Pricer)
        /// </summary>
        public string NewChecklistSubmittedDatePricer { get; set; }

        /// <summary>
        /// Old Checklist Submitted Sate (Peer)
        /// </summary>
        public string OldChecklistSubmittedDatePeer { get; set; }

        /// <summary>
        /// New Checklist Submitted Sate (Peer)
        /// </summary>
        public string NewChecklistSubmittedDatePeer { get; set; }

        /// <summary>
        /// Flag for whether to display the fields for a Completed proposal
        /// </summary>
        public bool ShowCompletedSection { get; set; }

        /// <summary>
        /// Flag for whether to display the Checklist submitted date (Peer)
        /// </summary>
        public bool ShowChecklistSubmittedDatePeer { get; set; }
    }
}
