// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceActivityReportTest
    {
        [TestMethod]
        public void TestComputeTimeInOneStateOpenEnded()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);

            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1}
            };
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(1)).Returns(histories);

            // this test shows a workspace in Init since the beginning of year 2011
            Assert.AreEqual((DateTime.Now - new DateTime(2011, 01, 01)).TotalHours.ToString("N2"),
                            sut.ComputeTimeSpan(histories, IES.Common.WorkspaceState.Initialization).TotalHours.ToString("N2"));
        }

        [TestMethod]
        public void TestComputeTimeInTwoStates()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);

            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1}
            };
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(1)).Returns(histories);

            // test for 2.5 days spent in init
            Assert.AreEqual((2.5).ToString("N2"),
                            sut.ComputeTimeSpan(histories, IES.Common.WorkspaceState.Initialization).TotalDays.ToString("N2"));
        }


        [TestMethod]
        public void TestComputeTimeInTwoInterestedStatesWithMixedOthers()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);


            // 2 days in working b/t index 1,2
            // 2 days b/t index 4,5
            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Locked, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Closed, OldValue = IES.Common.WorkspaceState.Locked, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Closed, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1}
            };
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(1)).Returns(histories);

            // test for 4 days spent in working
            Assert.AreEqual((4).ToString("N2"),
                            sut.ComputeTimeSpan(histories, IES.Common.WorkspaceState.Working).TotalDays.ToString("N2"));
        }

        [TestMethod]
        public void TestComputeTimeInThreeInterestedStatesWithMixedOthersOpenEnded()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);


            // 2 days in working b/t index 1,2
            // 2 days b/t index 4,5
            // + days b/t 1/11/2011 and 'now'
            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Locked, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Closed, OldValue = IES.Common.WorkspaceState.Locked, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Closed, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1}
            };
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(1)).Returns(histories);

            // test for 4 days spent in working + extra days for open end
            Assert.AreEqual((4 + (DateTime.Now - new DateTime(2011, 01, 11, 00, 00, 00)).TotalDays).ToString("N2"),
                            sut.ComputeTimeSpan(histories, IES.Common.WorkspaceState.Working).TotalDays.ToString("N2"));
        }

        [TestMethod]
        public void ComputeNumberOfTimesInStateTest()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);


            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Locked, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Closed, OldValue = IES.Common.WorkspaceState.Locked, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Closed, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1}
            };
            Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(1)).Returns(histories);

            Assert.AreEqual(3,
                            sut.ComputeNumberOfTimesInState(histories, IES.Common.WorkspaceState.Working));

            Assert.AreEqual(0,
                            sut.ComputeNumberOfTimesInState(histories, IES.Common.WorkspaceState.None));

            Assert.AreEqual(1,
                            sut.ComputeNumberOfTimesInState(histories, IES.Common.WorkspaceState.Locked));
        }

        [TestMethod]
        public void GenerateReportTest()
        {
			Mock<IPermissionsDTODataLoader> _IPermissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            Mock<ICommonDataMapper> mockCommonDataMapper = new Mock<ICommonDataMapper>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
			Mock<IRetriever> retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), mockCommonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _IPermissionsDTOLoader.Object);

            WorkspaceDTO workspace = new WorkspaceDTO{
                ProposalSubmittalDate = new DateTime(2011, 1, 1),
                Id = 1,
                NumberOfTimesExportedToProPricer = 1
            };

            FullWorkspace ws = new FullWorkspace(workspace);

            UserDTO author = new UserDTO{ UserID = 1 };
            UserDTO reviewer = new UserDTO{ UserID = 2 };
            UserDTO wsadmin = new UserDTO { UserID = 3 };

            Collection<BoeDTO> boes = new Collection<BoeDTO>
            {
                new BoeDTO{ AuthorIDs=new Collection<int>{author.UserID}, Id=1, State=IES.Common.BOEState.Unassigned },
                new BoeDTO{ AuthorIDs=new Collection<int>{author.UserID}, Id=2, State=IES.Common.BOEState.Draft },
                new BoeDTO{ AuthorIDs=new Collection<int>{author.UserID}, Id=3, State=IES.Common.BOEState.AwaitingApproval }
            };

            List<PermissionsDTO> authorPermissions = new List<PermissionsDTO>
            {
                new PermissionsDTO{ BOEId=boes[0].Id, ETIUserId = author.UserID, Role = IES.Common.Role.Author, PermissionId=1},
                new PermissionsDTO{ BOEId=boes[1].Id, ETIUserId = author.UserID, Role = IES.Common.Role.Author, PermissionId=2},
                new PermissionsDTO{ BOEId=boes[2].Id, ETIUserId = author.UserID, Role = IES.Common.Role.Author, PermissionId=3}
            };
            List<PermissionsDTO> reviewerPermissions = new List<PermissionsDTO>
            {
                new PermissionsDTO{ BOEId=boes[0].Id, ETIUserId = reviewer.UserID, Role = IES.Common.Role.WorkspaceReviewer, PermissionId=4},
                new PermissionsDTO{ BOEId=boes[1].Id, ETIUserId = reviewer.UserID, Role = IES.Common.Role.WorkspaceReviewer, PermissionId=5},
                new PermissionsDTO{ BOEId=boes[2].Id, ETIUserId = reviewer.UserID, Role = IES.Common.Role.WorkspaceReviewer, PermissionId=6}
            };
            List<PermissionsDTO> wsPermissions = new List<PermissionsDTO>
            {
                new PermissionsDTO{ ETIUserId = wsadmin.UserID, Role = IES.Common.Role.WorkspaceAdmin, PermissionId=7},
                new PermissionsDTO{ ETIUserId = wsadmin.UserID, Role = IES.Common.Role.WorkspaceAdmin, PermissionId=8},
                new PermissionsDTO{ ETIUserId = wsadmin.UserID, Role = IES.Common.Role.WorkspaceAdmin, PermissionId=9}
            };

            Collection<WorkspaceHistoryDTO> histories = new Collection<WorkspaceHistoryDTO>{
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 01), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.None, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 03, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 05, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Locked, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 07, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Closed, OldValue = IES.Common.WorkspaceState.Locked, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 08, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Closed, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 10, 12, 00, 00), NewValue = IES.Common.WorkspaceState.Initialization, OldValue = IES.Common.WorkspaceState.Working, WorkspaceID=1},
                new WorkspaceHistoryDTO{Date = new DateTime(2011, 01, 11, 00, 00, 00), NewValue = IES.Common.WorkspaceState.Working, OldValue = IES.Common.WorkspaceState.Initialization, WorkspaceID=1}
            };

            List<PermissionsDTO> allPermissions = new List<PermissionsDTO>();
            allPermissions.AddRange(authorPermissions);
            allPermissions.AddRange(reviewerPermissions);
            allPermissions.AddRange(wsPermissions);

            Collection<PermissionsDTO> allPermissionsCollection = new Collection<PermissionsDTO>(allPermissions);
            _IPermissionsDTOLoader.Setup(x => x.GetWorkspacePermissions(workspace.Id)).Returns(allPermissionsCollection);


            List<int> boeIds = boes.Select(x => x.Id).ToList<int>();
            _IPermissionsDTOLoader.Setup(x => x.GetBOEPermissions(boeIds)).Returns(allPermissionsCollection);


            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { new FullBoe(boes[0]), new FullBoe(boes[1]), new FullBoe(boes[2]) });
            retriever.Setup(x => x.GetWorkspaceHistoryByWorkspaceId(workspace.Id)).Returns(histories);

            WorkspaceActivityReport sut = new WorkspaceActivityReport(_IPermissionsDTOLoader.Object);
            WorkspaceActivityReportModelView mv =  sut.GenerateReport(ws);
            Assert.AreEqual((1).ToString(), mv.NumberOfBOEsDraft);

        }
    }
}
