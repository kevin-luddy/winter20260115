using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.DataBridge.DTO;
using GenBOE.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Dtos;
using IES.Common;
using Moq;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class UserDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetAllUsers()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            int results = sut.GetAllUsers().Count();
            int totalUsersFromDB;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                totalUsersFromDB = (from u in gbe.ETIusers
                                    select u).Count();
            }

            Assert.AreEqual(totalUsersFromDB, results, "The number of users returned did not match the number in the database.");
        }

        [TestMethod]
        public void L_GetByIds()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            ICollection<int> userIds = new Collection<int>() { this.WorkspaceAdmin.UserID, this.Author.UserID };

            ICollection<UserDTO> users = sut.GetByIds(userIds);

            Assert.IsTrue(users.Count == 2);
            Assert.IsNotNull(users.FirstOrDefault(u => u.UserID == this.WorkspaceAdmin.UserID));
            Assert.IsNotNull(users.FirstOrDefault(u => u.UserID == this.Author.UserID));
        }

        [TestMethod]
        public void L_UserExists()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            int userId = 0;
            Assert.IsTrue(sut.UserExists(this.Author.NTID, out userId));
            Assert.IsTrue(userId == this.Author.UserID);
            Assert.IsFalse(sut.UserExists("bogusNtId", out userId));
        }

        [TestMethod]
        public void L_GetUserById()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            Assert.IsTrue(sut.GetUserByID(this.Author.UserID).UserID == this.Author.UserID);
        }

        [TestMethod]
        public void L_GetOrCreateUserByNtid()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            Assert.IsTrue(sut.GetOrCreateUserByNtid(this.Author.NTID).UserID == this.Author.UserID);
        }

        [TestMethod]
        public void SaveUser()
        {
            UserDTO userDto = new UserDTO();
           
            string firstPartNTID =  Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string secondPartNTID = Math.Abs(new Random((int)DateTime.Now.Ticks).Next()).ToString();

            userDto.UserID = -1;
            userDto.NTID = firstPartNTID.Substring(0, 4) + secondPartNTID.Substring(0, 4);
            userDto.DisplayName = "test account";
            userDto.EmailAddress = "test@lmco.com";
            userDto.PhoneNumber = "610/555-1212";
            userDto.UpdateDate = DateTime.Now;
            userDto.IsUsPerson = true;
            userDto.IsSubcontractor = false;

            //SUT
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<MemoryCache> memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            UserDTODataLoader sut = new UserDTODataLoader(securityInformation, adUtils.Object);

            ICollection<UserDTO> beforeSave = sut.GetAllUsers();

            UserDTO newUser = sut.SaveUser(userDto);

            ICollection<UserDTO> afterSave = sut.GetAllUsers();

            // Assert
            Assert.IsTrue(afterSave.Count > beforeSave.Count);

            int userId;
            Assert.IsTrue(sut.UserExists(userDto.NTID, out userId));
            Assert.IsNotNull(userId, "user existed, but Id was not returned correctly");
            Assert.AreEqual(newUser.UserID, userId);
            Assert.AreEqual(userDto.NTID.ToLower(), newUser.NTID.ToLower());
            Assert.AreEqual(userDto.DisplayName, newUser.DisplayName);
            Assert.AreEqual(userDto.EmailAddress, newUser.EmailAddress);
            Assert.AreEqual(userDto.PhoneNumber, newUser.PhoneNumber);
            Assert.AreEqual(userDto.IsUsPerson, newUser.IsUsPerson);
            Assert.AreEqual(userDto.IsSubcontractor, newUser.IsSubcontractor);

            Assert.IsFalse(sut.UserExists("gibflib12", out userId));
            Assert.AreEqual(0, userId, "user should not have existed, but Id returned was " + userId);

            UserDTO user = sut.GetOrCreateUserByNtid(userDto.NTID);
            Assert.IsNotNull(user);
        }
    }
}
