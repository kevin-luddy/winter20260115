// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the Contracts loader
    /// </summary>
    [TestClass]
    public class ContractsLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Test the GetByProposalIds method. 
        /// </summary>
        [TestMethod]
        public void GetByIdsTest()
        {
            this.TestContractsLoader(false);
        }

        /// <summary>
        /// Test the GetByProposalIds method. 
        /// </summary>
        [TestMethod]
        public void GetByProposalIdsTest()
        {
            this.TestContractsLoader(true);
        }

        /// <summary>
        /// Test the Contracts Loader
        /// </summary>
        /// <param name="useProposalID">If true use GetContractForProposal method, else use GetById method</param>
        private void TestContractsLoader(bool useProposalID)
        {
            // Create new ContractDTO 
            ContractsLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            ProposalDto previouslySubmittedProposal = this.testData.GetProposal(true);

            ContractsDto testContractExpected = this.CreateContractsDto(proposal.Id, previouslySubmittedProposal.Id);

            int? contractId = sut.Save(testContractExpected);
            Assert.IsNotNull(contractId);

            ContractsDto testContractActual = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

            Assert.IsTrue(testContractActual.Id > 0);

            this.CompareContractData(testContractExpected, testContractActual);

            // Update Contract
            testContractActual.Updateable = UpdateType.Upsert;
            testContractActual.CustomerSubmittalDate = DateTime.Now;
            testContractActual.ContractsCorrespondenceLogNumber = "TestLogNumber2";
            testContractActual.FinalNegotiatedValue = 201;
            testContractActual.NegotiationsSubmitted = DateTime.Now;
            testContractActual.EppDelegationAuthority = 3;
            testContractActual.ProgramEppDate = DateTime.Now;
            testContractActual.LobEppDate = DateTime.Now;
            testContractActual.PreSpaceEppDate = DateTime.Now;
            testContractActual.SpaceEppDate = DateTime.Now;
            testContractActual.PreCorporateEppDate = DateTime.Now;
            testContractActual.CorporateEppDate = DateTime.Now;
            testContractActual.EppRosDelegationNotes = "Test EppRosDelegationNotes";
            testContractActual.LmWon = false;
            testContractActual.ModCompletedDate = DateTime.Now;

            contractId = sut.Save(testContractActual);
            Assert.IsNotNull(contractId);
            Assert.AreEqual(contractId, testContractActual.Id);

            ContractsDto testContractActual2 = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

            this.CompareContractData(testContractActual, testContractActual2);

            // This deletes the proposal in order to test deleting the contract
            this.testData.Cleanup();

            ContractsDto testContractActual3 = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

            Assert.IsNull(testContractActual3);
        }

        /// <summary>
        /// Creates a Contracts DTO based on a Propsal ID
        /// </summary>
        /// <param name="proposalId"> The proposal ID</param>
        /// <param name="previouslySubmittedProposalId"> The previously submitted proposal ID</param>
        /// <returns>A contracts dto object</returns>
        private ContractsDto CreateContractsDto(int proposalId, int previouslySubmittedProposalId)
        {
            return new ContractsDto
            {
                Updateable = UpdateType.Upsert,
                ProposalId = proposalId,
                PreviouslySubmittedROM = previouslySubmittedProposalId,
                CustomerSubmittalDate = DateTime.Now,
                ContractsCorrespondenceLogNumber = "Test LogNumber",
                FinalNegotiatedValue = 222L,
                NegotiationsSubmitted = DateTime.Now,
                EppDelegationAuthority = 3,
                ProgramEppDate = DateTime.Now,
                LobEppDate = DateTime.Now,
                PreSpaceEppDate = DateTime.Now,
                SpaceEppDate = DateTime.Now,
                PreCorporateEppDate = DateTime.Now,
                CorporateEppDate = DateTime.Now,
                EppRosDelegationNotes = "Test EppRosDelegationNotes",
                LmWon = false,
                ModCompletedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Asserts that the two Contracts dtos are equal.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        private void CompareContractData(ContractsDto expected, ContractsDto actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ProposalId, actual.ProposalId);
            Assert.AreEqual(expected.PreviouslySubmittedROM, actual.PreviouslySubmittedROM);
            Assert.AreEqual(expected.CustomerSubmittalDate.Value.Date, actual.CustomerSubmittalDate.Value.Date);
            Assert.AreEqual(expected.ContractsCorrespondenceLogNumber, actual.ContractsCorrespondenceLogNumber);
            Assert.AreEqual(expected.FinalNegotiatedValue, actual.FinalNegotiatedValue);
            Assert.AreEqual(expected.NegotiationsSubmitted.Value.Date, actual.NegotiationsSubmitted.Value.Date);
            Assert.AreEqual(expected.EppDelegationAuthority, actual.EppDelegationAuthority);
            Assert.AreEqual(expected.ProgramEppDate.Value.Date, actual.ProgramEppDate.Value.Date);
            Assert.AreEqual(expected.LobEppDate.Value.Date, actual.LobEppDate.Value.Date);
            Assert.AreEqual(expected.PreSpaceEppDate.Value.Date, actual.PreSpaceEppDate.Value.Date);
            Assert.AreEqual(expected.SpaceEppDate.Value.Date, actual.SpaceEppDate.Value.Date);
            Assert.AreEqual(expected.PreCorporateEppDate.Value.Date, actual.PreCorporateEppDate.Value.Date);
            Assert.AreEqual(expected.CorporateEppDate.Value.Date, actual.CorporateEppDate.Value.Date);
            Assert.AreEqual(expected.EppRosDelegationNotes, actual.EppRosDelegationNotes);
            Assert.AreEqual(expected.LmWon, actual.LmWon);
            Assert.AreEqual(expected.ModCompletedDate.Value.Date, actual.ModCompletedDate.Value.Date);
        }

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>A ContractsLoader loader</returns>
        private ContractsLoader CreateSystem()
        {
            return new ContractsLoader();
        }
    }
}
