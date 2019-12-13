// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System.Collections.Generic;
    using GenTRAC.ActionLogic.GeneralHelper;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the HtmlHelper class
    /// </summary>
    [TestClass]
    public class HtmlHelperTest
    {
        #region Setup

        /// <summary>
        /// Ad Utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtils = null;

        /// <summary>
        /// User Mapper
        /// </summary>
        private Mock<IUserMapper> userMapper = null;

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>System</returns>
        public HtmlHelper CreateSystem()
        {
            this.adUtils = new Mock<IActiveDirectoryUtilities>();
            this.userMapper = new Mock<IUserMapper>();

            return new HtmlHelper(this.adUtils.Object, this.userMapper.Object);
        }

        #endregion Setup

        /// <summary>
        /// Test Breakdown Group
        /// </summary>
        [TestMethod]
        public void B_BreakdownGroup_Test()
        {
            var sut = this.CreateSystem();

            UserDTO group = new UserDTO() { Id = 15, Ntid = "test.group", DisplayName = "Group" };

            UserData user1 = new UserData() { Ntid = "user1", DisplayName = "User1" };
            UserData user2 = new UserData() { Ntid = "user2", DisplayName = "User2" };
            UserData user3 = new UserData() { Ntid = "user3", DisplayName = "User3" };

            this.userMapper.Setup(x => x.GetById(group.Id)).Returns(group);
            this.adUtils.Setup(x => x.GetAdGroupUsers(group.Ntid)).Returns(new List<UserData>() { user1, user2, user3 });

            var result = sut.BreakdownGroup(group.Id);

            string expectedHtml = "<ul><li>User1</li><li>User2</li><li>User3</li></ul>";

            Assert.AreEqual(expectedHtml, result);
        }
    }
}
