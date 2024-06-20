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
		/// Total number of Confidence Error checks in the report
		/// </summary>
		public int MaximumConfidenceValue { get; set; }

		/// <summary>
		/// Total number Successfull Confidence checks in the report
		/// </summary>
		public int ConfidenceValue { get; set; }

		/// <summary>
		/// Confidence Score for the report
		/// </summary>
		public string ConfidenceScore => $"{ConfidenceValue}/{MaximumConfidenceValue}";

		/// <summary>
		/// Collection of data for the report
		/// </summary>
		public ICollection<ConfidenceReportItem> ConfidenceReportData { get; set; }
	}
}