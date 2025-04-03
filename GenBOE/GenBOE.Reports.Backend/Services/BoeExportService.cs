// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Reports.Backend.Services
{
	using System.Collections.Generic;
	using System.IO;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.IO.Export;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Exports a Workspace to Word
	/// </summary>
	public class BoeExportService : IBoeExportService
	{
		/// <summary>
		/// BOE Exporter
		/// </summary>
		private readonly IBOEExporter boeExporter;

		/// <summary>
		/// BOE Custom Exporter
		/// </summary>
		private readonly IBOECustomExporter boeCustomExporter;

		public BoeExportService(IBOEExporter boeExporter, IBOECustomExporter boeCustomExporter)
		{
			this.boeExporter = boeExporter;
			this.boeCustomExporter = boeCustomExporter;
		}

		/// <summary>
		/// Exports BOE(s) to Excel document.
		/// </summary>
		/// <param name="requestModel">Export Inputs</param>
		/// <returns>File location of Excel document</returns>
		public string ExportBoeToExcel(BOEExcelExportInputs exportInputs)
		{
			return this.boeExporter.ExportToExcelFile(exportInputs);
		}

		/// <summary>
		/// Exports BOE(s) to Word document(s) that are then zipped
		/// </summary>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
		/// <param name="exportFormat">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <returns>string location of Word or zipped Word document(s)</returns>
		public string ExportBoeToZip(ICollection<BoeCustomReportComponent> selectedComponents, bool isCustomExport,
			WorkspaceExportFormatDTO exportFormat, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews,
			List<BOESummaryGridModelView> boeSummaryGridModelViews)
		{
			if (isCustomExport)
			{
				return this.boeCustomExporter.ExportBOEsToZipFile(
						exportInputs,
						boeExportModelViews,
						boeSummaryGridModelViews,
						selectedComponents,
						exportFormat);
			}
			else
			{
				return this.boeExporter.ExportBOEsToZipFile(
						exportInputs,
						boeExportModelViews,
						boeSummaryGridModelViews,
						exportFormat.FileData,
						exportFormat.ExportFormat.TemplateType
					);
			}
		}

		/// <summary>
		/// Exports BOE(s) to Word document
		/// </summary>
		/// <param name="stream">Stream to stream file into</param>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
		/// <param name="exportFormat">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <returns>Name of document</returns>
		public string ExportBoeToWord(Stream stream, ICollection<BoeCustomReportComponent> selectedComponents, bool isCustomExport, 
			WorkspaceExportFormatDTO exportFormat, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			List<BOESummaryGridModelView> boeSummaryGridModelViews)
		{
			if (isCustomExport)
			{
				// Call the export function in the business layer
				this.boeCustomExporter.ExportBOEToWordStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, 
					selectedComponents, stream, exportFormat);
				return string.Format("genBOECustomExport-{0}.docx", exportInputs.Workspace.WorkspaceName).Replace(",", string.Empty);
			}
			else
			{
				// Call the export function in the business layer
				this.boeExporter.ExportBOEToWordStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, 
						exportFormat.FileData, stream, exportFormat.ExportFormat.TemplateType);

				return string.Format("genBOEExport-{0}.docx", exportInputs.Workspace.WorkspaceName).Replace(",", string.Empty);
			}
		}
	}
}
