// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Permissions Mapper Test
    /// </summary>
    [TestClass]
    public class SystemPermissionMapperTest
    {
        #region Setup

        /// <summary>
        /// System Permission Loader
        /// </summary>
        private Mock<ISystemPermissionLoader> systemPermissionsLoader = null;

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>Permission Mapper</returns>
        private SystemPermissionMapper CreateSystem()
        {
            this.systemPermissionsLoader = new Mock<ISystemPermissionLoader>();

            SystemPermissionMapper system = new SystemPermissionMapper(
                this.systemPermissionsLoader.Object);

            return system;
        }

        #endregion Setup

        /// <summary>
        /// Get by Id
        /// </summary>
        [TestMethod]
        public void M_GetPermissionByIdTest()
        {
			SystemPermissionMapper sut = this.CreateSystem();

            SystemPermissionDto systemPermissionDto = new SystemPermissionDto()
            {
                Id = 34,
                UserId = 31,
                Role = PtmRole.Admin
            };

            this.systemPermissionsLoader.Setup(x => x.GetById(systemPermissionDto.Id)).Returns(systemPermissionDto);

            SystemPermissionDto actualSystem = sut.GetById(systemPermissionDto.Id);

            DtoAssertHelpers.AssertDtos(systemPermissionDto, actualSystem);
        }

        /// <summary>
        /// Test Get System Permissions
        /// </summary>
        [TestMethod]
        public void M_GetSystemPermissions()
        {
			SystemPermissionMapper sut = this.CreateSystem();

            SystemPermissionDto systemPermissionDto1 = new SystemPermissionDto()
            {
                Id = 15,
                Role = PtmRole.Admin,
                UserId = 15
            };

            SystemPermissionDto systemPermissionDto2 = new SystemPermissionDto()
            {
                Id = 16,
                Role = PtmRole.Viewer,
                UserId = 16,
                LineOfBusinessIDs = new List<int> { 1, 2 }
            };

            SystemPermissionDto systemPermissionDto3 = new SystemPermissionDto()
            {
                Id = 17,
                Role = PtmRole.ProposalSetupAdmin,
                UserId = 17,
                LineOfBusinessIDs = new List<int> { 2, 3 }
            };

            this.systemPermissionsLoader.Setup(x => x.GetAllIds()).Returns(new List<int>() { systemPermissionDto1.Id, systemPermissionDto2.Id, systemPermissionDto3.Id });
            this.systemPermissionsLoader.Setup(x => x.GetByIds(new List<int>() { systemPermissionDto1.Id, systemPermissionDto2.Id, systemPermissionDto3.Id })).Returns(new List<SystemPermissionDto>() { systemPermissionDto1, systemPermissionDto2, systemPermissionDto3 });

			ICollection<SystemPermissionDto> toTest = sut.GetSystemPermissions();

            Assert.AreEqual(3, toTest.Count);
            DtoAssertHelpers.AssertDtos(systemPermissionDto1, toTest.Where(x => x.Id == systemPermissionDto1.Id).FirstOrDefault());
            DtoAssertHelpers.AssertDtos(systemPermissionDto2, toTest.Where(x => x.Id == systemPermissionDto2.Id).FirstOrDefault());
            DtoAssertHelpers.AssertDtos(systemPermissionDto3, toTest.Where(x => x.Id == systemPermissionDto3.Id).FirstOrDefault());
        }

        /// <summary>
        /// Get by User Id
        /// </summary>
        [TestMethod]
        public void M_GetByUserIdTest()
        {
			SystemPermissionMapper sut = this.CreateSystem();

            int userId = 14;

            SystemPermissionDto systemPermissionDto = new SystemPermissionDto()
            {
                Id = 34,
                UserId = userId,
                Role = PtmRole.Admin
            };

            this.systemPermissionsLoader.Setup(x => x.GetIdsByUserId(userId)).Returns(new List<int>() { systemPermissionDto.Id });
            this.systemPermissionsLoader.Setup(x => x.GetByIds(new List<int>() { systemPermissionDto.Id })).Returns(new List<SystemPermissionDto>() { systemPermissionDto });

			ICollection<SystemPermissionDto> toTest = sut.GetByUserId(userId);

            DtoAssertHelpers.AssertDtos(systemPermissionDto, toTest.FirstOrDefault());

            toTest = sut.GetByUserId(userId);
            DtoAssertHelpers.AssertDtos(systemPermissionDto, toTest.FirstOrDefault());
        }

        /// <summary>
        /// Test Save Permission
        /// </summary>
        [TestMethod]
        public void M_SavePermissionTest()
        {
			SystemPermissionMapper sut = this.CreateSystem();

            SystemPermissionDto systemPermissionBefore = new SystemPermissionDto()
            {
                Id = -1,
                UserId = 17,
                Role = PtmRole.Admin
            };

            SystemPermissionDto systemPermissionAfter = new SystemPermissionDto()
            {
                Id = 342,
                UserId = 14,
                Role = PtmRole.SystemPricer
            };

            this.systemPermissionsLoader.Setup(x => x.Save(systemPermissionBefore)).Returns(systemPermissionAfter.Id);

			int? actual = ((IInternalSystemPermissionMapper)sut).Save(systemPermissionBefore);
            Assert.AreEqual(systemPermissionAfter.Id, actual);
        }

        #region Exception Test

        /// <summary>
        /// Test Save Exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_SavePermissionsTest_Exception()
        {
			SystemPermissionMapper sut = this.CreateSystem();
            ((IInternalSystemPermissionMapper)sut).Save(null);
        }
        #endregion Exception Test
    }
}
