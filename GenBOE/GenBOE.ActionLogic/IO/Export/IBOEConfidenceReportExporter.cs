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

	/// <summary>
	/// Interface used for exporting a Confidence Report for a BOE
	/// </summary>
	public interface IBOEConfidenceReportExporter
	{
		/// <summary>
		/// Exports all data to the Excel file
		/// </summary>
		/// <param name="templateFileLocation"></param>
		/// <returns></returns>
		string ExportToExcelFile(string templateFileLocation);
	}
}
