// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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
    /// proposal mediator Test
    /// </summary>
    [TestClass]
    public class ProposalMediatorTest
    {
        /// <summary>
        /// Capture Mapper
        /// </summary>
        private Mock<IProposalLoader> proposalLoader = null;

        /// <summary>
        /// Test saving a proposal
        /// </summary>
        [TestMethod]
        public void B_SaveProposalTest()
        {
            ProposalMediator sut = this.CreateSystem();

            ProposalDto toSave = new ProposalDto()
            {
                Id = 24
            };

            sut.SaveProposal(toSave);
            this.proposalLoader.Verify(x => x.Save(toSave), Times.Once());
        }

        #region Exception Test

        /// <summary>
        /// Save capture exception test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SaveProposalTest_Exception()
        {
            ProposalMediator sut = this.CreateSystem();
            sut.SaveProposal(null);
        }

        #endregion Exception Test

        /// <summary>
        /// Create the system under test
        /// </summary>
        /// <returns>Propoal Mediator</returns>
        private ProposalMediator CreateSystem()
        {
            this.proposalLoader = new Mock<IProposalLoader>();

            return new ProposalMediator(this.proposalLoader.Object);
        }
    }
}
