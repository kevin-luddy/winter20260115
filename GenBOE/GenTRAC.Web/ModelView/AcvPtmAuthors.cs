// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.ModelView
{
	/// <summary>
	/// Lead/Backup Estimator and Cost Volume Lead for ACV (data returned from PTM)
	/// </summary>
	public class AcvPtmAuthors
	{
		/// <summary>
		/// Lead Estimator's NTID
		/// </summary>
		public string LeadEstimator { get; set; }

		/// <summary>
		/// Backup Estimator's NTID
		/// </summary>
		public string BackupEstimator { get; set; }

		/// <summary>
		/// Cost Volume Lead's NTID
		/// </summary>
		public string CostVolumeLead { get; set; }
	}
}