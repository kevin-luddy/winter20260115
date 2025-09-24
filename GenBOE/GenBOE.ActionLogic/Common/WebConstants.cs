// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	public static class WebConstants
	{
		#region URL Routing

		/// <summary>
		/// string.Empty
		/// </summary>
		public static readonly string URL_PATTERN_EMPTY = string.Empty;

		/// <summary>
		/// default/{controller}/{action}
		/// </summary>
		public static readonly string URL_PATTERN_DEFAULT = "default/{controller}/{action}";

		/// <summary>
		/// {workspace}/{controller}/{action}/{id}
		/// </summary>
		public static readonly string URL_PATTERN_WORKSPACE = "{workspace}/{controller}/{action}/{id}";

		/// <summary>
		/// {workspace}/{controller}/{action}/customfield/{customFieldID}
		/// </summary>
		public static readonly string URL_PATTERN_CUSTOM_FIELD = "{workspace}/{controller}/{action}/customfield/{customFieldID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/rate/{workspaceResourceRateID}
		/// </summary>
		public static readonly string URL_PATTERN_WORKSPACE_RATE = "{workspace}/{controller}/{action}/rate/{workspaceResourceRateID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}
		/// </summary>
		public static readonly string URL_PATTERN_BOE = "{workspace}/{controller}/{action}/boe/{boeID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/taskelement/{taskElementID}
		/// </summary>
		public static readonly string URL_PATTERN_BOE_TASK_ELEMENT = "{workspace}/{controller}/{action}/boe/{boeID}/taskelement/{taskElementID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/copyBoe/{copyBoeId}/taskelement/{taskElementID}/destinationTaskElement/{destinationTaskElementId}
		/// </summary>
		public static readonly string URL_PATTERN_BOE_TASK_ELEMENT_COPY_MOQ = "{workspace}/{controller}/{action}/boe/{boeID}/copyBoe/{copyBoeId}/taskelement/{taskElementID}/destinationTaskElement/{destinationTaskElementId}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/type/{taskType}
		/// </summary>
		public static readonly string URL_PATTERN_DUPLICATE_TASKS = "{workspace}/{controller}/{action}/boe/{boeID}/type/{taskType}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/odcelement/{odcElementID}
		/// </summary>
		public static readonly string URL_PATTERN_BOE_ODC = "{workspace}/{controller}/{action}/boe/{boeID}/odcelement/{odcElementID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/travelelement/{travelElementID}
		/// </summary>
		public static readonly string URL_PATTERN_BOE_TRAVEL = "{workspace}/{controller}/{action}/boe/{boeID}/travelelement/{travelElementID}";

		/// <summary>
		/// The URL pattern for dateshift index page
		/// </summary>
		public static readonly string URL_PATTERN_DATESHIFT = "{workspace}/{controller}/{action}/id/{id}/level/{level}";

		/// <summary>
		/// {workspace}/{controller}/{action}/report/{reportID}
		/// </summary>
		public static readonly string URL_PATTERN_REPORTS = "{workspace}/{controller}/{action}/report/{reportID}/{scopeParam}/{scope}";

		/// <summary>
		/// error/{action}
		/// </summary>
		public static readonly string URL_PATTERN_ERROR = "error/{action}";

		/// <summary>
		/// {workspace}/{controller}/{action}/boe/{boeID}/materialelement/{materialID}
		/// </summary>
		public static readonly string URL_PATTERN_BOE_MATERIAL = "{workspace}/{controller}/{action}/boe/{boeID}/materialelement/{materialID}";

		/// <summary>
		/// {workspace}/{controller}/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}
		/// </summary>
		public static readonly string URL_PATTERN_EXPORT_WS_CUSTOM_RESOURCES = "{workspace}/{controller}/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}";

		/// <summary>
		/// admin/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}
		/// </summary>
		public static readonly string URL_PATTERN_EXPORT_ADMIN_CUSTOM_RESOURCES = "admin/{action}/{showLabor}/{showIWTA}/{showSub}/{showODC}/{showTravel}/{showMaterials}/{searchText}";

		public static readonly string ROUTE_WORKSPACE = "WorkspaceRoute";
		public static readonly string ROUTE_REPORT = "ReportRoute";
		public static readonly string ROUTE_DEFAULT = "DefaultRoute";
		public static readonly string ROUTE_BOE = "BoeRoute";

		#endregion URL Routing

		public static readonly string BROWSER_REMINDER = "GenBOEBrowserReminder";
		public static readonly string ECI_FORBIDDEN_BANNER = "EciForbiddenBanner";
		public static readonly string UPDATE_WORKSPACE_TRAVEL_ZONE_RATE = "UpdateWorkspaceTravelZoneRate";
		public static readonly string UPDATE_WORKSPACE_OFFLOAD_RATES = "UpdateWorkspaceOffloadRates";
		public static readonly string UPDATE_WORKSPACE_UCOT_FACTOR = "UpdateWorkspaceUCOTFactor";

		public static readonly string SPACE_LEGACY_TM = "T&M";
		public static readonly string SPACE_1LMX_CORE = "1LMX - Core";
		public static readonly string SPACE_1LMX_SERVICES = "1LMX - Services";
		public static readonly string RMS_1LMX_CORE = "LM-Core";
		public static readonly string RMS_1LMX_SERVICES = "LM-Services";
		public static readonly string RMS_1LMX_I_AND_N = "LM-I&N";

		public static readonly string SKILLMIX_RESOURCE_CACHE_KEY = "SkillMix_Resources";
		public static readonly int SECONDS_TO_CACHE_SKILLMIX_RESOURCES = 86400;

		#region TEMPLATE FILES

		public static readonly string ISGS_LABOR_RATES_EXAMPLE_LOCATION = "~/Templates/Export/ISGS_Labor_Rates_example.xlsx";
		public static readonly string SSC_LABOR_RATES_EXAMPLE_LOCATION = "~/Templates/Export/SSC_Labor_Rates_example.xlsx";
		public static readonly string MST_LABOR_RATES_EXAMPLE_LOCATION = "~/Templates/Export/MST_Labor_Rates_example.xlsx";
		public static readonly string ISGS_LABOR_RATES_EXAMPLE_NAME = "ISGS_Labor_Rates_example.xlsx";
		public static readonly string SSC_LABOR_RATES_EXAMPLE_NAME = "SSC_Labor_Rates_example.xlsx";
		public static readonly string MST_LABOR_RATES_EXAMPLE_NAME = "MST_Labor_Rates_example.xlsx";

		#endregion

		#region CONTROLLERS

		public static readonly string CONTROLLER_ADMIN = "Admin";
		public static readonly string CONTROLLER_BOE = "BOE";
		public static readonly string CONTROLLER_BOE_COMMENTS = "BOEComments";
		public static readonly string CONTROLLER_BOE_HISTORY = "BOEHistory";
		public static readonly string CONTROLLER_BOE_LABOR = "BOELabor";
		public static readonly string CONTROLLER_BOE_FORMS = "BOEForm";
		public static readonly string CONTROLLER_CLIN = "CLIN";
		public static readonly string CONTROLLER_DATESHIFT = "DateShift";
		public static readonly string CONTROLLER_GENBOE = "GenBOE";
		public static readonly string CONTROLLER_HOME = "Home";
		public static readonly string CONTROLLER_PERMISSIONS = "Permissions";
		public static readonly string CONTROLLER_REPORTS = "Reports";
		public static readonly string CONTROLLER_WBS = "WBS";
		public static readonly string CONTROLLER_WORKSPACE = "Workspace";
		public static readonly string CONTROLLER_WORKSPACE_RECALCULATE_ACTUALS = "WorkspaceRecalculateActuals";
		public static readonly string CONTROLLER_FIND_REPLACE = "FindReplace";
		public static readonly string CONTROLLER_BOE_OTHER_DIRECT_COST = "BOEOtherDirectCost";
		public static readonly string CONTROLLER_BOE_MATERIAL = "BOEMaterial";
		public static readonly string CONTROLLER_BOE_TRAVEL = "BOETravel";
		public static readonly string CONTROLLER_BOE_ZONE_TRAVEL = "BOEZoneTravel";
		public static readonly string CONTROLLER_AUTOCOMPLETE = "AutoComplete";
		public static readonly string CONTROLLER_BOE_BULK_SUBMIT = "BOEBulkSubmit";
		public static readonly string CONTROLLER_RTE_TEMPLATES = "RTETemplates";

		#endregion CONTROLLERS

		#region ACTIONS

		public static readonly string ACTION_SYSTEM_EMAIL_PREFERENCES = "GetSystemEmailPreferences";
		public static readonly string ACTION_SAVE_SYSTEM_EMAIL_PREFERENCES = "SaveSystemEmailPreferences";
		public static readonly string ACTION_SAVE_UCOT = "SaveUCOT";
		public static readonly string ACTION_INDEX = "Index";
		public static readonly string ACTION_EDIT_BOE_INDEX = "EditBOEIndex";
		public static readonly string ACTION_INVALID_REQUEST = "InvalidRequest";
		public static readonly string ACTION_MANAGE_PERMISSIONS = "ManagePermissions";
		public static readonly string ACTION_ABOUT_USER = "AboutUser";
		public static readonly string ACTION_LIST_ALL_USERS = "ListAllUsers";
		public static readonly string ACTION_SYSTEM_SETTINGS = "GetSystemSettings";
		public static readonly string ACTION_SAVE_SYSTEM_SETTINGS = "SaveSystemSettings";
		public static readonly string ACTION_IMPORT_OVERDUE_TRAINING = "ImportOverdueTraining";

		public static readonly string ACTION_CANCEL = "Cancel";
		public static readonly string ACTION_ERROR = "Error";

		//readonly static public string ACTION_GET_IMAGE = "GetImage";

		#region TESTING
		public static readonly string ACTION_DISPLAY_FRONTPAGE = "DisplayKitchenSinkFrontPage";
		public static readonly string ACTION_DISPLAY_GENWIDGTDEMO = "DisplayGenWidgetDemo";
		public static readonly string ACTION_DISPLAY_GENGRIDWIDGTDEMO = "DisplayGenGridWidgetDemo";
		#endregion TESTING

		#region ADMIN

		public static readonly string ACTION_ASSIGNED_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE = "GetWorkspacesAssignedForExportTemplateId";
		public static readonly string ACTION_AVAILABLE_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE = "GetAvailableWorkspaceIdsForExportFormatId";
		public static readonly string ACTION_GET_ASSIGNED_WORKSPACE_INFO = "GetAssignedWorkspaceInfo";
		public static readonly string ACTION_CHECK_IS_USER_WILL_LOOSE_THEIR_SYSTEM_ADMIN_ACCESS = "CheckIfUserWillLooseTheirSystemAdminAccess";
		public static readonly string ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_CREATE_WORKSPACE_PERMISSIONS_ACCESS = "CheckIfUserWillLoseTheirCreateWorkspacePermissionsAccess";
		public static readonly string ACTION_COMPLETE_TRIPS_IMPORT = "CompleteTripsImport";
		public static readonly string ACTION_CREATE_EDIT_METRICS_ADMIN_GROUP = "CreateEditMetricAdminGroup";
		public static readonly string ACTION_DELETE_GROUP_METRICS_PERMISSIONS = "DeleteGroupMetricsPermissions";
		public static readonly string ACTION_DELETE_GROUP_SYSTEM_PERMISSIONS = "DeleteGroupSystemPermissions";
		public static readonly string ACTION_DELETE_METRIC_ADMIN_GROUPS = "DeleteMetricAdminGroups";
		public static readonly string ACTION_DELETE_USER_METRICS_PERMISSIONS = "DeleteUserMetricsPermissions";
		public static readonly string ACTION_DELETE_ALL_USER_METRICS_PERMISSIONS = "DeleteAllUserMetricsPermissions";
		public static readonly string ACTION_DELETE_USER_SYSTEM_PERMISSIONS = "DeleteUserSystemPermissions";
		public static readonly string ACTION_DELETE_USER_CREATE_WORKSPACE_PERMISSIONS = "DeleteUserCreateWorkspacePermissions";
		public static readonly string ACTION_DELETE_GROUP_CREATE_WORKSPACE_PERMISSIONS = "DeleteGroupCreateWorkspacePermissions";
		public static readonly string ACTION_DISPLAY_DEFAULT_PERF_ORGS = "DisplayDefaultPerfOrgs";
		public static readonly string ACTION_DISPLAY_DEFAULT_RESOURCES = "DisplayDefaultResources";
		public static readonly string ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID = "DisplayManageDefaultPerfOrgsGrid";
		public static readonly string ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID = "DisplayManageDefaultResourcesGrid";
		public static readonly string ACTION_DISPLAY_MANAGE_OUTPUT_FORMAT_TEMPLATE = "DisplayManageOutputFormatTemplates";
		public static readonly string ACTION_DISPLAY_MANAGE_SYSTEM_SETTINGS = "DisplayManageSystemSettings";
		public static readonly string ACTION_DISPLAY_MANAGE_TRIPS = "DisplayManageTripsForTravel";
		public static readonly string ACTION_DISPLAY_MANAGE_TRIPS_GRID = "DisplayManageTripsForTravelGrid";
		public static readonly string ACTION_CALCULATE_TRAVEL_TRIPS = "CalculateBOETravelTripCost";
		public static readonly string ACTION_DISPLAY_MANAGE_ZONE_TRAVEL = "DisplayManageZoneTravel";
		public static readonly string ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_GRID = "DisplayManageZoneTravelGrid";
		public static readonly string ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_DESTINATIONS = "DisplayManageZoneTravelDestinations";
		public static readonly string ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_DESTINATIONS_GRID = "DisplayManageZoneTravelDestinationsGrid";
		public static readonly string ACTION_DISPLAY_MANAGE_NONZONE_FEES_AND_COSTS = "DisplayManageNonzoneFeesAndCosts";
		public static readonly string ACTION_DISPLAY_MANAGE_NONZONE_FEES_AND_COSTS_GRID = "DisplayManageNonzoneFeesAndCostsGrid";
		public static readonly string ACTION_AUTOCOMPLETE_LOCATION_NAME = "AutocompleteTripLocationName";
		public static readonly string ACTION_AUTOCOMPLETE_DEP_LOCATION_CODE = "AutocompleteTripDepartureLocationCode";
		public static readonly string ACTION_AUTOCOMPLETE_DES_LOCATION_CODE = "AutocompleteTripDestinationLocationCode";
		public static readonly string ACTION_GET_LOCATION_DETAILS = "GetLocationDetails";
		public static readonly string ACTION_AUTOCOMPLETE_PER_DIEM = "AutocompleteTripPerDiemLocation";
		public static readonly string ACTION_GET_PER_DIEM_LOCATION_DETAILS = "GetPerDiemLocationDetails";
		public static readonly string ACTION_AUTOCOMPLETE_QUALIFICATION = "AutocompleteTripQualification";
		public static readonly string ACTION_DELETE_TRIPS = "DeleteTrips";
		public static readonly string ACTION_DELETE_ORIGINS = "DeleteOrigins";
		public static readonly string ACTION_SAVE_TRIP = "SaveTrip";
		public static readonly string ACTION_SAVE_ORIGIN = "SaveOrigin";
		public static readonly string ACTION_SAVE_DESTINATION = "SaveDestination";
		public static readonly string ACTION_SAVE_FEES_AND_COSTS = "SaveFeesAndCosts";
		public static readonly string ACTION_PAGE_TRIP_RESULTS = "PageTripResults";
		public static readonly string ACTION_DISPLAY_MANAGE_MISC_RATES = "DisplayManageMiscRates";
		public static readonly string ACTION_DISPLAY_MANAGE_MISC_RATES_GRID = "DisplayManageMiscRatesGrid";
		public static readonly string ACTION_DISPLAY_ESCALATION_RATES = "DisplayManageEscalationRates";
		public static readonly string ACTION_DISPLAY_ESCALATION_RATES_GRID = "DisplayManageEscalationRatesGrid";
		public static readonly string ACTION_DISPLAY_OFFLOAD_RATES = "DisplayManageOffloadRates";
		public static readonly string ACTION_PAGE_SYSTEM_OFFLOAD_RATES = "PageSystemOffloadRates";
		public static readonly string ACTION_DISPLAY_LEGACY_RESOURCES = "DisplayManageLegacyResources";
		public static readonly string ACTION_DISPLAY_REIMBURSEMENT_RATE = "DisplayManageMileageReimbursement";
		public static readonly string ACTION_DISPLAY_MANAGE_EMAIL_PREFERENCES = "DisplayManageEmailPreferences";
		public static readonly string ACTION_DISPLAY_OVERDUE_TRAINING = "DisplayOverdueTraining";
		public static readonly string ACTION_DISPLAY_SYSTEM_PROPRICER_EXPORTS = "DisplaySystemProPricerExports";
		public static readonly string ACTION_DOWNLOAD_SYSTEM_RESOURCE_RATES_TEMPLATE = "DownloadSystemResourceRatesTemplateExample";
		public static readonly string ACTION_IMPORT_SYSTEM_RESOURCE_RATES = "ImportSystemResourceRates";
		public static readonly string ACTION_IMPORT_PREVIEW_SYSTEM_RESOURCE_RATES = "ImportPreviewResultsForSystemResourceRates";
		public static readonly string ACTION_DELETE_SYSTEM_RESOURCE_RATES = "DeleteSystemResourceRates";
		public static readonly string ACTION_EXPORT_SYSTEM_RESOURCE_RATES = "ExportSystemResourceRates";
		public static readonly string ACTION_EXPORT_ESCALATION_RATES = "ExportEscalationRates";
		public static readonly string ACTION_IMPORT_ESCALATION_RATES = "ImportEscalationRates";
		public static readonly string ACTION_EXPORT_OFFLOAD_RATES = "ExportOffloadRates";
		public static readonly string ACTION_IMPORT_OFFLOAD_RATES = "ImportOffloadRates";
		public static readonly string ACTION_DISPLAY_IMPORT_PREVIEW_SYSTEM_RESOURCE_RATES = "SystemResourceRatesImportPreview";
		public static readonly string ACTION_PAGE_SYSTEM_RESOURCE_RATES = "PageSystemResourceRates";
		public static readonly string ACTION_DISPLAY_METRICS_ADMIN_JUMP = "DisplayMetricsAdminJump";
		public static readonly string ACTION_DISPLAY_METRICS_PERMISSIONS = "DisplayMetricsPermissions";
		public static readonly string ACTION_DISPLAY_METRICS_GROUPS = "DisplayMetricsGroups";
		public static readonly string ACTION_DISPLAY_SYSTEM_ADMIN_JUMP = "DisplaySystemAdminJump";
		public static readonly string ACTION_DISPLAY_SYSTEM_PERMISSIONS = "DisplaySystemPermissions";
		public static readonly string ACTION_DISPLAY_CREATE_WORKSPACE_PERMISSIONS = "DisplayCreateWorkspacePermissions";
		public static readonly string ACTION_EXPORT_DEFAULT_PERFORMING_ORGS = "ExportPerformingOrgs";
		public static readonly string ACTION_EXPORT_DEFAULT_RESOURCES = "ExportResources";
		public static readonly string ACTION_EXPORT_HISTORICAL_METRICS = "ExportHistoricalMetrics";
		public static readonly string ACTION_EXPORT_HISTORICAL_METRICS_TEMPLATE = "ExportHistoricalMetricsTemplate";
		public static readonly string ACTION_EXPORT_TRIPS = "ExportTrips";
		public static readonly string ACTION_EXPORT_TRIPS_TEMPLATE = "ExportTripsTemplate";
		public static readonly string ACTION_EXPORT_ZONE_TRAVEL_ORIGINS = "ExportZoneTravelOrigins";
		public static readonly string ACTION_GET_OUTPUT_FORMAT_TEMPLATE = "GetOutputFormatTemplate";
		public static readonly string ACTION_GET_WORKSPACES_USING_TEMPLATE = "GetWorkspacesUsingTemplate";
		public static readonly string ACTION_IMPORT_DEFAULT_PERFORMING_ORGS = "ImportPerformingOrgs";
		public static readonly string ACTION_IMPORT_DEFAULT_RESOURCES = "ImportResources";
		public static readonly string ACTION_IMPORT_TRIPS = "ImportTrips";
		public static readonly string ACTION_IMPORT_ORIGINS = "ImportOrigins";
		public static readonly string ACTION_METRICS_ADMIN = "MetricsAdmin";
		public static readonly string ACTION_PAGE_PERF_ORGS = "PagePerfOrgs";
		public static readonly string ACTION_PAGE_OUTPUT_FORMAT = "PageOutputFormat";
		public static readonly string ACTION_PAGE_RESOURCES = "PageResources";
		public static readonly string ACTION_PAGE_WORKSPACE_HOME = "PageWorkspaceHome";
		public static readonly string ACTION_SAVE_DEFAULT_PERF_ORGS = "SavePerfOrgs";
		public static readonly string ACTION_SAVE_DEFAULT_RESOURCES = "SaveResources";
		public static readonly string ACTION_SAVE_MISC_RATES = "SaveMiscRates";
		public static readonly string ACTION_DELETE_BOE_FORMS = "DeleteBOEForms";
		public static readonly string ACTION_CREATE_BOE_FORM = "CreateBoeForm";
		public static readonly string ACTION_SAVE_ESCALATION_RATES = "SaveEscalationRates";
		public static readonly string ACTION_SAVE_OFFLOAD_RATES = "SaveOffloadRates";
		public static readonly string ACTION_SAVE_MILEAGE_REIMBURSEMENT = "SaveMileageReimbursement";
		public static readonly string ACTION_SAVE_NEW_METRICS_PERMISSIONS = "SaveNewMetricsPermissions";
		public static readonly string ACTION_SAVE_NEW_SYSTEM_PERMISSIONS = "SaveNewSystemPermissions";
		public static readonly string ACTION_GET_GROUP_MEMBERS = "GetGroupMembers";
		public static readonly string ACTION_SAVE_NEW_CREATE_WORKSPACE_PERMISSIONS = "SaveNewCreateWorkspacePermissions";
		public static readonly string ACTION_SAVE_RESOURCE_LIST = "SaveResourceList";
		public static readonly string ACTION_SAVE_WORKSPACES_FOR_OUTPUT_TEMPLATES = "SaveExportTemplatesForWorkspaces";
		public static readonly string ACTION_SEARCH_PERF_ORGS = "SearchPerfOrgs";
		public static readonly string ACTION_SEARCH_RESOURCES = "SearchResources";
		public static readonly string ACTION_SYSTEM_ADMIN = "SystemAdmin";
		public static readonly string ACTION_EXPORT_DEFAULT_RESOURCES_TEMPLATE = "ExportDefaultResourcesTemplate";
		public static readonly string ACTION_DISPLAY_WHOS_ONELINE = "WhosOnline";
		public static readonly string ACTION_CONVERT_HTML_TO_TEXT = "ConvertHtmlToText";
		public static readonly string ACTION_DISPLAY_MANAGE_UCOT = "DisplayManageUCOT";

		#endregion ADMIN

		#region BOE ACTIONS

		public static readonly string ACTION_DISPLAY_BOE_ADVANCED_SEARCH = "DisplayBOEAdvancedSearch";
		public static readonly string ACTION_DISPLAY_BOE_PROJECTMAP_ADVANCED_SEARCH = "DisplayBOEProjectMapAdvancedSearch";
		public static readonly string ACTION_DISPLAY_BOE_DETAILS = "DisplayBOEDetails";
		public static readonly string ACTION_DISPLAY_BOE_EXPORT = "DisplayExportBOEButton";
		public static readonly string ACTION_DISPLAY_BOE_HEADER_DESCRIPTION = "DisplayBOEHeaderDescription";
		public static readonly string ACTION_DISPLAY_BOE_HEADER_TITLE = "DisplayBOEHeaderTitle";
		public static readonly string ACTION_DISPLAY_BOE_HEADER = "DisplayBOEHeader";
		public static readonly string ACTION_DISPLAY_BOE_QUICK_SEARCH = "DisplayBOEQuickSearch";
		public static readonly string ACTION_DISPLAY_BOE_SEARCH = "DisplayBOESearch";
		public static readonly string ACTION_DISPLAY_PROJECTMAP_BOE_SEARCH = "DisplayProjectMapBOESearch";
		public static readonly string ACTION_DISPLAY_BOE_SUBMIT_FOR_REVIEW = "DisplaySubmitForReviewBOEButton";
		public static readonly string ACTION_DISPLAY_BOE_SUMMARY = "DisplayBOESummary";
		public static readonly string ACTION_DISPLAY_BOE_VALIDATE = "DisplayValidateBOEButton";
		public static readonly string ACTION_DISPLAY_BOE_OFFLOAD = "DisplayOffloadBOEButton";
		public static readonly string ACTION_DISPLAY_BOE_VALIDATE_RESULTS = "DisplayBOEValidateResults";
		public static readonly string ACTION_DISPLAY_BOE_OFFLOAD_RESULTS = "DisplayBOEOffloadResults";
		public static readonly string ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_BUTTON = "DisplayConfidenceReportButton";
		public static readonly string ACTION_DISPLAY_BOE_CONFIDENCE_REPORT = "ConfidenceReport";
		public static readonly string ACTION_DISPLAY_INVALID_SUBMIT_FOR_APPROVAL = "DisplayInvalidSubmitForApproval";
		public static readonly string ACTION_DISPLAY_LABOR_CURVES = "DisplayLaborCurves";
		public static readonly string ACTION_DISPLAY_LABOR_RESOURCES = "DisplayResources";
		public static readonly string ACTION_DISPLAY_BUSINESS_RESOURCE_CODES = "DisplayBusinessResourceCodes";
		public static readonly string ACTION_DISPLAY_LABOR_PERF_ORGS = "DisplayPerfOrgs";
		public static readonly string ACTION_DISPLAY_MANAGE_BOE = "DisplayManageBOE";
		public static readonly string ACTION_DISPLAY_WORKSPACE_CALCULATE_ACTUALS = "DisplayWorkspaceCalculateActuals";
		public static readonly string ACTION_DISPLAY_MANAGE_BOE_FORMS = "DisplayManageBOEForms";
		public static readonly string ACTION_DISPLAY_MANAGE_BOE_FORMS_GRID = "DisplayManageBOEFormsGrid";
		public static readonly string ACTION_DISPLAY_MANAGE_BOE_GRID = "DisplayManageBOEGrid";
		public static readonly string ACTION_DISPLAY_SUBMIT_FOR_APPROVAL = "DisplaySubmitForApproval";
		public static readonly string ACTION_DISPLAY_TASK_ELEMENT_GRID = "DisplayTaskElementGrid";

		public static readonly string ACTION_ADVANCED_SEARCH_FOR_BOES = "AdvancedSearchForBOEs";
		public static readonly string ACTION_PROJECTMAP_ADVANCED_SEARCH_FOR_BOES = "ProjectMapAdvancedSearchForBOEs";
		public static readonly string ACTION_BOE_SUBMIT_FOR_REVIEW = "SubmitForReview";
		public static readonly string ACTION_COMPLETE_IMPORT_MANAGE_BOE = "CompleteImportManageBOE";
		public static readonly string ACTION_DATA_BOE = "LoadBOEDATA";
		public static readonly string ACTION_DELETE_TASK_ELEMENTS = "DeleteTaskElements";
		public static readonly string ACTION_EXPORT_BOE = "ExportBOEToWordFile";
		public static readonly string ACTION_EXPORT_CONFIDENCE_REPORT = "ExportConfidenceReport";
		public static readonly string ACTION_BOE_SEARCH_PREVIEW = "BOESearchPreview";
		public static readonly string ACTION_PROJECTMAP_SEARCH_PREVIEW = "ProjectMapSearchPreview";
		public static readonly string ACTION_EXPORT_MANAGE_BOE = "ExportManageBOE";
		public static readonly string ACTION_EXPORT_MANAGE_BOE_TEMPLATE = "ExportManageBOETemplate";
		public static readonly string ACTION_GET_UPDATED_BOE_HEADER = "GetUpdatedBOEHeader";
		public static readonly string ACTION_IMPORT_MANAGE_BOE = "ImportManageBOE";
		public static readonly string ACTION_IMPORT_WORKSPACEHOME_BOES = "ImportWorkspaceHomeBOEs";
		public static readonly string ACTION_PAGE_SEARCH_RESULTS = "PageSearchResults";
		public static readonly string ACTION_QUICK_SEARCH_FOR_BOES = "QuickSearchForBOEs";
		public static readonly string ACTION_SAVE_DUPLICATE_TASK_ELEMENTS = "SaveDuplicateTaskElements";
		public static readonly string ACTION_SAVE_EDIT_BOE_HEADER = "SaveEditBOEHeader";
		public static readonly string ACTION_SAVE_PBOE_FORM = "SavePBOE";
		public static readonly string ACTION_SAVE_IBOE_FORM = "SaveIBOE";
		public static readonly string ACTION_VALIDATE_IBOE_FORM = "ValidateIBOEForm";
		public static readonly string ACTION_VALIDATE_PBOE_FORM = "ValidatePBOEForm";
		public static readonly string ACTION_SAVE_MANAGE_BOE = "SaveManageBOE";
		public static readonly string ACTION_RESET_DRAFT_BOE = "ResetDraftBOE";
		public static readonly string ACTION_WORKSPACE_CALCULATE_ACTUALS = "WorkspaceCalculateActuals";
		public static readonly string ACTION_SAVE_BOE_STATES = "SaveBOEStates";
		public static readonly string ACTION_SUBMIT_FOR_APPROVAL = "SubmitForApproval";
		public static readonly string ACTION_VALIDATE_BOE = "ValidateBOE";
		public static readonly string ACTION_BOE_CONTAINS_SUM_OF_BOES = "BOEContainsSumOfBOEs";
		public static readonly string ACTION_BOE_COPY_CONFLICTS = "DisplayCopyBOEConflicts";
		public static readonly string ACTION_SAVE_COPY_OF_BOE = "SaveCopyOfBOE";
		public static readonly string ACTION_SAVE_COPY_OF_PROJECTMAP = "SaveCopyOfProjectMap";
		public static readonly string ACTION_COPY_MOQ_EQUATION = "CopyMoqEquation";
		public static readonly string ACTION_DELETE_ALL_BOE_TASK_ELEMENTS = "DeleteAllBOETaskElements";
		public static readonly string ACTION_SAVE_BULK_ROLE_ASSIGN = "SaveBoeBulkRoles";

		// BOEComments Controller
		public static readonly string ACTION_DISPLAY_BOE_COMMENTS = "DisplayBOEComments";
		public static readonly string ACTION_SAVE_BOE_COMMENTS = "SaveBoeComments";

		// BOEHistory Controller
		public static readonly string ACTION_DISPLAY_BOE_HISTORY = "DisplayBOEHistory";


		// BOELabor Controller
		public static readonly string ACTION_CALCULATE_SPREAD = "CalculateSpread";
		public static readonly string ACTION_CALCULATE_DISCRETE_UCOT_SPREAD = "CalculateDiscreteUCOTSpread";
		public static readonly string ACTION_SAVE_TASK_DATA_MODEL = "SaveTaskDataModel";
		public static readonly string ACTION_GET_TASK_DATA_MODEL = "GetTaskDataModel";
		public static readonly string ACTION_DISPLAY_IWTA_TASK_ELEMENT_DETAILS = "DisplayIWTATaskElementDetails";
		public static readonly string ACTION_DISPLAY_TASK_ELEMENT = "DisplayTask";
		public static readonly string ACTION_DISPLAY_MOQ_HOURS_EQUATION_FIELD = "DisplayMOQHoursEquationField";
		public static readonly string ACTION_IMPORT_LABOR_SPREAD = "ImportLaborSpread";
		public static readonly string ACTION_COMPLETE_IMPORT_LABOR_SPREAD = "CompleteImportLaborSpread";
		public static readonly string ACTION_IMPORT_LABOR_TYPE_AND_SPREAD = "ImportLaborTypeAndSpread";
		public static readonly string ACTION_PREVIEW_IMPORT_LABOR_TYPE_AND_SPREAD = "ImportPreviewLaborTypeAndSpread";
		public static readonly string ACTION_EXPORT_LABOR_TYPE_AND_SPREAD = "ExportLaborTypeAndSpread";
		public static readonly string ACTION_MOQ_VALIDATE = "MOQValidate";
		public static readonly string ACTION_MOQ_CALCULATE = "MOQCalculate";
		public static readonly string ACTION_VALIDATE_PERFORMING_ORGS = "ValidatePerformingOrgs";
		public static readonly string ACTION_GET_PERFORMING_ORG_ID_BYNAME = "GetPerformingOrgIDByName";
		public static readonly string ACTION_VALIDATE_RESOURCE = "ValidateResource";
		public static readonly string ACTION_VALIDATE_RESOURCE_BY_ID = "ValidateResourceByID";
		public static readonly string ACTION_MARK_WARNING_AS_CONFIRMED = "MarkWarningMessageAsConfirmed";
		public static readonly string ACTION_GET_SEARCH_TYPE_AHEAD = "GetTypeAheadTerms";
		public static readonly string ACTION_SAVE_REORDER_LABOR_TASK_ELEMENTS = "SaveReorderLaborTaskElements";
		public static readonly string ACTION_SAVE_REORDER_LABOR_TYPES = "SaveReorderLaborTypes";
		public static readonly string ACTION_LOAD_DUPLICATE_TASK_DIALOG = "LoadDuplicateTaskDialog";
		public static readonly string ACTION_IMPORT_MOQ_TABLES = "ImportMoqTables";
		public static readonly string ACTION_COMPLETE_IMPORT_MOQ_TABLES = "CompleteImportMoqTables";
		public static readonly string ACTION_EXPORT_MOQ_TABLES = "ExportMoqTables";
		public static readonly string ACTION_PARSE_SAP_FILTER = "ParseSapFilter";
		public static readonly string ACTION_CONVERT_SAP_FILTER = "ConvertSapFilter";
		public static readonly string ACTION_CALCULATE_ALL_ACTUALS_SAP = "CalculateAllActualsSap";
		public static readonly string ACTION_CALCULATE_ALL_ACTUALS_SAP_WITH_SKILL_MIX = "CalculateAllActualsSapWithSkillMix";
		public static readonly string ACTION_REFRESH_SKILL_MIX_TABLES = "RefreshSkillMixTables";
		public static readonly string ACTION_CHECK_TM_RATES = "CheckTMRates";
		public static readonly string ACTION_EXPORT_ACTUALS_SAP = "ExportActualsSap";
		public static readonly string ACTION_GET_SKILLMIX_CONVERTED_RESOURCES = "GetSkillMixConvertedResources";

		//BOE ODC Controller
		public static readonly string ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_GRID = "DisplayBOEOtherDirectCostGrid";
		public static readonly string ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_COMPOSITE = "DisplayBOEOtherDirectCostComposite";
		public static readonly string ACTION_DISPLAY_BOE_ODC_ELEMENT_DETAILS = "DisplayBOEODCElementDetails";
		public static readonly string ACTION_DISPLAY_BOE_ODC_TYPES = "DisplayBOEODCTypes";
		public static readonly string ACTION_DISPLAY_BOE_ODC_TYPES_GRID = "DisplayBOEODCTypesGrid";
		public static readonly string ACTION_DISPLAY_BOE_ODC_SPREAD = "DisplayBOEODCSpread";
		public static readonly string ACTION_DISPLAY_BOE_ODC_SPREAD_GRID = "DisplayBOEODCSpreadGrid";
		public static readonly string ACTION_IMPORT_ODC_TYPE = "ImportODCType";
		public static readonly string ACTION_PREVIEW_IMPORT_ODC_TYPE = "ImportPreviewODCType";
		public static readonly string ACTION_IMPORT_ODC_SPREAD = "ImportODCSpread";
		public static readonly string ACTION_COMPLETE_IMPORT_ODC_SPREAD = "CompleteImportODCSpread";
		public static readonly string ACTION_EXPORT_ODC_TYPE = "ExportODCType";
		public static readonly string ACTION_EXPORT_ODC_SPREAD = "ExportODCSpread";
		public static readonly string ACTION_DELETE_BOE_OTHER_DIRECT_COST = "DeleteBOEOtherDirectCost";
		public static readonly string ACTION_DELETE_ALL_BOE_OTHER_DIRECT_COST = "DeleteAllODCElements";
		public static readonly string ACTION_SAVE_REORDER_BOE_OTHER_DIRECT_TASK_ELEMENTS = "SaveReorderODCTaskElements";


		//BOE Travel Controller
		public static readonly string ACTION_DISPLAY_BOE_TRAVEL_GRID = "DisplayBOETravelGrid";
		public static readonly string ACTION_DISPLAY_BOE_TRAVEL_COMPOSITE = "DisplayBOETravelComposite";
		public static readonly string ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS = "DisplayBOETravelElementDetails";
		public static readonly string ACTION_DISPLAY_BOE_TRAVEL_TRIPS = "DisplayBOETravelTrips";
		public static readonly string ACTION_DISPLAY_BOE_TRAVEL_TRIPS_GRID = "DisplayBOETravelTripsGrid";
		public static readonly string ACTION_SAVE_EDIT_TRAVEL_DETAILS_COMPOSITE = "SaveEditTravelDetailsComposite";
		public static readonly string ACTION_DELETE_BOE_TRAVEL = "DeleteBOETravel";
		public static readonly string ACTION_DELETE_ALL_BOE_TRAVEL = "DeleteAllBOETravel";
		public static readonly string ACTION_SAVE_REORDER_TRAVEL_TASK_ELEMENTS = "SaveReorderTravelTaskElements";

		//BOE Zone Travel Controller
		public static readonly string ACTION_DISPLAY_BOE_ZONE_TRAVEL_GRID = "DisplayBOEZoneTravelGrid";
		public static readonly string ACTION_DISPLAY_BOE_ZONE_TRAVEL_COMPOSITE = "DisplayBOEZoneTravelComposite";
		public static readonly string ACTION_DISPLAY_BOE_ZONE_TRAVEL_ELEMENT_DETAILS = "DisplayBOEZoneTravelElementDetails";
		public static readonly string ACTION_DISPLAY_BOE_ZONE_TRAVEL_TRIPS = "DisplayBOEZoneTravelTrips";
		public static readonly string ACTION_DISPLAY_BOE_ZONE_TRAVEL_TRIPS_GRID = "DisplayBOEZoneTravelTripsGrid";
		public static readonly string ACTION_SAVE_EDIT_ZONE_TRAVEL_DETAILS_COMPOSITE = "SaveEditZoneTravelDetailsComposite";
		public static readonly string ACTION_DELETE_BOE_ZONE_TRAVEL = "DeleteBOEZoneTravel";
		public static readonly string ACTION_DELETE_ALL_BOE_ZONE_TRAVEL = "DeleteAllBOEZoneTravel";
		public static readonly string ACTION_SAVE_REORDER_ZONE_TRAVEL_TASK_ELEMENTS = "SaveReorderZoneTravelTaskElements";
		public static readonly string ACTION_VERIFY_CALCULATE_ZONE_TRAVEL_TRIP = "VerifyAndCalculateZoneTravelTrip";


		// Material
		public static readonly string ACTION_DISPLAY_BOE_MATERIAL_GRID = "DisplayBOEMaterialGrid";
		public static readonly string ACTION_DISPLAY_BOE_MATERIAL_COMPOSITE = "DisplayBOEMaterialComposite";
		public static readonly string ACTION_DISPLAY_BOE_MATERIAL_ELEMENT_DETAILS = "DisplayBOEMaterialElementDetails";
		public static readonly string ACTION_GET_FILTERED_TRIP_DATA = "GetFilteredTripData";
		public static readonly string ACTION_GET_TRIP_ID = "GetTripID";
		public static readonly string ACTION_SAVE_EDIT_MATERIAL_DETAILS_COMPOSITE = "SaveEditMaterialDetailsComposite";

		// BOEConfiguration Controller
		public static readonly string GET_SYSTEM_CONFIGURATION = "GetSystemConfiguration";
		public static readonly string GET_WORKSPACE_CONFIGURATION = "GetWorkspaceConfiguration";
		public static readonly string GET_HOME_MASTER_MENU_ITEMS = "GetHomeMasterMenuItems";

		// BOE Controller
		public static readonly string GET_BOE_HEADER = "GetBOEHeader";

		#endregion BOE ACTIONS

		#region CLIN
		public static readonly string ACTION_SAVE_CLIN = "SaveClin";
		public static readonly string ACTION_DELETE_CLINS = "DeleteCLINs";
		public static readonly string ACTION_GET_BOE_COUNT_FOR_CLIN = "GetBOECountForClin";
		public static readonly string ACTION_IMPORT_CLINS = "ImportCLINs";
		public static readonly string ACTION_COMPLETE_IMPORT_CLINS = "CompleteImportCLINs";
		public static readonly string ACTION_EXPORT_CLINS = "ExportCLINs";
		public static readonly string ACTION_GET_MANAGE_CLIN_MODEL = "GetManageClinGridModel";
		#endregion CLIN

		#region HOME

		public static readonly string ACTION_CLEAR_CACHE = "ClearCache";
		public static readonly string ACTION_AD_SYNC_ADD = "ADSyncAddUsers";
		public static readonly string ACTION_AD_SYNC_REMOVE = "ADSyncRemoveUsers";
		public static readonly string ACTION_AD_SYNC_UPDATE = "ADSyncUpdateUsers";
		public static readonly string ACTION_CREATE_WORKSPACE = "CreateWorkspace";
		public static readonly string ACTION_VALIDATE_CREATE_WORKSPACE_STEP_ONE = "ValidateCreateWorkspaceStepOne";
		public static readonly string ACTION_DISPLAY_CHOOSE_WORKSPACE = "DisplayChooseWorkspace";
		public static readonly string ACTION_DISPLAY_CREATE_WORKSPACE_FORM = "DisplayCreateWorkspaceForm";
		public static readonly string ACTION_DISPLAY_CREATE_WORKSPACE_DIV = "DisplayCreateWorkspaceDiv";
		public static readonly string ACTION_SAVE_NEW_WORKSPACE = "SaveNewWorkspace";
		public static readonly string ACTION_DISPLAY_WORKSPACE_SEARCH = "DisplayWorkspaceSearch";
		public static readonly string ACTION_PAGE_WORKSPACE_SEARCH_RESULTS = "PageWorkspaceSearchResults";
		public static readonly string ACTION_PERFORM_WORKSPACE_SEARCH = "PerformWorkspaceSearch";
		public static readonly string ACTION_VALIDATE_WORKSPACE_SEARCH = "ValidateWorkspaceSearch";
		public static readonly string ACTION_GET_DETAILS_FOR_WORKSPACE_TO_COPY = "GetDetailsForWorkspaceToCopy";
		public static readonly string ACTION_GET_BOES_FOR_WORKSPACE_TO_COPY = "GetBOEsForWorkspaceToCopy";
		public static readonly string ACTION_HOME_DISPLAY_MASTER_MENU = "DisplayHomeMasterMenu";
		public static readonly string ACTION_HOME_DISPLAY_WHOSONLINE = "DisplayWhosOnline";
		public static readonly string ACTION_HOME_PAGE_WHOSONLINE = "PageGenBOEMetricsWhosOnline";
		public static readonly string ACTION_HOME_DISPLAY_METRICS_DETAILS = "GenBoeMetrics";
		public static readonly string ACTION_HOME_GET_USER_METRICS = "GetUserMetricsModel";
		public static readonly string ACTION_HOME_RESTORE_WORKSPACE = "RestoreWorkspace";
		public static readonly string ACTION_HOME_RESTORE_PTM_WORKSPACE = "RestorePtmWorkspace";
		public static readonly string ACTION_HOME_DELETE_WORKSPACES = "DeleteWorkspaces";
		public static readonly string ACTION_HOME_CHANGE_FAVORITE = "ChangeFavorite";
		public static readonly string ACTION_HOME_GET_TRACKING_NUMBERS = "GetTrackingNumbers";


		#endregion HOME

		#region Reports

		public static readonly string ACTION_DISPLAY_EXPORTS = "DisplayExports";
		public static readonly string ACTION_DISPLAY_GENERAL_REPORTS = "DisplayGeneralReports";
		public static readonly string ACTION_DISPLAY_SUMMARY_REPORTS = "DisplaySummaryReports";
		public static readonly string ACTION_DISPLAY_CUSTOMER_REPORTS = "DisplayCustomerReports";
		public static readonly string ACTION_DISPLAY_FINANCE_REPORTS = "DisplayFinanceReports";
		public static readonly string ACTION_DISPLAY_ADDITIONAL_REPORTS = "DisplayAdditionalReports";
		public static readonly string ACTION_DISPLAY_BOE_STATUS_REPORT = "DisplayBOEStatusReport";
		public static readonly string ACTION_DISPLAY_WORKSPACE_ACTIVITY_REPORT = "DisplayWorkspaceActivityReport";
		public static readonly string ACTION_DISPLAY_BOE_ACTIVITY_REPORT = "DisplayBOEActivityReport";
		public static readonly string ACTION_EXPORT = "Export";
		public static readonly string ACTION_EXPORT_INL_FORMS = "ExportBOEForm";
		public static readonly string ACTION_VALIDATE_INL_FORM_TM_RESOURCES = "ValidateBOEFormTMResources";
		public static readonly string ACTION_DISPLAY_EXPORT_TO_PROPRICER_INDEX = "ProPricerIndex";
		public static readonly string ACTION_DISPLAY_EXPORT_TO_PROPRICER = "DisplayProPricer";
		public static readonly string ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID = "DisplayProPricerGrid";
		public static readonly string ACTION_DELETE_PROPRICER_EXPORT_FORMATS = "DeleteProPricerExportFormats";
		public static readonly string ACTION_SAVE_PROPRICER_EXPORT_FORMAT = "SaveProPricerExportFormat";
		public static readonly string ACTION_SAVE_PROPRICER_EXPORT_CUSTOM_FIELD = "SaveProPricerCustomField";
		public static readonly string ACTION_EXPORT_PROPRICER_EXPORT_FORMAT = "ExportProPricerExportFormat";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_REPORT_SELECTOR = "DisplayBoeCustomReportSelector";
		public static readonly string ACTION_DISPLAY_INL_FORM_EXPORT_GRID = "DisplayInlFormExportGrid";
		public static readonly string ACTION_EXPORT_BOE_CUSTOM_REPORT = "ExportCustomReport";
		public static readonly string ACTION_DISPLAY_BOE_DISCREPANCY = "DisplayBoeDiscrepancyReport";
		public static readonly string ACTION_DISPLAY_VALIDATE_ALL_BOE = "DisplayValidateAllBOE";
		public static readonly string ACTION_EXPORT_BOE_DISCREPANCY = "ExportBoeDiscrepancyReport";
		public static readonly string ACTION_GENERATE_REPORT_NONCE = "GenerateReportNonce";
		public static readonly string ACTION_VALIDATE_BOES_FOR_DISCREPANCIES = "ValidateBoesForDiscrepancies";
		public static readonly string ACTION_BULK_DOWNLOAD_REPORT = "BulkDownloadReports";
		public static readonly string ACTION_UPDATE_WS_PROPRICER_LAST_PROPOSAL = "UpdateProPricerLastProposal";
		public static readonly string ACTION_UPDATE_WS_PROPRICER_LAST_INSTANCE = "UpdateProPricerLastInstance";
		public static readonly string ACTION_WS_PROPRICER_PREVIEW = "ProPricerPreview";
		public static readonly string ACTION_WS_PROPRICER_EXPORT = "ProPricerExport";
		public static readonly string ACTION_COPY_FORMAT_TO_SYSTEM_LEVEL = "CopyFormatToSystemLevel";

		#region ManageReportsController

		public static readonly string GET_BOE_STATUS_REPORT = "GetBOEStatusReport";
		public static readonly string GET_VALIDATE_ALL_BOES_REPORT = "GetValidateAllBOEsReport";
		public static readonly string GET_BOE_DISCREPANCY_REPORT_HOURS_LABEL = "GetBOEDiscrepancyReportHoursLabel";
		public static readonly string GET_BOE_DISCREPANCY_REPORT = "GetBOEDiscrepancyReport";
		public static readonly string GET_GENERAL_REPORT = "GetGeneralReport";
		public static readonly string GET_EXPORTS = "GetExports";
		public static readonly string GET_CONFIDENCE_REPORT = "GetConfidenceReport";
		public static readonly string GET_WORKSPACE_STATUS = "GetWorkspaceStatus";

		#endregion ManageReportsController

		#endregion Reports

		#region SHARED

		public static readonly string ACTION_SHARED_DISPLAY_MASTER_MENU = "DisplaySiteMasterMenu";

		#endregion SHARED

		#region WBS

		public static readonly string ACTION_COMPLETE_IMPORT_WBS = "CompleteImportWBS";
		public static readonly string ACTION_CREATE_BOES = "CreateBOEs";
		public static readonly string ACTION_DISPLAY_MANAGE_WBS = "DisplayManageWBS";
		public static readonly string ACTION_DISPLAY_MANAGE_WBS_GRID = "DisplayManageWBSGrid";
		public static readonly string ACTION_DISPLAY_IMPORT_WBS = "DisplayImportWBSButton";
		public static readonly string ACTION_EXPORT_WBS = "ExportWBS";
		public static readonly string ACTION_EXPORT_WBS_TEMPLATE = "ExportWBSTemplate";
		public static readonly string ACTION_GET_BOE_COUNT_FOR_WBS = "GetBOECountForWBS";
		public static readonly string ACTION_IMPORT_WBS = "ImportWBS";
		public static readonly string ACTION_PAGE_MANAGE_WBS = "PageManageWBS";
		public static readonly string ACTION_SAVE_MANAGE_WBS_UPDATES = "SaveManageWBSUpdates";
		public static readonly string ACTION_UPLOAD_WBS_FILE = "UploadWBSFile";

		#endregion

		#region FindReplace

		public static readonly string ACTION_FIND_REPLACE = "FindReplace";
		public static readonly string ACTION_DISPLAY_FIND_REPLACE = "DisplayFindReplace";
		public static readonly string ACTION_FIND_REPLACE_RESULTS = "FindAllforReplace";
		public static readonly string ACTION_SAVE_REPLACED_VALUES = "SaveReplacedValues";
		public static readonly string ACTION_PAGE_FIND_REPLACE_RESULTS = "PageFindResults";


		#endregion FindReplace

		#region Workspace

		#region Display

		public static readonly string ACTION_DISPLAY_ADD_WORKSPACE_RESOURCE_RATE_TM = "DisplayAddWorkspaceResourceRateTM";
		public static readonly string ACTION_DISPLAY_ADD_WORKSPACE_RESOURCE_RATE = "DisplayAddWorkspaceResourceRate";
		public static readonly string ACTION_DISPLAY_BACKUP_VERSIONS = "DisplayBackupVersions";
		public static readonly string ACTION_DISPLAY_BACKUP_VERSIONS_GRID = "DisplayBackupVersionsGrid";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD = "DisplayBOECustomField";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_GRID = "DisplayBOECustomFieldsGrid";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE = "DisplayBOECustomFieldResource";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_GRID = "DisplayBOECustomFieldResourceGrid";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT = "DisplayBOECustomFieldResourceViewDefault";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG = "DisplayBOECustomFieldPerfOrg";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID = "DisplayBOECustomFieldPerfOrgGrid";
		public static readonly string ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT = "DisplayBOECustomFieldPerfOrgViewDefault";
		public static readonly string ACTION_DISPLAY_IMS_IMPORT_PAGE = "DisplayIMSImportPage";
		public static readonly string ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN = "DisplayVariableBOESumByCLIN";
		public static readonly string ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS = "DisplayVariableBOESumByWBS";
		public static readonly string ACTION_DISPLAY_WORKSPACE_ALLOW_SEARCH = "DisplayWorkspaceAllowSearch";
		public static readonly string ACTION_DISPLAY_WORKSPACE_HOME_HELP = "DisplayWorkspaceHomeHelp";
		public static readonly string ACTION_DISPLAY_WORKSPACE_IDENTIFICATION = "DisplayWorkspaceIdentification";
		public static readonly string ACTION_DISPLAY_WORKSPACE_OUTPUT_FORMAT = "DisplayWorkspaceOutputFormat";
		public static readonly string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES = "DisplayWorkspaceResourceRates";
		public static readonly string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_GRID = "DisplayWorkspaceResourceRatesGrid";
		public static readonly string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_TM = "DisplayWorkspaceResourceRatesTM";
		public static readonly string ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_GRID_TM = "DisplayWorkspaceResourceRatesGridTM";
		public static readonly string ACTION_DISPLAY_WORKSPACE_SETTINGS_JUMP = "DisplayWorkspaceSettingsJump";
		public static readonly string ACTION_DISPLAY_WORKSPACE_SHOW_GETTING_STARTED_HELP = "DisplayWorkspaceShowGettingStartedHelp";
		public static readonly string ACTION_DISPLAY_WORKSPACE_STATUS = "DisplayWorkspaceStatus";
		public static readonly string ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID = "DisplayWorkspaceStatusHistoryGrid";
		public static readonly string ACTION_DISPLAY_WORKSPACE_SUM_OF_BOE_VARIABLES = "DisplayWorkspaceSumofBOEVariables";
		public static readonly string ACTION_DISPLAY_WORKSPACE_DISCRETE_VARIABLES = "DisplayWorkspaceDiscreteVariables";
		public static readonly string ACTION_DISPLAY_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES = "WorkspaceResourceRatesImportPreview";
		public static readonly string ACTION_DISPLAY_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES_TM = "WorkspaceResourceRatesImportPreviewTM";
		public static readonly string ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG = "DisplayUpdateWorkspaceRatesDialog";
		public static readonly string ACTION_DISPLAY_WORKSPACE_EMAIL_PREFERENCES = "DisplayEmailPreferences";

		#endregion Display

		public static readonly string ACTION_CREATE_SIKORSKY_CUSTOM_FIELDS = "CreateSikorskyCustomFields";
		public static readonly string ACTION_CREATE_PROPRICER_CUSTOM_FIELDS = "CreateProPricerCustomFields";
		public static readonly string ACTION_DELETE_ALL_WORKSPACE_VARIABLES = "DeleteAllWorkspaceVariables";
		public static readonly string ACTION_DELETE_CUSTOM_FIELD = "DeleteCustomField";
		public static readonly string ACTION_DELETE_WORKSPACE_VERSIONS = "DeleteWorkspaceVersions";
		public static readonly string ACTION_EXPORT_BOE_CUSTOM_FIELD = "ExportBOECustomField";
		public static readonly string ACTION_EXPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG = "ExportBOECustomFieldPerfOrg";
		public static readonly string ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE = "ExportBOECustomFieldResource";
		public static readonly string ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE_TEMPLATE = "ExportBOECustomFieldResourceTemplate";
		public static readonly string ACTION_EXPORT_PROJECT_MAP = "ExportProjectMapData";
		public static readonly string ACTION_EXPORT_WORKSPACE_COMMENTS_AND_RESPONSES = "ExportWorkspaceCommentsAndResponses";
		public static readonly string ACTION_FIND_VALID_BOES_FOR_WORKSPACE_VARIABLE = "FindValidBOEsForWorkspaceVariable";
		public static readonly string ACTION_FIND_VALID_BOES_FOR_NOT_IN_USE_WORKSPACE_VARIABLE = "FindValidBOEsForNotInUseWorkspaceVariable";
		public static readonly string ACTION_IMPORT_ARTEMIS_IMS = "ImportIMSArtemis";
		public static readonly string ACTION_IMPORT_BOE_CUSTOM_FIELD = "ImportBOECustomField";
		public static readonly string ACTION_IMPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG = "ImportBOECustomFieldPerfOrg";
		public static readonly string ACTION_IMPORT_BOE_CUSTOM_FIELD_RESOURCE = "ImportBOECustomFieldResource";
		public static readonly string ACTION_IMPORT_PROJECT_IMS = "ImportIMSProject";
		public static readonly string ACTION_PAGE_CUSTOM_FIELD_PERFORMING_ORGS = "PageCustomFieldPerformingOrgs";
		public static readonly string ACTION_PAGE_CUSTOM_FIELD_RESOURCES = "PageCustomFieldResources";
		public static readonly string ACTION_REFRESH_VARIABLE_BOE_SUM_BY_CLIN = "RefreshVariableBOESumByCLIN";
		public static readonly string ACTION_REFRESH_VARIABLE_BOE_SUM_BY_WBS = "RefreshVariableBOESumByWBS";
		public static readonly string ACTION_RESTORE_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS = "RestoreCustomFieldPerformingOrganizations";
		public static readonly string ACTION_RESTORE_CUSTOM_FIELD_RESOURCES = "RestoreCustomFieldResources";
		public static readonly string ACTION_RESTORE_WORKSPACE_VERSION = "RestoreWorkspaceVersion";
		public static readonly string ACTION_EXPORT_WORKSPACE_VERSION = "ExportWorkspaceVersion";
		public static readonly string ACTION_SAVE_CUSTOM_FIELDS = "SaveCustomFields";
		public static readonly string ACTION_SAVE_CUSTOM_FIELD_PERFORMING_ORGS = "SaveCustomFieldPerformingOrgs";
		public static readonly string ACTION_SAVE_CUSTOM_FIELD_RESOURCES = "SaveCustomFieldResources";
		public static readonly string ACTION_SAVE_HIDE_GETTING_STARTED_HELP_MENU = "SaveHideGettingStartedHelpMenu";
		public static readonly string ACTION_SAVE_WORKSPACE_ALLOW_SEARCH = "SaveWorkspaceAllowSearch";
		public static readonly string ACTION_SAVE_WORKSPACE_IDENTIFICATION = "SaveWorkspaceIdentification";
		public static readonly string ACTION_UPDATE_CURRENT_WORKSPACE_IDENTIFICATION = "UpdateCurrentWorkspaceIdentification";
		public static readonly string ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT = "SaveWorkspaceOutputFormat";
		public static readonly string ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT_NEW_TEMPLATE = "SaveNewOutputFormatTemplate";
		public static readonly string ACTION_ARCHIVE_OUTPUT_FORMAT_TEMPLATE = "ArchiveOutputFormatTemplate";
		public static readonly string ACTION_RESTORE_OUTPUT_FORMAT_TEMPLATE = "RestoreOutputFormatTemplate";
		public static readonly string ACTION_SAVE_WORKSPACE_RESOURCE_RATE = "SaveWorkspaceResourceRate";
		public static readonly string ACTION_SAVE_WORKSPACE_RESOURCE_RATE_TM = "SaveWorkspaceResourceRateTM";
		public static readonly string ACTION_SAVE_WORKSPACE_STATUS = "SaveWorkspaceStatus";
		public static readonly string ACTION_SAVE_WORKSPACE_VARIABLES = "SaveWorkspaceVariables";
		public static readonly string ACTION_SAVE_WORKSPACE_VERSION = "SaveWorkspaceVersion";
		public static readonly string ACTION_SEARCH_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS = "SearchCustomFieldPerformingOrganizations";
		public static readonly string ACTION_SEARCH_CUSTOM_FIELD_RESOURCES = "SearchCustomFieldResources";
		public static readonly string ACTION_STATE_TRANSITION_VALIDATION = "WorkspaceStatusChangeValidation";
		public static readonly string ACTION_WORKSPACE_HOME = "SetUpWorkspaceHome";
		public static readonly string ACTION_WORKSPACE_OUTPUT_REQUEST_NEW_TEMPLATE = "WorkspaceOutputRequestNewTemplate";
		public static readonly string ACTION_WORKSPACE_SETTINGS = "WorkspaceSettings";
		public static readonly string ACTION_IS_WORKSPACE_NAME_AVAILABLE = "IsWorkspaceNameAvailable";
		public static readonly string ACTION_IS_WORKSPACE_SHORT_NAME_AVAILABLE = "IsWorkspaceShortNameAvailable";
		public static readonly string ACTION_GET_EXACT_COPY_DATA = "GetExactCopyData";
		public static readonly string ACTION_GET_NEXT_TRACKING_NUMBER_REVISION = "GetNextTrackingNumberRevision";
		public static readonly string ACTION_UPDATE_ZONE_TRAVEL_RATES = "UpdateZoneTravelRates";
		public static readonly string ACTION_UPDATE_OFFLOAD_RATES = "UpdateOffloadRates";
		public static readonly string ACTION_UPDATE_UCOT_FACTOR = "UpdateUCOTFactor";
		public static readonly string ACTION_IMPORT_WORKSPACE_RESOURCE_RATES = "ImportWorkspaceResourceRates";
		public static readonly string ACTION_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES = "ImportPreviewResultsForWorkspaceResourceRates";
		public static readonly string ACTION_IMPORT_WORKSPACE_RESOURCE_RATES_TM = "ImportWorkspaceResourceRatesTM";
		public static readonly string ACTION_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES_TM = "ImportPreviewResultsForWorkspaceResourceRatesTM";
		public static readonly string ACTION_EXPORT_WORKSPACE_RESOURCE_RATES = "ExportWorkspaceResourceRates";
		public static readonly string ACTION_EXPORT_BLANK_WORKSPACE_RESOURCE_RATES = "ExportBlankWorkspaceResourceRates";
		public static readonly string ACTION_EXPORT_WORKSPACE_RESOURCE_RATES_TM = "ExportWorkspaceResourceRatesTM";
		public static readonly string ACTION_EXPORT_BLANK_WORKSPACE_RESOURCE_RATES_TM = "ExportBlankWorkspaceResourceRatesTM";
		public static readonly string ACTION_GET_WORKSPACE_HOME_MODEL = "GetWorkspaceHomeModel";
		public static readonly string ACTION_SAVE_WORKSPACE_EMAIL_PREFERENCES = "SaveWorkspaceEmailPreferences";
		public static readonly string ACTION_GET_WORKSPACE_EMAIL_PREFERENCES = "GetWorkspaceEmailPreferences";
		public static readonly string ACTION_GET_BOE_BULK_SUBMIT_MODEL = "GetBOEBulkSubmitModel";
		public static readonly string ACTION_BOE_BULK_SUBMIT = "BOEBulkSubmit";
		public static readonly string ACTION_GET_MANAGE_WBS_MODEL = "GetManageWBSGridModel";
		public static readonly string ACTION_GET_MANAGE_BOE_MODEL = "GetManageBOEGridModel";
		public static readonly string ACTION_FIND_ADJACENT_BOES = "FindAdjacentBoes";
		public static readonly string ACTION_GET_WORKSPACE_RECALCULATE_ACTUALS_MODEL = "GetWorkspaceRecalculateActualsModel";
		public static readonly string ACTION_SEARCH_PLD_PROPOSALS = "SearchPLDProposals";
		public static readonly string ACTION_GET_PLD_PROPOSAL_DETAILS = "GetPLDProposalDetails";
		public static readonly string ACTION_GET_NEXT_PLD_WORKSPACE_SHORTNAME_FROM_TRACKING_NUMBER = "GetNextPLDWorkspaceShortNameFromTrackingNumber";




		#endregion Workspace

		#region RTE Custom Templates

		public static readonly string ACTION_DISPLAY_MANAGE_RTE_TEMPLATES = "Index";
		public static readonly string VIEW_MANAGE_RTE_TEMPLATES = "ManageRTETemplates";
		public static readonly string ACTION_GET_RTE_TEMPLATES = "GetRTETemplatesModel";
		public static readonly string ACTION_SAVE_RTE_TEMPLATES = "SaveRTETemplatesModel";
		public static readonly string ACTION_SEARCH_RTE_TEMPLATES = "SearchTemplates";
		public static readonly string ACTION_COPY_RTE_TEMPLATE = "CopyTemplate";

		#endregion RTE Custom Templates

		#region DateShift

		public static readonly string ACTION_APPLY_DATE_SHIFT = "ApplyDateShift";

		#endregion DateShift

		#endregion ACTION

		#region UI EVENTS

		public static readonly string EVENT_BOESUMMARYGRID_TRAVEL_HOURS_UPDATED = "BOESUMMARYGRID_TRAVEL_HOURS_UPDATED";
		public static readonly string EVENT_BOESUMMARYGRID_HOURS_UPDATED = "BOESUMMARYGRID_HOURS_UPDATED";
		public static readonly string EVENT_BOESUMMARYGRID_RELOAD = "BOESUMMARYGRID_RELOAD";
		public static readonly string EVENT_TASK_COST_TOTAL_UPDATED = "TASK_COST_TOTAL_UPDATED";
		public static readonly string EVENT_WIDGET_DATA_CLEAN = "DATA_CLEANED";
		public static readonly string EVENT_IMPORTWBS_ENABLE_IMPORT_WBS_BUTTON = "IMPORTWBS_ENABLE_IMPORT_WBS_BUTTON";
		public static readonly string EVENT_BOEDETAILS_RELOAD_BOE_COMMENT_GRID = "BOEDETAILS_RELOAD_BOE_COMMENT_GRID";
		public static readonly string EVENT_BOEDETAILS_RELOAD_BOE_HISTORY_GRID = "BOEDETAILS_RELOAD_BOE_HISTORY_GRID";
		public static readonly string EVENT_BOEDETAILS_RELOAD_BOE_OTHER_DIRECT_COST_GRID = "BOEDETAILS_RELOAD_BOE_OTHER_DIRECT_COST_GRID";
		public static readonly string EVENT_REPORTS_VIEW_REPORT = "REPORTS_VIEW_REPORT";
		public static readonly string EVENT_METRICS_VIEW = "EVENT_METRICS_VIEW";

		public static readonly string EVENT_DISPLAY_TASK_ELEMENT_DETAILS = "DISPLAY_TASK_ELEMENT_DETAILS";
		public static readonly string EVENT_DISPLAY_ODC_ELEMENT_DETAILS = "DISPLAY_ODC_ELEMENT_DETAILS";
		public static readonly string EVENT_DISPLAY_TRAVEL_ELEMENT_DETAILS = "DISPLAY_TRAVEL_ELEMENT_DETAILS";
		public static readonly string EVENT_DISPLAY_ZONE_TRAVEL_ELEMENT_DETAILS = "DISPLAY_ZONE_TRAVEL_ELEMENT_DETAILS";
		public static readonly string EVENT_DISPLAY_MATERIAL_ELEMENT_DETAILS = "DISPLAY_MATERIAL_ELEMENT_DETAILS";

		#endregion UI EVENTS

		#region VIEWS

		public static readonly string VIEW_INDEX = "Index";

		#region ADMIN

		public static readonly string VIEW_MANAGE_OUTPUT_TEMPLATES = "ManageOutputFormatTemplates";
		public static readonly string VIEW_MANAGE_OUTPUT_TEMPLATES_GRID = "ManageOutputFormatTemplatesGrid";
		public static readonly string VIEW_MANAGE_DEFAULT_PERF_ORGS = "ManageDefaultPerfOrgs";
		public static readonly string VIEW_MANAGE_DEFAULT_PERF_ORGS_GRID = "ManageDefaultPerfOrgsGrid";
		public static readonly string VIEW_MANAGE_DEFAULT_RESOURCES = "ManageDefaultResources";
		public static readonly string VIEW_MANAGE_DEFAULT_RESOURCES_GRID = "ManageDefaultResourcesGrid";
		public static readonly string VIEW_MANAGE_BOE_FORMS = "ManageBOEForms";
		public static readonly string VIEW_MANAGE_BOE_FORMS_GRID = "ManageBOEFormsGrid";
		public static readonly string VIEW_MANAGE_TRIPS = "ManageTripsForTravel";
		public static readonly string VIEW_MANAGE_TRIPS_GRID = "ManageTripsForTravelGrid";
		public static readonly string VIEW_MANAGE_TRIPS_IMPORT_VERIFICATION = "ManageTripsForTravelImportVerification";
		public static readonly string VIEW_MANAGE_ZONE_TRAVEL = "ManageZoneTravel";
		public static readonly string VIEW_MANAGE_ZONE_TRAVEL_GRID = "ManageZoneTravelGrid";
		public static readonly string VIEW_MANAGE_ZONE_TRAVEL_DESTINATIONS = "ManageZoneTravelDestinations";
		public static readonly string VIEW_MANAGE_ZONE_TRAVEL_DESTINATIONS_GRID = "ManageZoneTravelDestinationsGrid";
		public static readonly string VIEW_MANAGE_NONZONE_FEES_AND_COSTS = "ManageNonzoneFeesAndCosts";
		public static readonly string VIEW_MANAGE_NONZONE_FEES_AND_COSTS_GRID = "ManageNonzoneFeesAndCostsGrid";
		public static readonly string VIEW_MANAGE_OFFLOAD_RATES = "ManageOffloadRates";
		public static readonly string VIEW_PAGE_SYSTEM_OFFLOAD_RATES = "ManageOffloadRatesPagedData";
		public static readonly string VIEW_MANAGE_LEGACY_RESOURCES = "ManageLegacyResources";
		public static readonly string VIEW_MANAGE_MISC_RATES = "ManageMiscRates";
		public static readonly string VIEW_MANAGE_MISC_RATES_GRID = "ManageMiscRatesGrid";
		public static readonly string VIEW_MANAGE_ESCALATION_RATES = "ManageEscalationRates";
		public static readonly string VIEW_MANAGE_ESCALATION_RATES_GRID = "ManageEscalationRatesGrid";
		public static readonly string VIEW_MANAGE_MILEAGE_REIMBURSEMENT = "ManageMileageReimbursement";
		public static readonly string VIEW_MANAGE_EMAIL_PREFERENCES = "ManageEmailPreferences";
		public static readonly string VIEW_MANAGE_SYSTEM_SETTINGS = "ManageSystemSettings";
		public static readonly string VIEW_SYSTEM_PERMISSIONS_GRID = "ManageSystemPermissionsGrid";
		public static readonly string VIEW_CREATE_WORKSPACE_PERMISSIONS_GRID = "ManageCreateWorkspacePermissionsGrid";
		public static readonly string VIEW_SYSTEM_ADMIN_JUMP = "SystemAdminJump";
		public static readonly string VIEW_SYSTEM_ADMIN = "SystemAdmin";
		public static readonly string VIEW_SYSTEM_ADMIN_INFO = "MyInfo";
		public static readonly string VIEW_SYSTEM_RESOURCE_RATES = "SystemResourceRates";
		public static readonly string VIEW_PAGE_SYSTEM_RESOURCE_RATES = "SystemResourceRatesPagedData";
		public static readonly string VIEW_MANAGE_OVERDUE_TRAINING = "ManageOverdueTraining";
		public static readonly string VIEW_MANAGE_SYSTEM_PROPRICER_EXPORTS = "ManageSystemProPricerExports";
		public static readonly string VIEW_SYSTEM_EXPORT_TO_PROPRICER_GRID = "ManageSystemProPricerGrid";
		public static readonly string VIEW_MANAGE_UCOT = "ManageUCOT";

		#endregion ADMIN

		#region BOE

		public static readonly string VIEW_BOE_ADVANCED_SEARCH = "BOEAdvancedSearch";
		public static readonly string VIEW_BOE_PROJECTMAP_ADVANCED_SEARCH = "BOEProjectMapAdvancedSearch";
		public static readonly string VIEW_BOE_BOE_EXPORT = "BOEExport";
		public static readonly string VIEW_BOE_BOE_SUBMIT_FOR_APPROVAL = "BOESubmitForApproval";
		public static readonly string VIEW_BOE_BOE_SUBMIT_FOR_REVIEW = "BOESubmitForReview";
		public static readonly string VIEW_BOE_BOE_VALIDATE = "BOEValidate";
		public static readonly string VIEW_BOE_BOE_OFFLOAD = "BOEOffload";
		public static readonly string VIEW_BOE_DETAILS = "BOEDetails";
		public static readonly string VIEW_BOE_HEADER_DESCRIPTION = "BOEHeaderDescription";
		public static readonly string VIEW_BOE_HEADER_SPACE_TITLE = "BOEHeaderSpaceTitle";
		public static readonly string VIEW_BOE_HEADER_ISGS_TITLE = "BOEHeaderISGSTitle";
		public static readonly string VIEW_BOE_HEADER = "BOEHeader";
		public static readonly string VIEW_BOE_QUICK_SEARCH = "BOEQuickSearch";
		public static readonly string VIEW_BOE_SEARCH = "BOESearch";
		public static readonly string VIEW_BOE_SEARCH_RESULTS = "BOESearchResults";
		public static readonly string VIEW_BOE_PROJECTMAP_SEARCH = "BOEProjectMapSearch";
		public static readonly string VIEW_BOE_PROJECTMAP_SEARCH_RESULTS = "BOEProjectMapSearchResults";
		public static readonly string VIEW_BOE_SUMMARY = "BOESummary";
		public static readonly string VIEW_BOE_VALIDATION_RESULTS = "BOEValidateResults";
		public static readonly string VIEW_BOE_OFFLOAD_RESULTS = "BOEOffloadResults";
		public static readonly string VIEW_EDIT_BOE_INDEX = "EditBOEIndex";
		public static readonly string VIEW_MANAGE_BOE_IMPORT_VERIFICATION = "ManageBOEImportVerification";
		public static readonly string VIEW_TASK_ELEMENT_GRID = "TaskElementGrid";
		public static readonly string VIEW_SUMMARY_TASK_ELEMENT_GRID = "SummaryTaskElementGrid";
		public static readonly string VIEW_BOE_COPY_CONFLICTS = "BOECopyConflicts";
		public static readonly string VIEW_BOE_CONFIDENCE_REPORT_BUTTON = "BOEConfidenceReportButton";
		public static readonly string VIEW_BOE_CONFIDENCE_REPORT = "BOEConfidenceReport";

		public static readonly string VIEW_LABOR_TASK = "LaborTask";
		public static readonly string VIEW_LABOR_TASK_STATIC = "LaborTaskStatic";
		public static readonly string VIEW_ODC_ELEMENT_COMPOSITE = "BOEOtherDirectCostComposite";
		public static readonly string VIEW_TRAVEL_ELEMENT_COMPOSITE = "BOETravelComposite";
		public static readonly string VIEW_ZONE_TRAVEL_ELEMENT_COMPOSITE = "BOEZoneTravelComposite";

		//these 3 views make up the composite view
		public static readonly string VIEW_MOQ_EQUATION_FIELD = "MOQEquationField";
		public static readonly string VIEW_MOQ_TABLE_IMPORT_VERIFICATION = "MoqTableImportVerification";

		public static readonly string VIEW_BOE_COMMENTS_GRID = "BOECommentsGrid";

		public static readonly string VIEW_BOE_HISTORY_GRID = "BOEHistoryGrid";

		public static readonly string VIEW_BOE_FORM = "BOEForm";

		public static readonly string VIEW_BOE_OTHER_DIRECT_COST_COMPOSITE = "BOEOtherDirectCostComposite";
		public static readonly string VIEW_BOE_OTHER_DIRECT_COST_GRID = "BOEOtherDirectCostGrid";
		public static readonly string VIEW_ODC_ELEMENT_DETAILS = "ODCElementDetails";
		public static readonly string VIEW_ODC_TYPES = "ODCTypes";
		public static readonly string VIEW_ODC_TYPES_GRID = "ODCTypesGrid";
		public static readonly string VIEW_ODC_SPREAD_GRID = "ODCSpreadGrid";
		public static readonly string VIEW_ODC_SPREAD = "ODCSpread";

		public static readonly string VIEW_BOE_TRAVEL_GRID = "BOETravelGrid";
		public static readonly string VIEW_TRAVEL_ELEMENT_DETAILS = "TravelElementDetails";
		public static readonly string VIEW_TRAVEL_TRIPS = "TravelTrips";
		public static readonly string VIEW_TRAVEL_TRIPS_GRID = "TravelTripsGrid";

		public static readonly string VIEW_BOE_ZONE_TRAVEL_GRID = "BOEZoneTravelGrid";
		public static readonly string VIEW_ZONE_TRAVEL_ELEMENT_DETAILS = "ZoneTravelElementDetails";
		public static readonly string VIEW_ZONE_TRAVEL_TRIPS = "ZoneTravelTrips";
		public static readonly string VIEW_ZONE_TRAVEL_TRIPS_GRID = "ZoneTravelTripsGrid";

		#region BOEMaterial
		public static readonly string VIEW_MATERIAL_COMPOSITE = "BOEMaterialComposite";
		public static readonly string VIEW_BOE_MATERIAL_GRID = "BOEMaterialGrid";
		public static readonly string VIEW_MATERIAL_ELEMENT_DETAILS = "BOEMaterialElementDetails";
		public static readonly string VIEW_MATERIAL_TYPES = "BOEMaterialTypes";

		public static readonly string ACTION_DELETE_BOE_MATERIAL = "DeleteBOEMaterial";
		public static readonly string ACTION_DELETE_ALL_MATERIAL_TYPES = "DeleteAllMaterials";
		#endregion BOEMaterial

		#endregion BOE

		#region HOME

		public static readonly string VIEW_HOME_GENBOE_INDEX = "GenBOEIndex";
		public static readonly string VIEW_HOME_GENBOE_INDEX_SUBCONTRACTOR = "GenBOEIndexSubcontractor";
		public static readonly string VIEW_HOME_CHOOSE_WORKSPACE = "ChooseWorkspace";
		public static readonly string VIEW_HOME_CREATE_WORKSPACE = "CreateWorkspace";
		public static readonly string VIEW_HOME_WHOSONLINE_DIV = "GenBOEMetricsWhosOnlineDiv";
		public static readonly string VIEW_HOME_CREATE_WORKSPACE_SPACE_FORM = "CreateWorkspaceSpaceForm";
		public static readonly string VIEW_HOME_CREATE_WORKSPACE_MST_FORM = "CreateWorkspaceMSTForm";
		public static readonly string VIEW_HOME_CREATE_WORKSPACE_DIV = "CreateWorkspaceDiv";
		public static readonly string VIEW_HOME_WORKSPACE_SEARCH = "WorkspaceSearch";
		public static readonly string VIEW_HOME_WORKSPACE_SEARCH_RESULTS = "WorkspaceSearchResults";
		public static readonly string VIEW_HOME_WORKSPACE_SEARCH_BOE_LIST = "WorkspaceSearchBOEList";
		public static readonly string VIEW_HOME_MASTER_MENU = "HomeMasterMenu";
		public static readonly string VIEW_HOME_WHOS_ONLINE = "GenBOEMetricsWhosOnline";
		public static readonly string VIEW_HOME_WHOS_ONLINE_INDEX = "GenBOEMetricsWhosOnlineIndex";

		#endregion HOME

		#region PERMISSIONS

		public static readonly string VIEW_MANAGE_PERMISSIONS = "ManagePermissions";
		public static readonly string VIEW_MANAGE_PERMISSIONS_GRID = "ManagePermissionsGrid";
		public static readonly string ACTION_CHECK_IF_USER_WILL_LOSE_ADMIN_ACCESS = "CheckIfUserWillLoseTheirAdminAccess";
		public static readonly string ACTION_SAVE_PERMISSIONS = "SaveNewPermissions";
		public static readonly string ACTION_EDIT_PERMISSIONS = "EditPermissions";
		public static readonly string ACTION_DELETE_USER_PERMISSIONS = "DeleteUserPermissions";
		public static readonly string ACTION_DELETE_GROUP_PERMISSIONS = "DeleteGroupPermissions";
		public static readonly string ACTION_GET_WORKSPACE_PERMISSION_MODEL = "GetManagePermissionsModel";
		public static readonly string ACTION_IMPORT_PERMISSIONS = "ImportPermissions";
		public static readonly string ACTION_EXPORT_PERMISSIONS = "ExportPermissions";

		#endregion PERMISSIONS

		#region REPORTS

		public static readonly string VIEW_EXPORTS = "Exports";
		public static readonly string VIEW_GENERAL_REPORTS = "GeneralReports";
		public static readonly string VIEW_SUMMARY_REPORTS = "SummaryReports";
		public static readonly string VIEW_CUSTOMER_REPORTS = "CustomerReports";
		public static readonly string VIEW_FINANCE_REPORTS = "FinanceReports";
		public static readonly string VIEW_ADDITIONAL_REPORTS = "AdditionalReports";
		public static readonly string VIEW_BOE_STATUS_REPORT = "BOEStatusReport";
		public static readonly string VIEW_WORKSPACE_ACTIVITY_REPORT = "WorkspaceActivityReport";
		public static readonly string VIEW_BOE_ACTIVITY_REPORT = "BOEActivityReport";
		public static readonly string VIEW_EXPORT_TO_PROPRICER_INDEX = "ProPricerIndex";
		public static readonly string VIEW_EXPORT_TO_PROPRICER = "ProPricer";
		public static readonly string VIEW_EXPORT_TO_PROPRICER_GRID = "ProPricerGrid";
		public static readonly string VIEW_BOE_CUSTOM_REPORT_SELECTOR = "CustomReportSelector";
		public static readonly string VIEW_BOE_CUSTOM_REPORT_SELECTOR_FORM = "CustomReportSelectorForm";
		public static readonly string VIEW_INL_FORM_EXPORT_SELECTOR_GRID = "INLFormExportSelectorGrid";
		public static readonly string VIEW_BOE_DISCREPANCY_REPORT = "BoeDiscrepancyReport";
		public static readonly string VIEW_VALIDATE_ALL_BOE_REPORT = "ValidateAllBOEReport";


		#endregion REPORTS

		#region SHARED

		public static readonly string VIEW_RTE_TEMPLATE = "RteTemplate";
		public static readonly string VIEW_NOT_FOUND_CONTROL = "NotFoundControl";
		public static readonly string VIEW_INVALID_PARAMETERS = "InvalidParameters";
		public static readonly string VIEW_ERROR = "Error";
		public static readonly string VIEW_SECURITY_ERROR = "SecurityError";
		public static readonly string VIEW_SITE_MASTER_MENU = "SiteMasterMenu";

		#endregion SHARED

		#region WBS

		public static readonly string VIEW_MANAGE_WBS_GRID = "ManageWBSGrid";
		public static readonly string VIEW_MANAGE_WBSS = "ManageWBSs";
		public static readonly string VIEW_WBS_IMPORT_VERIFICATION = "WbsImportVerification";

		#endregion WBS

		#region FindReplace

		public static readonly string VIEW_FIND_REPLACE = "FindReplace";
		public static readonly string VIEW_FIND_REPLACE_FORM = "FindReplaceForm";
		public static readonly string VIEW_FIND_REPLACE_RESULTS = "FindReplaceResults";

		#endregion FindReplace

		#region BOEBulkSubmit

		public static readonly string VIEW_BOE_BULK_SUBMIT = "BOEBulkSubmit";

		#endregion BOEBulkSubmit

		#region Workspace

		public static readonly string VIEW_ADD_WORKSPACE_RESOURCE_RATE = "AddWorkspaceResourceRate";
		public static readonly string VIEW_ADD_WORKSPACE_RESOURCE_RATE_TM = "AddWorkspaceResourceRateTM";
		public static readonly string VIEW_BACKUP_VERSIONS = "ManageBackupVersions";
		public static readonly string VIEW_BACKUP_VERSIONS_GRID = "ManageBackupVersionsGrid";
		public static readonly string VIEW_BOE_CUSTOM_FIELDS_GRID = "BOECustomFieldsGrid";
		public static readonly string VIEW_BOE_CUSTOM_FIELD = "BOECustomField";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_RESOURCE = "BOECustomFieldResource";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_RESOURCE_GRID = "BOECustomFieldResourceGrid";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_RESOURCE_RESTORE = "BOECustomFieldResourceRestore";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT = "BOECustomFieldResourceViewDefault";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG = "BOECustomFieldPerformingOrg";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID = "BOECustomFieldPerformingOrgGrid";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_RESTORE = "BOECustomFieldPerformingOrgRestore";
		public static readonly string VIEW_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT = "BOECustomFieldPerformingOrgViewDefault";
		public static readonly string VIEW_FOR_IMS_IMPORT_PAGE = "IMSImportPage";
		public static readonly string VIEW_VARIABLE_BOE_SUM_BY_WBS = "VariableBOESumByWBS";
		public static readonly string VIEW_VARIABLE_BOE_SUM_BY_CLIN = "VariableBOESumByCLIN";
		public static readonly string VIEW_WORKSPACE_HOME = "WorkspaceHome";
		public static readonly string VIEW_PROJ_MAP_WORKSPACE_HOME = "ProjectMapIndex";
		public static readonly string VIEW_WORKSPACE_HOME_HELP = "WorkspaceHomeHelp";
		public static readonly string VIEW_WORKSPACE_SETTINGS = "WorkspaceSettings";
		public static readonly string VIEW_WORKSPACE_IDENTIFICATION_SPACE = "WorkspaceIdentificationSpace";
		public static readonly string VIEW_WORKSPACE_IDENTIFICATION_MST = "WorkspaceIdentificationMST";
		public static readonly string VIEW_WORKSPACE_SUM_OF_BOE_VARIABLES = "SumofBOEWorkspaceVariables";
		public static readonly string VIEW_WORKSPACE_DISCRETE_VARIABLES = "DiscreteWorkspaceVariables";
		public static readonly string VIEW_WORKSPACE_OUTPUT_FORMAT = "WorkspaceOutputFormat";
		public static readonly string VIEW_WORKSPACE_RESOURCE_RATES = "WorkspaceResourceRates";
		public static readonly string VIEW_WORKSPACE_RESOURCE_RATES_GRID = "WorkspaceResourceRatesGrid";
		public static readonly string VIEW_WORKSPACE_RESOURCE_RATES_TM = "WorkspaceResourceRatesTM";
		public static readonly string VIEW_WORKSPACE_RESOURCE_RATES_GRID_TM = "WorkspaceResourceRatesGridTM";
		public static readonly string VIEW_WORKSPACE_SETTINGS_JUMP = "WorkspaceSettingsJump";
		public static readonly string VIEW_WORKSPACE_STATUS = "WorkspaceStatus";
		public static readonly string VIEW_WORKSPACE_STATUS_HISTORY_GRID = "WorkspaceStatusHistoryGrid";
		public static readonly string VIEW_WORKSPACE_ALLOW_SEARCH_FORMAT = "WorkspaceAllowSearch";
		public static readonly string VIEW_WORKSPACE_UPDATE_RESOURCE_RATES = "UpdateWorkspaceResourceRateDialog";
		public static readonly string VIEW_DUPLICATE_TASK_DIALOG = "DuplicateTaskDialog";
		public static readonly string VIEW_WORKSPACE_EMAIL_PREFERENCES = "WorkspaceEmailPreferences";

		#endregion Workspace

		#region Active Directory Search

		public static readonly string VIEW_ACTIVE_DIRECTORY_SEARCH = "ActiveDirectorySearch";

		#endregion

		#endregion VIEWS

		#region Messages

		public static readonly string WORKSPACE_RESOURCE_RATE_TM_SPACE_HEADING_TEXT = "Manage T&M Subcontractor Labor Resource Rates";
		public static readonly string WORKSPACE_RESOURCE_RATE_TM_SPACE_JUMP_DESCRIPTION_TEXT = "Manage the rates assigned to T&M Subcontractor and IWTA resources.";
		public static readonly string WORKSPACE_RESOURCE_RATE_TM_SPACE_CONTROL_DESCRIPTION_TEXT = "Manage the rates assigned to T&M Subcontractor and IWTA resources.";

		#endregion Messages

		#region LMPI Labels

		public static readonly string LMPI_OCI_LABEL_TEXT = "Organizational Conflict of Interest - Lockheed Martin Proprietary Information";
		public static readonly string LMPI_LABEL_TEXT = "Lockheed Martin Proprietary Information";

		#endregion LMPI Labels
	}
}


