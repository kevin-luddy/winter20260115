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
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// ApprovalsController
    /// </summary>
    public class ApprovalsController : GenTRACController
    {
        /// <summary>
        /// ApprovalsControllerLogic
        /// </summary>
        private readonly ApprovalsControllerLogic approvalsLogic = null;
      
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inGenTRACControllerLogic">inGenTRACControllerLogic</param>
        /// <param name="inSiteMasterUtilities">inSiteMasterUtilities</param>
        /// <param name="inApprovalsControllerLogic">inApprovalsControllerLogic</param>
        /// <param name="inSecurityInformation">inSecurityInformation</param>
        public ApprovalsController(
            GenTRACControllerLogic inGenTRACControllerLogic,
            SiteMasterUtilities inSiteMasterUtilities,
            ApprovalsControllerLogic inApprovalsControllerLogic,
            IES.Common.ISecurityInformation inSecurityInformation)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.approvalsLogic = inApprovalsControllerLogic;
        }

        /// <summary>
        /// Display checklist index
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>create new proposal approval view</returns>
        public ViewResult DisplayApprovalsIndex(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            this.ViewBag.IsProposalDeletedOrArchived = this.approvalsLogic.IsProposalDeletedOrArchived(proposalId);
            ApprovalsIndexModelView model = this.approvalsLogic.GetApprovals(proposalId);
            return this.View(WebConstants.View.APPROVALS_INDEX, model);
        }
        
        /// <summary>
        /// Display the Approver section for the given role
        /// </summary>
        /// <param name="proposalId">ID of the proposal</param>
        /// <param name="role">role of the approver</param>
        /// <returns>Approver section for the given role</returns>
        public ViewResult DisplayApproverSection(int proposalId, PtmRole role)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            this.ViewBag.IsProposalDeletedOrArchived = this.approvalsLogic.IsProposalDeletedOrArchived(proposalId);
            ApprovalSectionModelView model = this.approvalsLogic.GetApprovalModel(proposalId, role);
            if (model != null)
            {
                // Add extra \ to any newlines, otherwise comments with multiple lines will break
                model.Comments = model.Comments.Replace("\n", "\\n");
                model.AdditionalEmailText = model.AdditionalEmailText.Replace("\n", "\\n");

                return this.View(WebConstants.View.APPROVER_SECTION, model);
            }
            else
            {
                // If model is null (most likely due to unrequired and not-set Independent Reviewer/Cover Sheet Approver or section being unavailable
                // due not being at the appropriate workflow step), return null so section is not displayed
                return null;
            }
        }

        /// <summary>
        /// Validates that the Cover Sheet Approver is still approved
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        /// <returns>True, if successful.</returns>
        public JsonResult ValidateCoverSheetApprover(int proposalId)
        {
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;

            this.approvalsLogic.ValidateCoverSheetApprover(proposalId, validationErrors);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            return this.Json(new { Status = true });    // Success - no validation errors found.
        }

        /// <summary>
        /// Saves approval for the given role
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="additionalEmailText">Additional approval email text.</param>
        /// <param name="comment">Approval comment</param>
        /// <param name="role">Role submitting approval</param>
        /// <returns>json result</returns>
        public JsonResult SaveApproval(int proposalId, string additionalEmailText, string comment, PtmRole role)
        {
            bool pricerSavedWhilePeerEditing = false;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.approvalsLogic.SaveApproval(proposalId, role, comment, additionalEmailText, SiteMasterUtilities.BaseUrlForInstructionLocation, out pricerSavedWhilePeerEditing);
                scope.Complete();
            }

            this.approvalsLogic.SendApprovalEMail(proposalId, role);
            return this.Json(new { Success = true, message = pricerSavedWhilePeerEditing ? "PricerSaved" : string.Empty });
        }

        /// <summary>
        /// Resets the workflow.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <returns>json result.</returns>
        public JsonResult ResetWorkflow(int proposalId)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.approvalsLogic.ResetWorkflow(proposalId);
                scope.Complete();
            }

            return this.Json(new { Success = true });
        }
    }
}
