// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;

    public class BOECommentsControllerLogic
    {
        private IBOECommentDTODataLoader boeCommentDTOLoader;
        private IUserDTODataLoader userDTOLoader;
        private ISecurityInformation securityInformation;
        private IBoeEmailer emailer;
        private IBoeApproverResponseDTODataLoader boeApproverResponseLoader;
        private IBoeMediator boeMediator;
        private IBOEStateMachine boeStateMachine;
        private IPermissionsDTODataLoader permissionsDTOLoader;

        public BOECommentsControllerLogic(
            IBOECommentDTODataLoader boeCommentDTOLoader,
            IUserDTODataLoader userDTOLoader,
            ISecurityInformation securityInformation,
            IBoeEmailer inEmailer,
            IBoeApproverResponseDTODataLoader inBoeApproverResponseLoader,
            IBoeMediator inBoeMediator,
            IBOEStateMachine inBoeStateMachine,
            IPermissionsDTODataLoader permissionsDTOLoader)
        {
            this.boeCommentDTOLoader = boeCommentDTOLoader;
            this.userDTOLoader = userDTOLoader;
            this.securityInformation = securityInformation;
            this.emailer = inEmailer;
            this.boeApproverResponseLoader = inBoeApproverResponseLoader;
            this.boeMediator = inBoeMediator;
            this.boeStateMachine = inBoeStateMachine;
            this.permissionsDTOLoader = permissionsDTOLoader;
        }

        public Collection<BOEComment> GetCommentsByBOEId(int boeID)
        {
            Collection<BOEComment> toReturn = new Collection<BOEComment>();

            // Get all comments and responses for the current BOE
            ICollection<BOECommentDTO> allComments = this.boeCommentDTOLoader.GetByBoeId(boeID);

            // Get all comments that do not reference any reviewer comments. This
            // will be the set of reviewer comments.
            var reviewerComments = from reviewerComment in allComments
                                   where reviewerComment.BOEResponseToCommentID == null
                                   select reviewerComment;

            // Loop over all reviewer comments and match them with responses, if applicable
            foreach (BOECommentDTO reviewerComment in reviewerComments)
            {
                // The response objects
                BOECommentDTO responderComment;
                UserDTO responder = null;

                // Get the user who entered the reviewer comment
                UserDTO reviewer = this.userDTOLoader.GetUserByID(reviewerComment.BOECommentETIUserID);

                try
                {
                    if (this.securityInformation.IsSubcontractorUser(reviewer.NTID, reviewer.IsSubcontractor))  // for display purposes, append (Sub) to all Subcontractor Names
                    {
                        reviewer.DisplayName += CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX;
                    }
                }
                catch (ArgumentNullException)
                {
                    // because this is only a label, do not treat as fatal
                }

                // Get the response comment for this reviewer comment
                responderComment = (from response in allComments
                                    where response.BOEResponseToCommentID == reviewerComment.Id
                                    select response).LastOrDefault();

                // If a response was found, let's get the user who entered the response
                if (responderComment != null)
                {
                    responder = this.userDTOLoader.GetUserByID(responderComment.BOECommentETIUserID);

                    try
                    {
                        if (this.securityInformation.IsSubcontractorUser(responder.NTID, responder.IsSubcontractor))    // for display purposes, append (Sub) to all Subcontractor Names
                        {
                            responder.DisplayName += CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX;
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        // because this is only a label, do not treat as fatal
                    }
                }

                // Add a new model view with all of the retrieved data for this reviewer comment
                toReturn.Add(new BOEComment(reviewerComment, reviewer, responderComment, responder));
            }

            return toReturn;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<int, ICollection<BOEComment>> GetAllCommentsInWorkspace(IReadOnlyCollection<FullBoe> boes)
        {
            Dictionary<int, ICollection<BOEComment>> toReturn = new Dictionary<int, ICollection<BOEComment>>();

            if (boes is null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            if (boes != null)
            {
                List<int> boeIds = new List<int>();
                foreach (FullBoe boe in boes)
                {
                    boeIds.Add(boe.Id);
                }
                Collection<PermissionsDTO> permissions = permissionsDTOLoader.GetBOEPermissions(boeIds);
                foreach (FullBoe boe in boes)
                {
                    ICollection<UserDTO> boeAuthors = userDTOLoader.GetByIds(boe.AuthorIDs);
                    ICollection<BOEComment> boeComments = GetCommentsByBOEId(boe.Id);
                    PermissionsDTO boePermissions = permissions.Where(x => x.BOEId == boe.Id).FirstOrDefault();

                    foreach (BOEComment comment in boeComments)
                    {
                        comment.BOETitle = boe.Title;
                        comment.ClinString = boe.Clin.ClinString == null ? null : boe.Clin.ClinString;
                        comment.WbsString = boe.Wbs.WbsString == null ? null : boe.Wbs.WbsString;
                        comment.BOEAuthors = string.Join("; ", boeAuthors.Select(x => x.DisplayName));
                        comment.CommenterRole = boePermissions.Role.ToString();
                        if (DateTime.Compare(comment.AuthorResponseUpdateDT.GetValueOrDefault(), DateTime.MinValue) == 0)
                        {
                            comment.AuthorResponseUpdateDT = null;
                        }

                    }

                    toReturn.Add(boe.Id, boeComments);
                } 
            }

            return toReturn;
        }

        public void SaveBOEComments(FullWorkspace workspace, FullBoe fullBOE, BOECommentsModelView boeComments)
        {
            if (workspace == null) { throw new ArgumentNullException(nameof(workspace)); }
            if (fullBOE == null) { throw new ArgumentNullException(nameof(fullBOE)); }

            if (boeComments != null)
            {
                int currentUserID = workspace.CurrentActiveUser.UserID;
                Collection<BOECommentDTO> boeCommentsToSave = new Collection<BOECommentDTO>();
                
                bool SendBOEApproversAuthorApproverRejectedEmail = false;
                bool SendBOEApproversAuthorApproverApprovedEmail = false;
                bool SendWorkspaceAdminEmailAllBOEsApprovedEmail = false;
                bool SendBOEAuthorRespondedToCommentEmail = false;

                bool SendBOEAuthorReviewerCommentedEmailed = this.ValidateAndPreProcessCommentsForSave(fullBOE, boeComments, currentUserID, ref boeCommentsToSave);
                
                BoeApproverResponseDTO approverResponse = null;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    // If there was an approval response passed back with the save, then we'll save the BOE with that response.
                    if (boeComments.Approver != null && fullBOE != null)
                    {
                        // Get the approval passed back by the user
                        BOEApprover approverResponseModelView = boeComments.Approver;

                        // If rejecting, a comment is required
                        if(approverResponseModelView.ApproverResponse == ApproverReponseType.Rejected && !boeCommentsToSave.Any())
                        {
                            throw new GenValidationException("A comment is required when rejecting a BOE.");
                        }

                        // Create an Approval DTO from the ModelView
                        approverResponse = new BoeApproverResponseDTO
                        {
                            ApproverResponded = approverResponseModelView.ApproverResponded,
                            ApproverResponse = approverResponseModelView.ApproverResponse,
                            Id = approverResponseModelView.BOEApprovalID,
                            BoeID = approverResponseModelView.BoeID,
                            ETIUserID = approverResponseModelView.ETIUserID,
                            CurrentUserETIUserID = currentUserID,
                            Updateable = UpdateType.Upsert,
                            UpdateDate = approverResponseModelView.UpdateDate
                        };

                        // Set the approval to Upsert to tell the back end to update
                        Collection<BoeApproverResponseDTO> boeApprovers = new Collection<BoeApproverResponseDTO> { approverResponse };

                        // Save the response with the BOE
                        this.boeApproverResponseLoader.Save(boeApprovers);

                        // Perform state transitions
                        // If the BOE was rejected, set it back to draft
                        if (approverResponse.ApproverResponse == ApproverReponseType.Rejected)
                        {
                            BOEState oldBOEState = fullBOE.State;
                            BOEState newBOEState = BOEState.Draft;  // BOE becomes editable following rejection

                            // Validate the Awaiting Approval to Draft state transition
                            string validationMessage;
                            if (!this.boeStateMachine.PerformStateTransitionValidation(fullBOE, workspace, oldBOEState, newBOEState, out validationMessage))
                            {
                                // not valid ... communicate to user
                                throw new ValidationException(validationMessage);
                            }

                            // If the transition is valid, set the BOE to Draft and save it
                            fullBOE.Updateable = UpdateType.Upsert;
                            fullBOE.State = newBOEState;
                            this.boeMediator.MediatedSave(workspace, fullBOE);

                            SendBOEApproversAuthorApproverRejectedEmail = true;

                            // Perform common state transition actions
                            this.boeStateMachine.PerformStateTransitionAction(fullBOE, workspace, oldBOEState, fullBOE.State);
                        }
                        // If the BOE was accepted by all approvers, set it to Approved
                        else if (approverResponse.ApproverResponse == ApproverReponseType.Approved)
                        {
                            IReadOnlyCollection<BoeApproverResponseDTO> boeApprovedApprovers = fullBOE.ApproverResponses;
                            if (boeApprovedApprovers.Count(a => a.ApproverResponse == ApproverReponseType.Approved) == boeApprovedApprovers.Count)
                            {
                                fullBOE.Updateable = UpdateType.Upsert;
                                fullBOE.State = BOEState.Approved;

                                this.boeMediator.MediatedSave(workspace, fullBOE);
                            }

                            SendBOEApproversAuthorApproverApprovedEmail = true;
                            SendWorkspaceAdminEmailAllBOEsApprovedEmail = true;
                        }
                    }

                    // If there were comments passed in, we'll save those as well
                    if (boeCommentsToSave.Any())
                    {
                        // Call the save
                        this.boeCommentDTOLoader.Save(boeCommentsToSave);

                        SendBOEAuthorRespondedToCommentEmail = true;
                    }

                    scope.Complete();
                }

                if (SendBOEApproversAuthorApproverRejectedEmail) {
                    this.emailer.SendBOEApproversAuthorApproverRejected(approverResponse, fullBOE); }
                if (SendBOEApproversAuthorApproverApprovedEmail) {
                    this.emailer.SendBOEApproversAuthorApproverApproved(approverResponse, fullBOE); }
                if (SendWorkspaceAdminEmailAllBOEsApprovedEmail) {
                    this.emailer.SendWorkspaceAdminEmailAllBOEsApproved(fullBOE); }
                if (SendBOEAuthorRespondedToCommentEmail) {
                    this.emailer.SendBOEAuthorRespondedToComment(boeCommentsToSave); }
                if (SendBOEAuthorReviewerCommentedEmailed)
                {
                    int boeCommenter = boeCommentsToSave.First().BOECommentETIUserID;
                    this.emailer.SendBOEAuthorReviewerCommented(fullBOE, boeCommenter);
                }
            }
        }

        /// <summary>
        /// Validates and prepares comments for a save
        /// </summary>
        /// <param name="boeDto">Boe</param>
        /// <param name="boeComments">Comments</param>
        /// <param name="currentUserID">Current User Id</param>
        /// <param name="boeCommentsToSave">Comments (if any) that need to be saved will be placed into this collection</param>
        /// <returns>An indication whether an email needs to be sent (SendBOEAuthorReviewerCommentedEmailed)</returns>
        internal bool ValidateAndPreProcessCommentsForSave(BoeDTO boeDto, BOECommentsModelView boeComments, int currentUserID, ref Collection<BOECommentDTO> boeCommentsToSave)
        {
            if (boeCommentsToSave == null) { boeCommentsToSave = new Collection<BOECommentDTO>(); }

            bool SendBOEAuthorReviewerCommentedEmailed = false;

            if (boeComments.Comments.Any())
            {
                int insertIndex = -1;
                foreach (BOEComment boeComment in boeComments.Comments)
                {
                    // Create the DTOs to save
                    BOECommentDTO commentDTO = null;

                    // Create a DTO for the author response, if it exists
                    if (!string.IsNullOrEmpty(boeComment.AuthorResponse))
                    {
                        // If the ID passed in is negative, this is a new comment.
                        // So, we need to set the ID to a unique negative count.
                        if (boeComment.AuthorResponseID < 0)
                        {
                            commentDTO = new BOECommentDTO();
                            commentDTO.Id = insertIndex;
                            commentDTO.UpdateDate = DateTime.Now;
                            insertIndex--;
                        }
                        else
                        {
                            // get the comment from the existing comments.
                            commentDTO = this.boeCommentDTOLoader.GetByIds(new List<int>() { boeComment.AuthorResponseID }).First();
                            DataRelationshipVerifier.VerifyDataRelation(commentDTO, boeDto.Id);

                            // set the UpdateDate for optimistic locking purposes
                            commentDTO.UpdateDate = new DateTime(boeComment.AuthorResponseUpdateDTLong);
                        }

                        commentDTO.FieldID = (int)FieldType.AuthorResponse;
                        commentDTO.BOEComment = boeComment.AuthorResponse;
                        commentDTO.BOEResponseToCommentID = boeComment.ReviewerCommentID;
                    }
                    // Create a DTO for the reviewer comment
                    else if (!string.IsNullOrEmpty(boeComment.ReviewerComment))
                    {
                        // If the ID passed in is negative, this is a new comment.
                        // So, we need to set the ID to a unique negative count.
                        if (boeComment.ReviewerCommentID < 0)
                        {
                            commentDTO = new BOECommentDTO();
                            commentDTO.Id = insertIndex--;
                            commentDTO.UpdateDate = DateTime.Now;
                        }
                        else
                        {
                            // get the comment from the existing comments.
                            commentDTO = this.boeCommentDTOLoader.GetByIds(new List<int>() { boeComment.ReviewerCommentID }).First();
                            DataRelationshipVerifier.VerifyDataRelation(commentDTO, boeDto.Id);

                            commentDTO.Id = boeComment.ReviewerCommentID;

                            // set the UpdateDate for optimistic locking purposes
                            commentDTO.UpdateDate = new DateTime(boeComment.ReviewerCommentUpdateDTLong);
                        }

                        commentDTO.FieldID = (int)FieldType.Comment;
                        commentDTO.BOEComment = boeComment.ReviewerComment;
                        SendBOEAuthorReviewerCommentedEmailed = true;
                    }

                    if (commentDTO != null)
                    {
                        commentDTO.BOECommentETIUserID = currentUserID;
                        commentDTO.BoeID = boeDto.Id;
                        commentDTO.Updateable = UpdateType.Upsert;
                        boeCommentsToSave.Add(commentDTO);
                    }
                }
            }

            return SendBOEAuthorReviewerCommentedEmailed;
        }
    }
}