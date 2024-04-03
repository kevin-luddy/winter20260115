// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Common
{
	/// <summary>
	/// User Information (NT Domain and ID) for the LOB Lead, Lead Estimator, and Backup Estimator for a given Proposal.
	/// </summary>
	public class ProposalUserInformationDTO
	{
		/// <summary>
		/// LOB Lead NT ID
		/// </summary>
		public string LOBEstLeadMgrNtId { get; set; }

		/// <summary>
		/// Lead Estimator NT ID
		/// </summary>
		public string LeadEstimatorNtId { get; set; }

		/// <summary>
		/// Backup Estimator NT ID
		/// </summary>
		public string BackupEstimatorNtId { get; set; }
	}
}