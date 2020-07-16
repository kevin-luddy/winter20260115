// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Email
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Mail;
    using GenBOE.ActionLogic.Workspace;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    public interface IBoeEmailer
    {
        /// <summary>
        /// Send an email to the BOE authors when a workspace is opened for edit
        /// </summary>
        /// <param name="inWorkspace">The workspace.</param>
        void SendWorkspaceAuthorsEmailOpenedForEdit(FullWorkspace inWorkspace);

        /// <summary>
        /// Send an email to the workspace reviewers that a BOE is ready for their review.
        /// </summary>
        /// <param name="inBOEForReview">The BOE ready for review</param>
        void SendBOESubmittedForReview(FullBoe inBOEForReview);

        /// <summary>
        /// Send an email to the BOE authors that a BOE's state has moved from Approved to Draft(open for edit)
        /// </summary>
        /// <param name="inBOEForEdit">The BOE.</param>
        /// <param name="inWorkspace">Workspace for the BOE.</param>
        void SendBOEAuthorsEmailOpenedForEdit(FullBoe inBOEForEdit, FullWorkspace inWorkspace);

        /// <summary>
        /// Send an email to the BOE approvers that a BOE has moved from Draft to Awaiting Approval
        /// </summary>
        /// <param name="inBOEForApproval">BOE to approve</param>
        void SendBOEApproversEmailAwaitingApproval(FullBoe inBOEForApproval);

        /// <summary>
        /// Sends email messages based on parameters and uses Host
        /// Validation Note:
        /// - The method will validate that the # of tokens in the subject/body are equal to the number of tokens sent in
        /// (i.e. "Hi, {0}, I'm a message" should have 1 value in the *ReplaceTokens array)
        /// Special Logic:
        /// - If the web.config contains a true value for "EmailsToCurrentlyLoggedInUser",
        /// this method will attempt to locate the currently logged in users' email.
        /// General Note:
        /// Check your spam folder as it appears messages will go there by default!
        /// </summary>
        /// <param name="inEmailTypeToSend">The email type to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email, null if none</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="inUserData">The user data.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        void SendEmail(EmailTypes inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist, string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, UserData inUserData, int workspaceId);

        /// <summary>
        /// An email is sent to each user who has entered a comment that the Author has responded to
        /// </summary>
        /// <param name="inBOEComments">BOE Comment DTOs</param>
        void SendBOEAuthorRespondedToComment(Collection<BOECommentDTO> inBOEComments);

        /// <summary>
        /// Send all workspace Admins an email if all the BOES in the workspace have been approved
        /// </summary>
        /// <param name="inBOE">The BOE.</param>
        void SendWorkspaceAdminEmailAllBOEsApproved(FullBoe inBOE);

        /// <summary>
        /// Send email to BOE Author and all Approvers when an Approver has approved the BOE
        /// </summary>
        /// <param name="inApprover">The approver.</param>
        /// <param name="inBOE">The BOE that was approved</param>
        void SendBOEApproversAuthorApproverApproved(BoeApproverResponseDTO inApprover, FullBoe inBOE);

        /// <summary>
        /// Send email to BOE Author and all Approvers when an Approver has rejected the BOE
        /// </summary>
        /// <param name="inRejecter">The approver that rejected the boe.</param>
        /// <param name="inBOE">The BOE that was rejected</param>
        void SendBOEApproversAuthorApproverRejected(BoeApproverResponseDTO inRejecter, FullBoe inBOE);

        /// <summary>
        /// Send email to previous and current BOE Author when an author is changed
        /// </summary>
        /// <param name="inPreviousAuthors">The previous authors.</param>
        /// <param name="inBOE">The BOE whose author was changed</param>
        void SendBOEAuthorsChanged(Collection<UserDTO> inPreviousAuthors, FullBoe inBOE);

        /// <summary>
        /// Send email to BOE Author and all previous and current Approvers when an approver is changed
        /// </summary>
        /// <param name="inPreviousApprovers">The previous approvers.</param>
        /// <param name="inBOE">The BOE whose approvers were changed</param>
        void SendBOEApproversChanged(Collection<UserDTO> inPreviousApprovers, FullBoe inBOE);

        /// <summary>
        /// An email is sent to an Author, Approvers, and Workspace Admin if a BOE has been deleted
        /// from the Manage BOE and Manage WBS page
        /// </summary>
        /// <param name="inBOE">BOE that has been deleted</param>
        /// <param name="inDeletedBy">the User that deleted the BOE</param>
        /// <param name="approvers">Collection of Approver Ids</param>
        /// <param name="wbsAssociatedWithBoe">The WBS associated with BOE.</param>
        /// <param name="clinAssociatedWithBoe">The CLIN associated with BOE.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeStateDictionary">The BOE state dictionary.</param>
        void SendBOEDeleted(BoeDTO inBOE, UserDTO inDeletedBy, Collection<int> approvers, WbsDTO wbsAssociatedWithBoe, ClinDTO clinAssociatedWithBoe, FullWorkspace workspace, IDictionary<int, BOEStateModelView> boeStateDictionary);

        /// <summary>
        /// An email is sent to BOE Authors and Approvers for changes to BOE fields
        /// (they should have been checked prior to invocation that they were in Awaiting Approval or Approved states)
        /// </summary>
        /// <param name="inBOE">BOE to email Authors and Approvers about</param>
        /// <param name="inFields">The WBS fields that were changed</param>
        void SendBOEUpdatedToAuthorsAndApprovers(FullBoe inBOE, ICollection<FieldChanged> inFields);

        /// <summary>
        /// This function will send an author an email if the BOE has been opened for edit via the workspace or BOE
        /// </summary>
        /// <param name="inBOE">The BOE DTO.</param>
        /// <param name="inWorkspace">Workspace the BOE belongs to.</param>
        /// <param name="approvers">The list of approvers for the BOE.</param>
        /// <param name="authors">List of authors for the BOE.</param>
        /// <param name="inUserData">User data for the email.</param>
        void SendAuthorEmailBOEOpenedForEdit(FullBoe inBOE, FullWorkspace inWorkspace, ICollection<UserDTO> approvers, ICollection<UserDTO> authors, UserData inUserData);

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Working to Locked
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        void SendWorkspaceWorkingToLocked(FullWorkspace workspace);

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Locked to Working
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        void SendWorkspaceLockedToWorking(FullWorkspace workspace);

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Working to Initialization
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        void SendWorkspaceWorkingToInitialization(FullWorkspace workspace);

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Initialization to Working (all times except 1st)
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        void SendWorkspaceInitializationToWorkingSubsequent(FullWorkspace workspace);

        /// <summary>
        /// Send a workspace restored email to Workspace Admins, Authors, Approvers, and Reviewers
        /// </summary>
        /// <param name="inWorkspace">the workspace that was restored</param>
        /// <param name="inRestoredFromTime">the date the version was restored from</param>
        void SendWorkspaceRestored(FullWorkspace inWorkspace, DateTime inRestoredFromTime);

        /// <summary>
        /// Send BOE Author an email if a reviewer has added comments to the BOE
        /// </summary>
        /// <param name="inBoe">The BOE.</param>
        /// <param name="inReviewerID">The reviewer identifier.</param>
        void SendBOEAuthorReviewerCommented(FullBoe inBoe, int inReviewerID);

                /// <summary>
        /// Send BOE Authors and Approvers an email that an In Use Resource has been updated
        /// </summary>
        /// <param name="inResource">in use resource that was updated</param>
        /// <param name="inFieldChanges">resource changes</param>
        /// <param name="inWorkspaceAdmin"> admin who made the resource changed</param>
        /// <param name="inWorkspace">workspace the resource resides in</param>
        void SendBOEAuthorsApproversInUseResourceUpdated(ResourceDTO inResource, Collection<FieldChanged> inFieldChanges, UserDTO inWorkspaceAdmin, FullWorkspace inWorkspace);

        /// <summary>
        /// Sends the BOE email to authors approvers when dates updated.
        /// </summary>
        /// <param name="userDateChangeInfo">The user date change information.</param>
        /// <param name="isErrorEmail">Whether to use the error or normal dateshift email</param>
        void SendBOEAuthorsApproversDatesUpdated(HashSet<UserDateChangeInfo> userDateChangeInfo, bool isErrorEmail);

        /// <summary>
        /// An email is sent to BOE Authors and Approvers for CLIN that are in use
        /// (they should have been checked prior to invocation that they were in Awaiting Approval or Approved states)
        /// </summary>
        /// <param name="inBOE">BOE to email Authors and Approvers about</param>
        /// <param name="inFields">The CLIN fields that were changed</param>
        void SendCLINUpdatedToAuthorsAndApprovers(FullBoe inBOE, ICollection<FieldChanged> inFields);

        /// <summary>
        /// Send an email when a template is assigned
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="emailType">Template Email type</param>
        void SendRteTemplateEmail(FullWorkspace ws, EmailTypes emailType);
    }
}
