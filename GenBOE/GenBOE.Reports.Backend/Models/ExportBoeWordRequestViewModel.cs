namespace GenBOE.Reports.Backend.Models
{
	using System.Collections.Generic;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Export BOE to Word Request
	/// </summary>
	public class ExportBoeWordRequestViewModel
	{
		/// <summary>
		/// Selected BOEs to export
		/// </summary>
		public ICollection<BoeCustomReportComponent> selectedComponents { get; set; }
		
		/// <summary>
		/// Should this use Custom Exporter or BOE Exporter
		/// </summary>
		public bool isCustomExport { get; set; }
		
		/// <summary>
		/// Export Format Template
		/// </summary>
		public WorkspaceExportFormatDTO wsExportFormatDTO { get; set; }
		
		/// <summary>
		/// BOE Export Inputs
		/// </summary>
		public BOEExportInputs exportInputs { get; set; }
		
		/// <summary>
		/// BOE Export Models
		/// </summary>
		public ICollection<BOEExportModelView> boeExportModelViews { get; set; }
		
		/// <summary>
		/// BOE Summary Grid data
		/// </summary>
		public List<BOESummaryGridModelView> boeSummaryGridModelViews { get; set; }
		
		/// <summary>
		/// If true, break out the output into separate files and return zip file
		/// </summary>
		public bool segmentedOutput { get; set; }
	}
}
