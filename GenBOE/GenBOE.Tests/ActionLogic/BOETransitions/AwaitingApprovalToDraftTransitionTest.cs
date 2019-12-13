// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOETransitions
{
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AwaitingApprovalToDraftTransitionTest : MOQObject
    {
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> retriever = new Mock<IRetriever>();

        [TestMethod]
        public void ValidateTestFail()
        {
            var emailer = new Mock<IBoeEmailer>();
            var commonMapper = new Mock<ICommonDataMapper>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO myboe = new BoeDTO { WorkspaceID = 2 };
            WorkspaceDTO myworkspace = new WorkspaceDTO { WorkspaceState = IES.Common.WorkspaceState.Initialization};

            this.retriever.Setup(x => x.GetWorkspaceById(myboe.WorkspaceID)).Returns(myworkspace);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullBoe boeObject = new FullBoe(myboe);
            factory.Setup(x => x.CreateFullWorkspace(myworkspace)).Returns(new FullWorkspace(myworkspace));
            retriever.Setup(x => x.GetFullWorkspaceById(myboe.WorkspaceID)).Returns(new FullWorkspace(myworkspace));

            AwaitingApprovalToDraftTransition sut = new AwaitingApprovalToDraftTransition(emailer.Object, commonMapper.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            string errorMessage = string.Empty;
            bool validateReturn = sut.Validate(boeObject, new FullWorkspace(myworkspace), out errorMessage);

            Assert.IsFalse(validateReturn, "expected false, received true from AwaitingApprovalToDraftTransitionTest validate");
            Assert.IsNotNull(errorMessage, "validate return string is null");
            Assert.IsTrue(!string.IsNullOrEmpty(errorMessage), "validate message contained no text");
        }


        [TestMethod]
        public void ValidateTestPass()
        {
            var emailer = new Mock<IBoeEmailer>();
            var commonMapper = new Mock<ICommonDataMapper>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO myboe = new BoeDTO { WorkspaceID = 2 };
            WorkspaceDTO myworkspace = new WorkspaceDTO { WorkspaceState = IES.Common.WorkspaceState.Working };

            this.retriever.Setup(x => x.GetWorkspaceById(myboe.WorkspaceID)).Returns(myworkspace);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            FullBoe boeObject = new FullBoe(myboe);
            factory.Setup(x => x.CreateFullWorkspace(myworkspace)).Returns(new FullWorkspace(myworkspace));
            retriever.Setup(x => x.GetFullWorkspaceById(myboe.WorkspaceID)).Returns(new FullWorkspace(myworkspace));

            AwaitingApprovalToDraftTransition sut = new AwaitingApprovalToDraftTransition(emailer.Object, commonMapper.Object, boeApproverLoader.Object, workspaceDataLoader.Object);

            string errorMessage = string.Empty;
            FullWorkspace workspace = new FullWorkspace(this.Workspace);
            workspace.WorkspaceState = IES.Common.WorkspaceState.Working;

            bool validateReturn = sut.Validate(boeObject, workspace, out errorMessage);

            Assert.IsTrue(validateReturn, "expected true, received false from AwaitingApprovalToDraftTransitionTest validate");
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage), "validate message contained text and should NOT have");
        }
    
    }
}
