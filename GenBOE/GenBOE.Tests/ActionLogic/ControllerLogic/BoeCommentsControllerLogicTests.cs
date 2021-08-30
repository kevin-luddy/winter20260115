// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Action Logic tests for BoeCommentControllerLogic class.
    /// </summary>
    [TestClass]
    public class BoeCommentsControllerLogicTests
    {
        private BOECommentsControllerLogic _commentsControllerLogic;
        private Collection<BOECommentDTO> _commentDtos = new Collection<BOECommentDTO>();

        private Mock<IBOECommentDTODataLoader> _boeCommentDTOLoader;
        private Mock<IUserDTODataLoader> _userDTOLoader;
        private Mock<ISecurityInformation> _securityInformation;
        private Mock<IBoeEmailer> _emailer;
        private Mock<IBoeApproverResponseDTODataLoader> _boeApproverResponseLoader;
        private Mock<IBoeMediator> _boeMediator;
        private Mock<IBOEStateMachine> _boeStateMachine;
        private Mock<IRetriever> _retriever;
        private Mock<IFullObjectFactory> _factory;
        private Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader;
        private Mock<ICommonDataMapper> _commonDataMapper;

        /// <summary>
        /// Initializes data before each test run for this class.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _boeCommentDTOLoader = new Mock<IBOECommentDTODataLoader>();
            _userDTOLoader = new Mock<IUserDTODataLoader>();
            _securityInformation = new Mock<ISecurityInformation>();
            _emailer = new Mock<IBoeEmailer>();
            _boeApproverResponseLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            _boeMediator = new Mock<IBoeMediator>();
            _boeStateMachine = new Mock<IBOEStateMachine>();
            _retriever = new Mock<IRetriever>();
            _factory = new Mock<IFullObjectFactory>();
            _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
            _commonDataMapper = new Mock<ICommonDataMapper>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), _factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
            _commentsControllerLogic = new BOECommentsControllerLogic(_boeCommentDTOLoader.Object, _userDTOLoader.Object, _securityInformation.Object,
                _emailer.Object, _boeApproverResponseLoader.Object, _boeMediator.Object, _boeStateMachine.Object);

            BOECommentDTO comment = new BOECommentDTO
            {
                BOEComment = "Test comment one",
                BOECommentETIUserID = 1,
                BoeID = 1,
                // Comments have a null response Id
                BOEResponseToCommentID = null,
                Id = 1,
                UpdateDate = DateTime.Now
            };

            // A response to the above comment.
            BOECommentDTO commentResponse = new BOECommentDTO
            {
                BOEComment = "Test comment one Response",
                BOECommentETIUserID = 2,
                BoeID = 1,
                BOEResponseToCommentID = 1,
                Id = 2,
                UpdateDate = DateTime.Now
            };
            _commentDtos.Add(comment);
            _commentDtos.Add(commentResponse);
            
        }

        /// <summary>
        /// Get comments by BOE Id test.
        /// </summary>
        [TestMethod]
        public void GetCommentsByBOEIdTest()
        {
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID="imauser", UserID=1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            _boeCommentDTOLoader.Setup(x => x.GetByBoeId(1)).Returns(_commentDtos);
            _userDTOLoader.Setup(x => x.GetUserByID(reviewerUser.UserID)).Returns(reviewerUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(reviewerUser.NTID, reviewerUser.IsSubcontractor)).Returns(false);
            _userDTOLoader.Setup(x => x.GetUserByID(authorUser.UserID)).Returns(authorUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(authorUser.NTID, authorUser.IsSubcontractor)).Returns(false);

            // Invoke
            ICollection<BOEComment> boeCommentViewModels = _commentsControllerLogic.GetCommentsByBOEId(1);
            Assert.IsTrue(boeCommentViewModels.Count == 1);


            // Assert reviewerUser & authorUser are populated in the model.
            BOEComment commentModel = boeCommentViewModels.FirstOrDefault(x => x.AuthorNtId == authorUser.NTID);
            Assert.IsNotNull(commentModel);
            BOECommentDTO authorComment = _commentDtos.First(x => x.BOECommentETIUserID == authorUser.UserID);
            BOECommentDTO reviewerComment = _commentDtos.First(x => x.BOECommentETIUserID == reviewerUser.UserID);

            // Assert Reviewer's comment (the original comment)
            Assert.AreEqual(commentModel.ReviewerID, 1);
            Assert.AreEqual(commentModel.ReviewerName, "I. M. Auser");
            Assert.AreEqual(commentModel.ReviewerNtId, "imauser");
            Assert.AreEqual(commentModel.ReviewerComment, reviewerComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);

            // Assert Author's comment (the respone to the original comment)
            Assert.AreEqual(commentModel.AuthorID, 2);
            Assert.AreEqual(commentModel.AuthorName, "I. M. Aresponder");
            Assert.AreEqual(commentModel.AuthorNtId, "imaresponder");
            Assert.AreEqual(commentModel.AuthorResponse, authorComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);
        }

        /// <summary>
        /// Get comments by BOE Id test when authors are subcontractors.
        /// </summary>
        [TestMethod]
        public void GetCommentsByBOEIdTest_SubUsers()
        {
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            _boeCommentDTOLoader.Setup(x => x.GetByBoeId(1)).Returns(_commentDtos);
            _userDTOLoader.Setup(x => x.GetUserByID(reviewerUser.UserID)).Returns(reviewerUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(reviewerUser.NTID, reviewerUser.IsSubcontractor)).Returns(true);
            _userDTOLoader.Setup(x => x.GetUserByID(authorUser.UserID)).Returns(authorUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(authorUser.NTID, authorUser.IsSubcontractor)).Returns(true);

            // Invoke
            ICollection<BOEComment> boeCommentViewModels = _commentsControllerLogic.GetCommentsByBOEId(1);
            Assert.IsTrue(boeCommentViewModels.Count == 1);

            // Assert reviewerUser & authorUser are populated in the model.
            BOEComment commentModel = boeCommentViewModels.FirstOrDefault(x => x.AuthorNtId == authorUser.NTID);
            Assert.IsNotNull(commentModel);
            BOECommentDTO authorComment = _commentDtos.First(x => x.BOECommentETIUserID == authorUser.UserID);
            BOECommentDTO reviewerComment = _commentDtos.First(x => x.BOECommentETIUserID == reviewerUser.UserID);

            // Assert Reviewer's comment (the original comment)
            Assert.AreEqual(commentModel.ReviewerID, 1);
            Assert.AreEqual(commentModel.ReviewerName, "I. M. Auser" + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX);
            Assert.AreEqual(commentModel.ReviewerNtId, "imauser");
            Assert.AreEqual(commentModel.ReviewerComment, reviewerComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);

            // Assert Author's comment (the respone to the original comment)
            Assert.AreEqual(commentModel.AuthorID, 2);
            Assert.AreEqual(commentModel.AuthorName, "I. M. Aresponder" + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX);
            Assert.AreEqual(commentModel.AuthorNtId, "imaresponder");
            Assert.AreEqual(commentModel.AuthorResponse, authorComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);
        }

        /// <summary>
        /// Get comments by BOE Id when the comment authors cannot be found in Active Directory.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), TestMethod]
        public void GetCommentsByBOEIdTest_UserNotInAD()
        {
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            _boeCommentDTOLoader.Setup(x => x.GetByBoeId(1)).Returns(_commentDtos);
            _userDTOLoader.Setup(x => x.GetUserByID(reviewerUser.UserID)).Returns(reviewerUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(reviewerUser.NTID, reviewerUser.IsSubcontractor)).Throws(new ArgumentNullException()); //happens when user cannot be found in AD
            _userDTOLoader.Setup(x => x.GetUserByID(authorUser.UserID)).Returns(authorUser);
            _securityInformation.Setup(x => x.IsSubcontractorUser(authorUser.NTID, authorUser.IsSubcontractor)).Throws(new ArgumentNullException());

            // Invoke
            ICollection<BOEComment> boeCommentViewModels = _commentsControllerLogic.GetCommentsByBOEId(1);
            Assert.IsTrue(boeCommentViewModels.Count == 1);


            // Assert reviewerUser & authorUser are populated in the model.
            BOEComment commentModel = boeCommentViewModels.FirstOrDefault(x => x.AuthorNtId == authorUser.NTID);
            Assert.IsNotNull(commentModel);
            BOECommentDTO authorComment = _commentDtos.First(x => x.BOECommentETIUserID == authorUser.UserID);
            BOECommentDTO reviewerComment = _commentDtos.First(x => x.BOECommentETIUserID == reviewerUser.UserID);

            // Assert Reviewer's comment (the original comment)
            Assert.AreEqual(commentModel.ReviewerID, 1);
            Assert.AreEqual(commentModel.ReviewerName, "I. M. Auser");
            Assert.AreEqual(commentModel.ReviewerNtId, "imauser");
            Assert.AreEqual(commentModel.ReviewerComment, reviewerComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);

            // Assert Author's comment (the respone to the original comment)
            Assert.AreEqual(commentModel.AuthorID, 2);
            Assert.AreEqual(commentModel.AuthorName, "I. M. Aresponder");
            Assert.AreEqual(commentModel.AuthorNtId, "imaresponder");
            Assert.AreEqual(commentModel.AuthorResponse, authorComment.BOEComment);
            Assert.AreEqual(commentModel.CommentType, BOECommentType.Comment);
        }

        /// <summary>
        /// Saves comments where that were approved.
        /// </summary>
        [TestMethod]
        public void SaveBoeApprovedCommentsTest()
        {
            BoeApproverResponseDTO approverResponseDto = new BoeApproverResponseDTO { ApproverResponded = true, ApproverResponse = ApproverReponseType.Approved, BoeID = 1 };
            FullBoe boe = new FullBoe() { Id = 1, WorkspaceID = 1 };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            BOEComment boeComment = new BOEComment(_commentDtos[0], reviewerUser, _commentDtos[1], authorUser);
            Collection<BOEComment> boeComments = new Collection<BOEComment>();
            boeComments.Add(boeComment);
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Approved,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                Comments = boeComments,
                UpdateDate = DateTime.Now
            };
            // Setup mock calls.
            _retriever.Setup(i => i.GetCurrentActiveUser()).Returns(new UserDTO { UserID = 1 });
            _retriever.Setup(i => i.GetApproverResponseCollectionByBoeId(boe.Id)).Returns(new Collection<BoeApproverResponseDTO> { approverResponseDto });
            _boeApproverResponseLoader.Setup(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>())).Returns(new Dictionary<int, int>());
            _boeMediator.Setup(i => i.MediatedSave(workspace, boe)).Returns(new Dictionary<int, int>());
            _boeCommentDTOLoader.Setup(i => i.GetByIds(new List<int> { authorUser.UserID })).Returns(new Collection<BOECommentDTO>() { _commentDtos[1] });
            _boeCommentDTOLoader.Setup(i => i.Save(new Collection<BOECommentDTO>() { _commentDtos[1] }));

            // Emails should be sent to author and admins. After invoke we can verify these method were called. 
            _emailer.Setup(i => i.SendBOEApproversAuthorApproverApproved(It.IsAny<BoeApproverResponseDTO>(), boe));
            _emailer.Setup(i => i.SendWorkspaceAdminEmailAllBOEsApproved(boe));
            _emailer.Setup(i => i.SendBOEAuthorRespondedToComment(new Collection<BOECommentDTO>() { _commentDtos[1] }));


            // Invoke
            _commentsControllerLogic.SaveBOEComments(workspace, boe, boeCommentsModelView);



            // Verify saves and emails sent.
            _boeMediator.Verify(i => i.MediatedSave(workspace, boe), Times.Exactly(1));
            _boeCommentDTOLoader.Verify(i => i.Save(new Collection<BOECommentDTO>() { _commentDtos[1] }), Times.Exactly(1));
            _boeApproverResponseLoader.Verify(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>()), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEApproversAuthorApproverApproved(It.IsAny<BoeApproverResponseDTO>(), boe), Times.Exactly(1));
            _emailer.Verify(i => i.SendWorkspaceAdminEmailAllBOEsApproved(boe), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEAuthorRespondedToComment(new Collection<BOECommentDTO>() { _commentDtos[1] }), Times.Exactly(1));
        }

        /// <summary>
        /// Saves comments where that were approved with no author comment.
        /// </summary>
        [TestMethod]
        public void SaveBoeApprovedCommentsNoAuthorResponseTest()
        {
            BoeApproverResponseDTO approverResponseDto = new BoeApproverResponseDTO { ApproverResponded = true, ApproverResponse = ApproverReponseType.Approved, BoeID = 1 };
            FullBoe boe = new FullBoe() { Id = 1, WorkspaceID = 1 };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            //UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            BOEComment boeComment = new BOEComment(_commentDtos[0], reviewerUser, null, null);
            Collection<BOEComment> boeComments = new Collection<BOEComment>();
            boeComments.Add(boeComment);
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Approved,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                Comments = boeComments,
                UpdateDate = DateTime.Now
            };
            // Setup mock calls.
            _retriever.Setup(i => i.GetCurrentActiveUser()).Returns(new UserDTO { UserID = 1 });
            _retriever.Setup(i => i.GetApproverResponseCollectionByBoeId(boe.Id)).Returns(new Collection<BoeApproverResponseDTO> { approverResponseDto });
            _boeApproverResponseLoader.Setup(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>())).Returns(new Dictionary<int, int>());
            _boeMediator.Setup(i => i.MediatedSave(workspace, boe)).Returns(new Dictionary<int, int>());
            _boeCommentDTOLoader.Setup(i => i.GetByIds(new List<int> { reviewerUser.UserID })).Returns(new Collection<BOECommentDTO>() { _commentDtos[0] });
            _boeCommentDTOLoader.Setup(i => i.Save(new Collection<BOECommentDTO>() { _commentDtos[0] }));

            // Emails should be sent to author and admins. After invoke we can verify these method were called. 
            _emailer.Setup(i => i.SendBOEApproversAuthorApproverApproved(It.IsAny<BoeApproverResponseDTO>(), boe));
            _emailer.Setup(i => i.SendWorkspaceAdminEmailAllBOEsApproved(boe));
            //_emailer.Setup(i => i.SendBOEAuthorRespondedToComment(new Collection<BOECommentDTO>() { _commentDtos[0] }));
            _emailer.Setup(i => i.SendBOEAuthorReviewerCommented(boe, It.IsAny<int>()));


            // Invoke
            _commentsControllerLogic.SaveBOEComments(workspace, boe, boeCommentsModelView);



            // Verify saves and emails sent.
            _boeMediator.Verify(i => i.MediatedSave(workspace, boe), Times.Exactly(1));
            _boeCommentDTOLoader.Verify(i => i.Save(new Collection<BOECommentDTO>() { _commentDtos[0] }), Times.Exactly(1));
            _boeApproverResponseLoader.Verify(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>()), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEApproversAuthorApproverApproved(It.IsAny<BoeApproverResponseDTO>(), boe), Times.Exactly(1));
            _emailer.Verify(i => i.SendWorkspaceAdminEmailAllBOEsApproved(boe), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEAuthorReviewerCommented(boe, reviewerUser.UserID), Times.Exactly(1));
        }

        /// <summary>
        /// Saves comments where that were approved.
        /// </summary>
        [TestMethod]
        public void SaveBoeRejectedCommentsTest()
        {
            BoeApproverResponseDTO approverResponseDto = new BoeApproverResponseDTO { ApproverResponded = true, ApproverResponse = ApproverReponseType.Approved, BoeID = 1 };
            FullBoe boe = new FullBoe() { Id = 1, WorkspaceID = 1, State = BOEState.AwaitingApproval };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            BOEComment boeComment = new BOEComment(_commentDtos[0], reviewerUser, _commentDtos[1], authorUser);
            Collection<BOEComment> boeComments = new Collection<BOEComment>();
            boeComments.Add(boeComment);
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Rejected,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                Comments = boeComments,
                UpdateDate = DateTime.Now
            };
            // Set the Ids of the comments to -1, -2 to force creation of the comment.
            boeCommentsModelView.Comments[0].AuthorResponseID = -1;

            // Setup mock calls.
            _retriever.Setup(i => i.GetCurrentActiveUser()).Returns(new UserDTO { UserID = 1 });
            _retriever.Setup(i => i.GetApproverResponseCollectionByBoeId(boe.Id)).Returns(new Collection<BoeApproverResponseDTO> { approverResponseDto });
            _boeApproverResponseLoader.Setup(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>())).Returns(new Dictionary<int, int>());
            _boeMediator.Setup(i => i.MediatedSave(workspace, boe)).Returns(new Dictionary<int, int>());
            _boeCommentDTOLoader.Setup(i => i.Save(new Collection<BOECommentDTO>() { _commentDtos[1] }));

            string validationMessage = It.IsAny<string>();
            _boeStateMachine.Setup(i => i.PerformStateTransitionValidation(boe, workspace, BOEState.AwaitingApproval, BOEState.Draft, out validationMessage)).Returns(true);
            _boeStateMachine.Setup(i => i.PerformStateTransitionAction(boe, workspace, BOEState.AwaitingApproval, BOEState.Draft));

            // Emails to be sent for rejection. After invoke we can verify these method were called. 
            _emailer.Setup(i => i.SendBOEApproversAuthorApproverRejected(It.IsAny<BoeApproverResponseDTO>(), boe));

            // Invoke
            _commentsControllerLogic.SaveBOEComments(workspace, boe, boeCommentsModelView);

            // Verify saves and emails sent.
            _boeMediator.Verify(i => i.MediatedSave(workspace, boe), Times.Exactly(1));
            _boeCommentDTOLoader.Verify(i => i.Save(It.IsAny<Collection<BOECommentDTO>>()), Times.Exactly(1));
            _boeApproverResponseLoader.Verify(i => i.Save(It.IsAny<Collection<BoeApproverResponseDTO>>()), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEApproversAuthorApproverRejected(It.IsAny<BoeApproverResponseDTO>(), boe), Times.Exactly(1));
            _emailer.Verify(i => i.SendBOEAuthorRespondedToComment(It.IsAny<Collection<BOECommentDTO>>()), Times.Exactly(1));

            // Verify BOE state set back to draft.
            _boeStateMachine.Verify(i => i.PerformStateTransitionValidation(boe, workspace, BOEState.AwaitingApproval, BOEState.Draft, out validationMessage), Times.Exactly(1));
            _boeStateMachine.Verify(i => i.PerformStateTransitionAction(boe, workspace, BOEState.AwaitingApproval, BOEState.Draft), Times.Exactly(1));

        }

        /// <summary>
        /// Test that Validation Exception gets thrown when a user rejects a BOE without a comment
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void SaveRejectedBOEComments_NoComments()
        {
            BoeApproverResponseDTO approverResponseDto = new BoeApproverResponseDTO { ApproverResponded = true, ApproverResponse = ApproverReponseType.Approved, BoeID = 1 };
            FullBoe boe = new FullBoe() { Id = 1, WorkspaceID = 1, State = BOEState.AwaitingApproval };
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            UserDTO reviewerUser = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
            UserDTO authorUser = new UserDTO { DisplayName = "I. M. Aresponder", NTID = "imaresponder", UserID = 2 };
            BOEComment boeComment = new BOEComment(_commentDtos[0], reviewerUser, _commentDtos[1], authorUser);
            Collection<BOEComment> boeComments = new Collection<BOEComment>();
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Rejected,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                Comments = boeComments,
                UpdateDate = DateTime.Now
            };

            // Setup mock calls.
            _retriever.Setup(i => i.GetCurrentActiveUser()).Returns(new UserDTO { UserID = 1 });

            // Invoke
            _commentsControllerLogic.SaveBOEComments(workspace, boe, boeCommentsModelView);
        }

        /// <summary>
        /// Tests an ArgumentNullException is throw when a null workspace is given for an argument.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveBOECommentsNullWorkspaceTest()
        {
            FullBoe boe = new FullBoe() { Id = 1, WorkspaceID = 1 };
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Approved,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                UpdateDate = DateTime.Now
            };
            _commentsControllerLogic.SaveBOEComments(null, boe, boeCommentsModelView);
        }

        /// <summary>
        /// Tests an ArgumentNullException is throw when a null workspace is given for an argument.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveBOECommentsNullBoeTest()
        {
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            BOECommentsModelView boeCommentsModelView = new BOECommentsModelView
            {
                Approver = new BOEApprover
                {
                    ApproverResponded = true,
                    ApproverResponse = ApproverReponseType.Approved,
                    BOEApprovalID = 1,
                    BoeID = 1,
                    ETIUserID = 3
                },
                UpdateDate = DateTime.Now
            };
            _commentsControllerLogic.SaveBOEComments(workspace, null, boeCommentsModelView);
        }

        /// <summary>
        /// Tests the ValidateAndPreProcessTripDataForSaving method, with blank data
        /// </summary>
        [TestMethod]
        public void ValidateAndPreProcessTripDataForSaving_TestWithBlankData()
        {
            int boeId = 1000;

            BoeDTO boeDto = new BoeDTO() { Id = boeId };
            int currentUserID = 2000;
            Collection<BOECommentDTO> boeCommentsToSave = null;

            BOECommentsModelView boeComments = new BOECommentsModelView() 
            { 
                Comments = new Collection<BOEComment>()
            };

            bool sendEmailResult = _commentsControllerLogic.ValidateAndPreProcessCommentsForSave(boeDto, boeComments, currentUserID, ref boeCommentsToSave);

            Assert.IsFalse(sendEmailResult);
            Assert.IsNotNull(boeCommentsToSave);
            Assert.IsFalse(boeCommentsToSave.Any());
        }

        /// <summary>
        /// Tests the ValidateAndPreProcessTripDataForSaving method
        /// </summary>
        [TestMethod]
        public void ValidateAndPreProcessTripDataForSaving_TestWithData()
        {
            int boeId = 1000;

            BoeDTO boeDto = new BoeDTO() { Id = boeId };
            int currentUserID = 2000;
            Collection<BOECommentDTO> boeCommentsToSave = null;

            int authorResponseId = 3;
            int reviewerResponseId = 8;

            BOECommentsModelView boeComments = new BOECommentsModelView()
            {
                Comments = new Collection<BOEComment>() 
                { 
                    new BOEComment() 
                    {
                        AuthorResponse = "Hello world",
                        AuthorResponseID = -20,
                        ReviewerCommentID = 6
                    },

                    new BOEComment() 
                    {
                        AuthorResponse = "Hello world 2222",
                        AuthorResponseID = authorResponseId,
                        ReviewerCommentID = 7

                    },
                    new BOEComment() 
                    {
                        ReviewerComment = "Hello world 3333",
                        ReviewerCommentID = -20
                    },

                    new BOEComment() 
                    {
                        ReviewerComment = "Hello world 4444",
                        ReviewerCommentID = reviewerResponseId
                    }
                }
            };

            _boeCommentDTOLoader.Setup(x => x.GetByIds(new List<int>() { authorResponseId })).Returns(new List<BOECommentDTO>() { new BOECommentDTO() { BoeID = boeId, Id = authorResponseId } });
            _boeCommentDTOLoader.Setup(x => x.GetByIds(new List<int>() { reviewerResponseId })).Returns(new List<BOECommentDTO>() { new BOECommentDTO() { BoeID = boeId, Id = reviewerResponseId } });

            bool sendEmailResult = _commentsControllerLogic.ValidateAndPreProcessCommentsForSave(boeDto, boeComments, currentUserID, ref boeCommentsToSave);

            Assert.IsTrue(sendEmailResult);
            Assert.IsNotNull(boeCommentsToSave);
            Assert.AreEqual(boeCommentsToSave.Count, 4);

            Assert.AreEqual(boeComments.Comments.ElementAt(0).AuthorResponse, boeCommentsToSave.ElementAt(0).BOEComment);
            Assert.IsTrue(boeCommentsToSave.ElementAt(0).Id < 0);
            Assert.AreEqual((int)FieldType.AuthorResponse, boeCommentsToSave.ElementAt(0).FieldID);
            Assert.AreEqual(boeComments.Comments.ElementAt(0).ReviewerCommentID, boeCommentsToSave.ElementAt(0).BOEResponseToCommentID);

            Assert.AreEqual(boeComments.Comments.ElementAt(1).AuthorResponse, boeCommentsToSave.ElementAt(1).BOEComment);
            Assert.AreEqual(authorResponseId, boeCommentsToSave.ElementAt(1).Id);
            Assert.AreEqual((int)FieldType.AuthorResponse, boeCommentsToSave.ElementAt(1).FieldID);
            Assert.AreEqual(boeComments.Comments.ElementAt(1).ReviewerCommentID, boeCommentsToSave.ElementAt(1).BOEResponseToCommentID);

            Assert.AreEqual(boeComments.Comments.ElementAt(2).ReviewerComment, boeCommentsToSave.ElementAt(2).BOEComment);
            Assert.IsTrue(boeCommentsToSave.ElementAt(2).Id < 0);
            Assert.AreEqual((int)FieldType.Comment, boeCommentsToSave.ElementAt(2).FieldID);

            Assert.AreEqual(boeComments.Comments.ElementAt(3).ReviewerComment, boeCommentsToSave.ElementAt(3).BOEComment);
            Assert.AreEqual(reviewerResponseId, boeCommentsToSave.ElementAt(3).Id);
            Assert.AreEqual((int)FieldType.Comment, boeCommentsToSave.ElementAt(3).FieldID);
        }
    }
}