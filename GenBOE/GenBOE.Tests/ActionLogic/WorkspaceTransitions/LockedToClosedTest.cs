// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.WorkspaceTransitions;
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
    public class LockedToClosedTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;

        private Mock<IFullObjectFactory> factory;
        private Mock<IRetriever> retriever;
        private Mock<ICommonDataMapper> commonMapper;
        private Mock<IPermissionsDTODataLoader> permissionsLoader;

        public LockedToClosedTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();
            this.commonMapper = new Mock<ICommonDataMapper>();
            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.commonMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);

            return new LockedToClosedTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void LockedToClosedActionTest()
        {
            LockedToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            WorkspaceDTO workspace = new WorkspaceDTO { Id = workspaceID, WorkspaceName = workspaceName, WorkspaceState = WorkspaceState.Locked };
            FullWorkspace workspaceFull = new FullWorkspace(workspace);

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(workspaceID)).Returns(workspaceFull);

            FullWorkspace ws = this.factory.Object.CreateFullWorkspace(workspaceID);

            sut.Action(ws, WorkspaceState.Locked, WorkspaceState.Closed);

            workspaceLoader.Verify(x => x.LockedDataExists(workspaceID), Times.Once());
            workspaceLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());
        }

        [TestMethod]
        public void LockedToClosedTestValidateTest()
        {
            LockedToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            WorkspaceDTO workspace = new WorkspaceDTO { Id = workspaceID, WorkspaceName = workspaceName, WorkspaceState = WorkspaceState.Locked };
            FullWorkspace workspaceFull = new FullWorkspace(workspace);

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });
            this.factory.Setup(x => x.CreateFullWorkspace(workspaceID)).Returns(workspaceFull);

            FullWorkspace ws = this.factory.Object.CreateFullWorkspace(workspaceID);

            sut.Action(ws, WorkspaceState.Locked, WorkspaceState.Closed);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullWorkspace(workspaceID), out validationMessage));

            workspaceLoader.Verify(x => x.LockedDataExists(workspaceID), Times.Once());
            workspaceLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());
        }
    }
}
