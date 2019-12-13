// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.IO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using IES.Models;
    using IES.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests bulk update of Cobra Mapping Details.
    /// </summary>
    [TestClass]
    public class CobraDetailLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Create sut.
        /// </summary>
        /// <returns>A CobraDetailLoader.</returns>
        public CobraDetailLoader CreateSut()
        {
            return this.testData.CobraDetailLoader;
        }

        /// <summary>
        /// Test Upsert (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX1()
        {
            CobraDetailLoader sut = this.CreateSut();
            CobraDetailModelView rdmv = new CobraDetailModelView()
            {
                Id = -1,
                RevisionId = -1,
                RateCode = "MockRate1",
                Description = "Mock Rate 1",
                Code1 = Code1.INDIRECT,
                Code1Description = Code1.INDIRECT.GetDescription(),
                RateSet = "MockRateSet1",
                Dirty = true,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(rdmv); // should cause exception - Upsert method is not supported
            }
        }

        /// <summary>
        /// Test Delete (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX2()
        {
            CobraDetailLoader sut = this.CreateSut();
            CobraDetailModelView rdmv = new CobraDetailModelView()
            {
                Id = -1,
                RevisionId = -1,
                RateCode = "MockRate1",
                Description = "Mock Rate 1",
                Code1 = Code1.INDIRECT,
                Code1Description = Code1.INDIRECT.GetDescription(),
                RateSet = "MockRateSet1",
                Dirty = true,
                Updateable = UpdateType.Deleted
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(rdmv);   // should cause exception - Delete method is not supported
            }
        }

        /// <summary>
        /// Test Bulk IO of Cobra Details.
        /// </summary>
        [TestMethod]
        public void CobraDetailLoader()
        {
            CobraDetailLoader sut = this.CreateSut();
            Collection<CobraDetailModelView> newCobraDetails = new Collection<CobraDetailModelView>();
            Collection<RateDetailModelView> newRateDetails = new Collection<RateDetailModelView>();
            IRevisionMediator revisionMediator = this.testData.RevisionMediator;
            RevisionModelView revision = revisionMediator.GetWipRevision();
            ICollection<OptionModelView> resourceClassOptions = this.testData.RateDetailLoader.GetResourceClassOptions(revision.Id);
            OptionModelView resourceClassOption0 = resourceClassOptions.First(x => x.Id == 0);            // get "Not Selected" resource class option
            OptionModelView resourceClassOption1 = resourceClassOptions.FirstOrDefault(x => x.Id > 0);    // get first valid resource class option

            decimal rate = 0;
            for (int i = 0; i < 3; i++)
            {
                // Insert test rates.
                rate = 1.1m;
                newRateDetails.Add(new RateDetailModelView()
                {
                    Id = -1 - i,
                    RevisionId = revision.Id,
                    RateCode = "TestRateCode" + i,
                    RateType = RateType.Cost,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Updateable = UpdateType.Upsert,
                    Description = "Description Text " + i,
                    RateCategory = RateCategory.GA,
                    RateDescription = "RateDescription" + i,

                    ResourceClassId = resourceClassOption0.Id,
                    ResourceClass = resourceClassOption0.Label,
                    ResourceClassId1 = resourceClassOption1.Id,
                    ResourceClass1 = resourceClassOption1.Label,
                    Values = new Collection<RateYearModelView>() { new RateYearModelView() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new RateYearModelView() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new RateYearModelView() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } }
                });
            }

            IDictionary<int, int> savedRates;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedRates = this.testData.RateDetailLoader.BulkSave(newRateDetails);
                scope.Complete();
            }

            int[] savedRateIds = { savedRates[-1], savedRates[-2], savedRates[-3] };

            // Try to load Rate and Cobra Details from DB to see that they were inserted.
            ICollection<RateDetailModelView> resultRates = this.GetTestRates(savedRateIds);
            ICollection<CobraDetailModelView> resultCobraDetails = this.GetTestCobraDetails(savedRateIds);

            Assert.AreEqual(3, resultRates.Count);          // 3 entities were inserted

            // verify the Cobra Details are empty
            Assert.AreEqual(3, resultCobraDetails.Count);   
            foreach (CobraDetailModelView cdmv in resultCobraDetails)
            {
                Assert.IsTrue(string.IsNullOrWhiteSpace(cdmv.RateSet));
                Assert.IsTrue(!cdmv.Code1.HasValue);
                Assert.IsTrue(string.IsNullOrWhiteSpace(cdmv.Code1Description));
            }

            // Update Cobra Mapping data
            foreach (RateDetailModelView resultRate in resultRates)
            {
                Code1 code1 = resultRate.Id % 2 == 0 ? Code1.INDIRECT : Code1.SVCCTR;
                newCobraDetails.Add(new CobraDetailModelView {
                    Id = resultRate.Id,
                    UpdateDate = resultRate.UpdateDate,
                    Code1 = code1,
                    Code1Description = code1.GetDescription(),
                    RateSet = "MockRateSet" + resultRate.Id,
                    Updateable = UpdateType.Upsert
                });
            }

            // Test the update.
            IDictionary<int, int> savedCobraDetails;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedCobraDetails = sut.BulkSave(newCobraDetails);
                scope.Complete();
            }

            resultCobraDetails = this.GetTestCobraDetails(savedRateIds);

            foreach (CobraDetailModelView cdmv in resultCobraDetails)
            {
                StringAssert.StartsWith(cdmv.RateSet, "MockRateSet");
                Code1? expectedCode1 = cdmv.Id % 2 == 0 ? Code1.INDIRECT : Code1.SVCCTR;
                Assert.AreEqual(expectedCode1, cdmv.Code1);
                Assert.AreEqual(expectedCode1.GetDescription(), cdmv.Code1Description);
            }

            // Delete the test data
            foreach (RateDetailModelView rdmv in resultRates)
            {
                rdmv.Updateable = UpdateType.Deleted;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedRates = this.testData.RateDetailLoader.BulkSave(resultRates);
                scope.Complete();
            }

            resultRates = this.GetTestRates(savedRateIds);

            Assert.AreEqual(0, resultRates.Count);

            return;
        }

        /// <summary>
        /// Reload the test Rate Details.
        /// </summary>
        /// <param name="savedRateIds">Result Rate Detail Ids from the insert.</param>
        /// <returns>Collection of the test Rate Details.</returns>
        private ICollection<RateDetailModelView> GetTestRates(int[] savedRateIds)
        {
            ICollection<RateDetailModelView> resultRates = null;
            int id1 = savedRateIds[0];
            int id2 = savedRateIds[1];
            int id3 = savedRateIds[2];

            using (IESEntities context = new IESEntities())
            {
                resultRates = context.RateCodes.Where(r => r.ID == id1 || r.ID == id2 || r.ID == id3)
                    .OrderBy(y => y.CategoryID)
                    .Select(r =>
                    new RateDetailModelView()
                    {
                        Id = r.ID,
                        Description = r.Description,
                        ProPricerMappings = r.ProPricerRateCodeXrefs.Select(m =>
                            new ProPricerRateCodeXrefModelView()
                            {
                                Id = m.ID,
                                UpdateDate = m.UpdateDate,
                                RateCodeId = m.RateCodeID,
                                Description = m.Description,
                                RateCodeExtensionId = m.RateCodeExtensionID,
                                ResourceClassId = m.ResourceClassID,
                                ResourceClass = m.ResourceClassLU == null ? string.Empty : m.ResourceClassLU.Description
                            }).ToList(),
                        RateCode = r.RateCode1,                     // Entity Framework adds 1 to avoid name collision.
                        RevisionId = r.RevisionID,
                        UpdateDate = r.UpdateDate,
                        Values = r.RateCodeYears.AsQueryable().OrderBy(y => y.Year).Select(m =>
                            new RateYearModelView()
                            {
                                Id = m.ID,
                                UpdateDate = m.UpdateDate,
                                RateCodeId = m.RateCodeID,
                                Year = m.Year,
                                Value = m.Rate
                            }).ToList()
                    }).ToList();
            }

            return resultRates;
        }

        /// <summary>
        /// Reload the test Cobra Details.
        /// </summary>
        /// <param name="savedRateIds">Result Cobra Detail Ids from the insert.</param>
        /// <returns>Collection of the test Cobra Details.</returns>
        private ICollection<CobraDetailModelView> GetTestCobraDetails(int[] savedRateIds)
        {
            ICollection<CobraDetailModelView> resultCobraDetails = null;
            int id1 = savedRateIds[0];
            int id2 = savedRateIds[1];
            int id3 = savedRateIds[2];

            using (IESEntities context = new IESEntities())
            {
                resultCobraDetails = context.RateCodes.Where(r => r.ID == id1 || r.ID == id2 || r.ID == id3)
                    .OrderBy(y => y.CategoryID)
                    .Select(r =>
                    new CobraDetailModelView()
                    {
                        Id = r.ID,
                        Description = r.Description,
                        Code1 = (Code1)r.CobraCode1ID,
                        RateSet = r.CobraRateSet,
                        RateCode = r.RateCode1,                     // Entity Framework adds 1 to avoid name collision.
                        RevisionId = r.RevisionID,
                        UpdateDate = r.UpdateDate
                    }).ToList();
            }

            return resultCobraDetails;
        }
    }
}
