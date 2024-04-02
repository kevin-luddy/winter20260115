// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.ModelView
{
	/// <summary>
	/// Lead Estimator and Backup Estimator for ACV (data returned from PTM)
	/// </summary>
	public class AcvPtmAuthors
	{
		/// <summary>
		/// Lead Estimator's name
		/// </summary>
		public string LeadEstimator { get; set; }

		/// <summary>
		/// Backup Estimator's name
		/// </summary>
		public string BackupEstimator { get; set; }
	}
}