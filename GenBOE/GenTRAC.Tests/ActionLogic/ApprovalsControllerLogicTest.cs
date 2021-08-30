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
    using System.Linq;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test teh ApprovalsControllerLogic class
    /// </summary>
    [TestClass]
    public class ApprovalsControllerLogicTest
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
        /// User Mapper
        /// </summary>
        private Mock<IUserMapper> userMapper = null;

        /// <summary>
        /// User Loader
        /// </summary>
        private Mock<IUserLoader> userLoader = null;

        /// <summary>
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory = null;

        /// <summary>
        /// Emailer
        /// </summary>
        private Mock<IPtmEmailer> emailer = null;

        /// <summary>
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator = null;

        /// <summary>
        /// Proposal Checklist Loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader = null;

        /// <summary>
        /// Checklist Mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator = null;

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<ApprovalsLoader> approvalsLoader = null;

        /// <summary>
        /// The approval emailer.
        /// </summary>
        private Mock<ApprovalEmailer> approvalEmailer = null;

        /// <summary>
        /// The attachment loader
        /// </summary>
        private Mock<IAttachmentLoader> attachmentLoader = null;

        /// <summary>
        /// Active Director Utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtils = null;

        /// <summary>
        /// Creates ApprovalsControllerLogic for testing
        /// </summary>
        /// <returns>ApprovalsControllerLogic</returns>
        private ApprovalsControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.userMapper = new Mock<IUserMapper>();
            this.userLoader = new Mock<IUserLoader>();
            this.emailer = new Mock<IPtmEmailer>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.approvalsLoader = new Mock<ApprovalsLoader>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.approvalEmailer = new Mock<ApprovalEmailer>();
            this.approvalEmailer.Setup(x => x.SendEmails(It.IsAny<int?>()));
            this.attachmentLoader = new Mock<IAttachmentLoader>();
            this.attachmentLoader.Setup(x => x.AllRequiredAttachmentsHaveBeenUploaded(It.IsAny<int>()));
            this.adUtils = new Mock<IActiveDirectoryUtilities>();
            
            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            return new ApprovalsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object, this.userLoader.Object, 
                this.emailer.Object, this.objectFactory.Object, this.approvalsLoader.Object, this.proposalMediator.Object, this.proposalChecklistLoader.Object, 
                this.checklistMediator.Object, this.approvalEmailer.Object, this.attachmentLoader.Object, this.adUtils.Object);
        }
        #endregion

        /// <summary>
        /// Tests GetApprovals method
        /// </summary>
        [TestMethod]
        public void GetApprovalsTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalsIndexModelView expected = new ApprovalsIndexModelView();
            ApprovalsIndexModelView actual = sut.GetApprovals(proposal.Id);

            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.MissingRole, actual.MissingRole);
            Assert.AreEqual("Test Name", actual.LeadEstimatorName);
        }

        /// <summary>
        /// Tests GetApprovals method when a role is missing
        /// </summary>
        [TestMethod]
        public void GetApprovalsTest_MissingRole()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            FullProposal fullProposal = new FullProposal(proposal);

            fullProposal.Permissions.Clear();
                
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalsIndexModelView expected = new ApprovalsIndexModelView();
            expected.MissingRole = true;
            ApprovalsIndexModelView actual = sut.GetApprovals(proposal.Id);

            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.MissingRole, actual.MissingRole);
        }

        /// <summary>
        /// Tests GetApprovalModel method for lead estimator
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_LeadEstimator()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.LeadEstimatorSignedDate;
            expected.Comments = proposal.LeadEstimatorSignatureComment;
            expected.IsReadOnly = true;
            expected.ApproverRole = PtmRole.Pricer;
            expected.IsNoBid = false;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.Pricer);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
            Assert.AreEqual(expected.IsNoBid, actual.IsNoBid);
        }

        /// <summary>
        /// Tests GetApprovalModel method for lead estimator
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_LeadEstimator_NoBid()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            // make no bid
            proposal.ProposalStatus = ProposalStatus.NoBid;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.LeadEstimatorSignedDate;
            expected.Comments = proposal.LeadEstimatorSignatureComment;
            expected.IsReadOnly = true;
            expected.ApproverRole = PtmRole.Pricer;
            expected.IsNoBid = true;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.Pricer);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
            Assert.AreEqual(expected.IsNoBid, actual.IsNoBid);
        }

        /// <summary>
        /// Tests GetApprovalModel method for cover sheet approver
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_CoverSheetApprover()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.CoverSheetApproverSignedDate;
            expected.Comments = proposal.CoverSheetApproverSignatureComment;
            expected.IsReadOnly = true;
            expected.ApproverRole = PtmRole.CoverSheetApprover;
            expected.IsNoBid = false;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.CoverSheetApprover);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
        }

        /// <summary>
        /// Tests GetApprovalModel method for cover sheet approver when section should not be available
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_CoverSheetApprover_NotAvailable()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.IsCCPDRequired = true;

            // Not available if Lead Estimator has not approved
            proposal.LeadEstimatorSignedDate = null;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView result = sut.GetApprovalModel(proposal.Id, PtmRole.CoverSheetApprover);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests GetApprovalModel method for cover sheet approver when section should not be available
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_CoverSheetApprover_NotAvailable2()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(new Collection<ProposalPermissionDto>());

            proposal.IsCCPDRequired = false;

            // Not available if CCPD required is set to false
            proposal.LeadEstimatorSignedDate = DateTime.Now;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView result = sut.GetApprovalModel(proposal.Id, PtmRole.CoverSheetApprover);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests GetApprovalModel method for pricing verification
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_PricingVerification()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.PricingVerifierSignedDate;
            expected.Comments = proposal.PricingVerifierSignatureComment;
            expected.IsReadOnly = true;
            expected.ApproverRole = PtmRole.PricingVerification;
            expected.IsNoBid = false;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.PricingVerification);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
            Assert.AreEqual(expected.IsNoBid, actual.IsNoBid);
        }

        /// <summary>
        /// Tests GetApprovalModel method for pricing verification when section should not be available
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_PricingVerification_NotAvailable()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            // Not available if Lead Estimator has not approved
            proposal.LeadEstimatorSignedDate = null;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView result = sut.GetApprovalModel(proposal.Id, PtmRole.PricingVerification);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests GetApprovalModel method for independent reviewer
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_IndependentReviewer()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.IndependentReviewerSignedDate;
            expected.Comments = proposal.IndependentReviewerSignatureComment;
            expected.IsReadOnly = true;
            expected.ApproverRole = PtmRole.PeerReviewer;
            expected.IsNoBid = false;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.PeerReviewer);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
            Assert.AreEqual(expected.IsNoBid, actual.IsNoBid);
        }

        /// <summary>
        /// Tests GetApprovalModel method for independent reviewer when section should not be available
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_IndependentReviewer_NotAvailable()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            // Not available if Lead Estimator has not approved
            proposal.LeadEstimatorSignedDate = null;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView result = sut.GetApprovalModel(proposal.Id, PtmRole.PeerReviewer);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests GetApprovalModel method for LOB estimating lead
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_LOBEstLead()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            UserDTO user = this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            ApprovalSectionModelView expected = new ApprovalSectionModelView();
            expected.ApproverName = user.DisplayName;
            expected.DateOfApproval = proposal.LOBEstimatingLeadSignedDate;
            expected.Comments = proposal.LOBEstimatingLeadSignatureComment;
            expected.IsReadOnly = false;
            expected.ApproverRole = PtmRole.LOBEstLead;
            expected.IsNoBid = false;

            ApprovalSectionModelView actual = sut.GetApprovalModel(proposal.Id, PtmRole.LOBEstLead);

            Assert.AreEqual(expected.ApproverName, actual.ApproverName);
            Assert.AreEqual(expected.DateOfApproval, actual.DateOfApproval);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual(expected.ApproverRole, actual.ApproverRole);
            Assert.AreEqual(expected.ApproverHeader, actual.ApproverHeader);
            Assert.AreEqual(expected.SignatureString, actual.SignatureString);
            Assert.AreEqual(expected.IsNoBid, actual.IsNoBid);
        }

        /// <summary>
        /// Tests GetApprovalModel method for LOB estimating lead when section should not be available
        /// </summary>
        [TestMethod]
        public void GetApprovalModelTest_LOBEstLead_NotAvailable()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            this.SetupUserForGetApprovalModelTest();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            // Not available if Lead Estimator has not approved
            proposal.LeadEstimatorSignedDate = null;
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ApprovalSectionModelView result = sut.GetApprovalModel(proposal.Id, PtmRole.LOBEstLead);

            // Not available if Cover Sheet Approver has not approved
            proposal.LeadEstimatorSignedDate = DateTime.Today;
            proposal.CoverSheetApproverSignedDate = null;
            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            result = sut.GetApprovalModel(proposal.Id, PtmRole.LOBEstLead);

            // Not available if Pricing Verifier has not approved
            proposal.CoverSheetApproverSignedDate = DateTime.Today;
            proposal.PricingVerifierSignedDate = null;
            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            result = sut.GetApprovalModel(proposal.Id, PtmRole.LOBEstLead);

            // Not available if Independent Reviewer is required and has not approved
            proposal.PricingVerifierSignedDate = DateTime.Today;
            proposal.IndependentReviewerSignedDate = null;
            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            result = sut.GetApprovalModel(proposal.Id, PtmRole.LOBEstLead);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Sets up the User DTO and related methods
        /// </summary>
        /// <returns>User DTO</returns>
        private UserDTO SetupUserForGetApprovalModelTest()
        {
            UserDTO user = new UserDTO()
            {
                DisplayName = "Test Name",
                Ntid = "testid"
            };

            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(user);
            this.retriever.Setup(x => x.GetCurrentUser()).Returns(user);

            return user;
        }

        /// <summary>
        /// Sets up the proposal and related methods
        /// </summary>
        /// <returns>Proposal DTO</returns>
        private ProposalDto SetupProposalForGetApprovalModelTest()
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

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>() { leadEstimator, coverSheetApprover, pricingVerification, independentReviewer, lobEstimatingLead };

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                LeadEstimatorSignedDate = DateTime.Today,
                LeadEstimatorSignatureComment = "LE Comment",
                CoverSheetApproverSignedDate = DateTime.Today.AddDays(1),
                CoverSheetApproverSignatureComment = "CSA Comment",
                PricingVerifierSignedDate = DateTime.Today.AddDays(2),
                PricingVerifierSignatureComment = "PV Comment",
                IndependentReviewerSignedDate = DateTime.Today.AddDays(3),
                IndependentReviewerSignatureComment = "IR Comment",
                LOBEstimatingLeadSignedDate = null,
                LOBEstimatingLeadSignatureComment = string.Empty
            };

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(permissions);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            return proposal;
        }

        /// <summary>
        /// Test SaveApproval method
        /// </summary>
        [TestMethod]
        public void SaveApprovalTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1
            };

            FullProposal fullProposal = new FullProposal(proposal);

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>()
            {
                new ProposalPermissionDto()
                {
                    Id = 1,
                    ProposalID = 1,
                    Role = PtmRole.Pricer,
                    UserId = 2
                },
                new ProposalPermissionDto()
                {
                    Id = 2,
                    ProposalID = 1,
                    Role = PtmRole.CoverSheetApprover,
                    UserId = 3
                }
            };

            ChecklistContentDto pprChecklistContent = new ChecklistContentDto();
            pprChecklistContent.Content = new Collection<ChecklistContentItem>();
            pprChecklistContent.Content.Add(new ChecklistContentItem()
            {
                Id = 1,
                TextType = ChecklistTextType.Question,
                SortOrder = 1
            });

            bool pricerSavedWhilePeerEditing;

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalChecklistLoader.Setup(x => x.GetPPRResponses(proposal.Id)).Returns(new Collection<ChecklistResponseItem>());
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetProposalPermissions(proposal.Id)).Returns(permissions);
            this.retriever.Setup(x => x.GetPARChecklistContent(It.IsAny<int>())).Returns(new ChecklistContentDto());
            this.retriever.Setup(x => x.GetPPRChecklistContent(It.IsAny<int>())).Returns(pprChecklistContent);
            this.userMapper.Setup(x => x.GetById(2)).Returns(new UserDTO() { Ntid = "test1" });
            this.userMapper.Setup(x => x.GetById(3)).Returns(new UserDTO() { Ntid = "test2" });

            string testComment = "Test comment";

            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>())).Verifiable();

            sut.SaveApproval(proposal.Id, PtmRole.CoverSheetApprover, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Once());
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Never());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Never());

            sut.SaveApproval(proposal.Id, PtmRole.PricingVerification, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Exactly(2));
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Never());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Never());

            sut.SaveApproval(proposal.Id, PtmRole.PeerReviewer, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Exactly(3));
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Never());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Never());

            Assert.AreEqual(WorkflowStatus.AllApproved, fullProposal.WorkflowStatus);
            sut.SaveApproval(proposal.Id, PtmRole.LOBEstLead, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Exactly(4));
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Once());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Once());
        }

        /// <summary>
        /// Test SaveApproval method
        /// </summary>
        [TestMethod]
        public void SaveApprovalTest_CCOPD_False()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                IsCCPDRequired = false
            };

            FullProposal fullProposal = new FullProposal(proposal);
            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>()
            {
                new ProposalPermissionDto()
                {
                    Id = 1,
                    ProposalID = 1,
                    Role = PtmRole.Pricer,
                    UserId = 2
                }
            };

            bool pricerSavedWhilePeerEditing;

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetProposalPermissions(proposal.Id)).Returns(permissions);
            this.userMapper.Setup(x => x.GetById(2)).Returns(new UserDTO() { Ntid = "test1" });
            this.userMapper.Setup(x => x.GetById(3)).Returns(new UserDTO() { Ntid = "test2" });

            string testComment = "Test comment";

            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>())).Verifiable();

            sut.SaveApproval(proposal.Id, PtmRole.PricingVerification, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Once());
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Never());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Never());

            sut.SaveApproval(proposal.Id, PtmRole.PeerReviewer, testComment, string.Empty, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);

            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Exactly(2));
            this.checklistMediator.Verify(x => x.SaveChecklistProposal(It.IsAny<ProposalChecklistDto>()), Times.Never());
            this.proposalMediator.Verify(x => x.SaveProposalStatus(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<ProposalStatus>()), Times.Never());

            Assert.AreEqual(WorkflowStatus.AllApproved, fullProposal.WorkflowStatus);
        }

        /// <summary>
        /// Tests for proper method return for a Deleted proposal.
        /// </summary>
        [TestMethod]
        public void IsProposalDeletedOrArchived_DeletedTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.ProposalStatus = ProposalStatus.Deleted;

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            Assert.IsTrue(sut.IsProposalDeletedOrArchived(proposal.Id));
        }

        /// <summary>
        /// Tests for proper method return for an Archived proposal.
        /// </summary>
        [TestMethod]
        public void IsProposalDeletedOrArchived_ArchivedTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.ProposalStatus = ProposalStatus.Archived;

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            Assert.IsTrue(sut.IsProposalDeletedOrArchived(proposal.Id));
        }

        /// <summary>
        /// Tests for proper method return for an InProgress proposal.
        /// </summary>
        [TestMethod]
        public void IsProposalDeletedOrArchived_InProgressTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.ProposalStatus = ProposalStatus.InProgress;

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            Assert.IsFalse(sut.IsProposalDeletedOrArchived(proposal.Id));
        }

        /// <summary>
        /// Tests for proper method return for a Completed proposal.
        /// </summary>
        [TestMethod]
        public void IsProposalDeletedOrArchived_CompletedTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.ProposalStatus = ProposalStatus.Completed;

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            Assert.IsFalse(sut.IsProposalDeletedOrArchived(proposal.Id));
        }

        /// <summary>
        /// Tests for proper method return for a Revision proposal.
        /// </summary>
        [TestMethod]
        public void IsProposalDeletedOrArchived_RevisedTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();
            proposal.ProposalStatus = ProposalStatus.Revised;

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            Assert.IsFalse(sut.IsProposalDeletedOrArchived(proposal.Id));
        }

        /// <summary>
        /// Test ValidateCoverSheetApprover for a valid Cover Sheet Approver
        /// </summary>
        [TestMethod]
        public void ValidateCoverSheetApprover_ValidTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(new UserDTO() { Ntid = "testuser" });
            this.adUtils.Setup(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();
            sut.ValidateCoverSheetApprover(proposal.Id, validationMessages);

            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Test ValidateCoverSheetApprover for an invalid Cover Sheet Approver
        /// </summary>
        [TestMethod]
        public void ValidateCoverSheetApprover_InvalidTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            ProposalDto proposal = this.SetupProposalForGetApprovalModelTest();

            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(new UserDTO() { Ntid = "testuser" });
            this.adUtils.Setup(x => x.IsMemberOfADGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();
            sut.ValidateCoverSheetApprover(proposal.Id, validationMessages);

            Assert.AreEqual(1, validationMessages.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.COVER_SHEET_APPROVER_NO_LONGER_APPROVED, validationMessages.First().ValidationIssue);
        }

        /// <summary>
        /// Test that ValidateCoverSheetApprover throws an exception for null validation messages parameter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateCoverSheetApprover_ExceptionTest()
        {
            ApprovalsControllerLogic sut = this.CreateSystem();
            sut.ValidateCoverSheetApprover(1, null);
        }
    }
}
