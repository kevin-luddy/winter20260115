// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.IO.Export
{
	using System.Collections.Generic;
	using System.IO;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Interface for BOE exporters
	/// </summary>
	public interface IBOEExporter
	{
		/// <summary>
		/// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
		/// </summary>
		/// <param name="workspace">Workspace containing BOE(s) in export.</param>
		void SetWorkspacePrecisionVariables(WorkspaceDTO workspace);

		/// <summary>
		/// This function will populate the BOEExportModelViews based on the BOE DTOs.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>
		/// the BOE Export ModelViews.
		/// </returns>
		ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(BOEExportInputs exportInputs);

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="fileStream">File stream</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="fileData">byte[] the template to copy and populate.</param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">Response</exception>
		void ExportBOEToWordFile(FileStream fileStream, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet);

		/// <summary>
		/// IES-707: Creates individual word documents for each BOE and compresses them into a single zip file.
		/// </summary>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="fileData">byte[] the template to copy and populate.</param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">if response or workspace is null</exception>
		string ExportBOEsToZipFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet);

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="returnStream">Output stream</param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">workspace</exception>
		/// <exception cref="GeneralAppException"></exception>
		/// <returns>true if successful, exception otherwise</returns>
		bool ExportBOEToWordStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData, Stream returnStream, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet);

		/// <summary>
		/// Exports the Manage BOEs to Excel.
		/// </summary>
		/// <param name="exportInputs">BOE ExportInputs.</param>
		/// <returns></returns>
		string ExportToExcelFile(BOEExcelExportInputs exportInputs);
	}
}