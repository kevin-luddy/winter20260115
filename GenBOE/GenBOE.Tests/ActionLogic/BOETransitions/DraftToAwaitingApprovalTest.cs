// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOETransitions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DraftToAwaitingApprovalTest : MOQObject
    {
        private Mock<ICommonDataMapper> commonDM = new Mock<ICommonDataMapper>();
        private Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
        private Mock<IRetriever> retriever = new Mock<IRetriever>();
        private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

        [TestMethod]
        public void DraftToAwaitingApprovalActionTest()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDM.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int boeID = 1;
            int workspaceID = this.Workspace.Id;

            var emailer = new Mock<IBoeEmailer>();
            var userLoader = new Mock<IUserDTODataLoader>();
            var secInfoLoader = new Mock<ISecurityInformation>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO boe1 = new BoeDTO { WBSID = 1, CLINID = 1, Id = boeID, State = BOEState.Draft, WorkspaceID = workspaceID };
            BoeDTO boe2 = new BoeDTO { WBSID = 2, CLINID = 2, Id = boeID + 1, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe3 = new BoeDTO { WBSID = 3, CLINID = 3, Id = boeID + 2, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe4 = new BoeDTO { WBSID = 4, CLINID = 4, Id = boeID + 3, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe5 = new BoeDTO { WBSID = 5, CLINID = 5, Id = boeID + 4, State = BOEState.DraftLocked, WorkspaceID = workspaceID };

            FullBoe boeFull1 = new FullBoe(boe1);
            FullBoe boeFull2 = new FullBoe(boe2);
            FullBoe boeFull3 = new FullBoe(boe3);
            FullBoe boeFull4 = new FullBoe(boe4);
            FullBoe boeFull5 = new FullBoe(boe5);

            this.retriever.Setup(r => r.GetFullBoesByWorkspaceId(workspaceID)).Returns(new Collection<FullBoe> { boeFull1, boeFull2, boeFull3, boeFull4, boeFull5 });

            DraftToAwaitingApprovalTransition sut = new DraftToAwaitingApprovalTransition(emailer.Object,
                                                                                  secInfoLoader.Object, userLoader.Object, boeApproverLoader.Object, workspaceDataLoader.Object);
            
            FullWorkspace workspaceFull = new FullWorkspace(this.Workspace);

            //Should not lock rates because WS is not locked
            sut.Action(boeFull1, workspaceFull, BOEState.Draft, BOEState.AwaitingApproval);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Never());

            workspaceFull.WorkspaceState = WorkspaceState.Locked;

            //Should lock rates because WS is locked and all BOEs are Draft Locked
            sut.Action(boeFull1, workspaceFull, BOEState.Draft, BOEState.AwaitingApproval);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());

            //Should not lock rates because there is still unlocked BOE
            boeFull4.State = BOEState.Draft;
            sut.Action(boeFull1, workspaceFull, BOEState.Draft, BOEState.AwaitingApproval);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());  // wasn't called this time

            //Should lock rates becase WS is locked and all BOEs locked/submitted for aproval
            boeFull4.State = BOEState.AwaitingApproval;
            sut.Action(boeFull1, workspaceFull, BOEState.Draft, BOEState.AwaitingApproval);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Exactly(2));
        }

        [TestMethod]
        public void DraftToAwaitingApprovalValidateTestNoAuthor()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var userLoader = new Mock<IUserDTODataLoader>();
            var secInfoLoader = new Mock<ISecurityInformation>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO boe = new BoeDTO {WBSID = 1,CLINID = 2,Id = boeID,State = BOEState.Draft,WorkspaceID = workspaceID};
            FullBoe boeObject = new FullBoe(boe);

            DraftToAwaitingApprovalTransition sut = new DraftToAwaitingApprovalTransition(emailer.Object,
                                                                                  secInfoLoader.Object, userLoader.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            string errorMessage = string.Empty;
            bool isValid = sut.Validate(boeObject, new FullWorkspace(this.Workspace), out errorMessage);
            Assert.IsFalse(isValid, "No author .. should have failed transition.  Validation is " + errorMessage);
        }

        [TestMethod]
        public void DraftToAwaitingApprovalValidateTestAuthorsDoNotMatch()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDM.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var userLoader = new Mock<IUserDTODataLoader>();
            var secInfoLoader = new Mock<ISecurityInformation>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            PermissionsDTO permission = new PermissionsDTO { WorkspaceId = 1, ETIUserId = 1, Role = Role.Approver };
            UserDTO theUser = new UserDTO { UserID = 1, NTID = "mrauthor" };
            UserDTO author = new UserDTO { UserID = 3, NTID = "mrauthor" };
            BoeDTO boe = new BoeDTO { WBSID = 1, CLINID = 2, Id = boeID, State = BOEState.Draft, WorkspaceID = workspaceID,
                                      AuthorIDs = new Collection<int>{author.UserID} };
            FullBoe boeObject = new FullBoe(boe);


            userLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO>() { author });
            userLoader.Setup(x => x.GetOrCreateUserByNtid("NOMATCH")).Returns(theUser);
            permissionsLoader.Setup(x => x.GetWorkspacePermissions(1)).Returns(new Collection<PermissionsDTO>() { permission });

            secInfoLoader.Setup(x => x.ActiveUserNTID).Returns("NOMATCH");

            userLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO>() { author });
            secInfoLoader.Setup(x => x.ActiveUserNTID).Returns("NOMATCH");

            DraftToAwaitingApprovalTransition sut = new DraftToAwaitingApprovalTransition(emailer.Object,
                                                                                  secInfoLoader.Object, userLoader.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            string errorMessage = string.Empty;
            bool isValid = sut.Validate(boeObject, new FullWorkspace(this.Workspace), out errorMessage);
            Assert.IsFalse(isValid, "The NTIDs should not have matched for active user and author's ntid.  Validation message " + errorMessage);
        }

        [TestMethod]
        public void DraftToAwaitingApprovalValidateTestAuthors()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDM.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var userLoader = new Mock<IUserDTODataLoader>();
            var secInfoLoader = new Mock<ISecurityInformation>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            UserDTO author = new UserDTO { UserID = 3, NTID = "mrauthor" };
            BoeDTO boe = new BoeDTO
            {
                WBSID = 1,
                CLINID = 2,
                Id = boeID,
                State = BOEState.Draft,
                WorkspaceID = workspaceID,
                AuthorIDs = new Collection<int>{author.UserID}
            };
            FullBoe boeObject = new FullBoe(boe);

            userLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO>() { author });
            secInfoLoader.Setup(x => x.ActiveUserNTID).Returns(author.NTID); // match the author's ntid

            DraftToAwaitingApprovalTransition sut = new DraftToAwaitingApprovalTransition(emailer.Object,
                                                                                  secInfoLoader.Object, userLoader.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            this.Workspace.WorkspaceState = WorkspaceState.Working;

            string errorMessage = string.Empty;
            bool isValid = sut.Validate(boeObject, new FullWorkspace(this.Workspace), out errorMessage);
            Assert.IsTrue(isValid, "The transition from draft to awaiting approval had a validation error : " + errorMessage);
        }      
    }
}
