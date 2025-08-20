namespace GenBOE.Web.ModelView
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.Dtos;
	using GenBOE.Objects;

	/// <summary>
	/// BOE Status Report Data used in genBOE Angular
	/// </summary>
	public class BOEStatusReportView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public BOEStatusReportView() : base()
		{
			boeStatusReportModel = new Collection<BOEStatusReportModelView>();
		}
		/// <summary>
		/// Data for BOE Status Report
		/// </summary>
		public Collection<BOEStatusReportModelView> boeStatusReportModel { get; set; }

		/// <summary>
		/// All WBS used for filter
		/// </summary>
		public IReadOnlyCollection<FullWbs> allWBS { get; set; }

		/// <summary>
		/// All CLIN used for filter
		/// </summary>
		public IReadOnlyCollection<FullClin> allCLIN { get; set; }

		/// <summary>
		/// UCOT Enabled For Workspace
		/// </summary>
		public bool isUCOTEnabledForWorkspace { get; set; }
	}
}