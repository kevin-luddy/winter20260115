// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Proposal information model view
    /// </summary>
    public class ProposalInformationModelView : PersistedDataModelView
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
        /// Proposal Status
        /// </summary>
        public ProposalStatus ProposalStatus { get; set; }

        /// <summary>
        /// Proposal Checklist Type
        /// </summary>
        public ProposalChecklistType ProposalChecklistType { get; set; }

        /// <summary>
        /// Gets or sets Proposal Title
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.PROPOSAL_TITLE_REQUIRED)]
        [StringLength(75)]
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets selected contract action type
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.CONTRACT_ACTION_TYPE_REQUIRED)]
        public ContractActionType? ContractActionType { get; set; }

        /// <summary>
        /// Text for when Other is selected for Contract Action Type
        /// </summary>
        public string ContractActionTypeOtherText { get; set; }

        /// <summary>
        /// Gets or sets Proposal Type
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = ValidationConstants.ProposalValidationConstants.PROPOSAL_TYPE_REQUIRED)]
        public int ProposalType { get; set; }

        /// <summary>
        /// Gets or sets Customer
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.CUSTOMER_REQUIRED)]
        [StringLength(50)]
        public string Customer { get; set; }

        /// <summary>
        /// Gets or sets Customer Type
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.CUSTOMER_TYPE_REQUIRED)]
        [EnumRange((int)CustomerType.Commercial, (int)CustomerType.NA, ErrorMessage = ValidationConstants.ProposalValidationConstants.CUSTOMER_TYPE_REQUIRED)]
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// Gets or sets Request Type
        /// </summary>
        public int? RequestType { get; set; }

        /// <summary>
        /// Gets or sets Proposal Class
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.PROPOSAL_CLASS_REQUIRED)]
        public int ProposalClass { get; set; }

        /// <summary>
        /// Gets or sets RFP Number
        /// </summary>
        [StringLength(40)]
        public string RFPNumber { get; set; }

        /// <summary>
        /// Gets or sets RFP Issued Date
        /// </summary>
        public string RFPIssuedDate { get; set; }

        /// <summary>
        /// Gets or sets RFP Received Date
        /// </summary>
        public string RFPReceivedDate { get; set; }

        /// <summary>
        /// Gets or sets Anticipated Delivery Date
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.DELIVERY_DATE_REQUIRED)]
        public string AnticipatedDeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets Revised Submittal Date
        /// </summary>
        public string RevisedSubmittalDate { get; set; }

        /// <summary>
        /// Gets or sets SSC Role
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.SSC_ROLE_REQUIRED)]
        public ISGSRole ISGSRole { get; set; }

        /// <summary>
        /// Gets or sets Schedule Proposal flag
        /// </summary>
        public bool? IsScheduleProposal { get; set; }

        /// <summary>
        /// Gets or sets Contract Type Group
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.CONTRACT_TYPE_GROUP_REQUIRED)]
        public int ContractTypeGroup { get; set; }

        /// <summary>
        /// Gets or sets Contract Type
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.CONTRACT_TYPE_REQUIRED)]
        public ICollection<int> ContractType { get; set; }

        /// <summary>
        /// Gets or sets elements of cost
        /// </summary>
        [DisplayName("Elements of Cost")]
        public ICollection<int> CostElements { get; set; }

        /// <summary>
        /// Gets or sets Estimated Proposal Value
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ProposalValidationConstants.EST_PROP_VALUE_IN_DOLLARS)]
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.EST_VALUE_REQUIRED)]
        public string EstimatedProposalValue { get; set; }

        /// <summary>
        /// List of allowed proposal types
        /// </summary>
        public ICollection<SelectListItem> ProposalTypesList { get; set; }

        /// <summary>
        /// List of allowed customer types
        /// </summary>
        public ICollection<SelectListItem> CustomerTypesList { get; set; }

        /// <summary>
        /// List of allowed request types
        /// </summary>
        public ICollection<SelectListItem> RequestTypesList { get; set; }

        /// <summary>
        /// List of allowed proposal classes
        /// </summary>
        public ICollection<SelectListItem> ProposalClassesList { get; set; }

        /// <summary>
        /// List of allowed ISGS roles
        /// </summary>
        public ICollection<SelectListItem> ISGSRolesList { get; set; }

        /// <summary>
        /// List of allowed Contract Type Groups
        /// </summary>
        public ICollection<SelectListItem> ContractTypeGroupsList { get; set; }

        /// <summary>
        /// Options for Contract Types
        /// </summary>
        public string ContractTypeHtmlOptions { get; set; }

        /// <summary>
        /// List of allowed elements of cost
        /// </summary>
        public ICollection<IES.Common.ListBoxItem> CostElementsList { get; set; }

        /// <summary>
        /// Inactive Contract Types
        /// </summary>
        public string InactiveContractTypes { get; set; }

        /// <summary>
        /// Inactive Elements of Cost
        /// </summary>
        public string InactiveCostElements { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalInformationModelView()
        {
            this.ProposalID = -1;
            this.ContractType = new List<int>();
            this.CostElements = new List<int>();
            this.ProposalType = 0;
            this.RequestType = 0;
            this.ProposalClass = 0;
        }

        /// <summary>
        /// Text for Proposal Type (used by read only
        /// </summary>
        public string ProposalTypeText { get; set; }

        /// <summary>
        /// Text for Request Type (used by read only
        /// </summary>
        public string RequestTypeText { get; set; }

		/// <summary>
		/// Text for Contract Type Group (used by read only
		/// </summary>
		public string ContractTypeGroupText { get; set; }

		/// <summary>
		/// Text for Proposal Class (used by read only
		/// </summary>
		public string ProposalClassText { get; set; }

        /// <summary>
        /// Document Id (RDSB)
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Whether this is the proposal info for creating a new revision
        /// </summary>
        public bool IsNewRevision { get; set; }
    }
}
