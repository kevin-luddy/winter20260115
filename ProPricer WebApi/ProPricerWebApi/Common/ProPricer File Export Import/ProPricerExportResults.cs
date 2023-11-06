// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
	/// <summary>
	/// Contains the ProPricer export results of the Task and Resource files.
	/// </summary>
	public class ProPricerExportResults
	{
		/// <summary>
		/// Task export results.
		/// </summary>
		public ProPricerExportResult TaskExportResults { get; set; }

		/// <summary>
		/// Resource export results.
		/// </summary>
		public ProPricerExportResult ResourceExportResults { get; set; }

		/// <summary>
		/// The message if an error occurred.
		/// </summary>
		public string ExportExceptionMessage { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProPricerExportResults()
		{
			this.TaskExportResults = new ProPricerExportResult();
			this.ResourceExportResults = new ProPricerExportResult();
			this.ExportExceptionMessage = string.Empty;
		}
	}
}
