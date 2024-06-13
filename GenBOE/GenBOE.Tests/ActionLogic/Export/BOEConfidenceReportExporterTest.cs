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

	/// <summary>
	/// Unit Test for BOEConfidenceReportExporter
	/// </summary>
	[TestClass]
	public class BOEConfidenceReportExporterTest
	{
		/// <summary>
		/// MOQ Type Loader
		/// </summary>
		private Mock<IMoqTypeDataLoader> moqTypeLoader;

		/// <summary>
		/// Create the System Under Test
		/// </summary>
		/// <returns>an instance of BOEConfidenceReports</returns>
		public BOEConfidenceReportExporter CreateSUT()
		{
			moqTypeLoader = new Mock<IMoqTypeDataLoader>();

			return new BOEConfidenceReportExporter();
		}

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

			//string fileLocation = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName() + ".xlsx");
			string fileLocation = Path.Combine(System.Environment.CurrentDirectory, "test" + ".xlsx");
			File.WriteAllBytes(fileLocation, Properties.Resources.ConfidenceReport);

			string result = sut.ExportToExcelFile(fileLocation, confidenceReportVM);

			try
			{
				// Assert file was created
				Assert.IsFalse(string.IsNullOrEmpty(result));
				Assert.IsTrue(File.Exists(result));
			}
			finally
			{
				// Uncomment out the line below when testing this method is done
				//File.Delete(result);
			}
		}
	}
}
