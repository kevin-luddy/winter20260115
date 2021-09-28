// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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

    [TestClass]
    public class LockedToCompleteTest
    {
        Mock<IBoeEmailer> emailer = null;
        Mock<IWorkspaceDTODataLoader> workspaceLoader = null;
        Mock<IRetriever> retriever;
        Mock<IFullObjectFactory> factory;

        public LockedToCompleteTransition CreateSystem()
        {
            emailer = new Mock<IBoeEmailer>();
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            return new LockedToCompleteTransition(emailer.Object, workspaceLoader.Object);
        }

        [TestMethod]
        public void LockedToCompleteTestActionTest()
        {
            LockedToCompleteTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>(){ new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 1, State = BOEState.AwaitingApproval }) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspaceID)).Returns(boes);

            sut.Action(workspace, WorkspaceState.Locked, WorkspaceState.Complete);
        }


        [TestMethod]
        public void LockedToCompleteTestValidateTest()
        {
            LockedToCompleteTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>()
            { 
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 1, State = BOEState.Approved }), 
                new FullBoe(new BoeDTO { WBSID = 3, CLINID = 4, Id = 2, State = BOEState.Approved })
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspaceID)).Returns(boes);

            sut.Action(workspace, WorkspaceState.Locked, WorkspaceState.Complete);

            string validationMessage = string.Empty;

            Assert.IsTrue(sut.Validate(workspace, out validationMessage));
        }


        [TestMethod]
        public void LockedToCompleteTestValidateFalseTest()
        {
            LockedToCompleteTransition sut = CreateSystem();

            int workspaceID = 1;
            string workspaceName = "Workspace ONE Test";

            FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO { WorkspaceName = workspaceName, Id = workspaceID });
            Collection<FullBoe> boes = new Collection<FullBoe>()
            { 
                new FullBoe(new BoeDTO { WBSID = 1, CLINID = 2, Id = 1, State = BOEState.AwaitingApproval }), 
                new FullBoe(new BoeDTO { WBSID = 3, CLINID = 4, Id = 2, State = BOEState.Approved })
            };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspaceID)).Returns(boes);

            sut.Action(workspace, WorkspaceState.Locked, WorkspaceState.Complete);

            string validationMessage = string.Empty;

            Assert.IsFalse(sut.Validate(workspace, out validationMessage));
        }
    }
}
