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
    using Common;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Who's Online Loader
    /// </summary>
    [TestClass]
    public class WhosOnlineLoaderTest
    {
        /// <summary>
        /// Test the Who's Online Loader
        /// </summary>
        [TestMethod]
        public void TestWhosOnlineLoader()
        {
            IWhosOnlineLoader sut = new WhosOnlineLoader();

            // Create test user data
            string testNtid = "testntid";
            string testDisplayName = "User, Test (US)";
            UserData userData = new UserData() { Ntid = testNtid, DisplayName = testDisplayName };

            // Get current Who's Online
            // If test user hasn't been added before, set initialModelView as a new MV
            ICollection<WhosOnlineModelView> initialValues = sut.GetWhosOnlineData(ApplicationName.RDM.GetDescription());
            WhosOnlineModelView initialModelView =
                initialValues.FirstOrDefault(x => x.Ntid == testNtid && x.DisplayName == testDisplayName) ??
                new WhosOnlineModelView();

            // Upsert user data to UserLog
            sut.UpdateLastAccessTime(userData, ApplicationName.RDM.GetDescription());

            // Get Updated Who's Online
            ICollection<WhosOnlineModelView> results = sut.GetWhosOnlineData(ApplicationName.RDM.GetDescription());

            // Assert results are returned
            Assert.IsTrue(results.Any());

            // Get the model view for the test user, assert it exists and that the time was updated
            // Using DateTime.Compare!=0 instead of >0 to avoid issues with timezones when running locally
            WhosOnlineModelView upsertedModelView = results.FirstOrDefault(x => x.Ntid == testNtid && x.DisplayName == testDisplayName);
            Assert.IsNotNull(upsertedModelView);
            Assert.IsTrue(DateTime.Compare(initialModelView.TimeLastAccessed, upsertedModelView.TimeLastAccessed) != 0);
        }
    }
}
