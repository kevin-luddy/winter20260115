// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
	using IES.Common.Core.Enums;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Banner Loader
    /// </summary>
    [TestClass]
    public class BannerLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Tests insert, update, retrieve and delete
        /// </summary>
        [TestMethod]
        public void TestBannerLoader()
        {
            IBannerLoader sut = this.testData.BannerLoader;

            BannerModelView item = new()
			{
                Id = -3,
                Updateable = UpdateType.Upsert,
                BannerText = "Sample Text",
                HoursToShow = 4,
                SelectedApps = new string[] { "RDM,RDSB" },
                StartDate = DateTime.Now.AddDays(-5),
                TurnOffTicker = true
            };

            int? id = null;

            // insert
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            ICollection<BannerModelView> results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.BannerText == item.BannerText && x.HoursToShow == item.HoursToShow && x.StartDate == item.StartDate && x.TurnOffTicker == item.TurnOffTicker && string.Join(",", x.SelectedApps) == string.Join(",", item.SelectedApps)));

            item = results.First(x => x.Id == id);
            item.BannerText = "Bubbly";
            item.HoursToShow = 5;
            item.SelectedApps = new string[] { "BOERMS,PTM" };
            item.StartDate = DateTime.Now.AddYears(-1);
            item.TurnOffTicker = false;
            item.Updateable = UpdateType.Upsert;

			// update
			using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.BannerText == item.BannerText && x.HoursToShow == item.HoursToShow && x.StartDate == item.StartDate && x.TurnOffTicker == item.TurnOffTicker && string.Join(",", x.SelectedApps) == string.Join(",", item.SelectedApps)));

            // delete
            item = results.First(x => x.Id == id);
            item.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetAll();
            Assert.AreEqual(0, results.Count(x => x.Id == id));
        }
    }
}
