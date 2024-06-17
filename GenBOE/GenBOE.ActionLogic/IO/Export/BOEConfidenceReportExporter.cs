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
		/// <param name="boeID">The BOE ID</param>
		/// <returns>The file location of the exported Excel file</returns>
		public string ExportToExcelFile(string templateFileLocation, ConfidenceReportModelView confidenceReport, int? boeID)
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
			string confidenceScore = "Confidence Score: " + confidenceReport.ConfidenceScore;
			worksheet.Add(new string[] { confidenceScore.ToString() });
			worksheet.Add(new string[] { "BOE", "Task", "MOQ Types", "RTE Fields", "Confidence Error Messages" });

			if (boeID.HasValue)
			{
				foreach (ConfidenceReportItem item in confidenceReport.ConfidenceReportData.Where(x => x.BoeId == boeID.Value))
				{
					worksheet.Add(item.BoeTitle, item.TaskTitle, item.MoqTypesString, item.RteFields.ToString(), item.ErrorText);
				}
			}
			else
			{
				foreach (ConfidenceReportItem item in confidenceReport.ConfidenceReportData)
				{
					worksheet.Add(item.BoeTitle, item.TaskTitle, item.MoqTypesString, item.RteFields.ToString(), item.ErrorText);
				}
			}

			// Pass the rows to the generic Excel exporter           
			toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, new List<ExcelExportWorksheet>() { worksheet }, new int?[] { 1 });

			return toReturn;
		}
	}
}
