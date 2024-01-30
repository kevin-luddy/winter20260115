// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.Common;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the user dto data mapper
    /// </summary>
    [TestClass]
    public class UserMapperTest
    {
        /// <summary>
        /// User Loader
        /// </summary>
        private Mock<IUserLoader> userLoader = null;

        /// <summary>
        /// Cache Loader
        /// </summary>
        private Mock<ICacheDataLoader> cacheLoader = null;

        /// <summary>
        ///  Security Information
        /// </summary>
        private Mock<ISecurityInformation> securityInformation = null;

        /// <summary>
        /// Active Directory Utils
        /// </summary>
        private Mock<IES.Common.IActiveDirectoryUtilities> adUtilities = null;

        /// <summary>
        /// Cache.
        /// </summary>
        private Mock<ICache> cache = null;

        /// <summary>
        /// Test get by id
        /// </summary>
        [TestMethod]
        public void M_GetByIdTest()
        {
			UserMapper sut = this.CreateSystem();

            UserDTO toReturn = new UserDTO()
            {
                Id = 15,
                FirstName = "Test",
                LastName = "Test"
            };

            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetDtoByIdDelegate>(), new object[] { toReturn.Id }, CacheConstants.USER + toReturn.Id)).Returns(toReturn);

			UserDTO result = sut.GetById(toReturn.Id);

            Assert.AreEqual(toReturn, result);
        }

        /// <summary>
        /// Test get all
        /// </summary>
        [TestMethod]
        public void M_GetAllTest()
        {
			UserMapper sut = this.CreateSystem();

            List<UserDTO> userCollection = new List<UserDTO>()
            {
                new UserDTO()
                {
                    Id = 3
                },
                new UserDTO()
                {
                    Id = 4
                }
            };

            List<int> allUserIds = new List<int>() { 3, 4 };

            this.userLoader.Setup(x => x.GetAllIds()).Returns(allUserIds);
            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetDtosByIdsDelegate<UserDTO>>(), CacheConstants.USER, It.IsAny<Dictionary<string, int>>())).Returns(userCollection);

			ICollection<UserDTO> result = sut.GetAll();

            Assert.IsTrue(result.Where(x => x.Id == userCollection[0].Id).FirstOrDefault() != null);
            Assert.IsTrue(result.Where(x => x.Id == userCollection[1].Id).FirstOrDefault() != null);
        }

        /// <summary>
        /// Test get all IDs
        /// </summary>
        [TestMethod]
        public void M_GetAllIdsTest()
        {
			UserMapper sut = this.CreateSystem();

            List<int> allUserIds = new List<int>() { 3, 4, 5, 6, 7, 8, 9 };

            this.userLoader.Setup(x => x.GetAllIds()).Returns(allUserIds);

			ICollection<int> result = sut.GetAllIds();

            for (int i = 0; i < allUserIds.Count; i++)
            {
                Assert.AreEqual(allUserIds[i], result.ElementAt(i));
            }
        }

        /// <summary>
        /// Test get by user data
        /// </summary>
        [TestMethod]
        public void M_GetByUserDataTest()
        {
        }

        /// <summary>
        /// Test Get Active User
        /// </summary>
        [TestMethod]
        public void M_GetActiveUserTest()
        {
        }

        /// <summary>
        /// Test GetUsersOnline
        /// </summary>
        [TestMethod]
        public void M_GetUsersOnlineTest()
        {
			UserMapper sut = this.CreateSystem();

            UsersOnlineDTO expectedResult = new UsersOnlineDTO()
            {
                Id = 5
            };

            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetUsersOnlineDelegate>(),
                                        It.IsAny<object[]>(),
                                        CacheConstants.USERS_ONLINE))
                                        .Returns(expectedResult);
            UsersOnlineDTO actualResult = sut.GetUsersOnline();

            DtoAssertHelpers.AssertDtos(expectedResult, actualResult);
        }

        /// <summary>
        /// Tests UpdateUserStatus
        /// </summary>
        [TestMethod]
        public void M_UpdateUsersStatusTest()
        {
			UserMapper sut = this.CreateSystem();
            string trackingNumber = "2014-00075";
            IES.Common.UserData user = new IES.Common.UserData()
            {
                FirstName = "Elizabeth",
                LastName = "Krall",
                Ntid = "empowell"
            };

            UsersOnlineDTO noDataInCache = new UsersOnlineDTO();

            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetUsersOnlineDelegate>(),
                                        It.IsAny<object[]>(),
                                        CacheConstants.USERS_ONLINE))
                                        .Returns(noDataInCache);

            sut.UpdateUsersStatus(user, trackingNumber);

            UsersOnlineDTO result = sut.GetUsersOnline();

            Assert.AreEqual(1, result.GetAllUsers().UserOnlineDetailsCollection.Count());
            Assert.IsTrue(result.ContainsUser(user.Ntid));
        }

        /// <summary>
        /// Test GetByNtidAndDomain
        /// </summary>
        [TestMethod]
        public void M_GetUserByNtidAndDomainTest()
        {
			UserMapper sut = this.CreateSystem();

            string userntid = "moquser";
            this.securityInformation.Setup(x => x.ActiveUserNTID).Returns(userntid);
            this.adUtilities.Setup(x => x.GetUserByQualifiedAccount(userntid, false)).Returns(new IES.Common.UserData());

            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetUserByNtidDelegate>(),
                                         It.IsAny<object[]>(),
                                         CacheConstants.USER + userntid))
                                .Returns(new UserDTO { Id = 2 });

            Assert.AreEqual(2, sut.GetActiveUser().Id);
        }

        /// <summary>
        /// Test User Exists
        /// </summary>
        [TestMethod]
        public void M_UserExistsTest()
        {
			UserMapper sut = this.CreateSystem();

			int notFoundUserId = 0;
            string notFoundUser = "notFoundUsr";
            this.userLoader.Setup(x => x.UserExists(notFoundUser, out notFoundUserId)).Returns(false);

			int foundUserId = 1;
            string foundUser = "foundUsr";
            this.userLoader.Setup(x => x.UserExists(foundUser, out foundUserId)).Returns(true);

            // call non cached not found user
            int userId = -1;
            Assert.IsFalse(sut.UserExists(notFoundUser, out userId), "user should not have been found");
            this.userLoader.Verify(x => x.UserExists(notFoundUser, out notFoundUserId), Times.Once());
            Assert.AreEqual(0, userId, "user wasn't found but " + notFoundUserId + " userid was returned.");

            // now call non cached not found user
            Assert.IsFalse(sut.UserExists(notFoundUser, out userId), "user should not have been found");
            this.userLoader.Verify(x => x.UserExists(notFoundUser, out notFoundUserId), Times.Exactly(2)); // user not found, DL still called, total 2
            Assert.AreEqual(0, userId, "user wasn't found but " + notFoundUserId + " userid was returned.");

            // call non cached found user
            Assert.IsTrue(sut.UserExists(foundUser, out userId), "user should not have been found");
            this.userLoader.Verify(x => x.UserExists(foundUser, out foundUserId), Times.Once()); // DL called 1 time for new parms
            Assert.AreEqual(1, userId, "user was found but " + notFoundUserId + " userid was returned.");

            // now call cached found user
            Assert.IsTrue(sut.UserExists(foundUser, out userId), "user should not have been found");
            this.userLoader.Verify(x => x.UserExists(foundUser, out foundUserId), Times.Once()); // not called this time, total still 3
            Assert.AreEqual(1, userId, "user was found but " + notFoundUserId + " userid was returned.");
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        [TestMethod]
        public void M_GetAllGroups()
        {
			UserMapper sut = this.CreateSystem();

            ICollection<int> groupIDs = new Collection<int> { 3, 4 };
            List<UserDTO> userCollection = new List<UserDTO>()
            {
                new UserDTO()
                {
                    Id = 3,
                    IsGroup = true
                },
                new UserDTO()
                {
                    Id = 4,
                    IsGroup = true
                }
            };
            this.userLoader.Setup(x => x.GetAllGroupIds()).Returns(groupIDs);
            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetDtosByIdsDelegate<UserDTO>>(), CacheConstants.USER, It.IsAny<Dictionary<string, int>>())).Returns(userCollection);

            ICollection<UserDTO> groups = sut.GetAllGroups();

            UserDTO firstEntry = (from g in groups
                             where g.Id == userCollection[0].Id
                             select g).FirstOrDefault();

            Assert.IsTrue(firstEntry.Id == 3);
            Assert.IsTrue(firstEntry.IsGroup == true);

            UserDTO secondEntry = (from g in groups
                                  where g.Id == userCollection[1].Id
                                  select g).FirstOrDefault();

            Assert.IsTrue(secondEntry.Id == 4);
            Assert.IsTrue(secondEntry.IsGroup == true);
        }

        /// <summary>
        /// Test Warm User Cache
        /// </summary>
        [TestMethod]
        public void M_WarmCacheUser()
        {
			Mock<ICache> cacheProxy = new Mock<ICache>();

            this.CreateSystem();
			CacheWarmingUserMapper sut = new CacheWarmingUserMapper(
                this.userLoader.Object,
                this.cacheLoader.Object,
                this.securityInformation.Object,
                this.adUtilities.Object,
                cacheProxy.Object);

            List<UserDTO> users = new List<UserDTO> 
            { 
                new UserDTO 
                {
                    Id = 12,
                    DisplayName = "Full Name",
                    FirstName = "Full",
                    LastName = "Name",
                    EmailAddress = "full.name@test.test",
                    Ntid = "fullname",
                    PhoneNumber = "867-5309"
                }
            };

            this.userLoader.Setup(x => x.GetAll()).Returns(users);

            sut.DoWarming();

            // verify the item was added to cache
            cacheProxy.Verify(x => x.Add(CacheConstants.USER + 12, users[0], -1), Times.Once());
            cacheProxy.Verify(x => x.Add(CacheConstants.USER + "fullname", users[0], -1), Times.Once());
        }

        #region Exception Tests

        /// <summary>
        /// Test Get By User Data Exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_GetByUserData_ExceptionTest()
        {
			UserMapper sut = this.CreateSystem();
            sut.GetByUserData(null);
        }

        /// <summary>
        /// Test an exception for get by ntid and domain
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_GetByNtid_ExceptionTest1()
        {
			UserMapper sut = this.CreateSystem();
            sut.GetByNtid(null);
        }

        /// <summary>
        /// Test an exception for get by ntid and domain
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_GetByNtid_ExceptionTest2()
        {
			UserMapper sut = this.CreateSystem();
            sut.GetByNtid(string.Empty);
        }

        #endregion Exception Tests

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>User Dto Data Mapper</returns>
        private UserMapper CreateSystem()
        {
            this.userLoader = new Mock<IUserLoader>();
            this.cacheLoader = new Mock<ICacheDataLoader>();
            this.securityInformation = new Mock<ISecurityInformation>();
            this.adUtilities = new Mock<IES.Common.IActiveDirectoryUtilities>();
            this.cache = new Mock<ICache>();

            return new UserMapper(this.userLoader.Object, this.cacheLoader.Object, this.securityInformation.Object, this.adUtilities.Object, this.cache.Object);
        }

        /// <summary>
        /// Test GetUserDtosByUserIds
        /// </summary>
        [TestMethod]
        public void M_GetUserDtosByUserIdsTest()
        {
			UserMapper sut = this.CreateSystem();

            List<UserDTO> userCollection = new List<UserDTO>()
            {
                new UserDTO()
                {
                    Id = 3,
                    DisplayName = "Three, 3"
                },
                new UserDTO()
                {
                    Id = 4,
                    DisplayName = "Four, 4"
                }
            };

            ICollection<int> allUserIds = new Collection<int>() { 3, 4 };

            this.userLoader.Setup(x => x.GetAllIds()).Returns(allUserIds);
            this.cacheLoader.Setup(x => x.GetData(It.IsAny<GetDtosByIdsDelegate<UserDTO>>(), CacheConstants.USER, It.IsAny<Dictionary<string, int>>())).Returns(userCollection);

			ICollection<UserDTO> result = sut.GetUserDtosByUserIds(allUserIds);

            Assert.IsTrue(result.Where(x => x.Id == userCollection[0].Id).FirstOrDefault() != null);
            Assert.IsTrue(result.Where(x => x.Id == userCollection[1].Id).FirstOrDefault() != null);

            // testing that this was correctly reordered
            Assert.IsTrue(result.First().DisplayName == "Four, 4"); 
        }
    }
}
