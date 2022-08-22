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
    using GenTRAC.Objects;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the Cover Sheet Data Loader
    /// </summary>
    [TestClass]
    public class CoverSheetDataLoaderTest
    {
        /// <summary>
        /// Gets or sets the Cover Sheet Data Loader
        /// </summary>
        private Mock<ICoverSheetDataLoader> CoverSheetDataLoader { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Loader.
        /// </summary>
        private Mock<IProposalLoader> ProposalLoader { get; set; }

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
            CoverSheetDataLoader = new Mock<ICoverSheetDataLoader>();
            ProposalLoader = new Mock<IProposalLoader>();
            ObjectFactory = new Mock<IFullObjectFactory>();
            UserMapper = new Mock<IUserMapper>();

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);
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

            // Prepare expected Cover Sheet Data
            CoverSheetDataDto expectedCoverSheetDataDto = new CoverSheetDataDto()
            {
                IsCCPDRequired = true,
                ContractActionType = ContractActionType.PriceRevisionRedetermination,
                ContractTypeGroup = 1,
                CoverSheetApproverNtid = "fakeuser2",
                CoverSheetApproverSignedDate = dateToTest,
                CustomerSubmittalDate = dateToTest,
                CageCode = "ABCXYZ",
                OfferorAddress = new List<string>()
                {
                    "Address 1 Mock",
                    "Address 2 Mock",
                    "City Mock",
                    "State Mock",
                    "Zip Mock"
                },
                ContractsLead = "fakeuser6"
            };

            // Setup returns
            ProposalLoader.Setup(x => x.GetById(proposalId)).Returns(fakeProposalDto);
            CoverSheetDataLoader.Setup(x => x.GetCoverSheetDataById(proposalId)).Returns(expectedCoverSheetDataDto);

            CoverSheetDataDto resultingCoverSheetDataDto = CoverSheetDataLoader.Object.GetCoverSheetDataById(proposalId);

            Assert.IsNotNull(resultingCoverSheetDataDto);
            Assert.AreEqual(expectedCoverSheetDataDto, resultingCoverSheetDataDto);
        }

        /// <summary>
        /// Get Cover Sheet Data by Id empty proposal test
        /// </summary>
        [TestMethod]
        public void GetCoverSheetDataByIdTestEmptyProposal()
        {
            int proposalId = 1;
            DateTime dateToTest = new DateTime(2010, 8, 18, 16, 32, 0);

            // Prepare a fake proposal Dto
            ProposalDto fakeProposalDto = null;

            // Setup expected data
            ProposalLoader.Setup(x => x.GetById(proposalId)).Returns(fakeProposalDto);

            Assert.IsNull(CoverSheetDataLoader.Object.GetCoverSheetDataById(proposalId));
        }
    }
}