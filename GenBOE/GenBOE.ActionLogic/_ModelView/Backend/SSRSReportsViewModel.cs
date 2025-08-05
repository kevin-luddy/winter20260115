// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using GenBOE.Dtos;
	using System;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>

	/// <summary>
	/// SSRS Report Modelview
	/// Inherits Exports Modelview, with report URL
	/// </summary>
	public class SSRSReportsViewModel : GeneralReportViewModel
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public SSRSReportsViewModel()
		{
			this.ReportUrl = new Uri(string.Empty);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="report">Report DTO</param>
		public SSRSReportsViewModel(ReportDTO report) : base(report)
		{
			this.ReportUrl = new Uri(string.Empty);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="report">Report DTO</param>
		/// <param name="url">Report URL</param>
		public SSRSReportsViewModel(ReportDTO report, Uri url) : base(report)
		{
			this.ReportUrl = url;
		}

		/// <summary>
		/// Gets/Sets the Report URL
		/// </summary>
		public Uri ReportUrl { get; set; }
	}
}
