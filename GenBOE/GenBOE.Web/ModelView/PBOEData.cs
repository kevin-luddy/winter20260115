// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using GenBOE.Dtos;
using IES.Common;

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// PBOE Data.
	/// </summary>
	public class PBOEData
	{
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
		public IList<string> SubResources { get; set; }

		/// <summary>
		/// LM Proposed Value for Procurement.
		/// </summary>
		public decimal TotalCost { get; set; }

		/// <summary>
		/// Value of Supplier proposal
		/// </summary>
		public decimal? SupplierProposedValue { get; set; }

		/// <summary>
		/// Expected Certified Cost or Pricing Data (CCoPD) Applicability
		/// </summary>
		public bool IsCCoPD { get; set; }

		/// <summary>
		/// Schedule of Events Price Analysis
		/// </summary>
		public ScheduleEvent PriceAnalysis { get; set; }

		/// <summary>
		/// Price Analysis Date
		/// </summary>
		[RequiredIf("PriceAnalysis", ScheduleEvent.Actual, ErrorMessage = "Price Analysis Date is required if Schedule Event is actual.")]
		public DateTime? PriceAnalysisDate { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis
		/// </summary>
		public ScheduleEvent CostAnalysis { get; set; }

		/// <summary>
		/// Cost Analysis Date
		/// </summary>
		[RequiredIf("CostAnalysis", ScheduleEvent.Actual, ErrorMessage = "Cost Analysis Date is required if Schedule Event is actual.")]
		public DateTime? CostAnalysisDate { get; set; }

		/// <summary>
		/// Schedule of Events Govt. Pricing Assistance for CCoPD Review - Receipt
		/// </summary>
		public ScheduleEvent GovtPricingReceived { get; set; }

		/// <summary>
		/// Govt. Pricing Assistance for CCoPD Review - Receipt Planned Date
		/// </summary>
		[RequiredIf("GovtPricingReceived", ScheduleEvent.Actual, ErrorMessage = "Govt Pricing Received Date is required if Govt Pricing Received is actual.")]
		public DateTime? GovtPricingReceivedDate { get; set; }

		/// <summary>
		/// Schedule of Events Cost Analysis Unqualified
		/// </summary>
		public ScheduleEvent CostAnalysisUnqualified { get; set; }
		
		/// <summary>
		/// Cost Analysis Unqualified Date
		/// </summary>
		[RequiredIf("CostAnalysisUnqualified", ScheduleEvent.Actual, ErrorMessage = "Cost Analysis Unqualified Date, is required if Cost Analysis is actual.")]
		public DateTime? CostAnalysisUnqualifiedDate { get; set; }
	}
}