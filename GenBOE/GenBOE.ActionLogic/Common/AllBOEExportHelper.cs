// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Web;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.ActionLogic.IO.Export;
	using IES.Common;
	using IES.Common.Compression;

	/// <summary>
	/// IES-707: AllBOEs export helper. Uses passed in function to convert the BOE to a Word document stream, then zips them - returning
	/// a the updated HttpResponse object ready for download.
	/// </summary>
	public static class AllBOEExportHelper
	{
		/// <summary>
		/// Creates individual documents for each BOE and compresses them into a single zip file.
		/// </summary>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="workSpace">Full workspace</param>
		/// <param name="response">What will ultimately be the response to the requester</param>
		/// <param name="returnFilename">File name that will be passed to browser (for download)</param>
		/// <param name="templatePath">Server path to the export template</param>
		/// <param name="getWordDocStream">Generic function that will convert the BOE data to a Word document</param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">if response or workspace is null</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static void ExportBOEsToZipFile<T>(
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			FullWorkspace workSpace,
			HttpResponseBase response,
			string returnFilename,
			string templatePath,
			Func<BOEExportInputs, ICollection<BOEExportModelView>, ICollection<BOESummaryGridModelView>, FullWorkspace, string, Stream, ExcelReportTemplateType, T> getWordDocStream,
			ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
		{
			_ = response ?? throw new ArgumentNullException(nameof(response));
			_ = workSpace ?? throw new ArgumentNullException(nameof(workSpace));

			SetResponseProperties(response, returnFilename);

			Dictionary<string, Stream> zipFiles = new Dictionary<string, Stream>();

			if (boeExportModelViews != null && getWordDocStream != null)
			{
				foreach (BOEExportModelView model in boeExportModelViews)
				{
					using (MemoryStream file = new MemoryStream())
					{
						// in order to reuse GenerateBOEToWordFileStream: we will create the expected list, but with just the single model
						ICollection<BOEExportModelView> boe = new List<BOEExportModelView>() { model };

						string fileName = GenerateExportFileName(model.WBSNumber, model.CLINNumber, model.BOETitle.Replace(" ", string.Empty), model.BoeID, workSpace.BOEExportSortByID);

						getWordDocStream(exportInputs, boe, boeSummaryGridModelViews, workSpace, templatePath, file, templateType);
						zipFiles.Add(fileName, new MemoryStream(file.ToArray()));
					}
				}
			}

			// now, let's zip the files up
			string savedZipFile = Zip.ZipFiles(zipFiles, HttpContext.Current.Server.MapPath(ImportExportConstants.EXPORT_PATH));

			using (FileStream zipStream = new FileStream(savedZipFile, FileMode.Open))
			{
				zipStream.CopyTo(response.OutputStream);
			}

			// Remove zip from server now that we have the content in the response stream
			File.Delete(savedZipFile);
		}

		/// <summary>
		/// Creates individual documents for each BOE with custom components and compresses them into a single zip file.
		/// </summary>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="workSpace">Full workspace</param>
		/// <param name="response">What will ultimately be the response to the requester</param>
		/// <param name="selectedComponents"></param>
		/// <param name="returnFilename">File name that will be passed to browser (for download)</param>
		/// <param name="exportFormat">Export Formatting DTO</param>
		/// <param name="getWordDocStream">Generic function that will convert the BOE data to a Word document</param>
		/// <typeparam name="T">Generic representing the return value of the passed in function</typeparam>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static void ExportCustomComponentBOEsToZipFile<T>(
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			FullWorkspace workSpace,
			HttpResponseBase response,
			ICollection<BoeCustomReportComponent> selectedComponents,
			string returnFilename,			
			WorkspaceExportFormatDTO exportFormat,
			Func<BOEExportInputs, ICollection<BOEExportModelView>, ICollection<BOESummaryGridModelView>, FullWorkspace, ICollection<BoeCustomReportComponent>, Stream, WorkspaceExportFormatDTO, T> getWordDocStream)
		{
			_ = response ?? throw new ArgumentNullException(nameof(response));
			_ = workSpace ?? throw new ArgumentNullException(nameof(workSpace));

			SetResponseProperties(response, returnFilename);

			Dictionary<string, Stream> zipFiles = new Dictionary<string, Stream>();

			if (boeExportModelViews != null && getWordDocStream != null)
			{
				foreach (BOEExportModelView model in boeExportModelViews)
				{
					using (MemoryStream file = new MemoryStream())
					{
						// in order to reuse GenerateBOEToWordFileStream: we will create the expected list, but with just the single model
						ICollection<BOEExportModelView> boe = new List<BOEExportModelView>() { model };

						string fileName = GenerateExportFileName(model.WBSNumber, model.CLINNumber, model.BOETitle.Replace(" ", string.Empty), model.BoeID, workSpace.BOEExportSortByID);

						getWordDocStream(exportInputs, boe, boeSummaryGridModelViews, workSpace, selectedComponents, file, exportFormat);
						zipFiles.Add(fileName, new MemoryStream(file.ToArray()));
					}
				}
			}

			// now, let's zip the files up
			string savedZipFile = Zip.ZipFiles(zipFiles, HttpContext.Current.Server.MapPath(ImportExportConstants.EXPORT_PATH));

			using (FileStream zipStream = new FileStream(savedZipFile, FileMode.Open))
			{
				zipStream.CopyTo(response.OutputStream);
			}

			// Remove zip from server now that we have the content in the response stream
			File.Delete(savedZipFile);
		}

		/// <summary>
		/// Sets properties of response object.
		/// </summary>
		/// <param name="response">Object containing response data</param>
		/// <param name="returnFilename">File name of the item that will be downloaded</param>
		private static void SetResponseProperties(HttpResponseBase response, string returnFilename)
		{
			response.ContentType = BOEExporter.CONTENT_TYPE_ZIP;
			response.Clear();
			response.BufferOutput = true;
			response.AppendHeader("Content-Disposition", $"attachment;filename={returnFilename}");
		}

		/// <summary>
		/// Formats the BOE export file name, depending on the workspace sort order.
		/// </summary>
		/// <param name="wbsNumber">Work Breakdown Structure Number</param>
		/// <param name="clin">Contract Line Item Number</param>
		/// <param name="boeTitle">Title of the BOE</param>
		/// <param name="boeId">Identifier for the BOE</param>
		/// <param name="exportSortOrder">Workspace sort order</param>
		/// <returns>String of properly formatted file name</returns>
		private static string GenerateExportFileName(string wbsNumber, string clin, string boeTitle, int boeId, int exportSortOrder)
		{
			string outFileName = string.Empty;

			if (exportSortOrder == SystemSettingConstants.EXPORT_SORT_WBS)
			{
				outFileName = $"{wbsNumber}_{clin}_{boeTitle}_{boeId}.docx";
			}
			else if (exportSortOrder == SystemSettingConstants.EXPORT_SORT_CLIN)
			{
				outFileName = $"{clin}_{wbsNumber}_{boeTitle}_{boeId}.docx";
			}

			return outFileName;
		}
	}
}