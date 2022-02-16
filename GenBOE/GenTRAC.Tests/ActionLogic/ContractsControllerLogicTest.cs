// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the Contracts Controller Logic
    /// </summary>
    [TestClass]
    public class ContractsControllerLogicTest
    {
        #region Properties

        /// <summary>
        /// Security Access
        /// </summary>
        private Mock<ISecurityAccess> securityAccess = null;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private Mock<IES.Common.IActiveDirectoryUtilities> adUtils = null;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private Mock<IProposalLoader> proposalLoader = null;

        /// <summary>
        /// The user mapper
        /// </summary>
        private Mock<IUserMapper> userMapper = null;

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<IApprovalsLoader> approvalsLoader;

        /// <summary>
        /// proposal checklist loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader = null;

        /// <summary>
        /// checklist mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator = null;

        /// <summary>
        /// propsoal mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator = null;

        /// <summary>
        /// propsoal mediator
        /// </summary>
        private Mock<IContractsLoader> contractsLoader = null;

        #endregion

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>A ContractsControllerLogic object</returns>
        private ContractsControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.adUtils = new Mock<IES.Common.IActiveDirectoryUtilities>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.contractsLoader = new Mock<IContractsLoader>();

            return new ContractsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object,
                this.objectFactory.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object,
                this.proposalMediator.Object, this.contractsLoader.Object);
        }

        /// <summary>
        /// Test the GetDataForProposalContracts method. 
        /// </summary>
        [TestMethod]
        public void GetDataForProposalContractsTest()
        {
            // Create new Contracts Controller Logic object
            ContractsControllerLogic sut = this.CreateSystem();

            int proposalId = 5;

            ContractsOffersDto contractsOffersDto1 = new ContractsOffersDto()
            {
                CustomerOfferAmount = 1111,
                CustomerOfferDate = DateTime.Now,
                LMCounterOfferDate = DateTime.Now,
                LMCounterOfferCost = 2222,
                LMCounterOfferCOM = 3333,
                LMCounterOfferProfitFee = 4444
            };

            ContractsOffersDto contractsOffersDto2 = new ContractsOffersDto()
            {
                CustomerOfferAmount = 5555,
                CustomerOfferDate = DateTime.Now,
                LMCounterOfferDate = DateTime.Now,
                LMCounterOfferCost = 6666,
                LMCounterOfferCOM = 7777,
                LMCounterOfferProfitFee = 8888
            };

            ContractsDto contractDto = new ContractsDto()
            {
                Updateable = UpdateType.Upsert,
                ProposalId = proposalId,
                PreviouslySubmittedROM = 2,
                CustomerSubmittalDate = DateTime.Now,
                ContractsCorrespondenceLogNumber = "Test_Log_Number",
                FinalNegotiatedValue = 3,
                NegotiationsSubmitted = DateTime.Now,
                ContractOffers = new List<ContractsOffersDto> { contractsOffersDto1, contractsOffersDto2 }
            };

            ICollection<SelectListItem> getRomProposalOptions = new Collection<SelectListItem>()
            {
                new SelectListItem()
                {
                    Selected = true,
                    Text = "TestValue",
                    Value = contractDto.PreviouslySubmittedROM.ToString()
                },
                new SelectListItem()
                {
                    Selected = true,
                    Text = "TestValue2",
                    Value = (contractDto.PreviouslySubmittedROM + 1).ToString()
                }
            };

            Tuple<DateTime?, decimal?> getRomDateAndValue = new Tuple<DateTime?, decimal?>(DateTime.Now, 0);
            this.proposalLoader.Setup(x => x.GetRomDateAndValue(contractDto.ProposalId)).Returns(getRomDateAndValue);

            this.proposalLoader.Setup(x => x.GetRomProposalOptions(contractDto.PreviouslySubmittedROM)).Returns(getRomProposalOptions);

            this.contractsLoader.Setup(x => x.GetContractForProposal(proposalId)).Returns(contractDto);

            ContractsModelView contractsModelView = null;
            contractsModelView = sut.GetDataForProposalContracts(proposalId);

            Assert.IsNotNull(contractsModelView);
            Assert.IsTrue(contractsModelView.ProposalId > 0);
            Assert.AreEqual(contractDto.ProposalId, contractsModelView.ProposalId);
            Assert.AreEqual(contractDto.CustomerSubmittalDate, contractsModelView.CustomerSubmittalDt);
            Assert.AreEqual(contractDto.ContractsCorrespondenceLogNumber, contractsModelView.ContractsCorrespondenceLogNumber);
            Assert.AreEqual(contractDto.FinalNegotiatedValue, contractsModelView.FinalNegotiatedValueLong);
            Assert.AreEqual(contractDto.NegotiationsSubmitted, contractsModelView.NegotiationsSubmittedDt);
            Assert.AreEqual(contractDto.PreviouslySubmittedROM.ToString(), contractsModelView.PreviouslySubmittedRoms.ElementAtOrDefault(0).Value);
            Assert.AreEqual(contractDto.PreviouslySubmittedROM, contractsModelView.PreviouslySubmittedROM);

            Assert.AreEqual(contractDto.ContractOffers.Count, contractsModelView.ContractOffers.Count);
            for (int i = 0; i < contractsModelView.ContractOffers.Count; i++)
            {
                this.CompareContractOffers(contractDto.ContractOffers.ElementAtOrDefault(i), contractsModelView.ContractOffers.ElementAtOrDefault(i));
            }
        }

        /// <summary>
        /// Test the GetRomDateAndValue method. 
        /// </summary>
        [TestMethod]
        public void GetRomDateAndValueTest()
        {
            // Create new Contracts Controller Logic object
            ContractsControllerLogic sut = this.CreateSystem();

            int proposalId = 5;

            Tuple<DateTime?, decimal?> testDateAndValue = new Tuple<DateTime?, decimal?>(DateTime.Now, 0);
            this.proposalLoader.Setup(x => x.GetRomDateAndValue(proposalId)).Returns(testDateAndValue);

            Tuple<DateTime?, decimal?> getRomDateAndValue = null;

            getRomDateAndValue = sut.GetRomDateAndValue(proposalId);

            Assert.IsNotNull(getRomDateAndValue);
            Assert.AreEqual(getRomDateAndValue, testDateAndValue);
        }

        /// <summary>
        /// Test throw execption for the GetRomDateAndValue method. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Invalid Proposal ID, is less than 0")]
        public void GetRomDateAndValueTestNull()
        {
            // Create new Contracts Controller Logic object
            ContractsControllerLogic sut = this.CreateSystem();

            Tuple<DateTime?, decimal?> getRomDateAndValue = sut.GetRomDateAndValue(-1);
        }

        /// <summary>
        /// Test the GetByProposalIds method. 
        /// </summary>
        [TestMethod]
        public void SaveContractTest()
        {
            // Create new Contracts Controller Logic object
            ContractsControllerLogic sut = this.CreateSystem();
            
            // TODO: SaveContract unit test. 
            
            // Assert that the id return from the save is the one we expect.
            // Verify method - that it ran, but if we get an Id, may not be needed) 
        }

        /// <summary>
        /// Test the ValidateContract method. 
        /// </summary>
        [TestMethod]
        public void ValidateContractTest()
        {
            // TODO: After Validation methods are implemented
        }

        /// <summary>
        /// Compare test for two contract offers
        /// </summary>
        /// <param name="expect">Expected Contracts Offer</param>
        /// <param name="actual">Actual Contracts Offer</param>
        private void CompareContractOffers(ContractsOffersDto expect, ContractsOfferModelView actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual((int)expect.CustomerOfferAmount, actual.CustomerOfferAmountInt);
            Assert.AreEqual(expect.CustomerOfferDate.Value.Date, actual.CustomerOfferDt.Value.Date);
            Assert.AreEqual(expect.LMCounterOfferDate.Value.Date, actual.LmCounterOfferDt.Value.Date);
            Assert.AreEqual((int)expect.LMCounterOfferCost, actual.LMCounterOfferCostInt);
            Assert.AreEqual((int)expect.LMCounterOfferCOM, actual.LMCounterOfferCOMInt);
            Assert.AreEqual((int)expect.LMCounterOfferProfitFee, actual.LMCounterOfferProfitFeeInt);
        }
    }
}
