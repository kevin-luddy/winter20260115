// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// The constants class, containing shared constants to be used across the entire solution.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The maximum length of an RTE field
        /// </summary>
        public const int MAX_RTE_LENGTH = 500000;

        /// <summary>
        /// The BOE database context name
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static string BOE_DB_CONTEXT_NAME = "name=GenBoeEntities";

        /// <summary>
        /// The IES database context name
        /// </summary>
        public const string IES_DB_CONTEXT_NAME = "name=IESEntities";

        /// <summary>
        /// The PTM database context name
        /// </summary>
        public const string PTM_DB_CONTEXT_NAME = "name=genTracEntities";

        /// <summary>
        /// The ContractType Not Set value.
        /// </summary>
        public const int CONTRACT_TYPE_NOT_SET = -1;

        /// <summary>
        /// The ContractType Not Set string value
        /// </summary>
        public const string CONTRACT_TYPE_NOT_SET_STRING = "Not Set";

        /// <summary>
        /// The Proposal class type not set value.
        /// </summary>
        public const int PROPOSAL_CLASS_TYPE_NOT_SET = -1;

        /// <summary>
		/// Prefix for the JWT Bearer
		/// </summary>
		public static readonly string TOKEN_PREFIX = "Bearer ";

        /// <summary>
        /// The IDIQ Contract Type
        /// </summary>
        public const string IDIQ_CONTRACT_TYPE = "IDIQ";

        /// <summary>
        /// The PTM IDIQ value
        /// </summary>
        public const int PTM_IDIQ_VALUE = 11;

        /// <summary>
        /// String that is used for unique wbs numbers for multi boe
        /// </summary>
        public const string UNIQUE_MULTI_NUMBER = "MULTI";

        /// <summary>
        /// String that gets logged when an activity starts.
        /// </summary>
        public const string LOG_ACTIVITY_START = "Begin {0}.";

        /// <summary>
        /// String that gets logged when an activity ends.
        /// </summary>
        public const string LOG_ACTIVITY_END = "Finished {0}: {1} milliseconds.";

        /// <summary>
        /// Error message to log, if an insert fails due to ID.
        /// </summary>
        public const string ERR_INSERT_FAILED_DUE_TO_ID = "The insert of a new item failed due to an ID.";

        /// <summary>
        /// Error message to log, if an update fails due to ID.
        /// </summary>
        public const string ERR_UPDATE_FAILED_DUE_TO_ID = "The returned ID from an update did not match the one passed in.";

        /// <summary>
        /// Currency Formatting String
        /// </summary>
        public const string MONEY_FORMATTING = "{0:$#,##0.00}";

        /// <summary>
        /// Date format for Users Online
        /// </summary>
        public const string USERS_ONLINE_LONG_DATE_FORMAT = "MMM d, yyyy hh:mm:ss tt";

        /// <summary>
        /// Percentage Formatting String
        /// </summary>
        public const string PERCENTAGE_FORMATTING = "P";

        /// <summary>
        /// Number with Commas Formatting String
        /// </summary>
        public const string NUMBER_WITH_COMMAS_FORMATTING = "N";

        /// <summary>
        /// Number with Commas Formatting String, no decimal places
        /// </summary>
        public const string NUMBER_WITH_COMMAS_FORMATTING_NO_DECIMAL_PLACES = "N0";

        /// <summary>
        /// Decimal Formatting String
        /// </summary>
        public const string DECIMAL_FORMATTING = "G";

        /// <summary>
        /// Fixed Point Formatting String (to 6 decimal places)
        /// </summary>
        public const string FIXED_POINT_FORMATTING_SIX_DECIMAL_PLACES = "F6";

        /// <summary>
        /// Date Formatting - MM/dd/yyyy.
        /// </summary>
        public const string DATE_FORMATTING_MONTH_DAY_YEAR = "MM/dd/yyyy";

        /// <summary>
        /// Date Formatting - yyyy-MM-dd.
        /// </summary>
        public const string DATE_FORMATTING_YEAR_MONTH_DAY = "yyyy-MM-dd";

        /// <summary>
        /// Class used to make the text red color
        /// </summary>
        public const string RED_TEXT_CSS_CLASS_STRING = "redtext";

        /// <summary>
        /// Class used to make the background grey color
        /// </summary>
        public const string GREY_BACKGROUND_CSS_CLASS_STRING = "background-color: #DFDFDF";

        /// <summary>
        /// Class used to make the background red color
        /// </summary>
        public const string RED_BACKGROUND_CSS_CLASS_STRING = "background-color: #FF5D5D";

        /// <summary>
        /// CSS used for late certification
        /// </summary>
        public const string SUBMITTED_LATE_BACKGROUND_CSS_CLASS_STRING = "background-color: #ff9933";

        /// <summary>
        /// CSS used for submitted background, not yet late
        /// </summary>
        public const string SUBMITTED_NOT_LATE_BACKGROUND_CSS_CLASS_STRING = "background-color: #0099ff";

        /// <summary>
        /// CSS used for pending contractual award
        /// </summary>
        public const string PENDING_AWARD_BACKGROUND_CSS_CLASS_STRING = "background-color: #89cdfa";

        /// <summary>
        /// Class used to make the background green color
        /// </summary>
        public const string GREEN_BACKGROUND_CSS_CLASS_STRING = "background-color: #92D050";

        /// <summary>
        /// Class used to make the background yellow color
        /// </summary>
        public const string YELLOW_BACKGROUND_CSS_CLASS_STRING = "background-color: #FCFF71";

        /// <summary>
        /// Class used to set the background color for forecasted dates that are near in the future.
        /// </summary>
        public const string FORECASTED_NEAR_DATE_CSS_CLASS_STRING = "background: linear-gradient(to right, rgba(255,93,93,1) 20%, rgba(255,93,93,0));";

        /// <summary>
        /// Class used to set the background color for forecasted dates that are further in the future.
        /// </summary>
        public const string FORECASTED_FAR_DATE_CSS_CLASS_STRING = "background: linear-gradient(to right, rgba(248,252,78,1) 20%, rgba(248,252,78,0));";

        /// <summary>
        /// CSS to set the background color for revised proposal's Est. Ship Date.
        /// </summary>
        public const string REVISED_DATE_CSS_STYLE_STRING = "background: linear-gradient(to right, rgb(210,180,140) 10%, rgb(255,228,196));";

        /// <summary>
        /// Class used to make the text white color
        /// </summary>
        public const string WHITE_TEXT_CSS_CLASS_STRING = "whitetext";

        /// <summary>
        /// Used so the Go To Proposal searches will work with either the current or legacy proposal tracking number structure.
        /// </summary>
        public const string PROPOSAL_CENTURY = "20";

        /// <summary>
        /// The length of the YY-NNNNN tracking number
        /// </summary>
        public const int TRACKING_NUMBER_LENGTH = 8;

        /// <summary>
        /// Proposal Class - Forecasted
        /// </summary>
        public const string PROPOSAL_CLASS_FORECASTED = "Forecasted";

        /// <summary>
        /// The length of the YYYY-NNNNN legacy tracking number
        /// </summary>
        public const int LEGACY_TRACKING_NUMBER_LENGTH = 10;

        /// <summary>
        /// The first checklist row comment version that has the PricerPageNumber and PricerRowComment features
        /// </summary>
        public const string CHECKLIST_ROW_COMMENT_VERSION = "ChecklistRowCommentVersion";

        /// <summary>
        /// The first checklist version that has Show/Hide PPR and PAR capability based on "Is Certified Cost or Pricing Data Required?" reponses
        /// </summary>
        public const string CHECKLIST_CHANGE_TO_PTM_VERSION = "ChecklistChangeToPTMVersion";

        /// <summary>
        /// The regular expression defining the '~' and '^' unallowed characters used as field delimiters and a row delimiters (respectively) for the 
        /// PAR checklist row responses.
        /// </summary>
        public const string CHECKLIST_ROW_INVALID_REGEX = @"[~^]";

        /// <summary>
        /// Used to deal w/ the offset for the enums, when converting between the DB and the application (to tell apart T and E rates)
        /// </summary>
        public const int T_RATES_OFFSET_FROM_E_RATES = 5;

        /// <summary>
        /// The default number of seconds for a TransactionScope timeout
        /// </summary>
        public const int DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT = 60;

        /// <summary>
        /// The default number of seconds for a TransactionScope timeout when copying a workspace
        /// </summary>
        public const int DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT = 600;

        /// <summary>
        /// Used to generate a unique identifier string based on the current system time
        /// </summary>
        public const string UNIQUE_IDENTIFIER_TIMESTAMP_FORMAT = "yyyyMMddHHmmssffff";

        /// <summary>
        /// The maximum number of columns per table supported by Microsoft Word
        /// </summary>
        public const int MAXIMUM_COLUMNS_PER_TABLE_MS_WORD = 63;

        /// <summary>
        /// Cost resource rate and spread decimal precision when tracking dollars and cents. Valid 
        /// precisions for cost are 0 and 2.
        /// </summary>
        public const int DOLLARS_AND_CENTS_PRECISION = 2;

        /// <summary>
        /// Timeout for Regex matching
        /// 
        /// It is set to 5 minutes.. This is not meant as a performance benchmark, just as a fail-safe to prevent the application from locking up IIS
        /// </summary>
        public static readonly TimeSpan REGEX_TIMEOUT = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Used for Zone Travel Resources when there is no Resource entered
        /// </summary>
        public static readonly string ZONE_TRAVEL_RESOURCE_NO_RATE = "NO-RATE";

        /// <summary>
        /// Used for Zone Travel Trips Grid when a field is not applicable
        /// Also used in the PPR&amp;D rate code tables when there is no rate value for a given year.
        /// </summary>
        public static readonly string NOT_APPLICABLE = "N/A";

        /// <summary>
        /// The default threshold for number of results returned by Quick Search or Advanced Search
        /// </summary>
        public const int SEARCH_RESULTS_THRESHOLD_DEFAULT = 100;

        /// <summary>
        /// The default cost resource decimal precision used when creating a ProjectMap workspace.
        /// </summary>
        public const int PROJECT_MAP_DECIMAL_PRECISION_DEFAULT = 0;

        /// <summary>
        /// SSRS Cost Analysis Reports - The ResourceUnit for Resources represented by Hours.
        /// </summary>
        public const string COST_ANALYSIS_RESOURCE_UNIT_HOURS = "Hours";

        /// <summary>
        /// SSRS Cost Analysis Reports - The ResourceUnit for Resources represented by Direct Dollars.
        /// </summary>
        public const string COST_ANALYSIS_RESOURCE_UNIT_DIRECT_DOLLARS = "Direct Dollars";

        /// <summary>
        /// SSRS BOE Summary Report - The ResourceUnit for Resources represented by Hours.
        /// </summary>
        public const string BOE_SUMMARY_RESOURCE_UNIT_HOURS = "Hours";

        /// <summary>
        /// SSRS BOE Summary Report - The ResourceUnit for Resources represented by Cost Dollars.
        /// </summary>
        public const string BOE_SUMMARY_RESOURCE_UNIT_COST_DOLLARS = "Cost Dollars";

        /// <summary>
        /// The custom field is required text
        /// </summary>
        public const string CUSTOM_FIELD_IS_REQUIRED = "Custom Field {0} is required.";

        /// <summary>
        /// At least 1 MOQ type is required for a task.
        /// </summary>
        public const string MOQ_TYPE_REQUIRED_FOR_TASK = "MOQ Type is required for the task. Please add at least one.";

        /// <summary>
        /// The field cannot be changed in a task in a locked workspace
        /// </summary>
        public const string CANNOT_CHANGE_IN_LOCKED_TASK = "{0} cannot be changed while the Workspace is locked.";

        /// <summary>
        /// SSRS RPS Report - The ResourceUnit for Resources represented by Hours.
        /// </summary>
        public const string RPS_RESOURCE_UNIT_HOURS = "Hours";

        /// <summary>
        /// SSRS RPS Report - The ResourceUnit for Resources represented by Dollars.
        /// </summary>
        public const string RPS_RESOURCE_UNIT_DOLLARS = "Dollars";

        /// <summary>
        /// The number of month columns that should be included in the SSRS RPS Report.
        /// </summary>
        public const int SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS = 204;

        /// <summary>
        /// do not show the SSRS parameters in the window
        /// </summary>
        public const string NO_REPORT_PARAMETERS = "&rc:Parameters=false";

        /// <summary>
        /// SSRS parameter for Report Type
        /// </summary>
        public const string REPORT_TYPE = "reportType";

        /// <summary>
        /// SSRS parameter for Workspace ID
        /// </summary>
        public const string WORKSPACE_ID = "wsid";

        /// <summary>
        /// Max Length of Performing Org Description
        /// </summary>
        public const int PERF_ORG_DESC_MAX_LENGTH = 50;

        /// <summary>
        /// The read only roles for RDSB.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly PtmRole[] READ_ONLY_ROLES = { PtmRole.PeerReviewer, PtmRole.CoverSheetApprover, PtmRole.LOBEstLead, PtmRole.PricingVerification };

        /// <summary>
        /// The edit roles for RDSB.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly PtmRole[] EDIT_ROLES = { PtmRole.Pricer, PtmRole.BackupPricer, PtmRole.CostVolumeLead, PtmRole.Admin };

        /// <summary>
        /// The tina training courseid
        /// </summary>
        public const string TINA_TRAINING_COURSEID = "000029WPL00";

        public const string SSC_OLD_BOE_WRITING_COURSE = "077091WPL00";

        public const string SSC_OLD_BOE_WRITING_COURSE2 = "077091ILT00";

        /// <summary>
        /// The subcontract administrator courseid
        /// </summary>
        public const string SUBCONTRACT_TRAINING_COURSEID = "216700SSM00";

        public const string SHARED_BOE_WRITING_COURSE = "076896WPL00";

        #region MOQ Type Text Field Lengths

        /// <summary>
        /// Standard length for MOQ Type Text fields (matches db length)
        /// </summary>
        public const int MOQ_TYPE_TEXT_FIELD_LENGTH = 255;

        /// <summary>
        /// Length for MOQ Type Repository Name field (matches db length)
        /// </summary>
        public const int MOQ_REPOSITORY_NAME_FIELD_LENGTH = 50;

        /// <summary>
        /// Length for MOQ Type Query Type field (matches db length)
        /// </summary>
        public const int MOQ_QUERY_TYPE_FIELD_LENGTH = 40;

        /// <summary>
        /// Length for MOQ Type Historical Program Name field (matches db length)
        /// </summary>
        public const int MOQ_HISTORICAL_PROG_NAME_FIELD_LENGTH = 125;

        /// <summary>
        /// Length for MOQ Type WBS Element field for RMS (matches db length)
        /// </summary>
        public const int MOQ_WBS_ELEMENT_RMS_FIELD_LENGTH = 12;

        /// <summary>
        /// Length for MOQ Type WBS Element field for SSC (matches db length)
        /// </summary>
        public const int MOQ_WBS_ELEMENT_SSC_FIELD_LENGTH = 5000;

        #endregion

        #region PickList

        /// <summary>
        /// Validation Constants for PickList across PTM/BOE
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class PickListValidation
        {
            /// <summary>
            /// The validation for number of items in PTM vs. BOE
            /// </summary>
            public const string NUMBER_ITEMS = "The number of Pick List items in PTM/BOE are different.";

            /// <summary>
            /// The validation for the picklist name in PTM vs. BOE
            /// </summary>
            public const string NAME = "PickList Loaders for PTM/BOE are not in sync for the Pick List Name.";

            /// <summary>
            /// The validation for Pick Lists in PTM/BOE that both has parents values.
            /// </summary>
            public const string CONTAINS_PARENTS = "IES does not currently support the saving of Pick Lists across PTM/BOE where both Pick Lists have parents.";

            /// <summary>
            /// The validation for Pick List values in PTM/BOE having different Multiple Parents value.
            /// </summary>
            public const string MULTIPLE_PARENTS = "PickList Loaders for PTM/BOE both use parents but are not in sync for allowing Multiple Parents.";

            /// <summary>
            /// The validation for Pick List values in PTM/BOE having different IsActive value.
            /// </summary>
            public const string ACTIVE = "The Is Active field for Pick List Item {0} is different in PTM/BOE";

            /// <summary>
            /// The validation for missing Pick List values.
            /// </summary>
            public const string MISSING = "{0} is missing inside the {1} Pick List values.";
        }
        #endregion PickList

        #region SSRS Report
        /// <summary>
        /// SSRS report parameters. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Report
        {
            /// <summary>
            /// proposal title
            /// </summary>
            public const string PROPOSAL_TITLE = "ProposalTitle";

            /// <summary>
            /// proposal status
            /// </summary>
            public const string PROPOSAL_STATUS = "ProposalStatus";

            /// <summary>
            /// all proposals. This will be passed in as True if selected
            /// </summary>
            public const string ALL_PROPOSALS = "AllProposals";

            /// <summary>
            /// specific proposals
            /// </summary>
            public const string SPECIFIC_PROPOSALS = "SpecificProposals";

            /// <summary>
            /// year selected for specific proposals
            /// </summary>
            public const string YEAR = "Year";

            /// <summary>
            /// Line Of Business (LOB) selected for specific proposals
            /// </summary>
            public const string LINE_OF_BUSINESS = "LOB";

            /// <summary>
            /// Program Area (PA) selected for specific proposals
            /// </summary>
            public const string PROGRAM_AREA = "PA";

            /// <summary>
            /// central estimator pricers selected for specific proposals
            /// </summary>
            public const string CENTRAL_ESTIMATOR = "CentralEstimator";

            /// <summary>
            /// submit start date 
            /// </summary>
            public const string SUBMIT_START_DATE = "SubmitStartDate";

            /// <summary>
            /// submit end date
            /// </summary>
            public const string SUBMIT_END_DATE = "SubmitEndDate";

            /// <summary>
            /// tracking number begins with
            /// </summary>
            public const string TRACKING_NUMBER = "TrackingNumber";

            /// <summary>
            /// user id of the person running the report and any group user Ids they are part of
            /// </summary>
            public const string EXECUTION_USER_ID = "ExecutionUserID";

            /// <summary>
            /// ProposalStatusID for the SSRS proposal Export report
            /// </summary>
            public const string PROPOSAL_STATUS_ID = "ProposalStatusID";

            /// <summary>
            /// The filter for proposal class for SSRS proposal Export report.
            /// </summary>
            public const string FILTER_PROPOSAL_CLASS = "ProposalClassID";

            /// <summary>
            /// AssignedStartDate for the SSRS proposal Export report
            /// </summary>
            public const string FILTER_START_DATE = "AssignedStart";

            /// <summary>
            /// AssignedEnd (Date) for the SSRS proposal Export report
            /// </summary>
            public const string FILTER_END_DATE = "AssignedEnd";

            /// <summary>
            /// Search string for the SSRS proposal Export report
            /// </summary>
            public const string SEARCH_TEXT = "Search";

            /// <summary>
            /// NTID for the SSRS proposal Export report
            /// </summary>
            public const string NTID = "NTID";

            /// <summary>
            /// NTDomain for the SSRS proposal Export report
            /// </summary>
            public const string NT_DOMAIN = "NTDomain";

            /// <summary>
            /// do not show the SSRS parameters in the window
            /// </summary>
            public const string NO_REPORT_PARAMETERS = "&rc:Parameters=false";

            /// <summary>
            /// The default proposal date format;
            /// </summary>
            public const string PROPOSAL_DATE_FORMAT = "mm/dd/yyyy";

            /// <summary>
            /// customer type
            /// </summary>
            public const string CUSTOMER_TYPE = "CustomerType";

            /// <summary>
            /// create start date 
            /// </summary>
            public const string CREATE_START_DATE = "CreateStartDate";

            /// <summary>
            /// create end date
            /// </summary>
            public const string CREATE_END_DATE = "CreateEndDate";

            /// <summary>
            /// pricer ID
            /// </summary>
            public const string PRICER_ID = "PricerNTID";

            /// <summary>
            /// submitted date
            /// </summary>
            public const string SUBMITTED_DATE = "SubmittedDate";

            /// <summary>
            /// the propsal id
            /// </summary>
            public const string PROPOSAL_ID = "ProposalID";

            /// <summary>
            /// the PAR Checklist Id 
            /// </summary>
            public const string PROPOSAL_ADEQUACY_REVIEW_ID = "ProposalAdequacyReviewID";

            /// <summary>
            /// the start date
            /// </summary>
            public const string START_DATE = "StartDate";

            /// <summary>
            /// the end date
            /// </summary>
            public const string END_DATE = "EndDate";

            /// <summary>
            /// Viewer Filter Option
            /// </summary>
            public const string SHOW_PROPOSALS_FOR_MY_ORGANIZATION = "ShowProposalsForMyOrganization";

            /// <summary>
            /// User and Group IDs
            /// </summary>
            public const string USER_AND_GROUP_IDS = "UserAndGroupIDs";
        }
        #endregion SSRS Report

        #region Contracts

        /// <summary>
        /// Validation failed for Cage Code
        /// </summary>
        public const string INVALID_CAGE_CODE = "Cage # is required.";

        /// <summary>
        /// Validation failed for Final Negotiated Value
        /// </summary>
        public const string INVALID_FINAL_NEGOTIATED_VALUE = "Final Negotiated Value is required.";

        /// <summary>
        /// Validation failed for Negotiations Submitted Date
        /// </summary>
        public const string INVALID_NEGOTIATIONS_SUBMITTED_DATE = "Negotiations Submitted Date is required.";

        /// <summary>
        /// Validation failed for MOD Completion Date
        /// </summary>
        public const string INVALID_MOD_COMPLETION_DATE = "MOD Completion Date is required.";

        /// <summary>
        /// Validation failed for LM Win/Loss flag
        /// </summary>
        public const string INVALID_LM_WIN_LOSS = "Contract Won setting is required.";

        /// <summary>
        /// Validation failed for LM Win/Loss flag because value is false
        /// </summary>
        public const string INVALID_LM_WIN_LOSS_FALSE = "Contract Won setting is No.";

        /// <summary>
        /// Validation that Proposal Submittal Date must be on/after Date Estimating Submits to Contracts
        /// </summary>
        public const string INVALID_PROPOSAL_SUBMITTAL_DATE = "\"Proposal Submittal Date to the Customer\" must be on or after the \"Date Estimating Submits to Contracts.\"";

        /// <summary>
        /// Validation that Negotiations Submitted mush be on/after Agreement date
        /// </summary>
        public const string INVALID_NEGOTIATIONS_SUBMITTED = "\"Date Confirmation of Negotiations Submitted\" must be on or after the \"Date of Agreement on Final Price (Handshake).\"";

        /// <summary>
        /// Validation that Days to Certification must be positive
        /// </summary>
        public const string INVALID_DAYS_TO_CERT = "\"The Date that the Certificate of CCoPD and any additional disclosures were delivered to the customer\" cannot be prior to the \"Date of Agreement on Final Price (Handshake)\"";

		#endregion


		/// <summary>
		/// SSRS Report Names
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class SSRSReportName
        {
            /// <summary>
            /// Summary Report RMS
            /// For Category/CLIN Summary and CLIN/Category Summary
            /// </summary>
            public const string SUMMARY_REPORT_RMS = "SummaryReportRMS";

            /// <summary>
            /// Engr CLIN Summary
            /// </summary>
            public const string PROJECT_CLIN_COST_SUMMARY = "ProjectCLINCostSummary";

            /// <summary>
            /// Cost Analysis Report RMS
            /// For Cost by CLIN, Res, Act & Yr (8yrs) and Cost by CLIN, Act & Yr (17yrs)
            /// </summary>
            public const string COST_ANALYSIS_REPORT_RMS = "CostAnalysisReportRMS";

            /// <summary>
            /// Flattened Cost Analysis Report RMS
            /// For Estimates by CLIN, Activity, and CY
            /// </summary>
            public const string COST_ANALYSIS_REPORT_RMS_FLAT = "FlattenedCostAnalysisReport";

            /// <summary>
            /// BOE Summary Report
            /// </summary>
            public const string BOE_SUMMARY_REPORT = "BoeSummaryReport";

            /// <summary>
            /// By Pricing Code (17 yrs)
            /// </summary>
            public const string BY_PRICING_CODE = "ByPricingCode";

            /// <summary>
            /// By Cat/Pricing Code (17 yrs)
            /// </summary>
            public const string BY_CAT_PRICING_CODE = "ByCatPricingCode";

            /// <summary>
            /// Offload Cost By Year
            /// </summary>
            public const string OFFLOAD_COST_BY_YEAR = "OffloadCostByYear";

            /// <summary>
            /// The offload detailed report
            /// For Engineering Offload Detailed Calculation Results.
            /// </summary>
            public const string OFFLOAD_DETAILED_REPORT = "OffloadDetailedReport";

            /// <summary>
            /// Offload Cost Summary
            /// </summary>
            public const string OFFLOAD_COST_SUMMARY = "OffloadCostSummary";

            /// <summary>
            /// Staffing Curves
            /// </summary>
            public const string STAFFING_CURVES = "StaffingCurves";

            /// <summary>
            /// RPS
            /// </summary>
            public const string RPS = "RPS";

            /// <summary>
            /// PRP
            /// </summary>
            public const string PRP = "PRP";

            /// <summary>
            /// RAM
            /// </summary>
            public const string RAM = "RAM";

            /// <summary>
            /// Pre vs Post Offload Totals
            /// </summary>
            public const string PRE_VS_POST_OFFLOAD_TOTALS = "PreVsPostOffloadTotals";

            /// <summary>
            /// The Workbench Offload SSRS report name.
            /// </summary>
            public const string WORKBENCH_OFFLOAD = "WorkbenchOffload";
        }

        /// <summary>
        /// Constants used by Sikorsky Custom Fields
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class SikorskyConstants
        {
            /// <summary>
            /// SOW Custom Field Name
            /// </summary>
            public const string SIKORSKY_CF_SOW = "SOW";

            /// <summary>
            /// Category Custom Field Name
            /// </summary>
            public const string SIKORSKY_CF_CATEGORY = "Category";

            /// <summary>
            /// Cam Name Custom Field Name
            /// </summary>
            public const string SIKORSKY_CF_CAMNAME = "Cam Name";

            /// <summary>
            /// Class of Cost Custom Field Name
            /// </summary>
            public const string SIKORSKY_CF_CLASSOFCOST = "Class Of Cost";

            /// <summary>
            /// Add / Delete Custom Field Name
            /// </summary>
            public const string SIKORSKY_CF_ADDDELETE = "Add / Delete";
        }

        /// <summary>
        /// Constants used by ProPricer Custom Fields
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProPricerCFConstants
        {
            /// <summary>
            /// Function Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_FUNCTION = "Function";

            /// <summary>
            /// SOW Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_SOW = "SOW";

            /// <summary>
            /// Location Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_LOCATION = "Location";

            /// <summary>
            /// Class of Cost Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_CLASSOFCOST = "Class Of Cost";

            /// <summary>
            /// Project Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_PROJECT = "Project";

            /// <summary>
            /// Field-A Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_FIELDA = "Field-A";

            /// <summary>
            /// Field-B Custom Field Name
            /// </summary>
            public const string PROPRICER_CF_FIELDB = "Field-B";
        }

        public const string WORD_SSRS_FORMAT = "WORDOPENXML";

        public const string EXCEL_SSRS_FORMAT = "EXCELOPENXML";

        public const string PDF_SSRS_FORMAT = "pdf";
    }
}