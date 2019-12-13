// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Validation
{
    /// <summary>
    /// Validation constants for Admin.
    /// </summary>
    public static class AdminValidationConstants
    {
        /// <summary>
        /// The rate code from column
        /// </summary>
        public const string RATE_CODE_FROM_COLUMN = "From";

        /// <summary>
        /// The rate code to column
        /// </summary>
        public const string RATE_CODE_TO_COLUMN = "To";

        /// <summary>
        /// The rate code for TO must be unique.
        /// </summary>
        public const string RATE_CODE_TO_UNIQUE = "Rate Code {0} must be unique in the TO column.";

        /// <summary>
        /// The rate code FROM and TO columns must not match.
        /// </summary>
        public const string RATE_CODE_FROM_TO_MATCH = "Rate Code {0} was used in both the FROM and TO columns.";

        /// <summary>
        /// The rate code TO/From rows are always required.
        /// </summary>
        public const string RATE_CODE_REQUIRED = "The {0} column is required in all replication rows.";

        /// <summary>
        /// The rate code is invalid.
        /// </summary>
        public const string RATE_CODE_INVALID = "The Rate Code {0} in the {1} column is invalid.";

        /// <summary>
        /// There are no Cobra configurations.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_REQUIRED = "At least one Cobra Year Configuration must be provided.";

        /// <summary>
        /// Years with missing or invalid Cobra Dates.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_MISSING_OR_INVALID_COBRA_DATE = "The following year(s) have a missing or invalid corresponding Cobra Date:  {0}.";

        /// <summary>
        /// Cobra Dates are missing Years.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_MISSING_YEAR = "The following Cobra Date(s) are missing a year:  {0}.";

        /// <summary>
        /// A Year has been duplicated.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_DUPLICATE_YEARS = "The following year(s) are being used for more than one Cobra Date:  {0}.";

        /// <summary>
        /// A Cobra Date has been duplicated.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_DUPLICATE_COBRA_DATES = "The following Cobra Date(s) are being used for more than one year:  {0}.";

        /// <summary>
        /// The combination of Year and Cobra Date is invalid.
        /// </summary>
        public const string COBRA_YEAR_CONFIGURATION_INVALID_DATE_COMBINATION = "The 'year' portion of the Cobra Date should either match the Year column or should match the previous year. The following year(s) have an invalid Cobra Date:  {0}.";

        /// <summary>
        /// The COBRA Rate Set field is not populated.
        /// </summary>
        public const string COBRA_MAPPING_MISSING_RATE_SET = "The following Rate Code(s) are missing a COBRA Rate Set value: {0}.";

        /// <summary>
        /// The COBRA Code1 field is not populated.
        /// </summary>
        public const string COBRA_MAPPING_MISSING_CODE_1 = "The following Rate Code(s) are missing a COBRA Code 1 value: {0}.";

        /// <summary>
        /// Minimum value for start or end year.
        /// </summary>
        public const int RATE_YEAR_CONFIGURATION_MIN_YEAR = 2000;

        /// <summary>
        /// Maximum value for start or end year.
        /// </summary>
        public const int RATE_YEAR_CONFIGURATION_MAX_YEAR = 2200;

        /// <summary>
        /// The start or end year is not numeric.
        /// </summary>
        public const string RATE_YEAR_CONFIGURATION_YEAR_NOT_NUMERIC = "The start and end year values must be numeric.";

        /// <summary>
        /// The start or end year is out of range.
        /// </summary>
        public const string RATE_YEAR_CONFIGURATION_YEAR_OUT_OF_RANGE = "The {0} year must be in the range {1} to {2}.";

        /// <summary>
        /// The start and end years are out-of-order.
        /// </summary>
        public const string RATE_YEAR_CONFIGURATION_YEARS_OUT_OF_ORDER = "Start year must be earlier than end year.";

        /// <summary>
        /// Picklist value is a duplicate
        /// </summary>
        public const string PICKLIST_VALUE_MUST_BE_UNIQUE = "Pick List value '{0}' is not unique (case insensitive).";

        /// <summary>
        /// The picklist item may not be deleted
        /// </summary>
        public const string PICKLIST_ITEM_MAY_NOT_BE_DELETED = "Pick List value '{0}' cannot be deleted because it has children: {1}.";

        /// <summary>
        /// The picklist is missing a parent.
        /// </summary>
        public const string PICKLIST_MISSING_PARENT = "Pick List value '{0}' needs a valid Parent selected.";

        /// <summary>
        /// Picklist items may not be updated or deleted
        /// </summary>
        public const string READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED = "Read-only items may not be updated or deleted.";
    }
}
