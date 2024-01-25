// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Proposal Permission Loader Test
    /// </summary>
    [TestClass]
    public class ProposalPermissionLoaderTest
    {
        /// <summary>
        /// Test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Create an instance of the loader
        /// </summary>
        /// <returns>Loader instance</returns>
        private ProposalPermissionLoader CreateSystem()
        {
            return new ProposalPermissionLoader();
        }

        /// <summary>
        /// Get all proposal permissions
        /// </summary>
        [TestMethod]
        public void L_GetAllProposalPermissions()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto currentProposal = this.testData.GetProposal();
            UserDTO currentUser = this.testData.GetUser(true);

            ProposalPermissionDto pricer = new ProposalPermissionDto()
            {
                Id = -1,
                UserId = currentUser.Id,
                Role = PtmRole.Pricer,
                ResourceType = ResourceType.NotSet,
                Updateable = UpdateType.Upsert,
                ProposalID = currentProposal.Id
            };

            ProposalPermissionDto testPricerPermissions = this.testData.GetProposalPermission(true, pricer);

            ICollection<ProposalPermissionDto> proposalPermissions = sut.GetAllProposalPermissions();

            ProposalPermissionDto toTestPricer = (from x in proposalPermissions
                                                  where x.Id == testPricerPermissions.Id
                                                  select x).FirstOrDefault();

            DtoAssertHelpers.AssertDtos(toTestPricer, pricer);
        }

        /// <summary>
        /// Save permissions and get permissions by Id
        /// </summary>
        [TestMethod]
        public void L_SavePermissionsAndGetPermissionsById()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(true);

            ProposalPermissionDto newPermissions = new ProposalPermissionDto()
            {
                ProposalID = proposal.Id,
                UserId = user.Id,
                Role = PtmRole.Pricer,
                ResourceType = ResourceType.NotSet,
                Updateable = UpdateType.Upsert
            };

            ProposalPermissionDto toTest;

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
        /// Test GetIdsByUserId
        /// </summary>
        [TestMethod]
        public void L_GetIdsByUserIdTest()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            UserDTO user = this.testData.GetUser(true);

            // create 2 permissions
            this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.Pricer, ResourceType = ResourceType.NotSet });
            this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.CaptureManager, ResourceType = ResourceType.NotSet });

			ICollection<int> result = sut.GetIdsByUserId(user.Id);

            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Test GetIdsByProposalId
        /// </summary>
        [TestMethod]
        public void L_GetIdsByProposalIdTest()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            UserDTO user = this.testData.GetUser(true);

            // create 2 permissions
            this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.Pricer, ResourceType = ResourceType.NotSet });
            this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.AdditionalPricingResource1, ResourceType = ResourceType.Strategist });

			ICollection<int> result = sut.GetIdsByProposalId(proposal.Id);

            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Test GetIdsForHomeProposalGrid
        /// </summary>
        [TestMethod]
        public void L_GetIdsForHomeProposalGrid()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal1 = this.testData.GetProposal(true);
            ProposalDto proposal2 = this.testData.GetProposal(true);
            UserDTO user = this.testData.GetUser(true);
            ICollection<int> ids = new Collection<int>() { proposal1.Id, proposal2.Id };

            // add 5 proposal roles to proposal1
            ProposalPermissionDto pricer1 = 
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal1.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.Pricer });
            ProposalPermissionDto captureManager1 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal1.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.CaptureManager });
            ProposalPermissionDto peerReviewer1 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal1.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.PeerReviewer });
            ProposalPermissionDto backupPricer1 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal1.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.BackupPricer });
            ProposalPermissionDto costVolumeLead1 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal1.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.CostVolumeLead });

            // add 3 proposal roles to proposal2
            ProposalPermissionDto pricer2 = 
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal2.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.Pricer });
            ProposalPermissionDto captureManager2 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal2.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.CaptureManager });
            ProposalPermissionDto backupPricer2 =
                this.testData.GetProposalPermission(true, new ProposalPermissionDto { ProposalID = proposal2.Id, Id = -1, Updateable = UpdateType.Upsert, UserId = user.Id, Role = PtmRole.BackupPricer });

			ICollection<int> result = sut.GetIdsForHomeProposalGrid(ids);

            // verify only pricers, capture managers, and peers returned
            Assert.AreEqual(6, result.Count);
            Assert.IsTrue(result.Contains(pricer1.Id));
            Assert.IsTrue(result.Contains(captureManager1.Id));
            Assert.IsTrue(result.Contains(peerReviewer1.Id));
            Assert.IsFalse(result.Contains(backupPricer1.Id));
            Assert.IsTrue(result.Contains(costVolumeLead1.Id));
            Assert.IsTrue(result.Contains(pricer2.Id));
            Assert.IsTrue(result.Contains(captureManager2.Id));
            Assert.IsFalse(result.Contains(backupPricer2.Id));
        }

        /// <summary>
        /// Test GetIdsByProposalId
        /// </summary>
        [TestMethod]
        public void L_DeleteIdsByProposalIdTest()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            UserDTO user = this.testData.GetUser(true);

            ProposalPermissionDto permission1 = new ProposalPermissionDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                Role = PtmRole.Pricer,
                UserId = user.Id,
                Updateable = UpdateType.Upsert
            };

            ProposalPermissionDto permission2 = new ProposalPermissionDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                Role = PtmRole.CostVolumeLead,
                UserId = user.Id,
                Updateable = UpdateType.Upsert
            };

            int? permissionId1;
            int? permissionId2;
            using (TransactionScope scope = new TransactionScope())
            {
                permissionId1 = sut.Save(permission1);
                permissionId2 = sut.Save(permission2);
                scope.Complete();
            }

            ICollection<int> permissions = sut.GetIdsByProposalId(proposal.Id);
            Assert.AreEqual(2, permissions.Count);
            Assert.IsTrue(permissions.Where(x => x == permissionId1).Any());
            Assert.IsTrue(permissions.Where(x => x == permissionId2).Any());

            using (TransactionScope scope = new TransactionScope())
            {
                sut.DeleteIdsByProposalId(proposal.Id);
                scope.Complete();
            }

            permissions = sut.GetIdsByProposalId(proposal.Id);
            Assert.AreEqual(0, permissions.Count);
        }

        /// <summary>
        /// Test Delete Proposal Permissions
        /// </summary>
        [TestMethod]
        public void L_DeleteProposalPermissionsTest()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            UserDTO user = this.testData.GetUser(true);

            ProposalPermissionDto permission1 = new ProposalPermissionDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                Role = PtmRole.Pricer,
                UserId = user.Id,
                Updateable = UpdateType.Upsert
            };

            ProposalPermissionDto permission2 = new ProposalPermissionDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                Role = PtmRole.AdditionalPricingResource1,
                ResourceType = ResourceType.Pricer,
                UserId = user.Id,
                Updateable = UpdateType.Upsert
            };

            int? permissionId1;
            int? permissionId2;
            using (TransactionScope scope = new TransactionScope())
            {
                permissionId1 = sut.Save(permission1);
                permissionId2 = sut.Save(permission2);
                scope.Complete();
            }

            ICollection<int> permissions = sut.GetIdsByProposalId(proposal.Id);
            Assert.AreEqual(2, permissions.Count);
            Assert.IsTrue(permissions.Where(x => x == permissionId1).Any());
            Assert.IsTrue(permissions.Where(x => x == permissionId2).Any());

            permission1 = sut.GetById(permissionId1.Value);
            permission2 = sut.GetById(permissionId2.Value);
            sut.Delete(new Collection<ProposalPermissionDto>() { permission1, permission2 });
            permission1 = sut.GetById(permissionId1.Value);
            Assert.IsNull(permission1);
            permission2 = sut.GetById(permissionId2.Value);
            Assert.IsNull(permission2);
        }

        #region Exception Test

        /// <summary>
        /// Exception test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_DeleteProposalPermissionsExceptionTest()
        {
			ProposalPermissionLoader sut = this.CreateSystem();

            sut.Delete(null);
        }

        #endregion
    }
}
