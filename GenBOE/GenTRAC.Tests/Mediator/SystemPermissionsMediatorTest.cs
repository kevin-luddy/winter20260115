// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Mediator
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Tests.DAL;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test System PermissionsMediator
    /// </summary>
    [TestClass]
    public class SystemPermissionsMediatorTest
    {
        /// <summary>
        /// Permission mapper
        /// </summary>
        private Mock<IInternalSystemPermissionMapper> permissionsMapper = null;

        /// <summary>
        /// Create mediator
        /// </summary>
        /// <returns>Mediator</returns>
        private SystemPermissionMediator CreateSystem()
        {
            this.permissionsMapper = new Mock<IInternalSystemPermissionMapper>();

            return new SystemPermissionMediator(this.permissionsMapper.Object);
        }

        /// <summary>
        /// Save
        /// </summary>
        [TestMethod]
        public void B_SavePermissionsTest()
        {
            SystemPermissionMediator sut = this.CreateSystem();

            SystemPermissionDto toSave = new SystemPermissionDto() { Id = 24 };

            this.permissionsMapper.Setup(x => x.Save(toSave)).Returns(toSave.Id);
            this.permissionsMapper.Setup(x => x.GetById(toSave.Id)).Returns(toSave);

            SystemPermissionDto groupPermission = new SystemPermissionDto() { Id = 45, UserId = 15, Role = PtmRole.Admin };

            this.permissionsMapper.Setup(x => x.Save(groupPermission)).Returns(groupPermission.Id);
            this.permissionsMapper.Setup(x => x.GetById(groupPermission.Id)).Returns(groupPermission);

            SystemPermissionDto result = sut.SavePermissionDto(toSave);
            SystemPermissionDto groupResult = sut.SavePermissionDto(groupPermission);

            Assert.AreEqual(toSave, result);
            Assert.AreEqual(groupPermission, groupResult);
        }

        /// <summary>
        /// Save collection
        /// </summary>
        [TestMethod]
        public void B_SavePermissionsCollectionTest()
        {
            SystemPermissionMediator sut = this.CreateSystem();

            Collection<SystemPermissionDto> allPermissionsToSave = new Collection<SystemPermissionDto>();

            SystemPermissionDto toSave = new SystemPermissionDto()
            {
                Id = 24,
                UserId = 15,
                Role = PtmRole.Admin,
                Updateable = UpdateType.Upsert,
                UpdateDate = DateTime.Now
            };

            allPermissionsToSave.Add(toSave);

            this.permissionsMapper.Setup(x => x.Save(toSave)).Returns(toSave.Id);
            this.permissionsMapper.Setup(x => x.GetById(toSave.Id)).Returns(toSave);

            System.Collections.Generic.ICollection<SystemPermissionDto> result = sut.SavePermissionDtos(allPermissionsToSave);

            DtoAssertHelpers.AssertDtos(result.First(), toSave);
        }

        #region Exception Test

        /// <summary>
        /// Save with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SavePermissionsTest_Exception()
        {
            SystemPermissionMediator sut = this.CreateSystem();
            sut.SavePermissionDto(null);
        }

        /// <summary>
        /// Save collection with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SavePermissionsCollectionTest_Exception()
        {
            SystemPermissionMediator sut = this.CreateSystem();
            sut.SavePermissionDtos(null);
        }

        #endregion Exception Test
    }
}
