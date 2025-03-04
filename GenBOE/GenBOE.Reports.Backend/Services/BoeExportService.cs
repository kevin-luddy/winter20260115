namespace GenBOE.Reports.Backend.Services
{
	using System;
	using System.Collections.Generic;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.WorkspaceExportFormat;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Exports a Workspace to Word
	/// </summary>
	public class BoeExportService : IBoeExportService
	{

		public BoeExportService()
		{
		}

		/// <summary>
		/// Exports BOE(s) to Word document, may be zipped
		/// </summary>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
		/// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <param name="segmentedOutput">Should the output be broken into segments and zipped</param>
		/// <returns>string location of Word or zipped Word document(s)</returns>
		public string ExportBoeToWord(ICollection<BoeCustomReportComponent> selectedComponents, bool isCustomExport, WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, List<BOESummaryGridModelView> boeSummaryGridModelViews, bool segmentedOutput)
		{
			throw new NotImplementedException();
		}
	}
}
