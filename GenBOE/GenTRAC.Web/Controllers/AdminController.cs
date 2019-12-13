// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.Admin;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Admin controller
    /// </summary>
    public class AdminController : GenTRACController
    {
        /// <summary>
        /// admin controller to use to execute testable controller logic
        /// </summary>
        private readonly AdminControllerLogic adminControllerLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inGenTRACControllerLogic">Controller Logic</param>
        /// <param name="inSiteMasterUtilities">Site Master Utilities</param>
        /// <param name="inAdminControllerLogic">admin controller logic class</param>
        /// <param name="inSecurityInformation">security information about user and their context</param>
        public AdminController(
            GenTRACControllerLogic inGenTRACControllerLogic,
            SiteMasterUtilities inSiteMasterUtilities,
            AdminControllerLogic inAdminControllerLogic,
            IES.Common.ISecurityInformation inSecurityInformation)
            : base( inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.adminControllerLogic = inAdminControllerLogic;
        }

        #region System Admin

        /// <summary>
        /// Display System Admin Permissions
        /// </summary>
        /// <returns>System Admin Permissions View</returns>
        public ViewResult DisplaySystemPermissions()
        {
            this.ViewData["currentUserId"] = this.adminControllerLogic.GetActiveUserId();

            ManagePermissionsModelView model = this.adminControllerLogic.GetManagePermissionsModelView();
            return this.View(WebConstants.View.ADMIN_SYSTEM_PERMISSIONS, model);
        }

        /// <summary>
        /// Display System Permissions Grid
        /// </summary>
        /// <param name="modelView">Current model view being viewed</param>
        /// <returns>The Grid</returns>
        public PartialViewResult DisplaySystemPermissionsGrid(PermissionsGridModelView modelView)
        {
            PermissionsGridModelView model = this.adminControllerLogic.GetManagePermissionsGridModelView(modelView);
            return this.PartialView(WebConstants.View.ADMIN_SYSTEM_PERMISSIONS_GRID, model);
        }

        /// <summary>
        /// Saves the System Permission roles for users
        /// </summary>
        /// <returns>JSON result</returns>
        /// <param name="permissionsToSave">System Permissions being saved</param>
        public JsonResult SaveSystemPermissionRoles(Collection<PermissionsModelView> permissionsToSave)
        {
            if (permissionsToSave == null || !permissionsToSave.Any())
            {
                throw new ArgumentNullException(nameof(permissionsToSave));
            }

            ICollection<ValidationMessage> validationErrors = this.adminControllerLogic.ValidatePermissionsToSave(permissionsToSave);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.adminControllerLogic.SaveUserPermissionRoles(permissionsToSave);
                scope.Complete();
            }

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Breaks down a group returning a list of users.
        /// </summary>
        /// <param name="groupId">group id</param>
        /// <returns>user list</returns>
        public ActionResult BreakdownGroup(int groupId)
        {
            return this.Content(this.adminControllerLogic.BreakdownGroup(groupId));
        }

        /// <summary>
        /// Display manage proposal info
        /// </summary>
        /// <returns>Display manage proposal info view</returns>
        public ViewResult DisplayManageProposalInfo()
        {
            return this.View(WebConstants.View.MANAGE_PROPOSAL_INFO);
        }

        /// <summary>
        /// Display manage proposal info details
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Display manage proposal info details view</returns>
        public ViewResult DisplayManageProposalInfoDetails(int proposalId)
        {
            ManageProposalInfoDetailsView manageProposalView = this.adminControllerLogic.GetManageProposalInfoDetailsView(proposalId);
            return this.View(WebConstants.View.MANAGE_PROPOSAL_INFO_DETAILS, manageProposalView);
        }

        /// <summary>
        /// Retrieve manage proposal tracking number
        /// </summary>
        /// <param name="trackingNumber">Tracking number</param>
        /// <returns>JSON result</returns>
        public JsonResult RetrieveManageProposalTrackingNumber(string trackingNumber)
        {
            int proposalId = this.adminControllerLogic.GetProposalIdByTrackingNumber(trackingNumber);
            if (proposalId < 0)
            {
                // proposal does not exist
                return this.Json(new { Result = "not_found" });
            }

            return this.Json(new { Result = proposalId.ToString() });
        }

        /// <summary>
        /// Save manage proposal details
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="manageProposalInfo">Manage proposal info</param>
        /// <returns>JSON result</returns>
        public JsonResult SaveManageProposalInfoDetails(int proposalId, ManageProposalInfoDetailsView manageProposalInfo)
        {
            if (manageProposalInfo == null)
            {
                throw new ArgumentNullException(nameof(manageProposalInfo));
            }

            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.adminControllerLogic.ValidateManageProposalInfo(manageProposalInfo, validationErrors);
            if (manageProposalInfo.NewStatus == ProposalStatus.Deleted)
            {
                this.adminControllerLogic.ValidateDeleteProposal(manageProposalInfo.ProposalID, validationErrors);
            }

            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.AdminControllerLogic.MANAGE_PROPOSAL_INFO_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                int? result = this.adminControllerLogic.SaveManageProposalInfo(proposalId, manageProposalInfo);
                if (result.HasValue && result.Value > 0)
                {
                    scope.Complete();
                    return this.Json(new { proposalId = result.Value.ToString() });
                }
            }

            return this.Json(new { Status = false });
        }

        /// <summary>
        /// Display Bulk Archive
        /// </summary>
        /// <returns>Display Bulk Archive view</returns>
        public ViewResult DisplayBulkArchive()
        {
            BulkArchiveModelView bulkArchiveModelView = this.adminControllerLogic.GetBulkArchiveModelView();

            return this.View(WebConstants.View.BULK_ARCHIVE, bulkArchiveModelView);
        }

        /// <summary>
        /// Get Program Areas for the given Line of Business. This method accepts
        /// "All" as a Line of Business, in which case "All" will be returned as the 
        /// only Program Area.
        /// </summary>
        /// <param name="lineOfBusiness">Line of Business or "All"</param>
        /// <returns>Corresponding Program Areas or "All"</returns>
        public string GetProgramAreasForLineOfBusinessForBulkArchive(string lineOfBusiness)
        {
            return this.adminControllerLogic.GetProgramAreasForLineOfBusinessForBulkArchive(lineOfBusiness);
        }

        /// <summary>
        /// Given a set of criteria, find out the number of affected proposals.
        /// </summary>
        /// <param name="bulkArchiveModelView">BulkArchiveModelView</param>
        /// <returns>The number of results that match the given data filters</returns>
        public int SearchBulkArchive(BulkArchiveModelView bulkArchiveModelView)
        {
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.adminControllerLogic.ValidateBulkArchiveRequest(bulkArchiveModelView, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.AdminControllerLogic.BULK_ARCHIVE_FORM);
            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }
            
            return this.adminControllerLogic.SearchBulkArchive(bulkArchiveModelView);
        }

        /// <summary>
        /// Given a set of criteria, bulk archive the number of affected proposals.
        /// </summary>
        /// <param name="bulkArchiveModelView">BulkArchiveModelView</param>
        /// <returns>The number of results that were archived</returns>
        public int ApplyBulkArchive(BulkArchiveModelView bulkArchiveModelView)
        {
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.adminControllerLogic.ValidateBulkArchiveRequest(bulkArchiveModelView, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.AdminControllerLogic.BULK_ARCHIVE_FORM);
            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            return this.adminControllerLogic.ApplyBulkArchive(bulkArchiveModelView);
        }
        #endregion
    }
}