// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test Class for MOQ Type Data Loader
    /// </summary>
    [TestClass]
    public class MoqTypeDataLoaderTest
    {
        /// <summary>
        /// Class initialize to set up global test case setup for using workspace/boe/task ids
        /// </summary>
        /// <param name="testContext"></param>
        [ClassInitialize]
        public static void InitializeGlobalTestCaseSetup(TestContext testContext)
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalTaskElementID();
        }
                
        /// <summary>
        /// This method tests the GetByIds method as well as Upsert and Delete
        /// </summary>
        [TestMethod]
        public void GetByIdsTest()
        {
            IMoqTypeDataLoader sut = new MoqTypeDataLoader();

            MoqTypeSelection moqTypeSelectionToSave = CreateTestMoqTypeSelection();

            moqTypeSelectionToSave.Updateable = UpdateType.Upsert;

            // Call upsert via Save
            int? savedMoqTypeId = sut.Save(moqTypeSelectionToSave);

            Assert.IsNotNull(savedMoqTypeId);

            // Call GetByIds via GetById
            MoqTypeSelection savedMoqTypeSelection = sut.GetById(savedMoqTypeId.Value);

            this.AssertMoqTypeSelection(moqTypeSelectionToSave, savedMoqTypeSelection, savedMoqTypeId.Value);
            
            // Test delete
            savedMoqTypeSelection.Updateable = UpdateType.Deleted;
            int? deletedMoqTypeId = sut.Save(savedMoqTypeSelection);

            savedMoqTypeSelection = sut.GetById(savedMoqTypeId.Value);
            Assert.IsNull(savedMoqTypeSelection);
        }

        /// <summary>
        /// This method tests the GetByWorkspaceId method as well as Upsert and Delete
        /// </summary>
        [TestMethod]
        public void GetByWorkspaceIdTest()
        {
            IMoqTypeDataLoader sut = new MoqTypeDataLoader();

            MoqTypeSelection moqTypeSelectionToSave = CreateTestMoqTypeSelection();
            moqTypeSelectionToSave.Updateable = UpdateType.Upsert;

            // save test data
            int? savedMoqTypeId = sut.Save(moqTypeSelectionToSave);

            Assert.IsNotNull(savedMoqTypeId);

            ICollection <MoqTypeSelection> result = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            Assert.AreEqual(1, result.Count);

            MoqTypeSelection savedMoqTypeSelection = result.First();
            this.AssertMoqTypeSelection(moqTypeSelectionToSave, savedMoqTypeSelection, savedMoqTypeId.Value);

            // delete test data
            savedMoqTypeSelection.Updateable = UpdateType.Deleted;
            int? deletedMoqTypeId = sut.Save(savedMoqTypeSelection);

            savedMoqTypeSelection = sut.GetById(savedMoqTypeId.Value);
            Assert.IsNull(savedMoqTypeSelection);
        }

        /// <summary>
        /// This method tests the GetByBoeId method as well as Upsert and Delete
        /// </summary>
        [TestMethod]
        public void GetByBoeIdTest()
        {
            IMoqTypeDataLoader sut = new MoqTypeDataLoader();

            MoqTypeSelection moqTypeSelectionToSave = CreateTestMoqTypeSelection();
            moqTypeSelectionToSave.Updateable = UpdateType.Upsert;

            // save test data
            int? savedMoqTypeId = sut.Save(moqTypeSelectionToSave);

            Assert.IsNotNull(savedMoqTypeId);

            ICollection<MoqTypeSelection> result = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID);

            Assert.AreEqual(1, result.Count);

            MoqTypeSelection savedMoqTypeSelection = result.First();
            this.AssertMoqTypeSelection(moqTypeSelectionToSave, savedMoqTypeSelection, savedMoqTypeId.Value);

            // delete test data
            savedMoqTypeSelection.Updateable = UpdateType.Deleted;
            int? deletedMoqTypeId = sut.Save(savedMoqTypeSelection);

            savedMoqTypeSelection = sut.GetById(savedMoqTypeId.Value);
            Assert.IsNull(savedMoqTypeSelection);
        }

        /// <summary>
        /// Create a MOQ Type Selection to save for testing
        /// </summary>
        /// <returns>MOQ Type Selection</returns>
        private MoqTypeSelection CreateTestMoqTypeSelection()
        {
            MoqTypeSelection moqTypeSelectionToSave = new MoqTypeSelection()
            {
                Id = -1,
                TaskId = GlobalTestCaseSetup.GlobalTaskElementID,
                SelectedMOQType = MOQType.AnalogousRelationships,
                CerName = "test name",
                CerLocation = "test location",
                DescriptionHoursRequired = "test desc",
                SmeReason = "test reason",
                SmeHoursLogic = "test hours logic",
                SmeDurationLogic = "test duration logic",
                SmeTaskEstimates = "test task estimates",
                Rationale = "test rationale",
                SkillMixRationale = "test skill mix",
                BoeId = GlobalTestCaseSetup.GlobalBOEID
            };

            MoqTableData moqTableData1 = new MoqTableData()
            {
                Id = -1,
                TableName = "test table 1",
                RepositoryName = "test repo 1",
                QueryType = "query type 1",
                DateOfReport = DateTime.Now,
                HistoricalProgramName = "test name 1",
                ContractNumber = "test contract 1",
                WbsElement = "test wbs 1",
                PoPStart = DateTime.Now.AddDays(-1),
                PoPEnd = DateTime.Now.AddDays(1),
                TotalWbsHours = 100,
                AdditionalQueryFilters = "test filters 1",
                TotalRelevantHours = 50
            };

            MoqTableData moqTableData2 = new MoqTableData()
            {
                Id = -1,
                TableName = "test table 2",
                RepositoryName = "test repo 2",
                QueryType = "query type 2",
                DateOfReport = DateTime.Now.AddDays(2),
                HistoricalProgramName = "test name 2",
                ContractNumber = "test contract 2",
                WbsElement = "test wbs 2",
                PoPStart = DateTime.Now.AddDays(-3),
                PoPEnd = DateTime.Now.AddDays(3),
                TotalWbsHours = 200,
                AdditionalQueryFilters = "test filters 2",
                TotalRelevantHours = 150
            };

            moqTypeSelectionToSave.TableData.Add(moqTableData1);
            moqTypeSelectionToSave.TableData.Add(moqTableData2);

            return moqTypeSelectionToSave;
        }

        /// <summary>
        /// Perform Asserts for MOQ Type Selection and MOQ Type Table Data
        /// </summary>
        /// <param name="expectedMoqTypeSelection">Expected MOQ Type Selection</param>
        /// <param name="resultMoqTypeSelection">Result MOQ Type Selection</param>
        /// <param name="expectedMoqTypeId">Expected MOQ Type Selection ID</param>
        private void AssertMoqTypeSelection(MoqTypeSelection expectedMoqTypeSelection, MoqTypeSelection resultMoqTypeSelection, int expectedMoqTypeId)
        {
            // Assert MOQ Type Selection
            Assert.AreEqual(expectedMoqTypeId, resultMoqTypeSelection.Id);
            Assert.AreEqual(expectedMoqTypeSelection.TaskId, resultMoqTypeSelection.TaskId);
            Assert.AreEqual(expectedMoqTypeSelection.SelectedMOQType, resultMoqTypeSelection.SelectedMOQType);
            Assert.AreEqual(expectedMoqTypeSelection.Order, resultMoqTypeSelection.Order);
            Assert.AreEqual(expectedMoqTypeSelection.CerName, resultMoqTypeSelection.CerName);
            Assert.AreEqual(expectedMoqTypeSelection.CerLocation, resultMoqTypeSelection.CerLocation);
            Assert.AreEqual(expectedMoqTypeSelection.DescriptionHoursRequired, resultMoqTypeSelection.DescriptionHoursRequired);
            Assert.AreEqual(expectedMoqTypeSelection.SmeReason, resultMoqTypeSelection.SmeReason);
            Assert.AreEqual(expectedMoqTypeSelection.SmeHoursLogic, resultMoqTypeSelection.SmeHoursLogic);
            Assert.AreEqual(expectedMoqTypeSelection.SmeDurationLogic, resultMoqTypeSelection.SmeDurationLogic);
            Assert.AreEqual(expectedMoqTypeSelection.Rationale, resultMoqTypeSelection.Rationale);
            Assert.AreEqual(expectedMoqTypeSelection.SkillMixRationale, resultMoqTypeSelection.SkillMixRationale);
            Assert.AreEqual(expectedMoqTypeSelection.BoeId, resultMoqTypeSelection.BoeId);

            // Assert Table Data
            Assert.AreEqual(expectedMoqTypeSelection.TableData.Count, resultMoqTypeSelection.TableData.Count);
            for (int i = 0; i < expectedMoqTypeSelection.TableData.Count; i++)
            {
                MoqTableData expected = expectedMoqTypeSelection.TableData.ElementAt(i);
                MoqTableData result = resultMoqTypeSelection.TableData.ElementAt(i);

                Assert.IsTrue(result.Id > 0);
                Assert.AreEqual(expectedMoqTypeId, result.MOQTypeSelectionId);
                Assert.AreEqual(expected.Order, result.Order);
                Assert.AreEqual(expected.TableName, result.TableName);
                Assert.AreEqual(expected.RepositoryName, result.RepositoryName);
                Assert.AreEqual(expected.QueryType, result.QueryType);
                Assert.AreEqual(expected.DateOfReport, result.DateOfReport);
                Assert.AreEqual(expected.HistoricalProgramName, result.HistoricalProgramName);
                Assert.AreEqual(expected.ContractNumber, result.ContractNumber);
                Assert.AreEqual(expected.WbsElement, result.WbsElement);
                Assert.AreEqual(expected.PoPStart, result.PoPStart);
                Assert.AreEqual(expected.PoPEnd, result.PoPEnd);
                Assert.AreEqual(expected.TotalWbsHours, result.TotalWbsHours);
                Assert.AreEqual(expected.AdditionalQueryFilters, result.AdditionalQueryFilters);
                Assert.AreEqual(expected.TotalRelevantHours, result.TotalRelevantHours);
            }
        }
    }
}
