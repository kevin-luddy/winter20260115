// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using ActionLogic;
    using DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Project Map DataLoader Tests.
    /// </summary>
    /// <seealso cref="GenBOE.Tests.DAL.DataLoaders.MOQLoaderObject" />
    [TestClass]
    public class ProjectMapDataLoaderTest : MOQLoaderObject
    {
        /// <summary>
        /// Tests modifying the Workspace DTOs.
        /// </summary>
        [TestMethod]
        public void TestProjectMapNonDiscrete()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            var sut = new ProjectMapDataLoader(new ProjectMapSpreadLoader());
            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            // Verify there is nothing in the workspace yet
            ICollection<ProjectMapModelView> models = sut.GetByWorkspaceId(workspaceId);
            Assert.AreEqual(0, models.Count);

            ICollection<ProjectMapModelView> expectedModels = ProjectMapConverterTest.GetProjectMapModelViews();
            expectedModels.AsParallel().ForAll(m => m.DiscreteMonths = null);
            expectedModels.AsParallel().ForAll(m => m.Updateable = UpdateType.Upsert);
            expectedModels.AsParallel().ForAll(m => m.WorkspaceId = workspaceId);

            // Test out the bulk save
            using (TransactionScope scope = new TransactionScope())
            {
                sut.BulkSave(expectedModels);
                scope.Complete();
            }

            // Test out retrieval
            models = sut.GetByWorkspaceId(workspaceId);
            AssertAreEqual(expectedModels, models, workspaceId);

            // Test out deletion
            WorkspaceDTO ws = new WorkspaceDTODataLoader().GetById(workspaceId);
            sut.DeleteAllInWs(workspaceId, ws.UpdateDate);
            models = sut.GetByWorkspaceId(workspaceId);
            Assert.AreEqual(0, models.Count);
        }

        /// <summary>
        /// Tests modifying the Workspace DTOs.
        /// </summary>
        [TestMethod]
        public void TestProjectMapDiscrete()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            var sut = new ProjectMapDataLoader(new ProjectMapSpreadLoader());
            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            // Verify there is nothing in the workspace yet
            ICollection<ProjectMapModelView> models = sut.GetByWorkspaceId(workspaceId);
            Assert.AreEqual(0, models.Count);

            ICollection<ProjectMapModelView> expectedModels = ProjectMapConverterTest.GetProjectMapModelViews();
            expectedModels.AsParallel().ForAll(m => m.Updateable = UpdateType.Upsert);
            expectedModels.AsParallel().ForAll(m => m.WorkspaceId = workspaceId);

            // Test out the bulk save
            using (TransactionScope scope = new TransactionScope())
            {
                sut.BulkSave(expectedModels);
                scope.Complete();
            }

            var wsSut = new WorkspaceDTODataLoader();
            WorkspaceDTO ws = wsSut.GetById(workspaceId);
            ws.ContractStartDate = new DateTime(expectedModels.Min(m => m.StartDate).Value.Year, 1, 15).Normalize();
            wsSut.SaveWorkspaceSettings(Author.UserID, ws);

            // Test out retrieval
            models = sut.GetByWorkspaceId(workspaceId);
            AssertAreEqual(expectedModels, models, workspaceId);

            // Test out paged retrieval (page size is 2)
            ProjectMapPageModelView firstPage = sut.GetProjectMapPagedData(workspaceId, 1);
            ProjectMapPageModelView secondPage = sut.GetProjectMapPagedData(workspaceId, 2);

            Assert.AreEqual(expectedModels.Count(), firstPage.TotalRows);
            Assert.AreEqual(expectedModels.Sum(m => m.Hours), firstPage.TotalHours);
            Assert.AreEqual(expectedModels.Sum(m => m.Dollars), firstPage.TotalDollars);
            Assert.AreEqual(expectedModels.Count(), secondPage.TotalRows);
            Assert.AreEqual(expectedModels.Sum(m => m.Hours), secondPage.TotalHours);
            Assert.AreEqual(expectedModels.Sum(m => m.Dollars), secondPage.TotalDollars);

            Assert.AreEqual(2, firstPage.Data.Count());
            Assert.AreEqual(2, secondPage.Data.Count());

            Assert.AreEqual(expectedModels.First().LegacyID, firstPage.Data.First().LegacyID); // using LegacyID since that is unique in the expected models
            Assert.AreEqual(expectedModels.ElementAt(2).LegacyID, secondPage.Data.First().LegacyID); // using LegacyID since that is unique in the expected models
            Assert.AreEqual(expectedModels.First().DiscreteMonths[0], firstPage.Data.First().DiscreteMonths[0]);

            // Test out deletion
            ws = wsSut.GetById(workspaceId);
            sut.DeleteAllInWs(workspaceId, ws.UpdateDate);
            models = sut.GetByWorkspaceId(workspaceId);
            Assert.AreEqual(0, models.Count);
        }

        /// <summary>
        /// Asserts the project map models are equal.
        /// </summary>
        /// <param name="expectedModels">The expected models.</param>
        /// <param name="models">The models.</param>
        private void AssertAreEqual(ICollection<ProjectMapModelView> expectedModels, ICollection<ProjectMapModelView> actualModels, int workspaceId)
        {
            Assert.AreEqual(expectedModels.Count, actualModels.Count);
            for (int i = 0; i < expectedModels.Count; i++)
            {
                ProjectMapModelView expected = expectedModels.ElementAt(i);
                ProjectMapModelView actual = actualModels.ElementAt(i);
                Assert.AreEqual(expected.ActivityID, actual.ActivityID);
                Assert.AreEqual(expected.ActivityName, actual.ActivityName);
                Assert.AreEqual(expected.AddDelete, actual.AddDelete);
                Assert.AreEqual(expected.CamName, actual.CamName);
                Assert.AreEqual(expected.Category, actual.Category);
                Assert.AreEqual(expected.ClassOfCost, actual.ClassOfCost);
                Assert.AreEqual(expected.TieredPercentage, actual.TieredPercentage);
                Assert.AreEqual(expected.Clin, actual.Clin);
                Assert.AreEqual(expected.CostCenter, actual.CostCenter);
                Assert.AreEqual(expected.Dollars, actual.Dollars);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.Hours, actual.Hours);
                Assert.AreEqual(expected.InitialResource, actual.InitialResource);
                Assert.AreEqual(expected.Offload, actual.Offload);
                Assert.AreEqual(expected.LegacyID, actual.LegacyID);
                Assert.AreEqual(expected.Rationale, actual.Rationale);
                Assert.AreEqual(expected.SowNumber, actual.SowNumber);
                Assert.AreEqual(expected.SowTitle, actual.SowTitle);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.Task, actual.Task);
                Assert.AreEqual(expected.WbsElementTitle, actual.WbsElementTitle);
                Assert.AreEqual(expected.WbsNumber, actual.WbsNumber);
                Assert.AreEqual(workspaceId, actual.WorkspaceId);

                if (expected.DiscreteMonths == null)
                {
                    Assert.IsNull(actual.DiscreteMonths);
                }
                else
                {
                    Assert.AreEqual(expected.DiscreteMonths.Length, actual.DiscreteMonths.Length);
                    for (int j = 0; j < expected.DiscreteMonths.Length; j++)
                    {
                        Assert.AreEqual(expected.DiscreteMonths[j], actual.DiscreteMonths[j]);
                    }
                }
            }
        }
    }
}
