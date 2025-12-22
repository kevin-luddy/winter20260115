// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// PLD Proposal DTO 
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class PLDProposalDTO
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public PLDProposalDTO()
		{
			PANumber = string.Empty;
			Version = -1;
			Title = string.Empty;
			Description = string.Empty;
			LineOfBusiness = string.Empty;
			LineOfBusinessId = null;
			Pricing = string.Empty;
			ProjectStartDate = DateTime.MinValue;
			ProjectEndDate = DateTime.MinValue;
			ProposalSubmittalDate = DateTime.MinValue;
			RFPNumber = string.Empty;
			LastModifiedDate = DateTime.MinValue;
			ProposalStatus = string.Empty;
		}

		/// <summary>
		/// Line of Business ID
		/// </summary>
		public int? LineOfBusinessId { get; set; }

		/// <summary>
		///  PA Number 
		/// </summary>
		public string PANumber { get; set; }

		/// <summary>
		/// PA Version
		/// </summary>
		public short Version { get; set; }

		/// <summary>
		/// PA Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// PA Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///  Line of Business
		/// </summary>
		public string LineOfBusiness { get; set; }

		/// <summary>
		/// Price  - by Name
		/// </summary>
		public string Pricing { get; set; }

		/// <summary>
		///  Project Start Date
		/// </summary>
		public DateTime? ProjectStartDate { get; set; }

		/// <summary>
		/// Project End Date
		/// </summary>
		public DateTime? ProjectEndDate { get; set; }

		/// <summary>
		/// Proposal Submittal Date
		/// </summary>
		public DateTime? ProposalSubmittalDate { get; set; }

		/// <summary>
		/// Proposal Submittal Date as a string for the UI
		/// </summary>
		public string ProposalSubmittalDateString => ProposalSubmittalDate.HasValue && ProposalSubmittalDate.Value != DateTime.MinValue ? ProposalSubmittalDate.Value.ToString("MM/dd/yyyy") : string.Empty;

		/// <summary>
		///  RFP Number
		/// </summary>
		public string RFPNumber { get; set; }

		/// <summary>
		/// Last Modified Date
		/// </summary>
		public DateTime? LastModifiedDate { get; set; }

		/// <summary>
		/// Last Modified Date as a string for the UI
		/// </summary>
		public string LastModifiedDateString => LastModifiedDate.HasValue ? LastModifiedDate.Value.ToString("MM/dd/yyyy") : "N/A";

		/// <summary>
		/// Proposal Status
		/// </summary>
		public string ProposalStatus { get; set; }
	}
}
