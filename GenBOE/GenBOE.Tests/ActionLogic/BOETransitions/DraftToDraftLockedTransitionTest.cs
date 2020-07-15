// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOETransitions
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DraftToDraftLockedTransitionTest : MOQObject
    {
        private Mock<ICommonDataMapper> commonMapper = new Mock<ICommonDataMapper>();
        private Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
        private Mock<IRetriever> retriever = new Mock<IRetriever>();
        private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

        [TestMethod]
        public void DraftToDraftLockedActionTest()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.commonMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.factory.Object);

            int boeID = 1;
            int workspaceID = this.Workspace.Id;

            Mock<IBoeEmailer> emailer = new Mock<IBoeEmailer>();
            Mock<IBoeApproverResponseDTODataLoader> boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            Mock<IWorkspaceDTODataLoader> workspaceDataLoader = new Mock<IWorkspaceDTODataLoader>();

            BoeDTO boe1 = new BoeDTO { WBSID = 1, CLINID = 1, Id = boeID, State = BOEState.Draft, WorkspaceID = workspaceID };
            BoeDTO boe2 = new BoeDTO { WBSID = 2, CLINID = 2, Id = boeID + 1, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe3 = new BoeDTO { WBSID = 3, CLINID = 3, Id = boeID + 2, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe4 = new BoeDTO { WBSID = 4, CLINID = 4, Id = boeID + 3, State = BOEState.DraftLocked, WorkspaceID = workspaceID };
            BoeDTO boe5 = new BoeDTO { WBSID = 5, CLINID = 5, Id = boeID + 4, State = BOEState.DraftLocked, WorkspaceID = workspaceID };

            FullBoe boeFull1 = new FullBoe(boe1);
            FullBoe boeFull2 = new FullBoe(boe2);
            FullBoe boeFull3 = new FullBoe(boe3);
            FullBoe boeFull4 = new FullBoe(boe4);
            FullBoe boeFull5 = new FullBoe(boe5);

            this.retriever.Setup(r => r.GetFullBoesByWorkspaceId(workspaceID)).Returns(new Collection<FullBoe> { boeFull1, boeFull2, boeFull3, boeFull4, boeFull5 });

            int? lastLockedBoeID = boe1.Id;
            workspaceDataLoader.Setup(r => r.GetLastLockedBOEID(workspaceID)).Returns(lastLockedBoeID);

            DraftToDraftLockedTransition sut = new DraftToDraftLockedTransition(
                emailer.Object,
                boeApproverLoader.Object,
                workspaceDataLoader.Object);

            FullWorkspace workspaceFull = new FullWorkspace(this.Workspace);

            sut.Action(boeFull1, workspaceFull, BOEState.Draft, BOEState.DraftLocked);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());

            sut.Action(boeFull2, workspaceFull, BOEState.Draft, BOEState.DraftLocked);
            workspaceDataLoader.Verify(x => x.LockTravelAndResourceRatesForWorkspace(workspaceID), Times.Once());  // wasn't called this time
        }
    }
}
