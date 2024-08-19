// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using GenBOE.ActionLogic.ModelView;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Used for exporting a Confidence Report for a BOE
	/// </summary>
	public class BOEConfidenceReportExporter : IBOEConfidenceReportExporter
	{
		/// <summary>
		/// Exports all data to the Excel file
		/// </summary>
		/// <param name="templateFileLocation">File location of the Excel template</param>
		/// <param name="confidenceReport">View model of confidence report</param>
		/// <param name="workspace">Current workspace ID</param>
		/// <returns>The file location of the exported Excel file</returns>
		public string ExportToExcelFile(string templateFileLocation, ConfidenceReportModelView confidenceReport, string workspace)
		{
			// Check inputs
			if (templateFileLocation == null)
			{
				throw new ArgumentNullException(nameof(templateFileLocation));
			}
			if (confidenceReport == null)
			{
				throw new ArgumentNullException(nameof(confidenceReport));
			}

			string toReturn = string.Empty;

			// Create a new worksheet to work off of
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();
			string baseURL = ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspace + "/";
			string confidenceScore = "Confidence Score: " + confidenceReport.ConfidenceScore;
			worksheet.Add(new string[] { confidenceScore.ToString() });
			worksheet.Add(new string[] { "WBS #", "WBS Title", "BOE", "BOE URL", "Task", "Task URL", "MOQ Types", "RTE Fields", "Confidence Error Messages" });

			if (confidenceReport.ConfidenceReportData.Count > 0)
			{
				foreach (ConfidenceReportItem item in confidenceReport.ConfidenceReportData)
				{
					string boeURL = baseURL + "/BOE/EditBOEIndex/boe/" + item.BoeId;
					string taskURL = baseURL + "/BOE/EditBOEIndex/boe/" + item.BoeId + "#LMLabor/task/" + item.TaskId; 
					worksheet.Add(item.WbsNumber, item.WbsTitle, item.BoeTitle, boeURL, item.TaskTitle, taskURL, item.MoqTypesString, item.RteFields.ToString(), item.ErrorText);
				}
			}
			else
			{
				// Not adding this blank row to the worksheet will cause the Excel file to be corrupted when trying to open it
				worksheet.Add();
			}

			// Pass the rows to the generic Excel exporter           
			toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, new List<ExcelExportWorksheet>() { worksheet }, new int?[] { 1 });

			return toReturn;
		}
	}
}
