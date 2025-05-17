// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.DataBridge.Core.Common;
using GenBOE.DataBridge.Core.DTO;
using GenBOE.DataBridge.Core.DTO.Export.BOE;
using GenBOE.DataBridge.Core.DTO.FullObjects;
using IES.Common.Core.Enums;
using Microsoft.AspNetCore.Hosting;

namespace GenBOE.DataBridge.Core.IO.Export
{
	/// <summary>
	/// This decorator determines the methods to be used for exporting templates in MST based on the template ID.
	/// Due to ISGS templates being used in MST, it must be determined which version of certain methods is to be used due to certain ones being overridden for MST templates.
	/// If an ISGS template is used, the base versions of methods overridden in BOEExportMST need to be used. Otherwise the override method will be used as it was when there were only MST templates.
	/// </summary>
	public class BOEExporterMSTDecorator : IBOEExporter
	{
		/// <summary>
		/// The base exporter.
		/// </summary>
		private IBOEExporter baseExporter;

		/// <summary>
		/// The MST exporter.
		/// </summary>
		private IBOEExporter mstExporter;

		public BOEExporterMSTDecorator(BOEExporter baseExporter, BOEExporterMST mstExporter)
		{
			this.baseExporter = baseExporter;
			this.mstExporter = mstExporter;
		}

		/// <summary>
		/// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
		/// </summary>
		/// <param name="workspace">Workspace containing BOE(s) in export</param>
		public void SetWorkspacePrecisionVariables(WorkspaceDTO workspace)
		{
			//Same for both ISGS and MST Templates
			mstExporter.SetWorkspacePrecisionVariables(workspace);
			baseExporter.SetWorkspacePrecisionVariables(workspace);
		}

		/// <summary>
		/// This function will populate the BOEExportModelViews based on the BOE DTOs.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>
		/// the BOE Export ModelViews.
		/// </returns>
		/// <exception cref="ArgumentNullException">exportInputs</exception>
		public ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(BOEExportInputs exportInputs)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}
			// The workspace's export format doesn't have the correct template type if this is a user template so always use the exportFormatDTO
			WorkspaceExportFormatDTO exportFormatDTO = exportInputs.WorkspaceExportFormat;

			if (exportFormatDTO != null && (int)exportFormatDTO.ExportFormat.TemplateType < 1001)
			{
				return baseExporter.ConvertBoeDTOsToExportMVs(exportInputs);
			}
			else
			{
				return mstExporter.ConvertBoeDTOsToExportMVs(exportInputs);
			}
		}

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="ws">Full WS</param>
		/// <param name="response">the web response object to write the file back to for user download</param>
		/// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
		/// <param name="templatePath">Physical path of the template to copy and populate.</param>
		/// <param name="templateType">Template type</param>
		public void ExportBOEToWordFile(FileStream fileStream, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
		{
			if ((int)templateType < 1001)
			{
				baseExporter.ExportBOEToWordFile(fileStream, exportInputs, boeExportModelViews, boeSummaryGridModelViews, fileData, templateType);
			}
			else
			{
				mstExporter.ExportBOEToWordFile(fileStream, exportInputs, boeExportModelViews, boeSummaryGridModelViews, fileData, templateType);
			}
		}

		/// <summary>
		/// Export data about the given BOE into a pre-formatted Word template and return the file path of
		/// the populated template.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
		/// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
		/// <param name="returnStream">Output stream</param>
		/// <param name="templateType">Template type</param>
		/// <returns>true if successful</returns>
		public bool ExportBOEToWordStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData, Stream returnStream, ExcelReportTemplateType templateType)
		{
			if ((int)templateType < 1001)
			{
				baseExporter.ExportBOEToWordStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, fileData, returnStream, templateType);
			}
			else
			{
				mstExporter.ExportBOEToWordStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, fileData, returnStream, templateType);
			}

			return true;
		}

		/// <summary>
		/// Exports the BOEs to Excel.
		/// </summary>
		/// <param name="exportInputs">BOE exportInputs.</param>
		/// <returns></returns>
		public string ExportToExcelFile(BOEExcelExportInputs exportInputs)
		{
			//Same for both ISGS and MST Templates
			return mstExporter.ExportToExcelFile(exportInputs);
		}

		/// <summary>
		/// IES-707: Creates individual documents for reach BOE and compresses them into a single zip file.
		/// </summary>
		/// <param name="webHostEnvironment">Web host env</param>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">if response or workspace is null</exception>
		public string ExportBOEsToZipFile(
			IWebHostEnvironment webHostEnvironment,
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			byte[] fileData,
			ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
		{
			return AllBOEExportHelper.ExportBOEsToZipFile<bool>(webHostEnvironment, exportInputs, boeExportModelViews, boeSummaryGridModelViews, fileData, ExportBOEToWordStream);
		}
	}
}
