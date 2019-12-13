// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the user dto data loader
    /// </summary>
    [TestClass]
    public class UserLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Get All User Ids Test
        /// </summary>
        [TestMethod]
        public void L_GetAllUserIdsTest()
        {
            var sut = this.CreateSystem();

            ICollection<int> beforeUserIDs = sut.GetAllIds();
            this.testData.GetUser(true);
            ICollection<int> afterUserIDs = sut.GetAllIds();

            Assert.AreEqual(beforeUserIDs.Count + 1, afterUserIDs.Count);
        }

        /// <summary>
        /// Get All Users test
        /// </summary>
        [TestMethod]
        public void L_GetAllUsersTest()
        {
            var sut = this.CreateSystem();

            UserDTO testUser = this.testData.GetUser();

            ICollection<UserDTO> users = sut.GetAll();
            int totalUsers = sut.GetAllIds().Count;
            UserDTO toTest = (from x in users
                              where x.Id == testUser.Id
                              select x).FirstOrDefault();

            Assert.AreEqual(totalUsers, users.Count);
            DtoAssertHelpers.AssertDtos(testUser, toTest);
        }

        /// <summary>
        /// Tests save, get by Id, and delete
        /// </summary>
        [TestMethod]
        public void L_SaveUserAndGetUserByID()
        {
            var sut = this.CreateSystem();

            string userIdentifier = TestData.CreateRandomWord(4);
            UserDTO newUser = new UserDTO()
            {
                Id = -1,
                DisplayName = "testUser_" + userIdentifier,
                FirstName = "TestUser",
                LastName = userIdentifier,
                Ntid = "testNtid_" + userIdentifier,
                EmailAddress = "test@ptm.ssc.lmco.com",
                PhoneNumber = "867-5309",
                Updateable = UpdateType.Upsert,
                UserType = UserType.User,
                IsGroup = false
            };

            int? newUserID;
            using (TransactionScope scope = new TransactionScope())
            {
                newUserID = sut.Save(newUser);
                scope.Complete();
            }

            UserDTO toTest = sut.GetById(newUserID.Value);
            DtoAssertHelpers.AssertDtos(newUser, toTest);

            // test delete
            toTest.Updateable = UpdateType.Deleted;
            int? deletedUserID;

            using (TransactionScope scope = new TransactionScope())
            {
                deletedUserID = sut.Save(toTest);
                scope.Complete();
            }

            Assert.AreEqual(newUserID, deletedUserID);
        }

        /// <summary>
        /// Test get user by ntid and domain
        /// </summary>
        [TestMethod]
        public void L_GetUserByNtidAndDomainTest()
        {
            var sut = this.CreateSystem();

            UserDTO newUser = this.testData.GetUser(true);

            var toTest = sut.GetByNtid(newUser.Ntid);

            Assert.AreEqual(newUser.Id, toTest.Id);
            Assert.AreEqual(newUser.DisplayName, toTest.DisplayName);
            Assert.AreEqual(newUser.EmailAddress, toTest.EmailAddress);
            Assert.AreEqual(newUser.FirstName, toTest.FirstName);
            Assert.AreEqual(newUser.LastName, toTest.LastName);
            Assert.AreEqual(newUser.Ntid, toTest.Ntid);
            Assert.AreEqual(newUser.PhoneNumber, toTest.PhoneNumber);
            Assert.AreEqual(newUser.UpdateDate, toTest.UpdateDate);
            Assert.AreEqual(newUser.IsGroup, toTest.IsGroup);
        }

        /// <summary>
        /// Test get all group ids 
        /// </summary>
        [TestMethod]
        public void L_GetAllGroupIds()
        {
            var sut = this.CreateSystem();

            // create a group user
            this.testData.GetUser(true, null, true);
            var toTest = sut.GetAllGroupIds();

            Assert.IsTrue(toTest.Any());
        }

        /// <summary>
        /// Test Get Users Online
        /// </summary>
        [TestMethod]
        public void L_GetUsersOnline()
        {
            var sut = this.CreateSystem();

            UsersOnlineDTO expectedValue = new UsersOnlineDTO();

            UsersOnlineDTO actualValue = sut.GetUsersOnline();

            DtoAssertHelpers.AssertDtos(expectedValue, actualValue);
        }

        #region Exception Tests

        /// <summary>
        /// Tests null NTID
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_GetUserByNtid_Exception1()
        {
            var sut = this.CreateSystem();
            sut.GetByNtid(null);
        }

        #endregion Exception Tests

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>A user loader</returns>
        private UserLoader CreateSystem()
        {
            return new UserLoader();
        }
    }
}
