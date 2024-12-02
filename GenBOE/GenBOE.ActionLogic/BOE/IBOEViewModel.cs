// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// View Model for IBOE
	/// </summary>
	public class IBOEViewModel
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
		/// Selected IWTA Resources
		/// </summary>
		public ICollection<string> Resources { get; set; }

		/// <summary>
		/// Revision
		/// </summary>
		public int? Revision { get; set; }

		/// <summary>
		/// IWTA Business Area
		/// </summary>
		public string BusinessArea { get; set; }

		/// <summary>
		/// Proposal Date
		/// </summary>
		public string ProposalDate { get; set; }

		/// <summary>
		/// Proposal Title
		/// </summary>
		public string ProposalTitle { get; set; }

		/// <summary>
		/// Clin/WBS data is populated in GenBoe 
		/// </summary>
		public List<IBOECLINContractXREFData> IBOECLINContractXREF { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Basis and Rationale
		/// </summary>
		public string BasisAndRationale { get; set; }

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
		/// Check if user has only read only access
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Checks to see if the PBOE is complete with validation.
		/// </summary>
		public bool IsComplete { get; set; }

		/// <summary>
		/// Clin/WBS data is populated in GenBoe 
		/// </summary>
		public ICollection<IBOEClinData> IboeClinData { get; set; } = new List<IBOEClinData>();

		/// <summary>
		/// Period of Performance
		/// </summary>
		public string PeriodOfPerformance { get; set; }

		/// <summary>
		/// Whether or not this IBOE's associated workspace(s) contain OCI
		/// </summary>
		public bool ContainsOCI { get; set; }
	}
}
