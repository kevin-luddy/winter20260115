// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// The PBOE Model View for BOE Forms.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView" />
    public class BOEFormPBOEModelView : BOEFormModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormPBOEModelView"/> class.
        /// </summary>
        public BOEFormPBOEModelView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormPBOEModelView"/> class.
        /// </summary>
        /// <param name="dto">The dto to copy from.</param>
        /// <exception cref="System.ArgumentNullException">dto</exception>
        public BOEFormPBOEModelView(BOEFormPBOEDTO dto) : base(dto)
        {
            if (ReferenceEquals(dto, null))
            {
                throw new ArgumentNullException(nameof(dto));
            }

            this.CCoPDApplies = dto.CCoPDApplies;
            this.SupplierCCoPD = dto.SupplierCCoPD;
            this.CommercialItemExceptionApplies = dto.CommercialItemExceptionApplies;
            this.CompetitionExceptionApplies = dto.CompetitionExceptionApplies;
            this.LessThanThresholdExceptionApplies = dto.LessThanThresholdExceptionApplies;
            this.OtherExceptionApplies = dto.OtherExceptionApplies;
            this.OtherText = dto.OtherText;
            this.RFP = dto.RFP;
            this.SupplierName = dto.SupplierName;
            this.ValidityDate = dto.ValidityDate;
            this.ShouldCostEstimate = dto.ShouldCostEstimate;
            this.ShouldCostEstimateDate = dto.ShouldCostEstimateDate;
            this.RFPRelease = dto.RFPRelease;
            this.RFPReleaseDate = dto.RFPReleaseDate;
            this.FirmSupplierReceipt = dto.FirmSupplierReceipt;
            this.FirmSupplierReceiptDate = dto.FirmSupplierReceiptDate;
            this.SourceSelection = dto.SourceSelection;
            this.SourceSelectionDate = dto.SourceSelectionDate;
            this.CID = dto.CID;
            this.CIDDate = dto.CIDDate;
            this.PriceAnalysis = dto.PriceAnalysis;
            this.PriceAnalysisDate = dto.PriceAnalysisDate;
            this.TechnicalEvaluation = dto.TechnicalEvaluation;
            this.TechnicalEvaluationDate = dto.TechnicalEvaluationDate;
            this.FactFinding = dto.FactFinding;
            this.FactFindingDate = dto.FactFindingDate;
            this.CostAnalysis = dto.CostAnalysis;
            this.CostAnalysisDate = dto.CostAnalysisDate;
            this.GovtPricing = dto.GovtPricing;
            this.GovtPricingDate = dto.GovtPricingDate;
            this.SupplierNegotiations = dto.SupplierNegotiations;
            this.SupplierNegotiationsDate = dto.SupplierNegotiationsDate;
            this.MOU = dto.MOU;
            this.MOUDate = dto.MOUDate;
            this.PlannedDate_WrittenApproval = dto.PlannedDate_WrittenApproval;
            this.PlannedDate_ApprovedSubmission = dto.PlannedDate_ApprovedSubmission;
            this.ProposalNumber = dto.ProposalNumber;
            this.CIDText = dto.CIDText;
            this.PriceAnalysisText = dto.PriceAnalysisText;
            this.TechnicalEvaluationText = dto.TechnicalEvaluationText;
            this.FactFindingText = dto.FactFindingText;
            this.CostAnalysisText = dto.CostAnalysisText;
            this.GovtPricingText = dto.GovtPricingText;
            this.SupplierNegotiationsText = dto.SupplierNegotiationsText;
            this.MOUText = dto.MOUText;
            this.ShouldCostEstimateText = dto.ShouldCostEstimateText;
            this.RFPReleaseText = dto.RFPReleaseText;
            this.FirmSupplierReceiptText = dto.FirmSupplierReceiptText;
            this.SourceSelectionText = dto.SourceSelectionText;
            this.SourceSelectionDescription = dto.SourceSelectionDescription;
            this.CommercialityDescription = dto.CommercialityDescription;
            this.TechnicalEvaluationDescription = dto.TechnicalEvaluationDescription;
            this.PriceAnalysisDescription = dto.PriceAnalysisDescription;
            this.CostAnalysisDescription = dto.CostAnalysisDescription;
            this.RationaleValueSummary = dto.RationaleValueSummary;
            this.GovtPricingReceived = dto.GovtPricingReceived ?? ScheduleEvent.NA;
            this.GovtPricingReceivedDate = dto.GovtPricingReceivedDate;
            this.GovtPricingReceivedText = dto.GovtPricingReceivedText;
            this.CostAnalysisUnqual = dto.CostAnalysisUnqual ?? ScheduleEvent.NA;
            this.CostAnalysisUnqualDate = dto.CostAnalysisUnqualDate;
            this.CostAnalysisUnqualText = dto.CostAnalysisUnqualText;
            this.SupplierProposedValue = dto.SupplierProposedValue;
            this.VendorId = dto.VendorId;
        }


        /// <summary>
        /// Gets or sets a value indicating whether CCoPD Applies.
        /// </summary>
        public bool CCoPDApplies { get; set; }

        /// <summary>
        /// Supplier CCoPD Applies
        /// </summary>
        [RequiredIf("CCoPDApplies", true, ErrorMessage = "Supplier CCoPD Applies is required when CCoPD Applies is selected.")]
        public TripleBooleanState? SupplierCCoPD { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether commercial item exception applies.
        /// </summary>
        public bool CommercialItemExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether competition exception applies.
        /// </summary>
        public bool CompetitionExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether less than threshold exception applies.
        /// </summary>
        public bool LessThanThresholdExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is an other exception that applies.
        /// </summary>
        public bool OtherExceptionApplies { get; set; }

        /// <summary>
        /// Other text, only required if OtherExceptionApplies is true
        /// </summary>
        [RequiredIf("OtherExceptionApplies", true, ErrorMessage = "Other Exception Applies Text is required when CCoPD Other Exception applies is checked.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for Other Exception Applies Text.")]
        public string OtherText { get; set; }

        /// <summary>
        /// Gets or sets the RFP.
        /// </summary>
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for RFP.")]
        public string RFP { get; set; }


        /// <summary>
        /// Gets or sets the proposal number.
        /// </summary>
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Proposal Number.")]
        public string ProposalNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the supplier.
        /// </summary>
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Supplier Name.")]
        public string SupplierName { get; set; }

        /// <summary>
        /// Gets or sets the validity date.
        /// </summary>
        [RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "Validity Date must be in mm/dd/yyyy format.")]
        public string ValidityDate { get; set; }

        /// <summary>
        /// Should Cost/Engineering Estimate
        /// </summary>
        [Required]
        public ScheduleEvent ShouldCostEstimate { get; set; }
        [RequiredIf("ShouldCostEstimate", ScheduleEvent.Actual, ErrorMessage = "Should Cost/Engineering Estimate Date is required if Schedule Event is actual.")]
        public DateTime? ShouldCostEstimateDate { get; set; }
        [RequiredIf("IsShouldCostEstimateTextRequired", true, ErrorMessage = "Should Cost/Engineering Estimate Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Should Cost/Engineering Estimate Text.")]
        public string ShouldCostEstimateText { get; set; }

        /// <summary>
        /// RFP Release to Supplier(s)
        /// </summary>
        [Required]
        public ScheduleEvent RFPRelease { get; set; }
        [RequiredIf("RFPRelease", ScheduleEvent.Actual, ErrorMessage = "RFP Release Date is required if Schedule Event is actual.")]
        public DateTime? RFPReleaseDate { get; set; }
        [RequiredIf("IsRFPReleaseTextRequired", true, ErrorMessage = "RFP Release Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for RFP Release Text.")]
        public string RFPReleaseText { get; set; }

        /// <summary>
        /// Firm Supplier proposal(s) Receipt
        /// </summary>
        [Required]
        public ScheduleEvent FirmSupplierReceipt { get; set; }
        [RequiredIf("FirmSupplierReceipt", ScheduleEvent.Actual, ErrorMessage = "Firm Supplier Proposal(s) Receipt Date is required if Schedule Event is actual.")]
        public DateTime? FirmSupplierReceiptDate { get; set; }
        [RequiredIf("IsFirmSupplierReceiptTextRequired", true, ErrorMessage = "Firm Supplier Proposal(s) Receipt Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Firm Supplier Proposal(s) Receipt Text.")]
        public string FirmSupplierReceiptText { get; set; }

        /// <summary>
        /// Source Selection
        /// </summary>
        [Required]
        public ScheduleEvent SourceSelection { get; set; }
        [RequiredIf("SourceSelection", ScheduleEvent.Actual, ErrorMessage = "Source Selection Date is required if Schedule Event is actual.")]
        public DateTime? SourceSelectionDate { get; set; }
        [RequiredIf("IsSourceSelectionTextRequired", true, ErrorMessage = "Source Selection Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Source Selection Text.")]
        public string SourceSelectionText { get; set; }

        /// <summary>
        /// Commercial item Documentation (CID)
        /// </summary>
        [Required]
        public ScheduleEvent CID { get; set; }
        [RequiredIf("CID", ScheduleEvent.Actual, ErrorMessage = "CID Date is required if Schedule Event is actual.")]
        public DateTime? CIDDate { get; set; }
        [RequiredIf("IsCIDTextRequired", true, ErrorMessage = "CID Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for CID Text.")]
        public string CIDText { get; set; }

        /// <summary>
        /// Price Analysis
        /// </summary>
        [Required]
        public ScheduleEvent PriceAnalysis { get; set; }
        [RequiredIf("PriceAnalysis", ScheduleEvent.Actual, ErrorMessage = "Price Analysis Date is required if Schedule Event is actual.")]
        public DateTime? PriceAnalysisDate { get; set; }
        [RequiredIf("IsPriceAnalysisTextRequired", true, ErrorMessage = "Price Analysis Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Price Analysis Text.")]
        public string PriceAnalysisText { get; set; }

        /// <summary>
        /// Technical Evaluation
        /// </summary>
        [Required]
        public ScheduleEvent TechnicalEvaluation { get; set; }
        [RequiredIf("TechnicalEvaluation", ScheduleEvent.Actual, ErrorMessage = "Technical Evaluation Date is required if Schedule Event is actual.")]
        public DateTime? TechnicalEvaluationDate { get; set; }
        [RequiredIf("IsTechnicalEvaluationTextRequired", true, ErrorMessage = "Technical Evaluation Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Technical Evaluation Text.")]
        public string TechnicalEvaluationText { get; set; }

        /// <summary>
        /// Fact Finding
        /// </summary>
        [Required]
        public ScheduleEvent FactFinding { get; set; }
        [RequiredIf("FactFinding", ScheduleEvent.Actual, ErrorMessage = "Fact Finding Date is required if Schedule Event is actual.")]
        public DateTime? FactFindingDate { get; set; }
        [RequiredIf("IsFactFindingTextRequired", true, ErrorMessage = "Fact Finding Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Fact Finding Text.")]
        public string FactFindingText { get; set; }

        /// <summary>
        /// Cost Analysis
        /// </summary>
        [Required]
        public ScheduleEvent CostAnalysis { get; set; }
        [RequiredIf("CostAnalysis", ScheduleEvent.Actual, ErrorMessage = "Cost Analysis Date is required if Schedule Event is actual.")]
        public DateTime? CostAnalysisDate { get; set; }
        [RequiredIf("IsCostAnalysisTextRequired", true, ErrorMessage = "Cost Analysis Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Cost Analysis Text.")]
        public string CostAnalysisText { get; set; }

        /// <summary>
        /// Govt. Pricing Assistance for CCoPD Review Requested by LM
        /// </summary>
        [Required]
        public ScheduleEvent GovtPricing { get; set; }
        [RequiredIf("GovtPricing", ScheduleEvent.Actual, ErrorMessage = "Govt Pricing Assistance Date is required if Schedule Event is actual.")]
        public DateTime? GovtPricingDate { get; set; }
        [RequiredIf("IsGovtPricingTextRequired", true, ErrorMessage = "Govt Pricing Assistance Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Govt Pricing Assistance Text.")]
        public string GovtPricingText { get; set; }

        /// <summary>
        /// Govt. Pricing Received for CCoPD Review Received by LM
        /// </summary>
        [Required]
        public ScheduleEvent GovtPricingReceived { get; set; }
        [RequiredIf("GovtPricingReceived", ScheduleEvent.Actual, ErrorMessage = "Govt Pricing Received Date is required if Govt Pricing Received is actual.")]
        public DateTime? GovtPricingReceivedDate { get; set; }
        [RequiredIf("IsGovtPricingReceivedTextRequired", true, ErrorMessage = "Govt Pricing Received Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Govt Pricing Received Text.")]
        public string GovtPricingReceivedText { get; set; }

        /// <summary>
        /// Cost Analysis – Unqualified (Final)
        /// </summary>
        public ScheduleEvent CostAnalysisUnqual { get; set; }
        [RequiredIf("CostAnalysisUnqual", ScheduleEvent.Actual, ErrorMessage = "Cost Analysis Unqualified Date, is required if Cost Analysis is actual.")]
        public DateTime? CostAnalysisUnqualDate { get; set; }
        [RequiredIf("IsCostAnalysisUnqualTextRequired", true, ErrorMessage = "Cost Analysis Unqualified Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Cost Analysis Unqualified Text.")]
        public string CostAnalysisUnqualText { get; set; }

        /// <summary>
        /// Supplier Negotiations Complete
        /// </summary>
        [Required]
        public ScheduleEvent SupplierNegotiations { get; set; }
        [RequiredIf("SupplierNegotiations", ScheduleEvent.Actual, ErrorMessage = "Supplier Negotiations Complete Date is required if Schedule Event is actual.")]
        public DateTime? SupplierNegotiationsDate { get; set; }
        [RequiredIf("IsSupplierNegotiationsTextRequired", true, ErrorMessage = "Supplier Negotiations Complete Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for Supplier Negotiations Complete Text.")]
        public string SupplierNegotiationsText { get; set; }

        /// <summary>
        /// Memorandum of Understanding
        /// </summary>
        [Required]
        public ScheduleEvent MOU { get; set; }
        [RequiredIf("MOU", ScheduleEvent.Actual, ErrorMessage = "MOU Date is required if Schedule Event is actual.")]
        public DateTime? MOUDate { get; set; }
        [RequiredIf("IsMOUTextRequired", true, ErrorMessage = "MOU Text is required if Schedule Event is planned and there is no Date set.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed for MOU Text.")]
        public string MOUText { get; set; }

        /// <summary>
        /// Source Selection Description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_SOURCE_SELECTION_DESC, "PBOEFormID")]
        [Required(ErrorMessage = "Source Selection Summary is required.")]
        public string SourceSelectionDescription { get; set; }

        /// <summary>
        /// Commerciality Description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_COMMERCIALITY_DESC, "PBOEFormID")]
        [RequiredIf("CommercialItemExceptionApplies", true, ErrorMessage = "Commerciality is required if Commerical Item Exemption applies.")]
        public string CommercialityDescription { get; set; }

        /// <summary>
        /// Technical Evaluation Description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_TECHNICAL_EVAL_DESC, "PBOEFormID")]
        [RequiredIf("CCoPDApplies", true, ErrorMessage = "Technical Evaluation Summary is required if CCoPD is set.")]
        public string TechnicalEvaluationDescription { get; set; }

        /// <summary>
        /// Price Analysis Description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_PRICE_ANALYSIS_DESC, "PBOEFormID")]
        [Required(ErrorMessage = "Price Analysis Description is required.")]
        public string PriceAnalysisDescription { get; set; }

        /// <summary>
        /// Cost Analysis Description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_COST_ANALYSIS_DESC, "PBOEFormID")]
        [RequiredIf("CCoPDApplies", true, ErrorMessage = "Cost Analysis Summary is required if CCoPD is set.")]
        public string CostAnalysisDescription { get; set; }

        /// <summary>
        /// Rationale for LM Proposed Value Summary
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_RATIONALE_VALUE_SUMMARY, "PBOEFormID")]
        [Required(ErrorMessage = "Rationale for LM Proposed Value Summary is required.")]
        public string RationaleValueSummary { get; set; }

        /// <summary>
        /// The Vendor ID associated with the supplier
        /// </summary>
        [Required(ErrorMessage = "The Vendor ID is required.")]
        public string VendorId { get; set; }

        /// <summary>
        /// Value of Supplier proposal
        /// </summary>
        [Range(0, 99999999999.99, ErrorMessage = "Supplier Proposed Value must be between 0 and $99,999,999,999.99.")]
        public decimal? SupplierProposedValue { get; set; }

        /// <summary>
        /// Date of customer written approval for submission after initial prime proposal.
        /// </summary>
        [RequiredIf("IsPlannedDatesRequired", true, ErrorMessage = "Date of Customer Written Approval for Submission after Initial Prime Proposal is required.")]
        public DateTime? PlannedDate_WrittenApproval { get; set; }
        
        /// <summary>
        /// Approved date for submission to customer.
        /// </summary>
        [RequiredIf("IsPlannedDatesRequired", true, ErrorMessage = "Approved Date for Submission to Customer is required.")]
        public DateTime? PlannedDate_ApprovedSubmission { get; set; }

        /// <summary>
        /// Field is used to determine if these 2 fields are required:
        /// - PlannedDate_ApprovedSubmission
        /// - PlannedDate_WrittenApproval
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsPlannedDatesRequired
        {
            get
            {
                return this.CostAnalysis == ScheduleEvent.Planned || this.CID == ScheduleEvent.Planned;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance of cid text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsCIDTextRequired { get { return this.CID == ScheduleEvent.Planned && !this.CIDDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of PriceAnalysis text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsPriceAnalysisTextRequired { get { return this.PriceAnalysis == ScheduleEvent.Planned && !this.PriceAnalysisDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of TechnicalEvaluation text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsTechnicalEvaluationTextRequired { get { return this.TechnicalEvaluation == ScheduleEvent.Planned && !this.TechnicalEvaluationDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of FactFinding text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsFactFindingTextRequired { get { return this.FactFinding == ScheduleEvent.Planned && !this.FactFindingDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of CostAnalysis text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsCostAnalysisTextRequired { get { return this.CostAnalysis == ScheduleEvent.Planned && !this.CostAnalysisDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of GovtPricing text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsGovtPricingTextRequired { get { return this.GovtPricing == ScheduleEvent.Planned && !this.GovtPricingDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of GovtPricingReceived text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsGovtPricingReceivedTextRequired { get { return this.GovtPricingReceived == ScheduleEvent.Planned && !this.GovtPricingReceivedDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of CostAnalysisUnqual text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsCostAnalysisUnqualTextRequired { get { return this.CostAnalysisUnqual == ScheduleEvent.Planned && !this.CostAnalysisUnqualDate.HasValue; } }
        
        /// <summary>
        /// Gets a value indicating whether this instance of SupplierNegotiations text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsSupplierNegotiationsTextRequired { get { return this.SupplierNegotiations == ScheduleEvent.Planned && !this.SupplierNegotiationsDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of MOU text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsMOUTextRequired { get { return this.MOU == ScheduleEvent.Planned && !this.MOUDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of ShouldCostEstimate text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsShouldCostEstimateTextRequired { get { return this.ShouldCostEstimate == ScheduleEvent.Planned && !this.ShouldCostEstimateDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of RFPRelease text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsRFPReleaseTextRequired { get { return this.RFPRelease == ScheduleEvent.Planned && !this.RFPReleaseDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of FirmSupplierReceipt text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsFirmSupplierReceiptTextRequired { get { return this.FirmSupplierReceipt == ScheduleEvent.Planned && !this.FirmSupplierReceiptDate.HasValue; } }

        /// <summary>
        /// Gets a value indicating whether this instance of SourceSelection text is required.
        /// </summary>
        /// <remarks>
        /// This is public, but it needs to be to work w/the validation.
        /// Validation is the only thing this property is used for.
        /// </remarks>
        public bool IsSourceSelectionTextRequired { get { return this.SourceSelection == ScheduleEvent.Planned && !this.SourceSelectionDate.HasValue; } }
    }
}