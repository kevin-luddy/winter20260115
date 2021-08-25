// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Proposal Certification Timeline model view
    /// </summary>
    public class ProposalCertificationTimelineModelView : PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProposalCertificationTimelineModelView"/> class.
        /// </summary>
        public ProposalCertificationTimelineModelView()
        {
            this.DaysToCertification = string.Empty;
        }

        /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets the agreement date.
        /// </summary>
        public string AgreementDate { get; set; }

        /// <summary>
        /// Gets or sets the certification date.
        /// </summary>
        public string CertificationDate { get; set; }

        /// <summary>
        /// Gets or sets the days to certification.
        /// </summary>
        public string DaysToCertification { get; set; }

        /// <summary>
        /// Gets or sets the cut off date utilization.
        /// </summary>
        public CutOffDateUtilization? CutOffDateUtilization { get; set; }

        /// <summary>
        /// Gets or sets the cut off date utilization list.
        /// </summary>
        public ICollection<SelectListItem> CutOffDateUtilizationList { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Reason Certification is Not Required
        /// </summary>
        public ReasonCertificationNotRequired? ReasonCertificationNotRequired { get; set; }

        /// <summary>
        /// Gets or sets the cut off date utilization list.
        /// </summary>
        public ICollection<SelectListItem> ReasonCertificationNotRequiredList { get; set; }

        /// <summary>
        /// Other text for ReasonCertificationNotRequiredComment
        /// </summary>
        public string OtherReasonCommentCertification { get; set; }

        /// <summary>
        /// Should we display the "Reset" button
        /// </summary>
        public bool DisplayCertificationReset { get; set; }

        /// <summary>
        /// Should we disable the Certification Required checkbox
        /// </summary>
        public bool DisableCompleteButton { get; set; }
    }
}