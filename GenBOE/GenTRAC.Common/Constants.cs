// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common
{
    /// <summary>
    /// The constants class, containing shared constants to be used across the entire solution.
    /// </summary>
    public static class Constants
    {
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
        public const string ERR_INSERT_FAILED_DUE_TO_ID = "The insert of a new item failed.";

        /// <summary>
        /// Error message to log, if an update fails due to ID.
        /// </summary>
        public const string ERR_UPDATE_FAILED_DUE_TO_ID = "The returned ID from an update did not match the one passed in.";

        /// <summary>
        /// Currency Formatting String
        /// </summary>
        public const string MONEY_FORMATTING = "C";

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
        /// The read only roles for RDSB.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly Role[] READ_ONLY_ROLES = { Role.PeerReviewer, Role.CoverSheetApprover, Role.LOBEstLeadMgr, Role.PricingVerification };

        /// <summary>
        /// The edit roles for RDSB.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly Role[] EDIT_ROLES = { Role.Pricer, Role.BackupPricer, Role.CostVolumeLead, Role.Admin };

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
            /// Program Area (LOB) selected for specific proposals
            /// This constant is set to LOB because the DB and SSRS reports were not updated with the new org names.  The previous ORG name for 
            /// Program Area was Line of Business.
            /// </summary>
            public const string PROGRAM_AREA = "LOB";

            /// <summary>
            /// LOB selected for specific proposals
            /// </summary>
            public const string LOB = "LOB";

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
        }
        #endregion SSRS Report
    }
}
