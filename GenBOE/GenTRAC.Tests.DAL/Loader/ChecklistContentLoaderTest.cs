// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the ChecklistContentLoaderTest
    /// </summary>
    [TestClass]
    public class ChecklistContentLoaderTest
    {
        /// <summary>
        /// Test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// PPR Proposal loader
        /// </summary>
        /// <returns>loader</returns>
        private IChecklistContentLoader CreatePPRSystem()
        {
            return new PPRChecklistContentLoader();
        }

        /// <summary>
        /// PAR Proposal loader
        /// </summary>
        /// <returns>loader</returns>
        private IChecklistContentLoader CreatePARSystem()
        {
            return new PARChecklistContentLoader();
        }

        /// <summary>
        /// Get PPR checklist ID by proposal ID
        /// </summary>
        [TestMethod]
        public void L_GetPPRChecklistIdByProposalId()
        {
            IChecklistContentLoader sut = this.CreatePPRSystem();

            this.GetChecklistIdByProposalId(sut);
        }

        /// <summary>
        /// Get PAR checklist ID by proposal ID
        /// </summary>
        [TestMethod]
        public void L_GetPARChecklistIdByProposalId()
        {
            IChecklistContentLoader sut = this.CreatePARSystem();

            this.GetChecklistIdByProposalId(sut);
        }

        /// <summary>
        /// Get checklist ID by proposal ID
        /// </summary>
        /// <param name="loader">Checklist content loader</param>
        private void GetChecklistIdByProposalId(IChecklistContentLoader loader)
        {
            ProposalDto testProposal = this.testData.GetProposal(inCreateNew: true);

            int? checklistId = loader.GetChecklistIdByProposalId(testProposal.Id);

            Assert.IsTrue(checklistId.HasValue);
        }

        /// <summary>
        /// Get PPR checklist by proposal ID
        /// </summary>
        [TestMethod]
        public void L_GetPPRChecklistByProposalId()
        {
            IChecklistContentLoader sut = this.CreatePPRSystem();

            this.GetChecklistByProposalId(sut);
        }

        /// <summary>
        /// Get PAR checklist by proposal ID
        /// </summary>
        [TestMethod]
        public void L_GetPARChecklistByProposalId()
        {
            IChecklistContentLoader sut = this.CreatePARSystem();

            this.GetChecklistByProposalId(sut);
        }

        /// <summary>
        /// Get checklist by proposal ID
        /// </summary>
        /// <param name="loader">Checklist content loader</param>
        private void GetChecklistByProposalId(IChecklistContentLoader loader)
        {
            ProposalDto testProposal = this.testData.GetProposal(inCreateNew: true);

            ChecklistContentDto content = loader.GetChecklistByProposalId(testProposal.Id);

            // no way to verify version or content, so just make sure they are initialized properly
            Assert.IsTrue(content.Version > 0);
            Assert.IsTrue(content.Content.Any());
        }
    }
}
