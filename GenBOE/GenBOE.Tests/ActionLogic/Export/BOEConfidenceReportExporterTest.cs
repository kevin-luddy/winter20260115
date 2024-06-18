// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Export
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Moq;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.ModelView;
	using System.IO;
	using GenBOE.DataBridge.DTO;
	using IES.Common.OfficeUtilities;
	using DocumentFormat.OpenXml.Packaging;

	/// <summary>
	/// Unit Test for BOEConfidenceReportExporter
	/// </summary>
	[TestClass]
	public class BOEConfidenceReportExporterTest
	{
		/// <summary>
		/// Create the System Under Test
		/// </summary>
		/// <returns>an instance of BOEConfidenceReports</returns>
		public BOEConfidenceReportExporter CreateSUT()
		{
			return new BOEConfidenceReportExporter();
		}

		/// <summary>
		/// Test method for Confidence Report Export
		/// </summary>
		[TestMethod]
		public void TestConfidenceReportExport()
		{
			BOEConfidenceReportExporter sut = CreateSUT();

			// Setup 
			ConfidenceReportModelView confidenceReportVM = new ConfidenceReportModelView();
			confidenceReportVM.TotalTaskCount = 10;
			confidenceReportVM.TasksWithoutErrors = 8;

			ConfidenceReportItem confidenceReportItem1 = new ConfidenceReportItem()
			{
				BoeId = 1,
				BoeTitle = "test title",
				TaskId = 1,
				TaskTitle = "test task",
				RteFields = 3,
				HasPoPError = false,
				HasMoqError = false,
				HasHistoricalRefError = true,
				ErrorText = "this is an error"
			};

			ConfidenceReportItem confidenceReportItem2 = new ConfidenceReportItem()
			{
				BoeId = 2,
				BoeTitle = "test title 2",
				TaskId = 2,
				TaskTitle = "test task 2",
				RteFields = 1,
				HasPoPError = false,
				HasMoqError = false,
				HasHistoricalRefError = true,
				ErrorText = "this is another error"
			};

			confidenceReportVM.ConfidenceReportData.Add(confidenceReportItem1);
			confidenceReportVM.ConfidenceReportData.Add(confidenceReportItem2);

			string fileLocation = Path.Combine(System.Environment.CurrentDirectory, "test" + ".xlsx");
			File.WriteAllBytes(fileLocation, Properties.Resources.ConfidenceReport);

			string result = sut.ExportToExcelFile(fileLocation, confidenceReportVM, 1);

			List<string> requiredColumns = new List<string>() { "BOE", "Task", "MOQ Types", "RTE Fields", "Confidence Error Messages" };

			try
			{
				// Assert file was created
				Assert.IsFalse(string.IsNullOrEmpty(result));
				Assert.IsTrue(File.Exists(result));

				using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(result, false))
				{
					List<Dictionary<string, string>> rows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(spreadsheet, "Confidence Report", requiredColumns.ToArray(), requiredColumns.ToArray()).ToList();

					Assert.IsTrue(rows.Count >= 2);

					bool containsItem1 = false, containsItem2 = false;

					foreach (Dictionary<string, string> row in rows)
					{
						if (row.ContainsKey("BOE") && row.ContainsValue("test title"))
						{
							containsItem1 = true;
						}
						if (row.ContainsKey("BOE") && row.ContainsValue("test title 2"))
						{
							containsItem2 = true;
						}
					}
					Assert.IsTrue(containsItem1);
					Assert.IsTrue(containsItem2);
				}
			}
			finally
			{
				// Uncomment out the line below when testing this method is done
				File.Delete(result);
			}
		}
	}
}
