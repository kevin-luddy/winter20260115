// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Objects
{
    using System;
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using GenTRAC.Tests.DAL;
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test Full Object Facotry Class
    /// </summary>
    [TestClass]
    public class FullObjectFactoryTest
    {
        /// <summary>
        /// Create System
        /// </summary>
        /// <returns>Full Object Factory</returns>
        public FullObjectFactory CreateSystem()
        {
            return new FullObjectFactory();
        }

        /// <summary>
        /// Test CreateFullProposal
        /// </summary>
        [TestMethod]
        public void O_CreateFullProposalTest()
        {
            FullObjectFactory sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalTitle = "test Proposal",
                ProposalStatus = ProposalStatus.InProgress,
                TrackingNumber = DateTime.Now.ToString("yy") + "-00005",
                ForecastedTrackingNumber = "F" + DateTime.Now.ToString("yy") + "-00005",
                OTISOpportunityID = "otis",
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int>() { 1, 4 }, // CostPlusAwardFee, FirmFixedPrice 
                CostElementTypeIds = new List<int> { (int)CostElementType.Labor },
                Customer = "cde",
                CustomerType = CustomerType.InternationalForeignMilitarySaleUSGovt,
                DeliveryDate = new DateTime(2013, 6, 1),
                EstimatedProposalValue = 0,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 1,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 1,
                ProgramName = "def",
                ProposalType = 1,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "ghi",
                DateAssigned = new DateTime(2014, 5, 1),
                DateCreated = new DateTime(2015, 12, 30),
                ContractActionType = ContractActionType.Other,
                ContractActionTypeOtherText = "Other text."
            };

            Mock<IRetriever> retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(retriever.Object);

            FullProposal expected = new FullProposal(proposal);

            FullProposal result = sut.CreateFullProposal(proposal);

            DtoAssertHelpers.AssertDtos(expected, result);
        }

        /// <summary>
        /// Test CreateFullProposal - Legacy uses a 4 digit year
        /// </summary>
        [TestMethod]
        public void O_CreateFullProposalLegacyTest()
        {
            FullObjectFactory sut = this.CreateSystem();
            bool legacy = true;

            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalTitle = "test Proposal",
                ProposalStatus = ProposalStatus.InProgress,
                TrackingNumber = DateTime.Now.Year.ToString() + "-00005",
                ForecastedTrackingNumber = "F" + DateTime.Now.ToString("yy") + "-00005",
                OTISOpportunityID = "otis",
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int>() { 1, 4 }, // CostPlusAwardFee, FirmFixedPrice
                CostElementTypeIds = new List<int> { (int)CostElementType.Labor },
                Customer = "cde",
                CustomerType = CustomerType.InternationalForeignMilitarySaleUSGovt,
                DeliveryDate = new DateTime(2013, 6, 1),
                EstimatedProposalValue = 0,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 1,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 1,
                ProgramName = "def",
                ProposalType = 6,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "ghi",
                DateAssigned = new DateTime(2014, 5, 1),
                DateCreated = new DateTime(2015, 12, 30),
                ContractActionType = ContractActionType.Other,
                ContractActionTypeOtherText = "Other text."
            };

            Mock<IRetriever> retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(retriever.Object);

            FullProposal expected = new FullProposal(proposal);

            FullProposal result = sut.CreateFullProposal(proposal);

            DtoAssertHelpers.AssertDtos(expected, result, legacy);
        }

        #region Exception tests

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public void C_CreateFullProposal_ExceptionTest()
        {
            FullObjectFactory sut = this.CreateSystem();
            sut.CreateFullProposal(null);
        }

        #endregion
    }
}
