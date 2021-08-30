// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Tests.DAL;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Proposal Permission Mediator Test
    /// </summary>
    [TestClass]
    public class ProposalPermissionMediatorTest
    {
        /// <summary>
        /// Proposal Permission mapper
        /// </summary>
        private Mock<IInternalProposalPermissionMapper> proposalPermissionsMapper = null;

        /// <summary>
        /// Create mediator
        /// </summary>
        /// <returns>Mediator</returns>
        private ProposalPermissionMediator CreateSystem()
        {
            this.proposalPermissionsMapper = new Mock<IInternalProposalPermissionMapper>();

            return new ProposalPermissionMediator(this.proposalPermissionsMapper.Object);
        }

        /// <summary>
        /// Save
        /// </summary>
        [TestMethod]
        public void B_SaveProposalPermissionsTest()
        {
            ProposalPermissionMediator sut = this.CreateSystem();

            ProposalPermissionDto toSave = new ProposalPermissionDto() { Id = 24 };

            this.proposalPermissionsMapper.Setup(x => x.Save(toSave)).Returns(toSave.Id);
            this.proposalPermissionsMapper.Setup(x => x.GetById(toSave.Id)).Returns(toSave);

            ProposalPermissionDto groupPermission = new ProposalPermissionDto() { Id = 45, UserId = 15, Role = PtmRole.CostVolumeLead };

            this.proposalPermissionsMapper.Setup(x => x.Save(groupPermission)).Returns(groupPermission.Id);
            this.proposalPermissionsMapper.Setup(x => x.GetById(groupPermission.Id)).Returns(groupPermission);

            var result = sut.SaveProposalPermissionDto(toSave);
            var groupResult = sut.SaveProposalPermissionDto(groupPermission);

            Assert.AreEqual(toSave, result);
            Assert.AreEqual(groupPermission, groupResult);
        }

        /// <summary>
        /// Save collection
        /// </summary>
        [TestMethod]
        public void B_SaveProposalPermissionsCollectionTest()
        {
            ProposalPermissionMediator sut = this.CreateSystem();

            Collection<ProposalPermissionDto> allPermissionsToSave = new Collection<ProposalPermissionDto>();

            ProposalPermissionDto toSave = new ProposalPermissionDto()
            {
                Id = 24,
                UserId = 15,
                Role = PtmRole.CaptureManager,
                Updateable = UpdateType.Upsert,
                UpdateDate = DateTime.Now
            };

            allPermissionsToSave.Add(toSave);

            this.proposalPermissionsMapper.Setup(x => x.Save(toSave)).Returns(toSave.Id);
            this.proposalPermissionsMapper.Setup(x => x.GetById(toSave.Id)).Returns(toSave);

            var result = sut.SaveProposalPermissionDtos(allPermissionsToSave);

            DtoAssertHelpers.AssertDtos(result.First(), toSave);
        }

        /// <summary>
        /// Delete Ids by Proposal Id test
        /// </summary>
        [TestMethod]
        public void B_DeleteProposalPermissionIdsByProposalIdTest()
        {
            ProposalPermissionMediator sut = this.CreateSystem();
            int proposalId = 15;
            sut.DeleteIdsByProposalId(proposalId);

            this.proposalPermissionsMapper.Verify(x => x.DeleteIdsByProposalId(proposalId), Times.Once());
        }

        /// <summary>
        /// Delete test
        /// </summary>
        [TestMethod]
        public void B_DeleteProposalPermissionsTest()
        {
            ProposalPermissionMediator sut = this.CreateSystem();

            ICollection<ProposalPermissionDto> permissions = new Collection<ProposalPermissionDto>();
            sut.Delete(permissions);

            this.proposalPermissionsMapper.Verify(x => x.Delete(permissions), Times.Once());
        }

        #region Exception Test

        /// <summary>
        /// Save with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SaveProposalPermissionsTest_Exception()
        {
            ProposalPermissionMediator sut = this.CreateSystem();
            sut.SaveProposalPermissionDto(null);
        }

        /// <summary>
        /// Save collection with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SaveProposalPermissionsCollectionTest_Exception()
        {
            ProposalPermissionMediator sut = this.CreateSystem();
            sut.SaveProposalPermissionDtos(null);
        }

        #endregion Exception Test
    }
}
