// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
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
		/// <returns></returns>
		public string ExportToExcelFile(string templateFileLocation, ConfidenceReportModelView confidenceReport)
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

			string toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

			// Create a new worksheet to work off of
			/*ExcelExportWorksheet worksheet = new ExcelExportWorksheet
			{
				"Confidence Score: " + confidenceReport.ConfidenceScore
			};

			// Create the document object in memory
			using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
			{
				// Get the specified worksheet part
				WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, worksheet.WorksheetName);
				ExcelExporter.PopulateDataRows(spreadsheet, worksheetPart, worksheet, 2);
			}*/

			// Is there a way to make the columns filterable code-wise?
			/*foreach (ConfidenceReportItem item in confidenceReport.ConfidenceReportData)
			{
				worksheet.Add(item.BoeTitle, item.TaskTitle, item.MoqTypesString, item.RteFields.ToString(), item.ErrorText);
			}*/

			// Below works, but overrides the header row
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();
			string confidenceScore = "Confidence Score: " + confidenceReport.ConfidenceScore;
			worksheet.Add(new string[] { confidenceScore.ToString() });

			foreach (ConfidenceReportItem item in confidenceReport.ConfidenceReportData)
			{
				worksheet.Add(item.BoeTitle, item.TaskTitle, item.MoqTypesString, item.RteFields.ToString(), item.ErrorText);
			}

			// Pass the rows to the generic Excel exporter           
			//toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);
			//toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, new List<ExcelExportWorksheet> { worksheet }, 2);
			// TO-DO: Tried this with 2, still overwrites row #2. maybe change to 1 and then do an add of empty cells?
			toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, new List<ExcelExportWorksheet>() { worksheet }, new int?[] { 2 });

			return toReturn;
		}
	}
}
