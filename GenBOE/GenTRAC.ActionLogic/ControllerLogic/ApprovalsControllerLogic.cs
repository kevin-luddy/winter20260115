// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Checklist Controller Logic
    /// </summary>
    public class ApprovalsControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// The name of the approver index form, needed for validation.
        /// </summary>
        public const string APPROVAL_INDEX_FORM = "approvalIndexForm";

        /// <summary>
        /// The logger
        /// </summary>
        private readonly IES.Common.Logger log = new IES.Common.Logger(typeof(ApprovalsControllerLogic));

        /// <summary>
        /// User Loader
        /// </summary>
        private readonly IUserLoader userLoader = null;

        /// <summary>
        /// Emailer
        /// </summary>
        private readonly IPtmEmailer emailer = null;

        /// <summary>
        /// The approval emailer.
        /// </summary>
        public ApprovalEmailer ApprovalEmailer { get; set; }

        /// <summary>
        /// User Loader
        /// </summary>
        private readonly IAttachmentLoader attachmentLoader;

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private readonly IActiveDirectoryUtilities adUtils;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="inUserLoader">User Loader</param>
        /// <param name="inEmailer">emailer</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="approvalsLoader">Approvals Loaders</param>
        /// <param name="proposalMediator">proposal mediator</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inApprovalEmailer">The approval emailer.</param>
        /// <param name="attachmentLoader">Attachment loader</param>
        /// <param name="adUtils">Active Directory Utilities</param>
        public ApprovalsControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IUserMapper inUserMapper,
            IUserLoader inUserLoader,
            IPtmEmailer inEmailer,
            IFullObjectFactory objectFactory,
            IApprovalsLoader approvalsLoader,
            IProposalMediator proposalMediator,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator inChecklistMediator,
            ApprovalEmailer inApprovalEmailer,
            IAttachmentLoader attachmentLoader,
            IActiveDirectoryUtilities adUtils)
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, proposalMediator)
        {
            this.userLoader = inUserLoader;
            this.emailer = inEmailer;
            this.ApprovalEmailer = inApprovalEmailer;
            this.attachmentLoader = attachmentLoader;
            this.adUtils = adUtils;
        }

        /// <summary>
        /// Get Approvals index view  
        /// </summary>
        /// <param name="proposalId">ID of the proposal</param>
        /// <returns>Approvals Index Model View</returns>
        public ApprovalsIndexModelView GetApprovals(int proposalId)
        {
            ApprovalsIndexModelView model = new ApprovalsIndexModelView();

            FullProposal proposal = this.GetFullProposalDto(proposalId);

            ProposalPermissionDto leadEstimatorPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.Pricer);
            ProposalPermissionDto coverSheetApproverPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.CoverSheetApprover);
            ProposalPermissionDto pricingVerificationPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.PricingVerification);
            ProposalPermissionDto independentReviewerPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.PeerReviewer);
            ProposalPermissionDto lobEstLeadPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.LOBEstLead);

            bool coverSheetApproverRequired = proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value;
            bool pricingVerifierRequired = proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value;
            
            if (leadEstimatorPermissions == null || (coverSheetApproverRequired && coverSheetApproverPermissions == null) || (pricingVerifierRequired && pricingVerificationPermissions == null) || lobEstLeadPermissions == null || (this.IndependentReviewerIsNeeded(proposal) && independentReviewerPermissions == null))
            {
                model.MissingRole = true;
            }

            model.IsPricer = this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId);

            if (leadEstimatorPermissions != null)
            {
                UserDTO user = this.UserMapper.GetById(leadEstimatorPermissions.UserId);
                if (user != null)
                {
                    model.LeadEstimatorName = user.DisplayName;
                }
            }

            return model;
        }

        /// <summary>
        /// Gets the Approval model for the given role
        /// </summary>
        /// <param name="proposalId">ID of the proposal</param>
        /// <param name="role">role of the approver</param>
        /// <returns>Approval Model View for the given role</returns>
        public ApprovalSectionModelView GetApprovalModel(int proposalId, PtmRole role)
        {
            FullProposal proposal = this.GetFullProposalDto(proposalId);
            ApprovalSectionModelView model = null;
            ProposalPermissionDto userPermissions = proposal.Permissions.FirstOrDefault(x => x.Role == role);

            // if permissions for the role aren't set (ex. Independent Reviewer/Cover Sheet Approver isn't required or set) or if section is
            // unavailable due not being at the appropriate workflow step, return model as null
            if (userPermissions != null && this.IsApprovalSectionAvailable(proposal, role))
            {
                model = new ApprovalSectionModelView();
                UserDTO user = this.UserMapper.GetById(userPermissions.UserId);
                model.ApproverRole = role;
                
                model.ApproverName = user.DisplayName;
                switch (role)
                {
                    case PtmRole.Pricer:
                        model.Comments = proposal.LeadEstimatorSignatureComment ?? string.Empty;
                        model.DateOfApproval = proposal.LeadEstimatorSignedDate;
                        model.AdditionalEmailText = proposal.ApprovalEmailText ?? string.Empty;
                        // only show the reset workflow button if the workflow has been started and the current user is the lead or backup estimator.
                        model.ShowResetWorkflowButton = proposal.WorkflowStatus != WorkflowStatus.NotStarted && proposal.ProposalStatus != ProposalStatus.Revised && this.IsCurrentUserPricerOrBackupEstimator(proposal.Id);
                        model.AllAttachmentsHaveBeenUploaded = this.attachmentLoader.AllRequiredAttachmentsHaveBeenUploaded(proposal.Id);
                        model.IsNoBid = proposal.ProposalStatus == ProposalStatus.NoBid;
                        break;
                    case PtmRole.CoverSheetApprover:
                        model.Comments = proposal.CoverSheetApproverSignatureComment ?? string.Empty;
                        model.DateOfApproval = proposal.CoverSheetApproverSignedDate;
                        break;
                    case PtmRole.PricingVerification:
                        model.Comments = proposal.PricingVerifierSignatureComment ?? string.Empty;
                        model.DateOfApproval = proposal.PricingVerifierSignedDate;
                        break;
                    case PtmRole.PeerReviewer:
                        model.Comments = proposal.IndependentReviewerSignatureComment ?? string.Empty;
                        model.DateOfApproval = proposal.IndependentReviewerSignedDate;
                        break;
                    case PtmRole.LOBEstLead:
                        model.Comments = proposal.LOBEstimatingLeadSignatureComment ?? string.Empty;
                        model.DateOfApproval = proposal.LOBEstimatingLeadSignedDate;
                        model.AllAttachmentsHaveBeenUploaded = this.attachmentLoader.AllRequiredAttachmentsHaveBeenUploaded(proposal.Id);
                        model.HasNonPreferredTool = proposal.BoeTool != BOETool.genBOE || proposal.PricingTool != PricingTool.ProPricer;
                        break;
                    default:
                        break;
                }

                // Section is read only except for the user who needs to approve it, if it hasn't been approved already.  The ProposalStatus must also
                // NOT be deleted or archived.
                bool userIsApprover = proposal.CurrentUser.Ntid == user.Ntid;
                if (model.DateOfApproval == null && userIsApprover && !this.IsProposalDeletedOrArchived(proposalId))
                {
                    model.IsReadOnly = false;
                }
            }

            return model;
        }

        /// <summary>
        /// Determines if the Proposal with the given ID has a ProposalStatus of either Deleted or Archived.
        /// </summary>
        /// <param name="proposalId">The ID of the Proposal.</param>
        /// <returns>Boolean value representing whether the ProposalStatus is either Deleted or Archived.</returns>
        public bool IsProposalDeletedOrArchived(int proposalId)
        {
            FullProposal proposal = this.GetFullProposalDto(proposalId);

            return proposal.ProposalStatus == ProposalStatus.Deleted || proposal.ProposalStatus == ProposalStatus.Archived;
        }

        /// <summary>
        /// Determines if the No Bid button will be enabled
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>True if user no bid should be enabled</returns>
        public bool IsNoBidEnabled(int proposalId)
        {
            FullProposal proposal = this.GetFullProposalDto(proposalId);
            return proposal.ProposalStatus == ProposalStatus.InProgress;
        }

        /// <summary>
        /// Validate that the Cover Sheet approver is still approved
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="validationMessages">Validation Messages</param>
        public void ValidateCoverSheetApprover(int proposalId, ICollection<ValidationMessage> validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            FullProposal proposal = this.GetFullProposalDto(proposalId);
            ProposalPermissionDto coverSheetApproverPermission = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.CoverSheetApprover);

            if(coverSheetApproverPermission != null)
            {
                UserDTO coverSheetApprover = this.UserMapper.GetById(coverSheetApproverPermission.UserId);
                string csaGroup = IES.Common.ConfigurationUtilities.GetAppSetting("CoverSheetApprovers").Split('\\').LastOrDefault();
                bool isMemeberOfCsaGroup = this.adUtils.IsMemberOfADGroup(coverSheetApprover.Ntid, csaGroup);

                if (!isMemeberOfCsaGroup)
                {
                    validationMessages.Add(new ValidationMessage(ValidationConstants.ProposalValidationConstants.COVER_SHEET_APPROVER_NO_LONGER_APPROVED));
                }
            }
        }

        /// <summary>
        /// Saves the approval for the given role
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <param name="role">role submitting approval</param>
        /// <param name="comment">approval comment</param>
        /// <param name="additionalEmailText">Additional approval email text.</param>
        /// <param name="baseUrlForInstructionLocation">Base URL for Instruction Location</param>
        /// <param name="pricerSavedWhilePeerEditing">Bool noting if a Pricer saved while Peer was editing</param>
        public void SaveApproval(int proposalId, PtmRole role, string comment, string additionalEmailText, string baseUrlForInstructionLocation, out bool pricerSavedWhilePeerEditing)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsControllerLogic.SaveApproval", this.log))
            {
                FullProposal proposal = this.GetFullProposalDto(proposalId);
                switch (role)
                {
                    case PtmRole.Pricer:
                        proposal.LeadEstimatorSignedDate = DateTime.Now;
                        proposal.LeadEstimatorSignatureComment = comment;
                        proposal.WorkflowStatus = WorkflowStatus.Started;
                        proposal.ApprovalEmailText = additionalEmailText;
                        break;
                    case PtmRole.CoverSheetApprover:
                        proposal.CoverSheetApproverSignedDate = DateTime.Now;
                        proposal.CoverSheetApproverSignatureComment = comment;
                        break;
                    case PtmRole.PricingVerification:
                        proposal.PricingVerifierSignedDate = DateTime.Now;
                        proposal.PricingVerifierSignatureComment = comment;
                        break;
                    case PtmRole.PeerReviewer:
                        proposal.IndependentReviewerSignedDate = DateTime.Now;
                        proposal.IndependentReviewerSignatureComment = comment;
                        break;
                    case PtmRole.LOBEstLead:
                        proposal.LOBEstimatingLeadSignedDate = DateTime.Now;
                        proposal.LOBEstimatingLeadSignatureComment = comment;
                        proposal.WorkflowStatus = WorkflowStatus.ProposalLocked;
                        proposal.WorkflowStatusLastUpdated = DateTime.Now;
                        break;
                }

                if (role != PtmRole.LOBEstLead)
                {
                    bool coverSheetApproverRequired = proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value;
                    // pricing verifier required if CCPD is yes or if it is set when CCPD is no (this validation handled through UI, only need to check if the role is set)
                    bool pricingVerifierRequired = proposal.Permissions.Any(p => p.Role == PtmRole.PricingVerification);
                    
                    // if all approvers have approved, then set workflow status accordingly
                    if ((!coverSheetApproverRequired || proposal.CoverSheetApproverSignedDate.HasValue) && (!pricingVerifierRequired || proposal.PricingVerifierSignedDate.HasValue))
                    {
                        bool hasIndependentReviewer = proposal.Permissions.Any(p => p.Role == PtmRole.PeerReviewer);

                        // check to see if independent reviewer has approved or if not set
                        if (proposal.IndependentReviewerSignedDate.HasValue || !hasIndependentReviewer)
                        {
                            proposal.WorkflowStatus = WorkflowStatus.AllApproved;
                            proposal.WorkflowStatusLastUpdated = DateTime.Now;
                        }
                    }
                }

                proposal.Updateable = IES.Common.UpdateType.Upsert;
                this.ProposalMediator.SaveProposal(proposal);

                pricerSavedWhilePeerEditing = false;
                if (role == PtmRole.LOBEstLead)
                {
                    this.SubmitChecklist(proposalId, baseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);
                }
            }
        }

        /// <summary>
        /// Submit checklist after LOB Est Lead approves
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="baseUrlForInstructionLocation">Base URL for Instruction Location</param>
        /// <param name="pricerSavedWhilePeerEditing">Bool noting if a Pricer saved while Peer was editing</param>
        private void SubmitChecklist(int proposalId, string baseUrlForInstructionLocation, out bool pricerSavedWhilePeerEditing)
        {
            ChecklistGeneralInformationModelView checklistGeneralInfo = this.GetDataForChecklistGeneralInformation(proposalId);
            ChecklistProposalPricingDataModelView checklistProposalPricingData = this.GetDataForChecklistProposalPricingData(proposalId);
            ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData = this.GetDataForChecklistPPRDocument(proposalId);
            ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData = this.GetDataForChecklistPARDocument(proposalId, baseUrlForInstructionLocation);

            // check if the pricer has saved since the peer opened the checklist. If so, add something to the Json result so we can display
            // an alert to the user, but continue with the save.
            pricerSavedWhilePeerEditing = this.HasPricerSavedSincePeerReviewOpenedChecklist(checklistGeneralInfo, checklistPARDocumentData);

            this.SaveChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRDocumentData, checklistPARDocumentData, true);
        }

        /// <summary>
        /// Resets the workflow.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        public void ResetWorkflow(int proposalId)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsControllerLogic.ResetWorkflow", this.log))
            {
                FullProposal proposal = this.GetFullProposalDto(proposalId);

                proposal.WorkflowStatus = WorkflowStatus.NotStarted;
                proposal.WorkflowStatusLastUpdated = null;
                proposal.LeadEstimatorSignedDate = null;
                proposal.CoverSheetApproverSignedDate = null;
                proposal.PricingVerifierSignedDate = null;
                proposal.IndependentReviewerSignedDate = null;
                proposal.LOBEstimatingLeadSignedDate = null;

                proposal.Updateable = IES.Common.UpdateType.Upsert;
                this.ProposalMediator.SaveProposal(proposal);

                // Get the proposal again to get the updated date
                proposal = this.GetFullProposalDto(proposalId);
                this.ChecklistMediator.UnlockChecklist(proposal.Id, proposal.UpdateDate, UnlockChecklistOption.UnlockPricer);
            }
        }

        /// <summary>
        /// SendApprovalEMail
        /// </summary>
        /// <param name="proposalId">int</param>
        /// <param name="approverRole">role</param>
        public void SendApprovalEMail(int proposalId, PtmRole approverRole)
        {
            FullProposal proposalDto = this.GetFullProposalDto(proposalId);
            UserDTO leadEstimator = this.userLoader.GetById(proposalDto.Permissions.First(x => x.Role == PtmRole.Pricer).UserId);
            UserDTO approver = this.userLoader.GetById(proposalDto.Permissions.First(x => x.Role == approverRole).UserId);
            EmailContent emailContent = Emails.APPROVAL_EMAIL;
            string commonUrl = IES.Common.ConfigurationUtilities.GetAppSetting("ServerURL");
            // todo: verify where we want link back to land
            string url = string.Format("{0}/proposal/DisplayProposalDetails/id/{1}/#Proposal", commonUrl, proposalId);
            string[] subjectTokens = { proposalDto.TrackingNumber.ToString(), proposalDto.ProposalTitle };
            string[] bodyTokens = { approver.DisplayName, ExtensionMethods.GetDescription(approverRole), url };

            this.emailer.SendDelegateEmail(emailContent, leadEstimator.EmailAddress, null, subjectTokens, bodyTokens, null);
        }

        /// <summary>
        /// Determines if Approval section should be shown based on workflow
        /// </summary>
        /// <param name="proposal">Proposal</param>
        /// <param name="role">Approval role</param>
        /// <returns>true if section should be shown, false if it shouldn't</returns>
        private bool IsApprovalSectionAvailable(FullProposal proposal, PtmRole role)
        {
            bool toReturn = false;

            bool hasIndependentReviewer = proposal.Permissions.Any(p => p.Role == PtmRole.PeerReviewer);
            bool coverSheetApproverRequired = (proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value) || proposal.Permissions.Any(p => p.Role == PtmRole.CoverSheetApprover);
            bool pricingVerificationApproverRequired = (proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value) || proposal.Permissions.Any(p => p.Role == PtmRole.PricingVerification);

            switch (role)
            {
                case PtmRole.Pricer:
                    // Lead Estimator always available
                    toReturn = true;
                    break;
                case PtmRole.CoverSheetApprover:
                    toReturn = proposal.LeadEstimatorSignedDate != null && coverSheetApproverRequired;
                    break;
                case PtmRole.PricingVerification:
                    toReturn = proposal.LeadEstimatorSignedDate != null && pricingVerificationApproverRequired;
                    break;
                case PtmRole.PeerReviewer:
                    // Cover Sheet Approver, Pricing Verification, and Independent Reviewer only available if Lead Estimator approved
                    toReturn = proposal.LeadEstimatorSignedDate != null && hasIndependentReviewer;
                    break;
                case PtmRole.LOBEstLead:
                    // LOB Estimating Lead only available if Lead Estimator, Cover Sheet Approver (if needed), Pricing Verification (if needed), and (if needed) Independent Reviewer approved
                    toReturn = proposal.LeadEstimatorSignedDate != null && (!coverSheetApproverRequired || proposal.CoverSheetApproverSignedDate != null) && (!pricingVerificationApproverRequired || proposal.PricingVerifierSignedDate != null) &&
                        (!hasIndependentReviewer || proposal.IndependentReviewerSignedDate != null);
                    break;
                default:
                    break;
            }

            return toReturn;
        }

        /// <summary>
        /// Determines if the user has access to set/revert No Bid
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>True if user has access, otherwise false</returns>
        public bool HasAccessToSetNoBid(int proposalId)
        {
            FullProposal proposal = this.GetFullProposalDto(proposalId);
            bool isLeadOrBackup = proposal.Permissions.Any(x => (x.Role == PtmRole.Pricer || x.Role == PtmRole.BackupPricer) && x.UserId == proposal.CurrentUser.Id);
            bool isAdmin = this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);

            return (isLeadOrBackup || isAdmin) && proposal.ProposalStatus != ProposalStatus.Revised;
        }

        /// <summary>
        /// Set proposal to No Bid status
        /// </summary>
        /// <param name="proposalId">ID of Proposal</param>
        public void SetProposalToNoBid(int proposalId)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsControllerLogic.SetProposalToNoBid", this.log))
            {
                // set status no bid
                FullProposal proposal = this.GetFullProposalDto(proposalId);
                proposal.ProposalStatus = ProposalStatus.NoBid;
                proposal.NoBidDate = DateTime.Now;
                proposal.Updateable = IES.Common.UpdateType.Upsert;
                this.ProposalMediator.SaveProposal(proposal);
            }
        }

        /// <summary>
        /// Revert the Proposal from No Bid back to In Progress
        /// </summary>
        /// <param name="proposalId">ID of Proposal to revert</param>
        public void RevertProposalFromNoBid(int proposalId)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ApprovalsControllerLogic.RevertProposalFromNoBid", this.log))
            {
                // set status in progress
                FullProposal proposal = this.GetFullProposalDto(proposalId);
                proposal.ProposalStatus = ProposalStatus.InProgress;
                proposal.NoBidDate = null;
                proposal.Updateable = IES.Common.UpdateType.Upsert;
                this.ProposalMediator.SaveProposal(proposal);
            }

            // reset workflow
            this.ResetWorkflow(proposalId);
        }
    }
}
