// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using ActionLogic.Core.Mediator;
	using DataBridge.Loaders;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Models;
	using IES.DataBridge.ModelViews;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// The data created during tests
	/// </summary>
	public class TestData
    {

        /// <summary>
        /// Random Number Generator
        /// </summary>
        private static Random randomNumberGenerator = new((int)DateTime.Now.Ticks);

        #region Class Information

        /// <summary>
        /// the current instance
        /// </summary>
        private static TestData currentInstance;

        /// <summary>
        /// Can't create an object, must use GetInstance
        /// </summary>
        private TestData()
        {
        }

        private static IRevisionMediator revisionMediator = new RevisionMediator(Mock.Of<ILogger<RevisionMediator>>(), new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>()), new NonCacheDataLoader(Mock.Of<ILogger<NonCacheDataLoader>>()));

        /// <summary>
        /// RateConfig Loader
        /// </summary>
        private static RateConfigLoader rateConfigLoader = new(Mock.Of<ILogger<RateConfigLoader>>());

        /// <summary>
        /// Revision Loader
        /// </summary>
        private static RevisionLoader revisionLoader = new(Mock.Of<ILogger<RevisionLoader>>());

        /// <summary>
        /// Burden Pool loader
        /// </summary>
        private static BurdenPoolLoader burdenPoolLoader = new(Mock.Of<ILogger<BurdenPoolLoader>>());

		/// <summary>
		/// Section Loader
		/// </summary>
		private static SectionLoader sectionLoader = new(Mock.Of<ILogger<SectionLoader>>());

		/// <summary>
		/// Cobra Detail loader
		/// </summary>
		private static CobraDetailLoader cobraDetailLoader = new(Mock.Of<ILogger<CobraDetailLoader>>());

        /// <summary>
        /// Cobra Years loader
        /// </summary>
        private static CobraYearsLoader cobraYearsLoader = new(Mock.Of<ILogger<CobraYearsLoader>>());

        /// <summary>
        /// Rate Code Year loader
        /// </summary>
        private static RateCodeYearLoader rateCodeYearLoader = new(Mock.Of<ILogger<RateCodeYearLoader>>());

        /// <summary>
        /// ProPricer Rate Code Xref loader
        /// </summary>
        private static ProPricerRateCodeXrefLoader proPricerXrefLoader = new(Mock.Of<ILogger<ProPricerRateCodeXrefLoader>>());

        /// <summary>
        /// Rate Detail loader
        /// </summary>
        private static RateDetailLoader rateDetailLoader = new(Mock.Of<ILogger<RateDetailLoader>>(), rateCodeYearLoader, proPricerXrefLoader);

        /// <summary>
        /// The file attachment loader
        /// </summary>
        private static FileAttachmentLoader fileAttachmentLoader = new(Mock.Of<ILogger<FileAttachmentLoader>>());

        /// <summary>
        /// The banner loader
        /// </summary>
        private static IBannerLoader bannerLoader = new BannerLoader(Mock.Of<ILogger<BannerLoader>>());

        /// <summary>
        /// The rate code replication loader
        /// </summary>
        private static IRateCodeReplicationLoader rateCodeReplicationLoader = new RateCodeReplicationLoader(Mock.Of<ILogger<RateCodeReplicationLoader>>());

        /// <summary>
        /// Burden Element Mappings 1
        /// </summary>
        private readonly string[] burdenElementRateCodeMappings1 = { "MockRate1", string.Empty, "MockRate2", "MockRate3", "MockRate1" };

        /// <summary>
        /// Burden Element Mappings 1 modified
        /// </summary>
        private readonly string[] burdenElementRateCodeMappings1Modified = { string.Empty, "MockRate1", string.Empty, string.Empty, "MockRate2" };

        /// <summary>
        /// Burden Element Mappings 2
        /// </summary>
        private readonly string[] burdenElementRateCodeMappings2 = { "MockRate2" };

        /// <summary>
        /// Burden Element Mappings 2 modified
        /// </summary>
        private readonly string[] burdenElementRateCodeMappings2Modified = { "MockRate1", string.Empty, string.Empty, string.Empty, "MockRate2" };

        /// <summary>
        /// Singleton pattern
        /// </summary>
        /// <returns>The test data instance</returns>
        public static TestData GetInstance()
        {
            if (currentInstance == null)
            {
                currentInstance = new TestData();
            }

            return currentInstance;
        }

        #endregion Class Information

        #region Users

        #region Setup

        /// <summary>
        /// Test user
        /// </summary>
        private UserData testUser = new()
        {
            Ntid = "testuser",
            DisplayName = "Test User",
            Email = "test@rdm.ssc.lmco.com",
            FirstName = "Test",
            LastName = "User 1",
            Phone = "555-1212",
            Company = "LM",
            IsGroup = false
        };

        /// <summary>
        /// Test User
        /// </summary>
        public UserData TestUser
        {
            get { return this.testUser; }
        }

        #endregion Setup

        #endregion Users

        /// <summary>
        /// Revision Mediator
        /// </summary>
        public IRevisionMediator RevisionMediator
        {
            get
            {
                return revisionMediator;
            }
        }

        #region Loaders

        /// <summary>
        /// RateConfig Loader
        /// </summary>
        public RateConfigLoader RateConfigLoader
        {
            get
            {
                return rateConfigLoader;
            }
        }

        /// <summary>
        /// Revision Loader
        /// </summary>
        public IRevisionLoader RevisionLoader
        {
            get
            {
                return revisionLoader;
            }
        }

        /// <summary>
        /// Section loader
        /// </summary>
        public SectionLoader SectionLoader
        {
            get
            {
                return sectionLoader;
            }
        }

        /// <summary>
        /// Burden Pool Loader
        /// </summary>
        public BurdenPoolLoader BurdenPoolLoader
        {
            get
            {
                return burdenPoolLoader;
            }
        }

        /// <summary>
        /// Cobra Detail Loader
        /// </summary>
        public CobraDetailLoader CobraDetailLoader
        {
            get
            {
                return cobraDetailLoader;
            }
        }

        /// <summary>
        /// Cobra Years Loader
        /// </summary>
        public CobraYearsLoader CobraYearsLoader
        {
            get
            {
                return cobraYearsLoader;
            }
        }

        /// <summary>
        /// Rate Code Year loader
        /// </summary>
        public RateCodeYearLoader RateCodeYearLoader
        {
            get
            {
                return rateCodeYearLoader;
            }
        }

        /// <summary>
        /// ProPricer Rate Code Xref loader
        /// </summary>
        public ProPricerRateCodeXrefLoader ProPricerRateCodeXrefLoader
        {
            get
            {
                return proPricerXrefLoader;
            }
        }

        /// <summary>
        /// Rate Detail loader
        /// </summary>
        public RateDetailLoader RateDetailLoader
        {
            get
            {
                return rateDetailLoader;
            }
        }

        /// <summary>
        /// Gets the file attachment loader.
        /// </summary>
        public FileAttachmentLoader FileAttachmentLoader
        {
            get
            {
                return fileAttachmentLoader;
            }
        }

        public IBannerLoader BannerLoader
        {
            get
            {
                return bannerLoader;
            }
        }

        /// <summary>
        /// Gets the rate code replication loader.
        /// </summary>
        public IRateCodeReplicationLoader RateCodeReplicationLoader
        {
            get
            {
                return rateCodeReplicationLoader;
            }
        }

        #endregion Loaders

        #region Revisions

        #region Setup

        /// <summary>
        /// Current Revision number counter
        /// </summary>
        private static int currentRevision = 777777;

        /// <summary>
        /// Thread-safe increment for currentRevision.
        /// </summary>
        /// <returns>Next Revision number</returns>
        public int IncrementCurrentRevision()
        {
            return Interlocked.Increment(ref currentRevision);
        }

        /// <summary>
        /// A collection of all revision created through this class
        /// </summary>
        private List<RevisionModelView> revisionCleanupList = new();

        /// <summary>
        /// Add given revision to list of revisions to be cleaned up at end of test run
        /// </summary>
        /// <param name="revision">Revision to be deleted at end of test run</param>
        public void AddRevisionForCleanup(RevisionModelView revision)
        {
            this.revisionCleanupList.Add(revision);
        }

        /// <summary>
        /// Generic revision
        /// </summary>
        private RevisionModelView genericRevision = new()
        {
            Id = 888888,
            Revision = "888888",
            History = "Mock Revision 888888",
            CreatedBy = "Felicioni, Frank (US)",
            StartYear = 2017,
            EndYear = 2040,
            ReleaseNotes = "Mock Notes 2"
        };

        /// <summary>
        /// Initialize Test Data
        /// </summary>
        public void Initialize()
        {
            // remove any leftover revisions from previous test runs
            ICollection<RevisionModelView> revisions = this.RevisionLoader.GetAll();
            if (revisions.Any())
            {
                using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    foreach (RevisionModelView revision in revisions)
                    {
                        int revisionNumber;
                        if (int.TryParse(revision.Revision, out revisionNumber) && revisionNumber >= currentRevision)
                        {
                            revision.Updateable = UpdateType.Deleted;
                            revisionLoader.Delete(revision);
                        }
                    }

                    scope.Complete();
                }
            }
        }

        #endregion Setup

        #region Reset

        /// <summary>
        /// Cleanup Test Data
        /// </summary>
        public void Cleanup()
        {
            // delete revisions
            this.ResetRevisions();
        }

        /// <summary>
        /// Resets the revisions so the next request will create a new one.
        /// </summary>
        private void ResetRevisions()
        {
            using (TransactionScope scope = new())
            {
                foreach (RevisionModelView revision in this.revisionCleanupList)
                {
                    RevisionModelView toDelete = null;
                    try
                    {
                        // get the current update time
                        toDelete = revisionMediator.GetById(revision.Id);
                    }
                    catch (ArgumentException)
                    {
                        // ignored - revision not found
                    }

                    // delete
                    if (toDelete != null)
                    {
                        toDelete.Updateable = UpdateType.Deleted;
                        revisionLoader.Delete(toDelete);
                    }
                }

                scope.Complete();
            }

            // Reset
            this.revisionCleanupList.Clear();
            this.genericRevision = null;
        }

        #endregion Reset

        #region Get

        /// <summary>
        /// Gets a revision, will create the default one if none exist
        /// </summary>
        /// <param name="createNew">Whether or not to create a new revision</param>
        /// <param name="revisionModelView">The revision to create</param>
        /// <returns>The revision</returns>
        public RevisionModelView GetRevision(bool createNew = false, RevisionModelView revisionModelView = null)
        {
            RevisionModelView toReturn;

            if (createNew)
            {
                using (TransactionScope scope = new())
                {
                    if (revisionModelView == null)
                    {
                        RevisionModelView newRevision = new()
                        {
                            Id = -1,
                            UpdateDate = DateTime.Now,
                            Updateable = UpdateType.Upsert,
                            Revision = this.IncrementCurrentRevision().ToString(),
                            History = "Mock Revision History",
                            CreatedBy = "Felicioni, Frank (US)",
                            StartYear = 2017,
                            EndYear = 2040,
                            ReleaseNotes = "Mock Release Notes"
                        };

                        toReturn = revisionMediator.GetById(revisionLoader.Upsert(newRevision).Value);
                        this.AddRevisionForCleanup(toReturn);
                    }
                    else
                    {
                        toReturn = revisionMediator.GetById(revisionLoader.Upsert(revisionModelView).Value);
                        this.AddRevisionForCleanup(toReturn);
                    }

                    scope.Complete();
                }
            }
            else
            {
                if (this.genericRevision == null)
                {
                    this.genericRevision = this.GetRevision(true);
                }

                toReturn = this.genericRevision;
            }

            return toReturn;
        }

        /// <summary>
        /// Helper method to publish a revision.
        /// </summary>
        /// <param name="revision">Revision to publish</param>
        /// <returns>published revision</returns>
        public RevisionModelView PublishRevision(RevisionModelView revision)
        {
            this.IncrementCurrentRevision();        // need to account for new revision
            int? newId = this.RevisionMediator.Publish(revision, this.testUser.DisplayName);
            RevisionModelView publishedRevision = this.RevisionMediator.GetById(newId);
            this.AddRevisionForCleanup(publishedRevision);    // need to clean up after tests complete
            return publishedRevision;
        }

        /// <summary>
        /// Helper method to rollback a revision.
        /// </summary>
        /// <param name="lastPublishedRevision">Revision to roll back to</param>
        /// <param name="wipRevision">Revision to roll back</param>
        /// <returns>rolled back revision</returns>
        public RevisionModelView RollbackRevision(IESUpdateableModelView lastPublishedRevision, RevisionModelView wipRevision)
        {
            this.IncrementCurrentRevision();    // need to account for new revision
            int? newId = this.RevisionMediator.Rollback(lastPublishedRevision, wipRevision);
            RevisionModelView newRevision = this.RevisionMediator.GetById(newId);
            this.AddRevisionForCleanup(newRevision);      // need to clean up after tests complete
            return newRevision;
        }

        /// <summary>
        /// First of three helper methods to modify revision data.
        /// This method adds Sections, Rate Codes, Rates, Burden Pools, Cobra and ProPricer mappings to a revision.
        /// </summary>
        /// <param name="revision">Revision</param>
        /// <returns>The revision</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public RevisionModelView ModifyRevisionData1(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // Add standard sections
                this.AddStandardSectionData(revision);

                // Add Burden Pools and mappings
                BurdenPoolDetailModelView burdenPool1 = new()
                {
                    Dirty = true,
                    Id = -1,
                    IsDeleted = false,
                    IsGaT2ApplicableForMissionSolutions = true,
                    IncludeGaT2InBurdAndCommBurdTables = false,
                    RevisionID = revision.Id,
                    BurdenPool = "MockBurdenPool1",
                    Description = "Mock Burden Pool 1",
                    IsCommercial = false,
                    // Note: BurdenElementRateCodeArray will be initialized (below) after the rate codes have been added
                    Updateable = UpdateType.Upsert
                };

                BurdenPoolDetailModelView burdenPool2 = new()
                {
                    Dirty = true,
                    Id = -1,
                    IsDeleted = false,
                    IsGaT2ApplicableForMissionSolutions = true,
                    IncludeGaT2InBurdAndCommBurdTables = false,
                    RevisionID = revision.Id,
                    BurdenPool = "MockBurdenPool2",
                    Description = "Mock Burden Pool 2",
                    IsCommercial = true,
                    // Note: BurdenElementRateCodeArray will be initialized (below) after the rate codes have been added
                    Updateable = UpdateType.Upsert
                };

                BurdenPoolDetailModelView burdenPool3 = new()
                {
                    Dirty = true,
                    Id = -1,
                    IsDeleted = false,
                    IsGaT2ApplicableForMissionSolutions = true,
                    IncludeGaT2InBurdAndCommBurdTables = false,
                    RevisionID = revision.Id,
                    BurdenPool = "MockBurdenPool3",
                    Description = "Mock Burden Pool 3",
                    IsCommercial = false,
                    // Note: BurdenElementRateCodeArray will be initialized (below) after the rate codes have been added
                    Updateable = UpdateType.Upsert
                };

                burdenPoolLoader.SaveBurdenPools(new Collection<BurdenPoolDetailModelView>() { burdenPool1, burdenPool2, burdenPool3 }, revision.Id);

                // Add Rate data
                RateDetailModelView rateDetail1 = new()
                {
                    Id = -1,
                    RevisionId = revision.Id,
                    IsDeleted = false,
                    RateCode = "MockRate1",
                    RateCategory = RateCategory.DirectLabor,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    CommercialBurdenPool = burdenPool1.BurdenPool,
                    CommercialBurdenPoolId = burdenPool1.Id,
                    Description = "Mock Rate 1",
                    Dirty = true,
                    GenerateAdditionalDirectLaborRates = false,
                    GovernmentBurdenPool = burdenPool2.BurdenPool,
                    GovernmentBurdenPoolId = burdenPool2.Id,
                    Values = new Collection<RateYearModelView>()
                    {
                        new()
                        {
                            Year = 2017,
                            Value = 1.17m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        },
                        new()
                        {
                            Year = 2018,
                            Value = 1.18m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        }
                    },
                    Updateable = UpdateType.Upsert
                };

                int sectionId = this.SectionLoader.GetAll(revision, true).First().Id;

                RateDetailModelView rateDetail2 = new()
                {
                    Id = -2,
                    RevisionId = revision.Id,
                    RateCode = "MockRate2",
                    RateCategory = RateCategory.DirectLabor,
                    RateType = RateType.Hours,
                    ResourceType = DirectRateMappingResourceType.Material,
                    IsDeleted = false,
                    CommercialBurdenPool = burdenPool1.BurdenPool,
                    CommercialBurdenPoolId = burdenPool1.Id,
                    GenerateAdditionalDirectLaborRates = true,
                    Description = "Mock Rate 2",
                    RateDescription = "Rate Description",   // note: this won't get saved, because GenerateAdditionalDirectLaborRates = true
                    RateDescription2 = "Rate Description 2",
                    Dirty = true,
                    GovernmentBurdenPool = burdenPool2.BurdenPool,
                    GovernmentBurdenPoolId = burdenPool2.Id,
                    Section = sectionId,
                    Values = new Collection<RateYearModelView>()
                    {
                        new()
                        {
                            Year = 2017,
                            Value = 2.17m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        },
                        new()
                        {
                            Year = 2018,
                            Value = 2.18m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        }
                    },
                    Updateable = UpdateType.Upsert
                };

                RateDetailModelView rateDetail3 = new()
                {
                    Id = -3,
                    RevisionId = revision.Id,
                    IsDeleted = false,
                    RateCode = "MockRate3",
                    RateCategory = RateCategory.DirectLabor,
                    RateType = RateType.Cost,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Description = "Mock Rate 3",
                    GenerateAdditionalDirectLaborRates = true,
                    RateDescription1 = "Rate Description 1",
                    RateDescription3 = "Rate Description 3",
                    Dirty = true,
                    CommercialBurdenPool = burdenPool1.BurdenPool,
                    CommercialBurdenPoolId = burdenPool1.Id,
                    GovernmentBurdenPool = burdenPool3.BurdenPool,
                    GovernmentBurdenPoolId = burdenPool3.Id,
                    Values = new Collection<RateYearModelView>()
                    {
                        new()
                        {
                            Year = 2017,
                            Value = 3.17m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        },
                        new()
                        {
                            Year = 2018,
                            Value = 3.18m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        }
                    },
                    Updateable = UpdateType.Upsert
                };

                RateDetailModelView rateDetail4 = new()
                {
                    Id = -4,
                    RevisionId = revision.Id,
                    IsDeleted = false,
                    RateCode = "MockRate4",
                    RateCategory = RateCategory.DirectLabor,
                    RateType = RateType.Cost,
                    ResourceType = DirectRateMappingResourceType.Labor,
                    Description = "Mock Rate 4",
                    GenerateAdditionalDirectLaborRates = true,
                    RateDescription1 = "Rate Description 1",
                    RateDescription4 = "Rate Description 4",
                    Dirty = true,
                    Values = new Collection<RateYearModelView>()
                    {
                        new()
                        {
                            Year = 2017,
                            Value = 4.17m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        },
                        new()
                        {
                            Year = 2018,
                            Value = 4.18m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        }
                    },
                    Updateable = UpdateType.Upsert
                };

                this.RateDetailLoader.SaveDetails(new Collection<RateDetailModelView> { rateDetail1, rateDetail2, rateDetail3, rateDetail4 });

                // Modify the COBRA mappings
                ICollection<CobraDetailModelView> cobraDetails = this.CobraDetailLoader.GetCobraDetailsByRevision(revision);
                CobraDetailModelView cobraDetail2 = cobraDetails.Where(x => x.RateCode.Equals("MockRate2")).First();
                cobraDetail2.RateSet = "MockRateSet2";
                cobraDetail2.Code1 = Code1.SVCCTR;
                cobraDetail2.Dirty = true;

                CobraDetailModelView cobraDetail3 = cobraDetails.Where(x => x.RateCode.Equals("MockRate3")).First();
                cobraDetail3.RateSet = "MockRateSet1";
                cobraDetail3.Code1 = Code1.INDIRECT;
                cobraDetail3.Dirty = true;

                CobraDetailModelView cobraDetail4 = cobraDetails.Where(x => x.RateCode.Equals("MockRate4")).First();
                cobraDetail4.RateSet = "MockRateSet1";
                cobraDetail4.Code1 = Code1.INDIRECT;
                cobraDetail4.Dirty = true;

                this.CobraDetailLoader.SaveDetails(cobraDetails.Where(x => x.Dirty).ToCollection());

                // Save the Burden Pool mappings
                BurdenPoolGridModelView burdenPoolResults = burdenPoolLoader.GetByRevision(revision.Id);
                int numBurdenElements = burdenPoolResults.BurdenElements.Count;

                BurdenPoolDetailModelView bp1 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool1");
                bp1.Dirty = true;
                bp1.Updateable = UpdateType.Upsert;
                bp1.BurdenElementRateCodeArray = this.PadBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings1, numBurdenElements);

                BurdenPoolDetailModelView bp2 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool2");
                bp2.Dirty = true;
                bp2.Updateable = UpdateType.Upsert;
                bp2.BurdenElementRateCodeArray = this.PadBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings2, numBurdenElements);
                burdenPoolLoader.SaveBurdenPools(burdenPoolResults.BurdenPools, revision.Id);

                scope.Complete();
            }

            return RevisionMediator.GetById(revision.Id);
        }

        /// <summary>
        /// Second of three helper methods to modify revision data.
        /// This method modifies Sections, Rate Codes, Rates, Burden Pools, Cobra and ProPricer mappings for a revision.
        /// It assumes that ModifyRevisionData1 has already been executed.
        /// </summary>
        /// <param name="revision">Revision to modify</param>
        public void ModifyRevisionData2(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            BurdenPoolGridModelView burdenPoolResults;
            BurdenPoolDetailModelView burdenPool1;
            BurdenPoolDetailModelView burdenPool2;
            BurdenPoolDetailModelView burdenPool3;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // Modify a Burden Pool
                burdenPoolResults = burdenPoolLoader.GetByRevision(revision.Id);
                int numBurdenElements = burdenPoolResults.BurdenElements.Count;
                burdenPool1 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool1");
                burdenPool2 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool2");
                burdenPool3 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool3");

                // Modify Burden Pool Name, Description, and Burden Element mapping.
                burdenPool1.BurdenPool = "MockBurdenPool1a";
                burdenPool1.Description = "Mock Burden Pool 1a";
                burdenPool1.Dirty = true;
                burdenPool1.IsDeleted = false;
                burdenPool1.IsCommercial = true;
                burdenPool1.ExcludeFCCOMFromCommercial = true;
                burdenPool1.Updateable = UpdateType.Upsert;

                // Modify the Burden Pool mappings
                burdenPool1.BurdenElementRateCodeArray = this.PadBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings1Modified, numBurdenElements);

                // Modify Burden Pool Description.
                burdenPool2.Description = "Mock Burden Pool 2a";
                burdenPool2.Dirty = true;
                burdenPool2.IsDeleted = false;
                burdenPool2.Updateable = UpdateType.Upsert;

                // Modify the Burden Pool mappings
                burdenPool2.BurdenElementRateCodeArray = this.PadBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings2Modified, numBurdenElements);

                // Delete a Burden Pool
                burdenPool3.Dirty = true;
                burdenPool3.IsDeleted = true;
                burdenPool3.Updateable = UpdateType.Deleted;

                burdenPoolLoader.SaveBurdenPools(new Collection<BurdenPoolDetailModelView>() { burdenPool1, burdenPool2, burdenPool3 }, revision.Id);

                scope.Complete();
            }

            burdenPoolResults = burdenPoolLoader.GetByRevision(revision.Id);
            burdenPool1 = burdenPoolResults.BurdenPools.FirstOrDefault(x => x.BurdenPool == "MockBurdenPool1a");
            Assert.IsNotNull(burdenPool1);
            burdenPool2 = burdenPoolResults.BurdenPools.FirstOrDefault(x => x.BurdenPool == "MockBurdenPool2");
            Assert.IsNotNull(burdenPool2);
            burdenPool3 = burdenPoolResults.BurdenPools.FirstOrDefault(x => x.BurdenPool == "MockBurdenPool3");
            Assert.IsNull(burdenPool3); // removed

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // Modify some rates
                ICollection<RateDetailModelView> rates = rateDetailLoader.GetRatesByRevision(revision);
                RateDetailModelView rateDetail1 = rates.First(x => x.RateCode == "MockRate1");
                rateDetail1.RateCategory = RateCategory.NonLaborEscalationFactor;
                rateDetail1.RateType = RateType.Cost;
                rateDetail1.Description = "Mock Rate 1a";
                rateDetail1.Updateable = UpdateType.Upsert;
                RateYearModelView rateYear2017 = rateDetail1.Values.First(x => x.Year == 2017);
                rateYear2017.Value = 11.17m; // Change rate value
                rateYear2017.Dirty = true;
                rateYear2017.Updateable = UpdateType.Upsert;
                RateYearModelView rateYear2018 = rateDetail1.Values.First(x => x.Year == 2018);
                rateYear2018.Value = 11.18m; // Change rate value
                rateYear2018.Dirty = true;
                rateYear2018.Updateable = UpdateType.Upsert;

                RateDetailModelView rateDetail2 = rates.First(x => x.RateCode == "MockRate2");
                rateDetail2.Description = "Mock Rate 2a";
                rateDetail2.CommercialBurdenPool = string.Empty;
                rateDetail2.CommercialBurdenPoolId = 0;
                rateDetail2.GovernmentBurdenPool = string.Empty;
                rateDetail2.GovernmentBurdenPoolId = 0;
                rateDetail2.Updateable = UpdateType.Upsert;
                rateYear2017 = rateDetail2.Values.First(x => x.Year == 2017);
                rateYear2017.Value = null;  // Remove rate for 2017
                rateYear2017.Dirty = true;
                rateYear2017.Updateable = UpdateType.Upsert;
                // Add rate for 2019
                rateDetail2.Values.Add(new RateYearModelView()
                {
                    Year = 2019,
                    Value = 22.19m,
                    Dirty = true,
                    Id = -1,
                    Updateable = UpdateType.Upsert
                });

                RateDetailModelView rateDetail3 = rates.First(x => x.RateCode == "MockRate3");
                rateDetail3.Updateable = UpdateType.Upsert;
                rateDetail3.Dirty = true;
                rateDetail3.RateDescription1 = null;                    // remove ProPricer extension #1
                rateDetail3.RateDescription2 = "Rate Description 2";    // add ProPricer extension #2

                RateDetailModelView rateDetail4 = rates.First(x => x.RateCode == "MockRate4");
                rateDetail4.Updateable = UpdateType.Upsert;
                rateDetail4.Dirty = true;
                rateDetail4.RateDescription1 = string.Empty; // remove ProPricer extension using empty string (instead of null)
                rateYear2018 = rateDetail4.Values.First(x => x.Year == 2018);
                rateYear2018.Value = 44.18m; // Change rate value
                rateYear2018.Dirty = true;
                rateYear2018.Updateable = UpdateType.Upsert;
                // Add rate for 2019
                rateDetail4.Values.Add(new RateYearModelView()
                {
                    Year = 2019,
                    Value = 44.19m,
                    Dirty = true,
                    Id = -1,
                    Updateable = UpdateType.Upsert
                });
                // Add rate for 2050 (beyond configured year range)
                rateDetail4.Values.Add(new RateYearModelView()
                {
                    Year = 2050,
                    Value = 44.50m,
                    Dirty = true,
                    Id = -1,
                    Updateable = UpdateType.Upsert
                });

                // Add a new rate code
                RateDetailModelView rateDetail5 = new()
                {
                    Id = -5,
                    RevisionId = revision.Id,
                    RateCode = "MockRate5",
                    RateCategory = RateCategory.DirectLabor,
                    RateType = RateType.Hours,
                    ResourceType = DirectRateMappingResourceType.Material,
                    IsDeleted = false,
                    CommercialBurdenPool = burdenPool1.BurdenPool,
                    CommercialBurdenPoolId = burdenPool1.Id,
                    Description = "Mock Rate 5",
                    RateDescription = "Rate Description",
                    Dirty = true,
                    GenerateAdditionalDirectLaborRates = false,
                    GovernmentBurdenPool = burdenPool2.BurdenPool,
                    GovernmentBurdenPoolId = burdenPool2.Id,
                    Values = new Collection<RateYearModelView>()
                    {
                        new()
                        {
                            Year = 2017,
                            Value = 5.17m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        },
                        new()
                        {
                            Year = 2018,
                            Value = 5.18m,
                            Dirty = true,
                            Id = -1,
                            Updateable = UpdateType.Upsert
                        }
                    },
                    Updateable = UpdateType.Upsert
                };

                this.RateDetailLoader.SaveDetails(new Collection<RateDetailModelView> { rateDetail1, rateDetail2, rateDetail3, rateDetail4, rateDetail5 });

                // Modify the COBRA mappings
                ICollection<CobraDetailModelView> cobraDetails = this.CobraDetailLoader.GetCobraDetailsByRevision(revision);
                CobraDetailModelView cobraDetail1 = cobraDetails.Where(x => x.RateCode.Equals("MockRate1")).First();
                cobraDetail1.RateSet = "MockRateSet1";
                cobraDetail1.Code1 = Code1.SVCCTR;
                cobraDetail1.Dirty = true;

                CobraDetailModelView cobraDetail5 = cobraDetails.Where(x => x.RateCode.Equals("MockRate5")).First();
                cobraDetail5.RateSet = "MockRateSet1";
                cobraDetail5.Code1 = Code1.SVCCTR;
                cobraDetail5.Dirty = true;

                this.CobraDetailLoader.SaveDetails(cobraDetails.Where(x => x.Dirty).ToCollection());

                // Modify section data
                ICollection<SectionModelView> sections = sectionLoader.GetAll(revision);
                SectionModelView section0 = sections.First(x => x.DisplayOrder == 0);
                SectionModelView section1 = sections.First(x => x.DisplayOrder == 1);

                SectionModelView section = this.FindSectionByTitle(section0, "Oversight Agencies");
                Assert.IsNotNull(section, "Unable to find 'Oversight Agencies' section");
                section.Title = "Oversight Agencies (modified - added a Rate Table and text content)";
                section.ChildNodes.Add(
                    new SectionModelView()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        ContentType = SectionContentType.RateTable,
                        ChildNodes = new List<SectionModelView>(),
                        DisplayOrder = 1,
                        DisplayRateCode = true
                    });

                section.ChildNodes.Add(
                    new SectionModelView()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        ContentType = SectionContentType.Text,
                        ChildNodes = new List<SectionModelView>(),
                        DisplayOrder = 2,
                        TextContent = "Added text content blah blah blah..."
                    });

                section.ChildNodes.Add(
                    new SectionModelView()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        Title = "New Section",
                        ContentType = SectionContentType.Section,
                        ChildNodes = new List<SectionModelView>()
                        {
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                Title = "New SubSection",
                                ContentType = SectionContentType.Section,
                                ChildNodes = new List<SectionModelView>(),
                                IsInternalSection = false,
                                DisplayOrder = 0
                            }
                        },
                        IsInternalSection = true,
                        DisplayOrder = 3
                    });

                section = this.FindSectionByTitle(section1, "Development Overhead");
                Assert.IsNotNull(section, "Unable to find 'Development Overhead' section");
                SectionModelView subSection0 = section.ChildNodes.First(x => x.DisplayOrder == 0);
                SectionModelView subSection2 = section.ChildNodes.First(x => x.DisplayOrder == 2);
                // modify sub section text content and swap the positions by updating the display order value 
                // (note: DisplayOrder changes are currently not recognized by GetSectionRowsCompare method)
                subSection0.DisplayOrder = 2;
                subSection0.TextContent = "This section was moved to the end.";
                subSection0.Updateable = UpdateType.Upsert;
                subSection2.DisplayOrder = 0;
                subSection2.TextContent = "This section was moved to the beginning.";
                subSection2.Updateable = UpdateType.Upsert;

                section = this.FindSectionByTitle(section1, "Production Overhead");
                Assert.IsNotNull(section, "Unable to find 'Production Overhead' section");
                // modify title
                section.Title = "Production Overhead (modified)";
                // remove one of the child sections
                subSection2 = section.ChildNodes.First(x => x.DisplayOrder == 2);
                section.ChildNodes.Remove(subSection2);

                sectionLoader.UpdateSectionsAndContent(revision, sections);

                scope.Complete();
            }
        }

        /// <summary>
        /// Third of three helper methods to modify revision data.
        /// This method deletes rate codes from a revision.
        /// It assumes that ModifyRevisionData1 and ModifyRevisionData2 have already been executed.
        /// </summary>
        /// <param name="revision">Revision to modify</param>
        public void ModifyRevisionData3(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                // Delete some rates
                ICollection<RateDetailModelView> rates = rateDetailLoader.GetRatesByRevision(revision);

                ICollection<RateDetailModelView> ratesToDelete = new Collection<RateDetailModelView>();
                RateDetailModelView rateDetail3 = rates.FirstOrDefault(x => x.RateCode == "MockRate3");
                if (rateDetail3 != null)
                {
                    rateDetail3.Dirty = true;
                    rateDetail3.IsDeleted = true;
                    rateDetail3.Updateable = UpdateType.Deleted;
                    ratesToDelete.Add(rateDetail3);
                }

                RateDetailModelView rateDetail4 = rates.FirstOrDefault(x => x.RateCode == "MockRate4");
                if (rateDetail4 != null)
                {
                    rateDetail4.Dirty = true;
                    rateDetail4.IsDeleted = true;
                    rateDetail4.Updateable = UpdateType.Deleted;
                    ratesToDelete.Add(rateDetail4);
                }

                RateDetailModelView rateDetail5 = rates.FirstOrDefault(x => x.RateCode == "MockRate5");
                if (rateDetail5 != null)
                {
                    rateDetail5.Dirty = true;
                    rateDetail5.IsDeleted = true;
                    rateDetail5.Updateable = UpdateType.Deleted;
                    ratesToDelete.Add(rateDetail5);
                }

                this.RateDetailLoader.SaveDetails(ratesToDelete);

                scope.Complete();
            }
        }

        /// <summary>
        /// Helper method to verify revision data after first set of modifications.
        /// </summary>
        /// <param name="revision">Revision to validate</param>
        public void VerifyModifiedRevisionData1(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            BurdenPoolGridModelView burdenPoolResults = burdenPoolLoader.GetByRevision(revision.Id);
            int numBurdenElements = burdenPoolResults.BurdenElements.Count;
            Assert.AreEqual(3, burdenPoolResults.BurdenPools.Count);
            BurdenPoolDetailModelView bp1 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool1");
            Assert.IsNotNull(bp1);
            Assert.IsTrue(bp1.Id >= 0);
            Assert.IsFalse(bp1.IsCommercial);
            this.VerifyBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings1, bp1.BurdenElementRateCodeArray, numBurdenElements);
            BurdenPoolDetailModelView bp2 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool2");
            Assert.IsNotNull(bp2);
            Assert.IsTrue(bp2.Id >= 0);
            Assert.IsTrue(bp2.IsCommercial);
            this.VerifyBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings2, bp2.BurdenElementRateCodeArray, numBurdenElements);
            BurdenPoolDetailModelView bp3 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool3");
            Assert.IsNotNull(bp3);
            Assert.IsTrue(bp3.Id >= 0);
            Assert.IsFalse(bp3.IsCommercial);

            Assert.AreEqual(5, burdenPoolResults.RateCodes.Count); // 4 rates + blank
            ICollection<RateDetailModelView> rates = rateDetailLoader.GetRatesByRevision(revision);
            Assert.AreEqual(4, rates.Count);
            RateDetailModelView rateDetail1 = rates.First(x => x.RateCode == "MockRate1" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 1");
            Assert.IsNotNull(rateDetail1);
            Assert.AreEqual(1, rateDetail1.Values.Count(x => x.Year == 2017 && x.Value == 1.17m));
            Assert.AreEqual(1, rateDetail1.Values.Count(x => x.Year == 2018 && x.Value == 1.18m));
            Assert.AreEqual(bp1.Id, rateDetail1.CommercialBurdenPoolId);
            Assert.AreEqual(bp1.BurdenPool, rateDetail1.CommercialBurdenPool);
            Assert.AreEqual(bp2.Id, rateDetail1.GovernmentBurdenPoolId);
            Assert.AreEqual(bp2.BurdenPool, rateDetail1.GovernmentBurdenPool);
            Assert.IsTrue(rateDetail1.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail2 = rates.First(x => x.RateCode == "MockRate2" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 2");
            Assert.IsNotNull(rateDetail2);
            Assert.AreEqual(1, rateDetail2.Values.Count(x => x.Year == 2017 && x.Value == 2.17m));
            Assert.AreEqual(1, rateDetail2.Values.Count(x => x.Year == 2018 && x.Value == 2.18m));
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail2.RateDescription));   // RateDescription is not saved when GenerateAdditionalDirectLaborRates is true
            Assert.AreEqual("Rate Description 2", rateDetail2.RateDescription2);
            Assert.AreEqual(bp1.Id, rateDetail2.CommercialBurdenPoolId);
            Assert.AreEqual(bp1.BurdenPool, rateDetail2.CommercialBurdenPool);
            Assert.AreEqual(bp2.Id, rateDetail2.GovernmentBurdenPoolId);
            Assert.AreEqual(bp2.BurdenPool, rateDetail2.GovernmentBurdenPool);
            Assert.IsTrue(rateDetail2.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail3 = rates.First(x => x.RateCode == "MockRate3" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 3");
            Assert.IsNotNull(rateDetail3);
            Assert.AreEqual(1, rateDetail3.Values.Count(x => x.Year == 2017 && x.Value == 3.17m));
            Assert.AreEqual(1, rateDetail3.Values.Count(x => x.Year == 2018 && x.Value == 3.18m));
            Assert.AreEqual("Rate Description 1", rateDetail3.RateDescription1);
            Assert.AreEqual("Rate Description 3", rateDetail3.RateDescription3);
            Assert.AreEqual(bp1.Id, rateDetail3.CommercialBurdenPoolId);
            Assert.AreEqual(bp1.BurdenPool, rateDetail3.CommercialBurdenPool);
            Assert.AreEqual(bp3.Id, rateDetail3.GovernmentBurdenPoolId);
            Assert.AreEqual(bp3.BurdenPool, rateDetail3.GovernmentBurdenPool);
            Assert.IsTrue(rateDetail3.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail4 = rates.First(x => x.RateCode == "MockRate4" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 4");
            Assert.IsNotNull(rateDetail4);
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2017 && x.Value == 4.17m));
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2018 && x.Value == 4.18m));
            Assert.AreEqual("Rate Description 1", rateDetail4.RateDescription1);
            Assert.AreEqual("Rate Description 4", rateDetail4.RateDescription4);
            Assert.AreEqual(0, rateDetail4.CommercialBurdenPoolId); // never set (mapping should be empty)
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail4.CommercialBurdenPool));
            Assert.AreEqual(0, rateDetail4.GovernmentBurdenPoolId); // never set (mapping should be empty)
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail4.GovernmentBurdenPool));
            Assert.IsFalse(rateDetail4.HasProPricerBurdenRateMappings);

            // verify COBRA mappings
            ICollection<CobraDetailModelView> cobraDetails = this.CobraDetailLoader.GetCobraDetailsByRevision(revision);
            Assert.AreEqual(4, cobraDetails.Count);
            CobraDetailModelView cobraDetail1 = cobraDetails.First(x => x.RateCode.Equals("MockRate1") && x.Code1 == Code1.NA);
            Assert.IsNotNull(cobraDetail1);
            CobraDetailModelView cobraDetail2 = cobraDetails.First(x => x.RateCode.Equals("MockRate2") && x.Code1 == Code1.SVCCTR);
            Assert.IsNotNull(cobraDetail2);
            CobraDetailModelView cobraDetail3 = cobraDetails.First(x => x.RateCode.Equals("MockRate3") && x.Code1 == Code1.INDIRECT);
            Assert.IsNotNull(cobraDetail3);
            CobraDetailModelView cobraDetail4 = cobraDetails.First(x => x.RateCode.Equals("MockRate4") && x.Code1 == Code1.INDIRECT);
            Assert.IsNotNull(cobraDetail4);

            // verify section data (minimal checks here, will be verified in detail by SectionLoaderTests)
            ICollection<SectionModelView> sections = sectionLoader.RetrieveAllSections(revision);
            Assert.IsNotNull(sections);
            Assert.AreEqual(2, sections.Count); // Top-level sections

            ICollection<OptionModelView> sectionOptions = sectionLoader.RetrieveSectionsAsOptions(revision);
            Assert.IsNotNull(sectionOptions);
            Assert.AreEqual(7, sectionOptions.Count); // 6 sections + blank option
        }

        /// <summary>
        /// Verify revision data after second set of modifications.
        /// </summary>
        /// <param name="revision">Revision to validate</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public void VerifyModifiedRevisionData2(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            // validate burden pool changes
            BurdenPoolGridModelView burdenPoolResults = burdenPoolLoader.GetByRevision(revision.Id);
            int numBurdenElements = burdenPoolResults.BurdenElements.Count;
            Assert.AreEqual(2, burdenPoolResults.BurdenPools.Count);
            BurdenPoolDetailModelView bp1 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool1a");
            Assert.IsNotNull(bp1);
            Assert.IsTrue(bp1.Id >= 0);
            Assert.IsTrue(bp1.IsCommercial);
            Assert.IsTrue(bp1.ExcludeFCCOMFromCommercial);
            this.VerifyBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings1Modified, bp1.BurdenElementRateCodeArray, numBurdenElements);

            BurdenPoolDetailModelView bp2 = burdenPoolResults.BurdenPools.First(x => x.BurdenPool == "MockBurdenPool2");
            Assert.IsNotNull(bp2);
            Assert.IsTrue(bp2.Id >= 0);
            Assert.AreEqual(bp2.Description, "Mock Burden Pool 2a");
            Assert.IsTrue(bp2.IsCommercial);
            this.VerifyBurdenElementRateCodeMappings(this.burdenElementRateCodeMappings2Modified, bp2.BurdenElementRateCodeArray, numBurdenElements);

            BurdenPoolDetailModelView bp3 = burdenPoolResults.BurdenPools.FirstOrDefault(x => x.BurdenPool == "MockBurdenPool3");
            Assert.IsNull(bp3); // MockBurdenPool3 was deleted

            // validate rate changes
            Assert.AreEqual(6, burdenPoolResults.RateCodes.Count); // 5 rates + blank
            ICollection<RateDetailModelView> rates = rateDetailLoader.GetRatesByRevision(revision);
            Assert.AreEqual(5, rates.Count);
            RateDetailModelView rateDetail1 = rates.First(x => x.RateCode == "MockRate1" &&
                                                               x.RateCategory == RateCategory.NonLaborEscalationFactor &&
                                                               x.Description == "Mock Rate 1a");
            Assert.IsNotNull(rateDetail1);
            Assert.AreEqual(1, rateDetail1.Values.Count(x => x.Year == 2017 && x.Value == 11.17m)); // updated
            Assert.AreEqual(1, rateDetail1.Values.Count(x => x.Year == 2018 && x.Value == 11.18m)); // updated
            Assert.AreEqual(1, rateDetail1.Values.Count(x => x.Year == 2019 && x.Value == null));   // no change (never set)
            Assert.AreEqual(bp1.Id, rateDetail1.CommercialBurdenPoolId);    // no change
            Assert.AreEqual(bp1.BurdenPool, rateDetail1.CommercialBurdenPool);
            Assert.AreEqual(bp2.Id, rateDetail1.GovernmentBurdenPoolId);    // no change
            Assert.AreEqual(bp2.BurdenPool, rateDetail1.GovernmentBurdenPool);
            Assert.IsTrue(rateDetail1.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail2 = rates.First(x => x.RateCode == "MockRate2" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 2a");
            Assert.IsNotNull(rateDetail2);
            Assert.AreEqual(1, rateDetail2.Values.Count(x => x.Year == 2017 && x.Value == null));   // removed
            Assert.AreEqual(1, rateDetail2.Values.Count(x => x.Year == 2018 && x.Value == 2.18m));  // no change
            Assert.AreEqual(1, rateDetail2.Values.Count(x => x.Year == 2019 && x.Value == 22.19m)); // added
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail2.RateDescription));   // no change
            Assert.AreEqual("Rate Description 2", rateDetail2.RateDescription2);
            Assert.AreEqual(0, rateDetail2.CommercialBurdenPoolId); // mapping removed
            Assert.AreEqual(string.Empty, rateDetail2.CommercialBurdenPool);
            Assert.AreEqual(0, rateDetail2.GovernmentBurdenPoolId); // mapping removed
            Assert.AreEqual(string.Empty, rateDetail2.GovernmentBurdenPool);
            Assert.IsTrue(rateDetail2.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail3 = rates.First(x => x.RateCode == "MockRate3" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 3");
            Assert.IsNotNull(rateDetail3);
            Assert.AreEqual(1, rateDetail3.Values.Count(x => x.Year == 2017 && x.Value == 3.17m));
            Assert.AreEqual(1, rateDetail3.Values.Count(x => x.Year == 2018 && x.Value == 3.18m));
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail3.RateDescription1));      // removed
            Assert.AreEqual("Rate Description 2", rateDetail3.RateDescription2);    // added ProPricer Extension #2
            Assert.AreEqual("Rate Description 3", rateDetail3.RateDescription3);    // no change
            Assert.AreEqual(bp1.Id, rateDetail3.CommercialBurdenPoolId);    // no change
            Assert.AreEqual(bp1.BurdenPool, rateDetail3.CommercialBurdenPool);
            Assert.AreEqual(0, rateDetail3.GovernmentBurdenPoolId);         // MockBurdenPool3 was deleted so mapping should be cleared
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail3.GovernmentBurdenPool));
            Assert.IsFalse(rateDetail3.HasProPricerBurdenRateMappings);

            RateDetailModelView rateDetail4 = rates.First(x => x.RateCode == "MockRate4" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 4");
            Assert.IsNotNull(rateDetail4);
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2017 && x.Value == 4.17m));  // no change
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2018 && x.Value == 44.18m)); // updated
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2019 && x.Value == 44.19m)); // added
            Assert.AreEqual(0, rateDetail4.Values.Count(x => x.Year == 2050 && x.Value == 44.50m)); // added, but beyond configured year range
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail4.RateDescription1));      // removed
            Assert.AreEqual("Rate Description 4", rateDetail4.RateDescription4);
            Assert.AreEqual(0, rateDetail4.CommercialBurdenPoolId);     // no change
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail4.CommercialBurdenPool));
            Assert.AreEqual(0, rateDetail4.GovernmentBurdenPoolId);    // no change
            Assert.IsTrue(string.IsNullOrEmpty(rateDetail4.GovernmentBurdenPool));
            Assert.IsFalse(rateDetail4.HasProPricerBurdenRateMappings);

            // verify added rate code
            RateDetailModelView rateDetail5 = rates.First(x => x.RateCode == "MockRate5" &&
                                                               x.RateCategory == RateCategory.DirectLabor &&
                                                               x.Description == "Mock Rate 5");
            Assert.IsNotNull(rateDetail5);
            Assert.AreEqual(1, rateDetail5.Values.Count(x => x.Year == 2017 && x.Value == 5.17m));
            Assert.AreEqual(1, rateDetail5.Values.Count(x => x.Year == 2018 && x.Value == 5.18m));
            Assert.AreEqual(1, rateDetail5.Values.Count(x => x.Year == 2019 && x.Value == null));
            Assert.AreEqual("Rate Description", rateDetail5.RateDescription);
            Assert.AreEqual(bp1.Id, rateDetail5.CommercialBurdenPoolId);
            Assert.AreEqual(bp1.BurdenPool, rateDetail5.CommercialBurdenPool);
            Assert.AreEqual(bp2.Id, rateDetail5.GovernmentBurdenPoolId);
            Assert.AreEqual(bp2.BurdenPool, rateDetail5.GovernmentBurdenPool);
            Assert.IsFalse(rateDetail5.HasProPricerBurdenRateMappings);

            // verify rate code for year 2050 is present (provided the EndYear is high enough)
            revision.EndYear = 2050;
            rates = rateDetailLoader.GetRatesByRevision(revision);
            rateDetail4 = rates.First(x => x.RateCode == "MockRate4");
            Assert.AreEqual(1, rateDetail4.Values.Count(x => x.Year == 2050 && x.Value == 44.50m));

            // verify COBRA mappings
            ICollection<CobraDetailModelView> cobraDetails = this.CobraDetailLoader.GetCobraDetailsByRevision(revision);
            Assert.AreEqual(5, cobraDetails.Count);
            CobraDetailModelView cobraDetail1 = cobraDetails.First(x => x.RateCode.Equals("MockRate1") && x.Code1 == Code1.SVCCTR);
            Assert.IsNotNull(cobraDetail1);
            CobraDetailModelView cobraDetail2 = cobraDetails.First(x => x.RateCode.Equals("MockRate2") && x.Code1 == Code1.SVCCTR);
            Assert.IsNotNull(cobraDetail2);
            CobraDetailModelView cobraDetail3 = cobraDetails.First(x => x.RateCode.Equals("MockRate3") && x.Code1 == Code1.INDIRECT);
            Assert.IsNotNull(cobraDetail3);
            CobraDetailModelView cobraDetail4 = cobraDetails.First(x => x.RateCode.Equals("MockRate4") && x.Code1 == Code1.INDIRECT);
            Assert.IsNotNull(cobraDetail4);
            CobraDetailModelView cobraDetail5 = cobraDetails.First(x => x.RateCode.Equals("MockRate5") && x.Code1 == Code1.SVCCTR);
            Assert.IsNotNull(cobraDetail5);

            // verify section data (minimal checks here, will be verified in detail by SectionLoaderTests)
            ICollection<SectionModelView> sections = sectionLoader.RetrieveAllSections(revision);
            Assert.IsNotNull(sections);
            Assert.AreEqual(2, sections.Count); // Top-level sections

            ICollection<OptionModelView> sectionOptions = sectionLoader.RetrieveSectionsAsOptions(revision);
            Assert.IsNotNull(sectionOptions);
            Assert.AreEqual(9, sectionOptions.Count); // 8 sections + blank option
        }

        /// <summary>
        /// Verify revision data after 3rd set of modifications.
        /// </summary>
        /// <param name="revision">Revision to validate</param>
        public void VerifyModifiedRevisionData3(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            // validate rate changes
            ICollection<RateDetailModelView> rates = rateDetailLoader.GetRatesByRevision(revision);
            Assert.AreEqual(2, rates.Count);
            RateDetailModelView rateDetail1 = rates.FirstOrDefault(x => x.RateCode == "MockRate1");
            Assert.IsNotNull(rateDetail1);

            RateDetailModelView rateDetail2 = rates.FirstOrDefault(x => x.RateCode == "MockRate2");
            Assert.IsNotNull(rateDetail2);

            RateDetailModelView rateDetail3 = rates.FirstOrDefault(x => x.RateCode == "MockRate3");
            Assert.IsNull(rateDetail3); // removed

            RateDetailModelView rateDetail4 = rates.FirstOrDefault(x => x.RateCode == "MockRate4");
            Assert.IsNull(rateDetail4); // removed

            RateDetailModelView rateDetail5 = rates.FirstOrDefault(x => x.RateCode == "MockRate5");
            Assert.IsNull(rateDetail5); // removed
        }

        /// <summary>
        /// Pad Burden Element Mappings array out to specified length.
        /// </summary>
        /// <param name="burdenElementRateCodeMappings">Burden Element Rate Code Mapping array</param>
        /// <param name="padToLength">Pad the array to the specified length</param>
        /// <returns>mappings</returns>
        public string[] PadBurdenElementRateCodeMappings(string[] burdenElementRateCodeMappings, int padToLength)
        {
            if (burdenElementRateCodeMappings == null)
            {
                throw new ArgumentNullException(nameof(burdenElementRateCodeMappings));
            }

            string[] mappings = new string[padToLength];
            for (int i = 0; i < padToLength; i++)
            {
                mappings[i] = i >= burdenElementRateCodeMappings.Length || string.IsNullOrEmpty(burdenElementRateCodeMappings[i]) ? string.Empty : burdenElementRateCodeMappings[i];
            }

            return mappings;
        }

        /// <summary>
        /// Helper method to verify burden element rate code mapping arrays.
        /// </summary>
        /// <param name="expected">Expected burden element rate code mappings.</param>
        /// <param name="actual">Actual burden element rate code mappings.</param>
        /// <param name="numBurdenElements">Number of Burden Elements mappings expected.</param>
        public void VerifyBurdenElementRateCodeMappings(string[] expected, string[] actual, int numBurdenElements)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            Assert.AreEqual(numBurdenElements, actual.Length);
            for (int i = 0; i < numBurdenElements; i++)
            {
                // Note: expected values array may not be populated out to full number of Burden Elements.
                if (i < expected.Length)
                {
                    Assert.AreEqual(expected[i], actual[i], "Burden Element/Rate Code mapping mismatch.");
                }
                else
                {
                    Assert.IsTrue(string.IsNullOrEmpty(actual[i]), "Unexpected Burden Element/Rate Code mapping.");
                }
            }
        }

        #endregion Get

        #endregion Revisions

        #region Sections

        #region Setup

        /// <summary>
        /// Adds standard section data in the revision.  This is the starting point for most loader tests.
        /// Generate nodes to make sections and content like:
        ///                  1 
        ///             (Introduction)                  
        ///       /          |          \   
        ///  (text)         1.1           1.2                         
        ///          (Points of contact)  (Oversight Agencies)
        ///                  |               |
        ///               (text)          (text)
        /// 
        ///           
        ///                          2
        ///  (Overhead, G&amp;A, Fringe Benefits and Facilities Cost of Money Rates)
        ///               /          |                      \   
        ///          (text)         2.1                       2.2      
        ///                (Development Overhead)     (Production Overhead)
        ///                    /     |     \             /     |     \   
        ///              (text) (rate tbl) (text)    (text) (rate tbl) (text) 
        /// 
        /// </summary>
        /// <param name="revision">Revision</param>
        public void AddStandardSectionData(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            // Add some sections
            SectionModelView section0 = new()
            {
                Id = -1,
                RevisionUniqueSectionId = -1,
                Title = "Introduction",
                ContentType = SectionContentType.Section,
                DisplayOrder = 0,
                IsInternalSection = false,
                IsRdsbRequired = false,
                ChildNodes = new List<SectionModelView>()
                {
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        ContentType = SectionContentType.Text,
                        ChildNodes = new List<SectionModelView>(),
                        DisplayOrder = 0,
                        TextContent = "This document describes blah blah blah..."
                    },
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        Title = "Points of Contact",
                        ContentType = SectionContentType.Section,
                        DisplayOrder = 1,
                        IsInternalSection = false,
                        IsRdsbRequired = false,
                        ChildNodes = new List<SectionModelView>()
                        {
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 0,
                                TextContent = "This document is prepared and blah blah blah..."
                            }
                        }
                    },
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        Title = "Oversight Agencies",
                        ContentType = SectionContentType.Section,
                        DisplayOrder = 2,
                        IsInternalSection = false,
                        IsRdsbRequired = false,
                        ChildNodes = new List<SectionModelView>()
                        {
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 0,
                                TextContent = "Denver Colorado, Sunnyvale California, blah blah blah..."
                            }
                        }
                    }
                }
            };

            SectionModelView section1 = new()
            {
                Id = -1,
                RevisionUniqueSectionId = -1,
                Title = "Overhead, G&A, Fringe Benefits and Facilities Cost of Money Rates",
                ContentType = SectionContentType.Section,
                DisplayOrder = 1,
                IsInternalSection = false,
                IsRdsbRequired = true,
                ChildNodes = new List<SectionModelView>()
                {
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        ContentType = SectionContentType.Text,
                        ChildNodes = new List<SectionModelView>(),
                        DisplayOrder = 0,
                        TextContent = "Overhead expenses are collected in pools and blah blah blah..."
                    },
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        Title = "Development Overhead",
                        ContentType = SectionContentType.Section,
                        DisplayOrder = 1,
                        IsInternalSection = false,
                        IsRdsbRequired = true,
                        ChildNodes = new List<SectionModelView>()
                        {
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 0,
                                TextContent = "The development overhead pool includes blah blah blah..."
                            },
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 1,
                                DisplayRateCode = false
                            },
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 2,
                                TextContent = "<p>2017-2019 rates reference FPRA-16-04, dated July 8, 2016</p>"
                            }
                        }
                    },
                    new()
                    {
                        Id = -1,
                        RevisionUniqueSectionId = -1,
                        Title = "Production Overhead",
                        ContentType = SectionContentType.Section,
                        DisplayOrder = 2,
                        IsInternalSection = false,
                        IsRdsbRequired = false,
                        ChildNodes = new List<SectionModelView>()
                        {
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 0,
                                TextContent = "The production overhead pool includes blah blah blah..."
                            },
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.RateTable,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 1,
                                DisplayRateCode = true
                            },
                            new()
                            {
                                Id = -1,
                                RevisionUniqueSectionId = -1,
                                ContentType = SectionContentType.Text,
                                ChildNodes = new List<SectionModelView>(),
                                DisplayOrder = 2,
                                TextContent = "<p>2017-2019 rates reference FPRA-16-04, dated July 8, 2016</p>"
                            }
                        }
                    }
                }
            };

            sectionLoader.UpdateSectionsAndContent(revision, new Collection<SectionModelView> { section0, section1 });
        }

        /// <summary>
        /// Adds baseline section data and associated rates to the revision.  This is the starting point for several section loader tests.
        /// Generate nodes to make sections and content like:
        ///           1                       2
        ///           |                     /   \
        ///          1.1                  2.1   2.2
        ///       /       \
        ///  1.1.1         1.1.2
        ///           /    /    \    \
        ///    1.1.2.1  1.1.2.2 Text  Table
        /// 
        /// </summary>
        /// <param name="revision">Revision</param>
        /// <returns>The revision</returns>
        public RevisionModelView AddBaselineSectionsAndRatesData(RevisionModelView revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

			Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

			ICollection<SectionModelView> expected = new List<SectionModelView>();
            SectionModelView section1 = new()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section
            };
            expected.Add(section1);

            SectionModelView section11 = new()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "SubTitle1",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section
            };

            section1.ChildNodes.Add(section11);

            SectionModelView section111 = new()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "SubSubTitle1",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section
            };
            section11.ChildNodes.Add(section111);

            // Add new child (i.e. 1.1.2) section to 1.1
            SectionModelView section112 = new()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1.1.2",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1.1.2"
            };

            section11.ChildNodes.Add(section112);

            // Add 1.1.2.1
            section112.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1.1.2.1",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1.1.2.1"
            });

            // Add 1.1.2.2
            section112.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1.1.2.2",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1.1.2.2"
            });

            section112.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 3,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Text1",
                TextContent = "This is some test text1.",
                DisplayRateCode = false,
                ContentType = SectionContentType.Text
            });

            section112.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 4,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Table1",
                DisplayRateCode = false,
                ContentType = SectionContentType.RateTable
            });

            // Add section 2.0 and children
            SectionModelView section2 = new()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 2",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "2"
            };
            expected.Add(section2);

            SectionModelView section21 = new()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 2.1",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "2.1"
            };
            section2.ChildNodes.Add(section21);

            SectionModelView section22 = new()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 2.2",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "2.2"
            };
            section2.ChildNodes.Add(section22);

            SectionLoader sut = new(Mock.Of<ILogger<SectionLoader>>());
            using (TransactionScope scope = new(TransactionScopeOption.Required))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are equivalent
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, false);

            // Add some rates and associate them with sections 1.1.2, 2.1 and 2.2.
            SectionModelView actualSection112 = actual.ElementAt(0).ChildNodes.ElementAt(0).ChildNodes.ElementAt(1);    // section 1.1.2
            SectionModelView actualSection2 = actual.ElementAt(1); // section 2
            SectionModelView actualSection21 = actual.ElementAt(1).ChildNodes.ElementAt(0); // section 2.1

            decimal rate = 1.11m;
            RateDetailModelView rate1 = new()
            {
                Id = -1,
                RevisionId = revision.Id,
                RateCode = "XXDDAA",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "1st rate code",
                Section = actualSection112.Id,
                Values = new Collection<RateYearModelView>() { new() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } },
                Updateable = UpdateType.Upsert
            };

            rate = 2.22m;
            RateDetailModelView rate2 = new()
            {
                Id = -2,
                RevisionId = revision.Id,
                RateCode = "XXDDAB",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "2nd rate code",
                Section = actualSection112.Id,
                Values = new Collection<RateYearModelView>() { new() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } },
                Updateable = UpdateType.Upsert
            };

            rate = 3.33m;
            RateDetailModelView rate3 = new()
            {
                Id = -3,
                RevisionId = revision.Id,
                RateCode = "XXDDAC",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "3rd rate code",
                Section = actualSection21.Id,
                Values = new Collection<RateYearModelView>() { new() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } },
                Updateable = UpdateType.Upsert
            };

            rate = 4.44m;
            RateDetailModelView rate4 = new()
            {
                Id = -4,
                RevisionId = revision.Id,
                RateCode = "XXDDAD",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "4th rate code",
                Section = actualSection2.Id,
                Values = new Collection<RateYearModelView>() { new() { Id = -1, Updateable = UpdateType.Upsert, Year = 2012, Value = rate, Dirty = true }, new() { Id = -2, Updateable = UpdateType.Upsert, Year = 2013, Value = rate, Dirty = true }, new() { Id = -3, Updateable = UpdateType.Upsert, Year = 2014, Value = rate, Dirty = true } },
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new(TransactionScopeOption.Required))
            {
                this.RateDetailLoader.SaveDetails(new Collection<RateDetailModelView> { rate1, rate2, rate3, rate4 });
                scope.Complete();
            }

            FileAttachmentRowModelView attachment1 = new()
			{
                Id = -1,
                Link = "http://www.google.com",
                Name = "Google",
                Updateable = UpdateType.Upsert,
                RevisionID = revision.Id
            };

            FileAttachmentRowModelView attachment2 = new()
			{
                Id = -2,
                Link = "http://www.bing.com",
                Name = "bing",
                SectionId = actualSection2.Id,
                Updateable = UpdateType.Upsert,
                RevisionID = revision.Id
            };

            using (TransactionScope scope = new(TransactionScopeOption.Required))
            {
                this.FileAttachmentLoader.Save(attachment1);
                this.FileAttachmentLoader.Save(attachment2);
                scope.Complete();
            }

            return revision;
        }

        #endregion Setup

        #region Helper Methods

        /// <summary>
        /// Recursively search sections for one with a matching title.
        /// </summary>
        /// <param name="parent">Parent section</param>
        /// <param name="title">Title to match</param>
        /// <returns>Matching section if found; null otherwise.</returns>
        public SectionModelView FindSectionByTitle(SectionModelView parent, string title)
        {
            if (parent == null || parent.ContentType != SectionContentType.Section)
            {
                return null;
            }

            if (parent.Title.Equals(title))
            {
                return parent;
            }

            if (parent.ChildNodes == null || !parent.ChildNodes.Any())
            {
                return null;
            }

            return parent.ChildNodes.Select(child => this.FindSectionByTitle(child, title)).FirstOrDefault(match => match != null);
        }

        /// <summary>
        /// Helper method to move a section node
        /// </summary>
        /// <param name="nodeToMove">This is the index of the node to be moved</param>
        /// <param name="fromChildNodes">The set of nodes moving from</param>
        /// <param name="toChildNodes">The set of nodes moving to</param>
        public void MoveSectionNode(int nodeToMove, ICollection<SectionModelView> fromChildNodes, ICollection<SectionModelView> toChildNodes)
        {
            if (fromChildNodes == null)
            {
                throw new ArgumentNullException(nameof(fromChildNodes));
            }

            if (toChildNodes == null)
            {
                throw new ArgumentNullException(nameof(toChildNodes));
            }

            SectionModelView sectionToMove = fromChildNodes.ElementAt(nodeToMove);
            toChildNodes.Add(sectionToMove);
            fromChildNodes.Remove(sectionToMove);
        }

        /// <summary>
        /// Recursively compares/asserts the equivalence of expected versus actual collections of SectionModelView objects.
        /// </summary>
        /// <param name="expected">Expected set of objects</param>
        /// <param name="actual">Actual set of objects</param>
        /// <param name="compareIds">if true, will compare the Id and UpdateData properties between objects;  otherwise, these properties will be ignored.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AssertSectionModelViewCollectionsAreEquivalent(ICollection<SectionModelView> expected, ICollection<SectionModelView> actual, bool compareIds = false)
        {
            if (IsCollectionNullOrEmpty(expected) && IsCollectionNullOrEmpty(actual))
            {
                return;
            }

            if (IsCollectionNullOrEmpty(expected) || IsCollectionNullOrEmpty(actual))
            {
                Assert.Fail();  // if we get here, one collection is null or empty but the other is not
            }

            // Compare number of child nodes
            Assert.AreEqual(expected.Count, actual.Count);

            // Sort the collections
            List<SectionModelView> expectedSorted = expected.OrderBy(x => x.DisplayOrder).ToList();
            List<SectionModelView> actualSorted = actual.OrderBy(x => x.DisplayOrder).ToList();

            // Compare each section MV
            for (int i = 0; i < expectedSorted.Count; i++)
            {
                AssertSectionModelViewsAreEquivalent(expectedSorted[i], actualSorted[i], compareIds);

                // Recursively compare child nodes
                this.AssertSectionModelViewCollectionsAreEquivalent(expectedSorted[i].ChildNodes, actualSorted[i].ChildNodes);
            }
        }

        /// <summary>
        /// Checks if a collection is null or empty
        /// </summary>
        /// <param name="collection">Collection</param>
        /// <returns>true if collection is null or empty; false otherwise.</returns>
        public static bool IsCollectionNullOrEmpty(ICollection<SectionModelView> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// Compares/asserts the equivalence of expected versus actual SectionModelView objects.
        /// </summary>
        /// <param name="expected">Expected MV</param>
        /// <param name="actual">Actual MV</param>
        /// <param name="compareIds">if true, will compare the Id properties between objects;  otherwise, these properties will be ignored.</param>
        public static void AssertSectionModelViewsAreEquivalent(SectionModelView expected, SectionModelView actual, bool compareIds = false)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            Assert.AreEqual(expected.DisplayOrder, actual.DisplayOrder);
            Assert.AreEqual(expected.ContentType, actual.ContentType);
            Assert.AreEqual(expected.DisplayRateCode, actual.DisplayRateCode);
            Assert.AreEqual(expected.IsInternalSection, actual.IsInternalSection);
            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.RevisionId, actual.RevisionId);
            Assert.AreEqual(expected.TextContent, actual.TextContent);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.IsRdsbRequired, actual.IsRdsbRequired);
            Assert.AreEqual(expected.SectionContainsCasbDisclosure, actual.SectionContainsCasbDisclosure);
            Assert.AreEqual(expected.SectionContainsNonCompliance, actual.SectionContainsNonCompliance);

            Assert.AreNotEqual(expected.UpdateDate, actual.UpdateDate); // ToDo: This could be removed if a dirty flag were checked before doing updates
            if (compareIds)
            {
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.ParentId, actual.ParentId);
            }
        }

        #endregion Helper Methods

        #endregion Sections

        /// <summary>
        /// Creates a random word
        /// </summary>
        /// <param name="size">size of the word</param>
        /// <param name="allowNumbers">whether or not to allow numbers</param>
        /// <param name="extraAllowedChars">any extra special characters allowed</param>
        /// <returns>the word</returns>
        public static string CreateRandomWord(int size, bool allowNumbers = false, string extraAllowedChars = "")
        {
            string viableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (allowNumbers)
            {
                viableChars = string.Concat(viableChars, "0123456789");
            }

            viableChars = string.Concat(viableChars, extraAllowedChars);

            string word = string.Empty;

            for (int i = 0; i < size; i++)
            {
                word = string.Concat(word, viableChars[randomNumberGenerator.Next(viableChars.Length)]);
            }

            return word;
        }
    }
}
