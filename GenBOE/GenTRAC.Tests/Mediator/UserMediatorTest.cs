// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Mediator
{
    using System;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test user mediator
    /// </summary>
    [TestClass]
    public class UserMediatorTest
    {
        /// <summary>
        /// The user mapper
        /// </summary>
        private Mock<IInternalUserMapper> userMapper = null;

        /// <summary>
        /// Test saving a user
        /// </summary>
        [TestMethod]
        public void B_SaveUser()
        {
            UserMediator sut = this.CreateSystem();

            UserDTO userToSave = new UserDTO()
            {
                Id = 15
            };

            this.userMapper.Setup(x => x.Save(userToSave)).Returns(userToSave.Id);

            int? result = sut.SaveUser(userToSave);
            Assert.AreEqual(userToSave.Id, result);
        }

        #region Exception Tests

        /// <summary>
        /// Test Save User Exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SaveUser_Exception()
        {
            UserMediator sut = this.CreateSystem();

            sut.SaveUser(null);
        }

        #endregion Exception Tests

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>User Mediator</returns>
        private UserMediator CreateSystem()
        {
            this.userMapper = new Mock<IInternalUserMapper>();

            return new UserMediator(this.userMapper.Object);
        }
    }
}
