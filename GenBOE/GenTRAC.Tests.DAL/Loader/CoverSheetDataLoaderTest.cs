// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.DataBridge.DTO.Contracts;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the Contracts loader
    /// </summary>
    [TestClass]
    public class CoverSheetDataLoaderTest
    {
        /// <summary>
        /// Gets or sets the Contracts Loader.
        /// </summary>
        private Mock<IContractsLoader> ContractsLoader { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Loader.
        /// </summary>
        private Mock<IProposalLoader> ProposalLoader { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Checklist Loader.
        /// </summary>
        private Mock<IProposalChecklistLoader> ProposalChecklistLoader { get; set; }

        /// <summary>
        /// Gets or sets the Cage Codes Loader.
        /// </summary>
        private Mock<ICageCodesLoader> CageCodesLoader { get; set; }

        /// <summary>
        /// Object Factory
        /// </summary>
        protected Mock<IFullObjectFactory> ObjectFactory { get; set; }

        /// <summary>
        /// The user mapper
        /// </summary>
        protected Mock<IUserMapper> UserMapper { get; set; }

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            // De-null properties with Mock data
            ProposalLoader = new Mock<IProposalLoader>();
            ProposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            ContractsLoader = new Mock<IContractsLoader>();
            CageCodesLoader = new Mock<ICageCodesLoader>();
            ObjectFactory = new Mock<IFullObjectFactory>();
            UserMapper = new Mock<IUserMapper>();

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);
        }

        /// <summary>
        /// Cover Sheet loader
        /// </summary>
        /// <returns>loader</returns>
        private CoverSheetDataLoader CreateSystem()
        {
            return new CoverSheetDataLoader(ProposalLoader.Object, ProposalChecklistLoader.Object, ContractsLoader.Object,
            CageCodesLoader.Object, ObjectFactory.Object, UserMapper.Object);
        }

        /// <summary>
        /// Get Cover Sheet Data by Id
        /// </summary>
        [TestMethod]
        public void GetCoverSheetDataByIdTest()
        {
            int proposalId = 1;
            DateTime dateToTest = new DateTime(2010, 8, 18, 16, 32, 0);

            // Prepare a fake proposal Dto
            ProposalDto fakeProposalDto = new ProposalDto()
            {
                Id = proposalId,
                ProposalTitle = "Test Title",
                TrackingNumber = "123456",
                OTISOpportunityID = "9999",
                ProposalStatus = ProposalStatus.InProgress,
                ContractActionType = ContractActionType.PriceRevisionRedetermination,
                ContractTypeGroup = 1,
                ContractTypeIds = new List<int>() { 1, 2, 3, 4 },
                CostElementTypeIds = new List<int>() { 1, 2, 3, 4 },
                UpdateDateAssigned = false,
                ForecastedTrackingNumber = "ABC",
                IsForecastProposal = false,
                HasWriteAccessToLinkedDocument = false,
                CoverSheetApproverSignedDate = dateToTest,
                IsCCPDRequired = true
            };

            // Prepare fake proposal checklist dto
            ProposalChecklistDto fakeProposalChecklistDto = new ProposalChecklistDto()
            {
                Id = proposalId,
                ProposalID = proposalId,
                CostThroughCom = 9998,
                Profit = 9999,
                SubmittedValue = 10000
            };

            // Prepare fake contracts dto
            ContractsDto fakeContractsDto = new ContractsDto()
            {
                Id = proposalId,
                ProposalId = proposalId,
                CustomerSubmittalDate = dateToTest,
                CageCode = "ABCXYZ"
            };

            // Prepare pricer user
            UserDTO fakeUserDtoPricer = new UserDTO()
            {
                Id = 1,
                Ntid = "fakeuser1",
                DisplayName = "FAKE USER 1",
                EmailAddress = "test1",
                PhoneNumber = "test-test-test",
                FirstName = "Fake1",
                LastName = "User1"
            };

            // Prepare fake cover sheet approver user
            UserDTO fakeUserDtoCoverSheetApprover = new UserDTO()
            {
                Id = 2,
                Ntid = "fakeuser2",
                DisplayName = "FAKE USER 2",
                EmailAddress = "test2",
                PhoneNumber = "test-test-test",
                FirstName = "Fake2",
                LastName = "User2"
            };

            // Prepare fake pricing verification user
            UserDTO fakeUserDtoPricingVerification = new UserDTO()
            {
                Id = 3,
                Ntid = "fakeuser3",
                DisplayName = "FAKE USER 3",
                EmailAddress = "test3",
                PhoneNumber = "test-test-test",
                FirstName = "Fake3",
                LastName = "User3"
            };

            // Prepare fake peer reviewer user
            UserDTO fakeUserDtoPeerReviewer = new UserDTO()
            {
                Id = 4,
                Ntid = "fakeuser4",
                DisplayName = "FAKE USER 4",
                EmailAddress = "test4",
                PhoneNumber = "test-test-test",
                FirstName = "Fake4",
                LastName = "User4"
            };

            // Prepare fake LOBEstLead user
            UserDTO fakeUserDtoLOBEstLead = new UserDTO()
            {
                Id = 5,
                Ntid = "fakeuser5",
                DisplayName = "FAKE USER 5",
                EmailAddress = "test5",
                PhoneNumber = "test-test-test",
                FirstName = "Fake5",
                LastName = "User5"
            };

            // Prepare fake contracts poc user
            UserDTO fakeUserDtoContractsPOC = new UserDTO()
            {
                Id = 6,
                Ntid = "fakeuser6",
                DisplayName = "FAKE USER 6",
                EmailAddress = "test6",
                PhoneNumber = "test-test-test",
                FirstName = "Fake6",
                LastName = "User6"
            };

            // Prepare fake cage code data
            CageCodeDTO fakeCageCodeDto = new CageCodeDTO()
            {
                CageCode = "ABCXYZ",
                Address1 = "Add1",
                Address2 = "Add2",
                City = "Test City",
                State = "Test State",
                Zip = "Test Zip"
            };

            // Setup complex Full Proposal object
            FullProposal fullProposal = new FullProposal(fakeProposalDto);
            fullProposal = GetFullProposalForMocks(fakeProposalDto, ProposalStatus.NoBid);

            // Setup expected data
            ProposalLoader.Setup(x => x.GetById(proposalId)).Returns(fakeProposalDto);
            ProposalChecklistLoader.Setup(x => x.GetById(proposalId)).Returns(fakeProposalChecklistDto);
            ContractsLoader.Setup(x => x.GetById(proposalId)).Returns(fakeContractsDto);
            CageCodesLoader.Setup(x => x.GetDataByCageCode(fakeContractsDto.CageCode)).Returns(fakeCageCodeDto);
            ObjectFactory.Setup(x => x.CreateFullProposal(fakeProposalDto)).Returns(fullProposal);
            UserMapper.Setup(x => x.GetById(1)).Returns(fakeUserDtoPricer);
            UserMapper.Setup(x => x.GetById(2)).Returns(fakeUserDtoCoverSheetApprover);
            UserMapper.Setup(x => x.GetById(3)).Returns(fakeUserDtoPricingVerification);
            UserMapper.Setup(x => x.GetById(4)).Returns(fakeUserDtoPeerReviewer);
            UserMapper.Setup(x => x.GetById(5)).Returns(fakeUserDtoLOBEstLead);
            UserMapper.Setup(x => x.GetById(6)).Returns(fakeUserDtoContractsPOC);
            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(GetPermissionsForMocks());

            CoverSheetDataLoader coverSheetLoader = CreateSystem();
            CoverSheetDataDto resultingCoverSheetDto = coverSheetLoader.GetCoverSheetDataById(1);

            // Setup offeror address with the data we expected
            resultingCoverSheetDto.OfferorAddress = new List<string>()
            {
                fakeCageCodeDto.Address1,
                fakeCageCodeDto.Address2,
                fakeCageCodeDto.City,
                fakeCageCodeDto.State,
                fakeCageCodeDto.Zip
            };

            CoverSheetDataDto expectedCoverSheetDto = new CoverSheetDataDto()
            {
                IsCCPDRequired = true,
                CostThroughCom = 9998,
                ProfitFee = 9999,
                LMSpaceTotalPrice = 10000,
                ContractActionType = ContractActionType.PriceRevisionRedetermination,
                ContractTypeGroup = 1,
                CoverSheetApproverNtid = "fakeuser2",
                CoverSheetApproverSignedDate = dateToTest,
                CustomerSubmittalDate = dateToTest,
                CageCode = "ABCXYZ",
                OfferorAddress = new List<string>()
                {
                    fakeCageCodeDto.Address1,
                    fakeCageCodeDto.Address2,
                    fakeCageCodeDto.City,
                    fakeCageCodeDto.State,
                    fakeCageCodeDto.Zip
                },
                ContractsLead = "fakeuser6"
            };

            Assert.IsNotNull(resultingCoverSheetDto);
            Assert.AreEqual(resultingCoverSheetDto.IsCCPDRequired, expectedCoverSheetDto.IsCCPDRequired);
            Assert.AreEqual(resultingCoverSheetDto.CostThroughCom, expectedCoverSheetDto.CostThroughCom);
            Assert.AreEqual(resultingCoverSheetDto.ProfitFee, expectedCoverSheetDto.ProfitFee);
            Assert.AreEqual(resultingCoverSheetDto.LMSpaceTotalPrice, expectedCoverSheetDto.LMSpaceTotalPrice);
            Assert.AreEqual(resultingCoverSheetDto.ContractActionType, expectedCoverSheetDto.ContractActionType);
            Assert.AreEqual(resultingCoverSheetDto.ContractTypeGroup, expectedCoverSheetDto.ContractTypeGroup);
            Assert.AreEqual(resultingCoverSheetDto.CoverSheetApproverNtid, expectedCoverSheetDto.CoverSheetApproverNtid);
            Assert.AreEqual(resultingCoverSheetDto.CoverSheetApproverSignedDate, expectedCoverSheetDto.CoverSheetApproverSignedDate);
            Assert.AreEqual(resultingCoverSheetDto.CustomerSubmittalDate, expectedCoverSheetDto.CustomerSubmittalDate);
            Assert.AreEqual(resultingCoverSheetDto.CageCode, expectedCoverSheetDto.CageCode);
            Assert.AreEqual(resultingCoverSheetDto.OfferorAddress.ToString(), expectedCoverSheetDto.OfferorAddress.ToString());
            Assert.AreEqual(resultingCoverSheetDto.ContractsLead, expectedCoverSheetDto.ContractsLead);
        }

        /// <summary>
        /// Get proposal permission set for mocking
        /// </summary>
        /// <returns>List of ProposalPermissionDTOs</returns>
        private List<ProposalPermissionDto> GetPermissionsForMocks()
        {
            ProposalPermissionDto leadEstimator = new ProposalPermissionDto()
            {
                UserId = 1,
                Role = PtmRole.Pricer
            };

            ProposalPermissionDto coverSheetApprover = new ProposalPermissionDto()
            {
                UserId = 2,
                Role = PtmRole.CoverSheetApprover
            };

            ProposalPermissionDto pricingVerification = new ProposalPermissionDto()
            {
                UserId = 3,
                Role = PtmRole.PricingVerification
            };

            ProposalPermissionDto independentReviewer = new ProposalPermissionDto()
            {
                UserId = 4,
                Role = PtmRole.PeerReviewer
            };

            ProposalPermissionDto lobEstimatingLead = new ProposalPermissionDto()
            {
                UserId = 5,
                Role = PtmRole.LOBEstLead
            };

            ProposalPermissionDto contractsPOC = new ProposalPermissionDto
            {
                UserId = 6,
                Role = PtmRole.ContractsPOC
            };

            return new List<ProposalPermissionDto>() { leadEstimator, coverSheetApprover, pricingVerification, independentReviewer, lobEstimatingLead, contractsPOC };
        }

        /// <summary>
        /// Aggregate method to create a full proposal
        /// </summary>
        /// <param name="proposalStatus">Status of the proposal</param>
        /// <returns>FullProposal object</returns>
        private FullProposal GetFullProposalForMocks(ProposalDto proposalDto, ProposalStatus proposalStatus = ProposalStatus.InProgress)
        {
            FullProposal fullProposal = new FullProposal(proposalDto);
            fullProposal.Permissions.AddRange(GetPermissionsForMocks());
            fullProposal.ProposalStatus = proposalStatus;

            return fullProposal;
        }
    }
}