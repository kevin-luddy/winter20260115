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
	public class BOEConfidenceReportExporter
	{
		/// <summary>
		/// Exports all data to the Excel file
		/// </summary>
		/// <param name="templateFileLocation">File location of the Excel template</param>
		/// <returns></returns>
		public string ExportToExcelFile(string templateFileLocation, )
		{
			// Create a new worksheet to work off of
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet
			{
				"Confidence Score: ",
				{ "BOE", "Task", "MOQ Types", "RTE Fields", "Confidence Error Messages" }
			};

			// Is there a way to make the columns filterable code-wise?
			// Confidence score is total # of tasks without errors divided by total number of tasks

			// Pass the rows to the generic Excel exporter           
			string toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

			return toReturn;
		}
	}
}
