// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    public class LockedToWorkingTest
    {
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();        

        [TestMethod]
        public void LockedToWorkingTestActionTest()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            
            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            LockedToWorkingTransition sut = new LockedToWorkingTransition(emailer.Object, workspaceLoader.Object);

            sut.Action(workspace, WorkspaceState.Locked, WorkspaceState.Working);
            workspaceLoader.Verify(x => x.RestoreTravelForWorkspace(workspaceID), Times.Once());
        }


        [TestMethod]
        public void LockedToWorkingTestValidateTest()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });

            LockedToWorkingTransition sut = new LockedToWorkingTransition(emailer.Object, workspaceLoader.Object);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(workspace, out validationMessage));
        }
    }
}
