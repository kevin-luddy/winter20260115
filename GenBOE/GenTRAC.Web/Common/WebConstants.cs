// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    /// <summary>
    /// All constants used throughout the web project
    /// </summary>
    public static class WebConstants
    {
        #region Route
        /// <summary>
        /// Route Constants
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Route
        {
            /// <summary>
            /// Home route
            /// </summary>
            public const string HOME_ROUTE = "home/{action}";

            /// <summary>
            /// GenTRAC base route
            /// </summary>
            public const string GENTRAC_BASE_ROUTE = "gentrac/{action}";

            /// <summary>
            /// Admin route
            /// </summary>
            public const string ADMIN_ROUTE = "admin/{action}";

            /// <summary>
            /// Proposal route
            /// </summary>
            public const string PROPOSAL_ROUTE = "proposal/{action}";

            /// <summary>
            /// Proposal id route
            /// </summary>
            public const string PROPOSAL_ID_ROUTE = "{controller}/{action}/id/{proposalid}";

            /// <summary>
            /// checklist route
            /// </summary>
            public const string CHECKLIST_ROUTE = "checklist/{action}";

            /// <summary>
            /// reports route
            /// </summary>
            public const string REPORTS_ROUTE = "reports/{action}";

            /// <summary>
            /// error route
            /// </summary>
            public const string ERROR_ROUTE = "{action}";
           
            /// <summary>
            /// Name
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
            public static class Name
            {
                /// <summary>
                /// GenTRAC base
                /// </summary>
                public const string GENTRAC_BASE = "Gentrac";

                /// <summary>
                /// Admin
                /// </summary>
                public const string ADMIN = "Admin";

                /// <summary>
                /// Home
                /// </summary>
                public const string HOME = "Home";

                /// <summary>
                /// Proposal
                /// </summary>
                public const string PROPOSAL = "Proposal";

                /// <summary>
                /// Proposal id
                /// </summary>
                public const string PROPOSAL_ID = "ProposalId";

                /// <summary>
                /// Checklist
                /// </summary>
                public const string CHECKLIST = "Checklist";

                /// <summary>
                /// Approvals
                /// </summary>
                public const string APPROVALS = "Approvals";

                /// <summary>
                /// Reports
                /// </summary>
                public const string REPORTS = "Reports";

                /// <summary>
                /// Error
                /// </summary>
                public const string ERROR = "Error";
            }
        }
        #endregion Route

        #region RequestKey
        /// <summary>
        /// Constants for Request Variables
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class RequestKey
        {
            /// <summary>
            /// Proposal ID
            /// </summary>
            public const string PROPOSAL_ID = "ProposalID";
        }
        #endregion RequestKey

        #region Action
        /// <summary>
        /// Constants for Actions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Action
        {
            /// <summary>
            /// Index
            /// </summary>
            public const string INDEX = "index";

            /// <summary>
            /// GenTRAC display home menu
            /// </summary>
            public const string GENTRAC_DISPLAY_HOME_MENU = "displayhomemenu";

            /// <summary>
            /// Active Directory search
            /// </summary>
            public const string ACTIVE_DIRECTORY_SEARCH = "Search";

            /// <summary>
            /// Active Directory search user name
            /// </summary>
            public const string ACTIVE_DIRECTORY_SEARCH_USER_NAME = "SearchUserName";

            /// <summary>
            /// Active Directory sync
            /// </summary>
            public const string ACTIVE_DIRECTORY_SYNC = "ActiveDirectorySync";

            /// <summary>
            /// Error
            /// </summary>
            public const string ERROR = "Error";

            /// <summary>
            /// Display Who's Online Popup
            /// </summary>
            public const string WHOS_ONLINE = "DisplayWhosOnline";
            
            #region Admin

            /// <summary>
            /// Admin breakdown group
            /// </summary>
            public const string ADMIN_BREAKDOWN_GROUP = "BreakdownGroup";

            /// <summary>
            /// Admin display system permissions grid
            /// </summary>
            public const string ADMIN_DISPLAY_SYSTEM_PERMISSIONS_GRID = "DisplaySystemPermissionsGrid";

            /// <summary>
            /// Admin display system permissions
            /// </summary>
            public const string ADMIN_DISPLAY_SYSTEM_PERMISSIONS = "DisplaySystemPermissions";

            /// <summary>
            /// Admin save system permissions
            /// </summary>
            public const string ADMIN_SAVE_SYSTEM_PERMISSIONS = "SaveSystemPermissionRoles";

            /// <summary>
            /// Display manage proposal info
            /// </summary>
            public const string DISPLAY_MANAGE_PROPOSAL_INFO = "DisplayManageProposalInfo";

            /// <summary>
            /// Display manage proposal info details
            /// </summary>
            public const string DISPLAY_MANAGE_PROPOSAL_INFO_DETAILS = "DisplayManageProposalInfoDetails";

            /// <summary>
            /// Retrieve manage proposal info
            /// </summary>
            public const string RETRIEVE_MANAGE_PROPOSAL_TRACKING_NUMBER = "RetrieveManageProposalTrackingNumber";

            /// <summary>
            /// Save manage proposal info details
            /// </summary>
            public const string SAVE_MANAGE_PROPOSAL_INFO_DETAILS = "SaveManageProposalInfoDetails";

            /// <summary>
            /// Display the Bulk Archive View
            /// </summary>
            public const string DISPLAY_BULK_ARCHIVE = "DisplayBulkArchive";

            /// <summary>
            /// Display Program Areas for the Bulk Archive View
            /// </summary>
            public const string BULK_ARCHIVE_FILTER_PROGRAM_AREAS = "GetProgramAreasForLineOfBusinessForBulkArchive";

            /// <summary>
            /// Search Bulk Archive
            /// </summary>
            public const string SEARCH_BULK_ARCHIVE = "SearchBulkArchive";

            /// <summary>
            /// Apply Bulk Archive, i.e. actually perform the bulk archive
            /// </summary>
            public const string APPLY_BULK_ARCHIVE = "ApplyBulkArchive";

            #endregion

            #region Home

            /// <summary>
            /// Home display GenTRAC home
            /// </summary>
            public const string HOME_DISPLAY_GENTRAC_HOME = "displaygentrachome";

            /// <summary>
            /// Home display proposal search validation
            /// </summary>
            public const string HOME_DISPLAY_PROPOSAL_SEARCH_VALIDATION = "ValidateProposalSearchResult";

            /// <summary>
            /// Home My proposals view
            /// </summary>
            public const string HOME_PROPOSAL_DISPLAY = "DisplayHomeProposal";

            /// <summary>
            /// Home My Proposals grid
            /// </summary>
            public const string HOME_PROPOSAL_GRID_DISPLAY = "DisplayHomeProposalGrid";

            /// <summary>
            /// Home My Proposals grid
            /// </summary>
            public const string HOME_PROPOSAL_FILTERS_DISPLAY = "DisplayHomeProposalFilters";

            /// <summary>
            /// SSRS export proposals from genTrac home page
            /// </summary>
            public const string VIEW_EXPORT_PROPOSALS_REPORT = "ViewExportProposalsReport";

            /// <summary>
            /// Go To Proposal
            /// </summary>
            public const string GOTO_PROPOSAL = "GoToProposal";

            /// <summary>
            /// Home clear cache
            /// </summary>
            public const string HOME_CLEAR_CACHE = "ClearCache";

            /// <summary>
            /// Display My Approvals
            /// </summary>
            public const string DISPLAY_MY_APPROVALS = "DisplayMyApprovals";

            #endregion Home

            #region Proposal

            /// <summary>
            /// Create new proposal
            /// </summary>
            public const string DISPLAY_PROPOSAL_INDEX = "DisplayProposalIndex";

            /// <summary>
            /// Display proposal information
            /// </summary>
            public const string DISPLAY_PROPOSAL_INFORMATION = "DisplayProposalInformation";

            /// <summary>
            /// Display proposal general information
            /// </summary>
            public const string DISPLAY_PROPOSAL_GENERAL_INFORMATION = "DisplayProposalGeneralInformation";

            /// <summary>
            /// Display proposal approvals
            /// </summary>
            public const string DISPLAY_PROPOSAL_APPROVALS = "DisplayProposalApprovals";

            /// <summary>
            /// Display proposal user information
            /// </summary>
            public const string DISPLAY_PROPOSAL_USER_INFORMATION = "DisplayProposalUserInformation";

            /// <summary>
            /// Display proposal comments
            /// </summary>
            public const string DISPLAY_PROPOSAL_COMMENTS = "DisplayProposalComments";

            /// <summary>
            /// Validates the proposal
            /// </summary>
            public const string VALIDATE_PROPOSAL_INFORMATION = "ValidateProposal";

            /// <summary>
            /// Saves the proposal
            /// </summary>
            public const string SAVE_PROPOSAL_INFORMATION = "SaveProposal";

            /// <summary>
            /// Saves only the comments for a read-only proposal
            /// </summary>
            public const string SAVE_PROPOSAL_COMMENTS = "SaveProposalComments";

            /// <summary>
            /// Displays the proposal details, the tab form that shows the proposal, checklist, and revisions
            /// </summary>
            public const string DISPLAY_PROPOSAL_DETAILS = "DisplayProposalDetails"; 

            /// <summary>
            /// Displays the proposal details for creating a Revision
            /// </summary>
            public const string DISPLAY_PROPOSAL_REVISION_DETAILS = "DisplayProposalRevisionDetails";

            /// <summary>
            /// Validate the proposal details for creating a Revision
            /// </summary>
            public const string VALIDATE_PROPOSAL_REVISION_DETAILS = "ValidateNewProposalRevision";             

            /// <summary>
            /// Filter Program Areas based on selected Line of Business
            /// </summary>
            public const string PROPOSAL_FILTER_PROGRAM_AREA = "LoadFilteredProgramAreas";

            /// <summary>
            /// Filter Contract Types based on selected Contract Type Group
            /// </summary>
            public const string PROPOSAL_FILTER_CONTRACT_TYPES = "LoadFilteredContractTypes";

            /// <summary>
            /// Marks the proposal for deletion
            /// </summary>
            public const string DELETE_PROPOSAL = "DeleteProposal";

            /// <summary>
            /// Displays Revision History Tab
            /// </summary>
            public const string DISPLAY_REVISION_HISTORY = "DisplayRevisionHistory";

            /// <summary>
            /// Reverts the Proposal to the prior version
            /// </summary>
            public const string REVERT_PROPOSAL_TO_PRIOR_VERSION = "RevertProposalToPriorVersion";

            #endregion Proposal

            #region Permissions

            #endregion Permissions

            #region Reports

            /// <summary>
            /// Reports home
            /// </summary>
            public const string REPORTS_HOME = "DisplayReportsHome";

            /// <summary>
            /// Proposal Log Reports
            /// </summary>
            public const string REPORTS_PROPOSAL_LOG = "DisplayProposalLog";

            /// <summary>
            /// view proposal log report
            /// </summary>
            public const string VIEW_PROPOSAL_LOG_REPORT = "ViewProposalLogReport";

            /// <summary>
            /// proposal log parameters
            /// </summary>
            public const string DISPLAY_PROPOSAL_LOG_REPORT_PARAMETERS = "DisplayProposalLogReportParameters";

            /// <summary>
            /// Proposal Activity Reports
            /// </summary>
            public const string REPORTS_PROPOSAL_ACTIVITY = "DisplayProposalActivity";

            /// <summary>
            /// view proposal activity report
            /// </summary>
            public const string VIEW_PROPOSAL_ACTIVITY_REPORT = "ViewProposalActivityReport";

            /// <summary>
            /// proposal activity parameters
            /// </summary>
            public const string DISPLAY_PROPOSAL_ACTIVITY_REPORT_PARAMETERS = "DisplayProposalActivityReportParameters";

            /// <summary>
            /// DFARS Reports
            /// </summary>
            public const string REPORTS_DFARS = "DisplayDfars";

            /// <summary>
            /// View DFARS Report
            /// </summary>
            public const string VIEW_DFARS_REPORT = "ViewDfarsReport";

            /// <summary>
            /// DFARS Report parameters
            /// </summary>
            public const string DISPLAY_DFARS_REPORT_PARAMETERS = "DisplayDfarsReportParameters";

            #endregion

            #region Contracts

            /// <summary>
            /// Display Contracts
            /// </summary>
            public const string DISPLAY_CONTRACTS_INDEX = "DisplayContractsIndex";

            /// <summary>
            /// Display Contracts
            /// </summary>
            public const string CALCULATE_OFFER_FIELDS = "CalculateOfferFields";

            /// <summary>
            /// Display Contracts
            /// </summary>
            public const string GET_SELECTED_ROM_DATA = "GetPreviouslySelectedRomData";

            #endregion

            #region Approvals
            /// <summary>
            /// approvals
            /// </summary>
            public const string DISPLAY_APPROVALS_INDEX = "DisplayApprovalsIndex";

            /// <summary>
            /// Display the approval section of the Approvals tab
            /// </summary>
            public const string DISPLAY_APPROVER_SECTION = "DisplayApproverSection";
            
            /// <summary>
            /// Save approval
            /// </summary>
            public const string SAVE_APPROVAL = "SaveApproval";

            /// <summary>
            /// Resets the workflow for a proposal.
            /// </summary>
            public const string RESET_WORKFLOW = "ResetWorkflow";

            /// <summary>
            /// Sets the Proposal as No Bid
            /// </summary>
            public const string SET_NO_BID = "SetProposalAsNoBid";

            /// <summary>
            /// Reverts the Proposal No Bid status
            /// </summary>
            public const string REVERT_NO_BID = "RevertProposalNoBidStatus";

            #endregion

            #region Checklist

            /// <summary>
            /// checklist
            /// </summary>
            public const string DISPLAY_CHECKLIST_INDEX = "DisplayChecklistIndex";

            /// <summary>
            /// Display checklist general information
            /// </summary>
            public const string DISPLAY_CHECKLIST_GENERAL_INFORMATION = "DisplayChecklistGeneralInformation";

            /// <summary>
            /// Display checklist general information
            /// </summary>
            public const string DISPLAY_CHECKLIST_PROPOSAL_PRICING_DATA = "DisplayChecklistProposalPricingData";

            /// <summary>
            /// Display checklist PPR Document
            /// </summary>
            public const string DISPLAY_CHECKLIST_PPR_DOCUMENT = "DisplayChecklistPPRDocument";

            /// <summary>
            /// Display checklist PAR Document
            /// </summary>
            public const string DISPLAY_CHECKLIST_PAR_DOCUMENT = "DisplayChecklistPARDocument";

            /// <summary>
            /// Saves the checklist
            /// </summary>
            public const string SAVE_CHECKLIST_INFORMATION = "SaveChecklist";

            /// <summary>
            /// Validates the general checklist page information as well as the PPR and PAR checklists
            /// </summary>
            public const string VALIDATE_CHECKLIST_INFORMATION = "ValidateChecklistInformation";

            /// <summary>
            /// Validates the Cover Sheet Approver is still in the appropriate AD group
            /// </summary>
            public const string VALIDATE_COVER_SHEET_APPROVER = "ValidateCoverSheetApprover";
            
            /// <summary>
            /// Export the checklist
            /// </summary>
            public const string EXPORT_PAR_CHECKLIST = "ExportPARChecklist";

            #endregion Checklist

            #region Post Submittal Attachments

            /// <summary>
            /// Display Post Submittal Attachments
            /// </summary>
            public const string DISPLAY_POST_SUBMITTAL_ATTACHMENTS = "DisplayPostSubmittalAttachments";

            /// <summary>
            /// Upload PSA Attachment
            /// </summary>
            public const string UPLOAD_ATTACHMENT = "UploadAttachment";

            /// <summary>
            /// Download PSA Attachment
            /// </summary>
            public const string DOWNLOAD_ATTACHMENT = "DownloadAttachment";

            /// <summary>
            /// Delete PSA Attachment
            /// </summary>
            public const string DELETE_ATTACHMENT = "DeleteAttachment";

            /// <summary>
            /// Validates that the required attachments are uploaded.
            /// </summary>
            public const string VALIDATE_REQUIRED_ATTACHMENTS = "ValidateRequiredAttachments";

            #endregion Post Submittal Attachments

            #region Certification Timeline

            /// <summary>
            /// Displays Certification Timeline
            /// </summary>
            public const string DISPLAY_CERTIFICATION_TIMELINE = "DisplayCertificationTimeline";

            /// <summary>
            /// Saves Certification timeline
            /// </summary>
            public const string SAVE_CERTIFICATION_TIMELINE = "SaveCertificationTimeline";

            /// <summary>
            /// Completes Certification timeline
            /// </summary>
            public const string COMPLETE_CERTIFICATION_TIMELINE = "CompleteCertificationTimeline";

            #endregion Certification Timeline
        }

        #endregion Action

        #region Controller
        /// <summary>
        /// Constants for Controllers
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Controller
        {
            /// <summary>
            /// Admin
            /// </summary>
            public const string ADMIN = "admin";

            /// <summary>
            /// Proposal
            /// </summary>
            public const string PROPOSAL = "proposal";

            /// <summary>
            /// GenTRAC
            /// </summary>
            public const string GEN_TRAC = "gentrac";

            /// <summary>
            /// Home
            /// </summary>
            public const string HOME = "home";

            /// <summary>
            /// Reports
            /// </summary>
            public const string REPORTS = "reports";

            /// <summary>
            /// Checklist
            /// </summary>
            public const string CHECKLIST = "checklist";

            /// <summary>
            /// Checklist
            /// </summary>
            public const string APPROVALS = "approvals";

            /// <summary>
            /// Contracts
            /// </summary>
            public const string CONTRACTS = "contracts";

            /// <summary>
            /// Revision
            /// </summary>
            public const string REVISION = "revision";

            /// <summary>
            /// Post Submittal Attachments
            /// </summary>
            public const string PSA = "postsubmittalattachments";

            /// <summary>
            /// The Certification timeline
            /// </summary>
            public const string CERTIFICATION_TIMELINE = "certificationtimeline";
        }
        #endregion Controller

        #region View
        /// <summary>
        /// Constants for Views
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class View
        {
            /// <summary>
            /// Index
            /// </summary>
            public const string INDEX = "Index";

            /// <summary>
            /// GenTRAC home menu
            /// </summary>
            public const string GENTRAC_HOME_MENU = "HomeMenu";

            /// <summary>
            /// Error
            /// </summary>
            public const string ERROR = "Error";

            /// <summary>
            /// Security error
            /// </summary>
            public const string SECURITY_ERROR = "SecurityError";

            /// <summary>
            /// Active Directory search
            /// </summary>
            public const string ACTIVE_DIRECTORY_SEARCH = "ActiveDirectorySearch";

            /// <summary>
            /// ONLINE_USERS
            /// </summary>
            readonly static public string ONLINE_USERS = "UsersOnlineDetails";

            #region Admin

            /// <summary>
            /// Admin system permissions
            /// </summary>
            public const string ADMIN_SYSTEM_PERMISSIONS = "ManagePermissions";

            /// <summary>
            /// Admin system permissions grid
            /// </summary>
            public const string ADMIN_SYSTEM_PERMISSIONS_GRID = "ManagePermissionsGrid";

            /// <summary>
            /// Manage proposal information
            /// </summary>
            public const string MANAGE_PROPOSAL_INFO = "ManageProposalInformation";

            /// <summary>
            /// Manage proposal information details
            /// </summary>
            public const string MANAGE_PROPOSAL_INFO_DETAILS = "ManageProposalInformationDetails";

            /// <summary>
            /// Bulk Archive
            /// </summary>
            public const string BULK_ARCHIVE = "BulkArchive";

            #endregion

            #region Home

            /// <summary>
            /// Home GenTRAC home
            /// </summary>
            public const string HOME_GENTRAC_HOME = "GenTRACHome";

            /// <summary>
            /// Home My proposals view
            /// </summary>
            public const string HOME_PROPOSAL = "GenTRACHomeProposal";

            /// <summary>
            /// Home My Proposals grid
            /// </summary>
            public const string HOME_PROPOSAL_GRID = "GenTRACHomeProposalGrid";

            /// <summary>
            /// Home Proposals filter
            /// </summary>
            public const string HOME_PROPOSAL_FILTERS = "GenTRACProposalFilters";

            /// <summary>
            /// Get My Approvals
            /// </summary>
            public const string GET_MY_APPROVALS = "MyApprovals";

            /// <summary>
            /// Home go to proposal
            /// </summary>
            public const string HOME_GOTO_PROPOSAL = "GoToProposal";

            /// <summary>
            /// PostSubmittalAttachments view
            /// </summary>
            public const string POST_SUBMITTAL_ATTACHMENTS = "PostSubmittalAttachments";

            /// <summary>
            /// The Certification timeline view
            /// </summary>
            public const string CERTIFICATION_TIMELINE = "CertificationTimeline";
            #endregion Home

            #region Permissions

            /// <summary>
            /// Permissions manage permissions
            /// </summary>
            public const string PERMISSIONS_MANAGE_PERMISSIONS = "ManagePermissions";

            /// <summary>
            /// Permissions manage permissions grid
            /// </summary>
            public const string PERMISSIONS_MANAGE_PERMISSIONS_GRID = "ManagePermissionsGrid";

            #endregion Permissions

            #region Reports

            /// <summary>
            /// Reports home
            /// </summary>
            public const string REPORTS_HOME = "ReportsHome";

            /// <summary>
            /// proposal log report
            /// </summary>
            public const string REPORT_PROPOSAL_LOG = "ProposalLogReport";

            /// <summary>
            /// proposal log report's parameters section
            /// </summary>
            public const string REPORT_PROPOSAL_LOG_PARAMETERS = "ProposalLogReportParameters";

            #region Proposal Log Report Parameter Strings
            /// <summary>
            /// the propsal log's proposal status parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_PROPOSAL_STATUS = "ProposalStatus";

            /// <summary>
            /// the propsoal log's all proposals parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_ALL_PROPOSALS = "AllProposals";

            /// <summary>
            /// the proposal log's specific proposals parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_SPECIFIC_PROPOSALS = "SpecificProposals";

            /// <summary>
            /// the proposal log's year parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_YEAR = "Year";

            /// <summary>
            /// the proposal log's Program Area parameter
            /// This constant must remain set to LOB becuase the database and SSRS were not updated to reflect the new org tier names.
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_PROGRAM_AREA = "LOB";

            /// <summary>
            /// the proposal log's segment parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_SEGMENT = "Segment";

            /// <summary>
            /// the proposal log's field estimator parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_FIELD_ESTIMATOR = "FieldEstimator";

            /// <summary>
            /// the proposal log's central estimator parameter
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_CENTRAL_ESTIMATOR = "CentralEstimator";

            /// <summary>
            /// the proposal log's submit start date
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_SUBMIT_START_DATE = "SubmitStartDate";

            /// <summary>
            /// the proposal log's submit end date
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_SUBMIT_END_DATE = "SubmitEndDate";

            /// <summary>
            /// the proposal log's tracking number
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_TRACKING_NUMBER = "TrackingNumber";

            /// <summary>
            /// the proposal log's execution user id
            /// </summary>
            public const string PROPOSAL_LOG_PARAMETER_EXECUTION_USER_ID = "ExecutionUserID";

            #endregion Proposal Log Report Parameter Strings

            /// <summary>
            /// proposal activity report
            /// </summary>
            public const string REPORT_PROPOSAL_ACTIVITY = "ProposalActivityReport";

            /// <summary>
            /// proposal activity report's parameters section
            /// </summary>
            public const string REPORT_PROPOSAL_ACTIVITY_PARAMETERS = "ProposalActivityReportParameters";

            /// <summary>
            /// Dfars report
            /// </summary>
            public const string REPORT_DFARS = "DfarsReport";

            /// <summary>
            /// Dfars report's parameter section
            /// </summary>
            public const string REPORT_DFARS_PARAMETERS = "DfarsReportParameters";

            #endregion

            #region Proposal

            /// <summary>
            /// Proposal index
            /// </summary>
            public const string PROPOSAL_INDEX = "ProposalIndex";

            /// <summary>
            /// Proposal information
            /// </summary>
            public const string PROPOSAL_INFORMATION = "ProposalInformation";

            /// <summary>
            /// Proposal general information
            /// </summary>
            public const string PROPOSAL_GENERAL_INFORMATION = "ProposalGeneralInformation";

            /// <summary>
            /// Proposal approvals information
            /// </summary>
            public const string PROPOSAL_APPROVALS = "ProposalApprovals";

            /// <summary>
            /// Proposal user information
            /// </summary>
            public const string PROPOSAL_USER_INFORMATION = "ProposalUserInformation";

            /// <summary>
            /// Proposal information form
            /// </summary>
            public const string PROPOSAL_INFORMATION_FORM = "ProposalInformationForm";

            /// <summary>
            /// Proposal Details
            /// </summary>
            public const string PROPOSAL_DETAILS = "ProposalDetails";

            /// <summary>
            /// Proposal post information
            /// </summary>
            public const string PROPOSAL_CERTIFICATION_TIMELINE = "CertificationTimeline";

            /// <summary>
            /// Proposal Revision History
            /// </summary>
            public const string PROPOSAL_RevisionHistory = "RevisionHistory";

            /// <summary>
            /// Proposal Comments
            /// </summary>
            public const string PROPOSAL_COMMENTS = "ProposalComments";

            #endregion Proposal

            #region Contracts

            /// <summary>
            /// Approvals index
            /// </summary>
            public const string CONTRACTS_INDEX = "ContractsIndex";

            #endregion

            #region Approvals

            /// <summary>
            /// Approvals index
            /// </summary>
            public const string APPROVALS_INDEX = "ApprovalsIndex";

            /// <summary>
            /// Approver section
            /// </summary>
            public const string APPROVER_SECTION = "ApproverSection";
            #endregion

            #region Checklist

            /// <summary>
            /// Checklist index
            /// </summary>
            public const string CHECKLIST_INDEX = "ChecklistIndex";

            /// <summary>
            /// checklist general information
            /// </summary>
            public const string CHECKLIST_GENERAL_INFORMATION = "ChecklistGeneralInformation";

            /// <summary>
            /// checklist proposal pricing data
            /// </summary>
            public const string CHECKLIST_PROPOSAL_PRICING_DATA = "ChecklistProposalPricingData";

            /// <summary>
            /// checklist PPR document
            /// </summary>
            public const string CHECKLIST_PPR_DOCUMENT = "ChecklistPPRDocument";

            /// <summary>
            /// checklist PPR document
            /// </summary>
            public const string CHECKLIST_PAR_DOCUMENT = "ChecklistPARDocument";

            #endregion Checklist
        }

        #endregion View

        #region Cookies

        /// <summary>
        /// Constants for the proposal filters cookie
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProposalFiltersCookieConstants
        {
            /// <summary>
            /// Key for proposal filter cookie
            /// </summary>
            public const string COOKIE_KEY = "ProposalFilterCookie";

            /// <summary>
            /// Filter option 
            /// </summary>
            public const string OPTION = "ProposalFilterOption";

            /// <summary>
            /// Viewer Filter option 
            /// </summary>
            public const string VIEWER_OPTION = "ViewerProposalFilterOption";

            /// <summary>
            /// Filter start date
            /// </summary>
            public const string START_DATE = "ProposalFilterStartDate";

            /// <summary>
            /// Filter end date
            /// </summary>
            public const string END_DATE = "ProposalFilterEndDate";
        }
        #endregion Cookies

        #region ApprovalDescription
        /// <summary>
        /// Constants for Approval Role descriptions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ApprovalDescription
        {
            /// <summary>
            /// Lead Estimator description
            /// </summary>
            public const string LEAD_ESTIMATOR = "The Lead Estimator is responsible for the overall compliance with FAR and the SSC CIPS 1.8.5 command media. The Cost Volume will be reviewed continuously through the development of the proposal so that the detection of any errors and corrections are made in a timely manner. Prior to proposal submittal, the Lead Estimator will sign the checklist attesting to the conclusion of the review and obtain all required signatures (dependent on type of proposal and dollar value).<br/><br/>By submitting your approval, you are attesting to the above description. Once your approval is submitted, you cannot undo it or edit your comment unless the workflow is reset by the Lead Estimator.<br/><br/>Reminder - You must maintain a copy of completed Checklists. In accordance with Space command media, the DFARS checklist should be exported and submitted with the cost volume when required by the RFP. Are you sure you wish to continue?";

            /// <summary>
            /// Cover Sheet Approver description
            /// </summary>
            public const string COVER_SHEET_APPROVER = "The Cover Sheet Approver will validate that every question on the checklist has been completed and validated in the proposal. If there are any non-compliant answers, the Cover Sheet Approver is responsible for coordinating the completion of a Proposal Corrective Action Plan form <a href='https://space.p.external.lmco.com/sites/cipspal/_layouts/lmssc.ide.cips/SearchResults.aspx?search=1.8.5&type=all'>N1.8.5-T1-Estmate-1.0-P-F1</a>. The Cover Sheet Approver needs to be working with the Lead Estimator ahead of time to work items as the proposal is being developed. The Cover Sheet Approver may assume the role of Independent Reviewer if they are independent of proposal development. If the Lead Estimator and the Cover Sheet Approver are the same person, an Independent Reviewer will be required.<br/><br/>By submitting your approval, you are attesting to the above description. Once your approval is submitted, you cannot undo it or edit your comment unless the workflow is reset by the Lead Estimator. Are you sure you wish to continue?";

            /// <summary>
            /// Pricing Verification description
            /// </summary>
            public const string PRICING_VERIFICATION = "By signing the checklist, the Pricing Verification individual attests that the final pricing has been validated, all rates and factors are current, and that the calculations are correct. This review should also ensure that BOEs are provided in the proposal for all rates (program unique and Space level). The reviewer ensures that proposal pricing is correct as of the date on the cover sheet and that if any errors are detected they are corrected prior to proposal submittal.<br/><br/>By submitting your approval, you are attesting to the above description. Once your approval is submitted, you cannot undo it or edit your comment unless the workflow is reset by the Lead Estimator. Are you sure you wish to continue?";

            /// <summary>
            /// Independent Reviewer description
            /// </summary>
            public const string INDEPENDENT_REVIEWER = "The Independent Reviewer will validate that every question on the checklist has been completed and validated in the proposal and review BOEs for compliance with the checklist. The Independent Reviewer needs to be involved as soon as practicable to ensure any error/duplications noticed are corrected in a timely manner.<br/><br/>By submitting your approval, you are attesting to the above description. Once your approval is submitted, you cannot undo it or edit your comment unless the workflow is reset by the Lead Estimator. Are you sure you wish to continue?";

            /// <summary>
            /// LOB Estimating Lead description
            /// </summary>
            public const string LOB_ESTIMATING_LEAD = "It is the LOB Estimating Lead/Manager's responsibility to ensure they meet with the proposal team and have an overall understanding of the proposal structure. The LOB lead/manager will check for Space Estimating System compliance ensuring that 1) a RAM is completed; 2) a cost kickoff package was presented to the proposal team; 3) a Compliance Review Checklist was completed and if there are any non-compliances that the PCA form is completed and management is advised; 4) ensure a cover sheet was signed by an authorized approver; and 5) that the proposal was entered into the Proposal Tracking Module (PTM) and that the dates and amounts are correct at time of submittal. The Lead/Manager may also do any level of review they deem necessary to ensure that any current proposal issues or prior audit/customer concerns are addressed in the proposal.<br/><br/>By submitting your approval, you are attesting to the above description. Once your approval is submitted, you cannot undo it or edit your comment unless the workflow is reset by the Lead Estimator. Are you sure you wish to continue?";

            /// <summary>
            /// The reset workflow description.
            /// </summary>
            public const string RESET_WORKFLOW = "Clicking ‘Reset Workflow’ will erase all existing signatures and comments and start the workflow from the beginning.";
        }
        #endregion
    }
}