// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using DataBridge.Loaders;
	using DataBridge.ModelViews;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Test Class for Rate Code Replication Loader
	/// </summary>
	[TestClass]
    public class RateCodeReplicationLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Tests insert, update, retrieve and delete
        /// </summary>
        [TestMethod]
        public void TestRateCodeReplicationLoader()
        {
			IRateCodeReplicationLoader sut = this.testData.RateCodeReplicationLoader;

			// First delete any old data
			using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			{
				ICollection<RateCodeModelView> rateCodes = sut.GetAll();
				foreach (RateCodeModelView rateCode in rateCodes)
				{
					if ((rateCode.From == "Sample Text" && rateCode.To == "XYZ") || (rateCode.From == "Bubbly" && rateCode.To == "Champagne"))
					{
						rateCode.Updateable = UpdateType.Deleted;
						sut.Save(rateCode);
					}
				}
				
				scope.Complete();
			}

			RateCodeModelView item = new()
			{
                Id = -3,
                Updateable = UpdateType.Upsert,
                From = "Sample Text",
                To = "XYZ"
            };

            int? id = null;

            // insert
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            ICollection<RateCodeModelView> results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.From == item.From && x.To == item.To));

            item = results.First(x => x.Id == id);
            item.From = "Bubbly";
            item.To = "Champagne";
            item.Updateable = UpdateType.Upsert;

            // update
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            // run verification
            ICollection<ValidationMessage> warnings = this.testData.RateDetailLoader.VerifyRateCodeReplication(this.testData.RevisionMediator.GetWipRevision().Id);
            Assert.IsTrue(warnings.Any(w => w.ValidationIssue.Contains(item.From)));
            Assert.IsTrue(warnings.Any(w => w.ValidationIssue.Contains(item.To)));

            results = sut.GetAll();
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.From == item.From && x.To == item.To));

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
