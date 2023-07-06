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
		/// The selected Sub Resources
		/// </summary>
		public ICollection<ResourceDTO> SubResources { get; set; }

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
	}

}