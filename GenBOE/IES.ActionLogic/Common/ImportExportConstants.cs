// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Import/Export Constants
    /// </summary>
    public static class ImportExportConstants
    {
        /// <summary>
        /// Path to Rate Code template
        /// </summary>
        public static readonly string PATH_TO_RATE_CODE_TEMPLATE = "~/Templates/Export/RateCodesImportExample.xlsm";

        /// <summary>
        /// Worksheet names - Rate Codes
        /// </summary>
        public static readonly string RATE_CODES = "Rate Codes";

        /// <summary>
        /// Worksheet names - Options Lists
        /// </summary>
        public static readonly string OPTIONS_LISTS = "Options Lists";

        // Workoffline template table offsets
        // Offsets are from the top left corner of the table

        /// <summary>
        /// Rate Category Column Offset
        /// </summary>
        public const int RATE_CATEGORY_CELL_COLUMN_OFFSET = 0;

        /// <summary>
        /// Linked Section Column Offset
        /// </summary>
        public const int SECTION_CELL_COLUMN_OFFSET = 3;

        /// <summary>
        /// Resource Type Column Offset
        /// </summary>
        public const int RESOURCE_TYPE_CELL_COLUMN_OFFSET = 4;

        /// <summary>
        /// Rate Type Column Offset
        /// </summary>
        public const int RATE_TYPE_CELL_COLUMN_OFFSET = 5;

        /// <summary>
        /// ProPricer Resource Class Column Offset
        /// </summary>
        public const int RESOURCE_CLASS_CELL_COLUMN_OFFSET = 7;

        /// <summary>
        /// ProPricer Resource Class1 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS1_CELL_COLUMN_OFFSET = 9;

        /// <summary>
        /// ProPricer Resource Class2 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS2_CELL_COLUMN_OFFSET = 11;

        /// <summary>
        /// ProPricer Resource Class3 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS3_CELL_COLUMN_OFFSET = 13;

        /// <summary>
        /// ProPricer Resource Class4 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS4_CELL_COLUMN_OFFSET = 15;

        /// <summary>
        /// ProPricer Resource Class5 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS5_CELL_COLUMN_OFFSET = 17;

        /// <summary>
        /// ProPricer Resource Class6 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS6_CELL_COLUMN_OFFSET = 19;

        /// <summary>
        /// ProPricer Resource Class7 Column Offset
        /// </summary>
        public const int RESOURCE_CLASS7_CELL_COLUMN_OFFSET = 21;

        /// <summary>
        /// Government Burden Pool Column Offset
        /// </summary>
        public const int GOVT_BURDEN_POOL_CELL_COLUMN_OFFSET = 22;

        /// <summary>
        /// Commercial Burden Pool Column Offset
        /// </summary>
        public const int COMM_BURDEN_POOL_CELL_COLUMN_OFFSET = 23;

        /// <summary>
        /// Worksheets to be hidden on export, and excluded from import
        /// </summary>
        private static string[] excludedSheetNames = { OPTIONS_LISTS };
        
        /// <summary>
        /// Excluded sheet names
        /// </summary>
        static public IList<string> ExcludedSheetNames
        {
            get
            {
                return excludedSheetNames;
            }
        }

        // Options Lists column headers

        /// <summary>
        /// Rate Category Column Header
        /// </summary>
        public static readonly string CATEGORY_COLUMN_HEADER = "Category";

        /// <summary>
        /// Section Column Header
        /// </summary>
        public static readonly string SECTION_COLUMN_HEADER = "Section";

        /// <summary>
        /// Resource Type Column Header
        /// </summary>
        public static readonly string RESOURCE_TYPE_COLUMN_HEADER = "Resource Type";

        /// <summary>
        /// Rate Type Column Header
        /// </summary>
        public static readonly string RATE_TYPE_COLUMN_HEADER = "Rate Type";

        /// <summary>
        /// Resource Class Column Header
        /// </summary>
        public static readonly string RESOURCE_CLASS_COLUMN_HEADER = "Resource Class";

        /// <summary>
        /// Government Burden Pool Column Header
        /// </summary>
        public static readonly string GOVERNMENT_BURDEN_POOL_COLUMN_HEADER = "Government Burden Pool";

        /// <summary>
        /// Commercial Burden Pool Column Header
        /// </summary>
        public static readonly string COMMERCIAL_BURDEN_POOL_COLUMN_HEADER = "Commercial Burden Pool";

        // Export column headers

        /// <summary>
        /// Rate Category Column Header
        /// </summary>
        public static readonly string RATE_CATEGORY_COLUMN_HEADER = "Category";

        /// <summary>
        /// Rate Code Column Header
        /// </summary>
        public static readonly string RATE_CODE_COLUMN_HEADER = "Rate Code";

        /// <summary>
        /// Rate Description Column Header
        /// </summary>
        public static readonly string RATE_DESCRIPTION_COLUMN_HEADER = "Description";

        /// <summary>
        /// Linked Section Column Header
        /// </summary>
        public static readonly string LINKED_SECTION_COLUMN_HEADER = "Linked Section";

        /// <summary>
        /// ProPricer Description Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION_COLUMN_HEADER = "ProPricer Description";

        /// <summary>
        /// ProPricer Resource Class Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS_COLUMN_HEADER = "Resource Class";

        /// <summary>
        /// ProPricer Description1 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION1_COLUMN_HEADER = "ProPricer Description1";

        /// <summary>
        /// ProPricer Resource Class1 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS1_COLUMN_HEADER = "Resource Class1";

        /// <summary>
        /// ProPricer Description2 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION2_COLUMN_HEADER = "ProPricer Description2";

        /// <summary>
        /// ProPricer Resource Class2 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS2_COLUMN_HEADER = "Resource Class2";

        /// <summary>
        /// ProPricer Description3 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION3_COLUMN_HEADER = "ProPricer Description3";

        /// <summary>
        /// ProPricer Resource Class3 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS3_COLUMN_HEADER = "Resource Class3";

        /// <summary>
        /// ProPricer Description4 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION4_COLUMN_HEADER = "ProPricer Description4";

        /// <summary>
        /// ProPricer Resource Class4 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS4_COLUMN_HEADER = "Resource Class4";

        /// <summary>
        /// ProPricer Description5 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION5_COLUMN_HEADER = "ProPricer Description5";

        /// <summary>
        /// ProPricer Resource Class5 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS5_COLUMN_HEADER = "Resource Class5";

        /// <summary>
        /// ProPricer Description6 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION6_COLUMN_HEADER = "ProPricer Description6";

        /// <summary>
        /// ProPricer Resource Class6 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS6_COLUMN_HEADER = "Resource Class6";

        /// <summary>
        /// ProPricer Description7 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_DESCRIPTION7_COLUMN_HEADER = "ProPricer Description7";

        /// <summary>
        /// ProPricer Resource Class7 Column Header
        /// </summary>
        public static readonly string PRO_PRICER_RESOURCE_CLASS7_COLUMN_HEADER = "Resource Class7";

        // data validation defined names

        /// <summary>
        /// Categories data validation
        /// </summary>
        public static readonly string CATEGORIES = "Categories";

        /// <summary>
        /// Sections data validation
        /// </summary>
        public static readonly string SECTIONS = "Sections";

        /// <summary>
        /// Resource Types data validation
        /// </summary>
        public static readonly string RESOURCE_TYPES = "ResourceTypes";

        /// <summary>
        /// Rate Types data validation
        /// </summary>
        public static readonly string RATE_TYPES = "RateTypes";

        /// <summary>
        /// Resource Classes data validation
        /// </summary>
        public static readonly string RESOURCE_CLASSES = "ResourceClasses";

        /// <summary>
        /// Government Burden Pools data validation
        /// </summary>
        public static readonly string GOVERNMENT_BURDEN_POOLS = "GovernmentBurdenPools";

        /// <summary>
        /// Commercial Burden Pools data validation
        /// </summary>
        public static readonly string COMMERCIAL_BURDEN_POOLS = "CommercialBurdenPools";
    }
}
