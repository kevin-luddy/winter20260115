// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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
    public class InitializationToClosedTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        Mock<IRetriever> retriever;
        Mock<IFullObjectFactory> factory;
        Mock<ICommonDataMapper> commonDM;
        Mock<IPermissionsDTODataLoader> permissionsLoader;

        public InitializationToClosedTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();
            this.commonDM = new Mock<ICommonDataMapper>();
            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDM.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            return new InitializationToClosedTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void InitializationToClosedActionTest()
        {
            InitializationToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            sut.Action(workspace, WorkspaceState.Initialization, WorkspaceState.Closed);

            this.workspaceLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());
        }

        [TestMethod]
        public void InitializationToClosedTestValidateTest()
        {
            InitializationToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            sut.Action(workspace, WorkspaceState.Initialization, WorkspaceState.Closed);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(workspace, out validationMessage));
        }
    }
}
