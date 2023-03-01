/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace UnitTestProject
{
	using APTSPropricerApi;
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.Controllers;
	using APTSPropricerApi.DTOs;
	using EBS.Core;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using ProPricer.UnitTests;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Tests for Batch Reports Controller
	/// </summary>
	[TestClass]
	public class BatchReportsControllerTest
	{
		/// <summary>
		/// Create SUT (System Under Test)
		/// </summary>
		private static BatchReportsController CreateSUT()
		{
			BatchReportsController service = new(Mock.Of<ILogger<BatchReportsController>>(),
				Configuration.ServiceProvider.GetService<PoolManagerList>());

			return service;
		}

		/// <summary>
		/// Tests retrieving batch reports
		/// </summary>
		[TestMethod]
		public void GetBatchReports()
		{
			BatchReportsController controller = CreateSUT();

			ProPricerResponse<ICollection<BatchReportDto>> results = controller.GetBatchReports(TestConstants.SpaceInstanceId);
			Assert.IsNotNull(results);
			Assert.IsTrue(results.IsSuccessful);
			Assert.IsTrue(results.Data.Any());
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

			BatchReportsController controller = CreateSUT();

			ProPricerResponse<ICollection<BatchReportDto>> results = controller.GetBatchReports(TestConstants.SpaceInstanceId);
			// pick id for Batch Report
			string batchId = results.Data.First(b => b.Name.StartsWith("15-2 iii a")).Id;
			ProPricerResponse<ICollection<Table>> response = controller.ExportBatchReport(TestConstants.SpaceInstanceId, new ProPricerExportContainer
			{
				batchReportId = batchId,
				proposalId = proposalId
			},
			out string _);

			Assert.IsTrue(response.IsSuccessful);
		}
	}
}