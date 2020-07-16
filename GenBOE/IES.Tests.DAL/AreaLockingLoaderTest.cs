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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Area Locking Loader
    /// </summary>
    [TestClass]
    public class AreaLockingLoaderTest
    {
        /// <summary>
        /// Test GetActiveLocksByOtherUsers with null user.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestLoader_EX1()
        {
            AreaLockingLoader sut = new AreaLockingLoader();
            sut.GetActiveLocksByOtherUsers(null);
        }

        /// <summary>
        /// Test the loader
        /// </summary>
        [TestMethod]
        public void TestLoader()
        {
            AreaLockingLoader sut = new AreaLockingLoader();

            AreaLockData data = new AreaLockData();
            data.TimeOfLock = DateTime.Now;
            data.LockedBy = new UserData()
            {
                Ntid = "testuser",
                DisplayName = "Test User"
            };

            // Using None for testing
            LockArea area = LockArea.None;
            
            // Test LockArea and GetAreaLock
            sut.LockArea(area, data);
            AreaLockData result = sut.GetAreaLock(area);

            // Assert lock is stored properly
            Assert.AreEqual(data.TimeOfLock, result.TimeOfLock);
            Assert.AreEqual(data.LockedBy.Ntid, result.LockedBy.Ntid);
            Assert.AreEqual(data.LockedBy.DisplayName, result.LockedBy.DisplayName);

            // Test Unlock Area (and GetAreaLock again)
            sut.UnlockArea(area);
            result = sut.GetAreaLock(area);

            // Assert is null after removing lock
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test the GetActiveLocksByOtherUsers method
        /// </summary>
        [TestMethod]
        public void TestGetActiveLocksByOtherUsers()
        {
            AreaLockingLoader sut = new AreaLockingLoader();

            // Create two locks made by different users
            UserData user1 = new UserData()
            {
                Ntid = "testuser1",
                DisplayName = "Test User 1"
            };

            UserData user2 = new UserData()
            {
                Ntid = "testuser2",
                DisplayName = "Test User 2"
            };

            AreaLockData lock1 = new AreaLockData()
            {
                Area = LockArea.RDMSections,
                TimeOfLock = DateTime.Now.AddMinutes(-1),
                LockedBy = user1.DeepClone()
            };

            AreaLockData lock2 = new AreaLockData()
            {
                Area = LockArea.RDMBurdenPools,
                TimeOfLock = DateTime.Now.AddMinutes(1),
                LockedBy = user2.DeepClone()
            };

            // Create locks
            sut.LockArea(LockArea.RDMSections, lock1);
            sut.LockArea(LockArea.RDMBurdenPools, lock2);

            ICollection<AreaLockData> result = sut.GetActiveLocksByOtherUsers(user1);

            // Assert we get user2's lock and not user1's
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(lock2.Area, result.ElementAt(0).Area);
            Assert.AreEqual(lock2.TimeOfLock, result.ElementAt(0).TimeOfLock);
            Assert.AreEqual(lock2.LockedBy.Ntid, result.ElementAt(0).LockedBy.Ntid);
            Assert.AreEqual(lock2.LockedBy.DisplayName, result.ElementAt(0).LockedBy.DisplayName);

            // Remove locks
            sut.UnlockArea(LockArea.RDMSections);
            sut.UnlockArea(LockArea.RDMBurdenPools);
        }
    }
}