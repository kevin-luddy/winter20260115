// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;

    public static class ValidationConstants
    {
        // Since these are used in data annotations, they must be const... readonly static will give a compiler error
        public const string WBS_NUMBER = @"^[a-zA-Z0-9]{1,5}(\.[a-zA-Z0-9]{1,5}){0,9}$";
        public const string DATE_MONTH_YEAR = @"^([0]?[1-9]|1[0-2])/([1-2]\d{3})$";
        public const string DATE_FULL = @"^([0]?[1-9]|1[0-2])/([0]?[1-9]|[12][0-9]|3[01])/([1-2]\d{3})$";
        public const string RATE_DECIMAL = @"^([0-9]{1,4}(\.[0-9]{1,3}){0,1})$";
        public const string RESOURCE_RATE_DECIMAL = @"^([0-9]{0,3}(\.[0-9]{1,2}){0,1})$";
        public const string TM_RESOURCE_RATE_DECIMAL = @"^([0-9]{0,4}(\.[0-9]{1,2}){0,1})$";
        public const string MISC_RATE_DECIMAL = @"^([0-9]{1,5}(\.[0-9]{1,2}){0,1})$";

        public const string NO_SPACES = @"^\S*$";

        public const int MAX_BOE_DESC_LENGTH = 500000;
        public const int MAX_SOURCES_OF_DATA_LENGTH = 500000;
        public const int MAX_TASK_DESC_LENGTH = 500000;
        public const int MAX_MOQ_TEXT_LENGTH = 500000;
        public const int MAX_TASK_TITLE_LENGTH = 100;
        public const int MAX_RTE_LENGTH = 500000;

        /// <summary>
        /// Text for Proposal Class being required
        /// </summary>
        public const string PROPOSAL_CLASS_REQUIRED = "Proposal Class is required.";

        /// <summary>
        /// Maximum number of characters supported for BOE title
        /// </summary>
        public const int MAX_BOE_TITLE_LENGTH = 100;
        public const int BOE_SOURCE_OF_DATA_CHAR_LIMIT = 2000;
        public const long RESOURCE_SPREAD_HOURS_LOW_LIMIT = -9999999999;
        public const long RESOURCE_SPREAD_HOURS_HIGH_LIMIT = 9999999999;
        public const Decimal RESOURCE_SPREAD_COST_LOW_LIMIT = -9999999999.99m;
        public const Decimal RESOURCE_SPREAD_COST_HIGH_LIMIT = 9999999999.99m;
        public const int MOQ_HOURS_EQUATION_LENGTH_LIMIT = 250;

        //Validation Messages for MultiBOE
        public const string MULTI_BOE_WBS_ASSIGNED = "Non Multi BOE cannot have Multi WBS assigned to it";
        public const string MULTI_BOE_CLIN_ASSIGNED = "Non Multi BOE cannot have Multi CLIN assigned to it";
        public const string MULTI_BOE_NEEDS_WBS = "Multi BOE needs Multi WBS assigned to it";
        public const string MULTI_BOE_NEEDS_CLIN = "Multi BOE needs Multi CLIN assigned to it";
        public const string MULTI_BOE_MATERIAL_BOE = "BOE cannot be a Multi BOE and a Material BOE";
    }
}
