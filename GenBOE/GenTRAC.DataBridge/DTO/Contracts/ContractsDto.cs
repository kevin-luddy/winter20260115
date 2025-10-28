// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using IES.Common;

	/// <summary>
	/// Contracts DTO
	/// </summary>
	[Serializable]
	public class ContractsDto : IES.Common.UpdateableDTO
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContractsDto"/> class.
		/// </summary>
		public ContractsDto()
		{
			this.Id = -1;
		}

		/// <summary>
		/// Gets or sets the Proposal Id that this Contract is linked to.
		/// </summary>
		public int ProposalId { get; set; }

		/// <summary>
		/// Previously Submitted ROM (PTM record)
		/// </summary>
		public int? PreviouslySubmittedROM { get; set; }

		/// <summary>
		/// Customer Submittal Date string
		/// </summary>
		public DateTime? CustomerDueDate { get; set; }

		/// <summary>
		/// Customer Submittal Date string
		/// </summary>
		public DateTime? CustomerSubmittalDate { get; set; }

		/// <summary>
		/// Cage #
		/// </summary>
		public string CageCode { get; set; }

		/// <summary>
		/// Contracts Correspondence Log Number
		/// </summary>
		public string ContractsCorrespondenceLogNumber { get; set; }

		/// <summary>
		/// Final Negotiated Value string
		/// </summary>
		public long? FinalNegotiatedValue { get; set; }

		/// <summary>
		/// Date Confirmation of Negotiations Submitted string
		/// </summary>
		public DateTime? NegotiationsSubmitted { get; set; }

		/// <summary>
		/// EPP Delegation Authority ID
		/// </summary>
		public int? EppDelegationAuthority { get; set; }

		/// <summary>
		/// Planned Bid EPP Date
		/// </summary>
		public DateTime? PlannedBidEppDate { get; set; }

		/// <summary>
		/// Scheduled Bid EPP Date
		/// </summary>
		public DateTime? ScheduledBidEppDate { get; set; }

		/// <summary>
		/// Planned Program EPP Date
		/// </summary>
		public DateTime? PlannedProgramEppDate { get; set; }

		/// <summary>
		/// Scheduled Program EPP Date
		/// </summary>
		public DateTime? ScheduledProgramEppDate { get; set; }

		/// <summary>
		/// Planned Mission Segment EPP Date
		/// </summary>
		public DateTime? PlannedMissionSegmentEppDate { get; set; }

		/// <summary>
		/// Scheduled Mission Segment EPP Date
		/// </summary>
		public DateTime? ScheduledMissionSegmentEppDate { get; set; }

		/// <summary>
		/// Planned LOB EPP Date
		/// </summary>
		public DateTime? PlannedLobEppDate { get; set; }

		/// <summary>
		/// Scheduled LOB EPP Date
		/// </summary>
		public DateTime? ScheduledLobEppDate { get; set; }

		/// <summary>
		/// Planned Pre Space EPP Date
		/// </summary>
		public DateTime? PlannedPreSpaceEppDate { get; set; }

		/// <summary>
		/// Scheduled Pre Space EPP Date
		/// </summary>
		public DateTime? ScheduledPreSpaceEppDate { get; set; }

		/// <summary>
		/// Planned Space EPP Date
		/// </summary>
		public DateTime? PlannedSpaceEppDate { get; set; }

		/// <summary>
		/// Scheduled Space EPP Date
		/// </summary>
		public DateTime? ScheduledSpaceEppDate { get; set; }

		/// <summary>
		/// Planned Pre Corporate EPP Date
		/// </summary>
		public DateTime? PlannedPreCorporateEppDate { get; set; }

		/// <summary>
		/// Scheduled Pre Corporate EPP Date
		/// </summary>
		public DateTime? ScheduledPreCorporateEppDate { get; set; }

		/// <summary>
		/// Planned Corporate Epp Date
		/// </summary>
		public DateTime? PlannedCorporateEppDate { get; set; }

		/// <summary>
		/// Scheduled Corporate Epp Date
		/// </summary>
		public DateTime? ScheduledCorporateEppDate { get; set; }

		/// <summary>
		/// EPP ROS Delegation Notes
		/// </summary>
		public string EppRosDelegationNotes { get; set; }

		/// <summary>
		/// LM Won contract
		/// </summary>
		public bool? LmWon { get; set; }

		/// <summary>
		/// Mod Completed Date
		/// </summary>
		public DateTime? ModCompletedDate { get; set; }

		/// <summary>
		/// Is Insurance Direct
		/// </summary>
		public TripleBooleanState? IsInsuranceDirect { get; set; }

		/// <summary>
		/// Insurance Type
		/// </summary>
		public InsuranceType? InsuranceType { get; set; }

		/// <summary>
		/// Proposed Insurance Value
		/// </summary>
		public long? ProposedInsurance { get; set; }

		/// <summary>
		/// Negotiated Insurance Value
		/// </summary>
		public long? NegotiatedInsurance { get; set; }
	}
}
