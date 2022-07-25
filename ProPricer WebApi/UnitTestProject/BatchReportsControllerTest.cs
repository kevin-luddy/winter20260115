namespace UnitTestProject
{
    using System.Collections.Generic;
    using System.Linq;
    using APTSPropricerApi;
    using APTSPropricerApi.Controllers;
    using APTSPropricerApi.DTOs;
    using EBS.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                ICollection<BatchReportDto> results = controller.Get(TestConstants.SpaceInstanceId);
                Assert.IsNotNull(results);
                Assert.IsTrue(results.Any());
            }
        }

        /// <summary>
        /// Tests the exporting of a batch report
        /// </summary>
        [TestMethod]
        public void ExportBatch()
        {
            string proposalId = "3ac7f35f-5e08-ed11-9f7b-64c901b7a0ad";
            
            using (BatchReportsController controller = new BatchReportsController())
            {
                ICollection<BatchReportDto> results = controller.Get(TestConstants.SpaceInstanceId);
                // pick id for Batch Report
                string batchId = results.First(b => b.Name.StartsWith("15-2 iii a")).Id;

                string fileAsString = controller.Post(TestConstants.SpaceInstanceId, new ProPricerExportContainer
                {
                    batchReportId = batchId,
                    proposalId = proposalId
                });

                Assert.IsNotNull(fileAsString);
            }
        }
    }
}