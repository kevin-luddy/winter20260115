// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using GenBOE.Dtos;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>

	public class GeneralReportViewModel
	{
		public GeneralReportViewModel()
		{
			this.ReportName = string.Empty;
			this.Description = string.Empty;
			this.ReportID = 0;
		}

		public GeneralReportViewModel(ReportDTO report) : this()
		{
			if (report != null)
			{
				this.ReportName = report.ReportName;
				this.ReportID = report.ReportID;
				this.Description = report.Description;
			}
		}

		public string ReportName { get; set; }
		public string Description { get; set; }
		public int ReportID { get; set; }
	}
}