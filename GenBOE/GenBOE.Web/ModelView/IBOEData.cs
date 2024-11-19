// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using GenBOE.Dtos;

	/// <summary>
	/// IBOE Data to send to NLF
	/// </summary>
	public class IBOEData
	{
		/// <summary>
		/// IBOE ID
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
		/// Revision
		/// </summary>
		public int Revision { get; set; }

		/// <summary>
		/// Form Version
		/// </summary>
		public int FormVersion { get; set; }

		/// <summary>
		/// Resource List
		/// </summary>
		public ICollection<string> Resources { get; set; }

		/// <summary>
		/// Collection of IBOEClinContractXREFs
		/// </summary>
		public ICollection<ClinContractDto> IBOEClinContractXREF { get; set; }

		/// <summary>
		/// The business area.
		/// </summary>
		public string BusinessArea { get; set; }
	}
}
