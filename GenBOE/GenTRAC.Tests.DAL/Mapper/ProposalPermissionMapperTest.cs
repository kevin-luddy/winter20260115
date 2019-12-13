// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Proposal Permissions Mapper Test
    /// </summary>
    [TestClass]
    public class ProposalPermissionMapperTest
    {
        #region Setup

        /// <summary>
        /// Proposal Permission Loader
        /// </summary>
        private Mock<IProposalPermissionLoader> proposalPermissionsLoader = null;

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>Permission Mapper</returns>
        private ProposalPermissionMapper CreateSystem()
        {
            this.proposalPermissionsLoader = new Mock<IProposalPermissionLoader>();

            ProposalPermissionMapper system = new ProposalPermissionMapper(
                this.proposalPermissionsLoader.Object);

            return system;
        }

        #endregion Setup

        /// <summary>
        /// Get Proposal Permissions by Proposal Id
        /// </summary>
        [TestMethod]
        public void M_GetProposalPermissionsByProposalIdTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 1;

            var permissionsDto1 = new ProposalPermissionDto() { Id = 1 };
            var permissionsDto2 = new ProposalPermissionDto() { Id = 2 };
            var permissionsDto3 = new ProposalPermissionDto() { Id = 3 };
            var permissionsDto4 = new ProposalPermissionDto() { Id = 4 };
            var permissionsDto5 = new ProposalPermissionDto() { Id = 5 };

            List<int> permissionIds = new List<int>() { 1, 2, 3, 4, 5 };

            this.proposalPermissionsLoader.Setup(x => x.GetIdsByProposalId(proposalId)).Returns(permissionIds);
            this.proposalPermissionsLoader.Setup(x => x.GetByIds(permissionIds)).Returns(new List<ProposalPermissionDto>() { permissionsDto1, permissionsDto2, permissionsDto3, permissionsDto4, permissionsDto5 });

            var result = sut.GetProposalPermissionsByProposalId(proposalId);

            Assert.AreEqual(permissionsDto1, result.ElementAt(0));
            Assert.AreEqual(permissionsDto2, result.ElementAt(1));
            Assert.AreEqual(permissionsDto3, result.ElementAt(2));
            Assert.AreEqual(permissionsDto4, result.ElementAt(3));
            Assert.AreEqual(permissionsDto5, result.ElementAt(4));
        }

        /// <summary>
        /// Get Proposal Permissions by User Id
        /// </summary>
        [TestMethod]
        public void M_GetProposalPermissionsByUserIdTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 1;
            int userId = 2;

            var permissionsDto1 = new ProposalPermissionDto() { Id = 1, UserId = 1 };
            var permissionsDto2 = new ProposalPermissionDto() { Id = 2, UserId = 1 };
            var permissionsDto3 = new ProposalPermissionDto() { Id = 3, UserId = 2 };
            var permissionsDto4 = new ProposalPermissionDto() { Id = 4, UserId = 2 };
            var permissionsDto5 = new ProposalPermissionDto() { Id = 5, UserId = 2 };

            List<int> permissionIds = new List<int>() { 1, 2, 3, 4, 5 };

            this.proposalPermissionsLoader.Setup(x => x.GetIdsByProposalId(proposalId)).Returns(permissionIds);
            this.proposalPermissionsLoader.Setup(x => x.GetByIds(permissionIds)).Returns(new List<ProposalPermissionDto>() { permissionsDto1, permissionsDto2, permissionsDto3, permissionsDto4, permissionsDto5 });

            var result = sut.GetProposalPermissionsByUserId(proposalId, userId);

            Assert.AreEqual(permissionsDto3, result.ElementAt(0));
            Assert.AreEqual(permissionsDto4, result.ElementAt(1));
            Assert.AreEqual(permissionsDto5, result.ElementAt(2));
        }

        /// <summary>
        /// Get by Id
        /// </summary>
        [TestMethod]
        public void M_GetProposalPermissionByIdTest()
        {
            var sut = this.CreateSystem();

            ProposalPermissionDto proposalPermissionsDto = new ProposalPermissionDto()
            {
                Id = 12,
                UserId = 14,
                ProposalID = 1,
                Role = PtmRole.Pricer
            };

            this.proposalPermissionsLoader.Setup(x => x.GetById(proposalPermissionsDto.Id)).Returns(proposalPermissionsDto);

            ProposalPermissionDto actualProposal = sut.GetById(proposalPermissionsDto.Id);

            DtoAssertHelpers.AssertDtos(proposalPermissionsDto, actualProposal);
        }

        /// <summary>
        /// Get by User Id
        /// </summary>
        [TestMethod]
        public void M_GetByUserIdTest()
        {
            var sut = this.CreateSystem();

            int userId = 14;

            ProposalPermissionDto proposalPermissionsDto = new ProposalPermissionDto()
            {
                Id = 12,
                UserId = userId,
                ProposalID = 1,
                Role = PtmRole.Pricer,
                ResourceType = ResourceType.NotSet
            };

            this.proposalPermissionsLoader.Setup(x => x.GetIdsByUserId(userId)).Returns(new List<int>() { proposalPermissionsDto.Id });
            this.proposalPermissionsLoader.Setup(x => x.GetByIds(new List<int>() { proposalPermissionsDto.Id })).Returns(new List<ProposalPermissionDto>() { proposalPermissionsDto });
            var toTest = sut.GetByUserId(userId);

            DtoAssertHelpers.AssertDtos(proposalPermissionsDto, toTest.FirstOrDefault());

            toTest = sut.GetByUserId(userId);
            DtoAssertHelpers.AssertDtos(proposalPermissionsDto, toTest.FirstOrDefault());
        }

        /// <summary>
        /// Test Save Permission
        /// </summary>
        [TestMethod]
        public void M_SaveProposalPermissionTest()
        {
            var sut = this.CreateSystem();

            ProposalPermissionDto permissionsDtoBefore = new ProposalPermissionDto()
            {
                Id = -1,
                UserId = 14,
                ProposalID = 1,
                Role = PtmRole.Pricer
            };

            ProposalPermissionDto permissionsDtoAfter = new ProposalPermissionDto()
            {
                Id = 12,
                UserId = 14,
                ProposalID = 1,
                Role = PtmRole.Pricer
            };

            this.proposalPermissionsLoader.Setup(x => x.Save(permissionsDtoBefore)).Returns(permissionsDtoAfter.Id);
            this.proposalPermissionsLoader.Setup(x => x.GetById(permissionsDtoAfter.Id)).Returns(permissionsDtoAfter);
            
            var actual = ((IInternalProposalPermissionMapper)sut).Save(permissionsDtoBefore);

            Assert.AreEqual(permissionsDtoAfter.Id, actual);
        }

        /// <summary>
        /// Delete Ids by Proposal Id test
        /// </summary>
        [TestMethod]
        public void M_DeleteProposalPermissionIdsByProposalIdTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            sut.DeleteIdsByProposalId(proposalId);

            this.proposalPermissionsLoader.Verify(x => x.DeleteIdsByProposalId(proposalId), Times.Once());
        }

        /// <summary>
        /// Delete test
        /// </summary>
        [TestMethod]
        public void M_DeleteProposalPermissionsTest()
        {
            var sut = this.CreateSystem();

            ICollection<ProposalPermissionDto> permissions = new Collection<ProposalPermissionDto>();
            sut.Delete(permissions);

            this.proposalPermissionsLoader.Verify(x => x.Delete(permissions), Times.Once());
        }

        #region Exception Test

        /// <summary>
        /// Exception test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_SaveProposalPermissionExceptionTest()
        {
            var sut = this.CreateSystem();

            ((IInternalProposalPermissionMapper)sut).Save(null);
        }

        /// <summary>
        /// Exception test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void M_DeleteProposalPermissionsExceptionTest()
        {
            var sut = this.CreateSystem();

            sut.Delete(null);
        }

        #endregion
    }
}
