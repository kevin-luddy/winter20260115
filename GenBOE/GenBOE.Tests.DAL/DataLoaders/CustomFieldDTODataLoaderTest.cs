// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Transactions;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class CustomFieldDTODataLoaderTest
    {
        [ClassInitialize]
        /// Since this function is defined as ClassInitialize, it will be the first function
        /// called in this entire class. Therefore, the workspace used in all of these test methods
        /// will be the same
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
        }

        [TestMethod]
        public void GetCustomFieldDTO()
        {
            var sut = new CustomFieldDTODataLoader();

            CustomFieldDTO customFieldDTOData = sut.GetById(GlobalTestCaseSetup.GlobalCustomFieldID);

            Assert.IsTrue(customFieldDTOData != null);
        }

        [TestMethod]
        public void SaveCustomFields()
        {
            var sut = new CustomFieldDTODataLoader();

            int customFieldDTOIdCount = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            CustomFieldDTO customFieldDTOData = new CustomFieldDTO();
            customFieldDTOData.Id = -1;
            customFieldDTOData.CustomFieldName = "City";
            customFieldDTOData.CustomFieldRequired = false;
            customFieldDTOData.CustomFieldDisplayID = IES.Common.CustomFieldType.BoeDisplay;
            customFieldDTOData.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            customFieldDTOData.Updateable = UpdateType.Upsert;
            customFieldDTOData.UpdateDate = DateTime.Now;

            Collection<CustomFieldDTO> customFieldDTOs = new Collection<CustomFieldDTO>{ customFieldDTOData };

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldDTOs);
                scope.Complete();
            }

            int customFieldDTOIdsAfterSaveCount = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            Assert.AreEqual(customFieldDTOIdCount + 1, customFieldDTOIdsAfterSaveCount, "Custom fields save did not work");
        }

        [TestMethod]
        public void DeleteCustomFields()
        {
            var sut = new CustomFieldDTODataLoader();

            int customFieldDTOIdCount = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            CustomFieldDTO customFieldDTOData = new CustomFieldDTO()
            {
                Id = -1,
                CustomFieldName = "State",
                CustomFieldRequired = false,
                CustomFieldDisplayID = IES.Common.CustomFieldType.BoeDisplay,
                WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID,
                Updateable = UpdateType.Upsert,
                UpdateDate = DateTime.Now
            };

            int? customFieldID;

            using (TransactionScope scope = new TransactionScope())
            {
                customFieldID = sut.Save(customFieldDTOData);
                scope.Complete();
            }

            int customFieldDTOIdsAfterSaveCount = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            Assert.AreEqual(customFieldDTOIdCount + 1, customFieldDTOIdsAfterSaveCount, "Custom fields save did not work");

            customFieldDTOData = sut.GetById(customFieldID.Value);
            customFieldDTOData.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(customFieldDTOData);
                scope.Complete();
            }

            int customFieldDTOIdsAfterDeleteCount = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;

            Assert.AreEqual(customFieldDTOIdCount, customFieldDTOIdsAfterDeleteCount, "Custom fields delete did not work");
        }
    }
}
