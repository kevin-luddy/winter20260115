// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using ActionLogic.ModelView.Admin;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;

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
        /// <returns>create new proposal view</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "model")]
        public ViewResult DisplayProposalIndex(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalIndexModelView model = this.proposalLogic.GetDataForProposalIndex(proposalId);
            return this.View(WebConstants.View.PROPOSAL_INDEX, model);
        }

        /// <summary>
        /// Display proposal details tabs
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>create new proposal view</returns>
        public ActionResult DisplayProposalDetails(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalIndexModelView model = this.proposalLogic.GetDataForProposalIndex(proposalId);
            return this.View(WebConstants.View.PROPOSAL_DETAILS, model); // this is for the tabs
        }

        /// <summary>
        /// Display proposal information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>proposal information view</returns>
        public PartialViewResult DisplayProposalInformation(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalInformationModelView model = this.proposalLogic.GetDataForProposalInformation(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_INFORMATION, model);
        }

        /// <summary>
        /// Display proposal general information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>proposal general information view</returns>
        public PartialViewResult DisplayProposalGeneralInformation(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalGeneralInformationModelView model = this.proposalLogic.GetDataForProposalGeneralInformation(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_GENERAL_INFORMATION, model);
        }

        /// <summary>
        /// Display proposal user information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>proposal user information view</returns>
        public PartialViewResult DisplayProposalApprovals(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalApprovalsModelView model = this.proposalLogic.GetDataForProposalApprovals(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_APPROVALS, model);
        }

        /// <summary>
        /// Display proposal user information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>proposal user information view</returns>
        public PartialViewResult DisplayProposalUserInformation(int? proposalId)
        {
            if (proposalId.HasValue)
            {
                this.ViewBag.proposalid = proposalId.Value.ToString();
            }
            else
            {
                this.ViewBag.proposalid = "null";
            }

            ProposalUserInformationModelView model = this.proposalLogic.GetDataForProposalUserInformation(proposalId);
            return this.PartialView(WebConstants.View.PROPOSAL_USER_INFORMATION, model);
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
            this.proposalLogic.ValidateGeneralInfoTypes(proposalGeneralInfo, validationErrors, isForecasted);

            bool invalidUnsavedUsers = false;
            if (!isForecasted)
            {
                invalidUnsavedUsers = this.proposalLogic.ValidateUserTypes(proposalId, proposalApprovalsInfo, proposalUserInfo, validationErrors);
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
        /// <returns>true if success else false</returns>
        public JsonResult SaveProposal(int? proposalId, ProposalInformationModelView proposalInfo, ProposalGeneralInformationModelView proposalGeneralInfo,
            ProposalApprovalsModelView proposalApprovalsInfo, ProposalUserInformationModelView proposalUserInfo)
        {
            if (proposalGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(proposalGeneralInfo));
            }

            if (proposalApprovalsInfo == null)
            {
                throw new ArgumentNullException(nameof(proposalApprovalsInfo));
            }

            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            bool invalidUnsavedUsers = this.ValidateProposalFields(proposalId, proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo, validationErrors);

            ProposalGeneralInformationModelView originalProposalInfo = this.proposalLogic.GetDataForProposalGeneralInformation(proposalId);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                proposalId = this.proposalLogic.SaveProposal(proposalInfo, proposalGeneralInfo, proposalApprovalsInfo, proposalUserInfo);

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

        #region Proposal Revisions

        /// <summary>
        /// Add a new revision for a Proposal
        /// </summary>
        /// <param name="proposalId">ID of Proposal</param>
        /// <returns>Json result</returns>
        public JsonResult SaveNewProposalRevision(int proposalId)
        {
            ProposalDto proposal = this.proposalLogic.GetByProposalId(proposalId);

            this.proposalLogic.ValidateSaveNewRevision(proposal);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.proposalLogic.SetProposalRevised(proposal.Id, proposal.UpdateDate);

                // TODO - BOEJ-4636 - create new revision
                int revisionId = -1;
                scope.Complete();
                return this.Json(new { revisionId = revisionId });
            }
        }

        #endregion
    }
}
