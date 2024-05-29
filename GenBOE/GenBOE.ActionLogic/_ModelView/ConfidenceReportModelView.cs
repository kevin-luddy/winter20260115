// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// View Model for the Confidence Report
	/// </summary>
	public class ConfidenceReportModelView
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConfidenceReportModelView()
		{
			ConfidenceReportData = new Collection<ConfidenceReportItem>();
		}

		/// <summary>
		/// Total number of tasks in the report
		/// </summary>
		public int TotalTaskCount { get; set; }

		/// <summary>
		/// Total number of tasks with no errors in the report
		/// </summary>
		public int TasksWithoutErrors { get; set; }

		/// <summary>
		/// Confidence Score for the report
		/// </summary>
		public string ConfidenceScore => $"{TasksWithoutErrors}/{TotalTaskCount}";

		/// <summary>
		/// Collection of data for the report
		/// </summary>
		public ICollection<ConfidenceReportItem> ConfidenceReportData { get; set; }
	}
}