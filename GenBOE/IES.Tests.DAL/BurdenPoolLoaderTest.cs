// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using Common;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
    using IES.Common.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for  BurdenPools Loader
    /// </summary>
    [TestClass]
    public class BurdenPoolLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Tests insert, update, retrieve and delete
        /// </summary>
        [TestMethod]
        public void TestBurdenPoolLoader()
        {
            BurdenPoolLoader sut = this.testData.BurdenPoolLoader;

            // insert
            RevisionModelView revision = this.testData.GetRevision(true);
            this.testData.ModifyRevisionData1(revision);    // Add revision data including Burden Pools

            // validate the Burden Pools were inserted
            BurdenPoolGridModelView item = sut.GetByRevision(revision.Id);
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item.BurdenPools.Count(x => x.BurdenPool.Equals("MockBurdenPool1")));
            Assert.AreEqual(1, item.BurdenPools.Count(x => x.BurdenPool.Equals("MockBurdenPool2")));

            // Modify revision data including Burden Pools
            this.testData.ModifyRevisionData2(revision);

            // Verify modified data, this includes modified, and deleted burden pools
            this.testData.VerifyModifiedRevisionData2(revision);
        }

        /// <summary>
        /// Test Save (not implemented).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestLoader_Save_EX1()
        {
            BurdenPoolLoader sut = this.testData.BurdenPoolLoader;
            BurdenPoolDetailModelView bpdmv = new BurdenPoolDetailModelView();
            sut.Save(bpdmv);   // should cause exception - Save method is not supported
        }

        /// <summary>
        /// Tests Adding duplicate burden pool.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestBurdenPoolLoader_Save_EX2()
        {
            BurdenPoolLoader sut = this.testData.BurdenPoolLoader;
            ICollection<BurdenPoolDetailModelView> burdenPools = new Collection<BurdenPoolDetailModelView>();
            sut.SaveBurdenPools(burdenPools, -1);   // should throw exception because burdenPools collection is empty
        }

        /// <summary>
        /// Tests Adding duplicate burden pool.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException), "Duplicate Burden Pools are not allowed: MockBurdenPool.")]
        public void TestBurdenPoolLoader_AddDuplicate_EX1()
        {
            BurdenPoolLoader sut = this.testData.BurdenPoolLoader;

            // insert
            RevisionModelView revision = this.testData.GetRevision(true);
            BurdenPoolDetailModelView bpdmv = new BurdenPoolDetailModelView();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                bpdmv.BurdenPool = "MockBurdenPool";
                bpdmv.Description = "Mock Description";
                bpdmv.Dirty = true;
                bpdmv.IsDeleted = false;
                bpdmv.IsGaT2ApplicableForMissionSolutions = false;
                bpdmv.IncludeGaT2InBurdAndCommBurdTables = false;
                bpdmv.IsCommercial = false;
                bpdmv.Updateable = UpdateType.Upsert;
                bpdmv.Id = -1; // should cause to add
                sut.SaveBurdenPools(new Collection<BurdenPoolDetailModelView> { bpdmv }, revision.Id);

                // Try to add same burden pool.
                bpdmv.Id = -1;
                sut.SaveBurdenPools(new Collection<BurdenPoolDetailModelView> { bpdmv }, revision.Id);

                scope.Complete();
            }
        }

        /// <summary>
        /// Tests burden pool options
        /// </summary>
        [TestMethod]
        public void TestBurdenPoolLoader_Options()
        {
            BurdenPoolLoader sut = this.testData.BurdenPoolLoader;

            // insert
            RevisionModelView revision = this.testData.GetRevision(true);
            this.testData.ModifyRevisionData1(revision);    // Add revision data including Burden Pools

            // verify burden pools match options list
            BurdenPoolGridModelView item = sut.GetByRevision(revision.Id);
            ICollection<OptionModelView> commercialBurdenPoolOptions;
            ICollection<OptionModelView> governmentBurdenPoolOptions;
            sut.GetBurdenPoolOptions(revision.Id, out commercialBurdenPoolOptions, out governmentBurdenPoolOptions);
            Assert.AreEqual(item.BurdenPools.Count + 2, commercialBurdenPoolOptions.Count + governmentBurdenPoolOptions.Count);
            Assert.IsTrue(commercialBurdenPoolOptions.Count(x => x.Id == 0 && string.IsNullOrEmpty(x.Label)) == 1);
            Assert.IsTrue(governmentBurdenPoolOptions.Count(x => x.Id == 0 && string.IsNullOrEmpty(x.Label)) == 1);
        }
    }
}
