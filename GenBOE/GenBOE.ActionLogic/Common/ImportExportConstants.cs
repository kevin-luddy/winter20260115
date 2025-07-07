// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
    using System.Collections.Generic;

    public static class ImportExportConstants
    {
        public static readonly string EXPORT_PATH = "~/Templates/Export/";

        // worksheet names
        public static readonly string OPTIONS_LISTS = "Options Lists";
       
		public const int RESOURCETYPE_RESOURCE_CELL_COLUMN_OFFSET = 1;
		public const int RESOURCETYPE_PERFORG_CELL_COLUMN_OFFSET = 2;
		public const int CLIN_CONTRACT_TYPE_COLUMN_OFFSET = 5;
		public const int PROJECTMAP_RESOURCE_CELL_COLUMN_OFFSET = 3;
		public const int PROJECTMAP_PERFORG_CELL_COLUMN_OFFSET = 4;
		public const int PROJECTMAP_LEGACY_RESOURCE_CELL_COLUMN_OFFSET = 5;
		public const int PROJECTMAP_CLIN_CELL_COLUMN_OFFSET = 9;
		public const int PROJECTMAP_OFFLOAD_CELL_COLUMN_OFFSET = 18;
		public const int PROJECTMAP_CLASS_OF_COST_CELL_COLUMN_OFFSET = 19;
		public const int PROJECTMAP_ADD_DELETE_CELL_COLUMN_OFFSET = 20;

		// Options Lists column headers
        public static readonly string MOQ_TYPE_COLUMN_HEADER = "MOQ Type";
        public static readonly string RESOURCE_COLUMN_HEADER = "Resource";
		public static readonly string BUSINESS_RESOURCE_CODE_COLUMN_HEADER = "Business Resource Code";
        public static readonly string SUB_RESOURCE_COLUMN_HEADER = "Subcontractor Resource";
        public static readonly string PERF_ORG_COLUMN_HEADER = "Perf Org";
        public static readonly string SPREAD_CURVE_COLUMN_HEADER = "Spread Curve";
        public static readonly string WBS_COLUMN_HEADER = "WBS";
        public static readonly string CLIN_COLUMN_HEADER = "CLIN";
        public static readonly string CLIN_CONTRACT_TYPES_OPTIONS_HEADER = "ContractTypes";
        public static readonly string OFFLOAD_COLUMN_HEADER = "Offload";
        public static readonly string ADD_DELETE_COLUMN_HEADER = "Add_Delete";
        public static readonly string CLASS_OF_COST_COLUMN_HEADER = "Class of Cost";

        // Export column headers
        public static readonly string BOE_TITLE_COLUMN_HEADER = "BOE Title";
        public static readonly string START_DATE_COLUMN_HEADER = "Start Date";
        public static readonly string END_DATE_COLUMN_HEADER = "End Date";
        public static readonly string AUTHORS_COLUMN_HEADER = "Authors";
        public static readonly string APPROVERS_COLUMN_HEADER = "Approvers";
        public static readonly string TOTAL_COST_COLUMN_HEADER = "Total Cost";
		public static readonly string TOTAL_UCOT_HOURS_COLUMN_HEADER = "Total UCOT Hours";
		public static readonly string GRAND_TOTAL_HOURS_COLUMN_HEADER = "Grand Total Hours";
		public static readonly string STATUS_COLUMN_HEADER = "Status";
        public static readonly string MATERIAL_COLUMN_HEADER = "Material";
        public static readonly string MULTI_CLIN_COLUMN_HEADER = "Multi WBS/CLIN";
        public static readonly string WBS_NUMBER_COLUMN_HEADER = "WBS #";
        public static readonly string WBS_TITLE_COLUMN_HEADER = "WBS Title";
        public static readonly string CLIN_NUMBER_COLUMN_HEADER = "CLIN #";
        public static readonly string CLIN_TITLE_COLUMN_HEADER = "CLIN Title";
        public static readonly string CLIN_CONTRACT_TYPE_HEADER = "Contract Type";
        public static readonly string RESOURCE_CUSTOM_FIELD_HEADER = "Resource Custom Field";
        public static readonly string PROJECTMAP_WBS_NUMBER_COLUMN_HEADER = "WBS Number";
        public static readonly string ACTIVITY_ID_COLUMN_HEADER = "Activity ID";
        public static readonly string ACTIVITY_NAME_COLUMN_HEADER = "Activity Name";
        public static readonly string WBS_ELEMENT_TITLE_COLUMN_HEADER = "WBS Element Title";
        public static readonly string INITIAL_RESOURCE_COLUMN_HEADER = "Activity Type Code";
        public static readonly string COST_CENTER_COLUMN_HEADER = "Cost Center";
        public static readonly string LEGACY_RESOURCE_COLUMN_HEADER = "Legacy Resource";
		public static readonly string BUSINESS_RESOURCE_CODE_HEADER = "Business Resource Code";
        public static readonly string SOW_COLUMN_HEADER = "SOW #";
        public static readonly string SOW_TITLE_COLUMN_HEADER = "SOW Title";
        public static readonly string TASK_COLUMN_HEADER = "TASK";
        public static readonly string HOURS_COLUMN_HEADER = "HOURS";
        public static readonly string DOLLARS_COLUMN_HEADER = "DOLLARS";
        public static readonly string RATIONALE_COLUMN_HEADER = "RATIONALE";
        public static readonly string BOE_COLUMN_HEADER = "BASIS OF ESTIMATE";
        public static readonly string CAM_NAME_COLUMN_HEADER = "CAM NAME";
        public static readonly string CATEGORY_COLUMN_HEADER = "CATEGORY";
        public static readonly string TIERED_PERCENTAGE_COLUMN_HEADER = "% Tier";
        
        // data validation defined names
        public static readonly string MOQ_TYPES = "MOQTypes";
        public static readonly string RESOURCES = "Resources";
		public static readonly string BUSINESS_RESOURCE_CODES = "BusinessResourceCodes";
        public static readonly string SUB_RESOURCES = "SubResources";
        public static readonly string PERFORGS = "PerfOrgs";
        public static readonly string SPREAD_CURVES = "SpreadCurves";
        public static readonly string WBSS = "WBSs";
        public static readonly string CLINS = "CLINs";
        public static readonly string OFFLOAD = "Offload";
        public static readonly string ADD_DELETE = "Add_Delete";
        public static readonly string CLASS_OF_COST = "ClassofCost";
        public static readonly string LEGACY_RESOURCES = "LegacyResources";

        // suffixes for dynamically generated tables
        public static readonly string WORKSPACE_TABLE_SUFFIX = "_Workspace";
        public static readonly string BOE_TABLE_SUFFIX = "_Boe";
        public static readonly string TASK_TABLE_SUFFIX = "_Task_";
        public static readonly string RESOURCE_TABLE_SUFFIX = "_Resource_";

        // prefix for dynamically generated custom field defined names
        public static readonly string CUSTOM_FIELD_DEFINED_NAME_PREFIX = "CustomField_";

        public static readonly string PLACEHOLDER_TEXT = "Text exists in genBOE.  Character limitations or existing rich text formatting restrict the export of this data.  Do not edit or remove this placeholder.  Edits made to this placeholder will replace data within genBOE.";
        public static readonly string PLACEHOLDER_TEXT_RTE_TEMPLATES = "Templates have been turned on by Workspace Administrator. Import via this field is disabled, any edits made will not be imported into the application.";

        public static readonly string SHAPE_ID_PREFIX = "_x0000_s"; // Prefix used by VmlDrawing Shape IDs
        public static readonly string SPID_ATTRIBUTE = "spid";      // Shape ID attribute
        public static readonly string BUTTON_PREFIX = "Button ";    // Prefix used when creating AlternateContent Buttons
        public static readonly string COLUMN_PREFIX = "Column";     // Prefix used when creating new TableColumns
        public static readonly string ADD_TASK_BUTTON = "Add New Task";
        public static readonly string ADD_RESOURCE_BUTTON = "Add Resource";
    }
}
