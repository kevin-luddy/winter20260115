// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge
{
    /// <summary>
    /// Used to display Revision History tab
    /// </summary>
    public class RevisionHistoryModelView
    {
        /// <summary>
        /// Proposal Id
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Is currently selected (what proposal are you viewing this tab from)
        /// </summary>
        public bool IsCurrentlySelected { get; set; } = false;

        /// <summary>
        /// Proposal Tracking Number
        /// </summary>
        public string TrackingNumber { get; set; } = string.Empty;

        /// <summary>
        /// Proposal Title
        /// </summary>
        public string ProposalTitle { get; set; } = string.Empty;

        /// <summary>
        /// Workflow completed line (completion status & date)
        /// </summary>
        public string WorkflowCompletedLine { get; set; } = string.Empty;

        /// <summary>
        /// Certification completed line (completion status & date)
        /// </summary>
        public string CertificationCompletedLine { get; set; } = string.Empty;

        /// <summary>
        /// Should the proposal display the Proposal Setup tab
        /// </summary>
        public bool DisplayProposalSetupTab { get; set; } = false;

        /// <summary>
        /// Should the proposal display the checklist tab
        /// </summary>
        public bool DisplayChecklistTab { get; set; } = false;

        /// <summary>
        /// Should the proposal display the Post Submittal Attachment tab
        /// </summary>
        public bool DisplayPSATab { get; set; } = false;

        /// <summary>
        /// Should the proposal display the approvals tab
        /// </summary>
        public bool DisplayApprovalsTab { get; set; } = false;

        /// <summary>
        /// Should the proposal display the certification tab
        /// </summary>
        public bool DisplayCertificationTab { get; set; } = false;

        /// <summary>
        /// Should the proposal display the revision tab
        /// </summary>
        public bool DisplayRevisionTab { get; set; } = false;
    }
}
