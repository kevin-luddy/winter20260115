// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System.Collections.ObjectModel;
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

    /// <summary>
    /// Test if validation returns true.
    /// </summary>
    [TestClass]
    public class InitializationToWorkingTransitionTest
    {
        Mock<IRetriever> retriever;
        Mock<IFullObjectFactory> factory;

        [TestMethod]
        public void InitializationToWorkingActionTest()
        {
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>()
            {
                new FullBoe(new BoeDTO { WBSID=1, CLINID=2, Id = 1, State = BOEState.AwaitingApproval }),
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 2, State = BOEState.Draft })
            };
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(boes);

            InitializationToWorkingTransition sut = new InitializationToWorkingTransition(emailer.Object, workspaceLoader.Object);

            sut.Action(workspace, WorkspaceState.Initialization, WorkspaceState.Working);
        }

        [TestMethod]
        public void FailValtionWorkspaceTransitionValidateTest()
        {
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            // ensure that any BOEs below DRAFT (unassigned) cause a failed validation and an error string to be returned from validation

            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>()
            {
                new FullBoe(new BoeDTO { WBSID=1, CLINID=2, Id = 1, State = BOEState.Unassigned }),
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 2, State = BOEState.Unassigned })
            };
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(boes);

            InitializationToWorkingTransition sut = new InitializationToWorkingTransition(emailer.Object, workspaceLoader.Object);

            string validationMessage = string.Empty;

            bool valid = sut.Validate(workspace, out validationMessage);

            Assert.IsFalse(valid, "validation should have failed");
            Assert.IsTrue(!string.IsNullOrEmpty(validationMessage), "validation string should have been valued with the validation error");
        }


        [TestMethod]
        public void PassValtionWorkspaceTransitionValidateTest()
        {
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            // ensure that all BOEs at DRAFT or above result in a successful validation and empty validation error message
            int workspaceID = 1;

            var emailer = new Mock<IBoeEmailer>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>()
            {
                new FullBoe(new BoeDTO { WBSID=1, CLINID=2, Id = 1, State = BOEState.Unassigned }),
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 1, State = BOEState.Draft })
            };
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(boes);

            InitializationToWorkingTransition sut = new InitializationToWorkingTransition(emailer.Object, workspaceLoader.Object);

            string validationMessage = string.Empty;

            // test with BOEs mixed between DRAFT and above
            bool valid = sut.Validate(workspace, out validationMessage);

            Assert.IsTrue(valid, "validation should have passed");
            Assert.IsTrue(string.IsNullOrEmpty(validationMessage), "validation string return should have been empty");

            // retest with all BOEs ABOVE DRAFT
            boes = new Collection<FullBoe>()
            {
                new FullBoe(new BoeDTO { WBSID=1, CLINID=2, Id = 1, State = BOEState.AwaitingApproval }),
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 1, State = BOEState.AwaitingApproval })
            };
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(boes);

            valid = sut.Validate(workspace, out validationMessage);

            Assert.IsTrue(valid, "validation2 should have passed");
            Assert.IsTrue(string.IsNullOrEmpty(validationMessage), "validation2 string return should have been empty");
        }
    }
}
