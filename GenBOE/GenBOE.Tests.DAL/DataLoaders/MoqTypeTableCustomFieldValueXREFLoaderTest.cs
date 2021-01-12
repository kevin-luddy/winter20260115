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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Test Class for the MOQ Type Table Custom Field XREF Loader
    /// </summary>
    [TestClass]
    public class MoqTypeTableCustomFieldValueXREFLoaderTest
    {
        /// <summary>
        /// Initialize the test
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalTaskElementID();
            GlobalTestCaseSetup.CreateCustomField(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalMoqTypeSelectionId();
            GlobalTestCaseSetup.CreateGlobalMoqTypeTableId();
        }

        /// <summary>
        /// Create SUT
        /// </summary>
        /// <returns>SUT</returns>
        private MoqTypeTableCustomFieldValueXREFLoader CreateSut()
        {
            return new MoqTypeTableCustomFieldValueXREFLoader();
        }

        /// <summary>
        /// Main test method to test upserting, deleting, and retrieving data
        /// </summary>
        [TestMethod]
        public void UpsertDeleteGetByIdsTest()
        {
            MoqTypeTableCustomFieldValueXREFLoader sut = this.CreateSut();

            // Test Upsert
            CustomFieldValueContainer cfValueContainer = new CustomFieldValueContainer()
            {
                ContainerID = -1,
                CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID,
                CustomFieldValueID = -1, //GlobalTestCaseSetup.GlobalCustomFieldValueID,
                OwnerID = GlobalTestCaseSetup.GlobalMoqTypeTableId,
                IsOpenEnded = true,
                OpenEndedValue = "test",
                Updateable = UpdateType.Upsert
            };

            ICollection<int?> resultIds = sut.SaveMoqTypeTableCustomFieldValueContainers(new Collection<CustomFieldValueContainer>() { cfValueContainer }, GlobalTestCaseSetup.GlobalMoqTypeTableId);

            Assert.AreEqual(1, resultIds.Count);
            Assert.IsNotNull(resultIds.FirstOrDefault());
            Assert.IsTrue(resultIds.First() > 0);

            // Test GetByIds
            ICollection<CustomFieldValueContainer> resultCFValueContainers = sut.GetByIds(resultIds.Select(x => x.Value).ToCollection());

            Assert.AreEqual(1, resultCFValueContainers.Count);
            Assert.IsNotNull(resultCFValueContainers.FirstOrDefault());

            CustomFieldValueContainer resultCFVC = resultCFValueContainers.First();
            Assert.AreEqual(cfValueContainer.CustomFieldID, resultCFVC.CustomFieldID);
            Assert.AreEqual(cfValueContainer.CustomFieldValueID, resultCFVC.CustomFieldValueID);
            // TODO other asserts?

            // Test Delete
            resultCFVC.Updateable = UpdateType.Deleted;
            sut.SaveMoqTypeTableCustomFieldValueContainer(resultCFVC, GlobalTestCaseSetup.GlobalMoqTypeTableId);

            resultCFValueContainers = sut.GetByIds(resultIds.Select(x => x.Value).ToCollection());

            Assert.IsFalse(resultCFValueContainers.Any());
        }
    }
}
