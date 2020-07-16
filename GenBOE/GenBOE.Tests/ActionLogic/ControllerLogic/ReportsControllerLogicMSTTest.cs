// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Web.Configuration;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReportsControllerLogicMSTTest
    {
        Mock<IMSTMetricLoader> metricLoader = new Mock<IMSTMetricLoader>();
        Mock<IBOEExporter> boeExporter = new Mock<IBOEExporter>();
        Mock<BOESummary> boeSummary;
        Mock<IBOECustomExporter> boeCustomeExporter = new Mock<IBOECustomExporter>();
        Mock<IWorkspaceExportFormatDTODataLoader> workspaceExportFormatDTOLoader = new Mock<IWorkspaceExportFormatDTODataLoader>();
        Mock<IFullObjectFactory> Factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> _retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();

        Mock<TravelTripCostCalculation> _TravelTripCostCalculator;
        Mock<RMSZoneTravelRatesFeesDataLoader> _RMSZoneTravelRatesFeesDataLoader;
        Mock<BOEDiscrepancyReport> boeDiscrepancyReport = new Mock<BOEDiscrepancyReport>(new Mock<IFullWorkspaceRecalculation>().Object, new Mock<IUserDTODataLoader>().Object);

        Mock<GenTRAC.DataBridge.DTO.IProposalLoader> proposalLoader = new Mock<GenTRAC.DataBridge.DTO.IProposalLoader>();
        Mock<IWorkspaceControllerLogic> workspaceControllerLogic = new Mock<IWorkspaceControllerLogic>();

        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);

            _TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();
            _RMSZoneTravelRatesFeesDataLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();

            boeSummary = new Mock<BOESummary>(_TravelTripCostCalculator.Object, _RMSZoneTravelRatesFeesDataLoader.Object);
        }

        /// <summary>
        /// Create sut
        /// </summary>
        /// <returns>sut</returns>
        public ReportsControllerLogicMST CreateSut()
        {
            return new ReportsControllerLogicMST(boeExporter.Object, boeSummary.Object, boeCustomeExporter.Object,
                workspaceExportFormatDTOLoader.Object, metricLoader.Object, boeDiscrepancyReport.Object,
                _commonDataMapper.Object, this.proposalLoader.Object, this.workspaceControllerLogic.Object, null, null);
        }

        /// <summary>
        /// This Test check GetMetricNameTaskElementmappingDTO. It will verify that the 
        /// mapping of dto task element to metric names is correct and that the value
        /// return is the proper mapping.
        /// </summary>
        [TestMethod]
        public void GetMetricNameTaskElementMappingDTOMSTTest()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            ICollection<BoeTaskElementDTO> TaskElementsCollection = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1 },
                new BoeTaskElementDTO() { Id = 2 },
                new BoeTaskElementDTO() { Id = 4 }};
            ICollection<int> inTaskElementIds = TaskElementsCollection.Select(x => x.Id).ToCollection<int>();

            ICollection<MetricIdTaskElementIdXrefDTO> xrefs = new Collection<MetricIdTaskElementIdXrefDTO>(){
                new MetricIdTaskElementIdXrefDTO() { MetricId = 1, TaskElementId = 10 },
                new MetricIdTaskElementIdXrefDTO() { MetricId = 2, TaskElementId = 10 },
                new MetricIdTaskElementIdXrefDTO() { MetricId = 2, TaskElementId = 20 },
                new MetricIdTaskElementIdXrefDTO() { MetricId = 3, TaskElementId = 30 }};
            ICollection<MSTMetricDetailsDTO> metrics = new Collection<MSTMetricDetailsDTO>(){
                new MSTMetricDetailsDTO() { Id = 1, MeasureName = "Measure 1" },
                new MSTMetricDetailsDTO() { Id = 2, MeasureName = "Measure 2" },
                new MSTMetricDetailsDTO() { Id = 3, MeasureName = "Measure 3" }};

            //Setup
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsCollection);
            metricLoader.Setup(x => x.GetTaskElementIdMeticIdMappings(inTaskElementIds)).Returns(xrefs);
            metricLoader.Setup(x => x.GetByTaskElementIds(inTaskElementIds)).Returns(metrics);

            ReportsControllerLogicMST sut = CreateSut();

            //ACt
            MetricNameTaskElementMappingDTO returnValue = sut.GetMetricNameTaskElementMappingDTO(workspace);

            //Assert
            Assert.AreEqual("ID 1: Measure 1, ID 2: Measure 2", returnValue.GetMetricNamesByTaskElementId(10));
            Assert.AreEqual("ID 2: Measure 2", returnValue.GetMetricNamesByTaskElementId(20));
            Assert.AreEqual("ID 3: Measure 3", returnValue.GetMetricNamesByTaskElementId(30));
        }

        /// <summary>
        /// This test checks GetMetricNameTaskElementMappingDTO. It will verify that when
        /// FullWorkspace is passed through that it will throw a ArgumentNullException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMetricNameTaskElementMappingDTOMSTTestWorkspaceNull()
        {
            //Value Declaration
            FullWorkspace workspace = null;

            ReportsControllerLogicMST sut = CreateSut();

            //ACt
            sut.GetMetricNameTaskElementMappingDTO(workspace);
        }

        #region Project Map Reports

        /// <summary>
        /// Test that GetSummaryReportsModelViews returns expected MVs for RMS
        /// </summary>
        [TestMethod]
        public void GetSummaryReportsModelViewsTest()
        {
            ReportsControllerLogicMST sut = this.CreateSut();

            int wsid = 1;

            Collection<ReportDTO> reports = new Collection<ReportDTO>
            {
                new ReportDTO() { ReportID = (int)Reports.CategoryClinSummary, ReportName = "Category Clin Summary", Description = "Category Clin Summary", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.ClinCategorySummary, ReportName = "Clin Category Summary", Description = "Clin Category Summary", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.ProjectClinCostSummary, ReportName = "Project Clin Cost Summary", Description = "Engr Clin Summary", ReportType = ReportType.SSRS }
            };

            Collection<Uri> urls = new Collection<Uri>()
            {
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.SUMMARY_REPORT_RMS, (int)SSRSReportType.ProjectCategoryCLINCostSummary)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.SUMMARY_REPORT_RMS, (int)SSRSReportType.ProjectCLINCategoryCostSummary)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.PROJECT_CLIN_COST_SUMMARY))
            };

            this._commonDataMapper.Setup(x => x.getReports()).Returns(reports);

            ICollection<SSRSReportsModelView> result = sut.GetSummaryReportsModelViews();

            Assert.IsTrue(result.Any());
            for(int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(reports[i].ReportID, result.ElementAt(i).ReportID);
                Assert.AreEqual(reports[i].ReportName, result.ElementAt(i).ReportName);
                Assert.AreEqual(reports[i].Description, result.ElementAt(i).Description);
                Assert.IsTrue(result.ElementAt(i).ReportUrl.ToString().StartsWith(urls[i].ToString()));
            }
        }

        /// <summary>
        /// Test that GetCustomerReportsModelViews returns expected MVs for RMS
        /// </summary>
        [TestMethod]
        public void GetCustomerReportsModelViewsTest()
        {
            ReportsControllerLogicMST sut = this.CreateSut();

            int wsid = 1;

            Collection<ReportDTO> reports = new Collection<ReportDTO>
            {
                new ReportDTO() { ReportID = (int)Reports.CostByClinResActYr, ReportName = "Cost By Clin, Resource, Activity, & Year", Description = "Cost By Clin, Resource, Activity, & Year", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.CostByClinActYr, ReportName = "Cost By Clin, Activity, & Year", Description = "Cost By Clin, Activity, & Year", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.BOESummaryReport, ReportName = "BOE Summary Report", Description = "BOE Summary Report", ReportType = ReportType.SSRS }
            };

            Collection<Uri> urls = new Collection<Uri>()
            {
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.COST_ANALYSIS_REPORT_RMS, (int)SSRSReportType.CostAnalysis8Years)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.COST_ANALYSIS_REPORT_RMS, (int)SSRSReportType.CostAnalysis17Years)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.BOE_SUMMARY_REPORT))
            };

            this._commonDataMapper.Setup(x => x.getReports()).Returns(reports);

            ICollection<SSRSReportsModelView> result = sut.GetCustomerReportsModelViews();

            Assert.IsTrue(result.Any());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(reports[i].ReportID, result.ElementAt(i).ReportID);
                Assert.AreEqual(reports[i].ReportName, result.ElementAt(i).ReportName);
                Assert.AreEqual(reports[i].Description, result.ElementAt(i).Description);
                Assert.IsTrue(result.ElementAt(i).ReportUrl.ToString().StartsWith(urls[i].ToString()));
            }
        }

        /// <summary>
        /// Test that GetFinanceReportsModelViews returns expected MVs for RMS
        /// </summary>
        [TestMethod]
        public void GetFinanceReportsModelViewsTest()
        {
            ReportsControllerLogicMST sut = this.CreateSut();

            int wsid = 1;

            Collection<ReportDTO> reports = new Collection<ReportDTO>
            {
                ////new ReportDTO() { ReportID = (int)Reports.ByPricingCode, ReportName = "By Pricing Code", Description = "CostByPricing", ReportType = ReportType.SSRS },
                ////new ReportDTO() { ReportID = (int)Reports.ByCatPricingCode, ReportName = "By Category/Pricing Code", Description = "By Category/Pricing Code", ReportType = ReportType.SSRS },
                ////new ReportDTO() { ReportID = (int)Reports.OffloadCostByYear, ReportName = "Offload Cost By Year", Description = "Offload Cost By Year", ReportType = ReportType.SSRS },
                ////new ReportDTO() { ReportID = (int)Reports.OffloadCostSummary, ReportName = "Offload Cost Summary", Description = "Offload Cost Summary", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.StaffingCurves, ReportName = "Staffing Curves", Description = "Staffing Curves", ReportType = ReportType.SSRS }
            };

            Collection<Uri> urls = new Collection<Uri>()
            {
                ////new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.BY_PRICING_CODE)),
                ////new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.BY_CAT_PRICING_CODE)),
                ////new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.OFFLOAD_COST_BY_YEAR)),
                ////new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.OFFLOAD_COST_SUMMARY)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.STAFFING_CURVES))
            };

            this._commonDataMapper.Setup(x => x.getReports()).Returns(reports);

            ICollection<SSRSReportsModelView> result = sut.GetFinanceReportsModelViews();

            Assert.IsTrue(result.Any());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(reports[i].ReportID, result.ElementAt(i).ReportID);
                Assert.AreEqual(reports[i].ReportName, result.ElementAt(i).ReportName);
                Assert.AreEqual(reports[i].Description, result.ElementAt(i).Description);
                Assert.IsTrue(result.ElementAt(i).ReportUrl.ToString().StartsWith(urls[i].ToString()));
            }
        }

        /// <summary>
        /// Test that GetAdditionalReportsModelViews returns expected MVs for RMS
        /// </summary>
        [TestMethod]
        public void GetAdditionalReportsModelViewTest()
        {
            ReportsControllerLogicMST sut = this.CreateSut();

            int wsid = 1;

            Collection<ReportDTO> reports = new Collection<ReportDTO>
            {
                new ReportDTO() { ReportID = (int)Reports.Rps, ReportName = "RPS", Description = "RPS", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.Prp, ReportName = "PRP", Description = "PRP", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.Ram, ReportName = "RAM", Description = "RAM", ReportType = ReportType.SSRS },
                new ReportDTO() { ReportID = (int)Reports.PreVsPostOffloadTotals, ReportName = "Pre Vs Post Offload Totals", Description = "Pre Vs Post Offload Totals", ReportType = ReportType.SSRS }
            };

            Collection<Uri> urls = new Collection<Uri>()
            {
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.RPS)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.PRP)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.RAM)),
                new Uri(this.getReportUrl(wsid, Constants.SSRSReportName.PRE_VS_POST_OFFLOAD_TOTALS))
            };

            this._commonDataMapper.Setup(x => x.getReports()).Returns(reports);

            ICollection<SSRSReportsModelView> result = sut.GetAdditionalReportsModelViews();

            Assert.IsTrue(result.Any());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(reports[i].ReportID, result.ElementAt(i).ReportID);
                Assert.AreEqual(reports[i].ReportName, result.ElementAt(i).ReportName);
                Assert.AreEqual(reports[i].Description, result.ElementAt(i).Description);
                Assert.IsTrue(result.ElementAt(i).ReportUrl.ToString().StartsWith(urls[i].ToString()));
            }
        }

        /// <summary>
        /// Gets the URL for the report
        /// </summary>
        /// <param name="wsid">Workspace ID</param>
        /// <param name="reportName">Name of the report</param>
        /// <param name="reportType">Report Type (if needed)</param>
        /// <returns>string containing report url</returns>
        private string getReportUrl(int wsid, string reportName, int? reportType = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Constants.NO_REPORT_PARAMETERS);
            if (reportType != null)
            {
                sb.Append($"&{Constants.REPORT_TYPE}={reportType}");
            }
            sb.Append("&nonce=");

            return $"{WebConfigurationManager.AppSettings["ReportServerLocation"]}/{WebConfigurationManager.AppSettings["ReportServerFolderName"]}/{reportName}{sb}";
        }

        #endregion

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages to make sure it returns no messages for RMS
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages()
        {
            ReportsControllerLogicMST sut = CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(null);
            Assert.IsFalse(result.Any());
        }
    }
}
