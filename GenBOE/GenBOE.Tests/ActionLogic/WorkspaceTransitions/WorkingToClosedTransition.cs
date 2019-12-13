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
    public class WorkingToClosedTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        private Mock<IFullObjectFactory> factory;
        private Mock<IRetriever> retriever;
        Mock<ICommonDataMapper> commonDataMapper = null;
        Mock<IPermissionsDTODataLoader> permissionsLoader = null;

        public WorkingToClosedTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);

            return new WorkingToClosedTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void WorkingToClosedActionTest()
        {
            WorkingToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspaceObject = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            sut.Action(workspaceObject, WorkspaceState.Working, WorkspaceState.Closed);

            this.workspaceLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());
        }

        [TestMethod]
        public void WorkingToClosedTestValidateTest()
        {
            WorkingToClosedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspaceObject = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            sut.Action(workspaceObject, WorkspaceState.Working, WorkspaceState.Closed);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(workspaceObject, out validationMessage));
        }
    }
}
