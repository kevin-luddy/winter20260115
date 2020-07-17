// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Transactions;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BoeDTODataLoaderTest
    {
        /// Since this function is defined as ClassInitialize, it will be the first function
        /// called in this entire class. Therefore, the workspace used in all of these test methods
        /// will be the same
        [ClassInitialize]
        public static void GetCurrentBOE(TestContext testContext)
        {
            Initialize();
        }

        private static void Initialize()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalTaskElementID();
        }

        [TestInitialize]
        public void CreateCustomFieldData()
        {
            GlobalTestCaseSetup.CreateCustomField(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateCustomFieldValue();
            GlobalTestCaseSetup.CreateBOECustomFieldValueXref();
            GlobalTestCaseSetup.CreateTaskElementCustomFieldValueXref();
            GlobalTestCaseSetup.CreateLaborTypeCustomFieldValueXref();
        }

        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private BoeDTODataLoader CreateTestLoader()
        {
            return new BoeDTODataLoader();
        }

        /// <summary>
        /// Tests the GetByWorkspaceId method.
        /// </summary>
        [TestMethod]
        public void L_GetBoesByWorkspaceID()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            int countFromDbDirectly = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                countFromDbDirectly = gbe.BOEs.Where(x => x.WorkspaceID == GlobalTestCaseSetup.GlobalWorkspaceID).Count();
            }

            ICollection<BoeDTO> boes = loader.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            // There should be 1 BOE
            Assert.IsTrue(boes.Any());
            Assert.IsTrue(boes.Count() == countFromDbDirectly);

            foreach (BoeDTO boe in boes)
            {
                // Check the BOE returned is the test BOE
                Assert.IsTrue(boe.WorkspaceID == GlobalTestCaseSetup.GlobalWorkspaceID);
            }
        }

        /// <summary>
        /// Tests the GetByIds method.
        /// </summary>
        [TestMethod]
        public void L_GetBoesByIds()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            ICollection<BoeDTO> boes = loader.GetByIds(new List<int>() { GlobalTestCaseSetup.GlobalBOEID });

            // There should be 1 BOE
            BoeDTO boe = boes.Single();
            Assert.IsTrue(boe.WorkspaceID == GlobalTestCaseSetup.GlobalWorkspaceID);
        }

        [TestMethod]
        public void L_CreateorSaveBOEHeader()
        {
            // Arrange
            BoeDTODataLoader sut = this.CreateTestLoader();

            // Act - test the Create option first

            BoeDTO header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();

            // get the header so we have the correct update dt and then adjust the description
            header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();
            string nameChange = Guid.NewGuid().ToString();
            header.Description = nameChange;
            header.Updateable = UpdateType.Upsert;
            header.HistoricMetricDisclosureChecked = false;
            header.UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(header);
                scope.Complete();
            }

            // Assert
            // Get the updated BOE to see that the description has changed
            header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();
            Assert.AreEqual(header.Description, nameChange, "The BOE Header save returned an error");

        }

        /// <summary>
        /// Tests the creation of a BOE without a WBS or CLIN (fail case)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EntityCommandExecutionException))]
        public void L_CreateBOEWithoutWBSOrClinFAIL()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            BoeDTO newDTO = new BoeDTO();
            newDTO.Updateable = UpdateType.Upsert;
            newDTO.Description = "moq fail because no WBS/CLIN";
            newDTO.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            newDTO.UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID;

            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(newDTO);
                scope.Complete();
            }

            // test should throw exception during SaveBOE .. if not die
            Assert.Fail("Did not throw EntityCommandExecutionException");
        }

        /// <summary>
        /// Tests the creation of multiple BOEs
        /// </summary>
        [TestMethod]
        public void L_SaveBOEs()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            // Create a few new BOEs
            string Desc = Guid.NewGuid().ToString();
            var BoeCollection = new Collection<BoeDTO>() {
                new BoeDTO() {
                    Id = -1,
                    Description = Desc,
                    DataSource = "data source",
                    UpdateDate = DateTime.Now,
                    Updateable = UpdateType.Upsert,
                    WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID,
                    WBSID = GlobalTestCaseSetup.GlobalWBSID,
                    UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID,
                    SOW = "sow",
                    SOWTitle = "title",
                    CamName = "test",
                    Category = "cat",
                    ClassOfCost = ClassOfCost.Recurring
                },
                new BoeDTO() {
                    Id = -2,
                    Description = Desc,
                    DataSource = "data source",
                    UpdateDate = DateTime.Now,
                    Updateable = UpdateType.Upsert,
                    WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID,
                    WBSID = GlobalTestCaseSetup.GlobalWBSID,
                    UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID
                },
                new BoeDTO() {
                    Id = -3,
                    Description = Desc,
                    DataSource = "data source",
                    UpdateDate = DateTime.Now,
                    Updateable = UpdateType.Upsert,
                    WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID,
                    WBSID = GlobalTestCaseSetup.GlobalWBSID,
                    UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID
                }
            };

            // Save BOEs
            Dictionary<int, int> saveResults;
            using (TransactionScope scope = new TransactionScope())
            {
                saveResults = loader.Save(BoeCollection);
                scope.Complete();
            }

            ICollection<BoeDTO> actualDTOs = loader.GetByIds(saveResults.Values.ToList(), true);

            foreach (BoeDTO boe in BoeCollection)
            {
                boe.Id = saveResults[boe.Id];
                BoeDTO actual = actualDTOs.First(b => b.Id == boe.Id);
                boe.StartDate = boe.StartDate.Normalize();
                boe.EndDate = boe.EndDate.Normalize();
                boe.SubmitForApprovalDate = boe.SubmitForApprovalDate.Normalize(DateTimePrecision.Day);
                boe.UpdateDate = actual.UpdateDate;
                boe.UpdatedByUserId = actual.UpdatedByUserId;
                boe.WCBID = actual.WCBID;
            }

            Assert.IsTrue(saveResults.Count == 3);

            VerifyCollections(BoeCollection, actualDTOs);
        }

        /// <summary>
        /// Tests ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_SaveBOEs_ArgumentNullException()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            Collection<BoeDTO> BoeCollection = null;

            loader.Save(BoeCollection);
        }

        /// <summary>
        /// Tests GetTaskVariableIdsByBoeId
        /// </summary>
        [TestMethod]
        public void L_GetTaskVariableIdsBasedOnBoeID()
        {
            var sut = this.CreateTestLoader();
            ICollection<int> TaskVarIDs = sut.GetTaskVariableIdsByBoeId(GlobalTestCaseSetup.GlobalBOEID);
            Assert.IsTrue(TaskVarIDs.Count > 0, "No Task Variables could be found that contain the global boe id");
        }

        /// <summary>
        /// Tests GetWorkspaceVariableIdsByBoeId
        /// </summary>
        [TestMethod]
        public void L_GetWorkspaceVariableIdsBasedOnBoeID()
        {
            var sut = this.CreateTestLoader();
            ICollection<int> WorkspaceVarIDs = sut.GetWorkspaceVariableIdsByBoeId(GlobalTestCaseSetup.GlobalBOEID);
            Assert.IsTrue(WorkspaceVarIDs.Count > 0, "No Workspace Variables could be found that contain the global boe id");
        }
        
        /// <summary>
        /// Tests saving of custom field value containers
        /// </summary>
        [TestMethod]
        public void SaveBOECustomFieldValueContainers()
        {
            Initialize();
            BoeDTODataLoader loader = this.CreateTestLoader();

            // get the global boe id
            BoeDTO boe = loader.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();

            int BeforeCustomFieldContainerCount = boe.CustomFieldValueContainers.Count;

            // set up a custom field value container to save
            Collection<CustomFieldValueContainer> customFieldValueContainer = new Collection<CustomFieldValueContainer>();
            foreach (CustomFieldValueContainer customField in boe.CustomFieldValueContainers)
            {
                customFieldValueContainer.Add(customField);
            }

            CustomFieldValueContainer customFieldValueXrefDTO = new CustomFieldValueContainer();
            customFieldValueXrefDTO.ContainerID = -1;
            customFieldValueXrefDTO.CustomFieldValueID = GlobalTestCaseSetup.GlobalCustomFieldValueID;
            customFieldValueXrefDTO.Updateable = UpdateType.Upsert;
            customFieldValueContainer.Add(customFieldValueXrefDTO);

            boe.CustomFieldValueContainers = customFieldValueContainer;
            boe.Updateable = UpdateType.Upsert;
            boe.UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID;

            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(boe);
                scope.Complete();
            }

            BoeDTO SavedBoe = loader.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();

            int AfterCustomFieldContainerCount = SavedBoe.CustomFieldValueContainers.Count;

            Assert.AreEqual(BeforeCustomFieldContainerCount + 1, AfterCustomFieldContainerCount, "The save did not work");

            // Now delete a custom field value container
            loader.DeleteBoeCustomFieldValueContainer(SavedBoe.CustomFieldValueContainers.ToCollection()[0], GlobalTestCaseSetup.GlobalBOEID);

            int AfterDeleteCount = loader.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault().CustomFieldValueContainers.Count;

            Assert.AreEqual(AfterCustomFieldContainerCount - 1, AfterDeleteCount, "The delete did not work");

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        [TestMethod]
        public void L_HistoricMetricTrue()
        {

            // Arrange
            BoeDTODataLoader sut = this.CreateTestLoader();

            // Act - test the Create option first

            BoeDTO header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();

            // get the header so we have the correct update dt and then adjust the description
            header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();
            string nameChange = Guid.NewGuid().ToString();
            header.Description = nameChange;
            header.Updateable = UpdateType.Upsert;
            header.HistoricMetricDisclosureChecked = true;
            header.UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(header);
                scope.Complete();
            }

            // Assert
            // Get the updated BOE to see that the description has changed
            header = sut.GetByIds(new Collection<int> { GlobalTestCaseSetup.GlobalBOEID }).FirstOrDefault();
            Assert.AreEqual(header.Description, nameChange, "The BOE Header save returned an error");

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Test delete BOE
        /// </summary>
        [TestMethod]
        public void L_DeleteBOE()
        {
            Initialize();
            BoeDTODataLoader loader = this.CreateTestLoader();

            // Create a new boe
            string Desc = Guid.NewGuid().ToString();
            var boe = new BoeDTO();
            boe.Id = -1;
            boe.Description = Desc;
            boe.DataSource = "data source";
            boe.UpdateDate = DateTime.Now;
            boe.Updateable = UpdateType.Upsert;
            boe.CLINID = GlobalTestCaseSetup.GlobalClinID;
            boe.WBSID = GlobalTestCaseSetup.GlobalWBSID;
            boe.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            boe.UpdatedByUserId = GlobalTestCaseSetup.GlobalBOEAuthorID;

            // Save BOE first, then delete it
            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(boe);
                scope.Complete();
            }

            // Get before count
            ICollection<BoeDTO> boes = loader.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
            int BeforeDeleteCount = boes.Count;

            BoeDTO boeToDelete = new BoeDTO();
            boeToDelete = (from b in boes
                           where b.Description == Desc
                           select b).FirstOrDefault();

            boeToDelete.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                loader.Save(boeToDelete);
                scope.Complete();
            }

            int AfterDeleteCount = loader.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            Assert.AreEqual(BeforeDeleteCount - 1, AfterDeleteCount, "the delete didn't occur");

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Test contains material false
        /// </summary>
        [TestMethod]
        public void L_BoeContainsMaterial_FALSE()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            bool containsMaterial = loader.BoeContainsMaterialElement(GlobalTestCaseSetup.GlobalBOEID);

            Assert.IsFalse(containsMaterial);

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Tests contains lobor cost true
        /// </summary>
        [TestMethod]
        public void L_BoeContainsLaborCost_TRUE()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            bool containsMaterial = loader.BoeContainsLaborCostElement(GlobalTestCaseSetup.GlobalBOEID);

            Assert.IsTrue(containsMaterial);

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Tests contains labor cost false
        /// </summary>
        [TestMethod]
        public void L_BoeContainsLaborCost_FALSE()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            int boeID = GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            bool containsMaterial = loader.BoeContainsLaborCostElement(boeID);

            Assert.IsFalse(containsMaterial);

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Test get BOE by resource id
        /// </summary>
        [TestMethod]
        public void L_GetBoeIDsUsingResourceID()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            int resourceID = GlobalTestCaseSetup.GlobalResourceID;
            ICollection<int> boeIDs = loader.GetIdsByResourceId(resourceID);

            Assert.IsTrue(boeIDs.Count > 0, "No BOE children use inputted resource ID");

            GlobalTestCaseSetup.ResetGlobalBOEID();
        }

        /// <summary>
        /// Test get BOE state
        /// </summary>
        [TestMethod]
        public void L_GetBoeState()
        {
            BoeDTODataLoader loader = this.CreateTestLoader();

            var boeId = GlobalTestCaseSetup.GlobalBOEID;
            var boeIdCollection = new Collection<int>() { boeId };
            var boe = loader.GetByIds(boeIdCollection).FirstOrDefault();

            //Confirm it gets the correct state
            var state = loader.GetBoeState(boeId);
            Assert.AreEqual(boe.State, state);

            //confirm returns a state of None when the BOE doesn't exist
            state = loader.GetBoeState(-1);
            Assert.AreEqual(BOEState.None, state);
        }

        /// <summary>
        /// Tests that RTE loading works as expected
        /// </summary>
        [TestMethod]
        public void TestingRteLoadChanges()
        {
            var sut = this.CreateTestLoader();

            // A first test:
            // Check the RTE data being correctly loaded/not loaded..
            int id = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).First().Id;
            BoeDTO nonRteLoadedBoe = sut.GetByIds(new List<int>() { id }).First();

            // make sure RTE data was not loaded by default
            Assert.IsFalse(nonRteLoadedBoe.WasDataSourceSet);
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);

            // make sure Description is handled correctly
            nonRteLoadedBoe.Description = "blah blah";
            Assert.IsFalse(nonRteLoadedBoe.WasDataSourceSet);
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);

            // load RTE data, make sure it loads correctly, leaving description alone
            sut.LoadRTEFields(new List<BoeDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDataSourceSet);
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.Description == "blah blah");


            // A second test:
            // do a manual, non RTE load first, then load RTE data after, then do a full RTE load, and compare the two, to make sure it's all the same
            nonRteLoadedBoe = sut.GetByIds(new List<int>() { id }).First();
            Assert.IsFalse(nonRteLoadedBoe.WasDataSourceSet);
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            sut.LoadRTEFields(new List<BoeDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDataSourceSet);
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);

            BoeDTO rteLoadedBoe = sut.GetByIds(new List<int>() { id }, true).First();
            Assert.IsTrue(rteLoadedBoe.WasDataSourceSet);
            Assert.IsTrue(rteLoadedBoe.WasDescriptionSet);

            VerifyDtos(nonRteLoadedBoe, rteLoadedBoe);
        }

        /// <summary>
        /// Used to compare that 2 collections contain the same data
        /// </summary>
        /// <param name="boes1">First set of Boes</param>
        /// <param name="boes2">Second set of Boes</param>
        public static void VerifyCollections(ICollection<BoeDTO> boes1, ICollection<BoeDTO> boes2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(boes1.Count, boes2.Count);
            for (int i = 0; i < boes1.Count; i++)
            {
                VerifyDtos(boes1.ElementAt(i), boes2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        /// <summary>
        /// Used to compare that 2 BOEs contain the same data
        /// </summary>
        /// <param name="boe1">First Boe</param>
        /// <param name="boe2">Second Boe</param>
        public static void VerifyDtos(BoeDTO boe1, BoeDTO boe2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if (!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(boe1.Id, boe2.Id);
                Assert.AreEqual(boe1.WorkspaceID, boe2.WorkspaceID);
                Assert.AreEqual(boe1.CLINID, boe2.CLINID);
                Assert.AreEqual(boe1.WBSID, boe2.WBSID);
                Assert.AreEqual(boe1.WCBID, boe2.WCBID);
                Assert.AreEqual(boe1.State, boe2.State);
                Assert.AreEqual(boe1.SubmitForApprovalDate, boe2.SubmitForApprovalDate);
            }
            else
            {
                Assert.AreEqual(boe1.State, BOEState.Draft);
                Assert.AreEqual(boe1.SubmitForApprovalDate.Date, DateTime.MinValue.Date);
            }

            Assert.AreEqual(boe1.AuthorIDs.Count, boe2.AuthorIDs.Count);
            for (int i = 0; i < boe1.AuthorIDs.Count; i++)
            {
                Assert.AreEqual(boe1.AuthorIDs.ElementAt(i), boe2.AuthorIDs.ElementAt(i));
            }

            Assert.AreEqual(boe1.SubcontractorAuthorIDs.Count, boe2.SubcontractorAuthorIDs.Count);
            for (int i = 0; i < boe1.SubcontractorAuthorIDs.Count; i++)
            {
                Assert.AreEqual(boe1.SubcontractorAuthorIDs.ElementAt(i), boe2.SubcontractorAuthorIDs.ElementAt(i));
            }

            Assert.AreEqual(boe1.CustomFieldValueContainers.Count, boe2.CustomFieldValueContainers.Count);
            for (int i = 0; i < boe1.CustomFieldValueContainers.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).ContainerID, boe2.CustomFieldValueContainers.ElementAt(i).ContainerID);
                    Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID, boe2.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID);
                    Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).CustomFieldID, boe2.CustomFieldValueContainers.ElementAt(i).CustomFieldID);
                }

                Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).UpdateDate, boe2.CustomFieldValueContainers.ElementAt(i).UpdateDate);
                Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).IsOpenEnded, boe2.CustomFieldValueContainers.ElementAt(i).IsOpenEnded);
                Assert.AreEqual(boe1.CustomFieldValueContainers.ElementAt(i).OpenEndedValue, boe2.CustomFieldValueContainers.ElementAt(i).OpenEndedValue);
            }

            Assert.AreEqual(boe1.CopySourceBoeId, boe2.CopySourceBoeId);
            Assert.AreEqual(boe1.EndDate, boe2.EndDate);
            Assert.AreEqual(boe1.HistoricMetricDisclosureChecked, boe2.HistoricMetricDisclosureChecked);
            Assert.AreEqual(boe1.isMaterial, boe2.isMaterial);
            Assert.AreEqual(boe1.NumAuthorReassigned, boe2.NumAuthorReassigned);
            Assert.AreEqual(boe1.StartDate, boe2.StartDate);
            Assert.AreEqual(boe1.Title, boe2.Title);
            Assert.AreEqual(boe1.UpdateDate, boe2.UpdateDate);
            Assert.AreEqual(boe1.UpdatedByUserId, boe2.UpdatedByUserId);
            Assert.AreEqual(boe1.WasDataSourceSet, boe2.WasDataSourceSet);

            if (boe1.WasDataSourceSet && boe2.WasDataSourceSet)
            {
                Assert.AreEqual(boe1.DataSource, boe2.DataSource);
            }

            if (boe1.WasDescriptionSet && boe2.WasDescriptionSet)
            {
                Assert.AreEqual(boe1.Description, boe2.Description);
            }
        }
    }
}