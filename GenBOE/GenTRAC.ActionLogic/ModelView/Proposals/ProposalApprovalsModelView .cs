// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using DataBridge.DTO;
    using GenTRAC.ActionLogic.Validation;

    /// <summary>
    /// Proposal approvals model view
    /// </summary>
    public class ProposalApprovalsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalApprovalsModelView()
        {
        }
        
        /// <summary>
        /// Gets or sets Lead Estimator
        /// </summary>
        [System.ComponentModel.DisplayName("Lead Estimator")]
        public string LeadEstimatorNtid { get; set; }

        /// <summary>
        /// Gets or sets Lead Estimator's account display name
        /// </summary>
        public string LeadEstimatorDisplayName { get; set; }

        /// <summary>
        /// Gets or sets CoverSheetApprover
        /// </summary>
        [System.ComponentModel.DisplayName("Cover Sheet Approver")]
        public string CoverSheetApproverNtid { get; set; }

        /// <summary>
        /// Gets or sets CoverSheetApprover's account display name
        /// </summary>
        public string CoverSheetApproverDisplayName { get; set; }

        /// <summary>
        /// Gets or sets PricingVerification
        /// </summary>
        [System.ComponentModel.DisplayName("Pricing Verification")]
        public string PricingVerificationNtid { get; set; }

        /// <summary>
        /// Gets or sets PricingVerification's account display name
        /// </summary>
        public string PricingVerificationDisplayName { get; set; }

        /// <summary>
        /// Gets or sets LOBEstimatingLeadMgr
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.LOB_ESTIMATING_LEAD_MGR_REQUIRED)]
        [System.ComponentModel.DisplayName("LOB Estimating Manager / Delegate")]
        public string LOBEstimatingLeadMgrNtid { get; set; }

        /// <summary>
        /// Gets or sets LOBEstimatingLeadMgr's account display name
        /// </summary>
        public string LOBEstimatingLeadMgrDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Independent Reviewer
        /// </summary>
        [System.ComponentModel.DisplayName("Independent Reviewer")]
        public string IndependentReviewerNtid { get; set; }

        /// <summary>
        /// Gets or setsIndependentReviewer's account display name
        /// </summary>
        public string IndependentReviewerDisplayName { get; set; }

        /// <summary>
        /// PricingVerificationList
        /// </summary>
        public ICollection<UserDTO> PricingVerificationList { get; set; }

        /// <summary>
        /// LOBEstimatingMgrList
        /// </summary>
        public ICollection<UserDTO> LOBEstimatingMgrList { get; set; }
        
        /// <summary>
        /// CoverSheetApproverList
        /// </summary>
        public ICollection<UserDTO> CoverSheetApproverList { get; set; }

        /// <summary>
        /// LeadEstimatorList
        /// </summary>
        public ICollection<UserDTO> LeadEstimatorList { get; set; }

        /// <summary>
        /// IndependentReviewerList
        /// </summary>
        public ICollection<UserDTO> IndependentReviewerList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lead estimator is read only.
        /// </summary>
        public bool IsLeadEstimatorReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is CCPD required.
        /// </summary>
        public bool? IsCCPDRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cover sheet approver is read only.
        /// </summary>
        public bool IsCoverSheetApproverReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pricing verification is read only.
        /// </summary>
        public bool IsPricingVerificationReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether independent reviewer is read only.
        /// </summary>
        public bool IsIndependentReviewerReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lob estimating lead MGR is read only.
        /// </summary>
        public bool IsLOBEstimatingLeadMgrReadOnly { get; set; }
    }
}
