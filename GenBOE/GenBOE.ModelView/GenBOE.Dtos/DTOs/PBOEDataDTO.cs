// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;

	/// <summary>
	/// DTO intended to show PBOE data.
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable]
	public class PBOEDataDTO
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public PBOEDataDTO()
		{
			this.PBoeID = -1;
			this.WorkspaceId = -1;
			this.WorkspaceName = null;
			this.SupplierName = null;
			this.VendorId = null;
			this.SubResources = null;
			this.TotalCost = null;
			this.SupplierProposedValue = null;
			this.BoeCcopd = 0;
			this.IsCCoPD = null;
			this.PriceAnalysis = null;
			this.PriceAnalysisDate = null;
			this.CostAnalysis = null;
			this.CostAnalysisDate = null;
			this.GovtPricingReceived = null;
			this.GovtPricingReceivedDate = null;
			this.CostAnalysisUnqualified = null;
			this.CostAnalysisUnqualifiedDate = null;
			this.IsCommercialItemException = null;
			this.IsCompetitionException = null;
			this.IsCCoPDOtherException = null;
			this.IsCCoPDThresholdException = null;
			this.TechnicalEvaluation = null;
			this.TechnicalEvaluationDate = null;
			this.RFPReleaseToSupplierDate = null;
			this.SupplierNegotiationsDate = null;
			this.ProposalDate = null;
			this.ValidityDate = null;
			this.Approver = null;
			this.SupplierProposalManagerDisplayName = null;
			this.SupplierProposalManagerEmail = null;
			this.ProposalTitle = null;
			this.ProposalSubmittalDate = null;
			this.ContractsLeadDisplayName = null;
			this.ContractsLeadEmail = null;
			this.LeadEstimatorId = 0;
			this.LeadEstimatorDisplayName = null;
			this.LeadEstimatorEmail = null;
			this.TrackingNumber = null;
			ClinContractXrefs = new Collection<ClinContractDto>();
		}

		/// <summary>
		/// Gets or sets the PBoe ID.
		/// </summary>
		public int PBoeID { get; set; }

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
		/// Gets or sets the Workspace ID
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// Gets or sets the Workspace Name
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Gets or sets the name of the supplier.
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// Vendor ID
		/// </summary>
		public string VendorId { get; set; }

		/// <summary>
		/// LM Supplier RFP Number
		/// </summary>
		public string RFP { get; set; }

		/// <summary>
		/// Proposal Number
		/// </summary>
		public string ProposalNumber { get; set; }

		/// <summary>
		/// The selected Sub Resource Ids
		/// </summary>
		public IList<string> SubResources { get; set; }

		/// <summary>
		/// Revision
		/// </summary>
		public int Revision { get; set; }

		/// <summary>
		/// Form Version
		/// </summary>
		public int FormVersion { get; set; }

		/// <summary>
		/// LM Proposed Value for Procurement.
		/// </summary>
		public decimal? TotalCost { get; set; }

		/// <summary>
		/// Value of Supplier proposal
		/// </summary>
		public decimal? SupplierProposedValue { get; set; }

		/// <summary>
		/// CCoPD Value Description with BOE enum
		/// </summary>
		public Ccopd BoeCcopd { get; set; }

		/// <summary>
		/// CCoPD Value Description with NLF enum
		/// </summary>
		public ExpectedCCoPDApplicability CCoPD
		{
			get
			{
				switch (BoeCcopd)
				{
					case Ccopd.None:
						return ExpectedCCoPDApplicability.Blank;
					case Ccopd.Applies:
						return ExpectedCCoPDApplicability.CCoPDApplies;
					case Ccopd.Commercial:
						return ExpectedCCoPDApplicability.CommercialItemExceptionApplies;
					case Ccopd.Competition:
						return ExpectedCCoPDApplicability.CompetitionExceptionApplies;
					case Ccopd.Threshold:
						return ExpectedCCoPDApplicability.ThresholdExceptionApplies;
					case Ccopd.Other:
						return ExpectedCCoPDApplicability.OtherExceptionApplies;
					default:
						return ExpectedCCoPDApplicability.Blank;
				}
			}
		}

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability
		/// </summary>
		public bool? IsCCoPD { get; set; }

		/// <summary>
		/// CCoPD Other Text
		/// </summary>
		public string CCoPDOtherText { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability and Commercial Item Expcetion
		/// </summary>
		public bool? IsCommercialItemException { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability and Competition Exception
		/// </summary>
		public bool? IsCompetitionException { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability with other Exception
		/// </summary>
		public bool? IsCCoPDOtherException { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability with CCoPD Threshold Exception
		/// </summary>
		public bool? IsCCoPDThresholdException { get; set; }

		/// <summary>
		/// Should Cost/Engineering Estimate
		/// </summary>
		public ScheduleEvent? ShouldCostEstimate { get; set; }

		/// <summary>
		/// Should Cost Estimate Date
		/// </summary>
		public DateTime? ShouldCostEstimateDate { get; set; }

		/// <summary>
		/// Should Cost Estimate Text
		/// </summary>
		public string ShouldCostEstimateText { get; set; }

		/// <summary>
		/// Sow Written
		/// </summary>
		public ScheduleEvent? SowWritten { get; set; }

		/// <summary>
		/// Sow Written Date
		/// </summary>
		public DateTime? SowWrittenDate { get; set; }

		/// <summary>
		/// Sow Written Text
		/// </summary>
		public string SowWrittenText { get; set; }

		/// <summary>
		/// Firm Supplier Receipt
		/// </summary>
		public ScheduleEvent? FirmSupplierReceipt { get; set; }

		/// <summary>
		/// Firm Supplier Receipt Date
		/// </summary>
		public DateTime? FirmSupplierReceiptDate { get; set; }

		/// <summary>
		/// Firm Supplier Receipt Text
		/// </summary>
		public string FirmSupplierReceiptText { get; set; }

		/// <summary>
		/// Source Selection
		/// </summary>
		public ScheduleEvent? SourceSelection { get; set; }

		/// <summary>
		/// Source Selection Date
		/// </summary>
		public DateTime? SourceSelectionDate { get; set; }

		/// <summary>
		/// Source Selection Text
		/// </summary>
		public string SourceSelectionText { get; set; }

		/// <summary>
		/// CID
		/// </summary>
		public ScheduleEvent? CID { get; set; }

		/// <summary>
		/// CID Date
		/// </summary>
		public DateTime? CIDDate { get; set; }

		/// <summary>
		/// CID Text
		/// </summary>
		public string CIDText { get; set; }

		/// <summary>
		/// Govt Review
		/// </summary>
		public ScheduleEvent? GovtReview { get; set; }

		/// <summary>
		/// Govt Review Date
		/// </summary>
		public DateTime? GovtReviewDate { get; set; }

		/// <summary>
		/// Govt Review Text
		/// </summary>
		public string GovtReviewText { get; set; }

		/// <summary>
		/// Schedule of Events Price Analysis
		/// </summary>
		public ScheduleEvent? PriceAnalysis { get; set; }

		/// <summary>
		/// Price Analysis Date
		/// </summary>
		public DateTime? PriceAnalysisDate { get; set; }

		/// <summary>
		/// Price Analysis Text
		/// </summary>
		public string PriceAnalysisText { get; set; }

		/// <summary>
		/// Fact Finding
		/// </summary>
		public ScheduleEvent? FactFinding { get; set; }

		/// <summary>
		/// Fact Finding Date
		/// </summary>
		public DateTime? FactFindingDate { get; set; }

		/// <summary>
		/// Fact Finding Text
		/// </summary>
		public string FactFindingText { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis
		/// </summary>
		public ScheduleEvent? CostAnalysis { get; set; }

		/// <summary>
		/// Cost Analysis Date
		/// </summary>
		public DateTime? CostAnalysisDate { get; set; }

		/// <summary>
		/// Cost Analysis Text
		/// </summary>
		public string CostAnalysisText { get; set; }

		/// <summary>
		/// Govt Pricing
		/// </summary>
		public ScheduleEvent? GovtPricing { get; set; }

		/// <summary>
		/// Govt Pricing Date
		/// </summary>
		public DateTime? GovtPricingDate { get; set; }

		/// <summary>
		/// Govt Pricing Text
		/// </summary>
		public string GovtPricingText { get; set; }

		/// <summary>
		/// Schedule of Events Govt. Pricing Assistance for CCoPD Review - Receipt
		/// </summary>
		public ScheduleEvent? GovtPricingReceived { get; set; }

		/// <summary>
		/// Govt. Pricing Assistance for CCoPD Review - Receipt Planned Date
		/// </summary>
		public DateTime? GovtPricingReceivedDate { get; set; }

		/// <summary>
		/// Govt Pricing Text
		/// </summary>
		public string GovtPricingReceivedText { get; set; }

		/// <summary>
		/// MOU
		/// </summary>
		public ScheduleEvent? MOU { get; set; }

		/// <summary>
		/// MOU Date
		/// </summary>
		public DateTime? MOUDate { get; set; }

		/// <summary>
		/// MOU Text
		/// </summary>
		public string MOUText { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis Unqualified
		/// </summary>
		public ScheduleEvent? CostAnalysisUnqualified { get; set; }

		/// <summary>
		/// Cost Analysis Unqualified Date
		/// </summary>
		public DateTime? CostAnalysisUnqualifiedDate { get; set; }

		/// <summary>
		/// Cost Analysis Unqualified Text
		/// </summary>
		public string CostAnalysisUnqualifiedText { get; set; }

		/// <summary>
		/// Technical Evaluation
		/// </summary>
		public ScheduleEvent? TechnicalEvaluation { get; set; }

		/// <summary>
		/// Technical Evaluation Date
		/// </summary>
		public DateTime? TechnicalEvaluationDate { get; set; }

		/// <summary>
		/// Technical Evaluation Text
		/// </summary>
		public string TechnicalEvaluationText { get; set; }

		/// <summary>
		/// RFP Release to Supplier(s)
		/// </summary>
		public ScheduleEvent? RFPRelease { get; set; }

		/// <summary>
		/// RFP Release to Supplier Date
		/// </summary>
		public DateTime? RFPReleaseToSupplierDate { get; set; }

		/// <summary>
		/// RFP Release Text
		/// </summary>
		public string RFPReleaseText { get; set; }

		/// <summary>
		/// Supplier Negotiations
		/// </summary>
		public ScheduleEvent? SupplierNegotiations { get; set; }

		/// <summary>
		/// Supplier Negotiations Date
		/// </summary>
		public DateTime? SupplierNegotiationsDate { get; set; }

		/// <summary>
		/// Supplier Negotiations
		/// </summary>
		public string SupplierNegotiationsText { get; set; }

		/// <summary>
		/// Supplier Proposal Date
		/// </summary>
		public string ProposalDate { get; set; }

		/// <summary>
		/// Supplier Proposal Validity Date
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

		/// <summary>
		/// Date of Agreement on Final Price (Handshake)
		/// </summary>
		public DateTime? AgreementDate { get; set; }

		/// <summary>
		/// Approver => Supplier Proposal Manager
		/// </summary>
		public string Approver { get; set; }

		/// <summary>
		/// Approver Display Name (Supplier Proposal Manager)
		/// </summary>
		public string SupplierProposalManagerDisplayName { get; set; }

		/// <summary>
		/// Approver Email (Supplier Proposal Manager)
		/// </summary>
		public string SupplierProposalManagerEmail { get; set; }

		/// <summary>
		/// Approver Phone (Supplier Proposal Manager)
		/// </summary>
		public string SupplierProposalManagerPhone { get; set; }

		/// <summary>
		/// Proposal Title / Name
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Workspace Proposal Title - used when there is no PBOE Proposal Title
		/// </summary>
		public string WorkspaceProposalTitle { get; set; }

		/// <summary>
		/// Workspace RFP Number - used when there is no PBOE Proposal Title
		/// </summary>
		public string WorkspaceRfpNumber { get; set; }

		/// <summary>
		/// Proposal Submittal Date
		/// </summary>
		public DateTime? ProposalSubmittalDate { get; set; }

		/// <summary>
		/// Contracts Lead Display Name
		/// </summary>
		public string ContractsLeadDisplayName { get; set; }

		/// <summary>
		/// Contracts Lead Email
		/// </summary>
		public string ContractsLeadEmail { get; set; }

		/// <summary>
		/// Contract Leads Phone
		/// </summary>
		public string ContractsLeadPhone { get; set; }

		/// <summary>
		/// Lead Estimator Id
		/// </summary>
		public int LeadEstimatorId { get; set; }

		/// <summary>
		/// Lead Estimator Display Name
		/// </summary>
		public string LeadEstimatorDisplayName { get; set; }

		/// <summary>
		/// Lead Estimator Email
		/// </summary>
		public string LeadEstimatorEmail { get; set; }

		/// <summary>
		/// Tracking Number
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// Planned Date Written Approval
		/// </summary>
		public DateTime? PlannedDateWrittenApproval { get; set; }

		/// <summary>
		/// Planned Date Approved Submission
		/// </summary>
		public DateTime? PlannedDateApprovedSubmission { get; set; }

		/// <summary>
		/// Supplier CCoPD
		/// </summary>
		public TripleBooleanState? SupplierCCoPD { get; set; }

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

		/// <summary>
		/// Collection of clin-contract xrefs
		/// </summary>
		public ICollection<ClinContractDto> ClinContractXrefs { get; set; }
	}
}