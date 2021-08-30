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
    public class NoneToInitializeTransitionTest
    {
        [TestMethod]
        public void NoneToInitializeTransitionAction()
        {
            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            WorkspaceDTO workspace = new WorkspaceDTO { 
                CreatedByUserID = 1, 
                CostVolumeLeadPricerUserID = 2, 
                Id = workspaceID,
                WorkspaceName = workspaceName
            };
            workspaceLoader.Setup(x => x.GetById(workspaceID)).Returns(workspace);

            NoneToInitializationTransition sut = new NoneToInitializationTransition(emailer.Object, workspaceLoader.Object);

            sut.Action(factory.Object.CreateFullWorkspace(workspaceID), WorkspaceState.Initialization, WorkspaceState.Working);
      
        }
    }
}
