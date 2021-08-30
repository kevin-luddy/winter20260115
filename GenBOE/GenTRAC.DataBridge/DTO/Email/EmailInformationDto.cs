// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Holds the Proposal information needed for sending an email to the provided users.
    /// </summary>
    [Serializable]
    public class EmailInformationDto
    {
        /// <summary>
        /// Gets or sets the email address to use.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets users to CC
        /// </summary>
        public Collection<UserDTO> ccUsers { get; set; }

        /// <summary>
        /// Gets or sets the Proposal tracking Number.
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets to Proposal ID.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Title.
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets the email type to send for the proposal.
        /// </summary>
        public EmailType ProposalEmailType { get; set; }

        /// <summary>
        /// Gets or sets the additional text.
        /// </summary>
        public string AdditionalText { get; set; }

        /// <summary>
        /// Gets the Proposal setup URL
        /// </summary>
        public Uri ProposalSetupUrl
        {
            get
            {
                string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
                string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#Proposal", commonUrl, this.ProposalId);
                return new Uri(url);
            }
        }

        /// <summary>
        /// Gets the Proposal approval URL.
        /// </summary>
        public Uri ProposalApprovalUrl
        {
            get
            {
                string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
                string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#Approvals", commonUrl, this.ProposalId);
                return new Uri(url);
            }
        }

        /// <summary>
        /// Gets the Proposal checklist URL
        /// </summary>
        public Uri ProposalChecklistUrl
        {
            get
            {
                string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
                string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#Checklist", commonUrl, this.ProposalId);
                return new Uri(url);
            }
        }

        /// <summary>
        /// Gets the proposal certification URL.
        /// </summary>
        public Uri ProposalCertificationUrl
        {
            get
            {
                string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
                string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#CertificationTimeline", commonUrl, this.ProposalId);
                return new Uri(url);
            }
        }

        /// <summary>
        /// Gets the Post Submittal Attachments URL.
        /// </summary>
        public Uri ProposalPsaUrl
        {
            get
            {
                string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
                string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#PSA", commonUrl, this.ProposalId);
                return new Uri(url);
            }
        }
    }
}