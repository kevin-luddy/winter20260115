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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkingToInitTransitionTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        private Mock<IFullObjectFactory> factory;

        public WorkingToInitTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.factory = new Mock<IFullObjectFactory>();

            return new WorkingToInitTransition(emailer.Object, workspaceLoader.Object);
        }

        public void WorkingToInitTransitionActionTest()
        {
            WorkingToInitTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            sut.Action(this.factory.Object.CreateFullWorkspace(workspaceID), WorkspaceState.Working, WorkspaceState.Initialization);
        }


        [TestMethod]
        public void WorkingToInitValidateTest()
        {
            WorkingToInitTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullWorkspace(workspaceID), out validationMessage));
        }
    }
}
