// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOETransitions
{
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
    public class ApprovedToDraftTransitionTest : MOQObject
    {
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> retriever = new Mock<IRetriever>();

        [TestMethod]
        public void ApprovedToDraftActionTest()
        {
            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO boe = new BoeDTO { WBSID = 1, CLINID = 2, Id = boeID, State = BOEState.Approved, WorkspaceID = workspaceID };

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullBoe boeObject = new FullBoe(boe);

            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(boeID)).Returns(new Collection<BoeApproverResponseDTO> { });

            ApprovedToDraftTransition sut = new ApprovedToDraftTransition(emailer.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            sut.Action(boeObject, new FullWorkspace(this.Workspace), BOEState.Approved, BOEState.Draft);
        }
        [TestMethod]
        public void ApprovedToDraftValidateTest()
        {

            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var commonMapper = new Mock<ICommonDataMapper>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            WorkspaceDTO workspace = new WorkspaceDTO { Id = workspaceID, WorkspaceState = IES.Common.WorkspaceState.Working };
            BoeDTO boe = new BoeDTO { WBSID = 1, CLINID = 2, Id = boeID, State = BOEState.Approved, WorkspaceID = workspaceID };

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonMapper.Object);

            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            commonMapper.Setup(x => x.getWorkspaceStateName(workspace.WorkspaceState)).Returns("Working");
            this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));

            this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
            FullBoe boeObject = new FullBoe(boe);

            ApprovedToDraftTransition sut = new ApprovedToDraftTransition(emailer.Object, boeApproverLoader.Object, workspaceDataLoader.Object);
            string errorMessage = string.Empty;
            bool isValid = sut.Validate(boeObject, new FullWorkspace(workspace), out errorMessage);

            Assert.IsTrue(isValid == true, "The Approved to Draft state did not work, validation error " + errorMessage);
        }

        [TestMethod]
        public void InvalidApprovedToDraftValidateTest()
        {
            int boeID = 1;
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var commonMapper = new Mock<ICommonDataMapper>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            WorkspaceDTO workspace = new WorkspaceDTO { Id = workspaceID, WorkspaceState = IES.Common.WorkspaceState.Complete};
            BoeDTO boe = new BoeDTO { WBSID = 1, CLINID = 2, Id = boeID, State = BOEState.Approved, WorkspaceID = workspaceID };

            this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonMapper.Object);

            Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            commonMapper.Setup(x => x.getWorkspaceStateName(workspace.WorkspaceState)).Returns("Locked");
            FullBoe boeObject = new FullBoe(boe);
            factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
            this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

            ApprovedToDraftTransition sut = new ApprovedToDraftTransition(emailer.Object, boeApproverLoader.Object, workspaceDataLoader.Object);
            string errorMessage = string.Empty;
            bool isValid = sut.Validate(boeObject, new FullWorkspace(workspace), out errorMessage);

            Assert.IsTrue(isValid == false, "The Approved to Draft state did not work because Workspace is not in working.  Validation error " + errorMessage);
        }
    }
}
