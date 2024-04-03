// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.DTO.Proposal
{
	using System;

	/// <summary>
	/// ACV Header data pulled from PTM
	/// </summary>
	public class AcvHeaderDataDto
	{
		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string PtmTrackingNumber { get; set; }

		/// <summary>
		/// Proposal Title
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Proposal Status
		/// </summary>
		public string ProposalStatus { get; set; }

		/// <summary>
		/// RFP Number
		/// </summary>
		public string RfpNumber { get; set; }

		/// <summary>
		/// Cost Volume Submittal Date
		/// If CCoPD is true, the Date Submission field from ACV will be used
		/// If CCoPD is false, then Revised Anticipated Delivery Date will be use if available, otherwise Anticipated Delivery Date
		/// </summary>
		public DateTime? CostVolumeSubmittalDate { get; set; }
	}
}
