namespace GenBOE.Reports.Backend.Services
{
	using System.Collections.Generic;
	using System.IO;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Exports a Workspace to Word
	/// </summary>
	public interface IBoeExportService
	{
		/// <summary>
		/// Exports BOE(s) to Excel document.
		/// </summary>
		/// <param name="requestModel">Export Inputs</param>
		/// <returns>File location of Excel document</returns>
		string ExportBoeToExcel(BOEExcelExportInputs exportInputs);

		/// <summary>
		/// Exports BOE(s) to Word document.  
		/// </summary>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
		/// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <returns>Name of document</returns>
		string ExportBoeToWord(FileStream fileStream, ICollection<BoeCustomReportComponent> selectedComponents, bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			List<BOESummaryGridModelView> boeSummaryGridModelViews);

		/// <summary>
		/// Exports BOE(s) to Word document(s) that are then zipped
		/// </summary>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating whether the export is a custom export</param>
		/// <param name="exportFormat">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <returns>string location of zipped Word document(s)</returns>
		string ExportBoeToZip(ICollection<BoeCustomReportComponent> selectedComponents, bool isCustomExport,
			WorkspaceExportFormatDTO exportFormat, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews,
			List<BOESummaryGridModelView> boeSummaryGridModelViews);

	}
}
