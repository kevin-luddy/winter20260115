// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.WorkspaceTransitions
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
    public class CompleteToWorkingTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        Mock<IRetriever> retriever;
        Mock<IFullObjectFactory> factory;

        public CompleteToWorkingTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            return new CompleteToWorkingTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void CompleteToWorkingTestActionTest()
        {
            CompleteToWorkingTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            sut.Action(workspace, WorkspaceState.Complete, WorkspaceState.Working);
            workspaceLoader.Verify(x => x.RestoreTravelForWorkspace(workspaceID), Times.Once());
        }

        [TestMethod]
        public void CompleteToWorkingTestValidateTest()
        {
            CompleteToWorkingTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullWorkspace(workspaceID), out validationMessage));
        }
    }
}
