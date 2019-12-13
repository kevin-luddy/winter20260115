// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests
{
    using System;
    using System.Collections.Generic;
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
    /// Test the GenTRAC Controller Logic test
    /// </summary>
    [TestClass]
    public class GenTRACControllerLogicTest
    {
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
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory = null;

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
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator;

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Lifetime managers for the Unity container
        /// </summary>
        private static List<ContainerControlledLifetimeManager> lifetimeManagers = new List<ContainerControlledLifetimeManager>();

        /// <summary>
        /// Get Lifetime Manager
        /// </summary>
        /// <returns>Returns a lifetime manager</returns>
        private static ContainerControlledLifetimeManager GetLifetimeManager()
        {
            ContainerControlledLifetimeManager manager = new ContainerControlledLifetimeManager();
            lifetimeManagers.Add(manager);
            return manager;
        }

        /// <summary>
        /// Initialization method
        /// </summary>
        /// <param name="context">Context</param>
        [ClassInitialize]
        public static void InitializeAll(TestContext context)
        {
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IES.Common.ICache), typeof(IES.Common.MemoryCache), GetLifetimeManager(), new InjectionMember[] { });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalLoader), typeof(ProposalLoader), GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IES.Common.CacheDataLoader), typeof(IES.Common.CacheDataLoader), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IES.Common.ICache)), -1));
        }

        /// <summary>
        /// Cleanup method
        /// </summary>
        [ClassCleanup]
        public static void CleanupAll()
        {
            IES.Common.classes.GenBOEUnityContainer.Container.Dispose();

            foreach (ContainerControlledLifetimeManager manager in lifetimeManagers)
            {
                manager.Dispose();
            }
        }

        /// <summary>
        /// Test Check Permissions
        /// </summary>
        [TestMethod]
        public void C_CheckPermissionsTest()
        {
            var sut = this.CreateSystem();

            PtmRole highestRole;
            this.secAccess.Setup(x => x.IsAuthorized(It.IsAny<SecurityPermissionsRequested>(), out highestRole)).Returns(SecurityAuthorization.CreateReadUpdateDelete);

            SecurityAuthorizationAndRole result = sut.CheckPermissions(PtmSecurityPage.Home, null);

            Assert.AreEqual(SecurityAuthorization.CreateReadUpdateDelete, result.Authorization);
        }

        /// <summary>
        /// Test Check Permissions
        /// </summary>
        [TestMethod]
        public void C_GetMasterViewTest()
        {
            var sut = this.CreateSystem();

            GenTRACMasterModelView result = sut.GetMasterView();

            Assert.AreEqual("Lockheed Martin Proprietary Information", result.HeaderFooter);
        }

        #region Exception Tests

        /// <summary>
        /// Test check permissions exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_CheckPermissionsExceptionTest()
        {
            var sut = this.CreateSystem();

            sut.CheckPermissions(null, null);
        }

        #endregion Exception Tests

        /// <summary>
        /// Creates the system
        /// </summary>
        /// <returns>GenTRAC Controller Logic</returns>
        private GenTRACControllerLogic CreateSystem()
        {
            this.secAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.proposalMediator = new Mock<IProposalMediator>();

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            return new GenTRACControllerLogic(this.secAccess.Object, this.proposalLoader.Object, this.userMapper.Object, this.objectFactory.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, this.proposalMediator.Object);
        }

        /// <summary>
        /// Test Get by Proposal ID
        /// </summary>
        [TestMethod]
        public void C_GetByProposalId()
        {
            int proposalid = 3;
            ProposalDto proposal = new ProposalDto { Id = proposalid, TrackingNumber = "201345", ForecastedTrackingNumber = "F201345" };
            var sut = this.CreateSystem();

            this.proposalLoader.Setup(x => x.GetById(proposalid)).Returns(proposal);
            ProposalDto proposalToTest = sut.GetByProposalId(proposalid);

            Assert.IsTrue(proposalToTest.Id == proposalid);
            Assert.IsTrue(proposalToTest.TrackingNumber == proposal.TrackingNumber);
            Assert.IsTrue(proposalToTest.ForecastedTrackingNumber == proposal.ForecastedTrackingNumber);
        }

        /// <summary>
        /// Test Get Users Online
        /// </summary>
        [TestMethod]
        public void C_GetUsersOnline()
        {
            UsersOnlineDTO expectedValue = new UsersOnlineDTO()
            {
                Id = 5
            };
            var sut = this.CreateSystem();

            this.userMapper.Setup(x => x.GetUsersOnline()).Returns(expectedValue);
            UsersOnlineDTO actualValue = sut.GetUsersOnline();

            Assert.AreEqual(expectedValue.Id, actualValue.Id);
        }

        /// <summary>
        /// Test GetListItemsForEnum
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Michoud")]
        [TestMethod]
        public void C_GetListItemsForPickListEnum()
        {
            List<SelectListItem> expectedResult = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "Select Proposal Location" },
                new SelectListItem() { Value = "7", Text = "Valley Forge, PA" },
                new SelectListItem() { Value = "9", Text = "Other" },
                new SelectListItem() { Value = "10", Text = "Cape Canaveral, FL" },
                new SelectListItem() { Value = "11", Text = "Denver, CO" },
                new SelectListItem() { Value = "12", Text = "Huntsville, AL" },
                new SelectListItem() { Value = "13", Text = "Michoud, LA" },
                new SelectListItem() { Value = "14", Text = "Sunnyvale, CA" }
            };

            List<SelectListItem> actualResult = EnumUtilities.GetListItemsForEnum(typeof(ProposalLocation), false).ToList();

            Assert.AreEqual(expectedResult.Count, actualResult.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].Text, actualResult[i].Text);
                Assert.AreEqual(expectedResult[i].Value, actualResult[i].Value);
            }
        }

        /// <summary>
        /// Test GetListItemsForEnum with a selected value of an inactive item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Michoud")]
        [TestMethod]
        public void C_GetListItemsForPickListEnumWithSelectedValueOfInactiveItem()
        {
            List<SelectListItem> expectedResult = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "Select Proposal Location" },
                new SelectListItem() { Value = "6", Text = "Rockville, MD" },               // Inactive item.
                new SelectListItem() { Value = "7", Text = "Valley Forge, PA" },
                new SelectListItem() { Value = "9", Text = "Other" },
                new SelectListItem() { Value = "10", Text = "Cape Canaveral, FL" },
                new SelectListItem() { Value = "11", Text = "Denver, CO" },
                new SelectListItem() { Value = "12", Text = "Huntsville, AL" },
                new SelectListItem() { Value = "13", Text = "Michoud, LA" },
                new SelectListItem() { Value = "14", Text = "Sunnyvale, CA" }
            };

            // Provide a selected value for an item that is inactive.  It should be returned with the results.
            List<SelectListItem> actualResult = EnumUtilities.GetListItemsForEnum(typeof(ProposalLocation), false, ProposalLocation.RockvilleMD.ToString()).ToList();

            Assert.AreEqual(expectedResult.Count, actualResult.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].Text, actualResult[i].Text);
                Assert.AreEqual(expectedResult[i].Value, actualResult[i].Value);
            }
        }

        /// <summary>
        /// Test GetListItemsForEnumSorted
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Michoud")]
        [TestMethod]
        public void C_GetListItemsForPickListEnumSorted()
        {
            List<SelectListItem> expectedResult = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "0", Text = "Select Proposal Location" },
                new SelectListItem() { Value = "10", Text = "Cape Canaveral, FL" },
                new SelectListItem() { Value = "11", Text = "Denver, CO" },
                new SelectListItem() { Value = "12", Text = "Huntsville, AL" },
                new SelectListItem() { Value = "13", Text = "Michoud, LA" },
                new SelectListItem() { Value = "9", Text = "Other" },
                new SelectListItem() { Value = "14", Text = "Sunnyvale, CA" },
                new SelectListItem() { Value = "7", Text = "Valley Forge, PA" }
            };

            // Items in the select list should be sorted by the Text property, but the default selection ('Select Proposal Location') should still be first.
            List<SelectListItem> actualResult = EnumUtilities.GetListItemsForEnumSorted(typeof(ProposalLocation), false).ToList();

            Assert.AreEqual(expectedResult.Count, actualResult.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].Text, actualResult[i].Text);
                Assert.AreEqual(expectedResult[i].Value, actualResult[i].Value);
            }
        }

        /// <summary>
        /// Test IndependentReviewerIsNeeded method for when the result should be true
        /// </summary>
        [TestMethod]
        public void IndependentReviewerIsNeededTest_True()
        {
            GenTRACControllerLogic sut = this.CreateSystem();

            int userID = 1;

            UserDTO userDto = new UserDTO()
            {
                Id = userID,
                Ntid = "testID"
            };

            this.userMapper.Setup(x => x.GetById(userID)).Returns(userDto);

            ProposalPermissionDto leadEstimator = new ProposalPermissionDto()
            {
                UserId = userID,
                Role = PtmRole.Pricer
            };

            ProposalPermissionDto coverSheetApprover = new ProposalPermissionDto()
            {
                UserId = userID,
                Role = PtmRole.CoverSheetApprover
            };

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>() { leadEstimator, coverSheetApprover };

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1,
                IsCCPDRequired = true
            };

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(permissions);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            // Test method that takes in full proposal
            bool result = sut.IndependentReviewerIsNeeded(fullProposal);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test IndependentReviewerIsNeeded method for when the result should be false
        /// </summary>
        [TestMethod]
        public void IndependentReviewerIsNeededTest_False()
        {
            GenTRACControllerLogic sut = this.CreateSystem();

            int userID_LE = 1;
            int userID_CSA = 2;

            UserDTO userDtoLE = new UserDTO()
            {
                Id = userID_LE,
                Ntid = "testIDLE"
            };

            UserDTO userDtoCSA = new UserDTO()
            {
                Id = userID_CSA,
                Ntid = "testIDCSA"
            };

            this.userMapper.Setup(x => x.GetById(userID_LE)).Returns(userDtoLE);
            this.userMapper.Setup(x => x.GetById(userID_CSA)).Returns(userDtoCSA);

            ProposalPermissionDto leadEstimator = new ProposalPermissionDto()
            {
                UserId = userID_LE,
                Role = PtmRole.Pricer
            };

            ProposalPermissionDto coverSheetApprover = new ProposalPermissionDto()
            {
                UserId = userID_CSA,
                Role = PtmRole.CoverSheetApprover
            };

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>() { leadEstimator, coverSheetApprover };

            ProposalDto proposal = new ProposalDto()
            {
                Id = 1
            };

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(permissions);

            FullProposal fullProposal = new FullProposal(proposal);

            this.proposalLoader.Setup(x => x.GetById(proposal.Id)).Returns(proposal);
            this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(fullProposal);

            // Test method that takes in full proposal
            bool result = sut.IndependentReviewerIsNeeded(fullProposal);

            Assert.IsFalse(result);
        }
    }
}