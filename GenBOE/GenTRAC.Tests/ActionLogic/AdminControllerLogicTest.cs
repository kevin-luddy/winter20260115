// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using GenBOE.DataBridge.DTO;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.GeneralHelper;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Admin;
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
    /// Test the active directory synchronization
    /// </summary>
    [TestClass]
    public class AdminControllerLogicTest
    {
        #region Setup

        /// <summary>
        /// Security Access
        /// </summary>
        private Mock<ISecurityAccess> secAccess = null;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private Mock<IProposalLoader> proposalLoader = null;

        /// <summary>
        /// User Mapper
        /// </summary>
        private Mock<IUserMapper> userMapper = null;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtils = null;

        /// <summary>
        /// User Mediator
        /// </summary>
        private Mock<IUserMediator> userMediator = null;

        /// <summary>
        /// Permissions Mapper
        /// </summary>
        private Mock<ISystemPermissionMapper> permissionsMapper = null;

        /// <summary>
        /// Permissions Mediator
        /// </summary>
        private Mock<ISystemPermissionMediator> permissionsMediator = null;

        /// <summary>
        /// Html Helper
        /// </summary>
        private Mock<IHtmlHelper> htmlHelper = null;

        /// <summary>
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory = null;

        /// <summary>
        /// Org Structure Data Mapper
        /// </summary>
        private Mock<IOrgStructureDataMapper> orgStructureMapper = null;

        /// <summary>
        /// Manage Proposal Info Loader
        /// </summary>
        private Mock<IManageProposalInfoLoader> manageProposalInfoLoader = null;

        /// <summary>
        /// Bulk Archive Loader
        /// </summary>
        private Mock<IBulkArchiveLoader> bulkArchiveLoader = null;

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<IApprovalsLoader> approvalsLoader;

        /// <summary>
        /// Proposal Checklist Mediator
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader;

        /// <summary>
        /// Checklist Mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator;

        /// <summary>
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator;

        /// <summary>
        /// The workspace loader
        /// </summary>
        private Mock<IWorkspaceDTODataLoader> workspaceLoader;

        /// <summary>
        /// Create the system under test
        /// </summary>
        /// <returns>Admin Controller Logic</returns>
        private AdminControllerLogic CreateSystem()
        {
            this.secAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.userMapper = new Mock<IUserMapper>();
            this.adUtils = new Mock<IActiveDirectoryUtilities>();
            this.userMediator = new Mock<IUserMediator>();
            this.permissionsMapper = new Mock<ISystemPermissionMapper>();
            this.permissionsMediator = new Mock<ISystemPermissionMediator>();
            this.htmlHelper = new Mock<IHtmlHelper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.orgStructureMapper = new Mock<IOrgStructureDataMapper>();
            this.manageProposalInfoLoader = new Mock<IManageProposalInfoLoader>();
            this.bulkArchiveLoader = new Mock<IBulkArchiveLoader>();
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2
            };

            this.orgStructureMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            AdminControllerLogic sut = new AdminControllerLogic(this.secAccess.Object, this.proposalLoader.Object, this.htmlHelper.Object,
                this.userMapper.Object, this.adUtils.Object, this.userMediator.Object, this.permissionsMapper.Object, this.permissionsMediator.Object,
                this.objectFactory.Object, this.orgStructureMapper.Object,
                this.manageProposalInfoLoader.Object, this.bulkArchiveLoader.Object, this.approvalsLoader.Object,
                this.proposalChecklistLoader.Object, this.checklistMediator.Object, this.proposalMediator.Object, this.workspaceLoader.Object, null);

            return sut;
        }

        #endregion Setup

        /// <summary>
        /// Get manage permissions model view test
        /// </summary>
        [TestMethod]
        public void C_GetManagePermissionsModelViewTest()
        {
            var sut = this.CreateSystem();

            ManagePermissionsModelView model = sut.GetManagePermissionsModelView();

            Assert.IsTrue(model.ViewerLinesOfBusinessList.Any());
            Assert.IsTrue(model.ProposalSetupAdminLinesOfBusinessList.Any());
        }

        /// <summary>
        /// Test Getting Manage Permissions model view
        /// </summary>
        [TestMethod]
        public void C_GetManagePermissionsGridModelViewTest()
        {
            var sut = this.CreateSystem();

            Collection<SystemPermissionDto> permissionsFromMapper = new Collection<SystemPermissionDto>();

            SystemPermissionDto permissionFromMapper = new SystemPermissionDto()
            {
                Id = 1,
                Role = PtmRole.Admin,
                UserId = 15
            };

            SystemPermissionDto permissionFromMapper2 = new SystemPermissionDto()
            {
                Id = 2,
                Role = PtmRole.Viewer,
                UserId = 15,
                LineOfBusinessIDs = { 1, 2}
            };

            SystemPermissionDto permissionFromMapper3 = new SystemPermissionDto()
            {
                Id = 3,
                Role = PtmRole.ProposalSetupAdmin,
                UserId = 15,
                LineOfBusinessIDs = { 1 }
            };

            permissionsFromMapper.Add(permissionFromMapper);
            permissionsFromMapper.Add(permissionFromMapper2);
            permissionsFromMapper.Add(permissionFromMapper3);

            Collection<PtmRole> testRoles = new Collection<PtmRole>();
            PtmRole testRole = PtmRole.Viewer;
            PtmRole testRole2 = PtmRole.Admin;
            PtmRole testRole3 = PtmRole.ProposalSetupAdmin;

            testRoles.Add(testRole);
            testRoles.Add(testRole2);
            testRoles.Add(testRole3);

            PermissionsModelView permissionsMv = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = testRoles,
                UserId = 15,
                UserName = "test",
                ViewerLinesOfBusiness = new List<int> { 1, 2 },
                ProposalSetupAdminLinesOfBusiness = new List<int> { 1 }
            };

            UserDTO testUser = new UserDTO()
            {
                UserType = UserType.User,
                DisplayName = "test",
                Id = 15
            };

            PermissionsGridModelView expected = new PermissionsGridModelView();

            expected.PermissionData.Add(permissionsMv);

            PermissionsGridModelView actual = new PermissionsGridModelView();

            this.permissionsMapper.Setup(x => x.GetSystemPermissions()).Returns(permissionsFromMapper);
            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(testUser);
            actual = sut.GetManagePermissionsGridModelView(actual);

            this.AssertModelView(actual, expected);
        }

        /// <summary>
        /// Test save new permissions logic
        /// </summary>
        [TestMethod]
        public void C_SaveUserPermissionRolesTest()
        {
            var sut = this.CreateSystem();

            int userId = 10;
            int deleteUserId = 11;
            string testDomain = "domain";
            string testNtid = "testntId";

            Collection<PtmRole> testRoles = new Collection<PtmRole>();
            PtmRole testRole = PtmRole.Viewer;
            PtmRole testRole2 = PtmRole.Admin;
            PtmRole testRole3 = PtmRole.ProposalSetupAdmin;

            testRoles.Add(testRole);
            testRoles.Add(testRole2);
            testRoles.Add(testRole3);

            PermissionsModelView permissionsMv = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = testRoles,
                UserId = userId,
                UserNtId = testNtid,
                UserName = "test"
            };

            UserDTO testUser = new UserDTO()
            {
                UserType = UserType.User,
                Id = userId
            };

            // group permission with no .
            PermissionsModelView permissionsMv2 = new PermissionsModelView()
            {
                UserType = UserType.Group,
                Roles = testRoles,
                UserId = -1,
                UserNtId = testNtid
            };

            PermissionsModelView permissionsMv3 = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = testRoles,
                UserId = -1,
                UserNtId = testNtid
            };

            PermissionsModelView permissionsMv4 = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = new Collection<PtmRole>(),
                UserId = deleteUserId,
                UserNtId = testNtid
            };

            // group permission with a .
            PermissionsModelView permissionsMv5 = new PermissionsModelView()
            {
                UserType = UserType.Group,
                Roles = testRoles,
                UserId = -1,
                UserNtId = testDomain + "." + testNtid
            };

            Collection<PermissionsModelView> modelViewsToSave = new Collection<PermissionsModelView>();

            modelViewsToSave.Add(permissionsMv);
            modelViewsToSave.Add(permissionsMv2);
            modelViewsToSave.Add(permissionsMv3);
            modelViewsToSave.Add(permissionsMv4);
            modelViewsToSave.Add(permissionsMv5);

            SystemPermissionDto permissionFromMapper = new SystemPermissionDto()
            {
                Id = 2,
                Role = PtmRole.SystemPricer,
                UserId = userId
            };

            UserData adInfoAboutUser = new UserData()
            {
                DisplayName = "test",
                Email = "testemail",
                FirstName = "firstname",
                Ntid = testNtid
            };

            Collection<SystemPermissionDto> permissionsFromMapper = new Collection<SystemPermissionDto>();
            permissionsFromMapper.Add(permissionFromMapper);

            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(testUser);
            this.adUtils.Setup(x => x.IsGroup(testNtid)).Returns(true);
            this.permissionsMapper.Setup(x => x.GetSystemPermissions()).Returns(permissionsFromMapper);
            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(It.IsAny<string>(), It.IsAny<bool>())).Returns(adInfoAboutUser);
            this.userMediator.Setup(x => x.SaveUser(It.IsAny<UserDTO>())).Returns(5);
            sut.SaveUserPermissionRoles(modelViewsToSave);

            // existing viewer results in delete
            permissionFromMapper.Role = PtmRole.Viewer;
            sut.SaveUserPermissionRoles(modelViewsToSave);

            // do it again with a new user
            this.permissionsMapper.Setup(x => x.GetSystemPermissions()).Returns(new Collection<SystemPermissionDto>());
            sut.SaveUserPermissionRoles(modelViewsToSave);
        }

        /// <summary>
        /// Test validation of permissions
        /// </summary>
        [TestMethod]
        public void C_ValidatePermissionsDataTest()
        {
            var sut = this.CreateSystem();

            Collection<SystemPermissionDto> permissionsFromMapper = new Collection<SystemPermissionDto>();

            SystemPermissionDto permissionFromMapper = new SystemPermissionDto()
            {
                Id = 1,
                Role = PtmRole.Admin,
                UserId = 15
            };

            SystemPermissionDto permissionFromMapper2 = new SystemPermissionDto()
            {
                Id = 2,
                Role = PtmRole.Viewer,
                UserId = 15
            };

            SystemPermissionDto permissionFromMapper3 = new SystemPermissionDto()
            {
                Id = 2,
                Role = PtmRole.Admin,
                UserId = 16
            };

            permissionsFromMapper.Add(permissionFromMapper);
            permissionsFromMapper.Add(permissionFromMapper2);
            permissionsFromMapper.Add(permissionFromMapper3);

            Collection<PtmRole> testRoles = new Collection<PtmRole>();
            PtmRole testRole = PtmRole.Admin;
            PtmRole testRole2 = PtmRole.SystemPricer;

            testRoles.Add(testRole);
            testRoles.Add(testRole2);

            PermissionsModelView permissionsMv = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = testRoles,
                UserId = 15,
                UserName = "test",
                UserNtId = "NtId"
            };

            PermissionsModelView permissionsMv2 = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = testRoles,
                UserId = 18,
                UserName = "test",
                UserNtId = "NtId"
            };

            PermissionsModelView permissionsMv3 = new PermissionsModelView()
            {
                UserType = UserType.User,
                Roles = new Collection<PtmRole>() { PtmRole.SystemPricer },
                UserId = -1,
                UserName = "test",
                UserNtId = "NtId"
            };

            UserDTO testUser = new UserDTO()
            {
                UserType = UserType.User,
                Id = 15,
                Ntid = "NtId"
            };

            this.permissionsMapper.Setup(x => x.GetSystemPermissions()).Returns(permissionsFromMapper);
            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(testUser);
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(testUser);

            ICollection<ValidationMessage> validationErrorsToTest = null;

            Collection<PermissionsModelView> dataToValidate = new Collection<PermissionsModelView>();

            dataToValidate.Add(permissionsMv);

            // successful validation, delete
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(validationErrorsToTest.Count, 0);

            // successful validation, delete multiple admin
            dataToValidate.Add(permissionsMv2);
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(validationErrorsToTest.Count, 0);

            // delete only admin
            permissionsFromMapper.Remove(permissionFromMapper3);
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(validationErrorsToTest.Count, 1);

            dataToValidate.Clear();
            dataToValidate.Add(permissionsMv3);

            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(permissionsMv3.UserNtId, false)).Returns(new UserData());

            // successful validation, new user
            permissionsFromMapper.Remove(permissionFromMapper);
            permissionsMv3.Roles = new Collection<PtmRole>() { PtmRole.Admin };
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(0, validationErrorsToTest.Count);

            // no roles
            permissionsMv3.Roles = new Collection<PtmRole>();
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // Viewer role, no lines of business
            permissionsMv3.Roles = new Collection<PtmRole>() { PtmRole.Viewer };
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // Proposal Setup Admin role, no lines of business
            permissionsMv3.Roles = new Collection<PtmRole>() { PtmRole.ProposalSetupAdmin };
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // null ntid
            permissionsMv3.ViewerLinesOfBusiness = new Collection<int>() { 1, 2 };
            permissionsMv3.ProposalSetupAdminLinesOfBusiness = new Collection<int>() { 2, 3 };
            permissionsMv3.UserNtId = null;
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // empty ntid
            permissionsMv3.UserNtId = " ";
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // invalid group
            permissionsMv3.UserNtId = "my.group";
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);

            // valid group, invalid domain
            this.adUtils.Setup(x => x.IsGroup(permissionsMv3.UserNtId)).Returns(true);
            permissionsMv3.UserNtId = "my.group";
            validationErrorsToTest = sut.ValidatePermissionsToSave(dataToValidate);
            Assert.AreEqual(1, validationErrorsToTest.Count);
        }

        /// <summary>
        /// Test Breakdown Group
        /// </summary>
        [TestMethod]
        public void C_BreakdownGroup_Test()
        {
            var sut = this.CreateSystem();

            this.htmlHelper.Setup(x => x.BreakdownGroup(5)).Returns("<ul><li>Item</li></ul>");

            Assert.AreEqual("<ul><li>Item</li></ul>", sut.BreakdownGroup(5));
        }

        /// <summary>
        /// Test inactive line of business
        /// </summary>
        [TestMethod]
        public void C_AdminInactiveLineOfBusinessTest()
        {
            var sut = this.CreateSystem();

            PickListDto lineOfBusiness1 = new PickListDto()
            {
                Id = 1,
                Text = "a",
                IsActive = true
            };
            PickListDto lineOfBusiness2 = new PickListDto()
            {
                Id = 2,
                Text = "c",
                IsActive = true
            };
            PickListDto lineOfBusiness3 = new PickListDto()
            {
                Id = 3,
                Text = "b",
                IsActive = true
            };

            this.orgStructureMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness1, lineOfBusiness2, lineOfBusiness3 });

            ManagePermissionsModelView model = sut.GetManagePermissionsModelView();

            // should contain 3 active lines of business
            Assert.AreEqual(3, model.ViewerLinesOfBusinessList.Count);

            // items should be sorted by name
            Assert.AreEqual("a", model.ViewerLinesOfBusinessList.ElementAt(0).Text);
            Assert.AreEqual("b", model.ViewerLinesOfBusinessList.ElementAt(1).Text);
            Assert.AreEqual("c", model.ViewerLinesOfBusinessList.ElementAt(2).Text);

            // make 1 line of business inactive
            lineOfBusiness3.IsActive = false;
            model = sut.GetManagePermissionsModelView();

            // should still contain 3 lines of business
            Assert.AreEqual(3, model.ViewerLinesOfBusinessList.Count);
        }

        /// <summary>
        /// Test Get Manage Proposal Info Details View
        /// </summary>
        [TestMethod]
        public void C_GetManageProposalInfoDetailsViewTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                TrackingNumber = "00-00000",
                ProposalTitle = "myTitle",
                UpdateDate = new DateTime(2014, 1, 1),
                ProposalStatus = ProposalStatus.InProgress
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ManageProposalInfoDetailsView manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);

            Assert.AreEqual(proposal.Id, manageProposalInfo.ProposalID);
            Assert.AreEqual(proposal.TrackingNumber, manageProposalInfo.TrackingNumber);
            Assert.AreEqual(proposal.ProposalTitle, manageProposalInfo.ProposalTitle);
            Assert.AreEqual(proposal.UpdateDate, manageProposalInfo.UpdateDate);
            Assert.AreEqual(proposal.ProposalStatus, manageProposalInfo.OldStatus);
            Assert.IsFalse(manageProposalInfo.ShowCompletedSection);
            Assert.IsFalse(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.IsNull(manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.IsNull(manageProposalInfo.NewTotalPrice);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePeer);

            // state transitions covered in C_GetValidStateTransitions

            fullProposal.ProposalStatus = ProposalStatus.Completed;

            DateTime pricerSubmitDate = new DateTime(2014, 1, 1);
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>()
            {
                new ProposalChecklistSaveInfo()
                {
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    SubmitDate = pricerSubmitDate,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(saveInfo);

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                ProposalID = proposal.Id,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22
            };

            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });

            manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);
            Assert.AreEqual(fullProposal.ProposalStatus, manageProposalInfo.OldStatus);
            Assert.IsTrue(manageProposalInfo.ShowCompletedSection);
            Assert.IsFalse(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.AreEqual(proposalChecklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy"), manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.AreEqual(proposalChecklist.SubmittedValue.ToString(), manageProposalInfo.NewTotalPrice);
            Assert.AreEqual(pricerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePeer);

            DateTime peerSubmitDate = new DateTime(2014, 2, 2);
            saveInfo.Add(new ProposalChecklistSaveInfo()
            {
                ChecklistType = ChecklistType.ProposalAdequacyReview,
                SubmitDate = peerSubmitDate,
                ResponseType = ChecklistResponseType.Peer
            });

            manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);
            Assert.IsTrue(manageProposalInfo.ShowCompletedSection);
            Assert.IsTrue(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.AreEqual(proposalChecklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy"), manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.AreEqual(proposalChecklist.SubmittedValue.ToString(), manageProposalInfo.NewTotalPrice);
            Assert.AreEqual(pricerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.AreEqual(peerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePeer);
        }

        /// <summary>
        /// Test Get Manage Proposal Info Details View - Legacy Version using 4 digit year
        /// </summary>
        [TestMethod]
        public void C_GetManageProposalInfoDetailsViewLegacyTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                TrackingNumber = "0000-00000",
                ProposalTitle = "myTitle",
                UpdateDate = new DateTime(2014, 1, 1),
                ProposalStatus = ProposalStatus.InProgress
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            ManageProposalInfoDetailsView manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);

            Assert.AreEqual(proposal.Id, manageProposalInfo.ProposalID);
            Assert.AreEqual(proposal.TrackingNumber, manageProposalInfo.TrackingNumber);
            Assert.AreEqual(proposal.ProposalTitle, manageProposalInfo.ProposalTitle);
            Assert.AreEqual(proposal.UpdateDate, manageProposalInfo.UpdateDate);
            Assert.AreEqual(proposal.ProposalStatus, manageProposalInfo.OldStatus);
            Assert.IsFalse(manageProposalInfo.ShowCompletedSection);
            Assert.IsFalse(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.IsNull(manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.IsNull(manageProposalInfo.NewTotalPrice);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePeer);

            // state transitions covered in C_GetValidStateTransitions

            fullProposal.ProposalStatus = ProposalStatus.Completed;

            DateTime pricerSubmitDate = new DateTime(2014, 1, 1);
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>()
            {
                new ProposalChecklistSaveInfo()
                {
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    SubmitDate = pricerSubmitDate,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(saveInfo);

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                ProposalID = proposal.Id,
                EstimatingSubmitsToContractsDate = new DateTime(2014, 5, 2),
                SubmittedValue = 22
            };

            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });

            manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);
            Assert.AreEqual(fullProposal.ProposalStatus, manageProposalInfo.OldStatus);
            Assert.IsTrue(manageProposalInfo.ShowCompletedSection);
            Assert.IsFalse(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.AreEqual(proposalChecklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy"), manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.AreEqual(proposalChecklist.SubmittedValue.ToString(), manageProposalInfo.NewTotalPrice);
            Assert.AreEqual(pricerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.IsNull(manageProposalInfo.NewChecklistSubmittedDatePeer);

            DateTime peerSubmitDate = new DateTime(2014, 2, 2);
            saveInfo.Add(new ProposalChecklistSaveInfo()
            {
                ChecklistType = ChecklistType.ProposalAdequacyReview,
                SubmitDate = peerSubmitDate,
                ResponseType = ChecklistResponseType.Peer
            });

            manageProposalInfo = sut.GetManageProposalInfoDetailsView(proposalId);
            Assert.IsTrue(manageProposalInfo.ShowCompletedSection);
            Assert.IsTrue(manageProposalInfo.ShowChecklistSubmittedDatePeer);
            Assert.AreEqual(proposalChecklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy"), manageProposalInfo.EstimatingSubmitsToContractsDate);
            Assert.AreEqual(proposalChecklist.SubmittedValue.ToString(), manageProposalInfo.NewTotalPrice);
            Assert.AreEqual(pricerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePricer);
            Assert.AreEqual(peerSubmitDate.ToString("MM/dd/yyyy"), manageProposalInfo.NewChecklistSubmittedDatePeer);
        }

        /// <summary>
        /// Test Save Manage Proposal Info
        /// </summary>
        [TestMethod]
        public void C_SaveManageProposalInfoTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ManageProposalInfoDetailsView manageProposalInfo = new ManageProposalInfoDetailsView()
            {
                ProposalID = proposalId,
                NewStatus = ProposalStatus.Completed
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                ProposalStatus = ProposalStatus.Completed
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            this.manageProposalInfoLoader.Setup(x => x.SaveProposalInfo(It.IsAny<ManageProposalInfoDto>())).Returns(proposalId);

            int? savedProposalId = sut.SaveManageProposalInfo(proposalId, manageProposalInfo);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.manageProposalInfoLoader.Verify(x => x.SaveProposalInfo(It.IsAny<ManageProposalInfoDto>()), Times.Once());

            // different values
            manageProposalInfo = new ManageProposalInfoDetailsView()
            {
                ProposalID = proposalId,
                NewStatus = ProposalStatus.Completed,
                OldProposalSubmittalDate = "01/01/2013",
                EstimatingSubmitsToContractsDate = "01/01/2014",
                OldTotalPrice = "1",
                NewTotalPrice = "10",
                OldChecklistSubmittedDatePricer = "01/01/2013",
                NewChecklistSubmittedDatePricer = "01/01/2014",
                OldChecklistSubmittedDatePeer = "01/01/2013",
                NewChecklistSubmittedDatePeer = "01/01/2014",
                Comments = "Test"
            };

            // changed save info, all values should not be null
            savedProposalId = sut.SaveManageProposalInfo(proposalId, manageProposalInfo);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.manageProposalInfoLoader.Verify(x => x.SaveProposalInfo(
                It.Is<ManageProposalInfoDto>(y => y.EstimatingSubmitsToContractsDate != null && y.TotalPrice != null &&
                y.ChecklistSubmittedDatePricer != null && y.ChecklistSubmittedDatePeer != null && y.Comments == "Test")), Times.Once());

            // equivalent values
            manageProposalInfo = new ManageProposalInfoDetailsView()
            {
                ProposalID = proposalId,
                NewStatus = ProposalStatus.Completed,
                OldProposalSubmittalDate = "01/01/2013",
                EstimatingSubmitsToContractsDate = "1/1/2013",
                OldTotalPrice = "10000",
                NewTotalPrice = "10,000",
                OldChecklistSubmittedDatePricer = "01/01/2013",
                NewChecklistSubmittedDatePricer = "01/1/2013",
                OldChecklistSubmittedDatePeer = "01/01/2013",
                NewChecklistSubmittedDatePeer = "01/01/2013"
            };

            // equivalent save info, all values should be null
            savedProposalId = sut.SaveManageProposalInfo(proposalId, manageProposalInfo);
            Assert.IsNotNull(savedProposalId);
            Assert.AreEqual(proposalId, savedProposalId.Value);
            this.manageProposalInfoLoader.Verify(x => x.SaveProposalInfo(
                It.Is<ManageProposalInfoDto>(y => y.EstimatingSubmitsToContractsDate == null && y.TotalPrice == null &&
                y.ChecklistSubmittedDatePricer == null && y.ChecklistSubmittedDatePeer == null)), Times.Exactly(2));
        }

        /// <summary>
        /// Test Save Manage Proposal Info with an invalid transition
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void C_SaveManageProposalInfoInvalidTransitionTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ManageProposalInfoDetailsView manageProposalInfo = new ManageProposalInfoDetailsView()
            {
                ProposalID = proposalId,
                NewStatus = ProposalStatus.Completed
            };

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                ProposalStatus = ProposalStatus.InProgress
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            sut.SaveManageProposalInfo(proposalId, manageProposalInfo);
        }

        /// <summary>
        /// Test Validate Manage Proposal Info View
        /// </summary>
        [TestMethod]
        public void C_ValidateManageProposalInfoTest()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ManageProposalInfoDetailsView manageProposalInfo = new ManageProposalInfoDetailsView()
            {
                ProposalID = proposalId,
                OldStatus = ProposalStatus.Completed,
                EstimatingSubmitsToContractsDate = null,
                NewTotalPrice = null,
                NewChecklistSubmittedDatePricer = null,
                NewChecklistSubmittedDatePeer = null,
                ShowChecklistSubmittedDatePeer = false
            };

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            sut.ValidateManageProposalInfo(manageProposalInfo, validationMessages);

            // required fields without peer
            Assert.AreEqual(3, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.SUBMITTAL_DATE_REQUIRED)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.TOTAL_PRICE_REQUIRED)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PRICER_REQUIRED)).Count());

            manageProposalInfo.ShowChecklistSubmittedDatePeer = true;

            validationMessages.Clear();
            sut.ValidateManageProposalInfo(manageProposalInfo, validationMessages);

            // required fields with peer
            Assert.AreEqual(4, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.SUBMITTAL_DATE_REQUIRED)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.TOTAL_PRICE_REQUIRED)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PRICER_REQUIRED)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PEER_REQUIRED)).Count());

            manageProposalInfo.EstimatingSubmitsToContractsDate = "123";
            manageProposalInfo.NewChecklistSubmittedDatePricer = "123";
            manageProposalInfo.NewChecklistSubmittedDatePeer = "123";
            manageProposalInfo.NewTotalPrice = "999,999,999";

            validationMessages.Clear();
            sut.ValidateManageProposalInfo(manageProposalInfo, validationMessages);

            // invalid formats
            Assert.AreEqual(3, validationMessages.Count);
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.SUBMITTAL_DATE_FORMAT)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PRICER_FORMAT)).Count());
            Assert.AreEqual(1, validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PEER_FORMAT)).Count());
           
            manageProposalInfo.EstimatingSubmitsToContractsDate = "01/01/2014";
            manageProposalInfo.NewTotalPrice = "123";
            manageProposalInfo.NewChecklistSubmittedDatePricer = "01/01/2014";
            manageProposalInfo.NewChecklistSubmittedDatePeer = "01/01/2014";

            validationMessages.Clear();
            sut.ValidateManageProposalInfo(manageProposalInfo, validationMessages);

            // validation success
            Assert.AreEqual(0, validationMessages.Count);
        }

        /// <summary>
        /// Test Get Valid State Transitions
        /// </summary>
        [TestMethod]
        public void C_GetValidStateTransitions()
        {
            var sut = this.CreateSystem();

            int proposalId = 15;

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId,
                ProposalStatus = ProposalStatus.InProgress
            };

            FullProposal fullProposal = new FullProposal(proposal);

            // InProgress
            List<ProposalStatus> validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Completed
            fullProposal.ProposalStatus = ProposalStatus.Completed;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));

            // No Bid
            fullProposal.ProposalStatus = ProposalStatus.NoBid;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.NoBid));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));

            int contentId = 1;
            ChecklistContentDto checklistContent = new ChecklistContentDto()
            {
                Version = 1,
                Content = new Collection<ChecklistContentItem>()
                {
                    new ChecklistContentItem()
                    {
                        Id = contentId,
                        ChecklistType = ChecklistType.ProposalPricingReview,
                        SortOrder = 1,
                        TextType = ChecklistTextType.Question
                    }
                }
            };

            this.retriever.Setup(x => x.GetPPRChecklistContent(proposalId)).Returns(checklistContent);

            // Archived with no checklist responses
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Deleted with no checklist responses
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            ChecklistResponseItem checklistResponse = new ChecklistResponseItem()
            {
                ChecklistContentId = contentId,
                Response = ChecklistResponseOption.Yes
            };

            ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
            {
                PPRResponses = new Collection<ChecklistResponseItem>() { checklistResponse }
            };

            this.retriever.Setup(x => x.GetProposalChecklists(proposalId)).Returns(new Collection<ProposalChecklistDto> { proposalChecklist });

            ProposalChecklistSaveInfo pricerSaveInfo = new ProposalChecklistSaveInfo()
            {
                ChecklistType = ChecklistType.ProposalPricingReview,
                SubmitDate = null,
                ResponseType = ChecklistResponseType.Pricer
            };

            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>() { pricerSaveInfo };
            this.retriever.Setup(x => x.GetAllChecklistSaveInfo(proposalId)).Returns(saveInfo);

            // Archived with no PAR and no pricer submit (InProgress)
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Deleted with no PAR and no pricer submit (InProgress)
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            pricerSaveInfo.SubmitDate = new DateTime(2014, 1, 1);

            // Archived with no PAR and pricer submit (Completed)
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Deleted with no PAR and pricer submit (Completed)
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            ProposalChecklistSaveInfo peerSaveInfo = new ProposalChecklistSaveInfo()
            {
                ChecklistType = ChecklistType.ProposalAdequacyReview,
                SubmitDate = null,
                ResponseType = ChecklistResponseType.Peer
            };
            saveInfo.Add(peerSaveInfo);
            checklistResponse.Response = ChecklistResponseOption.No;

            // Archived with PAR, pricer submit and no peer submit (InProgress)
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Deleted with PAR, pricer submit and no peer submit (InProgress)
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            peerSaveInfo.SubmitDate = new DateTime(2014, 1, 1);

            // Archived with PAR, pricer submit and peer submit (Completed)
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Deleted with PAR, pricer submit and peer submit (Completed)
            fullProposal.ProposalStatus = ProposalStatus.Deleted;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));

            // Archived for migrated data, can return to Completed
            checklistContent.Version = 0;
            fullProposal.ProposalStatus = ProposalStatus.Archived;
            validStates = sut.GetValidStateTransitions(fullProposal);
            Assert.IsTrue(validStates.Contains(ProposalStatus.Archived));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Deleted));
            Assert.IsTrue(validStates.Contains(ProposalStatus.Completed));
            Assert.IsFalse(validStates.Contains(ProposalStatus.InProgress));
            Assert.IsFalse(validStates.Contains(ProposalStatus.Revised));
        }

        /// <summary>
        /// Test Get Proposal Id by Tracking Number
        /// </summary>
        [TestMethod]
        public void C_GetProposalIdByTrackingNumberTest()
        {
            var sut = this.CreateSystem();
            string trackingNumber = "00-00000";
            int proposalId = 15;
            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(trackingNumber)).Returns(proposalId);

            int toTest = sut.GetProposalIdByTrackingNumber(trackingNumber);
            Assert.AreEqual(proposalId, toTest);
        }

        /// <summary>
        /// Test a successful Bulk Archive Request
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequest()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                EndDate = "02/02/2013"
            };

            List<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(0, validationErrorsToTest.Count);
        }

        /// <summary>
        /// Test a Bulk Archive Request with a Start Date later than the End Date.
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequestStartDateLater()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                EndDate = "01/01/2012"
            };

            ICollection<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(1, validationErrorsToTest.Count);
            Assert.AreEqual(1, validationErrorsToTest.Where(x => x.ValidationIssue.Contains(ValidationConstants.AdminValidationConstants.START_DATE_BEFORE_END_DATE)).Count());
        }

        /// <summary>
        /// Test a Bulk Archive Request with empty Dates
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequestEmptyDates()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = string.Empty,
                EndDate = string.Empty
            };

            ICollection<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(0, validationErrorsToTest.Count);
        }

        /// <summary>
        /// Test a Bulk Archive Request with empty Null Dates
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequestNullDates()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = null,
                EndDate = null
            };

            ICollection<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(0, validationErrorsToTest.Count);
        }

        /// <summary>
        /// Test a Bulk Archive Request with bad Date Formats
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequestBadDateFormats()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "bad",
                EndDate = "bad"
            };

            ICollection<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(1, validationErrorsToTest.Where(x => x.ValidationIssue.Contains(ValidationConstants.AdminValidationConstants.CREATED_DATE_FORMAT)).Count());
        }

        /// <summary>
        /// Test a Bulk Archive Request with a bad Start Date Format
        /// </summary>
        [TestMethod]
        public void C_ValidateBulkArchiveRequestBadStartDateFormat()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "bad",
                EndDate = "01/01/2013"
            };

            ICollection<ValidationMessage> validationErrorsToTest = new List<ValidationMessage>();

            sut.ValidateBulkArchiveRequest(model, validationErrorsToTest);

            Assert.AreEqual(1, validationErrorsToTest.Count);
            Assert.AreEqual(1, validationErrorsToTest.Where(x => x.ValidationIssue.Contains(ValidationConstants.AdminValidationConstants.CREATED_DATE_FORMAT)).Count());
        }

        /// <summary>
        /// Test the Search Bulk Archive method with a provided Start and End date.
        /// </summary>
        [TestMethod]
        public void C_SearchBulkArchiveTestDatesSpecified()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                EndDate = "05/01/2013",
                LineOfBusiness = "All",
                ProgramArea = "All"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.SearchBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.SearchBulkArchive(model));
        }

        /// <summary>
        /// Test the Search Bulk Archive method with no dates provided.
        /// </summary>
        [TestMethod]
        public void C_SearchBulkArchiveTestNoDates()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                LineOfBusiness = "1",
                ProgramArea = "2"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.SearchBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.SearchBulkArchive(model));
        }

        /// <summary>
        /// Test the Search Bulk Archive method with only start date provided
        /// </summary>
        [TestMethod]
        public void C_SearchBulkArchiveTestOnlyStartDate()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                LineOfBusiness = "1",
                ProgramArea = "2"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.SearchBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.SearchBulkArchive(model));
        }

        /// <summary>
        /// Test the Apply Bulk Archive method with dates provided.
        /// </summary>
        [TestMethod]
        public void C_ApplyBulkArchiveTestBothDates()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                EndDate = "05/01/2013",
                LineOfBusiness = "1",
                ProgramArea = "2"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.ApplyBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.ApplyBulkArchive(model));
        }

        /// <summary>
        /// Test the Apply Bulk Archive method with no dates provided.
        /// </summary>
        [TestMethod]
        public void C_ApplyBulkArchiveTestNoDates()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                LineOfBusiness = "1",
                ProgramArea = "2"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.ApplyBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.ApplyBulkArchive(model));
        }

        /// <summary>
        /// Test the Apply Bulk Archive method with only a StartDate provided
        /// </summary>
        [TestMethod]
        public void C_ApplyBulkArchiveTestOnlyStartDate()
        {
            var sut = this.CreateSystem();

            BulkArchiveModelView model = new BulkArchiveModelView()
            {
                StartDate = "01/01/2013",
                LineOfBusiness = "1",
                ProgramArea = "2"
            };

            int numberResults = 1;

            this.bulkArchiveLoader.Setup(x => x.ApplyBulkArchive(It.IsAny<BulkArchiveDto>())).Returns(numberResults);

            Assert.AreEqual(numberResults, sut.ApplyBulkArchive(model));
        }

         /// <summary>
        /// Test the GetBulkArchiveModelView() method. Verify that the Line of Business 
        /// and Program Area counts. 
        /// </summary>
        [TestMethod]
        public void C_GetBulkArchiveModelView()
        {
            var sut = this.CreateSystem();

            ICollection<PickListDto> lineOfBusinessSelectItems = new List<PickListDto>();

            PickListDto lineOfBusinessDto = new PickListDto()
            {
                Id = 1,
                Text = "ISGS_LineOfBusiness"
            };

            lineOfBusinessSelectItems.Add(lineOfBusinessDto);

            this.orgStructureMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(lineOfBusinessSelectItems);

            var result = sut.GetBulkArchiveModelView();

            Assert.AreEqual(2, result.LineOfBusinessOptions.Count());
            Assert.AreEqual("All", result.LineOfBusinessOptions.ElementAt(0).Text);
            Assert.AreEqual(lineOfBusinessDto.Text, result.LineOfBusinessOptions.ElementAt(1).Text);
            Assert.AreEqual(1, result.ProgramAreaOptions.Count());
            Assert.AreEqual("All", result.ProgramAreaOptions.ElementAt(0).Text);
        }        

        /// <summary>
        /// Test the GetProgramAreasForLineOfBusinessForBulkArchive() method
        /// with a null argument for the Line of Business.
        /// </summary>
        [TestMethod]
        public void C_GetProgramAreasForLineOfBusinessForBulkArchive_NullInput()
        {
            var sut = this.CreateSystem();

            var result = sut.GetProgramAreasForLineOfBusinessForBulkArchive(null);

            Assert.AreEqual(0, result.Count());
        }

        /// <summary>
        /// Test the GetProgramAreasForLineOfBusinessForBulkArchive() method
        /// with a "All" argument for the Line of Business.
        /// </summary>
        [TestMethod]
        public void C_GetProgramAreasForLineOfBusinessForBulkArchive_AllInput()
        {
            var sut = this.CreateSystem();

            var result = sut.GetProgramAreasForLineOfBusinessForBulkArchive("All");

            var expectedValue = string.Format("<option value=\"{0}\">{1}</option>", 0, "All");

            Assert.AreEqual(expectedValue, result);
        }

        /// <summary>
        /// Test the GetProgramAreasForLineOfBusinessForBulkArchive() method
        /// with a single Line of Business argument.
        /// </summary>
        [TestMethod]
        public void C_GetProgramAreasForLineOfBusinessForBulkArchive_SingleLineOfBusiness()
        {
            var sut = this.CreateSystem();

            IList<PickListDto> programAreaDtoList = new List<PickListDto>();

            PickListDto programAreaDto = new PickListDto()
            {
                Id = 1,
                ParentIds = new int[] { 1 },
                Text = "Program Area",
                IsActive = true
            };

            programAreaDtoList.Add(programAreaDto);

            this.orgStructureMapper.Setup(x => x.GetAllProgramAreas()).Returns(programAreaDtoList);

            PickListDto lineOfBusinessDto = new PickListDto()
            {
                Id = 1,
                Text = "ISGS Line of Business"
            };

            this.orgStructureMapper.Setup(x => x.GetLineOfBusinessById(1)).Returns(lineOfBusinessDto);

            StringBuilder expectedResult = new StringBuilder();

            expectedResult.Append(string.Format("<option value=\"{0}\">{1}</option>", 0, "All"));
            expectedResult.Append(string.Format("<option value=\"{0}\">{1}</option>",
                        programAreaDto.Id, programAreaDto.Text));

            var result = sut.GetProgramAreasForLineOfBusinessForBulkArchive("1");

            Assert.AreEqual(expectedResult.ToString(), result);
        }    

        #region Exception Tests

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_GetManagePermissionsGridPagedModelView_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.GetManagePermissionsGridModelView(null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidatePermissionsToSave_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.ValidatePermissionsToSave(null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SaveManageProposalInfo_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.SaveManageProposalInfo(1, null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateManageProposalInfo_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.ValidateManageProposalInfo(null, new List<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateManageProposalInfo_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.ValidateManageProposalInfo(new ManageProposalInfoDetailsView(), null);
        }

        /// <summary>
        /// Test a Bulk Archive Request with a null Bulk Archive Model
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateBulkArchiveRequestNullModel_ExceptionTest1()
        {
            var sut = this.CreateSystem();
            sut.ValidateBulkArchiveRequest(null, new List<ValidationMessage>());
        }

        /// <summary>
        /// Test a Bulk Archive Request with a null validation messages list
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateBulkArchiveRequestNullModel_ExceptionTest2()
        {
            var sut = this.CreateSystem();
            sut.ValidateBulkArchiveRequest(new BulkArchiveModelView(), null);
        }

        /// <summary>
        /// Test the SearchBulkArchive() method with a null model view
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_SearchBulkArchive_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.SearchBulkArchive(null);
        }

        /// <summary>
        /// Test the ApplyBulkArchive() method with a null model view
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ApplyBulkArchive_ExceptionTest()
        {
            var sut = this.CreateSystem();
            sut.ApplyBulkArchive(null);
        }

        #endregion Exception Tests

        /// <summary>
        /// Asserts Model View
        /// </summary>
        /// <param name="actual">actual model view</param>
        /// <param name="expected">expected model view</param>
        private void AssertModelView(PermissionsGridModelView actual, PermissionsGridModelView expected)
        {
            Assert.AreEqual(expected.PermissionData.Count(), actual.PermissionData.Count());

            foreach (PermissionsModelView actualPermission in actual.PermissionData)
            {
                // Only verify fields we fill in.
                PermissionsModelView expectedPermission = expected.PermissionData.Where(x => x.UserId == actualPermission.UserId).FirstOrDefault();
                Assert.AreEqual(actualPermission.UserName, expectedPermission.UserName);
                Assert.AreEqual(actualPermission.UserId, expectedPermission.UserId);
                Assert.AreEqual(actualPermission.UserType, expectedPermission.UserType);

                Assert.AreEqual(actualPermission.Roles.Count(), expectedPermission.Roles.Count());
                foreach (PtmRole actualRole in actualPermission.Roles)
                {
                    Assert.IsTrue(expectedPermission.Roles.Contains(actualRole));
                }

                Assert.AreEqual(actualPermission.UsersInAGroup.Count(), expectedPermission.UsersInAGroup.Count());
                foreach (string actualUsersInGroup in actualPermission.UsersInAGroup)
                {
                    Assert.IsTrue(expectedPermission.UsersInAGroup.Contains(actualUsersInGroup));
                }

                Assert.AreEqual(actualPermission.ProposalSetupAdminLinesOfBusiness.Count(), expectedPermission.ProposalSetupAdminLinesOfBusiness.Count());
                foreach (int actualProposalSetupAdminLOBs in actualPermission.ProposalSetupAdminLinesOfBusiness)
                {
                    Assert.IsTrue(expectedPermission.ProposalSetupAdminLinesOfBusiness.Contains(actualProposalSetupAdminLOBs));
                }

                Assert.AreEqual(actualPermission.ViewerLinesOfBusiness.Count(), expectedPermission.ViewerLinesOfBusiness.Count());
                foreach (int actualViewerLOB in actualPermission.ViewerLinesOfBusiness)
                {
                    Assert.IsTrue(expectedPermission.ViewerLinesOfBusiness.Contains(actualViewerLOB));
                }

                Assert.AreEqual(expectedPermission.ProposalSetupAdminLinesOfBusinessString, expectedPermission.ProposalSetupAdminLinesOfBusinessString);
                Assert.AreEqual(expectedPermission.ViewerLinesOfBusinessString, actualPermission.ViewerLinesOfBusinessString);
            }
        }

        #region Validate Proposal Delete

        /// <summary>
        /// Test ValidateProposalDelete with null validationErrors parameter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposalDelete_EX1()
        {
            var sut = this.CreateSystem();
            sut.ValidateDeleteProposal(1, null);
        }

        /// <summary>
        /// Test ValidateProposalDelete null proposalId parameter
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_EX2()
        {
            var sut = this.CreateSystem();
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            sut.ValidateDeleteProposal(null, validationErrors);

            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual(validationErrors.First().ValidationIssue, ValidationConstants.DeleteProposalValidationConstants.NULL_PROPOSAL_ID);
        }

        /// <summary>
        /// Test ValidateProposalDelete invalid proposalId parameter
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_InvalidProposalId()
        {
            var sut = this.CreateSystem();
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();
            sut.ValidateDeleteProposal(9999, validationErrors);

            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual(validationErrors.First().ValidationIssue, ValidationConstants.DeleteProposalValidationConstants.INVALID_PROPOSAL_ID);
        }

        /// <summary>
        /// Test ValidateProposalDelete with linked RDSB document
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_LinkedRdsbDocument()
        {
            var sut = this.CreateSystem();
            string trackingNumber1 = "18-11111";
            string trackingNumber2 = "18-22222";
            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = 1234,
                TrackingNumber = trackingNumber2
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal

            // setup BOE workspaces
            Collection<GenBOE.Dtos.WorkspaceDTO> workspaces = new Collection<GenBOE.Dtos.WorkspaceDTO>();
            GenBOE.Dtos.WorkspaceDTO wsOne = new GenBOE.Dtos.WorkspaceDTO();
            wsOne.WorkspaceName = "Workspace One";
            wsOne.Id = 1;
            wsOne.TrackingNumber = trackingNumber1;
            workspaces.Add(wsOne);

            // Setup Method
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            sut.ValidateDeleteProposal(proposalId, validationErrors);

            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual(validationErrors.First().ValidationIssue, ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_RDSB_DOCUMENT);
        }

        /// <summary>
        /// Test ValidateProposalDelete with linked BOE workspace
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_LinkedBoeWorkspace()
        {
            var sut = this.CreateSystem();
            string trackingNumber1 = "18-11111";
            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null,
                TrackingNumber = trackingNumber1
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal

            // setup BOE workspaces
            Collection<GenBOE.Dtos.WorkspaceDTO> workspaces = new Collection<GenBOE.Dtos.WorkspaceDTO>();
            GenBOE.Dtos.WorkspaceDTO wsOne = new GenBOE.Dtos.WorkspaceDTO();
            wsOne.WorkspaceName = "Workspace One";
            wsOne.Id = 1;
            wsOne.TrackingNumber = trackingNumber1;
            workspaces.Add(wsOne);

            // Setup Method
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            sut.ValidateDeleteProposal(proposalId, validationErrors);

            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual(validationErrors.First().ValidationIssue, ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_BOE_WORKSPACE);
        }

        /// <summary>
        /// Test ValidateProposalDelete with linked RDSB document and BOE workspace
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_LinkedRdsbDocumentAndBoeWorkspace()
        {
            var sut = this.CreateSystem();
            string trackingNumber1 = "18-11111";
            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = 1234,
                TrackingNumber = trackingNumber1
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal

            // setup BOE workspaces
            Collection<GenBOE.Dtos.WorkspaceDTO> workspaces = new Collection<GenBOE.Dtos.WorkspaceDTO>();
            GenBOE.Dtos.WorkspaceDTO wsOne = new GenBOE.Dtos.WorkspaceDTO();
            wsOne.WorkspaceName = "Workspace One";
            wsOne.Id = 1;
            wsOne.TrackingNumber = trackingNumber1;
            workspaces.Add(wsOne);

            // Setup Method
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            sut.ValidateDeleteProposal(proposalId, validationErrors);

            Assert.IsTrue(validationErrors.Count == 2);
            Assert.IsTrue(validationErrors.Any(x => x.ValidationIssue.Equals(ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_RDSB_DOCUMENT)));
            Assert.IsTrue(validationErrors.Any(x => x.ValidationIssue.Equals(ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_BOE_WORKSPACE)));
        }

        /// <summary>
        /// Test ValidateProposalDelete happy path
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalDelete_HappyPath()
        {
            var sut = this.CreateSystem();
            string trackingNumber1 = "18-11111";
            string trackingNumber2 = "18-22222";
            int? proposalId = 5;
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            ProposalDto proposal = new ProposalDto()
            {
                Id = proposalId.Value,
                ProposalStatus = ProposalStatus.InProgress,
                DocumentId = null,
                TrackingNumber = trackingNumber2
            };

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposalId.Value)).Returns(proposal); // GetFullProposal
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal); // GetFullProposal

            // setup BOE workspaces
            Collection<GenBOE.Dtos.WorkspaceDTO> workspaces = new Collection<GenBOE.Dtos.WorkspaceDTO>();
            GenBOE.Dtos.WorkspaceDTO wsOne = new GenBOE.Dtos.WorkspaceDTO();
            wsOne.WorkspaceName = "Workspace One";
            wsOne.Id = 1;
            wsOne.TrackingNumber = trackingNumber1;
            workspaces.Add(wsOne);

            // Setup Method
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            sut.ValidateDeleteProposal(proposalId, validationErrors);

            Assert.IsTrue(validationErrors.Count == 0);
        }
        #endregion
    }
}
