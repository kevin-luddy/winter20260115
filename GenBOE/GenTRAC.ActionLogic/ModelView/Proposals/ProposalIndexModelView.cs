// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        /// Gets or sets the Revised Proposal Title
        /// </summary>
        public string RevisedProposalTitle { get; set; }

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
            this.DisplayNewRevisionButton = false;
            this.DisplayRevertRevisionButton = false;
            this.HasRdsbDocument = false;
            this.GenBoeWorkspaces = new Collection<string>();
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

        /// <summary>
        /// Determines whether to show or hide the "Revert to Prior Version" button
        /// </summary>
        public bool DisplayRevertRevisionButton { get; set; }

        /// <summary>
        /// Determines if there is an RDSB Document for this Proposal
        /// </summary>
        public bool HasRdsbDocument { get; set; }

        /// <summary>
        /// Gets/Sets list of any Workspaces using this Proposal
        /// </summary>
        public ICollection<string> GenBoeWorkspaces { get; set; }

        /// <summary>
        /// Reason Certification is Not Required
        /// </summary>
        public ReasonCertificationNotRequired? ReasonCertificationNotRequired { get; set; }

        /// <summary>
        /// Whether creating a new Revision
        /// </summary>
        public bool IsNewRevision { get; set; }

        /// <summary>
        /// ID of the revised proposal if this proposal is a revision
        /// </summary>
        public int? RevisedProposalId { get; set; }
    }
}
