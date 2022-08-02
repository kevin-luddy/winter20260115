// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Checklist controller logic test
    /// </summary>
    [TestClass]
    public class ChecklistControllerLogicTest
    {
        #region Properties

        /// <summary>
        /// Security Access
        /// </summary>
        private Mock<ISecurityAccess> securityAccess = null;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private Mock<IProposalLoader> proposalLoader = null;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private Mock<IES.Common.IActiveDirectoryUtilities> adUtils = null;

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
        /// checklist mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator = null;

        /// <summary>
        /// proposal checklist loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader = null;

        /// <summary>
        /// propsoal mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<IApprovalsLoader> approvalsLoader;

        #endregion

        /// <summary>
        /// Checklist Controller logic under test
        /// </summary>
        /// <returns>home controller logic</returns>
        private ChecklistControllerLogic CreateSystem()
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

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            Mock<IES.Common.SecurityInformation> security = new Mock<IES.Common.SecurityInformation>(this.adUtils.Object);
            IES.Common.UserData user = new IES.Common.UserData()
            {
                Email = "james.basilio@lmco.com"
            };
            security.Setup(x => x.ActiveUserData).Returns(user);

            return new ChecklistControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object,
                this.objectFactory.Object, this.checklistMediator.Object, this.proposalMediator.Object,
                this.approvalsLoader.Object, this.proposalChecklistLoader.Object);
        }

        /// <summary>
        /// test GetDataForChecklistGeneralInformation
        /// </summary>
        [TestMethod]
        public void C_GetDataForChecklistGeneralInformationTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;
            int peerReviewerID = 12;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 1,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                IsCCPDRequired = true
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22
            };

            UserDTO peerReviewer = new UserDTO()
            {
                Id = peerReviewerID,
                Ntid = "blah",
                DisplayName = "Blah, Blah"
            };

            ProposalPermissionDto permission1 = new ProposalPermissionDto()
            {
                Id = 1,
                Role = PtmRole.Pricer,
                UserId = peerReviewerID
            };
            ICollection<ProposalPermissionDto> permissions = new Collection<ProposalPermissionDto> { permission1 };

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.userMapper.Setup(x => x.GetById(peerReviewerID)).Returns(peerReviewer);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId)).Returns(permissions);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);

            ChecklistGeneralInformationModelView checklistGeneralInfo = sut.GetDataForChecklistGeneralInformation(proposalId);

            Assert.AreEqual(checklistGeneralInfo.UpdateDate, proposalChecklist.UpdateDate);
            Assert.AreEqual(checklistGeneralInfo.ProposalID, proposalChecklist.ProposalID);
            Assert.AreEqual(checklistGeneralInfo.PricerDisplayName, peerReviewer.DisplayName);
            Assert.AreEqual(checklistGeneralInfo.SubmittedValue, proposalChecklist.SubmittedValue.ToString());
            if (proposalChecklist.EstimatingSubmitsToContractsDate.HasValue)
            {
                Assert.AreEqual(checklistGeneralInfo.EstimatingSubmitsToContractsDate, proposalChecklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy"));
            }
        }

        /// <summary>
        /// Save checklist with Peer Reviewer User NTID
        /// </summary>
        [TestMethod]
        public void C_SaveChecklistWithPeerReviewerUserNtIdTest()
        {
            var sut = this.CreateSystem();

            int userId = 10;
            int proposalId = 5;
            int checklistID = 7;

            UserDTO peerReviewer = new UserDTO()
            {
                UserType = IES.Common.UserType.User,
                Id = userId,
                Ntid = "blah"
            };

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString()
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView();

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView();

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView();

            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(peerReviewer);
            this.checklistMediator.Setup(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>())).Returns(checklistID);
            int? returnedID = sut.SaveChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false);

            Assert.IsNotNull(returnedID);
        }

        /// <summary>
        /// test GetDataForChecklistProposalPricingData
        /// </summary>
        [TestMethod]
        public void C_GetDataForChecklistProposalPricingDataTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                DateCreated = new DateTime(2021, 1, 1)
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                LMLaborHrs = -2.22m,
                LMLaborCost = 22,
                SubcontractorCost = -22,
                MaterialCost = 22,
                IWTACost = 22,
                TravelCost = -22,
                OtherDirectCosts = 22,
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = -98.89m
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(new Collection<ProposalChecklistSaveInfo>());
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);

            ChecklistProposalPricingDataModelView checklistProposalPricingData = sut.GetDataForChecklistProposalPricingData(proposalId);
            Assert.AreEqual(checklistProposalPricingData.UpdateDate, proposalChecklist.UpdateDate);
            Assert.AreEqual(checklistProposalPricingData.ProposalID, proposalChecklist.ProposalID);
            Assert.AreEqual(checklistProposalPricingData.LMLaborHrs, proposalChecklist.LMLaborHrs.ToString());
            Assert.AreEqual(checklistProposalPricingData.LMLaborCost, proposalChecklist.LMLaborCost.ToString());
            Assert.AreEqual(checklistProposalPricingData.SubcontractorCost, proposalChecklist.SubcontractorCost.ToString());
            Assert.AreEqual(checklistProposalPricingData.MaterialCost, proposalChecklist.MaterialCost.ToString());
            Assert.AreEqual(checklistProposalPricingData.IWTACost, proposalChecklist.IWTACost.ToString());
            Assert.AreEqual(checklistProposalPricingData.TravelCost, proposalChecklist.TravelCost.ToString());
            Assert.AreEqual(checklistProposalPricingData.OtherDirectCosts, proposalChecklist.OtherDirectCosts.ToString());
            Assert.AreEqual(checklistProposalPricingData.ProfitFeeComTotal, string.Format("{0:#,###0}", proposalChecklist.ProfitFeeWithCom));
            Assert.AreEqual(checklistProposalPricingData.ProfitFeeTotal, string.Format("{0:#,###0}", proposalChecklist.Profit));
            Assert.AreEqual(checklistProposalPricingData.ComTotal, string.Format("{0:#,###0}", proposalChecklist.Com));
            Assert.AreEqual(checklistProposalPricingData.ROSPercent, proposalChecklist.ROSPercentage.ToString());
            Assert.IsFalse(checklistProposalPricingData.SplitProfitFeeCOM);
        }

        /// <summary>
        /// test GetDataForChecklistProposalPricingData.. Only check here is that SplitProfitFeeCom = true
        /// </summary>
        [TestMethod]
        public void C_GetDataForChecklistProposalPricingDataTest_2()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                DateCreated = new DateTime(2021, 8, 1)
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(new Collection<ProposalChecklistSaveInfo>());

            ChecklistProposalPricingDataModelView checklistProposalPricingData = sut.GetDataForChecklistProposalPricingData(proposalId);
            Assert.IsTrue(checklistProposalPricingData.SplitProfitFeeCOM);
        }

        /// <summary>
        /// Verify that the validator is catching all of the pricing data errors
        /// </summary>
        [TestMethod]
        public void C_RunPricingValidationTest()
        {
            var validationResults = new List<ValidationResult>();
            var checkList = new ChecklistProposalPricingDataModelView()
            {
                ProposalID = 1,
                ProposalChecklistID = 1,
                LMLaborHrs = "-999,999,999.99",
                LMLaborCost = "-999,999,999,999",
                SubcontractorCost = "-999,999,999,999",
                MaterialCost = "-999,999,999,999",
                IWTACost = "-999,999,999,999",
                TravelCost = "-999,999,999,999",
                OtherDirectCosts = "-999,999,999,999",
                ProfitFeeComTotal = "-999,999,999,999",
                ProfitFeeTotal = "-999,999,999,999",
                ComTotal = "-999,999,999,999",
                ROSPercent = "-99.99"
            };

            var ctx = new ValidationContext(checkList, null, null);
            var result = Validator.TryValidateObject(checkList, ctx, validationResults, true);
            Assert.IsTrue(result);

            validationResults = new List<ValidationResult>();
            checkList.ROSPercent = "-0.99";

            ctx = new ValidationContext(checkList, null, null);
            result = Validator.TryValidateObject(checkList, ctx, validationResults, true);
            Assert.IsTrue(result);

            validationResults = new List<ValidationResult>();
            checkList = new ChecklistProposalPricingDataModelView()
            {
                ProposalID = 1,
                ProposalChecklistID = 1,
                LMLaborHrs = "-9.123",
                LMLaborCost = "-abc,def,ghi,jkl",
                SubcontractorCost = "-abc,def,ghi,jkl",
                MaterialCost = "-abc,def,ghi,jkl",
                IWTACost = "-abc,def,ghi,jkl,",
                TravelCost = "-abc,def,ghi,jkl",
                OtherDirectCosts = "-abc,def,ghi,jkl",
                ProfitFeeComTotal = "-abc,def,ghi,jkl",
                ProfitFeeTotal = "-abc,def,ghi,jkl",
                ComTotal = "-abc,def,ghi,jkl",
                ROSPercent = "-100.999"
            };

            ctx = new ValidationContext(checkList, null, null);
            result = Validator.TryValidateObject(checkList, ctx, validationResults, true);
            Assert.IsFalse(result);
            Assert.AreEqual(11, validationResults.Count);
        }

        /// <summary>
        /// Verify that the validator is catching the PAR Row Response entry errors
        /// </summary>
        [TestMethod]
        public void C_RunPARRowResponseValidationTest()
        {
            var validationResults = new List<ValidationResult>();
            var rowResponse = new ChecklistRowModelView()
            {
                RowType = ChecklistTextType.PricerComment,
                PeerResponse = ChecklistResponseOption.NotSet,
                PricerResponse = ChecklistResponseOption.Yes,
                DisplayText = "Technical data",
                ChecklistContentId = 12345,
                SortOrder = 2,
                ColumnOrder = 3,
                PricerPageNumber = "012345678901234567890123456789",
                PricerRowComment = "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789"
            };

            var ctx = new ValidationContext(rowResponse, null, null);
            var result = Validator.TryValidateObject(rowResponse, ctx, validationResults, true);
            Assert.IsTrue(result);

            validationResults = new List<ValidationResult>();
            rowResponse = new ChecklistRowModelView()
            {
                RowType = ChecklistTextType.PricerComment,
                PeerResponse = ChecklistResponseOption.NotSet,
                PricerResponse = ChecklistResponseOption.Yes,
                DisplayText = "Technical data",
                ChecklistContentId = 12345,
                SortOrder = 2,
                ColumnOrder = 3,
                PricerPageNumber = "0123456789012345678901234567890123456789",
                PricerRowComment = "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789" +
                                   "01234567890123456789012345678901234567890123456789"
            };

            ctx = new ValidationContext(rowResponse, null, null);
            result = Validator.TryValidateObject(rowResponse, ctx, validationResults, true);
            Assert.IsFalse(result);
            Assert.AreEqual(2, validationResults.Count);
        }

        /// <summary>
        /// Save checklist
        /// </summary>
        [TestMethod]
        public void C_SaveChecklistTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;
            string pprPricerComment = "ppr Pricer Comment";
            string parPricerComment = "par Pricing Comment";

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 2,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString()
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                ProfitFeeComTotal = "7777",
                ProfitFeeTotal = "777",
                ComTotal = "77",
                ROSPercent = "98989"
            };

            ChecklistRowModelView row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row9 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "Pricer Comment is", RowType = ChecklistTextType.PricerComment };
            Collection<ChecklistRowModelView> checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row9);

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                PPRPricerComment = pprPricerComment
            };

            ChecklistRowModelView row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No, SelectedCannedResponse = 1 };
            ChecklistRowModelView row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment };
            ChecklistRowModelView row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment };

            Collection<ChecklistRowModelView> checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = parPricerComment,
                PARPeerComment = string.Empty
            };

            this.checklistMediator.Setup(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>())).Returns(checklistID);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);

            int? returnedID = sut.SaveChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false);
            Assert.AreEqual(returnedID, checklistID);

            // Assert works for checklist version using canned responses
            checklistPARData.ChecklistVersion = ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION;
            returnedID = sut.SaveChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false);
            Assert.AreEqual(returnedID, checklistID);
        }

        /// <summary>
        /// test GetDataForChecklistPPRDocument
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Pricer"), TestMethod]
        public void C_GetDataForChecklistPPRDocumentTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;
            string pprPricerComment = "ppr Pricer Comment";

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            // PPR responses/contents
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Pricer Comments", TextType = ChecklistTextType.PricerComment };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                AbsoluteValue = 1500,
                ROSPercentage = 98989,
                PPRResponses = responses
            };

            proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview,
                new ProposalChecklistSaveInfo { UserID = 22, LastSaveDate = new DateTime(2014, 5, 2), SubmitDate = null, Comment = pprPricerComment });

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3 }
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            UserDTO pricer = new UserDTO { Id = 22, DisplayName = "Stevens, Bob" };
            this.userMapper.Setup(x => x.GetById(pricer.Id)).Returns(pricer);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);

            // get data
            ChecklistProposalPricingReviewDocumentModelView pprData = sut.GetDataForChecklistPPRDocument(proposalId);
            Assert.AreEqual(pprData.UpdateDate, proposalChecklist.UpdateDate);
            Assert.AreEqual(pprData.ProposalID, proposalChecklist.ProposalID);
            Assert.AreEqual(pprData.ProposalChecklistID, proposalChecklist.Id);
            Assert.AreEqual(pprData.PPRPricerComment, pprPricerComment);

            // verify rows
            var rows = pprData.ChecklistRows;

            // verify first row
            var row1 = (from r in rows
                        where r.SortOrder == content1.SortOrder
                        select r).First();
            Assert.AreEqual(row1.DisplayText, content1.Text);
            Assert.AreEqual(row1.RowType, content1.TextType);
            Assert.AreEqual(row1.PricerResponse, response1.Response);
            Assert.AreEqual(row1.PricerResponse, ChecklistResponseOption.NotSet);

            // verify second row
            var row2 = (from r in rows
                        where r.SortOrder == content2.SortOrder
                        select r).First();
            Assert.AreEqual(row2.DisplayText, content2.Text);
            Assert.AreEqual(row2.RowType, content2.TextType);
            Assert.AreEqual(row2.PricerResponse, response2.Response);
            Assert.AreEqual(row2.PricerResponse, ChecklistResponseOption.Yes);
        }

        /// <summary>
        /// test GetDataForChecklistPPRDocument when checklist is null
        /// the proposal checklist dto can be null right after the proposal has been saved. the checklist content has been populated but nothing else
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Pricer"), TestMethod]
        public void C_GetDataForChecklistPPRDocumentTest_WhenChecklistNull()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            // PPR responses/contents
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Pricer Comments", TextType = ChecklistTextType.PricerComment };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3 }
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            // get data
            ChecklistProposalPricingReviewDocumentModelView pprData = sut.GetDataForChecklistPPRDocument(proposalId);
            Assert.AreEqual(pprData.ProposalID, proposalId);
            Assert.AreEqual(pprData.PPRPricerComment, string.Empty);
            Assert.IsTrue(pprData.ChecklistRows.Any());

            // verify rows
            var rows = pprData.ChecklistRows;

            // verify first row
            var row1 = (from r in rows
                        where r.SortOrder == content1.SortOrder
                        select r).First();
            Assert.AreEqual(row1.DisplayText, content1.Text);
            Assert.AreEqual(row1.RowType, content1.TextType);
            Assert.AreEqual(row1.PricerResponse, response1.Response);
            Assert.AreEqual(row1.PricerResponse, ChecklistResponseOption.NotSet);

            // verify second row
            var row2 = (from r in rows
                        where r.SortOrder == content2.SortOrder
                        select r).First();
            Assert.AreEqual(row2.DisplayText, content2.Text);
            Assert.AreEqual(row2.RowType, content2.TextType);
            Assert.AreEqual(row2.PricerResponse, response2.Response);
            Assert.AreEqual(row2.PricerResponse, ChecklistResponseOption.Yes);
        }

        /// <summary>
        /// test GetDataForChecklistPARDocument
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Pricer"), TestMethod]
        public void C_GetDataForChecklistPARDocumentTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;
            string parPricerComment = "par Pricer Comment";
            string parPeerComment = "par Peer Comment";

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                ProposalStatus = ProposalStatus.InProgress,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 4,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            IDictionary<int, string> cannedResponses = new Dictionary<int, string>();
            cannedResponses.Add(1, "Test Response 1");
            cannedResponses.Add(2, "Test Response 2");
            cannedResponses.Add(3, "Test Response 3");
            cannedResponses.Add(4, "Test Response 4");

            // PPR responses/contents
            ChecklistContentItem pprContent1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "To be completed by the Lead Estimator", TextType = ChecklistTextType.Text };
            ChecklistContentItem pprContent2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "1. This proposal is exempt from completion of the Proposal Adequacy Review (PAR) Document?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> pprResponses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem pprResponse1 = new ChecklistResponseItem { ChecklistContentId = pprContent1.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem pprResponse2 = new ChecklistResponseItem { ChecklistContentId = pprContent2.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            pprResponses.Add(pprResponse1);
            pprResponses.Add(pprResponse2);

            // PAR responses/contents
            ChecklistContentItem parContent1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text, CannedResponses = cannedResponses };
            ChecklistContentItem parContent2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question, CannedResponses = cannedResponses };
            ChecklistContentItem parContent3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Pricer Comments", TextType = ChecklistTextType.PricerComment, CannedResponses = cannedResponses };
            ChecklistContentItem parContent4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Peer Comments", TextType = ChecklistTextType.PeerComment, CannedResponses = cannedResponses };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem parResponse1 = new ChecklistResponseItem { ChecklistContentId = parContent1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem parResponse2 = new ChecklistResponseItem { ChecklistContentId = parContent2.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes};
            ChecklistResponseItem parResponse3 = new ChecklistResponseItem { ChecklistContentId = parContent2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No, CannedResponseId = 1 };
            responses.Add(parResponse1);
            responses.Add(parResponse2);
            responses.Add(parResponse3);

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                AbsoluteValue = 1500,
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989,
                PARResponses = responses,
                ResponseType = ChecklistResponseType.Pricer
            };

            proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview,
                new ProposalChecklistSaveInfo { UserID = 22, LastSaveDate = new DateTime(2014, 5, 2), SubmitDate = null, Comment = parPricerComment });

            proposalChecklist.UserSaveInfo[ChecklistResponseType.Peer].Add(ChecklistType.ProposalAdequacyReview,
            new ProposalChecklistSaveInfo { UserID = 24, LastSaveDate = new DateTime(2014, 5, 5), SubmitDate = null, Comment = parPeerComment });

            ChecklistContentDto pprChecklistContent = new ChecklistContentDto()
            {
                Version = 4,
                Content = new Collection<ChecklistContentItem> { pprContent1, pprContent2 }
            };

            ChecklistContentDto parChecklistContent = new ChecklistContentDto()
            {
                Version = 4,
                Content = new Collection<ChecklistContentItem> { parContent1, parContent2, parContent3, parContent4 }
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(pprChecklistContent);
            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId)).Returns(parChecklistContent);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(new Collection<ProposalChecklistSaveInfo>());
            UserDTO pricer = new UserDTO { Id = 22, DisplayName = "Stevens, Bob" };
            this.userMapper.Setup(x => x.GetById(pricer.Id)).Returns(pricer);
            UserDTO peer = new UserDTO { Id = 24, DisplayName = "Monster, Cookie" };
            this.userMapper.Setup(x => x.GetById(peer.Id)).Returns(peer);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(pprResponses);
            this.proposalChecklistLoader.Setup(x => x.GetPARResponses(proposalId)).Returns(responses);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);

            // get data
            ChecklistProposalAdequacyReviewDocumentModelView parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.AreEqual(parData.UpdateDate, proposalChecklist.UpdateDate);
            Assert.AreEqual(parData.ProposalID, proposalChecklist.ProposalID);
            Assert.AreEqual(parData.ProposalChecklistID, proposalChecklist.Id);
            Assert.AreEqual(parData.PricerLastSavedDate, proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalAdequacyReview].LastSaveDate.Value.ToString("MM/dd/yyyy h:mm:ss tt"));
            Assert.AreEqual(parData.PARPricerComment, parPricerComment);
            Assert.AreEqual(parData.PARPeerComment, parPeerComment);
            Assert.AreEqual(parData.SecondHeaderColSpan, 4);
            Assert.AreEqual(parData.CommentColSpan, 5);
            Assert.AreEqual(parData.TextColSpan, 5);
            Assert.AreEqual(parData.DocumentFinalRowColSpan, 5);
            Assert.AreEqual(parData.RadioResponseColSpan, 3);
            Assert.IsFalse(parData.ShowExportButton);

            Assert.IsTrue(parData.ChecklistRows.Any());

            // Verify ShowCommentHideNA logic
            pprChecklistContent.Version = 6;
            parChecklistContent.Version = 6;
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            fullProposal = new FullProposal(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.AreEqual(parData.SecondHeaderColSpan, 6);
            Assert.AreEqual(parData.CommentColSpan, 7);
            Assert.AreEqual(parData.TextColSpan, 7);
            Assert.AreEqual(parData.DocumentFinalRowColSpan, 7);
            Assert.AreEqual(parData.RadioResponseColSpan, 3);

            // Verify ShowExport logic (Completed proposal)
            ProposalChecklistSaveInfo saveInfo = new ProposalChecklistSaveInfo() { SubmitDate = DateTime.Now, ResponseType = ChecklistResponseType.Pricer };
            ICollection<ProposalChecklistSaveInfo> checklistSaveInfo = new Collection<ProposalChecklistSaveInfo>() { saveInfo };
            proposal.ProposalStatus = ProposalStatus.Completed;
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            fullProposal = new FullProposal(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.AreEqual(parData.SecondHeaderColSpan, 10);
            Assert.AreEqual(parData.CommentColSpan, 11);
            Assert.AreEqual(parData.TextColSpan, 11);
            Assert.AreEqual(parData.DocumentFinalRowColSpan, 11);
            Assert.AreEqual(parData.RadioResponseColSpan, 3);
            Assert.IsFalse(parData.ShowExportButton);

            // Verify ShowExport logic (InProgress proposal) // TODO
            proposal.ProposalStatus = ProposalStatus.InProgress;
            parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.IsFalse(parData.ShowExportButton);

            // verify rows
            var rows = parData.ChecklistRows;
            // verify first row
            var row1 = (from r in rows
                        where r.SortOrder == parContent1.SortOrder
                        select r).First();
            Assert.AreEqual(row1.DisplayText, parContent1.Text);
            Assert.AreEqual(row1.RowType, parContent1.TextType);
            Assert.AreEqual(row1.PricerResponse, parResponse1.Response);
            Assert.AreEqual(row1.PricerResponse, ChecklistResponseOption.NotSet);

            // verify second row
            var row2 = (from r in rows
                        where r.SortOrder == parContent2.SortOrder
                        select r).First();
            Assert.AreEqual(row2.DisplayText, parContent2.Text);
            Assert.AreEqual(row2.RowType, parContent2.TextType);
            Assert.AreEqual(row2.PricerResponse, parResponse3.Response);
            Assert.AreEqual(row2.PricerResponse, ChecklistResponseOption.No);
            Assert.AreEqual(row2.PeerResponse, parResponse2.Response);
            Assert.AreEqual(row2.PeerResponse, ChecklistResponseOption.Yes);

            // Test for checklist version with canned responses
            parChecklistContent.Version = ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION;
            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId)).Returns(parChecklistContent);

            parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.IsTrue(parData.ShowExportButton);

            // verify rows
            rows = parData.ChecklistRows;
            // verify first row
            row1 = (from r in rows
                        where r.SortOrder == parContent1.SortOrder
                        select r).First();
            Assert.AreEqual(row1.DisplayText, parContent1.Text);
            Assert.AreEqual(row1.RowType, parContent1.TextType);
            Assert.AreEqual(row1.PricerResponse, parResponse1.Response);
            Assert.AreEqual(row1.PricerResponse, ChecklistResponseOption.NotSet);

            // verify second row
            row2 = (from r in rows
                        where r.SortOrder == parContent2.SortOrder
                        select r).First();
            Assert.AreEqual(row2.DisplayText, parContent2.Text);
            Assert.AreEqual(row2.RowType, parContent2.TextType);
            Assert.AreEqual(row2.PricerResponse, parResponse3.Response);
            Assert.AreEqual(row2.PricerResponse, ChecklistResponseOption.No);
            Assert.AreEqual(row2.SelectedCannedResponse, parResponse3.CannedResponseId);
            Assert.AreEqual(row2.PeerResponse, parResponse2.Response);
            Assert.AreEqual(row2.PeerResponse, ChecklistResponseOption.Yes);
        }

        /// <summary>
        /// test GetDataForChecklistPARDocument when the proposal checklist is null
        /// This is basically what will happen after a proposal has been created but no checklist info has been filled out by the user
        /// </summary>
        [TestMethod]
        public void C_GetDataForChecklistPARDocumentTest_WhenChecklistNull()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 4,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };
            
            // PAR responses/contents
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 4, Text = "Comments", TextType = ChecklistTextType.PeerComment };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem response3 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            Collection<ChecklistResponseItem> pricerResponsesInDB = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem pricerResponse1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No, CannedResponseId = 1 }; // this should be No so we can see Q1 response 
            ChecklistResponseItem pricerResponse2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No, CannedResponseId = 2 };
            ChecklistResponseItem pricerResponse3 = new ChecklistResponseItem { ChecklistContentId = content3.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem pricerResponse4 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No, CannedResponseId = 4 };
            pricerResponsesInDB.Add(pricerResponse1);
            pricerResponsesInDB.Add(pricerResponse2);
            pricerResponsesInDB.Add(pricerResponse3);
            pricerResponsesInDB.Add(pricerResponse4);

            ProposalChecklistDto checklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposalId,
                PARResponses = responses, // just want peer responses for this test case so we can wipe them out
                PPRResponses = pricerResponsesInDB
            };

            FullProposal fullProposal = new FullProposal(proposal);

            ProposalChecklistSaveInfo saveInfo = new ProposalChecklistSaveInfo() { SubmitDate = DateTime.Now, ResponseType = ChecklistResponseType.Pricer };
            ICollection<ProposalChecklistSaveInfo> checklistSaveInfo = new Collection<ProposalChecklistSaveInfo>();
            checklistSaveInfo.Add(saveInfo);

            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId)).Returns(checklistContent);
            UserDTO pricer = new UserDTO { Id = 22, DisplayName = "Stevens, Bob" };
            this.userMapper.Setup(x => x.GetById(pricer.Id)).Returns(pricer);
            this.proposalChecklistLoader.Setup(x => x.GetPARResponses(proposalId)).Returns(responses);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { checklist });

            // get data
            ChecklistProposalAdequacyReviewDocumentModelView parData = sut.GetDataForChecklistPARDocument(proposalId, string.Empty);
            Assert.AreEqual(parData.ProposalID, proposalId);
            Assert.AreEqual(parData.PricerLastSavedDate, string.Empty);
            Assert.AreEqual(parData.PARPricerComment, string.Empty);
            Assert.AreEqual(parData.PARPeerComment, string.Empty);

            Assert.IsTrue(parData.ChecklistRows.Any());

            // verify rows
            var rows = parData.ChecklistRows;

            // verify first row
            var row1 = (from r in rows
                        where r.SortOrder == content1.SortOrder
                        select r).First();
            Assert.AreEqual(row1.DisplayText, content1.Text);
            Assert.AreEqual(row1.RowType, content1.TextType);
            Assert.AreEqual(row1.PricerResponse, response1.Response);
            Assert.AreEqual(row1.PricerResponse, ChecklistResponseOption.NotSet);

            // verify second row
            var row2 = (from r in rows
                        where r.SortOrder == content2.SortOrder
                        select r).First();
            Assert.AreEqual(row2.DisplayText, content2.Text);
            Assert.AreEqual(row2.RowType, content2.TextType);
            Assert.AreEqual(row2.PricerResponse, response3.Response);
            Assert.AreEqual(row2.PricerResponse, ChecklistResponseOption.NotSet);
            Assert.AreEqual(row2.PeerResponse, response2.Response);
            Assert.AreEqual(row2.PeerResponse, ChecklistResponseOption.NotSet);
        }

        /// <summary>
        /// Validate checklist test. This test will validate on the save as a pricer
        /// </summary>
        [TestMethod]
        public void C_ValidateChecklistTest_SaveAsPricer()
        {
            var sut = this.CreateSystem();
            int proposalId = 5;
            int checklistID = 7;
            int peerReviewerID = 12;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId
            };

            // PPR responses/contents
            ChecklistContentItem pprContent1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "To be completed by the Lead Estimator", TextType = ChecklistTextType.Text };
            ChecklistContentItem pprContent2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "1. This proposal is exempt from completion of the Proposal Adequacy Review (PAR) Document?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> pprResponses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem pprResponse1 = new ChecklistResponseItem { ChecklistContentId = pprContent1.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem pprResponse2 = new ChecklistResponseItem { ChecklistContentId = pprContent2.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            pprResponses.Add(pprResponse1);
            pprResponses.Add(pprResponse2);

            // PAR responses/contents
            ChecklistContentItem parContent1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem parContent2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem parContent3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem parContent4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Peer Comments", TextType = ChecklistTextType.PeerComment };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem parResponse1 = new ChecklistResponseItem { ChecklistContentId = parContent1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            ChecklistResponseItem parResponse2 = new ChecklistResponseItem { ChecklistContentId = parContent2.Id, ResponseType = ChecklistResponseType.Peer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem parResponse3 = new ChecklistResponseItem { ChecklistContentId = parContent2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(parResponse1);
            responses.Add(parResponse2);
            responses.Add(parResponse3);

            ChecklistContentDto pprChecklistContent = new ChecklistContentDto()
            {
                Version = 4,
                Content = new Collection<ChecklistContentItem> { pprContent1, pprContent2 }
            };

            ChecklistContentDto parChecklistContent = new ChecklistContentDto()
            {
                Version = 4,
                Content = new Collection<ChecklistContentItem> { parContent1, parContent2, parContent3, parContent4 }
            };

            FullProposal fullProposal = new FullProposal(proposal);

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = "05062013",
                PricerId = peerReviewerID,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                ProfitFeeComTotal = string.Empty,
                ProfitFeeTotal = string.Empty,
                ComTotal = string.Empty,
                ROSPercent = string.Empty,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                ChecklistVersion = 4
            };
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(pprChecklistContent);
            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId)).Returns(parChecklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(pprResponses);
            this.proposalChecklistLoader.Setup(x => x.GetPARResponses(proposalId)).Returns(responses);

            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            var validationIssues = (from v in validationMessages
                                    select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.SUBMITTAL_DATE_FORMAT));

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();
            checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString(),
                PricerId = 14,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            // validate checklist on the save, will pass
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false, validationMessages);
            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// C_ValidateChecklistTest_SubmitAsPricer_Legacy
        /// Validate checklist test. This test will validate on the submit by a pricer
        /// This checklist logic uses legacy code to validate the PAR checklist "No" responses, and the legacy ChecklistVersion is less than 6
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), TestMethod]
        public void C_ValidateChecklistTest_SubmitAsPricer_Legacy()
        {
            var sut = this.CreateSystem();
            int proposalId = 5;
            int checklistID = 7;
            string pprPricerComment = "ppr Pricer Comment";
            string parPricerComment = "par Pricer Comment";

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = string.Empty,
                PricerId = 15,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = string.Empty,
                LMLaborCost = string.Empty,
                SubcontractorCost = string.Empty,
                MaterialCost = string.Empty,
                IWTACost = string.Empty,
                TravelCost = string.Empty,
                OtherDirectCosts = string.Empty,
                ProfitFeeComTotal = string.Empty,
                ProfitFeeTotal = string.Empty,
                ComTotal = string.Empty,
                ROSPercent = string.Empty,
                CostThroughCom = string.Empty,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistRowModelView row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row9 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment, PricerResponse = ChecklistResponseOption.NotSet };
            Collection<ChecklistRowModelView> checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row9);

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = string.Empty
            };

            ChecklistRowModelView row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "3", PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet };
            Collection<ChecklistRowModelView> checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                ChecklistVersion = 4
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                IsCCPDRequired = false
            };

            // PPR responses/contents. 
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = row1.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = row2.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = row4.DisplayText, TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row1.PricerResponse };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row2.PricerResponse };
            ChecklistResponseItem response4 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row4.PricerResponse };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response4);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            var validationIssues = (from v in validationMessages
                                    select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ESTIMATING_SUBMITS_TO_CONTRACTS_DATE_REQUIRED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PPR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PRICER_COMMENT_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            // fix PPR validation, general validation, and proposal pricing but validate Pricer PAR now
            checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString(),
                PricerId = 14,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                SubmittedValue = "10"
            };

            checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = "22.12",
                LMLaborCost = "22",
                SubcontractorCost = "22",
                MaterialCost = "22",
                IWTACost = "22",
                TravelCost = "22",
                OtherDirectCosts = "22",
                ProfitFeeComTotal = "234",
                ProfitFeeTotal = "34",
                ComTotal = "24",
                ROSPercent = "22.56",
                CostThroughCom = "22",
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row9 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment};
            checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row9);

            checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = pprPricerComment
            };

            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.NotSet };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet};
            ChecklistRowModelView row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment, PeerResponse = ChecklistResponseOption.NotSet };

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                PARPeerComment = string.Empty,
                ChecklistVersion = 4
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));

            // now fix everything on validate for a pricer
            validationMessages = new Collection<ValidationMessage>();
            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.Yes };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment };
            row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment };

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = parPricerComment,
                PARPeerComment = string.Empty
            };

            // validate checklist on the save, should succeed
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);
            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// C_ValidateChecklistTest_SubmitAsPricer
        /// Validate checklist test. This test will validate on the submit by a pricer
        /// This checklist validation uses a different set of logic than the legacy logic to validate row responses and ChecklistVersion >= 6
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), TestMethod]
        public void C_ValidateChecklistTest_SubmitAsPricer()
        {
            var sut = this.CreateSystem();
            int proposalId = 5;
            int checklistID = 7;
            string pprPricerComment = "ppr Pricer Comment";
            string parPricerComment = "par Pricer Comment";

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = string.Empty,
                PricerId = 15,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                DeliverChecklistDFARS = true
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = string.Empty,
                LMLaborCost = string.Empty,
                SubcontractorCost = string.Empty,
                MaterialCost = string.Empty,
                IWTACost = string.Empty,
                TravelCost = string.Empty,
                OtherDirectCosts = string.Empty,
                ProfitFeeComTotal = string.Empty,
                ProfitFeeTotal = string.Empty,
                ComTotal = string.Empty,
                ROSPercent = string.Empty,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistRowModelView row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row9 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment, PricerResponse = ChecklistResponseOption.NotSet };
            Collection<ChecklistRowModelView> checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row9);

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = string.Empty
            };

            ChecklistRowModelView row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "3", PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // should not be validated since SubmissionItem is null
            Collection<ChecklistRowModelView> checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                ChecklistVersion = 6
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId
            };

            // PPR responses/contents. 
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = row1.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = row2.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = row4.DisplayText, TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row1.PricerResponse };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row2.PricerResponse };
            ChecklistResponseItem response4 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row4.PricerResponse };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response4);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            var validationIssues = (from v in validationMessages
                                    select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ESTIMATING_SUBMITS_TO_CONTRACTS_DATE_REQUIRED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PPR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PRICER_COMMENT_NEEDED));
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_COMMENT_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_PAGE_NUMBER_NEEDED));
            
            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            proposal.IsCCPDRequired = false;
            fullProposal.IsCCPDRequired = false;

            // fix PPR validation, general validation, and proposal pricing but validate Pricer PAR now
            checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString(),
                PricerId = 14,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                SubmittedValue = "10"
            };

            checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = "22.12",
                LMLaborCost = "22",
                SubcontractorCost = "22",
                MaterialCost = "22",
                IWTACost = "22",
                TravelCost = "22",
                OtherDirectCosts = "22",
                ProfitFeeComTotal = "234",
                ProfitFeeTotal = "4",
                ComTotal = "2354",
                ROSPercent = "22.56",
                CostThroughCom = "22",
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment };
            checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row5);

            checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = pprPricerComment
            };

            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P1.1" };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No, PricerRowComment = "PAR Pricer Row Comment" };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.NotSet };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet };
            row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment, PeerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row12 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 7, SortOrder = 7, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // Should not be validated since SubmissionItem is null

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);
            checklistPARRows.Add(row12);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                PARPeerComment = string.Empty,
                ChecklistVersion = 6
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));

            // now fix everything on validate for a pricer
            validationMessages = new Collection<ValidationMessage>();
            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P1.1" };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No, PricerRowComment = "PAR Pricer Row Comment" };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P2.2" };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment };
            row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment };
            row12 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 7, SortOrder = 7, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // Should not be validated since SubmissionItem is null

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);
            checklistPARRows.Add(row12);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = parPricerComment,
                PARPeerComment = string.Empty
            };

            // validate checklist on the save, should succeed
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);
            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// C_ValidateChecklistTest_SubmitAsPricer_PTM
        /// Validate checklist test. This test will validate on the submit by a pricer
        /// This checklist validation uses the PTM set of logic to validate row responses and ChecklistVersion >= 9
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), TestMethod]
        public void C_ValidateChecklistTest_SubmitAsPricer_PTM()
        {
            var sut = this.CreateSystem();
            int proposalId = 5;
            int checklistID = 9;
            string pprPricerComment = "ppr Pricer Comment";
            string parPricerComment = "par Pricer Comment";

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = string.Empty,
                PricerId = 15,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                DeliverChecklistDFARS = true
            };

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = string.Empty,
                LMLaborCost = string.Empty,
                SubcontractorCost = string.Empty,
                MaterialCost = string.Empty,
                IWTACost = string.Empty,
                TravelCost = string.Empty,
                OtherDirectCosts = string.Empty,
                ProfitFeeComTotal = string.Empty,
                ProfitFeeTotal = string.Empty,
                ComTotal = string.Empty,
                ROSPercent = string.Empty,
                CostThroughCom = string.Empty,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistRowModelView row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row9 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment, PricerResponse = ChecklistResponseOption.NotSet };
            Collection<ChecklistRowModelView> checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row9);

            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = string.Empty
            };

            ChecklistRowModelView row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes };
            ChecklistRowModelView row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No };
            ChecklistRowModelView row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "3", PricerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // should not be validated since SubmissionItem is null
            Collection<ChecklistRowModelView> checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);

            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                ChecklistVersion = 9
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                IsCCPDRequired = true
            };

            // PPR responses/contents. 
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = row1.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = row2.DisplayText, TextType = ChecklistTextType.Question };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = row4.DisplayText, TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row1.PricerResponse };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row2.PricerResponse };
            ChecklistResponseItem response4 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = row4.PricerResponse };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response4);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 9,
                Content = new Collection<ChecklistContentItem> { content1, content2, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            var validationIssues = (from v in validationMessages
                                    select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ESTIMATING_SUBMITS_TO_CONTRACTS_DATE_REQUIRED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PPR_MUST_BE_ANSWERED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PRICER_COMMENT_NEEDED));
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_COMMENT_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_PAGE_NUMBER_NEEDED));

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            // now test with CCPDRequired = No.  This should bypass all the PPR and PAR checklist validation.
            proposal.IsCCPDRequired = false;
            fullProposal.IsCCPDRequired = false;

            // validate checklist on the save, will fail, but there should be no PPR or PAR checklist errors
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ESTIMATING_SUBMITS_TO_CONTRACTS_DATE_REQUIRED));

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            // fix PPR validation, general validation, and proposal pricing but validate Pricer PAR now
            proposal.IsCCPDRequired = true;
            fullProposal.IsCCPDRequired = true;
            checklistGeneralInfo = new ChecklistGeneralInformationModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 05, 02).ToShortDateString(),
                PricerId = 14,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                SubmittedValue = "10"
            };
            
            checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = "22.12",
                LMLaborCost = "22",
                SubcontractorCost = "22",
                MaterialCost = "22",
                IWTACost = "22",
                TravelCost = "22",
                OtherDirectCosts = "22",
                ProfitFeeComTotal = "234",
                ProfitFeeTotal = "6875",
                ComTotal = "41",
                ROSPercent = "22.56",
                CostThroughCom = "22",
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            row1 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.Yes };
            row2 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row3 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row4 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.No };
            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Comment is", RowType = ChecklistTextType.PricerComment };
            checklistPPRRows = new Collection<ChecklistRowModelView>();
            checklistPPRRows.Add(row1);
            checklistPPRRows.Add(row2);
            checklistPPRRows.Add(row3);
            checklistPPRRows.Add(row4);
            checklistPPRRows.Add(row5);

            checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ProposalChecklistID = checklistID,
                ChecklistRows = checklistPPRRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PPRPricerComment = pprPricerComment
            };

            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P1.1" };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No, PricerRowComment = "PAR Pricer Row Comment" };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.NotSet };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment, PeerResponse = ChecklistResponseOption.NotSet };
            row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment, PeerResponse = ChecklistResponseOption.NotSet };
            ChecklistRowModelView row12 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 7, SortOrder = 7, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // Should not be validated since SubmissionItem is null

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);
            checklistPARRows.Add(row12);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = string.Empty,
                PARPeerComment = string.Empty,
                ChecklistVersion = 9
            };
            checklistGeneralInfo.IsPTMChecklistUIEnabled = sut.IsPTMChecklistUIEnabled(checklistPARData.ChecklistVersion);

            // validate checklist on the save, will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsTrue(validationMessages.Any());
            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            // now test with CCPDRequired = No.  This should bypass all the PPR and PAR checklist validation.
            proposal.IsCCPDRequired = false;
            fullProposal.IsCCPDRequired = false;

            // validate checklist on the save, will succeed
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);

            Assert.IsFalse(validationMessages.Any());

            // clear out existing validation messages
            validationMessages = new Collection<ValidationMessage>();

            // now fix everything on validate for a pricer
            proposal.IsCCPDRequired = true;
            fullProposal.IsCCPDRequired = true;
            validationMessages = new Collection<ValidationMessage>();
            row5 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 1, SortOrder = 1, DisplayText = "Are you a banana?", RowType = ChecklistTextType.Question, SubmissionItem = "1", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P1.1" };
            row6 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 2, SortOrder = 2, DisplayText = "Are you an orange?", RowType = ChecklistTextType.Question, SubmissionItem = "2", PricerResponse = ChecklistResponseOption.No, PricerRowComment = "PAR Pricer Row Comment" };
            row7 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 3, SortOrder = 3, DisplayText = "I'm a vegetable", RowType = ChecklistTextType.Text, PricerResponse = ChecklistResponseOption.NotSet };
            row8 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 4, SortOrder = 4, DisplayText = "Are you a carrot?", RowType = ChecklistTextType.Question, SubmissionItem = "4", PricerResponse = ChecklistResponseOption.Yes, PricerPageNumber = "P2.2" };
            row10 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 5, SortOrder = 5, DisplayText = "comment is", RowType = ChecklistTextType.PricerComment };
            row11 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 6, SortOrder = 6, DisplayText = "comment is", RowType = ChecklistTextType.PeerComment };
            row12 = new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 7, SortOrder = 7, DisplayText = "Reserved?", RowType = ChecklistTextType.Question, PricerResponse = ChecklistResponseOption.NotSet }; // Should not be validated since SubmissionItem is null

            checklistPARRows = new Collection<ChecklistRowModelView>();
            checklistPARRows.Add(row5);
            checklistPARRows.Add(row6);
            checklistPARRows.Add(row7);
            checklistPARRows.Add(row8);
            checklistPARRows.Add(row10);
            checklistPARRows.Add(row11);
            checklistPARRows.Add(row12);

            checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView()
            {
                ProposalID = proposalId,
                ChecklistRows = checklistPARRows,
                ShowChecklistResponse = ShowChecklistResponse.Pricer,
                PARPricerComment = parPricerComment,
                PARPeerComment = string.Empty
            };

            // validate checklist on the save, should succeed
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);
            Assert.IsFalse(validationMessages.Any());

            // Test validation of canned responses in versions >= 12
            validationMessages = new Collection<ValidationMessage>();
            checklistPARData.ChecklistVersion = ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION;
            checklistPARData.ChecklistRows = new Collection<ChecklistRowModelView>();
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 8, SortOrder = 8, DisplayText = "Is Yes Required?", RowType = ChecklistTextType.Question, SubmissionItem = "8", YesOnly = true, PricerResponse = ChecklistResponseOption.No });
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 9, SortOrder = 9, DisplayText = "Is a Canned Response required for No?", RowType = ChecklistTextType.Question, SubmissionItem = "9", YesOnly = false, PricerResponse = ChecklistResponseOption.No, SelectedCannedResponse = null });
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 10, SortOrder = 10, DisplayText = "Is a comment required for Other?", RowType = ChecklistTextType.Question, SubmissionItem = "10", YesOnly = false, PricerResponse = ChecklistResponseOption.No, SelectedCannedResponse = ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID, PricerRowComment = string.Empty }); // todo set correct other id
            
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);
            Assert.IsTrue(validationMessages.Any());
            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_YES_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_CANNED_RESPONSE_NEEDED));
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.PAR_ROW_OTHER_COMMENT_NEEDED));

            // Test validation of canned responses where all valid
            validationMessages = new Collection<ValidationMessage>();
            checklistPARData.ChecklistRows = new Collection<ChecklistRowModelView>();
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 8, SortOrder = 8, DisplayText = "Is Yes Required?", RowType = ChecklistTextType.Question, SubmissionItem = "8", YesOnly = true, PricerResponse = ChecklistResponseOption.Yes });
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 9, SortOrder = 9, DisplayText = "Is a Canned Response required for No?", RowType = ChecklistTextType.Question, SubmissionItem = "9", YesOnly = false, PricerResponse = ChecklistResponseOption.No, SelectedCannedResponse = 2 });
            checklistPARData.ChecklistRows.Add(new ChecklistRowModelView() { ColumnOrder = 1, ChecklistContentId = 10, SortOrder = 10, DisplayText = "Is a comment required for Other?", RowType = ChecklistTextType.Question, SubmissionItem = "10", YesOnly = false, PricerResponse = ChecklistResponseOption.No, SelectedCannedResponse = ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID, PricerRowComment = "Yes, comment required for other" }); // todo set correct other id

            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, true, validationMessages);
            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// Test ValidateChecklist for fields related to the Cost Through COM changes
        /// </summary>
        [TestMethod]
        public void ValidateChecklistTest_CostThroughCOM()
        {
            ChecklistControllerLogic sut = this.CreateSystem();
            int proposalId = 1;
            int checklistID = 1;

            ChecklistProposalPricingDataModelView checklistProposalPricingData = new ChecklistProposalPricingDataModelView()
            {
                ProposalChecklistID = checklistID,
                ProposalID = proposalId,
                UpdateDate = DateTime.Now,
                LMLaborHrs = string.Empty,
                LMLaborCost = string.Empty,
                SubcontractorCost = string.Empty,
                MaterialCost = string.Empty,
                IWTACost = string.Empty,
                TravelCost = string.Empty,
                OtherDirectCosts = string.Empty,
                ProfitFeeComTotal = string.Empty,
                ProfitFeeTotal = string.Empty,
                ComTotal = string.Empty,
                ROSPercent = string.Empty,
                CostThroughCom = string.Empty,
                UseCostThroughCom = true,
                ShowChecklistResponse = ShowChecklistResponse.Pricer
            };

            ChecklistGeneralInformationModelView checklistGeneralInfo = new ChecklistGeneralInformationModelView() { ProposalID = proposalId };
            ChecklistProposalPricingReviewDocumentModelView checklistPPRData = new ChecklistProposalPricingReviewDocumentModelView() { ProposalID = proposalId };
            ChecklistProposalAdequacyReviewDocumentModelView checklistPARData = new ChecklistProposalAdequacyReviewDocumentModelView() { ProposalID = proposalId };
            ProposalDto proposal = new ProposalDto() { Id = proposalId };

            ChecklistContentDto checklistContent = new ChecklistContentDto();
            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto();
            ICollection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // assert that when Cost Through COM, Profit/Fee, and Submitted Value are empty/null, validation will pass
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false, validationMessages);

            IList<string> validationIssues = (from v in validationMessages
                                    select v.ValidationIssue).ToList();
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.COST_THRU_COM_PROFIT_FEE_SUM));

            checklistProposalPricingData.CostThroughCom = "1,000";
            checklistProposalPricingData.ProfitFeeTotal = "2,000";
            checklistGeneralInfo.SubmittedValue = "3,000";

            // assert that when Cost Through COM, Profit/Fee, and Submitted Value have values and CTC+PF=SV validation will pass
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false, validationMessages);

            validationIssues = (from v in validationMessages
                                              select v.ValidationIssue).ToList();
            Assert.IsFalse(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.COST_THRU_COM_PROFIT_FEE_SUM));

            checklistGeneralInfo.SubmittedValue = "3,001";

            // assert that when Cost Through COM, Profit/Fee, and Submitted Value have values and CTC+PF!=SV validation will fail
            sut.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRData, checklistPARData, false, validationMessages);

            validationIssues = (from v in validationMessages
                                select v.ValidationIssue).ToList();
            Assert.IsTrue(validationIssues.Contains(ValidationConstants.ChecklistValidationConstants.COST_THRU_COM_PROFIT_FEE_SUM));
        }

        /// <summary>
        /// Proposal checklist read only test
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void C_IsProposalChecklistReadOnlyTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            PtmRole outRole;

            ProposalDto proposal = new ProposalDto { Id = proposalId, ProposalStatus = ProposalStatus.InProgress };
            // General Read permission
            FullProposal fullProposal = new FullProposal(proposal);
            Collection<ProposalChecklistSaveInfo> checklistSaveInfo = new Collection<ProposalChecklistSaveInfo>();
            ProposalChecklistSaveInfo pricerPPRSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalPricingReview, Comment = "ab", SubmitDate = null, ResponseType = ChecklistResponseType.Pricer };
            ProposalChecklistSaveInfo pricerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "ab", SubmitDate = null, ResponseType = ChecklistResponseType.Pricer };
            ProposalChecklistSaveInfo peerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "ab", SubmitDate = null, ResponseType = ChecklistResponseType.Peer };
            checklistSaveInfo.Add(pricerPARSaveInfo);
            checklistSaveInfo.Add(peerPARSaveInfo);
            checklistSaveInfo.Add(pricerPPRSaveInfo);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.Read);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            string readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);

            // CreateReadUpdateDelete permission

            // check CRUD Permission for peer role. even though they have CRUD, all sections are still read only to them on the display
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(true);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);

            // check CRUD permission for backup pricer. they can edit all sections of the checklist
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(true);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("false", readOnly);

            // check CRUD permission for pricer. they can edit all sections of the checklist
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(true);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("false", readOnly);

            // Verify read Only for Pricer if they have submitted but Peer hasn't
            checklistSaveInfo = new Collection<ProposalChecklistSaveInfo>();
            pricerPPRSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalPricingReview, Comment = "submitted", SubmitDate = new DateTime(2014, 11, 11), ResponseType = ChecklistResponseType.Pricer };
            pricerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "submitted", SubmitDate = new DateTime(2014, 11, 11), ResponseType = ChecklistResponseType.Pricer };
            peerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "not it", SubmitDate = null, ResponseType = ChecklistResponseType.Peer };
            checklistSaveInfo.Add(pricerPARSaveInfo);
            checklistSaveInfo.Add(peerPARSaveInfo);
            checklistSaveInfo.Add(pricerPPRSaveInfo);

            fullProposal = new FullProposal(proposal);
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);

            // Verify Backup Pricer is readonly after submission.
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(true);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(false);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);

            // Verify it is still read only for Peer.
            checklistSaveInfo = new Collection<ProposalChecklistSaveInfo>();
            pricerPPRSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalPricingReview, Comment = "not it", SubmitDate = null, ResponseType = ChecklistResponseType.Pricer };
            pricerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "not it", SubmitDate = null, ResponseType = ChecklistResponseType.Pricer };
            peerPARSaveInfo = new ProposalChecklistSaveInfo { ChecklistType = ChecklistType.ProposalAdequacyReview, Comment = "submitted", SubmitDate = DateTime.Now, ResponseType = ChecklistResponseType.Peer };
            checklistSaveInfo.Add(pricerPARSaveInfo);
            checklistSaveInfo.Add(peerPARSaveInfo);
            checklistSaveInfo.Add(pricerPPRSaveInfo);

            fullProposal = new FullProposal(proposal);
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.BackupPricer, proposalId)).Returns(false);
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId)).Returns(true);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(checklistSaveInfo);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);

            // Verify read only if proposal is submitted
            proposal.ProposalStatus = ProposalStatus.Completed;
            fullProposal = new FullProposal(proposal);
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            readOnly = sut.IsProposalChecklistReadOnly(fullProposal);
            Assert.AreEqual("true", readOnly);
        }

        /// <summary>
        /// should PPR or PAR sections be displayed (pre-PTM checklist version) test
        ///     Checklist Version 1, CCPD = Yes, PPR Q1 = Yes       Expected Results => DisplayPPR=true, DisplayPAR=false
        ///     Checklist Version 1, CCPD = Yes, PPR Q1 = No        Expected Results => DisplayPPR=true, DisplayPAR=true
        ///     Checklist Version 1, CCPD = Yes, PPR Q1 = Not Set   Expected Results => DisplayPPR=true, DisplayPAR=true
        /// </summary>
        [TestMethod]
        public void C_ShouldChecklisSectionsBeDisplayed_PrePTMChecklist_CCPD_Yes_Test()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            int checklistID = 33;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 6,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                IsCCPDRequired = true   // test with CCPDRequired answer being set to "Yes"
            };

            // PPR responses/contents. // Test with Pricer's Question 1 answer being set to Yes
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Yes, this means that PAR data shouldn't be displayed
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            bool displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            bool displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, false);

            // Test with Pricer's Question 1 answer being set to No

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to No, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Test with Pricer's Question 1 answer being set to Not Set (default value in DB)

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Not Set, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Since PPR's Question 1 can never be set to NA, no test for it
        }

        /// <summary>
        /// should PPR or PAR sections be displayed (pre-PTM checklist version) test
        ///     Checklist Version 1, CCPD = No, PPR Q1 = Yes       Expected Results => DisplayPPR=true, DisplayPAR=false
        ///     Checklist Version 1, CCPD = No, PPR Q1 = No        Expected Results => DisplayPPR=true, DisplayPAR=true
        ///     Checklist Version 1, CCPD = No, PPR Q1 = Not Set   Expected Results => DisplayPPR=true, DisplayPAR=true
        /// </summary>
        [TestMethod]
        public void C_ShouldChecklisSectionsBeDisplayed_PrePTMChecklist_CCPD_No_Test()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            int checklistID = 33;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 6,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                IsCCPDRequired = false   // test with CCPDRequired answer being set to "No"
            };

            // PPR responses/contents. // Test with Pricer's Question 1 answer being set to Yes
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Yes, this means that PAR data shouldn't be displayed
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                AbsoluteValue = 1500,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            bool displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            bool displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, false);

            // Test with Pricer's Question 1 answer being set to No

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to No, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Test with Pricer's Question 1 answer being set to Not Set (default value in DB)

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Not Set, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Since PPR's Question 1 can never be set to NA, no test for it
        }

        /// <summary>
        /// should PPR or PAR sections be displayed (PTM checklist version) test
        ///     Checklist Version 9, CCPD = Yes, PPR Q1 = Yes       Expected Results => DisplayPPR=true, DisplayPAR=true
        ///     Checklist Version 9, CCPD = Yes, PPR Q1 = No        Expected Results => DisplayPPR=true, DisplayPAR=true
        ///     Checklist Version 9, CCPD = Yes, PPR Q1 = Not Set   Expected Results => DisplayPPR=true, DisplayPAR=true
        /// </summary>
        [TestMethod]
        public void C_ShouldChecklisSectionsBeDisplayed_PTMChecklist_CCPD_Yes_Test()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            int checklistID = 33;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 6,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                IsCCPDRequired = true   // test with CCPDRequired answer being set to "Yes"
            };

            // PPR responses/contents. // Test with Pricer's Question 1 answer being set to Yes
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Yes, this means that PAR data shouldn't be displayed
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            var checklistChangeToPTMtVersion = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.CHECKLIST_CHANGE_TO_PTM_VERSION]);
            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                AbsoluteValue = 1500,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            bool displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            bool displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Test with Pricer's Question 1 answer being set to No

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to No, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);

            // Test with Pricer's Question 1 answer being set to Not Set (default value in DB)

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Not Set, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, true);
            Assert.AreEqual(displayPAR, true);
        }

        /// <summary>
        /// should PPR or PAR sections be displayed (PTM checklist version) test
        ///     Checklist Version 9, CCPD = No, PPR Q1 = Yes       Expected Results => DisplayPPR=false, DisplayPAR=false
        ///     Checklist Version 9, CCPD = No, PPR Q1 = No        Expected Results => DisplayPPR=false, DisplayPAR=false
        ///     Checklist Version 9, CCPD = No, PPR Q1 = Not Set   Expected Results => DisplayPPR=false, DisplayPAR=false
        /// </summary>
        [TestMethod]
        public void C_ShouldChecklisSectionsBeDisplayed_PTMChecklist_CCPD_No_Test()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            int checklistID = 33;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 6,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now,
                IsCCPDRequired = false   // test with CCPDRequired answer being set to "No"
            };

            // PPR responses/contents. // Test with Pricer's Question 1 answer being set to Yes
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            ChecklistContentItem content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Yes, this means that PAR data shouldn't be displayed
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            var checklistChangeToPTMtVersion = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.CHECKLIST_CHANGE_TO_PTM_VERSION]);
            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                ProfitFeeWithCom = 7777,
                AbsoluteValue = 1500,
                ROSPercentage = 98989
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            bool displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            bool displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, false);
            Assert.AreEqual(displayPAR, false);

            // Test with Pricer's Question 1 answer being set to No

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to No, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, false);
            Assert.AreEqual(displayPAR, false);

            // Test with Pricer's Question 1 answer being set to Not Set (default value in DB)

            content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Price Comments", TextType = ChecklistTextType.PricerComment };
            content4 = new ChecklistContentItem { Id = 4, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 4, Text = "Can you say boo?", TextType = ChecklistTextType.Question };

            responses = new Collection<ChecklistResponseItem>();
            response1 = new ChecklistResponseItem { ChecklistContentId = content1.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            // set pricer comment to Not Set, this means that PAR data should be displayed
            response2 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.NotSet };
            response3 = new ChecklistResponseItem { ChecklistContentId = content4.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            responses.Add(response1);
            responses.Add(response2);
            responses.Add(response3);

            checklistContent = new ChecklistContentDto()
            {
                Version = checklistChangeToPTMtVersion,
                Content = new Collection<ChecklistContentItem> { content1, content2, content3, content4 }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            displayPPR = sut.ShouldChecklistPPRSectionBeDisplayed(fullProposal);
            displayPAR = sut.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposal);

            Assert.AreEqual(displayPPR, false);
            Assert.AreEqual(displayPAR, false);
        }

        /// <summary>
        /// test GetPPRQuestion1ContentId
        /// </summary>
        [TestMethod]
        public void C_GetPPRQuestion1ContentIdTest()
        {
            var sut = this.CreateSystem();
            int proposalId = 33;
            int checklistId = 4300;
            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            FullProposal fullProposal = new FullProposal(proposal);

            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Did you do that?", TextType = ChecklistTextType.Question };

            ChecklistContentDto pprchecklistcontent = new ChecklistContentDto { Content = new Collection<ChecklistContentItem> { content1, content2, content3 } };

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(pprchecklistcontent);

            int? contentID = sut.GetPPRQuestion1ContentId(proposalId, fullProposal);
            Assert.IsTrue(contentID.HasValue);
            Assert.AreEqual(contentID.Value, content2.Id);
        }

        /// <summary>
        /// test GetPPRQuestion1Response
        /// </summary>
        [TestMethod]
        public void C_GetPPRQuestion1ResponseTest()
        {
            var sut = this.CreateSystem();
            int proposalId = 33;
            int checklistId = 4300;
            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            FullProposal fullProposal = new FullProposal(proposal);

            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistId, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Did you do that?", TextType = ChecklistTextType.Question };

            Collection<ChecklistResponseItem> responses = new Collection<ChecklistResponseItem>();
            ChecklistResponseItem response1 = new ChecklistResponseItem { ChecklistContentId = content2.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.Yes };
            ChecklistResponseItem response2 = new ChecklistResponseItem { ChecklistContentId = content3.Id, ResponseType = ChecklistResponseType.Pricer, ProposalId = proposalId, Response = ChecklistResponseOption.No };
            responses.Add(response1);
            responses.Add(response2);
            ChecklistContentDto pprchecklistcontent = new ChecklistContentDto { Version = 1, Content = new Collection<ChecklistContentItem> { content1, content2, content3 } };

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(pprchecklistcontent);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposalId)).Returns(responses);

            ChecklistResponseOption response = sut.GetPPRQuestion1Response(proposalId, fullProposal);
            Assert.AreEqual(response, response1.Response);
            Assert.AreEqual(response, ChecklistResponseOption.Yes);
        }

        /// <summary>
        /// test GetChecklistVersion
        /// </summary>
        [TestMethod]
        public void C_GetChecklistVersionTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;
            int checklistID = 7;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                DeliveryDate = new DateTime(2014, 1, 1),
                BoeTool = BOETool.Excel,
                ContractTypeIds = new List<int> { 1, 2, 3 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                ProgramAreaId = 3,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                ProposalType = 3,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                UpdateDate = DateTime.Now
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                Id = checklistID,
                ProposalID = proposal.Id,
                UpdateDate = DateTime.Now,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22,
                AbsoluteValue = 1500,
                ProfitFeeWithCom = 7777,
                ROSPercentage = 98989
            };

            proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview,
                new ProposalChecklistSaveInfo { UserID = 22, LastSaveDate = new DateTime(2014, 5, 2), SubmitDate = null, Comment = "Because I said so" });

            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(new Collection<ProposalChecklistSaveInfo>());

            // get checklist version
            ChecklistIndexModelView checklistVersion = sut.GetChecklistVersion(proposalId);
            Assert.AreEqual(checklistContent.Version, checklistVersion.Version);
            Assert.IsTrue(checklistVersion.ShouldDisplayChecklist);
            Assert.IsFalse(checklistVersion.IsPricer);

            // verify checklist not displayed when version=0
            checklistContent.Version = 0;
            checklistVersion = sut.GetChecklistVersion(proposalId);
            Assert.IsFalse(checklistVersion.ShouldDisplayChecklist);

            // verify IsPricer flag
            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposalId)).Returns(true);
            checklistVersion = sut.GetChecklistVersion(proposalId);
            Assert.IsTrue(checklistVersion.IsPricer);
        }

        /// <summary>
        /// Test the PopulatePARChecklistSSRSParameters functions which calculates the argument string for the SSRS report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2241:Provide correct arguments to formatting methods"), TestMethod]
        public void PopulatePARChecklistSSRSParametersTest()
        {
            var sut = this.CreateSystem();

            PARChecklistModelView model = new PARChecklistModelView()
            {
                ProposalID = 555,
                ChecklistVersion = 101
            };

            string reportServerLocation = WebConfigurationManager.AppSettings["ReportServerLocation"];
            string folderName = WebConfigurationManager.AppSettings["ReportServerFolderName"];
            string title = "PAR Checklist Report";
            StringBuilder sb = new StringBuilder();
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);
            sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_ID, model.ProposalID));
            sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_ADEQUACY_REVIEW_ID, model.ChecklistVersion));
            string expected = string.Format("{0}/{1}/{2}{3}", reportServerLocation, folderName, title, sb);

            Uri uri = sut.PopulatePARChecklistSSRSParameters(model);
            Assert.AreEqual(expected, uri.ToString());
        }

        #region Exception Tests
        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveChecklist_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.SaveChecklist(null, new ChecklistProposalPricingDataModelView(), new ChecklistProposalPricingReviewDocumentModelView(), new ChecklistProposalAdequacyReviewDocumentModelView(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveChecklist_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.SaveChecklist(new ChecklistGeneralInformationModelView(), null, new ChecklistProposalPricingReviewDocumentModelView(), new ChecklistProposalAdequacyReviewDocumentModelView(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveChecklist_ExceptionTest3()
        {
            var sut = this.CreateSystem();
            sut.SaveChecklist(new ChecklistGeneralInformationModelView(), new ChecklistProposalPricingDataModelView(), null, new ChecklistProposalAdequacyReviewDocumentModelView(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveChecklist_ExceptionTest4()
        {
            var sut = this.CreateSystem();
            sut.SaveChecklist(new ChecklistGeneralInformationModelView(), new ChecklistProposalPricingDataModelView(), new ChecklistProposalPricingReviewDocumentModelView(), null, false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateChecklist_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.ValidateChecklist(null, new ChecklistProposalPricingDataModelView(), new ChecklistProposalPricingReviewDocumentModelView(), new ChecklistProposalAdequacyReviewDocumentModelView(), false, new Collection<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateChecklist_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.ValidateChecklist(new ChecklistGeneralInformationModelView(), null, new ChecklistProposalPricingReviewDocumentModelView(), new ChecklistProposalAdequacyReviewDocumentModelView(), false, new Collection<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateChecklist_ExceptionTest3()
        {
            var sut = this.CreateSystem();
            sut.ValidateChecklist(new ChecklistGeneralInformationModelView(), new ChecklistProposalPricingDataModelView(), null, new ChecklistProposalAdequacyReviewDocumentModelView(), false, new Collection<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateChecklist_ExceptionTest4()
        {
            var sut = this.CreateSystem();
            sut.ValidateChecklist(new ChecklistGeneralInformationModelView(), new ChecklistProposalPricingDataModelView(), new ChecklistProposalPricingReviewDocumentModelView(), null, false, new Collection<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateChecklist_ExceptionTest5()
        {
            var sut = this.CreateSystem();
            sut.ValidateChecklist(new ChecklistGeneralInformationModelView(), new ChecklistProposalPricingDataModelView(), new ChecklistProposalPricingReviewDocumentModelView(), new ChecklistProposalAdequacyReviewDocumentModelView(), false, null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_PopulatePARChecklistSSRSParameters_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.PopulatePARChecklistSSRSParameters(null);
        }

        #endregion Exception Tests
    }
}
