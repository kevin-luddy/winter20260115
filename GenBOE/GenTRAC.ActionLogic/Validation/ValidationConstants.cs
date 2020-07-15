// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Validation
{
    /// <summary>
    /// Validation constants
    /// </summary>
    public static class ValidationConstants
    {
        /// <summary>
        /// Decimal value
        /// </summary>
        public const string DECIMAL_WITH_COMMAS_FORMAT = @"^[+-]?\d{1,3}(?:,\d{3})*(?:[.]\d{1,2})?$";

        /// <summary>
        /// Integer value
        /// </summary>
        public const string INTEGER_WITH_COMMAS_FORMAT = @"\-?[\d,]*$";

        /// <summary>
        /// Contains only alphanumeric characters
        /// </summary>
        public const string ALPHANUMERIC_FORMAT = @"[a-zA-Z0-9 ]*$";

        /// <summary>
        /// percent format, 00.00 to +-99.00, decimal value up to 2 whole number digits and up to two (optional) decimal places
        /// </summary>
        public const string PERCENT_FORMAT = @"^([-+] ?)?\d{1,2}([.]\d{1,2})?$";

        /// <summary>
        /// Ratio format, ratio format will accept values between 0/0 and 99/99"
        /// </summary>
        public const string RATIO_FORMAT = @"^\d{1,2}\/{1}\d{1,2}$";

        /// <summary>
        /// Price range of +/- 999,999,999,999 with our without commas
        /// </summary>
        public const string PRICE_RANGE_FORMAT = @"^([-+] ?)?[\d]{1,3}(,?[\d]{3}){0,3}$";

        /// <summary>
        /// Absolute Value range - positive number
        /// </summary>
        public const string ABSOLUTE_VALUE_RANGE_FORMAT = @"^([+] ?)?[\d]{1,3}(,?[\d]{3}){0,3}$";

        /// <summary>
        /// Contains only alphanumeric characters and a dash
        /// </summary>
        public const string REPORT_TRACKING_NUMBER_FORMAT = @"^[a-zA-Z0-9-]*$";

        /// <summary>
        /// Must start with an alphanumeric character followed by any free text
        /// Used for validation to eliminate blanks and non-alphanumeric symbols for first character
        /// </summary>
        public const string FREE_TEXT_FORMAT = @"^[a-zA-Z0-9]+(.*)*$";

        /// <summary>
        /// Validation messages for proposal setup
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProposalValidationConstants
        {
            #region Proposal General Info Section
            /// <summary>
            /// text to be concated to the front of a validation message 
            /// </summary>
            private const string GENERAL_INFO_VALIDATION = "(General Information) - ";

            /// <summary>
            /// Line of Business is required where updated with the new name.
            /// </summary>
            public const string LINE_OF_BUSINESS_REQUIRED = GENERAL_INFO_VALIDATION + "Line of Business is required.";

            /// <summary>
            /// Program Area is required
            /// </summary>
            public const string PROGRAM_AREA_REQUIRED = GENERAL_INFO_VALIDATION + "Program Area is required.";

            /// <summary>
            /// Pricing Tool free text must start with an alphanumeric character.
            /// </summary>
            public const string PRICING_TOOL_FORMAT_ERROR = GENERAL_INFO_VALIDATION + "Pricing Tool must begin with an alphanumeric character.";

            /// <summary>
            /// BOE Tool free text must start with an alphanumeric character.
            /// </summary>
            public const string BOE_TOOL_FORMAT_ERROR = GENERAL_INFO_VALIDATION + "BOE Tool must begin with an alphanumeric character.";

            /// <summary>
            /// The certified cost pricing data is required on the Proposal tab.
            /// </summary>
            public const string CERTIFIED_COST_PRICING_DATA_REQUIRED = GENERAL_INFO_VALIDATION + "\"Is Certified Cost and Pricing Data Required?\" response is required.";

            /// <summary>
            /// The certified cost pricing data is required on the Proposal tab.
            /// </summary>
            public const string CERTIFIED_COST_PRICING_DATA_CLASSIFIED_REQUIRED = GENERAL_INFO_VALIDATION + "\"Is Certified Cost and Pricing Data Classified?\" response is required.";

            /// <summary>
            /// Program/Proposal Status is required
            /// </summary>
            public const string PROGRAM_PROPOSAL_STATUS_REQUIRED = GENERAL_INFO_VALIDATION + "Program/Proposal Status is required.";

            /// <summary>
            /// Response to "What pricing tool will be utilized?" is required
            /// </summary>
            public const string PRICING_TOOL_REQUIRED = GENERAL_INFO_VALIDATION + "\"What pricing tool will be utilized?\" response is required.";

            /// <summary>
            /// Response to "What BOE tool will be utilized?" is required
            /// </summary>
            public const string BOE_TOOL_REQUIRED = GENERAL_INFO_VALIDATION + "\"What BOE tool will be utilized?\" response is required.";

            #endregion Proposal General Info Section

            #region Proposal Info section
            /// <summary>
            /// text to be concated to the front of a validation message 
            /// </summary>
            private const string PROPOSAL_INFO_VALDIATION = "(Proposal Information) - ";

            /// <summary>
            /// proposal title is required
            /// </summary>
            public const string PROPOSAL_TITLE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Proposal Title is required.";

            /// <summary>
            /// proposal title must be unique
            /// </summary>
            public const string PROPOSAL_TITLE_MUST_BE_UNIQUE = PROPOSAL_INFO_VALDIATION + "Proposal Title must be unique.";

            /// <summary>
            ///  propsal type is required
            /// </summary>
            public const string PROPOSAL_TYPE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Proposal Type is required.";

            /// <summary>
            /// customer is required
            /// </summary>
            public const string CUSTOMER_REQUIRED = PROPOSAL_INFO_VALDIATION + "Customer is required.";

            /// <summary>
            /// customer type is required
            /// </summary>
            public const string CUSTOMER_TYPE_REQUIRED = PROPOSAL_INFO_VALDIATION + "End Customer Type is required.";

            /// <summary>
            ///  proposal class is required
            /// </summary>
            public const string PROPOSAL_CLASS_REQUIRED = PROPOSAL_INFO_VALDIATION + "Proposal Class is required.";

            /// <summary>
            ///  type of request is required
            /// </summary>
            public const string TYPE_OF_REQUEST_REQUIRED = PROPOSAL_INFO_VALDIATION + "Type of Request is required.";

            /// <summary>
            /// RFP Number is required
            /// </summary>
            public const string RFP_NUMBER_REQUIRED = PROPOSAL_INFO_VALDIATION + "RFP Number is required.";

            /// <summary>
            /// RFP Issued Date is required
            /// </summary>
            public const string RFP_ISSUED_DATE_REQUIRED = PROPOSAL_INFO_VALDIATION + "RFP Issued Date is required.";

            /// <summary>
            /// RFP Received Date is required
            /// </summary>
            public const string RFP_RECEIVED_DATE_REQUIRED = PROPOSAL_INFO_VALDIATION + "RFP Received Date is required.";

            /// <summary>
            /// RFP issued date format 
            /// </summary>
            public const string RFP_ISSUED_DATE_FORMAT = PROPOSAL_INFO_VALDIATION + "RFP Issued Date format must be mm/dd/yyyy";

            /// <summary>
            /// RFP received date format 
            /// </summary>
            public const string RFP_RECEIVED_DATE_FORMAT = PROPOSAL_INFO_VALDIATION + "RFP Received Date format must be mm/dd/yyyy";

            /// <summary>
            /// Anticipated delivery date is required
            /// </summary>
            public const string DELIVERY_DATE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Anticipated Delivery Date is required.";

            /// <summary>
            /// anticipated delivery date format 
            /// </summary>
            public const string ANTICIPATED_DELIVERY_DATE_FORMAT = PROPOSAL_INFO_VALDIATION + "Anticipated Delivery Date format must be mm/dd/yyyy";

            /// <summary>
            /// anticipated delivery date for Forecasted too close to today's date. 
            /// </summary>
            public const string ANTICIPATED_DELIVERY_DATE_INVALID_FORECAST = PROPOSAL_INFO_VALDIATION + "Anticipated Delivery Date must be more than {0} days away for Forecasted Proposals.";
            
            /// <summary>
            /// revised submittal date 
            /// </summary>
            public const string REVISED_SUBMITTAL_DATE_FORMAT = PROPOSAL_INFO_VALDIATION + "Revised Anticipated Delivery Date format must be mm/dd/yyyy";

            /// <summary>
            /// Space role is required
            /// </summary>
            public const string SSC_ROLE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Space Role is required.";

            /// <summary>
            /// Schedule Proposal is required
            /// </summary>
            public const string SCHEDULE_PROPOSAL_REQUIRED = PROPOSAL_INFO_VALDIATION + "\"New IDIQ contract vehicle\" (i.e., schedule proposal) response is required.";

            /// <summary>
            /// contract type group is required
            /// </summary>
            public const string CONTRACT_TYPE_GROUP_REQUIRED = PROPOSAL_INFO_VALDIATION + "Contract Type Group is required.";

            /// <summary>
            /// contract type is required
            /// </summary>
            public const string CONTRACT_TYPE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Contract Type is required.";

            /// <summary>
            /// Estimated Proposal Value is required
            /// </summary>
            public const string EST_VALUE_REQUIRED = "(Proposal Information) Estimated Proposal Value is required.";

            /// <summary>
            /// Elements of Cost are required
            /// </summary>
            public const string ELEMENTS_OF_COST_REQUIRED = PROPOSAL_INFO_VALDIATION + "Elements of Cost are required.";

            /// <summary>
            /// estimated proposal value is required
            /// </summary>
            public const string EST_PROP_VALUE_REQUIRED = PROPOSAL_INFO_VALDIATION + "Estimated Proposal Value is required.";

            /// <summary>
            /// estimated proposal value vaidation error if value is not a long
            /// </summary>
            public const string EST_PROP_VALUE_IN_DOLLARS = PROPOSAL_INFO_VALDIATION + "Please enter Estimated Proposal Value in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// invalid proposal class set to forecast when linked RDSB document.
            /// </summary>
            public const string INVALID_PROPOSAL_CLASS_FORECAST_DOCUMENT = PROPOSAL_INFO_VALDIATION + "Proposal Class cannot be set to Forecasted because there is a Tailored Rates & Disclosure Document linked to this proposal.";

            /// <summary>
            /// invalid proposal class set to forecast when linked BOE.
            /// </summary>
            public const string INVALID_PROPOSAL_CLASS_FORECAST_BOE = PROPOSAL_INFO_VALDIATION + "Proposal Class cannot be set to Forecasted because there is a BOE linked to this proposal.";

            #endregion Proposal Info section

            #region Proposal Approvals Section
            /// <summary>
            /// text to be concated to the front of a validation message 
            /// </summary>
            private const string APPROVALS_VALDIATION = "(Approvals) - ";

            /// <summary>
            /// Estimator is required
            /// </summary>
            public const string LEAD_ESTIMATOR_REQUIRED = APPROVALS_VALDIATION + "Lead Estimator is required.";

            /// <summary>
            /// Estimator is required
            /// </summary>
            public const string COVER_SHEET_APPROVER_REQUIRED = APPROVALS_VALDIATION + "Cover Sheet Approver is required.";

            /// <summary>
            /// Proposal Manager is required
            /// </summary>
            public const string PRICING_VERIFICATION_REQUIRED = APPROVALS_VALDIATION + "Pricing Verification is required.";

            /// <summary>
            /// Independent Reviewer is required
            /// </summary>
            public const string INDEPENDENT_REVIEWER_REQUIRED = APPROVALS_VALDIATION + "Independent Reviewer is required when CCOPD is set to Yes and the Cover Sheet Approver and Lead Estimator are the same person.";

            /// <summary>
            /// LOB Estimating Lead/Mgr is required
            /// </summary>
            public const string LOB_ESTIMATING_LEAD_MGR_REQUIRED = APPROVALS_VALDIATION + "LOB Estimating Lead/Mgr is required.";

            /// <summary>
            /// Lead Estimator and Independent Reviewer cannot be the same
            /// </summary>
            public const string LEAD_ESTIMATOR_AND_INDEPENDENT_REVIEWER_CANNOT_BE_SAME_PERSON = APPROVALS_VALDIATION + "Lead Estimator and Independent Reviewer cannot be the same person.";

            /// <summary>
            /// The lead estimator and pricing verification cannot be same person.
            /// </summary>
            public const string LEAD_ESTIMATOR_AND_PRICING_VERIFICATION_CANNOT_BE_SAME_PERSON = APPROVALS_VALDIATION + "Lead Estimator and Pricing Verification cannot be the same person.";

            /// <summary>
            /// The lead estimator and LOB Estimating Lead cannot be same person.
            /// </summary>
            public const string LEAD_ESTIMATOR_AND_LOB_LEAD_CANNOT_BE_SAME_PERSON = APPROVALS_VALDIATION + "Lead Estimator and LOB Estimating Lead/Mgr cannot be the same person.";

            /// <summary>
            /// The Cover Sheet Approver is no longer approved
            /// </summary>
            public const string COVER_SHEET_APPROVER_NO_LONGER_APPROVED = APPROVALS_VALDIATION + "The Cover Sheet Approver currently identified in the Proposal Setup is no longer approved for Cover Sheet Approval. Please return to the Proposal Setup tab and select another Cover Sheet Approver. Then select the Approvals tab to re-initiate the Approval Workflow.";

            #endregion

            #region Proposal User Info Section
            /// <summary>
            /// text to be concated to the front of a validation message 
            /// </summary>
            private const string USER_INFO_VALDIATION = "(User Information) - ";

            /// <summary>
            /// capture manager is required
            /// </summary>
            public const string CAPTURE_MANAGER_REQUIRED = USER_INFO_VALDIATION + "Capture Manager is required.";

            /// <summary>
            /// cost volume is required
            /// </summary>
            public const string COST_VOLUME_REQUIRED = USER_INFO_VALDIATION + "Cost Volume Lead is required.";

            /// <summary>
            /// Proposal Manager is required
            /// </summary>
            public const string PROPOSALMGR_REQUIRED = USER_INFO_VALDIATION + "Proposal Manager is required.";

            /// <summary>
            /// contracts POC is required
            /// </summary>
            public const string CONTRACTS_POC_REQUIRED = USER_INFO_VALDIATION + "Contracts POC is required.";

            /// <summary>
            /// supply chain material is required
            /// </summary>
            public const string SUPPLY_CHAIN_MATERIAL_REQUIRED = USER_INFO_VALDIATION + "Materials Lead is required if Elements of Cost includes Materials.";

            /// <summary>
            /// supply chain subcontractor is required
            /// </summary>
            public const string SUPPLY_CHAIN_SUBS_REQUIRED = USER_INFO_VALDIATION + "Subcontracts Lead is required if Elements of Cost includes Subcontractors.";

            /// <summary>
            /// additonal resource 1 type is required
            /// </summary>
            public const string ADDITIONAL_RESOURCE_1_TYPE_REQUIRED = USER_INFO_VALDIATION + "Estimating Resource Type is required if Additional Estimating Resource 1 is identified.";

            /// <summary>
            /// additonal resource 2 type is required
            /// </summary>
            public const string ADDITIONAL_RESOURCE_2_TYPE_REQUIRED = USER_INFO_VALDIATION + "Estimating Resource Type is required if Additional Estimating Resource 2 is identified.";

            /// <summary>
            /// Capture Manager invalid NTID
            /// </summary>
            public const string CAPTURE_MANAGER_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Capture Manager, or the user is not an LM, US based employee.";

            /// <summary>
            /// Cost Volume Lead invalid NTID
            /// </summary>
            public const string COST_VOLUME_LEAD_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Cost Volume Lead, or the user is not an LM, US based employee.";

            /// <summary>
            /// Additional Estimating Resource 1 invalid NTID
            /// </summary>
            public const string ADDITIONAL_PRICING_RESOURCE_1_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Additional Estimating Resource 1, or the user is not an LM, US based employee.";

            /// <summary>
            /// Additional Estimating Resource 2 invalid NTID
            /// </summary>
            public const string ADDITIONAL_PRICING_RESOURCE_2_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Additional Estimating Resource 2, or the user is not an LM, US based employee.";

            /// <summary>
            /// Material Lead invalid NTID
            /// </summary>
            public const string SUPPLY_CHAIN_POC_MATL_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Material Lead, or the user is not an LM, US based employee.";

            /// <summary>
            /// Subcontracts Lead invalid NTID
            /// </summary>
            public const string SUPPLY_CHAIN_POC_SUBS_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Subcontracts Lead, or the user is not an LM, US based employee.";

            /// <summary>
            /// Backup Lead Estimator invalid NTID
            /// </summary>
            public const string BACKUP_PRICER_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Backup Lead Estimator, or the user is not an LM, US based employee.";

            /// <summary>
            /// Contracts Lead invalid NTID
            /// </summary>
            public const string CONTRACTS_POC_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Contracts Lead, or the user is not an LM, US based employee.";

            /// <summary>
            /// Proposal Manager invalid NTID.
            /// </summary>
            public const string PROPOSAL_MANAGER_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Proposal Manager.";

            /// <summary>
            /// Tech Lead invalid NTID.
            /// </summary>
            public const string TECH_LEAD_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Tech Lead, or the user is not an LM, US based employee.";

            /// <summary>
            /// GenBOE Workspace Creator invalid NTID
            /// </summary>
            public const string WORKSPACE_CREATOR_INVALID_NTID = USER_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for GenBOE Workspace Creator.";

            #endregion  Proposal User Info Section
        }

        /// <summary>
        /// Validation messages for Certification Timeline
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class CertificationTimelineValidationConstants
        {
            /// <summary>
            /// The certification timeline comments required message.
            /// </summary>
            public const string COMMENTS_REQUIRED = "Comments are required if more than 5 Days to Certification.";

            /// <summary>
            /// The agreement date required message.
            /// </summary>
            public const string AGREEMENT_DATE_REQUIRED = "Agreement Date is required.";

            /// <summary>
            /// The certification date required message.
            /// </summary>
            public const string CERTIFICATION_DATE_REQUIRED = "Certification Date is required.";

            /// <summary>
            /// The cutoff date utilization required message.
            /// </summary>
            public const string CUTOFF_DATE_UTILIZATION_REQUIRED = "Cut-off Date Utilization is required.";

            /// <summary>
            /// validation message for other and comment
            /// </summary>
            public const string OTHER_REASON_COMMENT_REQUIRED = "When 'Reason Certification is not required' is set to 'Other', the comment is required.";

            /// <summary>
            /// validation message for wrong status and cert not required
            /// </summary>
            public const string CERTIFICATION_NOT_REQUIRED_WRONG_STATE = "Proposal's certification cannot be marked as not-required if the proposal is not in 'Submitted' state.";

            /// <summary>
            /// This should never happen, but just in case.
            /// </summary>
            public const string COMPLETE_FAILED_PROPOSAL = "You cannot complete a proposal with a 'Certification not required' reason. Please save it instead.";
        }

        /// <summary>
        /// Validation messages for proposal setup
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class RevisionValidationConstants
        {
            /// <summary>
            /// proposal title is required
            /// </summary>
            public const string PROPOSAL_TITLE_REQUIRED = "Proposal Title is required.";

            /// <summary>
            /// proposal title must be unique
            /// </summary>
            public const string PROPOSAL_TITLE_MUST_BE_UNIQUE = "Proposal Title must be unique.";

            /// <summary>
            /// Line of Business is required
            /// </summary>
            public const string LINE_OF_BUSINESS_REQUIRED = "Line of Business is required.";

            /// <summary>
            /// Program Area required
            /// </summary>
            public const string PROGRAM_AREA_REQUIRED = "Program Area is required.";

            /// <summary>
            /// Anticipated delivery date is required
            /// </summary>
            public const string DELIVERY_DATE_REQUIRED = "Anticipated Delivery Date is required.";

            /// <summary>
            /// estimated proposal value is required
            /// </summary>
            public const string EST_PROP_VALUE_REQUIRED = "Estimated Proposal Value is required.";

            /// <summary>
            /// estimated proposal value vaidation error if value is not a long
            /// </summary>
            public const string EST_PROP_VALUE_IN_DOLLARS = "Please enter Estimated Proposal Value in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// anticipated delivery date format 
            /// </summary>
            public const string ANTICIPATED_DELIVERY_DATE_FORMAT = "Anticipated Delivery Date format must be mm/dd/yyyy";
        }

        /// <summary>
        /// Validation messages for admin
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class AdminValidationConstants
        {
            /// <summary>
            /// User or Group Name is required
            /// </summary>
            public const string USER_OR_GROUP_IS_REQUIRED = "User/Group name is required.";

            /// <summary>
            /// role is required
            /// </summary>
            public const string ROLE_IS_REQUIRED = "A role is required.";

            /// <summary>
            /// If viewer is selected, line of business is required.
            /// </summary>
            public const string VIEWER_LINE_OF_BUSINESS_REQUIRED = "At least one Line of Business must be selected for the Viewer role.";

            /// <summary>
            /// If Proposal Setup Admin is selected, line of business is required
            /// </summary>
            public const string PROPOSAL_SETUP_ADMIN_LINES_OF_BUSINESS_REQUIRED = "At least one Line of Business must be selected for the Proposal Setup Admin role.";

            /// <summary>
            /// User/Group name is not valid
            /// </summary>
            public const string USER_OR_GROUP_NAME_IS_NOT_VALID = "{0} is not a valid User/Group name.";

            /// <summary>
            /// Created data format is wrong
            /// </summary>
            public const string CREATED_DATE_FORMAT = "Created Date format must be mm/dd/yyyy";

            /// <summary>
            /// Start Date must be before End Date
            /// </summary>
            public const string START_DATE_BEFORE_END_DATE = "Start Date must be before the End Date.";
        }

        /// <summary>
        /// Validation messages for checklist
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ChecklistValidationConstants
        {
            #region Checklist General Section
            /// <summary>
            /// text to be concated to the front of a validation message 
            /// </summary>
            private const string GENERAL_INFO_VALDIATION = "(General Information) - ";

            /// <summary>
            /// Proposal Submittal Date is required
            /// </summary>
            public const string SUBMITTAL_DATE_REQUIRED = GENERAL_INFO_VALDIATION + "Proposal Submittal Date is required.";

            /// <summary>
            /// Proposal Submitted Value is required
            /// </summary>
            public const string SUBMITTED_VALUE_REQUIRED = GENERAL_INFO_VALDIATION + "Proposal Submitted Value is required.";

            /// <summary>
            /// proposal submittal date format 
            /// </summary>
            public const string SUBMITTAL_DATE_FORMAT = GENERAL_INFO_VALDIATION + "Proposal Submittal Date format must be mm/dd/yyyy";

            /// <summary>
            /// Peer Reviewer invalid NTID
            /// </summary>
            public const string PEER_REVIEWER_INVALID_NTID = GENERAL_INFO_VALDIATION + "Invalid NT ID or User name not found in the Global Address List (GAL) for Peer Reviewer.";

            #endregion Checklist General Section
            #region Checklist Proposal Pricing Data
            /// <summary>
            /// text to be used to the front of a validation message 
            /// </summary>
            private const string PROPOSAL_PRICING_DATA_VALDIATION = "(Proposal Pricing Data) - ";

            /// <summary>
            /// total price should be a whole number
            /// </summary>
            public const string TOTAL_PRICE_WHOLE_NUMBERS = "(General Information) - Submitted Value must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// absolute value should be a whole number
            /// </summary>
            public const string ABSOLUTE_VALUE_WHOLE_NUMBERS = "If Given, Absolute Value must be in U.S. whole dollars, and a positive number.";

            /// <summary>
            /// lm labor cost should be a whole number
            /// </summary>
            public const string LM_LABOR_HRS_INVALID_RANGE = PROPOSAL_PRICING_DATA_VALDIATION + "LM Labor Hrs must be in the range +- 999,999,999.99 (2 decimal places max).";

            /// <summary>
            /// lm labor cost should be a whole number
            /// </summary>
            public const string LM_LABOR_COST_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "LM Labor Cost must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// subcontractor cost should be a whole number
            /// </summary>
            public const string SUBCONTRACTOR_COST_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "Subcontractor Cost must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// material cost should be a whole number
            /// </summary>
            public const string MATERIAL_COST_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "Material Cost must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// iwta cost should be a whole number
            /// </summary>
            public const string IWTA_COST_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "IWTA Cost must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// travel cost should be a whole number
            /// </summary>
            public const string TRAVEL_COST_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "Travel Cost must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// other direct costs should be a whole number
            /// </summary>
            public const string OTHER_DIRECT_COSTS_WHOLE_NUMBERS = PROPOSAL_PRICING_DATA_VALDIATION + "Other Direct Costs must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// profit/fee + com is a whole number
            /// </summary>
            public const string PROFIT_FEE_COM_WHOLE_NUMBER = PROPOSAL_PRICING_DATA_VALDIATION + "Profit/Fee + COM must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// ROS % is between 0.00 and 99.99
            /// </summary>
            public const string ROS_PERCENT_RANGE = PROPOSAL_PRICING_DATA_VALDIATION + "ROS% must be between 0.00 and +-99.99 (2 decimal places max).";

            #endregion Checklist Proposal Pricing Data

            #region Checklist PPR Data
            /// <summary>
            /// PPR checklist general validation message
            /// </summary>
            private const string CHECKLIST_PPR_VALDIATION = "(Proposal Pricing Review) - ";

            /// <summary>
            /// All PPR questions must be answered
            /// </summary>
            public const string ALL_PPR_MUST_BE_ANSWERED = CHECKLIST_PPR_VALDIATION + "All questions must be answered.";

            /// <summary>
            /// if No was answered, a Estimator comment is needed
            /// </summary>
            public const string PRICER_COMMENT_NEEDED = CHECKLIST_PPR_VALDIATION + "A comment is required to explain any \"No\" responses";

            /// <summary>
            /// If the Estimator changes the answer to question one while the Peer is in the process of editing the PAR section and attempts to save.
            /// </summary>
            public const string PRICER_UPDATED_QUESTION_ONE_OF_PPR_WHILE_PEER_EDITING = "The PAR document cannot be saved.  The Lead Estimator has updated question 1 of the PPR since the time you opened the checklist, making the proposal exempt from completing the PAR document.  Refresh the page to get the latest updates.";
            #endregion Checklist PPR Data
            #region Checklist PAR Data
            /// <summary>
            /// PAR checklist general validation message
            /// </summary>
            private const string CHECKLIST_PAR_VALIDATION = "(Proposal Adequacy Review) - ";

            /// <summary>
            /// All PAR questions must be answered
            /// </summary>
            public const string ALL_PAR_MUST_BE_ANSWERED = CHECKLIST_PAR_VALIDATION + "All questions must be answered.";

            /// <summary>
            /// if No was answered, a Estimator/peer comment is needed
            /// </summary>
            public const string PAR_COMMENT_NEEDED = CHECKLIST_PAR_VALIDATION + "A comment is required to explain any \"No\" responses";

            /// <summary>
            /// if No was answered, a Estimator/peer comment is needed
            /// </summary>
            public const string PAR_ROW_COMMENT_NEEDED = CHECKLIST_PAR_VALIDATION + "A comment is required to explain all \"No\" responses for each row.";

            /// <summary>
            /// if Yes was answered, a Estimator page number reference is needed
            /// </summary>
            public const string PAR_ROW_PAGE_NUMBER_NEEDED = CHECKLIST_PAR_VALIDATION + "A proposal page number is required to explain all \"Yes\" responses for each row.";

            /// <summary>
            /// If a '~' or a '^' character is used in the PAR row PageNumber and Comment fields, post an error since these characters are
            /// used as field and row delimiters (respectively).  For Estimator, speaks to both page number and comment columns.
            /// </summary>
            public const string PAR_ROW_INVALID_CHARACTER_PRICER = CHECKLIST_PAR_VALIDATION + "1 or more unallowed '~' or '^' characters were used to populate the row Page Number or Comment fields.";

            /// <summary>
            /// If a '~' or a '^' character is used in the PAR row PageNumber and Comment fields, post an error since these characters are
            /// used as field and row delimiters (respectively). For Peer, speaks to only comment column.
            /// </summary>
            public const string PAR_ROW_INVALID_CHARACTER_PEER = CHECKLIST_PAR_VALIDATION + "1 or more unallowed '~' or '^' characters were used to populate the row Comment fields.";

            /// <summary>
            /// If a row is set to Yes Only, then Yes must be selected
            /// </summary>
            public const string PAR_ROW_YES_NEEDED = CHECKLIST_PAR_VALIDATION + "A \"Yes\" is required to be selected for all rows where \"Yes\" is the only valid response.";

            /// <summary>
            /// If No is selected for a row, a canned response is required
            /// </summary>
            public const string PAR_ROW_CANNED_RESPONSE_NEEDED = CHECKLIST_PAR_VALIDATION + "A canned response is required to explain all \"No\" responses for each row.";

            /// <summary>
            /// If the selected canned response is "Other", then a comment is required
            /// </summary>
            public const string PAR_ROW_OTHER_COMMENT_NEEDED = CHECKLIST_PAR_VALIDATION + "A comment is required in all rows where canned response \"Other\" is selected.";

            /// <summary>
            /// Earliest checklist version using canned responses
            /// </summary>
            public const int CANNED_RESPONSE_CHECKLIST_VERSION = 12;

            /// <summary>
            /// ID of canned response "Other"
            /// </summary>
            public const int OTHER_CANNED_RESPONSE_ID = 999;

            /// <summary>
            /// Warning text for when "Other" is selected
            /// </summary>
            public const string OTHER_WARNING_TEXT = "Warning: This selection is atypical. Please coordinate all \"Other\" responses with Central Estimating prior to overtyping this warning with your reason for selecting \"No.\"";
            #endregion Checklist PAR Data
        }

        /// <summary>
        /// Validation messages for manage proposal info
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ManageProposalInfoValidationConstants
        {
            /// <summary>
            /// Proposal Submittal Date is required
            /// </summary>
            public const string SUBMITTAL_DATE_REQUIRED = "Proposal Submittal Date is required.";

            /// <summary>
            /// Proposal Submittal Date format 
            /// </summary>
            public const string SUBMITTAL_DATE_FORMAT = "Proposal Submittal Date format must be mm/dd/yyyy.";

            /// <summary>
            /// Total Price is required
            /// </summary>
            public const string TOTAL_PRICE_REQUIRED = "Total Price is required.";

            /// <summary>
            /// Total Price format
            /// </summary>
            public const string TOTAL_PRICE_FORMAT = "Total Price must be in U.S. whole dollars in the range +- 999,999,999,999.";

            /// <summary>
            /// Checklist Submitted Date (Estimator) is required
            /// </summary>
            public const string CHECKLIST_DATE_PRICER_REQUIRED = "Checklist Submitted Date (Lead Estimator) is required.";

            /// <summary>
            /// Checklist Submitted Date (Estimator) format
            /// </summary>
            public const string CHECKLIST_DATE_PRICER_FORMAT = "Checklist Submitted Date (Lead Estimator) format must be mm/dd/yyyy.";

            /// <summary>
            /// Checklist Submitted Date (Peer) is required
            /// </summary>
            public const string CHECKLIST_DATE_PEER_REQUIRED = "Checklist Submitted Date (Peer) is required.";

            /// <summary>
            /// Checklist Submitted Date (Peer) format
            /// </summary>
            public const string CHECKLIST_DATE_PEER_FORMAT = "Checklist Submitted Date (Peer) format must be mm/dd/yyyy.";
        }

        /// <summary>
        /// Validation messages for the proposal log report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProposalLogReportValidationConstants
        {
            /// <summary>
            /// Proposal Status is required
            /// </summary>
            public const string PROPOSAL_STATUS_REQUIRED = "Proposal Status is required.";

            /// <summary>
            /// specific proposal selection is required
            /// </summary>
            public const string SPECIFIC_PROPOSAL_SELECTION_REQUIRED = "Specific Proposal selection is required.";

            /// <summary>
            /// submit date range section
            /// </summary>
            public const string SUBMIT_DATE_RANGE_SECTION = "(Submit Date Range) - ";

            /// <summary>
            /// submit start date range Date format 
            /// </summary>
            public const string SUBMIT_START_NOT_VALID_DATE_FORMAT = SUBMIT_DATE_RANGE_SECTION + "Start Date is not a valid date format.";

            /// <summary>
            /// submit end date range Date format 
            /// </summary>
            public const string SUBMIT_END_NOT_VALID_DATE_FORMAT = SUBMIT_DATE_RANGE_SECTION + "End Date is not a valid date format.";

            /// <summary>
            /// submit date range is required
            /// </summary>
            public const string SUBMIT_DATE_REQUIRED = SUBMIT_DATE_RANGE_SECTION + "Date Range is required.";

            /// <summary>
            /// submit date range's start date has to be before end date
            /// </summary>
            public const string SUBMIT_START_DATE_RANGE = SUBMIT_DATE_RANGE_SECTION + "Start Date has to be before End Date.";

            /// <summary>
            /// tracking number section
            /// </summary>
            public const string TRACKING_NUMBER_SECTION = "(Tracking # begins with) – ";

            /// <summary>
            /// tracking number required
            /// </summary>
            public const string TRACKING_NUMBER_REQUIRED = TRACKING_NUMBER_SECTION + "Tracking # is required.";

            /// <summary>
            /// tracking number format
            /// </summary>
            public const string TRACKING_NUMBER_FORMAT = TRACKING_NUMBER_SECTION + "Please enter a valid Tracking #. Tracking number can only contain numbers, letters, and dashes, e.g., 2013-12345, 18-12345, or F-18-12.";
        }

        /// <summary>
        /// Validation messages for the proposal log report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProposalActivityReportValidationConstants
        {
            /// <summary>
            /// specific customer type/program area selection is required
            /// </summary>
            public const string SPECIFIC_CUSTOMER_TYPE_PROGRAM_AREA_SELECTION_REQUIRED = "End Customer Type / Program Area selection is required.";

            /// <summary>
            /// create date range section
            /// </summary>
            public const string CREATE_DATE_RANGE_SECTION = "(Create Date Range) - ";

            /// <summary>
            /// create start date range Date format 
            /// </summary>
            public const string CREATE_START_NOT_VALID_DATE_FORMAT = CREATE_DATE_RANGE_SECTION + "Start Date is not a valid date format.";

            /// <summary>
            /// create end date range Date format 
            /// </summary>
            public const string CREATE_END_NOT_VALID_DATE_FORMAT = CREATE_DATE_RANGE_SECTION + "End Date is not a valid date format.";

            /// <summary>
            /// create date range's start date has to be before end date
            /// </summary>
            public const string CREATE_START_DATE_RANGE = CREATE_DATE_RANGE_SECTION + "Start Date has to be before End Date.";

            /// <summary>
            /// if either the create start or end date is filled out, the other one must be filled out too
            /// </summary>
            public const string INCOMPLETE_CREATE_DATE_RANGE = CREATE_DATE_RANGE_SECTION + "Incomplete date range specified.";

            /// <summary>
            /// Estimator invalid NTID
            /// </summary>
            public const string PRICER_INVALID_NTID = "(Lead Estimator) - Invalid NTID.";
        }

        /// <summary>
        /// Validation messages for the DFARS report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class DfarsReportValidationConstants
        {
            /// <summary>
            /// Approval Workflow Completed Date Range Section
            /// </summary>
            public const string DATE_RANGE_SECTION = "(Actual Submittal Date Range) - ";

            /// <summary>
            /// Approval Workflow Completed Start Date invalid format
            /// </summary>
            public const string START_DATE_INVALID_FORMAT = DATE_RANGE_SECTION + "Start Date is not a valid date format.";

            /// <summary>
            /// Approval Workflow Completed End Date invalid format
            /// </summary>
            public const string END_DATE_INVALID_FORMAT = DATE_RANGE_SECTION + "End Date is not a valid date format.";

            /// <summary>
            /// Approval Workflow Completed Start Date must come before End Date
            /// </summary>
            public const string START_BEFORE_END_DATE = DATE_RANGE_SECTION + "Start Date has to be before End Date.";

            /// <summary>
            /// Approval Workflow Completed Date Range is incomplete
            /// </summary>
            public const string INCOMPLETE_DATE_RANGE = DATE_RANGE_SECTION + "Incomplete date range specified. Both Start and End Dates are needed.";

            /// <summary>
            /// A Program Area has been selected without its matching Line of Business
            /// </summary>
            public const string INVALID_PROGRAM_AREA = "(Program Area) - Line of Business {0} must also be selected when selecting Program Area {1}.";
        }

        /// <summary>
        /// Validation messages for the Export Proposals report
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ExportProposalReportValidationConstants
        {
            /// <summary>
            /// estimated ship date range section
            /// </summary>
            public const string SUBMIT_DATE_RANGE_SECTION = "(Date Range) - ";

            /// <summary>
            /// submit start date invalid date format  
            /// </summary>
            public const string SUBMIT_START_DATE_INVALID_DATE_FORMAT = SUBMIT_DATE_RANGE_SECTION + "Start Date is not a valid date format.";

            /// <summary>
            /// submit end date invalid date format  
            /// </summary>
            public const string SUBMIT_END_DATE_INVALID_DATE_FORMAT = SUBMIT_DATE_RANGE_SECTION + "End Date is not a valid date format.";
        }

        /// <summary>
        /// Validation messages for the delete proposal action
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class DeleteProposalValidationConstants
        {
            /// <summary>
            /// ProposalId invalid
            /// </summary>
            public const string NULL_PROPOSAL_ID = "Delete operation failed. Proposal Id was null.";

            /// <summary>
            /// ProposalId invalid
            /// </summary>
            public const string INVALID_PROPOSAL_ID = "Delete operation failed. Proposal could not be found.";

            /// <summary>
            /// Proposal linked to an RDSB document
            /// </summary>
            public const string HAS_LINKED_RDSB_DOCUMENT = "Delete operation not allowed. Proposal is linked to an RDSB document.";

            /// <summary>
            /// Proposal linked to a BOE workspace 
            /// </summary>
            public const string HAS_LINKED_BOE_WORKSPACE = "Delete operation not allowed. Proposal is linked to a BOE workspace.";
        }

        /// <summary>
        /// Validation messages for Proposal Revisions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ProposalRevisionConstants
        {
            /// <summary>
            /// Approval Workflow is not complete
            /// </summary>
            public const string WORKFLOW_NOT_COMPLETED = "Approval Workflow must be completed before adding a new Revision.";

            /// <summary>
            /// Certification Timeline cannot be completed
            /// </summary>
            public const string CERT_TIMELINE_COMPLETE = "Certification Timeline cannot be completed before adding a new Revision.";

            /// <summary>
            /// Proposal can't have "Revised" status/must be most recent revision, or original if not revised
            /// </summary>
            public const string NOT_LATEST_VERSION = "A new Revision cannot be added from a Proposal that is alread Revised. It can only be added from the latest version of the Proposal.";

            /// <summary>
            /// A Revision can only be created by the Lead Estimator or Backup Estimator
            /// </summary>
            public const string NOT_PERMITTED = "Only the Lead Estimator or Backup Estimator is permitted to create a new Revision of a Proposal.";
        }
    }
}
