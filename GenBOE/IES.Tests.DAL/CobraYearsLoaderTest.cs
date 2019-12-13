// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using Common;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Cobra Years Loader
    /// </summary>
    [TestClass]
    public class CobraYearsLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Tests insert, update, retrieve and delete
        /// </summary>
        [TestMethod]
        public void TestCobraYearsLoader()
        {
            CobraYearsLoader sut = this.testData.CobraYearsLoader;

            CobraYearGridModelView item = new CobraYearGridModelView()
            {
                CobraDate = DateTime.Now.Date,
                Updateable = UpdateType.Upsert,
                Year = DateTime.Now.Year
            };

            int? id = null;

            // insert
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            ICollection<CobraYearGridModelView> results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.CobraDate == item.CobraDate.Date.AddHours(12) && x.Year == item.Year));

            item = results.First(x => x.Id == id);
            item.Year = DateTime.Now.AddYears(10).Year;
            item.CobraDate = DateTime.Now.Date.AddDays(-100);
            item.Updateable = UpdateType.Upsert;

            // update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.CobraDate == item.CobraDate.Date.AddHours(12) && x.Year == item.Year));

            // delete
            item = results.First(x => x.Id == id);
            item.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetAll();
            Assert.AreEqual(0, results.Count(x => x.Id == id));
        }

        /// <summary>
        /// Tests Cobra Export
        /// </summary>
        [TestMethod]
        public void TestGetCobraYearExportRows()
        {
            CobraYearsLoader sut = this.testData.CobraYearsLoader;

            // Create a revision
            RevisionModelView revision = this.testData.GetRevision(true);
            Assert.IsNotNull(revision);

            // Add data (i.e. sections, rates, burden pools, etc.)
            this.testData.ModifyRevisionData1(revision);

            // Modify data in the revision
            this.testData.ModifyRevisionData2(revision);

            // generate cobra export rows starting with 2017
            int startYear = 2017;
            ICollection<CobraExportRowModelView> cobraExportRows = sut.GetCobraExportRows(revision.Id, startYear);
             this.ValidateCobraExportRows(cobraExportRows, startYear);

            // generate cobra export rows starting with 2018
            startYear = 2018;
            cobraExportRows = sut.GetCobraExportRows(revision.Id, startYear);
            this.ValidateCobraExportRows(cobraExportRows, startYear);
        }

        /// <summary>
        /// Helper method to validate COBRA export rows for various startYear values.
        /// </summary>
        /// <param name="cobraExportRows">COBRA export rows to validate</param>
        /// <param name="startYear">Start Year</param>
        private void ValidateCobraExportRows(ICollection<CobraExportRowModelView> cobraExportRows, int startYear)
        {
            // validate results
            string svcctr = Code1.SVCCTR.GetDescription();
            string indirect = Code1.INDIRECT.GetDescription();

            ICollection<CobraYearGridModelView> cobraYears = this.testData.CobraYearsLoader.GetAll();

            if (startYear == 2017)
            {
                DateTime dt2017 = cobraYears.Single(x => x.Year == 2017).CobraDate;
                Assert.AreEqual(12, cobraExportRows.Count);
                Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2017.Date && x.Description.Equals("Mock Rate 1a") && x.RateCode.Equals("MockRate1") && x.RateSet.Equals("MockRateSet1") && x.Value == 11.170000m));
                Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2017.Date && x.Description.Equals("Mock Rate 2a") && x.RateCode.Equals("MockRate2") && x.RateSet.Equals("MockRateSet2") && x.Value == null));
                Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(indirect) && x.Date.Date == dt2017.Date && x.Description.Equals("Mock Rate 3") && x.RateCode.Equals("MockRate3") && x.RateSet.Equals("MockRateSet1") && x.Value == 3.17m));
                Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(indirect) && x.Date.Date == dt2017.Date && x.Description.Equals("Mock Rate 4") && x.RateCode.Equals("MockRate4") && x.RateSet.Equals("MockRateSet1") && x.Value == 4.17m));
                Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2017.Date && x.Description.Equals("Mock Rate 5") && x.RateCode.Equals("MockRate5") && x.RateSet.Equals("MockRateSet1") && x.Value == 5.17m));
            }
            else
            {
                // startYear = 2018
                Assert.AreEqual(7, cobraExportRows.Count);
            }

            // validate data for 2018 and 2019
            DateTime dt2018 = cobraYears.Single(x => x.Year == 2018).CobraDate;
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2018.Date && x.Description.Equals("Mock Rate 1a") && x.RateCode.Equals("MockRate1") && x.RateSet.Equals("MockRateSet1") && x.Value == 11.18m));
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2018.Date && x.Description.Equals("Mock Rate 2a") && x.RateCode.Equals("MockRate2") && x.RateSet.Equals("MockRateSet2") && x.Value == 2.18m));
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(indirect) && x.Date.Date == dt2018.Date && x.Description.Equals("Mock Rate 3") && x.RateCode.Equals("MockRate3") && x.RateSet.Equals("MockRateSet1") && x.Value == 3.18m));
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(indirect) && x.Date.Date == dt2018.Date && x.Description.Equals("Mock Rate 4") && x.RateCode.Equals("MockRate4") && x.RateSet.Equals("MockRateSet1") && x.Value == 44.18m));
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2018.Date && x.Description.Equals("Mock Rate 5") && x.RateCode.Equals("MockRate5") && x.RateSet.Equals("MockRateSet1") && x.Value == 5.18m));
            DateTime dt2019 = cobraYears.Single(x => x.Year == 2019).CobraDate;
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(svcctr) && x.Date.Date == dt2019.Date && x.Description.Equals("Mock Rate 2a") && x.RateCode.Equals("MockRate2") && x.RateSet.Equals("MockRateSet2") && x.Value == 22.19m));
            Assert.AreEqual(1, cobraExportRows.Count(x => x.Code1.Equals(indirect) && x.Date.Date == dt2019.Date && x.Description.Equals("Mock Rate 4") && x.RateCode.Equals("MockRate4") && x.RateSet.Equals("MockRateSet1") && x.Value == 44.19m));
        }
    }
}
