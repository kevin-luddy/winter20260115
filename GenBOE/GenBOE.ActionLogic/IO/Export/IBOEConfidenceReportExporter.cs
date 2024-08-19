// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using GenBOE.ActionLogic.ModelView;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Interface used for exporting a Confidence Report for a BOE
	/// </summary>
	public interface IBOEConfidenceReportExporter
	{
		/// <summary>
		/// Exports all data to the Excel file
		/// </summary>
		/// <param name="templateFileLocation">File location of the Excel template</param>
		/// <param name="confidenceReport">View model of confidence report</param>
		/// <returns>The file location of the exported Excel file</returns>
		string ExportToExcelFile(string templateFileLocation, ConfidenceReportModelView confidenceReport, string workspace);
	}
}
