// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security.Principal;
    using System.Web.Mvc;
    using GenBOE.DataBridge.DTO;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the Proposal Controller Logic class
    /// </summary>
    [TestClass]
    public class ProposalControllerLogicTest
    {
        #region Properties

        /// <summary>
        /// Security Access
        /// </summary>
        private Mock<ISecurityAccess> securityAccess = null;

        /// <summary>
        /// Validation Methods
        /// </summary>
        private Mock<IValidationMethods> validationMethods = null;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private Mock<IProposalLoader> proposalLoader = null;

        /// <summary>
        /// proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator = null;

        /// <summary>
        /// AD Utils
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
        /// org structure data mapper
        /// </summary>
        private Mock<IOrgStructureDataMapper> orgStructureDataMapper = null;

        /// <summary>
        /// proposal permission mediator
        /// </summary>
        private Mock<IProposalPermissionMediator> proposalPermissionMediator = null;

        /// <summary>
        /// Security information.
        /// </summary>
        private Mock<IES.Common.ISecurityInformation> securityInformation = null;

        /// <summary>
        /// Cache.
        /// </summary>
        private Mock<IES.Common.ICacheDataLoader> cacheDataLoader = null;

        /// <summary>
        /// Pick List Mapper
        /// </summary>
        private Mock<IPickListMapper> pickListMapper = null;

        /// <summary>
        /// user loader
        /// </summary>
        private Mock<IUserLoader> userLoader = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<IApprovalsLoader> approvalsLoader;

        /// <summary>
        /// Proposal Checklist Loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader;

        /// <summary>
        /// Checklist Mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator;

        /// <summary>
        /// The workspace loader
        /// </summary>
        private Mock<IWorkspaceDTODataLoader> workspaceLoader;

        /// <summary>
        /// The GenBOE permissions loader
        /// </summary>
        private Mock<IPermissionsDTODataLoader> genBoePermissionsLoader;

        /// <summary>
        /// PTM Emailer
        /// </summary>
        private Mock<IPtmEmailer> ptmEmailer;

        #endregion

        /// <summary>
        /// Stubs
        /// </summary>
        private class Stubs
        {
            /// <summary>
            /// ProposalInformationModelView Stub
            /// </summary>
            public ProposalInformationModelView ProposalInformationVM
            {
                get
                {
                    return new ProposalInformationModelView()
                    {
                        AnticipatedDeliveryDate = DateTime.Now.AddDays(100).ToString("MM/dd/yyyy"),
                        RevisedSubmittalDate = "01/01/2014",
                        RFPIssuedDate = "01/01/2014",
                        RFPReceivedDate = "01/01/2014",
                        IsScheduleProposal = false,
                        EstimatedProposalValue = "100",
                        ContractType = new Collection<int> { 1, 2 },
                        CostElements = new Collection<int> { 1 },
                        RFPNumber = "rfp",
                        ISGSRole = ISGSRole.Prime,
                        RequestType = 0
                    };
                }
            }

            /// <summary>
            /// ProposalUserInformationModelView Stub
            /// </summary>
            public ProposalUserInformationModelView ProposalUserInformationVM
            {
                get
                {
                    return new ProposalUserInformationModelView()
                    {
                        CaptureManagerNtid = "n00000",
                        CostVolumeLeadNtid = "n00000",
                        ContractsPOCNtId = "n00000",
                        ProposalMgrNtid = "n00000",
                        SupplyChainPOCMaterialsNtId = "n00000",
                        SupplyChainPOCSubsNtId = "n00000"
                    };
                }
            }

                /// <summary>
                /// ProposalApprovalModelView Stub
                /// </summary>
            public ProposalApprovalsModelView ProposalApprovalsVM
            {
                get
                {
                    return new ProposalApprovalsModelView()
                    {
                        LeadEstimatorNtid = "n00000",
                        CoverSheetApproverNtid = "n00000",
                        PricingVerificationNtid = "n00011",
                        IndependentReviewerNtid = "n000012",
                        LOBEstimatingLeadMgrNtid = "n000000"
                    };
                }
            }
        }

        /// <summary>
        /// Proposal Controller logic under test
        /// </summary>
        /// <returns>home controller logic</returns>
        private ProposalControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.validationMethods = new Mock<IValidationMethods>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.adUtils = new Mock<IES.Common.IActiveDirectoryUtilities>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.orgStructureDataMapper = new Mock<IOrgStructureDataMapper>();
            this.proposalPermissionMediator = new Mock<IProposalPermissionMediator>();
            this.securityInformation = new Mock<IES.Common.ISecurityInformation>();
            this.cacheDataLoader = new Mock<IES.Common.ICacheDataLoader>();

            this.pickListMapper = new Mock<IPickListMapper>();
            this.userLoader = new Mock<IUserLoader>();
            this.pickListMapper.Setup(x => x.GetSelectListPickList(It.IsAny<PickListEnum>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<SelectListItem>());
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.genBoePermissionsLoader = new Mock<IPermissionsDTODataLoader>();
            this.ptmEmailer = new Mock<IPtmEmailer>();

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            Mock<IES.Common.SecurityInformation> security = new Mock<IES.Common.SecurityInformation>(this.adUtils.Object, new Mock<IES.Common.ICache>().Object);
            IES.Common.UserData user = new IES.Common.UserData()
            {
                Email = "james.basilio@lmco.com"
            };
            security.Setup(x => x.ActiveUserData).Returns(user);

            return new ProposalControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.validationMethods.Object,
                this.proposalMediator.Object, this.userMapper.Object, this.objectFactory.Object,
                this.orgStructureDataMapper.Object, this.proposalPermissionMediator.Object, this.securityInformation.Object, this.cacheDataLoader.Object,
                this.pickListMapper.Object, this.userLoader.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, 
                this.checklistMediator.Object, this.workspaceLoader.Object, this.genBoePermissionsLoader.Object, this.ptmEmailer.Object);
        }

        /// <summary>
        /// Test save new proposal
        /// </summary>
        [TestMethod]
        public void C_SaveNewProposalTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 25;
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalID = proposalId.Value,
                AnticipatedDeliveryDate = "01/01/2014",
                RevisedSubmittalDate = null,
                RFPIssuedDate = "01/01/2014",
                RFPReceivedDate = "01/01/2014",
                IsScheduleProposal = false,
                EstimatedProposalValue = "100",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalGeneralInformationModelView proposalGeneralInfo = new ProposalGeneralInformationModelView()
            {
                ProgramArea = "1",
                LineOfBusiness = "1",
                IsCostVolumeClassified = false
            };

            // setup all 9 user ids
            string ntid = "myNtid";
            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = ntid
            };

            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource2NtId = ntid,
                BackupPricerNtId = ntid,
                CaptureManagerNtid = ntid,
                ContractsPOCNtId = ntid,
                CostVolumeLeadNtid = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                SupplyChainPOCSubsNtId = ntid
            };

            ProposalCommentsModelView proposalComments = new ProposalCommentsModelView()
            {
                Comments = "test"
            };

            UserDTO user = new UserDTO()
            {
                Id = 1
            };

            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>())).Returns(proposalId);
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(user);
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);

            int? savedProposalId = sut.SaveProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, proposalComments, null);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.proposalPermissionMediator.Verify(x => x.SaveProposalPermissionDtos(It.IsAny<ICollection<ProposalPermissionDto>>()), Times.Once());
        }
        
        /// <summary>
        /// Test Save New Proposal when saving a new Revision
        /// </summary>
        [TestMethod]
        public void SaveNewProposalTest_NewRevision()
        {
            var sut = this.CreateSystem();

            int? proposalId = 1;
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalID = proposalId.Value,
                ProposalTrackingNumber = "20-00001-PR1",
                ProposalTitle = "New Revision Test-PR1",
                AnticipatedDeliveryDate = "01/01/2020",
                RevisedSubmittalDate = null,
                RFPIssuedDate = "01/01/2020",
                RFPReceivedDate = "01/01/2020",
                IsScheduleProposal = false,
                EstimatedProposalValue = "100",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalGeneralInformationModelView proposalGeneralInfo = new ProposalGeneralInformationModelView()
            {
                ProgramArea = "1",
                LineOfBusiness = "1",
                IsCostVolumeClassified = false
            };

            // setup all 9 user ids
            string ntid = "test";
            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = ntid
            };

            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource2NtId = ntid,
                BackupPricerNtId = ntid,
                CaptureManagerNtid = ntid,
                ContractsPOCNtId = ntid,
                CostVolumeLeadNtid = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                SupplyChainPOCSubsNtId = ntid
            };

            ProposalCommentsModelView proposalComments = new ProposalCommentsModelView()
            {
                Comments = "test"
            };

            UserDTO user = new UserDTO()
            {
                Id = 1
            };

            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>())).Returns(proposalId);
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(user);
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);

            int? savedProposalId = sut.SaveProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, proposalComments, proposalId.Value);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.proposalPermissionMediator.Verify(x => x.SaveProposalPermissionDtos(It.IsAny<ICollection<ProposalPermissionDto>>()), Times.Once());
        }

        /// <summary>
        /// Test Delete Proposal
        /// </summary>
        [TestMethod]
        public void TestDeleteProposal()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto();

            this.proposalLoader.Setup(x => x.Save(It.IsAny<ProposalDto>())).Returns(1);
            this.proposalLoader.Setup(x => x.Save(It.IsAny<ProposalDto>())).Verifiable();

            sut.DeleteProposal(proposal);

            // set to deleted after the call to make sure it was set when we verify
            proposal.Updateable = UpdateType.Deleted;

            this.proposalLoader.Verify(x => x.Save(proposal), Times.Once());
        }

        /// <summary>
        /// Test Delete Proposal throws an exception when proposal is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDeleteProposal_Exception()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            sut.DeleteProposal(null);
        }

        /// <summary>
        /// Validates a new proposal, with empty required lists
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalEmptyListsTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>(),
                CostElements = new List<int>(),
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), 
                new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(2, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Contract Type")).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Elements of Cost")).Count());

            validationMessages.Clear();
            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(),
                new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, true);

            Assert.AreEqual(1, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Contract Type")).Count());
        }

        /// <summary>
        /// Test save new proposal, with UserInfo user NTIDs
        /// </summary>
        [TestMethod]
        public void C_SaveNewProposalWithUserInfoNtIdsTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 25;
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalID = proposalId.Value,
                AnticipatedDeliveryDate = "01/01/2014",
                RevisedSubmittalDate = "01/01/2014",
                RFPIssuedDate = "01/01/2014",
                RFPReceivedDate = "01/01/2014",
                IsScheduleProposal = false,
                EstimatedProposalValue = "100",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalGeneralInformationModelView proposalGeneralInfo = new ProposalGeneralInformationModelView()
            {
                ProgramArea = "1",
                LineOfBusiness = "1"
            };
                        
            // setup all 9 user ids
            string ntid = "myNtid";
            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = ntid
            };

            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource2NtId = ntid,
                BackupPricerNtId = ntid,
                CaptureManagerNtid = ntid,
                ContractsPOCNtId = ntid,
                CostVolumeLeadNtid = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                SupplyChainPOCSubsNtId = ntid
            };

            ProposalCommentsModelView proposalComments = new ProposalCommentsModelView()
            {
                Comments = "test"
            };

            UserDTO user = new UserDTO()
            {
                Id = 1,
                UserType = IES.Common.UserType.User
            };

            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>())).Returns(proposalId);
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(user);
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);

            int? savedProposalId = sut.SaveProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, proposalComments, null);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.proposalPermissionMediator.Verify(x => x.SaveProposalPermissionDtos(It.IsAny<ICollection<ProposalPermissionDto>>()), Times.Once());
        }

        /// <summary>
        /// Test save new proposal, with UserInfo user NTIDs validation
        /// </summary>
        [TestMethod]
        public void C_SaveNewProposalWithUserInfoNtIdsValidationTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            // setup all 9 user ids
            string ntid = "xyz";

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource1Type = ResourceType.Pricer,
                AdditionalPricingResource2NtId = ntid,
                AdditionalPricingResource2Type = ResourceType.Pricer,
                BackupPricerNtId = ntid,
                CaptureManagerNtid = ntid,
                ContractsPOCNtId = ntid,
                CostVolumeLeadNtid = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                SupplyChainPOCSubsNtId = ntid
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, proposalUserInfo, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Validates a new proposal, with format failures
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalFormatsTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "bad",
                RevisedSubmittalDate = "bad",
                RFPIssuedDate = "bad",
                RFPReceivedDate = "bad",
                IsScheduleProposal = true,
                RFPNumber = "rfp",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(4, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.REVISED_SUBMITTAL_DATE_FORMAT)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.ANTICIPATED_DELIVERY_DATE_FORMAT)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.RFP_ISSUED_DATE_FORMAT)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.RFP_RECEIVED_DATE_FORMAT)).Count());
            
            validationMessages.Clear();
            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, true);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.ANTICIPATED_DELIVERY_DATE_FORMAT)).Count());
        }

        /// <summary>
        /// This test verifies the constraint that each proposal title must be unique.
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalUniqueProposalConstraintViolated()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "ProposalTitle",
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = DateTime.Now.AddDays(100).ToString("MM/dd/yyyy"),
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(false);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(1, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.PROPOSAL_TITLE_MUST_BE_UNIQUE)).Count());

            validationMessages.Clear();
            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, true);

            Assert.AreEqual(1, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.PROPOSAL_TITLE_MUST_BE_UNIQUE)).Count());
        }

        /// <summary>
        /// This test verifies the constraint that each proposal title must be unique.
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalUniqueProposalConstraintNotViolated()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "ProposalTitle",
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// This test confirms that this constraint does not apply when the Proposal Tite is null
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalUniqueProposalConstraintWithNullProposalTitle()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            // Set Proposal title equal to the already created proposal's title
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = null,
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// This test confirms the validation constraint that the Lead Estimator and Peer Reviewer cannot be the same person.
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerConstraintViolated()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            int proposalId = 5;

            // Ensure that the proposal title is unique to satisfy the associated validation constraint
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle",
                ProposalID = proposalId,
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe",
                IndependentReviewerNtid = "jdoe"
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            int pricerId = 5;

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = pricerId
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            // De-register the retriever used by FullProposal when getting permissions
            this.retriever = null;

            // Define the permissions that contain the Peer Reviewer and associated id
            ProposalPermissionDto permissionsDto = new ProposalPermissionDto()
            {
                UserId = pricerId,
                Role = PtmRole.PeerReviewer
            };

            // Set the retriever that's used by FullProposal to get permissions
            this.retriever = new Mock<IRetriever>();
            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(new List<ProposalPermissionDto> { permissionsDto });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test does not result in the validation constraint message since the Pricer and Peer Reviewer are different.
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerConstraintNotViolated()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            int proposalId = 5;

            // Ensure that the proposal title is unique to satisfy the associated validation constraint
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle",
                ProposalID = proposalId,
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe"
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            int pricerId = 5;

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = pricerId
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            // De-register the retriever used by FullProposal when getting permissions
            this.retriever = null;

            int peerReviewerId = 100; // arbitrary number different than pricer id

            // Define the permissions that contain the Peer Reviewer and associated id
            ProposalPermissionDto permissionsDto = new ProposalPermissionDto()
            {
                Id = peerReviewerId,
                Role = PtmRole.PeerReviewer
            };

            // Set the retriever that's used by FullProposal to get permissions
            this.retriever = new Mock<IRetriever>();
            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(new List<ProposalPermissionDto> { permissionsDto });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test verifies that this constraint does not apply when creating a new proposal without specifying
        /// a Pricer. 
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerConstraintWithNullProposalPricer()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle"
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe"
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns((UserDTO)null);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
        }
        
        /// <summary>
        /// Tests IndepedentReviewer Validation
        /// </summary>
        [TestMethod]
        public void C_ValidateIndependentReviewer()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalUserInformationModelView proposalUserInfoVM = new Stubs().ProposalUserInformationVM;
            ProposalInformationModelView proposalInfoVM = new Stubs().ProposalInformationVM;
            ProposalApprovalsModelView proposalApprovalsMV = new Stubs().ProposalApprovalsVM;
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            // CASE: if CoverSheetApproverNtid is same as LeadEstimatorNtid, IndependentReviewerNtid is required
            validationMessages = new List<ValidationMessage>();
            proposalApprovalsMV.CoverSheetApproverNtid = "n00001";
            proposalApprovalsMV.LeadEstimatorNtid = "n00001";
            proposalApprovalsMV.IndependentReviewerNtid = string.Empty;
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue( validationMessages.First().ValidationIssue.ContainsEquivalent("Independent Reviewer is Required") );
            validationMessages = new List<ValidationMessage>();
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, true);
            Assert.AreEqual(0, validationMessages.Count);

            // CASE: if CoverSheetApproverNtid is different from LeadEstimatorNtid, IndependentReviewerNtid is NOT required
            validationMessages = new List<ValidationMessage>();
            proposalApprovalsMV.CoverSheetApproverNtid = "n00001";
            proposalApprovalsMV.LeadEstimatorNtid = "n00002";
            proposalApprovalsMV.IndependentReviewerNtid = string.Empty;
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);

            // CASE: if CoverSheetApproverNtid is same as LeadEstimatorNtid, IndependentReviewerNtid is required, and IndependentReviewerNtid is set
            validationMessages = new List<ValidationMessage>();
            proposalApprovalsMV.LeadEstimatorNtid = "n00001";
            proposalApprovalsMV.CoverSheetApproverNtid = "n00001";
            proposalApprovalsMV.IndependentReviewerNtid = "n00002";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);

            // CASE: LeadEstimatorNtid is not allowed to be same as IndependentReviewerNtid
            validationMessages = new List<ValidationMessage>();
            proposalApprovalsMV.LeadEstimatorNtid = "n00001";
            proposalApprovalsMV.CoverSheetApproverNtid = "n00001";
            proposalApprovalsMV.IndependentReviewerNtid = "n00001";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
        }

        /// <summary>
        /// Tests PricingVerification Validation
        /// </summary>
        [TestMethod]
        public void C_ValidatePricingVerification()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalInformationModelView proposalInfoVM = new Stubs().ProposalInformationVM;
            List<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalApprovalsModelView proposalApprovalsMV = new Stubs().ProposalApprovalsVM;
            proposalApprovalsMV.LeadEstimatorNtid = "n00006";
            proposalApprovalsMV.CoverSheetApproverNtid = "n00001";
            proposalApprovalsMV.IndependentReviewerNtid = "n00002";
            ProposalUserInformationModelView proposalUserInfoVM = new Stubs().ProposalUserInformationVM;

            // CASE: Pricing verification not set
            proposalApprovalsMV.PricingVerificationNtid = string.Empty;
            
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
            validationMessages = new List<ValidationMessage>();
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, true);
            Assert.AreEqual(0, validationMessages.Count);
            validationMessages = new List<ValidationMessage>();

            // CASE: Pricing verification set
            proposalApprovalsMV.PricingVerificationNtid = "n00000";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);
            validationMessages = new List<ValidationMessage>();

            // CASE: LeadEstimatorNtid is not allowed to be same as PricingVerificationNtid
            proposalApprovalsMV.LeadEstimatorNtid = "n00000";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
        }

        /// <summary>
        /// Tests Materials Lead Validation
        /// </summary>
        [TestMethod]
        public void C_ValidateSupplyChainPOCMaterials()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalUserInformationModelView proposalUserInfoVM = new Stubs().ProposalUserInformationVM;
            ProposalInformationModelView proposalInfoVM = new Stubs().ProposalInformationVM;
            ProposalApprovalsModelView proposalApprovalsMV = new Stubs().ProposalApprovalsVM;
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            // CASE: Materials lead not set
            validationMessages = new List<ValidationMessage>();
            proposalInfoVM.CostElements = new Collection<int> { (int)CostElementType.Materials };
            proposalUserInfoVM.SupplyChainPOCMaterialsNtId = string.Empty;
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
            Assert.IsTrue(validationMessages.First().ValidationIssue.ContainsEquivalent("Materials Lead is Required"));
            validationMessages = new List<ValidationMessage>();
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, true);
            Assert.AreEqual(0, validationMessages.Count);

            // CASE: Materials lead is set
            validationMessages = new List<ValidationMessage>();
            proposalInfoVM.CostElements = new Collection<int> { (int)CostElementType.Materials };
            proposalUserInfoVM.SupplyChainPOCMaterialsNtId = "n00000";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Tests IndepdnentReviewer Validation
        /// </summary>
        [TestMethod]
        public void C_ValidateSupplyChainPOCSubs()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalUserInformationModelView proposalUserInfoVM = new Stubs().ProposalUserInformationVM;
            ProposalInformationModelView proposalInfoVM = new Stubs().ProposalInformationVM;
            ProposalApprovalsModelView proposalApprovalsMV = new Stubs().ProposalApprovalsVM;
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            // CASE:  
            validationMessages = new List<ValidationMessage>();
            proposalInfoVM.CostElements = new Collection<int> { (int)CostElementType.Subs };
            proposalUserInfoVM.SupplyChainPOCSubsNtId = string.Empty;
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(1, validationMessages.Count);
            // Assert.IsTrue(validationMessages.First().ValidationIssue.ContainsEquivalent("Independent Reviewer is Required"));

            // CASE:  
            validationMessages = new List<ValidationMessage>();
            proposalInfoVM.CostElements = new Collection<int> { (int)CostElementType.Subs };
            proposalUserInfoVM.SupplyChainPOCSubsNtId = "n00000";
            sut.ValidateProposal(proposalInfoVM, new ProposalGeneralInformationModelView(), proposalApprovalsMV, proposalUserInfoVM, validationMessages, false);
            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// This test verifies that this constraint does not apply when creating a new proposal with null proposal permissions
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerConstraintWithNullPermissions()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe"
            };

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = 1 // arbitrary number not used since the permissions are set to null for this test
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            // De-register the retriever used by FullProposal when getting permissions
            this.retriever = null;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            // Set the retriever that's used by FullProposal to get permissions
            this.retriever = new Mock<IRetriever>();

            List<ProposalPermissionDto> nullList = null;

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(nullList);
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test verifies that this constraint does not apply when creating a new proposal without a full proposal.
        /// Also verifies that Lead estimator and LOB Mgr not being same does not trigger when it's blank
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerConstraintWithNullFullProposalDto()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle"
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe"
            };

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = 1 // arbitrary number not used since the permissions are set to null for this test
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            FullProposal nullFullProposal = null;

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(nullFullProposal);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_LOB_LEAD_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test verifies that the lead pricer and LOB mgr cannot be the same
        /// </summary>
        [TestMethod]
        public void _ValidatePricerAndLobManagerNotSame()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle",
                IsScheduleProposal = false
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe",
                LOBEstimatingLeadMgrNtid = "jdoe"
            };

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = 1 // arbitrary number not used since the permissions are set to null for this test
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            FullProposal nullFullProposal = null;

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(nullFullProposal);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_LOB_LEAD_CANNOT_BE_SAME_PERSON)).Count());
            validationMessages.Clear();
            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, true);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_LOB_LEAD_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test verifies that the constraint doesn't apply when there's no Peer Reviewer.
        /// </summary>
        [TestMethod]
        public void C_ValidatePricerAndPeerReviewerWithNoPeerReviewer()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            int proposalId = 5;

            // Ensure that the proposal title is unique to satisfy the associated validation constraint
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = "myTitle",
                ProposalID = proposalId,
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013"
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "jdoe"
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalInfo.ProposalID
            };

            int pricerId = 5;

            UserDTO userDto = new UserDTO()
            {
                Ntid = proposalApprovalsInfo.LeadEstimatorNtid,
                Id = pricerId
            };

            this.userMapper.Setup(x => x.GetByNtid(proposalApprovalsInfo.LeadEstimatorNtid)).Returns(userDto);

            // De-register the retriever used by FullProposal when getting permissions
            this.retriever = null;

            int peerReviewerId = 100; // arbitrary number different than pricer id

            // Define the permissions that contain the Peer Reviewer and associated id
            ProposalPermissionDto permissionsDto = new ProposalPermissionDto()
            {
                Id = peerReviewerId,
                Role = PtmRole.BackupPricer
            };

            // Set the retriever that's used by FullProposal to get permissions
            this.retriever = new Mock<IRetriever>();
            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(new List<ProposalPermissionDto> { permissionsDto });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalInfo.ProposalID)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), proposalApprovalsInfo, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalValidationConstants.LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON)).Count());
        }

        /// <summary>
        /// This test verifies that a user can update a proposal. When the unique proposal constraint was first coded,
        /// a validation constraint was raised since the current proposal's title already existed.
        /// 
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalUpdateProposal()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            string proposalTitle = "ProposalTitle";
            int proposalId = 5;

            // Set Proposal title equal to the already created proposal's title
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ProposalTitle = proposalTitle,
                ProposalID = proposalId,
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 1 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeid2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, new ProposalUserInformationModelView(), validationMessages, false);

            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Validates a new proposal, with conditional failures
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalConditionalFailuresTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>() { 1 },
                CostElements = new List<int>() { 3, 5 },
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                RFPIssuedDate = "11/11/2018",
                RFPReceivedDate = "11/11/2018",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            string ntid = "myNtid";
            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource1Type = ResourceType.NotSet,
                AdditionalPricingResource2NtId = ntid,
                AdditionalPricingResource2Type = ResourceType.NotSet
            };

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, proposalUserInfo, validationMessages, false);

            Assert.AreEqual(4, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Materials Lead")).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Subcontracts Lead is required")).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Additional Estimating Resource 1")).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains("Additional Estimating Resource 2")).Count());

            validationMessages.Clear();
            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, proposalUserInfo, validationMessages, true);

            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Validates a new proposal, success
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalSuccessTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>() { 3 },
                CostElements = new List<int>() { 5 },
                AnticipatedDeliveryDate = "01/01/2013",
                RevisedSubmittalDate = "01/01/2013",
                RFPIssuedDate = "01/01/2013",
                RFPReceivedDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            string ntid = "myNtid";
            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                SupplyChainPOCSubsNtId = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource1Type = ResourceType.Pricer,
                AdditionalPricingResource2NtId = ntid,
                AdditionalPricingResource2Type = ResourceType.Strategist
            };

            sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, proposalUserInfo, validationMessages, false);

            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// Validates a new proposal, anticipated delivery failure for Forecasted
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalForecastedFailureAnticipatedDeliveryTest()
        {
            var sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView()
            {
                ContractType = new List<int>() { 3 },
                CostElements = new List<int>() { 5 },
                AnticipatedDeliveryDate = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy"),
                RevisedSubmittalDate = "01/01/2013",
                RFPIssuedDate = "01/01/2013",
                RFPReceivedDate = "01/01/2013",
                IsScheduleProposal = false,
                RFPNumber = "rfp",
                ISGSRole = ISGSRole.Prime,
                RequestType = 0
            };

            string ntid = "myNtid";
            ProposalUserInformationModelView proposalUserInfo = new ProposalUserInformationModelView()
            {
                SupplyChainPOCSubsNtId = ntid,
                SupplyChainPOCMaterialsNtId = ntid,
                AdditionalPricingResource1NtId = ntid,
                AdditionalPricingResource1Type = ResourceType.Pricer,
                AdditionalPricingResource2NtId = ntid,
                AdditionalPricingResource2Type = ResourceType.Strategist
            };

           sut.ValidateProposal(proposalInfo, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView() { LeadEstimatorNtid = "fakeId2", CoverSheetApproverNtid = "fakeId", PricingVerificationNtid = "fakeId" }, proposalUserInfo, validationMessages, true);

            Assert.AreEqual(1, validationMessages.Count);
        }

        /// <summary>
        /// Get proposal information test
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void C_GetDataForProposalInformationTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                TrackingNumber = "myTrackingNumber",
                ProposalTitle = "myTitle",
                UpdateDate = DateTime.Now,
                DeliveryDate = new DateTime(2014, 1, 1),
                ContractTypeGroup = 1, // ContractTypeGroup.CP,
                ContractTypeIds = new List<int> { 1, 2 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                IsScheduleProposal = false,
                ProposalType = 2,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                RFPIssuedDate = new DateTime(2014, 2, 2),
                RFPReceivedDate = new DateTime(2014, 3, 3)
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.pickListMapper.Setup(x => x.IsPickListActive(It.IsAny<PickListEnum>(), It.IsAny<int>())).Returns(true);
            this.validationMethods.Setup(x => x.IsEnumActive(It.IsAny<CostElementType>())).Returns(true);
            this.pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.ProposalClass, 1, It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<SelectListItem>() { new SelectListItem() { Text = "a", Value = "a" } });
            this.pickListMapper.Setup(x => x.GetChildren(PickListEnum.ContractType, 1)).Returns(new List<PickListDto>
            {
                new PickListDto { Id = 1, Text = "CPAF", IsActive = true },
                new PickListDto { Id = 2, Text = "CPPF", IsActive = true },
                new PickListDto { Id = 3, Text = "CPIF", IsActive = true }
            });

            ProposalInformationModelView proposalInfo = sut.GetDataForProposalInformation(proposalId);

            Assert.IsTrue(proposalInfo.CostElementsList.Any());
            Assert.IsTrue(proposalInfo.CustomerTypesList.Any());
            Assert.IsTrue(proposalInfo.ISGSRolesList.Any());
            Assert.IsTrue(proposalInfo.ProposalClassesList.Any());

            // all ContractType values are active, first 2 are selected
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                1, "CPAF")));
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                2, "CPPF")));
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\">{1}</option>",
                3, "CPIF")));
            Assert.AreEqual(string.Empty, proposalInfo.InactiveContractTypes);

            // all CostElement values are active
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 1).Any());
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 2).Any());
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 3).Any());
            Assert.AreEqual(string.Empty, proposalInfo.InactiveCostElements);

            Assert.AreEqual(proposal.Id, proposalInfo.ProposalID);
            Assert.AreEqual(proposal.TrackingNumber, proposalInfo.ProposalTrackingNumber);
            Assert.AreEqual(proposal.ForecastedTrackingNumber, proposalInfo.ForecastedTrackingNumber);
            Assert.AreEqual(proposal.ProposalTitle, proposalInfo.ProposalTitle);
            Assert.AreEqual(proposal.UpdateDate, proposalInfo.UpdateDate);
            Assert.AreEqual(proposal.DeliveryDate.ToString("MM/dd/yyyy"), proposalInfo.AnticipatedDeliveryDate);
            if (proposal.RevisedSubmittalDate.HasValue)
            {
                Assert.AreEqual(proposal.RevisedSubmittalDate == null ? null : proposal.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy"), proposalInfo.RevisedSubmittalDate);
            }

            Assert.AreEqual(proposal.ContractTypeIds, proposalInfo.ContractType);
            Assert.AreEqual(proposal.CostElementTypeIds, proposalInfo.CostElements);
            Assert.AreEqual(proposal.Customer, proposalInfo.Customer);
            Assert.AreEqual(proposal.CustomerType, proposalInfo.CustomerType);
            Assert.AreEqual(proposal.EstimatedProposalValue, long.Parse(proposalInfo.EstimatedProposalValue));
            Assert.AreEqual(proposal.ISGSRole, proposalInfo.ISGSRole);
            Assert.AreEqual(proposal.IsScheduleProposal, proposalInfo.IsScheduleProposal);
            Assert.AreEqual(proposal.ProposalType, proposalInfo.ProposalType);
            Assert.AreEqual(proposal.Request, proposalInfo.RequestType);
            Assert.AreEqual(proposal.ProposalClass, proposalInfo.ProposalClass);
            Assert.AreEqual(proposal.RFPNumber, proposalInfo.RFPNumber);
            Assert.AreEqual(proposal.RFPIssuedDate.Value.ToString("MM/dd/yyyy"), proposalInfo.RFPIssuedDate);
            Assert.AreEqual(proposal.RFPReceivedDate.Value.ToString("MM/dd/yyyy"), proposalInfo.RFPReceivedDate);
            Assert.IsFalse(proposalInfo.IsNewRevision);

            // test null dates (should never happen with proposal created through UI)
            fullProposal.RFPIssuedDate = null;
            fullProposal.RFPReceivedDate = null;
            proposalInfo = sut.GetDataForProposalInformation(proposalId);
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RFPIssuedDate));
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RFPReceivedDate));

            // verify values for new proposal
            proposalInfo = sut.GetDataForProposalInformation(null);
            Assert.AreEqual(-1, proposalInfo.ProposalID);
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.AnticipatedDeliveryDate));
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RevisedSubmittalDate));
            Assert.AreEqual(0, proposalInfo.ContractTypeGroup);
            Assert.IsFalse(proposalInfo.ContractType.Any());
            Assert.IsFalse(proposalInfo.CostElements.Any());
            Assert.AreEqual(string.Empty, proposalInfo.ContractTypeHtmlOptions);
            Assert.AreEqual(string.Empty, proposalInfo.InactiveContractTypes);
            Assert.AreEqual(string.Empty, proposalInfo.InactiveCostElements);
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.Customer));
            Assert.AreEqual(CustomerType.NotSet, proposalInfo.CustomerType);
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.EstimatedProposalValue));
            Assert.AreEqual(ISGSRole.NotSet, proposalInfo.ISGSRole);
            Assert.IsNull(proposalInfo.IsScheduleProposal);
            Assert.AreEqual(0, proposalInfo.ProposalType);
            Assert.AreEqual(0, proposalInfo.RequestType);
            Assert.AreEqual(0, proposalInfo.ProposalClass);
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RFPNumber));
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RFPIssuedDate));
            Assert.IsTrue(string.IsNullOrEmpty(proposalInfo.RFPReceivedDate));
            Assert.AreEqual(DateTime.MinValue, proposalInfo.UpdateDate);

            // test behavior of inactive contract type / cost element type
            this.validationMethods.Setup(x => x.IsEnumActive((CostElementType)1)).Returns(false);
            this.validationMethods.Setup(x => x.IsEnumActive((CostElementType)2)).Returns(false);
            this.pickListMapper.Setup(x => x.GetChildren(PickListEnum.ContractType, 1)).Returns(new List<PickListDto>
            {
                new PickListDto { Id = 1, Text = "CPAF", IsActive = false },
                new PickListDto { Id = 2, Text = "CPPF", IsActive = true },
                new PickListDto { Id = 3, Text = "CPIF", IsActive = false }
            });
            proposalInfo = sut.GetDataForProposalInformation(proposalId);
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains("CPAF"));
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                2, "CPPF")));
            Assert.IsFalse(proposalInfo.ContractTypeHtmlOptions.Contains("CPIF"));
            Assert.IsFalse(proposalInfo.CostElementsList.Where(x => x.ID == 1).Any());
            Assert.IsFalse(proposalInfo.CostElementsList.Where(x => x.ID == 2).Any());
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 3).Any());
            Assert.IsTrue(proposalInfo.InactiveContractTypes.Contains("CPAF"));  // Inactive and selected
            Assert.IsFalse(proposalInfo.InactiveContractTypes.Contains("CPPF")); // Active and selected
            Assert.IsFalse(proposalInfo.InactiveContractTypes.Contains("CPIF"));  // Inactive, but not selected
            Assert.IsTrue(proposalInfo.InactiveCostElements.Contains(((CostElementType)1).GetDescription()));
            Assert.IsTrue(proposalInfo.InactiveCostElements.Contains(((CostElementType)2).GetDescription()));
            Assert.IsFalse(proposalInfo.InactiveCostElements.Contains(((CostElementType)3).GetDescription()));

            // now make proposal readonly - html will contain ContractType 1 (selected) but not 3 (not selected), and all 3 Cost Elements (all selected)
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            proposalInfo = sut.GetDataForProposalInformation(proposalId);
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                1, "CPAF")));
            Assert.IsTrue(proposalInfo.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                2, "CPPF")));
            Assert.IsFalse(proposalInfo.ContractTypeHtmlOptions.Contains("CPIF"));
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 1).Any());
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 2).Any());
            Assert.IsTrue(proposalInfo.CostElementsList.Where(x => x.ID == 3).Any());
            Assert.AreNotEqual(string.Empty, proposalInfo.InactiveContractTypes);
            Assert.AreEqual(string.Empty, proposalInfo.InactiveCostElements);

            // inactive contract type group selected, contract group list will not be empty
            fullProposal.ProposalStatus = ProposalStatus.InProgress;
            this.pickListMapper.Setup(x => x.IsPickListActive(PickListEnum.ContractTypeGroup, 1)).Returns(false);
            proposalInfo = sut.GetDataForProposalInformation(proposalId);
            Assert.AreNotEqual(string.Empty, proposalInfo.ContractTypeHtmlOptions);
            Assert.AreNotEqual(string.Empty, proposalInfo.InactiveContractTypes);

            // no contract type group selected, contract group list should be empty
            this.pickListMapper.Setup(x => x.GetChildren(PickListEnum.ContractType, 0)).Returns(new List<PickListDto>
            {
            });
            fullProposal.ContractTypeGroup = 0;
            proposalInfo = sut.GetDataForProposalInformation(proposalId);
            Assert.AreEqual(string.Empty, proposalInfo.ContractTypeHtmlOptions);
            Assert.AreEqual(string.Empty, proposalInfo.InactiveContractTypes);
        }

        /// <summary>
        /// Get proposal general information test
        /// </summary>
        [TestMethod]
        public void C_GetDataForProposalGeneralInformationTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                BoeTool = BOETool.Excel,
                ProgramAreaId = 1,
                OTISOpportunityID = "myOtisId",
                ProposalLocation = ProposalLocation.ValleyForgePA,
                PricingTool = PricingTool.Excel,
                LineOfBusinessID = 2,
                ProgramName = "myProgram",
                UpdateDate = DateTime.Now,
                ProgramProposalStatus = ProgramProposalStatus.LMRetainedMST,
                IsCCPDRequired = true,
                IsCostVolumeClassified = false
            };

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2,
                IsActive = true
            };

            SelectListItem lob = new SelectListItem
            {
                Text = lineOfBusiness.Text,
                Value = lineOfBusiness.Id.ToString()
            };

            PickListDto programArea = new PickListDto()
            {
                Text = "myShortProgramAreaName",
                Id = 1,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            ChecklistContentDto parData = new ChecklistContentDto()
            {
                ChecklistType = ProposalChecklistType.Default,
                Version = 6
            };

            string programAreaHtml = "html";
            string programAreaHelpText = "helptext";

            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId.Value)).Returns(parData);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });
            this.orgStructureDataMapper.Setup(x => x.GetLineOfBusinessById(lineOfBusiness.Id)).Returns(lineOfBusiness);
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea });
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaById(programArea.Id)).Returns(programArea);
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaHtmlOptionsForLineOfBusiness(lineOfBusiness.Id, programArea.Id, true)).Returns(programAreaHtml);
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaDynamicHelpText()).Returns(programAreaHelpText);
            this.pickListMapper.Setup(x => x.GetById(PickListEnum.LineOfBusiness, lineOfBusiness.Id)).Returns(lineOfBusiness);
            this.pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, It.IsAny<int?>(), true, false)).Returns(new List<SelectListItem> { lob });
            this.pickListMapper.Setup(x => x.GetById(PickListEnum.ProgramArea, programArea.Id)).Returns(programArea);

            ProposalGeneralInformationModelView proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, false);

            Assert.IsTrue(proposalGeneralInfo.BOEToolsList.Any());
            Assert.IsTrue(proposalGeneralInfo.ProposalLocationsList.Any());
            Assert.IsTrue(proposalGeneralInfo.LinesOfBusinessList.Any());
            Assert.IsFalse(string.IsNullOrEmpty(proposalGeneralInfo.ProgramAreaHtmlOptions));
            Assert.AreEqual(programAreaHelpText, proposalGeneralInfo.ProgramAreaHelpText);

            Assert.AreEqual(proposal.Id, proposalGeneralInfo.ProposalID);
            Assert.AreEqual(proposal.BoeTool, proposalGeneralInfo.BOETool);
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.BOEToolName));
            Assert.AreEqual(proposal.ProgramAreaId, int.Parse(proposalGeneralInfo.ProgramArea));
            Assert.AreEqual(proposal.ProposalLocation, proposalGeneralInfo.ProposalLocation);
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.ProposalLocationName));
            Assert.AreEqual(proposal.PricingTool, proposalGeneralInfo.PricingTool);
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.PricingToolName));
            Assert.AreEqual(proposal.LineOfBusinessID, int.Parse(proposalGeneralInfo.LineOfBusiness));
            Assert.AreEqual(proposal.ProgramName, proposalGeneralInfo.ProgramName);
            Assert.AreEqual(proposal.UpdateDate, proposalGeneralInfo.UpdateDate);
            Assert.AreEqual(proposal.OTISOpportunityID, proposalGeneralInfo.OTISOpportunityID);
            Assert.AreEqual(programAreaHtml, proposalGeneralInfo.ProgramAreaHtmlOptions);
            Assert.AreEqual(proposal.ProgramProposalStatus, proposalGeneralInfo.ProgramProposalStatus);
            Assert.AreEqual(proposal.IsCCPDRequired, proposalGeneralInfo.IsCCPDRequired);
            Assert.AreEqual(proposal.IsCostVolumeClassified, proposalGeneralInfo.IsCostVolumeClassified);

            Assert.AreEqual(lineOfBusiness.Text, proposalGeneralInfo.LineOfBusinessSelectedText);
            Assert.AreEqual(programArea.Text, proposalGeneralInfo.ProgramAreaSelectedText);

            // verify values for ProposalLocation, PricingTool, and BOETool Other
            proposal.ProposalLocation = ProposalLocation.Other;
            proposal.ProposalLocationName = "Las Vegas, Nevada";
            proposal.PricingTool = PricingTool.Other;
            proposal.PricingToolName = "Custom Pricing Tool";
            proposal.BoeTool = BOETool.Other;
            proposal.BoeToolName = "Custom BOE Tool";

            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, false);

            Assert.AreEqual(proposal.ProposalLocation, proposalGeneralInfo.ProposalLocation);
            Assert.AreEqual(proposal.ProposalLocationName, proposalGeneralInfo.ProposalLocationName);
            Assert.AreEqual(proposal.PricingTool, proposalGeneralInfo.PricingTool);
            Assert.AreEqual(proposal.PricingToolName, proposalGeneralInfo.PricingToolName);
            Assert.AreEqual(proposal.BoeTool, proposalGeneralInfo.BOETool);
            Assert.AreEqual(proposal.BoeToolName, proposalGeneralInfo.BOEToolName);

            // verify values for new proposal
            proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(null, false);
            Assert.AreEqual(-1, proposalGeneralInfo.ProposalID);
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.ProposalLocationName));
            Assert.AreEqual(BOETool.NotSet, proposalGeneralInfo.BOETool);
            Assert.IsTrue(string.IsNullOrEmpty( proposalGeneralInfo.BOEToolName));
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.ProgramArea));
            Assert.AreEqual(0, (int)proposalGeneralInfo.PricingTool);
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.PricingToolName));
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.LineOfBusiness));
            Assert.IsTrue(string.IsNullOrEmpty(proposalGeneralInfo.ProgramName));
            Assert.AreEqual(DateTime.MinValue, proposalGeneralInfo.UpdateDate);
            Assert.AreEqual(ProgramProposalStatus.LMRetainedSSC, proposalGeneralInfo.ProgramProposalStatus);
            Assert.IsNull(proposalGeneralInfo.IsCCPDRequired);
            Assert.IsNull(proposalGeneralInfo.IsCostVolumeClassified);

            // Assert CCoPD is cleared when getting data for a new revision
            proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, true);
            Assert.IsNull(proposalGeneralInfo.IsCCPDRequired);
            Assert.IsFalse(proposalGeneralInfo.IsCCPDReadOnly);
        }

        /// <summary>
        /// Get proposal user information test
        /// </summary>
        [TestMethod]
        public void C_GetDataForProposalUserInformationTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value
            };

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "myNtId",
                DisplayName = "myDisplayName"
            };

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>();
            for (int i = 1; i < 9; i++)
            {
                permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = (PtmRole)i, ProposalID = proposal.Id });
            }

            permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = PtmRole.BackupPricer, ProposalID = proposal.Id });
            permissions[(int)PtmRole.AdditionalPricingResource1 - 1].ResourceType = ResourceType.Pricer;
            permissions[(int)PtmRole.AdditionalPricingResource2 - 1].ResourceType = ResourceType.Strategist;
            permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = PtmRole.GenBoeWorkspaceCreator, ProposalID = proposal.Id });

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.userMapper.Setup(x => x.GetById(user.Id)).Returns(user);
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId.Value)).Returns(permissions);
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(new Collection<ProposalChecklistSaveInfo>());
            this.genBoePermissionsLoader.Setup(x => x.GetCreateWorkspaceRolesForPtm(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("TestNtid", "TestDisplayName") });
            this.userLoader.Setup(x => x.GetUserDTOsByADGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<UserDTO>() { user });

            ProposalApprovalsModelView proposalApprovalsInfo = sut.GetDataForProposalApprovals(proposalId, false);
            ProposalUserInformationModelView proposalUserInfo = sut.GetDataForProposalUserInformation(proposalId);

            Assert.IsTrue(proposalUserInfo.AdditionalPricingResourceTypeList.Any());
            Assert.IsTrue(proposalUserInfo.GenBoeWorkspaceCreatorList.Any());

            Assert.AreEqual(user.DisplayName, proposalUserInfo.AdditionalPricingResource1DisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.AdditionalPricingResource2DisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.BackupPricerDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.CaptureManagerDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.ContractsPOCDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.CostVolumeLeadDisplayName);
            Assert.AreEqual(user.DisplayName, proposalApprovalsInfo.LeadEstimatorDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.SupplyChainPOCMaterialsDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.SupplyChainPOCSubsDisplayName);
            Assert.AreEqual(user.DisplayName, proposalUserInfo.GenBoeWorkspaceCreatorDisplayName);
            Assert.AreEqual(user.Ntid, proposalUserInfo.AdditionalPricingResource1NtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.AdditionalPricingResource2NtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.BackupPricerNtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.CaptureManagerNtid);
            Assert.AreEqual(user.Ntid, proposalUserInfo.ContractsPOCNtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.CostVolumeLeadNtid);
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.LeadEstimatorNtid);
            Assert.AreEqual(user.Ntid, proposalUserInfo.SupplyChainPOCMaterialsNtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.SupplyChainPOCSubsNtId);
            Assert.AreEqual(user.Ntid, proposalUserInfo.GenBoeWorkspaceCreatorNtid);
            Assert.AreEqual(ResourceType.Pricer, proposalUserInfo.AdditionalPricingResource1Type);
            Assert.AreEqual(ResourceType.Strategist, proposalUserInfo.AdditionalPricingResource2Type);
            Assert.IsTrue(proposalApprovalsInfo.LeadEstimatorList.Any(x => x.Ntid == user.Ntid));
            Assert.IsTrue(proposalApprovalsInfo.PricingVerificationList.Any(x => x.Ntid == user.Ntid));
            Assert.IsTrue(proposalApprovalsInfo.LOBEstimatingLeadList.Any(x => x.Ntid == user.Ntid));
            Assert.IsTrue(proposalApprovalsInfo.CoverSheetApproverList.Any(x => x.Ntid == user.Ntid));
            Assert.IsTrue(proposalApprovalsInfo.IndependentReviewerList.Any(x => x.Ntid == user.Ntid));
        }
        
        /// <summary>
        /// Test GetDataForProposalApprovals for a new revision
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalApprovals_NewRevision()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            int? proposalId = 1;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value
            };

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "testuser",
                DisplayName = "User, Test (US)"
            };

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>();
            for (int i = 1; i < 23; i++)
            {
                permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = (PtmRole)i, ProposalID = proposal.Id });
            }

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.userMapper.Setup(x => x.GetById(user.Id)).Returns(user);
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId.Value)).Returns(permissions);
            this.genBoePermissionsLoader.Setup(x => x.GetCreateWorkspaceRolesForPtm(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("TestNtid", "TestDisplayName") });
            this.userLoader.Setup(x => x.GetUserDTOsByADGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<UserDTO>() { user });

            ProposalApprovalsModelView proposalApprovalsInfo = sut.GetDataForProposalApprovals(proposalId, true);

            // Assert roles are set and not readonly
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.LeadEstimatorNtid);
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.PricingVerificationNtid);
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.LOBEstimatingLeadMgrNtid);
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.CoverSheetApproverNtid);
            Assert.AreEqual(user.Ntid, proposalApprovalsInfo.IndependentReviewerNtid);
            Assert.IsFalse(proposalApprovalsInfo.IsLeadEstimatorReadOnly);
            Assert.IsFalse(proposalApprovalsInfo.IsPricingVerificationReadOnly);
            Assert.IsFalse(proposalApprovalsInfo.IsLOBEstimatingLeadMgrReadOnly);
            Assert.IsFalse(proposalApprovalsInfo.IsCoverSheetApproverReadOnly);
            Assert.IsFalse(proposalApprovalsInfo.IsIndependentReviewerReadOnly);
        }

        /// <summary>
        /// Test GetDataForProposalComments
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalComments()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            int? proposalId = 1;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalSetupComments = "Test"
            };

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);

            ProposalCommentsModelView result = sut.GetDataForProposalComments(proposalId);

            Assert.IsNotNull(result);
            Assert.AreEqual(proposal.ProposalSetupComments, result.Comments);
        }

        /// <summary>
        /// Test GetDataForProposalComments when Proposal Id is null
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalComments_NullId()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalCommentsModelView result = sut.GetDataForProposalComments(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Comments);
        }

        /// <summary>
        /// Test that Is New Pricer is set properly
        /// </summary>
        [TestMethod]
        public void C_IsNewPricerTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId
            };

            ProposalApprovalsModelView proposalApprovalsInfo = new ProposalApprovalsModelView()
            {
                LeadEstimatorNtid = "myNtid"
            };

            UserDTO pricerUser = new UserDTO()
            {
                Id = 15,
                Ntid = "myNtid"
            };

            UserDTO pricerUser2 = new UserDTO()
            {
                Id = 16,
                Ntid = "myNtid2"
            };

            ProposalPermissionDto pricerPermission = new ProposalPermissionDto()
            {
                UserId = pricerUser.Id,
                Role = PtmRole.Pricer
            };
            ProposalPermissionDto outPermission;

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.userMapper.Setup(x => x.GetByNtid(pricerUser.Ntid)).Returns(pricerUser);
            this.userMapper.Setup(x => x.GetById(pricerUser.Id)).Returns(pricerUser);

            this.userMapper.Setup(x => x.GetByNtid(pricerUser2.Ntid)).Returns(pricerUser2);
            this.userMapper.Setup(x => x.GetById(pricerUser2.Id)).Returns(pricerUser2);

            bool isNewPricer = sut.IsNewPricer(proposalId, proposalApprovalsInfo, out outPermission);

            // new proposal, so isNewPricer = true
            Assert.IsTrue(isNewPricer);

            ICollection<ProposalPermissionDto> permissions = new Collection<ProposalPermissionDto>();
            FullProposal fullProposal = new FullProposal(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId)).Returns(permissions);

            isNewPricer = sut.IsNewPricer(proposalId, proposalApprovalsInfo, out outPermission);

            // no existing permission, so isNewPricer = true
            Assert.IsTrue(isNewPricer);

            permissions.Add(pricerPermission);

            isNewPricer = sut.IsNewPricer(proposalId, proposalApprovalsInfo, out outPermission);

            // existing permmission, same user id, so isNewPricer = false
            Assert.IsFalse(isNewPricer);

            proposalApprovalsInfo.LeadEstimatorNtid = pricerUser2.Ntid;
            isNewPricer = sut.IsNewPricer(proposalId, proposalApprovalsInfo, out outPermission);

            // existing permmission, different user ntid, so isNewPricer = true
            Assert.IsTrue(isNewPricer);
        }

        /// <summary>
        /// Test inactive line of business
        /// </summary>
        [TestMethod]
        public void C_ProposalInactiveLineOfBusinessTest()
        {
            var sut = this.CreateSystem();

            PickListDto lineOfBusiness1 = new PickListDto()
            {
                Id = 1,
                IsActive = true
            };
            PickListDto lineOfBusiness2 = new PickListDto()
            {
                Id = 2,
                IsActive = true
            };
            PickListDto lineOfBusiness3 = new PickListDto()
            {
                Id = 3,
                IsActive = true
            };

            List<SelectListItem> lobs = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select Line of Business"
                },
                new SelectListItem
                {
                    Text = lineOfBusiness1.Text,
                    Value = lineOfBusiness1.Id.ToString()
                },
                new SelectListItem
                {
                    Text = lineOfBusiness2.Text,
                    Value = lineOfBusiness2.Id.ToString()
                },
                new SelectListItem
                {
                    Text = lineOfBusiness3.Text,
                    Value = lineOfBusiness3.Id.ToString()
                }
            };

            this.pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, null, true, false)).Returns(lobs);
            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness1, lineOfBusiness2, lineOfBusiness3 });

            ProposalGeneralInformationModelView proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(null, false);

            // should contain 3 active lines of business + default entry
            Assert.AreEqual(4, proposalGeneralInfo.LinesOfBusinessList.Count);

            // make 1 line of business inactive
            lineOfBusiness3.IsActive = false;
            proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(null, false);
            lobs.RemoveAt(3);

            // should contain 2 active lines of business + default entry
            Assert.AreEqual(3, proposalGeneralInfo.LinesOfBusinessList.Count);
        }

        /// <summary>
        /// Test inactive Program Area
        /// </summary>
        public void C_ProposalInactiveProgramAreaTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                LineOfBusinessID = 2
            };

            PickListDto lineOfBusiness = new PickListDto()
            {
                Id = 2,
                IsActive = true
            };

            PickListDto programArea1 = new PickListDto()
            {
                Id = 1,
                Text = "programArea1",
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };
            PickListDto programArea2 = new PickListDto()
            {
                Id = 2,
                Text = "programArea2",
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });
            this.orgStructureDataMapper.Setup(x => x.GetLineOfBusinessById(lineOfBusiness.Id)).Returns(lineOfBusiness);
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea1, programArea2 });

            ProposalGeneralInformationModelView proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, false);

            // should contain both program Area entries
            Assert.IsFalse(string.IsNullOrEmpty(proposalGeneralInfo.ProgramAreaHtmlOptions));
            Assert.IsTrue(proposalGeneralInfo.ProgramAreaHtmlOptions.Contains(programArea1.Text));
            Assert.IsTrue(proposalGeneralInfo.ProgramAreaHtmlOptions.Contains(programArea2.Text));

            // make 1 program Area inactive
            programArea2.IsActive = false;

            proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, false);

            // should contain first program Area but not second
            Assert.IsFalse(string.IsNullOrEmpty(proposalGeneralInfo.ProgramAreaHtmlOptions));
            Assert.IsTrue(proposalGeneralInfo.ProgramAreaHtmlOptions.Contains(programArea1.Text));
            Assert.IsFalse(proposalGeneralInfo.ProgramAreaHtmlOptions.Contains(programArea2.Text));
        }

        /// <summary>
        /// Test a previously saved proposal with inactive line of business returns to UI correctly
        /// </summary>
        [TestMethod]
        public void C_SavedProposalInactiveLineOfBusinessTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                LineOfBusinessID = 2,
                ProgramAreaId = 1
            };

            PickListDto lineOfBusiness1 = new PickListDto()
            {
                Id = 2,
                IsActive = false
            };

            PickListDto programArea1 = new PickListDto()
            {
                Id = 1,
                ParentIds = new int[] { 2 }
            };

            ChecklistContentDto parData = new ChecklistContentDto()
            {
                ChecklistType = ProposalChecklistType.Default,
                Version = 6
            };

            this.retriever.Setup(x => x.GetPARChecklistContent(proposalId.Value)).Returns(parData);

            FullProposal fullProposal = new FullProposal(proposal);

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness1 });
            this.orgStructureDataMapper.Setup(x => x.GetLineOfBusinessById(lineOfBusiness1.Id)).Returns(lineOfBusiness1);
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaById(programArea1.Id)).Returns(programArea1);
            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.pickListMapper.Setup(x => x.GetById(PickListEnum.LineOfBusiness, lineOfBusiness1.Id)).Returns(lineOfBusiness1);
            this.pickListMapper.Setup(x => x.GetById(PickListEnum.ProgramArea, programArea1.Id)).Returns(programArea1);

            ProposalGeneralInformationModelView proposalGeneralInfo = sut.GetDataForProposalGeneralInformation(proposalId, false);

            // line of business should come back as the real value now even when inactive
            Assert.AreEqual("2", proposalGeneralInfo.LineOfBusiness);
        }

        /// <summary>
        /// Test if proposal is read only
        /// </summary>
        [TestMethod]
        public void C_IsProposalReadOnlyTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;
            PtmRole outRole;

            // Read permission
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.Read);
            string readOnly = sut.IsProposalReadOnly(proposalId, null);
            Assert.AreEqual("true", readOnly);

            // ReadUpdate permission
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.ReadUpdate);
            readOnly = sut.IsProposalReadOnly(proposalId, null);
            Assert.AreEqual("false", readOnly);

            // CreateReadUpdateDelete permission
            this.securityAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out outRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);
            readOnly = sut.IsProposalReadOnly(proposalId, null);
            Assert.AreEqual("false", readOnly);

            // test proposal statuses
            ProposalDto proposal = new ProposalDto()
            {
                Id = 15
            };
            FullProposal fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            // Archived always read only
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            readOnly = sut.IsProposalReadOnly(proposalId, fullProposal);
            Assert.AreEqual("true", readOnly);

            // Deleted always read only
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            readOnly = sut.IsProposalReadOnly(proposalId, fullProposal);
            Assert.AreEqual("true", readOnly);

            // Revised always read only
            fullProposal.ProposalStatus = ProposalStatus.Revised;
            readOnly = sut.IsProposalReadOnly(proposalId, fullProposal);
            Assert.AreEqual("true", readOnly);

            // will pickup CRUD permission from above
            fullProposal.ProposalStatus = ProposalStatus.InProgress;
            readOnly = sut.IsProposalReadOnly(proposalId, fullProposal);
            Assert.AreEqual("false", readOnly);

            // will pickup CRUD permission from above
            fullProposal.ProposalStatus = ProposalStatus.Completed;
            readOnly = sut.IsProposalReadOnly(proposalId, fullProposal);
            Assert.AreEqual("false", readOnly);
        }

        /// <summary>
        /// Test for determining what proposal permissions to add and delete
        /// </summary>
        [TestMethod]
        public void C_AddDeletePermissionsTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 5;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId
            };

            ProposalPermissionDto newPermission = new ProposalPermissionDto()
            {
                ProposalID = proposalId,
                UserId = 10
            };

            ProposalPermissionDto existingPermission = new ProposalPermissionDto()
            {
                ProposalID = proposalId,
                UserId = 10,
                Role = PtmRole.Pricer
            };

            ICollection<ProposalPermissionDto> permissionsToAdd = new Collection<ProposalPermissionDto>();
            ICollection<ProposalPermissionDto> permissionsToDelete = new Collection<ProposalPermissionDto>();

            // new proposal, permission is added
            sut.AddAndDeletePermissions(null, newPermission, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(1, permissionsToAdd.Count);
            Assert.AreEqual(0, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToAdd.Contains(newPermission));

            FullProposal fullProposal = new FullProposal(proposal);
            ICollection<ProposalPermissionDto> existingPermissions = new Collection<ProposalPermissionDto>();
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId)).Returns(existingPermissions);

            permissionsToAdd.Clear();
            permissionsToDelete.Clear();

            // existing proposal, new permission does not exist, permission is added
            sut.AddAndDeletePermissions(fullProposal, newPermission, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(1, permissionsToAdd.Count);
            Assert.AreEqual(0, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToAdd.Contains(newPermission));

            permissionsToAdd.Clear();
            permissionsToDelete.Clear();

            existingPermissions.Add(existingPermission);

            // existing proposal, new permission does exist, same user id and type, no changes made
            sut.AddAndDeletePermissions(fullProposal, newPermission, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(0, permissionsToAdd.Count);
            Assert.AreEqual(0, permissionsToDelete.Count);

            // existing proposal, existing permission but null new permission, existing permission deleted
            sut.AddAndDeletePermissions(fullProposal, null, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(0, permissionsToAdd.Count);
            Assert.AreEqual(1, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToDelete.Contains(existingPermission));

            permissionsToAdd.Clear();
            permissionsToDelete.Clear();
            existingPermission.UserId = 15;

            // existing proposal, new permission does exist, different user, permission replaced
            sut.AddAndDeletePermissions(fullProposal, newPermission, PtmRole.Pricer, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(1, permissionsToAdd.Count);
            Assert.AreEqual(1, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToAdd.Contains(newPermission));
            Assert.IsTrue(permissionsToDelete.Contains(existingPermission));

            permissionsToAdd.Clear();
            permissionsToDelete.Clear();
            existingPermission.UserId = newPermission.UserId;
            existingPermission.Role = PtmRole.AdditionalPricingResource1;
            existingPermission.ResourceType = ResourceType.Pricer;
            newPermission.ResourceType = ResourceType.Strategist;

            // existing proposal, new permission does exist, same user (Estimating resource 1), different resource type, permission replaced
            sut.AddAndDeletePermissions(fullProposal, newPermission, PtmRole.AdditionalPricingResource1, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(1, permissionsToAdd.Count);
            Assert.AreEqual(1, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToAdd.Contains(newPermission));
            Assert.IsTrue(permissionsToDelete.Contains(existingPermission));

            permissionsToAdd.Clear();
            permissionsToDelete.Clear();
            existingPermission.Role = PtmRole.AdditionalPricingResource2;

            // existing proposal, new permission does exist, same user (Estimating resource 2), different resource type, permission replaced
            sut.AddAndDeletePermissions(fullProposal, newPermission, PtmRole.AdditionalPricingResource2, permissionsToAdd, permissionsToDelete);
            Assert.AreEqual(1, permissionsToAdd.Count);
            Assert.AreEqual(1, permissionsToDelete.Count);
            Assert.IsTrue(permissionsToAdd.Contains(newPermission));
            Assert.IsTrue(permissionsToDelete.Contains(existingPermission));
        }
                
        /// <summary>
        /// Test for Get Program Areas For Line of Business
        /// </summary>
        [TestMethod]
        public void C_GetProgramAreasForLineOfBusinessTest()
        {
            var sut = this.CreateSystem();

            string programAreaHtml = "html";
            int lineOfBusiness = 5;
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaHtmlOptionsForLineOfBusiness(lineOfBusiness, null, true)).Returns(programAreaHtml);

            string result = sut.GetProgramAreasForLineOfBusiness(lineOfBusiness, null);

            Assert.AreEqual(programAreaHtml, result);
            this.orgStructureDataMapper.Verify(x => x.GetProgramAreaHtmlOptionsForLineOfBusiness(lineOfBusiness, null, true), Times.Once());
        }

        /// <summary>
        /// Test the validation constraints for the ProposalGeneralInformationModelView class
        /// </summary>
        [TestMethod]
        public void C_TryValidateProposalGeneralInformationModelView()
        {
            var validationResults = new List<ValidationResult>();
            var generalInfo = new ProposalGeneralInformationModelView()
            {
                ProposalID = 1,
                LineOfBusiness = "Good Business",
                ProgramArea = "My Kind of Program Area",
                ProgramName = "Awesome Program",
                OTISOpportunityID = "OTIS for the Mostest",
                ProposalLocation = ProposalLocation.ColoradoSpringsCO,
                PricingTool = PricingTool.Excel,
                BOETool = BOETool.Word
            };

            // Validator.TryValidateObject returns true if the validation succeeds
            var ctx = new ValidationContext(generalInfo, null, null);
            var result = Validator.TryValidateObject(generalInfo, ctx, validationResults, true);
            Assert.IsTrue(result);

            // Exceed the lenght constraints for the following 5 Gen'l Info variables.
            generalInfo.ProgramName = "012345678901234567890123456789012345678901234567890";
            generalInfo.OTISOpportunityID = "012345678901234567890123456789";
            generalInfo.ProposalLocationName = "012345678901234567890123456789012345678901234567890";
            generalInfo.PricingToolName = "012345678901234567890123456789012345678901234567890";
            generalInfo.BOEToolName = "012345678901234567890123456789012345678901234567890";

            // Validator returns false if the validation fails.  the validation results returns the number
            // of failures.  In this case, 5 expected failures for exceeding the allowable field lenght.
            ctx = new ValidationContext(generalInfo, null, null);
            result = Validator.TryValidateObject(generalInfo, ctx, validationResults, true);
            Assert.IsFalse(result);
            Assert.AreEqual(5, validationResults.Count);
            
            // In this case, none of the fields are populated.  Since two fields are mandatory, we expect
            // the validation count to fail with two failures.
            generalInfo = new ProposalGeneralInformationModelView();
            ctx = new ValidationContext(generalInfo, null, null);
            validationResults = new List<ValidationResult>();
            result = Validator.TryValidateObject(generalInfo, ctx, validationResults, true);
            Assert.IsFalse(result);
            Assert.AreEqual(2, validationResults.Count);
        }

        /// <summary>
        /// Test null ProposalGeneralInformationModelView input for ValidateGeneralInfoTypes()
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateGeneralInfoTypes_NullProposalGeneralInformationModelView()
        {
            var sut = this.CreateSystem();
            ICollection<ValidationMessage> validationList = new Collection<ValidationMessage>();

            sut.ValidateGeneralInfoTypes(null, validationList, false);
        }

        /// <summary>
        /// Test null inValidationErrors for ValidateGeneralInfoTypes
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateGeneralInfoType_NullValidationMessage()
        {
            var sut = this.CreateSystem();
            ProposalGeneralInformationModelView proposalGeneralInfo = new ProposalGeneralInformationModelView();

            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, null, false);
        }

        /// <summary>
        /// Test the regular expression validations for ValidateGeneralInfoTypes
        /// </summary>
        [TestMethod]
        public void C_ValidateGeneralInfoRegularExpressions()
        {
            var sut = this.CreateSystem();
            List<ValidationMessage> inValidationErrors = new List<ValidationMessage>();

            ProposalGeneralInformationModelView proposalGeneralInfo = new ProposalGeneralInformationModelView() 
            {
                ProposalID = 1,
                LineOfBusiness = "My line of business",
                ProgramArea = "Now you're talkin'",
                ProgramName = "Awesome Program",
                ProposalLocation = ProposalLocation.Other,
                ProposalLocationName = null,
                PricingTool = PricingTool.Excel,
                PricingToolName = null,
                BOETool = BOETool.Other,
                BOEToolName = null,
                IsCCPDRequired = true,
                IsCostVolumeClassified = false
            };

            // Validate null value for BOEToolName
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.BOE_TOOL_FORMAT_ERROR, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();
            ProposalDto proposal = new ProposalDto();
            this.proposalLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(proposal);
            this.workspaceLoader.Setup(x => x.GetAllWsNamesForTrackingNumber(It.IsAny<string>())).Returns(new List<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, true);
            Assert.AreEqual(0, inValidationErrors.Count);

            proposalGeneralInfo.PricingToolName = string.Empty;
            proposalGeneralInfo.BOEToolName = string.Empty;

            // Validate emtpy string value BOEToolName 
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.BOE_TOOL_FORMAT_ERROR, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();

            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, true);
            Assert.AreEqual(0, inValidationErrors.Count);

            proposalGeneralInfo.PricingToolName = " ";
            proposalGeneralInfo.BOEToolName = " ";

            // Validate blank value for BOEToolName
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.BOE_TOOL_FORMAT_ERROR, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, true);
            Assert.AreEqual(0, inValidationErrors.Count);

            proposalGeneralInfo.PricingToolName = ".";
            proposalGeneralInfo.BOEToolName = "!";

            // Validate punctuation characters for BOEToolName
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.BOE_TOOL_FORMAT_ERROR, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, true);
            Assert.AreEqual(0, inValidationErrors.Count);

            proposalGeneralInfo.ProposalLocationName = "yellow";
            proposalGeneralInfo.PricingToolName = "red";
            proposalGeneralInfo.BOEToolName = "blue";

            // Validate that correct input does not have any validation errors
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(0, inValidationErrors.Count);

            proposalGeneralInfo.IsCCPDRequired = null;

            // Validate CCOPD
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.CERTIFIED_COST_PRICING_DATA_REQUIRED, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.CERTIFIED_COST_PRICING_DATA_REQUIRED, inValidationErrors[0].ValidationIssue);

            inValidationErrors.Clear();
            proposalGeneralInfo.IsCCPDRequired = true;
            proposalGeneralInfo.IsCostVolumeClassified = null;
            sut.ValidateGeneralInfoTypes(proposalGeneralInfo, inValidationErrors, false);
            Assert.AreEqual(1, inValidationErrors.Count);
            Assert.AreEqual(ValidationConstants.ProposalValidationConstants.CERTIFIED_COST_PRICING_DATA_CLASSIFIED_REQUIRED, inValidationErrors[0].ValidationIssue);
        }

        /// <summary>
        /// Test ValidateGeneralInfoTypes for missing required fields for Pricing Tool and BOE Tool
        /// </summary>
        [TestMethod]
        public void C_ValidateGeneralInfoTypes_MissingPricingBoeTools()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalGeneralInformationModelView mv = new ProposalGeneralInformationModelView()
            {
                // Set other required fields so we only get the errors we're testing for
                IsCCPDRequired = false,
                IsCostVolumeClassified = false
            };
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            sut.ValidateGeneralInfoTypes(mv, validationErrors, false);

            Assert.AreEqual(2, validationErrors.Count);
            Assert.IsTrue(validationErrors.Select(x => x.ValidationIssue).Contains(ValidationConstants.ProposalValidationConstants.PRICING_TOOL_REQUIRED));
            Assert.IsTrue(validationErrors.Select(x => x.ValidationIssue).Contains(ValidationConstants.ProposalValidationConstants.BOE_TOOL_REQUIRED));
        }

        /// <summary>
        /// Test the ValidateUserTypes functionality, completed proposal
        /// </summary>
        [TestMethod]
        public void C_ValidateUserTypesCompletedProposalTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.Completed
            };

            FullProposal fullProposal = new FullProposal(proposal);

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "myNtId",
                DisplayName = "myDisplayName",
                UserType = IES.Common.UserType.User
            };

            UserDTO savedUser = new UserDTO()
            {
                Id = 2,
                Ntid = "savedNtId",
                DisplayName = "savedDisplayName"
            };

            List<ProposalPermissionDto> savedUserPermissions = PopulateUserAndPermissions(savedUser, proposal);
            
            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId.Value)).Returns(savedUserPermissions); // GetFullProposal
            this.userMapper.Setup(x => x.GetById(savedUser.Id)).Returns(savedUser); // UserMapper
            this.genBoePermissionsLoader.Setup(x => x.GetCreateWorkspaceRolesForPtm(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<KeyValuePair<string, string>>());

            // ProposalChecklistSaveInfo
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>();
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(new Collection<ProposalChecklistSaveInfo>());
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(saveInfo);

            // Validation Methods
            this.validationMethods.Setup(x => x.IsUserTypeValid(user.Ntid, IES.Common.UserType.User, false, false, false)).Returns(true);
            this.validationMethods.Setup(x => x.IsUserTypeValid(user.Ntid, IES.Common.UserType.User, false, true, true)).Returns(true);

            ProposalUserInformationModelView proposalUserInfo = PopulateUserInfo(user);
            ProposalApprovalsModelView proposalApprovalsInfo = PopulateApprovalsInfo(user);

            // verify that there are no validation errors
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(0, validationErrors.Count);

            // CASE: ProposalMgr not set
            proposalUserInfo.ProposalMgrNtid = string.Empty;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(1, validationErrors.Count);

            proposalUserInfo.ProposalMgrNtid = savedUser.Ntid;
            validationErrors.Clear();
            // CASE: CaptureManager not set
            proposalUserInfo.CaptureManagerNtid = string.Empty;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(2, validationErrors.Count);

            proposalUserInfo.CaptureManagerNtid = savedUser.Ntid;
            validationErrors.Clear();
            // CASE: CostVolumeLead not set
            proposalUserInfo.CostVolumeLeadNtid = string.Empty;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(2, validationErrors.Count);

            proposalUserInfo.CostVolumeLeadNtid = savedUser.Ntid;
            validationErrors.Clear();
            // CASE: LeadEstimator not set
            proposalApprovalsInfo.LeadEstimatorNtid = string.Empty;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(1, validationErrors.Count);

            proposalApprovalsInfo.LeadEstimatorNtid = user.Ntid;
            validationErrors.Clear();
            // CASE: ContractsPOC not set
            proposalUserInfo.ContractsPOCNtId = string.Empty;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(2, validationErrors.Count);

            proposalUserInfo.ContractsPOCNtId = savedUser.Ntid;
            validationErrors.Clear();
            // CASE: GenBOE Workspace Creator not set
            proposalUserInfo.GenBoeWorkspaceCreatorNtid = null;
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(1, validationErrors.Count); // Role not required, so no additional errors should return
        }

    /// <summary>
    /// Test the ValidateUserTypes functionality, verify the failed validation handling
    /// </summary>
    [TestMethod]
        public void C_ValidateUserTypesFailedValidationTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.Completed
            };

            FullProposal fullProposal = new FullProposal(proposal);

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "myNtId",
                DisplayName = "myDisplayName"
            };

            UserDTO savedUser = new UserDTO()
            {
                Id = 2,
                Ntid = "savedNtId",
                DisplayName = "savedDisplayName"
            };

            List<ProposalPermissionDto> savedUserPermissions = PopulateUserAndPermissions(savedUser, proposal);

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId.Value)).Returns(savedUserPermissions); // GetFullProposal
            this.userMapper.Setup(x => x.GetById(savedUser.Id)).Returns(savedUser); // UserMapper
            this.genBoePermissionsLoader.Setup(x => x.GetCreateWorkspaceRolesForPtm(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("TestNtid", "TestDisplayName") });

            // ProposalChecklistSaveInfo
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>();
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(new Collection<ProposalChecklistSaveInfo>());
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(saveInfo);

            // Validation Methods
            this.validationMethods.Setup(x => x.IsUserTypeValid(user.Ntid, IES.Common.UserType.User, false, false, false)).Returns(false);

            ProposalApprovalsModelView proposalApprovalsInfo = PopulateApprovalsInfo(user);
            ProposalUserInformationModelView proposalUserInfo = PopulateUserInfo(user);

            // verify that there are 9 validation errors
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(9, validationErrors.Count);
        }

        /// <summary>
        /// Test the ValidateUserTypes functionality, incomplete proposal
        /// </summary>
        [TestMethod]
        public void C_ValidateUserTypesIncompleteProposalTest()
        {
            var sut = this.CreateSystem();

            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.InProgress
            };

            FullProposal fullProposal = new FullProposal(proposal);

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "myNtId",
                DisplayName = "myDisplayName"
            };

            UserDTO savedUser = new UserDTO()
            {
                Id = 2,
                Ntid = "savedNtId",
                DisplayName = "savedDisplayName"
            };

            List<ProposalPermissionDto> savedUserPermissions = PopulateUserAndPermissions(savedUser, proposal);
            
            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal
            this.retriever.Setup(x => x.GetProposalPermissions(proposalId.Value)).Returns(savedUserPermissions); // GetFullProposal
            this.userMapper.Setup(x => x.GetById(savedUser.Id)).Returns(savedUser); // UserMapper
            this.genBoePermissionsLoader.Setup(x => x.GetCreateWorkspaceRolesForPtm(It.IsAny<string>(), It.IsAny<string>())).Returns(new Collection<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("TestNtid", "TestDisplayName") });

            // ProposalChecklistSaveInfo
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>();
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(new Collection<ProposalChecklistSaveInfo>());
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId.Value)).Returns(saveInfo);

            // Validation Methods
            this.validationMethods.Setup(x => x.IsUserTypeValid(user.Ntid, IES.Common.UserType.User, false, false, false)).Returns(true);
            this.validationMethods.Setup(x => x.IsUserTypeValid(user.Ntid, IES.Common.UserType.User, false, true, true)).Returns(true);

            ProposalApprovalsModelView proposalApprovalsInfo = PopulateApprovalsInfo(user);
            ProposalUserInformationModelView proposalUserInfo = PopulateUserInfo(user);

            // verify that there are no validation errors
            sut.ValidateUserTypes(proposalId.Value, proposalApprovalsInfo, proposalUserInfo, validationErrors);
            Assert.AreEqual(0, validationErrors.Count);
        }

        /// <summary>
        /// Helper method to populate a user DTO and it's permissions
        /// </summary>
        /// <param name="user">user DTO to define permisson on</param>
        /// <param name="proposal">proposal DTO to define permissions on</param>
        /// <returns>list of ProposalPermissionDTO</returns>
        private static List<ProposalPermissionDto> PopulateUserAndPermissions(UserDTO user, ProposalDto proposal)
        {
            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>();
            for (int i = 1; i < 9; i++)
            {
                permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = (PtmRole)i, ProposalID = proposal.Id });
            }

            permissions.Add(new ProposalPermissionDto() { UserId = user.Id, Role = PtmRole.BackupPricer, ProposalID = proposal.Id });
            permissions[(int)PtmRole.AdditionalPricingResource1 - 1].ResourceType = ResourceType.Pricer;
            permissions[(int)PtmRole.AdditionalPricingResource2 - 1].ResourceType = ResourceType.Strategist;

            return permissions;
        }

        /// <summary>
        /// populate a ProposalApprovalsModelView class from a single user DTO
        /// </summary>
        /// <param name="user">input User DTO to populate with</param>
        /// <returns>returns a populated ProposalUserInformationModelView</returns>
        private static ProposalApprovalsModelView PopulateApprovalsInfo(UserDTO user)
        {
            ProposalApprovalsModelView approvalsInfo = new ProposalApprovalsModelView();

            approvalsInfo.LeadEstimatorNtid = user.Ntid;
            approvalsInfo.LeadEstimatorDisplayName = user.DisplayName;

            return approvalsInfo;
        }

        /// <summary>
        /// populate a ProposalUserInformationModelView class from a single user DTO
        /// </summary>
        /// <param name="user">input User DTO to populate with</param>
        /// <returns>returns a populated ProposalUserInformationModelView</returns>
        private static ProposalUserInformationModelView PopulateUserInfo(UserDTO user)
        {
            ProposalUserInformationModelView userInfo = new ProposalUserInformationModelView();

            userInfo.CaptureManagerNtid = user.Ntid;
            userInfo.CaptureManagerDisplayName = user.DisplayName;
            
            userInfo.CostVolumeLeadNtid = user.Ntid;
            userInfo.CostVolumeLeadDisplayName = user.DisplayName;

            userInfo.AdditionalPricingResource1NtId = user.Ntid;
            userInfo.AdditionalPricingResource1DisplayName = user.DisplayName;
            userInfo.AdditionalPricingResource1Type = ResourceType.Pricer;

            userInfo.AdditionalPricingResource2NtId = user.Ntid;
            userInfo.AdditionalPricingResource2DisplayName = user.DisplayName;
            userInfo.AdditionalPricingResource2Type = ResourceType.Pricer;

            userInfo.SupplyChainPOCMaterialsNtId = user.Ntid;
            userInfo.SupplyChainPOCSubsDisplayName = user.DisplayName;

            userInfo.SupplyChainPOCSubsNtId = user.Ntid;
            userInfo.SupplyChainPOCSubsDisplayName = user.DisplayName;

            userInfo.ContractsPOCNtId = user.Ntid;
            userInfo.ContractsPOCDisplayName = user.DisplayName;

            userInfo.BackupPricerNtId = user.Ntid;
            userInfo.BackupPricerDisplayName = user.DisplayName;

            userInfo.ProposalMgrNtid = user.Ntid;
            userInfo.ProposalMgrDisplayName = user.DisplayName;
            return userInfo;
        }

        /// <summary>
        /// Test SaveProposalComments
        /// </summary>
        [TestMethod]
        public void TestSaveProposalComments()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            int proposalId = 1;
            ProposalCommentsModelView proposalComments = new ProposalCommentsModelView()
            {
                Comments = "new comments"
            };

            ProposalDto dto = new ProposalDto()
            {
                Id = 1,
                ProposalSetupComments = "old comments"
            };

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(dto);

            // Setup with call back so we can test that old comment was replaced with the new one
            string savedComment = string.Empty;
            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<ProposalDto>()))
                .Callback<ProposalDto>((proposal) => savedComment = proposal.ProposalSetupComments)
                .Returns(proposalId);

            int? result = sut.SaveProposalComments(proposalId, proposalComments);

            Assert.IsNotNull(result);
            Assert.AreEqual(proposalId, result.Value);
            this.proposalMediator.Verify(x => x.SaveProposal(It.IsAny<ProposalDto>()), Times.Once());

            // Assert comment was updated to the new value
            Assert.AreEqual(proposalComments.Comments, savedComment);
        }

        #region Exception Tests

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveNewProposal_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.SaveProposal(null, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), new ProposalCommentsModelView(), null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveNewProposal_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.SaveProposal(new ProposalInformationModelView(), null, new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), new ProposalCommentsModelView(), null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveNewProposal_ExceptionTest3()
        {
            var sut = this.CreateSystem();
            sut.SaveProposal(new ProposalInformationModelView(), new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), new ProposalCommentsModelView(), null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveNewProposal_ExceptionTest4()
        {
            var sut = this.CreateSystem();
            sut.SaveProposal(new ProposalInformationModelView(), new ProposalGeneralInformationModelView(), null, null, new ProposalCommentsModelView(), null);
        }
        
        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveNewProposal_ExceptionTest5()
        {
            var sut = this.CreateSystem();
            sut.SaveProposal(new ProposalInformationModelView(), new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), null, null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposal_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.ValidateProposal(null, new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), new List<ValidationMessage>(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposal_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.ValidateProposal(new ProposalInformationModelView(), null, new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), new List<ValidationMessage>(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposal_ExceptionTest4()
        {
            var sut = this.CreateSystem();
            sut.ValidateProposal(new ProposalInformationModelView(), new ProposalGeneralInformationModelView(), 
                null, null, new List<ValidationMessage>(), false);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposal_ExceptionTest5()
        {
            var sut = this.CreateSystem();
            sut.ValidateProposal(new ProposalInformationModelView(), new ProposalGeneralInformationModelView(), new ProposalApprovalsModelView(), new ProposalUserInformationModelView(), null, false);
        }

        #endregion Exception Tests

        /// <summary>
        /// Validates the saving certification timeline.
        /// </summary>
        [TestMethod]
        public void ValidateSavingCertificationTimeline()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal); 

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView();

            sut.ValidateCertification(5, model, false);

            model.AgreementDate = "01/01/2018";
            sut.ValidateCertification(5, model, false);

            model.CertificationDate = "01/02/2018";
            sut.ValidateCertification(5, model, false);

            model.CutOffDateUtilization = CutOffDateUtilization.NoRequest;
            sut.ValidateCertification(5, model, false);

            model.Comments = "Test";
            sut.ValidateCertification(5, model, false);
        }

        /// <summary>
        /// Validates the saving certification timeline throws exception for missing comments.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateSavingCertificationTimeline_ex1()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/17/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes
            };

            // missing comments
            sut.ValidateCertification(5, model, false);
        }

        /// <summary>
        /// Validates the completing of certification timeline.
        /// </summary>
        [TestMethod]
        public void ValidateCompletingCertificationTimeline()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55"
            };

            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline throws exception for missing comments.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimeline_ex1()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/17/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes
            };

            // missing comments
            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline throws exception for missing agreement date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimeline_ex2()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55"
            };

            // missing agreement date
            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline throws exception for missing certification date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimeline_ex3()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55"
            };

            // missing certification date
            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline throws exception for missing cutoff utilization selection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimeline_ex4()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);
            
            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                Comments = "55"
            };

            // missing cutoff utilization selection
            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline, specifically Certification Not Required functionality
        /// 
        /// Wrong proposal status
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimelineCertificationNotRequired_ex1()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Completed
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55",
                ReasonCertificationNotRequired = ReasonCertificationNotRequired.LostNotAwarded
            };

            sut.ValidateCertification(5, model, false);
        }

        /// <summary>
        /// Validates the completing of certification timeline, specifically Certification Not Required functionality
        /// 
        /// Other and no comment
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimelineCertificationNotRequired_ex2()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55",
                ReasonCertificationNotRequired = ReasonCertificationNotRequired.Other
            };

            sut.ValidateCertification(5, model, false);
        }

        /// <summary>
        /// Validates the completing of certification timeline, specifically Certification Not Required functionality
        /// 
        /// Complete and cert reason filled in
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void ValidateCompletingCertificationTimelineCertificationNotRequired_ex3()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55",
                ReasonCertificationNotRequired = ReasonCertificationNotRequired.LostNotAwarded
            };

            sut.ValidateCertification(5, model, true);
        }

        /// <summary>
        /// Validates the completing of certification timeline, specifically Certification Not Required functionality
        /// 
        /// Valid 1
        /// </summary>
        [TestMethod]
        public void ValidateCompletingCertificationTimelineCertificationNotRequired_1()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55",
                ReasonCertificationNotRequired = ReasonCertificationNotRequired.LostNotAwarded
            };

            sut.ValidateCertification(5, model, false);
        }

        /// <summary>
        /// Validates the completing of certification timeline, specifically Certification Not Required functionality
        /// 
        /// Valid 2
        /// </summary>
        [TestMethod]
        public void ValidateCompletingCertificationTimelineCertificationNotRequired_2()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = new ProposalDto()
            {
                Id = 5,
                ProposalStatus = ProposalStatus.Submitted
            };

            // GetDataForProposalUserInformation
            this.proposalLoader.Setup(x => x.GetById(5)).Returns(proposal);

            ProposalCertificationTimelineModelView model = new ProposalCertificationTimelineModelView
            {
                AgreementDate = "01/01/2018",
                CertificationDate = "01/03/2018",
                CutOffDateUtilization = CutOffDateUtilization.Yes,
                Comments = "55",
                ReasonCertificationNotRequired = ReasonCertificationNotRequired.Other,
                OtherReasonCommentCertification = "Boooo"
            };

            sut.ValidateCertification(5, model, false);
        }

        #region PTM Revisions

        /// <summary>
        /// Test ValidateSaveNewRevision for a valid proposal
        /// </summary>
        [TestMethod]
        public void TestValidateSaveNewRevision()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                WorkflowStatus = WorkflowStatus.ProposalLocked,
                ProposalStatus = ProposalStatus.Submitted
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            sut.ValidateSaveNewRevision(proposal.Id);

            // Nothing to assert, just shouldn't throw any exceptions
        }

        /// <summary>
        /// Test ValidateSaveNewRevision throws an exception when the approval workflow is not complete
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateSaveNewRevision_WorkflowNotComplete()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                WorkflowStatus = WorkflowStatus.Started,
                ProposalStatus = ProposalStatus.Submitted
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            sut.ValidateSaveNewRevision(proposal.Id);
        }

        /// <summary>
        /// Test ValidateSaveNewRevision throws an exception when the Certification Timeline is completed
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateSaveNewRevision_CertTimelineComplete()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                WorkflowStatus = WorkflowStatus.ProposalLocked,
                ProposalStatus = ProposalStatus.Completed,
                CertificationTimelineCompleted = DateTime.Now
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            sut.ValidateSaveNewRevision(proposal.Id);
        }

        /// <summary>
        /// Test ValidateSaveNewRevision throws an exception when it is not the latest version
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateSaveNewRevision_NotLatestVersion()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                WorkflowStatus = WorkflowStatus.ProposalLocked,
                ProposalStatus = ProposalStatus.Revised
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            sut.ValidateSaveNewRevision(proposal.Id);
        }
        
        /// <summary>
        /// Test ValidateSaveNewRevision throws an exception when the user is not the lead or backup estimator
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateSaveNewRevision_NotPermitted()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                WorkflowStatus = WorkflowStatus.ProposalLocked,
                ProposalStatus = ProposalStatus.Submitted
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(false);
            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);

            sut.ValidateSaveNewRevision(proposal.Id);
        }

        /// <summary>
        /// Test SetProposalRevised
        /// </summary>
        [TestMethod]
        public void TestSetProposalRevised()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now
            };

            this.proposalLoader.Setup(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Revised)).Verifiable();

            sut.SetProposalRevised(proposal.Id, proposal.UpdateDate);

            this.proposalLoader.Verify(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Revised), Times.Once());
        }

        /// <summary>
        /// Test GetDataForProposalRevisionIndex
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalRevisionIndex()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                TrackingNumber = "20-00001",
                ProposalTitle = "Test",
                ProposalStatus = ProposalStatus.Submitted
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(-1, It.IsAny<string>())).Returns(true);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.securityInformation.Setup(x => x.IsDomesticUser(It.IsAny<IPrincipal>())).Returns(true);

            ProposalIndexModelView result = sut.GetDataForProposalRevisionIndex(proposal.Id);

            Assert.AreEqual(-1, result.ProposalID);
            Assert.AreEqual(proposal.TrackingNumber + "-PR1", result.ProposalTrackingNumber);
            Assert.AreEqual(proposal.ProposalTitle + "-PR1", result.ProposalTitle);
            Assert.AreEqual(proposal.ProposalTitle, result.RevisedProposalTitle);
            Assert.AreEqual(ProposalStatus.InProgress, result.ProposalStatus);
            Assert.IsFalse(result.DisplayNewRevisionButton);
            Assert.IsFalse(result.DisplayRevertRevisionButton);
            Assert.AreEqual("false", result.IsReadOnly);
            Assert.IsTrue(result.IsUsUser);
            Assert.AreEqual(SecurityAuthorization.None, result.PsaVisibility);
            Assert.AreEqual(SecurityAuthorization.None, result.CertificationTimelineVisibility);
        }

        /// <summary>
        /// Test GetDataForProposalRevisionIndex when making a revision off of a previous revision
        /// Testing that the tracking number and title are updated to PR2 propoerly
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalRevisionIndex_SecondRevision()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            string baseTrackingNumber = "20-00001";
            string baseTitle = "Test";

            ProposalDto oldProposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                TrackingNumber = baseTrackingNumber,
                ProposalTitle = baseTitle,
                ProposalStatus = ProposalStatus.Submitted
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                TrackingNumber = baseTrackingNumber + "-PR1",
                ProposalTitle = baseTitle + "-PR1",
                ProposalStatus = ProposalStatus.Submitted
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>() { oldProposal, proposal });
            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(-1, It.IsAny<string>())).Returns(true);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.securityInformation.Setup(x => x.IsDomesticUser(It.IsAny<IPrincipal>())).Returns(true);

            ProposalIndexModelView result = sut.GetDataForProposalRevisionIndex(proposal.Id);

            Assert.AreEqual(-1, result.ProposalID);
            Assert.AreEqual(baseTrackingNumber + "-PR2", result.ProposalTrackingNumber);
            Assert.AreEqual(baseTitle + "-PR2", result.ProposalTitle);
        }

        /// <summary>
        /// Test GetDataForProposalRevisionIndex when making a revision off of a previous revision
        /// Testing that the tracking number and title are updated to PR2 propoerly
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalRevisionIndex_TitleNotUnique()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            string baseTrackingNumber = "20-00001";
            string baseTitle = "Test";

            ProposalDto oldProposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                TrackingNumber = baseTrackingNumber,
                ProposalTitle = baseTitle,
                ProposalStatus = ProposalStatus.Submitted
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                TrackingNumber = baseTrackingNumber + "-PR1",
                ProposalTitle = baseTitle + "-PR1",
                ProposalStatus = ProposalStatus.Submitted
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>() { oldProposal, proposal });
            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(-1, baseTitle + "-PR2")).Returns(false);
            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(-1, baseTitle + "-PR2_0")).Returns(true);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.securityInformation.Setup(x => x.IsDomesticUser(It.IsAny<IPrincipal>())).Returns(true);

            ProposalIndexModelView result = sut.GetDataForProposalRevisionIndex(proposal.Id);

            Assert.AreEqual(-1, result.ProposalID);
            Assert.AreEqual(baseTrackingNumber + "-PR2", result.ProposalTrackingNumber);
            Assert.AreEqual(baseTitle + "-PR2_0", result.ProposalTitle);
        }

        /// <summary>
        /// Test GetDataForProposalRevisionInformation
        /// </summary>
        [TestMethod]
        public void TestGetDataForProposalRevisionInformation()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            int? proposalId = 1;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                TrackingNumber = "20-00001",
                ProposalTitle = "test revision",
                UpdateDate = DateTime.Now,
                DeliveryDate = new DateTime(2020, 1, 1),
                ContractTypeGroup = 1,
                ContractTypeIds = new List<int> { 1, 2 },
                CostElementTypeIds = new List<int> { 1, 2, 3 },
                Customer = "myCustomer",
                CustomerType = CustomerType.InternationalCommercial,
                EstimatedProposalValue = 400,
                ISGSRole = ISGSRole.Prime,
                IsScheduleProposal = false,
                ProposalType = 2,
                Request = 1,
                ProposalClass = 1,
                RFPNumber = "myRFP",
                RFPIssuedDate = new DateTime(2020, 2, 2),
                RFPReceivedDate = new DateTime(2020, 3, 3)
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.proposalLoader.Setup(x => x.IsProposalTitleUnique(-1, It.IsAny<string>())).Returns(true);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);
            this.pickListMapper.Setup(x => x.IsPickListActive(It.IsAny<PickListEnum>(), It.IsAny<int>())).Returns(true);
            this.validationMethods.Setup(x => x.IsEnumActive(It.IsAny<CostElementType>())).Returns(true);
            this.pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.ProposalClass, 1, It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<SelectListItem>() { new SelectListItem() { Text = "a", Value = "a" } });
            this.pickListMapper.Setup(x => x.GetChildren(PickListEnum.ContractType, 1)).Returns(new List<PickListDto>
            {
                new PickListDto { Id = 1, Text = "CPAF", IsActive = true },
                new PickListDto { Id = 2, Text = "CPPF", IsActive = true },
                new PickListDto { Id = 3, Text = "CPIF", IsActive = true }
            });

            ProposalInformationModelView result = sut.GetDataForProposalRevisionInformation(proposalId);

            Assert.AreEqual(-1, result.ProposalID);
            Assert.AreEqual(proposal.ProposalTitle + "-PR1", result.ProposalTitle);
            Assert.AreEqual(proposal.TrackingNumber + "-PR1", result.ProposalTrackingNumber);
            Assert.AreEqual(ProposalStatus.InProgress, result.ProposalStatus);

            Assert.AreEqual(fullProposal.ContractTypeGroup, result.ContractTypeGroup);
            Assert.AreEqual(fullProposal.ContractTypeIds, result.ContractType);
            Assert.AreEqual(fullProposal.CostElementTypeIds, result.CostElements);
            Assert.AreEqual(fullProposal.Customer, result.Customer);
            Assert.AreEqual(fullProposal.CustomerType, result.CustomerType);
            Assert.AreEqual(fullProposal.ISGSRole, result.ISGSRole);
            Assert.AreEqual(fullProposal.IsScheduleProposal, result.IsScheduleProposal);
            Assert.AreEqual(fullProposal.Request, result.RequestType);
            Assert.AreEqual(fullProposal.ProposalClass, result.ProposalClass);
            Assert.AreEqual("Not Set", result.ProposalTypeText);
            Assert.AreEqual("Not Set", result.ProposalClassText);
            Assert.AreEqual("Not Set", result.RequestTypeText);
            Assert.AreEqual(fullProposal.DocumentId, result.DocumentId);
            Assert.IsTrue(result.CostElementsList.Any());
            Assert.IsTrue(result.CustomerTypesList.Any());
            Assert.IsTrue(result.ISGSRolesList.Any());
            Assert.IsTrue(result.ProposalClassesList.Any());

            // all ContractType values are active, first 2 are selected
            Assert.IsTrue(result.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                1, "CPAF")));
            Assert.IsTrue(result.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\" selected=\"selected\">{1}</option>",
                2, "CPPF")));
            Assert.IsTrue(result.ContractTypeHtmlOptions.Contains(string.Format("<option value=\"{0}\">{1}</option>",
                3, "CPIF")));
            Assert.AreEqual(string.Empty, result.InactiveContractTypes);

            // all CostElement values are active
            Assert.IsTrue(result.CostElementsList.Where(x => x.ID == 1).Any());
            Assert.IsTrue(result.CostElementsList.Where(x => x.ID == 2).Any());
            Assert.IsTrue(result.CostElementsList.Where(x => x.ID == 3).Any());
            Assert.AreEqual(string.Empty, result.InactiveCostElements);

            // Assert the following fields were not set
            Assert.AreEqual(0, result.ProposalType);
            Assert.IsNull(result.RFPNumber);
            Assert.IsNull(result.RFPIssuedDate);
            Assert.IsNull(result.RFPReceivedDate);
            Assert.IsNull(result.AnticipatedDeliveryDate);
            Assert.IsNull(result.RevisedSubmittalDate);
            Assert.IsNull(result.EstimatedProposalValue);

            Assert.IsTrue(result.IsNewRevision);
        }

        /// <summary>
        /// Test RevertRevisedProposal for a Proposal with CCoPD set to Yes
        /// </summary>
        [TestMethod]
        public void TestRevertRevisedProposal()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                IsCCPDRequired = true
            };

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalLoader.Setup(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, It.IsAny<ProposalStatus>())).Returns(1);
            this.proposalLoader.Setup(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, It.IsAny<ProposalStatus>())).Verifiable();

            sut.RevertRevisedProposal(proposal.Id);

            this.proposalLoader.Verify(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Submitted), Times.Once());
            this.proposalLoader.Verify(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Completed), Times.Never());
        }

        /// <summary>
        /// Test RevertRevisedProposal for a Proposal with CCoPD set to No
        /// </summary>
        [TestMethod]
        public void TestRevertRevisedProposal_CcopdNo()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                UpdateDate = DateTime.Now,
                IsCCPDRequired = false
            };

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.proposalLoader.Setup(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, It.IsAny<ProposalStatus>())).Returns(1);
            this.proposalLoader.Setup(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, It.IsAny<ProposalStatus>())).Verifiable();

            sut.RevertRevisedProposal(proposal.Id);

            this.proposalLoader.Verify(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Submitted), Times.Never());
            this.proposalLoader.Verify(x => x.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Completed), Times.Once());
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a fully valid proposal
        /// </summary>
        [TestMethod]
        public void TestValidateRevertRevisionToPriorVersion()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Pricer, proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);

            // Nothing to assert, just shouldn't throw any exceptions
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal missing a prior version to revert to
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_NoPriorVersion()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = null,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal that isn't the latest version
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_NotLatest()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>() { new ProposalDto() { RevisionOfId = proposal.Id } });
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal that's not in progress
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_NotInProgress()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.Submitted,
                DocumentId = null
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal where the user is not permitted to revert
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_NotPermitted()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(false);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal that has an associated RDSB document
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_HasDocument()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = 1
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Test ValidateRevertRevisionToPriorVersion for a Proposal that has one or more associated Workspaces
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateRevertRevisionToPriorVersion_HasWorkspace()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                RevisionOfId = 2,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null,
                TrackingNumber = "20-00001-PR1"
            };

            this.securityAccess.Setup(x => x.CurrentUserHasRole(It.IsAny<PtmRole>(), proposal.Id)).Returns(true);
            this.proposalLoader.Setup(x => x.GetAllSlim()).Returns(new Collection<ProposalDto>());
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>() { new GenBOE.Dtos.WorkspaceDTO() { TrackingNumber = proposal.TrackingNumber } });

            sut.ValidateRevertRevisionToPriorVersion(proposal);
        }

        /// <summary>
        /// Validate ValidateNewRevisionDoesNotExist for valid proposal
        /// </summary>
        [TestMethod]
        public void TestValidateNewRevisionDoesNotExist()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView() { ProposalTrackingNumber = "20-00001-PR1" };

            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(proposalInfo.ProposalTrackingNumber)).Returns(-1);

            sut.ValidateNewRevisionDoesNotExist(proposalInfo);
            // Nothing to assert, just no exception
        }

        /// <summary>
        /// Validate ValidateNewRevisionDoesNotExist throws exception when proposal exists
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IES.Common.Exceptions.ValidationException))]
        public void TestValidateNewRevisionDoesNotExist_Exception()
        {
            ProposalControllerLogic sut = this.CreateSystem();
            ProposalInformationModelView proposalInfo = new ProposalInformationModelView() { ProposalTrackingNumber = "20-00001-PR1" };

            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(proposalInfo.ProposalTrackingNumber)).Returns(1);

            sut.ValidateNewRevisionDoesNotExist(proposalInfo);
        }

        /// <summary>
        /// Validate ValidateNewRevisionDoesNotExist throws proper null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestValidateNewRevisionDoesNotExist_NullException()
        {
            ProposalControllerLogic sut = this.CreateSystem();

            sut.ValidateNewRevisionDoesNotExist(null);
        }
        #endregion
    }
}