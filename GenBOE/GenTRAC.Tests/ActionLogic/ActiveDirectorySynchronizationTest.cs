// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System.Collections.Generic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.Synchronization;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the active directory synchronization
    /// </summary>
    [TestClass]
    public class ActiveDirectorySynchronizationTest
    {
        /// <summary>
        /// Test the sync update users method
        /// </summary>
        [TestMethod]
        public void SyncUpdateUsers()
        {
            Mock<IActiveDirectoryUtilities> adUtils = new Mock<IActiveDirectoryUtilities>();
            Mock<IUserMapper> userMapper = new Mock<IUserMapper>();
            Mock<IUserMediator> userMediator = new Mock<IUserMediator>();

            UserDTO joesmith = new UserDTO
            {
                DisplayName = "joe smith",
                EmailAddress = "joesmith@moq.com",
                FirstName = "joe",
                LastName = "smith",
                PhoneNumber = "5555555555",
                Ntid = "joesmith"
            };

            UserDTO janesmith = new UserDTO
            {
                DisplayName = "jane smith",
                EmailAddress = "janesmith@moq.com",
                FirstName = "jane",
                LastName = "smith",
                PhoneNumber = "5555555555",
                Ntid = "janesmith"
            };

            ICollection<UserDTO> allUsers = new List<UserDTO>();
            allUsers.Add(joesmith);
            allUsers.Add(janesmith);

            UserData joesmithAD = new UserData
            {
                DisplayName = "joe smith",
                Email = "joesmith@moq.com",
                FirstName = "joe",
                LastName = "smith",
                Phone = "5555555555"
            };
            UserData janesmithAD = new UserData
            {
                DisplayName = "jane smith",
                Email = "janesmith@moq.com",
                FirstName = "jane",
                LastName = "smith",
                Phone = "5555555555"
            };

            userMapper.Setup(x => x.GetAll()).Returns(allUsers);
            adUtils.Setup(x => x.GetUserByQualifiedAccount(joesmith.Ntid, false)).Returns(joesmithAD);
            adUtils.Setup(x => x.GetUserByQualifiedAccount(janesmith.Ntid, false)).Returns(janesmithAD);
            adUtils.Setup(x => x.GetAdGroupUsers(It.IsAny<string>())).Returns(new List<UserData>());

            ActiveDirectorySynchronization sut = new ActiveDirectorySynchronization(adUtils.Object, userMapper.Object, userMediator.Object);

            // test with no changes to the user .. verify save user wasn't called
            sut.SyncUpdateUsers();

            userMediator.Verify(x => x.SaveUser(It.IsAny<UserDTO>()), Times.Never());

            // test with changes to the user 'joesmith' .. verify save user was called once
            joesmithAD.Phone = "5555551234";

            sut.SyncUpdateUsers();

            userMediator.Verify(x => x.SaveUser(It.IsAny<UserDTO>()), Times.Once());

            // test with changes to the user 'joesmith' and 'janesmith' .. verify save user was called twice
            joesmithAD.Phone = "5555551234";
            janesmithAD.DisplayName = "jane doe";

            sut.SyncUpdateUsers();

            userMediator.Verify(x => x.SaveUser(It.IsAny<UserDTO>()), Times.Exactly(2));
        }
    }
}
