// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    public class CompleteToLockedTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        private Mock<IFullObjectFactory> factory;

        public CompleteToLockedTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            factory = new Mock<IFullObjectFactory>();

            return new CompleteToLockedTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void CompleteToLockedTestActionTest()
        {
            CompleteToLockedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            sut.Action(this.factory.Object.CreateFullWorkspace(workspaceID), WorkspaceState.Complete, WorkspaceState.Locked);
        }


        [TestMethod]
        public void CompleteToLockedTestValidateTest()
        {
            CompleteToLockedTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { WorkspaceName = workspaceName });

            sut.Action(this.factory.Object.CreateFullWorkspace(workspaceID), WorkspaceState.Complete, WorkspaceState.Locked);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullWorkspace(workspaceID), out validationMessage));
        }
    }
}
