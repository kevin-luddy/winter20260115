// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Mediator
{
    using System;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// checklist mediator test
    /// </summary>
    [TestClass]
    public class ChecklistMediatorTest
    {
        /// <summary>
        /// proposal checklist loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader = null;

        /// <summary>
        /// Create the system under test
        /// </summary>
        /// <returns>Checklist Mediator</returns>
        private ChecklistMediator CreateSystem()
        {
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();

            return new ChecklistMediator(this.proposalChecklistLoader.Object);
        }

        /// <summary>
        /// Test saving a checklist
        /// </summary>
        [TestMethod]
        public void B_SaveChecklistTest()
        {
            ChecklistMediator sut = this.CreateSystem();

            ProposalChecklistDto toSave = new ProposalChecklistDto()
            {
                Id = 24
            };

            sut.SaveChecklistProposal(toSave);

            this.proposalChecklistLoader.Verify(x => x.Save(toSave), Times.Once());
        }

        /// <summary>
        /// Test unlocking a checklist
        /// </summary>
        [TestMethod]
        public void B_UnlockChecklistTest()
        {
            ChecklistMediator sut = this.CreateSystem();

            int proposalId = 15;
            DateTime updateDate = new DateTime(2014, 1, 1);
            UnlockChecklistOption unlockOption = UnlockChecklistOption.UnlockBoth;

            sut.UnlockChecklist(proposalId, updateDate, unlockOption);

            this.proposalChecklistLoader.Verify(x => x.UnlockChecklist(proposalId, updateDate, unlockOption), Times.Once());
        }

        #region Exception Test

        /// <summary>
        /// Save proposal checklist exception test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void B_SaveChecklistTest_Exception()
        {
            ChecklistMediator sut = this.CreateSystem();
            sut.SaveChecklistProposal(null);
        }

        #endregion Exception Test
    }
}
