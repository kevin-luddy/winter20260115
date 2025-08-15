// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
	using System.Collections.Generic;
	using GenBOE.Dtos;

	/// <summary>
	/// Class to pass BOE Status Report Data along with some Workspace Data
	/// Which the Original BOEStatusReportModelView does not have
	/// </summary>
	public class FullBOEStatusReportModelView
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public FullBOEStatusReportModelView()
		{
			HoursLabel = "Hours";
			IsUCOTEnabledForWorkspace = false;
			BOEStatusReports = new List<BOEStatusReportModelView>();
		}

		/// <summary>
		/// Hours Label
		/// </summary>
		public string HoursLabel { get; set; }

		/// <summary>
		/// Is UCOT Enabled for Workspace
		/// </summary>
		public bool IsUCOTEnabledForWorkspace { get; set; }

		/// <summary>
		/// BOE Status Reports
		/// </summary>
		public ICollection<BOEStatusReportModelView> BOEStatusReports { get; set; }
	}
}
