// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// The PBOE INL Form DTO.
    /// </summary>
    /// <seealso cref="GenBOE.Dtos.BOEFormDTO" />
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BOEFormPBOEDTO : BOEFormDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormPBOEDTO"/> class.
        /// </summary>
        public BOEFormPBOEDTO() : base()
        { 
        }

        /// <summary>
        /// The BOE Form type.
        /// </summary>
        public override BOEFormType BOEFormType
        {
            get
            {
                return BOEFormType.PBOE;
            }
        }        

        /// <summary>
        /// Gets or sets a value indicating whether CCoPD applies.
        /// </summary>
        public bool CCoPDApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether commercial item exception applies.
        /// </summary>
        public bool CommercialItemExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether competition exception applies.
        /// </summary>
        public bool CompetitionExceptionApplies { get; set; }

        /// <summary>
        /// Gets or set a value indicating whether less than threshold applies
        /// </summary>
        public bool LessThanThresholdExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is an other exception that applies.
        /// </summary>
        public bool OtherExceptionApplies { get; set; }

        /// <summary>
        /// Gets or sets the other text, used when there is an other exception that applies.
        /// </summary>
        public string OtherText { get; set; }

        /// <summary>
        /// Gets or sets the RFP.
        /// </summary>
        public string RFP { get; set; }

        /// <summary>
        /// Gets or sets the proposal number.
        /// </summary>
        public string ProposalNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the supplier.
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// Gets or sets the validity date.
        /// </summary>
        public string ValidityDate { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Value of Supplier proposal
        /// </summary>
        public decimal? SupplierProposedValue { get; set; }

        /// <summary>
        /// Should Cost/Engineering Estimate
        /// </summary>
        public ScheduleEvent ShouldCostEstimate { get; set; }
        public DateTime? ShouldCostEstimateDate { get; set; }
        public string ShouldCostEstimateText { get; set; }

        /// <summary>
        /// RFP Release to Supplier(s)
        /// </summary>
        public ScheduleEvent RFPRelease { get; set; }
        public DateTime? RFPReleaseDate { get; set; }
        public string RFPReleaseText { get; set; }

        /// <summary>
        /// Firm Supplier proposal(s) Receipt
        /// </summary>
        public ScheduleEvent FirmSupplierReceipt { get; set; }
        public DateTime? FirmSupplierReceiptDate { get; set; }
        public string FirmSupplierReceiptText { get; set; }

        /// <summary>
        /// Source Selection
        /// </summary>
        public ScheduleEvent SourceSelection { get; set; }
        public DateTime? SourceSelectionDate { get; set; }
        public string SourceSelectionText { get; set; }
        
        /// <summary>
        /// Commercial item Documentation (CID)
        /// </summary>
        public ScheduleEvent CID { get; set; }
        public DateTime? CIDDate { get; set; }
        public string CIDText { get; set; }
        
        /// <summary>
        /// Price Analysis
        /// </summary>
        public ScheduleEvent PriceAnalysis { get; set; }
        public DateTime? PriceAnalysisDate { get; set; }
        public string PriceAnalysisText { get; set; }
        
        /// <summary>
        /// Technical Evaluation
        /// </summary>
        public ScheduleEvent TechnicalEvaluation { get; set; }
        public DateTime? TechnicalEvaluationDate { get; set; }
        public string TechnicalEvaluationText { get; set; }
        
        /// <summary>
        /// Fact Finding
        /// </summary>
        public ScheduleEvent FactFinding { get; set; }
        public DateTime? FactFindingDate { get; set; }
        public string FactFindingText { get; set; }
        
        /// <summary>
        /// Cost Analysis
        /// </summary>
        public ScheduleEvent CostAnalysis { get; set; }
        public DateTime? CostAnalysisDate { get; set; }
        public string CostAnalysisText { get; set; }
        
        /// <summary>
        /// Govt. Pricing Assistance for CCoPD Review Requested by LM
        /// </summary>
        public ScheduleEvent GovtPricing { get; set; }
        public DateTime? GovtPricingDate { get; set; }
        public string GovtPricingText { get; set; }

        /// <summary>
        /// Govt Pricing Received for CCoPD Review
        /// </summary>
        public ScheduleEvent? GovtPricingReceived { get; set; }
        public DateTime? GovtPricingReceivedDate { get; set; }
        public string GovtPricingReceivedText { get; set; }

        /// <summary>
        /// Cost Analysis - Unqualified (Final)
        /// </summary>
        public ScheduleEvent? CostAnalysisUnqual { get; set; }
        public DateTime? CostAnalysisUnqualDate { get; set; }
        public string CostAnalysisUnqualText { get; set; }

        /// <summary>
        /// Supplier Negotiations Complete
        /// </summary>
        public ScheduleEvent SupplierNegotiations { get; set; }
        public DateTime? SupplierNegotiationsDate { get; set; }
        public string SupplierNegotiationsText { get; set; }
       
        /// <summary>
        /// Memorandum of Understanding
        /// </summary>
        public ScheduleEvent MOU { get; set; }
        public DateTime? MOUDate { get; set; }
        public string MOUText { get; set; }
       
        /// <summary>
        /// Date of customer written approval for submission after initial prime proposal.
        /// </summary>
        public DateTime? PlannedDate_WrittenApproval { get; set; }
        
        /// <summary>
        /// Approved date for submission to customer.
        /// </summary>
        public DateTime? PlannedDate_ApprovedSubmission { get; set; }

        /// <summary>
        /// Supplier CCoPD Applies
        /// </summary>
        public TripleBooleanState? SupplierCCoPD { get; set; }

        /// <summary>
        /// Source Selection Summary/Description
        /// </summary>
        public string SourceSelectionDescription { get; set; }

        /// <summary>
        /// Commerciality Summary/Description
        /// </summary>
        public string CommercialityDescription { get; set; }

        /// <summary>
        /// Technical Evaluation Summary/Description
        /// </summary>
        public string TechnicalEvaluationDescription { get; set; }

        /// <summary>
        /// Price Analysis Summary/Description
        /// </summary>
        public string PriceAnalysisDescription { get; set; }

        /// <summary>
        /// Cost Analysis Summary/Description
        /// </summary>
        public string CostAnalysisDescription { get; set; }

        /// <summary>
        /// Rationale for LM Proposed Value Summary
        /// </summary>
        public string RationaleValueSummary { get; set; }
    }
}
