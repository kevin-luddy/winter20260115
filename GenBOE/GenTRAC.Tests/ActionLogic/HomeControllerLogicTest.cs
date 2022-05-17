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
    using System.Net;
    using GenBOE.DataBridge.DTO;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.CacheWarming;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Home;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Home Controller logic test
    /// </summary>
    /// 
    [TestClass]
    public class HomeControllerLogicTest
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
        /// Cache
        /// </summary>
        private Mock<IES.Common.ICache> cache = null;

        /// <summary>
        /// Cache warmer
        /// </summary>
        private Mock<ICacheWarmer> cacheWarmer = null;

        /// <summary>
        /// Approvals loader
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
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator;

        /// <summary>
        /// The workspace loader
        /// </summary>
        private Mock<IWorkspaceDTODataLoader> workspaceLoader;

        /// <summary>
        /// The BOE security loader
        /// </summary>
        private Mock<GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader> boeSecurityLoader;

        /// <summary>
        /// The BOE security access
        /// </summary>
        private Mock<GenBOE.DataBridge.Common.Interfaces.ISecurityAccess> boeSecurityAccess;
        
        #endregion

        /// <summary>
        /// Home Controller logic under test
        /// </summary>
        /// <returns>home controller logic</returns>
        private HomeControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.adUtils = new Mock<IES.Common.IActiveDirectoryUtilities>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.orgStructureDataMapper = new Mock<IOrgStructureDataMapper>();
            this.cache = new Mock<IES.Common.ICache>();
            this.cacheWarmer = new Mock<ICacheWarmer>();
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.boeSecurityAccess = new Mock<GenBOE.DataBridge.Common.Interfaces.ISecurityAccess>();
            this.boeSecurityLoader = new Mock<GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader>();

            this.boeSecurityLoader.Setup(x => x.GetPermissionsForUser(It.IsAny<string>())).Returns(new Collection<GenBOE.DataBridge.Common.SecurityPermissionsResponse>()); 
            this.boeSecurityAccess.Setup(x => x.IsAuthorized(It.IsAny<GenBOE.DataBridge.Common.SecurityPermissionsRequested>(), It.IsAny<GenBOE.Dtos.WorkspaceDTO>(), It.IsAny<IReadOnlyCollection<GenBOE.DataBridge.Common.SecurityPermissionsResponse>>())).Returns(IES.Common.SecurityAuthorization.None);

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            Mock<IES.Common.SecurityInformation> security = new Mock<IES.Common.SecurityInformation>(this.adUtils.Object);
            IES.Common.UserData user = new IES.Common.UserData()
            {
                Email = "james.basilio@lmco.com"
            };
            security.Setup(x => x.ActiveUserData).Returns(user);

            return new HomeControllerLogic(this.securityAccess.Object, this.proposalLoader.Object,
                this.userMapper.Object, this.adUtils.Object, this.objectFactory.Object, this.orgStructureDataMapper.Object,
                this.cache.Object, this.cacheWarmer.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, 
                this.checklistMediator.Object, this.proposalMediator.Object, this.workspaceLoader.Object, 
                this.boeSecurityLoader.Object, this.boeSecurityAccess.Object);
        }

        /// <summary>
        ///  Get Data for Home Proposal test
        /// </summary>
        /// SUPPRESSION NOTE: Need this many classes to test HomecontrollerLogic
        [TestMethod]
        public void BasicGetDataForHomeProposalTest()
        {
            HomeControllerLogic sut = this.CreateSystem();

            // Proposal
            HomeProposalViewDto proposal = new HomeProposalViewDto()
            {
                TrackingNumber = "2013KG",
                ProposalId = 55,
                Status = ProposalStatus.InProgress,
                ProposalTitle = "TestProposal",
                EstValue = 500,
                SubmittedValue = 505,
                ProposalSubmittalDate = new DateTime(2014, 5, 2),
                ProgramArea = "C_MOCK",
                ProposalDateAssigned = Convert.ToDateTime("12/12/2014"),
                CaptureManagerDisplayName = "Cheese whiz",
                PricerDisplayName = "Reed Derr",
                PeerReviewerDisplayName = "Bar, Foo",
                CostVolumeLeadDisplayName = "Bar, Foo",
                ChecklistCompleteDate = new DateTime(2014, 3, 3),
                Customer = "SSC",
                HasLinkedDocument = true,
                IsCommercialCustomer = false
            };

            ICollection<HomeProposalViewDto> proposals = new Collection<HomeProposalViewDto>();
            proposals.Add(proposal);

            // set up any sorts/filters
            string sortField = null;
            System.Data.SqlClient.SortOrder? sortOrder = null;
            ProposalFiltersCookie cookie = new ProposalFiltersCookie();
            string searchText = null;

            string programAreaHelpText = "helptext";

            // Pricer
            UserDTO userPricer = new UserDTO
            {
                Id = 21,
                FirstName = "Reed",
                LastName = "Derr",
                DisplayName = "Reed Derr",
                Ntid = "reader",
                UserType = IES.Common.UserType.User
            };

            // setup mock data
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(userPricer);
            this.proposalLoader.Setup(x => x.GetProposalsByUser(1, null, null, null, userPricer.Ntid, false, string.Empty, null)).Returns(proposals);
            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaDynamicHelpText()).Returns(programAreaHelpText);
            this.workspaceLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(new Collection<GenBOE.Dtos.WorkspaceDTO>());

            HomeProposalModelView homeProposalView = sut.GetRolesForNewProposal();

            // Asserts
            Assert.IsFalse(homeProposalView.AllowNewProposal);

            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);

            // Asserts
            Assert.AreEqual(programAreaHelpText, homeProposalView.ProgramAreaHelpText);
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalTitle, proposal.ProposalTitle, "Proposal Title mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalId, proposal.ProposalId, "Proposal ID mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).TrackingNumber, proposal.TrackingNumber, "Proposal tracking number mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ForecastedTrackingNumber, proposal.ForecastedTrackingNumber, "Forecasted tracking number mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).EstValue, proposal.EstValue, "Proposal est value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).EstValueString, proposal.EstValue.ToString(), "Proposal est value string mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).SubmittedValue, proposal.SubmittedValue, "Proposal submitted value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).SubmittedValueString, proposal.SubmittedValue.ToString(), "Proposal submitted value string mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).CaptureManagerDisplayName, proposal.CaptureManagerDisplayName, "Capture Manager Display Name mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).PricerDisplayName, proposal.PricerDisplayName, "Pricer name mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).PeerReviewerDisplayName, proposal.PeerReviewerDisplayName, "Peer Reviewer name mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).CostVolumeLeadDisplayName, proposal.CostVolumeLeadDisplayName, "Cost Volume Lead name mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalDateAssigned.Value.ToString("MM/dd/yyyy"), proposal.ProposalDateAssigned.Value.ToString("MM/dd/yyyy"), "Proposal Date Assigned mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalDateAssignedString, proposal.ProposalDateAssigned.Value.ToString("MM/dd/yyyy"), "Proposal Date Assigned mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalDueDate.ToString("MM/dd/yyyy"), proposal.ProposalDueDate.ToString("MM/dd/yyyy"), "Proposal Due Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalDueDateString, proposal.ProposalDueDate.ToString("MM/dd/yyyy"), "Proposal Due Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalCompletedDate.Value.ToString("MM/dd/yyyy"), proposal.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy"), "Proposal Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalCompletedDateString, proposal.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy"), "Proposal Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ChecklistCompleteDate.Value.ToString("MM/dd/yyyy"), proposal.ChecklistCompleteDate.Value.ToString("MM/dd/yyyy"), "Checklist Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ChecklistCompleteDateString, proposal.ChecklistCompleteDate.Value.ToString("MM/dd/yyyy"), "Checklist Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).Status, proposal.Status, "proposal status mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProgramArea, proposal.ProgramArea, "program area mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).HasLinkedDocument, proposal.HasLinkedDocument, "Has Linked Document mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).IsCommercialCustomer, proposal.IsCommercialCustomer, "Is Commercial Customer mismatch");
            Assert.IsFalse(homeProposalView.CanCreateBOEWorkspace);

            // test additional field formatting and colors
            proposal.ProposalSubmittalDate = null;
            proposal.ChecklistCompleteDate = null;
            proposal.CostVolumeLeadDisplayName = string.Empty;
            proposal.PeerReviewerDisplayName = string.Empty;
            proposal.EstValue = 4000;
            // due date in past, should be red
            proposal.ProposalDueDate = DateTime.Now - TimeSpan.FromDays(5);

            // Enable can create workspace
            this.boeSecurityAccess.Setup(x => x.IsAuthorized(It.IsAny<GenBOE.DataBridge.Common.SecurityPermissionsRequested>(), It.IsAny<GenBOE.Dtos.WorkspaceDTO>(), It.IsAny<IReadOnlyCollection<GenBOE.DataBridge.Common.SecurityPermissionsResponse>>())).Returns(IES.Common.SecurityAuthorization.CreateReadUpdateDelete);

            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);

            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).EstValueString, "4,000", "Proposal est value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).DueDateBackground, Constants.RED_BACKGROUND_CSS_CLASS_STRING, "Proposal due date background value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ProposalCompletedDateString, string.Empty, "Proposal Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).ChecklistCompleteDateString, string.Empty, "Checklist Completed Date mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).PeerReviewerDisplayName, string.Empty, "Peer Reviewer name mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).CostVolumeLeadDisplayName, string.Empty, "Cost Volume Lead name mismatch");
            Assert.IsTrue(homeProposalView.CanCreateBOEWorkspace);

            // due date 10 days from now, should be yellow
            proposal.EstValue = -4000;
            proposal.ProposalDueDate = DateTime.Now + TimeSpan.FromDays(10);
            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);

            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).EstValueString, "(4,000)", "Proposal est value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).DueDateBackground, Constants.YELLOW_BACKGROUND_CSS_CLASS_STRING, "Proposal due date background value mismatch");

            // due date today, should be yellow
            proposal.ProposalDueDate = DateTime.Now + TimeSpan.FromDays(10);
            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);

            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).DueDateBackground, Constants.YELLOW_BACKGROUND_CSS_CLASS_STRING, "Proposal due date background value mismatch");

            // due date 20 days from now, should be green
            proposal.EstValue = 0;
            proposal.ProposalDueDate = DateTime.Now + TimeSpan.FromDays(20);
            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);

            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).EstValueString, "0", "Proposal est value mismatch");
            Assert.AreEqual(homeProposalView.DataRows.ElementAt(0).DueDateBackground, Constants.GREEN_BACKGROUND_CSS_CLASS_STRING, "Proposal due date background value mismatch");

            // change sort
            sortField = "ProposalTitle";
            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);
            Assert.AreEqual(sortField, homeProposalView.SortField);
            Assert.AreEqual(System.Data.SqlClient.SortOrder.Ascending, homeProposalView.Order);
            sortOrder = System.Data.SqlClient.SortOrder.Descending;
            homeProposalView = sut.GetDataForHomeProposal(sortField, sortOrder, cookie, searchText);
            Assert.AreEqual(sortField, homeProposalView.SortField);
            Assert.AreEqual(sortOrder, homeProposalView.Order);
        }

        /// <summary>
        /// Get Data for Proposal Filters test
        /// </summary>
        [TestMethod]
        public void BasicGetDataForProposalFiltersTest()
        {
            HomeControllerLogic sut = this.CreateSystem();

            HomeProposalFiltersModelView filtersView = sut.GetDataForProposalFilters();

            Assert.AreEqual(5, filtersView.FilterOptions.Count);
            Assert.AreEqual(ProposalFilterOption.InProgress, filtersView.FilterOption);
            Assert.AreEqual(0, filtersView.ViewerFilterOptions.Count);
            Assert.AreEqual(ViewerProposalFilterOption.ShowOnlyMyProposals, filtersView.ViewerFilterOption);

            this.securityAccess.Setup(x => x.CurrentUserHasRole(PtmRole.Viewer, null)).Returns(true);
            filtersView = sut.GetDataForProposalFilters();

            Assert.AreEqual(5, filtersView.FilterOptions.Count);
            Assert.AreEqual(ProposalFilterOption.InProgress, filtersView.FilterOption);
            Assert.AreEqual(2, filtersView.ViewerFilterOptions.Count);
            Assert.AreEqual(ViewerProposalFilterOption.ShowOnlyMyProposals, filtersView.ViewerFilterOption);
        }

        /// <summary>
        /// Get Proposal Id by Tracking Number test
        /// </summary>
        [TestMethod]
        public void C_GetProposalIdByTrackingNumberTest()
        {
            HomeControllerLogic sut = this.CreateSystem();
            string trackingNumber = "00-00000";
            int proposalId = 15;
            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(trackingNumber)).Returns(proposalId);

            int toTest = sut.GetProposalIdByTrackingNumber(trackingNumber);
            Assert.AreEqual(proposalId, toTest);
        }

        /// <summary>
        /// Get Proposal Id by Tracking Id test - Legacy Version
        /// </summary>
        [TestMethod]
        public void C_GetProposalIdByTrackingIdLegacyTest()
        {
            HomeControllerLogic sut = this.CreateSystem();
            string trackingNumber = "0000-00000";
            int proposalId = 15;
            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(trackingNumber)).Returns(proposalId);

            int toTest = sut.GetProposalIdByTrackingNumber(trackingNumber);
            Assert.AreEqual(proposalId, toTest);
        }

        /// <summary>
        /// Test clear cache
        /// </summary>
        [TestMethod]
        public void C_ClearCache_Test()
        {
            HomeControllerLogic sut = this.CreateSystem();

            this.cache.Setup(x => x.ClearCache());
            this.cacheWarmer.Setup(x => x.DoWarmCache());
            sut.ClearCache();

            // nothing to assert but verify the cache calls were made
            this.cache.Verify(x => x.ClearCache(), Times.Once());
            this.cacheWarmer.Verify(x => x.DoWarmCache(), Times.Once());
        }

        /// <summary>
        /// Test PopulateExportSSRSParameters function
        /// </summary>
        [TestMethod]
        public void PopulateExportSSRSParametersTest()
        {
            HomeControllerLogic sut = this.CreateSystem();
            Uri uri = null;
         
            UserDTO user = new UserDTO()
            {
                Id = 4567,
                Ntid = "kingkl"
            };

            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);

            string proposalClassFilter = ((int)ProposalClassFilterOption.NonForecasted).ToString();

            ExportProposalReportModelView goodExport = new ExportProposalReportModelView()
            {
                FilterOption = "0",
                FilterStartDate = "04/01/2013",
                FilterEndDate = "04/30/2013",
                SearchText = "ken",
                FilterProposalClass = proposalClassFilter
            };

            uri = sut.PopulateExportSSRSParameters(goodExport);
            
            // the name of this report
            Assert.IsTrue(uri.AbsoluteUri.Contains("Proposal%20Dashboard%20Report"));

            // Params
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS_ID + "=1"));
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.FILTER_START_DATE + "=04/01/2013"));
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.FILTER_END_DATE + "=04/30/2013"));
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.SEARCH_TEXT + "=ken"));
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.NTID + "=kingkl"));
            Assert.IsTrue(uri.AbsoluteUri.Contains(Constants.Report.FILTER_PROPOSAL_CLASS + "=" + proposalClassFilter));
        }

        /// <summary>
        /// Test GetMyApprovals
        /// </summary>
        [TestMethod]
        public void GetMyApprovalsTest()
        {
            HomeControllerLogic sut = this.CreateSystem();

            RequiredApprovalsModelView leadEstimatorMV = new RequiredApprovalsModelView()
            {
                TrackingNumber = "1",
                UserRole = PtmRole.Pricer
            };

            RequiredApprovalsModelView coverSheetApproverMV = new RequiredApprovalsModelView()
            {
                TrackingNumber = "2",
                UserRole = PtmRole.CoverSheetApprover
            };

            RequiredApprovalsModelView pricingVerificationMV = new RequiredApprovalsModelView()
            {
                TrackingNumber = "3",
                UserRole = PtmRole.PricingVerification
            };

            RequiredApprovalsModelView independentReviewerMV = new RequiredApprovalsModelView()
            {
                TrackingNumber = "4",
                UserRole = PtmRole.PeerReviewer
            };

            RequiredApprovalsModelView lobEstLeadMV = new RequiredApprovalsModelView()
            {
                TrackingNumber = "5",
                UserRole = PtmRole.LOBEstLead
            };

            ICollection<ApprovalsDto> approvalsCollection = new Collection<ApprovalsDto>() { leadEstimatorMV, coverSheetApproverMV, pricingVerificationMV, independentReviewerMV, lobEstLeadMV };

            this.approvalsLoader.Setup(x => x.GetApprovalsForUser(It.IsAny<int>())).Returns(approvalsCollection);

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                LeadEstimatorSignedDate = null,
                CoverSheetApproverSignedDate = null,
                PricingVerifierSignedDate = null,
                IndependentReviewerSignedDate = null
            };

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

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(permissions);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fullProposal);

            UserDTO user = new UserDTO()
            {
                Id = 1,
                Ntid = "testid"
            };

            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);
            this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(user);

            // No Lead Estimator signed date, so only Lead Estimator should be returned
            ICollection<RequiredApprovalsModelView> expected = new Collection<RequiredApprovalsModelView>() { leadEstimatorMV };

            ICollection<RequiredApprovalsModelView> actual = sut.GetMyApprovals();

            // Assert only 1 is returned and matches the expected role
            Assert.AreEqual(expected.Count, actual.Count);
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).UserRole, actual.ElementAt(i).UserRole);
            }

            proposal.LeadEstimatorSignedDate = DateTime.Today;
            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fullProposal);

            // With Lead Estimator signed date, Cover Sheet Approver, Pricing Verification, and Independent Reviewer 
            // should also be returned along with the Lead Estimator
            expected.Add(coverSheetApproverMV);
            expected.Add(pricingVerificationMV);
            expected.Add(independentReviewerMV);

            actual = sut.GetMyApprovals();

            // Assert 4 are returned and match the expected roles
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).UserRole, actual.ElementAt(i).UserRole);
            }

            proposal.CoverSheetApproverSignedDate = DateTime.Today;
            proposal.PricingVerifierSignedDate = DateTime.Today;
            proposal.IndependentReviewerSignedDate = DateTime.Today;
            fullProposal = new FullProposal(proposal);
            this.proposalLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fullProposal);

            // With signed dates for the other 4 roles, LOB Est Lead should also be returned with them
            expected.Add(lobEstLeadMV);

            actual = sut.GetMyApprovals();

            // Assert all 5 are returned and match the expected roles
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected.ElementAt(i).UserRole, actual.ElementAt(i).UserRole);
            }
        }
    }
}