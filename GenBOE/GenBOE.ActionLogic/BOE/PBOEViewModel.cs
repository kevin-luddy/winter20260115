// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using System;
	using System.Collections.Generic;
	using IES.Common;

	/// <summary>
	/// View Model for PBOE
	/// </summary>
	public class PBOEViewModel
	{
		/// <summary>
		/// Id - PK, Identity
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Update Date
		/// </summary>
		public DateTime UpdateDT { get; set; }

		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string PTMTrackingNumber { get; set; }

		/// <summary>
		/// Form Name
		/// </summary>
		public string FormName { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Basis and Rationale
		/// </summary>
		public string BasisAndRationale { get; set; }

		/// <summary>
		/// Proposal Title
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Proposal Date
		/// </summary>
		public string ProposalDate { get; set; }

		/// <summary>
		/// Point of Contact
		/// </summary>
		public string Poc { get; set; }

		/// <summary>
		/// Point of Contact Phone
		/// </summary>
		public string PocPhone { get; set; }

		/// <summary>
		/// Point of Contact Email
		/// </summary>
		public string PocEmail { get; set; }

		/// <summary>
		/// Approver
		/// </summary>
		public string Approver { get; set; }

		/// <summary>
		/// Approver Phone
		/// </summary>
		public string ApproverPhone { get; set; }

		/// <summary>
		/// Approver Email
		/// </summary>
		public string ApproverEmail { get; set; }

		/// <summary>
		/// Selected Sub Resources
		/// </summary>
		public ICollection<string> Resources { get; set; }

		/// <summary>
		/// Revision
		/// </summary>
		public int? Revision { get; set; }

		/// <summary>
		/// Form Version
		/// </summary>
		public int FormVersion { get; set; }

		/// <summary>
		/// CCoPD
		/// </summary>
		public ExpectedCCoPDApplicability? CCoPD { get; set; }

		/// <summary>
		/// CCoPD Other Text
		/// </summary>
		public string CCoPDOtherText { get; set; }

		/// <summary>
		/// RFP
		/// </summary>
		public string RFP { get; set; }

		/// <summary>
		/// Proposal Number
		/// </summary>
		public string ProposalNumber { get; set; }

		/// <summary>
		/// Supplier Name
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// Validity Date
		/// </summary>
		public string ValidityDate { get; set; }

		/// <summary>
		/// Supplier Proposal Supporting Data Included
		/// </summary>
		public int? SupplierProposalSupportingDataIncluded { get; set; }

		/// <summary>
		/// Price Analysis Included
		/// </summary>
		public int? PriceAnalysisIncluded { get; set; }

		/// <summary>
		/// Commercial Item Doc Included
		/// </summary>
		public int? CommercialItemDocIncluded { get; set; }

		/// <summary>
		/// Cost Analysis Included
		/// </summary>
		public int? CostAnalysisIncluded { get; set; }

		#region Schedule of Events

		/// <summary>
		/// Should Cost/Engineering Estimate
		/// </summary>
		public ScheduleEvent ShouldCostEstimate { get; set; }

		/// <summary>
		/// Should Cost Estimate Date
		/// </summary>
		public Nullable<DateTime> ShouldCostEstimateDate { get; set; }

		/// <summary>
		/// Should Cost Estimate Text
		/// </summary>
		public string ShouldCostEstimateText { get; set; }

		/// <summary>
		/// RFP Release to Supplier(s)
		/// </summary>
		public ScheduleEvent RFPRelease { get; set; }

		/// <summary>
		/// RFP Release Date
		/// </summary>
		public Nullable<DateTime> RFPReleaseDate { get; set; }

		/// <summary>
		/// RFP Release Text
		/// </summary>
		public string RFPReleaseText { get; set; }

		/// <summary>
		/// Firm Supplier Receipt
		/// </summary>
		public ScheduleEvent FirmSupplierReceipt { get; set; }

		/// <summary>
		/// Firm Supplier Receipt Date
		/// </summary>
		public Nullable<DateTime> FirmSupplierReceiptDate { get; set; }

		/// <summary>
		/// Firm Supplier Receipt Text
		/// </summary>
		public string FirmSupplierReceiptText { get; set; }

		/// <summary>
		/// Source Selection
		/// </summary>
		public ScheduleEvent SourceSelection { get; set; }

		/// <summary>
		/// Source Selection Date
		/// </summary>
		public Nullable<DateTime> SourceSelectionDate { get; set; }

		/// <summary>
		/// Source Selection Text
		/// </summary>
		public string SourceSelectionText { get; set; }

		/// <summary>
		/// CID
		/// </summary>
		public ScheduleEvent CID { get; set; }

		/// <summary>
		/// CID Date
		/// </summary>
		public Nullable<DateTime> CIDDate { get; set; }

		/// <summary>
		/// CID Text
		/// </summary>
		public string CIDText { get; set; }

		/// <summary>
		/// Price Analysis
		/// </summary>
		public ScheduleEvent PriceAnalysis { get; set; }

		/// <summary>
		/// Price Analysis Date
		/// </summary>
		public Nullable<DateTime> PriceAnalysisDate { get; set; }

		/// <summary>
		/// Price Analysis Text
		/// </summary>
		public string PriceAnalysisText { get; set; }

		/// <summary>
		/// Technical Evaluation
		/// </summary>
		public ScheduleEvent TechnicalEvaluation { get; set; }

		/// <summary>
		/// Technical Evaluation Date
		/// </summary>
		public Nullable<DateTime> TechnicalEvaluationDate { get; set; }

		/// <summary>
		/// Technical Evaluation Text
		/// </summary>
		public string TechnicalEvaluationText { get; set; }

		/// <summary>
		/// Fact Finding
		/// </summary>
		public ScheduleEvent FactFinding { get; set; }

		/// <summary>
		/// Fact Finding Date
		/// </summary>
		public Nullable<DateTime> FactFindingDate { get; set; }

		/// <summary>
		/// Fact Finding Text
		/// </summary>
		public string FactFindingText { get; set; }

		/// <summary>
		/// Cost Analysis
		/// </summary>
		public ScheduleEvent CostAnalysis { get; set; }

		/// <summary>
		/// Cost Analysis Date
		/// </summary>
		public Nullable<DateTime> CostAnalysisDate { get; set; }

		/// <summary>
		/// Cost Analysis Text
		/// </summary>
		public string CostAnalysisText { get; set; }

		/// <summary>
		/// Govt Pricing
		/// </summary>
		public ScheduleEvent GovtPricing { get; set; }

		/// <summary>
		/// Govt Pricing Date
		/// </summary>
		public Nullable<DateTime> GovtPricingDate { get; set; }

		/// <summary>
		/// Govt Pricing Text
		/// </summary>
		public string GovtPricingText { get; set; }

		/// <summary>
		/// Supplier Negotiations
		/// </summary>
		public ScheduleEvent SupplierNegotiations { get; set; }

		/// <summary>
		/// Supplier Negotiations
		/// </summary>
		public Nullable<DateTime> SupplierNegotiationsDate { get; set; }

		/// <summary>
		/// Supplier Negotiations
		/// </summary>
		public string SupplierNegotiationsText { get; set; }

		/// <summary>
		/// MOU
		/// </summary>
		public ScheduleEvent MOU { get; set; }

		/// <summary>
		/// MOU Date
		/// </summary>
		public Nullable<DateTime> MOUDate { get; set; }

		/// <summary>
		/// MOU Text
		/// </summary>
		public string MOUText { get; set; }

		/// <summary>
		/// Govt Pricing Received
		/// </summary>
		public ScheduleEvent GovtPricingReceived { get; set; }

		/// <summary>
		/// Govt Pricing Received Date
		/// </summary>
		public Nullable<DateTime> GovtPricingReceivedDate { get; set; }

		/// <summary>
		/// Govt Pricing Received Text
		/// </summary>
		public string GovtPricingReceivedText { get; set; }

		/// <summary>
		/// Cost Analysis Unqual
		/// </summary>
		public ScheduleEvent CostAnalysisUnqual { get; set; }

		/// <summary>
		/// Cost Analysis Unqual Date
		/// </summary>
		public Nullable<DateTime> CostAnalysisUnqualDate { get; set; }

		/// <summary>
		/// Cost Analysis Unqual Text
		/// </summary>
		public string CostAnalysisUnqualText { get; set; }

		#endregion Schedule of Events

		/// <summary>
		/// Planned Date Written Approval
		/// </summary>
		public Nullable<DateTime> PlannedDate_WrittenApproval { get; set; }

		/// <summary>
		/// Planned Date Approved Submission
		/// </summary>
		public Nullable<DateTime> PlannedDate_ApprovedSubmission { get; set; }

		/// <summary>
		/// Supplier CCoPD
		/// </summary>
		public TripleBooleanState? SupplierCCoPD { get; set; }

		#region Basis Of and Rationale

		/// <summary>
		/// Source Selection Description
		/// </summary>
		public string SourceSelectionDescription { get; set; }

		/// <summary>
		/// Commerciality Description
		/// </summary>
		public string CommercialityDescription { get; set; }

		/// <summary>
		/// Technical Evaluation Description
		/// </summary>
		public string TechnicalEvaluationDescription { get; set; }

		/// <summary>
		/// Price Analysis Description
		/// </summary>
		public string PriceAnalysisDescription { get; set; }

		/// <summary>
		/// Cost Analysis Description
		/// </summary>
		public string CostAnalysisDescription { get; set; }

		/// <summary>
		/// Rationale Value Summary
		/// </summary>
		public string RationaleValueSummary { get; set; }

		#endregion Basis Of and Rationale

		/// <summary>
		/// Vendor Id
		/// </summary>
		public string VendorId { get; set; }

		/// <summary>
		/// Supplier Proposed Value
		/// </summary>
		public Nullable<decimal> SupplierProposedValue { get; set; }

		/// <summary>
		/// List of PboeClinContractXREFs
		/// </summary>
		public List<PBOECLINContractXREFData> PboeClinContractXREF { get; set; } = new List<PBOECLINContractXREFData>();

		/// <summary>
		/// Check if user has only read only access
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Checks to see if the PBOE is complete with validation.
		/// </summary>
		public bool IsComplete { get; set; }

		/// <summary>
		/// The Supplier Period of Performance
		/// </summary>
		public string SupplierPOP { get; set; }

		/// <summary>
		/// Clin/WBS data is populated in GenBoe 
		/// </summary>
		public ICollection<PBOEClinData> PboeClinData { get; set; } = new List<PBOEClinData>();

		/// <summary>
		/// CCopd applies condition met
		/// </summary>
		public string SupplierCCoPDProposalValueConditionMetText { get; set; }

	}
}

