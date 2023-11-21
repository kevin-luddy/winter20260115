// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
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
			this.SupplierName = null;
			this.VendorId = null;
			this.SubResources = null;
			this.TotalCost = null;
			this.SupplierProposedValue = null;
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
			this.LeadEstimatorDisplayName = null;
			this.LeadEstimatorEmail = null;
	}

		/// <summary>
		/// Gets or sets the PBoe ID.
		/// </summary>
		public int PBoeID { get; set; }

		/// <summary>
		/// Gets or sets the name of the supplier.
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// Vendor ID
		/// </summary>
		public string VendorId { get; set; }

		/// <summary>
		/// The selected Sub Resource Ids
		/// </summary>
		public IList<string> SubResources { get; set; }

		/// <summary>
		/// LM Proposed Value for Procurement.
		/// </summary>
		public decimal? TotalCost { get; set; }

		/// <summary>
		/// Value of Supplier proposal
		/// </summary>
		public decimal? SupplierProposedValue { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability
		/// </summary>
		public bool? IsCCoPD { get; set; }

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
		/// Schedule of Events Price Analysis
		/// </summary>
		public ScheduleEvent? PriceAnalysis { get; set; }

		/// <summary>
		/// Price Analysis Date
		/// </summary>
		public DateTime? PriceAnalysisDate { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis
		/// </summary>
		public ScheduleEvent? CostAnalysis { get; set; }

		/// <summary>
		/// Cost Analysis Date
		/// </summary>
		public DateTime? CostAnalysisDate { get; set; }

		/// <summary>
		/// Schedule of Events Govt. Pricing Assistance for CCoPD Review - Receipt
		/// </summary>
		public ScheduleEvent? GovtPricingReceived { get; set; }

		/// <summary>
		/// Govt. Pricing Assistance for CCoPD Review - Receipt Planned Date
		/// </summary>
		public DateTime? GovtPricingReceivedDate { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis Unqualified
		/// </summary>
		public ScheduleEvent? CostAnalysisUnqualified { get; set; }

		/// <summary>
		/// Cost Analysis Unqualified Date
		/// </summary>
		public DateTime? CostAnalysisUnqualifiedDate { get; set; }

		/// <summary>
		/// Technical Evaluation
		/// </summary>
		public ScheduleEvent? TechnicalEvaluation { get; set; }

		/// <summary>
		/// Technical Evaluation Date
		/// </summary>
		public DateTime? TechnicalEvaluationDate { get; set; }

		/// <summary>
		/// RFP Release to Supplier Date
		/// </summary>
		public DateTime? RFPReleaseToSupplierDate { get; set; }

		/// <summary>
		/// Supplier Negotiations Date
		/// </summary>
		public DateTime? SupplierNegotiationsDate { get; set; }

		/// <summary>
		/// Supplier Proposal Date
		/// </summary>
		public string ProposalDate { get; set; }

		/// <summary>
		/// Supplier Proposal Validity Date
		/// </summary>
		public string ValidityDate { get; set; }

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
		/// Proposal Title / Name
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Proposal Submittal Date
		/// </summary>
		public DateTime? ProposalSubmittalDate { get; set;}

		/// <summary>
		/// Contracts Lead Display Name
		/// </summary>
		public string ContractsLeadDisplayName { get; set; }

		/// <summary>
		/// Contracts Lead Email
		/// </summary>
		public string ContractsLeadEmail { get; set; }

		/// <summary>
		/// Lead Estimator Display Name
		/// </summary>
		public string LeadEstimatorDisplayName { get; set; }

		/// <summary>
		/// Lead Estimator Email
		/// </summary>
		public string LeadEstimatorEmail { get; set; }
	}
}