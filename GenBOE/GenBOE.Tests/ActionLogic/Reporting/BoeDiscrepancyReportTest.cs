// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests the FullWorkspaceRecalculationTest Class
    ///</summary>
    [TestClass]
    public class BoeDiscrepancyReportTest
    {
        #region Create System, Loaders, Mappers and so on

        private Mock<IFullWorkspaceRecalculation> WsRecalculation;
        private Mock<IUserDTODataLoader> UserLoader;
        private Mock<IRetriever> Retriever;
        private Mock<IFullObjectFactory> Factory;
        private Mock<IPermissionsDTODataLoader> PermissionLoader;
        private Mock<ICommonDataMapper> CommonMapper;

        /// <summary>
        /// Creates System to test..
        /// </summary>
        /// <returns>SUT</returns>
        private BOEDiscrepancyReport CreateSystem()
        {
            this.WsRecalculation = new Mock<IFullWorkspaceRecalculation>();
            this.UserLoader = new Mock<IUserDTODataLoader>();

            this.Retriever = new Mock<IRetriever>();
            this.Factory = new Mock<IFullObjectFactory>();
            this.PermissionLoader = new Mock<IPermissionsDTODataLoader>();
            this.CommonMapper = new Mock<ICommonDataMapper>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.Retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.PermissionLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.CommonMapper.Object);

            BOEDiscrepancyReport report = new BOEDiscrepancyReport(this.WsRecalculation.Object, this.UserLoader.Object);

            return report;
        }

        #endregion

        /// <summary>
        /// Tests the GetReport method w/ invalid input
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetReportTest_Exception()
        {
            BOEDiscrepancyReport sut = this.CreateSystem();
            sut.GetReport(null);
        }

        /// <summary>
        /// Tests the GetReport method w/ no data
        /// </summary>
        [TestMethod]
        public void GetReportTest_WithNoData()
        {
            BOEDiscrepancyReport sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace();

            this.WsRecalculation.Setup(x => x.GetTaskElementsWithNonZeroDeltaLabor(ws)).Returns(new Collection<BoeTaskElementDTO>());
            this.WsRecalculation.Setup(x => x.GetTaskElementsWithInconsistentCosts(ws)).Returns(new Collection<BoeTaskElementDTO>());
            this.WsRecalculation.Setup(x => x.GetElementsWithInconsistentODCs(ws)).Returns(new Collection<OtherDirectCostDTO>());

            ICollection<BoeDiscrepancyReportModelView> result = sut.GetReport(ws);

            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Tests the GetReport method w/ full data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void GetReportTest_WithData()
        {
            BOEDiscrepancyReport sut = this.CreateSystem();
            FullWorkspace ws = new FullWorkspace() { Id = 1 };

            #region Setup Authors

            UserDTO author1 = new UserDTO()
            {
                UserID = 10,
                DisplayName = "user 1 display name"
            };

            UserDTO author2 = new UserDTO()
            {
                UserID = 20,
                DisplayName = "user 2 display name"
            };

            ICollection<UserDTO> authors = new List<UserDTO>() { author1, author2 };
            this.UserLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(authors);

            #endregion

            #region Setup Wbs Elements
            WbsDTO wbsdto1 = new WbsDTO()
            {
                Id = 1111,
                WbsNumber = "wbs 1 number",
                WbsTitle = "wbs 1 title"
            };
            WbsDTO wbsdto2 = new WbsDTO()
            {
                Id = 2222,
                WbsNumber = "wbs 2 number",
                WbsTitle = "wbs 2 title"
            };
            FullWbs wbs1 = new FullWbs(wbsdto1);

            FullWbs wbs2 = new FullWbs(wbsdto2);
           

            ICollection<FullWbs> wbsElements = new List<FullWbs>() { wbs1, wbs2 };
            this.Retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(wbsElements);
            this.Retriever.Setup(x=>x.GetWbsById(wbs1.Id)).Returns(wbsdto1);
            this.Factory.Setup(x=> x.CreateFullWbs(wbsdto1)).Returns(wbs1);
            this.Retriever.Setup(x => x.GetWbsById(wbs2.Id)).Returns(wbsdto2);
            this.Factory.Setup(x => x.CreateFullWbs(wbsdto2)).Returns(wbs2);
            #endregion

            #region Setup Clins

            ClinDTO clindt01 = new ClinDTO()
            {
                Id = 1,
                ClinNumber = "clin 1 number",
                ClinTitle = "clin 1 title"
            };
            FullClin clin1 = new FullClin(clindt01);

            ICollection<FullClin> clins = new List<FullClin>() { clin1 };
            this.Retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(clins);
            this.Retriever.Setup(x => x.GetClinById(1)).Returns(clindt01);
            this.Factory.Setup(x => x.CreateFullClin(clindt01)).Returns(clin1) ;
            #endregion

            #region Setup BOEs

            FullBoe boe1 = new FullBoe()
            {
                Id = 111,
                Title = "Boe 1 Title",
                AuthorIDs = new Collection<int>() { author1.UserID, author2.UserID },
                WBSID = wbs1.Id
            };

            FullBoe boe2 = new FullBoe()
            {
                Id = 222,
                Title = "Boe 2 Title",
                AuthorIDs = new Collection<int>() { author1.UserID },
                CLINID = clin1.Id
            };

            FullBoe boe3 = new FullBoe()
            {
                Id = 333,
                Title = "Boe 3 Title",
                AuthorIDs = new Collection<int>() { author2.UserID },
                WBSID = wbs2.Id
            };

            ICollection<FullBoe> boes = new List<FullBoe>() { boe1, boe2, boe3 };
            this.Retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(boes);

            #endregion

            #region Setup Hour Data

            BoeTaskElementDTO hour1 = new BoeTaskElementDTO()
            {
                BoeID = boe1.Id,
                Id = 101,
                BOETaskID = "task 1 id",
                TaskTitle = "task 1 title"
            };

            BoeTaskElementDTO hour2 = new BoeTaskElementDTO()
            {
                BoeID = boe2.Id,
                Id = 202,
                BOETaskID = "hour 2 task id",
                TaskTitle = "hour 2 task title"
            };

            BoeTaskElementDTO hour3 = new BoeTaskElementDTO()
            {
                BoeID = boe2.Id,
                Id = 303,
                BOETaskID = "hour 3 task id",
                TaskTitle = "hour 3 task title"
            };

            Collection<BoeTaskElementDTO> hoursWithProblems = new Collection<BoeTaskElementDTO>() { hour1, hour2, hour3 };
            this.WsRecalculation.Setup(x => x.GetTaskElementsWithNonZeroDeltaLabor(ws)).Returns(hoursWithProblems);

            #endregion

            #region Setup Cost Data

            BoeTaskElementDTO cost1 = new BoeTaskElementDTO()
            {
                BoeID = boe1.Id,
                Id = 101,
                BOETaskID = "task 1 id",
                TaskTitle = "task 1 title"
            };

            BoeTaskElementDTO cost2 = new BoeTaskElementDTO()
            {
                BoeID = boe2.Id,
                Id = 3300,
                BOETaskID = "cost 2 task id",
                TaskTitle = "cost 2 task title"
            };

            BoeTaskElementDTO cost3 = new BoeTaskElementDTO()
            {
                BoeID = boe3.Id,
                Id = 4400,
                BOETaskID = "cost 3 task id",
                TaskTitle = "cost 3 task title"
            };

            Collection<BoeTaskElementDTO> costsWithProblems = new Collection<BoeTaskElementDTO>() { cost1, cost2, cost3 };
            this.WsRecalculation.Setup(x => x.GetTaskElementsWithInconsistentCosts(ws)).Returns(costsWithProblems);

            #endregion

            #region Setup ODC Data

            OtherDirectCostDTO odc1 = new OtherDirectCostDTO()
            {
                BoeID = boe1.Id,
                Id = 555,
                BOETaskID = "odc 1 task id",
                TaskTitle = "odc 1 task title"
            };

            OtherDirectCostDTO odc2 = new OtherDirectCostDTO()
            {
                BoeID = boe2.Id,
                Id = 666,
                BOETaskID = "odc 2 task id",
                TaskTitle = "odc 2 task title"
            };

            OtherDirectCostDTO odc3 = new OtherDirectCostDTO()
            {
                BoeID = boe3.Id,
                Id = 777,
                BOETaskID = "odc 3 task id",
                TaskTitle = "odc 3 task title"
            };

            Collection<OtherDirectCostDTO> odcsWithProblems = new Collection<OtherDirectCostDTO>() { odc1, odc2, odc3 };
            this.WsRecalculation.Setup(x => x.GetElementsWithInconsistentODCs(ws)).Returns(odcsWithProblems);

            #endregion

            ICollection<BoeDiscrepancyReportModelView> result = sut.GetReport(ws);

            #region Setup Expected Data

            ICollection<BoeDiscrepancyReportModelView> expectedData = new List<BoeDiscrepancyReportModelView>()
            {
                new BoeDiscrepancyReportModelView()
                {
                    BoeId = boe1.Id,
                    BoeAuthors = "user 1 display name<br/>user 2 display name",
                    BoeTitle = boe1.Title,
                    Clin = CommonConstants.Unassigned_CLIN_Display_Text,
                    Wbs = wbs1.WbsString,
                    ElementsWithIssues = new List<BoeTaskDetailsMV>()
                    {
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.ODC,
                            DisplayedTaskId = odc1.BOETaskID,
                            TaskId = odc1.Id,
                            TaskTitle = odc1.TaskTitle
                        },
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.HoursAndCost,
                            DisplayedTaskId = hour1.BOETaskID,
                            TaskId = hour1.Id,
                            TaskTitle = hour1.TaskTitle
                        }
                    }
                },
                new BoeDiscrepancyReportModelView()
                {
                    BoeId = boe2.Id,
                    BoeAuthors = author1.DisplayName,
                    BoeTitle = boe2.Title,
                    Clin = clin1.ClinString,
                    Wbs = CommonConstants.Unassigned_WBS_Display_Text,
                    ElementsWithIssues = new List<BoeTaskDetailsMV>()
                    {
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.Cost,
                            DisplayedTaskId = cost2.BOETaskID,
                            TaskId = cost2.Id,
                            TaskTitle = cost2.TaskTitle
                        },
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.Hours,
                            DisplayedTaskId = hour2.BOETaskID,
                            TaskId = hour2.Id,
                            TaskTitle = hour2.TaskTitle
                        },
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.Hours,
                            DisplayedTaskId = hour3.BOETaskID,
                            TaskId = hour3.Id,
                            TaskTitle = hour3.TaskTitle
                        },
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.ODC,
                            DisplayedTaskId = odc2.BOETaskID,
                            TaskId = odc2.Id,
                            TaskTitle = odc2.TaskTitle
                        }
                    }
                },

                new BoeDiscrepancyReportModelView()
                {
                    BoeId = boe3.Id,
                    BoeAuthors = author2.DisplayName,
                    BoeTitle = boe3.Title,
                    Clin = CommonConstants.Unassigned_CLIN_Display_Text,
                    Wbs = wbs2.WbsString,
                    ElementsWithIssues = new List<BoeTaskDetailsMV>()
                    {
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.Cost,
                            DisplayedTaskId = cost3.BOETaskID,
                            TaskId = cost3.Id,
                            TaskTitle = cost3.TaskTitle
                        },
                        new BoeTaskDetailsMV()
                        {
                            DiscrepancyEnum = BoeInconsistencyEnum.ODC,
                            DisplayedTaskId = odc3.BOETaskID,
                            TaskId = odc3.Id,
                            TaskTitle = odc3.TaskTitle
                        }
                    }
                }
            };

            #endregion

            Assert.IsTrue(result.Count() == expectedData.Count());

            for (int i = 0; i < result.Count(); i++)
            {
                BoeDiscrepancyReportModelView resultItem = result.ElementAt(i);
                BoeDiscrepancyReportModelView expectedItem = expectedData.ElementAt(i);

                Assert.IsTrue(resultItem.BoeId == expectedItem.BoeId);
                Assert.IsTrue(resultItem.BoeAuthors == expectedItem.BoeAuthors);
                Assert.IsTrue(resultItem.BoeTitle == expectedItem.BoeTitle);
                Assert.IsTrue(resultItem.Clin == expectedItem.Clin);
                Assert.IsTrue(resultItem.Wbs == expectedItem.Wbs);
                Assert.IsTrue(resultItem.ElementsWithIssues.Count() == expectedItem.ElementsWithIssues.Count());

                for (int j = 0; j < resultItem.ElementsWithIssues.Count(); j++)
                {
                    BoeTaskDetailsMV resultElement = resultItem.ElementsWithIssues.ElementAt(j);
                    BoeTaskDetailsMV expectedElement = expectedItem.ElementsWithIssues.ElementAt(j);

                    Assert.IsTrue(resultElement.DiscrepancyEnum == expectedElement.DiscrepancyEnum);
                    Assert.IsTrue(resultElement.DisplayedTaskId == expectedElement.DisplayedTaskId);
                    Assert.IsTrue(resultElement.InconsistencyText == expectedElement.InconsistencyText);
                    Assert.IsTrue(resultElement.TaskId == expectedElement.TaskId);
                    Assert.IsTrue(resultElement.TaskTitle == expectedElement.TaskTitle);
                }
            }
        }
    }
}
