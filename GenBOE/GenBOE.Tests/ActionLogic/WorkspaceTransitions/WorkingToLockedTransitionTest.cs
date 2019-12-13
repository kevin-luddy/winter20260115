// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.WorkspaceTransitions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkingToLockedTransitionTest
    {
        private Mock<IFullObjectFactory> factory;
        private Mock<IRetriever> retriever;

        [TestMethod]
        public void WorkingToLockedTransitionActionTest()
        {
            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            WorkingToLockedTransition sut = new WorkingToLockedTransition(emailer.Object, workspaceLoader.Object);

            sut.Action(workspace, WorkspaceState.Initialization, WorkspaceState.Working);

            workspaceLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());
        }


        [TestMethod]
        public void WorkingToLockedValidateTest()
        {
            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            WorkingToLockedTransition sut = new WorkingToLockedTransition(emailer.Object, workspaceLoader.Object);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullWorkspace(workspaceID), out validationMessage));
        }
    }
}
