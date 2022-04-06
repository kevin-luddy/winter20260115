// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.classes
{
    /// <summary>
    /// String constants that can be used through the solution.
    /// </summary>
    static public class CommonConstants
    {
        // the system/global resource list id value
        readonly static public int GLOBAL_LIST_ID = 1;

        // value used to represent the "All" option in the Workspace Search screen
        readonly static public int PROPOSAL_ALL_SEARCH_VALUE = 7;

        /// <summary>
        /// Prefix all with FORMAT_STRING
        /// </summary>
        readonly static public string FORMAT_STRING_FULL_DATE_TIME = "M/dd/yyyy h:mm tt";

        readonly static public string BOE_SUBMITTED_FOR_APPROVAL_TEXT = "Submitted for approval";

        // suffix to append to Subcontractor Author Names for display purposes
        readonly static public string SUBCONTRACTOR_AUTHOR_SUFFIX = " (Sub)";

        readonly static public string SIGNED = "/signed/";

        /// <summary>
        /// The database Id of the global performing organization list.  All new workspaces are created with this list.
        /// </summary>
        readonly static public int GLOBAL_PERFORMING_ORG_LIST_ID = 1;

        /// <summary>
        /// When a string to be exported to excel needs to be force as a cell format of type string, this is
        /// prepended to the value and stripped off prior to setting the cells value to string type.
        /// </summary>
        readonly static public string FORCE_AS_STRING_VALUE = "#?";

        /// <summary>
        /// When a string to be exported to excel needs to be forced as a cell format of type Number, this is
        /// prepended to the value and stripped off prior to setting the cells value to string type.
        /// </summary>
        readonly static public string FORCE_AS_NUMBER_FOR_EXCEL = "FORCE_EXCEL_NUMBER";

        /// <summary>
        /// Set the text in the export as bold
        /// </summary>
        readonly static public string SET_AS_BOLD_FOR_EXCEL = "SET_BOLD_EXCEL";

        /// <summary>
        /// An additional header required for ISGS on the Manage BOE page.
        /// </summary>
        readonly static public string MANAGE_BOE_HEADER_ISGS = "Note:  If there are multiple functional estimates in one BOE, a unique functional approver must be assigned to approve each functional estimate (i.e. a BOE with estimates for PM, Finance & Contracts requires 3 functional approvers).";

        /// <summary>
        /// Suffix for the UI Id used to identify the search metrics dialog for MST.
        /// </summary>
        readonly static public string MSTMetricsDialogSuffix = "MST";

        /// <summary>
        /// Suffix for the UI Id used to identify the search metrics dialog for SpaceSystems and IS&GS.
        /// </summary>
        readonly static public string CommonMetricsDialogSuffix = "Common";

        /// <summary>
        /// Metric Search dialog title for SpaceSystems.
        /// </summary>
        readonly static public string CommonDialogTitle = "Historical Metrics Search";

        /// <summary>
        /// Metric Search dialog title for MST.
        /// </summary>
        readonly static public string MSTDialogTitle = "Historical Measures: Search";

        readonly static public string Unassigned_CLIN_Display_Text = "NO CLIN";
        readonly static public string Unassigned_WBS_Display_Text = "NO WBS";

        #region Label Constants
        readonly static public string BOE_MOQ_TEXT_LABEL = "MOQ Text";
            readonly static public string BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS = "MOQ Rationale";
            readonly static public string BOE_MOQ_EQUATION_LABEL = "MOQ Equation";
            readonly static public string BOE_MOQ_TYPES_HELP_TEXT_ISGS = @"<b>Standard</b> - Unbiased assessment of resource requirements.<br /><br /><b>Estimating Relationships</b> - Statistically analyze  historical data to develop mathematically fitted functions. Variables are logically related, statistically valid, verifiable. Includes parametric models.<br /><br /><b>Probability</b>  - Makes provision for uncertainty (random occurrence and dependency). Recognizes that estimate may be affected by other activities that must occur first. <br /><br /><b>Factor</b> - Summation of individual components of cost  to a constant unit of measure. <br /><br /><b>Unit</b> - Cost per unit. <br /><br /><b>Comparison</b> - Compare items being estimated to items of similar configuration with known costs. Adjustment factors for differences in complexity and/or size may need to be applied. <br /><br /><b>Judgment</b> - Relies solely on experience, common sense and knowledge in the absence of historical data. <br /><br /><b>Level of Effort</b> - Customer directed level of effort  over a stated period of time. Direction may be in the form of FFP-LOE or CPFF-LOE contract or documented in an SOW for a completion contract.<br /><br /><b>Quote</b>  - Supplier provided estimate.";
            readonly static public string BOE_MOQ_TYPES_HELP_TEXT_SPACE_SYSTEMS = @"<b>Analogy/similar to</b> - The analogy/similar to technique uses an analogous comparison between a previously completed effort and the new effort being estimated. The criterion is that the historical program being used for the “analogy” is predominately “similar to” and relevant to the new effort in question.<br /><br /><b>Bottom-up</b> - The bottom-up technique has historically been a subjective assessment of the labor effort required based on professional engineering judgment.<br /><br /><b>Cost estimating relationships (CERs)</b> - CERs mathematically relate cost (dependent variable) to physical/technical/cost characteristics (independent variables) of similar hardware items.<br /><br /><b>Historical experience factor (HEF)</b> - Historical Experience Factor (HEF) is used to derive related estimates for a current estimate. The HEF should be adjusted for any differences in complexity, production units, contract performance period, and for any other factors that influence the validity of the current estimate.<br /><br /><b>Labor standards and realization/performance factors</b> - The estimate is based on company or industry time standards for defined tasks, and modified by the application of performance or realization factors to the standards.<br /><br /><b>Level-of-effort (LOE)/support</b> - Generally limited to specific contracting situations (such as Level-of-Effort (LOE) or Time and Materials (T&M) contracts) but may also be appropriate for estimating certain elements of completion-type contract proposals, particularly when relevant historical cost data and attributes are limited or non-existent.<br /><br /><b>Data-driven cost models/equations</b> - Data-driven cost models combine multiple CERs or HEFs into an estimating system that links various product estimates through historical factors, cost-cost, discrete person loading, or other relationships.<br /><br /><b>Actual</b> - Actual Cost or Hours incurred on the program.<br /><br /><b>Quote</b> - Supplier provided estimate.";
            readonly static public string BOE_MOQ_TYPES_HELP_TEXT_MST = @"<b>Historical Performance</b> - Extrapolation of actual hours/costs based on prior performance of a similar activity.<br /><br /><b>Comparison/Analogy Method</b> - Estimate based on historical performance from a similar effort with adjustments for programmatic or technical.<br /><br /><b>Cost Estimating Relationships (CERs)/Historical Factors</b> -  Equations developed based on historical or statistically correlated relationship and valid explanation for the correlation (causal relationship).<br /><br /><b>Parametric Cost Models</b> - Use of calibrated, internally developed, or commercially available cost models.<br /><br /><b>Standard Time Estimating</b> - Labor Standards and Realization/ Performance Factors - time necessary to complete a defined element of work following a prescribed (recognized) technique or method.<br /><br /><b>Factor/Unit Method</b> - Generally a simple ratio (not always statistically validated).<br /><br /><b>Level-of-Effort/Support</b> - Uses resource-based estimates that are often based on a predetermined level of support for a given period of time.<br /><br /><b>Engineering/Judgmental Estimates</b> - Based on subject matter expert (SME) experience with similar efforts. This is the least preferable methodology to use.";
            readonly static public string LABEL_TEXT_LEAD_PRICER_SSC = "Estimating Lead/Pricer";
            readonly static public string LABEL_TEXT_SOURCE_OF_DATA_SSC = "Sources of Data";
            readonly static public string LABEL_TEXT_SOURCE_OF_DATA_ISGS = "Sources of Data **";
        #endregion

        /// <summary>
        /// Work in Progress constant.
        /// </summary>
        readonly static public string WorkInProgress = "Work In Progress";

        /// <summary>
        /// Previous Version constant
        /// </summary>
        readonly static public string PreviousVersion = "Previous Revision";

        /// <summary>
        /// ProPricer rate mappings property name base.
        /// </summary>
        readonly static public string RateDescriptionPropertyBase = "RateDescription";

        /// <summary>
        /// The number of Rate Table Years that display for RDM.
        /// </summary>
        readonly static public int RATE_TABLE_YEARS_TO_DISPLAY = 5;

        /// <summary>
        /// The number of years to adjust the Rate Table Years that display for RDM for backward-looking categories.
        /// </summary>
        readonly static public int RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT = 1;

        /// <summary>
        /// The automatic system backup name constant
        /// </summary>
        readonly static public string AUTO_SYSTEM_BACKUP_DAILY = "[SYS DAILY]";

        /// <summary>
        /// The automatic system backup deletion boes threshold
        /// </summary>
        public static readonly int AUTO_SYSTEM_BACKUP_DELETION_BOES_THRESHOLD = 5;

        /// <summary>
        /// The automatic system backup prior to deletion of boes
        /// </summary>
        public static readonly string AUTO_SYSTEM_BACKUP_DELETION_BOES = "[SYS BOE DELETION]";

        /// <summary>
        /// The system user identifier
        /// </summary>
        public static readonly int SYSTEM_USER_ID = 0;

        /// <summary>
        /// The automatic system backup prior to Template Assignment
        /// </summary>
        public static readonly string AUTO_SYSTEM_BACKUP_TEMPLATE_ASSIGN_CHANGE = "[SYS TEMPLATE ASSIGN CHANGE]";

        /// <summary>
        /// The automatic system backup prior to Template Prompt Deletion
        /// </summary>
        public static readonly string AUTO_SYSTEM_BACKUP_TEMPLATE_PROMPT_DELETE = "[SYS TEMPLATE PROMPT DEL]";

    }
}
