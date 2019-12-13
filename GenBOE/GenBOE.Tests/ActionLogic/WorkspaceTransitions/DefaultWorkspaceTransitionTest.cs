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
    public class DefaultWorkspaceTransitionTest
    {
        [TestMethod]
        public void DefaultWorkspaceTransitionActionTest()
        {
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(new WorkspaceDTO { Id=workspaceID, WorkspaceName = "Workspace ONE Test" });

            DefaultWorkspaceTransition sut = new DefaultWorkspaceTransition(emailer.Object, 
                                                                            workspaceLoader.Object);

            sut.Action(factory.Object.CreateFullWorkspace(workspaceID), WorkspaceState.Initialization, WorkspaceState.Working);
        }

        [TestMethod]
        public void DefaultWorkspaceTransitionValidateTest()
        {
            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            DefaultWorkspaceTransition sut = new DefaultWorkspaceTransition(emailer.Object,
                                                                workspaceLoader.Object);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(factory.Object.CreateFullWorkspace(1), out validationMessage));
        }
    }
}
