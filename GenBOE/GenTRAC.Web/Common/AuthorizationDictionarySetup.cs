// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// This class generates a dictionary that will be used by the controllers to check access authorization
    /// for each controller's action
    /// </summary>
    internal static class AuthorizationDictionarySetup
    {
        #region Inner Workings

        /// <summary>
        /// Generates the dictionary for Controller security checks.
        /// Each controller should have a region that maps ALL of its actions to the Security Page and Security Authorization.
        /// </summary>
        /// <returns>Dictionary of all controllers, actions and page/security authorizations</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> GenerateDictionary()
        {
            Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result = new Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>>();

            AddAdminControllerActions(result);
            AddGenTRACControllerActions(result);
            AddHomeControllerActions(result);
            AddReportsControllerActions(result);
            AddProposalControllerActions(result);
            AddChecklistControllerActions(result);
            AddApprovalsControllerActions(result);
            AddPostSubmittalAttachmentsControllerActions(result);
            AddCertificationTimelineControllerActions(result);

            return result;
        }

        /// <summary>
        /// Adds each action into the controller dictionary
        /// </summary>
        /// <param name="action">Controller Action</param>
        /// <param name="page">Security Page</param>
        /// <param name="authorization">Security Authorization</param>
        /// <param name="controller">Controller Dictionary</param>
        private static void AddActionToController(string action, PtmSecurityPage page, SecurityAuthorization authorization, Dictionary<string, SecurityPageAndAuthorization> controller)
        {
            controller.Add(action.ToLower(), new SecurityPageAndAuthorization(page, authorization));
        }

        #endregion

        #region Controller Setup Methods

        /// <summary>
        /// Generates items for the Admin Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddAdminControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> adminControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.ACTIVE_DIRECTORY_SYNC, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.ADMIN_DISPLAY_SYSTEM_PERMISSIONS, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.ADMIN_DISPLAY_SYSTEM_PERMISSIONS_GRID, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.ADMIN_BREAKDOWN_GROUP, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.ADMIN_SAVE_SYSTEM_PERMISSIONS, PtmSecurityPage.Admin, SecurityAuthorization.CreateReadUpdateDelete, adminControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_MANAGE_PROPOSAL_INFO, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_MANAGE_PROPOSAL_INFO_DETAILS, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.RETRIEVE_MANAGE_PROPOSAL_TRACKING_NUMBER, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.SAVE_MANAGE_PROPOSAL_INFO_DETAILS, PtmSecurityPage.Admin, SecurityAuthorization.CreateReadUpdateDelete, adminControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_BULK_ARCHIVE, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.BULK_ARCHIVE_FILTER_PROGRAM_AREAS, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.SEARCH_BULK_ARCHIVE, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            AddActionToController(WebConstants.Action.APPLY_BULK_ARCHIVE, PtmSecurityPage.Admin, SecurityAuthorization.Read, adminControllerActions);
            
            result.Add(WebConstants.Controller.ADMIN, adminControllerActions);
        }

        /// <summary>
        /// Generates items for the GenTRAC Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddGenTRACControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> genTracControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.GENTRAC_DISPLAY_HOME_MENU, PtmSecurityPage.Home, SecurityAuthorization.Read, genTracControllerActions);
            AddActionToController(WebConstants.Action.ERROR, PtmSecurityPage.Home, SecurityAuthorization.Read, genTracControllerActions);
            AddActionToController(WebConstants.Action.WHOS_ONLINE, PtmSecurityPage.Home, SecurityAuthorization.Read, genTracControllerActions);

            result.Add(WebConstants.Controller.GEN_TRAC, genTracControllerActions);
        }

        /// <summary>
        /// Generates items for the Home Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddHomeControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> homeControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.HOME_DISPLAY_GENTRAC_HOME, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);   
            AddActionToController(WebConstants.Action.ACTIVE_DIRECTORY_SEARCH, PtmSecurityPage.Home, SecurityAuthorization.None, homeControllerActions);
            AddActionToController(WebConstants.Action.ACTIVE_DIRECTORY_SEARCH_USER_NAME, PtmSecurityPage.Home, SecurityAuthorization.None, homeControllerActions);
            AddActionToController(WebConstants.Action.HOME_DISPLAY_PROPOSAL_SEARCH_VALIDATION, PtmSecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, homeControllerActions);
            AddActionToController(WebConstants.Action.HOME_PROPOSAL_DISPLAY, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);
            AddActionToController(WebConstants.Action.HOME_PROPOSAL_GRID_DISPLAY, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);
            AddActionToController(WebConstants.Action.HOME_PROPOSAL_FILTERS_DISPLAY, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);
            AddActionToController(WebConstants.Action.GOTO_PROPOSAL, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);
            AddActionToController(WebConstants.Action.VIEW_EXPORT_PROPOSALS_REPORT, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_MY_APPROVALS, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);

            // the clear cache menu option will only be shown to SecurityPage.Admin, but if a non Admin was to type in the url, they should be allowed to do so
            AddActionToController(WebConstants.Action.HOME_CLEAR_CACHE, PtmSecurityPage.Home, SecurityAuthorization.Read, homeControllerActions);

            result.Add(WebConstants.Controller.HOME, homeControllerActions);
        }

        /// <summary>
        /// Generates items for the Reports Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddReportsControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> reportsControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.REPORTS_HOME, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.REPORTS_PROPOSAL_LOG, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.VIEW_PROPOSAL_LOG_REPORT, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_LOG_REPORT_PARAMETERS, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.REPORTS_PROPOSAL_ACTIVITY, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.VIEW_PROPOSAL_ACTIVITY_REPORT, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_ACTIVITY_REPORT_PARAMETERS, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.REPORTS_DFARS, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.VIEW_DFARS_REPORT, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_DFARS_REPORT_PARAMETERS, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);
            AddActionToController(WebConstants.Action.PROPOSAL_FILTER_PROGRAM_AREA, PtmSecurityPage.Reports, SecurityAuthorization.Read, reportsControllerActions);

            result.Add(WebConstants.Controller.REPORTS, reportsControllerActions);
        }

        /// <summary>
        /// Generates items for the Proposal Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddProposalControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> proposalControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_INDEX, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_INFORMATION, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_GENERAL_INFORMATION, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_APPROVALS, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_USER_INFORMATION, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.VALIDATE_PROPOSAL_INFORMATION, PtmSecurityPage.Proposal, SecurityAuthorization.ReadUpdate, proposalControllerActions);
            AddActionToController(WebConstants.Action.SAVE_PROPOSAL_INFORMATION, PtmSecurityPage.Proposal, SecurityAuthorization.ReadUpdate, proposalControllerActions);
            AddActionToController(WebConstants.Action.DELETE_PROPOSAL, PtmSecurityPage.Proposal, SecurityAuthorization.CreateReadUpdateDelete, proposalControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_PROPOSAL_DETAILS, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.PROPOSAL_FILTER_PROGRAM_AREA, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            AddActionToController(WebConstants.Action.PROPOSAL_FILTER_CONTRACT_TYPES, PtmSecurityPage.Proposal, SecurityAuthorization.Read, proposalControllerActions);
            result.Add(WebConstants.Controller.PROPOSAL, proposalControllerActions);
        }

        /// <summary>
        /// Generates items for the Checklist Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddChecklistControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> checklistControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController(WebConstants.Action.DISPLAY_CHECKLIST_INDEX, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_CHECKLIST_GENERAL_INFORMATION, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_CHECKLIST_PROPOSAL_PRICING_DATA, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_CHECKLIST_PPR_DOCUMENT, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_CHECKLIST_PAR_DOCUMENT, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.SAVE_CHECKLIST_INFORMATION, PtmSecurityPage.Checklist, SecurityAuthorization.ReadUpdate, checklistControllerActions);
            AddActionToController(WebConstants.Action.VALIDATE_CHECKLIST_INFORMATION, PtmSecurityPage.Checklist, SecurityAuthorization.Read, checklistControllerActions);
            AddActionToController(WebConstants.Action.EXPORT_PAR_CHECKLIST, PtmSecurityPage.ChecklistReport, SecurityAuthorization.Read, checklistControllerActions);
            result.Add(WebConstants.Controller.CHECKLIST, checklistControllerActions);
        }

        /// <summary>
        /// Generates items for the Approvals Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddApprovalsControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> approvalsControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();
            AddActionToController(WebConstants.Action.DISPLAY_APPROVALS_INDEX, PtmSecurityPage.Approvals, SecurityAuthorization.Read, approvalsControllerActions);
            AddActionToController(WebConstants.Action.DISPLAY_APPROVER_SECTION, PtmSecurityPage.Approvals, SecurityAuthorization.Read, approvalsControllerActions);
            AddActionToController(WebConstants.Action.SAVE_APPROVAL, PtmSecurityPage.Approvals, SecurityAuthorization.ReadUpdate, approvalsControllerActions);
            AddActionToController(WebConstants.Action.RESET_WORKFLOW, PtmSecurityPage.Proposal, SecurityAuthorization.ReadUpdate, approvalsControllerActions);
            AddActionToController(WebConstants.Action.VALIDATE_COVER_SHEET_APPROVER, PtmSecurityPage.Approvals, SecurityAuthorization.Read, approvalsControllerActions);
            result.Add(WebConstants.Controller.APPROVALS, approvalsControllerActions);
        }

        /// <summary>
        /// Generates items for the Post Submittal Attachments Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddPostSubmittalAttachmentsControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> postSubmittalAttachmentsActions = new Dictionary<string, SecurityPageAndAuthorization>();
            AddActionToController(WebConstants.Action.DISPLAY_POST_SUBMITTAL_ATTACHMENTS, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.Read, postSubmittalAttachmentsActions);
            AddActionToController(WebConstants.Action.UPLOAD_ATTACHMENT, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.CreateReadUpdateDelete, postSubmittalAttachmentsActions);
            AddActionToController(WebConstants.Action.DOWNLOAD_ATTACHMENT, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.Read, postSubmittalAttachmentsActions);
            AddActionToController(WebConstants.Action.DELETE_ATTACHMENT, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.CreateReadUpdateDelete, postSubmittalAttachmentsActions);
            AddActionToController(WebConstants.Action.VALIDATE_REQUIRED_ATTACHMENTS, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.CreateReadUpdateDelete, postSubmittalAttachmentsActions);
            result.Add(WebConstants.Controller.PSA, postSubmittalAttachmentsActions);
        }

        /// <summary>
        /// Generates items for the Certification Timeline Controller.
        /// </summary>
        /// <param name="result">The result.</param>
        private static void AddCertificationTimelineControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> certificationTimelineActions = new Dictionary<string, SecurityPageAndAuthorization>();
            AddActionToController(WebConstants.Action.DISPLAY_CERTIFICATION_TIMELINE, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.Read, certificationTimelineActions);
            AddActionToController(WebConstants.Action.SAVE_CERTIFICATION_TIMELINE, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.CreateReadUpdateDelete, certificationTimelineActions);
            AddActionToController(WebConstants.Action.COMPLETE_CERTIFICATION_TIMELINE, PtmSecurityPage.PostSubmittalAttachments, SecurityAuthorization.CreateReadUpdateDelete, certificationTimelineActions);
            result.Add(WebConstants.Controller.CERTIFICATION_TIMELINE, certificationTimelineActions);
        }

        #endregion
    }
}