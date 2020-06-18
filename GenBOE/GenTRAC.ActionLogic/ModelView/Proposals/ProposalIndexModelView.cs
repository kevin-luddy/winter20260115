// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using IES.Common;

    /// <summary>
    /// Proposal information model view
    /// </summary>
    public class ProposalIndexModelView : PersistedDataModelView
    {
        /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets Proposal Tracking Number
        /// </summary>
        public string ProposalTrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets Forecasted Tracking Number
        /// </summary>
        public string ForecastedTrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Proposal status
        /// </summary>
        public ProposalStatus ProposalStatus { get; set; }

        /// <summary>
        /// Gets or sets Anticipated Delivery Date
        /// </summary>
        public string AnticipatedDeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets Revised Submittal Date
        /// </summary>
        public string RevisedSubmittalDate { get; set; }

        /// <summary>
        /// Gets or sets Approval Completed Date
        /// </summary>
        public string CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets Certification Completed Date
        /// </summary>
        public bool IsCCoPD { get; set; }

        /// <summary>
        /// Gets or sets Certification Completed Date
        /// </summary>
        public string CertificationCompletedDate { get; set; }

        /// <summary>
        /// Gets or sets whether Approval was completed before Certification was enabled.
        /// </summary>
        public bool CompletedBeforeCertification { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalIndexModelView()
        {
            this.ProposalID = -1;
        }

        /// <summary>
        /// Determines wheter the logged in user is a US user.
        /// </summary>
        public bool IsUsUser { get; set; }

        /// <summary>
        /// Determines whether to hide/show PSA tab
        /// </summary>
        public SecurityAuthorization PsaVisibility { get; set; }

        /// <summary>
        /// Determines whether to hide/show CertificationTimeline tab
        /// </summary>
        public SecurityAuthorization CertificationTimelineVisibility { get; set; }

        /// <summary>
        /// Determines whether to show or hide the + New Revision button
        /// </summary>
        public bool DisplayNewRevisionButton { get; set; }
    }
}
