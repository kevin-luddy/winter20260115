// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using ActionLogic.ModelView.Admin;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;

    /// <summary>
    /// Proposal Controller
    /// </summary>
    public class ProposalController : GenTRACController
    {
        /// <summary>
        /// Proposal Controller Logic
        /// </summary>
        private ProposalControllerLogic proposalLogic = null;

        /// <summary>
        /// Checklist Controller Logic - needed to clear Checklist if changing ProposalClass to Forecasted.
        /// </summary>
        private ChecklistControllerLogic checklistLogic = null;

        /// <summary>
        /// Post Submittal Attachments Controller Logic - needed to clear Post Submittal Attachments if changing ProposalClass to Forecasted.
        /// </summary>
        private PostSubmittalAttachmentsControllerLogic psaLogic = null;

        /// <summary>
        /// Admin Controller Logic
        /// </summary>
        private AdminControllerLogic adminLogic = null;

        /// <summary>
        /// Pick List Mapper
        /// </summary>
        private IPickListMapper pickListMapper = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inGenTRACControllerLogic">GenTRAC Controller Logic</param>
        /// <param name="inSiteMasterUtilities">Site Master Utilities</param>
        /// <param name="inProposalLogic">Proposal Controller Logic</param>
        /// <param name="checklistLogic">Checklist Controller Logic</param>
        /// <param name="psaLogic">Post Submittal Attachments Controller Logic</param>
        /// <param name="adminLogic">Admin Controller Logic</param>
        /// <param name="inSecurityInformation">security information about user and their context</param>
        /// <param name="pickListMapper">The picklist mapper.</param>
        public ProposalController(
            GenTRACControllerLogic inGenTRACControllerLogic,
            SiteMasterUtilities inSiteMasterUtilities,
            ProposalControllerLogic inProposalLogic,
            ChecklistControllerLogic checklistLogic,
            PostSubmittalAttachmentsControllerLogic psaLogic,
            AdminControllerLogic adminLogic,
            IES.Common.ISecurityInformation inSecurityInformation,
            IPickListMapper pickListMapper)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.proposalLogic = inProposalLogic;
            this.checklistLogic = checklistLogic;
            this.psaLogic = psaLogic;
            this.adminLogic = adminLogic;
            this.pickListMapper = pickListMapper;
        }

        /// <summary>
        /// Display proposal index
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="isNewRevision">If displaying Proposal Index for creating a new Revision</param>
        /// <param name="oldProposalId">ID of the old proposal if creating a new Revision</param>
        /// <returns>create new proposal view</returns>
        public ViewResult DisplayProposalIndex(int? proposalId, bool? isNewRevision, int? oldProposalId)
        {
            ProposalIndexModelView model;

            if (isNewRevision.HasValue && isNewRevision.Value)
            {
                model = this.proposalLogic.GetDataForProposalRevisionIndex(oldProposalId.Value);
            }
            else
            {
                model = this.proposalLogic.GetDataForProposalIndex(proposalId);
            }
 
            return this.View(WebConstants.View.PROPOSAL_INDEX, model);
        }

        /// <summary>
        /// Display proposal details tabs
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>create new proposal view</returns>
        public ActionResult DisplayProposalDetails(int? proposalId)
        {
            ProposalIndexModelView model = this.proposalLogic.GetDataForProposalIndex(proposalId);
            return this.View(WebConstants.View.PROPOSAL_DETAILS, model); // this is for the tabs
        }

        /// <summary>
        /// Validate proposal for creating a new Revision
        /// </summary>
        /// <param name="proposalId">ID of the proposal being revised</param>
        /// <returns>True if valid, validation errors if invalid</returns>
        public JsonResult ValidateNewProposalRevision(int proposalId)
        {
            this.proposalLogic.ValidateSaveNewRevision(proposalId);
            return this.Json(true);
        }

        /// <summary>
        /// Display proposal info for creating a new Revision
        /// </summary>
        /// <param name="proposalId">ID of the proposal being revised</param>
        /// <returns>create new revision view</returns>
        public ActionResult DisplayProposalRevisionDetails(int proposalId)
        {
            this.proposalLogic.ValidateSaveNewRevision(proposalId);

            ProposalIndexModelView model = this.proposalLogic.GetDataForProposalRevisionIndex(proposalId);
            return this.View(WebConstants.View.PROPOSAL_DETAILS, model);
        }

        /// <summary>
        /// Display proposal information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="isNewRevision">Whether creating a new revision</param>
        /// <returns>proposal information view</returns>
        public PartialViewResult DisplayProposalInformation(int? proposalId, bool isNewRevision = false)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalInformationModelView model;
            if (isNewRevision)
            {
                model = this.proposalLogic.GetDataForProposalRevisionInformation(proposalId);
            }
            else
            {
                model = this.proposalLogic.GetDataForProposalInformation(proposalId);
            }

            return this.PartialView(WebConstants.View.PROPOSAL_INFORMATION, model);
        }

        /// <summary>
        /// Display proposal general information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="isNewRevision">Whether creating a new revision</param>
        /// <returns>proposal general information view</returns>
        public PartialViewResult DisplayProposalGeneralInformation(int? proposalId, bool isNewRevision = false)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalGeneralInformationModelView model = this.proposalLogic.GetDataForProposalGeneralInformation(proposalId, isNewRevision);
            return this.PartialView(WebConstants.View.PROPOSAL_GENERAL_INFORMATION, model);
        }

        /// <summary>
        /// Display proposal user information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="isNewRevision">Whether creating a new revision</param>
        /// <returns>proposal user information view</returns>
        public PartialViewResult DisplayProposalApprovals(int? proposalId, bool isNewRevision = false)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalApprovalsModelView model = this.proposalLogic.GetDataForProposalApprovals(proposalId, isNewRevision);
            return this.PartialView(WebConstants.View.PROPOSAL_APPROVALS, model);
        }

		/// <summary>
		/// Display proposal user information
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <param name="isNewRevision">Whether creating a new revision</param>
		/// <returns>proposal user information view</returns>
		public PartialViewResult DisplayProposalUserInformation(int? proposalId, bool isNewRevision = false)
		{
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalUserInformationModelView model = this.proposalLogic.GetDataForProposalUserInformation(proposalId, isNewRevision);
            return this.PartialView(WebConstants.View.PROPOSAL_USER_INFORMATION, model);
        }

        /// <summary>
        /// Display Proposal Comments
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Proposal Comments view</returns>
        public PartialViewResult DisplayProposalComments(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalCommentsModelView model = this.proposalLogic.GetDataForProposalComments(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_COMMENTS, model);
        }

        /// <summary>
        /// Validates a proposal.  Throws an exception if any errors are detected.
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="proposalInfo">Proposal information model view</param>
        /// <param name="proposalGeneralInfo">Proposal general information model view</param>
        /// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
        /// <param name="proposalUserInfo">Proposal user information model view</param>
        /// <returns>JsonResult containing values for invalidUnsavedUsers, activeUserNtid, leadEstimatorNtid, backupPricerNtId, and backupPricerDisplayName</returns>
        public JsonResult ValidateProposal(int? proposalId, ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
            ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo)
        {
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            bool invalidUnsavedUsers = this.ValidateProposalFields(proposalId, proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, validationErrors);

            return this.Json(new
            {
                invalidUsers = invalidUnsavedUsers.ToString().ToLower(),
                // return values used by client to detect whether user will have access to proposal when saved.
                activeUserNtid = this.proposalLogic.GetActiveUser().Ntid,
                leadEstimatorNtid = proposalApprovalsInfo == null || string.IsNullOrEmpty(proposalApprovalsInfo.LeadEstimatorNtid) ? string.Empty : proposalApprovalsInfo.LeadEstimatorNtid,
                backupPricerNtId = proposalUserInfo == null || string.IsNullOrEmpty(proposalUserInfo.BackupPricerNtId) ? string.Empty : proposalUserInfo.BackupPricerNtId,
                backupPricerDisplayName = proposalUserInfo == null || string.IsNullOrEmpty(proposalUserInfo.BackupPricerDisplayName) ? string.Empty : proposalUserInfo.BackupPricerDisplayName
            });
        }

        /// <summary>
        /// Helper method for validating proposal fields.  Throws an exception if any errors are detected.
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="proposalInfo">Proposal information model view</param>
        /// <param name="proposalGeneralInfo">Proposal general information model view</param>
        /// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
        /// <param name="proposalUserInfo">Proposal user information model view</param>
        /// <param name="validationErrors">validation error collection</param>
        /// <returns>true if there are invalid unsaved users; false otherwise</returns>
        private bool ValidateProposalFields(int? proposalId, ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
            ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo,
            List<ValidationMessage> validationErrors)
        {
            ICollection<SelectListItem> proposalClassesList = this.pickListMapper.GetSelectListPickList(PickListEnum.ProposalClass, proposalInfo.ProposalClass);
            string proposalClassText = proposalClassesList.Any(x => x.Value == proposalInfo.ProposalClass.ToString()) ?
                    proposalClassesList.First(x => x.Value == proposalInfo.ProposalClass.ToString()).Text : "Not Set";

            bool isForecasted = proposalClassText == Constants.PROPOSAL_CLASS_FORECASTED;
            this.proposalLogic.ValidateProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, validationErrors, isForecasted);
            this.proposalLogic.ValidateGeneralInfoTypes(proposalGeneralInfo, validationErrors, isForecasted, proposalInfo.ProposalClass);
			
			bool invalidUnsavedUsers = false;
            if (!isForecasted)
			{
				// LOBs don't have consistent IDs between dev/uat/prod so we need to get the LOB picklist to get the NSS ID
				// in order to pass if the proposal has NSS as its LOB to the helper method
				ICollection<SelectListItem> lobList = pickListMapper.GetSelectListPickList(PickListEnum.LineOfBusiness);
				SelectListItem nssLob = lobList.FirstOrDefault(x => x.Text == Constants.NSS_LOB_NAME);
				bool isNss = nssLob != null && proposalGeneralInfo.LineOfBusiness.ToString() == nssLob.Value;
				invalidUnsavedUsers = this.proposalLogic.ValidateUserTypes(proposalId, proposalApprovalsInfo, proposalUserInfo, validationErrors, isNss);
            }

            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.ProposalControllerLogic.PROPOSAL_INFO_FORM);
            if (validationErrors.Any())
            {
                // Sort validation errors by section
                string[] sortArray = new string[4] { "Proposal Information", "General Information", "Approvals", "User Information" };
                validationErrors.Sort((a, b) =>
                    Array.IndexOf(sortArray, a.ValidationIssue.Split('(', ')')[1])
                    .CompareTo(Array.IndexOf(sortArray, b.ValidationIssue.Split('(', ')')[1])));

                throw new ValidationException(validationErrors);
            }

            return invalidUnsavedUsers;
        }

        /// <summary>
        /// Saves a proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="proposalInfo">Proposal information model view</param>
        /// <param name="proposalGeneralInfo">Proposal general information model view</param>
        /// <param name="proposalApprovalsInfo">Proposal approvals model view</param>
        /// <param name="proposalUserInfo">Proposal user information model view</param>
        /// <param name="proposalComments">Proposal Comments model view</param>
        /// <param name="revisionOfId">RevisionOfId for the Proposal</param>
        /// <param name="isNewRevision">Whether creating a new revision</param>
        /// <returns>true if success else false</returns>
        public JsonResult SaveProposal(int? proposalId, ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
            ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo, ProposalCommentsModelView proposalComments, int? revisionOfId, bool isNewRevision = false)
        {
            _ = proposalInfo ?? throw new ArgumentNullException(nameof(proposalInfo));
            _ = proposalGeneralInfo ?? throw new ArgumentNullException(nameof(proposalGeneralInfo));
            _ = proposalApprovalsInfo ?? throw new ArgumentNullException(nameof(proposalApprovalsInfo));

            if (isNewRevision)
            {
                proposalId = null;
                this.proposalLogic.ValidateNewRevisionDoesNotExist(proposalInfo);
            }

            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            bool invalidUnsavedUsers = this.ValidateProposalFields(proposalId, proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, validationErrors);

            ProposalGeneralInformationModelView originalProposalInfo = this.proposalLogic.GetDataForProposalGeneralInformation(proposalId, false);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                if (isNewRevision && revisionOfId.HasValue)
                {
                    ProposalDto proposal = this.proposalLogic.GetByProposalId(revisionOfId.Value);
                    this.proposalLogic.SetProposalRevised(proposal.Id, proposal.UpdateDate);
                }

                // The new revision doesn't have an RDSB document associated with it, so we need to clear the DocumentId
                if (isNewRevision)
				{
                    proposalInfo.DocumentId = null;
                }

                proposalId = this.proposalLogic.SaveProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, proposalComments, revisionOfId);

                if (proposalId.HasValue)
                {
                    bool isForecasted = false;

                    // Forecasted proposals should have all of their Checklist data and Post Submittal Attachments deleted.
                    if (proposalInfo != null)
                    {
                        isForecasted = proposalInfo.ProposalClassText == Constants.PROPOSAL_CLASS_FORECASTED;
                        if (isForecasted)
                        {
                            this.psaLogic.DeleteAllAttachments(proposalId);
                            this.checklistLogic.DeleteChecklist(proposalId);
                        }
                    }

                    // Only send these emails if non-forecasted
                    if (!isForecasted)
                    {
                        // Check to see if Non-preferred/Preferred BOE/Pricing Tool emails need to be sent
                        if ((proposalGeneralInfo.BOETool != BOETool.genBOE && proposalGeneralInfo.BOETool != originalProposalInfo.BOETool)
                            || (proposalGeneralInfo.PricingTool != PricingTool.ProPricer && proposalGeneralInfo.PricingTool != originalProposalInfo.PricingTool))
                        {
                            // Send non-preferred tool email if non-preferred tool selected and it hadn't been selected already (or if new proposal)
                            this.proposalLogic.SendNonpreferredToolsEmail(proposalApprovalsInfo.LOBEstimatingLeadMgrNtid, proposalInfo);
                        }
                        else if (originalProposalInfo.ProposalID > 0 && proposalGeneralInfo.BOETool == BOETool.genBOE && proposalGeneralInfo.PricingTool == PricingTool.ProPricer
                                && (originalProposalInfo.BOETool != BOETool.genBOE || originalProposalInfo.PricingTool != PricingTool.ProPricer))
                        {
                            // Send preferred tool email if preferred tools selected after having previously selected a non-preferred one
                            this.proposalLogic.SendPreferredToolsEmail(proposalApprovalsInfo.LOBEstimatingLeadMgrNtid, proposalInfo);
                        }
                    }

                    scope.Complete();
                    return this.Json(new { proposalId = proposalId.Value.ToString(), invalidUsers = invalidUnsavedUsers.ToString().ToLower() });
                }
            }

            return this.Json(new { Status = false });
        }

        /// <summary>
        /// Save only the comments of a read-only proposal
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="proposalComments">Proposal Comments</param>
        /// <returns>json result with proposal id</returns>
        public JsonResult SaveProposalComments(int proposalId, ProposalCommentsModelView proposalComments)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                int? returnedProposalId = this.proposalLogic.SaveProposalComments(proposalId, proposalComments);

                scope.Complete();
                return this.Json(new { proposalId = returnedProposalId.Value.ToString() });
            }
        }

        /// <summary>
        /// Delete a Proposal.
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>JSON result</returns>
        public JsonResult DeleteProposal(int proposalId)
        {
            ManageProposalInfoDetailsView manageProposalInfo = this.adminLogic.GetManageProposalInfoDetailsView(proposalId);
            manageProposalInfo.NewStatus = ProposalStatus.Deleted;

            // Validate and save the proposal (similar to AdminController.SaveManageProposalInfoDetails action)
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.adminLogic.ValidateManageProposalInfo(manageProposalInfo, validationErrors);
            this.adminLogic.ValidateDeleteProposal(manageProposalInfo.ProposalID, validationErrors);

            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.HomeControllerLogic.PROPOSAL_GRID_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                int? result = this.adminLogic.SaveManageProposalInfo(proposalId, manageProposalInfo);
                if (result.HasValue && result.Value > 0)
                {
                    scope.Complete();
                    return this.Json(new { proposalId = result.Value.ToString() });
                }
            }

            return this.Json(new { Status = false });
        }

        /// <summary>
        /// Revert a Proposal Revision to the prior version, deleting this version
        /// </summary>
        /// <param name="proposalId">ID of Proposal to be reverted</param>
        /// <returns>JSON result with ID of prior version</returns>
        public JsonResult RevertProposalToPriorVersion(int proposalId)
        {
            ProposalDto proposal = this.proposalLogic.GetByProposalId(proposalId);

            // validate reverting to prior version
            this.proposalLogic.ValidateRevertRevisionToPriorVersion(proposal);

            // validate deleting proposal 
            ManageProposalInfoDetailsView manageProposalInfo = this.adminLogic.GetManageProposalInfoDetailsView(proposalId);
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            this.adminLogic.ValidateDeleteProposal(manageProposalInfo.ProposalID, validationErrors);
            
            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                // delete proposal
                this.proposalLogic.DeleteProposal(proposal);

                // set status of prior version
                this.proposalLogic.RevertRevisedProposal(proposal.RevisionOfId.Value);

                scope.Complete();
            }

            return this.Json(new { proposalId = proposal.RevisionOfId });
        }

        /// <summary>
        /// Will get the Program Areas for the selected Line of Business
        /// </summary>
        /// <param name="lineOfBusiness">Selected Line of Business</param>
        /// <returns>Html for Program Area options</returns>
        public string LoadFilteredProgramAreas(string lineOfBusiness)
        {
            if (!string.IsNullOrEmpty(lineOfBusiness))
            {
                int lineOfBusinessId = int.Parse(lineOfBusiness);
                return this.proposalLogic.GetProgramAreasForLineOfBusiness(lineOfBusinessId, null);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Will get the Contract Types for the selected Contract Type Group
        /// </summary>
        /// <param name="contractTypeGroup">Selected Contract Type Group</param>
        /// <param name="isScheduleProposal">Determines whether to include IDIQ options or not</param>
        /// <returns>Html for Contract Type options</returns>
        public string LoadFilteredContractTypes(string contractTypeGroup, bool? isScheduleProposal)
        {
            bool includeIDIQ = isScheduleProposal ?? true;
            return this.proposalLogic.GetContractTypesForContractTypeGroup(contractTypeGroup, includeIDIQ);
        }

        /// <summary>
        /// Displays the Proposal's Revision History
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Revision History Partial View</returns>
        public PartialViewResult DisplayRevisionHistory(int proposalId)
        {
            ICollection<RevisionHistoryModelView> model = this.proposalLogic.GetRevisionHistory(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_RevisionHistory, model);
        }
    }
}
