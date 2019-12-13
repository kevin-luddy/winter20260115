// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test Home Controller Logic
    /// </summary>
    [TestClass]
    public class HomeControllerLogicTest
    {
        /// <summary>
        /// Controller logic sut
        /// </summary>
        private HomeControllerLogic sut;

        /// <summary>
        /// Mock Revision Loader
        /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new Mock<IRevisionMediator>();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new Mock<IAreaLockingLoader>();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<ActiveDirectoryUtilities> adUtils = new Mock<ActiveDirectoryUtilities>();

        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        /// <summary>
        /// Test User Data
        /// </summary>
        private UserData userData = new UserData();

        /// <summary>
        /// Test User Data for second user
        /// </summary>
        private UserData userData2 = new UserData();

        /// <summary>
        /// Area lock data
        /// </summary>
        private AreaLockData areaLockData = new AreaLockData();

        /// <summary>
        /// Active Users
        /// </summary>
        private readonly ICollection<AreaLockData> activeUsers = new Collection<AreaLockData>();

        /// <summary>
        /// Active Users when second user has lock
        /// </summary>
        private ICollection<AreaLockData> activeUsers2 = new Collection<AreaLockData>();

        /// <summary>
        /// Initialize test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.areaLockingLoader = new Mock<IAreaLockingLoader>();
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);

            this.sut = new HomeControllerLogic(this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object);

            this.userData = new UserData() { DisplayName = "Test User", Ntid = "test1" };
            this.userData2 = new UserData() { DisplayName = "Test User2", Ntid = "test2" };
            this.areaLockData = new AreaLockData() { Area = LockArea.None, TimeOfLock = DateTime.Now, LockedBy = this.userData.DeepClone() };
            this.activeUsers2 = new Collection<AreaLockData>() { new AreaLockData() { Area = LockArea.RDMBurdenPools, LockedBy = this.userData, TimeOfLock = DateTime.Now } };
            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(userData.Ntid, false)).Returns(userData);
            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(userData2.Ntid, false)).Returns(userData2);
        }

        /// <summary>
        /// Test LockArea() for adding a new lock without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestLockArea_EX1()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            this.sut.LockArea(LockArea.None, false, out status, out message);
        }

        /// <summary>
        /// Test LockArea() for adding a new lock without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestLockArea_EX2()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.LockArea(LockArea.None, false, out status, out message);
        }

        /// <summary>
        /// Test LockArea() for adding a new lock for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestLockArea_EX3()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDMAdmin
            this.sut.LockArea(LockArea.CobraData, false, out status, out message);
        }

        /// <summary>
        /// Test LockArea() for adding a new lock for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestLockArea_EX4()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.LockArea(LockArea.CobraData, false, out status, out message);
        }

        /// <summary>
        /// Test LockArea() for adding a new lock
        /// </summary>
        [TestMethod]
        public void TestLockArea_AddLock()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.LockArea(LockArea.None, false, out status, out message);

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.LockArea(LockArea.None, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.LockArea(LockArea.CobraData, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.LockArea(LockArea.CobraData, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// Test LockAllAreas() for adding new locks
        /// </summary>
        [TestMethod]
        public void TestLockAllAreas_AddLock()
        {
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            this.SetupGetLockAreaNull();

            LockModelView result2 = this.sut.GetCurrentLockInfo(LockArea.RDMBurdenPools);
            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.IsReadOnly);
            Assert.IsTrue(result2.IsLockAllowed);
            Assert.IsNull(result2.InUse);

            bool status;
            string message;

            LockModelView result = this.sut.LockAllAreas(out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// Test LockArea() for when a lock already exists
        /// </summary>
        [TestMethod]
        public void TestLockArea_ExistingLock()
        {
            this.SetupGetLockArea();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.LockArea(LockArea.None, false, out status, out message);

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.LockArea(LockArea.None, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.LockArea(LockArea.CobraData, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.LockArea(LockArea.CobraData, false, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// Test LockAllAreas() for when a lock already exists
        /// </summary>
        [TestMethod]
        public void TestLockAllAreas_ExistingLock()
        {
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            this.SetupGetLockAllAreas();

            LockModelView result2 = this.sut.GetCurrentLockInfo(LockArea.RDMBurdenPools);
            Assert.IsNotNull(result2);
            Assert.IsFalse(result2.IsReadOnly);
            Assert.IsTrue(result2.IsLockAllowed);
            Assert.AreEqual(this.areaLockData.TimeOfLock, result2.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result2.Editing);

            bool status;
            string message;

            LockModelView result = this.sut.LockAllAreas(out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);
            // Commented this out because this InUse is a different DateTime from the Burden Pool's Lock Time
            // When running locally (and on the server since it slowed down), the times are just off enough to cause the Assert to fail
            // Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// Test UnlockArea() without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestUnlockArea_EX1()
        {
            this.SetupGetLockAreaNull();
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            this.sut.UnlockArea(LockArea.None, false);
        }

        /// <summary>
        /// Test UnlockArea() without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestUnlockArea_EX2()
        {
            this.SetupGetLockAreaNull();
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.UnlockArea(LockArea.None, false);
        }

        /// <summary>
        /// Test UnlockArea() for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestUnlockArea_EX3()
        {
            this.SetupGetLockAreaNull();
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDMAdmin
            this.sut.UnlockArea(LockArea.CobraData, false);
        }

        /// <summary>
        /// Test UnlockArea() for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestUnlockArea_EX4()
        {
            this.SetupGetLockAreaNull();
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.UnlockArea(LockArea.CobraData, false);
        }

        /// <summary>
        /// Test UnlockArea() for when there is no lock
        /// </summary>
        [TestMethod]
        public void TestUnlockArea_NoLock()
        {
            this.SetupGetLockAreaNull();

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);
        }

        /// <summary>
        /// Test UnlockAllAreas() for when there is no lock
        /// </summary>
        [TestMethod]
        public void TestUnlockAllAreas_NoLock()
        {
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            this.SetupGetLockAreaNull();
            this.SetupGetUnlockAllAreas();

            ICollection<AreaLockData> result = this.sut.UnlockAllAreas();

            Assert.AreEqual(this.activeUsers, result);
        }

        /// <summary>
        /// Test UnlockArea() for when the user owns the lock
        /// </summary>
        [TestMethod]
        public void TestUnlockArea_UserOwnsLock()
        {
            this.SetupGetLockArea();

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNull(result.InUse);
            Assert.AreEqual(string.Empty, result.Editing);
        }

        /// <summary>
        /// Test UnlockAllAreas() for when the user owns a lock
        /// </summary>
        [TestMethod]
        public void TestUnlockAllAreas_UserOwnsLock()
        {
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            this.SetupGetLockAllAreas();
            this.SetupGetUnlockAllAreas();

            ICollection<AreaLockData> result = this.sut.UnlockAllAreas();

            Assert.AreEqual(this.activeUsers, result);
        }

        /// <summary>
        /// Test UnlockArea() for when the user does not own the lock
        /// </summary>
        [TestMethod]
        public void TestUnlockArea_UserDoesntOwnLock()
        {
            this.SetupGetLockArea();

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData2;  // user doesn't own lock
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result.Editing);

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.None, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result.Editing);

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result.Editing);

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.UnlockArea(LockArea.CobraData, false);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.areaLockData.LockedBy.DisplayName, result.Editing);
        }

        /// <summary>
        /// Test UnlockAllAreas() for when the user does not own the lock
        /// </summary>
        [TestMethod]
        public void TestUnlockAllAreas_UserDoesntOwnLock()
        {
            this.SetupGetLockAllAreas();
            this.SetupGetUnlockAllAreas();
            UserData activeUser = this.userData2;  // user doesn't own lock
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin

            ICollection<AreaLockData> result = this.sut.UnlockAllAreas();

            Assert.AreEqual(this.activeUsers2, result);
        }


        /// <summary>
        /// Test RefreshLockOnArea() without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestRefreshLockOnArea_EX1()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            this.sut.RefreshLockOnArea(LockArea.None, out status, out message);
        }

        /// <summary>
        /// Test RefreshLockOnArea() without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestRefreshLockOnArea_EX2()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.RefreshLockOnArea(LockArea.None, out status, out message);
        }

        /// <summary>
        /// Test RefreshLockOnArea() for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestRefreshLockOnArea_EX3()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDMAdmin
            this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);
        }

        /// <summary>
        /// Test RefreshLockOnArea() for Cobra Data without proper authorization
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void TestRefreshLockOnArea_EX4()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, false, false, true);  // RDMViewer
            this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);
        }
        /// <summary>
        /// Test RefreshLockOnArea for when there is no lock
        /// </summary>
        [TestMethod]
        public void TestRefreshLockOnArea_NoLock()
        {
            this.SetupGetLockAreaNull();

            bool status;
            string message;

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData;
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);
        }

        /// <summary>
        /// Test RefreshLockOnArea for when the user owns the lock
        /// </summary>
        [TestMethod]
        public void TestRefreshLockOnArea_UserOwnsLock()
        {
            this.SetupGetLockArea();

            bool status;
            string message;

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData;  // user owns lock
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsFalse(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.IsNotNull(result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsTrue(status);
            Assert.AreEqual(string.Empty, message);
        }

        /// <summary>
        /// Test RefreshLockOnArea for when the user does not own the lock
        /// </summary>
        [TestMethod]
        public void TestRefreshLockOnArea_UserDoesntOwnLock()
        {
            this.SetupGetLockArea();

            bool status;
            string message;

            // ----------------------------------------------------------------
            // Test LockArea.None (same logic for all other areas except CobraData)
            // ----------------------------------------------------------------
            UserData activeUser = this.userData2;  // user doesn't own lock
            this.SetupRdmUserPermissions(activeUser, true, false, false);  // RDM Admin
            LockModelView result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsFalse(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.None, out status, out message);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);    // editing allowed for RDMAdmins
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsFalse(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            // ----------------------------------------------------------------
            // Test LockArea.CobraData
            // ----------------------------------------------------------------
            this.SetupRdmUserPermissions(activeUser, true, true, false);  // RDMAdmin and RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsFalse(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));

            this.SetupRdmUserPermissions(activeUser, false, true, false);  // RDMCobraAdmin
            result = this.sut.RefreshLockOnArea(LockArea.CobraData, out status, out message);

            Assert.IsTrue(result.IsReadOnly);
            Assert.IsTrue(result.IsLockAllowed);   // editing allowed for RDMCobraAdmins 
            Assert.AreEqual(this.areaLockData.TimeOfLock, result.InUse);
            Assert.AreEqual(this.userData.DisplayName, result.Editing);
            Assert.IsFalse(status);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// Set up Security Information to Mock RDM user for desired permission configurations
        /// </summary>
        /// <param name="activeUser">User to mock</param>
        /// <param name="isRdmAdminUser">Value to mock for IsRdmAdminUser.</param>
        /// <param name="isRdmCobraAdminUser">Value to mock for IsRdmCobraAdminUser.</param>
        /// <param name="isRdmViewerUser">Value to mock for IsRdmViewerUser.</param>
        private void SetupRdmUserPermissions(UserData activeUser, bool isRdmAdminUser, bool isRdmCobraAdminUser, bool isRdmViewerUser)
        {
            this.securityInfo.Setup(x => x.ActiveUserData).Returns(activeUser);
            this.securityInfo.Setup(x => x.ActiveUserNTID).Returns(activeUser.Ntid);
            this.securityInfo.Setup(x => x.IsRdmAdminUser(activeUser.Ntid)).Returns(isRdmAdminUser);
            this.securityInfo.Setup(x => x.IsRdmCobraAdminUser(activeUser.Ntid)).Returns(isRdmCobraAdminUser);
            this.securityInfo.Setup(x => x.IsRdmViewerUser(activeUser.Ntid)).Returns(isRdmViewerUser);
        }

        /// <summary>
        /// Set up AreaLockingLoader.GetLockArea to return a result
        /// </summary>
        private void SetupGetLockArea()
        {
            this.areaLockingLoader.Setup(x => x.GetAreaLock(LockArea.None)).Returns(this.areaLockData);
            this.areaLockingLoader.Setup(x => x.GetAreaLock(LockArea.CobraData)).Returns(this.areaLockData);
        }

        /// <summary>
        /// Set up AreaLockingLoader.GetLockArea to return a result for RDM Burden Pools.
        /// </summary>
        private void SetupGetLockAllAreas()
        {
            this.areaLockingLoader.Setup(x => x.GetAreaLock(LockArea.RDMBurdenPools)).Returns(this.areaLockData);
        }

        /// <summary>
        /// Set up AreaLockingLoader.GetActiveLocksByOtherUsers to return
        /// different results depending on the user.
        /// </summary>
        private void SetupGetUnlockAllAreas()
        {
            this.areaLockingLoader.Setup(x => x.GetActiveLocksByOtherUsers(this.userData)).Returns(this.activeUsers);
            this.areaLockingLoader.Setup(x => x.GetActiveLocksByOtherUsers(this.userData2)).Returns(this.activeUsers2);
        }

        /// <summary>
        /// Set up AreaLockingLoader.GetLockArea to return null
        /// </summary>
        private void SetupGetLockAreaNull()
        {
            this.areaLockingLoader.Setup(x => x.GetAreaLock(LockArea.None)).Returns((AreaLockData)null);
            this.areaLockingLoader.Setup(x => x.GetAreaLock(LockArea.CobraData)).Returns((AreaLockData)null);
        }
    }
}
