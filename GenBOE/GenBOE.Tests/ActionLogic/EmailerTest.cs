// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.Dtos;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EmailerTest : MOQObject
    {
        private Mock<IRetriever> retriever = new Mock<IRetriever>();
        private IDictionary<int, BOEStateModelView> boeStatesDictionary;
        private IDictionary<int, WorkspaceStateModelView> workspaceStatesDictionary;
        private Mock<ICommonDataMapper> commonDataMapper;
        private CommonDataMapper realMapper;
        [TestInitialize]
        new public void Setup()
        {
            boeStatesDictionary = new Dictionary<int, BOEStateModelView>()
            {
                {0, new BOEStateModelView(){ BOEStateID = 0, BOEState = "None" }},
                {1, new BOEStateModelView(){ BOEStateID = 1, BOEState = "Unassigned"}},
                {2, new BOEStateModelView(){ BOEStateID = 2, BOEState = "Draft"}},
                {3, new BOEStateModelView(){ BOEStateID = 3, BOEState = "Awaiting Approval"}},
                {4, new BOEStateModelView(){ BOEStateID = 4, BOEState = "Approved"}}
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

            this.realMapper = new CommonDataMapper(new CommonDataLoader(), new NonCacheDataLoader());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void SendBOEAuthorsWBSUpdatedTest()
        {
            //OUTPUT: We should see 2 emails sent to 2 different authors with a different list of approvers for each (1 shared approver)
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            // create 3 BOE DTOs, only 2 of which are in DRAFT state
            UserDTO[] approversArray = new UserDTO[] {
                new UserDTO { UserID = 1, DisplayName = "John Q. Approver", EmailAddress="johnqapprover@lmco.com"},
                new UserDTO { UserID = 2, DisplayName = "Mary Q. Approver", EmailAddress="maryqapprover@lmco.com" },
                new UserDTO { UserID = 3, DisplayName = "Jane Q. Doe", EmailAddress="janedoeapprover@lmco.com" }
            };

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"},
                new UserDTO { UserID = 6, DisplayName = "Mary Q. Author", EmailAddress="maryqauthor@lmco.com" }
            };

            ClinDTO clin = new ClinDTO { Id = 1, ClinNumber = "clinnum1", ClinTitle = "clin title1" };

            WbsDTO wbs = new WbsDTO { Id = 1, inUse = true, WbsNumber = "wbsnum1", WbsTitle = "wbs title1" };

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            Collection<BoeDTO> boes = new Collection<BoeDTO> { };
            boes.Add(new BoeDTO
            {
                WorkspaceID = ws1.Id,
                CLINID = clin.Id,
                WBSID = wbs.Id,
                Id = 1,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE One",
                AuthorIDs = new Collection<int>{authorsArray[0].UserID}
            });

            Collection<BoeApproverResponseDTO> boeApprovers1 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = approversArray[0].UserID }, new BoeApproverResponseDTO { ETIUserID = approversArray[1].UserID } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(boeApprovers1);
            Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = approversArray[0].UserID }, new PermissionsDTO { ETIUserId = approversArray[1].UserID } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){1})).Returns(perm1);

            Collection<BoeApproverResponseDTO> boeApprovers2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = approversArray[1].UserID }, new BoeApproverResponseDTO { ETIUserID = approversArray[2].UserID } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprovers2);
            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = approversArray[1].UserID }, new PermissionsDTO { ETIUserId = approversArray[2].UserID } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);
            boes.Add(new BoeDTO
            {
                WorkspaceID = ws1.Id,
                WBSID = wbs.Id,
                Id = 2,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE Two",
                AuthorIDs = new Collection<int> { authorsArray[1].UserID }
            });

            // DRAFT BOE, not included
            boes.Add(new BoeDTO
            {
                WBSID = wbs.Id,
                WorkspaceID = ws1.Id,
                Id = 3,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE NO AUTHOR"
            });

            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(3)).Returns(new Collection<BoeApproverResponseDTO> { });

            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);
            userLoader.Setup(x => x.GetUserByID(6)).Returns(authorsArray[1]);

            UserData user = new UserData{DisplayName = "Active User", Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            Collection<FieldChanged> fieldsChanged = new Collection<FieldChanged>();
            fieldsChanged.Add(new FieldChanged { Field = "WBS #", OldValue = "1.1", NewValue = "1.2" });
            fieldsChanged.Add(new FieldChanged { Field = "WBS Title", OldValue = "Program Management", NewValue = "Finance" });

            Collection<PermissionsDTO> boeAuthorPermissions1 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[0].UserID,Role=Role.Author,BOEId=boes[0].Id} 
            };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){ boes[0].Id })).Returns(boeAuthorPermissions1);

            Collection<PermissionsDTO> boeAuthorPermissions2 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[1].UserID,Role=Role.Author,BOEId=boes[1].Id} 
            };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boes[1].Id})).Returns(boeAuthorPermissions2);



            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);


            foreach (BoeDTO boe in boes)
            {
                FullBoe boeObject = new FullBoe(boe);
                this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
                this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
                factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
                this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));
                factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));

                sut.PrivateSendBOEUpdatedEmails(boeObject, fieldsChanged, EmailTypes.BOEUpdatedToAuthorsAndApprovers, user);
                sut.PrivateSendBOEUpdatedEmails(boeObject, fieldsChanged, EmailTypes.CLINUpdatedToAuthorsAndApprovers, user);
            }
            userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(6), Times.AtLeastOnce());
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void SendBOEAuthorsAndApproversWBSUpdatedTest()
        {
            //OUTPUT: We should see 2 emails sent to 2 different authors with a different list of approvers for each (1 shared approver)
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            // create 3 BOE DTOs, only 2 of which are in DRAFT state
            UserDTO[] approversArray = new UserDTO[] {
                new UserDTO { UserID = 1, DisplayName = "John Q. Approver", EmailAddress="johnqapprover@lmco.com"},
                new UserDTO { UserID = 2, DisplayName = "Mary Q. Approver", EmailAddress="maryqapprover@lmco.com" },
                new UserDTO { UserID = 3, DisplayName = "Jane Q. Doe", EmailAddress="janedoeapprover@lmco.com" }
            };

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"},
                new UserDTO { UserID = 6, DisplayName = "Mary Q. Author", EmailAddress="maryqauthor@lmco.com" }
            };

            ClinDTO clin = new ClinDTO { Id = 1, ClinNumber = "clinnum1", ClinTitle = "clin title1" };

            WbsDTO wbs = new WbsDTO { Id = 1, inUse = true, WbsNumber = "wbsnum1", WbsTitle = "wbs title1" };

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            Collection<BoeDTO> boes = new Collection<BoeDTO> { };
            boes.Add(new BoeDTO
            {
                WorkspaceID = ws1.Id,
                CLINID = clin.Id,
                WBSID = wbs.Id,
                Id = 1,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE One",
                AuthorIDs = new Collection<int> { authorsArray[0].UserID }

            });
            Collection<BoeApproverResponseDTO> boeApprover1 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = approversArray[0].UserID }, new BoeApproverResponseDTO { ETIUserID = approversArray[1].UserID } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(boeApprover1);

            Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = approversArray[0].UserID }, new PermissionsDTO { ETIUserId = approversArray[1].UserID } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){ 1 })).Returns(perm1);

            boes.Add(new BoeDTO
            {
                WorkspaceID = ws1.Id,
                WBSID = wbs.Id,
                Id = 2,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE Two",
                AuthorIDs = new Collection<int> { authorsArray[1].UserID }
            });

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = approversArray[1].UserID }, new BoeApproverResponseDTO { ETIUserID = approversArray[2].UserID } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);
            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = approversArray[1].UserID }, new PermissionsDTO { ETIUserId = approversArray[2].UserID } };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() {2})).Returns(perm2);

            // DRAFT BOE, not included
            boes.Add(new BoeDTO
            {
                WBSID = wbs.Id,
                WorkspaceID = ws1.Id,
                Id = 3,
                State = BOEState.Draft,
                Title = "Test Title",
                Description = "BOE NO AUTHOR"
            });

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){ 3 })).Returns(new Collection<PermissionsDTO> { });

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approversArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approversArray[1]);
            userLoader.Setup(x => x.GetUserByID(3)).Returns(approversArray[2]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);
            userLoader.Setup(x => x.GetUserByID(6)).Returns(authorsArray[1]);

            Collection<PermissionsDTO> boeAuthorPermissions1 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[0].UserID,Role=Role.Author,BOEId=1} 
            };

            Collection<PermissionsDTO> boeAuthorPermissions2 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[1].UserID,Role=Role.Author,BOEId=2} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){ 1 })).Returns(boeAuthorPermissions1);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { 2 })).Returns(boeAuthorPermissions2);

            UserData user = new UserData { DisplayName = "Active User", Email = "" };
            secInfo.Setup(x => x.ActiveUserData).Returns(user);
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");

            Collection<FieldChanged> fieldsChanged = new Collection<FieldChanged>();
            fieldsChanged.Add(new FieldChanged { Field = "WBS #", OldValue = "1.1", NewValue = "1.2" });
            fieldsChanged.Add(new FieldChanged { Field = "WBS Title", OldValue = "Program Management", NewValue = "Finance" });

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            foreach (BoeDTO boe in boes)
            {
                FullBoe boeObject = new FullBoe(boe);
                this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
                this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
                factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
                factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
                this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

                sut.SendBOEUpdatedToAuthorsAndApprovers(boeObject, fieldsChanged);
                sut.SendCLINUpdatedToAuthorsAndApprovers(boeObject, fieldsChanged);
                sut.PrivateSendBOEUpdatedEmails(boeObject, fieldsChanged, EmailTypes.BOEUpdatedToAuthorsAndApprovers, user);
                sut.PrivateSendBOEUpdatedEmails(boeObject, fieldsChanged, EmailTypes.CLINUpdatedToAuthorsAndApprovers, user);
            }

            userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(6), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOESubmittedForReviewTest()
        {
            //OUTPUT: We should see 1 email sent to 2 reviewers
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = ws1.Id, State = BOEState.AwaitingApproval, AuthorIDs = new Collection<int>{5}, Title = "Test Title", Description = "TEST BOE DESC", WBSID = 1, CLINID = 1 };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));

            Collection<BoeApproverResponseDTO> boeApprovers = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(boeApprovers);
            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 3, Role = Role.WorkspaceReviewer, WorkspaceId = ws1.Id },
                            new PermissionsDTO { ETIUserId = 4, Role = Role.WorkspaceReviewer, WorkspaceId = ws1.Id }
                });
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() {1})).
                Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { BOEId=1, ETIUserId = 1, Role = Role.Approver, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=1, ETIUserId = 2, Role = Role.Approver, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=1, ETIUserId = 5, Role = Role.Author, WorkspaceId = ws1.Id }
                });

            factory.Setup(x => x.CreateFullBoe(boe)).Returns(new FullBoe(boe) );
            this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(ws1));

            UserDTO[] approversArray = new UserDTO[] {
                new UserDTO { UserID = 1, DisplayName = "John Q. Approver", EmailAddress="johnqapprover@lmco.com"},
                new UserDTO { UserID = 2, DisplayName = "Mary Q. Approver", EmailAddress="maryqapprover@lmco.com" }
            };

            UserDTO[] reviewersArray = new UserDTO[] {
                new UserDTO { UserID = 3, DisplayName = "John Q. Reviewer", EmailAddress="johnqreviewer@lmco.com"},
                new UserDTO { UserID = 4, DisplayName = "Mary Q. Reviewer", EmailAddress="maryqreviewer@lmco.com" }
            };

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"}
            };

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approversArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approversArray[1]);
            userLoader.Setup(x => x.GetUserByID(3)).Returns(reviewersArray[0]);
            userLoader.Setup(x => x.GetUserByID(4)).Returns(reviewersArray[1]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.AwaitingApproval)).Returns("Awaiting Approval");
            UserData user = new UserData{Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOESubmittedForReview(boeObject);
            sut.PrivateSendBOESubmittedForReview(boeObject, user);

            permLoader.Verify(x => x.GetBOEPermissions(new List<int>() { 1 }), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(1), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(2), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(3), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(4), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOECLINWBSChangedTest()
        {
            //OUTPUT: We should see 1 email sent to 2 authors

            var secInfo = new Mock<ISecurityInformation>();

            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();

            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = ws1.Id, State = BOEState.AwaitingApproval, AuthorIDs = new Collection<int> { 5 }, Title = "Test Title", Description = "TEST BOE DESC", WBSID = 1, CLINID = 1 };
            FullBoe boeObject = new FullBoe(boe);
            List<BoeTaskElementDTO> tasks = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO {
                    BoeID = boe.Id,
                    TaskTitle = "Task 1",
                    Id = 50,
                    OrdinaryVariables = new Collection<OrdinaryVariableDto> {
                        new OrdinaryVariableDto {
                            BoeID = boe.Id,
                            Id = 3,
                            SortBOEBy = VarSortBOEBy.CLIN,
                            ValueType = VarValueType.SumOfBOEs,
                            OrdinaryVariableName = "CLIN_Butter"
                        }
                    }
                },
                new BoeTaskElementDTO {
                    BoeID = boe.Id,
                    TaskTitle = "Task 2",
                    Id = 51,
                    OrdinaryVariables = new Collection<OrdinaryVariableDto> {
                        new OrdinaryVariableDto {
                            BoeID = boe.Id,
                            Id = 3,
                            SortBOEBy = VarSortBOEBy.WBS,
                            ValueType = VarValueType.SumOfBOEs,
                            OrdinaryVariableName = "WBS_Toast"
                        }
                    }
                }
            };

            this.retriever.Setup(x => x.GetClinById(1)).Returns(Clin1);
            this.retriever.Setup(x => x.GetWbsById(1)).Returns(this.Wbs);
            factory.Setup(x => x.CreateFullClin(It.IsAny<ClinDTO>())).Returns(new FullClin(Clin1));
            factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(new FullWbs(Wbs));
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(boeObject.WorkspaceID, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(tasks);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boe.Id })).
                Returns(new Collection<PermissionsDTO>{
                            new PermissionsDTO { BOEId=1, ETIUserId = 4, Role = Role.Author, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=1, ETIUserId = 5, Role = Role.Author, WorkspaceId = ws1.Id }
                });

            factory.Setup(x => x.CreateFullBoe(boe)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(ws1));

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"},
                new UserDTO { UserID = 4, DisplayName = "Mary Q. Author", EmailAddress="maryqauthor@lmco.com" }
            };

            userLoader.Setup(x => x.GetUserByID(4)).Returns(authorsArray[1]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.AwaitingApproval)).Returns("Awaiting Approval");
            UserData user = new UserData { Email = "" };
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOECLINWBSChanged(boeObject, true, true, true);
            sut.PrivateSendBOECLINWBSChanged(boeObject, true, true, true, user);

            permLoader.Verify(x => x.GetBOEPermissions(new List<int>() { boe.Id }), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(4), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());

            sut.PrivateSendBOECLINWBSChanged(boeObject, true, false, false, user);
            sut.PrivateSendBOECLINWBSChanged(boeObject, false, true, false, user);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void SendAuthorsEmailOpenedForEditTest()
        {
            // OUTPUT : 1 email to author with the approvers CCed detailing the workspace/boe

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            BoeDTO boe2 = new BoeDTO { State = BOEState.Draft, Id = 2, CLINID=1, WBSID=1, AuthorIDs = new Collection<int>{5}, WorkspaceID = workspaceID };

            Collection<BoeApproverResponseDTO> boeApprovers1 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(boeApprovers1);

            Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1 }, new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() {1})).Returns(perm1);


            Collection<BoeApproverResponseDTO> boeApprovers2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1 }, new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);

            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprovers2);
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(3)).Returns(new Collection<BoeApproverResponseDTO> { });
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(1)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 1 } });
            retriever.Setup(x => x.GetClinsByWorkspaceId(1)).Returns(new Collection<FullClin>() { new FullClin() { Id = 1 } });
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){3})).Returns(new Collection<PermissionsDTO> { });

            Collection<UserDTO> approverCollection = new Collection<UserDTO> {new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" } };

            Collection<UserDTO> authorCollection = new Collection<UserDTO> {new UserDTO { UserID=3, DisplayName="author johnson", EmailAddress = "author1@lmco.com" },
                new UserDTO { UserID =4,  DisplayName="approver2 taylor", EmailAddress = "author2@lmco.com" } };

            ICollection<UserDTO> allUsers = new Collection<UserDTO>() { approverCollection[0], approverCollection[1], authorCollection[0], authorCollection[1] };

            userLoader.Setup(x => x.GetByIds(new Collection<int>() { 1, 2, 3, 4 })).Returns(allUsers);

            FullWorkspace workspace = new FullWorkspace(ws1);
            FullBoe boeObject = new FullBoe(boe2);
            this.retriever.Setup(x => x.GetWorkspaceById(boe2.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boeObject.Id)).Returns(new FullBoe(boeObject));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorCollection[0].UserID,Role=Role.Author,BOEId=boe2.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){ boe2.Id })).Returns(boeAuthorPermissions);
            userLoader.Setup(x => x.GetByIds(new Collection<int>() { 3 })).Returns(new Collection<UserDTO>() { authorCollection[0] });

            EmailModelDomain email = new EmailModelDomain
            {
                Subject = @"genBOE: BOE Open For Edits",
                Body = @"The following BOE is now open for edits by the Author. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE Status: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}",
                EmailType = EmailTypes.BOEOpenedForEdit
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });
            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");

            UserData user = new UserData{ Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);


            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendWorkspaceAuthorsEmailOpenedForEdit(workspace);
            sut.PrivateSendWorkspaceStatusChange(boeObject, boeObject.Workspace, boeObject.Wbs, boeObject.Clin, approverCollection, authorCollection, user, EmailTypes.BOEOpenedForEdit);
        }

        [TestMethod]
        public void SendEmailWithCC()
        {
            this.SendEmail(true);
        }

        [TestMethod]
        public void SendEmailWithoutCC()
        {
            this.SendEmail(false);
        }


        private void SendEmail(bool inDoCC)
        {
            // test sending an email successfully ... 
            // includes using replacement tokens

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            UserDTO[] ccesArray = new UserDTO[] { new UserDTO { EmailAddress = "cc1@lmco.com" },
                                             new UserDTO { EmailAddress = "cc2@lmco.com" }
                                           };
            Collection<UserDTO> cces = new Collection<UserDTO>(ccesArray);

            EmailModelDomain email = new EmailModelDomain
            {
                Body = "generation TEST - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation TEST - Hi {0}, let me introduce myself",
                EmailType = EmailTypes.ApproverEmailBOEAwaitingApproval
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });


            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);
            UserData user = new UserData {Email="" };
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendEmail(EmailTypes.ApproverEmailBOEAwaitingApproval,
                           "willbereplaced@lmco.com", //sender will be swapped out b/c resource accounts are equal
                           inDoCC ? cces : new Collection<UserDTO>(),// pass in cclist if specified
                           new string[] { "SubjectName" },
                           new string[] { "BodyName", "Tired" },
                           null, user, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendEmailInvalidTokensBody()
        {
            // attempt to send an email with invalid string tokens in the body
            // (should fail before getting very far)

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
            
            EmailModelDomain email = new EmailModelDomain
            {
                Body = "Hello {0}.  How are you?  I am {1}!",
                Subject = "Hi {0}, let me introduce myself",
                EmailType = EmailTypes.ApproverEmailBOEAwaitingApproval,
                DefaultOn = true
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });

            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            UserData user = new UserData { Email = "" };
            sut.SendEmail(EmailTypes.ApproverEmailBOEAwaitingApproval,
                           "donotdeliver@lmco.com",
                           null, // no CC's
                           new string[] { "SubjectName" },
                           new string[] { "BodyName", "Tired", "InvalidBodyToken" },
                           null, user, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendEmailInvalidTokensSubject()
        {
            // attempt to send an email with invalid string tokens in the body
            // (should fail before getting very far)

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());

            EmailModelDomain email = new EmailModelDomain
            {
                Body = "Hello {0}.  How are you?  I am {1}!",
                Subject = "Hi {0}, let me introduce myself",
                EmailType = EmailTypes.ApproverEmailBOEAwaitingApproval,
                DefaultOn = true
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });

            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            UserData user = new UserData { Email = "" };
            sut.SendEmail(EmailTypes.ApproverEmailBOEAwaitingApproval,
                           "donotdeliver@lmco.com",
                           null, // no CC's
                           new string[] { "SubjectName", "InvalidSubjectToken" },
                           new string[] { "BodyName", "Tired" },
                           null,user, 1);
        }

        [TestMethod]
        public void SendBOEAuthorsEmailOpenedForEdit()
        {
            // OUTPUT : 1 email to all authors detailing the boe

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApproers = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApproers);
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(1)).Returns(new Collection<FullWbs>() { new FullWbs() { Id = 1 } });
            retriever.Setup(x => x.GetClinsByWorkspaceId(1)).Returns(new Collection<FullClin>() { new FullClin() { Id = 1 } });

            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1 }, new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);


            Collection<UserDTO> approverArray = new Collection<UserDTO> {
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" }
            };

            Collection<UserDTO> authorArray = new Collection<UserDTO> {
                new UserDTO { UserID=3, DisplayName="author1 johnson", EmailAddress = "author1@lmco.com" },
                new UserDTO { UserID=4,  DisplayName="author2 taylor", EmailAddress = "author2@lmco.com" }
            };
            userLoader.Setup(x => x.GetUserByID(3)).Returns(authorArray[0]);

            BoeDTO boe = new BoeDTO { State = BOEState.Approved, Id = 2, CLINID = 1, WBSID = 1, AuthorIDs = new Collection<int> { 3, 4 }, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };// new BoeDTO { ID = 1, WorkspaceID = workspaceID, State = BOEState.Approved };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            Collection<BoeApproverResponseDTO> boeApprovers2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprovers2);
            EmailModelDomain email = new EmailModelDomain
            {
                Subject = @"genBOE: BOE Open For Edits",
                Body = @"The following BOE is now open for edits by the Authors. The Authors must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE Status: {5}<BR/>Author(s): {6} <BR/> Approver(s): {7}<BR/><BR/>{8}",
                EmailType = EmailTypes.BOEOpenedForEdit
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });
            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Approved)).Returns("Approved");

            UserData user = new UserData{Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorArray[0].UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);


            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEAuthorsEmailOpenedForEdit(boeObject, boeObject.Workspace);
            sut.PrivateSendWorkspaceStatusChange(boeObject, boeObject.Workspace, boeObject.Wbs, boeObject.Clin, approverArray, authorArray, user, EmailTypes.BOEOpenedForEdit);

            permLoader.Verify(x => x.GetBOEPermissions(new List<int>(){2}), Times.AtLeastOnce());
             userLoader.Verify(x => x.GetUserByID(3), Times.Once());
        }

        [TestMethod]
        public void SendBOEAuthorRespondedToComment()
        {
            // An email is sent to each user who has entered a comment that the Author has responded to.  “View comments and responses” should be a hyperlink that directly opens the BOE comments.

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApprover = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover);

            boeCommentLoader.Setup(x => x.GetByIds(new List<int>(){2})).Returns(new Collection<BOECommentDTO>
            {
                new BOECommentDTO{Id=1, BOECommentETIUserID=1, BoeID=2, BOEComment="I hated this BOE", BOEResponseToCommentID=null}
            });

            boeCommentLoader.Setup(x => x.GetByIds(new List<int>(){1})).Returns(new Collection<BOECommentDTO> 
            {
                new BOECommentDTO{ Id = 1, BOECommentETIUserID = 1, BoeID = 2, BOEComment = "I hated this BOE", BOEResponseToCommentID = null }
            });
            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };

            userLoader.Setup(x => x.GetUserByID(5)).Returns(author);
            userLoader.Setup(x => x.GetUserByID(1)).Returns(new UserDTO { UserID = 1, EmailAddress = "uthor.dillahunty@lmco.com" });


            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);

            BoeDTO boe = new BoeDTO { State = BOEState.Approved, Id = 2, CLINID = 1, WBSID = 1, AuthorIDs = new Collection<int> {5}, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };
            Collection<BOECommentDTO> boeComments = new Collection<BOECommentDTO> { 
                {new BOECommentDTO {Id=2, BOEResponseToCommentID=1, BOEComment="you shouldn't hate", BoeID=2, BOECommentETIUserID=5 } }
            };
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Approved)).Returns("Approved");

            factory.Setup(x => x.CreateFullBoe(boeComments[0].BoeID)).Returns(new FullBoe(boe));
            retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(ws1);
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            UserData user = new UserData{ Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=5,Role=Role.Author,BOEId=2} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){1})).Returns(boeAuthorPermissions);


            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEAuthorRespondedToComment(boeComments);
            sut.PrivateSendBOEAuthorRespondedToComment(boeComments, user);

            userLoader.Verify(x => x.GetUserByID(5), Times.Once());
            boeCommentLoader.Verify(x => x.GetByIds(new List<int>(){1}), Times.AtLeastOnce());

        }

        [TestMethod]
        public void SendBOEApproversAuthorApproverApproved()
        {
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            BoeDTO boe = new BoeDTO
            {
                State = BOEState.AwaitingApproval,
                Id = 2,
                CLINID = 1,
                WBSID = 1,
                AuthorIDs = new Collection<int> {5},
                WorkspaceID = workspaceID,
                Title = "Test Title",
                Description = "Test Description"
            };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            Collection<BoeApproverResponseDTO> boeApprovers = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1, ApproverResponse = ApproverReponseType.Approved, BoeID=boe.Id },
                    new BoeApproverResponseDTO { ETIUserID = 2 , BoeID=boe.Id}};

            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(boe.Id)).Returns(boeApprovers);

            UserDTO[] approverArray = new UserDTO[] {
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" }
            };

            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=author.UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approverArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approverArray[1]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(author);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.AwaitingApproval)).Returns("Awaiting Approval");
            
            UserData user = new UserData{ Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEApproversAuthorApproverApproved(boeApprovers[0], boeObject);
            sut.PrivateSendBOEApproversAuthorApproverApproved(boeApprovers[0], boeObject, user);

            userLoader.Verify(x => x.GetUserByID(5), Times.Once());
            userLoader.Verify(x => x.GetUserByID(1), Times.AtLeastOnce());
            userLoader.Verify(x => x.GetUserByID(2), Times.Once());
        }

        [TestMethod]
        public void SendBOEApproversAuthorApproverRejected()
        {
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> {new BoeApproverResponseDTO { ETIUserID = 1 },
                    new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);

            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> {new PermissionsDTO { ETIUserId = 1 },
                    new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);

            BoeDTO boe = new BoeDTO
            {
                State = BOEState.Draft,
                Id = 2,
                CLINID = 1,
                WBSID = 1,
                AuthorIDs = new Collection<int> {5},
                WorkspaceID = workspaceID,
                Title = "Test Title",
                Description = "Test Description"
            };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));

            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            UserDTO[] approverArray = new UserDTO[] {
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" }
            };

            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=author.UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approverArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approverArray[1]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(author);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");

            UserData user = new UserData{ Email = ""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);
            userLoader.Setup(x => x.GetUserByID(author.UserID)).Returns(author);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, 
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEApproversAuthorApproverRejected(boeApprover2[0], boeObject);
            sut.PrivateSendBOEApproversAuthorApproverRejected(boeApprover2[0], boeObject, user);

            userLoader.Verify(x => x.GetUserByID(5), Times.Once());
            permLoader.Verify(x => x.GetBOEPermissions(new List<int>(){2}), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendApproverEmailBOEAwaitingApprovalTest()
        {
            // OUTPUT : 1 email to all approvers detailing the boe is awaiting approval

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(new Collection<BoeApproverResponseDTO> { });
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(3)).Returns(new Collection<BoeApproverResponseDTO> { });

            Collection<UserDTO> approverArray = new Collection<UserDTO>{
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" }
            };

            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };
            userLoader.Setup(x => x.GetUserByID(1)).Returns(approverArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approverArray[1]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(author);

            BoeDTO boe = new BoeDTO { State = BOEState.AwaitingApproval, Id = 2, CLINID = 1, WBSID = 1, AuthorIDs = new Collection<int> { author.UserID }, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };
            FullBoe boeObject = new FullBoe(boe);
            retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=author.UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);

            EmailModelDomain email = new EmailModelDomain
            {
                Subject = @"genBOE: BOE Awaiting Approval",
                Body = @"The Author has completed work on the following BOE.  You may approve the BOE or reject it to send it back to the Author for rework.<BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE Description: {5}<BR/>BOE Status: {6}<BR/>Author: {7} <BR/> Approver(s): {8}<BR/><BR/>{9}",
                EmailType = EmailTypes.ApproverEmailBOEAwaitingApproval
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });
            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.AwaitingApproval)).Returns("Awaiting Approval");

            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                      userLoader.Object, permLoader.Object,
                                      boeCommentLoader.Object,  
                                      dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEApproversEmailAwaitingApproval(boeObject);
            sut.PrivateSendApproverEmailBOEAwaitingApproval(boeObject, approverArray, user);
        }

        [TestMethod]
        public void SendWorkspaceAdminEmailAllBOEsApprovedTest()
        {
            //OUTPUT email to all workspace admins if all BOEs have been approved
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(new Collection<BoeApproverResponseDTO> { });
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(3)).Returns(new Collection<BoeApproverResponseDTO> { });

            UserDTO[] approverArray = new UserDTO[] {
                new UserDTO { UserID=1, DisplayName="admin1 jones", EmailAddress = "admin1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="admin2 smith", EmailAddress = "admin2@lmco.com" }
            };

            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { ETIUserId = 1, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id },
                            new PermissionsDTO { ETIUserId = 2, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }
                });

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approverArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approverArray[1]);

            BoeDTO boe = new BoeDTO { State = BOEState.Approved, Id = 2, CLINID = 1, WBSID = 1, AuthorIDs = new Collection<int> {5}, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(boeObject.WorkspaceID)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            commonDataMapper.Setup(x => x.getWorkspaceStateName(WorkspaceState.Working)).Returns("Working");

            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendWorkspaceAdminEmailAllBOEsApproved(boeObject);
            sut.PrivateSendWorkspaceAdminEmailAllBOEsApproved(boeObject, user);

            commonDataMapper.Verify(x => x.getWorkspaceStateName(WorkspaceState.Working), Times.AtLeastOnce());
            permLoader.Verify(x => x.GetWorkspacePermissions(ws1.Id), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOEAuthorChangedTest()
        {
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 },
                    new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);

            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1 },
                    new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);

            BoeDTO boe = new BoeDTO
            {
                State = BOEState.Draft,
                Id = 2,
                CLINID = 1,
                WBSID = 1,
                AuthorIDs = new Collection<int>{5},
                WorkspaceID = workspaceID,
                Title = "Test Title",
                Description = "Test Description"
            };

            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            Collection<UserDTO> authorArray = new Collection<UserDTO> {
                new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" },
                new UserDTO { DisplayName = "author doe", UserID = 6, EmailAddress = "author.doe@lmco.com" }
            };

            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorArray[0]);
            userLoader.Setup(x => x.GetUserByID(6)).Returns(authorArray[1]);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorArray[0].UserID,Role=Role.Author,BOEId=boe.Id}, 
             new PermissionsDTO{ETIUserId=authorArray[1].UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);

            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEAuthorsChanged(authorArray, boeObject);
            sut.PrivateSendBOEAuthorsChanged(authorArray, boeObject, user);

             userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOEApproversChangedTest()
        {
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };
            
            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1, ApproverResponse = ApproverReponseType.Approved },
                    new BoeApproverResponseDTO { ETIUserID = 2 }};
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);

            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1, Role=Role.Approver },
                    new PermissionsDTO { ETIUserId = 2, Role = Role.Approver },
                    new PermissionsDTO { ETIUserId=author.UserID,Role=Role.Author}};

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);
            BoeDTO boe = new BoeDTO
            {
                State = BOEState.AwaitingApproval,
                Id = 2,
                CLINID = 1,
                WBSID = 1,
                AuthorIDs = new Collection<int> { author.UserID},
                WorkspaceID = workspaceID,
                Title = "Test Title",
                Description = "Test Description"
            };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            UserDTO[] approverArray = new UserDTO[] {
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" },
                new UserDTO { UserID =3,  DisplayName="previous approver3 doe", EmailAddress = "approver3@lmco.com" }
            };

            userLoader.Setup(x => x.GetUserByID(1)).Returns(approverArray[0]);
            userLoader.Setup(x => x.GetUserByID(2)).Returns(approverArray[1]);
            userLoader.Setup(x => x.GetUserByID(3)).Returns(approverArray[2]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(author);

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.AwaitingApproval)).Returns("Awaiting Approval");

            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEApproversChanged(new Collection<UserDTO> { approverArray[2] }, boeObject);

            sut.PrivateSendBOEApproversChanged(new Collection<UserDTO> { approverArray[2] }, boeObject, user);

            commonDataMapper.Verify(x => x.getBOEStateName(BOEState.AwaitingApproval), Times.Once());
             userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void SendBOEDeleted()
        {
            // An email is sent to an author, approver, and workspace admins if a BOE they have been assigned to has
            // been deleted from the Manage BOE and Manage WBS page
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());

            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = 1 }, new BoeApproverResponseDTO { ETIUserID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);
            
            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = 1 }, new PermissionsDTO { ETIUserId = 2 } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);

            UserDTO[] approverArray = new UserDTO[] {
                new UserDTO { UserID=1, DisplayName="approver1 jones", EmailAddress = "approver1@lmco.com" },
                new UserDTO { UserID =2,  DisplayName="approver2 smith", EmailAddress = "approver2@lmco.com" }
            };

            UserDTO author = new UserDTO { DisplayName = "author dillahunty", UserID = 5, EmailAddress = "author.dillahunty@lmco.com" };
            var completeListOfUsers = new Collection<UserDTO>()
            {
                approverArray[0],
                approverArray[1],
                author
            };
            userLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(completeListOfUsers);

            WbsDTO wbs = new WbsDTO {Id = 1, WbsNumber = "wbsnum1", WbsTitle = "wbs title1" };

            BoeDTO boe = new BoeDTO { State = BOEState.Draft, Id = 2, CLINID = 1, WBSID = 1, AuthorIDs = new Collection<int>{5}, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");

            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            UserDTO deletedByUser = new UserDTO { DisplayName = "Meanie, Deleted", UserID = 9, EmailAddress = "deletedmeaine@lmco.com" };

            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).
               Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 13, Role = Role.WorkspaceUser, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 10, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 11, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }, // duplicate userid
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 12, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }
                });

            UserDTO[] adminsArray = new UserDTO[] {
                new UserDTO { UserID=13, EmailAddress = "admin13@lmco.com" },
                new UserDTO { UserID = 10, EmailAddress = "admin10@lmco.com" },
                new UserDTO { UserID = 11, EmailAddress = "admin11@lmco.com" },
                new UserDTO { UserID = 12, EmailAddress = "admin12@lmco.com" }
            };

            userLoader.Setup(x => x.GetUserByID(adminsArray[0].UserID)).Returns(adminsArray[0]);
            userLoader.Setup(x => x.GetUserByID(adminsArray[1].UserID)).Returns(adminsArray[1]);
            userLoader.Setup(x => x.GetUserByID(adminsArray[2].UserID)).Returns(adminsArray[2]);
            userLoader.Setup(x => x.GetUserByID(adminsArray[3].UserID)).Returns(adminsArray[3]);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);
            
            Collection<int> approverCollection = new Collection<int>{1,2};
            sut.SendBOEDeleted(boe, deletedByUser, approverCollection, wbs,  this.Clin1, factory.Object.CreateFullWorkspace(ws1), this.boeStatesDictionary);
            sut.PrivateSendBOEDeleted(boeObject, deletedByUser, user, approverCollection, wbs, this.Clin1, factory.Object.CreateFullWorkspace(ws1), boeStatesDictionary);

            permLoader.Verify(x => x.GetBOEPermissions(new List<int>(){2}), Times.Never());
            permLoader.Verify(x => x.GetWorkspacePermissions(ws1.Id), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOEDeleted_NoApproverAuthor()
        {
            // An email is sent to an author, approver, and workspace admins if a BOE they have been assigned to has
            // been deleted from the Manage BOE and Manage WBS page

            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            int workspaceID = 1;
            WorkspaceDTO ws1 = new WorkspaceDTO { Id = workspaceID, WorkspaceName = "Test Workspace+BOE ONE", Shortname = "testws1", WorkspaceState = WorkspaceState.Working };

            WbsDTO wbs = new WbsDTO {Id = 1, WbsNumber = "wbsnum1", WbsTitle = "wbs title1" };

            BoeDTO boe = new BoeDTO { State = BOEState.Draft, Id = 2, CLINID = 1, WBSID = 1, WorkspaceID = workspaceID, Title = "Test Title", Description = "Test Description" };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(boe.Id)).Returns(new Collection<BoeApproverResponseDTO> { });
            commonDataMapper.Setup(x => x.getBOEStateName(BOEState.Draft)).Returns("Draft");


            UserData user = new UserData{Email=""};
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            UserDTO deletedByUser = new UserDTO { DisplayName = "Meanie, Deleted", UserID = 9, EmailAddress = "deletedmeaine@lmco.com" };

            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).
               Returns(new Collection<PermissionsDTO>{ 
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 13, Role = Role.WorkspaceUser, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 10, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 11, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }, // duplicate userid
                            new PermissionsDTO { BOEId=boe.Id, ETIUserId = 12, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }
                });

            UserDTO[] adminsArray = new UserDTO[] {
                new UserDTO { UserID=13, EmailAddress = "admin13@lmco.com" },
                new UserDTO { UserID = 10, EmailAddress = "admin10@lmco.com" },
                new UserDTO { UserID = 11, EmailAddress = "admin11@lmco.com" },
                new UserDTO { UserID = 12, EmailAddress = "admin12@lmco.com" }
            };

            userLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(adminsArray);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object, boeCommentLoader.Object, 
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);
            Collection<int> approverCollection = new Collection<int>{1,2};
            sut.SendBOEDeleted(boe, deletedByUser, approverCollection, wbs, this.Clin1, factory.Object.CreateFullWorkspace(ws1), this.boeStatesDictionary);
            sut.PrivateSendBOEDeleted(boeObject, deletedByUser, user, approverCollection, wbs, this.Clin1, factory.Object.CreateFullWorkspace(ws1), this.boeStatesDictionary);

            permLoader.Verify(x => x.GetBOEPermissions(new List<int>(){boe.Id}), Times.Never());
            permLoader.Verify(x => x.GetWorkspacePermissions(ws1.Id), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendWorkspaceRestoredEmail()
        {
            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1", ContractStartDate = Convert.ToDateTime("04/01/2011"), ContractEndDate = Convert.ToDateTime("12/01/2011"), WorkspaceState = WorkspaceState.Working };
            FullWorkspace workspace = new FullWorkspace(ws1);

            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).Returns(new Collection<PermissionsDTO>
            {
                new PermissionsDTO { BOEId=1, ETIUserId = 7, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id },
                            new PermissionsDTO { BOEId=2, ETIUserId = 7, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }, // duplicate userid
                            new PermissionsDTO { BOEId=2, ETIUserId = 8, Role = Role.WorkspaceAdmin, WorkspaceId = ws1.Id }
            });


            permLoader.Setup(x => x.GetWorkspacePermissions(ws1.Id)).Returns(new Collection<PermissionsDTO>
            {
               
                            new PermissionsDTO { BOEId=null, ETIUserId = 10, Role = Role.WorkspaceReviewer, WorkspaceId = ws1.Id }
            });

            UserDTO admin7 = new UserDTO { UserID = 7, EmailAddress = "admin1@lmco.com" };
            UserDTO admin8 = new UserDTO { UserID = 8, EmailAddress = "admin2@lmco.com" };
            UserDTO approver9 = new UserDTO { UserID = 9, EmailAddress = "approver9@lmco.com" };
            UserDTO admin10 = new UserDTO { UserID = 10, EmailAddress = "adminAndAuthor10@lmco.com" };
            UserDTO approver11 = new UserDTO { UserID = 11, EmailAddress = "approver11@lmco.com" };

            userLoader.Setup(x => x.GetUserByID(admin7.UserID)).Returns(admin7);
            userLoader.Setup(x => x.GetUserByID(admin8.UserID)).Returns(admin8);
            userLoader.Setup(x => x.GetUserByID(approver9.UserID)).Returns(approver9);
            userLoader.Setup(x => x.GetUserByID(admin10.UserID)).Returns(admin10);
            userLoader.Setup(x => x.GetUserByID(approver11.UserID)).Returns(approver11);

            Collection<BoeApproverResponseDTO> boeApprover2 = new Collection<BoeApproverResponseDTO> { new BoeApproverResponseDTO { ETIUserID = approver9.UserID, BoeID = 2 }, new BoeApproverResponseDTO { ETIUserID = approver11.UserID, BoeID = 2 } };
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(2)).Returns(boeApprover2);
            retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(1)).Returns(new Collection<BoeApproverResponseDTO> { });

            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = approver9.UserID }, new PermissionsDTO { ETIUserId = approver11.UserID } };
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { 1, 2 })).Returns(perm2);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){2})).Returns(perm2);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){1})).Returns(new Collection<PermissionsDTO> { });

            Collection<BoeDTO> boes = new Collection<BoeDTO>{
                new BoeDTO{Id=1,WorkspaceID=ws1.Id}, // no author, no approvers
                new BoeDTO{Id=2, AuthorIDs = new Collection<int>{admin10.UserID}, WorkspaceID=ws1.Id}            };

            retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe(boes[0]), new FullBoe(boes[1]) });

            EmailModelDomain email = new EmailModelDomain
            {
                Subject = @"genBOE: {0} was restored to {1}",
                Body = @"All contents of the Workspace {0} was restored to {1}.  <BR/>  The Workspace status is set to {2}  <BR/>  The status of all BOEs has been set to Draft.  Previously approved BOEs will need to be reapproved.  <BR/>  {3}",
                EmailType = EmailTypes.WorkspaceRestored
            };

            Collection<EmailModelDomain> emails =
                new Collection<EmailModelDomain>(new EmailModelDomain[] { email });

            commonDataMapper.Setup(x => x.GetEmails()).Returns(emails);
            commonDataMapper.Setup(x => x.getWorkspaceStateName(WorkspaceState.Working)).Returns("Working");
            UserData user = new UserData() { Email = "" };
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            DateTime restoredTime = DateTime.Now;
            sut.SendWorkspaceRestored(workspace, restoredTime);
            sut.PrivateSendWorkspaceRestored(workspace, restoredTime, user);

            
            permLoader.Verify(x => x.GetWorkspacePermissions(ws1.Id), Times.AtLeastOnce());

            permLoader.Verify(x => x.GetWorkspacePermissions(ws1.Id), Times.AtLeastOnce());
            commonDataMapper.Verify(x => x.getWorkspaceStateName(WorkspaceState.Working), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SendBOEAuthorReviewerCommentedEmail()
        {
            // OUTPUT : we should see 1 email sent to 1 address, 1 author.  
            // there are 3 authors total defined but 1 is the same author and this tests the 'distinct' call

            
            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };
            ClinDTO clin = new ClinDTO { Id = 1, ClinNumber = "1", ClinTitle = "Firstclin" };

            WbsDTO wbs = new WbsDTO {Id = 1, WbsNumber = "2", WbsTitle = "SecondWbs" };

            BoeDTO boe = new BoeDTO
            {
                State = BOEState.Draft,
                Id = 1,
                AuthorIDs = new Collection<int>{5},
                WorkspaceID = ws1.Id,
                CLINID = clin.Id,
                WBSID = wbs.Id,
                Title = "Test Title",
                Description = "hola test"
            };
            FullBoe boeObject = new FullBoe(boe);
            this.retriever.Setup(x => x.GetWorkspaceById(boeObject.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
            factory.Setup(x => x.CreateFullWorkspace(ws1)).Returns(new FullWorkspace(ws1));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            UserDTO[] reviewersArray = new UserDTO[] {
                new UserDTO { UserID = 3, DisplayName = "John Q. Reviewer", EmailAddress="johnqreviewer@lmco.com"}
            };

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"}
            };

            userLoader.Setup(x => x.GetUserByID(3)).Returns(reviewersArray[0]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);

            Collection<PermissionsDTO> boeAuthorPermissions = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[0].UserID,Role=Role.Author,BOEId=boe.Id} 
            };

            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions);

            EmailModelDomain email = new EmailModelDomain
            {
                Subject = @"genBOE: {0} submitted comments for BOE",
                Body = @"{0} submitted or updated comments for the following BOE: <BR/> Workspace/Proposal: {1} <BR/> WBS: {2} {3} <BR/> CLIN: {4} {5} <BR/> BOE Status: {6} <BR/> BOE Description: {7} <BR/><BR/>{8}",
                EmailType = EmailTypes.BOEAuthorReviewerCommented
            };

            commonDataMapper.Setup(x => x.GetEmails()).Returns(new Collection<EmailModelDomain> { email });
            commonDataMapper.Setup(x => x.getBOEStateName(boe.State)).Returns("Draft");

            UserData user = new UserData() { Email = "" };
            secInfo.Setup(x => x.ActiveUserData).Returns(user);

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            sut.SendBOEAuthorReviewerCommented(boeObject, reviewersArray[0].UserID);
            sut.PrivateSendBOEAuthorReviewerCommented(boeObject, reviewersArray[0].UserID, user);

             userLoader.Verify(x => x.GetUserByID(3), Times.AtLeastOnce());
             userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), TestMethod]
        public void SendBOEAuthorsApproversInUseResourceUpdated()
        {
            // OUTPUT : we should see 1 email sent to 1 address, 1 author.  
            // there are 3 authors total defined but 1 is the same author and this tests the 'distinct' call

            var secInfo = new Mock<ISecurityInformation>();
            
            var userLoader = new Mock<IUserDTODataLoader>();
            var permLoader = new Mock<IPermissionsDTODataLoader>();
            var boeCommentLoader = new Mock<IBOECommentDTODataLoader>();
            
            var dataFetcher = new Mock<IDataFetchingScheduler>();
            var boeLoader = new Mock<IBoeDTODataLoader>();
            var workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            commonDataMapper.Setup(x => x.GetEmails()).Returns(realMapper.GetEmails());
            workspaceLoader.Setup(x => x.GetWorkspaceEmailOverrides(1)).Returns(new Collection<WorkspaceEmailOverrideDTO>());
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

            WorkspaceDTO ws1 = new WorkspaceDTO { Id = 1, WorkspaceName = "Test Workspace ONE", Shortname = "testws1" };
            FullWorkspace workspace = new FullWorkspace(ws1);
            ClinDTO clin = new ClinDTO { Id = 1, ClinNumber = "1", ClinTitle = "Firstclin" };

            WbsDTO wbs = new WbsDTO {Id = 1, WbsNumber = "2", WbsTitle = "SecondWbs" };

            UserDTO[] authorsArray = new UserDTO[] {
                new UserDTO { UserID = 5, DisplayName = "John Q. Author", EmailAddress="johnqauthor@lmco.com"}
            };

            BoeDTO boe = new BoeDTO
            {
                State = BOEState.Draft,
                Id = 1,
                AuthorIDs = new Collection<int> { authorsArray[0].UserID},
                WorkspaceID = ws1.Id,
                CLINID = clin.Id,
                WBSID = wbs.Id,
                Title = "Test Title",
                Description = "hola test"
            };

            BoeDTO boe2 = new BoeDTO
            {
                State = BOEState.Approved,
                Id = 2,
                AuthorIDs = new Collection<int> { authorsArray[0].UserID },
                WorkspaceID = ws1.Id,
                CLINID = null,
                WBSID = wbs.Id,
                Title = "Test Title",
                Description = "hola test"
            };
            Collection<BOEStateModelView> boeStates = new Collection<BOEStateModelView>();
            BOEStateModelView draftState = new BOEStateModelView()
            { 
                BOEStateID = (int)boe.State, 
                BOEState = "Draft"
            };
            BOEStateModelView approvedState = new BOEStateModelView()
            { 
                BOEStateID = (int)boe2.State, 
                BOEState = "Approved"
            };
            boeStates.Add(draftState);
            boeStates.Add(approvedState);


            this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(ws1);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws1.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe(boe), new FullBoe(boe2) });
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(new FullBoe(boe2));
            factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(new FullBoe(boe));
            this.retriever.Setup(x => x.GetFullWorkspaceById(ws1.Id)).Returns(new FullWorkspace(ws1));

            UserDTO[] approversArray = new UserDTO[] {
                new UserDTO { UserID = 3, DisplayName = "John Q. Reviewer", EmailAddress="johnqreviewer@lmco.com"}
            };

            userLoader.Setup(x => x.GetUserByID(3)).Returns(approversArray[0]);
            userLoader.Setup(x => x.GetUserByID(5)).Returns(authorsArray[0]);


            UserData Activeuser = new UserData() { Email = "" };
            secInfo.Setup(x => x.ActiveUserData).Returns(Activeuser);
            UserDTO user = new UserDTO { UserID = 1, DisplayName = "Mocky, Mock E" };
            Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { BOEId = boe.Id, ETIUserId = approversArray[0].UserID } };
            Collection<PermissionsDTO> perm2 = new Collection<PermissionsDTO> { new PermissionsDTO { BOEId = boe2.Id, ETIUserId = approversArray[0].UserID } };
            
            List<PermissionsDTO> bothPerms = new List<PermissionsDTO>();
            bothPerms.AddRange(perm1);
            bothPerms.AddRange(perm2);
            
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boe.Id, boe2.Id })).Returns(bothPerms.ToCollection());
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boe.Id })).Returns(perm1);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe2.Id})).Returns(perm2);
            

            Collection<PermissionsDTO> boeAuthorPermissions1 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[0].UserID,Role=Role.Author,BOEId=1} 
            };

            Collection<PermissionsDTO> boeAuthorPermissions2 = new Collection<PermissionsDTO> { 
             new PermissionsDTO{ETIUserId=authorsArray[0].UserID,Role=Role.Author,BOEId=2} 
            };
            List<PermissionsDTO> bothAuthorPerms = new List<PermissionsDTO>();
            bothAuthorPerms.AddRange(boeAuthorPermissions1);
            bothAuthorPerms.AddRange(boeAuthorPermissions2);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boe.Id, boe2.Id })).Returns(bothAuthorPerms.ToCollection());
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe.Id})).Returns(boeAuthorPermissions1);
            permLoader.Setup(x => x.GetBOEPermissions(new List<int>(){boe2.Id})).Returns(boeAuthorPermissions2);

            int resourceID = 1;
            ResourceDTO resource = new ResourceDTO { Id = resourceID, ResourceDesc = "TestInUse"};

            ICollection<int> boeIds = new Collection<int> { boe.Id, boe2.Id };
            boeLoader.Setup(x => x.GetIdsByResourceId(resourceID)).Returns(boeIds);
            factory.Setup(x => x.CreateFullBoes(boeIds)).Returns(new Collection<FullBoe>() { new FullBoe(boe), new FullBoe(boe2) });

            BoeEmailer sut = new BoeEmailer(commonDataMapper.Object, secInfo.Object,
                                        userLoader.Object, permLoader.Object,
                                        boeCommentLoader.Object,  
                                        dataFetcher.Object, factory.Object, boeLoader.Object, workspaceLoader.Object);

            Collection<FieldChanged> fieldsChanged = new Collection<FieldChanged>();
            fieldsChanged.Add(new FieldChanged { Field = "Description", OldValue = "1EC1-test", NewValue = "1DF2-test" });
            fieldsChanged.Add(new FieldChanged { Field = "Segment Region", OldValue = "1E", NewValue = "1D" });
            fieldsChanged.Add(new FieldChanged { Field = "Labor Type", OldValue = "C1", NewValue = "F2" });
            fieldsChanged.Add(new FieldChanged { Field = "Segment", OldValue = "1E", NewValue = "1D" });

            sut.SendBOEAuthorsApproversInUseResourceUpdated(resource, fieldsChanged, user, workspace);
            sut.PrivateSendBOEAuthorsApproversInUseResourceUpdated(resource, fieldsChanged, user, workspace, Activeuser);

            userLoader.Verify(x => x.GetUserByID(5), Times.AtLeastOnce());
        }
    }
}
