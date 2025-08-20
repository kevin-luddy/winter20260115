namespace GenBOE.Web.ModelView
{
	using System.Collections.Generic;
	using GenBOE.Dtos;

	/// <summary>
	/// BOE Status Report Data used in genBOE Angular
	/// </summary>
	public class BOEStatusReportView
	{
		/// <summary>
		/// Data for BOE Status Report
		/// </summary>
		public ICollection<BOEStatusReportGrid> boeStatusReportModel { get; set; }

		/// <summary>
		/// UCOT Enabled For Workspace
		/// </summary>
		public bool isUCOTEnabledForWorkspace { get; set; }
	}
}