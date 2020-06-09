// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System;
    using IES.Common;

    /// <summary>
    /// Lead Estimator Approval Model View
    /// </summary>
    public class ApprovalSectionModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ApprovalSectionModelView()
        {
            this.ApproverName = string.Empty;
            this.ApproverRole = PtmRole.NotSet;
            this.Comments = string.Empty;
            this.AdditionalEmailText = string.Empty;
            this.IsReadOnly = true;
            this.IsNoBid = false;
        }

        /// <summary>
        /// Name of Approver
        /// </summary>
        public string ApproverName { get; set; }

        /// <summary>
        /// Approver's Role
        /// </summary>
        public PtmRole ApproverRole { get; set; }
        
        /// <summary>
        /// Date and time approval was submitted
        /// </summary>
        public DateTime? DateOfApproval { get; set; }

        /// <summary>
        /// Comments submitted with approval
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the additional email text.
        /// </summary>
        public string AdditionalEmailText { get; set;  }

        /// <summary>
        /// Bool noting if approval section is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this proposal has non preferred tool.
        /// </summary>
        public bool HasNonPreferredTool { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this proposal has a status of No Bid
        /// </summary>
        public bool IsNoBid { get; set; }

        /// <summary>
        /// Header for Approver section
        /// </summary>
        public string ApproverHeader
        {
            get
            {
                switch (this.ApproverRole)
                {
                    case PtmRole.Pricer:
                        return "Lead Estimator";
                    case PtmRole.CoverSheetApprover:
                        return "Cover Sheet Approver";
                    case PtmRole.PricingVerification:
                        return "Pricing Verification";
                    case PtmRole.PeerReviewer:
                        return "Independent Reviewer";
                    case PtmRole.LOBEstLead:
                        return "LOB Estimating Manager / Delegate";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// String for displaying signature
        /// </summary>
        public string SignatureString
        {
            get
            {
                if (this.DateOfApproval.HasValue)
                {
                    return "Approved by " + this.ApproverName + " on " + this.DateOfApproval + " ET";
                }
                else
                {
                    return "Not signed";
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show reset workflow button].
        /// </summary>
        public bool ShowResetWorkflowButton { get; set; }

        /// <summary>
        /// Determines whether to enabled approval button
        /// </summary>
        public bool AllAttachmentsHaveBeenUploaded { get; set; }
    }
}
