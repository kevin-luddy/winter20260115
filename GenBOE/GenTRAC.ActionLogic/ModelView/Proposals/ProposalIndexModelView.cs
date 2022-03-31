// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenTRAC.DataBridge.DTO;
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
        /// Determines whether to hide/show Revision History tab
        /// </summary>
        public SecurityAuthorization RevisionHistoryVisibility { get; set; }

        /// <summary>
        /// Determines whether to hide/show the Contracts tab
        /// </summary>
        public SecurityAuthorization ContractsVisibility { get; set; }

        /// <summary>
        /// Determines whether to show or hide the + New Revision button
        /// </summary>
        public bool DisplayNewRevisionButton { get; set; }

        /// <summary>
        /// Should a New Revision button be disabled
        /// </summary>
        public bool NewRevisionButtonDisabled
        {
            get
            {
                // We disable the "Add New Revision" button if:
                //      Proposal is Revised
                //   OR Proposal is Completed AND is CCOPD AND certification completed OR certification marked as not required
                // In other words, we are allowed to add a new revision when
                //      proposal is Submitted (worflow completed, is CCOPD, and waiting for certification)
                //   OR proposal is Completed and is not CCOPD
                return this.ProposalStatus == ProposalStatus.Revised 
                        || (this.ProposalStatus == ProposalStatus.Completed && this.IsCCoPD && (this.CertificationCompletedDate != null || this.ReasonCertificationNotRequired.HasValue));
            }
        }

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

        /// <summary>
        /// Text for certification completed line
        /// </summary>
        public string CertificationCompletedLineText
        {
            get
            {
                DateTime? certificationCompleted = DateTime.TryParse(this.CertificationCompletedDate, out DateTime temp) ? (DateTime?)temp : null;
                return ProposalLoader.GetCertificationCompletedLineText(this.ProposalStatus, this.IsCCoPD, certificationCompleted);
            }
        }

        /// <summary>
        /// Text for workflow status line
        /// </summary>
        public string WorkflowStatusLineText
        {
            get
            {
                DateTime? completedDate = DateTime.TryParse(this.CompletedDate, out DateTime temp) ? (DateTime?)temp : null;
                return ProposalLoader.GetWorkflowCompletedLineText(this.ProposalStatus, DateTime.Parse(this.AnticipatedDeliveryDate), 
                    completedDate, DateTime.TryParse(this.RevisedSubmittalDate, out DateTime rsDate) ? rsDate : (DateTime?)null);
            }
        }
    }
}
