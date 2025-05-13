// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.IO.Export
{
	using System.Collections.Generic;
	using System.IO;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using IES.Common.Core.Enums;
	using Microsoft.AspNetCore.Hosting;

	public interface IBOECustomExporter
	{
		/// <summary>
		/// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
		/// </summary>
		/// <param name="ws">Workspace containing BOE(s) in export</param>
		void SetWorkspacePrecisionVariables(WorkspaceDTO ws);

		/// <summary>
		/// This function will populate the BOEExportModelViews based on the BOE DTOs.
		/// </summary>
		/// <param name="boes">Full Boes.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>
		/// the BOE Export ModelViews.
		/// </returns>
		ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(ICollection<BoeDTO> boes, BOEExportInputs exportInputs);

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="components">List of selected components</param>
		/// <param name="exportFormat">Export file info</param>
		void ExportBOEToWordFile(FileStream fileStream, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			ICollection<BoeCustomReportComponent> components, WorkspaceExportFormatDTO exportFormat);

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="selectedComponents">List of selected components</param>
		/// <param name="returnStream">Output stream</param>
		/// <param name="exportFormat">Export file info</param>
		/// <returns>true if successful</returns>
		bool ExportBOEToWordStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			ICollection<BoeCustomReportComponent> selectedComponents, Stream returnStream, WorkspaceExportFormatDTO exportFormat);

		/// <summary>
		/// IES-707: Creates individual word documents for each BOE and compresses them into a single zip file.
		/// </summary>
		/// <param name="webHostEnvironment">Web host env</param>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="selectedComponents">Custom components</param>
		/// <param name="exportFormat">Export parameters</param>
		string ExportBOEsToZipFile(IWebHostEnvironment webHostEnvironment, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			ICollection<BoeCustomReportComponent> selectedComponents, WorkspaceExportFormatDTO exportFormat);
	}
}
