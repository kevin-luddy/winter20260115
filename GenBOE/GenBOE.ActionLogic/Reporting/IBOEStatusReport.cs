// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.Dtos;
	using IES.Common.OfficeUtilities;

	public interface IBOEStatusReport
	{
		/// <summary>
		/// Generates the boe status report.
		/// </summary>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns></returns>
		Collection<BOEStatusReportModelView> GenerateBOEStatusReport(BOEExportInputs exportInputs);

		/// <summary>
		/// Mapper function so the original BOEStatusReportModelView cannot be used in the HTTP GET for Angular
		/// </summary>
		/// <param name="boeStatusReportModelView">legacy genBOE boeStatusReportModelView</param>
		/// <returns>Mapped BOEStatusReportGrid List</returns>
		ICollection<BOEStatusReportGrid> ConvertBOEStatusData(Collection<BOEStatusReportModelView> boeStatusReportModelView);

		/// <summary>
		/// Sends the boe status report to file.
		/// </summary>
		/// <param name="templateFileLocation">The template file location.</param>
		/// <param name="statusReport">The status report.</param>
		/// <param name="reportID">The report identifier.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns></returns>
		string SendBOEStatusReportToFile(string templateFileLocation, Collection<BOEStatusReportModelView> statusReport, int reportID, BOEExportInputs exportInputs);

		/// <summary>
		/// Gets the excel export worksheet.
		/// </summary>
		/// <param name="statusReport">The status report.</param>
		/// <param name="reportID">The report identifier.</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns></returns>

		ExcelExportWorksheet GetExcelExportWorksheet(Collection<BOEStatusReportModelView> statusReport, int reportID, BOEExportInputs exportInputs); 
	}
}
