// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    /// <summary>
    /// Constants for <see cref="BOEExporter"/>
    /// </summary>
    public static class BOEExporterConstants
    {
        #region Constants

        #region Tag Constants

        internal const string Container_BOE = "BOEContainer";
        internal const string Container_BOE_SUMMARY = "BOESummaryContainer";
        internal const string Container_BOE_SUMMARY_TASK = "BOESummaryTaskContainer";
        internal const string Container_BOEHeader = "BOEHeader";
        internal const string Container_BOENoBreak = "BOEContainerNoBreak";
        internal const string Container_BOESignatures = "BOESignatures";
        internal const string Container_BOESpreadTables = "BOESpreadTablesContainer";
        internal const string Container_HoursSummaryRollup = "HoursSummaryRollupContainer";
        internal const string Container_LaborHoursRollup = "LaborHoursRollupContainer";
        internal const string Container_LaborHoursSummaryByDate = "LaborHoursSummaryByDateContainer";
        internal const string Container_MaterialTaskElement = "MaterialTaskElementContainer";
        internal const string Container_MultiWbsClin = "MultiWbsClinContainer";
        internal const string Container_NonLaborHoursSummaryByDate = "NonLaborHoursSummaryByDateContainer";
        internal const string Container_SourcesOfData = "SourcesOfDataSection";
        internal const string Container_ODC = "ODCContainer";
        internal const string Container_ODCs = "ODCsContainer";
        internal const string Container_RevCode = "RevCodeContainer";
        internal const string Container_SummaryReference = "SummaryReferenceContainer";
        internal const string Container_TaskElement = "TaskElementContainer";
        internal const string Container_TaskSpreadTables = "TaskSpreadTablesContainer";
        internal const string Container_TaskID = "TaskIDContainer";
        internal const string Container_Travel = "TravelContainer";
        internal const string Container_Travels = "TravelsContainer";

        internal const string CustomFields_BOEContainer = "CustomFields-BOE";
        internal const string CustomFields_LaborContainer = "CustomFields-Labor";
        internal const string CustomFields_ResourceContainer = "CustomFields-Resource";
        
        internal const string FieldName_ApprovedBy = "ApprovedBy";
        internal const string FieldName_ApprovedByDate = "ApprovedByDate";
        internal const string FieldName_Apr = "Apr";
        internal const string FieldName_Aug = "Aug";
        internal const string FieldName_Author = "Author";
        internal const string FieldName_AuthorOneLine = "AuthorOneLine";
        internal const string FieldName_BOEDatePrepared = "BOEDatePrepared";
        internal const string FieldName_BOEDescription = "BOEDescription";
        internal const string FieldName_BOEEndDate = "BOEEndDate";
        internal const string FieldName_BOESegregation = "BOESegregation";
        internal const string FieldName_BOESourcesOfData = "BOESourcesOfData";
        internal const string FieldName_BOEStartDate = "BOEStartDate";
        internal const string FieldName_BOETitle = "BOETitle";
        internal const string FieldName_BOETitleInTask = "BOETitleInTask";
        internal const string FieldName_Cars = "Cars";
        internal const string FieldName_CLINString = "ClinString";
        internal const string FieldName_CLINEndDate = "CLINEndDate";
        internal const string FieldName_CLINNumber = "CLINNumber";
        internal const string FieldName_CLINStartDate = "CLINStartDate";
        internal const string FieldName_CLINTitle = "CLINTitle";
        internal const string FieldName_Cost = "Cost";
        internal const string FieldName_CostSummaryRollup = "CostSummaryRollup";
        internal const string FieldName_CostTotal = "CostTotal";
        internal const string FieldName_CustomFieldDescription = "CustomFieldDescription";
        internal const string FieldName_CustomFieldID = "CustomFieldID";
        internal const string FieldName_CustomFieldLabel = "CustomFieldLabel";
        internal const string FieldName_Date = "Date";
        internal const string FieldName_Days = "Days";
        internal const string FieldName_DaysTotal = "DaysTotal";
        internal const string FieldName_Dec = "Dec";
        internal const string FieldName_Departure = "Departure";
        internal const string FieldName_Destination = "Destination";
        internal const string FieldName_ElementOfCost = "ElementOfCost";
        internal const string FieldName_EndDate = "EndDate";
        internal const string FieldName_Feb = "Feb";
        internal const string FieldName_GroupID = "GroupID";
        internal const string FieldName_HeaderFooter = "BOE:HeaderFooter";
        internal const string FieldName_Hours = "Hours";
        internal const string FieldName_HoursSummaryRollup = "HoursSummaryRollup";
        internal const string FieldName_HoursTotal = "HoursTotal";
        internal const string FieldName_Jan = "Jan";
        internal const string FieldName_Jul = "Jul";
        internal const string FieldName_Jun = "Jun";
        internal const string FieldName_LaborCostTotal = "LaborCostTotal";
        internal const string FieldName_LaborDetailTaskTitle = "LaborDetailTaskTitle";
        internal const string FieldName_LaborHoursRollup = "LaborHoursRollup";
        internal const string FieldName_LaborTypeCost = "LaborTypeCost";
        internal const string FieldName_LaborTypeHours = "LaborTypeHours";
        internal const string FieldName_LaborTypeID = "LaborTypeID";
        internal const string FieldName_LaborTypes = "LaborTypes";
        internal const string FieldName_LaborTypesTotal = "LaborTypesTotal";
        internal const string FieldName_Mar = "Mar";
        internal const string FieldName_MaterialDetailTaskTitle = "MaterialDetailTaskTitle";
        internal const string FieldName_MaterialDirectCostRollup = "MaterialDirectCostRollup";
        internal const string FieldName_MatSubIWTACost = "MatSubIWTACost";
        internal const string FieldName_MatSubIWTACostTotal = "MatSubIWTACostTotal";
        internal const string FieldName_May = "May";
        internal const string FieldName_MethodOfQuoting = "MethodOfQuoting";
        internal const string FieldName_MethodOfQuoting_NoSpacing = "MethodOfQuoting-NoSpacing";
        internal const string FieldName_Mode = "Mode";
        internal const string FieldName_Months = "Months";
        internal const string FieldName_MonthsTotal = "MonthsTotal";
        internal const string FieldName_MOQEquationContainer = "MOQEquationContainer";
        internal const string FieldName_MOQTypeContainer = "MOQTypeContainer";
        internal const string FieldName_MOQRationaleContainer = "MOQRationaleContainer";
        internal const string FieldName_MOQSectionLabel = "MOQSectionLabel";
        internal const string FieldName_MOQEquation = "MOQEquation";
        internal const string FieldName_MOQEquationResult = "MOQEquationResult";
        internal const string FieldName_MOQType = "MOQType";
        internal const string FieldName_MultiLabel = "MultiClinWbsLabel";
        internal const string FieldName_NonzoneSummaryLabel = "NonzoneResourceTypesTableLabel";
        internal const string FieldName_Nov = "Nov";
        internal const string FieldName_Oct = "Oct";
        internal const string FieldName_OtherCost = "OtherCost";
        internal const string FieldName_OtherCostTotal = "OtherCostTotal";
        internal const string FieldName_ODCDirectCostRollup = "ODCDirectCostRollup";
        internal const string FieldName_People = "People";
        internal const string FieldName_PeopleTotal = "PeopleTotal";
        internal const string FieldName_PerformingOrg = "PerformingOrg";
        internal const string FieldName_PerformingOrgDescription = "PerformingOrgDescription";
        internal const string FieldName_PerformingOrgID = "PerformingOrgID";
        internal const string FieldName_POP = "POP";
        internal const string FieldName_PreparedBy = "PreparedBy";
        internal const string FieldName_PreparedByDate = "PreparedByDate";
        internal const string FieldName_ProgramName = "ProgramName";
        internal const string FieldName_ProgramNameHeader = "ProgramNameHeader";
        internal const string FieldName_Purpose = "Purpose";
        internal const string FieldName_Q1 = "Q1";
        internal const string FieldName_Q2 = "Q2";
        internal const string FieldName_Q3 = "Q3";
        internal const string FieldName_Q4 = "Q4";
        internal const string FieldName_Resource = "Resource";
        internal const string FieldName_ResourceCLIN = "ResourceCLIN";
        internal const string FieldName_ResourceSectionLabel = "ResourceSectionLabel";
        internal const string FieldName_ResourceCostHours = "ResourceCostHours";
        internal const string FieldName_ResourceCostHoursLabel = "ResourceCostHoursLabel";
        internal const string FieldName_ResourceStartDate = "ResourceStartDate";
        internal const string FieldName_ResourceEndDate = "ResourceEndDate";
        internal const string FieldName_ResourceDescription = "ResourceDescription";
        internal const string FieldName_ResourceElementOfCost = "ResourceElementOfCost";
        internal const string FieldName_ResourceID = "ResourceID";
        internal const string FieldName_ResourceName = "ResourceName";
        internal const string FieldName_ResourceRateType = "ResourceRateType";
        internal const string FieldName_ResourceType = "ResourceType";
        internal const string FieldName_ResourceWBS = "ResourceWBS";
        internal const string FieldName_RFPNumber = "RFPNumber";
        internal const string FieldName_SecondaryResourceName = "SecondaryResourceName";
        internal const string FieldName_SecondaryResourceDescription = "SecondaryResourceDescription";
        internal const string FieldName_Segment = "Segment";
        internal const string FieldName_SegmentRegion = "SegmentRegion";
        internal const string FieldName_Sep = "Sep";
        internal const string FieldName_SOURCEOFDATA = "SOURCEOFDATA";
        internal const string FieldName_SourcesOfData = "SourcesOfData";
        internal const string FieldName_SpreadCurve = "SpreadCurve";
        internal const string FieldName_StartDate = "StartDate";
        internal const string FieldName_Summary = "Summary";
        internal const string FieldName_SummaryByDate = "SummaryByDate";
        internal const string FieldName_SummaryByRevCode = "SummaryByRevCode";
        internal const string FieldName_SummaryLaborTypeCost = "SummaryLaborTypeCost";
        internal const string FieldName_SummaryLaborTypeHours = "SummaryLaborTypeHours";
        internal const string FieldName_SummaryLaborTypes = "SummaryLaborTypes";
        internal const string FieldName_SummaryReference = "SummaryReference";
        internal const string FieldName_SummaryYearGrandTotal = "SummaryYearGrandTotal";
        internal const string FieldName_SummaryYearTotal = "SummaryYearTotal";

        internal const string FieldName_BOESummaryLevel = "BOESummaryLevel";       
        internal const string FieldName_BOESummaryName = "BOESummaryName";
        internal const string FieldName_BOESummaryTaskTitle = "BOESummaryTaskTitle";
        internal const string FieldName_BOESummaryResource = "BOESummaryResourceName";

        internal const string FieldName_TableTitle = "TableTitle";
        internal const string FieldName_TaskCostTotal = "TaskCostTotal";
        internal const string FieldName_TaskDescription = "TaskDescription";
        internal const string FieldName_TaskDescription_NoSpacing = "TaskDescription-NoSpacing";
        internal const string FieldName_TaskDescriptionLabel = "TaskDescriptionLabel";
        internal const string FieldName_TaskDetailRevCode = "TaskDetailRevCode";
        internal const string FieldName_TaskDetailRevCodeDesc = "TaskDetailRevCodeDesc";
        internal const string FieldName_TaskElementDescription = "TaskDescription";
        internal const string FieldName_TaskElementRevCode = "SummaryRevCode";
        internal const string FieldName_TaskElementTitle = "TaskTitle";
        internal const string FieldName_TaskElementID = "TaskID";
        internal const string FieldName_TaskEndDate = "TaskEndDate";
        internal const string FieldName_TaskHoursTotal = "TaskHoursTotal";
        internal const string FieldName_TaskSegregation = "TaskSegregation";
        internal const string FieldName_TaskStartDate = "TaskStartDate";
        internal const string FieldName_TaskTypeDate = "TaskTypeDate";
        internal const string FieldName_TaskTypeDays = "TaskTypeDays";
        internal const string FieldName_TaskTypeDeparture = "TaskTypeDeparture";
        internal const string FieldName_TaskTypeDestination = "TaskTypeDestination";
        internal const string FieldName_TaskTypePeople = "TaskTypePeople";
        internal const string FieldName_TaskTypePurpose = "TaskTypePurpose";
        internal const string FieldName_TaskTypeRow_ODC = "TaskTypeRow-ODC";
        internal const string FieldName_TaskTypeRow_Travel = "TaskTypeRow-Travel";
        internal const string FieldName_TaskTypeSite = "Site";
        internal const string FieldName_TaskTypeSkillLevel = "TaskTypeSkillLevel";
        internal const string FieldName_TaskTypeSkillMix = "SkillMix";
        internal const string FieldName_TaskTypeTitle = "TaskTypeTitle";
        internal const string FieldName_TaskTypeTrips = "TaskTypeTrips";
        internal const string FieldName_Total = "Total";
        internal const string FieldName_SubTotal = "SubTotal";
        internal const string FieldName_TravelDirectCostRollup = "TravelDirectCostRollup";
        internal const string FieldName_TravelTripID = "TravelTripID";
        internal const string FieldName_TripDate = "TripDate";
        internal const string FieldName_Trips = "Trips";
        internal const string FieldName_UID = "UID";
        internal const string FieldName_WBSString = "WbsString";
        internal const string FieldName_WBSEndDate = "WBSEndDate";
        internal const string FieldName_WBSNumber = "WBSNumber";
        internal const string FieldName_WBSNumberInTask = "WBSNumberInTask";
        internal const string FieldName_WBSStartDate = "WBSStartDate";
        internal const string FieldName_WBSTitle = "WBSTitle";
        internal const string FieldName_Year = "Year";
        internal const string FieldName_Zone = "Zone";
        internal const string FieldName_ZoneSummaryLabel = "ZoneResourceTypesTableLabel";
        internal const string FieldName_RoundingNotice = "RoundingNotice";
        internal const string FieldName_RoundingNoticeAsterisk = "RoundingNoticeAsterisk";
        internal const string FieldNameSuffix_ByDate = "ByDate";
        internal const string FieldNameSuffix_ByRevCode = "ByRevCode";
        internal const string FieldNameSuffix_Description = "Description";
        internal const string FieldNameSuffix_DetailClin = "DetailClin";
        internal const string FieldNameSuffix_DetailEquation = "DetailEquation";
        internal const string FieldNameSuffix_DetailEquationsText = "DetailEquationsText";
        internal const string FieldNameSuffix_DetailEquationTotal = "DetailEquationTotal";
        internal const string FieldNameSuffix_DetailPOP = "DetailPOP";
        internal const string FieldNameSuffix_DetailRationale = "DetailRationale";
        internal const string FieldNameSuffix_DetailTaskTitle = "DetailTaskTitle";
        internal const string FieldNameSuffix_Hours = "Hours";
        internal const string FieldNameSuffix_HoursTotal = "HoursTotal";
        internal const string FieldNameSuffix_PerfOper = "PerfOper";
        internal const string FieldNameSuffix_RegionLabor = "RegionLabor";
        internal const string FieldNameSuffix_RevCode = "RevCode";
        internal const string FieldNameSuffix_Year = "Year";
        internal const string FieldNameSuffix_YearGrandTotal = "YearGrandTotal";
        internal const string FieldNameSuffix_YearTotal = "YearTotal";
        
        internal const string Marker_DataRow = "Marker-DataRow";
        internal const string Marker_SummaryDataRow = "Marker-SummaryDataRow";
        internal const string Marker_SummaryTotalsRow = "Marker-SummaryTotalsRow";
        internal const string Marker_TotalsRow = "Marker-TotalsRow";
        internal const string Marker_SubTotalsRow = "Marker-SubtotalsRow";
        internal const string Marker_DeleteRow = "Marker-DeleteRow";
        internal const string Marker_BOESummaryResourceRowMarker = "BOESummaryResourceRowMarker";
        internal const string Marker_BOESummaryBOERowMarker = "BOESummaryBOERowMarker";
        internal const string Marker_BOESummaryTaskRowMarker = "BOESummaryTaskRowMarker";


        internal const string SectionTitlePrefix = "SectionTitle-";

        internal const string Table_BOECostSummary = "BOECostSummaryTable";
        internal const string Table_BOEHoursSummary = "BOEHoursSummaryTable";
        internal const string Table_CostSpreadRollup = "CostSpreadRollupTable";
        internal const string Table_GfyCostSpreadRollup = "GfyCostSpreadRollupTable";
        internal const string Table_CostSpreadRollupByYear = "CostSpreadRollupByYearTable";
        internal const string Table_CostSpreadRollupByQuarter = "CostSpreadRollupByQuarterTable";
        internal const string Table_GfyCostSpreadRollupByQuarter = "GfyCostSpreadRollupByQuarterTable";
        internal const string Table_DirectCostRollup = "DirectCostRollupTable";
        internal const string Table_GfyDirectCostRollup = "GfyDirectCostRollupTable";
        internal const string Table_DirectCostRollupByYear = "DirectCostRollupByYearTable";
        internal const string Table_DirectCostRollupByQuarter = "DirectCostRollupByQuarterTable";
        internal const string Table_GfyDirectCostRollupByQuarter = "GfyDirectCostRollupByQuarterTable";
        internal const string Table_LaborAndNonLaborCostSummaryByDate = "LaborAndNonLaborCostSummaryByDateTable";
        internal const string Table_LaborCostSummaryByDate = "LaborCostSummaryByDateTable";
        internal const string Table_GfyLaborCostSummaryByDate = "GfyLaborCostSummaryByDateTable";
        internal const string Table_LaborCostSummaryByQuarter = "LaborCostSummaryByQuarterTable";
        internal const string Table_GfyLaborCostSummaryByQuarter = "GfyLaborCostSummaryByQuarterTable";
        internal const string Table_LaborHoursRollup = "LaborHoursRollupTable";
        internal const string Table_GfyLaborHoursRollup = "GfyLaborHoursRollupTable";
        internal const string Table_LaborHoursRollupByYear = "LaborHoursRollupByYearTable";
        internal const string Table_LaborHoursRollupByQuarter = "LaborHoursRollupByQuarterTable";
        internal const string Table_GfyLaborHoursRollupByQuarter = "GfyLaborHoursRollupByQuarterTable";
        internal const string Table_LaborHoursSummaryByDate = "LaborHoursSummaryByDateTable";
        internal const string Table_GfyLaborHoursSummaryByDate = "GfyLaborHoursSummaryByDateTable";
        internal const string Table_GfyLaborHoursSummaryByQuarter = "GfyLaborHoursSummaryByQuarterTable";
        internal const string Table_LaborHoursSummaryByQuarter = "LaborHoursSummaryByQuarterTable";
        internal const string Table_NonLaborCostSummaryByDate = "NonLaborCostSummaryByDateTable";
        internal const string Table_NonLaborHoursSummaryByDate = "NonLaborHoursSummaryByDateTable";
        internal const string Table_LaborHoursSummaryByCustomField = "LaborHoursSummaryByCustomFieldTable";
        internal const string Table_ResourceTypes = "ResourceTypesTable";
        internal const string Table_ResourceTypes_ZoneTravel = "ResourceTypesTable-Zone";
        internal const string Table_ResourceTypes_NonzoneTravel = "ResourceTypesTable-Nonzone";
        internal const string Table_ResourceSummaryByElementOfCost = "ResourceSummaryByElementOfCostTable";
        internal const string Table_ResourceSummaryByResourceType = "ResourceSummaryByResourceTypeTable";
        internal const string Table_ResourceSummaryByResourceID = "ResourceSummaryByResourceIDTable";
        internal const string Table_ResourceHoursRollup = "ResourceHoursRollupTable";
        internal const string Table_ResourceCostRollup = "ResourceCostRollupTable";
        internal const string Table_ResourceRollup = "ResourceRollupTable";
        internal const string Table_TaskSummary = "TaskSummaryTable";

        internal const string TaskContainerPrefix = "TaskContainer-";
        internal const string TaskRowPrefix = "TaskRow-";
        internal const string TaskTypeRowPrefix = "TaskTypeRow-";

        internal const string ResourceContainerPrefix = "ResourceContainer-";

        internal const string TextElement_Title = "Title";

        internal const string TaskType_Labor = "Labor";
        internal const string TaskType_ODC = "ODC";

        internal const string UNASSIGNED_VALUE = "Unassigned";

        internal const string UNASSIGNED_WBS_DISPLAY_TEXT = "NO WBS";
        internal const string UNASSIGNED_CLIN_DISPLAY_TEXT = "NO CLIN";

        internal const string CustomFieldName_BOESegregation = "BOE Segregation";
        internal const string CustomFieldName_RevCode = "Rev Code";
        internal const string CustomFieldName_TaskSegregation = "Task Segregation";
        internal const string CustomFieldName_SkillLevel = "Skill Level";
        internal const string CustomFieldName_Site = "Site";
        internal const string CustomFieldName_SkillMix = "Skill Mix";
        internal const string CustomFieldName_PWS = "PWS";
        internal const string CustomFieldName_STOT = "ST/OT";
        internal const string CustomFieldName_Premium = "Premium";
        internal const string CustomFieldName_RateBOE = "Rate BOE";
        internal const string CustomFieldName_Company = "Company";
        internal const string CustomFieldName_GovtLaborCategory = "Function";
        internal const string CustomFieldName_KeyPersonnel = "Field-B";
        internal const string CustomFieldName_EstimateMethod = "Estimate Method";
        internal const string CustomFieldName_SOW = "SOW";

        #endregion

        #region MIME Constants

        public const string ContentType_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string ContentType_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ContentType_XLSM = "application/vnd.ms-excel.sheet.macroEnabled.12";
        public const string ContentType_CSV = "text/csv";
        public const string ContentType_ZIP = "application/zip";

        internal const string CONTENT_HEADER_NAME = "Content-Disposition";
        internal const string CONTENT_HEADER_FORMAT_STRING = "attachment;filename={0}";

        #endregion

        #region Formatting Constants

        internal const string NUMERIC_FORMAT_COMMAS_NO_DECIMALS = "N0";
        internal const string CURRENCY_FORMAT_DEFAULT = "C2";
        internal const string CURRENCY_FORMAT_NO_DECIMALS = "C0";

        internal const string DECIMAL_FORMAT_NINE_DECIMAL_PLACES = "0.#########";

        internal const string DATE_FORMAT_STANDARD = "MM/dd/yyyy";
        internal const string DATE_FORMAT_MONTH_YEAR = "MM/yyyy";

        internal const string EXPORT_FONT_DEFAULT = "Times New Roman";

        internal const int CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR = 1; //-$n
        internal const int CURRENCY_NEGATIVE_PATTERN_DOLLAR_MINUS = 2; //$-n

        #endregion

        #region Other Constants

        internal const string BLANK_SPACE = " ";
        internal const string PORTION_MARKING_U = "(U) ";

        internal const string CONFIG_SETTING_LOG_EXPORT = "ExportLoggingEnableInternalExport";
        internal const string CONFIG_SETTING_LOG_VIEW_MODELS = "ExportLoggingEnableInternalMV";

        internal const string LMPI_OCI_LABEL_TEXT = "Organizational Conflict of Interest - Lockheed Martin Proprietary Information";
        internal const string LMPI_LABEL_TEXT = "Lockheed Martin Proprietary Information";

        internal const string GFY_QUARTERLY_TABLE_SUFFIX = " - GFY Quarter";

        /// <summary>
        /// BOEJ-2791 - Name of special template used on the "High Side" to summarize labor hours by custom field.
        /// </summary>
        public const string ALL_BOE_SUMMARY_BY_CUSTOM_FIELD_TEMPLATE_NAME = "(U) SSC Portrait with Custom Field Summaries and 1 Inch Margins";

        /// <summary>
        /// Summarize by option value when no summary tables are desired.
        /// </summary>
        public const string SUMMARIZE_BY_NONE = "None";

        #endregion

        #endregion
    }
}
