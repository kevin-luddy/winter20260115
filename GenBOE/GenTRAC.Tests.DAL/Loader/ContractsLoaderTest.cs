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
            Assert.AreEqual(testContractExpected.ContractOffers.Count, testContractActual.ContractOffers.Count);

            for (int i = 0; i < testContractExpected.ContractOffers.Count; i++)
            {
                this.CompareContractOffers(testContractExpected.ContractOffers.ElementAtOrDefault(i), testContractActual.ContractOffers.ElementAtOrDefault(i));
            }

            // Update Contract
            testContractActual.Updateable = UpdateType.Upsert;
            testContractActual.CustomerSubmittalDate = DateTime.Now;
            testContractActual.ContractsCorrespondenceLogNumber = "TestLogNumber2";
            testContractActual.FinalNegotiatedValue = 201;
            testContractActual.NegotiationsSubmitted = DateTime.Now;

            // Update first Contracts Offer (1 of 2)
            testContractActual.ContractOffers.First().Updateable = UpdateType.Upsert;
            testContractActual.ContractOffers.First().CustomerOfferAmount = 101L;
            testContractActual.ContractOffers.First().CustomerOfferDate = DateTime.Now;
            testContractActual.ContractOffers.First().LMCounterOfferDate = DateTime.Now;
            testContractActual.ContractOffers.First().LMCounterOfferCost = 102L;
            testContractActual.ContractOffers.First().LMCounterOfferCOM = 103L;
            testContractActual.ContractOffers.First().LMCounterOfferProfitFee = 104L;

            // Delete second Contracts Offer
            testContractActual.ContractOffers.Last().Updateable = UpdateType.Deleted;

            testContractActual.ContractOffers.Add(new ContractsOffersDto() 
            {
                Updateable = UpdateType.Upsert,
                CustomerOfferAmount = 123L,
                CustomerOfferDate = DateTime.Now,
                LMCounterOfferDate = DateTime.Now,
                LMCounterOfferCost = 456L,
                LMCounterOfferCOM = 789L,
                LMCounterOfferProfitFee = 987L
            });

            contractId = sut.Save(testContractActual);
            Assert.IsNotNull(contractId);
            Assert.AreEqual(contractId, testContractActual.Id);

            ContractsDto testContractActual2 = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

            this.CompareContractData(testContractActual, testContractActual2);
            Assert.AreEqual(testContractExpected.ContractOffers.Count, testContractActual.ContractOffers.Count - 1);

            // Updating the Id of the newest inserted offer for comparison
            testContractActual.ContractOffers.Last().Id = testContractActual2.ContractOffers.Last().Id;

            foreach (ContractsOffersDto offer in testContractActual.ContractOffers)
            {
                if (offer.Updateable == UpdateType.Upsert)
                {
                    Assert.IsTrue(testContractActual2.ContractOffers.Any(x => x.Id == offer.Id));
                    this.CompareContractOffers(offer, testContractActual2.ContractOffers.First(x => x.Id == offer.Id));
                }
                else
                {
                    Assert.IsFalse(testContractActual2.ContractOffers.Any(x => x.Id == offer.Id));
                }
            }

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
                ContractsCorrespondenceLogNumber = "TestLogNumber",
                FinalNegotiatedValue = 222L,
                NegotiationsSubmitted = DateTime.Now,
                ContractOffers = new Collection<ContractsOffersDto>()
                {
                    new ContractsOffersDto
                    {
                        Updateable = UpdateType.Upsert,
                        CustomerOfferAmount = 100L,
                        CustomerOfferDate = DateTime.Now,
                        LMCounterOfferDate = DateTime.Now,
                        LMCounterOfferCost = 200L,
                        LMCounterOfferCOM = 300L,
                        LMCounterOfferProfitFee = 400L
                    },
                    new ContractsOffersDto
                    {
                        Updateable = UpdateType.Upsert,
                        CustomerOfferAmount = 500L,
                        CustomerOfferDate = DateTime.Now,
                        LMCounterOfferDate = DateTime.Now,
                        LMCounterOfferCost = 600L,
                        LMCounterOfferCOM = 700L,
                        LMCounterOfferProfitFee = 800L
                    }
                }
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
        }

        /// <summary>
        /// Compare test for two contract offers
        /// </summary>
        /// <param name="expected">Expected Contracts Offer</param>
        /// <param name="actual">Actual Contracts Offer</param>
        private void CompareContractOffers(ContractsOffersDto expected, ContractsOffersDto actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ContractsDataId, actual.ContractsDataId);
            Assert.AreEqual(expected.CustomerOfferAmount, actual.CustomerOfferAmount);
            Assert.AreEqual(expected.CustomerOfferDate.Value.Date, actual.CustomerOfferDate.Value.Date);
            Assert.AreEqual(expected.LMCounterOfferDate.Value.Date, actual.LMCounterOfferDate.Value.Date);
            Assert.AreEqual(expected.LMCounterOfferCost, actual.LMCounterOfferCost);
            Assert.AreEqual(expected.LMCounterOfferCOM, actual.LMCounterOfferCOM);
            Assert.AreEqual(expected.LMCounterOfferProfitFee, actual.LMCounterOfferProfitFee);
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
