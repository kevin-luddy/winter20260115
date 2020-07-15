// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    using IES.Common.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// System Permission Loader Tests
    /// </summary>
    [TestClass]
    public class SystemPermissionLoaderTest
    {
        /// <summary>
        /// Test Data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>System Permission Loader</returns>
        private SystemPermissionLoader CreateSystem()
        {
            return new SystemPermissionLoader();
        }

        /// <summary>
        /// Get All System Permissions Test
        /// </summary>
        [TestMethod]
        public void L_GetAllTest()
        {
            var sut = this.CreateSystem();

            UserDTO currentUser = this.testData.GetUser(true);

            SystemPermissionDto systemAdmin = new SystemPermissionDto()
            {
                Id = -1,
                UserId = currentUser.Id,
                Role = PtmRole.Admin,
                Updateable = UpdateType.Upsert
            };

            SystemPermissionDto systemAdminPermission = this.testData.GetPermission(true, systemAdmin);

            ICollection<int> allIds = sut.GetAllIds();
            ICollection<SystemPermissionDto> allSystemPermissions = sut.GetAllSystemPermissions();

            SystemPermissionDto toTestSystemAdmin = (from x in allSystemPermissions
                                               where x.Id == systemAdminPermission.Id
                                               select x).FirstOrDefault();

            DtoAssertHelpers.AssertDtos(toTestSystemAdmin, systemAdmin);

            Assert.IsTrue(allIds.Contains(systemAdminPermission.Id));
            Assert.AreEqual(allSystemPermissions.Count, allIds.Count);
        }

        /// <summary>
        /// Test GetIdsByUserId
        /// </summary>
        [TestMethod]
        public void L_GetIdsByUserIdTest()
        {
            var sut = this.CreateSystem();

            UserDTO user = this.testData.GetUser(true);

            // create permission
            var permission = this.testData.GetPermission(true, new SystemPermissionDto { Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.Viewer, LineOfBusinessIDs = new List<int> { 10, 11} });

            var result = sut.GetIdsByUserId(user.Id);

            Assert.IsTrue(result.Contains(permission.Id));
        }

        /// <summary>
        /// Test Save and GetById for Admin Role
        /// </summary>
        [TestMethod]
        public void L_SavePermissionsAndGetPermissionsByIdAdmin()
        {
            var sut = this.CreateSystem();

            UserDTO user = this.testData.GetUser(true);

            SystemPermissionDto newPermissions = new SystemPermissionDto()
            {
                UserId = user.Id,
                Role = PtmRole.Admin,
                Updateable = UpdateType.Upsert
            };

            SystemPermissionDto toTest;

            using (TransactionScope scope = new TransactionScope())
            {
                toTest = sut.GetById((int)sut.Save(newPermissions));
                scope.Complete();
            }

            DtoAssertHelpers.AssertDtos(toTest, newPermissions);

            // test delete
            toTest.Updateable = UpdateType.Deleted;
            int? deletedPermissionsID;

            using (TransactionScope scope = new TransactionScope())
            {
                deletedPermissionsID = sut.Save(toTest);
                scope.Complete();
            }

            Assert.AreEqual(toTest.Id, deletedPermissionsID);
        }

        /// <summary>
        /// Test Save and GetById for Proposal Setup Admin role
        /// </summary>
        [TestMethod]
        public void L_SavePermissionsAndGetPermissionsByIdProposalSetupAdmin()
        {
            var sut = this.CreateSystem();

            UserDTO user = this.testData.GetUser(true);

            SystemPermissionDto newPermissions = new SystemPermissionDto()
            {
                UserId = user.Id,
                Role = PtmRole.ProposalSetupAdmin,
                LineOfBusinessIDs = new List<int> { 10, 11},
                Updateable = UpdateType.Upsert
            };

            SystemPermissionDto toTest;

            using (TransactionScope scope = new TransactionScope())
            {
                toTest = sut.GetById((int)sut.Save(newPermissions));
                scope.Complete();
            }

            DtoAssertHelpers.AssertDtos(toTest, newPermissions);

            // test delete
            toTest.Updateable = UpdateType.Deleted;
            int? deletedPermissionsID;

            using (TransactionScope scope = new TransactionScope())
            {
                deletedPermissionsID = sut.Save(toTest);
                scope.Complete();
            }

            Assert.AreEqual(toTest.Id, deletedPermissionsID);
        }

        #region Exception Tests

        /// <summary>
        /// Test upsert exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void L_UpsertNotImplementedTest()
        {
            var sut = this.CreateSystem();

            SystemPermissionDto newPermissions = new SystemPermissionDto()
            {
                Id = 51,
                UserId = 21,
                Role = PtmRole.Admin,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(newPermissions);
                scope.Complete();
            }
        }

        /// <summary>
        ///  Use this loader to save a capture role
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GeneralAppException))]
        public void L_UpsertWrongRoleTest()
        {
            var sut = this.CreateSystem();

            SystemPermissionDto newPermissions = new SystemPermissionDto()
            {
                UserId = 21,
                Role = PtmRole.Pricer,
                Updateable = UpdateType.Upsert
            };

            sut.Save(newPermissions);
        }

        /// <summary>
        /// Use this loader to delete a capture role
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GeneralAppException))]
        public void L_DeleteWrongRoleTest()
        {
            var sut = this.CreateSystem();

            SystemPermissionDto newPermissions = new SystemPermissionDto()
            {
                Id = 51,
                UserId = 21,
                Role = PtmRole.SupplyChainPOCMatl,
                Updateable = UpdateType.Deleted
            };

            sut.Save(newPermissions);
        }

        #endregion Exception Tests
    }
}
