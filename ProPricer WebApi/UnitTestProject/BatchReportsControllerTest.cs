namespace UnitTestProject
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Web;
	using APTSPropricerApi;
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Controllers;
	using APTSPropricerApi.DTOs;
	using EBS.Core;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests for Batch Reports Controller
	/// </summary>
	[TestClass]
	public class BatchReportsControllerTest
	{
		/// <summary>
		/// Tests retrieving batch reports
		/// </summary>
		[TestMethod]
		public void GetBatchReports()
		{
			using (BatchReportsController controller = new BatchReportsController())
			{
				ProPricerResponse<ICollection<BatchReportDto>> results = controller.GetBatchReports(TestConstants.SpaceInstanceId);
				Assert.IsNotNull(results);
				Assert.IsTrue(results.IsSuccessful);
				Assert.IsTrue(results.Data.Any());
			}
		}

		/// <summary>
		/// Tests the exporting of a batch report
		/// </summary>
		[TestMethod]
		public void ExportBatch()
		{
			if (!Directory.Exists(Constants.TEMP_DIRECTORY))
			{
				Directory.CreateDirectory(Constants.TEMP_DIRECTORY);
			}

			string proposalId = "3ac7f35f-5e08-ed11-9f7b-64c901b7a0ad";

			using (BatchReportsController controller = new BatchReportsController())
			{
				ProPricerResponse<ICollection<BatchReportDto>> results = controller.GetBatchReports(TestConstants.SpaceInstanceId);
				// pick id for Batch Report
				string batchId = results.Data.First(b => b.Name.StartsWith("15-2 iii a")).Id;

				ProPricerResponse<string> response = controller.ExportBatchReport(TestConstants.SpaceInstanceId, new ProPricerExportContainer
				{
					batchReportId = batchId,
					proposalId = proposalId
				},
				Constants.REPORT_TYPE_EXCEL);

				Assert.IsTrue(response.IsSuccessful);
			}
		}
	}
}