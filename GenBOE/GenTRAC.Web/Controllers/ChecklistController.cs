// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.Web.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Checklist Controller
    /// </summary>
    public class ChecklistController : GenTRACController
    {
        /// <summary>
        /// Checklist Controller Logic
        /// </summary>
        private ChecklistControllerLogic checklistLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inGenTRACControllerLogic">GenTRAC Controller Logic</param>
        /// <param name="inSiteMasterUtilities">Site Master Utilities</param>
        /// <param name="inChecklistControllerLogic">checklist controller logic</param>
        /// <param name="inSecurityInformation">security information about user and their context</param>
        public ChecklistController(
            GenTRACControllerLogic inGenTRACControllerLogic,
            SiteMasterUtilities inSiteMasterUtilities,
            ChecklistControllerLogic inChecklistControllerLogic,
            IES.Common.ISecurityInformation inSecurityInformation)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.checklistLogic = inChecklistControllerLogic;
        }

        /// <summary>
        /// Display checklist index
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>create new checklist view</returns>
        public ViewResult DisplayChecklistIndex(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            ChecklistIndexModelView model = this.checklistLogic.GetChecklistVersion(proposalId);
            return this.View(WebConstants.View.CHECKLIST_INDEX, model);
        }

        /// <summary>
        /// Display checklist general information
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>checklist general information view</returns>
        public PartialViewResult DisplayChecklistGeneralInformation(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            ChecklistGeneralInformationModelView model = this.checklistLogic.GetDataForChecklistGeneralInformation(proposalId);

            // This is no longer the case. Leaving this code here for now, in case the customer changes their mind. if it's after 6/2019 and you see this, delete it.. :)
            // Certain special fields need to remain editable even if the proposal is locked if the user is a Lead or Backup Estimator.
            this.ViewBag.KeepSpecialFieldsEditable = "false"; // this.checklistLogic.IsCurrentUserPricerOrBackupEstimator(proposalId).ToString().ToLower();
            return this.PartialView(WebConstants.View.CHECKLIST_GENERAL_INFORMATION, model);
        }

        /// <summary>
        /// Display checklist proposal pricing data
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>checklist proposal pricing data view</returns>
        public PartialViewResult DisplayChecklistProposalPricingData(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            ChecklistProposalPricingDataModelView model = this.checklistLogic.GetDataForChecklistProposalPricingData(proposalId);
            return this.PartialView(WebConstants.View.CHECKLIST_PROPOSAL_PRICING_DATA, model);
        }

        /// <summary>
        /// Display checklist PPR document
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>checklist PPR document data view</returns>
        public PartialViewResult DisplayChecklistPPRDocument(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            ChecklistProposalPricingReviewDocumentModelView model = this.checklistLogic.GetDataForChecklistPPRDocument(proposalId);
            return this.PartialView(WebConstants.View.CHECKLIST_PPR_DOCUMENT, model);
        }

        /// <summary>
        /// Display checklist PAR document
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>checklist PAR document data view</returns>
        public PartialViewResult DisplayChecklistPARDocument(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();
            ChecklistProposalAdequacyReviewDocumentModelView model = this.checklistLogic.GetDataForChecklistPARDocument(proposalId, SiteMasterUtilities.BaseUrlForInstructionLocation);
            return this.PartialView(WebConstants.View.CHECKLIST_PAR_DOCUMENT, model);
        }

        /// <summary>
        /// Save the checklist
        /// </summary>
        /// <param name="proposalId">proposal ID used for security</param>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data</param>
        /// <param name="checklistPPRDocumentData">PPR document data</param>
        /// <param name="checklistPARDocumentData">PAR document data</param>
        /// <returns>true if succesful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult SaveChecklist(int proposalId, ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;

                // Validate the PARs visibility, if there are validation errors do not continue.
                if (this.checklistLogic.ValidatePARVisibility(checklistGeneralInfo, validationErrors))
                {
                    validationErrors.ForEach(x => x.FormIDToTarget = ChecklistControllerLogic.CHECKLIST_GENERAL_INFO_FORM);
                    throw new ValidationException(validationErrors);
                }

                this.checklistLogic.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRDocumentData, checklistPARDocumentData, false, validationErrors);
                validationErrors.ForEach(x => x.FormIDToTarget = ChecklistControllerLogic.CHECKLIST_GENERAL_INFO_FORM);

                if (validationErrors.Any())
                {
                    throw new ValidationException(validationErrors);
                }

                // check if the pricer has saved since the peer opened the checklist. If so, add something to the Json result so we can display
                // an alert to the user, but continue with the save.
                bool pricerSavedWhilePeerEditing = this.checklistLogic.HasPricerSavedSincePeerReviewOpenedChecklist(checklistGeneralInfo, checklistPARDocumentData);

                int? checklistID = this.checklistLogic.SaveChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRDocumentData, checklistPARDocumentData, false);

                if (checklistID.HasValue)
                {
                    scope.Complete();
                    if (pricerSavedWhilePeerEditing)
                    {
                        return this.Json(new { Status = true, message = "PricerSaved" });
                    }
                    else
                    {
                        return this.Json(new { Status = true });
                    }
                }
            }

            return this.Json(new { Status = false });
        }

        /// <summary>
        /// Validate the checklist page. This action is called from either the Checklist or Approvals tab.  
        /// Note: When called from the Approvals tab, the checklist objects will be null, so data will be retrieved from the DB.
        /// </summary>
        /// <param name="proposalId">proposal ID used for security</param>
        /// <param name="checklistGeneralInfo">checklist general information (or null if called from Approvals tab)</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data (or null if called from Approvals tab)</param>
        /// <param name="checklistPPRDocumentData">PPR document data (or null if called from Approvals tab)</param>
        /// <param name="checklistPARDocumentData">PAR document data (or null if called from Approvals tab)</param>
        /// <returns>true if succesful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult ValidateChecklistInformation(int proposalId, ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData)
        {
            string formIDToTarget = ChecklistControllerLogic.CHECKLIST_GENERAL_INFO_FORM;
            if (checklistGeneralInfo == null && checklistProposalPricingData == null && checklistPPRDocumentData == null && checklistPARDocumentData == null)
            {
                formIDToTarget = ApprovalsControllerLogic.APPROVAL_INDEX_FORM;
                checklistGeneralInfo = this.checklistLogic.GetDataForChecklistGeneralInformation(proposalId);
                checklistProposalPricingData = this.checklistLogic.GetDataForChecklistProposalPricingData(proposalId);
                checklistPPRDocumentData = this.checklistLogic.GetDataForChecklistPPRDocument(proposalId);
                checklistPARDocumentData = this.checklistLogic.GetDataForChecklistPARDocument(proposalId, SiteMasterUtilities.BaseUrlForInstructionLocation);
            }

            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;

            // Validate the PARs visibility, if there are validation errors do not continue.
            if (this.checklistLogic.ValidatePARVisibility(checklistGeneralInfo, validationErrors))
            {
                validationErrors.ForEach(x => x.FormIDToTarget = formIDToTarget);
                throw new ValidationException(validationErrors);
            }

            this.checklistLogic.ValidateChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRDocumentData, checklistPARDocumentData, true, validationErrors); // do full validation as if we were submitting the form (isSubmit=true)
            validationErrors.ForEach(x => x.FormIDToTarget = formIDToTarget);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            return this.Json(new { Status = true });    // success - no validation errors found
        }

        /// <summary>
        /// Validate the PAR checklist and generate the SSRS report to display the checklist
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data</param>
        /// <param name="checklistPPRDocumentData">PPR document data</param>
        /// <param name="checklistPARDocumentData">PAR document data</param>
        /// <param name="reportParameters">Parameters used to generate the Export report</param>
        /// <returns>JSON Result</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult ExportPARChecklist(int proposalId, ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, PARChecklistModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            // Perform validation before generating export
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;

            this.checklistLogic.ValidatePARChecklist(checklistGeneralInfo, checklistProposalPricingData, checklistPPRDocumentData, checklistPARDocumentData, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = ChecklistControllerLogic.CHECKLIST_GENERAL_INFO_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            Uri reportUri = this.checklistLogic.PopulatePARChecklistSSRSParameters(reportParameters);
            return this.Json(new { Status = true, Url = reportUri });
        }
    }
}
