// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    /// Tests bulk insert, update, retrieve and delete of RateDetails.
    /// </summary>
    [TestClass]
    public class BulkIORateDetailTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Create sut.
        /// </summary>
        /// <returns>A RateDetailLoader.</returns>
        public RateDetailLoader CreateSut()
        {
            return this.testData.RateDetailLoader;
        }

        /// <summary>
        /// Test Upsert (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX1()
        {
            RateDetailLoader sut = this.CreateSut();
            RateDetailModelView rdmv = new RateDetailModelView()
            {
                Id = -1,
                RevisionId = -1,
                IsDeleted = false,
                RateCode = "MockRate1",
                RateCategory = RateCategory.DirectLabor,
                Description = "Mock Rate 1",
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
            RateDetailLoader sut = this.CreateSut();
            RateDetailModelView rdmv = new RateDetailModelView()
            {
                Id = -1,
                RevisionId = -1,
                IsDeleted = false,
                RateCode = "MockRate1",
                RateCategory = RateCategory.DirectLabor,
                Description = "Mock Rate 1",
                Dirty = true,
                Updateable = UpdateType.Deleted
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(rdmv);   // should cause exception - Delete method is not supported
            }
        }

        /// <summary>
        /// Test RateCodeYears Upsert (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX3()
        {
            RateCodeYearLoader sut = this.testData.RateCodeYearLoader;
            RateYearModelView rymv = new RateYearModelView()
            {
                Id = -1,
                Year = 2017,
                RateCodeId = -1,
                Dirty = true,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(rymv); // should cause exception - Upsert method is not supported
            }
        }

        /// <summary>
        /// Test RateCodeYears Delete (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX4()
        {
            RateCodeYearLoader sut = this.testData.RateCodeYearLoader;
            RateYearModelView rymv = new RateYearModelView()
            {
                Id = -1,
                Year = 2017,
                RateCodeId = -1,
                Dirty = true,
                Updateable = UpdateType.Deleted
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(rymv); // should cause exception - Delete method is not supported
            }
        }

        /// <summary>
        /// Test ProPricerRateCodeXrefLoader Upsert (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX5()
        {
            ProPricerRateCodeXrefLoader sut = this.testData.ProPricerRateCodeXrefLoader;
            ProPricerRateCodeXrefModelView pprcxmv = new ProPricerRateCodeXrefModelView()
            {
                Id = -1,
                RateCodeId = -1,
                RateCodeExtensionId = 0,
                Dirty = true,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(pprcxmv); // should cause exception - Upsert method is not supported
            }
        }

        /// <summary>
        /// Test ProPricerRateCodeXrefLoader Delete (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX6()
        {
            ProPricerRateCodeXrefLoader sut = this.testData.ProPricerRateCodeXrefLoader;
            ProPricerRateCodeXrefModelView pprcxmv = new ProPricerRateCodeXrefModelView()
            {
                Id = -1,
                RateCodeId = -1,
                RateCodeExtensionId = 0,
                Dirty = true,
                Updateable = UpdateType.Deleted
            };

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.Save(pprcxmv); // should cause exception - Delete method is not supported
            }
        }

        /// <summary>
        /// Test GetRatesForImport method.
        /// </summary>
        [TestMethod]
        public void TestGetRatesForImport()
        {
            RateDetailLoader sut = this.CreateSut();

            // Create a revision
            RevisionModelView revision = this.testData.GetRevision(true);
            Assert.IsNotNull(revision);

            // Add data (i.e. sections, rates, burden pools, etc.)
            this.testData.ModifyRevisionData1(revision);

            // Get the rates
            ICollection<RateDetailModelView> rates = this.testData.RateDetailLoader.GetRatesByRevision(revision);

            // Get the rates for import
            ICollection<RateDetailModelView> ratesForImport = sut.GetRatesForImport(revision, rates.Select(x => x.RateCode).ToArray());
            Assert.AreEqual(rates.Count, ratesForImport.Count); // verify the expected number of rows - should match number of rates in this revision

            // Get the rates for import
            RateDetailModelView rateDetail1 = rates.First(x => x.RateCode == "MockRate1");
            RateDetailModelView rateDetail2 = rates.First(x => x.RateCode == "MockRate2");

            ratesForImport = sut.GetRatesForImport(revision, new[] { rateDetail1.RateCode, rateDetail2.RateCode });
            Assert.AreEqual(2, ratesForImport.Count); // verify the expected number of rows - should match number of rate codes supplied
        }

        /// <summary>
        /// Test Bulk IO of Rate Details.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void BulkIORateDetail()
        {
            RateDetailLoader sut = this.CreateSut();
            Collection<RateDetailModelView> newRateDetails = new Collection<RateDetailModelView>();
            IRevisionMediator revisionMediator = this.testData.RevisionMediator;
            SectionLoader sectionLoader = this.testData.SectionLoader;
            RevisionModelView revision = revisionMediator.GetWipRevision();
            ICollection<SectionModelView> sections = sectionLoader.GetAll(revision);
            BurdenPoolLoader burdenPoolLoader = this.testData.BurdenPoolLoader;
            ICollection<OptionModelView> resourceClassOptions = sut.GetResourceClassOptions(revision.Id);
            OptionModelView resourceClassOption0 = resourceClassOptions.First(x => x.Id == 0);            // get "Not Selected" resource class option
            OptionModelView resourceClassOption1 = resourceClassOptions.FirstOrDefault(x => x.Id > 0);    // get first valid resource class option
            ICollection<OptionModelView> commercialBurdenPoolOptions;
            ICollection<OptionModelView> governmentBurdenPoolOptions;
            burdenPoolLoader.GetBurdenPoolOptions(revision.Id, out commercialBurdenPoolOptions, out governmentBurdenPoolOptions);
            OptionModelView commrcialBurdenPoolOption0 = commercialBurdenPoolOptions.First(x => x.Id == 0);            // get "Not Selected" burden pool option
            OptionModelView commrcialBurdenPoolOption1 = commercialBurdenPoolOptions.FirstOrDefault(x => x.Id > 0);    // get first valid burden pool option
            Assert.IsTrue(commrcialBurdenPoolOption1 != null && commrcialBurdenPoolOption1.Id > 0, "This test assumes at least one valid Commercial Burden Pool is available.");
            OptionModelView governmentBurdenPoolOption0 = governmentBurdenPoolOptions.First(x => x.Id == 0);            // get "Not Selected" burden pool option
            OptionModelView governmentBurdenPoolOption1 = governmentBurdenPoolOptions.FirstOrDefault(x => x.Id > 0);    // get first valid burden pool option
            Assert.IsTrue(governmentBurdenPoolOption1 != null && governmentBurdenPoolOption1.Id > 0, "This test assumes at least one valid Government Burden Pool is available.");

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
                    Section = sections.ElementAt(i).Id,
                    RateType = RateType.Cost,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    CommercialBurdenPoolId = commrcialBurdenPoolOption1.Id,
                    CommercialBurdenPool = commrcialBurdenPoolOption1.Label,
                    GovernmentBurdenPoolId = governmentBurdenPoolOption1.Id,
                    GovernmentBurdenPool = governmentBurdenPoolOption1.Label,
                    Updateable = UpdateType.Upsert,
                    Description = "Description Text " + i,
                    RateCategory = RateCategory.GA,
                    RateDescription = "RateDescription" + i,
                    RateDescription1 = "RateDescription1" + i,
                    RateDescription2 = "RateDescription2" + i,
                    RateDescription3 = "RateDescription3" + i,
                    RateDescription4 = "RateDescription4" + i,
                    RateDescription5 = "RateDescription5" + i,
                    RateDescription6 = "RateDescription6" + i,
                    RateDescription7 = "RateDescription7" + i,

                    ResourceClassId = resourceClassOption0.Id,
                    ResourceClass = resourceClassOption0.Label,
                    ResourceClassId1 = resourceClassOption1.Id,
                    ResourceClass1 = resourceClassOption1.Label,
                    ProPricerMappings = new Collection<ProPricerRateCodeXrefModelView>() { new ProPricerRateCodeXrefModelView() { Id = -1, Updateable = UpdateType.Upsert, Description = "PPX0", RateCodeExtensionId = null, ResourceClassId = resourceClassOption0.Id, ResourceClass = resourceClassOption0.Label, Dirty = true }, new ProPricerRateCodeXrefModelView() { Id = -2, Updateable = UpdateType.Upsert, Description = "PPX1", RateCodeExtensionId = 1, ResourceClassId = resourceClassOption1.Id, ResourceClass = resourceClassOption1.Label, Dirty = true }, new ProPricerRateCodeXrefModelView() { Id = -3, Updateable = UpdateType.Upsert, Description = "PPX2", RateCodeExtensionId = 2, Dirty = true } },
                    Values = new Collection<RateYearModelView>() { new RateYearModelView() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new RateYearModelView() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new RateYearModelView() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } }
                });
            }

            IDictionary<int, int> savedRates;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedRates = sut.BulkSave(newRateDetails);
                scope.Complete();
            }

            int[] savedRateIds = { savedRates[-1], savedRates[-2], savedRates[-3] };

            // Try to load RateDetails from DB to see that they were inserted.
            ICollection<RateDetailModelView> resultRates = this.GetTestRates(savedRateIds);

            Assert.AreEqual(3, resultRates.Count);      // 3 entities were inserted

            // retrieve the rates with only section information
            ICollection<RateSectionModelView> sectionRates = sut.GetWIPRateSections(revision).Where(r => savedRateIds.Contains(r.Id)).ToList();
            this.AssertEquality(resultRates, sectionRates);

            foreach (RateDetailModelView rdmv in resultRates)
            {
                StringAssert.Contains(rdmv.Description, "Description Text");
                rdmv.Description = "new" + rdmv.Description;
                
                // Clear RateType, ResourceType, and BurdenPoolId values
                rdmv.RateType = RateType.NotSet;
                rdmv.ResourceType = DirectRateMappingResourceType.None;
                rdmv.CommercialBurdenPoolId = commrcialBurdenPoolOption0.Id;
                rdmv.GovernmentBurdenPoolId = governmentBurdenPoolOption0.Id;
                rdmv.Updateable = UpdateType.Upsert;                    // Prepare for upsert test.

                Assert.AreEqual(3, rdmv.ProPricerMappings.Count);       // 3 Pro Pricer mappings were inserted
                Assert.AreEqual(3, rdmv.Values.Count);                  // 3 Rate Code Years were inserted

                foreach (ProPricerRateCodeXrefModelView ppx in rdmv.ProPricerMappings)
                {
                    StringAssert.Contains(ppx.Description, "PPX");

                    // Validate Resource Class values
                    switch (ppx.RateCodeExtensionId)
                    {
                        case 1:
                            StringAssert.Equals(ppx.ResourceClass, resourceClassOption1.Label);
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            StringAssert.Equals(ppx.ResourceClass, string.Empty);
                            break;
                        default:
                            StringAssert.Equals(ppx.ResourceClass, resourceClassOption0.Label);
                            break;
                    }

                    ppx.ResourceClass = resourceClassOption1.Label;
                    ppx.ResourceClassId = resourceClassOption1.Id;

                    ppx.Description = "new" + ppx.Description;
                    ppx.Updateable = UpdateType.Upsert;                    // Prepare for upsert test.
                }

                foreach (RateYearModelView ry in rdmv.Values)
                {
                    Assert.AreEqual(ry.Value, rate);
                    ry.Value = rate + 1;
                    ry.Updateable = UpdateType.Upsert;                    // Prepare for upsert test.
                }
            }

            // Test the update.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedRates = sut.BulkSave(resultRates);
                scope.Complete();
            }

            resultRates = this.GetTestRates(savedRateIds);

            foreach (RateDetailModelView rdmv in resultRates)
            {
                StringAssert.Contains(rdmv.Description, "newDescription Text");
                rdmv.Updateable = UpdateType.Deleted;                   // Prepare for delete test.

                Assert.AreEqual(3, rdmv.ProPricerMappings.Count);       // 3 Pro Pricer mappings were inserted
                Assert.AreEqual(3, rdmv.Values.Count);                  // 3 Rate Code Years were inserted

                foreach (ProPricerRateCodeXrefModelView ppx in rdmv.ProPricerMappings)
                {
                    StringAssert.Equals(ppx.ResourceClass, resourceClassOption1.Label);
                    Assert.AreEqual(resourceClassOption1.Id, ppx.ResourceClassId);
                    StringAssert.Contains(ppx.Description, "newPPX");
                    ppx.Updateable = UpdateType.Deleted;                // Prepare for delete test.
                }

                foreach (RateYearModelView ry in rdmv.Values)
                {
                    Assert.AreEqual(ry.Value, rate + 1);
                    ry.Updateable = UpdateType.Deleted;                 // Prepare for delete test.
                }
            }

            // Test the Delete.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                savedRates = sut.BulkSave(resultRates);
                scope.Complete();
            }

            resultRates = this.GetTestRates(savedRateIds);

            Assert.AreEqual(0, resultRates.Count);

            return;
        }

        /// <summary>
        /// Asserts the equality between expected rates and the RateSection Model Views..
        /// </summary>
        /// <param name="resultRates">The result rates.</param>
        /// <param name="sectionRates">The section rates.</param>
        private void AssertEquality(ICollection<RateDetailModelView> resultRates, ICollection<RateSectionModelView> sectionRates)
        {
            Assert.AreEqual(resultRates.Count, sectionRates.Count);
            for (int i = 0; i < resultRates.Count; i++)
            {
                Assert.AreEqual(resultRates.ElementAt(i).RateCode, sectionRates.ElementAt(i).RateCode);
                Assert.AreEqual(resultRates.ElementAt(i).Section, sectionRates.ElementAt(i).Section);
            }
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
                        CommercialBurdenPoolId = r.CommercialBurdenPoolID,
                        CommercialBurdenPool = r.CommercialBurdenPoolLU == null ? string.Empty : r.CommercialBurdenPoolLU.BurdenPool,
                        GovernmentBurdenPoolId = r.GovernmentBurdenPoolID,
                        GovernmentBurdenPool = r.GovernmentBurdenPoolLU == null ? string.Empty : r.GovernmentBurdenPoolLU.BurdenPool,
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
                        RateCategory = (RateCategory)r.CategoryID,
                        RateCode = r.RateCode1,                     // Entity Framework adds 1 to avoid name collision.
                        RateType = (RateType)r.RateTypeID,
                        ResourceType = (DirectRateMappingResourceType)r.ResourceTypeID,
                        RevisionId = r.RevisionID,
                        Section = r.Section.ID,
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
        /// Test GetRatesForRdsbDocument
        /// </summary>
        [TestMethod]
        public void TestGetRatesForRdsbDocument()
        {
            RateDetailLoader sut = this.CreateSut();

            RevisionModelView revision = this.testData.GetRevision(true);
            this.testData.ModifyRevisionData1(revision);

            ICollection<RdsbRateDetailModelView> results = sut.GetRatesForRdsbDocument(revision.Id);

            Assert.IsTrue(results.Any());

            // Assert that these fields are populated and not their default values
            foreach (RdsbRateDetailModelView result in results)
            {
                Assert.IsTrue(result.Id > 0);
                Assert.IsNotNull(result.RateCode);
                Assert.IsFalse(string.IsNullOrEmpty(result.Description));
                Assert.IsNotNull(result.Section);
            }
        }
    }
}
