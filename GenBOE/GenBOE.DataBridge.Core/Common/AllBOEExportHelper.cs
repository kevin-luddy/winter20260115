// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using DocumentFormat.OpenXml.Wordprocessing;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Utilities;

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
		/// <param name="templatePath">Server path to the export template</param>
		/// <param name="getWordDocStream">Generic function that will convert the BOE data to a Word document</param>
		/// <param name="templateType">Template type</param>
		/// <exception cref="ArgumentNullException">if response or workspace is null</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static string ExportBOEsToZipFile<T>(
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			FullWorkspace workSpace,
		byte[] fileData,
			Func<BOEExportInputs, ICollection<BOEExportModelView>, ICollection<BOESummaryGridModelView>, FullWorkspace, byte[], Stream, ExcelReportTemplateType, T> getWordDocStream,
			ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
		{
			_ = workSpace ?? throw new ArgumentNullException(nameof(workSpace));

			Dictionary<string, Stream> zipFiles = new Dictionary<string, Stream>();

			if (boeExportModelViews != null && getWordDocStream != null)
			{
				foreach (BOEExportModelView model in boeExportModelViews)
				{
					using (MemoryStream file = new MemoryStream())
					{
						// in order to reuse GenerateBOEToWordStream: we will create the expected list, but with just the single model
						ICollection<BOEExportModelView> boe = new List<BOEExportModelView>() { model };

						string fileName = GenerateExportFileName(model.WBSNumber, model.CLINNumber, model.BOETitle, model.BoeID, workSpace.BOEExportSortByID);

						getWordDocStream(exportInputs, boe, boeSummaryGridModelViews, workSpace, fileData, file, templateType);
						zipFiles.Add(fileName, new MemoryStream(file.ToArray()));
					}
				}
			}

			// now, let's zip the files up
			string savedZipFile = Zip.ZipFiles(zipFiles, Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ImportExportConstants.EXPORT_PATH));

			// dispose the streams
			foreach (Stream stream in zipFiles.Values)
			{
				stream.Dispose();
			}

			return savedZipFile;
		}

		/// <summary>
		/// Creates individual documents for each BOE with custom components and compresses them into a single zip file.
		/// </summary>
		/// <param name="exportInputs">The export inputs</param>
		/// <param name="boeExportModelViews">Collection of BOE View Models</param>
		/// <param name="boeSummaryGridModelViews"></param>
		/// <param name="workSpace">Full workspace</param>
		/// <param name="selectedComponents"></param>
		/// <param name="exportFormat">Export Formatting DTO</param>
		/// <param name="getWordDocStream">Generic function that will convert the BOE data to a Word document</param>
		/// <typeparam name="T">Generic representing the return value of the passed in function</typeparam>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static string ExportCustomComponentBOEsToZipFile<T>(
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
			FullWorkspace workSpace,
			ICollection<BoeCustomReportComponent> selectedComponents,
			WorkspaceExportFormatDTO exportFormat,
			Func<BOEExportInputs, ICollection<BOEExportModelView>, ICollection<BOESummaryGridModelView>, FullWorkspace, ICollection<BoeCustomReportComponent>, Stream, WorkspaceExportFormatDTO, T> getWordDocStream)
		{
			_ = workSpace ?? throw new ArgumentNullException(nameof(workSpace));

			Dictionary<string, Stream> zipFiles = new Dictionary<string, Stream>();

			if (boeExportModelViews != null && getWordDocStream != null)
			{
				foreach (BOEExportModelView model in boeExportModelViews)
				{
					using (MemoryStream file = new MemoryStream())
					{
						// in order to reuse GenerateBOEToWordStream: we will create the expected list, but with just the single model
						ICollection<BOEExportModelView> boe = new List<BOEExportModelView>() { model };

						string fileName = GenerateExportFileName(model.WBSNumber, model.CLINNumber, model.BOETitle, model.BoeID, workSpace.BOEExportSortByID);

						getWordDocStream(exportInputs, boe, boeSummaryGridModelViews, workSpace, selectedComponents, file, exportFormat);
						zipFiles.Add(fileName, new MemoryStream(file.ToArray()));
					}
				}
			}

			// now, let's zip the files up
			string savedZipFile = Zip.ZipFiles(zipFiles, Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ImportExportConstants.EXPORT_PATH));

			// dispose the streams
			foreach (Stream stream in zipFiles.Values)
			{
				stream.Dispose();
			}

			return savedZipFile;
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

			boeTitle = CommonUtilities.StripIllegalFileNameCharacters(boeTitle);
			wbsNumber = CommonUtilities.StripIllegalFileNameCharacters(wbsNumber);
			clin = CommonUtilities.StripIllegalFileNameCharacters(clin);

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