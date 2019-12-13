using System.Web.Configuration;
namespace GenBOE.Web.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    public static class WebConstants
    {
        #region URL Routing

        /// <summary>
        /// string.Empty
        /// </summary>
        readonly static public string URL_PATTERN_EMPTY = string.Empty;

        /// <summary>
        /// default/{controller}/{action}
        /// </summary>
        readonly static public string URL_PATTERN_DEFAULT = "default/{controller}/{action}";

        /// <summary>
        /// {workspace}/{controller}/{action}/{id}
        /// </summary>
        readonly static public string URL_PATTERN_WORKSPACE = "{workspace}/{controller}/{action}/{id}";

        /// <summary>
        /// {workspace}/{controller}/{action}/customfield/{customFieldID}
        /// </summary>
        readonly static public string URL_PATTERN_CUSTOM_FIELD = "{workspace}/{controller}/{action}/customfield/{customFieldID}";

        /// <summary>
        /// {workspace}/{controller}/{action}/rate/{workspaceResourceRateID}
        /// </summary>
        readonly static public string URL_PATTERN_WORKSPACE_RATE = "{workspace}/{controller}/{action}/rate/{workspaceResourceRateID}";
        
        /// <summary>
        /// {workspace}/{controller}/{action}/boe/{boeID}
        /// </summary>
        readonly static public string URL_PATTERN_BOE = "{workspace}/{controller}/{action}/boe/{boeID}";

        /// <summary>
        /// {workspace}/{controller}/{action}/boe/{boeID}/taskelement/{taskElementID}
        /// </summary>
        readonly static public string URL_PATTERN_BOE_TASK_ELEMENT = "{workspace}/{controller}/{action}/boe/{boeID}/taskelement/{taskElementID}";

        /// <summary>
        /// {workspace}/{controller}/{action}/boe/{boeID}/odcelement/{odcElementID}
        /// </summary>
        readonly static public string URL_PATTERN_BOE_ODC = "{workspace}/{controller}/{action}/boe/{boeID}/odcelement/{odcElementID}";

        /// <summary>
        /// {workspace}/{controller}/{action}/boe/{boeID}/travelelement/{travelElementID}
        /// </summary>
        readonly static public string URL_PATTERN_BOE_TRAVEL = "{workspace}/{controller}/{action}/boe/{boeID}/travelelement/{travelElementID}";
        
        /// <summary>
        /// {workspace}/{controller}/{action}/report/{reportID}
        /// </summary>
        readonly static public string URL_PATTERN_REPORTS = "{workspace}/{controller}/{action}/report/{reportID}";

        /// <summary>
        /// error/{action}
        /// </summary>
        readonly static public string URL_PATTERN_ERROR = "error/{action}";

        /// <summary>
        /// {workspace}/{controller}/{action}/boe/{boeID}/materialelement/{materialID}
        /// </summary>
        readonly static public string URL_PATTERN_BOE_MATERIAL = "{workspace}/{controller}/{action}/boe/{boeID}/materialelement/{materialID}";

        /// <summary>
        /// {workspace}/{controller}/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}
        /// </summary>
        readonly static public string URL_PATTERN_EXPORT_WS_CUSTOM_RESOURCES = "{workspace}/{controller}/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}";
        
        /// <summary>
        /// admin/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}
        /// </summary>
        readonly static public string URL_PATTERN_EXPORT_ADMIN_CUSTOM_RESOURCES = "admin/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}";
        
        /// <summary>
        /// testing/{controller}/{action}
        /// </summary>
        readonly static public string URL_PATTERN_FRONTEND_TESTING = "testing/{controller}/{action}";

        readonly static public string ROUTE_WORKSPACE = "WorkspaceRoute";
        readonly static public string ROUTE_REPORT = "ReportRoute";
        readonly static public string ROUTE_DEFAULT = "DefaultRoute";
        readonly static public string ROUTE_BOE = "BoeRoute";

        #endregion URL Routing

        readonly static public string BROWSER_REMINDER = "GenBOEBrowserReminder";
        readonly static public string UPDATE_WORKSPACE_RESOURCE_RATE = "UpdateWorkspaceResourceRate";
        readonly static public string STRING_IMPORT_WBS_EXCEPTION_PREPEND = "Import WBS Exception:";        


        #region CONTROLLERS

        readonly static public string CONTROLLER_ADMIN = "Admin";
        readonly static public string CONTROLLER_BOE = "BOE";
        readonly static public string CONTROLLER_BOE_COMMENTS = "BOEComments";
        readonly static public string CONTROLLER_BOE_HISTORY = "BOEHistory";
        readonly static public string CONTROLLER_BOE_LABOR = "BOELabor";
        readonly static public string CONTROLLER_CLIN = "CLIN";
        readonly static public string CONTROLLER_GENBOE = "GenBOE";
        readonly static public string CONTROLLER_HOME = "Home";
        readonly static public string CONTROLLER_PERMISSIONS = "Permissions";
        readonly static public string CONTROLLER_REPORTS = "Reports";
        readonly static public string CONTROLLER_WBS = "WBS";
        readonly static public string CONTROLLER_WORKSPACE = "Workspace";
        readonly static public string CONTROLLER_FIND_REPLACE = "FindReplace";
        readonly static public string CONTROLLER_MISSION_TASKS = "MissionTasks";
        readonly static public string CONTROLLER_BOE_OTHER_DIRECT_COST = "BOEOtherDirectCost";
        readonly static public string CONTROLLER_BOE_MATERIAL = "BOEMaterial";
        readonly static public string CONTROLLER_BOE_TRAVEL = "BOETravel";
        readonly static public string CONTROLLER_AUTOCOMPLETE = "AutoComplete";
        readonly static public string CONTROLLER_FE_TESTING = "FrontEndModuleTesting";

        #endregion CONTROLLERS

        #region ACTIONS

        readonly static public string ACTION_INDEX = "Index";
        readonly static public string ACTION_EDIT_BOE_INDEX = "EditBOEIndex";
        readonly static public string ACTION_INVALID_REQUEST = "InvalidRequest";
        readonly static public string ACTION_MANAGE_PERMISSIONS = "ManagePermissions";
        readonly static public string ACTION_ABOUT_USER = "AboutUser";
        readonly static public string ACTION_LIST_ALL_USERS = "ListAllUsers";

        readonly static public string ACTION_CANCEL = "Cancel";
        readonly static public string ACTION_ERROR = "Error";

        #region TESTING
        readonly static public string ACTION_DISPLAY_FRONTPAGE = "DisplayKitchenSinkFrontPage";
        readonly static public string ACTION_DISPLAY_GENWIDGTDEMO = "DisplayGenWidgetDemo";
        readonly static public string ACTION_DISPLAY_GENGRIDWIDGTDEMO = "DisplayGenGridWidgetDemo";
        #endregion TESTING

        #region ADMIN

        readonly static public string ACTION_ASSIGNED_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE = "GetWorkspacesAssignedForExportTemplateId";
        readonly static public string ACTION_AVAILABLE_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE = "GetAvailableWorkspaceIdsForExportFormatId";
        readonly static public string ACTION_CHECK_IS_USER_WILL_LOOSE_THEIR_SYSTEM_ADMIN_ACCESS = "CheckIfUserWillLooseTheirSystemAdminAccess";
        readonly static public string ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_CREATE_WORKSPACE_PERMISSIONS_ACCESS = "CheckIfUserWillLoseTheirCreateWorkspacePermissionsAccess";
        readonly static public string ACTION_CHECK_IF_RESOURCE_HAS_RATES = "CheckIfResourceHasRates";
        readonly static public string ACTION_COMPLETE_HISTORICAL_METRICS_IMPORT = "CompleteHistoricalMetricsImport";
        readonly static public string ACTION_COMPLETE_TRIPS_IMPORT = "CompleteTripsImport";
        readonly static public string ACTION_CREATE_EDIT_METRICS_ADMIN_GROUP = "CreateEditMetricAdminGroup";
        readonly static public string ACTION_DELETE_GROUP_METRICS_PERMISSIONS = "DeleteGroupMetricsPermissions";
        readonly static public string ACTION_DELETE_GROUP_SYSTEM_PERMISSIONS = "DeleteGroupSystemPermissions";
        readonly static public string ACTION_DELETE_METRIC_ADMIN_GROUPS = "DeleteMetricAdminGroups";
        readonly static public string ACTION_DELETE_USER_METRICS_PERMISSIONS = "DeleteUserMetricsPermissions";
        readonly static public string ACTION_DELETE_USER_SYSTEM_PERMISSIONS = "DeleteUserSystemPermissions";
        readonly static public string ACTION_DELETE_USER_CREATE_WORKSPACE_PERMISSIONS = "DeleteUserCreateWorkspacePermissions";
        readonly static public string ACTION_DELETE_GROUP_CREATE_WORKSPACE_PERMISSIONS = "DeleteGroupCreateWorkspacePermissions";
        readonly static public string ACTION_DISPLAY_DEFAULT_PERF_ORGS = "DisplayDefaultPerfOrgs";
        readonly static public string ACTION_DISPLAY_DEFAULT_RESOURCES = "DisplayDefaultResources";
        readonly static public string ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID = "DisplayManageDefaultPerfOrgsGrid";
        readonly static public string ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID = "DisplayManageDefaultResourcesGrid";
        readonly static public string ACTION_DISPLAY_MANAGE_OUTPUT_FORMAT_TEMPLATE = "DisplayManageOutputFormatTemplates";
        readonly static public string ACTION_DISPLAY_MANAGE_TRIPS = "DisplayManageTripsForTravel";
        readonly static public string ACTION_DISPLAY_MANAGE_TRIPS_GRID = "DisplayManageTripsForTravelGrid";
        readonly static public string ACTION_CALCULATE_TRAVEL_TRIPS = "CalculateBOETravelTripCost";
        readonly static public string ACTION_AUTOCOMPLETE_LOCATION_NAME = "AutocompleteTripLocationName";
        readonly static public string ACTION_AUTOCOMPLETE_DEP_LOCATION_CODE = "AutocompleteTripDepartureLocationCode";
        readonly static public string ACTION_AUTOCOMPLETE_DES_LOCATION_CODE = "AutocompleteTripDestinationLocationCode";
        readonly static public string ACTION_GET_LOCATION_DETAILS = "GetLocationDetails";
        readonly static public string ACTION_AUTOCOMPLETE_PER_DIEM = "AutocompleteTripPerDiemLocation";
        readonly static public string ACTION_GET_PER_DIEM_LOCATION_DETAILS = "GetPerDiemLocationDetails";
        readonly static public string ACTION_AUTOCOMPLETE_QUALIFICATION = "AutocompleteTripQualification";
        readonly static public string ACTION_DELETE_TRIPS = "DeleteTrips";
        readonly static public string ACTION_SAVE_TRIP = "SaveTrip";
        readonly static public string ACTION_PAGE_TRIP_RESULTS = "PageTripResults";
        readonly static public string ACTION_IS_TRIP_UNIQUE = "IsTripUnique";
        readonly static public string ACTION_DISPLAY_MANAGE_MISC_RATES = "DisplayManageMiscRates";
        readonly static public string ACTION_DISPLAY_MANAGE_MISC_RATES_GRID = "DisplayManageMiscRatesGrid";
        readonly static public string ACTION_DISPLAY_ESCALATION_RATES = "DisplayManageEscalationRates";
        readonly static public string ACTION_DISPLAY_ESCALATION_RATES_GRID = "DisplayManageEscalationRatesGrid";
        readonly static public string ACTION_DISPLAY_REIMBURSEMENT_RATE = "DisplayManageMileageReimbursement";
        readonly static public string ACTION_DISPLAY_SYSTEM_RESOURCE_RATES = "DisplaySystemResourceRates";
        readonly static public string ACTION_DOWNLOAD_SYSTEM_RESOURCE_RATES_TEMPLATE = "DownloadSystemResourceRatesTemplateExample";
        readonly static public string ACTION_IMPORT_SYSTEM_RESOURCE_RATES = "ImportSystemResourceRates";
        readonly static public string ACTION_IMPORT_PREVIEW_SYSTEM_RESOURCE_RATES = "ImportPreviewResultsForSystemResourceRates";
        readonly static public string ACTION_DELETE_SYSTEM_RESOURCE_RATES = "DeleteSystemResourceRates";
        readonly static public string ACTION_EXPORT_SYSTEM_RESOURCE_RATES = "ExportSystemResourceRates";
        readonly static public string ACTION_DISPLAY_IMPORT_PREVIEW_SYSTEM_RESOURCE_RATES = "SystemResourceRatesImportPreview";
        readonly static public string ACTION_PAGE_SYSTEM_RESOURCE_RATES = "PageSystemResourceRates";
        readonly static public string ACTION_DISPLAY_METRICS_ADMIN_JUMP = "DisplayMetricsAdminJump";
        readonly static public string ACTION_DISPLAY_METRICS_PERMISSIONS = "DisplayMetricsPermissions";
        readonly static public string ACTION_DISPLAY_METRICS_GROUPS = "DisplayMetricsGroups";
        readonly static public string ACTION_DISPLAY_HISTORICAL_METRICS = "DisplayHistoricalMetrics";
        readonly static public string ACTION_DISPLAY_HISTORICAL_METRICS_GRID = "DisplayHistoricalMetricsGrid";
        readonly static public string ACTION_DELETE_HISTORICAL_METRICS = "DeleteHistroicalMetrics";
        readonly static public string ACTION_DISPLAY_ADD_HISTORICAL_METRICS = "DisplayAddHistroicalMetrics";
        readonly static public string ACTION_DISPLAY_SYSTEM_ADMIN_JUMP = "DisplaySystemAdminJump";
        readonly static public string ACTION_DISPLAY_SYSTEM_PERMISSIONS = "DisplaySystemPermissions";
        readonly static public string ACTION_DISPLAY_CREATE_WORKSPACE_PERMISSIONS = "DisplayCreateWorkspacePermissions";
        readonly static public string ACTION_EXPORT_DEFAULT_PERFORMING_ORGS = "ExportPerformingOrgs";
        readonly static public string ACTION_EXPORT_DEFAULT_RESOURCES = "ExportResources";
        readonly static public string ACTION_EXPORT_HISTORICAL_METRICS = "ExportHistoricalMetrics";
        readonly static public string ACTION_EXPORT_HISTORICAL_METRICS_TEMPLATE = "ExportHistoricalMetricsTemplate";
        readonly static public string ACTION_EXPORT_TRIPS = "ExportTrips";
        readonly static public string ACTION_EXPORT_TRIPS_TEMPLATE = "ExportTripsTemplate";
        readonly static public string ACTION_GET_OUTPUT_FORMAT_TEMPLATE = "GetOutputFormatTemplate";
        readonly static public string ACTION_IMPORT_DEFAULT_PERFORMING_ORGS = "ImportPerformingOrgs";
        readonly static public string ACTION_IMPORT_DEFAULT_RESOURCES = "ImportResources";
        readonly static public string ACTION_IMPORT_HISTORICAL_METRICS = "ImportHistoricalMetrics";
        readonly static public string ACTION_IMPORT_TRIPS = "ImportTrips";
        readonly static public string ACTION_METRICS_ADMIN = "MetricsAdmin";
        readonly static public string ACTION_PAGE_PERF_ORGS = "PagePerfOrgs";
        readonly static public string ACTION_PAGE_OUTPUT_FORMAT = "PageOutputFormat";
        readonly static public string ACTION_PAGE_RESOURCES = "PageResources";
        readonly static public string ACTION_PAGE_HISTORICAL_METRICS = "PageHistoricalMetrics";
        readonly static public string ACTION_PAGE_WORKSPACE_HOME = "PageWorkspaceHome";
        readonly static public string ACTION_SAVE_DEFAULT_PERF_ORGS = "SavePerfOrgs";
        readonly static public string ACTION_SAVE_DEFAULT_RESOURCES = "SaveResources";
        readonly static public string ACTION_SAVE_MISC_RATES = "SaveMiscRates";
        readonly static public string ACTION_SAVE_ESCALATION_RATES = "SaveEscalationRates";
        readonly static public string ACTION_SAVE_MILEAGE_REIMBURSEMENT = "SaveMileageReimbursement";
        readonly static public string ACTION_SAVE_NEW_METRICS_PERMISSIONS = "SaveNewMetricsPermissions";
        readonly static public string ACTION_SAVE_NEW_SYSTEM_PERMISSIONS = "SaveNewSystemPermissions";
        readonly static public string ACTION_SAVE_NEW_CREATE_WORKSPACE_PERMISSIONS = "SaveNewCreateWorkspacePermissions"; 
        readonly static public string ACTION_SAVE_NEW_HISTORICAL_METRICS = "SaveHistoricalMetric";
        readonly static public string ACTION_SAVE_RESOURCE_LIST = "SaveResourceList";
        readonly static public string ACTION_SAVE_WORKSPACES_FOR_OUTPUT_TEMPLATES = "SaveExportTemplatesForWorkspaces";
        readonly static public string ACTION_SEARCH_PERF_ORGS = "SearchPerfOrgs";
        readonly static public string ACTION_SEARCH_RESOURCES = "SearchResources";
        readonly static public string ACTION_SYSTEM_ADMIN = "SystemAdmin";
        readonly static public string ACTION_EXPORT_DEFAULT_RESOURCES_TEMPLATE = "ExportDefaultResourcesTemplate";
        readonly static public string ACTION_DISPLAY_WHOS_ONELINE = "WhosOnline";

        #endregion ADMIN

        #region BOE ACTIONS

        readonly static public string ACTION_DISPLAY_BOE_ADVANCED_SEARCH = "DisplayBOEAdvancedSearch";
        readonly static public string ACTION_DISPLAY_BOE_DETAILS = "DisplayBOEDetails";
        readonly static public string ACTION_DISPLAY_BOE_EXPORT = "DisplayExportBOEButton";
        readonly static public string ACTION_DISPLAY_BOE_HEADER_DESCRIPTION = "DisplayBOEHeaderDescription";
        readonly static public string ACTION_DISPLAY_BOE_HEADER_TITLE = "DisplayBOEHeaderTitle";
        readonly static public string ACTION_DISPLAY_BOE_HEADER = "DisplayBOEHeader";
        readonly static public string ACTION_DISPLAY_BOE_QUICK_SEARCH = "DisplayBOEQuickSearch";
        readonly static public string ACTION_DISPLAY_BOE_SEARCH = "DisplayBOESearch";
        readonly static public string ACTION_DISPLAY_BOE_SUBMIT_FOR_REVIEW = "DisplaySubmitForReviewBOEButton";
        readonly static public string ACTION_DISPLAY_BOE_SUMMARY = "DisplayBOESummary";
        readonly static public string ACTION_DISPLAY_BOE_VALIDATE = "DisplayValidateBOEButton";
        readonly static public string ACTION_DISPLAY_BOE_VALIDATE_RESULTS = "DisplayBOEValidateResults";
        readonly static public string ACTION_DISPLAY_INVALID_SUBMIT_FOR_APPROVAL = "DisplayInvalidSubmitForApproval";
        readonly static public string ACTION_DISPLAY_LABOR_CURVES = "DisplayLaborCurves";
        readonly static public string ACTION_DISPLAY_MANAGE_BOE = "DisplayManageBOE";
        readonly static public string ACTION_DISPLAY_MANAGE_BOE_GRID = "DisplayManageBOEGrid";
        readonly static public string ACTION_DISPLAY_SUBMIT_FOR_APPROVAL = "DisplaySubmitForApproval";
        readonly static public string ACTION_DISPLAY_TASK_ELEMENT_GRID = "DisplayTaskElementGrid";

        readonly static public string ACTION_ADVANCED_SEARCH_FOR_BOES = "AdvancedSearchForBOEs";
        readonly static public string ACTION_BOE_SUBMIT_FOR_REVIEW = "SubmitForReview";
        readonly static public string ACTION_COMPLETE_IMPORT_MANAGE_BOE = "CompleteImportManageBOE";
        readonly static public string ACTION_DATA_BOE = "LoadBOEDATA";
        readonly static public string ACTION_DELETE_TASK_ELEMENTS = "DeleteTaskElements";
        readonly static public string ACTION_EXPORT_BOE = "ExportBOEToWordFile";
        readonly static public string ACTION_BOE_SEARCH_PREVIEW = "BOESearchPreview";
        readonly static public string ACTION_EXPORT_MANAGE_BOE = "ExportManageBOE";
        readonly static public string ACTION_EXPORT_MANAGE_BOE_TEMPLATE = "ExportManageBOETemplate";
        readonly static public string ACTION_GET_UPDATED_BOE_HEADER = "GetUpdatedBOEHeader";
        readonly static public string ACTION_IMPORT_MANAGE_BOE = "ImportManageBOE";
        readonly static public string ACTION_PAGE_MANAGE_BOE = "PageManageBOE";
        readonly static public string ACTION_PAGE_SEARCH_RESULTS = "PageSearchResults";
        readonly static public string ACTION_QUICK_SEARCH_FOR_BOES = "QuickSearchForBOEs";
        readonly static public string ACTION_SAVE_EDIT_BOE_HEADER = "SaveEditBOEHeader";
        readonly static public string ACTION_SAVE_MANAGE_BOE = "SaveManageBOE";
        readonly static public string ACTION_SUBMIT_FOR_APPROVAL = "SubmitForApproval";
        readonly static public string ACTION_VALIDATE_BOE = "ValidateBOE";
        readonly static public string ACTION_BOE_CONTAINS_SUM_OF_BOES = "BOEContainsSumOfBOEs";
        readonly static public string ACTION_BOE_COPY_CONFLICTS = "DisplayCopyBOEConflicts";
        readonly static public string ACTION_SAVE_COPY_OF_BOE = "SaveCopyOfBOE";
        readonly static public string ACTION_DELETE_ALL_BOE_TASK_ELEMENTS = "DeleteAllBOETaskElements";


        // BOEComments Controller
        readonly static public string ACTION_DISPLAY_BOE_COMMENTS = "DisplayBOEComments";
        readonly static public string ACTION_SAVE_BOE_COMMENTS = "SaveBoeComments";

        // BOEHistory Controller
        readonly static public string ACTION_DISPLAY_BOE_HISTORY = "DisplayBOEHistory";


        // BOELabor Controller
        readonly static public string ACTION_DELETE_ALL_MISSION_RESOURCE_TYPES = "DeleteAllMissionResourceTypes";
        readonly static public string ACTION_DISPLAY_DTS_COMPOSITE = "DisplayDTSComposite";
        readonly static public string ACTION_DISPLAY_DTS_RESOURCE_SPREAD = "DisplayDTSResourceSpread";
        readonly static public string ACTION_DISPLAY_DTS_RESOURCE_TYPES = "DisplayDTSResourceTypes";
        readonly static public string ACTION_DISPLAY_DTS_TASK_ELEMENT_DETAILS = "DisplayDTSTaskElementDetails";
        readonly static public string ACTION_DISPLAY_IWTA_TASK_ELEMENT_DETAILS = "DisplayIWTATaskElementDetails";
        readonly static public string ACTION_DISPLAY_TASK_ELEMENTS_COMPOSITE = "DisplayTaskElementsComposite";
        readonly static public string ACTION_DISPLAY_TASK_ELEMENT_DETAILS = "DisplayTaskElementDetails";
        readonly static public string ACTION_DISPLAY_MOQ_HOURS_EQUATION_FIELD = "DisplayMOQHoursEquationField";
        readonly static public string ACTION_DISPLAY_MOQ_COST_EQUATION_FIELD = "DisplayMOQCostEquationField";
        readonly static public string ACTION_DISPLAY_LABOR_TYPES = "DisplayLaborTypes";
        readonly static public string ACTION_DISPLAY_LABOR_TYPES_GRID = "DisplayLaborTypesGrid";
        readonly static public string ACTION_DISPLAY_MISSION_LABOR_TYPES_GRID = "DisplayMissionLaborTypesGrid";
        readonly static public string ACTION_DISPLAY_LABOR_SPREAD = "DisplayLaborSpread";
        readonly static public string ACTION_DISPLAY_LABOR_SPREAD_GRID = "DisplayLaborSpreadGrid";
        readonly static public string ACTION_REDISPLAY_LABOR_SPREAD_GRID = "RedisplayLaborSpreadGrid";
        readonly static public string ACTION_IMPORT_LABOR_SPREAD = "ImportLaborSpread";
        readonly static public string ACTION_COMPLETE_IMPORT_LABOR_SPREAD = "CompleteImportLaborSpread";
        readonly static public string ACTION_EXPORT_LABOR_SPREAD = "ExportLaborSpread";
        readonly static public string ACTION_IMPORT_LABOR_TYPE = "ImportLaborType";
        readonly static public string ACTION_PREVIEW_IMPORT_LABOR_TYPE = "ImportPreviewLaborType";
        readonly static public string ACTION_EXPORT_LABOR_TYPE = "ExportLaborType";
        readonly static public string ACTION_SAVE_EDIT_TASK_DETAILS_COMPOSITE = "SaveEditTaskDetailsComposite";
        readonly static public string ACTION_SAVE_EDIT_MISSION_TASK_DETAILS_COMPOSITE = "SaveEditMissionTaskDetailsComposite";
        readonly static public string ACTION_MOQ_VALIDATE = "MOQValidate";
        readonly static public string ACTION_MOQ_CALCULATE = "MOQCalculate";
        readonly static public string ACTION_SAVE_DTS_TASK_ELEMENT = "SaveDTSTaskElement";
        readonly static public string ACTION_SEARCH_PERFORMING_ORGS = "SeachPerformingOrgs";
        readonly static public string ACTION_GET_ALL_PERFORMING_ORGS = "GetAllPerformingOrgs";
        readonly static public string ACTION_VALIDATE_PERFORMING_ORGS = "ValidatePerformingOrgs";
        readonly static public string ACTION_GET_PERFORMING_ORG_ID_BYNAME = "GetPerformingOrgIDByName";
        readonly static public string ACTION_VALIDATE_RESOURCE = "ValidateResource";
        readonly static public string ACTION_VALIDATE_RESOURCE_BY_ID = "ValidateResourceByID";
        readonly static public string ACTION_SPREAD_HOURS = "SpreadHours";
        readonly static public string ACTION_MARK_WARNING_AS_CONFIRMED = "MarkWarningMessageAsConfirmed";
        readonly static public string ACTION_SEARCH_HISTORICAL_METRICS = "DisplayHistoricalMetricsResults";
        readonly static public string ACTION_GET_SEARCH_TYPE_AHEAD = "GetTypeAheadTerms";
        readonly static public string ACTION_GET_HISTORICAL_METRIC_DETAILS = "DisplayHistoricalMetricsDetails";
        readonly static public string ACTION_PAGE_HISTORICAL_METRIC_SEARCH_RESULTS = "PageHistoricalMetricsSearchResults";
        readonly static public string ACTION_CALCULATE_LABOR_COST = "CalculateUnburdenedLabor";
        readonly static public string ACTION_CALCULATE_DISCRETE_HOUR_SPREAD_COST = "CalculateDiscreteHourSpreadCost";
        readonly static public string ACTION_GET_ALL_LABOR_SPREADS_FOR_LABOR_TYPES = "GetAllLaborSpreadsForGivenLaborTypes";
        readonly static public string ACTION_GET_SPREAD_CURVES_FOR_RESOURCE = "GetSpreadCurvesForResource";
        readonly static public string ACTION_VERIFY_LABOR_TYPE_DATES = "VerifyLaborTypeDates";

        //BOE ODC Controller
        readonly static public string ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_GRID = "DisplayBOEOtherDirectCostGrid";
        readonly static public string ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_COMPOSITE = "DisplayBOEOtherDirectCostComposite";
        readonly static public string ACTION_DISPLAY_BOE_ODC_ELEMENT_DETAILS = "DisplayBOEODCElementDetails";
        readonly static public string ACTION_DISPLAY_BOE_ODC_TYPES = "DisplayBOEODCTypes";
        readonly static public string ACTION_DISPLAY_BOE_ODC_TYPES_GRID = "DisplayBOEODCTypesGrid";
        readonly static public string ACTION_DISPLAY_BOE_ODC_SPREAD = "DisplayBOEODCSpread";
        readonly static public string ACTION_DISPLAY_BOE_ODC_SPREAD_GRID = "DisplayBOEODCSpreadGrid";
        readonly static public string ACTION_SAVE_EDIT_ODC_DETAILS_COMPOSITE = "SaveEditODCDetailsComposite";
        readonly static public string ACTION_IMPORT_ODC_TYPE = "ImportODCType";
        readonly static public string ACTION_PREVIEW_IMPORT_ODC_TYPE = "ImportPreviewODCType";
        readonly static public string ACTION_IMPORT_ODC_SPREAD = "ImportODCSpread";
        readonly static public string ACTION_COMPLETE_IMPORT_ODC_SPREAD = "CompleteImportODCSpread";
        readonly static public string ACTION_EXPORT_ODC_TYPE = "ExportODCType";
        readonly static public string ACTION_EXPORT_ODC_SPREAD = "ExportODCSpread";
        readonly static public string ACTION_DELETE_BOE_OTHER_DIRECT_COST = "DeleteBOEOtherDirectCost";
        readonly static public string ACTION_DELETE_ALL_BOE_OTHER_DIRECT_COST = "DeleteAllODCElements";

        //BOE Travel Controller
        readonly static public string ACTION_DISPLAY_BOE_TRAVEL_GRID = "DisplayBOETravelGrid";
        readonly static public string ACTION_DISPLAY_BOE_TRAVEL_COMPOSITE = "DisplayBOETravelComposite";
        readonly static public string ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS = "DisplayBOETravelElementDetails";
        readonly static public string ACTION_DISPLAY_BOE_TRAVEL_TRIPS = "DisplayBOETravelTrips";
        readonly static public string ACTION_DISPLAY_BOE_TRAVEL_TRIPS_GRID = "DisplayBOETravelTripsGrid";
        readonly static public string ACTION_SAVE_EDIT_TRAVEL_DETAILS_COMPOSITE = "SaveEditTravelDetailsComposite";
        readonly static public string ACTION_DELETE_BOE_TRAVEL = "DeleteBOETravel";
        readonly static public string ACTION_DELETE_ALL_BOE_TRAVEL = "DeleteAllBOETravel";

        // Material
        readonly static public string ACTION_COMPLETE_IMPORT_BOE_MATERIALS_AS_WORKSPACE_ADMIN = "CompleteImportBOEMaterialsAsWorkspaceAdmin";
        readonly static public string ACTION_DISPLAY_BOE_MATERIAL_GRID = "DisplayBOEMaterialGrid";
        readonly static public string ACTION_DISPLAY_BOE_MATERIAL_COMPOSITE = "DisplayBOEMaterialComposite";
        readonly static public string ACTION_DISPLAY_BOE_MATERIAL_ELEMENT_DETAILS = "DisplayBOEMaterialElementDetails";
        readonly static public string ACTION_DISPLAY_BOE_MATERIAL_TYPES = "DisplayBOEMaterialTypes";
        readonly static public string ACTION_DISPLAY_BOE_MATERIAL_TYPES_GRID = "DisplayBOEMaterialTypesGrid";
        readonly static public string ACTION_GET_FILTERED_TRIP_DATA = "GetFilteredTripData";
        readonly static public string ACTION_GET_TRIP_ID = "GetTripID";
        readonly static public string ACTION_IMPORT_BOE_MATERIALS_AS_WORKSPACE_ADMIN = "ImportBOEMaterialsAsWorkspaceAdmin";
        readonly static public string ACTION_SAVE_EDIT_MATERIAL_DETAILS_COMPOSITE = "SaveEditMaterialDetailsComposite";
        readonly static public string ACTION_EXPORT_MATERIAL_TYPES = "ExportLaborTypes";

        #endregion BOE ACTIONS

        #region CLIN
        readonly static public string ACTION_SAVE_CLIN = "SaveClin";
        readonly static public string ACTION_DELETE_CLINS = "DeleteCLINs";
        readonly static public string ACTION_GET_BOE_COUNT_FOR_CLIN = "GetBOECountForClin";
        readonly static public string ACTION_DISPLAY_MANAGE_CLINS = "DisplayManageCLINs";
        readonly static public string ACTION_DISPLAY_MANAGE_CLIN_GRID = "DisplayManageCLINGrid";
        readonly static public string ACTION_PAGE_MANAGE_CLIN_GRID = "PageManageCLIN";
        readonly static public string ACTION_SEARCH_MANAGE_CLIN_GRID = "SearchManageCLIN";
        readonly static public string ACTION_IMPORT_CLINS = "ImportCLINs";
        readonly static public string ACTION_COMPLETE_IMPORT_CLINS = "CompleteImportCLINs";
        readonly static public string ACTION_EXPORT_CLINS = "ExportCLINs";
        #endregion CLIN

        #region HOME

        readonly static public string ACTION_CLEAR_CACHE = "ClearCache";
        readonly static public string ACTION_AD_SYNC_ADD = "ADSyncAddUsers";
        readonly static public string ACTION_AD_SYNC_REMOVE = "ADSyncRemoveUsers";
        readonly static public string ACTION_AD_SYNC_UPDATE = "ADSyncUpdateUsers";
        readonly static public string ACTION_CREATE_WORKSPACE = "CreateWorkspace";
        readonly static public string ACTION_VALIDATE_CREATE_WORKSPACE_STEP_ONE = "ValidateCreateWorkspaceStepOne";
        readonly static public string ACTION_DISPLAY_CHOOSE_WORKSPACE = "DisplayChooseWorkspace";
        readonly static public string ACTION_DISPLAY_CREATE_WORKSPACE_FORM = "DisplayCreateWorkspaceForm";
        readonly static public string ACTION_DISPLAY_CREATE_WORKSPACE_DIV = "DisplayCreateWorkspaceDiv";
        readonly static public string ACTION_SAVE_NEW_WORKSPACE = "SaveNewWorkspace";
        readonly static public string ACTION_DISPLAY_WORKSPACE_SEARCH = "DisplayWorkspaceSearch";
        readonly static public string ACTION_PAGE_WORKSPACE_SEARCH_RESULTS = "PageWorkspaceSearchResults";
        readonly static public string ACTION_PERFORM_WORKSPACE_SEARCH = "PerformWorkspaceSearch";
        readonly static public string ACTION_VALIDATE_WORKSPACE_SEARCH = "ValidateWorkspaceSearch";
        readonly static public string ACTION_GET_DETAILS_FOR_WORKSPACE_TO_COPY = "GetDetailsForWorkspaceToCopy";
        readonly static public string ACTION_GET_BOES_FOR_WORKSPACE_TO_COPY = "GetBOEsForWorkspaceToCopy";
        readonly static public string ACTION_HOME_DISPLAY_MASTER_MENU = "DisplayHomeMasterMenu";
        readonly static public string ACTION_HOME_DISPLAY_GENBOE_METRICS = "DisplayGenBOEMetrics";
        readonly static public string ACTION_HOME_DISPLAY_WHOSONLINE_DIV = "DisplayGenBOEMetricsWhosOnlineDiv";

        #endregion HOME

        #region Reports

        readonly static public string ACTION_DISPLAY_EXPORTS = "DisplayExports";
        readonly static public string ACTION_DISPLAY_GENERAL_REPORTS = "DisplayGeneralReports";
        readonly static public string ACTION_DISPLAY_BOE_STATUS_REPORT = "DisplayBOEStatusReport";
        readonly static public string ACTION_DISPLAY_WORKSPACE_ACTIVITY_REPORT = "DisplayWorkspaceActivityReport";
        readonly static public string ACTION_DISPLAY_BOE_ACTIVITY_REPORT = "DisplayBOEActivityReport";
        readonly static public string ACTION_EXPORT = "Export";
        readonly static public string ACTION_DISPLAY_EXPORT_TO_PROPRICER_INDEX = "ProPricerIndex";
        readonly static public string ACTION_DISPLAY_EXPORT_TO_PROPRICER = "DisplayProPricer";
        readonly static public string ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID = "DisplayProPricerGrid";
        readonly static public string ACTION_DELETE_PROPRICER_EXPORT_FORMATS = "DeleteProPricerExportFormats";
        readonly static public string ACTION_SAVE_PROPRICER_EXPORT_FORMAT = "SaveProPricerExportFormat";
        readonly static public string ACTION_EXPORT_PROPRICER_EXPORT_FORMAT = "ExportProPricerExportFormat";
        readonly static public string ACTION_DISPLAY_DTC_REPORT = "DisplayDesignToCostReport";
        readonly static public string ACTION_VALIDATE_BOE_CALC = "ValidateBOECalculation";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_REPORT_SELECTOR = "DisplayBoeCustomReportSelector";
        readonly static public string ACTION_VALIDATE_BOE_CUSTOM_REPORT = "PreValidateCustomExport";
        readonly static public string ACTION_EXPORT_BOE_CUSTOM_REPORT = "ExportCustomReport";

        #endregion Reports

        #region SHARED

        readonly static public string ACTION_SHARED_DISPLAY_MASTER_MENU = "DisplaySiteMasterMenu";

        #endregion SHARED

        #region WBS

        readonly static public string ACTION_COMPLETE_IMPORT_WBS = "CompleteImportWBS";
        readonly static public string ACTION_CREATE_BOES = "CreateBOEs";
        readonly static public string ACTION_DISPLAY_MANAGE_WBS = "DisplayManageWBS";
        readonly static public string ACTION_DISPLAY_MANAGE_WBS_GRID = "DisplayManageWBSGrid";
        readonly static public string ACTION_DISPLAY_IMPORT_WBS = "DisplayImportWBSButton";
        readonly static public string ACTION_EXPORT_WBS = "ExportWBS";
        readonly static public string ACTION_EXPORT_WBS_TEMPLATE = "ExportWBSTemplate";
        readonly static public string ACTION_GET_BOE_COUNT_FOR_WBS = "GetBOECountForWBS";
        readonly static public string ACTION_IMPORT_WBS = "ImportWBS";
        readonly static public string ACTION_PAGE_MANAGE_WBS = "PageManageWBS";
        readonly static public string ACTION_SAVE_MANAGE_WBS_UPDATES = "SaveManageWBSUpdates";
        readonly static public string ACTION_UPLOAD_WBS_FILE = "UploadWBSFile";

        #endregion

        #region FindReplace

        readonly static public string ACTION_FIND_REPLACE = "FindReplace";
        readonly static public string ACTION_DISPLAY_FIND_REPLACE = "DisplayFindReplace";
        readonly static public string ACTION_FIND_REPLACE_RESULTS = "FindAllforReplace";
        readonly static public string ACTION_SAVE_REPLACED_VALUES = "SaveReplacedValues";
        readonly static public string ACTION_PAGE_FIND_REPLACE_RESULTS = "PageFindResults";


        #endregion FindReplace

        #region Workspace

        #region Display

        readonly static public string ACTION_DISPLAY_ADD_WORKSPACE_RESOURCE_RATE = "DisplayAddWorkspaceResourceRate";
        readonly static public string ACTION_DISPLAY_BACKUP_VERSIONS = "DisplayBackupVersions";
        readonly static public string ACTION_DISPLAY_BACKUP_VERSIONS_GRID = "DisplayBackupVersionsGrid";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD = "DisplayBOECustomField";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_GRID = "DisplayBOECustomFieldsGrid";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE = "DisplayBOECustomFieldResource";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_GRID = "DisplayBOECustomFieldResourceGrid";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT = "DisplayBOECustomFieldResourceViewDefault";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG = "DisplayBOECustomFieldPerfOrg";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID = "DisplayBOECustomFieldPerfOrgGrid";
        readonly static public string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT = "DisplayBOECustomFieldPerfOrgViewDefault";
        readonly static public string ACTION_DISPLAY_DESIGN_TO_COST = "DisplayDesignToCost";
        readonly static public string ACTION_DISPLAY_DISTRIBUTED_TIME_SYSTEM = "DisplayDistributedTimeSystem";
        readonly static public string ACTION_DISPLAY_DISTRIBUTED_TIME_SYSTEM_GRID = "DisplayDistributedTimeSystemGrid";
        readonly static public string ACTION_DISPLAY_DISTRIBUTED_TIME_SYSTEM_SEGMENT = "DisplayDistributedTimeSystemSegment";
        readonly static public string ACTION_DISPLAY_IMS_IMPORT_PAGE = "DisplayIMSImportPage";
        readonly static public string ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN = "DisplayVariableBOESumByCLIN";
        readonly static public string ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS = "DisplayVariableBOESumByWBS";
        readonly static public string ACTION_DISPLAY_WORKSPACE_ALLOW_SEARCH = "DisplayWorkspaceAllowSearch";
        readonly static public string ACTION_DISPLAY_WORKSPACE_HOME_GRID = "DisplayWorkspaceHomeGrid";
        readonly static public string ACTION_DISPLAY_WORKSPACE_HOME_HELP = "DisplayWorkspaceHomeHelp";
        readonly static public string ACTION_DISPLAY_WORKSPACE_IDENTIFICATION = "DisplayWorkspaceIdentification";
        readonly static public string ACTION_DISPLAY_WORKSPACE_OUTPUT_FORMAT = "DisplayWorkspaceOutputFormat";
        readonly static public string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES = "DisplayWorkspaceResourceRates";
        readonly static public string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_GRID = "DisplayWorkspaceResourceRatesGrid";
        readonly static public string ACTION_DISPLAY_WORKSPACE_SETTINGS_JUMP = "DisplayWorkspaceSettingsJump";
        readonly static public string ACTION_DISPLAY_WORKSPACE_SHOW_GETTING_STARTED_HELP = "DisplayWorkspaceShowGettingStartedHelp";
        readonly static public string ACTION_DISPLAY_WORKSPACE_STATUS = "DisplayWorkspaceStatus";
        readonly static public string ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID = "DisplayWorkspaceStatusHistoryGrid";
        readonly static public string ACTION_DISPLAY_WORKSPACE_VARIABLES = "DisplayWorkspaceVariables";
        readonly static public string ACTION_DISPLAY_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES = "WorkspaceResourceRatesImportPreview";
        readonly static public string ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG = "DisplayUpdateWorkspaceRatesDialog";
        readonly static public string ACTION_DISPLAY_WORKSPACE_CALCULATE_LABOR_COST = "DisplayWorkspaceCalculateLaborCostSetting";

        #endregion Display

        readonly static public string ACTION_DELETE_ALL_WORKSPACE_VARIABLES = "DeleteAllWorkspaceVariables";
        readonly static public string ACTION_DELETE_CUSTOM_FIELD = "DeleteCustomField";
        readonly static public string ACTION_DELETE_WORKSPACE_VERSIONS = "DeleteWorkspaceVersions";
        readonly static public string ACTION_EXPORT_BOE_CUSTOM_FIELD = "ExportBOECustomField";
        readonly static public string ACTION_EXPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG = "ExportBOECustomFieldPerfOrg";
        readonly static public string ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE = "ExportBOECustomFieldResource";
        readonly static public string ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE_TEMPLATE = "ExportBOECustomFieldResourceTemplate";
        readonly static public string ACTION_FIND_VALID_BOES_FOR_WORKSPACE_VARIABLE = "FindValidBOEsForWorkspaceVariable";
        readonly static public string ACTION_FIND_VALID_BOES_FOR_NOT_IN_USE_WORKSPACE_VARIABLE = "FindValidBOEsForNotInUseWorkspaceVariable";
        readonly static public string ACTION_GET_RESOURCES_BY_SEGMENT = "GetResourcesBySegment";
        readonly static public string ACTION_IMPORT_ARTEMIS_IMS = "ImportIMSArtemis";
        readonly static public string ACTION_IMPORT_BOE_CUSTOM_FIELD = "ImportBOECustomField";
        readonly static public string ACTION_IMPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG = "ImportBOECustomFieldPerfOrg";
        readonly static public string ACTION_IMPORT_BOE_CUSTOM_FIELD_RESOURCE = "ImportBOECustomFieldResource";
        readonly static public string ACTION_IMPORT_PROJECT_IMS = "ImportIMSProject";
        readonly static public string ACTION_PAGE_CUSTOM_FIELD_PERFORMING_ORGS = "PageCustomFieldPerformingOrgs";
        readonly static public string ACTION_PAGE_CUSTOM_FIELD_RESOURCES = "PageCustomFieldResources";
        readonly static public string ACTION_PAGE_DESIGN_TO_COST = "PageDesignToCost";
        readonly static public string ACTION_REFRESH_VARIABLE_BOE_SUM_BY_CLIN = "RefreshVariableBOESumByCLIN";
        readonly static public string ACTION_REFRESH_VARIABLE_BOE_SUM_BY_WBS = "RefreshVariableBOESumByWBS";
        readonly static public string ACTION_RESTORE_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS = "RestoreCustomFieldPerformingOrganizations";
        readonly static public string ACTION_RESTORE_WORKSPACE_VERSION = "RestoreWorkspaceVersion";
        readonly static public string ACTION_SAVE_CUSTOM_FIELDS = "SaveCustomFields";
        readonly static public string ACTION_SAVE_CUSTOM_FIELD_PERFORMING_ORGS = "SaveCustomFieldPerformingOrgs";
        readonly static public string ACTION_SAVE_CUSTOM_FIELD_RESOURCES = "SaveCustomFieldResources";
        readonly static public string ACTION_SAVE_DTC = "SaveDTC";
        readonly static public string ACTION_SAVE_HIDE_GETTING_STARTED_HELP_MENU = "SaveHideGettingStartedHelpMenu";
        readonly static public string ACTION_SAVE_WORKSPACE_ALLOW_SEARCH = "SaveWorkspaceAllowSearch";
        readonly static public string ACTION_SAVE_WORKSPACE_DTS_SEGMENT_DETAILS = "SaveWorkspaceDTSSegmentDetails";
        readonly static public string ACTION_SAVE_WORKSPACE_DTS_SETTING = "SaveWorkspaceDTSSetting";
        readonly static public string ACTION_SAVE_WORKSPACE_IDENTIFICATION = "SaveWorkspaceIdentification";
        readonly static public string ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT = "SaveWorkspaceOutputFormat";
        readonly static public string ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT_NEW_TEMPLATE = "SaveNewOutputFormatTemplate";
        readonly static public string ACTION_SAVE_WORKSPACE_RESOURCE_RATE = "SaveWorkspaceResourceRate";
        readonly static public string ACTION_SAVE_WORKSPACE_STATUS = "SaveWorkspaceStatus";
        readonly static public string ACTION_SAVE_WORKSPACE_VARIABLES = "SaveWorkspaceVariables";
        readonly static public string ACTION_SAVE_WORKSPACE_VERSION = "SaveWorkspaceVersion";
        readonly static public string ACTION_SEARCH_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS = "SearchCustomFieldPerformingOrganizations";
        readonly static public string ACTION_SEARCH_CUSTOM_FIELD_RESOURCES = "SearchCustomFieldResources";
        readonly static public string ACTION_STATE_TRANSITION_VALIDATION = "WorkspaceStatusChangeValidation";
        readonly static public string ACTION_WORKSPACE_HOME = "SetUpWorkspaceHome";
        readonly static public string ACTION_WORKSPACE_OUTPUT_REQUEST_NEW_TEMPLATE = "WorkspaceOutputRequestNewTemplate";
        readonly static public string ACTION_WORKSPACE_SETTINGS = "WorkspaceSettings";
        readonly static public string ACTION_IS_WORKSPACE_NAME_AVAILABLE = "IsWorkspaceNameAvailable";
        readonly static public string ACTION_IS_WORKSPACE_SHORT_NAME_AVAILABLE = "IsWorkspaceShortNameAvailable";
        readonly static public string ACTION_UPDATE_WORKSPACE_RESOURCE_RATES = "UpdateWorkspaceResourceRate";
        readonly static public string ACTION_IMPORT_WORKSPACE_RESOURCE_RATES = "ImportWorkspaceResourceRates";
        readonly static public string ACTION_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES = "ImportPreviewResultsForWorkspaceResourceRates";
        readonly static public string ACTION_EXPORT_WORKSPACE_RESOURCE_RATES = "ExportWorkspaceResourceRates";
        readonly static public string ACTION_EXPORT_BLANK_WORKSPACE_RESOURCE_RATES = "ExportBlankWorkspaceResourceRates";
        readonly static public string ACTION_SAVE_WORKSPACE_CALCULATE_LABOR_COST = "SaveWorkspaceCalculateLaborCost";
        readonly static public string ACTION_VALIDATE_WORKSPACE_DATE_SHIFT = "ValidateWorkspaceDateShift";
        readonly static public string ACTION_GET_DATE_SHIFT_DATA = "GetDateShiftData";
        readonly static public string ACTION_SHIFT_DATES = "ShiftDates";
        readonly static public string ACTION_START_DATE_SHIFT_WORKFLOW = "StartDateShiftWorkflow";

        #endregion Workspace

        #region MISSION TASKS

        readonly static public string ACTION_IMPORT_MISSION_SUB_IWTA = "ImportMissionSubIWTA";
        readonly static public string ACTION_IMPORT_MISSION_SUB_IWTA_AS_BOE_AUTHOR = "ImportMissionSubIWTAAsBOEAuthor";

        readonly static public string ACTION_COMPLETE_IMPORT_MISSION_SUB_IWTA = "CompleteImportMissionSubIWTA";
        readonly static public string ACTION_COMPLETE_IMPORT_MISSION_SUB_IWTA_AS_BOE_AUTHOR = "CompleteImportMissionSubIWTAAsBOEAuthor";

        readonly static public string ACTION_IMPORT_MISSION_SUB_IWTA_SPREADS = "ImportMissionSubIWTASpreads";
        readonly static public string ACTION_IMPORT_MISSION_SUB_IWTA_SPREADS_AS_BOE_AUTHOR = "ImportMissionSubIWTASpreadsAsBOEAuthor";

        readonly static public string ACTION_COMPLETE_IMPORT_MISSION_SUB_IWTA_SPREADS = "CompleteImportMissionSubIWTASpreads";
        readonly static public string ACTION_COMPLETE_IMPORT_MISSION_SUB_IWTA_SPREADS_AS_BOE_AUTHOR = "CompleteImportMissionSubIWTASpreadsAsBOEAuthor";

        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_TYPES_AS_WORKSPACE_ADMIN = "ExportMissionResourceTypesAsWorkspaceAdmin";
        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_TYPES_AS_BOE_AUTHOR = "ExportMissionResourceTypesAsBOEAuthor";

        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_TYPES_TEMPLATE_AS_WORKSPACE_ADMIN = "ExportMissionResourceTypesTemplateAsWorkspaceAdmin";
        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_TYPES_TEMPLATE_AS_BOE_AUTHOR = "ExportMissionResourceTypesTemplateAsBOEAuthor";

        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_SPREADS_AS_WORKSPACE_ADMIN = "ExportMissionResourceSpreadsAsWorkspaceAdmin";
        readonly static public string ACTION_EXPORT_MISSION_RESOURCE_SPREADS_AS_BOE_AUTHOR = "ExportMissionResourceSpreadsAsBOEAuthor";


        #endregion

        #region AUTOCOMPLETE
        readonly static public string ACTION_AUTOCOMPLETE_RESOURCES = "AutoCompleteResources";
        #endregion AUTOCOMPLETE

        #endregion ACTION

        #region UI EVENTS

        readonly static public string EVENT_BOESUMMARYGRID_HOURS_UPDATED = "BOESUMMARYGRID_HOURS_UPDATED";
        readonly static public string EVENT_BOESUMMARYGRID_RELOAD = "BOESUMMARYGRID_RELOAD";
        readonly static public string EVENT_MOQ_HOURS_EQUATION_UPDATED = "MOQ_HOURS_EQUATION_UPDATED";
        readonly static public string EVENT_TASK_COST_TOTAL_UPDATED = "TASK_COST_TOTAL_UPDATED";
        readonly static public string EVENT_WIDGET_DATA_CLEAN = "DATA_CLEANED";
        readonly static public string EVENT_IMPORTWBS_ENABLE_IMPORT_WBS_BUTTON = "IMPORTWBS_ENABLE_IMPORT_WBS_BUTTON";
        readonly static public string EVENT_BOEDETAILS_RELOAD_BOE_COMMENT_GRID = "BOEDETAILS_RELOAD_BOE_COMMENT_GRID";
        readonly static public string EVENT_BOEDETAILS_RELOAD_BOE_HISTORY_GRID = "BOEDETAILS_RELOAD_BOE_HISTORY_GRID";
        readonly static public string EVENT_BOEDETAILS_RELOAD_BOE_OTHER_DIRECT_COST_GRID = "BOEDETAILS_RELOAD_BOE_OTHER_DIRECT_COST_GRID";
        readonly static public string EVENT_REPORTS_VIEW_REPORT = "REPORTS_VIEW_REPORT";
        readonly static public string EVENT_METRICS_VIEW = "EVENT_METRICS_VIEW";
        readonly static public string EVENT_BOES_VIEW = "EVENT_BOES_VIEW";

        readonly static public string EVENT_DISPLAY_TASK_ELEMENT_DETAILS = "DISPLAY_TASK_ELEMENT_DETAILS";
        readonly static public string EVENT_DISPLAY_ODC_ELEMENT_DETAILS = "DISPLAY_ODC_ELEMENT_DETAILS";
        readonly static public string EVENT_DISPLAY_TRAVEL_ELEMENT_DETAILS = "DISPLAY_TRAVEL_ELEMENT_DETAILS";
        readonly static public string EVENT_DISPLAY_MATERIAL_ELEMENT_DETAILS = "DISPLAY_MATERIAL_ELEMENT_DETAILS";

        #endregion UI EVENTS

        #region VIEWS

        readonly static public string VIEW_INDEX = "Index";

        #region ADMIN

        readonly static public string VIEW_MANAGE_OUTPUT_TEMPLATES = "ManageOutputFormatTemplates";
        readonly static public string VIEW_MANAGE_OUTPUT_TEMPLATES_GRID = "ManageOutputFormatTemplatesGrid";
        readonly static public string VIEW_MANAGE_DEFAULT_PERF_ORGS = "ManageDefaultPerfOrgs";
        readonly static public string VIEW_MANAGE_DEFAULT_PERF_ORGS_GRID = "ManageDefaultPerfOrgsGrid";
        readonly static public string VIEW_MANAGE_DEFAULT_RESOURCES = "ManageDefaultResources";
        readonly static public string VIEW_MANAGE_DEFAULT_RESOURCES_GRID = "ManageDefaultResourcesGrid";
        readonly static public string VIEW_MANAGE_TRIPS = "ManageTripsForTravel";
        readonly static public string VIEW_MANAGE_TRIPS_GRID = "ManageTripsForTravelGrid";
        readonly static public string VIEW_MANAGE_TRIPS_IMPORT_VERIFICATION = "ManageTripsForTravelImportVerification";
        readonly static public string VIEW_MANAGE_MISC_RATES = "ManageMiscRates";
        readonly static public string VIEW_MANAGE_MISC_RATES_GRID = "ManageMiscRatesGrid";
        readonly static public string VIEW_MANAGE_ESCALATION_RATES = "ManageEscalationRates";
        readonly static public string VIEW_MANAGE_ESCALATION_RATES_GRID = "ManageEscalationRatesGrid";
        readonly static public string VIEW_MANAGE_MILEAGE_REIMBURSEMENT = "ManageMileageReimbursement";
        readonly static public string VIEW_METRICS_ADMIN = "MetricsAdmin";
        readonly static public string VIEW_METRICS_ADMIN_JUMP = "MetricsAdminJump";
        readonly static public string VIEW_METRICS_GROUPS_GRID = "ManageMetricsGroupsGrid";
        readonly static public string VIEW_METRICS_PERMISSIONS_GRID = "ManageMetricsPermissionsGrid";
        readonly static public string VIEW_HISTORICAL_METRICS = "ManageHistoricalMetrics";
        readonly static public string VIEW_HISTORICAL_METRICS_GRID = "ManageHistoricalMetricsGrid";
        readonly static public string VIEW_ADD_HISTORICAL_METRICS = "AddHistoricalMetrics";
        readonly static public string VIEW_SYSTEM_PERMISSIONS_GRID = "ManageSystemPermissionsGrid";
        readonly static public string VIEW_CREATE_WORKSPACE_PERMISSIONS_GRID = "ManageCreateWorkspacePermissionsGrid";
        readonly static public string VIEW_SYSTEM_ADMIN_JUMP = "SystemAdminJump";
        readonly static public string VIEW_SYSTEM_ADMIN = "SystemAdmin";
        readonly static public string VIEW_SYSTEM_RESOURCE_RATES = "SystemResourceRates";
        readonly static public string VIEW_PAGE_SYSTEM_RESOURCE_RATES = "SystemResourceRatesPagedData";

        #endregion ADMIN

        #region BOE

        readonly static public string VIEW_BOE_ADVANCED_SEARCH = "BOEAdvancedSearch";
        readonly static public string VIEW_BOE_BOE_EXPORT = "BOEExport";
        readonly static public string VIEW_BOE_BOE_SUBMIT_FOR_APPROVAL = "BOESubmitForApproval";
        readonly static public string VIEW_BOE_BOE_SUBMIT_FOR_REVIEW = "BOESubmitForReview";
        readonly static public string VIEW_BOE_BOE_VALIDATE = "BOEValidate";
        readonly static public string VIEW_BOE_DETAILS = "BOEDetails";
        readonly static public string VIEW_BOE_HEADER_DESCRIPTION = "BOEHeaderDescription";
        readonly static public string VIEW_BOE_HEADER_SPACE_TITLE = "BOEHeaderSpaceTitle";
        readonly static public string VIEW_BOE_HEADER_ISGS_TITLE = "BOEHeaderISGSTitle";
        readonly static public string VIEW_BOE_HEADER = "BOEHeader";
        readonly static public string VIEW_BOE_QUICK_SEARCH = "BOEQuickSearch";
        readonly static public string VIEW_BOE_SEARCH = "BOESearch";
        readonly static public string VIEW_BOE_SEARCH_RESULTS = "BOESearchResults";
        readonly static public string VIEW_BOE_SUMMARY = "BOESummary";
        readonly static public string VIEW_BOE_VALIDATION_RESULTS = "BOEValidateResults";
        readonly static public string VIEW_EDIT_BOE_INDEX = "EditBOEIndex";
        readonly static public string VIEW_MANAGE_BOE = "ManageBOE";
        readonly static public string VIEW_MANAGE_BOE_GRID = "ManageBOEGrid";
        readonly static public string VIEW_MANAGE_BOE_IMPORT_VERIFICATION = "ManageBOEImportVerification";
        readonly static public string VIEW_TASK_ELEMENT_GRID = "TaskElementGrid";
        readonly static public string VIEW_BOE_COPY_CONFLICTS = "BOECopyConflicts";

        readonly static public string VIEW_TASK_ELEMENT_COMPOSITE = "TaskElementsComposite";
        readonly static public string VIEW_ODC_ELEMENT_COMPOSITE = "BOEOtherDirectCostComposite";
        readonly static public string VIEW_TRAVEL_ELEMENT_COMPOSITE = "BOETravelComposite";

        readonly static public string VIEW_DTS_COMPOSITE = "DTSComposite";
        readonly static public string VIEW_DTS_TASK_ELEMENT_DETAILS = "DTSTaskElementDetails";
        readonly static public string VIEW_DTS_RESOURCE_TYPES = "DTSResourceTypes";
        readonly static public string VIEW_DTS_RESOURCE_SPREAD = "DTSResourceSpread";

        //these 3 views make up the compposite view
        readonly static public string VIEW_LABOR_SPREAD = "LaborSpread";
        readonly static public string VIEW_LABOR_SPREAD_GRID = "LaborSpreadGrid";
        readonly static public string VIEW_LABOR_TYPES = "LaborTypes";
        readonly static public string VIEW_LABOR_TYPES_GRID = "LaborTypesGrid";
        readonly static public string VIEW_MISSION_LABOR_TYPES_GRID = "MissionLaborTypesGrid";
        readonly static public string VIEW_TASK_ELEMENT_DETAILS = "TaskElementDetails";
        readonly static public string VIEW_MOQ_EQUATION_FIELD = "MOQEquationField";

        readonly static public string VIEW_TASK_IMPORT_MISSION_SUB_IWTA = "TaskImportMissionSubIWTA";

        readonly static public string VIEW_BOE_HISTORICAL_METRICS_SEARCH_RESULTS = "HistoricalMetricsSearchResults";
        readonly static public string VIEW_ADD_HISTORICAL_METRIC_TO_BOE = "AddHistoricalMetricToBOE";

        readonly static public string VIEW_BOE_COMMENTS_GRID = "BOECommentsGrid";

        readonly static public string VIEW_BOE_HISTORY_GRID = "BOEHistoryGrid";

        readonly static public string VIEW_BOE_OTHER_DIRECT_COST_COMPOSITE = "BOEOtherDirectCostComposite";
        readonly static public string VIEW_BOE_OTHER_DIRECT_COST_GRID = "BOEOtherDirectCostGrid";
        readonly static public string VIEW_ODC_ELEMENT_DETAILS = "ODCElementDetails";
        readonly static public string VIEW_ODC_TYPES = "ODCTypes";
        readonly static public string VIEW_ODC_TYPES_GRID = "ODCTypesGrid";
        readonly static public string VIEW_ODC_SPREAD_GRID = "ODCSpreadGrid";
        readonly static public string VIEW_ODC_SPREAD = "ODCSpread";

        readonly static public string VIEW_BOE_TRAVEL_COMPOSITE = "BOETravelComposite";
        readonly static public string VIEW_BOE_TRAVEL_GRID = "BOETravelGrid";
        readonly static public string VIEW_TRAVEL_ELEMENT_DETAILS = "TravelElementDetails";
        readonly static public string VIEW_TRAVEL_TRIPS = "TravelTrips";
        readonly static public string VIEW_TRAVEL_TRIPS_GRID = "TravelTripsGrid";

        #region BOEMaterial
        readonly static public string VIEW_MATERIAL_COMPOSITE = "BOEMaterialComposite";
        readonly static public string VIEW_BOE_MATERIAL_GRID = "BOEMaterialGrid";
        readonly static public string VIEW_MATERIAL_ELEMENT_DETAILS = "BOEMaterialElementDetails";
        readonly static public string VIEW_MATERIAL_TYPES = "BOEMaterialTypes";
        readonly static public string VIEW_MATERIAL_TYPES_GRID = "BOEMaterialTypesGrid";
        readonly static public string VIEW_MATERIAL_IMPORT_VERIFICATION = "MaterialsImportVerification";

        readonly static public string ACTION_DELETE_BOE_MATERIAL = "DeleteBOEMaterial";
        readonly static public string ACTION_DELETE_ALL_MATERIAL_TYPES = "DeleteAllMaterials";
        #endregion BOEMaterial

        #endregion BOE

        #region CLIN
        readonly static public string VIEW_MANAGE_CLINS = "ManageCLINs";
        readonly static public string VIEW_MANAGE_CLINS_GRID = "ManageCLINGrid";

        #endregion CLIN

        #region HOME

        readonly static public string VIEW_HOME_GENBOE_INDEX = "GenBOEIndex";
        readonly static public string VIEW_HOME_GENBOE_INDEX_SUBCONTRACTOR = "GenBOEIndexSubcontractor";
        readonly static public string VIEW_HOME_CHOOSE_WORKSPACE = "ChooseWorkspace";
        readonly static public string VIEW_HOME_CREATE_WORKSPACE = "CreateWorkspace";
        readonly static public string VIEW_HOME_GENBOE_METRICS = "GenBOEMetrics";
        readonly static public string VIEW_HOME_WHOSONLINE_DIV = "GenBOEMetricsWhosOnlineDiv";
        readonly static public string VIEW_HOME_CREATE_WORKSPACE_FORM = "CreateWorkspaceForm";
        readonly static public string VIEW_HOME_CREATE_WORKSPACE_DIV = "CreateWorkspaceDiv";
        readonly static public string VIEW_HOME_WORKSPACE_SEARCH = "WorkspaceSearch";
        readonly static public string VIEW_HOME_WORKSPACE_SEARCH_RESULTS = "WorkspaceSearchResults";
        readonly static public string VIEW_HOME_WORKSPACE_SEARCH_BOE_LIST = "WorkspaceSearchBOEList";
        readonly static public string VIEW_HOME_MASTER_MENU = "HomeMasterMenu";

        #endregion HOME

        #region PERMISSIONS

        readonly static public string VIEW_MANAGE_PERMISSIONS = "ManagePermissions";
        readonly static public string VIEW_MANAGE_PERMISSIONS_GRID = "ManagePermissionsGrid";
        readonly static public string ACTION_CHECK_IF_USER_WILL_LOOSE_ADMIN_ACCESS = "CheckIfUserWillLooseTheirAdminAccess";
        readonly static public string ACTION_SAVE_PERMISSIONS = "SaveNewPermissions";
        readonly static public string ACTION_EDIT_PERMISSIONS = "EditPermissions";
        readonly static public string ACTION_DELETE_USER_PERMISSIONS = "DeleteUserPermissions";
        readonly static public string ACTION_DELETE_GROUP_PERMISSIONS = "DeleteGroupPermissions";

        #endregion PERMISSIONS

        #region REPORTS

        readonly static public string VIEW_EXPORTS = "Exports";
        readonly static public string VIEW_GENERAL_REPORTS = "GeneralReports";
        readonly static public string VIEW_BOE_STATUS_REPORT = "BOEStatusReport";
        readonly static public string VIEW_WORKSPACE_ACTIVITY_REPORT = "WorkspaceActivityReport";
        readonly static public string VIEW_BOE_ACTIVITY_REPORT = "BOEActivityReport";
        readonly static public string VIEW_EXPORT_TO_PROPRICER_INDEX = "ProPricerIndex";
        readonly static public string VIEW_EXPORT_TO_PROPRICER = "ProPricer";
        readonly static public string VIEW_EXPORT_TO_PROPRICER_GRID = "ProPricerGrid";
        readonly static public string VIEW_DTC_REPORT = "DesignToCostReport";
        readonly static public string VIEW_BOE_CUSTOM_REPORT_SELECTOR = "CustomReportSelector";
        readonly static public string VIEW_BOE_CUSTOM_REPORT_SELECTOR_FORM = "CustomReportSelectorForm";

        #endregion REPORTS

        #region SHARED

        readonly static public string VIEW_NOT_FOUND_CONTROL = "NotFoundControl";
        readonly static public string VIEW_INVALID_PARAMETERS = "InvalidParameters";
        readonly static public string VIEW_ERROR = "Error";
        readonly static public string VIEW_SECURITY_ERROR = "SecurityError";
        readonly static public string VIEW_SITE_MASTER_MENU = "SiteMasterMenu";

        #endregion SHARED

        #region WBS

        readonly static public string VIEW_MANAGE_WBS_GRID = "ManageWBSGrid";
        readonly static public string VIEW_MANAGE_WBSS = "ManageWBSs";
        readonly static public string VIEW_WBS_IMPORT_VERIFICATION = "WbsImportVerification";

        #endregion WBS

        #region FindReplace

        readonly static public string VIEW_FIND_REPLACE = "FindReplace";
        readonly static public string VIEW_FIND_REPLACE_FORM = "FindReplaceForm";
        readonly static public string VIEW_FIND_REPLACE_RESULTS = "FindReplaceResults";

        #endregion FindReplace

        #region Workspace

        readonly static public string VIEW_ADD_WORKSPACE_RESOURCE_RATE = "AddWorkspaceResourceRate";
        readonly static public string VIEW_DESIGN_TO_COST = "DesignToCost";
        readonly static public string VIEW_DESIGN_TO_COST_PAGED_DATA = "DesignToCostPagedData";
        readonly static public string VIEW_DISTRIBUTED_TIME_SYSTEM = "DistributedTimeSystem";
        readonly static public string VIEW_DISTRIBUTED_TIME_SYSTEM_GRID = "DistributedTimeSystemGrid";
        readonly static public string VIEW_DISTRIBUTED_TIME_SYSTEM_SEGMENT = "DistributedTimeSystemSegment";
        readonly static public string VIEW_BACKUP_VERSIONS = "ManageBackupVersions";
        readonly static public string VIEW_BACKUP_VERSIONS_GRID = "ManageBackupVersionsGrid";
        readonly static public string VIEW_BOE_CUSTOM_FIELDS_GRID = "BOECustomFieldsGrid";
        readonly static public string VIEW_BOE_CUSTOM_FIELD = "BOECustomField";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_RESOURCE = "BOECustomFieldResource";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_RESOURCE_GRID = "BOECustomFieldResourceGrid";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_RESOURCE_RESTORE = "BOECustomFieldResourceRestore";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT = "BOECustomFieldResourceViewDefault";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG = "BOECustomFieldPerformingOrg";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID = "BOECustomFieldPerformingOrgGrid";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_RESTORE = "BOECustomFieldPerformingOrgRestore";
        readonly static public string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT = "BOECustomFieldPerformingOrgViewDefault";
        readonly static public string VIEW_FOR_IMS_IMPORT_PAGE = "IMSImportPage";
        readonly static public string VIEW_VARIABLE_BOE_SUM_BY_WBS = "VariableBOESumByWBS";
        readonly static public string VIEW_VARIABLE_BOE_SUM_BY_CLIN = "VariableBOESumByCLIN";
        readonly static public string VIEW_WORKSPACE_HOME = "WorkspaceHome";
        readonly static public string VIEW_WORKSPACE_HOME_GRID = "WorkspaceHomeGrid";
        readonly static public string VIEW_WORKSPACE_HOME_HELP = "WorkspaceHomeHelp";
        readonly static public string VIEW_WORKSPACE_SETTINGS = "WorkspaceSettings";
        readonly static public string VIEW_WORKSPACE_IDENTIFICATION = "WorkspaceIdentification";
        readonly static public string VIEW_WORKSPACE_VARIABLES = "WorkspaceVariables";
        readonly static public string VIEW_WORKSPACE_OUTPUT_FORMAT = "WorkspaceOutputFormat";
        readonly static public string VIEW_WORKSPACE_RESOURCE_RATES = "WorkspaceResourceRates";
        readonly static public string VIEW_WORKSPACE_RESOURCE_RATES_GRID = "WorkspaceResourceRatesGrid";
        readonly static public string VIEW_WORKSPACE_SETTINGS_JUMP = "WorkspaceSettingsJump";
        readonly static public string VIEW_WORKSPACE_STATUS = "WorkspaceStatus";
        readonly static public string VIEW_WORKSPACE_STATUS_HISTORY_GRID = "WorkspaceStatusHistoryGrid";
        readonly static public string VIEW_WORKSPACE_ALLOW_SEARCH_FORMAT = "WorkspaceAllowSearch";
        readonly static public string VIEW_WORKSPACE_UPDATE_RESOURCE_RATES = "UpdateWorkspaceResourceRateDialog";
        readonly static public string VIEW_WORKSPACE_CALCULATE_LABOR_COST_SETTING = "WorkspaceCalculateLaborCostSetting";
        readonly static public string VIEW_WORKSPACE_UPDATE_CLINS = "WorkspaceUpdateClinDates";


        #endregion Workspace

        #region Active Directory Search

        readonly static public string VIEW_ACTIVE_DIRECTORY_SEARCH = "ActiveDirectorySearch";

        #endregion

        #endregion VIEWS

        #region Messages

        readonly static public string LABOR_TYPES_COST_WARNING = "One or more selected Resources have an incomplete Cost due to incomplete resource rates.  Please contact your Workspace Administrator to resolve this issue.";

        #endregion Messages
    }
}


