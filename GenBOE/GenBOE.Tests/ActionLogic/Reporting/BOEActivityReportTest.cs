// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEActivityReportTest
    {
        private const string BOEState_Draft = "Draft";
        private const string BOEState_None = "None";
        private const string BOEState_Approved = "Approved";
        private const string BOEState_Unassigned = "Unassigned";
        private const string BOEState_AwaitingApproval = "Awaiting Approval";

        private IDictionary<int, BOEStateModelView> boeStatesDictionary;
        private IDictionary<int, WorkspaceStateModelView> workspaceStatesDictionary;
        private Mock<ICommonDataMapper> commonDataMapper;

        [TestInitialize]
        public void Setup()
        {
            boeStatesDictionary = new Dictionary<int, BOEStateModelView>()
            {
                {0, new BOEStateModelView(){ BOEStateID = 0, BOEState = "None" }},
                {1, new BOEStateModelView(){ BOEStateID = 1, BOEState = "Unassigned"}},
                {2, new BOEStateModelView(){ BOEStateID = 2, BOEState = "Draft"}},
                {3, new BOEStateModelView(){ BOEStateID = 3, BOEState = "Awaiting Approval"}},
                {4, new BOEStateModelView(){ BOEStateID = 4, BOEState = "Approved"}},
                {5, new BOEStateModelView(){ BOEStateID = 5, BOEState = "DateShiftDraft"}},
                {6, new BOEStateModelView(){ BOEStateID = 6, BOEState = "Locked Draft"}}
            };

            workspaceStatesDictionary = new Dictionary<int, WorkspaceStateModelView>()
            {
                {0, new WorkspaceStateModelView(){ WorkspaceStateID = 0, WorkspaceState = "None"}},
                {1, new WorkspaceStateModelView(){ WorkspaceStateID = 1, WorkspaceState = "Initialization"}},
                {2, new WorkspaceStateModelView(){ WorkspaceStateID = 2, WorkspaceState = "Working"}},
                {3, new WorkspaceStateModelView(){ WorkspaceStateID = 3, WorkspaceState = "Locked"}},
                {4, new WorkspaceStateModelView(){ WorkspaceStateID = 4, WorkspaceState = "Complete"}},
                {5, new WorkspaceStateModelView(){ WorkspaceStateID = 5, WorkspaceState = "Closed"}}
            };

            commonDataMapper = new Mock<ICommonDataMapper>();
            commonDataMapper.Setup(x => x.getBOEStatesDictionary()).Returns(boeStatesDictionary);
            commonDataMapper.Setup(x => x.getWorkspaceStatesDictionary()).Returns(workspaceStatesDictionary);

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
        }

        [TestMethod]
        public void TestComputeTimeInOneStateOpenEnded()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object);

            Collection<BOEHistoryDTO> histories = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Draft, OldValue = BOEState_None, BoeID=1}
            };

            // this test shows a workspace in Init since the beginning of year 2011
            Assert.AreEqual((DateTime.Now - new DateTime(2011, 01, 01)).TotalHours.ToString("N2"),
                            sut.ComputeTimeSpan(histories, BOEState_Draft).TotalHours.ToString("N2"));

        }

        [TestMethod]
        public void TestComputeTimeInTwoStates()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object);

            Collection<BOEHistoryDTO> histories = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Draft, OldValue = BOEState_None, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Draft, BoeID=1}
            };

            // test for 2.5 days spent in init
            Assert.AreEqual((2.5).ToString("N2"),
                            sut.ComputeTimeSpan(histories, BOEState_Draft).TotalDays.ToString("N2"));
        }

        [TestMethod]
        public void TestComputeTimeInTwoInterestedStatesWithMixedOthers()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object);

            // 2 days in approved b/t index 1,2
            // 2 days b/t index 4,5
            Collection<BOEHistoryDTO> histories = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Draft, OldValue = BOEState_None, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Approved, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_AwaitingApproval, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Unassigned, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Approved, BoeID=1}
            };

            // test for 4 days spent in working
            Assert.AreEqual((4).ToString("N2"),
                            sut.ComputeTimeSpan(histories, BOEState_Approved).TotalDays.ToString("N2"));
        }

        [TestMethod]
        public void TestComputeTimeInThreeInterestedStatesWithMixedOthersOpenEnded()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object);

            // 2 days in Approved b/t index 1,2
            // 2 days b/t index 4,5
            // + days b/t 1/11/2011 and 'now'
            Collection<BOEHistoryDTO> histories = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Draft, OldValue = BOEState_None, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Approved, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_AwaitingApproval, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Unassigned, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Approved, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_Unassigned, BoeID=1}
            };

            // test for 4 days spent in working + extra days for open end
            Assert.AreEqual((4 + (DateTime.Now - new DateTime(2011, 01, 11, 00, 00, 00)).TotalDays).ToString("N2"),
                            sut.ComputeTimeSpan(histories, BOEState_Approved).TotalDays.ToString("N2"));
        }

        [TestMethod]
        public void ComputeNumberOfTimesInStateTest()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object);

            Collection<BOEHistoryDTO> histories = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Unassigned, OldValue = BOEState_None, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_AwaitingApproval, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Approved, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=1}
            };

            Assert.AreEqual(3,
                            sut.ComputeNumberOfTimesInState(histories, BOEState_Draft));

            Assert.AreEqual(0,
                            sut.ComputeNumberOfTimesInState(histories, BOEState_None));

            Assert.AreEqual(1,
                            sut.ComputeNumberOfTimesInState(histories, BOEState_AwaitingApproval));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void GenerateReportTest()
        {
            var _IBOEHistoryDTODataLoader = new Mock<IBOEHistoryDTODataLoader>();
            var _IUserDTODataLoader = new Mock<IUserDTODataLoader>();
            var _PermissionsLoader = new Mock<IPermissionsDTODataLoader>();
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _PermissionsLoader.Object);

            BOEActivityReport sut = new BOEActivityReport(_IBOEHistoryDTODataLoader.Object, _IUserDTODataLoader.Object, commonDataMapper.Object, _PermissionsLoader.Object);

            WorkspaceDTO workspace = new WorkspaceDTO
            {
                ProposalSubmittalDate = DateTime.Now + new TimeSpan(1, 0, 0, 0),
                Id = 1,
                NumberOfTimesExportedToProPricer = 1
            };

            UserDTO author1 = new UserDTO { UserID = 99, DisplayName = "author, joe" };
            UserDTO subcontractorAuthor1 = new UserDTO { UserID = 2, DisplayName = "sub, john" };
            UserDTO reviewer = new UserDTO { UserID = 2, DisplayName = "reviewer,jane" };
            UserDTO wsadmin = new UserDTO { UserID = 3, DisplayName = "wsadmin, dog" };

            Collection<BoeDTO> boes = new Collection<BoeDTO>
            {
                new BoeDTO{ AuthorIDs=new Collection<int>{author1.UserID}, Id=1, State=BOEState.Unassigned, WBSID=1, CLINID=1 },
                new BoeDTO{ AuthorIDs=new Collection<int>{author1.UserID}, Id=2, State=BOEState.Draft, WBSID=2, CLINID=2 },
                new BoeDTO{ AuthorIDs=new Collection<int>{author1.UserID}, Id=3, State=BOEState.AwaitingApproval, WBSID=3, CLINID=3 }
            };

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=author1.UserID,Role=Role.Author,BOEId=boes[0].Id}, 
             new PermissionsDTO{ETIUserId=subcontractorAuthor1.UserID,Role=Role.SubcontractorAuthor,BOEId=boes[0].Id} 
            };


            Collection<FullClin> clins = new Collection<FullClin>
            {
                new FullClin{ Id=1, ClinNumber="cone", ClinTitle="conetitle"},
                new FullClin{ Id=2, ClinNumber="ctwo", ClinTitle="ctwotitle"},
                new FullClin{ Id=3, ClinNumber="cthree", ClinTitle="cthreetitle"}
            };

            Collection<WbsDTO> wbss = new Collection<WbsDTO>
            {
                new WbsDTO{ Id=1, WbsNumber="wone", WbsTitle="wonetitle"},
                new WbsDTO{ Id=2, WbsNumber="wtwo", WbsTitle="wtwotitle"},
                new WbsDTO{ Id=3, WbsNumber="wthree", WbsTitle="wthreetitle"}
            };


            Collection<BOEHistoryDTO> histories0 = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Unassigned, OldValue = BOEState_None, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_AwaitingApproval, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Approved, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Draft, BoeID=1},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=1}};

            Collection<BOEHistoryDTO> histories1 = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=2},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Draft, BoeID=2},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_AwaitingApproval, BoeID=2},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Approved, BoeID=2},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Draft, BoeID=2},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=2}};

            Collection<BOEHistoryDTO> histories2 = new Collection<BOEHistoryDTO>{
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = BOEState_Unassigned, OldValue = BOEState_None, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = BOEState_AwaitingApproval, OldValue = BOEState_Draft, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = BOEState_Approved, OldValue = BOEState_AwaitingApproval, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Approved, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = BOEState_Unassigned, OldValue = BOEState_Draft, BoeID=3},
                new BOEHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = BOEState_Draft, OldValue = BOEState_Unassigned, BoeID=3}

            };

            _IUserDTODataLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<UserDTO>() { author1, subcontractorAuthor1, reviewer, wsadmin });

            _IBOEHistoryDTODataLoader.Setup(x => x.GetBOEHistory(boes[0].Id)).Returns(histories0);
            _IBOEHistoryDTODataLoader.Setup(x => x.GetBOEHistory(boes[1].Id)).Returns(histories1);
            _IBOEHistoryDTODataLoader.Setup(x => x.GetBOEHistory(boes[2].Id)).Returns(histories2);

            _PermissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(boeAuthorPermissions);

            FullWorkspace ws = new FullWorkspace(workspace);

            factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(ws);
            factory.Setup(x => x.CreateFullWorkspace(workspace.Id)).Returns(ws);
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(new List<FullBoe>() { new FullBoe(boes[0]), new FullBoe(boes[1]), new FullBoe(boes[2]) });
            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(clins);
            retriever.Setup(x => x.GetClinById(clins[0].Id)).Returns(clins[0]);
            retriever.Setup(x => x.GetClinById(clins[1].Id)).Returns(clins[1]);
            retriever.Setup(x => x.GetClinById(clins[2].Id)).Returns(clins[2]);
            factory.Setup(x => x.CreateFullClin(clins[0])).Returns(new FullClin(clins[0]));
            factory.Setup(x => x.CreateFullClin(clins[1])).Returns(new FullClin(clins[1]));
            factory.Setup(x => x.CreateFullClin(clins[2])).Returns(new FullClin(clins[2]));
            factory.Setup(x => x.CreateFullClin(clins[0].Id)).Returns(new FullClin(clins[0]));
            factory.Setup(x => x.CreateFullClin(clins[1].Id)).Returns(new FullClin(clins[1]));
            factory.Setup(x => x.CreateFullClin(clins[2].Id)).Returns(new FullClin(clins[2]));
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new List<FullWbs>() { new FullWbs(wbss[0]), new FullWbs(wbss[1]), new FullWbs(wbss[2]) });
            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>() { clins[0], clins[1], clins[2] });

            BOEActivityReportModelView mv = sut.GenerateReport(ws);
            Assert.IsTrue(mv.DaysLeftUntilProposalSubmittalDate.StartsWith("1 day left until proposal submittal"));
            Assert.AreEqual(clins[0].ClinTitle, mv.Rows[0].CLINTitle);
            Assert.AreEqual(clins[1].ClinTitle, mv.Rows[1].CLINTitle);
            Assert.AreEqual(clins[2].ClinTitle, mv.Rows[2].CLINTitle);

            Assert.AreEqual("2.0", mv.Rows[0].DaysInAwaitingApproval);
            Assert.AreEqual("2.0", mv.Rows[1].DaysInAwaitingApproval);
            Assert.AreEqual("2.0", mv.Rows[2].DaysInAwaitingApproval);

            Assert.AreEqual("3.0", mv.Rows[0].DaysInUnassigned);
            Assert.AreEqual("7.0", mv.Rows[1].DaysInUnassigned);
            Assert.AreEqual("3.0", mv.Rows[2].DaysInUnassigned);

        }
    }
}
