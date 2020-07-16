// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for Offline Application Loader
    /// </summary>
    [TestClass]
    public class OfflineApplicationLoaderTest
    {
        private OfflineApplicationLoader offlineApplicationLoader;

        private void CreateSystem()
        {
            this.offlineApplicationLoader = new OfflineApplicationLoader();
        }

        /// <summary>
        /// Test GetAll
        /// </summary>
        [TestMethod]
        public void GetAllTest()
        {
            this.CreateSystem();
            ICollection<OfflineApplicationModelView> results = offlineApplicationLoader.GetAll();
            Assert.IsTrue(results.Any());
        }

        /// <summary>
        /// Test GetByApplication
        /// </summary>
        [TestMethod]
        public void GetByApplicationTest()
        {
            this.CreateSystem();
            OfflineApplicationModelView result = offlineApplicationLoader.GetByApplication("BOE SSC");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.ApplicationName, "BOE SSC");
        }

        /// <summary>
        /// Test Update
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            this.CreateSystem();

            OfflineApplicationModelView testApp = offlineApplicationLoader.GetByApplication("BOE SSC");
            Assert.IsNotNull(testApp);

            // Create a mv with the opposite IsOffline value to save
            OfflineApplicationModelView toUpdateTestApp = new OfflineApplicationModelView()
            {
                ApplicationName = testApp.ApplicationName,
                UpdateDate = testApp.UpdateDate,
                IsOffline = !testApp.IsOffline
            };

            offlineApplicationLoader.Update(toUpdateTestApp);

            // Check that it updated
            OfflineApplicationModelView updatedTestApp = offlineApplicationLoader.GetByApplication("BOE SSC");

            Assert.IsNotNull(updatedTestApp);
            Assert.AreEqual(toUpdateTestApp.IsOffline, updatedTestApp.IsOffline);

            //revert to original value
            updatedTestApp.IsOffline = testApp.IsOffline;
            offlineApplicationLoader.Update(updatedTestApp);
        }

        /// <summary>
        /// Test Update fails for null argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateTest_Ex()
        {
            this.CreateSystem();
            offlineApplicationLoader.Update(null);
        }

        /// <summary>
        /// Test that GetByIds throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetByIdsTest()
        {
            this.CreateSystem();
            offlineApplicationLoader.GetByIds(null);
        }

        /// <summary>
        /// Test that Upsert throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void UpsertTest()
        {
            this.CreateSystem();
            offlineApplicationLoader.Save(new OfflineApplicationModelView() { Updateable = UpdateType.Upsert });
        }

        /// <summary>
        /// Test that Delete throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void DeleteTest()
        {
            this.CreateSystem();
            offlineApplicationLoader.Save(new OfflineApplicationModelView() { Updateable = UpdateType.Deleted });
        }
    }
}
